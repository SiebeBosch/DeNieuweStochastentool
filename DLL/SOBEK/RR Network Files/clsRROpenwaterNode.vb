Imports STOCHLIB.General
Public Class clsRROpenwaterNode
    Inherits clsRRNodeTPRecord
    Public RRData As clsOpenwate3BRecord
    Public WaterLevel As New List(Of clsSobekResult)
    Public Volume As New List(Of clsSobekResult)
    'Public Seepage As New List(Of clsSobekResult)
    'Public Rainfall As New List(Of clsSobekResult)
    Public RestTerm As New List(Of Decimal)

    Friend NodeTpRecord As clsRRNodeTPRecord

    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup, ByVal iId As String, ByVal iName As String, ByVal iX As Decimal, ByVal iY As Decimal, ByVal iNodeType As clsSobekNodeType)
        MyBase.New(mySetup, iId, iName, iX, iY, iNodeType)
        Setup = mySetup
    End Sub

End Class
