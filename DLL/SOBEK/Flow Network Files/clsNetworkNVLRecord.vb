Public Class clsNetworkNVLRecord
    Public nVectorPoints As Integer
    Public bbox As clsBoundingBox
    Public cp As New List(Of clsXY)

    Public Sub New()
        bbox = New clsBoundingBox

    End Sub
End Class
