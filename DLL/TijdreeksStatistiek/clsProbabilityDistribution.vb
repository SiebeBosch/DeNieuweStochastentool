Imports STOCHLIB.General
Public Class clsProbabilityDistribution
    Private Setup As clsSetup
    Public DistributionType As GeneralFunctions.EnmProbabilityDistribution
    Dim Title As String
    Dim LocPar As Double
    Dim ScalePar As Double
    Dim ShapePar As Double
    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Function IsValid() As Boolean
        If LocPar <> 0 OrElse ScalePar <> 0 OrElse ShapePar <> 0 Then
            Return True
        Else
            Return False
        End If
    End Function


    Public Function ParamCount() As Integer
        Select Case DistributionType
            Case GeneralFunctions.EnmProbabilityDistribution.GEV
                Return 3
            Case GeneralFunctions.EnmProbabilityDistribution.GumbelMax
                Return 2
            Case GeneralFunctions.EnmProbabilityDistribution.Exponential
                Return 2
            Case GeneralFunctions.EnmProbabilityDistribution.LogNormal
                Return 2
            Case GeneralFunctions.EnmProbabilityDistribution.GenPareto
                Return 3
        End Select
    End Function

    Public Function inverse(p As Double) As Double
        Select Case DistributionType
            Case GeneralFunctions.EnmProbabilityDistribution.GumbelMax
                Return GumbelMaxInverse(p)
            Case GeneralFunctions.EnmProbabilityDistribution.GEV
                Return 0
            Case GeneralFunctions.EnmProbabilityDistribution.GenPareto
                Return GenParInverse(p)
            Case GeneralFunctions.EnmProbabilityDistribution.Exponential
                Return 0
            Case GeneralFunctions.EnmProbabilityDistribution.LogNormal
                Return 0
        End Select
    End Function

    Public Function GumbelMaxInverse(p_ond As Double) As Double
        Return LocPar - ScalePar * Math.Log(-Math.Log(p_ond))
    End Function

    Public Function GenParInverse(p_ond As Double) As Double

        'CDF: F(x) = 1 - (1 + k*(x-mu)/sigma)^-1/k
        '1 - F(x) = (1 + k*(x-mu)/sigma)^-1/k
        '(1-F(x))^-k = 1 + k*(x-mu)/sigma
        '(1-F(x))^-k - 1 = k*(x-mu)/sigma
        'sigma * ((1-F(x))^-k -1)/k = x - mu
        'x = mu + sigma * (1-F(x))^-k -1)/k

        Return LocPar + ScalePar * ((1 - p_ond) ^ -ShapePar - 1) / ShapePar

        ''------------------------------------------------------------------------------------------------
        ''Datum: 25-7-2018
        ''Auteur: Siebe Bosch
        ''Deze routine berekent de waarde X gegeven de ONDERschrijdingskans p volgens Generalized Pareto
        ''Cumulatieve kansdichtheidsfunctie: F(x) = 1-(1+kz)^-1/k waarin:
        ''z = (x-mu)/sigma
        ''LET OP: het is niet gelukt om deze formule te inverteren, dus we lossen het iteratief op
        ''------------------------------------------------------------------------------------------------

        ''we weten op welke onderschrijdingskans we willen uitkomen en zoeken daarbij de X
        ''laten we X iteratief zoeken tussen mu - 10sigma en mu + 10sigma
        'Dim iIter As Integer, iSlice As Integer
        'Dim Xmin As Double, Xmax As Double, Slice As Double
        'Dim Xcur As Double, Pcur As Double, Pbest As Double, BestSlice As Integer, Xbest As Double

        'Xmin = LocPar - 10 * ScalePar
        'Xmax = LocPar + 10 * ScalePar
        'Pbest = -1

        'For iIter = 1 To 10
        '    'split the range in ten slices
        '    Slice = (Xmax - Xmin) / 10
        '    For iSlice = 1 To 10
        '        Xcur = Xmin + (iSlice - 0.5) * Slice
        '        Pcur = GenParCDF(Xcur)
        '        If Math.Abs(Pcur - p_ond) < Math.Abs(Pbest - p_ond) Then
        '            Pbest = Pcur
        '            Xbest = Xcur
        '            BestSlice = iSlice
        '        End If
        '    Next

        '    'narrow down the search window and move on to the next iteration
        '    'build in some extra security by surrounding slices in the next iteration
        '    Xmin = Xbest - 2 * Slice
        '    Xmax = Xbest + 2 * Slice

        'Next

        'return Xbest

    End Function

    Public Function GenParCDF(X As Double) As Double
        'calculates the cumulative probability density according to the Generalized Pareto probability distribution
        Dim par As Double
        par = (X - LocPar) / ScalePar

        If ShapePar = 0 Then
            Return 1 - Math.Exp(-par)
        Else
            Return 1 - (1 + ShapePar * par) ^ (-1 / ShapePar)
        End If
    End Function


    Public Sub SetTitle(myTitle As String)
        Title = myTitle
    End Sub

    Public Sub SetLocPar(myPar As Double)
        LocPar = myPar
    End Sub

    Public Sub SetScalePar(myPar As Double)
        ScalePar = myPar
    End Sub

    Public Sub SetShapePar(myPar As Double)
        ShapePar = myPar
    End Sub

    Public Function GetLocPar() As Double
        Return LocPar
    End Function

    Public Function GetScalePar() As Double
        Return ScalePar
    End Function

    Public Function GetShapePar() As Double
        Return ShapePar
    End Function

End Class
