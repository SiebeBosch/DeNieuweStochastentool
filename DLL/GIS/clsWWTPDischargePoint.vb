Imports STOCHLIB.General
Public Class clsWWTPDischargePoint
    Friend ID As String
    Friend X As Double
    Friend Y As Double
    Friend nt As GeneralFunctions.enmNodetype
    Private Setup As clsSetup

    Public InUse As Boolean

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

End Class
