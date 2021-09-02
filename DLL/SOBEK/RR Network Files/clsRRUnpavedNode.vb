Imports STOCHLIB.General
Public Class ClsRRUnpavedNode

    Friend ID As String
    Friend Name As String                        'This contains the actual name for the sewage area, so use it as the key for the CSO-locations
    Friend Area As Double
    Friend X As Double
    Friend Y As Double
    Friend InUse As Boolean

    'A record of all the files that are used to represent this node type
    Friend NodeTpRecord As New clsRRNodeTPRecord(Me.setup)
    Friend UnPaved3BRecord As New clsUnpaved3BRecord(Me.setup)
    Friend UnpavedAlfERNSRecord As New clsUnpavedALFERNSRecord(Me.setup)
    Friend UnpavedSepRecord As New clsUnpavedSEPRecord(Me.setup)
    Friend UnpavedStoRecord As New clsUnpavedSTORecord(Me.setup)

    Private setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        setup = mySetup
    End Sub

End Class

