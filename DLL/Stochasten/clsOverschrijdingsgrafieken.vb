Imports STOCHLIB.General

Public Class clsOverschrijdingsgrafieken
    Private Setup As clsSetup

    Dim Grafieken As New Dictionary(Of String, clsOverschrijdingsgrafiek)

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = New clsSetup
    End Sub

    Public Function GetAdd(LocationID As String) As clsOverschrijdingsgrafiek
        Dim Grafiek As clsOverschrijdingsgrafiek
        If Grafieken.ContainsKey(LocationID.Trim.ToUpper) Then
            Grafiek = Grafieken.Item(LocationID.Trim.ToUpper)
        Else
            Grafiek = New clsOverschrijdingsgrafiek()
            Grafieken.Add(LocationID.Trim.ToUpper, Grafiek)
        End If
        Return Grafieken.Item(LocationID.Trim.ToUpper)
    End Function

End Class
