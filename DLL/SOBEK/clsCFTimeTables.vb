Option Explicit On
Imports STOCHLIB.General

Public Class clsCFTimeTables
  Friend TimeTables As New Dictionary(Of String, clsSobekTable)
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each myTable As clsSobekTable In TimeTables.Values
      myTable.AddPrefix(Prefix)
    Next
  End Sub

  Friend Function GetTable(ByVal ID As String)
    If TimeTables.ContainsKey(ID.Trim.ToUpper) Then
      Return TimeTables(ID.Trim.ToUpper)
    Else
      Return Nothing
    End If
  End Function

  Friend Sub AddTable(ByVal myTable As clsSobekTable)
    TimeTables.Add(myTable.ID.Trim.ToUpper, myTable)
  End Sub
End Class
