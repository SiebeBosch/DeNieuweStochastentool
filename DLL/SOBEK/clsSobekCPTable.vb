Option Explicit On

Imports STOCHLIB.General

Friend Class clsSobekCPTable
  Friend CP As New List(Of clsSbkVectorPoint)
  Private setup As clsSetup

  Public Function Duplicate(ByVal newReachID As String) As clsSobekCPTable
    Dim newTable As New clsSobekCPTable(Me.setup)
    Dim newCP As clsSbkVectorPoint
    For Each myCP As clsSbkVectorPoint In CP
      newCP = myCP.Duplicate(newReachID)
      newTable.CP.Add(newCP)
    Next
    Return newTable
  End Function

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub
End Class
