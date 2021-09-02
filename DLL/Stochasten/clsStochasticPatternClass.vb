Public Class clsStochasticPatternClass

    Public Patroon As STOCHLIB.GeneralFunctions.enmNeerslagPatroon
    Public p As Double

    Public Sub New(ByVal myPatroon As String, ByVal myP As Double)
        Patroon = DirectCast([Enum].Parse(GetType(STOCHLIB.GeneralFunctions.enmNeerslagPatroon), myPatroon), STOCHLIB.GeneralFunctions.enmNeerslagPatroon)
        p = myP
    End Sub

End Class
