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
            Console.WriteLine("This application runs a series of simulations using multithreading (parallel).")
            Console.WriteLine("Please provide a path to a simulations file. Each line in the simulations file should refer to a .bat or .exe file, followed by a space and its arguments.")
            Console.WriteLine("E.g., ""C:\temp\Bommel\STOWA2014_HUIDIG_winter_MIDDELNAT_MIDDELHOOG_140mm\run.bat""")
            Console.WriteLine("The second argument is the maximum number of concurrent simulations, e.g., 4.")
            Console.WriteLine("Usage: BAT_RUNR.exe [SimulationsFile] [MaxConcurrentSimulations]")

            Console.WriteLine("Please enter the path to a simulations file:")
            simulationsFile = Console.ReadLine()

            Console.WriteLine("Please enter the maximum number of concurrent simulations:")
            While Not Integer.TryParse(Console.ReadLine(), maxConcurrentSimulations)
                Console.WriteLine("Invalid input. Please enter a numeric value for the maximum number of concurrent simulations:")
            End While
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
            ' Parse the simulation string into path and arguments
            Dim simulationParts = ParseSimulationString(simulation)

            Dim thread As New Thread(Sub()
                                         ' Combine the simulations file directory with the simulation path
                                         Dim fullPath As String = Path.Combine(Path.GetDirectoryName(simulationsFile), simulationParts.path)
                                         RunSimulation(fullPath, simulationParts.arguments)
                                     End Sub)
            thread.Start()
            threads.Add(thread)
        Next

        ' Wait for all threads to complete
        For Each thread In threads
            thread.Join()
        Next

        Console.WriteLine("All simulations complete")
    End Sub

    Function ParseSimulationString(simulation As String) As (path As String, arguments As String)
        Dim path As String = String.Empty
        Dim arguments As String = String.Empty

        If simulation.StartsWith(ControlChars.Quote) AndAlso simulation.Contains(ControlChars.Quote) Then
            Dim endQuoteIndex As Integer = simulation.IndexOf(ControlChars.Quote, 1)
            If endQuoteIndex <> -1 Then
                path = simulation.Substring(1, endQuoteIndex - 1)
                arguments = simulation.Substring(endQuoteIndex + 1).Trim()
            End If
        Else
            Dim parts() As String = simulation.Split(New Char() {" "c}, 2)
            path = parts(0)
            arguments = If(parts.Length > 1, parts(1), String.Empty)
        End If

        Return (path, arguments)
    End Function

    Sub RunSimulation(filePath As String, arguments As String)
        semaphore.WaitOne()

        Try
            Dim workingDirectory As String = Path.GetDirectoryName(filePath)

            If Not File.Exists(filePath) Then
                Console.WriteLine("File not found: " & filePath)
                Return
            End If

            If String.IsNullOrEmpty(workingDirectory) OrElse Not Directory.Exists(workingDirectory) Then
                Console.WriteLine("Working directory not found: " & workingDirectory)
                Return
            End If

            Console.WriteLine($"Starting simulation: {filePath} in directory {workingDirectory}")

            Dim process As New Process()
            process.StartInfo = New ProcessStartInfo() With {
                .FileName = filePath,
                .Arguments = arguments,
                .WorkingDirectory = workingDirectory,
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True
            }

            process.Start()

            Dim output As String = process.StandardOutput.ReadToEnd()
            Dim errorOutput As String = process.StandardError.ReadToEnd()
            process.WaitForExit()

            If process.ExitCode = 0 Then
                Console.WriteLine($"Completed simulation: {filePath}")
            Else
                Console.WriteLine($"Simulation failed: {filePath} with exit code {process.ExitCode}")
                If Not String.IsNullOrWhiteSpace(errorOutput) Then
                    Console.WriteLine($"Error: {errorOutput}")
                End If
            End If

        Catch ex As Exception
            Console.WriteLine($"Error running simulation {filePath}: {ex.Message}")
        Finally
            semaphore.Release()
        End Try
    End Sub
End Module
