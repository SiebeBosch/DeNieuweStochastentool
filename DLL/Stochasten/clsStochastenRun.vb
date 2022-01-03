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
    Public Dir As String            'directory for all input files and results files
    Public RelativeDir As String    'Dame directory, but relative to the Stochastenanalyse root dir
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
        If Not VolumeClass Is Nothing Then Pars(3) = VolumeClass.Volume 'neerslagvolume
        If Not PatternClass Is Nothing Then Pars(4) = PatternClass.Patroon.ToString 'neerslagpatroon
        If Not GWClass Is Nothing Then Pars(5) = GWClass.ID 'initiele grondwaterdiepte
        If Not WLClass Is Nothing Then Pars(6) = WLClass.ID 'waterhoogte
        If Not WindClass Is Nothing Then Pars(7) = WindClass.ID 'wind
        If Not Extra1Class Is Nothing Then Pars(8) = Extra1Class.ID 'bijv. ruwheid, gemaalfalen, sliblaag
        If Not Extra2Class Is Nothing Then Pars(9) = Extra2Class.ID 'bijv. ruwheid, gemaalfalen, sliblaag
        If Not Extra3Class Is Nothing Then Pars(10) = Extra3Class.ID 'bijv. ruwheid, gemaalfalen, sliblaag
        If Not Extra4Class Is Nothing Then Pars(10) = Extra4Class.ID 'bijv. ruwheid, gemaalfalen, sliblaag
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
        Dim fromFile As String, fromFile2 As String
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
                    If Not System.IO.Directory.Exists(Dir) Then System.IO.Directory.CreateDirectory(Dir)
                    File.Copy(BuiFile, Dir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(BuiFile), True)

                    'write the evaporation file for now we'll assume zero evaporation
                    Dim myEvp As New clsEvpFile(Me.Setup)
                    Dim Evap(Setup.GeneralFunctions.RoundUD(duur / 24, 0, True)) As Double
                    Setup.GeneralFunctions.UpdateProgressBar("Writing evaporation event.", 0, 10, True)
                    myEvp.BuildSTOWATYPE(Evap, SeasonClass.EventStart, StochastenAnalyse.DurationAdd)
                    myEvp.Write(EvpFile)
                    File.Copy(EvpFile, Dir & "\" & Me.Setup.GeneralFunctions.FileNameFromPath(EvpFile), True)

                    '----------------------------------------------------------------------------------------
                    'release the database for use by other instances
                    '----------------------------------------------------------------------------------------
                    Me.Setup.GeneralFunctions.DatabaseReleaseLock(Me.Setup.StochastenAnalyse.StochastsConfigFile)

                    'update the progress bar
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Running simulation " & runIdx & " of " & nRuns & ": " & ID, runIdx, nRuns, True)

                    Dim myProcess As New Process
                    myProcess.StartInfo.WorkingDirectory = myModel.TempWorkDir
                    myProcess.StartInfo.FileName = myModel.Exec
                    myProcess.StartInfo.Arguments = myModel.Args
                    myProcess.Start()

                    While Not myProcess.HasExited
                        'pom pom pom
                        Call Setup.GeneralFunctions.Wait(2000)
                    End While




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

                    'copy the groundwater file
                    If Not GWClass Is Nothing AndAlso GWClass.FileName <> "" Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying the groundwater file.", 0, 10, True)
                        If Not CopyGroundwaterFile(myModel) Then Throw New Exception("Fout bij het kopieren van het grondwaterbestand.")
                    End If

                    'create the boundary file
                    If Not WLClass Is Nothing Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for boundaries.", 0, 10, True)
                        If Not BuildWaterLevelBoundaries(myModel) Then Throw New Exception("Fout bij het aanmaken van het randvoorwaardenbestand.")
                    End If

                    'copy the file for the extra1 stochast
                    If Not Extra1Class Is Nothing AndAlso Extra1Class.FileName <> "" Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra1.", 0, 10, True)
                        If Not CopyExtraFiles(myModel, 1) Then Throw New Exception("Fout bij het kopieren van het bestand voor de extra stochast.")
                    End If

                    'copy the file for the extra2 stochast
                    If Not Extra2Class Is Nothing AndAlso Extra1Class.FileName <> "" Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra2.", 0, 10, True)
                        If Not CopyExtraFiles(myModel, 2) Then Throw New Exception("Fout bij het kopieren van het bestand voor de extra stochast.")
                    End If

                    'copy the file for the extra3 stochast
                    If Not Extra3Class Is Nothing AndAlso Extra1Class.FileName <> "" Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra3.", 0, 10, True)
                        If Not CopyExtraFiles(myModel, 3) Then Throw New Exception("Fout bij het kopieren van het bestand voor de extra stochast.")
                    End If

                    'copy the file for the extra4 stochast
                    If Not Extra4Class Is Nothing AndAlso Extra1Class.FileName <> "" Then
                        Setup.GeneralFunctions.UpdateProgressBar("Copying file for stochast extra4.", 0, 10, True)
                        If Not CopyExtraFiles(myModel, 4) Then Throw New Exception("Fout bij het kopieren van het bestand voor de extra stochast.")
                    End If

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
                    If Not System.IO.Directory.Exists(Dir) Then System.IO.Directory.CreateDirectory(Dir)
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

    Public Function CopyGroundwaterFile(ByRef myModel As clsSimulationModel) As Boolean
        Dim fromFile As String, toFile As String, toStochastDir As String

        Try
            fromFile = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(GWClass.FileName, Setup.Settings.RootDir)
            toFile = myModel.TempWorkDir & "\WORK\" & Setup.GeneralFunctions.FileNameFromPath(GWClass.FileName)
            toStochastDir = Dir & "\" & Setup.GeneralFunctions.FileNameFromPath(GWClass.FileName)
            If File.Exists(fromFile) Then
                FileCopy(fromFile, toFile)
                FileCopy(fromFile, toStochastDir)
            Else
                Throw New Exception("Fout: grondwaterbestand niet gevonden niet: " & fromFile)
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function CopyExtraFiles(ByRef myModel As clsSimulationModel, ExtraNum As Integer) As Boolean
        Dim fromFile As String, toFile As String, toStochastDir As String
        Dim ExtraClass As clsStochasticExtraClass = Nothing

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

        Try
            Dim fromFiles As String = ExtraClass.FileName
            While Not fromFiles = ""
                fromFile = Setup.GeneralFunctions.ParseString(fromFiles, ";")
                fromFile = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(fromFile, Me.Setup.Settings.RootDir)
                toFile = myModel.TempWorkDir & "\WORK\" & Setup.GeneralFunctions.FileNameFromPath(fromFile)
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


    Public Function CopyExtraAFile(ByRef myModel As clsSimulationModel) As Boolean
        Dim fromFile As String, toFile As String, toStochastDir As String

        Try
            fromFile = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(Extra4Class.FileName, Me.Setup.Settings.RootDir)
            toFile = myModel.TempWorkDir & "\WORK\" & Setup.GeneralFunctions.FileNameFromPath(Extra4Class.FileName)
            toStochastDir = Dir & "\" & Setup.GeneralFunctions.FileNameFromPath(Extra4Class.FileName)
            If File.Exists(fromFile) Then
                FileCopy(fromFile, toFile)
                FileCopy(fromFile, toStochastDir)
            Else
                Throw New Exception("Fout: bestand voor extra stochast niet gevonden niet: " & fromFile)
            End If
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

                    If Not myRecord Is Nothing Then

                        'query the database in order to retrieve the time series
                        Dim cn As New SQLite.SQLiteConnection
                        Dim da As SQLite.SQLiteDataAdapter
                        Dim dt As New DataTable
                        Dim query As String

                        'query the database to retrieve the water level boundary timeseries
                        If SeasonClass.WaterLevelsUse Then
                            cn.ConnectionString = "Data Source=" & Me.Setup.StochastenAnalyse.StochastsConfigFile & ";Version=3;"
                            'cn.ConnectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=" & Me.Setup.StochastenAnalyse.StochastsConfigFile & ";"
                            'cn.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" & Me.Setup.StochastenAnalyse.StochastsConfigFile & ";"
                            cn.Open()
                            query = "SELECT MINUUT, " & myNode & " from RANDREEKSEN where NAAM='" & WLClass.ID & "' AND DUUR=" & Me.Setup.StochastenAnalyse.Duration & " ORDER BY MINUUT;"
                            da = New SQLite.SQLiteDataAdapter(query, cn)
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

            Else
                Throw New Exception("Error: models other than SOBEK are not yet supported for adjusting boundary values")
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function



End Class
