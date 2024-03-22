Imports DocumentFormat.OpenXml.Spreadsheet
Imports STOCHLIB.General
Public Class clsRainfallEvents
    Private Setup As clsSetup

    Public Events As New Dictionary(Of Integer, clsTimeSeriesEvent)

    'for the classification of parameters by percentile it is possible to use multiple parameters
    'e.g. primary parameter is the HBV parameter lz (lower zone) and secondary is uz (upper zone) with a side parameter sm (soil moisture)
    Public PercentileClassifications As clspercentileClassifications ' Dictionary(Of String, List(Of clsPercentileClass))

    Public PlottingPositions As DataTable

    Private Series As clsModelTimeSeries     'the underlying timeseries with precipitation
    Private Season As clsSeason
    Private Duration As clsDuration

    Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsModelTimeSeries, ByRef mySeason As clsSeason, ByRef myDuration As clsDuration)
        Setup = mySetup
        Series = mySeries
        Season = mySeason
        Duration = myDuration
        PercentileClassifications = New clspercentileClassifications(Setup)
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsModelTimeSeries, ByRef mySeason As clsSeason)
        Setup = mySetup
        Series = mySeries
        Season = mySeason
    End Sub

    Public Sub Add(ByVal Idx As Integer, ByVal maxEvent As clsTimeSeriesEvent)
        Events.Add(Idx, maxEvent)
    End Sub

    Public Function CalculateParameterClassification(ByRef ModelParameters As clsModelParameterClass, TimestepStatistic As GeneralFunctions.enmTimestepStatistic, ByRef PercentileClassesTemplate As clsPercentileClasses) As Boolean

        'this function classifies the events by percentile classes for a given set of parameters
        'start with the primary parameter

        Try
            For Each myPercentileClass As clsPercentileClass In PercentileClassesTemplate.Classes.Values

                Dim newClass As New clsPercentileClass
                newClass.Name = myPercentileClass.Name
                newClass.Parameter = ModelParameters.PrimaryParameter
                newClass.LBoundPercentile = myPercentileClass.LBoundPercentile
                newClass.UboundPercentile = myPercentileClass.UboundPercentile
                newClass.RepresentativePercentile = myPercentileClass.RepresentativePercentile

                'read the parameter values from the events
                Dim ParameterValues As New List(Of Double)
                For Each myEvent As clsTimeSeriesEvent In Events.Values
                    ParameterValues.Add(myEvent.GetParameterValue(ModelParameters.PrimaryParameter, TimestepStatistic))
                Next

                'now calculate the percentile values for our primary parameter
                newClass.RepresentativeValue = Setup.GeneralFunctions.PercentileFromList(ParameterValues, myPercentileClass.RepresentativePercentile)
                newClass.LBoundValue = Setup.GeneralFunctions.PercentileFromList(ParameterValues, myPercentileClass.LBoundPercentile)
                newClass.UboundValue = Setup.GeneralFunctions.PercentileFromList(ParameterValues, myPercentileClass.UboundPercentile)

                'and add the index numbers of the events that fall within this percentile class to the list
                For i = 0 To Events.Count - 1
                    If ParameterValues(i) >= newClass.LBoundValue And ParameterValues(i) <= newClass.UboundValue Then
                        newClass.EventIdxNums.Add(i)
                    End If
                Next

                'now for the side parameters within this class we calculate the median over all events that fall within this class
                For Each mySideParameter As GeneralFunctions.enmModelParameter In ModelParameters.PrimarySideParameters
                    Dim SideParameterValues As New List(Of Double)
                    For i = 0 To newClass.EventIdxNums.Count - 1
                        SideParameterValues.Add(Events.Values(newClass.EventIdxNums(i)).GetParameterValue(mySideParameter, TimestepStatistic))
                    Next
                    'assign the median value to the class
                    newClass.SideParameterValues.Add(mySideParameter, Setup.GeneralFunctions.PercentileFromList(SideParameterValues, 0.5))
                Next

                'if we have a secondary parameter, we do the same for this parameter
                If ModelParameters.SecondaryParameter <> GeneralFunctions.enmModelParameter.none Then

                    'also for the secondary parameter we must iterate through each percentileClass
                    For Each myPercentileClass2 As clsPercentileClass In PercentileClassesTemplate.Classes.Values
                        Dim newClass2 As New clsPercentileClass
                        newClass2.Name = myPercentileClass2.Name
                        newClass2.Parameter = ModelParameters.SecondaryParameter
                        newClass2.LBoundPercentile = myPercentileClass2.LBoundPercentile
                        newClass2.UboundPercentile = myPercentileClass2.UboundPercentile
                        newClass2.RepresentativePercentile = myPercentileClass2.RepresentativePercentile

                        'read the parameter values from the events
                        'we'll only loop through the events that fall within our primary percentile class
                        Dim ParameterValues2 As New List(Of Double)
                        For i = 0 To newClass.EventIdxNums.Count - 1
                            ParameterValues2.Add(Events.Values(newClass.EventIdxNums(i)).GetParameterValue(ModelParameters.SecondaryParameter, TimestepStatistic))
                        Next

                        'now calculate the percentile values for our secondary parameter
                        newClass2.RepresentativeValue = Setup.GeneralFunctions.PercentileFromList(ParameterValues2, myPercentileClass2.RepresentativePercentile)
                        newClass2.LBoundValue = Setup.GeneralFunctions.PercentileFromList(ParameterValues2, myPercentileClass2.LBoundPercentile)
                        newClass2.UboundValue = Setup.GeneralFunctions.PercentileFromList(ParameterValues2, myPercentileClass2.UboundPercentile)

                        'and add the index numbers of the events that fall within this percentile class to the list
                        For i = 0 To newClass.EventIdxNums.Count - 1
                            If ParameterValues2(i) >= newClass2.LBoundValue And ParameterValues2(i) <= newClass2.UboundValue Then
                                newClass2.EventIdxNums.Add(i)
                            End If
                        Next

                        'now for the side parameters within this class we calculate the median over all events that fall within this class
                        For Each mySideParameter As GeneralFunctions.enmModelParameter In ModelParameters.SecondarySideParameters
                            Dim SideParameterValues2 As New List(Of Double)

                            For i = 0 To newClass2.EventIdxNums.Count - 1
                                SideParameterValues2.Add(Events.Values(newClass2.EventIdxNums(i)).GetParameterValue(mySideParameter, TimestepStatistic))
                            Next
                            newClass2.SideParameterValues.Add(mySideParameter, Setup.GeneralFunctions.PercentileFromList(SideParameterValues2, 0.5))
                        Next

                        PercentileClassifications.Add(New List(Of clsPercentileClass) From {newClass, newClass2})

                    Next

                Else
                    'if we don't have a secondary parameter, we just add the primary parameter
                    PercentileClassifications.Add(New List(Of clsPercentileClass) From {newClass})
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in clsRainfallEvents.CalculateParameterClassification: " & ex.Message)
            Return False
        End Try

    End Function


    Public Function getAdd(ByVal myIdx As Integer) As clsTimeSeriesEvent
        Dim myEvent As clsTimeSeriesEvent
        If Not Events.ContainsKey(myIdx) Then
            myEvent = New clsTimeSeriesEvent(Me.Setup, Me.Series, Me.Season, Me.Duration)
            Events.Add(myIdx, myEvent)
        End If
        Return Events.Item(myIdx)
    End Function

    Public Function CountByPatternType(ByVal Patroon As STOCHLIB.GeneralFunctions.enmNeerslagPatroon, ByVal PartOfSeason As STOCHLIB.GeneralFunctions.enmSeason) As Long
        Dim n As Long
        For Each myEvent As clsTimeSeriesEvent In Events.Values
            If myEvent.Pattern = GeneralFunctions.enmNeerslagPatroon.ONGECLASSIFICEERD Then myEvent.AnalyzePattern()
            If myEvent.Pattern = GeneralFunctions.enmNeerslagPatroon.ONGECLASSIFICEERD Then
                Me.Setup.Log.AddError("Een neerslagpatroon kon niet worden geclassificeerd.")
            Else
                If myEvent.Pattern = Patroon AndAlso myEvent.InSeason(PartOfSeason) Then n += 1
            End If
        Next

        Return n
    End Function

    Public Function TimestepPartOfEvents(ByVal ts As Long) As Boolean
        For Each myEvent As clsTimeSeriesEvent In Events.Values
            If myEvent.TimestepPartOfEvent(ts) Then Return True
        Next
        Return False
    End Function

    Public Function GetStartingTimeSteps() As List(Of Long)
        Dim myList As New List(Of Long)
        For Each myEvent As clsTimeSeriesEvent In Events.Values
            myList.Add(myEvent.StartTs)
        Next
        Return myList
    End Function

    Private Sub SortByDescendingVolume()
        'this routine sorts the events by precipitation sum in ascending order
        Dim i As Long = -1
        Dim Sorted = From entry In Events Order By entry.Value.Sum Descending
        Events = New Dictionary(Of Integer, clsTimeSeriesEvent)
        For Each entry In Sorted
            i += 1
            Events.Add(i, entry.Value)
        Next
        Sorted = Nothing
    End Sub

    Public Sub CalculatePlottingPositions(ByVal AnnualFrequency As Integer)
        'calculates the plotting positions of the event sums
        Dim P As Double, F As Double

        'if yearly maxima are plotted then AnnualFrequency = 1. in that case:
        '- the plotting positions are computed by: P = (i - 0.3)/(n + 0.4)
        '- the exceedance frequencies are computed by: F = -LN(1-P)

        'else if the annual frequency > 1, we're dealing with POT-events. in that case:
        '- the plotting positions are computed by: P = (i - 0.3)/(n + 0.4)
        '- the exceedance frequencies are computed by: F = P * AnnualFrequency

        'first the routine sorts the results by descending volume
        'then it calculates for each event its plotting position
        'important: assumes that the events have been sorted in DESCENDING order
        Dim H As Double, i As Long, n As Long = Series.getTimeSpanYears * Events.Count

        PlottingPositions = New DataTable
        PlottingPositions.Columns.Add("ReturnPeriod", GetType(Double))
        PlottingPositions.Columns.Add("Value", GetType(Double))

        Call SortByDescendingVolume()
        For Each myEvent As clsTimeSeriesEvent In Events.Values
            i += 1

            If AnnualFrequency = 1 Then
                P = (i - 0.3) / (Events.Count + 0.4) 'plotting position
                F = -Math.Log(1 - P) 'exceedance frequency in times per year
                H = 1 / F 'return period
                PlottingPositions.Rows.Add(H, myEvent.Sum) 'add the results to the datatable
            Else
                P = (i - 0.3) / (Events.Count + 0.4) 'plotting position
                F = P * AnnualFrequency 'exceedance frequency in times per year
                H = 1 / F 'return period
                PlottingPositions.Rows.Add(H, myEvent.Sum) 'add the results to the datatable
            End If

        Next

    End Sub

    Public Function getValue(ByVal ReturnPeriod As Double) As Double
        Return Me.Setup.GeneralFunctions.InterpolateFromDataTable(PlottingPositions, ReturnPeriod, 0, 1)
    End Function

End Class
