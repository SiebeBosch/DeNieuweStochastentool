Public Class clsNetworkNTWNodeType
    Dim TypeNum As Integer
    Dim TypeStr As String
    Public Sub New(myNum As Integer, myDescription As String)
        TypeNum = myNum
        TypeStr = myDescription
    End Sub

    Public Function getTypeStr() As String
        Return TypeStr
    End Function

End Class
