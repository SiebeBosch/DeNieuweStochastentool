Imports STOCHLIB.General
Public Class cls1D2DLink
    Private Setup As clsSetup

    Friend Cell2DIdx As Integer
    Friend MeshNode1DIdx As Integer

    Friend MeshNode1D As cls1DMeshNode
    Friend CellCenter2D As clsXY

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

End Class
