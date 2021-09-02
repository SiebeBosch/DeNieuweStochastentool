Option Explicit On

Imports STOCHLIB.General

Friend Class clsNetworkCNFLBXRecord
  Friend ID As String
  Friend nm As String
  Friend ci As String
  Friend lc As Double
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub
  Friend Sub Read(ByVal myRecord As String)
    Dim Done As Boolean, myStr As String
    Done = False

    While Not Done
      myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
      Select Case LCase(myStr)
        Case "id"
          ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "nm"
          nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "ci"
          ci = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "lc"
          lc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case ""
          If myRecord = "" Then Done = True
      End Select
    End While

  End Sub
End Class
