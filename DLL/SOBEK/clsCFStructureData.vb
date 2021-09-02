Option Explicit On
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO

Public Class clsCFStructureData
    Friend StructDatRecords As clsStructDatRecords
    Friend StructDefRecords As clsStructDefRecords
    Friend ControlDefRecords As clsControlDefRecords
    Friend ValveTabRecords As clsValveTabVLVERecords
    Private setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase

        ' Init classes:
        StructDatRecords = New clsStructDatRecords(Me.setup, Me.SbkCase)
        StructDefRecords = New clsStructDefRecords(Me.setup, Me.SbkCase)
        ControlDefRecords = New clsControlDefRecords(Me.setup, Me.SbkCase)
        ValveTabRecords = New clsValveTabVLVERecords(Me.setup, Me.SbkCase)

    End Sub

    Public Function ExportWeirDataCSV(Path As String, ByRef ControlTableWriter As StreamWriter, Optional ByVal Delimiter As String = ";") As Boolean
        Try
            Dim myDef As clsStructDefRecord = Nothing
            Dim myDat As clsStructDatRecord = Nothing
            Dim myContr As clsControlDefRecord = Nothing

            Dim StrucStr As String = ""
            Using WeirWriter As New StreamWriter(Path)

                WeirWriter.WriteLine("ObjectID;CrestWidth;CrestElevation;DischargeCoef;ContractionCoef;ControllerType;MeasurementStation;ControlledParameter;ObservedParameter;ControlFrequency;InitialValue;MinimumValue;MaximumValue;ValueBelowDeadband;ValueAboveDeadband;DeadbandSize;SetpointType;ConstantSetpointValue;SetpointTableID;InterpolationType;AttributeVelocity;K_Proportional;K_Integral;K_Differential")

                For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                    If myReach.InUse Then
                        For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                            If myObj.nt = enmNodetype.NodeCFWeir AndAlso myObj.InUse Then
                                If Not GetStructureRecords(myObj.ID, myDat, myDef, myContr) Then
                                    Me.setup.Log.AddError("Error retrieving structure records for Object " & myObj.ID)
                                    Continue For
                                Else
                                    StrucStr = myObj.ID                              'ID
                                    StrucStr &= Delimiter & myDef.cw                 'crest width
                                    StrucStr &= Delimiter & myDef.cl                 'crest level
                                    StrucStr &= Delimiter & myDef.ce                 'discharge coef
                                    StrucStr &= Delimiter & myDef.sc                 'lateral contraction coef

                                    'also write the controller, if applicable
                                    If myDat.ca = 1 Then
                                        AddControlDataToCSVString(StrucStr, Delimiter, myContr, ControlTableWriter)
                                    End If

                                    WeirWriter.WriteLine(StrucStr)
                                End If
                            End If
                        Next
                    End If
                Next
            End Using
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ExportOrificeDataCSV(Path As String, ByRef ControlTableWriter As StreamWriter, Optional ByVal Delimiter As String = ";") As Boolean
        Try
            Dim myDef As clsStructDefRecord = Nothing
            Dim myDat As clsStructDatRecord = Nothing
            Dim myContr As clsControlDefRecord = Nothing

            Dim StrucStr As String = ""
            Using OrificeWriter As New StreamWriter(Path)

                OrificeWriter.WriteLine("ObjectID;CrestWidth;CrestElevation;GateHeight;DischargeCoef;ContractionCoef;FlowDirection;ControllerType;MeasurementStation;ControlledParameter;ObservedParameter;ControlFrequency;InitialValue;MinimumValue;MaximumValue;ValueBelowDeadband;ValueAboveDeadband;DeadbandSize;SetpointType;ConstantSetpointValue;SetpointTableID;InterpolationType;AttributeVelocity;K_Proportional;K_Integral;K_Differential")

                For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                    If myReach.InUse Then
                        For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                            If myObj.nt = enmNodetype.NodeCFOrifice AndAlso myObj.InUse Then
                                If Not GetStructureRecords(myObj.ID, myDat, myDef, myContr) Then
                                    Me.setup.Log.AddError("Error retrieving structure records for Object " & myObj.ID)
                                    Continue For
                                Else
                                    StrucStr = myObj.ID                              'ID
                                    StrucStr &= Delimiter & myDef.cw                 'crest width
                                    StrucStr &= Delimiter & myDef.cl                 'crest level
                                    StrucStr &= Delimiter & myDef.gh                 'gate height
                                    StrucStr &= Delimiter & myDef.Mu                 'discharge coef
                                    StrucStr &= Delimiter & myDef.sc                 'lateral contraction coef
                                    StrucStr &= Delimiter & myDef.rt.ToString        'possible flow direction

                                    'also write the controller, if applicable
                                    If myDat.ca = 1 Then
                                        AddControlDataToCSVString(StrucStr, Delimiter, myContr, ControlTableWriter)
                                    End If

                                    OrificeWriter.WriteLine(StrucStr)
                                End If
                            End If
                        Next
                    End If
                Next
            End Using
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ExportCulvertDataCSV(Path As String, ByRef ControlTableWriter As StreamWriter, Optional ByVal Delimiter As String = ";") As Boolean
        Try
            Dim myDef As clsStructDefRecord = Nothing
            Dim myDat As clsStructDatRecord = Nothing
            Dim myProf As clsProfileDefRecord = Nothing
            Dim myContr As clsControlDefRecord = Nothing

            Dim StrucStr As String = ""
            Using CulvertWriter As New StreamWriter(Path)

                CulvertWriter.WriteLine("ObjectID;InvertUp;InvertDown;Length;InletCoef;OutletCoef;Shape;MaxWidth;MaxHeight;ControllerType;MeasurementStation;ControlledParameter;ObservedParameter;ControlFrequency;InitialValue;MinimumValue;MaximumValue;ValueBelowDeadband;ValueAboveDeadband;DeadbandSize;SetpointType;ConstantSetpointValue;SetpointTableID;InterpolationType;AttributeVelocity;K_Proportional;K_Integral;K_Differential")

                For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                    If myReach.InUse Then
                        For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                            If myObj.nt = enmNodetype.NodeCFCulvert AndAlso myObj.InUse Then
                                If Not GetStructureRecords(myObj.ID, myDat, myDef, myContr) Then
                                    Me.setup.Log.AddError("Error retrieving structure records for Object " & myObj.ID)
                                    Continue For
                                Else
                                    myProf = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(myDef.si)
                                    If myProf Is Nothing Then Continue For
                                    StrucStr = myObj.ID                              'ID
                                    StrucStr &= Delimiter & myDef.ll                 'invert up
                                    StrucStr &= Delimiter & myDef.rl                 'invert down
                                    StrucStr &= Delimiter & myDef.dl                 'length
                                    StrucStr &= Delimiter & myDef.li                 'inlet coef
                                    StrucStr &= Delimiter & myDef.lo                 'outlet coef
                                    StrucStr &= Delimiter & myProf.ty.ToString       'shape
                                    Select Case myProf.ty
                                        Case Is = GeneralFunctions.enmProfileType.closedcircle
                                            StrucStr &= Delimiter & myProf.rd
                                            StrucStr &= Delimiter & myProf.rd
                                        Case Is = GeneralFunctions.enmProfileType.closedrectangular
                                            StrucStr &= Delimiter & myProf.ltlwTable.Values1.Values(0) 'width
                                            StrucStr &= Delimiter & myProf.ltlwTable.XValues.Values(1) 'height
                                        Case Is = GeneralFunctions.enmProfileType.tabulated
                                            StrucStr &= Delimiter & myProf.ltlwTable.getMaxValue(1)
                                            StrucStr &= Delimiter & myProf.ltlwTable.getMaxValue(0)
                                        Case Else
                                            StrucStr &= Delimiter & "UNSUPPORTED" 'width
                                            StrucStr &= Delimiter & "UNSUPPORTED" 'height
                                    End Select

                                    'also write the controller, if applicable
                                    If myDat.ca = 1 Then
                                        AddControlDataToCSVString(StrucStr, Delimiter, myContr, ControlTableWriter)
                                    End If

                                    'write the csv-string to file
                                    CulvertWriter.WriteLine(StrucStr)
                                End If
                            End If
                        Next
                    End If
                Next
            End Using
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Sub AddControlDataToCSVString(ByRef strucStr As String, Delimiter As String, ByRef myContr As clsControlDefRecord, ByRef ControlTableWriter As StreamWriter)
        Select Case myContr.ct
            Case Is = 0
                strucStr &= Delimiter & myContr.ct.ToString      'controller type
                strucStr &= Delimiter & ""                       'measurement station
                strucStr &= Delimiter & myContr.ca.ToString      'controlled parameter
                strucStr &= Delimiter & ""                       'observed parameter
                strucStr &= Delimiter & myContr.cf               'control frequency
                strucStr &= Delimiter & ""                       'initial crest level
                strucStr &= Delimiter & ""                       'minimum crest level
                strucStr &= Delimiter & ""                       'maximum crest level
                strucStr &= Delimiter & ""                       'value when below dead band
                strucStr &= Delimiter & ""                       'value when above dead band
                strucStr &= Delimiter & ""                       'deadband size
                strucStr &= Delimiter & ""                       'setpoint type
                strucStr &= Delimiter & ""                       'constant setpoint value
                strucStr &= Delimiter & myContr.TimeTable.ID     'setpoint table id
                strucStr &= Delimiter & myContr.bl.ToString      'interpolation type
                strucStr &= Delimiter & ""                       'attribute velocity
                strucStr &= Delimiter & ""                       'K proportional
                strucStr &= Delimiter & ""                       'K integral
                strucStr &= Delimiter & ""                       'K differential
                For i = 0 To myContr.TimeTable.Dates.Count - 1
                    ControlTableWriter.WriteLine(myContr.TimeTable.ID & Delimiter & Format(myContr.TimeTable.Dates.Values(i), "yyyy/MM/dd hh:mm:ss") & Delimiter & Delimiter & myContr.TimeTable.Values1.Values(i))
                Next
            Case Is = 1
                strucStr &= Delimiter & myContr.ct.ToString      'controller type
                strucStr &= Delimiter & myContr.ml               'measurement station
                strucStr &= Delimiter & myContr.ca.ToString      'controlled parameter
                strucStr &= Delimiter & myContr.cp.ToString      'observed parameter
                strucStr &= Delimiter & myContr.cf               'control frequency
                strucStr &= Delimiter & ""                       'initial crest level
                strucStr &= Delimiter & ""                       'minimum crest level
                strucStr &= Delimiter & ""                       'maximum crest level
                strucStr &= Delimiter & ""                       'value when below dead band
                strucStr &= Delimiter & ""                       'value when above dead band
                strucStr &= Delimiter & ""                       'deadband size
                strucStr &= Delimiter & ""                       'setpoint type
                strucStr &= Delimiter & ""                       'constant setpoint value
                strucStr &= Delimiter & myContr.ControlTable.ID  'table id
                strucStr &= Delimiter & myContr.bl.ToString      'interpolation type
                strucStr &= Delimiter & ""                       'attribute velocity
                strucStr &= Delimiter & ""                       'K proportional
                strucStr &= Delimiter & ""                       'K integral
                strucStr &= Delimiter & ""                       'K differential
                For i = 0 To myContr.ControlTable.XValues.Count - 1
                    ControlTableWriter.WriteLine(myContr.ControlTable.ID & Delimiter & Delimiter & myContr.ControlTable.XValues.Values(i) & Delimiter & myContr.ControlTable.Values1.Values(i))
                Next
            Case Is = 2
                strucStr &= Delimiter & myContr.ct.ToString      'controller type
                strucStr &= Delimiter & myContr.ml               'measurement station
                strucStr &= Delimiter & myContr.ca.ToString      'controlled parameter
                strucStr &= Delimiter & myContr.cp.ToString      'observed parameter
                strucStr &= Delimiter & myContr.cf               'control frequency
                strucStr &= Delimiter & ""                       'initial crest level
                strucStr &= Delimiter & ""                       'minimum crest level
                strucStr &= Delimiter & ""                       'maximum crest level
                strucStr &= Delimiter & myContr.ui               'value when below dead band
                strucStr &= Delimiter & myContr.ua               'value when above dead band
                strucStr &= Delimiter & myContr.d_               'deadband size
                strucStr &= Delimiter & myContr.sptc.ToString      'interval type
                If myContr.sptc = enmSetpointType.FIXED Then
                    strucStr &= Delimiter & myContr.SetPointValue   'constant setpoint value
                    strucStr &= Delimiter & ""                      'no table id
                Else
                    strucStr &= Delimiter & ""                      'no constant setpoint
                    strucStr &= Delimiter & myContr.TimeTable.ID 'table id
                    For i = 0 To myContr.TimeTable.Dates.Count - 1
                        ControlTableWriter.WriteLine(myContr.TimeTable.ID & Delimiter & Format(myContr.TimeTable.Dates.Values(i), "yyyy/MM/dd hh:mm:ss") & Delimiter & Delimiter & myContr.TimeTable.Values1.Values(i))
                    Next
                End If
                strucStr &= Delimiter & myContr.bl.ToString      'interpolation type
                strucStr &= Delimiter & myContr.cv               'attribute velocity
                strucStr &= Delimiter & ""                       'K proportional
                strucStr &= Delimiter & ""                       'K integral
                strucStr &= Delimiter & ""                       'K differential
            Case Is = 3
                strucStr &= Delimiter & myContr.ct.ToString      'controller type
                strucStr &= Delimiter & myContr.ml               'measurement station
                strucStr &= Delimiter & myContr.ca.ToString      'controlled parameter
                strucStr &= Delimiter & myContr.cp.ToString      'observed parameter
                strucStr &= Delimiter & myContr.cf               'control frequency
                strucStr &= Delimiter & myContr.u0               'initial crest level
                strucStr &= Delimiter & myContr.ui               'minimum crest level
                strucStr &= Delimiter & myContr.ua               'maximum crest level
                strucStr &= Delimiter & ""                       'value when below dead band
                strucStr &= Delimiter & ""                       'value when above dead band
                strucStr &= Delimiter & ""                       'deadband size
                strucStr &= Delimiter & myContr.sptc.ToString    'setpoint type
                If myContr.sptc = enmSetpointType.FIXED Then
                    strucStr &= Delimiter & myContr.SetPointValue   'constant setpoint value
                    strucStr &= Delimiter & ""                      'no table id
                Else
                    strucStr &= Delimiter & ""                      'no constant setpoint
                    strucStr &= Delimiter & myContr.ControlTable.ID 'table id
                    For i = 0 To myContr.TimeTable.Dates.Count - 1
                        ControlTableWriter.WriteLine(myContr.TimeTable.ID & Delimiter & Format(myContr.TimeTable.Dates.Values(i), "yyyy/MM/dd hh:mm:ss") & Delimiter & Delimiter & myContr.TimeTable.Values1.Values(i))
                    Next
                End If
                strucStr &= Delimiter & myContr.bl.ToString      'interpolation type
                strucStr &= Delimiter & myContr.cv               'attribute velocity
                strucStr &= Delimiter & myContr.pf               'K proportional
                strucStr &= Delimiter & myContr.if_              'K integral
                strucStr &= Delimiter & myContr.df               'K differential
        End Select
    End Sub

    Public Function ExportPumpDataCSV(Path As String, ByRef ControlTableWriter As StreamWriter, Optional ByVal Delimiter As String = ";") As Boolean
        Try
            Dim myDef As clsStructDefRecord = Nothing
            Dim myDat As clsStructDatRecord = Nothing
            Dim myContr As clsControlDefRecord = Nothing

            Dim StrucStr As String = ""
            Using PumpWriter As New StreamWriter(Path)

                PumpWriter.WriteLine("ObjectID;ControlDirection;ReductionFactor;Capacity;OnLevelSuctionSide;OffLevelSuctionSide;OnLevelPressureSide;OffLevelPressureSide")

                For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                    If myReach.InUse Then
                        For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                            If myObj.nt = enmNodetype.NodeCFPump AndAlso myObj.InUse Then
                                If Not GetStructureRecords(myObj.ID, myDat, myDef, myContr) Then
                                    Me.setup.Log.AddError("Error retrieving structure records for Object " & myObj.ID)
                                    Continue For
                                Else
                                    For i = 0 To myDef.ctltTable.XValues.Count - 1
                                        StrucStr = myObj.ID                              'ID
                                        Select Case myDef.dn                             'control direction
                                            Case Is = 0
                                                StrucStr &= Delimiter & "NONE"
                                            Case Is = 1
                                                StrucStr &= Delimiter & "SUCTION SIDE"
                                            Case Is = 2
                                                StrucStr &= Delimiter & "PRESSURE SIDE"
                                            Case Is = 3
                                                StrucStr &= Delimiter & "BOTH"
                                        End Select

                                        Select Case myDef.rtcr1                         'reduction factor
                                            Case Is = 0
                                                StrucStr &= Delimiter & myDef.rtcr2     'constant value
                                            Case Is = 1
                                                StrucStr &= Delimiter & "UNSUPPORTED"      'exporting the table is not yet supported
                                        End Select

                                        Select Case myDef.ctlt                          'operation table
                                            Case Is = 1
                                                StrucStr &= Delimiter & myDef.ctltTable.XValues.Values(i)     'capacity
                                                StrucStr &= Delimiter & myDef.ctltTable.Values1.Values(i)     'suction side on level
                                                StrucStr &= Delimiter & myDef.ctltTable.Values2.Values(i)     'suction side off level
                                                StrucStr &= Delimiter & myDef.ctltTable.Values3.Values(i)     'pressure side on level
                                                StrucStr &= Delimiter & myDef.ctltTable.Values4.Values(i)     'pressure side off level
                                        End Select
                                        PumpWriter.WriteLine(StrucStr)
                                    Next
                                End If
                            End If
                        Next
                    End If
                Next
            End Using
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function ExportCSV(ExportDir As String, Optional ByVal Delimiter As String = ";") As Boolean
        Try
            Using ControlTableWriter As New StreamWriter(ExportDir & "\ControlTables.csv")
                ControlTableWriter.WriteLine("TableID;Date;XValue;YValue")
                If Not ExportWeirDataCSV(ExportDir & "\weirdata.csv", ControlTableWriter) Then Throw New Exception("Error exporting weir attribute data.")
                If Not ExportOrificeDataCSV(ExportDir & "\orificedata.csv", ControlTableWriter) Then Throw New Exception("Error exporting orifice attribute data.")
                If Not ExportPumpDataCSV(ExportDir & "\pumpdata.csv", ControlTableWriter) Then Throw New Exception("Error exporting pump attribute data.")
                If Not ExportCulvertDataCSV(ExportDir & "\culvertdata.csv", ControlTableWriter) Then Throw New Exception("Error exporting culvert attribute data.")
            End Using


        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Function getStructureLength(ByVal myObj As clsSbkReachObject, ByVal minLength As Double) As Double
        Dim myDat As clsStructDatRecord = Nothing, myDef As clsStructDefRecord = Nothing, myContr As clsControlDefRecord = Nothing
        Try
            Select Case myObj.nt
                Case Is = enmNodetype.NodeCFUniWeir, enmNodetype.NodeCFWeir, enmNodetype.NodeCFPump, enmNodetype.NodeCFOrifice, enmNodetype.NodeCFExtraResistance
                    Return minLength
                Case Is = enmNodetype.NodeCFCulvert, enmNodetype.NodeCFBridge
                    If Not GetStructureRecords(myObj.ID, myDat, myDef, myContr) Then Throw New Exception("Could not get structure data for " & myObj.ID)
                    Return Math.Max(minLength, myDef.dl)
            End Select

            Throw New Exception("Error: node type not supported " & myObj.nt.ToString)
        Catch ex As Exception
            Return minLength
        End Try

    End Function

    Friend Function GetTargetLevels(ByVal myNode As clsSbkReachObject, ByRef ZP As Double, ByRef WP As Double) As Boolean
        Try
            Select Case myNode.nt
                Case Is = enmNodetype.NodeCFPump
                    GetTargetLevelsFromPumpData(myNode.ID, ZP, WP)
                Case Is = enmNodetype.NodeCFWeir
                    GetTargetLevelsFromWeirData(myNode.ID, ZP, WP)
                Case Else
                    Throw New Exception("Not possible to retrieve target levels from structure " & myNode.ID & " of type " & myNode.nt.ToString)
            End Select

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error retrieving target levels for structure " & myNode.ID)
            Return False
        End Try
    End Function

    Public Function GetStructureRecords(ByVal ID As String, ByRef StrucDat As clsStructDatRecord, ByRef StrucDef As clsStructDefRecord, ByRef ContrDef As clsControlDefRecord) As Boolean
        Try
            If StructDatRecords.Records.ContainsKey(ID.Trim.ToUpper) Then
                StrucDat = StructDatRecords.Records.Item(ID.Trim.ToUpper)
                If StructDefRecords.Records.ContainsKey(StrucDat.dd.Trim.ToUpper) Then
                    StrucDef = StructDefRecords.Records.Item(StrucDat.dd.Trim.ToUpper)
                End If
                If StrucDat.ca = 1 AndAlso ControlDefRecords.Records.ContainsKey(StrucDat.cj.Trim.ToUpper) Then
                    ContrDef = ControlDefRecords.Records.Item(StrucDat.cj.Trim.ToUpper)
                End If
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetTargetLevelsFromPumpData(ID As String, ByRef ZP As Double, ByRef WP As Double) As Boolean
        'siebe bosch: 22-5-2017
        'this function looks up the target levels for a pumping station and returns them as if they were summer and winter target level
        'note: no distinction between summer and winter target level yet and no lookup of controller tables
        'for now we just take the average of the switch-on and switch-off level
        Dim StrucDat As clsStructDatRecord
        Dim StrucDef As clsStructDefRecord
        Dim ContrDef As clsControlDefRecord

        Try
            If StructDatRecords.Records.ContainsKey(ID.Trim.ToUpper) Then
                StrucDat = StructDatRecords.Records.Item(ID.Trim.ToUpper)
                If StrucDat.ca = 1 AndAlso ControlDefRecords.Records.ContainsKey(StrucDat.cj.Trim.ToUpper) Then
                    ContrDef = ControlDefRecords.Records.Item(StrucDat.cj.Trim.ToUpper)
                    ZP = ContrDef.GetFirstSetpointValue
                    WP = ZP
                ElseIf StructDefRecords.Records.ContainsKey(StrucDat.dd.Trim.ToUpper) Then
                    StrucDef = StructDefRecords.Records.Item(StrucDat.dd.Trim.ToUpper)
                    ZP = (StrucDef.ctltTable.Values1.Values(0) + StrucDef.ctltTable.Values2.Values(0)) / 2
                    WP = ZP
                    Return True
                Else
                    ZP = -999
                    WP = -999
                End If
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function GetTargetLevelsFromPumpData.")
            Return False
        End Try
    End Function

    Public Function GetTargetLevelsFromWeirData(ID As String, ByRef ZP As Double, ByRef WP As Double) As Boolean
        'siebe bosch: 22-5-2017
        'this function looks up the target levels for a weir and returns them as if they were summer and winter target level
        'note: no distinction between summer and winter target level yet and no lookup of controller tables
        'for now we just take the crest level
        Dim StrucDat As clsStructDatRecord
        Dim StrucDef As clsStructDefRecord
        Dim ContrDef As clsControlDefRecord

        Try
            If StructDatRecords.Records.ContainsKey(ID.Trim.ToUpper) Then
                StrucDat = StructDatRecords.Records.Item(ID.Trim.ToUpper)
                If StrucDat.ca = 1 AndAlso ControlDefRecords.Records.ContainsKey(StrucDat.cj.Trim.ToUpper) Then
                    ContrDef = ControlDefRecords.Records.Item(StrucDat.cj.Trim.ToUpper)
                    ZP = ContrDef.GetFirstSetpointValue
                    WP = ZP
                ElseIf StructDefRecords.Records.ContainsKey(StrucDat.dd.Trim.ToUpper) Then
                    StrucDef = StructDefRecords.Records.Item(StrucDat.dd.Trim.ToUpper)
                    ZP = StrucDef.cl
                    WP = StrucDef.cl
                    Return True
                End If
            Else
                ZP = -999
                WP = -999
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function GetTargetLevelsFromPumpData.")
            Return False
        End Try
    End Function

    Friend Function BuildWeirData(ByVal ID As String, ByRef meas As clsSbkReachObject, WP As Double, ZP As Double, CrestWidth As Double, MinCrest As Double, MaxCrest As Double, WinterCrest As Double, SummerCrest As Double, CrestChangeSpeed As Double, CrestCorrectionSummer As Double, CrestCorrectionWinter As Double, CrestCorrectionNonControlledWeirs As Double, DischargeCoef As Double, ContractionCoef As Double, PIDSetting As clsPIDSetting, TableStartYear As Integer, TableEndYear As Integer, SeasonTransitions As clsSeasonTransitions, ControllerCategory As GeneralFunctions.enmControllerCategory, SetInUse As Boolean) As Boolean
        Try
            Dim strucDat As New clsStructDatRecord(Me.setup)
            Dim strucDef As New clsStructDefRecord(Me.setup)
            Dim contrDef As clsControlDefRecord = Nothing

            'decide the regular crest level (the cl token). We take the highest value of summer and winter target level.
            Dim CrestLevel As Double
            If Not Double.IsNaN(SummerCrest) Then
                'if a crest level is given, this value is leading; regardless of other values such as minimum and maximum
                CrestLevel = SummerCrest
            Else
                CrestLevel = Math.Max(ZP, WP)
                Select Case ControllerCategory
                    Case enmControllerCategory.NONE
                        CrestLevel += CrestCorrectionNonControlledWeirs
                    Case enmControllerCategory.TIME
                        CrestLevel += CrestCorrectionSummer
                End Select
                'v2.000 forcing our crest level inside the min/max range
                If Not Double.IsNaN(MinCrest) Then CrestLevel = Math.Max(MinCrest, CrestLevel)
                If Not Double.IsNaN(MaxCrest) Then CrestLevel = Math.Min(MaxCrest, CrestLevel)
            End If

            strucDat.ID = ID
            strucDat.nm = ID
            strucDat.dd = ID
            strucDat.InUse = SetInUse

            'create the struct.def record
            strucDef = New clsStructDefRecord(Me.setup)
            strucDef.ID = ID
            strucDef.nm = ID
            strucDef.InUse = True
            strucDef.ty = 6  'weir?
            strucDef.cw = CrestWidth
            strucDef.ce = DischargeCoef
            strucDef.sc = ContractionCoef

            If ControllerCategory = GeneralFunctions.enmControllerCategory.NONE Then
                strucDat.ca = 0
                strucDat.cj = ""
                strucDef.cl = CrestLevel

            ElseIf ControllerCategory = GeneralFunctions.enmControllerCategory.TIME Then
                strucDat.ca = 1
                strucDat.cj = strucDat.ID
                strucDef.cl = CrestLevel

                'make a time controller that simply follows target level
                contrDef = New clsControlDefRecord(Me.setup)
                Dim SummerCrestLevel As Double = SummerCrest
                If Double.IsNaN(SummerCrest) Then SummerCrestLevel = ZP + CrestCorrectionSummer
                Dim WinterCrestLevel As Double = WinterCrest
                If Double.IsNaN(WinterCrest) Then WinterCrestLevel = WP + CrestCorrectionWinter
                contrDef = setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.BuildControlDefRecordForTargetLevelFollowWeir(ID, SummerCrestLevel, WinterCrestLevel, SeasonTransitions)

            ElseIf ControllerCategory = GeneralFunctions.enmControllerCategory.PID Then
                strucDat.ca = 1
                strucDat.cj = strucDat.ID
                strucDef.cl = CrestLevel

                'make a PID controller that attempts to obey target level
                contrDef = New clsControlDefRecord(Me.setup)
                contrDef = setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.BuildControlDefRecordForPIDControlledWeir(ID, meas.ID, MinCrest, MaxCrest, ZP, WP, PIDSetting, TableStartYear, TableEndYear, SeasonTransitions)
            ElseIf ControllerCategory = enmControllerCategory.INTERVAL Then
                strucDat.ca = 1
                strucDat.cj = strucDat.ID
                strucDef.cl = CrestLevel

                'make an INTERVAL controller that attempts to obey target level
                contrDef = New clsControlDefRecord(Me.setup)
                contrDef = setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.buildControlDefRecordForIntervalControlledOutletWeir(ID, meas.ID, MinCrest, MaxCrest, CrestChangeSpeed, ZP, WP, 0.1, TableStartYear, TableEndYear, SeasonTransitions)
            Else
                Me.setup.Log.AddError("Error: controller type for weir is not (yet) supported! " & ControllerCategory.ToString)
            End If

            If Not setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.Records.ContainsKey(ID.Trim.ToUpper) Then
                setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.Records.Add(strucDat.ID.Trim.ToUpper, strucDat)
            Else
                Me.setup.Log.AddError("Struct.dat record for weir " & strucDat.ID & " was already present in the collection and could not be added a second time. Please check for double instances of this structure ID.")
            End If
            If Not setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.Records.ContainsKey(strucDef.ID.Trim.ToUpper) Then
                setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.Records.Add(strucDef.ID.Trim.ToUpper, strucDef)
            Else
                Me.setup.Log.AddError("Struct.def record " & strucDef.ID & " was already present in the collection and could not be added a second time. Please check for double instances of this structure definition ID.")
            End If

            If contrDef IsNot Nothing Then setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.ControlDefRecords.Records.Add(contrDef.ID.Trim.ToUpper, contrDef)

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildWeirData.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Function BuildFixedWeirData(ByVal ID As String, CrestWidth As Double, CrestLevel As Double, DischargeCoef As Double, ContractionCoef As Double, SetInUse As Boolean) As Boolean
        Try
            Dim strucDat As New clsStructDatRecord(Me.setup)
            Dim strucDef As New clsStructDefRecord(Me.setup)

            strucDat.ID = ID
            strucDat.nm = ID
            strucDat.dd = ID
            strucDat.ca = 0
            strucDat.cj = ""
            strucDat.InUse = SetInUse

            'create the struct.def record
            strucDef = New clsStructDefRecord(Me.setup)
            strucDef.ID = ID
            strucDef.nm = ID
            strucDef.InUse = True
            strucDef.ty = 6  'weir?
            strucDef.cw = CrestWidth
            strucDef.ce = DischargeCoef
            strucDef.sc = ContractionCoef
            strucDef.cl = CrestLevel

            If Not setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.Records.ContainsKey(ID.Trim.ToUpper) Then
                setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.Records.Add(strucDat.ID.Trim.ToUpper, strucDat)
            Else
                Me.setup.Log.AddError("Struct.dat record for weir " & strucDat.ID & " was already present in the collection and could not be added a second time. Please check for double instances of this structure ID.")
            End If
            If Not setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.Records.ContainsKey(strucDef.ID.Trim.ToUpper) Then
                setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.Records.Add(strucDef.ID.Trim.ToUpper, strucDef)
            Else
                Me.setup.Log.AddError("Struct.def record " & strucDef.ID & " was already present in the collection and could not be added a second time. Please check for double instances of this structure definition ID.")
            End If

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildFixedWeirData")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Function BuildOrificeData(ByVal ID As String, CrestLevel As Double, CrestWidth As Double, GateHeight As Double, FlowDirections As GeneralFunctions.enmFlowDirection, SetInUse As Boolean, Optional ByVal MaxVelocity As Double = 0) As Boolean
        Try
            'this function builds the attribute data for a 'clean' orifices which has no controllers whatsoever
            Dim strucDat As New clsStructDatRecord(Me.setup)
            Dim strucDef As New clsStructDefRecord(Me.setup)

            'create the struct.dat record
            strucDat = New clsStructDatRecord(Me.setup)
            strucDat.ID = ID
            strucDat.nm = ID
            strucDat.dd = ID
            strucDat.InUse = True

            'create the struct.def record
            strucDef = New clsStructDefRecord(Me.setup)
            strucDef.ID = ID
            strucDef.nm = ID
            strucDef.InUse = True
            strucDef.ty = 7 'orifice
            strucDef.cl = CrestLevel
            strucDef.cw = CrestWidth
            strucDef.gh = CrestLevel + GateHeight
            strucDef.Mu = 0.63
            strucDef.sc = 1
            strucDef.rt = FlowDirections
            strucDef.mp = 0
            strucDef.mpUsed = False
            strucDef.mn = 0
            strucDef.mnUsed = False

            'flow limiter
            If MaxVelocity > 0 Then
                strucDef.mpUsed = 1
                strucDef.mp = MaxVelocity
                strucDef.mnUsed = 1
                strucDef.mn = MaxVelocity
            End If

            setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.Records.Add(strucDat.ID.Trim.ToUpper, strucDat)
            setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.Records.Add(strucDef.ID.Trim.ToUpper, strucDef)

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildOrificeData when processing orifice with ID: " & ID)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Friend Function BuildOrificeDataAsEmergencyStop(ByVal ID As String, ByRef measLoc As clsSbkReachObject, EmergencyStopLevel As Double, CrestWidth As Double, CrestLevel As Double, MaxGateHeight As Double, SetInUse As Boolean) As Boolean
        Try
            Dim strucDat As New clsStructDatRecord(Me.setup)
            Dim strucDef As New clsStructDefRecord(Me.setup)
            Dim contrDef As New clsControlDefRecord(Me.setup)

            'create the struct.dat record
            strucDat = New clsStructDatRecord(Me.setup)
            strucDat.ID = ID
            strucDat.nm = ID
            strucDat.dd = ID
            strucDat.InUse = True

            'create the struct.def record
            strucDef = New clsStructDefRecord(Me.setup)
            strucDef.ID = ID
            strucDef.nm = ID
            strucDef.InUse = True
            strucDef.ty = 7 'orifice
            strucDef.cl = CrestLevel
            strucDef.cw = CrestWidth
            strucDef.gh = CrestLevel + MaxGateHeight
            strucDef.Mu = 0.63
            strucDef.sc = 1
            strucDef.rt = enmFlowDirection.BOTH
            strucDef.mp = 0
            strucDef.mpUsed = False
            strucDef.mn = 0
            strucDef.mnUsed = False

            setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.Records.Add(strucDat.ID.Trim.ToUpper, strucDat)
            setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.Records.Add(strucDef.ID.Trim.ToUpper, strucDef)

            'finally create the control definition (if applicable)
            If measLoc Is Nothing Then Throw New Exception("Error: measurement location was not specified, but emergency stop controller was requested for orifice " & ID & ". Controller could not be created.")
            contrDef = setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.BuildControlDefRecordForEmergencyStopDownstream(strucDat.ID, measLoc.ID, EmergencyStopLevel, MaxGateHeight, strucDef)
            strucDat.ca = 1
            strucDat.cj = contrDef.ID
            strucDef.ov = 0
            ControlDefRecords.Records.Add(contrDef.ID.Trim.ToUpper, contrDef)

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildOrificeAsEmergencyStopData.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Function BuildOrificeAsInletData(ByVal ID As String, ByRef measLoc As clsSbkReachObject, Targetlevels As clsTargetLevels, CrestWidth As Double, CrestLevel As Double, MaxGateHeight As Double, InletOnOff As Tuple(Of Double, Double), TableStartYear As Integer, TableEndYear As Integer, SeasonTransitions As clsSeasonTransitions, FlowDirections As GeneralFunctions.enmFlowDirection, SetInUse As Boolean) As Boolean
        Try
            Dim strucDat As New clsStructDatRecord(Me.setup)
            Dim strucDef As New clsStructDefRecord(Me.setup)
            Dim contrDef As New clsControlDefRecord(Me.setup)

            'create the struct.dat record
            strucDat = New clsStructDatRecord(Me.setup)
            strucDat.ID = ID
            strucDat.nm = ID
            strucDat.dd = ID
            strucDat.InUse = True

            'create the struct.def record
            strucDef = New clsStructDefRecord(Me.setup)
            strucDef.ID = ID
            strucDef.nm = ID
            strucDef.InUse = True
            strucDef.ty = 7 'orifice
            strucDef.cl = CrestLevel
            strucDef.cw = CrestWidth
            strucDef.gh = CrestLevel + MaxGateHeight
            strucDef.Mu = 0.63
            strucDef.sc = 1
            strucDef.rt = FlowDirections
            strucDef.mp = 0
            strucDef.mpUsed = False
            strucDef.mn = 0
            strucDef.mnUsed = False

            setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.Records.Add(strucDat.ID.Trim.ToUpper, strucDat)
            setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.Records.Add(strucDef.ID.Trim.ToUpper, strucDef)

            'finally create the control definition (if applicable)
            With SeasonTransitions
                If measLoc Is Nothing Then Throw New Exception("Error: measurement location was not specified, but inlet controller gate was requested for orifice " & ID & ". Controller could not be created.")
                If Targetlevels Is Nothing Then Throw New Exception("Error: target levels were not specified, but inlet controller gate was requested for orifice " & ID & ". Controller could not be created.")
                contrDef = setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.BuildControlDefRecordForInletControlGate(strucDat.ID, measLoc.ID, CrestLevel, CrestLevel + MaxGateHeight, Targetlevels.getZPInlet, Targetlevels.getWPInlet, InletOnOff, TableStartYear, TableEndYear, SeasonTransitions)
                strucDat.ca = 1
                strucDat.cj = contrDef.ID
                strucDef.ov = 0
                ControlDefRecords.Records.Add(contrDef.ID.Trim.ToUpper, contrDef)
            End With

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildOrificeAsInletData.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Function BuildOrificeAsOutletData(ByVal ID As String, ByRef measLoc As clsSbkReachObject, Targetlevels As clsTargetLevels, CrestWidth As Double, CrestLevel As Double, MaxGateHeight As Double, OutletOnOff As Tuple(Of Double, Double), TableStartYear As Integer, TableEndYear As Integer, SeasonTransitions As clsSeasonTransitions, FlowDirections As GeneralFunctions.enmFlowDirection, SetInUse As Boolean) As Boolean
        Try
            Dim strucDat As New clsStructDatRecord(Me.setup)
            Dim strucDef As New clsStructDefRecord(Me.setup)
            Dim contrDef As New clsControlDefRecord(Me.setup)

            'create the struct.dat record
            strucDat = New clsStructDatRecord(Me.setup)
            strucDat.ID = ID
            strucDat.nm = ID
            strucDat.dd = ID
            strucDat.InUse = True

            'create the struct.def record
            strucDef = New clsStructDefRecord(Me.setup)
            strucDef.ID = ID
            strucDef.nm = ID
            strucDef.InUse = True
            strucDef.ty = 7 'orifice
            strucDef.cl = CrestLevel
            strucDef.cw = CrestWidth
            strucDef.gh = CrestLevel + MaxGateHeight
            strucDef.Mu = 0.63
            strucDef.sc = 1
            strucDef.rt = FlowDirections
            strucDef.mp = 0
            strucDef.mpUsed = False
            strucDef.mn = 0
            strucDef.mnUsed = False

            setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDatRecords.Records.Add(strucDat.ID.Trim.ToUpper, strucDat)
            setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.StructDefRecords.Records.Add(strucDef.ID.Trim.ToUpper, strucDef)

            'finally create the control definition
            With SeasonTransitions
                If measLoc Is Nothing Then Throw New Exception("Error: measurement location was not specified, but outlet controller gate was requested for orifice " & ID & ". Controller could not be created.")
                If Targetlevels Is Nothing Then Throw New Exception("Error: target levels were not specified, but outlet controller gate was requested for orifice " & ID & ". Controller could not be created.")
                contrDef = setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.StructureData.BuildControlDefRecordForOutletControlGate(strucDat.ID, measLoc.ID, CrestLevel, CrestLevel + MaxGateHeight, Targetlevels, OutletOnOff, TableStartYear, TableEndYear, SeasonTransitions)
                strucDat.ca = 1
                strucDat.cj = contrDef.ID
                strucDef.ov = 0
                ControlDefRecords.Records.Add(contrDef.ID.Trim.ToUpper, contrDef)
            End With

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildOrificeAsInletData.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Function AddStructDatRecord(ByVal ID As String, ByVal Name As String, ByVal DefinitionID As String, Optional ByVal InUse As Boolean = False) As clsStructDatRecord
        Dim datRecord As New clsStructDatRecord(Me.setup)
        datRecord.ID = ID
        datRecord.nm = Name
        datRecord.dd = DefinitionID
        InUse = True
        StructDatRecords.Records.Add(datRecord.ID.Trim.ToUpper, datRecord)
        Return datRecord
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)

        StructDatRecords.AddPrefix(Prefix)
        StructDefRecords.AddPrefix(Prefix)
        ControlDefRecords.AddPrefix(Prefix)

    End Sub


    Public Function BuildStructDatRecord(ByVal ID As String, ByVal Name As String) As clsStructDatRecord
        Dim SDat As New clsStructDatRecord(Me.setup)
        SDat.ID = ID
        SDat.nm = Name
        SDat.InUse = True
        SDat.dd = ID
        SDat.ca = 0
        SDat.cj = ""
        Return SDat
    End Function

    Public Function BuildControlDefRecordForTargetLevelFollowWeir(ID As String, ZP As Double, WP As Double, SeasonTransitions As clsSeasonTransitions, Optional ByVal DefaultCrestLevelAdjustment As Double = 0) As clsControlDefRecord
        Try
            Dim controlDef As New clsControlDefRecord(Me.setup)
            controlDef.ID = ID
            controlDef.nm = ID
            controlDef.InUse = True
            controlDef.ct = enmSobekControllerType.TIME 'time controller
            controlDef.ac = 1 'controller is active
            controlDef.ca = 0 'controlled parameter: crest level
            controlDef.cf = 1 'control each timestep
            controlDef.mc = 0 'change of value in time
            controlDef.bl = 0 'not sure. should be a block interpolation method but manual sais different than sobek files
            controlDef.titv = True
            controlDef.TimeTable.AddDatevalPair(New Date(2000, 1, 1), WP + DefaultCrestLevelAdjustment)
            controlDef.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.WinSumStartMonth, SeasonTransitions.WinSumStartDay), WP + DefaultCrestLevelAdjustment)
            controlDef.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.WinSumEndMonth, SeasonTransitions.WinSumEndDay), ZP + DefaultCrestLevelAdjustment)
            controlDef.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.SumWinStartMonth, SeasonTransitions.SumWinStartDay), ZP + DefaultCrestLevelAdjustment)
            controlDef.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.SumWinEndMonth, SeasonTransitions.SumWinEndDay), WP + DefaultCrestLevelAdjustment)
            controlDef.TimeTable.pdin1 = 1
            controlDef.TimeTable.pdin2 = 1
            controlDef.TimeTable.PDINPeriod = "365;00:00:00"
            Return controlDef
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildControlDefRecordForTargetLevelFollowWeir: " & ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function BuildControlDefRecordForInletControlGate(ID As String, measID As String, minOpeningGateLevel As Double, maxOpeningGateLevel As Double, ZP As Double, WP As Double, InletOnOff As Tuple(Of Double, Double), StartYear As Integer, EndYear As Integer, SeasonTransitions As clsSeasonTransitions) As clsControlDefRecord
        Try
            Dim controlDef As New clsControlDefRecord(Me.setup)
            controlDef.ID = ID
            controlDef.nm = ID
            controlDef.ct = enmSobekControllerType.INTERVAL 'interval controller is what we prefer
            controlDef.ac = 1 'controller active
            controlDef.ca = 2 'gate height
            controlDef.cf = 1 'control frequency
            controlDef.ml = measID
            controlDef.cp = 0 'measured parameter = water level
            controlDef.ui = maxOpeningGateLevel 'gate elevation when water level < target
            controlDef.ua = minOpeningGateLevel 'gate elevation when water level > target
            controlDef.cn = 1 'control interval type (0=fixed, 1= variable interval)
            controlDef.du = 0 'fixed interval
            controlDef.cv = 0.1 'crest movement speed
            controlDef.dt = 0 'deadband type (0=fixed)
            controlDef.d_ = Math.Abs(InletOnOff.Item1 - InletOnOff.Item2) / 100
            controlDef.bl = 0 'linear interpolation
            controlDef.va = 0.1 'crest moving speed
            If WP <> ZP Then
                controlDef.sptc = 1 'target level table
                For y = StartYear To EndYear
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumStartMonth, SeasonTransitions.WinSumStartDay), WP + (InletOnOff.Item1 + InletOnOff.Item2) / 200) 'correction on target level because of symmetric deadband
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumEndMonth, SeasonTransitions.WinSumEndDay), ZP + (InletOnOff.Item1 + InletOnOff.Item2) / 200) 'correction on target level because of symmetric deadband
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinStartMonth, SeasonTransitions.SumWinStartDay), ZP + (InletOnOff.Item1 + InletOnOff.Item2) / 200) 'correction on target level because of symmetric deadband
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinEndMonth, SeasonTransitions.SumWinEndDay), WP + (InletOnOff.Item1 + InletOnOff.Item2) / 200) 'correction on target level because of symmetric deadband
                Next
            Else
                controlDef.sptc = 0 'target level constant
                controlDef.SetPointValue = ZP + (InletOnOff.Item1 + InletOnOff.Item2) / 200 'correction on target level because of symmetric deadband
            End If
            controlDef.InUse = True
            Return controlDef
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildControlDefRecordForPIDControlledWeir.")
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function buildcontroldefrecordForTimeControlledGate(ID As String, DatesValues As Dictionary(Of Date, Double), Optional ByVal Periodicity As String = "365;00:00:00") As clsControlDefRecord
        Try
            Dim controlDef As New clsControlDefRecord(Me.setup)
            controlDef.ID = ID
            controlDef.nm = ID
            controlDef.ct = enmSobekControllerType.TIME 'time controller
            controlDef.ac = 1 'controller active
            controlDef.ca = 2 'gate height
            controlDef.cf = 1 'control frequency
            controlDef.mc = 0
            controlDef.bl = 1 'block interpolation
            controlDef.titv = True

            If Not Periodicity = "" Then
                controlDef.TimeTable.pdin1 = 1
                controlDef.TimeTable.pdin2 = 1
                controlDef.TimeTable.PDINPeriod = Periodicity
            Else
                controlDef.TimeTable.pdin1 = 0
                controlDef.TimeTable.pdin2 = 0
            End If

            For Each myDate As Date In DatesValues.Keys
                controlDef.TimeTable.AddDatevalPair(myDate, DatesValues.Item(myDate))
            Next
            controlDef.InUse = True
            Return controlDef
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function buildcontroldefrecordForTimeControlledGate.")
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function BuildControlDefRecordForOutletControlGate(ID As String, measID As String, minOpeningGateLevel As Double, maxOpeningGateLevel As Double, TargetLevels As clsTargetLevels, OutletOnOff As Tuple(Of Double, Double), StartYear As Integer, EndYear As Integer, SeasonTransitions As clsSeasonTransitions) As clsControlDefRecord
        Try
            Dim controlDef As New clsControlDefRecord(Me.setup)
            controlDef.ID = ID
            controlDef.nm = ID
            controlDef.ct = enmSobekControllerType.INTERVAL 'interval controller is what we prefer
            controlDef.ac = 1 'controller active
            controlDef.ca = 2 'gate height
            controlDef.cf = 1 'control frequency
            controlDef.ml = measID
            controlDef.cp = 0 'measured parameter = water level
            controlDef.ui = minOpeningGateLevel 'gate height when water level < target
            controlDef.ua = maxOpeningGateLevel 'gate height when water level > target
            controlDef.cn = 1 'control interval type (0=fixed, 1= variable interval)
            controlDef.du = 0 'fixed interval
            controlDef.cv = 0.1 'crest movement speed
            controlDef.dt = 0 'deadband type (0=fixed)
            controlDef.d_ = Math.Abs(OutletOnOff.Item1 - OutletOnOff.Item2) / 100
            controlDef.bl = 0 'linear interpolation
            controlDef.va = 0.1 'crest moving speed
            If TargetLevels.getWPOutlet <> TargetLevels.getZPOutlet Then
                controlDef.sptc = 1 'target level table
                For y = StartYear To EndYear
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumStartMonth, SeasonTransitions.WinSumStartDay), TargetLevels.getWPOutlet + (OutletOnOff.Item1 / 100 + OutletOnOff.Item2 / 100) / 2) 'correction on target level because of symmetric deadband
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumEndMonth, SeasonTransitions.WinSumEndDay), TargetLevels.getZPOutlet + (OutletOnOff.Item1 / 100 + OutletOnOff.Item2 / 100) / 2) 'correction on target level because of symmetric deadband
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinStartMonth, SeasonTransitions.SumWinStartDay), TargetLevels.getZPOutlet + (OutletOnOff.Item1 / 100 + OutletOnOff.Item2 / 100) / 2) 'correction on target level because of symmetric deadband
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinEndMonth, SeasonTransitions.SumWinEndDay), TargetLevels.getWPOutlet + (OutletOnOff.Item1 / 100 + OutletOnOff.Item2 / 100) / 2) 'correction on target level because of symmetric deadband
                Next
            Else
                controlDef.sptc = 0 'target level constant
                controlDef.SetPointValue = TargetLevels.getZPOutlet + (OutletOnOff.Item1 / 100 + OutletOnOff.Item2 / 100) / 2 'correction on target level because of symmetric deadband
            End If
            controlDef.InUse = True
            Return controlDef
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildControlDefRecordForOutletControlGate.")
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function buildControlDefRecordForIntervalControlledOutletWeir(ID As String, measID As String, minCrest As Double, MaxCrest As Double, CrestChangeSpeed As Double, ZP As Double, WP As Double, DeadBand As Double, StartYear As Integer, EndYear As Integer, SeasonTransitions As clsSeasonTransitions) As clsControlDefRecord
        Try
            Dim controlDef As New clsControlDefRecord(Me.setup)
            controlDef.ID = ID
            controlDef.nm = ID
            controlDef.InUse = True
            controlDef.ct = enmSobekControllerType.INTERVAL
            controlDef.ac = 1 'active
            controlDef.ca = 0 'controllled parameter: crest leel
            controlDef.cf = 1 'control frequency each timestep
            controlDef.ml = measID
            controlDef.cp = 0 'measured parameter: waterlevel
            controlDef.u0 = ZP
            controlDef.d_ = DeadBand
            controlDef.ui = MaxCrest
            controlDef.ua = minCrest
            controlDef.cv = CrestChangeSpeed 'crest change speed
            controlDef.bl = 1                           'v1.86: switched to block interpolation
            controlDef.cn = enmSetpointType.VARIABLE    'v1.86: changed this setting to variable. Seems to be the default in SOBEK
            If WP <> ZP Then
                controlDef.sptc = 1 'target level table
                For y = StartYear To EndYear
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumStartMonth, SeasonTransitions.WinSumStartDay), WP)
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumEndMonth, SeasonTransitions.WinSumEndDay), ZP)
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinStartMonth, SeasonTransitions.SumWinStartDay), ZP)
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinEndMonth, SeasonTransitions.SumWinEndDay), WP)
                Next
            Else
                controlDef.sptc = 0 'target level constant
                controlDef.SetPointValue = ZP
            End If
            Return controlDef
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function buildControlDefRecordForIntervalControlledOutletWeir.")
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function buildControlDefRecordForIntervalControlledInletWeir(ID As String, measID As String, CrestLow As Double, CrestHigh As Double, CrestChangeSpeed As Double, ZP As Double, WP As Double, StartYear As Integer, EndYear As Integer, SeasonTransitions As clsSeasonTransitions, InletOnCM As Double, InletOffCM As Double) As clsControlDefRecord
        Try
            'new function in v1.900
            Dim controlDef As New clsControlDefRecord(Me.setup)
            controlDef.ID = ID
            controlDef.nm = ID
            controlDef.InUse = True
            controlDef.ct = enmSobekControllerType.INTERVAL
            controlDef.ac = 1                                   'active
            controlDef.ca = 0                                   'controllled parameter: crest leel
            controlDef.cf = 1                                   'control frequency each timestep
            controlDef.ml = measID
            controlDef.cp = 0                                   'measured parameter: waterlevel
            controlDef.ui = CrestLow                            'minimum crest level
            controlDef.ua = CrestHigh                           'maximum crest level
            controlDef.cn = enmSetpointType.VARIABLE            'v1.86: changed this setting to variable. Seems to be the default in SOBEK
            controlDef.du = 0
            controlDef.cv = CrestChangeSpeed                    'crest change speed
            controlDef.dt = 0                                   'deadband type
            controlDef.d_ = (InletOffCM - InletOnCM) / 100      'deadband
            controlDef.bl = 1                                   'v1.86: switched to block interpolation
            controlDef.sptc = 1                                 'target level table. if the water level drops below the target + margin, the weir is dropped
            For y = StartYear To EndYear
                controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumStartMonth, SeasonTransitions.WinSumStartDay), WP + (InletOnCM + InletOffCM) / 200)
                controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumEndMonth, SeasonTransitions.WinSumEndDay), ZP + (InletOnCM + InletOffCM) / 200)
                controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinStartMonth, SeasonTransitions.SumWinStartDay), ZP + (InletOnCM + InletOffCM) / 200)
                controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinEndMonth, SeasonTransitions.SumWinEndDay), WP + (InletOnCM + InletOffCM) / 200)
            Next
            Return controlDef
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function buildControlDefRecordForIntervalControlledSummerInletWeir.")
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function BuildControlDefRecordForPIDControlledWeir(ID As String, measID As String, minCrest As Double, MaxCrest As Double, ZP As Double, WP As Double, PIDSetting As clsPIDSetting, StartYear As Integer, EndYear As Integer, SeasonTransitions As clsSeasonTransitions) As clsControlDefRecord
        Try
            Dim controlDef As New clsControlDefRecord(Me.setup)
            controlDef.ID = ID
            controlDef.nm = ID
            controlDef.InUse = True
            controlDef.ct = enmSobekControllerType.PID 'PID
            controlDef.ac = 1 'active
            controlDef.ca = 0 'controllled parameter: crest leel
            controlDef.cf = 1 'control frequency each timestep
            controlDef.ml = measID
            controlDef.cp = 0 'measured parameter: waterlevel
            controlDef.ui = minCrest
            controlDef.ua = MaxCrest
            controlDef.u0 = ZP
            controlDef.pf = PIDSetting.Kp
            controlDef.if_ = PIDSetting.Ki
            controlDef.df = PIDSetting.Kd
            controlDef.va = 0.1 'crest moving speed
            controlDef.bl = 0 'linear interpolation
            If WP <> ZP Then
                controlDef.sptc = 1 'target level table
                For y = StartYear To EndYear
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumStartMonth, SeasonTransitions.WinSumStartDay), WP)
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.WinSumEndMonth, SeasonTransitions.WinSumEndDay), ZP)
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinStartMonth, SeasonTransitions.SumWinStartDay), ZP)
                    controlDef.TimeTable.AddDatevalPair(New Date(y, SeasonTransitions.SumWinEndMonth, SeasonTransitions.SumWinEndDay), WP)
                Next
            Else
                controlDef.sptc = 0 'target level constant
                controlDef.SetPointValue = ZP
            End If
            Return controlDef
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildControlDefRecordForPIDControlledWeir.")
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function BuildControlDefRecordForWeirCalamityController(ByVal ID As String, Meas As clsSbkReachObject, ByRef TargetLevels As clsTargetLevels, OnOffCM As Tuple(Of Double, Double), CrestWhenAbove As Double, CrestWhenBelow As Double) As clsControlDefRecord
        'this routine builds a controller for a weir calamity action
        Dim CDef As New clsControlDefRecord(Me.setup)
        CDef.ID = ID
        CDef.nm = ID
        CDef.InUse = True
        CDef.ct = enmSobekControllerType.INTERVAL         'interval controller
        CDef.ac = 1         'controller is active
        CDef.ca = 0         'controlled parameter = crest level
        CDef.cf = 1         'control frequency = 1
        CDef.ml = Meas.ID   'measurement location
        CDef.cp = 0         'water level is being measured
        CDef.ui = CrestWhenBelow
        CDef.ua = CrestWhenAbove
        CDef.cn = 1
        CDef.du = 0
        CDef.cv = 0.01       'crest velocity
        CDef.dt = 0
        CDef.d_ = Math.Abs(OnOffCM.Item1 - OnOffCM.Item2) / 100                               'band width
        CDef.sptc = 0        'constant target level
        CDef.SetPointValue = TargetLevels.getWPOutlet + (OnOffCM.Item1 + OnOffCM.Item2) / 200          'target level
        CDef.bl = 0         'block interpolation
        CDef.hcht = 1       'control table
        Return CDef
    End Function

    Public Function BuildControlDefRecordForWeirFlushConservationController(ByVal ID As String, CrestDown As Double, CrestUp As Double, SeasonTransitions As clsSeasonTransitions) As clsControlDefRecord
        'this routine builds a controller for a weir event action where it is designed to do flush conservation (raise during the flush period)
        Dim CDef As New clsControlDefRecord(Me.setup)
        With SeasonTransitions
            CDef.ID = ID
            CDef.nm = ID
            CDef.InUse = True
            CDef.ct = enmSobekControllerType.TIME         'time controller
            CDef.ac = 1         'controller is active
            CDef.ca = 0         'controlled parameter = crest level
            CDef.cf = 1         'control frequency = 1
            CDef.mc = 0         'crest speed
            CDef.bl = 0         'block interpolation
            CDef.cp = 0         'water level is being measured
            CDef.titv = True
            CDef.TimeTable.pdin1 = 1
            CDef.TimeTable.pdin2 = 1
            CDef.TimeTable.PDINPeriod = "365;00:00:00"
            CDef.TimeTable.AddDatevalPair(New Date(2000, 1, 1), CrestDown)
            CDef.TimeTable.AddDatevalPair(New Date(2000, .FlushStartMonth, .FlushStartDay), CrestUp)
            CDef.TimeTable.AddDatevalPair(New Date(2000, .FlushEndMonth, .FLushEndDay), CrestDown)
        End With
        Return CDef
    End Function

    Public Function BuildControlDefRecordForEmergencyStopDownstream(ByVal ID As String, ByVal MeasID As String, ByVal Threshold As Double, ByVal OpeningHeight As Double, ByRef SDef As clsStructDefRecord) As clsControlDefRecord
        'this routine builds a controller for an emergency stop, downstream control. 
        'under normal circumstances this gate is open
        'It closes when the water level downstream exceeds a certain level
        Dim CDef As New clsControlDefRecord(Me.setup)
        CDef.ID = ID
        CDef.nm = ID
        CDef.InUse = True
        CDef.ct = enmSobekControllerType.HYDRAULIC         'hydraulic controller
        CDef.ac = 1         'controller is active
        CDef.ca = 2         'controlled parameter = gate height
        CDef.cf = 1         'control frequency = 1
        CDef.ml = MeasID    'measurement location
        CDef.cp = 0         'water level is being measured
        CDef.bl = 1         'linear interpolation
        CDef.hcht = 1       'control table
        CDef.ControlTable.AddDataPair(2, Threshold, SDef.cl + OpeningHeight)  'if H < threshold, gate is open
        CDef.ControlTable.AddDataPair(2, Threshold + 0.01, SDef.cl)           'if H > threshold, gate is closed
        Return CDef
    End Function

    Public Function BuildControlDefRecordForEmergencyStopOverrule(ByVal ID As String, ByVal MeasID As String, ByVal Threshold As Double, ByVal OpeningHeight As Double, ByRef SDef As clsStructDefRecord) As clsControlDefRecord
        'this routine overrules an emergency stop by opening a gate when upstream target level exceeds a given safety threshold
        Dim CDef As New clsControlDefRecord(Me.setup)
        CDef.ID = ID
        CDef.nm = ID
        CDef.InUse = True
        CDef.ct = enmSobekControllerType.HYDRAULIC         'hydraulic controller
        CDef.ac = 1         'controller is active
        CDef.ca = 2         'controlled parameter = gate height
        CDef.cf = 1         'control frequency = 1
        CDef.ml = MeasID    'measurement location
        CDef.cp = 0         'water level is being measured
        CDef.bl = 1         'linear interpolation
        CDef.hcht = 1       'control table
        CDef.ControlTable.AddDataPair(2, Threshold, SDef.cl)  'if H < threshold, gate is closed
        CDef.ControlTable.AddDataPair(2, Threshold + 0.01, SDef.cl + OpeningHeight)           'if H > threshold, gate is open
        Return CDef
    End Function

    Public Function BuildStructDefRecordForOrifice(ByVal ID As String, ByVal Name As String, ByVal cl As Double, ByVal cw As Double, ByVal gh As Double) As clsStructDefRecord
        Dim SDef As New clsStructDefRecord(Me.setup)
        SDef.ID = ID
        SDef.nm = Name
        SDef.InUse = True
        SDef.ty = 7                 'orifice
        SDef.cl = cl                'crest level (m AD)
        SDef.cw = cw                'crest width
        SDef.gh = gh                'gate height
        SDef.Mu = 0.63              'coef
        SDef.sc = 1                 'coef
        SDef.rt = 0                 'flow in both directions possible
        SDef.mpUsed = 0             'no flow limiter positive
        SDef.mp = 0                 'flow limitation positive = 0
        SDef.mnUsed = 0             'no flow limter negative
        SDef.mn = 0                 'flow limitation negative = 0
        Return SDef
    End Function

    Public Function BuildStructDefRecordForFlowLimitation(ByVal ID As String, ByVal Name As String, ByVal ValueMMPD As Double, ByVal TotalArea As Double, ByVal WP As Double) As clsStructDefRecord
        Dim SDef As New clsStructDefRecord(Me.setup)
        SDef.ID = ID
        SDef.nm = Name
        SDef.InUse = True
        SDef.ty = 7                 'orifice
        SDef.cl = WP - 1            'crest one meter below target level
        SDef.cw = 2                 'crest width
        SDef.gh = WP - 1 + 10       'gate lower end elevation level
        SDef.Mu = 0.63              'coef
        SDef.sc = 1                 'coef
        SDef.rt = 0
        SDef.mpUsed = 1
        SDef.mp = Format(Me.setup.GeneralFunctions.MMPD2M3PS(ValueMMPD, TotalArea), "0.000")
        SDef.mnUsed = 0
        SDef.mn = 0
        Return SDef
    End Function

    Public Function BuildStructDatRecord(ByVal ID As String, ByVal Name As String, ByVal ControllerID As String) As clsStructDatRecord
        Dim SDat As New clsStructDatRecord(Me.setup)
        SDat.ID = ID
        SDat.InUse = True
        SDat.nm = Name
        SDat.dd = ID
        SDat.ca = 0
        SDat.cj = ""
        Return SDat
    End Function

    Public Function BuildStructDatRecordWithController(ByVal ID As String, ByVal Name As String, ByVal ControllerID As String) As clsStructDatRecord
        Dim SDat As New clsStructDatRecord(Me.setup)
        SDat.ID = ID
        SDat.InUse = True
        SDat.nm = Name
        SDat.dd = ID
        SDat.ca = 1
        SDat.cj = ControllerID
        Return SDat
    End Function

    Public Function BuildStructDefRecordForWeir(ByVal ID As String, ByVal Name As String, ByVal WP As Double, ByVal CrestWidth As Double) As clsStructDefRecord
        Dim SDef As New clsStructDefRecord(Me.setup)
        SDef.ID = ID
        SDef.nm = Name
        SDef.InUse = True
        SDef.ty = 6
        SDef.cl = WP
        SDef.cw = CrestWidth
        SDef.ce = 1.5
        SDef.sc = 0.9
        SDef.rt = 0
        Return SDef
    End Function

    Public Function BuildControlDefRecordForWeir(ByVal ID As String, ByVal WP As Double, ByVal ZP As Double) As clsControlDefRecord
        'bouw de sturing op zomer/winterpeil in
        Dim CDef As New clsControlDefRecord(Me.setup)
        CDef.ID = ID            'controller id
        CDef.nm = ID            'controller name
        CDef.InUse = True       'set de controller op InUse
        CDef.ct = 0             'controller type (0=time controller)
        CDef.ac = 1             'controller active (0=no, 1=yes)
        CDef.ca = 0             'controlled parameter (0=crest level)
        CDef.cf = 1             'update frequency
        CDef.mc = 0             'max dX/dt
        CDef.bl = 1             'interpolation method
        CDef.titv = True        'time table will be specified
        CDef.TimeTable.pdin1 = 1 'period true
        CDef.TimeTable.pdin2 = 1 'block interpolation
        CDef.TimeTable.PDINPeriod = "365;00:00:00"
        CDef.TimeTable.AddDatevalPair(New Date(2000, 1, 1), WP)
        CDef.TimeTable.AddDatevalPair(New Date(2000, 4, 15), ZP)
        CDef.TimeTable.AddDatevalPair(New Date(2000, 10, 15), WP)
        Return CDef
    End Function

    Public Function BuildControlDefRecordForTimeControlledInlet(ByVal ID As String, ByVal TotalArea As Double, ByVal InletCapMMPD As Double, ByVal StartDay As Integer, ByVal StartMonth As Integer, ByVal StopDay As Integer, ByVal StopMonth As Integer) As clsControlDefRecord
        'bouw de inlaatsturing op basis van een tijdstabel in
        Dim CDef As New clsControlDefRecord(Me.setup)
        CDef.ID = ID            'controller id
        CDef.nm = ID            'controller name
        CDef.InUse = True
        CDef.ct = 0             'controller type (0=time controller)
        CDef.ac = 1             'controller active (0=no, 1=yes)
        CDef.ca = 3             'controlled parameter (3 = pump capacity)
        CDef.cf = 1             'update frequency
        CDef.mc = 0
        CDef.bl = 1
        CDef.titv = True
        CDef.TimeTable = New clsSobekTable(setup)
        CDef.TimeTable.pdin1 = 1
        CDef.TimeTable.pdin2 = 1
        CDef.TimeTable.PDINPeriod = "365;00:00:00"
        CDef.TimeTable.AddDatevalPair(New Date(2000, 1, 1), 0)
        CDef.TimeTable.AddDatevalPair(New Date(2000, StartMonth, StartDay), setup.GeneralFunctions.MMPD2M3PS(InletCapMMPD, TotalArea))
        CDef.TimeTable.AddDatevalPair(New Date(2000, StopMonth, StopDay), 0)
        Return CDef
    End Function

    Public Function BuildControlDefRecordForPump(ByVal ID As String, ByVal measID As String, ByVal PumpCapAboveThreshold As Double, PumpCapBelowThreshold As Double, ByVal WP As Double, ByVal ZP As Double, StartYear As Integer, EndYear As Integer, SeasonTransitions As clsSeasonTransitions, OnCM As Integer, OffCM As Integer) As clsControlDefRecord
        'bouw de sturing op zomer/winterpeil in
        'let op: pompnum is 0-based. Eerste pomp heeft nr 0, tweede 1 etc.
        'we create a fictive summer/winter target level because the deadband is symmetrical. 
        Dim CDef As New clsControlDefRecord(Me.setup)
        Dim j As Integer

        CDef.ID = ID                                'controller id
        CDef.nm = ID                                'controller name
        CDef.InUse = True
        CDef.ct = 2                                 'controller type (2=interval controller)
        CDef.ac = 1                                 'controller active (0=no, 1=yes)
        CDef.ca = 3                                 'controlled parameter (3 = pump capacity)
        CDef.cf = 1                                 'update frequency
        CDef.ml = measID                            'measurement station ID (="meas" &Dummyconstruction ID)
        CDef.cp = 0                                 'measured parameter (0= water level)
        CDef.ui = PumpCapBelowThreshold             'value if measured < targed - deadband/2
        CDef.ua = PumpCapAboveThreshold             'pump cap in m3/s when measured > target + deadband/2
        CDef.cn = 1                                 '1=variable interval
        CDef.du = 0                                 'd(u) (fixed interval)
        CDef.cv = 0.1                               'control velocity
        CDef.dt = 0                                 'fixed dead band
        CDef.d_ = Math.Abs(OnCM - OffCM) / 100
        CDef.bl = 1                                 '1 = linear interpolation

        'calculate a fictive target level because the deadband has to become symmetric
        WP += (OnCM + OffCM) / 100 / 2
        ZP += (OnCM + OffCM) / 100 / 2

        If ZP <> WP Then
            CDef.sptc = 1           'variable set point in time
            CDef.TimeTable.AddDatevalPair(New Date(StartYear, 1, 1), WP)
            For j = StartYear To EndYear
                CDef.TimeTable.AddDatevalPair(New Date(j, SeasonTransitions.WinSumStartMonth, SeasonTransitions.WinSumStartDay), WP)
                CDef.TimeTable.AddDatevalPair(New Date(j, SeasonTransitions.WinSumEndMonth, SeasonTransitions.WinSumEndDay), ZP)
                CDef.TimeTable.AddDatevalPair(New Date(j, SeasonTransitions.SumWinStartMonth, SeasonTransitions.SumWinStartDay), ZP)
                CDef.TimeTable.AddDatevalPair(New Date(j, SeasonTransitions.SumWinEndMonth, SeasonTransitions.SumWinEndDay), WP)
            Next
            CDef.TimeTable.AddDatevalPair(New Date(EndYear, 12, 31), WP)
        Else
            CDef.sptc = 0
            CDef.SetPointValue = WP
        End If
        Return CDef

    End Function


    Public Function BuildControlDefRecordForFlushPump(ByVal ID As String, ByVal PumpCap As Double, SeasonTransitions As clsSeasonTransitions) As clsControlDefRecord
        'bouw de sturing op doorspoelregime
        'let op: pompnum is 0-based. Eerste pomp heeft nr 0, tweede 1 etc.
        'we create a fictive summer/winter target level because the deadband is symmetrical. 
        Dim CDef As New clsControlDefRecord(Me.setup)
        CDef.ID = ID                                'controller id
        CDef.nm = ID                                'controller name
        CDef.InUse = True
        CDef.ct = 0                                 'controller type (0 = time controller)
        CDef.ac = 1                                 'controller active (0=no, 1=yes)
        CDef.ca = 3                                 'controlled parameter (3 = pump capacity)
        CDef.cf = 1                                 'update frequency
        CDef.mc = 0                                 'change value/dt
        CDef.bl = 1                                 'linear interpolation
        CDef.TimeTable.pdin1 = 1                    'block interpolation
        CDef.TimeTable.pdin2 = 1                    'use return period
        CDef.TimeTable.PDINPeriod = "365;00:00:00"  '1 year return period
        CDef.TimeTable.AddDatevalPair(New Date(2000, 1, 1), 0)
        CDef.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.FlushStartMonth, SeasonTransitions.FlushStartDay), PumpCap)
        CDef.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.FlushEndMonth, SeasonTransitions.FLushEndDay), 0)
        Return CDef

    End Function

    Public Function BuildControlDefRecordForEmergencyPump(ByVal ID As String, ByVal CapMultiplier As Double, TimeSeriesTable As String, DatabaseTimeTablePumpID As String, ParameterName As String) As clsControlDefRecord
        'bouw de sturing op tijdstabel met noodbemaling
        'let op: pompnum is 0-based. Eerste pomp heeft nr 0, tweede 1 etc.
        Dim CDef As New clsControlDefRecord(Me.setup)
        CDef.ID = ID                                'controller id
        CDef.nm = ID                                'controller name
        CDef.InUse = True
        CDef.ct = 0                                 'controller type (0 = time controller)
        CDef.ac = 1                                 'controller active (0=no, 1=yes)
        CDef.ca = 3                                 'controlled parameter (3 = pump capacity)
        CDef.cf = 1                                 'update frequency
        CDef.mc = 0                                 'change value/dt
        CDef.bl = 1                                 'linear interpolation
        CDef.TimeTable.pdin1 = 1                    'block interpolation
        CDef.TimeTable.pdin2 = 0                    'no return period
        CDef.TimeTable.PDINPeriod = ""              'no return period

        Dim dt As New DataTable
        Dim query As String = "SELECT DATEANDTIME, DATAVALUE FROM " & TimeSeriesTable & " WHERE PUMPID='" & DatabaseTimeTablePumpID & "' AND PARAMETER='" & ParameterName & "' ORDER BY DATEANDTIME;"
        Me.setup.GeneralFunctions.SQLiteQuery(Me.setup.SqliteCon, query, dt)

        For i = 0 To dt.Rows.Count - 1
            CDef.TimeTable.AddDatevalPair(dt.Rows(i)(0), dt.Rows(i)(1) * CapMultiplier)
        Next
        Return CDef

    End Function

    Public Function BuildStructDefRecordForPump(ByVal ID As String, ByVal Name As String, ByVal PumpCaps As clsPumpOperationItem, ByVal WP As Double, ByVal ZP As Double) As clsStructDefRecord
        Dim SDef As New clsStructDefRecord(Me.setup)

        'HET STRUCT.DEF RECORD
        SDef.ID = ID
        SDef.nm = Name
        SDef.InUse = True
        SDef.ty = 9
        SDef.dn = 1
        SDef.rtcr1 = 0
        SDef.rtcr2 = 1
        SDef.rtcr3 = 0
        SDef.ctlt = 1
        If WP <> ZP Then
            'zorg dat hij altijd aanstaat; wordt straks overruled door controller
            SDef.ctltTable.AddDataPair(5, Format(PumpCaps.Cap, "0.000"), -9, -9.9, 0, 0)
        Else
            SDef.ctltTable.AddDataPair(5, Format(PumpCaps.Cap, "0.000"), WP + PumpCaps.UpOnMargin / 100, WP + PumpCaps.UpOffMargin / 100, 0, 0)
        End If
        Return SDef
    End Function

    Public Function BuildStructDefRecordForLevelControlledInlet(ByVal ID As String, ByVal Name As String, ByVal TotalArea As Double, ByVal CapMMPD As Double, ByVal WP As Double, ByVal ZP As Double) As clsStructDefRecord
        Dim SDef As New clsStructDefRecord(Me.setup)
        'HET STRUCT.DEF RECORD
        SDef.ID = ID
        SDef.nm = Name
        SDef.InUse = True

        SDef.ty = 9
        SDef.dn = 2
        SDef.rtcr1 = 0
        SDef.rtcr2 = 1
        SDef.rtcr3 = 0
        SDef.ctlt = 1
        SDef.ctltTable.AddDataPair(5, Format(setup.GeneralFunctions.MMPD2M3PS(CapMMPD, TotalArea), "0.000"), 0, 0, WP - 0.15, WP - 0.1)
        Return SDef
    End Function

    ''' <summary>
    ''' TODO: Siebe invullen
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub Export(ByVal Append As Boolean, ExportDir As String)

        Try
            Me.StructDatRecords.Write(Append, ExportDir)
            Me.StructDefRecords.Write(Append, ExportDir)
            Me.ControlDefRecords.Write(Append, ExportDir)
            Me.ValveTabRecords.Write(Append, ExportDir)
        Catch ex As Exception
            Dim log As String = "Error in Export CF structure data"
            Me.setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try


    End Sub

    Friend Sub MatchElevationWithChannelBed(ByVal Culverts As Boolean, ByVal Bridges As Boolean)
        'doorloop alle takobjecten
        Dim StrucDat As clsStructDatRecord
        Dim StrucDef As clsStructDefRecord

        For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
            For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge AndAlso Bridges = True Then
                    'zoek het structdat-record op
                    StrucDat = SbkCase.CFData.Data.StructureData.StructDatRecords.Records(myObj.ID.Trim.ToUpper)
                    If Not StrucDat Is Nothing Then
                        StrucDef = SbkCase.CFData.Data.StructureData.StructDefRecords.Records(StrucDat.ID.Trim.ToUpper)
                        If Not StrucDef Is Nothing Then
                            myReach.getBedLevel(myObj.lc, StrucDef.rl)
                        End If
                    End If
                End If
            Next
        Next
    End Sub

    Friend Function shiftByElevationGrid(ByVal Culverts As Boolean, ByVal Bridges As Boolean, ByVal Orifices As Boolean, ByVal UniWeir As Boolean) As Boolean

        Dim myReach As clsSbkReach, iReach As Long
        Dim nReach As Long = SbkCase.CFTopo.Reaches.Reaches.Count
        Dim myObj As clsSbkReachObject
        Dim myStrucDat As clsStructDatRecord
        Dim myStrucDef As clsStructDefRecord
        Dim myControlDef As clsControlDefRecord
        Dim myShift As Single

        Me.setup.GeneralFunctions.UpdateProgressBar("Shifting structures by value from elevation grid...", 0, 10)
        For Each myReach In SbkCase.CFTopo.Reaches.Reaches.Values
            iReach += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
            For Each myObj In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFCulvert AndAlso Culverts = True Then
                    If StructDatRecords.Records.ContainsKey(myObj.ID.Trim.ToUpper) Then
                        myStrucDat = StructDatRecords.Records.Item(myObj.ID.Trim.ToUpper)
                        If StructDefRecords.Records.ContainsKey(myStrucDat.dd.Trim.ToUpper) Then
                            myStrucDef = StructDefRecords.Records.Item(myStrucDat.dd.Trim.ToUpper)
                            myObj.calcXY()
                            Me.setup.GISData.ElevationGrid.GetCellValueFromXY(myObj.X, myObj.Y, myShift)
                            myStrucDef.rl += myShift
                            myStrucDef.ll += myShift
                        End If
                    End If
                ElseIf myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge AndAlso Bridges = True Then
                    If StructDatRecords.Records.ContainsKey(myObj.ID.Trim.ToUpper) Then
                        myStrucDat = StructDatRecords.Records.Item(myObj.ID.Trim.ToUpper)
                        If StructDefRecords.Records.ContainsKey(myStrucDat.dd.Trim.ToUpper) Then
                            myStrucDef = StructDefRecords.Records.Item(myStrucDat.dd.Trim.ToUpper)
                            myObj.calcXY()
                            Me.setup.GISData.ElevationGrid.GetCellValueFromXY(myObj.X, myObj.Y, myShift)
                            myStrucDef.rl += myShift
                        End If
                    End If
                ElseIf myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice AndAlso Orifices = True Then
                    If StructDatRecords.Records.ContainsKey(myObj.ID.Trim.ToUpper) Then
                        myStrucDat = StructDatRecords.Records.Item(myObj.ID.Trim.ToUpper)
                        If StructDefRecords.Records.ContainsKey(myStrucDat.dd.Trim.ToUpper) Then
                            myStrucDef = StructDefRecords.Records.Item(myStrucDat.dd.Trim.ToUpper)
                            myObj.calcXY()
                            Me.setup.GISData.ElevationGrid.GetCellValueFromXY(myObj.X, myObj.Y, myShift)
                            myStrucDef.cl += myShift
                            myStrucDef.gh += myShift 'notice that gh (gate height) is (in the files) expressed in m AD. Hence we'll need to adjust it too

                            If myStrucDat.ca = 1 Then
                                'a controller is active for this orifice, and since the gate height is expressed in m AD we MUST adjust it too!
                                myControlDef = ControlDefRecords.Records.Item(myStrucDat.cj.Trim.ToUpper)
                                If Not myControlDef Is Nothing Then
                                    myControlDef.ui += myShift
                                    myControlDef.ua += myShift
                                    myControlDef.u0 += myShift

                                    'in case of a time controller or hydraulic controller we'll also have to adjust!
                                    If myControlDef.ct = 0 Then
                                        myControlDef.TimeTable.addToValues(1, myShift)
                                    ElseIf myControlDef.ct = 1 Then
                                        myControlDef.ControlTable.addToValues(1, myShift)
                                    End If

                                End If
                            End If

                        End If
                    End If
                ElseIf myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir AndAlso UniWeir = True Then
                    If StructDatRecords.Records.ContainsKey(myObj.ID.Trim.ToUpper) Then
                        myStrucDat = StructDatRecords.Records.Item(myObj.ID.Trim.ToUpper)
                        If StructDefRecords.Records.ContainsKey(myStrucDat.dd.Trim.ToUpper) Then
                            myStrucDef = StructDefRecords.Records.Item(myStrucDat.dd.Trim.ToUpper)
                            myObj.calcXY()
                            Me.setup.GISData.ElevationGrid.GetCellValueFromXY(myObj.X, myObj.Y, myShift)
                            myStrucDef.cl += myShift
                        End If
                    End If
                End If
            Next
        Next

    End Function

    Public Function getMaxPumpCapacity(ByVal ID As String) As Double
        Dim DatRecord As clsStructDatRecord
        Dim DefRecord As clsStructDefRecord
        Dim ContrRecord As clsControlDefRecord
        Dim TotCap As Double = 0
        Dim i As Long

        DatRecord = StructDatRecords.Records.Item(ID.Trim.ToUpper)
        If Not DatRecord Is Nothing Then

            If DatRecord.ca = 0 Then
                'geen controller actief
                DefRecord = StructDefRecords.Records.Item(DatRecord.dd.Trim.ToUpper)
                'elke rij in de tabel bevat een additionele capaciteit, dus optellen!
                For i = 0 To DefRecord.ctltTable.XValues.Count - 1
                    TotCap += DefRecord.ctltTable.XValues.Values(i)
                Next
                Return TotCap
            ElseIf DatRecord.ca = 1 Then
                'controller actief!
                ContrRecord = ControlDefRecords.Records.Item(DatRecord.cj.Trim.ToUpper)
                If ContrRecord.ct = 0 Then 'time
                    Return 0
                ElseIf ContrRecord.ct = 1 Then 'hydraulic dus tabel met cap als functie van andere grootheid
                    Return ContrRecord.ControlTable.Values1.Values(ContrRecord.ControlTable.Values1.Count - 1)
                ElseIf ContrRecord.ct = 2 Then 'interval
                    Return ContrRecord.ua
                ElseIf ContrRecord.ct = 3 Then 'PID
                    Return 0
                End If
            End If

        End If

        Return 0
    End Function

    Public Sub AddGroundLayerToCulverts()

        'doorloop nu alle culverts, zoek de omringende dwarsprofielen en interpoleer de laagste bodemhoogtes
        Dim myObj As clsSbkReachObject, myReach As clsSbkReach
        Dim BOBup As Double, BOBdown As Double, BOBmax As Double, MinBedLevel As Double
        Dim upDist As Double, dnDist As Double

        Dim myProfUp As clsSbkReachObject
        Dim myProfDatUp As clsProfileDatRecord = Nothing
        Dim myProfDefUp As clsProfileDefRecord = Nothing
        Dim myProfDown As clsSbkReachObject
        Dim myProfDatDown As clsProfileDatRecord
        Dim myProfDefDown As clsProfileDefRecord
        Dim myStructDat As clsStructDatRecord
        Dim myStructDef As clsStructDefRecord
        Dim myStructProfDef As clsProfileDefRecord

        For Each myReach In SbkCase.CFTopo.Reaches.Reaches.Values
            For Each myObj In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFCulvert Then
                    myStructDat = SbkCase.CFData.Data.StructureData.StructDatRecords.Records(myObj.ID.Trim.ToUpper)
                    myStructDef = SbkCase.CFData.Data.StructureData.StructDefRecords.Records(myStructDat.dd.Trim.ToUpper)
                    myStructProfDef = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.Records(myStructDef.si.Trim.ToUpper)
                    BOBup = myStructDef.ll
                    BOBdown = myStructDef.rl
                    BOBmax = Math.Max(BOBup, BOBdown)
                    myReach = SbkCase.CFTopo.Reaches.GetReach(myObj.ci)

                    'zoek het bovenstroomse en het benedenstroomse profiel
                    Dim ReachesUpProcessed As New Dictionary(Of String, clsSbkReach)
                    Dim ReachesDnProcessed As New Dictionary(Of String, clsSbkReach)
                    myProfUp = myReach.GetUpstreamObject(ReachesUpProcessed, myObj.lc, upDist, False, True, False, False)
                    myProfDown = myReach.GetDownstreamObject(ReachesDnProcessed, myObj.lc, dnDist, False, True, False, False)

                    If myProfUp Is Nothing And myProfDown Is Nothing Then
                        'de tak heeft geen profiel
                        setup.Log.AddWarning("Reach " & myObj.ci & " has no cross section. Could not determine ground layer.")
                    Else
                        'v1.74: added error checking. Now the routine is only executed if the objects are not nothing
                        If myProfUp Is Nothing Then
                            'neem het laagste niveau van het benedenstroomse profiel
                            'bereken het laagste bodemniveau van het benedenstroomse profiel
                            myProfDatDown = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(myProfDown.ID.ToUpper)
                            If Not myProfDatDown Is Nothing Then
                                myProfDefDown = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.GetRecord(myProfDatDown.di)
                                If Not myProfDefDown Is Nothing Then
                                    myProfDatDown.CalculateLowestBedLevel(myProfDefDown)
                                    MinBedLevel = myProfDatDown.CalculateLowestBedLevel(myProfDefDown)
                                End If
                            End If
                        ElseIf myProfDown Is Nothing Then
                            'neem het laagste niveau van het bovenstroomse profiel
                            'bereken het laagste bodemniveau van het bovenstroomse profiel
                            myProfDatUp = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(myProfUp.ID.ToUpper)
                            If Not myProfDatUp Is Nothing Then
                                myProfDefUp = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.GetRecord(myProfDatUp.di)
                                If Not myProfDefUp Is Nothing Then
                                    myProfDatUp.CalculateLowestBedLevel(myProfDefUp)
                                    MinBedLevel = myProfDatUp.CalculateLowestBedLevel(myProfDefUp)
                                End If
                            End If
                        Else  'interpoleer
                            'bereken het laagste bodemniveau van het bovenstroomse profiel
                            myProfDatUp = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(myProfUp.ID.ToUpper)
                            If Not myProfDatUp Is Nothing Then
                                myProfDefUp = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.GetRecord(myProfDatUp.di)
                                If Not myProfDefUp Is Nothing Then
                                    myProfDatUp.CalculateLowestBedLevel(myProfDefUp)
                                End If
                            End If

                            'bereken het laagste bodemniveau van het benedenstroomse profiel
                            myProfDatDown = SbkCase.CFData.Data.ProfileData.ProfileDatRecords.Records(myProfDown.ID.ToUpper)
                            If Not myProfDatDown Is Nothing Then
                                myProfDefDown = SbkCase.CFData.Data.ProfileData.ProfileDefRecords.GetRecord(myProfDatDown.di)
                                If Not myProfDefDown Is Nothing Then
                                    myProfDatDown.CalculateLowestBedLevel(myProfDefDown)
                                End If
                            End If
                            If Not myProfUp Is Nothing AndAlso Not myProfDatUp Is Nothing AndAlso Not myProfDown Is Nothing AndAlso Not myProfDatDown Is Nothing AndAlso Not myProfDefDown Is Nothing AndAlso Not myProfDefUp Is Nothing Then
                                MinBedLevel = setup.GeneralFunctions.Interpolate(myProfUp.lc, myProfDatUp.CalculateLowestBedLevel(myProfDefUp), myProfDown.lc, myProfDatDown.CalculateLowestBedLevel(myProfDefDown), myObj.lc)
                            End If
                        End If

                        myStructProfDef.gl = Math.Round(Math.Max(MinBedLevel - BOBmax, 0), 2)
                        'zorg dat de sliblaag nooit groter is dan de opening van de duiker zelf
                        If myStructProfDef.ty = 4 Then myStructProfDef.gl = Math.Min(myStructProfDef.gl, (myStructProfDef.rd * 2) - 0.05)
                        If myStructProfDef.gl > 0 Then myStructProfDef.gu = 1
                    End If
                End If

            Next
        Next

    End Sub

    'friend Sub calcGroundLayer(ByRef Setup As clsSetup)
    '  'doorloop alle reachobjects
    '  For Each myObj As clsSbkReachObject In 

    '    For Each myDat As clsStructDatRecord In StructDatRecords.Records

    '    Next
    'End Sub
End Class
