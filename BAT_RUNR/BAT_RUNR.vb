Imports System
Imports System.IO
Imports System.Threading

Module BAT_RUNR
    Dim semaphore As Semaphore
    Dim simulationsFile As String
    Dim maxConcurrentSimulations As Integer
    Dim completedSimulations As Integer = 0
    Dim totalSimulations As Integer = 0
    Dim lockObj As New Object()
    Dim LogDir As String

    Sub Main(args As String())

        Console.WriteLine("BAT_RUNR version 2.0.0.")
        Console.WriteLine("This application runs simulations in parallel from a JSON file.")
        Console.WriteLine("Args: [simulations.json] [max_concurrent]")
        If args.Length < 2 Then
            Console.WriteLine("Please enter the path to a simulations file (.json):")
            simulationsFile = Console.ReadLine()
            Console.WriteLine("Please enter the maximum number of concurrent simulations:")
            While Not Integer.TryParse(Console.ReadLine(), maxConcurrentSimulations)
                Console.WriteLine("Invalid input. Please enter a numeric value:")
            End While
        Else
            simulationsFile = args(0)
            Console.WriteLine($"Path to the simulations file set: {args(0)}")
            maxConcurrentSimulations = Integer.Parse(args(1))
            Console.WriteLine($"Maximum number of concurrent simulations: {args(1)}")
        End If

        If Not File.Exists(simulationsFile) Then
            Console.WriteLine("Simulations file not found: " & simulationsFile)
            Exit Sub
        End If

        'get the directory of the simulations file
        LogDir = Path.GetDirectoryName(simulationsFile)

        ' Read and parse JSON
        Dim jsonContent As String = File.ReadAllText(simulationsFile)

        Dim simulations As SimulationsContainer
        Try
            simulations = System.Text.Json.JsonSerializer.Deserialize(Of SimulationsContainer)(jsonContent)
            If simulations Is Nothing OrElse simulations.simulations.Count = 0 Then
                Console.WriteLine("No simulations found in the provided file.")
                Exit Sub
            End If
        Catch ex As Exception
            Console.WriteLine("Error parsing simulations file: " & ex.Message)
            Exit Sub
        End Try

        ' Create directory for the log file and error file
        Dim simulationLogFile As String = Path.Combine(LogDir, "simulation_logs.txt")
        Dim errorLogFile As String = Path.Combine(LogDir, "error_logs.txt")
        If Not Directory.Exists(LogDir) Then
            Directory.CreateDirectory(LogDir)
        End If

        totalSimulations = simulations.simulations.Count

        semaphore = New Semaphore(maxConcurrentSimulations, maxConcurrentSimulations)
        Dim threads As New List(Of Thread)

        For Each sim In simulations.simulations

            'for debugging onlhy
            'RunSimulation(sim.WorkDir, sim.Executable, sim.Arguments, simulationLogFile, errorLogFile)

            Dim thread As New Thread(Sub()
                                         RunSimulation(sim.WorkDir, sim.Executable, sim.Arguments, simulationLogFile, errorLogFile)
                                         UpdateProgress()
                                     End Sub)
            thread.Start()
            threads.Add(thread)
        Next

        For Each thread In threads
            thread.Join()
        Next
        Console.WriteLine("All simulations complete")
    End Sub

    Public Class SimulationsContainer
        Public Property simulations As List(Of Simulation)
    End Class

    Public Class Simulation
        Public Property ModelType As String
        Public Property Executable As String
        Public Property Arguments As String
        Public Property WorkDir As String
    End Class

    Sub RunSimulation(workDir As String, filePath As String, arguments As String, simulationLogFile As String, errorLogFile As String)
        semaphore.WaitOne()

        Try
            If Not File.Exists(filePath) Then
                Dim errorMsg = $"File not found: {filePath}"
                SyncLock lockObj
                    File.AppendAllText(errorLogFile, $"{errorMsg}{Environment.NewLine}")
                End SyncLock
                Console.WriteLine(errorMsg)
                Return
            End If

            If Not Directory.Exists(workDir) Then
                Dim errorMsg = $"Working directory not found: {workDir}"
                SyncLock lockObj
                    File.AppendAllText(errorLogFile, $"{errorMsg}{Environment.NewLine}")
                End SyncLock
                Console.WriteLine(errorMsg)
                Return
            End If

            'Console.WriteLine($"Starting simulation: {filePath} in directory {workDir}")
            SyncLock lockObj
                File.AppendAllText(simulationLogFile, $"Starting simulation: {filePath} in directory {workDir}{Environment.NewLine}")
            End SyncLock

            Using process As New Process()
                process.StartInfo = New ProcessStartInfo() With {
                .FileName = filePath,
                .Arguments = arguments,
                .WorkingDirectory = workDir,
                .UseShellExecute = True,
                .CreateNoWindow = False,
                .WindowStyle = ProcessWindowStyle.Hidden
            }
                process.Start()
                process.WaitForExit()

                If process.ExitCode <> 0 Then
                    Dim errorMsg = $"Simulation failed: {filePath} with exit code {process.ExitCode}"
                    SyncLock lockObj
                        File.AppendAllText(errorLogFile, $"{errorMsg}{Environment.NewLine}")
                    End SyncLock
                    Console.WriteLine(errorMsg)
                Else
                    SyncLock lockObj
                        File.AppendAllText(simulationLogFile, $"Simulation completed: {filePath} in directory {workDir}{Environment.NewLine}")
                    End SyncLock
                End If
            End Using

        Catch ex As Exception
            SyncLock lockObj
                File.AppendAllText(errorLogFile, $"Error running simulation {filePath}: {ex.Message}{Environment.NewLine}")
            End SyncLock
            Console.WriteLine($"Error running simulation {filePath}: {ex.Message}")
        Finally
            semaphore.Release()
        End Try
    End Sub


    Sub UpdateProgress()
        SyncLock lockObj
            completedSimulations += 1
            ' Clear the current line and write the new progress
            Console.Write(vbCr & New String(" "c, Console.WindowWidth - 1)) ' Clear the line
            Console.Write(vbCr & $"Progress: {completedSimulations}/{totalSimulations} simulations complete")
            Console.Out.Flush() ' Ensure the output is flushed to the console
        End SyncLock
    End Sub

End Module
