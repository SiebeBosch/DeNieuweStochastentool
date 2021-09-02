Imports STOCHLIB.General

Public Class clsSbkPolygonIntersection
    Inherits clsSbkReachObject
    Friend FromPolyIdx As Integer
    Friend ToPolyIdx As Integer

    Public Sub New(ByRef mySetup As clssetup, ByRef myCase As clsSobekCase)
        MyBase.New(mySetup, myCase)
    End Sub

    Public Sub SetFromPolyIdx(myIdx As Integer)
        FromPolyIdx = myIdx
    End Sub
    Public Sub SetToPolyIdx(myIdx As Integer)
        ToPolyIdx = myIdx
    End Sub

End Class
