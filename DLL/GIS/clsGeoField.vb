Imports STOCHLIB.General
Public Class clsGeoField
    Private Setup As clsSetup

    Public Name As String
    Public FieldType As GeneralFunctions.enmInternalVariable
    Public ColIdx As Integer

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myName As String, myFieldType As GeneralFunctions.enmInternalVariable, myColIdx As Integer)
        Setup = mySetup
        Name = myName
        FieldType = myFieldType
        ColIdx = myColIdx
    End Sub



End Class
