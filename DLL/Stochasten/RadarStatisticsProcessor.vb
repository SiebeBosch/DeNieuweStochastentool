Public Class RadarStatisticsProcessor
    ' Class to hold the results
    Public Class ProcessingResults
        Public Property AreaSize As Double
        Public Property TSeries As Double()
        Public Property XSeries As Double()
        Public Property ResultMatrix As Double(,)  ' 2D array for results
        Public Property STOWA2019_5_73km2 As Double(,)  ' For the 5.73 km² special case
    End Class

    Public Function ProcessAreaStatistics(
        ByVal area As Double,
        ByVal T_series_Langbein As Double(),
        ByVal x_series As Double(),
        ByVal fittedParams As FitResults,
        Optional ByVal Aref As Double = 1.0) As ProcessingResults

        Dim results As New ProcessingResults With {
            .AreaSize = area,
            .TSeries = T_series_Langbein,
            .XSeries = x_series,
            .ResultMatrix = New Double(T_series_Langbein.Length - 1, x_series.Length - 1) {},
            .STOWA2019_5_73km2 = New Double(T_series_Langbein.Length - 1, x_series.Length - 1) {}
        }

        ' Constants from original code
        Const C As Double = 82
        Const D1 As Double = 24

        For i As Integer = 0 To T_series_Langbein.Length - 1
            Dim T As Double = T_series_Langbein(i)

            For j As Integer = 0 To x_series.Length - 1
                Dim x As Double = x_series(j)

                ' Check initial conditions
                If x >= 0.5 OrElse (x < 0.5 AndAlso area <= 968.37) Then
                    ' Calculate location parameter
                    Dim locAref, loc As Double
                    If x <= C Then
                        locAref = CalculateLocationParameterLessOrEqualC(x, Aref, fittedParams.LocationParameters, C)
                        loc = CalculateLocationParameterLessOrEqualC(x, area, fittedParams.LocationParameters, C)
                    Else
                        locAref = CalculateLocationParameterGreaterThanC(x, Aref, fittedParams.LocationParameters, C)
                        loc = CalculateLocationParameterGreaterThanC(x, area, fittedParams.LocationParameters, C)
                    End If

                    ' Calculate dispersion
                    Dim dispAref = CalculateDispersion(x, Aref, fittedParams.DispersionParameters)
                    Dim disp = CalculateDispersion(x, area, fittedParams.DispersionParameters)

                    ' Calculate shape
                    Dim shapeAref, shape As Double
                    If x < D1 Then
                        shapeAref = CalculateShapeLessThanD1(x, Aref, fittedParams.ShapeParameters, D1)
                        shape = CalculateShapeLessThanD1(x, area, fittedParams.ShapeParameters, D1)
                    Else
                        shapeAref = CalculateShapeGreaterOrEqualD1(x, fittedParams.ShapeParameters, D1)
                        shape = CalculateShapeGreaterOrEqualD1(x, fittedParams.ShapeParameters, D1)
                    End If

                    ' Calculate final results
                    Dim R_ref, R As Double
                    If T <= 50 Then
                        R_ref = CalculateFinalResult(locAref, dispAref, shapeAref, T)
                        R = CalculateFinalResult(loc, disp, shape, T)
                    Else
                        ' For T > 50, use T = 50 in calculations
                        R_ref = CalculateFinalResult(locAref, dispAref, shapeAref, 50)
                        R = CalculateFinalResult(loc, disp, shape, 50)
                    End If

                    ' Calculate ARF factors
                    Dim ARFpunt_5punt73km2 = 1 - 0.08266513 * Math.Pow(x, -0.289186)
                    Dim ARF = R / R_ref

                    ' Store final results
                    results.ResultMatrix(i, j) = ARFpunt_5punt73km2 * ARF
                    results.STOWA2019_5_73km2(i, j) = ARFpunt_5punt73km2
                Else
                    results.ResultMatrix(i, j) = Double.NaN
                    results.STOWA2019_5_73km2(i, j) = Double.NaN
                End If
            Next
        Next

        Return results
    End Function

    ' Helper calculation methods
    Private Function CalculateLocationParameterLessOrEqualC(x As Double, area As Double, params As Double(), C As Double) As Double
        Return params(0) * Math.Pow(x, params(1)) +
               (params(2) + params(3) * Math.Log(x / C)) * Math.Pow(area, params(4)) +
               params(5)
    End Function

    Private Function CalculateLocationParameterGreaterThanC(x As Double, area As Double, params As Double(), C As Double) As Double
        Return params(0) * Math.Pow(x, params(1)) +
               params(2) * Math.Pow(area, params(4)) +
               params(3) * Math.Pow(area, params(6)) * (x - C) +
               params(5)
    End Function

    Private Function CalculateDispersion(x As Double, area As Double, params As Double()) As Double
        Return params(0) +
               params(1) * Math.Log(x) +
               params(2) * Math.Log(area)
    End Function

    Private Function CalculateShapeLessThanD1(x As Double, area As Double, params As Double(), D1 As Double) As Double
        Return params(0) +
               params(1) * Math.Log(area) * (Math.Log(D1) - Math.Log(x))
    End Function

    Private Function CalculateShapeGreaterOrEqualD1(x As Double, params As Double(), D1 As Double) As Double
        Return params(0) +
               params(2) * (x - D1)
    End Function

    Private Function CalculateFinalResult(loc As Double, disp As Double, shape As Double, T As Double) As Double
        Return loc * (1 + disp * (1 - Math.Pow(T, -1 * shape)) / shape)
    End Function
End Class
