Public Class clsStochasticGroundwaterClass

    Public ID As String
    Public p As Double
    Public FileName As String

    Public Sub New(ByVal myID As String, ByVal myP As Double, ByVal myFile As String)
        ID = myID
        p = myP
        FileName = myFile
    End Sub

    Public Sub New(ByVal myID As String, ByVal myP As Double)
        ID = myID
        p = myP
    End Sub
End Class
