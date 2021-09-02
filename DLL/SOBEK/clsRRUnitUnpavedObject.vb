Imports STOCHLIB.General

Public Class clsRRUnitUnpavedObject
    'this object contains everything for a unit RR object of the unpaved type

    Public ID As String
    Public CatchmentID As String
    Public bt As Integer
    Public FreeboardID As String

    'a list of all underlying RR nodes
    Friend RRNodeTpRecords As New Dictionary(Of String, clsRRNodeTPRecord)

    Dim x As Double, y As Double
    Dim lv As Double, ga As Double, ar As New Dictionary(Of Integer, Double), ig As Double, gl As Double, mg As Double
    Dim sp As Double, co As Integer = 1, ss As Double = 0, cv As Double = 0    'seepage. we assume constant seepage
    Dim ic As Double    'inf
    Dim ml As Double, il As Double = 0    'storage
    Dim lv1 As Double, lv2 As Double, lv3 As Double    'drainage depths
    Dim cvo1 As Double, cvo2 As Double, cvo3 As Double, cvo4 As Double, cvi As Double, cvs As Double 'drainage values
    Dim bnd As Double    'downstream boundary

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Setup = mySetup
        SbkCase = myCase
    End Sub

    Public Function ScaleToUnitSize(UnitAreaM2 As Double) As Boolean
        Dim totalLandUseArea As Double
        For i = 1 To 16
            totalLandUseArea += ar.Item(i)
        Next
        Dim multiplier = UnitAreaM2 / totalLandUseArea

        ga *= multiplier
        For i = 1 To 16
            ar.Item(i) *= multiplier
        Next
    End Function

    Public Function Calculate(EnforcedMinimumFreeboard As Double, LimitGroundwaterAreaToUnitSize As Boolean) As Boolean
        Try

            'here we'll establish an average unpaved profile for the unit type that represents a large range of unpaved nodes
            Dim n As Long, i As Integer
            Dim UP3B As clsUnpaved3BRecord, UPSEP As clsUnpavedSEPRecord, UPINF As clsUnpavedINFRecord, UPSTO As clsUnpavedSTORecord, UPALF As clsUnpavedALFERNSRecord
            Dim BND3B As clsBound3B3BRecord, BNDNode As clsRRNodeTPRecord

            Dim TotalArea As Double = 0
            Dim GroundwaterArea As Double = 0
            ar = New Dictionary(Of Integer, Double)
            For i = 1 To 16
                ar.Add(i, 0)
            Next

            'new in v1.796: area-weighed surface level and boundary value
            For Each myNode As clsRRNodeTPRecord In RRNodeTpRecords.Values
                UP3B = SbkCase.RRData.Unpaved3B.GetRecord(myNode.ID)
                TotalArea += UP3B.getTotalLandUseArea
                GroundwaterArea += UP3B.ga
            Next

            For Each myNode As clsRRNodeTPRecord In RRNodeTpRecords.Values

                n += 1
                UP3B = SbkCase.RRData.Unpaved3B.GetRecord(myNode.ID)
                UPSEP = SbkCase.RRData.UnpavedSep.Records.Item(UP3B.SP.Trim.ToUpper)
                UPALF = SbkCase.RRData.UnpavedAlf.ERNSRecords.Item(UP3B.ed.Trim.ToUpper)
                UPINF = SbkCase.RRData.UnpavedInf.Records.Item(UP3B.ic.Trim.ToUpper)
                UPSTO = SbkCase.RRData.UnpavedSto.Records.Item(UP3B.sd.Trim.ToUpper)
                BNDNode = SbkCase.RRTopo.getDownstreamNode(myNode.ID.Trim.ToUpper)
                BND3B = SbkCase.RRData.Bound3B3B.Records.Item(BNDNode.ID.Trim.ToUpper)

                bt = UP3B.bt

                Dim NodeArea As Double = 0
                For i = 1 To 16
                    ar.Item(i) += UP3B.ar.Item(i)
                    NodeArea += UP3B.ar.Item(i)
                Next

                x += myNode.X
                y += myNode.Y
                lv += UP3B.lv * NodeArea
                ga += UP3B.ga
                ig += UP3B.igconst
                gl += UP3B.gl
                mg += UP3B.mg

                sp += UPSEP.sp
                ml += UPSTO.ml

                lv1 += UPALF.lv1
                lv2 += UPALF.lv2
                lv3 += UPALF.lv3
                cvo1 += UPALF.cvo1
                cvo2 += UPALF.cvo2
                cvo3 += UPALF.cvo3
                cvo4 += UPALF.cvo4
                cvs += UPALF.cvs
                cvi += UPALF.cvi

                ic += UPINF.ic

                bnd += SbkCase.RRData.getBoundaryLevel(BND3B.ID, GeneralFunctions.enmHydroMathOperation.MIN) * NodeArea
            Next

            'now that we've searched all members we can create the final files
            x = x / n
            y = y / n

            sp = sp / n
            ic = ic / n

            ml = ml / n

            lv1 = lv1 / n
            lv2 = lv2 / n
            lv3 = lv3 / n
            cvo1 = cvo1 / n
            cvo2 = cvo2 / n
            cvo3 = cvo3 / n
            cvo4 = cvo4 / n
            cvi = cvi / n
            cvs = cvs / n

            lv = lv / TotalArea 'v1.796: introducing area-weighted surface level
            ig = ig / n
            gl = gl / n
            mg = mg / n

            bnd = bnd / TotalArea 'v1.796: introducing area-weighted boundary condition

            'v1.796: introducing enforced minimum freeboard
            If (lv - bnd) < EnforcedMinimumFreeboard Then
                bnd = lv - EnforcedMinimumFreeboard
            End If

            If ga > TotalArea AndAlso LimitGroundwaterAreaToUnitSize Then
                ga = TotalArea
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function Build(ByRef SbkCase As clsSobekCase) As Boolean
        Try
            Dim newUP As clsRRNodeTPRecord
            Dim newBND As clsRRNodeTPRecord
            Dim newLink As clsRRBrchTPRecord
            Dim UP3B As clsUnpaved3BRecord, UPSep As clsUnpavedSEPRecord, UPINF As clsUnpavedINFRecord, UPALF As clsUnpavedALFERNSRecord, UPSTO As clsUnpavedSTORecord, BND3B As clsBound3B3BRecord

            '---------------------------------------------------------------------------------------------------------
            '               TOPO
            '---------------------------------------------------------------------------------------------------------
            newUP = SbkCase.RRTopo.GetAddNode(ID, x, y, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_UNPAVED"), True)
            newBND = SbkCase.RRTopo.GetAddNode("bnd" & ID, x + 100, y, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_BOUNDARY"), True)
            newLink = SbkCase.RRTopo.GetAddLink("lnk" & ID, newUP.ID, newBND.ID, Me.Setup.SOBEKData.ActiveProject.ActiveCase.BranchTypes.GetByParentID("3B_LINK"), True)

            '---------------------------------------------------------------------------------------------------------
            '               DATA
            '---------------------------------------------------------------------------------------------------------
            'get Or create the New unpaved.3b record
            UP3B = SbkCase.RRData.Unpaved3B.GetAddRecord(ID)

            UPSep = New clsUnpavedSEPRecord(Me.Setup)
            UPSep.Create(Setup, ID, ID, co, cv, sp, ss)
            If Not SbkCase.RRData.UnpavedSep.Records.ContainsKey(UPSep.ID.Trim.ToUpper) Then
                SbkCase.RRData.UnpavedSep.Records.Add(UPSep.ID.Trim.ToUpper, UPSep)
            Else
                Me.Setup.Log.AddWarning("Unpaved.sep record with id " & UPSep.ID & " was already in the collection.")
            End If

            UPSTO = New clsUnpavedSTORecord(Me.Setup)
            UPSTO.Create(Setup, ID, ID, ml, il)
            If Not SbkCase.RRData.UnpavedSto.Records.ContainsKey(UPSTO.ID.Trim.ToUpper) Then
                SbkCase.RRData.UnpavedSto.Records.Add(UPSTO.ID.Trim.ToUpper, UPSTO)
            Else
                Me.Setup.Log.AddWarning("Unpaved.sto record with id " & UPSTO.ID & " was already in the collection.")
            End If

            UPINF = New clsUnpavedINFRecord(Me.Setup)
            UPINF.Create(Setup, ID, ID, ic)
            If Not SbkCase.RRData.UnpavedInf.Records.ContainsKey(UPINF.ID.Trim.ToUpper) Then
                SbkCase.RRData.UnpavedInf.Records.Add(UPINF.ID.Trim.ToUpper, UPINF)
            Else
                Me.Setup.Log.AddWarning("Unpaved.inf record with id " & UPINF.ID & " was already in the collection.")
            End If

            UPALF = New clsUnpavedALFERNSRecord(Me.Setup)
            UPALF.Create(Setup, ID, ID, cvi, cvo1, cvo2, cvo3, cvo4, lv1, lv2, lv3, cvs)
            If Not SbkCase.RRData.UnpavedAlf.ERNSRecords.ContainsKey(UPALF.ID.Trim.ToUpper) Then
                SbkCase.RRData.UnpavedAlf.ERNSRecords.Add(UPALF.ID.Trim.ToUpper, UPALF)
            Else
                Me.Setup.Log.AddWarning("Unpaved.alf record with id " & UPALF.ID & " was already in the collection.")
            End If

            'UP3B Or create the  downstream node
            UP3B.InUse = True
            UP3B.lv = lv
            UP3B.bt = bt
            UP3B.ga = ga
            UP3B.ig = 0
            UP3B.igconst = ig
            UP3B.gl = gl
            UP3B.mg = mg
            For i As Integer = 1 To 16
                UP3B.ar.Item(i) += ar.Item(i)
            Next
            UP3B.sd = UPSTO.ID
            UP3B.ed = UPALF.ID
            UP3B.SP = UPSep.ID
            UP3B.ic = UPINF.ID
            UP3B.co = clsUnpaved3BRecord.enmCo.ernst
            UP3B.na = 16
            UP3B.aaf = 1
            UP3B.ms = CatchmentID

            BND3B = New clsBound3B3BRecord(Me.Setup)
            BND3B.Create(Setup, "bnd" & ID, bnd)
            If Not SbkCase.RRData.Bound3B3B.Records.ContainsKey(BND3B.ID.Trim.ToUpper) Then
                SbkCase.RRData.Bound3B3B.Records.Add(BND3B.ID.Trim.ToUpper, BND3B)
            Else
                Me.Setup.Log.AddWarning("Bound3b.3b record with ID " & BND3B.ID & " was already in the collection.")
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function Build of class clsRRUnitUnpavedObject.")
            Return False
        End Try

    End Function

End Class
