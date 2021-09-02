Public Class clsSobekResult

  Public DateVal As Double
  Public Value As Single
  Public Pos As Single
  Public Neg As Single

  Public Sub New(ByVal iDateVal As Double, ByVal iValue As Single, Optional ByVal SplitPosNeg As Boolean = False)
    DateVal = iDateVal
    Value = iValue
    If SplitPosNeg Then
      If iValue > 0 Then
        Pos = iValue
        Neg = 0
      Else
        Pos = 0
        Neg = iValue
      End If
    End If
  End Sub

  Public Sub New()

  End Sub

End Class
