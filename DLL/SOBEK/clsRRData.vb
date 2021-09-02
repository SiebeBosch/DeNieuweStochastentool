Option Explicit On

Imports STOCHLIB.General
Imports System.IO

Public Class clsRRData
    Friend Active As Boolean
    Friend Unpaved3B As clsUnpaved3B
    Friend UnpavedAlf As clsUnpavedAlf
    Friend UnpavedSep As clsUnpavedSep
    Friend UnpavedSto As clsUnpavedSto
    Friend UnpavedInf As clsUnpavedInf
    Friend UnpavedTbl As clsUnpavedTBL
    Friend Greenhse3B As clsGreenhse3B
    Friend GreenhseRF As clsGreenHseRF
    Friend Openwate3B As clsOpenwate3B
    Friend Paved3B As clsPaved3B
    Friend PavedDWA As clsPavedDWA
    Friend PavedSTO As clsPavedSTO
    Friend Sacr3BSACR As clsSacramentoSACR
    Friend Sacr3BCAPS As clsSacramentoCAPS
    Friend Sacr3BOPAR As clsSacramentoOPAR
    Friend Sacr3BUNIH As clsSacramentoUNIH
    Friend Bound3B3B As clsBound3B3B
    Friend BOund3BTBL As clsbound3BTBL
    Friend WWTP3B As clsWWTP3B
    Friend Results As clsRRResults

    Friend Stats As clsRRStats

    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase


    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        Unpaved3B = New clsUnpaved3B(Me.Setup, Me.SbkCase)
        UnpavedAlf = New clsUnpavedAlf(Me.Setup)
        UnpavedSep = New clsUnpavedSep(Me.Setup)
        UnpavedInf = New clsUnpavedInf(Me.Setup)
        UnpavedSto = New clsUnpavedSto(Me.Setup)
        UnpavedTbl = New clsUnpavedTBL(Me.Setup)
        Greenhse3B = New clsGreenhse3B(Me.Setup)
        GreenhseRF = New clsGreenHseRF(Me.Setup)
        Openwate3B = New clsOpenwate3B(Me.Setup)
        Paved3B = New clsPaved3B(Me.Setup)
        PavedDWA = New clsPavedDWA(Me.Setup)
        PavedSTO = New clsPavedSTO(Me.Setup)
        Sacr3BSACR = New clsSacramentoSACR(Me.Setup)
        Sacr3BCAPS = New clsSacramentoCAPS(Me.Setup)
        Sacr3BOPAR = New clsSacramentoOPAR(Me.Setup)
        Sacr3BUNIH = New clsSacramentoUNIH(Me.Setup)
        Bound3B3B = New clsBound3B3B(Me.Setup)
        BOund3BTBL = New clsbound3BTBL(Me.Setup)
        WWTP3B = New clsWWTP3B(Me.Setup)
        Results = New clsRRResults(Me.Setup, Me.SbkCase)
        Stats = New clsRRStats(Me.Setup)

    End Sub

    Public Sub AddPrefix(ByVal Prefix As String)
        Unpaved3B.AddPrefix(Prefix)
        UnpavedAlf.AddPrefix(Prefix)
        UnpavedSep.AddPrefix(Prefix)
        UnpavedSto.AddPrefix(Prefix)
        UnpavedInf.AddPrefix(Prefix)
        UnpavedTbl.AddPrefix(Prefix)
        Greenhse3B.AddPrefix(Prefix)
        Paved3B.AddPrefix(Prefix)
        PavedDWA.AddPrefix(Prefix)
        PavedSTO.AddPrefix(Prefix)
        Bound3B3B.AddPrefix(Prefix)
        BOund3BTBL.AddPrefix(Prefix)
        WWTP3B.AddPrefix(Prefix)
    End Sub

    Public Sub WriteUnpaved3B(path As String)
        Unpaved3B.Write(path, False)
    End Sub

    Public Sub WriteUnpavedAlf(path As String)
        UnpavedAlf.Write(path, False)
    End Sub

    Public Sub WritePaved3B(path As String)
        Paved3B.Write(path, False)
    End Sub

    Public Sub WriteGreenhse3b(path As String)
        Greenhse3B.Write(path, False)
    End Sub


    Public Sub WriteUnpavedSep(path As String)
        UnpavedSep.Write(path, False)
    End Sub

    Public Sub AddConstantToUnpavedSep(myVal As Double)
        UnpavedSep.AddConstant(myVal)
    End Sub
    Public Sub SetConstantToUnpavedSep(myVal As Double)
        UnpavedSep.SetConstant(myVal)
    End Sub

    Public Sub WriteUnpavedTBL(path As String)
        UnpavedTbl.Write(path, False)
    End Sub

    Public Sub ExportAll(ByVal Append As Boolean, ExportDir As String)
        Unpaved3B.Write(Append, ExportDir)
        UnpavedAlf.Write(Append, ExportDir)
        UnpavedInf.Write(Append, ExportDir)
        UnpavedSep.Write(Append, ExportDir)
        UnpavedSto.Write(Append, ExportDir)
        Paved3B.Write(Append, ExportDir)
        PavedDWA.Write(Append, ExportDir)
        PavedSTO.Write(Append, ExportDir)
        Sacr3BSACR.Write(Append, ExportDir)
        Sacr3BCAPS.Write(Append, ExportDir)
        Sacr3BOPAR.Write(Append, ExportDir)
        Sacr3BUNIH.Write(Append, ExportDir)
        Greenhse3B.Write(Append, ExportDir)
        GreenhseRF.Write(Append, ExportDir)
        Bound3B3B.Write(Append, ExportDir)
        BOund3BTBL.Write(Append, ExportDir)
        WWTP3B.Write(Append, ExportDir)
    End Sub

    Public Function GetMinMax(ByRef MinZ As Double, ByRef MaxZ As Double) As Boolean
        Try
            For Each up3b As clsUnpaved3BRecord In Unpaved3B.Records.Values
                If up3b.lv > MaxZ Then MaxZ = up3b.lv
                If up3b.lv < MinZ Then MinZ = up3b.lv
            Next
            For Each pv3b As clsPaved3BRecord In Paved3B.Records.Values
                If pv3b.lv > MaxZ Then MaxZ = pv3b.lv
                If pv3b.lv < MinZ Then MinZ = pv3b.lv
            Next
            For Each gr3b As clsGreenhse3BRecord In Greenhse3B.Records.Values
                If gr3b.sl > MaxZ Then MaxZ = gr3b.sl
                If gr3b.sl < MinZ Then MinZ = gr3b.sl
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetUnpaved3BRecord(id As String) As clsUnpaved3BRecord
        If Unpaved3B.Records.ContainsKey(id.Trim.ToUpper) Then
            Return Unpaved3B.Records.Item(id.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetUnpavedAlfERNSTRecord(id As String) As clsUnpavedALFERNSRecord
        If UnpavedAlf.ERNSRecords.ContainsKey(id.Trim.ToUpper) Then
            Return UnpavedAlf.ERNSRecords.Item(id.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetPaved3BRecord(id As String) As clsPaved3BRecord
        If Paved3B.Records.ContainsKey(id.Trim.ToUpper) Then
            Return Paved3B.Records.Item(id.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetGreenhouse3BRecord(id As String) As clsGreenhse3BRecord
        If Greenhse3B.Records.ContainsKey(id.Trim.ToUpper) Then
            Return Greenhse3B.Records.Item(id.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetUnpavedTBLIG_TRecord(TableID As String) As clsUnpavedTBLIG_TRecord
        Try
            Return UnpavedTbl.IG_TRecords.Item(TableID.Trim.ToUpper)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function RemoveAllSeepage() As Boolean
        'this function sets all seepage for all unpaved nodes to zero
        Try
            Dim sep As New clsUnpavedSEPRecord(Me.Setup)
            sep.co = 0
            sep.sp = 0
            sep.ss = 0
            sep.nm = "NOSEEP"
            sep.ID = "NOSEEP"

            'add the new unpaved.sep record to the collection
            If Not UnpavedSep.Records.ContainsKey(sep.ID.Trim.ToUpper) Then
                UnpavedSep.Records.Add(sep.ID.Trim.ToUpper, sep)
            End If

            'assign it to each of the unpaved nodes
            For Each up As clsUnpaved3BRecord In Unpaved3B.Records.Values
                up.SP = sep.ID
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function RemoveAllSeepage of class clsRRData.")
            Return False
        End Try
    End Function

    Public Function GetAddUnpavedTBLIG_TRecord(NodeID As String) As clsUnpavedTBLIG_TRecord
        Try
            Dim my3BRecord As clsUnpaved3BRecord
            Dim myTBLRecord As clsUnpavedTBLIG_TRecord = Nothing
            If Unpaved3B.Records.ContainsKey(NodeID.Trim.ToUpper) Then
                my3BRecord = Unpaved3B.Records.Item(NodeID.Trim.ToUpper)
                If Not my3BRecord.igtable = "" AndAlso my3BRecord.ig = 1 AndAlso UnpavedTbl.IG_TRecords.ContainsKey(my3BRecord.igtable.Trim.ToUpper) Then
                    Return UnpavedTbl.IG_TRecords.Item(my3BRecord.igtable.Trim.ToUpper)
                Else
                    myTBLRecord = New clsUnpavedTBLIG_TRecord(Me.Setup)
                    myTBLRecord.ID = NodeID
                    myTBLRecord.nm = NodeID
                    UnpavedTbl.IG_TRecords.Add(myTBLRecord.ID.Trim.ToUpper, myTBLRecord)
                    my3BRecord.ig = 1
                    my3BRecord.igtable = myTBLRecord.ID
                End If
            End If
            Return myTBLRecord
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetAddUnpavedTBLIG_TRecord of class clsRRData.")
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function


    Public Function GetAddUnpavedTBLSC_TRecord(NodeID As String) As clsUnpavedTBLSC_TRecord
        Try
            Dim my3BRecord As clsUnpaved3BRecord
            Dim myTBLRecord As clsUnpavedTBLSC_TRecord = Nothing
            If Unpaved3B.Records.ContainsKey(NodeID.Trim.ToUpper) Then
                my3BRecord = Unpaved3B.Records.Item(NodeID.Trim.ToUpper)
                If Not my3BRecord.SCurve = "" AndAlso my3BRecord.su = 1 AndAlso UnpavedTbl.SC_TRecords.ContainsKey(my3BRecord.SCurve.Trim.ToUpper) Then
                    Return UnpavedTbl.SC_TRecords.Item(my3BRecord.SCurve.Trim.ToUpper)
                Else
                    myTBLRecord = New clsUnpavedTBLSC_TRecord(Me.Setup)
                    myTBLRecord.ID = NodeID
                    myTBLRecord.nm = NodeID
                    UnpavedTbl.SC_TRecords.Add(myTBLRecord.ID.Trim.ToUpper, myTBLRecord)
                    my3BRecord.su = 1
                    my3BRecord.SCurve = myTBLRecord.ID
                End If
            End If
            Return myTBLRecord
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetAddUnpavedTBLSC_TRecord of class clsRRData.")
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Sub calcStats()
        Dim i As Long
        Stats.minUPMV = 99999999
        Stats.maxUPMV = -99999999
        Stats.minPVMV = 99999999
        Stats.maxPVMV = -99999999

        For Each myRecord As clsUnpaved3BRecord In Unpaved3B.Records.Values
            Stats.nUP += 1
            If myRecord.bt <= 121 Then
                Stats.SoilType(myRecord.bt) += myRecord.ga
            Else
                Me.Setup.Log.AddError("Unknown soil type number " & myRecord.bt & " in schematisation.")
            End If
            Stats.TotUPArea(0) += myRecord.ga
            Stats.avgUPMV += myRecord.lv * myRecord.ga

            If myRecord.lv < Stats.minUPMV Then Stats.minUPMV = myRecord.lv
            If myRecord.lv > Stats.maxUPMV Then Stats.maxUPMV = myRecord.lv
            For i = 1 To 16
                Stats.TotUPArea(i) += myRecord.ar(i)
            Next
        Next
        Stats.avgUPMV = Stats.avgUPMV / Stats.TotUPArea(0)

        For Each myrecord As clsPaved3BRecord In Paved3B.Records.Values
            Stats.nPV += 1
            Stats.TotPVArea += myrecord.ar
            If myrecord.lv < Stats.minPVMV Then Stats.minPVMV = myrecord.lv
            If myrecord.lv > Stats.maxPVMV Then Stats.maxPVMV = myrecord.lv
            Stats.avgPVMV += myrecord.lv * myrecord.ar
        Next
        Stats.avgPVMV = Stats.avgPVMV / Stats.TotPVArea
    End Sub

    Public Function getSoilTypeArea(ByVal ID As String, ByVal NodeType As Integer) As Double
        Dim UP As clsUnpaved3BRecord = Unpaved3B.GetRecord(ID)
        If Not UP Is Nothing Then
            If UP.bt = NodeType Then
                Return UP.getTotalLandUseArea
            Else
                Return 0
            End If
        Else
            Return 0
        End If
    End Function

    Public Function getSurfaceLevel(ByVal ID As String) As Double
        Dim UP As clsUnpaved3BRecord = Unpaved3B.GetRecord(ID)
        Dim PV As clsPaved3BRecord = Paved3B.GetRecord(ID)
        Dim GR As clsGreenhse3BRecord = Greenhse3B.GetRecord(ID)

        If Not UP Is Nothing Then
            Return UP.getSurfaceLevel
        ElseIf Not PV Is Nothing Then
            Return PV.lv
        ElseIf Not GR Is Nothing Then
            Return GR.sl
        Else
            Return 0
        End If
    End Function

    Public Sub Stats2Worksheet(ByRef mySheet As clsExcelSheet, ByVal c As Long, ByVal CaseName As String)
        Dim r As Long
        Dim i As Long

        r = 0
        mySheet.ws.Cells(r, c).Value = CaseName

        'begin met rr unpaved
        r += 1
        mySheet.ws.Cells(r, 0).Value = "Number unpaved nodes"
        mySheet.ws.Cells(r, c).Value = Stats.nUP

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Min. level unpaved"
        mySheet.ws.Cells(r, c).Value = Stats.minUPMV

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Mean. level unpaved"
        mySheet.ws.Cells(r, c).Value = Stats.avgUPMV

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Max. level unpaved"
        mySheet.ws.Cells(r, c).Value = Stats.maxUPMV

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Groundwater area unpaved"
        mySheet.ws.Cells(r, c).Value = Stats.TotUPArea(0)

        For i = 1 To 16
            r += 1
            mySheet.ws.Cells(r, 0).Value = "Area land use " & i
            mySheet.ws.Cells(r, c).Value = Stats.TotUPArea(i)
        Next

        r += 1
        For i = 1 To 12
            r += 1
            mySheet.ws.Cells(r, 0).Value = "Area soiltype (no CAPSIM) " & i
            mySheet.ws.Cells(r, c).Value = Stats.SoilType(i)
        Next

        r += 1
        For i = 101 To 121
            r += 1
            mySheet.ws.Cells(r, 0).Value = "Area soiltype (CAPSIM)" & i
            mySheet.ws.Cells(r, c).Value = Stats.SoilType(i)
        Next

        r += 1

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Number paved nodes"
        mySheet.ws.Cells(r, c).Value = Stats.nPV

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Area paved"
        mySheet.ws.Cells(r, c).Value = Stats.TotPVArea

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Min. level paved"
        mySheet.ws.Cells(r, c).Value = Stats.minPVMV

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Mean. level paved"
        mySheet.ws.Cells(r, c).Value = Stats.avgPVMV

        r += 1
        mySheet.ws.Cells(r, 0).Value = "Max. level unpaved"
        mySheet.ws.Cells(r, c).Value = Stats.maxPVMV
    End Sub


    ''' <summary>
    ''' This routine cleans up model objects and data for the flow module.
    ''' It will remove any unused data records
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub CleanUp(ByVal Unpaved As Boolean, ByVal DrainDefs As Boolean)
        Dim RemoveKeys As New List(Of String)

        Dim Up3B As clsUnpaved3BRecord, UpErns As clsUnpavedALFERNSRecord

        'unpaved.3b opschonen
        If Unpaved Then
            For Each Up3B In Unpaved3B.Records.Values
                If SbkCase.RRTopo.Nodes.ContainsKey(Up3B.ID.Trim.ToUpper) Then
                    Up3B.InUse = True
                Else
                    Up3B.InUse = False
                End If
            Next
        End If


        RemoveKeys = New List(Of String)
        For Each myKey As String In Unpaved3B.Records.Keys
            Up3B = Unpaved3B.Records.Item(myKey)
            If Not Up3B.InUse Then RemoveKeys.Add(myKey)
        Next
        For Each myKey As String In RemoveKeys
            Unpaved3B.Records.Remove(myKey)
        Next

        'unpaved.alf opschonen
        If DrainDefs Then
            For Each UpErns In UnpavedAlf.ERNSRecords.Values
                UpErns.InUse = False
                For Each Up3B In Unpaved3B.Records.Values
                    If Up3B.ed = UpErns.ID Then
                        UpErns.InUse = True
                        Exit For
                    End If
                Next
            Next

            RemoveKeys = New List(Of String)
            For Each myKey As String In UnpavedAlf.ERNSRecords.Keys
                UpErns = UnpavedAlf.ERNSRecords.Item(myKey)
                If Not UpErns.InUse Then RemoveKeys.Add(myKey)
            Next
            For Each myKey As String In RemoveKeys
                UnpavedAlf.ERNSRecords.Remove(myKey)
            Next

        End If
    End Sub


    Friend Function shiftByElevationGrid(ByVal Unpaved As Boolean, ByVal Paved As Boolean) As Boolean

        Dim myUp As clsUnpaved3BRecord
        Dim myPv As clsPaved3BRecord
        Dim myNodeTP As clsRRNodeTPRecord
        Dim iNode As Long, nNode As Long
        Dim myVal As Double

        If Unpaved Then
            Me.Setup.GeneralFunctions.UpdateProgressBar("Shifting unpaved nodes by elevation grid...", 0, 10)
            nNode = Unpaved3B.Records.Count
            iNode = 0
            For Each myUp In Unpaved3B.Records.Values
                iNode += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iNode, nNode)
                myNodeTP = Me.SbkCase.RRTopo.GetNode(myUp.ID)
                If Not myNodeTP Is Nothing Then
                    Me.Setup.GISData.ElevationGrid.GetCellValueFromXY(myNodeTP.X, myNodeTP.Y, myVal)
                    myUp.lv += myVal
                End If
            Next
        End If

        If Paved Then
            Me.Setup.GeneralFunctions.UpdateProgressBar("Shifting paved nodes by elevation grid...", 0, 10)
            nNode = Paved3B.Records.Count
            iNode = 0
            For Each myPv In Paved3B.Records.Values
                iNode += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iNode, nNode)
                myNodeTP = Me.SbkCase.RRTopo.GetNode(myPv.ID)
                If Not myNodeTP Is Nothing Then
                    Me.Setup.GISData.ElevationGrid.GetCellValueFromXY(myNodeTP.X, myNodeTP.Y, myVal)
                    myPv.lv += myVal
                End If
            Next
        End If

        Return True

    End Function

    Public Function BuildRRCFBoundaryData(ByVal UseAreaShapeFile As Boolean, Optional ByVal minDepth As Double = 0) As Boolean
        Dim myIdx As Integer
        Dim WP As Double, ZP As Double, i As Integer = 0
        Dim bn As clsSbkReachNode, en As clsSbkReachNode

        Setup.GeneralFunctions.UpdateProgressBar("Building boundary data for rainfall runoff...", 0, 10, True)

        'open the area shapefile
        If UseAreaShapeFile Then
            If Not Me.Setup.GISData.SubcatchmentDataSource.Open Then Me.Setup.Log.AddError("Could not open subcatchments shapefile.")
        End If

        Try
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values

                i += 1
                Setup.GeneralFunctions.UpdateProgressBar("", i, SbkCase.CFTopo.Reaches.Reaches.Count)
                For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection Then
                        myObj.calcXY()
                        If UseAreaShapeFile Then
                            If Me.Setup.GISData.SubcatchmentDataSource.GetRecordIdxByCoord(myObj.X, myObj.Y, myIdx) Then
                                Me.Setup.GISData.SubcatchmentDataSource.GetSummerOutletTargetLevel(myIdx, ZP)
                                Me.Setup.GISData.SubcatchmentDataSource.GetWinterOutletTargetLevel(myIdx, WP)
                                If minDepth > 0 Then ZP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, myObj.lc, ZP, minDepth)
                                If minDepth > 0 Then WP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, myObj.lc, WP, minDepth)
                            End If
                        Else
                            ZP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, myObj.lc, -999, minDepth)
                            WP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, myObj.lc, -999, minDepth)
                        End If
                        Call setBoundaryRecords(myObj.ID, WP, ZP, False)
                    End If
                Next

                bn = myReach.bn
                If bn.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode Then
                    If UseAreaShapeFile Then
                        If Me.Setup.GISData.SubcatchmentDataSource.GetRecordIdxByCoord(bn.X, bn.Y, myIdx) Then
                            Me.Setup.GISData.SubcatchmentDataSource.GetSummerOutletTargetLevel(myIdx, ZP)
                            Me.Setup.GISData.SubcatchmentDataSource.GetWinterOutletTargetLevel(myIdx, WP)
                            If minDepth > 0 Then ZP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, 0, ZP, minDepth)
                            If minDepth > 0 Then WP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, 0, WP, minDepth)
                        End If
                    Else
                        ZP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, 0, -999, minDepth)
                        WP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, 0, -999, minDepth)
                    End If
                    Call setBoundaryRecords(bn.ID, WP, ZP, False)
                End If

                en = myReach.en
                If en.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode Then
                    If UseAreaShapeFile Then
                        If Me.Setup.GISData.SubcatchmentDataSource.GetRecordIdxByCoord(en.X, en.Y, myIdx) Then
                            Me.Setup.GISData.SubcatchmentDataSource.GetSummerOutletTargetLevel(myIdx, ZP)
                            Me.Setup.GISData.SubcatchmentDataSource.GetWinterOutletTargetLevel(myIdx, WP)
                            ZP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, myReach.getReachLength, ZP, minDepth)
                            WP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, myReach.getReachLength, WP, minDepth)
                        End If
                    Else
                        ZP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, myReach.getReachLength, -999, minDepth)
                        WP = SbkCase.CFData.CorrectWaterLevelForMinimumDepth(myReach, myReach.getReachLength, -999, minDepth)
                    End If
                    Call setBoundaryRecords(en.ID, WP, ZP, False)
                End If
            Next

            If UseAreaShapeFile Then Me.Setup.GISData.SubcatchmentDataSource.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function BuildRRCFBoundaryData.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function
    Public Function BuildRRCFBoundaryDataFromFlowResults(ByVal UseLastTimestep As Boolean) As Boolean
        Dim i As Integer = 0
        Dim n As Long = Me.SbkCase.RRTopo.Nodes.Values.Count
        Dim HisResults As New List(Of HisDataRow)
        Dim myCalc As String = ""

        Try

            'then get the HIS results for all calculation points
            If UseLastTimestep Then
                HisResults = SbkCase.CFData.Results.CalcPnt.ReadLastTimeStep("Waterlevel")
            Else
                Throw New Exception("Option for HIS-results selection not yet supported.")
            End If

            Setup.GeneralFunctions.UpdateProgressBar("Building boundary data for rainfall runoff...", 0, 10, True)
            For Each myBnd As clsRRNodeTPRecord In Me.SbkCase.RRTopo.Nodes.Values
                i += 1
                Setup.GeneralFunctions.UpdateProgressBar("", i, n, True)
                If myBnd.nt.ParentID = "SBK_SBK-3B-REACH" OrElse myBnd.nt.ParentID = "SBK_SBK-3B-NODE" Then

                    Dim myObj As clsSbkReachObject = SbkCase.CFTopo.getReachObject(myBnd.ID, False)
                    If Not myObj Is Nothing Then
                        'get the values from calcpnt.his
                        'first find the nearest calculation point
                        If Not SbkCase.CFTopo.GetNearestCalculationPointID(myObj, myCalc) Then
                            Me.Setup.Log.AddWarning("Could not find nearest calculation point for object " & myBnd.ID)
                        Else
                            For Each HISRow As HisDataRow In HisResults
                                If HISRow.LocationName.Trim.ToLower = myCalc.Trim.ToLower Then Call setBoundaryRecords(myBnd.ID, HISRow.Value, HISRow.Value, False)
                            Next
                        End If
                    End If
                End If
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function BuildRRBoundaryData.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function BuildRRBoundaryDataFromGIS(ByVal AlwaysTable As Boolean) As Boolean
        Dim myIdx As Integer
        Dim WP As Double, ZP As Double, i As Integer = 0
        Dim n As Long = Me.SbkCase.RRTopo.Nodes.Values.Count

        Try
            Setup.GeneralFunctions.UpdateProgressBar("Building boundary data for rainfall runoff...", 0, 10, True)
            If Not Me.Setup.GISData.SubcatchmentDataSource.Open() Then Throw New Exception("Could not open shapefile.")
            For Each myBnd As clsRRNodeTPRecord In Me.SbkCase.RRTopo.Nodes.Values
                i += 1
                Setup.GeneralFunctions.UpdateProgressBar("", i, n, True)
                If myBnd.nt.ParentID = "3B_BOUNDARY" Then
                    If Me.Setup.GISData.SubcatchmentDataSource.GetRecordIdxByCoord(myBnd.X, myBnd.Y, myIdx) Then
                        Me.Setup.GISData.SubcatchmentDataSource.GetSummerOutletTargetLevel(myIdx, ZP)
                        Me.Setup.GISData.SubcatchmentDataSource.GetWinterOutletTargetLevel(myIdx, WP)
                        Call setBoundaryRecords(myBnd.ID, WP, ZP, AlwaysTable)
                    End If
                End If
            Next
            Me.Setup.GISData.SubcatchmentDataSource.Close()

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function BuildRRBoundaryData.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function getBoundaryLevel(ByVal ID As String, ByVal Opt As STOCHLIB.GeneralFunctions.enmHydroMathOperation) As Double
        Dim BND As clsBound3B3BRecord, TBL As clsBound3BTBLRecord
        BND = Bound3B3B.GetRecord(ID)

        If Not BND Is Nothing Then
            If BND.bl = 0 Then    'constant value
                Return BND.bl2
            ElseIf BND.bl = 1 Then 'value in time table
                TBL = BOund3BTBL.GetRecord(BND.TableID)
                If Not TBL Is Nothing Then
                    Return TBL.TimeTable.GetStatistic(1, Opt)
                End If
            ElseIf BND.bl = 2 Then 'value online from flow module
                Me.Setup.Log.AddWarning("Could not retrieve boundary level fo node " & ID & " since it is in simultaneous mode.")
                Return -999
            End If
        Else
            Me.Setup.Log.AddWarning("Could not retrieve boundary level for node " & ID)
            Return -999
        End If

    End Function


    Friend Sub setBoundaryRecords(ByVal ObjID As String, ByVal WP As Double, ByVal ZP As Double, ByVal AlwaysTable As Boolean)

        Dim myB3BRecord As New clsBound3B3BRecord(Me.Setup)
        Dim myTBLRecord As New clsBound3BTBLRecord(Me.Setup)

        If ZP <> WP OrElse AlwaysTable Then
            myB3BRecord = Bound3B3B.getAddRecord(ObjID)
            myB3BRecord.bl = 1
            myB3BRecord.TableID = ObjID
            myB3BRecord.is_ = 0

            myTBLRecord = BOund3BTBL.getAddRecord(myB3BRecord.TableID)
            myTBLRecord.nm = myB3BRecord.TableID
            myTBLRecord.TimeTable.pdin1 = 1
            myTBLRecord.TimeTable.pdin2 = 1
            myTBLRecord.TimeTable.PDINPeriod = "365;00:00:00"
            myTBLRecord.TimeTable.AddDatevalPair(New Date(2000, 1, 1), WP, 200)
            myTBLRecord.TimeTable.AddDatevalPair(New Date(2000, 4, 15), ZP, 200)
            myTBLRecord.TimeTable.AddDatevalPair(New Date(2000, 10, 15), WP, 0)
        Else
            myB3BRecord = Bound3B3B.getAddRecord(ObjID)
            myB3BRecord.bl = 0
            myB3BRecord.bl2 = WP
            myB3BRecord.is_ = 0
        End If

    End Sub

    Public Function WriteBoundaryConditionsBC(ExportDir As String) As Boolean
        'this function produces a BoundaryConditions.bc file for D-Hydro
        'it contains a boundary value for each of the RR-on-fow-connections
        'note: at the moment of writing the values themselves have no actual meaning
        Try
            Using bcWriter As New StreamWriter(ExportDir & "\BoundaryConditions.bc", False)
                'write the header
                bcWriter.WriteLine("[General]")
                bcWriter.WriteLine("    fileVersion           = 1.01                ")
                bcWriter.WriteLine("    fileType              = boundConds          ")
                bcWriter.WriteLine("")

                'now for each boundary, write its name, function, quantity , unit and value
                For Each myRecord As clsBound3B3BRecord In Bound3B3B.Records.Values
                    bcWriter.WriteLine("[Boundary]")
                    bcWriter.WriteLine("    name                  = " & myRecord.ID)
                    bcWriter.WriteLine("    function              = constant            ")
                    bcWriter.WriteLine("    quantity              = water_level         ")
                    bcWriter.WriteLine("    unit                  = m                   ")
                    bcWriter.WriteLine("    0 ")
                    bcWriter.WriteLine("")
                Next

            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteBoundaryConditionsBC:" & ex.Message)
            Return False
        End Try
    End Function

    Public Function WriteDIMRXML(ExportDir As String) As Boolean
        'this function writes the DIMR.xml file for D-Hydro. 
        'this file contains the coupling between lateral nodes in D-Hydro 1D Flow and the boundary nodes in RR
        Dim attrList As List(Of String)
        Dim valsList As List(Of String)

        Try
            Using xmlWriter As New StreamWriter(ExportDir & "\dimr_config.xml", False)

                'write the header and the schema
                xmlWriter.WriteLine(Strings.Replace("<?xml version=%1.0% encoding=%utf-8% standalone=%yes%?>", "%", Chr(34)))
                xmlWriter.WriteLine(Strings.Replace("<dimrConfig xmlns=%http: //schemas.deltares.nl/dimr% xmlns:xsi=%http://www.w3.org/2001/XMLSchema-instance% xsi:schemaLocation=%http://schemas.deltares.nl/dimr http://content.oss.deltares.nl/schemas/dimr-1.2.xsd%>", "%", Chr(34)))

                'write the documentation block
                Me.Setup.GeneralFunctions.writexmlOpeningTag(xmlWriter, "documentation", 2)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "fileVersion", 1.2, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "createdBy", "Deltares, Coupling Team", 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "creationDate", "2021-06-11T11:52:41.5839591Z", 4)
                Me.Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "documentation", 2)

                'write the control block
                Me.Setup.GeneralFunctions.writexmlOpeningTag(xmlWriter, "control", 2)
                Me.Setup.GeneralFunctions.writexmlOpeningTag(xmlWriter, "parallel", 4)
                Me.Setup.GeneralFunctions.writexmlOpeningTag(xmlWriter, "startGroup", 6)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "time", "0 3600 31536000", 8)
                attrList = New List(Of String) From {"name"}
                valsList = New List(Of String) From {"flow_to_rr"}
                Me.Setup.GeneralFunctions.writeXMLElementWithAttributes(xmlWriter, "coupler", 8, attrList, valsList)
                attrList = New List(Of String) From {"name"}
                valsList = New List(Of String) From {"Rainfall Runoff"}
                Me.Setup.GeneralFunctions.writeXMLElementWithAttributes(xmlWriter, "start", 8, attrList, valsList)
                attrList = New List(Of String) From {"name"}
                valsList = New List(Of String) From {"rr_to_flow"}
                Me.Setup.GeneralFunctions.writeXMLElementWithAttributes(xmlWriter, "coupler", 8, attrList, valsList)
                Me.Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "startGroup", 6)
                attrList = New List(Of String) From {"name"}
                valsList = New List(Of String) From {"FlowFM"}
                Me.Setup.GeneralFunctions.writeXMLElementWithAttributes(xmlWriter, "start", 6, attrList, valsList)
                Me.Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "parallel", 4)
                Me.Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "control", 2)

                'write the component component
                attrList = New List(Of String) From {"name"}
                valsList = New List(Of String) From {"Rainfall Runoff"}
                Me.Setup.GeneralFunctions.writeXMLOpeningTagWithAttributes(xmlWriter, "component", 2, attrList, valsList)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "library", "dflowfm", 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "workingDir", "dflowfm", 4)
                attrList = New List(Of String) From {"key", "value"}
                valsList = New List(Of String) From {"threads", "1"}
                Me.Setup.GeneralFunctions.writeXMLElementWithAttributes(xmlWriter, "setting", 4, attrList, valsList)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "inputFile", "FlowFM.mdu", 4)
                Me.Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "component", 2)

                'write the coupler component for rr to flow
                attrList = New List(Of String) From {"name"}
                valsList = New List(Of String) From {"rr_to_flow"}
                Me.Setup.GeneralFunctions.writeXMLOpeningTagWithAttributes(xmlWriter, "coupler", 2, attrList, valsList)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "sourceComponent", "Rainfall Runoff", 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "targetComponent", "FlowFM", 4)

                'now we have to loop through all rr-on-flow-connections and write them to an item in the dimr.xml file
                For Each myReach As clsSbkReach In Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Values
                    If myReach.InUse Then
                        For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                            If myObj.nt = GeneralFunctions.enmNodetype.NodeRRCFConnection AndAlso myObj.InUse Then
                                Me.Setup.GeneralFunctions.writexmlOpeningTag(xmlWriter, "item", 4)
                                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "sourceName", "catchments/" & myObj.ID & "/water_discharge", 6)
                                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "sourceName", "laterals/" & myObj.ID & "/water_discharge", 6)
                                Me.Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "item", 4)
                            End If
                        Next
                    End If
                Next

                Me.Setup.GeneralFunctions.writexmlOpeningTag(xmlWriter, "logger", 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "workingDir", ".", 6)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "outputFile", "rr_to_flow.nc", 6)
                Me.Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "logger", 4)
                Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "coupler", 2)

                'write the coupler component for flow to rr
                attrList = New List(Of String) From {"name"}
                valsList = New List(Of String) From {"flow_to_rr"}
                Me.Setup.GeneralFunctions.writeXMLOpeningTagWithAttributes(xmlWriter, "coupler", 2, attrList, valsList)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "sourceComponent", "FlowFM", 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "targetComponent", "Rainfall Runoff", 4)

                'now we have to loop through all rr-on-flow-connections and write them to an item in the dimr.xml file
                For Each myReach As clsSbkReach In Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.Reaches.Reaches.Values
                    If myReach.InUse Then
                        For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                            If myObj.nt = GeneralFunctions.enmNodetype.NodeRRCFConnection AndAlso myObj.InUse Then
                                Me.Setup.GeneralFunctions.writexmlOpeningTag(xmlWriter, "item", 4)
                                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "sourceName", "laterals/" & myObj.ID & "/water_level", 6)
                                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "sourceName", "catchments/" & myObj.ID & "/water_level", 6)
                                Me.Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "item", 4)
                            End If
                        Next
                    End If
                Next
                Me.Setup.GeneralFunctions.writexmlOpeningTag(xmlWriter, "logger", 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "workingDir", ".", 6)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "outputFile", "flow_to_rr.nc", 6)
                Me.Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "logger", 4)
                Setup.GeneralFunctions.writexmlclosingTag(xmlWriter, "coupler", 2)

            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteDIMRXML of class clsRRData.")
            Return False
        End Try
    End Function

    Public Function WriteSOBEK3BFNM(ExportDir As String) As Boolean
        'this function writes the SOBEK_3B.FNM file for D-Hydro. 
        'this is basicaly a list of input- and output files for SOBEK and D-HYDRO
        Try
            Dim myContent As String = Me.Setup.SOBEKData.GetSobek_3bFNMFileContent()
            Using fnmWriter As New StreamWriter(ExportDir & "\Sobek_3b.fnm", False)
                fnmWriter.Write(myContent)
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteSOBEK3BFNM of class clsRRData.")
            Return False
        End Try
    End Function


    Public Sub WriteBoundaryData(ByVal Append As Boolean, ExportDir As String)
        Using my3BWriter As New System.IO.StreamWriter(ExportDir & "\bound3b.3b", Append)
            Using myTBLWriter As New System.IO.StreamWriter(ExportDir & "\bound3b.tbl", Append)
                For Each myB3BRecord As clsBound3B3BRecord In Bound3B3B.Records.Values
                    myB3BRecord.Write(my3BWriter)
                Next
                For Each myTBLRecord As clsBound3BTBLRecord In BOund3BTBL.Records.Values
                    myTBLRecord.Write(myTBLWriter)
                Next
                myTBLWriter.Close()
                myTBLWriter.Dispose()
            End Using
            my3BWriter.Close()
            my3BWriter.Dispose()
        End Using
    End Sub

    Friend Sub getDrainDefsFromOtherCases(ByRef Cases As List(Of ClsSobekCase))
        Dim minDist As Double
        Dim myDist As Double
        Dim activeUP As clsUnpaved3BRecord
        Dim otherUP As clsUnpaved3BRecord
        Dim otherERNS As clsUnpavedALFERNSRecord = Nothing
        Dim activeERNS As clsUnpavedALFERNSRecord

        Dim i As Integer, ModelIdx As Integer

        'gooi de bestaande drainagedefinities weg
        UnpavedAlf.ERNSRecords = New Dictionary(Of String, clsUnpavedALFERNSRecord)
        UnpavedAlf.FileContent = Nothing

        'Doorloop alle knopen en zoek het dichtstbijgelegen exemplaar
        For Each activeUP In Unpaved3B.Records.Values
            minDist = 99999999
            i = 0
            For Each otherCase In Cases
                i += 1
                'zoek nu de XY-coordinaat op van elke RR Unpaved knoop en bepaal of die het dichtst bij ligt
                For Each otherUP In otherCase.RRData.Unpaved3B.Records.Values
                    myDist = Math.Sqrt((activeUP.X - otherUP.X) ^ 2 + (activeUP.Y - otherUP.Y) ^ 2)
                    If myDist < minDist Then
                        minDist = myDist
                        If otherCase.RRData.UnpavedAlf.ERNSRecords.ContainsKey(otherUP.ed) Then
                            otherERNS = otherCase.RRData.UnpavedAlf.ERNSRecords(otherUP.ed)
                            ModelIdx = i
                        End If
                    End If
                Next
            Next
            'we hebben alle andere cases doorlopen, dus schrijf nu de gevonden ERNST-definitie naar het doelmodel
            'voeg het indexnummer van de broncase toe aan het ID voor het geval dezelfde draindef in meerdere bronmodellen voorkomt
            activeERNS = New clsUnpavedALFERNSRecord(Setup)
            activeERNS.ID = otherERNS.ID & "_" & Str(ModelIdx).Trim
            activeUP.ed = activeERNS.ID

            'kopieer de attributen van de gevonden drainagedefinitie
            activeERNS.nm = otherERNS.nm
            activeERNS.cvi = otherERNS.cvi
            activeERNS.cvs = otherERNS.cvs
            activeERNS.cvo1 = otherERNS.cvo1
            activeERNS.cvo2 = otherERNS.cvo2
            activeERNS.cvo3 = otherERNS.cvo3
            activeERNS.cvo4 = otherERNS.cvo4
            activeERNS.lv1 = otherERNS.lv1
            activeERNS.lv2 = otherERNS.lv2
            activeERNS.lv3 = otherERNS.lv3
            If Not UnpavedAlf.ERNSRecords.ContainsKey(activeERNS.ID.Trim.ToUpper) Then UnpavedAlf.ERNSRecords.Add(activeERNS.ID.Trim.ToUpper, activeERNS)
        Next
    End Sub

    Friend Sub getUPStorDefsFromOtherCases(ByRef Cases As List(Of ClsSobekCase))
        Dim minDist As Double
        Dim myDist As Double
        Dim activeUP As clsUnpaved3BRecord
        Dim otherUP As clsUnpaved3BRecord
        Dim otherSTOR As clsUnpavedSTORecord = Nothing
        Dim activeSTOR As clsUnpavedSTORecord

        Dim i As Integer, ModelIdx As Integer

        'gooi de bestaande storage definities weg
        UnpavedSto.Records = New Dictionary(Of String, clsUnpavedSTORecord)
        UnpavedSto.FileContent = Nothing

        'Doorloop alle knopen en zoek het dichtstbijgelegen exemplaar
        For Each activeUP In Unpaved3B.Records.Values
            minDist = 99999999
            i = 0
            For Each otherCase In Cases
                i += 1
                'zoek nu de XY-coordinaat op van elke RR Unpaved knoop en bepaal of die het dichtst bij ligt
                For Each otherUP In otherCase.RRData.Unpaved3B.Records.Values
                    myDist = Math.Sqrt((activeUP.X - otherUP.X) ^ 2 + (activeUP.Y - otherUP.Y) ^ 2)
                    If myDist < minDist Then
                        minDist = myDist
                        If otherCase.RRData.UnpavedSto.Records.ContainsKey(otherUP.sd.Trim.ToUpper) Then
                            otherSTOR = otherCase.RRData.UnpavedSto.Records(otherUP.sd.Trim.ToUpper)
                            ModelIdx = i
                        End If
                    End If
                Next
            Next
            'we hebben alle andere cases doorlopen, dus schrijf nu de gevonden ERNST-definitie naar het doelmodel
            'voeg het indexnummer van de broncase toe aan het ID voor het geval dezelfde draindef in meerdere bronmodellen voorkomt
            activeSTOR = New clsUnpavedSTORecord(Setup)
            activeSTOR.ID = otherSTOR.ID & "_" & Str(ModelIdx).Trim
            activeUP.sd = activeSTOR.ID

            'kopieer de attributen van de gevonden drainagedefinitie
            activeSTOR.nm = otherSTOR.nm
            activeSTOR.il = otherSTOR.il
            activeSTOR.ml = otherSTOR.ml
            If Not UnpavedSto.Records.ContainsKey(activeSTOR.ID.Trim.ToUpper) Then UnpavedSto.Records.Add(activeSTOR.ID.Trim.ToUpper, activeSTOR)
        Next
    End Sub

    Friend Sub getPVStorDefsFromOtherCases(ByRef Cases As List(Of ClsSobekCase))
        Dim minDist As Double
        Dim myDist As Double
        Dim activePV As clsPaved3BRecord
        Dim otherPV As clsPaved3BRecord
        Dim otherSTOR As clsPavedSTORecord = Nothing
        Dim activeSTOR As clsPavedSTORecord

        Dim i As Integer, ModelIdx As Integer

        'gooi de bestaande storage definities weg
        PavedSTO.Records = New Dictionary(Of String, clsPavedSTORecord)
        PavedSTO.FileContent = Nothing

        'Doorloop alle knopen en zoek het dichtstbijgelegen exemplaar
        For Each activePV In Paved3B.Records.Values
            minDist = 99999999
            i = 0
            For Each otherCase In Cases
                i += 1
                'zoek nu de XY-coordinaat op van elke RR Paved knoop en bepaal of die het dichtst bij ligt
                For Each otherPV In otherCase.RRData.Paved3B.Records.Values
                    myDist = Math.Sqrt((activePV.X - otherPV.X) ^ 2 + (activePV.Y - otherPV.Y) ^ 2)
                    If myDist < minDist Then
                        minDist = myDist
                        If otherCase.RRData.PavedSTO.Records.ContainsKey(otherPV.sd.Trim.ToUpper) Then
                            otherSTOR = otherCase.RRData.PavedSTO.Records(otherPV.sd.Trim.ToUpper)
                            ModelIdx = i
                        End If
                    End If
                Next
            Next
            'we hebben alle andere cases doorlopen, dus schrijf nu de gevonden ERNST-definitie naar het doelmodel
            'voeg het indexnummer van de broncase toe aan het ID voor het geval dezelfde draindef in meerdere bronmodellen voorkomt
            activeSTOR = New clsPavedSTORecord(Setup)
            activeSTOR.ID = otherSTOR.ID & "_" & Str(ModelIdx).Trim
            activePV.sd = activeSTOR.ID

            'kopieer de attributen van de gevonden drainagedefinitie
            activeSTOR.nm = otherSTOR.nm
            activeSTOR.ms = otherSTOR.ms
            activeSTOR.is_ = otherSTOR.is_
            activeSTOR.mr1 = otherSTOR.mr1
            activeSTOR.mr2 = otherSTOR.mr2
            activeSTOR.ir1 = otherSTOR.ir1
            activeSTOR.ir2 = otherSTOR.ir2

            If Not PavedSTO.Records.ContainsKey(activeSTOR.ID.Trim.ToUpper) Then PavedSTO.Records.Add(activeSTOR.ID.Trim.ToUpper, activeSTOR)
        Next
    End Sub


    Friend Sub getDWADefsFromOtherCases(ByRef Cases As List(Of ClsSobekCase))
        Dim minDist As Double
        Dim myDist As Double
        Dim activePV As clsPaved3BRecord
        Dim otherPV As clsPaved3BRecord
        Dim otherDWA As clsPavedDWARecord = Nothing
        Dim activeDWA As clsPavedDWARecord

        Dim i As Integer, ModelIdx As Integer

        'gooi de bestaande DWAage definities weg
        PavedDWA.Records = New Dictionary(Of String, clsPavedDWARecord)
        PavedDWA.FileContent = Nothing

        'Doorloop alle knopen en zoek het dichtstbijgelegen exemplaar
        For Each activePV In Paved3B.Records.Values
            minDist = 99999999
            i = 0
            For Each otherCase In Cases
                i += 1
                'zoek nu de XY-coordinaat op van elke RR Paved knoop en bepaal of die het dichtst bij ligt
                For Each otherPV In otherCase.RRData.Paved3B.Records.Values
                    myDist = Math.Sqrt((activePV.X - otherPV.X) ^ 2 + (activePV.Y - otherPV.Y) ^ 2)
                    If myDist < minDist Then
                        minDist = myDist
                        If otherCase.RRData.PavedDWA.Records.ContainsKey(otherPV.dw.Trim.ToUpper) Then
                            otherDWA = otherCase.RRData.PavedDWA.Records(otherPV.dw.Trim.ToUpper)
                            ModelIdx = i
                        End If
                    End If
                Next
            Next
            'we hebben alle andere cases doorlopen, dus schrijf nu de gevonden ERNST-definitie naar het doelmodel
            'voeg het indexnummer van de broncase toe aan het ID voor het geval dezelfde draindef in meerdere bronmodellen voorkomt
            activeDWA = New clsPavedDWARecord(Setup)
            activeDWA.ID = otherDWA.ID & "_" & Str(ModelIdx).Trim
            activePV.dw = activeDWA.ID

            'kopieer de attributen van de gevonden drainagedefinitie
            activeDWA.nm = otherDWA.nm
            activeDWA.do_ = otherDWA.do_
            activeDWA.wc = otherDWA.wc
            activeDWA.wd = otherDWA.wd
            For i = 0 To 23
                activeDWA.wh(i) = otherDWA.wh(i)
            Next
            activeDWA.sc = otherDWA.sc
            If Not PavedDWA.Records.ContainsKey(activeDWA.ID.Trim.ToUpper) Then PavedDWA.Records.Add(activeDWA.ID.Trim.ToUpper, activeDWA)
        Next
    End Sub

    Friend Sub getINFDefsFromOtherCases(ByRef Cases As List(Of ClsSobekCase))
        Dim minDist As Double
        Dim myDist As Double
        Dim activeUP As clsUnpaved3BRecord
        Dim otherUP As clsUnpaved3BRecord
        Dim otherINF As clsUnpavedINFRecord = Nothing
        Dim activeINF As clsUnpavedINFRecord

        Dim i As Integer, ModelIdx As Integer

        'gooi de bestaande INFage definities weg
        UnpavedInf.Records = New Dictionary(Of String, clsUnpavedINFRecord)
        UnpavedInf.FileContent = Nothing

        'Doorloop alle knopen en zoek het dichtstbijgelegen exemplaar
        For Each activeUP In Unpaved3B.Records.Values
            minDist = 99999999
            i = 0
            For Each otherCase In Cases
                i += 1
                'zoek nu de XY-coordinaat op van elke RR Unpaved knoop en bepaal of die het dichtst bij ligt
                For Each otherUP In otherCase.RRData.Unpaved3B.Records.Values
                    myDist = Math.Sqrt((activeUP.X - otherUP.X) ^ 2 + (activeUP.Y - otherUP.Y) ^ 2)
                    If myDist < minDist Then
                        minDist = myDist
                        If otherCase.RRData.UnpavedInf.Records.ContainsKey(otherUP.ic.Trim.ToUpper) Then
                            otherINF = otherCase.RRData.UnpavedInf.Records(otherUP.ic.Trim.ToUpper)
                            ModelIdx = i
                        End If
                    End If
                Next
            Next
            'we hebben alle andere cases doorlopen, dus schrijf nu de gevonden ERNST-definitie naar het doelmodel
            'voeg het indexnummer van de broncase toe aan het ID voor het geval dezelfde draindef in meerdere bronmodellen voorkomt
            activeINF = New clsUnpavedINFRecord(Setup)
            activeINF.ID = otherINF.ID & "_" & Str(ModelIdx).Trim
            activeINF.nm = otherINF.nm
            activeINF.ic = otherINF.ic
            activeUP.ic = activeINF.ID
            If Not UnpavedInf.Records.ContainsKey(activeINF.ID.Trim.ToUpper) Then UnpavedInf.Records.Add(activeINF.ID.Trim.ToUpper, activeINF)
        Next
    End Sub

    Friend Sub getSeepageFromOtherCases(ByRef Cases As List(Of ClsSobekCase))
        Dim minDist As Double
        Dim myDist As Double
        Dim activeUP As clsUnpaved3BRecord
        Dim otherUP As clsUnpaved3BRecord
        Dim otherSEEP As clsUnpavedSEPRecord = Nothing
        Dim activeSEEP As clsUnpavedSEPRecord

        Dim i As Integer, ModelIdx As Integer

        'gooi de bestaande seepdefinities weg
        UnpavedSep.Records = New Dictionary(Of String, clsUnpavedSEPRecord)
        UnpavedSep.FileContent = Nothing

        'Doorloop alle knopen en zoek het dichtstbijgelegen exemplaar
        For Each activeUP In Unpaved3B.Records.Values
            minDist = 99999999
            i = 0
            For Each otherCase In Cases
                i += 1
                'zoek nu de XY-coordinaat op van elke RR Unpaved knoop en bepaal of die het dichtst bij ligt
                For Each otherUP In otherCase.RRData.Unpaved3B.Records.Values
                    myDist = Math.Sqrt((activeUP.X - otherUP.X) ^ 2 + (activeUP.Y - otherUP.Y) ^ 2)
                    If myDist < minDist Then
                        minDist = myDist
                        If otherCase.RRData.UnpavedSep.Records.ContainsKey(otherUP.SP.Trim.ToUpper) Then
                            otherSEEP = otherCase.RRData.UnpavedSep.Records(otherUP.SP.Trim.ToUpper)
                            ModelIdx = i
                        End If
                    End If
                Next
            Next
            'we hebben alle andere cases doorlopen, dus schrijf nu de gevonden ERNST-definitie naar het doelmodel
            'voeg het indexnummer van de broncase toe aan het ID voor het geval dezelfde draindef in meerdere bronmodellen voorkomt
            activeSEEP = New clsUnpavedSEPRecord(Setup)
            activeSEEP.ID = otherSEEP.ID & "_" & Str(ModelIdx).Trim
            activeUP.SP = activeSEEP.ID

            'kopieer de attributen van de gevonden drainagedefinitie
            activeSEEP.co = otherSEEP.co
            activeSEEP.cv = otherSEEP.cv
            activeSEEP.nm = otherSEEP.nm
            activeSEEP.sp = otherSEEP.sp
            activeSEEP.ss = otherSEEP.ss

            If Not UnpavedSep.Records.ContainsKey(activeSEEP.ID.Trim.ToUpper) Then UnpavedSep.Records.Add(activeSEEP.ID.Trim.ToUpper, activeSEEP)

        Next


    End Sub

    Friend Sub getPavedParametersFromOtherCases(ByVal Cases As List(Of ClsSobekCase), ByVal Caps As Boolean, ByVal PumpDirections As Boolean, ByVal ms As Boolean)
        Dim minDist As Double
        Dim myDist As Double
        Dim activePV As clsPaved3BRecord
        Dim otherPV As clsPaved3BRecord

        'Doorloop alle knopen en zoek het dichtstbijgelegen exemplaar
        For Each activePV In Paved3B.Records.Values
            minDist = 99999999
            For Each otherCase In Cases
                'zoek nu de XY-coordinaat op van elke RR Unpaved knoop en bepaal of die het dichtst bij ligt
                For Each otherPV In otherCase.RRData.Paved3B.Records.Values
                    myDist = Math.Sqrt((activePV.X - otherPV.X) ^ 2 + (activePV.Y - otherPV.Y) ^ 2)
                    If myDist < minDist Then
                        minDist = myDist
                        If ms Then activePV.ms = otherPV.ms
                        If Caps Then
                            activePV.qc = otherPV.qc
                            activePV.MixedCap = otherPV.MixedCap
                            activePV.DWFCap = otherPV.DWFCap
                        End If
                        If PumpDirections Then
                            activePV.qo1 = otherPV.qo1
                            activePV.qo2 = otherPV.qo2
                        End If
                    End If
                Next
            Next
        Next
    End Sub


    Friend Sub getUnpavedParametersFromOtherCases(ByRef Cases As List(Of ClsSobekCase), ByVal ms As Boolean, ByVal ig As Boolean, ByVal bt As Boolean)
        Dim minDist As Double
        Dim myDist As Double
        Dim activeUP As clsUnpaved3BRecord
        Dim otherUP As clsUnpaved3BRecord

        'Doorloop alle knopen en zoek het dichtstbijgelegen exemplaar
        For Each activeUP In Unpaved3B.Records.Values
            minDist = 99999999
            For Each otherCase In Cases
                'zoek nu de XY-coordinaat op van elke RR Unpaved knoop en bepaal of die het dichtst bij ligt
                For Each otherUP In otherCase.RRData.Unpaved3B.Records.Values
                    myDist = Math.Sqrt((activeUP.X - otherUP.X) ^ 2 + (activeUP.Y - otherUP.Y) ^ 2)
                    If myDist < minDist Then
                        minDist = myDist
                        If ms Then activeUP.ms = otherUP.ms
                        If bt Then activeUP.bt = otherUP.bt
                        If ig Then
                            activeUP.ig = otherUP.ig
                            activeUP.igconst = otherUP.igconst
                            activeUP.igtable = otherUP.igtable
                        End If
                    End If
                Next
            Next
        Next

    End Sub


    Public Enum enmNodetype
        NodeRRUnpaved = 43
        NodeRRsacramento = 54
        NodeRRPaved = 42
        NodeRRWWTP = 56
        NodeRROpenWater = 45
        NodeRRGreenhouse = 44
        NodeRRBoundary = 46
        NodeRRPump = 47
        NodeRRIndustry = 48
        NodeRRCFConnectionNode = 35
        NodeRRCFConnection = 34
        NodeRRWeir = 49
    End Enum

End Class
