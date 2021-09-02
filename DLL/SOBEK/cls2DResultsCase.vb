Public Class cls2DResultsCase
    Public Name As String
    Public DepthMaps As List(Of String)                  'a list of results filenames for the current case
    Public WaterLevelMaps As List(Of String)
    Public VelocityMaps As List(Of String)
    Public uVelocityMaps As List(Of String)
    Public vVelocityMaps As List(Of String)

    Public Parameters As List(Of Tuple(Of String, String))  'this is a container where we can store the case characteristics
    Public Sub New(myName As String)
        Name = myName
        DepthMaps = New List(Of String)
        WaterLevelMaps = New List(Of String)
        VelocityMaps = New List(Of String)
        uVelocityMaps = New List(Of String)
        vVelocityMaps = New List(Of String)
        Parameters = New List(Of Tuple(Of String, String))
    End Sub

End Class
