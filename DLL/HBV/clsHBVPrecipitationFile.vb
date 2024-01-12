
Imports STOCHLIB.General

Public Class clsHBVPrecipitationFile
    Private Setup As clsSetup

    Dim Parameter As String = "P"
    Dim FirstNum As Integer = 9 'no idea what this is
    Dim Name As String
    Dim SecondNum As Integer = 24 'number of hours per timestep
    Dim Records As List(Of clsHBVPrecipitationFileRecord)

    Public Sub New(ByRef mySetup As clsSetup, StationName As String)
        Setup = mySetup
        Name = StationName & "_P"
        Records = New List(Of clsHBVPrecipitationFileRecord)
    End Sub




End Class
