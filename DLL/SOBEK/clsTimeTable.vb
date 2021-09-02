Imports STOCHLIB.General
Imports GemBox.Spreadsheet

Public Class clsTimeTable
    'Author: Siebe Bosch
    'Date: 27-6-2013
    'Description: a blistering fast class for storing temporal data
    Public ID As String
    Public Records As New Dictionary(Of DateTime, clsTimeTableRecord)
    Public AnnualSummary As New Dictionary(Of Integer, Double)

    Public FieldNames As New List(Of String)
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Function getnFields() As Integer
        Return Records.Values(0).Fields.Count
    End Function

    Public Sub IdentifyPOTEvents(ByVal DurationHours As Integer, ByVal nExceedancesPerYear As Integer, ByVal DataFieldIdx As Integer, ByVal ResultsFieldIdx As Integer, ByVal SumFieldIdx As Integer)
        'this routine identifies Peak Over Threshold events in a time series and writes the event index numbers to a specified results field

        'first establish the total TimeSpan
        Dim FirstRecord As clsTimeTableRecord = Records.Values(0)
        Dim SecondRecord As clsTimeTableRecord = Records.Values(1)
        Dim LastRecord As clsTimeTableRecord = Records.Values(Records.Values.Count - 1)
        Dim mySpan As TimeSpan = LastRecord.Datum.Subtract(FirstRecord.Datum)
        Dim myStep As TimeSpan = SecondRecord.Datum.Subtract(FirstRecord.Datum)
        Dim TotalYears As Long = mySpan.TotalDays / 365.25
        Dim StepHours As Long = myStep.TotalHours
        Dim i As Long, j As Long
        Dim mySum As Double

        Dim Duration As Integer = DurationHours / StepHours
        Dim TargetExceedances As Integer = TotalYears * nExceedancesPerYear

        'first compute the sum of each duration and write it to the specified field
        For i = 0 To Records.Values.Count - Duration
            mySum = 0
            For j = i To i + Duration - 1
                mySum += Records.Values(j).GetValue(DataFieldIdx)
            Next
            Records.Values(i).SetValue(SumFieldIdx, mySum)
        Next

        Stop

        'For i = 1 To TargetExceedances
        '  Me.Setup.GeneralFunctions.UpdateProgressBar("Identifying Peak Over Threshold values in timeseries...", i, TargetExceedances)
        '  MaxSum = 0
        '  For j = 0 To Records.Values.Count - 1 - Duration
        '    mySum = 0
        '    For k = j To j + Duration - 1
        '      mySum += Records.Values(j).GetValue(DataFieldIdx)
        '      idxSum += Records.Values(j).GetValue(ResultsFieldIdx)
        '    Next
        '    If mySum > MaxSum AndAlso idxSum = 0 Then
        '      MaxSum = mySum
        '      maxIdx = j
        '    End If
        '  Next

        '  For j = maxIdx To maxIdx + Duration - 1
        '    Records.Values(j).SetValue(ResultsFieldIdx, i)
        '  Next
        'Next

    End Sub


    Public Function calculateAnnualSummary(ByVal FieldIdx As Long, ByVal Opt As STOCHLIB.GeneralFunctions.enmHydroMathOperation, ByVal TimestepAsMultiplier As Boolean) As Boolean
        'This function creates an annual summary of the contents inf this timetable
        'optional are: annual sum of all values and multiplying the values with the timestep in seconds
        Dim myRecord As clsTimeTableRecord, myYear As Integer
        Dim ts As Integer = GetTimestepSeconds()

        Try
            For Each myRecord In Records.Values
                myYear = Year(myRecord.Datum)
                If Not AnnualSummary.ContainsKey(myYear) Then AnnualSummary.Add(myYear, 0)

                If Opt = GeneralFunctions.enmHydroMathOperation.SUM Then
                    If TimestepAsMultiplier Then
                        AnnualSummary.Item(myYear) += myRecord.GetValue(FieldIdx) * ts
                    Else
                        AnnualSummary.Item(myYear) += myRecord.GetValue(FieldIdx)
                    End If
                Else
                    Throw New Exception("Mathematical option " & Opt.ToString & " not (yet) supported for calculating annual summary of clsTimeTable.")
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in sub calculateAnnualSummary of class clsTimeTable.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Sub ToExcelSheet(ByVal SheetName As String)
        'writes the content of this timetable to a new Excel sheet without (yet) writing it to Excel
        'this allows for adding more data to the workbook if need be
        Dim ws As clsExcelSheet = Setup.ExcelFile.GetAddSheet(SheetName)
        Dim r As Long, c As Long, n As Long

        r = 0
        c = 0
        ws.ws.Cells(r, c).Value = ID

        For Each myPar As String In FieldNames
            c += 1
            ws.ws.Cells(r, c).Value = myPar
        Next

        Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting timetable " & ID & " to excel.", 0, 10, True)
        n = Records.Values.Count
        For Each record As clsTimeTableRecord In Records.Values
            r += 1
            c = 0
            ws.ws.Cells(r, c).Value = record.Datum
            Me.Setup.GeneralFunctions.UpdateProgressBar("", r, n)
            For Each myField As Double In record.Fields.Values
                c += 1
                ws.ws.Cells(r, c).Value = myField
            Next
        Next

    End Sub

    Public Function getMaxVal(Optional ByVal FieldIdx As Integer = -1, Optional ByRef RecordIdx As Integer = -1) As Double
        Dim myRecord As clsTimeTableRecord
        Dim myVal As Double, myMax As Double = -99999999
        Dim iRecord As Integer

        For iRecord = 0 To Records.Values.Count - 1
            myRecord = Records.Values(iRecord)
            If FieldIdx < 0 Then
                'walk through all fields
                For Each myVal In myRecord.Fields.Values
                    If myVal > myMax Then
                        myMax = myVal
                        RecordIdx = iRecord
                    End If
                Next
            Else
                myVal = myRecord.GetValue(FieldIdx)
                If myVal > myMax Then
                    myMax = myVal
                    RecordIdx = iRecord
                End If
            End If
        Next
        Return myMax

    End Function


    Public Function getMinVal(Optional ByVal FieldIdx As Integer = -1, Optional ByRef RecordIdx As Integer = -1) As Double
        Dim myRecord As clsTimeTableRecord
        Dim myVal As Double, myMin As Double = 99999999
        Dim iRecord As Integer

        For iRecord = 0 To Records.Values.Count - 1
            myRecord = Records.Values(iRecord)
            If FieldIdx < 0 Then
                'walk through all fields
                For Each myVal In myRecord.Fields.Values
                    If myVal < myMin Then
                        myMin = myVal
                        RecordIdx = iRecord
                    End If
                Next
            Else
                myVal = myRecord.GetValue(FieldIdx)
                If myVal < myMin Then
                    myMin = myVal
                    RecordIdx = iRecord
                End If
            End If
        Next
        Return myMin

    End Function

    Public Function GetTimestepSeconds() As Integer
        Dim FirstRecord As clsTimeTableRecord, SecondRecord As clsTimeTableRecord
        If Records.Count >= 2 Then
            FirstRecord = Records.Values(0)
            SecondRecord = Records.Values(1)
            Return (SecondRecord.Datum.ToOADate - FirstRecord.Datum.ToOADate) * 24 * 3600
        Else
            Return 0
        End If
    End Function

    Public Function GetAddRecord(ByVal myDate As DateTime) As clsTimeTableRecord
        Dim myRecord As clsTimeTableRecord

        If Records.ContainsKey(myDate) Then
            Return Records.Item(myDate)
        Else
            myRecord = New clsTimeTableRecord
            myRecord.Datum = myDate
            Records.Add(myDate, myRecord)
        End If

        Return myRecord

    End Function

    Public Function DischargeReductionByWaterConservationStructure(ByVal PercentageOpenWater As Double, ByVal MaxDH As Double, ByVal QOrificeMMPD As Double, ByVal PercentageAffected As Integer, ByVal Area As Double, ByVal SourceFieldIdx As Integer, ByVal ReturnIfOverflow As Boolean) As Boolean

        Dim myRecord As clsTimeTableRecord
        Dim FractionAffected As Double = PercentageAffected / 100
        Dim FractionOpenWater As Double = PercentageOpenWater / 100
        Dim OWArea As Double = Area * FractionAffected * FractionOpenWater
        Dim ts As Integer = GetTimestepSeconds()
        Dim MaxVol As Double = OWArea * MaxDH                                 'max extra storable volume of water behind the structure
        Dim StoredVol As Double = 0, UitOpening As Double = 0, UitOverstort As Double = 0
        Dim MaxQOut As Double = Me.Setup.GeneralFunctions.mmpd2m3ps(QOrificeMMPD, Area * FractionAffected)
        Dim MaxVolOut As Double = MaxQOut * ts
        Dim Q_mg As Double = Me.Setup.GeneralFunctions.mmpd2m3ps(12, Area * FractionAffected)

        'orifice and weir parameters. Mind you: exact elevations do not matter. It's essentially about dH
        Dim dH As Double, W_orifice As Double, gh As Double = 0.1, mu As Double = 0.63, cw_orifice As Double = 1, Z As Double = 0, H1 As Double = 0.3
        Dim W_weir As Double, ce_weir As Double = 1.5, cw_weir As Double = 0.7

        'Author: Siebe Bosch
        'Date: 25-8-2013
        'Description: adjusts a timetable with discharges in such a manner that the discharges are reduced similar
        'to the way this would happen with water conservation structures
        'note: the variable ReturnIfOverflow is a shortcut to check whether in the tested configuration overflows occur

        'fields:
        '0 = original timetable
        '1 = unaffected fraction
        '2 = total after conversion (= the final result)
        '3 = affected fraction after conversion
        '4 = baseflow through water conservation structure (= part of 3)
        '5 = overflow over water conservation structure (= part of 3)

        Try

            'first calculate the required width for the orifice. Assume a gate height of 10cm
            W_orifice = Me.Setup.GeneralFunctions.calcOrificeWidth(MaxQOut, mu, cw_orifice, Z, H1, gh)
            W_weir = Me.Setup.GeneralFunctions.calcWeirWidth(Q_mg, ce_weir, cw_weir, 0.05)

            'set the fieldnames for this timetable
            FieldNames = New List(Of String)
            FieldNames.Add("Sourcedata (m3/s)")
            FieldNames.Add("Unaffected Fraction (m3/s)")
            FieldNames.Add("Results total (m3/s)")
            FieldNames.Add("Results Affected (m3/s)")
            FieldNames.Add("Baseflow Affected (m3/s)")
            FieldNames.Add("Overflow Affected (m3/s)")

            'initialize all fields
            'first thing to do is take the percentage that is UNaffected and put those values inside the dedicated field
            For Each myRecord In Records.Values
                myRecord.SetValue(1, myRecord.GetValue(SourceFieldIdx) * (1 - FractionAffected))
                myRecord.SetValue(2, myRecord.GetValue(SourceFieldIdx) * (1 - FractionAffected))
                myRecord.SetValue(3, 0)                                                          'field for results affected fraction
                myRecord.SetValue(4, 0)                                                          'field for baseflow affected fraction
                myRecord.SetValue(5, 0)                                                          'field for overflow affected fraction
            Next

            For Each myRecord In Records.Values

                'initialize the volumes
                UitOpening = 0
                UitOverstort = 0
                StoredVol += myRecord.GetValue(SourceFieldIdx) * FractionAffected * ts           'geborgen volume neemt toe door inkomend debiet

                'note: we'll maximize dH to the maximum storage height. Otherwise we'll get timestep issues
                'where dH becomes extremely high, causing unrealistic discharges through the orifice
                dH = Math.Min(MaxDH * StoredVol / MaxVol, MaxDH)                                                        'hoogte h1 ten opzichte van onderkant orifice

                'first calculate the baseflow through the orifice
                If StoredVol > 0 Then
                    UitOpening = Me.Setup.GeneralFunctions.QORIFICE(0, W_orifice, gh, mu, cw_orifice, dH, 0) * ts   'calculate outflow with the orifice flow formula
                    If UitOpening > StoredVol Then UitOpening = StoredVol 'outflow cannot be larger than stored volume
                    StoredVol -= UitOpening                                                   'correct the stored volume
                End If

                'then calculate the overflow volume
                If StoredVol > MaxVol Then                                               'overstort!
                    dH = MaxDH * (StoredVol / MaxVol - 1)                                   'dikte overstortende straal
                    UitOverstort = Me.Setup.GeneralFunctions.QWEIR(W_weir, ce_weir, dH, 0, 0, cw_weir) * ts
                    If UitOverstort > (StoredVol - MaxVol) Then UitOverstort = StoredVol - MaxVol
                    StoredVol -= UitOverstort                                               'pas geborgen volume aan
                End If

                If ReturnIfOverflow = True AndAlso UitOverstort > 0 Then Return True

                'store the results1
                myRecord.AddToValue(2, (UitOpening + UitOverstort) / ts)                  'add to the total
                myRecord.SetValue(3, (UitOpening + UitOverstort) / ts)                    'total flow affected area
                myRecord.SetValue(4, UitOpening / ts)                                     'baseflow
                myRecord.SetValue(5, UitOverstort / ts)                                   'overflow
            Next

            If ReturnIfOverflow = True Then Return False 'if we end up here, no overflow has occurred, so return false

            Return True

        Catch ex As Exception

        End Try

    End Function

    Public Function DischargeReductionByWeirAdjustment(ByVal PercentageOpenWater As Double, ByVal MaxDH As Double, ByVal Timing As Integer, ByVal PercentageAffected As Integer, ByVal Area As Double, ByVal SourceFieldIdx As Integer) As Boolean

        Dim myRecord As clsTimeTableRecord
        Dim FractionAffected As Double = PercentageAffected / 100
        Dim FractionOpenWater As Double = PercentageOpenWater / 100
        Dim OWArea As Double = Area * FractionAffected * FractionOpenWater
        Dim ts As Integer = GetTimestepSeconds(), nHours As Long
        Dim MaxVol As Double = OWArea * MaxDH                                 'max extra storable volume of water behind the structure
        Dim StoredVol As Double = 0, UitOpening As Double = 0, UitOverstort As Double = 0
        Dim Q_mg As Double = Me.Setup.GeneralFunctions.mmpd2m3ps(12, Area * FractionAffected)

        'orifice and weir parameters. Mind you: exact elevations do not matter. It's essentially about dH
        Dim dH As Double, gh As Double = 0.1, mu As Double = 0.63, cw_orifice As Double = 1, Z As Double = 0, H1 As Double = 0.3
        Dim W_weir As Double, ce_weir As Double = 1.5, cw_weir As Double = 0.7

        'Author: Siebe Bosch
        'Date: 25-8-2013
        'Description: adjusts a timetable with discharges in such a manner that the discharges are reduced similar
        'to the way this would happen with water conservation structures

        'fields:
        '0 = original timetable
        '1 = unaffected fraction
        '2 = total after conversion (= the final result)
        '3 = affected fraction after conversion
        '4 = baseflow through water conservation structure (= part of 3)
        '5 = overflow over water conservation structure (= part of 3)

        Try

            'first calculate the required width for the orifice. Assume a gate height of 10cm
            W_weir = Me.Setup.GeneralFunctions.calcWeirWidth(Q_mg, ce_weir, cw_weir, 0.05)

            'set the fieldnames for this timetable
            FieldNames = New List(Of String)
            FieldNames.Add("Sourcedata (m3/s)")
            FieldNames.Add("Unaffected Fraction (m3/s)")
            FieldNames.Add("Results total (m3/s)")
            FieldNames.Add("Results Affected (m3/s)")
            FieldNames.Add("Baseflow Affected (m3/s)")
            FieldNames.Add("Overflow Affected (m3/s)")

            'initialize all values in the table
            'first thing to do is take the percentage that is UNaffected and put those values inside the dedicated field
            For Each myRecord In Records.Values
                myRecord.SetValue(1, myRecord.GetValue(SourceFieldIdx) * (1 - FractionAffected)) 'field for unaffected fraction
                myRecord.SetValue(2, myRecord.GetValue(SourceFieldIdx) * (1 - FractionAffected)) 'field for results total
                myRecord.SetValue(3, 0)                                                          'field for results affected fraction
                myRecord.SetValue(4, 0)                                                          'field for baseflow affected fraction
                myRecord.SetValue(5, 0)                                                          'field for overflow affected fraction
            Next

            nHours = -ts / 3600                                                         'initialize the timstep in hours
            For Each myRecord In Records.Values
                nHours += ts / 3600

                'initialize the volumes
                UitOpening = 0
                UitOverstort = 0
                StoredVol += myRecord.GetValue(SourceFieldIdx) * FractionAffected * ts           'geborgen volume neemt toe door inkomend debiet

                If nHours < Timing Then
                    dH = StoredVol / (Area * FractionOpenWater * FractionAffected)                                                        'hoogte h1 ten opzichte van onderkant orifice
                    UitOverstort = Me.Setup.GeneralFunctions.QWEIR(W_weir, ce_weir, dH, 0, 0, cw_weir) * ts
                    If UitOverstort > StoredVol Then UitOverstort = StoredVol
                    StoredVol -= UitOverstort
                Else
                    dH = MaxDH * (StoredVol / MaxVol - 1)                                   'dikte overstortende straal
                    If StoredVol > MaxVol Then
                        UitOverstort = Me.Setup.GeneralFunctions.QWEIR(W_weir, ce_weir, dH, 0, 0, cw_weir) * ts
                        If UitOverstort > (StoredVol - MaxVol) Then UitOverstort = (StoredVol - MaxVol)
                        StoredVol -= UitOverstort
                    Else
                        UitOverstort = 0
                    End If
                End If

                'store the results1
                myRecord.AddToValue(2, (UitOpening + UitOverstort) / ts)                  'add to the total
                myRecord.SetValue(3, (UitOpening + UitOverstort) / ts)                    'total flow affected area
                myRecord.SetValue(4, UitOpening / ts)                                     'baseflow
                myRecord.SetValue(5, UitOverstort / ts)                                   'overflow

            Next

            Return True
        Catch ex As Exception

        End Try


    End Function

    Public Function OptimizeWaterConservationStructure(ByVal PercentageOpenWater As Double, ByVal MaxDH As Double, ByVal Area As Double, ByVal FieldIdx As Integer, ByVal PercentageAffected As Integer) As Double
        Dim PeakIdx As Long, MaxFlow As Double = 0, MaxFlowMMPD As Double = 0, BaseFlow As Double, LBoundQ As Double, UBoundQ As Double
        Dim i As Long, myRecord As clsTimeTableRecord
        Dim MaxStor As Double = MaxDH * PercentageOpenWater / 100 * Area
        Dim Done As Boolean
        'Author: Siebe Bosch
        'Date: 26-8-2013
        'Description: calculates the optimal value (mm/d) for the base discharge of a water conservation structure
        'In order to do this it investigates the discharge table and tries to maximize storage during the peak and prevent overflow

        'first find the peakflow
        For i = 0 To Records.Count - 1
            myRecord = Records.Values(i)
            If myRecord.Fields(FieldIdx) > MaxFlow Then
                PeakIdx = i
                MaxFlow = myRecord.Fields(FieldIdx)
            End If
        Next

        MaxFlowMMPD = Me.Setup.GeneralFunctions.m3ps2mmpd(MaxFlow, Area)

        'in 10 steps, iterate to the baseflow where overflow only just starts
        i = 0
        LBoundQ = 0
        UBoundQ = MaxFlowMMPD
        While Not Done
            i += 1
            BaseFlow = (UBoundQ + LBoundQ) / 2
            If DischargeReductionByWaterConservationStructure(PercentageOpenWater, MaxDH, BaseFlow, PercentageAffected, Area, 0, True) Then
                'we have overflow, so run again with a higher baseflow: the center between baseflow and uboundq
                LBoundQ = BaseFlow
            Else
                'we have no overflow, so run again with a lower baseflow: the center between lboundq and baseflow
                UBoundQ = BaseFlow
            End If
            If i = 10 Then Done = True
        End While

        Return BaseFlow

    End Function

    Public Function OptimizeWeirAdjustmentForStorage(ByVal PercentageOpenWater As Double, ByVal MaxDH As Double, ByVal Area As Double, ByVal FieldIdx As Integer) As Double
        Dim PeakIdx As Long, MaxFlow As Double
        Dim i As Long, myRecord As clsTimeTableRecord
        Dim mySum As Double, ts As Long
        'Author: Siebe Bosch
        'Date: 26-8-2013
        'Description: calculates the optimal timing for raising a weir to reduce peak discharge
        'In order to do this it assumes a symmetrical peak and tries to store the 50% of water before and the 50% after the peak
        'the function returns a number that reprents the number of hours between start of the event and the moment the weir needs to be raised

        'first find the moment the peakflow occurs
        For i = 0 To Records.Count - 1
            myRecord = Records.Values(i)
            If myRecord.Fields(FieldIdx) > MaxFlow Then
                PeakIdx = i
                MaxFlow = myRecord.Fields(FieldIdx)
            End If
        Next

        'determine the timestep size in seconds
        ts = (Records.Values(1).Datum - Records.Values(0).Datum).TotalSeconds

        'now walk backwards in time until we have a discharge sum that's 50% of the storage volume
        mySum = 0
        For i = PeakIdx - 1 To 0 Step -1
            myRecord = Records.Values(i)
            mySum += ts * myRecord.GetValue(FieldIdx) 'calculates the m3 cumulative backwards from the peak
            If mySum > (MaxDH * Area * PercentageOpenWater / 100) / 2 Then
                Return (Records.Values(i + 1).Datum - Records.Values(0).Datum).TotalHours
            End If
        Next

        'not found, so return 0
        Return 0

    End Function

End Class
