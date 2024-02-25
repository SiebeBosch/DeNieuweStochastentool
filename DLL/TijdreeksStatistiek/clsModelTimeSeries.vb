Imports STOCHLIB.General
Imports GemBox.Spreadsheet
Imports System.Text
Imports System.IO

Public Class clsModelTimeSeries
    Public Name As String

    'for this rainfall series we can derive statistics per season (e.g. meteorological summer, hydrological halfyear etc.)
    Public Seasons As clsSeasons
    Public KNMI14Scenarios As clsKNMI14Scenarios
    Public DailySums As clsModelTimeSeries

    Public Dates As New List(Of DateTime)
    Public Values As New Dictionary(Of GeneralFunctions.enmModelParameter, List(Of Single)) 'index 0-based

    Public InUse As List(Of Integer)   'stores the event number in the time series

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        Seasons = New clsSeasons(Me.Setup, Me)
        KNMI14Scenarios = New clsKNMI14Scenarios(Me.Setup, Me)
    End Sub


    Public Sub New(ByRef mySetup As clsSetup, SeriesName As String)
        Name = SeriesName
        Setup = mySetup
        Seasons = New clsSeasons(Me.Setup, Me)
        KNMI14Scenarios = New clsKNMI14Scenarios(Me.Setup, Me)
    End Sub

    Public Sub calcDailyPrecipitationSums()
        'computes the daily precipitation sums and adds them to the DailySums object
        Dim i As Long, nextDate As Date

        Dim curDate As New Date(Year(Dates(0)), Month(Dates(0)), Day(Dates(0)))
        Dim curVol As Double = Values.Item(GeneralFunctions.enmModelParameter.precipitation)(0)

        DailySums = New clsModelTimeSeries(Me.Setup)
        For i = 1 To Dates.Count - 1
            nextDate = New Date(Year(Dates(i)), Month(Dates(i)), Day(Dates(i)))
            If nextDate > curDate Then
                'current date is now complete, so add it to the series
                DailySums.Dates.Add(curDate)
                DailySums.Values.Item(GeneralFunctions.enmModelParameter.precipitation).Add(curVol) 'add the current volume to the event
                curVol = Values.Item(GeneralFunctions.enmModelParameter.precipitation)(i)           'initialize the volume for the next day since we're already inside the next day
                curDate = nextDate
            Else
                curVol += Values.Item(GeneralFunctions.enmModelParameter.precipitation)(i)          'add the precipitation
            End If
        Next

        'finally complete the last date and add it to the series
        DailySums.Dates.Add(curDate)
        DailySums.Values.Item(GeneralFunctions.enmModelParameter.precipitation).Add(curVol)

    End Sub

    Public Function getSeasonSum(ModelParameter As GeneralFunctions.enmModelParameter, ByVal mySeason As STOCHLIB.GeneralFunctions.enmSeason) As Double
        'computes the precipitation sum for a given season over the entire time series
        Dim i As Long, sum As Double = 0
        For i = 0 To Values.Item(ModelParameter).Count - 1
            If Me.Setup.GeneralFunctions.MeteorologischSeizoen(Dates(i)) = mySeason Then
                sum += Values.Item(ModelParameter)(i)
            End If
        Next
        Return sum
    End Function

    Public Function countWetDaysBySeason(ByVal mySeason As STOCHLIB.GeneralFunctions.enmSeason, ByVal VolumeThreshold As Double) As Long
        'counts the total number of wet days (> 0.1mm) for a given season in the entire time series
        Dim i As Long, n As Long = 0
        For i = 0 To DailySums.Values.Count - 1
            If Me.Setup.GeneralFunctions.MeteorologischSeizoen(DailySums.Dates(i)) = mySeason Then
                If DailySums.Values(GeneralFunctions.enmModelParameter.precipitation)(i) >= VolumeThreshold Then n += 1
            End If
        Next
        Return n
    End Function

    Public Sub ClearStats()
        Seasons = New clsSeasons(Me.Setup, Me)
    End Sub

    Public Function getWindowSum(ModelParameter As GeneralFunctions.enmModelParameter, ByVal startIdx As Long, ByVal nSteps As Long) As Double
        Dim mySum As Double, i As Long
        For i = startIdx To startIdx + nSteps - 1
            mySum += Values.Item(ModelParameter)(i)
        Next
        Return mySum
    End Function

    Public Function GetTimeStepSizeMinutes() As Double
        Dim mySpan As TimeSpan = Dates(1).Subtract(Dates(0))
        Return mySpan.TotalMinutes
    End Function

    Public Function getTimeSpanYears() As Double
        Dim mySpan As TimeSpan = Dates(Dates.Count - 1).Subtract(Dates(0))
        Return mySpan.Days / 365.25
    End Function

    Public Function ReadFromCSV(ByVal Path As String, ByVal Delimiter As String, ByVal DateFormatting As String, ByVal DateHeader As String, ByVal ValuesHeader As String) As Boolean
        Try
            Dim myStr As String, tmpStr As String, DateColIdx As Integer = 0, HeaderColIdx As Integer = 0, curIdx As Integer, i As Integer
            Dim nNonNumeric As Long, r As Long
            Dim myDate As DateTime, lastDate As DateTime
            Dim LastColIdx As Integer = 0

            Using csvReader As New StreamReader(Path)

                'find the column index for the data
                myStr = csvReader.ReadLine
                curIdx = 0
                While Not myStr = ""
                    curIdx += 1
                    tmpStr = Me.Setup.GeneralFunctions.ParseString(myStr, Delimiter)
                    If tmpStr.Trim.ToUpper = ValuesHeader.Trim.ToUpper Then
                        HeaderColIdx = curIdx
                    ElseIf tmpStr.Trim.ToUpper = DateHeader.Trim.ToUpper Then
                        DateColIdx = curIdx
                    End If
                    If Math.Min(HeaderColIdx, DateColIdx) > 0 Then
                        LastColIdx = Math.Max(HeaderColIdx, DateColIdx)
                        Exit While
                    End If
                End While

                If HeaderColIdx = 0 Then
                    Throw New Exception("Error: could not find data column " & ValuesHeader & " in csv file: " & Path)
                ElseIf DateColIdx = 0 Then
                    Throw New Exception("Error: could not find date column " & DateHeader & " in csv file: " & Path)
                Else
                    While Not csvReader.EndOfStream
                        r += 1

                        'update the progress bar only once every 1000 rows
                        If r / 1000 = Math.Round(r / 1000) Then
                            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading csv file...", csvReader.BaseStream.Position, csvReader.BaseStream.Length, True)
                        End If

                        myStr = csvReader.ReadLine
                        For i = 1 To LastColIdx
                            tmpStr = Me.Setup.GeneralFunctions.ParseString(myStr, Delimiter)
                            If i = DateColIdx Then
                                'prevent importing double dates!!!! implemented 20140920 for RACMO timeseries
                                Call Me.Setup.GeneralFunctions.ParseDateString(tmpStr, DateFormatting, myDate)
                                If myDate <> lastDate Then
                                    Dates.Add(myDate)
                                Else
                                    Me.Setup.Log.AddError("CSV file contains multiple records of date " & myDate)
                                    Exit For 'skip to the next record
                                End If
                                lastDate = myDate
                            ElseIf i = HeaderColIdx Then
                                If IsNumeric(tmpStr) Then
                                    Values.Item(GeneralFunctions.enmModelParameter.precipitation).Add(Convert.ToSingle(tmpStr))
                                Else
                                    nNonNumeric += 1
                                End If
                            End If
                        Next
                    End While
                End If
            End Using

            If nNonNumeric > 0 Then Me.Setup.Log.AddWarning("csv file " & Path & " contained " & nNonNumeric & " non-numeric values for time series " & ValuesHeader & ".")

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function

    Public Function WriteCSV(ByVal ScenarioName As String) As Boolean
        'This function writes the original timeseries to a csv-file, but enhanced with event index number and event precipitation sum
        Dim i As Long
        Try
            Using csvWriter As New StreamWriter(Me.Setup.Settings.ExportDirRoot & "\" & Name & "_" & ScenarioName & ".csv")
                csvWriter.WriteLine("Date," & Name)
                For i = 0 To Dates.Count - 1
                    csvWriter.WriteLine(Format(Dates(i), "yyyy/MM/dd HH:mm") & "," & Values.Item(GeneralFunctions.enmModelParameter.precipitation)(i))
                Next
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function



End Class
