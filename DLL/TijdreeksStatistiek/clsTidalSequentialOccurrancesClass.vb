Imports STOCHLIB.General

Public Class clsTidalSequentialOccurrancesClass

  'the original events containing all highs and lows
  Public Events As Dictionary(Of Long, clsTidalHighsLowsEvent)
  Public nSequential As Long

  Private Setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup
    Events = New Dictionary(Of Long, clsTidalHighsLowsEvent)
  End Sub

End Class
