Option Explicit On

Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System

Public Class clsSbkReach
    Friend Id As String
    Friend num As Integer 'the reach number as internally used?
    Friend bn As clsSbkReachNode
    Friend en As clsSbkReachNode
    Friend ReachType As clsSobekBranchType
    Friend ReachSegments As New Dictionary(Of String, clsSbkReachSegment)

    Friend ChannelUsage As STOCHLIB.GeneralFunctions.enmChannelUsage
    Friend BoundingBox As clsBoundingBox
    Friend BoundBox As MapWinGIS.Extents

    'meta information
    Friend CrossSectionOrigin As String  'meta-informatie om bij te houden wat de herkomst/totstandkoming van dwarsprofielen op deze tak is
    Friend CrossSectionVerdict As Integer 'rapportcijfer voor de herkomst van de dwarsprofielen op deze tak

    Friend InUse As Boolean
    Friend Done As Boolean      'facilitates the registration of whether the reach has already been processed
    Friend OriginalID As String 'the ID for the original reach, before splitting or merging
    Friend ReachCategory As String 'a placeholder to store the reach category in (hoofdwatergang, secundair etc.)
    Friend RouteNumber As Integer 'an identifier to keep track of chains of interpolation between reaches

    Friend NetworkTPBrchRecord As clsNetworkTPBrchRecord
    Friend NetworkcpRecord As clsNetworkCPRecord
    'Friend InitialDatFLINRecord As clsInitialDatFLINRecord
    Friend ReachObjects As clsSbkReachObjects

    Friend upshift As Double 'specially for Infoworks: vertical shift of the reach's cross section on the upstream side
    Friend dnshift As Double 'specially for Infoworks: vertical shift of the reach's cross section on the downstream side
    Friend ProfDef As clsProfileDefRecord 'specially for Infoworks: the cross section definition that applies to this reach

    'derived variables
    Friend TotalVolumeUnderSurfaceLevel As Double

    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        bn = New clsSbkReachNode(Me.Setup, Me.SbkCase)
        en = New clsSbkReachNode(Me.Setup, Me.SbkCase)
        NetworkTPBrchRecord = New clsNetworkTPBrchRecord(Me.Setup)
        NetworkcpRecord = New clsNetworkCPRecord(Me.Setup, Me)
        ReachObjects = New clsSbkReachObjects(Me.Setup, Me.SbkCase)
        ReachType = New clsSobekBranchType
    End Sub
    Public Function IsUrban() As Boolean
        Select Case ReachType.ParentReachType
            Case enmReachtype.ReachCFChannel
                Return False
            Case enmReachtype.ReachCFChannelWithLateral
                Return False
            Case enmReachtype.ReachOFDamBreak
                Return False
            Case enmReachtype.ReachOFLine1D2DBoundary
                Return False
            Case enmReachtype.ReachOFLineBoundary
                Return False
            Case enmReachtype.ReachSFDWAPipeWithRunoff
                Return True
            Case enmReachtype.ReachSFInternalCulvert
                Return True
            Case enmReachtype.ReachSFInternalOrifice
                Return True
            Case enmReachtype.ReachSFInternalPump
                Return True
            Case enmReachtype.ReachSFInternalWeir
                Return True
            Case enmReachtype.ReachSFPipe
                Return True
            Case enmReachtype.ReachSFPipeAndComb
                Return True
            Case enmReachtype.ReachSFPipeAndMeas
                Return True
            Case enmReachtype.ReachSFPipeWithRunoff
                Return True
            Case enmReachtype.ReachSFRWAPipeWithRunoff
                Return True
            Case enmReachtype.ReachSFStreet
                Return True
        End Select
    End Function
    Public Function IsPipe() As Boolean
        Select Case ReachType.ParentReachType
            Case enmReachtype.ReachSFDWAPipeWithRunoff, enmReachtype.ReachSFPipe, enmReachtype.ReachSFPipeAndComb, enmReachtype.ReachSFPipeAndMeas, enmReachtype.ReachSFPipeWithRunoff, enmReachtype.ReachSFRWAPipeWithRunoff
                Return True
            Case Else
                Return False
        End Select
    End Function

    Public Function IsChannel() As Boolean
        Select Case ReachType.ParentReachType
            Case enmReachtype.ReachCFChannel, enmReachtype.ReachCFChannelWithLateral
                Return True
            Case Else
                Return False
        End Select
    End Function

    Public Function IsStreet() As Boolean
        Select Case ReachType.ParentReachType
            Case enmReachtype.ReachSFStreet
                Return True
            Case Else
                Return False
        End Select
    End Function

    Public Function CenterPointInPolygon(ByRef myShape As MapWinGIS.Shape) As Boolean
        'this function determines if our reach intersects or crosses with a given polygon
        Try
            'first we convert our reach to a shape
            'Dim myPolyLine As MapWinGIS.Shape = BuildShape(0, getReachLength)

            'we place a (fictional) reach object at the center of our reach and compute its co-ordinates
            Dim centerpoint As New MapWinGIS.Point
            Dim centerobj As New clsSbkReachObject(Me.Setup, Me.SbkCase)
            centerobj.ci = Me.Id
            centerobj.lc = getReachLength() / 2
            centerobj.calcXY()
            centerpoint.x = centerobj.X
            centerpoint.y = centerobj.Y

            'next we performe an intersection with our polygon and return the result
            If myShape.PointInThisPoly(centerpoint) Then
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CenterPointInPolygon of class clsSbkReach: " & ex.Message)
            Return False
        End Try
    End Function
    Public Function PolygonIntersection(ByRef myShape As MapWinGIS.Shape) As Boolean
        'this function determines if our reach intersects or crosses with a given polygon
        Try
            'first we convert our reach to a shape
            Dim myPolyLine As MapWinGIS.Shape = BuildShape(0, getReachLength)

            'next we performe an intersection with our polygon and return the result
            If myPolyLine.Intersects(myShape) Then
                Return True
            ElseIf myPolyLine.Crosses(myShape) Then
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function PolygonIntersection of class clsSbkReach: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function BuildVerdictJSON() As String
        Dim myJSON As String = ""
        Dim Lat As Double, Lon As Double, i As Integer
        Try
            'calculate the coordinate of each vector point
            NetworkcpRecord.CalcCPTable(Me.Id, Me.bn, Me.en)

            'start by writing the properties. then initialize the geometry
            myJSON &= "{ %type%: %Feature%, %properties%: { %ID%: %" & Id & "%, %verdict%: " & CrossSectionVerdict & ",%comment%: %" & CrossSectionOrigin & "%},"
            myJSON &= "%geometry%: {%type%: %MultiLineString%, %coordinates%: [ [ "

            For Each cp As clsSbkVectorPoint In NetworkcpRecord.CPTable.CP
                i += 1
                'convert RD New to WGS84 and write the coordinates
                Me.Setup.GeneralFunctions.RD2WGS84(cp.X, cp.Y, Lat, Lon)
                myJSON &= "[ " & Lon & ", " & Lat & " ]"
                If i < NetworkcpRecord.CPTable.CP.Count Then myJSON &= ","
            Next

            'close geometry and properties
            myJSON &= " ] ] } }"

            myJSON = myJSON.Replace("%", Chr(34))
            Return myJSON
        Catch ex As Exception
            Return String.Empty
        End Try
    End Function

    Public Function getInterpolatedReachesChain() As Dictionary(Of String, clsSbkReach)
        Try
            Dim ReachesChain As New Dictionary(Of String, clsSbkReach)
            Dim dnReach As clsSbkReach = Nothing, upReach As clsSbkReach = Nothing
            Dim Done As Boolean
            Dim myReach As clsSbkReach

            'first add the current reach
            ReachesChain.Add(Me.Id.Trim.ToUpper, Me)

            'then walk in downstream direction
            Done = False
            myReach = Me
            Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
            While Not Done
                If myReach.GetDownstreamInterpolatedReach(ReachesDnProcessed, dnReach) Then
                    If ReachesChain.ContainsKey(dnReach.Id.Trim.ToUpper) Then Exit While 'we have completed a full circle, so it seems
                    ReachesChain.Add(dnReach.Id.Trim.ToUpper, dnReach)
                    myReach = dnReach
                Else
                    Done = True
                End If
            End While

            Done = False
            myReach = Me
            Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
            While Not Done
                If myReach.GetUpstreamInterpolatedReach(ReachesUpProcessed, upReach) Then
                    If ReachesChain.ContainsKey(upReach.Id.Trim.ToUpper) Then Exit While 'we have completed a full circle, so it seems
                    ReachesChain.Add(upReach.Id.Trim.ToUpper, upReach)
                    myReach = upReach
                Else
                    Done = True
                End If
            End While

            Return ReachesChain
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetInterpolatedReaches")
            Return Nothing
        End Try
    End Function



    Public Function GetDownstreamInterpolatedReaches(ByRef dnReaches As Dictionary(Of String, clsSbkReach)) As Boolean
        'given a starting reach, this function walks in downstream direction in order to complete the chain of interpolated reaches
        Try
            Dim Done As Boolean = False
            Dim dnReach As clsSbkReach = Nothing
            Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
            While Not Done
                If GetDownstreamInterpolatedReach(ReachesDnProcessed, dnReach) Then
                    If dnReaches.ContainsKey(dnReach.Id.Trim.ToUpper) Then Return True 'we have completed a full circle, so it seems
                    dnReaches.Add(dnReach.Id.Trim.ToUpper, dnReach)
                End If
            End While
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetDownstreamInterpolatedReaches")
            Return False
        End Try
    End Function

    Public Function GetUpstreamInterpolatedReaches(ByRef upReaches As Dictionary(Of String, clsSbkReach)) As Boolean
        'given a starting reach, this function walks in upstream direction in order to complete the chain of interpolated reaches
        Try
            Dim Done As Boolean = False
            Dim upReach As clsSbkReach = Nothing
            Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
            While Not Done
                If GetUpstreamInterpolatedReach(ReachesUpProcessed, upReach) Then
                    If upReaches.ContainsKey(upReach.Id.Trim.ToUpper) Then Return True 'we have completed a full circle, so it seems
                    upReaches.Add(upReach.Id.Trim.ToUpper, upReach)
                End If
            End While
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetUpstreamInterpolatedReaches")
            Return False
        End Try
    End Function

    Public Function CalculateExtent() As Boolean
        Dim CP As clsSbkVectorPoint
        Dim i As Long
        Try
            'first we need a table of curving points; NOT in the way SOBEK stores it internally but including the actual XY coordinates of the vector points
            NetworkcpRecord.CalcCPTable(Me.Id, Me.bn, Me.en)
            If NetworkcpRecord.CPTable.CP.Count > 0 Then
                CP = NetworkcpRecord.CPTable.CP(i)
                BoundingBox = New clsBoundingBox
                BoundingBox.XLL = CP.X
                BoundingBox.XUR = CP.X
                BoundingBox.YLL = CP.Y
                BoundingBox.YUR = CP.Y
                For i = 1 To NetworkcpRecord.CPTable.CP.Count - 1
                    CP = NetworkcpRecord.CPTable.CP(i)
                    If CP.X < BoundingBox.XLL Then BoundingBox.XLL = CP.X
                    If CP.X > BoundingBox.XUR Then BoundingBox.XUR = CP.X
                    If CP.Y < BoundingBox.YLL Then BoundingBox.YLL = CP.Y
                    If CP.Y > BoundingBox.YUR Then BoundingBox.YUR = CP.Y
                Next
            Else
                Throw New Exception("Error: reach has no vector points: " & Id)
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function CalculateExtent of class clsSbkReach.")
            Return False
        End Try
    End Function

    Public Function AddBeginAndEndNodeAsReachObjects() As Boolean
        Try
            Dim myObj As clsSbkReachObject

            'add the begin node
            myObj = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myObj.ID = bn.ID
            myObj.ci = Id
            myObj.lc = 0
            myObj.nt = enmNodetype.NodeCFGridpointFixed
            myObj.InUse = True
            ReachObjects.ReachObjects.Add(myObj.ID.Trim.ToUpper, myObj)

            'add the end node
            myObj = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myObj.ID = en.ID
            myObj.ci = Id
            myObj.lc = getReachLength()
            myObj.nt = enmNodetype.NodeCFGridpointFixed
            myObj.InUse = True
            ReachObjects.ReachObjects.Add(myObj.ID.Trim.ToUpper, myObj)

            're-sort the reachobjects by chainage
            ReachObjects.Sort()

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function AddBeginAndEndNodeAsReachObjects of class clsSbkReach.")
            Return False
        End Try

    End Function

    Public Function cpCount() As Integer
        Return NetworkcpRecord.Table.XValues.Count
    End Function

    Public Function HasStructure(Optional ByVal StartChainage As Double = 0, Optional ByVal EndChainage As Double = 0) As Boolean
        If EndChainage = 0 Then EndChainage = getReachLength()
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.lc >= StartChainage AndAlso myObj.lc < EndChainage AndAlso Setup.GeneralFunctions.isStructure(myObj.nt) Then Return True
        Next
        Return False
    End Function

    Public Function BuildShape(Optional ByVal StartChainage As Double = 0, Optional ByVal EndChainage As Double = 0) As MapWinGIS.Shape
        If EndChainage = 0 Then EndChainage = getReachLength()
        Dim CP As clsSbkVectorPoint
        Dim NewShape As New MapWinGIS.Shape
        Dim myObj As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        NewShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)
        NetworkcpRecord.CalcCPTable(Id, bn, en)

        'our start point 
        myObj.ci = Id
        myObj.lc = StartChainage
        myObj.calcXY()
        NewShape.AddPoint(myObj.X, myObj.Y)

        'all existing curving points between start and end point
        For i = 0 To NetworkcpRecord.CPTable.CP.Count - 1
            CP = NetworkcpRecord.CPTable.CP(i)
            If CP.Dist > StartChainage AndAlso CP.Dist < EndChainage Then
                NewShape.AddPoint(CP.X, CP.Y)
            End If
        Next

        'the endpoint
        myObj.ci = Id
        myObj.lc = EndChainage
        myObj.calcXY()
        NewShape.AddPoint(myObj.X, myObj.Y)

        Return NewShape
    End Function

    Public Function StorageTableByPolygon(ByRef SF As MapWinGIS.Shapefile, ByVal IDFieldIdx As Integer, ByRef StorageTables As Dictionary(Of String, Dictionary(Of Integer, Double)), ByVal DistanceIncrement As Integer) As Boolean
        'this function walks with the given increment through the entire reach and builds up the storage tables dictionary
        Dim Chainage As Double, X As Double, Y As Double
        Dim ShapeIdx As Integer, ID As String
        Dim myObj As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim myTable As Dictionary(Of Integer, Double)
        Try
            For Chainage = DistanceIncrement / 2 To getReachLength() - DistanceIncrement / 2 Step DistanceIncrement
                'figure out in which shape this location falls
                calcXY(Chainage, X, Y)
                ShapeIdx = SF.PointInShapefile(X, Y)
                ID = SF.CellValue(IDFieldIdx, ShapeIdx)
                'get or create the dictionary for this shapeID
                If Not StorageTables.ContainsKey(ID.Trim.ToUpper) Then
                    myTable = New Dictionary(Of Integer, Double)
                    StorageTables.Add(ID.Trim.ToUpper, myTable)
                Else
                    myTable = StorageTables.Item(ID.Trim.ToUpper)
                End If

                'for this particular location, add the storage table to the table
                Call SupplementStorageTable(Chainage, DistanceIncrement, myTable)

            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function SupplementStorageTable(Chainage As Double, DistanceIncrement As Integer, ByRef StorageTable As Dictionary(Of Integer, Double)) As Boolean
        Try
            'this function supplements a given storage table with the channel storage for a given chainage & length on a given reach
            'it will interpolate between the two surrounding cross sections
            Dim upDist As Double, dnDist As Double
            Dim DownstreamReachesProcessed As New Dictionary(Of String, clsSbkReach)
            Dim UpstreamReachesProcessed As New Dictionary(Of String, clsSbkReach)
            Dim upCrs As clsSbkReachObject = GetUpstreamObject(DownstreamReachesProcessed, Chainage, upDist, False, True, False, False, True)
            Dim dnCrs As clsSbkReachObject = GetDownstreamObject(UpstreamReachesProcessed, Chainage, dnDist, False, True, False, False, True)
            Dim upDat As clsProfileDatRecord = Nothing, upDef As clsProfileDefRecord = Nothing
            Dim dnDat As clsProfileDatRecord = Nothing, dnDef As clsProfileDefRecord = Nothing
            Me.SbkCase.CFData.Data.ProfileData.getProfileDefinition(upCrs.ID, upDat, upDef)
            Me.SbkCase.CFData.Data.ProfileData.getProfileDefinition(dnCrs.ID, dnDat, dnDef)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ContainsreachObjectByID(ID As String) As Boolean
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.ID = ID Then Return True
        Next
        Return False
    End Function
    Public Function ContainsReachObject(NodeType As enmNodetype, FromChainage As Double, ToChainage As Double) As Boolean
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.lc >= FromChainage AndAlso myObj.lc <= ToChainage AndAlso myObj.nt = NodeType Then Return True
        Next
        Return False
    End Function

    Public Function exportToShape() As MapWinGIS.Shape
        Try
            'exports the current reach as an ESRI shape
            Dim reachShape As New MapWinGIS.Shape
            reachShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)

            NetworkcpRecord.CalcCPTable(Me.Id, Me.bn, Me.en)    'build a table that contains ALL vector point.
            For i = 0 To NetworkcpRecord.CPTable.CP.Count - 1
                reachShape.AddPoint(NetworkcpRecord.CPTable.CP(i).X, NetworkcpRecord.CPTable.CP(i).Y)
            Next
            Return reachShape
        Catch ex As Exception
            Me.Setup.Log.AddError("Error converting reach to shape: " & Id)
            Return Nothing
        End Try
    End Function

    Public Function calcBoundBox() As MapWinGIS.Extents
        'new in v1.78
        Try
            If NetworkcpRecord.CPTable.CP.Count = 0 Then NetworkcpRecord.CalcCPTable(Me.Id, Me.bn, Me.en)
            If NetworkcpRecord.CPTable.CP.Count = 0 Then Throw New Exception("Reach has no vector points: " & Id)
            Dim xMin As Double = 9.0E+99
            Dim xMax As Double = -9.0E+99
            Dim yMin As Double = 9.0E+99
            Dim yMax As Double = -9.0E+99

            BoundBox = New MapWinGIS.Extents
            Dim i As Integer
            For i = 0 To NetworkcpRecord.CPTable.CP.Count - 1
                If NetworkcpRecord.CPTable.CP(i).X < xMin Then xMin = NetworkcpRecord.CPTable.CP(i).X
                If NetworkcpRecord.CPTable.CP(i).X > xMax Then xMax = NetworkcpRecord.CPTable.CP(i).X
                If NetworkcpRecord.CPTable.CP(i).Y < yMin Then yMin = NetworkcpRecord.CPTable.CP(i).Y
                If NetworkcpRecord.CPTable.CP(i).Y > yMax Then yMax = NetworkcpRecord.CPTable.CP(i).Y
            Next
            BoundBox.SetBounds(xMin, yMin, 0, xMax, yMax, 0)
            Return BoundBox
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function
    Public Function calcBoundingBox() As Boolean
        Try
            BoundingBox = New clsBoundingBox
            Dim i As Integer
            For i = 0 To NetworkcpRecord.CPTable.CP.Count - 1
                If NetworkcpRecord.CPTable.CP(i).X < BoundingBox.XLL Then BoundingBox.XLL = NetworkcpRecord.CPTable.CP(i).X
                If NetworkcpRecord.CPTable.CP(i).X > BoundingBox.XUR Then BoundingBox.XUR = NetworkcpRecord.CPTable.CP(i).X
                If NetworkcpRecord.CPTable.CP(i).Y < BoundingBox.YLL Then BoundingBox.YLL = NetworkcpRecord.CPTable.CP(i).Y
                If NetworkcpRecord.CPTable.CP(i).Y > BoundingBox.YUR Then BoundingBox.YUR = NetworkcpRecord.CPTable.CP(i).Y
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function getprofileByIndex(ProfIdx As Integer) As clsSbkReachObject
        Dim idx As Integer = -1
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = enmNodetype.SBK_PROFILE Then
                idx += 1
                If idx = ProfIdx Then Return myObj
            End If
        Next
        Return Nothing
    End Function

    Public Function CopyMetaDataFromReach(ByRef myReach As clsSbkReach) As Boolean
        Try
            If Not myReach.CrossSectionOrigin Is Nothing Then CrossSectionOrigin = myReach.CrossSectionOrigin
            If Not myReach.CrossSectionVerdict = Nothing Then CrossSectionVerdict = myReach.CrossSectionVerdict
            If Not myReach.OriginalID = Nothing Then OriginalID = myReach.OriginalID
            If Not myReach.ReachCategory Is Nothing Then ReachCategory = myReach.ReachCategory
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function getextent(ByRef xMin As Double, ByRef yMin As Double, ByRef xMax As Double, ByRef yMax As Double) As Boolean
        Try
            xMin = 9.0E+99
            yMin = 9.0E+99
            xMax = -9.0E+99
            yMax = -9.0E+99

            'first make sure there is a filled cptable object
            If NetworkcpRecord.CPTable.CP.Count = 0 Then NetworkcpRecord.CalcCPTable(Id, bn, en)

            'now walk through each vector point and set the extents
            For Each cp As clsSbkVectorPoint In NetworkcpRecord.CPTable.CP
                If cp.X < xMin Then xMin = cp.X
                If cp.Y < yMin Then yMin = cp.Y
                If cp.X > xMax Then xMax = cp.X
                If cp.Y > yMax Then yMax = cp.Y
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getExtent of class clssbkreach.")
            Return False
        End Try
    End Function

    Public Function Rename(newID As String) As Boolean
        Try
            'this function 'renames'the current reach.
            'well, in fact it does not rename it, but it clones it and moves all objects over to its clone
            'this is because the ID is being used in all sorts of dictionary keys and it's not that easy to adjust.

            If newID.Trim.ToUpper = Id.Trim.ToUpper Then Throw New Exception("Error renaming reach: new ID equals old id: " & Id)
            newID = SbkCase.CFTopo.Reaches.CreateID(newID)

            'update all references to the current reach from its reach objects
            For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
                myObj.ci = newID
            Next

            'update all reference to the current reach from the start- and endnode
            bn.ci = newID
            en.ci = newID

            'update all references to the current reach from the nodes.dat records
            For Each DatRecord As clsNodesDatNODERecord In SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Values
                If Not DatRecord.r1 Is Nothing AndAlso DatRecord.r1.Trim.ToUpper = Id.Trim.ToUpper Then DatRecord.r1 = newID
                If Not DatRecord.r2 Is Nothing AndAlso DatRecord.r2.Trim.ToUpper = Id.Trim.ToUpper Then DatRecord.r2 = newID
            Next

            'remove the 'old' reach
            SbkCase.CFTopo.Reaches.Reaches.Remove(Id.Trim.ToUpper)

            'and add the new one
            Id = newID
            SbkCase.CFTopo.Reaches.Reaches.Add(Id.Trim.ToUpper, Me)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error renaming reach " & Id & " to " & newID)
            Return False
        End Try
    End Function


    Public Function HasInterpolation() As Boolean
        'checks if there is an interpolation record present for this reach that allows interpolation with other reaches.
        With SbkCase.CFData.Data.NodesData.NodesDatNodeRecords
            If .records.ContainsKey(bn.ID.Trim.ToUpper) Then
                Return (.records.Item(bn.ID.Trim.ToUpper).ni = 1)
            ElseIf .records.ContainsKey(en.ID.Trim.ToUpper) Then
                Return (.records.Item(en.ID.Trim.ToUpper).ni = 1)
            Else
                Return False
            End If
        End With
    End Function

    Public Function disableAllCrossSections(AddToMessage As String) As Boolean
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = enmNodetype.SBK_PROFILE Then
                Me.Setup.Log.AddMessage("Cross Section " & myObj.ID & " has been disabled in the building process: " & AddToMessage)
                myObj.InUse = False
            End If
        Next
    End Function

    Public Function getFirstVectorAngle(ByRef Angle As Double) As Boolean
        Try
            If Not NetworkcpRecord Is Nothing Then
                If NetworkcpRecord.Table.XValues.Count > 0 Then
                    Angle = NetworkcpRecord.Table.Values1.Values(0)
                    Return True
                ElseIf NetworkcpRecord.CPTable.CP.Count = 2 Then
                    If getReachLength() = 0 Then
                        'this is a reach with zero length. Throw an error!
                        Throw New Exception("Severe error: reach with ID " & Id & " has zero length!")
                    Else
                        'this reach consists of just two points: start point and end point
                        'v1.870 added this exception for situations where we do not have any curving points
                        Angle = Me.Setup.GeneralFunctions.LineAngleDegrees(bn.X, bn.Y, en.X, en.Y)
                        Return True
                    End If
                End If
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error retrieving angle of first vector for reach " & Id)
            Return False
        End Try
    End Function


    Public Function getVectorAngle(Chainage As Double, ByRef Angle As Double) As Boolean
        Try
            If Not NetworkcpRecord Is Nothing Then
                If NetworkcpRecord.Table.XValues.Count > 0 Then
                    If NetworkcpRecord.CPTable.CP.Count = 0 Then NetworkcpRecord.CalcCPTable(Id, bn, en)    '(re)calculate the cp-table if empty
                    For i = 0 To NetworkcpRecord.CPTable.CP.Count - 1
                        If Chainage >= NetworkcpRecord.CPTable.CP(i).Dist Then
                            Angle = NetworkcpRecord.CPTable.CP(i).Angle
                        Else
                            Exit For
                        End If
                    Next
                    Return True
                End If
            End If
            Throw New Exception("Could not get angle for chainage " & Chainage & " on reach " & Id)
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getVectorAngle of class clsSbkReach")
            Return False
        End Try
    End Function

    Public Function getLastVectorAngle(ByRef Angle As Double) As Boolean
        Try
            If Not NetworkcpRecord Is Nothing Then
                If NetworkcpRecord.Table.XValues.Count > 0 Then
                    Angle = NetworkcpRecord.Table.Values1.Values(NetworkcpRecord.Table.XValues.Count - 1)
                    Return True
                ElseIf NetworkcpRecord.CPTable.CP.Count = 2 Then
                    If getReachLength() = 0 Then
                        'this is a reach with zero length. Throw an error!
                        Throw New Exception("Severe error: reach with ID " & Id & " has zero length!")
                    Else
                        'this reach consists of just two points: start point and end point
                        'v1.870 added this exception for situations where we do not have any curving points
                        Angle = Me.Setup.GeneralFunctions.LineAngleDegrees(bn.X, bn.Y, en.X, en.Y)
                        Return True
                    End If
                End If
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error retrieving angle of last vector for reach " & Id)
            Return False
        End Try
    End Function


    Public Function MoveAllObjects(ByRef TargetReach As clsSbkReach, ScaleChainageByReachLength As Boolean) As Boolean
        'this function migrates all reach objects on the current reach to another reach
        'optional is to scale the chainages by the reach lengths
        Try
            Dim curLength As Double = getReachLength()
            Dim targetLength As Double = TargetReach.getReachLength()
            For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
                myObj.ci = TargetReach.Id
                myObj.lc = myObj.lc * targetLength / curLength
                TargetReach.ReachObjects.Add(myObj)
            Next
            ReachObjects.ReachObjects.Clear()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function GetIncomingReaches(ByRef myNode As clsSbkReachNode) As List(Of clsSbkReach)
        Dim myList As New List(Of clsSbkReach)
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            If myReach.InUse AndAlso myReach.en.ID = myNode.ID AndAlso Not myReach.Id = Id Then
                myList.Add(myReach)
            End If
        Next
        Return myList
    End Function

    Public Function GetOutgoingReaches(ByRef myNode As clsSbkReachNode) As List(Of clsSbkReach)
        Dim myList As New List(Of clsSbkReach)
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            If myReach.InUse AndAlso myReach.bn.ID = myNode.ID AndAlso Not myReach.Id = Id Then
                myList.Add(myReach)
            End If
        Next
        Return myList
    End Function

    Public Function getNeighborReachNearestAngle(Upstream As Boolean, AngleMarginDegrees As Double, SkipReachIDs As List(Of String)) As clsSbkReach
        'this function returns the neighbor reach with the angle that is nearest to that of the current reach
        Try
            'make a collection of incoming reaches and a collection of outgoing reaches
            Dim ReachesIn As New List(Of clsSbkReach)
            Dim ReachesOut As New List(Of clsSbkReach)
            Dim AnglesIn As New List(Of Double)
            Dim AnglesOut As New List(Of Double)
            Dim myAngle As Double, curAngle As Double
            Dim myReach As clsSbkReach = Nothing

            If Upstream Then
                ReachesIn = GetIncomingReaches(bn)
                ReachesOut = GetOutgoingReaches(bn)
                If Not getFirstVectorAngle(curAngle) Then Throw New Exception("Error retrieving angle for the first vector of reach " & Id)
            Else
                ReachesIn = GetIncomingReaches(en)
                ReachesOut = GetOutgoingReaches(en)
                If Not getLastVectorAngle(curAngle) Then Throw New Exception("Error retrieving angle for the last vector of reach " & Id)
            End If

            'figure out the most logical combination
            For i = 0 To ReachesIn.Count - 1
                If Not ReachesIn(i).getLastVectorAngle(myAngle) Then Me.Setup.Log.AddError("Error retrieving angle for last vector of reach " & ReachesIn(i).Id & ". Reach interpolation over nodes might have been gone wrong.")
                AnglesIn.Add(myAngle)
            Next
            For i = 0 To ReachesOut.Count - 1
                If Not ReachesOut(i).getFirstVectorAngle(myAngle) Then Me.Setup.Log.AddError("Error retrieving angle for first vector of reach " & ReachesIn(i).Id & ". Reach interpolation over nodes might have been gone wrong.")
                AnglesOut.Add(myAngle)
            Next

            'now walk through all combinations and keep the best!
            'however: for downstream connection we have a slight preference for outgoing reaches; for upstream connection for incoming reaches
            Dim minAngleDiff As Double = 999

            If Upstream Then
                'for upstream connection we have a slight preference for incoming reaches. therefore we handle them first
                'we'll use a margin, giving the first attempt a slight advantage
                findNearestAngle(ReachesIn, AnglesIn, curAngle, myReach, minAngleDiff, AngleMarginDegrees, SkipReachIDs)
                findNearestAngle(ReachesOut, AnglesOut, curAngle, myReach, minAngleDiff, AngleMarginDegrees, SkipReachIDs)
            Else
                'for downstream connection we have a slight preference for outgoing reaches. therefore we handle them first
                'we'll use a margin, giving the first attempt a slight advantage
                findNearestAngle(ReachesOut, AnglesOut, curAngle, myReach, minAngleDiff, AngleMarginDegrees, SkipReachIDs)
                findNearestAngle(ReachesIn, AnglesIn, curAngle, myReach, minAngleDiff, AngleMarginDegrees, SkipReachIDs)
            End If

            Return myReach
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getUpstreamReachNearestAngle when processing reach " & Id)
            Return Nothing
        End Try

    End Function

    Public Function findNearestAngle(ByRef ReachCollection As List(Of clsSbkReach), Angles As List(Of Double), ReferenceAngle As Double, ByRef myReach As clsSbkReach, ByRef minAngleDiff As Double, MarginDegrees As Double, SkipReachIDs As List(Of String)) As Boolean
        Try
            Dim i As Integer, myAngleDiff As Double
            For i = 0 To ReachCollection.Count - 1

                'v2.112: new skip reaches that 
                If Not SkipReachIDs.Contains(ReachCollection.Item(i).Id.Trim.ToUpper) Then
                    'first try for situations where both reaches would have the same direction
                    myAngleDiff = Setup.GeneralFunctions.AngleDifferenceDegrees(Angles(i), ReferenceAngle)
                    If myAngleDiff < (minAngleDiff - MarginDegrees) Then
                        minAngleDiff = myAngleDiff
                        myReach = ReachCollection.Item(i)
                    End If

                    'then also try for situations where both reaches have opposite directions
                    myAngleDiff = Setup.GeneralFunctions.AngleDifferenceDegrees(Angles(i), Setup.GeneralFunctions.NormalizeAngle(ReferenceAngle - 180))
                    If myAngleDiff < (minAngleDiff - MarginDegrees) Then
                        minAngleDiff = myAngleDiff
                        myReach = ReachCollection.Item(i)
                    End If
                End If

            Next
            Return False
        Catch ex As Exception
            Return True
        End Try

    End Function


    Public Function GetUpstreamReachesSameDirection() As List(Of clsSbkReach)
        Try
            Dim myList As New List(Of clsSbkReach)
            For Each tmpReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                If tmpReach.InUse AndAlso Not tmpReach.Id = Id Then
                    If tmpReach.en.ID = bn.ID Then
                        myList.Add(tmpReach)
                    End If
                End If
            Next
            Return myList
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function GetUpstreamReachesReverse() As List(Of clsSbkReach)
        Try
            Dim myList As New List(Of clsSbkReach)
            For Each tmpReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                If tmpReach.InUse AndAlso Not tmpReach.Id = Id Then
                    If tmpReach.bn.ID = bn.ID Then
                        myList.Add(tmpReach)
                    End If
                End If
            Next
            Return myList
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function GetDownstreamReachesSameDirection() As List(Of clsSbkReach)
        Try
            Dim myList As New List(Of clsSbkReach)
            For Each tmpReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                If tmpReach.InUse AndAlso Not tmpReach.Id = Id Then
                    If tmpReach.bn.ID = en.ID Then
                        myList.Add(tmpReach)
                    End If
                End If
            Next
            Return myList
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function GetDownstreamReachesReverse() As List(Of clsSbkReach)
        Try
            Dim myList As New List(Of clsSbkReach)
            For Each tmpReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                If tmpReach.InUse AndAlso Not tmpReach.Id = Id Then
                    If tmpReach.en.ID = en.ID Then
                        myList.Add(tmpReach)
                    End If
                End If
            Next
            Return myList
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function getTargetLevelsCollectionFromShapefile(ByRef TargetLevels As Dictionary(Of String, clsTargetLevels), Chainage As Double, MaxDistUp As Double, MaxDistDown As Double, ByVal IncludeConnectedReaches As Boolean, SkipPointsOutsideShapefile As Boolean) As Boolean
        Try
            Dim myLoc As New clsSbkReachObject(Me.Setup, Me.SbkCase)
            Dim myReach As clsSbkReach = Nothing
            Dim ShapeIdx As Long
            Dim newTL As clsTargetLevels
            Dim myKey As String = ""
            myLoc.ci = Id

            With Setup.GISData.SubcatchmentDataSource

                'start by retrieving the target levels for the initial location
                myLoc.lc = Chainage
                myLoc.calcXY()
                .GetRecordIdxByCoord(myLoc.X, myLoc.Y, ShapeIdx)
                If Not (ShapeIdx < 0 AndAlso SkipPointsOutsideShapefile) Then

                    newTL = New clsTargetLevels
                    newTL.setShapeID(.GetTextValue(ShapeIdx, enmInternalVariable.ID))
                    If Not Setup.GISData.SubcatchmentDataSource.getTargetLevels(ShapeIdx, newTL) Then Throw New Exception("Error retrieving target levels for subcatchment " & newTL.GetID)
                    myKey = myLoc.ci.Trim.ToUpper & Math.Round(myLoc.lc, 2).ToString.Trim
                    If Not TargetLevels.ContainsKey(myKey) Then TargetLevels.Add(myKey, newTL)

                    If MaxDistUp <> 0 Then
                        myLoc.lc = Chainage - Math.Abs(MaxDistUp)
                        If myLoc.lc < 0 Then

                            'apparently we'll jump over the starting node for this reach, so also
                            'add the startnode for this reach, but only if we didn't already investigate it already in the previous step
                            If Chainage <> 0 Then
                                .GetRecordIdxByCoord(bn.X, bn.Y, ShapeIdx)
                                newTL = New clsTargetLevels
                                newTL.setShapeID(.GetTextValue(ShapeIdx, enmInternalVariable.ID))
                                If Not Setup.GISData.SubcatchmentDataSource.getTargetLevels(ShapeIdx, newTL) Then Throw New Exception("Error retrieving target levels for subcatchment " & newTL.GetID)
                                myKey = bn.ID.Trim.ToUpper
                                If Not TargetLevels.ContainsKey(myKey) Then TargetLevels.Add(myKey, newTL)
                            End If

                            'if allowed, jump over the connection node and investigate sections on all connected reaches
                            If IncludeConnectedReaches Then
                                Dim upLocs As New List(Of clsSbkReachObject)
                                upLocs = getLocationsFromUpstreamReaches(-myLoc.lc)
                                For Each upLoc As clsSbkReachObject In upLocs
                                    upLoc.calcXY()
                                    .GetRecordIdxByCoord(upLoc.X, upLoc.Y, ShapeIdx)
                                    newTL = New clsTargetLevels
                                    newTL.setShapeID(.GetTextValue(ShapeIdx, enmInternalVariable.ID))
                                    If Not Setup.GISData.SubcatchmentDataSource.getTargetLevels(ShapeIdx, newTL) Then Throw New Exception("Error retrieving target levels for subcatchment " & newTL.GetID)
                                    myKey = upLoc.ci.Trim.ToUpper & Math.Round(upLoc.lc, 2).ToString.Trim
                                    If Not TargetLevels.ContainsKey(myKey) Then TargetLevels.Add(myKey, newTL)
                                Next
                            End If
                        Else
                            myLoc.calcXY()
                            .GetRecordIdxByCoord(myLoc.X, myLoc.Y, ShapeIdx)
                            newTL = New clsTargetLevels
                            newTL.setShapeID(.GetTextValue(ShapeIdx, enmInternalVariable.ID))
                            If Not Setup.GISData.SubcatchmentDataSource.getTargetLevels(ShapeIdx, newTL) Then Throw New Exception("Error retrieving target levels for subcatchment " & newTL.GetID)
                            myKey = myLoc.ci.Trim.ToUpper & Math.Round(myLoc.lc, 2).ToString.Trim
                            If Not TargetLevels.ContainsKey(myKey) Then TargetLevels.Add(myKey, newTL)
                        End If
                    End If

                    If MaxDistDown <> 0 Then
                        myLoc.lc = Chainage + Math.Abs(MaxDistDown)
                        If myLoc.lc > getReachLength() Then
                            myLoc.lc -= getReachLength()

                            'apparently we'll jump over the end node for this reach, so also
                            'add the end node for this reach, but only if we didn't already investigate it already in the previous step
                            If Chainage <> getReachLength() Then
                                .GetRecordIdxByCoord(en.X, en.Y, ShapeIdx)
                                newTL = New clsTargetLevels
                                newTL.setShapeID(.GetTextValue(ShapeIdx, enmInternalVariable.ID))
                                If Not Setup.GISData.SubcatchmentDataSource.getTargetLevels(ShapeIdx, newTL) Then Throw New Exception("Error retrieving target levels for subcatchment " & newTL.GetID)
                                myKey = en.ID.Trim.ToUpper
                                If Not TargetLevels.ContainsKey(myKey) Then TargetLevels.Add(myKey, newTL)
                            End If

                            'if allowed, jump over the connection node and investigate sections on all connected reaches
                            If IncludeConnectedReaches Then
                                Dim dnLocs As New List(Of clsSbkReachObject)
                                dnLocs = getLocationsFromDownstreamReaches(myLoc.lc)
                                For Each dnLoc As clsSbkReachObject In dnLocs
                                    dnLoc.calcXY()
                                    .GetRecordIdxByCoord(dnLoc.X, dnLoc.Y, ShapeIdx)
                                    newTL = New clsTargetLevels
                                    newTL.setShapeID(.GetTextValue(ShapeIdx, enmInternalVariable.ID))
                                    If Not Setup.GISData.SubcatchmentDataSource.getTargetLevels(ShapeIdx, newTL) Then Throw New Exception("Error retrieving target levels for subcatchment " & newTL.GetID)
                                    myKey = dnLoc.ci.Trim.ToUpper & Math.Round(dnLoc.lc, 2).ToString.Trim
                                    If Not TargetLevels.ContainsKey(myKey) Then TargetLevels.Add(myKey, newTL)
                                Next
                            End If
                        Else
                            myLoc.calcXY()
                            .GetRecordIdxByCoord(myLoc.X, myLoc.Y, ShapeIdx)
                            newTL = New clsTargetLevels
                            newTL.setShapeID(.GetTextValue(ShapeIdx, enmInternalVariable.ID))
                            If Not Setup.GISData.SubcatchmentDataSource.getTargetLevels(ShapeIdx, newTL) Then Throw New Exception("Error retrieving target levels for subcatchment " & newTL.GetID)
                            myKey = myLoc.ci.Trim.ToUpper & Math.Round(myLoc.lc, 2).ToString.Trim
                            If Not TargetLevels.ContainsKey(myKey) Then TargetLevels.Add(myKey, newTL)
                        End If
                    End If
                End If
            End With

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getTargetLevelsCollectionFromShapefile.")
            Return False
        End Try
    End Function

    Public Function GetPolygonShapeIdx(ByRef PolygonShapefile As clsShapeFile, Chainage As Double) As Integer
        Try
            Dim myLoc As New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myLoc.ci = Id
            myLoc.lc = Chainage
            myLoc.calcXY()
            Return PolygonShapefile.GetShapeIdxByCoord(myLoc.X, myLoc.Y)
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetPolygonShapeIdx while processing reach " & Id & " at chainage " & Chainage)
            Return Nothing
        End Try
    End Function


    Public Function getNearestAdjacentSubCatchmentShapes(ByRef UpShapeIdx As Integer, ByRef DnShapeIdx As Integer, ByRef UpReachID As String, ByRef UpReachChainage As Double, ByRef DnReachID As String, ByRef DnReachChainage As Double, StartChainage As Double, MaxSearchRadius As Double, ReachesMaxSearchDepth As Integer) As Boolean
        'this function searches for the two adjacent subcatchment shapes nearest to a given point on a reach
        'it walks in steps of 1m from 0 to a given maximum
        'it returns byref the shape index numbers for the shapes found
        Try
            Dim myLoc As New clsSbkReachObject(Me.Setup, Me.SbkCase)
            Dim upReach As clsSbkReach = Nothing
            Dim dnReach As clsSbkReach = Nothing
            Dim tmpShapeIdx As Integer = -1
            Dim radius As Integer
            Dim upChainage As Double, dnChainage As Double
            Dim nReachesSearched As Integer
            If ReachesMaxSearchDepth = 0 Then ReachesMaxSearchDepth = 1 'there is ALWAYS at least one reach to search

            UpShapeIdx = -1
            DnShapeIdx = -1

            'step from 0 distance to max search radius from the startlocation
            For radius = 0 To MaxSearchRadius
                If radius = 0 Then
                    'there is nothing to search yet. initialize the output by the starting point
                    myLoc.ci = Id
                    myLoc.lc = StartChainage
                    myLoc.calcXY()
                    Setup.GISData.SubcatchmentDataSource.GetRecordIdxByCoord(myLoc.X, myLoc.Y, UpShapeIdx)
                    DnShapeIdx = UpShapeIdx
                    UpReachID = Id
                    DnReachID = Id
                    UpReachChainage = StartChainage
                    DnReachChainage = StartChainage
                Else
                    'first search the upstream side
                    nReachesSearched = 0
                    Dim UpstreamReachesSearched As New List(Of String)
                    UpstreamReachesSearched.Add(Me.Id.Trim.ToUpper)
                    If Not getUpstreamLocation(StartChainage, Math.Abs(radius), upReach, upChainage, UpstreamReachesSearched) Then Throw New Exception("Error retrieving adjacent subcatchment shapes from shapefile.")
                    myLoc.ci = upReach.Id
                    myLoc.lc = upChainage
                    myLoc.calcXY()
                    Setup.GISData.SubcatchmentDataSource.GetRecordIdxByCoord(myLoc.X, myLoc.Y, tmpShapeIdx)

                    'make sure we have a valid shape index for both the upstream and downstream point
                    If tmpShapeIdx >= 0 Then

                        If UpShapeIdx < 0 Then
                            UpShapeIdx = tmpShapeIdx
                            UpReachID = upReach.Id
                            UpReachChainage = upChainage
                        End If

                        If DnShapeIdx < 0 Then
                            DnShapeIdx = tmpShapeIdx
                            DnReachID = dnReach.Id
                            DnReachChainage = dnChainage
                        End If

                        If tmpShapeIdx <> DnShapeIdx Then
                            UpShapeIdx = tmpShapeIdx
                            UpReachID = upReach.Id
                            UpReachChainage = upChainage
                            Return True
                        End If
                    End If

                    'then search the downstream side
                    nReachesSearched = 0
                    Dim DownstreamReachesSearched As New List(Of String)
                    DownstreamReachesSearched.Add(Me.Id.Trim.ToUpper)
                    If Not getDownstreamLocation(StartChainage, Math.Abs(radius), dnReach, dnChainage, DownstreamReachesSearched) Then Throw New Exception("Error retrieving adjacent subcatchment shapes from shapefile.")
                    myLoc.ci = dnReach.Id
                    myLoc.lc = dnChainage
                    myLoc.calcXY()
                    Setup.GISData.SubcatchmentDataSource.GetRecordIdxByCoord(myLoc.X, myLoc.Y, tmpShapeIdx)

                    'make sure we have a valid shape index for both the upstream and downstream point
                    If tmpShapeIdx >= 0 Then

                        If UpShapeIdx < 0 Then
                            UpShapeIdx = tmpShapeIdx
                            UpReachID = upReach.Id
                            UpReachChainage = upChainage
                        End If

                        If DnShapeIdx < 0 Then
                            DnShapeIdx = tmpShapeIdx
                            DnReachID = dnReach.Id
                            DnReachChainage = dnChainage
                        End If

                        If tmpShapeIdx <> UpShapeIdx Then
                            DnShapeIdx = tmpShapeIdx
                            DnReachID = dnReach.Id
                            DnReachChainage = dnChainage
                            Return True
                        End If
                    End If
                End If
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getNearestAdjacentSubCatchmentShapes.")
            Return False
        End Try
    End Function


    Public Function getTargetLevelsFromSubcatchmentsDataSource(ByRef TargetLevels As clsTargetLevels, Chainage As Double, ChainageShift As Double, ByRef ShapeIdx As Integer, ByVal ObjectID As String) As Boolean
        Try
            Dim myLoc As New clsSbkReachObject(Me.Setup, Me.SbkCase)
            Dim myReach As clsSbkReach = Nothing
            Dim NetChainage As Double = Chainage + ChainageShift
            Dim ChainageUpstreamReach As Double = 0
            Dim ChainageDownstreamReach As Double = 0
            Dim UpstreamReachesSearched As New List(Of String), DownstreamReachesSearched As New List(Of String)

            If NetChainage < 0 Then
                If getUpstreamLocation(Chainage, Math.Abs(ChainageShift), myReach, ChainageUpstreamReach, UpstreamReachesSearched) Then
                    'set the location to pick the target levels from and calculate the x and y coordinate
                    myLoc.ci = myReach.Id
                    myLoc.lc = ChainageUpstreamReach
                    myLoc.calcXY()
                Else
                    'no upstream location was found by following the channels. Therefore we will extrapolate the first segment and calculate XY from that
                    'v2.112: when an upstream shape is not found, we do a second attempt by extrapolating beyond the extent of our reach
                    myReach.calcXY(NetChainage, myLoc.X, myLoc.Y)
                End If
            ElseIf NetChainage > getReachLength() Then
                If getDownstreamLocation(Chainage, Math.Abs(ChainageShift), myReach, ChainageDownstreamReach, DownstreamReachesSearched) Then
                    'set the location to pick the target levels from and calculate the x and y coordinate
                    myLoc.ci = myReach.Id
                    myLoc.lc = ChainageDownstreamReach
                    myLoc.calcXY()
                Else
                    'v2.112: when a downstream shape is not found, we do a second attempt by extrapolating beyond the extent of our reach
                    myReach.calcXY(NetChainage, myLoc.X, myLoc.Y)
                End If
            Else
                'the required chainage is located on our reach. So this should be easy
                myReach = Me
                myLoc.ci = myReach.Id
                myLoc.lc = NetChainage
                myLoc.calcXY()
            End If

            'find the target levels


            With Setup.GISData.SubcatchmentDataSource
                .GetRecordIdxByCoord(myLoc.X, myLoc.Y, ShapeIdx)
                If ShapeIdx < 0 Then
                    Me.Setup.Log.AddWarning("No underlying shape found while searching target levels for object " & ObjectID & " at a distance of " & ChainageShift & " in subcatchments datasource. Co-ordinate " & myLoc.X & "," & myLoc.Y & ". Target levels not found.")
                    Return False
                End If
                TargetLevels.setShapeID(.GetTextValue(ShapeIdx, enmInternalVariable.ID))
                If Not Setup.GISData.SubcatchmentDataSource.getTargetLevels(ShapeIdx, TargetLevels) Then Throw New Exception("Error retrieving target levels for subcatchment " & TargetLevels.GetID)
            End With

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getTargetLevelsFromShapefile.")
            Return False
        End Try
    End Function

    Public Function getInundationLevelFromSubcatchmentsDataSource(ByRef InundationLevel As Double, Chainage As Double, ChainageShift As Double) As Boolean
        Try
            Dim myLoc As New clsSbkReachObject(Me.Setup, Me.SbkCase)
            Dim myReach As clsSbkReach = Nothing
            Dim ShapeIdx As Long
            Dim myChainage As Double
            Dim UpstreamReachesSearched As New List(Of String)
            UpstreamReachesSearched.Add(Me.Id.Trim.ToUpper)
            Dim DownstreamReachesSearched As New List(Of String)
            DownstreamReachesSearched.Add(Me.Id.Trim.ToUpper)

            If ChainageShift = 0 Then
                myReach = Me
                myChainage = Chainage
            ElseIf ChainageShift < 0 Then
                If getUpstreamLocation(Chainage, Math.Abs(ChainageShift), myReach, myChainage, UpstreamReachesSearched) Then
                    'we found a valid upstream location so we can retrieve the target levels for it
                    'set the location to pick the inundation level from and calculate the x and y coordinate
                    myLoc.ci = myReach.Id
                    myLoc.lc = myChainage
                    myLoc.calcXY()
                Else
                    'so an upstream location was not found by following the channels. Therefore we will extrapolate the first segment and calculate XY from that
                    'v2.112: when an upstream shape is not found, we do a second attempt by extrapolating beyond the extent of our reach
                    myReach.calcXY(Chainage + ChainageShift, myLoc.X, myLoc.Y)
                End If
            End If
            If getDownstreamLocation(Chainage, Math.Abs(ChainageShift), myReach, myChainage, DownstreamReachesSearched) Then
                'we found a valid downstream location so we can retrieve the target levels for it
                myLoc.ci = myReach.Id
                myLoc.lc = myChainage
                myLoc.calcXY()
            Else
                'v2.112: when a downstream shape is not found, we do a second attempt by extrapolating beyond the extent of our reach
                myReach.calcXY(Chainage + ChainageShift, myLoc.X, myLoc.Y)
            End If

            'find the inundation level
            With Setup.GISData.SubcatchmentDataSource
                .GetRecordIdxByCoord(myLoc.X, myLoc.Y, ShapeIdx)
                InundationLevel = .GetNumericalValue(ShapeIdx, enmInternalVariable.InundationLevel, enmMessageType.None)
            End With

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getInundationLevelFromShapefile.")
            Return False
        End Try
    End Function


    Public Sub replaceStartingNode(ByRef newNode As clsSbkReachNode)
        'this routine replaces the existing beginning node of a reach by another
        'in order to do so it will have to adjust the vector points too

        'first calculate the vector point table in XY
        If NetworkcpRecord.CPTable.CP.Count = 0 Then
            NetworkcpRecord.CalcCPTable(Id, bn, en)
        End If

        'then change the first coordinate
        bn = newNode
        NetworkcpRecord.CPTable.CP(0).X = bn.X
        NetworkcpRecord.CPTable.CP(0).Y = bn.Y
        NetworkcpRecord.CPTable.CP(0).Angle = Setup.GeneralFunctions.LineAngleDegrees(bn.X, bn.Y, NetworkcpRecord.CPTable.CP(1).X, NetworkcpRecord.CPTable.CP(1).Y)

        'then recalculate the original vector table
        NetworkcpRecord.CalcTable()

    End Sub


    Public Sub replaceEndingNode(ByRef newNode As clsSbkReachNode)
        'this routine replaces the existing beginning node of a reach by another
        'in order to do so it will have to adjust the vector points too

        'first calculate the vector point table in XY
        If NetworkcpRecord.CPTable.CP.Count = 0 Then
            NetworkcpRecord.CalcCPTable(Id, bn, en)
        End If

        'then change the last vector
        en = newNode
        NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1).X = en.X
        NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1).Y = en.Y
        NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 2).Angle = Setup.GeneralFunctions.LineAngleDegrees(NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 2).X, NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 2).Y, en.X, en.Y)

        'then recalculate the original vector table
        NetworkcpRecord.CalcTable()

    End Sub

    Public Function RemoveReachInterpolation() As Boolean
        'this routine removes all reach interpolations to and from this reach
        Dim myNode As clsNodesDatNODERecord
        With SbkCase.CFData.Data.NodesData.NodesDatNodeRecords

            'upstream node
            If .records.ContainsKey(bn.ID.Trim.ToUpper) Then
                myNode = .records.Item(bn.ID.Trim.ToUpper)
                'if there is node interpolation AND this reach is involved, disable it
                If myNode.ni = 1 AndAlso (myNode.r1.Trim.ToUpper = Id.Trim.ToUpper OrElse myNode.r2.Trim.ToUpper = Id.Trim.ToUpper) Then
                    myNode.ni = 0
                End If
            End If

            'downstream node
            If .records.ContainsKey(en.ID.Trim.ToUpper) Then
                myNode = .records.Item(en.ID.Trim.ToUpper)
                'if there is node interpolation AND this reach is involved, disable it
                If myNode.ni = 1 AndAlso (myNode.r1.Trim.ToUpper = Id.Trim.ToUpper OrElse myNode.r2.Trim.ToUpper = Id.Trim.ToUpper) Then
                    myNode.ni = 0
                End If
            End If

        End With

    End Function

    Public Function AddNewReachObject(ByVal NameBase As String, ByVal NodeType As STOCHLIB.GeneralFunctions.enmNodetype, ByVal Chainage As Double, ByVal AllowPlacementOnNeighbourReach As Boolean, ByVal SetInuse As Boolean, Optional ByVal MinimumDistanceOnReach As Double = 1) As clsSbkReachObject
        Dim ReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim neighborReach As clsSbkReach
        ReachObject.ID = SbkCase.CFTopo.MakeUniqueNodeID(NameBase)
        ReachObject.InUse = SetInuse
        ReachObject.nt = NodeType
        ReachObject.ci = Id

        Dim UpstreamReachesSearched As New List(Of String)
        UpstreamReachesSearched.Add(Me.Id.Trim.ToUpper)
        Dim DownstreamReachesSearched As New List(Of String)
        DownstreamReachesSearched.Add(Me.Id.Trim.ToUpper)

        'if the chainage is invalid, find the most suitable neighbor to place our reach object on!
        'note: for now we stick to the minimum distance on the reach. We won't move our objects further on a neighboring reach
        If getReachLength() < MinimumDistanceOnReach Then
            ReachObject.lc = Chainage
            ReachObjects.Add(ReachObject)
            Me.Setup.Log.AddWarning("Warning: reach " & Id & " has a length < minimum distance for objects: " & MinimumDistanceOnReach & "m. The object " & ReachObject.ID & " was placed at its original distance of " & Chainage & "m to make it possible anyway.")
            Return ReachObject
        ElseIf Chainage < MinimumDistanceOnReach Then
            If AllowPlacementOnNeighbourReach Then

                neighborReach = getNeighborReachNearestAngle(True, 0, UpstreamReachesSearched)
                If Not neighborReach Is Nothing Then
                    ReachObject.ci = neighborReach.Id
                    If neighborReach.bn.ID = bn.ID Then
                        ReachObject.lc = MinimumDistanceOnReach
                        neighborReach.ReachObjects.Add(ReachObject)
                    Else
                        ReachObject.lc = neighborReach.getReachLength - MinimumDistanceOnReach
                        neighborReach.ReachObjects.Add(ReachObject)
                    End If
                    Return ReachObject
                Else
                    ReachObject.lc = MinimumDistanceOnReach
                    ReachObjects.Add(ReachObject)
                    Me.Setup.Log.AddMessage("Reach object " & ReachObject.ID & " had a chainage < " & MinimumDistanceOnReach & "m. The object was placed at " & MinimumDistanceOnReach & "m from the beginning of the reach instead.")
                    Return ReachObject
                End If
            Else
                ReachObject.lc = MinimumDistanceOnReach
                ReachObjects.Add(ReachObject)
                Me.Setup.Log.AddMessage("Reach object " & ReachObject.ID & " had a chainage < " & MinimumDistanceOnReach & "m. The object was placed at " & MinimumDistanceOnReach & "m from the beginning of the reach instead.")
                Return ReachObject
            End If

        ElseIf Chainage > getReachLength() - MinimumDistanceOnReach Then

            If AllowPlacementOnNeighbourReach Then
                neighborReach = getNeighborReachNearestAngle(False, 0, DownstreamReachesSearched)
                If Not neighborReach Is Nothing Then
                    ReachObject.ci = neighborReach.Id
                    If neighborReach.bn.ID = en.ID Then
                        ReachObject.lc = MinimumDistanceOnReach
                        neighborReach.ReachObjects.Add(ReachObject)
                    Else
                        ReachObject.lc = neighborReach.getReachLength - MinimumDistanceOnReach
                        neighborReach.ReachObjects.Add(ReachObject)
                    End If
                    Return ReachObject
                Else
                    ReachObject.lc = getReachLength() - MinimumDistanceOnReach
                    ReachObjects.Add(ReachObject)
                    Me.Setup.Log.AddMessage("Reach object " & ReachObject.ID & " had a chainage > reach length - " & MinimumDistanceOnReach & "m. The object was placed at " & MinimumDistanceOnReach & "m from the end of the reach instead.")
                    Return ReachObject
                End If
            Else
                ReachObject.lc = getReachLength() - MinimumDistanceOnReach
                ReachObjects.Add(ReachObject)
                Me.Setup.Log.AddMessage("Reach object " & ReachObject.ID & " had a chainage > reach length - " & MinimumDistanceOnReach & "m. The object was placed at " & MinimumDistanceOnReach & "m from the end of the reach instead.")
                Return ReachObject
            End If

        Else
            ReachObject.lc = Chainage
            ReachObjects.Add(ReachObject)
            Return ReachObject
        End If

        'if we end up here something's wrong.
        Me.Setup.Log.AddError("Error adding reach object " & ReachObject.ID & " to reach " & Id & " at chainage " & Chainage & ".")
        Return Nothing
    End Function


    Public Function AddNewCrossSection(ByVal NameBase As String, ByVal CrossSectionType As STOCHLIB.GeneralFunctions.enmProfileType, ByVal Chainage As Double, ByVal AllowPlacementOnNeighbourReach As Boolean, ByVal SetInuse As Boolean, Optional ByVal MinimumDistanceOnReach As Double = 1) As clsSbkReachObject
        'this function creates a new reachobject of the type SBK_PROFILE and adds it on the required location on the current reach
        Dim ReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim neighborReach As clsSbkReach
        ReachObject.ID = SbkCase.CFTopo.MakeUniqueNodeID(NameBase)
        ReachObject.InUse = SetInuse
        ReachObject.nt = enmNodetype.SBK_PROFILE
        ReachObject.ProfileType = CrossSectionType
        ReachObject.ci = Id

        Dim UpstreamReachesSearched As New List(Of String)
        UpstreamReachesSearched.Add(Me.Id.Trim.ToUpper)
        Dim DownstreamReachesSearched As New List(Of String)
        DownstreamReachesSearched.Add(Me.Id.Trim.ToUpper)

        'if the chainage is invalid, find the most suitable neighbor to place our reach object on!
        'note: for now we stick to the minimum distance on the reach. We won't move our objects further on a neighboring reach
        If getReachLength() < MinimumDistanceOnReach Then
            ReachObject.lc = Chainage
            ReachObjects.Add(ReachObject)
            Me.Setup.Log.AddWarning("Warning: reach " & Id & " has a length < minimum distance for objects: " & MinimumDistanceOnReach & "m. The object " & ReachObject.ID & " was placed at its original distance of " & Chainage & "m to make it possible anyway.")
            Return ReachObject
        ElseIf Chainage < MinimumDistanceOnReach Then
            If AllowPlacementOnNeighbourReach Then

                neighborReach = getNeighborReachNearestAngle(True, 0, UpstreamReachesSearched)
                If Not neighborReach Is Nothing Then
                    ReachObject.ci = neighborReach.Id
                    If neighborReach.bn.ID = bn.ID Then
                        ReachObject.lc = MinimumDistanceOnReach
                        neighborReach.ReachObjects.Add(ReachObject)
                    Else
                        ReachObject.lc = neighborReach.getReachLength - MinimumDistanceOnReach
                        neighborReach.ReachObjects.Add(ReachObject)
                    End If
                    Return ReachObject
                Else
                    ReachObject.lc = MinimumDistanceOnReach
                    ReachObjects.Add(ReachObject)
                    Me.Setup.Log.AddMessage("Reach object " & ReachObject.ID & " had a chainage < " & MinimumDistanceOnReach & "m. The object was placed at " & MinimumDistanceOnReach & "m from the beginning of the reach instead.")
                    Return ReachObject
                End If
            Else
                ReachObject.lc = MinimumDistanceOnReach
                ReachObjects.Add(ReachObject)
                Me.Setup.Log.AddMessage("Reach object " & ReachObject.ID & " had a chainage < " & MinimumDistanceOnReach & "m. The object was placed at " & MinimumDistanceOnReach & "m from the beginning of the reach instead.")
                Return ReachObject
            End If

        ElseIf Chainage > getReachLength() - MinimumDistanceOnReach Then

            If AllowPlacementOnNeighbourReach Then
                neighborReach = getNeighborReachNearestAngle(False, 0, DownstreamReachesSearched)
                If Not neighborReach Is Nothing Then
                    ReachObject.ci = neighborReach.Id
                    If neighborReach.bn.ID = en.ID Then
                        ReachObject.lc = MinimumDistanceOnReach
                        neighborReach.ReachObjects.Add(ReachObject)
                    Else
                        ReachObject.lc = neighborReach.getReachLength - MinimumDistanceOnReach
                        neighborReach.ReachObjects.Add(ReachObject)
                    End If
                    Return ReachObject
                Else
                    ReachObject.lc = getReachLength() - MinimumDistanceOnReach
                    ReachObjects.Add(ReachObject)
                    Me.Setup.Log.AddMessage("Cross section " & ReachObject.ID & " had a chainage > reach length - " & MinimumDistanceOnReach & "m. The object was placed at " & MinimumDistanceOnReach & "m from the end of the reach instead.")
                    Return ReachObject
                End If
            Else
                ReachObject.lc = getReachLength() - MinimumDistanceOnReach
                ReachObjects.Add(ReachObject)
                Me.Setup.Log.AddMessage("Cross section " & ReachObject.ID & " had a chainage > reach length - " & MinimumDistanceOnReach & "m. The object was placed at " & MinimumDistanceOnReach & "m from the end of the reach instead.")
                Return ReachObject
            End If

        Else
            ReachObject.lc = Chainage
            ReachObjects.Add(ReachObject)
            Return ReachObject
        End If

        'if we end up here something's wrong.
        Me.Setup.Log.AddError("Error adding cross section " & ReachObject.ID & " to reach " & Id & " at chainage " & Chainage & ".")
        Return Nothing
    End Function

    Public Function getLocationsFromUpstreamReaches(DistanceOnUpstreamReach As Double) As List(Of clsSbkReachObject)
        'given a location on the current reach, this function searches for an upstream location on all reaches that are connected 
        'to its start node. This means: including incoming and outgoing reaches to that node

        Dim inReaches As New List(Of clsSbkReach)
        Dim outReaches As New List(Of clsSbkReach)
        Dim newLoc As clsSbkReachObject, Locations As New List(Of clsSbkReachObject)
        inReaches = GetIncomingReaches(bn)
        outReaches = GetOutgoingReaches(bn)

        For Each inReach As clsSbkReach In inReaches
            If Not inReach.Id = Id Then
                If DistanceOnUpstreamReach <= inReach.getReachLength Then
                    'return the location
                    newLoc = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    newLoc.lc = inReach.getReachLength - DistanceOnUpstreamReach
                    newLoc.ci = inReach.Id
                    Locations.Add(newLoc)
                Else
                    'upstream reach is shorter than we'd want, so return the start node of that reach
                    newLoc = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    newLoc.lc = 0
                    newLoc.ci = inReach.Id
                    Locations.Add(newLoc)
                End If
            End If
        Next

        For Each outReach As clsSbkReach In outReaches
            If Not outReach.Id = Id Then
                If DistanceOnUpstreamReach <= outReach.getReachLength Then
                    'return the location
                    newLoc = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    newLoc.lc = DistanceOnUpstreamReach
                    newLoc.ci = outReach.Id
                    Locations.Add(newLoc)
                Else
                    'downstream reach is shorter than we'd want, so return the end node of that reach
                    newLoc = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    newLoc.lc = outReach.getReachLength
                    newLoc.ci = outReach.Id
                    Locations.Add(newLoc)
                End If
            End If
        Next

        Return Locations
    End Function

    Public Function getLocationsFromDownstreamReaches(DistanceOnDownstreamReach As Double) As List(Of clsSbkReachObject)
        'given a location on the current reach, this function searches for a downstream location on all reaches that are connected 
        'to its end node. This means: including incoming and outgoing reaches to that node

        Dim inReaches As New List(Of clsSbkReach)
        Dim outReaches As New List(Of clsSbkReach)
        Dim newLoc As clsSbkReachObject, Locations As New List(Of clsSbkReachObject)
        outReaches = GetOutgoingReaches(en)
        inReaches = GetIncomingReaches(en)

        For Each inReach As clsSbkReach In inReaches
            If Not inReach.Id = Id Then
                If DistanceOnDownstreamReach <= inReach.getReachLength Then
                    'return the location
                    newLoc = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    newLoc.lc = inReach.getReachLength - DistanceOnDownstreamReach
                    newLoc.ci = inReach.Id
                    Locations.Add(newLoc)
                Else
                    'upstream reach is shorter than we'd want, so return the start node of that reach
                    newLoc = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    newLoc.lc = 0
                    newLoc.ci = inReach.Id
                    Locations.Add(newLoc)
                End If
            End If
        Next

        For Each outReach As clsSbkReach In outReaches
            If Not outReach.Id = Id Then
                If DistanceOnDownstreamReach <= outReach.getReachLength Then
                    'return the location
                    newLoc = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    newLoc.lc = DistanceOnDownstreamReach
                    newLoc.ci = outReach.Id
                    Locations.Add(newLoc)
                Else
                    'downstream reach is shorter than we'd want, so return the end node of that reach
                    newLoc = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    newLoc.lc = outReach.getReachLength
                    newLoc.ci = outReach.Id
                    Locations.Add(newLoc)
                End If
            End If
        Next

        Return Locations
    End Function

    Public Function getLocationFromUpstreamReach(ByRef upReach As clsSbkReach, ByRef upDist As Double) As Boolean
        Try
            Dim UpstreamReachesSearched As New List(Of String)
            UpstreamReachesSearched.Add(Me.Id.Trim.ToUpper)

            upReach = getNeighborReachNearestAngle(True, 0, UpstreamReachesSearched)
            If Not upReach Is Nothing AndAlso upReach.getReachLength >= upDist Then
                If upReach.en.ID = bn.ID Then
                    'transform the distance from our original startnode into a chainage for the upstream reach
                    upDist = upReach.getReachLength - upDist
                Else
                    'do nothing since the distance already represents the chainage on the upstream reach
                End If
                Return True
            ElseIf Not upReach Is Nothing Then
                If upReach.en.ID = bn.ID Then
                    upDist = 0 'veiligheidsklep. We gaan (voor nu) niet meer dan één tak overspringen
                Else
                    upDist = upReach.getReachLength 'veiligheidsklep. We gaan (voor nu) niet meer dan één tak overspringen
                End If
                Return True
            Else
                upReach = Nothing
                Return False
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getLocationFromupstreamReach.")
            Return False
        End Try
    End Function

    Public Function getLocationFromDownstreamReach(ByRef dnReach As clsSbkReach, ByRef dnDist As Double) As Boolean
        Try
            Dim DownstreamReachesSearched As New List(Of String)
            DownstreamReachesSearched.Add(Me.Id.Trim.ToUpper)

            dnReach = getNeighborReachNearestAngle(False, 0, DownstreamReachesSearched)
            If Not dnReach Is Nothing AndAlso dnReach.getReachLength >= dnDist Then
                If dnReach.en.ID = en.ID Then
                    'transform the distance from our original startnode into a chainage for the downstream reach
                    dnDist = dnReach.getReachLength - dnDist
                Else
                    'do nothing since the distance already represents the chainage on the downstream reach
                End If
                Return True
            ElseIf Not dnReach Is Nothing Then
                If dnReach.en.ID = en.ID Then
                    dnDist = 0 'veiligheidsklep. We gaan (voor nu) niet meer dan één tak overspringen
                Else
                    dnDist = dnReach.getReachLength 'veiligheidsklep. We gaan (voor nu) niet meer dan één tak overspringen
                End If
                Return True
            Else
                dnReach = Nothing
                Return False
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getLocationFromDownstreamReach.")
            Return False
        End Try
    End Function

    Public Function getDownstreamLocation(ByVal startDist As Double, ByVal radius As Double, ByRef myReach As clsSbkReach, ByRef myChainage As Double, ByRef ReachesSearched As List(Of String)) As Boolean
        'v2.112: changed this function into a recursive function; only stopping when the suitable location is found OR when all reaches have been processed 
        'this function is similar to the one above (getLocationFromDownstreamReach) however it allows to search further than it's own reach
        Dim dnReach As clsSbkReach

        Try
            'first add self to our list of searched reaches
            ReachesSearched.Add(Id.Trim.ToUpper)

            If (startDist + radius) < getReachLength() Then
                myReach = Me
                myChainage = startDist + radius
            Else
                'the location we're looking for is not on the current reach. try the first one downstream
                'be careful! if the downstream reach as a different direction, we'll have to start searching for UPstream location
                dnReach = getNeighborReachNearestAngle(False, 10, ReachesSearched)
                radius -= (getReachLength() - startDist)            'subtract the currently processed reach from the search radius

                If dnReach Is Nothing Then
                    'apparently the furthest we can move downstream is the current reach. Return the maximum chainage from this reach
                    myReach = Me
                    myChainage = getReachLength()
                    Return False    'v2.112 adding this exit allows us to search for alternative ways to identify target levels downstream (extrapolation)
                Else
                    If dnReach.bn.ID = en.ID Then
                        If Not dnReach.getDownstreamLocation(0, radius, myReach, myChainage, ReachesSearched) Then Throw New Exception("Error finding downstream location on reach " & Id & " for chainage " & startDist & " and radius " & radius & ".")
                    Else
                        If Not dnReach.getUpstreamLocation(dnReach.getReachLength, radius, myReach, myChainage, ReachesSearched) Then Throw New Exception("Error finding upstream location on reach " & Id & " for chainage " & startDist & " and radius " & radius & ".")
                    End If
                End If

            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getDownstreamLocation: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function getUpstreamLocation(ByVal startChainage As Double, ByVal radius As Double, ByRef myReach As clsSbkReach, ByRef myChainage As Double, ByRef ReachesSearched As List(Of String)) As Boolean
        'v2.112: changed this function into a recursive function; only stopping when the suitable location is found OR when all reaches have been processed 
        'this function is similar to getLocationFromUpstreamReach however it allows to search further than it's own reach
        Dim upReach As clsSbkReach

        Try
            ReachesSearched.Add(Me.Id.Trim.ToUpper) 'add self to the list of searches reaches

            If (startChainage - radius) >= 0 Then
                myReach = Me
                myChainage = startChainage - radius
            Else
                'the location we're looking for is not on the current reach. try the first one upstream
                'be careful! if the upstream reach as a different direction, we'll have to start searching for DOWNstream location
                upReach = getNeighborReachNearestAngle(True, 10, ReachesSearched)
                radius -= startChainage            'subtract the currently processed reach from the search radius

                If upReach Is Nothing Then
                    'apparently the furthest we can move upstream is the current reach. Return chainage 0 from this reach
                    myReach = Me
                    myChainage = 0
                    Return False    'v2.112: returning false here so we can find alternative ways to find the upstream target levels
                Else
                    If upReach.en.ID = bn.ID Then
                        If Not upReach.getUpstreamLocation(upReach.getReachLength, radius, myReach, myChainage, ReachesSearched) Then Throw New Exception("could not find upstream location, starting from reach " & Id & ", at chainage " & startChainage & " with search radius " & radius & ".")
                    Else
                        If Not upReach.getDownstreamLocation(0, radius, myReach, myChainage, ReachesSearched) Then Throw New Exception("could not find upstream location, starting from reach " & Id & ", at chainage " & startChainage & " with search radius " & radius & ".")
                    End If
                End If
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getUpstreamLocation: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function containsLinkage(Optional ByRef myLinkageNode As clsSbkReachObject = Nothing) As Boolean
        Dim myObj As clsSbkReachObject
        For Each myObj In ReachObjects.ReachObjects.Values
            If myObj.nt = enmNodetype.NodeCFLinkage Then
                myLinkageNode = myObj
                Return True
            End If
        Next
        Return False
    End Function

    Public Function containsMeasurement(Optional ByRef myMeas As clsSbkReachObject = Nothing) As Boolean
        Dim myObj As clsSbkReachObject
        For Each myObj In ReachObjects.ReachObjects.Values
            If myObj.nt = enmNodetype.MeasurementStation Then
                myMeas = myObj
                Return True
            End If
        Next
        Return False
    End Function

    Public Function getNextReachNode(ByRef FirstReachNode As clsSbkReachNode) As clsSbkReachNode
        If bn.ID = FirstReachNode.ID Then
            Return en
        Else
            Return bn
        End If
    End Function

    Public Function ContainsOtherCrossSectionType(ReferenceProfileType As GeneralFunctions.enmProfileType) As Boolean
        Try
            'this function figures out if our reach already contains cross sections of a different type
            'first find any existing cross section (including via interpolation)
            'v2.113: added a list of checked reaches in order to prevent an endless loop
            Dim ReachesChecked As New Dictionary(Of String, clsSbkReach)   'we'll keep the TRIM.TOUPPER of the reach's ID's in this list
            Dim ExistingProfile As clsSbkReachObject = GetFirstObjectOnReach(enmNodetype.SBK_PROFILE, ReachesChecked, True, True)
            If ExistingProfile Is Nothing Then Return False
            If ExistingProfile.ProfileType = ReferenceProfileType Then Return False Else Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in funtion ContainsOtherCrossSectionType of class clsSbkReach: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function ContainsCrossSectionFromBetterSource(QualityIdx As Integer) As Boolean
        'QualityIdx is the quality index number for the object we want to check. Lower = better. So we return true if we have a cross section that has a lower qualityidx value
        'this function is used to distinguish yz-profiles based on the primary datasource from the ones based on the secondary datasource
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = enmNodetype.SBK_PROFILE AndAlso myObj.InUse Then
                If myObj.QualityIdx < QualityIdx Then Return True
            End If
        Next
        Return False
    End Function
    Public Function LineSegmentIntersection(X1 As Double, Y1 As Double, X2 As Double, Y2 As Double, ByRef Xsect As Double, ByRef Ysect As Double, ByRef Chainage As Double) As Boolean
        'finds out if any of the reach's line segments intersect with a given line segment
        'returns True if an intersection Is found
        'also returns the exact X and Y coordinate and the segment index number
        Try
            Dim cumDist As Double
            For j = 0 To NetworkcpRecord.CPTable.CP.Count - 2
                With NetworkcpRecord.CPTable
                    If X1 < .CP(j).X AndAlso X1 < .CP(j + 1).X AndAlso X2 < .CP(j).X AndAlso X2 < .CP(j + 1).X Then
                        'do nothing
                    ElseIf X1 > .CP(j).X AndAlso X1 > .CP(j + 1).X AndAlso X2 > .CP(j).X AndAlso X2 > .CP(j + 1).X Then
                        'do nothing
                    ElseIf Y1 < .CP(j).Y AndAlso Y1 < .CP(j + 1).Y AndAlso Y2 < .CP(j).Y AndAlso Y2 < .CP(j + 1).Y Then
                        'do nothing
                    ElseIf Y1 > .CP(j).Y AndAlso Y1 > .CP(j + 1).Y AndAlso Y2 > .CP(j).Y AndAlso Y2 > .CP(j + 1).Y Then
                        'do nothing
                    Else
                        'found a possible intersection now calculate the exact location and decide whether it actually IS an intersection
                        'y = a1X+b1 = a2X+b2
                        Dim reachLine As New clsLineDefinition(Me.Setup)
                        Dim myLine As New clsLineDefinition(Me.Setup)
                        reachLine = Setup.GeneralFunctions.LineFromPoints(.CP(j).X, .CP(j).Y, .CP(j + 1).X, .CP(j + 1).Y)
                        myLine = Setup.GeneralFunctions.LineFromPoints(X1, Y1, X2, Y2)
                        Xsect = (myLine.b - reachLine.b) / (reachLine.a - myLine.a)
                        Ysect = myLine.a * Xsect + myLine.b

                        'only if the xy-intersection lies within the bounding box of both line segments we have an actual intersection.
                        'we already checked if we're inside the channel segment bounding box, so only checking the cross section line segment remains
                        If Xsect >= Math.Min(X1, X2) AndAlso Xsect <= Math.Max(X1, X2) AndAlso Ysect >= Math.Min(Y1, Y2) AndAlso Ysect <= Math.Max(Y1, Y2) Then
                            Chainage = cumDist + Setup.GeneralFunctions.Pythagoras(.CP(j).X, .CP(j).Y, Xsect, Ysect)
                            Return True
                        End If
                    End If
                    cumDist += Setup.GeneralFunctions.Pythagoras(.CP(j).X, .CP(j).Y, .CP(j + 1).X, .CP(j + 1).Y)
                End With
            Next
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function calcXY(ByVal Chainage As Double, ByRef X As Double, ByRef Y As Double) As Boolean
        'this routine calculates the XY-coordinate for a given distance on the current reach
        Dim nPoints As Integer
        If Chainage < 0 Then
            'we must extrapolate from the first two coordinates of our reach
            NetworkcpRecord.CalcCPTable(Id, bn, en)
            X = Me.Setup.GeneralFunctions.Extrapolate(NetworkcpRecord.CPTable.CP(1).Dist, NetworkcpRecord.CPTable.CP(1).X, NetworkcpRecord.CPTable.CP(0).Dist, NetworkcpRecord.CPTable.CP(0).X, Chainage)
            Y = Me.Setup.GeneralFunctions.Extrapolate(NetworkcpRecord.CPTable.CP(1).Dist, NetworkcpRecord.CPTable.CP(1).Y, NetworkcpRecord.CPTable.CP(0).Dist, NetworkcpRecord.CPTable.CP(0).Y, Chainage)
            Return True
        ElseIf Chainage > getReachLength() Then
            'we must extrapolate from the last two coordinates of our reach
            NetworkcpRecord.CalcCPTable(Id, bn, en)
            nPoints = NetworkcpRecord.CPTable.CP.Count
            X = Me.Setup.GeneralFunctions.Extrapolate(NetworkcpRecord.CPTable.CP(nPoints - 2).Dist, NetworkcpRecord.CPTable.CP(nPoints - 2).X, NetworkcpRecord.CPTable.CP(nPoints - 1).Dist, NetworkcpRecord.CPTable.CP(nPoints - 1).X, Chainage)
            Y = Me.Setup.GeneralFunctions.Extrapolate(NetworkcpRecord.CPTable.CP(nPoints - 2).Dist, NetworkcpRecord.CPTable.CP(nPoints - 2).Y, NetworkcpRecord.CPTable.CP(nPoints - 1).Dist, NetworkcpRecord.CPTable.CP(nPoints - 1).Y, Chainage)
            Return True
        Else
            Return NetworkcpRecord.CalcXY(Chainage, X, Y)
        End If
    End Function

    Public Function calcSnapLocationInsidePolygon(ByVal X As Double, ByVal Y As Double, ByVal SearchRadius As Double, ByRef SnapChainage As Double, ByRef SnapDistance As Double, ByVal Polygon As MapWinGIS.Shape) As Boolean
        'this routine calculates the snapping location (chainage) on a reach for a given XY-coordinate
        'with an additional constraint: snapping can only take place inside a given polygon
        'this function is new in v1.77
        Try
            'first check if the current reach intersects with our polygon
            If BoundingBox Is Nothing Then calcBoundingBox()
            If BoundingBox.XUR < Polygon.Extents.xMin Then Return False
            If BoundingBox.XLL > Polygon.Extents.xMax Then Return False
            If BoundingBox.YUR < Polygon.Extents.yMin Then Return False
            If BoundingBox.YLL > Polygon.Extents.yMax Then Return False

            'make sure we have a list of curving points for our current reach
            If NetworkcpRecord.CPTable.CP.Count = 0 Then NetworkcpRecord.CalcCPTable(Me.Id, Me.bn, Me.en)

            'walk through each vector point and find the nearest snapping location, provided both surrounding vector points lie inside our polygon
            'OPMERKING: de aanname dat beide omringende vectorpunten binnen het polygoon moeten liggen is een grove
            'op dit moment is geen betere methodiek beschikbaar om ook snapping te doen wanneer uitsluitend een deel van de vector binnen de polygoon ligt
            'de huidige implementatie is van v1.77
            Dim pt1 As New MapWinGIS.Point
            Dim pt2 As New MapWinGIS.Point
            Dim ChainageOnSegment As Double
            Dim curSnappingDistance As Double
            Dim SnappingPointFound As Boolean
            For i = 0 To NetworkcpRecord.CPTable.CP.Count - 2
                pt1.x = NetworkcpRecord.CPTable.CP(i).X
                pt1.y = NetworkcpRecord.CPTable.CP(i).Y
                pt2.x = NetworkcpRecord.CPTable.CP(i + 1).X
                pt2.y = NetworkcpRecord.CPTable.CP(i + 1).Y
                If Polygon.PointInThisPoly(pt1) AndAlso Polygon.PointInThisPoly(pt2) Then
                    If Me.Setup.GeneralFunctions.PointToLineSnapping(NetworkcpRecord.CPTable.CP(i).X, NetworkcpRecord.CPTable.CP(i).Y, NetworkcpRecord.CPTable.CP(i + 1).X, NetworkcpRecord.CPTable.CP(i + 1).Y, X, Y, SearchRadius, ChainageOnSegment, curSnappingDistance) Then
                        If curSnappingDistance < SnapDistance AndAlso curSnappingDistance < SearchRadius Then
                            SnappingPointFound = True
                            SnapChainage = NetworkcpRecord.CPTable.CP(i).Dist + ChainageOnSegment
                            SnapDistance = curSnappingDistance
                        End If
                    End If
                End If

                'also look for snapping to the vector points, if allowed
                'snapping to a boundary node is not allowed so skip those. Siebe: added for v1.71 on 12-5-2020
                If Not (i = 0 AndAlso bn.isBoundary) Then
                    If Setup.GeneralFunctions.Pythagoras(NetworkcpRecord.CPTable.CP(i).X, NetworkcpRecord.CPTable.CP(i).Y, X, Y) < SnapDistance Then
                        SnapChainage = NetworkcpRecord.CPTable.CP(i).Dist
                        SnapDistance = Setup.GeneralFunctions.Pythagoras(NetworkcpRecord.CPTable.CP(i).X, NetworkcpRecord.CPTable.CP(i).Y, X, Y)
                        SnappingPointFound = True
                    End If
                End If
            Next

            'also examine the last vector
            'snapping to a boundary node is not allowed so skip those. Siebe: added for v1.71 on 12-5-2020
            If Not en.isBoundary Then
                If Setup.GeneralFunctions.Pythagoras(NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1).X, NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1).Y, X, Y) < SnapDistance Then
                    SnapChainage = getReachLength()
                    SnapDistance = Setup.GeneralFunctions.Pythagoras(NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1).X, NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1).Y, X, Y)
                    SnappingPointFound = True
                End If
            End If

            ''EXPERIMENTAL: if the angle between our search location and vector is nearest to 180, we have a point that is almost ON the reach
            ''now walk through our vector points and search for the best snapping location
            'Dim maxAngleDiff As Double = 0, curAngle1 As Double, curAngle2 As Double, curDiff As Double
            'For i = 0 To NetworkcpRecord.CPTable.CP.Count - 2
            '    'search for the vector points where the angle from our starting point to both vector point is at a maximum
            '    curAngle1 = Setup.GeneralFunctions.LineAngleDegrees(X, Y, NetworkcpRecord.CPTable.CP(i).X, NetworkcpRecord.CPTable.CP(i).Y)
            '    curAngle2 = Setup.GeneralFunctions.LineAngleDegrees(X, Y, NetworkcpRecord.CPTable.CP(i + 1).X, NetworkcpRecord.CPTable.CP(i + 1).Y)
            '    curDiff = Setup.GeneralFunctions.AngleDifferenceDeg(curAngle1, curAngle2)
            '    If Math.Abs(Math.Abs(curDiff) - 180) < Math.Abs(maxAngleDiff - 180) Then

            '    End If
            'Next
            Return SnappingPointFound
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function calcSnapLocation(ByVal X As Double, ByVal Y As Double, ByVal SearchRadius As Double, ByRef SnapChainage As Double, ByRef SnapDistance As Double, AllowSnappingToVectorPoints As Boolean) As Boolean
        'this routine calculates the snapping location (chainage) on a reach for a given XY-coordinate
        Try
            If BoundingBox Is Nothing Then calcBoundingBox()
            'first check if the current reach is within search range from our point of origin
            'for this we use the snapping distance as found so far
            If X < BoundingBox.XLL - SnapDistance Then Return False
            If X > BoundingBox.XUR + SnapDistance Then Return False
            If Y < BoundingBox.YLL - SnapDistance Then Return False
            If Y > BoundingBox.YUR + SnapDistance Then Return False

            'the current reach has the potential to yield a candidate for snapping. Execute the snapping algorithm and compute the new snapping distance, if a better candidate is found
            If Not NetworkcpRecord.CalcSnapLocation(X, Y, SearchRadius, SnapChainage, SnapDistance, AllowSnappingToVectorPoints) Then Return False
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function BuildInitialDatRecord(ByVal WaterLevel As Boolean, ByVal Value As Double) As clsInitialDatFLINRecord
        Dim IDat As New clsInitialDatFLINRecord(Me.Setup)

        'initialization data
        IDat = New clsInitialDatFLINRecord(Me.Setup)
        IDat.nm = "initial"
        IDat.ss = 0
        IDat.ID = Id
        IDat.ci = Id
        IDat.ty = 1    'type initiele waterstand
        IDat.q_lq1 = 0 'constante initiele flow
        IDat.q_lq2 = 0 'initiele q = 0
        IDat.q_lq3 = 9999900000.0

        If WaterLevel Then
            IDat.lv_ll1 = 0 'constante initiele waterstand
        Else
            IDat.lv_ll1 = 1 'constante initiele waterdiepte
        End If
        IDat.lv_ll2 = Value
        IDat.lv_ll3 = 9999900000.0
        Return IDat

    End Function

    Public Function calculateTotalVolumeUnderSurfaceLevel() As Double
        'Author: Siebe Bosch
        'Date: 17-9-2013
        'Description calculates the total volume under surface level for this reach
        'note: surface level must already be stored in the token .rs in profile.dat!!!!!

        'first create a dictionary of all profiles on this reach, sorted by distance on reach
        Dim myDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim firstDat As clsProfileDatRecord, firstDef As clsProfileDefRecord
        Dim lastDat As clsProfileDatRecord, prevDat As clsProfileDatRecord, nextDat As clsProfileDatRecord
        myDict = getObjectsOfTypeSorted(enmNodetype.SBK_PROFILE)
        Dim Volume As Double = 0
        Dim i As Integer
        Dim n As Integer = myDict.Count

        'make sure the area underneath the surface level has actually been calculated
        For Each myDat In SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Values
            If myDat.AreaUnderSurfaceLevel <= 0 Then
                firstDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(myDat.di.Trim.ToUpper)
                If Not firstDef Is Nothing Then
                    myDat.CalculateAreaUnderSurfaceLevel(firstDef)
                Else
                    Me.Setup.Log.AddError("Could not find profile definition for profile " & myDat.ID)
                End If
            End If
        Next

        'now start calculating the total volume under surface level for this reach
        If myDict.Count = 0 Then
            Volume = 0
        ElseIf myDict.Count = 1 Then
            prevDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(myDict.Values(0).ID.Trim.ToUpper)
            Volume = getReachLength() * prevDat.AreaUnderSurfaceLevel
        ElseIf myDict.Count >= 2 Then
            firstDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(myDict.Values(0).ID.Trim.ToUpper)
            lastDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(myDict.Values(n - 1).ID.Trim.ToUpper)

            'first calculate the beginning and end segments
            Volume = myDict.Values(0).lc * firstDat.AreaUnderSurfaceLevel
            Volume += (getReachLength() - myDict.Values(n - 1).lc) * lastDat.AreaUnderSurfaceLevel

            'then calculate the middle sections
            For i = 0 To n - 2
                prevDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(myDict.Values(i).ID.Trim.ToUpper)
                nextDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(myDict.Values(i + 1).ID.Trim.ToUpper)
                Volume += (myDict.Values(i + 1).lc - myDict.Values(i).lc) * (prevDat.AreaUnderSurfaceLevel + nextDat.AreaUnderSurfaceLevel) / 2
            Next

        End If

        TotalVolumeUnderSurfaceLevel = Volume
        Return TotalVolumeUnderSurfaceLevel

    End Function

    Public Function IsPartOfLoop(ByRef LoopReaches As clsSbkReaches, ByVal IgnoreDisabledReaches As Boolean) As Boolean
        Dim curReach As clsSbkReach = Me
        Dim StartNode As clsSbkReachNode = Me.bn
        Dim PrevNode As clsSbkReachNode = Me.bn
        Dim NextNode As clsSbkReachNode = Me.en
        Dim nReaches As Integer
        Dim Junction As clsSbkReachNode = Nothing
        Dim Done As Boolean

        'This function investigates whether a reach is part of a loop that is only connected to 
        'the rest of the schematization by one single connection (e.g. a lassoo)
        'it returns true/false and an instance of clsSbkReaches containing all the reaches involved

        'just to make sure we're currently in a valid reach
        If IgnoreDisabledReaches AndAlso curReach.InUse = False Then Return False

        'start by adding the current reach to the collection
        LoopReaches.Reaches.Add(curReach.Id.Trim.ToUpper, curReach)

        Done = False
        'walk through all reaches sequentially
        While Not Done
            nReaches = NextNode.NumConnectedReaches(True)
            Select Case nReaches
                Case Is = 0 'dangling node? cannot be part of a loop anyway
                    Return False
                Case Is = 1 'reach is dead end, so cannot be part of a loop
                    Return False
                Case Is = 2 'current reach can still be part of a loop so add it to the collection
                    If Not curReach.Id = Id Then LoopReaches.Reaches.Add(curReach.Id.Trim.ToUpper, curReach)

                    'Now move over to the next reach
                    PrevNode = NextNode
                    curReach = SbkCase.CFTopo.Reaches.GetnextReach(curReach, NextNode, IgnoreDisabledReaches)
                    NextNode = curReach.getNextReachNode(PrevNode)
                    If IgnoreDisabledReaches AndAlso curReach.InUse = False Then Return False

                Case Is >= 3 'this is a junction. set it as such and add the reach to the collection
                    Junction = NextNode
                    If Not curReach.Id = Id Then LoopReaches.Reaches.Add(curReach.Id.Trim.ToUpper, curReach)
                    Done = True
            End Select
        End While


        'walk through all reaches in the oposite direction
        Done = False
        StartNode = Me.en
        PrevNode = Me.en
        NextNode = Me.bn
        curReach = Me

        While Not Done
            nReaches = NextNode.NumConnectedReaches(True)
            Select Case nReaches
                Case Is = 0 'dangling node? cannot be part of a loop anyway
                    Return False
                Case Is = 1 'reach is dead end, so cannot be part of a loop
                    Return False
                Case Is = 2 'current reach can still be part of a loop so add it to the collection
                    If Not curReach.Id = Id Then LoopReaches.Reaches.Add(curReach.Id.Trim.ToUpper, curReach)

                    'Now move over to the next reach
                    PrevNode = NextNode
                    curReach = SbkCase.CFTopo.Reaches.GetnextReach(curReach, NextNode, IgnoreDisabledReaches)
                    NextNode = curReach.getNextReachNode(PrevNode)
                    If IgnoreDisabledReaches AndAlso curReach.InUse = False Then Return False

                Case Is >= 3 'this is a junction. set it as such and add the reach to the collection
                    Done = True
                    If NextNode.ID = Junction.ID Then
                        If Not curReach.Id = Id Then LoopReaches.Reaches.Add(curReach.Id.Trim.ToUpper, curReach)
                        Return True
                    Else
                        Return False
                    End If
            End Select

        End While


    End Function

    Public Sub BuildCPAngle(ByVal AddAngleDegrees As Double)

        'this routine builds a line between two reachnodes that has one bob (knik)
        Dim myAngle As Double, legDist As Double, halfDist As Double

        NetworkcpRecord = New clsNetworkCPRecord(Me.Setup, Me)
        myAngle = Me.Setup.GeneralFunctions.NormalizeAngle(Me.Setup.GeneralFunctions.LineAngleDegrees(Me.bn.X, Me.bn.Y, Me.en.X, Me.en.Y))
        halfDist = Math.Sqrt((Me.bn.X - Me.en.X) ^ 2 + (Me.bn.Y - Me.en.Y) ^ 2) / 2

        'the length of each leg:
        'cos = halfdist/legdist dus legdist = halfdist/cos(angle)
        'don't forget to convert degrees to radials!
        legDist = halfDist / Math.Cos(Me.Setup.GeneralFunctions.D2R(AddAngleDegrees))

        NetworkcpRecord.Table.AddDataPair(2, legDist / 2, Me.Setup.GeneralFunctions.NormalizeAngle(myAngle + AddAngleDegrees))
        NetworkcpRecord.Table.AddDataPair(2, legDist * 3 / 2, Me.Setup.GeneralFunctions.NormalizeAngle(myAngle - AddAngleDegrees))

    End Sub



    Friend Function calcOpenWaterAreaAtTargetLevel(ByVal TargetLevel As Double) As Double
        'Author: Siebe Bosch
        'Date: 20-8-2013
        'Decription: calculates the openwater area for the entire reach, based on the profiles and a given target level
        Dim myDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim curDat As clsProfileDatRecord, curDef As clsProfileDefRecord
        Dim nextDat As clsProfileDatRecord, nextDef As clsProfileDefRecord
        Dim upDat As clsProfileDatRecord, upDef As clsProfileDefRecord
        Dim dnDat As clsProfileDatRecord, dnDef As clsProfileDefRecord
        Dim curObj As clsSbkReachObject, nextObj As clsSbkReachObject
        Dim myArea As Double
        Dim i As Long
        Dim Z As Double
        Dim ReachesProcessed As Dictionary(Of String, clsSbkReach)

        Try

            myDict = getObjectsOfTypeSorted(enmNodetype.SBK_PROFILE)
            myArea = 0

            If myDict.Values.Count = 0 Then
                'implemented this in v1.794 
                'no cross section in this reach. Try to find upstream or downstream via interpolation
                Dim upDist As Double, dnDist As Double
                Dim nProfs As Integer = 0

                ReachesProcessed = New Dictionary(Of String, clsSbkReach)
                Dim upProf As clsSbkReachObject = GetUpstreamObject(ReachesProcessed, 0, upDist, False, True, False, False, True)
                If Not upProf Is Nothing Then
                    nProfs += 1
                    upDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(upProf.ID.Trim.ToUpper)
                    upDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(upDat.di.Trim.ToUpper)
                    Z = TargetLevel - upDat.rl
                    myArea += getReachLength() * upDef.getProfileWidthAtLevel(Z)
                End If

                ReachesProcessed = New Dictionary(Of String, clsSbkReach)
                Dim dnProf As clsSbkReachObject = GetDownstreamObject(ReachesProcessed, getReachLength, dnDist, False, True, False, False, True)
                If Not dnProf Is Nothing Then
                    nProfs += 1
                    dnDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(dnProf.ID.Trim.ToUpper)
                    dnDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(dnDat.di.Trim.ToUpper)
                    Z = TargetLevel - dnDat.rl
                    myArea += getReachLength() * dnDef.getProfileWidthAtLevel(Z)
                End If

                'we hebben het oppervlak in eerste aanleg berekend door voor alle profielen te vermenigvuldigen met de taklengte. Nu dus corrigeren daarvoor
                If nProfs > 0 Then myArea = myArea / nProfs

            ElseIf myDict.Values.Count = 1 Then
                'één profiel wat geldt over de hele tak
                curObj = myDict.Values(0)
                curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                Z = TargetLevel - curDat.rl
                myArea = getReachLength() * curDef.getProfileWidthAtLevel(Z)
            Else
                'meerdere profielen op de tak
                curObj = myDict.Values(0)
                curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                Z = TargetLevel - curDat.rl
                myArea = curObj.lc * curDef.getProfileWidthAtLevel(Z)

                For i = 1 To myDict.Values.Count - 1
                    curObj = myDict.Values(i - 1)
                    nextObj = myDict.Values(i)
                    curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                    curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                    nextDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(nextObj.ID.Trim.ToUpper)
                    nextDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(nextDat.di.Trim.ToUpper)
                    Z = TargetLevel - curDat.rl
                    myArea += (nextObj.lc - curObj.lc) * (curDef.getProfileWidthAtLevel(Z) + nextDef.getProfileWidthAtLevel(Z)) / 2
                Next

                curObj = myDict.Values(myDict.Values.Count - 1)
                curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                Z = TargetLevel - curDat.rl
                myArea += (getReachLength() - curObj.lc) * curDef.getProfileWidthAtLevel(Z)
            End If

            Return myArea

        Catch ex As Exception
            Me.Setup.Log.AddWarning("Could not calculate Openwater Area for reach " & Id)
        End Try

    End Function

    Friend Function calcMaxOpenWaterArea() As Double
        'Author: Siebe Bosch
        'Date: 20-8-2013
        'Decription: calculates the openwater area for the entire reach, based on the maximum profile width of profiles
        Dim myDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim curDat As clsProfileDatRecord, curDef As clsProfileDefRecord
        Dim nextDat As clsProfileDatRecord, nextDef As clsProfileDefRecord
        Dim curObj As clsSbkReachObject, nextObj As clsSbkReachObject
        Dim myArea As Double
        Dim i As Long

        Try
            myDict = getObjectsOfTypeSorted(enmNodetype.SBK_PROFILE)

            If myDict.Values.Count = 1 Then
                curObj = myDict.Values(0)
                curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                myArea = getReachLength() * curDef.getMaximumWidth
            ElseIf myDict.Values.Count > 1 Then
                curObj = myDict.Values(0)
                curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                myArea = curObj.lc * curDef.getMaximumWidth

                For i = 1 To myDict.Values.Count - 1
                    curObj = myDict.Values(i - 1)
                    nextObj = myDict.Values(i)
                    curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                    curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                    nextDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(nextObj.ID.Trim.ToUpper)
                    nextDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(nextDat.di.Trim.ToUpper)
                    myArea += (nextObj.lc - curObj.lc) * (curDef.getMaximumWidth + nextDef.getMaximumWidth) / 2
                Next

                curObj = myDict.Values(myDict.Values.Count - 1)
                curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                myArea += (getReachLength() - curObj.lc) * curDef.getMaximumWidth
            End If

            Return myArea

        Catch ex As Exception
            Me.Setup.Log.AddWarning("Could not calculate Openwater Area for reach " & Id)
        End Try

    End Function

    Friend Function calcMinOpenWaterArea() As Double
        'Author: Siebe Bosch
        'Date: 20-8-2013
        'Decription: calculates the openwater area for the entire reach, based on the minimum profile width of profiles
        Dim myDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim curDat As clsProfileDatRecord, curDef As clsProfileDefRecord
        Dim nextDat As clsProfileDatRecord, nextDef As clsProfileDefRecord
        Dim curObj As clsSbkReachObject, nextObj As clsSbkReachObject
        Dim myArea As Double
        Dim i As Long

        Try
            myDict = getObjectsOfTypeSorted(enmNodetype.SBK_PROFILE)

            If myDict.Values.Count = 1 Then
                curObj = myDict.Values(0)
                curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                myArea = getReachLength() * curDef.getMinimumWidth
            ElseIf myDict.Values.Count > 1 Then
                curObj = myDict.Values(0)
                curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                myArea = curObj.lc * curDef.getMinimumWidth

                For i = 1 To myDict.Values.Count - 1
                    curObj = myDict.Values(i - 1)
                    nextObj = myDict.Values(i)
                    curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                    curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                    nextDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(nextObj.ID.Trim.ToUpper)
                    nextDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(nextDat.di.Trim.ToUpper)
                    myArea += (nextObj.lc - curObj.lc) * (curDef.getMinimumWidth + nextDef.getMinimumWidth) / 2
                Next

                curObj = myDict.Values(myDict.Values.Count - 1)
                curDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(curObj.ID.Trim.ToUpper)
                curDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(curDat.di.Trim.ToUpper)
                myArea += (getReachLength() - curObj.lc) * curDef.getMinimumWidth
            End If

            Return myArea

        Catch ex As Exception
            Me.Setup.Log.AddWarning("Could not calculate Openwater Area for reach " & Id)
        End Try

    End Function

    Friend Sub setStructuresInUse(ByVal Setting As Boolean)
        Dim myDat As clsStructDatRecord
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If Setup.GeneralFunctions.isStructure(myObj.nt) Then
                myDat = Me.SbkCase.CFData.Data.StructureData.StructDatRecords.FindByID(myObj.ID, "", False)
                myDat.InUse = Setting
            End If
        Next
    End Sub

    Friend Function Clone(ByVal newID As String, Optional ByVal UseIDasPostfix As Boolean = True) As clsSbkReach

        Dim NewReach As New clsSbkReach(Me.Setup, Me.SbkCase)

        If UseIDasPostfix Then
            NewReach.Id = Id & newID
        Else
            NewReach.Id = newID
        End If
        NewReach.InUse = True
        NewReach.bn = bn
        NewReach.en = en
        NewReach.ReachType.ParentReachType = ReachType.ParentReachType
        NewReach.CopyMetaDataFromReach(Me)        'also copy meta information, if present

        NewReach.NetworkcpRecord = NetworkcpRecord.Duplicate(NewReach.Id)
        NewReach.NetworkTPBrchRecord = NetworkTPBrchRecord.Duplicate(NewReach.Id)

        Return NewReach

    End Function

    Friend Function TruncateDownstreamPart(ByVal Chainage As Double, ByRef EndNode As clsSbkReachNode) As Boolean
        Try
            Dim newTable As New clsSobekCPTable(Me.Setup)
            For i = 0 To NetworkcpRecord.CPTable.CP.Count - 1
                If NetworkcpRecord.CPTable.CP(i).Dist < Chainage Then
                    newTable.CP.Add(New clsSbkVectorPoint(Me.Setup, NetworkcpRecord.CPTable.CP(i).ID, NetworkcpRecord.CPTable.CP(i).X, NetworkcpRecord.CPTable.CP(i).Y))     'use this point
                End If
            Next
            newTable.CP.Add(New clsSbkVectorPoint(Me.Setup, EndNode.ID, EndNode.X, EndNode.Y))
            NetworkcpRecord.CPTable = newTable
            NetworkcpRecord.CompleteCPTableFromCoordinates()
            NetworkcpRecord.CalcTable()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error truncating downstream part from reach " & Id)
            Return False
        End Try
    End Function

    Friend Function TruncateUpstreamPart(ByVal Chainage As Double, ByRef StartNode As clsSbkReachNode) As Boolean
        Try
            Dim newTable As New clsSobekCPTable(Me.Setup)
            newTable.CP.Add(New clsSbkVectorPoint(Me.Setup, StartNode.ID, StartNode.X, StartNode.Y))
            For i = 0 To NetworkcpRecord.CPTable.CP.Count - 1
                If NetworkcpRecord.CPTable.CP(i).Dist > Chainage Then
                    newTable.CP.Add(New clsSbkVectorPoint(Me.Setup, NetworkcpRecord.CPTable.CP(i).ID, NetworkcpRecord.CPTable.CP(i).X, NetworkcpRecord.CPTable.CP(i).Y))     'use this point
                End If
            Next
            NetworkcpRecord.CPTable = newTable
            NetworkcpRecord.CompleteCPTableFromCoordinates()
            NetworkcpRecord.CalcTable()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error truncating downstream part from reach " & Id)
            Return False
        End Try
    End Function

    Friend Enum enmIntersectModus
        StartAndEndNodes = 1
        OneSide = 2
        AllNodes = 3
    End Enum

    Friend Function getCalculationPoint(ByVal myID As String) As clsSbkReachObject
        If ReachObjects.ReachObjects.ContainsKey(myID.Trim.ToUpper) Then
            Return ReachObjects.ReachObjects.Item(myID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Function HasUpstreamReach(ByVal SkipInactiveReaches As Boolean) As Boolean
        'determines whether the current reach has an upstream connected reach
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            If Not Id = myReach.Id Then 'avoid comparing to itself!
                If bn.ID = myReach.en.ID OrElse bn.ID = myReach.bn.ID Then
                    If SkipInactiveReaches = False OrElse myReach.InUse = True Then
                        Return True
                    End If
                End If
            End If
        Next
        Return False
    End Function

    Friend Function HasDownstreamReach(ByVal SkipInactiveReaches As Boolean) As Boolean
        'determines whether the current reach has an upstream connected reach
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            If Not Id = myReach.Id Then 'avoid comparing to itself!
                If en.ID = myReach.en.ID OrElse en.ID = myReach.bn.ID Then
                    If SkipInactiveReaches = False OrElse myReach.InUse = True Then
                        Return True
                    End If
                End If
            End If
        Next
        Return False
    End Function

    Friend Sub CalculateReachObjectIndexNumbers()
        'deze routine kent een indexnummer toe aan alle objecten op een tak
        'dit gebeurt in oplopende volgorde naar afstand op de tak
        For Each Obj1 As clsSbkReachObject In ReachObjects.ReachObjects.Values
            Obj1.ReachObjectOrderIdx = 1   'initialiseer het objectvolgordeindex op 1
            For Each Obj2 As clsSbkReachObject In ReachObjects.ReachObjects.Values
                If Obj2.lc < Obj1.lc Then Obj1.ReachObjectOrderIdx += 1
            Next
        Next
    End Sub

    Friend Function PartOfLoop(ByRef Reaches As Dictionary(Of String, clsSbkReach)) As Boolean
        'this routine checks whether the current reach is part of a loop that has only one connection to the rest of the model
        'and returns a dictionary containing the reaches in that loop

        Dim curReach As clsSbkReach = Me
        Dim curNode As clsSbkReachNode
        Dim Junction As clsSbkReachNode = Nothing
        Dim nConnected As Integer
        Dim Done As Boolean = False

        'first walk from the begin node until you find a junction of three reaches
        curNode = curReach.bn
        While Not Done
            nConnected = curNode.NumConnectedReaches(False)
            Select Case nConnected
                Case Is <= 1
                    Return False        'this reach is a dead end and not part of a loop
                Case Is = 2
                    Reaches.Add(curReach.Id.Trim.ToUpper, curReach)
                Case Is = 3
                    'encountered a junction. If no junction yet found, then assign here
                    If Junction Is Nothing Then
                        Reaches.Add(curReach.Id.Trim.ToUpper, curReach)
                        Junction = curNode
                    Else
                        If curNode.ID = Junction.ID Then
                            'current node = previously found junction ID. The loop is complete!
                            Reaches.Add(curReach.Id.Trim.ToUpper, curReach)
                            Return True
                        Else
                            'current node <> previously found junction ID. This is not a loop!
                            Return False
                        End If
                    End If
                    Done = True
            End Select

            'move on to the next reach

        End While



    End Function

    Friend Function getReachLength() As Double
        'geeft de totale lengte van de onderhavige tak terug
        If NetworkcpRecord.CPTable.CP.Count > 0 Then
            'introduced in v1.797: recalculate the table in case Dist = 0
            If NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1).Dist = 0 Then NetworkcpRecord.CalcAll(Id, bn, en)
            Return NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1).Dist
        Else
            Dim Dist1 As Double, VectorLength As Double
            Dim i As Long

            For i = 0 To NetworkcpRecord.Table.XValues.Count - 1
                Dist1 = NetworkcpRecord.Table.XValues.Values(i)
                VectorLength = VectorLength + (Dist1 - VectorLength) * 2
            Next
            Return VectorLength
        End If

    End Function

    Friend Function getReachSlope(ByRef Slope As Double) As Boolean

        Try
            'returns the slope of the reach in m/km
            'in order to do so it retrieves the first and last cross section and divides the elevation difference by the distance between both
            Dim upProf As clsSbkReachObject = GetFirstProfile(Me)
            Dim downProf As clsSbkReachObject = GetLastProfile(Me)
            Dim upLevel As Double, downLevel As Double
            Dim DistKm As Double
            Dim nProfiles As Integer = CountObjectsOfType(enmNodetype.SBK_PROFILE)

            If nProfiles = 0 Then
                Return False
            ElseIf nProfiles = 1 Then
                'cannot compute reach slope since it has only one cross section
                'so try using neighbors if interpolation
                Return GetMultiReachSlope(Slope)
            Else
                If Not upProf Is Nothing AndAlso Not downProf Is Nothing Then
                    DistKm = (downProf.lc - upProf.lc) / 1000
                    If Not SbkCase.CFData.Data.ProfileData.GetCrossSectionBedLevel(upProf.ID, upLevel) Then Throw New Exception("Slope for reach " & Id & " could not be determined because of error retrieving bed level for profile " & upProf.ID)
                    If Not SbkCase.CFData.Data.ProfileData.GetCrossSectionBedLevel(downProf.ID, downLevel) Then Throw New Exception("Slope for reach " & Id & " could not be determined because of error retrieving bed level for profile " & downProf.ID)

                    If DistKm > 0 Then
                        Slope = Math.Abs((upLevel - downLevel) / DistKm)
                    Else
                        Throw New Exception("Reach slope could not be determined for reach " & Id & ". Distance between up and downstream cross section = 0.")
                    End If
                Else
                End If
                Return True
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Slope = 0
            Return False
        End Try

    End Function

    Public Function GetMultiReachSlope(ByRef Slope) As Boolean
        Dim upReach As clsSbkReach = Nothing
        Dim downReach As clsSbkReach = Nothing
        Dim myObj As clsSbkReachObject = Nothing
        Dim myDist As Double, myLevel As Double
        Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
        Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)

        If GetUpstreamInterpolatedReach(ReachesUpProcessed, upReach) Then
            Dim upLevel As Double
            Dim upObj As clsSbkReachObject = Nothing
            myObj = GetFirstProfile(Me)
            If Not SbkCase.CFData.Data.ProfileData.GetCrossSectionBedLevel(myObj.ID, myLevel) Then Return False
            'we've found a reach upstream that interpolates with our current reach
            If upReach.en.ID.Trim.ToUpper = bn.ID.Trim.ToUpper Then
                upObj = upReach.GetLastProfile(Me)
                If Not myObj Is Nothing AndAlso Not upObj Is Nothing Then
                    myDist = myObj.lc + (upReach.getReachLength - upObj.lc)
                    If Not SbkCase.CFData.Data.ProfileData.GetCrossSectionBedLevel(upObj.ID, upLevel) Then Return False
                    Slope = Math.Abs((upLevel - myLevel) / myDist / 1000)
                    Return True
                End If
            ElseIf upReach.bn.ID.Trim.ToUpper = bn.ID.Trim.ToUpper Then
                upObj = upReach.GetFirstProfile(Me)
                If Not myObj Is Nothing AndAlso Not upObj Is Nothing Then
                    myDist = myObj.lc + upObj.lc
                    If Not SbkCase.CFData.Data.ProfileData.GetCrossSectionBedLevel(upObj.ID, upLevel) Then Return False
                    Slope = Math.Abs((upLevel - myLevel / myDist / 1000))
                    Return True
                End If
            End If
        End If

        If GetDownstreamInterpolatedReach(ReachesDnProcessed, downReach) Then
            Dim downLevel As Double
            Dim downObj As clsSbkReachObject = Nothing
            myObj = GetLastProfile(Me)
            If Not SbkCase.CFData.Data.ProfileData.GetCrossSectionBedLevel(myObj.ID, myLevel) Then Return False
            'we've found a reach downstream that actually interpolates with our current reach
            If downReach.bn.ID.Trim.ToUpper = en.ID.Trim.ToUpper Then
                downObj = downReach.GetFirstProfile(Me)
                If myObj IsNot Nothing AndAlso downObj IsNot Nothing Then
                    myDist = (getReachLength() - myObj.lc) + downObj.lc
                    If Not SbkCase.CFData.Data.ProfileData.GetCrossSectionBedLevel(downObj.ID, downLevel) Then Return False
                    Slope = Math.Abs((myLevel - downLevel) / myDist / 1000)
                    Return True
                End If
            ElseIf downReach.en.ID.Trim.ToUpper = en.ID.Trim.ToUpper Then
                downObj = downReach.GetLastProfile(Me)
                If myObj IsNot Nothing AndAlso downObj IsNot Nothing Then
                    myDist = (getReachLength() - myObj.lc) + (downReach.getReachLength - downObj.lc)
                    If Not SbkCase.CFData.Data.ProfileData.GetCrossSectionBedLevel(downObj.ID, downLevel) Then Return False
                    Slope = Math.Abs((downLevel - myLevel / myDist / 1000))
                    Return True
                End If
            End If
        End If

        Slope = 0
        Return False

    End Function

    Friend Function GetUpstreamInterpolatedReach(ByRef ReachesProcessed As Dictionary(Of String, clsSbkReach), ByRef UpReach As clsSbkReach) As Boolean
        'figures out which (if any) reach lies upstream to the current reach and is being interpolated
        'we include the ID of the start reach to avoid ending up in an endless loop
        Try
            Dim NodesDat As clsNodesDatNODERecord
            If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(bn.ID.Trim.ToUpper) Then
                NodesDat = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.GetRecord(bn.ID.Trim.ToUpper)
                If NodesDat IsNot Nothing Then
                    If NodesDat.ni = 1 Then
                        If NodesDat.r1.Trim.ToUpper = Id.Trim.ToUpper Then
                            UpReach = SbkCase.CFTopo.Reaches.GetReach(NodesDat.r2)
                            If UpReach IsNot Nothing Then
                                If ReachesProcessed.ContainsKey(UpReach.Id.Trim.ToUpper) Then
                                    'we're running in circles! Exit the loop
                                    Return False
                                Else
                                    ReachesProcessed.Add(Id.Trim.ToUpper, Me)
                                    Return True
                                End If
                            Else
                                Me.Setup.Log.AddError("Invalid reach interpolation found in nodes.dat for node " & NodesDat.ID & ", between " & NodesDat.r1 & " and " & NodesDat.r2)
                            End If
                            Return True
                        ElseIf NodesDat.r2.Trim.ToUpper = Id.Trim.ToUpper Then
                            UpReach = SbkCase.CFTopo.Reaches.GetReach(NodesDat.r1)
                            If UpReach IsNot Nothing Then
                                If ReachesProcessed.ContainsKey(UpReach.Id.Trim.ToUpper) Then
                                    'we're running in circles! Exit the loop
                                    Return False
                                Else
                                    ReachesProcessed.Add(Id.Trim.ToUpper, Me)
                                    Return True
                                End If
                            Else
                                Me.Setup.Log.AddError("Invalid reach interpolation found in nodes.dat for node " & NodesDat.ID & ", between " & NodesDat.r1 & " and " & NodesDat.r2)
                            End If
                        End If
                    End If
                End If
            End If
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetUpstreamInterpolatedReach of class clsSbkReach while processing reach " & Id)
            Return False
        End Try

    End Function

    Friend Function GetDownstreamInterpolatedReach(ByRef ReachesProcessed As Dictionary(Of String, clsSbkReach), ByRef DownReach As clsSbkReach) As Boolean
        'figures out (if any) which reach lies upstream to the current reach and is being interpolated
        Try
            Dim NodesDat As clsNodesDatNODERecord = Nothing
            NodesDat = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.GetRecord(en.ID.Trim.ToUpper)
            If Not NodesDat Is Nothing Then
                If NodesDat.ni = 1 Then
                    If NodesDat.r1.Trim.ToUpper = Id.Trim.ToUpper Then
                        DownReach = SbkCase.CFTopo.Reaches.GetReach(NodesDat.r2)
                        If Not DownReach Is Nothing Then
                            If ReachesProcessed.ContainsKey(DownReach.Id.Trim.ToUpper) Then
                                'We're running in circles! Exit the loop
                                Return False
                            Else
                                ReachesProcessed.Add(Id.Trim.ToUpper, Me)
                                Return True
                            End If
                        Else
                            Me.Setup.Log.AddError("Invalid reach interpolation found in nodes.dat, for node " & NodesDat.ID & ", between " & NodesDat.r1 & " and " & NodesDat.r2)
                        End If
                    ElseIf NodesDat.r2.Trim.ToUpper = Id.Trim.ToUpper Then
                        DownReach = SbkCase.CFTopo.Reaches.GetReach(NodesDat.r1)
                        If Not DownReach Is Nothing Then
                            If ReachesProcessed.ContainsKey(DownReach.Id.Trim.ToUpper) Then
                                'we're running in circles! Exit the loop
                                Return False
                            Else
                                ReachesProcessed.Add(Id.Trim.ToUpper, Me)
                                Return True
                            End If
                        Else
                            Me.Setup.Log.AddError("Invalid reach interpolation found in nodes.dat, for node " & NodesDat.ID & ", between " & NodesDat.r1 & " and " & NodesDat.r2)
                        End If
                    End If
                End If
            End If
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetDownstreamInterpolatedReach of class clsSbkReach while processing reach " & Id)
            Return False
        End Try

    End Function

    Friend Function GetDownstreamReachSameDirection(ByRef DownReach As clsSbkReach) As Boolean
        'figures out (if any) which reach lies downstream to the current reach
        Try
            Dim minAngleDif As Double = 9.0E+99
            Dim myAngleDif As Double
            Dim curReachAngle As Double, nextReachAngle As Double
            DownReach = Nothing

            'we'll search for the reach that has the smallest angle difference with our own reach
            getLastVectorAngle(curReachAngle)   'calculate the angle of the last vector of this reach
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                If myReach.bn.ID = en.ID Then
                    getFirstVectorAngle(nextReachAngle)
                    myAngleDif = Setup.GeneralFunctions.AngleDifferenceDegrees(curReachAngle, nextReachAngle)
                    If myAngleDif < minAngleDif Then
                        minAngleDif = myAngleDif
                        DownReach = myReach
                    End If
                End If
            Next

            If DownReach Is Nothing Then
                Return False
            Else
                Return True
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetDownstreamReach of class clsSbkReach while processing reach " & Id)
            Return False
        End Try

    End Function

    Friend Function GetUpstreamReachSameDirection(ByRef UpReach As clsSbkReach) As Boolean
        'figures out (if any) which reach lies upstream to the current reach
        Try
            Dim minAngleDif As Double = 9.0E+99
            Dim myAngleDif As Double
            Dim curReachAngle As Double, prevReachAngle As Double
            UpReach = Nothing

            'we'll search for the reach that has the smallest angle difference with our own reach
            getFirstVectorAngle(curReachAngle)   'calculate the angle of the last vector of this reach
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                If myReach.en.ID = bn.ID Then
                    getLastVectorAngle(prevReachAngle)
                    myAngleDif = Setup.GeneralFunctions.AngleDifferenceDegrees(curReachAngle, prevReachAngle)
                    If myAngleDif < minAngleDif Then
                        minAngleDif = myAngleDif
                        UpReach = myReach
                    End If
                End If
            Next

            If UpReach Is Nothing Then
                Return False
            Else
                Return True
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetUpstreamReach of class clsSbkReach while processing reach " & Id)
            Return False
        End Try

    End Function

    Friend Function CountObjectsOfType(ObjectType As enmNodetype) As Integer
        Dim n As Integer = 0
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = ObjectType Then n += 1
        Next
        Return n
    End Function

    Friend Function getProfileAttributes(ByVal myDist As Double, ByRef BedLevel As Double, ByRef BedWidth As Double, ByRef MaxWidth As Double, ByRef FlowWidth As Double, Optional ByVal TargetLevel As Double = Double.NaN) As Boolean
        Try
            'new in v1.79: this function extends the possibilities of the previous function getBedlevel
            'returns several profile attributes for a given distance on a reach
            Dim upProfile As clsSbkReachObject
            Dim downProfile As clsSbkReachObject
            Dim upDat As New clsProfileDatRecord(Me.Setup)
            Dim upDef As New clsProfileDefRecord(Me.Setup, SbkCase)
            Dim downDat As New clsProfileDatRecord(Me.Setup)
            Dim downDef As New clsProfileDefRecord(Me.Setup, SbkCase)
            Dim upBL As Double, downBL As Double
            Dim upMinFlowWidth As Double, downMinFlowWidth As Double
            Dim upMaxFlowWidth As Double, downMaxFlowWidth As Double
            Dim upFlowWidth As Double, downFlowWidth As Double
            Dim upDist As Double, dnDist As Double
            Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
            Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)

            upProfile = GetUpstreamObject(ReachesUpProcessed, myDist, upDist, False, True, False, False, True)
            downProfile = GetDownstreamObject(ReachesDnProcessed, myDist, dnDist, False, True, False, False, True)

            If Not upProfile Is Nothing Then
                upDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(upProfile.ID.Trim.ToUpper)
                If Not upDat Is Nothing Then upDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(upDat.di.Trim.ToUpper)
                If Not upDef Is Nothing Then
                    upBL = upDat.CalculateLowestBedLevel(upDef)
                    Call upDat.calculateMinFlowWidth(upDef)
                    Call upDat.calculateMaxFlowWidth(upDef)
                    upFlowWidth = upDat.calculateFlowWidthAtTargetLevel(upDef, TargetLevel)
                    upMinFlowWidth = upDat.calculateMinFlowWidth(upDef)
                    upMaxFlowWidth = upDat.calculateMaxFlowWidth(upDef)
                End If
            End If
            If Not downProfile Is Nothing Then
                downDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(downProfile.ID.Trim.ToUpper)
                If Not downDat Is Nothing Then downDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(downDat.di.Trim.ToUpper)
                If Not downDef Is Nothing Then
                    downBL = downDat.CalculateLowestBedLevel(downDef)
                    Call downDat.calculateMinFlowWidth(downDef)
                    Call downDat.calculateMaxFlowWidth(downDef)
                    downFlowWidth = downDat.calculateFlowWidthAtTargetLevel(downDef, TargetLevel)
                    downMinFlowWidth = downDat.calculateMinFlowWidth(downDef)
                    downMaxFlowWidth = downDat.calculateMaxFlowWidth(downDef)
                End If
            End If

            BedLevel = Double.NaN
            BedWidth = Double.NaN
            MaxWidth = Double.NaN
            FlowWidth = Double.NaN
            If Not upProfile Is Nothing AndAlso downProfile Is Nothing Then
                BedLevel = upBL
                BedWidth = upMinFlowWidth
                MaxWidth = upMaxFlowWidth
                FlowWidth = upFlowWidth
            ElseIf Not downProfile Is Nothing AndAlso upProfile Is Nothing Then
                BedLevel = downBL
                BedWidth = downMinFlowWidth
                MaxWidth = downMaxFlowWidth
                FlowWidth = downFlowWidth
            ElseIf Not upProfile Is Nothing AndAlso Not downProfile Is Nothing Then
                'interpolate values between the surrounding cross sections
                BedLevel = Setup.GeneralFunctions.Interpolate(0, upBL, (upDist + dnDist), downBL, upDist)
                BedWidth = Setup.GeneralFunctions.Interpolate(0, upMinFlowWidth, (upDist + dnDist), downMinFlowWidth, upDist)
                MaxWidth = Setup.GeneralFunctions.Interpolate(0, upMaxFlowWidth, (upDist + dnDist), downMaxFlowWidth, upDist)
                FlowWidth = Setup.GeneralFunctions.Interpolate(0, upFlowWidth, (upDist + dnDist), downFlowWidth, upDist)
            ElseIf upProfile Is Nothing AndAlso downProfile Is Nothing Then
                Return False
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getProfileAttributes of class clsSbkReach.")
            Return False
        End Try


    End Function

    Friend Function getBedLevel(ByVal myDist As Double, ByRef BedLevel As Double, Optional ByVal IncludeGroundLayer As Boolean = False) As Boolean
        Try
            'v2.0 redesigned for situations where the profile.dat or profile.def record are missing
            'geeft voor een gegeven afstand op de tak de bodemhoogte terug
            Dim upProfile As clsSbkReachObject
            Dim downProfile As clsSbkReachObject
            Dim upDat As New clsProfileDatRecord(Me.Setup)
            Dim upDef As New clsProfileDefRecord(Me.Setup, SbkCase)
            Dim downDat As New clsProfileDatRecord(Me.Setup)
            Dim downDef As New clsProfileDefRecord(Me.Setup, SbkCase)
            Dim upBL As Double, downBL As Double
            Dim upDist As Double, dnDist As Double
            Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
            Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
            BedLevel = Double.NaN                           'initialize the bed level to NaN to make sure we don't keep results from other reaches in memory

            upProfile = GetUpstreamObject(ReachesUpProcessed, myDist, upDist, False, True, False, False, True)
            downProfile = GetDownstreamObject(ReachesDnProcessed, myDist, dnDist, False, True, False, False, True)

            'v2.0: implemented error handling for situations where the .dat or .def record are not found
            If Not upProfile Is Nothing AndAlso upProfile.InUse Then
                If Not SbkCase.CFData.Data.ProfileData.getProfileDefinition(upProfile.ID, upDat, upDef) Then
                    upProfile.InUse = False
                    Me.Setup.Log.AddError("Error: missing data records for cross section " & upProfile.ID & ". Cross section has been disabled.")
                End If
                upBL = upDat.CalculateLowestBedLevel(upDef, IncludeGroundLayer)
            End If

            'v2.0: implemented error handling for situations where the .dat or .def record are not found
            If Not downProfile Is Nothing AndAlso downProfile.InUse Then
                If Not SbkCase.CFData.Data.ProfileData.getProfileDefinition(downProfile.ID, downDat, downDef) Then
                    downProfile.InUse = False
                    Me.Setup.Log.AddError("Error: missing data records for cross section " & downProfile.ID & ". Cross section has been disabled.")
                End If
                downBL = downDat.CalculateLowestBedLevel(downDef, IncludeGroundLayer)
            End If

            If Not upProfile Is Nothing AndAlso upProfile.InUse AndAlso Not downProfile Is Nothing AndAlso downProfile.InUse Then
                'interpolate bed levels between the surrounding cross sections
                BedLevel = Setup.GeneralFunctions.Interpolate(0, upBL, (upDist + dnDist), downBL, upDist)
            ElseIf Not upProfile Is Nothing AndAlso upProfile.InUse Then
                BedLevel = upBL
            ElseIf Not downProfile Is Nothing AndAlso downProfile.InUse Then
                BedLevel = downBL
            Else
                BedLevel = Double.NaN
                Return False
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getBedLevel of class clsSbkReach.")
            Return False
        End Try


    End Function

    Friend Function buildInitialDatRecordFromShapeFile(ByRef SubcatchmentDataSource As clsSubcatchmentDataSource, TargetlevelFieldIdx As Integer, ByVal upProfile As clsSbkReachObject, ByVal downProfile As clsSbkReachObject, ByVal minDepth As Double, ByVal addDepth As Double, ByRef InitialDatFLINRecord As clsInitialDatFLINRecord) As Boolean
        'deze routine bouwt een initial.dat record voor de onderhavige tak, aan de hand van de opgegeven area shapefile met streefpeil
        Dim startShapeIdx As Integer, endShapeIdx As Integer, TLStart As Double, TLEnd As Double, CPStart As clsSbkVectorPoint, CPEnd As clsSbkVectorPoint
        InitialDatFLINRecord = New clsInitialDatFLINRecord(Setup)

        Dim upDat As New clsProfileDatRecord(Me.Setup)
        Dim upDef As New clsProfileDefRecord(Me.Setup, SbkCase)
        Dim downDat As New clsProfileDatRecord(Me.Setup)
        Dim downDef As New clsProfileDefRecord(Me.Setup, SbkCase)
        Dim upBL As Double, downBL As Double
        Dim maxLv As Double
        Dim TLStartFound As Boolean, TLEndFound As Boolean

        Try
            If Not upProfile Is Nothing Then
                If Not upProfile.ID Is Nothing Then
                    upDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(upProfile.ID.Trim.ToUpper)
                    If Not upDat Is Nothing Then upDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(upDat.di.Trim.ToUpper)
                    If Not upDef Is Nothing Then upBL = upDat.CalculateLowestBedLevel(upDef)
                End If
            End If
            If Not downProfile Is Nothing Then
                If Not downProfile.ID Is Nothing Then
                    downDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(downProfile.ID.Trim.ToUpper)
                    If Not downDat Is Nothing Then downDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(downDat.di.Trim.ToUpper)
                    If Not downDef Is Nothing Then downBL = downDat.CalculateLowestBedLevel(downDef)
                End If
            End If

            'in case only one of the two was found
            If upProfile Is Nothing AndAlso downProfile Is Nothing Then Exit Function
            If upProfile Is Nothing OrElse upProfile.ID Is Nothing Then upBL = downBL
            If downProfile Is Nothing OrElse downProfile.ID Is Nothing Then downBL = upBL

            'zoek de start- en eindcoordinaten
            CPStart = NetworkcpRecord.CPTable.CP(0)
            CPEnd = NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1)

            'lees het streefpeil behorende bij de start- en eindknoop
            TLStartFound = False
            TLEndFound = False
            If SubcatchmentDataSource.PolySF.getUnderlyingShapeIdx(CPStart.X, CPStart.Y, startShapeIdx) Then
                TLStart = SubcatchmentDataSource.PolySF.sf.CellValue(TargetlevelFieldIdx, startShapeIdx)
                TLStartFound = True
            End If
            If SubcatchmentDataSource.PolySF.getUnderlyingShapeIdx(CPEnd.X, CPEnd.Y, endShapeIdx) Then
                TLEnd = SubcatchmentDataSource.PolySF.sf.CellValue(TargetlevelFieldIdx, endShapeIdx)
                TLEndFound = True
            End If

            'schrijf het initial.dat flin-record als initiele waterSTAND
            InitialDatFLINRecord.ID = Id
            InitialDatFLINRecord.ci = Id
            InitialDatFLINRecord.lc = 9999900000.0
            InitialDatFLINRecord.nm = "initial"
            InitialDatFLINRecord.ss = 0
            InitialDatFLINRecord.q_lq1 = 0
            InitialDatFLINRecord.q_lq2 = 0
            InitialDatFLINRecord.q_lq3 = 9999900000.0

            'EDIT: 7-7-2017
            'De routine om een initiele DIEPTE op te geven verwijderd
            'deze leidde tot crashes en de oorzaak heb ik nooit onderozcht.

            If TLStartFound AndAlso TLEndFound Then
                If TLStart = TLEnd Then
                    InitialDatFLINRecord.ty = 1     '1 =  water level, 0 = water depth
                    InitialDatFLINRecord.lv_ll1 = 0 'constant
                    InitialDatFLINRecord.lv_ll2 = TLStart + addDepth
                    InitialDatFLINRecord.lv_ll3 = 9999900000.0
                    'ElseIf Not upDat Is Nothing AndAlso Not upDef Is Nothing AndAlso Not downDat Is Nothing AndAlso Not downDef Is Nothing Then
                    '    'we hebben de waterdieptes. Leg de gemiddelde waterdiepte op
                    '    upDepth = TLStart - upBL
                    '    downDepth = TLEnd - downBL
                    '    myDepth = (upDepth + downDepth) / 2
                    '    If myDepth < minDepth Then myDepth = minDepth
                    '    InitialDatFLINRecord.ty = 0     '1 =  water level, 0 = water depth
                    '    InitialDatFLINRecord.lv_ll1 = 0 'constant
                    '    InitialDatFLINRecord.lv_ll2 = myDepth + addDepth
                    '    InitialDatFLINRecord.lv_ll3 = 9999900000.0
                Else
                    'neem het hoogste streefpeil en leg dit op
                    'SIEBE: later verbeteren door de diepte te berekenen en op te leggen
                    maxLv = Math.Max(TLStart, TLEnd)
                    InitialDatFLINRecord.ty = 1     '1 =  water level, 0 = water depth
                    InitialDatFLINRecord.lv_ll1 = 0 'constant
                    InitialDatFLINRecord.lv_ll2 = maxLv + addDepth
                    InitialDatFLINRecord.lv_ll3 = 9999900000.0
                End If
            ElseIf TLStartFound Then
                InitialDatFLINRecord.ty = 1     '1 =  water level, 0 = water depth
                InitialDatFLINRecord.lv_ll1 = 0 'constant
                InitialDatFLINRecord.lv_ll2 = TLStart + addDepth
                InitialDatFLINRecord.lv_ll3 = 9999900000.0
            ElseIf TLEndFound Then
                InitialDatFLINRecord.ty = 1     '1 =  water level, 0 = water depth
                InitialDatFLINRecord.lv_ll1 = 0 'constant
                InitialDatFLINRecord.lv_ll2 = TLEnd + addDepth
                InitialDatFLINRecord.lv_ll3 = 9999900000.0
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddWarning("Could not determine bed level fo reach " & Id & " and therefore no initial water level/depth either.")
            Return False
        End Try

    End Function


    Friend Function buildInitialDatRecordFromTargetLevels(TargetLevelType As GeneralFunctions.enmTargetLevelType, ByRef upProfile As clsSbkReachObject, ByRef downProfile As clsSbkReachObject, ByVal minDepth As Double, ByVal addDepth As Double, ByRef InitialDatFLINRecord As clsInitialDatFLINRecord) As Boolean
        'deze routine bouwt een initial.dat record voor de onderhavige tak, aan de hand van de opgegeven area shapefile met streefpeil
        Dim TLStart As Double, TLEnd As Double
        InitialDatFLINRecord = New clsInitialDatFLINRecord(Setup)

        Dim upDat As New clsProfileDatRecord(Me.Setup)
        Dim upDef As New clsProfileDefRecord(Me.Setup, SbkCase)
        Dim downDat As New clsProfileDatRecord(Me.Setup)
        Dim downDef As New clsProfileDefRecord(Me.Setup, SbkCase)
        Dim upBL As Double, downBL As Double
        Dim maxTL As Double, initWL As Double

        Try
            If Not upProfile Is Nothing Then
                If Not upProfile.ID Is Nothing Then
                    If Not SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.ContainsKey(upProfile.ID.Trim.ToUpper) Then
                        Me.Setup.Log.AddError("Possible error building initial.dat record for reach " & Id & ". No profile.def record found for " & upProfile.ID)
                    Else
                        upDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(upProfile.ID.Trim.ToUpper)
                        If Not upDat Is Nothing Then upDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(upDat.di.Trim.ToUpper)
                        If Not upDef Is Nothing Then upBL = upDat.CalculateLowestBedLevel(upDef)
                    End If
                End If
            End If
            If Not downProfile Is Nothing Then
                If Not downProfile.ID Is Nothing Then
                    If Not SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.ContainsKey(downProfile.ID.Trim.ToUpper) Then
                        Me.Setup.Log.AddError("Possible error building initial.dat record for reach " & Id & ". No profile.def record found for " & downProfile.ID)
                    Else
                        downDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(downProfile.ID.Trim.ToUpper)
                        If Not downDat Is Nothing Then downDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(downDat.di.Trim.ToUpper)
                        If Not downDef Is Nothing Then downBL = downDat.CalculateLowestBedLevel(downDef)
                    End If
                End If
            End If

            'in case only one of the two was found
            If upProfile Is Nothing AndAlso downProfile Is Nothing Then Throw New Exception("Error building initial.dat record for reach: " & Id)
            If upProfile Is Nothing OrElse upProfile.ID Is Nothing Then upBL = downBL
            If downProfile Is Nothing OrElse downProfile.ID Is Nothing Then downBL = upBL

            'lees het streefpeil behorende bij de start- en eindknoop
            If bn.TargetLevels IsNot Nothing Then
                Select Case TargetLevelType
                    Case Is = enmTargetLevelType.WinterOut
                        TLStart = bn.TargetLevels.getWPOutlet
                    Case Is = enmTargetLevelType.WinterIn
                        TLStart = bn.TargetLevels.getWPInlet
                    Case Is = enmTargetLevelType.SummerOut
                        TLStart = bn.TargetLevels.getZPOutlet
                    Case Is = enmTargetLevelType.SummerIn
                        TLStart = bn.TargetLevels.getZPInlet
                End Select
            Else
                'v2.114: changed Nothing to Double.Nan
                TLStart = Double.NaN
            End If

            If en.TargetLevels IsNot Nothing Then
                Select Case TargetLevelType
                    Case Is = enmTargetLevelType.WinterOut
                        TLEnd = en.TargetLevels.getWPOutlet
                    Case Is = enmTargetLevelType.WinterIn
                        TLEnd = en.TargetLevels.getWPInlet
                    Case Is = enmTargetLevelType.SummerOut
                        TLEnd = en.TargetLevels.getZPOutlet
                    Case Is = enmTargetLevelType.SummerIn
                        TLEnd = en.TargetLevels.getZPInlet
                End Select
            Else
                'v2.114: changed Nothing to Double.Nan
                TLEnd = Double.NaN
            End If

            'schrijf het initial.dat flin-record als initiele waterSTAND
            InitialDatFLINRecord.ID = Id
            InitialDatFLINRecord.ci = Id
            InitialDatFLINRecord.lc = 9999900000.0
            InitialDatFLINRecord.nm = "initial"
            InitialDatFLINRecord.ss = 0
            InitialDatFLINRecord.q_lq1 = 0
            InitialDatFLINRecord.q_lq2 = 0
            InitialDatFLINRecord.q_lq3 = 9999900000.0

            'v2.114: replaced the check on bn.TargetLevels is nothing and en.TargetLevels is nothing by Double.Isnan(TlStart) and Double.Isnan(TlEnd) 
            If Double.IsNaN(TLStart) AndAlso Double.IsNaN(TLEnd) Then
                Throw New Exception("Could Not create initial.dat record for reach " & Id & " since no target levels could be found for start and end node")
            ElseIf Double.IsNaN(TLEnd) Then
                TLEnd = TLStart
            Else
                TLStart = TLEnd
            End If

            'neem het hoogste van de beide streefpeilen en leg dit op
            maxTL = Math.Max(TLStart, TLEnd)
            initWL = maxTL                      'initialize the initial level to be the highest target level
            If initWL - Math.Max(upBL, downBL) < minDepth Then initWL = Math.Max(upBL, downBL) + minDepth
            initWL += addDepth

            InitialDatFLINRecord.ty = 1     '1 =  water level, 0 = water depth
            InitialDatFLINRecord.lv_ll1 = 0 'constant
            InitialDatFLINRecord.lv_ll2 = initWL
            InitialDatFLINRecord.lv_ll3 = 9999900000.0
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddWarning(ex.Message)
            Return False
        End Try

    End Function

    Friend Function getCalculationPointsSorted() As Dictionary(Of Double, clsSbkReachObject)
        'deze functie geeft een dictionary van alle rekenpunten op deze tak terug, gesorteerd naar oplopende afstand op de tak
        Dim myDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim sortedDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim myObj As clsSbkReachObject

        'vul de nieuwe dictionary
        For Each myObj In ReachObjects.ReachObjects.Values
            If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpoint OrElse myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpointFixed Then
                If Not myDict.ContainsKey(myObj.lc) Then myDict.Add(myObj.lc, myObj)
            End If
        Next

        'sorteer naar afstand op de tak
        sortedDict = (From entry In myDict Order By entry.Key Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
        Return sortedDict

    End Function

    Friend Function getObjectsOfTypeSorted(ObjectType As STOCHLIB.GeneralFunctions.enmNodetype) As Dictionary(Of Double, clsSbkReachObject)
        'deze functie geeft een dictionary van alle dwarsprofielen op deze tak terug, gesorteerd naar oplopende afstand op de tak
        Dim myDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim sortedDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim myObj As clsSbkReachObject

        'vul de nieuwe dictionary
        For Each myObj In ReachObjects.ReachObjects.Values
            If myObj.nt = ObjectType Then
                If Not myDict.ContainsKey(myObj.lc) Then myDict.Add(myObj.lc, myObj)
            End If
        Next

        'sorteer naar afstand op de tak
        sortedDict = (From entry In myDict Order By entry.Key Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
        Return sortedDict

    End Function

    Friend Function getStructuresSorted() As Dictionary(Of Double, clsSbkReachObject)
        'deze functie geeft een dictionary van alle dwarsprofielen op deze tak terug, gesorteerd naar oplopende afstand op de tak
        Dim myDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim sortedDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim myObj As clsSbkReachObject

        'vul de nieuwe dictionary
        For Each myObj In ReachObjects.ReachObjects.Values
            If Setup.GeneralFunctions.isStructure(myObj.nt) Then
                If Not myDict.ContainsKey(myObj.lc) Then myDict.Add(myObj.lc, myObj)
            End If
        Next

        'sorteer naar afstand op de tak
        sortedDict = (From entry In myDict Order By entry.Key Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
        Return sortedDict

    End Function

    Friend Function BuildCalculationGrid(ByVal ApplyAttributeLength As Boolean, ByVal OverAll As Double, ByVal MergeDist As Double, ByVal IDBase As String, ByVal Culverts As Double, ByVal Pumps As Double, ByVal Weirs As Double, ByVal Orifices As Double, ByVal Bridges As Double, ByVal OtherStructures As Double, ByVal QBounds As Double, ByVal HBounds As Double, Optional ByVal ReplaceCPs As Boolean = True, Optional ByVal ReplaceFixedCPs As Boolean = False, Optional ByVal AddIndexNumber As Boolean = False, Optional ByVal AddChainageToID As Boolean = False, Optional ByVal SkipReachesWithPrefix As String = "") As Boolean

        '------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        'Author: Siebe Bosch
        'Date: 30-6-2013
        'Description: creates a calculation grid for the underlying SOBEK-Reach
        '1-4-2020: added the ApplyAttributeLength clause
        '
        'EDIT 17-4-2017
        'I added measurement stations as well and gave them the same treatment as lateral nodes. Hope that works well in order to prevent measurement stations picking up waterlevels from the wrong side of a structure
        'EDIT 2-4-2020
        'Complete redesign of this entire function:
        '-Added the option ApplyAttributeLength (use the attribute length of structures) and redesigned the entire distribution routine for new calculation points
        '-Implemented a method to 'snap' the outer calculation segments to the start/end of the reach if possible
        '------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        Try
            'first: for reaches with a given prefix we will reduce the number of calculation points
            Dim ReducePoints As Boolean
            If SkipReachesWithPrefix <> "" AndAlso Left(Id, SkipReachesWithPrefix.Length).Trim.ToUpper = SkipReachesWithPrefix.Trim.ToUpper Then ReducePoints = True

            Dim myDict As New Dictionary(Of Double, clsSbkReachObject)
            Dim sortedDict As New Dictionary(Of Double, clsSbkReachObject)
            Dim curObj As clsSbkReachObject, nextObj As clsSbkReachObject, prevObj As clsSbkReachObject, newObj As clsSbkReachObject = Nothing, myObj As clsSbkReachObject
            Dim i As Integer
            Dim StructureLength As Double
            Dim Margin As Double = 0.1  'we use this margin to make sure no two adjacent calculation points end up together

            'remove all existing calculation points
            For i = ReachObjects.ReachObjects.Count - 1 To 0 Step -1
                If ReachObjects.ReachObjects.Values(i).nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpoint AndAlso ReplaceCPs Then ReachObjects.ReachObjects.Remove(ReachObjects.ReachObjects.Values(i).ID.Trim.ToUpper)
                If ReachObjects.ReachObjects.Values(i).nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpointFixed AndAlso ReplaceFixedCPs Then ReachObjects.ReachObjects.Remove(ReachObjects.ReachObjects.Values(i).ID.Trim.ToUpper)
            Next

            'add the starting point of the reach as a virtual node in the new dictionary
            myObj = New clsSbkReachObject(Setup, SbkCase)
            If bn.nt = enmNodetype.NodeCFBoundary AndAlso bn.isQBoundary Then
                myObj.nt = enmNodetype.NodeCFBoundary
                myObj.SubType = enmNodeSubType.QBoundary
                myObj.NeedsCalculationPoints = True
            ElseIf bn.nt = enmNodetype.NodeCFBoundary AndAlso bn.isHBoundary Then
                myObj.nt = enmNodetype.NodeCFBoundary
                myObj.SubType = enmNodeSubType.HBoundary
                myObj.NeedsCalculationPoints = False
            Else
                myObj.nt = enmNodetype.NodeCFConnectionNode
                myObj.NeedsCalculationPoints = False
            End If
            myObj.ID = "start"
            myObj.lc = 0
            If Not myDict.ContainsKey(myObj.lc) Then myDict.Add(myObj.lc, myObj)

            'add the endpoint of the reach as a virtual node in the new dictionary
            myObj = New clsSbkReachObject(Setup, SbkCase)
            If en.nt = enmNodetype.NodeCFBoundary AndAlso en.isQBoundary Then
                myObj.nt = enmNodetype.NodeCFBoundary
                myObj.SubType = enmNodeSubType.QBoundary
                myObj.NeedsCalculationPoints = True
            ElseIf en.nt = enmNodetype.NodeCFBoundary AndAlso en.isHBoundary Then
                myObj.nt = enmNodetype.NodeCFBoundary
                myObj.SubType = enmNodeSubType.HBoundary
                myObj.NeedsCalculationPoints = False
            Else
                myObj.nt = enmNodetype.NodeCFConnectionNode
                myObj.NeedsCalculationPoints = False
            End If
            myObj.ID = "end"
            myObj.lc = getReachLength()
            If Not myDict.ContainsKey(myObj.lc) Then myDict.Add(myObj.lc, myObj)

            If getReachLength() = 0 Then Throw New Exception("Reach with zero length found! " & Id)

            'populate the new dictionary with only the relevant reach objects
            For Each myObj In ReachObjects.ReachObjects.Values
                If myObj.InUse AndAlso (myObj.isStructure OrElse myObj.isLateral OrElse myObj.isLinkage OrElse myObj.isBoundary OrElse myObj.isMeas) Then
                    If myObj.isStructure Then myObj.NeedsCalculationPoints = True
                    If myObj.isLateral Then myObj.NeedsCalculationPoints = False
                    If myObj.isMeas Then myObj.NeedsCalculationPoints = False
                    If Not myDict.ContainsKey(myObj.lc) Then
                        If myObj.lc > getReachLength() Then
                            Me.Setup.Log.AddError("Error creating calculation point. Location of object " & myObj.ID & ": " & myObj.lc & " is beyond the reach length of " & getReachLength())
                        ElseIf myObj.lc < 0 Then
                            Me.Setup.Log.AddError("Error creating calculation point. Location of object " & myObj.ID & ": " & myObj.lc & " is beyond the reach length of " & getReachLength())
                        Else
                            myDict.Add(myObj.lc, myObj)
                        End If
                    End If
                End If
            Next

            'sort the objects by distance on the reach
            sortedDict = (From entry In myDict Order By entry.Key Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)

            'we will implement our calculation points in two cycles: first we will simply implemen them 'as is', so with their desired spacing
            'then we will check for overlapping segments and correct those

            '------------------------------------------------------------------------------------------------------------------------------
            'first set the desired spacing, depending on the type of object we're dealing with
            '------------------------------------------------------------------------------------------------------------------------------

            For i = 0 To sortedDict.Count - 1
                curObj = sortedDict.Values(i)

                Select Case curObj.nt
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge
                        'v1.72 added the structurelength > 0 clause to avoid calculation points exactly on the object
                        If ApplyAttributeLength AndAlso SbkCase.CFData.Data.GetStructureLength(curObj.ID, StructureLength) AndAlso StructureLength > 0 Then
                            FitReachObjectCalculationSegment(sortedDict, i, StructureLength, Margin)
                        Else
                            FitReachObjectCalculationSegment(sortedDict, i, Bridges, Margin)
                        End If
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFCulvert
                        'v1.72 added the structurelength > 0 clause to avoid calculation points exactly on the object
                        If ApplyAttributeLength AndAlso SbkCase.CFData.Data.GetStructureLength(curObj.ID, StructureLength) AndAlso StructureLength > 0 Then
                            FitReachObjectCalculationSegment(sortedDict, i, StructureLength, Margin)
                        Else
                            FitReachObjectCalculationSegment(sortedDict, i, Culverts, Margin)
                        End If
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice
                        FitReachObjectCalculationSegment(sortedDict, i, Orifices, Margin)
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump
                        FitReachObjectCalculationSegment(sortedDict, i, Pumps, Margin)
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir
                        FitReachObjectCalculationSegment(sortedDict, i, Weirs, Margin)
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir, STOCHLIB.GeneralFunctions.enmNodetype.NodeCFExtraResistance 'siebe 12-5-2020 added Extra Resistance nodes
                        FitReachObjectCalculationSegment(sortedDict, i, OtherStructures, Margin)
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary
                        If curObj.SubType = enmNodeSubType.QBoundary Then
                            FitReachNodeCalculationSegment(sortedDict, i, QBounds, Margin)
                        ElseIf curObj.SubType = enmNodeSubType.HBoundary Then
                            FitReachNodeCalculationSegment(sortedDict, i, HBounds, Margin)
                        End If
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.MeasurementStation
                        curObj.dxUp = Margin
                        curObj.dxDn = Margin
                    Case Else 'handle it as connection node
                        curObj.dxUp = 0
                        curObj.dxDn = 0
                End Select
            Next

            '------------------------------------------------------------------------------------------------------------------------------------------------------------
            ' now that we're sure that every object has a grid spacing that is in accordance with its settings and that actually fits in between both surrounding objects
            ' we can start distributing them in order to optimize the fit and minimize the number of calculation points
            '------------------------------------------------------------------------------------------------------------------------------------------------------------

            Dim OverlapUp As Double
            Dim OverlapDn As Double
            Dim Shift As Double
            Dim Done As Boolean = False
            Dim RemainingOverlap As Boolean = False
            Dim Iter As Integer

            'first we will try to shift 'n snap the calculation segment of the outer objects to resp. the start and end of the reach
            While Not Done
                Done = True
                Iter += 1
                For i = 1 To sortedDict.Values.Count - 2
                    prevObj = sortedDict.Values(i - 1)
                    curObj = sortedDict.Values(i)
                    nextObj = sortedDict.Values(i + 1)

                    If curObj.NeedsCalculationPoints Then
                        'start by trying to avoid having to add a calculation point to the upstream side
                        OverlapUp = (prevObj.lc + prevObj.dxDn) - (curObj.lc - curObj.dxUp)
                        OverlapDn = (curObj.lc + curObj.dxDn) - (nextObj.lc - nextObj.dxUp)
                        If OverlapUp > 0 AndAlso OverlapDn > 0 Then
                            'there is no use redistributing this cell since it overlaps on both sides. We will however need to initiate another iteration
                            Done = False
                        ElseIf OverlapDn > 0 Then
                            'there is only overlap on the downstream side. So move this object to upstream. However: make sure it does not overshoot the upstream object
                            Shift = Math.Min(OverlapDn, (curObj.lc - curObj.dxUp) - curObj.calcSegMin)
                            curObj.dxUp += Shift
                            curObj.dxDn -= Shift
                            Done = False
                        ElseIf OverlapUp > 0 Then
                            'there is only overlap on the upstream side. So move this object to downstream. However: make sure it does not overshoot the downstream object
                            Shift = Math.Min(OverlapUp, curObj.calcSegMax - (curObj.lc + curObj.dxDn))
                            curObj.dxUp -= Shift
                            curObj.dxDn += Shift
                            Done = False
                        End If
                    End If
                Next
                If Iter > sortedDict.Values.Count Then
                    Done = True  'safety valve to prevent eternal loop
                    RemainingOverlap = True
                End If
            End While

            'if there is remaining overlap we will now remove it by choosing the centerpoint
            If RemainingOverlap Then
                For i = 1 To sortedDict.Values.Count - 2
                    prevObj = sortedDict.Values(i - 1)
                    curObj = sortedDict.Values(i)
                    nextObj = sortedDict.Values(i + 1)

                    If curObj.NeedsCalculationPoints Then
                        OverlapUp = (prevObj.lc + prevObj.dxDn) - (curObj.lc - curObj.dxUp)
                        OverlapDn = (curObj.lc + curObj.dxDn) - (nextObj.lc - nextObj.dxUp)

                        If OverlapUp > 0 Then
                            prevObj.dxDn -= OverlapUp / 2
                            curObj.dxUp -= OverlapUp / 2
                        End If

                        If OverlapDn > 0 Then
                            nextObj.dxUp -= OverlapDn / 2
                            curObj.dxDn -= OverlapDn / 2
                        End If

                    End If
                Next
            End If

            '-------------------------------------------------------------------------------------------------------------------------------------------------
            'now that we're done we can start creating the calculation points
            '-------------------------------------------------------------------------------------------------------------------------------------------------
            If ReducePoints Then
                'for this reach we will limit ourselves to the absolute minimum number of calculation points required to make the simulation run
                For i = 1 To sortedDict.Values.Count - 1
                    prevObj = sortedDict.Values(i - 1)
                    curObj = sortedDict.Values(i)
                    If curObj.NeedsCalculationPoints AndAlso prevObj.NeedsCalculationPoints Then
                        'create just one calculation point
                        AddCalculationPoint(ReachObjects, True, Id, ((prevObj.lc + prevObj.dxDn) + (curObj.lc - curObj.dxUp)) / 2, IDBase, AddIndexNumber, AddChainageToID, True)
                    End If
                Next
            Else
                For i = 1 To sortedDict.Values.Count - 1
                    prevObj = sortedDict.Values(i - 1)
                    curObj = sortedDict.Values(i)

                    If curObj.NeedsCalculationPoints AndAlso prevObj.NeedsCalculationPoints Then
                        'let's see how close both objects are. If within range, merge both calculation points
                        If (curObj.lc - curObj.dxUp) - (prevObj.lc + prevObj.dxDn) < Margin Then
                            'if both calculation points lie within margin, merge them
                            AddCalculationPoint(ReachObjects, True, Id, ((prevObj.lc + prevObj.dxDn) + (curObj.lc - curObj.dxUp)) / 2, IDBase, AddIndexNumber, AddChainageToID, True)
                        ElseIf curObj.lc - prevObj.lc <= MergeDist Then
                            'also if both objects lie within merge distance, create just one calculation point
                            AddCalculationPoint(ReachObjects, True, Id, ((prevObj.lc + prevObj.dxDn) + (curObj.lc - curObj.dxUp)) / 2, IDBase, AddIndexNumber, AddChainageToID, True)
                        Else
                            'we need to create two separate calculation points
                            AddCalculationPoint(ReachObjects, True, Id, prevObj.lc + prevObj.dxDn, IDBase, AddIndexNumber, AddChainageToID, True)
                            AddCalculationPoint(ReachObjects, True, Id, curObj.lc - curObj.dxUp, IDBase, AddIndexNumber, AddChainageToID, True)

                            'and if the distance between both points exceeds our overall-distance, add regular points
                            If (curObj.lc - curObj.dxUp) - (prevObj.lc + prevObj.dxDn) > OverAll Then
                                AddCalculationPointsGapFill(ReachObjects, True, prevObj.lc + prevObj.dxDn, (curObj.lc - curObj.dxUp) - (prevObj.lc + prevObj.dxDn), OverAll, IDBase, AddIndexNumber, AddChainageToID, True)
                            End If

                        End If
                    ElseIf curObj.NeedsCalculationPoints AndAlso i = 1 Then
                        'we're at the start of the reach. See how close our desired gridpoint is to the start of the reach and decide whether to create a new one or not
                        If (curObj.lc - curObj.dxUp) > Math.Max(MergeDist, Margin) Then
                            AddCalculationPoint(ReachObjects, True, Id, curObj.lc - curObj.dxUp, IDBase, AddIndexNumber, AddChainageToID, True)
                        End If
                        'if the distance between both points exceeds our overall-distance, also add regular points
                        If (curObj.lc - curObj.dxUp) > OverAll Then
                            AddCalculationPointsGapFill(ReachObjects, True, 0, (curObj.lc - curObj.dxUp), OverAll, IDBase, AddIndexNumber, AddChainageToID, True)
                        End If
                    ElseIf i = 1 Then
                        'we're at the start of the reach but the first object does not need calculation points. This however does not excuses us from creating the overall grid
                        If (curObj.lc - curObj.dxUp) > OverAll Then
                            AddCalculationPointsGapFill(ReachObjects, True, 0, (curObj.lc - curObj.dxUp), OverAll, IDBase, AddIndexNumber, AddChainageToID, True)
                        End If
                    ElseIf curObj.NeedsCalculationPoints Then
                        'just create a calculation point for our current object
                        AddCalculationPoint(ReachObjects, True, Id, curObj.lc - curObj.dxUp, IDBase, AddIndexNumber, AddChainageToID, True)
                    ElseIf prevObj.NeedsCalculationPoints AndAlso i = sortedDict.Values.Count - 1 Then
                        'we're at the end of the reach. See how close our desired gridpoint is to the end of the reach and decide whether to create a calculation point
                        If (getReachLength() - (prevObj.lc + prevObj.dxDn)) > Math.Max(Margin, MergeDist) Then
                            AddCalculationPoint(ReachObjects, True, Id, prevObj.lc + prevObj.dxDn, IDBase, AddIndexNumber, AddChainageToID, True)
                        End If
                        'if the distance between both points exceeds our overall-distance, also add regular points
                        If (getReachLength() - (prevObj.lc + prevObj.dxDn)) > OverAll Then
                            AddCalculationPointsGapFill(ReachObjects, True, (prevObj.lc + prevObj.dxDn), getReachLength() - (prevObj.lc + prevObj.dxDn), OverAll, IDBase, AddIndexNumber, AddChainageToID, True)
                        End If
                    ElseIf i = sortedDict.Values.Count - 1 Then
                        'we're at the end of the reach but the last object does not need calculation points. This however does not excuses us from creating the overall grid
                        'v2.000: fixed a bug. Used curObj instead of prevObj
                        If (getReachLength() - (prevObj.lc + prevObj.dxDn)) > OverAll Then
                            AddCalculationPointsGapFill(ReachObjects, True, prevObj.lc + prevObj.dxDn, getReachLength() - (prevObj.lc + prevObj.dxDn), OverAll, IDBase, AddIndexNumber, AddChainageToID, True)
                        End If
                    ElseIf prevObj.NeedsCalculationPoints Then
                        'just create a calculation point for our previous object
                        AddCalculationPoint(ReachObjects, True, Id, prevObj.lc + prevObj.dxDn, IDBase, AddIndexNumber, AddChainageToID, True)
                    End If
                Next
            End If


            ''------------------------------------------------------------------------------------------------------------------------------
            ''finally deal with a special case: a reach without any calculationgrid-related objects
            ''------------------------------------------------------------------------------------------------------------------------------
            'If sortedDict.Count = 2 Then
            '    AddCalculationPointsGapFill(ReachObjects, True, 0, getReachLength, OverAll, IDBase, AddIndexNumber, AddChainageToID, True)
            'End If


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function BuildCalculationGrid of class clsSbkReach.")
            Return False
        End Try


    End Function

    Public Function FitReachObjectCalculationSegment(ByRef sortedDict As Dictionary(Of Double, clsSbkReachObject), i As Integer, SegmentSize As Double, Margin As Double) As Boolean
        'here we are fitting our calculation segment by comparing it to its surroundings
        'also we calculate the minimum allowed chainage for our calculation segment and the maximum allowed chainage
        Try
            Dim curObj As clsSbkReachObject = sortedDict.Values(i)
            Dim prevObj As clsSbkReachObject = sortedDict.Values(i - 1)
            Dim nextObj As clsSbkReachObject = sortedDict.Values(i + 1)
            Dim MaxSpace As Double

            'set the minimum allowed chainage for our calculation segment
            curObj.calcSegMin = prevObj.lc
            If i > 1 Then curObj.calcSegMin += Margin

            'set the maximum allowed chainage for our calculation segment
            curObj.calcSegMax = nextObj.lc
            If i < sortedDict.Values.Count - 2 Then curObj.calcSegMax -= Margin

            MaxSpace = curObj.calcSegMax - curObj.calcSegMin

            If SegmentSize <= MaxSpace Then
                'Theoretically we have sufficient space for our calculation segment for this object. We just need to position it in such a way that it fits.
                'first try to place our outer segments all the way to resp. the start or end of the reach
                If i = 1 AndAlso curObj.lc <= (SegmentSize - Margin) Then
                    curObj.dxUp = curObj.lc - prevObj.dxDn      'snap the object to the start of the reach. Siebe 12-05-2020 for v1.71 added the prevobj.dxdn since the start of the reach might also have a grid spacing
                    curObj.dxDn = SegmentSize - curObj.dxUp     'assign the remaning gridspacing to the downstream side of the object
                ElseIf i = sortedDict.Values.Count - 2 AndAlso (curObj.calcSegMax - curObj.lc) <= (SegmentSize - Margin) Then
                    curObj.dxDn = curObj.calcSegMax - curObj.lc 'snap the object to the end of the reach
                    curObj.dxUp = SegmentSize - curObj.dxDn     'assign the remaining gridspacing to the upstream side of the object
                Else
                    'for all other situations we will initially try to centralize the calculation segment around the object
                    If SegmentSize / 2 <= (curObj.lc - curObj.calcSegMin) AndAlso SegmentSize / 2 <= (curObj.calcSegMax - curObj.lc) Then
                        'we can centralize our object
                        curObj.dxUp = SegmentSize / 2
                        curObj.dxDn = SegmentSize / 2
                    Else
                        'weighing by the amount of space downstream/upstream of the object
                        curObj.dxUp = SegmentSize * (curObj.lc - curObj.calcSegMin) / (curObj.calcSegMax - curObj.calcSegMin)
                        curObj.dxDn = SegmentSize * (curObj.calcSegMax - curObj.lc) / (curObj.calcSegMax - curObj.calcSegMin)
                    End If
                End If
            Else
                'we have insufficient space for our calculation segment for this object so maximize its usage of space
                Me.Setup.Log.AddWarning("Insufficient space for calculation segment for object " & curObj.ID & ". Size of " & SegmentSize & " was reduced to " & (curObj.lc - curObj.calcSegMin) + (curObj.calcSegMax - curObj.lc) & ".")
                curObj.dxUp = curObj.lc - curObj.calcSegMin
                curObj.dxDn = curObj.calcSegMax - curObj.lc
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ValidateCalculationSegment of class clsSbkReach while processing reach " & Id & ": " & ex.Message)
            Return False
        End Try
    End Function

    Public Function FitReachNodeCalculationSegment(ByRef sortedDict As Dictionary(Of Double, clsSbkReachObject), i As Integer, SegmentSize As Double, Margin As Double) As Boolean
        'here we are fitting our calculation segment by comparing it to its surroundings
        Try
            Dim RightMax As Double
            Dim LeftMin As Double
            Dim curObj As clsSbkReachObject = sortedDict.Values(i)

            If i = 0 Then
                'set the maximum range for our desired calculation segment
                RightMax = sortedDict.Values(1).lc
                'siebe for v1.71: we ran into a problem of a weir that was too close to a boundary (5 cm). As a result the calculation point ended up 5cm before the boundary. Hence the Math.Max-implementation below.
                If sortedDict.Values.Count > 2 Then RightMax = Math.Max(RightMax - Margin, RightMax / 2)  'a margin only applies if the next node is NOT the end of the reach
                curObj.dxUp = 0
                curObj.dxDn = Math.Min(RightMax - curObj.lc, SegmentSize)
            ElseIf i = sortedDict.Values.Count - 1 Then
                'set the maximum range for our desired calculation segment
                LeftMin = sortedDict.Values(sortedDict.Values.Count - 2).lc
                'siebe for v1.71: we ran into a problem of a weir that was too close to a boundary (5 cm). As a result the calculation point ended up 5cm before the boundary. Hence the Math.Min-implementation below.
                If sortedDict.Values.Count > 2 Then LeftMin = Math.Min(LeftMin + Margin, (getReachLength() + LeftMin) / 2)    'a margin only applies if the previous node is NOT the start of the reach
                curObj.dxUp = Math.Min(curObj.lc - LeftMin, SegmentSize)
                curObj.dxDn = 0
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FitReachNodeCalculationSegment of class clsSbkReach.")
            Return False
        End Try
    End Function

    Public Function CramCalculationSegment(ByRef sortedDict As Dictionary(Of Double, clsSbkReachObject), PointIdx As Integer, Margin As Double) As Boolean
        Try
            'in this function we will move a calculation closer to the center. Cramming them together so to speak
            Dim CenterIdx As Double = (sortedDict.Count - 1) / 2
            Dim curObj As clsSbkReachObject = sortedDict.Values(PointIdx)
            Dim nextObj As clsSbkReachObject = sortedDict.Values(PointIdx + 1)
            Dim prevObj As clsSbkReachObject = sortedDict.Values(PointIdx - 1)

            Dim DesiredShift As Double
            Dim MaxShift As Double

            If (curObj.dxUp + curObj.dxDn) > 0 Then
                If PointIdx <= CenterIdx Then
                    'our current point is left from the center. So we'll have to move its calculation segment to the right
                    If curObj.lc + curObj.dxDn < nextObj.lc - nextObj.dxUp Then
                        DesiredShift = Math.Abs((curObj.lc + curObj.dxDn) - (nextObj.lc - nextObj.dxUp))
                        MaxShift = curObj.dxUp - Margin
                        curObj.dxDn += Math.Min(DesiredShift, MaxShift)
                        curObj.dxUp -= Math.Min(DesiredShift, MaxShift)
                    End If
                Else
                    'our current point is right from the center. So we'll have to move its calculation segment to the left
                    If curObj.lc - curObj.dxUp > prevObj.lc + prevObj.dxDn Then
                        DesiredShift = Math.Abs((curObj.lc - curObj.dxUp) - (prevObj.lc + prevObj.dxDn))
                        MaxShift = curObj.dxDn - Margin
                        curObj.dxUp += Math.Min(DesiredShift, MaxShift)
                        curObj.dxDn -= Math.Min(DesiredShift, MaxShift)
                    End If
                End If
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function CramCalculationSegment of class clsSbkReach.")
            Return False
        End Try

    End Function

    Public Function ShiftCalculationSegmentLeftToClickRight(ByRef sortedDict As Dictionary(Of Double, clsSbkReachObject), PointIdx As Integer, Margin As Double) As Boolean
        'in this function we will shift a calculation segment to the LEFT in order to click it (if possible) to the calculation segment of the object immediately to the right
        'this is only done/necessary if there is overlap
        'if a shift has been applied, this function returns TRUE else it returns FALSE
        Dim curObj As clsSbkReachObject = sortedDict.Values(PointIdx)
        Dim prevObj As clsSbkReachObject = sortedDict.Values(PointIdx - 1)
        Dim nextObj As clsSbkReachObject = sortedDict.Values(PointIdx + 1)
        Dim Overlap As Double, MaxShift As Double
        Dim ShiftApplied As Boolean = False

        'ony start this routine if we have a calculation segment to begin with
        If curObj.dxUp + curObj.dxDn > 0 Then

            'decide for our object the absolute minimum for our calculation segment and the absolute maximum
            Dim MinLeft As Double = prevObj.lc
            If PointIdx > 1 Then MinLeft += Margin
            Dim MaxRight As Double = nextObj.lc
            If PointIdx < sortedDict.Values.Count - 2 Then MaxRight -= Margin

            If (curObj.lc + curObj.dxDn) > (nextObj.lc - nextObj.dxUp) Then
                'we have found overlapping calculation segments!
                Overlap = Math.Abs((curObj.lc + curObj.dxDn) - (nextObj.lc - nextObj.dxUp))

                'now, we can only shift our calculation segment so far as the upstream segment allows us
                If (curObj.lc - curObj.dxUp) > MinLeft Then
                    MaxShift = Math.Abs((curObj.lc - curObj.dxUp) - MinLeft)
                    curObj.dxUp += Math.Min(Overlap, MaxShift)
                    curObj.dxDn -= Math.Min(Overlap, MaxShift)
                    ShiftApplied = True
                End If
            End If

        End If

        Return ShiftApplied


    End Function

    Public Function ShiftCalculationSegmentRightToClickLeft(ByRef sortedDict As Dictionary(Of Double, clsSbkReachObject), PointIdx As Integer, Margin As Double) As Boolean
        'in this function we will shift a calculation segment to the RIGHT in order to click it (if possible) to the calculation segment of the object immediately to the LEFT
        'this is only done/necessary if there is overlap
        'if a shift has been applied, this function returns TRUE else it returns FALSE
        Dim curObj As clsSbkReachObject = sortedDict.Values(PointIdx)
        Dim prevObj As clsSbkReachObject = sortedDict.Values(PointIdx - 1)
        Dim nextObj As clsSbkReachObject = sortedDict.Values(PointIdx + 1)
        Dim Overlap As Double, MaxShift As Double
        Dim ShiftApplied As Boolean = False

        If curObj.dxUp + curObj.dxDn > 0 Then

            'decide for our object the absolute minimum for our calculation segment and the absolute maximum
            Dim MinLeft As Double = prevObj.lc
            If PointIdx > 1 Then MinLeft += Margin
            Dim MaxRight As Double = nextObj.lc
            If PointIdx < sortedDict.Values.Count - 2 Then MaxRight -= Margin

            If (curObj.lc - curObj.dxUp) < (prevObj.lc + prevObj.dxDn) Then
                'we have found overlapping calculation segments!
                Overlap = Math.Abs((curObj.lc - curObj.dxUp) - (prevObj.lc + prevObj.dxDn))

                'now, we can only shift our calculation segment so far as the downstream segment allows us
                If (curObj.lc + curObj.dxDn) < MaxRight Then
                    MaxShift = Math.Abs(MaxRight - (curObj.lc + curObj.dxDn))
                    curObj.dxUp -= Math.Min(Overlap, MaxShift)
                    curObj.dxDn += Math.Min(Overlap, MaxShift)
                    ShiftApplied = True
                End If
            End If
        End If


        Return ShiftApplied

    End Function


    Friend Sub BuildCalculationGridTest(ByVal OverAll As Double, ByVal MergeDist As Double, ByVal IDBase As String, ByVal Culverts As Double, ByVal Pumps As Double, ByVal Weirs As Double, ByVal Orifices As Double, ByVal Bridges As Double, ByVal UniWeirs As Double, ByVal QBounds As Double, ByVal HBounds As Double, Optional ByVal ReplaceCPs As Boolean = True, Optional ByVal ReplaceFixedCPs As Boolean = False, Optional ByVal AddIndexNumber As Boolean = False, Optional ByVal AddChainageToID As Boolean = False)
        Dim sortedDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim newObj As clsSbkReachObject = Nothing

        '------------------------------------------------------------------------------------------------
        'Author: Siebe Bosch
        'Date: 30-6-2013
        'Description: creates a calculation grid for the underlying SOBEK-Reach
        '
        'EDIT 17-4-2017
        'I added measurement stations as well and gave them the same treatment as lateral nodes. Hope that works well in order to prevent measurement stations picking up waterlevels from the wrong side of a structure
        '------------------------------------------------------------------------------------------------

        ''remove all existing calculation points
        'For i = ReachObjects.ReachObjects.Count - 1 To 0 Step -1
        '    If ReachObjects.ReachObjects.Values(i).nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpoint AndAlso ReplaceCPs Then ReachObjects.ReachObjects.Remove(ReachObjects.ReachObjects.Values(i).ID.Trim.ToUpper)
        '    If ReachObjects.ReachObjects.Values(i).nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpointFixed AndAlso ReplaceFixedCPs Then ReachObjects.ReachObjects.Remove(ReachObjects.ReachObjects.Values(i).ID.Trim.ToUpper)
        'Next

        ''add the starting point of the reach as a virtual node in the new dictionary
        'myObj = New clsSbkReachObject(Setup, SbkCase)
        'If bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary AndAlso bn.isQBoundary Then
        '    myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.QBoundary
        'ElseIf bn.nt = enmNodetype.NodeCFBoundary AndAlso bn.isHboundary Then
        '    myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.HBoundary
        'Else
        '    myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFConnectionNode   'add the start node as if it were a connection node (heck, maybe it even is one)
        'End If
        'myObj.ID = "start"
        'myObj.lc = 0
        'If Not ReachObjects.ReachObjects.ContainsKey(myObj.lc) Then ReachObjects.ReachObjects.Add(myObj.lc, myObj)

        ''add the endpoint of the reach as a virtual node in the new dictionary
        'myObj = New clsSbkReachObject(Setup, SbkCase)
        'If en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary AndAlso en.isQBoundary Then
        '    myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.QBoundary
        'ElseIf en.nt = enmNodetype.HBoundary AndAlso en.isHboundary Then
        '    myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.HBoundary
        'Else
        '    myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFConnectionNode   'add the end node as if it were a connection node (heck, maybe it even is one)
        'End If
        'myObj.ID = "end"
        'myObj.lc = getReachLength()
        'If Not ReachObjects.ReachObjects.ContainsKey(myObj.lc) Then ReachObjects.ReachObjects.Add(myObj.lc, myObj)

        'If getReachLength() = 0 Then
        '    Me.Setup.Log.AddError("Reach with zero length found! " & Id)
        '    Exit Sub
        'End If

        ''we'll rebuild the entire routine from scratch. We'll walk through the reach and start by implementing ALL required calculation points
        'Dim AddPoints As New List(Of Double)    'keeps track of the chainages where we want to add our calculation points
        'For Each myObj In ReachObjects.ReachObjects.Values
        '    Select Case myObj.nt
        '        Case Is = GeneralFunctions.enmNodetype.HBoundary
        '            If HBounds > 0 AndAlso myObj.lc = 0 Then AddPoints.Add(myObj.lc + HBounds) ' addCalculationPoint(ReachObjects, True, Id, myObj.lc + HBounds, IDBase, AddIndexNumber, AddChainageToID, True)
        '            If HBounds > 0 AndAlso myObj.lc > getReachLength() - HBounds Then AddPoints.Add(myObj.lc - HBounds) 'addCalculationPoint(ReachObjects, True, Id, myObj.lc - HBounds, IDBase, AddIndexNumber, AddChainageToID, True)
        '        Case Is = GeneralFunctions.enmNodetype.QBoundary
        '            If QBounds > 0 AndAlso myObj.lc = 0 Then AddPoints.Add(myObj.lc + QBounds) ' addCalculationPoint(ReachObjects, True, Id, myObj.lc + QBounds, IDBase, AddIndexNumber, AddChainageToID, True)
        '            If QBounds > 0 AndAlso myObj.lc > getReachLength() - QBounds Then AddPoints.Add(myObj.lc - QBounds) 'addCalculationPoint(ReachObjects, True, Id, myObj.lc - QBounds, IDBase, AddIndexNumber, AddChainageToID, True)
        '        Case Is = GeneralFunctions.enmNodetype.NodeCFWeir
        '            If Weirs > 0 AndAlso myObj.lc > Weirs / 2 Then AddPoints.Add(myObj.lc - Weirs / 2)
        '            If Weirs > 0 AndAlso myObj.lc < getReachLength() - Weirs / 2 Then AddPoints.Add(myObj.lc + Weirs / 2)
        '        Case Is = GeneralFunctions.enmNodetype.NodeCFUniWeir
        '            If UniWeirs > 0 AndAlso myObj.lc > UniWeirs / 2 Then AddPoints.Add(myObj.lc - UniWeirs / 2) ' addCalculationPoint(ReachObjects, True, Id, myObj.lc - UniWeirs / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '            If UniWeirs > 0 AndAlso myObj.lc < getReachLength() - UniWeirs / 2 Then AddPoints.Add(myObj.lc + UniWeirs / 2) ' addCalculationPoint(ReachObjects, True, Id, myObj.lc + UniWeirs / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '        Case Is = GeneralFunctions.enmNodetype.NodeCFOrifice
        '            If Orifices > 0 AndAlso myObj.lc > Orifices / 2 Then AddPoints.Add(myObj.lc - Orifices / 2) 'addCalculationPoint(ReachObjects, True, Id, myObj.lc - Orifices / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '            If Orifices > 0 AndAlso myObj.lc < getReachLength() - Orifices / 2 Then AddPoints.Add(myObj.lc + Orifices / 2) ' addCalculationPoint(ReachObjects, True, Id, myObj.lc + Orifices / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '        Case Is = GeneralFunctions.enmNodetype.NodeCFCulvert
        '            If Culverts > 0 AndAlso myObj.lc > Culverts / 2 Then AddPoints.Add(myObj.lc - Culverts / 2) 'addCalculationPoint(ReachObjects, True, Id, myObj.lc - Culverts / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '            If Culverts > 0 AndAlso myObj.lc < getReachLength() - Culverts / 2 Then AddPoints.Add(myObj.lc + Culverts / 2) ' addCalculationPoint(ReachObjects, True, Id, myObj.lc + Culverts / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '        Case Is = GeneralFunctions.enmNodetype.NodeCFPump
        '            If Pumps > 0 AndAlso myObj.lc > Pumps / 2 Then AddPoints.Add(myObj.lc - Pumps / 2) ' addCalculationPoint(ReachObjects, True, Id, myObj.lc - Pumps / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '            If Pumps > 0 AndAlso myObj.lc < getReachLength() - Pumps / 2 Then AddPoints.Add(myObj.lc + Pumps / 2) ' addCalculationPoint(ReachObjects, True, Id, myObj.lc + Pumps / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '        Case Is = GeneralFunctions.enmNodetype.NodeCFBridge
        '            If Bridges > 0 AndAlso myObj.lc > Bridges / 2 Then AddPoints.Add(myObj.lc - Bridges / 2) 'addCalculationPoint(ReachObjects, True, Id, myObj.lc - Bridges / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '            If Bridges > 0 AndAlso myObj.lc < getReachLength() - Bridges / 2 Then AddPoints.Add(myObj.lc + Bridges / 2) 'addCalculationPoint(ReachObjects, True, Id, myObj.lc + Bridges / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        '    End Select
        'Next

        ''add the points to the reach objects list
        'For i = 0 To AddPoints.Count - 1
        '    addCalculationPoint(ReachObjects, True, Id, AddPoints(i), IDBase, AddIndexNumber, AddChainageToID, True)
        'Next
        'ReachObjects.Sort()


        ''next we will attempt to reduce the number of calculation points between structures, where allowed
        'Dim FirstSegmentStructure As Boolean, SecondSegmentStructure As Boolean
        'AddPoints = New List(Of Double)
        'For i = 0 To ReachObjects.ReachObjects.Count - 3
        '    prevObj = ReachObjects.ReachObjects.Values(i)
        '    If prevObj.isWaterLevelObject Then
        '        FirstSegmentStructure = False
        '        SecondSegmentStructure = False
        '        For j = i + 1 To ReachObjects.ReachObjects.Count - 2
        '            curObj = ReachObjects.ReachObjects.Values(j)
        '            If curObj.isWaterLevelObject Then
        '                For k = j + 1 To ReachObjects.ReachObjects.Count - 1
        '                    nextObj = ReachObjects.ReachObjects.Values(k)
        '                    If nextObj.isWaterLevelObject Then

        '                    End If
        '                Next
        '            ElseIf curObj.isStructure Then
        '                FirstSegmentStructure = True
        '            End If
        '        Next

        '    End If


        '    If prevObj.isWaterLevelObject Then
        '        Dim StructureDetected As Boolean = False
        '        Dim LateralDetected As Boolean = False
        '        For j = i + 1 To ReachObjects.ReachObjects.Count - 1
        '            curObj = ReachObjects.ReachObjects.Values(j)
        '            If curObj.nt = enmNodetype.NodeCFGridpointFixed OrElse curObj.nt = enmNodetype.NodeCFGridpoint Then
        '                If curObj.lc - prevObj.lc < MergeDist AndAlso Not StructureDetected Then
        '                    prevObj.InUse = False
        '                    curObj.InUse = False
        '                    AddPoints.Add((prevObj.lc + curObj.lc) / 2)
        '                End If
        '                Exit For
        '            ElseIf curObj.isStructure Then
        '                StructureDetected = True
        '            ElseIf curObj.isLateral Then
        '                LateralDetected = True
        '            End If
        '        Next
        '    End If
        'Next

        ''add the points to the reach objects list
        'For i = 0 To AddPoints.Count - 1
        '    addCalculationPoint(ReachObjects, True, Id, AddPoints(i), IDBase, AddIndexNumber, AddChainageToID, True)
        'Next
        'ReachObjects.Sort()

        ''now add the regular grid points
        'AddPoints = New List(Of Double)
        'Dim myDist As Double, myChainage As Double
        'For i = 0 To ReachObjects.ReachObjects.Count - 2
        '    prevObj = ReachObjects.ReachObjects.Values(i)
        '    If prevObj.isWaterLevelObject Then
        '        For j = i + 1 To ReachObjects.ReachObjects.Count - 1
        '            curObj = ReachObjects.ReachObjects.Values(j)
        '            If curObj.isWaterLevelObject Then
        '                'measure the distance between both gridpoints
        '                myDist = curObj.lc - prevObj.lc
        '                'if the distance between both gridpoints exceeds the over-all gridpoint distance, add the points
        '                If myDist > OverAll Then
        '                    'while we're more than two times the over-all distance from the next calculation point, add a point
        '                    myChainage = prevObj.lc
        '                    While (curObj.lc - myChainage) > OverAll * 2
        '                        myChainage += OverAll
        '                        AddPoints.Add(myChainage)
        '                    End While
        '                    'place the final one in the middle
        '                    If (curObj.lc - myChainage) > OverAll Then
        '                        myChainage += (curObj.lc - myChainage) / 2
        '                        AddPoints.Add(myChainage)
        '                    End If
        '                End If
        '                Exit For
        '            End If
        '        Next
        '    End If
        'Next

        ''add the points to the reach objects list
        'For i = 0 To AddPoints.Count - 1
        '    addCalculationPoint(ReachObjects, True, Id, AddPoints(i), IDBase, AddIndexNumber, AddChainageToID, True)
        'Next
        'ReachObjects.Sort()

        ''finally we will walk through all items and see if we're lacking some calculation points
        'AddPoints = New List(Of Double)
        'For i = 1 To ReachObjects.ReachObjects.Count - 1
        '    prevObj = ReachObjects.ReachObjects.Values(i - 1)
        '    curObj = ReachObjects.ReachObjects.Values(i)

        '    Dim AddPoint As Boolean = False
        '    If prevObj.isStructure AndAlso curObj.isStructure Then AddPoint = True
        '    If prevObj.isStructure AndAlso curObj.isLateral Then AddPoint = True
        '    If prevObj.isLateral AndAlso curObj.isStructure Then AddPoint = True
        '    If AddPoint Then AddPoints.Add((prevObj.lc + curObj.lc) / 2) ' addCalculationPoint(ReachObjects, True, Id, (prevObj.lc + curObj.lc) / 2, IDBase, AddIndexNumber, AddChainageToID, True)
        'Next

        ''add the points to the reach objects list
        'For i = 0 To AddPoints.Count - 1
        '    addCalculationPoint(ReachObjects, True, Id, AddPoints(i), IDBase, AddIndexNumber, AddChainageToID, True)
        'Next
        'ReachObjects.Sort()


    End Sub
    Friend Function AddCalculationPointsGapFill(ByRef ReachObjects As clsSbkReachObjects, ByVal isFixed As Boolean, ByVal StartChainage As Double, GapSize As Double, MaxSegmentSize As Double, ByVal IDBase As String, ByVal AddIndexNumber As Boolean, ByVal AddChainage As Boolean, SetInuse As Boolean) As Boolean
        Try
            Dim iPoint As Integer
            If GapSize > MaxSegmentSize Then
                Dim nPoints As Integer = Me.Setup.GeneralFunctions.RoundUD(GapSize / MaxSegmentSize, 0, False)
                Dim segSize As Double = GapSize / (nPoints + 1)
                For iPoint = 1 To nPoints
                    AddCalculationPoint(ReachObjects, isFixed, Id, StartChainage + iPoint * segSize, IDBase, AddIndexNumber, AddChainage, SetInuse)
                Next
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function addCalculationPointsGapFill of class clsSbkReach.")
            Return False
        End Try
    End Function
    Friend Function AddCalculationPoint(ByRef ReachObjects As clsSbkReachObjects, ByVal isFixed As Boolean, ByVal ReachID As String, ByVal lc As Double, ByVal IDBase As String, ByVal AddIndexNumber As Boolean, ByVal AddChainage As Boolean, SetInuse As Boolean) As Boolean
        Dim newObj As clsSbkReachObject

        newObj = New clsSbkReachObject(Setup, SbkCase)
        If isFixed Then
            newObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpointFixed
        Else
            newObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpoint
        End If
        newObj.lc = lc
        newObj.ci = ReachID
        newObj.InUse = SetInuse
        If AddChainage Then
            newObj.ID = IDBase.Trim & "_" & Int(lc).ToString.Trim
        Else
            newObj.ID = IDBase.Trim & "_" & ReachObjects.ReachObjects.Count
        End If
        newObj.calcXY() 'berekent de bijbehorende XY-coordinaat
        If Not ReachObjects.ReachObjects.ContainsKey(newObj.ID.Trim.ToUpper) Then
            ReachObjects.ReachObjects.Add(newObj.ID.Trim.ToUpper, newObj)
            Return True
        Else
            Return False
        End If

    End Function


    Friend Sub fillEmptySpaceWithCalculationPoints(ByRef ReachObjects As clsSbkReachObjects, ByVal isFixed As Boolean, ByVal IDBase As String, ByVal Dist As Double, ByVal startlc As Double, ByVal endlc As Double, ByVal AddIndexNumber As Boolean, ByVal AddChainageToID As Boolean)
        'deze routine creert evenredig verspreide rekenpunten over een tak, gegeven startlocatie, eindlocatie en rekenpuntenafstand
        Dim TotalDist As Double = endlc - startlc
        Dim nSegments As Long = Me.Setup.GeneralFunctions.RoundUD(TotalDist / Dist, 0, True)
        Dim iPoint As Long, nPoints As Long = nSegments - 1
        Dim segLength As Double = TotalDist / nSegments

        If nPoints > 0 Then
            For iPoint = 1 To nPoints
                AddCalculationPoint(ReachObjects, isFixed, Id, startlc + iPoint * segLength, IDBase, AddIndexNumber, AddChainageToID, True)
            Next
        End If

    End Sub

    Friend Function OptimizeReachObjectLocations(ByVal MinDistBetweenReachObjects As Double) As Boolean

        Try
            Dim myObj As clsSbkReachObject, prevObj As clsSbkReachObject
            Dim i As Integer = 0
            Dim nObjects As Integer = ReachObjects.ReachObjects.Count
            Dim Done As Boolean = False

            'first find out if it will fit in the first place
            Dim ReachLen As Double = NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1).Dist
            If ReachLen / (ReachObjects.ReachObjects.Count + 1) > MinDistBetweenReachObjects AndAlso ReachObjects.ReachObjects.Count > 0 Then
                'sort the reach objects by distance
                Dim objList As New List(Of clsSbkReachObject)
                For Each myObj In ReachObjects.ReachObjects.Values
                    objList.Add(myObj)
                Next
                Dim query As IEnumerable(Of clsSbkReachObject) = objList.OrderBy(Function(clsSbkReachObject) clsSbkReachObject.lc)

                'first the lone wolves :)
                If query.Count = 1 AndAlso ReachLen >= (2 * MinDistBetweenReachObjects) Then
                    If query(0).lc <= MinDistBetweenReachObjects Then query(0).lc = MinDistBetweenReachObjects
                    If query(0).lc >= (ReachLen - MinDistBetweenReachObjects) Then query(0).lc = (ReachLen - MinDistBetweenReachObjects)
                End If

                'walk through all reach objects and force the minimum distances
                If query.Count > 1 Then
                    For i = 1 To query.Count - 1
                        myObj = query(i)
                        prevObj = query(i - 1)
                        If (myObj.lc - prevObj.lc) <= MinDistBetweenReachObjects Then
                            myObj.lc = prevObj.lc + MinDistBetweenReachObjects
                        End If
                    Next
                End If

                i = 0
                While Not Done
                    i += 1
                    If Double.IsNaN(query(0).lc) Then Throw New Exception("Error in function OptimizeReachObjectLocations: chainage NaN for node " & query(0).ID & " and reach " & Id)
                    If Double.IsNaN(query(query.Count - 1).lc) Then Throw New Exception("Error in function OptimizeReachObjectLocations: chainage NaN for node " & query(query.Count - 1).ID & " and reach " & Id)

                    'check of de totale afstand tussen eerste en laatste object nog altijd passen op de tak
                    If (query(query.Count - 1).lc - query(0).lc) <= (ReachLen - (2 * MinDistBetweenReachObjects)) Then
                        'het past nog! Schuif nu zodanig heen of terug over de tak dat we nog altijd helemaal op de tak passsen
                        Dim Overshoot As Double = query(query.Count - 1).lc - (ReachLen - MinDistBetweenReachObjects)
                        Dim Undershoot As Double = query(0).lc - MinDistBetweenReachObjects
                        If Overshoot > 0 Then
                            For Each myObj In query
                                myObj.lc = myObj.lc - Overshoot
                            Next
                        End If
                        If Undershoot < 0 Then
                            For Each myObj In query
                                myObj.lc = myObj.lc - Undershoot
                            Next
                        End If
                        Done = True
                    Else
                        'het past niet langer. We moeten de boel wat in elkaar schuiven tot het wel weer past
                        'haal van alle tussenafstanden een meter van af, natuurlijk gelimiteerd tot de opgegeven minimumwaarde
                        For i = 1 To query.Count - 1
                            Dim Dist As Double
                            myObj = query(i)
                            prevObj = query(i - 1)
                            Dist = Math.Max(MinDistBetweenReachObjects, (myObj.lc - prevObj.lc - 1))
                            myObj.lc = prevObj.lc + Dist
                        Next
                        If i > 100 Then Done = True 'veiligheidsklep
                    End If
                End While
            Else
                'v2.109 the number of objects is too large to meet the minimum distance requirements between our objects. So now we will distribute our objects evenly
                'sort the reach objects by distance
                Dim objList As New List(Of clsSbkReachObject)
                For Each myObj In ReachObjects.ReachObjects.Values
                    objList.Add(myObj)
                Next
                Dim query As IEnumerable(Of clsSbkReachObject) = objList.OrderBy(Function(clsSbkReachObject) clsSbkReachObject.lc)

                'calculate the average distance we can assume for our objects
                Dim segLen As Double = getReachLength() / (objList.Count + 1)

                'walk through all reach objects and adjust their chainage
                If query.Count > 1 Then
                    For i = 0 To query.Count - 1
                        myObj = query(i)
                        myObj.lc = (i + 1) * segLen
                    Next
                End If

            End If
            Return True
        Catch ex As Exception
            Return False
            Me.Setup.Log.AddError(ex.Message)
        End Try

    End Function

    Friend Sub OptimizeReachObjectLocation(ByRef myObj As clsSbkReachObject, Optional ByVal MinDistFromReachObjects As Integer = 2)
        Dim i As Long = 0
        Dim nObjects As Integer = ReachObjects.ReachObjects.Count
        Dim exObj As clsSbkReachObject
        Dim minDist As Double = 9999999999, mini As Long
        Dim Done As Boolean = False
        Dim ReachLength As Long = Me.Setup.GeneralFunctions.RoundUD(getReachLength(), 0, False)

        'first sort all existing reach objects by ascending distance
        Dim objList As New List(Of clsSbkReachObject)
        For Each exObj In ReachObjects.ReachObjects.Values
            objList.Add(exObj)
        Next
        Dim query As IEnumerable(Of clsSbkReachObject) = objList.OrderBy(Function(clsSbkReachObject) clsSbkReachObject.lc)

        'first move in positive direction, starting with the current object location
        'and try to find an empty space where our object can be placed
        For i = Me.Setup.GeneralFunctions.RoundUD(myObj.lc, 0, True) To ReachLength
            Done = True
            For Each exObj In ReachObjects.ReachObjects.Values
                If Math.Abs(exObj.lc - i) < MinDistFromReachObjects Then
                    'existing reach object too close
                    Done = False
                End If
            Next
            If Done Then
                mini = i
                minDist = Math.Abs(i - myObj.lc)
                Exit For
            End If
        Next

        'next move in negative direction, starting with the current object location
        'and try to find an even closer empty space where our object van be placed
        For i = Me.Setup.GeneralFunctions.RoundUD(myObj.lc, 0, False) To 0 Step -1
            Done = True
            For Each exObj In ReachObjects.ReachObjects.Values
                If Math.Abs(exObj.lc - i) < MinDistFromReachObjects Then
                    'existing reach object too lcase
                    Done = False
                End If
            Next
            If Done Then
                If Math.Abs(i - myObj.lc) < minDist Then
                    mini = i
                    minDist = Math.Abs(i - myObj.lc)
                    Exit For
                End If
            End If
        Next

        'location found, so apply it to the object
        myObj.lc = mini

    End Sub

    Friend Sub OptimizeCalculationGrid(ByVal minDist As Double)
        Dim myDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim sortedDict As New Dictionary(Of Double, clsSbkReachObject)
        Dim curObj As clsSbkReachObject, prevObj As clsSbkReachObject, nextObj As clsSbkReachObject
        Dim myObj As clsSbkReachObject
        Dim i As Long

        'voeg het nulpunt van de tak toe als virtuele knoop in de nieuwe collectie
        myObj = New clsSbkReachObject(Setup, SbkCase)
        myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
        myObj.ID = "start"
        myObj.lc = 0
        myDict.Add(myObj.lc, myObj)

        'voeg ook het eindpunt van de tak toe als virtuele knoop
        myObj = New clsSbkReachObject(Setup, SbkCase)
        myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
        myObj.ID = "end"
        myObj.lc = getReachLength()
        If Not myDict.ContainsKey(myObj.lc) Then myDict.Add(myObj.lc, myObj)

        If getReachLength() = 0 Then Me.Setup.Log.AddMessage("Reach with zero length encountered! " & Id)

        'vul de nieuwe (virtuele) dictionary met alle takobjecten
        For Each myObj In ReachObjects.ReachObjects.Values
            If Not myDict.ContainsKey(myObj.lc) Then myDict.Add(myObj.lc, myObj)
        Next

        'sorteer ze naar afstand op de tak
        sortedDict = (From entry In myDict Order By entry.Key Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)

        'pas nu de ligging van elk rekenpunt zo aan dat hij past tussen de omringende takobjecten
        For i = 1 To sortedDict.Values.Count - 2
            prevObj = sortedDict.Values(i - 1)
            curObj = sortedDict.Values(i)
            nextObj = sortedDict.Values(i + 1)
            Dim myDist As Double = nextObj.lc - prevObj.lc

            If (curObj.lc - prevObj.lc) < minDist Then
                'hij voldoet momenteel niet aan de eisen. optimaliseren dus!
                If myDist > 2 * minDist Then
                    curObj.lc = prevObj.lc + minDist
                Else
                    'hij kan uberhaupt niet aan de eisen voldoen. Dan maar exact in het midden leggen
                    curObj.lc = prevObj.lc + (nextObj.lc - prevObj.lc) / 2
                End If
            ElseIf (nextObj.lc - curObj.lc) < minDist Then
                'hij voldoet momenteel niet aan de eisen. optimaliseren dus!
                If myDist > 2 * minDist Then
                    curObj.lc = nextObj.lc - minDist
                Else
                    'hij kan uberhaupt niet aan de eisen voldoen. Dan maar exact in het midden leggen
                    curObj.lc = prevObj.lc + (nextObj.lc - prevObj.lc) / 2
                End If
            End If
        Next
    End Sub

    Friend Function GetUpstreamObject(ByRef ReachesProcessed As Dictionary(Of String, clsSbkReach), ByVal myChainage As Double, ByRef upDist As Double, ByVal Structures As Boolean, ByVal Profiles As Boolean,
                                      ByVal Laterals As Boolean, ByVal WaterlevelPoints As Boolean, Optional ByVal IncludeInterpolatedReaches As Boolean = False) As clsSbkReachObject
        Try
            '--------------------------------------------------------------------------------------------------------------
            'Datum: 26-05-2012
            'Auteur: Siebe Bosch
            'Deze functie zoekt, gegeven een afstand op de tak, het dichtstbijzijnde bovenstrooms gelegen takobject van een gegeven type
            '--------------------------------------------------------------------------------------------------------------
            Dim minDist As Double = 9999999999
            Dim nearestObject As clsSbkReachObject = Nothing
            Dim IncludeObject As Boolean = False

            'in case we're looking for a cross section on an urban pipe, we don't have an actual object at our disposal. 
            'in that case we will return a fake object for the reach's centerpoint
            If Profiles AndAlso IsPipe() AndAlso myChainage >= getReachLength() / 2 Then
                Dim myObj As New clsSbkReachObject(Me.Setup, Me.SbkCase)
                myObj.ID = "l_" & Id        'siebe: let op: eigenlijk worden pipes behandeld als links; niet als reaches. Het is niet zeker of het hier dus goed gaat. Het moet eigenlijk "l_" & LinkID zijn of zo
                myObj.ci = Id
                myObj.lc = getReachLength() / 2
                myObj.nt = enmNodetype.SBK_PROFILE
                Return myObj
            Else
                For Each myObject As clsSbkReachObject In ReachObjects.ReachObjects.Values
                    If Structures = True AndAlso Setup.GeneralFunctions.isStructure(myObject.nt) AndAlso myObject.InUse Then
                        IncludeObject = True
                    ElseIf Profiles = True AndAlso (myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE) AndAlso myObject.InUse Then
                        IncludeObject = True
                    ElseIf Laterals = True AndAlso myObject.isLateral AndAlso myObject.InUse Then
                        IncludeObject = True
                    ElseIf WaterlevelPoints = True AndAlso myObject.isWaterLevelObject AndAlso myObject.InUse Then
                        IncludeObject = True
                    Else
                        IncludeObject = False
                    End If

                    If IncludeObject Then
                        If myObject.lc < myChainage Then
                            If (myChainage - myObject.lc) < minDist Then
                                minDist = (myChainage - myObject.lc)
                                nearestObject = myObject
                            End If
                        End If
                    End If
                Next myObject

                'v1.79: implemented a safety valve to avoid eternal loops. This could happen when one of the reaches has the same start- and endpoint
                If nearestObject Is Nothing AndAlso IncludeInterpolatedReaches Then
                    Dim upReach As clsSbkReach = Nothing
                    If GetUpstreamInterpolatedReach(ReachesProcessed, upReach) Then
                        upDist += myChainage
                        Return upReach.GetUpstreamObject(ReachesProcessed, upReach.getReachLength, upDist, Structures, Profiles, Laterals, WaterlevelPoints, IncludeInterpolatedReaches)
                    End If
                Else
                    upDist += minDist
                End If

                Return nearestObject

            End If
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    Friend Function GetUpstreamObjectOfType(ByVal myChainage As Double, ByRef upDist As Double, ObjectType As GeneralFunctions.enmNodetype, Optional ByVal IncludeInterpolatedReaches As Boolean = False, Optional ByVal IncludeUpstreamReachesSameDirection As Boolean = False) As clsSbkReachObject
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 26-05-2012
        'Auteur: Siebe Bosch
        'Deze functie zoekt, gegeven een afstand op de tak, het dichtstbijzijnde bovenstrooms gelegen takobject van een gegeven type
        '--------------------------------------------------------------------------------------------------------------
        Dim minDist As Double = 9999999999
        Dim nearestObject As clsSbkReachObject = Nothing
        Dim IncludeObject As Boolean = False
        For Each myObject As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObject.nt = ObjectType Then
                IncludeObject = True
            Else
                IncludeObject = False
            End If

            If IncludeObject Then
                If myObject.lc < myChainage Then
                    If (myChainage - myObject.lc) < minDist Then
                        minDist = (myChainage - myObject.lc)
                        nearestObject = myObject
                    End If
                End If
            End If
        Next myObject

        'first, try interpolated reaches
        Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
        If nearestObject Is Nothing AndAlso IncludeInterpolatedReaches Then
            Dim upReach As clsSbkReach = Nothing
            If GetUpstreamInterpolatedReach(ReachesUpProcessed, upReach) Then
                upDist += myChainage
                Return upReach.GetUpstreamObjectOfType(upReach.getReachLength, upDist, ObjectType, IncludeInterpolatedReaches, False)
            End If
        End If

        'then try downstream reaches that have the same direction
        If nearestObject Is Nothing AndAlso IncludeUpstreamReachesSameDirection Then
            Dim upReach As clsSbkReach = Nothing
            If GetUpstreamReachSameDirection(upReach) Then
                upDist += myChainage
                Return upReach.GetUpstreamObjectOfType(upReach.getReachLength, upDist, ObjectType, False, IncludeUpstreamReachesSameDirection)
            End If
        End If

        upDist += minDist
        Return nearestObject

    End Function

    ''' <summary>
    ''' Zoekt, gegeven de tak, het dichtstbijzijnde RRCF-connection, binnen een opgegeven zoekstraal.
    ''' Als er geen connection wordt gevonden, wordt er een aangemaakt
    ''' </summary>
    ''' <param name="mySnapLocation"></param>
    ''' <param name="minDistFromReachStart"></param>
    ''' <param name="SearchRadiusUp"></param>
    ''' <param name="SearchRadiusDown"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function findAddRRCFConnection(ByVal mySnapLocation As clsSbkReachObject, ByVal IDBase As String, ByVal minDistFromReachStart As Double, Optional ByVal SearchRadiusUp As Double = 100, Optional ByVal SearchRadiusDown As Double = 0)
        Dim myObject As clsSbkReachObject = Nothing
        Dim Nearest As Double = 9999999999999
        Dim myDist As Double

        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection Then
                myDist = Math.Abs(myObj.lc - mySnapLocation.lc)
                If myObj.lc >= mySnapLocation.lc Then
                    If Math.Abs(myObj.lc - mySnapLocation.lc) < SearchRadiusDown Then
                        If Math.Abs(myObj.lc - mySnapLocation.lc) < Nearest Then
                            Nearest = Math.Abs(myObj.lc - mySnapLocation.lc)
                            myObject = myObj
                        End If
                    End If
                ElseIf myObj.lc < mySnapLocation.lc Then
                    If Math.Abs(myObj.lc - mySnapLocation.lc) < SearchRadiusUp Then
                        If Math.Abs(myObj.lc - mySnapLocation.lc) < Nearest Then
                            Nearest = Math.Abs(myObj.lc - mySnapLocation.lc)
                            myObject = myObj
                        End If
                    End If
                End If
            End If
        Next

        If myObject Is Nothing Then
            'object niet gevonden, dus maak er een aan en voeg hem toe aan de tak
            myObject = New clsSbkReachObject(Setup, SbkCase)
            myObject.ci = Id
            myObject.lc = mySnapLocation.lc
            myObject.ID = SbkCase.CFTopo.MakeUniqueNodeID(IDBase)
            myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection
            ReachObjects.ReachObjects.Add(myObject.ID.Trim.ToUpper, myObject)
        End If

        Return myObject

    End Function

    Friend Function AddRRCFConnection(ByVal NodeID As String, ByRef lastUsedDistance As Double, Optional ByVal Increment As Double = 0.1) As clsSbkReachObject
        'alleen toevoegen als het object nog niet bestaat!
        If SbkCase.CFTopo.getReachObject(NodeID.Trim.ToUpper, False) Is Nothing Then
            'object niet gevonden, dus maak er een aan en voeg hem toe aan de tak
            Dim myObject = New clsSbkReachObject(Setup, SbkCase)
            myObject.ci = Id
            myObject.lc = lastUsedDistance + Increment
            lastUsedDistance = myObject.lc
            myObject.ID = NodeID
            myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection
            myObject.InUse = True
            ReachObjects.ReachObjects.Add(myObject.ID.Trim.ToUpper, myObject)
            Return myObject
        Else
            Return SbkCase.CFTopo.getReachObject(NodeID.Trim.ToUpper, False)
        End If
    End Function

    Friend Function AddLateralNode(ByVal NodeID As String, ByRef lastUsedDistance As Double, Optional ByVal Increment As Double = 0.1) As clsSbkReachObject
        'alleen toevoegen als het object nog niet bestaat!
        If SbkCase.CFTopo.getReachObject(NodeID.Trim.ToUpper, False) Is Nothing Then
            'object niet gevonden, dus maak er een aan en voeg hem toe aan de tak
            Dim myObject = New clsSbkReachObject(Setup, SbkCase)
            myObject.ci = Id
            myObject.lc = lastUsedDistance + Increment
            lastUsedDistance = myObject.lc
            myObject.ID = NodeID
            myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
            myObject.InUse = True
            ReachObjects.ReachObjects.Add(myObject.ID.Trim.ToUpper, myObject)
            Return myObject
        Else
            Return SbkCase.CFTopo.getReachObject(NodeID.Trim.ToUpper, False)
        End If
    End Function

    Friend Function GetDownstreamObject(ByRef ReachesProcessed As Dictionary(Of String, clsSbkReach), ByVal myChainage As Double, ByRef dnDist As Double, ByVal Structures As Boolean, ByVal Profiles As Boolean,
                                        ByVal Laterals As Boolean, ByVal WaterlevelPoints As Boolean, Optional ByVal IncludeInterpolatedReaches As Boolean = False) As clsSbkReachObject
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 26-05-2012
        'Auteur: Siebe Bosch
        'Deze functie zoekt, gegeven een afstand op de tak, het dichtstbijzijnde benedenstrooms gelegen takobject
        '--------------------------------------------------------------------------------------------------------------
        Dim minDist As Double = 9999999999
        Dim nearestObject As clsSbkReachObject = Nothing
        Dim IncludeObject As Boolean = False
        For Each myObject As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If Structures = True AndAlso myObject.isStructure AndAlso myObject.InUse Then
                IncludeObject = True
            ElseIf Profiles = True AndAlso myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE AndAlso myObject.InUse Then
                IncludeObject = True
            ElseIf Laterals = True AndAlso myObject.isLateral AndAlso myObject.InUse Then
                IncludeObject = True
            ElseIf WaterlevelPoints = True AndAlso myObject.isWaterLevelObject AndAlso myObject.InUse Then
                IncludeObject = True
            Else
                IncludeObject = False
            End If

            If IncludeObject Then
                If myObject.lc > myChainage Then
                    If myObject.lc - myChainage < minDist Then
                        minDist = myObject.lc - myChainage
                        nearestObject = myObject
                    End If
                End If
            End If
        Next myObject

        'v1.79: implemented a safety valve to avoid eternal loops. This could happen when one reach has the same start- and endpoint
        If nearestObject Is Nothing AndAlso IncludeInterpolatedReaches Then
            Dim dnReach As clsSbkReach = Nothing
            If GetDownstreamInterpolatedReach(ReachesProcessed, dnReach) Then
                dnDist += (getReachLength() - myChainage)
                Return dnReach.GetDownstreamObject(ReachesProcessed, 0, dnDist, Structures, Profiles, Laterals, WaterlevelPoints, IncludeInterpolatedReaches)
            End If
        Else
            dnDist += minDist
        End If

        Return nearestObject

    End Function

    Friend Function GetDownstreamObjectOfType(ByVal myChainage As Double, ByRef dnDist As Double, ByVal ObjectType As GeneralFunctions.enmNodetype, Optional ByVal IncludeInterpolatedReaches As Boolean = False, Optional ByVal IncludeDownstreamReachesSameDirection As Boolean = False) As clsSbkReachObject
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 26-05-2012
        'Auteur: Siebe Bosch
        'Deze functie zoekt, gegeven een afstand op de tak, het dichtstbijzijnde benedenstrooms gelegen takobject
        '--------------------------------------------------------------------------------------------------------------
        Dim minDist As Double = 9999999999
        Dim nearestObject As clsSbkReachObject = Nothing
        Dim IncludeObject As Boolean = False
        For Each myObject As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObject.nt = ObjectType Then
                IncludeObject = True
            Else
                IncludeObject = False
            End If

            If IncludeObject Then
                If myObject.lc > myChainage Then
                    If myObject.lc - myChainage < minDist Then
                        minDist = myObject.lc - myChainage
                        nearestObject = myObject
                    End If
                End If
            End If
        Next myObject

        'first, try interpolated reaches
        Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
        If nearestObject Is Nothing AndAlso IncludeInterpolatedReaches Then
            Dim dnReach As clsSbkReach = Nothing
            If GetDownstreamInterpolatedReach(ReachesDnProcessed, dnReach) Then
                dnDist += (getReachLength() - myChainage)
                Return dnReach.GetDownstreamObjectOfType(0, dnDist, ObjectType, IncludeInterpolatedReaches, False)
            End If
        End If

        'then try downstream reaches that have the same direction
        If nearestObject Is Nothing AndAlso IncludeDownstreamReachesSameDirection Then
            Dim dnReach As clsSbkReach = Nothing
            If GetDownstreamReachSameDirection(dnReach) Then
                dnDist += (getReachLength() - myChainage)
                Return dnReach.GetDownstreamObjectOfType(0, dnDist, ObjectType, False, IncludeDownstreamReachesSameDirection)
            End If
        End If

        dnDist += minDist
        Return nearestObject

    End Function

    Friend Function GetNearestObject(ByVal myDist As Double, ByVal Structures As Boolean, ByVal Profiles As Boolean,
                                        ByVal Laterals As Boolean) As clsSbkReachObject
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 26-05-2012
        'Auteur: Siebe Bosch
        'Deze functie zoekt, gegeven een afstand op de tak, het dichtstbijzijnde takobject
        '--------------------------------------------------------------------------------------------------------------
        Dim minDist As Double = 9999999999
        Dim nearestObject As clsSbkReachObject = Nothing
        Dim IncludeObject As Boolean = False
        For Each myObject As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If Structures = True AndAlso (myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge _
                                   OrElse myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFCulvert _
                                   OrElse myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice _
                                   OrElse myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump _
                                   OrElse myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir _
                                   OrElse myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir) Then
                IncludeObject = True
            ElseIf Profiles = True AndAlso (myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE) Then
                IncludeObject = True
            ElseIf Laterals = True AndAlso (myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral _
                                     OrElse myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection) Then
                IncludeObject = True
            Else
                IncludeObject = False
            End If

            If IncludeObject Then
                If Math.Abs(myObject.lc - myDist) < minDist Then
                    minDist = Math.Abs(myObject.lc - myDist)
                    nearestObject = myObject
                End If
            End If
        Next myObject

        Return nearestObject

    End Function


    Friend Function GetNearestObjectOfType(ByVal Chainage As Double, myType As STOCHLIB.GeneralFunctions.enmNodetype, ByRef Distance As Double) As clsSbkReachObject
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 26-03-2019
        'Auteur: Siebe Bosch
        'Deze functie zoekt, gegeven een afstand op de tak, het dichtstbijzijnde takobject van een gegeven type
        'v2.114: added the InUse clause
        '--------------------------------------------------------------------------------------------------------------
        Dim minDist As Double = 9999999999
        Dim nearestObject As clsSbkReachObject = Nothing
        For Each myObject As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObject.InUse AndAlso myObject.nt = myType Then
                If Math.Abs(myObject.lc - Chainage) < minDist Then
                    minDist = Math.Abs(myObject.lc - Chainage)
                    nearestObject = myObject
                End If
            End If
        Next
        Distance = minDist
        Return nearestObject
    End Function

    ''' <summary>
    ''' Deze routine voegt een dwarsprofiel toe aan de onderhavige sobektak als die er geen heeft.
    ''' hij kiest uit de aanpalende takken de langste die er is (is een arbitraire keuze)
    ''' en pakt vervolgens het dichtstbijzijnde profiel
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub AddCrossSectionFromNeighbors()
        'deze routine voegt een dwarsprofiel toe aan de tak als die er geen heeft.
        'hij kiest uit de aanpalende takken de langste die er is (is een arbitraire keuze)
        'en pakt vervolgens het dichtstbijzijnde profiel
        Try
            Dim copyFromReach As clsSbkReach = Nothing
            Dim myProfile As clsSbkReachObject = Nothing
            Dim myProfDat As New clsProfileDatRecord(Me.Setup)
            Dim myProfDef As New clsProfileDefRecord(Me.Setup, SbkCase)
            Dim maxLen As Double = 0

            'doorloop alle takken en geef de langste terug
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                Dim lastCP As clsSbkVectorPoint = myReach.NetworkcpRecord.CPTable.CP(myReach.NetworkcpRecord.CPTable.CP.Count - 1)
                Dim ProfileID As String = ""
                If myReach.en.ID = bn.ID Then
                    If lastCP.Dist > maxLen AndAlso myReach.ContainsProfile(ProfileID) Then
                        maxLen = lastCP.Dist
                        copyFromReach = myReach
                        myProfile = copyFromReach.GetLastProfile(myReach)
                    End If
                ElseIf myReach.en.ID = en.ID Then
                    If lastCP.Dist > maxLen AndAlso myReach.ContainsProfile(ProfileID) Then
                        maxLen = lastCP.Dist
                        copyFromReach = myReach
                        myProfile = copyFromReach.GetLastProfile(Me)
                    End If
                ElseIf myReach.bn.ID = en.ID Then
                    If lastCP.Dist > maxLen AndAlso myReach.ContainsProfile(ProfileID) Then
                        maxLen = lastCP.Dist
                        copyFromReach = myReach
                        myProfile = copyFromReach.GetFirstProfile(Me)
                    End If
                ElseIf myReach.bn.ID = bn.ID Then
                    If lastCP.Dist > maxLen AndAlso myReach.ContainsProfile(ProfileID) Then
                        maxLen = lastCP.Dist
                        copyFromReach = myReach
                        myProfile = copyFromReach.GetFirstProfile(Me)
                    End If
                End If
            Next

            If copyFromReach IsNot Nothing Then

                'haal nu de profielinformatie op
                If Not myProfile.ID = "" Then
                    myProfDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(myProfile.ID.Trim.ToUpper)
                    myProfDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(myProfDat.di.Trim.ToUpper)
                    Dim lastCP As clsSbkVectorPoint = NetworkcpRecord.CPTable.CP(NetworkcpRecord.CPTable.CP.Count - 1)

                    'maak nu een nieuw dwarsprofiel (reachobject) aan; kopie van het gevonden exemplaar
                    Dim newProfile As clsSbkReachObject = New clsSbkReachObject(Me.Setup, SbkCase)
                    newProfile.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE
                    newProfile.ID = ReachObjects.CreateID("Copy_")
                    newProfile.ci = Id
                    newProfile.lc = lastCP.Dist / 2
                    newProfile.ProfileType = myProfile.ProfileType
                    newProfile.Name = myProfile.Name
                    ReachObjects.ReachObjects.Add(newProfile.ID.Trim.ToUpper, newProfile)

                    'maak ook een nieuw profile.dat-record aan en verwijs naar de bestaande profile.def
                    Dim newProfileDat As New clsProfileDatRecord(Me.Setup)
                    newProfileDat.InUse = True
                    newProfileDat.ID = newProfile.ID
                    newProfileDat.di = myProfDef.ID
                    newProfileDat.rl = myProfDat.rl
                    newProfileDat.rs = myProfDat.rs
                    SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Add(newProfileDat.ID.Trim.ToUpper, newProfileDat)
                End If
            End If

        Catch
            Me.Setup.Log.AddError("Error in function AddCrossSectionIfNotPresent of class clsSbkReach.")
            ' TODO: Geen throw exception?
        End Try
    End Sub
    Friend Function GetLastProfile(ByRef StartReach As clsSbkReach, Optional ByVal SearchUpstreamInterpolatedReaches As Boolean = True, Optional ByVal SearchDownstreamInterpolatedReaches As Boolean = True) As clsSbkReachObject
        Dim Profile As clsSbkReachObject = Nothing
        Dim downReach As clsSbkReach = Nothing
        Dim upReach As clsSbkReach = Nothing
        Dim mydist As Double = 0

        'find the last profile on the current reach
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.InUse AndAlso myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE AndAlso myObj.lc > mydist Then
                mydist = myObj.lc
                Profile = myObj
            End If
        Next myObj

        'if the profile is missing, look for a downstream reach that interpolates with the current one
        'since we're looking for the LAST profile on the reach, it makes sense to look downstream first
        Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
        If Profile Is Nothing Then
            If SearchDownstreamInterpolatedReaches Then
                If GetDownstreamInterpolatedReach(ReachesDnProcessed, downReach) Then
                    'v1.840: added clause if not downReach is nothing
                    If Not downReach Is Nothing Then
                        If downReach.Id = StartReach.Id Then
                            Profile = Nothing                          'we stumbled upon a circular reach so stop here
                        ElseIf downReach.bn.ID = en.ID AndAlso downReach.InUse Then
                            Profile = downReach.GetFirstProfile(StartReach, False, True) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        ElseIf downReach.en.ID = en.ID AndAlso downReach.InUse Then
                            Profile = downReach.GetLastProfile(StartReach, True, False) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        End If
                    End If
                End If
            End If
        End If

        'if the profile is still missing, look for an upstream reach that interpolates with the current one
        Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
        If Profile Is Nothing Then
            If SearchUpstreamInterpolatedReaches Then
                If GetUpstreamInterpolatedReach(ReachesUpProcessed, upReach) Then
                    'v1.840: added clause if not upReach is nothing
                    If Not upReach Is Nothing Then
                        If upReach.Id = StartReach.Id Then
                            Profile = Nothing                             'we stumbled upon a circular reach so stop here
                        ElseIf upReach.en.ID = bn.ID AndAlso upReach.InUse Then
                            Profile = upReach.GetLastProfile(StartReach, True, False) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        ElseIf upReach.bn.ID = bn.ID AndAlso upReach.InUse Then
                            Profile = upReach.GetFirstProfile(StartReach, False, True) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        End If
                    End If
                End If
            End If
        End If

        Return Profile
    End Function

    Friend Function GetFirstCulvert() As clsSbkReachObject
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = enmNodetype.NodeCFCulvert Then
                Return myObj
            End If
        Next
        Return Nothing
    End Function

    Friend Function GetFirstProfile(ByRef StartReach As clsSbkReach, Optional ByVal SearchUpstreamInterpolatedReaches As Boolean = True, Optional ByVal SearchDownstreamInterpolatedReaches As Boolean = True) As clsSbkReachObject
        Dim Profile As clsSbkReachObject = Nothing
        Dim upReach As clsSbkReach = Nothing
        Dim downReach As clsSbkReach = Nothing
        Dim mydist As Double = 99999999999999

        'find the first profile on the current reach
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.InUse AndAlso myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE And myObj.lc < mydist Then
                mydist = myObj.lc
                Profile = myObj
            End If
        Next

        'if the profile is missing, look for an upstream reach that interpolates with the current one
        'since we're looking for the FIRST profile on thre reach, it makes sense to look upstream first
        Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
        If Profile Is Nothing Then
            If SearchUpstreamInterpolatedReaches Then
                If GetUpstreamInterpolatedReach(ReachesUpProcessed, upReach) Then
                    'v1.840: added clause if not upReach is nothing
                    If Not upReach Is Nothing Then
                        If upReach.Id = StartReach.Id Then
                            Profile = Nothing                             'we stumbled upon a circular reach so stop here
                        ElseIf upReach.en.ID = bn.ID AndAlso upReach.InUse Then
                            Profile = upReach.GetLastProfile(StartReach, True, False)
                        ElseIf upReach.bn.ID = bn.ID AndAlso upReach.InUse Then
                            Profile = upReach.GetFirstProfile(StartReach, False, True)
                        End If
                    End If
                End If
            End If
        End If

        'if the profile is still missing, look for a downstream reach that interpolates with the current one
        Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
        If Profile Is Nothing Then
            If SearchDownstreamInterpolatedReaches Then
                If GetDownstreamInterpolatedReach(ReachesDnProcessed, downReach) Then
                    'v1.840: added clause if not downReach is nothing
                    If Not downReach Is Nothing Then
                        If downReach.Id = StartReach.Id Then
                            Profile = Nothing                             'we stumbled upon a circular reach so stop here
                        ElseIf downReach.bn.ID = en.ID AndAlso downReach.InUse Then
                            Profile = downReach.GetFirstProfile(StartReach, False, True)
                        ElseIf downReach.en.ID = en.ID AndAlso downReach.InUse Then
                            Profile = downReach.GetLastProfile(StartReach, True, False)
                        End If
                    End If
                End If
            End If
        End If

        Return Profile
    End Function

    Friend Function GetLastObjectOnReach(NodeType As GeneralFunctions.enmNodetype, ByRef ReachesChecked As Dictionary(Of String, clsSbkReach), Optional ByVal AllowInterpolationUpstream As Boolean = True, Optional ByVal AllowInterpolationDownstream As Boolean = True) As clsSbkReachObject
        'v2.113: since this is a recursive function we must keep track of the reaches that have already been processed in order to prevent an endless loop
        Dim myObject As clsSbkReachObject = Nothing
        Dim downReach As clsSbkReach = Nothing
        Dim upReach As clsSbkReach = Nothing
        Dim mydist As Double = 0

        If ReachesChecked.ContainsKey(Id.Trim.ToUpper) Then
            'this reach has already been checked earlier. So this means we would end up in an endless loop if we rechecked this one
            Return Nothing
        Else
            ReachesChecked.Add(Id.Trim.ToUpper, Me)
            'find the last object on the current reach
            For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
                If myObj.nt = NodeType And myObj.lc > mydist Then
                    mydist = myObj.lc
                    myObject = myObj
                End If
            Next myObj

            'if the object is missing, look for a downstream reach that interpolates with the current one
            'since we're looking for the LAST object on the reach, it makes sense to look downstream first
            Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
            If myObject Is Nothing Then
                If AllowInterpolationDownstream Then
                    If GetDownstreamInterpolatedReach(ReachesDnProcessed, downReach) Then
                        If downReach.Id = Id Then
                            myObject = Nothing                                             'we stumbled upon a circular reach
                        ElseIf downReach.bn.ID = en.ID Then
                            myObject = downReach.GetFirstObjectOnReach(NodeType, ReachesChecked, False, True) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        ElseIf downReach.en.ID = en.ID Then
                            myObject = downReach.GetLastObjectOnReach(NodeType, ReachesChecked, True, False) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        End If
                    End If
                End If
            End If

            'if the object is still missing, look for an upstream reach that interpolates with the current one
            Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
            If myObject Is Nothing Then
                If AllowInterpolationUpstream Then
                    If GetUpstreamInterpolatedReach(ReachesUpProcessed, upReach) Then
                        If upReach.Id = Id Then
                            myObject = Nothing                                             'we stumbled upon a circular reach
                        ElseIf upReach.en.ID = bn.ID Then
                            myObject = upReach.GetLastObjectOnReach(NodeType, ReachesChecked, True, False) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        ElseIf upReach.bn.ID = bn.ID Then
                            myObject = upReach.GetFirstObjectOnReach(NodeType, ReachesChecked, False, True) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        End If
                    End If
                End If
            End If

            Return myObject
        End If



    End Function

    Friend Function GetFirstObjectOnReach(NodeType As GeneralFunctions.enmNodetype, ByRef ReachesChecked As Dictionary(Of String, clsSbkReach), Optional ByVal AllowInterpolationUpstream As Boolean = True, Optional ByVal AllowInterpolationDownstream As Boolean = True) As clsSbkReachObject
        'v2.113: since this is a recursive function we must keep track of the reaches that have already been processed in order to prevent an endless loop
        Dim myObject As clsSbkReachObject = Nothing
        Dim upReach As clsSbkReach = Nothing
        Dim downReach As clsSbkReach = Nothing
        Dim mydist As Double = 99999999999999

        If ReachesChecked.ContainsKey(Id.Trim.ToUpper) Then
            'this reach has already been checked earlier. So this means we would end up in an endless loop if we rechecked this one
            Return Nothing
        Else
            'add the current reach to the list of checked reaches
            ReachesChecked.Add(Id.Trim.ToUpper, Me)

            'find the first object on the current reach
            For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
                If myObj.nt = NodeType And myObj.lc < mydist Then
                    mydist = myObj.lc
                    myObject = myObj
                End If
            Next

            'if the object is missing, look for an upstream reach that interpolates with the current one
            'since we're looking for the FIRST object on thre reach, it makes sense to look upstream first
            Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
            If myObject Is Nothing Then
                If AllowInterpolationUpstream Then
                    If GetUpstreamInterpolatedReach(ReachesUpProcessed, upReach) Then
                        If upReach.Id = Id Then
                            myObject = Nothing                                             'we stumbled upon a circular reach
                        ElseIf upReach.en.ID = bn.ID Then
                            myObject = upReach.GetLastObjectOnReach(NodeType, ReachesChecked, True, False) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        ElseIf upReach.bn.ID = bn.ID Then
                            myObject = upReach.GetFirstObjectOnReach(NodeType, ReachesChecked, False, True) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        End If
                    End If
                End If
            End If

            'if the object is still missing, look for a downstream reach that interpolates with the current one
            Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
            If myObject Is Nothing Then
                If AllowInterpolationDownstream Then
                    If GetDownstreamInterpolatedReach(ReachesDnProcessed, downReach) Then
                        If downReach.Id = Id Then
                            myObject = Nothing                                             'we stumbled upon a circular reach
                        ElseIf downReach.bn.ID = en.ID Then
                            myObject = downReach.GetFirstObjectOnReach(NodeType, ReachesChecked, False, True) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        ElseIf downReach.en.ID = en.ID Then
                            myObject = downReach.GetLastObjectOnReach(NodeType, ReachesChecked, True, False) 'to avoid an infinity loop, make sure the current reach will not be searched again!
                        End If
                    End If

                End If
            End If
            Return myObject

        End If


    End Function



    Friend Function ContainsLateral() As Boolean
        'deze routine onderzoekt of de onderhavige tak een lateral heeft
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral Then
                Return True
            End If
        Next myObj
        Return False
    End Function

    Friend Function ContainsRRCFConnection() As Boolean
        'deze routine onderzoekt of de onderhavige tak een lateral heeft
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection Then
                Return True
            End If
        Next myObj
        Return False
    End Function

    Friend Function GetProfileType(ByRef ID As String, Optional ByVal InterpolatedReaches As Boolean = False) As enmProfileType
        If ContainsProfile(ID, InterpolatedReaches) Then
            Dim ProfDat As clsProfileDatRecord = Nothing, ProfDef As clsProfileDefRecord = Nothing
            If SbkCase.CFData.Data.ProfileData.getProfileRecords(ID, ProfDat, ProfDef) Then
                Return ProfDef.ty
            Else
                'this profiel does not have attribute data associated yet
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function

    Friend Function ContainsProfile(ByRef ID As String, Optional ByVal IncludeInterpolatedReaches As Boolean = False) As Boolean
        'deze routine onderzoekt of de onderhavige tak een dwarsprofiel heeft
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE AndAlso myObj.InUse Then
                ID = myObj.ID
                Return True
            End If
        Next myObj

        'siebe added this routine on 12-4-2017
        'it lists all connected and interpolated reaches w.r.t. the current reach
        If IncludeInterpolatedReaches Then
            Dim upReaches As New Dictionary(Of String, clsSbkReach)
            Dim dnReaches As New Dictionary(Of String, clsSbkReach)
            upReaches = SbkCase.CFTopo.Reaches.GetUpstreamInterpolatedReachChain(Me.Id)
            dnReaches = SbkCase.CFTopo.Reaches.GetDownstreamInterpolatedReachChain(Me.Id)
            For i = 0 To upReaches.Count - 1
                If upReaches.Values(i).ContainsProfile(ID, False) Then Return True 'note: this function calls itself, so the argument must be FALSE to prevent a loop
            Next
            For i = 0 To dnReaches.Count - 1
                If dnReaches.Values(i).ContainsProfile(ID, False) Then Return True 'note: this function calls itself, so the argument must be FALSE to prevent a loop
            Next
        End If

        Return False
    End Function


    Friend Function ContainsObject(ByVal NodeType As GeneralFunctions.enmNodetype, ByRef foundObj As clsSbkReachObject) As Boolean

        'deze routine onderzoekt of de onderhavige tak een dwarsprofiel heeft
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = NodeType AndAlso myObj.InUse Then
                foundObj = myObj
                Return True
            End If
        Next myObj

        Return False
    End Function

    Friend Function ContainsStructure() As Boolean
        'deze routine onderzoekt of de onderhavige tak een dwarsprofiel heeft
        Dim Found As Boolean = False
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If Setup.GeneralFunctions.isStructure(myObj.nt) Then
                Found = True
                Exit For
            End If
        Next myObj

        Return Found
    End Function


    Friend Function ContainsPumpingStation() As Boolean
        Dim found As Boolean = False
        For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump Then
                found = True
                Exit For
            End If
        Next myObj

        Return found
    End Function

    Friend Function GetLocalWidth(ByVal myChainage As Double) As Double
        'deze functie interpoleert dwarsprofielinformatie en geeft de maximale breedte bij de gegeven afstand terug
        Dim beforeProf As clsSbkReachObject = Nothing
        Dim afterProf As clsSbkReachObject = Nothing
        Dim minDistBefore As Double = 9999999999999
        Dim minDistAfter As Double = 9999999999999
        Dim beforeWidth As Double = 0, afterWidth As Double = 0
        Dim upDist As Double, dnDist As Double

        If myChainage < 0 OrElse myChainage > getReachLength() Then
            Return 0
        Else
            beforeProf = GetUpstreamObjectOfType(myChainage, upDist, GeneralFunctions.enmNodetype.SBK_PROFILE, True, False)
            afterProf = GetDownstreamObjectOfType(myChainage, dnDist, GeneralFunctions.enmNodetype.SBK_PROFILE, True, False)

            'beforeProf = GetUpstreamObject(myChainage, upDist, False, True, False, True)
            'afterProf = GetDownstreamObject(myChainage, dnDist, False, True, False, True)

            ''doorloop alle takobjecten
            'For Each myObj As clsSbkReachObject In ReachObjects.ReachObjects.Values
            '    If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE Then
            '        If myObj.lc > myDist AndAlso Math.Abs(myObj.lc - myDist) <= minDistAfter Then
            '            minDistAfter = Math.Abs(myObj.lc - myDist)
            '            afterProf = myObj
            '        ElseIf myObj.lc < myDist AndAlso Math.Abs(myDist - myObj.lc) < minDistBefore Then
            '            minDistBefore = Math.Abs(myDist - myObj.lc)
            '            beforeProf = myObj
            '        End If
            '    End If
            'Next

            'If beforeProf Is Nothing Then
            '    beforeProf = GetUpstreamObject(myDist, False, True, False, True)
            'End If

            If beforeProf Is Nothing AndAlso afterProf Is Nothing Then
                Return 0
            ElseIf beforeProf Is Nothing Then
                Return SbkCase.CFData.Data.ProfileData.getMaxProfileWidth(afterProf.ID)
            ElseIf afterProf Is Nothing Then
                Return SbkCase.CFData.Data.ProfileData.getMaxProfileWidth(beforeProf.ID)
            Else
                beforeWidth = SbkCase.CFData.Data.ProfileData.getMaxProfileWidth(beforeProf.ID)
                afterWidth = SbkCase.CFData.Data.ProfileData.getMaxProfileWidth(afterProf.ID)
                Return Setup.GeneralFunctions.Interpolate(beforeProf.lc, beforeWidth, afterProf.lc, afterWidth, myChainage)
            End If

        End If

    End Function

    Friend Function IsDummy(ByRef myModel As ClsSobekCase) As Boolean

        If myModel.Prefixes.GFE <> "" Then
            If myModel.Prefixes.GFE <> "" AndAlso InStr(Id, myModel.Prefixes.GFE, CompareMethod.Text) > 0 Then
                Return True
            ElseIf myModel.Prefixes.GFE <> "" AndAlso InStr(bn.ID, myModel.Prefixes.GFE, CompareMethod.Text) > 0 _
                      AndAlso InStr(en.ID, myModel.Prefixes.GFE, CompareMethod.Text) > 0 Then
                Return True
            Else
                Return False
            End If
        End If

        If myModel.Prefixes.GPG <> "" Then
            If InStr(Id, myModel.Prefixes.GPG, CompareMethod.Text) > 0 Then
                Return True
            ElseIf myModel.Prefixes.GPG <> "" AndAlso InStr(bn.ID, myModel.Prefixes.GPG, CompareMethod.Text) > 0 _
                      AndAlso InStr(en.ID, myModel.Prefixes.GPG, CompareMethod.Text) > 0 Then
                Return True
            Else
                Return False
            End If
        End If

        Return False

    End Function

    Friend Sub SetReachNodes(ByRef myTopo As clsCFTopology)
        Dim bnStr As String, enStr As String
        Dim errMsg As String = String.Empty

        Try
            bnStr = NetworkTPBrchRecord.bn.Trim.ToUpper
            enStr = NetworkTPBrchRecord.en.Trim.ToUpper

            If myTopo.ReachNodes.ReachNodes.ContainsKey(bnStr.Trim.ToUpper) Then
                bn = myTopo.ReachNodes.ReachNodes(bnStr)
            Else
                errMsg = "Error in sub setReachNodes of class clsSbkReach. Begin node for reach " & Id & " not found."
            End If

            If myTopo.ReachNodes.ReachNodes.ContainsKey(enStr.Trim.ToUpper) Then
                en = myTopo.ReachNodes.ReachNodes(enStr)
            Else
                errMsg = "Error in sub setReachNodes of class clsSbkReach. Begin node for reach " & Id & " not found."
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError(errMsg)
            Throw New Exception(errMsg)
        End Try
    End Sub

    Friend Function InsidePolygon(ByRef myPoly As MapWinGIS.Shape, ByVal IntersectModus As enmIntersectModus) As Boolean
        'deze functie bepaalt of een sobektak in zijn geheel binnen een polygoon ligt
        'bepaal of het beginpunt binnen de polygoon ligt

        Dim myPoint As New MapWinGIS.Point
        Dim utils As New MapWinGIS.Utils

        Try
            If Not myPoly Is Nothing AndAlso myPoly.IsValid Then

                Dim bnInPoly As Boolean, enInPoly As Boolean

                'bepaal of het beginpunt binnen de polygoon ligt
                myPoint = New MapWinGIS.Point
                myPoint.x = bn.X
                myPoint.y = bn.Y
                'Me.setup.Log.AddDebugMessage("Voor utils.PointInPolygon()")
                bnInPoly = utils.PointInPolygon(myPoly, myPoint)
                'Me.setup.Log.AddDebugMessage("Na utils.PointInPolygon()")

                'bepaal of het eindpunt binnen de polygoon ligt
                myPoint = New MapWinGIS.Point
                myPoint.x = en.X
                myPoint.y = en.Y
                'Me.setup.Log.AddDebugMessage("Voor utils.PointInPolygon()")
                enInPoly = utils.PointInPolygon(myPoly, myPoint)
                'Me.setup.Log.AddDebugMessage("Na utils.PointInPolygon()")

                If IntersectModus = enmIntersectModus.StartAndEndNodes Then
                    If bnInPoly AndAlso enInPoly Then
                        Return True
                    Else
                        Return False
                    End If
                End If

                If IntersectModus = enmIntersectModus.OneSide Then
                    If bnInPoly OrElse enInPoly Then
                        Return True
                    Else
                        Return False
                    End If
                End If

                If IntersectModus = enmIntersectModus.AllNodes Then
                    Throw New NotImplementedException(enmIntersectModus.AllNodes.ToString() + " is not implemented")
                End If
            End If

        Catch ex As Exception
            Dim log As String = "Error in InsidePolygon"
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        Finally
            ' Opschonen:
            Me.Setup.GeneralFunctions.ReleaseComObject(myPoint, False)
            Me.Setup.GeneralFunctions.ReleaseComObject(utils, True)
        End Try
    End Function

    Friend Function IsDeadEnd(ByRef myReaches As clsSbkReaches, ByVal IgnoreDisabledReaches As Boolean, ByVal KeepBoundaries As Boolean) As Boolean
        'checkt of een tak doodlopend is
        'de optie SkipDisabledReaches is bedoeld om rekening te houden met takken die in een eerdere loop al op inactief waren gezet
        'als de waarde true is, worden dergelijke takken niet meegenomen als een verbinding

        If KeepBoundaries = True And (bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary Or en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary) Then
            Return False
        ElseIf CountConnectedReaches(myReaches, True) > 1 Then
            'deze tak kunnen we (nog) niet weggooien omdat hij nog op ten minste twee plaatsen aan andere, actieve, takken zit
            Return False
        Else
            Return True
        End If

    End Function
    Friend Function CountConnectedReaches(ByRef myReaches As clsSbkReaches, ByVal IgnoreDisabledReaches As Boolean) As Integer
        'bekijkt of er andere takken verbonden zijn aan een gegeven reach
        Dim nConnected As Integer = 0
        Dim bnDone As Boolean = False 'beginknoop is doorlopen op verbindingen met andere takken
        Dim enDone As Boolean = False 'eindknoop is doorlopen op verbindingen met andere takken

        'zoek eerst uit of aan de beginknoop van onze tak een andere tak gekoppeld is (op normale wijze)
        For Each myReach As clsSbkReach In myReaches.Reaches.Values
            If myReach.Id <> Id Then 'sluit zichzelf als kandidaat uit
                'check of deze tak op enigerlei wijze aan de BEGINknoop van de onderhavige tak gekoppeld is
                If myReach.bn.ID = bn.ID Or myReach.en.ID = bn.ID Then
                    'we hebben een verbinding gevonden. Nu nog checken of de gevonden tak niet al is uitgeschakeld
                    If IgnoreDisabledReaches = True And myReach.InUse = False Then
                        'geen officiele verbinding want deze mogen we overslaan
                    Else
                        nConnected += 1
                        bnDone = True
                        Exit For
                    End If
                End If
            End If
        Next myReach

        'zoek dan uit of aan de eindknoop van onze tak een andere tak gekoppeld is (op normale wijze)
        For Each myReach As clsSbkReach In myReaches.Reaches.Values
            If myReach.Id <> Id Then 'sluit zichzelf als kandidaat uit
                'check of deze tak op enigerlei wijze aan de EINDknoop van de onderhavige tak gekoppeld is
                If myReach.bn.ID = en.ID Or myReach.en.ID = en.ID Then
                    'we hebben een verbinding gevonden. Nu nog checken of de gevonden tak niet al is uitgeschakeld
                    If IgnoreDisabledReaches = True And myReach.InUse = False Then
                        'geen officiele verbinding want deze mogen we overslaan
                    Else
                        nConnected += 1
                        enDone = True
                        Exit For
                    End If
                End If
            End If
        Next myReach

        'als de beginknoop van het type linkage is, zoek dan uit of de tak waar hij aan vast zit actief is
        If Not bnDone Then 'het kan voorkomen dat de beginknoop zowel een linkage is als gekoppeld aan andere takken
            If bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLinkage Then
                Dim myReach As clsSbkReach = myReaches.Reaches(bn.ci)
                If IgnoreDisabledReaches = True And myReach.InUse = False Then
                    'geen officiele verbinding want deze mogen we overslaan
                Else
                    nConnected += 1
                End If
            End If
        End If

        'als de eindknoop van het type linkage is, zoek dan uit of de tak waar hij aan vast zit actief is
        If Not enDone Then 'het kan voorkomen dat de eindknoop zowel een linkage is als gekoppeld aan andere takken
            If en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLinkage Then
                Dim myReach As clsSbkReach = myReaches.Reaches(en.ci)
                If IgnoreDisabledReaches = True And myReach.InUse = False Then
                    'geen officiele verbinding want deze mogen we overslaan
                Else
                    nConnected += 1
                End If
            End If
        End If

        'controleer nu of de tak evt via linkage nodes aan de onderhavige tak zit vastgeklampt
        For Each myReach As clsSbkReach In myReaches.Reaches.Values
            If myReach.bn.ci = Id Or myReach.en.ci = Id Then
                If IgnoreDisabledReaches = True And myReach.InUse = False Then
                    'geen officiele verbinding want deze mogen we overslaan
                Else
                    nConnected += 1
                End If
            End If
        Next myReach

        Return nConnected

    End Function

    Public Function cutoffDeadPartAtLinkage(ByVal KeepBoundaries As Boolean, ByVal KeepLatConn As Boolean, ByVal KeepRRCFConnNodes As Boolean, ByVal KeepLaterals As Boolean, ByVal KeepRRCFConnections As Boolean) As Boolean
        Dim query As IEnumerable(Of clsSbkReachObject)
        Dim myObj As clsSbkReachObject
        Dim i As Long, TruncateIdx As Long = -1
        Dim myNode As clsSbkReachNode

        Try

            'firstly, if the dead-end side is an rr-on-flow-connection node and those nodes need to be kept, leave immediatelly
            'do the same if the dead-end siide is a connection node with lateral discharge and storage
            'and do the same if the dead-end side is a boundary and the boundaries must be kept
            If Not HasUpstreamReach(True) Then
                If bn.nt = enmNodetype.NodeRRCFConnectionNode AndAlso KeepRRCFConnNodes Then Return True
                If bn.nt = enmNodetype.ConnNodeLatStor AndAlso KeepLatConn Then Return True
                If bn.isBoundary AndAlso KeepBoundaries Then Return True
            End If

            If Not HasDownstreamReach(True) Then
                If en.nt = enmNodetype.NodeRRCFConnectionNode AndAlso KeepRRCFConnNodes Then Return True
                If en.nt = enmNodetype.ConnNodeLatStor AndAlso KeepLatConn Then Return True
                If en.isBoundary AndAlso KeepBoundaries Then Return True
            End If

            'first we'll make a dictionary of all reach objects
            Dim objList As New List(Of clsSbkReachObject)
            For Each myObj In ReachObjects.ReachObjects.Values
                objList.Add(myObj)
            Next

            'start cutting on the upstream side, if allowed
            If Not HasUpstreamReach(True) Then
                'this reach has no upstream reach, so locate the first linkage node upstream from any other influx
                'first sort the list of objects by ascending chainage
                query = objList.OrderBy(Function(clsSbkReachObject) clsSbkReachObject.lc)

                For i = 0 To query.Count - 1
                    If query(i).isLateral Then Exit For 'no truncating beyond this point since we've found an inflow point
                    If query(i).isLinkage Then
                        TruncateIdx = i
                        Exit For
                    End If
                Next

                'start truncating at the specified point by cutting off the upstream part
                If TruncateIdx >= 0 Then
                    myNode = Me.SbkCase.CFTopo.ReachNodes.ReachNodes.Item(query(TruncateIdx).ID.Trim.ToUpper)
                    Call Me.SbkCase.CFTopo.SplitReachAtLinkageNode(Me, myNode, False, True)
                    Return True
                End If

            ElseIf Not HasDownstreamReach(True) Then
                'this reach has no downstream reach, so locate the first linkage node after any influx
                'first sort the list of objects by descending chainage
                query = objList.OrderByDescending(Function(clsSbkReachObject) clsSbkReachObject.lc)

                For i = 0 To query.Count - 1
                    If query(i).isLateral Then Exit For 'no truncating upstream from this point since we've found an inflow point
                    If query(i).isLinkage Then
                        TruncateIdx = i
                        Exit For
                    End If
                Next

                'start truncating at the specified point by cutting off the downstream part
                If TruncateIdx >= 0 Then
                    myNode = Me.SbkCase.CFTopo.ReachNodes.ReachNodes.Item(query(TruncateIdx).ID.Trim.ToUpper)
                    Call Me.SbkCase.CFTopo.SplitReachAtLinkageNode(Me, myNode, True, False)
                    Return True
                End If

            End If

            Return True

        Catch ex As Exception
            Return False
        End Try




    End Function

    Public Function cutoffDeadPartAtRRonFlowConnections(ByVal KeepBoundaries As Boolean, ByVal KeepLatConn As Boolean, ByVal KeepRRCFConnNodes As Boolean, ByVal KeepLaterals As Boolean) As Boolean
        Dim query As IEnumerable(Of clsSbkReachObject)
        Dim myObj As clsSbkReachObject
        Dim i As Long, TruncateIdx As Long = -1
        Dim myNode As clsSbkReachObject

        Try

            If Not HasUpstreamReach(True) Then
                If bn.nt = enmNodetype.NodeRRCFConnectionNode AndAlso KeepRRCFConnNodes Then Return True
                If bn.nt = enmNodetype.ConnNodeLatStor AndAlso KeepLatConn Then Return True
                If bn.isBoundary AndAlso KeepBoundaries Then Return True
            End If

            If Not HasDownstreamReach(True) Then
                If en.nt = enmNodetype.NodeRRCFConnectionNode AndAlso KeepRRCFConnNodes Then Return True
                If en.nt = enmNodetype.ConnNodeLatStor AndAlso KeepLatConn Then Return True
                If en.isBoundary AndAlso KeepBoundaries Then Return True
            End If

            'first we'll make a dictionary of all reach objects
            Dim objList As New List(Of clsSbkReachObject)
            For Each myObj In ReachObjects.ReachObjects.Values
                objList.Add(myObj)
            Next

            'start cutting on the upstream side, if allowed
            If Not HasUpstreamReach(True) Then
                'this reach has no upstream reach, so locate the first rr on flow connection upstream from any other influx
                'first sort the list of objects by ascending chainage
                query = objList.OrderBy(Function(clsSbkReachObject) clsSbkReachObject.lc)

                For i = 0 To query.Count - 1
                    If query(i).nt = enmNodetype.NodeRRCFConnection Then
                        'truncate here!
                        TruncateIdx = i
                        Exit For
                    ElseIf query(i).isLateral Then
                        Exit For 'no truncation allowed since a lateral is located on the upstream side, even before the first rr-on-flow-connection
                    End If
                Next

                'start truncating at the specified point by cutting off the upstream part
                If TruncateIdx >= 0 Then
                    myNode = Me.ReachObjects.ReachObjects.Item(query(TruncateIdx).ID.Trim.ToUpper)
                    Call Me.SbkCase.CFTopo.SplitReachAtReachObject(myNode, False, True)
                    Return True
                End If


            ElseIf Not HasDownstreamReach(True) Then
                'this reach has no downstream reach, so locate the first linkage node after any influx
                'first sort the list of objects by descending chainage
                query = objList.OrderByDescending(Function(clsSbkReachObject) clsSbkReachObject.lc)

                For i = 0 To query.Count - 1
                    If query(i).nt = enmNodetype.NodeRRCFConnection Then
                        'truncate here!
                        TruncateIdx = i
                        Exit For
                    ElseIf query(i).isLateral Then
                        Exit For
                    End If
                Next

                'start truncating at the specified point by cutting off the downstream part
                If TruncateIdx >= 0 Then
                    myNode = Me.ReachObjects.ReachObjects.Item(query(TruncateIdx).ID.Trim.ToUpper)
                    Call Me.SbkCase.CFTopo.SplitReachAtReachObject(myNode, True, False)
                    Return True
                End If

            End If

            Return True

        Catch ex As Exception
            Return False
        End Try




    End Function

End Class
