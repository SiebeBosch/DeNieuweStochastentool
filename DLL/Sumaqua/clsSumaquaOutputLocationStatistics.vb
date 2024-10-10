Public Class clsSumaquaOutputLocationStatistics
    Public ColIdx As Integer
    Public ID As String
    Public X As Double
    Public Y As Double

    Public Max As Double
    Public Min As Double
    Public First As Double
    Public Last As Double
    Public Mean As Double
    Public Median As Double

    Public Sub New(myIDx As Integer, myID As String)
        ColIdx = myIDx
        ID = myID
    End Sub

End Class
