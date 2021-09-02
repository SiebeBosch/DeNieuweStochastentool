Imports STOCHLIB.General

Public Class clsTidalAmplitudeClass
  'the original events containing all highs and lows
  Public Events As Dictionary(Of Long, clsTidalHighsLowsEvent)

  Public Name As String
  Public lBoundVal As Double     'lower amplitude value defining this class
  Public uBoundVal As Double     'upper amplitude value defining this class
  Public repVal As Double        'amplitude value representing this entire class

  Public uPercentile As Double   'the upper percentile value defining this class
  Public lPercentile As Double   'the lower percentile value defining this class
  Public repPercentile As Double 'the percentile value defining the representative value of this class

  'classified by elevated values
  Public TidalElevatedClasses As clsTidalElevatedClasses
  Public SequentialElevatedClasses As clssequentialElevatedClasses

  Private Setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup
    Events = New Dictionary(Of Long, clsTidalHighsLowsEvent)
    TidalElevatedClasses = New clsTidalElevatedClasses(Me.Setup)
    SequentialElevatedClasses = New clsSequentialElevatedClasses(Me.Setup, Me)
  End Sub

  Public Function CalcAverageLow() As Double
    'returns the average low value of all low tides inside this amplitude class
    Dim n As Long, Sum As Double
    For Each myEvent As clsTidalHighsLowsEvent In Events.Values
      For Each Tide As clsTidalHighLow In myEvent.Values.Values
        n += 1
        Sum += Tide.Low
      Next
    Next
    If n > 0 Then
      Return Sum / n
    Else
      Return 0
    End If
  End Function

  Public Function CalcAverageHigh() As Double
    'returns the average high value of all low tides inside this amplitude class
    Dim n As Long, Sum As Double
    For Each myEvent As clsTidalHighsLowsEvent In Events.Values
      For Each Tide As clsTidalHighLow In myEvent.Values.Values
        n += 1
        Sum += Tide.High
      Next
    Next
    If n > 0 Then
      Return Sum / n
    Else
      Return 0
    End If
  End Function

  Public Function ClassifyEventsByElevatedLevels(ByRef ElevationClassesGrid As Windows.Forms.DataGridView, ByVal SearchPercentage As Double, ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent) As Boolean

    Try
      'classifies all events by elevated high/lows inside this class
      Dim j As Long
      Dim nEvents = Events.Count
      Dim myEvent As clsTidalHighsLowsEvent
      Dim Levels(Events.Count - 1) As Double
      Dim myElevatedClass As clsTidalElevatedClass

      Dim nTidesPerEvent As Long = Events.Values(0).Values.Count

      'we'll only look in the last x% of the duration, so determine a start- and end index number for searching each event
      Dim nTidesSearched As Long = Me.Setup.GeneralFunctions.RoundUD(nTidesPerEvent * SearchPercentage / 100, 0, True)
      Dim endSearchIdx As Long = Events.Values(0).Values.Count - 1
      Dim startSearchIdx As Long = Math.Max(0, endSearchIdx - nTidesSearched + 1)

      'first build the elevation classes, based on the observed maxima
      Call BuildElevationClasses(ElevationClassesGrid, TidalComponent, startSearchIdx, endSearchIdx)

      For j = 0 To Events.Count - 1
        myEvent = Events.Values(j)
        myElevatedClass = myEvent.getTidalElevationClass(TidalComponent, startSearchIdx, endSearchIdx, TidalElevatedClasses)
        myElevatedClass.Events.Add(j, myEvent)
        myEvent.PercentileClassified = True
      Next

      '-------------------------------------------------------------------------------------------
      '              CLASSIFY BY THE NUMBER OF SEQUENTIAL ELEVATIONS INSIDE ITS CLASS
      '-------------------------------------------------------------------------------------------
      'this option classifies only by the number of sequential occurrances inside the highest class found
      'we'll consider the lowest class as the one without any elevated levels
      For Each myElevatedClass In TidalElevatedClasses.Classes.Values
        If myElevatedClass.lPerc = 0 Then
          'the class containing the lowest elevated tide is considered as 'no elevated tide at all'
          'therefore this class will be represented by an ordinary tidal wave without any elevations
          myElevatedClass.ClassifyAsNotElevated()
        Else
          myElevatedClass.ClassifyBySequentialOccurrances(TidalComponent, startSearchIdx, endSearchIdx)
        End If
      Next

      Return True
    Catch ex As Exception
      Return False
    End Try

  End Function

  Public Function ClassifyEventsByElevationSequence(ByRef ElevationClassesGrid As Windows.Forms.DataGridView, ByVal SearchPercentage As Double, ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent) As Boolean

    Try
      'classifies all events by the SEQUENCE of elevated high/lows inside this class
      Dim nTidesPerEvent As Long = Events.Values(0).Values.Count

      'we'll only look in the last x% of the duration, so determine a start- and end index number for searching each event
      Dim nTidesSearched As Long = Me.Setup.GeneralFunctions.RoundUD(nTidesPerEvent * SearchPercentage / 100, 0, True)
      Dim endSearchIdx As Long = Events.Values(0).Values.Count - 1
      Dim startSearchIdx As Long = Math.Max(0, endSearchIdx - nTidesSearched + 1)

      'first build the elevation classes, based on the observed maxima
      Call BuildElevationClasses(ElevationClassesGrid, TidalComponent, startSearchIdx, endSearchIdx)
      Call SequentialElevatedClasses.ClassifyEvents(TidalComponent, startSearchIdx, endSearchIdx, 3)

      Return True
    Catch ex As Exception
      Return False
    End Try

  End Function

  Public Sub BuildElevationClasses(ByRef ElevationClassesGrid As Windows.Forms.DataGridView, ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, ByVal startSearchIdx As Long, ByVal endSearchIdx As Long)

    '--------------------------------------------------------------------------------------------
    'author: Siebe Bosch
    'date: 8 feb 2015
    'this routine actually creates the tidal elevation classes. It sets the upper and lower
    'boundary value for each class by applying the corresponding percentile to the observed values
    '--------------------------------------------------------------------------------------------
    Dim myElevatedClass As clsTidalElevatedClass
    Dim myEvent As clsTidalHighsLowsEvent
    Dim Levels(Events.Count - 1) As Double
    Dim i As Long

    'first create an array containing the requested values (e.g. highest low tide, highest midtide or highest high tide) per event
    For i = 0 To Events.Count - 1
      myEvent = Events.Values(i)
      Levels(i) = myEvent.GetHighestElevation(TidalComponent, startSearchIdx, endSearchIdx)
    Next

    'based on the array, create a class for each of the elevated tide
    'note: here we'll take into account that the elevated values should only occur in the last x% of the event
    For i = 0 To ElevationClassesGrid.Rows.Count - 1
      myElevatedClass = New clsTidalElevatedClass(Me.Setup)
      myElevatedClass.Name = ElevationClassesGrid.Rows(i).Cells(0).Value
      myElevatedClass.lPerc = ElevationClassesGrid.Rows(i).Cells(1).Value
      myElevatedClass.uPerc = ElevationClassesGrid.Rows(i).Cells(2).Value
      myElevatedClass.Code = "[" & myElevatedClass.lPerc & "-" & myElevatedClass.uPerc & "]"
      myElevatedClass.repPerc = (myElevatedClass.lPerc + myElevatedClass.uPerc) / 2
      myElevatedClass.lBoundVal = Me.Setup.GeneralFunctions.Percentile(Levels, myElevatedClass.lPerc)
      myElevatedClass.uBoundVal = Me.Setup.GeneralFunctions.Percentile(Levels, myElevatedClass.uPerc)
      myElevatedClass.ReprVal = Me.Setup.GeneralFunctions.Percentile(Levels, myElevatedClass.repPerc)
      TidalElevatedClasses.Classes.Add(myElevatedClass.repPerc, myElevatedClass)
    Next
  End Sub


End Class
