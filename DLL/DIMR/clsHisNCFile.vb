Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative
Imports STOCHLIB.General
Public Class clsHisNCFile
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

    Public Function ReadResultsAtObservationPoints(ByRef Waterlevels As Double(,), ByRef Discharges As Double(,), ByRef Times As Double(), ByRef IDList As String()) As Boolean
        Try
            'we use the microsoft scientific dataset library to read this netcdf-file
            'returns byref the results:
            'waterlevels contains a 2D array where the first dimension contains the timesteps and the second the object index
            'IDList contains an array of strings

            Dim WaterLevelIDIdx As Integer
            Dim DischargeIDIdx As Integer
            Dim TimestepIDIdx As Integer
            Dim TimeIDIdx As Integer
            Dim StationIDIdx As Integer
            Dim i As Integer

            'Dim dataset = sds.DataSet.Open(Path & "?openMode=readOnly")
            Dim dataset = sds.DataSet.Open(Path)
            Dim myDataset As sds.DataSet() = dataset.GetLinkedDataSets
            Dim myDimensions As sds.ReadOnlyDimensionList = dataset.Dimensions
            Dim myVariables As sds.ReadOnlyVariableCollection = dataset.Variables

            For i = 0 To dataset.Variables.Count - 1
                If dataset.Variables.Item(i).Name = "waterlevel" Then WaterLevelIDIdx = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name = "discharge_magnitude" Then DischargeIDIdx = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name = "timestep" Then TimestepIDIdx = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name = "station_id" Then StationIDIdx = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name = "timestep" Then TimestepIDIdx = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name = "time" Then TimeIDIdx = dataset.Variables.Item(i).ID
            Next

            Waterlevels = dataset.GetData(Of Double(,))(WaterLevelIDIdx)
            Discharges = dataset.GetData(Of Double(,))(DischargeIDIdx)
            Dim ObservationPointIDs As Byte(,) = dataset.GetData(Of Byte(,))(StationIDIdx)
            Dim TimeStamps As Double() = dataset.GetData(Of Double())(TimestepIDIdx)
            Times = dataset.GetData(Of Double())(TimeIDIdx)                'times are expressed in seconds w.r.t. the reference date as stated in the .MDU file

            'de id's zijn samengesteld uit een array van bytes
            Dim IDArray As Byte()
            ReDim IDList(UBound(ObservationPointIDs, 1))
            For i = 0 To UBound(ObservationPointIDs, 1)
                IDArray = Me.Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(ObservationPointIDs, i)
                IDList(i) = Me.Setup.GeneralFunctions.CharCodeBytesToString(IDArray, True)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadResultsAtObservationPoints of class clsHisNCFile: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function ReadDischargeAtStructures(StructureType As String, ByRef Discharges As Double(,), ByRef Times As Double(), ByRef IDList As String()) As Boolean
        Try
            'UNTESTED. There are multiple variables having discharge (e.g. weir, culvert, etc.)


            'we use the microsoft scientific dataset library to read this netcdf-file
            'returns byref the results:
            'discharges contains a 2D array where the first dimension contains the timesteps and the second the object index
            'IDList contains an array of strings

            Dim DischargeIDIdx As Integer = -1
            Dim TimestepIDIdx As Integer
            Dim TimeIDIdx As Integer
            Dim StationIDIdx As Integer
            Dim i As Integer

            'Dim dataset = sds.DataSet.Open(Path & "?openMode=readOnly")
            Dim dataset = sds.DataSet.Open(Path)
            Dim myDataset As sds.DataSet() = dataset.GetLinkedDataSets
            Dim myDimensions As sds.ReadOnlyDimensionList = dataset.Dimensions
            Dim myVariables As sds.ReadOnlyVariableCollection = dataset.Variables

            For i = 0 To dataset.Variables.Count - 1
                If dataset.Variables.Item(i).Name.Trim.ToLower.Contains("discharge") AndAlso dataset.Variables.Item(i).Name.Trim.ToLower.Contains(StructureType.Trim.ToLower) Then DischargeIDIdx = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name = "timestep" Then TimestepIDIdx = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name = "station_id" Then StationIDIdx = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name = "timestep" Then TimestepIDIdx = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name = "time" Then TimeIDIdx = dataset.Variables.Item(i).ID
            Next

            If DischargeIDIdx = -1 Then
                Throw New Exception($"Error in function ReadDischargesAtStructures of class clsHisNCFile: Discharge variable for structure type {StructureType} not found")
            End If

            Discharges = dataset.GetData(Of Double(,))(DischargeIDIdx)
            Dim StructureIDs As Byte(,) = dataset.GetData(Of Byte(,))(StationIDIdx)
            Dim TimeStamps As Double() = dataset.GetData(Of Double())(TimestepIDIdx)
            Times = dataset.GetData(Of Double())(TimeIDIdx)                'times are expressed in seconds w.r.t. the reference date as stated in the .MDU file

            'de id's zijn samengesteld uit een array van bytes
            Dim IDArray As Byte()
            ReDim IDList(UBound(StructureIDs, 1))
            For i = 0 To UBound(StructureIDs, 1)
                IDArray = Me.Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(StructureIDs, i)
                IDList(i) = Me.Setup.GeneralFunctions.CharCodeBytesToString(IDArray, True)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReadDischargesAtStructures of class clsHisNCFile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadStochasticResults(ID As String, Parameter As String, ByRef MaxInLastTimeStep As Boolean, ByRef Min As Double, ByRef tsMin As Integer, ByRef Max As Double, ByRef tsMax As Integer, ByRef Avg As Double, ResultsStartPercentage As Double) As Boolean
        Return True
    End Function



End Class
