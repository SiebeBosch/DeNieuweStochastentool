Option Explicit On

Imports STOCHLIB.General

Public Class clsNetworkOBIOBIDRecord
    Friend ID As String 'id
    Friend ci As String 'knooptype
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Public Function GetNodeType() As STOCHLIB.GeneralFunctions.enmNodetype

        Select Case ci
      'eerst de reachnodes (knopen die een integraal onderdeel van de tak vormen
            Case Is = "SBK_CHANNEL_STORCONN&LAT"
                Return GeneralFunctions.enmNodetype.ConnNodeLatStor
            Case Is = "SBK_CHANNELCONNECTION"
                Return GeneralFunctions.enmNodetype.NodeCFConnectionNode
            Case Is = "SBK_CHANNELLINKAGENODE"
                Return GeneralFunctions.enmNodetype.NodeCFLinkage
            Case Is = "SBK_BOUNDARY"
                Return GeneralFunctions.enmNodetype.NodeCFBoundary
            Case Is = "SBK_SBK-3B-NODE"
                Return GeneralFunctions.enmNodetype.NodeRRCFConnectionNode
            Case Is = "SBK_CONNECTIONNODE"                  'manhole
                Return GeneralFunctions.enmNodetype.NodeSFManhole
            Case Is = "SBK_CONN&LAT"
                Return GeneralFunctions.enmNodetype.NodeSFManholeWithLateralFlow
            Case Is = "SBK_CONN&RUNOFF"                     'manhole with runoff
                Return GeneralFunctions.enmNodetype.NodeSFManholeWithRunoff
            Case Is = "SBK_EXTPUMP"                         'external pump
                Return GeneralFunctions.enmNodetype.NodeSFExternalPump

        'en nu op naar de takobjecten
            Case Is = "SBK_PROFILE"
                Return GeneralFunctions.enmNodetype.SBK_PROFILE
            Case Is = "SBK_WEIR"
                Return GeneralFunctions.enmNodetype.NodeCFWeir
            Case Is = "SBK_UNIWEIR"
                Return GeneralFunctions.enmNodetype.NodeCFUniWeir
            Case Is = "SBK_CULVERT"
                Return GeneralFunctions.enmNodetype.NodeCFCulvert
            Case Is = "SBK_PUMP"
                Return GeneralFunctions.enmNodetype.NodeCFPump
            Case Is = "SBK_BRIDGE"
                Return GeneralFunctions.enmNodetype.NodeCFBridge
            Case Is = "SBK_ORIFICE"
                Return GeneralFunctions.enmNodetype.NodeCFOrifice
            Case Is = "SBK_MEASSTAT"
                Return GeneralFunctions.enmNodetype.MeasurementStation
            Case Is = "SBK_LATERALFLOW"
                Return GeneralFunctions.enmNodetype.NodeCFLateral
            Case Is = "SBK_SBK-3B-REACH"
                Return GeneralFunctions.enmNodetype.NodeRRCFConnection
            Case Is = "SBK_GRIDPOINT"
                Return GeneralFunctions.enmNodetype.NodeCFGridpoint
            Case Is = "SBK_GRIDPOINTFIXED"
                Return GeneralFunctions.enmNodetype.NodeCFGridpointFixed
            Case Is = "SBK_EXTRARESISTANCE"
                Return GeneralFunctions.enmNodetype.NodeCFExtraResistance
            Case Else
                'nog niet ondersteunde objecten
                Me.setup.Log.AddWarning("Sobek object type not (yet) supported: node " & ID)
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
