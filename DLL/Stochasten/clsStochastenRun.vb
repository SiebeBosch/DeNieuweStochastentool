Imports System.Windows.Forms
Imports System.IO
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.Threading

Public Class clsStochastenRun

    '----------------------------------------------------------------------------------------
    'Author: Siebe Bosch
    'Date: 10-8-2014
    'This class contains the blueprint for one run in a stochastic analysis.
    '----------------------------------------------------------------------------------------

    Public ID As String
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
        If Not VolumeClass Is Nothing Then P *= VolumeClass.P
        If Not PatternClass Is Nothing Then P *= PatternClass.p
        If Not GWClass Is Nothing Then P *= GWClass.p
        If Not WLClass Is Nothing Then P *= WLClass.p
        If Not WindClass Is Nothing Then P *= WindClass.P
        If Not Extra1Class Is Nothing Then P *= Extra1Class.p
        If Not Extra2Class Is Nothing Then P *= Extra2Class.p
        If Not Extra3Class Is Nothing Then P *= Extra3Class.p
        If Not Extra4Class Is Nothing Then P *= Extra4Class.p
        Return P
    End Function



    Public Function Execute(Optional ByVal runIdx As Long = 0, Optional ByVal nRuns As Long = 1) As Boolean
        '------------------------------------------------------------------------
        'author: Siebe Bosch
        'date: 16-8-2014
        'this routine actually kicks off a model run!
        '------------------------------------------------------------------------
        Dim fromFile As String = String.Empty, fromFile2 As String = String.Empty
        Dim toFile As String, toFile2 As String
        Dim LockFile As String = Me.Setup.StochastenAnalyse.StochastsConfigFile & ".lock"

        Try

            'now copy the model to its temporary working location
            For Each myModel As clsSimulationModel In StochastenAnalyse.Models.Values

                '--------------------------------------------------------------------------------------------
                '    wait until the database is free to use. Then lock it for the current simulation
                '--------------------------------------------------------------------------------------------
                Me.Setup.GeneralFunctions.DatabaseWaitForUnlock(Me.Setup.StochastenAnalyse.StochastsConfigFile, 60)
                Me.Setup.GeneralFunctions.DatabaseWriteLockFile(Me.Setup.StochastenAnalyse.StochastsConfigFile, ID)

                If myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.DIMR Then

                    'create the paths to the meteo files, both absolute and relative. Meteofiles reside in the RR module
                    Setup.GeneralFunctions.UpdateProgressBar("Preparing meteo files. ", 0, 10, True)
                    Dim myMeteoDir As String = myModel.TempWorkDir & "\" & Me.Setup.DIMRData.DIMRConfig.RR.GetSubDir & "\"

                    'copy the original project to the temporary work dir and then read it from the new location
                    Setup.GeneralFunctions.UpdateProgressBar("Cloning model schematisation. ", 0, 10, True)
                    Dim myProject = New clsDIMR(Me.Setup, myModel.ModelDir)

                    myProject.CloneCaseForCommandLineRun(myModel.TempWorkDir, SeasonClass.EventStart, SeasonClass.EventStart.AddHours(StochastenAnalyse.Duration + StochastenAnalyse.DurationAdd))

                    'create the meteo files and copy them into the case directory
                    If Me.Setup.DIMRData.RR IsNot Nothing Then
                        Dim BuiName As String = Me.Setup.DIMRData.DIMRConfig.RR.GetBuiFileName
                        Dim EvpName As String = Me.Setup.DIMRData.DIMRConfig.RR.GetEvpFileName
                        BuiFile = myMeteoDir & BuiName
                        EvpFile = myMeteoDir & EvpName

                        Dim myBui As New clsBuiFile(Me.Setup)
                        Setup.GeneralFunctions.UpdateProgressBar("Retrieving rainfall pattern.", 0, 10, True)
                        Dim Verloop() As Double = StochastenAnalyse.getBuiVerloop(PatternClass.Patroon)
                        For Each Station As clsMeteoStation In StochastenAnalyse.MeteoStations.MeteoStations.Values
                            If Station.StationType = enmMeteoStationType.precipitation Then
                                Setup.GeneralFunctions.UpdateProgressBar("Building rainfall data.", 0, 10, True)
                                myBui.BuildSTOWATYPE(Station.Name, VolumeClass.Volume, Station.Factor, SeasonClass.EventStart, Verloop, StochastenAnalyse.DurationAdd)
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
                        If GWClass IsNot Nothing Then
                            If GWClass.RRFiles.Count > 0 Then
                                Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                                If Not CopyGroundwaterFiles(myModel, GWClass.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                            End If
                            If GWClass.RRFiles.Count > 0 Then
                                Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                                If Not CopyGroundwaterFiles(myModel, GWClass.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                            End If
                            If GWClass.RRFiles.Count > 0 Then
                                Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                                If Not CopyGroundwaterFiles(myModel, GWClass.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                            End If
                        End If
                        '--------------------------------------------------------------------------------------------------------------------

                    End If


                    If Me.Setup.DIMRData.FlowFM IsNot Nothing Then

                        '--------------------------------------------------------------------------------------------------------------------
                        'create the boundary file
                        If WLClass IsNot Nothing Then
                            Setup.GeneralFunctions.UpdateProgressBar("Copying file for boundaries.", 0, 10, True)
                            If Not BuildWaterLevelBoundaries(myModel) Then Throw New Exception("Fout bij het aanmaken van het randvoorwaardenbestand.")
                        End If
                        '--------------------------------------------------------------------------------------------------------------------

                        '--------------------------------------------------------------------------------------------------------------------
                        'copy the RR files for the extra1 stochast
                        If Extra1Class IsNot Nothing Then
                            Setup.GeneralFunctions.UpdateProgressBar("Copying Files for stochast extra1.", 0, 10, True)
                            If Extra1Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, 1, Extra1Class.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra1 stochast.")
                            If Extra1Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, 1, Extra1Class.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
                            If Extra1Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, 1, Extra1Class.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception("Fout bij het kopieren van de RTC-bestanden voor de extra1 stochast.")
                        End If
                        '--------------------------------------------------------------------------------------------------------------------

                        '--------------------------------------------------------------------------------------------------------------------
                        'copy the file for the extra2 stochast
                        If Extra2Class IsNot Nothing Then
                            Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra2.", 0, 10, True)
                            If Extra2Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, 2, Extra2Class.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra2 stochast.")
                            If Extra2Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, 2, Extra2Class.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
                            If Extra2Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, 2, Extra2Class.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception("Fout bij het kopieren van de RTC-bestanden voor de extra2 stochast.")
                        End If
                        '--------------------------------------------------------------------------------------------------------------------

                        '--------------------------------------------------------------------------------------------------------------------
                        'copy the file for the extra3 stochast
                        If Extra3Class IsNot Nothing Then
                            Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra3.", 0, 10, True)
                            If Extra3Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, 3, Extra3Class.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra3 stochast.")
                            If Extra3Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, 3, Extra3Class.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
                            If Extra3Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, 3, Extra3Class.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception("Fout bij het kopieren van de RTC-bestanden voor de extra3 stochast.")
                        End If
                        '--------------------------------------------------------------------------------------------------------------------

                        '--------------------------------------------------------------------------------------------------------------------
                        'copy the file for the extra4 stochast
                        If Extra4Class IsNot Nothing Then
                            Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra4.", 0, 10, True)
                            If Extra4Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, 4, Extra4Class.RRFiles, Me.Setup.DIMRData.DIMRConfig.RR.SubDir) Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra4 stochast.")
                            If Extra4Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, 4, Extra4Class.FlowFiles, Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir) Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
                            If Extra4Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, 4, Extra4Class.RTCFiles, Me.Setup.DIMRData.DIMRConfig.RTC.SubDir) Then Throw New Exception("Fout bij het kopieren van de RTC-bestanden voor de extra4 stochast.")
                        End If
                        '--------------------------------------------------------------------------------------------------------------------

                    End If



                    '----------------------------------------------------------------------------------------
                    'release the database for use by other instances
                    '----------------------------------------------------------------------------------------
                    Me.Setup.GeneralFunctions.DatabaseReleaseLock(Me.Setup.StochastenAnalyse.StochastsConfigFile)

                    'update the progress bar
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Running simulation " & runIdx & " of " & nRuns & ": " & ID, runIdx, nRuns, True)

                    Dim Exec As String = myModel.TempWorkDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(myModel.Exec)
                    If Not System.IO.File.Exists(Exec) Then Exec = myModel.Exec

                    Dim myProcess As New Process
                    myProcess.StartInfo.WorkingDirectory = myModel.TempWorkDir
                    myProcess.StartInfo.FileName = Exec
                    myProcess.StartInfo.Arguments = ""
                    myProcess.Start()

                    While Not myProcess.HasExited
                        'pom pom pom
                        Call Setup.GeneralFunctions.Wait(2000)
                    End While




                    ''v2.3.3 some models requre running a batchfile from the copied model dir. Other require a call to an exe file with a given path.
                    ''therefore we will try to find a local file copy first
                    'Dim Exec As String = myModel.TempWorkDir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(myModel.Exec)
                    'If Not System.IO.File.Exists(Exec) Then Exec = myModel.Exec


                    ''Dim info As New System.Diagnostics.ProcessStartInfo
                    ''info.FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\sample.exe"
                    ''info.Arguments = "/s /v/qn"
                    ''Dim process As New System.Diagnostics.Process
                    ''process.StartInfo = info
                    ''process.Start()
                    ''MessageBox.Show(info.Arguments.ToString())
                    ''process.Close()
                    'Dim myProcess As System.Diagnostics.Process
                    'Dim info As System.Diagnostics.ProcessStartInfo

                    'Directory.SetCurrentDirectory(Path.GetDirectoryName(myModel.TempWorkDir))
                    'info = New System.Diagnostics.ProcessStartInfo
                    'info.UseShellExecute = True
                    'info.WorkingDirectory = myModel.TempWorkDir
                    'info.FileName = Exec
                    ''info.Arguments = myModel.TempWorkDir & "\" & myModel.Args
                    'info.Arguments = ""

                    'myProcess = New System.Diagnostics.Process
                    'myProcess.StartInfo = info
                    'myProcess.Start()

                    While Not myProcess.HasExited
                        'waiting for the process to finish before starting the next (note: to be replaced with multithreading in the long term)
                        Call Setup.GeneralFunctions.Wait(2000)
                    End While

                    myProcess.Close()

                    'If logStr = "0" OrElse StochastenAnalyse.AllowCrashedResults Then
                    'v2.3.2: from dir was incorrect. Replaced myproject.projectdir by myModel.tempWorkdir
                    For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values
                        If myModel.ModelType = enmSimulationModel.SOBEK Then
                            fromFile = myModel.TempWorkDir & "\WORK\" & myFile.FileName
                        ElseIf myModel.ModelType = enmSimulationModel.DIMR Then
                            If myFile.HydroModule = enmHydroModule.RR Then
                                fromFile = myModel.TempWorkDir & "\" & myProject.DIMRConfig.RR.SubDir & "\" & myFile.FileName
                            ElseIf myFile.HydroModule = enmHydroModule.FLOW Then
                                fromFile = myModel.TempWorkDir & "\" & myProject.DIMRConfig.Flow1D.SubDir & "\output\" & myFile.FileName
                            End If
                        Else
                            fromFile = myModel.TempWorkDir & "\" & myFile.FileName
                        End If

                        toFile = OutputFilesDir & "\" & myFile.FileName

                        If File.Exists(fromFile) Then
                            Call FileCopy(fromFile, toFile)
                        Else
                            Me.Setup.Log.AddError("Fout: uitvoerbestand bestaat niet: " & fromFile)
                        End If
                    Next
                    'Else
                    '    Throw New Exception("Simulation for stochastic combination " & ID & " was unsuccessful.")
                    'End If



                ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.SOBEK Then

                    'create the paths to the meteo files, both absolute and relative
                    Setup.GeneralFunctions.UpdateProgressBar("Preparing meteo files. ", 0, 10, True)
                    Dim myMeteoDir As String = myModel.TempWorkDir & "\METEO\" & SeasonClass.Name.ToString & "_" & PatternClass.Patroon.ToString & "_" & VolumeClass.Volume & "mm\"
                    If Not Directory.Exists(myMeteoDir) Then Directory.CreateDirectory(myMeteoDir)
                    BuiFile = myMeteoDir & "meteo.bui"
                    EvpFile = myMeteoDir & "meteo.evp"
                    QscFile = myMeteoDir & "meteo.qsc"
                    WdcFile = myMeteoDir & "meteo.wdc"
                    QwcFile = myMeteoDir & "meteo.qwc"
                    TmpFile = myMeteoDir & "meteo.tmp"
                    RnfFile = myMeteoDir & "meteo.rnf"
                    Me.Setup.GeneralFunctions.AbsoluteToRelativePath(myModel.TempWorkDir & "\CMTWORK\", BuiFile, BuiFileRelative)
                    Me.Setup.GeneralFunctions.AbsoluteToRelativePath(myModel.TempWorkDir & "\CMTWORK\", EvpFile, EvpFileRelative)
                    Me.Setup.GeneralFunctions.AbsoluteToRelativePath(myModel.TempWorkDir & "\CMTWORK\", QscFile, QscFileRelative)
                    Me.Setup.GeneralFunctions.AbsoluteToRelativePath(myModel.TempWorkDir & "\CMTWORK\", WdcFile, WdcFileRelative)
                    Me.Setup.GeneralFunctions.AbsoluteToRelativePath(myModel.TempWorkDir & "\CMTWORK\", QwcFile, QwcFileRelative)
                    Me.Setup.GeneralFunctions.AbsoluteToRelativePath(myModel.TempWorkDir & "\CMTWORK\", TmpFile, TmpFileRelative)
                    Me.Setup.GeneralFunctions.AbsoluteToRelativePath(myModel.TempWorkDir & "\CMTWORK\", RnfFile, RnfFileRelative)

                    'copy the original project to the temporary work dir and then read it from the new location
                    Setup.GeneralFunctions.UpdateProgressBar("Cloning model schematisation. ", 0, 10, True)
                    Dim myProject = New clsSobekProject(Me.Setup, myModel.ModelDir, Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec), True)

                    'v2.040: changed mymodel.exec to mymodel.ModelDir. This fixes a bug for users who have their models on a different drive than their program
                    If Not myProject.CloneCaseForCommandLineRun(Directory.GetParent(myModel.ModelDir).FullName, myModel.CaseName.Trim.ToUpper, myModel.TempWorkDir, BuiFileRelative, EvpFileRelative, QscFileRelative, WdcFileRelative, QwcFileRelative, TmpFileRelative, RnfFileRelative) Then Throw New Exception("Error: could not clone SOBEK case for running from the command line.")
                    myProject = New clsSobekProject(Me.Setup, myModel.TempWorkDir, Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec), True)

                    '--------------------------------------------------------------------------------------------------------------------
                    'copy the groundwater file
                    If GWClass IsNot Nothing Then
                        If GWClass.RRFiles.Count > 0 Then
                            Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                            If Not CopyGroundwaterFiles(myModel, GWClass.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                        End If
                        If GWClass.FlowFiles.Count > 0 Then
                            Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                            If Not CopyGroundwaterFiles(myModel, GWClass.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                        End If
                        If GWClass.RTCFiles.Count > 0 Then
                            Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                            If Not CopyGroundwaterFiles(myModel, GWClass.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                        End If

                    End If
                    '--------------------------------------------------------------------------------------------------------------------

                    '--------------------------------------------------------------------------------------------------------------------
                    'create the boundary file
                    If Not WLClass Is Nothing Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for boundaries.", 0, 10, True)
                        If Not BuildWaterLevelBoundaries(myModel) Then Throw New Exception("Fout bij het aanmaken van het randvoorwaardenbestand.")
                    End If
                    '--------------------------------------------------------------------------------------------------------------------

                    '--------------------------------------------------------------------------------------------------------------------
                    'copy the file for the extra1 stochast
                    If Extra1Class IsNot Nothing Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra1.", 0, 10, True)
                        If Not Extra1Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, 1, Extra1Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra1 stochast.")
                        If Not Extra1Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, 1, Extra1Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
                        If Not Extra1Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, 1, Extra1Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra1 stochast.")
                    End If
                    '--------------------------------------------------------------------------------------------------------------------

                    '--------------------------------------------------------------------------------------------------------------------
                    'copy the file for the extra2 stochast
                    If Extra2Class IsNot Nothing Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra2.", 0, 10, True)
                        If Not Extra2Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, 2, Extra2Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra2 stochast.")
                        If Not Extra2Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, 2, Extra2Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
                        If Not Extra2Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, 2, Extra2Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra2 stochast.")
                    End If
                    '--------------------------------------------------------------------------------------------------------------------

                    '--------------------------------------------------------------------------------------------------------------------
                    'copy the file for the extra3 stochast
                    If Extra3Class IsNot Nothing Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra3.", 0, 10, True)
                        If Not Extra3Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, 3, Extra3Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra3 stochast.")
                        If Not Extra3Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, 3, Extra3Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
                        If Not Extra3Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, 3, Extra3Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra3 stochast.")
                    End If
                    '--------------------------------------------------------------------------------------------------------------------

                    '--------------------------------------------------------------------------------------------------------------------
                    'copy the file for the extra4 stochast
                    If Extra4Class IsNot Nothing Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra4.", 0, 10, True)
                        If Not Extra4Class.RRFiles <> "" Then If Not CopyExtraFiles(myModel, 4, Extra4Class.RRFiles, "") Then Throw New Exception("Fout bij het kopieren van de RR-bestanden voor de extra4 stochast.")
                        If Not Extra4Class.FlowFiles <> "" Then If Not CopyExtraFiles(myModel, 4, Extra4Class.FlowFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
                        If Not Extra4Class.RTCFiles <> "" Then If Not CopyExtraFiles(myModel, 4, Extra4Class.RTCFiles, "") Then Throw New Exception("Fout bij het kopieren van de Flow-bestanden voor de extra4 stochast.")
                    End If
                    '--------------------------------------------------------------------------------------------------------------------

                    'write the precipitation file and make a backup in the stochast directory
                    Dim myBui As New clsBuiFile(Me.Setup)
                    Setup.GeneralFunctions.UpdateProgressBar("Retrieving rainfall pattern.", 0, 10, True)
                    Dim Verloop() As Double = StochastenAnalyse.getBuiVerloop(PatternClass.Patroon)
                    For Each Station As clsMeteoStation In StochastenAnalyse.MeteoStations.MeteoStations.Values
                        If Station.StationType = enmMeteoStationType.precipitation Then
                            Setup.GeneralFunctions.UpdateProgressBar("Building rainfall data.", 0, 10, True)
                            myBui.BuildSTOWATYPE(Station.Name, VolumeClass.Volume, Station.Factor, SeasonClass.EventStart, Verloop, StochastenAnalyse.DurationAdd)
                        ElseIf Station.StationType = enmMeteoStationType.evaporation Then
                            Setup.GeneralFunctions.UpdateProgressBar("Building evaporation data.", 0, 10, True)
                            myBui.BuildLongTermEVAP(SeasonClass.Name, StochastenAnalyse.Duration, StochastenAnalyse.DurationAdd)
                        End If
                    Next
                    Setup.GeneralFunctions.UpdateProgressBar("Writing rainfall event.", 0, 10, True)
                    myBui.Write(BuiFile, 3)

                    'copy the meteo file to the unique directory for our desired run
                    If Not System.IO.Directory.Exists(InputFilesDir) Then System.IO.Directory.CreateDirectory(InputFilesDir)
                    File.Copy(BuiFile, Dir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(BuiFile), True)

                    'write the evaporation file for now we'll assume zero evaporation
                    Dim myEvp As New clsEvpFile(Me.Setup)
                    Dim Evap(Setup.GeneralFunctions.RoundUD(duur / 24, 0, True)) As Double
                    Setup.GeneralFunctions.UpdateProgressBar("Writing evaporation event.", 0, 10, True)
                    myEvp.BuildSTOWATYPE(Evap, SeasonClass.EventStart, StochastenAnalyse.DurationAdd)
                    myEvp.Write(EvpFile)
                    File.Copy(EvpFile, Dir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(EvpFile), True)

                    Setup.GeneralFunctions.UpdateProgressBar("Writing radiation file.", 0, 10, True)
                    Using myWriter As New StreamWriter(QscFile)
                        myWriter.WriteLine("CONSTANTS   'TEMP' 'RAD'")
                        myWriter.WriteLine("DATA        0 0")
                    End Using
                    File.Copy(QscFile, Dir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(QscFile), True)

                    Setup.GeneralFunctions.UpdateProgressBar("Writing wind file.", 0, 10, True)
                    Using myWriter As New StreamWriter(WdcFile)
                        myWriter.WriteLine("GLMT MTEO nm '(null)' ss 0 id '0' ci '-1' lc 9.9999e+009 wu 1")
                        myWriter.WriteLine("wv tv 0 0 9.9999e+009 wd td 0 0 9.9999e+009 su 0 sh ts")
                        myWriter.WriteLine("0 9.9999e+009 9.9999e+009 tu 0 tp tw 0 9.9999e+009 9.9999e+009 au 0 at ta 0")
                        myWriter.WriteLine("9.9999e+009 9.9999e+009 mteo glmt")
                    End Using
                    File.Copy(WdcFile, Dir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(WdcFile), True)

                    Using mywriter As New StreamWriter(QwcFile)
                        mywriter.WriteLine("CONSTANTS   'VWIND' 'WINDDIR'")
                        mywriter.WriteLine("DATA       0 0")
                    End Using
                    File.Copy(QwcFile, Dir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(QwcFile), True)

                    Using mywriter As New StreamWriter(TmpFile)
                        mywriter.WriteLine("")
                    End Using
                    File.Copy(TmpFile, Dir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(TmpFile), True)

                    Using mywriter As New StreamWriter(RnfFile)
                        mywriter.WriteLine("")
                    End Using
                    File.Copy(RnfFile, Dir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(RnfFile), True)

                    '----------------------------------------------------------------------------------------
                    'release the database for use by other instances
                    '----------------------------------------------------------------------------------------
                    Me.Setup.GeneralFunctions.DatabaseReleaseLock(Me.Setup.StochastenAnalyse.StochastsConfigFile)

                    'update the progress bar
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Running simulation " & runIdx & " of " & nRuns & ": " & ID, runIdx, nRuns, True)

                    Dim myProcess As New Process
                    myProcess.StartInfo.WorkingDirectory = myModel.TempWorkDir & "\CMTWORK"
                    myProcess.StartInfo.FileName = myModel.Exec
                    myProcess.StartInfo.Arguments = myModel.Args
                    myProcess.Start()

                    While Not myProcess.HasExited
                        'pom pom pom
                        Call Setup.GeneralFunctions.Wait(2000)
                    End While

                    'if the run was succesful, copy the files
                    Dim logReader As New StreamReader(myModel.TempWorkDir & "\CMTWORK\PLUVIUS1.RTN")
                    Dim logStr As String = logReader.ReadLine.Trim
                    logReader.Close()

                    'warning if results are used although simulation crashed
                    If logStr <> "0" AndAlso StochastenAnalyse.AllowCrashedResults Then Me.Setup.Log.AddWarning("Simulatie " & ID & " was niet succesvol, maar resultaat werd toch gebruikt in de nabewerking, conform uw instellingen.")

                    If logStr = "0" OrElse StochastenAnalyse.AllowCrashedResults Then
                        For Each myFile As clsResultsFile In myModel.ResultsFiles.Files.Values
                            fromFile = myModel.TempWorkDir & "\WORK\" & myFile.FileName
                            fromFile2 = myModel.TempWorkDir & "\WORK\" & Replace(myFile.FileName, ".his", ".hia", , , CompareMethod.Text)
                            toFile = Dir & "\" & myFile.FileName
                            toFile2 = Dir & "\" & Replace(myFile.FileName, ".his", ".hia", , , CompareMethod.Text)

                            If File.Exists(fromFile) Then
                                Call FileCopy(fromFile, toFile)
                            Else
                                Me.Setup.Log.AddError("Fout: uitvoerbestand bestaat niet: " & fromFile)
                            End If

                            If File.Exists(fromFile2) Then
                                Call FileCopy(fromFile2, toFile2)
                            Else
                                Me.Setup.Log.AddWarning("Uitvoerbestand bestaat niet: " & fromFile2 & ". resultaten voor ID's langer dan 20 karakters kunnen daarom niet correct worden uitgelezen.")
                            End If

                        Next
                    Else
                        Throw New Exception("Simulation for stochastic combination " & ID & " was unsuccessful.")
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function

    Public Function CopyGroundwaterFiles(ByRef myModel As clsSimulationModel, FileNames As List(Of String), ModelSubdir As String) As Boolean
        Dim fromFile As String, toFile As String, toStochastDir As String

        Try
            'v2.205: introducing multi-file support for groundwater stochast
            For Each FileName As String In FileNames
                fromFile = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(FileName, Setup.Settings.RootDir)

                'the target location depends on the type of model we're writing this stochast for
                If myModel.ModelType = enmSimulationModel.DIMR Then
                    toFile = myModel.TempWorkDir & "\" & ModelSubdir & "\" & Setup.GeneralFunctions.FileNameFromPath(FileName)
                ElseIf myModel.ModelType = enmSimulationModel.SOBEK Then
                    toFile = myModel.TempWorkDir & "\WORK\" & Setup.GeneralFunctions.FileNameFromPath(FileName)
                Else
                    Throw New Exception("Stochast initial groundwater not yet supported for requested model type: " & myModel.ModelType.ToString)
                End If

                toStochastDir = Dir & "\" & Setup.GeneralFunctions.FileNameFromPath(FileName)
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

    Public Function CopyExtraFiles(ByRef myModel As clsSimulationModel, ExtraNum As Integer, ExtraFiles As String, ModelSubdir As String) As Boolean
        Dim fromFile As String, toFile As String, toStochastDir As String
        Dim ExtraClass As clsStochasticExtraClass = Nothing

        Try

            Select Case ExtraNum
                Case Is = 1
                    ExtraClass = Extra1Class
                Case Is = 2
                    ExtraClass = Extra2Class
                Case Is = 3
                    ExtraClass = Extra3Class
                Case Is = 4
                    ExtraClass = Extra4Class
            End Select

            While Not ExtraFiles = ""
                fromFile = Setup.GeneralFunctions.ParseString(ExtraFiles, ";")
                fromFile = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(fromFile, Me.Setup.Settings.RootDir)

                If myModel.ModelType = enmSimulationModel.SOBEK Then
                    toFile = myModel.TempWorkDir & "\WORK\" & Setup.GeneralFunctions.FileNameFromPath(fromFile)
                ElseIf myModel.ModelType = enmSimulationModel.DIMR Then
                    toFile = myModel.TempWorkDir & "\" & ModelSubdir & "\" & Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir & "\" & Setup.GeneralFunctions.FileNameFromPath(fromFile)
                Else
                    Throw New Exception("Kan invoerbestand " & fromFile & " niet naar het doelmodel kopieren omdat het modeltype niet wordt ondersteund voor de onderhavige stochast: " & myModel.ModelType.ToString)
                End If

                toStochastDir = Dir & "\" & Setup.GeneralFunctions.FileNameFromPath(fromFile)
                If File.Exists(fromFile) Then
                    FileCopy(fromFile, toFile)
                    FileCopy(fromFile, toStochastDir)
                Else
                    Throw New Exception("Fout: bestand voor extra stochast niet gevonden niet: " & fromFile)
                End If

            End While
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function BuildWaterLevelBoundaries(ByRef myModel As clsSimulationModel) As Boolean

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
                            cn.ConnectionString = "Data Source=" & Me.Setup.StochastenAnalyse.StochastsConfigFile & ";Version=3;"
                            'cn.ConnectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=" & Me.Setup.StochastenAnalyse.StochastsConfigFile & ";"
                            'cn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" & Me.Setup.StochastenAnalyse.StochastsConfigFile & ";"
                            cn.Open()
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
                Using myWriter As New StreamWriter(myModel.TempWorkDir & "\WORK\boundary.dat")
                    myBoundaryDat.Write(myWriter)
                End Using
                File.Copy(myModel.TempWorkDir & "\WORK\boundary.dat", Dir & "\boundary.dat", True)

            ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.DIMR Then

                'first we must read the contents of boundaries.bc into memory
                Dim BoundariesBC As New STOCHLIB.clsBoundariesBC(Me.Setup, myModel.TempWorkDir & "\" & Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir & "\boundaries.bc")
                BoundariesBC.Read()

                'now we must replace the timeseries as specified in this file's content
                'get the node id's
                If SeasonClass.WaterLevelsUse Then
                    For Each NodeID As String In myModel.Boundaries
                        'see if this boundary has a record inside BoundariesBC
                        If BoundariesBC.BoundaryConditions.ContainsKey(NodeID.Trim.ToUpper) Then
                            'we found the matching record! Now replace it with the requested series from our database
                            cn.ConnectionString = "Data Source=" & Me.Setup.StochastenAnalyse.StochastsConfigFile & ";Version=3;"
                            cn.Open()


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

                        End If

                    Next
                End If

                'and finally: write the adjusted boundaries.bc file
                Dim Path As String = myModel.TempWorkDir & "\" & Me.Setup.DIMRData.DIMRConfig.Flow1D.SubDir & "\" & "boundaries.bc"
                BoundariesBC.Write(Path, SeasonClass.EventStart)
                File.Copy(Path, Dir & "\boundaries.bc", True)

            Else
                Throw New Exception("Error: models other than SOBEK or DIMR are not yet supported for adjusting boundary values")
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function



End Class
