Imports STOCHLIB.General
Public Class clsHBVIndataParFile
    Private Setup As clsSetup

    'example of indata.par file
    'station' 'Partij_P'
    'type' 'p'
    'stnno' 8
    'weight' 1
    'elev' 100
    'station' '308_T'
    'type' 't'
    'stnno' 3
    'weight' 1
    'elev' 100
    'station' 'maasstricht'
    'type' 'e'
    'stnno' 380
    'weight' 1
    'elev' 100

    Dim Stations As New Dictionary(Of String, clsHBVStation)

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub



End Class

Public Class clsHBVStation
    Private Setup As clsSetup

    Dim Name As String
    Dim Type As String  'p, t, e
    Dim StnNo As Integer
    Dim Weight As Double
    Dim Elev As Double

    Public Sub New(ByRef mySetup As clsSetup, StationName As String)
        Setup = mySetup
        Name = StationName
    End Sub

End Class
