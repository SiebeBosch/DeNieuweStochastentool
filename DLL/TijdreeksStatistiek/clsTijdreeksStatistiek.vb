Imports STOCHLIB.General
Imports GemBox.Spreadsheet
Imports System.Data.Sql
Imports System.IO

Public Class clsTijdreeksStatistiek

    Public NeerslagReeksen As New Dictionary(Of String, clsRainfallSeries)
    Public GetijdenReeksen As New Dictionary(Of String, clsTidalTimeSeries)
    Public Database As String

    Public POTFrequency As Integer              'number of target exceedances per year for a POT-analysis
    Public MinTimeStepsBetweenEvents As Integer 'minimum number of timesteps between each two identified rainfall events

    Dim con As OleDb.OleDbConnection
    Dim da As OleDb.OleDbDataAdapter
    Dim dt As DataTable

    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub Initialize(ByVal Season As String, ByVal Duration As Integer)
        'adds the required season and duration to the dictionares
        For Each mySeries As clsRainfallSeries In NeerslagReeksen.Values
            Dim mySeason As clsSeason = mySeries.Seasons.Add(Me.Setup, mySeries, Season)
            Dim myDuration As clsDuration = mySeason.AddDuration(Duration)
        Next
    End Sub

    Public Function addRainfallSeriesFromHBVReport(ByRef wb As clsExcelBook) As Boolean
        'this function reads a rainfall timeseries + groundwater statistics (uz, lz and sm) or upper zone lowerzone and soilmosture from the HBV Reports Excel format to the NeerslagReeksen dictionary

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading precipitation series from HBV report...", 0, 10, True)

            Dim sheetnum As Integer = 0
            For Each ws As clsExcelSheet In wb.Sheets
                sheetnum += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", sheetnum, wb.Sheets.Count, True)
                Dim mySeries As New clsRainfallSeries(Me.Setup, ws.SheetName)
                Dim HeaderRowIdx As Integer = 3
                Dim DatesColIdx As Integer = 0
                Dim PrecColIdx As Integer = -1  'precipitation column
                'Dim UzColIdx As Integer = -1    'upper zone column
                'Dim LzColIdx As Integer = -1    'lower zone column
                'Dim SmColIdx As Integer = -1    'soil moisture column

                For i = 0 To 100
                    If Left(ws.ws.Cells(HeaderRowIdx, i).Value.ToString, 4) = "prec" Then
                        PrecColIdx = i
                        Exit For
                        'ElseIf Left(ws.Cells(HeaderRowIdx, i).Value.ToString, 2) = "lz" Then
                        '    LzColIdx = i
                        'ElseIf Left(ws.Cells(HeaderRowIdx, i).Value.ToString, 2) = "uz" Then
                        '    UzColIdx = i
                        'ElseIf Left(ws.Cells(HeaderRowIdx, i).Value.ToString, 2) = "sm" Then
                        '    SmColIdx = i
                    End If
                Next

                If PrecColIdx = -1 Then Throw New Exception("Error: no precipitation column found in Excel worksheet.")

                Dim rowIdx As Integer = HeaderRowIdx
                While Not ws.ws.Cells(rowIdx + 1, DatesColIdx).Value = ""
                    rowIdx += 1
                    mySeries.Dates.Add(ws.ws.Cells(rowIdx, DatesColIdx).Value)
                    mySeries.Values.Add(ws.ws.Cells(rowIdx, PrecColIdx).Value)
                End While

                Me.Setup.TijdreeksStatistiek.NeerslagReeksen.Add(mySeries.Name, mySeries)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Import complete.", 0, 10, True)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function addRainfallSeriesFromHisFile(ByVal HisFilePath As String, ByVal LocID As String, ByVal PartOfParameterName As String, Optional ByVal Multiplier As Double = 1) As Boolean
        Dim Dates() As Date = Nothing
        Dim Values() As Double = Nothing
        Dim i As Long, n As Long

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading " & PartOfParameterName & " from hisfile...", 0, 10)
            Dim myReader As New clsHisFileBinaryReader(HisFilePath, Me.Setup)
            If Not myReader.ReadAddLocationResultsToArray(LocID, PartOfParameterName, Dates, Values, Multiplier) Then Throw New Exception("Error reading " & PartOfParameterName & " results for " & LocID & "from hisfile " & HisFilePath)
            Dim newSeries As New clsRainfallSeries(Me.Setup)
            newSeries.Name = PartOfParameterName
            n = Dates.Count - 1
            For i = 0 To Dates.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                newSeries.Dates.Add(Dates(i))
                newSeries.Values.Add(Values(i))
            Next
            NeerslagReeksen.Add(newSeries.Name, newSeries)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Sub ClearStats()
        For Each mySeries As clsRainfallSeries In NeerslagReeksen.Values
            mySeries.ClearStats()
        Next
    End Sub

    Public Sub calcDailySums()
        'computes the daily precipitation sums and writes them to a timeseries
        For Each mySeries As clsRainfallSeries In NeerslagReeksen.Values
            mySeries.calcDailySums()
        Next
    End Sub

    Public Sub ApplyKNMI14Scenarios(ByVal myScenario As String)
        For Each mySeries As clsRainfallSeries In NeerslagReeksen.Values
            mySeries.KNMI14Scenarios.Apply(myScenario)
        Next
    End Sub


    Public Function readPrecipitationFromCSV(ByVal Path As String, ByVal Delimiter As String, ByVal DateFormatting As String, ByVal DateHeader As String, ByVal ValuesHeader As String) As Boolean
        Try
            Dim mySeries As New clsRainfallSeries(Me.Setup)
            mySeries.Name = ValuesHeader
            If Not mySeries.ReadFromCSV(Path, Delimiter, DateFormatting, DateHeader, ValuesHeader) Then Throw New Exception("Error reading timeseries from csv file.")
            Me.Setup.TijdreeksStatistiek.NeerslagReeksen.Add(ValuesHeader, mySeries)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Import complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function readWaterLevelsFromCSV(ByVal Path As String, ByVal Delimiter As String, ByVal DateFormatting As String, TimeFormatting As String, SeriesName As String, ByVal DataValueColumnName As String, Multiplier As Double, DateColumnName As String, TimeColumnName As String) As Boolean
        Try
            Dim mySeries As New clsTidalTimeSeries(Me.Setup)
            mySeries.Name = DataValueColumnName
            If Not mySeries.ReadFromCSV(Path, Delimiter, DateFormatting, TimeFormatting, DataValueColumnName, Multiplier, DateColumnName, TimeColumnName) Then Throw New Exception("Error reading csv file: " & Path)
            Me.Setup.TijdreeksStatistiek.GetijdenReeksen.Add(SeriesName, mySeries)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Import complete.", 0, 10, True)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function writeExcel(ByRef chkResultsSeasons As Windows.Forms.CheckedListBox, ByVal POT As Boolean, ByVal YEARMAX As Boolean, ByVal Patterns As Boolean) As Boolean
        'this routine writes all POT-analyses and annual maxima that are in memory to Excel
        Try
            If Not PlottingPositionsToExcel() Then Throw New Exception("Error writing plotting positions to Excel.")
            If YEARMAX Then If Not YearMaximaToExcel() Then Throw New Exception("Error writing yearly maxima to Excel.")
            If POT Then If Not POTValuesToExcel() Then Throw New Exception("Error writing POT-values to Excel.")
            If Patterns Then
                If Not PatternsToExcel(chkResultsSeasons) Then Throw New Exception("Error writing pattern statistics to Excel.")
            End If
            If Not GeneralStatsToExcel() Then Throw New Exception("Error writing general statistics to Excel.")
            Me.Setup.ExcelFile.Path = Me.Setup.Settings.ExportDirRoot & "\statistics_" & Me.Setup.GeneralFunctions.RemoveSurroundingQuotes(NeerslagReeksen.Values(0).Name, True, True) & ".xlsx"
            Me.Setup.ExcelFile.Save(False)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function writeEnhancedSeries() As Boolean
        Dim i As Integer, n As Integer
        'this function exports the original timeseries, but adds the event index numbers and the event sum to it
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing enhanced timeseries.", 0, 10)
            For Each mySeries As clsRainfallSeries In NeerslagReeksen.Values
                For Each mySeason As clsSeason In mySeries.Seasons.Seasons.Values
                    n = mySeason.Durations.Count
                    i = 0
                    For Each myDuration As clsDuration In mySeason.Durations.Values
                        i += 1
                        Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                        If Not myDuration.WriteCSV() Then Throw New Exception("Error writing CSV file.")
                    Next
                Next
            Next
            Setup.GeneralFunctions.UpdateProgressBar("Export complete.", 0, 10)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function writeCSV(ByVal ScenarioName As String) As Boolean
        'this function exports the original timeseries, but adds the event index numbers and the event sum to it
        Try
            For Each mySeries As clsRainfallSeries In NeerslagReeksen.Values
                If Not mySeries.WriteCSV(ScenarioName) Then Throw New Exception("Error writing CSV file for series " & mySeries.Name)
            Next
            Setup.GeneralFunctions.UpdateProgressBar("Export complete.", 0, 10)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function PlottingPositionsToExcel() As Boolean
        Dim ws As clsExcelSheet
        Dim r As Long, c As Long
        ws = Me.Setup.ExcelFile.GetAddSheet("PlottingPositions")

        Try

            c = -2
            For Each myReeks As clsRainfallSeries In NeerslagReeksen.Values
                For Each mySeason As STOCHLIB.clsSeason In myReeks.Seasons.Seasons.Values
                    For Each myduration As clsDuration In mySeason.Durations.Values

                        If Not myduration.AnnualMaxima Is Nothing Then
                            c += 2
                            r = 0
                            ws.ws.Cells(r, c).Value = "Reeks:"
                            ws.ws.Cells(r, c + 1).Value = myReeks.Name
                            r += 1
                            ws.ws.Cells(r, c).Value = "Analyse:"
                            ws.ws.Cells(r, c + 1).Value = "jaarmaxima"
                            r += 1
                            ws.ws.Cells(r, c).Value = "Seizoen:"
                            ws.ws.Cells(r, c + 1).Value = mySeason.Season.ToString
                            r += 1
                            ws.ws.Cells(r, c).Value = "Duur (uren):"
                            ws.ws.Cells(r, c + 1).Value = myduration.DurationHours & "h"
                            r += 1
                            ws.ws.Cells(r, c).Value = "Herhalingstijd"
                            ws.ws.Cells(r, c + 1).Value = "Volume"

                            For Each Row As DataRow In myduration.AnnualMaxima.PlottingPositions.Rows
                                r += 1
                                ws.ws.Cells(r, c).Value = Row(0)
                                ws.ws.Cells(r, c + 1).Value = Row(1)
                            Next

                        End If

                        If Not myduration.POTEvents Is Nothing Then
                            c += 2
                            r = 0
                            ws.ws.Cells(r, c).Value = "Reeks:"
                            ws.ws.Cells(r, c + 1).Value = myReeks.Name
                            r += 1
                            ws.ws.Cells(r, c).Value = "Analyse:"
                            ws.ws.Cells(r, c + 1).Value = "POT"
                            r += 1
                            ws.ws.Cells(r, c).Value = "Seizoen:"
                            ws.ws.Cells(r, c + 1).Value = mySeason.Season.ToString
                            r += 1
                            ws.ws.Cells(r, c).Value = "Duur:"
                            ws.ws.Cells(r, c + 1).Value = myduration.DurationHours
                            r += 1
                            ws.ws.Cells(r, c).Value = "Herhalingstijd"
                            ws.ws.Cells(r, c + 1).Value = "Volume"

                            For Each Row As DataRow In myduration.POTEvents.PlottingPositions.Rows
                                r += 1
                                ws.ws.Cells(r, c).Value = Row(0)
                                ws.ws.Cells(r, c + 1).Value = Row(1)
                            Next

                        End If
                    Next
                Next

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in sub PlottingPositionsToExcel in class clsNeerslagstatistiek.")
            Return False
        End Try

    End Function

    Public Function POTValuesToExcel() As Boolean
        Dim ws As clsExcelSheet
        Dim r As Long, c As Long, i As Integer

        Try
            For Each myReeks As clsRainfallSeries In NeerslagReeksen.Values
                ws = Me.Setup.ExcelFile.GetAddSheet(myReeks.Name & "_POT")
                c = -7
                For Each mySeason As clsSeason In myReeks.Seasons.Seasons.Values
                    For Each myDuration As clsDuration In mySeason.Durations.Values

                        If Not myDuration.POTEvents Is Nothing Then
                            c += 7
                            r = 0
                            i = 0
                            ws.ws.Cells(r, c).Value = "Date/Time"
                            ws.ws.Cells(r, c + 1).Value = "Volume " & myDuration.DurationHours & "h"
                            ws.ws.Cells(r, c + 2).Value = "Event sum (mm)"
                            ws.ws.Cells(r, c + 3).Value = "Event number " & myDuration.DurationHours & "h"
                            ws.ws.Cells(r, c + 4).Value = "Meteo season"
                            ws.ws.Cells(r, c + 5).Value = "Meteo halfyear"
                            ws.ws.Cells(r, c + 6).Value = "Hydro halfyear"

                            For Each myEvent As clsTimeSeriesEvent In myDuration.POTEvents.Events.Values
                                i += 1

                                'protection against too many rows in Excel
                                If r > 1000000 Then
                                    r = 0
                                    c += 7
                                    ws.ws.Cells(r, c).Value = "Date/Time"
                                    ws.ws.Cells(r, c + 1).Value = "Volume " & myDuration.DurationHours & "h"
                                    ws.ws.Cells(r, c + 2).Value = "Event sum (mm)"
                                    ws.ws.Cells(r, c + 3).Value = "Event number " & myDuration.DurationHours & "h"
                                    ws.ws.Cells(r, c + 4).Value = "Meteo season"
                                    ws.ws.Cells(r, c + 5).Value = "Meteo halfyear"
                                    ws.ws.Cells(r, c + 6).Value = "Hydro halfyear"
                                    r += 1
                                End If

                                For Each myRecord As clsTimeTableRecord In myEvent.TimeTable.Records.Values
                                    r += 1
                                    ws.ws.Cells(r, c).Value = myRecord.Datum
                                    ws.ws.Cells(r, c + 1).Value = myRecord.GetValue(0)
                                    ws.ws.Cells(r, c + 2).Value = myEvent.Sum
                                    ws.ws.Cells(r, c + 3).Value = i
                                    ws.ws.Cells(r, c + 4).Value = myEvent.MeteoSeason.ToString
                                    ws.ws.Cells(r, c + 5).Value = myEvent.MeteoHalfYear.ToString
                                    ws.ws.Cells(r, c + 6).Value = myEvent.HydroHalfYear.ToString
                                Next
                            Next
                        End If
                    Next
                Next
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function PatternsToExcel(ByRef chkResultsSeasons As Windows.Forms.CheckedListBox) As Boolean

        Dim ws As clsExcelSheet
        Dim r As Long, i As Integer

        Try
            For Each myReeks As clsRainfallSeries In NeerslagReeksen.Values
                ws = Me.Setup.ExcelFile.GetAddSheet(myReeks.Name & "_PATTERNS")
                r = 0

                ws.ws.Cells(r, 0).Value = "Season Analyzed"
                ws.ws.Cells(r, 1).Value = "Duration"
                ws.ws.Cells(r, 2).Value = "Season"
                ws.ws.Cells(r, 3).Value = "Events in Series"
                ws.ws.Cells(r, 4).Value = "Events in Season"
                ws.ws.Cells(r, 5).Value = "Pattern"
                ws.ws.Cells(r, 6).Value = myReeks.Name

                'cycle through each season that we've done a POT-analysis and/or annual max analysis for
                For Each AnalyzedSeason As clsSeason In myReeks.Seasons.Seasons.Values
                    'cycle through each duration inside this season
                    For Each myDuration As clsDuration In AnalyzedSeason.Durations.Values
                        'cycle throught the analysis-seasons, so the seasons we want to produce outcome for
                        For Each mySeasonStr As String In chkResultsSeasons.CheckedItems
                            Dim ResultsSeason As STOCHLIB.GeneralFunctions.enmSeason
                            If Not Setup.GeneralFunctions.SeasonFromString(mySeasonStr, ResultsSeason) Then Throw New Exception("Error: season not found in enumerator: " & mySeasonStr)
                            If Not myDuration.POTEvents Is Nothing Then

                                Dim nHoog As Long = myDuration.POTEvents.CountByPatternType(GeneralFunctions.enmNeerslagPatroon.HOOG, ResultsSeason)
                                Dim nMidHoog As Long = myDuration.POTEvents.CountByPatternType(GeneralFunctions.enmNeerslagPatroon.MIDDELHOOG, ResultsSeason)
                                Dim nMidLaag As Long = myDuration.POTEvents.CountByPatternType(GeneralFunctions.enmNeerslagPatroon.MIDDELLAAG, ResultsSeason)
                                Dim nLaag As Long = myDuration.POTEvents.CountByPatternType(GeneralFunctions.enmNeerslagPatroon.LAAG, ResultsSeason)
                                Dim nKort As Long = myDuration.POTEvents.CountByPatternType(GeneralFunctions.enmNeerslagPatroon.KORT, ResultsSeason)
                                Dim nLang As Long = myDuration.POTEvents.CountByPatternType(GeneralFunctions.enmNeerslagPatroon.LANG, ResultsSeason)
                                Dim nUniform As Long = myDuration.POTEvents.CountByPatternType(GeneralFunctions.enmNeerslagPatroon.UNIFORM, ResultsSeason)
                                Dim nTotaal As Long = nHoog + nMidHoog + nMidLaag + nLaag + nKort + nLang + nUniform

                                For i = 1 To 7
                                    r += 1
                                    ws.ws.Cells(r, 0).Value = AnalyzedSeason.Season.ToString
                                    ws.ws.Cells(r, 1).Value = myDuration.DurationHours
                                    ws.ws.Cells(r, 2).Value = ResultsSeason.ToString
                                    ws.ws.Cells(r, 3).Value = myDuration.POTEvents.Events.Count
                                    ws.ws.Cells(r, 4).Value = nTotaal

                                    Select Case i
                                        Case Is = 1
                                            ws.ws.Cells(r, 5).Value = "HOOG"
                                            ws.ws.Cells(r, 6).Value = nHoog / nTotaal
                                        Case Is = 2
                                            ws.ws.Cells(r, 5).Value = "MIDDELHOOG"
                                            ws.ws.Cells(r, 6).Value = nMidHoog / nTotaal
                                        Case Is = 3
                                            ws.ws.Cells(r, 5).Value = "MIDDELLAAG"
                                            ws.ws.Cells(r, 6).Value = nMidLaag / nTotaal
                                        Case Is = 4
                                            ws.ws.Cells(r, 5).Value = "LAAG"
                                            ws.ws.Cells(r, 6).Value = nLaag / nTotaal
                                        Case Is = 5
                                            ws.ws.Cells(r, 5).Value = "KORT"
                                            ws.ws.Cells(r, 6).Value = nKort / nTotaal
                                        Case Is = 6
                                            ws.ws.Cells(r, 5).Value = "LANG"
                                            ws.ws.Cells(r, 6).Value = nLang / nTotaal
                                        Case Is = 7
                                            ws.ws.Cells(r, 5).Value = "UNIFORM"
                                            ws.ws.Cells(r, 6).Value = nUniform / nTotaal
                                    End Select

                                Next

                            End If
                        Next
                    Next

                Next
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function GeneralStatsToExcel() As Boolean
        Dim ws As clsExcelSheet
        Dim r As Long, c As Long

        Try
            For Each myReeks As clsRainfallSeries In NeerslagReeksen.Values
                ws = Me.Setup.ExcelFile.GetAddSheet(myReeks.Name & "_stats")
                c = 0
                r = 0

                'fist the year round stats
                c += 1
                ws.ws.Cells(0, c).Value = myReeks.Name

                r += 1
                ws.ws.Cells(r, 0).Value = "years"
                ws.ws.Cells(r, c).Value = myReeks.getTimeSpanYears

                r += 1
                ws.ws.Cells(r, 0).Value = "average year round volume"
                ws.ws.Cells(r, c).Value = myReeks.getWindowSum(0, myReeks.Values.Count - 1) / myReeks.getTimeSpanYears

                r += 1
                ws.ws.Cells(r, 0).Value = "average summer volume"
                ws.ws.Cells(r, c).Value = myReeks.getSeasonSum(GeneralFunctions.enmSeason.meteosummerquarter) / myReeks.getTimeSpanYears

                r += 1
                ws.ws.Cells(r, 0).Value = "average winter volume"
                ws.ws.Cells(r, c).Value = myReeks.getSeasonSum(GeneralFunctions.enmSeason.meteowinterquarter) / myReeks.getTimeSpanYears

                r += 1
                ws.ws.Cells(r, 0).Value = "average spring volume"
                ws.ws.Cells(r, c).Value = myReeks.getSeasonSum(GeneralFunctions.enmSeason.meteospringquarter) / myReeks.getTimeSpanYears

                r += 1
                ws.ws.Cells(r, 0).Value = "average autumn volume"
                ws.ws.Cells(r, c).Value = myReeks.getSeasonSum(GeneralFunctions.enmSeason.meteoautumnquarter) / myReeks.getTimeSpanYears

                r += 1
                ws.ws.Cells(r, 0).Value = "average number of summer days exceeding 0.1"
                ws.ws.Cells(r, c).Value = myReeks.countWetDaysBySeason(GeneralFunctions.enmSeason.meteosummerquarter, 0.1) / myReeks.getTimeSpanYears

                r += 1
                ws.ws.Cells(r, 0).Value = "average number of summer days exceeding 20"
                ws.ws.Cells(r, c).Value = myReeks.countWetDaysBySeason(GeneralFunctions.enmSeason.meteosummerquarter, 20) / myReeks.getTimeSpanYears

                r += 1
                ws.ws.Cells(r, 0).Value = "average number of winter days exceeding 0.1"
                ws.ws.Cells(r, c).Value = myReeks.countWetDaysBySeason(GeneralFunctions.enmSeason.meteowinterquarter, 0.1) / myReeks.getTimeSpanYears

                r += 1
                ws.ws.Cells(r, 0).Value = "average number of winter days exceeding 10"
                ws.ws.Cells(r, c).Value = myReeks.countWetDaysBySeason(GeneralFunctions.enmSeason.meteowinterquarter, 10) / myReeks.getTimeSpanYears

                For Each mySeason As clsSeason In myReeks.Seasons.Seasons.Values
                    For Each myDuration As clsDuration In mySeason.Durations.Values
                        If Not myDuration.AnnualMaxima Is Nothing Then
                            r += 1
                            ws.ws.Cells(r, 0).Value = "T10 " & mySeason.Season.ToString & " " & myDuration.DurationHours
                            ws.ws.Cells(r, c).Value = myDuration.AnnualMaxima.getValue(10)
                        End If
                    Next
                Next



            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function YearMaximaToExcel() As Boolean
        Dim ws As clsExcelSheet
        Dim r As Long, c As Long, i As Integer
        Try

            For Each myReeks As clsRainfallSeries In NeerslagReeksen.Values
                ws = Me.Setup.ExcelFile.GetAddSheet(myReeks.Name & "_YearMax")
                c = -3
                For Each mySeason As STOCHLIB.clsSeason In myReeks.Seasons.Seasons.Values
                    For Each myduration As clsDuration In mySeason.Durations.Values

                        If Not myduration.AnnualMaxima Is Nothing Then
                            c += 3
                            r = 0
                            i = 0
                            ws.ws.Cells(r, c).Value = "Date/Time"
                            ws.ws.Cells(r, c + 1).Value = "Volume " & myduration.DurationHours & "h"
                            ws.ws.Cells(r, c + 2).Value = "Event number " & myduration.DurationHours & "h"

                            For Each myEvent As clsTimeSeriesEvent In myduration.AnnualMaxima.Events.Values
                                i += 1
                                For Each myRecord As clsTimeTableRecord In myEvent.TimeTable.Records.Values
                                    r += 1
                                    ws.ws.Cells(r, c).Value = myRecord.Datum
                                    ws.ws.Cells(r, c + 1).Value = myRecord.GetValue(0)
                                    ws.ws.Cells(r, c + 2).Value = i
                                Next
                            Next
                        End If
                    Next
                Next
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function CalcAnnualMaxEvents(ByVal Duration As Integer, ByVal Seizoen As String) As Boolean
        'This routine identifies the individual rainfall events that make up the annual maxima 
        Try
            Dim myReeks As clsRainfallSeries = NeerslagReeksen.Values(0)
            Dim mySeason As clsSeason = myReeks.Seasons.GetAddByDescription(Seizoen)
            Dim myDuration As clsDuration = mySeason.GetAddDuration(Duration)
            If Not myDuration.calculateYearMaxima() Then Throw New Exception("Error computing annyual maxima from series.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function CalcPOTEvents(ByVal Duration As Integer, ByVal Seizoen As String, ByVal CalcSTOWAPatterns As Boolean) As Boolean
        'this routine identifies individual rainfall events that meet the POT-criterium
        'the criterium is given in n exceedances per year
        Try
            For Each mySeries As clsRainfallSeries In NeerslagReeksen.Values
                Dim mySeason As clsSeason = mySeries.Seasons.GetAddByDescription(Seizoen)
                Dim myDuration As clsDuration = mySeason.GetAddDuration(Duration)
                myDuration.calculatePOTEvents(POTFrequency, MinTimeStepsBetweenEvents, CalcSTOWAPatterns)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

End Class
