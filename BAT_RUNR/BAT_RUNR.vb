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
            Console.WriteLine("This application runs a series of simulations using multithreading (parallel)")
            Console.WriteLine("Please provide a path to a simulations file. Each line in the simulations file should refer to a .bat or .exe file, followed by a space and its arguments.")
            Console.WriteLine("E.g. ""C:\temp\Bommel\STOWA2014_HUIDIG_winter_MIDDELNAT_MIDDELHOOG_140mm\run.bat.""")
            Console.WriteLine("The second argument is the maximum number of concurrent simulations, e.g. 4.")
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

        'set the current working directory equal to the directory of the simulations file
        Directory.SetCurrentDirectory(GetDirFromPath(simulationsFile))

        ' Initialize the semaphore with the maximum number of concurrent simulations
        semaphore = New Semaphore(maxConcurrentSimulations, maxConcurrentSimulations)

        Dim simulations As String() = File.ReadAllLines(simulationsFile)
        Dim threads As New List(Of Thread)
        For Each simulation In simulations

            'first distinguish any arguments from the path
            Dim simulationParts = ParseSimulationString(simulation)

            Dim thread As New Thread(Sub()
                                         ' Combine the current directory with the path
                                         Dim fullPath As String = System.IO.Path.Combine(Directory.GetCurrentDirectory(), simulationParts.path)

                                         ' Run the simulation with the full path and arguments
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

        ' Check if the simulation string is enclosed in quotes
        If simulation.StartsWith(ControlChars.Quote) AndAlso simulation.Contains(ControlChars.Quote) Then
            ' Find the ending quote of the path
            Dim endQuoteIndex As Integer = simulation.IndexOf(ControlChars.Quote, 1)
            If endQuoteIndex <> -1 Then
                ' Extract the path and subsequent arguments
                path = simulation.Substring(1, endQuoteIndex - 1)
                arguments = simulation.Substring(endQuoteIndex + 1).Trim()
            End If
        Else
            ' If not enclosed in quotes, split normally
            Dim parts() As String = simulation.Split(New Char() {" "c}, 2)
            path = parts(0)
            arguments = If(parts.Length > 1, parts(1), String.Empty)
        End If

        Return (path, arguments)
    End Function


    Sub RunSimulation(filePath As String, arguments As String)
        semaphore.WaitOne() ' Acquire the semaphore

        Try
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
