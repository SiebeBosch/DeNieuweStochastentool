Imports STOCHLIB.General
Public Class cls1DNode
    Private Setup As clsSetup
    Friend ID As String
    Friend X As Double
    Friend Y As Double
    Public Sub New(ByRef mySetup As clsSetup, myID As String)
        Setup = mySetup
        ID = myID
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myID As String, myX As Double, myY As Double)
        Setup = mySetup
        ID = myID
        X = myX
        Y = myY
    End Sub

End Class
