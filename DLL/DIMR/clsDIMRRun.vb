Imports STOCHLIB.General
Imports System.IO
Public Class clsDIMRRun
    'this class contains all the content required for a DIMR simulation
    Public Operations As New List(Of clsDIMRFileOperation)          'all file operations required to establish this particular run
    Public Scenarios As New Dictionary(Of String, clsDIMRScenario)  'the unique combination of scenario's that make up this simulation
    Public InputFiles As New Dictionary(Of String, String)          'key = the original filename, value = the full path to this file as used in the run
    Public OutputFiles As New List(Of String)

    'to prevent certain properties to be recomputed multiple times (multiple operations) we store them here
    Public BreachDateTime As DateTime = Nothing         'the actual moment of the dambreak
    Public RestartFilePath As String        'the restartfile right before T0 - 24 hours
    Public TStart As DateTime = Nothing     'the start of the simulation, matching our restart file

    Public Name As String

    Public DIMR As clsDIMR         'this object contains all information needed to run the simulation
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub SetDIMRProject(ByRef DIMRProject As clsDIMR)
        DIMR = DIMRProject
    End Sub

    Public Function SetOutputFiles(myOutputFiles As List(Of String))
        OutputFiles = myOutputFiles
    End Function

    Public Function GetName() As String
        Console.WriteLine("Simulation name is " & Name)
        Return Name
    End Function

    Public Function GetOrCreateName() As String
        If Name = String.Empty Then
            'if no name present, generate a new name based on the scenario's
            'the name of a run is made up of the names of each underying scenario
            If Scenarios.Count > 0 Then
                Name = Scenarios.Values(0).Name
                For i = 1 To Scenarios.Values.Count - 1
                    Name &= "_" & Scenarios.Values(i).Name
                Next
            End If
        End If
        Return Name
    End Function

    Public Sub SetName(myName As String)
        Name = myName
    End Sub

    Public Function ExecuteAndRemoveUnNecessaryOutputFiles() As Boolean
        Try
            If Not Execute() Then Throw New Exception("Error executing simulation " & GetName())

            'we only want to keep the files specified in the output section of the Excel configuration (saving disk space)
            Dim Files As New Collection
            Setup.GeneralFunctions.CollectAllFilesInDir(DIMR.FlowFM.getOutputFullDir, False, "", Files)
            For Each path As String In Files
                Dim i As Integer
                Dim Keep As Boolean = False
                For i = 0 To OutputFiles.Count - 1

                    'here we convert the (user friendly *.extension) by the regular expression: .*\.extension
                    Dim Pattern As String = OutputFiles(i)
                    If Left(Pattern, 1) = "*" Then
                        Pattern = Right(Pattern, Pattern.Length - 1) & "$"
                    End If

                    If Setup.GeneralFunctions.RegExMatch(Pattern, path, True) Then
                        Keep = True
                        Exit For
                    End If
                Next
                If Not Keep Then File.Delete(path)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ExecuteAndRemoveUnNecessaryOutputFiles: " & ex.Message)
            Return False
        End Try
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
