Imports MathNet.Numerics.LinearAlgebra
Imports MathNet.Numerics.Optimization
Imports STOCHLIB.General

Public Class RadarStatisticsFitter
    ' Constants
    Private Setup As clsSetup
    Private Const C As Double = 82  ' For location parameter
    Private Const D1 As Double = 24 ' For shape parameter

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    ' Location parameter model
    Private Function LocationModel(parameters As Double(), duur As Double, area As Double) As Double
        Dim a_coef = parameters(0)
        Dim b_coef = parameters(1)
        Dim c_coef = parameters(2)
        Dim d_coef = parameters(3)
        Dim e_coef = parameters(4)
        Dim f_coef = parameters(5)
        Dim g_coef = parameters(6)

        Dim duur2 = If(duur > C, C, duur)
        Dim duur3 = If(duur <= C, C, duur)

        Return a_coef * duur ^ b_coef +
               (c_coef + d_coef * Math.Log(duur2 / C)) * area ^ e_coef +
               d_coef * area ^ g_coef * (duur3 - C) +
               f_coef
    End Function

    ' Shape parameter model
    Private Function ShapeModel(parameters As Double(), duur As Double, area As Double) As Double
        Dim a_coef = parameters(0)
        Dim b_coef = parameters(1)
        Dim c_coef = parameters(2)

        Dim duur2 = If(duur < D1, D1, duur)
        Dim duur3 = If(duur > D1, D1, duur)

        Return a_coef +
               b_coef * Math.Log(area) * (Math.Log(D1) - Math.Log(duur3)) +
               c_coef * (duur2 - D1)
    End Function

    ' Dispersion parameter model
    Private Function DispersionModel(parameters As Double(), duur As Double, area As Double) As Double
        Dim a_coef = parameters(0)
        Dim b_coef = parameters(1)
        Dim d_coef = parameters(2)

        Return a_coef +
               b_coef * Math.Log(duur) +
               d_coef * Math.Log(area)
    End Function

    ' Fit all parameters
    Public Function FitAllParameters(lijst_Duur As Double(),
                               lijst_Area As Double(),
                               lijst_loc As Double(),
                               lijst_kappa As Double(),
                               lijst_disp As Double()) As FitResults
        Try
            Me.Setup.Log.AddMessage("Starting FitAllParameters")
            ' Create empty FitResults object
            Dim results As New FitResults()

            ' Try location fit first
            Me.Setup.Log.AddMessage("Starting Location fit")
            Try
                results.LocationParameters = FitLocation(lijst_Duur, lijst_Area, lijst_loc)
                Me.Setup.Log.AddMessage("Location fit completed successfully")
            Catch ex As Exception
                Me.Setup.Log.AddError("Location fit failed: " & ex.Message)
                results.LocationParameters = Nothing
            End Try

            ' Try shape fit
            Me.Setup.Log.AddMessage("Starting Shape fit")
            Try
                results.ShapeParameters = FitShape(lijst_Duur, lijst_Area, lijst_kappa)
                Me.Setup.Log.AddMessage("Shape fit completed successfully")
            Catch ex As Exception
                Me.Setup.Log.AddError("Shape fit failed: " & ex.Message)
                results.ShapeParameters = Nothing
            End Try

            ' Try dispersion fit
            Me.Setup.Log.AddMessage("Starting Dispersion fit")
            Try
                results.DispersionParameters = FitDispersion(lijst_Duur, lijst_Area, lijst_disp)
                Me.Setup.Log.AddMessage("Dispersion fit completed successfully")
            Catch ex As Exception
                Me.Setup.Log.AddError("Dispersion fit failed: " & ex.Message)
                results.DispersionParameters = Nothing
            End Try

            ' Check if any fit was successful
            If results.LocationParameters Is Nothing AndAlso
           results.ShapeParameters Is Nothing AndAlso
           results.DispersionParameters Is Nothing Then
                Me.Setup.Log.AddError("All fits failed")
                Return Nothing
            End If

            Me.Setup.Log.AddMessage("FitAllParameters completed")
            Return results

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FitAllParameters of class RadarStatisticsFitter: " & ex.Message)
            Return Nothing
        End Try
    End Function


    ' Fit location parameter
    Private Function FitLocation(lijst_Duur As Double(), lijst_Area As Double(), lijst_loc As Double()) As Double()
        Try
            Dim initialGuess = Vector(Of Double).Build.Dense({
    24.412378,  ' a_coef
    0.191958,   ' b_coef
    -0.698425,  ' c_coef
    0.104733,   ' d_coef
    0.234975,   ' e_coef
    -9.162435,  ' f_coef
    -0.009387   ' g_coef
})

            Dim objective = Function(parameters As Double()) As Double
                                Dim sumSquaredError As Double = 0
                                For i As Integer = 0 To lijst_loc.Length - 1
                                    Dim predicted = LocationModel(parameters, lijst_Duur(i), lijst_Area(i))
                                    sumSquaredError += (predicted - lijst_loc(i)) ^ 2
                                Next
                                Return sumSquaredError
                            End Function


            Dim result As Double() = OptimizeParameters(objective, initialGuess)

            Debug.Print("parameters after fit: " & String.Join(",", result))

            Return result
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FitLocation of class RadarStatisticsFitter: " & ex.Message)
        End Try

    End Function


    Private Class LocationObjectiveFunction
        Implements IObjectiveFunction
        Implements IObjectiveFunctionEvaluation

        Private _lijst_Duur As Double()
        Private _lijst_Area As Double()
        Private _lijst_loc As Double()
        Private _parent As RadarStatisticsFitter
        Private _currentPoint As Vector(Of Double)

        ' Constructor remains the same
        Public Sub New(lijst_Duur As Double(), lijst_Area As Double(), lijst_loc As Double(), parent As RadarStatisticsFitter)
            _lijst_Duur = lijst_Duur
            _lijst_Area = lijst_Area
            _lijst_loc = lijst_loc
            _parent = parent
        End Sub

        ' Explicit implementation for IObjectiveFunction.CreateNew
        Private Function CreateNewForIObjectiveFunction() As IObjectiveFunction _
    Implements IObjectiveFunction.CreateNew
            Return CreateNewInstance()
        End Function

        ' Keep just the shared logic for creating a new instance
        Private Function CreateNewInstance() As IObjectiveFunction
            Dim clone As New LocationObjectiveFunction(_lijst_Duur, _lijst_Area, _lijst_loc, _parent)
            clone._currentPoint = If(_currentPoint IsNot Nothing, _currentPoint.Clone(), Nothing)
            Return clone
        End Function


        ' Implementation for IObjectiveFunction
        Public Function Fork() As IObjectiveFunction Implements IObjectiveFunction.Fork
            Return New LocationObjectiveFunction(_lijst_Duur, _lijst_Area, _lijst_loc, _parent)
        End Function

        ' Add the Sub version of EvaluateAt
        Public Sub EvaluateAt(point As Vector(Of Double)) Implements IObjectiveFunction.EvaluateAt
            _currentPoint = point
        End Sub ' Custom objective function class

        Public ReadOnly Property Value As Double Implements IObjectiveFunctionEvaluation.Value
            Get
                Dim sumSquaredError As Double = 0
                For i As Integer = 0 To _lijst_loc.Length - 1
                    Dim predicted = _parent.LocationModel(_currentPoint.ToArray(), _lijst_Duur(i), _lijst_Area(i))
                    sumSquaredError += (predicted - _lijst_loc(i)) ^ 2
                Next
                Return sumSquaredError
            End Get
        End Property




        Public ReadOnly Property Point As Vector(Of Double) Implements IObjectiveFunctionEvaluation.Point
            Get
                Return _currentPoint
            End Get
        End Property


        Public ReadOnly Property IsGradientSupported As Boolean Implements IObjectiveFunctionEvaluation.IsGradientSupported
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property Gradient As Vector(Of Double) Implements IObjectiveFunctionEvaluation.Gradient
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public ReadOnly Property IsHessianSupported As Boolean Implements IObjectiveFunctionEvaluation.IsHessianSupported
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property Hessian As Matrix(Of Double) Implements IObjectiveFunctionEvaluation.Hessian
            Get
                Throw New NotImplementedException()
            End Get
        End Property
    End Class


    ' Fit shape parameter
    Private Function FitShape(lijst_Duur As Double(), lijst_Area As Double(), lijst_kappa As Double()) As Double()
        Try
            Dim initialGuess = Vector(Of Double).Build.Dense({
    2.0,    ' a_coef
    0.3,    ' b_coef
    0.4     ' c_coef
})

            Dim objective = Function(parameters As Double()) As Double
                                Dim sumSquaredError As Double = 0
                                For i As Integer = 0 To lijst_kappa.Length - 1
                                    Dim predicted = ShapeModel(parameters, lijst_Duur(i), lijst_Area(i))
                                    sumSquaredError += (predicted - lijst_kappa(i)) ^ 2
                                Next
                                Return sumSquaredError
                            End Function

            Return OptimizeParameters(objective, initialGuess)
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FitShape of class RadarStatisticsFitter: " & ex.Message)
        End Try

    End Function

    ' Fit dispersion parameter
    Private Function FitDispersion(lijst_Duur As Double(), lijst_Area As Double(), lijst_disp As Double()) As Double()
        Try
            Dim initialGuess = Vector(Of Double).Build.Dense({
    2.0,    ' a_coef
    0.3,    ' b_coef
    0.52    ' d_coef
})

            Dim objective = Function(parameters As Double()) As Double
                                Dim sumSquaredError As Double = 0
                                For i As Integer = 0 To lijst_disp.Length - 1
                                    Dim predicted = DispersionModel(parameters, lijst_Duur(i), lijst_Area(i))
                                    sumSquaredError += (predicted - lijst_disp(i)) ^ 2
                                Next
                                Return sumSquaredError
                            End Function

            Return OptimizeParameters(objective, initialGuess)
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FitDispersion of class RadarStatisticsFitter: " & ex.Message)
        End Try

    End Function

    ' First create a class that implements IObjectiveFunction
    Private Class ObjectiveFunctionWrapper
        Implements IObjectiveFunction

        Private _objective As Func(Of Double(), Double)
        Private _value As Double
        Private _point As Vector(Of Double)

        Public Sub New(objective As Func(Of Double(), Double))
            _objective = objective
        End Sub

        Public ReadOnly Property Value As Double Implements IObjectiveFunctionEvaluation.Value
            Get
                Return _value
            End Get
        End Property

        Public ReadOnly Property Point As Vector(Of Double) Implements IObjectiveFunctionEvaluation.Point
            Get
                Return _point
            End Get
        End Property

        Public Sub EvaluateAt(point As Vector(Of Double)) Implements IObjectiveFunction.EvaluateAt
            ' Evaluate and store the point/value
            _point = point
            _value = _objective(point.ToArray())
        End Sub

        Public Function Fork() As IObjectiveFunction Implements IObjectiveFunction.Fork
            Return New ObjectiveFunctionWrapper(_objective)
        End Function

        Public Function CreateNew() As IObjectiveFunction Implements IObjectiveFunction.CreateNew
            Return New ObjectiveFunctionWrapper(_objective)
        End Function

        Public ReadOnly Property IsGradientSupported As Boolean Implements IObjectiveFunctionEvaluation.IsGradientSupported
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property Gradient As Vector(Of Double) Implements IObjectiveFunctionEvaluation.Gradient
            Get
                Throw New NotSupportedException("Gradient is not supported")
            End Get
        End Property

        Public ReadOnly Property IsHessianSupported As Boolean Implements IObjectiveFunctionEvaluation.IsHessianSupported
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property Hessian As Matrix(Of Double) Implements IObjectiveFunctionEvaluation.Hessian
            Get
                Throw New NotSupportedException("Hessian is not supported")
            End Get
        End Property

        Private _lastPoint As Vector(Of Double)
        Private _lastValue As Double
    End Class

    ' Then modify your OptimizeParameters function to use this wrapper
    Private Function OptimizeParameters(objective As Func(Of Double(), Double), initialGuess As Vector(Of Double)) As Double()
        Try
            ' First check if the objective function can be evaluated at initial guess
            Dim testValue = objective(initialGuess.ToArray())
            If Double.IsNaN(testValue) OrElse Double.IsInfinity(testValue) Then
                Me.Setup.Log.AddError("Initial objective function evaluation failed with value: " & testValue)
                Return initialGuess.ToArray()
            End If

            ' Configure solver with correct parameters
            Dim solver = New NelderMeadSimplex(
            convergenceTolerance:=0.00000001,
            maximumIterations:=5000)

            ' Create wrapper for the objective function
            Dim objectiveWrapper = New ObjectiveFunctionWrapper(objective)

            Try
                Dim result = solver.FindMinimum(objectiveWrapper, initialGuess)
                If result.ReasonForExit = ExitCondition.Converged Then
                    Return result.MinimizingPoint.ToArray()
                Else
                    Me.Setup.Log.AddWarning($"Optimization did not fully converge. Reason: {result.ReasonForExit}")
                    Return result.MinimizingPoint.ToArray()
                End If
            Catch ex As Exception
                Me.Setup.Log.AddError("Optimization failed: " & ex.Message)
                Return initialGuess.ToArray()
            End Try
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in OptimizeParameters: " & ex.Message)
            Return initialGuess.ToArray()
        End Try
    End Function




End Class

' Class to hold all fit results
Public Class FitResults
    Public Property LocationParameters As Double()
    Public Property ShapeParameters As Double()
    Public Property DispersionParameters As Double()
End Class