Imports STOCHLIB.General

Public Class clsTidalSequentialOccurrancesClasses
  Public Classes As New Dictionary(Of Integer, clsTidalSequentialOccurrancesClass) 'key = number of sequential occurrances inside the class
  Private Setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup)
    Classes = New Dictionary(Of Integer, clsTidalSequentialOccurrancesClass)
    Setup = mySetup
  End Sub

End Class
