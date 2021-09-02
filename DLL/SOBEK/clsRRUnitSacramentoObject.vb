Imports STOCHLIB.General

Public Class clsRRUnitSacramentoObject
    'this object contains everything for a unit RR object of the unpaved type

    Public ID As String
    Public CatchmentID As String

    'a list of all underlying RR nodes
    Friend RRNodeTpRecords As New Dictionary(Of String, clsRRNodeTPRecord)

    Dim x As Double, y As Double
    Dim ar As Double, aaf As Double

    'the CAPS variables
    Dim uztwm As Double, uztwc As Double, uzfwm As Double, uzfwc As Double, lztwm As Double, lztwc As Double, lzfsm As Double, lzfsc As Double, lzfpm As Double, lzfpc As Double, uzk As Double, lzsk As Double, lzpk As Double

    'the OPAR variables
    Dim zperc As Double, rexp As Double, pfree As Double, rserv As Double, pctim As Double, adimp As Double, sarva As Double, side As Double, ssout As Double, pm As Double, pt1 As Double, pt2 As Double

    'the UNIH variables
    Dim dt As Integer, uh As New List(Of Double)
    Dim bnd As Double    'downstream boundary

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Setup = mySetup
        SbkCase = myCase
    End Sub

    Public Function ScaleToUnitSize(UnitAreaM2 As Double) As Boolean
        ar = UnitAreaM2
    End Function

    Public Function GetArea() As Double
        Return ar
    End Function

    Public Function Calculate() As Boolean
        Try

            'here we'll establish an average sacramento profile for the unit type that represents a whole range of sacramento nodes
            Dim n As Long, i As Integer
            Dim SACRSACR As clsSACRMNTO3BSACRRecord, SACRCAPS As clsSACRMNTO3BCAPSRecord, SACROPAR As clsSACRMNTO3BOPARRecord, SACRUNIH As clsSACRMNTO3BUNIHRecord
            Dim BND3B As clsBound3B3BRecord, BNDNode As clsRRNodeTPRecord

            For Each myNode As clsRRNodeTPRecord In RRNodeTpRecords.Values
                n += 1
                SACRSACR = SbkCase.RRData.Sacr3BSACR.GetRecord(myNode.ID)
                SACRCAPS = SbkCase.RRData.Sacr3BCAPS.Records.Item(SACRSACR.ca.Trim.ToUpper)
                SACROPAR = SbkCase.RRData.Sacr3BOPAR.GetRecord(SACRSACR.op.Trim.ToUpper)
                SACRUNIH = SbkCase.RRData.Sacr3BUNIH.GetRecord(SACRSACR.uh.Trim.ToUpper)
                BNDNode = SbkCase.RRTopo.getDownstreamNode(myNode.ID.Trim.ToUpper)
                BND3B = SbkCase.RRData.Bound3B3B.Records.Item(BNDNode.ID.Trim.ToUpper)

                x += myNode.X
                y += myNode.Y
                bnd += BND3B.bl2

                'the SACR variables
                ar += SACRSACR.ar
                aaf += SACRSACR.aaf

                'the CAPS variables
                uztwm += SACRCAPS.uztwm
                uztwc += SACRCAPS.uztwc
                uzfwm += SACRCAPS.uzfwm
                uzfwc += SACRCAPS.uzfwc
                lztwm += SACRCAPS.lztwm
                lztwc += SACRCAPS.lztwc
                lzfsm += SACRCAPS.lzfsm
                lzfsc += SACRCAPS.lzfsc
                lzfpm += SACRCAPS.lzfpm
                lzfpc += SACRCAPS.lzfpc
                uzk += SACRCAPS.uzk
                lzsk += SACRCAPS.lzsk
                lzpk += SACRCAPS.lzpk

                'the OPAR variables
                zperc += SACROPAR.zperc
                rexp += SACROPAR.rexp
                pfree += SACROPAR.pfree
                rserv += SACROPAR.rserv
                pctim += SACROPAR.pctim
                adimp += SACROPAR.adimp
                sarva += SACROPAR.sarva
                side += SACROPAR.side
                ssout += SACROPAR.ssout
                pm += SACROPAR.pm
                pt1 += SACROPAR.pt1
                pt2 += SACROPAR.pt2

                'the UNIH variables
                dt = SACRUNIH.dt
                For i = 0 To SACRUNIH.uh.Count - 1
                    If i > uh.Count - 1 Then uh.Add(0)
                    uh.Item(i) += SACRUNIH.uh.Item(i)
                Next
            Next

            'now that we've searched all members we can create the final files
            x = x / n
            y = y / n
            aaf = aaf / n
            bnd = bnd / n

            'the CAPS variables
            uztwm = uztwm / n
            uztwc = uztwc / n
            uzfwm = uzfwm / n
            uzfwc = uzfwc / n
            lztwm = lztwm / n
            lztwc = lztwc / n
            lzfsm = lzfsm / n
            lzfsc = lzfsc / n
            lzfpm = lzfpm / n
            lzfpc = lzfpc / n
            uzk = uzk / n
            lzsk = lzsk / n
            lzpk = lzpk / n

            'the OPAR variables
            zperc = zperc / n
            rexp = rexp / n
            pfree = pfree / n
            rserv = rserv / n
            pctim = pctim / n
            adimp = adimp / n
            sarva = sarva / n
            side = side / n
            ssout = ssout / n
            pm = pm / n
            pt1 = pt1 / n
            pt2 = pt2 / n


            'the UNIH variables
            dt = dt
            For i = 0 To uh.Count - 1
                uh.Item(i) = uh.Item(i) / n
            Next

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
            Dim SACRSACR As clsSACRMNTO3BSACRRecord, SACRCAPS As clsSACRMNTO3BCAPSRecord, SACROPAR As clsSACRMNTO3BOPARRecord, SACRUNIH As clsSACRMNTO3BUNIHRecord, BND3B As clsBound3B3BRecord

            '---------------------------------------------------------------------------------------------------------
            '               TOPO
            '---------------------------------------------------------------------------------------------------------
            newUP = SbkCase.RRTopo.GetAddNode(ID, x, y, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_SACRAMENTO"), True)
            newBND = SbkCase.RRTopo.GetAddNode("bnd" & ID, x + 100, y, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_BOUNDARY"), True)
            newLink = SbkCase.RRTopo.GetAddLink("lnk" & ID, newUP.ID, newBND.ID, Me.Setup.SOBEKData.ActiveProject.ActiveCase.BranchTypes.GetByParentID("3B_LINK"), True)

            '---------------------------------------------------------------------------------------------------------
            '               DATA
            '---------------------------------------------------------------------------------------------------------
            'get Or create the New unpaved.3b record
            SACRSACR = SbkCase.RRData.Sacr3BSACR.GetAddRecord(ID)

            SACRCAPS = New clsSACRMNTO3BCAPSRecord(Me.Setup)
            SACRCAPS.Create(Setup, ID, ID, uztwm, uztwc, uzfwm, uzfwc, lztwm, lztwc, lzfsm, lzfsc, lzfpm, lzfpc, uzk, lzsk, lzpk)
            SbkCase.RRData.Sacr3BCAPS.Records.Add(SACRCAPS.ID.Trim.ToUpper, SACRCAPS)

            SACROPAR = New clsSACRMNTO3BOPARRecord(Me.Setup)
            SACROPAR.Create(Setup, ID, ID, zperc, rexp, pfree, rserv, pctim, adimp, sarva, side, ssout, pm, pt1, pt2)
            SbkCase.RRData.Sacr3BOPAR.Records.Add(SACROPAR.ID.Trim.ToUpper, SACROPAR)

            SACRUNIH = New clsSACRMNTO3BUNIHRecord(Me.Setup)
            SACRUNIH.Create(Setup, ID, ID, dt, uh)
            SbkCase.RRData.Sacr3BUNIH.Records.Add(SACRUNIH.ID.Trim.ToUpper, SACRUNIH)

            'complete the SACR record
            SACRSACR.InUse = True
            SACRSACR.ID = ID
            SACRSACR.Name = ID
            SACRSACR.ar = ar
            SACRSACR.ms = CatchmentID
            SACRSACR.aaf = aaf
            SACRSACR.ca = SACRCAPS.ID
            SACRSACR.op = SACROPAR.ID
            SACRSACR.uh = SACRUNIH.ID

            BND3B = New clsBound3B3BRecord(Me.Setup)
            BND3B.Create(Setup, "bnd" & ID, bnd)
            SbkCase.RRData.Bound3B3B.Records.Add(BND3B.ID.Trim.ToUpper, BND3B)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function Build of class clsRRUnitSacramentoObject.")
            Return False
        End Try

    End Function
End Class
