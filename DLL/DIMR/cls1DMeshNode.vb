Public Class cls1DMeshNode
    Public ID As String
    Friend Idx As Integer
    Friend Chainage As Double
    Friend X As Double
    Friend Y As Double
    Friend Branch As cls1DBranch

    Public Sub New(myID As String, myIdx As Integer, myChainage As Double, ByRef myBranch As cls1DBranch)
        ID = myID
        Idx = myIdx
        Chainage = myChainage
        Branch = myBranch
    End Sub



End Class
