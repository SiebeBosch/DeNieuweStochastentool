Public Class clsNetworkNTWNode
    Dim ID As String
    Dim Name As String
    Dim X As Double
    Dim Y As Double
    Dim nt As GeneralFunctions.enmNodetype
    'Dim NODETYPE As clsNetworkNTWNodeType

    'optional variables
    Dim PolygonIdx As Integer           'optional. used for quick spatial analysis

    Private NetworkNTW As clsNetworkNTW

    Public Function GetID() As String
        Return ID
    End Function

    Public Function getX() As Double
        Return X
    End Function

    Public Function getY() As Double
        Return Y
    End Function

    Public Sub setPolygonIdx(Idx As Integer)
        PolygonIdx = Idx
    End Sub

    Public Function GetNodeType() As GeneralFunctions.enmNodetype
        Return nt
    End Function

    'Public Function GetNodeType() As clsNetworkNTWNodeType
    '    Return NODETYPE
    'End Function

    Public Function getPolygonIdx() As Integer
        Return PolygonIdx
    End Function

    Public Sub New(ByRef myNetworkNTW As clsNetworkNTW, NodeID As String, NodeName As String, NodeTypeNum As Integer, NodeTypeStr As String, NodeX As Double, NodeY As Double)
        NetworkNTW = myNetworkNTW
        ID = NodeID
        Name = NodeName
        X = NodeX
        Y = NodeY
        nt = NodeTypeNum ' Me.setup.generalfunctions.
        'nt = NetworkNTW.GetAddNodeType(NodeTypeNum, NodeTypeStr)
    End Sub

End Class
