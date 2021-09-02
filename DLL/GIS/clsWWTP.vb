Imports STOCHLIB.General
Public Class clsWWTP
    Friend ID As String
    Friend X As Double
    Friend Y As Double
    Friend Boundary As clsWWTPDischargePoint
    Private Setup As clsSetup

    Public InUse As Boolean

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Function BuildBound3BRecord(ByVal Value As Double) As clsBound3B3BRecord
        Dim B3B As New clsBound3B3BRecord(Me.Setup)
        B3B.ID = Boundary.ID
        B3B.bl = 0
        B3B.bl2 = Value
        B3B.is_ = 0
        Return B3B
    End Function

    Public Function BuildWWTP3BRecord() As clsWWTP3BRecord
        Dim W3B As New clsWWTP3BRecord(Me.Setup)
        W3B.ID = ID
        W3B.tb = 0
        W3B.InUse = True
        Return W3B
    End Function

End Class
