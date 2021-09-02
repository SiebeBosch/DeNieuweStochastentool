Option Explicit On

Imports STOCHLIB.General

Public Class clsSbkReachNode

    Friend ID As String
    Friend AreaID As String     'the id of the subcatchment that this node represents (optional)
    Friend Name As String
    Friend X As Double
    Friend Y As Double
    Friend ci As String 'reachID van aantakkende tak (alleen voor Linkage Nodes)
    Friend lc As Double 'afstand op de aantakkende tak waarop de linkage node aanklampt
    Friend nt As STOCHLIB.GeneralFunctions.enmNodetype          'verouderd. vanaf nu vervangen door NodeType (volgende regel!)
    Friend SubType As STOCHLIB.GeneralFunctions.enmNodeSubType
    Friend NodeType As clsSobekNodeType
    Friend InUse As Boolean

    Friend TargetLevels As clsTargetLevels  'a place to store the underlying target levels from GIS
    Friend BedLevel As Double               'a place to store the underlying bed level

    Friend SnapChainage As Double       'the chainage on SnapReach where this node could snap to
    Friend SnapReachID As String        'the nearest other sobek reach this node could snap to
    Friend SnapDistance As Double       'the distance between the original point and its snapping location

    Friend Bound3B3BRecord As clsBound3B3BRecord
    Friend Bound3BTBLRecord As clsBound3BTBLRecord
    Friend NetworkTPNodeRecord As clsNetworkTPNodeRecord
    Friend NetworkTPNdlkRecord As clsNetworkTPNdlkRecord
    Friend NetworkObiRecord As clsNetworkOBIOBIDRecord
    Private Setup As clsSetup
    Private SbkCase As clsSobekCase


    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'Init classes
        NetworkTPNodeRecord = New clsNetworkTPNodeRecord(Me.Setup)
        NetworkTPNdlkRecord = New clsNetworkTPNdlkRecord(Me.Setup)
        NetworkObiRecord = New clsNetworkOBIOBIDRecord(Me.Setup)
        Bound3B3BRecord = New clsBound3B3BRecord(Me.Setup)
        Bound3BTBLRecord = New clsBound3BTBLRecord(Me.Setup)
    End Sub

    Public Function Rename(NewID As String) As Boolean
        Try

            'clone the current node
            Dim newNode As clsSbkReachNode = Clone(NewID)
            SbkCase.CFTopo.ReachNodes.ReachNodes.Add(NewID.Trim.ToUpper, newNode)

            'adjust all references
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                If myReach.bn.ID.Trim.ToUpper = ID.Trim.ToUpper Then myReach.bn = newNode
                If myReach.en.ID.Trim.ToUpper = ID.Trim.ToUpper Then myReach.en = newNode
            Next

            'also change the nodes.dat record
            Dim myDat As clsNodesDatNODERecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.GetRecord(ID)
            If Not myDat Is Nothing Then
                Dim newDat As clsNodesDatNODERecord
                newDat = myDat.Clone(newNode.ID)
                SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Add(newDat.ID.Trim.ToUpper, newDat)
                myDat.InUse = False
            End If

            'set the old node to false
            'v1.77
            InUse = False

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function Clone(NewID As String) As clsSbkReachNode
        Dim newNode As New clsSbkReachNode(Me.Setup, Me.SbkCase)
        newNode.ID = NewID
        newNode.InUse = InUse
        newNode.lc = lc
        newNode.Name = Name
        newNode.NetworkObiRecord = NetworkObiRecord
        newNode.NetworkTPNdlkRecord = NetworkTPNdlkRecord
        newNode.NetworkTPNodeRecord = NetworkTPNodeRecord
        newNode.NodeType = NodeType
        newNode.nt = nt
        newNode.SnapChainage = SnapChainage
        newNode.SnapDistance = SnapDistance
        newNode.SnapReachID = SnapReachID
        newNode.TargetLevels = TargetLevels
        newNode.X = X
        newNode.Y = Y
        newNode.BedLevel = BedLevel
        newNode.Bound3B3BRecord = Bound3B3BRecord
        newNode.Bound3BTBLRecord = Bound3BTBLRecord
        newNode.ci = ci
        Return newNode
    End Function

    Public Function getFirstDownstreamReach() As clsSbkReach
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            If myReach.InUse AndAlso myReach.bn.ID.Trim.ToUpper = ID.Trim.ToUpper Then
                Return myReach
            End If
        Next
        Return Nothing
    End Function

    Public Function getFirstUpstreamReach() As clsSbkReach
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            If myReach.InUse AndAlso myReach.en.ID.Trim.ToUpper = ID.Trim.ToUpper Then
                Return myReach
            End If
        Next
        Return Nothing
    End Function

    Public Function exportAsShape() As MapWinGIS.Shape
        Try
            Dim nodeShape As New MapWinGIS.Shape
            nodeShape.Create(MapWinGIS.ShpfileType.SHP_POINT)
            nodeShape.AddPoint(Me.X, Me.Y)
            Return nodeShape
        Catch ex As Exception
            Me.Setup.Log.AddError("Error exporting reachnode to shape: " & ID)
            Return Nothing
        End Try
    End Function

    Friend Function makeMapWinGisPoint() As MapWinGIS.Point
        Dim myPoint As New MapWinGIS.Point
        myPoint.x = X
        myPoint.y = Y
        Return myPoint
    End Function

    Friend Sub SetNodeType()
        Select Case NetworkObiRecord.ci
            Case "SBK_CHANNEL_STORCONN&LAT"
                nt = STOCHLIB.GeneralFunctions.enmNodetype.ConnNodeLatStor
            Case "SBK_CHANNELCONNECTION"
                nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFConnectionNode
            Case "SBK_CHANNELLINKAGENODE"
                nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLinkage
            Case "SBK_BOUNDARY"
                nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary
            Case "SBK_SBK-3B-NODE"
                nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode
            Case "SBK_CONNECTIONNODE"
                nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFConnectionNode
            Case "SBK_CONN&RUNOFF"
                nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeSFManholeWithRunoff
            Case "SBK_EXTPUMP"
                nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeSFExternalPump
        End Select
    End Sub

    Friend Function isQBoundary() As Boolean
        If nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary Then
            Return Me.SbkCase.CFData.Data.BoundaryData.isQBoundary(ID)
        Else
            Return False
        End If
    End Function

    Friend Function isHBoundary() As Boolean
        If nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary Then
            Return Me.SbkCase.CFData.Data.BoundaryData.isHBoundary(ID)
        Else
            Return False
        End If
    End Function

    Friend Function isLateral() As Boolean
        If nt = GeneralFunctions.enmNodetype.ConnNodeLatStor OrElse nt = GeneralFunctions.enmNodetype.NodeRRCFConnectionNode Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function isBoundary() As Boolean
        If nt = GeneralFunctions.enmNodetype.NodeCFBoundary Then
            Return True
        Else
            Return False
        End If
    End Function

    Friend Function isConnectionNode() As Boolean
        Select Case nt
            Case Is = GeneralFunctions.enmNodetype.NodeCFConnectionNode
                Return True
            Case Is = GeneralFunctions.enmNodetype.ConnNodeLatStor
                Return True
            Case Is = GeneralFunctions.enmNodetype.NodeRRCFConnectionNode
                Return True
            Case Is = GeneralFunctions.enmNodetype.NodeSFManhole
                Return True
            Case Is = GeneralFunctions.enmNodetype.NodeSFManholeWithLateralFlow
                Return True
            Case Is = GeneralFunctions.enmNodetype.NodeSFManholeWithRunoff
                Return True
            Case Else
                Return False
        End Select
    End Function

    Friend Function NumConnectedReaches(ByVal IgnoreDisabledReaches As Boolean) As Integer
        'deze routine geeft terug hoeveel takken een gegeven reachnode verbindt
        Dim n As Integer
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            'we behandelen boundaries als doorlopende takken die nooit mogen worden verwijderd
            If nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary Then
                Return 99 'geef een groot getal terug om zeker te zijn dat deze tak niet wordt weggeknipt
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


    Friend Function GetConnectedReaches(ByVal IgnoreDisabledReaches As Boolean, ByRef Reaches As List(Of clsSbkReach)) As Integer
        'deze routine geeft terug welke takken een gegeven reachnode verbindt
        Dim n As Integer
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            'we behandelen boundaries als doorlopende takken die nooit mogen worden verwijderd
            If nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary Then
                Return 1
            ElseIf myReach.bn.ID = ID OrElse myReach.en.ID = id Then
                If IgnoreDisabledReaches = False Or (IgnoreDisabledReaches = True And myReach.InUse = True) Then
                    Reaches.Add(myReach)
                    n = n + 1
                End If
            End If
        Next myReach

        'als hij een linkage node is, verbindt hij nog een tak die we in de voorgaande loop nog niet hebbben gezien
        If ci <> "" Then n = n + 1
        Return n

    End Function


End Class
