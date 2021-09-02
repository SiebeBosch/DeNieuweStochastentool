Imports STOCHLIB.General

Public Class clsStochastenBoundaryNode
  Public ID As String
  Public prnPath As String

  Private Setup As clsSetup
  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup

  End Sub

End Class
