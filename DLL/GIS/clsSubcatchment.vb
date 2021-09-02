Option Explicit On

Imports System.Runtime.InteropServices
Imports STOCHLIB.General
Imports GemBox.Spreadsheet

' TODO Geeft Setup class mee aan de constructor dan is die overal bekend

Public Class clsSubcatchment
    Friend ID As String
    Friend Catchment As clsCatchment
    Public WP As Double
    Public ZP As Double
    Public OutflowStructureID As String                                 'outflow structure ID
    Public SurfaceLevel As Double

    'Public TotalShape As MapWinGIS.Shape                                'the shape that encompasses the entire subcatchment
    'Public TotalShapeNoChannels As MapWinGIS.Shape                      'the shape that encompasses the entire subcatchment WITHOUT existing channel area

    'v1.798: introducing a list of 1d backbone reaches that intersect our subcatchment
    Public BackboneReaches As New List(Of String)

    'v1.798
    'here we introduce two variables that keep track of data we need to build models based on the topological relationships
    Friend TotalChannelLength As Double     'the total length of all channels inside this subcatchment
    Friend UpstreamSubcatchments As Dictionary(Of String, clsSubcatchment) 'a list of all subcatchment that discharge via this one


    'keep a list of index numbers for the landuse shapefile that intersect with our subcatchment
    'Public LandUseShapeIndices As New List(Of Integer)
    Friend SkippedSoilTypesArea As New Dictionary(Of String, Double)
    Friend SkippedLanduseTypesArea As New Dictionary(Of String, Double)

    'path to our intermediate shapefiles that are used in the building process
    'Public CatchmentBuilderSF As clsCatchmentBuilderShapefile           'a union of landuse, soiltype, sewage areas for the current subcatchment
    Public SoilGrid As clsRaster                                        'the soiltypes grid, clipped by this subcatchment's extents
    Public LandUseGrid As clsRaster                                         'the landuse grid, clipped by this subcatchment's extents
    Public SewageAreaGrid As clsRaster

    '----------------------------------------------------------------------------------------------------------------------------------------------------------------
    'below we define the compete landuse list for this subcatchment
    'it includes all subdivisions of unpaved area by soiltype and landuse class as well as sewage areas.
    '----------------------------------------------------------------------------------------------------------------------------------------------------------------
    Public PavedSewageAreas As New Dictionary(Of String, Double)         'contains the paved area that is assigned to each sewage area inside this subcatchment
    Public PavedUntreatedArea As Double
    Public GreenhouseArea As Double

    '----------------------------------------------------------------------------------------------------------------------------------------------------------------
    'we also keep track of the areas that have been moved between untreated paved, treated paved and unpaved areas
    '----------------------------------------------------------------------------------------------------------------------------------------------------------------
    Public PavedAreaMovedToTreatedPavedArea As Double                   'the area that has been moved from untreated paved to treated paved in order to meet the attribute value of the sewage area
    Public UnpavedAreaMovedToTreatedPavedArea As Double                 'the area that has been moved from unpaved to treated paved in order to meet the attribute value of the sewage area
    Public TreatedPavedAreaMovedToPavedArea As Double                   'the area that has been moved from treated paved to untreated paved in order to meet the attribute value of the sewage area

    'the following three are used on the smallest level of subcatchments, where every shape only has ONE type of landuse and ONE sewage area
    'Friend SewageAreaID As String                                       'contains the id for the sewage area in which this subcatchment lies
    'Friend GISLanduseCode As Integer                                    'the landuse code from GIS (either LGN or BGT)

    Friend InUse As Boolean = True
    Friend RRNodeTPRecords As Dictionary(Of String, clsRRNodeTPRecord)  'a dictionary containing all rr nodes inside this subcatchment
    Friend ErnstDrainageDefinition As clsErnstDrainageDefinition        'contains the complete drainage definition for this area
    Friend RRMultiPaved As clsRRPavedNodes                              'kan bestaan uit verschillende typen rioleringen, dus meerdere
    'let op: omdat iedere area meerdere paved-knopen kan hebben, hebben we de RRCF-connectie ondergebracht bij het object clsRRPavedNode
    Friend RRPavedNode As clsRRPavedNode                                'de originele enkelvoudige paved-knoop
    Friend RRCFPaved As clsSbkReachObject                               'let op: in geval van modelcatchments krijg het hele modelcatchment slechts één rrcf-conn. Dan wordt deze niet gebruikt

    Friend LatPrecipitation As clsSbkReachObject                        'reachobject for precipitation on openwater
    Friend LatEvaporation As clsSbkReachObject                          'reachobject for evaporation on openwater
    Friend LatSeepage As clsSbkReachObject                              'reachobject for seepage on openwater
    Friend LatDrainageInflow As clsSbkReachObject                       'reachobject for drainage inflow on openwater

    Friend RRGreenhouseNode As clsRRGreenhouseNode                      'ieder gebied heeft max 1 greenhouse-knoop
    Friend RRCFGreenhouse As clsSbkReachObject                          'ieder gebied heeft max 1 greenhouse connection
    Friend RROpenwaterNode As clsRROpenwaterNode                        'ieder gebied heeft max 1 openwater-knoop

    'Friend LandUseList As New Dictionary(Of String, Double) 'lijst met oppervlaktes per landgebruikstype
    'Friend SbkSoilType() As Double                          'lijst met oppervlaktes per sobekbodemtype (CAPSIM)
    Friend CAPSIMTypes As New Dictionary(Of Integer, Double) 'lijst met oppervlaktes per CAPSIM bodemtypenummer
    'Friend SbkGWTable As New Dictionary(Of String, Double)  'lijst met oppervlaktes per grondwatertrap
    Friend ElevationData As New List(Of Single)

    'EDIT 29-6-2017 to solve memory issues, we've decided to remove these in-memory storage tables and rely on the database connection from now on
    'Public OpenWaterStorage As clsSobekTable                'lijst met oppervlakte openwater per centimeter stijging
    'Public ElevationStorage As clsSobekTable                'lijst met oppervlakte maaiveld, afgeleid van ElevationData
    'Public TotalStorage As clsSobekTable                    'lijst met de totale bergingscapaciteit van de area
    Friend LowestPoint As New clsXYZ                         'the lowest surface elevation point
    Friend SnappingStartPoint As clsXYZ                      'the starting point from which to 'snap' to a reach
    Friend SnapLocation As clsSbkReachObject
    Friend Channels As clsChannels
    Friend AfvoerCoef As Double                             'afvoercoefficient van het gebied
    Friend SeepageSummer As Double                          'de zomergemiddelde kwelflux in mm/d
    Friend SeepageWinter As Double                          'de wintergemiddelde kwelflux in mm/d
    Friend WeightedDischargeCoefFactor As Double            'een vermenigvuldigingsfactor voor de neerslag, waarmee gecorrigeerd wordt voor verschillen in afvoercoefficienten tussen afwateringseenheden
    Friend MeteoStation As clsMeteoStation                  'neerslagstation
    Friend MeteoStationID As String                         'ID van het neerslagstation
    Friend SummerFlushVolume As Double                      'flux van de zomerdoorspoeling in mm/d

    'derived values
    Public TotalAttributeArea As Double                     'the total area of this subcatchment as registered in the attribute data
    Public TotalSewageArea As Double                        'the total area of sewage area that covers our subcatchment
    Friend OpenwaterArea As Double = 0                      'openwater area for this subcatchment. Optionally retrieved from area shapefile! If 0 then recomputed during the process

    'Friend MaskShape As MapWinGIS.Shape                     'een shape voor masking (gedeelten die je wilt overhouden)
    'Friend CutawayShape As MapWinGIS.Shape                  'een shape voor wegsnijden (gedeelten die je wilt wegsnijden)
    'Friend Stats As clsAreaStats                             'statistieken voor deze area

    Public GridCollection As clsGridCollection              'collection of all underlying grid tiles
    Public ElevationGrid As clsRaster                       'the matching elevation grid for this area
    'Public ElevationTableCollection As Dictionary(Of String, clsSobekTable)  'a list of tables containing elevation data. e.g. for a buffer around each connection point of a shapefile

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        setup = mySetup

        'initialize our container for upstream subcatchments
        UpstreamSubcatchments = New Dictionary(Of String, clsSubcatchment)

        'initialize our GIS data. Bugfix in v2.0. me.Setup was not available in the class constructor yet so moved it to here
        SewageAreaGrid = New clsRaster(Me.setup)

        ErnstDrainageDefinition = New clsErnstDrainageDefinition
        'OpenWaterStorage = New clsSobekTable(Me.setup)
        'ElevationStorage = New clsSobekTable(Me.setup)
        'TotalStorage = New clsSobekTable(Me.setup)
        Channels = New clsChannels(Me.setup)
        GridCollection = New clsGridCollection(Me.setup)
        ElevationGrid = New clsRaster(Me.setup)
        'ElevationTableCollection = New Dictionary(Of String, clsSobekTable)
        'Stats = New clsAreaStats(Me.setup, Me)
        RRNodeTPRecords = New Dictionary(Of String, clsRRNodeTPRecord)
        Me.Initialize()
    End Sub

    Public Function PopulateBackboneReachList(BackboneChannelShapefile As String, ReachIDFieldIdx As Integer, ByRef SubcatchmentDataSource As MapWinGIS.Shapefile, SubcatchmentShapeIdx As Integer) As Boolean
        Try
            'new functionality in v1.798: creates a list of all 1D reaches from the backbone schematization that intersect our subcatchment
            'clip the channels shapefile by the extent of our total shape
            Dim ChannelSF As New MapWinGIS.Shapefile
            ChannelSF.Open(BackboneChannelShapefile)

            'deselect all shapes from the subcatchments shapefile
            SubcatchmentDataSource.SelectNone()

            'select the current subcatchment
            SubcatchmentDataSource.ShapeSelected(SubcatchmentShapeIdx) = True

            'use the selected shape to clip the channels shapefile
            Dim ChannelsClipped As New MapWinGIS.Shapefile
            ChannelsClipped = ChannelSF.Clip(False, Setup.GISData.SubcatchmentDataSource.Shapefile.sf, True)

            Dim ReachID As String
            If Not ChannelsClipped Is Nothing Then
                For i = 0 To ChannelsClipped.NumShapes - 1
                    ReachID = ChannelsClipped.CellValue(ReachIDFieldIdx, i)
                    If Not BackboneReaches.Contains(ReachID.Trim.ToUpper) Then BackboneReaches.Add(ReachID.Trim.ToUpper)
                Next
            Else
                Me.Setup.Log.AddMessage("No reaches from the backbone schematization intersect with subcatchment " & ID & ". Snapping inside subcatchment not possible.")
            End If

            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function PopulateBackboneReachees of class clsSubcatchment.")
            Return False
        End Try
    End Function


    Public Function assignCrossSections(ByRef profDef As clsProfileDefRecord) As Boolean
        Try
            'assigns a new cross sections for every reach that connects to our current subcatchment. 
            'it positions this cross section ad 1/4th of the reachlength from the centerpoint
            'the reason is that the 2nd half of the reach is assigned to another subcatchment
            'one exception though: if a reach ends or starts at a boundary, there is no AreaID
            'in those situations we add another cross section, at the boundary's end
            Dim profDat As clsProfileDatRecord
            Dim profObj As clsSbkReachObject
            Dim iReach As Integer = 0
            Dim nReach As Integer = Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Assigning cross sections to reaches in area " & ID, iReach, nReach, True)
            For Each myReach As clsSbkReach In Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                'create a cross section object for this subcatchment and add it to the reach
                Dim chainage As Double = 0
                Dim Use As Boolean = False
                If myReach.bn.AreaID = ID AndAlso myReach.InUse Then
                    'v1.799: possibilitiy to change the location of the profile
                    'chainage = Me.Setup.ChannelBuilder.GeneralSettings.BoundaryDistance / 2 ' myReach.getReachLength / 4
                    chainage = myReach.getReachLength * 1 / 4
                    Use = True
                ElseIf myReach.en.AreaID = ID AndAlso myReach.InUse Then
                    'v1.799: possibilitiy to change the location of the profile
                    'chainage = myReach.getReachLength - Me.Setup.ChannelBuilder.GeneralSettings.BoundaryDistance / 2 ' * 3 / 4
                    chainage = myReach.getReachLength * 3 / 4
                    Use = True
                End If

                If Use Then
                    profObj = New clsSbkReachObject(Me.Setup, Me.Setup.SOBEKData.ActiveProject.ActiveCase)
                    profObj.ID = Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.MakeUniqueNodeID("crs" & ID)
                    profObj.Name = "crs" & ID
                    profObj.InUse = True
                    profObj.lc = chainage
                    profObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE
                    profObj.ci = myReach.Id
                    myReach.ReachObjects.Add(profObj)

                    'create a profile.dat record and add it to the collection
                    profDat = New clsProfileDatRecord(Me.Setup)
                    profDat.ID = profObj.ID
                    profDat.InUse = True
                    profDat.rl = 0
                    profDat.rs = 0
                    profDat.di = profDef.ID
                    Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Add(profDat.ID.Trim.ToUpper, profDat)
                End If

                ''add a cross section for situations where the reach starts or ends at a boundary
                'Use = False
                'If myReach.bn.AreaID = "" OrElse myReach.bn.AreaID Is Nothing Then
                '    chainage = myReach.getReachLength / 4
                '    Use = True
                'ElseIf myReach.en.AreaID = "" OrElse myReach.en.AreaID Is Nothing Then
                '    chainage = myReach.getReachLength * 3 / 4
                '    Use = True
                'End If

                'If Use Then
                '    profObj = New clsSbkReachObject(Me.Setup, Me.Setup.SOBEKData.ActiveProject.ActiveCase)
                '    profObj.ID = Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.MakeUniqueNodeID("crs" & ID)
                '    profObj.Name = "crs" & ID
                '    profObj.InUse = True
                '    profObj.lc = chainage
                '    profObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE
                '    profObj.ci = myReach.Id
                '    myReach.ReachObjects.Add(profObj)

                '    'create a profile.dat record and add it to the collection
                '    profDat = New clsProfileDatRecord(Me.Setup)
                '    profDat.ID = profObj.ID
                '    profDat.InUse = True
                '    profDat.rl = 0
                '    profDat.rs = 0
                '    profDat.di = profDef.ID
                '    Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Add(profDat.ID.Trim.ToUpper, profDat)
                'End If




            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error infunction assignCrossSections of class clsSubcatchment.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function CalcTotalTreatedPavedArea() As Double
        Dim myArea As Double = 0
        For Each mySewage As Double In PavedSewageAreas.Values
            myArea += mySewage
        Next
    End Function


    Public Sub Clear()
        'OpenWaterStorage = Nothing
        'ElevationStorage = Nothing
        'TotalStorage = Nothing
        Channels = Nothing
        GridCollection = Nothing
        ElevationGrid = Nothing
        'ElevationTableCollection = Nothing
    End Sub

    Public Function SetDefaultLandUse() As Boolean
        'UnpavedLandUseList.Values(0).Values(1) += TotalGISArea
        Return True
    End Function

    Public Function GetAreaAtElevation(Elevation As Double) As Double
        Dim dt As New DataTable
        If Not Me.setup.GeneralFunctions.SQLiteQuery(setup.SqliteCon, "SELECT ELEVATION, AREA FROM CHANNELTABLES WHERE AREAID = '" & ID.Trim.ToUpper & "';", dt) Then Throw New Exception("Error retrieving channel storage table for area " & ID & ".")
        Return Me.setup.GeneralFunctions.InterpolateFromDataTable(dt, WP, 0, 1)
    End Function

    Public Sub SetID(ByVal myID As String)
        ID = myID
    End Sub
    Public Function GetID() As String
        Return ID
    End Function


    Public Function GetChannelStorageFromDatabase(ByRef StorageTable As clsSobekTable) As Boolean
        Try
            Dim dt As New DataTable
            Me.setup.GeneralFunctions.SQLiteQuery(Me.setup.SqliteCon, "SELECT ELEVATION, AREA FROM CHANNELTABLES WHERE AREAID = '" & ID.Trim.ToUpper & "' ORDER BY ELEVATION;", dt)
            If dt.Rows.Count = 0 Then Return False
            StorageTable.PopulateFromDataTable(ID, dt, 0)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error populating channel storage table for area " & ID & " from database.")
            Return False
        End Try
    End Function

    Public Function GetTotalStorageFromDatabase(ByRef StorageTable As clsSobekTable, CloseDBAfterwards As Boolean) As Boolean
        Try
            Dim dt As New DataTable
            Me.setup.GeneralFunctions.SQLiteQuery(Me.setup.SqliteCon, "SELECT ELEVATION, AREA FROM STORAGETABLES WHERE AREAID = '" & ID.Trim.ToUpper & "' ORDER BY ELEVATION;", dt, CloseDBAfterwards)
            If dt.Rows.Count = 0 Then Return False
            StorageTable.PopulateFromDataTable(ID, dt, 0)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error populating total storage table for area " & ID & " from database.")
            Return False
        End Try
    End Function

    Public Sub SetRaster(ByRef myRaster As clsRaster)
        ElevationGrid = myRaster
    End Sub

    'Public Sub SetMaskShape(ByVal myShape As MapWinGIS.Shape)
    '    MaskShape = myShape
    'End Sub

    'Public Sub setCutawayShape(ByVal myShape As MapWinGIS.Shape)
    '    CutawayShape = myShape
    'End Sub

    Public Function BurnLowestChannelElevationUsingCirularBuffer(ByVal SurfaceLevelRadius As Double, ByVal ReplaceNodataVals As Boolean, ByVal ReplaceZeroVals As Boolean, ByVal ReplaceOtherVals As Boolean) As Boolean

        'author: siebe bosch
        'date: 19-2-2014
        'edit: 27-5-2014
        'this routine "burns" channel dimensions (buffer around channel shapefile) into the elevation grid
        'prerequirements are that the Area must have a channel shapefile specified and an elevation grid

        Try
            Dim GDALArgs As String
            Dim myUtils As New MapWinGIS.Utils
            Dim selectionGridPath As String = Me.setup.Settings.ExportDirRoot & "\" & ID & "_select.tif"
            Dim clippedSFpath As String = Me.setup.Settings.ExportDirRoot & "\" & ID & "_clipped.shp"
            Dim bufferSFPath As String = Me.setup.Settings.ExportDirRoot & "\" & ID & "_buffer.shp"
            Dim selectionGrid As clsRaster
            Dim curLv As Double
            Dim r As Long, c As Long
            Dim ClippedSF As New MapWinGIS.Shapefile
            Dim BufferSF As New MapWinGIS.Shapefile

            'first clear the selection of shapes in the area shapefile, then only select the shape belonging to the current area
            'finally clip the channel shapefile using the extents of the selected area shape
            Me.Setup.GISData.SubcatchmentDataSource.SelectRecordsbyTextFieldValue(GeneralFunctions.enmInternalVariable.ID, ID)
            ClippedSF = Me.Setup.GISData.ChannelDataSource.Shapefile.sf.Clip(False, Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf, True)
            Me.setup.GISData.SubcatchmentDataSource.Shapefile.sf.SelectNone()

            If ClippedSF Is Nothing Then Throw New Exception("")
            If Not ClippedSF.SaveAs(clippedSFpath) Then Throw New Exception("Could not save clipped shapefile of channels for area " & ID)
            If ClippedSF Is Nothing Then Throw New Exception("Warning: channels could not be clipped for area " & ID & ". No channels were burned in the elevation grid for this area.")

            'now create a buffer around the shapefile
            BufferSF = ClippedSF.BufferByDistance(SurfaceLevelRadius, 8, False, True)
            If Not BufferSF.SaveAs(bufferSFPath) Then Throw New Exception("Could not save buffered shapefile of channels for area " & ID)

            'finally rasteri  ze the buffer shapefile, using the extents of the area's elevation grid
            Call Me.setup.GISData.MakeUniformIntField(bufferSFPath, "SELECT", 1)
            GDALArgs = "-a SELECT -of GTiff -te " & ElevationGrid.XLLCorner & " " & ElevationGrid.YLLCorner & " " & ElevationGrid.XURCorner & " " & ElevationGrid.YURCorner & " -tr " & ElevationGrid.dX & " " & ElevationGrid.dY & " -ot Float32 "
            If Not BufferSF.Open(bufferSFPath) Then Throw New Exception("Could not open buffer shapefile containing channels for area " & ID)
            If Not myUtils.GDALRasterize(bufferSFPath, selectionGridPath, GDALArgs) Then Throw New Exception("Could not rasterize cutaway shapefile.")
            BufferSF.Close()
            BufferSF = Nothing

            If Not ElevationGrid.Grid.Open(ElevationGrid.Path, MapWinGIS.GridDataType.UnknownDataType, True) Then Throw New Exception("Could not read grid " & ElevationGrid.Path)

            'process the values from this rasterized shapefile and apply them to the newgrid
            selectionGrid = New clsRaster(Me.setup)
            selectionGrid.setExistingPath(selectionGridPath)
            If Not selectionGrid.Read(False) Then Throw New Exception("Could not read rasterized buffer shapefile.")
            Me.setup.GeneralFunctions.UpdateProgressBar("Filling up empty grid cells under shape buffer with lowest surrounding value...", 0, 100)

            'walk through the elevation grid and find cells that lay inside the selection
            Me.setup.GeneralFunctions.UpdateProgressBar("Burning lowest elevation values into empty grid cells for area " & ID, 0, 1)
            For r = 0 To ElevationGrid.nRow - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", r + 1, ElevationGrid.nRow)
                For c = 0 To ElevationGrid.nCol - 1
                    If selectionGrid.Grid.Value(c, r) = 1 Then

                        'the current cell falls inside the buffer around the channels. The value will be adjusted if the
                        curLv = ElevationGrid.Grid.Value(c, r)

                        If (curLv = ElevationGrid.NoDataVal OrElse curLv < -3.4E+38) AndAlso ReplaceNodataVals = True Then
                            ElevationGrid.Grid.Value(c, r) = ElevationGrid.getNearestLowestValue(r, c, False)
                        ElseIf curLv = 0 AndAlso ReplaceZeroVals = True Then
                            ElevationGrid.Grid.Value(c, r) = ElevationGrid.getNearestLowestValue(r, c, False)
                        ElseIf ReplaceOtherVals Then
                            ElevationGrid.Grid.Value(c, r) = ElevationGrid.getNearestLowestValue(r, c, False)
                        End If

                    End If
                Next
            Next

            ClippedSF.Close()
            selectionGrid.Close()
            selectionGrid = Nothing
            myUtils = Nothing

            'clean up after yourself!
            If System.IO.File.Exists(selectionGridPath) Then Me.setup.GeneralFunctions.deleteGrid(selectionGridPath)
            If System.IO.File.Exists(clippedSFpath) Then Me.setup.GeneralFunctions.deleteShapeFile(clippedSFpath)
            If System.IO.File.Exists(bufferSFPath) Then Me.setup.GeneralFunctions.deleteShapeFile(bufferSFPath)

            'and save our precious result :)
            If Not ElevationGrid.Grid.Save() Then Throw New Exception("Error in function BurnShapeFileInGrid: could not save grid " & ElevationGrid.Grid.Filename)
            ElevationGrid.Close()
            Return True

        Catch ex As Exception
            Me.setup.Log.AddWarning(ex.Message)
            Return False
        End Try
    End Function

    Public Function CutawayShapeFromGrid(ByVal mySfPath As String) As Boolean
        'author: siebe bosch
        'date: 29-1-2014
        'this routine cuts away the area of a shapefile from the elevation grid
        'it does so by setting the grid values underneath the shapes to nodata
        Try
            Dim GDALArgs As String
            Dim myUtils As New MapWinGIS.Utils
            Dim cutGridPath As String = Me.setup.Settings.ExportDirRoot & "\" & ID & "_cutaway.tif"
            Dim cutGrid As MapWinGIS.Grid, mySF As New MapWinGIS.Shapefile
            Dim r As Long, c As Long

            Call Me.setup.GISData.MakeUniformIntField(mySfPath, "SELECT", 1)
            GDALArgs = "-a SELECT -of GTiff -te " & ElevationGrid.XLLCorner & " " & ElevationGrid.YLLCorner & " " & ElevationGrid.XURCorner & " " & ElevationGrid.YURCorner & " -tr " & ElevationGrid.dX & " " & ElevationGrid.dY & " -ot Float32 "
            If Not mySF.Open(mySfPath) Then Throw New Exception("Could not open cutaway shapefile.")
            If Not myUtils.GDALRasterize(mySfPath, cutGridPath, GDALArgs) Then Throw New Exception("Could not rasterize cutaway shapefile.")
            mySF.Close()

            'process the values from this rasterized shapefile and apply them to the newgrid
            cutGrid = New MapWinGIS.Grid
            If Not cutGrid.Open(cutGridPath, MapWinGIS.GridDataType.UnknownDataType, False, MapWinGIS.GridFileType.GeoTiff) Then Throw New Exception("Could not read rasterized cutaway shapefile.")
            Me.setup.GeneralFunctions.UpdateProgressBar("Cutting out shape shape for elevation grid " & ID, 0, 100)
            For r = 0 To ElevationGrid.nRow - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", r + 1, ElevationGrid.nRow)
                For c = 0 To ElevationGrid.nCol - 1
                    If cutGrid.Value(c, r) = 1 Then
                        ElevationGrid.Grid.Value(c, r) = ElevationGrid.NoDataVal
                    End If
                Next
            Next

            cutGrid.Close()
            If System.IO.File.Exists(cutGridPath) Then System.IO.File.Delete(cutGridPath)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    'Public Function PopulateRasters() As Boolean
    '  Dim nGrid As Long = Me.setup.GISData.GridCollection.Grids.Count, iGrid As Long = 0
    '  Dim mySf As New MapWinGIS.Shapefile

    '  'THIS IS AN OLD ROUTINE FROM BEFORE I DISCOVERED THE VRT FORMAT (VIRTUAL GRID THAT REPRESENTS A COLLECTION OF GRIDS)

    '  Try
    '    Me.setup.GeneralFunctions.UpdateProgressBar("Populating rasters for area " & ID, 0, 1)
    '    If Shape Is Nothing Then Throw New Exception("Area shape not set for area " & ID)
    '    For Each myGrid As clsRaster In Me.setup.GISData.GridCollection.Grids.Values
    '      iGrid += 1
    '      Me.setup.GeneralFunctions.UpdateProgressBar("", iGrid, nGrid)

    '      'make sure the header has actually been read!
    '      If myGrid.XLLCorner = 0 OrElse myGrid.YURCorner = 0 Then
    '        If Not myGrid.ReadHeader(MapWinGIS.GridDataType.UnknownDataType, False) Then Throw New Exception("Could not read grid header for grid " & myGrid.Path)
    '      End If

    '      If Me.setup.GISData.ShapeIntersectsGrid(Shape, myGrid) Then
    '        'add it to the local grid collection for this area
    '        GridCollection.Grids.Add(myGrid.Path, myGrid)
    '      End If
    '    Next

    '    mySf.Close()
    '    Return True
    '  Catch ex As Exception
    '    Me.setup.Log.AddError(ex.Message)
    '    Return False
    '  End Try
    'End Function

    Public Function ValidateTotalStorageTable(CloseDBAfterwards As Boolean) As Boolean

        Try
            If InUse Then
                'this function validates the total storage and adds warnings if some things are not ok
                'It only returns false if the total storage table is nothing or contains no values
                Dim TotalStorage As clsSobekTable
                TotalStorage = New clsSobekTable(Me.setup)
                If Not GetTotalStorageFromDatabase(TotalStorage, False) Then Throw New Exception("Error retrieving storage table from database for subcatchment " & ID)

                If TotalStorage Is Nothing Then
                    Me.setup.Log.AddWarning("No data found in storage table for area " & ID)
                    Return False
                ElseIf TotalStorage.XValues.Count = 0 Then
                    Me.setup.Log.AddWarning("No data points found in storage table for area " & ID)
                    Return False
                Else
                    Dim meanSL As Double = TotalStorage.XValues.Values((TotalStorage.XValues.Count - 1) / 2)
                    If WP > meanSL Then Me.setup.Log.AddWarning("Winter target level > mean surface level for area " & ID & ".")
                    If ZP > meanSL Then Me.setup.Log.AddWarning("Summer target level > mean surface level for area " & ID & ".")
                    If WP < (meanSL - 3) Then Me.setup.Log.AddWarning("Winter target level more than 3 m under mean surface level for area " & ID & ".")
                    If ZP < (meanSL - 3) Then Me.setup.Log.AddWarning("Summer target level more than 3 m under mean surface level for area " & ID & ".")
                    If Not TotalStorage.IsIncreasing Then Throw New Exception("Error: elevation table for area " & ID & " is non-increasing.")
                End If
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error: total storage table for area " & ID & " is not valid.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        Finally
            If CloseDBAfterwards Then Me.setup.SqliteCon.Close()
        End Try


    End Function

    Public Sub Initialize()
        'ReDim SbkSoilType(121) '21 bodemsoorten, maar SOBEK loopt van 101 to 121. Gebruik index 0 voor totaalopp

        'initialize the shapefiles
        SoilGrid = New clsRaster(Me.Setup)
        LandUseGrid = New clsRaster(Me.setup)

    End Sub

    Public Function getAreaFromElevationData() As Double
        Return setup.GISData.ElevationGrid.dX * setup.GISData.ElevationGrid.dY * ElevationData.Count
    End Function

    Public Function getLargestCAPSIMType() As String
        '------------------------------------------------------------------------------------------------
        ' Auteur: Siebe Bosch
        ' Datum: 16-04-2012
        '
        ' Omschrijving:
        ' Deze subroutine geeft de meest voorkomende bodemsoort in de area terug
        '------------------------------------------------------------------------------------------------
        Dim mymax As Double = 0, myMaxBod As String = ""
        For i As Integer = 0 To CAPSIMTypes.Count - 1
            If CAPSIMTypes.Values(i) > mymax Then
                mymax = CAPSIMTypes.Values(i)
                myMaxBod = CAPSIMTypes.Keys(i)
            End If
        Next
        Return myMaxBod

    End Function

    Public Function OpenGrid(ByRef myGrid As MapWinGIS.Grid, ByVal myGridPath As String, ByVal myGridType As MapWinGIS.GridDataType) As Boolean
        'wacht dat het grid leesbaar is alvorens hem te openen. Dit doen we door een aantal keer te herhalen
        Dim Retry As Boolean = True, nTry As Integer = 0

        While Retry
            Try
                If Not System.IO.File.Exists(myGridPath) Then Throw New Exception("Error: grid does not exist: " & myGridPath)
                If myGrid.Open(myGridPath, MapWinGIS.GridDataType.UnknownDataType) Then Return True
            Catch ex As Exception
                nTry += 1
                Me.setup.GeneralFunctions.Wait(1000)
                Me.setup.Log.AddWarning("Attempt " & nTry & " to read " & myGridPath & " failed. Retrying.")
                If nTry > 30 Then
                    Me.setup.Log.AddError("Could not access " & myGridPath & " for reading.")
                    Return False
                End If
            End Try
        End While
        Return False

    End Function

    Public Function ReadTotalStorageFromDatabase(ByRef StorageTable As clsSobekTable, CloseDBAfterwards As Boolean) As Boolean
        Try
            Dim dt As New DataTable

            'retrieve the area record from the database
            Me.setup.GeneralFunctions.SQLiteQuery(Me.setup.SqliteCon, "SELECT ELEVATION, AREA FROM STORAGETABLES WHERE AREAID = '" & ID.Trim.ToUpper & "' ORDER BY ELEVATION;", dt, False)
            StorageTable.PopulateFromDataTable(ID, dt, 0)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function ReadTotalStorageFromDatabase")
        Finally
            If CloseDBAfterwards Then Me.setup.SqliteCon.Close()
        End Try
    End Function

    Public Function ReadElevationFromDatabase(ByRef StorageTable As clsSobekTable) As Boolean
        Try
            Dim dt As New DataTable

            'retrieve the area record from the database
            Me.setup.GeneralFunctions.SQLiteQuery(Me.setup.SqliteCon, "SELECT ELEVATION, AREA FROM SURFACETABLES WHERE AREAID = '" & ID.Trim.ToUpper & "' ORDER BY ELEVATION;", dt)
            If dt.Rows.Count = 0 Then Return False
            StorageTable.PopulateFromDataTable(ID, dt, 0)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function ReadElevationFromDatabase")
        End Try
    End Function


    Public Function CalcChannelStorageBasedOnUpstreamArea(CloseDBAfterwards As Boolean, UpstreamArea As Double, DischargeMMPD As Double, Chezy As Double, MaxSlopeCMperKM As Double) As Boolean
        Try
            Dim query As String
            Dim MinimumStorage As New clsSobekTable(Me.Setup)

            If InUse Then
                'example: at 7 mm/d we want no more than 2 cm/km with Chezy 40
                'Q = A * C * SQRT(R * i) where R = hydraulic radius and i = slope (m/m)
                'given a rectangular profile where w = 5 x d we get:
                'A = 5d * d = 5d^2
                'P = 5d + 2d = 7d
                'this results in:
                'Q = 5d^2 * C * SQRT(5d^2/7d * i)
                'Q/C = 5d^2 * SQRT(5d^2/7d * i)
                'Q/5C = d^2 *  SQRT(5d^2/7d * i)
                'Q^2/25C^2 = d^4 * 5/7d * i
                '7/5*Q^2/i25C^2 = d^5
                'd = (7/5*Q^2/i25C^2)^(1/5)

                Dim Q As Double = UpstreamArea * DischargeMMPD / 1000 / 24 / 3600 'in m3/s
                Dim C As Double = Chezy 'chezy value for smooth, well maintained channels
                Dim i As Double = (MaxSlopeCMperKM / 100) / 1000 '2 cm slope per 1000 m
                'Dim d As Double = Math.Max(0.5, (Q ^ 2 / ((6 * C) ^ 2 * i)) ^ (1 / 3)) 'depth of our channel. apply a minimum of 50 cm
                Dim d As Double = (7 / 5 * Q ^ 2 / (25 * i * C ^ 2)) ^ (1 / 5)
                Dim w As Double = d * 5                                'width of our channel as a function of depth
                d = Math.Max(0.5, d)                                   'enforce a minimum depth of 50 cm

                'add three datapairs that represent the underwater storage
                MinimumStorage.AddDataPair(2, WP - d, w * TotalChannelLength)
                MinimumStorage.AddDataPair(2, WP, w * TotalChannelLength)
                MinimumStorage.AddDataPair(2, WP + 1, w * TotalChannelLength)

            End If


            '--------------------------------------------------------------------------------------------------------------
            '     UPDATING THE DATABASE
            '--------------------------------------------------------------------------------------------------------------
            'update the database by first removing the existing table
            query = "DELETE FROM CHANNELTABLES WHERE AREAID = '" & ID.Trim.ToUpper & "';"
            If Not Setup.GeneralFunctions.SQLiteNoQuery(Setup.SqliteCon, query, False) Then Throw New Exception("Error deleting old channel storage records from database.")

            'then insert the new table. do this via a bulk insert
            Using cmd As New SQLite.SQLiteCommand
                cmd.Connection = Me.Setup.SqliteCon
                Using transaction = Me.Setup.SqliteCon.BeginTransaction
                    For i = 0 To MinimumStorage.XValues.Count - 1
                        cmd.CommandText = "INSERT INTO CHANNELTABLES (AREAID, ELEVATION, AREA) VALUES ('" & ID.Trim.ToUpper & "'," & MinimumStorage.XValues.Values(i) & "," & MinimumStorage.Values1.Values(i) & ");"
                        cmd.ExecuteNonQuery()
                    Next
                    transaction.Commit() 'this is where the bulk insert is finally executed.
                End Using
            End Using



            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function


    Public Function CombineChannelAndElevationStorage(CloseDBAfterwards As Boolean) As Boolean
        'deze routine combineert de bergingstabellen voor openwater en maaiveld
        'let op: het betreft hier geen optelling, maar telkens een selectie van de grootste waarde. In het channel-gedeelte
        'zal meestal het oppervlak openwater groter zijn, maar hogerop zal de maaiveldberging leidend zijn
        Dim nAffected As Integer, ChannelStor As Boolean = True, ElevationStor As Boolean = True

        Try

            If InUse Then
                '--------------------------------------------------------------------------------------------------------------
                '     'START BY RETRIEVING BOTH STORAGE TABLES FROM THE DATABASE
                '--------------------------------------------------------------------------------------------------------------
                Dim ChannelStorage As New clsSobekTable(Me.setup)
                Dim ElevationStorage As New clsSobekTable(Me.setup)
                Dim TotalStorage As New clsSobekTable(Me.setup)

                If Not GetChannelStorageFromDatabase(ChannelStorage) Then
                    Me.setup.Log.AddWarning("No Channel Storage table retrieved from database for subcatchment " & ID)
                End If


                '--------------------------------------------------------------------------------------------------------------
                '     COMPUTING TOTAL STORAGE
                '--------------------------------------------------------------------------------------------------------------
                If ChannelStorage.XValues.Count = 0 Then
                    TotalStorage = ElevationStorage
                ElseIf ElevationStorage.XValues.Count = 0 Then
                    TotalStorage = ChannelStorage
                Else
                    TotalStorage = setup.GeneralFunctions.mergeStorageTablesByMaximum(ChannelStorage, ElevationStorage)
                End If

                '--------------------------------------------------------------------------------------------------------------
                '     UPDATING THE DATABASE
                '--------------------------------------------------------------------------------------------------------------
                'remove any old records regarding this area and then insert the new table
                nAffected = Me.setup.GeneralFunctions.SQLiteNoQuery(setup.SqliteCon, "DELETE FROM STORAGETABLES WHERE AREAID = '" & ID.Trim.ToUpper & "';", False)

                'then insert the new table. do this via a bulk insert
                Dim i As Long, n As Long = TotalStorage.XValues.Count
                Using cmd As New SQLite.SQLiteCommand
                    cmd.Connection = Me.setup.SqliteCon
                    Using transaction = Me.setup.SqliteCon.BeginTransaction
                        For i = 0 To n - 1
                            cmd.CommandText = "INSERT INTO STORAGETABLES (CATCHMENTID, AREAID,ELEVATION,AREA) VALUES ('" & Catchment.ID & "','" & ID.Trim.ToUpper & "'," & TotalStorage.XValues.Values(i) & "," & TotalStorage.Values1.Values(i) & ");"
                            cmd.ExecuteNonQuery()
                        Next
                        transaction.Commit() 'this is where the bulk insert is finally executed.
                    End Using
                End Using

            End If

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        Finally
            If CloseDBAfterwards Then Me.setup.SqliteCon.Close()
        End Try
    End Function

    Public Sub OnMessageLogged(ByVal sender As Object, ByVal e As MessageEventArgs)
        If e.MessageType = MessageEventArgs.MessageTypes.Debug Then
            Debug.Write("Debug: ")
        End If

        Debug.WriteLine(e.MessageText)

    End Sub


End Class
