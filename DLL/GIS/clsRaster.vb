Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports MapWinGIS

Public Class clsRaster
    Public Path As String
    Public GridValuesMultiplier As Double = 1 'e.g. used to convert units such as cm NAP to m NAP

    Public Grid As MapWinGIS.Grid   'optie 1: inlezen als Mapwindow-grid
    Friend ASCII As clsASCIIGrid    'optie 2: inlezen als 2D-array (sneller bewerken en zoeken)

    Public XLLCenter As Double, YLLCenter As Double
    Public XLLCorner As Double, YLLCorner As Double, XURCorner As Double, YURCorner As Double
    Public NoDataVal As Double
    Public dX As Double, dY As Double, CellArea As Double
    Public Selected(,) As Boolean
    Friend nCol As Integer, nRow As Integer
    Public Stats As clsGridStats

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        Grid = New MapWinGIS.Grid
        ASCII = New clsASCIIGrid(Me.Setup)
        Stats = New clsGridStats
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myPath As String)
        Setup = mySetup
        Grid = New MapWinGIS.Grid
        ASCII = New clsASCIIGrid(Me.Setup)
        Stats = New clsGridStats
        Path = myPath
    End Sub
    Public Function GetPath() As String
        Return Path
    End Function
    Public Sub SetPath(myPath As String)
        Path = myPath
    End Sub

    Public Function IdentifyNodataBlocks() As Dictionary(Of String, clsNoDataQuadrant)
        'this function returns a full list of rectangular blocks that represent a nodata-value inside this grid
        'the technique we're using is a quad tree, each time halving the block and checking if the entire block had nodata-value
        'this means we'll introduce a recursive function here
        Dim myQuadrant As New clsNoDataQuadrant(Me.Setup, Me, "0", 0, nRow - 1, 0, nCol - 1)
        Dim myBlocks As New Dictionary(Of String, clsNoDataQuadrant)

        myQuadrant.Evaluate(myBlocks) 'in order to evaluate we'll pass the blocks byref

        Return myBlocks

    End Function
    Public Function GetGridPercentileFromCircle(ObjectID As String, XCenter As Double, YCenter As Double, Radius As Double, Percentiles As List(Of Double), nCircleSteps As Integer, tempDir As String, ByRef Results As List(Of Double)) As Boolean
        Try
            'so here we will create a circle shape and overlay that with the grid.
            'then we will retrieve the values inside our circle and derive the required percentile from it
            Results = New List(Of Double)
            Dim Shape As New MapWinGIS.Shape
            Dim i As Integer
            Shape.Create(ShpfileType.SHP_POLYGON)

            'step through the circle in radians
            Dim Angle As Double = 0
            For i = 1 To nCircleSteps
                Dim pnt = New Point()
                Angle = 2 * Math.PI * i / nCircleSteps
                Me.Setup.GeneralFunctions.PolarCoordinates(XCenter, YCenter, Radius, Angle, pnt.x, pnt.y)
                Shape.InsertPoint(pnt, i - 1)
            Next

            'Dim sf As New MapWinGIS.Shapefile
            'sf.CreateNewWithShapeID("c:\temp\testshape.shp", ShpfileType.SHP_POLYGON)
            'sf.EditInsertShape(Shape, 0)
            'sf.Save()

            If Not GridPercentilesFromShape(ObjectID, Shape, Percentiles, tempDir, Results) Then Throw New Exception("Could not retrieve percentile values from grid for object " & ObjectID)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetGridPercentileFromCircle: " & ex.Message)
            Return False
        End Try


    End Function

    Public Function GridPercentilesFromShape(ObjectID As String, ByRef Shape As MapWinGIS.Shape, Percentiles As List(Of Double), tempDir As String, ByRef Results As List(Of Double), Optional ByVal ReuseExistingClippedGrid As Boolean = True) As Boolean
        Try
            'now that we have a circle, we can calculate the percentile values underneath
            Dim utils As New MapWinGIS.Utils
            Dim Values As New List(Of Double)
            Dim r As Integer, c As Integer
            Dim tmpPath As String = tempDir & "\" & Me.Setup.GeneralFunctions.WindowsSafeFilename(ObjectID) & ".tif"

            'clip the grid by using our shape as a mask
            If Not ReuseExistingClippedGrid OrElse Not System.IO.File.Exists(tmpPath) Then
                utils.ClipGridWithPolygon2(Grid, Shape, tmpPath, False)
            End If

            'read the clipped grid and create a list of values inside it
            Dim tmpGrid As New MapWinGIS.Grid
            tmpGrid.Open(tmpPath, , True)
            For r = 0 To tmpGrid.Header.NumberRows - 1
                For c = 0 To tmpGrid.Header.NumberCols - 1
                    If tmpGrid.Value(c, r) <> tmpGrid.Header.NodataValue Then
                        Values.Add(tmpGrid.Value(c, r))
                    End If
                Next
            Next

            If Values.Count = 0 Then
                'no result, but this is not a reason to return false
                Return True
            Else
                'read the results to our list
                Results = New List(Of Double)
                For Each Percentile As Double In Percentiles
                    Results.Add(Me.Setup.GeneralFunctions.PercentileFromList(Values, Percentile) * GridValuesMultiplier)
                Next
                Return True
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in Function GridPercentilesFromShape: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function GetUniqueValues(ByRef Values As List(Of Single)) As Boolean
        Try
            Dim NoData As Object = Grid.Header.NodataValue
            Dim nRows As Integer = Grid.Header.NumberRows
            Dim nCols As Integer = Grid.Header.NumberCols
            Dim Vals() As Single
            Me.Setup.GeneralFunctions.UpdateProgressBar("Collecting unique values from " & Path & "...", 0, 10, True)
            For r = 0 To nRows - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", r, nRows)

                ReDim Vals(0 To nCols - 1)
                Grid.GetRow(r, Vals(0))
                For c = 0 To nCols - 1
                    If Vals(c) <> NoData AndAlso Not Values.Contains(Vals(c)) Then
                        Values.Add(Vals(c))
                    End If
                Next
                'For c = 0 To Grid.Header.NumberCols - 1
                '    If Grid.Value(c, r) <> NoData AndAlso Not Values.Contains(Grid.Value(c, r)) Then
                '        Values.Add(Grid.Value(c, r))
                '    End If
                'Next
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function clipByPolygonToVRT(ByVal OutputFileName As String, PolygonFilePath As String) As Boolean
        'this function uses GDAL (version 1, as implemented in MapWinGIS 4.94)
        'it creates a vrt file that represents a (virtually) clipped portion of the raster
        'the advantage is that it is not necessary to actually clip the grid and save it to file. This saves a lot of time!
        'example from QGis GDAL: gdalwarp -overwrite -multi -s_srs EPSG:28992 -q -cutline C:/temp/test.shp -dstalpha -of VRT D:/SYNC/PROJECTEN/H1210.WSHD/11.Catchment_Builder/HW/Input/AHN/AHN2.vrt C:/temp/test.vrt
        Try
            Dim ut As New MapWinGIS.Utils
            ut.GDALWarp(Path, OutputFileName, "-ot Float32 -overwrite -multi -crop_to_cutline -cutline " & Chr(34) & PolygonFilePath & Chr(34) & " -of vrt ")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error clipping raster by polygon to VRT: " & PolygonFilePath)
            Return False
        End Try
    End Function

    Public Function clipByPolygonToTIF(ByVal OutputFileName As String, PolygonFilePath As String, DataType As GeneralFunctions.enmGDALDataType, CellSize As Double) As Boolean
        'this function uses GDAL (version 1, as implemented in MapWinGIS 4.94)
        'example from QGis GDAL: gdalwarp -overwrite -multi -s_srs EPSG:28992 -q -cutline C:/temp/test.shp -dstalpha -of VRT D:/SYNC/PROJECTEN/H1210.WSHD/11.Catchment_Builder/HW/Input/AHN/AHN2.vrt C:/temp/test.vrt
        'gdalwarp -dstnodata -999 -q -cutline D:\SYNC\PROJECTEN\H1272.RIJNLAND.WSA\02.CatchmentBuilder\output\GIS\Area_OR-4.01.OB_W.shp -crop_to_cutline -tr 5.0 5.0 -of GMT D:\SYNC\PROJECTEN\H1272.RIJNLAND.WSA\02.CatchmentBuilder\output\GIS\soil.tif D:/SYNC/PROJECTEN/H1272.RIJNLAND.WSA/02.CatchmentBuilder/output/clipclip.tif
        'gdalwarp -dstnodata -999 -q -cutline D:\SYNC\PROJECTEN\H1272.RIJNLAND.WSA\02.CatchmentBuilder\output\GIS\Area_OR-4.01.OB_W.shp -crop_to_cutline -tr 5.0 5.0 -of GMT D:\SYNC\PROJECTEN\H1272.RIJNLAND.WSA\02.CatchmentBuilder\output\GIS\soil.tif D:/SYNC/PROJECTEN/H1272.RIJNLAND.WSA/02.CatchmentBuilder/output/clipclip.tif

        'SIEBE: NIET GEBRUIKEN CROP TO CUTLINE WERKT NIET!!!!!!

        Try
            Dim ut As New MapWinGIS.Utils
            Dim Args As String = "-dstnodata -999 -q -cutline " & PolygonFilePath & " -crop_to_cutline -tr " & CellSize & " " & CellSize & " -ot " & DataType.ToString & " -overwrite -multi -of GTiff "
            Args = "-dstnodata -999 -q -cutline " & PolygonFilePath & " -crop_to_cutline -tr " & CellSize & " " & CellSize & " -ot " & DataType.ToString & " -overwrite -of GTiff "
            Args = "-dstnodata -999 -q -cutline " & PolygonFilePath & " -crop_to_cutline -tr " & CellSize & " " & CellSize & " -ot " & DataType.ToString & " -of GTiff "
            ut.GDALWarp(Path, OutputFileName, Args)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error clipping raster by polygon to TIF: " & PolygonFilePath)
            Return False
        End Try
    End Function



    Public Function InterpolateNodataCells() As Boolean
        Dim r As Integer, c As Integer
        Dim x As Double, y As Double
        Dim Done As Boolean, Radius As Double
        Dim myCollection As List(Of Double)
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Interpolating...", 0, 10)
            For r = 0 To Grid.Header.NumberRows - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", r + 1, Grid.Header.NumberRows)
                For c = 0 To Grid.Header.NumberCols - 1

                    If Grid.Value(c, r) = Grid.Header.NodataValue Then
                        Done = False
                        Radius = 0
                        If Not getXYCenterFromRC(r, c, x, y) Then Throw New Exception("Error retrieving X and Y coordinate for cell " & r & "," & c)
                        While Not Done
                            Radius += Grid.Header.dX
                            myCollection = CellValuesFromRadius(x, y, Radius, Grid.Header.dX, False)
                            If myCollection.Count > 0 Then
                                Done = True
                                Grid.Value(c, r) = Convert.ToSingle(myCollection.Average)
                            End If
                        End While
                    End If
                Next
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Complete.", 0, 10, True)
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error while interpolating nodatacells.")
        End Try

    End Function

    Public Function CellValuesFromRadius(Xcenter As Double, Ycenter As Double, Radius As Double, StepSize As Double, Optional ByVal IncludeNodataCells As Boolean = False) As List(Of Double)
        Dim newList As New List(Of Double)
        Dim newVal As Double
        Dim Angle As Double, X As Double, Y As Double
        Dim length As Double = 2 * Math.PI * Radius
        For Dist As Integer = 0 To length Step StepSize
            Angle = Setup.GeneralFunctions.D2R(Dist / length * 360)
            X = Xcenter + Math.Sin(Angle) * Radius
            Y = Ycenter + Math.Cos(Angle) * Radius
            If GetCellValueFromXY(X, Y, newVal) Then
                If newVal <> Grid.Header.NodataValue Then newList.Add(newVal)
            End If
        Next
        Return newList
    End Function


    Public Function Crop(ByRef SourceGrid As clsRaster, ByVal XLL As Double, ByVal YLL As Double, ByVal XUR As Double, ByVal YUR As Double, ByVal ResampleMethod As STOCHLIB.GeneralFunctions.enmResampleMethod, ByVal NodataVal As Double, ByVal CellSize As Double) As Boolean

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Cropping raster...", 0, 10)
            Dim r As Long, c As Long, X As Double, Y As Double
            Dim rSource As Long, cSource As Long
            Dim newHeader As MapWinGIS.GridHeader

            'create a completly new grid, based on the header settings
            Grid = New MapWinGIS.Grid
            newHeader = New MapWinGIS.GridHeader
            newHeader.dX = CellSize
            newHeader.dY = CellSize
            newHeader.XllCenter = XLL + newHeader.dX / 2
            newHeader.YllCenter = YLL + newHeader.dY / 2
            newHeader.NumberCols = Me.Setup.GeneralFunctions.RoundUD((XUR - XLL) / newHeader.dX, 0, False)
            newHeader.NumberRows = Me.Setup.GeneralFunctions.RoundUD((YUR - YLL) / newHeader.dY, 0, False)
            newHeader.NodataValue = NodataVal

            If Not Grid.CreateNew(Path, newHeader, GridDataType.DoubleDataType, 0, True) Then Throw New Exception("Error creating new grid.")
            Call CompleteMetaHeader()

            If ResampleMethod = enmResampleMethod.CellCenter Then
                For r = 0 To Grid.Header.NumberRows - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", r + 1, Grid.Header.NumberRows)
                    For c = 0 To Grid.Header.NumberCols - 1

                        'get the cell center coordinate
                        If Not getXYCenterFromRC(r, c, X, Y) Then Throw New Exception("Error retrieving X and Y coordinate for cell " & r & "," & c)

                        'find the corresponding cell in the source grid
                        If Not Me.Setup.GridEditor.SourceGrid.GetRCFromXY(X, Y, rSource, cSource) Then
                            'set cell to nodata
                            Grid.Value(c, r) = Grid.Header.NodataValue
                        Else
                            'get the value from the source grid
                            If Me.Setup.GridEditor.SourceGrid.Grid.Value(cSource, rSource) = Me.Setup.GridEditor.SourceGrid.Grid.Header.NodataValue Then
                                Grid.Value(c, r) = NodataVal
                            Else
                                Grid.Value(c, r) = Me.Setup.GridEditor.SourceGrid.Grid.Value(cSource, rSource)
                            End If
                        End If
                    Next
                Next
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Sub Initialize(ByVal myPath As String)
        Path = myPath
    End Sub

    Public Function ReadHeader(ByVal myType As MapWinGIS.GridDataType, ByVal inRAM As Boolean) As Boolean
        If Grid.Open(Path, myType, inRAM) Then
            Call CompleteMetaHeader()
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub GetCellBounds(ByRef r As Long, ByRef c As Long, ByRef Xcs As Double, ByRef Ycs As Double, ByRef Xce As Double, ByRef Yce As Double)
        Xcs = Grid.Header.XllCenter + (c - 0.5) * Grid.Header.dX
        Xce = Grid.Header.XllCenter + (c + 0.5) * Grid.Header.dX
        Ycs = Grid.Header.YllCenter + (Grid.Header.NumberRows - r - 0.5) * Grid.Header.dY
        Yce = Grid.Header.YllCenter + (Grid.Header.NumberRows - r - 1.5) * Grid.Header.dY
    End Sub

    Public Sub getGridCenter(ByRef X As Double, ByRef Y As Double)
        X = (XLLCorner + XURCorner) / 2
        Y = (YLLCorner + YURCorner) / 2
    End Sub

    Public Function setNewPath(ByVal myPath As String, Optional ByVal Multiplier As Double = 1) As Boolean
        Try
            Path = myPath
            GridValuesMultiplier = Multiplier
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function setExistingPath(ByVal myPath As String, Optional ByVal Multiplier As Double = 1) As Boolean
        If System.IO.File.Exists(myPath) Then
            Path = myPath
            GridValuesMultiplier = Multiplier
            Return True
        Else
            Return False
        End If
    End Function

    Public Function PointInside(ByRef myPoint As MapWinGIS.Point) As Boolean
        If myPoint.x >= XLLCorner AndAlso myPoint.x < XURCorner Then
            If myPoint.y > YLLCorner AndAlso myPoint.y < YURCorner Then
                Return True
            End If
        End If
        Return False
    End Function


    Public Function ShapeInside(ByRef myShape As MapWinGIS.Shape) As Boolean
        If myShape.Extents.xMin >= XLLCorner AndAlso myShape.Extents.xMax <= XURCorner Then
            If myShape.Extents.yMin >= YLLCorner AndAlso myShape.Extents.yMax <= YURCorner Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Function ShapeOverlaps(ByRef myShape As MapWinGIS.Shape) As Boolean
        If myShape.Extents.xMin >= XLLCorner AndAlso myShape.Extents.xMin <= XURCorner AndAlso myShape.Extents.yMin >= YLLCorner AndAlso myShape.Extents.yMin <= YLLCorner Then Return True
        If myShape.Extents.xMin >= XLLCorner AndAlso myShape.Extents.xMin <= XURCorner AndAlso myShape.Extents.yMax >= YLLCorner AndAlso myShape.Extents.yMax <= YLLCorner Then Return True
        If myShape.Extents.xMax >= XLLCorner AndAlso myShape.Extents.xMax <= XURCorner AndAlso myShape.Extents.yMin >= YLLCorner AndAlso myShape.Extents.yMin <= YLLCorner Then Return True
        If myShape.Extents.xMax >= XLLCorner AndAlso myShape.Extents.xMax <= XURCorner AndAlso myShape.Extents.yMax >= YLLCorner AndAlso myShape.Extents.yMax <= YLLCorner Then Return True
        Return False
    End Function

    Public Sub Save()
        If Not Path = "" Then
            'save under the given filename
            Grid.Save(Path)
        Else
            'save under the original name
            Grid.Save()
        End If
    End Sub

    Public Sub SaveAs(savePath As String)
        Grid.Save(savePath)
    End Sub

    Public Sub Close()
        Grid.Close()
    End Sub

    Public Sub InitializeSelected()
        Dim Rows As Integer = Grid.Header.NumberRows
        Dim Cols As Integer = Grid.Header.NumberCols
        ReDim Selected(0 To Rows - 1, 0 To Cols - 1)
    End Sub

    Public Sub calcStats()
        Dim r As Integer, c As Integer
        Dim myVal As Double

        Try
            Stats.min = 9999999
            Stats.max = -9999999
            Stats.nSelected = 0
            Stats.nRow = nRow
            Stats.nCol = nCol
            Stats.NodataVal = Grid.Header.NodataValue
            Stats.nNoData = 0

            Setup.GeneralFunctions.UpdateProgressBar("Getting stats for grid " & Path, 0, 10)
            For r = 0 To Stats.nRow - 1
                Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), Stats.nRow)
                For c = 0 To Stats.nCol - 1
                    If Selected(r, c) Then Stats.nSelected += 1
                    myVal = Grid.Value(c, r)
                    If (myVal - NoDataVal) > 1 Then  'to avoid some rounding errors...
                        If myVal < Stats.min Then Stats.min = myVal
                        If myVal > Stats.max Then Stats.max = myVal
                    Else
                        Stats.nNoData += 1
                    End If
                Next
            Next
            Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try


    End Sub

    Public Function ElevationTableByPolygon(ByVal ID As String, ByRef Shape As MapWinGIS.Shape, ByRef Lowestpoint As clsXYZ, Optional ByVal DivideByReachLength As Double = 1, Optional ByVal ElevationMultiplier As Double = 1) As clsSobekTable

        'This function builds an elevation table for a shape from the current grid
        'if the shape extents allow to do so, the grid will be clipped and the clipped grid processed
        'otherwise the overall grid will be sampled
        Dim myTable As New clsSobekTable(Me.Setup)
        myTable = Me.Setup.GISData.ElevationGrid.ElevationTableByPolygonByClipping(ID, Shape, Lowestpoint, 1, Me.Setup.Settings.GISSettings.ElevationMultiplier)

        'if this (fast!) method did not succeed, it might be that nowhere the polygon overlaps an entire grid cell.
        'in that case, try again, but this time without clipping (=slow!)
        If myTable Is Nothing OrElse myTable.XValues.Count = 0 Then
            myTable = Me.Setup.GISData.ElevationGrid.ElevationTableByPolygonNoClipping(ID, Shape, Lowestpoint, 1, Me.Setup.Settings.GISSettings.ElevationMultiplier)
        End If

        'if we didn't succeed, write an error
        If myTable Is Nothing OrElse myTable.XValues.Count = 0 Then
            Me.Setup.Log.AddError("Error building elevation table for shape " & ID)
        End If

        'return the result
        Return myTable

    End Function

    Public Function BuildYZTableFromRadius(ByVal CenterX As Double, ByVal CenterY As Double, ByVal LineAngle As Double, ByVal maxRadius As Integer, ByVal stepSize As Integer, ByVal TruncateAtFirstNodataCell As Boolean) As clsSobekTable
        Dim newTable As New clsSobekTable(Me.Setup)
        Dim i As Integer, X As Double, Y As Double, Z As Double
        Dim r As Long, c As Long

        'date: 20-2-2015
        'author: Siebe Bosch
        'description: drapes a line over the elevation grid and returns the results as an YZ-table (distance-elevation)
        'find the distance on the left side from which the profile cross section exceeds the surface level
        For i = -maxRadius To maxRadius Step stepSize
            X = CenterX + i * Math.Sin(LineAngle)
            Y = CenterY + i * Math.Cos(LineAngle)
            Me.Setup.GISData.ElevationGrid.GetRCFromXY(X, Y, r, c)                      'find corresponding row and column in the grid
            Z = Me.Setup.GISData.ElevationGrid.Grid.Value(c, r)                         'get value from the elevation grid
            If Z <> Me.Setup.GISData.ElevationGrid.NoDataVal Then
                newTable.AddDataPair(4, i, Z, X, Y)
            ElseIf TruncateAtFirstNodataCell Then  'value = nodata and truncate at nodata cells is active
                If i < 0 Then
                    'encountered a nodata cell so truncate left by clearing the table and starting over
                    newTable = New clsSobekTable(Me.Setup)
                ElseIf i > 0 Then
                    'encountered a nodata cell to the right, which means we're done. Exit the loop
                    Exit For
                ElseIf i = 0 Then
                    'the center cell itself is nodata
                    Me.Setup.Log.AddError("Could not build an YZ table for location " & CenterX & "," & CenterY & " since it is located on a nodata-cell. Deactivate the truncate by first nodata cell option and try again.")
                    Return Nothing
                End If
            End If
        Next
        Return newTable
    End Function

    Public Function BuildYZTableFromLineSegment(ByVal CenterX As Double, ByVal CenterY As Double, Xstart As Double, Ystart As Double, Xend As Double, yEnd As Double, ByVal stepSize As Double, ByVal TruncateAtFirstNodataCell As Boolean, Optional ByVal LowerThreshold As Double = -9.0E+99) As clsSobekTable
        Dim newTable As New clsSobekTable(Me.Setup)
        Dim dist As Double, X As Double, Y As Double, Z As Single
        Dim r As Long, c As Long
        Dim DistLeft As Double = Setup.GeneralFunctions.Pythagoras(CenterX, CenterY, Xstart, Ystart)
        Dim DistRight As Double = Setup.GeneralFunctions.Pythagoras(CenterX, CenterY, Xend, yEnd)
        Dim LineAngle As Double = Me.Setup.GeneralFunctions.D2R(Me.Setup.GeneralFunctions.NormalizeAngle(Setup.GeneralFunctions.LineAngleDegrees(Xstart, Ystart, Xend, yEnd)))
        Dim myNoDataVal As Single = Convert.ToSingle(NoDataVal)

        'date: 20-2-2015
        'author: Siebe Bosch
        'description: drapes a line over the elevation grid and returns the results as an YZ-table (distance-elevation)
        'find the distance on the left side from which the profile cross section exceeds the surface level
        'v2.203: replaced the integer used for stepsize by a double, making steps < 1 m possible
        If stepSize = 0 Then
            stepSize = 0.5
            Me.Setup.Log.AddError("Could not walk along the line segment since the assigned step size was 0. A stepsize of 0.5 was applied instead.")
        End If

        dist = -DistLeft
        While Not dist > DistRight
            dist += stepSize
            X = CenterX + dist * Math.Sin(LineAngle)
            Y = CenterY + dist * Math.Cos(LineAngle)
            Me.Setup.GISData.ElevationGrid.GetRCFromXY(X, Y, r, c)                      'find corresponding row and column in the grid
            Z = Convert.ToSingle(Me.Setup.GISData.ElevationGrid.Grid.Value(c, r))                         'get value from the elevation grid
            If Me.Setup.GISData.ElevationGrid.Grid.Value(c, r) <> Me.Setup.GISData.ElevationGrid.Grid.Header.NodataValue Then
                If Z > LowerThreshold Then
                    newTable.AddDataPair(4, dist, Z, X, Y)
                End If
            ElseIf TruncateAtFirstNodataCell Then  'value = nodata and truncate at nodata cells is active
                If dist < 0 Then
                    'encountered a nodata cell so truncate left by clearing the table and starting over
                    newTable = New clsSobekTable(Me.Setup)
                ElseIf dist > 0 Then
                    'encountered a nodata cell to the right, which means we're done. Exit the loop
                    Exit While
                ElseIf dist = 0 Then
                    'the center cell itself is nodata
                    Me.Setup.Log.AddError("Could not build an YZ table for location " & CenterX & "," & CenterY & " since it is located on a nodata-cell. Deactivate the truncate by first nodata cell option and try again.")
                    Return Nothing
                End If
            End If
        End While
        Return newTable
    End Function

    Public Function ElevationTableByPolygonNoClipping(ByVal ID As String, ByVal Shape As MapWinGIS.Shape, ByRef Lowestpoint As clsXYZ, Optional ByVal DivideByReachLength As Double = 1, Optional ByVal ElevationMultiplier As Double = 1) As clsSobekTable

        'this function builds an elevation table for a shape from the grid without clipping the grid first
        'reason for this function is that too small shapes do not succeed in clipping the grid (returns an empty grid)
        Dim n As Long = 0, nCum As Long = 0
        Dim r As Long, c As Long, myVal As Single
        Dim myTable As New clsSobekTable(Me.Setup)
        Dim r1 As Integer, r2 As Integer, c1 As Integer, c2 As Integer
        Dim myUtils As New MapWinGIS.Utils
        Lowestpoint.Z = 99999999
        Dim myPoint As New MapWinGIS.Point

        Try

            Call GetRCFromXY(Shape.Extents.xMin, Shape.Extents.yMax, r1, c1) 'notice that mapwindow calculates row number from top to bottom
            Call GetRCFromXY(Shape.Extents.xMax, Shape.Extents.yMin, r2, c2) 'notice that mapwindow calculates row number from top to bottom

            Me.Setup.GeneralFunctions.UpdateProgressBar("Building elevation table from overall elevation raster for area " & ID, 0, 1)
            For r = r1 To r2
                Me.Setup.GeneralFunctions.UpdateProgressBar("", r + 1, nRow)
                For c = c1 To c2
                    Call getXYCenterFromRC(r, c, myPoint.x, myPoint.y)

                    If myUtils.PointInPolygon(Shape, myPoint) Then
                        myVal = Me.Setup.GISData.ElevationGrid.Grid.Value(c, r)
                        If myVal > -999 Then ' Math.Abs(myVal - myRaster.NoDataVal) > 1 Then 'there is a bug in MapWinGis when clipping grids. Nodata-values are shifted by 1. This is a correction
                            n += 1
                            myVal = Math.Round(Grid.Value(c, r), 2)
                            If myVal < Lowestpoint.Z Then 'maak van de huidige cel het laagste punt
                                Me.Setup.GISData.ElevationGrid.getXYCenterFromRC(r, c, Lowestpoint.X, Lowestpoint.Y)
                            End If
                            If myTable.ElevationCollection.ContainsKey(myVal) Then
                                myTable.ElevationCollection.Item(myVal) += 1
                            Else
                                myTable.ElevationCollection.Add(myVal, 1)
                            End If
                        End If
                    End If
                Next
            Next

            'to get a reasonable estimate for the lowest point after all, take the interior point
            If Not Shape Is Nothing AndAlso Lowestpoint.X = 0 Then
                Lowestpoint.X = Shape.InteriorPoint.x
                Lowestpoint.Y = Shape.InteriorPoint.y
            End If

            '----------------------------------------------------------------------------------------
            ' start sorting the data we found!
            '----------------------------------------------------------------------------------------
            If myTable.ElevationCollection.Count = 0 Then
                Return Nothing
            Else
                If Not myTable.SortElevationData(ElevationMultiplier, dX, DivideByReachLength) Then Throw New Exception("Could not build elevation table from grid for area " & ID)
                Return myTable
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Debug.Print("Error in function ElevationTableFromGrid for area " & ID)
            Me.Setup.Log.AddError("Error in function ElevationTableFromGrid for area " & ID)
            Return Nothing
        End Try

    End Function

    Public Function ElevationTableByPolygonByClipping(ByVal ID As String, ByRef Shape As MapWinGIS.Shape, ByRef Lowestpoint As clsXYZ, Optional ByVal DivideByReachLength As Double = 1, Optional ByVal ElevationMultiplier As Double = 1) As clsSobekTable

        'this routine builds an elevation table by first clipping the raster by shape
        'then filling and sorting the elevation table
        Dim myGridPath As String = Me.Setup.Settings.ExportDirGIS & "\ahn_clip_" & ID.Trim & ".tif"
        Dim myUtils As New MapWinGIS.Utils
        Dim ElevationGrid As clsRaster
        Dim ElevationStorage As New clsSobekTable(Me.Setup)

        'here we will decide if the clipped grid can be loaded into memory (InRam) or whether it is too large to fit
        'v2.108: the code below refered to me.setup.GISData.Elevationgrid. However, this is an in-class function. It should refer to itself
        Dim nCells As Long = (Shape.Extents.xMax - Shape.Extents.xMin) / Me.Grid.Header.dX * (Shape.Extents.yMax - Shape.Extents.yMin) / Me.Grid.Header.dY
        Dim InRam As Boolean = nCells < 200000000.0 'assume that a 5x10 km grid with 0.5m resolution is the maximum to still fit in memory

        'clip the current area (using the shape) from the total elevation grid
        If System.IO.File.Exists(myGridPath) Then System.IO.File.Delete(myGridPath)

        If Me.Grid.Header.NodataValue = 0 Then Throw New Exception("Critical error: elevation grid has an invalid nodata-value of 0. Please supply a valid elevation grid.")
        Call myUtils.ClipGridWithPolygon(Path, Shape, myGridPath, False)

        'set the newly clipped grid as the elevation grid for this area
        ElevationGrid = New clsRaster(Me.Setup)
        ElevationGrid.setExistingPath(myGridPath)

        'finally calculate the storage table for the (clipped) grid that's associated with this area
        If myUtils.LastErrorCode = 0 Then
            ElevationStorage = ElevationGrid.BuildElevationTable(ID, Shape, Lowestpoint, 1, Me.Setup.Settings.GISSettings.ElevationMultiplier,,, InRam)
        End If
        myUtils = Nothing
        Return ElevationStorage

    End Function


    Public Function BuildElevationTable(ByVal ID As String, ByRef Shape As MapWinGIS.Shape, ByRef LowestPoint As clsXYZ, Optional ByVal DivideByReachLength As Double = 1, Optional ByVal ElevationMultiplier As Double = 1, Optional ByRef MaskGrid As clsRaster = Nothing, Optional ByVal GridAlreadyOpen As Boolean = False, Optional ByVal InMemory As Boolean = False) As clsSobekTable

        Dim n As Long = 0, nCum As Long = 0
        Dim r As Long, c As Long, myVal As Single
        Dim myTable As New clsSobekTable(Me.Setup)
        LowestPoint.Z = 99999999

        Try
            If ElevationMultiplier = 0 Then Throw New Exception("Error building elevation table: elevation multiplier is set to zero. Please check the internal settings.")

            Me.Setup.GeneralFunctions.UpdateProgressBar("Building elevation table from raster " & Path, 0, 1)

            If Not System.IO.File.Exists(Path) Then
                Throw New Exception("Error in function ElevationTableFromGrid. Grid does not exist" & Path)
            ElseIf Not GridAlreadyOpen Then
                If Not Grid.Open(Path, MapWinGIS.GridDataType.UnknownDataType, InMemory, MapWinGIS.GridFileType.UseExtension) Then Throw New Exception("Error in function ElevationTableFromGrid. Could not read grid " & Path)
                CompleteMetaHeader()
            End If

            If Not MaskGrid Is Nothing Then

                If MaskGrid.XLLCorner <> XLLCorner OrElse MaskGrid.YURCorner <> YURCorner OrElse MaskGrid.CellArea <> CellArea Then Throw New Exception("Error: masking grid does not match elevation grid extents. " & MaskGrid.Path)
                'only process the cells that fall inside the mask
                For r = 0 To nRow - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", r + 1, nRow)
                    For c = 0 To nCol - 1
                        If MaskGrid.Grid.Value(c, r) = 1 Then
                            myVal = Math.Round(Grid.Value(c, r), 2)
                            'v2.108: attempting to fix the previous stupid work-around (myval > -999) by directly comparing the cellvalue with the nodata-value
                            If Grid.Value(c, r) <> Grid.Header.NodataValue AndAlso Grid.Value(c, r) > -1.0E+33 AndAlso Grid.Value(c, r) < 1.0E+33 Then
                                n += 1
                                If myVal < LowestPoint.Z Then 'maak van de huidige cel het laagste punt
                                    If Me.Setup.GeneralFunctions.GetMapWinGridCellCenterXY(XLLCenter, YLLCenter, dX, dY, nRow, nCol, r, c, LowestPoint.X, LowestPoint.Y) Then LowestPoint.Z = myVal
                                End If
                                If myTable.ElevationCollection.ContainsKey(myVal) Then
                                    myTable.ElevationCollection.Item(myVal) += 1
                                Else
                                    myTable.ElevationCollection.Add(myVal, 1)
                                End If
                            End If
                            'If myVal > -999 Then ' Math.Abs(myVal - myRaster.NoDataVal) > 1 Then 'there is a bug in MapWinGis when clipping grids. Nodata-values are shifted by 1. This is a correction
                            'End If
                        End If
                    Next
                Next

            Else
                'process all cells
                For r = 0 To nRow - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", r + 1, nRow)
                    For c = 0 To nCol - 1
                        'v1.72: this is an attempt to fix the nodata-value mismatch when converting it to double precision. We compare the cell value directly to the in memory nodata-value now
                        If Grid.Value(c, r) <> Grid.Header.NodataValue AndAlso Grid.Value(c, r) > -1.0E+33 AndAlso Grid.Value(c, r) < 1.0E+33 Then
                            myVal = Math.Round(Grid.Value(c, r), 2)

                            'If myVal > -999 Then ' Math.Abs(myVal - myRaster.NoDataVal) > 1 Then 'there is a bug in MapWinGis when clipping grids. Nodata-values are shifted by 1. This is a correction
                            n += 1
                            If myVal < LowestPoint.Z Then 'maak van de huidige cel het laagste punt
                                If Me.Setup.GeneralFunctions.GetMapWinGridCellCenterXY(XLLCenter, YLLCenter, dX, dY, nRow, nCol, r, c, LowestPoint.X, LowestPoint.Y) Then LowestPoint.Z = myVal
                            End If

                            If myTable.ElevationCollection.ContainsKey(myVal) Then
                                myTable.ElevationCollection.Item(myVal) += 1
                            Else
                                myTable.ElevationCollection.Add(myVal, 1)
                            End If
                            'End If

                        End If

                    Next
                Next
            End If

            If Not GridAlreadyOpen Then
                Grid.Close()
            End If

            'to get a reasonable estimate for the lowest point after all, take the interior point
            If Not Shape Is Nothing AndAlso LowestPoint.X = 0 Then
                LowestPoint.X = Shape.InteriorPoint.x
                LowestPoint.Y = Shape.InteriorPoint.y
            End If

            '----------------------------------------------------------------------------------------
            ' start sorting the data we found!
            '----------------------------------------------------------------------------------------
            If myTable.ElevationCollection.Count = 0 Then
                Me.Setup.Log.AddError("No valid elevation data found for area " & ID)
                Return Nothing
            Else
                If Not myTable.SortElevationData(ElevationMultiplier, dX, DivideByReachLength) Then Throw New Exception("Could not build elevation table from grid for area " & ID)
                Return myTable
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Debug.Print("Error in function ElevationTableFromGrid for area " & ID)
            Me.Setup.Log.AddError("Error in function ElevationTableFromGrid for area " & ID)
            Return Nothing
        End Try

    End Function

    Public Function BuildStorageTable(ByVal ID As String, ByRef Shape As MapWinGIS.Shape, Optional ByVal DivideByReachLength As Double = 1, Optional ByVal ElevationMultiplier As Double = 1, Optional ByVal GridAlreadyOpen As Boolean = False, Optional ByVal InMemory As Boolean = False, Optional nDecimals As Integer = 2) As clsSobekTable

        Dim n As Long = 0, nCum As Long = 0
        Dim r As Long, c As Long, myVal As Single
        Dim myTable As New clsSobekTable(Me.Setup)
        Dim myUtils As New MapWinGIS.Utils
        Dim myPoint As New MapWinGIS.Point
        Dim tmpGrid As MapWinGIS.Grid, tmpGridPath As String

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building storage table from grid.", 0, 1)

            If Not GridAlreadyOpen Then
                If Not Grid.Open(Path, MapWinGIS.GridDataType.UnknownDataType, InMemory, MapWinGIS.GridFileType.UseExtension) Then Throw New Exception("Error in function ElevationTableFromGrid. Could not read grid " & Path)
                CompleteMetaHeader()
            End If

            'start by clipping the grid by polygon
            tmpGridPath = Me.Setup.Settings.ExportDirRoot & "\tmpgrid.tif"
            If System.IO.File.Exists(tmpGridPath) Then System.IO.File.Delete(tmpGridPath)
            myUtils.ClipGridWithPolygon2(Grid, Shape, Me.Setup.Settings.ExportDirRoot & "\tmpgrid.tif", False)

            tmpGrid = New MapWinGIS.Grid
            tmpGrid.Open(tmpGridPath)

            'process all cells
            For r = 0 To tmpGrid.Header.NumberRows - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", r + 1, tmpGrid.Header.NumberRows)
                For c = 0 To tmpGrid.Header.NumberCols
                    If tmpGrid.Value(c, r) > -999 Then ' Math.Abs(myVal - myRaster.NoDataVal) > 1 Then 'there is a bug in MapWinGis when clipping grids. Nodata-values are shifted by 1. This is a correction
                        n += 1
                        myVal = Math.Round(tmpGrid.Value(c, r), nDecimals)
                        If myTable.ElevationCollection.ContainsKey(myVal) Then
                            myTable.ElevationCollection.Item(myVal) += 1
                        Else
                            myTable.ElevationCollection.Add(myVal, 1)
                        End If
                    End If
                Next
            Next

            tmpGrid.Close()

            If Not GridAlreadyOpen Then
                Grid.Close()
            End If

            '----------------------------------------------------------------------------------------
            ' start sorting the data we found!
            '----------------------------------------------------------------------------------------
            If myTable.ElevationCollection.Count = 0 Then
                Me.Setup.Log.AddError("No valid elevation data found for area " & ID)
                Return Nothing
            Else
                'If Not myTable.SortElevationData(ElevationMultiplier, dX, DivideByReachLength) Then Throw New Exception("Could not build elevation table from grid for area " & ID)
                If Not myTable.SortStorageData(ElevationMultiplier, dX, DivideByReachLength) Then Throw New Exception("Could not build storage table from grid for area " & ID)
                Return myTable
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Debug.Print("Error in function ElevationTableFromGrid for area " & ID)
            Me.Setup.Log.AddError("Error in function ElevationTableFromGrid for area " & ID)
            Return Nothing
        End Try

    End Function



    Public Sub selectAll(ByVal IgnoreNodataCells As Boolean)
        Dim r As Long, c As Long

        ReDim Selected(nRow - 1, nCol - 1)
        Stats.nSelected = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Busy selecting all cells...", 0, 10)
        If IgnoreNodataCells Then
            For r = 0 To nRow - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
                For c = 0 To nCol - 1
                    If Grid.Value(c, r) <> NoDataVal Then
                        Selected(r, c) = True
                        Stats.nSelected += 1
                    End If
                Next
            Next
        Else
            For r = 0 To nRow - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
                For c = 0 To nCol - 1
                    Selected(r, c) = True
                    Stats.nSelected += 1
                Next
            Next
        End If

        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub


    Public Sub clearSelection()
        Dim r As Long, c As Long
        Stats.nSelected = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Clearing selection...", 0, 10)
        For r = 0 To nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
            For c = 0 To nCol - 1
                Selected(r, c) = False
            Next
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub

    Public Sub invertSelection()
        Dim r As Long, c As Long
        Stats.nSelected = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Inverting selection...", 0, 10)
        For r = 0 To nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
            For c = 0 To nCol - 1
                If Selected(r, c) Then
                    Selected(r, c) = False
                    Stats.nSelected -= 1
                Else
                    Selected(r, c) = True
                    Stats.nSelected += 1
                End If
            Next
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub


    Public Sub AddValueRangeToSelection(ByVal FromVal As Double, ByVal ToVal As Double, ByVal IgnoreNodataCells As Boolean)
        Dim r As Integer, c As Integer

        Stats.nSelected = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Adding cells within value range to selection...", 0, 10)
        For r = 0 To nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
            If IgnoreNodataCells Then
                For c = 0 To nCol - 1
                    If Grid.Value(c, r) <> NoDataVal AndAlso Grid.Value(c, r) >= FromVal AndAlso Grid.Value(c, r) <= ToVal Then
                        Selected(r, c) = True
                        Stats.nSelected += 1
                    ElseIf Selected(r, c) Then
                        Stats.nSelected += 1
                    End If
                Next
            Else
                For c = 0 To nCol - 1
                    If Grid.Value(c, r) >= FromVal AndAlso Grid.Value(c, r) <= ToVal Then
                        Selected(r, c) = True
                        Stats.nSelected += 1
                    ElseIf Selected(r, c) Then
                        Stats.nSelected += 1
                    End If
                Next
            End If
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)

    End Sub

    Public Sub RemoveValueRangeFromSelection(ByVal FromVal As Double, ByVal ToVal As Double, ByVal IgnoreNodataCells As Boolean)
        Dim r As Integer, c As Integer

        Stats.nSelected = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Removing cells within value range from selection...", 0, 10)
        For r = 0 To nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
            If IgnoreNodataCells Then
                For c = 0 To nCol - 1
                    If Grid.Value(c, r) <> NoDataVal AndAlso Grid.Value(c, r) >= FromVal AndAlso Grid.Value(c, r) <= ToVal Then
                        Selected(r, c) = False
                    ElseIf Selected(r, c) Then
                        Stats.nSelected += 1
                    End If
                Next
            Else
                For c = 0 To nCol - 1
                    If Grid.Value(c, r) >= FromVal AndAlso Grid.Value(c, r) <= ToVal Then
                        Selected(r, c) = False
                    ElseIf Selected(r, c) Then
                        Stats.nSelected += 1
                    End If
                Next
            End If
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)

    End Sub

    Public Sub RemoveNoDataCellsFromSelection()
        Dim r As Integer, c As Integer
        Stats.nSelected = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Removing nodata-cells from selection...", 0, 10)
        For r = 0 To nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
            For c = 0 To nCol - 1
                If Grid.Value(c, r) = NoDataVal Then
                    Selected(r, c) = False
                ElseIf Selected(r, c) Then
                    Stats.nSelected += 1
                End If
            Next
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub

    Public Sub AddSelectionGrid(ByRef Selectiongrid As clsRaster)

        'Haalt de celselectie op uit een zogeheten selectiegrid
        'Alle cellen waarbij de waarde ongelijk is aan nodata-value, worden beschouwd als "selectie"
        Dim r As Integer, c As Integer
        Dim myVal As Single

        If Not Selectiongrid.Read(False) Then Throw New Exception("Error reading selection grid.")

        Stats.nSelected = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Collecting values from the selection grid...", 0, 10)
        For r = 0 To Selectiongrid.nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", r + 1, Selectiongrid.nRow)
            For c = 0 To Selectiongrid.nCol - 1
                myVal = Selectiongrid.Grid.Value(c, r)
                If myVal <> Selectiongrid.NoDataVal Then                  'alle waarden die <> nodataval zijn, beschouwen we als een 'selectie'
                    Selected(r, c) = True
                    Stats.nSelected += 1
                End If
            Next
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub


    Public Function AddSelectionFromShape(ByRef PolySF As STOCHLIB.clsShapeFile) As Boolean
        'waarschijnlijk niet de snelste routine, maar 'gets the job done'
        'doorloopt alle cellen in het raster en voegt ze toe aan de selectie als ze binnen de gegeven shape liggen
        Dim r As Integer, c As Integer, ShapeIdx As Integer
        Dim x As Double, y As Double
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Adding selection by shape...", 0, 10)
            PolySF.sf.BeginPointInShapefile()
            For r = 0 To Grid.Header.NumberRows - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", r, Grid.Header.NumberRows - 1)
                For c = 0 To Grid.Header.NumberCols - 1
                    getXYCenterFromRC(r, c, x, y)
                    ShapeIdx = PolySF.GetShapeIdxByCoord(x, y)
                    If ShapeIdx >= 0 Then
                        Selected(r, c) = True
                        Stats.nSelected += 1
                    End If
                Next
            Next
            PolySF.sf.EndPointInShapefile()
            Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub CreateFromShapeFile(ByRef SelectionShapeFile As clsSubcatchmentDataSource, ByVal FileName As String, ByVal XLLCorner As Double, YLLCorner As Double, XURCorner As Double, YURCorner As Double, dX As Double, dY As Double, NoDataVal As Double)
        Try

            'Haalt de celselectie op uit een shapefile
            Path = FileName
            Dim myUtils As New Utils
            Dim Ext As String = Setup.GeneralFunctions.getExtensionFromFileName(Path)

            'The routine below has the problem that it only creates a grid for the OUTLINES of the polygons, not the area
            'SelectionGrid.Grid = myUtils.ShapefileToGrid(SelectionShapeFile.PolySF.sf, False, Grid.Header, dX, True, 2)
            'SelectionGrid.Grid.Save("c:\temp\selectiongrid.asc", MapWinGIS.GridFileType.Ascii)
            'therefore we switch to using GDAL
            Dim myArgs As String

            Select Case Ext.Trim.ToUpper
                Case Is = "ASC"
                    myArgs = " -of AAIGrid -burn 1 -a_nodata " & NoDataVal & " -te " & XLLCorner & " " & YLLCorner & " " & XURCorner & " " & YURCorner & " -tr " & dX & " " & dY & " "
                Case Is = "TIF", "TIFF"
                    myArgs = " -of GTiff -burn 1 -a_nodata " & NoDataVal & " -te " & XLLCorner & " " & YLLCorner & " " & XURCorner & " " & YURCorner & " -tr " & dX & " " & dY & " "
                Case Else
                    myArgs = " -of HFA -burn 1 -a_nodata " & NoDataVal & " -te " & XLLCorner & " " & YLLCorner & " " & XURCorner & " " & YURCorner & " -tr " & dX & " " & dY & " "
            End Select
            If Not myUtils.GDALRasterize(SelectionShapeFile.PolySF.Path, Path, myArgs) Then Throw New Exception("Error rasterizing shapefile.")
            Setup.GeneralFunctions.UpdateProgressBar("Selection grid successfully created from shapefile.", 0, 10)
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try

    End Sub

    Public Sub setNodata()
        Dim r As Long, c As Long
        Me.Setup.GeneralFunctions.UpdateProgressBar("Setting cell values to nodata...", 0, 10)
        For r = 0 To Grid.Header.NumberRows - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", r, nRow, False)
            'Debug.Print("row" & r & "," & Grid.Value(708, 0))  'SIEBE: let op, laat deze routine voorlopig staan wegens bug in mapwingis 4.9. Iets met schrijfsnelheid zorgt dat de veranderingen aan je grid anders niet doorkomen. Mail aan paul meems 18-7-2015
            For c = 0 To Grid.Header.NumberCols - 1
                If Selected(r, c) AndAlso Not Grid.Value(c, r) = NoDataVal Then
                    Grid.PutFloatWindow2(r, r, c, c, NoDataVal)
                End If
            Next
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub

    Public Sub AddNoneNoDataCellsToSelection()
        Dim r As Integer, c As Integer

        ReDim Selected(nRow - 1, nCol - 1)
        Stats.nSelected = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Adding nodata-cells to selection...", 0, 10)
        For r = 0 To nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
            For c = 0 To nCol - 1
                If Grid.Value(c, r) = NoDataVal Then
                    Selected(r, c) = False
                Else
                    Selected(r, c) = True
                    Stats.nSelected += 1
                End If
            Next
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub

    Public Sub multConstant(ByVal myVal As Single)
        Dim r As Long, c As Long
        Me.Setup.GeneralFunctions.UpdateProgressBar("Multiplying cell value...", 0, 10)
        For r = 0 To nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
            For c = 0 To nCol - 1
                If Selected(r, c) Then
                    Grid.PutFloatWindow(r, r, c, c, Grid.Value(c, r) * myVal)
                End If
            Next
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub


    Public Sub addConstant(ByVal myVal As Single)
        Dim r As Long, c As Long

        Me.Setup.GeneralFunctions.UpdateProgressBar("Adding constant to selected cells...", 0, 10)
        For r = 0 To nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
            For c = 0 To nCol - 1
                If Selected(r, c) Then
                    Grid.PutFloatWindow(r, r, c, c, Grid.Value(c, r) + myVal)
                End If
            Next
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)

    End Sub
    Public Sub ReplaceValues(ByVal myVal As Single)
        'vervangt waarden van de geselecteerde cellen in het Target grid door een opgegeven constante waarde
        Dim r As Integer, c As Integer

        Me.Setup.GeneralFunctions.UpdateProgressBar("Collecting values from the source grid...", 0, 10)
        For r = 0 To nRow - 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r + 1), nRow)
            For c = 0 To nCol - 1
                If Selected(r, c) Then
                    Grid.PutFloatWindow(r, r, c, c, myVal)
                End If
            Next
        Next
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub


    Public Sub GetValuesFromSourceGrid(ByRef SourceGrid As clsRaster)
        'Haalt waarden voor de geselecteerde cellen in het Target grid op uit een ander grid (source grid)
        'en voert ze in
        Dim r As Integer, c As Integer
        Dim r_start As Integer, c_start As Integer, r_end As Integer, c_end As Integer
        Dim X As Double, Y As Double, myVal As Single

        'bereken om te beginnen de start- en eindkolommen
        SourceGrid.Read(False)
        If Not GetRowColExtent(SourceGrid.XLLCorner, SourceGrid.XURCorner, SourceGrid.YLLCorner, SourceGrid.YURCorner, c_start, c_end, r_start, r_end) Then Throw New Exception("Error finding matching columns for source grid and target grid.")

        Me.Setup.GeneralFunctions.UpdateProgressBar("Collecting values from the source grid...", 0, 10)
        For r = r_start To r_end
            Me.Setup.GeneralFunctions.UpdateProgressBar("", (r - r_start - 1), (r_end - r_start))
            For c = c_start To c_end
                If Selected(r, c) Then
                    Call GetCellCenter(r, c, X, Y)                 'een geselecteerde cel gevonden. Zoek (X,Y) op van het centrum
                    If Not SourceGrid.GetCellValueFromXY(X, Y, myVal) Then Me.Setup.Log.AddWarning("No grid value found for co-ordinate " & X & "," & Y)
                    If Not myVal = SourceGrid.NoDataVal Then Grid.PutFloatWindow(r, r, c, c, myVal)
                End If
            Next
        Next

        SourceGrid.Close()
        Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub

    Public Function MaxElevationForShape(ByVal ID As String, ByVal OriginalValue As Double, ByRef myShape As MapWinGIS.Shape, ByVal StepSize As Integer) As Double
        'This routine walks through a shape and retrieves the maximum elevation data from the elevation grid
        Dim j As Long, myDist As Integer
        Dim myElevation As Double, myMax As Double = -999999999
        Dim segLength As Double, Found As Boolean = False
        Dim curPoint As MapWinGIS.Point, nextPoint As MapWinGIS.Point
        Dim StartDist As Integer, EndDist As Integer
        Dim X As Double, Y As Double

        For j = 0 To myShape.numPoints - 2
            curPoint = myShape.Point(j)
            nextPoint = myShape.Point(j + 1)
            StartDist = 0
            segLength = Math.Sqrt((nextPoint.x - curPoint.x) ^ 2 + (nextPoint.y - curPoint.y) ^ 2)
            EndDist = Me.Setup.GeneralFunctions.RoundUD(segLength, 0, False)

            For myDist = StartDist To EndDist
                X = Setup.GeneralFunctions.Interpolate(0, curPoint.x, segLength, nextPoint.x, myDist)
                Y = Setup.GeneralFunctions.Interpolate(0, curPoint.y, segLength, nextPoint.y, myDist)
                If Not Me.Setup.GISData.ElevationGrid.GetCellValueFromXY(X, Y, myElevation) Then Me.Setup.Log.AddWarning("No elevation value found for co-ordinate " & X & "," & Y)
                If myElevation > myMax AndAlso Not myElevation = Me.Setup.GISData.ElevationGrid.Grid.Header.NodataValue Then
                    myMax = myElevation
                    Found = True
                End If
            Next
        Next

        If Not Found Then
            Me.Setup.Log.AddWarning("Surface level for cross section " & ID & " could not be retrieved from grid. Value was unchanged.")
            Return OriginalValue
        Else
            Return myElevation
        End If


    End Function

    Public Function PointInsideGrid(ByVal X As Double, ByVal Y As Double) As Boolean
        If X >= XLLCorner AndAlso X <= XLLCorner + dX * nCol AndAlso Y >= YLLCorner AndAlso Y <= YLLCorner + dY * nRow Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function PointInValidCell(ByVal X As Double, ByVal Y As Double) As Boolean
        Dim myValue As Single

        Try
            If Not GetCellValueFromXY(X, Y, myValue) Then
                Me.Setup.Log.AddError("No grid value found for co-ordinate " & X & "," & Y & ". Location probably outside of grid.")
                Return False
            ElseIf Not myValue = NoDataVal Then
                Return True
            Else
                Return True
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Function GetCellValueFromXY(ByVal x As Double, ByVal y As Double, ByRef myValue As Single) As Boolean
        Dim r As Integer, c As Integer
        Try
            If Not GetRCFromXY(x, y, r, c) Then
                Me.Setup.Log.AddError("Error getting grid row and column number for co-ordinate " & x & "," & y & ". Location probably outside grid.")
                Return False
            End If
            myValue = Grid.Value(c, r)
            Return True
        Catch ex As Exception
            myValue = 0
            Return False
        End Try
    End Function

    Public Function GetRCFromXY(ByVal X As Double, ByVal Y As Double, ByRef rowIdx As Integer, ByRef colIdx As Integer) As Boolean

        If X >= XLLCorner AndAlso X <= XURCorner AndAlso Y >= YLLCorner AndAlso Y <= YURCorner Then
            ' MapWindow telt rijen van boven naar beneden (gechecked!)
            colIdx = Setup.GeneralFunctions.RoundUD((X - XLLCorner) / dX, 0, False)
            rowIdx = Setup.GeneralFunctions.RoundUD((YURCorner - Y) / dY, 0, False)
            Return True
        ElseIf X < XLLCorner Then
            colIdx = Math.Min(Math.Max(0, Setup.GeneralFunctions.RoundUD((X - XLLCorner) / dX, 0, False)), Grid.Header.NumberCols - 1)
            rowIdx = Math.Min(Math.Max(0, Setup.GeneralFunctions.RoundUD((YURCorner - Y) / dY, 0, False)), Grid.Header.NumberRows - 1)
            Return False
        End If

    End Function

    Public Function getPointFromRC(ByVal r As Long, ByVal c As Long, ByRef myPoint As MapWinGIS.Point) As Boolean
        'note: the row and column numbers are zero-based
        If r >= 0 AndAlso r <= (nRow - 1) AndAlso c >= 0 AndAlso c <= (nCol - 1) Then
            myPoint.x = XLLCorner + (c + 0.5) * dX
            myPoint.y = YURCorner - (r + 0.5) * dY
            Return True
        Else
            Return False
        End If
    End Function

    Public Function getXYCenterFromRC(ByVal r As Long, ByVal c As Long, ByRef X As Double, ByRef Y As Double) As Boolean
        'note: the row and column numbers are zero-based
        If r >= 0 AndAlso r <= (nRow - 1) AndAlso c >= 0 AndAlso c <= (nCol - 1) Then
            X = XLLCorner + (c + 0.5) * dX
            Y = YURCorner - (r + 0.5) * dY
            Return True
        Else
            Return False
        End If
    End Function

    Public Function getXYLowerLeftFromRC(ByVal r As Long, ByVal c As Long, ByRef X As Double, ByRef Y As Double) As Boolean
        'note: the row and column numbers are zero-based
        If r >= 0 AndAlso r <= (nRow - 1) AndAlso c >= 0 AndAlso c <= (nCol - 1) Then
            X = XLLCorner + (c) * dX
            Y = YURCorner - (r + 1) * dY
            Return True
        Else
            Return False
        End If
    End Function

    Public Function getXYUpperRightFromRC(ByVal r As Long, ByVal c As Long, ByRef X As Double, ByRef Y As Double) As Boolean
        'note: the row and column numbers are zero-based
        If r >= 0 AndAlso r <= (nRow - 1) AndAlso c >= 0 AndAlso c <= (nCol - 1) Then
            X = XLLCorner + (c + 1) * dX
            Y = YURCorner - (r) * dY
            Return True
        Else
            Return False
        End If
    End Function


    Public Function GetRowColExtent(ByVal xMin As Double, ByVal xMax As Double, ByVal yMin As Double, ByVal yMax As Double,
                                    ByRef startCol As Integer, ByRef endCol As Integer, ByRef startRow As Integer, ByRef endRow As Integer) As Boolean

        ' Paul Meems, 5 June 2012: Added:
        If Grid Is Nothing Then Throw New Exception("The grid object is not set")

        'zoek het kolomnummer bij xMin
        If xMin > XURCorner OrElse xMax < XLLCorner OrElse yMin > YURCorner OrElse yMax < YLLCorner Then
            Me.Setup.Log.AddDebugMessage("Shape valt in zijn geheel buiten het grid")
            Return False
        End If

        'let op: MapWindow telt rijen van boven naar beneden!!!!!
        startCol = Math.Max(0, Me.Setup.GeneralFunctions.RoundUD((xMin - XLLCorner) / dX, 0, False))
        endCol = Math.Min(nCol - 1, Me.Setup.GeneralFunctions.RoundUD((xMax - XLLCorner) / dX, 0, False) - 1)
        startRow = Math.Max(0, Me.Setup.GeneralFunctions.RoundUD((YURCorner - yMax) / dY, 0, False))
        endRow = Math.Min(nRow - 1, Me.Setup.GeneralFunctions.RoundUD((YURCorner - yMin) / dY, 0, False) - 1)

        Return True
    End Function

    Public Function GetCellCenter(ByVal rowIdx As Integer, ByVal colIdx As Integer, ByRef X As Double, ByRef Y As Double) As Boolean

        ' MapWindow telt rijen van boven naar beneden (gechecked!)
        X = XLLCenter + dX * colIdx
        Y = YLLCenter + ((nRow - 1) * dY) - (dY * rowIdx)
        Return True

    End Function

    Public Function GetLowestValue() As Double
        Dim r As Long, c As Long
        Dim minVal As Double = 9000000000.0

        For r = 0 To nRow - 1
            For c = 0 To nCol - 1
                If Grid.Value(c, r) <> Grid.Header.NodataValue AndAlso Grid.Value(c, r) < minVal Then
                    minVal = Grid.Value(c, r)
                End If
            Next
        Next
        Return minVal
    End Function

    Public Function CompleteMetaHeaderWithoutReading() As Boolean
        'completes the meta-header without actually reading the grid
        'this speeds up the reading process but BE CAREFUL! If the grid changes
        'during a process, the metadata is not automatically updated
        'Author: Siebe Bosch
        'Date: 7-2-2014
        Try

            If XLLCorner = 0 AndAlso dX > 0 AndAlso XLLCenter <> 0 Then
                XLLCorner = XLLCenter - dX / 2
                If nCol > 0 Then XURCorner = XLLCorner + nCol * dX
            ElseIf XURCorner = 0 AndAlso XLLCorner <> 0 AndAlso dX <> 0 AndAlso nCol > 0 Then
                XURCorner = XLLCorner + nCol * dX
            End If

            If YLLCorner = 0 AndAlso dY > 0 AndAlso YLLCenter <> 0 Then
                YLLCorner = YLLCenter - dY / 2
                If nRow > 0 Then YURCorner = YLLCorner + nRow * dY
            ElseIf YURCorner = 0 AndAlso YLLCorner <> 0 AndAlso dY <> 0 AndAlso nCol > 0 Then
                YURCorner = YLLCorner + nRow * dY
            End If

            If dX > 0 AndAlso dY > 0 Then
                CellArea = dX * dY
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function CompleteMetaHeader() As Boolean
        'completes the meta-header using data from the actual header
        'Author: Siebe Bosch
        'Date: 7-2-2014

        Try
            If Not Grid.Header Is Nothing Then
                MetaDataFromHeader()
            Else
                If Not Grid.Open(Path) Then Throw New Exception("Could not read " & Path)
                MetaDataFromHeader()
                Grid.Close()
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Sub MetaDataFromHeader()

        XLLCenter = Grid.Header.XllCenter
        YLLCenter = Grid.Header.YllCenter
        nRow = Grid.Header.NumberRows
        nCol = Grid.Header.NumberCols
        dX = Grid.Header.dX
        dY = Grid.Header.dY
        NoDataVal = Grid.Header.NodataValue

        'afgeleide data
        XLLCorner = XLLCenter - dX / 2
        YLLCorner = YLLCenter - dY / 2
        YURCorner = YLLCorner + dY * nRow
        XURCorner = XLLCorner + dX * nCol
        CellArea = dX * dY

    End Sub

    Public Function BuildSCurve(ByVal nDecimals As Integer) As Integer()

        Dim myArray(100) As Integer, r As Long, c As Long, myVal As Double
        Dim i As Long, j As Integer, n As Long
        Dim myDict As New Dictionary(Of Long, Integer)
        Dim sortedDict As New Dictionary(Of Long, Integer)

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building S-curve from grid.", 0, 10)
            i = 0
            For r = 0 To Grid.Header.NumberRows - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", r + 1, Grid.Header.NumberRows)
                For c = 0 To Grid.Header.NumberCols - 1
                    myVal = Grid.Value(c, r)
                    If Not myVal = Grid.Header.NodataValue Then
                        i += 1
                        myDict.Add(i, myVal * 10 ^ nDecimals)
                    End If
                Next
            Next
            n = myDict.Count

            'sorteer naar oplopende rasterwaarde
            sortedDict = (From entry In myDict Order By entry.Value Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)

            myArray(0) = sortedDict.Values(0)
            For i = 1 To 99
                j = i / 100 * n
                myArray(i) = sortedDict.Values(j)
            Next
            myArray(100) = sortedDict.Values(n - 1)
            Return myArray

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        Finally
            myDict.Clear()
            sortedDict.Clear()
            myDict = Nothing
            sortedDict = Nothing
            GC.Collect()
        End Try


    End Function


    Public Function getLowestValueFromCircularBuffer(ByVal r As Long, ByVal c As Long, ByVal Radius As Double, ByVal ValueIfNotFound As Double, Optional ByVal IncludeZeroVals As Boolean = True) As Double

        'returns the lowest value from grid inside a given buffer radius
        Dim cellRadius As Integer = Radius / dX
        Dim curCellRadius As Integer
        Dim X As Double, Y As Double
        Dim minVal As Double = 99999999
        Dim myVal As Double
        Dim Found As Boolean = False
        Dim CircleCells As New Dictionary(Of String, clsRC)
        Dim FilledCircleCells As New Dictionary(Of String, clsRC)
        Dim myRC As clsRC


        Try

            'note: this routine contains a work-around for an error I haven't yet solved. 
            'it's the fact that some grids return -3.689349E+19 for a nodata-cell (SINGLE) whereas nodata-value =  -3.4028234663852886E+38 (double)

            'first try to find results inside the given (filled) circle
            'in order to do so it first collects all cells that lie inside the filled circle
            If getXYCenterFromRC(r, c, X, Y) Then                   'only process if the current row and column numbers are valid

                'walk through all circles, stepping one grid cell size at a time and collect all cells inside the circle
                For curCellRadius = 0 To cellRadius
                    CircleCells = CollectCellsFromCircle(c, r, curCellRadius)
                    For Each myKey In CircleCells.Keys
                        If Not FilledCircleCells.ContainsKey(myKey) Then FilledCircleCells.Add(myKey, CircleCells.Item(myKey))
                    Next
                Next

                For Each myRC In FilledCircleCells.Values
                    myVal = Me.Grid.Value(myRC.C, myRC.R)
                    If (myVal = 0 AndAlso IncludeZeroVals) OrElse (myVal <> NoDataVal AndAlso myVal > -3.0E+19) Then 'contains the workaround mentioned in the introduction
                        If myVal < minVal Then
                            minVal = myVal
                            Found = True
                        End If
                    End If
                Next

                If Found Then
                    Return minVal
                Else
                    Return ValueIfNotFound
                End If
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getLowestValueFromCircularBuffer")
        End Try

    End Function

    Public Function getNearestLowestValue(ByVal r As Long, ByVal c As Long, Optional ByVal IncludeZeroVals As Boolean = True) As Double

        '---------------------------------------------------------------------------------------
        'date: 17 april 2014
        'author: Siebe Bosch
        'continuously increases a circle radius and returns the lowest value from the first ring
        'that actually contains a valid value
        '---------------------------------------------------------------------------------------

        Dim curCellRadius As Integer
        Dim X As Double, Y As Double
        Dim minVal As Double = 99999999
        Dim myVal As Double
        Dim Found As Boolean = False
        Dim CircleCells As New Dictionary(Of String, clsRC)
        Dim myRC As clsRC

        Try

            'note: this routine contains a work-around for an error I haven't yet solved. 
            'it's the fact that some grids return -3.689349E+19 for a nodata-cell (SINGLE) whereas nodata-value =  -3.4028234663852886E+38 (double)

            If getXYCenterFromRC(r, c, X, Y) Then                   'only process if the current row and column numbers are valid

                'walk through all circles, increasing the radius one cell size at a time and collect all cells inside the circle
                For curCellRadius = 0 To Math.Max(Me.Grid.Header.NumberCols, Me.Grid.Header.NumberRows)
                    CircleCells = CollectCellsFromCircle(c, r, curCellRadius)

                    'as soon as a valid value is found given the current radius, return the lowest value from the circle!
                    minVal = 9999999999
                    For Each myRC In CircleCells.Values
                        myVal = Me.Grid.Value(myRC.C, myRC.R)
                        If (myVal = 0 AndAlso IncludeZeroVals) OrElse (myVal <> NoDataVal AndAlso myVal > -3.0E+19) Then 'contains the workaround mentioned in the introduction
                            If myVal < minVal Then
                                minVal = myVal
                                Found = True
                            End If
                        End If
                    Next

                    'if a value was found in the current circle radius, we're done!
                    If Found Then Return minVal

                Next
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getLowestValueFromCircularBuffer")
        End Try

    End Function

    Public Function Read(Optional ByVal InRAM As Boolean = True) As Boolean

        ' Paul Meems, 5 June 2012: Made several changes
        'If Not Me.Grid Is Nothing Then
        '  If Not String.IsNullOrEmpty(Me.Grid.Filename) Then
        '    Me.setup.Log.AddWarning("Elevation grid is already open!")
        '    Return False
        '  End If
        'End If
        Try
            If Not Grid.Open(Path, GridDataType.UnknownDataType, InRAM) Then
                Dim log As String = "Failed to open gridfile: " + Grid.ErrorMsg(Grid.LastErrorCode)
                Me.Setup.Log.AddError(log)
                Throw New Exception(log)
            Else
                Call CompleteMetaHeader()
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error reading grid " & Path)
            Return False
        End Try

    End Function

    Public Function GetNearestCell(ByVal r As Integer, ByVal c As Integer, ByRef Value As Double, ByRef Distance As Double, ByVal SkipZeroVals As Boolean) As Boolean
        'This function retrieves the nearest by valid grid cell. It does so by creating an increasing circle around the cell and
        'returning the fist valid cell value and its distance
        Dim CellRadius As Integer = 0
        Dim CircleCells As New Dictionary(Of String, clsRC)
        Dim myRC As clsRC, myVal As Double

        Do
            'create a collection of cells that overlap the circle around our center point
            CircleCells = CollectCellsFromCircle(c, r, CellRadius)

            If SkipZeroVals Then
                For Each myRC In CircleCells.Values
                    myVal = Grid.Value(myRC.C, myRC.R)
                    If myVal <> NoDataVal AndAlso myVal <> 0 AndAlso myVal > -3.0E+19 Then 'last one is to avoid nodata values of single type where nodat val is double
                        Distance = Me.Setup.GeneralFunctions.Pythagoras(c, r, myRC.C, myRC.R) * dX    'distance (m) between the point found and the original cell
                        Value = myVal
                        Return True
                    End If
                Next
            Else
                For Each myRC In CircleCells.Values
                    myVal = Grid.Value(myRC.C, myRC.R)
                    If myVal <> NoDataVal AndAlso myVal > -3.0E+19 Then                    'last one is to avoid nodata values of single type where nodat val is double
                        Distance = Me.Setup.GeneralFunctions.Pythagoras(c, r, myRC.C, myRC.R) * dX    'distance (m) between the point found and the original cell
                        Value = myVal
                        Return True
                    End If
                Next
            End If

            'safety valve
            CellRadius += 1
            If CellRadius > nRow Then Return False

        Loop
        Return False

    End Function

    Public Function GetNearestEdgeCell(ByVal StartR As Integer, ByVal StartC As Integer, ByRef EdgeR As Integer, ByRef EdgeC As Integer, ByVal MaxRadius As Double, ByRef Distance As Double) As Boolean
        'This function retrieves the nearest by valid grid cell. It does so by creating an increasing circle around the cell and
        'returning the fist valid cell value and its distance
        Dim CellRadius As Integer = 0
        Dim CircleCells As New Dictionary(Of String, clsRC)
        Dim myRC As clsRC


        Do
            'create a collection of cells that overlap the circle around our center point
            CircleCells = CollectCellsFromCircle(StartC, StartR, CellRadius)

            For Each myRC In CircleCells.Values
                If IsEdgeCell(myRC.R, myRC.C) Then
                    EdgeR = myRC.R
                    EdgeC = myRC.C
                    Distance = Me.Setup.GeneralFunctions.Pythagoras(StartC, StartR, myRC.C, myRC.R) * dX    'distance (m) between the point found and the original cell
                    Return True
                End If
            Next

            'safety valve
            CellRadius += 1
            If CellRadius > nRow Then Return False
            If CellRadius * Grid.Header.dX > MaxRadius Then Return False
        Loop
        Return False

    End Function

    Public Function IsEdgeCell(r As Integer, c As Integer, Optional IncludeDiagonalCells As Boolean = True) As Boolean
        If Grid.Value(c, r) = Grid.Header.NodataValue Then Return False
        If r = 0 OrElse c = 0 OrElse r = Grid.Header.NumberRows - 1 OrElse c = Grid.Header.NumberCols - 1 Then Return True
        If Grid.Value(c, r - 1) = Grid.Header.NodataValue Then Return True
        If Grid.Value(c, r + 1) = Grid.Header.NodataValue Then Return True
        If Grid.Value(c - 1, r) = Grid.Header.NodataValue Then Return True
        If Grid.Value(c + 1, r) = Grid.Header.NodataValue Then Return True
        If IncludeDiagonalCells Then
            If Grid.Value(c - 1, r - 1) = Grid.Header.NodataValue Then Return True
            If Grid.Value(c - 1, r + 1) = Grid.Header.NodataValue Then Return True
            If Grid.Value(c + 1, r - 1) = Grid.Header.NodataValue Then Return True
            If Grid.Value(c + 1, r + 1) = Grid.Header.NodataValue Then Return True
        End If
        Return False
    End Function


    Public Function CollectCellsFromCircle(ByVal c0 As Integer, ByVal r0 As Integer, ByVal radius As Integer) As Dictionary(Of String, clsRC)

        'Date: 1-3-2014
        'Author: Siebe Bosch
        'Description: midpoint circle algorithm
        'c#-code from wikipedia: http://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        'then converted to vb.net using http://www.developerfusion.com/tools/convert/csharp-to-vb/?batchId=dc6b40bd-ab00-42f9-bcee-a4c646033fd1
        'and added a collection of instances of the class clsXr to store the results in
        Dim c As Integer = radius, r As Integer = 0
        Dim radiusError As Integer = 1 - c
        Dim Points As New Dictionary(Of String, clsRC)
        Dim myKey As String

        While c >= r
            'note: row = y, col = x, to we've swapped the order in order to store results in clsRC
            myKey = Str(r + r0).Trim & "_" & Str(c + c0).Trim
            If Not Points.ContainsKey(myKey) AndAlso CellValid(r + r0, c + c0) Then Points.Add(myKey, New clsRC(r + r0, c + c0))
            myKey = Str(c + r0).Trim & "_" & Str(r + c0).Trim
            If Not Points.ContainsKey(myKey) AndAlso CellValid(c + r0, r + c0) Then Points.Add(myKey, New clsRC(c + r0, r + c0))
            myKey = Str(r + r0).Trim & "_" & Str(-c + c0).Trim
            If Not Points.ContainsKey(myKey) AndAlso CellValid(r + r0, -c + c0) Then Points.Add(myKey, New clsRC(r + r0, -c + c0))
            myKey = Str(c + r0).Trim & "_" & Str(-r + c0).Trim
            If Not Points.ContainsKey(myKey) AndAlso CellValid(c + r0, -r + c0) Then Points.Add(myKey, New clsRC(c + r0, -r + c0))
            myKey = Str(-r + r0).Trim & "_" & Str(-c + c0).Trim
            If Not Points.ContainsKey(myKey) AndAlso CellValid(-r + r0, -c + c0) Then Points.Add(myKey, New clsRC(-r + r0, -c + c0))
            myKey = Str(-c + r0).Trim & "_" & Str(-r + c0).Trim
            If Not Points.ContainsKey(myKey) AndAlso CellValid(-c + r0, -r + c0) Then Points.Add(myKey, New clsRC(-c + r0, -r + c0))
            myKey = Str(-r + r0).Trim & "_" & Str(c + c0).Trim
            If Not Points.ContainsKey(myKey) AndAlso CellValid(-r + r0, c + c0) Then Points.Add(myKey, New clsRC(-r + r0, c + c0))
            myKey = Str(-c + r0).Trim & "_" & Str(r + c0).Trim
            If Not Points.ContainsKey(myKey) AndAlso CellValid(-c + r0, r + c0) Then Points.Add(myKey, New clsRC(-c + r0, r + c0))

            r += 1
            If radiusError < 0 Then
                radiusError += 2 * r + 1
            Else
                c -= 1
                radiusError += 2 * (r - c + 1)
            End If
        End While

        Return Points
    End Function

    Public Function CellValid(ByRef r As Integer, ByRef c As Integer) As Boolean
        If r >= 0 AndAlso r <= nRow - 1 Then
            If c >= 0 AndAlso c <= nCol - 1 Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Function CollectCellsFromLine(ByVal c0 As Integer, ByVal r0 As Integer, ByVal c1 As Integer, ByVal r1 As Integer) As Dictionary(Of String, clsRC)
        'Date: 1-3-2014
        'Author: Siebe Bosch
        'Description: returns a collection of cells that follow a straight line between (c0,r0) and (c1 and r1)
        'it uses the Bresenham's line algorithm to do so
        Dim Points As New Dictionary(Of String, clsRC)
        Dim myKey As String, myRC As clsRC
        Dim dx As Integer, dy As Integer
        Dim sx As Integer, sy As Integer
        Dim err As Integer, e2 As Integer

        dx = Math.Abs(c1 - c0)
        dy = Math.Abs(r1 - r0)
        If c0 < c1 Then sx = 1 Else sx = -1
        If r0 < r1 Then sy = 1 Else sy = -1
        err = dx - dy

        Do
            myRC = New clsRC(r0, c0)
            myKey = Str(r0).Trim & "_" & Str(c0).Trim
            If Not Points.ContainsKey(myKey) Then Points.Add(myKey, myRC)

            If c0 = c1 And r0 = r1 Then Exit Do
            e2 = 2 * err
            If e2 > -dy Then
                err = err - dy
                c0 = c0 + sx
            End If
            If c0 = c1 And r0 = r1 Then
                myRC = New clsRC(r0, c0)
                myKey = Str(r0).Trim & "_" & Str(c0).Trim
                If Not Points.ContainsKey(myKey) Then Points.Add(myKey, myRC)
                Exit Do
            End If
            If e2 < dx Then
                err = err + dx
                r0 = r0 + sy
            End If
        Loop

        Return Points
    End Function

End Class
