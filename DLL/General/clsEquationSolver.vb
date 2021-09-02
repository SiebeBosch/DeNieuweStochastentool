Imports STOCHLIB.General

Public Class clsEquationSolver

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    'Code:
    ' An equation solver class.
    ' Probably not really quick, but it's all VBasic code.
    '
    ' It does a significant amount of work in the ' parsing of an equation, so it's more efficient ' when solving the same equation several times.
    '
    ' The equation is not case sensitive.
    '
    '
    ' 1-1-96: A Bug related to determining the difference between
    '         a negative sign and negation was fixed. (And a priority
    '         level PRI_NEG was added.)  - TPA
    '

    'Error defines for clsEquation

    Const EQ_PAREN = 1100     ' Unbalanced parenthesis
    Const EQ_FUNCTION = 1101  ' Unknown function:
    Const EQ_VARIABLE = 1102  ' Unknown variable:
    Const EQ_INVALID = 1103   ' Invalid Equation
    Const EQ_ARGS = 1104      ' Invalids arguments to function:
    Const EQ_NAME = 1105      ' Unable to add an unnamed function:

    Private Dirty As Boolean
    Private Parsed As Boolean

    Private Vars As New Collection
    Private Equ As String
    Private degrees As Boolean

    Private dAnswer As Double
    Private EquParsed As Collection   'The parsed equation
    Private EquOrder As Collection   'Order in which to solve the equation


    ' Constants used in parsing
    ' Priority levels
    Private Const PRI_ADD = 1
    Private Const PRI_MOD = 2
    Private Const PRI_MUL = 3
    Private Const PRI_NEG = 4
    Private Const PRI_EXP = 5
    Private Const PRI_VAR = 6
    Private Const PRI_PAR = 7
    Private Const PRI_LEVEL = 7

    Private Const EQ_NONE = 0
    Private Const EQ_STRING = 1
    Private Const EQ_NUMBER = 2

    Private Const ER_NONE = 0
    Private Const ER_VAR = 1

    Private Const pi = 3.14159265358979
    Private Const DEG_TO_RAD = 0.01745329251995
    Private Const RAD_TO_DEG = 57.2957795131

    Public Sub SetDegrees(b As Boolean)
        If b <> degrees Then
            degrees = b
            Dirty = True
        End If
    End Sub

    Public Function GetDegrees() As Boolean
        Return degrees
    End Function

    Private Function GetRight(ByVal j As Long, v() As Object) As Long
        Dim i As Long
        For i = j + 1 To v.Count - 1
            If Not v(i) Is Nothing Then
                GetRight = i
                Exit Function
            End If
        Next i
        GetRight = 0
    End Function

    Private Function GetLeft(ByVal j As Long, v() As Object) As Long
        Dim i As Long
        For i = j - 1 To 0 Step -1
            If Not v(i) Is Nothing Then
                GetLeft = i
                Exit Function
            End If
        Next i
        GetLeft = 0
    End Function

    Public Sub VarClear()
        Vars = New Collection
        Dirty = True
    End Sub

    Public Sub LetEquation(e As String)
        Parsed = False
        Dirty = True
        Equ = e.ToLower
    End Sub

    Public Function GetEquation() As String
        Return Equ
    End Function

    Private Sub Parse()
        Dim i As Integer
        Dim s As String
        Dim t As Integer
        Dim j As Integer
        Dim sTmp As String
        Dim P As Integer
        Dim EquPriority As New Collection
        Dim maxPriority
        Dim isNeg As Boolean

        s = ""
        t = EQ_NONE
        j = 1
        P = 0
        isNeg = False
        EquParsed = New Collection

        EquParsed.Add("")
        EquPriority.Add("")
        maxPriority = PRI_LEVEL

        For i = 1 To Len(Equ)
            sTmp = Mid$(Equ, i, 1)

            Select Case sTmp
                Case "A" To "Z", "a" To "z", "_"
                    If t = EQ_NONE Then
                        t = EQ_STRING
                        s = sTmp
                    ElseIf t = EQ_NUMBER Then
                        t = EQ_STRING
                        EquParsed.Add(s, , j)
                        EquPriority.Add(0, , j)
                        j = j + 1
                        EquParsed.Add("*", , j)
                        EquPriority.Add(PRI_MUL + P, , j)
                        j = j + 1
                        s = sTmp
                    Else
                        s = s + sTmp
                    End If
                    isNeg = True

                Case "1" To "9", "0", "."
                    If t = EQ_NONE Then
                        t = EQ_NUMBER
                        s = sTmp
                    Else
                        s = s + sTmp
                    End If
                    isNeg = True

                Case "("
                    If t = EQ_STRING Then
                        EquParsed.Add(s + sTmp, , j)
                        EquPriority.Add(P + PRI_PAR, , j)
                        j = j + 1
                        s = ""
                    ElseIf t = EQ_NUMBER Then
                        EquParsed.Add(s, , j)
                        EquPriority.Add(0, , j)
                        j = j + 1
                        EquParsed.Add("*", , j)
                        EquPriority.Add(P + PRI_MUL, , j)
                        j = j + 1
                        EquParsed.Add(sTmp, , j)
                        EquPriority.Add(P + PRI_PAR, , j)
                        j = j + 1
                        s = ""
                    Else
                        EquParsed.Add(sTmp, , j)
                        EquPriority.Add(P + PRI_PAR, , j)
                        j = j + 1
                    End If

                    P = P + PRI_LEVEL
                    t = EQ_NONE

                    If maxPriority < P + PRI_LEVEL Then
                        maxPriority = P + PRI_LEVEL
                    End If
                    isNeg = False

                Case "*", "/"
                    If t <> EQ_NONE Then
                        EquParsed.Add(s, , j)
                        EquPriority.Add(IIf(t = EQ_STRING, P + PRI_VAR, 0), , j)
                        j = j + 1
                        s = ""
                    End If

                    EquParsed.Add(sTmp, , j)
                    EquPriority.Add(P + PRI_MUL, , j)
                    j = j + 1
                    t = EQ_NONE
                    isNeg = False

                Case "\"
                    If t <> EQ_NONE Then
                        EquParsed.Add(s, , j)
                        EquPriority.Add(IIf(t = EQ_STRING, P + PRI_VAR, 0), , j)
                        j = j + 1
                        s = ""
                    End If

                    EquParsed.Add(sTmp, , j)
                    EquPriority.Add(P + PRI_MUL, , j)
                    j = j + 1
                    t = EQ_NONE
                    isNeg = False

                Case "+"
                    If t <> EQ_NONE Then
                        EquParsed.Add(s, , j)
                        EquPriority.Add(IIf(t = EQ_STRING, P + PRI_VAR, 0), , j)
                        j = j + 1
                        s = ""
                        EquParsed.Add(sTmp, , j)
                        EquPriority.Add(P + PRI_ADD, , j)
                        j = j + 1
                        t = EQ_NONE
                    Else
                        'Ignore things like "(+1)"
                    End If
                    isNeg = False

                Case "-"
                    If t <> EQ_NONE Then
                        EquParsed.Add(s, , j)
                        EquPriority.Add(IIf(t = EQ_STRING, P + PRI_VAR, 0), , j)
                        j = j + 1
                        s = ""
                    End If

                    If isNeg Then
                        EquParsed.Add(sTmp, , j)
                        EquPriority.Add(P + PRI_ADD, , j)
                        j = j + 1
                        t = EQ_NONE
                    Else
                        EquParsed.Add("~", , j)
                        EquPriority.Add(P + PRI_NEG, , j)
                        j = j + 1
                        t = EQ_NONE
                    End If

                    isNeg = False

                Case "^"
                    If t <> EQ_NONE Then
                        EquParsed.Add(s, , j)
                        EquPriority.Add(IIf(t = EQ_STRING, P + PRI_VAR, 0), , j)
                        j = j + 1
                        s = ""
                    End If

                    EquParsed.Add(sTmp, , j)
                    EquPriority.Add(P + PRI_EXP, , j)
                    j = j + 1
                    t = EQ_NONE
                    isNeg = False

                Case "%"
                    If t <> EQ_NONE Then
                        EquParsed.Add(s, , j)
                        EquPriority.Add(IIf(t = EQ_STRING, P + PRI_VAR, 0), , j)
                        j = j + 1
                        s = ""
                    End If

                    EquParsed.Add(sTmp, , j)
                    EquPriority.Add(P + PRI_MOD, , j)
                    j = j + 1
                    t = EQ_NONE
                    isNeg = False

                Case ","
                    If t <> EQ_NONE Then
                        EquParsed.Add(s, , j)
                        EquPriority.Add(IIf(t = EQ_STRING, P + PRI_VAR, 0), , j)
                        j = j + 1
                        s = ""
                    End If

                    EquParsed.Add(Nothing, , j)
                    EquPriority.Add(0, , j)
                    j = j + 1
                    t = EQ_NONE
                    isNeg = False

                Case ")"
                    If t <> EQ_NONE Then
                        EquParsed.Add(s, , j)
                        EquPriority.Add(IIf(t = EQ_STRING, P + PRI_VAR, 0), , j)
                        j = j + 1
                        s = ""
                    End If

                    EquParsed.Add(sTmp, , j)
                    EquPriority.Add(P - (PRI_LEVEL - PRI_PAR), , j)
                    P = P - PRI_LEVEL
                    j = j + 1
                    t = EQ_NONE
                    isNeg = True
            End Select
        Next i

        If s <> "" Then
            EquParsed.Add(s, , j)
            EquPriority.Add(IIf(t = EQ_STRING, P + PRI_VAR, 0), , j)
            j = j + 1
        End If

        EquParsed.Remove(j)
        EquPriority.Remove(j)

        If P <> 0 Then
            Err.Raise(EQ_PAREN, "clsEquation", "Unbalanced parenthesis")
            Exit Sub
        End If

        ' Debugging section...
        'For i = 1 To EquParsed.Count
        '   Debug.Print EquParsed(i) & ";";
        'Next i
        'Debug.Print
        '   For i = 1 To EquPriority.Count
        '   Debug.Print EquPriority(i) & ";";
        'Next i
        'Debug.Print
        'Debug.Print "MaxPriority = " & maxPriority
        ' End Debugging section....

        EquOrder = New Collection
        EquOrder.Add("")

        For j = 0 To maxPriority - 1
            For i = EquPriority.Count - 1 To 0 Step -1
                If EquPriority(i) = j Then
                    EquOrder.Add(i, , , 1)
                End If
            Next i
        Next j

        EquOrder.Remove(0)

        'For i = 1 To EquOrder.Count
        '   Debug.Print EquOrder(i) & ";";
        'Next i
        'Debug.Print

        Parsed = True
    End Sub

    Public Sub VarRemove(Name As String)
        On Error Resume Next
        Vars.Remove(Name)
        Dirty = True
    End Sub

    Public Function Solution() As Double
        If Dirty Then
            Solve()
        End If

        Solution = dAnswer
    End Function

    Public Sub Solve()
        Dim i As Long
        Dim l As Long
        Dim r As Long
        Dim m As Long
        Dim n As Long
        Dim X As Double
        Dim Y As Double
        Dim v As Object
        Dim eSpace As Integer
        Dim Temp() As Object

        Try
            If Not Parsed Then
                Parse()
            End If

            ' Copy the equation to a working array
            ReDim Temp(0 To EquParsed.Count - 1)

            For i = 1 To EquParsed.Count
                Temp(i) = EquParsed(i)
            Next

            eSpace = ER_NONE

            ' Solve the equation
            For i = 1 To EquOrder.Count
                'Debug.Print "Pro -> " & EquOrder(i) & " = ";
                'For j2 = 1 To UBound(Temp)
                '   Debug.Print Temp(j2) & ";";
                'Next j2
                'Debug.Print

                m = EquOrder(i)
                v = Temp(m)

                Select Case v
         ' Standard operators
                    Case "~"  'Negative operator (inserted by the parser)
                        r = GetRight(m, Temp)
                        Temp(m) = -CDbl(Temp(r))
                        Temp(r) = Nothing

                    Case "*"
                        l = GetLeft(m, Temp)
                        r = GetRight(m, Temp)
                        Temp(l) = CDbl(Temp(l)) * CDbl(Temp(r))
                        Temp(r) = Nothing
                        Temp(m) = Nothing

                    Case "/"
                        l = GetLeft(m, Temp)
                        r = GetRight(m, Temp)
                        Temp(l) = CDbl(Temp(l)) / CDbl(Temp(r))
                        Temp(r) = Nothing
                        Temp(m) = Nothing

                    Case "\"
                        l = GetLeft(m, Temp)
                        r = GetRight(m, Temp)
                        Temp(l) = CDbl(Temp(l)) \ CDbl(Temp(r))
                        Temp(r) = Nothing
                        Temp(m) = Nothing

                    Case "+"
                        l = GetLeft(m, Temp)
                        r = GetRight(m, Temp)
                        Temp(l) = CDbl(Temp(l)) + CDbl(Temp(r))
                        Temp(r) = Nothing
                        Temp(m) = Nothing

                    Case "-"
                        l = GetLeft(m, Temp)
                        r = GetRight(m, Temp)
                        Temp(l) = CDbl(Temp(l)) - CDbl(Temp(r))
                        Temp(r) = Nothing
                        Temp(m) = Nothing

                    Case "^"
                        l = GetLeft(m, Temp)
                        r = GetRight(m, Temp)
                        Temp(l) = CDbl(Temp(l)) ^ CDbl(Temp(r))
                        Temp(r) = Nothing
                        Temp(m) = Nothing

                    Case "%"
                        l = GetLeft(m, Temp)
                        r = GetRight(m, Temp)
                        Temp(l) = CDbl(Temp(l)) Mod CDbl(Temp(r))
                        Temp(r) = Nothing
                        Temp(m) = Nothing

                    Case "("
                        i = i + 1
                        n = EquOrder(i)
                        r = GetRight(m, Temp)
                        If r >= n Then
                            Temp(m) = 0#
                            Temp(n) = Nothing
                        Else
                            Temp(m) = Temp(r)
                            Temp(r) = Nothing
                            Temp(n) = Nothing
                        End If

                    Case Else
                        If Right$(Temp(m), 1) = "(" Then
                            'Must be a function
                            i = i + 1
                            n = EquOrder(i)

                            l = GetRight(m, Temp)
                            r = GetLeft(n, Temp)

                            If l >= n Then
                                Err.Raise(EQ_ARGS, "clsEquation", "Invalid arguments to function: " & v & ")")
                                Exit Sub
                            Else
                                X = CDbl(Temp(l))
                            End If

                            If r <= m Then
                                Err.Raise(EQ_ARGS, "clsEquation", "Invalid arguments to function: " & v & ")")
                                Exit Sub
                            Else
                                Y = CDbl(Temp(r))
                            End If

                            Temp(r) = Nothing
                            Temp(l) = Nothing
                            Temp(m) = Nothing
                            Temp(n) = Nothing

                            Select Case v
                  ' Standard functions
                                Case "abs("
                                    Temp(m) = Math.Abs(X)

                                Case "atn("
                                    If degrees Then
                                        Temp(m) = Math.Atan(X) * RAD_TO_DEG
                                    Else
                                        Temp(m) = Math.Atan(X)
                                    End If

                                Case "arctan("
                                    If degrees Then
                                        Temp(m) = Math.Atan(X) * RAD_TO_DEG
                                    Else
                                        Temp(m) = Math.Atan(X)
                                    End If

                                Case "cos("
                                    If degrees Then
                                        Temp(m) = Math.Cos(X * DEG_TO_RAD)
                                    Else
                                        Temp(m) = Math.Cos(X)
                                    End If

                                Case "exp("
                                    Temp(m) = Math.Exp(X)

                                Case "fix("
                                    Temp(m) = Fix(X)

                                Case "int("
                                    Temp(m) = Int(X)

                                Case "log("
                                    Temp(m) = Math.Log(X)

                                Case "rnd("
                                    Temp(m) = Rnd(X)

                                Case "sgn("
                                    Temp(m) = Math.Sign(X)

                                Case "sin("
                                    If degrees Then
                                        Temp(m) = Math.Sin(X * DEG_TO_RAD)
                                    Else
                                        Temp(m) = Math.Sin(X)
                                    End If

                                Case "sqr("
                                    Temp(m) = Math.Sqrt(X)

                                Case "tan("
                                    If degrees Then
                                        Temp(m) = Math.Tan(X * DEG_TO_RAD)
                                    Else
                                        Temp(m) = Math.Tan(X)
                                    End If

                     ' 2 variable functions
                                Case "min("
                                    Temp(m) = IIf(X < Y, X, Y)

                                Case "max("
                                    Temp(m) = IIf(X > Y, X, Y)

                                Case "random("
                                    Temp(m) = (Rnd() * (Y - X)) + X

                                Case "mod("
                                    Temp(m) = X Mod Y

                                Case "logn("
                                    Temp(m) = Math.Log(X) / Math.Log(Y)

                     ' Misc equations
                                Case "rand("
                                    Temp(m) = Int(Rnd() * X)

                     ' Derived functions
                                Case "sec("
                                    If degrees Then
                                        Temp(m) = (1 / Math.Cos(X * DEG_TO_RAD))
                                    Else
                                        Temp(m) = 1 / Math.Cos(X)
                                    End If

                                Case "cosec("
                                    If degrees Then
                                        Temp(m) = (1 / Math.Sin(X * DEG_TO_RAD))
                                    Else
                                        Temp(m) = 1 / Math.Sin(X)
                                    End If

                                Case "cotan("
                                    If degrees Then
                                        Temp(m) = (1 / Math.Tan(X * DEG_TO_RAD))
                                    Else
                                        Temp(m) = 1 / Math.Tan(X)
                                    End If

                                Case "arcsin("
                                    If degrees Then
                                        Temp(m) = (Math.Atan(X / Math.Sqrt(-X * X + 1))) * RAD_TO_DEG
                                    Else
                                        Temp(m) = Math.Atan(X / Math.Sqrt(-X * X + 1))
                                    End If

                                Case "arccos("
                                    If degrees Then
                                        Temp(m) = (Math.Atan(-X / Math.Sqrt(-X * X + 1)) + 2 * Math.Atan(1)) * RAD_TO_DEG
                                    Else
                                        Temp(m) = Math.Atan(-X / Math.Sqrt(-X * X + 1)) + 2 * Math.Atan(1)
                                    End If

                                Case "arcsec("
                                    If degrees Then
                                        Temp(m) = (Math.Atan(X / Math.Sqrt(X * X - 1)) + (Math.Sign(X) - 1) * (2 * Math.Atan(1))) * RAD_TO_DEG
                                    Else
                                        Temp(m) = Math.Atan(X / Math.Sqrt(X * X - 1)) + (Math.Sign(X) - 1) * (2 * Math.Atan(1))
                                    End If

                                Case "arccosec("
                                    If degrees Then
                                        Temp(m) = (Math.Atan(X / Math.Sqrt(X * X - 1)) + (Math.Sign(X) - 1) * (2 * Math.Atan(1))) * RAD_TO_DEG
                                    Else
                                        Temp(m) = Math.Atan(X / Math.Sqrt(X * X - 1)) + (Math.Sign(X) - 1) * (2 * Math.Atan(1))
                                    End If

                                Case "arccotan("
                                    If degrees Then
                                        Temp(m) = (Math.Atan(X * DEG_TO_RAD) + 2 * Math.Atan(1)) * RAD_TO_DEG
                                    Else
                                        Temp(m) = Math.Atan(X) + 2 * Math.Atan(1)
                                    End If

                                Case "sinh("
                                    Temp(m) = (Math.Exp(X) - Math.Exp(-X)) / 2

                                Case "cosh("
                                    Temp(m) = (Math.Exp(X) - Math.Exp(-X)) / (Math.Exp(X) + Math.Exp(-X))

                                Case "tanh("
                                    Temp(m) = (Math.Exp(X) - Math.Exp(-X)) / (Math.Exp(X) + Math.Exp(-X))

                                Case "sech("
                                    Temp(m) = 2 / (Math.Exp(X) + Math.Exp(-X))

                                Case "cosech("
                                    Temp(m) = 2 / (Math.Exp(X) - Math.Exp(-X))

                                Case "cotanh("
                                    Temp(m) = (Math.Exp(X) + Math.Exp(-X)) / (Math.Exp(X) - Math.Exp(-X))

                                Case "arcsinh("
                                    Temp(m) = Math.Log(X + Math.Sqrt(X * X + 1))

                                Case "arccosh("
                                    Temp(m) = Math.Log(X + Math.Sqrt(X * X - 1))

                                Case "arctanh("
                                    Temp(m) = Math.Log((1 + X) / (1 - X)) / 2

                                Case "arcsech("
                                    Temp(m) = Math.Log((Math.Sqrt(-X * X + 1) + 1) / X)

                                Case "arccosech("
                                    Temp(m) = Math.Log((Math.Sign(X) * Math.Sqrt(X * X + 1) + 1) / X)

                                Case "arccotanh("
                                    Temp(m) = Math.Log((X + 1) / (X - 1)) / 2

                                Case "log10("
                                    Temp(m) = Math.Log(X) / Math.Log(10)

                                Case "log2("
                                    Temp(m) = Math.Log(X) / Math.Log(2)

                                Case "ln("    'A macro to Log
                                    Temp(m) = Math.Log(X)

                     ' conversion functions
                                Case "deg("   ' Radians to degrees
                                    Temp(m) = X * RAD_TO_DEG

                                Case "rad("   ' Degrees to radians
                                    Temp(m) = X * DEG_TO_RAD

                                Case Else
                                    Err.Raise(EQ_FUNCTION, "clsEquation", "Undefined Function: " & v & ")")
                                    Exit Sub
                            End Select
                        Else
                            'Must be a variable
                            Select Case v
                                Case "pi"
                                    Temp(m) = pi

                                Case "e"
                                    Temp(m) = 2.718281828

                                Case "rnd"
                                    Temp(m) = Rnd()

                                Case Else
                                    eSpace = ER_VAR
                                    Temp(m) = CDbl(Vars(Temp(m)))
                                    eSpace = ER_NONE
                            End Select
                        End If
                End Select
            Next i

            dAnswer = CDbl(Temp(GetRight(0, Temp)))
            Dirty = False
            Exit Sub
        Catch ex As Exception
            Setup.Log.AddError("Error in Function Solve of clsEquationSolver")
        End Try

        'Edit: siebe: remvoed the error sovler since it was a mess after translating to VB.NET
        'SolveError:

        '  Select Case Err()
        ''Overflow, division by 0, internal errors...
        '      Case Is = '6', '11', EQ_PAREN To EQ_NAME
        '          'Err.Raise(Err, "clsEquation", Err.Description)
        '      Case Is = '5'
        '          Select Case eSpace
        '              Case Is = ER_VAR
        '          Err.Raise(EQ_VARIABLE, "clsEquation", "Undefined Variable:" & v)
        '              Case Else
        '          'Err.Raise(Err, "clsEquation", Err.Description)
        '  End Select
        '      Case Else
        '          Err.Raise(EQ_INVALID, "clsEquation", "Invalid Equation")
        '  End Select
    End Sub

    Public Function GetVar(Name As String) As Double
        Try
            Return CDbl(Vars(Name))
        Catch ex As Exception
            Return 0#
        End Try
    End Function

    Public Sub LetVar(Name As String, Num As Double)
        On Error Resume Next
        Dirty = True
        Vars.Remove(Name)
        Vars.Add(Num, Name)
    End Sub

    Private Sub Class_Initialize()
        Dirty = False
        Parsed = True
        degrees = False
    End Sub

End Class
