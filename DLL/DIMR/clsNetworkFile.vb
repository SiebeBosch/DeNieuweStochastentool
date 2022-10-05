Imports Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative
Imports STOCHLIB.General

Public Class clsNetworkFile
    Private Setup As clsSetup
    Private FlowFM As clsFlowFMComponent
    Friend Path As String

    Friend Branches As New Dictionary(Of String, cls1DBranch)
    Friend Nodes1D As New Dictionary(Of String, cls1DNode)
    Friend CellCenters2D As New Dictionary(Of Integer, clsXY)         'a low memory storage facility for 2D cell centerpoints.
    Friend Links1D2D As New List(Of cls1D2DLink)

    'all variables describing the network file's content

    'this block reads all variables concerning our 1D network from the file
    Dim links As Int32(,)
    Dim network1d_geom_y As Double()
    Dim network1d_geom_x As Double()
    Dim network1d_geom_node_count As Int32()
    Dim network1d_geometry As Int32
    Dim network1d_edge_nodes As Int32(,)
    Dim network1d_branch_order As Int32()
    Dim network1d_edge_length As Double()
    Dim network1d_branch_long_name As Byte(,)
    Dim network1d_branch_id As Byte(,)
    Dim network1d_node_y As Double()
    Dim network1d_node_x As Double()
    Dim network1d_node_long_name As Byte(,)
    Dim network1d_node_id As Byte(,)
    Dim network1d As Int32
    Dim mesh1d_node_offset As Double()
    Dim mesh1d_node_branch As Int32()
    Dim mesh1d_edge_y As Double()
    Dim mesh1d_edge_x As Double()
    Dim mesh1d_edge_offset As Double()
    Dim mesh1d_edge_branch As Int32()
    Dim mesh1d_edge_nodes As Int32(,)
    Dim mesh1d_node_long_name As Byte(,)
    Dim mesh1d_node_id As Byte(,)
    Dim mesh1d As Int32

    Dim mesh2d_face_x As Double()
    Dim mesh2d_face_y As Double()


    'opm Arthur van Dam d.d. 5-10-2022:
    'of het 0- of 1-based is staat gespecificeerd:
    'Default Is 0, en met attribuut :start_index kan de file specificeren dat het bijvoorbeeld 1 Is, zie
    Dim Base1 As Boolean = False


    Public Sub New(myPath As String, ByRef mySetup As clsSetup, ByRef myFlowFM As clsFlowFMComponent)
        Path = myPath
        Setup = mySetup
        FlowFM = myFlowFM
    End Sub

    Public Function Read() As Boolean
        Try
            Me.Setup.Log.AddMessage("Setting network file variables.")
            Dim DataSet As Microsoft.Research.Science.Data.DataSet
            Dim myDatasets As Microsoft.Research.Science.Data.DataSet()
            Me.Setup.Log.AddMessage("Network file variables successfully set.")

            'we use the microsoft scientific dataset library to read this netcdf-file
            If Not System.IO.File.Exists(Path) Then
                Throw New Exception("FlowFM network file does not exist and could not be read: " & Path)
            Else
                Me.Setup.Log.AddMessage("FlowFM Network file found: " & Path)
            End If

            DataSet = Microsoft.Research.Science.Data.DataSet.Open(Path & "?openMode=readOnly")
            Me.Setup.Log.AddMessage("FlowFM networkfile successfully opened: " & Path)

            myDatasets = DataSet.GetLinkedDataSets
            Me.Setup.Log.AddMessage("Number of datasets found in network file: " & myDatasets.Length & ".")

            Dim myDimensions As Microsoft.Research.Science.Data.ReadOnlyDimensionList = DataSet.Dimensions
            Me.Setup.Log.AddMessage("Number of dimensions found in network file: " & myDimensions.Count & ".")

            Dim myVariables As Microsoft.Research.Science.Data.ReadOnlyVariableCollection = DataSet.Variables
            Me.Setup.Log.AddMessage("Number of variables found in network file: " & myVariables.Count & ".")

            Dim i As Integer

            'determine whether our data is 1-based or 0-based
            Dim start_index As Integer = 0

            '20220309: naming is now consistent with the variable names themselves
            Dim Mesh2d_face_zIdx As Integer = -1
            Dim links_contact_typeIdx As Integer = -1
            Dim links_contact_long_nameIdx As Integer = -1
            Dim links_contact_idIdx As Integer = -1
            Dim linksIdx As Integer = -1
            Dim Mesh2d_interface_sigmaIdx As Integer = -1
            Dim Mesh2d_layer_sigmaIdx As Integer = -1
            Dim Mesh2d_face_y_bndIdx As Integer = -1
            Dim Mesh2d_face_x_bndIdx As Integer = -1
            Dim Mesh2d_face_yIdx As Integer = -1
            Dim Mesh2d_face_xIdx As Integer = -1
            Dim Mesh2d_face_nodesIdx As Integer = -1
            Dim Mesh2d_edge_nodesIdx As Integer = -1
            Dim Mesh2d_edge_yIdx As Integer = -1
            Dim Mesh2d_edge_xIdx As Integer = -1
            Dim Mesh2d_node_zIdx As Integer = -1
            Dim Mesh2d_node_yIdx As Integer = -1
            Dim Mesh2d_node_xIdx As Integer = -1
            Dim Mesh2dIdx As Integer = -1
            Dim projected_coordinate_systemIdx As Integer = -1

            Dim mesh1d_edge_nodesIdx As Integer = -1
            Dim mesh1d_node_long_nameIdx As Integer = -1
            Dim mesh1d_node_idIdx As Integer = -1
            Dim mesh1d_edge_yIdx As Integer = -1
            Dim mesh1d_edge_xIdx As Integer = -1
            Dim mesh1d_edge_offsetIdx As Integer = -1
            Dim mesh1d_edge_branchIdx As Integer = -1
            Dim mesh1d_node_yIdx As Integer = -1
            Dim mesh1d_node_xIdx As Integer = -1
            Dim mesh1d_node_offsetIdx As Integer = -1
            Dim mesh1d_node_branchIdx As Integer = -1
            Dim mesh1dIdx As Integer = -1

            Dim network_branch_typeIdx As Integer = -1
            Dim network_branch_orderIdx As Integer = -1
            Dim network_geom_yIdx As Integer = -1
            Dim network_geom_xIdx As Integer = -1
            Dim network_geom_node_countIdx As Integer = -1
            Dim network_geometryIdx As Integer = -1
            Dim network_node_yIdx As Integer = -1
            Dim network_node_xIdx As Integer = -1
            Dim network_node_long_nameIdx As Integer = -1
            Dim network_node_idIdx As Integer = -1
            Dim network_edge_lengthIdx As Integer = -1
            Dim network_branch_long_nameIdx As Integer = -1
            Dim network_branch_idIdx As Integer = -1
            Dim network_edge_nodesIdx As Integer = -1
            Dim networkIdx As Integer = -1

            'network1d_node_id

            For i = 0 To DataSet.Variables.Count - 1
                Debug.Print(DataSet.Variables(i).Name)
                Select Case DataSet(i).Name.Trim.ToLower
                    Case Is = "mesh2d_face_z"
                        Mesh2d_face_zIdx = DataSet(i).ID
                    Case Is = "links_contact_type"
                        links_contact_typeIdx = DataSet(i).ID
                    Case Is = "links_contact_long_name"
                        links_contact_long_nameIdx = DataSet(i).ID
                    Case Is = "links_contact_id"
                        links_contact_idIdx = DataSet(i).ID
                    Case Is = "links"
                        linksIdx = DataSet(i).ID
                    Case Is = "mesh2d_interface_sigma"
                        Mesh2d_interface_sigmaIdx = DataSet(i).ID
                    Case Is = "mesh2d_layer_sigma"
                        Mesh2d_layer_sigmaIdx = DataSet(i).ID
                    Case Is = "mesh2d_face_y_bnd"
                        Mesh2d_face_y_bndIdx = DataSet(i).ID
                    Case Is = "mesh2d_face_x_bnd"
                        Mesh2d_face_x_bndIdx = DataSet(i).ID
                    Case Is = "mesh2d_face_y"
                        Mesh2d_face_yIdx = DataSet(i).ID
                    Case Is = "mesh2d_face_x"
                        Mesh2d_face_xIdx = DataSet(i).ID
                    Case Is = "mesh2d_face_nodes"
                        Mesh2d_face_nodesIdx = DataSet(i).ID
                    Case Is = "mesh2d_edge_nodes"
                        Mesh2d_edge_nodesIdx = DataSet(i).ID
                    Case Is = "mesh2d_edge_y"
                        Mesh2d_edge_yIdx = DataSet(i).ID
                    Case Is = "mesh2d_edge_x"
                        Mesh2d_edge_xIdx = DataSet(i).ID
                    Case Is = "mesh2d_node_z"
                        Mesh2d_node_zIdx = DataSet(i).ID
                    Case Is = "mesh2d_node_y"
                        Mesh2d_node_yIdx = DataSet(i).ID
                    Case Is = "mesh2d_node_x"
                        Mesh2d_node_xIdx = DataSet(i).ID
                    Case Is = "mesh2d"
                        Mesh2dIdx = DataSet(i).ID
                    Case Is = "projected_coordinate_system"
                        projected_coordinate_systemIdx = DataSet(i).ID
                    Case Is = "mesh1d_edge_nodes"
                        mesh1d_edge_nodesIdx = DataSet(i).ID
                    Case Is = "mesh1d_node_long_name"
                        mesh1d_node_long_nameIdx = DataSet(i).ID
                    Case Is = "mesh1d_node_id"
                        mesh1d_node_idIdx = DataSet(i).ID
                    Case Is = "mesh1d_edge_y"
                        mesh1d_edge_yIdx = DataSet(i).ID
                    Case Is = "mesh1d_edge_x"
                        mesh1d_edge_xIdx = DataSet(i).ID
                    Case Is = "mesh1d_edge_offset"
                        mesh1d_edge_offsetIdx = DataSet(i).ID
                    Case Is = "mesh1d_edge_branch"
                        mesh1d_edge_branchIdx = DataSet(i).ID
                    Case Is = "mesh1d_node_y"
                        mesh1d_node_yIdx = DataSet(i).ID
                    Case Is = "mesh1d_node_x"
                        mesh1d_node_xIdx = DataSet(i).ID
                    Case Is = "mesh1d_node_offset"
                        mesh1d_node_offsetIdx = DataSet(i).ID
                    Case Is = "mesh1d_node_branch"
                        mesh1d_node_branchIdx = DataSet(i).ID
                    Case Is = "mesh1d"
                        mesh1dIdx = DataSet(i).ID
                    Case Is = "network1d_branch_type", "network_branch_type"
                        network_branch_typeIdx = DataSet(i).ID
                    Case Is = "network1d_branch_order", "network_branch_order"
                        network_branch_orderIdx = DataSet(i).ID
                    Case Is = "network1d_geom_y", "network_geom_y"
                        network_geom_yIdx = DataSet(i).ID
                    Case Is = "network1d_geom_x", "network_geom_x"
                        network_geom_xIdx = DataSet(i).ID
                    Case Is = "network1d_geom_node_count", "network_geom_node_count"
                        network_geom_node_countIdx = DataSet(i).ID
                    Case Is = "network1d_geometry", "network_geometry"
                        network_geometryIdx = DataSet(i).ID
                    Case Is = "network1d_node_y", "network_node_y"
                        network_node_yIdx = DataSet(i).ID
                    Case Is = "network1d_node_x", "network_node_x"
                        network_node_xIdx = DataSet(i).ID
                    Case Is = "network1d_node_long_name", "network_node_long_name"
                        network_node_long_nameIdx = DataSet(i).ID
                    Case Is = "network1d_node_id", "network_node_id"
                        network_node_idIdx = DataSet(i).ID
                    Case Is = "network1d_edge_length", "network_edge_length"
                        network_edge_lengthIdx = DataSet(i).ID
                    Case Is = "network1d_branch_long_name", "network_branch_long_name"
                        network_branch_long_nameIdx = DataSet(i).ID
                    Case Is = "network1d_branch_id", "network_branch_id"
                        network_branch_idIdx = DataSet(i).ID
                    Case Is = "network1d_edge_nodes", "network_edge_nodes"
                        network_edge_nodesIdx = DataSet(i).ID
                    Case Is = "network1d", "network"
                        networkIdx = DataSet(i).ID
                End Select
            Next

            'read all variables describing our 1D network from the file
            'v2.3.4: added all the idx >= 0 clauses to prevent a crash when one of the datasets is not present
            If network_geom_yIdx >= 0 Then network1d_geom_y = DataSet.GetData(Of Double())(network_geom_yIdx)
            If network_geom_xIdx >= 0 Then network1d_geom_x = DataSet.GetData(Of Double())(network_geom_xIdx)
            If network_geom_node_countIdx >= 0 Then network1d_geom_node_count = DataSet.GetData(Of Int32())(network_geom_node_countIdx)
            If network_geometryIdx >= 0 Then network1d_geometry = DataSet.GetData(Of Int32)(network_geometryIdx)
            If network_edge_nodesIdx >= 0 Then network1d_edge_nodes = DataSet.GetData(Of Int32(,))(network_edge_nodesIdx)
            If network_branch_orderIdx >= 0 Then network1d_branch_order = DataSet.GetData(Of Int32())(network_branch_orderIdx)
            If network_edge_lengthIdx >= 0 Then network1d_edge_length = DataSet.GetData(Of Double())(network_edge_lengthIdx)
            If network_branch_long_nameIdx >= 0 Then network1d_branch_long_name = DataSet.GetData(Of Byte(,))(network_branch_long_nameIdx)
            If network_branch_idIdx >= 0 Then network1d_branch_id = DataSet.GetData(Of Byte(,))(network_branch_idIdx)
            If network_node_yIdx >= 0 Then network1d_node_y = DataSet.GetData(Of Double())(network_node_yIdx)
            If network_node_xIdx >= 0 Then network1d_node_x = DataSet.GetData(Of Double())(network_node_xIdx)
            If network_node_long_nameIdx >= 0 Then network1d_node_long_name = DataSet.GetData(Of Byte(,))(network_node_long_nameIdx)
            If network_node_idIdx >= 0 Then network1d_node_id = DataSet.GetData(Of Byte(,))(network_node_idIdx)
            If networkIdx >= 0 Then network1d = DataSet.GetData(Of Int32)(networkIdx)
            If mesh1d_node_offsetIdx >= 0 Then mesh1d_node_offset = DataSet.GetData(Of Double())(mesh1d_node_offsetIdx)
            If mesh1d_node_branchIdx >= 0 Then mesh1d_node_branch = DataSet.GetData(Of Int32())(mesh1d_node_branchIdx)
            If mesh1d_edge_yIdx >= 0 Then mesh1d_edge_y = DataSet.GetData(Of Double())(mesh1d_edge_yIdx)
            If mesh1d_edge_xIdx >= 0 Then mesh1d_edge_x = DataSet.GetData(Of Double())(mesh1d_edge_xIdx)
            If mesh1d_edge_offsetIdx >= 0 Then mesh1d_edge_offset = DataSet.GetData(Of Double())(mesh1d_edge_offsetIdx)
            If mesh1d_edge_branchIdx >= 0 Then mesh1d_edge_branch = DataSet.GetData(Of Int32())(mesh1d_edge_branchIdx)
            If mesh1d_edge_nodesIdx >= 0 Then mesh1d_edge_nodes = DataSet.GetData(Of Int32(,))(mesh1d_edge_nodesIdx)
            If mesh1d_node_long_nameIdx >= 0 Then mesh1d_node_long_name = DataSet.GetData(Of Byte(,))(mesh1d_node_long_nameIdx)
            If mesh1d_node_idIdx >= 0 Then mesh1d_node_id = DataSet.GetData(Of Byte(,))(mesh1d_node_idIdx)
            mesh1d = DataSet.GetData(Of Int32)(mesh1dIdx)

            'read the 2D network
            If Mesh2d_face_xIdx >= 0 Then mesh2d_face_x = DataSet.GetData(Of Double())(Mesh2d_face_xIdx)
            If Mesh2d_face_xIdx >= 0 Then mesh2d_face_y = DataSet.GetData(Of Double())(Mesh2d_face_yIdx)

            'read all 1D2D links
            If linksIdx >= 0 Then links = DataSet.GetData(Of Int32(,))(linksIdx)

            Me.Setup.Log.AddMessage("Networkfile successfully read: " & Path)

            If Not ReadReachNodes() Then Throw New Exception("Error reading the network's reach nodes (network1d_node).")
            If Not ReadReaches() Then Throw New Exception("Error reading the network's reaches")
            If Not ReadMeshNodes() Then Throw New Exception("Error reading the network's mesh nodes")
            If Not Read2DCellCenters() Then Throw New Exception("Error reading the network's 2D Cells")
            'If Not Read1D2DLinks() Then Throw New Exception("Error reading the network's 1D2D links")

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading network file: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function Read2DCellCenters() As Boolean
        'this function simply reads the cell center points (X, Y) for each 2D cell
        Try
            Dim myCellCenter As clsXY
            If mesh2d_face_x Is Nothing Then Return True
            For i = 0 To mesh2d_face_x.Count - 1
                myCellCenter = New clsXY(mesh2d_face_x(i), mesh2d_face_y(i))
                CellCenters2D.Add(i, myCellCenter)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function Read2DCellCenters: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadMeshNodes() As Boolean
        Try
            Dim IDArray As Byte()
            Dim ID As String
            Dim BranchNumber As Integer
            Dim Offset As Double

            If Branches Is Nothing OrElse Branches.Count = 0 Then Throw New Exception("Unable to read mesh before reading branches.")

            Dim LastIdx As Integer = -1

            For i = 0 To UBound(mesh1d_node_id)
                IDArray = Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(mesh1d_node_id, i)
                ID = Setup.GeneralFunctions.CharCodeBytesToString(IDArray, True)

                'v2.3.4: it looks like mesh1d_node_branch does not contain branch index numbers (0 based) but branch numbers (1 based)
                'so re retrieve the actual branch via branhces.values(number -1) from now on
                BranchNumber = mesh1d_node_branch(i)

                'v2.3.5: determine whether the array containing edge_nodes is 1-based or 0-based !!!!!!!!!!!
                'in order to find the correct branch we must first determine a correction in case the index = 1
                Dim BaseCorrection As Integer = 0
                BaseCorrection = start_index_correction(network1d_edge_nodes)

                Dim Branch As cls1DBranch = Branches.Values(BranchNumber + BaseCorrection)

                If Branch Is Nothing Then
                    Me.Setup.Log.AddError("Error assigning mesh node " & i & " to a branch. Associated branch with number " & BranchNumber & " not found.")
                Else
                    'now that we have our branch, figure out our chainage and XY coordinate and add our meshnode to the reach
                    Offset = mesh1d_node_offset(i)
                    Dim MeshNode As New cls1DMeshNode(ID, i, Offset, Branch)
                    Branch.AddMeshNode(MeshNode)
                End If

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadMeshNodes of class clsNetworkFile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetUpstreamBranches(refBranch As cls1DBranch, ByRef ProcessedBranchIDs As List(Of String), ByRef UpstreamBranches As List(Of cls1DBranch)) As Boolean
        Try
            'this function retrieves a list of all DIRECTLY upstream connected branches to a given the reference branch
            'provided the branch found is not yet present in the processedBranches dictionary
            UpstreamBranches = New List(Of cls1DBranch)
            For i = 0 To Branches.Count - 1
                If Not ProcessedBranchIDs.Contains(Branches.Values(i).ID) Then
                    If Branches.Values(i).en.ID.Trim.ToUpper = refBranch.bn.ID.Trim.ToUpper Then
                        UpstreamBranches.Add(Branches.Values(i))
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ReadReachNodes() As Boolean
        Try
            Dim IDArray As Byte()
            Dim ID As String
            Dim X As Double, Y As Double

            'start by reading the reachnodes
            For i = 0 To UBound(network1d_node_id)
                IDArray = Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(network1d_node_id, i)
                ID = Setup.GeneralFunctions.CharCodeBytesToString(IDArray, True)
                X = network1d_node_x(i)
                Y = network1d_node_y(i)

                Dim myNode As New cls1DNode(Me.Setup, ID)
                myNode.X = X
                myNode.Y = Y
                myNode.Idx = i
                Nodes1D.Add(myNode.ID.Trim.ToUpper, myNode)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadReachNodes of class clsNetworkFile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function Read1D2DLinks() As Boolean
        Try
            'array has two dimensions:
            'dimension 1 = each link
            'dimension 2 = resp. 2D cell and 1D node index
            Dim myLink As cls1D2DLink
            Dim MeshNodeFound As Boolean = False

            For i = 0 To UBound(links, 1)
                Debug.Print("i is " & i)

                myLink = New cls1D2DLink(Me.Setup)
                myLink.MeshNode1DIdx = links(i, 0)
                myLink.Cell2DIdx = links(i, 1)
                MeshNodeFound = False

                'assign the actual 1D node to our link
                For Each myBranch As cls1DBranch In Branches.Values
                    For Each myMeshNode As cls1DMeshNode In myBranch.MeshNodes.Values
                        If myMeshNode.Idx = myLink.MeshNode1DIdx Then
                            myLink.MeshNode1D = myMeshNode
                            MeshNodeFound = True
                            Exit For
                        End If
                        If MeshNodeFound Then Exit For
                    Next
                Next

                If CellCenters2D.ContainsKey(myLink.Cell2DIdx) Then
                    myLink.CellCenter2D = CellCenters2D.Item(myLink.Cell2DIdx)
                Else
                    Stop
                End If


                Links1D2D.Add(myLink)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function Read1D2DLinks of clss clsNetworkFile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function start_index_correction(ByRef myArray As Integer(,)) As Integer
        'applies a correction value for 2D-arrays where our array is 1-based instead of 0-based
        Try
            Dim j As Integer, k As Integer
            Dim start_index As Integer = 999
            For j = 0 To UBound(network1d_edge_nodes, 1)
                For k = 0 To UBound(network1d_edge_nodes, 2)
                    If network1d_edge_nodes(j, k) < start_index Then start_index = network1d_edge_nodes(j, k)
                Next
            Next
            Return -start_index
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function start_index_correction: " & ex.Message)
            Return 0
        End Try
    End Function


    Public Function ReadReaches() As Boolean
        Try
            Dim IDArray As Byte()
            Dim ID As String

            'every ID is constructed from 40 bytes. Transform this array to an actual ID here
            Dim vpstartidx As Integer = 0 'for every reach keep track of the starting position
            Dim vpidx As Integer = 0
            Dim prevChainage As Double, curChainage As Double

            'proceed with reading the reaches
            For i = 0 To UBound(network1d_branch_id, 1)

                IDArray = Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(network1d_branch_id, i)
                ID = Setup.GeneralFunctions.CharCodeBytesToString(IDArray, True)

                Dim myBranch As New cls1DBranch(Me.Setup, FlowFM, ID)
                myBranch.RouteNumber = network1d_branch_order(i)

                'v2.3.5: determine whether the array containing edge_node numbers is 1-based or 0-based
                'and set the appropriate correction for our internal 0-based implementation!!!!!!!!!!!
                Dim BaseCorrection As Integer = 0
                BaseCorrection = start_index_correction(network1d_edge_nodes)

                myBranch.bn = Nodes1D.Values(network1d_edge_nodes(i, 0) + BaseCorrection)
                myBranch.en = Nodes1D.Values(network1d_edge_nodes(i, 1) + BaseCorrection)

                If myBranch.en Is Nothing Then Me.Setup.Log.AddError("Error determining end node for branch " & myBranch.ID)
                If myBranch.bn Is Nothing Then Me.Setup.Log.AddError("Error determining begin node for reach " & myBranch.ID)

                'add the starting point as a vector
                prevChainage = 0
                curChainage = 0
                myBranch.AddVectorPoint(network1d_geom_x(vpstartidx), network1d_geom_y(vpstartidx), 0)    'add the first vector point
                'If network1d_geom_node_count(i) > 2 Then Stop
                'walk through all vector points
                For vpidx = vpstartidx + 1 To vpstartidx + network1d_geom_node_count(i) - 1
                    curChainage = prevChainage + Me.Setup.GeneralFunctions.Pythagoras(network1d_geom_x(vpidx - 1), network1d_geom_y(vpidx - 1), network1d_geom_x(vpidx), network1d_geom_y(vpidx))
                    myBranch.AddVectorPoint(network1d_geom_x(vpidx), network1d_geom_y(vpidx), curChainage)
                    prevChainage = curChainage
                Next

                'make sure for the next branch we start at the correct vectorpoint index
                vpstartidx += network1d_geom_node_count(i)

                Branches.Add(myBranch.ID.Trim.ToUpper, myBranch)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadReaches of class clsNetworkFile: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function GetCoordsFromChainage(BranchID As String, Chainage As Double, ByRef X As Double, ByRef Y As Double) As Boolean
        Try
            Dim Branch As cls1DBranch
            If Not Branches.ContainsKey(BranchID.Trim.ToUpper) Then Throw New Exception("Branch does not exist: " & BranchID)
            Branch = Branches.Item(BranchID.Trim.ToUpper)
            Branch.GetCoordsFromChainage(Chainage, X, Y)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error computing X and Y coordinate for a given branch and chainage: " & BranchID & ", " & Chainage & ":" & ex.Message)
            Return False
        End Try
    End Function




    Public Function GetFirstUpstreamMeshNode(BranchID As String, Chainage As Double, ByRef MeshNode As cls1DMeshNode) As Boolean
        Try
            Dim Branch As cls1DBranch
            Dim MinDist As Double = 9.0E+99
            Dim Found As Boolean = False
            MeshNode = Nothing  'initialize our byref node to be nothing
            If Not Branches.ContainsKey(BranchID.Trim.ToUpper) Then Throw New Exception("Branch not found: " & BranchID)
            Branch = Branches.Item(BranchID.Trim.ToLower)
            For Each myNode As cls1DMeshNode In Branch.MeshNodes.Values
                If myNode.Chainage <= Chainage Then
                    If (Chainage - myNode.Chainage) < MinDist Then
                        Found = True
                        MinDist = Chainage - myNode.Chainage
                        MeshNode = myNode
                    End If
                End If
            Next
            If Not Found Then Return False Else Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetFirstUpstreamMeshNode of class clsNetworkFile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function Find1DSnapLocation(ByVal X As Double, ByVal Y As Double, ByVal SearchRadius As Double, ByRef snapBranch As cls1DBranch, ByRef snapChainage As Double, ByRef snapDistance As Double, ExcludeBranches As List(Of String)) As Boolean
        Try
            Dim nearestDist As Double = 9.9E+100
            Dim reachSnapDist As Double = 9.9E+100, reachSnapChainage As Double
            Dim DistToBoundingBox As Double
            Dim Found As Boolean = False

            For Each myBranch As cls1DBranch In Branches.Values
                If Not ExcludeBranches.Contains(myBranch.ID) Then

                    'calculate the bounding box for our current reach
                    If myBranch.BoundingBox Is Nothing Then myBranch.calcBoundingBox()

                    If X >= myBranch.BoundingBox.XLL - SearchRadius AndAlso X <= myBranch.BoundingBox.XUR + SearchRadius AndAlso Y >= myBranch.BoundingBox.YLL - SearchRadius AndAlso Y <= myBranch.BoundingBox.YUR + SearchRadius Then
                        'ok, we've found a reach that matches the criteria, but is it worth to actually find the snapping point? 
                        'only if the distance to the extent is smaller than the nearest distance found so far!
                        DistToBoundingBox = myBranch.BoundingBox.calcDistance(X, Y)
                        If DistToBoundingBox <= nearestDist Then 'important: this MUST be <=, not < because any point INSIDE the bounding box gets value 0 and must be tried anyhow
                            If myBranch.CalcSnapLocation(X, Y, SearchRadius, reachSnapChainage, reachSnapDist) Then
                                If reachSnapDist <= SearchRadius AndAlso reachSnapDist < nearestDist Then
                                    Found = True
                                    nearestDist = reachSnapDist
                                    snapDistance = reachSnapDist
                                    snapChainage = reachSnapChainage
                                    snapBranch = myBranch
                                End If
                            End If
                        End If
                    End If
                End If
            Next
            Return Found
        Catch ex As Exception
            Me.Setup.Log.AddError("Error snapping location to branches.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



    Public Function Find1DSnapLocationVia1D2DLinks(ByVal X As Double, ByVal Y As Double, ByVal SearchRadius As Double, ByRef snapBranch As cls1DBranch, ByRef snapChainage As Double, ByRef snapDistance As Double) As Boolean
        'this function finds the snappingpoint on 1D from a given start position X, Y
        'it does so by following the 1D2D links
        Try
            Dim nearestDist As Double = 9.9E+100
            Dim CurDist As Double
            Dim CurLink As cls1D2DLink = Nothing
            Dim Found As Boolean = False

            For Each myLink As cls1D2DLink In Links1D2D
                CurDist = Me.Setup.GeneralFunctions.Pythagoras(myLink.CellCenter2D.X, myLink.CellCenter2D.Y, X, Y)
                If CurDist < SearchRadius AndAlso CurDist < nearestDist Then
                    nearestDist = CurDist
                    CurLink = myLink
                    Found = True
                End If
            Next

            'now that we found our nearest link, figure out the branch and chainage of the meshnode
            If Found Then
                snapChainage = CurLink.MeshNode1D.Chainage
                snapBranch = CurLink.MeshNode1D.Branch
            End If

            Return Found
        Catch ex As Exception
            Me.Setup.Log.AddError("Error snapping location to branches.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


End Class
