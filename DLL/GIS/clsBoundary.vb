Imports STOCHLIB.General

''' <summary>
''' Geen constructor nodig
''' </summary>
''' <remarks></remarks>
Public Class clsBoundary
    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Friend ID As String
    Friend Name As String
    Friend X As Double
    Friend Y As Double
    Friend BoundType As GeneralFunctions.enmBoundaryType
    Friend BoundVal As Double

End Class
