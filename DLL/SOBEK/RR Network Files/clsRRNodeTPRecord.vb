Option Explicit On
Imports STOCHLIB.General

Public Class clsRRNodeTPRecord

    Public ID As String
    Public nm As String
    Public ri As String
    Public mt As Integer
    Public mtString As String
    Public nt As clsSobekNodeType
    Public OBI As String
    Public X As Double
    Public Y As Double

    Public InUse As Boolean

    'derived parameters
    Friend CatchmentID As String
    Friend SubcatchmentID As String
    Friend Tim As List(Of Double)
    Friend Links As New List(Of clsRRLink)
    Friend WaterLevels As clsTimeTable
    Friend Volumes As clsTimeTable
    Friend Seepage As clsTimeTable
    Friend Rainfall As clsTimeTable
    Public UnitNodeID As String     'this is an ID that can be used to store the ID for aggregated situations, where we work with unit nodes (e.g. for RTC Tools)

    'other derived parameters
    Friend ModelcatchmentID As String 'keeps track of to which modelcatchment this RR node belongs

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase


    Public Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
        WaterLevels = New clsTimeTable(Me.Setup)
        Volumes = New clsTimeTable(Me.Setup)
        Seepage = New clsTimeTable(Me.Setup)
        Rainfall = New clsTimeTable(Me.Setup)

    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        WaterLevels = New clsTimeTable(Me.Setup)
        Volumes = New clsTimeTable(Me.Setup)
        Seepage = New clsTimeTable(Me.Setup)
        Rainfall = New clsTimeTable(Me.Setup)

    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal iId As String, ByVal iName As String, ByVal iX As Decimal, ByVal iY As Decimal, ByVal iNodeType As clsSobekNodeType, Optional ByVal SetInuse As Boolean = True)
        ID = iId
        nm = iName
        X = iX
        Y = iY
        nt = iNodeType
        InUse = True
        Setup = mySetup
        InUse = SetInuse
    End Sub

    Public Function getUpstreamAreas(ByRef Unpaved As Double, ByRef Paved As Double, ByRef Greenhouse As Double, ByRef Sacramento As Double, ByRef NodesDone As Dictionary(Of String, clsRRNodeTPRecord)) As Boolean
        'this function calculates the upstream area by landuse for a given RR node
        Dim Done As Boolean
        Dim myNode As clsRRNodeTPRecord
        Try
            While Not Done
                Done = True
                For Each myLink As clsRRBrchTPRecord In SbkCase.RRTopo.Links.Values
                    If myLink.en.Trim.ToUpper = ID.Trim.ToUpper Then
                        If Not NodesDone.ContainsKey(myLink.bn.Trim.ToUpper) Then
                            'nieuwe knoop gevonden
                            Done = False
                            myNode = SbkCase.RRTopo.GetNode(myLink.bn)
                            NodesDone.Add(myLink.bn.Trim.ToUpper, myNode)
                            Select Case myNode.nt.ParentID
                                Case Is = "3B_UNPAVED"
                                    Dim UP3B As clsUnpaved3BRecord = SbkCase.RRData.Unpaved3B.GetRecord(myNode.ID)
                                    Unpaved += UP3B.getTotalLandUseArea
                                Case Is = "3B_PAVED"
                                    Dim PV3B As clsPaved3BRecord = SbkCase.RRData.Paved3B.GetRecord(myNode.ID)
                                    Paved += PV3B.ar
                                Case Is = "3B_GREENHOUSE"
                                    Dim GR3B As clsGreenhse3BRecord = SbkCase.RRData.Greenhse3B.GetRecord(myNode.ID)
                                    Greenhouse += GR3B.getTotalArea
                                Case Is = "3B_SACRAMENTO"
                                    Dim SACR As clsSACRMNTO3BSACRRecord = SbkCase.RRData.Sacr3BSACR.GetRecord(myNode.ID)
                                    Sacramento += SACR.ar
                            End Select

                            'dig deeper and look even further upstream for more nodes
                            If Not myNode.getUpstreamAreas(Unpaved, Paved, Greenhouse, Sacramento, NodesDone) Then Throw New Exception("Error getting upstream areas for node " & myNode.ID)

                        End If
                    End If
                Next
            End While
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getUpstreamAreas of class clsRRNodeTPRecord.")
            Return False
        End Try



    End Function


    Public Function hasArea() As Boolean
        Select Case nt.ParentID
            Case Is = "3B_UNPAVED"
                Return True
            Case Is = "3B_PAVED"
                Return True
            Case Is = "3B_GREENHOUSE"
                Return True
            Case Is = "3B_SACRAMENTO"
                Return True
            Case Is = "3B_HBV"
                Return True
            Case Is = "3B_OPENWATER"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Public Function getDownstreamRROnFlowConnection() As clsRRNodeTPRecord
        Dim Done As Boolean = False
        Dim myNode As clsRRNodeTPRecord

        'first collect all downstream connected RR nodes and return the one rr on flow connection
        Dim downNodes As New Dictionary(Of String, String)
        downNodes = SbkCase.RRTopo.getDownstreamNodes(Me.ID)

        For Each myStr As String In downNodes.Values
            myNode = SbkCase.RRTopo.GetNode(myStr)
            If myNode.nt.ParentID = "SBK_SBK-3B-REACH" OrElse myNode.nt.ParentID = "SBK_SBK-3B-NODE" Then
                Return myNode
            End If
        Next
        Return Nothing
    End Function

    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, NodeTypeNum As Integer
        Done = False

        InUse = True

        While Not Done
            myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "nm"
                    nm = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "ri"
                    ri = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "mt"
                    mt = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    mtString = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "nt"
                    NodeTypeNum = Convert.ToInt16(Me.Setup.GeneralFunctions.ParseString(myRecord, " "))
                    nt = SbkCase.NodeTypes.GetByNum(NodeTypeNum)
                    If nt Is Nothing Then Me.Setup.Log.AddError("Node type number " & nt.SbkNum & " not found in ntrpluv.ini and/or ntrpluv.obj.")
                Case "obid"
                    OBI = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    'double check the node type num. If the user has not updated their case to the sobek-verstion they're using
                    'the number and type may go wrong
                    If Not nt.ParentID = OBI Then
                        'not ok. switch back to the original parent node type in order to force at leas the correct node type
                        Me.Setup.Log.AddError("Node type number " & nt.SbkNum & " parent node type description " & OBI & " do not correspond. Please make sure your SOBEK case has been UPDATED AND SAVED in your current SOBEK version!")
                        nt = SbkCase.NodeTypes.GetByParentID(OBI)
                    End If

                Case "px"
                    X = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "py"
                    Y = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case ""
                    Done = True
            End Select
        End While
    End Sub

End Class
