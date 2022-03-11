Public Class clsDIMRRun
    'this class contains all the content required for a DIMR simulation
    Public ClassName As String
    Friend Operations As New List(Of clsDIMRFileOperation)
    Friend Scenarios As New Dictionary(Of String, clsDIMRScenario)    'the unique combination of scenario's that make up this simulation

    Public Function GetName() As String
        'the name of a run is made up of the names of each underying scenario
        Dim Name As String
        If Scenarios.Count > 0 Then
            Name = Scenarios.Values(0).Name
            For i = 1 To Scenarios.Values.Count - 1
                Name &= "_" & Scenarios.Values(i).Name
            Next
        End If
        Return Name
    End Function

    Public Sub AddOperation(ByRef myOperation As clsDIMRFileOperation)
        'for each individual simulation we not only use the file adjustments from each of the underlying scenarios:
        'we can also specify individual modifications that are specific for this run
        Operations.Add(myOperation)
    End Sub

    Public Sub AddScenario(ByRef myScenario As clsDIMRScenario)
        If Not Scenarios.ContainsKey(myScenario.Name.Trim.ToUpper) Then
            Scenarios.Add(myScenario.Name.Trim.ToUpper, myScenario)
        End If
    End Sub

End Class
