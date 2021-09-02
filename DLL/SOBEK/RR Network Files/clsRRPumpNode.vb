Imports STOCHLIB.General
Public Class clsRRPumpNode
    Inherits clsRRNodeTPRecord
    Public RRData As clsStruct3BRecord
    Friend NodeTpRecord As clsRRNodeTPRecord

    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup, ByVal iId As String, ByVal iName As String, ByVal iX As Decimal, ByVal iY As Decimal, ByVal iNodeType As clsSobekNodeType)
        MyBase.New(mySetup, iId, iName, iX, iY, iNodeType)
        Setup = mySetup
        RRData = New clsStruct3BRecord(Me.Setup)
    End Sub


End Class
