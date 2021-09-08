Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsCFAttributeData

    Public TimeTables As clsCFTimeTables          'bevat alle tijdsafhankelijke tabellen van de flow-module
    Public LateralData As clsCFLateraldata        'bevat alle laterale data van de flowmodule (ook met storage)
    Public ProfileData As clsCFProfileData        'bevat alle profieldata van de flowmodule
    Public StructureData As clsCFStructureData    'bevat alle kunstwerkdata van de flowmodule
    Public FrictionData As clsCFFrictionData      'bevat alle ruwheidsdata van de flowmodule
    Public BoundaryData As clsCFBoundaryData      'bevat alle boundarydata van de flowmodule
    Public NodesData As clsCFNodesData            'bevat alle data uit nodes.dat
    Public InitialData As clsCFInitialData        'bevat alle initialisatiegegevens

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        TimeTables = New clsCFTimeTables(Me.Setup)          'bevat alle tijdsafhankelijke tabellen van de flow-module
        LateralData = New clsCFLateraldata(Me.Setup, Me.SbkCase)        'bevat alle laterale data van de flowmodule (ook met storage)
        ProfileData = New clsCFProfileData(Me.Setup, Me.SbkCase)        'bevat alle profieldata van de flowmodule
        StructureData = New clsCFStructureData(Me.Setup, Me.SbkCase)    'bevat alle kunstwerkdata van de flowmodule
        FrictionData = New clsCFFrictionData(Me.Setup, Me.SbkCase)      'bevat alle ruwheidsdata van de flowmodule
        BoundaryData = New clsCFBoundaryData(Me.Setup, Me.SbkCase)      'bevat alle boundarydata van de flowmodule
        NodesData = New clsCFNodesData(Me.Setup, Me.SbkCase)            'bevat alle data uit nodes.dat
        InitialData = New clsCFInitialData(Me.Setup, Me.SbkCase)        'bevat alle data uit initial.dat
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        TimeTables.AddPrefix(Prefix)
        LateralData.AddPrefix(Prefix)
        ProfileData.AddPrefix(Prefix)
        StructureData.AddPrefix(Prefix)
        FrictionData.AddPrefix(Prefix)
        BoundaryData.AddPrefix(Prefix)
        NodesData.AddPrefix(Prefix)
        InitialData.AddPrefix(Prefix)
    End Sub

    Public Function GetStructureLength(ID As String, ByRef AttributeLength As Double) As Boolean
        Try
            'this function returns the attribute length of a given structure
            'note: this only happens for bridges, culverts and siphons. Other structure types do not have  a structure length
            'get the struct.dat record
            Dim StrucDat As clsStructDatRecord = Nothing, StrucDef As clsStructDefRecord = Nothing, ContrDef As clsControlDefRecord = Nothing
            StructureData.GetStructureRecords(ID, StrucDat, StrucDef, ContrDef)
            If Not StrucDef Is Nothing Then
                Select Case StrucDef.ty
                    Case Is = 10    'culverts and siphons
                        AttributeLength = StrucDef.dl
                        Return True
                    Case Is = 12    'bridges
                        AttributeLength = StrucDef.dl
                        Return True
                    Case Else
                        AttributeLength = 0
                        Return True
                End Select
            End If
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError("Error retrieving structure definition for structure " & ID)
            Return False
        End Try
    End Function
    Public Function getMinMax(ByRef minZ As Double, ByRef maxZ As Double) As Boolean
        Try
            minZ = 9.0E+99
            maxZ = -9.0E+99
            Dim myZ As Double
            For Each ProfDat As clsProfileDatRecord In ProfileData.ProfileDatRecords.Records.Values
                Dim ProfDef As clsProfileDefRecord = ProfileData.ProfileDefRecords.Records.Item(ProfDat.di.Trim.ToUpper)
                myZ = ProfDat.rl + ProfDef.getMinimumElevation
                If myZ > -999 AndAlso myZ < minZ Then minZ = myZ        'we have included this safety for certain situations where the elevation was 999999
                myZ = ProfDat.rl + ProfDef.getMaximumElevation
                If myZ < 999 AndAlso myZ > maxZ Then maxZ = myZ        'we have included this safety for certain situations where the elevation was 999999
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Friend Function ImportAll(ByVal CaseDir As String, ByRef myModel As clsSobekCase) As Boolean
        Try
            Dim ProfileDat As New clsSobekDataFile(Me.Setup)
            Dim ProfileDef As New clsSobekDataFile(Me.Setup)
            Dim StructDat As New clsSobekDataFile(Me.Setup)
            Dim StructDef As New clsSobekDataFile(Me.Setup)
            Dim ValveTab As New clsSobekDataFile(Me.Setup)
            Dim BoundDat As New clsSobekDataFile(Me.Setup)
            Dim BoundLat As New clsSobekDataFile(Me.Setup)
            Dim LateralDat As New clsSobekDataFile(Me.Setup)
            Dim ControlDef As New clsSobekDataFile(Me.Setup)
            Dim FrictionDat As New clsSobekDataFile(Me.Setup)
            Dim NodesDat As New clsSobekDataFile(Me.Setup)
            Dim InitialDat As New clsSobekDataFile(Me.Setup)

            'belangrijk: eerst de .dat-records. Die checken namelijk op aanwezigheid in het netwerk
            ProfileData.ProfileDatRecords.Read(ProfileDat.Read(CaseDir & "\profile.dat", "CRSN"))
            StructureData.StructDatRecords.Read(StructDat.Read(CaseDir & "\struct.dat", "STRU"))

            'nu pas de def-records. Die moeten checken op bestaande verwijzingen vanuit profile.dat en struct.dat
            If Not ProfileData.ProfileDefRecords.Read(ProfileDef.Read(CaseDir & "\profile.def", "CRDS")) Then Throw New Exception("Error reading profile.def records.")
            StructureData.StructDefRecords.Read(StructDef.Read(CaseDir & "\struct.def", "STDS"))
            StructureData.ControlDefRecords.Read(ControlDef.Read(CaseDir & "\control.def", "CNTL"))
            StructureData.ValveTabRecords.Read(ValveTab.Read(CaseDir & "\valve.tab", "VLVE"))

            BoundaryData.BoundaryDatFLBORecords.Read(BoundDat.Read(CaseDir & "\boundary.dat", "FLBO"))
            If Not LateralData.LateralDatFLBRRecords.Read(LateralDat.Read(CaseDir & "\lateral.dat", "FLBR")) Then Me.Setup.Log.AddError("Error reading FLBR records from lateral.dat.")
            If Not LateralData.LateraldatFLNORecords.Read(LateralDat.Read(CaseDir & "\lateral.dat", "FLNO")) Then Me.Setup.Log.AddError("Error reading FLNO records from lateral.dat.")
            LateralData.BoundlatDatBTBLRecords.Read(BoundLat.Read(CaseDir & "\boundlat.dat", "BTBL"))
            NodesData.NodesDatNodeRecords.Read(NodesDat.Read(CaseDir & "\nodes.dat", "NODE"))
            FrictionData.FrictionDatBDFRRecords.Read(FrictionDat.Read(CaseDir & "\friction.dat", "BDFR"))
            FrictionData.FrictionDatCRFRRecords.Read(FrictionDat.Read(CaseDir & "\friction.dat", "CRFR"))
            FrictionData.FrictionDatSTFRRecords.Read(FrictionDat.Read(CaseDir & "\friction.dat", "STFR"))
            FrictionData.FrictionDatXRSTRecords.Read(FrictionDat.Read(CaseDir & "\friction.dat", "XRST"))
            FrictionData.FrictionDatD2FRRecords.Read(FrictionDat.Read(CaseDir & "\friction.dat", "D2FR"))
            InitialData.Read(InitialDat.Read(CaseDir & "\initial.dat", "FLIN"))
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ImportAll of class clsCFAttributeData.")
            Return False
        End Try

    End Function

    Public Function getNodesDatRecord(NodeID As String) As clsNodesDatNODERecord
        If NodesData.NodesDatNodeRecords.records.ContainsKey(NodeID.Trim.ToUpper) Then
            Return NodesData.NodesDatNodeRecords.records.Item(NodeID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Sub ImportCrossSectionData(ByVal CaseDir As String, ByRef myModel As clsSobekCase)
        Dim ProfileDat As New clsSobekDataFile(Me.Setup)
        Dim ProfileDef As New clsSobekDataFile(Me.Setup)

        ProfileData.ProfileDatRecords.Read(ProfileDat.Read(CaseDir & "\profile.dat", "CRSN"))
        ProfileData.ProfileDefRecords.Read(ProfileDef.Read(CaseDir & "\profile.def", "CRDS"))

    End Sub

    Public Sub ImportByFile(ByVal ProfDat As Boolean, ByVal ProfDef As Boolean, ByVal StrucDat As Boolean, ByVal StrucDef As Boolean, ByVal ControlDef As Boolean, ByVal BoundDat As Boolean, ByVal LatDat As Boolean, ByVal BoundLat As Boolean, ByVal FrictionDat As Boolean, ByVal InitDat As Boolean, ByVal NodesDat As Boolean)

        'belangrijk: altijd eerst de .dat-records. Die checken namelijk op aanwezigheid in het netwerk
        If ProfDat Then
            Dim ProfileDat As New clsSobekDataFile(Me.Setup)
            ProfileData.ProfileDatRecords.Read(ProfileDat.Read(SbkCase.CaseDir & "\profile.dat", "CRSN"))
        End If

        If StrucDat Then
            Dim StructDat As New clsSobekDataFile(Me.Setup)
            StructureData.StructDatRecords.Read(StructDat.Read(SbkCase.CaseDir & "\struct.dat", "STRU"))
        End If

        If ProfDef Then
            Dim ProfileDef As New clsSobekDataFile(Me.Setup)
            ProfileData.ProfileDefRecords.Read(ProfileDef.Read(SbkCase.CaseDir & "\profile.def", "CRDS"))
        End If

        If StrucDef Then
            Dim StructDef As New clsSobekDataFile(Me.Setup)
            StructureData.StructDefRecords.Read(StructDef.Read(SbkCase.CaseDir & "\struct.def", "STDS"))
        End If

        If ControlDef Then
            Dim ContrDef As New clsSobekDataFile(Me.Setup)
            StructureData.ControlDefRecords.Read(ContrDef.Read(SbkCase.CaseDir & "\control.def", "CNTL"))
        End If

        If BoundDat Then
            Dim BoundaryDat As New clsSobekDataFile(Me.Setup)
            BoundaryData.BoundaryDatFLBORecords.Read(BoundaryDat.Read(SbkCase.CaseDir & "\boundary.dat", "FLBO"))
            BoundaryData.BoundlatDatBTBLRecords.Read(BoundaryDat.Read(SbkCase.CaseDir & "\boundary.dat", "BTBL"))
        End If

        If LatDat Then
            Dim LateralDat As New clsSobekDataFile(Me.Setup)
            LateralData.LateralDatFLBRRecords.Read(LateralDat.Read(SbkCase.CaseDir & "\lateral.dat", "FLBR"))
            LateralData.LateraldatFLNORecords.Read(LateralDat.Read(SbkCase.CaseDir & "\lateral.dat", "FLNO"))
        End If

        If BoundLat Then
            Dim BoundLatDat As New clsSobekDataFile(Me.Setup)
            LateralData.BoundlatDatBTBLRecords.Read(BoundLatDat.Read(SbkCase.CaseDir & "\boundlat.dat", "BTBL"))
        End If

        If FrictionDat Then
            Dim FrictDat As New clsSobekDataFile(Me.Setup)
            FrictionData.FrictionDatBDFRRecords.Read(FrictDat.Read(SbkCase.CaseDir & "\friction.dat", "BDFR"))
            FrictionData.FrictionDatSTFRRecords.Read(FrictDat.Read(SbkCase.CaseDir & "\friction.dat", "STFR"))
            FrictionData.FrictionDatCRFRRecords.Read(FrictDat.Read(SbkCase.CaseDir & "\friction.dat", "CRFR"))
        End If

        If NodesDat Then
            Dim NodDat As New clsSobekDataFile(Me.Setup)
            NodesData.NodesDatNodeRecords.Read(NodDat.Read(SbkCase.CaseDir & "\nodes.dat", "NODE"))
        End If

        If InitDat Then
            Dim InitialDat As New clsSobekDataFile(Me.Setup)
            InitialData.Read(InitialDat.Read(SbkCase.CaseDir & "\initial.dat", "FLIN"))
        End If

    End Sub

    Friend Sub ImportStructureData(ByVal CaseDir As String, ByRef myModel As clsSobekCase)
        Dim ProfileDef As New clsSobekDataFile(Me.Setup)
        Dim StructDat As New clsSobekDataFile(Me.Setup)
        Dim StructDef As New clsSobekDataFile(Me.Setup)
        Dim ControlDef As New clsSobekDataFile(Me.Setup)

        'belangrijk: eerst de .dat-records. Die checken namelijk op aanwezigheid in het netwerk
        StructureData.StructDatRecords.Read(StructDat.Read(CaseDir & "\struct.dat", "STRU"))
        ProfileData.ProfileDefRecords.Read(ProfileDef.Read(CaseDir & "\profile.def", "CRDS"))
        StructureData.StructDefRecords.Read(StructDef.Read(CaseDir & "\struct.def", "STDS"))
        StructureData.ControlDefRecords.Read(ControlDef.Read(CaseDir & "\control.def", "CNTL"))

    End Sub




End Class
