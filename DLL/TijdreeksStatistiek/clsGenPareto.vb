Public Class clsGenPareto
    Dim mu As Double 'mu
    Dim s As Double  'sigma
    Dim k As Double  'kappa

    Public Sub SetParameters(myk As Double, mys As Double, mymu As Double)
        mu = mymu
        s = mys
        k = myk
    End Sub

    Public Function getMu() As Double
        Return mu
    End Function

    Public Function getS() As Double
        Return s
    End Function

    Public Function getk() As Double
        Return k
    End Function


End Class
