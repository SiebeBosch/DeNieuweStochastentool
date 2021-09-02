Imports STOCHLIB.General

Public Class clsTidalClassification
  Public ExcelPath As String
  Public MaxLows As Integer
  Public AmplitudeClasses As Integer
  Public ResponseTime As Integer
  Public TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent

  Public Percentiles As New Dictionary(Of Double, Double) 'key is the percentile, value is the elevation
  Dim ExcelFile As clsExcelBook
  Dim TidalTable As clsSobekTable

  Private Setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup
    Percentiles = New Dictionary(Of Double, Double)
  End Sub

  Public Function Analyze() As Boolean
    Try
      'the csv file has already been read and is now present in a timeseries
      For Each myReeks As clsTidalTimeSeries In Me.Setup.TijdreeksStatistiek.GetijdenReeksen.Values
        myReeks.CalcTidalMinMax(True)
        'calculate some stats from the computed high and low tides
        myReeks.TidalHighsLowsEvent.CalcStats()
      Next
    Catch ex As Exception
      Me.Setup.Log.AddError(ex.Message)
      Return False
    End Try
  End Function

  Public Function Classify(ByRef AmplitudeClassesGrid As Windows.Forms.DataGridView, ByRef ElevationClassesGrid As Windows.Forms.DataGridView, ByVal Duration As Long, ByVal Uitloop As Long, ByVal ApplicationPercentage As Integer, ByVal AdvancedTidalClassification As Boolean) As Boolean

    Try
      Dim nTidesPerDuration As Long
      Dim i As Integer
      Dim n As Integer = Setup.TijdreeksStatistiek.GetijdenReeksen.Count

      Me.Setup.GeneralFunctions.UpdateProgressBar("Classifying tidal series...", 0, 10)

      'compute how many high and low tides each duration will contain
      nTidesPerDuration = Me.Setup.GeneralFunctions.RoundUD(Duration / 12.5, 0, False)

      'compute the amplitudes and subdivide the series of highs/lows into 'events'
      For Each myReeks As clsTidalTimeSeries In Me.Setup.TijdreeksStatistiek.GetijdenReeksen.Values
        i += 1
        Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)

        'first subdivide the timeseries of highs and lows into separate events with a given duration
        If Not myReeks.TidalHighsLowsEvent.BuildEventsFromTides(nTidesPerDuration, ApplicationPercentage) Then Throw New Exception("Error splitting the series of tidal highs/lows into separate events.")

        'then compute statistical characteristics for each event
        If Not myReeks.TidalHighsLowsEvent.CalcStatsForEvents() Then Throw New Exception("Could not derive statistics for events with tidal highs and lows.")

        'then classify the events by dividing them over nAmplitudeKlassen collections of amplitude classes
        If Not myReeks.classifyEventsByAmplitude(AmplitudeClassesGrid) Then Throw New Exception("Error classifying tidal events into amplitude classes.")

        Dim ExperimentalMethod As Boolean = True

        If AdvancedTidalClassification Then
          'advanced method where sequences of elevated values over various classes are taken into account
          For Each AmplitudeClass As clsTidalAmplitudeClass In myReeks.TidalAmplitudeClasses.Classes.Values
            If Not AmplitudeClass.ClassifyEventsByElevationSequence(ElevationClassesGrid, ApplicationPercentage, TidalComponent) Then Throw New Exception("Error classifying events by elevated tide.")
          Next
          If Not myReeks.SequentialElevationStatsToExcel(myReeks.TidalHighsLowsEvent.TidalHighLowEvents.Count, nTidesPerDuration, TidalComponent, ApplicationPercentage) Then Throw New Exception("Error writing Excel.")
          If Not myReeks.BuildSequentialElevationDesignSeries(Duration, Uitloop, TidalComponent, ApplicationPercentage) Then Throw New Exception("Error building design series for tidal movement.")
        Else
          'simple method where only sequential elevated values inside the same class is taken into account
          For Each AmplitudeClass As clsTidalAmplitudeClass In myReeks.TidalAmplitudeClasses.Classes.Values
            If Not AmplitudeClass.ClassifyEventsByElevatedLevels(ElevationClassesGrid, ApplicationPercentage, TidalComponent) Then Throw New Exception("Error classifying events by elevated tide.")
          Next
          If Not myReeks.ClassificationStatsToExcel(myReeks.TidalHighsLowsEvent.TidalHighLowEvents.Count, nTidesPerDuration, TidalComponent, ApplicationPercentage) Then Throw New Exception("Error writing Excel.")
          If Not myReeks.BuildDesignSeries(Duration, Uitloop, TidalComponent, ApplicationPercentage) Then Throw New Exception("Error building design series for tidal movement.")
        End If
      Next

      Me.Setup.GeneralFunctions.UpdateProgressBar("Classification complete.", 0, 10)
      Return True
    Catch ex As Exception
      Me.Setup.Log.AddError(ex.Message)
      Return False
    End Try
  End Function



End Class


