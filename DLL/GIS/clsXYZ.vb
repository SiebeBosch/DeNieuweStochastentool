Public Class clsXYZ
    Public ID As String
    Public X As Double
    Public Y As Double
    Public Z As Double

    Public Sub New()

    End Sub

    Public Sub New(ByVal myX As Double, ByVal myY As Double, ByVal myZ As Double)
        X = myX
        Y = myY
        Z = myZ
    End Sub

    Public Sub New(ByVal myID As String, ByVal myX As Double, ByVal myY As Double, ByVal myZ As Double)
        ID = myID
        X = myX
        Y = myY
        Z = myZ
    End Sub


End Class
