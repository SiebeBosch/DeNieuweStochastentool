Imports HYDROC01.General
Public Class clsSubcatchmentSubdivision
    Private Setup As clsSetup
    Friend Subcatchment As clsSubcatchment

    Public ID As String
    Dim UnpavedAreaSum As Double

    'keep a list of areas for each sobek landuse type and for each sobek soiltype
    Friend SbkLandUseList As Dictionary(Of HYDROC01.GeneralFunctions.EnmSobekLanduseType, Double)    'contains the 16 landusetypes for SOBEK and their area
    Friend SbkSoilTypeList As Dictionary(Of HYDROC01.GeneralFunctions.EnmCapsimSoilType, Double)     'contains the 21 soiltypes for SOBEK and their area

    'also keep a list of all used soil and landuse classes so we can decide the main class for this subdivision later on
    Friend SoilClassList As New Dictionary(Of String, Double)
    Friend LanduseClassList As New Dictionary(Of String, Double)

    'create an object to store the unpaved data in
    Friend RRUnpavedNode As ClsRRUnpavedNode
    Friend RRCFUnpaved As clsSbkReachObject
    Friend RRCFSurfaceFlow As clsSbkReachObject

    'this is a subdivision of a subcatchment
    'it contains all data that will be clustered into one unpaved-node
    'this class keeps track of all combinations of landuse class and soil class and contains its area
    Public Sub New(ByRef mySetup As clsSetup, ByRef mySub As clsSubcatchment)
        Dim i As Integer
        Setup = mySetup
        Subcatchment = mySub
        SbkLandUseList = New Dictionary(Of HYDROC01.GeneralFunctions.EnmSobekLanduseType, Double)
        SbkSoilTypeList = New Dictionary(Of GeneralFunctions.EnmCapsimSoilType, Double)

        'initialize the list of sobek landuse types
        For i = 1 To 16
            SbkLandUseList.Add(i, 0)
        Next

        'initialize the list of CAPSIM soil types
        For i = 101 To 121
            SbkSoilTypeList.Add(i, 0)
        Next
    End Sub

    Public Function GetMainSoilClassName() As String
        Dim MaxArea As Double = 0, MainClassName As String = Nothing
        For Each SoilClass As String In SoilClassList.Keys
            If SoilClassList.Item(SoilClass) > MaxArea Then
                MaxArea = SoilClassList.Item(SoilClass)
                MainClassName = SoilClass
            End If
        Next
        Return MainClassName
    End Function

    Public Function GetMainLanduseClassName() As String
        Dim MaxArea As Double = 0, MainClassName As String = Nothing
        For Each LanduseClass As String In LanduseClassList.Keys
            If LanduseClassList.Item(LanduseClass) > MaxArea Then
                MaxArea = LanduseClassList.Item(LanduseClass)
                MainClassName = LanduseClass
            End If
        Next
        Return MainClassName
    End Function



    Public Function GetMainSoilType() As GeneralFunctions.EnmCapsimSoilType
        Dim maxArea As Double = 0
        Dim mainType As GeneralFunctions.EnmCapsimSoilType = Nothing
        Dim i As Integer
        For i = 0 To SbkSoilTypeList.Count - 1
            If SbkSoilTypeList.Values(i) > maxArea Then
                mainType = SbkSoilTypeList.Keys(i)
                maxArea = SbkSoilTypeList.Item(mainType)
            End If
        Next
        Return mainType
    End Function

    Public Function GetUnpavedAreaSum() As Double
        Return UnpavedAreaSum
    End Function

    Public Function CalculateUnpavedLanduseSum() As Double
        Dim Sum As Double
        For i = 1 To 16
            Sum += SbkLandUseList.Item(i)
        Next
        Return Sum
    End Function

    Public Function AddArea(SbkSoilType As HYDROC01.GeneralFunctions.EnmCapsimSoilType, SbkLanduseType As HYDROC01.GeneralFunctions.EnmSobekLanduseType, SoilClass As String, LanduseClass As String, Area As Double, ByRef SkippedSoilTypesArea As Dictionary(Of String, Double), ByRef SkippedLanduseTypesArea As Dictionary(Of String, Double)) As Boolean
        Try
            If Not SbkSoilTypeList.ContainsKey(SbkSoilType) Then
                If SkippedSoilTypesArea.ContainsKey(SbkSoilType.ToString) Then
                    SkippedSoilTypesArea.Item(SbkSoilType.ToString) += Area
                Else
                    SkippedSoilTypesArea.Add(SbkSoilType.ToString, Area)
                End If
                Return False
            ElseIf Not SbkLandUseList.ContainsKey(SbkLanduseType) Then
                If SkippedLanduseTypesArea.ContainsKey(SbkLanduseType.ToString) Then
                    SkippedLanduseTypesArea.Item(SbkLanduseType.ToString) += Area
                Else
                    SkippedLanduseTypesArea.Add(SbkLanduseType.ToString, Area)
                End If
                Return False
            Else
                'add this area to its corresponding landuse and soiltype
                SbkLandUseList.Item(SbkLanduseType) += Area
                SbkSoilTypeList.Item(SbkSoilType) += Area

                'also add this area to the corresponding soilclass and landuseclass lists
                If Not SoilClassList.ContainsKey(SoilClass.Trim.ToUpper) Then SoilClassList.Add(SoilClass.Trim.ToUpper, Area) Else SoilClassList.Item(SoilClass.Trim.ToUpper) += Area
                If Not LanduseClassList.ContainsKey(LanduseClass.Trim.ToUpper) Then LanduseClassList.Add(LanduseClass.Trim.ToUpper, Area) Else LanduseClassList.Item(LanduseClass.Trim.ToUpper) += Area

                'finally add this area to the variable TotalArea. This is for quick reference
                UnpavedAreaSum += Area
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddWarning(ex.Message)
            Return False
        End Try
    End Function

    Public Function BuildUnpavedRRCFConnectionData(ByRef SbkCase As ClsSobekCase, ByRef SeasonTransitions As clsSeasonTransitions) As Boolean
        Dim Bound3B3BRecord As clsBound3B3BRecord
        Dim Bound3BTBLRecord As clsBound3BTBLRecord
        Try

            '-------------------------------------------------------------------------------------------------------------------
            '           RR on Flow Connection for Unpaved node
            '-------------------------------------------------------------------------------------------------------------------
            If RRUnpavedNode.InUse Then
                Bound3B3BRecord = New clsBound3B3BRecord(Me.Setup, RRCFUnpaved.ID, Subcatchment.ZP, Subcatchment.WP)
                If Not SbkCase.RRData.Bound3B3B.Records.ContainsKey(Bound3B3BRecord.ID.Trim.ToUpper) Then SbkCase.RRData.Bound3B3B.Records.Add(Bound3B3BRecord.ID.Trim.ToUpper, Bound3B3BRecord)
                If Subcatchment.ZP <> Subcatchment.WP Then
                    Bound3BTBLRecord = New clsBound3BTBLRecord(Me.Setup, RRCFUnpaved.ID, Subcatchment.ZP, Subcatchment.WP, SeasonTransitions)
                    If Not SbkCase.RRData.BOund3BTBL.Records.ContainsKey(Bound3BTBLRecord.ID.Trim.ToUpper) Then SbkCase.RRData.BOund3BTBL.Records.Add(Bound3BTBLRecord.ID.Trim.ToUpper, Bound3BTBLRecord)
                End If

                If Setup.CatchmentBuilder.Settings.AddSurfaceFlowLinks Then
                    Bound3B3BRecord = New clsBound3B3BRecord(Me.Setup, RRCFSurfaceFlow.ID, Subcatchment.ZP, Subcatchment.WP)
                    If Not SbkCase.RRData.Bound3B3B.Records.ContainsKey(Bound3B3BRecord.ID.Trim.ToUpper) Then SbkCase.RRData.Bound3B3B.Records.Add(Bound3B3BRecord.ID.Trim.ToUpper, Bound3B3BRecord)
                    If Subcatchment.ZP <> Subcatchment.WP Then
                        Bound3BTBLRecord = New clsBound3BTBLRecord(Me.Setup, RRCFSurfaceFlow.ID, Subcatchment.ZP, Subcatchment.WP, SeasonTransitions)
                        If Not SbkCase.RRData.BOund3BTBL.Records.ContainsKey(Bound3BTBLRecord.ID.Trim.ToUpper) Then SbkCase.RRData.BOund3BTBL.Records.Add(Bound3BTBLRecord.ID.Trim.ToUpper, Bound3BTBLRecord)
                    End If
                End If
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function




End Class
