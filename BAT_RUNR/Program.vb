Imports System

Module Program
    Sub Main(args As String())
        Console.WriteLine("Running a batchfile")

        'this application simply executes a bat file
        'the bat file is passed as an argument to this application
        'it is assumed that the bat file is located in the same folder as this application
        'the raison d'etre for this application is that it is not possible to execute DIMR bat files directly from a clickonce application
        'this always gave issues probably related to path variables or registry entries

        Dim myBatFile As String
        If args.Length < 1 Then
            Console.WriteLine("Please enter the path to the batch file:")
            myBatFile = Console.ReadLine()
        Else
            myBatFile = args(0)
        End If

        'Dim myBatFileFolder As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        'Dim myBatFileFullPath As String = System.IO.Path.Combine(myBatFileFolder, myBatFile)

        'If Not System.IO.File.Exists(myBatFileFullPath) Then
        '    Console.WriteLine("Bat file not found: " & myBatFileFullPath)
        '    Exit Sub
        'End If

        Dim myProcess As New Process
        myProcess.StartInfo.FileName = myBatFile
        myProcess.StartInfo.WorkingDirectory = GetDirFromPath(myBatFile)
        myProcess.StartInfo.UseShellExecute = False
        myProcess.StartInfo.RedirectStandardOutput = False
        myProcess.StartInfo.RedirectStandardError = False
        myProcess.StartInfo.CreateNoWindow = False
        myProcess.Start()

        'Dim myOutput As String = myProcess.StandardOutput.ReadToEnd()
        'Dim myError As String = myProcess.StandardError.ReadToEnd()

        myProcess.WaitForExit()

        'Console.WriteLine("Output:")
        'Console.WriteLine(myOutput)
        'Console.WriteLine("Error:")
        'Console.WriteLine(myError)

        Console.WriteLine("Done")

    End Sub

    Friend Function GetDirFromPath(ByVal path As String) As String
        Try
            Return path.Substring(0, path.LastIndexOf("\") + 1)
        Catch ex As Exception
            Return ""
        End Try
    End Function

End Module
