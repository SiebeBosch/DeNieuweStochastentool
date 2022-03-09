Imports STOCHLIB.General
Imports System.IO

Public Class clsSobekSimulation

    Private Setup As clsSetup
    Dim Model As clsSimulationModel         'the simulation to be run
    Dim Bui As clsBuiFile                   'the rainfall event to be simulated
    Dim SimulationDir As String             'the directory in which the simulation will be performed
    Friend ResultsDir As String                'the directory in which the simulation results are written
    Friend ResultsFiles As List(Of String)     'a list of files that will be written to the resultsdir upon simulation
    Friend Simulations As clsSobekSimulations

    Dim buiFile As String, EvpFile As String, QscFile As String, WdcFile As String, QwcFile As String, TmpFile As String, RnfFile As String
    Dim BuiFileRelative As String, EvpFileRelative As String, QscFileRelative As String, WdcFileRelative As String, QwcFileRelative As String, TmpFileRelative As String, RnfFileRelative As String

    Public Sub New(ByRef mySetup As clsSetup, ByRef myModel As clsSimulationModel, ByRef mySimulations As clsSobekSimulations, ByRef myBui As clsBuiFile, mySimulationDir As String, myResultsDir As String)
        Setup = mySetup
        Model = myModel
        Bui = myBui
        ResultsDir = myResultsDir
        SimulationDir = mySimulationDir
        Simulations = mySimulations
    End Sub
    Public Sub New(ByRef mySetup As clsSetup, ByRef myModel As clsSimulationModel, mySimulationDir As String, myResultsDir As String)
        Setup = mySetup
        Model = myModel
        SimulationDir = mySimulationDir
        ResultsDir = myResultsDir
    End Sub

    Public Function getSimulationDir() As String
        Return SimulationDir
    End Function
    Public Function Run() As Boolean
        Try
            'Debug.Print("At the start the current thread " & Threading.Thread.CurrentThread.ManagedThreadId & " has state: " & Threading.Thread.CurrentThread.ThreadState.ToString)
            If Not System.IO.Directory.Exists(ResultsDir) Then System.IO.Directory.CreateDirectory(ResultsDir)
            If Not System.IO.Directory.Exists(Simulations.MeteoDir) Then System.IO.Directory.CreateDirectory(Simulations.MeteoDir)

            If Not Bui Is Nothing Then
                If Not WriteMeteoFiles() Then Throw New Exception("Error writing Meteo files.")
            End If
            'Debug.Print("After writing meteo files the current thread " & Threading.Thread.CurrentThread.ManagedThreadId & " has state: " & Threading.Thread.CurrentThread.ThreadState.ToString)

            'copy the original project to the temporary work dir and then read it from the new location
            'Setup.GeneralFunctions.UpdateProgressBar("Cloning model schematisation. ", 0, 10, True)
            Dim myProject = New STOCHLIB.clsSobekProject(Me.Setup, Model.ModelDir, True)

            If Not myProject.CloneCaseForCommandLineRun(Directory.GetParent(Directory.GetParent(Model.Exec).FullName).FullName, Model.CaseName.Trim.ToUpper, SimulationDir, BuiFileRelative, EvpFileRelative, QscFileRelative, WdcFileRelative, QwcFileRelative, TmpFileRelative, RnfFileRelative) Then Throw New Exception("Error: could not clone SOBEK case for running from the command line.")
            myProject = New STOCHLIB.clsSobekProject(Me.Setup, SimulationDir, True)
            'Debug.Print("After cloning the project the current thread " & Threading.Thread.CurrentThread.ManagedThreadId & " has state: " & Threading.Thread.CurrentThread.ThreadState.ToString)

            'decide wether we are dealing with a summer or a winter situation and copy the required corresponding files over
            If Bui.IsSummer(Me.Simulations.WinSumDate, Me.Simulations.SumWinDate) Then
                For Each myFile As String In Me.Simulations.SummerFiles
                    System.IO.File.Copy(myFile, SimulationDir & "\Work\" & Setup.GeneralFunctions.FileNameFromPath(myFile), True)
                Next
            Else
                For Each myFile As String In Me.Simulations.WinterFiles
                    System.IO.File.Copy(myFile, SimulationDir & "\Work\" & Setup.GeneralFunctions.FileNameFromPath(myFile), True)
                Next
            End If


            'write the current rainfall event (bui) and subsequently copy the Evaporation file to be used
            If Not Bui Is Nothing Then
                Bui.Write(buiFile)
                If System.IO.File.Exists(Simulations.EvpFile) Then
                    File.Copy(Simulations.EvpFile, EvpFile, True)
                Else
                    Dim myEvp As New STOCHLIB.clsEvpFile(Me.Setup)
                    Dim Evap(Setup.GeneralFunctions.RoundUD(Bui.getTotalSpan.TotalHours / 24, 0, True)) As Double
                    'Setup.GeneralFunctions.UpdateProgressBar("Writing evaporation event.", 0, 10, True)
                    myEvp.BuildSTOWATYPE(Evap, Bui.StartDate, 0)
                    myEvp.Write(EvpFile)
                    Me.Setup.Log.AddWarning("No evaporation found for event number " & Bui.number & ".Zero evaporation was assumed.")
                End If

                'Setup.GeneralFunctions.UpdateProgressBar("Writing radiation file.", 0, 10, True)
                Using myWriter As New StreamWriter(QscFile)
                    myWriter.WriteLine("CONSTANTS   'TEMP' 'RAD'")
                    myWriter.WriteLine("DATA        0 0")
                End Using

                'Setup.GeneralFunctions.UpdateProgressBar("Writing wind file.", 0, 10, True)
                Using myWriter As New StreamWriter(WdcFile)
                    myWriter.WriteLine("GLMT MTEO nm '(null)' ss 0 id '0' ci '-1' lc 9.9999e+009 wu 1")
                    myWriter.WriteLine("wv tv 0 0 9.9999e+009 wd td 0 0 9.9999e+009 su 0 sh ts")
                    myWriter.WriteLine("0 9.9999e+009 9.9999e+009 tu 0 tp tw 0 9.9999e+009 9.9999e+009 au 0 at ta 0")
                    myWriter.WriteLine("9.9999e+009 9.9999e+009 mteo glmt")
                End Using

                Using mywriter As New StreamWriter(QwcFile)
                    mywriter.WriteLine("CONSTANTS   'VWIND' 'WINDDIR'")
                    mywriter.WriteLine("DATA       0 0")
                End Using

                Using mywriter As New StreamWriter(TmpFile)
                    mywriter.WriteLine("")
                End Using

                Using mywriter As New StreamWriter(RnfFile)
                    mywriter.WriteLine("")
                End Using
            End If

            'Me.Setup.GeneralFunctions.UpdateProgressBar("Simulating event " & Bui.number & "...", 0, 10)
            'Debug.Print("Before executing the simulation the current thread " & Threading.Thread.CurrentThread.ManagedThreadId & " has state: " & Threading.Thread.CurrentThread.ThreadState.ToString)

            Using pProcess As New Process
                pProcess.StartInfo.FileName = Model.Exec
                pProcess.StartInfo.Arguments = Model.Args
                pProcess.StartInfo.WorkingDirectory = SimulationDir & "\CMTWORK"
                pProcess.StartInfo.UseShellExecute = False
                pProcess.StartInfo.RedirectStandardOutput = True
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                pProcess.StartInfo.CreateNoWindow = True
                pProcess.Start()
                Dim output As String = pProcess.StandardOutput.ReadToEnd()
                pProcess.WaitForExit()
            End Using


            'Debug.Print("After running the simulation the current thread " & Threading.Thread.CurrentThread.ManagedThreadId & " has state: " & Threading.Thread.CurrentThread.ThreadState.ToString)

            'if the run was succesful, copy the files
            Dim logReader As New StreamReader(SimulationDir & "\CMTWORK\PLUVIUS1.RTN")
            Dim logStr As String = logReader.ReadLine.Trim
            logReader.Close()

            'warning if results are used although simulation crashed
            If logStr <> "0" Then
                Throw New Exception("Simulation with event number " & Bui.number & " was unsuccessful.")
            Else
                Dim fromFile As String, fromFile2 As String
                Dim toFile As String, toFile2 As String

                'if our simulation was succssful we'll copy the Flow Resultsfile(s) to our results dir
                For Each myFile As STOCHLIB.clsResultsFile In Model.ResultsFiles.Files.Values

                    If Model.ModelType = GeneralFunctions.enmSimulationModel.SOBEK Then
                        fromFile = SimulationDir & "\WORK\" & myFile.FileName
                        fromFile2 = SimulationDir & "\WORK\" & Replace(myFile.FileName, ".his", ".hia", , , CompareMethod.Text)
                        toFile = ResultsDir & "\" & myFile.FileName
                        toFile2 = ResultsDir & "\" & Replace(myFile.FileName, ".his", ".hia", , , CompareMethod.Text)

                        If File.Exists(fromFile) Then
                            While Setup.GeneralFunctions.IsFileLocked(fromFile)
                                System.Threading.Thread.Sleep(100)
                            End While
                            Call FileCopy(fromFile, toFile)
                        Else
                            Me.Setup.Log.AddError("Fout: uitvoerbestand bestaat niet: " & fromFile)
                        End If

                        If File.Exists(fromFile2) Then
                            While Setup.GeneralFunctions.IsFileLocked(fromFile2)
                                System.Threading.Thread.Sleep(100)
                            End While
                            Call FileCopy(fromFile2, toFile2)
                        Else
                            Me.Setup.Log.AddWarning("Uitvoerbestand bestaat niet: " & fromFile2 & ". resultaten voor ID's langer dan 20 karakters kunnen daarom niet correct worden uitgelezen.")
                        End If

                    ElseIf Model.ModelType = GeneralFunctions.enmSimulationModel.DIMR Then

                        fromFile = SimulationDir & "\WORK\" & myFile.FileName
                        fromFile2 = SimulationDir & "\WORK\" & Replace(myFile.FileName, ".his", ".hia", , , CompareMethod.Text)
                        toFile = ResultsDir & "\" & myFile.FileName
                        toFile2 = ResultsDir & "\" & Replace(myFile.FileName, ".his", ".hia", , , CompareMethod.Text)

                        If File.Exists(fromFile) Then
                            While Setup.GeneralFunctions.IsFileLocked(fromFile)
                                System.Threading.Thread.Sleep(100)
                            End While
                            Call FileCopy(fromFile, toFile)
                        Else
                            Me.Setup.Log.AddError("Fout: uitvoerbestand bestaat niet: " & fromFile)
                        End If

                        If File.Exists(fromFile2) Then
                            While Setup.GeneralFunctions.IsFileLocked(fromFile2)
                                System.Threading.Thread.Sleep(100)
                            End While
                            Call FileCopy(fromFile2, toFile2)
                        Else
                            Me.Setup.Log.AddWarning("Uitvoerbestand bestaat niet: " & fromFile2 & ". resultaten voor ID's langer dan 20 karakters kunnen daarom niet correct worden uitgelezen.")
                        End If


                    Else
                        Me.Setup.Log.AddError("Error copying results file to results folder. Modeltype not yet supported: " & Model.ModelType.ToString)
                    End If

                Next
            End If

            'Debug.Print("After writing the results the current thread " & Threading.Thread.CurrentThread.ManagedThreadId & " has state: " & Threading.Thread.CurrentThread.ThreadState.ToString)

            'let's clean up after ourselves, shall we?
            If Simulations.RemoveRunsWhenDone Then
                System.IO.Directory.Delete(SimulationDir, True)
            End If
            'Me.Setup.GeneralFunctions.UpdateProgressBar("Simulations complete.", 10, 10, True)
            'Debug.Print("At the end of the sub the results the current thread " & Threading.Thread.CurrentThread.ManagedThreadId & " has state: " & Threading.Thread.CurrentThread.ThreadState.ToString)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in sub Run of class clsSobekSimulation.")
            Me.Setup.Log.AddError(ex.Message)
            'Debug.Print("Exception was thrown in thread " & Threading.Thread.CurrentThread.ManagedThreadId & ". Current state is: " & Threading.Thread.CurrentThread.ThreadState.ToString)
            Return False
        End Try
    End Function

    Public Function WriteMeteoFiles() As Boolean
        Try
            buiFile = Simulations.MeteoDir & "\Event" & Bui.number & ".bui"
            EvpFile = Simulations.MeteoDir & "\Event" & Bui.number & ".evp"
            QscFile = Simulations.MeteoDir & "\Event" & Bui.number & ".qsc"
            WdcFile = Simulations.MeteoDir & "\Event" & Bui.number & ".wdc"
            QwcFile = Simulations.MeteoDir & "\Event" & Bui.number & ".qwc"
            TmpFile = Simulations.MeteoDir & "\Event" & Bui.number & ".tmp"
            RnfFile = Simulations.MeteoDir & "\Event" & Bui.number & ".rnf"
            If Not Me.Setup.GeneralFunctions.AbsoluteToRelativePath(SimulationDir & "\CMTWORK\", buiFile, BuiFileRelative) Then Throw New Exception("Error writing relative path for .bui file.")
            If Not Me.Setup.GeneralFunctions.AbsoluteToRelativePath(SimulationDir & "\CMTWORK\", EvpFile, EvpFileRelative) Then Throw New Exception("Error writing relative path for .evp file.")
            If Not Me.Setup.GeneralFunctions.AbsoluteToRelativePath(SimulationDir & "\CMTWORK\", QscFile, QscFileRelative) Then Throw New Exception("Error writing relative path for .qsc file.")
            If Not Me.Setup.GeneralFunctions.AbsoluteToRelativePath(SimulationDir & "\CMTWORK\", WdcFile, WdcFileRelative) Then Throw New Exception("Error writing relative path for .wdc file.")
            If Not Me.Setup.GeneralFunctions.AbsoluteToRelativePath(SimulationDir & "\CMTWORK\", QwcFile, QwcFileRelative) Then Throw New Exception("Error writing relative path for .qwc file.")
            If Not Me.Setup.GeneralFunctions.AbsoluteToRelativePath(SimulationDir & "\CMTWORK\", TmpFile, TmpFileRelative) Then Throw New Exception("Error writing relative path for .tmp file.")
            If Not Me.Setup.GeneralFunctions.AbsoluteToRelativePath(SimulationDir & "\CMTWORK\", RnfFile, RnfFileRelative) Then Throw New Exception("Error writing relative path for .rnf file.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function WriteMeteoFiles of class clsSobekSimulation.")
            Return False
        End Try
    End Function
End Class
