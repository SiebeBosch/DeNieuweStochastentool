Public Class clsTemporalPatternBranches
    'this class describes all the branches on ONE level inside a tree
    Dim BlockSize As Integer 'the size of each branch on this level

    Public Sub New(EventDuration As Integer, Divider As Integer)
        BlockSize = EventDuration / Divider

    End Sub

End Class
