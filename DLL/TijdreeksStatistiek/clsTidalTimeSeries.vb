Option Explicit On

Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO
Imports System.Windows.Forms

Public Class clsTidalTimeSeries

    Public Name As String
    Public Dates As New List(Of DateTime) 'list of dates/times
    Public Values As New List(Of Single)  'list of elevations

    'derived objects
    Public TidalHighsLowsEvent As clsTidalHighsLowsEvent 'an event containing only the high and low tides from the timeseries
    Public TidalAmplitudeClasses As clsTidalAmplitudeClasses 'the events from the above class, subdivided in amplitude classes

    'parent objects
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Function CalcTidalMinMax(ByVal WriteToExcel As Boolean) As Boolean
        Try
            'create a new dictionary for the resulting series of highs and lows
            TidalHighsLowsEvent = New clsTidalHighsLowsEvent(Me.Setup)
            Dim SearchRadius As Long, i As Long, tsLow As Long, tsHigh As Long
            Dim r As Long, Done As Boolean = False
            Dim CurLowVal As Double, CurLowDate As Date, CurHighVal As Double, CurHighDate As Date
            Dim TideComplete As Boolean
            Dim myTide As clsTidalHighLow

            'first find out the timestep size of the tidal series
            If Dates.Count < 2 Then Throw New Exception("Insufficient number of dates in the tidal timeseries.")
            Dim TimeStepMinutes = Dates(1).Subtract(Dates(0)).TotalMinutes

            If TimeStepMinutes <= 60 Then
                SearchRadius = 5 * 60 / TimeStepMinutes 'search -5 hours to +5 hours
            Else
                Throw New Exception("Timestep in tidal series is too large to extract accurate highs and lows.")
            End If

            'search the first low within a window of 14 hours
            For i = 0 To (14 * 60) / TimeStepMinutes
                If Values(i) < CurLowVal Then
                    CurLowDate = Dates(i)
                    CurLowVal = Values(i)
                    tsLow = i
                End If
            Next

            'search for the first high within a window of 10 hours after the first low
            CurHighVal = -999999
            For i = tsLow To tsLow + (10 * 60) / TimeStepMinutes
                If Values(i) > CurHighVal Then
                    CurHighDate = Dates(i)
                    CurHighVal = Values(i)
                    tsHigh = i
                End If
            Next

            'initialize the series by adding the first found tidal low
            myTide = New clsTidalHighLow
            myTide.DateLow = Dates(tsLow)
            myTide.Low = Values(tsLow)
            myTide.tsLow = tsLow
            myTide.DateHigh = Dates(tsHigh)
            myTide.High = Values(tsHigh)
            myTide.tsHigh = tsHigh
            myTide.Amplitude = myTide.High - myTide.Low
            TidalHighsLowsEvent.Values.Add(0, myTide)
            TideComplete = True

            'now that we have a starting position, process the rest of the series
            While Not myTide Is Nothing
                myTide = getNextTidalMinMax(myTide, TimeStepMinutes, SearchRadius)
                If Not myTide Is Nothing Then TidalHighsLowsEvent.Values.Add(TidalHighsLowsEvent.Values.Count, myTide)
            End While

            If WriteToExcel Then
                Dim ws As clsExcelSheet
                ws = Me.Setup.ExcelFile.GetAddSheet(Name)
                r = 0
                ws.ws.Cells(r, 0).Value = "Date High"
                ws.ws.Cells(r, 1).Value = "High"
                ws.ws.Cells(r, 2).Value = "Date Low"
                ws.ws.Cells(r, 3).Value = "Low"
                For Each myTide In TidalHighsLowsEvent.Values.Values
                    r += 1
                    ws.ws.Cells(r, 0).Value = myTide.DateHigh
                    ws.ws.Cells(r, 1).Value = myTide.High
                    ws.ws.Cells(r, 2).Value = myTide.DateLow
                    ws.ws.Cells(r, 3).Value = myTide.Low
                Next
            End If


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function SequentialElevationStatsToExcel(ByVal nTotalEvents As Long, ByVal nTidesPerEvent As Long, ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, ByVal SearchPercentage As Integer) As Boolean
        Dim ws As clsExcelSheet
        Dim r As Long

        ws = Me.Setup.ExcelFile.GetAddSheet("Stats")

        ws.ws.Cells(0, 0).Value = "Amplitudeklasse"
        ws.ws.Cells(0, 1).Value = "Getijdencomponent"
        ws.ws.Cells(0, 2).Value = "Verloop verhoging"
        ws.ws.Cells(0, 3).Value = "ID"
        ws.ws.Cells(0, 4).Value = "Aantal gevonden events"
        ws.ws.Cells(0, 5).Value = "Kans op optreden in de laatste " & SearchPercentage & " % van de duur"

        For Each myAmplitudeClass As clsTidalAmplitudeClass In TidalAmplitudeClasses.Classes.Values
            For Each mySequentialElevationClass As clsSequentialElevatedClass In myAmplitudeClass.SequentialElevatedClasses.Classes.Values
                r += 1
                ws.ws.Cells(r, 0).Value = myAmplitudeClass.Name
                ws.ws.Cells(r, 1).Value = TidalComponent.ToString
                ws.ws.Cells(r, 2).Value = mySequentialElevationClass.GetSequentialElevationClasses(myAmplitudeClass.TidalElevatedClasses)
                ws.ws.Cells(r, 3).Value = myAmplitudeClass.Name & "-" & mySequentialElevationClass.Name
                ws.ws.Cells(r, 4).Value = mySequentialElevationClass.Events.Count
                ws.ws.Cells(r, 5).Value = mySequentialElevationClass.Events.Count / nTotalEvents
            Next
        Next

        Return True
    End Function

    Public Function ClassificationStatsToExcel(ByVal nTotalEvents As Long, ByVal nTidesPerEvent As Long, ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, ByVal SearchPercentage As Integer) As Boolean
        Dim ws As clsExcelSheet
        Dim r As Long

        ws = Me.Setup.ExcelFile.GetAddSheet("Stats")

        ws.ws.Cells(0, 0).Value = "Amplitudeklasse"
        ws.ws.Cells(0, 1).Value = "Verhogingsklasse"
        ws.ws.Cells(0, 2).Value = "Getijdencomponent"
        ws.ws.Cells(0, 3).Value = "Percentielklasse verhoging"
        ws.ws.Cells(0, 4).Value = "Klasse vertegenwoordigd door percentielwaarde"
        ws.ws.Cells(0, 5).Value = "Aantal achtereenvolgende verhogingen in de klasse"
        ws.ws.Cells(0, 6).Value = "Aantal gevonden events"
        ws.ws.Cells(0, 7).Value = "Kans op optreden in de laatste " & SearchPercentage & " % van de duur"

        For Each myAmplitudeClass As clsTidalAmplitudeClass In TidalAmplitudeClasses.Classes.Values
            For Each myElevationClass As clsTidalElevatedClass In myAmplitudeClass.TidalElevatedClasses.Classes.Values
                For Each mySequentialOccurrancesClass As clsTidalSequentialOccurrancesClass In myElevationClass.SequentialOccurrancesClasses.Classes.Values
                    r += 1
                    ws.ws.Cells(r, 0).Value = myAmplitudeClass.Name
                    ws.ws.Cells(r, 1).Value = myElevationClass.Name
                    ws.ws.Cells(r, 2).Value = TidalComponent.ToString
                    ws.ws.Cells(r, 3).Value = "[" & myElevationClass.lPerc & " tot " & myElevationClass.uPerc & "]"
                    ws.ws.Cells(r, 4).Value = myElevationClass.repPerc
                    ws.ws.Cells(r, 5).Value = mySequentialOccurrancesClass.nSequential
                    ws.ws.Cells(r, 6).Value = mySequentialOccurrancesClass.Events.Count
                    ws.ws.Cells(r, 7).Value = mySequentialOccurrancesClass.Events.Count / nTotalEvents
                Next
            Next
        Next

        Return True
    End Function

    Public Function classifyEventsByAmplitude(ByRef AmplitudeClassesGrid As Windows.Forms.DataGridView) As Boolean
        'classifies the collection of tidal events into amplitude classes
        Dim i As Long, myEvent As clsTidalHighsLowsEvent
        Dim Amplitudes(TidalHighsLowsEvent.TidalHighLowEvents.Count - 1) As Double
        Dim myAmplitude As Double
        Dim myAmplitudeClass As clsTidalAmplitudeClass

        Try
            'create an instance for the object containing the amplitude classes
            TidalAmplitudeClasses = New clsTidalAmplitudeClasses(Me.Setup)

            'first create an array with the average amplitude per event
            If TidalHighsLowsEvent.TidalHighLowEvents.Count > 0 Then

                'compute the average amplitude per tidal event and place the values in an array
                For i = 0 To TidalHighsLowsEvent.TidalHighLowEvents.Count - 1
                    myEvent = TidalHighsLowsEvent.TidalHighLowEvents.Values(i)
                    Amplitudes(i) = myEvent.calcAverageAmplitude
                Next

                'then create the amplitude classes and assign each event to its corresponding class
                For Each myRow As DataGridViewRow In AmplitudeClassesGrid.Rows
                    myAmplitudeClass = New clsTidalAmplitudeClass(Me.Setup)
                    myAmplitudeClass.Name = myRow.Cells(0).Value
                    myAmplitudeClass.lPercentile = myRow.Cells(1).Value
                    myAmplitudeClass.uPercentile = myRow.Cells(2).Value
                    myAmplitudeClass.repPercentile = (myAmplitudeClass.lPercentile + myAmplitudeClass.uPercentile) / 2
                    myAmplitudeClass.lBoundVal = Me.Setup.GeneralFunctions.Percentile(Amplitudes, myAmplitudeClass.lPercentile)
                    myAmplitudeClass.uBoundVal = Me.Setup.GeneralFunctions.Percentile(Amplitudes, myAmplitudeClass.uPercentile)
                    myAmplitudeClass.repVal = Me.Setup.GeneralFunctions.Percentile(Amplitudes, myAmplitudeClass.repPercentile)

                    'walk through all events, calculate their average amplitude and assign each event to its corresponding class
                    For i = 0 To TidalHighsLowsEvent.TidalHighLowEvents.Values.Count - 1
                        myEvent = TidalHighsLowsEvent.TidalHighLowEvents.Values(i)
                        If Not myEvent.AmplitudeClassified Then
                            myAmplitude = myEvent.calcAverageAmplitude
                            If myAmplitude >= myAmplitudeClass.lBoundVal AndAlso myAmplitude <= myAmplitudeClass.uBoundVal Then
                                myEvent.AmplitudeClassified = True
                                myAmplitudeClass.Events.Add(i, myEvent)
                            End If
                        End If
                    Next
                    Call TidalAmplitudeClasses.Classes.Add(myAmplitudeClass.repVal, myAmplitudeClass)
                Next
            Else
                Throw New Exception("Error: tidal timeseries has not yet been subdivided into separate events of a given duration. Please contact Hydroconsult for support.")
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function getNextTidalMinMax(ByVal prevTide As clsTidalHighLow, ByVal TimestepMinutes As Long, ByVal SearchRadius As Long) As clsTidalHighLow
        Dim i As Long, ts As Long, newTide As clsTidalHighLow
        Dim CurHighDate As Date, CurHighTs As Long, CurHighVal As Double = -999999
        Dim CurLowDate As Date, CurLowTs As Long, CurLowVal As Double = 999999

        Try

            If Math.Max(prevTide.tsLow, prevTide.tsHigh) + (12.5 * 60) / TimestepMinutes > Dates.Count - 1 Then Return Nothing

            'find the next low
            i = prevTide.tsLow + 12.5 * 60 / TimestepMinutes
            For ts = i - SearchRadius To Math.Min(i + SearchRadius, Dates.Count - 1)
                If Values(ts) < CurLowVal Then
                    CurLowVal = Values(ts)
                    CurLowDate = Dates(ts)
                    CurLowTs = ts
                End If
            Next

            'find the next high
            i = prevTide.tsHigh + 12.5 * 60 / TimestepMinutes
            For ts = i - SearchRadius To Math.Min(i + SearchRadius, Dates.Count - 1)
                If Values(ts) > CurHighVal Then
                    CurHighVal = Values(ts)
                    CurHighDate = Dates(ts)
                    CurHighTs = ts
                End If
            Next

            'create a new tidal pair
            newTide = New clsTidalHighLow
            newTide.DateHigh = CurHighDate
            newTide.High = CurHighVal
            newTide.tsHigh = CurHighTs
            newTide.DateLow = CurLowDate
            newTide.Low = CurLowVal
            newTide.tsLow = CurLowTs
            Return newTide

        Catch ex As Exception
            Stop
            Return Nothing
        End Try



    End Function

    Public Function ReadFromCSV(ByVal Path As String, ByVal Delimiter As String, ByVal DateFormatting As String, TimeFormatting As String, ByVal DataValueColumnName As String, Multiplier As Double, ByVal DateColumnName As String, Optional ByVal TimeColumnName As String = "NULL") As Boolean
        Try
            Dim myStr As String, tmpStr As String, HeaderColIdx As Integer = -1, DateColIdx As Integer = -1, TimeColIdx As Integer = -1, curIdx As Integer, i As Integer
            Dim nNonNumeric As Long, r As Long
            Dim myDate As DateTime, lastDate As DateTime, mySpan As TimeSpan, myVal As Single, isValid As Boolean

            Using csvReader As New StreamReader(Path)

                'find the column index for the data and the date
                myStr = csvReader.ReadLine
                curIdx = -1
                While Not myStr = ""
                    tmpStr = Me.Setup.GeneralFunctions.ParseString(myStr, Delimiter)
                    curIdx += 1
                    If tmpStr.Trim.ToUpper = DataValueColumnName.Trim.ToUpper Then
                        HeaderColIdx = curIdx
                    ElseIf tmpStr.Trim.ToUpper = DateColumnName.Trim.ToUpper Then
                        DateColIdx = curIdx
                    ElseIf Not TimeColumnName = "NULL" AndAlso tmpStr.Trim.ToUpper = TimeColumnName.Trim.ToUpper Then
                        TimeColIdx = curIdx
                    End If
                End While

                If HeaderColIdx < 0 Then
                    Throw New Exception("Error: could not find data column " & DataValueColumnName & " in csv file: " & Path)
                ElseIf DateColIdx < 0 Then
                    Throw New Exception("Error: could not find date column " & DateColumnName & " in csv file: " & Path)
                Else
                    While Not csvReader.EndOfStream
                        r += 1
                        isValid = False

                        'update the progress bar only once every 1000 rows
                        If r / 1000 = Math.Round(r / 1000) Then
                            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading csv file...", csvReader.BaseStream.Position, csvReader.BaseStream.Length, True)
                        End If

                        mySpan = New TimeSpan()
                        myStr = csvReader.ReadLine
                        For i = 0 To Math.Max(Math.Max(HeaderColIdx, DateColIdx), TimeColIdx)
                            tmpStr = Me.Setup.GeneralFunctions.ParseString(myStr, Delimiter)
                            If i = DateColIdx Then
                                If Not Me.Setup.GeneralFunctions.ParseDateString(tmpStr, DateFormatting, myDate) Then Throw New Exception("Error parsing string: " & tmpStr & Delimiter & myStr)
                            ElseIf i = HeaderColIdx Then
                                If IsNumeric(tmpStr) Then
                                    myVal = Convert.ToSingle(tmpStr) * Multiplier
                                    If myVal > -9 AndAlso myVal < 9 Then isValid = True 'safety measure
                                Else
                                    nNonNumeric += 1
                                End If
                            ElseIf i = TimeColIdx Then
                                If Not Me.Setup.GeneralFunctions.ParseTimeString(tmpStr, TimeFormatting, mySpan) Then Throw New Exception("Error parsing time String: " & tmpStr)
                            End If
                        Next
                        myDate = myDate.Add(mySpan)

                        'prevent importing double dates!!!! implemented 20140920 for RACMO timeseries
                        If myDate <> lastDate AndAlso isValid Then
                            Dates.Add(myDate)
                            Values.Add(myVal)
                        Else
                            Me.Setup.Log.AddError("CSV file contains multiple records of date " & myDate)
                        End If
                        lastDate = myDate

                    End While
                End If
            End Using

            If nNonNumeric > 0 Then Me.Setup.Log.AddWarning("csv file " & Path & " contained " & nNonNumeric & " non-numeric values for time series " & DataValueColumnName & ".")

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function


    Public Function BuildDesignSeries(ByVal Duration As Integer, ByVal Uitloop As Integer, ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, ByVal ApplicationPercentage As Integer) As Boolean
        Try
            Dim ws As clsExcelSheet, i As Long, c As Long = 0
            Dim Hoog As Double, Laag As Double, SpringHoog As Double, SpringLaag As Double
            ws = Me.Setup.ExcelFile.GetAddSheet("OntwerpGetij")
            Dim EventName As String

            For Each myAmplitudeClass As clsTidalAmplitudeClass In TidalAmplitudeClasses.Classes.Values
                For Each myElevationClass As clsTidalElevatedClass In myAmplitudeClass.TidalElevatedClasses.Classes.Values
                    For Each mySequentialClass As clsTidalSequentialOccurrancesClass In myElevationClass.SequentialOccurrancesClasses.Classes.Values

                        Hoog = myAmplitudeClass.CalcAverageHigh
                        Laag = myAmplitudeClass.CalcAverageLow

                        Dim Calculation(,) As Object = Nothing
                        Select Case TidalComponent
                            Case Is = enmTidalComponent.VerhoogdLaagwater
                                SpringHoog = myElevationClass.ReprVal + myAmplitudeClass.repVal
                                SpringLaag = myElevationClass.ReprVal
                                EventName = mySequentialClass.nSequential & "xHLW_" & myElevationClass.Name & "_" & myAmplitudeClass.Name
                                BuildStochasticTidalTimeSeries(EventName, New Date(2000, 1, 1), Duration, Uitloop, 10, Hoog, Laag, 12.5, SpringHoog, SpringLaag, mySequentialClass.nSequential, ApplicationPercentage, Calculation)
                            Case Is = enmTidalComponent.VerhoogdeMiddenstand
                                SpringHoog = myElevationClass.ReprVal + myAmplitudeClass.repVal / 2
                                SpringLaag = myElevationClass.ReprVal - myAmplitudeClass.repVal / 2
                                EventName = mySequentialClass.nSequential & "xMID_" & myElevationClass.Name & "_" & myAmplitudeClass.Name
                                BuildStochasticTidalTimeSeries(EventName, New Date(2000, 1, 1), Duration, Uitloop, 10, Hoog, Laag, 12.5, SpringHoog, SpringLaag, mySequentialClass.nSequential, ApplicationPercentage, Calculation)
                            Case Is = enmTidalComponent.VerhoogdHoogwater
                                SpringHoog = myElevationClass.ReprVal
                                SpringLaag = myElevationClass.ReprVal - myAmplitudeClass.repVal
                                EventName = mySequentialClass.nSequential & "xHHW_" & myElevationClass.Name & "_" & myAmplitudeClass.Name
                                BuildStochasticTidalTimeSeries(EventName, New Date(2000, 1, 1), Duration, Uitloop, 10, Hoog, Laag, 12.5, SpringHoog, SpringLaag, mySequentialClass.nSequential, ApplicationPercentage, Calculation)
                        End Select

                        c += 1
                        For i = 0 To UBound(Calculation, 1) - 1
                            ws.ws.Cells(i, 0).Value = Calculation(i, 0)
                            ws.ws.Cells(i, c).Value = Calculation(i, 3)
                        Next

                    Next
                Next
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function BuildSequentialElevationDesignSeries(ByVal Duration As Integer, ByVal Uitloop As Integer, ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, ByVal ApplicationPercentage As Integer) As Boolean
        Try
            Dim ws As clsExcelSheet, i As Long, c As Long = 0
            Dim Hoog As Double, Laag As Double, SpringHoog As List(Of Double), SpringLaag As List(Of Double)
            ws = Me.Setup.ExcelFile.GetAddSheet("OntwerpGetij")
            Dim ElevationClassIdx As Integer
            Dim StartElevatedValuesIdx As Integer
            Dim nWaves As Integer

            'first decide which tidal wave inside the duration will carry the first elevated value
            nWaves = Duration / 12.5
            StartElevatedValuesIdx = Me.Setup.GeneralFunctions.RoundUD(nWaves * (100 - ApplicationPercentage) / 100, 0, True)

            For Each myAmplitudeClass As clsTidalAmplitudeClass In TidalAmplitudeClasses.Classes.Values
                For Each mySequentialClass As clsSequentialElevatedClass In myAmplitudeClass.SequentialElevatedClasses.Classes.Values
                    Hoog = myAmplitudeClass.CalcAverageHigh
                    Laag = myAmplitudeClass.CalcAverageLow
                    SpringHoog = New List(Of Double)
                    SpringLaag = New List(Of Double)

                    Dim Calculation(,) As Object = Nothing
                    Select Case TidalComponent
                        Case Is = enmTidalComponent.VerhoogdLaagwater
                            For Each ElevationClassIdx In mySequentialClass.SequentialElevationClassIndices
                                SpringHoog.Add(myAmplitudeClass.TidalElevatedClasses.Classes.Values(ElevationClassIdx).ReprVal + myAmplitudeClass.repVal)
                                SpringLaag.Add(myAmplitudeClass.TidalElevatedClasses.Classes.Values(ElevationClassIdx).ReprVal)
                            Next
                            BuildStochasticTidalTimeSeriesAdvanced(myAmplitudeClass.Name & "-" & mySequentialClass.Name, New Date(2000, 1, 1), Duration, Uitloop, 10, Hoog, Laag, 12.5, StartElevatedValuesIdx, SpringHoog, SpringLaag, Calculation)
                        Case Is = enmTidalComponent.VerhoogdeMiddenstand
                            For Each ElevationClassIdx In mySequentialClass.SequentialElevationClassIndices
                                SpringHoog.Add(myAmplitudeClass.TidalElevatedClasses.Classes.Values(ElevationClassIdx).ReprVal + myAmplitudeClass.repVal / 2)
                                SpringLaag.Add(myAmplitudeClass.TidalElevatedClasses.Classes.Values(ElevationClassIdx).ReprVal - myAmplitudeClass.repVal / 2)
                            Next
                            BuildStochasticTidalTimeSeriesAdvanced(myAmplitudeClass.Name & "-" & mySequentialClass.Name, New Date(2000, 1, 1), Duration, Uitloop, 10, Hoog, Laag, 12.5, StartElevatedValuesIdx, SpringHoog, SpringLaag, Calculation)
                        Case Is = enmTidalComponent.VerhoogdHoogwater
                            For Each ElevationClassIdx In mySequentialClass.SequentialElevationClassIndices
                                SpringHoog.Add(myAmplitudeClass.TidalElevatedClasses.Classes.Values(ElevationClassIdx).ReprVal)
                                SpringLaag.Add(myAmplitudeClass.TidalElevatedClasses.Classes.Values(ElevationClassIdx).ReprVal - myAmplitudeClass.repVal)
                            Next
                            BuildStochasticTidalTimeSeriesAdvanced(myAmplitudeClass.Name & "-" & mySequentialClass.Name, New Date(2000, 1, 1), Duration, Uitloop, 10, Hoog, Laag, 12.5, StartElevatedValuesIdx, SpringHoog, SpringLaag, Calculation)
                    End Select

                    c += 1
                    For i = 0 To UBound(Calculation, 1) - 1
                        ws.ws.Cells(i, 0).Value = Calculation(i, 0)
                        ws.ws.Cells(i, c).Value = Calculation(i, 3)
                    Next

                Next
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub BuildStochasticTidalTimeSeries(ByVal EventName As String, ByVal StartEvent As Date, ByVal DurationHours As Long, ByVal UitloopHours As Long, ByVal TimestepMinutes As Integer, ByVal HighTide As Double, ByVal LowTide As Double, ByVal PeriodHours As Double, ByVal Springvloed As Double, ByVal SpringEb As Double, ByVal nSequentialExceedances As Integer, ByVal ApplicationPercentage As Integer, ByRef Calculation(,) As Object)
        'this routine generates an artificial timeseries with tidal movement and nSequentialExceedances elevated tidal highs and lows

        'declare variables
        Dim LengthMinutes As Integer, TimeMinutes As Integer
        Dim i As Integer, nSteps As Integer

        'convert durations to minutes
        Dim DurationMinutes As Long = DurationHours * 60         'convert duration to minutes
        Dim UitloopMinutes As Long = UitloopHours * 60           'convert uitloop duration to minutes
        Dim PeriodeMinutes As Long = PeriodHours * 60            'convert tidal period to minutes
        Dim StartElevations As Integer

        'the elevations start at at n times a whole period. Initialize the starting moment of the elevated levels at minimum one period
        StartElevations = PeriodeMinutes
        For i = PeriodeMinutes To DurationMinutes Step PeriodeMinutes
            'first make sure the number of exceedances actually fit inside the remaining timesteps
            'EDIT SIEBE BOSCH 27-7-2016: we'll accept a tiny exceedance of the remaining timesteps in order to prevent elevations to occur in the first few timesteps
            If (PeriodeMinutes * nSequentialExceedances) <= (DurationMinutes - i) Then
                'the number of exceedances still fit
                StartElevations = i
                'as soon as the current step is inide the last ApplicationPercentage, keep the current index as the starting point and exit the loop
                If i > (DurationMinutes - DurationMinutes * ApplicationPercentage / 100) Then Exit For
            End If
        Next

        'create time axis
        LengthMinutes = DurationMinutes + UitloopMinutes              'length of the event in minutes
        nSteps = Math.Round(LengthMinutes / TimestepMinutes, 0) + 1   'number of timesteps of the event

        ReDim Calculation(0 To nSteps, 0 To 4)     'we'll use the first row for headers
        Calculation(0, 0) = "Tijdstap in minuten"
        Calculation(0, 1) = "Amplitude"
        Calculation(0, 2) = "Verticale shift"
        Calculation(0, 3) = EventName

        'walk through all output timesteps and generate the tidal data
        For i = 1 To nSteps

            TimeMinutes = (i - 1) * TimestepMinutes
            Calculation(i, 0) = TimeMinutes

            If TimeMinutes < (StartElevations - 0.25 * PeriodeMinutes) Then
                'periode vanaf inloop tot laatste 'nulwaarde' voorafgaand aan springvloed
                Calculation(i, 1) = HighTide - LowTide '"Amplitude"
                Calculation(i, 2) = (HighTide + LowTide) / 2 '"Verticale shift"

            ElseIf TimeMinutes >= (StartElevations - 0.25 * PeriodeMinutes) And TimeMinutes < StartElevations Then
                'overgangsperiode tussen laatste 'nulwaarde' en Springvloed
                Calculation(i, 1) = Springvloed - LowTide '"Amplitude"
                Calculation(i, 2) = (Springvloed + LowTide) / 2 '"Verticale shift"

            ElseIf TimeMinutes >= StartElevations And TimeMinutes < (StartElevations + PeriodeMinutes * (nSequentialExceedances - 0.5)) Then
                'periode tussen springvloed en laatste springeb
                Calculation(i, 1) = Springvloed - SpringEb '"Amplitude"
                Calculation(i, 2) = (Springvloed + SpringEb) / 2 '"Verticale shift"

            ElseIf TimeMinutes >= (StartElevations + PeriodeMinutes * (nSequentialExceedances - 0.5)) And TimeMinutes < (StartElevations + PeriodeMinutes * nSequentialExceedances) Then ' + 0.5 * PeriodeMinutes) Then
                'periode tussen laatste springeb en eerste vloed van uitloop
                Calculation(i, 1) = HighTide - SpringEb '"Amplitude"
                Calculation(i, 2) = (HighTide + SpringEb) / 2 '"Verticale shift"

            Else
                'periode tussen eerst vloed / eb van uitloop en einde van reeks
                Calculation(i, 1) = HighTide - LowTide '"Amplitude"
                Calculation(i, 2) = (HighTide + LowTide) / 2 '"Verticale shift"
            End If

        Next

        'doorrekenen en terugshiften in tijd.
        For i = 1 To nSteps - 1
            'bereken: Waterstand = A * cos (periode/2 pi() * verschoven tijdstap) + verticale shift + algemene verhoging
            'Calculation(i, 3) = 0.5 * Calculation(i, 1) * Math.Cos((2 * 3.141593) / PeriodeMinutes * Calculation(i, 0)) + Calculation(i, 2) ' + Calculation(i, 2)) ' + VerhogingInM
            Calculation(i, 3) = 0.5 * Calculation(i, 1) * Math.Cos((2 * 3.141593) / PeriodeMinutes * Calculation(i, 0)) + Calculation(i, 2) ' + Calculation(i, 2)) ' + VerhogingInM

            'verschuif tijdsas terug in de tijd en maak er een numerieke waarde van met startdatum 1-1-2000
            Calculation(i, 0) = StartEvent.Add(New TimeSpan(0, Calculation(i, 0), 0))

        Next

    End Sub

    Public Sub BuildStochasticTidalTimeSeriesAdvanced(ByVal EventName As String, ByVal StartEvent As Date, ByVal DurationHours As Long, ByVal UitloopHours As Long, ByVal TimestepMinutes As Integer, ByVal HighAvg As Double, ByVal LowAvg As Double, ByVal PeriodHours As Double, ByVal StartElevatedWavesIdx As Integer, ByVal Vloed As List(Of Double), ByVal Eb As List(Of Double), ByRef Calculation(,) As Object)
        'this routine generates an artificial timeseries with tidal movement and various elevated tidal highs and lows

        'declare variables
        Dim LengthMinutes As Integer, TimeMinutes As Integer
        Dim i As Integer, nSteps As Integer
        Dim TidalWaveIdx As Integer, RestMinuten As Integer
        Dim ElevatedWaveIdx As Integer

        'convert durations to minutes
        Dim DurationMinutes As Long = DurationHours * 60         'convert duration to minutes
        Dim UitloopMinutes As Long = UitloopHours * 60           'convert uitloop duration to minutes
        Dim PeriodeMinutes As Long = PeriodHours * 60            'convert tidal period to minutes

        'create time axis
        LengthMinutes = DurationMinutes + UitloopMinutes              'length of the event in minutes
        nSteps = Math.Round(LengthMinutes / TimestepMinutes, 0) + 1   'number of timesteps of the event

        ReDim Calculation(0 To nSteps, 0 To 4)     'we'll use the first row for headers
        Calculation(0, 0) = "Tijdstap in minuten"
        Calculation(0, 1) = "Amplitude"
        Calculation(0, 2) = "Verticale shift"
        Calculation(0, 3) = EventName

        Try
            'walk through all output timesteps and generate the tidal data

            For i = 1 To nSteps
                TimeMinutes = (i - 1) * TimestepMinutes
                Calculation(i, 0) = TimeMinutes

                'decide in which tidal wave we currently are
                TidalWaveIdx = Me.Setup.GeneralFunctions.RoundUD(TimeMinutes / PeriodeMinutes, 0, False)
                ElevatedWaveIdx = TidalWaveIdx - StartElevatedWavesIdx

                RestMinuten = TimeMinutes - PeriodeMinutes * TidalWaveIdx

                If TidalWaveIdx >= StartElevatedWavesIdx AndAlso TidalWaveIdx <= (StartElevatedWavesIdx + Vloed.Count - 1) Then

                    'we're inside the period of predefined highs and lows
                    If RestMinuten < (0.25 * PeriodeMinutes) Then
                        'oploop naar het hoogwater
                        If ElevatedWaveIdx > 0 Then
                            Calculation(i, 1) = Vloed(ElevatedWaveIdx) - Eb(ElevatedWaveIdx - 1) '"Amplitude"
                            Calculation(i, 2) = (Vloed(ElevatedWaveIdx) + Eb(ElevatedWaveIdx - 1)) / 2 '"Verticale shift"
                        Else
                            'oploop naar het eerste hoogwater, dus gebruik de gemiddelde laagwaterstand als laagwaterbasis
                            Calculation(i, 1) = Vloed(ElevatedWaveIdx) - LowAvg '"Amplitude"
                            Calculation(i, 2) = (Vloed(ElevatedWaveIdx) + LowAvg) / 2 '"Verticale shift"
                        End If

                    ElseIf RestMinuten < (0.75 * PeriodeMinutes) Then
                        'neergang naar laagwater
                        Calculation(i, 1) = Vloed(ElevatedWaveIdx) - Eb(ElevatedWaveIdx) '"Amplitude"
                        Calculation(i, 2) = (Vloed(ElevatedWaveIdx) + Eb(ElevatedWaveIdx)) / 2 '"Verticale shift"

                    ElseIf RestMinuten <= PeriodeMinutes Then
                        'oploop naar doodtij
                        If ElevatedWaveIdx < (Vloed.Count - 1) Then
                            Calculation(i, 1) = Vloed(ElevatedWaveIdx + 1) - Eb(ElevatedWaveIdx) '"Amplitude"
                            Calculation(i, 2) = (Vloed(ElevatedWaveIdx + 1) + Eb(ElevatedWaveIdx)) / 2 '"Verticale shift"
                        Else
                            Calculation(i, 1) = HighAvg - Eb(ElevatedWaveIdx) '"Amplitude"
                            Calculation(i, 2) = (HighAvg + Eb(ElevatedWaveIdx)) / 2 '"Verticale shift"
                        End If
                    End If

                Else
                    Calculation(i, 1) = HighAvg - LowAvg '"Amplitude"
                    Calculation(i, 2) = (HighAvg + LowAvg) / 2 '"Verticale shift"
                End If
            Next

            'doorrekenen en terugshiften in tijd.
            For i = 1 To nSteps - 1
                'bereken: Waterstand = A * cos (periode/2 pi() * verschoven tijdstap) + verticale shift + algemene verhoging
                'Calculation(i, 3) = 0.5 * Calculation(i, 1) * Math.Cos((2 * 3.141593) / PeriodeMinutes * Calculation(i, 0)) + Calculation(i, 2) ' + Calculation(i, 2)) ' + VerhogingInM
                'Calculation(i, 3) = 0.5 * Calculation(i, 1) * Math.Cos((2 * 3.141593) / PeriodeMinutes * Calculation(i, 0)) + Calculation(i, 2) ' + Calculation(i, 2)) ' + VerhogingInM
                Calculation(i, 3) = 0.5 * Calculation(i, 1) * Math.Sin((2 * 3.141593) / PeriodeMinutes * Calculation(i, 0)) + Calculation(i, 2) ' + Calculation(i, 2)) ' + VerhogingInM

                'verschuif tijdsas terug in de tijd en maak er een numerieke waarde van met startdatum 1-1-2000
                Calculation(i, 0) = StartEvent.Add(New TimeSpan(0, Calculation(i, 0), 0))
            Next
        Catch ex As Exception
            Stop
        End Try



    End Sub
End Class
