Imports STOCHLIB.General
Public Class clsRRGreenhouseNode

    Friend ID As String
    Friend Name As String   'This contains the actual name for the sewage area, so use it as the key for the CSO-locations
    Friend Area As Double
    Friend X As Double
    Friend Y As Double
    Friend InUse As Boolean

    Friend NodeTpRecord As clsRRNodeTPRecord
    Public Greenhse3BRecord As clsGreenhse3BRecord
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        NodeTpRecord = New clsRRNodeTPRecord(Me.Setup)
        Greenhse3BRecord = New clsGreenhse3BRecord(Me.Setup)
    End Sub

End Class
