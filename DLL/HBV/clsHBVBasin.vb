Imports DocumentFormat.OpenXml.Wordprocessing
Imports STOCHLIB.General

Public Class clsHBVBasin
    Private Setup As clsSetup

    Friend Name As String
    Friend RainfallStation As String

    Friend SubBasins As Dictionary(Of String, clsHBVSubBasin)

    Public Sub New(ByRef mySetup As clsSetup, BasinName As String)
        Setup = mySetup
        Name = BasinName
        SubBasins = New Dictionary(Of String, clsHBVSubBasin)
    End Sub

End Class
