Option Explicit On
Imports STOCHLIB.General

Public Class clsSobekCaseListItem
  Friend dir As String
  Friend name As String
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub
  Friend Sub Read(ByVal myStr As String)
    dir = Me.setup.GeneralFunctions.ParseString(myStr, " ")
    name = Mid(myStr, 2, myStr.Length - 2)
  End Sub
End Class
