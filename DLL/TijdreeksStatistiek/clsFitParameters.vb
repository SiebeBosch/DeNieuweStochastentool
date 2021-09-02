Public Class clsFitParameters
    Dim Loc As Double       'location parameter mu
    Dim Scale As Double     'scale parameter sigma
    Dim Shape As Double     'shape parameter teta
    Dim MLE As Double       'maximum likelihood estimation value
    Dim AIC As Double       'akaike information criterion
    Dim KS As Double        'Kolmogorov-Smirnov goodness-of-fit test result
    Dim Threshold As Double 'threshold value (only for POT-distributions)

    Public Function getMLE() As Double
        Return MLE
    End Function
    Public Function getAIC() As Double
        Return AIC
    End Function
    Public Function getKS() As Double
        Return KS
    End Function
    Public Function getLoc() As Double
        Return Loc
    End Function
    Public Function getScale() As Double
        Return Scale
    End Function
    Public Function getShape() As Double
        Return Shape
    End Function
    Public Function getThreshold() As Double
        Return Threshold
    End Function
    Public Sub setLoc(myLoc As Double)
        Loc = myLoc
    End Sub
    Public Sub setScale(myScale As Double)
        Scale = myScale
    End Sub
    Public Sub setShape(myShape As Double)
        Shape = myShape
    End Sub
    Public Sub setMLE(myMLE As Double)
        MLE = myMLE
    End Sub
    Public Sub setAIC(myAIC As Double)
        AIC = myAIC
    End Sub
    Public Sub setKS(myKS As Double)
        ks = myKS
    End Sub
End Class
