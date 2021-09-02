Imports STOCHLIB.General

Public Class clsRRUnitPavedObject
    'this object contains everything for a unit RR object of the unpaved type

    Public ID As String
    Public CatchmentID As String

    'a list of all underlying RR nodes
    Friend RRNodeTpRecords As New Dictionary(Of String, clsRRNodeTPRecord)

    Dim x As Double, y As Double
    Dim ar As Double, lv As Double, sd As String, ss As Integer, qc1 As Double, qcConst As Double, qcTable As String, qc3 As Double, qo1 As Integer, qo2 As Integer, meteo As String, aaf As Double, is_ As Double, np As Integer, dw As String, ro As Double, ru As Double, qh As String 'paved.3b
    Dim mr1 As Double, mr2 As Double, ir1 As Double, ir2 As Double, ms As Double 'paved.sto
    Dim do_ As Integer, wc As Double, wd As Double, sc As Double     'paved.dwa

    Dim bnd As Double    'downstream boundary

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Setup = mySetup
        SbkCase = myCase
    End Sub

    Public Function GetArea() As Double
        Return ar
    End Function

    Public Function ScaleToUnitSize(UnitAreaM2 As Double) As Boolean
        qcConst = qcConst * UnitAreaM2 / ar 'pumpcapacity is expressed in m3/s, so we'll have to scale that back too
        qc3 = qc3 * UnitAreaM2 / ar
        ar = UnitAreaM2
    End Function

    Public Function Calculate() As Boolean
        Try
            'here we'll establish an average unpaved profile for the unit type that represents a large range of unpaved nodes
            Dim n As Long
            Dim PV3B As clsPaved3BRecord, PVSTO As clsPavedSTORecord, PVDWA As clsPavedDWARecord
            Dim BND3B As clsBound3B3BRecord, BNDNode As clsRRNodeTPRecord

            For Each myNode As clsRRNodeTPRecord In RRNodeTpRecords.Values
                n += 1
                PV3B = SbkCase.RRData.Paved3B.GetRecord(myNode.ID)
                PVSTO = SbkCase.RRData.PavedSTO.Records.Item(PV3B.sd.Trim.ToUpper)
                PVDWA = SbkCase.RRData.PavedDWA.Records.Item(PV3B.dw.Trim.ToUpper)
                BNDNode = SbkCase.RRTopo.getDownstreamNode(myNode.ID.Trim.ToUpper)      'siebe nog aanpassen: beide nodes zoeken
                BND3B = SbkCase.RRData.Bound3B3B.Records.Item(BNDNode.ID.Trim.ToUpper)

                x += myNode.X
                y += myNode.Y

                'the paved.3b data
                ar += PV3B.ar
                lv += PV3B.lv
                qc1 = PV3B.qc
                If qc1 = 0 Then
                    qcConst += PV3B.MixedCap 'likewise
                Else
                    qcTable = PV3B.PumpCapTable     'siebe, some day add up the tables to get the total pump capacity...
                End If
                qc3 += PV3B.DWFCap
                qo1 = PV3B.qo1
                qo2 = PV3B.qo2
                meteo = CatchmentID
                aaf = 1
                is_ = 0
                np += PV3B.np   'number of people
                ro = PV3B.ro
                ru = PV3B.ru
                qh = PV3B.qh

                'the paved.sto data
                ms += PVSTO.ms   'max storage on streets (mm)
                is_ += PVSTO.is_ 'initial storage on streets (mm)
                mr1 += PVSTO.mr1 'max storage in mixed sewer (mm)
                mr2 += PVSTO.mr2 'max storage in dwf sewer (mm)
                ir1 += PVSTO.ir1 'init storage in mixed sewer (mm)
                ir2 += PVSTO.ir2 'init storage in dwf sewer (mm)

                'the paved.dwa data
                do_ = PVDWA.do_
                wc += PVDWA.wc
                wd += PVDWA.wd
                sc = 0

                bnd += SbkCase.RRData.getBoundaryLevel(BND3B.ID, GeneralFunctions.enmHydroMathOperation.MIN)
            Next

            'now that we've searched all members we can create the final files
            x = x / n
            y = y / n

            lv = lv / n
            qc3 = qc3 / n

            ms = ms / n
            is_ = is_ / n
            mr1 = mr1 / n
            mr2 = mr2 / n
            ir1 = ir1 / n
            ir2 = ir2 / n

            bnd = bnd / n

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function Build(ByRef SbkCase As clsSobekCase) As Boolean
        Try
            Dim newPV As clsRRNodeTPRecord, newWWTP As clsRRNodeTPRecord, bndWWTP As clsRRNodeTPRecord
            Dim newBND As clsRRNodeTPRecord
            Dim newLink As clsRRBrchTPRecord
            Dim PV3B As clsPaved3BRecord, PVSTO As clsPavedSTORecord, PVDWA As clsPavedDWARecord, BND3B As clsBound3B3BRecord
            Dim WWTP3B As clsWWTP3BRecord

            '---------------------------------------------------------------------------------------------------------
            '               TOPO
            '---------------------------------------------------------------------------------------------------------
            newWWTP = SbkCase.RRTopo.GetAddNode("AWZI", x - 1000, y, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_WWTP"), True)
            bndWWTP = SbkCase.RRTopo.GetAddNode("bndAWZI", x - 1000, y - 100, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_BOUNDARY"), True)
            newLink = SbkCase.RRTopo.GetAddLink("lnkAWZI", newWWTP.ID, bndWWTP.ID, Me.Setup.SOBEKData.ActiveProject.ActiveCase.BranchTypes.GetByParentID("3B_LINK_RWZI"), True)

            newPV = SbkCase.RRTopo.GetAddNode(ID, x, y, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_PAVED"), True)
            newBND = SbkCase.RRTopo.GetAddNode("bnd" & ID, x + 100, y, Me.Setup.SOBEKData.ActiveProject.ActiveCase.NodeTypes.GetByParentID("3B_BOUNDARY"), True)
            newLink = SbkCase.RRTopo.GetAddLink("lnk" & ID, newPV.ID, newBND.ID, Me.Setup.SOBEKData.ActiveProject.ActiveCase.BranchTypes.GetByParentID("3B_LINK"), True)
            newLink = SbkCase.RRTopo.GetAddLink("rwzi" & ID, newPV.ID, newWWTP.ID, Me.Setup.SOBEKData.ActiveProject.ActiveCase.BranchTypes.GetByParentID("3B_LINK_RWZI"), True)

            '---------------------------------------------------------------------------------------------------------
            '               DATA
            '---------------------------------------------------------------------------------------------------------
            'get Or create the New paved.3b record
            PV3B = SbkCase.RRData.Paved3B.GetAddRecord(ID)

            'and the wwtp + boundary records
            WWTP3B = SbkCase.RRData.WWTP3B.getAddRecord("AWZI")
            WWTP3B.tb = 0
            BND3B = SbkCase.RRData.Bound3B3B.getAddRecord(newBND.ID)
            BND3B.Create(Me.Setup, BND3B.ID, bnd)

            PVSTO = New clsPavedSTORecord(Me.Setup)
            PVSTO.Create(Setup, ID, ID, ms, is_, mr1, mr2, ir1, ir2)
            If Not SbkCase.RRData.PavedSTO.Records.ContainsKey(PVSTO.ID.Trim.ToUpper) Then
                SbkCase.RRData.PavedSTO.Records.Add(PVSTO.ID.Trim.ToUpper, PVSTO)
            Else
                Me.Setup.Log.AddWarning("A paved.sto record with ID " & PVSTO.ID & " was already part of the collection.")
            End If

            PVDWA = New clsPavedDWARecord(Me.Setup)
            PVDWA.Create(Setup, ID, ID, do_, wc, wd, sc)
            If Not SbkCase.RRData.PavedDWA.Records.ContainsKey(PVDWA.ID.Trim.ToUpper) Then
                SbkCase.RRData.PavedDWA.Records.Add(PVDWA.ID.Trim.ToUpper, PVDWA)
            Else
                Me.Setup.Log.AddWarning("A paved.dwa record with ID " & PVDWA.ID & " was already part of the collection.")
            End If

            'UP3B Or create the  downstream node
            PV3B.InUse = True
            PV3B.ar = ar
            PV3B.lv = lv
            PV3B.sd = PVSTO.ID
            PV3B.ss = ss
            PV3B.qc = qc1
            PV3B.MixedCap = qcConst
            PV3B.DWFCap = qc3
            PV3B.PumpCapTable = qcTable
            PV3B.qo1 = qo1
            PV3B.qo2 = qo2
            PV3B.ms = CatchmentID
            PV3B.is0 = is_
            PV3B.np = np
            PV3B.dw = PVDWA.ID
            PV3B.ro = ro
            PV3B.ru = ru
            PV3B.qh = qh

            BND3B = New clsBound3B3BRecord(Me.Setup)
            BND3B.Create(Setup, "bnd" & ID, bnd)
            If Not SbkCase.RRData.Bound3B3B.Records.ContainsKey(BND3B.ID.Trim.ToUpper) Then
                SbkCase.RRData.Bound3B3B.Records.Add(BND3B.ID.Trim.ToUpper, BND3B)
            Else
                Me.Setup.Log.AddWarning("A bound3b.3b record with ID " & BND3B.ID & " was already part of the collection.")
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function Build of class clsRRUnitPavedObject.")
            Return False
        End Try

    End Function

End Class
