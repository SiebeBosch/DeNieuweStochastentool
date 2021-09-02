Imports STOCHLIB.General

Public Class clsSequentialElevatedClasses

  Public Classes As New Dictionary(Of String, clsSequentialElevatedClass)
  Private Setup As clsSetup
  Private AmplitudeClass As clsTidalAmplitudeClass

  Public Sub New(ByRef mySetup As clsSetup, ByRef myAmplitudeClass As clsTidalAmplitudeClass)
    Setup = mySetup
    AmplitudeClass = myAmplitudeClass
  End Sub

  Public Function ClassifyEvents(ByVal TidalComponent As STOCHLIB.GeneralFunctions.enmTidalComponent, ByVal startSearchIdx As Integer, ByVal endSearchIdx As Integer, ByVal nTidalWaves As Integer) As Boolean
    'this function classifies the events inside the current amplitude class into unique sequential patterns of elevated levels
    Dim mySortedList As New List(Of Integer)
    Dim i As Integer, myKey As String
    Dim mySeqClass As clsSequentialElevatedClass

    Try
      For Each myEvent As clsTidalHighsLowsEvent In AmplitudeClass.Events.Values
        Dim myList As New List(Of Integer) 'list of the heaviest series of occurrances in elevated tidal classes
        myList = myEvent.getElevationClassSequence(TidalComponent, startSearchIdx, endSearchIdx, AmplitudeClass.TidalElevatedClasses, nTidalWaves)

        'the key will be derived from a sorted list. This puts various combinations of the same elevations in one and the same class
        mySortedList = New List(Of Integer)
        For Each myInt As Integer In myList
          mySortedList.Add(myInt)
        Next
        mySortedList.Sort()

        myKey = ""
        For i = 0 To myList.Count - 1
          myKey &= AmplitudeClass.TidalElevatedClasses.Classes.Values(mySortedList(i)).Code
        Next

        If Classes.ContainsKey(myKey) Then
          Classes.Item(myKey).Events.Add(myEvent)
        Else
          mySeqClass = New clsSequentialElevatedClass
          mySeqClass.Events.Add(myEvent)
          mySeqClass.Name = myKey
          mySeqClass.SequentialElevationClassIndices = myList 'stores the order of elevation classes encountered
          Classes.Add(myKey, mySeqClass)
        End If

      Next

      Return True
    Catch ex As Exception
      Me.Setup.Log.AddError(ex.Message)
      Return False
    End Try
  End Function


End Class
