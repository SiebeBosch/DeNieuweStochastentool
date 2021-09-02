Public Class clsEmbankment
  Friend LeftValue As Double
  Friend RightValue As Double
  Friend GeneralValue As Double

  Public Sub setGeneral(ByVal myVal As Double)
    GeneralValue = myval
  End Sub

  Public Function getGeneral() As Double
    Return GeneralValue
  End Function

  Public Sub setLeft(ByVal myVal As Double)
    LeftValue = myVal
  End Sub

  Public Function getLeft() As Double
    Return LeftValue
  End Function

  Public Sub setRight(ByVal myval As Double)
    RightValue = myval
  End Sub

  Public Function getRight() As Double
    Return RightValue
  End Function

End Class
