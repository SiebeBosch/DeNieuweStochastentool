Public Class clsNetworkNTWRRCluster
    Dim ID As String
    Dim SourceNode As clsNetworkNTWNode
    Dim SinkNode As clsNetworkNTWNode
    Dim Link As clsNetworkNTWReachSegment

    Public Sub New(ByRef mySourceNode As clsNetworkNTWNode, ByRef mySinkNode As clsNetworkNTWNode, ByRef myLink As clsNetworkNTWReachSegment)
        ID = myLink.getID
        SourceNode = mySourceNode
        SinkNode = mySinkNode
        Link = myLink
    End Sub
End Class
