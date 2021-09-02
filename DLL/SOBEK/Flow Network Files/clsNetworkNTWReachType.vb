Public Class clsNetworkNTWReachType
    Dim TypeNum As Integer
    Dim ReachType As String
    Dim ParentType As String

    Public Sub New(myTypeNum As Integer, ParentTypeStr As String, TypeStr As String)
        TypeNum = myTypeNum
        ReachType = TypeStr
        ParentType = ParentTypeStr
    End Sub

    Public Function getParentReachType()
        Return ParentType
    End Function

End Class
