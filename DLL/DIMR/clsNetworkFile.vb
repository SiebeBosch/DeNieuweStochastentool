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

            'read all variables describing our 1D network from the file
            network1d_geom_y = DataSet.GetData(Of Double())(24)
            network1d_geom_x = DataSet.GetData(Of Double())(23)
            network1d_geom_node_count = DataSet.GetData(Of Int32())(22)
            network1d_geometry = DataSet.GetData(Of Int32)(21)
            network1d_edge_nodes = DataSet.GetData(Of Int32(,))(20)
            network1d_branch_order = DataSet.GetData(Of Int32())(19)
            network1d_edge_length = DataSet.GetData(Of Double())(18)
            network1d_branch_long_name = DataSet.GetData(Of Byte(,))(17)
            network1d_branch_id = DataSet.GetData(Of Byte(,))(16)
            network1d_node_y = DataSet.GetData(Of Double())(15)
            network1d_node_x = DataSet.GetData(Of Double())(14)
            network1d_node_long_name = DataSet.GetData(Of Byte(,))(13)
            network1d_node_id = DataSet.GetData(Of Byte(,))(12)
            network1d = DataSet.GetData(Of Int32)(11)
            mesh1d_node_offset = DataSet.GetData(Of Double())(10)
            mesh1d_node_branch = DataSet.GetData(Of Int32())(9)
            mesh1d_edge_y = DataSet.GetData(Of Double())(8)
            mesh1d_edge_x = DataSet.GetData(Of Double())(7)
            mesh1d_edge_offset = DataSet.GetData(Of Double())(6)
            mesh1d_edge_branch = DataSet.GetData(Of Int32())(5)
            mesh1d_edge_nodes = DataSet.GetData(Of Int32(,))(4)
            mesh1d_node_long_name = DataSet.GetData(Of Byte(,))(3)
            mesh1d_node_id = DataSet.GetData(Of Byte(,))(2)
            mesh1d = DataSet.GetData(Of Int32)(1)

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
