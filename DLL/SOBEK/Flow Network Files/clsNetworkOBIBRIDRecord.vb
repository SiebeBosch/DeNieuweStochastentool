Option Explicit On

Imports STOCHLIB.General

Public Class clsNetworkOBIBRIDRecord
    Friend ID As String 'id
    Friend ci As String 'knooptype
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Public Function GetReachType() As STOCHLIB.GeneralFunctions.enmReachtype

        Select Case ci
      'eerst de reachnodes (knopen die een integraal onderdeel van de tak vormen
            Case Is = "SBK_CHANNEL"
                Return GeneralFunctions.enmReachtype.ReachCFChannel
            Case Is = "SBK_INTCULVERT"
                Return GeneralFunctions.enmReachtype.ReachSFInternalCulvert
            Case Is = "SBK_INTWEIR"
                Return GeneralFunctions.enmReachtype.ReachSFInternalWeir
            Case Is = "SBK_INTORIFICE"
                Return GeneralFunctions.enmReachtype.ReachSFInternalOrifice
            Case Is = "SBK_INTPUMP"
                Return GeneralFunctions.enmReachtype.ReachSFInternalPump
            Case Is = "SBK_PIPE"
                Return GeneralFunctions.enmReachtype.ReachSFPipe
            Case Is = "SBK_PIPE&RUNOFF"
                Return GeneralFunctions.enmReachtype.ReachSFDWAPipeWithRunoff
            Case Is = "SBK_RWAPIPE&RUNOFF"
                Return GeneralFunctions.enmReachtype.ReachSFRWAPipeWithRunoff
            Case Is = "SBK_DWAPIPE&RUNOFF"
                Return GeneralFunctions.enmReachtype.ReachSFDWAPipeWithRunoff
            Case Is = "SBK_STREET"
                Return GeneralFunctions.enmReachtype.ReachSFDWAPipeWithRunoff
            Case Else
                'nog niet ondersteunde objecten
                Me.setup.Log.AddWarning("Sobek reach type " & ci & " not (yet) supported: node " & ID)
        End Select

    End Function
    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String
        Done = False

        While Not Done
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ci"
                    ci = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case ""
                    Done = True
            End Select
        End While
    End Sub

End Class
