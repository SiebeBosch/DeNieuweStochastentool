Imports STOCHLIB.General

Public Class clsRRBoundaryNode
    Inherits clsRRNodeTPRecord
    Friend NodeTpRecord As clsRRNodeTPRecord
    Public Q As List(Of Decimal)

    Public Sub New(ByRef mySetup As clsSetup, ByVal iId As String, ByVal iName As String, ByVal iX As Decimal, ByVal iY As Decimal, ByVal iNodeType As clsSobekNodeType)
    MyBase.New(mySetup, iId, iName, iX, iY, iNodeType)
  End Sub
End Class
