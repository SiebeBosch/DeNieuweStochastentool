Imports STOCHLIB.General

Public Class clsStochastenBoundary
  Public Nodes As New Dictionary(Of String, clsStochastenBoundaryNode)
  Private Setup As clsSetup
  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup

  End Sub

End Class
