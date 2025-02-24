﻿Imports STOCHLIB.General
Imports MathNet.Numerics.LinearAlgebra
Imports alglib
Imports DocumentFormat.OpenXml.Bibliography
Public Class clsGebiedsreductie

    Private Setup As clsSetup

    'first set our GEV-parameters as three 2D-arrays. Notice that VB.NET uses 0-based arrays, so we have to subtract 1 from the indices from the R-script

    Dim loc(,) As Double = {{7.99601641257175, 10.4546551438572, 13.218049837703, 16.7704942642987, 20.5924110465457, 25.4984238527503, 28.5751072514721, 34.136582266701, 41.9004197187678, 51.5226619550072, 70.1872119340305, 74.1239158516677}, {6.61733409113894, 9.33020236014733, 12.3188911160318, 16.0368037795387, 19.9950944517894, 24.9376842328987, 28.0414864406803, 33.5992754331044, 41.4011403042835, 51.0704987778862, 69.7420298803996, 73.7329050681309}, {5.55435065517195, 8.3733217409595, 11.5328027927596, 15.381186895456, 19.433331900897, 24.4390612761673, 27.570183546931, 33.0760957702847, 40.8747607670408, 50.5668568967305, 69.2493618437318, 73.2583441622298}, {4.76785769613192, 7.60102463965611, 10.8701551239488, 14.8237031567731, 18.9377349488511, 24.0217782949645, 27.1546516004664, 32.5960880037195, 40.3617920247273, 50.074362055797, 68.7564890232306, 72.75705086347}, {4.16597491995785, 6.95852398653494, 10.2973312132505, 14.3451049633338, 18.5066339713206, 23.6395489925322, 26.75410806539, 32.1209622260836, 39.8917306628043, 49.6046604727027, 68.2619561793752, 72.2410172767708}, {3.70149950934942, 6.4039461161634, 9.80095639203887, 13.9223073450949, 18.1030219173478, 23.2729032573903, 26.3717275851971, 31.671209684885, 39.4480199592926, 49.1433740461701, 67.7185375418625, 71.6913458109891}, {3.34057373946442, 5.91926666648388, 9.34720017624355, 13.539268902388, 17.738765450445, 22.9312157399957, 26.0244143114607, 31.2787050596421, 39.0678101926059, 48.7405579514312, 67.217164238321, 71.1933316799655}, {3.05698484788124, 5.50482619119558, 8.94202829167071, 13.1932505021018, 17.4354020534288, 22.6262837043886, 25.7333904730299, 30.969570958488, 38.7773123038217, 48.4306594877267, 66.8679554043205, 70.8448529796266}, {2.82968125249177, 5.15181217792647, 8.57002673754884, 12.8705996322783, 17.1621611476945, 22.3407249837604, 25.4691444922108, 30.696169180035, 38.5264397374482, 48.141460564565, 66.5753674772188, 70.5565217418994}}
    Dim disp(,) As Double = {{0.345459609984356, 0.338237399103431, 0.316527623047147, 0.292683856304298, 0.273591890808796, 0.260335369980682, 0.256515651793413, 0.248704701270904, 0.239695954975164, 0.241072741574144, 0.219633773347014, 0.217471054440495}, {0.329455055940785, 0.319417644779727, 0.298607775060746, 0.279128461179457, 0.263850146597729, 0.250695722298117, 0.24690823760194, 0.241418377500722, 0.234040373403165, 0.23698220515843, 0.215274541730967, 0.213011187817643}, {0.321558241203672, 0.306200951556576, 0.28324654124055, 0.269301971608663, 0.256583936221282, 0.243293446714805, 0.239318041950248, 0.235156568753082, 0.228243072166741, 0.232943389041207, 0.211306684872056, 0.209399757780673}, {0.311661795673261, 0.295998175660371, 0.270597117385042, 0.261295821296992, 0.251113956618164, 0.237466675656844, 0.233262925978768, 0.22980174168234, 0.222808559241149, 0.22915303796985, 0.206964361764742, 0.205572442355971}, {0.299561270948559, 0.284698000407061, 0.259571397030302, 0.254510267641862, 0.246332825137254, 0.230953259016275, 0.226908521414953, 0.225125738365461, 0.218959533966523, 0.226407901290734, 0.202634834409498, 0.201738982863439}, {0.288240287658128, 0.273839024393026, 0.249039934010838, 0.248110446555378, 0.241677253528045, 0.225017279915954, 0.221013668747786, 0.222185305956247, 0.215601551791201, 0.224292695801226, 0.198098652231839, 0.197562293106601}, {0.27942185289408, 0.265511645134226, 0.239889910739321, 0.241396480378367, 0.237131044223392, 0.219054883339077, 0.216173204247784, 0.22041794836498, 0.212475214580748, 0.222371900519736, 0.193768470351555, 0.193409354051288}, {0.27346937519929, 0.259725239615703, 0.233613464525722, 0.235093017433718, 0.232891079197069, 0.214354127220826, 0.212831506690783, 0.219454547427255, 0.209859439184168, 0.220133490333087, 0.18987284456655, 0.190005552014752}, {0.26893456758941, 0.25530887387054, 0.229662731254204, 0.228581723177871, 0.227517191513201, 0.209520804742285, 0.209854314580711, 0.218824835970191, 0.207006925053442, 0.216793034816539, 0.185722674645976, 0.186934914735874}}
    Dim kappa(,) As Double = {{-0.0990140162497294, -0.133557989496117, -0.169686674919718, -0.176262468754872, -0.175338440062201, -0.188177488285709, -0.191121620622388, -0.190316759742033, -0.16581365221165, -0.118373844938239, -0.060505168263816, -0.0512522408942082}, {-0.0725296789340225, -0.0956040791114591, -0.142810043113753, -0.162617080697623, -0.169778160870852, -0.190904779857454, -0.195092766324782, -0.192502372266216, -0.16266779043278, -0.112543981283232, -0.0546804052018978, -0.0458452704818572}, {-0.0670775491160267, -0.0663877643646879, -0.117942886440903, -0.147080445986701, -0.163822355951894, -0.187364433003022, -0.191492342802146, -0.194549482350984, -0.162862893446567, -0.110052744226073, -0.0513837362529452, -0.0415688112727007}, {-0.0706138017798152, -0.0464676578408127, -0.0996642626564461, -0.131448473224354, -0.159646064229244, -0.177712506948985, -0.18473148862316, -0.197776422041316, -0.168583707983155, -0.110412309798056, -0.0498597169175864, -0.0389557533067463}, {-0.0835571279541525, -0.0410218822608488, -0.085529043377726, -0.110252512485808, -0.151064850033211, -0.167341719630605, -0.179518976394131, -0.201510106651065, -0.171198675915258, -0.108175486217544, -0.048495637746028, -0.0366930817566132}, {-0.0999319728387095, -0.0496148614421001, -0.0741324815424861, -0.0866633366707748, -0.143998686682755, -0.155354592677637, -0.174391902624955, -0.20062059486348, -0.174393146965155, -0.106691835063473, -0.0513958478694518, -0.0378007973080677}, {-0.110177688433509, -0.0618518454390864, -0.0670411710749044, -0.0630680537980209, -0.133265160323314, -0.141234654771305, -0.164196101952651, -0.193381063023804, -0.175302668486457, -0.104118385624191, -0.0572061442164475, -0.0412919325797747}, {-0.113226502553864, -0.0703468304725661, -0.0570674742837164, -0.03755717414805, -0.11648130087866, -0.125890075342847, -0.150030730964239, -0.183561465189088, -0.173779622424461, -0.102712102860235, -0.0646012036002722, -0.045498437512225}, {-0.110952299091445, -0.0746721453601331, -0.0429331788392205, -0.0119834876493325, -0.0994474016655955, -0.112837615794725, -0.135688588347242, -0.173429803570496, -0.173286380740916, -0.106785855808423, -0.074572630413224, -0.0499499102107633}}
    Dim duur() As Double = {0.25, 0.5, 1, 2, 4, 8, 12, 24, 48, 96, 192, 216} '12 durations: columns of the arrays above
    Dim Area() As Double = {5.73, 51.57, 143.25, 280.77, 464.13, 693.33, 968.37, 1289.25, 1655.97} '9 areas: rows of the arrays above

    Dim x_series() As Double = {0.25, 0.5, 1, 2, 4, 8, 12, 24, 48, 96, 192, 216}
    Dim A_series() As Double = {10}
    Dim T_series_Langbein() As Double = {0.5, 1, 2, 5, 10, 20, 25, 30, 50, 100}
    Dim Aref As Double = 5.73
    Dim k As Integer = 0
    Dim lijst_Duur() As Double
    Dim lijst_Area() As Double
    Dim lijst_loc() As Double
    Dim lijst_disp() As Double
    Dim lijst_kappa() As Double

    Private _fitResults As FitResults  ' Add as class member

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub


    ' Function to round numbers to integers
    Public Function RoundUp(ByVal x As Double) As Integer
        Return CInt(System.Math.Truncate(x + 0.5))
    End Function

    Public Function Main() As Boolean
        'this is a copy of the R-script to compute the area reduction factor

        ' First count how many elements we'll need
        Dim elementCount As Integer = 0
        For z As Integer = 1 To 9
            For i As Integer = 1 To 12
                Dim isDurationValid = duur(i - 1) <> 0.25
                Dim isAreaValidFor15min = (duur(i - 1) = 0.25 AndAlso Area(z - 1) <> 1289.25 AndAlso Area(z - 1) <> 1655.97)
                If isDurationValid OrElse isAreaValidFor15min Then
                    elementCount += 1
                End If
            Next
        Next

        ' Initialize arrays with the correct size
        ReDim lijst_Area(elementCount - 1)
        ReDim lijst_Duur(elementCount - 1)
        ReDim lijst_loc(elementCount - 1)
        ReDim lijst_disp(elementCount - 1)
        ReDim lijst_kappa(elementCount - 1)

        k = 0
        For areaidx As Integer = 0 To 8
            For duridx As Integer = 0 To 11
                If (duur(duridx) <> 0.25 OrElse (duur(duridx) = 0.25 AndAlso (Area(areaidx) <> 1289.25 AndAlso Area(areaidx) <> 1655.97))) Then
                    lijst_Area(k) = Area(areaidx)
                    lijst_Duur(k) = duur(duridx)
                    lijst_loc(k) = loc(areaidx, duridx)
                    lijst_disp(k) = disp(areaidx, duridx)
                    lijst_kappa(k) = kappa(areaidx, duridx)
                    k += 1
                End If
            Next
        Next

        ' For location parameter (radar statistic)
        Dim C As Double = 82
        Dim lijst_Duur2() As Double = lijst_Duur.Clone()  ' Create a copy of lijst_Duur
        Dim lijst_Duur3() As Double = lijst_Duur.Clone()

        ' Replace values > C with C in lijst_Duur2
        For i As Integer = 0 To lijst_Duur2.Length - 1
            If lijst_Duur2(i) > C Then
                lijst_Duur2(i) = C
            End If
        Next

        ' Replace values <= C with C in lijst_Duur3
        For i As Integer = 0 To lijst_Duur3.Length - 1
            If lijst_Duur3(i) <= C Then
                lijst_Duur3(i) = C
            End If
        Next


        Debug.Print("lijst_Duur before fit: " & String.Join(", ", lijst_Duur))
        Debug.Print("lijst_Area before fit: " & String.Join(", ", lijst_Area))
        Debug.Print("lijst_loc before fit: " & String.Join(", ", lijst_loc))
        Debug.Print("lijst_kappa before fit: " & String.Join(", ", lijst_kappa))
        Debug.Print("lijst_disp before fit: " & String.Join(", ", lijst_disp))


        ' Create instance of fitter and perform the fits
        Dim fitter As New RadarStatisticsFitter(Me.Setup)
        _fitResults = fitter.FitAllParameters(lijst_Duur, lijst_Area, lijst_loc, lijst_kappa, lijst_disp)

        ' Access results
        Dim locCoefs = _fitResults.LocationParameters
        Dim shapeCoefs = _fitResults.ShapeParameters
        Dim dispCoefs = _fitResults.DispersionParameters

        Debug.Print("loc_coefs after fit: " & String.Join(", ", locCoefs))
        Debug.Print("shape_coefs after fit: " & String.Join(", ", shapeCoefs))
        Debug.Print("disp_coefs after fit: " & String.Join(", ", dispCoefs))

        ' Then process the statistics for each area
        Dim processor As New RadarStatisticsProcessor()


        ' You should process all areas in A_series
        For Each myArea In A_series
            Dim areaResults = processor.ProcessAreaStatistics(
                area:=myArea,
                T_series_Langbein:=T_series_Langbein,
                x_series:=x_series,
                fittedParams:=_fitResults)

            ' Access results
            Dim resultMatrix = areaResults.ResultMatrix
            Dim stowaResults = areaResults.STOWA2019_5_73km2

        Next


    End Function


    ' Function to compute Areal Reduction Factor for a given surface area, duration and return period
    Public Function ComputeARF(surfaceAreaKm2 As Double, durationHours As Double, returnPeriod As Double) As Double
        If _fitResults Is Nothing Then
            ' Need to run Main first to get the fits
            Main()
        End If

        Dim processor As New RadarStatisticsProcessor()
        Dim result = processor.ProcessAreaStatistics(
            area:=surfaceAreaKm2,
            T_series_Langbein:=New Double() {returnPeriod},
            x_series:=New Double() {durationHours},
            fittedParams:=_fitResults, Aref:=Aref)

        Return result.ResultMatrix(0, 0)
    End Function


    Public Function CalculateByArea(Season As String, Pattern As String, volume As Double, durationMinutes As Integer, areaKm2 As Double) As (Boolean, Double)
        Try
            Dim T As Double = Me.Setup.Neerslagstatistiek.STOWA2024_JAARROND_T(durationMinutes, volume, 2014, "", "")

            ' Determine which set of parameters to use based on duration
            Dim ARF As Double = ComputeARF(areaKm2, durationMinutes / 60, T)
            Me.Setup.Log.AddMessage($"Reduction factor {ARF} computed for duration {durationMinutes / 60}H at season {Season}, pattern {Pattern}, volume {volume} and area {areaKm2} km2.")
            Return (True, ARF)

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalculateByArea of class clsGebiedsreductie: " & ex.Message)
            Return (False, 0)
        End Try

    End Function


    Public Function CalculateByAreaAdvanced(Season As String, Pattern As String, volume As Double, fractie As Double(), durationMinutes As Integer, areaKm2 As Double) As (Boolean, List(Of Double))
        Try
            ' Determine which set of parameters to use based on duration
            Dim Result As New List(Of Double)


            'first decide all the inlying durations
            Dim durations As New List(Of Integer)
            'If durationMinutes <= 15 Then durations.Add(15)              '15 minutes. skip this because we're working with 1 hour timesteps
            'If durationMinutes <= 30 Then durations.Add(30)              '30 minutes. skip this because we're working with 1 hour timesteps
            If durationMinutes >= 60 Then durations.Add(1)                '1 hour
            If durationMinutes >= 120 Then durations.Add(2)               '2 hours
            If durationMinutes >= 240 Then durations.Add(4)               '4 hours
            If durationMinutes >= 480 Then durations.Add(8)               '8 hours
            If durationMinutes >= 720 Then durations.Add(12)              '12 hours
            If durationMinutes >= 1440 Then durations.Add(24)             '24 hours
            If durationMinutes >= 2880 Then durations.Add(48)             '2 days
            If durationMinutes >= 5760 Then durations.Add(96)             '4 days
            If durationMinutes >= 11520 Then durations.Add(192)           '8 days
            If durationMinutes >= 12960 Then durations.Add(216)           '9 days

            'now calculate the highest return period for each duration
            Dim maxDur As Integer
            Dim maxDurStartIdx As Integer 'the start index of the duration in the fractie array
            Dim MaxT As Double = 0 'maximum return period
            Dim curVol As Double, curmaxVol As Double, curStartIdx As Integer, curT As Double

            'walk through all durations and calculate which one yields the highest return period
            For Each dur As Integer In durations
                curmaxVol = 0
                For startIdx = 0 To fractie.Length - dur
                    'create a window of the given duration and walk through the fractie array to calculate curVol
                    curVol = 0
                    For i = startIdx To startIdx + dur - 1
                        curVol += fractie(i) * volume
                    Next

                    'if this is the highest volume so far, store it. And also store the start index
                    If curVol > curmaxVol Then
                        curmaxVol = curVol
                        curStartIdx = startIdx
                    End If

                Next

                curT = Me.Setup.Neerslagstatistiek.STOWA2024_JAARROND_T(dur * 60, curmaxVol, 2014, "", "")

                'if this is the highest return period so far, store it
                If curT > MaxT Then
                    MaxT = curT
                    maxDur = dur
                    maxDurStartIdx = curStartIdx
                End If
            Next

            'now that we found our critical duration and the startIdx we can calculate the reduction factor and apply it
            '2025-02-1: added a check for MaxT < 0.5 because the ARF is not defined for return periods < 0.5
            Dim ARF As Double = If(MaxT < 0.5, 1, ComputeARF(areaKm2, maxDur, MaxT))

            If ARF > 1 Then
                Me.Setup.Log.AddWarning($"CalculateByAreaAdvanced computed an Areal Reduction Factor > 1 for area {areaKm2}, duration {maxDur} and return period {MaxT}.")
            End If

            'finally implement the reduction factor in the section of our fractie array
            For i = 0 To fractie.Count - 1
                If i < maxDurStartIdx Then
                    Result.Add(volume * fractie(i)) 'no adjustment
                ElseIf i < maxDurStartIdx + maxDur Then 'within the subwindow adjust the volumes by applying the ARF
                    Result.Add(volume * fractie(i) * ARF)
                Else
                    Result.Add(volume * fractie(i)) 'no adjustment
                End If
            Next

            Me.Setup.Log.AddMessage($"Reduction factor {ARF} computed for inlying duration {maxDur}H of {durationMinutes / 60}H at season {Season}, pattern {Pattern}, volume {volume} and area {areaKm2} km2.")
            Return (True, Result)

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in CalculateByAreaAdvanced: " & ex.Message)
            Return (False, Nothing)
        End Try


    End Function



End Class

''Public Class clsAreaReductionTable
''    Friend years As Integer
''    Friend locpar_LM As Double
''    Friend sclpar_LM As Double
''    Friend k_LM As Double
''    Friend cV_LM As Double

''    Public Sub New(iYears As Integer, iLocpar_LM As Double, iSclPar_LM As Double, ik_LM As Double, icV_LM As Double)
''        years = iYears
''        locpar_LM = iLocpar_LM
''        sclpar_LM = iSclPar_LM
''        k_LM = ik_LM
''        cV_LM = icV_LM
''    End Sub
''End Class

'Public Class clsGebiedsreductie
'    Private Setup As clsSetup
'    Dim Pars As Dictionary(Of (Integer, Integer), clsAreaReductionTable) 'key: first item: duration (minutes) and second item: number of pixels (area)

'    Public Sub New(ByRef Setup As clsSetup)
'        Me.Setup = Setup
'        Pars = New Dictionary(Of (Integer, Integer), clsAreaReductionTable)
'        'first we store the tables as provided by Aart Overeem (KNMI)
'        'first item of the key = duration (minutes) and second item = number of pixels (area)

'        'important: the second item in the key is not the area in km2 but the number of pixels in the radar grid cell size (5.73 km2)

'        '15 minutes
'        Pars.Add((15, 1), New clsAreaReductionTable(117610, 7.99601641257175, 2.76230071131555, -0.0990140162497294, 0.345459609984356))
'        Pars.Add((15, 9), New clsAreaReductionTable(96957, 6.61733409113894, 2.18011417317504, -0.0725296789340225, 0.329455055940785))
'        Pars.Add((15, 25), New clsAreaReductionTable(81434, 5.55435065517195, 1.78604722770556, -0.0670775491160267, 0.321558241203672))
'        Pars.Add((15, 49), New clsAreaReductionTable(69084, 4.76785769613192, 1.48595909109105, -0.0706138017798152, 0.311661795673261))
'        Pars.Add((15, 81), New clsAreaReductionTable(59071, 4.16597491995785, 1.2479647417624, -0.0835571279541525, 0.299561270948559))
'        Pars.Add((15, 121), New clsAreaReductionTable(50084, 3.70149950934942, 1.0669212833413, -0.0999319728387095, 0.288240287658128))
'        Pars.Add((15, 169), New clsAreaReductionTable(42047, 3.34057373946442, 0.933429304010456, -0.110177688433509, 0.27942185289408))
'        Pars.Add((15, 225), New clsAreaReductionTable(34884, 3.05698484788124, 0.835991736343779, -0.113226502553864, 0.27346937519929))
'        Pars.Add((15, 289), New clsAreaReductionTable(28063, 2.82968125249177, 0.760999104054734, -0.110952299091445, 0.26893456758941))

'        '30 minutes
'        Pars.Add((30, 1), New clsAreaReductionTable(117610, 10.4546551438572, 3.53615536438155, -0.133557989496117, 0.338237399103431))
'        Pars.Add((30, 9), New clsAreaReductionTable(96957, 9.33020236014733, 2.98023126319651, -0.0956040791114591, 0.319417644779727))
'        Pars.Add((30, 25), New clsAreaReductionTable(81434, 8.3733217409595, 2.56391908477116, -0.0663877643646879, 0.306200951556576))
'        Pars.Add((30, 49), New clsAreaReductionTable(69084, 7.60102463965611, 2.24988942648773, -0.0464676578408127, 0.295998175660371))
'        Pars.Add((30, 81), New clsAreaReductionTable(59071, 6.95852398653494, 1.98107786475107, -0.0410218822608488, 0.284698000407061))
'        Pars.Add((30, 121), New clsAreaReductionTable(50084, 6.4039461161634, 1.75365035671569, -0.0496148614421001, 0.273839024393026))
'        Pars.Add((30, 169), New clsAreaReductionTable(42047, 5.91926666648388, 1.57163423060632, -0.0618518454390864, 0.265511645134226))
'        Pars.Add((30, 225), New clsAreaReductionTable(34884, 5.50482619119558, 1.42974230155107, -0.0703468304725661, 0.259725239615703))
'        Pars.Add((30, 289), New clsAreaReductionTable(28063, 5.15181217792647, 1.31530336553894, -0.0746721453601331, 0.25530887387054))

'        '60 minutes
'        Pars.Add((60, 1), New clsAreaReductionTable(117610, 13.218049837703, 4.18387789644685, -0.169686674919718, 0.316527623047147))
'        Pars.Add((60, 9), New clsAreaReductionTable(96957, 12.3188911160318, 3.67851666737384, -0.142810043113753, 0.298607775060746))
'        Pars.Add((60, 25), New clsAreaReductionTable(81434, 11.5328027927596, 3.2666265018585, -0.117942886440903, 0.28324654124055))
'        Pars.Add((60, 49), New clsAreaReductionTable(69084, 10.8701551239488, 2.94143264206879, -0.0996642626564461, 0.270597117385042))
'        Pars.Add((60, 81), New clsAreaReductionTable(59071, 10.2973312132505, 2.67289264870716, -0.085529043377726, 0.259571397030302))
'        Pars.Add((60, 121), New clsAreaReductionTable(50084, 9.80095639203887, 2.44082953311646, -0.0741324815424861, 0.249039934010838))
'        Pars.Add((60, 169), New clsAreaReductionTable(42047, 9.34720017624355, 2.24229901594163, -0.0670411710749044, 0.239889910739321))
'        Pars.Add((60, 225), New clsAreaReductionTable(34884, 8.94202829167071, 2.08897820910422, -0.0570674742837164, 0.233613464525722))
'        Pars.Add((60, 289), New clsAreaReductionTable(28063, 8.57002673754884, 1.96821574746702, -0.0429331788392205, 0.229662731254204))

'        '120 minutes
'        Pars.Add((120, 1), New clsAreaReductionTable(117610, 16.7704942642987, 4.90845293340405, -0.176262468754872, 0.292683856304298))
'        Pars.Add((120, 9), New clsAreaReductionTable(96957, 16.0368037795387, 4.47632836121955, -0.162617080697623, 0.279128461179457))
'        Pars.Add((120, 25), New clsAreaReductionTable(81434, 15.381186895456, 4.14218395662763, -0.147080445986701, 0.269301971608663))
'        Pars.Add((120, 49), New clsAreaReductionTable(69084, 14.8237031567731, 3.87337169101183, -0.131448473224354, 0.261295821296992))
'        Pars.Add((120, 81), New clsAreaReductionTable(59071, 14.3451049633338, 3.65097650356868, -0.110252512485808, 0.254510267641862))
'        Pars.Add((120, 121), New clsAreaReductionTable(50084, 13.9223073450949, 3.45426989247272, -0.0866633366707748, 0.248110446555378))
'        Pars.Add((120, 169), New clsAreaReductionTable(42047, 13.539268902388, 3.26833185993273, -0.0630680537980209, 0.241396480378367))
'        Pars.Add((120, 225), New clsAreaReductionTable(34884, 13.1932505021018, 3.10164107029804, -0.03755717414805, 0.235093017433718))
'        Pars.Add((120, 289), New clsAreaReductionTable(28063, 12.8705996322783, 2.94198384227863, -0.0119834876493325, 0.228581723177871))

'        '4 hours
'        Pars.Add((240, 1), New clsAreaReductionTable(117610, 20.5924110465457, 5.63391667453638, -0.175338440062201, 0.273591890808796))
'        Pars.Add((240, 9), New clsAreaReductionTable(96957, 19.9950944517894, 5.27570860234007, -0.169778160870852, 0.263850146597729))
'        Pars.Add((240, 25), New clsAreaReductionTable(81434, 19.433331900897, 4.98628079302675, -0.163822355951894, 0.256583936221282))
'        Pars.Add((240, 49), New clsAreaReductionTable(69084, 18.9377349488511, 4.75552955239209, -0.159646064229244, 0.251113956618164))
'        Pars.Add((240, 81), New clsAreaReductionTable(59071, 18.5066339713206, 4.55879142993647, -0.151064850033211, 0.246332825137254))
'        Pars.Add((240, 121), New clsAreaReductionTable(50084, 18.1030219173478, 4.37508861754263, -0.143998686682755, 0.241677253528045))
'        Pars.Add((240, 169), New clsAreaReductionTable(42047, 17.738765450445, 4.20641197449785, -0.133265160323314, 0.237131044223392))
'        Pars.Add((240, 225), New clsAreaReductionTable(34884, 17.4354020534288, 4.06054960045783, -0.11648130087866, 0.232891079197069))
'        Pars.Add((240, 289), New clsAreaReductionTable(28063, 17.1621611476945, 3.90468670462041, -0.0994474016655955, 0.227517191513201))

'        '8 hours
'        Pars.Add((480, 1), New clsAreaReductionTable(117610, 25.4984238527503, 6.63814160762999, -0.188177488285709, 0.260335369980682))
'        Pars.Add((480, 9), New clsAreaReductionTable(96957, 24.9376842328987, 6.2517707612089, -0.190904779857454, 0.250695722298117))
'        Pars.Add((480, 25), New clsAreaReductionTable(81434, 24.4390612761673, 5.94586345235306, -0.187364433003022, 0.243293446714805))
'        Pars.Add((480, 49), New clsAreaReductionTable(69084, 24.0217782949645, 5.70437183507096, -0.177712506948985, 0.237466675656844))
'        Pars.Add((480, 81), New clsAreaReductionTable(59071, 23.6395489925322, 5.45963088150022, -0.167341719630605, 0.230953259016275))
'        Pars.Add((480, 121), New clsAreaReductionTable(50084, 23.2729032573903, 5.2368053867251, -0.155354592677637, 0.225017279915954))
'        Pars.Add((480, 169), New clsAreaReductionTable(42047, 22.9312157399957, 5.02319478874797, -0.141234654771305, 0.219054883339077))
'        Pars.Add((480, 225), New clsAreaReductionTable(34884, 22.6262837043886, 4.85003729570501, -0.125890075342847, 0.214354127220826))
'        Pars.Add((480, 289), New clsAreaReductionTable(28063, 22.3407249837604, 4.68084667712355, -0.112837615794725, 0.209520804742285))

'        '12 hours
'        Pars.Add((720, 1), New clsAreaReductionTable(117610, 28.5751072514721, 7.32996226167805, -0.191121620622388, 0.256515651793413))
'        Pars.Add((720, 9), New clsAreaReductionTable(96957, 28.0414864406803, 6.92367399680706, -0.195092766324782, 0.24690823760194))
'        Pars.Add((720, 25), New clsAreaReductionTable(81434, 27.570183546931, 6.59804234266047, -0.191492342802146, 0.239318041950248))
'        Pars.Add((720, 49), New clsAreaReductionTable(69084, 27.1546516004664, 6.33417348625882, -0.18473148862316, 0.233262925978768))
'        Pars.Add((720, 81), New clsAreaReductionTable(59071, 26.75410806539, 6.07073510289352, -0.179518976394131, 0.226908521414953))
'        Pars.Add((720, 121), New clsAreaReductionTable(50084, 26.3717275851971, 5.8285122648216, -0.174391902624955, 0.221013668747786))
'        Pars.Add((720, 169), New clsAreaReductionTable(42047, 26.0244143114607, 5.62578103038035, -0.164196101952651, 0.216173204247784))
'        Pars.Add((720, 225), New clsAreaReductionTable(34884, 25.7333904730299, 5.4768762666372, -0.150030730964239, 0.212831506690783))
'        Pars.Add((720, 289), New clsAreaReductionTable(28063, 25.4691444922108, 5.34480986036999, -0.135688588347242, 0.209854314580711))

'        '24 hours
'        Pars.Add((1440, 1), New clsAreaReductionTable(117610, 34.136582266701, 8.48992849504951, -0.190316759742033, 0.248704701270904))
'        Pars.Add((1440, 9), New clsAreaReductionTable(96957, 33.5992754331044, 8.11148256025994, -0.192502372266216, 0.241418377500722))
'        Pars.Add((1440, 25), New clsAreaReductionTable(81434, 33.0760957702847, 7.77806118908848, -0.194549482350984, 0.235156568753082))
'        Pars.Add((1440, 49), New clsAreaReductionTable(69084, 32.5960880037195, 7.49063779528556, -0.197776422041316, 0.22980174168234))
'        Pars.Add((1440, 81), New clsAreaReductionTable(59071, 32.1209622260836, 7.23125533815616, -0.201510106651065, 0.225125738365461))
'        Pars.Add((1440, 121), New clsAreaReductionTable(50084, 31.671209684885, 7.03687741384064, -0.20062059486348, 0.222185305956247))
'        Pars.Add((1440, 169), New clsAreaReductionTable(42047, 31.2787050596421, 6.89438799675961, -0.193381063023804, 0.22041794836498))
'        Pars.Add((1440, 225), New clsAreaReductionTable(34884, 30.969570958488, 6.79641317871125, -0.183561465189088, 0.219454547427255))
'        Pars.Add((1440, 289), New clsAreaReductionTable(28063, 30.696169180035, 6.71708418573438, -0.173429803570496, 0.218824835970191))

'        '2 days
'        Pars.Add((2880, 1), New clsAreaReductionTable(117610, 41.9004197187678, 10.0433611183503, -0.16581365221165, 0.239695954975164))
'        Pars.Add((2880, 9), New clsAreaReductionTable(96957, 41.4011403042835, 9.68953833613133, -0.16266779043278, 0.234040373403165))
'        Pars.Add((2880, 25), New clsAreaReductionTable(81434, 40.8747607670408, 9.32938097154997, -0.162862893446567, 0.228243072166741))
'        Pars.Add((2880, 49), New clsAreaReductionTable(69084, 40.3617920247273, 8.99295272942037, -0.168583707983155, 0.222808559241149))
'        Pars.Add((2880, 81), New clsAreaReductionTable(59071, 39.8917306628043, 8.7346747550457, -0.171198675915258, 0.218959533966523))
'        Pars.Add((2880, 121), New clsAreaReductionTable(50084, 39.4480199592926, 8.50505431831376, -0.174393146965155, 0.215601551791201))
'        Pars.Add((2880, 169), New clsAreaReductionTable(42047, 39.0678101926059, 8.30094135387385, -0.175302668486457, 0.212475214580748))
'        Pars.Add((2880, 225), New clsAreaReductionTable(34884, 38.7773123038217, 8.13778501314935, -0.173779622424461, 0.209859439184168))
'        Pars.Add((2880, 289), New clsAreaReductionTable(28063, 38.5264397374482, 7.97523982330591, -0.173286380740916, 0.207006925053442))

'        '4 days
'        Pars.Add((5760, 1), New clsAreaReductionTable(117610, 51.5226619550072, 12.4207093706914, -0.118373844938239, 0.241072741574144))
'        Pars.Add((5760, 9), New clsAreaReductionTable(96957, 51.0704987778862, 12.1027994189244, -0.112543981283232, 0.23698220515843))
'        Pars.Add((5760, 25), New clsAreaReductionTable(81434, 50.5668568967305, 11.7792150186861, -0.110052744226073, 0.232943389041207))
'        Pars.Add((5760, 49), New clsAreaReductionTable(69084, 50.074362055797, 11.474692189488, -0.110412309798056, 0.22915303796985))
'        Pars.Add((5760, 81), New clsAreaReductionTable(59071, 49.6046604727027, 11.2308870718641, -0.108175486217544, 0.226407901290734))
'        Pars.Add((5760, 121), New clsAreaReductionTable(50084, 49.1433740461701, 11.0224998455835, -0.106691835063473, 0.224292695801226))
'        Pars.Add((5760, 169), New clsAreaReductionTable(42047, 48.7405579514312, 10.8385305040521, -0.104118385624191, 0.222371900519736))
'        Pars.Add((5760, 225), New clsAreaReductionTable(34884, 48.4306594877267, 10.6612101121665, -0.102712102860235, 0.220133490333087))
'        Pars.Add((5760, 289), New clsAreaReductionTable(28063, 48.141460564565, 10.4367333362928, -0.106785855808423, 0.216793034816539))

'        '8 days
'        Pars.Add((11520, 1), New clsAreaReductionTable(117610, 70.1872119340305, 15.4154821977777, -0.060505168263816, 0.219633773347014))
'        Pars.Add((11520, 9), New clsAreaReductionTable(96957, 69.7420298803996, 15.0136835218905, -0.0546804052018978, 0.215274541730967))
'        Pars.Add((11520, 25), New clsAreaReductionTable(81434, 69.2493618437318, 14.6328530807044, -0.0513837362529452, 0.211306684872056))
'        Pars.Add((11520, 49), New clsAreaReductionTable(69084, 68.7564890232306, 14.2301428678774, -0.0498597169175864, 0.206964361764742))
'        Pars.Add((11520, 81), New clsAreaReductionTable(59071, 68.2619561793752, 13.8322501868761, -0.048495637746028, 0.202634834409498))
'        Pars.Add((11520, 121), New clsAreaReductionTable(50084, 67.7185375418625, 13.4149510181542, -0.0513958478694518, 0.198098652231839))
'        Pars.Add((11520, 169), New clsAreaReductionTable(42047, 67.217164238321, 13.0245670958287, -0.0572061442164475, 0.193768470351555))
'        Pars.Add((11520, 225), New clsAreaReductionTable(34884, 66.8679554043205, 12.6964089029675, -0.0646012036002722, 0.18987284456655))
'        Pars.Add((11520, 289), New clsAreaReductionTable(28063, 66.5753674772188, 12.3645553134078, -0.074572630413224, 0.185722674645976))

'        '9 days
'        Pars.Add((12960, 1), New clsAreaReductionTable(117610, 74.1239158516677, 16.1198061395207, -0.0512522408942082, 0.217471054440495))
'        Pars.Add((12960, 9), New clsAreaReductionTable(96957, 73.7329050681309, 15.7059336898081, -0.0458452704818572, 0.213011187817643))
'        Pars.Add((12960, 25), New clsAreaReductionTable(81434, 73.2583441622298, 15.3402795229841, -0.0415688112727007, 0.209399757780673))
'        Pars.Add((12960, 49), New clsAreaReductionTable(69084, 72.75705086347, 14.9568446446211, -0.0389557533067463, 0.205572442355971))
'        Pars.Add((12960, 81), New clsAreaReductionTable(59071, 72.2410172767708, 14.5738293464359, -0.0366930817566132, 0.201738982863439))
'        Pars.Add((12960, 121), New clsAreaReductionTable(50084, 71.6913458109891, 14.1635066743173, -0.0378007973080677, 0.197562293106601))
'        Pars.Add((12960, 169), New clsAreaReductionTable(42047, 71.1933316799655, 13.7694562929812, -0.0412919325797747, 0.193409354051288))
'        Pars.Add((12960, 225), New clsAreaReductionTable(34884, 70.8448529796266, 13.4609153977979, -0.045498437512225, 0.190005552014752))
'        Pars.Add((12960, 289), New clsAreaReductionTable(28063, 70.5565217418994, 13.1894773758818, -0.0499499102107633, 0.186934914735874))

'    End Sub
'    Private Function InterpolateGEVParameters(durationMinutes As Integer, areaKm2 As Double) As Tuple(Of Double, Double, Double)
'        ' Convert area to number of pixels (each pixel is 5.73 km²)
'        Dim pixels As Double = areaKm2 / 5.73  ' Keep as Double to avoid rounding errors

'        ' Get the nearest duration from our Pars dictionary
'        Dim uniqueDurations = Pars.Keys.Select(Function(k) k.Item1).Distinct().OrderBy(Function(x) x).ToList()
'        Dim nearestDuration = uniqueDurations.OrderBy(Function(d) Math.Abs(d - durationMinutes)).First()

'        ' For the reference area (5.73 km²), we know this is 1 pixel
'        Dim pixelOptions = {1, 9, 25, 49, 81, 121, 169, 225, 289}

'        ' Find the bracketing pixel values
'        Dim lowerPixel = pixelOptions.Where(Function(p) p <= pixels).DefaultIfEmpty(1).Max()
'        Dim upperPixel = pixelOptions.Where(Function(p) p >= pixels).DefaultIfEmpty(289).Min()

'        ' Get parameters for both bounds
'        Dim paramsLower = Pars((nearestDuration, lowerPixel))
'        Dim paramsUpper = Pars((nearestDuration, upperPixel))

'        ' Calculate interpolation weight
'        Dim weight As Double = 0
'        If upperPixel <> lowerPixel Then
'            weight = (pixels - lowerPixel) / (upperPixel - lowerPixel)
'        End If

'        ' Interpolate parameters
'        Dim loc = paramsLower.locpar_LM + weight * (paramsUpper.locpar_LM - paramsLower.locpar_LM)
'        Dim cv = paramsLower.cV_LM + weight * (paramsUpper.cV_LM - paramsLower.cV_LM)  ' Using CV_LM instead of sclpar_LM
'        Dim shape = paramsLower.k_LM + weight * (paramsUpper.k_LM - paramsLower.k_LM)

'        Return New Tuple(Of Double, Double, Double)(loc, cv, shape)
'    End Function




'    Private Function CalculateReductionFactorUpTo720(volume As Double, durationHours As Double, areaKm2 As Double) As Double
'        If durationHours > 12 Then Throw New Exception("Function only valid for durations <= 12 hours")
'        If durationHours < 0.25 OrElse (durationHours < 0.5 AndAlso areaKm2 > 968.37) Then Return Double.NaN

'        ' Calculate point to 5.73 km² ARF (unchanged)
'        Dim ARFpoint_5_73 As Double = 1 - 0.08266513 * Math.Pow(durationHours, -0.289186)

'        ' Get interpolated GEV parameters
'        Dim params = InterpolateGEVParameters(CInt(durationHours * 60), areaKm2)
'        Dim paramsRef = InterpolateGEVParameters(CInt(durationHours * 60), 5.73)

'        ' Estimate return period
'        Dim T As Double = Me.Setup.Neerslagstatistiek.STOWA2024_JAARROND_T(
'        CInt(durationHours * 60), volume, 2014, "", "")
'        If T > 50 Then T = 50

'        ' Calculate R and R_ref using GEV distribution and CV_LM
'        Dim R_ref As Double = paramsRef.Item1 * (1 + paramsRef.Item2 * (1 - Math.Pow(T, -1 * paramsRef.Item3)) / paramsRef.Item3)
'        Dim R As Double = params.Item1 * (1 + params.Item2 * (1 - Math.Pow(T, -1 * params.Item3)) / params.Item3)

'        Return ARFpoint_5_73 * (R / R_ref)
'    End Function
'    Private Function CalculateReductionFactorAbove720(volume As Double, durationHours As Double, areaKm2 As Double) As Double
'        Try
'            If durationHours <= 12 Then Throw New Exception("Function only valid for durations > 12 hours")
'            If durationHours > 240 Then Throw New Exception("Function only valid for durations <= 240 hours")

'            ' Calculate point to 5.73 km² ARF (unchanged)
'            Dim ARFpoint_5_73 As Double = 1 - 0.08266513 * Math.Pow(durationHours, -0.289186)

'            ' Get interpolated GEV parameters
'            Dim params = InterpolateGEVParameters(CInt(durationHours * 60), areaKm2)
'            Dim paramsRef = InterpolateGEVParameters(CInt(durationHours * 60), 5.73)

'            ' Estimate return period
'            Dim T As Double = Me.Setup.Neerslagstatistiek.STOWA2024_JAARROND_T(
'            CInt(durationHours * 60), volume, 2014, "", "")

'            ' Cap at 50 years as per R script
'            If T > 50 Then T = 50

'            ' Calculate R and R_ref using GEV distribution with CV_LM
'            Dim R_ref As Double = paramsRef.Item1 * (1 + paramsRef.Item2 * (1 - Math.Pow(T, -1 * paramsRef.Item3)) / paramsRef.Item3)
'            Dim R As Double = params.Item1 * (1 + params.Item2 * (1 - Math.Pow(T, -1 * params.Item3)) / params.Item3)

'            Return ARFpoint_5_73 * (R / R_ref)
'        Catch ex As Exception
'            Me.Setup.Log.AddError(ex.Message)
'            Return Double.NaN  ' Return NaN instead of 0 to indicate invalid result
'        End Try
'    End Function


'    Public Function calculateAbove720ByReturnPeriod(durationHours As Double, T As Double, areaKm2 As Double) As Double
'        'we basically use this as a testing routine since it allows us to compare the results with the R script and/or Meteobase
'        Try
'            If durationHours <= 12 Then Throw New Exception("Function only valid for durations > 12 hours")
'            If durationHours > 240 Then Throw New Exception("Function only valid for durations <= 240 hours")

'            ' Calculate point to 5.73 km² ARF (unchanged)
'            Dim ARFpoint_5_73 As Double = 1 - 0.08266513 * Math.Pow(durationHours, -0.289186)

'            ' Get interpolated GEV parameters
'            Dim params = InterpolateGEVParameters(CInt(durationHours * 60), areaKm2)
'            Dim paramsRef = InterpolateGEVParameters(CInt(durationHours * 60), 5.73)

'            ' Cap at 50 years as per R script
'            If T > 50 Then T = 50

'            ' Calculate R and R_ref using GEV distribution with CV_LM
'            Dim R_ref As Double = paramsRef.Item1 * (1 + paramsRef.Item2 * (1 - Math.Pow(T, -1 * paramsRef.Item3)) / paramsRef.Item3)
'            Dim R As Double = params.Item1 * (1 + params.Item2 * (1 - Math.Pow(T, -1 * params.Item3)) / params.Item3)

'            Return ARFpoint_5_73 * (R / R_ref)
'        Catch ex As Exception
'            Me.Setup.Log.AddError(ex.Message)
'            Return Double.NaN  ' Return NaN instead of 0 to indicate invalid result
'        End Try
'    End Function

'End Class

