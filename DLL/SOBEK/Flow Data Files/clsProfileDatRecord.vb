Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsProfileDatRecord
    Friend ID As String
    Friend di As String
    Public rl As Double
    Friend rs As Double
    Friend Record As String
    Friend InUse As Boolean

    'extra's
    Friend AreaUnderSurfaceLevel As Double
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Public Function getID() As String
        Return ID
    End Function

    Friend Sub Build()
        'stel nu de string samen, want we hebben alleen nog de elementen van de record
        Record = "CRSN id '" & ID & "' di '" & di & "' rl " & rl & " rs " & rs & " crsn"
    End Sub
    Friend Sub Read(ByVal myRecord As String)
        Record = myRecord
        Dim Done As Boolean, myStr As String
        Done = False

        While Not Done
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "di"
                    di = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "rl"
                    rl = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "rs"
                    rs = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case ""
                    If myRecord = "" Then Done = True
            End Select
        End While

        InUse = True

    End Sub

    Friend Sub Write(ByRef myWriter As StreamWriter)
        myWriter.WriteLine("CRSN id '" & ID & "' di '" & di & "' rl " & Format(rl, "0.###") & " rs " & Format(rs, "0.###") & " crsn")
    End Sub

    Friend Sub CalculateAreaUnderSurfaceLevel(ByRef DefRecord As clsProfileDefRecord)
        'Author: Siebe Bosch
        'Date: 17-9-2013
        'Description: Calculates the wetted area (m2) under surface level
        'and writes it to the variable AreaUnderSurfaceLevel
        'it uses the value for token rs to do so
        If DefRecord.ty = 10 Then
            AreaUnderSurfaceLevel = DefRecord.ltyzTable.CalculateAreaUnderValueFromYZTable(rs - rl)
        End If

    End Sub

    Friend Function CalculateLowestBedLevel(ByRef DefRecord As clsProfileDefRecord, Optional IncludeGroundLevel As Boolean = False) As Double
        'v1.792 when calculating the lowest bed level we can choose to include or exclude the groundlayer. Also new is the option to add a value
        Try
            Dim i As Integer
            Dim lowestbedlevel As Double = 999999999
            Select Case DefRecord.ty
                Case Is = 0 'tabulated
                    lowestbedlevel = rl + DefRecord.ltlwTable.XValues.Values(0)
                    If IncludeGroundLevel AndAlso DefRecord.gu = 1 AndAlso DefRecord.gl > 0 Then lowestbedlevel += DefRecord.gl
                Case Is = 1 'trapezoidal
                    lowestbedlevel = rl + DefRecord.bl
                    If IncludeGroundLevel AndAlso DefRecord.gu = 1 AndAlso DefRecord.gl > 0 Then lowestbedlevel += DefRecord.gl
                Case Is = 4 'round
                    lowestbedlevel = rl + DefRecord.bl
                    If IncludeGroundLevel AndAlso DefRecord.gu = 1 AndAlso DefRecord.gl > 0 Then lowestbedlevel += DefRecord.gl
                Case Is = 10 'yz
                    For i = 0 To DefRecord.ltyzTable.XValues.Count - 1
                        If DefRecord.ltyzTable.Values1.Values(i) < lowestbedlevel Then lowestbedlevel = DefRecord.ltyzTable.Values1.Values(i)
                        If Not IncludeGroundLevel AndAlso DefRecord.GroundLayerThicknessImplementedInDefinition > 0 Then
                            'v1.792 correcting for a previously implemented ground layer
                            'the profile definition has already had a ground layer implemented inside. We'll need to subtract it to retrieve the bed level itself
                            lowestbedlevel -= DefRecord.GroundLayerThicknessImplementedInDefinition
                        End If
                    Next
                    lowestbedlevel += rl
                    'yz profiles have no ground layer. This makes it impossible to account for a groundlayer
                Case Is = 11 'asymmetrical trapezium
                    lowestbedlevel = rl + DefRecord.ltyzTable.getMinValue(1)
                    If Not IncludeGroundLevel AndAlso DefRecord.GroundLayerThicknessImplementedInDefinition > 0 Then
                        'v1.792 correcting for a previously implemented ground layer
                        'the profile definition has already had a ground layer implemented inside. We'll need to subtract it to retrieve the bed level itself
                        lowestbedlevel -= DefRecord.GroundLayerThicknessImplementedInDefinition
                    End If
                Case Else
                    MsgBox("Error in sub calculateLowestBedLevel. Cross section type " & DefRecord.ty & " for profile " & ID & " not (yet) supported.")
                    setup.Log.AddError("Error in sub calculateLowestBedLevel. Cross section type " & DefRecord.ty & " for profile " & ID & " not (yet) supported.")
            End Select
            Return lowestbedlevel
        Catch ex As Exception
            Me.setup.Log.AddError("Error calculating lowest bedlevel for cross section " & ID)
            Return Double.NaN
        End Try
    End Function

    Friend Function calculateMinFlowWidth(ByRef DefRecord As clsProfileDefRecord) As Double
        Try
            Dim i As Integer
            Dim minimumflowwidth As Double = Double.NaN
            Select Case DefRecord.ty
                Case Is = 0 'tabulated
                    minimumflowwidth = DefRecord.ltlwTable.Values1.Values(0)
                Case Is = 1 'trapezoidal
                    minimumflowwidth = DefRecord.bw
                Case Is = 4 'round
                    minimumflowwidth = 0
                Case Is = 10, 11 'yz and asymm trapezium
                    Dim DeepestIdx As Integer = DefRecord.ltyzTable.GetLowestIdx(1)
                    Dim startIdx As Integer = DeepestIdx
                    Dim endIdx As Integer = DeepestIdx
                    For i = DeepestIdx To 0 Step -1
                        If DefRecord.ltyzTable.Values1.Values(i) <= DefRecord.ltyzTable.Values1.Values(DeepestIdx) Then
                            startIdx = i
                        End If
                    Next
                    For i = DeepestIdx To DefRecord.ltyzTable.Values1.Count - 1
                        If DefRecord.ltyzTable.Values1.Values(i) <= DefRecord.ltyzTable.Values1.Values(DeepestIdx) Then
                            endIdx = i
                        End If
                    Next
                    minimumflowwidth = DefRecord.ltyzTable.XValues.Values(endIdx) - DefRecord.ltyzTable.XValues.Values(startIdx)
                Case Else
                    MsgBox("Error in sub calculateMinFlowWidth. Cross section type " & DefRecord.ty & " for profile " & ID & " not (yet) supported.")
                    setup.Log.AddError("Error in sub calculateMinFlowWidth. Cross section type " & DefRecord.ty & " for profile " & ID & " not (yet) supported.")
            End Select

            Return minimumflowwidth
        Catch ex As Exception
            Me.setup.Log.AddError("Error calculating minimum flow width for profile " & ID)
            Return Double.NaN
        End Try
    End Function

    Friend Function calculateFlowWidthAtTargetLevel(ByRef DefRecord As clsProfileDefRecord, TargetLevel As Double) As Double
        Try
            Dim FlowWidthAtTargetlevel As Double
            Dim Depth As Double = TargetLevel - rl 'subtract the vertical shift of our profile.dat 
            FlowWidthAtTargetlevel = DefRecord.getProfileWidthAtLevel(Depth)
            Return FlowWidthAtTargetlevel
        Catch ex As Exception
            Return Double.NaN
        End Try
    End Function

    Friend Function calculateMaxFlowWidth(ByRef DefRecord As clsProfileDefRecord) As Double
        Try
            Dim maximumflowwidth As Double = Double.NaN
            Select Case DefRecord.ty
                Case Is = 0 'tabulated
                    maximumflowwidth = DefRecord.ltlwTable.Values1.Values(DefRecord.ltlwTable.Values1.Count - 1)
                Case Is = 1 'trapezoidal
                    maximumflowwidth = DefRecord.sw
                Case Is = 4 'round
                    maximumflowwidth = DefRecord.rd * 2
                Case Is = 10, 11 'yz and asymm trapezium
                    maximumflowwidth = DefRecord.ltyzTable.XValues.Values(DefRecord.ltyzTable.XValues.Count - 1) - DefRecord.ltyzTable.XValues.Values(0)
                Case Else
                    MsgBox("Error in sub calculateMaxFlowWidth. Cross section type " & DefRecord.ty & " for profile " & ID & " not (yet) supported.")
                    setup.Log.AddError("Error in sub calculateMaxFlowWidth. Cross section type " & DefRecord.ty & " for profile " & ID & " not (yet) supported.")
            End Select
            Return maximumflowwidth
        Catch ex As Exception
            Me.setup.Log.AddError("Error calculating maximum flow width for profile " & ID)
            Return Double.NaN
        End Try


    End Function

End Class
