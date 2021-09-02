Imports STOCHLIB.General
Public Class clsNetworkTopology
    Private Setup As clsSetup

    Dim Topology As New Dictionary(Of String, clsTopologicalConnection)

    'network topology is described here as a dictionary of areas in which each area describes to which other area it is connected and via which structure
    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Function ExtendWithSobekStructuresAndSubcatchmentsShapefile(StructureType As GeneralFunctions.enmNodetype, Optional ByVal maxSearchRadius As Double = 100) As Boolean
        'deze functie selecteert peilscheidende kunstwerken en voegt ze toe aan de netwerktopologie
        Dim upAreaIdx As Integer, dnAreaIdx As Integer
        Dim upObj As New clsSbkReachObject(Me.Setup, Me.Setup.SOBEKData.ActiveProject.ActiveCase)
        Dim dnObj As New clsSbkReachObject(Me.Setup, Me.Setup.SOBEKData.ActiveProject.ActiveCase)
        Try
            Dim iReach As Integer = 0
            Dim nReach As Integer = Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building structures...", iReach, nReach, True)
            For Each myReach As clsSbkReach In Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.nt = StructureType Then
                            myObj.calcXY()
                            'creëer een steeds groter wordende zoekstraal en prik in de peilgebiedenkaart
                            For r = 1 To maxSearchRadius
                                upObj.ci = myReach.Id
                                upObj.lc = myObj.lc - r
                                upObj.calcXY()
                                upAreaIdx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.GetShapeIdxByCoord(upObj.X, upObj.Y)
                                dnObj.ci = myReach.Id
                                dnObj.lc = myObj.lc + 1
                                dnObj.calcXY()
                                dnAreaIdx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.GetShapeIdxByCoord(dnObj.X, dnObj.Y)
                                If upAreaIdx <> dnAreaIdx Then
                                    'this structure will be added to the topological relation between upstream and downstream area
                                    Dim key As String = Me.Setup.GISData.SubcatchmentDataSource.GetTextValue(upAreaIdx, GeneralFunctions.enmInternalVariable.ID).Trim.ToUpper & "_" & Me.Setup.GISData.SubcatchmentDataSource.GetTextValue(dnAreaIdx, GeneralFunctions.enmInternalVariable.ID).Trim.ToUpper
                                    If Topology.ContainsKey(key) Then
                                        'place the object halfway our connection
                                        Topology.Item(key).AddStructureHalfway(myObj.ID, StructureType)
                                    End If
                                    Exit For
                                End If
                            Next
                        End If
                    Next
                End If
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub BuildFromSobekReachesAndSubcatchmentsShapefile()
        'this routine builds the topology for two given datasets:
        '1. a SOBEK network of reaches
        '2. a subcatchments shapefile

        'read the subcatchment's shapefile and set the property BeginPointInShapefile to True in order to speed up the indexing process
        Me.Setup.GISData.SubcatchmentDataSource.Open()

        Dim p1X As Double, p1Y As Double
        Dim p2X As Double, p2Y As Double
        Dim p1Idx As Integer, p2Idx As Integer
        Dim p1Id As String, p2Id As String
        Dim p1 As New clsXY, p2 As New clsXY
        Dim i As Integer = 0, iReach As Integer = 0, nReach As Integer = Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Count

        'loop through each sobek reach
        Me.Setup.GeneralFunctions.UpdateProgressBar("Analyzing topology...", 0, 10, True)
        For Each myReach As clsSbkReach In Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Values
            iReach += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
            If myReach.InUse Then
                'walk along the reach using two points with a distance of 1 m and investigate whether two different ID's are touched
                For i = 0 To myReach.getReachLength - 1
                    myReach.calcXY(i, p1X, p1Y)
                    myReach.calcXY(i + 1, p2X, p2Y)

                    'note: the file has already been opened (see row 18) AND the option BeginPointInShapefile has already been set. So now we can speed up the process by not doing that over and over in the following function calls
                    p1Idx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.GetShapeIdxByCoord(p1X, p1Y, False, False, False)
                    p2Idx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.GetShapeIdxByCoord(p2X, p2Y, False, False, False)

                    If p1Idx <> p2Idx Then
                        'we have found a transition from one shape to another

                        'create the from-location
                        If p1Idx >= 0 Then
                            'area shape found. Use it centerpoint as the origin for this topological connection
                            p1Id = Me.Setup.GISData.SubcatchmentDataSource.GetTextValue(p1Idx, GeneralFunctions.enmInternalVariable.ID)
                            p1 = New clsXY(p1Id, p1Id, Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.Shape(p1Idx).InteriorPoint.x, Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.Shape(p1Idx).InteriorPoint.y)
                        Else
                            'no area shape found. Use the starting point of the reach as the origin for this topological connection
                            p1Id = ""
                            p1 = New clsXY(p1Id, p1Id, myReach.bn.X, myReach.bn.Y)
                        End If

                        'create the to-location
                        If p2Idx >= 0 Then
                            'area shape found. Use it centerpoint as the target for this topological connection
                            p2Id = Me.Setup.GISData.SubcatchmentDataSource.GetTextValue(p2Idx, GeneralFunctions.enmInternalVariable.ID)
                            p2 = New clsXY(p2Id, p2Id, Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.Shape(p2Idx).InteriorPoint.x, Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.Shape(p2Idx).InteriorPoint.y)
                        Else
                            'no area shape found. Use the endpoint of the reach as the origin for this topological connection
                            p2Id = ""
                            p2 = New clsXY(p2Id, p2Id, myReach.en.X, myReach.en.Y)
                        End If

                        'create the topological relation and add the structure
                        Dim key As String = p1Id.Trim.ToUpper & "_" & p2Id.Trim.ToUpper
                        Dim myTopo As clsTopologicalConnection
                        If Not Topology.ContainsKey(key) Then
                            myTopo = New clsTopologicalConnection(Me.Setup, p1, p2, Nothing)
                            Topology.Add(key, myTopo)
                        Else
                            If Not Topology.Item(key).Structures.ContainsKey("") Then Topology.Item(key).Structures.Add("", Nothing)
                        End If

                    End If

                Next
            End If
        Next
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

        Me.Setup.GISData.SubcatchmentDataSource.Close()

    End Sub

    'Public Function WriteToAreaShapefile() As Boolean
    '    Try
    '        Dim AreaID As String
    '        Dim iConn As Integer = 0
    '        Dim AreaField As String, StrucField As String
    '        Dim AreaFieldIdx As Integer, StrucFieldIdx As Integer
    '        Dim i As Integer, n As Integer

    '        Me.Setup.GISData.SubcatchmentDataSource.Open()
    '        Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.StartEditingTable()
    '        n = Me.Setup.GISData.SubcatchmentDataSource.GetNumberOfRecords

    '        'first clear all topology fields from the subcatchments shapefile from previous runs
    '        Dim Done As Boolean = False
    '        Dim FieldIdx As Integer
    '        iConn = 0
    '        While Not Done
    '            Done = True
    '            iConn += 1
    '            FieldIdx = Me.Setup.GISData.SubcatchmentDataSource.Fields(toarea.GetFieldIdx("ToArea" & iConn)
    '            If fieldidx >= 0 Then
    '                Done = False
    '                Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.EditDeleteField(FieldIdx)
    '            End If
    '            FieldIdx = Me.Setup.GISData.SubcatchmentDataSource.GetFieldIdx("ViaStruc" & iConn)
    '            If FieldIdx >= 0 Then
    '                Done = False
    '                Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.EditDeleteField(FieldIdx)
    '            End If
    '        End While

    '        'then walk through each area and see if it is a member of one of the topological connections we've found
    '        For i = 0 To n - 1
    '            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
    '            AreaID = Me.Setup.GISData.SubcatchmentDataSource.getAreaIDByIdx(i)
    '            iConn = 0

    '            'search for this area for any existing topological connections
    '            For Each Connection As clsTopologicalConnection In Topology.Values
    '                If Connection.FromNode.ID = AreaID Then
    '                    If Connection.Structures.Count = 0 Then
    '                        'no structure, but the areas are connected (open connection) otherwise there would not be a connection
    '                        iConn += 1
    '                        AreaField = "ToArea" & iConn
    '                        StrucField = "ViaStruc" & iConn
    '                        AreaFieldIdx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.getaddfield(AreaField, MapWinGIS.FieldType.STRING_FIELD, 0, 20)
    '                        StrucFieldIdx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.GetAddField(StrucField, MapWinGIS.FieldType.STRING_FIELD, 0, 20)
    '                        Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.EditCellValue(AreaFieldIdx, i, Connection.ToNode.ID)
    '                    Else
    '                        For Each myStructure As clsTopologyStructurelocation In Connection.Structures.Values
    '                            iConn += 1
    '                            AreaField = "ToArea" & iConn
    '                            StrucField = "ViaStruc" & iConn
    '                            AreaFieldIdx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.GetAddField(AreaField, MapWinGIS.FieldType.STRING_FIELD, 0, 20)
    '                            StrucFieldIdx = Me.Setup.GISData.SubcatchmentDataSource.Shapefile.GetAddField(StrucField, MapWinGIS.FieldType.STRING_FIELD, 0, 20)
    '                            Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.EditCellValue(AreaFieldIdx, i, Connection.ToNode.ID)
    '                            If Not myStructure Is Nothing Then Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.EditCellValue(StrucFieldIdx, i, myStructure.ID)
    '                        Next
    '                    End If
    '                End If
    '            Next
    '        Next
    '        Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.StopEditingTable(True)
    '        Me.Setup.GISData.SubcatchmentDataSource.Shapefile.Close()
    '        Return True
    '    Catch ex As Exception
    '        Return False
    '    End Try
    'End Function
    Public Function ExportToShapefiles(ExportDir As String) As Boolean
        Try
            Dim ChannelSF As New clsPolyLineShapeFile(Me.Setup)
            Dim ChannelIdFieldIdx As Integer, ChannelShapeIdx As Integer

            Dim WeirSF As New clsPointShapeFile(Me.Setup)
            Dim WeirIdFieldIdx As Integer, WeirShapeIdx As Integer

            Dim ChannelShape As MapWinGIS.Shape
            Dim WeirShape As MapWinGIS.Shape
            Dim i As Integer = 0, n As Integer = Topology.Count

            ChannelSF.CreateNew(ExportDir & "\Connections.shp")
            ChannelSF.Open()
            ChannelSF.StartEditing(True)
            ChannelSF.AddField("ID", MapWinGIS.FieldType.STRING_FIELD, 20, 0, ChannelIdFieldIdx)

            WeirSF.CreateNew(ExportDir & "\Weirs.shp")
            WeirSF.Open()
            WeirSF.StartEditing(True)
            WeirSF.AddField("ID", MapWinGIS.FieldType.STRING_FIELD, 20, 0, WeirIdFieldIdx)

            Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting topology to shapefiles...", 0, 10, True)
            For Each myConnection As clsTopologicalConnection In Topology.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                ChannelShape = New MapWinGIS.Shape
                ChannelShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)
                ChannelShape.AddPoint(myConnection.FromNode.X, myConnection.FromNode.Y)
                ChannelShape.AddPoint(myConnection.ToNode.X, myConnection.ToNode.Y)
                ChannelShapeIdx = ChannelSF.SF.sf.EditAddShape(ChannelShape)
                ChannelSF.SF.sf.EditCellValue(ChannelIdFieldIdx, ChannelShapeIdx, myConnection.FromNode.ID & "_" & myConnection.ToNode.ID)

                For Each myStructure As clsTopologyStructurelocation In myConnection.Structures.Values
                    If Not myStructure Is Nothing AndAlso myStructure.StructureType = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir Then
                        WeirShape = New MapWinGIS.Shape
                        WeirShape.Create(MapWinGIS.ShpfileType.SHP_MULTIPOINT)
                        WeirShape.AddPoint(myStructure.X, myStructure.Y)
                        WeirShapeIdx = WeirSF.SF.sf.EditAddShape(WeirShape)
                        WeirSF.SF.sf.EditCellValue(WeirIdFieldIdx, WeirShapeIdx, myStructure.ID)
                    End If
                Next

            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

            ChannelSF.StopEditing(True, True)
            ChannelSF.SF.sf.Save()
            ChannelSF.Close()

            WeirSF.StopEditing(True, True)
            WeirSF.SF.sf.Save()
            WeirSF.Close()

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


End Class
