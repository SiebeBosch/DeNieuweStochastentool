Option Explicit On
Imports System.IO
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Public Class clsProfileDefRecord

    Friend ID As String
    Friend nm As String
    Friend ty As GeneralFunctions.enmProfileType
    Friend wm As Double
    Friend W1 As Double
    Friend W2 As Double
    Friend sw As Double
    Friend ltlw As Boolean

    'tabulated and river profiles ty = 0
    Friend IsRiverProfile As Boolean
    Friend dk As Integer 'summerdike
    Friend dc As Double 'dike crest
    Friend db As Double 'floodplain base level behind dike
    Friend df As Double 'flow area behind dike
    Friend dt As Double 'total area behind dike
    Friend gl As Double 'groundlayer thickness
    Friend gu As Integer   'groundlayer used
    Friend ll As Double  'river profile thingies
    Friend rl As Double 'river profile thingies
    Friend lw As Double 'river profile thingies
    Friend rw As Double 'river profile thingies

    'trapezoidal ty = 1
    Friend bl As Double 'bottom level
    Friend bw As Double 'bottom width
    Friend bs As Double 'bank slope
    Friend aw As Double 'max flow width
    Friend lu As GeneralFunctions.enmConveyanceMethod

    'open circle ty = 3
    Friend rd As Double 'radius

    'closed circle cross section ty = 4

    'egg ty = 6
    Friend bo As Double 'bottom width profile

    Friend st As Integer 'storage type 0=reservoir, 1=loss
    Friend ltswMaaiveld As Double 'storage width at maaiveld. Dit getal komt achter token lt sw 0
    Public ltlwTable As clsSobekTable
    Public ltswTable As clsSobekTable
    Public ltyzTable As clsSobekTable
    Public DataTable As clsSobekTable

    Friend InUse As Boolean
    Friend LeftProcessed As Boolean   'a personal variable to keep track if the left side has already been processed in one way or another, e.g. NVO's
    Friend RightProcessed As Boolean  'a personal variable to keep track if the right side has already been processed in one way or another, e.g. NVO's
    Friend Processed As Boolean

    Friend LeftVolumeChange As Double 'the change in volume due to e.g. NVO's, expressed in m2
    Friend RightVolumeChange As Double 'the change in volume due to e.g. NVO's, expressed in m2

    Friend GroundLayerThicknessImplementedInDefinition As Double 'this variable we'll use to store information for situations where a ground layer has been implemented DIRECTLY in the profile definition (so not in gl and gu).

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        ltlwTable = New clsSobekTable(Me.Setup)
        ltswTable = New clsSobekTable(Me.Setup)
        ltyzTable = New clsSobekTable(Me.Setup)
        DataTable = New clsSobekTable(Me.Setup)

    End Sub

    Public Function CheckAscending() As Boolean
        'this function checks if the yz-table of the current profile definition is ascending
        Select Case ty
            Case Is = GeneralFunctions.enmProfileType.yztable
                For i = 1 To ltyzTable.XValues.Values.Count - 1
                    If ltyzTable.XValues.Values(i) <= ltyzTable.XValues.Values(i - 1) Then
                        Return False
                    End If
                Next
                Return True
            Case Else
                Return True
        End Select
    End Function

    Public Function addYZCoordinatesAtDepth(Depth) As Tuple(Of Double, Double)
        'this function creates two (interpolated) yz-points in the current profile definition at the given depth
        'it returns the distances (Y-values) for both points
        'first find the index number for the deepest point
        Dim MinIdx As Integer = ltyzTable.GetLowestIdx(1)
        Dim LeftIdx As Integer = -1, RightIdx As Integer = -1
        Dim minZ As Double = ltyzTable.Values1(MinIdx)
        Dim Z As Double = minZ + Depth                      'the elevation value for which we're looking up the Y-values
        Dim Y1 As Double = Double.NaN
        Dim Y2 As Double = Double.NaN
        Dim Distances As Tuple(Of Double, Double)
        Dim newTable = New clsSobekTable(Me.Setup)

        'walk left to find the first point where our depth is exceeded
        For i = MinIdx To 1 Step -1
            If ltyzTable.Values1(i - 1) - minZ > Depth AndAlso ltyzTable.Values1(i) - minZ < Depth Then
                LeftIdx = i
                Y1 = Me.Setup.GeneralFunctions.Interpolate(ltyzTable.Values1(i - 1), ltyzTable.XValues(i - 1), ltyzTable.Values1(i), ltyzTable.XValues(i), Z)
                Exit For
            End If
        Next
        For i = MinIdx To ltyzTable.XValues.Count - 2
            If ltyzTable.Values1(i) - minZ < Depth AndAlso ltyzTable.Values1(i + 1) - minZ > Depth Then
                RightIdx = i
                Y2 = Me.Setup.GeneralFunctions.Interpolate(ltyzTable.Values1(i), ltyzTable.XValues(i), ltyzTable.Values1(i + 1), ltyzTable.XValues(i + 1), Z)
                Exit For
            End If
        Next

        Distances = New Tuple(Of Double, Double)(Y1, Y2)

        'populate the new table
        If LeftIdx >= 0 AndAlso RightIdx >= 0 Then
            For i = 0 To LeftIdx - 1
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next
            newTable.AddDataPair(2, Y1, Z)
            For i = LeftIdx To RightIdx
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next
            newTable.AddDataPair(2, Y2, Z)
            For i = RightIdx + 1 To ltyzTable.XValues.Count - 1
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next
        ElseIf LeftIdx >= 0 Then
            For i = 0 To LeftIdx - 1
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next
            newTable.AddDataPair(2, Y1, Z)
            For i = LeftIdx To ltyzTable.XValues.Count - 1
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next
        ElseIf RightIdx >= 0 Then
            For i = 0 To RightIdx
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next
            newTable.AddDataPair(2, Y2, Z)
            For i = RightIdx + 1 To ltyzTable.XValues.Count - 1
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next
        Else
            newTable = ltyzTable
        End If

        'and replace the old one with it
        ltyzTable = newTable
        Return Distances

    End Function

    Friend Function AutoEmbankmentFriction(MainFricType As GeneralFunctions.enmFrictionType, SlopeFricType As GeneralFunctions.enmFrictionType, PlainFricType As GeneralFunctions.enmFrictionType, MainVal As Double, SlopeVal As Double, PlainVal As Double, Optional ByVal SlopeThresholdHorVert As Double = 3) As clsFrictionDatCRFRRecord
        Try
            Dim fricDat As New clsFrictionDatCRFRRecord(Me.Setup)
            Dim i As Integer
            Dim curY As Double, nextY As Double, curZ As Double, nextZ As Double, mySlope As Double
            Dim MainLeftIdx As Integer = -1, SlopeLeftIdx As Integer = -1, PlainleftIdx As Integer = -1
            Dim MainrightIdx As Integer = -1, SloperightIdx As Integer = -1, PlainRightIdx As Integer = -1

            If ty = enmProfileType.yztable Then
                Dim LowestIdx As Integer = GetLowestLevelIdx()
                For i = LowestIdx To 1 Step -1
                    curY = ltyzTable.XValues.Values(i)
                    curZ = ltyzTable.Values1.Values(i)
                    nextY = ltyzTable.XValues.Values(i - 1)
                    nextZ = ltyzTable.Values1.Values(i - 1)
                    mySlope = Math.Abs(curY - nextY) / Math.Abs(curZ - nextZ)
                    If mySlope <= SlopeThresholdHorVert AndAlso MainLeftIdx < 0 Then
                        'side slope starts here
                        MainLeftIdx = i
                    ElseIf MainLeftIdx >= 0 AndAlso mySlope > SlopeThresholdHorVert Then
                        'floodplain starts here and ends at index 0
                        SlopeLeftIdx = i
                        Exit For
                    End If
                Next

                'wrap it up
                If MainLeftIdx < 0 Then
                    MainLeftIdx = 0
                ElseIf SlopeLeftIdx < 0 Then
                    SlopeLeftIdx = 0
                ElseIf PlainleftIdx < 0 Then
                    PlainleftIdx = 0
                End If

                For i = LowestIdx To ltyzTable.XValues.Values.Count - 2
                    curY = ltyzTable.XValues.Values(i)
                    curZ = ltyzTable.Values1.Values(i)
                    nextY = ltyzTable.XValues.Values(i + 1)
                    nextZ = ltyzTable.Values1.Values(i + 1)
                    mySlope = Math.Abs(curY - nextY) / Math.Abs(curZ - nextZ)
                    If mySlope <= SlopeThresholdHorVert AndAlso MainrightIdx < 0 Then
                        'side slope starts here
                        MainrightIdx = i
                    ElseIf MainrightIdx >= 0 AndAlso mySlope > SlopeThresholdHorVert Then
                        'floodplain starts here and ends at index 0
                        SloperightIdx = i
                        Exit For
                    End If
                Next

                'wrap it up
                If MainrightIdx < 0 Then
                    MainrightIdx = ltyzTable.XValues.Count - 1
                ElseIf SloperightIdx < 0 Then
                    SloperightIdx = ltyzTable.XValues.Count - 1
                ElseIf PlainRightIdx < 0 Then
                    PlainRightIdx = ltyzTable.XValues.Count - 1
                End If

                'now that we've identified the slopes etc. we can construct a friction record
                fricDat.ID = ID
                fricDat.nm = ID
                fricDat.cs = ID
                fricDat.InUse = True

                'write left plain
                If PlainleftIdx >= 0 AndAlso SlopeLeftIdx >= 0 Then
                    fricDat.ltysTable.AddDataPair(2, ltyzTable.XValues.Values(PlainleftIdx), ltyzTable.XValues.Values(SlopeLeftIdx))
                    fricDat.ftysTable.AddDataPair(2, PlainFricType, PlainVal,,,,,, True)
                    fricDat.frysTable.AddDataPair(2, PlainFricType, PlainVal,,,,,, True)
                End If
                'write left slope
                If MainLeftIdx >= 0 AndAlso SlopeLeftIdx >= 0 Then
                    fricDat.ltysTable.AddDataPair(2, ltyzTable.XValues.Values(SlopeLeftIdx), ltyzTable.XValues.Values(MainLeftIdx))
                    fricDat.ftysTable.AddDataPair(2, SlopeFricType, SlopeVal,,,,,, True)
                    fricDat.frysTable.AddDataPair(2, SlopeFricType, SlopeVal,,,,,, True)
                End If
                'write main slope
                If MainrightIdx >= 0 AndAlso MainLeftIdx >= 0 AndAlso MainLeftIdx <> MainrightIdx Then
                    fricDat.ltysTable.AddDataPair(2, ltyzTable.XValues.Values(MainLeftIdx), ltyzTable.XValues.Values(MainrightIdx))
                    fricDat.ftysTable.AddDataPair(2, MainFricType, MainVal,,,,,, True)
                    fricDat.frysTable.AddDataPair(2, MainFricType, MainVal,,,,,, True)
                End If
                'write right slope
                If SloperightIdx >= 0 AndAlso MainrightIdx >= 0 Then
                    fricDat.ltysTable.AddDataPair(2, ltyzTable.XValues.Values(MainrightIdx), ltyzTable.XValues.Values(SloperightIdx))
                    fricDat.ftysTable.AddDataPair(2, SlopeFricType, SlopeVal,,,,,, True)
                    fricDat.frysTable.AddDataPair(2, SlopeFricType, SlopeVal,,,,,, True)
                End If
                'write right plain
                If SloperightIdx >= 0 AndAlso PlainRightIdx >= 0 Then
                    fricDat.ltysTable.AddDataPair(2, ltyzTable.XValues.Values(SloperightIdx), ltyzTable.XValues.Values(PlainRightIdx))
                    fricDat.ftysTable.AddDataPair(2, PlainFricType, PlainVal,,,,,, True)
                    fricDat.frysTable.AddDataPair(2, PlainFricType, PlainVal,,,,,, True)
                End If
                Return fricDat
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    Friend Function UniformFriction(FricType As GeneralFunctions.enmFrictionType, FricVal As Double) As clsFrictionDatCRFRRecord
        Try
            Dim fricDat As New clsFrictionDatCRFRRecord(Me.Setup)
            fricDat.ID = ID
            fricDat.nm = ID
            fricDat.cs = ID
            fricDat.InUse = True
            If ty = enmProfileType.yztable Then
                fricDat.ltysTable.AddDataPair(2, ltyzTable.XValues.Values(0), ltyzTable.XValues.Values(ltyzTable.XValues.Count - 1))
                fricDat.ftysTable.AddDataPair(2, FricType, FricVal,,,,,, True)
                fricDat.frysTable.AddDataPair(2, FricType, FricVal,,,,,, True)
                Return fricDat
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Friend Sub CleanUp()

        'fixes any errors in the cross section definition
        If ty = enmProfileType.yztable Then
            If Not ltyzTable.RemoveDuplicates(0) Then Me.Setup.Log.AddMessage("Duplicate points found in YZ-profile " & ID & ". Duplicates were removed.")
        ElseIf ty = enmProfileType.tabulated Then
            If Not ltlwTable.RemoveDuplicates(0) Then Me.Setup.Log.AddMessage("Duplicate points found in tabulated profile " & ID & ". Duplicates were removed.")
        End If

    End Sub

    Friend Function Clone(ByVal newID As String) As clsProfileDefRecord
        Dim NewDef As New clsProfileDefRecord(Me.Setup, Me.SbkCase)

        NewDef.ID = newID
        NewDef.nm = newID
        NewDef.ty = ty
        NewDef.wm = wm
        NewDef.W1 = W1
        NewDef.W2 = W2
        NewDef.sw = sw
        NewDef.ltlw = ltlw

        'tabulated profiles of river type ty = 0 with token dk present
        NewDef.dk = dk 'summerdike
        NewDef.dc = dc 'dike crest
        NewDef.db = db 'floodplain base level behind dike
        NewDef.df = df 'flow area behind dike
        NewDef.dt = dt 'total area behind dike
        NewDef.gl = gl 'groundlayer thickness
        NewDef.gu = gu   'groundlayer used
        NewDef.ll = ll
        NewDef.rl = rl
        NewDef.lw = lw
        NewDef.rw = rw

        'trapezoidal ty = 1
        NewDef.bl = bl 'bottom level
        NewDef.bw = bw 'bottom width
        NewDef.bs = bs 'bank slope
        NewDef.aw = aw 'max flow width

        'open circle ty = 3
        NewDef.rd = rd 'radius

        'closed circle cross section ty = 4

        'egg ty = 6
        NewDef.bo = bo 'bottom width profile

        NewDef.st = st 'storage type 0=reservoir, 1=loss
        NewDef.ltswMaaiveld = ltswMaaiveld 'storage width at maaiveld. Dit getal komt achter token lt sw 0
        NewDef.ltlwTable = ltlwTable.Clone
        NewDef.ltswTable = ltswTable.Clone
        NewDef.ltyzTable = ltyzTable.Clone
        NewDef.DataTable = DataTable.Clone

        NewDef.InUse = InUse
        NewDef.LeftProcessed = LeftProcessed   'a personal variable to keep track if the left side has already been processed in one way or another, e.g. NVO's
        NewDef.RightProcessed = RightProcessed  'a personal variable to keep track if the right side has already been processed in one way or another, e.g. NVO's

        Return NewDef

    End Function

    Public Function ShiftVertically(Value As Double) As Boolean
        Try
            Dim newtable As New clsSobekTable(Me.Setup)
            newtable.XValues = New Dictionary(Of String, Single)
            newtable.Values1 = New Dictionary(Of String, Single)
            For i = 0 To ltyzTable.XValues.Count - 1
                newtable.XValues.Add(i.ToString, ltyzTable.XValues.Values(i))
                newtable.Values1.Add(i.ToString, ltyzTable.Values1.Values(i) + Value)
            Next
            ltyzTable = newtable
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ConverToTabulated() As Boolean
        Try
            Select Case ty
                Case enmProfileType.yztable
                    YZToTabulated()
                    Return True
                Case enmProfileType.tabulated
                    'no conversion needed
                    Return True
                Case Else
                    Throw New Exception("Could not convert profile definition " & ID & ". Only YZ- and Tabulated profiles are supported thus far.")
                    Return False
            End Select
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ConvertTabulated of class clsProfileDefRecord: " & ex.Message)
            Return False
        End Try
    End Function
    Public Function YZToTabulated() As Boolean

        Try
            'this routine converts yz data to tabulated
            'first create a sort index by values1 in ascending order

            Dim Z As Single
            Dim i As Integer, j As Integer
            Dim minVal As Single = ltyzTable.getMinValue(1)
            Dim minvalidx As Integer = ltyzTable.GetMinValueIdx(1)

            Dim newTable As New Dictionary(Of Single, Single)
            Dim newY As New Dictionary(Of Single, Boolean) 'true marks the start of a segment where the profile is lower than the current z-value

            'first we'll create a new table, cloned from the original.
            'however: we will add two fake values: one at the beginning and one at the end
            'we'll make sure that both have an extremely high z-value and an x-value that nearly matches their neighbor
            Dim cloneTable As clsSobekTable = ltyzTable.Clone(, True, True)

            With cloneTable

                For i = 1 To .XValues.Count - 2
                    Z = .Values1.Values(i)

                    'create a list of all points that cross this elevation
                    newY = New Dictionary(Of Single, Boolean) 'contains distances y where the profile crosses the current elevation

                    'followed by the rest of the profile except the last segment
                    For j = 1 To .XValues.Count - 1
                        If .Values1.Values(j) = Z AndAlso j > 0 AndAlso j < .XValues.Count - 1 Then
                            If .Values1.Values(j - 1) < Z AndAlso .Values1.Values(j + 1) < Z Then
                                'local hill. skip
                            ElseIf .Values1.Values(j - 1) < Z AndAlso .Values1.Values(j + 1) = Z Then
                                'moving up to a plain. skip
                            ElseIf .Values1.Values(j - 1) > Z AndAlso .Values1.Values(j + 1) = Z Then
                                'moving down to a plain. Add starting point
                                newY.Add(.XValues.Values(j), True)
                            ElseIf .Values1.Values(j - 1) = Z AndAlso .Values1.Values(j + 1) > Z Then
                                'moving up from a plain. Ad end point
                                newY.Add(.XValues.Values(j), False)
                            ElseIf .Values1.Values(j - 1) = Z AndAlso .Values1.Values(j + 1) < Z Then
                                'moving down from a plain. skip
                            ElseIf .Values1.Values(j - 1) > Z AndAlso .Values1.Values(j + 1) > Z Then
                                'local thall.
                                newY.Add(.XValues.Values(j) - 0.001, True)
                                newY.Add(.XValues.Values(j) + 0.001, False)
                            ElseIf .Values1.Values(j - 1) < Z AndAlso .Values1.Values(j + 1) > Z Then 'going up
                                newY.Add(.XValues.Values(j), False)
                            ElseIf .Values1.Values(j - 1) > Z AndAlso .Values1.Values(j + 1) < Z Then 'going down
                                newY.Add(.XValues.Values(j), True)
                            End If
                        ElseIf .Values1.Values(j) < Z Then
                            If .Values1.Values(j - 1) > Z Then 'going down
                                newY.Add(Setup.GeneralFunctions.Interpolate(.Values1.Values(j), .XValues.Values(j), .Values1.Values(j - 1), .XValues.Values(j - 1), Z), True)
                            End If
                        ElseIf .Values1.Values(j) > Z Then
                            If .Values1.Values(j - 1) < Z Then 'going up
                                newY.Add(Setup.GeneralFunctions.Interpolate(.Values1.Values(j), .XValues.Values(j), .Values1.Values(j - 1), .XValues.Values(j - 1), Z), False)
                            End If
                        End If
                    Next

                    'now that we have a list of all crossing points we can simply add up the segments and calculate the total width for this elevation
                    Dim TotalWidth As Double = 0
                    For j = 0 To newY.Count - 2
                        If newY.Values(j) = True Then
                            If newY.Values(j + 1) = False Then
                                'this is the only correct combination
                                TotalWidth += (newY.Keys(j + 1) - newY.Keys(j))
                            Else
                                'there cannot be two records in a row both having TRUE
                                Me.Setup.Log.AddError("Possible errors converting YZ to tabulated cross section definition " & ID)
                            End If
                        End If
                    Next
                    If Not newTable.ContainsKey(Z) Then newTable.Add(Z, TotalWidth)
                Next
            End With


            'finally sort and add to table
            Dim sortedDict = (From entry In newTable Order By entry.Key Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)

            ltlwTable = New clsSobekTable(Me.Setup)
            ltlwTable.XValues = New Dictionary(Of String, Single)
            ltlwTable.Values1 = New Dictionary(Of String, Single)
            For i = 0 To sortedDict.Count - 1
                If Not ltlwTable.XValues.ContainsKey(sortedDict.Keys(i).ToString) Then
                    ltlwTable.XValues.Add(sortedDict.Keys(i).ToString, sortedDict.Keys(i))
                    ltlwTable.Values1.Add(sortedDict.Keys(i).ToString, sortedDict.Values(i))
                End If
            Next

            ty = enmProfileType.tabulated
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function YZToTabulated of class clsSobekTable.")
            Return False
        End Try

    End Function

    Friend Function NarrowYZCrossSection(FillDepth, Slope, TopElevation, TopWidth) As Boolean
        Dim newTable As New clsSobekTable(Me.Setup)
        Dim tmpTable As New clsSobekTable(Me.Setup)
        Dim minIdx As Integer = ltyzTable.GetMinValueIdx(1)
        Dim newMin As Double = ltyzTable.Values1.Values(minIdx.ToString.Trim) + FillDepth
        Dim myDepth As Double = TopElevation - newMin
        Dim YLowest As Double = ltyzTable.XValues.Values(minIdx)

        Try
            'create the theoretical profile for the narrowed part and align it symmetrically around the lowest point of our existing profile
            For i = 0 To ltyzTable.XValues.Count - 1
                If ltyzTable.XValues.Values(i) < YLowest - TopWidth / 2 AndAlso ltyzTable.Values1.Values(i) >= TopElevation Then newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next
            newTable.AddDataPair(2, YLowest - TopWidth / 2, TopElevation)
            newTable.AddDataPair(2, YLowest - myDepth * Slope / 2, newMin)
            newTable.AddDataPair(2, YLowest + myDepth * Slope / 2, newMin)
            newTable.AddDataPair(2, YLowest + TopWidth / 2, TopElevation)
            For i = 0 To ltyzTable.XValues.Count - 1
                If ltyzTable.XValues.Values(i) > YLowest + TopWidth / 2 AndAlso ltyzTable.Values1.Values(i) >= TopElevation Then newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next
            ltyzTable = newTable
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Function ContainsVerticalSlot(Optional ByVal ZYRatio As Double = 1 / 0.2) As Boolean
        'if the vertical spike's dZ/dY (both down AND up) exceeds 1m per 20 cm we consider this a vertical slot
        Dim i As Long, dZL As Double, dZR As Double, dY As Double

        Select Case ty
            Case Is = enmProfileType.yztable
                Dim LowestIdx As Integer = ltyzTable.GetMinValueIdx(1)
                If LowestIdx > 0 AndAlso LowestIdx < ltyzTable.Values1.Count - 1 Then
                    dZL = ltyzTable.Values1.Values(LowestIdx - 1) - ltyzTable.Values1.Values(i)
                    dZR = ltyzTable.Values1.Values(LowestIdx + 1) - ltyzTable.Values1.Values(i)
                    dY = ltyzTable.XValues.Values(LowestIdx + 1) - ltyzTable.XValues.Values(LowestIdx - 1)
                    If dZL > 0 AndAlso dZR > 0 AndAlso Math.Min(dZL, dZR) / dY >= ZYRatio Then Return True
                End If
        End Select
        Return False
    End Function

    Public Function getWettedPerimeter(WaterLevel As Double, VerticalShift As Double) As Double
        Dim i As Long, P As Double = 0, w As Double
        Dim y As Double, LocalLevel As Double, localDepth As Double
        Dim minZ As Double

        Select Case ty
            Case Is = enmProfileType.yztable
                minZ = ltyzTable.getMinValue(1)

                'eigenlijk zouden we hieronder telkens de profielhoogtes moeten corrigeren door de vertical shift erbij op te tellen, maar het is
                'eenvoudiger om eenmalig de waterlevel te corrigeren, de andere kant op.
                LocalLevel = WaterLevel - VerticalShift

                If LocalLevel > minZ Then
                    For i = 0 To ltyzTable.XValues.Count - 2
                        If ltyzTable.Values1.Values(i) <= LocalLevel AndAlso ltyzTable.Values1.Values(i + 1) <= LocalLevel Then
                            'all under water
                            P += Setup.GeneralFunctions.Pythagoras(ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i), ltyzTable.XValues.Values(i + 1), ltyzTable.Values1.Values(i + 1))
                        ElseIf ltyzTable.Values1.Values(i) >= LocalLevel AndAlso ltyzTable.Values1.Values(i + 1) <= LocalLevel Then
                            'left bank partially under water
                            y = Setup.GeneralFunctions.Interpolate(ltyzTable.Values1(i), ltyzTable.XValues(i), ltyzTable.Values1(i + 1), ltyzTable.XValues(i + 1), LocalLevel)
                            P += Setup.GeneralFunctions.Pythagoras(y, LocalLevel, ltyzTable.XValues.Values(i + 1), ltyzTable.Values1.Values(i + 1))
                        ElseIf ltyzTable.Values1.Values(i) <= LocalLevel AndAlso ltyzTable.Values1.Values(i + 1) >= LocalLevel Then
                            'right bank
                            y = Setup.GeneralFunctions.Interpolate(ltyzTable.Values1(i), ltyzTable.XValues(i), ltyzTable.Values1(i + 1), ltyzTable.XValues(i + 1), LocalLevel)
                            P += Setup.GeneralFunctions.Pythagoras(ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i), y, LocalLevel)
                        End If
                    Next
                End If
                Return P
            Case Is = enmProfileType.trapezium
                'in case of a trapezium the cross section definition itself always starts at 0, so depth = level - verticalshift
                localDepth = WaterLevel - VerticalShift
                If localDepth > 0 Then
                    P += bw + 2 * bs * localDepth 'bed width + 2 x the slope * depth
                End If
                Return P
            Case Is = enmProfileType.tabulated
                'eigenlijk zouden we hieronder telkens de profielhoogtes moeten corrigeren door de vertical shift erbij op te tellen, maar het is
                'eenvoudiger om eenmalig de waterlevel te corrigeren, de andere kant op.
                LocalLevel = WaterLevel - VerticalShift
                minZ = ltlwTable.XValues.Values(0)

                If LocalLevel > minZ Then
                    P += ltlwTable.Values1.Values(0)        '= bodembreedte
                    For i = 0 To ltlwTable.XValues.Count - 2
                        If LocalLevel >= ltlwTable.XValues.Values(i + 1) Then
                            'also the next record is still flooded
                            P += Setup.GeneralFunctions.Pythagoras(ltlwTable.XValues.Values(i), ltlwTable.Values1.Values(i), ltlwTable.XValues.Values(i + 1), ltlwTable.Values1.Values(i + 1))
                        Else
                            'find the with that corresponds with the local level
                            w = Setup.GeneralFunctions.Interpolate(ltlwTable.XValues.Values(i), ltlwTable.Values1.Values(i), ltlwTable.XValues.Values(i + 1), ltlwTable.Values1.Values(i + 1), LocalLevel)
                            P += Setup.GeneralFunctions.Pythagoras(ltlwTable.XValues.Values(i), ltlwTable.Values1.Values(i), LocalLevel, w)
                            Exit For
                        End If
                    Next
                End If
                Return P
            Case Else
                Me.Setup.Log.AddError("Error in function getWettedPerimeter. Cross Section type not (yet) supported: " & ty.ToString)
        End Select
        Return P
    End Function

    Public Function ConvertToYZ() As Boolean
        'this function converts the current profile to an yz type
        Try
            Select Case ty
                Case Is = enmProfileType.trapezium
                    ltyzTable = New clsSobekTable(Me.Setup)
                    ltyzTable.AddDataPair(2, -aw / 2, bl + ((aw - bw) / 2) / bs)
                    ltyzTable.AddDataPair(2, -bw / 2, bl)
                    ltyzTable.AddDataPair(2, bw / 2, bl)
                    ltyzTable.AddDataPair(2, aw / 2, bl + ((aw - bw) / 2) / bs)
                    ty = enmProfileType.yztable
                    st = 0
                    ltswMaaiveld = ltyzTable.Values1.Values(0)

                    'also implement the ground layer in the profile definition itself. Reason is that yz profiles do not support ground layers as separate attribute 
                    If gu = 1 AndAlso gl > 0 Then
                        AddGroundLayer(Me.Setup, gl)
                        gu = 0 'should not make any difference, but it is neat to set groundlayer as an attribute to false now that we've implemented it in the profile definition itself
                    End If
                Case Is = enmProfileType.yztable
                    Me.Setup.Log.AddMessage("Unnecessary attempt to convert yz table to yz table: " & ID)
                Case Else
                    Throw New Exception("Error: cross section type " & ty.ToString & " cannot (yet) be converted to YZ type.")
                    Return False
            End Select
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ConvertToYZ of class clsProfileDefRecord for profile definition  " & ID & ": " & ex.Message)
            Return False
        End Try
    End Function

    Friend Sub Read(ByVal myRecord As String)
        Dim myStr As String
        Dim Pos As String, Table As String
        Dim activeToken As String = ""         'om bij te houden waar een eventuele tabel bij hoort!
        Dim origRecord As String = myRecord

        While Not myRecord = ""
            myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case Is = "id"
                    ID = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "nm"
                    nm = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ty"
                    ty = Me.Setup.GeneralFunctions.ParsestringNumeric(myRecord, " ")
                Case Is = "lu"
                    lu = DirectCast([Enum].Parse(GetType(STOCHLIB.GeneralFunctions.enmConveyanceMethod), Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), True), STOCHLIB.GeneralFunctions.enmConveyanceMethod)
                Case Is = "wm"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), wm, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "w1"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), W1, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "w2"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), W2, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "sw"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), sw, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "lt"
                    myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ").ToLower
                    If myStr = "lw" Then
                        activeToken = "ltlw"
                    ElseIf myStr = "sw" Then
                        activeToken = "ltsw"
                    ElseIf myStr = "yz" Then 'yz tabel
                        activeToken = "ltyz"
                    End If
                Case Is = "dk"
                    If Not Me.Setup.GeneralFunctions.StringtoInt32(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), dk, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                    IsRiverProfile = True
                Case Is = "dc"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), dc, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "db"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), db, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "df"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), df, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "dt"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), dt, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "gl"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), gl, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "gu"
                    If Not Me.Setup.GeneralFunctions.StringtoInt32(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), gu, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "bl"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), bl, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "sw"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), sw, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "bw"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), bw, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "bs"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), bs, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "aw"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), aw, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "rd"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), rd, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "ll"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), ll, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "rl"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), rl, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "lw"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), lw, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "rw"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), rw, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "bo"
                    If Not Me.Setup.GeneralFunctions.StringtoDouble(Me.Setup.GeneralFunctions.ParseString(myRecord, " ", 1, True), bo, 0) Then Me.Setup.Log.AddError("Error parsing string after token " & myStr & " in profile.def record " & origRecord)
                Case Is = "st"
                    st = 0        'Storage Type (1=loss above surface level) gaat fout vanaf v2.12.003, dus ALTIJD op 0 houden.
                Case Is = "TBLE"
                    Pos = InStr(1, myRecord, "tble", CompareMethod.Binary)
                    Table = Left(myRecord, Pos - 1)
                    myRecord = Right(myRecord, myRecord.Length + 1 - Pos - 4)

                    If activeToken = "ltyz" Then
                        Call ltyzTable.Read(Table)
                        ltyzTable.ID = ID
                    ElseIf activeToken = "ltlw" Then
                        Call ltlwTable.Read(Table)
                        ltyzTable.ID = ID
                    ElseIf activeToken = "ltsw" Then
                        Call ltswTable.Read(Table)
                        ltyzTable.ID = ID
                    End If

                Case Is = "0"
                    If activeToken = "ltsw" Then
                        ltswMaaiveld = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
                Case "crds"
                Case "CRDS"
                Case Else
                    'niks doen
            End Select
        End While

        InUse = True

    End Sub

    Friend Function getLeftYZBankIdx() As Long
        'this routine finds the left embankment index from the yz table
        'it does so by finding the highest factor between embankment slope and main channel slope
        Dim lowestIdx As Long = GetLowestLevelIdx()
        Dim MainSlope As Double, BankSlope As Double, BendFact As Double, MaxBendFact As Double, MaxBendIdx As Long

        Dim i As Long
        MaxBendIdx = lowestIdx / 2 'default value for the embankment is halfway the profile

        For i = lowestIdx - 1 To 1 Step -1
            BankSlope = (ltyzTable.Values1.Values(i - 1) - ltyzTable.Values1.Values(i)) / (ltyzTable.XValues.Values(i) - ltyzTable.XValues.Values(i - 1))
            MainSlope = (ltyzTable.Values1.Values(i) - ltyzTable.Values1.Values(i + 1)) / (ltyzTable.XValues.Values(i + 1) - ltyzTable.XValues.Values(i))
            If BankSlope > 0 AndAlso MainSlope > 0 Then
                BendFact = MainSlope / BankSlope
                If BendFact > MaxBendFact Then
                    MaxBendFact = BendFact
                    MaxBendIdx = i
                End If
            End If
        Next

        Return MaxBendIdx

    End Function

    Friend Function IsSymmetric() As Boolean
        'this function finds out if the current profile definition is symmetric or not
        Select Case ty
            Case Is = enmProfileType.asymmetricaltrapezium
                Return False
            Case Is = enmProfileType.closedcircle
                Return True
            Case Is = enmProfileType.closedrectangular
                Return True
            Case Is = enmProfileType.eggshape
                Return True
            Case Is = enmProfileType.eggshape2
                Return True
            Case Is = enmProfileType.opencircle
                Return True
            Case Is = enmProfileType.sedredge
                Return False
            Case Is = enmProfileType.tabulated
                Return True
            Case Is = enmProfileType.trapezium
                Return True
            Case Is = enmProfileType.yztable
                Return ltyzTable.IsSymmetric(1) 'note: only checks for the z-values to be symmetric so far
        End Select
    End Function

    Friend Function NumDataPoints() As Integer
        'returns the number of data points that this cross section definition is made of
        Select Case ty
            Case Is = enmProfileType.asymmetricaltrapezium
                Return ltyzTable.XValues.Count
            Case Is = enmProfileType.closedcircle
                Return 0
            Case Is = enmProfileType.closedrectangular
                Return 0
            Case Is = enmProfileType.eggshape
                Return 0
            Case Is = enmProfileType.eggshape2
                Return 0
            Case Is = enmProfileType.opencircle
                Return 0
            Case Is = enmProfileType.sedredge
                Me.Setup.Log.AddWarning("Warning: function NumDataPoints not (yet) supported for cross sections of the sedredge type")
                Return 0
            Case Is = enmProfileType.tabulated
                Return ltlwTable.XValues.Count
            Case Is = enmProfileType.trapezium
                Return 4
            Case Is = enmProfileType.yztable
                Return ltyzTable.XValues.Count
        End Select
    End Function

    Friend Function RemoveVerticalSlot(ByVal MinDepthWidthRatio As Double) As Boolean
        Try
            Dim SkipIdx As List(Of Integer)
            'vertical slots kunnnen alleen bestaan in YZ- of Tabelprofielen
            If ty = enmProfileType.tabulated Then
                Dim Width As Double = ltlwTable.Values1.Values(0)
                Dim Depth As Double = ltlwTable.XValues.Values(1) - ltlwTable.XValues.Values(0)
                If Width > 0 AndAlso Depth / Width > MinDepthWidthRatio Then
                    Dim newTable As New clsSobekTable(Me.Setup)
                    SkipIdx = New List(Of Integer)
                    newTable = ltlwTable.ClonePart(1, ltlwTable.XValues.Count - 1, SkipIdx)
                    ltlwTable = newTable
                End If
            ElseIf ty = enmProfileType.yztable Then
                Dim LowestIdx As Integer = GetLowestLevelIdx()
                Dim LowestKey As String = ltyzTable.XValues.Keys(LowestIdx)
                If LowestIdx > 1 AndAlso LowestIdx < ltyzTable.XValues.Count - 2 Then
                    Dim Width As Double = ltyzTable.XValues.Values(LowestIdx + 1) - ltyzTable.XValues.Values(LowestIdx - 1)
                    Dim Depth As Double = ltyzTable.Values1.Values(LowestIdx - 1) - ltyzTable.Values1.Values(LowestIdx)
                    If Width > 0 AndAlso Depth / Width > MinDepthWidthRatio Then
                        Dim OriginalElevation As Double = Setup.GeneralFunctions.Extrapolate(ltyzTable.XValues.Values(LowestIdx - 2), ltyzTable.Values1.Values(LowestIdx - 2), ltyzTable.XValues.Values(LowestIdx - 1), ltyzTable.Values1.Values(LowestIdx - 1), ltyzTable.XValues.Values(LowestIdx))
                        ltyzTable.Values1.Item(LowestKey) = OriginalElevation
                    End If
                End If
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function RemoveVerticalSlot.")
            Return False
        End Try
    End Function
    Friend Function AddVerticalSlot(ByVal Width As Double, ByVal Depth As Double) As Boolean

        'inserts a vertical slot in the current cross section definition
        Dim myTable As New clsSobekTable(Me.Setup)
        Dim j As Long
        Dim Y1 As Double, Y2 As Double, Z1 As Double, Z2 As Double
        Dim Yleft As Double, Yright As Double, Zleft As Double, Zright As Double

        If ty = enmProfileType.tabulated Then 'tabulated
            'als dit profiel al een vertical slot heeft, slaan we hem over
            If ltlwTable.Values1.Values(0) = 0 AndAlso ltlwTable.Values1.Values(1) = Width Then
                'heeft al een vertical slot, dus overslaan
            Else
                myTable.AddDataPair(2, ltlwTable.XValues.Values(0) - Depth, 0)
                myTable.AddDataPair(2, ltlwTable.XValues.Values(0) - 0.01, Width)
                For j = 0 To ltlwTable.XValues.Count - 1
                    myTable.AddDataPair(2, ltlwTable.XValues.Values(j), Math.Max(ltlwTable.Values1.Values(j), Width))
                Next

                'neem nu de waarden van de nieuwe tabel over in het record
                ltlwTable = myTable
            End If
        ElseIf ty = enmProfileType.trapezium Then
            'must first be converted to YZ-profile to implement vertical slot
            'v1.792: the previous implementation did not take groundlayer into account. This one does.
            ConvertToYZ()           'convert this profile definition to YZ
            Call AddVerticalSlot(Width, Depth)  'and call this function again!

            'Dim profheight As Double = ((aw - bw) / 2) / bs
            'ty = enmProfileType.yztable

            ''make sure the vertical slot is always smaller than the bed width
            'If Width >= bw Then Width = bw / 2

            'ltyzTable.AddDataPair(2, -aw / 2, profheight)
            'ltyzTable.AddDataPair(2, -bw / 2, 0)
            'ltyzTable.AddDataPair(2, -Width / 2, 0)
            'ltyzTable.AddDataPair(2, 0, -Depth)
            'ltyzTable.AddDataPair(2, Width / 2, 0)
            'ltyzTable.AddDataPair(2, bw / 2, 0)
            'ltyzTable.AddDataPair(2, aw / 2, profheight)


        ElseIf ty = enmProfileType.yztable Then 'yz
            Dim LowestIndices As List(Of Long) = GetLowestLevelIndices()

            'LET OP: AFHANDELING VAN PROFIELEN MET 1 DIEP PUNT OF MET MEERDERE VERSCHILT!
            Select Case LowestIndices.Count
                Case Is = 1

                    Dim WidthLeft As Double
                    Dim WidthRight As Double
                    If LowestIndices(0) > 0 Then WidthLeft = ltyzTable.XValues.Values(LowestIndices(0)) - ltyzTable.XValues.Values(LowestIndices(0) - 1) Else WidthLeft = 0
                    If LowestIndices(0) < ltyzTable.XValues.Count - 1 Then WidthRight = ltyzTable.XValues.Values(LowestIndices(0) + 1) - ltyzTable.XValues.Values(LowestIndices(0)) Else WidthRight = 0

                    If LowestIndices(0) = 0 Then
                        'het diepste punt zit helemaal links
                        Me.Setup.Log.AddError("Error implementing vertical slot in profile " & ID & ". Lowest point in profile is located on the leftmost side of the profile.")
                    ElseIf LowestIndices(0) = ltyzTable.XValues.Count - 1 Then
                        'het diepste punt zit helemaal rechts
                        Me.Setup.Log.AddError("Error implementing vertical slot in profile " & ID & ". Lowest point in profile is located on the rightmost side of the profile.")
                    Else
                        'implementeer alle punten ter linker zijde
                        Dim LastLeftIdx As Integer
                        For j = 0 To LowestIndices(0) - 1
                            If Math.Abs(ltyzTable.XValues.Values(j) - ltyzTable.XValues.Values(LowestIndices(0))) > Width / 2 Then
                                myTable.AddDataPair(2, ltyzTable.XValues.Values(j), ltyzTable.Values1.Values(j))
                                LastLeftIdx = j
                            End If
                        Next

                        'voeg het linker punt van het slot toe
                        Y1 = ltyzTable.XValues.Values(LastLeftIdx)
                        Y2 = ltyzTable.XValues.Values(LowestIndices(0))
                        Z1 = ltyzTable.Values1.Values(LastLeftIdx)
                        Z2 = ltyzTable.Values1.Values(LowestIndices(0))
                        Yleft = ltyzTable.XValues.Values(LowestIndices(0)) - Width / 2
                        Zleft = Setup.GeneralFunctions.Interpolate(Y1, Z1, Y2, Z2, Yleft, , True)
                        myTable.AddDataPair(2, Yleft, Zleft)

                        'verdiep het laagste punt met de opgegeven diepte
                        myTable.AddDataPair(2, ltyzTable.XValues.Values(LowestIndices(0)), ltyzTable.Values1.Values(LowestIndices(0)) - Depth)

                        'voeg het rechter punt van het slot toe
                        Dim FirstRightIdx As Integer
                        For j = LowestIndices(0) + 1 To ltyzTable.XValues.Values.Count - 1
                            If Math.Abs(ltyzTable.XValues.Values(j) - ltyzTable.XValues.Values(LowestIndices(0))) > Width / 2 Then
                                FirstRightIdx = j
                                Exit For
                            End If
                        Next
                        Y1 = ltyzTable.XValues.Values(LowestIndices(0))
                        Y2 = ltyzTable.XValues.Values(FirstRightIdx)
                        Z1 = ltyzTable.Values1.Values(LowestIndices(0))
                        Z2 = ltyzTable.Values1.Values(FirstRightIdx)
                        Yright = ltyzTable.XValues.Values(LowestIndices(0)) + Width / 2
                        Zright = Setup.GeneralFunctions.Interpolate(Y1, Z1, Y2, Z2, Yright, , True)
                        myTable.AddDataPair(2, Yright, Zright)

                        'ter rechter zijde
                        For j = FirstRightIdx To ltyzTable.XValues.Count - 1
                            myTable.AddDataPair(2, ltyzTable.XValues.Values(j), ltyzTable.Values1.Values(j))
                        Next

                        'neem nu de waarden van de nieuwe tabel over in het record
                        ltyzTable = myTable
                    End If




                    ''if a friction section must be used, do it here
                    'If AddFrictionSection Then
                    '    Dim CRFR As clsFrictionDatCRFRRecord
                    '    If SbkCase.CFData.Data.FrictionData.FrictionDatCRFRRecords.records.ContainsKey(ID.Trim.ToUpper) Then
                    '        CRFR = SbkCase.CFData.Data.FrictionData.FrictionDatCRFRRecords.records.Item(ID.Trim.ToUpper)
                    '        CRFR.ltysTable = New clsSobekTable(Me.Setup)
                    '        CRFR.ltysTable.AddDataPair(2, 0, Yleft)
                    '        CRFR.ltysTable.AddDataPair(2, Yleft, Yright)
                    '        CRFR.ltysTable.AddDataPair(2, Yright, ltyzTable.XValues.Values(ltyzTable.XValues.Count - 1))
                    '        CRFR.ftysTable.AddDataPair(2, 1, Yleft)
                    '        CRFR.ltysTable.AddDataPair(2, 1, Yright)
                    '        CRFR.ltysTable.AddDataPair(2, 1, ltyzTable.XValues.Values(ltyzTable.XValues.Count - 1))
                    '    End If
                    'End If


                    Return True

                Case Is > 1
                    'we hebben te maken met twee of meer identieke diepste punten naast elkaar.
                    'zorg dat de breedte van het slot kleiner is dan de afstand tussen de buitenste twee diepste punten
                    Dim minWidth As Double = ltyzTable.XValues.Values(LowestIndices(LowestIndices.Count - 1)) - ltyzTable.XValues.Values(LowestIndices(0))
                    If Width >= minWidth Then Width = minWidth / 2

                    'ter linker zijde
                    For j = 0 To LowestIndices(0)
                        myTable.AddDataPair(2, ltyzTable.XValues.Values(j), ltyzTable.Values1.Values(j))
                    Next

                    'voeg het slot in
                    Y1 = ltyzTable.XValues.Values(LowestIndices(0)) + (minWidth - Width) / 2
                    Z1 = ltyzTable.Values1.Values(LowestIndices(0))
                    myTable.AddDataPair(2, Y1, Z1)
                    Y1 = ltyzTable.XValues.Values(LowestIndices(0)) + (minWidth - Width) / 2 + Width / 2
                    Z1 = ltyzTable.Values1.Values(LowestIndices(0)) - Depth
                    myTable.AddDataPair(2, Y1, Z1)
                    Y1 = ltyzTable.XValues.Values(LowestIndices(LowestIndices.Count - 1)) - (minWidth - Width) / 2
                    Z1 = ltyzTable.Values1.Values(LowestIndices(LowestIndices.Count - 1))
                    myTable.AddDataPair(2, Y1, Z1)

                    'ter rechter zijde; start met het voormalige diepste punt aan de rechter kant
                    For j = LowestIndices(LowestIndices.Count - 1) To ltyzTable.XValues.Count - 1
                        myTable.AddDataPair(2, ltyzTable.XValues.Values(j), ltyzTable.Values1.Values(j))
                    Next

                    'neem nu de waarden van de nieuwe tabel over in het record
                    ltyzTable = myTable
                    Return True

            End Select


        End If

    End Function

    Friend Function getRightYZBankIdx() As Long
        'this routine finds the left embankment index from the yz table
        'it does so by finding the highest factor between embankment slope and main channel slope
        Dim lowestIdx As Long = GetLowestLevelIdx()
        Dim MainSlope As Double, BankSlope As Double, BendFact As Double, MaxBendFact As Double, MaxBendIdx As Long

        Dim i As Long
        MaxBendIdx = (ltyzTable.XValues.Count - 1 + lowestIdx) / 2 'default value for the embankment is halfway the right side

        For i = lowestIdx + 1 To ltyzTable.XValues.Count - 2
            BankSlope = (ltyzTable.Values1.Values(i + 1) - ltyzTable.Values1.Values(i)) / (ltyzTable.XValues.Values(i + 1) - ltyzTable.XValues.Values(i))
            MainSlope = (ltyzTable.Values1.Values(i) - ltyzTable.Values1.Values(i - 1)) / (ltyzTable.XValues.Values(i) - ltyzTable.XValues.Values(i - 1))
            If BankSlope > 0 AndAlso MainSlope > 0 Then
                BendFact = MainSlope / BankSlope
                If BendFact > MaxBendFact Then
                    MaxBendFact = BendFact
                    MaxBendIdx = i
                End If
            End If
        Next

        Return MaxBendIdx

    End Function


    Friend Function TableToYZ(ByVal UseTotalWidth As Boolean) As Boolean

        'this routine converts a cross section defenition of type tabulated (or river profile for that matter) to YZ
        Dim i As Long

        Try
            Dim Z As Dictionary(Of String, Single) = ltlwTable.XValues
            Dim Y As Dictionary(Of String, Single)

            If UseTotalWidth Then
                Y = ltlwTable.Values1
            Else
                Y = ltlwTable.Values2
            End If

            ty = enmProfileType.yztable
            For i = ltlwTable.XValues.Count - 1 To 0 Step -1
                ltyzTable.AddDataPair(2, -Y.Values(i) / 2, Z.Values(i))
            Next
            For i = 0 To ltlwTable.XValues.Count - 1
                ltyzTable.AddDataPair(2, Y.Values(i) / 2, Z.Values(i))
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function TableToYZ of class clsProfileDefRecord.")
            Return False
        End Try

    End Function

    Friend Function InsertNVOLeft(ByVal ProfileShift As Double, ByVal TargetLevel As Double, ByVal Width As Double, ByVal Depth As Double) As Boolean
        'This routine inserts a nature-friendly kerb into the current profile definition
        Dim DeepestIdx As Long, i As Long
        Dim curDist As Double, nextDist As Double
        Dim curY As Double, nextY As Double
        Dim newTable As New clsSobekTable(Me.Setup)
        Dim LeftNVO As Double
        Dim LeftTL As Double
        Dim LeftShift As Double
        Dim Done As Boolean

        Try
            Select Case ty
                Case Is = 10

                    newTable = New clsSobekTable(Me.Setup)

                    'find important intersection points
                    DeepestIdx = ltyzTable.GetMinValueIdx(1)

                    'find the x-values that correspond with the intersection with target level and target level - depth
                    If Not ltyzTable.getIntersectionXVal(1, False, True, TargetLevel - ProfileShift - Depth, 0, DeepestIdx, LeftNVO) Then Throw New Exception("Searched Left Side Elevation (Targetlevel - Cross Section Shift - Depth) of " & TargetLevel - ProfileShift - Depth & " lies outside the range of the profile (definition) " & ID)
                    If Not ltyzTable.getIntersectionXVal(1, False, True, TargetLevel - ProfileShift, 0, DeepestIdx, LeftTL) Then Throw New Exception("Searched Left Side Elevation (Targetlevel - Cross Section Shift - Depth) of " & TargetLevel - ProfileShift & " lies outside the range of the profile (definition) " & ID)
                    LeftShift = Width - (LeftNVO - LeftTL) 'the amount by which all points left of the NVO need to be shifted to the left

                    'now start filling the new table
                    For i = 0 To DeepestIdx - 1
                        curDist = ltyzTable.XValues.Values(i)
                        curY = ltyzTable.Values1.Values(i)
                        nextDist = ltyzTable.XValues.Values(i + 1)
                        nextY = ltyzTable.Values1.Values(i + 1)

                        If curDist <= LeftTL Then
                            If curY >= (TargetLevel - ProfileShift) Then
                                newTable.AddDataPair(2, curDist - LeftShift, curY)
                            End If
                        ElseIf curDist <= LeftNVO AndAlso Done = False Then
                            'add the interpolated points
                            newTable.AddDataPair(2, LeftNVO - Width, TargetLevel - ProfileShift)
                            newTable.AddDataPair(2, LeftNVO, TargetLevel - Depth - ProfileShift)
                            Done = True
                        ElseIf curDist > LeftNVO Then
                            newTable.AddDataPair(2, curDist, curY)
                        End If
                    Next

                    'from center to right
                    For i = DeepestIdx To ltyzTable.XValues.Count - 1
                        curDist = ltyzTable.XValues.Values(i)
                        curY = ltyzTable.Values1.Values(i)
                        newTable.AddDataPair(2, curDist, curY)
                    Next

                    ltyzTable = newTable
                    Return True

                Case Else
                    Throw New Exception("Cross Section definition type not (yet) supported for construction of nature-friendly kerbs: " & ID)
            End Select

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Function InsertNVORight(ByVal ProfileShift As Double, ByVal TargetLevel As Double, ByVal Width As Double, ByVal Depth As Double) As Boolean
        'This routine inserts a nature-friendly kerb into the current profile definition
        Dim DeepestIdx As Long, i As Long
        Dim curDist As Double, prevDist As Double
        Dim curY As Double, prevY As Double
        Dim newTable As New clsSobekTable(Me.Setup)
        Dim RightNVO As Double
        Dim RightTL As Double
        Dim RightShift As Double
        Dim Done As Boolean

        Try
            Select Case ty
                Case Is = 10

                    newTable = New clsSobekTable(Me.Setup)

                    'find important intersection points
                    DeepestIdx = ltyzTable.GetMinValueIdx(1)

                    'find the x-values that correspond with the intersection with target level and target level - depth
                    If Not ltyzTable.getIntersectionXVal(1, True, False, TargetLevel - ProfileShift - Depth, DeepestIdx, ltyzTable.XValues.Count - 1, RightNVO) Then Throw New Exception("Searched Right Side Elevation (Targetlevel - Cross Section Shift - Depth) of " & TargetLevel - ProfileShift - Depth & " lies outside the range of the profile (definition) " & ID)
                    If Not ltyzTable.getIntersectionXVal(1, True, False, TargetLevel - ProfileShift, DeepestIdx, ltyzTable.XValues.Count - 1, RightTL) Then Throw New Exception("Searched Right Side Elevation (Targetlevel - Cross Section Shift) of " & TargetLevel - ProfileShift & " lies outside the range of the profile (definition) " & ID)
                    RightShift = Width - (RightTL - RightNVO) 'the amount by which all points left of the NVO need to be shifted to the left

                    'now start filling the new table. start with left to right
                    For i = 0 To DeepestIdx
                        curDist = ltyzTable.XValues.Values(i)
                        curY = ltyzTable.Values1.Values(i)
                        newTable.AddDataPair(2, curDist, curY)
                    Next

                    'now the right side, which will get the NVO
                    For i = DeepestIdx + 1 To ltyzTable.XValues.Count - 1
                        prevDist = ltyzTable.XValues.Values(i - 1)
                        prevY = ltyzTable.Values1.Values(i - 1)
                        curDist = ltyzTable.XValues.Values(i)
                        curY = ltyzTable.Values1.Values(i)

                        If curDist >= RightTL AndAlso Done = True Then
                            If curY >= (TargetLevel - ProfileShift) Then
                                newTable.AddDataPair(2, curDist + RightShift, curY)
                            End If
                        ElseIf curDist >= RightNVO AndAlso Done = False Then
                            'add the interpolated points
                            newTable.AddDataPair(2, RightNVO, TargetLevel - Depth - ProfileShift)
                            newTable.AddDataPair(2, RightNVO + Width, TargetLevel - ProfileShift)
                            Done = True
                        ElseIf curDist < RightNVO Then
                            newTable.AddDataPair(2, curDist, curY)
                        End If
                    Next

                    ltyzTable = newTable
                    Return True

                Case Else
                    Throw New Exception("Cross Section definition type not (yet) supported for construction of nature-friendly kerbs: " & ID)
            End Select

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function
    Friend Sub Write(ByRef myWriter As StreamWriter)
        Dim i As Integer

        Try
            Select Case ty
                Case Is = 0 'tabelprofielen
                    If Not IsRiverProfile Then
                        myWriter.WriteLine("CRDS id '" & ID & "' nm '" & nm & "' ty " & ty & " wm " & wm & " w1 " & W1 & " w2 " & W2 & " sw 0 gl " & gl & " gu " & gu & " lt lw")
                        myWriter.WriteLine("TBLE")
                        If ltlwTable Is Nothing Then
                            Me.Setup.Log.AddError("Elevation-width-table not present for cross section definition of tabulated type: " & ID)
                        Else
                            If Not ltlwTable.Values2 Is Nothing AndAlso ltlwTable.Values2.Count = ltlwTable.XValues.Count Then
                                For i = 0 To ltlwTable.XValues.Count - 1
                                    myWriter.WriteLine(ltlwTable.XValues.Values(i) & " " & ltlwTable.Values1.Values(i) & " " & ltlwTable.Values2.Values(i) & " <")
                                Next
                            ElseIf Not ltlwTable.Values1 Is Nothing AndAlso ltlwTable.Values1.Count = ltlwTable.XValues.Count Then
                                For i = 0 To ltlwTable.XValues.Count - 1
                                    myWriter.WriteLine(ltlwTable.XValues.Values(i) & " " & ltlwTable.Values1.Values(i) & " " & ltlwTable.Values1.Values(i) & " <")
                                Next
                            End If
                        End If
                        myWriter.WriteLine("tble crds")
                    Else
                        'river profiles are distinguished from rural tabulated profiles through the existance of the dk token
                        myWriter.WriteLine("CRDS id '" & ID & "' nm '" & nm & "' ty " & ty & " wm " & wm & " w1 " & W1 & " w2 " & W2 & " sw " & sw & " bl " & bl & " lt lw 'Level Width' PDIN 0 0 '' pdin CLTT 'Level [m]' 'Tot. Width [m]' 'Flow width [m]' cltt CLID '(null)' '(null)' '(null)' clid")
                        myWriter.WriteLine("TBLE")
                        If ltlwTable Is Nothing Then
                            Me.Setup.Log.AddError("Elevation-width-table not present for cross section definition of tabulated type: " & ID)
                        Else
                            If Not ltlwTable.Values2 Is Nothing AndAlso ltlwTable.Values2.Count = ltlwTable.XValues.Count Then
                                For i = 0 To ltlwTable.XValues.Count - 1
                                    myWriter.WriteLine(ltlwTable.XValues.Values(i) & " " & ltlwTable.Values1.Values(i) & " " & ltlwTable.Values2.Values(i) & " <")
                                Next
                            ElseIf Not ltlwTable.Values1 Is Nothing AndAlso ltlwTable.Values1.Count = ltlwTable.XValues.Count Then
                                For i = 0 To ltlwTable.XValues.Count - 1
                                    myWriter.WriteLine(ltlwTable.XValues.Values(i) & " " & ltlwTable.Values1.Values(i) & " " & ltlwTable.Values1.Values(i) & " <")
                                Next
                            End If
                        End If
                        myWriter.WriteLine("tble")
                        myWriter.WriteLine(" dk " & dk & " dc " & dc & " db " & db & " df " & df & " dt " & dt & " bw " & bw & " bs " & bs & " aw " & aw & " rd " & rd & " ll " & ll & " rl " & rl & " lw " & lw & " rw " & rw)
                        myWriter.WriteLine("crds")
                    End If
                Case Is = 1 '
                    myWriter.WriteLine("CRDS id '" & ID & "' nm '" & nm & "' ty " & ty & " bl " & bl & " bw " & bw & " bs " & bs & " aw " & aw & " sw " & sw & "  gl " & gl & " gu " & gu & " crds")
                Case Is = 4 'rond
                    myWriter.WriteLine("CRDS id '" & ID & "' nm '" & nm & "' ty " & ty & " bl " & bl & " rd " & rd & "  gl " & gl & " gu " & gu & " crds")
                Case Is = 6 'egg
                    myWriter.WriteLine("CRDS id '" & ID & "' nm '" & nm & "' ty " & ty & " bl " & bl & " bo " & bo & " gu " & gu & " gl " & gl & "         crds")
                Case Is = 10 'yz-profiel
                    myWriter.WriteLine("CRDS id '" & ID & "' nm '" & nm & "' ty " & ty & " lu " & Convert.ToInt32(lu) & " st 0 lt sw 0 " & ltswMaaiveld & " lt yz") 'let op: st (storage type altijd op 0!!!!)
                    myWriter.WriteLine("TBLE")
                    If ltyzTable Is Nothing Then
                        Me.Setup.Log.AddError("yz-table not present for cross section definition of yz type: " & ID)
                    Else
                        For i = 0 To ltyzTable.XValues.Count - 1
                            myWriter.WriteLine(ltyzTable.XValues.Values(i) & " " & ltyzTable.Values1.Values(i) & " <")
                        Next
                    End If
                    myWriter.WriteLine("tble")
                    myWriter.WriteLine("gl " & gl)
                    myWriter.WriteLine("gu " & gu)
                    myWriter.WriteLine("crds")
                Case Is = 11 'asymmetrical trapezium
                    Dim mystr As String = "CRDS id '" & ID & "' nm '" & nm & "' ty " & ty & " st 0 lt sw" 'let op: st (storage type) altijd op nul!!!
                    If ltswTable Is Nothing Then
                        Me.Setup.Log.AddError("Table not present for cross section definition of type asymmetrical trapezium: " & ID)
                    ElseIf ltswTable.XValues.Count = 0 Then
                        myWriter.WriteLine(mystr & " 0 " & ltswMaaiveld & " lt yz")
                    Else
                        myWriter.WriteLine(mystr)
                        myWriter.WriteLine("TBLE")
                        For i = 0 To ltswTable.XValues.Count - 1
                            myWriter.WriteLine(ltswTable.XValues.Values(i) & " " & ltswTable.Values1.Values(i) & " <")
                        Next
                        myWriter.WriteLine("tble")
                        myWriter.WriteLine("lt yz")
                    End If

                    'de tabel met yz-waarden
                    myWriter.WriteLine("TBLE")
                    If ltyzTable Is Nothing Then
                        Me.Setup.Log.AddError("YZ-table not present for cross section definition of type asymmetrical trapezium: " & ID)
                    Else
                        For i = 0 To ltyzTable.XValues.Count - 1
                            myWriter.WriteLine(ltyzTable.XValues.Values(i) & " " & ltyzTable.Values1.Values(i) & " <")
                        Next
                    End If
                    myWriter.WriteLine("tble")
                    myWriter.WriteLine("gl " & gl)
                    myWriter.WriteLine("gu " & gu)
                    myWriter.WriteLine("crds")
            End Select
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in sub Write of class clsProfileDefRecord.")
        End Try

    End Sub

    Friend Function getMinimumElevation() As Double
        Dim myMin As Double = 9.0E+99
        If ty = 0 Then  'tabulated
            myMin = ltlwTable.XValues.Values(0)
        ElseIf ty = 1 Then 'trapezoidal
            myMin = bl 'the vertical shift determines the vertical elevation
        ElseIf ty = 2 OrElse ty = 4 Then 'circle
            myMin = 0 'the vertical shift determines the vertical elevation
        ElseIf ty = 10 OrElse ty = 11 Then 'yz or asym trapezium
            For i = 0 To ltyzTable.XValues.Values.Count - 1
                If ltyzTable.Values1.Values(i) < myMin Then myMin = ltyzTable.Values1.Values(i)
            Next
        End If
        Return myMin
    End Function

    Friend Function getMaximumElevation() As Double
        Dim myMax As Double = -9.0E+99
        If ty = 0 Then  'tabulated
            myMax = ltlwTable.XValues.Values(ltlwTable.XValues.Count - 1)
        ElseIf ty = 1 Then 'trapezoidal
            myMax = bl + (aw - bw) / (2 * bs) 'bed level + slope * (max_width - bed_width)
        ElseIf ty = 2 OrElse ty = 4 Then 'circle
            myMax = rd
        ElseIf ty = 10 OrElse ty = 11 Then 'yz or asym trapezium
            For i = 0 To ltyzTable.XValues.Values.Count - 1
                If ltyzTable.Values1.Values(i) > myMax Then myMax = ltyzTable.Values1.Values(i)
            Next
        End If
        Return myMax
    End Function


    Friend Function getMaximumWidth() As Double
        If ty = 0 Then  'tabulated
            Return ltlwTable.Values1.Values(ltlwTable.Values1.Count - 1)
        ElseIf ty = 1 Then 'trapezoidal
            Return aw
        ElseIf ty = 2 OrElse ty = 4 Then 'circle
            Return rd * 2
        ElseIf ty = 10 OrElse ty = 11 Then 'yz or asym trapezium
            Return ltyzTable.XValues.Values(ltyzTable.XValues.Count - 1) - ltyzTable.XValues.Values(0)
        End If
    End Function

    Friend Function getMinimumWidth() As Double
        If ty = 0 Then  'tabulated
            Return ltlwTable.Values1.Values(0)
        ElseIf ty = 1 Then 'trapezoidal
            Return bw
        ElseIf ty = 2 OrElse ty = 4 Then 'circle
            Return rd * 2
        ElseIf ty = 10 OrElse ty = 11 Then 'yz or asym trapezium
            'here we'll return the width of the two points surrounding the lowest point
            Dim lowestIdx As Integer = ltyzTable.GetMinValueIdx(0)
            Dim fromIdx As Integer = Math.Max(0, lowestIdx - 1)
            Dim toIdx As Integer = Math.Min(lowestIdx + 1, ltyzTable.XValues.Count - 1)
            Return ltyzTable.XValues.Values(toIdx) - ltyzTable.XValues.Values(fromIdx)
        End If
    End Function

    Friend Function getProfileWidthAtLevel(ByVal Level As Double) As Double
        'NOTE: level should be expressed as the elevation relative to the profile definition itself
        'in other words: when passing the level variable we have already corrected for the vertical shift of our cross section itself
        If ty = 0 Then  'tabulated
            Return ltlwTable.InterpolateFromXValues(Level, 1, enmExtrapolationMethod.MakeZero, enmExtrapolationMethod.KeepConstant)
        ElseIf ty = 1 Then 'trapezoidal
            If Level > (aw - bw) / 2 / bs Then
                Return aw
            Else
                Return bw + bs * Level * 2
            End If
        ElseIf ty = 2 OrElse ty = 4 Then 'circle
            If Level <= 0 OrElse Level >= rd * 2 Then
                Return 0
            Else
                Dim y As Double = Math.Abs(rd - Level)
                'apply pythagoras to compute the cross section width at the current level
                Return 2 * Math.Sqrt(rd ^ 2 - y ^ 2)
            End If
            Return rd * 2
        ElseIf ty = 10 OrElse ty = 11 Then 'yz or asym trapezium
            Return getProfileWidthFromYZTable(Level)
        Else
            Me.Setup.Log.AddError("Error in function getProfileWidthAtLevel. cross section type not supported: ty =" & ty)
        End If
    End Function

    Public Function getProfileWidthFromYZTable(ByVal Z As Double) As Double
        Try
            Dim Y1 As Double = ltyzTable.XValues.Values(0)
            Dim Y2 As Double = ltyzTable.XValues.Values(ltyzTable.XValues.Count - 1)
            Dim Zmin As Double = ltyzTable.getMinValue(1)
            Dim i As Integer

            If Z < Zmin Then Return 0

            'find the left y value
            If Z > ltyzTable.Values1.Values(0) Then
                Y1 = ltyzTable.XValues.Values(0)
            Else
                For i = 0 To ltyzTable.XValues.Count - 2
                    If ltyzTable.Values1.Values(i) >= Z AndAlso ltyzTable.Values1.Values(i + 1) < Z Then
                        Y1 = Setup.GeneralFunctions.Interpolate(ltyzTable.Values1.Values(i), ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i + 1), ltyzTable.XValues.Values(i + 1), Z)
                        Exit For
                    End If
                Next
            End If

            'find the rigth y value
            If Z > ltyzTable.Values1.Values(ltyzTable.Values1.Values.Count - 1) Then
                Y2 = ltyzTable.XValues.Values(ltyzTable.XValues.Values.Count - 1)
            Else
                For i = ltyzTable.XValues.Count - 1 To 1 Step -1
                    If ltyzTable.Values1.Values(i) >= Z AndAlso ltyzTable.Values1.Values(i - 1) < Z Then
                        Y2 = Setup.GeneralFunctions.Interpolate(ltyzTable.Values1.Values(i), ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i - 1), ltyzTable.XValues.Values(i - 1), Z)
                        Exit For
                    End If
                Next
            End If

            If Not Y2 > Y1 Then Throw New Exception("Error determining cross section width for yz cross section definition " & ID & " at elevation " & Z)
            Return (Y2 - Y1)
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getProfileWidthFromYZTable.")
            Return 0
        End Try



    End Function


    Friend Function FindLeftFloodPlainSection() As Double
        'Date: 30-5-2013
        'Author: Siebe Bosch
        'Description: Finds the location (Y-value) of the left floodplain in an YZ-profile definition
        'The function will look for a sharp drop in steepness. In other words: the second derivative!
        'where the second derivative (dy2/dx2)/(dy1/dx1) is smallest, the a transition to a floodplain is found
        Dim LowestIdx As Long = GetLowestLevelIdx(), i As Long, BankIdx As Long
        Dim PrevY As Double, PrevZ As Double, CurY As Double, CurZ As Double, NextY As Double, NextZ As Double
        Dim PrevSlope As Double, NextSlope As Double, minSlope As Double, Horizontal As Integer, Found As Boolean

        Try
            If ty = 10 Then

                If LowestIdx >= 1 Then
                    BankIdx = LowestIdx / 2                         'initialize our floodplain at halfway the left side
                    minSlope = 999                             'second derivative >1 means that the slope is increasing with distance
                    Found = False

                    'we prefer the crossing from very steep to shallow
                    'therefore start searching within very steep slopes to find a nice transition towards a plain
                    For Horizontal = 1 To 20 'start with 1:1, then 1:2, etc.

                        If Found Then Exit For
                        For i = LowestIdx - 1 To 1 Step -1

                            'find the co-ordinates to calculate the derivatives
                            PrevY = ltyzTable.XValues.Values(i + 1)
                            PrevZ = ltyzTable.Values1.Values(i + 1)
                            CurY = ltyzTable.XValues.Values(i)
                            CurZ = ltyzTable.Values1.Values(i)
                            NextY = ltyzTable.XValues.Values(i - 1)
                            NextZ = ltyzTable.Values1.Values(i - 1)

                            PrevSlope = (CurZ - PrevZ) / (PrevY - CurY)
                            NextSlope = (NextZ - CurZ) / (CurY - NextY)

                            If PrevSlope > 1 / Horizontal Then
                                If NextSlope < 1 / 5 AndAlso NextSlope < PrevSlope Then
                                    If NextSlope / PrevSlope < minSlope Then
                                        minSlope = NextSlope / PrevSlope
                                        BankIdx = i
                                        Found = True
                                    End If
                                End If
                            End If
                        Next

                    Next
                    Return ltyzTable.XValues.Values(BankIdx)
                Else
                    Me.Setup.Log.AddWarning("Lowest point for cross section definition " & ID & " lies on the utmost left side. Could not determine floodplain.")
                    Return ltyzTable.XValues.Values(0)
                End If

            End If
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try

    End Function

    Friend Function FindRightFloodPlainSection() As Double
        'Date: 30-5-2013
        'Author: Siebe Bosch
        'Description: Finds the location (Y-value) of the right floodplain in an YZ-profile definition
        'The function will look for a sharp drop in steepness. In other words: the second derivative!
        'where the second derivative (dy2/dx2)/(dy1/dx1) is smallest, the a transition to a floodplain is found
        Dim LowestIdx As Long = GetLowestLevelIdx(), i As Long, BankIdx As Long
        Dim PrevY As Double, PrevZ As Double, CurY As Double, CurZ As Double, NextY As Double, NextZ As Double
        Dim PrevSlope As Double, NextSlope As Double, minSlope As Double, Found As Boolean

        Try
            If ty = 10 Then
                If LowestIdx < ltyzTable.XValues.Count - 1 Then
                    BankIdx = LowestIdx + ((ltyzTable.XValues.Count - 1) - LowestIdx) / 2   'initialize our floodplain at halfway the right side
                    minSlope = 999                                         'second derivative >1 means that the slope is increasing with distance
                    Found = False

                    'we prefer the crossing from very steep to shallow
                    'therefore start searching within very steep slopes to find a nice transition towards a plain
                    For Horizontal = 1 To 20 'start with 1:1, then 1:2, etc.

                        If Found Then Exit For
                        For i = LowestIdx + 1 To ltyzTable.XValues.Count - 2

                            'find the co-ordinates to calculate the derivatives
                            PrevY = ltyzTable.XValues.Values(i - 1)
                            PrevZ = ltyzTable.Values1.Values(i - 1)
                            CurY = ltyzTable.XValues.Values(i)
                            CurZ = ltyzTable.Values1.Values(i)
                            NextY = ltyzTable.XValues.Values(i + 1)
                            NextZ = ltyzTable.Values1.Values(i + 1)

                            PrevSlope = (CurZ - PrevZ) / (CurY - PrevY)
                            NextSlope = (NextZ - CurZ) / (NextY - CurY)

                            If PrevSlope > 1 / Horizontal Then
                                If NextSlope < 1 / 5 AndAlso NextSlope < PrevSlope Then
                                    If NextSlope / PrevSlope < minSlope Then
                                        minSlope = NextSlope / PrevSlope
                                        BankIdx = i
                                        Found = True
                                    End If
                                End If
                            End If
                        Next

                        'If PrevSlope > 0 AndAlso NextSlope <> 0 Then
                        '  If (NextSlope / PrevSlope) < minDerivative Then
                        '    minDerivative = NextSlope / PrevSlope
                        '    BankIdx = i
                        '  End If
                        'End If
                        'If PrevSlope > 1 / 3 Then
                        '  If (NextSlope - PrevSlope) < minDerivative Then
                        '    minDerivative = NextSlope / PrevSlope
                        '    BankIdx = i
                        '  End If
                        'End If
                    Next
                    Return ltyzTable.XValues.Values(BankIdx)
                ElseIf LowestIdx < ltyzTable.XValues.Count - 1 Then
                    Return ltyzTable.XValues.Values(LowestIdx + 1)
                Else
                    'apparently the lowest point lies on the edge of the cross section
                    Me.Setup.Log.AddWarning("Cross section definition " & ID & " has lowest point on the utmost side. No floodplain could be determined.")
                    Return ltyzTable.XValues.Values(LowestIdx)
                End If

            End If
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
        End Try

    End Function


    Friend Function GetHighestLevelIdx() As Integer
        'geeft het index van het laagste punt van de profielDEFINITIE terug (let op: dus niet van het hele profiel
        'Daarvoor is ook info uit profile.dat nodig)
        Dim i As Integer, myMax As Double = -999999999, myMaxY As Double = 0
        Dim maxIdx As Integer = 0
        If ty = 10 Then
            For i = 0 To ltyzTable.Values1.Count - 1
                If ltyzTable.Values1.Values(i) > myMax Then
                    myMax = ltyzTable.Values1.Values(i)
                    myMaxY = ltyzTable.XValues.Values(i)
                    maxIdx = i
                End If
            Next
        End If
        Return maxIdx
    End Function

    Friend Function GetHighestLevelLeftIdx(ByVal LowestIdx As Integer) As Integer
        'geeft het index van het laagste punt van de profielDEFINITIE terug (let op: dus niet van het hele profiel
        'Daarvoor is ook info uit profile.dat nodig)
        Dim i As Integer, myMax As Double = -999999999, myMaxY As Double = 0
        Dim maxIdx As Integer = 0
        If ty = 10 Then
            For i = LowestIdx To 0 Step -1
                If ltyzTable.Values1.Values(i) > myMax Then
                    myMax = ltyzTable.Values1.Values(i)
                    myMaxY = ltyzTable.XValues.Values(i)
                    maxIdx = i
                End If
            Next
        End If
        Return maxIdx
    End Function

    Friend Function GetHighestLevelRightIdx(ByVal LowestIdx As Integer) As Integer
        'geeft het index van het laagste punt van de profielDEFINITIE terug (let op: dus niet van het hele profiel
        'Daarvoor is ook info uit profile.dat nodig)
        Dim i As Integer, myMax As Double = -999999999, myMaxY As Double = 0
        Dim maxIdx As Integer = 0
        If ty = 10 Then
            For i = LowestIdx To ltyzTable.Values1.Count - 1
                If ltyzTable.Values1.Values(i) > myMax Then
                    myMax = ltyzTable.Values1.Values(i)
                    myMaxY = ltyzTable.XValues.Values(i)
                    maxIdx = i
                End If
            Next
        End If
        Return maxIdx
    End Function

    Friend Function GetLowestLevelIdx() As Integer
        'geeft het index van het laagste punt van de profielDEFINITIE terug (let op: dus niet van het hele profiel
        'Daarvoor is ook info uit profile.dat nodig)
        Dim i As Integer, myMin As Double = 999999999, myMinY As Double = 0

        Dim minIdx As Integer = 0
        If ty = 10 Then
            For i = 0 To ltyzTable.Values1.Count - 1
                If ltyzTable.Values1.Values(i) < myMin Then
                    myMin = ltyzTable.Values1.Values(i)
                    myMinY = ltyzTable.XValues.Values(i)
                    minIdx = i
                End If
            Next
        End If
        Return minIdx
    End Function

    Friend Function GetLowestLevelIndices() As List(Of Long)
        'geeft een of meerdere indexnummer terug van het laagste punt van de profielDEFINITIE
        'als meerdere punten naast elkaar dezelfde z-waarde hebben, geeft hij dus alle punten terug
        Dim myList As New List(Of Long)

        Dim i As Integer, myMin As Double = 999999999
        Dim minIdx As Integer = 0
        If ty = 10 Then
            For i = 0 To ltyzTable.Values1.Count - 1
                If ltyzTable.Values1.Values(i) < myMin Then
                    'nieuw laagste punt gevonden. reset de lijst en voeg dit punt toe
                    myList = New List(Of Long)
                    myList.Add(i)
                    minIdx = i
                    myMin = ltyzTable.Values1.Values(i)
                ElseIf ltyzTable.Values1.Values(i) = myMin AndAlso i = minIdx + 1 Then
                    'naastgelegen even laag punt gevonden
                    myList.Add(i)
                    minIdx = i
                End If
            Next
        End If
        Return myList

    End Function

    Friend Function GetLowestLevel(Optional ByVal ReturnDistance As Boolean = False) As Double
        'geeft het laagste punt van de profielDEFINITIE terug (let op: dus niet van het hele profiel
        'Daarvoor is ook info uit profile.dat nodig)
        Dim i As Integer, myMin As Double = 999999999, myMinY As Double = 0
        If ty = 10 Then
            For i = 0 To ltyzTable.Values1.Count - 1
                If ltyzTable.Values1.Values(i) < myMin Then
                    myMin = ltyzTable.Values1.Values(i)
                    myMinY = ltyzTable.XValues.Values(i)
                End If
            Next
        End If
        If ReturnDistance Then
            Return myMinY
        Else
            Return myMin
        End If
    End Function

    Friend Function GetHighestLevel(Optional ByVal ReturnDistance As Boolean = False) As Double
        'geeft het hoogste punt van de profielDEFINITIE terug (let op: dus niet van het hele profiel
        'voor dat laatste is ook info uit profile.dat nodig)
        Dim i As Integer, myMax As Double = -999999999, myMaxY As Double = 0
        If ty = 10 Then
            For i = 0 To ltyzTable.Values1.Count - 1
                If ltyzTable.Values1.Values(i) > myMax Then
                    myMax = ltyzTable.Values1.Values(i)
                    myMaxY = ltyzTable.XValues.Values(i)
                End If
            Next
        End If
        If ReturnDistance Then
            Return myMaxY
        Else
            Return myMax
        End If
    End Function
    Friend Function GetLowestEmbankmentLevel(ByVal YZ As Boolean) As Double
        'geeft de laagste dijkhoogte (links of rechts dus) terug
        'kan alleen voor YZ-profielen
        Dim Left As Double, Right As Double
        If ty = 10 AndAlso YZ = True Then
            Left = ltyzTable.Values1.Values(0)
            Right = ltyzTable.Values1.Values(ltyzTable.Values1.Values.Count - 1)
            Return Math.Min(Left, Right)
        End If

    End Function

    Friend Function GetHighestEmbankmentLevel(ByVal YZ As Boolean) As Double
        'geeft de hoogste dijkhoogte (links of rechts dus) terug
        'kan alleen voor YZ-profielen
        Dim Left As Double, Right As Double
        If ty = 10 AndAlso YZ = True Then
            Left = ltyzTable.Values1.Values(0)
            Right = ltyzTable.Values1.Values(ltyzTable.Values1.Values.Count - 1)
            Return Math.Min(Left, Right)
        End If

    End Function

    Friend Sub AddGroundLayer(ByRef setup As clsSetup, ByVal myValue As Double, Optional ByVal StrictlyHorizontal As Boolean = True)
        'this routine adds a ground layer to a cross section. 
        'the provided groundlayer thickness can either be expressed as depth (m) or as a percentage of the total profile height
        'for the latter, the variable DepthAsPercentage needs to be set to true. False is default
        Dim newTable As clsSobekTable
        Dim minBedLevel As Double
        Dim maxBedLevel As Double
        Dim newBedLevel As Double
        Dim Y As Double, Z As Double
        Dim Y2 As Double, Z2 As Double
        Dim i As Integer, n As Integer

        If ty = enmProfileType.yztable OrElse ty = enmProfileType.asymmetricaltrapezium Then
            newTable = New clsSobekTable(Me.Setup)
            minBedLevel = GetLowestLevel()
            maxBedLevel = GetHighestLevel()

            'determine the new bed level
            newBedLevel = minBedLevel + myValue

            If Not StrictlyHorizontal Then
                'this is a loosly interpreted version: it simply lifts up all z-values to at least the lowest level + groundlayer
                'v1.79: fixed a bug in this version.
                For i = 0 To ltyzTable.XValues.Count - 1
                    Z = ltyzTable.Values1.Values(i)
                    If Z < newBedLevel Then
                        newTable.XValues.Add(Str(i), ltyzTable.XValues.Values(i))
                        newTable.Values1.Add(Str(i), newBedLevel)
                    Else
                        newTable.XValues.Add(Str(i), ltyzTable.XValues.Values(i))
                        newTable.Values1.Add(Str(i), ltyzTable.Values1.Values(i))
                    End If
                Next
                ltyzTable = newTable

                'v1.792 store the information that this groundlayer has been implemented in the profile definition itself
                GroundLayerThicknessImplementedInDefinition = myValue

            Else
                'this is the strict version. It creates a perfectly horizontal ground layer and in order to do so it will insert new yz-points at the
                'at the locations where the ground layer and the original bed intersect
                For i = 0 To ltyzTable.XValues.Count - 2
                    n += 1
                    Y = ltyzTable.XValues.Values(i)
                    Z = ltyzTable.Values1.Values(i)
                    Y2 = ltyzTable.XValues.Values(i + 1)
                    Z2 = ltyzTable.Values1.Values(i + 1)

                    If Z >= newBedLevel And Z2 >= newBedLevel Then
                        newTable.XValues.Add(Str(n), Y)
                        newTable.Values1.Add(Str(n), Z)
                    ElseIf Z >= newBedLevel And Z2 < newBedLevel Then
                        'eerste punt gewoon toevoegen
                        newTable.XValues.Add(Str(n), Y)
                        newTable.Values1.Add(Str(n), Z)

                        n += 1
                        'nieuw punt toevoegen: tussen eerste en tweede punt interpoleren
                        newTable.XValues.Add(Str(n), setup.GeneralFunctions.Interpolate(Z, Y, Z2, Y2, newBedLevel))
                        newTable.Values1.Add(Str(n), newBedLevel)

                    ElseIf Z < newBedLevel And Z2 < newBedLevel Then
                        'ophogen
                        newTable.XValues.Add(Str(n), Y)
                        newTable.Values1.Add(Str(n), newBedLevel)
                    ElseIf Z < newBedLevel And Z2 >= newBedLevel Then
                        'eerste punt ophogen
                        newTable.XValues.Add(Str(n), Y)
                        newTable.Values1.Add(Str(n), newBedLevel)

                        'nieuw punt toevoegen: tussen eerste en tweede punt interpoleren
                        n += 1
                        newTable.XValues.Add(Str(n), setup.GeneralFunctions.Interpolate(Z, Y, Z2, Y2, newBedLevel))
                        newTable.Values1.Add(Str(n), newBedLevel)
                    End If
                Next

                'dan het laatste punt nog even afhandelen
                Y = ltyzTable.XValues.Values(ltyzTable.XValues.Count - 1)
                Z = ltyzTable.Values1.Values(ltyzTable.Values1.Count - 1)

                n += 1
                newTable.XValues.Add(Str(n), Y)
                newTable.Values1.Add(Str(n), Math.Max(Z, newBedLevel))

                'en tot slot de oorspronkelijke tabel vervangen door de nieuwe
                ltyzTable = newTable

                'v1.792 store the information that this groundlayer has been implemented in the profile definition itself
                GroundLayerThicknessImplementedInDefinition = myValue

            End If
        ElseIf ty = enmProfileType.trapezium OrElse ty = enmProfileType.closedcircle OrElse ty = enmProfileType.tabulated OrElse ty = enmProfileType.closedrectangular Then
            'adding a groundlayer to a trapezium, round or tabulated profile or rectangular
            gl += myValue
            gu = 1
        Else
            Me.Setup.Log.AddError("Functionality AddGroundLayer not supported for selected cross section type " & ty & ". Definition ID: " & ID)
        End If

    End Sub

    Friend Function TruncateYZByFixedIncrement(ByVal Increment As Double) As Boolean

        Try
            Dim StartIdx As Integer, LeftBankIdx As Integer, RightBankIdx As Integer, i As Integer

            'start by findint the lowest point
            StartIdx = GetLowestLevelIdx()
            LeftBankIdx = 0
            RightBankIdx = ltyzTable.XValues.Values.Count - 1

            'step left until the next step equals increment
            For i = StartIdx - 1 To 0 Step -1
                If ltyzTable.XValues.Values(i + 1) - ltyzTable.XValues.Values(i) = Increment Then
                    LeftBankIdx = i + 1
                    Exit For
                End If
            Next

            For i = StartIdx + 1 To ltyzTable.XValues.Values.Count - 1
                If ltyzTable.XValues.Values(i) - ltyzTable.XValues.Values(i - 1) = Increment Then
                    RightBankIdx = i - 1
                    Exit For
                End If
            Next

            Dim newTable As New clsSobekTable(Me.Setup)
            newTable.ID = ltyzTable.ID
            For i = LeftBankIdx To RightBankIdx
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            Next

            If newTable.XValues.Count <= 2 Then
                'the previous method failed; probably because the deepest point was actually inside an area with the fixed increment in question.
                'so now we'll revert to another method: stepping from left to right
                'simply look for the first valid section and create a cross section out of that one
                newTable = New clsSobekTable(Me.Setup)
                Dim TableStart As Boolean = False

                For i = 1 To ltyzTable.XValues.Count - 1
                    If ltyzTable.XValues.Values(i) - ltyzTable.XValues.Values(i - 1) <> Increment Then
                        If Not TableStart Then
                            newTable.AddDataPair(2, ltyzTable.XValues.Values(i - 1), ltyzTable.Values1.Values(i - 1))
                            TableStart = True
                        End If
                        newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
                    Else
                        If TableStart Then Exit For 'we reached the end of the cross section
                    End If
                Next
            End If

            ltyzTable = newTable
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Friend Sub TruncateYZatEmbankments(Optional ByVal StartAtGivenDistance As Double = -999, Optional ByVal StartAtCenter As Boolean = False)
        'deze subroutine kort dwarsprofielen van het type YZ in ter hoogte van hun kades
        Dim newTable As New clsSobekTable(Me.Setup)
        Dim i As Integer, fromIdx As Integer = 0, toIdx As Integer = ltyzTable.XValues.Count - 1
        Dim minY As Double, minZ As Double, maxZLeft As Double, maxZRight As Double
        Dim Y As Double, Z As Double, myRatio As Double, maxRatio As Double
        Dim StartIdx As Integer, LeftBankIdx As Integer, RightBankIdx As Integer

        'zoek het startpunt
        If StartAtGivenDistance <> -999 Then
            'we start at a given distance on the profile
            StartIdx = ltyzTable.getIdxFromValue(0, StartAtGivenDistance)
        ElseIf StartAtCenter Then
            'we start at the cross section center
            StartIdx = ltyzTable.getCenterIdx(0)
        Else
            'we start at the lowest point of the entire profile
            StartIdx = GetLowestLevelIdx()
        End If

        minY = ltyzTable.XValues.Values(StartIdx)
        minZ = ltyzTable.Values1.Values(StartIdx)

        GetHighestLevelIdx()
        'zoek het hoogste punt links
        LeftBankIdx = GetHighestLevelLeftIdx(StartIdx)
        maxZLeft = ltyzTable.Values1.Values(LeftBankIdx)

        'zoek het hoogste punt rechts
        RightBankIdx = GetHighestLevelRightIdx(StartIdx)
        maxZRight = ltyzTable.Values1.Values(RightBankIdx)

        'wandel vanuit dit diepste punt terug naar links totdat je een lokale hoogte hebt gevonden (of niet)
        maxRatio = 0
        For i = StartIdx To 1 Step -1
            If ltyzTable.Values1.Values(i - 1) < ltyzTable.Values1.Values(i) Then
                'lokale hoogte gevonden. We beschouwen dit punt als een embankment wanneer de hoogte meer dan 2/3 van de totale profielhoogte bedraagt
                myRatio = (ltyzTable.Values1.Values(i) - minZ) / (maxZLeft - minZ)
                If myRatio > 2 / 3 AndAlso myRatio > maxRatio Then
                    fromIdx = i
                    maxRatio = myRatio
                End If
            End If
        Next

        'wandel nu vanuit het diepste punt naar rechts totdat je een lokale hoogte hebt gevonden (of niet)
        maxRatio = 0
        For i = StartIdx To ltyzTable.XValues.Count - 2
            If ltyzTable.Values1.Values(i + 1) < ltyzTable.Values1.Values(i) Then
                'lokale hoogte gevonden. We beschouwen dit punt als een embankment wanneer de hoogte meer dan 2/3 van de totale profielhoogte bedraagt
                myRatio = (ltyzTable.Values1.Values(i) - minZ) / (maxZRight - minZ)
                If myRatio > 2 / 3 AndAlso myRatio > maxRatio Then
                    toIdx = i
                    maxRatio = myRatio
                End If
            End If
        Next

        'vul de nieuwe tabel met waarden
        For i = fromIdx To toIdx
            Y = ltyzTable.XValues.Values(i)
            Z = ltyzTable.Values1.Values(i)

            'add these items to the table
            newTable.AddDataPair(2, Y, Z)
            If ltyzTable.Values2.Count = ltyzTable.XValues.Count Then newTable.Values2.Add(Str(Y), ltyzTable.Values2.Values(i))
            If ltyzTable.Values3.Count = ltyzTable.XValues.Count Then newTable.Values3.Add(Str(Y), ltyzTable.Values3.Values(i))
        Next

        'vervang de oude tabel door de nieuwe
        ltyzTable.XValues.Clear()
        ltyzTable.Values1.Clear()
        ltyzTable.Values2.Clear()
        ltyzTable.Values3.Clear()
        For i = 0 To newTable.XValues.Count - 1
            Y = newTable.XValues.Values(i)
            ltyzTable.AddDataPair(2, newTable.XValues.Values(i), newTable.Values1.Values(i))
            If newTable.Values2.Count = newTable.XValues.Count Then ltyzTable.Values2.Add(Str(Y), newTable.Values2.Values(i))
            If newTable.Values3.Count = newTable.XValues.Count Then ltyzTable.Values3.Add(Str(Y), newTable.Values3.Values(i))
        Next

    End Sub

    Friend Sub TruncateTabulatedAtEmbankments()
        Me.Setup.Log.AddWarning("Function TruncateTabulatedAtEmbankments not available yet.")
    End Sub

    Friend Function TruncateTabulatedByFixedIncrement(Increment As Double) As Boolean
        Try
            Dim EmbankmentIdx As Integer, i As Integer
            Dim newTable As New clsSobekTable(Me.Setup)
            For i = 1 To ltlwTable.XValues.Count - 1
                If ltlwTable.Values1.Values(i) - ltlwTable.Values1.Values(i - 1) = Increment Then
                    EmbankmentIdx = i
                End If
            Next
            newTable.ID = ltlwTable.ID
            For i = 0 To EmbankmentIdx
                newTable.AddDataPair(2, ltlwTable.XValues.Values(i), ltlwTable.Values1.Values(i))
            Next
            ltlwTable = newTable
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Sub TruncateYZFromDist(ByRef Setup As clsSetup, ByVal fromDist As Double)
        Dim newTable As New clsSobekTable(Me.Setup)
        Dim i As Long

        For i = 1 To ltyzTable.XValues.Count - 1
            If ltyzTable.XValues.Values(i - 1) <= fromDist Then
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i - 1), ltyzTable.Values1.Values(i - 1))
                If ltyzTable.XValues.Values(i) > fromDist Then
                    'interpoleren
                    newTable.AddDataPair(2, fromDist, Setup.GeneralFunctions.Interpolate(ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i), ltyzTable.XValues.Values(i + 1), ltyzTable.Values1.Values(i + 1), fromDist))
                End If
            End If
        Next

        'en de laatste nog even checken
        If ltyzTable.XValues.Values(ltyzTable.XValues.Values.Count - 1) <= fromDist Then
            newTable.AddDataPair(2, ltyzTable.XValues.Values(ltyzTable.XValues.Values.Count - 1), ltyzTable.Values1.Values(ltyzTable.XValues.Values.Count - 1))
        End If

        'vervang de oude tabel door de nieuwe
        newTable.ID = ltyzTable.ID
        ltyzTable = newTable
    End Sub


    Friend Sub TruncateYZtoDist(ByRef Setup As clsSetup, ByVal toDist As Double)
        Dim newTable As New clsSobekTable(Me.Setup)
        Dim i As Long

        For i = 0 To ltyzTable.XValues.Count - 2
            If ltyzTable.XValues.Values(i) <= toDist Then
                If ltyzTable.XValues.Values(i + 1) > toDist Then
                    newTable.AddDataPair(2, toDist, Setup.GeneralFunctions.Interpolate(ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i), ltyzTable.XValues.Values(i + 1), ltyzTable.Values1.Values(i + 1), toDist))
                End If
            ElseIf ltyzTable.XValues.Values(i) > toDist Then
                newTable.AddDataPair(2, ltyzTable.XValues.Values(i), ltyzTable.Values1.Values(i))
            End If
        Next
        newTable.AddDataPair(2, ltyzTable.XValues.Values(ltyzTable.XValues.Values.Count - 1), ltyzTable.Values1.Values(ltyzTable.XValues.Values.Count - 1))

        'vervang de oude tabel door de nieuwe
        newTable.ID = ltyzTable.ID
        ltyzTable = newTable
    End Sub


    Friend Sub TruncateYZ(ByRef Setup As clsSetup, ByVal Width As Double)
        'zoek eers de y-waarde van het diepste punt
        Dim newTable As New clsSobekTable(Me.Setup)
        Dim i As Integer, Y As Double, Z As Double, Y2 As Double, Z2 As Double, newY As Double, newZ As Double
        Dim minY As Double = GetLowestLevel(True)
        Dim LeftBank As Double = minY - Width / 2
        Dim RightBank As Double = minY + Width / 2

        For i = 0 To ltyzTable.XValues.Count - 2
            Y = ltyzTable.XValues.Values(i)
            Z = ltyzTable.Values1.Values(i)
            Y2 = ltyzTable.XValues.Values(i + 1)
            Z2 = ltyzTable.Values1.Values(i + 1)

            If Y = LeftBank And i = 0 Then
                'zichzelf toevoegen
                newTable.AddDataPair(2, Y, Z)
            ElseIf Y > LeftBank And i = 0 Then
                'extra punt toevoegen
                newY = LeftBank
                newZ = Z
                newTable.AddDataPair(2, newY, newZ)
                newTable.AddDataPair(2, Y, Z)
            ElseIf Y < LeftBank And Y2 < LeftBank Then
                'doe niets
            ElseIf Y < LeftBank And Y2 >= LeftBank Then
                'extra punt interpoleren
                newY = LeftBank
                newZ = Setup.GeneralFunctions.Interpolate(Y, Z, Y2, Z2, newY)
                newTable.AddDataPair(2, newY, newZ)
            ElseIf Y >= LeftBank And Y2 >= LeftBank And Y <= RightBank And Y2 < RightBank Then
                newTable.AddDataPair(2, Y, Z)
            ElseIf Y <= RightBank And Y2 > RightBank Then
                newTable.AddDataPair(2, Y, Z)
                'extra punt interpoleren
                newY = RightBank
                newZ = Setup.GeneralFunctions.Interpolate(Y, Z, Y2, Z2, newY)
                newTable.AddDataPair(2, newY, newZ)
            ElseIf Y > RightBank And Y2 > RightBank Then
                Exit For
            End If
        Next i

        Y = ltyzTable.XValues.Values(ltyzTable.XValues.Count - 1)
        Z = ltyzTable.Values1.Values(ltyzTable.XValues.Count - 1)
        If Y < RightBank Then
            'zichzelf toevoegen
            newTable.AddDataPair(2, Y, Z)

            'extra punt toevoegen
            newY = RightBank
            newZ = Z
            newTable.AddDataPair(2, newY, newZ)
        ElseIf Y = RightBank Then
            'alleen zichzelf toevoegen
            newTable.AddDataPair(2, Y, Z)
        End If

        'vervang de oude tabel door de nieuwe
        ltyzTable = newTable
    End Sub

    Public Function ShiftYZLowestToCenter() As Boolean
        'yz-tabellen:
        'xvalues = distance
        'values1 = z-value
        'values2 = x-coordinate
        'values3 = y-coordinate

        'find the y-value of the lowest point
        Dim LowestIdx As Integer = ltyzTable.GetMinValueIdx(1)
        Dim LowestZ As Double = ltyzTable.Values1.Values(LowestIdx)
        Dim LowestY As Double = ltyzTable.XValues.Values(LowestIdx)
        Dim nLowest As Integer = 1

        'check if either of the two surrounding values have equal z-value. 
        'if so, the y-value of the lowest point is determined as the center
        If LowestIdx > 0 AndAlso ltyzTable.Values1.Values(LowestIdx - 1) = LowestZ Then
            LowestY += ltyzTable.XValues.Values(LowestIdx - 1)
            nLowest += 1
        End If
        If LowestIdx < ltyzTable.Values1.Values.Count - 1 AndAlso ltyzTable.Values1.Values(LowestIdx + 1) = LowestZ Then
            LowestY += ltyzTable.XValues.Values(LowestIdx + 1)
            nLowest += 1
        End If

        'shift the y-values
        LowestY = LowestY / nLowest
        ltyzTable.ShiftXValues(-LowestY)

    End Function

End Class
