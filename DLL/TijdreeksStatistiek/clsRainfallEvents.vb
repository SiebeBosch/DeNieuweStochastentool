Imports STOCHLIB.General
Public Class clsRainfallEvents

  Public Events As New Dictionary(Of Integer, clsTimeSeriesEvent)
  Public PlottingPositions As DataTable

  Private Series As clsRainfallSeries     'the underlying timeseries with precipitation
  Private Season As clsSeason
  Private Duration As clsDuration
  Private Setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsRainfallSeries, ByRef mySeason As clsSeason, ByRef myDuration As clsDuration)
    Setup = mySetup
    Series = mySeries
    Season = mySeason
    Duration = myDuration
  End Sub

  Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsRainfallSeries, ByRef mySeason As clsSeason)
    Setup = mySetup
    Series = mySeries
    Season = mySeason
  End Sub

  Public Sub Add(ByVal Idx As Integer, ByVal maxEvent As clsTimeSeriesEvent)
    Events.Add(Idx, maxEvent)
  End Sub

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
