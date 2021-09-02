Public Class clsPIDSetting
    Public Kp As Double
    Public Ki As Double
    Public Kd As Double

    Public Sub New(myKp As Double, myKi As Double, myKd As Double)
        Kp = myKp
        Ki = myKi
        Kd = myKd
    End Sub

    Public Sub New()

    End Sub

End Class
