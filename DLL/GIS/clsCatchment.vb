Imports STOCHLIB.General
Imports GemBox.Spreadsheet
Imports System.IO

Public Class clsCatchment

    'general attributes
    Public ID As String
    Public InUse As Boolean
    Public UnpavedModel As STOCHLIB.GeneralFunctions.enmRainfallRunoffModel
    Public PavedModel As STOCHLIB.GeneralFunctions.enmRainfallRunoffModel
    Public GreenhouseModel As STOCHLIB.GeneralFunctions.enmRainfallRunoffModel
    Public RainfallRunoffConnection As STOCHLIB.GeneralFunctions.enmRainfallRunoffConnection  'this can either be an rr openwater node or an rr-on-flow connection
    Public MakeDummy As Boolean
    Public RecalcStorage As Boolean                                 'if true, the routine wil rebuild the catchment storage tables from the groud up, based on the GIS-data provided
    Public RecalcSeepage As Boolean                                 'if true, the routine will rebuild the catchemnt seepage data from GIS
    Public FlowLimiterMMPD As Double                                'Optional. if zero, no flow limiter structure will be implemented
    Public MeasurementStation As clsSbkReachObject                  'if not nothing, the pumps in this catchment will be subject to an emergency stop if threshold is exceeded
    Public EmergencyThreshold As Double                             'emergency threshold level. If exceeded all pumps inside the catchment will be disabled
    Public BuiFile As clsBuiFile
    Public QTable As STOCHLIB.clsTimeTable                          'timetable for sum of discharges Values1 = SOBEK, Values2 = SOBEK CSO's, Values3 = Wageningenmodel
    Public Shapes As New List(Of MapWinGIS.Shape)                   'a collection of all shapes that make up this catchment
    Public TotalShape As MapWinGIS.Shape                            'a merged shape for this catchment
    Public ShapeIdx As Integer                                      'the index number of the shape containing this catchment
    Public CatchmentIdx As Integer                                  'used for meteo stations
    Friend Subcatchments As clsAreas                                 'inliggende peilgebieden of afwaterende eenheden

    'collections of specific inflow locaties inside the catchment
    Public RRInflowLocations As clsRRInflowLocations                'collection of all RR and lateral Inflow locations inside the catchment
    Public RRCSOLocations As clsRRInflowLocations                   'collection of all RR and lateral CSO inflow locations inside the catchment

    'v1.798: introducing a list of 1d backbone reaches that intersect our subcatchment
    Public BackboneReaches As New List(Of String)

    'collection of unit RR nodes
    Public RRUnitUnpavedObjects As Dictionary(Of String, clsRRUnitUnpavedObject)
    Public RRUnitPavedObjects As Dictionary(Of String, clsRRUnitPavedObject)
    Public RRUnitSacramentoObjects As Dictionary(Of String, clsRRUnitSacramentoObject)
    Public RRUnitGreenhouseObjects As Dictionary(Of String, clsRRUnitGreenhouseObject)

    'collections of rainfall runoff model objects
    Public RRNodeTPRecords As Dictionary(Of String, clsRRNodeTPRecord)      'a list of all RR nodes, just by their node.tp record
    Public RRUnpavedNodes As clsRRUnpavedNodes
    Public RRPavedNodes As clsRRPavedNodes

    'associated simulation models
    Public WagModID As String                                       'identifier for the wageningenmodel
    Public UseSobek As Boolean                                      'whether tis catchment is linked to a SOBEK-model or not

    'references to external objects
    Friend CSOMeteoStation As clsMeteoStation                       'reference to a meteo station that contains the combined sewer overflows for the entire catchment

    'surface areas
    Private AreaGIS As Double                                        'totaaloppervlak van alle inliggende shapes
    Public ModelAreas As clsModelAreas
    Private AreaCSOCorrectionFactor As Double                           'correctiefactor voor het oppervlak van afwaterende eenheden in het geval er rioleringsgebieden in het spel zijn. Het verharde oppervlak daarvan telt namelijk niet mee in de verdeling van de inkomende drainagefluxen en oppervlakkige afstroming

    'references to higher order classes
    Private SbkCase As clsSobekCase
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, Optional ByRef mySbkCase As clsSobekCase = Nothing)
        Me.Setup = mySetup
        SbkCase = mySbkCase
        Subcatchments = New clsAreas(Me.Setup)
        BuiFile = New clsBuiFile(Me.Setup)
        QTable = New STOCHLIB.clsTimeTable(Me.Setup)
        RRInflowLocations = New clsRRInflowLocations(Me.Setup)
        RRCSOLocations = New clsRRInflowLocations(Me.Setup)

        RRNodeTPRecords = New Dictionary(Of String, clsRRNodeTPRecord)
        RRUnpavedNodes = New clsRRUnpavedNodes(Me.Setup)
        RRPavedNodes = New clsRRPavedNodes(Me.Setup)
        ModelAreas = New clsModelAreas(Me.Setup)
    End Sub

    Public Sub setSobekCase(ByRef myCase As clsSobekCase)
        SbkCase = myCase
    End Sub

    Public Function PopulateRRUnitObjects(Optional ByRef FreeboardClasses As Dictionary(Of Double, clsFreeboardClass) = Nothing) As Boolean
        Try
            PopulateRRPavedUnitObjects()                                           'paved nodes have a different status since we'll create just one per catchment
            PopulateRRUnpavedUnitObjects(FreeboardClasses)                         'here we'll write a collection to each catchment
            PopulateRRSacramentoUnitObjects()                                      'here we'll create a collection to each catchment for the sacramento nodes
            PopulateRRGreenhouseUnitObjects()                                      'here we'll create a collection to each catchment for the greenhouse nodes
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



    Public Function getExtent() As MapWinGIS.Extents
        Return TotalShape.Extents
    End Function

    Public Function PopulateBackboneReachList(BackboneChannelShapefile As String, ReachIDFieldIdx As Integer, ByRef CatchmentShapefile As MapWinGIS.Shapefile, CatchmentShapeIdx As Integer) As Boolean
        Try
            'new functionality in v1.798: creates a list of all 1D reaches from the backbone schematization that intersect our subcatchment
            'clip the channels shapefile by the extent of our total shape
            Dim ChannelSF As New MapWinGIS.Shapefile
            ChannelSF.Open(BackboneChannelShapefile)

            'deselect all shapes from the subcatchments shapefile
            CatchmentShapefile.SelectNone()

            'select the current subcatchment
            CatchmentShapefile.ShapeSelected(CatchmentShapeIdx) = True

            'use the selected shape to clip the channels shapefile
            Dim ChannelsClipped As New MapWinGIS.Shapefile
            ChannelsClipped = ChannelSF.Clip(False, Setup.GISData.CatchmentDataSource.Shapefile.sf, True)

            Dim ReachID As String
            'v1.890: this routine caused a crash in situations where no channels were found. Hence the if not nothing
            If Not ChannelsClipped Is Nothing Then
                For i = 0 To ChannelsClipped.NumShapes - 1
                    ReachID = ChannelsClipped.CellValue(ReachIDFieldIdx, i)
                    If Not BackboneReaches.Contains(ReachID.Trim.ToUpper) Then BackboneReaches.Add(ReachID.Trim.ToUpper)
                Next
            End If

            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function PopulateBackboneReaches of class clsCatchment.")
            Return False
        End Try
    End Function

    Public Function PopulateRRPavedUnitObjects() As Boolean
        Try
            'this routine builds a collection of RR Paved unit objects: one per catchment (for e.g. RTC Tools)
            Dim PV3B As clsPaved3BRecord, PVSTO As clsPavedSTORecord, PVDWA As clsPavedDWARecord
            Dim BNDNode As clsRRNodeTPRecord, myID As String = ""
            RRUnitPavedObjects = New Dictionary(Of String, clsRRUnitPavedObject)

            'walk through all subcatchments and classify each RR node
            For Each Node As clsRRNodeTPRecord In RRNodeTPRecords.Values

                Select Case Node.nt.ParentID
                    Case Is = "3B_PAVED"

                        PV3B = SbkCase.RRData.Paved3B.Records.Item(Node.ID.Trim.ToUpper)
                        PVSTO = SbkCase.RRData.PavedSTO.Records.Item(PV3B.sd.Trim.ToUpper)
                        PVDWA = SbkCase.RRData.PavedDWA.Records.Item(PV3B.dw.Trim.ToUpper)
                        BNDNode = SbkCase.RRTopo.getDownstreamNode(Node.ID)     'siebe: fix this later, looking up BOTH links

                        'read the .3b file and get the ss token (sewage system). Inside this catchment we'll create 3 types of sewage systems: mixed, separated and improved separated
                        Select Case PV3B.ss
                            Case Is = clsPaved3BRecord.enmSystemType.Mixed
                                myID = ID & "_pv_mix"
                            Case Is = clsPaved3BRecord.enmSystemType.Separated
                                myID = ID & "_pv_sep"
                            Case Is = clsPaved3BRecord.enmSystemType.ImprovedSeparated
                                myID = ID & "_pv_imp"
                        End Select

                        Dim myUnitObject As clsRRUnitPavedObject
                        If RRUnitPavedObjects.ContainsKey(myID.Trim.ToUpper) Then
                            myUnitObject = RRUnitPavedObjects.Item(myID.Trim.ToUpper)
                        Else
                            myUnitObject = New clsRRUnitPavedObject(Me.Setup, Me.SbkCase)
                            myUnitObject.ID = myID
                            myUnitObject.CatchmentID = ID
                            RRUnitPavedObjects.Add(myID.Trim.ToUpper, myUnitObject)
                        End If
                        If Not myUnitObject.RRNodeTpRecords.ContainsKey(Node.ID.Trim.ToUpper) Then
                            myUnitObject.RRNodeTpRecords.Add(Node.ID.Trim.ToUpper, Node)
                        Else
                            Me.Setup.Log.AddWarning("Warning: multiple instances found for node with id " & Node.ID)
                        End If

                End Select

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function PopulateRRPavedObjects")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function PopulateRRGreenhouseUnitObjects() As Boolean
        Try
            'this routine builds a collection of RR Greenhouse unit objects: one per catchment (for e.g. RTC Tools)
            Dim GR3B As clsGreenhse3BRecord, GRRF As clsGreenHseRFRecord
            Dim BNDNode As clsRRNodeTPRecord, myID As String = ""
            RRUnitGreenhouseObjects = New Dictionary(Of String, clsRRUnitGreenhouseObject)

            'walk through all subcatchments and classify each RR node
            For Each Node As clsRRNodeTPRecord In RRNodeTPRecords.Values

                Select Case Node.nt.ParentID
                    Case Is = "3B_GREENHOUSE"

                        GR3B = SbkCase.RRData.Greenhse3B.Records.Item(Node.ID.Trim.ToUpper)
                        GRRF = SbkCase.RRData.GreenhseRF.Records.Item(GR3B.sd.Trim.ToUpper)
                        BNDNode = SbkCase.RRTopo.getDownstreamNode(Node.ID)     'siebe: fix this later, looking up BOTH links

                        myID = ID & "_gr"

                        Dim myUnitObject As clsRRUnitGreenhouseObject
                        If RRUnitGreenhouseObjects.ContainsKey(myID.Trim.ToUpper) Then
                            myUnitObject = RRUnitGreenhouseObjects.Item(myID.Trim.ToUpper)
                        Else
                            myUnitObject = New clsRRUnitGreenhouseObject(Me.Setup, Me.SbkCase)
                            myUnitObject.ID = myID
                            myUnitObject.CatchmentID = ID
                            RRUnitGreenhouseObjects.Add(myID.Trim.ToUpper, myUnitObject)
                        End If
                        If Not myUnitObject.RRNodeTpRecords.ContainsKey(Node.ID.Trim.ToUpper) Then
                            myUnitObject.RRNodeTpRecords.Add(Node.ID.Trim.ToUpper, Node)
                        Else
                            Me.Setup.Log.AddWarning("Warning: multiple instances found for node with id " & Node.ID)
                        End If

                End Select

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function PopulateRRGreenhouseObjects")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function PopulateRRSacramentoUnitObjects() As Boolean
        Try
            'this routine builds a collection of RR Unit objects (for e.g. RTC Tools)
            Dim SACR3BSACR As clsSACRMNTO3BSACRRecord
            Dim SACR3BCAPS As clsSACRMNTO3BCAPSRecord
            Dim SACR3BOPAR As clsSACRMNTO3BOPARRecord
            Dim SACR3BUNIH As clsSACRMNTO3BUNIHRecord
            Dim BNDNode As clsRRNodeTPRecord, myID As String

            RRUnitSacramentoObjects = New Dictionary(Of String, clsRRUnitSacramentoObject)

            'walk through all subcatchments and classify each sacramento node
            For Each Node As clsRRNodeTPRecord In RRNodeTPRecords.Values

                Select Case Node.nt.ParentID
                    Case Is = "3B_SACRAMENTO"

                        SACR3BSACR = SbkCase.RRData.Sacr3BSACR.Records.Item(Node.ID.Trim.ToUpper)
                        SACR3BCAPS = SbkCase.RRData.Sacr3BCAPS.Records.Item(SACR3BSACR.ca.Trim.ToUpper)
                        SACR3BOPAR = SbkCase.RRData.Sacr3BOPAR.Records.Item(SACR3BSACR.op.Trim.ToUpper)
                        SACR3BUNIH = SbkCase.RRData.Sacr3BUNIH.Records.Item(SACR3BSACR.uh.Trim.ToUpper)
                        BNDNode = SbkCase.RRTopo.getDownstreamNode(Node.ID)

                        Dim DownVal As Double = SbkCase.RRData.getBoundaryLevel(BNDNode.ID, STOCHLIB.GeneralFunctions.enmHydroMathOperation.AVG)
                        myID = ID & "_SACR"

                        'make sure every individual node also has this unit node ID assigned
                        Node.UnitNodeID = myID

                        Dim myUnitObject As clsRRUnitSacramentoObject = Nothing
                        If RRUnitSacramentoObjects.ContainsKey(myID.Trim.ToUpper) Then
                            myUnitObject = RRUnitSacramentoObjects.Item(myID.Trim.ToUpper)
                        Else
                            myUnitObject = New clsRRUnitSacramentoObject(Me.Setup, Me.SbkCase)
                            myUnitObject.ID = myID
                            myUnitObject.CatchmentID = ID
                            RRUnitSacramentoObjects.Add(myID.Trim.ToUpper, myUnitObject)
                        End If
                        If Not myUnitObject.RRNodeTpRecords.ContainsKey(Node.ID.Trim.ToUpper) Then
                            myUnitObject.RRNodeTpRecords.Add(Node.ID.Trim.ToUpper, Node)
                        Else
                            Me.Setup.Log.AddWarning("Warning: multiple instances found for node with id " & Node.ID)
                        End If

                End Select

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function PopulateRRSacramentoObjects")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function PopulateRRUnpavedUnitObjects(ByRef FreeboardClasses As Dictionary(Of Double, clsFreeboardClass)) As Boolean
        Try
            'this routine builds a collection of RR Unit objects (for e.g. RTC Tools)
            Dim Up3B As clsUnpaved3BRecord, UpSep As clsUnpavedSEPRecord, UpAlf As clsUnpavedALFERNSRecord, UpInf As clsUnpavedINFRecord, UpSto As clsUnpavedSTORecord
            Dim myFB As clsFreeboardClass, BNDNode As clsRRNodeTPRecord, myID As String
            RRUnitUnpavedObjects = New Dictionary(Of String, clsRRUnitUnpavedObject)

            'walk through all subcatchments and classify each RR node
            For Each Node As clsRRNodeTPRecord In RRNodeTPRecords.Values

                Select Case Node.nt.ParentID
                    Case Is = "3B_UNPAVED"


                        Up3B = SbkCase.RRData.Unpaved3B.Records.Item(Node.ID.Trim.ToUpper)
                        UpSep = SbkCase.RRData.UnpavedSep.Records.Item(Up3B.SP.Trim.ToUpper)
                        UpInf = SbkCase.RRData.UnpavedInf.Records.Item(Up3B.ic.Trim.ToUpper)
                        UpAlf = SbkCase.RRData.UnpavedAlf.ERNSRecords.Item(Up3B.ed.Trim.ToUpper)
                        UpSto = SbkCase.RRData.UnpavedSto.Records.Item(Up3B.sd.Trim.ToUpper)

                        BNDNode = SbkCase.RRTopo.getDownstreamNode(Node.ID)

                        'disable the original objects 
                        Call SbkCase.RRTopo.DisableAllLinks(Node.ID)
                        Up3B.InUse = False

                        'get the freeboard value
                        Dim DownVal As Double = SbkCase.RRData.getBoundaryLevel(BNDNode.ID, STOCHLIB.GeneralFunctions.enmHydroMathOperation.AVG)
                        Dim Freeboard As Double = Up3B.lv - DownVal
                        myFB = Nothing
                        For Each myFBClass As clsFreeboardClass In FreeboardClasses.Values
                            If Freeboard <= myFBClass.ToValue Then
                                myFB = myFBClass
                                Exit For
                            End If
                        Next

                        'create a unique node id for this catchment + soil type + freeboard class
                        myID = ID & "_bt" & Up3B.bt & "_" & myFB.Label

                        'make sure every individual node also has this unit node ID assigned
                        Node.UnitNodeID = myID

                        Dim myUnitObject As clsRRUnitUnpavedObject = Nothing
                        If RRUnitUnpavedObjects.ContainsKey(myID.Trim.ToUpper) Then
                            myUnitObject = RRUnitUnpavedObjects.Item(myID.Trim.ToUpper)
                        Else
                            myUnitObject = New clsRRUnitUnpavedObject(Me.Setup, Me.SbkCase)
                            myUnitObject.ID = myID
                            myUnitObject.CatchmentID = ID
                            myUnitObject.bt = Up3B.bt
                            myUnitObject.FreeboardID = myFB.Label
                            RRUnitUnpavedObjects.Add(myID.Trim.ToUpper, myUnitObject)
                        End If
                        If Not myUnitObject.RRNodeTpRecords.ContainsKey(Node.ID.Trim.ToUpper) Then
                            myUnitObject.RRNodeTpRecords.Add(Node.ID.Trim.ToUpper, Node)
                        Else
                            Me.Setup.Log.AddWarning("Warning: multiple instances found for node with id " & Node.ID)
                        End If
                End Select

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function PopulateRRUnitObjects")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function UnitRRPavedAreasToExcelByNode(ScaleNodes As Boolean, UnitAreaM2 As Double, ByRef ws As clsExcelSheet, ByRef r As Integer) As Boolean
        Try
            Dim PV3B As clsPaved3BRecord
            For Each myUnit As clsRRUnitPavedObject In RRUnitPavedObjects.Values
                For Each myrecord As clsRRNodeTPRecord In myUnit.RRNodeTpRecords.Values
                    If myrecord.nt.ParentID = "3B_PAVED" Then
                        PV3B = SbkCase.RRData.Paved3B.GetRecord(myrecord.ID)
                        Dim downNode As clsRRNodeTPRecord = SbkCase.RRTopo.getDownstreamNode(myrecord.ID)
                        r += 1
                        ws.ws.Cells(r, 0).Value = myrecord.ID 'write the subcatchment id to the worksheet's first row
                        ws.ws.Cells(r, 1).Value = "paved"
                        ws.ws.Cells(r, 2).Value = myUnit.ID
                        ws.ws.Cells(r, 3).Value = myUnit.CatchmentID
                        ws.ws.Cells(r, 4).Value = myrecord.SubcatchmentID
                        ws.ws.Cells(r, 5).Value = PV3B.ar
                        ws.ws.Cells(r, 6).Value = PV3B.ar / UnitAreaM2
                        ws.ws.Cells(r, 7).Value = myrecord.ModelcatchmentID
                    End If
                Next
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function UnitRRPavedAreasToExcelByNode")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function UnitRRSacramentoAreasToExcelByNode(ScaleNodes As Boolean, UnitAreaM2 As Double, ByRef ws As clsExcelSheet, ByRef r As Integer) As Boolean
        Try
            Dim SA3B As clsSACRMNTO3BSACRRecord
            For Each myUnit As clsRRUnitSacramentoObject In RRUnitSacramentoObjects.Values
                For Each myrecord As clsRRNodeTPRecord In myUnit.RRNodeTpRecords.Values
                    If myrecord.nt.ParentID = "3B_SACRAMENTO" Then
                        SA3B = SbkCase.RRData.Sacr3BSACR.GetRecord(myrecord.ID)
                        Dim downNode As clsRRNodeTPRecord = SbkCase.RRTopo.getDownstreamNode(myrecord.ID)
                        r += 1
                        ws.ws.Cells(r, 0).Value = myrecord.ID 'write the subcatchment id to the worksheet's first row
                        ws.ws.Cells(r, 1).Value = "sacramento"
                        ws.ws.Cells(r, 2).Value = myUnit.ID
                        ws.ws.Cells(r, 3).Value = myUnit.CatchmentID
                        ws.ws.Cells(r, 4).Value = myrecord.SubcatchmentID
                        ws.ws.Cells(r, 5).Value = SA3B.ar
                        ws.ws.Cells(r, 6).Value = SA3B.ar / UnitAreaM2
                        ws.ws.Cells(r, 7).Value = myrecord.ModelcatchmentID
                    End If
                Next
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function UnitRRSacramentoAreasToExcelByNode")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function UnitRRGreenhouseAreasToExcelByNode(ScaleNodes As Boolean, UnitAreaM2 As Double, ByRef ws As clsExcelSheet, ByRef r As Integer) As Boolean
        Try
            Dim GR3B As clsGreenhse3BRecord

            For Each myUnit As clsRRUnitGreenhouseObject In RRUnitGreenhouseObjects.Values
                For Each myrecord As clsRRNodeTPRecord In myUnit.RRNodeTpRecords.Values
                    If myrecord.nt.ParentID = "3B_GREENHOUSE" Then
                        GR3B = SbkCase.RRData.Greenhse3B.GetRecord(myrecord.ID)
                        Dim downNode As clsRRNodeTPRecord = SbkCase.RRTopo.getDownstreamNode(myrecord.ID)
                        r += 1
                        ws.ws.Cells(r, 0).Value = myrecord.ID 'write the subcatchment id to the worksheet's first row
                        ws.ws.Cells(r, 1).Value = "greenhouse"
                        ws.ws.Cells(r, 2).Value = myUnit.ID
                        ws.ws.Cells(r, 3).Value = myUnit.CatchmentID
                        ws.ws.Cells(r, 4).Value = myrecord.SubcatchmentID
                        ws.ws.Cells(r, 5).Value = GR3B.getTotalArea
                        ws.ws.Cells(r, 6).Value = GR3B.getTotalArea / UnitAreaM2
                        ws.ws.Cells(r, 7).Value = myrecord.ModelcatchmentID
                    End If
                Next
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function UnitRRSacramentoAreasToExcelByNode")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function UnitRRGreenhouseAreasToExcelBySubcatchment(ScaleNodes As Boolean, UnitAreaM2 As Double, ByRef ws As clsExcelSheet, ByRef r As Integer) As Boolean
        Try
            'this function assigns the unit RR areas we have to the multitude of subcatchments inside the current catchment
            'the result of this will be written to Excel.
            Dim nNodes As Integer
            ws.ws.Cells(0, 0).Value = "Catchment ID"
            ws.ws.Cells(0, 1).Value = "Node ID"
            If ScaleNodes Then
                ws.ws.Cells(0, 2).Value = "Unit Area Multiplier"
            Else
                ws.ws.Cells(0, 2).Value = "Area"
            End If

            'walk through each unit object and write it's ID to the excel header
            'NOTE: the unit ID also contains the catchment ID
            For Each myUnit As clsRRUnitGreenhouseObject In RRUnitGreenhouseObjects.Values
                r += 1
                nNodes += myUnit.RRNodeTpRecords.Count
                ws.ws.Cells(r, 0).Value = ID
                ws.ws.Cells(r, 1).Value = myUnit.ID      'writes the unit node id to the header of the excel file
                If ScaleNodes Then
                    ws.ws.Cells(r, 2).Value = myUnit.GetTotalArea / UnitAreaM2
                Else
                    ws.ws.Cells(r, 2).Value = myUnit.GetTotalArea
                End If
            Next

            Me.Setup.Log.AddMessage("Total number of greehouse nodes processed in catchment " & ID & ":" & nNodes)

            r += 1
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function UnitRRGreenhouseAreasToExcel")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function

    Public Function ScaleRRUnitObjects(UnitAreaM2 As Double) As Boolean
        Try
            For Each myUnitObject As clsRRUnitUnpavedObject In RRUnitUnpavedObjects.Values
                myUnitObject.ScaleToUnitSize(UnitAreaM2)
            Next
            For Each myUnitObject As clsRRUnitPavedObject In RRUnitPavedObjects.Values
                myUnitObject.ScaleToUnitSize(UnitAreaM2)
            Next
            For Each myUnitObject As clsRRUnitSacramentoObject In RRUnitSacramentoObjects.Values
                myUnitObject.ScaleToUnitSize(UnitAreaM2)
            Next
            For Each myUnitObject As clsRRUnitGreenhouseObject In RRUnitGreenhouseObjects.Values
                myUnitObject.ScaleToUnitSize(UnitAreaM2)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ScaleRRUnitObjects")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function BuildRRUnitObjects(ByRef sbkCase As clsSobekCase) As Boolean
        Try
            For Each myUnitObject As clsRRUnitUnpavedObject In RRUnitUnpavedObjects.Values
                If Not myUnitObject.Build(sbkCase) Then Throw New Exception("Error building RR Unit Unpaved Objects for catchment " & ID)
            Next
            For Each myUnitObject As clsRRUnitPavedObject In RRUnitPavedObjects.Values
                If Not myUnitObject.Build(sbkCase) Then Throw New Exception("Error building RR Unit Paved Objects for catchment " & ID)
            Next
            For Each myUnitObject As clsRRUnitSacramentoObject In RRUnitSacramentoObjects.Values
                If Not myUnitObject.Build(sbkCase) Then Throw New Exception("Error building RR Unit Sacramento Objects for catchment " & ID)
            Next
            For Each myUnitObject As clsRRUnitGreenhouseObject In RRUnitGreenhouseObjects.Values
                If Not myUnitObject.Build(sbkCase) Then Throw New Exception("Error building RR Unit Greenhouse Objects for catchment " & ID)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function getRRPavedCSOArea(ByVal rrcfCsoPrefix As String) As Double
        'This routine finds all RR-paved nodes that represent 
        'a dedicated Combined Sewage Overflow (CSO) area
        'and sums up the areas per catchment
        'these areas are recognized by the prefix of their downstream node id
        Dim myNode As clsRRNodeTPRecord, downNode As clsRRNodeTPRecord, myLink As clsRRBrchTPRecord
        Dim myUtils As New MapWinGIS.Utils, myPoint As New MapWinGIS.Point
        Dim PV3B As clsPaved3BRecord, myArea As Double = 0

        For Each myNode In SbkCase.RRTopo.Nodes.Values
            myPoint.x = myNode.X
            myPoint.y = myNode.Y
            If myNode.nt.ID = "3B_PAVED" Then
                If myUtils.PointInPolygon(TotalShape, myPoint) Then
                    For Each myLink In SbkCase.RRTopo.Links.Values
                        If myLink.bn.Trim.ToUpper = myNode.ID.Trim.ToUpper Then
                            downNode = SbkCase.RRTopo.Nodes.Item(myLink.en.Trim.ToUpper)
                            If Not downNode Is Nothing AndAlso Left(downNode.ID.Trim.ToUpper, rrcfCsoPrefix.Length) = rrcfCsoPrefix.Trim.ToUpper Then
                                PV3B = SbkCase.RRData.Paved3B.GetRecord(myNode.ID.Trim.ToUpper)
                                If Not PV3B Is Nothing Then myArea += PV3B.ar
                            End If
                        End If
                    Next
                End If
            End If
        Next

        Return myArea

    End Function

    Public Function getRRPavedArea() As Double
        'This routine finds all RR-paved nodes inside the catchment
        'and sums up the areas
        Dim myNode As clsRRNodeTPRecord
        Dim myUtils As New MapWinGIS.Utils, myPoint As New MapWinGIS.Point
        Dim PV3B As clsPaved3BRecord, myArea As Double = 0

        For Each myNode In SbkCase.RRTopo.Nodes.Values
            myPoint.x = myNode.X
            myPoint.y = myNode.Y
            If myNode.nt.ID = "3B_PAVED" Then
                If myUtils.PointInPolygon(TotalShape, myPoint) Then
                    PV3B = SbkCase.RRData.Paved3B.GetRecord(myNode.ID.Trim.ToUpper)
                    If Not PV3B Is Nothing Then myArea += PV3B.ar
                End If
            End If
        Next

        Return myArea

    End Function

    Public Function getRRUnPavedArea() As Double

        Try
            'This routine finds all RR-unpaved nodes inside the catchment
            'and sums up the areas
            Dim myNode As clsRRNodeTPRecord
            Dim myUtils As New MapWinGIS.Utils, myPoint As New MapWinGIS.Point
            Dim UP3B As clsUnpaved3BRecord, myArea As Double = 0
            Dim i As Integer

            For Each myNode In SbkCase.RRTopo.Nodes.Values
                myPoint.x = myNode.X
                myPoint.y = myNode.Y
                If myNode.nt.ID = "3B_UNPAVED" Then
                    If myUtils.PointInPolygon(TotalShape, myPoint) Then
                        UP3B = SbkCase.RRData.Unpaved3B.GetRecord(myNode.ID.Trim.ToUpper)
                        If Not UP3B Is Nothing Then
                            For i = 1 To 16
                                myArea += UP3B.ar.Item(i)
                            Next
                        End If
                    End If
                End If
            Next

            Return myArea
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in sub getRRUnpavedArea for catchment " & ID & ". an unpaved area of 0 was returned.")
            Return 0
        End Try

    End Function


    Public Sub PopulateBuiFile()
        '--------------------------------------------------------------------------------------------------------------
        'date: 28 june 2013
        'author: Siebe Bosch
        'description: populates the .bui-file with discharge results per catchment
        'prerequisites: the .bui file must already contain meteo stations
        'note: since discharges per catchment are stored in m3/s, we need to convert tot mm/ts
        '--------------------------------------------------------------------------------------------------------------
        Dim ms As clsMeteoStation, msIdx As Integer, ts As Integer, myVal As Double
        Dim myRecord As clsTimeTableRecord
        Dim i As Long

        For msIdx = 0 To BuiFile.MeteoStations.MeteoStations.Values.Count - 1
            ms = BuiFile.MeteoStations.MeteoStations.Values(msIdx)
            ts = QTable.GetTimestepSeconds                        'gets the timestep in seconds from the catchment's QTable

            For i = 0 To QTable.Records.Count - 1
                myRecord = QTable.Records.Values(i)
                If ms.RepresentsCSO Then                                                  'this meteo station represents the Combined Sewer Overflow discharge
                    myVal = myRecord.GetValue(1)                                            'spilling discharge in m3/s
                    myVal = ts * Me.Setup.GeneralFunctions.m3ps2mmps(myVal, ModelAreas.RRPavedCSO)   'convert to mm/ts
                Else                                                                      'this meteo station represents regular drainage fluxes
                    'IMPORTANT: in this case do not convert volumes based on specified surface area that WagMod has been calibrated on
                    'since we already have the total m3/s for the catchment. 
                    'Now we'll have to start writing the results based on the actual areas! Therefore GISArea - RRCSOLocationsArea
                    myVal = myRecord.GetValue(0)                                  'drainage flux in m3/s
                    myVal = ms.Factor * ts * Me.Setup.GeneralFunctions.m3ps2mmps(myVal, AreaGIS - ModelAreas.RRPavedNonCSO) 'convert to mm/ts
                End If
                BuiFile.Values(i, msIdx) = myVal
            Next
        Next

    End Sub

    Public Sub SetMeteoStationsByArea(ByVal ApplyWeightedDischargeCoef As Boolean, ByVal AddCSOStation As Boolean)
        '--------------------------------------------------------------------------------------------------------------
        'datum: 11 mei 2012
        'laatst aangepast: 20 augustus 2013
        'auteur: Siebe Bosch
        'deze routine maakt een set meteostations aan op basis van de afvoerwegingsfactoren van elke afwateringseenheid
        '--------------------------------------------------------------------------------------------------------------
        Dim ms As New clsMeteoStation(Me.Setup)
        Dim myArea As clsSubcatchment

        'naast de stations met factoren, willen we een station met afvoerfactor 1 hebben
        'naamgeving van de neerslagstations wordt opgebouwd uit de eerste drie karakters van de naam + "_" + indexnummer (om uniciteit te garanderen)
        'plus "_QFACT_" en dan de oppervlaktegewogen afvoercoefficient
        ms = New clsMeteoStation(Me.Setup)
        ms.Factor = 1
        ms.CatchmentIdx = CatchmentIdx
        ms.ID = Left(ID, 3) & "_" & ms.CatchmentIdx.ToString.Trim & "_QFACT_1.0000"
        BuiFile.AddMeteoStation(ms)

        For Each myArea In Subcatchments.Subcatchments.Values
            If Not ApplyWeightedDischargeCoef Then
                myArea.MeteoStation = BuiFile.MeteoStations.GetAdd(ms, ms.ID.Trim.ToUpper)
            Else
                ms = New clsMeteoStation(Me.Setup)
                ms.CatchmentIdx = CatchmentIdx
                ms.Factor = Math.Round(myArea.WeightedDischargeCoefFactor, 4)
                ms.ID = Left(ID, 3) & "_" & ms.CatchmentIdx.ToString.Trim & "_QFACT_" & Format(ms.Factor, "0.0000")
                myArea.MeteoStation = BuiFile.MeteoStations.GetAdd(ms, ms.ID.Trim.ToUpper)
            End If
        Next

        If AddCSOStation Then
            'a separate rainfall station for all Combined Sewage Overflows
            ms = New clsMeteoStation(Me.Setup)
            ms.Factor = 1
            ms.CatchmentIdx = CatchmentIdx
            ms.RepresentsCSO = True
            ms.ID = Left(ID, 3) & "_" & ms.CatchmentIdx.ToString.Trim & "_CSO"
            BuiFile.MeteoStations.GetAdd(ms, ms.ID.Trim.ToUpper)
            'also set the reference to this CSO meteo station in the catchment itself
            CSOMeteoStation = ms
        End If

    End Sub


    Public Sub collectInflowFromSobek(ByRef SbkCase As clsSobekCase, Optional ByVal CSOCollection As Boolean = False)

        'Author: Siebe Bosch
        'Date: 4 july 2013
        'aggregeert voor het onderhavige stroomgebied alle SOBEK-RR-fluxen
        'bereid eerst de hisfile voor op uitlezen en verzamel dan de resultaten in een sobektimetable per catchment
        'binnen deze klasse krijgen catchments altijd neerslag in mm/h toegewezen. Dit vereenvoudigt later het maken van .bui-files
        Dim myIDs As New Dictionary(Of String, String), myID As String
        Dim i As Long = 0, FieldIdx As Integer

        If CSOCollection Then
            FieldIdx = 1
        Else
            FieldIdx = 0
        End If

        Me.Setup.GeneralFunctions.UpdateProgressBar("Collecting total SOBEK discharge for catchment " & ID & ".", 0, 10)
        If System.IO.File.Exists(SbkCase.CaseDir & "\bndflodt.his") Then
            Call SbkCase.CFData.Results.BndFlodtBinary.OpenFile()
            Dim myHeader As clsHisFileBinaryReader.stHisFileHeader = SbkCase.CFData.Results.BndFlodtBinary.GetHisFileHeader
            i = 0

            If CSOCollection Then
                myIDs = RRCSOLocations.getRRCFIds
            Else
                myIDs = RRInflowLocations.getRRCFIds
            End If

            For Each myID In myIDs.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, myIDs.Count, True)
                Call SbkCase.CFData.Results.BndFlodtBinary.ReadAddLocationResults(myID, myHeader.parameters.Item(0), QTable, FieldIdx)
            Next
            Call SbkCase.CFData.Results.BndFlodtBinary.Close()
            Call SbkCase.CFData.Results.BndFlodtBinary.Dispose()

        ElseIf System.IO.File.Exists(SbkCase.CaseDir & "\qlat.his") Then
            Call SbkCase.CFData.Results.QLatBinary.OpenFile()
            Dim myHeader As clsHisFileBinaryReader.stHisFileHeader = SbkCase.CFData.Results.QLatBinary.GetHisFileHeader
            Me.Setup.GeneralFunctions.UpdateProgressBar("Collecting total discharge for catchment " & ID, 0, 10)
            i = 0

            If CSOCollection Then
                myIDs = RRCSOLocations.getRRCFIds
            Else
                myIDs = RRInflowLocations.getRRCFIds
            End If

            For Each myID In myIDs.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, myIDs.Count, True)
                Call SbkCase.CFData.Results.QLatBinary.ReadAddLocationResults(myID, myHeader.parameters.Item(0), QTable, FieldIdx)
            Next
            Call SbkCase.CFData.Results.QLatBinary.Close()
            Call SbkCase.CFData.Results.QLatBinary.Dispose()
        End If

    End Sub

End Class
