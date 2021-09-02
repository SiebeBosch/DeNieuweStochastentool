Imports STOCHLIB.General

Public Class clsTidalElevatedClass

  Public Events As Dictionary(Of Long, clsTidalHighsLowsEvent)
  Public SequentialOccurrancesClasses As clsTidalSequentialOccurrancesClasses

  Public Name As String
  Public lPerc As Double               'lower percentile that this class represents
  Public uPerc As Double               'upper precentile that this class represents
  Public Code As String                'code representing the class range
  Public repPerc As Double             'the percentile that will represent all events in this class
  Public lBoundVal As Double           'lower boundary value for this class
  Public uBoundVal As Double           'upper boundary value for this class
  Public ReprVal As Double             'representative elevated value for this class

  Private Setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup
    Events = New Dictionary(Of Long, clsTidalHighsLowsEvent)
    SequentialOccurrancesClasses = New clsTidalSequentialOccurrancesClasses(Me.Setup)
  End Sub

  Public Function ClassifyAsNotElevated() As Boolean

    'classifies all events inside this class of events with 'elevated' tides as if they have zero elevated levels
    'in other words: this will be the class that represents all normal tidal behaviour
    Dim i As Long, myEvent As clsTidalHighsLowsEvent
    Dim myOccurranceClass As clsTidalSequentialOccurrancesClass
    Try
      myOccurranceClass = New clsTidalSequentialOccurrancesClass(Me.Setup)
      myOccurranceClass.nSequential = 0
      SequentialOccurrancesClasses.Classes.Add(myOccurranceClass.nSequential, myOccurranceClass)
      For i = 0 To Events.Count - 1
        myEvent = Events.Values(i)
        myOccurranceClass.Events.Add(i, myEvent)
      Next
      Return True
    Catch ex As Exception
      Me.Setup.Log.AddError(ex.Message)
      Return False
    End Try

  End Function

  Public Function ClassifyBySequentialOccurrances(ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, Optional ByVal StartSearchIdx As Long = -1, Optional ByVal EndSearchIdx As Long = -1) As Boolean
    'classifies the events inside this class by number of sequential occurances inside the classes bounds
    Dim nSequential As Long, i As Long, myEvent As clsTidalHighsLowsEvent
    Dim myOccurranceClass As clsTidalSequentialOccurrancesClass

    If StartSearchIdx < 0 Then StartSearchIdx = 0
    If EndSearchIdx < 0 Then EndSearchIdx = Events.Values(0).Values.Count - 1

    For i = 0 To Events.Count - 1
      myEvent = Events.Values(i)
      nSequential = myEvent.CountSequentialOccurrances(lBoundVal, uBoundVal, TidalComponent, StartSearchIdx, EndSearchIdx)

      'find the class that represents the found number of sequential occurrances
      If Not SequentialOccurrancesClasses.Classes.ContainsKey(nSequential) Then
        myOccurranceClass = New clsTidalSequentialOccurrancesClass(Me.Setup)
        myOccurranceClass.nSequential = nSequential
        myOccurranceClass.Events.Add(i, myEvent)
        SequentialOccurrancesClasses.Classes.Add(myOccurranceClass.nSequential, myOccurranceClass)
      Else
        myOccurranceClass = SequentialOccurrancesClasses.Classes.Item(nSequential)
        myOccurranceClass.Events.Add(i, myEvent)
      End If
    Next
    Return True
  End Function



End Class
