Imports System
Imports System.IO
Imports System.Threading

Module BAT_RUNR
    Dim semaphore As Semaphore
    Dim simulationsFile As String
    Dim maxConcurrentSimulations As Integer

    Sub Main(args As String())
        Console.WriteLine("Running simulations")


        If args.Length < 2 Then
            Console.WriteLine("Usage: BAT_RUNR.exe [SimulationsFile] [MaxConcurrentSimulations]")
            Exit Sub
        Else
            simulationsFile = args(0)
            maxConcurrentSimulations = Integer.Parse(args(1))
        End If

        If Not File.Exists(simulationsFile) Then
            Console.WriteLine("Simulations file not found: " & simulationsFile)
            Exit Sub
        End If

        ' Initialize the semaphore with the maximum number of concurrent simulations
        semaphore = New Semaphore(maxConcurrentSimulations, maxConcurrentSimulations)

        Dim simulations As String() = File.ReadAllLines(simulationsFile)
        Dim threads As New List(Of Thread)

        For Each simulation In simulations
            Dim thread As New Thread(Sub() RunSimulation(simulation))
            thread.Start()
            threads.Add(thread)
        Next

        ' Wait for all threads to complete
        For Each thread In threads
            thread.Join()
        Next

        Console.WriteLine("All simulations complete")
    End Sub

    Sub RunSimulation(simulation As String)
        semaphore.WaitOne() ' Acquire the semaphore

        Try
            Dim simulationArgs As String() = simulation.Split(" "c)
            Dim filePath As String = RemoveQuotes(simulationArgs(0))
            Dim arguments As String = String.Join(" ", simulationArgs.Skip(1))
            Dim workingDirectory As String = GetDirFromPath(filePath)

            If Not File.Exists(filePath) Then
                Console.WriteLine("File not found: " & filePath)
                Return
            End If

            If Not Directory.Exists(workingDirectory) Then
                Console.WriteLine("Working directory not found: " & workingDirectory)
                Return
            End If


            ' Adding feedback about the simulation start
            Console.WriteLine("Starting simulation: " & filePath)

            Dim process As New Process
            process.StartInfo.FileName = filePath
            process.StartInfo.Arguments = arguments
            process.StartInfo.WorkingDirectory = workingDirectory
            process.StartInfo.UseShellExecute = True
            process.StartInfo.CreateNoWindow = False
            process.Start()
            process.WaitForExit()

            ' Adding feedback about the simulation completion
            Console.WriteLine("Completed simulation: " & filePath)
        Finally
            semaphore.Release() ' Release the semaphore
        End Try
    End Sub


    ' Function to remove extra quotes from a path
    Function RemoveQuotes(path As String) As String
        Return path.Trim(ControlChars.Quote)
    End Function

    Friend Function GetDirFromPath(ByVal path As String) As String
        Try
            Return path.Substring(0, path.LastIndexOf("\") + 1)
        Catch ex As Exception
            Return ""
        End Try
    End Function

End Module
