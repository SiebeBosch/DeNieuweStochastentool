Public Class clsResultsLocation
  Dim ID As String
  Dim myAlias As String
  Dim myPar As String
  Dim Type As STOCHLIB.GeneralFunctions.enmValueStatistic

  Public Sub New(ByVal iID As String, ByVal iAlias As String, ByVal iPar As String, ByVal iType As STOCHLIB.GeneralFunctions.enmValueStatistic)
    ID = iID
    myAlias = iAlias
    myPar = iPar
    Type = iType
  End Sub

End Class
