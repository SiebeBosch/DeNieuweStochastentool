Imports STOCHLIB.General

Public Class clsNeerslagstatistiek

    Private Setup As clsSetup

    Public Sub New(ByRef Setup As clsSetup)
        Me.Setup = Setup
    End Sub

    Public Function STOWA2015_2018_WINTER_T(ByVal DuurMinuten As Integer, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String, ByVal Volume As Double) As Double
        Dim T_estimate As Double = 1
        Dim V_estimate As Double
        Dim Done As Boolean = False
        Dim iIter As Integer = 0

        While Not Done
            V_estimate = STOWA2015_2018_NDJF_V(DuurMinuten, T_estimate, Zichtjaar, Scenario, Corner)
            iIter += 1

            If iIter = 1000 OrElse Math.Abs(V_estimate - Volume) < 0.001 Then
                Done = True
            Else
                T_estimate = T_estimate * Volume / V_estimate
            End If
        End While

        Return T_estimate
    End Function

    Public Function GLOCDF(ByVal mu As Double, ByVal Sigma As Double, ByVal teta As Double, ByVal x As Double) As Double
        Dim Z As Double = (x - mu) / Sigma

        If teta = 0 Then
            Return (1 + Math.Exp(-Z)) ^ -1
        Else
            Return (1 + (1 - teta * Z) ^ (1 / teta)) ^ -1
        End If
    End Function

    Public Function GLOINVERSE(ByVal mu As Double, ByVal Sigma As Double, ByVal teta As Double, ByVal value As Double) As Double
        If teta = 0 Then
            Return mu - Sigma * Math.Log(1 / value - 1)
        Else
            Return mu + Sigma * ((1 - (1 / value - 1) ^ teta) / teta)
        End If
    End Function

    Public Function GEVCDF(ByVal mu As Double, ByVal Sigma As Double, ByVal Zeta As Double, ByVal x As Double) As Double
        Dim Z As Double = (x - mu) / Sigma
        Dim T As Double

        If Zeta = 0 Then
            T = Math.Exp(-Z)
        Else
            T = (1 - Zeta * Z) ^ (1 / Zeta)
        End If

        Return Math.Exp(-T)
    End Function

    Public Function GEVINVERSE(ByVal mu As Double, ByVal Sigma As Double, ByVal Zeta As Double, ByVal value As Double) As Double
        '------------------------------------------------------------------------------------------------
        'Datum: 9-11-2010
        'Auteur: Siebe Bosch
        'Deze routine berekent de ONDERschrijdingskans p van een bepaalde parameterwaarde volgens GEV-verdeling
        'dit betekent gewoon dat we de verdelingsfunctie gaan berekenen (= de integraal van de kansdichtheidsfunctie)
        '------------------------------------------------------------------------------------------------
        Return mu + Sigma * (((-1 * Math.Log(value)) ^ Zeta - 1) / -Zeta)
    End Function

    Public Function STOWA2015_2018_NDJF_V(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Dim locpar As Double, scalepar As Double, shapepar As Double, dispcoef As Double
        Dim P As Double
        Dim Multiplier As Double

        P = Math.Exp(-1 / T)

        If DuurMinuten > 720 Then
            dispcoef = GEVDispCoefBasisstatistiek2015WinterLang(DuurMinuten, Zichtjaar, Scenario, Corner)
            locpar = GEVLocParBasisstatistiek2015WinterLang(DuurMinuten, Zichtjaar, Scenario, Corner)
            scalepar = dispcoef * locpar
            shapepar = GEVShapeParBasisstatistiek2015WinterLang(DuurMinuten, Zichtjaar, Scenario)
            Return GEVINVERSE(locpar, scalepar, shapepar, P)
        Else
            dispcoef = GEVDispCoefBasisstatistiek2018WinterKort(DuurMinuten)
            locpar = GEVLocParBasisstatistiek2018WinterKort(DuurMinuten)
            scalepar = dispcoef * locpar
            shapepar = GEVShapeParBasisStatistiek2018WinterKort(DuurMinuten)

            If Zichtjaar = 2014 Then
                Multiplier = 1
            Else
                Multiplier = STOWA2015_WINTER_V(DuurMinuten, T, Zichtjaar, Scenario, Corner) / STOWA2015_WINTER_V(DuurMinuten, T, 2014, "", "")
            End If

            Return GEVINVERSE(locpar, scalepar, shapepar, P) * 1.02 * Multiplier
        End If
    End Function

    Public Function STOWA2015_WINTER_V(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Dim locpar As Double, scalepar As Double, shapepar As Double, dispcoef As Double
        Dim P As Double

        P = Math.Exp(-1 / T)

        If DuurMinuten < 120 Then DuurMinuten = 120

        dispcoef = GEVDispCoefBasisstatistiek2015WinterLang(DuurMinuten, Zichtjaar, Scenario, Corner)
        locpar = GEVLocParBasisstatistiek2015WinterLang(DuurMinuten, Zichtjaar, Scenario, Corner)
        scalepar = dispcoef * locpar
        shapepar = GEVShapeParBasisstatistiek2015WinterLang(DuurMinuten, Zichtjaar, Scenario)

        Return GEVINVERSE(locpar, scalepar, shapepar, P)
    End Function

    Public Function GEVDispCoefBasisstatistiek2015WinterLang(ByVal DuurMinuten As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        ' This function calculates the dispersion coefficient for GEV distribution for long duration (>= 2 hours) according to STOWA 2015
        Select Case Zichtjaar
            Case 2014
                Return 0.234
            Case 2030
                Select Case Corner
                    Case "lower"
                        Return 0.23
                    Case "center"
                        Return 0.233
                    Case "upper"
                        Return 0.236
                End Select
            Case 2050
                Select Case Scenario
                    Case "GL"
                        Select Case Corner
                            Case "lower"
                                Return 0.234
                            Case "center"
                                Return 0.236
                            Case "upper"
                                Return 0.239
                        End Select
                    Case "GH"
                        Select Case Corner
                            Case "lower"
                                Return 0.232
                            Case "center"
                                Return 0.235
                            Case "upper"
                                Return 0.237
                        End Select
                    Case "WL"
                        Select Case Corner
                            Case "lower"
                                Return 0.235
                            Case "center"
                                Return 0.241
                            Case "upper"
                                Return 0.247
                        End Select
                    Case "WH"
                        Select Case Corner
                            Case "lower"
                                Return 0.227
                            Case "center"
                                Return 0.233
                            Case "upper"
                                Return 0.239
                        End Select
                End Select
            Case 2085
                Select Case Scenario
                    Case "GL"
                        Select Case Corner
                            Case "lower"
                                Return 0.229
                            Case "center"
                                Return 0.233
                            Case "upper"
                                Return 0.237
                        End Select
                    Case "GH"
                        Select Case Corner
                            Case "lower"
                                Return 0.228
                            Case "center"
                                Return 0.232
                            Case "upper"
                                Return 0.236
                        End Select
                    Case "WL"
                        Select Case Corner
                            Case "lower"
                                Return 0.236
                            Case "center"
                                Return 0.246
                            Case "upper"
                                Return 0.255
                        End Select
                    Case "WH"
                        Select Case Corner
                            Case "lower"
                                Return 0.226
                            Case "center"
                                Return 0.236
                            Case "upper"
                                Return 0.245
                        End Select
                End Select
        End Select
        Return 0 ' Default return value if no condition is met
    End Function

    Public Function GEVLocParBasisstatistiek2015WinterLang(ByVal DuurMinuten As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Select Case Zichtjaar
            Case 2014
                Return (0.67 - 0.0426 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.193)
            Case 2030
                Select Case Corner
                    Case "lower"
                        Return (0.667 - 0.0435 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.197)
                    Case "center"
                        Return (0.665 - 0.043 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.196)
                    Case "upper"
                        Return (0.666 - 0.0425 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.194)
                End Select
            Case 2050
                Select Case Scenario
                    Case "GL"
                        Select Case Corner
                            Case "lower"
                                Return (0.668 - 0.0431 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.196)
                            Case "center"
                                Return (0.668 - 0.0426 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.194)
                            Case "upper"
                                Return (0.667 - 0.0422 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.193)
                        End Select
                    Case "GH"
                        Select Case Corner
                            Case "lower"
                                Return (0.661 - 0.0437 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.2)
                            Case "center"
                                Return (0.661 - 0.0432 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.198)
                            Case "upper"
                                Return (0.66 - 0.0426 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.196)
                        End Select
                    Case "WL"
                        Select Case Corner
                            Case "lower"
                                Return (0.671 - 0.0421 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.19)
                            Case "center"
                                Return (0.672 - 0.0411 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.186)
                            Case "upper"
                                Return (0.671 - 0.0402 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.183)
                        End Select
                    Case "WH"
                        Select Case Corner
                            Case "lower"
                                Return (0.662 - 0.0431 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.196)
                            Case "center"
                                Return (0.66 - 0.0422 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.193)
                            Case "upper"
                                Return (0.661 - 0.0412 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.189)
                        End Select
                End Select
            Case 2085
                Select Case Scenario
                    Case "GL"
                        Select Case Corner
                            Case "lower"
                                Return (0.667 - 0.0429 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.195)
                            Case "center"
                                Return (0.666 - 0.0423 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.193)
                            Case "upper"
                                Return (0.667 - 0.0416 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.19)
                        End Select
                    Case "GH"
                        Select Case Corner
                            Case "lower"
                                Return (0.651 - 0.0437 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.205)
                            Case "center"
                                Return (0.651 - 0.043 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.202)
                            Case "upper"
                                Return (0.65 - 0.0423 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.2)
                        End Select
                    Case "WL"
                        Select Case Corner
                            Case "lower"
                                Return (0.675 - 0.0417 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.185)
                            Case "center"
                                Return (0.674 - 0.0403 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.18)
                            Case "upper"
                                Return (0.675 - 0.0389 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.175)
                        End Select
                    Case "WH"
                        Select Case Corner
                            Case "lower"
                                Return (0.653 - 0.043 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.198)
                            Case "center"
                                Return (0.651 - 0.0415 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.193)
                            Case "upper"
                                Return (0.647 - 0.0402 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.19)
                        End Select
                End Select
        End Select
        Return 0
    End Function

    Public Function GEVShapeParBasisstatistiek2015WinterLang(ByVal DuurMinuten As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String) As Double
        If DuurMinuten / 60 <= 240 Then
            Return -0.09 + 0.017 * DuurMinuten / 60 / 24
        Else
            Return -0.09 + 0.683 * Math.Log(DuurMinuten / 60 / 24)
        End If
    End Function

    Public Function GEVDispCoefBasisstatistiek2018WinterKort(ByVal DuurMinuten As Double) As Double
        If DuurMinuten <= 91 Then
            Return 0.41692 - 0.07583 * Math.Log10(DuurMinuten)
        Else
            Return 0.2684
        End If
    End Function

    Public Function GEVLocParBasisstatistiek2018WinterKort(ByVal DuurMinuten As Double) As Double
        Return 4.883 - 5.587 * Math.Log10(DuurMinuten) + 3.526 * Math.Log10(DuurMinuten) ^ 2
    End Function

    Public Function GEVShapeParBasisStatistiek2018WinterKort(ByVal DuurMinuten As Double) As Double
        Return -0.294 + 0.1474 * Math.Log10(DuurMinuten) - 0.0192 * Math.Log10(DuurMinuten) ^ 2
    End Function

    Public Function GEVShapeParBasisStatistiek2018ZomerKort(ByVal DuurMinuten As Double) As Double
        Return -0.0336 - 0.264 * Math.Log10(DuurMinuten) + 0.0636 * Math.Log10(DuurMinuten) ^ 2
    End Function

    Public Function STOWA2019_NDJF_T(ByVal DuurMinuten As Integer, ByVal Volume As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Dim T_estimate As Double = STOWA2015_2018_WINTER_T(DuurMinuten, Zichtjaar, Scenario, Corner, Volume)
        Dim V_estimate As Double
        Dim Done As Boolean = False
        Dim iIter As Integer = 0

        While Not Done
            V_estimate = STOWA2019_NDJF_V(DuurMinuten, T_estimate, Zichtjaar, Scenario, Corner)
            iIter += 1

            If iIter = 1000 OrElse Math.Abs(V_estimate - Volume) < 0.001 Then
                Done = True
            Else
                T_estimate = T_estimate * Volume / V_estimate
            End If
        End While

        Return T_estimate
    End Function

    Public Function STOWA2024_NDJF_V(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Dim baseValue As Double = STOWA2019_NDJF_V(DuurMinuten, T, 2014, "", "")
        Dim VeranderGetal As Double = getVeranderGetal(Zichtjaar, Scenario, "winter", DuurMinuten / 60)

        Return VeranderGetal * baseValue
    End Function

    Public Function STOWA2024_NDJF_T(ByVal DuurMinuten As Integer, ByVal Volume As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Dim VeranderGetal As Double = getVeranderGetal(Zichtjaar, Scenario, "winter", DuurMinuten / 60)

        Dim scaledVolume As Double = Volume / VeranderGetal
        Return STOWA2019_NDJF_T(DuurMinuten, scaledVolume, 2014, "", "")
    End Function

    Public Function STOWA2024_JAARROND_V(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Dim baseValue As Double = STOWA2019_JAARROND_V(DuurMinuten, T, 2014, "", "")
        Dim VeranderGetal As Double = getVeranderGetal(Zichtjaar, Scenario, "jaarrond", DuurMinuten / 60)

        Return VeranderGetal * baseValue
    End Function

    Public Function STOWA2024_JAARROND_T(ByVal DuurMinuten As Integer, ByVal Volume As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String, Optional ByVal Debugging As Boolean = False) As Double
        Dim VeranderGetal As Double = getVeranderGetal(Zichtjaar, Scenario, "jaarrond", DuurMinuten / 60)

        Dim scaledVolume As Double = Volume / VeranderGetal
        Return STOWA2019_JAARROND_T(DuurMinuten, scaledVolume, 2014, "", "", Debugging)
    End Function

    Public Function STOWA2019_JAARROND_T(ByVal DuurMinuten As Integer, ByVal Volume As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String, Optional ByVal Debugging As Boolean = False) As Double
        Dim T_estimate As Double = 1
        Dim V_estimate As Double
        Dim Done As Boolean = False
        Dim iIter As Integer = 0

        While Not Done
            V_estimate = STOWA2019_JAARROND_V(DuurMinuten, T_estimate, Zichtjaar, Scenario, Corner, Debugging)
            iIter += 1

            If iIter = 1000 OrElse Math.Abs(V_estimate - Volume) < 0.001 Then
                Done = True
            Else
                T_estimate = T_estimate * Volume / V_estimate
            End If
        End While

        Return T_estimate
    End Function

    Public Function getVeranderGetal(ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Seizoen As String, ByVal DuurUren As Double) As Double
        Select Case Zichtjaar
            Case 2014
                Return 1

            Case 2033
                Select Case Scenario
                    Case "L"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(0.6, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(0.6, DuurUren)
                        End If
                    Case Else
                        Return 0
                End Select

            Case 2050
                Select Case Scenario
                    Case "L"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(0.8, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(0.8, DuurUren)
                        End If
                    Case "M"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(1.1, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(1.1, DuurUren)
                        End If
                    Case "H"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(1.5, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(1.5, DuurUren)
                        End If
                    Case Else
                        Return 0
                End Select

            Case 2100
                Select Case Scenario
                    Case "L"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(0.8, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(0.8, DuurUren)
                        End If
                    Case "M"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(1.9, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(1.9, DuurUren)
                        End If
                    Case "H"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(4.0, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(4.0, DuurUren)
                        End If
                    Case Else
                        Return 0
                End Select

            Case 2150
                Select Case Scenario
                    Case "L"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(0.8, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(0.8, DuurUren)
                        End If
                    Case "M"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(2.1, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(2.1, DuurUren)
                        End If
                    Case "H"
                        If Seizoen = "winter" Then
                            Return VeranderGetalFunctieWinter(5.5, DuurUren)
                        Else
                            Return verandergetalfunctieJaarrond(5.5, DuurUren)
                        End If
                    Case Else
                        Return 0
                End Select

            Case Else
                Return 0
        End Select
    End Function

    Public Function VeranderGetalFunctieWinter(ByVal Ts As Double, ByVal D As Double, Optional ByVal T As Double = 1) As Double
        Dim v As Double

        If D < 1 / 6 Then
            Throw New Exception("Gekozen duur " & D & " valt buiten domein (10 minuten t/m 240 uur)")
        ElseIf D <= 24 Then
            v = 1.244
        ElseIf D < 120 Then
            v = Poly(D)
        ElseIf D <= 240 Then
            v = 1.181
        Else
            Throw New Exception("Gekozen duur " & D & " valt buiten domein: 10 minuten t/m 10 dagen (240 uur)")
        End If

        Return 1 + (v - 1) * (Ts - 0.4) / (4 - 0.4)
    End Function

    Public Function verandergetalfunctieJaarrond(ByVal Ts As Double, ByVal D As Double, Optional ByVal T As Double = 1) As Double
        Dim v As Double

        If D < 1 / 6 Then
            Throw New Exception("Gekozen duur " & D & " valt buiten domein (10 minuten t/m 240 uur)")
        ElseIf D <= 24 Then
            v = 1.234
        ElseIf D < 120 Then
            v = LogPoly(D)
        ElseIf D <= 240 Then
            v = 1.109
        Else
            Throw New Exception("Gekozen duur " & D & " valt buiten domein: 10 minuten t/m 10 dagen (240 uur)")
        End If

        Return 1 + (v - 1) * (Ts - 0.4) / (4 - 0.4)
    End Function

    Public Function Poly(ByVal D As Double) As Double
        Return 0.000005952 * D * D - 0.001515 * D + 1.277
    End Function

    Public Function LogPoly(ByVal D As Double) As Double
        Dim logD As Double = Math.Log(D)
        Return 0.009143 * logD * logD - 0.1508 * logD + 1.621
    End Function

    Public Function STOWA2019_NDJF_V(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Dim P As Double = Math.Exp(-1 / T)
        Dim locpar As Double, scalepar As Double, shapepar As Double, dispcoef As Double
        Dim Volume As Double
        Dim Multiplier As Double

        If DuurMinuten > 720 Then
            dispcoef = GEVDispCoefBasisstatistiek2019LangeDuurWinter(DuurMinuten)
            locpar = GEVLocparBasisstatistiek2019LangeDuurWinter(DuurMinuten)
            scalepar = dispcoef * locpar
            shapepar = GEVShapeParBasisstatistiek2019LangeDuurWinter(DuurMinuten)
            Volume = GEVINVERSE(locpar, scalepar, shapepar, P)
        Else
            dispcoef = GEVDispCoefBasisstatistiek2019KorteDuurWinter(DuurMinuten)
            locpar = GEVLocparBasisstatistiek2019KorteDuurWinter(DuurMinuten)
            scalepar = dispcoef * locpar
            shapepar = GEVShapeParBasisstatistiek2019KorteDuurWinter(DuurMinuten)
            Volume = GEVINVERSE(locpar, scalepar, shapepar, P)
        End If

        Multiplier = If(Zichtjaar <> 2014, STOWA2019_MULTIPLIER_WINTER(DuurMinuten, T, Zichtjaar, Scenario, Corner), 1)

        Return Volume * Multiplier
    End Function

    Public Function GEVDispCoefBasisstatistiek2019LangeDuurWinter(ByVal DuurMinuten As Integer) As Double
        Return 0.234
    End Function


    Public Function GEVLocparBasisstatistiek2019LangeDuurWinter(ByVal DuurMinuten As Integer) As Double
        Return (0.67 - 0.0426 * Math.Log(DuurMinuten / 60)) ^ (-1 / 0.193)
    End Function

    Public Function GEVShapeParBasisstatistiek2019LangeDuurWinter(ByVal DuurMinuten As Integer) As Double
        Return -0.09 + 0.017 * DuurMinuten / 60 / 24
    End Function

    Public Function GEVDispCoefBasisstatistiek2019KorteDuurWinter(ByVal DuurMinuten As Integer) As Double
        If DuurMinuten <= 91 Then
            Return 0.41692 - 0.07583 * Math.Log10(DuurMinuten)
        Else
            Return 0.2684
        End If
    End Function

    Public Function GEVLocparBasisstatistiek2019KorteDuurWinter(ByVal DuurMinuten As Integer) As Double
        Return 1.07 * 1.02 * (4.883 - 5.587 * Math.Log10(DuurMinuten) + 3.526 * Math.Log10(DuurMinuten) ^ 2)
    End Function

    Public Function GEVShapeParBasisstatistiek2019KorteDuurWinter(ByVal DuurMinuten As Integer) As Double
        Return -0.294 + 0.1474 * Math.Log10(DuurMinuten) - 0.0192 * Math.Log10(DuurMinuten) ^ 2
    End Function

    Public Function GLOScaleParBasisstatistiek2019KorteDuur(ByVal DuurMinuten As Integer) As Double
        Return GLODispCoefBasisstatistiek2019KorteDuur(DuurMinuten) * GLOLocparBasisstatistiek2019KorteDuur(DuurMinuten)
    End Function

    Public Function GEVScaleParBasisstatistiek2019KorteDuurWinter(ByVal DuurMinuten As Integer) As Double
        'Return GLODispCoefBasisstatistiek2019KorteDuurWinter(DuurMinuten) * GLOLocparBasisstatistiek2019KorteDuurWinter(DuurMinuten)
        Return Nothing
    End Function

    Public Function GLOShapeParBasisstatistiek2019KorteDuur(ByVal DuurMinuten As Integer, ByVal T_estimate As Double) As Double
        If DuurMinuten <= 90 OrElse (DuurMinuten <= 720 AndAlso T_estimate <= 120) Then
            Return -0.0336 - 0.264 * Math.Log10(DuurMinuten) + 0.0636 * Math.Log10(DuurMinuten) ^ 2
        Else
            Return -0.31 - 0.0544 * Math.Log10(DuurMinuten) + 0.0288 * Math.Log10(DuurMinuten) ^ 2
        End If
    End Function

    Public Function GEVLocparBasisstatistiek2019LangeDuur(ByVal DuurMinuten As Integer) As Double
        Return 1.02 * (0.239 - 0.025 * Math.Log(DuurMinuten / 60)) ^ (-1 / 0.512)
    End Function

    Public Function GEVLocParBasisstatistiek2015(ByVal DuurMinuten As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Select Case Zichtjaar
            Case 2014
                Return (0.239 - 0.025 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.512)

            Case 2030
                Select Case Corner
                    Case "lower"
                        Return (0.246 - 0.0257 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.503)
                    Case "center"
                        Return (0.24 - 0.025 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.506)
                    Case "upper"
                        Return (0.235 - 0.0243 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.509)
                    Case Else
                        Return -999
                End Select

            Case 2050
                Select Case Scenario
                    Case "GL"
                        Select Case Corner
                            Case "lower"
                                Return (0.247 - 0.0258 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.501)
                            Case "center"
                                Return (0.241 - 0.025 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.504)
                            Case "upper"
                                Return (0.236 - 0.0243 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.506)
                            Case Else
                                Return -999
                        End Select
                    Case "GH"
                        Select Case Corner
                            Case "lower"
                                Return (0.269 - 0.0272 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.474)
                            Case "center"
                                Return (0.26 - 0.0263 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.479)
                            Case "upper"
                                Return (0.252 - 0.0254 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.483)
                            Case Else
                                Return -999
                        End Select
                    Case "WL"
                        Select Case Corner
                            Case "lower"
                                Return (0.262 - 0.0266 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.48)
                            Case "center"
                                Return (0.249 - 0.0252 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.485)
                            Case "upper"
                                Return (0.241 - 0.024 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.486)
                            Case Else
                                Return -999
                        End Select
                    Case "WH"
                        Select Case Corner
                            Case "lower"
                                Return (0.289 - 0.0287 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.451)
                            Case "center"
                                Return (0.276 - 0.0271 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.456)
                            Case "upper"
                                Return (0.265 - 0.0257 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.459)
                            Case Else
                                Return -999
                        End Select
                    Case Else
                        Return -999
                End Select

            Case 2085
                Select Case Scenario
                    Case "GL"
                        Select Case Corner
                            Case "lower"
                                Return (0.252 - 0.0261 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.494)
                            Case "center"
                                Return (0.243 - 0.025 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.498)
                            Case "upper"
                                Return (0.235 - 0.0241 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.501)
                            Case Else
                                Return -999
                        End Select
                    Case "GH"
                        Select Case Corner
                            Case "lower"
                                Return (0.271 - 0.0274 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.471)
                            Case "center"
                                Return (0.26 - 0.0262 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.476)
                            Case "upper"
                                Return (0.25 - 0.0251 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.481)
                            Case Else
                                Return -999
                        End Select
                    Case "WL"
                        Select Case Corner
                            Case "lower"
                                Return (0.272 - 0.0272 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.464)
                            Case "center"
                                Return (0.248 - 0.0244 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.475)
                            Case "upper"
                                Return (0.23 - 0.0223 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.482)
                            Case Else
                                Return -999
                        End Select
                    Case "WH"
                        Select Case Corner
                            Case "lower"
                                Return (0.286 - 0.0284 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.448)
                            Case "center"
                                Return (0.262 - 0.0256 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.458)
                            Case "upper"
                                Return (0.247 - 0.0236 * Math.Log(DuurMinuten / 60)) ^ (1 / -0.461)
                            Case Else
                                Return -999
                        End Select
                    Case Else
                        Return -999
                End Select

            Case Else
                Return -999
        End Select
    End Function

    Public Function GLODispCoefBasisstatistiek2018(ByVal DuurMinuten As Double) As Double
        If DuurMinuten <= 104 Then
            Return 0.04704 + 0.1978 * Math.Log10(DuurMinuten) - 0.05729 * Math.Log10(DuurMinuten) ^ 2
        Else
            Return 0.2801 - 0.0333 * Math.Log10(DuurMinuten)
        End If
    End Function

    Public Function GLOLocParBasisstatistiek2018(ByVal DuurMinuten As Double) As Double
        Return 7.339 + 0.848 * Math.Log10(DuurMinuten) + 2.844 * Math.Log10(DuurMinuten) ^ 2
    End Function

    Public Function GLOShapeParBasisStatistiek2018(ByVal DuurMinuten As Double) As Double
        Return -0.0336 - 0.264 * Math.Log10(DuurMinuten) + 0.0636 * Math.Log10(DuurMinuten) ^ 2
    End Function

    Public Function GEVDispCoefBasisstatistiek2015(ByVal DuurMinuten As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Select Case Zichtjaar
            Case 2014
                Return 0.378 - 0.0578 * Math.Log(DuurMinuten / 60) + 0.0054 * Math.Log(DuurMinuten / 60) ^ 2

            Case 2030
                Select Case Corner
                    Case "lower"
                        Return 0.377 - 0.0565 * Math.Log(DuurMinuten / 60) + 0.005 * Math.Log(DuurMinuten / 60) ^ 2
                    Case "center"
                        Return 0.384 - 0.0576 * Math.Log(DuurMinuten / 60) + 0.0051 * Math.Log(DuurMinuten / 60) ^ 2
                    Case "upper"
                        Return 0.39 - 0.0587 * Math.Log(DuurMinuten / 60) + 0.0052 * Math.Log(DuurMinuten / 60) ^ 2
                    Case Else
                        Return -999
                End Select

            Case 2050
                Select Case Scenario
                    Case "GL"
                        Select Case Corner
                            Case "lower"
                                Return 0.377 - 0.0577 * Math.Log(DuurMinuten / 60) + 0.0053 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "center"
                                Return 0.384 - 0.0589 * Math.Log(DuurMinuten / 60) + 0.0054 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "upper"
                                Return 0.391 - 0.06 * Math.Log(DuurMinuten / 60) + 0.0055 * Math.Log(DuurMinuten / 60) ^ 2
                        End Select
                    Case "GH"
                        Select Case Corner
                            Case "lower"
                                Return 0.374 - 0.0563 * Math.Log(DuurMinuten / 60) + 0.0051 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "center"
                                Return 0.382 - 0.0574 * Math.Log(DuurMinuten / 60) + 0.0051 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "upper"
                                Return 0.39 - 0.0586 * Math.Log(DuurMinuten / 60) + 0.0052 * Math.Log(DuurMinuten / 60) ^ 2
                        End Select
                    Case "WL"
                        Select Case Corner
                            Case "lower"
                                Return 0.375 - 0.0557 * Math.Log(DuurMinuten / 60) + 0.0049 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "center"
                                Return 0.386 - 0.0572 * Math.Log(DuurMinuten / 60) + 0.005 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "upper"
                                Return 0.398 - 0.0591 * Math.Log(DuurMinuten / 60) + 0.0052 * Math.Log(DuurMinuten / 60) ^ 2
                        End Select
                    Case "WH"
                        Select Case Corner
                            Case "lower"
                                Return 0.4 - 0.0698 * Math.Log(DuurMinuten / 60) + 0.0064 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "center"
                                Return 0.416 - 0.0728 * Math.Log(DuurMinuten / 60) + 0.0066 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "upper"
                                Return 0.432 - 0.0755 * Math.Log(DuurMinuten / 60) + 0.0069 * Math.Log(DuurMinuten / 60) ^ 2
                        End Select
                End Select

            Case 2085
                Select Case Scenario
                    Case "GL"
                        Select Case Corner
                            Case "lower"
                                Return 0.377 - 0.0553 * Math.Log(DuurMinuten / 60) + 0.005 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "center"
                                Return 0.386 - 0.0563 * Math.Log(DuurMinuten / 60) + 0.0051 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "upper"
                                Return 0.394 - 0.0572 * Math.Log(DuurMinuten / 60) + 0.0052 * Math.Log(DuurMinuten / 60) ^ 2
                        End Select
                    Case "GH"
                        Select Case Corner
                            Case "lower"
                                Return 0.384 - 0.0559 * Math.Log(DuurMinuten / 60) + 0.0046 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "center"
                                Return 0.395 - 0.0572 * Math.Log(DuurMinuten / 60) + 0.0047 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "upper"
                                Return 0.405 - 0.0584 * Math.Log(DuurMinuten / 60) + 0.0047 * Math.Log(DuurMinuten / 60) ^ 2
                        End Select
                    Case "WL"
                        Select Case Corner
                            Case "lower"
                                Return 0.374 - 0.0581 * Math.Log(DuurMinuten / 60) + 0.0053 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "center"
                                Return 0.398 - 0.0612 * Math.Log(DuurMinuten / 60) + 0.0055 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "upper"
                                Return 0.423 - 0.0657 * Math.Log(DuurMinuten / 60) + 0.0059 * Math.Log(DuurMinuten / 60) ^ 2
                        End Select
                    Case "WH"
                        Select Case Corner
                            Case "lower"
                                Return 0.391 - 0.0654 * Math.Log(DuurMinuten / 60) + 0.0055 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "center"
                                Return 0.415 - 0.0681 * Math.Log(DuurMinuten / 60) + 0.0056 * Math.Log(DuurMinuten / 60) ^ 2
                            Case "upper"
                                Return 0.435 - 0.0702 * Math.Log(DuurMinuten / 60) + 0.0056 * Math.Log(DuurMinuten / 60) ^ 2
                        End Select
                End Select
        End Select
        Return -999
    End Function

    Public Function GEVShapeParBasisstatistiek2015(ByVal DuurMinuten As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String) As Double
        If DuurMinuten / 60 <= 240 Then
            Return -0.09 + 0.017 * DuurMinuten / 60 / 24
        Else
            Return 0
        End If
    End Function

    Public Function GEVDispCoefBasisstatistiek2019LangeDuur(ByVal DuurMinuten As Integer) As Double
        Return 0.478 - 0.0681 * Math.Log10(DuurMinuten)
    End Function

    Public Function GEVScaleParBasisstatistiek2019LangeDuur(ByVal DuurMinuten As Integer) As Double
        Return GEVLocparBasisstatistiek2019LangeDuur(DuurMinuten) * GEVDispCoefBasisstatistiek2019LangeDuur(DuurMinuten)
    End Function

    Public Function GEVScaleParBasisstatistiek2019LangeDuurWinter(ByVal DuurMinuten As Integer) As Double
        Return GEVLocparBasisstatistiek2019LangeDuurWinter(DuurMinuten) * GEVDispCoefBasisstatistiek2019LangeDuurWinter(DuurMinuten)
    End Function

    Public Function GEVShapeParBasisstatistiek2019LangeDuur(ByVal DuurMinuten As Integer) As Double
        Return 0.118 - 0.266 * Math.Log10(DuurMinuten) + 0.0586 * Math.Log10(DuurMinuten) ^ 2
    End Function

    Public Function STOWA2015_2018_JAARROND_T(ByVal DuurMinuten As Integer, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String, ByVal Volume As Double) As Double
        Dim locpar As Double, scalepar As Double, shapepar As Double, dispcoef As Double
        Dim P As Double

        If DuurMinuten > 720 Then
            dispcoef = GEVDispCoefBasisstatistiek2015(DuurMinuten, Zichtjaar, Scenario, Corner)
            locpar = GEVLocParBasisstatistiek2015(DuurMinuten, Zichtjaar, Scenario, Corner)
            scalepar = dispcoef * locpar
            shapepar = GEVShapeParBasisstatistiek2015(DuurMinuten, Zichtjaar, Scenario)
            P = GEVCDF(locpar, scalepar, shapepar, Volume)
        Else
            Dim adjustedVolume As Double = Volume / 1.02
            dispcoef = GLODispCoefBasisstatistiek2018(DuurMinuten)
            locpar = GLOLocParBasisstatistiek2018(DuurMinuten)
            scalepar = dispcoef * locpar
            shapepar = GLOShapeParBasisStatistiek2018(DuurMinuten)
            P = GLOCDF(locpar, scalepar, shapepar, adjustedVolume)
        End If

        Return 1 / -Math.Log(P)
    End Function

    Public Function STOWA2015_JAARROND_V(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Dim P As Double = Math.Exp(-1 / T)

        Dim dispcoef As Double = GEVDispCoefBasisstatistiek2015(DuurMinuten, Zichtjaar, Scenario, Corner)
        Dim locpar As Double = GEVLocParBasisstatistiek2015(DuurMinuten, Zichtjaar, Scenario, Corner)
        Dim scalepar As Double = dispcoef * locpar
        Dim shapepar As Double = GEVShapeParBasisstatistiek2015(DuurMinuten, Zichtjaar, Scenario)

        Return GEVINVERSE(locpar, scalepar, shapepar, P)
    End Function

    Public Function STOWA2015_2018_JAARROND_V(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Dim locpar As Double, scalepar As Double, shapepar As Double, dispcoef As Double
        Dim P As Double = Math.Exp(-1 / T)

        If DuurMinuten > 720 Then
            dispcoef = GEVDispCoefBasisstatistiek2015(DuurMinuten, Zichtjaar, Scenario, Corner)
            locpar = GEVLocParBasisstatistiek2015(DuurMinuten, Zichtjaar, Scenario, Corner)
            scalepar = dispcoef * locpar
            shapepar = GEVShapeParBasisstatistiek2015(DuurMinuten, Zichtjaar, Scenario)
            Return GEVINVERSE(locpar, scalepar, shapepar, P)
        Else
            dispcoef = GLODispCoefBasisstatistiek2018(DuurMinuten)
            locpar = GLOLocParBasisstatistiek2018(DuurMinuten)
            scalepar = dispcoef * locpar
            shapepar = GLOShapeParBasisStatistiek2018(DuurMinuten)
            Return GLOINVERSE(locpar, scalepar, shapepar, P) * 1.02
        End If
    End Function

    Public Function STOWA2019_JAARROND_V(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String, Optional ByVal Debugging As Boolean = False) As Double
        Dim P As Double = Math.Exp(-1 / T)
        Dim Volume As Double
        Dim locpar As Double, scalepar As Double, shapepar As Double, dispcoef As Double

        ' Calculate base volume
        If DuurMinuten > 720 Then
            dispcoef = GEVDispCoefBasisstatistiek2019LangeDuur(DuurMinuten)
            locpar = GEVLocparBasisstatistiek2019LangeDuur(DuurMinuten)
            scalepar = dispcoef * locpar
            shapepar = GEVShapeParBasisstatistiek2019LangeDuur(DuurMinuten)
            Volume = GEVINVERSE(locpar, scalepar, shapepar, P)
        Else
            dispcoef = GLODispCoefBasisstatistiek2019KorteDuur(DuurMinuten)
            locpar = GLOLocparBasisstatistiek2019KorteDuur(DuurMinuten)
            scalepar = dispcoef * locpar
            shapepar = GLOShapeParBasisstatistiek2019KorteDuur(DuurMinuten, T)
            Volume = GLOINVERSE(locpar, scalepar, shapepar, P)
        End If

        If Debugging Then
            Console.WriteLine("DuurMinuten: " & DuurMinuten)
            Console.WriteLine("dispcoef: " & dispcoef)
            Console.WriteLine("locpar: " & locpar)
            Console.WriteLine("scalepar: " & scalepar)
            Console.WriteLine("shapepar: " & shapepar)
            Console.WriteLine("Volume: " & Volume)
        End If

        ' Apply climate scenario adjustments if not current climate
        If Zichtjaar = 2014 Then Return Volume

        Dim KorteDuurMultiplier As Double = STOWA2019_KORTEDUUR_MULTIPLIER1(Zichtjaar, Scenario, Corner)
        Dim LangeDuurMultiplier As Double = STOWA2019_LANGEDUUR_MULTIPLIER(DuurMinuten, T, Zichtjaar, Scenario, Corner)
        Dim Multiplier As Double

        If DuurMinuten <= 120 Then
            Multiplier = KorteDuurMultiplier
        ElseIf DuurMinuten < 1440 Then
            LangeDuurMultiplier = STOWA2019_LANGEDUUR_MULTIPLIER(1440, T, Zichtjaar, Scenario, Corner)
            Multiplier = Setup.GeneralFunctions.Interpolate(120, KorteDuurMultiplier, 1440, LangeDuurMultiplier, DuurMinuten)
        Else
            Multiplier = LangeDuurMultiplier
        End If

        Return Volume * Multiplier
    End Function


    Public Function STOWA2019_LANGEDUUR_MULTIPLIER(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Return STOWA2015_JAARROND_V(DuurMinuten, T, Zichtjaar, Scenario, Corner) / STOWA2015_JAARROND_V(DuurMinuten, T, 2014, "", "")
    End Function

    Public Function STOWA2019_MULTIPLIER_WINTER(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Return STOWA2015_WINTER_V(DuurMinuten, T, Zichtjaar, Scenario, Corner) / STOWA2015_WINTER_V(DuurMinuten, T, 2014, "", "")
    End Function

    Public Function STOWA2019_KORTEDUUR_MULTIPLIER2(ByVal DuurMinuten As Integer, ByVal T As Double, ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Select Case Zichtjaar
            Case 2030
                Select Case Corner
                    Case "lower", "upper"
                        Return STOWA2015_JAARROND_V(DuurMinuten, T, Zichtjaar, Scenario, Corner) /
                               STOWA2015_JAARROND_V(DuurMinuten, T, 2014, "", "")
                    Case Else
                        Return 1
                End Select

            Case 2050
                If Scenario = "GH" AndAlso Corner = "lower" Then
                    Return STOWA2015_JAARROND_V(DuurMinuten, T, Zichtjaar, "GL", "lower") /
                           STOWA2015_JAARROND_V(DuurMinuten, T, 2014, "", "")
                ElseIf Scenario = "WL" AndAlso Corner = "upper" Then
                    Return STOWA2015_JAARROND_V(DuurMinuten, T, Zichtjaar, "WH", "upper") /
                           STOWA2015_JAARROND_V(DuurMinuten, T, 2014, "", "")
                Else
                    Return 1
                End If

            Case 2085
                If Scenario = "GH" AndAlso Corner = "lower" Then
                    Return STOWA2015_JAARROND_V(DuurMinuten, T, Zichtjaar, "GL", "lower") /
                           STOWA2015_JAARROND_V(DuurMinuten, T, 2014, "", "")
                ElseIf Scenario = "WL" AndAlso Corner = "upper" Then
                    Return STOWA2015_JAARROND_V(DuurMinuten, T, Zichtjaar, "WL", "upper") /
                           STOWA2015_JAARROND_V(DuurMinuten, T, 2014, "", "")
                Else
                    Return 1
                End If

            Case Else
                Return 1
        End Select
    End Function


    Public Function STOWA2019_KORTEDUUR_MULTIPLIER1(ByVal Zichtjaar As Integer, ByVal Scenario As String, ByVal Corner As String) As Double
        Select Case Zichtjaar
            Case 2030
                Select Case Corner
                    Case "lower"
                        Return 1.0385
                    Case "upper"
                        Return 1.077
                    Case Else
                        Return 1
                End Select

            Case 2050
                If Scenario = "GH" AndAlso Corner = "lower" Then
                    Return 1.0385
                ElseIf Scenario = "WL" AndAlso Corner = "upper" Then
                    Return 1.2125
                Else
                    Return 1
                End If

            Case 2085
                If Scenario = "GH" AndAlso Corner = "lower" Then
                    Return 1.064
                ElseIf Scenario = "WL" AndAlso Corner = "upper" Then
                    Return 1 + ((3.5 - 0.3) / 3.5 * 45) / 100
                Else
                    Return 1
                End If

            Case Else
                Return 1
        End Select
    End Function

    Public Function GLOLocparBasisstatistiek2019KorteDuur(ByVal DuurMinuten As Integer) As Double
        Return 1.02 * (7.339 + 0.848 * Math.Log10(DuurMinuten) + 2.844 * Math.Log10(DuurMinuten) ^ 2)
    End Function

    Public Function GLODispCoefBasisstatistiek2019KorteDuur(ByVal DuurMinuten As Integer) As Double
        If DuurMinuten <= 104 Then
            Return 0.04704 + 0.1978 * Math.Log10(DuurMinuten) - 0.05729 * Math.Log10(DuurMinuten) ^ 2
        Else
            Return 0.2801 - 0.0333 * Math.Log10(DuurMinuten)
        End If
    End Function





End Class
