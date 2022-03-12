Public Class clsDIMRRun
    'this class contains all the content required for a DIMR simulation
    Public ClassName As String
    Friend Operations As New List(Of clsDIMRFileOperation)
    Friend Scenarios As New Dictionary(Of String, clsDIMRScenario)    'the unique combination of scenario's that make up this simulation
    Public ResultsDir As String
    Public DimrConfigPath As String 'each run gets its own unique dimr_config.xml
    Public MDUFileName As String 'each run gets its own unique .mdu file

    Public Sub SetDimrconfigPath(myPath As String)
        DimrConfigPath = myPath
    End Sub

    Public Sub SetMDUFileName(myFileName As String)
        MDUFileName = myFileName
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

    Public Sub SetResultsDir(myDir As String)
        ResultsDir = myDir
    End Sub

    Public Function Execute(BatchFilePath As String, WorkDir As String) As Boolean
        Try
            Dim myProcess As New Process
            myProcess.StartInfo.WorkingDirectory = WorkDir
            myProcess.StartInfo.FileName = BatchFilePath
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
