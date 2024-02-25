Imports STOCHLIB.General
Imports System.IO

Public Class clsDuration

    Public DurationHours As Integer     'the duration in hours
    Public DurationTimesteps As Integer 'the duration in the series, expressed in number of timesteps
    Public AnnualMaxima As clsRainfallEvents
    Public POTEvents As clsRainfallEvents

    Public InUse As New List(Of Integer)

    Private Setup As clsSetup
    Private Series As clsModelTimeSeries
    Private Season As clsSeason

    Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsModelTimeSeries, ByRef mySeason As clsSeason, ByVal myDurationHours As Integer)
        Setup = mySetup
        Series = mySeries
        Season = mySeason
        DurationHours = myDurationHours
    End Sub

    Public Function getInUse(ByVal startIdx As Long, ByVal endIdx As Long) As Boolean
        Dim i As Long
        For i = Math.Max(0, startIdx) To Math.Min(endIdx, InUse.Count - 1)
            If InUse.Item(i) > 0 Then Return True
        Next
        Return False
    End Function

    Public Function calculateYearMaxima() As Boolean
        'This routine retrieves the annual maximum precipitation events from a long timeseries
        'It returns an instance of the class clsRainfallEvents
        Dim ts As Long, lastYear As Integer = 0
        Dim curYear As Integer
        Dim newEvent As clsTimeSeriesEvent, maxEvent As clsTimeSeriesEvent = Nothing
        Dim mySum As Double, lastIdx As Long = 0
        Dim newRecord As clsTimeTableRecord
        Dim IsInUse As Boolean

        Dim AnnualMaxVal As New Dictionary(Of Integer, Double)    'contains for each year the annual maximum precipitation sum
        Dim AnnualMaxIdx As New Dictionary(Of Integer, Long)      'contains for each year the indexnumber for the start of the highest precipitation event

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Calculating yearly maxima for " & Season.Season.ToString & ", " & DurationHours & "H", 0, 10)

            'calculate the timestep size for the given duration
            DurationTimesteps = DurationHours * (60 / Series.GetTimeStepSizeMinutes)

            AnnualMaxima = New clsRainfallEvents(Me.Setup, Me.Series, Me.Season, Me)

            'first find the index numbers for the precipitation maxima of each year in the series
            'walk through all records in the precipitation series with the duration as step size
            For ts = 0 To Series.Values.Count - 1 - DurationTimesteps
                Me.Setup.GeneralFunctions.UpdateProgressBar("", ts, Series.Values.Count - 1 - DurationTimesteps)

                'find out in which year we are and create a dictionary entry for this year
                curYear = Year(Series.Dates.Item(ts))
                If Not AnnualMaxIdx.ContainsKey(curYear) Then AnnualMaxIdx.Add(curYear, 0)
                If Not AnnualMaxVal.ContainsKey(curYear) Then AnnualMaxVal.Add(curYear, 0)

                'calculate the window's sum
                mySum = Series.getWindowSum(GeneralFunctions.enmModelParameter.precipitation, ts, DurationTimesteps)

                'check if the first date of the event matches the season we're researching
                IsInUse = False
                If Not Season.DateInSeason(Series.Dates(ts)) Then IsInUse = True 'only process if the event date is inside the requested season

                'If Season.Season = GeneralFunctions.enmSeason.hydrosummerhalfyear AndAlso Not Me.Setup.GeneralFunctions.HydrologischHalfJaar(Series.Dates(ts)) = GeneralFunctions.enmSeason.hydrosummerhalfyear Then
                '  IsInUse = True
                'ElseIf Season.Season = GeneralFunctions.enmSeason.hydrowinterhalfyear AndAlso Not Me.Setup.GeneralFunctions.HydrologischHalfJaar(Series.Dates(ts)) = GeneralFunctions.enmSeason.hydrowinterhalfyear Then
                '  IsInUse = True
                'ElseIf Season.Season = GeneralFunctions.enmSeason.meteosummerhalfyear AndAlso Not Me.Setup.GeneralFunctions.MeteorologischHalfJaar(Series.Dates(ts)) = GeneralFunctions.enmSeason.meteosummerhalfyear Then
                '  IsInUse = True
                'ElseIf Season.Season = GeneralFunctions.enmSeason.meteowinterhalfyear AndAlso Not Me.Setup.GeneralFunctions.MeteorologischHalfJaar(Series.Dates(ts)) = GeneralFunctions.enmSeason.meteowinterhalfyear Then
                '  IsInUse = True
                'ElseIf Season.Season = GeneralFunctions.enmSeason.meteospringquarter AndAlso Not Me.Setup.GeneralFunctions.MeteorologischSeizoen(Series.Dates(ts)) = GeneralFunctions.enmSeason.meteospringquarter Then
                '  IsInUse = True
                'ElseIf Season.Season = GeneralFunctions.enmSeason.meteosummerquarter AndAlso Not Me.Setup.GeneralFunctions.MeteorologischSeizoen(Series.Dates(ts)) = GeneralFunctions.enmSeason.meteosummerquarter Then
                '  IsInUse = True
                'ElseIf Season.Season = GeneralFunctions.enmSeason.meteoautumnquarter AndAlso Not Me.Setup.GeneralFunctions.MeteorologischSeizoen(Series.Dates(ts)) = GeneralFunctions.enmSeason.meteoautumnquarter Then
                '  IsInUse = True
                'ElseIf Season.Season = GeneralFunctions.enmSeason.meteowinterquarter AndAlso Not Me.Setup.GeneralFunctions.MeteorologischSeizoen(Series.Dates(ts)) = GeneralFunctions.enmSeason.meteowinterquarter Then
                '  IsInUse = True
                'ElseIf Season.Season = GeneralFunctions.enmSeason.marchthroughoctober AndAlso Not Me.Setup.GeneralFunctions.MeteorologischSeizoen(Series.Dates(ts)) = GeneralFunctions.enmSeason.marchthroughoctober Then
                '  IsInUse = True
                'ElseIf Season.Season = GeneralFunctions.enmSeason.novemberthroughfebruary AndAlso Not Me.Setup.GeneralFunctions.MeteorologischSeizoen(Series.Dates(ts)) = GeneralFunctions.enmSeason.novemberthroughfebruary Then
                '  IsInUse = True
                'End If

                If IsInUse = False Then
                    If mySum > AnnualMaxVal.Item(curYear) Then
                        'prevent overlapping events accross years
                        If AnnualMaxIdx.Count <= 1 Then
                            AnnualMaxVal.Item(curYear) = mySum
                            AnnualMaxIdx.Item(curYear) = ts
                        ElseIf AnnualMaxIdx.Item(curYear - 1) <= ts - DurationTimesteps Then
                            AnnualMaxVal.Item(curYear) = mySum
                            AnnualMaxIdx.Item(curYear) = ts
                        End If
                    End If
                End If
            Next

            'next, actually create the precipitation events and add them to the duration
            For Each myKey As Integer In AnnualMaxIdx.Keys
                newEvent = New clsTimeSeriesEvent(Me.Setup, Me.Series, Me.Season, Me)
                newEvent.StartTs = AnnualMaxIdx.Item(myKey)
                For ts = newEvent.StartTs To newEvent.StartTs + DurationTimesteps - 1
                    newRecord = New clsTimeTableRecord
                    newRecord.Datum = Series.Dates.Item(ts)
                    newRecord.SetValue(0, Series.Values(GeneralFunctions.enmModelParameter.precipitation).Item(ts))
                    If Not newEvent.TimeTable.Records.ContainsKey(newRecord.Datum) Then
                        newEvent.TimeTable.Records.Add(newRecord.Datum, newRecord)
                    End If
                Next
                newEvent.CalculateSum(GeneralFunctions.enmModelParameter.precipitation)
                AnnualMaxima.Events.Add(myKey, newEvent)
            Next

            'finally compute the plotting positions and place the results in a datatable (for later binding with the chart object)
            AnnualMaxima.CalculatePlottingPositions(1)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in sub calculateAnnualMaxima of class clsRainfallSeries.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function calculatePOTEvents(ByVal ModelParameter As GeneralFunctions.enmModelParameter, ByVal PotFrequency As Integer, ByVal MinTimestepsBetweenEvents As Integer, ByVal CalcSTOWAPatterns As Boolean) As Boolean
        'This routine identifies all rainfall events that meet the POT-criteria of
        'nPerYear exceedances
        'it does this by sorting the volume sums in descending order
        Dim mySpan As TimeSpan, nYears As Integer
        Dim i As Long, n As Long, nTarget As Long
        Dim Done As Boolean = False
        Dim nVals As Long = Series.Dates.Count
        Dim newEvent As clsTimeSeriesEvent
        Dim newRecord As clsTimeTableRecord
        Dim curIdx As Long = -1, ts As Long, midts As Long
        Dim IsInUse As Boolean
        Dim startts As Long, endts As Long

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Calculating POT-events for " & Season.Season.ToString & ", " & DurationHours & "H", 0, 10)

            DurationTimesteps = DurationHours * (60 / Series.GetTimeStepSizeMinutes)
            POTEvents = New clsRainfallEvents(Me.Setup, Me.Series, Me.Season, Me)

            'first find out how many years we actually have and set the target number of events we're searching for
            mySpan = Series.Dates.Item(Series.Dates.Count - 1).Subtract(Series.Dates.Item(0))
            nYears = mySpan.Days / 365.2425
            nTarget = PotFrequency * nYears           'the number of POT-exceedances we're looking for!

            'first find the maximum precipitation and in the mean time create the list of inuse values
            'now sort the list of sums by value in descending order
            InUse = New List(Of Integer)
            Dim Sums As New List(Of clsValueIndexPair)
            For i = 0 To Series.Values.Item(ModelParameter).Count - 1 - DurationTimesteps
                Sums.Add(New clsValueIndexPair(Series.getWindowSum(ModelParameter, i, DurationTimesteps), i))
            Next

            For i = 0 To Series.Values.Item(ModelParameter).Count - 1
                InUse.Add(0)
            Next

            'sort the precipitation sums in descending order
            'then make the corresponding index numbers(a lot!) faster accessible by dumping them in a new list
            Dim Sorted = From entry In Sums Order By entry.Value Descending
            Dim SortedIdx As New List(Of Long)
            For Each myEntry As clsValueIndexPair In Sorted
                SortedIdx.Add(myEntry.Index)
            Next
            Sorted = Nothing

            'now find the threshold above whicht the requested number of exceedances per year occur, given the boundary conditions given (time between events)
            While Not Done
                curIdx += 1

                'find the timestep index for the currently found event and find out whether this event meets the critera regarding minimum distance from other events or overlapping other events
                ts = SortedIdx(curIdx)                        'starting timestep for the currenly investigated event
                midts = ts + (DurationTimesteps - 1) / 2               'center timestep for the currently investigated event
                startts = ts - MinTimestepsBetweenEvents
                endts = ts + DurationTimesteps - 1 + MinTimestepsBetweenEvents
                IsInUse = getInUse(startts, endts)

                If Not Season.DateInSeason(Series.Dates(midts)) Then IsInUse = True 'only process if the event date is in the requested season

                'new event found!
                If IsInUse = False Then
                    n += 1

                    'update progress bar
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", n, nTarget, False)

                    newEvent = New clsTimeSeriesEvent(Me.Setup, Me.Series, Me.Season, Me)
                    newEvent.StartTs = ts

                    'calculate the hydrological and meteorological season this event falls in
                    newEvent.MeteoSeason = Me.Setup.GeneralFunctions.MeteorologischSeizoen(Series.Dates(midts))
                    newEvent.MeteoHalfYear = Me.Setup.GeneralFunctions.MeteorologischHalfJaar(Series.Dates(midts))
                    newEvent.HydroHalfYear = Me.Setup.GeneralFunctions.HydrologischHalfJaar(Series.Dates(midts))

                    For i = ts To ts + DurationTimesteps - 1
                        InUse(i) = 1
                        newRecord = New clsTimeTableRecord
                        newRecord.Datum = Series.Dates.Item(i)

                        'start by adding the value for each of the modelparameters present in the series
                        For Each myModelpar As GeneralFunctions.enmModelParameter In Series.Values.Keys
                            newRecord.SetValue(myModelpar, Series.Values(myModelpar)(i))
                        Next

                        'add the record to the event
                        If Not newEvent.TimeTable.Records.ContainsKey(newRecord.Datum) Then
                            newEvent.TimeTable.Records.Add(newRecord.Datum, newRecord)
                        Else
                            Me.Setup.Log.AddWarning("Multiple instances of date " & newRecord.Datum & " in data series.")
                        End If
                    Next
                    newEvent.CalculateSum(ModelParameter)
                    If CalcSTOWAPatterns Then newEvent.AnalyzePattern()
                    POTEvents.Events.Add(n, newEvent)
                End If
                If n = nTarget OrElse curIdx = Sums.Count - 1 Then Done = True
            End While

            'Dim InuseStr As String = InUse(0)
            'For i = 1 To InUse.Count - 1
            '    InuseStr &= "," & InUse(i)
            'Next
            'Debug.Print("inuse:" & InuseStr)

            'finally calculate the plotting positions for each event sum and place them in a datatable (for later binding with the charts object)
            POTEvents.CalculatePlottingPositions(PotFrequency)

            Me.Setup.GeneralFunctions.UpdateProgressBar("POT-events complete.", 0, 10)

            InUse = Nothing
            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in sub calculatePOTEvents of class clsRainfallSeries.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function GetPOTExceedanceTable() As DataTable
        Return POTEvents.PlottingPositions
    End Function

    Public Function WriteCSV() As Boolean
        'This function writes the original timeseries to a csv-file, but enhanced with event index number and event precipitation sum
        Dim Idx As Long, i As Long, Strings() As String
        Dim myEvent As clsTimeSeriesEvent
        Try
            'first fill the new array with the records we know contain an event
            ReDim Strings(Series.Dates.Count - 1)
            For Idx = 0 To POTEvents.Events.Values.Count - 1
                myEvent = POTEvents.Events.Values(Idx)
                For i = myEvent.StartTs To myEvent.StartTs + DurationTimesteps - 1
                    Strings(i) = Format(Series.Dates(i), "yyyy/MM/dd HH:mm") & "," & Me.Setup.GeneralFunctions.MeteorologischSeizoen(Series.Dates(i)).ToString & "," & Series.Values(GeneralFunctions.enmModelParameter.precipitation)(i) & "," & Idx + 1 & "," & myEvent.Sum & "," & myEvent.MeteoSeason.ToString & "," & myEvent.MeteoHalfYear.ToString & "," & myEvent.HydroHalfYear.ToString
                Next
            Next

            'now add the strings that are still empty
            For i = 0 To Series.Dates.Count - 1
                If Strings(i) = "" Then
                    Strings(i) = Format(Series.Dates(i), "yyyy/MM/dd hh:mm") & "," & Me.Setup.GeneralFunctions.MeteorologischSeizoen(Series.Dates(i)).ToString & "," & Series.Values(GeneralFunctions.enmModelParameter.precipitation)(i)
                End If
            Next

            Using csvWriter As New StreamWriter(Me.Setup.Settings.ExportDirRoot & "\" & Series.Name & "_" & Season.Season.ToString & "_" & DurationHours & "h.csv")
                csvWriter.WriteLine("Date,Season,Value,EventIdx,EventSum,Meteo Season Event,Meteo Halfyear Event,Hydro Halfyear Event")
                For i = 0 To Series.Dates.Count - 1
                    csvWriter.WriteLine(Strings(i))
                Next
            End Using
            Strings = Nothing

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
