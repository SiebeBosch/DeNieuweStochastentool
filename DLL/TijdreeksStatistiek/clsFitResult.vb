Public Class clsFitResult
    Public Threshold As Double
    Public dist As clsProbabilityDistribution

    Public Sub New(th As Double, myDist As clsProbabilityDistribution)
        Threshold = th
        dist = myDist
    End Sub

End Class
