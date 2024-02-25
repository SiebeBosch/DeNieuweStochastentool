Imports STOCHLIB.General

Public Class clsKNMI14Scenarios

    Private Setup As clsSetup
    Private Series As clsModelTimeSeries

    Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsModelTimeSeries)
        Setup = mySetup
        Series = mySeries
    End Sub

    Public Sub Apply(ByVal myScenario As String)
        Dim ScenarioName As STOCHLIB.GeneralFunctions.enmKNMI14Scenario = ScenarioNameFromString(myScenario)
        Select Case ScenarioName
            Case Is = GeneralFunctions.enmKNMI14Scenario.GL2050
                Call GL2050()
            Case Is = GeneralFunctions.enmKNMI14Scenario.GH2050
                Call GH2050()
            Case Is = GeneralFunctions.enmKNMI14Scenario.WL2050
                Call WL2050()
            Case Is = GeneralFunctions.enmKNMI14Scenario.WH2050
                Call WH2050()
        End Select

        'since the time series has been adjusted, we'll need to update its name
        Series.Name &= "_" & myScenario

    End Sub

    Public Function ScenarioNameFromString(ByVal myScenario As String) As STOCHLIB.GeneralFunctions.enmKNMI14Scenario
        Select Case myScenario.Trim.ToUpper
            Case Is = "GL2050"
                Return GeneralFunctions.enmKNMI14Scenario.GL2050
            Case Is = "GH2050"
                Return GeneralFunctions.enmKNMI14Scenario.GH2050
            Case Is = "WL2050"
                Return GeneralFunctions.enmKNMI14Scenario.WL2050
            Case Is = "WH2050"
                Return GeneralFunctions.enmKNMI14Scenario.WH2050
            Case Is = "GL2085"
                Return GeneralFunctions.enmKNMI14Scenario.GL2085
            Case Is = "GH2085"
                Return GeneralFunctions.enmKNMI14Scenario.GH2085
            Case Is = "WL2085"
                Return GeneralFunctions.enmKNMI14Scenario.WL2085
            Case Is = "WH2085"
                Return GeneralFunctions.enmKNMI14Scenario.WH2085
        End Select
    End Function

    Public Sub GL2050()
        Call SUMMER(GeneralFunctions.enmModelParameter.precipitation, 1, 1.025, 1.025, 1.005)
        Call WINTER(GeneralFunctions.enmModelParameter.precipitation, 1.014, 1.031, 1.054, 1.054, 0.997)
        Call SPRING(GeneralFunctions.enmModelParameter.precipitation, 1.0437)
        Call AUTUMN(GeneralFunctions.enmModelParameter.precipitation, 1.07)
    End Sub
    Public Sub GH2050()
        Call SUMMER(GeneralFunctions.enmModelParameter.precipitation, 0.81, 1, 1.03, 0.946)
        Call WINTER(GeneralFunctions.enmModelParameter.precipitation, 1.125, 1.0712, 1, 1.0723, 1.014)
        Call SPRING(GeneralFunctions.enmModelParameter.precipitation, 1.0216)
        Call AUTUMN(GeneralFunctions.enmModelParameter.precipitation, 1.08)
    End Sub
    Public Sub WL2050()
        Call SUMMER(GeneralFunctions.enmModelParameter.precipitation, 1, 1.028, 1.032, 1.007)
        Call WINTER(GeneralFunctions.enmModelParameter.precipitation, 1.12, 1.07, 1, 1.093, 0.996)
        Call SPRING(GeneralFunctions.enmModelParameter.precipitation, 1.1077)
        Call AUTUMN(GeneralFunctions.enmModelParameter.precipitation, 1.03)
    End Sub
    Public Sub WH2050()
        Call SUMMER(GeneralFunctions.enmModelParameter.precipitation, 0.703, 0.98, 1.105, 0.9)
        Call WINTER(GeneralFunctions.enmModelParameter.precipitation, 1.325, 1.113, 1, 1.116, 1.0245)
        Call SPRING(GeneralFunctions.enmModelParameter.precipitation, 1.087)
        Call AUTUMN(GeneralFunctions.enmModelParameter.precipitation, 1.075)
    End Sub

    Public Sub SUMMER(ModelParameter As GeneralFunctions.enmModelParameter, ByVal Summer01mmVolumeFactor24H As Double, ByVal Summer20mmIncrease24H As Double, ByVal SummerT10VolumeFactor24H As Double, ByVal SummerWetDaysIncrease24H As Double)

        '----------------------------------------------------------------------------------------------------------------
        '           SUMMER SECTION FOR KNMI '14 CLIMATE SCENARIO 2050 GH
        '----------------------------------------------------------------------------------------------------------------
        Dim mySeason As clsSeason
        Dim myDuration As clsDuration
        Dim myLineHigh As clsLineDefinition
        Dim myLineLow As clsLineDefinition
        Dim myEvent As clsTimeSeriesEvent
        Dim mySum As clsDailyPrecipitationSum
        Dim myMultiplier As Double
        Dim ts As Long
        Dim nWetDays As Long

        'first we'll need some statistics for daily precipitation sum, so aggregate to daily volumes
        mySeason = Series.Seasons.Add(Me.Setup, Series, "summerquarter")
        mySeason.AggregateByDay(ModelParameter)
        nWetDays = CalcWetDays(mySeason)

        'now that we've met the required change in number of wet days, we can start changing the volumes
        myDuration = mySeason.AddDuration(24)
        myDuration.calculateYearMaxima()
        myDuration.AnnualMaxima.CalculatePlottingPositions(1)
        'myDuration.calculatePOTEvents(100, 0)                  'since we've figured out a better plotting position for annual maxima, the pot events are no longer needed
        'myDuration.POTEvents.CalculatePlottingPositions(100)

        'Now we have multiplication factors for two volumes, so define a linear relationship between the volume and multiplier
        Dim SummerT10Volume24H As Double = Me.Setup.GeneralFunctions.InterpolateFromDataTable(myDuration.AnnualMaxima.PlottingPositions, 10, 0, 1)

        'the events with high volumes (> 20 mm) we'll adjust from the collection of POT-events
        myLineHigh = New clsLineDefinition(Me.Setup)
        myLineHigh.Calculate(20, Summer20mmIncrease24H, SummerT10Volume24H, SummerT10VolumeFactor24H)
        For Each myEvent In myDuration.AnnualMaxima.Events.Values
            If myEvent.Sum >= 20 Then
                For ts = myEvent.StartTs To myEvent.StartTs + myDuration.DurationTimesteps - 1
                    myMultiplier = (myLineHigh.a * myEvent.Sum + myLineHigh.b)
                    Series.Values(ModelParameter)(ts) *= myMultiplier
                Next
                myEvent.CalculateSum(GeneralFunctions.enmModelParameter.precipitation)
            End If
        Next

        'the events with low volumes (<20 mm) we'll adjust from the collection of daily volumes
        myLineLow = New clsLineDefinition(Me.Setup)
        myLineLow.Calculate(0.1, Summer01mmVolumeFactor24H, 20, Summer20mmIncrease24H)
        For Each mySum In mySeason.DailySums.Values
            If mySum.Sum < 20 AndAlso mySum.Sum >= 0.1 Then
                myMultiplier = (myLineLow.a * mySum.Sum + myLineLow.b)
                For ts = mySum.StartTs To mySum.StartTs + 23                      'first adjust the original timeseries
                    Series.Values(ModelParameter)(ts) *= myMultiplier
                Next
                mySum.Sum *= myMultiplier                                         'finally adjust the daily sum
            End If
        Next

        'now that the main volumes have been adjusted we'll need to meet the number of wet days requirement
        'base the target number on the number of wet days in the original series
        'then recalculate nWetDays since they have chanced due to adjusted volumes
        Dim nTarget As Long = nWetDays * SummerWetDaysIncrease24H
        nWetDays = CalcWetDays(mySeason)

        If nTarget < nWetDays Then
            Call RemoveWetDays(ModelParameter, nWetDays - nTarget, mySeason)
        ElseIf nTarget > nWetDays Then
            Call AddWetDays(ModelParameter, nTarget - nWetDays, mySeason)
        End If

        'recalculate the return periods
        myDuration.calculateYearMaxima()
        myDuration.AnnualMaxima.CalculatePlottingPositions(1)

    End Sub
    Public Sub WINTER(ModelParameter As GeneralFunctions.enmModelParameter, ByVal Winter01mmFactor24H As Double, ByVal Winter10mmFactor24H As Double, ByVal WinterT01VolumeFactor240H As Double, ByVal WinterT10VolumeFactor240H As Double, ByVal WinterWetDaysIncrease24H As Double)

        '----------------------------------------------------------------------------------------------------------------
        '           WINTER SECTION FOR KNMI '14 CLIMATE SCENARIO 2050 GL
        '----------------------------------------------------------------------------------------------------------------
        Dim mySeason As clsSeason
        Dim myDuration As clsDuration
        Dim myLineHigh As clsLineDefinition
        Dim myLineLow As clsLineDefinition
        Dim myEvent As clsTimeSeriesEvent
        Dim mySum As clsDailyPrecipitationSum
        Dim myMultiplier As Double
        Dim ts As Long
        Dim nWetDays As Long

        'first we'll need some statistics for daily precipitation sum, so aggregate to daily volumes
        mySeason = Series.Seasons.Add(Me.Setup, Series, "winterquarter")
        mySeason.AggregateByDay(ModelParameter)
        nWetDays = CalcWetDays(mySeason)

        'now that we've met the required change in number of wet days, we can start changing the volumes
        'we'll start with the T10 volume in 240 hours. This has to be increased by 6%
        'we'll make up our own increase for the T1 volume in 240 hours. and iteratively adjust it until the 10mm/24H volume increase meets the expectations
        myDuration = mySeason.AddDuration(240)
        myDuration.calculateYearMaxima()
        myDuration.AnnualMaxima.CalculatePlottingPositions(1)
        'myDuration.calculatePOTEvents(100, 0)
        'myDuration.POTEvents.CalculatePlottingPositions(100)

        'since we only have one point to increase, we'll have to make up our own point 2 and later check whether the increase of the 10mm volume in 24 hour meets the expectations
        Dim WinterT01Volume240H As Double = myDuration.AnnualMaxima.getValue(0.1)
        Dim WinterT10Volume240H As Double = myDuration.AnnualMaxima.getValue(10)

        myLineHigh = New clsLineDefinition(Me.Setup)
        myLineHigh.Calculate(WinterT01Volume240H, WinterT01VolumeFactor240H, WinterT10Volume240H, WinterT10VolumeFactor240H)
        For Each myEvent In myDuration.AnnualMaxima.Events.Values
            If myEvent.Sum >= WinterT01Volume240H Then
                myMultiplier = (myLineHigh.a * myEvent.Sum + myLineHigh.b)
                For ts = myEvent.StartTs To myEvent.StartTs + myDuration.DurationTimesteps - 1
                    Series.Values(ModelParameter)(ts) *= myMultiplier
                Next
                myEvent.CalculateSum(GeneralFunctions.enmModelParameter.precipitation)
            End If
        Next

        'now adjust the more frequent volumes. We'll do this based on the daily volumes
        myLineLow = New clsLineDefinition(Me.Setup)
        myLineLow.Calculate(0.1, Winter01mmFactor24H, 10, Winter10mmFactor24H)
        For Each mySum In mySeason.DailySums.Values
            If mySum.Sum <= 10 AndAlso mySum.Sum >= 0.1 Then
                myMultiplier = (myLineLow.a * mySum.Sum + myLineLow.b)
                For ts = mySum.StartTs To mySum.StartTs + 23                      'first adjust the original timeseries
                    Series.Values(ModelParameter)(ts) *= myMultiplier
                Next
                mySum.Sum *= myMultiplier                                         'finally adjust the daily sum
            End If
        Next

        'now that the main volumes have been adjusted we'll need to meet the number of wet days requirement
        'base the target number on the number of wet days in the original series
        'then recalculate nWetDays since they have chanced due to adjusted volumes
        Dim nTarget As Long = nWetDays * WinterWetDaysIncrease24H
        nWetDays = CalcWetDays(mySeason)

        If nTarget < nWetDays Then
            Call RemoveWetDays(ModelParameter, nWetDays - nTarget, mySeason)
        ElseIf nTarget > nWetDays Then
            Call AddWetDays(ModelParameter, nTarget - nWetDays, mySeason)
        End If

        'recalculate the return periods
        myDuration.calculateYearMaxima()
        myDuration.AnnualMaxima.CalculatePlottingPositions(1)

    End Sub
    Public Sub SPRING(ModelParameter As GeneralFunctions.enmModelParameter, ByVal Factor As Double)

        '----------------------------------------------------------------------------------------------------------------
        '           SPRING SECTION FOR KNMI '14 CLIMATE SCENARIO 2050 GL
        '----------------------------------------------------------------------------------------------------------------
        Dim mySeason As clsSeason
        Dim ts As Long
        mySeason = Series.Seasons.Add(Me.Setup, Me.Series, "springquarter")

        'average spring volumes increase by 4.5%, however some winter and summer events partially overlap spring and have influence too
        For ts = 0 To Series.Dates.Count - 1
            If mySeason.DateInSeason(Series.Dates(ts)) Then
                Series.Values(ModelParameter)(ts) *= Factor
            End If
        Next

    End Sub
    Public Sub AUTUMN(ModelParameter As GeneralFunctions.enmModelParameter, ByVal Factor As Double)

        '----------------------------------------------------------------------------------------------------------------
        '           AUTUMN SECTION FOR KNMI '14 CLIMATE SCENARIO 2050 GL
        '----------------------------------------------------------------------------------------------------------------
        Dim mySeason As clsSeason
        Dim ts As Long
        mySeason = Series.Seasons.Add(Me.Setup, Me.Series, "autumnquarter")

        'average autumn volumes increase by 7%, however some winter and summer events partially overlap spring and have influence too
        For ts = 0 To Series.Dates.Count - 1
            If mySeason.DateInSeason(Series.Dates(ts)) Then
                Series.Values(ModelParameter)(ts) *= Factor
            End If
        Next

    End Sub

    Public Sub RemoveWetDays(ModelParameter As GeneralFunctions.enmModelParameter, nRemove As Long, ByRef mySeason As clsSeason)
        Dim i As Long, ts As Long, EndIdx As Long, StartIdx As Long
        Dim mySum As clsDailyPrecipitationSum

        'since the volumes are sorted in descending order, we can look up the last index
        For i = 0 To mySeason.DailySums.Values.Count
            mySum = mySeason.DailySums.Values(i)
            If mySum.Sum < 0.1 Then
                EndIdx = i - 1
                StartIdx = EndIdx - nRemove + 1
                Exit For
            End If
        Next

        For i = StartIdx To EndIdx
            mySum = mySeason.DailySums.Values(i)
            For ts = mySum.StartTs To mySum.StartTs + 23
                Series.Values(ModelParameter)(ts) = 0
            Next
            mySum.Sum = 0
        Next

    End Sub
    Public Sub AddWetDays(ModelParameter As GeneralFunctions.enmModelParameter, ByVal nAdd As Long, ByRef mySeason As clsSeason)

        Dim i As Long, myIdx As Long, EndIdx As Long, StartIdx As Long
        Dim mySum As clsDailyPrecipitationSum

        'since the volumes are sorted in descending order, we can look up the start and end index
        For i = 0 To mySeason.DailySums.Values.Count
            mySum = mySeason.DailySums.Values(i)
            If mySum.Sum < 0.1 Then
                StartIdx = i
                EndIdx = mySeason.DailySums.Values.Count - 1
                Exit For
            End If
        Next

        For i = 1 To nAdd
            myIdx = Me.Setup.GeneralFunctions.GetRandom(StartIdx, EndIdx)
            mySum = mySeason.DailySums.Values(myIdx)
            Series.Values(ModelParameter)(mySum.StartTs) += 0.1
            mySum.Sum += 0.15
        Next

    End Sub
    Public Function CalcWetDays(ByRef mySeason As clsSeason)
        'calculates the number of wet days (>= 0.1 mm)
        'from the list of daily precipitation sums
        Dim i As Long, n As Long, mySum As clsDailyPrecipitationSum
        For i = 0 To mySeason.DailySums.Count - 1
            mySum = mySeason.DailySums.Values(i)
            If mySum.Sum >= 0.1 Then
                n += 1
            End If
        Next
        Return n
    End Function

End Class
