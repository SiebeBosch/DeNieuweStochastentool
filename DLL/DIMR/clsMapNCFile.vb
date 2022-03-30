Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative
Imports STOCHLIB.General
Public Class clsMapNCFile
    Private Setup As clsSetup
    Friend Path As String


    Dim Branches As New Dictionary(Of String, cls1DBranch)
    Dim Nodes As New Dictionary(Of String, cls1DNode)

    'all variables describing the results file's content (timeseries)

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


    Public Function GetWaterlevelsForMeshNode(ID As String, ByRef Results As Double()) As Boolean
        Try
            'we use the microsoft scientific dataset library to read this netcdf-file
            'returns byref the results:
            'waterlevels contains a 2D array where the first dimension contains the timesteps and the second the object index
            'IDList contains an array of strings

            'Dim WaterLevelID As Integer
            'Dim TimestepID As Integer
            'Dim StationID As Integer
            'Dim i As Integer

            Dim dataset = sds.DataSet.Open(Path & "?openMode=readOnly")
            Dim myDataset As sds.DataSet() = dataset.GetLinkedDataSets
            Dim myDimensions As sds.ReadOnlyDimensionList = dataset.Dimensions
            Dim myVariables As sds.ReadOnlyVariableCollection = dataset.Variables
            Dim Waterlevels(,) As Double
            Dim IDArray As Byte()
            Dim IDList As String()

            Stop

            Dim WaterLevelID As String = ""
            Dim MeshNodeID As String = ""

            For i = 0 To dataset.Variables.Count - 1
                Debug.Print(dataset.Variables.Item(i).Name)
                If dataset.Variables.Item(i).Name = "mesh1d_waterlevel" Then
                    WaterLevelID = dataset.Variables.Item(i).ID
                ElseIf dataset.Variables.Item(i).Name = "mesh1d_node_id" Then
                    MeshNodeID = dataset.Variables.Item(i).ID
                End If
            Next
            Waterlevels = dataset.GetData(Of Double(,))(WaterLevelID)  'first dimension = timestep, second = meshnodes
            Dim MeshNodeIDs As Byte(,) = dataset.GetData(Of Byte(,))(MeshNodeID)
            ReDim IDList(UBound(MeshNodeIDs, 1))
            For i = 0 To UBound(MeshNodeIDs, 1)
                IDArray = Me.Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(MeshNodeIDs, i)
                IDList(i) = Me.Setup.GeneralFunctions.CharCodeBytesToString(IDArray, True)
            Next

            Dim myID As String
            Dim ts As Integer
            Dim locidx As Integer
            For locidx = 0 To IDList.Count - 1
                myID = IDList(locidx)
                If myID.Trim.ToUpper = ID.Trim.ToUpper Then
                    'found our ID!
                    ReDim Results(0 To UBound(Waterlevels, 1))
                    For ts = 0 To UBound(Waterlevels, 1)
                        Results(ts) = Waterlevels(ts, locidx)
                    Next
                    Return True
                End If
            Next
            Throw New Exception("Unable to retrieve waterlevels for meshnode " & ID)
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadWaterLevelsAtObservationPoints of class clsHisNCFile: " & ex.Message)
            Return False
        End Try
    End Function

End Class
