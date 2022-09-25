Option Explicit On
Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative
Imports STOCHLIB.General


Public Class clsFouNCFile
    Private Setup As clsSetup
    Friend Path As String


    Dim Variables As sds.ReadOnlyVariableCollection

    Dim Mesh2d_fourier001_maxID As Integer = -1
    Dim Mesh1d_fourier001_maxID As Integer = -1
    Dim Mesh2d_fourier002_maxID As Integer = -1
    Dim Mesh1d_fourier002_maxID As Integer = -1
    Dim Mesh2d_face_xID As Integer = -1
    Dim Mesh2d_face_yID As Integer = -1
    Dim Mesh1d_node_xID As Integer = -1
    Dim Mesh1d_node_yID As Integer = -1
    Dim Mesh1d_node_idID As Integer = -1
    Dim Mesh1d_node_long_nameID As Integer = -1

    'define all variables for the data
    Dim Mesh2d_fourier001_max As Double()       'size: number of faces
    Dim Mesh1d_fourier001_max As Double()       'size: number of nodes
    Dim Mesh2d_fourier002_max As Double()       'size: number of faces
    Dim Mesh1d_fourier002_max As Double()       'size: number of nodes
    Friend Mesh2d_face_x As Double()       'size: number of faces
    Friend Mesh2d_face_y As Double()       'size: number of faces
    Dim Mesh1d_node_x As Double()       'size: number of nodes
    Dim Mesh1d_node_y As Double()       'size: number of nodes

    Dim Mesh1d_node_id As Byte(,)      'since this parameter is 2D (40 bytes per id) we also create an array with all ids
    Dim Mesh1d_node_ids As String()

    Dim Mesh1d_node_long_name As Byte(,)
    Dim Mesh1d_node_long_names As String()


    Public Sub New(myPath As String, ByRef mySetup As clsSetup)
        Path = myPath
        Setup = mySetup
    End Sub

    Public Function get2DMaximumWaterLevels() As Double()
        'returns the maximum water level per 2D cell
        Return Mesh2d_fourier002_max
    End Function

    Public Function Read() As Boolean
        Try
            'note: which variable is stored wehre in the fourier file is specified in the fm-folder in the .fou file
            'e.g.:
            '*var tsrts   sstop   numcyc  knfac   v0plu   layno   elp    
            ' wl  21600 - 1      0       1.0     0.0             min    
            ' wl  21600 - 1      0       1.0     0.0             max    
            ' uc  21600 - 1      0       1.0     0.0     1.0     max    
            ' in this example it's the second layer (mesh2d_fourier002_max) that contains the maximum waterlevels
            ' to do: implement a functionality to read this file and select the correct variable from our Fou.nc file.


            Dim dataset = sds.DataSet.Open(Path & "?openMode=readOnly")
            Dim myDataset As sds.DataSet() = dataset.GetLinkedDataSets
            Dim myDimensions As sds.ReadOnlyDimensionList = dataset.Dimensions
            Variables = dataset.Variables


            'set the names for all variables
            For i = 0 To DataSet.Variables.Count - 1
                Debug.Print(DataSet.Variables.Item(i).Name)
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier001_max" Then Mesh2d_fourier001_maxID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh1d_fourier001_max" Then Mesh1d_fourier001_maxID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier002_max" Then Mesh2d_fourier002_maxID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh1d_fourier002_max" Then Mesh1d_fourier002_maxID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh2d_face_x" Then Mesh2d_face_xID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh2d_face_y" Then Mesh2d_face_yID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_x" Then Mesh1d_node_xID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_y" Then Mesh1d_node_yID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_id" Then Mesh1d_node_idID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_long_name" Then Mesh1d_node_long_nameID = DataSet.Variables.Item(i).ID
                If DataSet.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier001_max" Then Mesh2d_fourier001_maxID = DataSet.Variables.Item(i).ID
            Next

            'and read the fourier file's content!
            If Mesh2d_fourier001_maxID >= 0 Then Mesh2d_fourier001_max = DataSet.GetData(Of Double())(Mesh2d_fourier001_maxID)
            If Mesh1d_fourier001_maxID >= 0 Then Mesh1d_fourier001_max = DataSet.GetData(Of Double())(Mesh1d_fourier001_maxID)
            If Mesh2d_fourier002_maxID >= 0 Then Mesh2d_fourier002_max = DataSet.GetData(Of Double())(Mesh2d_fourier002_maxID)
            If Mesh1d_fourier002_maxID >= 0 Then Mesh1d_fourier002_max = DataSet.GetData(Of Double())(Mesh1d_fourier002_maxID)
            If Mesh2d_face_xID >= 0 Then Mesh2d_face_x = DataSet.GetData(Of Double())(Mesh2d_face_xID)
            If Mesh2d_face_yID >= 0 Then Mesh2d_face_y = DataSet.GetData(Of Double())(Mesh2d_face_yID)
            If Mesh1d_node_xID >= 0 Then Mesh1d_node_x = DataSet.GetData(Of Double())(Mesh1d_node_xID)
            If Mesh1d_node_yID >= 0 Then Mesh1d_node_y = DataSet.GetData(Of Double())(Mesh1d_node_yID)
            If Mesh1d_node_idID >= 0 Then Mesh1d_node_id = DataSet.GetData(Of Byte(,))(Mesh1d_node_idID)
            If Mesh1d_node_long_nameID >= 0 Then Mesh1d_node_long_name = DataSet.GetData(Of Byte(,))(Mesh1d_node_long_nameID)

            'de id's zijn samengesteld uit een array van bytes
            Dim IDArray As Byte()
            ReDim Mesh1d_node_ids(UBound(Mesh1d_node_id, 1))
            For i = 0 To UBound(Mesh1d_node_id, 1)
                IDArray = Me.Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(Mesh1d_node_id, i)
                Mesh1d_node_ids(i) = Me.Setup.GeneralFunctions.CharCodeBytesToString(IDArray, True)
            Next

            'idem voor de long_names
            Dim LongNameArray As Byte()
            ReDim Mesh1d_node_long_names(UBound(Mesh1d_node_long_name, 1))
            For i = 0 To UBound(Mesh1d_node_long_name, 1)
                LongNameArray = Me.Setup.GeneralFunctions.GetRowFrom2DArrayOfByte(Mesh1d_node_long_name, i)
                Mesh1d_node_long_names(i) = Me.Setup.GeneralFunctions.CharCodeBytesToString(LongNameArray, True)
            Next

            Return True
        Catch ex As Exception
            Return True
        End Try
    End Function

End Class
