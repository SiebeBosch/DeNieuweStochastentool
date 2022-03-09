Imports Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative
Imports STOCHLIB.General

Public Class clsNetworkFile
    Private Setup As clsSetup
    Friend Path As String

    Dim Branches As New Dictionary(Of String, cls1DBranch)
    Dim Nodes As New Dictionary(Of String, cls1DNode)

    'all variables describing the network file's content

    'this block reads all variables concerning our 1D network from the file
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



    Public Sub New(myPath As String, ByRef mySetup As clsSetup)
        Path = myPath
        Setup = mySetup
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

            For i = 0 To DataSet.Variables.Count - 1
                Debug.Print(DataSet.Variables(i).Name)
                Select Case DataSet(i).Name
                    Case Is = "Mesh2d_face_z"
                        Mesh2d_face_zIdx = DataSet(i).ID
                    Case Is = "links_contact_type"
                        links_contact_typeIdx = DataSet(i).ID
                    Case Is = "links_contact_long_name"
                        links_contact_long_nameIdx = DataSet(i).ID
                    Case Is = "links_contact_id"
                        links_contact_idIdx = DataSet(i).ID
                    Case Is = "links"
                        linksIdx = DataSet(i).ID
                    Case Is = "Mesh2d_interface_sigma"
                        Mesh2d_interface_sigmaIdx = DataSet(i).ID
                    Case Is = "Mesh2d_layer_sigma"
                        Mesh2d_layer_sigmaIdx = DataSet(i).ID
                    Case Is = "Mesh2d_face_y_bnd"
                        Mesh2d_face_y_bndIdx = DataSet(i).ID
                    Case Is = "Mesh2d_face_x_bnd"
                        Mesh2d_face_x_bndIdx = DataSet(i).ID
                    Case Is = "Mesh2d_face_y"
                        Mesh2d_face_yIdx = DataSet(i).ID
                    Case Is = "Mesh2d_face_x"
                        Mesh2d_face_xIdx = DataSet(i).ID
                    Case Is = "Mesh2d_face_nodes"
                        Mesh2d_face_nodesIdx = DataSet(i).ID
                    Case Is = "Mesh2d_edge_nodes"
                        Mesh2d_edge_nodesIdx = DataSet(i).ID
                    Case Is = "Mesh2d_edge_y"
                        Mesh2d_edge_yIdx = DataSet(i).ID
                    Case Is = "Mesh2d_edge_x"
                        Mesh2d_edge_xIdx = DataSet(i).ID
                    Case Is = "Mesh2d_node_z"
                        Mesh2d_node_zIdx = DataSet(i).ID
                    Case Is = "Mesh2d_node_y"
                        Mesh2d_node_yIdx = DataSet(i).ID
                    Case Is = "Mesh2d_node_x"
                        Mesh2d_node_xIdx = DataSet(i).ID
                    Case Is = "Mesh2d"
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
                    Case Is = "network_branch_type"
                        network_branch_typeIdx = DataSet(i).ID
                    Case Is = "network_branch_order"
                        network_branch_orderIdx = DataSet(i).ID
                    Case Is = "network_geom_y"
                        network_geom_yIdx = DataSet(i).ID
                    Case Is = "network_geom_x"
                        network_geom_xIdx = DataSet(i).ID
                    Case Is = "network_geom_node_count"
                        network_geom_node_countIdx = DataSet(i).ID
                    Case Is = "network_geometry"
                        network_geometryIdx = DataSet(i).ID
                    Case Is = "network_node_y"
                        network_node_yIdx = DataSet(i).ID
                    Case Is = "network_node_x"
                        network_node_xIdx = DataSet(i).ID
                    Case Is = "network_node_long_name"
                        network_node_long_nameIdx = DataSet(i).ID
                    Case Is = "network_node_id"
                        network_node_idIdx = DataSet(i).ID
                    Case Is = "network_edge_length"
                        network_edge_lengthIdx = DataSet(i).ID
                    Case Is = "network_branch_long_name"
                        network_branch_long_nameIdx = DataSet(i).ID
                    Case Is = "network_branch_id"
                        network_branch_idIdx = DataSet(i).ID
                    Case Is = "network_edge_nodes"
                        network_edge_nodesIdx = DataSet(i).ID
                    Case Is = "network"
                        networkIdx = DataSet(i).ID
                End Select
            Next

            'read all variables describing our 1D network from the file
            network1d_geom_y = DataSet.GetData(Of Double())(network_geom_yIdx)
            network1d_geom_x = DataSet.GetData(Of Double())(network_geom_xIdx)
            network1d_geom_node_count = DataSet.GetData(Of Int32())(network_geom_node_countIdx)
            network1d_geometry = DataSet.GetData(Of Int32)(network_geometryIdx)
            network1d_edge_nodes = DataSet.GetData(Of Int32(,))(network_edge_nodesIdx)
            network1d_branch_order = DataSet.GetData(Of Int32())(network_branch_orderIdx)
            network1d_edge_length = DataSet.GetData(Of Double())(network_edge_lengthIdx)
            network1d_branch_long_name = DataSet.GetData(Of Byte(,))(network_branch_long_nameIdx)
            network1d_branch_id = DataSet.GetData(Of Byte(,))(network_branch_idIdx)
            network1d_node_y = DataSet.GetData(Of Double())(network_node_yIdx)
            network1d_node_x = DataSet.GetData(Of Double())(network_node_xIdx)
            network1d_node_long_name = DataSet.GetData(Of Byte(,))(network_node_long_nameIdx)
            network1d_node_id = DataSet.GetData(Of Byte(,))(network_node_idIdx)
            network1d = DataSet.GetData(Of Int32)(networkIdx)
            mesh1d_node_offset = DataSet.GetData(Of Double())(mesh1d_node_offsetIdx)
            mesh1d_node_branch = DataSet.GetData(Of Int32())(mesh1d_node_branchIdx)
            mesh1d_edge_y = DataSet.GetData(Of Double())(mesh1d_edge_yIdx)
            mesh1d_edge_x = DataSet.GetData(Of Double())(mesh1d_edge_xIdx)
            mesh1d_edge_offset = DataSet.GetData(Of Double())(mesh1d_edge_offsetIdx)
            mesh1d_edge_branch = DataSet.GetData(Of Int32())(mesh1d_edge_branchIdx)
            mesh1d_edge_nodes = DataSet.GetData(Of Int32(,))(mesh1d_edge_nodesIdx)
            mesh1d_node_long_name = DataSet.GetData(Of Byte(,))(mesh1d_node_long_nameIdx)
            mesh1d_node_id = DataSet.GetData(Of Byte(,))(mesh1d_node_idIdx)
            mesh1d = DataSet.GetData(Of Int32)(Mesh1DIdx)

            Me.Setup.Log.AddMessage("Networkfile successfully read: " & Path)

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
                Nodes.Add(myNode.ID.Trim.ToUpper, myNode)
            Next

            'every ID is constructed from 40 bytes. Transform this array to an actual ID here
            Dim vpstartidx As Integer = 0 'for every reach keep track of the starting position
            Dim vpidx As Integer = 0
            Dim prevChainage As Double, curChainage As Double


            For i = 0 To UBound(network1d_branch_id, 1) - 1

                IDArray = Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(network1d_branch_id, i)
                ID = Setup.GeneralFunctions.CharCodeBytesToString(IDArray, True)

                Dim myBranch As New cls1DBranch(Me.Setup, ID)
                myBranch.RouteNumber = network1d_branch_order(i)

                'let op: de array network1d_edge_nodes lijkt alleen cijfers > 0 te bevatten. Dit suggereert dat hij niet 0-based is maar 1-based
                myBranch.bn = Nodes.Values(network1d_edge_nodes(i, 0) - 1)  '-1 is needed because network1d_edge_nodes refers to node numbers, not index numbers
                myBranch.en = Nodes.Values(network1d_edge_nodes(i, 1) - 1)  '-1 is needed because network1d_edge_nodes refers to node numbers, not index numbers

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

                'and finally add the branch
                Branches.Add(myBranch.ID.Trim.ToUpper, myBranch)
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading network file: " & ex.Message)
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


End Class
