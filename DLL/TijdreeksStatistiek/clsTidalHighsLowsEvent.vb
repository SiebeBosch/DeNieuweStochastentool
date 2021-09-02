Imports STOCHLIB.General

Public Class clsTidalHighsLowsEvent

    'the actual values of this event (high and low water levels)
    Public Values As New Dictionary(Of Long, clsTidalHighLow)
    Public ElevationClasses As New List(Of clsTidalElevatedClass)

    'derived collections of events
    Public TidalHighLowEvents As Dictionary(Of Long, clsTidalHighsLowsEvent) 'a series of subevents, derived from the original series with a given duration

    Public LowestLow As Double
    Public HighestLow As Double
    Public LowestHigh As Double
    Public HighestHigh As Double

    'tidal statistics for this event
    Dim AvgLow As Double
    Dim AvgHigh As Double
    Dim AvgLevel As Double
    Dim AvgAmplitude As Double

    Public AmplitudeClassified As Boolean               'keeps track if the current event has already been assigned to an amplitude class
    Public PercentileClassified As Boolean              'keeps track if the current event has already been assigned to an elevation class
    Public SequentialOccurancesClassified As Boolean    'keeps track if the current event has already been assigned to a sequentialoccurrance class

    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup)
        Values = New Dictionary(Of Long, clsTidalHighLow)
        Setup = mySetup
    End Sub

    Public Function CountSequentialOccurrances(ByVal lBound As Double, ByVal uBound As Double, ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, Optional ByVal StartSearchIdx As Long = -1, Optional ByVal EndSearchIdx As Long = -1) As Long
        Dim i As Long, j As Long = -1, val As Double
        Dim n As Long, nMax As Long

        If StartSearchIdx < 0 Then StartSearchIdx = 0
        If EndSearchIdx < 0 Then EndSearchIdx = Values.Count - 1

        'first place the tidal values inside (the last part of) this event in an array
        Dim Vals(EndSearchIdx - StartSearchIdx) As Double
        For i = StartSearchIdx To EndSearchIdx
            j += 1
            Select Case TidalComponent
                Case Is = GeneralFunctions.enmTidalComponent.VerhoogdLaagwater
                    Vals(j) = Values.Values(i).Low
                Case Is = GeneralFunctions.enmTidalComponent.VerhoogdeMiddenstand
                    Vals(j) = (Values.Values(i).Low + Values.Values(i).High) / 2
                Case Is = GeneralFunctions.enmTidalComponent.VerhoogdHoogwater
                    Vals(j) = Values.Values(i).High
            End Select
        Next

        'then count the max number of sequential exceedances of the given level
        For Each val In Vals
            If val >= lBound Then
                n += 1
                If n > nMax Then nMax = n
            Else
                n = 0
            End If
        Next
        Return nMax
    End Function

    Public Function GetHighestElevation(ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, Optional ByVal startIdx As Long = -1, Optional ByVal endIdx As Long = -1) As Double
        Dim i As Long, curVal As Double, maxVal As Double = -9999999999

        If startIdx < 0 Then startIdx = 0
        If endIdx < 0 Then endIdx = Values.Values.Count - 1

        Select Case TidalComponent
            Case Is = GeneralFunctions.enmTidalComponent.VerhoogdLaagwater
                For i = startIdx To endIdx
                    curVal = Values.Values(i).Low
                    If curVal > maxVal Then maxVal = curVal
                Next
            Case Is = GeneralFunctions.enmTidalComponent.VerhoogdeMiddenstand
                For i = startIdx To endIdx
                    curVal = (Values.Values(i).High + Values.Values(i).Low) / 2
                    If curVal > maxVal Then maxVal = curVal
                Next
            Case Is = GeneralFunctions.enmTidalComponent.VerhoogdHoogwater
                For i = startIdx To endIdx
                    curVal = Values.Values(i).High
                    If curVal > maxVal Then maxVal = curVal
                Next
        End Select
        Return maxVal
    End Function

    Public Sub CalcStats()
        Dim SumHigh As Double
        Dim SumLow As Double
        Dim i As Long
        Dim Tide As clsTidalHighLow

        'calculates some statistics for the current event with tides e.g. lowest high tide, highest low tide
        LowestLow = 9999999
        HighestLow = -9999999
        LowestHigh = 9999999
        HighestHigh = -9999999

        For i = 0 To Values.Count - 1
            Tide = Values.Values(i)
            If Tide.Low > HighestLow Then HighestLow = Tide.Low
            If Tide.Low < LowestLow Then LowestLow = Tide.Low
            If Tide.High > HighestHigh Then HighestHigh = Tide.High
            If Tide.High < LowestHigh Then LowestHigh = Tide.High
            SumHigh += Tide.High
            SumLow += Tide.Low
        Next

        AvgHigh = SumHigh / Values.Count
        AvgLow = SumLow / Values.Count
        AvgLevel = ((SumHigh + SumLow) / 2) / Values.Count
        AvgAmplitude = (SumHigh - SumLow) / Values.Count

    End Sub

    Public Function calcAverageAmplitude() As Double
        Dim mySum As Double
        If Values.Count > 0 Then
            For Each myHighLow As clsTidalHighLow In Values.Values
                mySum += (myHighLow.High - myHighLow.Low)
            Next
            Return mySum / Values.Count
        Else
            Return 0
        End If
    End Function

    Public Function getAmplitudeByPercentile(ByVal myPercentile As Double) As Double
        'this function returns the amplitude size belonging to a given percentile
        '0 = smallest amplitude, 1 = largest amplitude

        'create an array and fill it with all observed amplitudes
        Dim Amplitudes(Values.Count - 1) As Double, i As Long
        For i = 0 To Values.Count - 1
            Amplitudes(i) = Values.Values(i).Amplitude
        Next

        Return Me.Setup.GeneralFunctions.Percentile(Amplitudes, myPercentile)
    End Function

    Public Function CollectAllLows() As Double()
        Dim Lows(Values.Count - 1) As Double
        Dim i As Long
        For i = 0 To Values.Count - 1
            Lows(i) = Values(i).Low
        Next
        Return Lows
    End Function

    Public Function CalcAmplitudes() As Boolean
        Try
            'first create an array of the amplitudes
            Dim Amplitudes(Values.Count - 1) As Double, i As Long = -1
            For Each myTide As clsTidalHighLow In Values.Values
                i += 1
                Amplitudes(i) = myTide.High - myTide.Low
                myTide.Amplitude = Amplitudes(i)
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function BuildEventsFromTides(ByVal nTidesPerDuration As Long, ByVal SearchPercentage As Integer) As Boolean
        Dim n As Integer, i As Long, EventNum As Long, myEvent As clsTidalHighsLowsEvent
        Dim startIdx As Long, endIdx As Long 'the search timespan inside each event
        Dim nSearch As Long, nSkip As Long

        'specify the start and end index number for the search timespan inside each event
        'this is meant for searching elevations in only the last x % of the duration
        nSearch = Me.Setup.GeneralFunctions.RoundUD(SearchPercentage / 100 * nTidesPerDuration, 0, True)  'calculate the number of events that are included in the search period. We'll round up
        nSkip = nTidesPerDuration - nSearch
        startIdx = Math.Max(0, nSkip)
        endIdx = nTidesPerDuration - 1

        'this function splits the series of highs and lows into different 'events' of a given duration
        TidalHighLowEvents = New Dictionary(Of Long, clsTidalHighsLowsEvent)
        myEvent = New clsTidalHighsLowsEvent(Me.Setup)
        For i = 0 To Values.Count - 1
            n += 1
            If n < nTidesPerDuration Then
                myEvent.Values.Add(i, Values.Values(i))
            Else
                'conclude the current event and ad it to the collection
                myEvent.Values.Add(i, Values.Values(i))
                EventNum += 1
                TidalHighLowEvents.Add(EventNum, myEvent)

                'and initialize a new one
                n = 0
                myEvent = New clsTidalHighsLowsEvent(Me.Setup)
            End If
        Next
        Return True
    End Function

    Public Function CalcStatsForEvents() As Boolean
        'finally calculate some stats for all series we've just created (highest low, lowest high etc.)
        For Each myEvent As clsTidalHighsLowsEvent In TidalHighLowEvents.Values
            myEvent.CalcStats()
        Next
        Return True
    End Function


    Public Function getTidalElevationClass(ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, ByVal startSearchIdx As Long, ByVal endSearchIdx As Long, ByRef TidalElevatedClasses As clsTidalElevatedClasses) As clsTidalElevatedClass
        'siebe bosch, 7-2-2015
        'this routine decides in which tidal elevation class the event belongs
        'it does not necessarily search for the highest elevation class that is being touched by the event
        'since multiple sequential elevations appear to be more limiting on discharge than single extremes
        'schematically an event in the elevation classes 01034002 would be assigned to class 3 in stead of 4

        Dim ClassNumbers As New List(Of Integer)
        Dim HighestClass As Integer, i As Integer
        Dim curClass As Integer, nextClass As Integer

        Dim OneLowerFound As Boolean    'whether the highest was followed by an occurrance in one class lower or vice versa

        'set the maximum number of waves we can search. This is dependent on the duration we're allowed to search
        'if the duration allows it, we start by classifying 3 sequential elevations and then step down to 2, and 1.
        Dim SearchTidalWaves As Integer = Math.Min(3, endSearchIdx - startSearchIdx + 1)

        'fill the array with the index numbers of each elevationclass
        ClassNumbers = getElevationClassSequence(TidalComponent, startSearchIdx, endSearchIdx, TidalElevatedClasses, SearchTidalWaves)
        HighestClass = ClassNumbers.Max

        '--------------------------------------------------------------------------------------------------------
        '    OLD METHOD: SIMPLY RETURN THE NUMBER OF THE HIGHEST CLASS INSIDE OF WHICH A TIDAL WAVE WAS OBSERVED
        '--------------------------------------------------------------------------------------------------------
        'Return TidalElevatedClasses.Classes.Values(HighestClass)

        '--------------------------------------------------------------------------------------------------------
        '    NEW METHOD: IF AN ADJACENT WAVE LIES IN A CLASS ONE LOWER THAN THE HIGHEST, ASSIGN 2X THE LOWER CLASS
        '--------------------------------------------------------------------------------------------------------

        If HighestClass > 1 Then
            For i = 0 To ClassNumbers.Count - 2
                curClass = ClassNumbers.Item(i)
                nextClass = ClassNumbers.Item(i + 1)

                If curClass = HighestClass AndAlso nextClass = HighestClass Then
                    Return TidalElevatedClasses.Classes.Values(HighestClass)
                End If

                If curClass = HighestClass AndAlso nextClass = HighestClass - 1 Then OneLowerFound = True
                If curClass = HighestClass - 1 AndAlso nextClass = HighestClass Then OneLowerFound = True
            Next
        End If

        If OneLowerFound Then
            'we prefer using a double occurrance inside one lower elevation class over the single occurrance in the highest
            Return TidalElevatedClasses.Classes.Values(HighestClass - 1)
        Else
            'the maximum is a lonely boy so return it as is
            Return TidalElevatedClasses.Classes.Values(HighestClass)
        End If

    End Function

    Public Function getElevationClassSequence(ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, ByVal startSearchIdx As Long, ByVal endSearchIdx As Long, ByRef tidalElevationClasses As clsTidalElevatedClasses, ByVal nTidalWaves As Integer) As List(Of Integer)
        Dim i As Integer, j As Integer, sum As Long, maxSum As Long, tsMax As Integer = startSearchIdx
        Dim mySequence As New List(Of Integer) 'a list of the elevation class numbers in the order they appear
        'gets from the tidal waves inside an event the heaviest sequence of nTidalWaves
        'it does so by calculating the sum of the elevation class over the sequence of nTidalWaves

        'search inside the event which sequence of tidal waves is the heaviest
        For i = startSearchIdx To endSearchIdx - nTidalWaves + 1
            sum = 0
            For j = i To i + nTidalWaves - 1
                sum += getElevationClass(TidalComponent, j, tidalElevationClasses)
            Next
            If sum > maxSum Then
                maxSum = sum
                tsMax = i
            End If
        Next

        'highest sequence found. Create the list and return it
        For i = tsMax To tsMax + nTidalWaves - 1
            mySequence.Add(getElevationClass(TidalComponent, i, tidalElevationClasses))
        Next

        Return mySequence

    End Function

    Public Function getElevationClass(ByVal tidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, ByVal searchidx As Integer, ByRef tidalElevationClasses As clsTidalElevatedClasses) As Integer
        Dim myTidalClass As clsTidalElevatedClass
        Dim searchValue As Double
        Dim i As Integer

        'finds for the current tidal wave to which elevation class it belongs
        Select Case tidalComponent
            Case Is = GeneralFunctions.enmTidalComponent.VerhoogdLaagwater
                searchValue = Values.Values(searchidx).Low
            Case Is = GeneralFunctions.enmTidalComponent.VerhoogdeMiddenstand
                searchValue = (Values.Values(searchidx).Low + Values.Values(searchidx).High) / 2
            Case Is = GeneralFunctions.enmTidalComponent.VerhoogdHoogwater
                searchValue = Values.Values(searchidx).High
        End Select

        For i = 0 To tidalElevationClasses.Classes.Count - 1
            myTidalClass = tidalElevationClasses.Classes.Values(i)
            If myTidalClass.lBoundVal <= searchValue AndAlso searchValue <= myTidalClass.uBoundVal Then
                Return i
            End If
        Next

        'if we end up here, no class was found. Return the lowest or highest by default
        If searchValue < tidalElevationClasses.Classes.Values(0).lBoundVal Then Return 0
        If searchValue > tidalElevationClasses.Classes.Values(tidalElevationClasses.Classes.Count - 1).uBoundVal Then Return tidalElevationClasses.Classes.Count - 1

    End Function

End Class
