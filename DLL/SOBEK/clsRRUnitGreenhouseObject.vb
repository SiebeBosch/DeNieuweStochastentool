Imports STOCHLIB.General

Public Class clsRRUnitGreenhouseObject
    'this object contains everything for a unit RR object of the unpaved type

    Public ID As String
    Public CatchmentID As String

    'a list of all underlying RR nodes
    Friend RRNodeTpRecords As New Dictionary(Of String, clsRRNodeTPRecord)

    Dim x As Double, y As Double

    'greenhse.3b
    Dim na As Integer, ar As New List(Of Double), sl As Double, as_ As Integer, aaf As Double, is_ As Integer, ms As String, si As String

    'greenhse.rf
    Dim mk As Double, ik As Double

    Dim bnd As Double    'downstream boundary

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Setup = mySetup
        SbkCase = myCase
    End Sub

    Public Function GetAreasList() As List(Of Double)
        Return ar
    End Function

    Public Function GetTotalArea() As Double
        Dim myTotal As Double
        For Each item In ar
            myTotal += item
        Next
        Return myTotal
    End Function

    Public Function ScaleToUnitSize(UnitAreaM2 As Double) As Boolean
        Dim totalLandUseArea As Double = GetTotalArea()
        Dim multiplier = UnitAreaM2 / totalLandUseArea
        Dim i As Long
        For i = 0 To ar.Count - 1
            ar.Item(i) *= multiplier
        Next
    End Function

    Public Function Calculate() As Boolean
        Try
            'here we'll establish an average unpaved profile for the unit type that represents a large range of unpaved nodes
            Dim n As Long
            Dim GR3B As clsGreenhse3BRecord, GRHF As clsGreenHseRFRecord
            Dim BND3B As clsBound3B3BRecord, BNDNode As clsRRNodeTPRecord

            For Each myNode As clsRRNodeTPRecord In RRNodeTpRecords.Values
                n += 1
                GR3B = SbkCase.RRData.Greenhse3B.GetRecord(myNode.ID)
                GRHF = SbkCase.RRData.GreenhseRF.Records.Item(GR3B.sd.Trim.ToUpper)
                BNDNode = SbkCase.RRTopo.getDownstreamNode(myNode.ID.Trim.ToUpper)
                BND3B = SbkCase.RRData.Bound3B3B.Records.Item(BNDNode.ID.Trim.ToUpper)

                x += myNode.X
                y += myNode.Y
                bnd += BND3B.bl2

                'the greenhse.3b data
                na = GR3B.na
                For i = 0 To na - 1
                    If i > ar.Count - 1 Then ar.Add(0)
                    If i > GR3B.ar.Count - 1 Then GR3B.ar.Add(0)
                Next
                For i = 0 To na - 1
                    ar.Item(i) += GR3B.ar.Item(i)
                Next
                sl += GR3B.sl
                as_ += GR3B.as_
                aaf += GR3B.aaf
                is_ = 0

                'the greenhse.rf data (roof storage)
                mk += GRHF.mk
                ik += GRHF.ik

            Next

            'now that we've searched all members we can create the final files
            x = x / n
            y = y / n
            bnd = bnd / n

            sl = sl / n
            ms = CatchmentID
            mk = mk / n
            ik = ik / n
            si = ""
            aaf = aaf / n

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function Build(ByRef SbkCase As clsSobekCase) As Boolean
        Try
            Dim newGR As clsRRNodeTPRecord
            Dim newBND As clsRRNodeTPRecord
            Dim newLink As clsRRBrchTPRecord
            Dim GR3B As clsGreenhse3BRecord, GRFH As clsGreenHseRFRecord, BND3B As clsBound3B3BRecord

            '---------------------------------------------------------------------------------------------------------
            '               TOPO
            '---------------------------------------------------------------------------------------------------------
            newGR = SbkCase.RRTopo.GetAddNode(ID, x, y, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_GREENHOUSE"), True)
            newBND = SbkCase.RRTopo.GetAddNode("bnd" & ID, x + 100, y, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_BOUNDARY"), True)
            newLink = SbkCase.RRTopo.GetAddLink("lnk" & ID, newGR.ID, newBND.ID, Me.Setup.SOBEKData.ActiveProject.ActiveCase.BranchTypes.GetByParentID("3B_LINK"), True)

            '---------------------------------------------------------------------------------------------------------
            '               DATA
            '---------------------------------------------------------------------------------------------------------
            'get Or create the New unpaved.3b record
            GR3B = SbkCase.RRData.Greenhse3B.GetAddRecord(ID)

            GRFH = New clsGreenHseRFRecord(Me.Setup)
            GRFH.Create(Setup, ID, ID, mk, ik)
            SbkCase.RRData.GreenhseRF.Records.Add(GRFH.ID.Trim.ToUpper, GRFH)

            GR3B.InUse = True
            GR3B.na = na
            For i = 0 To ar.Count - 1
                GR3B.ar.Add(ar.Item(i))
            Next
            GR3B.sl = sl
            GR3B.as_ = as_
            GR3B.si = ""
            GR3B.sd = GRFH.ID
            GR3B.ms = CatchmentID
            GR3B.aaf = aaf
            GR3B.isConc = is_

            BND3B = New clsBound3B3BRecord(Me.Setup)
            BND3B.Create(Setup, "bnd" & ID, bnd)
            SbkCase.RRData.Bound3B3B.Records.Add(BND3B.ID.Trim.ToUpper, BND3B)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function Build of class clsRRUnitGreenhouseObject.")
        End Try

    End Function
End Class
