Option Explicit On

Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO
Imports System.Windows.Forms
Imports MapWinGIS

Public Class clsGISData
    Implements MapWinGIS.ICallback

    Public DTMDataSource As clsGeoDatasource

    'generic geodata placeholders
    Public SoilDataSource As clsGeoDatasource
    Public LanduseDataSource As clsGeoDatasource
    Public SewageAreaDataSource As clsGeoDatasource

    'GeoDatasource voor gebieden:
    Public CatchmentDataSource As clsGeoDatasource
    Public SubcatchmentDataSource As clsGeoDatasource

    'Shapefiles voor gebieden:
    Public ChannelAreasShapeFile As ClsPolyShapeFile                'a shapefile that covers the surface area of the EXISTING channels in the 1D-schematisation (backbone)
    Public SoilShapefile As clsBodemShapeFile
    Public LanduseShapeFile As clsLGNShapeFile
    Public DikeShapeFile As clsPolyLineShapeFile
    Public BAGShape As ClsPolyShapeFile
    Public PointShapeFile As clsPointShapeFile
    Public PolyLineShapeFile As clsPolyLineShapeFile

    'Public GridCollection As clsGridCollection
    Public SnappingPointsShapeFile As clsPointShapeFile     'a shapefile to log all snapping locations while building a model

    Public ChannelDataSource As clsGeoDatasource
    Public BoundaryDataSource As clsGeoDatasource
    Public WeirDataSource As clsGeoDatasource
    Public OutletpumpDataSource As clsGeoDatasource
    Public CulvertDataSource As clsGeoDatasource
    Public AbutmentBridgeDataSource As clsGeoDatasource
    Public PillarBridgeDataSource As clsGeoDatasource
    Public SiphonDataSource As clsGeoDatasource
    Public OrificeDataSource As clsGeoDatasource
    Public SluiceDataSource As clsGeoDatasource
    Public FixedDamsDataSource As clsGeoDatasource
    Public OpenFishPassDataSource As clsGeoDatasource
    Public InletPumpDataSource As clsGeoDatasource
    Public FlushPumpDataSource As clsGeoDatasource
    Public MobilePumpDataSource As clsGeoDatasource
    Public TrapeziumPolylineDataSource As clsGeoDatasource
    Public TrapeziumPointDataSource As clsGeoDatasource
    Public BathymetryPolygonsDatasource As clsGeoDatasource
    Public BathymetryDataSource As clsGeoDatasource
    Public PerpendicularShapeFile As clsPerpendicularShapeFile
    Public XYZProfileShapefiles As List(Of clsXYZProfileShapeFile)
    'Public TrapeziumPolylineShapeFile As clsTrapeziumProfileShapeFile
    Public TrapeziumPointShapeFile As clsTrapeziumProfileShapeFile


    'Shapefiles voor rioleringsgebieden, overstorten en AWZI's
    'Public SewageAreaShapeFile As clsSewageAreaShapeFile       'v1.890: replaced this instance by SewageAreaDataSource
    Public WWTPDischargePointsShapeFile As clsWWTPDischargePointsShapefile

    'Rasterbestanden
    Public ElevationGrid As clsRaster 'inlezen hoogteraster met MapWindow-ocx
    Public LandUseGrid As clsRaster 'inlezen met Mapwindow Ocx
    Public SoilGrid As clsRaster    'inlezen met de Mapwindow ocx
    Public SeepageGridSummer As clsRaster
    Public SeepageGridWinter As clsRaster
    Public HoogteRasterASCII As clsASCIIGrid 'rechtstreeks inlezen hoogteraster uit ASCII-grid (sneller?)

    'Collections en Dictionaries
    Public Catchments As clsCatchments
    Friend ChannelCategories As Dictionary(Of String, clsChannelCategory)
    Friend ChannelUsageCategories As Dictionary(Of String, clsChannelCategory)

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup

        DTMDataSource = New clsGeoDatasource(Me.setup, False)

        SoilDataSource = New clsGeoDatasource(Me.setup, False)
        LanduseDataSource = New clsGeoDatasource(Me.setup, False)
        'CatchmentsDataSource = New clsGeoDatasource(Me.setup)
        'SubCatchmentsDataSource = New clsGeoDatasource(Me.setup)
        SewageAreaDataSource = New clsGeoDatasource(Me.setup, False)

        'GeoDatasource voor gebieden
        CatchmentDataSource = New clsGeoDatasource(Me.setup, True)
        SubcatchmentDataSource = New clsGeoDatasource(Me.setup, True)

        'Shapefiles voor gebieden:
        'CatchmentShapeFile = New clsSubcatchmentDataSource(Me.setup)             'hoogste orde gebieden (stroomgebieden)
        'SubcatchmentDataSource = New clsSubcatchmentDataSource(Me.setup)              'middelste orde gebieden (peilgebieden)
        ChannelAreasShapeFile = New ClsPolyShapeFile(Me.setup)
        SoilShapefile = New clsBodemShapeFile(Me.setup)
        LanduseShapeFile = New clsLGNShapeFile(Me.setup)
        BAGShape = New ClsPolyShapeFile(Me.setup)

        'Shapefiles voor watergangen, kunstwerken, loodlijnen en kades:
        ChannelDataSource = New clsGeoDatasource(Me.setup, False)
        BoundaryDataSource = New clsGeoDatasource(Me.setup, False)
        WeirDataSource = New clsGeoDatasource(Me.setup, False)
        CulvertDataSource = New clsGeoDatasource(Me.setup, False)
        AbutmentBridgeDataSource = New clsGeoDatasource(Me.setup, False)
        PillarBridgeDataSource = New clsGeoDatasource(Me.setup, False)
        SluiceDataSource = New clsGeoDatasource(Me.setup, False)
        FixedDamsDataSource = New clsGeoDatasource(Me.setup, False)
        SiphonDataSource = New clsGeoDatasource(Me.setup, False)
        OrificeDataSource = New clsGeoDatasource(Me.setup, False)
        OutletpumpDataSource = New clsGeoDatasource(Me.setup, False)
        InletPumpDataSource = New clsGeoDatasource(Me.setup, False)
        FlushPumpDataSource = New clsGeoDatasource(Me.setup, False)
        MobilePumpDataSource = New clsGeoDatasource(Me.setup, False)
        TrapeziumPolylineDataSource = New clsGeoDatasource(Me.setup, False)
        TrapeziumPointDataSource = New clsGeoDatasource(Me.setup, False)
        BathymetryPolygonsDatasource = New clsGeoDatasource(Me.setup, False)
        BathymetryDataSource = New clsGeoDatasource(Me.setup, False)

        PerpendicularShapeFile = New clsPerpendicularShapeFile(Me.setup)

        'create a list of xyz profiles shapefiles and populate it with two instances
        XYZProfileShapefiles = New List(Of clsXYZProfileShapeFile)
        XYZProfileShapefiles.Add(New clsXYZProfileShapeFile(Me.setup))
        XYZProfileShapefiles.Add(New clsXYZProfileShapeFile(Me.setup))

        'TrapeziumPolylineShapeFile = New clsTrapeziumProfileShapeFile(Me.setup)
        TrapeziumPointShapeFile = New clsTrapeziumProfileShapeFile(Me.setup)
        SnappingPointsShapeFile = New clsPointShapeFile(Me.setup)

        'Shapefiles voor rioolgebieden, overstortlocaties (CSO's) en AWZI's
        'SewageAreaShapeFile = New clsSewageAreaShapeFile(Me.setup, Nothing, Me.setup.progressBar, Me.setup.progressLabel)
        WWTPDischargePointsShapeFile = New clsWWTPDischargePointsShapefile(Me.setup)

        'Rasterbestanden
        ElevationGrid = New clsRaster(Me.setup)                            'inlezen hoogteraster met MapWindow-ocx
        LandUseGrid = New clsRaster(Me.setup)
        SoilGrid = New clsRaster(Me.setup)
        SeepageGridSummer = New clsRaster(Me.setup)                           'raster met kwelgegevens
        SeepageGridWinter = New clsRaster(Me.setup)                           'raster met kwelgegevens
        HoogteRasterASCII = New clsASCIIGrid(Me.setup)                    'rechtstreeks inlezen hoogteraster uit ASCII-grid (sneller?)

        'Collections en Dictionaries
        Catchments = New clsCatchments(Me.setup)                                        'Stroomgebieden
        ChannelCategories = New Dictionary(Of String, clsChannelCategory)               'categorieen watergangen (bijv primair, secunair en tertiair)
        ChannelUsageCategories = New Dictionary(Of String, clsChannelCategory)          'categorieën watergangen qua gebruik
    End Sub

    Public Function InitializeChannelShapefile(ByVal Path As String) As clsShapeFile
        Try
            Dim ChannelShapeFile = New clsShapeFile(Me.setup)
            ChannelShapeFile.CreateNew(Path)
            Return ChannelShapeFile
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function ReadCatchmentsFromDataSource() As Boolean
        Try
            Dim Catchment As clsCatchment
            CatchmentDataSource.Open()
            Dim n As Integer = CatchmentDataSource.GetNumberOfRecords
            For i = 0 To n - 1
                Catchment = New clsCatchment(Me.setup)
                Catchment.ID = CatchmentDataSource.GetTextValue(i, enmInternalVariable.ID)
                Catchment.TotalShape = CatchmentDataSource.GetShape(i)
                If Catchment.ID IsNot Nothing AndAlso Not Catchments.Catchments.ContainsKey(Catchment.ID.Trim.ToUpper) Then
                    Catchments.Catchments.Add(Catchment.ID.Trim.ToUpper, Catchment)
                End If
            Next
            CatchmentDataSource.Close()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function ReadCatchmentsFromDataSource")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function




    Public Function JoinShapeDataByID(TargetFilePath As String, TargetFileIDField As String, SourceFilePath As String, SourceFileIDField As String) As Boolean
        'this function joins data between two shapefiles by their ID field. 
        'this is a perfect method to copy data from one shapefile to another when both shapefiles share one common field
        Try
            If Not System.IO.File.Exists(SourceFilePath) Then Throw New Exception("Error: source shapefile does not exist.")
            If Not System.IO.File.Exists(TargetFilePath) Then Throw New Exception("Error: target shapefile does not exist.")
            Dim TargetSF As New MapWinGIS.Shapefile
            Dim SourceSF As New MapWinGIS.Shapefile
            Dim TargetSFIDFieldIdx As Integer, SourceSFIDFieldIdx As Integer
            Dim TargetSFValueFieldIdx As Integer, SourceSFValueFieldIdx As Integer
            Dim rTarget As Integer, rSource As Integer

            TargetSF.Open(TargetFilePath)
            SourceSF.Open(SourceFilePath)

            TargetSF.StartEditingTable()
            TargetSFIDFieldIdx = Me.setup.GeneralFunctions.GetShapeFieldIdxByName(TargetSF, TargetFileIDField)
            SourceSFIDFieldIdx = Me.setup.GeneralFunctions.GetShapeFieldIdxByName(SourceSF, SourceFileIDField)

            If TargetSFIDFieldIdx < 0 Then Throw New Exception("Error: shapefield for ID of target shapefile not found: " & TargetFileIDField)
            If SourceSFIDFieldIdx < 0 Then Throw New Exception("Error: shapefield for ID of source shapefile not found: " & SourceFileIDField)

            For SourceSFValueFieldIdx = 0 To SourceSF.NumFields - 1
                TargetSFValueFieldIdx = Me.setup.GeneralFunctions.GetShapeFieldIdxByName(TargetSF, SourceSF.Field(SourceSFValueFieldIdx).Name)
                If TargetSFValueFieldIdx < 0 Then
                    'we will create a new field in the target shapefile and clone it from the source field
                    TargetSFValueFieldIdx = TargetSF.EditAddField(SourceSF.Field(SourceSFValueFieldIdx).Name, SourceSF.Field(SourceSFValueFieldIdx).Type, SourceSF.Field(SourceSFValueFieldIdx).Precision, SourceSF.Field(SourceSFValueFieldIdx).Width)
                End If

                'now that we have a valid shapefield, we can start copying the data
                For rTarget = 0 To TargetSF.NumShapes - 1
                    For rSource = 0 To SourceSF.NumShapes - 1
                        If SourceSF.CellValue(SourceSFIDFieldIdx, rSource) = TargetSF.CellValue(TargetSFIDFieldIdx, rTarget) Then
                            TargetSF.EditCellValue(TargetSFValueFieldIdx, rTarget, SourceSF.CellValue(SourceSFValueFieldIdx, rSource))
                            Exit For
                        End If
                    Next
                Next
            Next
            TargetSF.StopEditingTable(True)
            Me.setup.GeneralFunctions.UpdateProgressBar("Data successfully copied to " & Me.setup.GeneralFunctions.FileNameFromPath(TargetFilePath), 0, 10, True)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function JoinShapeDataByID in class clsGISData.")
            Return False
        End Try

    End Function

    Public Function GridsIdenticalExtentAndResolution(ByRef Header1 As MapWinGIS.GridHeader, ByRef Header2 As MapWinGIS.GridHeader) As Boolean
        If Header1.dX <> Header2.dX Then Return False
        If Header1.dY <> Header2.dY Then Return False
        If Header1.XllCenter <> Header2.XllCenter Then Return False
        If Header1.YllCenter <> Header2.YllCenter Then Return False
        If Header1.NumberCols <> Header2.NumberCols Then Return False
        If Header1.NumberRows <> Header2.NumberRows Then Return False
        Return True
    End Function

    Public Function CalculateDifferenceGrid(ByRef myGrid As MapWinGIS.Grid, ByRef refGrid As MapWinGIS.Grid, ResultsPath As String) As Boolean
        Try
            Dim diffGrid As New MapWinGIS.Grid
            If Not GridsIdenticalExtentAndResolution(myGrid.Header, refGrid.Header) Then Throw New Exception("Error calculating difference between grid. Extent or cellsizes to not match")
            If Not diffGrid.CreateNew(ResultsPath, myGrid.Header, myGrid.DataType, 0, True, GridFileType.GeoTiff) Then Throw New Exception("Error creating difference grid.")
            For r = 0 To myGrid.Header.NumberRows - 1
                For c = 0 To myGrid.Header.NumberRows - 1
                    If myGrid.Value(c, r) = myGrid.Header.NodataValue OrElse refGrid.Value(c, r) = refGrid.Header.NodataValue Then
                        diffGrid.Value(c, r) = diffGrid.Header.NodataValue
                    Else
                        diffGrid.Value(c, r) = myGrid.Value(c, r) - refGrid.Value(c, r)
                    End If
                Next
            Next
            diffGrid.Save()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function WatershedDelineation(GridPath As String, ByCoordinate As Boolean, SinkX As Double, SinkY As Double, ResultsGridPath As String) As Boolean
        Try
            Dim r As Double, c As Double
            Dim Stack As New Dictionary(Of Tuple(Of Integer, Integer), Boolean)    'dictionary key is a tuple consisting of row, column. Value = "have all surrounded cells been investigated?"

            ElevationGrid.Path = GridPath
            ElevationGrid.Read(True)

            Dim WatershedGrid As New clsRaster(Me.setup, ResultsGridPath)
            WatershedGrid.Grid.CreateNew(ResultsGridPath, ElevationGrid.Grid.Header, GridDataType.ShortDataType, 0, True, GridFileType.GeoTiff)
            WatershedGrid.Grid.Open(ResultsGridPath)

            'NOTE: first step should be PIT REMOVAL

            If ByCoordinate Then
                'now we will create a stack of cells. For each cell we will look around and only add the ones that have equal or higher elevation than the current one
                If Not ElevationGrid.GetRCFromXY(SinkX, SinkY, r, c) Then Throw New Exception("Error retrieving grid cell from co-ordinate.")
            Else
                r = SinkY
                c = SinkX
            End If

            'initalize by adding the sink point's underlying cell
            Stack.Add(New Tuple(Of Integer, Integer)(r, c), False)

            Dim Done As Boolean = False
            Dim Iter As Integer = 0
            Dim iCell As Integer = 0
            Dim nCells As Integer
            While Not Done
                Done = True 'initialize done as true. We will set it back to false as soon as cells are added to our stack
                Iter += 1
                iCell = 0
                nCells = Stack.Count
                Me.setup.GeneralFunctions.UpdateProgressBar("Iteration " & Iter, 0, 10, True)

                'walk through each cell of our stack 
                For Each myKey As Tuple(Of Integer, Integer) In Stack.Keys
                    iCell += 1
                    Me.setup.GeneralFunctions.UpdateProgressBar("", iCell, nCells)

                    r = myKey.Item1
                    c = myKey.Item2
                    If Stack.Item(myKey) = False AndAlso ElevationGrid.Grid.Value(c, r) <> ElevationGrid.Grid.Header.NodataValue Then 'we use the code 9 to indicate that this cell is complete and does not need any more processing
                        'reset and investigate all 8 cells surrounding our current cell
                        Dim StepsDone As Integer = 0
                        Dim r1 As Integer, c1 As Integer
                        While Me.setup.GeneralFunctions.NextSurroundingCell(StepsDone, r, c, r1, c1)
                            If c1 < 0 OrElse r1 < 0 Then
                                'neigboring cell does not exist
                            ElseIf c1 >= ElevationGrid.Grid.Header.NumberCols OrElse r1 >= ElevationGrid.Grid.Header.NumberRows Then
                                'neigboring cell does not exist
                            ElseIf Stack.ContainsKey(New Tuple(Of Integer, Integer)(r1, c1)) Then
                                'the neigboring cell is already part of our stack! No need to add it again
                            ElseIf ElevationGrid.Grid.Value(c1, r1) = ElevationGrid.Grid.Header.NodataValue Then
                                'neigboring cell has nodata-value
                            ElseIf ElevationGrid.Grid.Value(c1, r1) >= ElevationGrid.Grid.Value(c, r) Then
                                'neigboring cell has higher or equal value to our current cell. Add it to our stack! We will investigate it in the next iteration
                                Stack.Add(New Tuple(Of Integer, Integer)(r1, c1), 0)
                                WatershedGrid.Grid.Value(c1, r1) = 1
                                Done = False 'we just added a new cell to our stack, so we're not done yet!
                            End If
                        End While
                        'all surrounding cells have been investigated. Set your value to true
                        Stack.Item(myKey) = True
                    End If

                    Stack.Item(myKey) = 0
                    If r > 0 Then
                        'we can investicate the row above
                        If c > 0 Then
                            'we can investigate the column before
                            If ElevationGrid.Grid.Value(c, r) >= ElevationGrid.Grid.Value(c, r) Then
                                Stack.Item(myKey) = 0
                            End If

                        End If
                    End If
                Next
            End While
            Me.setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

            ElevationGrid.Close()
            WatershedGrid.Save()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetTargetLevelsFromShapefile(X As Double, Y As Double, ByRef TargetLevels As clsTargetLevels, ByRef ShapeIdx As Integer) As Boolean
        Try
            'find the target levels
            With setup.GISData.SubcatchmentDataSource.Shapefile
                ShapeIdx = .GetShapeIdxByCoord(X, Y)
                If ShapeIdx < 0 Then
                    Me.setup.Log.AddWarning("No underlying shape found in area shapefile for co-ordinate " & X & "," & Y & ". Target levels not found.")
                    Return False
                End If
                TargetLevels.setShapeID(setup.GISData.SubcatchmentDataSource.GetTextValue(ShapeIdx, enmInternalVariable.ID))
                If Not setup.GISData.SubcatchmentDataSource.getTargetLevels(ShapeIdx, TargetLevels) Then Throw New Exception("Error retrieving target levels for subcatchment " & TargetLevels.GetID)
            End With
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in Function GetTargetLevelsFromShapefile of class clsGISData.")
            Return False
        End Try
    End Function

    Public Function ShapeFileTypeIsPolygon(Path As String) As Boolean
        Try
            Dim myType As MapWinGIS.ShpfileType
            If getShapeFileType(Path, myType) Then
                Return (myType = ShpfileType.SHP_POLYGON)
            End If
            Return False
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in Function ShapeFileTypeIsPolygon")
            Return False
        End Try
    End Function
    Public Function AggegatePolylineLengthByPolygon(PolyLineSFPath As String, PolygonSFPath As String, FieldName As String) As Boolean
        Try
            'this function calculates the total length of polylines from a apolyline shapefile inside each polygon of a polygon shapefile
            'it then writes the results to the polygon shapefile
            Dim LineSF As New clsPolyLineShapeFile(Me.setup)
            Dim PolySF As New ClsPolyShapeFile(Me.setup)
            Dim TotalLength As Double = 0
            Dim i As Integer, j As Integer

            'read the lineshapefile and the polygon shapefile and start editing its table
            LineSF.SF.sf.Open(PolyLineSFPath)
            PolySF.sf.Open(PolygonSFPath)
            PolySF.sf.StartEditingTable()

            'retrieve or create the line length field
            Dim LenFieldIdx As Integer = PolySF.sf.FieldIndexByName(FieldName)
            If LenFieldIdx < 0 Then LenFieldIdx = PolySF.sf.EditAddField(FieldName, FieldType.DOUBLE_FIELD, 2, 10)

            'walk through each polygon and clip the polyline shapefile by the polygon's extents
            'then compute the total polyline length inside the polygon and add the result to the attribute table of the polygon shapefile
            Me.setup.GeneralFunctions.UpdateProgressBar("Computing polyline shape length...", 0, 10, True)
            For i = 0 To PolySF.sf.NumShapes - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, PolySF.sf.NumShapes)
                PolySF.sf.ShapeSelected(i) = True
                Dim ClippedSF As MapWinGIS.Shapefile = LineSF.SF.sf.Clip(False, PolySF.sf, True)
                If Not ClippedSF Is Nothing Then
                    TotalLength = 0
                    For j = 0 To ClippedSF.NumShapes - 1
                        TotalLength += ClippedSF.Shape(j).Length
                    Next
                    PolySF.sf.EditCellValue(LenFieldIdx, i, TotalLength)
                End If
                PolySF.sf.ShapeSelected(i) = False
            Next
            Me.setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

            'finalize editing the attribute table and close both shapefiles
            PolySF.sf.StopEditingTable(True)
            LineSF.Close()
            PolySF.Close()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function AggregatePolylinelengthByPolygon of class clsGISData.")
            Return False
        End Try

    End Function


    Public Function BuildStorageTablesToColumnFormat(ShapeFilePath As String, IDField As String, GridPath As String) As Boolean
        Try
            Dim SF As New MapWinGIS.Shapefile, Shape As MapWinGIS.Shape
            Dim Grid As New MapWinGIS.Grid
            Dim tmpGrid As New MapWinGIS.Grid
            Dim IDFieldIdx As Integer
            Dim tmpGridPath

            setup.ExcelFile.Path = Me.setup.Settings.ExportDirRoot & "\StorageTables.xlsx"
            Dim wsA As clsExcelSheet = Me.setup.ExcelFile.GetAddSheet("AreaTable")
            Dim wsV As clsExcelSheet = Me.setup.ExcelFile.GetAddSheet("VolumeTable")
            Dim wscol As Integer = -1

            If Not SF.Open(ShapeFilePath) Then Throw New Exception("Error reading shapefile.")
            IDFieldIdx = SF.FieldIndexByName(IDField)

            If Not Grid.Open(GridPath, GridDataType.UnknownDataType, False, GridFileType.UseExtension) Then Throw New Exception("Error reading grid.")

            Dim elevation As Double, surfacearea As Double, volume As Double
            Dim r As Long

            For i = 0 To SF.NumShapes - 1
                Dim Utils As New MapWinGIS.Utils
                Shape = SF.Shape(i)
                tmpGridPath = Me.setup.Settings.ExportDirRoot & "\tmpGrid_" & SF.CellValue(IDFieldIdx, i) & ".tif"
                If System.IO.File.Exists(tmpGridPath) Then setup.GeneralFunctions.deleteGrid(tmpGridPath)
                Dim StorageTable As New clsStorageTable(Me.setup)
                If Not Utils.ClipGridWithPolygon2(Grid, Shape, tmpGridPath, False) Then
                    Me.setup.Log.AddError("Error clipping grid by polygon: " & i)
                    Continue For
                Else
                    If Not BuildStorageTableFromGrid(tmpGridPath, StorageTable, 2) Then Throw New Exception("Error building storage table for grid: " & tmpGridPath)
                End If

                'write the header for the current dataset
                wscol += 2
                r = 0
                wsA.ws.Cells(r, wscol - 1).Value = "Elevation"
                wsV.ws.Cells(r, wscol - 1).Value = "Elevation"
                wsA.ws.Cells(r, wscol).Value = SF.CellValue(IDFieldIdx, i)
                wsV.ws.Cells(r, wscol).Value = SF.CellValue(IDFieldIdx, i)

                'write the values for the current dataset
                For Each elevation In StorageTable.AreasTable.Keys
                    r += 1
                    surfacearea = StorageTable.getAreaByKey(elevation)
                    volume = StorageTable.getVolumeByKey(elevation)
                    wsA.ws.Cells(r, wscol - 1).Value = elevation
                    wsV.ws.Cells(r, wscol - 1).Value = elevation
                    wsA.ws.Cells(r, wscol).Value = surfacearea
                    wsV.ws.Cells(r, wscol).Value = volume
                Next
                Utils = Nothing
            Next
            SF.Close()
            Grid.Close()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        Finally
            setup.ExcelFile.Save(True)
        End Try
    End Function


    Public Function BuildStorageTablesToTableFormat(ShapeFilePath As String, IDField As String, GridPath As String) As Boolean
        Try
            Dim SF As New MapWinGIS.Shapefile, Shape As MapWinGIS.Shape
            Dim Grid As New MapWinGIS.Grid
            Dim tmpGrid As New MapWinGIS.Grid
            Dim IDFieldIdx As Integer
            Dim tmpGridPath

            'initialize the excel table
            setup.ExcelFile.Path = Me.setup.Settings.ExportDirRoot & "\StorageTables.xlsx"
            Dim wsA As clsExcelSheet = Me.setup.ExcelFile.GetAddSheet("AreaTable")
            Dim wsV As clsExcelSheet = Me.setup.ExcelFile.GetAddSheet("VolumeTable")
            Dim wscol As Integer = -1

            'read the shapefile
            If Not SF.Open(ShapeFilePath) Then Throw New Exception("Error reading shapefile.")
            IDFieldIdx = SF.FieldIndexByName(IDField)

            'read the elevation grid
            If Not Grid.Open(GridPath, GridDataType.UnknownDataType, False, GridFileType.UseExtension) Then Throw New Exception("Error reading grid.")
            Dim minVal As Double = Grid.Minimum
            Dim maxVal As Double = Grid.Maximum
            Dim elevation As Double
            Dim r As Long, c As Long

            'first setup a basic 2D array structure for the data
            Dim Areas As Double(,)
            Dim Volumes As Double(,)
            Dim IDs As New List(Of String)
            Dim Elevations As New List(Of Double)
            Dim nVals As Integer = (Math.Round(maxVal, 2) - Math.Round(minVal, 2)) / 0.01 + 1
            ReDim Areas(nVals - 1, SF.NumShapes - 1)
            ReDim Volumes(nVals - 1, SF.NumShapes - 1)

            'then create the list of elevations
            elevation = minVal
            For i = 0 To nVals - 1
                elevation = Math.Round(minVal + i * 0.01, 2)
                Elevations.Add(elevation)
            Next

            'then create the list of id's
            For i = 0 To SF.NumShapes - 1
                IDs.Add(SF.CellValue(IDFieldIdx, i))
            Next

            'now walk through each combination of area + elevation and retrieve the area and volume
            For i = 0 To SF.NumShapes - 1
                Dim Utils As New MapWinGIS.Utils
                Shape = SF.Shape(i)
                tmpGridPath = Me.setup.Settings.ExportDirRoot & "\tmpGrid_" & SF.CellValue(IDFieldIdx, i) & ".tif"
                If System.IO.File.Exists(tmpGridPath) Then setup.GeneralFunctions.deleteGrid(tmpGridPath)
                Dim StorageTable As New clsStorageTable(Me.setup)
                If Not Utils.ClipGridWithPolygon2(Grid, Shape, tmpGridPath, False) Then
                    Me.setup.Log.AddError("Error clipping grid by polygon: " & i)
                    Continue For
                Else
                    If Not BuildStorageTableFromGrid(tmpGridPath, StorageTable, 2) Then Throw New Exception("Error building storage table for grid: " & tmpGridPath)
                End If

                'write the values for the current dataset
                For Each elevation In StorageTable.AreasTable.Keys
                    If Elevations.Contains(elevation) Then
                        r = Elevations.IndexOf(elevation)
                        c = IDs.IndexOf(SF.CellValue(IDFieldIdx, i))
                        Areas(r, c) = StorageTable.getAreaByKey(elevation)
                        Volumes(r, c) = StorageTable.getVolumeByKey(elevation)
                    End If
                Next
                Utils = Nothing
            Next

            'since all tables must be ascending, we can now perform a correction for any values that dit not occur in the elevation grrid
            For c = 0 To IDs.Count - 1
                Dim lastArea As Double = 0
                Dim lastVolume As Double = 0
                Dim lastElevation As Double = Elevations.Item(0)
                For r = 0 To Elevations.Count - 1
                    If Areas(r, c) < lastArea Then Areas(r, c) = lastArea
                    If Volumes(r, c) <= lastVolume Then Volumes(r, c) = lastVolume + (Elevations(r) - lastElevation) * lastArea
                    lastArea = Areas(r, c)
                    lastVolume = Volumes(r, c)
                    lastElevation = Elevations(r)
                Next
            Next

            'now that the arrays are filled, we can write them to excel
            Dim ID As String
            For c = 0 To SF.NumShapes - 1
                ID = SF.CellValue(IDFieldIdx, c)
                wsA.ws.Cells(0, c + 1).Value = ID
                wsV.ws.Cells(0, c + 1).Value = ID
            Next

            Dim Elev As Double
            For r = 0 To Elevations.Count - 1
                Elev = Elevations.Item(r)
                wsA.ws.Cells(r + 1, 0).Value = Elev
                wsV.ws.Cells(r + 1, 0).Value = Elev
            Next

            For c = 0 To IDs.Count - 1
                For r = 0 To Elevations.Count - 1
                    wsA.ws.Cells(r + 1, c + 1).Value = Areas(r, c)
                    wsV.ws.Cells(r + 1, c + 1).Value = Volumes(r, c)
                Next
            Next

            SF.Close()
            Grid.Close()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        Finally
            setup.ExcelFile.Save(True)
        End Try
    End Function

    Public Function BuildStorageTableFromGrid(GridPath As String, ByRef StorageTable As clsStorageTable, nDecimals As Integer) As Boolean
        Try
            Dim Grid As New MapWinGIS.Grid, r As Integer, c As Integer
            If Not Grid.Open(GridPath, GridDataType.UnknownDataType, False, GridFileType.UseExtension) Then Throw New Exception("Error building storage table from grid " & GridPath)
            Dim NodataVal As Single = Convert.ToSingle(Grid.Header.NodataValue)
            Dim Area As Double = Grid.Header.dX * Grid.Header.dY
            Me.setup.GeneralFunctions.UpdateProgressBar("Building storage table from grid " & GridPath, 0, 10)
            For r = 0 To Grid.Header.NumberRows - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", r, Grid.Header.NumberRows)
                For c = 0 To Grid.Header.NumberCols - 1
                    If Not Convert.ToSingle(Grid.Value(c, r)) = NodataVal Then
                        StorageTable.AddValue(Math.Round(Grid.Value(c, r), nDecimals), Area)
                    End If
                Next
            Next

            'now that we have the elevations and areas, convert the table to a new table with 1 cm increment and complete it
            StorageTable.Complete()             'sorts the table in ascending order and cumulates the areas and volumes

            'now we'll have to postprocess the storage table by sorting in 
            Grid.Close()
            Me.setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
            Return True
        Catch ex As Exception
            Return False
        Finally
        End Try
    End Function


    Public Sub setLGNShapeFile(ByVal Path As String)
        LanduseShapeFile = New clsLGNShapeFile(Me.setup)
        LanduseShapeFile.Path = Path
    End Sub

    Public Sub setCatchmentsShapefile(ByVal Path As String)
        'v2.000: migration to generic geodatasources!
        CatchmentDataSource.SetShapefileByPath(Path)
        'CatchmentShapeFile = New clsSubcatchmentDataSource(Me.setup, Path)
    End Sub

    Public Sub setBAGShapeFile(ByVal Path As String)
        BAGShape = New ClsPolyShapeFile(Me.setup, Path)
    End Sub

    Public Sub setDikeShapeFile(ByVal Path As String)
        DikeShapeFile = New clsPolyLineShapeFile(Me.setup)
        DikeShapeFile.SF.Path = Path
    End Sub

    Public Sub setPointShapeFile(ByVal path As String)
        PointShapeFile = New clsPointShapeFile(Me.setup, path)
    End Sub

    Public Function ClipShapeFiles(ShapeFileToBeClipped As Shapefile, ShapeFileToClip As Shapefile, SelectedShapesOnly As Boolean) As Shapefile
        Try
            Dim newSf As MapWinGIS.Shapefile
            newSf = ShapeFileToBeClipped.Clip(False, ShapeFileToClip, SelectedShapesOnly)
            If newSf Is Nothing Then Throw New Exception(ShapeFileToBeClipped.ErrorMsg(ShapeFileToBeClipped.LastErrorCode))
            Return newSf
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error clipping shapefile " & ShapeFileToBeClipped.Filename & " by shapefile " & ShapeFileToClip.Filename & ".")
            Return Nothing
        End Try
    End Function

    Public Function UnionShapeFiles(ShapeFile1 As Shapefile, ShapeFile2 As Shapefile) As Shapefile
        Return ShapeFile1.Union(False, ShapeFile2, False)
    End Function

    Public Function InverseDistanceWeighting() As Boolean
        'this routine performs an inverse distance weighing on a grid for a given set of z-values in a point shape file
        'it uses an existing grid set in setup.gisdata.elevationgrid as input
        'it uses setup.gisdata.pointshapefile as input

        'in order to save time the routine will only process cells of the starting grid that do not have nodata-value

        Try
            'first, read the xyz-values from the shapefile
            Dim XY(,) As Double = PointShapeFile.GetXYArray()
            Dim N As Integer = PointShapeFile.CountShapes
            Dim NX As Integer = 2   'the number of spatial dimensions (x,y) = 2
            Dim D As Integer = 2    'quadratic model (usually best results)
            Dim NQ As Integer = 15 ' 2 * Math.Max(3 / 4 * (NX + 2) * (NX + 1), 2 ^ NX + 1) 'number of points used to calculate nodal functions
            Dim NW As Integer = 25  'arbitrary choice
            Dim Z As New idwinterpolant 'the interpolated value
            Dim nRows As Integer = setup.GISData.ElevationGrid.nRow

            'start by calculating the interpolant, based on the array of XYZ-coordinates
            Call idwbuildmodifiedshepard(XY, N, NX, D, NQ, NW, Z)

            'time to read the grid!
            If Not setup.GISData.ElevationGrid.Read(False) Then Throw New Exception("Error reading grid.")

            'now we can use the interpolant Z to compute the value for other pairs of XY-coordinates
            Me.setup.GeneralFunctions.UpdateProgressBar("Processing grid...", 0, nRows, True)
            Dim r As Integer, c As Integer
            Dim X(NX - 1) As Double
            For r = 0 To setup.GISData.ElevationGrid.nRow - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", r, setup.GISData.ElevationGrid.nRow)
                For c = 0 To setup.GISData.ElevationGrid.nCol - 1
                    If setup.GISData.ElevationGrid.Grid.Value(c, r) <> setup.GISData.ElevationGrid.Grid.Header.NodataValue Then
                        Call setup.GISData.ElevationGrid.getXYCenterFromRC(r, c, X(0), X(1))
                        setup.GISData.ElevationGrid.Grid.Value(c, r) = idwcalc(Z, X)
                        Debug.Print(idwcalc(Z, X))
                    End If
                Next
            Next

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try



    End Function

    Public Sub setElevationGrid(ByVal Path As String, Optional ByVal InRam As Boolean = True)
        ElevationGrid = New clsRaster(Me.setup)
        ElevationGrid.Path = Path
        ElevationGrid.ReadHeader(MapWinGIS.GridDataType.UnknownDataType, InRam)
    End Sub

    Friend Function ReadSubcatchmentsFromShapefile(CatchmentShapefile As String, SubcatchmentIDField As String, CatchmentIDField As String) As Boolean
        'this function creates a collection of subcatchments that include an indicator of to which catchment they belong
        Dim Shapefile As New MapWinGIS.Shapefile
        Dim CatchmIdx As Long, SubCatchmIdx As Long
        Dim CatchmentID As String, SubCatchmentID As String
        Dim Catchment As clsCatchment, Subcatchment As clsSubcatchment
        Dim Utils As New MapWinGIS.Utils
        Dim i As Long
        Dim TotalShape As MapWinGIS.Shape

        Try

            If Not Shapefile.Open(CatchmentShapefile) Then Throw New Exception("Error reading subcatchment shapefile " & CatchmentShapefile)
            CatchmIdx = Shapefile.FieldIndexByName(CatchmentIDField)
            SubCatchmIdx = Shapefile.FieldIndexByName(SubcatchmentIDField)

            setup.GeneralFunctions.UpdateProgressBar("Reading (sub)catchments from shapefile.", 0, 10)
            For i = 0 To Shapefile.NumShapes - 1
                setup.GeneralFunctions.UpdateProgressBar("", i, Shapefile.NumShapes - 1)
                CatchmentID = Shapefile.CellValue(CatchmIdx, i)
                SubCatchmentID = Shapefile.CellValue(SubCatchmIdx, i)
                If CatchmentID = "" Then
                    Me.setup.Log.AddWarning("Warning: empty ID found for catchment in shapefile.")
                    Continue For
                End If
                If SubCatchmentID = "" Then
                    Me.setup.Log.AddWarning("Warning: empty ID found for subcatchment in shapefile.")
                    Continue For
                End If
                Catchment = Catchments.getAdd(CatchmentID)                                  'gets or adds this catchment

                Catchment.Shapes.Add(Shapefile.Shape(i))                                    'adds this shape to the catchment
                Subcatchment = Catchment.Subcatchments.getAdd(SubCatchmentID)               'gets or adds this subcatchment
                TotalShape = Shapefile.Shape(i)                                'sets this shape for the subcatchment
            Next

            'finalize by aggregating the collections of shapes for each catchment into TotalShape
            Dim newCatchSF As New MapWinGIS.Shapefile
            Dim newSubCatchCF As New MapWinGIS.Shapefile
            newCatchSF = Shapefile.AggregateShapes(False, CatchmIdx)
            For i = 0 To newCatchSF.NumShapes - 1
                Catchment = Catchments.getByID(newCatchSF.CellValue(0, i))
                Catchment.TotalShape = newCatchSF.Shape(i)
            Next

            'now aggregate the collections of shapes for each subcatchment, find its underlying catchment and add it to 
            newSubCatchCF = Shapefile.AggregateShapes(False, SubCatchmIdx)
            For i = 0 To newSubCatchCF.NumShapes - 1
                For Each Catchment In Catchments.Catchments.Values
                    If Catchment.Subcatchments.Subcatchments.ContainsKey(newSubCatchCF.CellValue(0, i).trim.toupper) Then
                        Subcatchment = Catchment.Subcatchments.Subcatchments.Item(newSubCatchCF.CellValue(0, i).trim.toupper)
                        TotalShape = newSubCatchCF.Shape(i)
                    End If
                Next
            Next


            Shapefile.Close()
            Return True
        Catch ex As Exception
            setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function SampleShapeCenterDataFromGrid(ByVal myShapeFilePath As String, ByVal ShapeField As String, ByVal myGridPath As String) As Boolean
        Try
            Dim i As Long, myShape As MapWinGIS.Shape, myPoint As MapWinGIS.Point
            Dim myLength As Double, myValue As Double, myFieldIdx As Long
            Dim mySF As New clsShapeFile(Me.setup, myShapeFilePath), myGrid As New clsRaster(Me.setup, myGridPath)

            If Not mySF.Open() Then Throw New Exception("Error opening shapefile.")
            If Not myGrid.ReadHeader(MapWinGIS.GridDataType.UnknownDataType, False) Then Throw New Exception("Error reading grid file header.")
            If Not myGrid.CompleteMetaHeader Then Throw New Exception("Error autocompleting grid header.")
            If Not myGrid.Read(False) Then Throw New Exception("Error reading raster file.")
            If Not mySF.sf.StartEditingTable Then Throw New Exception("Error editing table for shapefile.")

            myFieldIdx = mySF.GetFieldIdx(ShapeField)
            If myFieldIdx < 0 Then
                myFieldIdx = mySF.sf.EditAddField(ShapeField, MapWinGIS.FieldType.DOUBLE_FIELD, 10, 10)
            End If

            Me.setup.GeneralFunctions.UpdateProgressBar("Sampling grid values....", 0, 10)
            For i = 0 To mySF.sf.NumShapes - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i + 1, mySF.sf.NumShapes)

                myShape = mySF.sf.Shape(i)
                myLength = myShape.Length
                myPoint = myShape.Center

                'get the grid value for this point
                If Not myGrid.GetCellValueFromXY(myPoint.x, myPoint.y, myValue) Then Me.setup.Log.AddWarning("No grid value found for shape " & i)
                mySF.sf.EditCellValue(myFieldIdx, i, myValue)
            Next
            Me.setup.GeneralFunctions.UpdateProgressBar("Sampling complete.", 0, 10)

            mySF.sf.StopEditingTable(True)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function




    Public Function CutawayShapeFileFromGrid(ByRef myGrid As clsRaster, ByRef mySfPath As String, ByVal ID As String)
        'author: siebe bosch
        'date: 27-1-2014
        'this routine cuts away the area of a shapefile from a given grid
        'it does so by first rasterizing the shapefile, giving it a field with values of 1
        'then setting the grid values underneath the shapes to nodata if field value = 1
        Try
            Dim GDALArgs As String
            Dim myUtils As New MapWinGIS.Utils
            Dim cutGridPath As String = Me.setup.Settings.ExportDirRoot & "\" & ID & "_CUTAWAY.tif"
            Dim cutGrid As MapWinGIS.Grid, mySF As New MapWinGIS.Shapefile
            Dim r As Long, c As Long

            If Not myGrid.Grid.Open(myGrid.Path, MapWinGIS.GridDataType.UnknownDataType, True) Then Throw New Exception("Error reading grid " & myGrid.Path)
            myGrid.CompleteMetaHeader()

            Call Me.setup.GISData.MakeUniformIntField(mySfPath, "SELECT", 1)
            GDALArgs = "-a SELECT -of GTiff -te " & myGrid.XLLCorner & " " & myGrid.YLLCorner & " " & myGrid.XURCorner & " " & myGrid.YURCorner & " -tr " & myGrid.dX & " " & myGrid.dY & " -ot Float64 "
            If Not mySF.Open(mySfPath) Then Throw New Exception("Could not open cutaway shapefile.")
            If Not myUtils.GDALRasterize(mySfPath, cutGridPath, GDALArgs) Then Throw New Exception("Could not rasterize cutaway shapefile.")
            mySF.Close()
            mySF = Nothing

            'process the values from this rasterized shapefile and apply them to the newgrid
            cutGrid = New MapWinGIS.Grid
            If Not cutGrid.Open(cutGridPath, MapWinGIS.GridDataType.UnknownDataType, False, MapWinGIS.GridFileType.GeoTiff) Then Throw New Exception("Could not read rasterized cutaway shapefile.")
            Me.setup.GeneralFunctions.UpdateProgressBar("Processing cutaway grid for area " & ID, 0, 100)
            For r = 0 To myGrid.nRow - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", r + 1, myGrid.nRow)
                For c = 0 To myGrid.nCol - 1
                    If cutGrid.Value(c, r) = 1 Then
                        myGrid.Grid.Value(c, r) = myGrid.NoDataVal
                    End If
                Next
            Next
            cutGrid.Close()
            cutGrid = Nothing

            If System.IO.File.Exists(cutGridPath) Then System.IO.File.Delete(cutGridPath)
            myGrid.CompleteMetaHeader()
            If Not myGrid.Grid.Save() Then Throw New Exception("Error in function CutawayShapeFileFromGrid. Could not save grid " & myGrid.Grid.Filename)
            myGrid.Close()
            myUtils = Nothing
            Return True
        Catch ex As Exception
            Me.setup.Log.AddWarning(ex.Message)
            Return False
        End Try

    End Function

    Friend Function clipAreasFromCatchmentShapeFile() As Boolean
        'doorloopt alle catchments en clipt de areashapefile naar ieder catchment. 
        Dim myCatchment As clsCatchment, myUtils As New MapWinGIS.Utils, i As Integer
        Dim myShape As MapWinGIS.Shape, mySubcatchment As clsSubcatchment, myAreaID As String
        Dim clippedShape As MapWinGIS.Shape = Nothing
        For Each myCatchment In Catchments.Catchments.Values
            Me.setup.GeneralFunctions.UpdateProgressBar("Clipping area shapes for catchment " & myCatchment.ID, 0, 10)
            'doorloop nu iedere shape in de area shape, clip deze naar het polygoon van het stroomgebied en voeg hem toe aan de collection
            For i = 0 To SubcatchmentDataSource.Shapefile.sf.NumShapes - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i + 1, SubcatchmentDataSource.Shapefile.sf.NumShapes)
                myShape = SubcatchmentDataSource.Shapefile.sf.Shape(i)
                If myShape.IsValid Then clippedShape = myUtils.ClipPolygon(MapWinGIS.PolygonOperation.INTERSECTION_OPERATION, myShape, myCatchment.TotalShape)

                'voeg deze shape toe als area aan het onderhavige catchment
                If Not clippedShape Is Nothing Then
                    If clippedShape.IsValid Then
                        myAreaID = SubcatchmentDataSource.GetTextValue(i, enmInternalVariable.ID)
                        mySubcatchment = myCatchment.Subcatchments.getAdd(myAreaID)
                        'v1.799: siebe hier nog de shape opslaan!
                        'mySubcatchment.TotalShape = clippedShape
                    End If
                End If
            Next
        Next

    End Function

    Friend Function ReadSubcatchmentsShapeFile(Optional ByVal minArea As Double = 0, Optional ByVal maxNum As Integer = 0) As Boolean
        'maakt een collectie met Catchments en onderliggende Areas aan
        'en doet dit door de shapefile met area's in te lezen
        'Let op: dit kan dus alleen als de shapefile met areas ook een veld met Catchment ID bevat!!!
        Dim myShape As MapWinGIS.Shape, i As Long
        Dim mySubcatchment As clsSubcatchment
        Dim nAreas As Integer
        Dim CatchmentIDx As Integer

        'retrieve the catchment column index
        If Me.setup.GISData.SubcatchmentDataSource.Fields.ContainsKey(enmInternalVariable.ParentID) Then
            'for this property stick to the first field encountered
            For Each GeoField In Me.setup.GISData.SubcatchmentDataSource.Fields.Item(enmInternalVariable.ParentID).Values
                CatchmentIDx = GeoField.ColIdx
            Next
        End If

        Dim myCatchment As clsCatchment
        Dim myCatchmentID As String
        Dim utils As New MapWinGIS.Utils

        'foutafhandeling
        If CatchmentIDx <= 0 Then
            Me.setup.Log.AddError("Could not run function ReadAreaShapeFile since the area shape file has no valid CatchmentID Field index number specified.")
            Return False
        End If

        'speciaal voor debugdoeleinden kunnen we hier het aantal in te lezen gebieden beperken
        If maxNum = 0 Then
            nAreas = Me.setup.GISData.SubcatchmentDataSource.GetNumberOfRecords
        Else
            nAreas = maxNum
        End If

        'maak eerst de collectie met areas leeg
        For i = 0 To nAreas - 1
            'haal de shape op
            myShape = SubcatchmentDataSource.Shapefile.sf.Shape(i)
            If myShape.IsValid AndAlso myShape.Area > minArea Then

                'zoek het stroomgebied op en maak dit eventueel aan
                myCatchmentID = SubcatchmentDataSource.Shapefile.sf.CellValue(CatchmentIDx, i)
                myCatchment = GetAddCatchment(myCatchmentID.Trim.ToUpper)

                'maak nieuwe area aan
                mySubcatchment = New clsSubcatchment(Me.setup)
                mySubcatchment.Catchment = myCatchment

                'bepaal het ID van deze area
                If SubcatchmentDataSource.Fields.ContainsKey(enmInternalVariable.ID) Then
                    mySubcatchment.ID = SubcatchmentDataSource.GetTextValue(i, enmInternalVariable.ID)
                Else
                    mySubcatchment.ID = "Gebied" & i.ToString.Trim
                End If

                'voeg de area toe aan het stroomgebied.
                If Not myCatchment.Subcatchments.Subcatchments.ContainsKey(mySubcatchment.ID.Trim.ToUpper) Then
                    myCatchment.Subcatchments.Subcatchments.Add(mySubcatchment.ID, mySubcatchment)
                Else
                    setup.Log.AddError("ID of subcatchments must be unique!")
                    Return False
                End If
            ElseIf myShape.Area < minArea Then
                setup.Log.AddWarning("Shape index# " & i & " in area shapefile is " & myShape.Area & " m2, which is smaller than minimum of " & Me.setup.Settings.GISSettings.minShapeArea & " m2 and was skipped during import.")
            ElseIf Not myShape.IsValid Then
                setup.Log.AddWarning("Shape index# " & i & " in area shapefile is not valid and was was skipped during import.")
            End If
        Next

        Return True

    End Function


    Public Function GetCatchment(ByVal myId As String) As clsCatchment
        If Catchments.Catchments.ContainsKey(myId.Trim.ToUpper) Then
            Return Catchments.Catchments.Item(myId.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetAddCatchment(ByVal myID As String) As clsCatchment
        Dim myCatchment As clsCatchment
        If Catchments.Catchments.ContainsKey(myID.Trim.ToUpper) Then
            Return Catchments.Catchments.Item(myID.Trim.ToUpper)
        Else
            myCatchment = New clsCatchment(Me.setup)
            myCatchment.ID = myID
            myCatchment.CatchmentIdx = Catchments.Catchments.Count      'for backwards compatibility we need this counter (for Aa en Maas, Boven-Aa)
            Catchments.Catchments.Add(myID.Trim.ToUpper, myCatchment)
            Return Catchments.Catchments.Item(myID.Trim.ToUpper)
        End If
    End Function

    Friend Sub GetLabelPoint(ByVal mypoly As MapWinGIS.Shape, ByRef X As Double, ByRef Y As Double)
        'deze routine bepaalt voor een gegeven polygoon een geschikt labelpoint
        'het doet dit door eerst een rechthoek om de polygoon te trekken, en dan de langste zijde te halveren
        'het labelpoint komt ergens op die lijn

        Dim myPoint As MapWinGIS.Point = mypoly.InteriorPoint
        X = myPoint.x
        Y = myPoint.y
        Me.setup.GeneralFunctions.ReleaseComObject(myPoint, True)

    End Sub

    Public Function GetShapeFieldIdx(ByRef sf As MapWinGIS.Shapefile, path As String, FromMemory As Boolean, FieldName As String) As Integer
        Try
            If FromMemory Then
                Return setup.GeneralFunctions.GetShapeFieldIdxByName(sf, FieldName)
            Else
                Return setup.GISData.getShapeFieldIdxFromFileName(path, FieldName)
            End If
            Throw New Exception("Error getting shapefield index for fieldname " & FieldName & " in file " & path)
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function GetShapeFieldIdx from class clsGISData.")
            Return -1
        End Try
    End Function
    Public Function getShapeFieldIdxFromFileName(ByVal Path As String, ByVal FieldName As String) As Integer
        Dim sf = New MapWinGIS.Shapefile
        Dim i As Long

        Try
            If FieldName.Trim = "" Then
                Me.setup.Log.AddMessage("One or empty fieldnames specified for shapefile " & Path & ".")
                Return -1
            Else
                If System.IO.File.Exists(Path) Then
                    If sf.Open(Path) Then
                        For i = 0 To sf.NumFields - 1
                            If sf.Field(i).Name.Trim.ToLower = FieldName.Trim.ToLower Then
                                Return i
                            End If
                        Next
                        Me.setup.Log.AddWarning("Could not find shapefield " & FieldName & " in shapefile " & Path)
                        Return -1
                    Else
                        Me.setup.Log.AddError("Could not read shapefile " & Path & " due to error " & sf.ErrorMsg(sf.LastErrorCode))
                        Return -1
                    End If
                Else
                    Me.setup.Log.AddWarning("Shapefile does not exist: " & Path)
                    Return -1
                End If
            End If

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return -1
        Finally
            sf.Close()
        End Try

    End Function

    Public Function getShapeFieldIdxFromShapeFile(ByVal sf As MapWinGIS.Shapefile, ByVal FieldName As String) As Integer
        Dim i As Long

        Try
            For i = 0 To sf.NumFields - 1
                If sf.Field(i).Name.Trim.ToLower = FieldName.Trim.ToLower Then
                    Return i
                End If
            Next
            Me.setup.Log.AddError("Could not find shapefield " & FieldName & " in shapefile " & sf.Filename)
            Return -1
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return -1
        End Try

    End Function


    Public Function getFieldTypeFromShapefilePath(ByVal Path As String, ByVal FieldName As String) As MapWinGIS.FieldType
        Dim sf = New MapWinGIS.Shapefile
        Dim i As Long

        Try
            If System.IO.File.Exists(Path) Then
                If sf.Open(Path) Then
                    For i = 0 To sf.NumFields - 1
                        If sf.Field(i).Name.Trim.ToLower = FieldName.Trim.ToLower Then
                            Return sf.Field(i).Type
                        End If
                    Next
                    Me.setup.Log.AddError("Could not find shapefield " & FieldName & " in shapefile " & Path)
                    sf.Close()
                    Return Nothing
                Else
                    Me.setup.Log.AddError("Could not read shapefile " & sf.Filename & ".")
                    Return Nothing
                End If
            Else
                Me.setup.Log.AddWarning("Shapefile does not exist: " & Path)
                Return Nothing
            End If
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function





    Public Function YZTableFromGrid(ByVal myPath As String, ByRef LowestPoint As clsXYZ, ByRef PolyShape As MapWinGIS.Shape, Optional ByVal nSteps As Long = 0, Optional ByVal DivideByReachLength As Double = 1, Optional ByVal ElevationMultiplier As Double = 1) As clsSobekTable

        Dim myGrid As MapWinGIS.Grid, i As Long, stepSize As Double
        Dim XLLCenter As Double, YLLCenter As Double
        Dim DX As Double, DY As Double, nRows As Long, nCols As Long, NoDataVal As Double
        Dim r As Long, c As Long, myVal As Double
        Dim ElevationData As New List(Of Double)
        Dim myTable As New clsSobekTable(Me.setup)

        If System.IO.File.Exists(myPath) Then
            myGrid = New MapWinGIS.Grid
            If myGrid.Open(myPath, MapWinGIS.GridDataType.UnknownDataType, False) Then
                XLLCenter = myGrid.Header.XllCenter
                YLLCenter = myGrid.Header.YllCenter
                DX = myGrid.Header.dX
                DY = myGrid.Header.dY
                nRows = myGrid.Header.NumberRows
                nCols = myGrid.Header.NumberCols
                NoDataVal = myGrid.Header.NodataValue

                Me.setup.GeneralFunctions.UpdateProgressBar("Building YZ-table from elevation grid " & myPath, 0, 10)

                For r = 0 To nRows - 1
                    Me.setup.GeneralFunctions.UpdateProgressBar("", r + 1, nRows)
                    For c = 0 To nCols - 1
                        myVal = myGrid.Value(c, r)
                        If Math.Abs(myVal - NoDataVal) > 1 Then  'the clipping of grids makes rounding errors!
                            If myVal < LowestPoint.Z Then 'maak van de huidige cel het laagste punt
                                If Me.setup.GeneralFunctions.GetMapWinGridCellCenterXY(XLLCenter, YLLCenter, DX, DY, nRows, nCols, r, c, LowestPoint.X, LowestPoint.Y) Then LowestPoint.Z = myVal
                            End If
                            ElevationData.Add(myVal)
                        End If
                    Next
                Next
                myGrid.Close()
                If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)
                If System.IO.File.Exists(Replace(myPath, ".asc", ".prj")) Then System.IO.File.Delete(Replace(myPath, ".asc", ".prj"))
            Else
                setup.Log.AddError("Error reading grid " & myPath)
            End If
        Else
            'to get a reasonable estimate for the lowest point after all, take the interior point
            If Not PolyShape Is Nothing Then
                LowestPoint.X = PolyShape.InteriorPoint.x
                LowestPoint.Y = PolyShape.InteriorPoint.y
            End If

            If System.IO.File.Exists(myPath) Then System.IO.File.Delete(myPath)
            If System.IO.File.Exists(Replace(myPath, ".asc", ".prj")) Then System.IO.File.Delete(Replace(myPath, ".asc", ".prj"))
            Return Nothing
        End If

        If ElevationData.Count > 0 Then

            'decide which stepsize to use when walking through this massive table
            'if nSteps is zero, we'll use a step size of 1 by default
            If nSteps <= 0 Then
                stepSize = 1
            ElseIf ElevationData.Count > nSteps / 2 Then
                'NOTE: because we're making an YZ table every value will be used twice: once negative and once positive
                nSteps = nSteps / 2 'therefore correct the stepsize
                stepSize = Me.setup.GeneralFunctions.RoundUD(ElevationData.Count / nSteps, 0, False)
            Else
                stepSize = 1
            End If

            ElevationData.Sort()

            'write all elevation data to an yz-table, do this in the stepsize defined above
            'Note: we'll divide the total width by two and use it as a negative value for the left side and positive for the right side

            'the first point. 
            myTable.AddDataPair(2, -ElevationData.Count * DX * DY / (DivideByReachLength * 2), ElevationData(ElevationData.Count - 1) * ElevationMultiplier)

            'left side
            For i = ElevationData.Count - 1 - stepSize To stepSize Step -stepSize
                If ElevationData(i) <> ElevationData(i + stepSize) Then
                    myTable.AddDataPair(2, -(i - 1) * DX * DY / (DivideByReachLength * 2), ElevationData(i) * ElevationMultiplier)
                End If
            Next

            'centerpoint
            myTable.AddDataPair(2, 0, ElevationData(0) * ElevationMultiplier)

            'right side
            For i = stepSize To ElevationData.Count - 1 - stepSize Step stepSize
                If ElevationData(i) <> ElevationData(i + stepSize) Then
                    myTable.AddDataPair(2, (i - 1) * DX * DY / (DivideByReachLength * 2), ElevationData(i) * ElevationMultiplier)
                End If
            Next

            'and the last point
            myTable.AddDataPair(2, ElevationData.Count * DX * DY / (DivideByReachLength * 2), ElevationData(ElevationData.Count - 1) * ElevationMultiplier)

            'de lijst met elevationdata mag worden leeggemaakt. Wordt hierna niet langer gebruikt
            ElevationData = Nothing
            Return myTable
        Else
            Return Nothing
        End If
    End Function

    Public Function getPolyLineMidPoint(ByVal chanShape As MapWinGIS.Shape) As clsXY
        Dim myXY As New clsXY
        Dim prevPoint As MapWinGIS.Point, prevDist As Double, maxDist As Double
        Dim curPoint As MapWinGIS.Point, curDist As Double
        Dim i As Long

        maxDist = chanShape.Length
        curDist = 0
        prevDist = 0
        For i = 1 To chanShape.numPoints - 1
            prevPoint = chanShape.Point(i - 1)
            curPoint = chanShape.Point(i)
            prevDist = curDist
            curDist += Math.Sqrt((curPoint.x - prevPoint.x) ^ 2 + (curPoint.y - prevPoint.y) ^ 2)
            If curDist >= maxDist / 2 Then
                myXY.X = setup.GeneralFunctions.Interpolate(prevDist, prevPoint.x, curDist, curPoint.x, maxDist / 2)
                myXY.Y = setup.GeneralFunctions.Interpolate(prevDist, prevPoint.y, curDist, curPoint.y, maxDist / 2)
                Return myXY
            End If
        Next
        Return Nothing

    End Function

    Public Function ElevationTableFromFilledCircle(ByRef myGrid As clsRaster, ByVal r As Integer, ByVal c As Integer, ByVal StartRadius As Double, ByVal EndRadius As Double, ByVal MinNumValidPoints As Integer, ByVal SkipNodataCells As Boolean, ByVal SkipZeroValues As Boolean) As clsSobekTable

        'author: Siebe Bosch
        'date: 21-02-2014
        'this routine builds an elevation table from a circular buffer (filled or doughnut) around a given point and adds the valid points to an elevation table
        'it builds circles around the starting cell, increasing the radius until both of the following criteria are met:
        'number of VALID values > minimum
        'last radius >= maxRadius
        Try

            Dim maxCellRadius As Integer, minCellRadius As Integer, cellRadius As Integer
            Dim curLv As Double, Skip As Boolean, n As Integer = 0
            Dim myTable As New clsSobekTable(Me.setup)
            myTable.ElevationCollection = New Dictionary(Of Single, Long)
            Dim CircleDict As New Dictionary(Of String, clsRC)
            Dim TotalDict As New Dictionary(Of String, clsRC)
            Dim myKey As String, myRC As clsRC

            'set the minimum and maximum search radius
            minCellRadius = StartRadius / myGrid.dX       'min cell radius expressed in the number of cells
            maxCellRadius = EndRadius / myGrid.dX         'max cell radius expressed in the number of cells

            cellRadius = minCellRadius

            Do
                CircleDict = myGrid.CollectCellsFromCircle(c, r, cellRadius)

                'Dim X As Double, Y As Double
                'myGrid.getXYFromRC(r, c, X, Y)

                For Each myKey In CircleDict.Keys
                    'avoid double entries
                    If Not TotalDict.ContainsKey(myKey) Then
                        TotalDict.Add(myKey, CircleDict.Item(myKey))

                        myRC = CircleDict.Item(myKey)
                        curLv = myGrid.Grid.Value(myRC.C, myRC.R)
                        Skip = False

                        If (curLv = myGrid.NoDataVal OrElse curLv < -3.4E+38) AndAlso SkipNodataCells Then Skip = True
                        If curLv = 0 AndAlso SkipZeroValues Then Skip = True

                        If Not Skip Then
                            n += 1
                            curLv = Math.Round(curLv, 2)  'round to 2 digits. This is sufficient for elevation tables and saves a lot of memory!
                            If myTable.ElevationCollection.ContainsKey(curLv) Then
                                myTable.ElevationCollection.Item(curLv) += 1
                            Else
                                myTable.ElevationCollection.Add(curLv, 1)
                            End If
                        End If

                    End If
                Next

                'if we have sufficient data, exit the loop. Otherwise increase the search radius
                If n >= MinNumValidPoints AndAlso cellRadius >= maxCellRadius Then Exit Do
                cellRadius += 1   'increase the cell radius by one cell
            Loop

            'clean the dictionaries from memory
            CircleDict = Nothing
            TotalDict = Nothing

            'now postprocess the elevation data and place the appropriate results in an elevation-storage table
            If myTable.ElevationCollection.Count = 0 Then
                Return Nothing
            ElseIf Not myTable.SortElevationData(1, myGrid.dX, 1) Then
                Throw New Exception("Could not process elevation grid for circular buffer around cell " & r & " " & c)
            End If

            Return myTable
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function ElevationTableFromCircularBuffer of class clsGISData.")
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function GetNearestShapeDistance(ByVal X As Double, ByVal Y As Double, ByRef mySF As MapWinGIS.Shapefile, ByRef ShapeIdx As Long, ByRef Chainage As Double, ByRef Distance As Double, ByVal Criterium As Double) As Boolean

        'calculates the closest distance from a point (X,Y) to the nearest shape in a shapefile
        'it does this by creating a circle and intersecting it with the shapefile
        'it adjusts the circle's radius until the smallest possible radius with intersection is reached
        Dim Done As Boolean = False
        Dim Radius As Double, Dist As Double, iShape As Long, myShape As MapWinGIS.Shape
        Dim ptShape As MapWinGIS.Shape, bufShape As MapWinGIS.Shape
        Dim MaxRadius As Double, MinRadius As Double, Intersects As Boolean

        'find the largest possible radius that intersects with the shapefile
        Radius = Me.setup.GeneralFunctions.Pythagoras(X, Y, mySF.Extents.xMin, mySF.Extents.yMin)
        Dist = Me.setup.GeneralFunctions.Pythagoras(X, Y, mySF.Extents.xMin, mySF.Extents.yMax)
        If Dist > Radius Then Radius = Dist
        Dist = Me.setup.GeneralFunctions.Pythagoras(X, Y, mySF.Extents.xMax, mySF.Extents.yMin)
        If Dist > Radius Then Radius = Dist
        Dist = Me.setup.GeneralFunctions.Pythagoras(X, Y, mySF.Extents.xMax, mySF.Extents.yMax)
        If Dist > Radius Then Radius = Dist

        'create a shape from the point
        ptShape = New MapWinGIS.Shape
        ptShape.Create(MapWinGIS.ShpfileType.SHP_POINT)
        ptShape.AddPoint(X, Y)

        'initialize lastRadius
        MaxRadius = Radius
        MinRadius = 0

        While Not Done

            'create a buffer around the shape
            bufShape = New MapWinGIS.Shape
            bufShape = ptShape.Buffer(Radius, 100)

            'check if it intersects 
            Intersects = False
            For iShape = 0 To mySF.NumShapes - 1
                myShape = mySF.Shape(iShape)
                If myShape.Intersects(bufShape) Then
                    Intersects = True
                    Exit For
                End If
            Next

            If Intersects Then
                MaxRadius = Radius
            Else
                MinRadius = Radius
            End If

            'calculate the new radius
            Radius = (MaxRadius + MinRadius) / 2
            If MaxRadius - MinRadius < Criterium Then Done = True

        End While

        Return Radius

    End Function



    Public Function FindSnapLocationInShapeFile(ByVal X As Double, ByVal Y As Double, ByVal RadiusIncrement As Double, ByRef mySF As MapWinGIS.Shapefile, ByRef ShapeIdx As Long, ByRef Chainage As Double, ByRef Distance As Double) As Boolean

        'date: 2014-03-03
        'Author: Siebe Bosch
        'calculates the closest distance to a line in a shapefile
        'it does so by creating circular buffers around the starting point, increasing the radius and searching for the first intersection with the shapefile
        'once the intersecting ShapeIdx is found, it uses GetClosestPoint to find the nearest shape point to our (X,Y) co-ordinate
        'then there's only two line segments left to investigate: the line before the closest point and the line after
        'for both lines the routine then finds the shortest distance to our co-ordinate and returns the distance plus the distance of this point on the shape itself (chainage)
        Dim i As Long
        Dim lc As Double
        Dim ShapeLine As clsLineDefinition
        Dim ptShape As MapWinGIS.Shape
        Dim newSf As New MapWinGIS.Shapefile
        Dim Utils As New MapWinGIS.Utils
        Dim myDist As Double, myChainage As Double
        Dim PointIdx As Long

        'initialize
        Distance = 99999999999          'smallest distance between the co-ordinate and its snap-point on the shape
        Chainage = 0                    'distance of the snap-point on the shape
        ptShape = New MapWinGIS.Shape
        ptShape.Create(MapWinGIS.ShpfileType.SHP_POINT)
        ptShape.AddPoint(X, Y)

        Dim myShape As MapWinGIS.Shape, myPoint As MapWinGIS.Point, prevPoint As MapWinGIS.Point, nextPoint As MapWinGIS.Point

        'note: at first, mychainage is the distance on the line segment we're processing
        'this means it does not take into account the earlier line segments of the shape
        'therefore the chainage needs correction in the end.

        Try

            'initialize the search radius

            'Dim myWatch As New Stopwatch
            'myWatch.Start()
            'Debug.Print("Start: " & myWatch.ElapsedMilliseconds)

            '---------------------------------------------------------------------
            'start searching for the nearest intersection with our shapefile
            'increase the search radius until we found one
            '---------------------------------------------------------------------
            'Do
            '  bufShape = New MapWinGIS.Shape
            '  bufShape = ptShape.Buffer(Radius, 20)

            '  'now check where it intersects with our shapefile
            '  For i = 0 To mySF.NumShapes - 1
            '    iShape = mySF.Shape(i)
            '    If bufShape.Intersects(iShape) Then
            '      ShapeIdx = i
            '      Exit Do
            '    End If
            '  Next
            '  Radius += RadiusIncrement
            'Loop

            Dim Indices As New Object
            Dim myExtents As New MapWinGIS.Extents()
            myExtents.SetBounds(X - RadiusIncrement, Y - RadiusIncrement, 0, X + RadiusIncrement, Y + RadiusIncrement, 0) 'create a minimum size initial extent

            i = 1
            While Not mySF.SelectShapes(myExtents, 0, MapWinGIS.SelectMode.INTERSECTION, Indices)
                i += 1
                myExtents.SetBounds(X - RadiusIncrement * i, Y - RadiusIncrement * i, 0, X + RadiusIncrement * i, Y + RadiusIncrement * i, 0)
            End While

            'Debug.Print("Buffered: " & myWatch.ElapsedMilliseconds)

            '--------------------------------------------------------------------------------------------------------------
            'next, find the nearest point on the shapefile. First create a new shapefile containing only the shape we found
            '--------------------------------------------------------------------------------------------------------------
            ShapeIdx = Indices(0)
            myShape = mySF.Shape(ShapeIdx)
            mySF.SelectNone()
            mySF.ShapeSelected(ShapeIdx) = True
            newSf = mySF.ExportSelection()

            'now figure out which is the closest point on our new shapefile.
            newSf.GetClosestVertex(X, Y, 9999999, ShapeIdx, PointIdx, lc)

            If ShapeIdx >= 0 Then
                myShape = newSf.Shape(ShapeIdx)

                '--------------------------------------------------------------------------------------------------------------
                'two line segments remain to be investigated further: the one before the closest point and the one after
                'only if the pointidx = 0 or numpoints-2 there's only one line segment to investigate
                'note: distance = distance from co-ordinate to the snap-location; chainage is the distance of the snap-location on the shape
                '--------------------------------------------------------------------------------------------------------------
                If PointIdx = myShape.numPoints - 1 Then
                    'the snap point lies on the last line segment of the shape
                    'calculate the distance and chainage (= the distance of the snap point on the line shape, relative to the starting point)
                    prevPoint = myShape.Point(PointIdx - 1)
                    myPoint = myShape.Point(PointIdx)
                    ShapeLine = New clsLineDefinition(Me.setup, prevPoint, myPoint)
                    Call ShapeLine.getMinDistFromPoint(X, Y, myDist, myChainage)
                    Distance = myDist
                    Chainage = myChainage
                    For i = 1 To PointIdx - 1
                        Chainage += Me.setup.GeneralFunctions.Pythagoras(myShape.Point(i - 1).x, myShape.Point(i - 1).y, myShape.Point(i).x, myShape.Point(i).y)
                    Next
                    Return True
                ElseIf PointIdx = 0 Then
                    'the snap point lies on the first line segment of the shape
                    'calculate the distance and the chainage
                    myPoint = myShape.Point(PointIdx)
                    nextPoint = myShape.Point(PointIdx + 1)
                    ShapeLine = New clsLineDefinition(Me.setup, myPoint, nextPoint)
                    Call ShapeLine.getMinDistFromPoint(X, Y, myDist, myChainage)
                    Distance = myDist
                    Chainage = myChainage
                    Return True
                Else
                    'the snap point lies somewhere in a middle line segment
                    'calculate the distance and the chainage on both surrounding lines and use the smallest distance

                    'set the relevant shape vertices
                    prevPoint = myShape.Point(PointIdx - 1)
                    myPoint = myShape.Point(PointIdx)
                    nextPoint = myShape.Point(PointIdx + 1)

                    'start with the upstream line segment
                    ShapeLine = New clsLineDefinition(Me.setup, prevPoint, myPoint)
                    Call ShapeLine.getMinDistFromPoint(X, Y, myDist, myChainage)
                    If myDist < Distance Then
                        Distance = myDist
                        Chainage = myChainage
                        For i = 1 To PointIdx - 1
                            Chainage += Me.setup.GeneralFunctions.Pythagoras(myShape.Point(i - 1).x, myShape.Point(i - 1).y, myShape.Point(i).x, myShape.Point(i).y)
                        Next
                    End If

                    'and then the downstream line segment
                    ShapeLine = New clsLineDefinition(Me.setup, myPoint, nextPoint)
                    Call ShapeLine.getMinDistFromPoint(X, Y, myDist, myChainage)
                    If myDist < Distance Then
                        Distance = myDist
                        Chainage = myChainage
                        For i = 1 To PointIdx
                            Chainage += Me.setup.GeneralFunctions.Pythagoras(myShape.Point(i - 1).x, myShape.Point(i - 1).y, myShape.Point(i).x, myShape.Point(i).y)
                        Next
                    End If
                    Return True

                End If
            End If




            'Debug.Print("Snaplocation: " & myWatch.ElapsedMilliseconds)

            'myWatch.Reset()

            'if we end up here, something went wrong
            Return False

        Catch ex As Exception
            Me.setup.Log.AddError("Error in sub GetClosestDistanceToShapeFile of class clsGISData.")
            Return False
        End Try

    End Function


    Public Function FindSnapLocationInShapeFileBySearchRadius(ByVal X As Double, ByVal Y As Double, ByVal SearchRadius As Double, ByRef mySF As MapWinGIS.Shapefile, ByRef SnappingShapeIdx As Long, ByRef SnappingChainage As Double, ByRef SnappingDistance As Double, AllowDiagonalSnappingToVectorPoints As Boolean) As Boolean

        'new feature in v1.798
        'date: 2020-10-28
        'Author: Siebe Bosch
        'calculates the closest distance to a line in a shapefile by using a maximum search radius
        'it does so by first selecting all shapes that lie within the search perimeter of our point

        Dim i As Long
        Dim SegSnapDist As Double, SegSnapChainage As Double
        Dim MinDist As Double = 9.0E+99
        Dim PrevLength As Double

        'initialize
        SnappingShapeIdx = -1
        Dim myShape As MapWinGIS.Shape

        Try
            For i = 0 To mySF.NumShapes - 1
                If X < mySF.Shape(i).Extents.xMin - SearchRadius Then Continue For
                If X > mySF.Shape(i).Extents.xMax + SearchRadius Then Continue For
                If Y < mySF.Shape(i).Extents.yMin - SearchRadius Then Continue For
                If Y > mySF.Shape(i).Extents.yMax + SearchRadius Then Continue For

                'if we end up here we may have found a shape that is eligible for snapping
                myShape = mySF.Shape(i)
                PrevLength = 0

                'walk through all segments and calculate the snapping point
                For j = 0 To myShape.numPoints - 2
                    If Me.setup.GeneralFunctions.PointToLineSnapping(myShape.Point(j).x, myShape.Point(j).y, myShape.Point(j + 1).x, myShape.Point(j + 1).y, X, Y, SearchRadius, SegSnapChainage, SegSnapDist, AllowDiagonalSnappingToVectorPoints) Then
                        If SegSnapDist <= SearchRadius AndAlso SegSnapDist < MinDist Then
                            'we found a new valid point.
                            MinDist = SegSnapDist
                            SnappingChainage = PrevLength + SegSnapChainage   'the chainage for this point is the length of all previous segments
                            SnappingDistance = SegSnapDist
                            SnappingShapeIdx = i
                        End If
                    End If
                    PrevLength += Me.setup.GeneralFunctions.Pythagoras(myShape.Point(j).x, myShape.Point(j).y, myShape.Point(j + 1).x, myShape.Point(j + 1).y)
                Next
            Next

            If SnappingShapeIdx < 0 Then Return False Else Return True

        Catch ex As Exception
            Me.setup.Log.AddError("Error in sub FindSnapLocationInShapeFileBySearchRadius of class clsGISData.")
            Return False
        End Try

    End Function

    'Public Function FindNearestShapePoint(ByVal X As Double, ByVal Y As Double, ByRef mySF As MapWinGIS.Shapefile, ByRef ShapeIdx As Long, ByRef Chainage As Double, ByRef Distance As Double) As Boolean

    '  'calculates the closest distance to a line in a shapefile
    '  'returns the distance (pytagoras, perpendicular to the line), the chainage (distance on the shape of the intersection point of the perpendicular line) and distance
    '  Dim i As Long, j As Long, k As Long
    '  Dim ShapeLine As clsLineDefinition
    '  Dim myDist As Double, myChainage As Double
    '  Dim maxRadius As Double, ShpIdx As Long, PointIdx As Long
    '  Dim myShape As MapWinGIS.Shape
    '  Dim LastPointIdx As Long


    '  'initialize
    '  Distance = 99999999999
    '  Chainage = 0
    '  Dim Found As Boolean = False

    '  'note: mychainage is the distance on the line segment we're processing
    '  'this means it does not take into account the earlier line segments of the shape
    '  'therefore the chainage needs correction in the end.

    '  Try

    '    For i = 0 To mySF.NumShapes - 1
    '      For j = 0 To mySF.Shape(i).numPoints - 2

    '        'create a straight line definition for this segment
    '        ShapeLine = New clsLineDefinition(Me.setup, mySF.Shape(i).Point(j), mySF.Shape(i).Point(j + 1))

    '        'find the shortest distance from the XY-coordinate to this line
    '        Call ShapeLine.getMinDistFromPoint(X, Y, myDist, myChainage)

    '        If myDist < Distance Then
    '          Distance = myDist

    '          'calculate the real chainage on this shape by adding the lenght of all previous line segments
    '          Chainage = myChainage
    '          If j > 0 Then
    '            For k = 1 To j
    '              Chainage += Me.setup.GeneralFunctions.Pythagoras(mySF.Shape(i).Point(k).x, mySF.Shape(i).Point(k).y, mySF.Shape(i).Point(k - 1).x, mySF.Shape(i).Point(k - 1).y)
    '            Next
    '          End If

    '          ShapeIdx = i
    '          Found = True
    '        End If

    '      Next
    '    Next

    '    Return Found

    '  Catch ex As Exception
    '    Me.setup.Log.AddError("Error in sub GetClosestDistanceToShapeFile of class clsGISData.")
    '    Return False
    '  End Try

    'End Function

    Public Function ShapeIntersectsGrid(ByRef myShape As MapWinGIS.Shape, ByRef myGrid As clsRaster) As Boolean
        'this function finds out whether or not a grid intersects with a shape
        'NOTE: is not perfect!!
        'author: siebe bosch
        'date: 22 jan 2014

        'ensure that the header has been read
        If myGrid.XLLCorner = 0 OrElse myGrid.YURCorner = 0 Then
            myGrid.ReadHeader(MapWinGIS.GridDataType.UnknownDataType, False)
        End If

        Dim myPoint As MapWinGIS.Point
        Dim myUtils As New MapWinGIS.Utils

        Dim i As Long

        'first check either of the four corners of the grid. If one of them lies inside the grid, return true
        myPoint = New MapWinGIS.Point
        myPoint.x = myGrid.XLLCorner
        myPoint.y = myGrid.YLLCorner
        If myUtils.PointInPolygon(myShape, myPoint) Then Return True

        myPoint = New MapWinGIS.Point
        myPoint.x = myGrid.XLLCorner
        myPoint.y = myGrid.YURCorner
        If myUtils.PointInPolygon(myShape, myPoint) Then Return True

        myPoint = New MapWinGIS.Point
        myPoint.x = myGrid.XURCorner
        myPoint.y = myGrid.YLLCorner
        If myUtils.PointInPolygon(myShape, myPoint) Then Return True

        myPoint = New MapWinGIS.Point
        myPoint.x = myGrid.XURCorner
        myPoint.y = myGrid.YURCorner
        If myUtils.PointInPolygon(myShape, myPoint) Then Return True

        'then walk through all of the shape's vector points and check whether they're inside the grid
        For i = 0 To myShape.numPoints - 1
            myPoint = myShape.Point(i)
            If myPoint.x >= myGrid.XLLCorner AndAlso myPoint.x <= myGrid.XURCorner AndAlso myPoint.y >= myGrid.YLLCorner AndAlso myPoint.y <= myGrid.YURCorner Then Return True
        Next

        'if still not found, check for line intersections between the grid's boundaries and shape lines
        'SiEBE: NOG INBOUWEN

        Return False

    End Function

    Public Function MakeUniformIntField(ByVal shpPath As String, ByVal FieldName As String, ByVal FieldVal As Integer) As Integer
        'this function creates a new shapefile field of the Integer type 
        'and fills allrecords with the specified uniform value
        Dim sf As New MapWinGIS.Shapefile, iField As Long, iShape As Long, Found As Boolean = False
        Dim newField As New MapWinGIS.Field, FieldIdx As Long
        Try
            If System.IO.File.Exists(shpPath) Then

                If Not sf.Open(shpPath) Then Throw New Exception("Could not open shapefile " & shpPath)
                If Not sf.StartEditingTable Then Throw New Exception("Could not edit shapfile table " & shpPath)

                'remove the existing field of the same name (if found)
                For iField = 0 To sf.NumFields - 1
                    If sf.Field(iField).Name.ToLower = FieldName.ToLower Then
                        sf.EditDeleteField(iField)
                        Exit For
                    End If
                Next

                'create a new field of the required name
                FieldIdx = sf.EditAddField(FieldName, MapWinGIS.FieldType.INTEGER_FIELD, 10, 10)
                For iShape = 0 To sf.NumShapes - 1
                    sf.EditCellValue(FieldIdx, iShape, FieldVal)
                Next

                sf.StopEditingTable(True)
                sf.Close()
            End If

            Return FieldIdx

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return 0
        End Try

    End Function

    Public Function PopulateComboBoxByShapeValues(ByRef cmbValues As ComboBox, ByVal ShapeFile As String, ByRef cmbIDField As String) As Boolean
        Try
            Dim sf As New MapWinGIS.Shapefile
            Dim Field As MapWinGIS.Field
            'Dim idx As Integer


            If Not sf.Open(ShapeFile) Then Throw New Exception("Error reading shapefile " & ShapeFile)
            'idx = sf.FieldIndexByName(cmbIDField)
            'If idx < 0 Then Throw New Exception("Error: shape field " & cmbIDField & " not found in shapefile " & ShapeFile)
            Field = sf.FieldByName(cmbIDField)
            If Field Is Nothing Then Throw New Exception("Error: shapefield " & cmbIDField & " not found in shapefile " & ShapeFile)

            sf.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function getShapeFileType(ByVal myShapeFilePath As String, ByRef myType As MapWinGIS.ShpfileType) As Boolean
        Try
            Dim mw As New MapWinGIS.Shapefile
            If Not mw.Open(myShapeFilePath) Then Throw New Exception
            myType = mw.ShapefileType
            mw.Close()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Shapefile could not be opened: " & myShapeFilePath)
            Return False
        End Try
    End Function

    Public Sub BuildFloodDepthMap(hGridPath As String, eGridPath As String, ResultsPath As String)
        Dim hGrid As New MapWinGIS.Grid, eGrid As New MapWinGIS.Grid
        hGrid.Open(hGridPath, GridDataType.UnknownDataType, False, GridFileType.UseExtension)
        eGrid.Open(eGridPath, GridDataType.UnknownDataType, False, GridFileType.UseExtension)

        Dim nRows As Integer = hGrid.Header.NumberRows
        Dim nCols As Integer = hGrid.Header.NumberCols

        Try
            Dim r As Long, c As Long

            If hGrid.Header.NumberRows <> eGrid.Header.NumberRows Then
                Throw New Exception("Error: extrents of elevation grid and waterlevel grid are nog equal.")
            ElseIf hGrid.Header.NumberCols <> eGrid.Header.NumberCols Then
                Throw New Exception("Error: extrents of elevation grid and waterlevel grid are nog equal.")
            End If

            'read the elevation grid
            Me.setup.GeneralFunctions.UpdateProgressBar("Processing grids...", 0, 10)

            'for now, adjust the values in the elevation grid. When finished, we'll save it under a different name
            For r = 0 To nRows - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", r, 1000)
                For c = 0 To nCols - 1
                    If hGrid.Value(c, r) <> hGrid.Header.NodataValue Then
                        If eGrid.Value(c, r) <> eGrid.Header.NodataValue Then
                            hGrid.Value(c, r) = Math.Max(hGrid.Value(c, r) - eGrid.Value(c, r), 0)
                        Else
                            hGrid.Value(c, r) = hGrid.Header.NodataValue
                        End If
                    End If
                Next
            Next

            hGrid.Save(ResultsPath, GridFileType.UseExtension)

            hGrid.Close()
            eGrid.Close()

        Catch ex As Exception

        End Try


    End Sub

    Public Sub Progress(KeyOfSender As String, Percent As Integer, Message As String) Implements ICallback.Progress
        Me.setup.GeneralFunctions.UpdateProgressBar(Message, Percent, 100, True)
    End Sub

    Public Sub [Error](KeyOfSender As String, ErrorMsg As String) Implements ICallback.Error
        Throw New NotImplementedException()
    End Sub
End Class

