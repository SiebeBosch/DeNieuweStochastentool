Imports System
Imports System.IO
Imports System.Threading

Module BAT_RUNR
    Dim semaphore As Semaphore
    Dim simulationsFile As String
    Dim maxConcurrentSimulations As Integer
    Dim maxWaitTimeHours As Integer
    Dim completedSimulations As Integer = 0
    Dim totalSimulations As Integer = 0
    Dim lockObj As New Object()
    Dim LogDir As String

    Sub Main(args As String())

        Console.WriteLine("BAT_RUNR version 2.1.0.")
        Console.WriteLine("This application runs simulations in parallel from a JSON file.")
        Console.WriteLine("Args: [simulations.json] [max_concurrent]")
        If args.Length < 3 Then
            Console.WriteLine("Please enter the path to a simulations file (.json):")
            simulationsFile = Console.ReadLine()
            Console.WriteLine("Please enter the maximum number of concurrent simulations:")
            While Not Integer.TryParse(Console.ReadLine(), maxConcurrentSimulations)
                Console.WriteLine("Invalid input. Please enter a numeric value:")
            End While
            Console.WriteLine("Please enter the maximum wait time for each simulation (hours):")
            While Not Integer.TryParse(Console.ReadLine(), maxWaitTimeHours)
                Console.WriteLine("Invalid input. Please enter a numeric value:")
            End While
        Else
            simulationsFile = args(0)
            Console.WriteLine($"Path to the simulations file set: {args(0)}")
            maxConcurrentSimulations = Integer.Parse(args(1))
            Console.WriteLine($"Maximum number of concurrent simulations: {args(1)}")
            maxWaitTimeHours = Integer.Parse(args(2))
            Console.WriteLine($"Maximum wait time per simulation (hours): {args(2)}")
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

            Console.WriteLine($"Queuing simulation: {sim.Executable} with arguments: {sim.Arguments}")

            Dim thread As New Thread(Sub()
                                         Try
                                             Console.WriteLine("Thread created for simulation: " & sim.Executable)
                                             RunSimulation(sim.WorkDir, sim.Executable, sim.Arguments, simulationLogFile, errorLogFile)
                                             UpdateProgress()
                                         Catch ex As Exception
                                             Console.WriteLine("Error in thread: " & ex.Message)
                                             SyncLock lockObj
                                                 File.AppendAllText(errorLogFile, $"Thread error: {ex.Message}{Environment.NewLine}")
                                             End SyncLock
                                         End Try
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
        ' First acquire the semaphore before doing anything
        semaphore.WaitOne()

        Try
            SyncLock lockObj
                File.AppendAllText(simulationLogFile, $"[{DateTime.Now}] STARTED: {filePath}{Environment.NewLine}")
            End SyncLock

            If Not File.Exists(filePath) Then
                Dim errorMsg = $"File not found: {filePath}"
                SyncLock lockObj
                    File.AppendAllText(errorLogFile, $"{errorMsg}{Environment.NewLine}")
                End SyncLock
                Console.WriteLine(errorMsg)
                Exit Try  ' Use Exit Try instead of Return
            End If

            If Not Directory.Exists(workDir) Then
                Dim errorMsg = $"Working directory not found: {workDir}"
                SyncLock lockObj
                    File.AppendAllText(errorLogFile, $"{errorMsg}{Environment.NewLine}")
                End SyncLock
                Console.WriteLine(errorMsg)
                Exit Try  ' Use Exit Try instead of Return
            End If

            Console.WriteLine($"Starting simulation: {filePath}")
            'Console.WriteLine($"Starting simulation: {filePath} in directory {workDir}")
            SyncLock lockObj
                File.AppendAllText(simulationLogFile, $"Starting simulation: {filePath} in directory {workDir}{Environment.NewLine}")
            End SyncLock

            Using process As New Process()
                process.StartInfo = New ProcessStartInfo() With {
                    .FileName = filePath,
                    .Arguments = arguments,
                    .WorkingDirectory = workDir,
                    .UseShellExecute = False,
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True,
                    .RedirectStandardInput = True,  ' Add this line
                    .CreateNoWindow = True
                }
                AddHandler process.OutputDataReceived, Sub(sender, e)
                                                           If e.Data IsNot Nothing Then
                                                               SyncLock lockObj
                                                                   File.AppendAllText(simulationLogFile, $"[Output] {e.Data}{Environment.NewLine}")
                                                               End SyncLock
                                                           End If
                                                       End Sub
                AddHandler process.ErrorDataReceived, Sub(sender, e)
                                                          If e.Data IsNot Nothing Then
                                                              SyncLock lockObj
                                                                  File.AppendAllText(errorLogFile, $"[Error] {e.Data}{Environment.NewLine}")
                                                              End SyncLock
                                                          End If
                                                      End Sub

                process.Start()
                process.BeginOutputReadLine()
                process.BeginErrorReadLine()            'forces to exit in case the 'press any key to continue' prompt appears 
                process.StandardInput.WriteLine()

                'process.WaitForExit()
                If Not process.WaitForExit(maxWaitTimeHours * 3600 * 1000) Then 'wait for exit
                    process.Kill()
                    Dim errorMsg = $"Simulation killed due to timeout: {filePath}"
                    Console.WriteLine(errorMsg)
                    SyncLock lockObj
                        File.AppendAllText(errorLogFile, $"{errorMsg}{Environment.NewLine}")
                    End SyncLock
                End If

                SyncLock lockObj
                    File.AppendAllText(simulationLogFile, $"[{DateTime.Now}] FINISHED: {filePath} with exit code {process.ExitCode}{Environment.NewLine}")
                End SyncLock

                If process.ExitCode <> 0 Then
                    Dim errorMsg = $"Simulation failed: {filePath} with exit code {process.ExitCode}"
                    SyncLock lockObj
                        File.AppendAllText(errorLogFile, $"{errorMsg}{Environment.NewLine}")
                    End SyncLock
                    Console.WriteLine(errorMsg)
                Else
                    Console.WriteLine($"Simulation completed successfully: {filePath}")
                End If
            End Using

        Catch ex As Exception
            SyncLock lockObj
                File.AppendAllText(errorLogFile, $"Error running simulation {filePath}: {ex.Message}{Environment.NewLine}")
            End SyncLock
            Console.WriteLine($"Error running simulation {filePath}: {ex.Message}")
        Finally
            Try
                Console.WriteLine("Thread completed for simulation: " & filePath)
                semaphore.Release()
            Catch ex As Exception
                Console.WriteLine("Semaphore release failed: " & ex.Message)
            End Try
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
