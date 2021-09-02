Public Class clsXY
  Public ID As String
  Public Name As String
  Public X As Double
  Public Y As Double

  Public Sub New()

  End Sub

  Public Sub New(ByRef myX As Double, ByVal myY As Double)
    X = myX
    Y = myY
  End Sub

  Public Sub New(ByVal myId As String, ByVal myName As String, ByVal myX As Double, ByVal myY As Double)
    ID = myId
    Name = myName
    X = myX
    Y = myY
  End Sub
End Class
