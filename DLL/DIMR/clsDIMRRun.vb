Public Class clsDIMRRun
    'this class contains all the content required for a DIMR simulation
    Public Operations As New List(Of clsDIMRFileOperation)          'all file operations required to establish this particular run
    Public Scenarios As New Dictionary(Of String, clsDIMRScenario)  'the unique combination of scenario's that make up this simulation
    Public InputFiles As New Dictionary(Of String, String)          'key = the original filename, value = the full path to this file as used in the run
    Public DIMR As clsDIMR         'this object contains all information needed to run the simulation

    Public Sub SetDIMRProject(ByRef DIMRProject As clsDIMR)
        DIMR = DIMRProject
    End Sub


    Public Function GetName() As String
        'the name of a run is made up of the names of each underying scenario
        Dim Name As String = String.Empty
        If Scenarios.Count > 0 Then
            Name = Scenarios.Values(0).Name
            For i = 1 To Scenarios.Values.Count - 1
                Name &= "_" & Scenarios.Values(i).Name
            Next
        End If
        Return Name
    End Function

    Public Function Execute() As Boolean
        Try
            Dim myProcess As New Process
            myProcess.StartInfo.WorkingDirectory = DIMR.ProjectDir
            myProcess.StartInfo.FileName = DIMR.BatchFilePath
            myProcess.StartInfo.Arguments = ""
            myProcess.Start()

            While Not myProcess.HasExited
                'pom pom pom
                Call Wait(2000)
            End While

            Return True

        Catch ex As Exception
            Console.WriteLine("Fout bij het uitvoeren van de simulatie: " & ex.Message)
            Return False
        End Try
    End Function


    Public Sub Wait(ByVal interval As Integer)
        'loops for a specified period of time (miliseconds)
        Dim sw As New Stopwatch
        sw.Start()
        Do While sw.ElapsedMilliseconds < interval
            'allow UI to remain responsive
            'Application.DoEvents()
        Loop
        sw.Stop()
    End Sub

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
