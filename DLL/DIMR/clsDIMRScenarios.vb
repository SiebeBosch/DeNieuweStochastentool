Public Class clsDIMRScenarios
    Public Scenarios As New Dictionary(Of String, clsDIMRScenario)

    Public Sub Add(ByRef myScenario As clsDIMRScenario)
        Scenarios.Add(myScenario.Name.Trim.ToUpper, myScenario)
    End Sub

End Class
