Imports STOCHLIB.General
Public Class clsParameterPercentileCombination
    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Public Name As String
    Public Parameters As List(Of GeneralFunctions.enmModelParameter)
    Public PercentileClasses As List(Of clsPercentileClass)

End Class
