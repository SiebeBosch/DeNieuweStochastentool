Imports STOCHLIB.General
Public Class clsHBVCompTxtFile
    Private Setup As clsSetup

    Dim ResultsSeries As List(Of clsHBVResultsSeriesRecord)

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        ResultsSeries = New List(Of clsHBVResultsSeriesRecord)
    End Sub

End Class
