Imports System.Windows.Forms
Imports System.IO
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.Threading
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Runtime.InteropServices.ComTypes

Public Class clsStochastenRun

    '----------------------------------------------------------------------------------------
    'Author: Siebe Bosch
    'Date: 10-8-2014
    'This class contains the blueprint for one run in a stochastic analysis.
    '----------------------------------------------------------------------------------------

    Public ID As String
    Public Idx As Integer   'the index number of our run
    Public P As Double  'the resulting probability of all underlying stochastic classes

    Public IDexceptVolume As String
    Public InputFilesDir As String              'directory for all input files
    Public OutputFilesDir As String             'directory for all output files
    Public RelativeDir As String           'same directory, but relative to the Stochastenanalyse root dir
    Public RelativeOutputDir As String          'same directory, but relative to the Stochastenanalyse root dir
    Public SeasonClass As clsStochasticSeasonClass
    Public VolumeClass As clsStochasticVolumeClass
    Public PatternClass As clsStochasticPatternClass
    Public GWClass As clsStochasticGroundwaterClass
    Public WLClass As clsStochasticWaterLevelClass
    Public WindClass As clsStochasticWindClass
    Public Extra1Class As clsStochasticExtraClass
    Public Extra2Class As clsStochasticExtraClass
    Public Extra3Class As clsStochasticExtraClass
    Public Extra4Class As clsStochasticExtraClass

    Public Klimaatscenario As STOCHLIB.GeneralFunctions.enmKlimaatScenario
    Public duur As Integer

    Public ExtraModelInputFilesDir As String 'directory for all extra model input files

    Public BuiFile As String 'path to the bui-file
    Public EvpFile As String 'path to the evp-file
    Public QscFile As String 'path to the evp-file
    Public WdcFile As String 'path to the evp-file
    Public QwcFile As String 'path to the evp-file
    Public TmpFile As String 'path to the evp-file
    Public RnfFile As String 'path to the evp-file
    Public BuiFileRelative As String 'relative path to the bui-file (for use in casedesc.cmt)
    Public EvpFileRelative As String 'relative path to the evp-file (for use in casedesc.cmt)
    Public QscFileRelative As String 'relative path to the evp-file (for use in casedesc.cmt)
    Public WdcFileRelative As String 'relative path to the evp-file (for use in casedesc.cmt)
    Public QwcFileRelative As String 'relative path to the evp-file (for use in casedesc.cmt)
    Public TmpFileRelative As String 'relative path to the evp-file (for use in casedesc.cmt)
    Public RnfFileRelative As String 'relative path to the evp-file (for use in casedesc.cmt)

    Private StochastenAnalyse As clsStochastenAnalyse 'from inside each run we want access to the properties of the overall analysis
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByRef myAnalyse As clsStochastenAnalyse)
        Setup = mySetup
        StochastenAnalyse = myAnalyse
    End Sub

    Public Function getModelCaseDir(ByRef myModel As clsSimulationModel) As String
        Return myModel.TempWorkDir & "\" & ID
    End Function

    Public Function getExeDir(ByRef myModel As clsSimulationModel) As String
        If myModel.RunLocalCopy Then
            Return myModel.TempWorkDir & "\" & ID
        Else
            Return Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec)
        End If
    End Function

    Public Function getExePath(ByRef myModel As clsSimulationModel) As String
        Return getExeDir(myModel) & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(myModel.Exec)
    End Function

    Public Function getMeteoForcing() As clsMeteoForcing
        'creates and returns the meteo forcing for this run so it can be passed to a DHYDROSERVER implementation
        Dim myForcing As New clsMeteoForcing(duur, VolumeClass.Volume, PatternClass.Patroon, SeasonClass.Name)
        Return myForcing
    End Function

    Public Function GetFlowForcing() As clsFlowForcing
        'creates and returns the flow forcing for this run so it can be passed to a DHYDROSERVER implementation
        Dim myForcing As New clsFlowForcing(WLClass.ID, SeasonClass.Name)
        Return myForcing
    End Function

    Public Function CalcIDExcepVolume() As String
        'the most robust method to calculate this ID is to get rid of the string containing Volume
        Dim VolStr As String = VolumeClass.Volume.ToString & "mm_"
        IDexceptVolume = Replace(ID, VolStr, "")
        Return IDexceptVolume
    End Function

    Public Function GetID() As String
        Return ID
    End Function

    Public Function GetIDexceptVolume() As String
        Return IDexceptVolume
    End Function

    Public Sub AddToGrid(ByRef myGrid As DataGridView)

        Dim Pars(12)
        Pars(0) = GetID()
        Pars(1) = SeasonClass.Name.ToString
        Pars(2) = duur
        If VolumeClass IsNot Nothing Then Pars(3) = VolumeClass.Volume 'neerslagvolume
        If PatternClass IsNot Nothing Then Pars(4) = PatternClass.Patroon.ToString 'neerslagpatroon
        If GWClass IsNot Nothing Then Pars(5) = GWClass.ID 'initiele grondwaterdiepte
        If WLClass IsNot Nothing Then Pars(6) = WLClass.ID 'waterhoogte
        If WindClass IsNot Nothing Then Pars(7) = WindClass.ID 'wind
        If Extra1Class IsNot Nothing Then Pars(8) = Extra1Class.ID 'bijv. ruwheid, gemaalfalen, sliblaag
        If Extra2Class IsNot Nothing Then Pars(9) = Extra2Class.ID 'bijv. ruwheid, gemaalfalen, sliblaag
        If Extra3Class IsNot Nothing Then Pars(10) = Extra3Class.ID 'bijv. ruwheid, gemaalfalen, sliblaag
        If Extra4Class IsNot Nothing Then Pars(10) = Extra4Class.ID 'bijv. ruwheid, gemaalfalen, sliblaag
        Pars(11) = calcP()
        Pars(12) = False
        myGrid.Rows.Add(Pars)

    End Sub

    Public Function calcP() As Double
        Dim P As Double = SeasonClass.P
        If VolumeClass IsNot Nothing Then P *= VolumeClass.P
        If PatternClass IsNot Nothing Then P *= PatternClass.p
        If GWClass IsNot Nothing Then P *= GWClass.p
        If WLClass IsNot Nothing Then P *= WLClass.p
        If WindClass IsNot Nothing Then P *= WindClass.P
        If Extra1Class IsNot Nothing Then P *= Extra1Class.p
        If Extra2Class IsNot Nothing Then P *= Extra2Class.p
        If Extra3Class IsNot Nothing Then P *= Extra3Class.p
        If Extra4Class IsNot Nothing Then P *= Extra4Class.p
        Return P
    End Function

    Public Function ConsoleLogLocalVariables()
        ' Get and print all environment variables
        Dim environmentVariables As IDictionary = Environment.GetEnvironmentVariables()
        Console.WriteLine("Environment Variables:")
        For Each de As DictionaryEntry In environmentVariables
            Console.WriteLine($"{de.Key} = {de.Value}")
        Next

        ' Example of reading registry entries
        ' You need to modify the key path and value names according to your needs
        Try
            '' Reading a sample registry key (replace with the actual key you need)
            'Dim regKey As RegistryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion")

            'If regKey IsNot Nothing Then
            '    Console.WriteLine(vbCrLf & "Registry Entries:")

            '    ' Replace "ProductName" with the actual registry value you want to read
            '    Dim value As Object = regKey.GetValue("ProductName")
            '    If value IsNot Nothing Then
            '        Console.WriteLine("ProductName: " & value.ToString())
            '    End If

            '    regKey.Close()
            'End If
        Catch ex As Exception
            Console.WriteLine("Error reading registry: " & ex.Message)
        End Try

        Console.WriteLine("Press any key to exit...")
        Console.ReadKey()
    End Function

    Public Function DeleteRun() As Boolean
        Try
            For Each myModel As clsSimulationModel In StochastenAnalyse.Models.Values
                Dim runDir As String = getModelCaseDir(myModel) ' myModel.TempWorkDir & "\" & ID
                'delete the run directory and its contents
                If System.IO.Directory.Exists(runDir) Then
                    System.IO.Directory.Delete(runDir, True)
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError($"Unable to delete simulation: {ID}:" & ex.Message)
            Return False
        End Try
    End Function
    Public Function CopyResultsFiles(Optional ByVal runIdx As Long = 0, Optional ByVal nRuns As Long = 1) As Boolean
        '------------------------------------------------------------------------
        'author: Siebe Bosch
        'date: 16-8-2014
        'this routine copies the requested results files from the temporary directory to the output directory
        '------------------------------------------------------------------------
        Dim fromFile As String = String.Empty, fromFile2 As String = String.Empty
        Dim toFile As String, toFile2 As String
        Dim nErrors As Integer = 0
        Try

            For Each myModel As clsSimulationModel In StochastenAnalyse.Models.Values

                'for each run we need to create a unique subdirectory to myModel.TempWorkDir
                Dim runDir As String = getModelCaseDir(myModel) ' myModel.TempWorkDir & "\" & ID
                If Not System.IO.Directory.Exists(runDir) Then
                    Me.Setup.Log.AddError("Unable to copy simulation results to the results directory. The directory " & runDir & " which should contain the results does not exist.")
                    nErrors += 1
                    Continue For
                End If

                If myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.DIMR Then

                    Setup.GeneralFunctions.UpdateProgressBar("Copying results for model " & myModel.CaseName, 0, 10, True)
                    Dim myProject = New clsDIMR(Me.Setup, myModel.ModelDir)

                    'update the progress bar
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Copying results for simulation " & runIdx & " Of " & nRuns & ": " & ID, runIdx, nRuns, True)

                    'If logStr = "0" OrElse StochastenAnalyse.AllowCrashedResults Then
                    'v2.3.2: from dir was incorrect. Replaced myproject.projectdir by myModel.tempWorkdir
                    For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values
                        If myModel.ModelType = enmSimulationModel.SOBEK Then
                            fromFile = runDir & "\WORK\" & myFile.FileName
                        ElseIf myModel.ModelType = enmSimulationModel.DIMR Then
                            If myFile.HydroModule = enmHydroModule.RR Then
                                fromFile = runDir & "\" & myProject.DIMRConfig.RR.SubDir & "\" & myFile.FileName
                            ElseIf myFile.HydroModule = enmHydroModule.FLOW Then
                                fromFile = runDir & "\" & myProject.DIMRConfig.Flow1D.SubDir & "\output\" & myFile.FileName
                            ElseIf myFile.HydroModule = enmHydroModule.RTC Then
                                fromFile = runDir & "\" & myProject.DIMRConfig.RTC.SubDir & "\" & myFile.FileName
                            End If
                        Else
                            fromFile = runDir & "\" & myFile.FileName
                        End If
                        toFile = OutputFilesDir & "\" & myFile.FileName
                        GeneralFunctions.EnsureDirectoryPathExists(OutputFilesDir)

                        If File.Exists(fromFile) Then
                            Call FileCopy(fromFile, toFile)
                        Else
                            Me.Setup.Log.AddError("Fout: uitvoerbestand niet gevonden: " & fromFile & ". Check of de uitvoerbestanden van het model juist gedefinieerd zijn.")
                            nErrors += 1
                        End If
                    Next

                ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.SOBEK Then

                    Setup.GeneralFunctions.UpdateProgressBar("Copying results... ", 0, 10, True)
                    Dim myProject = New clsSobekProject(Me.Setup, myModel.ModelDir, Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec), True)

                    'if the run was succesful, copy the files
                    Dim logReader As New StreamReader(runDir & "\CMTWORK\PLUVIUS1.RTN")
                    Dim logStr As String = logReader.ReadLine.Trim
                    logReader.Close()

                    'warning if results are used although simulation crashed
                    If logStr <> "0" AndAlso StochastenAnalyse.AllowCrashedResults Then Me.Setup.Log.AddWarning("Simulatie " & ID & " was niet succesvol, maar resultaat werd toch gebruikt in de nabewerking, conform uw instellingen.")

                    If logStr = "0" OrElse StochastenAnalyse.AllowCrashedResults Then
                        For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values
                            fromFile = runDir & "\WORK\" & myFile.FileName
                            fromFile2 = runDir & "\WORK\" & Replace(myFile.FileName, ".his", ".hia", , , CompareMethod.Text)
                            toFile = OutputFilesDir & "\" & myFile.FileName
                            toFile2 = OutputFilesDir & "\" & Replace(myFile.FileName, ".his", ".hia", , , CompareMethod.Text)

                            If File.Exists(fromFile) Then
                                Call FileCopy(fromFile, toFile)
                            Else
                                Me.Setup.Log.AddError("Fout: uitvoerbestand bestaat niet: " & fromFile & ". Check of de uitvoerbestanden van het model juist gedefinieerd zijn.")
                                nErrors += 1
                            End If

                            If File.Exists(fromFile2) Then
                                Call FileCopy(fromFile2, toFile2)
                            Else
                                Me.Setup.Log.AddWarning("Uitvoerbestand bestaat niet: " & fromFile2 & ". resultaten voor ID's langer dan 20 karakters kunnen daarom niet correct worden uitgelezen.")
                                nErrors += 1
                            End If

                        Next
                    Else
                        Throw New Exception("Simulation for stochastic combination " & ID & " was unsuccessful.")
                    End If

                ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.SUMAQUA Then
                    For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values
                        fromFile = runDir & "\OUTPUT\" & myFile.FileName
                        toFile = OutputFilesDir & "\" & myFile.FileName

                        If File.Exists(fromFile) Then
                            Call FileCopy(fromFile, toFile)
                        Else
                            Me.Setup.Log.AddError("Fout: uitvoerbestand niet gevonden: " & fromFile & ". Check of de uitvoerbestanden van het model juist gedefinieerd zijn.")
                            nErrors += 1
                        End If

                    Next
                End If
            Next

            If nErrors > 0 Then Return False Else Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error copying results files: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function Build(Optional ByVal runIdx As Long = 0, Optional ByVal nRuns As Long = 1) As Boolean
        '------------------------------------------------------------------------
        'author: Siebe Bosch
        'date: 16-8-2014
        'this routine actually kicks off a model run!
        '------------------------------------------------------------------------

        Try

            'now copy the model to its temporary working location
            For Each myModel As clsSimulationModel In StochastenAnalyse.Models.Values

                'for each run we need to create a unique subdirectory to myModel.TempWorkDir
                'in case of a HBV model we need to include a DAT subdirectory
                Dim runDir As String = If(myModel.ModelType = enmSimulationModel.HBV, myModel.TempWorkDir & "\DAT\" & ID, myModel.TempWorkDir & "\" & ID)
                If Not System.IO.Directory.Exists(runDir) Then System.IO.Directory.CreateDirectory(runDir)

                '--------------------------------------------------------------------------------------------
                '    wait until the database is free to use. Then lock it for the current simulation
                '--------------------------------------------------------------------------------------------
                'Me.Setup.GeneralFunctions.DatabaseWaitForUnlock(Me.Setup.StochastenAnalyse.StochastsConfigFile, 60)
                'Me.Setup.GeneralFunctions.DatabaseWriteLockFile(Me.Setup.StochastenAnalyse.StochastsConfigFile, ID)

                If myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.DIMR Then
                    If Not BuildDIMRModelRun(myModel, runDir, runIdx, nRuns) Then Me.Setup.Log.AddError("Unable to build run in " & runDir & ". Skipping.")
                ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.SOBEK Then
                    If BuildSobekModelRun(myModel, runDir) Then Me.Setup.Log.AddError("Unable to build run in " & runDir & ". Skipping.")
                ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.HBV Then
                    If Not BuildHBVModelRun(myModel, myModel.TempWorkDir, runDir, ID) Then Me.Setup.Log.AddError("Unable to build run in " & runDir & ". Skipping.")
                ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.SUMAQUA Then
                    If Not BuildSumaquaModelRun(myModel, runDir) Then Me.Setup.Log.AddError("Unable to build run in " & runDir & ". Skipping.")
                End If

                If System.IO.Directory.Exists(ExtraModelInputFilesDir) Then
                    Dim di As New DirectoryInfo(ExtraModelInputFilesDir)
                    Dim files As IO.FileInfo() = di.GetFiles("*.*", SearchOption.AllDirectories)

                    For Each file As IO.FileInfo In files
                        ' Get the relative path of the file
                        Dim relativePath As String = file.FullName.Substring(ExtraModelInputFilesDir.Length + 1)

                        ' Combine runDir with the relative path to get the new destination
                        Dim destPath As String = Path.Combine(runDir, relativePath)

                        ' Create the directory structure if it doesn't exist
                        Dim destDir As String = Path.GetDirectoryName(destPath)
                        If Not Directory.Exists(destDir) Then
                            Directory.CreateDirectory(destDir)
                        End If

                        ' Copy the file to the new destination
                        System.IO.File.Copy(file.FullName, destPath, True)
                    Next
                End If


                '----------------------------------------------------------------------------------------
                'release the database for use by other instances
                '----------------------------------------------------------------------------------------
                Me.Setup.GeneralFunctions.DatabaseReleaseLock(Me.Setup.StochastenAnalyse.StochastsConfigFile)

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function

    Public Function BuildDIMRModelRun(ByRef myModel As clsSimulationModel, runDir As String, runIdx As Integer, nRuns As Integer) As Boolean
        Try

            'create the paths to the meteo files, both absolute and relative. Meteofiles reside in the RR module
            Setup.GeneralFunctions.UpdateProgressBar("Preparing meteo files. ", runIdx + 1, nRuns, True)
            Dim myMeteoDir As String = runDir & "\" & Me.Setup.DIMRData.DIMRConfig.RR.GetSubDir & "\"

            'copy the original project to the temporary work dir and then read it from the new location
            Setup.GeneralFunctions.UpdateProgressBar($"Cloning model schematisation for run {runIdx + 1} of {nRuns}...", runIdx + 1, nRuns, True)
            Dim myProject = New clsDIMR(Me.Setup, myModel.ModelDir)

            myProject.CloneAndAdjustCaseForCommandLineRun(runDir, SeasonClass.EventStart, SeasonClass.EventStart.AddHours(StochastenAnalyse.Duration + StochastenAnalyse.DurationAdd))

            'create the meteo files and copy them into the case directory
            If Me.Setup.DIMRData.RR IsNot Nothing Then
                Dim BuiName As String = Me.Setup.DIMRData.DIMRConfig.RR.GetBuiFileName
                Dim EvpName As String = Me.Setup.DIMRData.DIMRConfig.RR.GetEvpFileName
                BuiFile = myMeteoDir & BuiName
                EvpFile = myMeteoDir & EvpName

                Dim myBui As New clsBuiFile(Me.Setup)
                Setup.GeneralFunctions.UpdateProgressBar("Retrieving rainfall pattern.", 0, 10, True)
                Dim res As (Boolean, Double()) = StochastenAnalyse.getBuiVerloop(PatternClass.Patroon, SeasonClass.Name, myModel.ModelType)
                If Not res.Item1 Then Throw New Exception("Error getting the rainfall pattern.")
                For Each Station As clsMeteoStation In StochastenAnalyse.MeteoStations.MeteoStations.Values
                    If Station.StationType = enmMeteoStationType.precipitation Then
                        Setup.GeneralFunctions.UpdateProgressBar("Building rainfall data.", 0, 10, True)
                        myBui.BuildSTOWATYPE(Station.Name, SeasonClass.Name, PatternClass.Patroon, VolumeClass.Volume, SeasonClass.Volume_Multiplier, SeasonClass.EventStart, res.Item2, StochastenAnalyse.DurationAdd, Station.gebiedsreductie, Station.ConstantFactor, Station.oppervlak)
                    ElseIf Station.StationType = enmMeteoStationType.evaporation Then
                        Setup.GeneralFunctions.UpdateProgressBar("Building evaporation data.", 0, 10, True)
                        myBui.BuildLongTermEVAP(SeasonClass.Name, StochastenAnalyse.Duration, StochastenAnalyse.DurationAdd)
                    End If
                Next
                Setup.GeneralFunctions.UpdateProgressBar("Writing rainfall event.", 0, 10, True)
                myBui.Write(BuiFile, 3)

                'copy the precipitation file to the unique directory for our desired run
                If Not System.IO.Directory.Exists(InputFilesDir) Then System.IO.Directory.CreateDirectory(InputFilesDir)
                File.Copy(BuiFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(BuiFile), True)

                'write the evaporation file for now we'll assume zero evaporation
                Dim myEvp As New clsEvpFile(Me.Setup)
                Dim Evap(Setup.GeneralFunctions.RoundUD(duur / 24, 0, True)) As Double
                Setup.GeneralFunctions.UpdateProgressBar("Writing evaporation event.", 0, 10, True)
                myEvp.BuildSTOWATYPE(Evap, SeasonClass.EventStart, StochastenAnalyse.DurationAdd)
                myEvp.Write(EvpFile)
                File.Copy(EvpFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(EvpFile), True)

                '--------------------------------------------------------------------------------------------------------------------
                'copy the groundwater file(s)
                '--------------------------------------------------------------------------------------------------------------------
                If GWClass IsNot Nothing Then
                    If GWClass.RRFiles IsNot Nothing AndAlso GWClass.RRFiles.Count > 0 Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                        If Not CopyGroundwaterFiles(myModel, runDir, GWClass.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception($"Fout bij het kopieren van de grondwaterbestanden {GWClass.RRFiles}")
                    End If
                    If GWClass.FlowFiles IsNot Nothing AndAlso GWClass.FlowFiles.Count > 0 Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                        If Not CopyGroundwaterFiles(myModel, runDir, GWClass.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception($"Fout bij het kopieren van de grondwaterbestanden {GWClass.FlowFiles}")
                    End If
                    If GWClass.RTCFiles IsNot Nothing AndAlso GWClass.RTCFiles.Count > 0 Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                        If Not CopyGroundwaterFiles(myModel, runDir, GWClass.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception($"Fout bij het kopieren van de grondwaterbestanden {GWClass.RTCFiles}")
                    End If
                End If
                '--------------------------------------------------------------------------------------------------------------------

            End If


            If Me.Setup.DIMRData.FlowFM IsNot Nothing Then

                '--------------------------------------------------------------------------------------------------------------------
                'create the boundary file
                If WLClass IsNot Nothing Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying file for boundaries.", 0, 10, True)
                    If Not BuildWaterLevelBoundaries(myModel, runDir) Then Throw New Exception("Fout bij het aanmaken van het randvoorwaardenbestand.")
                End If
                '--------------------------------------------------------------------------------------------------------------------

                '--------------------------------------------------------------------------------------------------------------------
                'copy the RR files for the extra1 stochast
                If Extra1Class IsNot Nothing Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying Files for stochast extra1.", 0, 10, True)
                    If Extra1Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra1Class.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra1 stochast.")
                    If Extra1Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra1Class.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
                    If Extra1Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra1Class.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception("Fout bij het kopieren van de RTC-bestanden voor de extra1 stochast.")
                End If
                '--------------------------------------------------------------------------------------------------------------------

                '--------------------------------------------------------------------------------------------------------------------
                'copy the file for the extra2 stochast
                If Extra2Class IsNot Nothing Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra2.", 0, 10, True)
                    If Extra2Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra2Class.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra2 stochast.")
                    If Extra2Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra2Class.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
                    If Extra2Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra2Class.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception("Fout bij het kopieren van de RTC-bestanden voor de extra2 stochast.")
                End If
                '--------------------------------------------------------------------------------------------------------------------

                '--------------------------------------------------------------------------------------------------------------------
                'copy the file for the extra3 stochast
                If Extra3Class IsNot Nothing Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra3.", 0, 10, True)
                    If Extra3Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra3Class.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra3 stochast.")
                    If Extra3Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra3Class.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
                    If Extra3Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra3Class.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception("Fout bij het kopieren van de RTC-bestanden voor de extra3 stochast.")
                End If
                '--------------------------------------------------------------------------------------------------------------------

                '--------------------------------------------------------------------------------------------------------------------
                'copy the file for the extra4 stochast
                If Extra4Class IsNot Nothing Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra4.", 0, 10, True)
                    If Extra4Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra4Class.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra4 stochast.")
                    If Extra4Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra4Class.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
                    If Extra4Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra4Class.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception("Fout bij het kopieren van de RTC-bestanden voor de extra4 stochast.")
                End If
                '--------------------------------------------------------------------------------------------------------------------

            End If

            'update the progress bar
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building simulation " & runIdx & " of " & nRuns & ": " & ID, runIdx, nRuns, True)

        Catch ex As Exception

        End Try
    End Function


    Public Function BuildSobekModelRun(ByRef myModel As clsSimulationModel, rundir As String) As Boolean
        Try

            'create the paths to the meteo files, both absolute and relative
            Setup.GeneralFunctions.UpdateProgressBar("Preparing meteo files. ", 0, 10, True)
            Dim myMeteoDir As String = rundir & "\METEO\" & SeasonClass.Name.ToString & "_" & PatternClass.Patroon.ToString & "_" & VolumeClass.Volume & "mm\"
            If Not Directory.Exists(myMeteoDir) Then Directory.CreateDirectory(myMeteoDir)
            BuiFile = myMeteoDir & "meteo.bui"
            EvpFile = myMeteoDir & "meteo.evp"
            QscFile = myMeteoDir & "meteo.qsc"
            WdcFile = myMeteoDir & "meteo.wdc"
            QwcFile = myMeteoDir & "meteo.qwc"
            TmpFile = myMeteoDir & "meteo.tmp"
            RnfFile = myMeteoDir & "meteo.rnf"
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(rundir & "\CMTWORK\", BuiFile, BuiFileRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(rundir & "\CMTWORK\", EvpFile, EvpFileRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(rundir & "\CMTWORK\", QscFile, QscFileRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(rundir & "\CMTWORK\", WdcFile, WdcFileRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(rundir & "\CMTWORK\", QwcFile, QwcFileRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(rundir & "\CMTWORK\", TmpFile, TmpFileRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(rundir & "\CMTWORK\", RnfFile, RnfFileRelative)

            'copy the original project to the temporary work dir and then read it from the new location
            Setup.GeneralFunctions.UpdateProgressBar("Cloning model schematisation. ", 0, 10, True)
            Dim myProject = New clsSobekProject(Me.Setup, myModel.ModelDir, Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec), True)

            'v2.040: changed mymodel.exec to mymodel.ModelDir. This fixes a bug for users who have their models on a different drive than their program
            If Not myProject.CloneCaseForCommandLineRun(Directory.GetParent(myModel.ModelDir).FullName, myModel.CaseName.Trim.ToUpper, rundir, BuiFileRelative, EvpFileRelative, QscFileRelative, WdcFileRelative, QwcFileRelative, TmpFileRelative, RnfFileRelative) Then Throw New Exception("Error: could not clone SOBEK case for running from the command line.")
            myProject = New clsSobekProject(Me.Setup, rundir, Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec), True)

            '--------------------------------------------------------------------------------------------------------------------
            'copy the groundwater file
            If GWClass IsNot Nothing Then
                If GWClass.RRFiles.Count > 0 Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                    If Not CopyGroundwaterFiles(myModel, rundir, GWClass.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                End If
                If GWClass.FlowFiles.Count > 0 Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                    If Not CopyGroundwaterFiles(myModel, rundir, GWClass.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                End If
                If GWClass.RTCFiles.Count > 0 Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                    If Not CopyGroundwaterFiles(myModel, rundir, GWClass.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                End If

            End If
            '--------------------------------------------------------------------------------------------------------------------

            '--------------------------------------------------------------------------------------------------------------------
            'create the boundary file
            If WLClass IsNot Nothing Then
                Setup.GeneralFunctions.UpdateProgressBar("Copying file for boundaries.", 0, 10, True)
                If Not BuildWaterLevelBoundaries(myModel, rundir) Then Throw New Exception("Fout bij het aanmaken van het randvoorwaardenbestand.")
            End If
            '--------------------------------------------------------------------------------------------------------------------

            '--------------------------------------------------------------------------------------------------------------------
            'copy the file for the extra1 stochast
            If Extra1Class IsNot Nothing Then
                Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra1.", 0, 10, True)
                If Not Extra1Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra1Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra1 stochast.")
                If Not Extra1Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra1Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
                If Not Extra1Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra1Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
            End If
            '--------------------------------------------------------------------------------------------------------------------

            '--------------------------------------------------------------------------------------------------------------------
            'copy the file for the extra2 stochast
            If Extra2Class IsNot Nothing Then
                Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra2.", 0, 10, True)
                If Not Extra2Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra2Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra2 stochast.")
                If Not Extra2Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra2Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
                If Not Extra2Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra2Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
            End If
            '--------------------------------------------------------------------------------------------------------------------

            '--------------------------------------------------------------------------------------------------------------------
            'copy the file for the extra3 stochast
            If Extra3Class IsNot Nothing Then
                Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra3.", 0, 10, True)
                If Not Extra3Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra3Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra3 stochast.")
                If Not Extra3Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra3Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
                If Not Extra3Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra3Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
            End If
            '--------------------------------------------------------------------------------------------------------------------

            '--------------------------------------------------------------------------------------------------------------------
            'copy the file for the extra4 stochast
            If Extra4Class IsNot Nothing Then
                Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra4.", 0, 10, True)
                If Not Extra4Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra4Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra4 stochast.")
                If Not Extra4Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra4Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
                If Not Extra4Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, rundir, Extra4Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
            End If
            '--------------------------------------------------------------------------------------------------------------------

            'write the precipitation file and make a backup in the stochast directory
            Dim myBui As New clsBuiFile(Me.Setup)
            Setup.GeneralFunctions.UpdateProgressBar("Retrieving rainfall pattern.", 0, 10, True)
            Dim res As (Boolean, Double()) = StochastenAnalyse.getBuiVerloop(PatternClass.Patroon, SeasonClass.Name, myModel.ModelType)
            If Not res.Item1 Then Throw New Exception("Error retrieving the rainfall pattern from database.")
            For Each Station As clsMeteoStation In StochastenAnalyse.MeteoStations.MeteoStations.Values
                If Station.StationType = enmMeteoStationType.precipitation Then
                    Setup.GeneralFunctions.UpdateProgressBar("Building rainfall data.", 0, 10, True)
                    myBui.BuildSTOWATYPE(Station.Name, SeasonClass.Name, PatternClass.Patroon, VolumeClass.Volume, SeasonClass.Volume_Multiplier, SeasonClass.EventStart, res.Item2, StochastenAnalyse.DurationAdd, Station.gebiedsreductie, Station.ConstantFactor, Station.oppervlak)
                ElseIf Station.StationType = enmMeteoStationType.evaporation Then
                    Setup.GeneralFunctions.UpdateProgressBar("Building evaporation data.", 0, 10, True)
                    myBui.BuildLongTermEVAP(SeasonClass.Name, StochastenAnalyse.Duration, StochastenAnalyse.DurationAdd)
                End If
            Next
            Setup.GeneralFunctions.UpdateProgressBar("Writing rainfall event.", 0, 10, True)
            myBui.Write(BuiFile, 3)

            'copy the meteo file to the unique directory for our desired run
            If Not System.IO.Directory.Exists(InputFilesDir) Then System.IO.Directory.CreateDirectory(InputFilesDir)
            File.Copy(BuiFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(BuiFile), True)

            'write the evaporation file for now we'll assume zero evaporation
            Dim myEvp As New clsEvpFile(Me.Setup)
            Dim Evap(Setup.GeneralFunctions.RoundUD(duur / 24, 0, True)) As Double
            Setup.GeneralFunctions.UpdateProgressBar("Writing evaporation event.", 0, 10, True)
            myEvp.BuildSTOWATYPE(Evap, SeasonClass.EventStart, StochastenAnalyse.DurationAdd)
            myEvp.Write(EvpFile)
            File.Copy(EvpFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(EvpFile), True)

            Setup.GeneralFunctions.UpdateProgressBar("Writing radiation file.", 0, 10, True)
            Using myWriter As New StreamWriter(QscFile)
                myWriter.WriteLine("CONSTANTS   'TEMP' 'RAD'")
                myWriter.WriteLine("DATA        0 0")
            End Using
            File.Copy(QscFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(QscFile), True)

            Setup.GeneralFunctions.UpdateProgressBar("Writing wind file.", 0, 10, True)
            Using myWriter As New StreamWriter(WdcFile)
                myWriter.WriteLine("GLMT MTEO nm '(null)' ss 0 id '0' ci '-1' lc 9.9999e+009 wu 1")
                myWriter.WriteLine("wv tv 0 0 9.9999e+009 wd td 0 0 9.9999e+009 su 0 sh ts")
                myWriter.WriteLine("0 9.9999e+009 9.9999e+009 tu 0 tp tw 0 9.9999e+009 9.9999e+009 au 0 at ta 0")
                myWriter.WriteLine("9.9999e+009 9.9999e+009 mteo glmt")
            End Using
            File.Copy(WdcFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(WdcFile), True)

            Using mywriter As New StreamWriter(QwcFile)
                mywriter.WriteLine("CONSTANTS   'VWIND' 'WINDDIR'")
                mywriter.WriteLine("DATA       0 0")
            End Using
            File.Copy(QwcFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(QwcFile), True)

            Using mywriter As New StreamWriter(TmpFile)
                mywriter.WriteLine("")
            End Using
            File.Copy(TmpFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(TmpFile), True)

            Using mywriter As New StreamWriter(RnfFile)
                mywriter.WriteLine("")
            End Using
            File.Copy(RnfFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(RnfFile), True)

        Catch ex As Exception
            Me.Setup.Log.AddError("Error Integer Function BuildSobekModelRun Of MyClass clsStochastenRun: " & ex.Message)
        End Try
    End Function

    Public Function BuildHBVModelRun(ByRef myModel As clsSimulationModel, parDir As String, runDir As String, runID As String) As Boolean
        Try

            'copy the original project to the temporary work dir and then read it from the new location
            Setup.GeneralFunctions.UpdateProgressBar("Cloning model schematisation. ", 0, 10, True)
            Dim myProject = New clsHBVProject(Me.Setup, myModel.ModelDir, Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec), True)

            'we need to know the station numbers for the meteo stations by reading the original meteo files
            If Not Me.Setup.StochastenAnalyse.AssignMeteoStationNumbersFromHBVProject(myProject) Then Throw New Exception("Error assigning meteo station numbers.")

            'v2.040: changed mymodel.exec to mymodel.ModelDir. This fixes a bug for users who have their models on a different drive than their program
            Dim Startdate As Date = SeasonClass.EventStart
            Dim Enddate As Date = SeasonClass.EventStart.AddHours(StochastenAnalyse.Duration + StochastenAnalyse.DurationAdd)

            'create a runDir .par file
            Using parWriter As New StreamWriter(parDir & "\" & runID & ".par")
                parWriter.WriteLine("fileformat 1")
                parWriter.WriteLine($"district '{runID}'")
                parWriter.WriteLine($"Directory 'DAT\{runID}\'")
            End Using

            If Not myProject.CloneAndAdjustCaseForCommandLineRun(runDir, Startdate, Enddate) Then Throw New Exception("Error: could not clone HBV model for running from the command line.")

            'create a series of temperatures for this run
            Dim temp_Hourly As New clsHBVTempHourlyFile(Me.Setup)
            temp_Hourly.Build(Startdate, StochastenAnalyse.Duration + StochastenAnalyse.DurationAdd)
            temp_Hourly.Write(runDir & "\temp_hourly.txt")

            'create seq.par file which contains the start- and end date of our simulation
            Dim seqPar As New clsHBVSeqParFile(Me.Setup)
            seqPar.Build(Startdate, Enddate)
            seqPar.Write(runDir & "\seq.par")

            'initialize a new project object from the new location
            myProject = New clsHBVProject(Me.Setup, runDir, Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec), False)

            '--------------------------------------------------------------------------------------------------------------------
            'copy the groundwater files and/or folder
            If GWClass IsNot Nothing Then
                If GWClass.RootFolder.Length > 0 Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater folder.", 0, 10, True)
                    If Not CopyHBVGroundwaterFolder(myModel, runDir, GWClass.RootFolder, "") Then Throw New Exception("Fout bij het kopieren van de map met grondwaterbestanden.")
                End If
                If GWClass.RRFiles.Count > 0 Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                    If Not CopyGroundwaterFiles(myModel, runDir, GWClass.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                End If
                If GWClass.FlowFiles.Count > 0 Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                    If Not CopyGroundwaterFiles(myModel, runDir, GWClass.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                End If
                If GWClass.RTCFiles.Count > 0 Then
                    Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                    If Not CopyGroundwaterFiles(myModel, runDir, GWClass.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                End If

            End If
            '--------------------------------------------------------------------------------------------------------------------

            ''--------------------------------------------------------------------------------------------------------------------
            ''create the boundary file
            'If Not WLClass Is Nothing Then
            '    Setup.GeneralFunctions.UpdateProgressBar("Copying file for boundaries.", 0, 10, True)
            '    If Not BuildWaterLevelBoundaries(myModel, runDir) Then Throw New Exception("Fout bij het aanmaken van het randvoorwaardenbestand.")
            'End If
            ''--------------------------------------------------------------------------------------------------------------------

            ''--------------------------------------------------------------------------------------------------------------------
            ''copy the file for the extra1 stochast
            'If Extra1Class IsNot Nothing Then
            '    Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra1.", 0, 10, True)
            '    If Not Extra1Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 1, Extra1Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra1 stochast.")
            '    If Not Extra1Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 1, Extra1Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
            '    If Not Extra1Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 1, Extra1Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
            'End If
            ''--------------------------------------------------------------------------------------------------------------------

            ''--------------------------------------------------------------------------------------------------------------------
            ''copy the file for the extra2 stochast
            'If Extra2Class IsNot Nothing Then
            '    Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra2.", 0, 10, True)
            '    If Not Extra2Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 2, Extra2Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra2 stochast.")
            '    If Not Extra2Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 2, Extra2Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
            '    If Not Extra2Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 2, Extra2Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
            'End If
            ''--------------------------------------------------------------------------------------------------------------------

            ''--------------------------------------------------------------------------------------------------------------------
            ''copy the file for the extra3 stochast
            'If Extra3Class IsNot Nothing Then
            '    Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra3.", 0, 10, True)
            '    If Not Extra3Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 3, Extra3Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra3 stochast.")
            '    If Not Extra3Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 3, Extra3Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
            '    If Not Extra3Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 3, Extra3Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
            'End If
            ''--------------------------------------------------------------------------------------------------------------------

            ''--------------------------------------------------------------------------------------------------------------------
            ''copy the file for the extra4 stochast
            'If Extra4Class IsNot Nothing Then
            '    Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra4.", 0, 10, True)
            '    If Not Extra4Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 4, Extra4Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra4 stochast.")
            '    If Not Extra4Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 4, Extra4Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
            '    If Not Extra4Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, 4, Extra4Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
            'End If
            ''--------------------------------------------------------------------------------------------------------------------

            'write the precipitation file and make a backup in the stochast directory
            Setup.GeneralFunctions.UpdateProgressBar("Preparing meteo files. ", 0, 10, True)

            'Dim myMeteoDir As String = runDir & "\METEO\" & SeasonClass.Name.ToString & "_" & PatternClass.Patroon.ToString & "_" & VolumeClass.Volume & "mm\"
            'If Not Directory.Exists(myMeteoDir) Then Directory.CreateDirectory(myMeteoDir)
            Dim res As (Boolean, Double()) = StochastenAnalyse.getBuiVerloop(PatternClass.Patroon, SeasonClass.Name, myModel.ModelType)
            If res.Item1 = False Then Throw New Exception("Error retrieving the rainfall pattern from database.")
            For Each Station As clsMeteoStation In StochastenAnalyse.MeteoStations.MeteoStations.Values
                If Station.StationType = enmMeteoStationType.precipitation Then
                    Dim myBui As New clsBuiFile(Me.Setup)
                    Setup.GeneralFunctions.UpdateProgressBar("Building rainfall data.", 0, 10, True)
                    'generate a HBV rainfall file and set both the absolute and relative paths
                    BuiFile = runDir & "\" & Station.ID & ".txt"
                    Me.Setup.GeneralFunctions.AbsoluteToRelativePath(runDir, BuiFile, BuiFileRelative)
                    myBui.BuildSTOWATYPE(Station.Name, SeasonClass.Name, PatternClass.Patroon, VolumeClass.Volume, SeasonClass.Volume_Multiplier, SeasonClass.EventStart, res.Item2, StochastenAnalyse.DurationAdd, Station.gebiedsreductie, Station.ConstantFactor, Station.oppervlak)
                    myBui.WriteHBV(BuiFile, Station, 3)
                ElseIf Station.StationType = enmMeteoStationType.evaporation Then
                    'Setup.GeneralFunctions.UpdateProgressBar("Building evaporation data.", 0, 10, True)
                    'myBui.BuildLongTermEVAP(SeasonClass.Name, StochastenAnalyse.Duration, StochastenAnalyse.DurationAdd)
                End If

                'copy the meteo file to the unique directory for our desired run
                If Not System.IO.Directory.Exists(InputFilesDir) Then System.IO.Directory.CreateDirectory(InputFilesDir)
                File.Copy(BuiFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(BuiFile), True)

            Next
            Setup.GeneralFunctions.UpdateProgressBar("Writing rainfall event.", 0, 10, True)


            ''write the evaporation file for now we'll assume zero evaporation
            'Dim myEvp As New clsEvpFile(Me.Setup)
            'Dim Evap(Setup.GeneralFunctions.RoundUD(duur / 24, 0, True)) As Double
            'Setup.GeneralFunctions.UpdateProgressBar("Writing evaporation event.", 0, 10, True)
            'myEvp.BuildSTOWATYPE(Evap, SeasonClass.EventStart, StochastenAnalyse.DurationAdd)
            'myEvp.Write(EvpFile)
            'File.Copy(EvpFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(EvpFile), True)

            'Setup.GeneralFunctions.UpdateProgressBar("Writing radiation file.", 0, 10, True)
            'Using myWriter As New StreamWriter(QscFile)
            '    myWriter.WriteLine("CONSTANTS   'TEMP' 'RAD'")
            '    myWriter.WriteLine("DATA        0 0")
            'End Using
            'File.Copy(QscFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(QscFile), True)

            'Setup.GeneralFunctions.UpdateProgressBar("Writing wind file.", 0, 10, True)
            'Using myWriter As New StreamWriter(WdcFile)
            '    myWriter.WriteLine("GLMT MTEO nm '(null)' ss 0 id '0' ci '-1' lc 9.9999e+009 wu 1")
            '    myWriter.WriteLine("wv tv 0 0 9.9999e+009 wd td 0 0 9.9999e+009 su 0 sh ts")
            '    myWriter.WriteLine("0 9.9999e+009 9.9999e+009 tu 0 tp tw 0 9.9999e+009 9.9999e+009 au 0 at ta 0")
            '    myWriter.WriteLine("9.9999e+009 9.9999e+009 mteo glmt")
            'End Using
            'File.Copy(WdcFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(WdcFile), True)

            'Using mywriter As New StreamWriter(QwcFile)
            '    mywriter.WriteLine("CONSTANTS   'VWIND' 'WINDDIR'")
            '    mywriter.WriteLine("DATA       0 0")
            'End Using
            'File.Copy(QwcFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(QwcFile), True)

            'Using mywriter As New StreamWriter(TmpFile)
            '    mywriter.WriteLine("")
            'End Using
            'File.Copy(TmpFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(TmpFile), True)

            'Using mywriter As New StreamWriter(RnfFile)
            '    mywriter.WriteLine("")
            'End Using
            'File.Copy(RnfFile, InputFilesDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(RnfFile), True)

            '----------------------------------------------------------------------------------------
            'release the database for use by other instances
            '----------------------------------------------------------------------------------------
            Me.Setup.GeneralFunctions.DatabaseReleaseLock(Me.Setup.StochastenAnalyse.StochastsConfigFile)

        Catch ex As Exception

        End Try
    End Function

    Public Function BuildSumaquaModelRun(ByRef myModel As clsSimulationModel, runDir As String) As Boolean
        Try
            'for the SUMAQUA model we need to copy the entire model directory
            'for now only the stochasts Extra1 through 4 are supported

            'copy the original project to the temporary work dir and then read it from the new location
            Setup.GeneralFunctions.UpdateProgressBar("Cloning model schematisation. ", 0, 10, True)
            Dim myProject = New clsSumaquaProject(Me.Setup, myModel.ModelDir, myModel.CaseName)

            'v2.040: changed mymodel.exec to mymodel.ModelDir. This fixes a bug for users who have their models on a different drive than their program
            Dim Startdate As Date = SeasonClass.EventStart
            Dim Enddate As Date = SeasonClass.EventStart.AddHours(StochastenAnalyse.Duration + StochastenAnalyse.DurationAdd)

            If Not myProject.CloneAndAdjustCaseForCommandLineRun(runDir, Startdate, Enddate) Then Throw New Exception("Error: could not clone Sumaqua model for running from the command line.")

            'initialize a new project object from the new location
            myProject = New clsSumaquaProject(Me.Setup, runDir, myModel.CaseName)

            '--------------------------------------------------------------------------------------------------------------------
            'copy the file for the extra1 stochast
            If Extra1Class IsNot Nothing Then
                Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra1.", 0, 10, True)
                If Extra1Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra1Class.RRFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra1 stochast.")
                If Extra1Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra1Class.FlowFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
                If Extra1Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra1Class.RTCFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
            End If
            '--------------------------------------------------------------------------------------------------------------------

            '--------------------------------------------------------------------------------------------------------------------
            'copy the file for the extra2 stochast
            If Extra2Class IsNot Nothing Then
                Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra2.", 0, 10, True)
                If Extra2Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra2Class.RRFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra2 stochast.")
                If Extra2Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra2Class.FlowFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
                If Extra2Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra2Class.RTCFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
            End If
            '--------------------------------------------------------------------------------------------------------------------

            '--------------------------------------------------------------------------------------------------------------------
            'copy the file for the extra3 stochast
            If Extra3Class IsNot Nothing Then
                Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra3.", 0, 10, True)
                If Extra3Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra3Class.RRFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra3 stochast.")
                If Extra3Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra3Class.FlowFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
                If Extra3Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra3Class.RTCFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
            End If
            '--------------------------------------------------------------------------------------------------------------------

            '--------------------------------------------------------------------------------------------------------------------
            'copy the file for the extra4 stochast
            If Extra4Class IsNot Nothing Then
                Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra4.", 0, 10, True)
                If Extra4Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra4Class.RRFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra4 stochast.")
                If Extra4Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra4Class.FlowFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
                If Extra4Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, runDir, Extra4Class.RTCFiles, "INPUT") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
            End If
            '--------------------------------------------------------------------------------------------------------------------

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error building Sumaqua model run: " & ex.Message)
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

    Public Function CopyHBVGroundwaterFolder(ByRef myModel As clsSimulationModel, RunDir As String, SourceFolder As String, ModelSubdir As String) As Boolean
        Try
            Dim SourceFolderAbsolute As String = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(SourceFolder, Setup.Settings.RootDir)

            'iterate throuh each subdir in the specified RootFolder
            For Each SourceSubDir As String In Directory.GetDirectories(SourceFolderAbsolute)
                Dim SourceSubDirName As String = Me.Setup.GeneralFunctions.FileNameFromPath(SourceSubDir)

                'since every catchment may have multiple subcatchments, starting with the same name as our SubDirName, we will now make a collection of all subdirs in RunDir &"\" & ModelSuDir that start with SubDirName
                Dim ModelInputDirs As String() = Directory.GetDirectories(RunDir & "\" & ModelSubdir)
                For Each ModelInputDir As String In ModelInputDirs

                    Dim PartOfPath As String
                    If ModelSubdir.Length > 0 Then
                        PartOfPath = RunDir & "\" & ModelSubdir & "\" & SourceSubDirName
                    Else
                        PartOfPath = RunDir & "\" & SourceSubDirName
                    End If

                    If ModelInputDir.StartsWith(PartOfPath) Then
                        'we found a match. Copy the contents of the subdirectory to the new location
                        For Each File As String In Directory.GetFiles(SourceSubDir)
                            FileCopy(File, ModelInputDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(File))
                        Next
                    End If
                Next
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CopyHBVGroundwaterFolder of class clsStochastenRun: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function CopyGroundwaterFiles(ByRef myModel As clsSimulationModel, RunDir As String, FileNames As List(Of String), ModelSubdir As String) As Boolean
        Dim fromFile As String, toFile As String, toStochastDir As String

        Try
            'v2.205: introducing multi-file support for groundwater stochast
            For Each FileName As String In FileNames
                fromFile = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(FileName, Setup.Settings.RootDir)

                'the target location depends on the type of model we're writing this stochast for
                If myModel.ModelType = enmSimulationModel.DIMR Then
                    toFile = RunDir & "\" & ModelSubdir & "\" & Setup.GeneralFunctions.FileNameFromPath(FileName)
                ElseIf myModel.ModelType = enmSimulationModel.SOBEK Then
                    toFile = RunDir & "\WORK\" & Setup.GeneralFunctions.FileNameFromPath(FileName)
                Else
                    Throw New Exception("Stochast initial groundwater not yet supported for requested model type: " & myModel.ModelType.ToString)
                End If

                toStochastDir = InputFilesDir & "\" & Setup.GeneralFunctions.FileNameFromPath(FileName)
                If File.Exists(fromFile) Then
                    FileCopy(fromFile, toFile)
                    FileCopy(fromFile, toStochastDir)
                Else
                    Throw New Exception("Fout: grondwaterbestand niet gevonden niet: " & fromFile)
                End If
            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function CopyExtraFiles(ByRef myModel As clsSimulationModel, RunDir As String, ExtraFiles As String, ModelSubdir As String) As Boolean
        Dim fromFile As String, toFile As String, toStochastDir As String

        Try

            While Not ExtraFiles = ""
                fromFile = Setup.GeneralFunctions.ParseString(ExtraFiles, ";")
                fromFile = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(fromFile, Me.Setup.Settings.RootDir)

                If Not System.IO.File.Exists(fromFile) Then
                    Throw New Exception("Fout: bestand voor extra stochast niet gevonden: " & fromFile)
                End If

                If myModel.ModelType = enmSimulationModel.SOBEK Then
                    toFile = RunDir & "\WORK\" & Setup.GeneralFunctions.FileNameFromPath(fromFile)
                ElseIf myModel.ModelType = enmSimulationModel.DIMR Then
                    toFile = RunDir & "\" & ModelSubdir & "\" & Setup.GeneralFunctions.FileNameFromPath(fromFile)
                ElseIf myModel.ModelType = enmSimulationModel.SUMAQUA Then
                    toFile = RunDir & "\" & ModelSubdir & "\" & Setup.GeneralFunctions.FileNameFromPath(fromFile)
                Else
                    Throw New Exception("Kan invoerbestand " & fromFile & " niet naar het doelmodel kopieren omdat het modeltype niet wordt ondersteund voor de onderhavige stochast: " & myModel.ModelType.ToString)
                End If

                Me.Setup.Log.AddMessage($"Copying file for extra stochast from {fromFile} to {toFile}...")

                toStochastDir = InputFilesDir & "\" & Setup.GeneralFunctions.FileNameFromPath(fromFile)
                FileCopy(fromFile, toFile)
                FileCopy(fromFile, toStochastDir)

            End While
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CopyExtraFiles: " & ex.Message)
            Return False
        End Try

    End Function

    Public Function BuildWaterLevelBoundaries(ByRef myModel As clsSimulationModel, RunDir As String) As Boolean

        'this function builds water level boundaries for a stochastic simulation
        'in case of SOBEK-simulations, the model must already have been read, including the boundary.dat file contents
        Dim r As Long
        Dim TableStart As DateTime
        Dim mySpan As New TimeSpan

        'query the database in order to retrieve the time series
        Dim cn As New SQLite.SQLiteConnection
        Dim da As SQLite.SQLiteDataAdapter
        Dim dt As New DataTable
        Dim query As String

        Try
            cn.ConnectionString = "Data Source=" & Me.Setup.StochastenAnalyse.StochastsConfigFile & ";Version=3;"
            If Not cn.State = ConnectionState.Open Then cn.Open()

            If myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.SOBEK Then
                Dim myBoundaryDat As New clsBoundaryDatFLBORecords(Me.Setup)
                myBoundaryDat = Me.Setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.BoundaryData.BoundaryDatFLBORecords.Clone

                'get the node id's
                For Each myNode As String In myModel.Boundaries
                    'get the boundary data from the record
                    Dim myRecord As clsBoundaryDatFLBORecord = myBoundaryDat.GetByID(myNode)
                    Dim WLTable As New clsSobekTable(Me.Setup)
                    Dim WindTable As New clsSobekTable(Me.Setup)

                    If myRecord IsNot Nothing Then

                        'query the database to retrieve the water level boundary timeseries
                        If SeasonClass.WaterLevelsUse Then
                            query = "SELECT MINUUT, " & myNode & " from RANDREEKSEN where NAAM='" & WLClass.ID & "' AND DUUR=" & Me.Setup.StochastenAnalyse.Duration & " ORDER BY MINUUT;"
                            da = New SQLite.SQLiteDataAdapter(query, cn)
                            dt = New DataTable
                            da.Fill(dt)

                            If dt.Rows.Count > 0 Then
                                'first subtract the difference between the start of the table and the start of the simulation
                                'also make sure the waterlevel boundary is set to be time dependent!
                                myRecord.h_wt = 1
                                myRecord.h_wt1 = 0
                                WLTable.pdin1 = 0
                                WLTable.pdin2 = 0
                                WLTable.PDINPeriod = "''"
                                TableStart = SeasonClass.EventStart
                                mySpan = TableStart.Subtract(SeasonClass.EventStart)
                                For r = 0 To dt.Rows.Count - 1
                                    WLTable.AddDatevalPair(TableStart.AddMinutes(dt.Rows(r)(0)), dt.Rows(r)(1))
                                Next
                                myRecord.HWTTable = WLTable
                            End If
                        End If

                    Else
                        Throw New Exception("Error: no record found in boundary.dat for node with ID " & myNode)
                    End If
                Next

                'export the adjusted boundary.dat file to the temporary working dir of the model AND the stochasts dir as a copy
                Using myWriter As New StreamWriter(RunDir & "\WORK\boundary.dat")
                    myBoundaryDat.Write(myWriter)
                End Using
                File.Copy(RunDir & "\WORK\boundary.dat", InputFilesDir & "\boundary.dat", True)

            ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.DIMR Then

                'first we must read the contents of boundaries.bc into memory
                Dim BoundariesBC As New STOCHLIB.clsBoundariesBC(Me.Setup, RunDir & "\" & Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir & "\boundaries.bc")
                BoundariesBC.Read()

                'now we must replace the timeseries as specified in this file's content
                'get the node id's
                If SeasonClass.WaterLevelsUse Then
                    For Each NodeID As String In myModel.Boundaries
                        'see if this boundary has a record inside BoundariesBC
                        If BoundariesBC.BoundaryConditions.ContainsKey(NodeID.Trim.ToUpper) Then
                            'we found the matching record! Now replace it with the requested series from our database

                            query = "SELECT MINUUT, WAARDE from RANDREEKSEN where NAAM='" & WLClass.ID & "' AND DUUR=" & Me.Setup.StochastenAnalyse.Duration & " AND NODEID='" & NodeID & "' ORDER BY MINUUT;"
                            da = New SQLite.SQLiteDataAdapter(query, cn)
                            dt = New DataTable
                            da.Fill(dt)

                            'write our timeseries to the boundaries.bc content
                            Dim myData As New clsSobekTable(Me.Setup)
                            For r = 0 To dt.Rows.Count - 1
                                myData.AddDataPair(2, dt.Rows(r)(0), dt.Rows(r)(1))
                            Next

                            BoundariesBC.BoundaryConditions.Item(NodeID.Trim.ToUpper).SetDatatable(myData)
                            BoundariesBC.BoundaryConditions.Item(NodeID.Trim.ToUpper).bcfunction = "timeseries"
                            BoundariesBC.BoundaryConditions.Item(NodeID.Trim.ToUpper).timeInterpolation = "linear"
                            BoundariesBC.BoundaryConditions.Item(NodeID.Trim.ToUpper).quantities = New Dictionary(Of String, clsQuantity)
                            BoundariesBC.BoundaryConditions.Item(NodeID.Trim.ToUpper).quantities.Add("TIME", New clsQuantity("time", $"minutes since {Strings.Format(SeasonClass.EventStart, "yyyy-MM-dd HH:mm:ss")}"))
                            BoundariesBC.BoundaryConditions.Item(NodeID.Trim.ToUpper).quantities.Add("WATERLEVELBND", New clsQuantity("waterlevelbnd", "m"))

                        End If

                    Next
                End If

                'and finally: write the adjusted boundaries.bc file
                Dim Path As String = RunDir & "\" & Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir & "\" & "boundaries.bc"
                BoundariesBC.Write(Path, SeasonClass.EventStart)
                File.Copy(Path, InputFilesDir & "\boundaries.bc", True)

            Else
                Throw New Exception("Error: models other than SOBEK or DIMR are not yet supported for adjusting boundary values")
            End If

            cn.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function



End Class
