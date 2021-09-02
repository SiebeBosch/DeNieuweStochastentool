Public Class clsCFReachElement
  Public ID As String
  Public name As String
  Public x As Decimal
  Public y As Decimal
  Public ReachID As String
  Public ReachDist As Decimal

  Public NodeType As clsSobekNodeType
  Public Tim As List(Of Double)

  Public Sub New(ByVal iId As String, ByVal iName As String, ByVal iX As Decimal, ByVal iY As Decimal, ByVal iNodeType As clsSobekNodeType)
    ID = iId
    name = iName
    x = iX
    y = iY
    NodeType = iNodeType
  End Sub
End Class
