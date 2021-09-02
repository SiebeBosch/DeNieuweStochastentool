Imports STOCHLIB.General
Imports GemBox.Spreadsheet

Public Class clsShapeFile

    Public Path As String
    Public sf As New MapWinGIS.Shapefile
    Public Fields As New Dictionary(Of GeneralFunctions.enmInternalVariable, clsShapeFieldIndexPair)

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        'initalize the dictionary of fields with a fictional field named "NONE" and field index -1 so that the calling function can retrieve something it can handle 
        Fields.Add(GeneralFunctions.enmInternalVariable.None, New clsShapeFieldIndexPair("NONE", -1))
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myPath As String)
        Setup = mySetup
        Path = myPath
        'initalize the dictionary of fields with a fictional field named "NONE" and field index -1 so that the calling function can retrieve something it can handle 
        Fields.Add(GeneralFunctions.enmInternalVariable.None, New clsShapeFieldIndexPair("NONE", -1))
    End Sub

    Public Function CreateNew(ByVal mypath As String) As Boolean
        Path = mypath

        If System.IO.File.Exists(mypath) Then
            Me.Setup.GeneralFunctions.deleteShapeFile(mypath)
        End If

        If Not sf.CreateNew(Path, MapWinGIS.ShpfileType.SHP_POLYLINE) Then Return False
        Return True
    End Function


    Public Function CreateField(ByVal FieldName As String, FieldType As MapWinGIS.FieldType, Precision As Integer, Length As Integer, ByRef FieldIdx As Integer) As Boolean
        Try
            FieldIdx = sf.EditAddField(FieldName, FieldType, Precision, Length)
            If FieldIdx >= 0 Then
                Return True
            Else
                Return False
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CreateField of class clsShapefile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetCreateField(ByVal FieldName As String, FIeldType As MapWinGIS.FieldType, Precision As Integer, Length As Integer, ByRef FieldIdx As Integer) As Boolean
        Try
            For i = 0 To sf.NumFields - 1
                If sf.Field(i).Name.Trim.ToUpper = FieldName Then
                    FieldIdx = i
                    Return True
                End If
            Next

            'if we end up here, a new field must be created
            Return CreateField(FieldName, FIeldType, Precision, Length, FieldIdx)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetCreateField of class clsShapefile: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function AddEditField(MyType As GeneralFunctions.enmInternalVariable, FieldName As String) As Boolean
        Dim FieldIdx As Integer = sf.FieldIndexByName(FieldName)
        If FieldIdx >= 0 Then
            If Not Fields.ContainsKey(MyType) Then
                Fields.Add(MyType, New clsShapeFieldIndexPair(FieldName, FieldIdx))
            Else
                Fields.Item(MyType).FieldIndex = FieldIdx
                Fields.Item(MyType).FieldName = FieldName
            End If
            Return True
        Else
            Return False
        End If
    End Function

    Public Function WriteElevationPercentageToNewField(Percentage As Double, FieldName As String, Optional ByVal NullOutChannelAreaShapefile As Boolean = False) As Boolean
        'Date: 17-10-2013
        'Author: Siebe Bosch
        'Description: creates SOBEK-profile data for an intersection by polygon.
        'For each reach inside a polygon, a profile is created, based on the elevation table inside that polygon
        'and the total length of the reaches inside the polygon.
        'Pre-requirements: An elevation grid, a Channel Shapefile and a Polygon Shapefile
        Dim polyShape As MapWinGIS.Shape, iPoly As Long
        Dim Utils As New MapWinGIS.Utils, LowestPoint As New clsXYZ
        Dim ElevationTable As New clsSobekTable(Me.Setup)
        Dim nPoly As Long
        Dim myArea As Double
        Dim FieldIdx As Integer
        Dim workSF As MapWinGIS.Shapefile

        Try
            'let's set the shapefile
            If NullOutChannelAreaShapefile Then
                If Not Me.Setup.GISData.ChannelAreasShapeFile.Open() Then Throw New Exception("Error reading channel area shapefile.")
                workSF = sf.Difference(False, Setup.GISData.ChannelAreasShapeFile.sf, False)
            Else
                workSF = sf
            End If

            If Not Open() Then Throw New Exception("Error opening polygon shapefile.")
            sf.StartEditingTable()

            'add the field
            FieldIdx = sf.EditAddField(FieldName, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)

            nPoly = sf.NumShapes
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building SOBEK Cross Section Data from elevation grid.", 0, 10)

            'walk through all polygons
            For iPoly = 0 To nPoly - 1

                'update the progress bar
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iPoly + 1, nPoly)
                polyShape = workSF.Shape(iPoly)

                If Not polyShape.IsValid Then
                    polyShape.FixUp(polyShape)
                    If Not polyShape.IsValid Then
                        Throw New Exception("Polygon index number " & iPoly & " was invalid and could not be fixed automatically.")
                    Else
                        Me.Setup.Log.AddWarning("Polygon index number " & iPoly & " was invalid, but was successfully fixed.")
                    End If
                End If

                'now clip the elevation grid by this polygon
                Dim tmpGrid = New clsRaster(Me.Setup)
                tmpGrid.Initialize(Me.Setup.Settings.ExportDirRoot & "\tmpGrid" & iPoly.ToString.Trim & ".asc")
                If System.IO.File.Exists(tmpGrid.Path) Then System.IO.File.Delete(tmpGrid.Path)
                Call Utils.ClipGridWithPolygon(Me.Setup.GISData.ElevationGrid.Path, polyShape, tmpGrid.Path, False)

                'and create a sobek table out of it
                Dim SobekTable As New clsSobekTable(Me.Setup)
                SobekTable = tmpGrid.BuildElevationTable("tmp", polyShape, LowestPoint, 1, 1)

                If SobekTable Is Nothing Then Continue For

                'remove the temporary grid file again to save space
                If System.IO.File.Exists(tmpGrid.Path) Then System.IO.File.Delete(tmpGrid.Path)

                'finally retriev the required percentile
                myArea = Percentage / 100 * SobekTable.Values1.Values(SobekTable.Values1.Values.Count - 1)
                Dim myElevation As Double = SobekTable.InterpolateXValueFromValues(myArea)

                'and write it to the shapefiled
                sf.EditCellValue(FieldIdx, iPoly, myElevation)

            Next

            sf.StopEditingTable(True)
            If Not sf.Close() Then Throw New Exception("Error closing polygon shapefile.")
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10)

            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteElevationPercentageToNewField.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function getShapeValueByCoord(ByVal X As Double, ByVal Y As Double, ByVal FieldIdx As Integer) As Object
        'let op: voordat deze functie aangeroepen kan worden, moet sf.BeginPointInShapefile zijn geactiveerd. Na afloop sf.EndPointInShapefile!!!!
        'deze actie kost echter elke keer 2 ms, dus doen we het overkoepelend.
        Dim ShapeIdx As Integer = GetShapeIdxByCoord(X, Y)
        Return sf.CellValue(FieldIdx, ShapeIdx)
    End Function

    Public Function GetShapeIdxByValue(FieldIdx As Integer, FieldValue As String) As Integer
        Try
            For i = 0 To sf.NumShapes - 1
                If sf.CellValue(FieldIdx, i).ToString.Trim.ToUpper = FieldValue.Trim.ToUpper Then
                    Return i
                End If
            Next
            Return -1
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetShapeIdxByValue of class clsShapeFile.")
            Return -1
        End Try
    End Function

    Public Function GetShapeIdxByCoord(ByVal X As Double, ByVal Y As Double) As Integer
        Dim Idx As Integer
        Try
            'sf.BeginPointInShapefile() 'note: this statement has to be in the beginning but it is not practical to do it every time
            Idx = sf.PointInShapefile(X, Y)
            'sf.EndPointInShapefile()
            If Idx >= 0 Then
                Return Idx
            Else
                Return -1
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetRecordIdxByCoord of class clsGeoDatasource: " & ex.Message)
            Return -1
        Finally
        End Try
    End Function
    Public Function ValuesUnique(FieldIdx As Integer) As Boolean
        Try
            Dim ValuesList As New List(Of String)
            For i = 0 To sf.NumShapes - 1
                If ValuesList.Contains(sf.CellValue(FieldIdx, i)) Then
                    Return False
                Else
                    ValuesList.Add(sf.CellValue(FieldIdx, i))
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ValuesUnique of class clsShapefile: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function AddEditFieldByIndex(MyType As GeneralFunctions.enmInternalVariable, FieldName As String, FieldIdx As Integer) As Boolean
        If FieldIdx >= 0 Then
            If Fields.ContainsKey(MyType) Then
                Fields.Item(MyType).FieldIndex = FieldIdx
                Fields.Item(MyType).FieldName = FieldName
            Else
                Fields.Add(MyType, New clsShapeFieldIndexPair(FieldName, FieldIdx))
            End If
            Return True
        Else
            Return False
        End If
    End Function


    Public Function GetAddField(Name As String, Type As MapWinGIS.FieldType, Precision As Integer, Width As Integer) As Integer
        'gets the shapefield index for a given fieldname. If not present it will create one
        If sf.FieldIndexByName(Name) < 0 Then
            'does not yet exist so create
            Return sf.EditAddField(Name, Type, Precision, Width)
        Else
            Return sf.FieldIndexByName(Name)
        End If
    End Function


    Friend Function getUnderlyingShapeIdx(ByVal X As Double, ByVal Y As Double, ByRef ShapeIdx As Long) As Boolean
        'geeft voor een gegeven XY-coordinaat het indexnummer van de onderliggende shape terug
        Dim myUtils As New MapWinGIS.Utils
        Dim myPoint As New MapWinGIS.Point
        Try
            myPoint.x = X
            myPoint.y = Y
            Dim i As Long
            For i = 0 To sf.NumShapes - 1
                If sf.Shape(i).Extents.xMin <= X AndAlso sf.Shape(i).Extents.xMax >= X AndAlso sf.Shape(i).Extents.yMin <= Y AndAlso sf.Shape(i).Extents.yMax >= Y Then
                    If myUtils.PointInPolygon(sf.Shape(i), myPoint) Then
                        ShapeIdx = i
                        Return True
                    End If
                End If
            Next
        Catch ex As Exception
            Return False
        Finally
            myUtils = Nothing
            myPoint = Nothing
        End Try
    End Function


    Public Function GetShapeIdxByCoord(ByVal X As Double, ByVal Y As Double, Optional CloseWhenDone As Boolean = False, Optional ByVal OpenFile As Boolean = True, Optional ByVal SetBeginPointInShapefile As Boolean = True) As Long
        'siebe: v1.71 we removed the methods 'beginpointinshapefile and endpointinshapefile since it consumes too much time to execute for each object separately
        'instead you can now set this when opening the shapefile itself. See the Open and Close functions
        Dim Idx As Long
        Try
            'LET OP: als deze functie vaak wordt aangeroepen is het verstandig om de file al geopend te hebben én om daarbij de optie BeginPointInShapefile al te hebben geactiveerd
            If OpenFile Then
                If Not sf.Open(SetBeginPointInShapefile) Then Throw New Exception("Could Not open shapefile " & sf.Filename)
            End If
            Idx = sf.PointInShapefile(X, Y)
            If Idx >= 0 Then
                Return Idx
            Else
                Me.Setup.Log.AddWarning("No shape found underlying coordinate " & X & "," & Y & ".")
                Return -1
            End If
            If CloseWhenDone Then Close()
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return -1
        Finally
        End Try
    End Function

    Public Function GetField(myType As GeneralFunctions.enmInternalVariable) As clsShapeFieldIndexPair
        If Fields.ContainsKey(myType) Then
            Return Fields.Item(myType)
        Else
            'v1.890: replaced the fictional field with fieldidx = -1. This helps the calling function to continue
            Return Fields.Item(GeneralFunctions.enmInternalVariable.None)
        End If
    End Function


    Public Function Open() As Boolean
        'v1.890: more detailed error handling when reading shapefiles
        Try
            If sf.Open(Path) Then
                Return True
            Else
                Throw New Exception("Error reading shapefile. Error code: " & sf.ErrorMsg(sf.LastErrorCode))
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error reading shapefile " & Path)
            Return False
        End Try
    End Function

    Public Sub Close()
        sf.Close()
    End Sub

    Public Function GetFieldIdx(ByVal Name As String) As Integer
        Dim i As Long
        For i = 0 To sf.NumFields - 1
            If sf.Field(i).Name.Trim.ToUpper = Name.Trim.ToUpper Then Return i
        Next
        Return -1
    End Function

    Public Function getUniqueValuesFromField(ByVal fieldName As String, ByRef Values As List(Of String)) As Boolean
        'this function populates a list with all unique values present in a given field of the underlying shapefile.
        Try
            Dim FieldIdx As Long = GetFieldIdx(fieldName), i As Long
            If FieldIdx < 0 Then Throw New Exception("Error: fieldname " & fieldName & " does not occur in shapefile " & Path)

            For i = 0 To sf.NumShapes - 1
                If Not Values.Contains(sf.CellValue(FieldIdx, i)) Then Values.Add(sf.CellValue(FieldIdx, i))
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
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
            myShape = sf.Shape(ShapeIdx)

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



End Class

