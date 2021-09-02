Public Class clsSequentialElevatedClass
  Public Name As String
  Public SequentialElevationClassIndices As New List(Of Integer) 'a sequential list of the elevation classes that the events inside this class run through
  Public Events As New List(Of clsTidalHighsLowsEvent)

  Public Function GetSequentialElevationClasses(ByRef ElevationClasses As clsTidalElevatedClasses) As String
    Dim i As Integer, myStr As String = ""
    For i = 0 To SequentialElevationClassIndices.Count - 1
      myStr &= ElevationClasses.Classes.Values(SequentialElevationClassIndices(i)).Code
    Next
    Return myStr
  End Function

End Class
