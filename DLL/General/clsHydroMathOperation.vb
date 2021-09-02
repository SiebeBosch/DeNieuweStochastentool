Imports STOCHLIB.General
Public Class clsHydroMathOperation
    Public Representation As String
    Public nArgs As Integer
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, myRepresentation As String, mynArgs As Integer)
        Setup = mySetup
        Representation = myRepresentation
        nArgs = mynArgs
    End Sub

    Public Function EvaluateOneParameterOperation(ParVal As Double, ObjectID As String, Optional ByVal TempDir As String = "", Optional ByVal XCoord As Double = 0, Optional ByVal YCoord As Double = 0) As Double
        If Double.IsNaN(ParVal) Then
            Return Double.NaN
        Else
            Select Case Representation.Trim.ToUpper
                Case "ABS"
                    Return Math.Abs(ParVal)
            End Select

        End If
    End Function

    Public Function EvaluateTwoParametersOperation(LeftParVal As Double, RightParVal As Double, ObjectID As String, Optional ByVal TempDir As String = "", Optional ByVal XCoord As Double = 0, Optional ByVal YCoord As Double = 0) As Double

        'if the previous calls resulted in Double.NaN values, there are limits to what we can do
        If Double.IsNaN(LeftParVal) OrElse Double.IsNaN(RightParVal) Then
            'one of the two parameters are NaN, so nothing we can do here
            Return Double.NaN
        Else
            Select Case Representation.Trim.ToUpper
                Case "AVG"
                    Return (LeftParVal + RightParVal) / 2
                Case "MAX"
                    Return Math.Max(LeftParVal, RightParVal)
                Case "MIN"
                    Return Math.Min(LeftParVal, RightParVal)
                Case "SUM"
                    Return LeftParVal + RightParVal
                Case "DIFF"
                    Return LeftParVal - RightParVal
                Case "DTM"
                    If TempDir = "" Then Throw New Exception("Error evaluating two parameters operation DTM: a temporary work directory has not been provided for this operation.")
                    If System.IO.File.Exists(Me.Setup.GISData.DTMDataSource.GetPrimaryDatasourcePath) Then
                        'elevation grid has been specified and exists. retrieve our percentiles
                        Dim Percentiles As New List(Of Double)
                        Percentiles.Add(RightParVal)
                        Dim Results As New List(Of Double)
                        If Me.Setup.GISData.DTMDataSource.GridPercentileFromCircle(ObjectID, XCoord, YCoord, LeftParVal, Percentiles, 20, TempDir, Results) Then
                            If Results.Count > 0 Then
                                Return Results(0)
                            Else
                                Me.Setup.Log.AddError("Unable to retrieve percentile value from elevation grid for circle around " & ObjectID & ".")
                                Return Double.NaN
                            End If
                        End If
                    Else
                        'no elevation grid specified. Return NaN
                        Return Double.NaN
                    End If
            End Select
        End If
    End Function


End Class
