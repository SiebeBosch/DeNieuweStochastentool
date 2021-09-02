Option Explicit On
Imports System.IO
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports GemBox.Spreadsheet

Public Class clsCFData
    Public Data As clsCFAttributeData
    Public Results As clsCFResults
    Friend Stats As clsCFStats

    'derived values
    Friend TotalVolumeUnderSurfaceLevel As Double

    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        Data = New clsCFAttributeData(Me.Setup, Me.SbkCase)
        Results = New clsCFResults(Me.Setup, Me.SbkCase)
        Stats = New clsCFStats(Me.Setup, Me.SbkCase)
    End Sub

    Public Function GetNodesDatRecord(id As String) As clsNodesDatNODERecord
        If Data.NodesData.NodesDatNodeRecords.records.ContainsKey(id.Trim.ToUpper) Then
            Return Data.NodesData.NodesDatNodeRecords.records.Item(id.Trim.ToUpper)
        End If
        Return Nothing
    End Function

    Public Sub Check()
        'performs a check on the flow data and write results to the logfile
        Data.ProfileData.CheckTypesOnReach(Data.NodesData)
    End Sub

    Public Function CreateSyntheticTidalBoundary(BoundaryID As String, LowTide As Double, HighTide As Double) As Boolean
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Creating synthetic tidal boundary...", 0, 10, True)
            If Not Data.BoundaryData.BoundaryDatFLBORecords.records.ContainsKey(BoundaryID.Trim.ToUpper) Then Throw New Exception("Error: no boundary record exists for boundary " & BoundaryID)
            Dim BoundRecord As clsBoundaryDatFLBORecord = Data.BoundaryData.BoundaryDatFLBORecords.records.Item(BoundaryID.Trim.ToUpper)
            Dim i As Integer
            BoundRecord.st = 0
            BoundRecord.ty = 0
            BoundRecord.h_wt = 1
            BoundRecord.h_wt1 = 0
            BoundRecord.h_wt11 = 0
            BoundRecord.HWTTable = New clsSobekTable(Me.Setup)
            BoundRecord.HWTTable.pdin1 = 0
            BoundRecord.HWTTable.pdin2 = 1
            BoundRecord.HWTTable.PDINPeriod = "0;12:25:00"

            Dim StartDate As New DateTime(2000, 1, 1, 0, 0, 0)
            Dim CurDate As DateTime = StartDate
            Dim CurVal As Double
            Dim Amplitude As Double = HighTide - LowTide
            Dim MeanLevel As Double = (HighTide + LowTide) / 2
            For i = 0 To 74 'this represents 12 hours and 20 minutes
                CurVal = Me.Setup.GeneralFunctions.GENERATE_TIDAL_SINUS(HighTide - LowTide, StartDate, MeanLevel, CurDate)
                BoundRecord.HWTTable.AddDatevalPair(CurDate, CurVal)
                CurDate = CurDate.AddMinutes(10)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Sub WriteLateralDat(path As String)
        Using latWriter As New StreamWriter(path)
            Data.LateralData.Write(latWriter)
        End Using
    End Sub

    Public Sub ImportByFile(ByVal ProfDat As Boolean, ByVal ProfDef As Boolean, ByVal StrucDat As Boolean, ByVal StrucDef As Boolean, ByVal ControlDef As Boolean, ByVal BoundDat As Boolean, ByVal LatDat As Boolean, ByVal BoundLat As Boolean, ByVal FrictionDat As Boolean, ByVal InitDat As Boolean, ByVal NodesDat As Boolean)
        Data.ImportByFile(ProfDat, ProfDef, StrucDat, StrucDef, ControlDef, BoundDat, LatDat, BoundLat, FrictionDat, InitDat, NodesDat)
    End Sub

    Public Sub AddPrefix(ByVal Prefix As String)
        Data.AddPrefix(Prefix)
    End Sub

    Public Function getLateralDatFLBRRecord(ID As String) As clsLateralDatFLBRRecord
        If Data.LateralData.LateralDatFLBRRecords.records.ContainsKey(ID.Trim.ToUpper) Then
            Return Data.LateralData.LateralDatFLBRRecords.records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Public Function getLateralDatFLNORecord(ID As String) As clsLateralDatFLNORecord
        If Data.LateralData.LateraldatFLNORecords.records.ContainsKey(ID.Trim.ToUpper) Then
            Return Data.LateralData.LateraldatFLNORecords.records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function


    Public Function ExportCSV(ByVal ExportDir As String) As Boolean
        Try
            Data.BoundaryData.ExportCSV(ExportDir & "\boundaries.csv")
            Data.StructureData.ExportCSV(ExportDir)
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function WeirTimeControllerFromDatatable(ID As String, ByRef dt As DataTable, DateColIdx As Integer, ValColidx As Integer) As Boolean
        Try
            Dim StrucDat As clsStructDatRecord = Nothing, StrucDef As clsStructDefRecord = Nothing, ControlDef As clsControlDefRecord = Nothing
            Dim myObj As clsSbkReachObject = SbkCase.CFTopo.GetReachObject(ID)
            If myObj Is Nothing Then Throw New Exception("Error: no reach object found with ID " & ID & ".")
            If Not myObj.nt = enmNodetype.NodeCFWeir Then Throw New Exception("Reach object with ID " & ID & " must be of rectangular weir type.")
            If Not Data.StructureData.GetStructureRecords(ID, StrucDat, StrucDef, ControlDef) Then Throw New Exception("Error: could not retrieve strucure data records for weir " & ID)

            StrucDat.ca = 1 'set the controller to active
            If StrucDat.cj = "" Then
                'no existing control record so create one
                ControlDef = New clsControlDefRecord(Me.Setup)
                ControlDef.ID = myObj.ID
                ControlDef.nm = myObj.ID
                ControlDef.ct = 0
                ControlDef.ac = 1   'controller active
                ControlDef.cf = 1   'control frequency
                ControlDef.InUse = True
                Data.StructureData.ControlDefRecords.Records.Add(ControlDef.ID.Trim.ToUpper, ControlDef)
                StrucDat.cj = ControlDef.ID
            End If

            ControlDef.ct = 0 'time controller
            ControlDef.mc = 0 'maximum change velocity
            ControlDef.bl = 1 'linear interpolation
            ControlDef.titv = True
            For r = 0 To dt.Rows.Count - 1
                ControlDef.TimeTable.AddDatevalPair(dt.Rows(r)(DateColIdx), dt.Rows(r)(ValColidx))
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function WeirTimeControllerFromTimeSeries(ID As String, ByRef TimeSeries As clsTimeSeries) As Boolean
        Try
            Dim StrucDat As clsStructDatRecord = Nothing, StrucDef As clsStructDefRecord = Nothing, ControlDef As clsControlDefRecord = Nothing
            Dim myObj As clsSbkReachObject = SbkCase.CFTopo.GetReachObject(ID)
            If myObj Is Nothing Then Throw New Exception("Error: no reach object found with ID " & ID & ".")
            If Not myObj.nt = enmNodetype.NodeCFWeir Then Throw New Exception("Reach object with ID " & ID & " must be of rectangular weir type.")
            If Not Data.StructureData.GetStructureRecords(ID, StrucDat, StrucDef, ControlDef) Then Throw New Exception("Error: could not retrieve strucure data records for weir " & ID)

            StrucDat.ca = 1 'set the controller to active
            If StrucDat.cj = "" Then
                'no existing control record so create one
                ControlDef = New clsControlDefRecord(Me.Setup)
                ControlDef.ID = myObj.ID
                ControlDef.nm = myObj.ID
                ControlDef.ct = 0
                ControlDef.ac = 1   'controller active
                ControlDef.cf = 1   'control frequency
                ControlDef.InUse = True
                Data.StructureData.ControlDefRecords.Records.Add(ControlDef.ID.Trim.ToUpper, ControlDef)
                StrucDat.cj = ControlDef.ID
            End If

            ControlDef.ct = 0 'time controller
            ControlDef.mc = 0 'maximum change velocity
            ControlDef.bl = 1 'linear interpolation
            ControlDef.titv = True
            For r = 0 To TimeSeries.Records.Count - 1
                ControlDef.TimeTable.AddDatevalPair(TimeSeries.Records(r).iDateTime, TimeSeries.Records(r).Value)
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function BorderDividingVerdict(ByRef Obj As clsSbkReachObject, ByVal UpShpIdx As Long, ByVal DnShpIdx As Long, ByRef Rating As Integer, ByRef Comment As String) As Boolean
        'this function calculates how well a given structure is capable of maintaning a target level along the border of an area (Peilscheidend)
        'first check the type of structure

        'get the target levels
        Dim upTL As New clsTargetLevels
        Dim dnTL As New clsTargetLevels
        Setup.GISData.SubcatchmentDataSource.getTargetLevels(UpShpIdx, upTL)
        Setup.GISData.SubcatchmentDataSource.getTargetLevels(DnShpIdx, dnTL)

        'decide which level the object needs to be able to control
        Dim MaxTL As Double = Math.Max(upTL.getZPOutlet, dnTL.getWPOutlet)
        Dim StrucDat As clsStructDatRecord = Nothing, StrucDef As clsStructDefRecord = Nothing, ContrDef As clsControlDefRecord = Nothing

        Data.StructureData.GetStructureRecords(Obj.ID, StrucDat, StrucDef, ContrDef)

        Select Case Obj.nt
            Case Is = enmNodetype.NodeCFPump
                Rating = 10
                Comment = "PUMPING STATION PRESENT"
                Return True
            Case Is = enmNodetype.NodeCFOrifice
                If StrucDef.cl >= MaxTL Then
                    Rating = 10
                    Comment = "ORIFICE CREST > MAX TARGET LEVEL"
                ElseIf StrucDef.gh = 0 AndAlso ContrDef.ca = 0 Then
                    Rating = 10
                    Comment = "ORIFICE GATE HEIGHT = 0, NO CONTROLLER"
                Else
                    Rating = 6
                    Comment = "ORIFICE CREST < MAX TARGET LEVEL, CONTROLLER ACTIVE"
                End If
            Case Is = enmNodetype.NodeCFCulvert
                If StrucDef.rl >= MaxTL OrElse StrucDef.ll >= MaxTL Then
                    Rating = 10
                    Comment = "CULVERT INVERT >= MAX TARGET LEVEL"
                ElseIf StrucDat.ca = 1 Then
                    Rating = 6
                    Comment = "CULVERT INVERT < MAX TARGET LEVEL, CONTROLLER ACTIVE"
                End If
            Case Is = enmNodetype.NodeCFBridge
                Rating = 0
                Comment = "BRIDGE"
            Case Is = enmNodetype.NodeCFUniWeir

            Case Is = enmNodetype.NodeCFWeir
                If StrucDef.cl >= MaxTL AndAlso ContrDef.ca = 0 Then
                    Rating = 10
                    Comment = "WEIR CREST LEVEL > MAX TARGET LEVEL, NO CONTROLLER"
                ElseIf ContrDef.ca = 1 Then
                    Rating = 6
                    Comment = "WEIR CONTROLLER ACTIVE"
                End If
            Case Else

        End Select
    End Function

    Public Function CalculateReachObjectStorageTable(ByRef myObj As clsSbkReachObject, ByRef myReach As clsSbkReach, RuralChannels As Boolean, UrbanChannels As Boolean, UrbanStreets As Boolean, RuralNodes As Boolean, UrbanNodes As Boolean, ByRef upProf As clsSbkReachObject, ByRef dnProf As clsSbkReachObject, ByVal distToUpProf As Double, ByVal distToDnProf As Double, ByVal Length As Double, ByVal LowestZ As Double, ByVal VerticalStepSize As Double, ByVal nElevations As Integer, ByRef AreaTable As Double()) As Boolean
        Try
            Dim upDat As clsProfileDatRecord = Nothing, upDef As clsProfileDefRecord = Nothing
            Dim dnDat As clsProfileDatRecord = Nothing, dnDef As clsProfileDefRecord = Nothing
            Dim nDat As clsNodesDatNODERecord = Nothing
            Dim upWidth As Double, dnWidth As Double, curWidth As Double

            If Not upProf Is Nothing Then If Not Data.ProfileData.getProfileRecords(upProf.ID, upDat, upDef) Then Throw New Exception("Error retrieving upstream cross section data for object " & myObj.ID)
            If Not dnProf Is Nothing Then If Not Data.ProfileData.getProfileRecords(dnProf.ID, dnDat, dnDef) Then Throw New Exception("Error retrieving downstream cross section data for object " & myObj.ID)

            'also add the storage of the node itself
            If (myObj.IsUrbanType AndAlso UrbanNodes) Then
                'nodes of either urban type can contain well storage and street storage
                If Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myObj.ID.Trim.ToUpper) Then
                    nDat = Data.NodesData.NodesDatNodeRecords.records.Item(myObj.ID.Trim.ToUpper)
                Else
                    nDat = Nothing
                End If
            ElseIf myObj.nt = enmNodetype.ConnNodeLatStor AndAlso RuralNodes Then
                'nodes of rural type Connection Node With Lateral lDischarge and Storage can contain well storage and street storage
                If Data.NodesData.NodesDatNodeRecords.records.ContainsKey(myObj.ID.Trim.ToUpper) Then
                    nDat = Data.NodesData.NodesDatNodeRecords.records.Item(myObj.ID.Trim.ToUpper)
                Else
                    nDat = Nothing
                End If
            End If

            Dim ProcessReach As Boolean = False
            If myReach.IsPipe AndAlso UrbanChannels Then
                ProcessReach = True
            ElseIf myReach.IsChannel AndAlso RuralChannels Then
                ProcessReach = True
            ElseIf myReach.IsStreet AndAlso UrbanStreets Then
                ProcessReach = True
            End If

            Dim z As Double = LowestZ, i As Integer
            For i = 0 To nElevations - 1
                'compute the storage in channels and/or pipes
                If ProcessReach Then
                    'determine the cross section width of the upstream and downstream cross sections and interpolate to the current location
                    If Not upDef Is Nothing Then upWidth = upDef.getProfileWidthAtLevel(z - upDat.rl)
                    If Not dnDef Is Nothing Then dnWidth = dnDef.getProfileWidthAtLevel(z - dnDat.rl)
                    If upDef Is Nothing AndAlso Not dnDef Is Nothing Then
                        'read the downstream profile width
                        curWidth = dnWidth
                    ElseIf dnDef Is Nothing AndAlso Not upDef Is Nothing Then
                        'read the upstream profile width
                        curWidth = upWidth
                    Else
                        curWidth = Me.Setup.GeneralFunctions.Interpolate(0, upWidth, distToUpProf + distToDnProf, dnWidth, distToUpProf)
                    End If
                    AreaTable(i) = curWidth * Length
                End If
                'compute the storage in connection nodes and/or manholes
                If Not nDat Is Nothing Then
                    AreaTable(i) += nDat.CalcStorageArea(z)
                End If
                z += VerticalStepSize
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function exportInfoworksICM(FolderPath As String) As Boolean
        Try
            'let op: voorafgaand aan deze functie moeten alle takken zijn opgeknipt zodat ze slechts één profiel bevatten
            Dim ProfDat As clsProfileDatRecord = Nothing, ProfDef As clsProfileDefRecord = Nothing
            Dim StrucDat As clsStructDatRecord = Nothing, StrucDef As clsStructDefRecord = Nothing, ContrDef As clsControlDefRecord = Nothing
            Dim FricDat As clsFrictionDatCRFRRecord = Nothing, BoundDat As clsBoundaryDatFLBORecord = Nothing
            Dim ShapeStr As String = "", BoundTypeStr As String = "", BoundVal As Double, Height As Double, Width As Double, myObj As clsSbkReachObject = Nothing
            Dim ProfileFound As Boolean = False
            Dim cIDx As Integer, cBNx As Integer, cENx As Integer, cLx As Integer, cSx As Integer, cWx As Integer, cHx As Integer, cUx As Integer, cDx As Integer
            Dim chIdx As Integer, chBNIdx As Integer, chENIdx As Integer, chTYIdx As Integer, chShIDIdx As Integer, chOrigShIDIdx As Integer, chUSIdx As Integer, chDSIdx As Integer, chGRIdx As Integer, chLEIdx As Integer
            Dim nodeIDdx As Integer, nodexIdx As Integer, nodeyIdx As Integer, nodezIdx As Integer
            Dim boundIDx As Integer, boundxIdx As Integer, boundyIdx As Integer, boundTypeIdx As Integer, boundValIdx As Integer
            Dim oIdx As Integer, oBNx As Integer, oENx As Integer, oCWx As Integer, oCLx As Integer, oGHx As Integer
            Dim wIdx As Integer, wBNx As Integer, wENx As Integer, wCWx As Integer, wCLx As Integer
            Dim pIdx As Integer, pBNx As Integer, pENx As Integer, pCAPx As Integer, pONx As Integer, pOFFx As Integer
            Dim myReach As clsSbkReach

            'create a shapefile for the channels
            Dim ChannelSF As New MapWinGIS.Shapefile, cSFPath As String = FolderPath & "\channels.shp"
            Me.Setup.GeneralFunctions.deleteShapeFile(cSFPath)
            ChannelSF.CreateNew(cSFPath, MapWinGIS.ShpfileType.SHP_POLYLINE)
            ChannelSF.StartEditingShapes(True)
            chIdx = ChannelSF.EditAddField("id", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            chBNIdx = ChannelSF.EditAddField("bn", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            chENIdx = ChannelSF.EditAddField("en", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            chTYIdx = ChannelSF.EditAddField("link_type", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            chShIDIdx = ChannelSF.EditAddField("shape_id", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            chOrigShIDIdx = ChannelSF.EditAddField("origshp_id", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            chUSIdx = ChannelSF.EditAddField("us_invert", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            chDSIdx = ChannelSF.EditAddField("ds_invert", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            chLEIdx = ChannelSF.EditAddField("length", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            chGRIdx = ChannelSF.EditAddField("gradient", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)

            'create a shapefile for the conduits
            Dim ConduitSF As New MapWinGIS.Shapefile, SFPath As String = FolderPath & "\conduits.shp"
            Me.Setup.GeneralFunctions.deleteShapeFile(SFPath)
            ConduitSF.CreateNew(SFPath, MapWinGIS.ShpfileType.SHP_POLYLINE)
            ConduitSF.StartEditingShapes(True)
            cIDx = ConduitSF.EditAddField("asset_id", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            cBNx = ConduitSF.EditAddField("bn", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            cENx = ConduitSF.EditAddField("en", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            cLx = ConduitSF.EditAddField("length", MapWinGIS.FieldType.DOUBLE_FIELD, 5, 10)
            cSx = ConduitSF.EditAddField("shape", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            cWx = ConduitSF.EditAddField("width", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            cHx = ConduitSF.EditAddField("height", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            cUx = ConduitSF.EditAddField("invert_up", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            cDx = ConduitSF.EditAddField("invert_dn", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)

            'create a shapefile for the orifices
            Dim OrificeSF As New MapWinGIS.Shapefile, OrificeSFPath As String = FolderPath & "\orifices.shp"
            Me.Setup.GeneralFunctions.deleteShapeFile(OrificeSFPath)
            OrificeSF.CreateNew(OrificeSFPath, MapWinGIS.ShpfileType.SHP_POLYLINE)
            OrificeSF.StartEditingShapes(True)
            oIdx = OrificeSF.EditAddField("asset_id", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            oBNx = OrificeSF.EditAddField("bn", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            oENx = OrificeSF.EditAddField("en", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            oCWx = OrificeSF.EditAddField("crestwidth", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            oCLx = OrificeSF.EditAddField("crestlevel", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            oGHx = OrificeSF.EditAddField("gateheight", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)

            'create a shapefile for the weirs
            Dim WeirSF As New MapWinGIS.Shapefile, WeirSFPath As String = FolderPath & "\weirs.shp"
            Me.Setup.GeneralFunctions.deleteShapeFile(WeirSFPath)
            WeirSF.CreateNew(WeirSFPath, MapWinGIS.ShpfileType.SHP_POLYLINE)
            WeirSF.StartEditingShapes(True)
            wIdx = WeirSF.EditAddField("asset_id", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            wBNx = WeirSF.EditAddField("bn", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            wENx = WeirSF.EditAddField("en", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            wCWx = WeirSF.EditAddField("crestwidth", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            wCLx = WeirSF.EditAddField("crestlevel", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)

            'create a shapefile for the pumps
            Dim PumpSF As New MapWinGIS.Shapefile, PumpSFPath As String = FolderPath & "\pumps.shp"
            Me.Setup.GeneralFunctions.deleteShapeFile(PumpSFPath)
            PumpSF.CreateNew(PumpSFPath, MapWinGIS.ShpfileType.SHP_POLYLINE)
            PumpSF.StartEditingShapes(True)
            pIdx = PumpSF.EditAddField("asset_id", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            pBNx = PumpSF.EditAddField("bn", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            pENx = PumpSF.EditAddField("en", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            pCAPx = PumpSF.EditAddField("cap", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            pONx = PumpSF.EditAddField("on", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            pOFFx = PumpSF.EditAddField("off", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)

            'create a shapefile for the nodes
            Dim NodeSF As New MapWinGIS.Shapefile, NodeSFPath As String = FolderPath & "\nodes.shp"
            Me.Setup.GeneralFunctions.deleteShapeFile(NodeSFPath)
            NodeSF.CreateNew(NodeSFPath, MapWinGIS.ShpfileType.SHP_POINT)
            NodeSF.StartEditingShapes(True)
            nodeIDdx = NodeSF.EditAddField("asset_id", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            nodexIdx = NodeSF.EditAddField("x", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            nodeyIdx = NodeSF.EditAddField("y", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            nodezIdx = NodeSF.EditAddField("z", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)

            'create a shapefile for the boundaries
            Dim BoundSF As New MapWinGIS.Shapefile, BoundSFPath As String = FolderPath & "\boundaries.shp"
            Me.Setup.GeneralFunctions.deleteShapeFile(BoundSFPath)
            BoundSF.CreateNew(BoundSFPath, MapWinGIS.ShpfileType.SHP_POINT)
            BoundSF.StartEditingShapes(True)
            boundIDx = BoundSF.EditAddField("asset_id", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            boundxIdx = BoundSF.EditAddField("x", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            boundyIdx = BoundSF.EditAddField("y", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)
            boundTypeIdx = BoundSF.EditAddField("type", MapWinGIS.FieldType.STRING_FIELD, 0, 30)
            boundValIdx = BoundSF.EditAddField("value", MapWinGIS.FieldType.DOUBLE_FIELD, 10, 12)

            'write the connection nodes and boundaries
            For Each myNode As clsSbkReachNode In SbkCase.CFTopo.ReachNodes.ReachNodes.Values
                If myNode.nt = enmNodetype.NodeCFBoundary Then
                    BoundDat = Data.BoundaryData.BoundaryDatFLBORecords.records.Item(myNode.ID.Trim.ToUpper)
                    If BoundDat.ty = 0 Then
                        BoundTypeStr = "H"
                        BoundVal = BoundDat.h_wd
                    Else
                        BoundTypeStr = "Q"
                        BoundVal = BoundDat.q_dw1
                    End If

                    'convert the node into a mapwingis shape and add it to the boundaries shapefile
                    Dim myShape As MapWinGIS.Shape = myNode.exportAsShape()
                    Dim ShpIdx As Integer = BoundSF.EditAddShape(myShape)
                    BoundSF.EditCellValue(nodeIDdx, ShpIdx, myNode.ID)
                    BoundSF.EditCellValue(nodexIdx, ShpIdx, myNode.X)
                    BoundSF.EditCellValue(nodeyIdx, ShpIdx, myNode.Y)
                    BoundSF.EditCellValue(boundTypeIdx, ShpIdx, BoundTypeStr)
                    BoundSF.EditCellValue(boundValIdx, ShpIdx, BoundVal)
                    'boundWriter.WriteLine(myNode.ID & "," & myNode.X & "," & myNode.Y & "," & BoundTypeStr & "," & BoundVal)
                Else
                    'convert the node into a mapwingis shape and add it to the nodes shapefile
                    Dim myShape As MapWinGIS.Shape = myNode.exportAsShape()
                    Dim ShpIdx As Integer = NodeSF.EditAddShape(myShape)
                    NodeSF.EditCellValue(nodeIDdx, ShpIdx, myNode.ID)
                    NodeSF.EditCellValue(nodexIdx, ShpIdx, myNode.X)
                    NodeSF.EditCellValue(nodeyIdx, ShpIdx, myNode.Y)
                    NodeSF.EditCellValue(nodezIdx, ShpIdx, 0)
                    'nodeWriter.WriteLine(myNode.ID & "," & myNode.X & "," & myNode.Y & "," & "0")
                End If
            Next

            'write all cross sections
            Using profileWriter As New StreamWriter(FolderPath & "\CrossSections.csv")
                profileWriter.WriteLine("shape_id,dist,value")
                For Each myReach In Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Values
                    If myReach.InUse AndAlso Not myReach.ProfDef Is Nothing Then
                        If myReach.ProfDef.ty = enmProfileType.yztable Then
                            For i = 0 To myReach.ProfDef.ltyzTable.XValues.Count - 1
                                'notice that all cross sections get the ID of the reach they represent
                                profileWriter.WriteLine(myReach.Id & "," & myReach.ProfDef.ltyzTable.XValues.Values(i) & "," & myReach.ProfDef.ltyzTable.Values1.Values(i))
                            Next
                        End If
                    End If
                Next
            End Using

            'write the initial data (if present)
            Using initWriter As New StreamWriter(FolderPath & "\Initial.csv")
                initWriter.WriteLine("NodeID,Type,Value")
                Dim DatRecord As clsInitialDatFLINRecord
                Dim nNotFound As Integer = 0

                For Each myNode As clsSbkReachNode In Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.ReachNodes.ReachNodes.Values
                    If myNode.InUse Then

                        'find a reach for which this node is the upstream node. 
                        myReach = myNode.getFirstDownstreamReach
                        If myReach Is Nothing Then myReach = myNode.getFirstUpstreamReach
                        If Not myReach Is Nothing Then
                            If Data.InitialData.InitialDatFLINRecords.records.ContainsKey(myReach.Id.Trim.ToUpper) Then
                                DatRecord = Data.InitialData.InitialDatFLINRecords.records.Item(myReach.Id.Trim.ToUpper)
                                Select Case DatRecord.ty
                                    Case Is = 0
                                        'water depth
                                        initWriter.WriteLine(myNode.ID & "," & "Depth," & DatRecord.lv_ll2)
                                    Case Is = 1
                                        'water level
                                        initWriter.WriteLine(myNode.ID & "," & "Waterlevel," & DatRecord.lv_ll2)
                                End Select
                            Else
                                nNotFound += 1
                            End If
                        End If

                    End If
                Next
                If nNotFound > 0 Then Me.Setup.Log.AddWarning("Could not find initialization value for " & nNotFound & " reaches.")
            End Using



            'write all reaches & structurereaches
            For Each myReach In SbkCase.CFTopo.Reaches.Reaches.Values
                If myReach.InUse Then

                    'convert this reach to a Mapwingis Shape for export
                    Dim myShape As MapWinGIS.Shape = myReach.exportToShape()

                    If myReach.ContainsObject(enmNodetype.NodeCFCulvert, myObj) Then
                        Data.StructureData.GetStructureRecords(myObj.ID, StrucDat, StrucDef, ContrDef)
                        If Data.ProfileData.ProfileDefRecords.Records.ContainsKey(StrucDef.si.Trim.ToUpper) Then
                            ProfDef = Data.ProfileData.ProfileDefRecords.Records.Item(StrucDef.si.Trim.ToUpper)
                            Select Case ProfDef.ty
                                Case Is = 0 'rectangular OR tabulated
                                    If Left(ProfDef.nm, 2) = "r_" Then
                                        ShapeStr = "RECT"                   'rectangular
                                        Height = (ProfDef.ltlwTable.XValues.Values(1) - ProfDef.ltyzTable.XValues.Values(0)) * 1000
                                        Width = ProfDef.ltlwTable.Values1.Values(1) * 1000
                                    Else
                                        Me.Setup.Log.AddError("Export Of tabulated profiles Not yet supported. Culvert ID = " & myObj.ID)
                                    End If
                                Case Is = 4 'closed circle
                                    ShapeStr = "CIRC"
                                    Height = ProfDef.rd * 1000 * 2
                                    Width = Height
                                Case Else
                                    Me.Setup.Log.AddError("Profile shape could Not be identified For culvert " & myObj.ID)
                                    ShapeStr = "CIRC"
                            End Select

                            'add the shape to the conduits shapefile
                            Dim ShpIdx As Integer = ConduitSF.EditAddShape(myShape)
                            ConduitSF.EditCellValue(cIDx, ShpIdx, myObj.ID)
                            ConduitSF.EditCellValue(cBNx, ShpIdx, myReach.bn.ID)
                            ConduitSF.EditCellValue(cENx, ShpIdx, myReach.en.ID)
                            ConduitSF.EditCellValue(cLx, ShpIdx, StrucDef.dl)
                            ConduitSF.EditCellValue(cSx, ShpIdx, ShapeStr)
                            ConduitSF.EditCellValue(cWx, ShpIdx, Width)
                            ConduitSF.EditCellValue(cHx, ShpIdx, Height)
                            ConduitSF.EditCellValue(cUx, ShpIdx, StrucDef.rl)
                            ConduitSF.EditCellValue(cDx, ShpIdx, StrucDef.ll)
                        Else
                            Me.Setup.Log.AddError("Error: profile definition not found for culvert " & myObj.ID)
                        End If
                    ElseIf myReach.ContainsObject(enmNodetype.NodeCFWeir, myObj) Then
                        Data.StructureData.GetStructureRecords(myObj.ID, StrucDat, StrucDef, ContrDef)

                        'add the shape to the orifice shapefile
                        Dim ShpIdx As Integer = WeirSF.EditAddShape(myShape)
                        WeirSF.EditCellValue(wIdx, ShpIdx, myObj.ID)
                        WeirSF.EditCellValue(wBNx, ShpIdx, myReach.bn.ID)
                        WeirSF.EditCellValue(wENx, ShpIdx, myReach.en.ID)
                        WeirSF.EditCellValue(wCWx, ShpIdx, StrucDef.cw)
                        WeirSF.EditCellValue(wCLx, ShpIdx, StrucDef.cl)
                        'weirWriter.WriteLine(myObj.ID & "," & myReach.bn.ID & "," & myReach.en.ID & "," & StrucDef.cw & "," & StrucDef.cl)
                    ElseIf myReach.ContainsObject(enmNodetype.NodeCFPump, myObj) Then
                        Data.StructureData.GetStructureRecords(myObj.ID, StrucDat, StrucDef, ContrDef)
                        If StrucDef.dn = 1 Then 'only outlet pumps! We will skip inlet pumps
                            'add the shape to the pumps shapefile
                            Dim ShpIdx As Integer = PumpSF.EditAddShape(myShape)
                            PumpSF.EditCellValue(pIdx, ShpIdx, myObj.ID)
                            PumpSF.EditCellValue(pBNx, ShpIdx, myReach.bn.ID)
                            PumpSF.EditCellValue(pENx, ShpIdx, myReach.en.ID)
                            PumpSF.EditCellValue(pCAPx, ShpIdx, StrucDef.ctltTable.XValues.Values(0))
                            PumpSF.EditCellValue(pONx, ShpIdx, StrucDef.ctltTable.Values1.Values(0))
                            PumpSF.EditCellValue(pOFFx, ShpIdx, StrucDef.ctltTable.Values2.Values(0))
                            'pumpWriter.WriteLine(myObj.ID & "," & myReach.bn.ID & "," & myReach.en.ID & "," & StrucDef.ctltTable.XValues.Values(0) & "," & StrucDef.ctltTable.Values1.Values(0) & "," & StrucDef.ctltTable.Values2.Values(0))
                        End If
                    ElseIf myReach.ContainsObject(enmNodetype.NodeCFOrifice, myObj) Then
                        Data.StructureData.GetStructureRecords(myObj.ID, StrucDat, StrucDef, ContrDef)

                        'add the shape to the orifice shapefile
                        Dim ShpIdx As Integer = OrificeSF.EditAddShape(myShape)
                        OrificeSF.EditCellValue(oIdx, ShpIdx, myObj.ID)
                        OrificeSF.EditCellValue(oBNx, ShpIdx, myReach.bn.ID)
                        OrificeSF.EditCellValue(oENx, ShpIdx, myReach.en.ID)
                        OrificeSF.EditCellValue(oCWx, ShpIdx, StrucDef.cw)
                        OrificeSF.EditCellValue(oCLx, ShpIdx, StrucDef.cl)
                        OrificeSF.EditCellValue(oGHx, ShpIdx, StrucDef.gh - StrucDef.cl)
                    Else
                        'no supported structure found on this reach, so export it as a channel
                        Dim ShpIdx As Integer = ChannelSF.EditAddShape(myShape)

                        If Not myReach.ProfDef Is Nothing Then
                            'notice that each channel gets its own cross section definition that has the same ID as the channel itself
                            ChannelSF.EditCellValue(chIdx, ShpIdx, myReach.Id)
                            ChannelSF.EditCellValue(chBNIdx, ShpIdx, myReach.bn.ID)
                            ChannelSF.EditCellValue(chENIdx, ShpIdx, myReach.en.ID)
                            ChannelSF.EditCellValue(chTYIdx, ShpIdx, "CHANNEL")
                            ChannelSF.EditCellValue(chShIDIdx, ShpIdx, myReach.Id)
                            ChannelSF.EditCellValue(chOrigShIDIdx, ShpIdx, myReach.ProfDef.ID)
                            ChannelSF.EditCellValue(chUSIdx, ShpIdx, myReach.upshift)
                            ChannelSF.EditCellValue(chDSIdx, ShpIdx, myReach.dnshift)
                            ChannelSF.EditCellValue(chLEIdx, ShpIdx, myReach.getReachLength)
                            'channelWriter.WriteLine(myReach.Id & "," & myReach.bn.ID & "," & myReach.en.ID & "," & "CHANNEL" & "," & myReach.Id & "," & myReach.ProfDef.ID & "," & myReach.upshift & "," & myReach.dnshift & "," & myReach.getReachLength)
                        Else
                            ChannelSF.EditCellValue(chIdx, ShpIdx, myReach.Id)
                            ChannelSF.EditCellValue(chBNIdx, ShpIdx, myReach.bn.ID)
                            ChannelSF.EditCellValue(chENIdx, ShpIdx, myReach.en.ID)
                            ChannelSF.EditCellValue(chTYIdx, ShpIdx, "CHANNEL")
                            ChannelSF.EditCellValue(chLEIdx, ShpIdx, myReach.getReachLength)
                            'channelWriter.WriteLine(myReach.Id & "," & myReach.bn.ID & "," & myReach.en.ID & "," & "CHANNEL" & "," & "" & "," & "" & "," & 0 & "," & 0 & "," & myReach.getReachLength)
                        End If
                    End If
                End If
            Next

            NodeSF.StopEditingShapes(True, True)
            NodeSF.Save()

            BoundSF.StopEditingShapes(True, True)
            BoundSF.Save()

            OrificeSF.StopEditingShapes(True, True)
            OrificeSF.Save()

            PumpSF.StopEditingShapes(True, True)
            PumpSF.Save()

            WeirSF.StopEditingShapes(True, True)
            WeirSF.Save()

            ChannelSF.StopEditingShapes(True, True)
            ChannelSF.Save()

            ConduitSF.StopEditingShapes(True, True)
            ConduitSF.Save()


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in sub exportInfoworksICM.")
            Return False
        End Try
    End Function

    Public Function ClearInitialDatRecords() As Boolean
        Try
            Data.InitialData.InitialDatFLINRecords.records.Clear()
            Data.InitialData.GlobalRecord = Nothing
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function makeGlobalInitialDatRecord(InitType As GeneralFunctions.enmInitialType, InitVal As Double) As Boolean
        Try
            Dim myDat As New clsInitialDatFLINRecord(Me.Setup)
            myDat.ID = "-1"
            myDat.ci = "-1"
            Select Case InitType
                Case Is = GeneralFunctions.enmInitialType.DEPTH
                    myDat.ty = 0
                Case Is = GeneralFunctions.enmInitialType.WATERLEVEL
                    myDat.ty = 1
            End Select
            myDat.lv_ll1 = 0
            myDat.lv_ll2 = InitVal
            Data.InitialData.GlobalRecord = myDat
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error In Function makeGlobalinitialDatRecord Of Class clsCFData.")
            Return False
        End Try
    End Function

    Public Function disableReachObjects(NodeType As enmNodetype) As Boolean
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = NodeType Then myObj.InUse = False
            Next
        Next
    End Function

    Public Function ProfileWidthByRecordNumberToExcel(Tabulated As Boolean, RecordNumber As Integer) As Boolean
        Try
            Dim myDat As clsProfileDatRecord, myDef As clsProfileDefRecord
            Dim ws As clsExcelSheet = Setup.ExcelFile.GetAddSheet("Width by elevation record " & RecordNumber)
            Dim r As Integer = 0, i As Long, n As Long = Data.ProfileData.ProfileDatRecords.Records.Count

            ws.ws.Cells(r, 0).Value = "ID"
            ws.ws.Cells(r, 1).Value = "Width (m)"

            Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting cross section width...", 0, 10)

            For Each myDat In Data.ProfileData.ProfileDatRecords.Records.Values
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                If Data.ProfileData.ProfileDefRecords.Records.ContainsKey(myDat.di.Trim.ToUpper) Then
                    myDef = Data.ProfileData.ProfileDefRecords.Records.Item(myDat.di.Trim.ToUpper)
                    If Tabulated AndAlso myDef.ty = enmProfileType.tabulated Then
                        r += 1
                        If myDef.ltlwTable.XValues.Count >= RecordNumber Then
                            ws.ws.Cells(r, 0).Value = myDat.ID
                            ws.ws.Cells(r, 1).Value = myDef.ltlwTable.Values1.Values(RecordNumber - 1)
                        Else
                            ws.ws.Cells(r, 0).Value = myDat.ID
                            ws.ws.Cells(r, 1).Value = "NAN"
                        End If
                    End If
                End If
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function SetProfileElevationFromGrid(ByVal SearchRadius As Double) As Boolean

        'first, create a shapefile with lines perpendicular to the reach direction
        Me.Setup.GISData.PerpendicularShapeFile.CreateNew(Me.Setup.Settings.ExportDirRoot & "\loodlijnen.shp")
        Me.Setup.GISData.PerpendicularShapeFile.buildAccrossFromCalcPoints(Me.SbkCase, SearchRadius)

        'next calculate the elevation data
        Data.ProfileData.ProfileDatRecords.setElevationFromGrid()


    End Function

    Public Sub ExportProfileData(ByVal Append As Boolean, ExportDir As String)
        Data.ProfileData.Export(Append, ExportDir)
    End Sub

    Public Sub ExportFrictionData(ByVal append As Boolean, ExportDir As String)
        Data.FrictionData.Export(append, ExportDir)
    End Sub
    Public Sub ExportNodesData(ByVal append As Boolean, ExportDir As String)
        Data.NodesData.Export(append, ExportDir)
    End Sub
    Public Sub ExportBoundaryData(ByVal append As Boolean, ExportDir As String)
        Data.BoundaryData.Export(append, ExportDir)
    End Sub

    Public Sub ExportAll(ByVal Append As Boolean, ExportDir As String)
        Data.BoundaryData.Export(Append, ExportDir)
        Data.FrictionData.Export(Append, ExportDir)
        Data.LateralData.Export(Append, ExportDir)
        Data.NodesData.Export(Append, ExportDir)
        Data.ProfileData.Export(Append, ExportDir)
        Data.StructureData.Export(Append, ExportDir)
    End Sub

    Public Sub ExportLateralData(ByVal Append As Boolean, ExportDir As String)
        Data.LateralData.Export(Append, ExportDir)
    End Sub

    ''' <summary>
    ''' This routine cleans up model objects and data for the flow module.
    ''' It will remove any unused data records
    ''' </summary>
    ''' <remarks></remarks>
    Friend Function CleanUp(ByVal ProfDat As Boolean, ByVal ProfDef As Boolean, ByVal StructDat As Boolean, ByVal StructDef As Boolean, ByVal ControlDef As Boolean, ByVal BoundDat As Boolean, ByVal LatDat As Boolean, ByVal FrictDat As Boolean, ByVal NodesDat As Boolean) As Boolean
        Try
            Dim myObj As clsSbkReachObject
            Dim myReach As clsSbkReach
            Dim profDatRecord As clsProfileDatRecord, structDatRecord As clsStructDatRecord, controlDefRecord As clsControlDefRecord
            Dim profDefRecord As clsProfileDefRecord, structDefRecord As clsStructDefRecord
            Dim FLBORecord As clsBoundaryDatFLBORecord, BDFRRecord As clsFrictionDatBDFRRecord
            Dim CRFRRecord As clsFrictionDatCRFRRecord, STFRRecord As clsFrictionDatSTFRRecord
            Dim FLBRRecord As clsLateralDatFLBRRecord, FLNORecord As clsLateralDatFLNORecord, BTBLRecord As clsBoundlatDatBTBLRecord
            Dim NODERecord As clsNodesDatNODERecord

            Dim i As Integer

            'profile.dat opschonen
            If ProfDat Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Cleaning up profile.dat...", 0, 10, True)
                For i = 0 To Data.ProfileData.ProfileDatRecords.Records.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.ProfileData.ProfileDatRecords.Records.Count - 1)
                    profDatRecord = Data.ProfileData.ProfileDatRecords.Records.Values(i)

                    'zoek de tak waarop dit record wordt gebruikt
                    myObj = SbkCase.CFTopo.getReachObject(profDatRecord.ID.Trim.ToUpper, False)

                    If myObj Is Nothing Then
                        profDatRecord.InUse = False
                        Me.Setup.Log.AddMessage("Cross Section record " & profDatRecord.ID & " was not in use and has been removed.")
                    Else
                        myReach = SbkCase.CFTopo.Reaches.GetReach(myObj.ci.Trim.ToUpper)
                        If myReach Is Nothing Then
                            Me.Setup.Log.AddMessage("No reach found for cross section " & myObj.ID)
                        ElseIf myReach.InUse = False Then
                            profDatRecord.InUse = False
                            Me.Setup.Log.AddMessage("Reach " & myReach.Id & " was not in use; therefore the cross section " & profDatRecord.ID & ", located on that reach has been removed as well.")
                        End If
                    End If
                Next
            End If

            'struct.dat opschonen
            If StructDat Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Cleaning up struct.dat...", 0, 10, True)
                For i = 0 To Data.StructureData.StructDatRecords.Records.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.StructureData.StructDatRecords.Records.Count - 1)
                    structDatRecord = Data.StructureData.StructDatRecords.Records.Values(i)
                    'zoek de tak waarop dit record wordt gebruikt
                    myObj = SbkCase.CFTopo.getReachObject(structDatRecord.ID.Trim.ToUpper, False)
                    If myObj Is Nothing Then
                        structDatRecord.InUse = False
                        Me.Setup.Log.AddMessage("Structure record " & structDatRecord.ID & " was not in use and has been removed.")
                    Else
                        myReach = SbkCase.CFTopo.Reaches.GetReach(myObj.ci.Trim.ToUpper)
                        If myReach Is Nothing Then
                            Me.Setup.Log.AddMessage("No reach found for structure " & myObj.ID)
                        ElseIf myReach.InUse = False Then
                            structDatRecord.InUse = False
                            Me.Setup.Log.AddMessage("Reach " & myReach.Id & " was not in use; therefore the structure " & structDatRecord.ID & ", located on that reach has been removed as well.")
                        End If
                    End If
                Next
            End If

            'struct.def opschonen
            If StructDef Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Cleaning up struct.def...", 0, 10, True)
                For i = 0 To Data.StructureData.StructDefRecords.Records.Values.Count - 1
                    structDefRecord = Data.StructureData.StructDefRecords.Records.Values(i)
                    Dim Found As Boolean = False

                    'zoek het struct.dat-record waarop naar dit object wordt verwezen
                    For Each datRecord As clsStructDatRecord In Data.StructureData.StructDatRecords.Records.Values
                        If datRecord.dd.Trim.ToUpper = structDefRecord.ID.Trim.ToUpper Then
                            Found = True
                            Exit For
                        End If
                    Next

                    If Not Found Then
                        structDefRecord.InUse = False
                        Me.Setup.Log.AddMessage("Structure definition " & structDefRecord.ID & " was not in use and has been removed.")
                    End If
                Next
            End If

            'profile.def opschonen
            If ProfDef Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Cleaning up profile.def...", 0, 10, True)
                For i = 0 To Data.ProfileData.ProfileDefRecords.Records.Values.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.ProfileData.ProfileDefRecords.Records.Values.Count - 1)
                    profDefRecord = Data.ProfileData.ProfileDefRecords.Records.Values(i)
                    profDefRecord.CleanUp()

                    Dim Found As Boolean = False

                    'zoek het profile.dat-record waarop naar dit object wordt verwezen
                    For Each datRecord As clsProfileDatRecord In Data.ProfileData.ProfileDatRecords.Records.Values
                        If datRecord.di.Trim.ToUpper = profDefRecord.ID.Trim.ToUpper Then
                            Found = True
                            Exit For
                        End If
                    Next

                    'zoek evt het struct.def-record waarop naar dit object wordt verwezen
                    For Each strdefRecord As clsStructDefRecord In Data.StructureData.StructDefRecords.Records.Values
                        If Not strdefRecord.si Is Nothing Then
                            If strdefRecord.si.Trim.ToUpper = profDefRecord.ID.Trim.ToUpper Then
                                Found = True
                                Exit For
                            End If
                        End If
                    Next

                    If Not Found Then
                        profDefRecord.InUse = False
                        Me.Setup.Log.AddMessage("Cross Section definition " & profDefRecord.ID & " was not in use and has been removed.")
                    End If
                Next
            End If

            'control.def opschonen
            If ControlDef Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Cleaning up control.def...", 0, 10, True)
                For i = 0 To Data.StructureData.ControlDefRecords.Records.Values.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.StructureData.ControlDefRecords.Records.Values.Count)
                    controlDefRecord = Data.StructureData.ControlDefRecords.Records.Values(i)
                    Dim Found As Boolean = False
                    For Each strdatRecord As clsStructDatRecord In Data.StructureData.StructDatRecords.Records.Values
                        If Not strdatRecord.cj Is Nothing Then
                            If strdatRecord.cj = controlDefRecord.ID Then
                                Found = True
                            End If
                        End If
                    Next

                    If Not Found Then
                        controlDefRecord.InUse = False
                        Me.Setup.Log.AddMessage("Controller definition " & controlDefRecord.ID & " was not in use and has been removed.")
                    End If
                Next
            End If

            'boundary.dat opschonen
            If BoundDat Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Cleaning up boundary.dat...", 0, 10, True)
                For i = 0 To Data.BoundaryData.BoundaryDatFLBORecords.records.Values.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.BoundaryData.BoundaryDatFLBORecords.records.Count)
                    FLBORecord = Data.BoundaryData.BoundaryDatFLBORecords.records.Values(i)
                    If Not SbkCase.CFTopo.ReachNodes.ReachNodes.ContainsKey(FLBORecord.ID.Trim.ToUpper) Then
                        FLBORecord.InUse = False
                        Me.Setup.Log.AddMessage("Boundary definition " & FLBORecord.ID & " was not in use and has been removed.")
                    End If
                Next
            End If

            'friction.dat opschonen
            If FrictDat Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Cleaning up friction.dat...", 0, 10, True)
                For i = 0 To Data.FrictionData.FrictionDatBDFRRecords.records.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.FrictionData.FrictionDatBDFRRecords.records.Count)
                    BDFRRecord = Data.FrictionData.FrictionDatBDFRRecords.records.Values(i)
                    If Not SbkCase.CFTopo.Reaches.Reaches.ContainsKey(BDFRRecord.ci.Trim.ToUpper) Then
                        BDFRRecord.InUse = False
                        Me.Setup.Log.AddMessage("Bed friction definition " & BDFRRecord.ID & " was not in use and has been removed.")
                    End If
                Next

                For i = 0 To Data.FrictionData.FrictionDatCRFRRecords.records.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.FrictionData.FrictionDatCRFRRecords.records.Count)
                    CRFRRecord = Data.FrictionData.FrictionDatCRFRRecords.records.Values(i)
                    If Not SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records.ContainsKey(CRFRRecord.cs.Trim.ToUpper) Then
                        CRFRRecord.InUse = False
                        Me.Setup.Log.AddMessage("Cross Section friction definition " & CRFRRecord.ID & " was not in use and has been removed.")
                    End If
                Next

                For i = 0 To Data.FrictionData.FrictionDatSTFRRecords.records.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.FrictionData.FrictionDatSTFRRecords.records.Count)
                    STFRRecord = Data.FrictionData.FrictionDatSTFRRecords.records.Values(i)
                    If Not SbkCase.CFData.Data.StructureData.StructDefRecords.Records.ContainsKey(STFRRecord.ci.Trim.ToUpper) Then
                        STFRRecord.InUse = False
                        Me.Setup.Log.AddMessage("Structure friction definition " & STFRRecord.ID & " was not in use and has been removed.")
                    End If
                Next

            End If

            'lateral.dat opschonen
            If LatDat Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Cleaning up lateral.dat...", 0, 10, True)
                For i = 0 To Data.LateralData.LateralDatFLBRRecords.records.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.LateralData.LateralDatFLBRRecords.records.Count)
                    FLBRRecord = Data.LateralData.LateralDatFLBRRecords.records.Values(i)
                    If SbkCase.CFTopo.getReachObject(FLBRRecord.ID.Trim.ToUpper, False) Is Nothing Then
                        FLBRRecord.InUse = False
                        Me.Setup.Log.AddMessage("Lateral definition " & FLBRRecord.ID & " was not in use and has been removed.")
                    Else
                        FLBRRecord.InUse = True
                    End If
                Next

                For i = 0 To Data.LateralData.LateraldatFLNORecords.records.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.LateralData.LateraldatFLNORecords.records.Count)
                    FLNORecord = Data.LateralData.LateraldatFLNORecords.records.Values(i)
                    If SbkCase.CFTopo.getReachNode(FLNORecord.ID.Trim.ToUpper) Is Nothing Then
                        FLNORecord.InUse = False
                        Me.Setup.Log.AddMessage("Lateral definition " & FLNORecord.ID & " was not in use and has been removed.")
                    Else
                        FLNORecord.InUse = True
                    End If
                Next

                For i = 0 To Data.LateralData.BoundlatDatBTBLRecords.records.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.LateralData.BoundlatDatBTBLRecords.records.Count)
                    BTBLRecord = Data.LateralData.BoundlatDatBTBLRecords.records.Values(i)
                    BTBLRecord.InUse = False
                    For Each FLBRRecord In Data.LateralData.LateralDatFLBRRecords.records.Values
                        If FLBRRecord.InUse AndAlso FLBRRecord.LibTableID.Trim.ToUpper = BTBLRecord.ID.Trim.ToUpper Then BTBLRecord.InUse = True
                    Next
                Next
            End If

            'nodes.dat opschonen
            If NodesDat Then
                Me.Setup.GeneralFunctions.UpdateProgressBar("Cleaning up nodes.dat...", 0, 10, True)
                For i = 0 To Data.NodesData.NodesDatNodeRecords.records.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Data.NodesData.NodesDatNodeRecords.records.Count)
                    NODERecord = Data.NodesData.NodesDatNodeRecords.records.Values(i)
                    If SbkCase.CFTopo.getReachNode(NODERecord.ID.Trim.ToUpper) Is Nothing Then
                        NODERecord.InUse = False
                        Me.Setup.Log.AddMessage("Node " & NODERecord.ID & " not in the network hence its node definition has been removed from nodes.dat.")
                    End If
                Next
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in Function CleanUp Of MyClass clsCFData.")
        End Try

    End Function

    Public Sub WriteBoundaryData(ByVal Append As Boolean, ExportDir As String)
        Call Data.BoundaryData.Export(Append, ExportDir)
    End Sub

    Public Sub WriteStructureData(ByVal Append As Boolean, ExportDir As String)
        Call Data.StructureData.Export(Append, ExportDir)
    End Sub

    Public Sub calcStats()
        'Berekent statistische kentallen van alle elementen in de flow-module van SOBEK

        Stats.nReaches = SbkCase.CFTopo.Reaches.Reaches.Values.Count
        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            Stats.TotalReachLength += myReach.getReachLength

            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                Select Case myObj.nt
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge
                        Stats.nBridges += 1
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFCulvert
                        Stats.nCulverts += 1
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral
                        Stats.nLaterals += 1
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.MeasurementStation
                        Stats.nMeas += 1
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice
                        Stats.nOrifices += 1
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE
                        Stats.nProfiles += 1
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump
                        Stats.nPumps += 1
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection
                        Stats.nRRCFConn += 1
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir
                        Stats.nUniWeirs += 1
                    Case Is = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir
                        Stats.nWeirs += 1
                End Select

            Next

        Next
    End Sub

    Public Function WeirDataFromShapeFile(ByVal AutoDetectPrefix As Boolean, ByVal VerticalShift As Double) As Boolean

        'Retrieves the target levels and crest width for weirs from a shapefile
        'and applies these values to the corresponding structures in the model schematisation
        'NOTE: ONLY APPLIES TO STRUCTURES WITHOUT A CONTROLLER DEFINITION

        Dim myShape As MapWinGIS.Shape
        Dim ID As String, WP As Double, ZP As Double, Width As Double
        Dim myDat As clsStructDatRecord, myDef As clsStructDefRecord, myCtr As clsControlDefRecord
        Dim ShapeIdx As Long

        Try
            'open the shapefile
            If Me.Setup.GISData.WeirDataSource.Open() Then
                'read all struct.dat records
                For Each myDat In Data.StructureData.StructDatRecords.Records.Values

                    'structure must have no controller
                    If myDat.ca = 0 Then

                        'get the corresponding structure definition
                        myDef = Data.StructureData.StructDefRecords.FindByID(myDat.dd.Trim.ToUpper)
                        If Not myDef Is Nothing AndAlso myDef.ty = 6 Then 'only weirs
                            myShape = Me.Setup.GISData.WeirDataSource.GetShapeByTextValue(enmInternalVariable.ID, myDat.ID, ShapeIdx)
                            If Not myShape Is Nothing Then
                                ID = Me.Setup.GISData.WeirDataSource.GetTextValue(ShapeIdx, enmInternalVariable.ID, enmMessageType.ErrorMessage)
                                ZP = Me.Setup.GISData.WeirDataSource.GetTextValue(ShapeIdx, enmInternalVariable.ZPHighSideOutlet, enmMessageType.ErrorMessage)
                                WP = Me.Setup.GISData.WeirDataSource.GetTextValue(ShapeIdx, enmInternalVariable.WPHighSideOutlet, enmMessageType.ErrorMessage)
                                Width = Me.Setup.GISData.WeirDataSource.GetTextValue(ShapeIdx, enmInternalVariable.CrestWidth, enmMessageType.ErrorMessage)
                                If Not Width <= 0 Then
                                    myDef.cw = Width
                                Else
                                    Me.Setup.Log.AddWarning("Crest width for weir " & myDat.ID & " was <= 0. Value was therefore not changed.")
                                End If

                                If Not ZP = 0 AndAlso Not WP = 0 AndAlso Not ZP <= -9 AndAlso Not WP <= -9 Then
                                    If ZP = WP Then
                                        myDef.cl = ZP + VerticalShift
                                    Else
                                        myCtr = New clsControlDefRecord(Me.Setup)
                                        myCtr.setAsTargetLevelController(myDat.ID, WP + VerticalShift, ZP + VerticalShift)
                                        myCtr.InUse = True
                                        myDat.cj = myDat.ID 'set control definition id for the structure
                                        myDat.ca = 1        'set controller to active
                                        Data.StructureData.ControlDefRecords.Records.Add(myCtr.ID.Trim.ToUpper, myCtr)
                                    End If
                                Else
                                    Me.Setup.Log.AddWarning("One or more crest levels for structure " & myDat.ID & " were equal to zero or < -9. Structure's crest was therefore not changed.")
                                End If
                            ElseIf myDef Is Nothing Then
                                Me.Setup.Log.AddWarning("No structure definition found for structure " & myDat.ID & ".")
                            End If
                        End If
                    End If
                Next
                Me.Setup.GISData.WeirDataSource.Close()
            Else
                Throw New Exception("Could not open Weirs datasource " & Me.Setup.GISData.WeirDataSource.getPrimaryDataSourcePath)
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function BuildFlowBoundaryDataFromGIS(ByVal Prefixes As List(Of String), ByVal AlwaysTable As Boolean) As Boolean
        Dim myIdx As Integer
        Dim WP As Double, ZP As Double, i As Integer = 0
        Dim n As Long = Me.SbkCase.CFTopo.Reaches.Reaches.Count

        Try
            Setup.GeneralFunctions.UpdateProgressBar("Building boundary data for flow module...", 0, 10, True)
            If Not Me.Setup.GISData.SubcatchmentDataSource.Open() Then Throw New Exception("Could not open shapefile.")
            For Each myReach As clsSbkReach In Me.SbkCase.CFTopo.Reaches.Reaches.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                If myReach.InUse Then
                    For Each Prefix In Prefixes
                        If Left(myReach.Id.Trim.ToUpper, Prefix.Trim.Length) = Prefix.Trim.ToUpper Then
                            If myReach.bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary Then
                                If Me.Setup.GISData.SubcatchmentDataSource.GetRecordIdxByCoord(myReach.bn.X, myReach.bn.Y, myIdx) Then
                                    Me.Setup.GISData.SubcatchmentDataSource.GetSummerOutletTargetLevel(myIdx, ZP)
                                    Me.Setup.GISData.SubcatchmentDataSource.GetWinterOutletTargetLevel(myIdx, WP)
                                    Call setBoundaryRecords(myReach.bn.ID, WP, ZP, AlwaysTable)
                                End If
                            End If
                            If myReach.en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBoundary Then
                                If Me.Setup.GISData.SubcatchmentDataSource.GetRecordIdxByCoord(myReach.en.X, myReach.en.Y, myIdx) Then
                                    Me.Setup.GISData.SubcatchmentDataSource.GetSummerOutletTargetLevel(myIdx, ZP)
                                    Me.Setup.GISData.SubcatchmentDataSource.GetWinterOutletTargetLevel(myIdx, WP)
                                    Call setBoundaryRecords(myReach.en.ID, WP, ZP, AlwaysTable)
                                End If
                            End If
                            Exit For
                        End If
                    Next
                End If
            Next
            Me.Setup.GISData.SubcatchmentDataSource.Close()

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function BuildFlowBoundaryData.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Sub setBoundaryRecords(ByVal ObjID As String, ByVal WP As Double, ByVal ZP As Double, ByVal AlwaysTable As Boolean)

        'FLBO id 'cKGM026' st 0 ty 0 h_ wt 1 0 0 PDIN 0 1 '365;00:00:00' pdin
        'TBLE
        ''1999/11/30;00:00:00' 1 < 
        ''2000/10/10;00:00:00' 2 < 
        ''2001/10/10;00:00:00' 1 < 
        ''2002/10/10;00:00:00' 2 < 
        'tble flbo

        'FLBO id 'cGPGKST0921_147' st 0 ty 0 h_ wd 0  -0.92 0 flbo

        Dim myDatRecord As clsBoundaryDatFLBORecord
        myDatRecord = Data.BoundaryData.BoundaryDatFLBORecords.getAddRecord(ObjID)
        myDatRecord.InUse = True

        If ZP <> WP OrElse AlwaysTable Then
            myDatRecord.ty = 0
            myDatRecord.h_wt1 = 1
            myDatRecord.HWTTable = New clsSobekTable(Me.Setup)
            myDatRecord.HWTTable.pdin1 = 0
            myDatRecord.HWTTable.pdin2 = 1
            myDatRecord.HWTTable.PDINPeriod = "'365;00:00:00'"
            myDatRecord.HWTTable.AddDatevalPair(New Date(2000, 1, 1), WP)
            myDatRecord.HWTTable.AddDatevalPair(New Date(2000, 4, 15), ZP)
            myDatRecord.HWTTable.AddDatevalPair(New Date(2000, 10, 15), WP)
        Else
            myDatRecord.st = 0
            myDatRecord.ty = 0
            myDatRecord.h_wd = 0
            myDatRecord.h_wd0 = WP
        End If
    End Sub

    Public Function ImplementThreeSectionRoughnessByDepth(IDFilter As String, Depth As Double, FrictionTypeLeftBank As GeneralFunctions.enmFrictionType, FrictionValueLeftBank As Double, FrictionTypeMain As GeneralFunctions.enmFrictionType, FrictionValueMain As Double, FrictionTypeRightBank As GeneralFunctions.enmFrictionType, FrictionValueRightBank As Double) As Boolean
        Try
            Dim ProfDef As clsProfileDefRecord
            Dim FricDat As clsFrictionDatCRFRRecord
            Dim iProf As Integer = 0, nProf As Integer = Data.ProfileData.ProfileDatRecords.Records.Count
            Dim FilterList As New List(Of String)
            While Not IDFilter = ""
                Dim myFilter As String = Setup.GeneralFunctions.ParseString(IDFilter, ";")
                If Not myFilter = "" Then FilterList.Add(myFilter)
            End While

            For Each ProfDat As clsProfileDatRecord In Data.ProfileData.ProfileDatRecords.Records.Values
                iProf += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iProf, nProf)
                If Me.Setup.GeneralFunctions.TextMatchFromExpressionsList(FilterList, ProfDat.ID, True) Then
                    If Data.ProfileData.ProfileDefRecords.Records.ContainsKey(ProfDat.di.Trim.ToUpper) Then
                        ProfDef = Data.ProfileData.ProfileDefRecords.Records.Item(ProfDat.di.Trim.ToUpper)
                        If ProfDef.ty = enmProfileType.yztable Then
                            FricDat = Data.FrictionData.getCRFRRecordByProfileDefinition(ProfDef.ID)

                            'clean up our existing friction record for this profile
                            If Not FricDat Is Nothing Then Data.FrictionData.FrictionDatCRFRRecords.records.Remove(FricDat.ID.Trim.ToUpper)

                            'first we need to make sure our cross section has a yz-value at the required depth
                            Dim Distances As Tuple(Of Double, Double)
                            Distances = ProfDef.addYZCoordinatesAtDepth(Depth)

                            FricDat = New clsFrictionDatCRFRRecord(Me.Setup)
                            FricDat.ID = ProfDef.ID
                            FricDat.nm = ProfDef.ID
                            FricDat.InUse = True
                            FricDat.cs = ProfDef.ID
                            FricDat.ltysTable = New clsSobekTable(Me.Setup)
                            FricDat.ftysTable = New clsSobekTable(Me.Setup)
                            FricDat.frysTable = New clsSobekTable(Me.Setup)

                            If Not Double.IsNaN(Distances.Item1) AndAlso Not Double.IsNaN(Distances.Item2) Then
                                'we found two valid section points. So go for the full description of two floodplains and one main sectin
                                FricDat.ltysTable.AddDataPair(2, ProfDef.ltyzTable.XValues.Values(0), Distances.Item1)
                                FricDat.ltysTable.AddDataPair(2, Distances.Item1, Distances.Item2)
                                FricDat.ltysTable.AddDataPair(2, Distances.Item2, ProfDef.ltyzTable.XValues.Values(ProfDef.ltyzTable.XValues.Count - 1))
                                FricDat.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeLeftBank), FrictionValueLeftBank,,,,,, True)
                                FricDat.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeMain), FrictionValueMain,,,,,, True)
                                FricDat.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeRightBank), FrictionValueRightBank,,,,,, True)
                                FricDat.frysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeLeftBank), FrictionValueLeftBank,,,,,, True)
                                FricDat.frysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeMain), FrictionValueMain,,,,,, True)
                                FricDat.frysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeRightBank), FrictionValueRightBank,,,,,, True)
                            ElseIf Not Double.IsNaN(Distances.Item1) Then
                                'we found only a left side valid point
                                FricDat.ltysTable.AddDataPair(2, ProfDef.ltyzTable.XValues.Values(0), Distances.Item1)
                                FricDat.ltysTable.AddDataPair(2, Distances.Item1, ProfDef.ltyzTable.XValues.Values(ProfDef.ltyzTable.XValues.Count - 1))
                                FricDat.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeLeftBank), FrictionValueLeftBank,,,,,, True)
                                FricDat.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeMain), FrictionValueMain,,,,,, True)
                                FricDat.frysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeLeftBank), FrictionValueLeftBank,,,,,, True)
                                FricDat.frysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeMain), FrictionValueMain,,,,,, True)
                            ElseIf Not Double.IsNaN(Distances.Item2) Then
                                'we found only a right side valid point
                                FricDat.ltysTable.AddDataPair(2, ProfDef.ltyzTable.XValues.Values(0), Distances.Item2)
                                FricDat.ltysTable.AddDataPair(2, Distances.Item2, ProfDef.ltyzTable.XValues.Values(ProfDef.ltyzTable.XValues.Count - 1))
                                FricDat.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeMain), FrictionValueMain,,,,,, True)
                                FricDat.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeRightBank), FrictionValueRightBank,,,,,, True)
                                FricDat.frysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeMain), FrictionValueMain,,,,,, True)
                                FricDat.frysTable.AddDataPair(2, Convert.ToInt32(FrictionTypeRightBank), FrictionValueRightBank,,,,,, True)
                            End If

                            'add the new fric.dat to the friction.dat records
                            Data.FrictionData.FrictionDatCRFRRecords.records.Add(FricDat.ID.Trim.ToUpper, FricDat)
                        End If
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ImplementThreeSectionRoughnessByDepth of class clsCFData: " & ex.Message)
            Return False
        End Try
    End Function
    Public Function MakeProfileDefinitionsUnique() As Boolean
        Try
            'Author: Siebe Bosch
            'Date: 8-9-2013
            'Description: makes the profile definitions for each cross section unique
            'This is useful in case local adjustments need to be done e.g. NVO's
            Dim ProfDat As clsProfileDatRecord
            Dim ProfDef As clsProfileDefRecord
            Dim NewDef As clsProfileDefRecord
            Dim NewID As String
            Dim i As Long = 0

            For Each ProfDat In Data.ProfileData.ProfileDatRecords.Records.Values
                If Data.ProfileData.ProfileDefRecords.Records.ContainsKey(ProfDat.di.Trim.ToUpper) Then
                    ProfDef = Data.ProfileData.ProfileDefRecords.Records.Item(ProfDat.di.Trim.ToUpper)

                    If ProfDat.ID.Trim.ToUpper <> ProfDef.ID.Trim.ToUpper Then

                        'make a definition ID that does not yet exist
                        NewID = ProfDat.ID  'start by trying the profile ID itself
                        While Data.ProfileData.ProfileDefRecords.Records.ContainsKey(NewID.Trim.ToUpper)
                            i += 1
                            NewID = ProfDat.ID & "_" & i
                        End While
                        NewDef = ProfDef.Clone(NewID)

                        'add the new definition
                        Data.ProfileData.ProfileDefRecords.Records.Add(NewDef.ID.Trim.ToUpper, NewDef)
                        ProfDat.di = NewDef.ID

                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function MakeProfileDefinitionsUnique.")
            Return False
        End Try

    End Function


    Public Sub Stats2Worksheet(ByRef mySheet As clsExcelSheet, ByVal c As Long, ByVal CaseName As String)
        Dim r As Long

        r = 0
        mySheet.ws.Cells(r, c).Value = CaseName

        'begin met reaches
        r += 1
        mySheet.ws.Cells(r, 0).Value = "Number of reaches"
        mySheet.ws.Cells(r, c).Value = Stats.nReaches

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Total reach length"
        mySheet.ws.Cells(r, c).Value = Stats.TotalReachLength

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Weirs"
        mySheet.ws.Cells(r, c).Value = Stats.nWeirs

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Culverts"
        mySheet.ws.Cells(r, c).Value = Stats.nCulverts

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Bridges"
        mySheet.ws.Cells(r, c).Value = Stats.nBridges

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Orifices"
        mySheet.ws.Cells(r, c).Value = Stats.nOrifices

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Pumps"
        mySheet.ws.Cells(r, c).Value = Stats.nPumps

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Universal weirs"
        mySheet.ws.Cells(r, c).Value = Stats.nUniWeirs

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Lateral nodes"
        mySheet.ws.Cells(r, c).Value = Stats.nLaterals

        r += 1
        mySheet.ws.Cells(r, 0).Value = "RR on flow connections"
        mySheet.ws.Cells(r, c).Value = Stats.nRRCFConn

    End Sub

    Public Sub writeFrictionData(ExportDir As String)
        Me.Data.FrictionData.Export(False, ExportDir)
    End Sub

    Public Sub writeProfileData(ExportDir)
        Me.Data.ProfileData.Export(False, ExportDir)
    End Sub

    Public Sub writeProfileDat(ExportDir As String)
        Me.Data.ProfileData.ProfileDatRecords.Write(False, ExportDir)
    End Sub

    Public Sub writeLateralData(ExportDir As String)
        Me.Data.LateralData.Export(False, ExportDir)
    End Sub

    ''' <summary>
    ''' Berekent de bodemhoogte gegeven een tak en afstand op de tak
    ''' </summary>
    ''' <param name="myReach"></param>
    ''' <param name="lc"></param>
    ''' <remarks></remarks>
    Friend Function CalculateBedLevel(ByVal myReach As clsSbkReach, ByVal lc As Double, ByRef BL As Double) As Boolean
        Dim upProfile As clsSbkReachObject, downProfile As clsSbkReachObject
        Dim upDat As clsProfileDatRecord = Nothing, upDef As clsProfileDefRecord = Nothing
        Dim downDat As clsProfileDatRecord = Nothing, downDef As clsProfileDefRecord = Nothing
        Dim upBL As Double, downBL As Double, upBLFound As Boolean, downBLFound As Boolean
        Dim upDist As Double, dnDist As Double

        Try
            Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
            Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
            upProfile = myReach.GetUpstreamObject(ReachesUpProcessed, lc, upDist, False, True, False, False, True)
            downProfile = myReach.GetDownstreamObject(ReachesDnProcessed, lc, dnDist, False, True, False, False, True)

            If Not upProfile Is Nothing Then
                If SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.ContainsKey(upProfile.ID.Trim.ToUpper) Then
                    upDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(upProfile.ID.Trim.ToUpper)
                    If Not upDat Is Nothing Then upDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(upDat.di.Trim.ToUpper)
                    If Not upDef Is Nothing Then upBL = upDat.CalculateLowestBedLevel(upDef)
                    If Not Double.IsNaN(upBL) Then upBLFound = True
                End If
            End If
            If Not downProfile Is Nothing Then
                If SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records.ContainsKey(downProfile.ID.Trim.ToUpper) Then
                    downDat = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(downProfile.ID.Trim.ToUpper)
                    If Not downDat Is Nothing Then downDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(downDat.di.Trim.ToUpper)
                    If Not downDef Is Nothing Then downBL = downDat.CalculateLowestBedLevel(downDef)
                    If Not Double.IsNaN(downBL) Then downBLFound = True
                End If
            End If

            If upBLFound AndAlso downBLFound Then
                BL = Setup.GeneralFunctions.Interpolate(upProfile.lc, upBL, downProfile.lc, downBL, lc)
                Return True
            ElseIf upBLFound Then
                BL = upBL
                Return True
            ElseIf downBLFound Then
                BL = downBL
                Return True
            Else
                Throw New Exception("Error: could find neither an upstream nor downstream cross section for reach " & myReach.Id)
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalculateBedLevel")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function simplifyTableProfileDefinitions(ByVal percentileIncrement As Integer) As Boolean
        Dim myDef As clsProfileDefRecord, minY As Single, maxY As Single, stepsize As Single
        Dim minX As Single, maxX As Single
        Dim myTable As clsSobekTable, newTable As clsSobekTable
        Dim newX As Single, newVal1 As Single, newVal2 As Single, newVal3 As Single, nCols As Integer
        Dim i As Integer

        Dim nPoints As Integer = (100 / percentileIncrement) + 1
        Dim nSteps As Integer = 100 / percentileIncrement

        For Each myDef In Data.ProfileData.ProfileDefRecords.Records.Values
            If myDef.ty = GeneralFunctions.enmProfileType.tabulated Then
                If myDef.ltlwTable.XValues.Count > nPoints Then
                    myTable = myDef.ltlwTable
                    newTable = New clsSobekTable(Me.Setup)

                    nCols = myTable.getnDataCols

                    'get min and max value for the parameter (Value1 and XValue)
                    minY = myTable.Values1.Values(0)
                    maxY = myTable.Values1.Values(myTable.Values1.Values.Count - 1)
                    minX = myTable.XValues.Values(0)
                    maxX = myTable.XValues.Values(myTable.XValues.Values.Count - 1)
                    stepsize = (maxY - minY) / nSteps

                    'build a new table based on the maximum number of steps
                    If nCols = 1 Then
                        newTable.AddDataPair(2, minX, minY) 'add the lowest value anyhow
                        For i = 1 To nSteps - 1
                            newVal1 = minY + i * stepsize
                            newX = myTable.InterpolateXValueFromValues(newVal1, 1)
                            newTable.AddDataPair(2, newX, newVal1)
                        Next
                        newTable.AddDataPair(2, maxX, maxY) 'add the highest value anyhow
                    ElseIf nCols = 2 Then
                        newTable.AddDataPair(3, minX, minY, myTable.Values2.Values(0)) 'add the lowest value anyhow
                        For i = 1 To nSteps - 1
                            newVal1 = minY + i * stepsize
                            newX = myTable.InterpolateXValueFromValues(newVal1, 1)
                            newVal2 = myTable.InterpolateFromXValues(newX, 2)
                            newTable.AddDataPair(3, newX, newVal1, newVal2)
                        Next
                        newTable.AddDataPair(3, maxX, maxY, myTable.Values2.Values(myTable.Values2.Count - 1)) 'add the highest value anyhow
                    ElseIf nCols = 3 Then
                        newTable.AddDataPair(4, minX, minY, myTable.Values2.Values(0), myTable.Values3.Values(0)) 'add the lowest value anyhow
                        For i = 1 To nSteps - 1
                            newVal1 = minY + i * stepsize
                            newX = myTable.InterpolateXValueFromValues(newVal1, 1)
                            newVal2 = myTable.InterpolateFromXValues(newX, 2)
                            newVal3 = myTable.InterpolateFromXValues(newX, 3)
                            newTable.AddDataPair(4, newX, newVal1, newVal2, newVal3)
                        Next
                        newTable.AddDataPair(4, maxX, maxY, myTable.Values2.Values(myTable.Values2.Count - 1), myTable.Values3.Values(myTable.Values3.Count - 1)) 'add the highest value anyhow
                    End If
                    myDef.ltlwTable = newTable
                End If
            End If
        Next

    End Function


    Friend Function CorrectWaterLevelForMinimumDepth(ByRef myReach As clsSbkReach, ByRef myDist As Double, ByVal myLV As Double, ByVal minDepth As Double) As Double
        Dim myBL As Double 'bed level
        If CalculateBedLevel(myReach, myDist, myBL) Then
            If myLV < (myBL + minDepth) Then
                Return myBL + minDepth
            Else
                Return myLV
            End If
        Else
            Return myLV
        End If
    End Function

    Friend Function ExtractData(ByRef targetTopo As clsCFTopology) As clsCFAttributeData
        'in de toekomst gaan we hier nog een selectie op uitvoeren, namelijk alleen die dwarsprofielen kunstwerken en 
        'wat dies meer zij die ook daadwerkelijk in het te vervaardigen model terecht moeten komen.
        'maar voor nu dumpen we gewoon alle attribuutdata in het doelmodel
        Return Data
    End Function


    Friend Sub WriteMODHMS()
        Dim tmpStr As String

        Dim MXSEG As Integer, MXCHN As Integer, MXJUN As Integer, MXBRN As Integer, MXPNTLD As Integer, MXZDGC As Integer, MXSEC_T As Integer,
        MXPT_T As Integer, ISTRUCTR As Integer, NLINKS As Integer, NSBODY As Integer, ICHRCH As Integer

        Dim ICHCB As Integer, ICHCC As Integer, IRFTYPC As Integer, GRAVT As Long, VISCKCH As Integer, ISCHEMEC As Integer, WEIRCD As Double,
        IMPGCH As Integer, IOBKRCH As Integer, KRSILCH As Integer, IPRSMN As Integer

        Dim IRCH As Integer = 0, ISTRTSETG As Integer = 0, IENDSETG As Integer = 0

        MXSEG = SbkCase.CFTopo.Reaches.Reaches.Count
        MXCHN = SbkCase.CFTopo.Reaches.Reaches.Count
        MXJUN = SbkCase.CFTopo.ReachNodes.ReachNodes.Count
        MXBRN = 6
        MXPNTLD = 0
        MXZDGC = 10
        MXSEC_T = 1000
        MXPT_T = 100
        ISTRUCTR = 0
        NLINKS = 0
        NSBODY = 0
        ICHRCH = 0

        ICHCB = 95              'Unit number for saving/printing cell-by-cell budgets and fluxes. + ve = unit number, -ve = print in output file, 0 = no.
        ICHCC = 0               'Unit number for saving/printing cell-by-cell budgets and mass fluxes from contaminant transport simulation. + ve = unit number, -ve = print in output file, 0 = no.
        IRFTYPC = 0             'friction. 0=manning, 1=Chezy, 2=Darcy/Weisbach, 3=Darcy/Weisbach
        GRAVT = 240260000000    'Gravitational acceleration in a consistent set of units.
        VISCKCH = 1.0           'Kinematic viscosity of water in a consistent set of units. (only for Darcy/Weisbach)
        ISCHEMEC = 0            'Index of groundwater interaction scheme. = 0 allows CHF node heads to fall below land surface. = 1 do not allow CHF node heads to fall below land surface. = 2 use decoupled approach with flux coupling. = 3 use decoupled approach with head coupling.
        WEIRCD = 0.6            'Weir discharge coefficient (equations 11a and 11b) for flow between channel and overland nodes. This coefficient includes effects of contraction and has been experimentally determined to be around 0.6.
        IMPGCH = 0              'Flag for treatment of the gradient term of equation 7, for numerical efficiency. = 0 if this term is treated semi-implicitly for Newton-Raphson linearization. = 1 if this term is treated fully implicitly for Newton-Raphson linearization. = 2 if bed slope approximation is is used instead of head slope for calculation of this term.
        IOBKRCH = 1             'Flag indicating if the conductance term is to be adjusted over the obstruction storage height. = 0 if no = 1 if yes.
        KRSILCH = 1             'Flag for treatment of depth in the flow term = 0 if flow depths are computed from the bed elevation of the respective nodes. = 1 if flow depths are computed from the sill elevation between the two nodes for which flow is computed.
        IPRSMN = 0              'Flag indicating treatment of full pipe flow = 0 if full pipe flow is treated with the channel flow equations. = 1 if full pipe flow is treated with the Priessman Slot concept to compute pressurized flow.


        Using chfWriter = New StreamWriter(Me.Setup.Settings.ExportDirRoot & "\sobekmodel.chf", False)

            '1. Heading
            chfWriter.WriteLine("Channel Package")

            '2a. MXSEG MXCHN MXJUN MXBRN MXPNTLD MXZDGC MXSEC_T MXPT_T ISTRUCTR NLINKS NSBODY ICHRCH
            tmpStr = ""
            tmpStr &= Setup.GeneralFunctions.FormatI10(MXSEG)    'MXSEG (max number of channel segments in the simulation) 
            tmpStr &= Setup.GeneralFunctions.FormatI10(MXCHN)    'MXCHN (max number of channel segments in the simulation)
            tmpStr &= Setup.GeneralFunctions.FormatI10(MXJUN)    'MXJUN (max number of junctions in the simulation) 
            tmpStr &= Setup.GeneralFunctions.FormatI10(MXBRN)    'MXBRN (max number of branches in the simulation) 
            tmpStr &= Setup.GeneralFunctions.FormatI10(MXPNTLD)  'MXPNTLD (max number of pointloads on channels in the simulation)
            tmpStr &= Setup.GeneralFunctions.FormatI10(MXZDGC)   'MXZDGC (Maximum number of zero-depth gradient and critical depth boundary conditions that exist in the simulation.
            tmpStr &= Setup.GeneralFunctions.FormatI10(MXSEC_T)  'MXSEC_T(Maximum number of general cross-section types (tabulated) in the simulation)
            tmpStr &= Setup.GeneralFunctions.FormatI10(MXPT_T)   'MXPT_T (Maximum number of interpolation points (rows) that will be used to define tabulated hydraulic properties (depth versus area, wetted perimeter, and top width)
            tmpStr &= Setup.GeneralFunctions.FormatI10(ISTRUCTR) 'ISTRUCTR (0 = no structures, 1 = structures, 2 = structures only on direct flow links or boundaries
            tmpStr &= Setup.GeneralFunctions.FormatI10(NLINKS)   'NLINKS (number of direct flow links (Q/H-relation via structure routines)
            tmpStr &= Setup.GeneralFunctions.FormatI10(NSBODY)   'NSBODY (Number of surface water bodies modeled with the CHF package, for which tabular depth-area relationships are entered)
            tmpStr &= Setup.GeneralFunctions.FormatI10(ICHRCH)   'ICHRCH (Areal reacharge index, 0 = no areal reacharge on segments, 1 = yes (inflow * surface area), 2 = 
            chfWriter.WriteLine(tmpStr)

            '2b ICHCB ICHCC IRFTYPC GRAVT VISCKCH ISCHEMEC WEIRCD IMPGCH IOBKRCH KRSILCH IPRSMN
            tmpStr = ""
            tmpStr &= Setup.GeneralFunctions.FormatI10(ICHCB)    'Unit number for saving/printing cell-by-cell budgets and fluxes. + ve = unit number, -ve = print in output file, 0 = no.
            tmpStr &= Setup.GeneralFunctions.FormatI10(ICHCC)    'Unit number for saving/printing cell-by-cell budgets and mass fluxes from contaminant transport simulation. + ve = unit number, -ve = print in output file, 0 = no.
            tmpStr &= Setup.GeneralFunctions.FormatI10(IRFTYPC)  'frictional 0=manning, 1=chezy, 2=Darcy, 3=Darcy2
            tmpStr &= Setup.GeneralFunctions.FormatI10(GRAVT)
            tmpStr &= Setup.GeneralFunctions.FormatI10(VISCKCH)
            tmpStr &= Setup.GeneralFunctions.FormatI10(ISCHEMEC)
            tmpStr &= Setup.GeneralFunctions.FormatI10(WEIRCD)
            tmpStr &= Setup.GeneralFunctions.FormatI10(IMPGCH)
            tmpStr &= Setup.GeneralFunctions.FormatI10(IOBKRCH)
            tmpStr &= Setup.GeneralFunctions.FormatI10(KRSILCH)
            tmpStr &= Setup.GeneralFunctions.FormatI10(IPRSMN)
            chfWriter.WriteLine(tmpStr)

            '3. structures (only if structures exist (ISTRUCTR=1 or ISTRUCTR=2)
            'MXQHIPT MXZDST MXQH MXQH1H2 MXQHUHD MXQ1H1Q2H2 MXSTRCH MXQT MXQDAR

            '4. reaches
            '4a. IRCH ISTRTSEG IENDSEG
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                tmpStr = ""
                IRCH += 1
                ISTRTSETG = 1
                IENDSETG = 2
                tmpStr &= Setup.GeneralFunctions.FormatI10(IRCH)
                tmpStr &= Setup.GeneralFunctions.FormatI10(ISTRTSETG)
                tmpStr &= Setup.GeneralFunctions.FormatI10(IENDSETG)
            Next

            '5. IRCHBSD MXRULS ISTORCAL LNKSTOR ICHWADI
            chfWriter.WriteLine("         0         0         0         1         0         \IRECHBSD  MXRULS  ISTORCAL LNKSTOR ICHWADI")

            '6. only if MXRULS > 0
            '6a. NWORK CYTIME LURULE LUEASY IRULPRT IOLDTR ITHT
            '6b. STRKR (MXSTR)

            '7. only if IRCHBSD=1
            'Item 7 is entered MXCHN times, once for each reach that can exist in the domain.
            '7. IRCH IXSEC SR SL BWIDTH BEDZ RGHN BEDKOM ZBANK RILLSH OBSTRH
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                chfWriter.WriteLine("         0       0.0(10E14.6)                    1 IXSEC")
                chfWriter.WriteLine("         0       0.0(10E14.6)                    1 SR")
                chfWriter.WriteLine("         0       0.0(10E14.6)                    1 SL")
                chfWriter.WriteLine("         0     100.0(10E14.6)                    1 BWIDTH")
                chfWriter.WriteLine("        20       1.0(10E14.6)                    1 BEDZ")
                chfWriter.WriteLine(" 1.000000E+002 4.000000E+002")
                chfWriter.WriteLine("         00.01000000(10E14.6)                    1 RGHN")
                chfWriter.WriteLine("         00.01000000(10E14.6)                    1 BEDCOM")
                chfWriter.WriteLine("         0      80.0(10E14.6)                    1 ZBANK")
                chfWriter.WriteLine("         0       0.0(10E14.6)                    1 RILLSHCH")
                chfWriter.WriteLine("         0       0.0(10E14.6)                    1 OBSTRHCH")
            Next


            chfWriter.WriteLine("         0       0.0(10E14.6)                    1 INITIAL HEAD")
            chfWriter.WriteLine("        20       1.0(10E14.6)                    1 LENGTH OF SEGMENT")
            chfWriter.WriteLine(" 4.000488E+000 1.029000E+003")
            chfWriter.WriteLine("        20         1(10I10)                      1 GW NODE")
            chfWriter.WriteLine("      9221      9235")
            chfWriter.WriteLine("         0         1         1         0                                                            ITMP, ITMPBND, ITMPZDGC, ITMPSCHEMEC")
            chfWriter.WriteLine("         0         1(10I5)                       1 IBOUNDCH")
            chfWriter.WriteLine("         1       669         1     0.001       CHF Zero-Depth Gradient BC #1")
            chfWriter.Close()
        End Using

    End Sub

    Public Sub WriteBufferShapeFile()
        'deze routine schrijft een nieuwe shapefile weg met een buffer rond alle watergangen uit het onderhavige model
        'de breedte wordt automatisch afgeleid uit de afmetingen van de dwarsprofielen op de takken
        Dim mySF As New MapWinGIS.Shapefile()
        Dim myShape As MapWinGIS.Shape
        Dim myCP As clsSbkVectorPoint, nextCP As clsSbkVectorPoint
        Dim myRadius As Double, myAngle As Double
        Dim myPoint As New MapWinGIS.Point, firstPoint As New MapWinGIS.Point
        Dim i As Integer, j As Integer, iShape As Integer = -1
        Dim iReach As Integer = 0
        Dim iField As Integer
        Dim newShapeFile As String = Me.Setup.Settings.ExportDirRoot & "\Channels_buffered.shp"

        Try
            If System.IO.File.Exists(newShapeFile) Then Me.Setup.GeneralFunctions.deleteShapeFile(newShapeFile)
            If Not mySF.CreateNewWithShapeID(newShapeFile, MapWinGIS.ShpfileType.SHP_POLYGON) Then Throw New Exception("Could not create new shapefile.")
            If Not mySF.StartEditingShapes() Then Throw New Exception

            Dim field As New MapWinGIS.Field()
            field.Type = MapWinGIS.FieldType.INTEGER_FIELD
            field.Name = "VALUE"
            If Not mySF.EditInsertField(field, iField) Then Throw New Exception

            Me.Setup.GeneralFunctions.UpdateProgressBar("Building buffer shapes...", 0, 10)

            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, SbkCase.CFTopo.Reaches.Reaches.Count)

                'doorloop alle curving points van de tak en stel de polygooncoordinaten in
                For i = 0 To myReach.NetworkcpRecord.CPTable.CP.Count - 2
                    iShape += 1
                    'maak een nieuwe shape aan
                    myShape = New MapWinGIS.Shape
                    myShape.Create(MapWinGIS.ShpfileType.SHP_POLYGON)

                    myCP = myReach.NetworkcpRecord.CPTable.CP(i)
                    nextCP = myReach.NetworkcpRecord.CPTable.CP(i + 1)
                    myRadius = myReach.GetLocalWidth(myCP.Dist + (nextCP.Dist - myCP.Dist) / 2) / 2 'half the width

                    'voeg een punt links van het curving point toe
                    myAngle = Setup.GeneralFunctions.NormalizeAngle(myCP.Angle) - 90
                    myAngle = Setup.GeneralFunctions.NormalizeAngle(myAngle)
                    'bereken de coordinaten van het punt

                    j = 0
                    myPoint = New MapWinGIS.Point
                    Call Setup.GeneralFunctions.OffsetPoint(myCP.X, myCP.Y, myCP.Angle, myRadius, myPoint.x, myPoint.y, True)
                    myShape.InsertPoint(myPoint, j)

                    If j = 0 Then
                        firstPoint = New MapWinGIS.Point
                        firstPoint.x = myPoint.x
                        firstPoint.y = myPoint.y
                    End If

                    'dan het tweede curving point
                    j += 1
                    myPoint = New MapWinGIS.Point
                    Call Setup.GeneralFunctions.OffsetPoint(nextCP.X, nextCP.Y, myCP.Angle, myRadius, myPoint.x, myPoint.y, True)
                    myShape.InsertPoint(myPoint, j)

                    'dan het derde curving point
                    j += 1
                    myPoint = New MapWinGIS.Point
                    Call Setup.GeneralFunctions.OffsetPoint(nextCP.X, nextCP.Y, myCP.Angle, myRadius, myPoint.x, myPoint.y, False)
                    myShape.InsertPoint(myPoint, j)

                    'dan het vierde curving point
                    j += 1
                    myPoint = New MapWinGIS.Point
                    Call Setup.GeneralFunctions.OffsetPoint(myCP.X, myCP.Y, myCP.Angle, myRadius, myPoint.x, myPoint.y, False)
                    myShape.InsertPoint(myPoint, j)

                    'en tenslotte het eerste weer en voeg de shape toe aan de shapefile met attribuutwaarde 1
                    j += 1
                    myShape.InsertPoint(firstPoint, j)
                    mySF.EditInsertShape(myShape, iShape)
                    mySF.EditCellValue(iField, iShape, 1)
                Next

            Next

            mySF.StopEditingShapes()
            mySF.Save()
            mySF.Close()

        Catch ex As Exception
            Setup.Log.AddError(ex.Message)
            Setup.Log.AddError("Error creating shapefile")
        End Try
    End Sub

    Public Sub WriteCrossSectionsPolylineShapefile(path As String)
        'deze routine schrijft een nieuwe shapefile weg met polylines die ieder dwarsprofiel vertegenwoordigen
        Dim mySF As New MapWinGIS.Shapefile()
        Dim myShape As MapWinGIS.Shape
        Dim myPoint As New MapWinGIS.Point, firstPoint As New MapWinGIS.Point
        Dim iShape As Integer = -1
        Dim iReach As Integer = 0
        Dim iField As Integer = 0, iReachField As Integer = 1, iChainageField As Integer = 2, iRadiusField As Integer = 3, iFricTypeField As Integer = 4, iFricValField As Integer = 5
        Dim FrictionType As GeneralFunctions.enmFrictionType
        Dim FrictionVal As Double

        Try
            If System.IO.File.Exists(path) Then Me.Setup.GeneralFunctions.deleteShapeFile(path)
            If Not mySF.CreateNewWithShapeID(path, MapWinGIS.ShpfileType.SHP_POLYLINE) Then Throw New Exception("Could not create new shapefile.")
            If Not mySF.StartEditingShapes() Then Throw New Exception("Error editing shapefile.")
            If Not mySF.StartEditingTable() Then Throw New Exception("Error editing shapefile table.")

            Dim field As New MapWinGIS.Field()
            field.Type = MapWinGIS.FieldType.INTEGER_FIELD
            field.Name = "VALUE"
            If Not mySF.EditInsertField(field, iField) Then Throw New Exception

            Dim reachField As New MapWinGIS.Field()
            reachField.Type = MapWinGIS.FieldType.STRING_FIELD
            reachField.Name = "REACHID"
            If Not mySF.EditInsertField(reachField, iReachField) Then Throw New Exception

            Dim chainageField As New MapWinGIS.Field()
            chainageField.Type = MapWinGIS.FieldType.DOUBLE_FIELD
            chainageField.Name = "CHAINAGE"
            If Not mySF.EditInsertField(chainageField, iChainageField) Then Throw New Exception

            Dim widthField As New MapWinGIS.Field()
            widthField.Type = MapWinGIS.FieldType.DOUBLE_FIELD
            widthField.Name = "WIDTH"
            If Not mySF.EditInsertField(widthField, iRadiusField) Then Throw New Exception

            Dim fricTypeField As New MapWinGIS.Field()
            fricTypeField.Type = MapWinGIS.FieldType.STRING_FIELD
            fricTypeField.Name = "FRICTYPE"
            If Not mySF.EditInsertField(fricTypeField, iFricTypeField) Then Throw New Exception

            Dim fricValField As New MapWinGIS.Field()
            fricValField.Type = MapWinGIS.FieldType.DOUBLE_FIELD
            fricValField.Name = "FRICVAL"
            If Not mySF.EditInsertField(fricValField, iFricValField) Then Throw New Exception

            Me.Setup.GeneralFunctions.UpdateProgressBar("Building polyline shapes...", 0, 10)
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, SbkCase.CFTopo.Reaches.Reaches.Count)

                'doorloop alle takobjecten
                For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    If myObj.nt = enmNodetype.SBK_PROFILE Then
                        myObj.calcXY()
                        myObj.calcAngle()
                        iShape += 1

                        'siebe; nog verbeteren dit
                        Dim LeftDist As Double = Data.ProfileData.getMaxProfileWidth(myObj.ID) / 2
                        Dim RightDist As Double = Data.ProfileData.getMaxProfileWidth(myObj.ID) / 2

                        Call SbkCase.GetFrictionValue(myReach, myObj.lc, myObj.lc, FrictionType, FrictionVal)

                        'maak een nieuwe shape aan
                        myShape = New MapWinGIS.Shape
                        myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)

                        'neem het punt te linkerzijde als startpunt en het punt te rechterzijde als eindpunt
                        'we roteren vanuit 
                        myPoint = New MapWinGIS.Point
                        Me.Setup.GeneralFunctions.RotatePoint(0, LeftDist, myObj.X, myObj.Y, myObj.Angle - 90, myPoint.x, myPoint.y)
                        myShape.InsertPoint(myPoint, 0)

                        myPoint = New MapWinGIS.Point
                        Me.Setup.GeneralFunctions.RotatePoint(0, RightDist, myObj.X, myObj.Y, myObj.Angle + 90, myPoint.x, myPoint.y)
                        myShape.InsertPoint(myPoint, 0)

                        mySF.EditInsertShape(myShape, iShape)
                        mySF.EditCellValue(iField, iShape, 1)
                        mySF.EditCellValue(iReachField, iShape, myReach.Id)
                        mySF.EditCellValue(iChainageField, iShape, myObj.lc)
                        mySF.EditCellValue(iRadiusField, iShape, LeftDist + RightDist)
                        mySF.EditCellValue(iFricTypeField, iShape, FrictionType.ToString)
                        mySF.EditCellValue(iFricValField, iShape, FrictionVal)

                    End If
                Next

                ''doorloop alle curving points van de tak en stel de polygooncoordinaten in
                'For i = 0 To myReach.NetworkcpRecord.CPTable.CP.Count - 2
                '    iShape += 1

                '    myCP = myReach.NetworkcpRecord.CPTable.CP(i)
                '    nextCP = myReach.NetworkcpRecord.CPTable.CP(i + 1)
                '    myRadius = myReach.GetLocalWidth(myCP.Dist + (nextCP.Dist - myCP.Dist) / 2) / 2 'half the width
                '    Call SbkCase.GetFrictionValue(myReach, myCP.Dist, nextCP.Dist, FrictionType, FrictionVal)

                '    'maak een nieuwe (tijdelijke) shape aan
                '    myShape = New MapWinGIS.Shape
                '    myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)

                '    myPoint = New MapWinGIS.Point
                '    myPoint.x = myCP.X
                '    myPoint.y = myCP.Y
                '    myShape.InsertPoint(myPoint, 0)

                '    myPoint = New MapWinGIS.Point
                '    myPoint.x = nextCP.X
                '    myPoint.y = nextCP.Y
                '    myShape.InsertPoint(myPoint, 1)

                '    'maak een buffershape van deze shape
                '    bufShape = New MapWinGIS.Shape
                '    bufShape = myShape.Buffer(myRadius, 12)

                '    mySF.EditInsertShape(bufShape, iShape)
                '    mySF.EditCellValue(iField, iShape, 1)
                '    mySF.EditCellValue(iReachField, iShape, myReach.Id)
                '    mySF.EditCellValue(iChainageField, iShape, myCP.Dist)
                '    mySF.EditCellValue(iRadiusField, iShape, myRadius)
                '    mySF.EditCellValue(iFricTypeField, iShape, FrictionType.ToString)
                '    mySF.EditCellValue(iFricValField, iShape, FrictionVal)
                'Next

            Next

            mySF.StopEditingShapes(True, True)
            mySF.Save()
            mySF.Close()

        Catch ex As Exception
            Setup.Log.AddError(ex.Message)
            Setup.Log.AddError("Error creating shapefile")
        End Try
    End Sub

    Public Sub WriteBufferShapeFile2()
        'deze routine schrijft een nieuwe shapefile weg met een buffer rond alle watergangen uit het onderhavige model
        'de breedte wordt automatisch afgeleid uit de afmetingen van de dwarsprofielen op de takken
        Dim mySF As New MapWinGIS.Shapefile()
        Dim myShape As MapWinGIS.Shape, bufShape As MapWinGIS.Shape
        Dim myCP As clsSbkVectorPoint, nextCP As clsSbkVectorPoint
        Dim myRadius As Double
        Dim myPoint As New MapWinGIS.Point, firstPoint As New MapWinGIS.Point
        Dim i As Integer, iShape As Integer = -1
        Dim iReach As Integer = 0
        Dim iField As Integer = 0, iReachField As Integer = 1, iChainageField As Integer = 2, iRadiusField As Integer = 3, iFricTypeField As Integer = 4, iFricValField As Integer = 5
        Dim newShapeFile As String = Me.Setup.Settings.ExportDirRoot & "\Channels_buffered.shp"
        Dim FrictionType As GeneralFunctions.enmFrictionType
        Dim FrictionVal As Double

        Try
            If System.IO.File.Exists(newShapeFile) Then Me.Setup.GeneralFunctions.deleteShapeFile(newShapeFile)
            If Not mySF.CreateNewWithShapeID(newShapeFile, MapWinGIS.ShpfileType.SHP_POLYGON) Then Throw New Exception("Could not create new shapefile.")
            If Not mySF.StartEditingShapes() Then Throw New Exception("Error editing shapefile.")
            If Not mySF.StartEditingTable() Then Throw New Exception("Error editing shapefile table.")

            Dim field As New MapWinGIS.Field()
            field.Type = MapWinGIS.FieldType.INTEGER_FIELD
            field.Name = "VALUE"
            If Not mySF.EditInsertField(field, iField) Then Throw New Exception

            Dim reachField As New MapWinGIS.Field()
            reachField.Type = MapWinGIS.FieldType.STRING_FIELD
            reachField.Name = "REACHID"
            If Not mySF.EditInsertField(reachField, iReachField) Then Throw New Exception

            Dim chainageField As New MapWinGIS.Field()
            chainageField.Type = MapWinGIS.FieldType.DOUBLE_FIELD
            chainageField.Name = "CHAINAGE"
            If Not mySF.EditInsertField(chainageField, iChainageField) Then Throw New Exception

            Dim radiusField As New MapWinGIS.Field()
            radiusField.Type = MapWinGIS.FieldType.DOUBLE_FIELD
            radiusField.Name = "RADIUS"
            If Not mySF.EditInsertField(radiusField, iRadiusField) Then Throw New Exception

            Dim fricTypeField As New MapWinGIS.Field()
            fricTypeField.Type = MapWinGIS.FieldType.STRING_FIELD
            fricTypeField.Name = "FRICTYPE"
            If Not mySF.EditInsertField(fricTypeField, iFricTypeField) Then Throw New Exception

            Dim fricValField As New MapWinGIS.Field()
            fricValField.Type = MapWinGIS.FieldType.DOUBLE_FIELD
            fricValField.Name = "FRICVAL"
            If Not mySF.EditInsertField(fricValField, iFricValField) Then Throw New Exception


            Me.Setup.GeneralFunctions.UpdateProgressBar("Building buffer shapes...", 0, 10)

            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, SbkCase.CFTopo.Reaches.Reaches.Count)

                'doorloop alle curving points van de tak en stel de polygooncoordinaten in
                For i = 0 To myReach.NetworkcpRecord.CPTable.CP.Count - 2
                    iShape += 1

                    myCP = myReach.NetworkcpRecord.CPTable.CP(i)
                    nextCP = myReach.NetworkcpRecord.CPTable.CP(i + 1)
                    myRadius = myReach.GetLocalWidth(myCP.Dist + (nextCP.Dist - myCP.Dist) / 2) / 2 'half the width
                    Call SbkCase.GetFrictionValue(myReach, myCP.Dist, nextCP.Dist, FrictionType, FrictionVal)

                    'maak een nieuwe (tijdelijke) shape aan
                    myShape = New MapWinGIS.Shape
                    myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)

                    myPoint = New MapWinGIS.Point
                    myPoint.x = myCP.X
                    myPoint.y = myCP.Y
                    myShape.InsertPoint(myPoint, 0)

                    myPoint = New MapWinGIS.Point
                    myPoint.x = nextCP.X
                    myPoint.y = nextCP.Y
                    myShape.InsertPoint(myPoint, 1)

                    'maak een buffershape van deze shape
                    bufShape = New MapWinGIS.Shape
                    bufShape = myShape.Buffer(myRadius, 12)

                    mySF.EditInsertShape(bufShape, iShape)
                    mySF.EditCellValue(iField, iShape, 1)
                    mySF.EditCellValue(iReachField, iShape, myReach.Id)
                    mySF.EditCellValue(iChainageField, iShape, myCP.Dist)
                    mySF.EditCellValue(iRadiusField, iShape, myRadius)
                    mySF.EditCellValue(iFricTypeField, iShape, FrictionType.ToString)
                    mySF.EditCellValue(iFricValField, iShape, FrictionVal)
                Next

            Next

            mySF.StopEditingShapes(True, True)
            mySF.Save()
            mySF.Close()

        Catch ex As Exception
            Setup.Log.AddError(ex.Message)
            Setup.Log.AddError("Error creating shapefile")
        End Try
    End Sub


    Public Sub truncateProfilesByEmbankment(ByVal Tabulated As Boolean, ByVal Trapezium As Boolean, ByVal OpenCircle As Boolean, ByVal Sedredge As Boolean, ByVal ClosedCircle As Boolean, ByVal Type5 As Boolean, ByVal EggShape As Boolean, ByVal EggShape2 As Boolean, ByVal Rectangle As Boolean, ByVal Type9 As Boolean, ByVal YZ As Boolean, ByVal AsymTrapezoidal As Boolean)
        Call Data.ProfileData.Truncate(5, 0, 0, 0, 0, Tabulated, Trapezium, OpenCircle, Sedredge, ClosedCircle, Type5, EggShape, EggShape2, Rectangle, Type9, YZ, AsymTrapezoidal)
    End Sub

    Public Sub exportYZProfileSectionsToShapeFile(ByVal FileNameBase As String, ByVal OnlyStartEnd As Boolean)
        Call Data.ProfileData.ProfileDefRecords.exportYZProfileSectionsToShapeFile(FileNameBase, OnlyStartEnd)
    End Sub

    Public Sub SetFloodPlainFriction(ByVal FricType As STOCHLIB.GeneralFunctions.enmFrictionType, ByVal LeftPlain As Double, ByVal MainChannel As Double, ByVal RightPlain As Double, ByVal ProcessedProfilesOnly As Boolean)
        'Date: 30-5-2013
        'Author: Siebe Bosch
        'Description: Identifies from all YZ-cross sections the friction sections for: left floodplain, main channel and right floodplain
        Dim myDef As clsProfileDefRecord
        Dim myFric As clsFrictionDatCRFRRecord

        For Each myDef In Data.ProfileData.ProfileDefRecords.Records.Values
            If ProcessedProfilesOnly = False OrElse myDef.Processed = True Then
                myFric = New clsFrictionDatCRFRRecord(Me.Setup)
                If myFric.SetByFloodplain(myDef, FricType, LeftPlain, MainChannel, RightPlain) Then
                    If Data.FrictionData.FrictionDatCRFRRecords.records.ContainsKey(myFric.ID.Trim.ToUpper) Then Data.FrictionData.FrictionDatCRFRRecords.records.Remove(myFric.ID.Trim.ToUpper)
                    Data.FrictionData.FrictionDatCRFRRecords.records.Add(myFric.ID.Trim.ToUpper, myFric)
                End If
            End If
        Next
    End Sub

    Public Function DisableControllers(Weirs As Boolean) As Boolean
        Dim myObj As clsSbkReachObject
        Try
            For Each myDat In Data.StructureData.StructDatRecords.Records.Values
                If myDat.ca = 1 Then
                    myObj = Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.getReachObject(myDat.ID, False)
                    If myObj.nt = enmNodetype.NodeCFWeir AndAlso Weirs Then
                        myDat.ca = 0
                    ElseIf myObj.nt = enmNodetype.NodeCFUniWeir AndAlso Weirs Then
                        myDat.ca = 0
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function DisableControllers")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



    Public Function BuildFrictionDataForEmbankments(ByVal myFriction As enmFrictionType, ByVal MainVal As Double, ByVal SlopeVal As Double, FloodPlainVal As Double, SlopeThreshold As Double, IncludeByID As Boolean, IncludeFilters As List(Of String), ExcludeByID As Boolean, ExcludeFilter As List(Of String)) As Boolean
        'this routine autodetects embankments and sets friction values for all cross sections
        Dim ProfDat As clsProfileDatRecord, ProfDef As clsProfileDefRecord
        Dim FricDat As clsFrictionDatCRFRRecord
        Dim i As Integer, n As Integer = Data.ProfileData.ProfileDatRecords.Records.Count

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building friction data...", 0, 10)
            For Each ProfDat In Data.ProfileData.ProfileDatRecords.Records.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)


                'Me.Setup.GeneralFunctions.UpdateProgressBar("Building friction data...", 0, 10)
                If Setup.GeneralFunctions.IncludebyRegEx(ProfDat.ID, True, IncludeByID, IncludeFilters, ExcludeByID, ExcludeFilter) Then
                    ProfDef = Data.ProfileData.ProfileDefRecords.Records(ProfDat.di.Trim.ToUpper)
                    FricDat = New clsFrictionDatCRFRRecord(Me.Setup)
                    FricDat = ProfDef.AutoEmbankmentFriction(myFriction, myFriction, myFriction, MainVal, SlopeVal, FloodPlainVal, SlopeThreshold)
                    If Not FricDat Is Nothing Then
                        If Data.FrictionData.FrictionDatCRFRRecords.records.ContainsKey(FricDat.ID.Trim.ToUpper) Then Data.FrictionData.FrictionDatCRFRRecords.records.Remove(FricDat.ID.Trim.ToUpper)
                        Data.FrictionData.FrictionDatCRFRRecords.records.Add(FricDat.ID.Trim.ToUpper, FricDat)
                    End If
                End If
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10)
            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function BuildFrictionDataForEmbankments of class clsCFData.")
            Return False
        End Try
    End Function

    Public Function BuildUniformFrictionPerCrossSection(ByVal myFriction As enmFrictionType, ByVal FricVal As Double, IncludeByID As Boolean, IncludeFilter As String, ExcludeByID As Boolean, ExcludeFilter As String) As Boolean
        'this routine autodetects embankments and sets friction values for all cross sections
        Dim ProfDat As clsProfileDatRecord, ProfDef As clsProfileDefRecord
        Dim FricDat As clsFrictionDatCRFRRecord
        Dim i As Integer, n As Integer = Data.ProfileData.ProfileDatRecords.Records.Count

        Try
            Dim IncludeList As List(Of String) = Setup.GeneralFunctions.ParseList(IncludeFilter, ";")
            Dim ExcludeList As List(Of String) = Setup.GeneralFunctions.ParseList(ExcludeFilter, ";")

            Me.Setup.GeneralFunctions.UpdateProgressBar("Building friction data...", 0, 10)
            For Each ProfDat In Data.ProfileData.ProfileDatRecords.Records.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                If Setup.GeneralFunctions.IncludebyRegEx(ProfDat.ID, True, IncludeByID, IncludeList, ExcludeByID, ExcludeList) Then
                    ProfDef = Data.ProfileData.ProfileDefRecords.Records(ProfDat.di.Trim.ToUpper)
                    FricDat = New clsFrictionDatCRFRRecord(Me.Setup)
                    FricDat = ProfDef.UniformFriction(myFriction, FricVal)
                    If Not FricDat Is Nothing Then
                        If Data.FrictionData.FrictionDatCRFRRecords.records.ContainsKey(FricDat.ID.Trim.ToUpper) Then Data.FrictionData.FrictionDatCRFRRecords.records.Remove(FricDat.ID.Trim.ToUpper)
                        Data.FrictionData.FrictionDatCRFRRecords.records.Add(FricDat.ID.Trim.ToUpper, FricDat)
                    End If
                End If
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10)
            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function BuildFrictionDataForEmbankments of class clsCFData.")
            Return False
        End Try
    End Function


End Class
