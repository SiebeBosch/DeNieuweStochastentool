Imports GemBox.Spreadsheet
Imports STOCHLIB.General
Imports MapWinGIS
Public Class clsGeoDatasource
    'this class is a generic placeholder for geodata. It can contain vector and rasterdata and handles any conversions between them
    'it contains an actual raster as well as the conversiontable used to store the data in an integer datatype

    'v1.74: implemented a callback function so we can update our progress bar while performing rasterize operations
    Implements MapWinGIS.ICallback
    Private Setup As clsSetup
    Friend Raster As clsRaster
    Friend Shapefile As clsShapeFile
    Friend GridTable As Dictionary(Of Integer, String)              'table of contents for the raster. converts numerical raster values to their text value and vice versa
    Public PrimaryDataSource As GeneralFunctions.enmGeoDataSource   'the type of geodata source that contains our data (e.g. shapefile, GeoJSON, grid)
    Friend Addendum As clsExcelBook                                 'the addendum in Excel format
    Friend AddendumSheetName As String                               'the name of the addendum excel sheet that contains our data

    Friend TotalShape As MapWinGIS.Shape                            'one shape that represents the entire datasource

    'all individual fields. Since every field can be represented by multiple other fields (especially target levels!) each entry contains a list of fields
    Public Fields As New Dictionary(Of GeneralFunctions.enmInternalVariable, Dictionary(Of Integer, clsGeoField))

    ''below we specify all groups of fields
    'Public TargetLevelRegimesFields As New List(Of clsTargetLevelFields)
    'Public CAPFields As New Dictionary(Of Integer, clsGeoField)  'key = pump index number
    'Public OnMarginFieldsUp As New Dictionary(Of Integer, clsGeoField)  'key = pump index number
    'Public OffMarginFieldsUp As New Dictionary(Of Integer, clsGeoField) 'key = pump index number
    'Public OnMarginFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public OffMarginFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public OnLevelFieldsUp As New Dictionary(Of Integer, clsGeoField)  'key = pump index number
    'Public OffLevelFieldsUp As New Dictionary(Of Integer, clsGeoField) 'key = pump index number
    'Public OnLevelFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public OffLevelFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public BedLevelFieldsUp As New Dictionary(Of Integer, clsGeoField)
    'Public BedLevelFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public BedWidthFieldsUp As New Dictionary(Of Integer, clsGeoField)
    'Public BedWidthFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public WaterlevelFieldsUp As New Dictionary(Of Integer, clsGeoField)
    'Public WaterlevelFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public WaterSurfaceWidthFieldsUp As New Dictionary(Of Integer, clsGeoField)
    'Public WaterSurfaceWidthFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public WaterDepthFieldsUp As New Dictionary(Of Integer, clsGeoField)
    'Public WaterDepthFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public SlopeFieldsUp As New Dictionary(Of Integer, clsGeoField)
    'Public SlopeFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public SurfaceLevelFieldsUp As New Dictionary(Of Integer, clsGeoField)
    'Public SurfaceLevelFieldsDown As New Dictionary(Of Integer, clsGeoField)
    'Public SurfaceWidthFieldsUp As New Dictionary(Of Integer, clsGeoField)
    'Public SurfaceWidthFieldsDown As New Dictionary(Of Integer, clsGeoField)

    Public SewageAreaIdField As String
    Public SewageCategoryField As String
    Public WWTPIDField As String
    Public POCField As String
    Public PavedAreaField As String 'the field that contains the (user defined) paved area as attribute value
    Public StorageField As String 'the field that contains the storage (mm) per sewage area

    Public SewageAreaIdFieldIdx As Integer = -1
    Public SewageCategoryFieldIdx As Integer = -1
    Public WWTPIDFieldIdx As Integer = -1
    Public POCFieldIdx As Integer = -1
    Public PavedAreaFieldIdx As Integer = -1
    Public StorageFieldIdx As Integer = -1

    Private SelectionOperator As String
    Private SelectionOperand As String

    Public AlwaysSetPointInShapefileSearchMode As Boolean   'when opening the datasource, this setting makes sure to always set the 'BeginPointINShapefile' and when closing the 'EndPointInShapefile' mode

    Public Sub New(ByRef mySetup As clsSetup, myAlwaysSetPointInShapefileSearchMode As Boolean)
        Setup = mySetup
        Raster = New clsRaster(Me.Setup)
        Shapefile = New clsShapeFile(Me.Setup)
        AlwaysSetPointInShapefileSearchMode = myAlwaysSetPointInShapefileSearchMode
    End Sub
    Public Function GetGeoFields(FieldType As GeneralFunctions.enmInternalVariable) As Dictionary(Of Integer, clsGeoField)
        If Not Fields.ContainsKey(FieldType) Then Return Nothing
        Return Fields.Item(FieldType)
    End Function

    Public Function CheckDoubleIDs(ObjectType As String) As Boolean
        'v2.108: added this double ID's check
        Try
            Dim ID As String
            Dim IDList As New List(Of String)
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    For i = 0 To Shapefile.sf.NumShapes - 1
                        ID = GetTextValue(i, GeneralFunctions.enmInternalVariable.ID)
                        If IDList.Contains(ID) Then
                            Me.Setup.Log.AddError("Multiple instances of ID " & ID & " found in " & ObjectType & " datsource.")
                        Else
                            IDList.Add(ID)
                        End If
                    Next
            End Select
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Sub AddGeoField(FieldType As GeneralFunctions.enmInternalVariable, ByRef GeoField As clsGeoField)
        If Not Fields.ContainsKey(FieldType) Then Fields.Add(FieldType, New Dictionary(Of Integer, clsGeoField))
        Fields.Item(FieldType).Add(Fields.Item(FieldType).Count, GeoField)
    End Sub

    Public Function GetFirstGeoField(FieldType As GeneralFunctions.enmInternalVariable) As clsGeoField
        If Not Fields.ContainsKey(FieldType) Then Return Nothing
        If Fields.Item(FieldType) Is Nothing Then Return Nothing
        If Fields.Item(FieldType).Count = 0 Then Return Nothing
        Return Fields.Item(FieldType).Values(0)
    End Function

    Public Sub Progress(KeyOfSender As String, Percent As Integer, Message As String) Implements ICallback.Progress
        Me.Setup.GeneralFunctions.UpdateProgressBar(Message, Percent, 100, True)
    End Sub

    Public Sub [Error](KeyOfSender As String, ErrorMsg As String) Implements ICallback.Error
        Select Case ErrorMsg
            Case Is = "Table: Index Out of Bounds"
                'door negatieve veldindex in shapefile. Dit gebruiken we actief als feature, dus niet als foutmelding afhandelen.
            Case Else
                Me.Setup.Log.AddError("Error returned from MapWinGIS Callback function: " & ErrorMsg)
                Me.Setup.Log.AddError(ErrorMsg)
        End Select
    End Sub

    Public Function BoundingBoxIntersections(BoundingBox As MapWinGIS.Extents, ByRef Indices() As Integer) As Boolean
        Try
            If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile Then
                If Shapefile.sf.SelectShapes(BoundingBox,, MapWinGIS.SelectMode.INTERSECTION, Indices) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Throw New Exception("Error: function BoundingBoxIntersections is not yet available for datatypes other than shapefile.")
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function BoundingBoxIntersections: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function StorageTableFromPolygon(ID As String, ByRef myShape As MapWinGIS.Shape, TotalReachLength As Double) As clsSobekTable
        Try
            'this function builds a storage table from our datagsource underneath a given polygon
            If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Grid Then
                Dim TempGrid As New MapWinGIS.Grid
                Dim LowestPoint As New clsXYZ
                Dim Result As clsSobekTable
                Result = Raster.ElevationTableByPolygonByClipping(ID, myShape, LowestPoint, TotalReachLength)
                Return Result
            Else
                Throw New Exception("this function can only be applied to datasources of a grid format.")
            End If
            Return Nothing
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function StorageTableFromPolygon: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function GetFeatureStartPoint(RecordIdx As Integer) As MapWinGIS.Point
        Try
            If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile Then
                Return Shapefile.sf.Shape(RecordIdx).Point(0)
            Else
                Throw New Exception("Error: function GetStartPoint is not yet available for datatypes other than shapefile.")
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetStartPoint: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function GetFeatureEndPoint(RecordIdx As Integer) As MapWinGIS.Point
        Try
            If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile Then
                Return Shapefile.sf.Shape(RecordIdx).Point(Shapefile.sf.Shape(RecordIdx).numPoints - 1)
            Else
                Throw New Exception("Error: function GetFeatureEndPoint is not yet available for datatypes other than shapefile.")
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetFeatureEndPoint: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function SetGridValueMultiplier(Multiplier As Double) As Boolean
        Try
            Raster.GridValuesMultiplier = Multiplier
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SetGridValueMultiplier of class clsGeoDatasource: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetExtents(ByRef Extents As MapWinGIS.Extents) As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Extents = Shapefile.sf.Extents
                Case GeneralFunctions.enmGeoDataSource.Grid
                    Extents = Raster.Grid.Extents
                Case Else
                    Throw New Exception("Function GetExtents not yet supported for datatype " & PrimaryDataSource.ToString)
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function WriteNumericalValue(RecordIdx As Integer, FieldIdx As Integer, Value As Double) As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    If FieldIdx < Shapefile.sf.NumFields Then
                        Shapefile.sf.EditCellValue(FieldIdx, RecordIdx, Value)
                    Else
                        Throw New Exception("Could not write value " & Value & " to field index " & FieldIdx & " of shapefile " & Shapefile.Path & " since the field index exceeds the number of fields.")
                    End If
                Case Else
                    Throw New Exception("Datasources other than shapefiles are not yet supported for this function.")
            End Select
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function writeNumericalValue of class clsGeoDatasource: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function SetSourceByPath(path As String) As Boolean
        Try
            Select Case Strings.Right(path, 3).Trim.ToUpper
                Case "SHP"
                    SetShapefileByPath(path)
                Case Else
                    Throw New Exception("Error setting geodatasource by path. So far only Shapefiles are supported: " & path)
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GetAddField(FieldName As String, FieldType As MapWinGIS.FieldType, Size As Integer, Precision As Integer, ByRef FieldIdx As Integer) As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    'start editing the shapefile
                    If Not Shapefile.GetCreateField(FieldName, FieldType, 2, 10, FieldIdx) Then Throw New Exception("Error receiving or creating field " & FieldName & " in shapefile " & GetPrimaryDatasourcePath())
                    Return True
                Case Else
                    Me.Setup.Log.AddError("Function GetAddField not supported for datasources other than shapefile.")
                    Return False
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetAddField of class clsGeoDataSource: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetFieldIdxByName(FieldName As String) As Integer
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    For Each myFields As Dictionary(Of Integer, clsGeoField) In Fields.Values
                        For Each myField As clsGeoField In myFields.Values
                            If myField.Name.Trim.ToUpper = FieldName.Trim.ToUpper Then
                                'we found our field! return the index number for this field
                                Return myField.ColIdx
                            End If
                        Next
                    Next
                    Throw New Exception("Function GetFieldIdxByName not supported for datasources other than shapefile.")
            End Select
        Catch ex As Exception
            Return -1
        End Try
    End Function

    Public Function GetFieldName(FieldType As GeneralFunctions.enmInternalVariable, Optional ByVal FieldDepthIdx As Integer = 0) As String
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    If Fields.ContainsKey(FieldType) Then
                        Return Fields.Item(FieldType).Item(FieldDepthIdx).Name
                    End If
                    Return String.Empty
                Case Else
                    Me.Setup.Log.AddError("Function GetFieldName not supported for datasources other than shapefile.")
                    Return String.Empty
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetFieldName of class clsGeoDataSource: " & ex.Message)
            Return String.Empty
        End Try
    End Function

    Public Function PointDataFromReachDist(ByVal ShapeIdx As Long, ByVal Dist As Double, ByRef X As Double, ByRef Y As Double, ByRef Angle As Double) As Boolean
        'Date: 16-6-2013
        'Author: Siebe Bosch
        'Description: searches for a given shape & distance on the shape the corresponding X- and Y-coordinates as well as the angle of the shape
        'on that particular location
        Dim i As Long

        Try
            Dim myShape As MapWinGIS.Shape
            Dim prevPoint As MapWinGIS.Point, prevDist As Double
            Dim nextPoint As MapWinGIS.Point, nextDist As Double
            myShape = GetShape(ShapeIdx)

            prevDist = 0
            nextDist = 0

            For i = 0 To myShape.numPoints - 2
                prevPoint = myShape.Point(i)
                nextPoint = myShape.Point(i + 1)
                nextDist += Math.Sqrt((nextPoint.y - prevPoint.y) ^ 2 + (nextPoint.x - prevPoint.x) ^ 2)

                If nextDist >= Dist Then
                    'interpolate to find the XY-coordinate that belongs to the given distance on the reach
                    X = Setup.GeneralFunctions.Interpolate(prevDist, prevPoint.x, nextDist, nextPoint.x, Dist)
                    Y = Setup.GeneralFunctions.Interpolate(prevDist, prevPoint.y, nextDist, nextPoint.y, Dist)
                    Angle = Me.Setup.GeneralFunctions.LineAngleDegrees(prevPoint.x, prevPoint.y, nextPoint.x, nextPoint.y)
                    Angle = Me.Setup.GeneralFunctions.NormalizeAngle(Angle)
                    Return True
                End If
                prevDist = nextDist

            Next
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function PointDataFromReachDist.")
            Return False
        End Try


    End Function
    Public Function SetRecordSelected(RecordIdx As Integer, Selected As Boolean) As Boolean
        Try
            'make sure this information is stored in the appropriate datasource
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Shapefile.sf.ShapeSelected(RecordIdx) = Selected
            End Select
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetGeoDataType() As GeneralFunctions.enmGeoDataType
        'here we get the geodatatype of our source
        'Important: the datasource must already be open!
        Select Case PrimaryDataSource
            Case GeneralFunctions.enmGeoDataSource.Shapefile
                'Open()
                Select Case Shapefile.sf.ShapefileType
                    Case ShpfileType.SHP_POINT, ShpfileType.SHP_POINTM, ShpfileType.SHP_POINTZ, ShpfileType.SHP_MULTIPOINT, ShpfileType.SHP_MULTIPOINTM, ShpfileType.SHP_MULTIPOINTZ
                        Return GeneralFunctions.enmGeoDataType.Point
                    Case ShpfileType.SHP_POLYLINE, ShpfileType.SHP_POLYLINEM, ShpfileType.SHP_POLYLINEZ
                        Return GeneralFunctions.enmGeoDataType.Line
                    Case ShpfileType.SHP_POLYGON, ShpfileType.SHP_POLYGONM, ShpfileType.SHP_POLYGONZ
                        Return GeneralFunctions.enmGeoDataType.Polygon
                End Select
                'Close()
            Case GeneralFunctions.enmGeoDataSource.Grid
                Return GeneralFunctions.enmGeoDataType.Grid
            Case Else
                Me.Setup.Log.AddError("Error in function GetFeatureType of class clsGeoDatasource. Datasources other than shapefiles or grids are not yet supported.")
                Return Nothing
        End Select
    End Function


    Public Function GetColumnIndexByName(FieldName As String) As Integer
        Try
            'this function returns the column index number (or field index number if you will) for a field with given name
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Dim FieldIdx As Integer = Shapefile.GetFieldIdx(FieldName)
                    If FieldIdx >= 0 Then
                        Return FieldIdx
                    Else
                        'try the addendum
                        If Not Addendum Is Nothing Then
                            FieldIdx = Addendum.GetFieldIdx(AddendumSheetName, FieldName, Shapefile.sf.NumFields - 1)
                            Return FieldIdx
                        Else
                            Return -1
                        End If
                    End If
                Case Else
                    Throw New Exception("Datasources other than shapefile are not yet supported in function GetColumnIndexByName.")
            End Select
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function GetColumnIndexByName Of class clsGeoDatasource")
            Return -1
        End Try
    End Function

    Public Function SetSelectionOperator(ByVal myString As String) As Boolean
        SelectionOperator = myString
        Return True
    End Function

    Public Function SetSelectionOperand(ByVal myOperand As Object) As Boolean
        SelectionOperand = myOperand
        Return True
    End Function

    Public Function ConvertToJSONJavascriptVariableFile(path As String) As Boolean
        Try
            'start by transforming the subcatchments shapefile into a geojson file. Later we will read this file into memory and write it to our data.js
            Dim utils As New MapWinGIS.Utils
            If System.IO.File.Exists(path) Then System.IO.File.Delete(path)
            utils.OGR2OGR(Shapefile.Path, path, "-f GeoJSON -t_srs EPSG:4326")

            'read the subcatchments json to memory and convert it into JS content
            Dim areaJSON As String = "let subcatchments =" & vbCrLf
            Using areaReader As New System.IO.StreamReader(path)
                areaJSON &= areaReader.ReadToEnd
            End Using

            'write the subcatchments json contents to a .js file
            Using areaWriter As New System.IO.StreamWriter(path)
                areaWriter.Write(areaJSON)
            End Using
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ConvertToJSON(Path As String) As Boolean
        Try
            If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile Then
                Dim utils As New MapWinGIS.Utils
                If System.IO.File.Exists(Path) Then System.IO.File.Delete(Path)
                utils.OGR2OGR(Shapefile.Path, Path, "-f GeoJSON -t_srs EPSG:4326")
            Else
                Throw New Exception("Error: can Not (yet) convert source file Of " & PrimaryDataSource.ToString & " Type to JSON.")
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function CreateTotalShape() As Boolean
        'this routine merges all shapes inside a shapefile into one TotalShape
        Dim Shape1 As New MapWinGIS.Shape, Shape2 As New MapWinGIS.Shape
        Dim utils As New MapWinGIS.Utils()
        Dim i As Long

        Try
            'hanteer een UNION om alle shapes in één samen te voegen
            If Not Open() Then Throw New Exception("Could not open subcatchments shapefile.")

            'read the number of shapes in this shapefile
            Dim numShapes = Shapefile.sf.NumShapes
            If numShapes = 0 Then Throw New Exception("Shapefile was emtpy.")

            'read the first shape
            TotalShape = Me.Shapefile.sf.Shape(0)
            If Not TotalShape.IsValid Then TotalShape.FixUp(TotalShape)
            If Not TotalShape.IsValid Then Throw New Exception("Error in shapefile: first shape is corrupt and could not be fixed.")

            Me.Setup.GeneralFunctions.UpdateProgressBar("Merging all shapes...", 0, numShapes)

            'read the shapefile, starting with the second shape and merge with totalshape
            If numShapes > 1 Then
                For i = 1 To numShapes - 1
                    Console.WriteLine("Processing shape " & i + 1 & " from " & numShapes)

                    'set shape 1
                    Shape1 = TotalShape

                    'set shape 2 and attempt to fix if not valid
                    Shape2 = Shapefile.sf.Shape(i)
                    If Not Shape2.IsValid Then Shape2.FixUp(Shape2)

                    'only if valid, merge with shape 1
                    If Shape2.IsValid Then
                        TotalShape = utils.ClipPolygon(MapWinGIS.PolygonOperation.UNION_OPERATION, Shape1, Shape2)
                        If Not TotalShape.IsValid Then
                            TotalShape.FixUp(TotalShape) 'als dit leidt tot een corrupte shape, probeer te fiksen
                            If Not TotalShape.IsValid Then TotalShape = Shape1 'als het fixen niet gelukt is, val terug op de oorspronkelijke totalshape, van voor de union
                        End If
                    End If

                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, numShapes)
                Next i
            End If

            Return True
        Catch ex As Exception
            Dim log As String = "Error in MergeAllShapes"
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            Return False
        Finally
            'If Not Shape1 Is Nothing Then Me.setup.GeneralFunctions.ReleaseComObject(Shape1, False) releasing this one screws up the TotalShape
            If Not Shape2 Is Nothing Then Me.Setup.GeneralFunctions.ReleaseComObject(Shape2, False)
            Me.Setup.GeneralFunctions.ReleaseComObject(utils, True)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Done merging shapes...", 0, 10)
        End Try
    End Function


    Public Function getSelectedStatus(Idx As Integer) As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Return Shapefile.sf.ShapeSelected(Idx)
            End Select
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getSelectedStatus of class clsGeoDataSource for record index " & Idx & ": " & ex.Message)
            Return False
        End Try
    End Function
    Public Function GetNumberOfRecords() As Integer
        Select Case PrimaryDataSource
            Case GeneralFunctions.enmGeoDataSource.Shapefile
                Return Shapefile.sf.NumShapes
        End Select
    End Function

    Public Function getTextValueByPoint(ByVal FieldType As GeneralFunctions.enmInternalVariable, ByVal myPoint As MapWinGIS.Point) As String
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Dim ShpIdx As Integer = Shapefile.sf.PointInShapefile(myPoint.x, myPoint.y)
                    If ShpIdx >= 0 Then
                        If Fields.ContainsKey(FieldType) Then
                            For Each myGeoField As clsGeoField In Fields.Item(FieldType).Values
                                'v2.114: replaced IsDbNull by IsNothing
                                If Not IsNothing(Shapefile.sf.CellValue(myGeoField.ColIdx, ShpIdx)) Then
                                    Return Shapefile.sf.CellValue(myGeoField.ColIdx, ShpIdx)
                                End If
                            Next
                        End If
                    Else
                        Return Nothing
                    End If
            End Select
            Return String.Empty
        Catch ex As Exception
            Return String.Empty
        Finally
        End Try
    End Function

    Public Function GetShapeByTextValue(FieldType As STOCHLIB.GeneralFunctions.enmInternalVariable, FieldValue As String, ByRef ShapeIdx As Integer) As MapWinGIS.Shape
        ShapeIdx = getRecordIdxByTextValue(FieldType, FieldValue)
        If ShapeIdx >= 0 Then
            Return GetShape(ShapeIdx)
        Else
            Return Nothing
        End If
    End Function
    Public Function getRecordIdxByTextValue(ByVal FieldType As GeneralFunctions.enmInternalVariable, ByVal Value As String) As Integer
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    If Fields.ContainsKey(FieldType) Then
                        For Each GeoField As clsGeoField In Fields.Item(FieldType).Values
                            Dim RecordIdx As Integer
                            RecordIdx = Shapefile.GetShapeIdxByValue(GeoField.ColIdx, Value)
                            If RecordIdx >= 0 Then Return RecordIdx
                        Next
                    End If
            End Select
            Return -1
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetRecordIdxByCoord of class clsGeoDatasource: " & ex.Message)
            Return -1
        Finally
        End Try


    End Function

    Public Function SelectRecordsbyTextFieldValue(FieldType As GeneralFunctions.enmInternalVariable, FieldValue As String) As Boolean
        Try
            For i = 0 To GetNumberOfRecords() - 1
                If GetTextValue(i, FieldType).Trim.ToUpper = FieldValue.Trim.ToUpper Then
                    SetRecordSelected(i, True)
                Else
                    SetRecordSelected(i, False)
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SelectRecordsbyTextFieldValue of class clsGeoDataSource while processing fieldtype " & FieldType.ToString & " with value " & FieldValue & ": " & ex.Message)
            Return False
        End Try

    End Function
    Public Function GetRecordIdxByCoord(ByVal X As Double, ByVal Y As Double, ByRef Idx As Integer) As Boolean
        Idx = -1
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Idx = Shapefile.GetShapeIdxByCoord(X, Y)
                    If Idx >= 0 Then Return True Else Return False
            End Select
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetRecordIdxByCoord of class clsGeoDatasource: " & ex.Message)
            Return False
        Finally
        End Try
    End Function

    Public Function GetNumericalValueByCoord(ByVal X As Double, ByVal Y As Double, FieldType As GeneralFunctions.enmInternalVariable) As Double
        Dim Idx As Integer = -1
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Idx = Shapefile.GetShapeIdxByCoord(X, Y)
                    If Idx >= 0 Then
                        If Fields.ContainsKey(FieldType) Then
                            Return GetNumericalValue(Idx, FieldType, GeneralFunctions.enmMessageType.None)
                        Else
                            Return Double.NaN
                        End If
                    Else
                        Return Double.NaN
                    End If
            End Select
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetNumericalValueByCoord of class clsGeoDatasource: " & ex.Message)
            Return False
        Finally
        End Try
    End Function

    Public Function GetCreateShapefile() As MapWinGIS.Shapefile
        If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile Then
            Return Shapefile.sf
        Else
            Me.Setup.Log.AddError("Error in function GetCreateShapefile of class clsGeoDatasource. Data types other than shapefile are not yet supported.")
            Return Nothing
        End If
    End Function

    Public Function CreateNewShapefile(path As String, ShapeFileType As MapWinGIS.ShpfileType) As Boolean
        Try
            PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile
            Shapefile = New clsShapeFile(Me.Setup, path)
            Shapefile.sf.CreateNew(path, ShapeFileType)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetCreateShapefileClassInstance() As clsShapeFile
        If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile Then
            Return Shapefile
        Else
            Me.Setup.Log.AddError("Error in function GetCreateShapefileClassInstance of class clsGeoDatasource. Data types other than shapefile are not yet supported.")
            Return Nothing
        End If
    End Function
    Public Function getTargetLevels(ByVal shapeIdx As Long, ByRef TL As clsTargetLevels) As Boolean
        'given a certain shape index number, this function returns the appropriate target levels
        'it always starts with the first target levels in the collection. If these are not available or valid it moves on 
        'to the secondary level values and so forth
        'the reason for this layering is because various users will have various types of target levels:
        'regular target levels, fixed target levels, flexible target levels
        'this function allows to have a fallback scenario. If a target level type is not available or valid, it tries the next 
        'v1.860: added the clauses that implement Double.NaN if no shape present
        Try
            TL = New clsTargetLevels 'v2.000 since we've passed TL by reference, we must reset its values to make sure we actually get a value for the current shape
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile

                    'v2.114: IsDbNull apparently did not work in identifying empty shapefile cells. IsNothing does
                    If Fields.ContainsKey(GeneralFunctions.enmInternalVariable.WPOutlet) Then
                        For Each GeoField As clsGeoField In Fields.Item(GeneralFunctions.enmInternalVariable.WPOutlet).Values
                            If GeoField.ColIdx >= 0 AndAlso Not IsNothing(Shapefile.sf.CellValue(GeoField.ColIdx, shapeIdx)) Then
                                TL.setWPOutlet(Shapefile.sf.CellValue(GeoField.ColIdx, shapeIdx))
                            End If
                            'v2.111: the hierarchy did not work properly. From now on we will only proceed to one level lower in case our value is still NaN
                            If Not Double.IsNaN(TL.getWPOutlet) Then Exit For
                        Next
                    End If

                    'v2.111: the hierarchy did not work properly. From now on we will only continue to one level lower in case our value is still NaN
                    'v2.114: IsDbNull apparently did not work in identifying empty shapefile cells. IsNothing does
                    If Fields.ContainsKey(GeneralFunctions.enmInternalVariable.ZPOutlet) Then
                        For Each GeoField As clsGeoField In Fields.Item(GeneralFunctions.enmInternalVariable.ZPOutlet).Values
                            If GeoField.ColIdx >= 0 AndAlso Not IsNothing(Shapefile.sf.CellValue(GeoField.ColIdx, shapeIdx)) Then
                                TL.setZPOutlet(Shapefile.sf.CellValue(GeoField.ColIdx, shapeIdx))
                            End If
                            'v2.111: the hierarchy did not work properly. From now on we will only proceed to one level lower in case our value is still NaN
                            If Not Double.IsNaN(TL.getZPOutlet) Then Exit For
                        Next
                    End If

                    If Fields.ContainsKey(GeneralFunctions.enmInternalVariable.WPInlet) Then
                        For Each GeoField As clsGeoField In Fields.Item(GeneralFunctions.enmInternalVariable.WPInlet).Values
                            If GeoField.ColIdx >= 0 AndAlso Not IsNothing(Shapefile.sf.CellValue(GeoField.ColIdx, shapeIdx)) Then
                                TL.setWPinlet(Shapefile.sf.CellValue(GeoField.ColIdx, shapeIdx))
                            End If
                            'v2.111: the hierarchy did not work properly. From now on we will only proceed to one level lower in case our value is still NaN
                            If Not Double.IsNaN(TL.getWPInlet) Then Exit For
                        Next
                    End If

                    If Fields.ContainsKey(GeneralFunctions.enmInternalVariable.ZPInlet) Then
                        For Each GeoField As clsGeoField In Fields.Item(GeneralFunctions.enmInternalVariable.ZPInlet).Values
                            If GeoField.ColIdx >= 0 AndAlso Not IsNothing(Shapefile.sf.CellValue(GeoField.ColIdx, shapeIdx)) Then
                                TL.setZPinlet(Shapefile.sf.CellValue(GeoField.ColIdx, shapeIdx))
                            End If
                            'v2.111: the hierarchy did not work properly. From now on we will only proceed to one level lower in case our value is still NaN
                            If Not Double.IsNaN(TL.getZPInlet) Then Exit For
                        Next
                    End If
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getTargetLevels of class clsSubcatchmentDataSource.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function ListUniqueValuesFromField(FieldName As String, ByRef myList As List(Of String)) As Boolean
        Try
            Dim FieldIdx As Long, i As Long
            myList = New List(Of String)
            Open()
            'please notice that the fields have not necessarily been read yet in this stage. Therefore we will revert to reading straight from the source
            'first retrieve the field
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    FieldIdx = Shapefile.GetFieldIdx(FieldName)
                    If FieldIdx < 0 Then Throw New Exception("Field " & FieldName & " not found in shapefile " & Shapefile.Path)
                    For i = 0 To Shapefile.sf.NumShapes - 1
                        If Shapefile.sf.CellValue(FieldIdx, i) Is Nothing Then
                            'siebe 27-8-2019 added the value of NULL as a reserved keyword
                            If Not myList.Contains("NULL") Then myList.Add("NULL")
                        ElseIf Not myList.Contains(Shapefile.sf.CellValue(FieldIdx, i)) Then
                            myList.Add(Shapefile.sf.CellValue(FieldIdx, i))
                        End If
                    Next
                Case Else
                    Me.Setup.Log.AddError("Listing unique values from datasource type " & PrimaryDataSource.ToString & " not yet supported.")
            End Select
            Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in Function ListUniqueValuesFromField Of class clsGeoDatasource.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Sub SetGrid(ByVal GridPath As String)
        Raster = New clsRaster(Me.Setup, GridPath)
    End Sub

    Public Function Open() As Boolean
        Try
            'this function opens all members of this datasource for reading. E.g. a shapefile + addendum

            'start with the shapefile, if exists
            If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile Then
                If Not Shapefile Is Nothing Then
                    If Not Shapefile.Open() Then Throw New Exception("Could not open shapefile " & Shapefile.Path)
                    If AlwaysSetPointInShapefileSearchMode Then Shapefile.sf.BeginPointInShapefile()
                End If
            ElseIf PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Grid Then
                If Not Raster Is Nothing Then
                    If Not Raster.Read(False) Then Throw New Exception("Could Not open grid " & Raster.Path)
                End If
            End If

            'proceed with the addendum, if exists
            If Not Addendum Is Nothing Then
                If Not Addendum.Read() Then Throw New Exception("Could not open addendum file " & Addendum.Path)
            End If

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function Close() As Boolean
        Try
            'this function closes all members of this datasource
            If Not Shapefile Is Nothing Then
                If AlwaysSetPointInShapefileSearchMode Then Shapefile.sf.EndPointInShapefile()
                Shapefile.Close()
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function FixUpAndCopy() As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    'fixup the shapefile and write it to our export directory and set the path to this new location
                    Dim fixedSF As New MapWinGIS.Shapefile
                    Dim SubcatchmentDataSourcePath As String = Setup.Settings.ExportDirRoot & "\subcatchments.shp"
                    Shapefile.Open()
                    Me.Setup.GeneralFunctions.FixupShapefile(Shapefile.sf, fixedSF, True, True, SubcatchmentDataSourcePath)
                    Shapefile.sf = fixedSF
                    Shapefile.Path = SubcatchmentDataSourcePath
                    Return True
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FixupAndCopy of class clsGeoDatasource:" & ex.Message)
            Return False
        End Try

    End Function

    Friend Sub CreateMergedShapePerCatchment(ByRef Catchments As clsCatchments)
        'creates for each catchment a shape that has been merged from all underlying subcatchments
        Dim Shape1 As MapWinGIS.Shape = Nothing
        Dim Shape2 As MapWinGIS.Shape = Nothing
        Dim myShape As New MapWinGIS.Shape
        Dim myCatchment As clsCatchment = Nothing
        Dim utils As New MapWinGIS.Utils()
        Dim i As Long

        Try
            Dim CID As String
            Dim nRecords As Integer = GetNumberOfRecords()

            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Merging shapes by catchment...", 0, nRecords)

                    If nRecords = 0 Then
                        Me.Setup.Log.AddError("Geen shapes in MergeShapesByCatchment")
                    ElseIf nRecords = 1 Then
                        myCatchment = Catchments.getAdd(GetTextValue(0, GeneralFunctions.enmInternalVariable.ParentID))
                        myCatchment.TotalShape = Shapefile.sf.Shape(0)
                    Else
                        Dim ColIdx As Integer = -1
                        If Fields.ContainsKey(GeneralFunctions.enmInternalVariable.ParentID) Then
                            For Each GeoField As clsGeoField In Fields.Item(GeneralFunctions.enmInternalVariable.ParentID).Values
                                If GeoField.ColIdx >= 0 Then
                                    ColIdx = GeoField.ColIdx
                                    Exit For
                                End If
                            Next
                        End If

                        If ColIdx < 0 Then Throw New Exception("Field for Catchment ID not found in area shapefile.")

                        For i = 0 To nRecords - 1
                            'vraag eerst het ID van de catchment op
                            CID = GetTextValue(i, GeneralFunctions.enmInternalVariable.ParentID)
                            myCatchment = Catchments.getAdd(CID.Trim.ToUpper)

                            If myCatchment.TotalShape Is Nothing Then
                                If Shapefile.sf.Shape(i).IsValid Then myCatchment.TotalShape = Shapefile.sf.Shape(i)
                            Else
                                If Shapefile.sf.Shape(i).IsValid Then
                                    Shape1 = myCatchment.TotalShape
                                    Shape2 = Shapefile.sf.Shape(i)
                                    myCatchment.TotalShape = utils.ClipPolygon(MapWinGIS.PolygonOperation.UNION_OPERATION, Shape1, Shape2)
                                End If
                            End If

                            'werk de progressbar bij
                            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, nRecords)
                        Next i
                    End If
            End Select



        Catch ex As Exception
            Dim log As String = "Error in MergeShapesByCatchment"
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)

        Finally
            'Me.setup.GeneralFunctions.ReleaseComObject(myShape, False)
            If Not Shape1 Is Nothing Then Me.Setup.GeneralFunctions.ReleaseComObject(Shape1, False)
            If Not Shape2 Is Nothing Then Me.Setup.GeneralFunctions.ReleaseComObject(Shape2, False)
            Me.Setup.GeneralFunctions.ReleaseComObject(utils, True)
        End Try
    End Sub

    Public Function GridPercentileFromCircle(ObjectID As String, XCenter As Double, YCenter As Double, Radius As Double, Percentiles As List(Of Double), nCircleSteps As Integer, TempDir As String, ByRef Results As List(Of Double)) As Boolean
        Try
            If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Grid Then
                If Not Raster.GetGridPercentileFromCircle(ObjectID, XCenter, YCenter, Radius, Percentiles, nCircleSteps, TempDir, Results) Then Throw New Exception("Error retrieving grid percentiles for object " & ObjectID)
            Else
                Throw New Exception("Function GridPercentileFromCircle works on grid datasources only.")
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GridPercentileFromCircle: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function getTargetLevelsByPoint(ByVal myPoint As MapWinGIS.Point, ByRef TargetLevels As clsTargetLevels) As Boolean
        Dim Utils As New MapWinGIS.Utils, i As Long, myShape As MapWinGIS.Shape
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    For i = 0 To Shapefile.sf.NumShapes - 1
                        myShape = Shapefile.sf.Shape(i)
                        If Utils.PointInPolygon(myShape, myPoint) Then
                            getTargetLevels(i, TargetLevels)
                        End If
                    Next
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getTargetLevelsByPoint")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        Finally
            Utils = Nothing
            myShape = Nothing
        End Try
    End Function

    Public Function GetTargetLevelsFromReachLocation(ByRef myReach As clsSbkReach, myChainage As Double, ChainageShift As Integer, ByRef TargetLevels As clsTargetLevels) As Boolean
        Try
            Dim sampleLoc As New clsSbkReachObject(Me.Setup, Me.Setup.SOBEKData.ActiveProject.ActiveCase)
            Dim ShapeIdx As Long

            sampleLoc.ci = myReach.Id
            sampleLoc.lc = myChainage + ChainageShift
            If sampleLoc.lc < 0 Then sampleLoc.lc = 0
            If sampleLoc.lc > myReach.getReachLength Then sampleLoc.lc = myReach.getReachLength
            sampleLoc.calcXY()
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    ShapeIdx = Shapefile.GetShapeIdxByCoord(sampleLoc.X, sampleLoc.Y, False, False)
                    If Not getTargetLevels(ShapeIdx, TargetLevels) Then Throw New Exception("Error retrieving target levels for shape " & ShapeIdx)
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error retrieving target levels for location " & myChainage & " on reach " & myReach.Id & ".")
            Return False
        End Try
    End Function

    Public Function GetWinterOutletTargetLevel(ByRef ShapeIdx As Long, ByRef myVal As Double) As Boolean
        Try
            'this routine walks through all available target level fields until it finds one that has a valid value
            myVal = GetNumericalValue(ShapeIdx, GeneralFunctions.enmInternalVariable.WPOutlet)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("No winter outlet target level found for shape number " & ShapeIdx)
            Return False
        End Try
        Return False
    End Function

    Public Function GetWinterInletTargetLevel(ByRef ShapeIdx As Long, ByRef myVal As Double) As Boolean
        Try
            myVal = GetNumericalValue(ShapeIdx, GeneralFunctions.enmInternalVariable.WPInlet)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("No winter inlet target level found for shape number " & ShapeIdx)
            Return False
        End Try
        Return False
    End Function

    Public Function IDsUnique() As Boolean
        Try
            'this function checks if all ID's in the given datasource are unique
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    'for this routine we will stick to the first layer of geofields specified
                    If Fields.ContainsKey(GeneralFunctions.enmInternalVariable.ID) Then
                        For Each GeoField As clsGeoField In Fields.Item(GeneralFunctions.enmInternalVariable.ID).Values
                            Return Shapefile.ValuesUnique(GeoField.ColIdx)
                        Next
                    End If
                Case Else
                    Throw New Exception("Datasource types other than shapfiles are not yet supported.")
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function IDsUnique of class clsGeoDatasource: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetShape(ShapeIdx As Integer) As MapWinGIS.Shape
        Select Case PrimaryDataSource
            Case GeneralFunctions.enmGeoDataSource.Shapefile
                Return Shapefile.sf.Shape(ShapeIdx)
            Case Else
                Me.Setup.Log.AddError("Returning shape from datasources other than shapefiles is not yet supported.")
                Return Nothing
        End Select
    End Function

    Public Function GetTextValue(RecordIdx As Integer, FieldType As STOCHLIB.GeneralFunctions.enmInternalVariable, Optional ByVal OnError As GeneralFunctions.enmMessageType = GeneralFunctions.enmMessageType.ErrorMessage) As String
        Try
            If Not Fields.ContainsKey(FieldType) Then
                Select Case OnError
                    Case GeneralFunctions.enmMessageType.ErrorMessage
                        Me.Setup.Log.AddError("Field for " & FieldType.ToString & " not in datasource " & GetPrimaryDatasourcePath() & ".")
                    Case GeneralFunctions.enmMessageType.Warning
                        Me.Setup.Log.AddWarning("Field for " & FieldType.ToString & " not in datasource " & GetPrimaryDatasourcePath() & ".")
                    Case GeneralFunctions.enmMessageType.Message
                        Me.Setup.Log.AddMessage("Field for " & FieldType.ToString & " not in datasource " & GetPrimaryDatasourcePath() & ".")
                    Case Else
                        Return String.Empty
                End Select
            Else
                Dim myFields As Dictionary(Of Integer, clsGeoField) = Fields.Item(FieldType)
                For Each myField As clsGeoField In myFields.Values
                    Select Case PrimaryDataSource
                        Case GeneralFunctions.enmGeoDataSource.Shapefile
                            If myField.ColIdx < Shapefile.sf.NumFields Then
                                'the primary data source contains this value
                                Return Shapefile.sf.CellValue(myField.ColIdx, RecordIdx)
                            Else
                                'look up the ID in our primary datasource and find its value in the addendum
                                Dim ID As String = GetTextValue(RecordIdx, GeneralFunctions.enmInternalVariable.ID, OnError)
                                Return Addendum.GetTextValue(AddendumSheetName, ID, myField.ColIdx - Shapefile.sf.NumFields) 'the field count continues from primary datasource to addendum. Keep in mind that the first column in Excel does not count since it contains the ID
                            End If
                        Case Else
                            Throw New Exception("Datasouces other than shapefiles not yet supported in function GetTextValue of class clsGeoDatasource.")
                    End Select
                Next
            End If

            'no result found so return nothing
            Return Nothing

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetTextFieldValue of class clsGeoDatasource: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function GetNumericalValueByFieldIdx(recordIdx As Integer, FieldIdx As Integer, FieldType As GeneralFunctions.enmInternalVariable, Optional ByVal OnError As GeneralFunctions.enmMessageType = GeneralFunctions.enmMessageType.None) As Double
        Try
            If FieldIdx >= 0 Then
                Select Case PrimaryDataSource
                    Case GeneralFunctions.enmGeoDataSource.Shapefile
                        If FieldIdx < Shapefile.sf.NumFields Then
                            'the primary data source contains this value
                            Return Me.Setup.GeneralFunctions.ForceNumeric(Shapefile.sf.CellValue(FieldIdx, recordIdx), FieldType.ToString, Double.NaN)
                        Else
                            'look up the ID in our primary datasource and find its value in the addendum
                            Dim ID As String = GetTextValue(recordIdx, GeneralFunctions.enmInternalVariable.ID)
                            Return Me.Setup.GeneralFunctions.ForceNumeric(Addendum.GetNumericalValue(AddendumSheetName, ID, FieldIdx, GetPrimaryDatasourceLastFieldIndex), FieldType.ToString, Double.NaN) 'the field count continues from primary datasource to addendum. Keep in mind that the first column in Excel does not count since it contains the ID
                        End If
                    Case Else
                        Throw New Exception("Error in function GetNumericalValueByFieldIdx. Datasources other than shapefile are not yet supported.")
                End Select
            Else
                Throw New Exception("Field not found in geodatasource ")
            End If
        Catch ex As Exception
            Select Case OnError
                Case GeneralFunctions.enmMessageType.ErrorMessage
                    Me.Setup.Log.AddError(ex.Message)
                Case GeneralFunctions.enmMessageType.Message
                    Me.Setup.Log.AddMessage(ex.Message)
                Case GeneralFunctions.enmMessageType.Warning
                    Me.Setup.Log.AddWarning(ex.Message)
            End Select
            Return Double.NaN
        End Try
    End Function

    Public Function GetNumericalValue(RecordIdx As Integer, FieldType As STOCHLIB.GeneralFunctions.enmInternalVariable, Optional ByVal OnError As GeneralFunctions.enmMessageType = GeneralFunctions.enmMessageType.None, Optional ByVal ResultWhenMissing As Double = Double.NaN, Optional ByVal Multiplier As Double = 1) As Double
        Try
            If Not Fields.ContainsKey(FieldType) Then
                Select Case OnError
                    Case GeneralFunctions.enmMessageType.ErrorMessage
                        Me.Setup.Log.AddError("Field for " & FieldType.ToString & " not in datasource " & GetPrimaryDatasourcePath() & ".")
                        Return ResultWhenMissing
                    Case GeneralFunctions.enmMessageType.Warning
                        Me.Setup.Log.AddWarning("Field for " & FieldType.ToString & " not in datasource " & GetPrimaryDatasourcePath() & ".")
                        Return ResultWhenMissing
                    Case GeneralFunctions.enmMessageType.Message
                        Me.Setup.Log.AddMessage("Field for " & FieldType.ToString & " not in datasource " & GetPrimaryDatasourcePath() & ".")
                        Return ResultWhenMissing
                    Case Else
                        Return ResultWhenMissing
                End Select
            Else
                'v2.100: since every field can reside in multiple GeoFields, we'll walk through each of them and stop as soon as a valid value is found
                Dim myFields As Dictionary(Of Integer, clsGeoField) = Fields.Item(FieldType)
                For Each myField As clsGeoField In myFields.Values
                    Select Case PrimaryDataSource
                        Case GeneralFunctions.enmGeoDataSource.Shapefile
                            If myField.ColIdx < Shapefile.sf.NumFields Then
                                'the primary data source contains this value
                                'v2.114: replaced IsDbNull by IsNothing
                                'v2.201: return ResultWhenMissing when the result is nothing
                                If IsNothing(Shapefile.sf.CellValue(myField.ColIdx, RecordIdx)) Then Return ResultWhenMissing
                                Return Me.Setup.GeneralFunctions.ForceNumeric(Shapefile.sf.CellValue(myField.ColIdx, RecordIdx), FieldType.ToString, ResultWhenMissing, GeneralFunctions.enmMessageType.None, Multiplier)
                            Else
                                'look up the ID in our primary datasource and find its value in the addendum
                                Dim ID As String = GetTextValue(RecordIdx, GeneralFunctions.enmInternalVariable.ID)
                                Return Me.Setup.GeneralFunctions.ForceNumeric(Addendum.GetNumericalValue(AddendumSheetName, ID, myField.ColIdx, GetPrimaryDatasourceLastFieldIndex), FieldType.ToString, ResultWhenMissing, GeneralFunctions.enmMessageType.None, Multiplier) 'the field count continues from primary datasource to addendum. Keep in mind that the first column in Excel does not count since it contains the ID
                            End If
                    End Select
                Next
            End If
            Return ResultWhenMissing
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetNumericalValue of class clsGeoDatasource: " & ex.Message)
            Return ResultWhenMissing
        End Try
    End Function

    Public Function GetIntegerValue(RecordIdx As Integer, FieldType As STOCHLIB.GeneralFunctions.enmInternalVariable, Optional ByVal OnError As GeneralFunctions.enmMessageType = GeneralFunctions.enmMessageType.None, Optional ByVal ResultWhenMissing As Double = 0) As Integer
        Try
            If Not Fields.ContainsKey(FieldType) Then
                Select Case OnError
                    Case GeneralFunctions.enmMessageType.ErrorMessage
                        Me.Setup.Log.AddError("Field for " & FieldType.ToString & " not in datasource " & GetPrimaryDatasourcePath() & ".")
                    Case GeneralFunctions.enmMessageType.Warning
                        Me.Setup.Log.AddWarning("Field for " & FieldType.ToString & " not in datasource " & GetPrimaryDatasourcePath() & ".")
                    Case GeneralFunctions.enmMessageType.Message
                        Me.Setup.Log.AddMessage("Field for " & FieldType.ToString & " not in datasource " & GetPrimaryDatasourcePath() & ".")
                    Case Else
                        Return ResultWhenMissing
                End Select
            Else
                'v2.100: since every field can reside in multiple GeoFields, we'll walk through each of them and stop as soon as a valid value is found
                Dim myFields As Dictionary(Of Integer, clsGeoField) = Fields.Item(FieldType)
                For Each myField As clsGeoField In myFields.Values
                    Select Case PrimaryDataSource
                        Case GeneralFunctions.enmGeoDataSource.Shapefile
                            If myField.ColIdx < Shapefile.sf.NumFields Then
                                'the primary data source contains this value
                                'v2.114: replaced IsDbNull by IsNothing
                                If Not IsNothing(Shapefile.sf.CellValue(myField.ColIdx, RecordIdx)) Then
                                    Return Me.Setup.GeneralFunctions.ForceNumeric(Shapefile.sf.CellValue(myField.ColIdx, RecordIdx), FieldType.ToString, ResultWhenMissing)
                                End If
                            Else
                                'look up the ID in our primary datasource and find its value in the addendum
                                Dim ID As String = GetTextValue(RecordIdx, GeneralFunctions.enmInternalVariable.ID)
                                Return Me.Setup.GeneralFunctions.ForceNumeric(Addendum.GetNumericalValue(AddendumSheetName, ID, myField.ColIdx, GetPrimaryDatasourceLastFieldIndex), FieldType.ToString, ResultWhenMissing) 'the field count continues from primary datasource to addendum. Keep in mind that the first column in Excel does not count since it contains the ID
                            End If
                    End Select
                Next
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetIntegerValue of class clsGeoDatasource: " & ex.Message)
            Return ResultWhenMissing
        End Try
    End Function

    Public Sub SetSourcePaths(myPrimarySourcePath As String, Optional ByVal myAddendumPath As String = "", Optional ByVal myAddendumSheetName As String = "")
        SetPrimaryDatasource(myPrimarySourcePath)
        If Not myAddendumPath = "" Then SetAddendum(myAddendumPath)
        If Not myAddendumSheetName = "" Then SetAddendumSheetName(myAddendumSheetName)
    End Sub

    Public Function SetPrimaryDatasource(path As String) As Boolean
        Try
            If Right(path, 3).Trim.ToUpper = "SHP" Then
                PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile
                SetShapefileByPath(path)
            ElseIf Right(path, 3).Trim.ToUpper = "TIF" OrElse Right(path, 3).Trim.ToUpper = "ASC" OrElse Right(path, 3).Trim.ToUpper = "IMG" OrElse Right(path, 3).Trim.ToUpper = "VRT" Then
                PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Grid
                SetGrid(path)
            Else
                Throw New Exception("Datasource of types other than Shapefile are not yet supported.")
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SetPrimaryDatasource of class clsGeoDatasource.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function GetPrimaryDatasourcePath() As String
        Select Case PrimaryDataSource
            Case Is = GeneralFunctions.enmGeoDataSource.Shapefile
                Return Shapefile.Path
            Case Is = GeneralFunctions.enmGeoDataSource.Grid
                Return Raster.Path
            Case Else
                Return Nothing
        End Select
    End Function

    Public Function GetPrimaryDatasourceLastFieldIndex() As Integer
        Select Case PrimaryDataSource
            Case Is = GeneralFunctions.enmGeoDataSource.Shapefile
                If Shapefile.sf.NumFields > 0 Then
                    Return Shapefile.sf.NumFields - 1
                Else
                    Open()
                    Return Shapefile.sf.NumFields - 1
                End If
            Case Else
                Return Nothing
        End Select
    End Function

    Public Function PopulateDataGridViewColumnCombobox(ByRef myCol As System.Windows.Forms.DataGridViewComboBoxColumn) As Boolean
        'old function arguments: ByVal SourceFilePath As String, ByVal AddendumPath As String, ByVal SheetName As String, 
        Try
            'this function populates a combobox with all fields of the given datasource
            'please note that the fields have not yet been populated yet so we revert to reading them directly from the source files
            Dim i As Integer

            'set the primary datasource and the addendum (if present)
            'SetPrimaryDatasource(SourceFilePath)
            'SetAddendum(AddendumPath)
            'SetAddendumSheetName(SheetName)
            'now that both datasources are open, we can populate our combobox with all available fields
            Open()

            'start with the fields from the primary datasource
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    If Shapefile.sf.NumFields = 0 Then Throw New Exception("Error: shapefile does not contain any fields: " & Shapefile.Path)
                    myCol.Items.Add("") 'v2.105: always add the possibility to select an empty value
                    For i = 0 To Shapefile.sf.NumFields - 1
                        myCol.Items.Add(Shapefile.sf.Field(i).Name.Trim.ToUpper)
                    Next
                Case Else
                    Throw New Exception("Primary datasources of types other than shapefile are not yet supported.")
            End Select

            'add all fields from the Addendum file
            If Not Addendum Is Nothing Then
                Addendum.Read()
                Dim r As Integer, c As Integer = 0 'we assume the first column contains the ID so skip that one
                Dim mySheet As ExcelWorksheet = Addendum.GetSheetByName(AddendumSheetName)
                While Not mySheet.Cells(r, c + 1).Value = ""
                    c += 1
                    myCol.Items.Add(mySheet.Cells(r, c).Value.ToString.Trim.ToUpper)
                End While
            End If

            Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function PopulateDataGridViewColumnCombobox while processing " & GetPrimaryDatasourcePath() & " or its addendum.")
            Return False
        End Try
    End Function
    Public Function PopulateFieldsComboBox(ByVal SourceFilePath As String, ByVal AddendumPath As String, ByRef cmb As System.Windows.Forms.ComboBox, Optional ByVal PreSelect As String = "") As Boolean
        Try
            'this function populates a combobox with all fields of the given datasource
            'please note that the fields have not yet been populated yet so we revert to reading them directly from the source files
            Dim i As Integer

            'set the primary datasource and the addendum (if present)
            SetPrimaryDatasource(SourceFilePath)
            SetAddendum(AddendumPath)
            'now that both datasources are open, we can populate our combobox with all available fields
            Open()

            'start with the fields from the primary datasource
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    If Shapefile.sf.NumFields = 0 Then Throw New Exception("Error: shapefile does not contain any fields: " & Shapefile.Path)
                    For i = 0 To Shapefile.sf.NumFields - 1
                        cmb.Items.Add(Shapefile.sf.Field(i).Name.Trim.ToUpper)
                    Next
                Case Else
                    Throw New Exception("Primary datasources of types other than shapefile are not yet supported.")
            End Select

            'add all fields from the Addendum file
            If Not Addendum Is Nothing Then
                Addendum.Read()
                Dim r As Integer, c As Integer = 0 'we assume the first column contains the ID so skip that one
                Dim mySheet As ExcelWorksheet = Addendum.GetSheetByName(AddendumSheetName)
                While Not mySheet.Cells(r, c).Value = ""
                    c += 1
                    cmb.Items.Add(mySheet.Cells(r, c).Value.name.trim.toupper)
                End While
            End If

            Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function PopulateComboBoxShapeFields while processing " & SourceFilePath & " or its addendum.")
            Return False
        End Try
    End Function

    Public Function SetBeginPointInShapefile() As Boolean
        Try
            If Not Shapefile Is Nothing Then
                Shapefile.sf.BeginPointInShapefile()
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function setEndPointInShapefile() As Boolean
        Try
            If Not Shapefile Is Nothing Then
                Shapefile.sf.EndPointInShapefile()
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function PointInFeatureIdx(X As Double, Y As Double) As Integer
        Return Shapefile.sf.PointInShapefile(X, Y)
    End Function

    Public Function SetShapefileByPath(ByVal ShapePath As String) As Boolean
        Try
            PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile
            Shapefile = New clsShapeFile(Me.Setup, ShapePath)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SetShapefileByPath of class clsGeoDatasource: " & ex.Message)
            Return False
        End Try
    End Function

    Public Sub SetShapeFile(ByRef myShapefile As clsShapeFile)
        PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile
        Shapefile = myShapefile
    End Sub

    Public Sub SetShapeFile(ByRef sf As MapWinGIS.Shapefile)
        PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile
        Shapefile = New clsShapeFile(Me.Setup, sf.Filename)
        Shapefile.sf = sf
    End Sub


    Public Function SetAddendum(path As String) As Boolean
        Try
            If System.IO.File.Exists(path) Then
                If Right(path, 4).Trim.ToUpper = "XLSX" Then
                    Addendum = New clsExcelBook(Me.Setup, path)
                Else
                    Throw New Exception("Addendum data types other than Excel (.XLSX) are not supported.")
                End If
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SetAddendumPath of class clsGeoDatasource.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function SetAddendumSheetName(mySheetName As String) As Boolean
        Try
            AddendumSheetName = mySheetName
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SetAddendumSheetName of class clsGeoDatasource, processing sheetname " & mySheetName & ":" & ex.Message)
            Return False
        End Try
    End Function

    Public Function StartEditingTable() As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Return Shapefile.sf.StartEditingTable()
                Case Else
                    Throw New Exception("Datasource of types other than shapefile not yet supported.")
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function StartEditingTable of clas clsGeoDataSource: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function Save() As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Shapefile.sf.Save()
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error saving geodatasource " & GetPrimaryDatasourcePath())
            Return False
        End Try
    End Function

    Public Function StopEditingTable(ApplyChanges As Boolean) As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    Return Shapefile.sf.StopEditingTable(ApplyChanges)
                Case Else
                    Throw New Exception("Datasource of types other than shapefile not yet supported.")
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function StopEditingTable of clas clsGeoDataSource: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function SetField(FieldType As GeneralFunctions.enmInternalVariable, fieldName As String, Optional ByVal Purpose As String = "", Optional ByVal MessageType As GeneralFunctions.enmMessageType = GeneralFunctions.enmMessageType.Warning) As Boolean
        Try
            If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile Then
                'since we call our shapefile from within our geodatasource we won't have to log our (error/warning) message again so pass None here
                If SetFieldByShapefile(FieldType, fieldName, Purpose, GeneralFunctions.enmMessageType.None) Then Return True
            Else
                Throw New Exception("Error in function SetField of class clsGeoDataSource. Datasource of types other than shapefile not yet supported.")
            End If

            'we have processed the primary datasource, but no luck. So try the addendum
            If Not Addendum Is Nothing Then
                If SetFieldByAddendum(FieldType, fieldName, Purpose, GeneralFunctions.enmMessageType.None) Then Return True
            End If

            'if we end up here, the field has not been found
            Select Case MessageType
                Case GeneralFunctions.enmMessageType.ErrorMessage
                    If fieldName = "" Then
                        Me.Setup.Log.AddError("Field '" & FieldType.ToString & "' for '" & Purpose & "' not specified in datasource " & GetPrimaryDatasourcePath())
                    Else
                        Me.Setup.Log.AddError("Could Not set field with id '" & fieldName & "' of type '" & FieldType.ToString & "' for '" & Purpose & "' in datasource " & GetPrimaryDatasourcePath())
                    End If
                Case GeneralFunctions.enmMessageType.Message
                    If fieldName = "" Then
                        Me.Setup.Log.AddMessage("Field '" & FieldType.ToString & "' for '" & Purpose & "' not specified in datasource " & GetPrimaryDatasourcePath())
                    Else
                        Me.Setup.Log.AddMessage("Could Not set field with id '" & fieldName & "' of type '" & FieldType.ToString & "' for " & Purpose & "' in datasource " & GetPrimaryDatasourcePath())
                    End If
                Case GeneralFunctions.enmMessageType.Warning
                    If fieldName = "" Then
                        Me.Setup.Log.AddWarning("Field '" & FieldType.ToString & "' for '" & Purpose & "' not specified in datasource " & GetPrimaryDatasourcePath())
                    Else
                        Me.Setup.Log.AddWarning("Could Not set field with id '" & fieldName & "' of type '" & FieldType.ToString & "' for " & Purpose & "' in datasource " & GetPrimaryDatasourcePath())
                    End If
            End Select


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SetField of class clsGeoDataSource: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function AddFieldToList(FieldType As GeneralFunctions.enmInternalVariable, fieldName As String, Optional ByVal Purpose As String = "", Optional ByVal MessageType As GeneralFunctions.enmMessageType = GeneralFunctions.enmMessageType.Warning) As Boolean
        'this function is meant to add fields to a list of fields of the same category (e.g. pumpcapacities, on/off levels, target levels)
        Try
            If PrimaryDataSource = GeneralFunctions.enmGeoDataSource.Shapefile Then
                If Not Fields.ContainsKey(FieldType) Then Fields.Add(FieldType, New Dictionary(Of Integer, clsGeoField))
                Fields.Item(FieldType).Add(Fields.Item(FieldType).Count, New clsGeoField(Me.Setup, fieldName, FieldType, GetColumnIndexByName(fieldName)))
            Else
                Throw New Exception("Error in function AddFieldToList of class clsGeoDataSource. Datasource of types other than shapefile not yet supported.")
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function AddFieldToList of class clsGeoDataSource: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function SetFieldByShapefile(FieldType As GeneralFunctions.enmInternalVariable, FieldName As String, Optional ByVal Purpose As String = "", Optional ByVal MessageType As GeneralFunctions.enmMessageType = GeneralFunctions.enmMessageType.Warning) As Boolean
        Try
            Dim FieldIdx As Integer
            FieldIdx = Shapefile.sf.FieldIndexByName(FieldName)
            If FieldIdx < 0 Then
                Select Case MessageType
                    Case GeneralFunctions.enmMessageType.ErrorMessage
                        Me.Setup.Log.AddError("Field for " & Purpose & " field not specified or not found in shapefile " & Shapefile.Path & ":'" & FieldName & "'")
                    Case GeneralFunctions.enmMessageType.Warning
                        Me.Setup.Log.AddWarning("Field for " & Purpose & " field not specified or not found in shapefile " & Shapefile.Path & ":'" & FieldName & "'")
                    Case GeneralFunctions.enmMessageType.Message
                        Me.Setup.Log.AddMessage("Field for " & Purpose & " field not specified or not found in shapefile " & Shapefile.Path & ":'" & FieldName & "'")
                End Select
                Return False
            Else
                If Not Fields.ContainsKey(FieldType) Then
                    Dim NewFields As New Dictionary(Of Integer, clsGeoField)
                    Dim NewField As New clsGeoField(Me.Setup)
                    NewField.Name = FieldName
                    NewField.FieldType = FieldType
                    NewField.ColIdx = FieldIdx
                    NewFields.Add(NewFields.Count, NewField)
                    Fields.Add(FieldType, NewFields)
                Else
                    Dim myFields As Dictionary(Of Integer, clsGeoField) = Fields.Item(FieldType)
                    Dim NewField As New clsGeoField(Me.Setup)
                    NewField.Name = FieldName
                    NewField.FieldType = FieldType
                    NewField.ColIdx = FieldIdx
                    myFields.Add(myFields.Count, NewField)
                End If
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function SetShapeField of class clsGeoDatasource.")
            Return False
        End Try
    End Function

    Public Function SetFieldByAddendum(FieldType As GeneralFunctions.enmInternalVariable, FieldName As String, Optional ByVal Purpose As String = "", Optional ByVal MessageType As GeneralFunctions.enmMessageType = GeneralFunctions.enmMessageType.Warning) As Boolean
        Try
            Dim FieldIdx As Integer

            If FieldName Is Nothing Then Return False
            FieldIdx = Addendum.GetFieldIdx(AddendumSheetName, FieldName, GetPrimaryDatasourceLastFieldIndex)
            If FieldIdx < 0 Then
                Select Case MessageType
                    Case GeneralFunctions.enmMessageType.ErrorMessage
                        Me.Setup.Log.AddError("Field for " & Purpose & " field not specified or not found in shapefile " & Shapefile.Path & ":'" & FieldName & "'")
                    Case GeneralFunctions.enmMessageType.Warning
                        Me.Setup.Log.AddWarning("Field for " & Purpose & " field not specified or not found in shapefile " & Shapefile.Path & ":'" & FieldName & "'")
                    Case GeneralFunctions.enmMessageType.Message
                        Me.Setup.Log.AddMessage("Field for " & Purpose & " field not specified or not found in shapefile " & Shapefile.Path & ":'" & FieldName & "'")
                End Select
                Return False
            Else
                If Not Fields.ContainsKey(FieldType) Then
                    Dim NewFields As New Dictionary(Of Integer, clsGeoField)
                    Fields.Add(FieldType, NewFields)
                    Dim NewField As New clsGeoField(Me.Setup)
                    NewField.Name = FieldName
                    NewField.FieldType = FieldType
                    NewField.ColIdx = FieldIdx
                    NewFields.Add(NewFields.Count, NewField)
                Else
                    Dim myFields As Dictionary(Of Integer, clsGeoField)
                    myFields = Fields.Item(FieldType)
                    Dim NewField As New clsGeoField(Me.Setup)
                    NewField.Name = FieldName
                    NewField.FieldType = FieldType
                    NewField.ColIdx = FieldIdx
                    myFields.Add(myFields.Count, NewField)
                End If
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function SetFieldByAddendum of class clsGeoDatasource: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function AddShapeFieldToFieldsList(ByRef FieldsList As Dictionary(Of Integer, clsGeoField), FieldType As GeneralFunctions.enmInternalVariable, FieldName As String, Optional ByVal Purpose As String = "", Optional ByVal MessageType As GeneralFunctions.enmMessageType = GeneralFunctions.enmMessageType.Warning) As Boolean
        Try
            Dim FieldIdx As Integer
            FieldIdx = Shapefile.sf.FieldIndexByName(FieldName)
            If FieldIdx < 0 Then
                'siebe: hier nog de addendum checken!
                Select Case MessageType
                    Case GeneralFunctions.enmMessageType.ErrorMessage
                        Me.Setup.Log.AddError("Field for " & Purpose & " field not specified or not found in shapefile " & Shapefile.Path & ":'" & FieldName & "'")
                    Case GeneralFunctions.enmMessageType.Warning
                        Me.Setup.Log.AddWarning("Field for " & Purpose & " field not specified or not found in shapefile " & Shapefile.Path & ":'" & FieldName & "'")
                    Case GeneralFunctions.enmMessageType.Message
                        Me.Setup.Log.AddMessage("Field for " & Purpose & " field not specified or not found in shapefile " & Shapefile.Path & ":'" & FieldName & "'")
                End Select
            Else


                Dim newField As New clsGeoField(Me.Setup)
                newField.Name = FieldName
                newField.FieldType = FieldType
                newField.ColIdx = FieldIdx
                FieldsList.Add(FieldsList.Count, newField)
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function SetShapeField of class clsGeoDatasource.")
            Return False
        End Try
    End Function


    Public Function SetShapeFieldIdx(FieldType As GeneralFunctions.enmInternalVariable, FieldName As String, FieldIdx As Integer) As Boolean
        Try
            Shapefile.Open()
            Shapefile.AddEditFieldByIndex(FieldType, FieldName, FieldIdx)
            Shapefile.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function SetShapeField of class clsGeoDatasource.")
            Return False
        End Try
    End Function

    Public Function PrepareShapefileForCatchmentBuilder(ShapefilePath As String, GridPath As String, CellSize As Double, FieldType As STOCHLIB.GeneralFunctions.enmInternalVariable, FieldName As String, Recompute As Boolean) As Boolean
        Try
            '-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            ' this function preps a datasource for usage in Catchment Builder. It rasterizes in case it's a vector file
            ' then it sets the primarydatasource to 'raster'
            ' and it stores the path to the raster file in the Raster variable
            '-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            SetShapefileByPath(ShapefilePath)
            Open()
            SetField(FieldType, FieldName)
            GridFromShapefile(FieldName, GridPath, CellSize, Recompute, Setup.GISData.SubcatchmentDataSource.Shapefile.sf.Extents, CellSize)
            Raster = New clsRaster(Me.Setup, GridPath)
            Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error preparing datasource for use in Catchment Builder: " & ShapefilePath)
            Return False
        End Try
    End Function

    Public Function PrepareGridForCatchmentBuilder(GridPath As String) As Boolean
        Try
            '-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            ' this function preps a datasource for usage in Catchment Builder. It rasterizes in case it's a vector file
            '-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            If Not GridFromGrid(GridPath) Then Throw New Exception("Could not create grid values index for " & GridPath)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error preparing datasource for use in Catchment Builder: " & GridPath)
            Return False
        End Try
    End Function

    Public Function GridFromGrid(ByVal RasterfilePath As String) As Boolean
        Try
            'here we will prepare a native grid as the data source
            'we will still need a conversion table 
            SetGrid(RasterfilePath)
            GridTable = New Dictionary(Of Integer, String)
            Dim Values As New List(Of Single)

            'write all unique values to the gridtable
            Raster.Read()
            Raster.GetUniqueValues(Values)
            Raster.Close()

            For i = 0 To Values.Count - 1
                GridTable.Add(Values(i), Values(i).ToString)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error setting grid as datasource: " & RasterfilePath)
            Return False
        End Try
    End Function

    Public Function GridFromShapefile(ByVal FieldName As String, RasterfilePath As String, CellSize As Double, Recompute As Boolean, Optional ByVal CustomExtents As MapWinGIS.Extents = Nothing, Optional ByVal ExtraMargin As Double = 0) As Boolean
        Try
            'open the shapefile and retrieve the field index
            'v1.860: introduced an ExtraMargin to ensure that the rasterized feature's extents always exceeds the original feature's extents
            Shapefile.Open()
            Dim FieldIdx As Integer = Shapefile.sf.FieldIndexByName(FieldName)

            'create a new field for the numerical value
            Dim NumFieldIdx As Integer
            Shapefile.sf.StartEditingTable()
            NumFieldIdx = Shapefile.sf.FieldIndexByName("NUMERICAL")
            If NumFieldIdx < 0 Then
                NumFieldIdx = Shapefile.sf.EditAddField("NUMERICAL", MapWinGIS.FieldType.INTEGER_FIELD, 10, 4)
            End If

            'populate the conversion table and fill the numerical field's values
            GridTable = New Dictionary(Of Integer, String)
            Dim i As Integer, Value As String, GridVal As Integer
            For i = 0 To Shapefile.sf.NumShapes - 1
                Value = Shapefile.sf.CellValue(FieldIdx, i)
                If Not GridTable.ContainsValue(Value) Then
                    GridVal = GridTable.Count + 1
                    GridTable.Add(GridVal, Value)
                Else
                    'get the key by its value
                    For Each j As Integer In GridTable.Keys
                        If GridTable.Item(j) = Value Then
                            GridVal = j
                            Exit For
                        End If
                    Next
                End If
                Shapefile.sf.EditCellValue(NumFieldIdx, i, GridVal)
            Next

            'save the shapefile
            Shapefile.sf.StopEditingTable(True)
            Shapefile.sf.SaveAs(Shapefile.Path)

            Dim Extents As MapWinGIS.Extents
            If Not CustomExtents Is Nothing Then
                Extents = CustomExtents
            Else
                Extents = Shapefile.sf.Extents
            End If

            'finally rasterize the shapefile if required
            If Recompute OrElse Not System.IO.File.Exists(RasterfilePath) Then
                Dim Args As String = " -of GTiff -a NUMERICAL -a_nodata -999 -te " & Extents.xMin - ExtraMargin & " " & Extents.yMin - ExtraMargin & " " & Extents.xMax + ExtraMargin & " " & Extents.yMax + ExtraMargin & " -tr " & CellSize & " " & CellSize & " "
                Dim ut As New MapWinGIS.Utils
                ut.GlobalCallback = Me
                ut.GDALRasterize(Shapefile.sf.Filename, RasterfilePath, Args)
                ut = Nothing
            End If

            Shapefile.Close()
            SetGrid(RasterfilePath)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error generating raster datasource from shapefile " & Shapefile.Path)
            Return False
        End Try
    End Function


    Public Function GetSummerOutletTargetLevel(ByRef ShapeIdx As Long, ByRef myVal As Double) As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    If Fields.ContainsKey(GeneralFunctions.enmInternalVariable.ZPOutlet) Then
                        For Each GeoField As clsGeoField In Fields.Item(GeneralFunctions.enmInternalVariable.ZPOutlet).Values
                            'v2.114: replaced IsDbNull by IsNothing
                            If Not IsNothing(Shapefile.sf.CellValue(GeoField.ColIdx, ShapeIdx)) Then
                                myVal = Shapefile.sf.CellValue(GeoField.ColIdx, ShapeIdx)
                                Return True
                            End If
                        Next
                    End If
            End Select
            Throw New Exception("")
        Catch ex As Exception
            Me.Setup.Log.AddError("No summer outlet target level found for shape number " & ShapeIdx)
            Return False
        End Try
        Return False
    End Function

    Public Function GetSummerInletTargetLevel(ByRef ShapeIdx As Long, ByRef myVal As Double) As Boolean
        Try
            Select Case PrimaryDataSource
                Case GeneralFunctions.enmGeoDataSource.Shapefile
                    If Fields.ContainsKey(GeneralFunctions.enmInternalVariable.ZPInlet) Then
                        For Each GeoField As clsGeoField In Fields.Item(GeneralFunctions.enmInternalVariable.ZPInlet).Values
                            'v2.114: Replaced IsDbNull by IsNothing
                            If Not IsNothing(Shapefile.sf.CellValue(GeoField.ColIdx, ShapeIdx)) Then
                                myVal = Shapefile.sf.CellValue(GeoField.ColIdx, ShapeIdx)
                                Return True
                            End If
                        Next
                    End If
            End Select
            Throw New Exception("")
        Catch ex As Exception
            Me.Setup.Log.AddError("No summer inlet target level found for shape number " & ShapeIdx)
            Return False
        End Try
        Return False
    End Function


End Class
