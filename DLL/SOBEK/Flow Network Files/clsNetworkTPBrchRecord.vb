﻿Option Explicit On

Imports STOCHLIB.General

Friend Class clsNetworkTPNodeRecord
  Friend ID As String
  Friend Name As String
  Friend X As Double
  Friend Y As Double
  Friend NodeType As enmNodetype

  Friend Enum enmNodetype
    ConnectionNode = 12
    ConnNodeLatstor = 13
    Boundary = 14
    RRCFConnNode = 35
    LinkageNode = 15
  End Enum
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
          Name = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "px"
          X = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "py"
          Y = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case ""
          Done = True
      End Select
    End While

    'zet hem standaard op connection node
    NodeType = enmNodetype.ConnectionNode

  End Sub

End Class
