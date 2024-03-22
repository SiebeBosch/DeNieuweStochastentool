Option Explicit On
Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative
Imports STOCHLIB.General
Imports DocumentFormat.OpenXml.InkML
Imports System.IO
Imports MapWinGIS
Imports Ionic
Imports System.Security.Cryptography
Imports DocumentFormat.OpenXml.Drawing.Diagrams
Imports DocumentFormat.OpenXml

Public Class clsFouNCFile
    Private Setup As clsSetup
    Friend Path As String


    Dim Variables As sds.ReadOnlyVariableCollection

    Dim Mesh2d_fourier001_maxID As Integer = -1
    Dim Mesh1d_fourier001_maxID As Integer = -1
    Dim Mesh2d_fourier002_maxID As Integer = -1
    Dim Mesh2d_fourier002_maxdepthID As Integer = -1
    Dim Mesh1d_fourier002_maxID As Integer = -1
    Dim Mesh2d_face_xID As Integer = -1
    Dim Mesh2d_face_yID As Integer = -1
    Dim Mesh2d_node_xID As Integer = -1
    Dim Mesh2d_node_yID As Integer = -1
    Dim Mesh2d_face_nodesID As Integer = -1
    Dim Mesh1d_node_xID As Integer = -1
    Dim Mesh1d_node_yID As Integer = -1
    Dim Mesh1d_node_idID As Integer = -1
    Dim Mesh1d_node_long_nameID As Integer = -1

    'define all variables for the data
    Dim Mesh2d_fourier001_max As Double()       'size: number of faces
    Dim Mesh1d_fourier001_max As Double()       'size: number of nodes
    Dim Mesh2d_fourier002_max As Double()       'size: number of faces
    Dim Mesh2d_fourier002_max_depth As Double()       'size: number of faces
    Dim Mesh1d_fourier002_max As Double()       'size: number of nodes
    Friend Mesh2d_face_x As Double()       'size: number of faces
    Friend Mesh2d_face_y As Double()       'size: number of faces
    Friend Mesh2d_node_x As Double()       'x-coordinate of mesh nodes
    Friend Mesh2d_node_y As Double()       'y-coordianate of mesh nodes
    Friend Mesh2d_face_nodes As Integer(,) 'first dimension; face index, second dimension: node index, counted counterclockwise
    Friend Mesh2d_face_nodes_start_index As Integer 'the start index for nodes in this collection
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

    Public Function get2DMaxima(Parameter As GeneralFunctions.enm2DParameter) As Double()
        'returns the maximum water level per 2D cell
        Select Case Parameter
            Case GeneralFunctions.enm2DParameter.depth
                Return Mesh2d_fourier002_max_depth
            Case GeneralFunctions.enm2DParameter.waterlevel
                Return Mesh2d_fourier002_max
        End Select
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
            For i = 0 To dataset.Variables.Count - 1
                Debug.Print(dataset.Variables.Item(i).Name)
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_x" Then Mesh1d_node_xID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_y" Then Mesh1d_node_yID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_id" Then Mesh1d_node_idID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_node_long_name" Then Mesh1d_node_long_nameID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_fourier001_max" Then Mesh1d_fourier001_maxID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier002_max" Then Mesh2d_fourier002_maxID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier002_max_depth" Then Mesh2d_fourier002_maxdepthID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh1d_fourier002_max" Then Mesh1d_fourier002_maxID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier001_max" Then Mesh2d_fourier001_maxID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_face_x" Then Mesh2d_face_xID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_face_y" Then Mesh2d_face_yID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_face_nodes" Then Mesh2d_face_nodesID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_node_x" Then Mesh2d_node_xID = dataset.Variables.Item(i).ID
                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_node_y" Then Mesh2d_node_yID = dataset.Variables.Item(i).ID

                If dataset.Variables.Item(i).Name.Trim.ToLower = "mesh2d_fourier001_max" Then Mesh2d_fourier001_maxID = dataset.Variables.Item(i).ID
            Next

            'and read the fourier file's content!
            If Mesh2d_fourier001_maxID >= 0 Then Mesh2d_fourier001_max = dataset.GetData(Of Double())(Mesh2d_fourier001_maxID)
            If Mesh1d_fourier001_maxID >= 0 Then Mesh1d_fourier001_max = dataset.GetData(Of Double())(Mesh1d_fourier001_maxID)
            If Mesh2d_fourier002_maxID >= 0 Then Mesh2d_fourier002_max = dataset.GetData(Of Double())(Mesh2d_fourier002_maxID)
            If Mesh2d_fourier002_maxdepthID >= 0 Then Mesh2d_fourier002_max_depth = dataset.GetData(Of Double())(Mesh2d_fourier002_maxdepthID)
            If Mesh1d_fourier002_maxID >= 0 Then Mesh1d_fourier002_max = dataset.GetData(Of Double())(Mesh1d_fourier002_maxID)
            If Mesh2d_face_xID >= 0 Then Mesh2d_face_x = dataset.GetData(Of Double())(Mesh2d_face_xID)
            If Mesh2d_face_yID >= 0 Then Mesh2d_face_y = dataset.GetData(Of Double())(Mesh2d_face_yID)
            If Mesh2d_face_nodesID >= 0 Then
                Mesh2d_face_nodes = dataset.GetData(Of Integer(,))(Mesh2d_face_nodesID)
                Mesh2d_face_nodes_start_index = GetStartIndex(Mesh2d_face_nodes)            'strangely we cannot find the start_index in the metadata although panoply sais it's there. Hence this function
                'If dataset.Metadata.ContainsKey("start_index") Then Mesh2d_face_nodes_start_index = dataset.Metadata.Item("start_index")
            End If
            If Mesh2d_node_xID >= 0 Then Mesh2d_node_x = dataset.GetData(Of Double())(Mesh2d_node_xID)
            If Mesh2d_node_yID >= 0 Then Mesh2d_node_y = dataset.GetData(Of Double())(Mesh2d_node_yID)
            If Mesh1d_node_xID >= 0 Then Mesh1d_node_x = dataset.GetData(Of Double())(Mesh1d_node_xID)
            If Mesh1d_node_yID >= 0 Then Mesh1d_node_y = dataset.GetData(Of Double())(Mesh1d_node_yID)
            If Mesh1d_node_idID >= 0 Then Mesh1d_node_id = dataset.GetData(Of Byte(,))(Mesh1d_node_idID)
            If Mesh1d_node_long_nameID >= 0 Then Mesh1d_node_long_name = dataset.GetData(Of Byte(,))(Mesh1d_node_long_nameID)

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

    Public Function GetStartIndex(data As Integer(,)) As Integer
        'figure out the start index for a 2D array
        Dim i As Integer, j As Integer
        Dim start_index As Integer = 99999
        For i = 0 To UBound(data, 1)
            For j = 0 To UBound(data, 2)
                If data(i, j) >= 0 AndAlso data(i, j) < start_index Then start_index = data(i, j)
            Next
        Next
        Return start_index
    End Function

    Public Function ReprojectAndWriteMeshToWebJS(path As String) As Boolean
        ' <summary>
        ' Writes the mesh to a GeoJSON object inside a JS file
        ' Assumes the source data is in RD New projection (EPSG:29882)
        ' </summary>
        ' <param name="path">path to the .js file to be written.</param>
        ' <returns>a .js file holding a javascript variable that contains a GeoJSON object.</returns>
        Try
            Dim i As Long
            Dim Lat As Double, Lng As Double

            Using meshWriter As New StreamWriter(path)
                meshWriter.WriteLine("let Mesh = ")
                meshWriter.WriteLine("   {")
                meshWriter.WriteLine("        ""type"": ""FeatureCollection"",")
                meshWriter.WriteLine("        ""name"":  ""test"",")
                meshWriter.WriteLine("        ""timesteps"": 0,")
                meshWriter.WriteLine("        ""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:OGC:1.3:CRS84""} },")
                meshWriter.WriteLine("        ""features"": [")

                For i = 0 To UBound(Mesh2d_face_nodes, 1)
                    Dim featureStr As String = "            { ""type"": ""Feature"", ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [["

                    'write its coordinates. Notice that the coordinates are written counterclockwise as they should be
                    Me.Setup.GeneralFunctions.RD2WGS84(Mesh2d_node_x(Mesh2d_face_nodes(i, 0) - Mesh2d_face_nodes_start_index), Mesh2d_node_y(Mesh2d_face_nodes(i, 0) - Mesh2d_face_nodes_start_index), Lat, Lng)
                    featureStr &= "[" & Lng & "," & Lat & "]"
                    For j = 1 To UBound(Mesh2d_face_nodes, 2)
                        If Mesh2d_face_nodes(i, j) > -999 Then
                            Me.Setup.GeneralFunctions.RD2WGS84(Mesh2d_node_x(Mesh2d_face_nodes(i, j) - Mesh2d_face_nodes_start_index), Mesh2d_node_y(Mesh2d_face_nodes(i, j) - Mesh2d_face_nodes_start_index), Lat, Lng)
                            featureStr &= ",[" & Lng & "," & Lat & "]"
                        End If
                    Next
                    Me.Setup.GeneralFunctions.RD2WGS84(Mesh2d_node_x(Mesh2d_face_nodes(i, 0) - Mesh2d_face_nodes_start_index), Mesh2d_node_y(Mesh2d_face_nodes(i, 0) - Mesh2d_face_nodes_start_index), Lat, Lng)
                    featureStr &= ",[" & Lng & "," & Lat & "]]]}}"
                    If i < UBound(Mesh2d_face_nodes, 1) Then featureStr &= ","
                    meshWriter.WriteLine(featureStr)
                Next

                meshWriter.WriteLine("        ]")
                meshWriter.WriteLine("   }")
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ReprojectAndWriteFloodLevelsMeshToWebJS(resultspath As String, ReturnPeriods As List(Of Integer), ExceedanceValues As Dictionary(Of Integer, List(Of Double))) As Boolean
        ' <summary>
        ' Writes the mesh, including flood levels to a GeoJSON object inside a JS file
        ' Assumes the source data is in RD New projection (EPSG:29882)
        ' </summary>
        ' <param name="resultspath">path to the .js file to be written.</param>
        ' <param name="DatabaseTable">Name of the table in SQLite that holds the return periods.</param>
        ' <param name="FeatureIdxField">Name of the field in SQLite table that holds the feature index number.</param>
        ' <param name="ReturnPeriodField">Name of the field in SQLite table that holds the return period.</param>
        ' <param name="ValuesField">Name of the field in SQLite table that holds the water levels.</param>
        ' <param name="OnlyWriteActiveCells">A boolean that reduces the number of cells written by only writing those cells that have level differences between the return periods.</param>
        ' <returns>a .js file holding a javascript variable that contains a GeoJSON object.</returns>
        Try
            Dim i As Long
            Dim Lat As Double, Lng As Double

            Dim features As New List(Of String)

            Using meshWriter As New StreamWriter(resultspath)
                meshWriter.WriteLine("let Mesh = ")
                meshWriter.WriteLine("   {")
                meshWriter.WriteLine("        ""type"": ""FeatureCollection"",")
                meshWriter.WriteLine("        ""name"":  ""test"",")
                meshWriter.WriteLine("        ""timesteps"": 0,")
                meshWriter.WriteLine("        ""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:OGC:1.3:CRS84""} },")
                meshWriter.WriteLine("        ""features"": [")

                For i = 0 To UBound(Mesh2d_face_nodes, 1)

                    'we will only generate the cells for which the exceedance level for the highest return period exceeds the level for the lowest return period
                    Dim waterLevels As List(Of Double)

                    'only those cells have been written to the database that have a difference in water levels between the return periods
                    If ExceedanceValues.ContainsKey(i) Then
                        waterLevels = ExceedanceValues(i)
                        If waterLevels.Max() > waterLevels.Min() Then

                            Dim featureStr As String = "            { ""type"": ""Feature"", ""properties"": {"
                            featureStr &= """T" & ReturnPeriods(0) & """:" & Format(ExceedanceValues(i)(0), "0.00")
                            For j = 1 To ReturnPeriods.Count - 1
                                featureStr &= ", ""T" & ReturnPeriods(j) & """:" & Format(ExceedanceValues(i)(j), "0.00")
                            Next
                            featureStr &= "}, ""geometry"": { ""type"": ""Polygon"", ""coordinates"": [["

                            'write its coordinates. Notice that the coordinates are written counterclockwise as they should be
                            Me.Setup.GeneralFunctions.RD2WGS84(Mesh2d_node_x(Mesh2d_face_nodes(i, 0) - Mesh2d_face_nodes_start_index), Mesh2d_node_y(Mesh2d_face_nodes(i, 0) - Mesh2d_face_nodes_start_index), Lat, Lng)
                            featureStr &= "[" & Lng & "," & Lat & "]"
                            For j = 1 To UBound(Mesh2d_face_nodes, 2)
                                If Mesh2d_face_nodes(i, j) > -999 Then
                                    Me.Setup.GeneralFunctions.RD2WGS84(Mesh2d_node_x(Mesh2d_face_nodes(i, j) - Mesh2d_face_nodes_start_index), Mesh2d_node_y(Mesh2d_face_nodes(i, j) - Mesh2d_face_nodes_start_index), Lat, Lng)
                                    featureStr &= ",[" & Lng & "," & Lat & "]"
                                End If
                            Next
                            Me.Setup.GeneralFunctions.RD2WGS84(Mesh2d_node_x(Mesh2d_face_nodes(i, 0) - Mesh2d_face_nodes_start_index), Mesh2d_node_y(Mesh2d_face_nodes(i, 0) - Mesh2d_face_nodes_start_index), Lat, Lng)
                            featureStr &= ",[" & Lng & "," & Lat & "]]]}}"
                            features.Add(featureStr)
                        End If
                    End If

                Next

                'write all records but the last with a comma
                For i = 0 To features.Count - 2
                    meshWriter.WriteLine(features(i) & ",")
                Next
                'write the last record without a comma
                meshWriter.WriteLine(features(features.Count - 1))

                meshWriter.WriteLine("        ]")
                meshWriter.WriteLine("   }")
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ReprojectAndWriteMeshToGeoJSON(ShapefilePath As String, ByRef jsWriter As System.IO.StreamWriter, ScenarioName As String, SourceProjection As MapWinGIS.GeoProjection, TargetProjection As MapWinGIS.GeoProjection, ByRef Extents As MapWinGIS.Extents) As Boolean
        'this function reprojects our mesh and generates a GeoJSON file of it
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reprojecting and converting mesh from Fou file to GeoJSON...", 0, 10, True)

            'remove existing shapefile
            If System.IO.File.Exists(ShapefilePath) Then Me.Setup.GeneralFunctions.DeleteShapeFile(ShapefilePath)

            'first convert to shapefile
            Dim FaceFieldIdx As Integer
            Dim FaceidxFieldIdx As Integer
            Dim ResultsFieldName As String = "RESULT"
            Dim ResultsFieldType As MapWinGIS.FieldType = MapWinGIS.FieldType.INTEGER_FIELD
            Dim ResultsFieldIdx As Integer
            Dim ResultsFieldLength As Integer = 10
            Dim ResulsFieldPrecision As Integer = 0
            Dim mySF As New MapWinGIS.Shapefile

            If Not MeshToShapefileUsingMapwingis(ShapefilePath, Extents, "FACEIDX", FaceidxFieldIdx, "FACEID", FaceFieldIdx, ResultsFieldName, ResultsFieldIdx, ResultsFieldType, ResultsFieldLength, ResulsFieldPrecision) Then Throw New Exception("Error converting mesh to shapefile")

            'read the shapefile
            If Not mySF.Open(ShapefilePath) Then Throw New Exception("Error reading temporary shapefile: " & ShapefilePath)
            mySF.GeoProjection = SourceProjection

            'now reproject the shapefile
            Dim ReprojectionCount As Integer
            mySF = mySF.Reproject(TargetProjection, ReprojectionCount)
            Extents = mySF.Extents      'return our projected shapefile's extents to the calling function

            'finally write our shapefile to GeoJSON format. Do this by first wrapping the Mapwingis.Shapefile in an own clsShapefile class instance
            Dim Shapefile As New clsShapeFile(Me.Setup)
            Shapefile.sf = mySF
            Shapefile.WriteToGeoJSONForWeb(jsWriter, ScenarioName, FaceFieldIdx, 0)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ReprojectAndWriteMeshToGeoJSON: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function MeshToShapefileUsingMapwingis(ResultsPath As String, ByRef Extents As MapWinGIS.Extents, FaceIdxFieldname As String, ByRef FaceIdxFieldIdx As Integer, FaceIdFieldName As String, ByRef FaceIdFieldIdx As Integer, ResultFieldName As String, ByRef ResultsFieldIdx As Integer, ResultFieldType As MapWinGIS.FieldType, ResultFieldLength As Integer, ResultFieldPrecision As Integer) As Boolean
        Try
            'this function writes the 2D mesh to a shapefile. The shapefile may later be used to insert specific simulation results
            If ResultsPath <> "" Then
                If Not System.IO.Directory.Exists(Setup.GeneralFunctions.DirFromFileName(ResultsPath)) Then System.IO.Directory.CreateDirectory(Setup.GeneralFunctions.DirFromFileName(ResultsPath))
                If System.IO.File.Exists(ResultsPath) Then Me.Setup.GeneralFunctions.DeleteShapeFile(ResultsPath)
            End If
            Dim mySf As New MapWinGIS.Shapefile
            mySf.CreateNewWithShapeID(ResultsPath, ShpfileType.SHP_POLYGON)

            mySf.GeoProjection = New GeoProjection
            mySf.GeoProjection.ImportFromEPSG("28992")
            mySf.StartEditingShapes(True)

            'add the columns. notice that the resultsfield stays empty but it is necessary for later adding results
            FaceIdxFieldIdx = mySf.EditAddField(FaceIdxFieldname, MapWinGIS.FieldType.INTEGER_FIELD, 0, 10)
            FaceIdFieldIdx = mySf.EditAddField(FaceIdFieldName, MapWinGIS.FieldType.STRING_FIELD, 0, 20)
            ResultsFieldIdx = mySf.EditAddField(ResultFieldName, ResultFieldType, ResultFieldPrecision, ResultFieldLength) 'prepare a field for our result. This saves time later

            Dim j As Integer
            'Dim FaceId As String
            Dim FaceIdx As Integer
            Dim ShapeIdx As Integer

            'read the classmap file and walk through all cells and write them as a feature
            Me.Setup.GeneralFunctions.UpdateProgressBar("Converting mesh to shapefile...", 0, 10, True)
            For FaceIdx = 0 To UBound(Mesh2d_face_x)
                'FaceIdx = Convert.ToInt32(FaceId)
                Me.Setup.GeneralFunctions.UpdateProgressBar("", FaceIdx, UBound(Mesh2d_face_x))

                'create a new shape for this face
                Dim newShape As New MapWinGIS.Shape
                Dim shapeCreatedSuccess As Boolean = newShape.Create(ShpfileType.SHP_POLYGON)

                'write its coordinates. Notice that Mapwindow writes polygons counterclockwise!
                Dim startIdx As Integer = -1
                For j = UBound(Mesh2d_face_nodes, 2) To 0 Step -1
                    'NOTE: the index numbers inside FaceNodes are 1-based!
                    If Mesh2d_face_nodes(FaceIdx, j) > -999 Then
                        If startIdx = -1 Then startIdx = j
                        newShape.AddPoint(Mesh2d_face_x(Mesh2d_face_nodes(FaceIdx, j) - 1), Mesh2d_face_y(Mesh2d_face_nodes(FaceIdx, j) - 1))
                    End If
                Next

                'close the polygon by adding the first point again
                newShape.AddPoint(Mesh2d_face_x(Mesh2d_face_nodes(FaceIdx, startIdx) - 1), Mesh2d_face_y(Mesh2d_face_nodes(FaceIdx, startIdx) - 1))

                'and add the shape to our file and assign the cell's ID
                ShapeIdx = mySf.EditAddShape(newShape)
                mySf.EditCellValue(FaceIdxFieldIdx, ShapeIdx, ShapeIdx)                     'write the face's index number to our shapefile
                mySf.EditCellValue(FaceIdFieldIdx, ShapeIdx, FaceIdx)              'write the face's name to our shapefile
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Mesh written...", 0, 10, True)

            If Not mySf.StopEditingShapes(True, True) Then Throw New Exception("StopEditingShapes failed: " & mySf.ErrorMsg(mySf.LastErrorCode))

            Extents = New MapWinGIS.Extents
            Extents.SetBounds(mySf.Extents.xMin, mySf.Extents.yMin, 0, mySf.Extents.xMax, mySf.Extents.yMax, 0)

            If Not mySf.Close() Then Throw New Exception("Error: could not close shapefile.")

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function DepthToShapefile of class clsClassMapFile: " & ex.Message)
            Return False
        End Try

    End Function


    'Public Function FloodStatisticsToGeoJSON(ResultsPath As String, MaxDepth As Boolean, MaxVelocity As Boolean, TInund As Boolean, TMax As Boolean, T20cm As Boolean, T50cm As Boolean) As Boolean

    '    Try
    '        Dim Name As String = "DHydro"

    '        'now we write the results to a plain JSON file
    '        Dim jsonstr As String
    '        Dim idx As Integer = -1

    '        Using meshWriter As New StreamWriter(ResultsPath)
    '            meshWriter.WriteLine("{")
    '            meshWriter.WriteLine("""type"": ""FeatureCollection"",")
    '            meshWriter.WriteLine("""name"": """ & Name & """,")
    '            meshWriter.WriteLine("""crs"": { ""type"": ""name"", ""properties"": { ""name"": ""urn:ogc:def:crs:EPSG::" & "28992" & """} },")
    '            meshWriter.WriteLine("""features"": [")

    '            'walk through all cells and write them as a feature
    '            Dim nFaceNodes As Integer = UBound(FaceNodes, 1) + 1
    '            For i = 0 To nFaceNodes - 1

    '                'only if this cell contains depths>0 it will be written
    '                Dim maxDepthClass As Integer = 1 'first increment class will be skipped since it represents dry cells
    '                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, nFaceNodes)

    '                'check if we have an elevated waterdepth in this cell
    '                For k = 0 To TimeStamps.Count - 1
    '                    If waterDepths(k, i) > 1 Then
    '                        maxDepthClass = waterDepths(k, i)
    '                        Exit For
    '                    End If
    '                Next

    '                'again: skip the lowest class since it represents totally dry cells
    '                If maxDepthClass > 1 Then

    '                    'only if this cell contains depths>0 it will be written
    '                    Dim maxClass As Integer = 1 'first increment class will be skipped since it represents dry cells
    '                    For k = 0 To TimeStamps.Count - 1
    '                        If waterDepths(k, i) > 1 Then
    '                            maxClass = waterDepths(k, i)
    '                            Exit For
    '                        End If
    '                    Next

    '                    'again: skip the lowest class since it represents dry cells
    '                    If maxClass > 1 Then

    '                        idx += 1

    '                        If idx > 0 Then
    '                            'this is not the first line, so close the previous record with a comma and a CRLF
    '                            jsonstr = "," & vbCrLf
    '                        Else
    '                            jsonstr = ""
    '                        End If

    '                        'we have found a cell that contains water depths > 0. Write it to our JSON!
    '                        jsonstr &= "{ ""type"": ""Feature"", ""geometry"": { ""type"": ""MultiPolygon"", ""coordinates"": [[["

    '                        'write our face's geometry. Start with the first coordinate
    '                        'Me.Setup.GeneralFunctions.RD2WGS84(FaceNodesX(FaceNodes(i, 0) - 1), FaceNodesY(FaceNodes(i, 0) - 1), lat, lon)
    '                        jsonstr &= "[" & FaceNodesX(FaceNodes(i, 0) - 1) & "," & FaceNodesY(FaceNodes(i, 0) - 1) & "]"

    '                        'now write the remaining coordinates, if in use
    '                        For j = 1 To UBound(FaceNodes, 2)
    '                            'unused facenodes are indicted by -999 (e.g. triangular faces where ubound(facenodes,2) = 3)
    '                            'NOTE: the index numbers inside FaceNodes are 1-based!
    '                            If FaceNodes(i, j) > -999 Then
    '                                'Me.Setup.GeneralFunctions.RD2WGS84(FaceNodesX(FaceNodes(i, j) - 1), FaceNodesY(FaceNodes(i, j) - 1), lat, lon)
    '                                jsonstr &= ", [" & FaceNodesX(FaceNodes(i, j) - 1) & "," & FaceNodesY(FaceNodes(i, j) - 1) & "]"
    '                            End If
    '                        Next
    '                        jsonstr &= ", [" & FaceNodesX(FaceNodes(i, 0) - 1) & "," & FaceNodesY(FaceNodes(i, 0) - 1) & "]"
    '                        jsonstr &= "]]]}, ""properties"": { ""i"": " & idx

    '                        'for analysis in GIS we cannot write the results in arrays inside the geoJSON
    '                        Dim TsInundH As Integer = -1
    '                        Dim TsInundM As Integer = -1
    '                        Dim Ts20cm As Integer = -1
    '                        Dim Ts50cm As Integer = -1
    '                        Dim TsMax As Integer = -1
    '                        Dim MaxDepthVal As Double = 0
    '                        Dim MaxVelVal As Double = 0

    '                        For j = 1 To TimeStamps.Count - 1

    '                            If DepthClassValues(waterDepths(j, i) - 1) > MaxDepthVal Then
    '                                MaxDepthVal = DepthClassValues(waterDepths(j, i) - 1)
    '                                TsMax = j
    '                            End If
    '                            If VelocityClassValues(velocities(j, i) - 1) > MaxVelVal Then
    '                                MaxVelVal = VelocityClassValues(velocities(j, i) - 1)
    '                            End If

    '                            'as soon as our depth thresholds are exceeded, write the timestep
    '                            If DepthClassValues(waterDepths(j, i) - 1) > 0 AndAlso TsInundH = -1 Then
    '                                TsInundH = getHoursFromTimestepIdx(j)
    '                                TsInundM = getMinutesFromTimestepIdx(j)
    '                            End If
    '                            If DepthClassValues(waterDepths(j, i) - 1) >= 0.2 AndAlso Ts20cm = -1 Then
    '                                Ts20cm = getHoursFromTimestepIdx(j)
    '                            End If
    '                            If DepthClassValues(waterDepths(j, i) - 1) >= 0.5 AndAlso Ts50cm = -1 Then
    '                                Ts50cm = getHoursFromTimestepIdx(j)
    '                            End If
    '                        Next

    '                        jsonstr &= ", """ & GeneralFunctions.enmFloodFieldName.T_FLOOD_H.ToString & """: " & TsInundH & ", """ & GeneralFunctions.enmFloodFieldName.T_FLOOD_M.ToString & """: " & TsInundM & ", """ & GeneralFunctions.enmFloodFieldName.T_20CM_H.ToString & """: " & Ts20cm & ", """ & GeneralFunctions.enmFloodFieldName.T_50CM_H.ToString & """: " & Ts50cm & ", """ & GeneralFunctions.enmFloodFieldName.MAXD_CM.ToString & """: " & Convert.ToInt32(MaxDepthVal * 100) & ", """ & GeneralFunctions.enmFloodFieldName.MAXD_M.ToString & """: " & TsMax & ", """ & GeneralFunctions.enmFloodFieldName.MAXV_CMPS.ToString & """:" & Convert.ToInt16(MaxVelVal * 100) & "}}"

    '                        meshWriter.Write(jsonstr)

    '                    End If

    '                End If




    '            Next

    '            'write the last line ending
    '            meshWriter.Write(vbCrLf)

    '            Me.Setup.GeneralFunctions.UpdateProgressBar("Export to JSON complete.", 0, 10, True)

    '            meshWriter.WriteLine("]")
    '            meshWriter.WriteLine("}")
    '        End Using

    '        Return True
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error in function StatisticsToGeoJSON of class clsClassMapFile: " & ex.Message)
    '        Return False
    '    End Try

    'End Function


End Class
