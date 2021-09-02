Option Explicit On

Imports STOCHLIB.General

Friend Class clsCFReachNode

  'this class defines any of the node types that are used for the beginning or end of a reach:
  'connection node, connection node w lateral discharge and storage, linkage node, boundary and rrcf connecton node

  Friend ID As String
  Friend Name As String
  Friend X As Double
  Friend Y As Double

  Friend ci As String 'reachID van aantakkende tak (alleen voor Linkage Nodes)
  Friend lc As Double 'afstand op de aantakkende tak waarop de linkage node aanklampt
  Friend nt As enmNodetype
  Friend NetworkTPNodeRecord As clsNetworkTPNodeRecord
  Friend NetworkTPNdlkRecord As clsNetworkTPNdlkRecord
  Friend NetworkObiRecord As clsNetworkOBIOBIDRecord

  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup

    'Init classes:
    NetworkTPNodeRecord = New clsNetworkTPNodeRecord(Me.setup)
    NetworkTPNdlkRecord = New clsNetworkTPNdlkRecord(Me.setup)
    NetworkObiRecord = New clsNetworkOBIOBIDRecord(Me.setup)

  End Sub
  Friend Enum enmNodetype
    NodeRRUnpaved = 43
    NodeRRsacramento = 54
    NodeRRPaved = 42
    NodeRRWWTP = 56
    NodeRROpenWater = 45
    NodeRRGreenhouse = 44
    NodeRRBoundary = 46
    NodeRRPump = 47
    NodeRRIndustry = 48
    NodeRRCFConnectionNode = 35
    NodeRRCFConnection = 34
    NodeRRWeir = 49
    NodeRROrifice = 50
    NodeRRFriction = 51
    NodeCFConnectionNode = 12
    NodeCFConnNodeLatStor = 13
    NodeCFBoundary = 14
    NodeCFMeasurement = 18
    NodeCFLinkageNode = 15
    NodeCFLateral = 19
    nodecfprofile = 20
    nodecfweir = 21
    NodeCFUniversalWeir = 22
    NodeCFOrifice = 23
    NodeCFCulvert = 24
    nodecfbridge = 26
    nodecfpump = 27
  End Enum

  Friend Sub SetNodeType()
    Select Case NetworkObiRecord.ci
      Case "SBK_CHANNEL_STORCONN&LAT"
        nt = enmNodetype.NodeCFConnNodeLatStor
      Case "SBK_CHANNELCONNECTION"
        nt = enmNodetype.NodeCFConnectionNode
      Case "SBK_CHANNELLINKAGENODE"
        nt = enmNodetype.NodeCFLinkageNode
      Case "SBK_BOUNDARY"
        nt = enmNodetype.NodeCFBoundary
      Case "SBK_SBK-3B-NODE"
        nt = enmNodetype.NodeRRCFConnectionNode
    End Select
  End Sub

  Friend Function NumConnectedReaches(ByRef Reaches As clsSbkReaches, ByVal IgnoreDisabledReaches As Boolean) As Integer
    'deze routine geeft terug hoeveel takken een gegeven reachnode verbindt
    Dim n As Integer
    For Each myReach As clsSbkReach In Reaches.Reaches.Values
      'we behandelen boundaries als doorlopende takken die nooit mogen worden verwijderd
      If nt = enmNodetype.NodeCFBoundary Then
        NumConnectedReaches = 99 'geef een groot getal terug om zeker te zijn dat deze tak niet wordt weggeknipt
      ElseIf myReach.bn.ID = ID Or myReach.en.ID = ID Then
        If IgnoreDisabledReaches = False Or (IgnoreDisabledReaches = True And myReach.InUse = True) Then
          n = n + 1
        End If
      End If
    Next myReach

    'als hij een linkage node is, verbindt hij nog een tak die we in de voorgaande loop nog niet hebbben gezien
    If ci <> "" Then n = n + 1
    Return n

  End Function
End Class
