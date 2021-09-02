Option Explicit On

Public Class clsCFSettings

  Public LateralPrefix As String
  Public PrecipLateralPrefix As String
  Public EvapLateralPrefix As String
  Public SkipReachPrefixes As New Specialized.StringCollection

  Public Sub Initialize()

  End Sub

  Friend Sub setLateralPrefix(ByVal myPrefix As String)
    LateralPrefix = myPrefix
  End Sub
  Friend Function getLateralPrefix() As String
    Return LateralPrefix
  End Function

End Class
