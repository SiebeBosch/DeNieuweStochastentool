Public Class clsSavitzkyGolaySmoothing
  'Svitzky-Golay Smooths a dataset by fitting a 2nd-degree polynomal trough a moving window of datapoints.
  'NOTE: this code originates from VB6 and all arrays are 1-based
  'in order to get this to work in DOTNET we increased the array size to make them 0-based
  'but we'll still use the 1-based values inside, so effectively the 0-values are not used!
  'code from http://www.planetsourcecode.com/vb/scripts/ShowCode.asp?txtCodeId=3051&lngWId=1

  'Dynamic data arrays
  Dim DataX() As Double
  Dim DataY() As Double
  Dim SmoothedY() As Double
  Dim DataI() As Double

  Private Const PI As Double = 3.14159265358979

  Dim NP As Integer
  Dim SmoothCount As Integer

  'The matrix for the Savitzky-Golay Coefficents
  'These are filled in the form load event
  Dim SGCoef(0 To 11, 0 To 13) As Integer

  Public Sub New()

    'Set the Smoothing Coefficients for Savitzky-Golay
    'The zeroth value is the normalization factor

    SGCoef(1, 1) = 17
    SGCoef(1, 2) = 12
    SGCoef(1, 3) = -3
    SGCoef(1, 0) = 35

    SGCoef(2, 1) = 7
    SGCoef(2, 2) = 6
    SGCoef(2, 3) = 3
    SGCoef(2, 4) = -2
    SGCoef(2, 0) = 21

    SGCoef(3, 1) = 59
    SGCoef(3, 2) = 54
    SGCoef(3, 3) = 39
    SGCoef(3, 4) = 14
    SGCoef(3, 5) = -21
    SGCoef(3, 0) = 231

    SGCoef(4, 1) = 89
    SGCoef(4, 2) = 84
    SGCoef(4, 3) = 69
    SGCoef(4, 4) = 44
    SGCoef(4, 5) = 9
    SGCoef(4, 6) = -36
    SGCoef(4, 0) = 429


    SGCoef(5, 1) = 25
    SGCoef(5, 2) = 24
    SGCoef(5, 3) = 21
    SGCoef(5, 4) = 16
    SGCoef(5, 5) = 9
    SGCoef(5, 6) = 0
    SGCoef(5, 7) = -11
    SGCoef(5, 0) = 143

    SGCoef(6, 1) = 167
    SGCoef(6, 2) = 162
    SGCoef(6, 3) = 147
    SGCoef(6, 4) = 122
    SGCoef(6, 5) = 87
    SGCoef(6, 6) = 42
    SGCoef(6, 7) = -13
    SGCoef(6, 8) = -78
    SGCoef(6, 0) = 1105

    SGCoef(7, 1) = 43
    SGCoef(7, 2) = 42
    SGCoef(7, 3) = 39
    SGCoef(7, 4) = 34
    SGCoef(7, 5) = 27
    SGCoef(7, 6) = 18
    SGCoef(7, 7) = 7
    SGCoef(7, 8) = -6
    SGCoef(7, 9) = -21
    SGCoef(7, 0) = 323

    SGCoef(8, 1) = 269
    SGCoef(8, 2) = 264
    SGCoef(8, 3) = 249
    SGCoef(8, 4) = 224
    SGCoef(8, 5) = 189
    SGCoef(8, 6) = 144
    SGCoef(8, 7) = 89
    SGCoef(8, 8) = 24
    SGCoef(8, 9) = -51
    SGCoef(8, 10) = -136
    SGCoef(8, 0) = 2261

    SGCoef(9, 1) = 329
    SGCoef(9, 2) = 324
    SGCoef(9, 3) = 309
    SGCoef(9, 4) = 284
    SGCoef(9, 5) = 249
    SGCoef(9, 6) = 204
    SGCoef(9, 7) = 149
    SGCoef(9, 8) = 84
    SGCoef(9, 9) = 9
    SGCoef(9, 10) = -76
    SGCoef(9, 11) = -171
    SGCoef(9, 0) = 3059

    SGCoef(10, 1) = 79
    SGCoef(10, 2) = 78
    SGCoef(10, 3) = 75
    SGCoef(10, 4) = 70
    SGCoef(10, 5) = 63
    SGCoef(10, 6) = 54
    SGCoef(10, 7) = 43
    SGCoef(10, 8) = 30
    SGCoef(10, 9) = 15
    SGCoef(10, 10) = -2
    SGCoef(10, 11) = -21
    SGCoef(10, 12) = -42
    SGCoef(10, 0) = 806

    SGCoef(11, 1) = 467
    SGCoef(11, 2) = 462
    SGCoef(11, 3) = 447
    SGCoef(11, 4) = 422
    SGCoef(11, 5) = 387
    SGCoef(11, 6) = 322
    SGCoef(11, 7) = 287
    SGCoef(11, 8) = 222
    SGCoef(11, 9) = 147
    SGCoef(11, 10) = 62
    SGCoef(11, 11) = -33
    SGCoef(11, 12) = -138
    SGCoef(11, 13) = -253
    SGCoef(11, 0) = 5135

  End Sub

  Public Function Calculate(ByVal XVals() As Double, ByVal YVals() As Double, ByVal deg As Integer, ByVal Cumulative As Boolean) As Double()
    Dim i As Long

    'NOTE: ALL ARRAYS INSIDE THIS CLASS ARE 1-BASED, SO CONVERT THE INCOMING ARRAYS FIRST
    ReDim DataX(0 To XVals.Count) 'we won't be using the data in slot 0
    ReDim DataY(0 To YVals.Count) 'we won't be using the data in slot 0
    ReDim SmoothedY(0 To YVals.Count) 'we won't be using the data in slot 0
    Dim Result(0 To XVals.Count - 1) As Double

    NP = XVals.Count 'set the number of points

    'convert the incoming values to a (virtual) 1-based format
    For i = 0 To XVals.Count - 1
      DataX(i + 1) = XVals(i)
      DataY(i + 1) = YVals(i)
    Next

    'perform the Savitzky-Golay smoothing
    Call SavGolSmooth(deg, Cumulative)

    'translate the result back into a 0-based array
    For i = 0 To XVals.Count - 1
      Result(i) = SmoothedY(i + 1)
    Next
    Return Result

  End Function

  Public Sub SavGolSmooth(ByVal Degree As Integer, ByVal CumulativeSmooth As Boolean)
    'Savitzky_Golay Smoothing
    'If SmoothCurrent is set to true, then the last smoothed data set will be smoothed,
    '(i.e. The new smoothing will be cumulative over the last smoothing operation.  If
    'SmoothCurrent is false, then the original Y-data will be smoothed, and the smoothedY array will
    'be overwritten

    'The Aavitzky-Golay smoothing algorithm essentialy fits the data to a second order polynomial
    'within a moving data window.  It assumes that the data has a fixed spacing in the x direction,
    'but does work even if this is not the case.

    'For more info see:
    '"Smoothing and Differentiation of Data by Simplified Least Squares Procedure",
    'Abraham Savitzky and Marcel J. E. Golay, Analytical Chemistry, Vol. 36, No. 8, Page 1627 (1964)

    'Degree 1 = 3 point
    'Degree 2 = 5 point
    'Degree 3 = 7 point
    'Degree 4 = 9 point
    'Degree 5 = 11 point ... etc

    Dim i As Integer, J As Integer
    Dim TempSum As Double
    On Error Resume Next

    'Logging the data is useful if the data is always above zero, and spans
    'several orders of magnitude

    If CumulativeSmooth Then
      For i = 1 To NP
        If SmoothedY(i) <> 0 Then
          SmoothedY(i) = Math.Log(SmoothedY(i))
        Else
          SmoothedY(i) = 0.000001
        End If
      Next i
    Else
      For i = 1 To NP
        If DataY(i) <> 0 Then
          DataY(i) = Math.Log(DataY(i))
        Else
          DataY(i) = 0.000001
        End If
      Next i
    End If

    If CumulativeSmooth = False Then
      'we cannot smooth too close to the data bounds
      For i = 1 To Degree
        SmoothedY(i) = DataY(i)
      Next i
      For i = NP - (Degree + 1) To NP
        SmoothedY(i) = DataY(i)
      Next i

      SmoothCount = 0
      For i = 1 + Degree To NP - Degree
        TempSum = DataY(i) * SGCoef(Degree - 1, 1)
        For J = 1 To Degree
          TempSum = TempSum + DataY(i - J) * (SGCoef(Degree - 1, J + 1))
          TempSum = TempSum + DataY(i + J) * (SGCoef(Degree - 1, J + 1))
        Next J
        SmoothedY(i) = TempSum / SGCoef(Degree - 1, 0)
      Next i

    Else
      'The last smoothed data will be used to create a new smoothed data set,
      'therefore the smoothing operations will be additive

      For i = 1 + Degree To NP - Degree
        TempSum = SmoothedY(i) * SGCoef(Degree - 1, 1)
        For J = 1 To Degree
          TempSum = TempSum + SmoothedY(i - J) * (SGCoef(Degree - 1, J + 1))
          TempSum = TempSum + SmoothedY(i + J) * (SGCoef(Degree - 1, J + 1))

        Next J
        SmoothedY(i) = TempSum / SGCoef(Degree - 1, 0)
      Next i
    End If

    If CumulativeSmooth Then
      For i = 1 To NP
        SmoothedY(i) = Math.Exp(SmoothedY(i))
      Next
    Else
      For i = 1 To NP
        DataY(i) = Math.Exp(DataY(i))
        SmoothedY(i) = Math.Exp(SmoothedY(i))
      Next
    End If

    SmoothCount = SmoothCount + 1

  End Sub
End Class
