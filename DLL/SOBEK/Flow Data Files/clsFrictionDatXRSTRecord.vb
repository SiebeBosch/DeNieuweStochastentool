Option Explicit On

Imports STOCHLIB.General

Public Class clsFrictionDatXRSTRecord

    Public ID As String 'ID
    Public nm As String 'name
    Public ty As Integer 'type of extra friction
    Public Table As clsSobekTable

    Public InUse As Boolean

    Private record As String
    Private Setup As clsSetup

    Public Sub New(ByVal mySetup As clsSetup)
        Me.Setup = mySetup
        Table = New clsSobekTable(Me.Setup)
    End Sub

    Friend Sub build()
        Dim i As Long
        record = "XRST id '" & ID & "' nm '" & nm & "' ty " & ty & " rt rs" & vbCrLf
        record &= "TBLE" & vbCrLf
        For i = 0 To Table.XValues.Count - 1
            record &= Table.XValues.Values(i) & " " & Table.Values1.Values(i) & " <" & vbCrLf
        Next
        record &= "tble xrst"
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim myStr As String
        record = myRecord

        While Not myRecord = ""
            myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case Is = "id"
                    ID = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "nm"
                    nm = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ty"
                    ty = Me.Setup.GeneralFunctions.ParsestringNumeric(myRecord, " ")
                Case Is = "rt"
                Case Is = "rs"
                    Table = Setup.GeneralFunctions.ParseSobekTable(myRecord)
            End Select
        End While

    End Sub

    Public Function ComputeFromTabulatedCrossSectionTable(BridgeVerticalShift As Double, ByRef bridgeCrossSectionTabulatedTable As clsSobekTable, CrossSectionVerticalShift As Double, ByRef ChannelCrossSectionTabulatedTable As clsSobekTable, AbutmentStartDepth As Double, EntranceLossCoef As Double, ExitLossCoef As Double, length As Double, ManningBridge As Double, ManningChannel As Double, IncludeEntranceLoss As Boolean, IncludeFrictionLoss As Boolean, IncludeExitLoss As Boolean) As Boolean
        'extra resistance node requires KSI as a function of waterlevel.
        'dH = KSI * Q * |Q|
        'KSI = dH/Q^2)
        'and when applying the discharge formula of an abutment bridge:
        'Q = mu * A * SQRT(2gdh)
        'this finally results in: KSI(h) = SQRT(EntranceLoss + FrictionLoss + ExitLoss)/(2gA)
        'here:
        'EntranceLoss is a constant (default 0.25)
        'FrictionLoss is computed by 2gL/(R*C^2)
        'ExitLoss is computed by coef0 * (1 - Af/Af2) (wetted perimeter inside bridge/downstream)

        'we subdivide our cross section in 50 sections
        'Dim StepSize As Double = (bridgeCrossSectionTabulatedTable.XValues.Values(bridgeCrossSectionTabulatedTable.XValues.Values.Count - 1) - bridgeCrossSectionTabulatedTable.XValues.Values(0)) / 50

        'IMPORTANT: since we're dealing with an EXTRA resistance node here, we must not double-count the resistance of the profile itself
        'hence we start calculating P and A from the base of the abutments
        Dim h As Double, d As Double, i As Integer
        Dim Abridge As Double, Achann As Double, Pbridge As Double, Pchann As Double, Rbridge As Double, Rchann As Double
        Dim KSI As Double
        Dim xiI As Double, xiF As Double, Xifchann As Double, xiO As Double, XifExtra As Double = 0
        For i = 0 To bridgeCrossSectionTabulatedTable.XValues.Count - 1
            d = bridgeCrossSectionTabulatedTable.XValues.Values(i)         'depth w.r.t. the reference level of the table
            h = BridgeVerticalShift + d                   'waterlevel
            Abridge = bridgeCrossSectionTabulatedTable.CalculateAreaUnderValueFromTabulated(d)
            Pbridge = bridgeCrossSectionTabulatedTable.CalculateWettedPerimeterUnderValueFromTabulated(d)
            Achann = ChannelCrossSectionTabulatedTable.CalculateAreaUnderValueFromTabulated(h - CrossSectionVerticalShift)
            Pchann = ChannelCrossSectionTabulatedTable.CalculateWettedPerimeterUnderValueFromTabulated(h - CrossSectionVerticalShift)
            'we only implement extra resistance when the lower end of our abutment bridge is exceeded.
            'in this computation we only take the difference between both
            'only implement KSI above abutment start elevation
            If d >= AbutmentStartDepth AndAlso Abridge > 0 AndAlso Pbridge > 0 AndAlso Achann > 0 Then
                Rbridge = Abridge / Pbridge
                Rchann = Achann / Pchann

                'calculate the friction loss of our channel and bridge. The extra resistance due to friction is the difference between our channel friction loss and bridge friction loss
                If IncludeFrictionLoss Then
                    Me.Setup.GeneralFunctions.CalculateFrictionLoss(length, GeneralFunctions.enmFrictionType.MANNING, ManningChannel, Rchann, Xifchann)
                    Me.Setup.GeneralFunctions.CalculateFrictionLoss(length, GeneralFunctions.enmFrictionType.MANNING, ManningBridge, Rbridge, xiF)
                    XifExtra = xiF - Xifchann
                Else
                    XifExtra = 0
                End If

                'calculate the entrance loss
                If IncludeEntranceLoss Then xiI = EntranceLossCoef Else xiI = 0

                'calculate the exit loss by comparing our wetted area with that of the channel itself
                If IncludeExitLoss Then Me.Setup.GeneralFunctions.CalculateBridgeExitLoss(ExitLossCoef, Abridge, Achann, xiO) 'exception to the rule: to determine the output loss we actually need the entire wetted perimeter and compare it to the channel profile!

                'calculate our extra resistance value ksi for the current depth
                'make sure the extra resistance can never drop below zero!
                KSI = Math.Max(0, (xiI + XifExtra + xiO)) / (2 * 9.81 * Abridge ^ 2)

                Table.AddDataPair(9, h, KSI, Abridge, Pbridge, Achann, Pchann, xiI, XifExtra, xiO)
            Else
                'zero extra resistance since we're below the abutment start elevation
                Table.AddDataPair(9, h, 0, Abridge, Pbridge, Achann, Pchann, xiI, XifExtra, xiO)
                'when below abutment start elevation, KSI = 0
            End If
        Next
    End Function




    Friend Sub write(ByRef datWriter As System.IO.StreamWriter)
        Call build()
        datWriter.WriteLine(record)
    End Sub

End Class
