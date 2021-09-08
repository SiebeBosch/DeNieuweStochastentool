Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public Class clsCFTopology
    Friend Active As Boolean

    Friend Reaches As clsSbkReaches           'alle takken
    Friend ReachNodes As clsSbkReachNodes     'alle knopen die het begin of eind van een tak vormen, incl Linkage Nodes
    Public NetworkOBIOBIDrecords As clsNetworkOBIOBIDRecords  'alle objecttypen in het flow-netwerk
    Public NetworkOBIBRIDrecords As clsNetworkOBIBRIDRecords  'alle taktypen in het flow netwerk

    Public VectorPoints As New Dictionary(Of String, clsSbkVectorPoint)
    Public WaterLevelPoints As New Dictionary(Of String, clsSbkVectorPoint)

    Friend ReachTiles As clsTiles            'keeps track of all reaches in a tile structure, so it's easy to track a nearby reach for a given point
    Friend ReachNodeTiles As clsTiles        'keeps track of all reachnodes in a tile structure, so it's easy to track a nearby reachnode for a given point
    Friend xMin As Double, yMin As Double, xMax As Double, yMax As Double

    Friend calculationpointsRead As Boolean = False 'keeps track of whether the calculation points in this schematization have already been read
    Friend reachsegmentsread As Boolean = False    'keeps track of whether the reachsegments in this schematization ahve already been read

    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    'de onderstaande structure wordt gebruikt om de dichtstbijzijnde curving points tov het lozingspunt van een gebied te zoeken
    Friend Structure CP
        Friend ReachID As String
        Friend DistToOutflowPoint As Double
        Friend CP As clsSbkVectorPoint
    End Structure

    Public Function GetBoundaryNodes() As Dictionary(Of String, clsSbkReachNode)
        Dim myDict As New Dictionary(Of String, clsSbkReachNode)
        For Each myNode As clsSbkReachNode In ReachNodes.ReachNodes.Values
            If myNode.InUse AndAlso myNode.isBoundary Then
                myDict.Add(myNode.ID.Trim.ToUpper, myNode)
            End If
        Next
        Return myDict
    End Function
    Public Function GetBoundaryNodeIDs() As List(Of String)
        Dim myList As New List(Of String)
        For Each myNode As clsSbkReachNode In ReachNodes.ReachNodes.Values
            If myNode.InUse AndAlso myNode.isBoundary Then
                myList.Add(myNode.ID)
            End If
        Next
        Return myList
    End Function

    Public Function BuildReachesVerdictJSON() As String
        Dim myJSON As String
        Dim nInuse As Integer = Reaches.CountInuse, i As Integer = 0
        myJSON = "{" & vbCrLf
        myJSON &= "%type%: %FeatureCollection%, " & vbCrLf
        myJSON &= "%name% :  %reachesverdict%," & vbCrLf
        'myJSON &= "%crs%: { %type%: %name%, %properties%: { %name%: %urn:ogc : def : crs : EPSG : 28992% } }," & vbCrLf
        myJSON &= "%crs%: { %type%: %name%, %properties%: { %name%: %urn:ogc:def:crs:OGC:1.3:CRS84% } }," & vbCrLf
        myJSON &= "%features% :  [" & vbCrLf

        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If myReach.InUse Then
                i += 1
                myJSON &= myReach.BuildVerdictJSON()
                If i < nInuse Then myJSON &= ","
                myJSON &= vbCrLf
            End If
        Next
        myJSON &= "]" & vbCrLf
        myJSON &= "}" & vbCrLf
        myJSON = myJSON.Replace("%", Chr(34))
        Return myJSON
    End Function
    Public Function calcSnapLocation(X As Double, Y As Double, SearchRadius As Double, ByRef snapReach As clsSbkReach, ByRef myChainage As Double, ByRef myDistance As Double, AllowSnappingToVectorPoints As Boolean) As Boolean
        Try
            Dim minDist As Double = SearchRadius
            Dim Found As Boolean = False
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                Dim curDist As Double
                Dim curChainage As Double
                If myReach.InUse Then
                    If myReach.calcSnapLocation(X, Y, SearchRadius, curChainage, curDist, AllowSnappingToVectorPoints) Then
                        If curDist <= minDist Then
                            minDist = curDist
                            myChainage = curChainage
                            myDistance = curDist
                            snapReach = myReach
                            Found = True
                        End If
                    End If
                End If
            Next
            Return Found
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in Function calcSnapLocation Of MyClass clsCFTopology")
            Me.Setup.Log.AddError(ex.Message)
            Return False

        End Try
    End Function


    Public Function SobekHLocationsToDatabase(TableName As String, LocationColumn As String, ParameterColumn As String, XColumn As String, YColumn As String, HisFileCol As String, HisParCol As String, HisLocCol As String, HisfileParameter As String, Parameter As String) As Boolean
        Dim query As String
        Dim HisFile As String = "CALCPNT.HIS"
        Dim iReach As Integer, nReach As Integer = Reaches.Reaches.Values.Count
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing waterlevel points to database...", 0, 10, True)

            'clear the old locations
            query = "DELETE FROM " & TableName & " WHERE " & ParameterColumn & "='" & Parameter & "';"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.isWaterLevelObject Then
                            myObj.calcXY()
                            query = "INSERT INTO " & TableName & " (" & LocationColumn & "," & ParameterColumn & "," & XColumn & "," & YColumn & "," & HisFileCol & "," & HisParCol & "," & HisLocCol & ") VALUES ('" & myObj.ID & "','" & Parameter & "'," & myObj.X & "," & myObj.Y & ",'" & HisFile & "','" & HisfileParameter.ToString & "','" & myObj.ID & "');"
                            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                        End If
                    Next
                End If
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Me.Setup.SqliteCon.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function SobekStructureLocationsToDatabase(TableName As String, LocationColumn As String, ParameterColumn As String, XColumn As String, YColumn As String, HisFileCol As String, HisParCol As String, HisLocCol As String, HisfileParameter As String, Parameter As String) As Boolean
        Dim query As String
        Dim HisFile As String = "STRUC.HIS"
        Dim iReach As Integer, nReach As Integer = Reaches.Reaches.Values.Count
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing structure locations to database...", 0, 10, True)

            'clear the old locations
            query = "DELETE FROM " & TableName & " WHERE " & ParameterColumn & "='" & Parameter & "';"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.isStructure Then
                            myObj.calcXY()
                            query = "INSERT INTO " & TableName & " (" & LocationColumn & "," & ParameterColumn & "," & XColumn & "," & YColumn & "," & HisFileCol & "," & HisParCol & "," & HisLocCol & ") VALUES ('" & myObj.ID & "','" & Parameter & "'," & myObj.X & "," & myObj.Y & ",'" & HisFile & "','" & HisfileParameter.ToString & "','" & myObj.ID & "');"
                            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                        End If
                    Next
                End If
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Me.Setup.SqliteCon.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function SetReachesExtent() As Boolean
        Try
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                myReach.CalculateExtent()
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function SetReachesExtent() of class clsCFTopology.")
        End Try
    End Function

    Public Function SetPartOfReachesInuse(ReachStartIdx As Integer, ReachEndIdx As Integer) As Boolean
        Dim i As Integer
        For i = 0 To Reaches.Reaches.Values.Count - 1
            If i >= ReachStartIdx AndAlso i <= ReachEndIdx Then
                Reaches.Reaches.Values(i).InUse = True
            Else
                Reaches.Reaches.Values(i).InUse = False
            End If
        Next
    End Function

    Public Function ToggleCrossSectionsInUse() As Boolean
        'this function inverts the InUse boolean for all cross section reach objects.
        Try
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE Then myObj.InUse = Not (myObj.InUse)
                    Next
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub ModelTreeToExcel(SheetName As String, NodeType As GeneralFunctions.enmNodetype)
        'this function exports a model tree from relevant objects. For now we only support weirs
        Dim myObj As clsSbkReachObject, upObj As clsSbkReachObject, dnObj As clsSbkReachObject
        Dim upDist As Double = 9.0E+99, dnDist As Double = 9.0E+99
        Dim r As Integer
        Dim ws As clsExcelSheet

        Me.Setup.GeneralFunctions.UpdateProgressBar("Processing objects of type " & NodeType.ToString, 0, 10)
        ws = Me.Setup.ExcelFile.GetAddSheet(SheetName)

        ws.ws.Cells(r, 0).Value = "Object ID"
        ws.ws.Cells(r, 1).Value = "Upstream Object"
        ws.ws.Cells(r, 2).Value = "Downstream Object"

        For Each myReach In Reaches.Reaches.Values
            If myReach.InUse Then
                For Each myObj In myReach.ReachObjects.ReachObjects.Values
                    If myObj.nt = NodeType Then
                        upDist = 0
                        dnDist = 0
                        r += 1
                        upObj = myReach.GetUpstreamObjectOfType(myObj.lc, upDist, NodeType, True, True)
                        dnObj = myReach.GetDownstreamObjectOfType(myObj.lc, dnDist, NodeType, True, True)

                        ws.ws.Cells(r, 0).Value = myObj.ID
                        If Not upObj Is Nothing Then ws.ws.Cells(r, 1).Value = upObj.ID
                        If Not dnObj Is Nothing Then ws.ws.Cells(r, 2).Value = dnObj.ID
                    End If
                Next
            End If
        Next
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10)
    End Sub

    Friend Function ExportCSV(ExportDir As String) As Boolean
        Try
            Dim Idx As Integer = 0, nReach As Integer = Reaches.Reaches.Count, nNodes As Integer = ReachNodes.ReachNodes.Count
            Using ReachWriter As New StreamWriter(ExportDir & "\reaches.csv")
                Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting reaches...", 0, 10)
                ReachWriter.WriteLine("ReachID;ReachType;FromNode;EndNode")
                For Each myReach As clsSbkReach In Reaches.Reaches.Values
                    Idx += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", Idx, nReach)
                    If myReach.InUse Then
                        ReachWriter.WriteLine(myReach.Id & ";" & myReach.ReachType.ID & ";" & myReach.bn.ID & ";" & myReach.en.ID)
                    End If
                Next
            End Using

            Idx = 0
            Using VectorWriter As New StreamWriter(ExportDir & "\vectorpoints.csv")
                Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting vector points...", 0, 10)
                VectorWriter.WriteLine("ReachID;PointIdx;X;Y")
                For Each myReach As clsSbkReach In Reaches.Reaches.Values
                    Idx += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", Idx, nReach)
                    If myReach.InUse Then
                        myReach.NetworkcpRecord.CalcCPTable(myReach.Id, myReach.bn, myReach.en)
                        For i = 0 To myReach.NetworkcpRecord.CPTable.CP.Count - 1
                            VectorWriter.WriteLine(myReach.Id & ";" & i & ";" & myReach.NetworkcpRecord.CPTable.CP(i).X & ";" & myReach.NetworkcpRecord.CPTable.CP(i).Y)
                        Next
                    End If
                Next
            End Using

            Idx = 0
            Using NodeWriter As New StreamWriter(ExportDir & "\nodes.csv")
                Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting reachnodes...", 0, 10)
                NodeWriter.WriteLine("NodeID;NodeType;X;Y")
                For Each myNode As clsSbkReachNode In ReachNodes.ReachNodes.Values
                    Idx += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", Idx, nNodes)
                    If myNode.InUse Then
                        NodeWriter.WriteLine(myNode.ID & ";SBK_CONNECTIONNODE;" & myNode.X & ";" & myNode.Y)
                    End If
                Next
            End Using

            Using ProfWriter As New StreamWriter(ExportDir & "\profiles.csv")
                Using WeirWriter As New StreamWriter(ExportDir & "\weirs.csv")
                    Using OrificeWriter As New StreamWriter(ExportDir & "\orifices.csv")
                        Using PumpWriter As New StreamWriter(ExportDir & "\pumps.csv")
                            Using CulvertWriter As New StreamWriter(ExportDir & "\culverts.csv")
                                Using MeasWriter As New StreamWriter(ExportDir & "\measurementstations.csv")
                                    Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting reach objects...", 0, 10)
                                    ProfWriter.WriteLine("ObjectID;ObjectName;ReachID;Chainage;X;Y")
                                    WeirWriter.WriteLine("ObjectID;ObjectName;ReachID;Chainage;X;Y")
                                    OrificeWriter.WriteLine("ObjectID;ObjectName;ReachID;Chainage;X;Y")
                                    PumpWriter.WriteLine("ObjectID;ObjectName;ReachID;Chainage;X;Y")
                                    CulvertWriter.WriteLine("ObjectID;ObjectName;ReachID;Chainage;X;Y")
                                    MeasWriter.WriteLine("ObjectID;ObjectName;ReachID;Chainage;X;Y")
                                    Dim myWriter As StreamWriter = Nothing
                                    For Each myReach As clsSbkReach In Reaches.Reaches.Values
                                        If myReach.InUse Then
                                            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                                                If myObj.InUse Then
                                                    myObj.calcXY()
                                                    Select Case myObj.nt
                                                        Case Is = GeneralFunctions.enmNodetype.SBK_PROFILE
                                                            myWriter = ProfWriter
                                                        Case Is = GeneralFunctions.enmNodetype.NodeCFWeir
                                                            myWriter = WeirWriter
                                                        Case Is = GeneralFunctions.enmNodetype.NodeCFOrifice
                                                            myWriter = OrificeWriter
                                                        Case Is = GeneralFunctions.enmNodetype.NodeCFPump
                                                            myWriter = PumpWriter
                                                        Case Is = GeneralFunctions.enmNodetype.NodeCFCulvert
                                                            myWriter = CulvertWriter
                                                        Case Is = GeneralFunctions.enmNodetype.MeasurementStation
                                                            myWriter = MeasWriter
                                                    End Select
                                                    myWriter.WriteLine(myObj.ID & ";" & myObj.Name & ";" & myObj.ci & ";" & myObj.lc & ";" & myObj.X & ";" & myObj.Y)
                                                End If
                                            Next
                                        End If
                                    Next
                                End Using
                            End Using
                        End Using
                    End Using
                End Using
            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Friend Sub ReadReachTiles(TileSize As Double)
        'create a set of tiles that represents the entire model extent
        ReachTiles = New clsTiles(Me.Setup)
        If xMin = 0 OrElse xMax = 0 OrElse yMin = 0 OrElse yMax = 0 Then Call getExtent()
        ReachTiles.Create(xMin - 1, yMin - 1, xMax + 1, yMax + 1, TileSize)
    End Sub

    Public Function CountHPoints() As Integer
        Dim WaterLevelPoints As New Dictionary(Of String, clsSbkVectorPoint)
        WaterLevelPoints = CollectAllWaterLevelPoints()
        Return WaterLevelPoints.Count
    End Function

    Public Function WaterlevelPointsToDatabase(ByRef con As SQLite.SQLiteConnection, TableName As String, IDColumnID As String, XColumnID As String, YColumnID As String) As Boolean
        Try
            Dim myQuery As String
            If Not con.State = ConnectionState.Open Then con.Open()
            WaterLevelPoints = CollectAllWaterLevelPoints()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing locations to database...", 0, 10, True)
            For i = 0 To WaterLevelPoints.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, WaterLevelPoints.Count)
                myQuery = "INSERT INTO " & TableName & " (" & IDColumnID & "," & XColumnID & "," & YColumnID & ") VALUES ('" & WaterLevelPoints.Values(i).ID & "'," & WaterLevelPoints.Values(i).X & "," & WaterLevelPoints.Values(i).Y & ");"
                Me.Setup.GeneralFunctions.SQLiteNoQuery(con, myQuery, False)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Process complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub ExportReachObjectsOfTypeToBNA(ObjectType As GeneralFunctions.enmNodetype, exportDir As String, FileNameBase As String)
        Using objWriter As New System.IO.StreamWriter(exportDir & "\" & FileNameBase & ".bna", False)
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.InUse AndAlso myObj.nt = ObjectType Then
                            myObj.calcXY()
                            objWriter.WriteLine(Setup.GeneralFunctions.BNAString(myObj.ID, myObj.Name, myObj.X, myObj.Y))
                        End If
                    Next
                End If
            Next
        End Using
    End Sub

    Public Function getNearestUpstreamProfileByIndex(ReachIdx As Integer, ProfileIdx As Integer, Optional ByVal AllowInterpolatedReaches As Boolean = True) As clsSbkReachObject
        Dim myReach As clsSbkReach, myObj As clsSbkReachObject, curIdx As Integer = -1
        Dim upDist As Double
        Dim ReachesProcessed As New Dictionary(Of String, clsSbkReach)
        If Reaches.Reaches.Count > ReachIdx Then
            myReach = Reaches.Reaches.Values(ReachIdx)
            For Each myObj In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE Then
                    curIdx += 1
                    If curIdx = ProfileIdx Then
                        'find the nearest upstream cross section and return it.
                        Return myReach.GetUpstreamObject(ReachesProcessed, myObj.lc, upDist, False, True, False, False, AllowInterpolatedReaches)
                    End If
                End If
            Next
        End If
        Return Nothing
    End Function

    Public Function getNearestDownstreamProfileByIndex(ReachIdx As Integer, ProfileIdx As Integer, Optional ByVal AllowInterpolatedReaches As Boolean = True) As clsSbkReachObject
        Dim myReach As clsSbkReach, myObj As clsSbkReachObject, curIdx As Integer = -1
        Dim dnDist As Double
        Dim ReachesProcessed As New Dictionary(Of String, clsSbkReach)
        If Reaches.Reaches.Count > ReachIdx Then
            myReach = Reaches.Reaches.Values(ReachIdx)
            For Each myObj In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE Then
                    curIdx += 1
                    If curIdx = ProfileIdx Then
                        'find the nearest downstream cross section and return it.
                        Return myReach.GetDownstreamObject(ReachesProcessed, myObj.lc, dnDist, False, True, False, False, AllowInterpolatedReaches)
                    End If
                End If
            Next
        End If
        Return Nothing
    End Function

    Public Function getProfileByIndex(ReachIdx As Integer, ProfileIdx As Integer) As clsSbkReachObject
        Dim myReach As clsSbkReach, myObj As clsSbkReachObject, curIdx As Integer = -1
        If Reaches.Reaches.Count > ReachIdx Then
            myReach = Reaches.Reaches.Values(ReachIdx)
            For Each myObj In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE Then
                    curIdx += 1
                    If curIdx = ProfileIdx Then
                        Return myObj
                    End If
                End If
            Next
        End If
        Return Nothing
    End Function


    Friend Function ReadReachNodeTiles(TileSize As Double) As Boolean
        Try
            ReachNodeTiles = New clsTiles(Me.Setup)
            If xMin = 0 OrElse xMax = 0 OrElse yMin = 0 OrElse yMax = 0 Then
                If Not getExtent() Then Throw New Exception("Error creating tiles.")
            End If
            ReachNodeTiles.Create(xMin - 1, yMin - 1, xMax + 1, yMax + 1, TileSize)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ReadReachNodeTiles of class clsCFTopology.")
            Return False
        End Try
    End Function

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'init classes:
        Reaches = New clsSbkReaches(Me.Setup, Me.SbkCase)   'alle takken
        ReachNodes = New clsSbkReachNodes(Me.Setup, Me.SbkCase)         'alle knopen die het begin of eind van een tak vormen, incl Linkage Nodes
        NetworkOBIOBIDrecords = New clsNetworkOBIOBIDRecords(Me.Setup, Me.SbkCase)
        NetworkOBIBRIDrecords = New clsNetworkOBIBRIDRecords(Me.Setup, Me.SbkCase)

    End Sub

    Public Function BuildMeasurementStationRecord(ID As String, reachID As String, chainage As String) As clsSbkReachObject
        Try
            Dim measObj As New clsSbkReachObject(Me.Setup, Me.SbkCase)
            measObj.ID = ID
            measObj.Name = measObj.ID
            measObj.InUse = True
            measObj.nt = GeneralFunctions.enmNodetype.MeasurementStation
            measObj.ci = reachID
            measObj.lc = chainage
            Return measObj
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    Public Function AddReachStartEndNodesAsReachObjects() As Boolean
        Try
            Dim up As clsSbkReachObject
            Dim dn As clsSbkReachObject
            For Each myReach As clsSbkReach In Reaches.Reaches.Values

                'begin node
                up = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                up.ID = myReach.bn.ID
                up.nt = myReach.bn.nt
                up.ci = myReach.Id
                up.lc = 0
                up.InUse = True
                myReach.ReachObjects.ReachObjects.Add(up.ID.Trim.ToUpper, up)

                'end node
                dn = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                dn.ID = myReach.en.ID
                dn.nt = myReach.en.nt
                dn.ci = myReach.Id
                dn.lc = myReach.getReachLength
                dn.InUse = True
                myReach.ReachObjects.ReachObjects.Add(dn.ID.Trim.ToUpper, dn)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function AddReachStartEndNodesAsReachObjects of class clsCFTopology.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function MakeLateralsForReachCenters(ProjectArea As Double, Optional ByVal SkipReachesPrefixList As List(Of String) = Nothing) As Boolean
        Try
            Dim myObj As clsSbkReachObject
            Dim TotalLength As Double = GetTotalReachLength()
            Dim ReachLength As Double
            Dim i As Long = 0
            Dim n As Long = Reaches.Reaches.Values.Count

            Setup.GeneralFunctions.UpdateProgressBar("Building lateral nodes & data...", 0, 10, True)
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                i += 1
                Setup.GeneralFunctions.UpdateProgressBar("", i, n)

                'v1.794: added a functionality to skip structure reaches when adding a lateral for each reach
                Dim SkipReach As Boolean = False
                If Not SkipReachesPrefixList Is Nothing Then
                    For Each Prefix As String In SkipReachesPrefixList
                        If Not Prefix = "" AndAlso Left(myReach.Id, Prefix.Length) = Prefix Then SkipReach = True
                    Next
                End If

                If myReach.InUse AndAlso Not SkipReach Then
                    ReachLength = myReach.getReachLength
                    myObj = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    myObj.ci = myReach.Id
                    myObj.lc = ReachLength / 2
                    myObj.calcXY()
                    myObj.ID = "owlat" & myReach.Id 'v1.880: changed the prefix to distinguish between laterals per subcatchment and laterals for openwater
                    myObj.InUse = True
                    myObj.nt = GeneralFunctions.enmNodetype.NodeCFLateral
                    myReach.ReachObjects.ReachObjects.Add(myObj.ID.Trim.ToUpper, myObj)

                    'make a lateral.dat record
                    Dim latDat As clsLateralDatFLBRRecord
                    latDat = Me.SbkCase.CFData.Data.LateralData.BuildLateralDatFLBRRecord(myObj.ID, myReach.getReachLength / TotalLength * ProjectArea, 0, "Station1", True)

                    'v1.76: we calculate the openwater area and add it to our lateral.
                    latDat.sc = 0
                    latDat.lt = 0
                    latDat.dclt1 = 7
                    latDat.ir = 0
                    latDat.ms = "Station1"
                    latDat.ii = 0
                    latDat.ar = myReach.calcMaxOpenWaterArea 'we use the highest profile width
                    latDat.InUse = True
                    SbkCase.CFData.Data.LateralData.LateralDatFLBRRecords.records.Add(latDat.ID.Trim.ToUpper, latDat)
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetTotalReachLength() As Double
        Dim TotalLength As Double
        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If myReach.InUse Then
                TotalLength += myReach.getReachLength
            End If
        Next
        Return TotalLength
    End Function

    Public Function AssignNearestCrossSectionForReachesWithoutProfiles(Tabulated As Boolean, YZ As Boolean, Trapezium As Boolean, Circular As Boolean, AllowCopyFromStructureReaches As Boolean) As Boolean
        Dim nChanged As Integer = 1
        Dim ID As String = ""
        Dim r As Long = 0
        Dim Idx As Integer
        Dim myReach As clsSbkReach, myobj As clsSbkReachObject, newObj As clsSbkReachObject, objReach As clsSbkReach = Nothing
        Dim profDat As clsProfileDatRecord = Nothing, profDef As clsProfileDefRecord = Nothing
        Dim reachesChecked As Dictionary(Of String, clsSbkReach)
        Dim ProfileFound As Boolean = False
        Dim newDat As clsProfileDatRecord = Nothing
        Dim ws As clsExcelSheet
        Dim PROFILEID As String
        ws = Setup.ExcelFile.GetAddSheet("Cross Section Copies")
        ws.ws.Cells(0, 0).Value = "Source Reach"
        ws.ws.Cells(0, 1).Value = "Profile ID"
        ws.ws.Cells(0, 2).Value = "Source X"
        ws.ws.Cells(0, 3).Value = "Source Y"
        ws.ws.Cells(0, 4).Value = "Target Reach"
        ws.ws.Cells(0, 5).Value = "Copy ID"
        ws.ws.Cells(0, 6).Value = "Copy X"
        ws.ws.Cells(0, 7).Value = "Copy Y"

        Try
            'walk through all reaches and check whether they have a profile (include profiles from interpolated neighboring reaches)
            'if not, copy the nearest profile
            Dim nReach As Integer = Reaches.Reaches.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Assigning nearest cross section for reaches without profile...", 0, 10, True)

            For i = 0 To Reaches.Reaches.Values.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, nReach)

                myReach = Reaches.Reaches.Values(i)
                PROFILEID = ""

                myobj = Nothing
                If myReach.InUse AndAlso Not myReach.ContainsProfile(PROFILEID, True) Then

                    reachesChecked = New Dictionary(Of String, clsSbkReach)
                    reachesChecked.Add(myReach.Id.Trim.ToUpper, myReach)    'mark the current reach as already checked and continue searching deeper inside the network

                    'keep searching all reaches until a profile is found
                    Idx = 0
                    While myobj Is Nothing
                        Idx += 1
                        objReach = Nothing
                        myobj = getNearestObjectFromNeighborReaches(reachesChecked, GeneralFunctions.enmNodetype.SBK_PROFILE, objReach)

                        'if this type of cross section is not allowed, set it back to nothing anyway
                        If Not myobj Is Nothing Then
                            profDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(myobj.ID.Trim.ToUpper)
                            If Not profDat Is Nothing Then
                                profDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(profDat.di.Trim.ToUpper)
                                If Not profDef Is Nothing Then
                                    If profDef.ty = GeneralFunctions.enmProfileType.tabulated AndAlso Not Tabulated Then
                                        myobj = Nothing
                                    ElseIf profDef.ty = YZ AndAlso Not YZ Then
                                        myobj = Nothing
                                    ElseIf profDef.ty = GeneralFunctions.enmProfileType.trapezium AndAlso Not Trapezium Then
                                        myobj = Nothing
                                    ElseIf profDef.ty = GeneralFunctions.enmProfileType.closedcircle AndAlso Not Circular Then
                                        myobj = Nothing
                                    ElseIf objReach.ChannelUsage = GeneralFunctions.enmChannelUsage.LINESTRUCTURE AndAlso Not AllowCopyFromStructureReaches Then
                                        myobj = Nothing
                                    End If
                                End If
                            End If
                        End If

                        If Idx > 100 Then Exit While 'safety valve to prevent eternal loops
                    End While

                    If Not myobj Is Nothing Then
                        myobj.calcXY()
                        profDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(myobj.ID.Trim.ToUpper)
                        If Not profDat Is Nothing Then
                            ID = MakeUniqueNodeID(profDat.ID)
                            profDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(profDat.di.Trim.ToUpper)
                        End If
                        If Not profDef Is Nothing Then
                            r += 1
                            newDat = New clsProfileDatRecord(Me.Setup)
                            newDat.ID = ID
                            newDat.di = profDef.ID
                            newDat.InUse = True
                            newDat.rl = profDat.rl
                            newDat.rs = profDat.rs

                            If SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.ContainsKey(newDat.ID.Trim.ToUpper) Then
                                SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(newDat.ID.Trim.ToUpper) = newDat
                            Else
                                SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Add(newDat.ID.Trim.ToUpper, newDat)
                            End If

                            myReach.CrossSectionOrigin = "Copied from reach" & myobj.ci
                            myReach.CrossSectionVerdict = "4"

                            newObj = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                            newObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE
                            newObj.InUse = True
                            newObj.ci = myReach.Id
                            newObj.lc = myReach.getReachLength / 2
                            newObj.ProfileType = myobj.ProfileType
                            newObj.calcXY()
                            newObj.ID = MakeUniqueNodeID(myobj.ID)
                            myReach.ReachObjects.Add(newObj)

                            ws.ws.Cells(r, 0).Value = myobj.ci
                            ws.ws.Cells(r, 1).Value = myobj.ID
                            ws.ws.Cells(r, 2).Value = myobj.X
                            ws.ws.Cells(r, 3).Value = myobj.Y
                            ws.ws.Cells(r, 4).Value = myReach.Id
                            ws.ws.Cells(r, 5).Value = ID
                            ws.ws.Cells(r, 6).Value = newObj.X
                            ws.ws.Cells(r, 7).Value = newObj.Y
                        End If
                    End If
                End If
            Next

            Me.Setup.Log.AddMessage("Empty reaches were successfully equipped with nearest cross section data.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function AssignNearestCrossSectionForReachesWithoutProfiles.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function getNearestObjectFromNeighborReaches(ByRef ReachesChecked As Dictionary(Of String, clsSbkReach), NodeType As GeneralFunctions.enmNodetype, ByRef objReach As clsSbkReach) As clsSbkReachObject
        Try
            Dim curDist As Double = 0
            Dim tmpObj As clsSbkReachObject = Nothing
            Dim defObj As clsSbkReachObject = Nothing
            Dim curReach As clsSbkReach
            Dim i As Long

            'start by walking through all items in the collection of reaches we've already checked
            'from there on, search for the next available reach that has NOT yet been checked
            Me.Setup.GeneralFunctions.UpdateProgressBar("Finding nearest object from neighboring reaches...", 0, 10, True)
            For i = 0 To ReachesChecked.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, ReachesChecked.Count)
                curReach = ReachesChecked.Values(i)
                'this reach has been checked. See if it has up or downstream reaches that still need checking
                For Each myReach As clsSbkReach In Reaches.Reaches.Values
                    'only try reaches that have not yet been checked for objects
                    If Not ReachesChecked.ContainsKey(myReach.Id.Trim.ToUpper) Then
                        If myReach.bn.ID = curReach.en.ID OrElse myReach.bn.ID = curReach.bn.ID Then
                            tmpObj = myReach.GetFirstObjectOnReach(GeneralFunctions.enmNodetype.SBK_PROFILE, ReachesChecked, False, False)
                            If tmpObj IsNot Nothing Then
                                defObj = tmpObj
                                objReach = myReach
                                Exit For
                            End If
                        ElseIf myReach.en.ID = curReach.en.ID OrElse myReach.en.ID = curReach.bn.ID Then
                            tmpObj = myReach.GetLastObjectOnReach(GeneralFunctions.enmNodetype.SBK_PROFILE, ReachesChecked, False, False)
                            If tmpObj IsNot Nothing Then
                                defObj = tmpObj
                                objReach = myReach
                                Exit For
                            End If
                        End If
                    End If
                Next
            Next
            Return defObj
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function GetIncomingReaches(ByRef myNode As clsSbkReachNode, SkipLineStructures As Boolean, Optional ByVal SkipCircularReaches As Boolean = False) As List(Of clsSbkReach)
        'v2.112: added the SkipLineStructures argument to avoid interpolation with e.g. long culverts
        Dim myList As New List(Of clsSbkReach)
        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If myReach.InUse AndAlso myReach.en.ID = myNode.ID AndAlso (Not myReach.ChannelUsage = GeneralFunctions.enmChannelUsage.LINESTRUCTURE OrElse SkipLineStructures) Then
                If SkipCircularReaches AndAlso myReach.bn.ID = myReach.en.ID Then
                    'skip this reach
                Else
                    myList.Add(myReach)
                End If
            End If
        Next
        Return myList
    End Function

    Public Function GetOutgoingReaches(ByRef myNode As clsSbkReachNode, SkipLineStructures As Boolean, Optional ByVal SkipCircularReaches As Boolean = False) As List(Of clsSbkReach)
        'v2.112: added the SkipLineStructures argument to avoid interpolation with e.g. long culverts
        Dim myList As New List(Of clsSbkReach)
        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If myReach.InUse AndAlso myReach.bn.ID = myNode.ID AndAlso (Not myReach.ChannelUsage = GeneralFunctions.enmChannelUsage.LINESTRUCTURE OrElse SkipLineStructures) Then
                If SkipCircularReaches AndAlso myReach.bn.ID = myReach.en.ID Then
                    'skip this reach
                Else
                    myList.Add(myReach)
                End If
            End If
        Next
        Return myList
    End Function


    Public Function ApplyReachInterpolation(Tabulated As Boolean, YZ As Boolean, Trapezium As Boolean, Circular As Boolean, MinimumProfileWidth As Double) As Boolean
        'this routine applys reach interpolation on connection nodes
        'it also checks whether the cross section types match
        Try
            Dim ws As clsExcelSheet, r As Long = 0
            Dim myAngle As Double
            Dim Found As Boolean
            Dim NodesDat As clsNodesDatNODERecord = Nothing
            Dim upProf As clsSbkReachObject, dnProf As clsSbkReachObject
            Dim upType As Integer, dnType As Integer
            Dim upDat As clsProfileDatRecord = Nothing, upDef As clsProfileDefRecord = Nothing
            Dim dnDat As clsProfileDatRecord = Nothing, dnDef As clsProfileDefRecord = Nothing

            ws = Setup.ExcelFile.GetAddSheet("Interpolation over reaches")
            ws.ws.Cells(r, 0).Value = "NodeID"
            ws.ws.Cells(r, 1).Value = "X"
            ws.ws.Cells(r, 2).Value = "Y"
            ws.ws.Cells(r, 3).Value = "Upstream Reach"
            ws.ws.Cells(r, 4).Value = "Downstream Reach"

            'walk through all reachnodes
            Dim iNode As Integer = 0
            Dim nNode As Integer = ReachNodes.ReachNodes.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Applying reach interpolation on nodes...", 0, 10, True)

            For Each myNode As clsSbkReachNode In ReachNodes.ReachNodes.Values
                iNode += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iNode, nNode)
                NodesDat = New clsNodesDatNODERecord(Me.Setup)
                Found = False

                If myNode.InUse AndAlso Not myNode.nt = GeneralFunctions.enmNodetype.NodeCFBoundary AndAlso Not SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myNode.ID.Trim.ToUpper) Then

                    'make a collection of incoming reaches and a collection of outgoing reaches
                    Dim ReachesIn As New List(Of clsSbkReach)
                    Dim ReachesOut As New List(Of clsSbkReach)
                    ReachesIn = GetIncomingReaches(myNode, True, True) 'v2.113: skipping circular reaches. Interpolation to circular reaches causes SOBEK to crash without error message
                    ReachesOut = GetOutgoingReaches(myNode, True, True) 'v2.113: skipping circular reaches. Interpolation to circular reaches causes SOBEK to crash without error message
                    Dim AnglesIn As New List(Of Double)
                    Dim AnglesOut As New List(Of Double)

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
                    Dim minAngleDiff As Double = 999, myAngleDiff As Double
                    For i = 0 To ReachesIn.Count - 1
                        For j = 0 To ReachesOut.Count - 1

                            'v2.113: making sure we are not interpolating a circular reach!
                            If ReachesIn.Item(i).Id IsNot ReachesOut.Item(j).Id Then
                                'calculate the angle difference between the incoming and outgoing reach
                                myAngleDiff = Setup.GeneralFunctions.AngleDifferenceDegrees(AnglesIn(i), AnglesOut(j))

                                'check for profiles on the upstream reach if they are not of the same type, skip this combination
                                upProf = ReachesIn(i).GetLastProfile(ReachesIn(i), True, False) 'find the first cross section on the current reach or further upstream
                                dnProf = ReachesOut(j).GetFirstProfile(ReachesOut(j), False, True) 'find the first cross section on the current reach or further downstream
                                upDef = Nothing
                                dnDef = Nothing

                                If upProf IsNot Nothing Then Call SbkCase.CFData.Data.ProfileData.getProfileDefinition(upProf.ID, upDat, upDef)
                                If dnProf IsNot Nothing Then Call SbkCase.CFData.Data.ProfileData.getProfileDefinition(dnProf.ID, dnDat, dnDef)

                                If upProf IsNot Nothing AndAlso Not dnProf Is Nothing Then
                                    upType = SbkCase.CFData.Data.ProfileData.GetProfileType(upProf.ID)
                                    dnType = SbkCase.CFData.Data.ProfileData.GetProfileType(dnProf.ID)
                                    If upType <> dnType Then Continue For
                                ElseIf upProf IsNot Nothing Then
                                    upType = SbkCase.CFData.Data.ProfileData.GetProfileType(upProf.ID)
                                    If upType = GeneralFunctions.enmProfileType.yztable AndAlso Not YZ Then Continue For
                                    If upType = GeneralFunctions.enmProfileType.trapezium AndAlso Not Trapezium Then Continue For
                                    If upType = GeneralFunctions.enmProfileType.closedcircle AndAlso Not Circular Then Continue For
                                    If upType = GeneralFunctions.enmProfileType.tabulated AndAlso Not Tabulated Then Continue For
                                    If upDef IsNot Nothing AndAlso upDef.getMaximumWidth < MinimumProfileWidth Then Continue For

                                ElseIf Not dnProf Is Nothing Then
                                    dnType = SbkCase.CFData.Data.ProfileData.GetProfileType(dnProf.ID)
                                    If dnType = GeneralFunctions.enmProfileType.yztable AndAlso Not YZ Then Continue For
                                    If dnType = GeneralFunctions.enmProfileType.trapezium AndAlso Not Trapezium Then Continue For
                                    If dnType = GeneralFunctions.enmProfileType.closedcircle AndAlso Not Circular Then Continue For
                                    If dnType = GeneralFunctions.enmProfileType.tabulated AndAlso Not Tabulated Then Continue For
                                    If dnDef IsNot Nothing AndAlso dnDef.getMaximumWidth < MinimumProfileWidth Then Continue For
                                End If

                                If myAngleDiff < minAngleDiff Then
                                    minAngleDiff = myAngleDiff
                                    NodesDat.r1 = ReachesIn(i).Id
                                    NodesDat.r2 = ReachesOut(j).Id
                                    NodesDat.ni = 1
                                    NodesDat.ID = myNode.ID
                                    NodesDat.InUse = True
                                    NodesDat.ty = 0
                                    Found = True
                                End If
                            End If
                        Next
                    Next
                End If

                If Found Then
                    SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Add(NodesDat.ID.Trim.ToUpper, NodesDat)
                    r += 1
                    ws.ws.Cells(r, 0).Value = myNode.ID
                    ws.ws.Cells(r, 1).Value = myNode.X
                    ws.ws.Cells(r, 2).Value = myNode.Y
                    ws.ws.Cells(r, 3).Value = NodesDat.r1
                    ws.ws.Cells(r, 4).Value = NodesDat.r2
                End If

            Next

            'finally, for the verdict shapefile, we will walk through all reaches and register which ones have interpolated cross sections
            Dim iReach As Integer = 0
            Dim nReach As Integer = Reaches.Reaches.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Calculating reach verdict...", 0, 10, True)

            For Each myReach In Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                If myReach.InUse AndAlso Not myReach.ContainsProfile(False) AndAlso myReach.CrossSectionOrigin Is Nothing Then
                    Dim bnNode As clsNodesDatNODERecord, enNode As clsNodesDatNODERecord
                    If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.bn.ID.Trim.ToUpper) Then
                        bnNode = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.bn.ID.Trim.ToUpper)
                        If bnNode.ni = 1 AndAlso (bnNode.r1.Trim.ToUpper = myReach.Id.Trim.ToUpper OrElse bnNode.r2.Trim.ToUpper = myReach.Id.Trim.ToUpper) Then
                            myReach.CrossSectionOrigin = "Reach to reach interpolation"
                            myReach.CrossSectionVerdict = 8
                        End If
                    End If
                    If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.en.ID.Trim.ToUpper) Then
                        enNode = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.en.ID.Trim.ToUpper)
                        If enNode.ni = 1 AndAlso (enNode.r1.Trim.ToUpper = myReach.Id.Trim.ToUpper OrElse enNode.r2.Trim.ToUpper = myReach.Id.Trim.ToUpper) Then
                            myReach.CrossSectionOrigin = "Reach to reach interpolation"
                            myReach.CrossSectionVerdict = 8
                        End If
                    End If
                End If
            Next

            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

            ''finally, for the verdict shapefile, we will walk through all nodes and register which reaches have interpolated cross sections assigned
            'For Each NodesDat In SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Values
            '    If NodesDat.ni = 1 Then
            '        If Reaches.Reaches.ContainsKey(NodesDat.r1.Trim.ToUpper) Then
            '            If Not Reaches.Reaches.Item(NodesDat.r1.Trim.ToUpper).ContainsProfile(False) Then
            '                Reaches.Reaches.Item(NodesDat.r1.Trim.ToUpper).CrossSectionOrigin = "Reach to reach interpolation"
            '                Reaches.Reaches.Item(NodesDat.r1.Trim.ToUpper).CrossSectionVerdict = 8
            '            End If
            '        End If
            '        If Reaches.Reaches.ContainsKey(NodesDat.r2.Trim.ToUpper) Then
            '            If Not Reaches.Reaches.Item(NodesDat.r2.Trim.ToUpper).ContainsProfile(False) Then
            '                Reaches.Reaches.Item(NodesDat.r2.Trim.ToUpper).CrossSectionOrigin = "Reach to reach interpolation"
            '                Reaches.Reaches.Item(NodesDat.r2.Trim.ToUpper).CrossSectionVerdict = 8
            '            End If
            '        End If
            '    End If
            'Next

            Me.Setup.Log.AddMessage("Interpolation of reaches over connection nodes was successfully implemented.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ApplyNodeInterpolation")
            Return False
        End Try
    End Function


    Public Function FindNearestReachObjectUpstreamOnReach(ByRef Reach As clsSbkReach, Chainage As Double, Structures As Boolean, Profiles As Boolean, Laterals As Boolean, ByRef objDist As Double) As clsSbkReachObject
        'this function searches the upstream part of the current reach for the nearest upstream structure, profile and/or lateral reach object
        'starting at a given chainage
        objDist = 9.0E+99
        Dim Found As Boolean = False, foundObject As clsSbkReachObject = Nothing, Dist As Double
        For Each myObj As clsSbkReachObject In Reach.ReachObjects.ReachObjects.Values
            If (Profiles AndAlso myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE) OrElse (Structures AndAlso Setup.GeneralFunctions.isStructure(myObj.nt)) OrElse (Laterals AndAlso myObj.isLateral) Then
                If myObj.lc < Chainage Then
                    Dist = Chainage - myObj.lc
                    If Dist < objDist Then
                        objDist = Dist
                        foundObject = myObj
                        Found = True
                    End If
                End If
            End If
        Next
        Return foundObject
    End Function

    Public Function FindNearestReachObjectDownstreamOnReach(ByRef Reach As clsSbkReach, Chainage As Double, Structures As Boolean, Profiles As Boolean, Laterals As Boolean, ByRef objDist As Double) As clsSbkReachObject
        'this function searches the downstream part of the current reach for the nearest upstream structure, profile and/or lateral reach object
        'starting at a given chainage
        objDist = 9.0E+99
        Dim Found As Boolean = False, foundObject As clsSbkReachObject = Nothing, Dist As Double
        For Each myObj As clsSbkReachObject In Reach.ReachObjects.ReachObjects.Values
            If (Profiles AndAlso myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE) OrElse (Structures AndAlso Setup.GeneralFunctions.isStructure(myObj.nt)) OrElse (Laterals AndAlso myObj.isLateral) Then
                If myObj.lc > Chainage Then
                    Dist = myObj.lc - Chainage
                    If Dist < objDist Then
                        objDist = Dist
                        foundObject = myObj
                        Found = True
                    End If
                End If
            End If
        Next
        Return foundObject
    End Function


    Public Function FindNearestObjectOnReach(ByRef Reach As clsSbkReach, Chainage As Double, Structures As Boolean, Profiles As Boolean, Laterals As Boolean) As clsSbkReachObject
        Try
            Dim DistUp As Double
            Dim DistDn As Double
            Dim NearestUpDist As Double = 9.0E+99
            Dim NearestDnDist As Double = 9.0E+99
            Dim NearestUp As clsSbkReachObject = Nothing
            Dim NearestDn As clsSbkReachObject = Nothing

            NearestUp = FindNearestReachObjectUpstreamOnReach(Reach, Chainage, Structures, Profiles, Laterals, DistUp)
            NearestDn = FindNearestReachObjectDownstreamOnReach(Reach, Chainage, Structures, Profiles, Laterals, DistDn)

            If Not NearestUp Is Nothing AndAlso Not NearestDn Is Nothing Then
                If DistUp < DistDn Then Return NearestUp Else Return NearestDn
            ElseIf Not NearestUp Is Nothing Then
                Return NearestUp
            ElseIf Not NearestDn Is Nothing Then
                Return NearestDn
            Else
                Return Nothing
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in functon FindingNearestObjectOnReach for reach " & Reach.Id & " and chainage " & Chainage & ".")
            Return Nothing
        End Try
    End Function

    Public Function CreateAndSnapReachObjectsFromShapefile(ByRef sf As MapWinGIS.Shapefile, sfPath As String, IdFieldIdx As Integer, ObjectType As GeneralFunctions.enmNodetype, MaxSnapDistance As Integer, IncludeInactiveReaches As Boolean, AllowSnappingToVectorPoints As Boolean, Optional ByVal SetInUseValue As Boolean = True) As Boolean
        Dim i As Long, snapReach As clsSbkReach = Nothing, snapChainage As Double, snapDistance As Double, myLoc As clsSbkReachObject
        Try

            Dim ID As String
            If Not sf.Open(sfPath) Then Throw New Exception("Error: could not open shapefile: " & sfPath)

            'import the object and add it as a reachobject
            Setup.GeneralFunctions.UpdateProgressBar("Snapping objects of type " & ObjectType.ToString & " to SOBEK reaches", 0, 10)
            For i = 0 To sf.NumShapes - 1
                ID = sf.CellValue(IdFieldIdx, i)
                Setup.GeneralFunctions.UpdateProgressBar("", i, sf.NumShapes)

                If Reaches.FindSnapLocation(sf.Shape(i).Point(0).x, sf.Shape(i).Point(0).y, MaxSnapDistance, snapReach, snapChainage, snapDistance, AllowSnappingToVectorPoints, IncludeInactiveReaches) Then
                    If Not SbkCase.CFTopo.ReachObjectExists(ID) Then
                        myLoc = snapReach.AddNewReachObject(ID, ObjectType, snapChainage, False, SetInUseValue)
                        myLoc.SnapDistance = snapDistance
                        myLoc.snapChainage = snapChainage
                    Else
                        Me.Setup.Log.AddWarning("Multiple instances of reach object " & ID & " found. Please check your input files.")
                    End If
                End If
            Next
            sf.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function


    Public Function FindUpstreamLocation(ByRef myReach As clsSbkReach, ByVal Chainage As Double, ByVal moveUpstream As Double, ByRef foundReach As clsSbkReach, ByRef foundChainage As Double) As Boolean
        'this function finds an upstream location for a given distance from a given chainage at a given reach.
        Try
            'first try to move within the same reach
            If Chainage > moveUpstream Then
                foundChainage = Chainage - moveUpstream
                foundReach = myReach
                Return True
            Else
                'we'll have to find another reach upstream
                moveUpstream -= Chainage
                For Each upReach As clsSbkReach In Reaches.Reaches.Values
                    If upReach.en.ID = myReach.bn.ID AndAlso upReach.InUse Then
                        If upReach.getReachLength >= moveUpstream Then
                            foundReach = upReach
                            foundChainage = upReach.getReachLength - moveUpstream
                            Return True
                        End If
                    End If
                Next

                'if not found, check if other reaches have our own reach 
                For Each upreach As clsSbkReach In Reaches.Reaches.Values
                    If upreach.bn.ID = myReach.bn.ID AndAlso upreach.InUse AndAlso Not upreach.Id = myReach.Id Then
                        If upreach.getReachLength = moveUpstream Then
                            foundReach = upreach
                            foundChainage = moveUpstream
                            Return True
                        End If
                    End If
                Next

                'if still not found, move the location to max 1m from upstream
                If Chainage > 1 Then
                    foundReach = myReach
                    foundChainage = 1
                    Me.Setup.Log.Warnings.Add("Warning: exact upstream point at distance " & moveUpstream & " from " & myReach.Id & " at chainage " & Chainage & " could not be found. A nearer location was chosen instead.")
                    Return True
                Else
                    Me.Setup.Log.AddError("Error finding upstream location at distance " & moveUpstream & " from " & myReach.Id & " at chainage " & Chainage)
                    Return False
                End If

            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FindUpstreamLocation for reach " & myReach.Id & " and chainage " & Chainage)
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function ReplaceReachNodeWithBoundary(ByRef oldNode As clsSbkReachNode, newID As String) As Boolean
        Try
            'create the new node
            Dim newNode As New clsSbkReachNode(Me.Setup, Setup.SOBEKData.ActiveProject.ActiveCase)
            newNode.X = oldNode.X
            newNode.Y = oldNode.Y
            newNode.ID = newID
            newNode.nt = GeneralFunctions.enmNodetype.NodeCFBoundary
            newNode.NetworkObiRecord.ID = newNode.ID
            newNode.NetworkObiRecord.ci = "SBK_BOUNDARY"
            newNode.InUse = True

            If Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.ReachNodes.ReachNodes.ContainsKey(newNode.ID.Trim.ToUpper) Then
                Me.Setup.Log.AddWarning("Schematisation already contained a node with the intended boundary ID. That particular node has been changed to boundary type.")
                oldNode.nt = GeneralFunctions.enmNodetype.NodeCFBoundary
                oldNode.NetworkObiRecord.ci = "SBK_BOUNDARY"
            Else
                Me.Setup.Log.AddMessage("Reach node " & oldNode.ID & " was replaced by boundary node " & newNode.ID)
                oldNode.InUse = False
                'add the new boundary node to the schematization
                Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.ReachNodes.ReachNodes.Add(newNode.ID.Trim.ToUpper, newNode)

                'replace all references to the old node with the new one
                For Each myReach As clsSbkReach In Reaches.Reaches.Values
                    If myReach.bn.ID = oldNode.ID Then myReach.bn = newNode
                    If myReach.en.ID = oldNode.ID Then myReach.en = newNode
                Next
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function SplitReachToAccommodateStructure(ByRef myReach As clsSbkReach, ByRef mainStructure As clsSbkReachObject, ByVal minLength As Double, Optional ByVal ApplyNodeInterpolation As Boolean = True, Optional ByVal AllowSplittingTooShortReaches As Boolean = True) As clsSbkReach
        Try
            'this function splits a reach to accommodate for a structure with parallel components (e.g. multiple pumps or barrels)
            'note: also the splitChainage location is updated if the branch is split up on the upstream side of that chainage
            Dim upReach As clsSbkReach = Nothing, downReach As clsSbkReach = Nothing
            Dim upReach2 As clsSbkReach = Nothing, downReach2 As clsSbkReach = Nothing
            Dim StructuresSorted As Dictionary(Of Double, clsSbkReachObject)

            'error handling
            If minLength = 0 Then Throw New Exception("Error splitting reach to accomodate structure. Minlength should be greater than 0.")

            'start by building a list of structures on this reach, by ascending chainage
            StructuresSorted = myReach.getStructuresSorted
            'walk through all reaches
            If myReach.getReachLength <= minLength Then
                If AllowSplittingTooShortReaches Then
                    'the reach is too short. However we are allowed to split. Do this only if another structure is found
                    If StructuresSorted.Count > 1 Then
                        For i = 0 To StructuresSorted.Count - 2
                            'split between structures
                            If StructuresSorted.Values(i).ID = mainStructure.ID Then
                                SplitReachAtChainage(myReach, (StructuresSorted.Values(i).lc + StructuresSorted.Values(i + 1).lc) / 2, upReach, downReach, ApplyNodeInterpolation)
                                Return upReach
                            ElseIf StructuresSorted.Values(i + 1).ID = mainStructure.ID Then
                                SplitReachAtChainage(myReach, (StructuresSorted.Values(i).lc + StructuresSorted.Values(i + 1).lc) / 2, upReach, downReach, ApplyNodeInterpolation)
                                Return downReach
                            End If
                        Next
                    Else
                        'the reach is too short but it only 
                        Return myReach
                    End If
                Else
                    'the reach is too short. Don't change a thing and simply return the original reach
                    Return myReach
                End If
            Else
                'the reach is eligable for splitting. First priority is to find a spot that does not split up too much if not necessary and that does not overlap with other structures
                If mainStructure.lc <= minLength AndAlso mainStructure.lc > myReach.getReachLength - minLength Then

                    If StructuresSorted.Count > 1 Then
                        For i = 0 To StructuresSorted.Count - 2
                            'split between structures
                            If StructuresSorted.Values(i).ID = mainStructure.ID Then
                                SplitReachAtChainage(myReach, (StructuresSorted.Values(i).lc + StructuresSorted.Values(i + 1).lc) / 2, upReach, downReach, ApplyNodeInterpolation)
                                Return upReach
                            ElseIf StructuresSorted.Values(i + 1).ID = mainStructure.ID Then
                                SplitReachAtChainage(myReach, (StructuresSorted.Values(i).lc + StructuresSorted.Values(i + 1).lc) / 2, upReach, downReach, ApplyNodeInterpolation)
                                Return downReach
                            End If
                        Next
                    Else
                        'the reach is too short but it only 
                        Return myReach
                    End If
                ElseIf mainStructure.lc <= minLength Then
                    SplitReachAtChainage(myReach, minLength, upReach, downReach, ApplyNodeInterpolation)
                    'v1.799: move the main structure to halfway our newly created reach
                    mainStructure.lc = upReach.getReachLength / 2
                    Return upReach
                ElseIf mainStructure.lc > myReach.getReachLength - minLength Then
                    SplitReachAtChainage(myReach, myReach.getReachLength - minLength, upReach, downReach, ApplyNodeInterpolation)
                    'v1.799: move the main structure to halfway our newly created reach
                    'v1.870: corrected a bug that placed the structure on a chainage that was incorrectly based on the original reach's length!
                    mainStructure.lc = downReach.getReachLength / 2
                    Return downReach
                Else
                    'somewehere in the middle of the reach. Split in the middle, so two split actions!
                    SplitReachAtChainage(myReach, mainStructure.lc - minLength / 2, upReach, downReach, ApplyNodeInterpolation)
                    SplitReachAtChainage(downReach, minLength, upReach2, downReach2, ApplyNodeInterpolation)
                    Return upReach2
                End If
            End If
            Return myReach
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function SplitReachToAccommodateStructure.")
            Return myReach
        End Try

    End Function

    Public Function CreateParallelReach(ByRef myReach As clsSbkReach, ExtraAngleDegrees As Double, ByVal ReverseDirection As Boolean, ID As String, UseIDasPostfix As Boolean) As clsSbkReach
        'this function parallizes an existing reach by creating new reach parallel to the original one to accommodate structures with multiple items.
        Dim newReach As clsSbkReach, ReachAngle As Double
        ReachAngle = Setup.GeneralFunctions.LineAngleDegrees(myReach.bn.X, myReach.bn.Y, myReach.en.X, myReach.en.Y)

        newReach = New clsSbkReach(Me.Setup, SbkCase)
        newReach = myReach.Clone(ID, UseIDasPostfix)
        newReach.ReachType.ParentReachType = myReach.ReachType.ParentReachType

        'if desired, reverse the reach direction (e.g. for inlet structures)
        If ReverseDirection Then
            newReach.bn = myReach.en
            newReach.en = myReach.bn
        End If

        newReach.BuildCPAngle(ExtraAngleDegrees)         'build in angles up to max 45 degrees
        newReach.NetworkcpRecord.CalcCPTable(newReach.Id, newReach.bn, newReach.en)
        Reaches.Reaches.Add(newReach.Id.Trim.ToUpper, newReach)
        Return newReach

    End Function

    Public Function getWaterLevelPointsInsideShape(ByRef myShape As MapWinGIS.Shape) As List(Of String)
        Dim newList As New List(Of String)
        Dim myPoint As clsSbkReachObject
        Dim mwPoint As MapWinGIS.Point

        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            For Each myPoint In myReach.ReachObjects.ReachObjects.Values
                If myPoint.isWaterLevelObject Then
                    myPoint.calcXY()
                    mwPoint = New MapWinGIS.Point
                    mwPoint.x = myPoint.X
                    mwPoint.y = myPoint.Y
                    If myShape.PointInThisPoly(mwPoint) Then newList.Add(myPoint.ID)
                End If
            Next
        Next
        Return newList
    End Function

    Public Function findNearestReachNode(X As Double, Y As Double, SearchRadius As Double) As clsSbkReachNode
        Dim minDist As Double = 9.9E+100, myDist As Double, nearestNode As clsSbkReachNode = Nothing
        For Each myNode As clsSbkReachNode In ReachNodes.ReachNodes.Values
            myDist = Me.Setup.GeneralFunctions.Pythagoras(X, Y, myNode.X, myNode.Y)
            If myDist <= SearchRadius AndAlso myDist < minDist AndAlso myNode.InUse Then
                minDist = myDist
                nearestNode = myNode
            End If
        Next
        Return nearestNode
    End Function

    Public Function BuildProfilesFromXYZShapeFile(SourceIdx As Integer, SelectedCategoriesOnly As Boolean, SelectedCatchmentsOnly As Boolean) As Boolean
        Try
            Dim iShape As Long, myReach As clsSbkReach, iPoint As Long
            Dim myPoint As MapWinGIS.Point, myShape As MapWinGIS.Shape, nShape As Long
            If Setup.GISData.XYZProfileShapefiles.Item(SourceIdx) Is Nothing Then Throw New Exception("Error building YZ Profiles from point shapefile because shapefile object was empty.")
            If Not Setup.GISData.XYZProfileShapefiles.Item(SourceIdx).Open() Then Throw New Exception("Error opening XYZ Profile shapefile.")

            nShape = Setup.GISData.XYZProfileShapefiles.Item(SourceIdx).sf.NumShapes - 1
            Setup.GeneralFunctions.UpdateProgressBar("Building YZ profiles from point shapefile...", 0, nShape)
            'If SelectedCategoriesOnly AndAlso Setup.GISData.ChannelCategories Is Nothing Then Throw New Exception("Error building channels from shapefile since no channel categories were specified.")

            'walk through all channel shapes
            For iShape = 0 To nShape - 1
                Setup.GeneralFunctions.UpdateProgressBar("", iShape, nShape - 1)
                'myCat = Setup.GISData.ChannelShapeFile.sf.CellValue(Setup.GISData.ChannelShapeFile.CategoryFieldIdx, iShape)

                'If SelectedCategoriesOnly = False OrElse Setup.GISData.ChannelCategories.Item(myCat.Trim.ToUpper).InUse Then

                'voeg deze tak toe
                myShape = Setup.GISData.ChannelDataSource.GetShape(iShape)
                myReach = New clsSbkReach(Setup, Me.SbkCase) 'create a new reach
                myReach.InUse = True
                myReach.Id = "reach" & iShape.ToString.Trim
                myReach.ReachType.ParentReachType = GeneralFunctions.enmReachtype.ReachCFChannel

                'check if every niew reach needs to lie (at least partially) inside an active catchment
                If SelectedCatchmentsOnly = False OrElse Setup.GISData.Catchments.PointInsideCatchment(myShape.Point(0).x, myShape.Point(0).y) OrElse Setup.GISData.Catchments.PointInsideCatchment(myShape.Point(myShape.numPoints - 1).x, myShape.Point(myShape.numPoints - 1).y) Then
                    Dim bn As clsSbkReachNode = AddReachNode(New clsXYZ(myShape.Point(0).x, myShape.Point(0).y, 0), "s" & iShape, GeneralFunctions.enmNodetype.NodeCFConnectionNode)
                    bn.nt = GeneralFunctions.enmNodetype.NodeCFConnectionNode
                    Dim en As clsSbkReachNode = AddReachNode(New clsXYZ(myShape.Point(myShape.numPoints - 1).x, myShape.Point(myShape.numPoints - 1).y, 0), "e" & iShape, GeneralFunctions.enmNodetype.NodeCFConnectionNode)
                    bn.nt = GeneralFunctions.enmNodetype.NodeCFConnectionNode

                    myReach.bn = bn
                    myReach.en = en

                    'add the vector point coordinates to the CPTable
                    For iPoint = 0 To myShape.numPoints - 1
                        myPoint = myShape.Point(iPoint)
                        myReach.NetworkcpRecord.CPTable.CP.Add(New clsSbkVectorPoint(Me.Setup, myReach.Id, myPoint.x, myPoint.y))
                    Next
                    If Not myReach.NetworkcpRecord.CompleteCPTableFromCoordinates() Then Me.Setup.Log.AddError("Unable to complete table of vector points for reach " & myReach.Id & ".")
                    myReach.NetworkcpRecord.CalcTable()
                    Reaches.Reaches.Add(myReach.Id.Trim.ToUpper, myReach)
                End If


                'End If
            Next

            Setup.GISData.ChannelDataSource.Close()
            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError("Error building cross sections from XYZ Shapefile")
            Me.Setup.Log.AddError(ex.Message)
        End Try

    End Function

    Public Function GetReachByID(ID As String) As clsSbkReach
        If Reaches.Reaches.ContainsKey(ID.Trim.ToUpper) Then
            Return Reaches.Reaches.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function


    Public Sub UpdateReachNodesInUseTag()
        'updates the 'inuse-tag' for all reachnodes
        'first initialize all reachnodes to InUSe = false
        For Each myNode As clsSbkReachNode In ReachNodes.ReachNodes.Values
            myNode.InUse = False
        Next

        'then, if a reachnode is part of a reach that's inuse, set it to InUSe again
        For Each myReach In Reaches.Reaches.Values
            If myReach.InUse Then
                myReach.bn.InUse = True
                myReach.en.InUse = True
            End If
        Next

    End Sub

    Public Function getExtent() As Boolean
        'this function gets the extent of the current sobek modelschematization and stores it in the cfTopology class
        Try
            xMin = 9.0E+99
            yMin = 9.0E+99
            xMax = -9.0E+99
            yMax = -9.0E+99
            Dim myxMin As Double
            Dim myyMin As Double
            Dim myxMax As Double
            Dim myyMax As Double
            Dim iReach As Long, nReach As Long = Reaches.Reaches.Values.Count

            Me.Setup.GeneralFunctions.UpdateProgressBar("Finding model extent...", iReach, nReach)
            If Reaches.Reaches.Count = 0 Then Throw New Exception("Error: no reaches found in the topology.")
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                myReach.getextent(myxMin, myyMin, myxMax, myyMax)
                If myxMin < xMin Then xMin = myxMin
                If myyMin < yMin Then yMin = myyMin
                If myxMax > xMax Then xMax = myxMax
                If myyMax > yMax Then yMax = myyMax
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getExtent of class clsCFTopology.")
            Return False
        End Try

    End Function

    Public Function SnapReachnodesToReachThroughSplit(SearchRadius As Double, MinDistFromReachStartEnd As Double) As Boolean
        'this function snaps a reach end node to an existing reach if it lies close enough in its vicinity
        Dim i As Long, iNode As clsSbkReachNode, nNode As Long
        Dim xMin As Double, yMin As Double, xMax As Double, yMax As Double
        Dim snapChainage As Double, snapDistance As Double
        Dim ws As clsExcelSheet, r As Long
        Dim SearchReaches As New List(Of clsSbkReach)
        Dim snapReach As clsSbkReach = Nothing
        Dim minDist As Double, Found As Boolean
        Dim snapToNode As clsSbkReachNode
        Dim myReach As clsSbkReach

        'start by writing the excel headers
        ws = Setup.ExcelFile.GetAddSheet("Node on Reach snapping")
        r = 0
        ws.ws.Cells(0, 0).Value = "Node ID"
        ws.ws.Cells(0, 1).Value = "X"
        ws.ws.Cells(0, 2).Value = "Y"
        ws.ws.Cells(0, 3).Value = "Reach ID"
        ws.ws.Cells(0, 4).Value = "Reach Chainage"
        ws.ws.Cells(0, 5).Value = "Snapping Distance"
        ws.ws.Cells(0, 6).Value = "Snap location moved to node"

        Try
            'populate all tiles with all reaches that might potentially intersect with them
            ReadReachTiles(100)
            For Each myReach In Reaches.Reaches.Values
                myReach.getextent(xMin, yMin, xMax, yMax)
                ReachTiles.AddAreaObject(myReach, xMin, yMin, xMax, yMax)
            Next

            'start by finding a possible snap location for each of the connection nodes
            Setup.GeneralFunctions.UpdateProgressBar("Snapping connection nodes to existing reaches...", 0, 10)
            nNode = ReachNodes.ReachNodes.Count
            For i = 0 To nNode - 1
                Setup.GeneralFunctions.UpdateProgressBar("", i + 1, nNode)
                iNode = ReachNodes.ReachNodes.Values(i)

                'if we found a node that is active AND that is a dead end, try to snap it
                'siebe may 6 2019: for Noorderzijlvest this check if nconnected = 1 does not work since it results in unconnected parallel reaches
                'therefore removed the check

                If iNode.InUse Then ' AndAlso iNode.NumConnectedReaches(True) = 1 Then

                    Found = False
                    minDist = 9.0E+99

                    'now collect all reaches that might lie within the same tile or its neigbors
                    Dim myReaches As List(Of Object) = ReachTiles.getNearbyObjects(iNode.X, iNode.Y, True)
                    For Each myReach In myReaches
                        If myReach.InUse AndAlso Not iNode.ID = myReach.bn.ID AndAlso Not iNode.ID = myReach.en.ID Then
                            If myReach.calcSnapLocation(iNode.X, iNode.Y, SearchRadius, snapChainage, snapDistance, True) Then

                                If snapDistance < SearchRadius Then
                                    Found = True
                                    If snapDistance < minDist Then
                                        minDist = snapDistance
                                        iNode.SnapChainage = snapChainage
                                        iNode.SnapDistance = snapDistance
                                        iNode.SnapReachID = myReach.Id
                                    End If
                                End If
                            End If
                        End If
                    Next

                    'if snapping takes place on a structure reach, revert to its start- or endnode
                    If Found Then
                        snapReach = Reaches.GetReach(iNode.SnapReachID)
                        r += 1
                        ws.ws.Cells(r, 0).Value = iNode.ID
                        ws.ws.Cells(r, 1).Value = iNode.X
                        ws.ws.Cells(r, 2).Value = iNode.Y
                        ws.ws.Cells(r, 3).Value = iNode.SnapReachID
                        ws.ws.Cells(r, 4).Value = iNode.SnapChainage
                        ws.ws.Cells(r, 5).Value = iNode.SnapDistance

                        'we cannot snap to a structure reach Or wehen the snapping location Is too close to the start Or end
                        If snapReach.ChannelUsage = GeneralFunctions.enmChannelUsage.LINESTRUCTURE OrElse iNode.SnapChainage < MinDistFromReachStartEnd OrElse iNode.SnapChainage > snapReach.getReachLength - MinDistFromReachStartEnd Then

                            'here we will snap to the start- or endpoint of our snapping reach, for one of the above reasons
                            Dim upDist As Double = Setup.GeneralFunctions.Pythagoras(iNode.X, iNode.Y, snapReach.bn.X, snapReach.bn.Y)
                            Dim dnDist As Double = Setup.GeneralFunctions.Pythagoras(iNode.X, iNode.Y, snapReach.en.X, snapReach.en.Y)

                            If upDist < dnDist Then
                                snapToNode = snapReach.bn
                                ws.ws.Cells(r, 6).Value = snapReach.bn.ID
                            Else
                                snapToNode = snapReach.en
                                ws.ws.Cells(r, 6).Value = snapReach.en.ID
                            End If

                            'v2.112: first make sure we won't end up with 0-length reaches
                            Dim SnappingAllowed As Boolean = True
                            For Each myReach In myReaches
                                If myReach.bn.ID = iNode.ID AndAlso myReach.en.ID = snapToNode.ID Then
                                    SnappingAllowed = False
                                    Exit For
                                End If
                                If myReach.en.ID = iNode.ID AndAlso myReach.bn.ID = snapToNode.ID Then
                                    SnappingAllowed = False
                                    Exit For
                                End If
                            Next

                            'walk through all reaches we found inside the quadrant + its neigbors and replace all existing references to iNode
                            If SnappingAllowed Then
                                For Each myReach In myReaches
                                    If myReach.bn.ID = iNode.ID Then myReach.replaceStartingNode(snapToNode)
                                    If myReach.en.ID = iNode.ID Then myReach.en = snapToNode
                                Next
                            End If

                        Else
                            If Not SplitReachWithExistingReachNode(iNode, snapReach, iNode.SnapChainage) Then Me.Setup.Log.AddError("Error splitting reach " & iNode.SnapReachID & " for snapping node " & iNode.ID & ".")
                        End If

                    End If
                End If
            Next

            Me.Setup.Log.AddMessage("Reach nodes have been successfully snapped to reaches.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function SnapReachnodesToReachThroughSplit of class clsCFTopology.")
            Return False
        End Try
    End Function

    Public Function ExportBoundariesToShapefile(path As String) As Boolean
        Try
            Dim BoundFieldIdx As Integer, BoundTypeIdx As Integer, BoundValIdx As Integer
            If Not Setup.GISData.BoundaryDataSource.CreateNewShapefile(path, MapWinGIS.ShpfileType.SHP_POINT) Then Throw New Exception("Error creating boundaries shapefile.")
            If Not Setup.GISData.BoundaryDataSource.Shapefile.sf.StartEditingShapes(True) Then Throw New Exception("Error editing newly created boundaries shapefile.")
            If Not Setup.GISData.BoundaryDataSource.Shapefile.CreateField("BOUNDID", MapWinGIS.FieldType.STRING_FIELD, 0, 30, BoundFieldIdx) Then Throw New Exception("Could not create ID Field in newly created Boundaries shapefile.")
            If Not Setup.GISData.BoundaryDataSource.Shapefile.CreateField("BOUNDTYPE", MapWinGIS.FieldType.STRING_FIELD, 0, 30, BoundTypeIdx) Then Throw New Exception("Could not create TYPE Field in newly created Boundaries shapefile.")
            If Not Setup.GISData.BoundaryDataSource.Shapefile.CreateField("BOUNDVAL", MapWinGIS.FieldType.DOUBLE_FIELD, 2, 10, BoundValIdx) Then Throw New Exception("Could not create VALUE Field in newly created Boundaries shapefile.")

            Setup.GISData.BoundaryDataSource.SetField(GeneralFunctions.enmInternalVariable.ID, "BOUNDID", "Boundary ID Field", GeneralFunctions.enmMessageType.ErrorMessage)
            Setup.GISData.BoundaryDataSource.SetField(GeneralFunctions.enmInternalVariable.BoundaryCategory, "BOUNDTYPE", "Boundary Type", GeneralFunctions.enmMessageType.ErrorMessage)
            Setup.GISData.BoundaryDataSource.SetField(GeneralFunctions.enmInternalVariable.BoundaryValue, "BOUNDVAL", "Boundary Type", GeneralFunctions.enmMessageType.ErrorMessage)

            'v1.799: switched from processing all reaches to processing all reach nodes to avoid double instances of boundaries in the resulting shapefile!
            For Each myReachNode As clsSbkReachNode In ReachNodes.ReachNodes.Values
                If myReachNode.InUse AndAlso myReachNode.nt = GeneralFunctions.enmNodetype.NodeCFBoundary Then
                    Dim myShape As MapWinGIS.Shape = myReachNode.exportAsShape()
                    Dim myShapeIdx As Integer = Setup.GISData.BoundaryDataSource.Shapefile.sf.EditAddShape(myShape)
                    Setup.GISData.BoundaryDataSource.Shapefile.sf.EditCellValue(Setup.GISData.BoundaryDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.ID).ColIdx, myShapeIdx, myReachNode.ID)
                    Setup.GISData.BoundaryDataSource.Shapefile.sf.EditCellValue(Setup.GISData.BoundaryDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.BoundaryCategory).ColIdx, myShapeIdx, "H")
                    Setup.GISData.BoundaryDataSource.Shapefile.sf.EditCellValue(Setup.GISData.BoundaryDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.BoundaryValue).ColIdx, myShapeIdx, "0")
                End If
            Next

            Setup.GISData.BoundaryDataSource.Shapefile.sf.StopEditingShapes(True, True)
            Setup.GISData.BoundaryDataSource.Shapefile.sf.Save()
            Setup.GISData.BoundaryDataSource.Shapefile.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error In Function ExportBoundariesToShapefile")
            Return False
        End Try
    End Function

    Public Function ExportReachesToShapefile(path As String) As Boolean
        Try
            Dim ReachIdx As Integer, FromNodeIdx As Integer, ToNodeIdx As Integer
            If Not Setup.GISData.ChannelDataSource.CreateNewShapefile(path, MapWinGIS.ShpfileType.SHP_POLYLINE) Then Throw New Exception("Error creating Reaches shapefile.")
            Setup.GISData.ChannelDataSource.Open()
            If Not Setup.GISData.ChannelDataSource.Shapefile.sf.StartEditingShapes(True) Then Throw New Exception("Error editing newly created channels shapefile.")
            If Not Setup.GISData.ChannelDataSource.Shapefile.CreateField("REACHID", MapWinGIS.FieldType.STRING_FIELD, 0, 30, ReachIdx) Then Throw New Exception("Could Not create ID Field In newly created Channel Shapefile.")
            If Not Setup.GISData.ChannelDataSource.Shapefile.CreateField("FROMNODE", MapWinGIS.FieldType.STRING_FIELD, 0, 30, FromNodeIdx) Then Throw New Exception("Could Not create ID Field In newly created Channel Shapefile.")
            If Not Setup.GISData.ChannelDataSource.Shapefile.CreateField("TONODE", MapWinGIS.FieldType.STRING_FIELD, 0, 30, ToNodeIdx) Then Throw New Exception("Could Not create ID Field In newly created Channel Shapefile.")
            Setup.GISData.ChannelDataSource.SetField(GeneralFunctions.enmInternalVariable.ID, "REACHID")
            Setup.GISData.ChannelDataSource.SetField(GeneralFunctions.enmInternalVariable.FromNodeID, "FROMNODE")
            Setup.GISData.ChannelDataSource.SetField(GeneralFunctions.enmInternalVariable.ToNodeID, "TONODE")
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                If myReach.InUse Then
                    Dim myShape As MapWinGIS.Shape = myReach.exportToShape()
                    Dim myShapeIdx As Integer = Setup.GISData.ChannelDataSource.Shapefile.sf.EditAddShape(myShape)
                    Setup.GISData.ChannelDataSource.Shapefile.sf.EditCellValue(Setup.GISData.ChannelDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.ID).ColIdx, myShapeIdx, myReach.Id)
                    Setup.GISData.ChannelDataSource.Shapefile.sf.EditCellValue(Setup.GISData.ChannelDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.FromNodeID).ColIdx, myShapeIdx, myReach.bn.ID)
                    Setup.GISData.ChannelDataSource.Shapefile.sf.EditCellValue(Setup.GISData.ChannelDataSource.GetFirstGeoField(GeneralFunctions.enmInternalVariable.ToNodeID).ColIdx, myShapeIdx, myReach.en.ID)
                End If
            Next
            Setup.GISData.ChannelDataSource.Shapefile.sf.StopEditingShapes(True, True)
            Setup.GISData.ChannelDataSource.Shapefile.sf.Save()
            Setup.GISData.ChannelDataSource.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error In Function exportReachesToVerdictShapefile Of Class clsCFTopology.")
            Return False
        End Try
    End Function

    Public Function RemoveUnusedReachesAndReachNodes() As Boolean
        Try
            Dim i As Long, myReach As clsSbkReach, myNode As clsSbkReachNode
            Dim nReaches As Long = Reaches.Reaches.Count
            Dim nNodes As Long = ReachNodes.ReachNodes.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Removing unused reaches...", 0, 10, True)
            For i = Reaches.Reaches.Count - 1 To 0 Step -1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", nReaches - i, nReaches)
                myReach = Reaches.Reaches.Values(i)
                If myReach.InUse = False Then Reaches.Reaches.Remove(Reaches.Reaches.Keys(i))
            Next

            Me.Setup.GeneralFunctions.UpdateProgressBar("Removing unused connection nodes...", 0, 10, True)
            For i = ReachNodes.ReachNodes.Count - 1 To 0 Step -1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", nNodes - i, nNodes)
                myNode = ReachNodes.ReachNodes.Values(i)
                If myNode.InUse = False Then ReachNodes.ReachNodes.Remove(ReachNodes.ReachNodes.Keys(i))
            Next

            Me.Setup.Log.AddMessage("Unused reaches And reachnodes have successfully been removed.")
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetNearestCalculationPointID(ByVal Obj As clsSbkReachObject, ByRef myCalcID As String) As Boolean
        Dim minDist As Double = 9.0E+99, myDist As Double
        Dim myReach As clsSbkReach

        Try
            myReach = Reaches.GetReach(Obj.ci)

            'start met de beginknoop van de onderhavige tak. is altijd een rekenpunt
            minDist = Math.Abs(Obj.lc)
            myCalcID = myReach.bn.ID

            'doorloop alle rekenpunten op zoek naar een die dichterbij ligt
            For Each myCalc As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                If myCalc.IsGridPoint Then
                    myDist = Math.Abs(myCalc.lc - Obj.lc)
                    If myDist < minDist Then
                        minDist = myDist
                        myCalcID = myCalc.ID
                    End If
                End If
            Next

            'en als laatste de eindknoop van de tak
            If Math.Abs(myReach.getReachLength - Obj.lc) < minDist Then
                minDist = Math.Abs(myReach.getReachLength - Obj.lc)
                myCalcID = myReach.en.ID
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function Check() As Boolean
        'this function checks the validity of the flow topology
        Try
            If Not CheckForNodesAtSameLocation() Then Me.Setup.Log.AddError("Error in flow topology check: check for nodes at the same location failed.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function Check of class clsCFTopology.")
            Return False
        End Try
    End Function


    Friend Function CheckForNodesAtSameLocation() As Boolean
        'this function checks for double nodes at the same location
        Dim i As Long, j As Long, iObj As clsSbkReachObject, jObj As clsSbkReachObject

        Try
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                If myReach.ReachObjects.ReachObjects.Count > 1 Then
                    For i = 0 To myReach.ReachObjects.ReachObjects.Values.Count - 1
                        iObj = myReach.ReachObjects.ReachObjects.Values(i)
                        For j = i + 1 To myReach.ReachObjects.ReachObjects.Values.Count - 1
                            jObj = myReach.ReachObjects.ReachObjects.Values(j)
                            If Math.Abs(iObj.lc - jObj.lc) < 0.1 Then Me.Setup.Log.AddError("Two objects at the same location on the reach: " & iObj.ID & " and " & jObj.ID)
                        Next
                    Next
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function Check of class clsCFTopology.")
            Return False
        End Try
    End Function

    Public Sub AddPrefix(ByVal Prefix As String)
        Reaches.AddPrefix(Prefix)
        ReachNodes.addPrefix(Prefix)
    End Sub

    Public Function getNodeType(ByVal ID As String) As STOCHLIB.GeneralFunctions.enmNodetype

        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If myReach.bn.ID.Trim.ToUpper = ID.Trim.ToUpper Then
                Return myReach.bn.nt
            ElseIf myReach.en.ID.Trim.ToUpper = ID.Trim.ToUpper Then
                Return myReach.bn.nt
            Else
                For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    If myObj.ID.Trim.ToUpper = ID.Trim.ToUpper Then
                        Return myObj.nt
                    End If
                Next

                'if still not found, get the node type from the OBI record
                Dim myRecord As clsNetworkOBIOBIDRecord = NetworkOBIOBIDrecords.GetRecord(ID)
                If myRecord Is Nothing Then
                    Return GeneralFunctions.enmNodetype.NodeCFGridpoint
                Else
                    Return myRecord.GetNodeType
                End If
            End If
        Next
    End Function

    Public Function GetNodeXY(ID As String, ByRef X As Double, ByRef Y As Double) As Boolean
        ID = ID.Trim.ToUpper
        Try
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                If myReach.bn.ID.Trim.ToUpper = ID Then
                    X = myReach.bn.X
                    Y = myReach.bn.Y
                    Return True
                ElseIf myReach.en.ID.Trim.ToUpper = ID Then
                    X = myReach.en.X
                    Y = myReach.en.Y
                    Return True
                Else
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.ID.Trim.ToUpper = ID Then
                            myObj.calcXY()
                            X = myObj.X
                            Y = myObj.Y
                            Return True
                        End If
                    Next
                End If
            Next
            Me.Setup.Log.AddError("Node ID not found in SOBEK Schematization: " & ID)
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetNodeXY while processing node " & ID)
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GetReachNodeByID(ByVal NodeID As String) As clsSbkReachNode
        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If myReach.bn.ID.Trim.ToUpper = NodeID.Trim.ToUpper Then
                Return myReach.bn
            ElseIf myReach.en.ID.Trim.ToUpper = NodeID.Trim.ToUpper Then
                Return myReach.en
            End If
        Next
        Return Nothing
    End Function
    Public Function GetReachByNodeID(ByVal NodeID As String) As clsSbkReach
        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If myReach.bn.ID.Trim.ToUpper = NodeID.Trim.ToUpper Then
                Return myReach
            ElseIf myReach.en.ID.Trim.ToUpper = NodeID.Trim.ToUpper Then
                Return myReach
            ElseIf myReach.ReachObjects.ReachObjects.ContainsKey(NodeID.Trim.ToUpper) Then
                Return myReach
            End If
        Next
        Return Nothing
    End Function

    Friend Function GetStorageConstructionDischargeObject(ByVal NodeID As String) As clsSbkReachObject
        Dim myReach As clsSbkReach
        Dim constReach As clsSbkReach
        Dim myObj As clsSbkReachObject

        myReach = GetReachByNodeID(NodeID)

        'make a collection containing all reaches that make up the storage construction
        Dim myConstruction As New Dictionary(Of String, clsSbkReach)
        myConstruction = CollectStorageReachesFromUpstreamReach(myReach)

        'walk through the OUTGOING reaches to investigate the structure type
        For Each constReach In myConstruction.Values
            If constReach.bn.ID = myReach.en.ID Then
                For Each myObj In constReach.ReachObjects.ReachObjects.Values
                    If Setup.GeneralFunctions.isStructure(myObj.nt) Then Return myObj
                Next
            End If
        Next

        'no outgoing structure found, so retun nothing
        Return Nothing

    End Function

    Friend Function CollectStorageReachesFromUpstreamReach(ByVal myReach As clsSbkReach, Optional ByVal UpReachPrefix As String = "rUp") As Dictionary(Of String, clsSbkReach)
        'returns a dictionary of reaches that together form an entire storage construction
        Dim myDict As New Dictionary(Of String, clsSbkReach)
        Dim nextReaches As New Dictionary(Of String, clsSbkReach)

        If Left(myReach.Id, UpReachPrefix.Length).Trim.ToUpper = UpReachPrefix.Trim.ToUpper Then
            'apparently this is a storage construction. collect the downstream construction reaches and include the current reach
            myDict = Reaches.GetDownReaches(myReach)
            myDict.Add(myReach.Id.Trim.ToUpper, myReach)
        End If

        Return myDict

    End Function


    Public Sub WriteCalculationPointsToExcel()
        Dim topoSheet As clsExcelSheet
        Dim row As Long = 0

        'in case the vector points have not yet been read
        If WaterLevelPoints.Count = 0 Then Call CollectAllWaterLevelPoints()

        topoSheet = Me.Setup.ExcelFile.GetAddSheet("CalcPoints")

        topoSheet.ws.Cells(row, 0).Value = "NodeID"
        topoSheet.ws.Cells(row, 1).Value = "Nodetype"
        topoSheet.ws.Cells(row, 2).Value = "ReachID"
        topoSheet.ws.Cells(row, 3).Value = "VectorID"
        topoSheet.ws.Cells(row, 4).Value = "VectorPointIndex"
        topoSheet.ws.Cells(row, 5).Value = "Distance"
        topoSheet.ws.Cells(row, 6).Value = "X"
        topoSheet.ws.Cells(row, 7).Value = "Y"
        topoSheet.ws.Cells(row, 8).Value = "Lat"
        topoSheet.ws.Cells(row, 9).Value = "Lon"

        For Each myResultsPoint As clsSbkVectorPoint In WaterLevelPoints.Values
            row += 1
            myResultsPoint.toWorkSheet(topoSheet, row)
        Next

    End Sub

    Public Sub WriteVectorPointsToExcel()
        Dim topoSheet As clsExcelSheet
        Dim row As Long = 0
        Dim Lat As Double, Lon As Double, myVectorPoint As clsSbkVectorPoint

        'in case the vector points have not yet been read
        If VectorPoints.Count = 0 Then Call CollectAllVectorPoints()

        topoSheet = Me.Setup.ExcelFile.GetAddSheet("VectorPoints")

        topoSheet.ws.Cells(row, 0).Value = "ReachID"
        topoSheet.ws.Cells(row, 1).Value = "Distance"
        topoSheet.ws.Cells(row, 2).Value = "VectorPointIndex"
        topoSheet.ws.Cells(row, 3).Value = "VectorID"
        topoSheet.ws.Cells(row, 4).Value = "X"
        topoSheet.ws.Cells(row, 5).Value = "Y"
        topoSheet.ws.Cells(row, 6).Value = "Lat"
        topoSheet.ws.Cells(row, 7).Value = "Lon"

        'schrijf de topologie (curving points) naar het werkblad topoSheet
        For Each myVectorPoint In VectorPoints.Values
            'schrijf de topologie van dit vectorpunt weg naar het Excel-werkblad
            Call Me.Setup.GeneralFunctions.RD2WGS84(myVectorPoint.X, myVectorPoint.Y, Lat, Lon)
            topoSheet.ws.Cells(row, 0).Value = myVectorPoint.ReachID
            topoSheet.ws.Cells(row, 1).Value = myVectorPoint.Dist
            topoSheet.ws.Cells(row, 2).Value = myVectorPoint.Idx
            topoSheet.ws.Cells(row, 3).Value = myVectorPoint.ID
            topoSheet.ws.Cells(row, 4).Value = myVectorPoint.X
            topoSheet.ws.Cells(row, 5).Value = myVectorPoint.Y
            topoSheet.ws.Cells(row, 6).Value = Lat
            topoSheet.ws.Cells(row, 7).Value = Lon
        Next

    End Sub

    Public Function AddReachNode(ByVal Location As clsXYZ, ByVal ID As String, ByVal NodeType As GeneralFunctions.enmNodetype) As clsSbkReachNode
        Dim myNode As clsSbkReachNode
        If ReachNodes.ReachNodes.ContainsKey(ID.Trim.ToUpper) Then
            myNode = ReachNodes.ReachNodes.Item(ID.Trim.ToUpper)
        Else
            myNode = New clsSbkReachNode(Me.Setup, Me.SbkCase)
            myNode.ID = ID
            myNode.X = Location.X
            myNode.Y = Location.Y
            myNode.Name = ID
            myNode.InUse = True
            myNode.nt = NodeType
            ReachNodes.ReachNodes.Add(myNode.ID.Trim.ToUpper, myNode)
        End If
        Return myNode
    End Function

    Public Function CollectAllWaterLevelPoints() As Dictionary(Of String, clsSbkVectorPoint)
        'this routine collects all calculation points inside the current case
        'it assumes that the calculation points are stored per reach, so it harvests them from there
        'and stores them in the collection CalcPoints on this level
        Dim iReach As Integer, nReach As Integer, iVector As Long
        Dim myResultsPoint As clsSbkVectorPoint

        'reset the dictionary of calculation points
        WaterLevelPoints = New Dictionary(Of String, clsSbkVectorPoint)

        iReach = 0
        nReach = Reaches.Reaches.Count
        Me.Setup.GeneralFunctions.UpdateProgressBar("Populating reachobjects for the reference case.", iReach, nReach)

        For Each myReach In Reaches.Reaches.Values
            iReach += 1
            iVector = 0
            Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)

            'doorloop alle rekenpunten op de tak. Start en eindnode zijn al inbegrepen want maken onderdeel uit van network.gr
            For Each myObject In myReach.ReachObjects.ReachObjects.Values
                If myObject.isWaterLevelObject AndAlso Not myObject.isQBound Then
                    iVector += 1
                    myObject.calcXY()
                    myResultsPoint = New clsSbkVectorPoint(Me.Setup)
                    myResultsPoint.VectorID = myReach.Id & "_" & iVector
                    myResultsPoint.Idx = iVector
                    myResultsPoint.ID = myObject.ID
                    myResultsPoint.X = myObject.X
                    myResultsPoint.Y = myObject.Y
                    myResultsPoint.Dist = myObject.lc
                    myResultsPoint.ReachID = myReach.Id
                    myResultsPoint.Angle = myReach.NetworkcpRecord.getAngle(myResultsPoint.Dist)
                    myResultsPoint.nt = myObject.nt
                    If Not WaterLevelPoints.ContainsKey(myResultsPoint.ID.Trim.ToUpper) Then
                        WaterLevelPoints.Add(myResultsPoint.ID.Trim.ToUpper, myResultsPoint)
                    End If
                End If
            Next
        Next
        Return WaterLevelPoints
    End Function

    Public Sub CollectAllVectorPoints()

        'populates the dictionary with vectr points for this sobek case
        Dim row As Long = 0
        Dim iReach As Integer, nReach As Integer, iVector As Long
        VectorPoints = New Dictionary(Of String, clsSbkVectorPoint)

        iReach = 0
        nReach = Reaches.Reaches.Count
        Me.Setup.GeneralFunctions.UpdateProgressBar("Populating reach objects for the reference case.", iReach, nReach)
        For Each myReach In Reaches.Reaches.Values
            iReach += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)

            iVector = 0
            For Each myVectorPoint In myReach.NetworkcpRecord.CPTable.CP
                iVector += 1
                row += 1

                'voeg een indexnummer en ID toe aan het vectorpunt en voeg het vectorpunt toe aan de dictionary
                myVectorPoint.Idx = iVector
                myVectorPoint.ReachID = myReach.Id
                myVectorPoint.ID = myReach.Id & "_" & myVectorPoint.Idx

                'zoek de omringende rekenpunten
                myVectorPoint.upCP = myReach.ReachObjects.GetUpstreamCalculationPoint(myVectorPoint.Dist)
                myVectorPoint.downCP = myReach.ReachObjects.GetDownstreamCalculationPoint(myVectorPoint.Dist)
                If myVectorPoint.upCP Is Nothing Then myVectorPoint.upCP = myVectorPoint.downCP
                If myVectorPoint.downCP Is Nothing Then myVectorPoint.downCP = myVectorPoint.upCP

                'zoek het vectorpunt toe aan de lokale collectie
                VectorPoints.Add(myVectorPoint.ID.Trim.ToUpper, myVectorPoint)
            Next
        Next
    End Sub

    Friend Sub RemoveReachesByPrefix(ByVal Prefix As String)
        'This sub removes all reaches in the current case if they have the specified prefix
        Dim RemoveKeys As New List(Of String)

        'make a list of all keys from reaches that need removal
        For Each myKey In Reaches.Reaches.Keys
            If Left(myKey.Trim.ToUpper, Prefix.Trim.Length) = Prefix.Trim.ToUpper Then
                RemoveKeys.Add(myKey)
            End If
        Next

        'remove the appropriate reaches
        For Each myKey In RemoveKeys
            Reaches.Reaches.Remove(myKey)
        Next

    End Sub

    Public Function RemoveDeadBranches(ByVal MaxLength As Double, ByVal KeepBoundaries As Boolean, ByVal KeepConnLat As Boolean, ByVal KeepRRCFConnNodes As Boolean, ByVal KeepLaterals As Boolean, ByVal KeepRRCFConnections As Boolean, KeepCrossSections As Boolean, KeepStructures As Boolean) As Boolean
        Try
            Dim KeepReach As Boolean
            Dim i As Long, r As Long
            Dim ws As clsExcelSheet
            Dim myLoop As clsSbkReaches
            Dim nAdjusted As Integer = 1

            ws = Setup.ExcelFile.GetAddSheet("Dead Branches Removed")
            ws.ws.Cells(0, 0).Value = "Reach ID"
            ws.ws.Cells(0, 1).Value = "X start"
            ws.ws.Cells(0, 2).Value = "Y start"
            ws.ws.Cells(0, 3).Value = "X end"
            ws.ws.Cells(0, 4).Value = "Y end"
            ws.ws.Cells(0, 5).Value = "Length"

            While nAdjusted > 0
                nAdjusted = 0
                i = 0
                Dim n As Integer = Reaches.Reaches.Count
                Me.Setup.GeneralFunctions.UpdateProgressBar("Identifying and removing dead branches.", 0, 10)
                For Each myReach As clsSbkReach In Reaches.Reaches.Values

                    i += 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                    If myReach.InUse AndAlso myReach.getReachLength <= MaxLength Then
                        'first establish whether the reach itself should be kept
                        KeepReach = False
                        Dim ProfID As String = ""
                        If myReach.bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary AndAlso KeepBoundaries Then KeepReach = True
                        If myReach.en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary AndAlso KeepBoundaries Then KeepReach = True
                        If myReach.bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.ConnNodeLatStor AndAlso KeepConnLat Then KeepReach = True
                        If myReach.en.nt = STOCHLIB.GeneralFunctions.enmNodetype.ConnNodeLatStor AndAlso KeepConnLat Then KeepReach = True
                        If myReach.bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode AndAlso KeepRRCFConnNodes Then KeepReach = True
                        If myReach.en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode AndAlso KeepRRCFConnNodes Then KeepReach = True
                        If myReach.ContainsProfile(ProfID) AndAlso KeepCrossSections Then KeepReach = True
                        If myReach.ContainsStructure AndAlso KeepStructures Then KeepReach = True
                        If myReach.ContainsLateral AndAlso KeepLaterals Then KeepReach = True
                        If myReach.ContainsRRCFConnection AndAlso KeepRRCFConnections Then KeepReach = True
                        If myReach.containsLinkage Then KeepReach = True
                        If myReach.containsMeasurement Then KeepReach = True

                        'next find out whether this reach is a dead end or part of a loop
                        myLoop = New clsSbkReaches(Me.Setup, Me.SbkCase)  'initialize the collection of reaches in a loop
                        If KeepReach = False Then
                            If myReach.HasUpstreamReach(True) = False OrElse myReach.HasDownstreamReach(True) = False Then
                                myReach.InUse = False
                                nAdjusted += 1
                                r += 1
                                ws.ws.Cells(r, 0).Value = myReach.Id
                                ws.ws.Cells(r, 1).Value = myReach.bn.X
                                ws.ws.Cells(r, 2).Value = myReach.bn.Y
                                ws.ws.Cells(r, 3).Value = myReach.en.X
                                ws.ws.Cells(r, 4).Value = myReach.en.Y
                                ws.ws.Cells(r, 5).Value = myReach.getReachLength
                            ElseIf myReach.IsPartOfLoop(myLoop, True) Then

                                'we found a loop! Now only if all of the reaches inside the loop meet the requirements, we can set them to Inuse = False
                                For Each LoopReach As clsSbkReach In myLoop.Reaches.Values
                                    Dim ProfileID As String = ""
                                    If LoopReach.bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary AndAlso KeepBoundaries Then KeepReach = True
                                    If LoopReach.en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary AndAlso KeepBoundaries Then KeepReach = True
                                    If LoopReach.bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.ConnNodeLatStor AndAlso KeepConnLat Then KeepReach = True
                                    If LoopReach.en.nt = STOCHLIB.GeneralFunctions.enmNodetype.ConnNodeLatStor AndAlso KeepConnLat Then KeepReach = True
                                    If LoopReach.bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode AndAlso KeepRRCFConnNodes Then KeepReach = True
                                    If LoopReach.en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode AndAlso KeepRRCFConnNodes Then KeepReach = True
                                    If myReach.ContainsProfile(ProfileID) AndAlso KeepCrossSections Then KeepReach = True
                                    If myReach.ContainsStructure AndAlso KeepStructures Then KeepReach = True
                                    If LoopReach.ContainsLateral AndAlso KeepLaterals Then KeepReach = True
                                    If LoopReach.ContainsRRCFConnection AndAlso KeepRRCFConnections Then KeepReach = True
                                Next

                                'if KeepReach is still False, it means we can actually remove the entire loop!
                                If KeepReach = False Then
                                    Call myLoop.SetInuse(False)
                                    For Each loopreach As clsSbkReach In myLoop.Reaches.Values
                                        r += 1
                                        ws.ws.Cells(r, 0).Value = loopreach.Id
                                        ws.ws.Cells(r, 1).Value = loopreach.bn.X
                                        ws.ws.Cells(r, 2).Value = loopreach.bn.Y
                                        ws.ws.Cells(r, 3).Value = loopreach.en.X
                                        ws.ws.Cells(r, 4).Value = loopreach.en.Y
                                        ws.ws.Cells(r, 5).Value = myReach.getReachLength
                                    Next
                                End If
                            End If
                        End If
                    End If
                Next
            End While
            Me.Setup.Log.AddMessage("Dead branches have been successfully removed.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function RemoveDeadBranches of class clsCFTopology.")
            Return False
        End Try



    End Function

    Public Sub TruncateDeadBranchesAtLinkageNode(ByVal KeepBoundaries As Boolean, ByVal KeepLatConn As Boolean, ByVal KeepRRCFConNodes As Boolean, ByVal KeepLaterals As Boolean, ByVal KeepRRCFConnections As Boolean)
        'this section cuts off parts of branches that form 'dead wood'
        Dim i As Long, n As Long = Reaches.Reaches.Count
        Dim myReach As clsSbkReach

        Me.Setup.GeneralFunctions.UpdateProgressBar("Identifying And removing dead parts Of branches.", 0, 10)
        For i = 0 To Reaches.Reaches.Count - 1
            myReach = Reaches.Reaches.Values(i)

            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
            If myReach.InUse AndAlso myReach.containsLinkage Then
                myReach.cutoffDeadPartAtLinkage(KeepBoundaries, KeepLatConn, KeepRRCFConNodes, KeepLaterals, KeepRRCFConnections)
            End If
        Next

    End Sub


    Public Sub TruncateDeadBranchesAtRRonFlowConnections(ByVal KeepBoundaries As Boolean, ByVal KeepLatConn As Boolean, ByVal KeepRRCFConNodes As Boolean, ByVal KeepLaterals As Boolean)
        'this section cuts off parts of branches that form 'dead wood'
        Dim i As Long, n As Long = Reaches.Reaches.Count
        Dim myReach As clsSbkReach

        Me.Setup.GeneralFunctions.UpdateProgressBar("Identifying And removing dead parts Of branches.", 0, 10)
        For i = 0 To Reaches.Reaches.Count - 1
            myReach = Reaches.Reaches.Values(i)

            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
            If myReach.InUse AndAlso myReach.ContainsRRCFConnection Then
                myReach.cutoffDeadPartAtRRonFlowConnections(KeepBoundaries, KeepLatConn, KeepRRCFConNodes, KeepLaterals)
            End If
        Next

    End Sub


    Public Function SplitReachesByShapeFileIntersection() As Boolean
        Dim myReach As clsSbkReach
        Dim curCP As clsSbkVectorPoint, nextCP As clsSbkVectorPoint
        Dim curPoint As MapWinGIS.Point, nextPoint As MapWinGIS.Point
        Dim i As Long, j As Long, myShape As MapWinGIS.Shape, k As Long
        ' Dim a1 As Double, b1 As Double, a2 As Double, b2 As Double
        Dim Utils As New MapWinGIS.Utils
        'Dim AreaShape As MapWinGIS.Shape
        'Author: Siebe Bosch
        'Description: splits SOBEK-reaches at their intersections with the area shapefile

        Try
            'walk through every reach and subsequently every vector
            For Each myReach In Reaches.Reaches.Values
                For i = 0 To myReach.NetworkcpRecord.CPTable.CP.Count - 2
                    curCP = myReach.NetworkcpRecord.CPTable.CP(i)
                    nextCP = myReach.NetworkcpRecord.CPTable.CP(i + 1)
                    myShape = New MapWinGIS.Shape
                    myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)
                    myShape.AddPoint(curCP.X, curCP.Y)
                    myShape.AddPoint(nextCP.X, nextCP.Y)
                    ' Me.Setup.GeneralFunctions.lineEquation(curCP.X, curCP.Y, nextCP.X, nextCP.Y, a1, b1)

                    'find an intersection of this shape with any of the shapes in the shapefile
                    For j = 0 To Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.NumShapes - 1
                        If myShape.Intersects(Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.Shape(j)) Then
                            For k = 0 To Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.Shape(j).numPoints - 2
                                curPoint = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.Shape(j).Point(k)
                                nextPoint = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.Shape(j).Point(k + 1)
                                ' Me.Setup.GeneralFunctions.lineEquation(curCP.X, curCP.Y, nextCP.X, nextCP.Y, a2, b2)

                                ' If Me.Setup.GeneralFunctions.LineIntersection() Then
                                '  Exit For
                                '  End If
                            Next

                        End If

                    Next



                Next
            Next
        Catch ex As Exception

        End Try



    End Function

    Public Sub EmbankmentsToExcel(ByVal Profiles As Boolean, ByVal CalculationPoints As Boolean)

        Dim prSheet As clsExcelSheet, cpSheet As clsExcelSheet
        Dim myReach As clsSbkReach, myObject As clsSbkReachObject
        Dim prRow As Long = 0, cpRow As Long = 0

        If Profiles Then
            prSheet = Me.Setup.ExcelFile.GetAddSheet("Profiles")
            prSheet.ws.Cells(prRow, 0).Value = "ID"
            prSheet.ws.Cells(prRow, 1).Value = "Embankment level"
            prSheet.ws.Cells(prRow, 2).Value = "Left embankment"
            prSheet.ws.Cells(prRow, 3).Value = "Right embankment"

            For Each myReach In SbkCase.CFTopo.Reaches.Reaches.Values
                For Each myObject In myReach.ReachObjects.ReachObjects.Values
                    If myObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE AndAlso Profiles Then
                        prRow += 1
                        If Not myObject.Embankment Is Nothing Then
                            prSheet.ws.Cells(prRow, 0).Value = myObject.ID
                            prSheet.ws.Cells(prRow, 1).Value = Math.Min(myObject.Embankment.getLeft, myObject.Embankment.getRight)
                            prSheet.ws.Cells(prRow, 2).Value = myObject.Embankment.getLeft
                            prSheet.ws.Cells(prRow, 3).Value = myObject.Embankment.getRight
                        End If
                    End If
                Next
            Next
        End If

        If CalculationPoints Then
            cpSheet = Me.Setup.ExcelFile.GetAddSheet("Calculation Points")
            cpSheet.ws.Cells(cpRow, 0).Value = "ID"
            cpSheet.ws.Cells(cpRow, 1).Value = "Embankment level"
            cpSheet.ws.Cells(prRow, 2).Value = "Left embankment"
            cpSheet.ws.Cells(prRow, 3).Value = "Right embankment"

            For Each myReach In SbkCase.CFTopo.Reaches.Reaches.Values
                For Each myObject In myReach.ReachObjects.ReachObjects.Values
                    If myObject.isWaterLevelObject AndAlso CalculationPoints Then
                        cpRow += 1
                        If Not myObject.Embankment Is Nothing Then
                            cpSheet.ws.Cells(cpRow, 0).Value = myObject.ID
                            cpSheet.ws.Cells(cpRow, 1).Value = Math.Min(myObject.Embankment.getLeft, myObject.Embankment.getRight)
                            cpSheet.ws.Cells(cpRow, 2).Value = myObject.Embankment.getLeft
                            cpSheet.ws.Cells(cpRow, 3).Value = myObject.Embankment.getRight
                        End If
                    End If
                Next
            Next
        End If

    End Sub

    Public Function EmbankmentsFromGrid(ByVal SearchRadius As Integer, ByVal Profiles As Boolean, ByVal CalculationPoints As Boolean) As Boolean

        Dim myReach As clsSbkReach, myObj As clsSbkReachObject
        Dim iReach As Long = 0
        Dim prShapeFile As New MapWinGIS.Shapefile, cpShapeFile As New MapWinGIS.Shapefile
        Dim prBuffer As New MapWinGIS.Shapefile, cpBuffer As New MapWinGIS.Shapefile
        Dim tempGrid As New MapWinGIS.Grid
        Dim ut As New MapWinGIS.Utils
        Dim myAngle As Double, r As Long, c As Long, x As Double, y As Double, i As Long
        Dim myVal As Double

        Try

            Me.Setup.GISData.ElevationGrid.Read(False)
            Me.Setup.GISData.ElevationGrid.CompleteMetaHeader()

            Me.Setup.GeneralFunctions.UpdateProgressBar("Retrieving embankment levels...", 0, 10)
            For Each myReach In Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, Reaches.Reaches.Count)
                For Each myObj In myReach.ReachObjects.ReachObjects.Values
                    If (myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE AndAlso Profiles) OrElse (myObj.isWaterLevelObject AndAlso CalculationPoints) Then

                        myObj.InitializeEmbankment()
                        myObj.Embankment.setLeft(-999)
                        myObj.Embankment.setRight(-999)
                        myObj.calcAngle()

                        'left embankment
                        myAngle = Me.Setup.GeneralFunctions.D2R(Me.Setup.GeneralFunctions.NormalizeAngle(myObj.Angle - 90)) 'deduct 90 degrees, normalize and convert to radials
                        For i = 0 To SearchRadius 'move away from the reach object perpendicular to the reach direction with 1m step size
                            x = myObj.X + i * Math.Sin(myAngle)
                            y = myObj.Y + i * Math.Cos(myAngle)
                            Me.Setup.GISData.ElevationGrid.GetRCFromXY(x, y, r, c)               'find corresponding row and column in the grid
                            myVal = Me.Setup.GISData.ElevationGrid.Grid.Value(c, r)              'get value from the elevation grid
                            If myVal <> Me.Setup.GISData.ElevationGrid.Grid.Header.NodataValue AndAlso myVal > myObj.Embankment.getLeft Then myObj.Embankment.setLeft(myVal)
                        Next

                        'right embankment
                        myAngle = Me.Setup.GeneralFunctions.D2R(Me.Setup.GeneralFunctions.NormalizeAngle(myObj.Angle + 90)) 'deduct 90 degrees, normalize and convert to radials
                        For i = 0 To SearchRadius 'move away from the reach object perpendicular to the reach direction with 1m step size
                            x = myObj.X + i * Math.Sin(myAngle)
                            y = myObj.Y + i * Math.Cos(myAngle)
                            Me.Setup.GISData.ElevationGrid.GetRCFromXY(x, y, r, c)               'find corresponding row and column in the grid
                            myVal = Me.Setup.GISData.ElevationGrid.Grid.Value(c, r)              'get value from the elevation grid
                            If myVal <> Me.Setup.GISData.ElevationGrid.Grid.Header.NodataValue AndAlso myVal > myObj.Embankment.getRight Then myObj.Embankment.setRight(myVal)
                        Next

                    End If
                Next
            Next

            Me.Setup.GISData.ElevationGrid.Close()

            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.write(Me.Setup.Settings.ExportDirRoot & "\logfile.txt", True)
        End Try


    End Function

    Friend Sub BuildElevationProfilesFromElevationGrid(ByVal SearchRadius As Integer, ByVal Profiles As Boolean, ByVal CalculationPoints As Boolean)

        Dim myReach As clsSbkReach, myObj As clsSbkReachObject
        For Each myReach In Reaches.Reaches.Values
            For Each myObj In myReach.ReachObjects.ReachObjects.Values

                If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE AndAlso Profiles Then
                    myObj.ElevationProfileFromGrid(SearchRadius)
                End If

                If myObj.isWaterLevelObject AndAlso CalculationPoints Then
                    myObj.ElevationProfileFromGrid(SearchRadius)
                End If

            Next
        Next

    End Sub


    Friend Sub AddCSOLocationsToNetwork()
        'Auteur: Siebe Bosch
        'Datum: 2-4-2013
        'Beschrijving: voegt riooloverstorlocaties toe als RRCF-connections aan de modelschematisatie
        Dim myXYZ As clsXYZ

        For Each myCSO As clsRRInflowLocation In Me.Setup.CSOLocations.RRInflowLocations.Values
            If myCSO.InUse Then
                myXYZ = New clsXYZ
                myXYZ.X = myCSO.X
                myXYZ.Y = myCSO.Y
                SbkCase.CFTopo.AddRRCFConnection(myCSO.NodeID, myXYZ, True)
            End If
        Next

    End Sub

    Public Function ReadCalculationPointsByReach() As Boolean
        Try
            Dim Records As New Collection
            Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)
            Dim Path As String = SbkCase.CaseDir & "\network.gr"
            Dim i As Long

            'voorwaarde is dat de takken al wel zijn ingelezen
            If Reaches.Reaches.Count = 0 Then Call Import(SbkCase.CaseDir, "")

            'lees eventueel ook de rekenpunten uit network.gr
            'leest de knooptypes uit network.gr. en voegt ze toe aan de klasse SbkReachObjects bij de tak.
            Dim Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(Path, "GRID")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading computational grid points...", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim myRecord As New clsNetworkGRrecord(Me.Setup, Me.SbkCase)
                    myRecord.Read(Records(i), True)
                    myReach = SbkCase.CFTopo.Reaches.GetReach(myRecord.ci.Trim.ToUpper)
                    myReach.ReachObjects.NetworkGRRecord = myRecord
                Next i
            End If
            calculationpointsRead = True
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function



    Friend Sub Import(ByVal path As String, StructureReachPrefix As String)
        Me.Setup.Log.AddDebugMessage("In Import of clsStuwen")
        Dim i As Integer
        Dim Records As New Collection
        Dim myReach As New clsSbkReach(Me.Setup, Me.SbkCase)

        Try
            'leest nodes in uit network.tp
            Dim Datafile As New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.TP", "NODE")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Flow Network ...", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim NODERecord As New clsNetworkTPNodeRecord(Me.Setup)
                    NODERecord.Read(Records(i))
                    ReachNodes.AddNetworkTPNodeRecord(NODERecord)
                Next
            End If

            'leest branches in uit network.tp
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.TP", "BRCH")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Flow Branches...", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim BRCHRecord As New clsNetworkTPBrchRecord(Me.Setup)
                    BRCHRecord.Read(Records(i))
                    Reaches.AddNetworkTPBRCHRecord(BRCHRecord, StructureReachPrefix)
                Next i
            End If

            'leest linkage nodes in uit network.tp
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.TP", "NDLK")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Linkage Nodes...", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim NDLKRecord As New clsNetworkTPNdlkRecord(Me.Setup)
                    NDLKRecord.Read(Records(i))
                    ReachNodes.AddNetworkTPNdlkRecord(NDLKRecord)

                    'a linkage node is not only a reachnode (beginning or end of a reach) but also an object on the reach it attaches to
                    myReach = Reaches.GetReach(NDLKRecord.ci.Trim.ToUpper)
                    myReach.ReachObjects.addNetworkTPNDLKRecord(NDLKRecord)
                Next i
            End If

            'leest de curving points uit network.cp
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.CP", "BRCH")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Vector Points...", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim myRecord As New clsNetworkCPRecord(Me.Setup, myReach)
                    myRecord.Read(Records(i))
                    Reaches.AddNetworkCPRecord(myRecord)
                Next
            End If

            'leest de kunstwerken uit network.st
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.ST", "STRU")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Flow Structures..", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim myRecord As New clsNetworkSTRecord(Me.Setup)
                    myRecord.Read(Records(i))
                    myReach = Reaches.GetReach(myRecord.ci.Trim.ToUpper)
                    myReach.ReachObjects.AddNetworkSTrecord(myRecord)
                Next i
            End If

            'leest de profielen uit network.cr
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.CR", "CRSN")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Flow Cross Sections..", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim myRecord As New clsNetworkCRRecord(Me.Setup)
                    myRecord.Read(Records(i))
                    myReach = Reaches.GetReach(myRecord.ci.Trim.ToUpper)
                    myReach.ReachObjects.AddNetworkCRrecord(myRecord, Setup)
                Next i
            End If

            'leest de meetstations uit network.me
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.ME", "MEAS")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Flow Measurement Stations..", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim myRecord As New clsNetworkMErecord(Me.Setup)
                    myRecord.Read(Records(i))
                    myReach = Reaches.GetReach(myRecord.ci.Trim.ToUpper)
                    myReach.ReachObjects.AddNetworkMERecord(myRecord)
                Next i
            End If

            'lees de laterals uit network.cn
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.CN", "FLBX")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Flow Lateral Nodes..", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim myRecord As New clsNetworkCNFLBXRecord(Me.Setup)
                    myRecord.Read(Records(i))
                    myReach = Reaches.GetReach(myRecord.ci.Trim.ToUpper)
                    myReach.ReachObjects.AddNetworkCNFLBXRecord(myRecord)
                Next i
            End If

            'lees de RRCF-connections uit network.cn
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.CN", "FLBR")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Flow Lateral Conn Nodes..", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim myRecord As New clsNetworkCNFLBRRecord(Me.Setup)
                    myRecord.Read(Records(i))
                    myReach = Reaches.GetReach(myRecord.ci.Trim.ToUpper)
                    myReach.ReachObjects.AddNetworkCNFLBRRecord(myRecord)
                Next i
            End If

            'leest de knooptypes uit network.obi.
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.OBI", "OBID")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Flow Object Types...", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim myRecord As New clsNetworkOBIOBIDRecord(Me.Setup)
                    myRecord.Read(Records(i))
                    NetworkOBIOBIDrecords.Add(myRecord)
                Next i
            End If

            'leest de taktypes uit network.obi.
            Datafile = New clsSobekDataFile(Me.Setup)
            Records = Datafile.Read(path & "\NETWORK.OBI", "BRID")
            If Records.Count > 0 Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Reading Reach Types...", 0, Records.Count)
                For i = 1 To Records.Count
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                    Dim myRecord As New clsNetworkOBIBRIDRecord(Me.Setup)
                    myRecord.Read(Records(i))
                    NetworkOBIBRIDrecords.Add(myRecord)
                Next i
            End If

            'nu we alle objecten hebben ingeladen, gaan we de interne verwijzingen goed zetten
            'maak ook meteen een handzamer tabel aan voor de curving points
            Me.Setup.GeneralFunctions.UpdateProgressBar("Creating new table of curving points ..", 0, Reaches.Reaches.Values.Count)
            i = 0
            Me.Setup.Log.AddDebugMessage("Reaches.Reaches.Values.Count: " & Reaches.Reaches.Values.Count)
            For Each myReach2 As clsSbkReach In Reaches.Reaches.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Reaches.Reaches.Values.Count)
                myReach2.SetReachNodes(Me)     'zet de verwijzingen naar de instanties voor begin en endnode
                myReach2.NetworkcpRecord.CalcCPTable(myReach2.Id, myReach2.bn, myReach2.en)  'berekent een handzamer tabel met buigpunten (hoek en afstand vanaf de punten zelf + X en Y
            Next myReach2

            'met alle interne verwijzingen (bijv. begin- en eindknopen van de takken) op hun plaats
            'kunnen we nu de objecttypen uit de OBI-file halen en toekennen: tak- en knooptypen
            If Not SetObjectTypesFromNetworkOBIRecords() Then Throw New Exception("Error in function SetObjectTypesFromNetworkOBIRecords")

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in sub Import of class clsCFTopology: " & ex.Message)
        End Try

    End Sub

    Friend Function SetObjectTypesFromNetworkOBIRecords() As Boolean
        'this routine sets the sobek node type for all Flow nodes by looking it up in the Network.OBI record
        Try
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                'find the reach type of this reach
                myReach.ReachType.ParentReachType = NetworkOBIBRIDrecords.GetReachType(myReach.Id)
                'find the node types for the begin and end node of this reach
                myReach.bn.nt = NetworkOBIOBIDrecords.GetNodeType(myReach.bn.ID)
                myReach.en.nt = NetworkOBIOBIDrecords.GetNodeType(myReach.en.ID)
                For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    myObj.nt = NetworkOBIOBIDrecords.GetNodeType(myObj.ID)
                Next
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SetObjectTypesFromNetworkOBIRecords")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Function AddReachObject(ByRef myObject As clsSbkReachObject) As Boolean
        Dim myReach As clsSbkReach
        myReach = Reaches.GetReach(myObject.ci)
        If Not myReach Is Nothing Then
            If myObject.lc <= 0 Then
                Me.Setup.Log.AddError("Error adding reach object to SOBEK Schematization: distance on reach <= 0 for " & myObject.ID)
                Return False
            ElseIf myObject.lc >= myReach.getReachLength Then
                Me.Setup.Log.AddError("Error adding reach object to SOBEK Schematization: distance on reach >= reach length for " & myObject.ID)
                Return False
            Else
                Call myReach.ReachObjects.ReachObjects.Add(myObject.ID, myObject)
                Return True
            End If
        Else
            Me.Setup.Log.AddError("Could not find reach " & myObject.ci & " to add object " & myObject.ID & " to.")
            Return False
        End If

    End Function

    Friend Function BuildAndAddReachObject(ID As String, ByRef myReach As clsSbkReach, Chainage As Double, NodeType As GeneralFunctions.enmNodetype) As Boolean
        Try
            Dim myObj As New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myObj.ID = MakeUniqueNodeID(ID)
            myObj.nt = NodeType
            myObj.InUse = True
            myObj.lc = Chainage
            myObj.ci = myReach.Id
            If Not myReach.ReachObjects.ReachObjects.ContainsKey(myObj.ID.Trim.ToUpper) Then
                myReach.ReachObjects.ReachObjects.Add(myObj.ID.Trim.ToUpper, myObj)
            Else
                Throw New Exception("Error: node with ID " & myObj.ID & " was already present in the schematization.")
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' TODO: Siebe invullen
    ''' </summary>
    ''' <remarks></remarks>
    Friend Function Export(ByVal Append As Boolean, ExportDir As String) As Boolean
        Try
            Me.WriteNetwork(Append, ExportDir)
            Me.Setup.Log.AddMessage("Flow topology successfully written.")
            Return True
        Catch ex As Exception
            Dim log As String = "Error in Export topology"
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
            Return False
        End Try

    End Function

    Public Sub exportProfilesToBNA()
        Using bnaWriter As New System.IO.StreamWriter(Me.Setup.Settings.ExportDirRoot & "\profiles.bna", False)
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    If myObj.InUse AndAlso myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE Then
                        If myObj.X = 0 OrElse myObj.Y = 0 Then myObj.calcXY()
                        If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE Then bnaWriter.WriteLine(myObj.BNAString)
                    End If
                Next
            Next
        End Using
    End Sub

    Public Sub exportReachObjectsToBNA(FileNameBase As String, NodeType As STOCHLIB.GeneralFunctions.enmNodetype)
        Using bnaWriter As New System.IO.StreamWriter(Me.Setup.Settings.ExportDirRoot & "\" & FileNameBase & ".bna", False)
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.InUse AndAlso myObj.nt = NodeType Then bnaWriter.WriteLine(myObj.BNAString)
                    Next
                End If
            Next
        End Using
    End Sub

    Public Function exportStructureLocationsToShapefile(ShpPath As String, OnlyIfObservedAvailable As Boolean, LocationsQuery As String) As Boolean
        Try
            Dim qPointSF As New clsPointShapeFile(Me.Setup)
            Dim myShape As MapWinGIS.Shape
            Dim IDIdx As Integer, nReach As Integer = Reaches.Reaches.Count, iReach As Integer
            Dim dt As New DataTable, IDList As New List(Of String)
            If OnlyIfObservedAvailable Then
                Me.Setup.GeneralFunctions.SQLiteQuery(Setup.SqliteCon, LocationsQuery, dt, False)
                For i = 0 To dt.Rows.Count - 1
                    IDList.Add(dt.Rows(i)(0).ToString.Trim.ToUpper)
                Next
            End If

            If System.IO.File.Exists(ShpPath) Then Me.Setup.GeneralFunctions.DeleteShapeFile(ShpPath)
            qPointSF.SF.sf.CreateNew(ShpPath, MapWinGIS.ShpfileType.SHP_POINT)
            qPointSF.SF.sf.StartEditingShapes(True)
            qPointSF.AddField("OBJECTID", MapWinGIS.FieldType.STRING_FIELD, 12, 4, IDIdx)

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing structure locations to shapefile...", iReach, nReach)
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                iReach += 1
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.InUse Then
                            If Setup.GeneralFunctions.isStructure(myObj.nt) Then
                                If Not OnlyIfObservedAvailable OrElse IDList.Contains(myObj.ID.Trim.ToUpper) Then
                                    myObj.calcXY()
                                    myShape = New MapWinGIS.Shape
                                    myShape.Create(MapWinGIS.ShpfileType.SHP_POINT)
                                    myShape.AddPoint(myObj.X, myObj.Y)
                                    qPointSF.SF.sf.EditAddShape(myShape)
                                    qPointSF.SF.sf.EditCellValue(IDIdx, qPointSF.SF.sf.NumShapes - 1, myObj.ID)
                                End If

                            End If
                        End If
                    Next
                End If
            Next
            qPointSF.SF.sf.SaveAs(ShpPath)
            qPointSF.StopEditing(True, True)
            qPointSF.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function exportWaterLevelPointsToShapefile(ShpPath As String, OnlyIfObservedAvailable As Boolean, LocationsQuery As String) As Boolean
        Try
            Dim hPointSF As New clsPointShapeFile(Me.Setup)
            Dim myShape As MapWinGIS.Shape
            Dim IDIdx As Integer, nReach As Integer = Reaches.Reaches.Count, iReach As Integer
            Dim dt As New DataTable, IDList As New List(Of String)
            Dim IDsDone As New List(Of String)
            'dt.Columns.Add("", Type.GetType("System.String"))
            If OnlyIfObservedAvailable Then
                Me.Setup.GeneralFunctions.SQLiteQuery(Setup.SqliteCon, LocationsQuery, dt, False)
                For i = 0 To dt.Rows.Count - 1
                    IDList.Add(dt.Rows(i)(0).ToString.Trim.ToUpper)
                Next
            End If

            If System.IO.File.Exists(ShpPath) Then Me.Setup.GeneralFunctions.DeleteShapeFile(ShpPath)
            hPointSF.SF.sf.CreateNew(ShpPath, MapWinGIS.ShpfileType.SHP_POINT)
            hPointSF.SF.sf.StartEditingShapes(True)
            hPointSF.AddField("OBJECTID", MapWinGIS.FieldType.STRING_FIELD, 12, 4, IDIdx)

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing water level points to shapefile...", iReach, nReach)
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                iReach += 1
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.InUse Then
                            If myObj.isWaterLevelObject Then

                                If Not OnlyIfObservedAvailable OrElse IDList.Contains(myObj.ID.Trim.ToUpper) AndAlso Not IDsDone.Contains(myObj.ID.Trim.ToUpper) Then
                                    IDsDone.Add(myObj.ID.Trim.ToUpper)
                                    myObj.calcXY()
                                    myShape = New MapWinGIS.Shape
                                    myShape.Create(MapWinGIS.ShpfileType.SHP_POINT)
                                    myShape.AddPoint(myObj.X, myObj.Y)
                                    hPointSF.SF.sf.EditAddShape(myShape)
                                    hPointSF.SF.sf.EditCellValue(IDIdx, hPointSF.SF.sf.NumShapes - 1, myObj.ID)
                                End If

                            End If
                        End If
                    Next
                End If
            Next
            hPointSF.SF.sf.SaveAs(ShpPath)
            hPointSF.StopEditing(True, True)
            hPointSF.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



    Public Function WriteNetwork(ByVal Append As Boolean, ExportDir As String) As Boolean
        Try
            Dim myDist As Double, myAngle As Double, i As Integer
            Using tpWriter As New StreamWriter(ExportDir & "\network.tp", Append)
                Using obiwriter As New StreamWriter(ExportDir & "\network.obi", Append)
                    Using crwriter As New StreamWriter(ExportDir & "\network.cr", Append)
                        Using stwriter As New StreamWriter(ExportDir & "\network.st", Append)
                            Using cpwriter As New StreamWriter(ExportDir & "\network.cp", Append)
                                Using cnwriter As New StreamWriter(ExportDir & "\network.cn", Append)
                                    Using mewriter As New StreamWriter(ExportDir & "\network.me", Append)

                                        tpWriter.WriteLine("TP_1.0")
                                        crwriter.WriteLine("CR_1.1")
                                        cpwriter.WriteLine("CP_1.0")
                                        cnwriter.WriteLine("CN_1.1")
                                        mewriter.WriteLine("ME_1.0")
                                        obiwriter.WriteLine("OBI2.0")
                                        stwriter.WriteLine("ST_1.0")

                                        'inventariseer voor elke reachnode of die in gebruik is
                                        For Each myReachNode As clsSbkReachNode In ReachNodes.ReachNodes.Values
                                            myReachNode.InUse = False 'initialiseer op false
                                            For Each myreach As clsSbkReach In Reaches.Reaches.Values
                                                If myreach.InUse AndAlso (myreach.bn.ID.Trim.ToUpper = myReachNode.ID.Trim.ToUpper OrElse myreach.en.ID.Trim.ToUpper = myReachNode.ID.Trim.ToUpper) Then
                                                    myReachNode.InUse = True
                                                    Exit For
                                                End If
                                            Next
                                        Next myReachNode

                                        'schrijf eerst alle begin- en eindknopen die NIET van het type linkage zijn
                                        For Each myreachnode As clsSbkReachNode In ReachNodes.ReachNodes.Values
                                            If myreachnode.InUse Then
                                                Select Case myreachnode.nt
                                                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary
                                                        tpWriter.WriteLine("NODE id '" & myreachnode.ID & "' nm '" & myreachnode.ID & "' px " & myreachnode.X & " py " & myreachnode.Y & " node")
                                                        obiwriter.WriteLine("OBID id '" & myreachnode.ID & "' ci 'SBK_BOUNDARY' obid")
                                                        cnwriter.WriteLine("FLBO id '" & myreachnode.ID & "' ci '" & myreachnode.ID & "' flbo")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFConnectionNode
                                                        tpWriter.WriteLine("NODE id '" & myreachnode.ID & "' nm '" & myreachnode.ID & "' px " & myreachnode.X & " py " & myreachnode.Y & " node")
                                                        obiwriter.WriteLine("OBID id '" & myreachnode.ID & "' ci 'SBK_CHANNELCONNECTION' obid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.ConnNodeLatStor
                                                        tpWriter.WriteLine("NODE id '" & myreachnode.ID & "' nm '" & myreachnode.ID & "' px " & myreachnode.X & " py " & myreachnode.Y & " node")
                                                        obiwriter.WriteLine("OBID id '" & myreachnode.ID & "' ci 'SBK_CHANNEL_STORCONN&LAT' obid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode
                                                        tpWriter.WriteLine("NODE id '" & myreachnode.ID & "' nm '" & myreachnode.ID & "' px " & myreachnode.X & " py " & myreachnode.Y & " node")
                                                        obiwriter.WriteLine("OBID id '" & myreachnode.ID & "' ci 'SBK_SBK-3B-NODE' obid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeSFManhole
                                                        tpWriter.WriteLine("NODE id '" & myreachnode.ID & "' nm '" & myreachnode.ID & "' px " & myreachnode.X & " py " & myreachnode.Y & " node")
                                                        obiwriter.WriteLine("OBID id '" & myreachnode.ID & "' ci 'SBK_CONNECTIONNODE' obid")
                                                    Case Else
                                                        Me.Setup.Log.AddError("Error: node type not recognized for reachnode " & myreachnode.ID)
                                                End Select
                                            End If
                                        Next myreachnode

                                        'Nu alle takobjecten. Alleen wegschrijven als de tak ook daadwerkelijk InUse is
                                        For Each myReach As clsSbkReach In Reaches.Reaches.Values
                                            If myReach.InUse Then
                                                For Each myReachObject As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                                                    If myReachObject.InUse Then
                                                        'final check for valid positioning on reach
                                                        If myReachObject.lc < 0 OrElse myReachObject.lc > myReach.getReachLength Then
                                                            Me.Setup.Log.AddError("Reach object " & myReachObject.ID & " has invalid distance " & myReachObject.lc & " on reach " & myReach.Id & " and could not be created.")
                                                        End If

                                                        Select Case myReachObject.nt
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_PROFILE' obid")
                                                                crwriter.WriteLine("CRSN id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " crsn")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_BRIDGE' obid")
                                                                stwriter.WriteLine("STRU id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " stru")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFCulvert
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_CULVERT' obid")
                                                                stwriter.WriteLine("STRU id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " stru")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "'ci 'SBK_ORIFICE' obid")
                                                                stwriter.WriteLine("STRU id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " stru")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_PUMP' obid")
                                                                stwriter.WriteLine("STRU id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " stru")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_WEIR' obid")
                                                                stwriter.WriteLine("STRU id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " stru")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_UNIWEIR' obid")
                                                                stwriter.WriteLine("STRU id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " stru")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_LATERALFLOW' obid")
                                                                cnwriter.WriteLine("FLBX id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " flbx")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_SBK-3B-REACH' obid")
                                                                cnwriter.WriteLine("FLBX id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " flbx")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.MeasurementStation
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_MEASSTAT' obid")
                                                                mewriter.WriteLine("MEAS id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ObID 'SBK_MEASSTAT' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " meas")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFExtraResistance
                                                                obiwriter.WriteLine("OBID ID '" & myReachObject.ID & "' ci 'SBK_EXTRARESISTANCE' obid")
                                                                stwriter.WriteLine("STRU id '" & myReachObject.ID & "' nm '" & myReachObject.Name & "' ci '" & myReachObject.ci & "' lc " & myReachObject.lc & " stru")
                                                            Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpointFixed
                                                                'we do not write grid points as part of the network yet
                                                            Case Else
                                                                Me.Setup.Log.AddWarning("Exporting SOBEK-objects of type " & myReachObject.nt & " " & myReachObject.ci & " not (yet) supported.")
                                                        End Select
                                                    End If
                                                Next myReachObject
                                            End If
                                        Next myReach

                                        'dan alle takken
                                        For Each myReach As clsSbkReach In Reaches.Reaches.Values
                                            If myReach.InUse Then
                                                Select Case myReach.ReachType.ParentReachType
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachCFChannel
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_CHANNEL' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachCFChannelWithLateral
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_CHANNEL&LAT' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachOFDamBreak
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_DAMBRK' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFPipe
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_PIPE' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFPipeWithRunoff
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_PIPE&RUNOFF' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFDWAPipeWithRunoff
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_DWAPIPE&RUNOFF' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFRWAPipeWithRunoff
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_RWAPIPE&RUNOFF' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFPipeAndComb
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_PIPE&COMB' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFPipeAndMeas
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_PIPE&MEAS' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFInternalWeir
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm 'internal weir' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 500 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_INTWEIR' brid")
                                                        'stwriter.WriteLine("STRU id 'l_" & myReach.Id & "_1' nm 'internal weir_1' ci '" & myReach.Id & "' lc " & myReach.getReachLength / 2 & " stru")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFInternalOrifice
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm 'internal orifice' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 500 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_INTORIFICE' brid")
                                                        'stwriter.WriteLine("STRU id 'l_" & myReach.Id & "_1' nm 'internal orifice_1' ci '" & myReach.Id & "' lc " & myReach.getReachLength / 2 & " stru")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFInternalCulvert
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm 'internal culvert' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 500 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_INTCULVERT' brid")
                                                        'stwriter.WriteLine("STRU id 'l_" & myReach.Id & "_1' nm 'internal culvert_1' ci '" & myReach.Id & "' lc " & myReach.getReachLength / 2 & " stru")
                                                        'crwriter.WriteLine("CRSN id 'l_" & myReach.Id & "_1' nm 'internal culvert_1' ci '" & myReach.Id & "' lc " & myReach.getReachLength / 2 & " crsn")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachSFInternalPump
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm 'internal pump' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 500 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_INTPUMP' brid")
                                                        'stwriter.WriteLine("STRU id 'l_" & myReach.Id & "_1' nm 'internal pump_1' ci '" & myReach.Id & "' lc " & myReach.getReachLength / 2 & " stru")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachOFLineBoundary
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'FLS_LINEBOUNDARY' brid")
                                                    Case Is = STOCHLIB.GeneralFunctions.enmReachtype.ReachOFLine1D2DBoundary
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'FLS_LINE1D2DBOUNDARY' brid")
                                                    Case Else
                                                        tpWriter.WriteLine("BRCH id '" & myReach.Id & "' nm '' bn '" & myReach.bn.ID & "' en '" & myReach.en.ID & "' al " & myReach.getReachLength & " vc_opt 0 vc_equi -1 vc_len 200 brch")
                                                        obiwriter.WriteLine("BRID id '" & myReach.Id & "' ci 'SBK_CHANNEL' brid")
                                                        Me.Setup.Log.AddWarning("Reach: " & myReach.Id & ": had no valid reach type assigned. Default channel type was applied.")
                                                End Select

                                                cpwriter.WriteLine("BRCH id '" & myReach.Id & "' cp 1 ct bc ")
                                                cpwriter.WriteLine("TBLE")
                                                For i = 0 To myReach.NetworkcpRecord.Table.XValues.Count - 1
                                                    myDist = myReach.NetworkcpRecord.Table.XValues.Values(i)
                                                    myAngle = myReach.NetworkcpRecord.Table.Values1.Values(i)
                                                    cpwriter.WriteLine(myDist & " " & myAngle & " <")
                                                Next i

                                                cpwriter.WriteLine("tble brch")
                                                cpwriter.WriteLine("")

                                            End If
                                        Next myReach

                                        'en tenslotte alle linkage nodes
                                        'exporteer deze ook in een BNA-file
                                        For Each myReachNode As clsSbkReachNode In ReachNodes.ReachNodes.Values
                                            If myReachNode.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLinkage AndAlso myReachNode.InUse Then
                                                tpWriter.WriteLine("NDLK id '" & myReachNode.ID & "' nm '" & myReachNode.ID & "' px " & myReachNode.X & " py " & myReachNode.Y & " ci '" & myReachNode.ci & "' lc " & myReachNode.lc & " ndlk")
                                                obiwriter.WriteLine("OBID ID '" & myReachNode.ID & "' ci 'SBK_CHANNELLINKAGENODE' obid")
                                            End If
                                        Next myReachNode

                                        mewriter.Close()
                                        cnwriter.Close()
                                        cpwriter.Close()
                                        stwriter.Close()
                                        crwriter.Close()
                                        obiwriter.Close()
                                        tpWriter.Close()
                                    End Using
                                End Using
                            End Using
                        End Using
                    End Using
                End Using
            End Using
            Me.Setup.Log.AddMessage("Flow topology successfully written.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in Function WriteNetwork Of class clsCFTopology")
            Return False
        End Try

    End Function

    Public Function GetInsertReachNode(ByVal SearchFromPoint As clsXYZ, ByVal CatchmentID As String, ByVal PreferReachInsidePolygon As Boolean, ByVal PreferReachInsideCatchment As Boolean, ByVal PreferReachInsideAreaShapeFile As Boolean, Optional ByVal UpStreamSearchRadius As Double = 100, Optional ByVal DownStreamSearchRadius As Double = 100, Optional ByVal ExcludeReachesWithPrefix As List(Of String) = Nothing) As clsSbkReachNode
        'based on a starting point, this routine searches for a snap location
        'then, within a search radius it will try to find a connection node, returning it
        'if not found, it will split the reach into two parts, inserting a connectionnode and returning that
        'v1.794: added the list of do-not-snap-to reach prefixes to this routine
        Dim Found As Boolean = False
        Dim Log As String = ""
        Dim SnapReach As clsSbkReach = Nothing
        Dim SnapChainage As Double
        Dim SnapDistance As Double
        Dim myNode As clsSbkReachNode = Nothing
        Dim upReach As clsSbkReach = Nothing, dnReach As clsSbkReach = Nothing
        Dim myBedLevel As Double

        'exception handling introduced in v2.0. If all goes well, a minimum value of 1 m has already been enforced in the initialization
        If UpStreamSearchRadius <= 0 Then Throw New Exception("Invalid upstream search radius of " & UpStreamSearchRadius & " specified for reach snapping.")
        If DownStreamSearchRadius <= 0 Then Throw New Exception("Invalid downstream search radius of " & DownStreamSearchRadius & " specified for reach snapping.")

        Try
            'stap 1: zoek de optimale snapping location
            If Not Reaches.FindSnapLocation(SearchFromPoint.X, SearchFromPoint.Y, 9.0E+99, SnapReach, SnapChainage, SnapDistance, True,, ExcludeReachesWithPrefix) Then Throw New Exception("Error finding snap location.")

            'stap 2: zoek of binnen de opgegeven zoektstraal al een bestaande connection node ligt. Zo niet, splits de tak op
            If SnapChainage <= UpStreamSearchRadius Then
                SnapReach.getBedLevel(0, SnapReach.bn.BedLevel) 'retrieve the bed level for the node we found. we might need it later to create profile data
                'SnapReach.bn.Rename(SearchFromPoint.ID) v1.77 renaming was unsuccessful for this case since the newly created nodes did not have a new reach attached yet.
                Return SnapReach.bn
            ElseIf SnapChainage >= SnapReach.getReachLength - DownStreamSearchRadius Then
                SnapReach.getBedLevel(SnapReach.getReachLength, SnapReach.en.BedLevel)  'retrieve the bed level for the node found. we might need it later to create profile data
                'SnapReach.en.Rename(SearchFromPoint.ID) v1.77 renaming was unsuccessful for this case since the newly created nodes did not have a new reach attached yet.
                Return SnapReach.en
            Else
                If Not SnapReach.getBedLevel(SnapChainage, myBedLevel) Then Me.Setup.Log.AddWarning("Could not retrieve bedlevel from backbone model for object " & SearchFromPoint.ID & " in catchment " & CatchmentID) 'retrieve the bed level for the node we found. we might need it later to create profile data
                myNode = SplitReachAtChainage(SnapReach, SnapChainage, upReach, dnReach, True, SearchFromPoint.ID)
                myNode.BedLevel = myBedLevel
            End If
            Return myNode

        Catch ex As Exception
            Log = "Error in function getInsertReachNode of class clsCFTopology"
            Me.Setup.Log.AddError(Log + ": " + ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function SplitReachAtReachObject(ByRef myNode As clsSbkReachObject, Optional ByVal KeepUpstreamReach As Boolean = True, Optional ByVal KeepDownstreamReach As Boolean = True) As clsSbkReachNode

        'Splits an existing sobek reach on an existing RR-CF connection
        Dim NewReachUp As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim NewReachDown As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim NewNodeRecord As New clsNetworkTPNodeRecord(Me.Setup)
        Dim NodesDatRecord As clsNodesDatNODERecord
        Dim NewNode As clsSbkReachNode

        Try

            'find the reach on which this node is located
            Dim myReach As clsSbkReach = GetReachByNodeID(myNode.ID)
            myNode.calcXY() 'before removing the original reach, calculate the XY-coordinate of our object

            'set inuse parameters
            Me.Reaches.Reaches.Remove(myReach.Id.Trim.ToUpper)   'remove the original reach from the dictionary of reaches
            If KeepUpstreamReach Then NewReachUp.InUse = True
            If KeepDownstreamReach Then NewReachDown.InUse = True

            'change the node type into a connection node and remove its expression as reachobject from the reach
            NewNode = New clsSbkReachNode(Me.Setup, SbkCase)
            NewNode.nt = GeneralFunctions.enmNodetype.NodeRRCFConnectionNode
            NewNode.ID = myNode.ID
            NewNode.X = myNode.X
            NewNode.Y = myNode.Y
            NewNode.Name = myNode.Name
            ReachNodes.ReachNodes.Add(NewNode.ID.Trim.ToUpper, NewNode)

            'definde the vector points for the new reaches
            NewReachUp.NetworkcpRecord.GetPartFromReach(NewReachUp.bn, NewReachUp.en, myReach, 0, myNode.lc)
            NewReachDown.NetworkcpRecord.GetPartFromReach(NewReachDown.bn, NewReachDown.en, myReach, myNode.lc, 9999999999)
            myReach.ReachObjects.ReachObjects.Remove(myNode.ID.Trim.ToUpper)

            NewReachUp.bn = myReach.bn               'beginknoop bovenstroomse tak gelijk aan die van oorspronkelijke tak
            NewReachUp.en = NewNode                  'eindknoop van bovenstroomse tak wordt de nieuwe knoop
            NewReachDown.bn = NewNode                'beginknoop van benedenstroomse tak wordt de nieuwe knoop
            NewReachDown.en = myReach.en             'eindknoop van de benedenstroomse tak is gelijk aan die van de oorspronkelijke

            If KeepUpstreamReach AndAlso KeepDownstreamReach Then
                NewReachUp.Id = Reaches.CreateID(myReach.Id)
                If Not Me.Reaches.Reaches.ContainsKey(NewReachUp.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReachUp.Id.Trim.ToUpper, NewReachUp)
                NewReachDown.Id = Reaches.CreateID(myReach.Id)
                If Not Me.Reaches.Reaches.ContainsKey(NewReachDown.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReachDown.Id.Trim.ToUpper, NewReachDown)
            ElseIf KeepUpstreamReach Then
                NewReachUp.Id = myReach.Id
                If Not Me.Reaches.Reaches.ContainsKey(NewReachUp.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReachUp.Id.Trim.ToUpper, NewReachUp)
            ElseIf KeepDownstreamReach Then
                NewReachDown.Id = myReach.Id
                If Not Me.Reaches.Reaches.ContainsKey(NewReachDown.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReachDown.Id.Trim.ToUpper, NewReachDown)
            End If

            'move all reach objects and delete the original reach
            If KeepUpstreamReach Then MoveAllReachObjects(myReach, NewReachUp, 0, myNode.lc) 'verplaats alle objecten op de tak naar de nieuwe tak!
            If KeepDownstreamReach Then MoveAllReachObjects(myReach, NewReachDown, myNode.lc, 999999999) 'verplaats alle objecten op de tak naar de nieuwe tak!

            'create a nodes.dat record for interpolation over the newly created reaches
            If KeepUpstreamReach AndAlso KeepDownstreamReach Then
                If Not SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(NewNode.ID) Then
                    NodesDatRecord = New clsNodesDatNODERecord(Me.Setup)
                    NodesDatRecord.ID = NewNode.ID
                    NodesDatRecord.ty = 0
                    NodesDatRecord.ni = 1
                    NodesDatRecord.r1 = NewReachUp.Id
                    NodesDatRecord.r2 = NewReachDown.Id
                    SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Add(NodesDatRecord.ID.Trim.ToUpper, NodesDatRecord)
                Else
                    NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(NewNode.ID.Trim.ToUpper)
                    NodesDatRecord.ni = 1
                    NodesDatRecord.r1 = NewReachUp.Id
                    NodesDatRecord.r2 = NewReachDown.Id
                End If
            End If

            'also adjust the nodesdatrecords of any surrounding nodes
            If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.bn.ID.Trim.ToUpper) Then
                NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.bn.ID.Trim.ToUpper)
                If NodesDatRecord.ni = 1 Then
                    If NodesDatRecord.r1 = myReach.Id Then NodesDatRecord.r1 = NewReachUp.Id
                    If NodesDatRecord.r2 = myReach.Id Then NodesDatRecord.r2 = NewReachUp.Id
                End If
            End If

            If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.en.ID.Trim.ToUpper) Then
                NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.en.ID.Trim.ToUpper)
                If NodesDatRecord.ni = 1 Then
                    If NodesDatRecord.r1 = myReach.Id Then NodesDatRecord.r1 = NewReachDown.Id
                    If NodesDatRecord.r2 = myReach.Id Then NodesDatRecord.r2 = NewReachDown.Id
                End If
            End If

            Return NewNode
        Catch ex As Exception
            Me.Setup.Log.AddError("Error splitting reach at snap location: " & myNode.ID)
            Return Nothing
        End Try

    End Function


    Public Function SplitReachAtLinkageNode(ByRef myReach As clsSbkReach, ByRef myLinkage As clsSbkReachNode, Optional ByVal KeepUpstreamReach As Boolean = True, Optional ByVal KeepDownstreamReach As Boolean = True) As clsSbkReachNode

        'Splits an existing sobek reach on an existing Linkage Node
        Dim NewReachUp As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim NewReachDown As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim NewNodeRecord As New clsNetworkTPNodeRecord(Me.Setup)
        Dim NodesDatRecord As clsNodesDatNODERecord

        Try

            Dim myLinkageConnection As clsSbkReachObject = myReach.ReachObjects.ReachObjects.Item(myLinkage.ID.Trim.ToUpper)

            'set inuse parameters
            Me.Reaches.Reaches.Remove(myReach.Id.Trim.ToUpper)   'remove the original reach from the dictionary of reaches
            If KeepUpstreamReach Then NewReachUp.InUse = True
            If KeepDownstreamReach Then NewReachDown.InUse = True

            'change the node type to a connection node and remove its expression as reachobject from the reach
            myLinkage.nt = GeneralFunctions.enmNodetype.NodeCFConnectionNode
            myReach.ReachObjects.ReachObjects.Remove(myLinkage.ID.Trim.ToUpper)
            NewReachUp.bn = myReach.bn               'beginknoop bovenstroomse tak gelijk aan die van oorspronkelijke tak
            NewReachUp.en = myLinkage                'eindknoop van bovenstroomse tak wordt de nieuwe knoop
            NewReachDown.bn = myLinkage              'beginknoop van benedenstroomse tak wordt de nieuwe knoop
            NewReachDown.en = myReach.en             'eindknoop van de benedenstroomse tak is gelijk aan die van de oorspronkelijke

            'definde the vector points for the new reaches
            NewReachUp.NetworkcpRecord.GetPartFromReach(NewReachUp.bn, NewReachUp.en, myReach, 0, myLinkageConnection.lc)
            NewReachDown.NetworkcpRecord.GetPartFromReach(NewReachDown.bn, NewReachDown.en, myReach, myLinkageConnection.lc, 9999999999)

            If KeepUpstreamReach AndAlso KeepDownstreamReach Then
                NewReachUp.Id = Reaches.CreateID(myReach.Id)
                If Not Me.Reaches.Reaches.ContainsKey(NewReachUp.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReachUp.Id.Trim.ToUpper, NewReachUp)
                NewReachDown.Id = Reaches.CreateID(myReach.Id)
                If Not Me.Reaches.Reaches.ContainsKey(NewReachDown.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReachDown.Id.Trim.ToUpper, NewReachDown)
            ElseIf KeepUpstreamReach Then
                NewReachUp.Id = myReach.Id
                If Not Me.Reaches.Reaches.ContainsKey(NewReachUp.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReachUp.Id.Trim.ToUpper, NewReachUp)
            ElseIf KeepDownstreamReach Then
                NewReachDown.Id = myReach.Id
                If Not Me.Reaches.Reaches.ContainsKey(NewReachDown.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReachDown.Id.Trim.ToUpper, NewReachDown)
            End If

            'move all reach objects and delete the original reach
            If KeepUpstreamReach Then MoveAllReachObjects(myReach, NewReachUp, 0, myLinkageConnection.lc) 'verplaats alle objecten op de tak naar de nieuwe tak!
            If KeepDownstreamReach Then MoveAllReachObjects(myReach, NewReachDown, myLinkageConnection.lc, 999999999) 'verplaats alle objecten op de tak naar de nieuwe tak!

            'create a nodes.dat record for interpolation over the newly created reaches
            If KeepUpstreamReach AndAlso KeepDownstreamReach Then
                If Not SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myLinkage.ID) Then
                    NodesDatRecord = New clsNodesDatNODERecord(Me.Setup)
                    NodesDatRecord.ID = myLinkage.ID
                    NodesDatRecord.ty = 0
                    NodesDatRecord.ni = 1
                    NodesDatRecord.r1 = NewReachUp.Id
                    NodesDatRecord.r2 = NewReachDown.Id
                    SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Add(NodesDatRecord.ID.Trim.ToUpper, NodesDatRecord)
                Else
                    NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myLinkage.ID.Trim.ToUpper)
                    NodesDatRecord.ni = 1
                    NodesDatRecord.r1 = NewReachUp.Id
                    NodesDatRecord.r2 = NewReachDown.Id
                End If
            End If

            'also adjust the nodesdatrecords of any surrounding nodes
            If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.bn.ID.Trim.ToUpper) Then
                NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.bn.ID.Trim.ToUpper)
                If NodesDatRecord.ni = 1 Then
                    If NodesDatRecord.r1 = myReach.Id Then NodesDatRecord.r1 = NewReachUp.Id
                    If NodesDatRecord.r2 = myReach.Id Then NodesDatRecord.r2 = NewReachUp.Id
                End If
            End If

            If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.en.ID.Trim.ToUpper) Then
                NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.en.ID.Trim.ToUpper)
                If NodesDatRecord.ni = 1 Then
                    If NodesDatRecord.r1 = myReach.Id Then NodesDatRecord.r1 = NewReachDown.Id
                    If NodesDatRecord.r2 = myReach.Id Then NodesDatRecord.r2 = NewReachDown.Id
                End If
            End If

            Return myLinkage
        Catch ex As Exception
            Me.Setup.Log.AddError("Error splitting reach at snap location: " & myLinkage.ID)
            Return Nothing
        End Try

    End Function

    Public Function SplitReachWithExistingReachNode(ByRef myNode As clsSbkReachNode, ByRef myReach As clsSbkReach, Chainage As Double) As Boolean

        'this function splits an existing reach on a given chainage. 
        'This means not just splitting at vector points, but also inbetween! (awesome, right?)
        Try
            Dim upReach As New clsSbkReach(Me.Setup, Me.SbkCase)
            Dim downReach As New clsSbkReach(Me.Setup, Me.SbkCase)
            Dim myxMin As Double, myyMin As Double, myxMax As Double, myyMax As Double

            upReach = myReach.Clone("u")
            upReach.TruncateDownstreamPart(Chainage, myNode)
            'upReach.Id = Reaches.CreateID(myReach.Id)
            upReach.ReachType.ParentReachType = GeneralFunctions.enmReachtype.ReachCFChannel
            upReach.bn = myReach.bn
            upReach.en = myNode
            Me.MoveAllReachObjects(myReach, upReach, 0, Chainage)   'verplaats alle objecten op de tak naar de nieuwe tak!
            Reaches.Reaches.Add(upReach.Id.Trim.ToUpper, upReach)

            'add the new reach to the tiles collection
            upReach.getextent(myxMin, myyMin, myxMax, myyMax)
            ReachTiles.AddAreaObject(upReach, myxMin, myyMin, myxMax, myyMax)

            downReach = myReach.Clone("d")
            downReach.TruncateUpstreamPart(Chainage, myNode)
            'downReach.Id = Reaches.CreateID(myReach.Id)
            downReach.ReachType.ParentReachType = GeneralFunctions.enmReachtype.ReachCFChannel
            downReach.en = myReach.en
            downReach.bn = myNode
            Me.MoveAllReachObjects(myReach, downReach, Chainage, myReach.getReachLength)   'verplaats alle objecten op de tak naar de nieuwe tak!
            Reaches.Reaches.Add(downReach.Id.Trim.ToUpper, downReach)

            'add the new reach to the tiles collection
            downReach.getextent(myxMin, myyMin, myxMax, myyMax)
            ReachTiles.AddAreaObject(downReach, myxMin, myyMin, myxMax, myyMax)

            'now that this snapping point has been handled, reset the usage of objects
            myReach.InUse = False
            upReach.InUse = True
            downReach.InUse = True
            myNode.SnapReachID = ""
            myNode.SnapChainage = 0
            myNode.SnapDistance = 0

            'create a new nodes.dat record to interpolate cross sections between the two original reaches
            Dim nodesDat As New clsNodesDatNODERecord(Me.Setup)
            nodesDat.ID = myNode.ID
            nodesDat.InUse = True
            nodesDat.ty = 0
            nodesDat.ni = 1
            nodesDat.r1 = upReach.Id
            nodesDat.r2 = downReach.Id
            If Not SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(nodesDat.ID.Trim.ToUpper) Then SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Add(nodesDat.ID.Trim.ToUpper, nodesDat)

            'also update the nodes.dat references in the original upstream and downstream nodes
            SbkCase.CFData.Data.NodesData.UpdateInterpolationReferencesAfterSplit(myReach, upReach, downReach)

            'finish by updating all other references to the original reach
            'EDIT 7-7-2017: the routines below were too slow and all they did was update the snapchainage for iNode. 
            'If Not UpdateSplitReachReferences(myReach, 0, Chainage, upReach) Then Me.Setup.Log.AddError("Error adjusting reach references after splitting: " & myReach.Id)
            'If Not UpdateSplitReachReferences(myReach, Chainage, myReach.getReachLength, downReach) Then Me.Setup.Log.AddError("Error adjusting reach references after splitting: " & myReach.Id)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try



    End Function

    Public Function UpdateSplitReachReferences(ByRef myReach As clsSbkReach, startlc As Double, endlc As Double, ByRef newReach As clsSbkReach) As Boolean
        Try
            'this routine updates all references to reaches that have just been split
            'this means that it also corrects for the adjusted chainages

            'start with splitting all snap-references for ReachNodes
            Dim iNode As clsSbkReachNode
            For i = 0 To ReachNodes.ReachNodes.Values.Count - 1
                iNode = ReachNodes.ReachNodes.Values(i)
                If Not iNode.SnapReachID = "" Then
                    If iNode.SnapReachID = myReach.Id AndAlso iNode.SnapChainage >= startlc AndAlso iNode.SnapChainage <= endlc Then
                        iNode.SnapReachID = newReach.Id
                        iNode.SnapChainage = iNode.SnapChainage - startlc 'correct for the fact that the new reach is shorter than the original
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function SplitReachAtVectorPoint(ByRef myReach As clsSbkReach, ByRef myVectorPoint As clsSbkReachObject) As clsSbkReachNode

        'Splits an existing sobek reach at a specified location on a reach.
        Dim NewReach1 As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim NewReach2 As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim NewNode As New clsSbkReachNode(Me.Setup, Me.SbkCase)
        Dim NewNodeRecord As New clsNetworkTPNodeRecord(Me.Setup)
        Dim NodesDatRecord As clsNodesDatNODERecord

        Dim otherReach As clsSbkReach

        Try

            myReach.InUse = False                                   'set the parameter InUse for the 'old' reach to FALSE

            'create a unique ID for the new connection node and set its properties
            NewNode.ID = MakeUniqueNodeID(myVectorPoint.ID)
            NewNode.X = myVectorPoint.X 'x-coordinaat van de koppelknoop is gelijk aan de gevonden snaplocatie
            NewNode.Y = myVectorPoint.Y  'y-coordinaat van de koppelknoop is gelijk aan de gevonden snaplocatie
            NewNode.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFConnectionNode
            NewNode.InUse = True

            'add the new node to the network
            Me.ReachNodes.ReachNodes.Add(NewNode.ID.Trim.ToUpper, NewNode)

            'make sure that any other reaches that might have started or ended at the split location are now redirected to the newly created connection node
            For Each otherReach In Reaches.Reaches.Values
                If otherReach.en.ID.Trim.ToUpper = NewNode.ID.Trim.ToUpper Then
                    otherReach.en = NewNode
                ElseIf otherReach.bn.ID.Trim.ToUpper = NewNode.ID.Trim.ToUpper Then
                    otherReach.bn = NewNode
                End If
            Next

            'schrijf de begin- en eindknoop van beide nieuwe takken weg
            NewReach1.bn = myReach.bn               'beginknoop bovenstroomse tak gelijk aan die van oorspronkelijke tak
            NewReach1.en = NewNode                  'eindknoop van bovenstroomse tak wordt de nieuwe knoop
            NewReach1.Id = Reaches.CreateID(myReach.Id)       'creëer een nieuw ID voor de bovenstroomse tak

            NewReach1.InUse = True
            NewReach1.NetworkcpRecord.GetPartFromReach(NewReach1.bn, NewReach1.en, myReach, 0, myVectorPoint.lc)
            If Not Me.Reaches.Reaches.ContainsKey(NewReach1.Id.Trim.ToUpper) Then
                Me.Reaches.Reaches.Add(NewReach1.Id.Trim.ToUpper, NewReach1)
            End If
            Me.MoveAllReachObjects(myReach, NewReach1, 0, myVectorPoint.lc)   'verplaats alle objecten op de tak naar de nieuwe tak!

            NewReach2.bn = NewNode                  'beginknoop van benedenstroomse tak wordt de nieuwe knoop
            NewReach2.en = myReach.en               'eindknoop van de benedenstroomse tak is gelijk aan die van de oorspronkelijke
            NewReach2.Id = Reaches.CreateID(myReach.Id)       'creëer een nieuw ID voor de benedenstroomse tak

            NewReach2.InUse = True
            NewReach2.NetworkcpRecord.GetPartFromReach(NewReach2.bn, NewReach2.en, myReach, myVectorPoint.lc, 9999999999)
            If Not Me.Reaches.Reaches.ContainsKey(NewReach2.Id.Trim.ToUpper) Then
                Me.Reaches.Reaches.Add(NewReach2.Id.Trim.ToUpper, NewReach2)
            End If
            Me.MoveAllReachObjects(myReach, NewReach2, myVectorPoint.lc, 999999999) 'verplaats alle objecten op de tak naar de nieuwe tak!

            'create a nodes.dat record for interpolation over the newly created reaches
            NodesDatRecord = New clsNodesDatNODERecord(Me.Setup)
            NodesDatRecord.ID = NewNode.ID
            NodesDatRecord.ty = 0
            NodesDatRecord.ni = 1
            NodesDatRecord.r1 = NewReach1.Id
            NodesDatRecord.r2 = NewReach2.Id
            SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Add(NodesDatRecord.ID.Trim.ToUpper, NodesDatRecord)

            'also adjust the nodesdatrecords of any surrounding nodes
            If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.bn.ID.Trim.ToUpper) Then
                NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.bn.ID.Trim.ToUpper)
                If NodesDatRecord.ni = 1 Then
                    If NodesDatRecord.r1 = myReach.Id Then NodesDatRecord.r1 = NewReach1.Id
                    If NodesDatRecord.r2 = myReach.Id Then NodesDatRecord.r2 = NewReach1.Id
                End If
            End If

            If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.en.ID.Trim.ToUpper) Then
                NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.en.ID.Trim.ToUpper)
                If NodesDatRecord.ni = 1 Then
                    If NodesDatRecord.r1 = myReach.Id Then NodesDatRecord.r1 = NewReach2.Id
                    If NodesDatRecord.r2 = myReach.Id Then NodesDatRecord.r2 = NewReach2.Id
                End If
            End If

            'finally remove the original reach
            Me.Reaches.Reaches.Remove(myReach.Id.Trim.ToUpper)

            Return NewNode
        Catch ex As Exception
            Me.Setup.Log.AddError("Error splitting reach at vector point: " & myVectorPoint.ID)
            Return Nothing
        End Try

    End Function

    Public Function SplitReachAtChainage(ByRef myReach As clsSbkReach, ByRef myChainage As Double, ByRef newReach1 As clsSbkReach, ByRef NewReach2 As clsSbkReach, Optional ByVal ApplyNodeInterpolation As Boolean = True, Optional ByVal NodeID As String = "", Optional ByVal newReach1ID As String = "", Optional ByVal newReach2ID As String = "") As clsSbkReachNode

        'Splits an existing sobek reach at a specified location on a reach.
        newReach1 = New clsSbkReach(Me.Setup, Me.SbkCase)
        NewReach2 = New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim NewNode As New clsSbkReachNode(Me.Setup, Me.SbkCase)
        Dim NewNodeRecord As New clsNetworkTPNodeRecord(Me.Setup)
        Dim NodesDatRecord As clsNodesDatNODERecord
        Dim myDat As clsFrictionDatBDFRRecord
        Dim newDat As clsFrictionDatBDFRRecord
        Dim myInit As clsInitialDatFLINRecord
        Dim newInit As clsInitialDatFLINRecord

        Try
            myReach.InUse = False                                   'set the parameter InUse for the 'old' reach to FALSE

            'create a unique ID for the new connection node and set its properties
            If NodeID = "" Then
                NewNode.ID = MakeUniqueNodeID(myReach.Id)
            Else
                NewNode.ID = MakeUniqueNodeID(NodeID)
            End If

            myReach.calcXY(myChainage, NewNode.X, NewNode.Y) 'x-coordinaat van de koppelknoop is gelijk aan de gevonden snaplocatie
            NewNode.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFConnectionNode
            NewNode.InUse = True

            'add the new node to the network
            Me.ReachNodes.ReachNodes.Add(NewNode.ID.Trim.ToUpper, NewNode)

            'write the start- and end node for the new upstream reach
            newReach1.bn = myReach.bn               'beginknoop bovenstroomse tak gelijk aan die van oorspronkelijke tak
            newReach1.en = NewNode                  'eindknoop van bovenstroomse tak wordt de nieuwe knoop
            If newReach1ID <> "" Then
                newReach1.Id = newReach1ID       'ging fout bij een tweede split op dezelfde tak! Onduidelijk waarom.
            Else
                newReach1.Id = Reaches.CreateID(myReach.Id)       'ging fout bij een tweede split op dezelfde tak! Onduidelijk waarom.
            End If
            newReach1.ReachType.ParentReachType = myReach.ReachType.ParentReachType
            newReach1.CopyMetaDataFromReach(myReach)           'include all meta data, if present

            newReach1.InUse = True
            newReach1.NetworkcpRecord.GetPartFromReach(newReach1.bn, newReach1.en, myReach, 0, myChainage)
            If Not Reaches.Reaches.ContainsKey(newReach1.Id.Trim.ToUpper) Then
                Reaches.Reaches.Add(newReach1.Id.Trim.ToUpper, newReach1)
            End If
            'siebe
            Me.MoveAllReachObjects(myReach, newReach1, 0, myChainage)   'verplaats alle objecten op de tak naar de nieuwe tak!

            NewReach2.bn = NewNode                  'beginknoop van benedenstroomse tak wordt de nieuwe knoop
            NewReach2.en = myReach.en               'eindknoop van de benedenstroomse tak is gelijk aan die van de oorspronkelijke
            If newReach2ID <> "" Then
                NewReach2.Id = newReach2ID
            Else
                NewReach2.Id = Reaches.CreateID(myReach.Id)
            End If
            NewReach2.Id = Reaches.CreateID(myReach.Id)       'ging fout!
            NewReach2.ReachType.ParentReachType = myReach.ReachType.ParentReachType
            NewReach2.CopyMetaDataFromReach(myReach)           'include all meta data, if present

            NewReach2.InUse = True
            NewReach2.NetworkcpRecord.GetPartFromReach(NewReach2.bn, NewReach2.en, myReach, myChainage, 9999999999)
            If Not Reaches.Reaches.ContainsKey(NewReach2.Id.Trim.ToUpper) Then
                Reaches.Reaches.Add(NewReach2.Id.Trim.ToUpper, NewReach2)
            End If
            Me.MoveAllReachObjects(myReach, NewReach2, myChainage, 999999999) 'verplaats alle objecten op de tak naar de nieuwe tak!

            If ApplyNodeInterpolation Then
                'create a nodes.dat record for interpolation of cross sections over the newly created reaches
                NodesDatRecord = New clsNodesDatNODERecord(Me.Setup)
                NodesDatRecord.ID = NewNode.ID
                NodesDatRecord.ty = 0
                NodesDatRecord.ni = 1
                NodesDatRecord.r1 = newReach1.Id
                NodesDatRecord.r2 = NewReach2.Id
                SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Add(NodesDatRecord.ID.Trim.ToUpper, NodesDatRecord)
            End If

            'also adjust the nodesdatrecords of any surrounding nodes
            If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.bn.ID.Trim.ToUpper) Then
                NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.bn.ID.Trim.ToUpper)
                If NodesDatRecord.ni = 1 Then
                    If NodesDatRecord.r1 = myReach.Id Then NodesDatRecord.r1 = newReach1.Id
                    If NodesDatRecord.r2 = myReach.Id Then NodesDatRecord.r2 = newReach1.Id
                End If
            End If

            If SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myReach.en.ID.Trim.ToUpper) Then
                NodesDatRecord = SbkCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Item(myReach.en.ID.Trim.ToUpper)
                If NodesDatRecord.ni = 1 Then
                    If NodesDatRecord.r1 = myReach.Id Then NodesDatRecord.r1 = NewReach2.Id
                    If NodesDatRecord.r2 = myReach.Id Then NodesDatRecord.r2 = NewReach2.Id
                End If
            End If

            'if a friction record is present, it also needs to be split
            If SbkCase.CFData.Data.FrictionData.FrictionDatBDFRRecords.records.ContainsKey(myReach.Id.Trim.ToUpper) Then
                myDat = SbkCase.CFData.Data.FrictionData.FrictionDatBDFRRecords.records.Item(myReach.Id.Trim.ToUpper)

                newDat = New clsFrictionDatBDFRRecord(Me.Setup)
                newDat = myDat.Clone(newReach1.Id)
                If Not SbkCase.CFData.Data.FrictionData.FrictionDatBDFRRecords.records.ContainsKey(newDat.ID.Trim.ToUpper) Then
                    SbkCase.CFData.Data.FrictionData.FrictionDatBDFRRecords.records.Add(newDat.ID.Trim.ToUpper, newDat)
                End If

                newDat = New clsFrictionDatBDFRRecord(Me.Setup)
                newDat = myDat.Clone(NewReach2.Id)
                If Not SbkCase.CFData.Data.FrictionData.FrictionDatBDFRRecords.records.ContainsKey(newDat.ID.Trim.ToUpper) Then
                    SbkCase.CFData.Data.FrictionData.FrictionDatBDFRRecords.records.Add(newDat.ID.Trim.ToUpper, newDat)
                End If

                myDat.InUse = False
            End If

            'if an initial.dat record is present, it also needs to be split
            If SbkCase.CFData.Data.InitialData.InitialDatFLINRecords.records.ContainsKey(myReach.Id.Trim.ToUpper) Then
                myInit = SbkCase.CFData.Data.InitialData.InitialDatFLINRecords.records.Item(myReach.Id.Trim.ToUpper)

                newInit = New clsInitialDatFLINRecord(Me.Setup)
                newInit = myInit.Clone(newReach1.Id)
                newInit.ID = newReach1.Id
                newInit.ci = newReach1.Id
                If Not SbkCase.CFData.Data.InitialData.InitialDatFLINRecords.records.ContainsKey(newInit.ID.Trim.ToUpper) Then
                    SbkCase.CFData.Data.InitialData.InitialDatFLINRecords.records.Add(newInit.ID.Trim.ToUpper, newInit)
                End If

                newInit = New clsInitialDatFLINRecord(Me.Setup)
                newInit = myInit.Clone(NewReach2.Id)
                newInit.ID = NewReach2.Id
                newInit.ci = NewReach2.Id
                If Not SbkCase.CFData.Data.InitialData.InitialDatFLINRecords.records.ContainsKey(newInit.ID.Trim.ToUpper) Then
                    SbkCase.CFData.Data.InitialData.InitialDatFLINRecords.records.Add(newInit.ID.Trim.ToUpper, newInit)
                End If

            End If

            '--------------------------------------------------------------------------------------------------
            'siebe 1 juni 2018
            'versie 37
            'het verwijderen uit de collectie vervangen door inuse=true teneinde de indexering intact te houden
            '--------------------------------------------------------------------------------------------------

            'finally remove the original reach
            myReach.InUse = False
            'Me.Reaches.Reaches.Remove(myReach.Id.Trim.ToUpper)

            Return NewNode
        Catch ex As Exception
            Me.Setup.Log.AddError("Error splitting reach at vector point: " & myChainage)
            Return Nothing
        End Try

    End Function
    Friend Function getReachNode(ByVal NodeID As String) As clsSbkReachNode
        If ReachNodes.ReachNodes.ContainsKey(NodeID.Trim.ToUpper) Then
            Return ReachNodes.ReachNodes.Item(NodeID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Function getAddReachNode(ByVal NodeID As String) As clsSbkReachNode
        Dim newNode As clsSbkReachNode
        If ReachNodes.ReachNodes.ContainsKey(NodeID.Trim.ToUpper) Then
            Return ReachNodes.ReachNodes.Item(NodeID.Trim.ToUpper)
        Else
            newNode = New clsSbkReachNode(Setup, SbkCase)
            newNode.ID = NodeID
            ReachNodes.ReachNodes.Add(newNode.ID.Trim.ToUpper, newNode)
            Return newNode
        End If
    End Function

    Friend Function getReachObject(ByVal ObjID As String, IncludeInactiveReaches As Boolean) As clsSbkReachObject
        Dim myObj As clsSbkReachObject
        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If IncludeInactiveReaches OrElse myReach.InUse Then
                For Each myObj In myReach.ReachObjects.ReachObjects.Values
                    If myObj.ID.Trim.ToUpper = ObjID.Trim.ToUpper Then
                        Return myObj
                    End If
                Next
            End If
        Next
        Return Nothing
    End Function


    'Friend Function splitReach(ByVal myReach As clsSbkReach, ByVal splitDistance As Double)

    'Dim NewReach1 As clsSbkReach
    'Dim NewReach2 As clsSbkReach
    'Dim NewNode As clsSbkReachNode

    ''knip ter plaatse van de splitDistance de tak op
    'NewReach1 = New clsSbkReach(Me.Setup, Me.SbkCase)       'splits de tak myreach op in een bovenstrooms en benedenstrooms deel
    'NewReach2 = New clsSbkReach(Me.Setup, Me.SbkCase)       'idem
    'NewNode = New clsSbkReachNode(Me.Setup, Me.SbkCase)     'de splitsingsknoop
    'myReach.InUse = False

    ''bedenk een nieuw ID voor de splitsingsknoop en zorg dat die nog niet in het flow-netwerk bestaat
    ''bepaal dan de dimensies van de splitsingsknoop
    'NewNode.ID = MakeUniqueNodeID(myReach.Id)
    'NewNode.X = myReach.getCoordsFromDistance(NewNode.X, NewNode.Y)
    'NewNode.nt = clsSbkReachNode.enmNodetype.ConnectionNode

    ''we moeten hem als nieuwe knoop invoegen in het bestaande netwerk. Dat betekent: opknippen bestaande tak!
    'Me.ReachNodes.ReachNodes.Add(NewNode.ID.Trim.ToUpper, NewNode)
    'myMC.reachConnection = NewNode   'vertel de dummytak dat we voor zijn benedenstroomse knoop verwijzen naar een bestaande knoop in het grote netwerk

    ''schrijf de begin- en eindknoop van beide nieuwe takken weg
    'NewReach1.bn = myReach.bn               'beginknoop bovenstroomse tak gelijk aan die van oorspronkelijke tak
    'NewReach1.en = NewNode                  'eindknoop van bovenstroomse tak wordt de nieuwe knoop
    'NewReach1.Id = Reaches.CreateID()       'creëer een nieuw ID voor de bovenstroomse tak
    'NewReach1.InUse = True
    'NewReach1.NetworkcpRecord.GetPartFromReach(NewReach1.bn, NewReach1.en, myReach, 0, myMC.OnreachDist, 0)
    'If Not Me.Reaches.Reaches.ContainsKey(NewReach1.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReach1.Id.Trim.ToUpper, NewReach1)
    'Me.MoveAllReachObjects(myReach, NewReach1, 0, myMC.OnreachDist, 0)   'verplaats alle objecten op de tak naar de nieuwe tak!

    'NewReach2.bn = NewNode                  'beginknoop van benedenstroomse tak wordt de nieuwe knoop
    'NewReach2.en = myReach.en               'eindknoop van de benedenstroomse tak is gelijk aan die van de oorspronkelijke
    'NewReach2.Id = Reaches.CreateID()       'creëer een nieuw ID voor de benedenstroomse tak
    'NewReach2.InUse = True
    'NewReach2.NetworkcpRecord.GetPartFromReach(NewReach2.bn, NewReach2.en, myReach, myMC.OnreachDist, 9999999999, myMC.OnreachDist)
    'If Not Me.Reaches.Reaches.ContainsKey(NewReach2.Id.Trim.ToUpper) Then Me.Reaches.Reaches.Add(NewReach2.Id.Trim.ToUpper, NewReach2)
    'Me.MoveAllReachObjects(myReach, NewReach2, myMC.OnreachDist, 999999999, myMC.OnreachDist) 'verplaats alle objecten op de tak naar de nieuwe tak!

    ''nu de oorspronkelijke tak verwijderen
    'Me.Reaches.Reaches.Remove(myReach.Id.Trim.ToUpper)
    'End Function

    Friend Function getSimilarObjectID(ByRef refobj As clsSbkReachObject, ByVal requiredNodeType As STOCHLIB.GeneralFunctions.enmNodetype) As clsSbkReachObject

        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            For Each myObj In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = requiredNodeType Then
                    If Me.Setup.GeneralFunctions.IDsSimilar(refobj.ID, myObj.ID) Then
                        Return myObj
                    End If
                End If
            Next
        Next
        Return Nothing

    End Function

    Friend Function getSimilarReachObject(ByRef RefObj As clsSbkReachObject, MaxSearchRadius As Double) As clsSbkReachObject
        Dim myObj As clsSbkReachObject
        Dim Distance As Double
        Dim myReach As clsSbkReach

        'probeer eerst of we een exacte match van het ID kunnen vinden.
        myObj = getReachObject(RefObj.ID, False)
        If Not myObj Is Nothing Then Return myObj

        'zoek nu op dezelfde locatie naar een object van hetzelfde type
        myReach = Reaches.GetReach(RefObj.ci)
        If Not myReach Is Nothing Then
            myObj = myReach.GetNearestObjectOfType(RefObj.lc, RefObj.nt, Distance)
            If Distance <= MaxSearchRadius Then Return myObj
        End If

        'zoek naar een near identical match
        For Each myReach In Reaches.Reaches.Values
            For Each myObj In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = RefObj.nt Then
                    If Me.Setup.GeneralFunctions.IDsSimilar(RefObj.ID, myObj.ID) Then
                        Return myObj
                    End If
                End If
            Next
        Next

        'als we nu nog hier zijn: niks teruggeven
        Return Nothing

    End Function

    Friend Function FindAddRRCFConnection(ByVal IDBase As String, ByVal StartPoint As clsXYZ, ByVal CatchmentID As String, ByVal PreferReachInsidePolygon As Boolean,
                                           ByVal PreferReachInsideCatchment As Boolean, Optional ByVal SkipBeginAndEndNode As Boolean = False, Optional ByVal MinDistFromReachObjects As Double = 2, Optional ByVal SearchRadiusUp As Double = 100, Optional ByVal SearchRadiusDown As Double = 0, Optional ByRef SubCatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef CatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef ProjectShape As MapWinGIS.Shape = Nothing) As clsSbkReachObject

        Dim mySnapLocation As clsSbkReachObject, myReach As clsSbkReach, myRRCFConn As clsSbkReachObject
        'zoek eerst het meest dichbijgelegen vectorpoint
        'zoek daarna op de gevonden tak of er binnen de zoekstraal al een object van het gewenste type ligt
        mySnapLocation = FindNearestVectorPoint(StartPoint, SkipBeginAndEndNode, MinDistFromReachObjects, SubCatchmentShape, CatchmentShape, ProjectShape)
        myReach = Reaches.GetReach(mySnapLocation.ci.Trim.ToUpper)
        myRRCFConn = myReach.findAddRRCFConnection(mySnapLocation, IDBase, MinDistFromReachObjects, SearchRadiusUp, SearchRadiusDown)
        Return myRRCFConn
    End Function


    Friend Function AddRRCFConnection(ByVal ID As String, ByVal StartPoint As clsXYZ, Optional ByVal SkipBeginAndEndNode As Boolean = False, Optional ByVal MinDistFromReachObjects As Double = 2, Optional ByRef SubCatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef CatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef ProjectShape As MapWinGIS.Shape = Nothing, Optional ByVal SkipReachesWithPrefix As List(Of String) = Nothing) As clsSbkReachObject

        Dim mySnapLocation As clsSbkReachObject, myReach As clsSbkReach, myRRCFConn As clsSbkReachObject

        'zoek eerst het meest dichbijgelegen vectorpoint
        'voeg dan het takobject toe aan de gevonden tak en op de gevonden locatie
        mySnapLocation = FindNearestVectorPoint(StartPoint, SkipBeginAndEndNode, MinDistFromReachObjects, SubCatchmentShape, CatchmentShape, ProjectShape, SkipReachesWithPrefix)

        If Not mySnapLocation Is Nothing Then
            myReach = Reaches.GetReach(mySnapLocation.ci.Trim.ToUpper)
            myRRCFConn = myReach.AddRRCFConnection(ID, mySnapLocation.lc)
            myReach.OptimizeReachObjectLocation(myRRCFConn, MinDistFromReachObjects)
            Return myRRCFConn
        Else
            'no snap location found. This can be because no existing flow model was read. Create a fictious snap location
            myRRCFConn = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myRRCFConn.ID = ID
            myRRCFConn.X = StartPoint.X
            myRRCFConn.Y = StartPoint.Y
            myRRCFConn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection
            Return myRRCFConn
        End If

    End Function

    Friend Function AddLateralNode(ByVal ID As String, ByVal StartPoint As clsXYZ, Optional ByVal SkipBeginAndEndNode As Boolean = False, Optional ByVal MinDistFromReachObjects As Double = 2, Optional ByRef SubCatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef CatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef ProjectShape As MapWinGIS.Shape = Nothing, Optional ByVal SkipReachesWithPrefix As List(Of String) = Nothing) As clsSbkReachObject

        Dim mySnapLocation As clsSbkReachObject, myReach As clsSbkReach, myLateral As clsSbkReachObject

        'zoek eerst het meest dichbijgelegen vectorpoint
        'voeg dan het takobject toe aan de gevonden tak en op de gevonden locatie
        mySnapLocation = FindNearestVectorPoint(StartPoint, SkipBeginAndEndNode, MinDistFromReachObjects, SubCatchmentShape, CatchmentShape, ProjectShape, SkipReachesWithPrefix)

        If Not mySnapLocation Is Nothing Then
            myReach = Reaches.GetReach(mySnapLocation.ci.Trim.ToUpper)
            myLateral = myReach.AddLateralNode(ID, mySnapLocation.lc)
            myReach.OptimizeReachObjectLocation(myLateral, MinDistFromReachObjects)
            Return myLateral
        Else
            'no snap location found. This can be because no existing flow model was read. Create a fictious snap location
            myLateral = New clsSbkReachObject(Me.Setup, Me.SbkCase)
            myLateral.ID = ID
            myLateral.X = StartPoint.X
            myLateral.Y = StartPoint.Y
            myLateral.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
            Return myLateral
        End If

    End Function

    ''' <summary>
    ''' TODO: Siebe invullen
    ''' Deze functie zoekt het dichtstbijzijnde curving point van alle sobektakken voor een gegeven polygoon
    ''' </summary>
    ''' <param name="SkipBeginAndEndNode"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function FindNearestVectorPoint(ByVal StartPoint As clsXYZ, Optional ByVal SkipBeginAndEndNode As Boolean = False, Optional ByVal MinDistFromReachObjects As Double = 2, Optional ByRef subCatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef CatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef ProjectShape As MapWinGIS.Shape = Nothing, Optional ByVal SkipReachesWithPrefix As List(Of String) = Nothing) As clsSbkReachObject
        Dim SnapLocation As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim ReachFound As Boolean = False
        Dim myPoint As New MapWinGIS.Point
        Dim Shape1 As New MapWinGIS.Shape, Shape2 As New MapWinGIS.Shape
        Dim utils As New MapWinGIS.Utils
        Dim OrigX As Double, OrigY As Double
        Dim starti As Integer, endi As Integer
        Dim tmpCP As CP, CPList As New List(Of CP)

        Dim minDist As Double = 9999999999999999
        Dim myStruc As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim nextStruc As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim lastCP As New clsSbkVectorPoint(Me.Setup)
        Dim upObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim downObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim i As Long

        Try

            OrigX = StartPoint.X 'dit is het startpunt waarvandaan we het dichtstbijzijnde punt moeten vinden, dus houd hem vast!
            OrigY = StartPoint.Y 'dit is het startpunt waarvandaan we het dichtstbijzijnde punt moeten vinden, dus houd hem vast!

            'verzamel een lijst met alle geldige curving points
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                Dim SkipReach As Boolean
                If Not SkipReachesWithPrefix Is Nothing Then
                    SkipReach = Me.Setup.GeneralFunctions.HasPrefixFromList(myReach.Id, SkipReachesWithPrefix, False)
                End If

                If myReach.InUse = True AndAlso Not SkipReach Then
                    'version 1.66: from hereon we skip the clause below. Sommige takken hebben slechts twee curving points maar zijn geen dummytakjes.
                    'we vervangen de clausule door een prefix-check
                    'begin met een arbitraire keuze: we willen alleen 'snappen' naar takken die meer dan twee curving points hebben of die langer zijn dan dummylengte (geen dummyreaches dus!)
                    'If myReach.NetworkcpRecord.CPTable.CP.Count > 2 Then

                    If SkipBeginAndEndNode Then
                        starti = 1
                        endi = myReach.NetworkcpRecord.CPTable.CP.Count - 2
                    Else
                        starti = 0
                        endi = myReach.NetworkcpRecord.CPTable.CP.Count - 1
                    End If

                    'vul de verzameling met alle geldige curving points en bereken de afstand tot het snap-point
                    For i = starti To endi
                        tmpCP = New CP
                        tmpCP.CP = myReach.NetworkcpRecord.CPTable.CP(i)
                        tmpCP.ReachID = myReach.Id
                        tmpCP.DistToOutflowPoint = Math.Sqrt((OrigX - myReach.NetworkcpRecord.CPTable.CP(i).X) ^ 2 + (OrigY - myReach.NetworkcpRecord.CPTable.CP(i).Y) ^ 2)
                        CPList.Add(tmpCP)
                    Next
                    'End If
                End If
            Next

            'sorteer de verzameling naar afstand tot het lozingspunt
            Dim query As IEnumerable(Of CP) = CPList.OrderBy(Function(cp) cp.DistToOutflowPoint)

            'vanaf nu is het simpel: wandel naar het eerste het beste CP dat we vinden dat aan alle randvoorwaarden voldoet
            If Not subCatchmentShape Is Nothing Then
                For Each CP In query
                    myPoint.x = CP.CP.X
                    myPoint.y = CP.CP.Y
                    If CP.DistToOutflowPoint > subCatchmentShape.Perimeter Then Exit For 'de afstand tot het lozingspunt > omtrek shape, dus punt kan nooit in de polygoon liggen
                    If utils.PointInPolygon(subCatchmentShape, myPoint) Then
                        SnapLocation.ci = CP.ReachID
                        SnapLocation.lc = CP.CP.Dist
                        SnapLocation.X = CP.CP.X
                        SnapLocation.Y = CP.CP.Y
                        Call OptimizeLateralOnReachLocation(SnapLocation, MinDistFromReachObjects)
                        Return SnapLocation
                    End If
                Next
            End If

            'niet gevonden, dus zoek binnen de catchment
            If Not ReachFound AndAlso Not CatchmentShape Is Nothing Then
                For Each CP In query
                    myPoint.x = CP.CP.X
                    myPoint.y = CP.CP.Y
                    If CP.DistToOutflowPoint > CatchmentShape.Perimeter Then Exit For 'de afstand tot het lozingspunt > omtrek shape, dus punt kan nooit in de polygoon liggen
                    If utils.PointInPolygon(CatchmentShape, myPoint) Then
                        SnapLocation.ci = CP.ReachID
                        SnapLocation.lc = CP.CP.Dist
                        SnapLocation.X = CP.CP.X
                        SnapLocation.Y = CP.CP.Y
                        Call OptimizeLateralOnReachLocation(SnapLocation, MinDistFromReachObjects)
                        Return SnapLocation
                    End If
                Next
            End If

            'niet gevonden, dus zoek binnen de hele shapefile
            If Not ReachFound AndAlso Not ProjectShape Is Nothing Then
                For Each CP In query
                    myPoint.x = CP.CP.X
                    myPoint.y = CP.CP.Y
                    If utils.PointInPolygon(ProjectShape, myPoint) Then
                        SnapLocation.ci = CP.ReachID
                        SnapLocation.lc = CP.CP.Dist
                        SnapLocation.X = CP.CP.X
                        SnapLocation.Y = CP.CP.Y
                        Call OptimizeLateralOnReachLocation(SnapLocation, MinDistFromReachObjects)
                        Return SnapLocation
                    End If
                Next

                'nog altijd niet gevonden, dus pak nu gewoon de dichtstbijzijnde mogelijkheid
            Else
                For Each CP In query
                    myPoint.x = CP.CP.X
                    myPoint.y = CP.CP.Y
                    SnapLocation.ci = CP.ReachID
                    SnapLocation.lc = CP.CP.Dist
                    SnapLocation.X = CP.CP.X
                    SnapLocation.Y = CP.CP.Y
                    Call OptimizeLateralOnReachLocation(SnapLocation, MinDistFromReachObjects)
                    Return SnapLocation
                Next
            End If

        Catch ex As Exception
            Dim log As String = "Error in FindNearestVectorPoint"
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        Finally
            Me.Setup.GeneralFunctions.ReleaseComObject(myPoint, False)
            'Me.setup.GeneralFunctions.ReleaseComObject(myShape, True) '20/6/2012 SB: Volgens mij corrumpeert deze functie het hergebruik van myShape in de tweede ronde! COM object that has been separated from its underlying RCW cannot be used.
            Me.Setup.GeneralFunctions.ReleaseComObject(Shape1, False)
            Me.Setup.GeneralFunctions.ReleaseComObject(Shape2, False)
            Me.Setup.GeneralFunctions.ReleaseComObject(utils, True)
        End Try
        Return Nothing

    End Function

    Public Function NodeIDIsUnique(ByVal ID As String) As Boolean
        If ReachNodes.ReachNodes.ContainsKey(ID.Trim.ToUpper) Then Return False
        If ReachObjectExists(ID) Then Return False
        Return True
    End Function

    ''' <summary>
    ''' This function creates a unique ID for a new node or reach object
    ''' In order to do so it searches until it has found an ID that does not
    ''' yet occur in the sobek schematization
    ''' </summary>
    ''' <param name="IDBase"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MakeUniqueNodeID(ByVal IDBase As String) As String
        Dim IsUnique As Boolean
        Dim newID As String = ""
        Dim i As Integer = 0
        IsUnique = True

        'make sure no empty ID's are returned
        If IDBase = "" Then IDBase = "Node"

        'if the propoposed initial ID is already unique, we'll leave it there 
        If ReachNodes.ReachNodes.ContainsKey(IDBase.Trim.ToUpper) Then IsUnique = False
        If ReachObjectExists(IDBase) Then IsUnique = False
        If IsUnique Then Return IDBase

        'if the IDBase appears not to be unique, we'll add a _ + number postfix until we find a unique one
        'if the IDBase already has a _ + number postfix we'll increase the number
        Dim uScorePos As Integer = InStr(IDBase, "_")
        If uScorePos > 0 AndAlso IsNumeric(Right(IDBase, IDBase.Length - uScorePos)) Then
            i = Int(Right(IDBase, IDBase.Length - uScorePos))
            IDBase = Left(IDBase, uScorePos)
        Else
            i = 0
            IDBase &= "_"                             'add an underscore to distinguish between the numerical part and the textual part
        End If

        While Not IsUnique

            i += 1
            newID = IDBase.Trim & i.ToString.Trim

            'initialiseer hem op true. Binnen deze loop zullen we hem falsificeren als hij toch al blijkt te bestaan
            IsUnique = True

            'zoek of er reachnodes bestaan met dit ID
            If ReachNodes.ReachNodes.ContainsKey(newID.Trim.ToUpper) Then IsUnique = False
            If ReachObjectExists(newID) Then IsUnique = False

        End While

        Return newID
    End Function

    Public Function MakeUniqueReachID(ByVal IDBase As String) As String
        Dim IsUnique As Boolean
        Dim newID As String = ""
        Dim i As Integer = 0
        IsUnique = True

        'if the propoposed initial ID is already unique, we'll leave it there 
        If IDBase = "" Then IDBase = "Reach"
        If Reaches.Reaches.ContainsKey(IDBase.Trim.ToUpper) Then IsUnique = False
        If IsUnique Then Return IDBase

        'if the IDBase appears not to be unique, we'll add a postfix until we find a unique one
        While Not IsUnique
            i += 1
            newID = IDBase.Trim & "_" & i.ToString.Trim

            'initialiseer hem op true. Binnen deze loop zullen we hem falsificeren als hij toch al blijkt te bestaan
            IsUnique = True

            'zoek of er reachnodes bestaan met dit ID
            If Reaches.Reaches.ContainsKey(newID.Trim.ToUpper) Then IsUnique = False

        End While

        Return newID
    End Function

    Public Sub OptimizeLateralOnReachLocation(ByRef SnapLocation As clsSbkReachObject, ByVal MindistFromReachObjects As Double)
        Dim minDist As Double = 9999999999999999
        Dim myDist As Double, originalSnapDistance As Double
        Dim myStruc As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim nextStruc As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim nCP As Integer, lastCP As New clsSbkVectorPoint(Me.Setup)
        Dim upObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim downObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim upDist As Double, dnDist As Double
        Dim UpstreamReachesProcessed As Dictionary(Of String, clsSbkReach)
        Dim DownstreamReachesProcessed As Dictionary(Of String, clsSbkReach)

        'nu we de snaplocatie gevonden hebben, gaan we 'wandelen' tot we een plek hebben gevonden die aan de
        'eisen voor de relatie tot takobjecten voldoet
        Dim myReachTopology As clsSbkReach = Me.SbkCase.CFTopo.Reaches.GetReach(SnapLocation.ci.Trim.ToUpper)
        originalSnapDistance = SnapLocation.lc
        nCP = myReachTopology.NetworkcpRecord.CPTable.CP.Count - 1
        lastCP = myReachTopology.NetworkcpRecord.CPTable.CP(nCP)

        'eerst wandelen we meter voor meter naar benedenstrooms.
        minDist = 9999999999999999
        For myDist = Me.Setup.GeneralFunctions.RoundUD(originalSnapDistance, 0, True) To Me.Setup.GeneralFunctions.RoundUD(lastCP.Dist, 0, False)
            'zoek het direct bovenstrooms gelegen takobject op
            UpstreamReachesProcessed = New Dictionary(Of String, clsSbkReach)
            upObject = myReachTopology.GetUpstreamObject(UpstreamReachesProcessed, myDist, upDist, True, True, True, False)
            DownstreamReachesProcessed = New Dictionary(Of String, clsSbkReach)
            downObject = myReachTopology.GetDownstreamObject(DownstreamReachesProcessed, myDist, dnDist, True, True, True, False)

            If upObject Is Nothing Then
                'maak een fictief bovenstrooms object
                upObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                upObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.Virtual
                upObject.lc = 0
            End If

            If downObject Is Nothing Then
                'maak een fictief benedenstrooms object
                downObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                downObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.Virtual
                downObject.lc = lastCP.Dist
            End If

            If (myDist - upObject.lc) >= MindistFromReachObjects AndAlso (downObject.lc - myDist) >= MindistFromReachObjects Then
                'we hebben een beter plekje gevonden!
                SnapLocation.lc = myDist
                myReachTopology.NetworkcpRecord.CalcXY(SnapLocation.lc, SnapLocation.X, SnapLocation.Y)
                minDist = Math.Abs(myDist - originalSnapDistance)
                Exit For
            End If

            If (downObject.lc - myDist) < MindistFromReachObjects _
                AndAlso (downObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir OrElse downObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir _
                         OrElse downObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump OrElse downObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice) Then
                'we steken straks een stuw over, en dat mag niet dus verlaat nu de loop
                Exit For
            End If
        Next

        'en dan wandelen we meter voor meter naar bovenstrooms.
        For myDist = Me.Setup.GeneralFunctions.RoundUD(originalSnapDistance, 0, False) To 0 Step -1
            'zoek het direct bovenstrooms gelegen takobject op
            UpstreamReachesProcessed = New Dictionary(Of String, clsSbkReach)
            upObject = myReachTopology.GetUpstreamObject(UpstreamReachesProcessed, myDist, upDist, True, True, True, False)
            DownstreamReachesProcessed = New Dictionary(Of String, clsSbkReach)
            downObject = myReachTopology.GetDownstreamObject(DownstreamReachesProcessed, myDist, dnDist, True, True, True, False)

            If upObject Is Nothing Then
                'maak een fictief bovenstrooms object
                upObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                upObject.lc = 0
            End If

            If downObject Is Nothing Then
                'maak een fictief bovenstrooms object
                downObject = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                downObject.lc = lastCP.Dist
            End If

            If myDist - upObject.lc > MindistFromReachObjects AndAlso downObject.lc - myDist > MindistFromReachObjects Then
                If Math.Abs(myDist - originalSnapDistance) < minDist Then
                    'we hebben een nog beter plekje gevonden!
                    SnapLocation.lc = myDist
                    myReachTopology.NetworkcpRecord.CalcXY(SnapLocation.lc, SnapLocation.X, SnapLocation.Y)
                    Exit For
                End If
            End If

            If (myDist - upObject.lc) < MindistFromReachObjects _
              AndAlso (upObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir OrElse upObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir _
                       OrElse upObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump OrElse upObject.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice) Then
                'we steken straks een stuw over, en dat mag niet dus verlaat nu de loop
                Exit For
            End If
        Next
    End Sub

    'v2.203 model of strabeekse vloedgraaf failed due to  MinDistFromReachObjects =2. Made this argument compulsory
    Public Function OptimizeAllReachObjectLocations(MinDistBetweenReachObjects As Double) As Boolean
        Try
            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                Call myReach.OptimizeReachObjectLocations(MinDistBetweenReachObjects)
            Next
            Me.Setup.Log.AddMessage("Positions of reachobjects on reaches were successfully optimized.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function OptimizeAllReachObjectLocations.")
            Return False
        End Try
    End Function

    Friend Function CreateAndSnapReachObject(ByVal ID As String, ByVal X As Double, ByVal Y As Double, ByVal nt As STOCHLIB.GeneralFunctions.enmNodetype, ByVal ReplaceExisting As Boolean, Optional ByRef SubCatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef CatchmentShape As MapWinGIS.Shape = Nothing, Optional ByRef ProjectShape As MapWinGIS.Shape = Nothing) As Boolean
        Try

            'this function creates a reach object (clsSbkReachObject) and snaps it to the nearest vector point in the schematisation
            Dim myObj As clsSbkReachObject = getReachObject(ID, False)
            Dim myReach As clsSbkReach
            Dim XYZ As New clsXYZ(X, Y, 0)

            If Not myObj Is Nothing AndAlso ReplaceExisting Then
                'remove the existing object and create new one
                myReach = Reaches.Reaches.Item(myObj.ci.Trim.ToUpper)
                myReach.ReachObjects.ReachObjects.Remove(myObj.ID.Trim.ToUpper)
                myObj = FindNearestVectorPoint(XYZ, True, 2)
                myObj.ID = ID
                myObj.Name = ID
                myObj.InUse = True
                myObj.nt = nt
                myReach = Reaches.Reaches.Item(myObj.ci.Trim.ToUpper)
                myReach.ReachObjects.Add(myObj)
            ElseIf myObj Is Nothing Then
                myObj = FindNearestVectorPoint(XYZ, True, 2)
                myObj.ID = ID
                myObj.Name = ID
                myObj.InUse = True
                myObj.nt = nt
                myReach = Reaches.Reaches.Item(myObj.ci.Trim.ToUpper)
                myReach.ReachObjects.Add(myObj)
            Else
                Me.Setup.Log.AddWarning("Measurement station " & ID & " could not be added to the schematization since an object with the same ID already existed.")
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function


    Friend Sub SnapLateralsFromAreas()
        Dim mySubcatchment As clsSubcatchment, myCatchment As clsCatchment
        Dim myLateral As clsSbkReachObject
        Dim i As Integer = 0
        Try
            For Each myCatchment In Me.Setup.GISData.Catchments.Catchments.Values
                i = 0
                For Each mySubcatchment In myCatchment.Subcatchments.Subcatchments.Values

                    i += 1
                    Setup.GeneralFunctions.UpdateProgressBar("Finding snap location for area " & mySubcatchment.ID, i, myCatchment.Subcatchments.Subcatchments.Count)
                    mySubcatchment.SnapLocation = FindNearestVectorPoint(mySubcatchment.LowestPoint, True, 5)

                    'voeg nu ook de knoop toe aan de actieve sobek-case om te voorkomen dat dezelfde snaplocatie meerdere malen wordt gebruikt
                    myLateral = New clsSbkReachObject(Me.Setup, Me.SbkCase)
                    myLateral.ID = Me.Setup.Settings.CFSettings.getLateralPrefix & mySubcatchment.ID
                    myLateral.ci = mySubcatchment.SnapLocation.ci
                    myLateral.lc = mySubcatchment.SnapLocation.lc
                    myLateral.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
                    Call AddReachObject(myLateral)
                Next
            Next
        Catch ex As Exception
            Setup.Log.AddDebugMessage("Error in sub FindSnapLocations")
        End Try
    End Sub

    Friend Function GetNearestReach(ByVal X As Double, ByVal Y As Double) As clsSbkReach
        Dim Nearest As New clsSbkReach(Me.Setup, Me.SbkCase)
        Dim minDist As Double = 999999999999999
        Dim myDist As Double

        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            For Each myCP As clsSbkVectorPoint In myReach.NetworkcpRecord.CPTable.CP
                myDist = Math.Sqrt((myCP.X - X) ^ 2 + (myCP.Y - Y) ^ 2)
                If myDist < minDist Then
                    Nearest = myReach
                    minDist = myDist
                End If
            Next myCP
        Next myReach

        Return Nearest
    End Function
    Friend Function GetNearestCPIdx(ByVal myReach As clsSbkReach, ByVal X As Double, ByVal Y As Double) As Integer
        Dim i As Integer
        Dim myDist As Double
        Dim minDist As Double = 99999999999999
        Dim minXY As New clsXY
        Dim myCP As clsSbkVectorPoint
        Dim myIdx As Integer

        For i = 0 To myReach.NetworkcpRecord.CPTable.CP.Count - 1
            myCP = myReach.NetworkcpRecord.CPTable.CP(i)
            myDist = Math.Sqrt((myCP.X - X) ^ 2 + (myCP.Y - Y) ^ 2)
            If myDist < minDist Then
                minDist = myDist
                myIdx = i
            End If
        Next i

        Return myIdx

    End Function
    ''' <summary>
    ''' Deze functie zoekt uit of een takobject bestaat en geeft een boolean terug 
    ''' </summary>
    ''' <param name="ID"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function ReachObjectExists(ByVal ID As String) As Boolean
        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If myReach.ReachObjects.ReachObjects.ContainsKey(ID.Trim.ToUpper) Then
                Return True
            End If
        Next myReach
        Return False
    End Function

    Friend Function GetReachObject(ByVal ID As String) As clsSbkReachObject
        For Each myReach As clsSbkReach In Reaches.Reaches.Values
            If myReach.ReachObjects.ReachObjects.ContainsKey(ID.Trim.ToUpper) Then
                Return myReach.ReachObjects.ReachObjects.Item(ID.Trim.ToUpper)
            End If
        Next
        Return Nothing
    End Function


    Public Function ReplaceLinkageNodes() As Boolean
        'this function replaces all linkage nodes in the current model by connection nodes with interpolation!
        'note: the collection of reaches MUST be reset after each node change since it always results in a new reach
        Dim LinkageNodeFound As Boolean = True
        Dim myReach As clsSbkReach
        Dim i As Long, n As Long, cycle As Long = 0
        Dim myLinkageReachObject As New clsSbkReachObject(Me.Setup, Me.SbkCase)
        Dim myLinkageNode As New clsSbkReachNode(Me.Setup, Me.SbkCase)

        Try
            While ReachNodes.CountLinkageNodes > 0
                n = Reaches.Reaches.Values.Count
                cycle += 1
                i = 0

                For i = 0 To n - 1
                    myReach = Reaches.Reaches.Values(i)
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Replacing Linkage Nodes, cycle " & cycle, i, n)
                    If myReach.containsLinkage(myLinkageReachObject) Then
                        'find the linkage node in its reachnode-form
                        myLinkageNode = ReachNodes.ReachNodes.Item(myLinkageReachObject.ID.Trim.ToUpper)
                        Call SplitReachAtLinkageNode(myReach, myLinkageNode)
                    End If
                Next
            End While
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try
    End Function


    Friend Sub MoveAllReachObjects(ByRef FromReach As clsSbkReach, ByRef ToReach As clsSbkReach, ByVal FromDist As Double,
                                   ByVal ToDist As Double)
        '--------------------------------------------------------------------------------------------------------------
        'Datum: 18-05-2012
        'Auteur: Siebe Bosch
        'Deze subroutine verplaatst objecten van de ene naar de andere tak.
        '--------------------------------------------------------------------------------------------------------------
        Dim myReachNode As clsSbkReachNode, i As Long
        Dim myObject As clsSbkReachObject

        If FromDist < 0 Then FromDist = 0
        If ToDist > FromReach.getReachLength Then ToDist = FromReach.getReachLength

        For i = FromReach.ReachObjects.ReachObjects.Values.Count - 1 To 0 Step -1
            myObject = FromReach.ReachObjects.ReachObjects.Values(i)
            If myObject.lc >= FromDist And myObject.lc <= ToDist Then
                myObject.ci = ToReach.Id
                myObject.lc -= FromDist
                ToReach.ReachObjects.ReachObjects.Add(myObject.ID.Trim.ToUpper, myObject)   'voeg toe aan de nieuwe tak

                'if the object is a linkage node, we'll also have to adjust the ci record for the clsSbkReachNodes record
                If myObject.nt = GeneralFunctions.enmNodetype.NodeCFLinkage Then
                    myReachNode = ReachNodes.ReachNodes.Item(myObject.ID.Trim.ToUpper)
                    If Not myReachNode Is Nothing Then
                        myReachNode.ci = ToReach.Id
                        myReachNode.lc -= FromDist
                    Else
                        Me.Setup.Log.AddError("Error finding linkage node " & myObject.ID & " as starting or ending node of a reach, although it was found as a reach object.")
                    End If
                End If

                'finally remove the instance from the original reach
                FromReach.ReachObjects.ReachObjects.Remove(FromReach.ReachObjects.ReachObjects.Keys(i))

            End If


        Next


    End Sub

    Public Function changeConnectionNodesToBoundariesWithConstantFlow(ByVal ConstantQ As Double, ShapeFilePath As String) As Boolean
        Dim FLBORecord As clsBoundaryDatFLBORecord
        Dim UseShapeFile As Boolean = False
        Dim myUtils As New MapWinGIS.Utils
        Dim ShapeFile As New MapWinGIS.Shapefile
        Dim Apply As Boolean


        Try
            If ShapeFilePath <> "" AndAlso System.IO.File.Exists(ShapeFilePath) Then
                UseShapeFile = True
                ShapeFile = New MapWinGIS.Shapefile
                If Not ShapeFile.Open(ShapeFilePath) Then Throw New Exception("Error reading shapefile: " & ShapeFilePath)
            End If

            For Each myReach As clsSbkReach In Reaches.Reaches.Values
                If myReach.bn.isConnectionNode AndAlso Not myReach.HasUpstreamReach(True) AndAlso Not myReach.bn.isLateral Then

                    Apply = True
                    If UseShapeFile Then
                        Dim InPolygon As Boolean = False
                        For j = 0 To ShapeFile.NumShapes - 1
                            If myUtils.PointInPolygon(ShapeFile.Shape(j), myReach.bn.makeMapWinGisPoint) Then InPolygon = True
                        Next
                        If Not InPolygon Then Apply = False     'falsify if point is not inside any of the polygons
                    End If

                    If Apply Then
                        myReach.bn.nt = GeneralFunctions.enmNodetype.NodeCFBoundary
                        myReach.bn.NetworkObiRecord.ID = myReach.bn.ID
                        myReach.bn.NetworkObiRecord.ci = "SBK_BOUNDARY"
                        FLBORecord = SbkCase.CFData.Data.BoundaryData.BoundaryDatFLBORecords.GetByID(myReach.bn.ID.Trim.ToUpper)
                        If Not FLBORecord Is Nothing Then
                            'set this record to the constant discharge
                            FLBORecord.ty = 1
                            FLBORecord.q_dw = 0
                            FLBORecord.q_dw0 = ConstantQ
                            FLBORecord.q_dw1 = 0
                        Else
                            FLBORecord = New clsBoundaryDatFLBORecord(Me.Setup)
                            FLBORecord.ID = myReach.bn.ID
                            FLBORecord.InUse = True
                            FLBORecord.ty = 1
                            FLBORecord.q_dw = 0
                            FLBORecord.q_dw0 = ConstantQ
                            FLBORecord.q_dw1 = 0
                            SbkCase.CFData.Data.BoundaryData.BoundaryDatFLBORecords.records.Add(FLBORecord.ID.Trim.ToUpper, FLBORecord)
                        End If
                    End If
                End If

                If myReach.en.isConnectionNode AndAlso Not myReach.HasDownstreamReach(True) AndAlso Not myReach.en.isLateral Then

                    Apply = True
                    If UseShapeFile Then
                        Dim InPolygon As Boolean = False
                        For j = 0 To ShapeFile.NumShapes - 1
                            If myUtils.PointInPolygon(ShapeFile.Shape(j), myReach.en.makeMapWinGisPoint) Then InPolygon = True
                        Next
                        If Not InPolygon Then Apply = False     'falsify if point is not inside any of the polygons
                    End If

                    If Apply Then
                        myReach.en.nt = GeneralFunctions.enmNodetype.NodeCFBoundary
                        myReach.en.NetworkObiRecord.ID = myReach.en.ID
                        myReach.en.NetworkObiRecord.ci = "SBK_BOUNDARY"
                        FLBORecord = SbkCase.CFData.Data.BoundaryData.BoundaryDatFLBORecords.GetByID(myReach.en.ID.Trim.ToUpper)
                        If Not FLBORecord Is Nothing Then
                            'set this record to the constant discharge
                            FLBORecord.ty = 1
                            FLBORecord.q_dw = 0
                            FLBORecord.q_dw0 = ConstantQ
                            FLBORecord.q_dw1 = 0
                        Else
                            FLBORecord = New clsBoundaryDatFLBORecord(Me.Setup)
                            FLBORecord.ID = myReach.en.ID
                            FLBORecord.InUse = True
                            FLBORecord.ty = 1
                            FLBORecord.q_dw = 0
                            FLBORecord.q_dw0 = ConstantQ
                            FLBORecord.q_dw1 = 0
                            SbkCase.CFData.Data.BoundaryData.BoundaryDatFLBORecords.records.Add(FLBORecord.ID.Trim.ToUpper, FLBORecord)
                        End If
                    End If
                End If

            Next

            If UseShapeFile Then
                ShapeFile.Close()
            End If

            Me.Setup.Log.AddMessage("Dead end connectoin nodes successfully changed to boundaries with constant flow.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function changeConnectionNodesToBoundariesWithConstantFlow.")
            Me.Setup.Log.AddError(ex.Message)
            Return False

        End Try


    End Function

End Class
