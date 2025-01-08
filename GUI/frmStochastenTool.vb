Imports STOCHLIB
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO
Imports System.Xml
Imports MapWinGIS
Imports Ionic.Zip
Imports Microsoft.VisualBasic.FileIO
Imports System.Data.SqlTypes

'for MATLAB files
Imports csmatio.io
Imports csmatio.types
Imports System.Security.Cryptography.X509Certificates
Imports MathNet.Numerics.RootFinding
Imports Apache.Arrow
Imports System.Data.Common
Imports System.Data.SQLite
Imports System.Collections.ObjectModel
Imports System.Transactions


'========================================================================================================================
'   GUI GENERAL
'========================================================================================================================
Public Class frmStochasten

    'Dim me.Setup.con As OleDb.OleDbConnection
    Dim queryGWWin As String
    Dim daGWWin As OleDb.OleDbDataAdapter
    Dim dtGWWin As DataTable
    Dim queryGWZom As String
    Dim daGWZom As OleDb.OleDbDataAdapter
    Dim dtGWZom As DataTable
    Dim dtLocations As DataTable
    Dim dtModels As DataTable
    Dim PastedFromClipBoard As Boolean = False
    Private Setup As clsSetup

    Dim SelectedRows As New List(Of Integer)

    'variables for the markers
    Dim minScale As Double = 0, maxScale As Double = 0

    Dim ActiveRunRef As String 'the currently selected reference run, according to the sliders
    Dim ActiveRunVGL As String 'the currently selected comparison run, according to the sliders

    Dim ActiveExtraGrid As DataGridView 'the currently active datagrid in the Extra category

    'temporary datatables to store the available values for stochasts
    Dim dtSeizoen As DataTable, dtVolume As DataTable, dtPattern As DataTable, dtGW As DataTable
    Dim dtBoundary As DataTable, dtWind As DataTable
    Dim dtExtra1 As DataTable, dtExtra2 As DataTable, dtExtra3 As DataTable, dtExtra4 As DataTable

    Dim dataGrids As List(Of DataGridView) 'all the datagrids on the form
    Dim dtRes As DataTable

    'for charting purposes
    Private tooltip As New ToolTip()
    Private clickPosition As System.Nullable(Of System.Drawing.Point) = Nothing
    Private x_left As Double, y_bottom As Double, x_right As Double, y_top As Double
    Private frmPattern As frmPatroon

    'for me.Setup.controledoeleinden
    Private controleResultaten As STOCHLIB.clsResultsByVolume



    ' Declare the tooltip

    Private ttPopulateRuns As New ToolTip()
    Private ttDeleteResults As New ToolTip()
    Private ttBuild As New ToolTip()
    Private ttSimulate As New ToolTip()
    Private ttCopyResults As New ToolTip()
    Private ttUitlezen As New ToolTip()
    Private ttPostprocessing As New ToolTip()
    Private ttViewer As New ToolTip()
    Private ttExport As New ToolTip()


    Private Sub frmStochastenTool_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Set up the delays for the tooltip.
        ttPopulateRuns.AutoPopDelay = 5000
        ttDeleteResults.AutoPopDelay = 5000
        ttBuild.AutoPopDelay = 5000
        ttSimulate.AutoPopDelay = 5000
        ttCopyResults.AutoPopDelay = 5000
        ttUitlezen.AutoPopDelay = 5000
        ttPostprocessing.AutoPopDelay = 5000
        ttViewer.AutoPopDelay = 5000
        ttExport.AutoPopDelay = 5000

        ttPopulateRuns.InitialDelay = 1000
        ttDeleteResults.InitialDelay = 1000
        ttBuild.InitialDelay = 1000
        ttSimulate.InitialDelay = 1000
        ttCopyResults.InitialDelay = 1000
        ttUitlezen.InitialDelay = 1000
        ttPostprocessing.InitialDelay = 1000
        ttViewer.InitialDelay = 1000
        ttExport.InitialDelay = 1000

        ttPopulateRuns.ReshowDelay = 500
        ttDeleteResults.ReshowDelay = 500
        ttBuild.ReshowDelay = 500
        ttSimulate.ReshowDelay = 500
        ttCopyResults.ReshowDelay = 500
        ttUitlezen.ReshowDelay = 500
        ttPostprocessing.ReshowDelay = 500
        ttViewer.ReshowDelay = 500
        ttExport.ReshowDelay = 500

        ' Force the tooltip text to be displayed whether or not the form is active.
        ttPopulateRuns.ShowAlways = True
        ttDeleteResults.ShowAlways = True
        ttBuild.ShowAlways = True
        ttSimulate.ShowAlways = True
        ttCopyResults.ShowAlways = True
        ttUitlezen.ShowAlways = True
        ttPostprocessing.ShowAlways = True
        ttViewer.ShowAlways = True
        ttExport.ShowAlways = True

        ' Set up the tooltip text for the Button1.
        ttPopulateRuns.SetToolTip(Me.btnPopulateRuns, "Vult de tabel met simulaties voor alle combinaties van de opgegeven stochasten.")
        ttDeleteResults.SetToolTip(Me.btnWissen, "Verwijdert de resultaatbestanden van de geselecteerde simulaties uit de stochastenmap.")
        ttBuild.SetToolTip(Me.btnBuild, "Schrijft een instantie van elk van de geselecteerde simulaties naar de tijdelijke werkmap.")
        ttSimulate.SetToolTip(Me.btnSimulate, "Start de simulatie van de geselecteerde simulaties. Le top: gebruik om de simulaties elders en in multithreading te draaien het aparte programma BAT_RUNR.")
        ttCopyResults.SetToolTip(Me.btnCopyResults, "Kopieert de opgegeven resultaatbestanden uit de tijdelijke simulatiemap naar de stochastenmap.")
        ttUitlezen.SetToolTip(Me.btnUitlezen, "Leest de resultaten van de geselecteerde simulaties uit en schrijft ze naar de database.")
        ttPostprocessing.SetToolTip(Me.btnPostprocessing, "Bewerkt de resultaten van de geselecteerde simulaties na tot overschrijdingsgrafieken en schrijft die naar de database.")
        ttViewer.SetToolTip(Me.btnViewer, "Schrijft de herhalingstijden naar de webviewer en opent die.")
        ttExport.SetToolTip(Me.btnExport, "Exporteert de resultaten van de stochastenanalyse naar een Excel-bestand en CSV-bestanden.")

    End Sub



    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        dtLocations = New DataTable
        dtModels = New DataTable

        dataGrids = New List(Of DataGridView)

        ' Add any initialization after the InitializeComponent() call.
        Setup = New clsSetup
        Setup.InitializeStochasten()
        Setup.SetProgress(prProgress, lblProgress)

    End Sub

    Private Sub CmbDuration_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbDuration.SelectedIndexChanged
        RebuildAllGrids()
    End Sub


    Public Function EnableButtons(ForceAllTrue As Boolean) As Boolean
        Try
            'this function enables/disables buttons, based on whether results are available

            'first check if all results files are present. We can do this by checking the DONE column in the grid
            Dim AllResultsFilesPresent As Boolean = True
            For Each myRow As DataGridViewRow In grRuns.Rows
                If Not myRow.Cells("DONE").Value Then
                    AllResultsFilesPresent = False
                    Exit For
                End If
            Next

            If ForceAllTrue Then
                btnUitlezen.Enabled = True
                btnPostprocessing.Enabled = True
                btnViewer.Enabled = True
                btnExport.Enabled = True
            Else
                'if all results files are present, enable the buttons for uitlezen, postprocessing, export and viewer
                btnUitlezen.Enabled = AllResultsFilesPresent
                btnPostprocessing.Enabled = AllResultsFilesPresent
                btnViewer.Enabled = AllResultsFilesPresent
                btnExport.Enabled = AllResultsFilesPresent
            End If


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error enabling buttons: " & ex.Message)
            Return False
        End Try

    End Function

    Private Sub btnBuild_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBuild.Click

        'clear the log
        Me.Setup.Log.Clear()

        'make sure to accept crashed results if specified
        Me.Setup.StochastenAnalyse.AllowCrashedResults = chkUseCrashedResults.Checked

        'tijdens de berekeningen willen we ook op andere machines kunnen draaien, dus verbreek de verbinding met de database
        If Me.Setup.SqliteCon IsNot Nothing Then Me.Setup.SqliteCon.Close()

        'lees eerst de basiscase in van de modelschematisatie(s)
        For Each myModel As STOCHLIB.clsSimulationModel In Me.Setup.StochastenAnalyse.Models.Values
            If myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.SOBEK Then
                Call Setup.SetAddSobekProject(myModel.ModelDir, Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec))
                If Not Setup.SOBEKData.ActiveProject.Cases.ContainsKey(myModel.CaseName.Trim.ToUpper) Then Throw New Exception("Error: could not find case " & myModel.CaseName & " in SOBEK project.")
                Setup.SetActiveCase(myModel.CaseName)
                Setup.InitSobekModel(True, True)
                Setup.ReadSobekDataDetail(False, False, False, False, False, True, False, False, False, False, False)
            ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.DIMR OrElse myModel.ModelType = enmSimulationModel.DHYDRO OrElse myModel.ModelType = enmSimulationModel.DHYDROSERVER Then
                'do nothing since D-Hydro does not yet have a case manager
                Call Setup.SetDIMRProject(myModel.ModelDir)
                Call Setup.DIMRData.DIMRConfig.Read()
            ElseIf myModel.ModelType = enmSimulationModel.HBV Then
                'do nothing since HBV does not yet have a case manager
                Call Setup.SetHBVProject(myModel.ModelDir)
                Call Setup.HBVData.BasinFile.Read()
            ElseIf myModel.ModelType = enmSimulationModel.SUMAQUA Then
                Call Setup.setSumaquaProject(myModel.ModelDir, myModel.CaseName)
            End If
        Next

        'de meteo stations
        Me.Setup.StochastenAnalyse.MeteoStations.Initialize()
        For Each myRow As DataGridViewRow In grMeteoStations.Rows
            Dim gebiedsreductie As STOCHLIB.GeneralFunctions.enmGebiedsreductie = [Enum].Parse(GetType(STOCHLIB.GeneralFunctions.enmGebiedsreductie), myRow.Cells(2).Value.ToString())
            Me.Setup.StochastenAnalyse.MeteoStations.Add(myRow.Cells(0).Value, myRow.Cells(1).Value, gebiedsreductie, myRow.Cells(3).Value, myRow.Cells(4).Value)
        Next

        'een d-hydromodel op de server of via de casemanager kan niet in combinatie met andere modellen draaien omdat het zelfstandig alle runs aanstuurt
        If Setup.StochastenAnalyse.Models.Count = 1 AndAlso Setup.StochastenAnalyse.Models.Values(0).ModelType = enmSimulationModel.DHYDRO Then
            'just one D-Hydro model & run on a shared folder

            'build the populate_cases.json
            Dim myJSON As String = Build_Populate_Cases_JSON(Setup.StochastenAnalyse.Models.Values(0))
            Using jsonWriter As New StreamWriter(Me.Setup.StochastenAnalyse.Models.Values(0).TempWorkDir & "\populate_cases.json")
                jsonWriter.Write(myJSON)
            End Using

            'also write the input files to the same folder

        ElseIf Setup.StochastenAnalyse.Models.Count > 1 AndAlso Setup.StochastenAnalyse.Models.Values(0).ModelType = enmSimulationModel.DHYDRO Then
            Throw New Exception("Kritieke fout: het aansturen van DHYDRO via de case manager kan alleen als slechts één model betrokken is bij de analyse. Maak voor meer modellen gebruik van de DIMR om D-Hydro te draaien.")
        Else
            'de runs
            If grRuns.SelectedRows.Count > 0 Then
                If Not Me.Setup.StochastenAnalyse.Runs.BuildSelected(grRuns, btnPostprocessing) Then
                    MsgBox("Fouten bij het draaien van de geselecteerde runs. me.Setup.controleer de logfile voor meldingen.")
                    Me.Setup.Log.write(Setup.StochastenAnalyse.ResultsDir & "\logfile.txt", True)
                End If
            Else
                MsgBox("Selecteer de rijen van de simulaties die u wilt draaien")
            End If
        End If

        'afsluiten & logfile schrijven
        Dim logfile As String = Replace(Me.Setup.StochastenAnalyse.XMLFile, ".xml", ".log", , , Microsoft.VisualBasic.CompareMethod.Text)
        Me.Setup.GeneralFunctions.UpdateProgressBar("klaar.", 0, 10, True)
        Me.Setup.Log.write(Me.Setup.Settings.RootDir & "\" & logfile, True)

    End Sub

    Public Function Build_Populate_Cases_JSON(ByRef myModel As STOCHLIB.clsSimulationModel) As String
        Try
            Dim dt As DataTable
            Dim query As String
            Dim i As Integer

            'this function creates a populate_cases.json file for D-Hydro simulations on the server (DHYDROSERVER)
            Dim myJSON As String = "{" & vbCrLf

            'write the boundary conditions (stochasts):
            myJSON &= vbTab & "%boundary_conditions%: [" & vbCrLf

            'meteo forcing is built up of: duration_volume_pattern_season
            'we loop through all simulations and create all necessary combinations of these parameters
            Dim MeteoForcings As Dictionary(Of String, STOCHLIB.clsMeteoForcing)
            i = 0
            MeteoForcings = Me.Setup.StochastenAnalyse.Runs.GetSelectedMeteoForcings(grRuns)
            myJSON &= vbTab & vbTab & "{" & vbCrLf
            myJSON &= vbTab & vbTab & "%meteo%: [" & vbCrLf
            For Each myForcing As STOCHLIB.clsMeteoForcing In MeteoForcings.Values
                i += 1
                myJSON &= vbTab & vbTab & vbTab & "{" & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & "%id%:%" & myForcing.GetID & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & "%name%:%" & myForcing.GetID & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & "%stowa_bui%: {" & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & "%duration%:" & myForcing.GetDuration & "," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & "%pattern%:%" & myForcing.GetPattern.ToString.ToLower & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & "%volume%:" & myForcing.GetVolume & "," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & "%season%:%" & myForcing.GetSeason.ToLower & "%" & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & "}" & vbCrLf
                If i = MeteoForcings.Values.Count Then
                    myJSON &= vbTab & vbTab & vbTab & "}" & vbCrLf
                Else
                    myJSON &= vbTab & vbTab & vbTab & "}," & vbCrLf
                End If
            Next
            myJSON &= vbTab & vbTab & "]}," & vbCrLf

            'flow forcing is built up of timeseries
            myJSON &= vbTab & vbTab & "{" & vbCrLf
            myJSON &= vbTab & vbTab & "%flow%: [" & vbCrLf
            Dim FlowForcings As Dictionary(Of String, STOCHLIB.clsFlowForcing)
            FlowForcings = Me.Setup.StochastenAnalyse.Runs.GetSelectedFlowForcings(grRuns)
            Dim j As Integer
            j = 0
            For Each myForcing As STOCHLIB.clsFlowForcing In FlowForcings.Values
                j += 1
                myJSON &= vbTab & vbTab & vbTab & "{" & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & "%id%:%" & myForcing.GetID & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & "%name%:%" & myForcing.GetID & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & vbTab & "%model_objects%: [" & vbCrLf

                'walk through all boundary objects and retrieve the timeseries for them from the database
                Dim TableStart As Date
                dt = New DataTable

                i = 0
                For Each myNodeID As String In myModel.Boundaries
                    i += 1
                    myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & "{" & vbCrLf
                    myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & "%objectid%: %" & myNodeID & "%," & vbCrLf
                    myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & "%type%: %" & "waterlevelbnd" & "%," & vbCrLf
                    myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & "%time_series%: [" & vbCrLf

                    'retrieve the timeseries from our database and write it to the JSON
                    dt = New DataTable
                    query = "SELECT MINUUT, WAARDE from RANDREEKSEN where KLIMAATSCENARIO='" & Me.Setup.StochastenAnalyse.KlimaatScenario.ToString & "' AND NAAM='" & myForcing.GetClassID & "' AND NODEID='" & myNodeID & "' AND DUUR=" & Me.Setup.StochastenAnalyse.Duration & " ORDER BY MINUUT;"
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt)
                    If dt.Rows.Count > 0 Then
                        'first subtract the difference between the start of the table and the start of the simulation
                        'also make sure the waterlevel boundary is set to be time dependent!
                        Dim SeasonClass As STOCHLIB.clsStochasticSeasonClass = Me.Setup.StochastenAnalyse.Seasons.Item(myForcing.GetSeason.Trim)
                        TableStart = SeasonClass.GetEventStart
                        For r = 0 To dt.Rows.Count - 1
                            myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & vbTab & "{%date%: %" & Format(TableStart.AddMinutes(dt.Rows(r)(0)), "yyyy-MM-dd") & "%, %time%: %" & Format(TableStart.AddMinutes(dt.Rows(r)(0)), "HH:mm:ss") & "%, %value%: " & dt.Rows(r)(1) & "}"
                            If r < dt.Rows.Count - 1 Then myJSON &= ","
                            myJSON &= vbCrLf
                        Next
                    End If
                    myJSON &= vbTab & vbTab & vbTab & vbTab & vbTab & "]}"
                    If i < myModel.Boundaries.Count Then myJSON &= ","
                    myJSON &= vbCrLf
                    myJSON &= vbTab & vbTab & vbTab & vbTab & "]" & vbCrLf
                Next
                myJSON &= vbTab & vbTab & vbTab & "}"
                If j < FlowForcings.Values.Count Then myJSON &= ","
                myJSON &= vbCrLf
            Next
            myJSON &= vbTab & vbTab & "]}" & vbCrLf
            myJSON &= vbTab & "]," & vbCrLf

            'write the model
            myJSON &= vbTab & "%models%: [" & vbCrLf
            myJSON &= vbTab & vbTab & "{" & vbCrLf
            myJSON &= vbTab & vbTab & vbTab & "%id%: %D-Hydro-model%," & vbCrLf
            myJSON &= vbTab & vbTab & vbTab & "%name%: %D-Hydro-model%," & vbCrLf
            myJSON &= vbTab & vbTab & vbTab & "%path%: %" & Strings.Replace(Me.Setup.StochastenAnalyse.Models.Values(0).ModelDir, "\", "\\") & "%," & vbCrLf
            myJSON &= vbTab & vbTab & vbTab & "%mdu_file%: %" & Me.Setup.DIMRData.DIMRConfig.Flow1D.GetInputFile & "%," & vbCrLf
            myJSON &= vbTab & vbTab & vbTab & "%fnm_file%: %" & Me.Setup.DIMRData.DIMRConfig.RR.GetInputFile & "%," & vbCrLf
            myJSON &= vbTab & vbTab & vbTab & "%rtc_file%: %" & Me.Setup.DIMRData.DIMRConfig.RTC.GetInputFile & "%," & vbCrLf
            myJSON &= vbTab & vbTab & vbTab & "%results%: [" & vbCrLf

            'now retrieve the requested output from our database
            query = "SELECT LOCATIEID, LOCATIENAAM, MODELPAR, RESULTSFILE, RESULTSTYPE FROM OUTPUTLOCATIONS WHERE MODELID='" & Me.Setup.StochastenAnalyse.Models.Values(0).Id & "';"
            dt = New DataTable
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt, True)
            For i = 0 To dt.Rows.Count - 1
                myJSON &= vbTab & vbTab & vbTab & vbTab & "{%id%: %" & dt.Rows(i)(0) & "%, %name%:%" & dt.Rows(i)(1) & "%, %parameter%:%" & dt.Rows(i)(2) & "%, %filename%:%" & dt.Rows(i)(3) & "%,%filter%:%" & dt.Rows(i)(4).ToString.Trim & "%}"
                If i < dt.Rows.Count - 1 Then myJSON &= ","
                myJSON &= vbCrLf
            Next

            myJSON &= vbTab & vbTab & vbTab & "]" & vbCrLf
            myJSON &= vbTab & vbTab & "}" & vbCrLf
            myJSON &= vbTab & "]," & vbCrLf

            myJSON &= vbTab & "%cases%: [" & vbCrLf

            'walk through each selected case and create a case for it
            Dim Selection As New Dictionary(Of String, STOCHLIB.clsStochastenRun)
            Selection = Me.Setup.StochastenAnalyse.Runs.GetSelected(grRuns)
            i = 0
            For Each myRun As STOCHLIB.clsStochastenRun In Selection.Values
                i += 1
                myJSON &= vbTab & vbTab & "{" & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & "%id%:%" & myRun.ID & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & "%name%:%" & myRun.ID & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & "%model%:%" & "D-Hydro-model" & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & "%meteo_bc_id%:%" & myRun.getMeteoForcing.GetID & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & "%flow_bc_id%:%" & myRun.GetFlowForcing.GetID & "%," & vbCrLf
                myJSON &= vbTab & vbTab & vbTab & "%files%:[" & vbCrLf

                'write references to all input files for the case here
                Dim FirstFileWritten As Boolean = False
                If myRun.GWClass IsNot Nothing Then

                    If myRun.GWClass.RRFiles.Count > 0 Then
                        If FirstFileWritten Then myJSON &= "," & vbCrLf
                        myJSON &= vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.GWClass.RRFiles(0), "\", "\\") & "%}"
                        FirstFileWritten = True
                        For j = 1 To myRun.GWClass.RRFiles.Count - 1
                            myJSON &= "," & vbCrLf & vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.GWClass.RRFiles(0), "\", "\\") & "%}"
                        Next
                    End If

                    If myRun.GWClass.FlowFiles.Count > 0 Then
                        If FirstFileWritten Then myJSON &= "," & vbCrLf
                        myJSON &= vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.GWClass.FlowFiles(0), "\", "\\") & "%}"
                        FirstFileWritten = True
                        For j = 1 To myRun.GWClass.FlowFiles.Count - 1
                            myJSON &= "," & vbCrLf & vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.GWClass.FlowFiles(0), "\", "\\") & "%}"
                        Next
                    End If

                    If myRun.GWClass.RTCFiles.Count > 0 Then
                        If FirstFileWritten Then myJSON &= "," & vbCrLf
                        myJSON &= vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.GWClass.RTCFiles(0), "\", "\\") & "%}"
                        FirstFileWritten = True
                        For j = 1 To myRun.GWClass.RTCFiles.Count - 1
                            myJSON &= "," & vbCrLf & vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.GWClass.RTCFiles(0), "\", "\\") & "%}"
                        Next
                    End If

                End If
                If myRun.Extra1Class IsNot Nothing Then myJSON &= "," & vbCrLf & vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.Extra1Class.RRFiles, "\", "\\") & "%}"
                If myRun.Extra2Class IsNot Nothing Then myJSON &= "," & vbCrLf & vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.Extra2Class.RRFiles, "\", "\\") & "%}"
                If myRun.Extra3Class IsNot Nothing Then myJSON &= "," & vbCrLf & vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.Extra3Class.RRFiles, "\", "\\") & "%}"
                If myRun.Extra4Class IsNot Nothing Then myJSON &= "," & vbCrLf & vbTab & vbTab & vbTab & vbTab & "{%path%:%" & Strings.Replace(myRun.Extra4Class.RRFiles, "\", "\\") & "%}"
                myJSON &= vbCrLf
                myJSON &= vbTab & vbTab & vbTab & "]" & vbCrLf
                myJSON &= vbTab & vbTab & "}"
                If i < Selection.Values.Count Then myJSON &= ","
                myJSON &= vbCrLf
            Next

            myJSON &= vbTab & "]" & vbCrLf
            myJSON &= "}"
            Return Replace(myJSON, "%", Chr(34))
        Catch ex As Exception
            Me.Setup.Log.AddError("Error creating populate_cases.json for DHYDROSERVER simulation: " & ex.Message)
            Return String.Empty
        End Try


    End Function

    Public Sub BuildMeteoStationsGrid()
        Try
            Dim da As SQLite.SQLiteDataAdapter
            Dim dt As New DataTable
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            'First load the data
            Dim query As String = "SELECT naam, soort, gebiedsreductie, ARF, oppervlak from METEOSTATIONS;"
            da = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
            da.Fill(dt)

            'populate our datagrid with the result of the query
            grMeteoStations.DataSource = dt

            ''Create the ComboBox column
            'Dim colGebiedsReductie As New DataGridViewComboBoxColumn()
            'colGebiedsReductie.DataPropertyName = "gebiedsreductie"
            'colGebiedsReductie.HeaderText = "Gebiedsreductie"
            'colGebiedsReductie.Name = "colGebiedsreductie"

            ''Instead of using enum values directly, create a list of strings
            'Dim values As New List(Of String)
            'For Each value In [Enum].GetValues(GetType(STOCHLIB.GeneralFunctions.enmGebiedsreductie))
            '    values.Add(value.ToString())
            'Next
            'colGebiedsReductie.DataSource = values

            ''Replace the existing column
            'If grMeteoStations.Columns.Contains("gebiedsreductie") Then
            '    Dim columnIndex As Integer = grMeteoStations.Columns("gebiedsreductie").Index
            '    grMeteoStations.Columns.RemoveAt(columnIndex)
            '    grMeteoStations.Columns.Insert(columnIndex, colGebiedsReductie)
            'End If

            grMeteoStations.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            Me.Setup.SqliteCon.Close()

        Catch fail As Exception
            Dim [error] As [String] = "The following error has occurred:" & vbLf & vbLf
            [error] += fail.Message.ToString() & vbLf & vbLf
            MessageBox.Show([error])
            Me.Close()
        End Try
    End Sub


    Public Sub PopulateOutputLocationsGrid()
        Dim query As [String]

        'this routine refreshes the datagridview that contains output locations
        'create a database me.Setup.connection to the SQLite database that me.Setup.contains the me.Setup.configuration of stochasts.
        'the name of the me.Setup.connection is equal to the path to the database file

        Try

            Dim da As SQLite.SQLiteDataAdapter
            Dim dt As New DataTable

            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            'populate the grid me.Setup.containing meteo stations
            query = "SELECT LOCATIEID,LOCATIENAAM, MODELID,MODULE,MODELPAR,RESULTSFILE,RESULTSTYPE,X,Y,LAT,LON,ZP,WP FROM OUTPUTLOCATIONS;"
            da = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
            da.Fill(dt)
            grOutputLocations.DataSource = dt

            'v2.3.5: auto resize column for locationNaam and LoCATIEID
            For colIdx As Integer = 0 To dt.Columns.Count - 1
                If dt.Columns(colIdx).ColumnName = "LOCATIEID" OrElse dt.Columns(colIdx).ColumnName = "LOCATIENAAM" Then
                    grOutputLocations.Columns(colIdx).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill 'ID of outputlocation
                End If
            Next

            Me.Setup.SqliteCon.Close()

        Catch fail As Exception
            Dim [error] As [String] = "The following error has occurred:" & vbLf & vbLf
            [error] += fail.Message.ToString() & vbLf & vbLf
            MessageBox.Show([error])
            Me.Close()
        End Try

    End Sub

    Public Function GetAddGroupBox(myTabPage As TabPage, GroupBoxText As String, GroupBoxIdx As Integer) As GroupBox
        Dim newGroupBox As New GroupBox
        Dim xLoc As Double

        'first check if we can find an existing groupbox by GroupBoxText
        For Each myControl As Control In myTabPage.Controls
            If myControl.Text.Trim.ToUpper = GroupBoxText.Trim.ToUpper Then Return myControl
        Next

        'if not found, create a new groupbox and return that one
        Dim GroupBoxSpace As Double = tabStochastentool.Width * (grSeizoenen.Rows.Count + 1) * 0.03
        Dim GroupBoxSpacing As Double = GroupBoxSpace / (grSeizoenen.Rows.Count + 1)
        newGroupBox.Text = GroupBoxText
        newGroupBox.Height = (tabStochastentool.Height - tabStochastentool.ItemSize.Height) * 0.96
        newGroupBox.Width = (tabStochastentool.Width - GroupBoxSpace) / grSeizoenen.Rows.Count
        xLoc = (GroupBoxIdx + 1) * GroupBoxSpacing + GroupBoxIdx * newGroupBox.Width
        newGroupBox.Location = New System.Drawing.Point(xLoc, (tabStochastentool.Height - tabStochastentool.ItemSize.Height) * 0.02)
        Return newGroupBox
    End Function


    Private Sub CmbClimate_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbClimate.SelectedIndexChanged
    End Sub

    Private Sub FrmStochasten_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        If Me.Setup.SqliteCon IsNot Nothing Then
            If Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Close()
        End If
    End Sub

    Private Sub FrmStochasten_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try

            'set the license
            ToonPolygonenToolStripMenuItem.Checked = My.Settings.ShowPolygons

            cmbDuration.Items.Add("12")
            cmbDuration.Items.Add("24")
            cmbDuration.Items.Add("48")
            cmbDuration.Items.Add("72")
            cmbDuration.Items.Add("96")
            cmbDuration.Items.Add("192")
            cmbDuration.Items.Add("216")


            Me.Setup.GeneralFunctions.PopulateComboboxWithKlimaatScenarios(cmbClimate)

            'add a handler to the Models datagridview to support selecting a model executable
            'for the executable we will stick to an absolute path!
            AddHandler grModels.CellDoubleClick,
                Sub(sender2, eventargs2)
                    If grModels.Columns(eventargs2.ColumnIndex).Name = "EXECUTABLE" Then
                        Dim dlgOpen As New OpenFileDialog With {
                                    .InitialDirectory = Setup.Settings.RootDir
                                    }
                        dlgOpen.ShowDialog()
                        grModels.Rows(eventargs2.RowIndex).Cells(eventargs2.ColumnIndex).Value = dlgOpen.FileName
                        'Setup.GeneralFunctions.AbsoluteToRelativePath(Setup.Settings.RootDir, dlgOpen.FileName, grModels.Rows(eventargs2.RowIndex).Cells(eventargs2.ColumnIndex).Value)
                    ElseIf grModels.Columns(eventargs2.ColumnIndex).Name = "MODELDIR" Then
                        Dim dlgFolder As New FolderBrowserDialog
                        dlgFolder.ShowDialog()
                        grModels.Rows(eventargs2.RowIndex).Cells(eventargs2.ColumnIndex).Value = Me.Setup.GeneralFunctions.RemovetrailingbackslashFromDir(dlgFolder.SelectedPath)
                    ElseIf grModels.Columns(eventargs2.ColumnIndex).Name = "TEMPWORKDIR" Then
                        Dim dlgFolder As New FolderBrowserDialog
                        dlgFolder.ShowDialog()
                        grModels.Rows(eventargs2.RowIndex).Cells(eventargs2.ColumnIndex).Value = dlgFolder.SelectedPath
                    End If
                End Sub


            Console.WriteLine("Stochastentool launched successfully.")
            Debug.Print("Stochastentool launched successfully.")
        Catch ex As Exception
            Console.WriteLine("Critical error launching stochastentool: " & ex.Message)
            Debug.Print("Critical error launching stochastentool: " & ex.Message)
        End Try

    End Sub

    Public Sub SetDutchProjection()
        Dim projection = New GeoProjection()
        projection.ImportFromEPSG(28992)
    End Sub


    Private Sub PasteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PasteToolStripMenuItem.Click

        'temporarily disable the function cellvaluechanged
        PastedFromClipBoard = True

        If ActiveControl.Name = grWaterLevelSeries.Name Then
            If grWaterLevelSeries.SelectedCells(0).ColumnIndex = 0 Then
                MsgBox("Fout: datums zijn automatisch aangemaakt op basis van de neerslagduur en uitloop en mogen niet worden aangepast.")
            Else
                Call Me.Setup.GeneralFunctions.PasteClipBoardToDataGridView(ActiveControl)
                Call UpdateWaterLevelSeries()
            End If
        ElseIf ActiveControl.Name = grWindSeries.Name Then
            If grWindSeries.SelectedCells(0).ColumnIndex = 0 Then
                MsgBox("Fout: datums zijn automatisch aangemaakt op basis van de neerslagduur en uitloop en mogen niet worden aangepast.")
            Else
                Call Me.Setup.GeneralFunctions.PasteClipBoardToDataGridView(ActiveControl)
                Call UpdateWindSeries()
            End If
        ElseIf ActiveControl.Name = grWaterLevelClasses.Name Then
            Call Me.Setup.GeneralFunctions.PasteClipBoardToDataGridView(ActiveControl)
        ElseIf ActiveControl.Name = grModels.Name Then
            Call Me.Setup.GeneralFunctions.PasteClipBoardToDataGridView(ActiveControl)
        ElseIf ActiveControl.GetType() = GetType(DataGridView) Then
            'v2.3.2 bugfix: only if the active control is of the type datagridview will we paste to a datagridview
            'for all other objects we will not activate a routine that updates the database. 
            'here we will need to implement that inside the control's event listener
            Call Me.Setup.GeneralFunctions.PasteClipBoardToDataGridView(ActiveControl)
            'MsgBox("Plakken van waarden is alleen toegestaan in de grids met tijdreeksen.")
            'PastedFromClipBoard = False
        End If

        'reenable the function cellvaluechanged
        PastedFromClipBoard = False

    End Sub

    Private Function CalcCheckSum(ByRef myGrid As DataGridView, ByVal UseCol As String, ByVal KansCol As String) As Double
        Dim sum As Double = 0
        For Each Row As DataGridViewRow In myGrid.Rows
            If Not IsDBNull(Row.Cells(UseCol).Value) AndAlso Row.Cells(UseCol).Value = True Then
                If Not Row.Cells(KansCol).Value Is DBNull.Value Then sum += Row.Cells(KansCol).Value
            End If
        Next
        Return sum
    End Function

    '========================================================================================================================
    '   XML
    '========================================================================================================================

    Private Sub OpenXMLToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenXMLToolStripMenuItem.Click

        'initialize the progress bar
        Me.Setup.SetProgress(prProgress, lblProgress)

        'first clean up all UI-elements
        For Each myRow As DataGridViewRow In grRuns.Rows
            grRuns.Rows.Remove(myRow)
        Next

        'Initialize the Stochastenanalyse and the progress bar
        Setup.StochastenAnalyse.Initialize()

        dlgOpenFile.Filter = "XML-bestand|*.xml"
        Dim res As DialogResult = dlgOpenFile.ShowDialog()
        If res = DialogResult.OK Then
            Setup.Settings.SetRootDir(Path.GetDirectoryName(dlgOpenFile.FileName) & "\")
            Setup.Settings.RootDir = Path.GetDirectoryName(dlgOpenFile.FileName) & "\"
            Setup.StochastenAnalyse.XMLFile = Me.Setup.GeneralFunctions.FileNameFromPath(dlgOpenFile.FileName)


            Me.Cursor = Cursors.WaitCursor

            If ReadXML(dlgOpenFile.FileName, Setup.Settings.RootDir) Then
                'XML file was read succesfully. Now store the values in the data model

                tabStochastentool.Enabled = True
                btnPopulateRuns.Enabled = True

                If cmbDuration.Text = "" Then
                    cmbDuration.SelectedIndex = 0
                End If

                'stochastic settings
                'v2.2.2: enforcing a valid numerical value for duration
                Setup.StochastenAnalyse.Duration = cmbDuration.Text
                Setup.StochastenAnalyse.DurationAdd = txtUitloop.Text

                'numerical settings
                Setup.StochastenAnalyse.MaxParallel = txtMaxParallel.Text

                'enable menu items that are asssociated with the database and/or me.Setup.configuration
                MappenBeherenToolStripMenuItem.Enabled = True
                KaartToolStripMenuItem.Enabled = True
                GrafiekenToolStripMenuItem.Enabled = True

                Setup.StochastenAnalyse.ResultsDir = Setup.GeneralFunctions.RelativeToAbsolutePath(txtResultatenDir.Text & "\" & Setup.StochastenAnalyse.KlimaatScenario.ToString & "\" & Setup.StochastenAnalyse.Duration.ToString & "H", Setup.Settings.RootDir)
                Setup.StochastenAnalyse.InputFilesDir = Setup.GeneralFunctions.RelativeToAbsolutePath(txtInputDir.Text, Setup.Settings.RootDir)
                Setup.StochastenAnalyse.OutputFilesDir = Setup.GeneralFunctions.RelativeToAbsolutePath(txtOutputDir.Text, Setup.Settings.RootDir)


                Call RebuildAllGrids()

            End If

            'configure the popup-form showing the current pattern
            frmPattern = New frmPatroon(Me.Setup, Setup.StochastenAnalyse.Duration)

            Me.Cursor = Cursors.Default

            'show the logfile
            Me.Setup.Log.write(Setup.Settings.RootDir & "\logfile.txt", True)

        End If


    End Sub

    Public Sub BuildStochasticGrids()

        'remove all controls regarding the stochasts. Luckily the tabs ONLY me.Setup.contain me.Setup.controls regarding the stochasts...
        tabVolumes.Controls.Clear()
        tabPatronen.Controls.Clear()
        Call ClearGroundwaterGrids()    'the groundwater tab also me.Setup.contains other me.Setup.controls
        Call ClearExtraGrids()
        Setup.StochastenAnalyse.ClearGrids()      'also remove the grids from memory

        'now rebuild all grids from scratch
        If Not BuildSeasonsGrid() Then MsgBox("Error building seasons datagrid.")
        If Not BuildVolumesGrids() Then MsgBox("Error building volume datagrids.")
        If Not BuildPatternsGrids() Then MsgBox("Error building pattern datagrids.")
        If Not BuildGroundwaterGrids() Then MsgBox("Error building groundwater datagrids.")
        If Not BuildExtraGrids(1) Then MsgBox("Error building extra1 datagrids.")
        If Not BuildExtraGrids(2) Then MsgBox("Error building extra2 datagrids.")
        If Not BuildExtraGrids(3) Then MsgBox("Error building extra3 datagrids.")
        If Not BuildExtraGrids(4) Then MsgBox("Error building extra4 datagrids.")
    End Sub

    Public Function ReadXML(ByVal path As String, ByVal RootDir As String) As Boolean
        Try
            'v2.3.7: all path references are relative from now on; relative to the location of the XML
            Dim m_xmld As XmlDocument
            Dim m_nodelist As XmlNodeList
            Dim m_node As XmlNode
            Dim n_node As XmlNode
            Dim Catchments As New Dictionary(Of String, String)
            Dim BoundaryNodes As New Dictionary(Of Integer, STOCHLIB.clsStochastBoundaryNode)
            Dim query As String
            Dim Klimaatscenario As String = "STOWA2024_HUIDIG"

            'Create the XML Document
            m_xmld = New XmlDocument()
            'Load the Xml file
            m_xmld.Load(path)

            '------------------------------------------------------------------------------------
            'Get the list of settings-nodes 
            '------------------------------------------------------------------------------------
            m_nodelist = m_xmld.SelectNodes("/stochastentool/instellingen")
            For Each m_node In m_nodelist
                'Loop through the nodes
                For Each n_node In m_node.ChildNodes
                    If n_node.Name.Trim.ToLower = "stochastenmap" Then
                        'depricated. From now on we distinguish two folders: one for input files and one for output files
                        'this code is for backwards compatibility
                        txtInputDir.Text = n_node.InnerText
                        txtOutputDir.Text = n_node.InnerText
                    ElseIf n_node.Name.Trim.ToLower = "invoermap" Then
                        txtInputDir.Text = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(n_node.InnerText, RootDir)
                    ElseIf n_node.Name.Trim.ToLower = "uitvoermap" Then
                        txtOutputDir.Text = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(n_node.InnerText, RootDir)
                    ElseIf n_node.Name.Trim.ToLower = "resultatenmap" Then
                        txtResultatenDir.Text = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(n_node.InnerText, RootDir)
                    ElseIf n_node.Name.Trim.ToLower = "extramodelinputmap" Then
                        txtExtraFilesDir.Text = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(n_node.InnerText, RootDir)
                    ElseIf n_node.Name.Trim.ToLower = "resultatenmodules" Then
                        Setup.GeneralFunctions.readXMLAttributeBOOL(n_node, "Flow1D", chk1D.Checked)
                        Setup.GeneralFunctions.readXMLAttributeBOOL(n_node, "Flow2D", chk2D.Checked)
                    ElseIf n_node.Name.Trim.ToLower = "maxparallel" Then
                        txtMaxParallel.Text = n_node.InnerText
                    ElseIf n_node.Name.Trim.ToLower = "klimaatscenario" Then
                        Klimaatscenario = n_node.InnerText
                    ElseIf n_node.Name.Trim.ToLower = "duur" Then
                        cmbDuration.SelectedItem = n_node.InnerText
                    ElseIf n_node.Name.Trim.ToLower = "uitloop" Then
                        txtUitloop.Text = n_node.InnerText
                    ElseIf n_node.Name.Trim.ToLower = "stochastenconfigfile" Then
                        txtDatabase.Text = Setup.GeneralFunctions.RelativeToAbsolutePath(n_node.InnerText, Setup.Settings.RootDir)
                        Setup.StochastenAnalyse.StochastsConfigFile = txtDatabase.Text
                    ElseIf n_node.Name.Trim.ToLower = "peilgebieden" Then
                        If Setup.GeneralFunctions.readXMLAttribute(n_node, "pad", path) Then
                            Setup.StochastenAnalyse.SubcatchmentShapefile = Setup.GeneralFunctions.RelativeToAbsolutePath(path, RootDir)
                            txtPeilgebieden.Text = Setup.StochastenAnalyse.SubcatchmentShapefile
                            PopulateSubCatchmentComboBoxes()
                            Dim WPField As String = "", ZPField As String = ""
                            Setup.GeneralFunctions.readXMLAttribute(n_node, "winterpeilveld", WPField, True)
                            Setup.GeneralFunctions.readXMLAttribute(n_node, "zomerpeilveld", ZPField, True)
                            If cmbWinterpeil.Items.Contains(WPField) Then cmbWinterpeil.SelectedItem = WPField.Trim.ToUpper
                            If cmbZomerpeil.Items.Contains(ZPField) Then cmbZomerpeil.SelectedItem = ZPField.Trim.ToUpper
                        End If
                    ElseIf n_node.Name.Trim.ToLower = "resultatengecrashtesommentoestaan" Then
                        chkUseCrashedResults.Checked = Me.Setup.GeneralFunctions.BooleanFromText(n_node.InnerText)
                    ElseIf n_node.Name.Trim.ToLower = "simulatieverwijderennakopierenresultaten" Then
                        chkRemoveSimulationAfterCopyingResultsFiles.Checked = Me.Setup.GeneralFunctions.BooleanFromText(n_node.InnerText)
                    ElseIf n_node.Name.Trim.ToLower = "leesresultatenvanafpercentage" Then
                        txtResultsStartPercentage.Text = n_node.InnerText
                    ElseIf n_node.Name.Trim.ToLower = "volumesalsfrequenties" Then
                        'dit is de 'oude' methode waar we neerslagvolumes nog in frequenties uitdrukten. Wordt nog altijd ondersteund
                        Setup.StochastenAnalyse.VolumesAsFrequencies = Me.Setup.GeneralFunctions.BooleanFromText(n_node.InnerText)
                    End If
                Next
            Next

            '------------------------------------------------------------------------------------
            'initialize the database me.Setup.connection
            '------------------------------------------------------------------------------------
            Setup.SetDatabaseConnection(Setup.StochastenAnalyse.StochastsConfigFile)
            Setup.StochastenAnalyse.InitializeDatabaseConnection()
            Call UpgradeDatabase()
            '------------------------------------------------------------------------------------

            PopulateSimulationModelsGrid()

            'now repopulate the datagridview based on the output locations
            query = "SELECT MODELID, MODULE, RESULTSFILE, MODELPAR, LOCATIEID, LOCATIENAAM, RESULTSTYPE, X, Y, LAT, LON, ZP, WP FROM OUTPUTLOCATIONS;"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtLocations, True)
            grOutputLocations.DataSource = dtLocations

            'if our combobox contains the selected climate scenario, select it
            For Each item As Object In cmbClimate.Items
                If item.ToString = Klimaatscenario Then
                    cmbClimate.SelectedItem = item
                    Exit For
                End If
            Next


            Call RebuildAllGrids()

            Me.Setup.SqliteCon.Close()

            Return True

        Catch errorVariable As Exception
            'Error trapping
            Me.Setup.Log.AddError(errorVariable.ToString)
            MsgBox("Error reading XML file: " & errorVariable.ToString)
            Console.Write(errorVariable.ToString())
            Return False
        End Try

    End Function

    Public Sub PopulateSimulationModelsGrid()
        Dim query As String = "SELECT MODELID, MODELTYPE, EXECUTABLE, ARGUMENTS, MODELDIR, CASENAME, TEMPWORKDIR, RESULTSFILES_RR, RESULTSFILES_FLOW, RESULTSFILES_RTC FROM SIMULATIONMODELS;"
        If Not Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtModels, False) Then Throw New Exception("Error retrieving the simulation models from the database.")
        grModels.DataSource = dtModels


        'v2.2.2 make sure to resize all columns
        For Each Col As DataGridViewColumn In grModels.Columns
            Col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        Next

    End Sub


    Public Sub PopulateSubCatchmentComboBoxes()
        'populate the comboboxes
        Call Setup.GeneralFunctions.PopulateComboBoxShapeFields(Me.Setup.StochastenAnalyse.SubcatchmentShapefile, cmbWinterpeil, enmErrorLevel._Message)
        Call Setup.GeneralFunctions.PopulateComboBoxShapeFields(Me.Setup.StochastenAnalyse.SubcatchmentShapefile, cmbZomerpeil, enmErrorLevel._Message)
    End Sub

    Private Sub SaveXMLToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveXMLToolStripMenuItem.Click
        End
    End Sub


    '========================================================================================================================
    '   STOCHAST SEIZOEN
    '========================================================================================================================
    Public Function BuildSeasonsGrid() As Boolean

        Try
            Dim query As [String]

            'this routine refreshes all datagridview instances that me.Setup.contain stochasts with their selection
            If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso System.IO.File.Exists(Me.Setup.StochastenAnalyse.StochastsConfigFile) Then
                'create a database me.Setup.connection to the SQLite database that me.Setup.contains the me.Setup.configuration of stochasts.
                'the name of the me.Setup.connection is equal to the path to the database file

                Dim da As SQLite.SQLiteDataAdapter
                Dim dtSeizoenen As New DataTable

                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

                'populate the grid me.Setup.containing the season statistics
                query = "SELECT SEASON, USE, EVENTSTART, KANS, VOLUME_MULTIPLIER from SEIZOENEN;"
                da = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
                da.Fill(dtSeizoenen)
                grSeizoenen.DataSource = dtSeizoenen
                Me.Setup.SqliteCon.Close()
                Setup.StochastenAnalyse.SeasonGrid = grSeizoenen
                grSeizoenen.Columns(2).DefaultCellStyle.Format = "yyyy-MM-dd"
                'grSeizoenen.Columns(2).HeaderText = "EVENTSTART"  'something goes wrong here. Probably because of the binding. We skip this part
                grSeizoenen.Columns(2).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                grSeizoenen.Columns(4).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

            End If

            lblCheckSumSeizoenen.Text = "Checksum=" & CalcCheckSum(grSeizoenen, "USE", "KANS")

            Return True
        Catch ex As Exception
            Dim [error] As [String] = "The following error has occurred:" & vbLf & vbLf
            [error] += ex.Message.ToString() & vbLf & vbLf
            MessageBox.Show([error])
            Return False
        End Try

    End Function

    '========================================================================================================================
    '   STOCHAST VOLUME
    '========================================================================================================================

    Public Function BuildVolumesGrids() As Boolean
        Dim query As [String]
        Dim GridIdx As Integer = -1
        Dim myGroupBox As GroupBox = Nothing
        Dim myVolumesLabel As Windows.Forms.Label
        Dim myLabels As New List(Of Windows.Forms.Label)

        'this routine refreshes all datagridview instances that me.Setup.contain stochasts with their selection
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Me.Setup.SqliteCon IsNot Nothing Then
            'create a database me.Setup.connection to the SQLite database that me.Setup.contains the me.Setup.configuration of stochasts.
            'the name of the me.Setup.connection is equal to the path to the database file

            Try
                Setup.StochastenAnalyse.VolumeGrids = New List(Of DataGridView)
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

                'remove all existing controls from this tab page before proceeding
                tabVolumes.Controls.Clear()

                For Each myRow As DataGridViewRow In grSeizoenen.Rows

                    If Not IsDBNull(myRow.Cells("USE").Value) AndAlso myRow.Cells("USE").Value = 1 Then
                        Dim dt As New DataTable

                        GridIdx += 1

                        'create a new datagridview for the current season
                        Dim mySeason As String = myRow.Cells(0).Value
                        Dim myVolumesGrid = New DataGridView

                        myVolumesLabel = New Windows.Forms.Label

                        myVolumesGrid.Name = mySeason & "_Volumes"
                        myVolumesGrid.AllowUserToAddRows = False
                        myVolumesGrid.ScrollBars = ScrollBars.Horizontal
                        myVolumesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells

                        myGroupBox = GetAddGroupBox(tabVolumes, mySeason, GridIdx)
                        tabVolumes.Controls.Add(myGroupBox)

                        'add the newly created grid to the tab Volumes
                        myVolumesGrid.Height = myGroupBox.Height * 0.93
                        myVolumesGrid.Width = myGroupBox.Width * 0.93
                        myVolumesGrid.Location = New System.Drawing.Point(myGroupBox.Width * 0.03, myGroupBox.Height * 0.03)
                        myGroupBox.Controls.Add(myVolumesGrid)
                        myVolumesLabel.Location = New Drawing.Point(myGroupBox.Width * 0.03, myGroupBox.Height * 0.97)
                        myVolumesLabel.Text = "Checksum: "
                        myGroupBox.Controls.Add(myVolumesLabel)

                        ''first select the meteostations from our database
                        'Dim mt As DataTable = getMeteoStations()
                        'Dim StationsString As String = ""
                        'For i = 0 To mt.Rows.Count - 1
                        '    StationsString &= ", " & mt.Rows(i)(0).ToString
                        'Next

                        'query = $"Select VOLUME, USE, KANS, KANSCORR{StationsString} from VOLUMES where DUUR=" & cmbDuration.Text & " And SEIZOEN='" & mySeason & "' AND KLIMAATSCENARIO='" & cmbClimate.Text & "' ORDER BY VOLUME;"

                        'populate the newly created grid me.Setup.containing volumes and update them based on the selection of classes
                        query = "SELECT VOLUME, USE, KANS, KANSCORR from VOLUMES where DUUR=" & cmbDuration.Text & " AND SEIZOEN=""" & mySeason & """ AND KLIMAATSCENARIO='" & cmbClimate.Text & "' ORDER BY VOLUME;"

                        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt)
                        myVolumesGrid.DataSource = dt

                        Setup.StochastenAnalyse.VolumeGrids.Add(myVolumesGrid)
                        'dataGrids.Add(myVolumesGrid)
                        myLabels.Add(myVolumesLabel)

                        AddHandler myVolumesGrid.CellValueChanged,
                            Sub(sender2, eventargs2)
                                If myVolumesGrid.Columns(eventargs2.ColumnIndex).Name = "USE" Then Call UpdateVolumes(myVolumesGrid, mySeason, myVolumesLabel)
                            End Sub

                        Call UpdateVolumes(myVolumesGrid, mySeason, myVolumesLabel)

                        'auto-resize columns to fill the entire width.
                        For Each myCol As DataGridViewColumn In myVolumesGrid.Columns
                            myCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        Next
                        'Setup.StochastenAnalyse.UpdateVolumesCheckSum(grVolumesZomer, grVolumesWinter, lblChecksumVolumes)
                    End If


                Next
                Me.Setup.SqliteCon.Close()
                Return True

            Catch fail As Exception
                Dim [error] As [String] = "The following error has occurred:" & vbLf & vbLf
                [error] += fail.Message.ToString() & vbLf & vbLf
                MessageBox.Show([error])
                Me.Close()
            End Try

        End If

    End Function

    Public Function getMeteoStations() As DataTable
        Dim StationsString As String = "" ' = Me.Setup.StochastenAnalyse.MeteoStations.MeteoStations.Values(0).Name
        Dim query As String = "SELECT DISTINCT NAAM FROM METEOSTATIONS;"
        Dim mt As New DataTable
        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, mt)
        Return mt
    End Function

    Public Sub UpdateVolumes(ByRef grVolumes As DataGridView, ByVal Season As String, ByRef lblChecksum As Windows.Forms.Label)

        'this routine updates the database from the datagridview me.Setup.containoing the summer volumes
        Dim query As String, Checksum As Double
        Dim i As Long, myRow As DataGridViewRow, prevRow As DataGridViewRow, nextRow As DataGridViewRow
        Dim Done As Boolean, radius As Integer

        Dim Duur As Integer = Val(cmbDuration.Text)

        'initialize the frequencies for the active rows by setting them equal to the original values
        For Each myRow In grVolumes.Rows
            myRow.Cells("KANSCORR").Value = myRow.Cells("KANS").Value
        Next

        'recalculate the frequencies for the active rows
        For i = 0 To grVolumes.Rows.Count - 1
            myRow = grVolumes.Rows(i)
            If myRow.Cells("USE").Value = 0 Then
                Done = False
                radius = 0
                While Not Done
                    radius += 1

                    'find the previous row
                    If i - radius >= 0 Then
                        prevRow = grVolumes.Rows(i - radius)
                    Else
                        prevRow = Nothing
                    End If

                    'find the next row
                    If i + radius <= grVolumes.Rows.Count - 1 Then
                        nextRow = grVolumes.Rows(i + radius)
                    Else
                        nextRow = Nothing
                    End If

                    If Not prevRow Is Nothing AndAlso Not nextRow Is Nothing Then
                        If prevRow.Cells("USE").Value = 1 AndAlso nextRow.Cells("USE").Value = 1 Then
                            'divide the frequency over the surrounding two
                            prevRow.Cells("KANSCORR").Value += myRow.Cells("KANS").Value / 2
                            nextRow.Cells("KANSCORR").Value += myRow.Cells("KANS").Value / 2
                            myRow.Cells("KANSCORR").Value = 0
                            Done = True
                        ElseIf prevRow.Cells("USE").Value = 1 Then
                            'prevRow is the nearest active one, so assign the entire frequency to that one
                            prevRow.Cells("KANSCORR").Value += myRow.Cells("KANS").Value
                            myRow.Cells("KANSCORR").Value = 0
                            Done = True
                        ElseIf nextRow.Cells("USE").Value = 1 Then
                            'nextRow is the nearest active one, so assign the entire frequency to that one
                            nextRow.Cells("KANSCORR").Value += myRow.Cells("KANS").Value
                            myRow.Cells("KANSCORR").Value = 0
                            Done = True
                        End If
                    ElseIf Not prevRow Is Nothing Then
                        If prevRow.Cells("USE").Value = 1 Then
                            prevRow.Cells("KANSCORR").Value += myRow.Cells("KANS").Value
                            myRow.Cells("KANSCORR").Value = 0
                            Done = True
                        End If
                    ElseIf Not nextRow Is Nothing Then
                        If nextRow.Cells("USE").Value = 1 Then
                            nextRow.Cells("KANSCORR").Value += myRow.Cells("KANS").Value
                            myRow.Cells("KANSCORR").Value = 0
                            Done = True
                        End If
                    End If

                    'veiligheidsklep voor het geval geen enkel volume is aangevinkt
                    If radius > grVolumes.Rows.Count Then
                        Done = True
                    End If

                End While
            End If
        Next

        If cmbDuration.Text <> "" AndAlso cmbClimate.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then

            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            'first update the checkboxes
            Checksum = 0
            For Each myRow In grVolumes.Rows
                Checksum += myRow.Cells("KANS").Value
                query = "UPDATE VOLUMES SET USE=" & myRow.Cells("USE").Value & ", KANSCORR=" & myRow.Cells("KANSCORR").Value & " WHERE VOLUME=" & myRow.Cells("VOLUME").Value & " AND DUUR=" & Duur & " AND SEIZOEN=""" & Season & """ AND KLIMAATSCENARIO='" & cmbClimate.Text & "';"
                Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, True)
            Next
            lblChecksum.Text = "Checksum: " & Checksum

            Me.Setup.SqliteCon.Close()
        End If
    End Sub

    '========================================================================================================================
    '   STOCHAST PATROON
    '========================================================================================================================
    Public Function BuildPatternsGrids() As Boolean
        Dim query As [String]
        Dim GridIdx As Integer = -1
        Dim myGroupBox As GroupBox = Nothing

        'this routine refreshes all datagridview instances that me.Setup.contain stochasts with their selection
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso System.IO.File.Exists(Me.Setup.StochastenAnalyse.StochastsConfigFile) Then
            'create a database me.Setup.connection to the SQLite database that me.Setup.contains the me.Setup.configuration of stochasts.
            'the name of the me.Setup.connection is equal to the path to the database file

            Try

                Setup.StochastenAnalyse.PatternGrids = New List(Of DataGridView)
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

                'remove all existing controls from this tab page before proceeding
                tabPatronen.Controls.Clear()


                For Each myRow As DataGridViewRow In grSeizoenen.Rows

                    If Not IsDBNull(myRow.Cells("USE").Value) AndAlso myRow.Cells("USE").Value = 1 Then

                        Dim da As SQLite.SQLiteDataAdapter
                        Dim dt As New DataTable

                        GridIdx += 1

                        'create a new datagridview for the current season
                        Dim mySeason As String = myRow.Cells(0).Value
                        Dim myPatternsGrid As New DataGridView With {
                        .Name = mySeason & "_Patterns",
                        .AllowUserToAddRows = False
                            }

                        myGroupBox = GetAddGroupBox(tabPatronen, mySeason, GridIdx)
                        tabPatronen.Controls.Add(myGroupBox)

                        'add the newly created grid to the tab Patronen
                        myPatternsGrid.Height = myGroupBox.Height * 0.94
                        myPatternsGrid.Width = myGroupBox.Width * 0.94 - 44
                        myPatternsGrid.Location = New System.Drawing.Point(myGroupBox.Width * 0.03, myGroupBox.Height * 0.03)
                        myGroupBox.Controls.Add(myPatternsGrid)
                        Setup.StochastenAnalyse.PatternGrids.Add(myPatternsGrid)

                        'add a copy button
                        Dim copyButton As New Button
                        copyButton.Size = New Size(23, 23)
                        copyButton.Text = "c"
                        copyButton.Location = New System.Drawing.Point(myGroupBox.Width - 30, 20)
                        myGroupBox.Controls.Add(copyButton)


                        AddHandler copyButton.Click,
                            Sub(sender2, eventargs2)
                                'with this option we will copy the settings from another climate scenario to our current one
                                Dim myFields As New Dictionary(Of String, STOCHLIB.clsDataField)
                                myFields.Add("DUUR", New STOCHLIB.clsDataField("DUUR", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT))
                                myFields.Add("KANS", New STOCHLIB.clsDataField("KANS", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL))
                                myFields.Add("KANSCORR", New STOCHLIB.clsDataField("KANSCORR", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL))
                                myFields.Add("KLIMAATSCENARIO", New STOCHLIB.clsDataField("KLIMAATSCENARIO", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("PATROON", New STOCHLIB.clsDataField("PATROON", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("SEIZOEN", New STOCHLIB.clsDataField("SEIZOEN", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("USE", New STOCHLIB.clsDataField("USE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT))
                                Dim myForm As New frmCopySettingsFromClimateScenario(Me.Setup, "PATRONEN", myFields, cmbClimate.Text)
                                myForm.ShowDialog()
                                If myForm.DialogResult = DialogResult.OK Then
                                    BuildPatternsGrids()    'when the user has copied the configuration from another climate scenario we need to rebuild our grids
                                End If
                            End Sub


                        'populate the grid for this season me.Setup.containing patterns
                        query = "SELECT PATROON, KANS, USE, KANSCORR from PATRONEN where DUUR=" & cmbDuration.Text & " AND SEIZOEN=""" & mySeason & """ AND KLIMAATSCENARIO=""" & cmbClimate.Text & """;"
                        da = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
                        da.Fill(dt)
                        myPatternsGrid.DataSource = dt

                        AddHandler myPatternsGrid.CellValueChanged,
                            Sub(sender2, eventargs2)
                                'invoke only if the "USE" column was changed!
                                If myPatternsGrid.Columns(eventargs2.ColumnIndex).Name = "USE" Then
                                    UpdatePatterns(myPatternsGrid, mySeason)
                                End If
                            End Sub
                        Call UpdatePatterns(myPatternsGrid, mySeason)

                        'also add a label for the checksum to the tab Patronen
                        Dim myLabel As New Label

                        'auto-resize columns to fill the entire width.
                        For Each myCol As DataGridViewColumn In myPatternsGrid.Columns
                            myCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        Next
                    End If


                Next

                Me.Setup.SqliteCon.Close()
                Return True

            Catch fail As Exception
                Dim [error] As [String] = "The following error has occurred:" & vbLf & vbLf
                [error] += fail.Message.ToString() & vbLf & vbLf
                MessageBox.Show([error])
                Return False
            End Try

        End If

    End Function



    Public Sub RefreshPatternsGrids()
        Dim query As [String]

        'this routine refreshes all datagridview instances that me.Setup.contain stochasts with their selection
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso System.IO.File.Exists(Me.Setup.StochastenAnalyse.StochastsConfigFile) Then
            'create a database me.Setup.connection to the SQLite database that me.Setup.contains the me.Setup.configuration of stochasts.
            'the name of the me.Setup.connection is equal to the path to the database file

            Try

                Dim da As SQLite.SQLiteDataAdapter
                Dim dt As New DataTable

                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

                'make a list of all unique seasons
                query = "SELECT DISTINCT SEASON FROM SEIZOENEN;"
                Dim st As New DataTable
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, st, True)

                For i = 0 To st.Rows.Count - 1
                    'populate the grid me.Setup.containing summer volumes
                    query = "SELECT PATROON, USE, KANS from PATRONEN where KLIMAATSCENARIO=""" & Me.Setup.StochastenAnalyse.KlimaatScenario.ToString & """ AND DUUR=" & cmbDuration.Text & " AND SEIZOEN=""" & st.Rows(i)(0).ToString & """;"
                    da = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
                    da.Fill(dt)
                Next

                Me.Setup.SqliteCon.Close()

            Catch fail As Exception
                Dim [error] As [String] = "The following error has occurred:" & vbLf & vbLf
                [error] += fail.Message.ToString() & vbLf & vbLf
                MessageBox.Show([error])
                Me.Close()
            End Try

        End If

        'lblChecksumPatronenWinter.Text = "Checksum=" & CalcCheckSum(grPatronenWinter, "USE", "KANS")
        'lblChecksumPatronenZomer.Text = "Checksum=" & CalcCheckSum(grPatronenZomer, "USE", "KANS")

    End Sub


    Public Sub UpdatePatterns(ByRef PatternGrid As DataGridView, Season As String)
        'this routine updates the probabilities of the patterns based on the checked and unchecked patterns
        Dim Hoog As Boolean, MiddelHoog As Boolean, MiddelLaag As Boolean, Laag As Boolean, Kort As Boolean, Lang As Boolean, Uniform As Boolean
        Dim pHoog As Double, pMiddelHoog As Double, pMiddelLaag As Double, pLaag As Double, pKort As Double, pLang As Double, pUniform As Double
        Dim query As String, Checksum As Double

        'first investigate the active and non-active patterns
        For Each myRow As DataGridViewRow In PatternGrid.Rows
            Select Case myRow.Cells("PATROON").Value.ToString.Trim.ToUpper
                Case Is = "HOOG", "PATROON1D"
                    Hoog = If(myRow.Cells("USE").Value Is DBNull.Value, False, myRow.Cells("USE").Value)
                    pHoog = myRow.Cells("KANS").Value
                Case Is = "MIDDELHOOG", "PATROON1C"
                    MiddelHoog = If(myRow.Cells("USE").Value Is DBNull.Value, False, myRow.Cells("USE").Value)
                    pMiddelHoog = myRow.Cells("KANS").Value
                Case Is = "MIDDELLAAG", "PATROON1B"
                    MiddelLaag = If(myRow.Cells("USE").Value Is DBNull.Value, False, myRow.Cells("USE").Value)
                    pMiddelLaag = myRow.Cells("KANS").Value
                Case Is = "LAAG", "PATROON1A"
                    Laag = If(myRow.Cells("USE").Value Is DBNull.Value, False, myRow.Cells("USE").Value)
                    pLaag = myRow.Cells("KANS").Value
                Case Is = "KORT", "PATROON2A"
                    Kort = If(myRow.Cells("USE").Value Is DBNull.Value, False, myRow.Cells("USE").Value)
                    pKort = myRow.Cells("KANS").Value
                Case Is = "LANG", "PATROON2B"
                    Lang = If(myRow.Cells("USE").Value Is DBNull.Value, False, myRow.Cells("USE").Value)
                    pLang = myRow.Cells("KANS").Value
                Case Is = "UNIFORM", "PATROON0"
                    Uniform = If(myRow.Cells("USE").Value Is DBNull.Value, False, myRow.Cells("USE").Value)
                    pUniform = myRow.Cells("KANS").Value
            End Select
        Next

        'then decide the probabilities
        For Each myRow As DataGridViewRow In PatternGrid.Rows
            Select Case myRow.Cells("PATROON").Value.ToString.Trim.ToUpper
                Case Is = "HOOG", "PATROON1D"
                    If Hoog = False Then
                        myRow.Cells("KANSCORR").Value = 0
                    ElseIf Hoog AndAlso MiddelHoog AndAlso MiddelLaag Then
                        myRow.Cells("KANSCORR").Value = pHoog
                    ElseIf Hoog AndAlso MiddelHoog Then
                        myRow.Cells("KANSCORR").Value = pHoog
                    ElseIf Hoog AndAlso MiddelLaag Then
                        myRow.Cells("KANSCORR").Value = pHoog + pMiddelHoog / 2
                    Else
                        myRow.Cells("KANSCORR").Value = pHoog + pMiddelHoog + pMiddelLaag
                    End If
                Case Is = "MIDDELHOOG", "PATROON1C"
                    If MiddelHoog = False Then
                        myRow.Cells("KANSCORR").Value = 0
                    ElseIf Hoog AndAlso MiddelHoog AndAlso MiddelLaag Then
                        myRow.Cells("KANSCORR").Value = pMiddelHoog
                    ElseIf Hoog Then
                        myRow.Cells("KANSCORR").Value = pMiddelHoog + pMiddelLaag
                    ElseIf MiddelLaag Then
                        myRow.Cells("KANSCORR").Value = pMiddelHoog + pHoog
                    Else
                        myRow.Cells("KANSCORR").Value = pHoog + pMiddelHoog + pMiddelLaag
                    End If
                Case Is = "MIDDELLAAG", "PATROON1B"
                    If MiddelLaag = False Then
                        myRow.Cells("KANSCORR").Value = 0
                    ElseIf MiddelLaag AndAlso MiddelHoog AndAlso Hoog Then
                        myRow.Cells("KANSCORR").Value = pMiddelLaag
                    ElseIf MiddelLaag AndAlso MiddelHoog Then
                        myRow.Cells("KANSCORR").Value = pMiddelLaag
                    ElseIf MiddelLaag AndAlso Hoog Then
                        myRow.Cells("KANSCORR").Value = pMiddelLaag + pMiddelHoog / 2
                    ElseIf MiddelLaag Then
                        myRow.Cells("KANSCORR").Value = pMiddelLaag + pMiddelHoog + pHoog
                    End If
                Case Is = "LAAG", "PATROON1A"
                    If Laag = False Then
                        myRow.Cells("KANSCORR").Value = 0
                    ElseIf Laag AndAlso Uniform Then
                        myRow.Cells("KANSCORR").Value = pLaag
                    Else
                        myRow.Cells("KANSCORR").Value = pLaag + pUniform
                    End If
                Case Is = "UNIFORM", "PATROON0"
                    If Uniform = False Then
                        myRow.Cells("KANSCORR").Value = 0
                    ElseIf Uniform AndAlso Laag Then
                        myRow.Cells("KANSCORR").Value = pUniform
                    Else
                        myRow.Cells("KANSCORR").Value = pLaag + pUniform
                    End If
                Case Is = "KORT", "PATROON2A"
                    If Kort = False Then
                        myRow.Cells("KANSCORR").Value = 0
                    ElseIf Kort AndAlso Lang Then
                        myRow.Cells("KANSCORR").Value = pKort
                    ElseIf Kort Then
                        myRow.Cells("KANSCORR").Value = pKort + pLang
                    End If
                Case Is = "LANG", "PATROON2B"
                    If Lang = False Then
                        myRow.Cells("KANSCORR").Value = 0
                    ElseIf Lang AndAlso Kort Then
                        myRow.Cells("KANSCORR").Value = pLang
                    ElseIf Lang Then
                        myRow.Cells("KANSCORR").Value = pKort + pLang
                    End If
            End Select
        Next

        'finally we will make sure the checksum = 1
        'in case the user has not selected at leas one pattern for each of the three types (one-peak, two-peaks, no-peak), there will be probabilities missing
        Checksum = 0
        For Each myRow In PatternGrid.Rows
            Checksum += myRow.cells("KANSCORR").value
        Next
        If Checksum <> 1 AndAlso Checksum <> 0 Then
            For Each myRow In PatternGrid.Rows
                myRow.cells("KANSCORR").value = myRow.cells("KANSCORR").value * 1 / Checksum
            Next
        End If

        'final check for rounding errors
        Checksum = 0
        For Each myrow In PatternGrid.Rows
            Checksum += myrow.cells("KANSCORR").value
        Next
        Checksum = Math.Round(Checksum, 3)

        If cmbDuration.Text <> "" AndAlso cmbClimate.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            'first update the checkboxes
            Checksum = 0
            For Each myRow In PatternGrid.Rows
                Checksum += myRow.Cells("KANSCORR").Value
                Dim KansCorr As Double
                If Double.IsNaN(myRow.cells("KANSCORR").value) Then KansCorr = 0 Else KansCorr = myRow.cells("KANSCORR").value
                query = "UPDATE PATRONEN SET USE=" & If(IsDBNull(myRow.cells("USE").value), 0, myRow.Cells("USE").Value) & ", KANSCORR=" & KansCorr & " WHERE KLIMAATSCENARIO=""" & cmbClimate.Text & """ AND DUUR=" & Setup.StochastenAnalyse.Duration & " AND SEIZOEN=""" & Season & """ AND PATROON=""" & myRow.Cells("PATROON").Value & """;"
                Dim newCommand = New SQLite.SQLiteCommand(query, Me.Setup.SqliteCon)
                newCommand.ExecuteNonQuery()
            Next
            'lblChecksum.Text = "Checksum: " & CheckSum
            'Call Setup.StochastenAnalyse.UpdateVolumesCheckSum(grVolumesZomer, grVolumesWinter, lblChecksumVolumes)

            Me.Setup.SqliteCon.Close()
        End If

    End Sub

    '========================================================================================================================
    '   STOCHAST GROUNDWATER
    '========================================================================================================================

    Public Function BuildGroundwaterGrids() As Boolean

        Dim query As [String]
        Dim GridIdx As Integer = -1
        Dim myGroupBox As GroupBox = Nothing

        'this routine refreshes all datagridview instances that me.Setup.contain stochasts with their selection
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso System.IO.File.Exists(Me.Setup.StochastenAnalyse.StochastsConfigFile) Then
            'create a database me.Setup.connection to the SQLite database that me.Setup.contains the me.Setup.configuration of stochasts.
            'the name of the me.Setup.connection is equal to the path to the database file

            Try
                Setup.StochastenAnalyse.GroundwaterGrids = New List(Of DataGridView)
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

                'remove all existing controls from this tab page before proceeding
                tabGrondwater.Controls.Clear()

                For Each myRow As DataGridViewRow In grSeizoenen.Rows
                    If Not IsDBNull(myRow.Cells("USE").Value) AndAlso myRow.Cells("USE").Value = 1 Then
                        Dim dt As New DataTable

                        GridIdx += 1

                        'create a new datagridview for the current season
                        Dim mySeason As String = myRow.Cells(0).Value
                        Dim myGroundwaterGrid As New DataGridView With {
                        .Name = mySeason & "_GW",
                        .AllowUserToAddRows = False
                            }

                        'add a groupbox to the tab Volumes
                        myGroupBox = GetAddGroupBox(tabGrondwater, mySeason, GridIdx)
                        tabGrondwater.Controls.Add(myGroupBox)

                        'add the newly created grid to the tab Grondwater
                        myGroundwaterGrid.Height = myGroupBox.Height - 40
                        myGroundwaterGrid.Width = myGroupBox.Width - 44
                        myGroundwaterGrid.Location = New System.Drawing.Point(7, 20)
                        myGroupBox.Controls.Add(myGroundwaterGrid)
                        Me.Setup.StochastenAnalyse.GroundwaterGrids.Add(myGroundwaterGrid)

                        'add an add and remove button the the groupbox as well
                        Dim addButton As New Button
                        Dim remButton As New Button
                        Dim copyButton As New Button
                        addButton.Size = New Size(23, 23)
                        addButton.Text = "+"
                        remButton.Size = New Size(23, 23)
                        remButton.Text = "-"
                        copyButton.Size = New Size(23, 23)
                        copyButton.Text = "c"
                        addButton.Location = New System.Drawing.Point(myGroupBox.Width - 30, 20)
                        remButton.Location = New System.Drawing.Point(myGroupBox.Width - 30, 50)
                        copyButton.Location = New System.Drawing.Point(myGroupBox.Width - 30, 80)
                        myGroupBox.Controls.Add(addButton)
                        myGroupBox.Controls.Add(remButton)
                        myGroupBox.Controls.Add(copyButton)

                        ReadGroundwaterClasses(mySeason, dt)    'reads the groundwater classes for the current season and climate from the database
                        myGroundwaterGrid.DataSource = dt       'binds the results to the datagridview

                        AddHandler myGroundwaterGrid.CellValueChanged,
                            Sub(sender2, eventargs2)
                                Call UpdateGroundwaterClasses(myGroundwaterGrid, mySeason, cmbClimate.Text)
                            End Sub

                        'v2.205: introducing multi-file support for groundwater stochast
                        AddHandler myGroundwaterGrid.CellDoubleClick,
                            Sub(sender2, eventargs2)
                                If myGroundwaterGrid.Columns(eventargs2.ColumnIndex).Name = "RRFILES" OrElse myGroundwaterGrid.Columns(eventargs2.ColumnIndex).Name = "FLOWFILES" OrElse myGroundwaterGrid.Columns(eventargs2.ColumnIndex).Name = "RTCFILES" Then
                                    Dim dlgOpen As New OpenFileDialog With {
                                    .InitialDirectory = Setup.Settings.RootDir,
                                    .Multiselect = True
                                    }
                                    Dim Result As DialogResult = dlgOpen.ShowDialog()
                                    If Result = DialogResult.OK Then
                                        Dim FileNames As String = ""
                                        Setup.GeneralFunctions.AbsoluteToRelativePath(Setup.Settings.RootDir, dlgOpen.FileNames(0), FileNames)
                                        For i = 1 To dlgOpen.FileNames.Length - 1
                                            Dim FileName As String = ""
                                            Setup.GeneralFunctions.AbsoluteToRelativePath(Setup.Settings.RootDir, dlgOpen.FileNames(i), FileName)
                                            FileNames &= ";" & FileName
                                        Next
                                        myGroundwaterGrid.Rows(eventargs2.RowIndex).Cells(eventargs2.ColumnIndex).Value = FileNames
                                    End If
                                ElseIf myGroundwaterGrid.Columns(eventargs2.ColumnIndex).Name = "FOLDER" Then
                                    Dim dlgOpen As New FolderBrowserDialog
                                    'initialize the dialog with the last used directory
                                    dlgOpen.SelectedPath = Setup.Settings.RootDir
                                    Dim Result As DialogResult = dlgOpen.ShowDialog()
                                    If Result = DialogResult.OK Then
                                        Dim FolderName As String = ""
                                        Setup.GeneralFunctions.AbsoluteToRelativePath(Setup.Settings.RootDir, dlgOpen.SelectedPath, FolderName)
                                        myGroundwaterGrid.Rows(eventargs2.RowIndex).Cells(eventargs2.ColumnIndex).Value = FolderName
                                    End If
                                End If
                            End Sub

                        AddHandler addButton.Click,
                            Sub(sender2, eventargs2)
                                query = "INSERT INTO GRONDWATER (KLIMAATSCENARIO, SEIZOEN, NAAM, USE, FOLDER, RRFILES, FLOWFILES, RTCFILES, KANS) VALUES ('" & cmbClimate.Text & "','" & mySeason & "',''," & False & ",'','','','',0);"
                                Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
                                ReadGroundwaterClasses(mySeason, dt)    're-populate the groundwater grid by re-reading the datatable from database
                            End Sub

                        AddHandler remButton.Click,
                            Sub(sender2, eventargs2)
                                If myGroundwaterGrid.SelectedRows.Count > 0 Then
                                    For Each dataRow As DataGridViewRow In myGroundwaterGrid.SelectedRows
                                        query = "DELETE FROM GRONDWATER WHERE KLIMAATSCENARIO='" & cmbClimate.Text & "' AND SEIZOEN=""" & mySeason & """ AND NAAM='" & dataRow.Cells("NAAM").Value & "';"
                                        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
                                    Next
                                    ReadGroundwaterClasses(mySeason, dt)
                                Else
                                    MsgBox("Selecteer eerst de te verwijderen rijen in het gegevensgrid.")
                                End If
                            End Sub

                        AddHandler copyButton.Click,
                            Sub(sender2, eventargs2)
                                'with this option we will copy the settings from another climate scenario to our current one
                                Dim myFields As New Dictionary(Of String, STOCHLIB.clsDataField)
                                myFields.Add("FOLDERS", New STOCHLIB.clsDataField("FOLDERS", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("RRFILES", New STOCHLIB.clsDataField("RRFILES", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("FLOWFILES", New STOCHLIB.clsDataField("FLOWFILES", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("RTCFILES", New STOCHLIB.clsDataField("RTCFILES", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("KANS", New STOCHLIB.clsDataField("KANS", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL))
                                myFields.Add("KLIMAATSCENARIO", New STOCHLIB.clsDataField("KLIMAATSCENARIO", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("NAAM", New STOCHLIB.clsDataField("NAAM", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("SEIZOEN", New STOCHLIB.clsDataField("SEIZOEN", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("USE", New STOCHLIB.clsDataField("USE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT))
                                Dim myForm As New frmCopySettingsFromClimateScenario(Me.Setup, "GRONDWATER", myFields, cmbClimate.Text)
                                myForm.ShowDialog()
                                If myForm.DialogResult = DialogResult.OK Then
                                    BuildGroundwaterGrids()    'when the user has copied the configuration from another climate scenario we need to rebuild our grids
                                End If
                            End Sub


                        'also add a label for the checksum to the tab Patronen
                        Dim myLabel As New Label

                        'auto-resize columns to fill the entire width.
                        For Each myCol As DataGridViewColumn In myGroundwaterGrid.Columns
                            myCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        Next
                    End If
                Next

                Me.Setup.SqliteCon.Close()
                Return True

            Catch fail As Exception
                Dim [error] As [String] = "The following error has occurred:" & vbLf & vbLf
                [error] += fail.Message.ToString() & vbLf & vbLf
                MessageBox.Show([error])
                Return False
            End Try
        End If
    End Function

    Public Function ReadGroundwaterClasses(ByVal Season As String, ByRef dt As DataTable) As Boolean
        'reads all groundwater classes from database for a given climate scenario and season
        'the function populates a datatable that is the data source for the corresponding datagridview
        Try
            Dim query As String
            dt.Clear()
            query = "SELECT NAAM,FOLDER,RRFILES,FLOWFILES,RTCFILES,USE,KANS FROM GRONDWATER WHERE SEIZOEN=""" & Season & """ AND KLIMAATSCENARIO='" & cmbClimate.Text & "';"
            Dim da = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
            da.Fill(dt)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function



    Public Sub UpdateGroundwaterClasses(ByRef gwGrid As DataGridView, Season As String, KlimaatScenario As String)
        'this routine updates the probabilities and filenames for the initial groundwater stochast
        Dim query As String, Checksum As Double
        Dim nAffected As Integer

        If cmbDuration.Text <> "" AndAlso cmbClimate.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            'first delete all rows for the current Season and Climate
            query = "DELETE FROM GRONDWATER WHERE SEIZOEN=""" & Season & """ AND KLIMAATSCENARIO='" & KlimaatScenario & "';"
            Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

            'next write the current datagridview back into the database
            Checksum = 0
            For Each myRow As DataGridViewRow In gwGrid.Rows
                Checksum += myRow.Cells("KANS").Value
                query = "INSERT INTO GRONDWATER (KLIMAATSCENARIO, SEIZOEN, NAAM, USE, FOLDER, RRFILES, FLOWFILES, RTCFILES, KANS) VALUES ('" & KlimaatScenario & "','" & Season & "','" & myRow.Cells("NAAM").Value & "'," & myRow.Cells("USE").Value & ",'" & myRow.Cells("FOLDER").Value & "','" & myRow.Cells("RRFILES").Value & "','" & myRow.Cells("FLOWFILES").Value & "','" & myRow.Cells("RTCFILES").Value & "'," & myRow.Cells("KANS").Value & ");"
                Dim newCommand = New SQLite.SQLiteCommand(query, Me.Setup.SqliteCon)
                nAffected = newCommand.ExecuteNonQuery()
            Next
            'lblChecksum.Text = "Checksum: " & CheckSum
            'Call Setup.StochastenAnalyse.UpdateVolumesCheckSum(grVolumesZomer, grVolumesWinter, lblChecksumVolumes)

            Me.Setup.SqliteCon.Close()
        End If

    End Sub

    Public Sub ClearGroundwaterGrids()
        'this routine removes all existing datagridviews from the groundwater tab INCLUDING the corresponding labels and group boxes
        'it does so by removing the GroupBoxes that me.Setup.contain these grids

        'make a list of me.Setup.controls to remove
        Dim RemoveList As New List(Of Control)
        Dim myControl As Control
        For Each myControl In tabGrondwater.Controls
            If myControl.GetType Is GetType(GroupBox) Then RemoveList.Add(myControl)
        Next

        'actually remove the me.Setup.controls
        For Each myControl In RemoveList
            tabGrondwater.Controls.Remove(myControl)
        Next

    End Sub

    '========================================================================================================================
    '   STOCHAST BOUNDARY
    '========================================================================================================================

    Private Sub BtnAddBoundaryNode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddBoundaryNode.Click
        '--------------------------------------------------------------------------------------------
        'deze routine voegt een boundary node toe aan het datagridview met randknopen
        '--------------------------------------------------------------------------------------------
        Dim myForm As New STOCHLIB.frmAddStochastenNode(Me.Setup, grModels)

        'this routine refreshes all datagridview instances that me.Setup.contain stochasts with their selection
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then

            myForm.ShowDialog()
            If myForm.DialogResult = Windows.Forms.DialogResult.OK Then

                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                Dim cmd As SQLite.SQLiteCommand

                Dim Pars(1) As Object
                Pars(0) = myForm.ModelID
                Pars(1) = myForm.NodeID

                If IsNumeric(Pars(1)) Then
                    MsgBox("Fout: het ID van een boundary node mag geen numerieke waarde zijn. Hernoem het ID in de modelschematisatie.")
                Else
                    'add randknoop as a new record to the RANDKNOPEN table
                    cmd = New SQLite.SQLiteCommand With {
                    .Connection = Me.Setup.SqliteCon,
                    .CommandText = "INSERT INTO RANDKNOPEN (MODELID, KNOOPID) VALUES ('" & Pars(0) & "','" & Pars(1) & "');"
                        }
                    cmd.ExecuteNonQuery()
                End If

            End If

            Call BuildBoundaryNodesGrid()
        End If

    End Sub

    Private Sub BtnDeleteBoundaryNode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeleteBoundaryNode.Click
        Dim KNOOPID As String

        'this routine refreshes all datagridview instances that me.Setup.contain stochasts with their selection
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            For Each myRow As DataGridViewRow In grBoundaryNodes.SelectedRows
                KNOOPID = myRow.Cells("KNOOPID").Value

                'remove the node from the table with nodes
                Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM RANDKNOPEN WHERE KNOOPID='" & KNOOPID & "';")

            Next
            Me.Setup.SqliteCon.Close()

            Call BuildBoundaryNodesGrid()

        End If

    End Sub

    Private Sub BuildBoundaryNodesGrid()
        '--------------------------------------------------------------------------------------------
        'populates the grid me.Setup.containing boundary nodes with values from the database
        '--------------------------------------------------------------------------------------------
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            Dim query As String = "SELECT MODELID, KNOOPID FROM RANDKNOPEN;"
            Dim daBND As New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
            Dim dtBND As New DataTable
            daBND.Fill(dtBND)
            grBoundaryNodes.DataSource = dtBND

            For Each column As DataGridViewColumn In grBoundaryNodes.Columns
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            Next

            Me.Setup.SqliteCon.Close()
        End If
    End Sub

    Private Sub GrBoundaryNodes_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles grBoundaryNodes.CellValueChanged
        MsgBox("Gegevens in dit grid mogen alleen worden gewijzigd met behulp van de knoppen aan de zijkant.")
        BuildBoundaryNodesGrid()
    End Sub

    Private Sub BtnRemoveNode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        '--------------------------------------------------------------------------------------------
        'deze routine verwijdert een randknoop uit het datagridview met boundary nodes
        '--------------------------------------------------------------------------------------------
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If grBoundaryNodes.SelectedRows.Count > 0 Then
                For Each myRow As DataGridViewRow In grBoundaryNodes.SelectedRows
                    Dim KNOOPID As String = myRow.Cells(1).Value
                    If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                    Dim cmd As New SQLite.SQLiteCommand With {
                    .Connection = Me.Setup.SqliteCon,
                    .CommandText = "DELETE FROM RANDKNOPEN WHERE KNOOPID='" & KNOOPID & "';"
                        }
                    cmd.ExecuteNonQuery()
                    grBoundaryNodes.Rows.Remove(myRow)
                Next
            Else
                MsgBox("Selecteer de rijen die verwijderd moeten worden.")
            End If
            Me.Setup.SqliteCon.Close()
        End If
    End Sub

    Private Sub GrWaterhoogteKlassen_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles grWaterLevelClasses.RowHeaderMouseClick
        Call BuildWaterLevelSeriesGrid()
    End Sub

    Private Sub BuildWaterLevelSeriesGrid()
        Dim daBND As SQLite.SQLiteDataAdapter
        Dim dtBND As DataTable

        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Me.Setup.SqliteCon IsNot Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            'gives, in combination with a selected boundary node, a timeseries
            If grWaterLevelClasses.SelectedRows.Count = 1 Then

                Dim dtSeries As DataTable   'a datatable for each timeseries
                dtBND = New DataTable           'a datatable for the entire collection (each boundary)
                Dim timecol As New DataColumn("MINUUT")
                dtBND.Columns.Add(timecol)

                '---------------------------------------------------------------------------------------------------
                'populates the grid containing the timeseries for this combination of boundary and boundary class
                '---------------------------------------------------------------------------------------------------
                Dim NAAM As String = grWaterLevelClasses.SelectedRows(0).Cells("NAAM").Value

                Dim query As String = "SELECT MINUUT, WAARDE"
                Dim colidx As Integer = 0
                Dim minuut As Integer, waarde As Double
                Dim ts As Integer
                For Each myRow As DataGridViewRow In grBoundaryNodes.Rows
                    colidx += 1
                    dtSeries = New DataTable

                    Dim valcol As New DataColumn(myRow.Cells("KNOOPID").Value)
                    dtBND.Columns.Add(valcol)

                    query = "SELECT MINUUT, WAARDE FROM RANDREEKSEN WHERE NODEID='" & myRow.Cells("KNOOPID").Value & "' AND KLIMAATSCENARIO='" & Me.Setup.StochastenAnalyse.KlimaatScenario.ToString & "' AND DUUR=" & cmbDuration.Text & " AND NAAM='" & NAAM & "' ORDER BY MINUUT;"
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtSeries, False)

                    For ts = 0 To dtSeries.Rows.Count - 1
                        minuut = dtSeries.Rows(ts)(0)
                        waarde = dtSeries.Rows(ts)(1)

                        Dim Found As Boolean = False
                        For i = 0 To dtBND.Rows.Count - 1
                            If dtBND.Rows(i)(0) = minuut Then
                                dtBND.Rows(i)(colidx) = waarde
                                Found = True
                                Exit For
                            End If
                        Next
                        If Not Found Then
                            dtBND.Rows.Add()
                            dtBND.Rows(dtBND.Rows.Count - 1)(0) = minuut
                            dtBND.Rows(dtBND.Rows.Count - 1)(colidx) = waarde
                        End If
                    Next
                Next

                grWaterLevelSeries.DataSource = dtBND

                'also show in the chart
                chartBoundaries.Titles.Clear()
                chartBoundaries.Titles.Add("Randvoorwaardenklasse " & NAAM)
                chartBoundaries.Series.Clear()
                chartBoundaries.ChartAreas(0).AxisX.Title = "Minuut"
                chartBoundaries.ChartAreas(0).AxisY.Title = "Waterhoogte (m NAP)"
                For Each myRow As DataGridViewRow In grBoundaryNodes.Rows
                    Dim KnoopID As String = myRow.Cells("KNOOPID").Value
                    chartBoundaries.Series.Add(KnoopID)
                    chartBoundaries.Series(KnoopID).XValueMember = "MINUUT"
                    chartBoundaries.Series(KnoopID).YValueMembers = KnoopID
                    chartBoundaries.Series(KnoopID).ChartType = DataVisualization.Charting.SeriesChartType.Line
                Next
                chartBoundaries.DataSource = dtBND

                ''find out if we're missing dates/values. 10 minutes base is compulsory for water levels!
                'Dim missing As Integer = ((Int(cmbDuration.Text) + Int(txtUitloop.Text)) * 6) - grWaterLevelSeries.Rows.Count
                'If missing > 0 Then
                '    Dim LastTS As Long
                '    If grWaterLevelSeries.Rows.Count > 0 Then
                '        LastTS = grWaterLevelSeries.Rows(grWaterLevelSeries.Rows.Count - 1).Cells("MINUUT").Value
                '    Else
                '        LastTS = 0
                '    End If

                '    'add the missing rows to the database table
                '    If Me.Setup.SqliteCon.State = ConnectionState.Closed Then Me.Setup.SqliteCon.Open()
                '    Dim cmd As New SQLite.SQLiteCommand With {
                '    .Connection = Me.Setup.SqliteCon
                '        }

                '    For i = 0 To missing - 1
                '        cmd.CommandText = "INSERT INTO RANDREEKSEN (KLIMAATSCENARIO, DUUR, NAAM, NODEID, MINUUT, WAARDE) VALUES ('" & Me.Setup.StochastenAnalyse.KlimaatScenario.ToString & "'," & cmbDuration.Text & ",'" & NAAM & "'," & LastTS + i * 10 & ");"
                '        cmd.ExecuteNonQuery()
                '    Next
                'End If

                ''refresh the datagrid
                'daBND = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
                'dtBND = New DataTable
                'daBND.Fill(dtBND)
                'grWaterLevelSeries.DataSource = dtBND

            End If
            Me.Setup.SqliteCon.Close()
        End If
    End Sub

    Private Sub UpdateWaterLevelSeries()
        'changes in the timeseries for water levels (boundaries) must be updated in the database
        Dim cmd As SQLite.SQLiteCommand
        Dim query As String, Timestep As Long
        Dim r As Integer, c As Integer

        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Me.Setup.SqliteCon IsNot Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            'get the name of the boundary class
            Dim NAAM As String = grWaterLevelClasses.SelectedRows(0).Cells("NAAM").Value

            ''first remove the old records that me.Setup.contain data for this class
            cmd = New SQLite.SQLiteCommand With {
            .Connection = Me.Setup.SqliteCon
                }
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            cmd.CommandText = "DELETE * FROM RANDREEKSEN WHERE KLIMAATSCENARIO='" & Me.Setup.StochastenAnalyse.KlimaatScenario.ToString & "' AND DUUR=" & cmbDuration.Text & " AND NAAM='" & NAAM & "';"
            cmd.ExecuteNonQuery()

            'now populate the database again with the data from the grid
            cmd = New SQLite.SQLiteCommand With {
            .Connection = Me.Setup.SqliteCon
                }
            If cmd.Connection.State = ConnectionState.Closed Then cmd.Connection.Open()
            For r = 0 To grWaterLevelSeries.RowCount - 1
                Timestep = grWaterLevelSeries.Rows(r).Cells(0).Value
                query = "INSERT INTO RANDREEKSEN (KLIMAATSCENARIO, DUUR, NAAM, NODEID, MINUUT, WAARDE) VALUES ('" & Me.Setup.StochastenAnalyse.KlimaatScenario.ToString & "'," & cmbDuration.Text & ",'" & NAAM & "','" & grWaterLevelSeries.Columns.Item(c).HeaderText & "'," & Timestep & "," & grWaterLevelSeries.Rows(r).Cells(c).Value & ");"
                cmd.CommandText = query
                cmd.ExecuteNonQuery()
            Next
            Me.Setup.SqliteCon.Close()
        End If
    End Sub

    Private Sub BtnAddWaterLevelRows_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim myForm As New STOCHLIB.frmAddRows, i As Integer
        myForm.ShowDialog()
        If myForm.DialogResult = Windows.Forms.DialogResult.OK Then
            For i = 1 To myForm.Aantal
                grWaterLevelSeries.Rows.Add()
            Next
        End If
    End Sub

    Private Sub GrWaterhoogteKlassen_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles grWaterLevelClasses.CellValueChanged
        '------------------------------------------------------------------------------------------------
        'this routine edits a selected boundary class (row)
        '------------------------------------------------------------------------------------------------
        Dim myRow As DataGridViewRow = grWaterLevelClasses.Rows(e.RowIndex)

        If e.ColumnIndex = 0 Then
            MsgBox("Error: de naam van een klasse mag niet worden gewijzigd. Verwijder de oude en voeg een nieuwe klasse toe met behulp van de knoppen [+] en [-].")
            Call BuildWaterLevelClassesGrid()
        Else
            If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                Dim cmd As New SQLite.SQLiteCommand With {
                .Connection = Me.Setup.SqliteCon,
                .CommandText = "UPDATE RANDVOORWAARDEN SET USE=" & myRow.Cells("USE").Value & ", KANS=" & myRow.Cells("KANS").Value & " WHERE NAAM='" & myRow.Cells("NAAM").Value & "' AND DUUR=" & cmbDuration.Text & " AND KLIMAATSCENARIO='" & cmbClimate.Text & "';"
                    }
                cmd.ExecuteNonQuery()
                Me.Setup.SqliteCon.Close()
                lblBoundaryChecksum.Text = "Checksum=" & CalcCheckSum(grWaterLevelClasses, "USE", "KANS")
            End If
        End If

    End Sub

    Private Sub BtnDeleteBoundaryClass_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeleteBoundaryClass.Click
        '------------------------------------------------------------------------------------------------
        'this routine deletes a selected boundary class (row)
        '------------------------------------------------------------------------------------------------
        If grWaterLevelClasses.SelectedRows.Count > 0 Then
            For Each myRow As DataGridViewRow In grWaterLevelClasses.SelectedRows
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                Dim cmd As New SQLite.SQLiteCommand With {
                .Connection = Me.Setup.SqliteCon,
                .CommandText = "DELETE FROM RANDVOORWAARDEN WHERE NAAM='" & myRow.Cells("NAAM").Value & "' AND DUUR=" & cmbDuration.Text & " AND KLIMAATSCENARIO='" & cmbClimate.Text & "';"
                    }
                cmd.ExecuteNonQuery()

                cmd.CommandText = "DELETE FROM RANDREEKSEN WHERE KLIMAATSCENARIO='" & Me.Setup.StochastenAnalyse.KlimaatScenario.ToString & "' AND NAAM='" & myRow.Cells("NAAM").Value & "' AND DUUR=" & cmbDuration.Text & ";"
                cmd.ExecuteNonQuery()
            Next
            Call BuildWaterLevelClassesGrid()
        Else
            MsgBox("Selecteer de te verwijderen rijen.")
        End If
    End Sub

    Private Sub BtnAddBoundaryClass_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddBoundaryClass.Click
        '------------------------------------------------------------------------------------------------
        'this routine adds a new boundary class
        '------------------------------------------------------------------------------------------------
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            Dim Pars(2) As Object
            Dim myForm As New STOCHLIB.frmAddBoundaryClass
            myForm.ShowDialog()
            If myForm.DialogResult = Windows.Forms.DialogResult.OK Then
                Pars(0) = myForm.NAAM
                Pars(1) = cmbDuration.Text
                Pars(2) = True

                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                Dim cmd As New SQLite.SQLiteCommand With {
                .Connection = Me.Setup.SqliteCon,
                .CommandText = "INSERT INTO RANDVOORWAARDEN (NAAM, DUUR, USE, KLIMAATSCENARIO) VALUES ('" & Pars(0) & "'," & Pars(1) & "," & Pars(2) & ",'" & cmbClimate.Text & "');"
                    }
                cmd.ExecuteNonQuery()
                Me.Setup.SqliteCon.Close()
            End If

            Call BuildWaterLevelClassesGrid()
        End If
    End Sub

    Private Function BuildWaterLevelClassesGrid() As Boolean

        Try
            If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                Dim query As String = "SELECT NAAM, USE, KANS FROM RANDVOORWAARDEN WHERE DUUR=" & cmbDuration.Text & " AND KLIMAATSCENARIO='" & cmbClimate.Text & "' ORDER BY NAAM;"
                Dim dtBND As New DataTable
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtBND)
                grWaterLevelClasses.DataSource = dtBND
                grWaterLevelClasses.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill 'contains the name of the class
                lblBoundaryChecksum.Text = "Checksum=" & CalcCheckSum(grWaterLevelClasses, "USE", "KANS")
                Me.Setup.SqliteCon.Close()
                Setup.StochastenAnalyse.WaterLevelGrid = grWaterLevelClasses
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            MsgBox(ex.Message)
            Return False
        End Try

    End Function

    Private Sub GrWaterLevelSeries_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles grWaterLevelSeries.CellValueChanged
        '------------------------------------------------------------------------------------------------
        'this routine edits a selected boundary timetable record (row)
        '------------------------------------------------------------------------------------------------
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Me.Setup.SqliteCon IsNot Nothing Then

            If PastedFromClipBoard = False Then
                If grWaterLevelClasses.SelectedRows.Count > 0 Then

                    If e.ColumnIndex = 0 Then
                        MsgBox("Fout: datums zijn automatisch aangemaakt op basis van neerslagduur en uitloop en mogen niet worden aangepast.")
                    Else
                        Dim myRow As DataGridViewRow = grWaterLevelSeries.Rows(e.RowIndex)
                        Dim NAAM As String = grWaterLevelClasses.SelectedRows(0).Cells("NAAM").Value
                        Dim TIMESTEP As Long = myRow.Cells("MINUUT").Value
                        Dim DUUR As Integer = cmbDuration.Text
                        Dim KNOOPID As String
                        Dim i As Integer

                        If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                        Dim cmd As New SQLite.SQLiteCommand With {
                        .Connection = Me.Setup.SqliteCon
                            }

                        For i = 0 To grBoundaryNodes.Rows.Count - 1
                            KNOOPID = grBoundaryNodes.Rows(i).Cells("KNOOPID").Value
                            cmd.CommandText = "UPDATE RANDREEKSEN SET WAARDE=" & myRow.Cells("WAARDE").Value & " WHERE KLIMAATSCENARIO='" & Me.Setup.StochastenAnalyse.KlimaatScenario.ToString & "' AND NAAM='" & NAAM & "' AND NODEID='" & KNOOPID & "' AND MINUUT=" & TIMESTEP & " AND DUUR=" & DUUR & ";"
                            cmd.ExecuteNonQuery()
                        Next
                    End If
                End If
            End If
            Me.Setup.SqliteCon.Close()
        End If

    End Sub

    '========================================================================================================================
    '   STOCHAST WIND
    '========================================================================================================================

    Private Sub GrWindKlassen_RowHeaderMouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellMouseEventArgs) Handles grWindKlassen.RowHeaderMouseClick
        Call RefreshWindSeries()
    End Sub

    Private Sub GrWindKlassen_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles grWindKlassen.CellValueChanged
        '------------------------------------------------------------------------------------------------
        'this routine edits a selected wind class (row)
        '------------------------------------------------------------------------------------------------
        Dim myRow As DataGridViewRow = grWindKlassen.Rows(e.RowIndex)
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If e.ColumnIndex = 0 Then
                MsgBox("Fout: de naam van de klasse kan niet worden gewijzigd. Vervang hem door een nieuwe.")
            Else
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                Dim cmd As New SQLite.SQLiteCommand With {
                .Connection = Me.Setup.SqliteCon,
                .CommandText = "UPDATE WIND SET USE=" & myRow.Cells("USE").Value & ", " & cmbClimate.Text & "=" & myRow.Cells(cmbClimate.Text).Value & " WHERE NAAM='" & myRow.Cells("NAAM").Value & "' AND DUUR=" & cmbDuration.Text & ";"
                    }
                cmd.ExecuteNonQuery()
                lblWindChecksum.Text = "Checksum=" & CalcCheckSum(grWindKlassen, "USE", cmbClimate.Text)
            End If
        End If
    End Sub

    Private Sub BtnAddWindClass_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddWindClass.Click
        '------------------------------------------------------------------------------------------------
        'this routine adds a new wind class
        '------------------------------------------------------------------------------------------------
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            Dim Pars(2) As Object
            Dim myForm As New STOCHLIB.frmAddBoundaryClass
            myForm.ShowDialog()
            If myForm.DialogResult = Windows.Forms.DialogResult.OK Then
                Pars(0) = myForm.NAAM
                Pars(1) = cmbDuration.Text
                Pars(2) = True

                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                Dim cmd As New SQLite.SQLiteCommand With {
                .Connection = Me.Setup.SqliteCon,
                .CommandText = "INSERT INTO WIND (NAAM, DUUR, USE) VALUES ('" & Pars(0) & "'," & Pars(1) & "," & Pars(2) & ");"
                    }
                cmd.ExecuteNonQuery()
                Me.Setup.SqliteCon.Close()
            End If

            Call BuildWindGrid()
        End If
    End Sub

    Private Sub BtnDeleteWindClass_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeleteWindClass.Click
        '------------------------------------------------------------------------------------------------
        'this routine deletes a selected wind class (row)
        '------------------------------------------------------------------------------------------------
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If grWindKlassen.SelectedRows.Count > 0 Then
                For Each myRow As DataGridViewRow In grWindKlassen.SelectedRows
                    If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                    Dim cmd As New SQLite.SQLiteCommand With {
                    .Connection = Me.Setup.SqliteCon,
                    .CommandText = "DELETE FROM WIND WHERE NAAM='" & myRow.Cells("NAAM").Value & "' AND DUUR=" & cmbDuration.Text & ";"
                        }
                    cmd.ExecuteNonQuery()

                    'also remove the corresponding records from the timeseries database
                    cmd.CommandText = "DELETE FROM WINDREEKSEN WHERE NAAM='" & myRow.Cells("NAAM").Value & "' AND DUUR=" & cmbDuration.Text & ";"
                    cmd.ExecuteNonQuery()
                    Me.Setup.SqliteCon.Close()
                Next
                Call BuildWindGrid()
            Else
                MsgBox("Selecteer de te verwijderen rijen.")
            End If
        End If
    End Sub

    Private Function BuildWindGrid() As Boolean
        Try
            If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                Dim query As String = "SELECT NAAM, USE, KANS FROM WIND WHERE DUUR=" & cmbDuration.Text & " AND KLIMAATSCENARIO='" & cmbClimate.Text & "';"
                Dim daBND As New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
                Dim dtBND As New DataTable
                daBND.Fill(dtBND)
                grWindKlassen.DataSource = dtBND
                lblWindChecksum.Text = "Checksum=" & CalcCheckSum(grWindKlassen, "USE", cmbClimate.Text)
                Me.Setup.SqliteCon.Close()
                Setup.StochastenAnalyse.WindGrid = grWindKlassen
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            MsgBox(ex.Message)
            Return False
        End Try
    End Function

    Private Sub RefreshWindSeries()

        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            Dim daBND As SQLite.SQLiteDataAdapter
            Dim dtBND As DataTable

            'gives, if a wind class was selected, a timeseries
            If grWindKlassen.SelectedRows.Count = 1 Then

                '---------------------------------------------------------------------------------------------------
                'populates the grid me.Setup.containing the timeseries for this wind class
                '---------------------------------------------------------------------------------------------------
                Dim NAAM As String = grWindKlassen.SelectedRows(0).Cells("NAAM").Value

                Dim query As String = "SELECT DATUM, GRADEN, SNELHEID"
                For Each myRow As DataGridViewRow In grBoundaryNodes.Rows
                    query &= ", " & myRow.Cells("KNOOPID").Value
                Next
                query &= " FROM WINDREEKSEN WHERE DUUR=" & cmbDuration.Text & " AND NAAM='" & NAAM & "' ORDER BY DATUM;"

                daBND = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
                dtBND = New DataTable
                daBND.Fill(dtBND)
                grWindSeries.DataSource = dtBND

                'find out if we're missing dates/values. 1 hour base is compulsory for wind!
                Dim missing As Integer = ((Int(cmbDuration.Text) + Int(txtUitloop.Text))) - grWindSeries.Rows.Count
                If missing > 0 Then
                    Dim LastDate As DateTime
                    If grWindSeries.Rows.Count > 0 Then
                        LastDate = grWindSeries.Rows(grWindSeries.Rows.Count - 1).Cells("DATUM").Value
                    Else
                        LastDate = New DateTime(2000, 1, 1)
                    End If

                    'add the missing rows to the database table
                    Dim cmd As New SQLite.SQLiteCommand With {
                    .Connection = Me.Setup.SqliteCon
                        }
                    For i = 0 To missing - 1
                        cmd.CommandText = "INSERT INTO WINDREEKSEN (DUUR, NAAM, DATUM) VALUES (" & cmbDuration.Text & ",'" & NAAM & "'," & (LastDate + New TimeSpan(i, 0, 0)).ToOADate & ");"
                        cmd.ExecuteNonQuery()
                    Next
                End If

                'refresh the datagrid
                daBND = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
                dtBND = New DataTable
                daBND.Fill(dtBND)
                grWindSeries.DataSource = dtBND
            End If
        End If
        Me.Setup.SqliteCon.Close()
    End Sub

    Private Sub UpdateWindSeries()
        'changes in the timeseries for water levels (boundaries) must be updated in the database
        'note: old method was by using the UPDATE SQL-command, but this was too slow. Now we'll try removing all old data and writing the new data
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            Dim cmd As SQLite.SQLiteCommand
            Dim myVal As Double
            Dim myDeg As Double, myBeaufort As Integer

            'get the name of the wind class
            Dim NAAM As String = grWindKlassen.SelectedRows(0).Cells("NAAM").Value
            Dim DUUR As String = cmbDuration.Text

            'first collect the names of the boundary nodes
            Dim Locs(grBoundaryNodes.Rows.Count - 1) As String
            For i = 0 To grBoundaryNodes.Rows.Count - 1
                Locs(i) = grBoundaryNodes.Rows(i).Cells("KNOOPID").Value
            Next

            For Each myRow As DataGridViewRow In grWindSeries.Rows
                'add the missing rows to the database table
                cmd = New SQLite.SQLiteCommand With {
                .Connection = Me.Setup.SqliteCon
                    }

                If myRow.Cells("GRADEN").Value Is DBNull.Value Then
                    myDeg = 0
                Else
                    myDeg = myRow.Cells("GRADEN").Value
                End If

                If myRow.Cells("BEAUFORT").Value Is DBNull.Value Then
                    myBeaufort = 0
                Else
                    myBeaufort = myRow.Cells("BEAUFORT").Value
                End If

                cmd.CommandText = "UPDATE WINDREEKSEN SET GRADEN=" & myDeg & ", BEAUFORT=" & myBeaufort
                For i = 0 To Locs.Count - 1

                    If myRow.Cells(Locs(0)).Value Is DBNull.Value Then
                        myVal = 0
                    Else
                        myVal = myRow.Cells(Locs(0)).Value
                    End If
                    cmd.CommandText &= "," & Locs(i) & "=" & myVal

                Next
                cmd.CommandText &= " WHERE DUUR=" & cmbDuration.Text & " AND DATUM=" & myRow.Cells("DATUM").Value.toOAdate & " AND NAAM='" & NAAM & "';"
                cmd.ExecuteNonQuery()
            Next
            Me.Setup.SqliteCon.Close()
        End If

    End Sub

    Private Sub GrWindSeries_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles grWindSeries.CellValueChanged
        '------------------------------------------------------------------------------------------------
        'this routine edits a selected wind timetable record (row)
        '------------------------------------------------------------------------------------------------
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            If Not PastedFromClipBoard Then
                If grWindKlassen.SelectedRows.Count > 0 Then

                    If e.ColumnIndex = 0 Then
                        MsgBox("Fout: de datums zijn automatisch aangemaakt op basis van neerslagduur en uitloop en mogen niet worden aangepast.")
                    Else
                        Dim myRow As DataGridViewRow = grWindSeries.Rows(e.RowIndex)
                        Dim NAAM As String = grWindKlassen.SelectedRows(0).Cells("NAAM").Value
                        Dim DATUM As DateTime = myRow.Cells("DATUM").Value
                        Dim DUUR As Integer = cmbDuration.Text
                        Dim KNOOPID As String
                        Dim i As Integer

                        If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                        Dim cmd As New SQLite.SQLiteCommand With {
                        .Connection = Me.Setup.SqliteCon
                            }

                        'update de windrichting en -snelheid
                        cmd.CommandText = "UPDATE WINDREEKSEN SET GRADEN=" & myRow.Cells("GRADEN").Value & " WHERE NAAM='" & NAAM & "' AND DATUM=" & DATUM.ToOADate & " AND DUUR=" & DUUR & ";"
                        cmd.ExecuteNonQuery()
                        cmd.CommandText = "UPDATE WINDREEKSEN SET BEAUFORT=" & myRow.Cells("BEAUFORT").Value & " WHERE NAAM='" & NAAM & "' AND DATUM=" & DATUM.ToOADate & " AND DUUR=" & DUUR & ";"
                        cmd.ExecuteNonQuery()

                        'update de windopzet per boundary node
                        For i = 0 To grBoundaryNodes.Rows.Count - 1
                            KNOOPID = grBoundaryNodes.Rows(i).Cells("KNOOPID").Value
                            cmd.CommandText = "UPDATE WINDREEKSEN SET " & KNOOPID & "='" & myRow.Cells(KNOOPID).Value & "' WHERE NAAM='" & NAAM & "' AND DATUM=" & DATUM.ToOADate & " AND DUUR=" & DUUR & ";"
                            cmd.ExecuteNonQuery()
                        Next

                    End If
                    'Call RefreshWindSeries()
                End If
            End If
        End If
        Me.Setup.SqliteCon.Close()

    End Sub

    Function RowHeaderClicked(sender2, eventargs2)
        ActiveExtraGrid = sender2
        'MsgBox("rows selected: " & sender2.selectedrows.count)
        'SelectedRows = New List(Of Integer)
        'For i = 0 To sender2.rows.Count - 1
        '    If sender2.rows(i).selected Then SelectedRows.Add(i)
        'Next
        'MsgBox("Selectedrows is " & SelectedRows.Count)
    End Function

    '========================================================================================================================
    '   STOCHAST EXTRA1 t/m 4
    '========================================================================================================================

    Private Function BuildExtraGrids(ExtraNum As Integer) As Boolean
        Dim query As [String]
        Dim GridIdx As Integer = -1
        Dim myList As New List(Of DataGridView)


        'this routine refreshes all datagridview instances that me.Setup.contain stochasts with their selection
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso System.IO.File.Exists(Me.Setup.StochastenAnalyse.StochastsConfigFile) Then
            'create a database me.Setup.connection to the SQLite database that me.Setup.contains the me.Setup.configuration of stochasts.
            'the name of the me.Setup.connection is equal to the path to the database file
            Dim myGroupBox As GroupBox = Nothing


            Try

                'first remove all existing datagrids for the current Extra-class
                For Each myControl As Control In tabExtra.Controls
                    Dim i As Integer
                    For i = myControl.Controls.Count - 1 To 0 Step -1
                        If myControl.Controls.Item(i).Name.Contains("_Extra" & ExtraNum.ToString.Trim) Then myControl.Controls.RemoveAt(i)
                    Next
                Next

                myList = New List(Of DataGridView)
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

                For Each myRow As DataGridViewRow In grSeizoenen.Rows

                    If Not IsDBNull(myRow.Cells("USE").Value) AndAlso myRow.Cells("USE").Value = 1 Then
                        Dim dt As New DataTable

                        GridIdx += 1

                        'create a new datagridview for the current season
                        Dim mySeason As String = myRow.Cells(0).Value
                        Dim myExtraGrid As New DataGridView With {
                        .Name = mySeason & "_Extra" & ExtraNum.ToString.Trim,
                        .AllowUserToAddRows = False,
                        .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                        .MultiSelect = True,
                        .SelectionMode = DataGridViewSelectionMode.RowHeaderSelect
                            }

                        'add or get groupbox to the tab Extra
                        If ExtraNum = 1 Then
                            myGroupBox = GetAddGroupBox(tabExtra, mySeason, GridIdx)
                            tabExtra.Controls.Add(myGroupBox)
                        Else
                            For Each myGroupBox In tabExtra.Controls
                                If myGroupBox.Text = mySeason Then Exit For
                            Next
                        End If

                        'add the newly created grid to the tab Patronen
                        Dim GridSpace As Double = myGroupBox.Height * 5 * 0.02
                        Dim GridSpacing As Double = myGroupBox.Height * 0.02

                        myExtraGrid.Height = (myGroupBox.Height - GridSpace) / 4
                        myExtraGrid.Width = myGroupBox.Width - 44
                        myExtraGrid.Location = New System.Drawing.Point(7, (ExtraNum - 1) * myExtraGrid.Height + ExtraNum * GridSpacing)
                        myGroupBox.Controls.Add(myExtraGrid)
                        myList.Add(myExtraGrid)

                        'add an add and remove button the the groupbox as well
                        Dim addButton As New Button
                        Dim remButton As New Button
                        Dim copyButton As New Button
                        addButton.Size = New Size(23, 23)
                        addButton.Text = "+"
                        remButton.Size = New Size(23, 23)
                        remButton.Text = "-"
                        copyButton.Size = New Size(23, 23)
                        copyButton.Text = "c"
                        addButton.Location = New System.Drawing.Point(myGroupBox.Width - 30, myExtraGrid.Location.Y)
                        remButton.Location = New System.Drawing.Point(myGroupBox.Width - 30, myExtraGrid.Location.Y + 30)
                        copyButton.Location = New System.Drawing.Point(myGroupBox.Width - 30, myExtraGrid.Location.Y + 60)
                        myGroupBox.Controls.Add(addButton)
                        myGroupBox.Controls.Add(remButton)
                        myGroupBox.Controls.Add(copyButton)

                        ReadExtraClasses(mySeason, ExtraNum, dt)    'reads the groundwater classes for the current season and climate from the database
                        myExtraGrid.DataSource = dt

                        AddHandler myExtraGrid.RowHeaderMouseClick, AddressOf RowHeaderClicked

                        AddHandler myExtraGrid.CellValueChanged,
                            Sub(sender2, eventargs2)
                                ActiveExtraGrid = sender2
                                Call UpdateExtra(myExtraGrid, ExtraNum, mySeason, cmbClimate.Text)
                            End Sub

                        AddHandler myExtraGrid.CellDoubleClick,
                            Sub(sender2, eventargs2)
                                ActiveExtraGrid = sender2
                                If myExtraGrid.Columns(eventargs2.ColumnIndex).Name = "RRFILES" OrElse myExtraGrid.Columns(eventargs2.ColumnIndex).Name = "FLOWFILES" OrElse myExtraGrid.Columns(eventargs2.ColumnIndex).Name = "RTCFILES" Then
                                    Dim dlgOpen As New OpenFileDialog With {
                                    .Multiselect = True, 'we will allow the user to select multiple files within one class!
                                    .InitialDirectory = Setup.Settings.RootDir
                                    }
                                    dlgOpen.ShowDialog()
                                    If dlgOpen.FileNames.Count > 0 Then
                                        Setup.GeneralFunctions.AbsoluteToRelativePath(Setup.Settings.RootDir, dlgOpen.FileNames(0), myExtraGrid.Rows(eventargs2.RowIndex).Cells(eventargs2.ColumnIndex).Value)
                                        For i = 1 To dlgOpen.FileNames.Count - 1
                                            Dim ExtraPath As String = ""
                                            Setup.GeneralFunctions.AbsoluteToRelativePath(Setup.Settings.RootDir, dlgOpen.FileNames(i), ExtraPath)
                                            myExtraGrid.Rows(eventargs2.RowIndex).Cells(eventargs2.ColumnIndex).Value &= ";" & ExtraPath
                                        Next
                                    End If

                                End If
                            End Sub

                        AddHandler addButton.Click,
                            Sub(sender2, eventargs2)
                                query = "INSERT INTO EXTRA" & ExtraNum & " (KLIMAATSCENARIO, SEIZOEN, NAAM, USE, KANS, RRFILES, FLOWFILES, RTCFILES) VALUES ('" & cmbClimate.Text & "','" & mySeason & "',''," & False & ",0,'','','');"
                                If Not Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query) Then Throw New Exception("Could not execute query to insert stochastic class in database: " & query)
                                BuildExtraGrids(ExtraNum) 'rebuild our extra grid so our changes become visible
                            End Sub

                        AddHandler remButton.Click,
                            Sub(sender2, eventargs2)
                                'MsgBox("Extragrid has " & myExtraGrid.Rows.Count & " rows")
                                'If myExtraGrid.SelectedCells.Count > 0 Then
                                '    Dim rowIdx As Integer = myExtraGrid.SelectedCells.Item(0).RowIndex
                                '    query = "DELETE FROM EXTRA" & ExtraNum & " WHERE KLIMAATSCENARIO='" & cmbClimate.Text & "' AND SEIZOEN='" & mySeason & "' AND NAAM='" & myExtraGrid.Rows(rowIdx).Cells("NAAM").Value & "';"
                                '    Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
                                '    BuildExtraGrids(ExtraNum) 'rebuild our extra grid so our changes become visible
                                'Else
                                '    MsgBox("Selecteer eerst de te verwijderen rij in het gegevensgrid.")
                                'End If

                                If ActiveExtraGrid.SelectedRows.Count > 0 Then
                                    For i = 0 To ActiveExtraGrid.SelectedRows.Count - 1
                                        query = "DELETE FROM EXTRA" & ExtraNum & " WHERE KLIMAATSCENARIO='" & cmbClimate.Text & "' AND SEIZOEN=""" & mySeason & """ AND NAAM='" & ActiveExtraGrid.SelectedRows(i).Cells("NAAM").Value & "';"
                                        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
                                    Next
                                Else
                                    MsgBox("Selecteer eerst de te verwijderen rijen in het gegevensgrid.")
                                End If

                                'If myExtraGrid.SelectedRows.Count > 0 Then
                                '    For Each dataRow As DataGridViewRow In myExtraGrid.SelectedRows
                                '        query = "DELETE FROM EXTRA" & ExtraNum & " WHERE KLIMAATSCENARIO='" & cmbClimate.Text & "' AND SEIZOEN='" & mySeason & "' AND NAAM='" & dataRow.Cells("NAAM").Value & "';"
                                '        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
                                '    Next
                                '    ReadExtraClasses(mySeason, ExtraNum, dt)
                                '    myExtraGrid.DataSource = dt
                                'Else
                                '    MsgBox("Selecteer eerst de te verwijderen rijen in het gegevensgrid.")
                                'End If
                                BuildExtraGrids(ExtraNum) 'rebuild our extra grid so our changes become visible immediately
                            End Sub

                        AddHandler copyButton.Click,
                            Sub(sender2, eventargs2)
                                'with this option we will copy the settings from another climate scenario to our current one
                                Dim myFields As New Dictionary(Of String, STOCHLIB.clsDataField)
                                myFields.Add("KANS", New STOCHLIB.clsDataField("KANS", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL))
                                myFields.Add("KLIMAATSCENARIO", New STOCHLIB.clsDataField("KLIMAATSCENARIO", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("NAAM", New STOCHLIB.clsDataField("NAAM", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("SEIZOEN", New STOCHLIB.clsDataField("SEIZOEN", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("USE", New STOCHLIB.clsDataField("USE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT))
                                myFields.Add("RRFILES", New STOCHLIB.clsDataField("RRFILES", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("FLOWFILES", New STOCHLIB.clsDataField("FLOWFILES", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                myFields.Add("RTCFILES", New STOCHLIB.clsDataField("RTCFILES", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
                                Dim myForm As New frmCopySettingsFromClimateScenario(Me.Setup, "EXTRA" & ExtraNum, myFields, cmbClimate.Text)
                                myForm.ShowDialog()
                                If myForm.DialogResult = DialogResult.OK Then
                                    BuildExtraGrids(ExtraNum)    'when the user has copied the configuration from another climate scenario we need to rebuild our grids
                                End If
                            End Sub

                        Call UpdateExtra(myExtraGrid, ExtraNum, mySeason, cmbClimate.Text)

                        'also add a label for the checksum to the tab Patronen
                        Dim myLabel As New Label

                        'auto-resize columns to fill the entire width.
                        For Each myCol As DataGridViewColumn In myExtraGrid.Columns
                            myCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        Next
                    End If

                Next

                Select Case ExtraNum
                    Case Is = 1
                        Me.Setup.StochastenAnalyse.Extra1Grids = myList
                    Case Is = 2
                        Me.Setup.StochastenAnalyse.Extra2Grids = myList
                    Case Is = 3
                        Me.Setup.StochastenAnalyse.Extra3Grids = myList
                    Case Is = 4
                        Me.Setup.StochastenAnalyse.Extra4Grids = myList
                End Select

                Me.Setup.SqliteCon.Close()
                Return True

            Catch fail As Exception
                Dim [error] As [String] = "The following error has occurred:" & vbLf & vbLf
                [error] += fail.Message.ToString() & vbLf & vbLf
                MessageBox.Show([error])
                Return False
            End Try
        End If

    End Function

    Public Function ReadExtraClasses(ByVal Season As String, ByVal ExtraNum As Integer, ByRef dt As DataTable) As Boolean
        'reads all groundwater classes from database for a given climate scenario and season
        'the function populates a datatable that is the data source for the corresponding datagridview
        Try
            Dim query As String
            dt.Clear()
            query = "SELECT NAAM,USE,KANS,RRFILES,FLOWFILES,RTCFILES FROM EXTRA" & ExtraNum & " WHERE KLIMAATSCENARIO='" & cmbClimate.Text & "' AND SEIZOEN=""" & Season & """;"
            Dim da = New SQLite.SQLiteDataAdapter(query, Me.Setup.SqliteCon)
            da.Fill(dt)
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Sub UpdateExtra(ByRef ExtraGrid As DataGridView, ExtraNum As Integer, Season As String, KlimaatScenario As String)
        'this routine updates the probabilities of the patterns based on the checked and unchecked patterns
        Dim query As String, Checksum As Double
        Dim nAffected As Integer

        If cmbDuration.Text <> "" AndAlso cmbClimate.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            'first delete all instances for the current season and climate scenario
            query = "DELETE FROM EXTRA" & ExtraNum.ToString.Trim & " WHERE KLIMAATSCENARIO='" & KlimaatScenario & "' AND SEIZOEN=""" & Season & """;"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

            'now write the currently active grid to the database
            Checksum = 0
            For Each myRow As DataGridViewRow In ExtraGrid.Rows
                Checksum += myRow.Cells("KANS").Value
                query = "INSERT INTO EXTRA" & ExtraNum.ToString.Trim & " (KLIMAATSCENARIO, SEIZOEN, NAAM, USE, KANS, RRFILES, FLOWFILES, RTCFILES) VALUES ('" & KlimaatScenario & "','" & Season & "','" & myRow.Cells("NAAM").Value & "'," & myRow.Cells("USE").Value & "," & myRow.Cells("KANS").Value & ",'" & myRow.Cells("RRFILES").Value & "','" & myRow.Cells("FLOWFILES").Value & "','" & myRow.Cells("RTCFILES").Value & "');"
                Dim newCommand = New SQLite.SQLiteCommand(query, Me.Setup.SqliteCon)
                nAffected = newCommand.ExecuteNonQuery()
            Next
            'lblChecksum.Text = "Checksum: " & CheckSum
            'Call Setup.StochastenAnalyse.UpdateVolumesCheckSum(grVolumesZomer, grVolumesWinter, lblChecksumVolumes)

            Me.Setup.SqliteCon.Close()
        End If

    End Sub

    Public Sub ClearExtraGrids()
        'this routine removes all existing datagridviews from the Extra tab INCLUDING the corresponding labels and group boxes
        'it does so by removing the GroupBoxes that me.Setup.contain these grids

        'make a list of me.Setup.controls to remove
        Dim RemoveList As New List(Of Control)
        Dim myControl As Control
        For Each myControl In tabExtra.Controls
            If myControl.GetType Is GetType(GroupBox) Then RemoveList.Add(myControl)
        Next

        'actually remove the me.Setup.controls
        For Each myControl In RemoveList
            tabExtra.Controls.Remove(myControl)
        Next

    End Sub

    '========================================================================================================================
    '   POSTPROCESSING
    '========================================================================================================================

    Private Sub BtnPostprocessing_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPostprocessing.Click
        Try
            Me.Setup.SetProgress(prProgress, lblProgress)

            Me.Setup.StochastenAnalyse.InitializeDatabaseConnection()

            Call UpdateHerhalingstijdenTables2D(0)         'return periods for 2D results (depths and levels) for the climate scenario and duration

            'make sure our Stochastenanalyse object knows which climate and duration we're analyzing
            Setup.StochastenAnalyse.SetSettings(cmbClimate.Text, cmbDuration.Text)

            Me.Cursor = Cursors.WaitCursor
            If Not Setup.StochastenAnalyse.CalculateExceedanceTables(chk1D.Checked, chk2D.Checked) Then Throw New Exception("Error calculating exceedance tables.")

            'now that our postprocessing is done, we can enable the publishing and export functionalities
            btnViewer.Enabled = True
            btnExport.Enabled = True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.write(Me.Setup.StochastenAnalyse.ResultsDir & "\logfile.txt", True)
        Finally
            Me.Cursor = Cursors.Default
            Me.Setup.StochastenAnalyse.CloseDatabaseConnection()
        End Try





    End Sub
    Private Sub GrNeerslagstations_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles grMeteoStations.CellValueChanged

        If String.IsNullOrEmpty(cmbClimate.Text) OrElse String.IsNullOrEmpty(cmbDuration.Text) OrElse Setup.SqliteCon Is Nothing Then
            Return
        End If
        If grMeteoStations.Rows.Count = 0 Then Return

        Try
            Using conn As New SQLite.SQLiteConnection(Setup.SqliteCon.ConnectionString)
                conn.Open()

                Dim row = grMeteoStations.Rows(e.RowIndex)
                Dim naam As String = row.Cells(0).Value?.ToString()
                Dim type As String = row.Cells(1).Value?.ToString()?.ToLower()

                ' Parse GebiedsReductie
                Dim gebiedsReductie As enmGebiedsreductie
                If Not [Enum].TryParse(row.Cells(2).Value?.ToString(), True, gebiedsReductie) Then
                    MessageBox.Show("Invalid value for GebiedsReductie")
                    Return
                End If

                Dim arf As Double = Convert.ToDouble(row.Cells(3).Value)
                Dim oppervlak As Double = If(IsDBNull(row.Cells(4).Value), 0, Convert.ToDouble(row.Cells(4).Value))

                Dim query As String
                Using cmd As New SQLite.SQLiteCommand(conn)
                    Select Case e.ColumnIndex
                        Case 0 ' Name updated
                            query = "UPDATE METEOSTATIONS SET naam = @Naam 
                                WHERE soort = @Type 
                                AND gebiedsreductie = @GebiedsReductie 
                                AND ARF = @ARF 
                                AND oppervlak = @Oppervlak"
                        Case 1 ' Type updated
                            query = "UPDATE METEOSTATIONS SET soort = @Type 
                                WHERE naam = @Naam 
                                AND gebiedsreductie = @GebiedsReductie 
                                AND ARF = @ARF 
                                AND oppervlak = @Oppervlak"
                        Case 2 ' GebiedsReductie updated
                            query = "UPDATE METEOSTATIONS SET gebiedsreductie = @GebiedsReductie 
                                WHERE naam = @Naam 
                                AND soort = @Type 
                                AND ARF = @ARF 
                                AND oppervlak = @Oppervlak"
                        Case 3 ' ARF updated
                            query = "UPDATE METEOSTATIONS SET ARF = @ARF 
                                WHERE naam = @Naam 
                                AND soort = @Type 
                                AND gebiedsreductie = @GebiedsReductie 
                                AND oppervlak = @Oppervlak"
                        Case 4 ' Oppervlak updated
                            query = "UPDATE METEOSTATIONS SET oppervlak = @Oppervlak 
                                WHERE naam = @Naam 
                                AND soort = @Type 
                                AND gebiedsreductie = @GebiedsReductie 
                                AND ARF = @ARF"
                        Case Else
                            Return
                    End Select

                    cmd.CommandText = query
                    cmd.Parameters.AddWithValue("@Naam", naam)
                    cmd.Parameters.AddWithValue("@Type", type)
                    cmd.Parameters.AddWithValue("@GebiedsReductie", gebiedsReductie.ToString())
                    cmd.Parameters.AddWithValue("@ARF", arf)
                    cmd.Parameters.AddWithValue("@Oppervlak", oppervlak)

                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error updating database: {ex.Message}")
        End Try
    End Sub


    Private Sub BtnAddMeteoStation_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddMeteoStation.Click
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Me.Setup.SqliteCon IsNot Nothing Then
            Dim myForm As New STOCHLIB.frmAddMeteoStation(Me.Setup)
            myForm.ShowDialog()
            If myForm.DialogResult = Windows.Forms.DialogResult.OK Then
                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                Dim cmd As New SQLite.SQLiteCommand With {
                .Connection = Me.Setup.SqliteCon,
                .CommandText = "INSERT INTO METEOSTATIONS (naam, soort, gebiedsreductie, ARF, oppervlak) VALUES ('" & myForm.naam & "','" & myForm.soort & "','" & myForm.gebiedsreductie.ToString & "'," & myForm.ARF & "," & myForm.oppervlak & ");"
                    }
                cmd.ExecuteNonQuery()
                Me.Setup.SqliteCon.Close()
            End If
            Call BuildMeteoStationsGrid()
        Else
            MsgBox("Set the database connection first!")
        End If
    End Sub

    Private Sub BtnRemoveMeteoStation_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveMeteoStation.Click
        If cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" AndAlso Not Me.Setup.SqliteCon Is Nothing Then
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            Dim cmd As New SQLite.SQLiteCommand With {
            .Connection = Me.Setup.SqliteCon
                }
            For Each myRow As DataGridViewRow In grMeteoStations.SelectedRows
                cmd.CommandText = "DELETE FROM METEOSTATIONS WHERE NAAM='" & myRow.Cells(0).Value & "';"
                cmd.ExecuteNonQuery()
            Next
            Call BuildMeteoStationsGrid()
            Me.Setup.SqliteCon.Close()
        End If
    End Sub


    Private Sub GetijdenClassificerenToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GetijdenClassificerenToolStripMenuItem.Click
        Dim myForm As New STOCHLIB.frmClassifyTidesByStatistics(Me.Setup)
        myForm.Show()
    End Sub

    Public Sub RebuildAllGrids()
        'this routine can only be executed when all data has been read and in place
        If Setup.SqliteCon IsNot Nothing AndAlso Not Setup.SqliteCon.ConnectionString = "" AndAlso cmbClimate.Text <> "" AndAlso cmbDuration.Text <> "" Then
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Output locations...", 0, 20, True)
            PopulateOutputLocationsGrid()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Meteostations controls...", 1, 20, True)
            Call BuildMeteoStationsGrid()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Seasons controls...", 2, 20, True)
            Call BuildSeasonsGrid()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Volumes controls...", 3, 20, True)
            Call BuildVolumesGrids()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Patterns controls...", 4, 20, True)
            Call BuildPatternsGrids()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Groundwater controls...", 5, 20, True)
            Call BuildGroundwaterGrids()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building boundary node controls...", 6, 20, True)
            Call BuildBoundaryNodesGrid()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building waterlevel controls...", 7, 20, True)
            Call BuildWaterLevelClassesGrid()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Wind controls...", 8, 20, True)
            Call BuildWindGrid()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Extra1 controls...", 9, 20, True)
            Call BuildExtraGrids(1)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Extra2 controls...", 10, 20, True)
            Call BuildExtraGrids(2)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Extra3 controls...", 11, 20, True)
            Call BuildExtraGrids(3)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building Extra4 controls...", 12, 20, True)
            Call BuildExtraGrids(4)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Controls successfully created.", 13, 20, True)

        End If
    End Sub

    Private Sub BtnWissen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnWissen.Click

        frmEraseYesNo.ShowDialog()
        If frmEraseYesNo.DialogResult = Windows.Forms.DialogResult.Yes Then
            If grRuns.SelectedRows.Count > 0 Then
                If Not Me.Setup.StochastenAnalyse.Runs.ClearSelected(grRuns, btnPostprocessing) Then
                    MsgBox("Fout bij het opschonen van de geselecteerde runs. me.Setup.controleer de logfile voor meldingen.")
                    Me.Setup.Log.write(Setup.StochastenAnalyse.ResultsDir & "\logfile.txt", True)
                End If
            Else
                MsgBox("Selecteer de rijen van de simulaties waarvan u het resultaat wilt wissen")
            End If

            'ververst het grid met runs
            Me.Setup.StochastenAnalyse.RefreshRunsGrid(grRuns, btnPostprocessing)

        End If

    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        frmAbout.Show()
    End Sub

    Private Sub GrRuns_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles grRuns.SelectionChanged
        Dim nUnrun As Long = 0
        For Each myRow As DataGridViewRow In grRuns.SelectedRows
            If myRow.Cells("DONE").Value = False Then nUnrun += 1
        Next
        lblSelected.Text = grRuns.SelectedRows.Count & " geselecteerd, waarvan " & nUnrun & " nog niet gesimuleerd."
    End Sub

    Public Sub ConfigureSlider2(ByRef dt As DataTable, ByRef tr As Windows.Forms.TrackBar, ByRef lb As Windows.Forms.Label)
        If dt.Rows.Count > 1 Then
            tr.Enabled = True
            tr.Minimum = 0
            tr.Maximum = dt.Rows.Count - 1
            lb.Text = dt.Rows(0)(0)
        ElseIf dt.Rows.Count > 0 Then
            tr.Enabled = False
            tr.Minimum = 0
            tr.Maximum = 1 'can't change this, unfortunately
            tr.Value = 0
            lb.Text = dt.Rows(0)(0)
        End If
    End Sub

    Private Sub ChartControle_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        If clickPosition.HasValue AndAlso e.Location <> clickPosition Then
            tooltip.RemoveAll()
            clickPosition = Nothing
        End If
    End Sub


    'Private Sub StructuurUpdatenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StructuurUpdatenToolStripMenuItem.Click
    Private Sub UpgradeDatabase()
        Dim dt As New DataTable
        Dim ColSchema As Object, TableSchema As Object

        Try
            'in deze routine zorgen we dat de database bijgewerkt wordt en geschikt gemaakt voor de huidige versie van de software
            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            ColSchema = Me.Setup.SqliteCon.GetSchema("COLUMNS")
            TableSchema = Me.Setup.SqliteCon.GetSchema("TABLES")

            Call UpdateClimateScenariosTable(1)
            Call UpdateDurationsTable(2)
            Call UpdateSeasonsTable(3)       'adds all required columns to the seasons table
            Call UpdateVolumesTable(5)       'adds all required columns to the volumes table and reorganizes the data inside
            Call UpdatePatternsTable(10)      'adds all required columns to the patterns table and reorganizes the data inside
            Call UpdateNeerslagverloopTable(15) 'adds all required columns to the neerslagverloop table
            Call UpdateGroundwaterTable(20)   'adds all required columns to the groundwater table and reorganizes the data inside
            Call UpdateBoundariesTable(25)    'adds all required columns to the boundaries table and reorganizes the data inside
            Call UpdateExtraTables()        'adds all required columns to the 'extra' tables and reorganizes the data inside
            Call UpdateResultsTable(35)       'adds all required columns to the results table
            Call UpdateResults2DTable(40)       'adds all required columns to the results table

            Call UpdateExtraFilesPerRunsTable(42) 'adds all required columns to the extra files per run table
            Call UpdateRunsTable(45)          'adds all reaquired columns to the runs table
            Call UpdateSimulationModelsTable(50)
            Call UpdateOutputLocationsTableStructure(55)
            Call UpdatePolygonsTable(60)
            Call UpdateMeteostationsTable(65) 'adds all required columns to the meteostations table
            Call UpdateRandknopenTable(70)    'adds all required columns to the randknopen table
            Call UpdateRandvoorwaardenTable(75) 'adds all required columns to the randvoorwaarden table
            Call UpdateRandreeksenTable(80)   'adds all required columns to the randreeksen table
            Call UpdateWindTable(85)          'adds all required columns to the wind table
            Call UpdateWindreeksenTable(90)   'adds all required columns to the windreeksen table
            Call UpdateHerhalingstijdenTable(95)    'adds all required columns to th exceedance table
            Call UpdateHerhalingstijdenTable2D(98)   'adds all required columns to th exceedance table for 2D results

            Setup.GeneralFunctions.UpdateProgressBar("tables successfully updated", 0, 15, True)
            Me.Setup.SqliteCon.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub


    Public Sub UpdatePolygonsTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               tabel POLYGONS
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table POLYGONS", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "POLYGONS") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "POLYGONS")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "POLYGONS", "SHAPEIDX") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "POLYGONS", "SHAPEIDX", enmSQLiteDataType.SQLITENULL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "POLYGONS", "POINTIDX") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "POLYGONS", "POINTIDX", enmSQLiteDataType.SQLITEINT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "POLYGONS", "LAT") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "POLYGONS", "LAT", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "POLYGONS", "LON") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "POLYGONS", "LON", enmSQLiteDataType.SQLITEREAL)
        '------------------------------------------------------------------------------------
    End Sub

    Public Sub UpdateOutputLocationsTableStructure(progress As Integer)
        '------------------------------------------------------------------------------------
        '               tabel OUTPUTLOCATIONS
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table OUTPUTLOCATIONS", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Setup.SqliteCon, "OUTPUTLOCATIONS") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "OUTPUTLOCATIONS")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "LOCATIEID") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "LOCATIEID", enmSQLiteDataType.SQLITETEXT, "LOC_IDIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "LOCATIENAAM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "LOCATIENAAM", enmSQLiteDataType.SQLITETEXT, "LOC_NAAMIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "MODELID") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "MODELID", enmSQLiteDataType.SQLITEINT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "MODULE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "MODULE", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "RESULTSFILE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "RESULTSFILE", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "MODELPAR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "MODELPAR", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "RESULTSTYPE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "RESULTSTYPE", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "X") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "X", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "Y") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "Y", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "LAT") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "LAT", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "LON") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "LON", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "ZP") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "ZP", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Setup.SqliteCon, "OUTPUTLOCATIONS", "WP") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "OUTPUTLOCATIONS", "WP", enmSQLiteDataType.SQLITEREAL)
        '------------------------------------------------------------------------------------
    End Sub

    Public Sub UpdateSimulationModelsTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE THE TABLE SIMULATIONMODELS
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table SIMULATIONMODELS", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "SIMULATIONMODELS") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "SIMULATIONMODELS")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "MODELID") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "MODELID", enmSQLiteDataType.SQLITETEXT, "SIM_MODELIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "MODELTYPE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "MODELTYPE", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "EXECUTABLE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "EXECUTABLE", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "ARGUMENTS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "ARGUMENTS", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "MODELDIR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "MODELDIR", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "CASENAME") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "CASENAME", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "TEMPWORKDIR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "TEMPWORKDIR", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "RESULTSFILES_RR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "RESULTSFILES_RR", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "RESULTSFILES_FLOW") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "RESULTSFILES_FLOW", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SIMULATIONMODELS", "RESULTSFILES_RTC") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SIMULATIONMODELS", "RESULTSFILES_RTC", enmSQLiteDataType.SQLITETEXT)
    End Sub

    Public Sub UpdateExtraFilesPerRunsTable(progress As Integer)
        '--------------------------------------------------------------------------------
        'update the table RUNS
        '--------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table EXTRAFILES", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "EXTRAFILES") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "EXTRAFILES")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "EXTRAFILES", "RUNID") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "EXTRAFILES", "RUNID", enmSQLiteDataType.SQLITETEXT, "EXTRAFILES_RUNIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "EXTRAFILES", "RRFILES") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "EXTRAFILES", "RRFILES", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "EXTRAFILES", "FLOWFILES") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "EXTRAFILES", "FLOWFILES", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "EXTRAFILES", "RTCFILES") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "EXTRAFILES", "RTCFILES", enmSQLiteDataType.SQLITETEXT)
        '------------------------------------------------------------------------------------
    End Sub

    Public Sub UpdateRunsTable(progress As Integer)
        '--------------------------------------------------------------------------------
        'update the table RUNS
        '--------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table RUNS", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "RUNS") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "RUNS")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, "RUNS_KLIMAATIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "DUUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "DUUR", enmSQLiteDataType.SQLITEINT, "RUNS_DUURIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "RUNID") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "RUNID", enmSQLiteDataType.SQLITETEXT, "RUNS_RUNIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "RUNIDX") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "RUNIDX", enmSQLiteDataType.SQLITEINT, "RUNS_RUNIDXIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "RELATIVEDIR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "RELATIVEDIR", enmSQLiteDataType.SQLITETEXT, "RUNS_DIRIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "SEIZOEN") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "SEIZOEN", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "SEIZOEN_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "SEIZOEN_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "VOLUME") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "VOLUME", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "VOLUME_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "VOLUME_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "PATROON") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "PATROON", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "PATROON_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "PATROON_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "GW") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "GW", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "GW_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "GW_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "BOUNDARY") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "BOUNDARY", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "BOUNDARY_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "BOUNDARY_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "WIND") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "WIND", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "WIND_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "WIND_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "EXTRA1") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "EXTRA1", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "EXTRA1_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "EXTRA1_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "EXTRA2") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "EXTRA2", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "EXTRA2_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "EXTRA2_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "EXTRA3") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "EXTRA3", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "EXTRA3_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "EXTRA3_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "EXTRA4") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "EXTRA4", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "EXTRA4_P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "EXTRA4_P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "P") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "P", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RUNS", "RELATIVEDIR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RUNS", "RELATIVEDIR", enmSQLiteDataType.SQLITETEXT)

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("RUNS", "KLIMAATSCENARIO")

        '------------------------------------------------------------------------------------
    End Sub

    Public Sub UpdateResultsTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL RESULTATEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table RESULTATEN", progress, 100, True)
        Dim Fields As New Dictionary(Of String, clsSQLiteField)
        Fields.Add("KLIMAATSCENARIO", New clsSQLiteField("KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("DUUR", New clsSQLiteField("DUUR", enmSQLiteDataType.SQLITEINT, True))
        Fields.Add("LOCATIENAAM", New clsSQLiteField("LOCATIENAAM", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("RUNID", New clsSQLiteField("RUNID", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("P", New clsSQLiteField("P", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("MINVAL", New clsSQLiteField("MINVAL", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("MAXVAL", New clsSQLiteField("MAXVAL", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("AVGVAL", New clsSQLiteField("AVGVAL", enmSQLiteDataType.SQLITEREAL, False))
        Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteTable(Me.Setup.SqliteCon, "RESULTATEN", Fields)

        Dim CompositeIndex As New List(Of String) From {"KLIMAATSCENARIO", "DUUR", "LOCATIENAAM", "RUNID"}
        Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteCompositeIndex(Me.Setup.SqliteCon, "RESULTATEN", CompositeIndex)

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("RESULTATEN", "KLIMAATSCENARIO")
        '--------------------------------------------------------------------------------
    End Sub

    Public Sub UpdateResultsTables2D(progress As Integer)
        '------------------------------------------------------------------------------------
        ' UPDATE TABELS MAXDEPTH2D and MAXLEVEL: one for each climate scenario and duration a unique table
        '------------------------------------------------------------------------------------

        Dim kt As New DataTable
        Dim dt As New DataTable

        Dim k As Integer
        Dim parameter As enm2DParameter
        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT DISTINCT NAAM FROM KLIMAATSCENARIOS;", kt, False)
        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT DISTINCT DUUR FROM DUREN;", dt, False)

        For k = 0 To 1
            If k = 0 Then parameter = enm2DParameter.waterlevel Else parameter = enm2DParameter.depth
            Dim TableName As String = Me.Setup.StochastenAnalyse.get2DResultsTableName(parameter, Me.Setup.StochastenAnalyse.KlimaatScenario.ToString, Me.Setup.StochastenAnalyse.Duration)
            If Not Me.Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, TableName) Then Me.Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, TableName)

            Setup.GeneralFunctions.UpdateProgressBar($"Updating table {TableName}", progress, 100, True)

            'clear the table before proceeding
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM " & TableName & ";", False)

            Dim Fields As New Dictionary(Of String, clsSQLiteField)

            'Upgrade our MAXDIEPTE2D table
            Fields = New Dictionary(Of String, clsSQLiteField)
            Fields.Add("FEATUREIDX", New clsSQLiteField("FEATUREIDX", enmSQLiteDataType.SQLITEINT, True))
            Fields.Add("RUNID", New clsSQLiteField("RUNID", enmSQLiteDataType.SQLITETEXT, True))
            Fields.Add("P", New clsSQLiteField("P", enmSQLiteDataType.SQLITEREAL, False))
            Fields.Add("WAARDE", New clsSQLiteField("WAARDE", enmSQLiteDataType.SQLITEREAL, False))
            Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteTable(Me.Setup.SqliteCon, TableName, Fields)

            Dim CompositeIndex As New List(Of String) From {"FEATUREIDX", "RUNID"}
            Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteCompositeIndex(Me.Setup.SqliteCon, TableName, CompositeIndex)
        Next

        '--------------------------------------------------------------------------------

    End Sub




    Public Sub UpdateResults2DTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL RESULTATEN2D
        '------------------------------------------------------------------------------------

        'DROP TABLE RESULTATEN2D. We will now store the results in separate tables for each parameter and statistic
        If Me.Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "RESULTATEN2D") Then Me.Setup.GeneralFunctions.SQLiteDropTable(Me.Setup.SqliteCon, "RESULTATEN2D")
        Exit Sub

        Setup.GeneralFunctions.UpdateProgressBar("Updating table RESULTATEN2D", progress, 100, True)
        Dim Fields As New Dictionary(Of String, clsSQLiteField)

        'Upgrade our RESULTATEN2D table
        Fields = New Dictionary(Of String, clsSQLiteField)
        Fields.Add("KLIMAATSCENARIO", New clsSQLiteField("KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("DUUR", New clsSQLiteField("DUUR", enmSQLiteDataType.SQLITEINT, True))
        Fields.Add("FEATUREIDX", New clsSQLiteField("FEATUREIDX", enmSQLiteDataType.SQLITEINT, True))
        Fields.Add("RUNID", New clsSQLiteField("RUNID", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("P", New clsSQLiteField("P", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("PARAMETER", New clsSQLiteField("PARAMETER", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("MINVAL", New clsSQLiteField("MINVAL", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("MAXVAL", New clsSQLiteField("MAXVAL", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("AVGVAL", New clsSQLiteField("AVGVAL", enmSQLiteDataType.SQLITEREAL, False))
        Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteTable(Me.Setup.SqliteCon, "RESULTATEN2D", Fields)

        Dim CompositeIndex As New List(Of String) From {"KLIMAATSCENARIO", "DUUR", "FEATUREIDX", "RUNID", "PARAMETER"}
        Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteCompositeIndex(Me.Setup.SqliteCon, "RESULTATEN2D", CompositeIndex)

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("RESULTATEN2D", "KLIMAATSCENARIO")
        '--------------------------------------------------------------------------------
    End Sub



    Public Sub UpdateHerhalingstijdenTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL RESULTATEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table HERHALINGSTIJDEN", progress, 100, True)
        Dim Fields As New Dictionary(Of String, clsSQLiteField)

        'Upgrade our HERHALINGSTIJDEN table
        Fields = New Dictionary(Of String, clsSQLiteField)
        Fields.Add("KLIMAATSCENARIO", New clsSQLiteField("KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("DUUR", New clsSQLiteField("DUUR", enmSQLiteDataType.SQLITEINT, True))
        Fields.Add("LOCATIENAAM", New clsSQLiteField("LOCATIENAAM", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("RUNID", New clsSQLiteField("RUNID", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("SEIZOEN", New clsSQLiteField("SEIZOEN", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("VOLUME", New clsSQLiteField("VOLUME", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("PATROON", New clsSQLiteField("PATROON", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("GW", New clsSQLiteField("GW", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("BOUNDARY", New clsSQLiteField("BOUNDARY", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("WIND", New clsSQLiteField("WIND", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("EXTRA1", New clsSQLiteField("EXTRA1", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("EXTRA2", New clsSQLiteField("EXTRA2", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("EXTRA3", New clsSQLiteField("EXTRA3", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("EXTRA4", New clsSQLiteField("EXTRA4", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("HERHALINGSTIJD", New clsSQLiteField("HERHALINGSTIJD", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("WAARDE", New clsSQLiteField("WAARDE", enmSQLiteDataType.SQLITEREAL, False))
        Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteTable(Me.Setup.SqliteCon, "HERHALINGSTIJDEN", Fields)

        Dim CompositeIndex As New List(Of String) From {"KLIMAATSCENARIO", "DUUR", "LOCATIENAAM", "RUNID"}
        Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteCompositeIndex(Me.Setup.SqliteCon, "HERHALINGSTIJDEN", CompositeIndex)
        '--------------------------------------------------------------------------------
    End Sub


    Public Sub UpdateHerhalingstijdenTables2D(progress As Integer)

        '------------------------------------------------------------------------------------
        '               UPDATE TABEL RESULTATEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Tabellen bijwerken voor herhalingstijden 2D", progress, 100, True)

        Dim kt As New DataTable
        Dim dt As New DataTable

        Dim k As Integer
        Dim parameter As enm2DParameter
        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT DISTINCT NAAM FROM KLIMAATSCENARIOS;", kt, True)
        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT DISTINCT DUUR FROM DUREN;", dt, True)

        For k = 0 To 1
            If k = 1 Then parameter = enm2DParameter.depth Else parameter = enm2DParameter.waterlevel
            Dim TableName As String = Me.Setup.StochastenAnalyse.get2DReturnPeriodsTableName(parameter, Me.Setup.StochastenAnalyse.KlimaatScenario.ToString, Me.Setup.StochastenAnalyse.Duration)
            If Not Me.Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, TableName) Then Me.Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, TableName)

            Setup.GeneralFunctions.UpdateProgressBar($"Updating table {TableName}", progress, 100, True)

            'clear the table before proceeding
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM " & TableName & ";", False)

            Dim Fields As New Dictionary(Of String, clsSQLiteField)

            'Upgrade our HERHALINGSTIJDEN2D table
            Fields = New Dictionary(Of String, clsSQLiteField)
            Fields.Add("FEATUREIDX", New clsSQLiteField("FEATUREIDX", enmSQLiteDataType.SQLITEINT, True))
            Fields.Add("RUNID", New clsSQLiteField("RUNID", enmSQLiteDataType.SQLITETEXT, True))
            Fields.Add("RUNIDX", New clsSQLiteField("RUNIDX", enmSQLiteDataType.SQLITEINT, True))
            Fields.Add("SEIZOEN", New clsSQLiteField("SEIZOEN", enmSQLiteDataType.SQLITETEXT, False))
            Fields.Add("VOLUME", New clsSQLiteField("VOLUME", enmSQLiteDataType.SQLITEREAL, False))
            Fields.Add("PATROON", New clsSQLiteField("PATROON", enmSQLiteDataType.SQLITETEXT, False))
            Fields.Add("GW", New clsSQLiteField("GW", enmSQLiteDataType.SQLITETEXT, False))
            Fields.Add("BOUNDARY", New clsSQLiteField("BOUNDARY", enmSQLiteDataType.SQLITETEXT, False))
            Fields.Add("WIND", New clsSQLiteField("WIND", enmSQLiteDataType.SQLITETEXT, False))
            Fields.Add("EXTRA1", New clsSQLiteField("EXTRA1", enmSQLiteDataType.SQLITETEXT, False))
            Fields.Add("EXTRA2", New clsSQLiteField("EXTRA2", enmSQLiteDataType.SQLITETEXT, False))
            Fields.Add("EXTRA3", New clsSQLiteField("EXTRA3", enmSQLiteDataType.SQLITETEXT, False))
            Fields.Add("EXTRA4", New clsSQLiteField("EXTRA4", enmSQLiteDataType.SQLITETEXT, False))
            Fields.Add("HERHALINGSTIJD", New clsSQLiteField("HERHALINGSTIJD", enmSQLiteDataType.SQLITEREAL, False))
            Fields.Add("WAARDE", New clsSQLiteField("WAARDE", enmSQLiteDataType.SQLITEREAL, False))
            Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteTable(Me.Setup.SqliteCon, TableName, Fields)

            Dim CompositeIndex As New List(Of String) From {"FEATUREIDX", "RUNID"}
            Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteCompositeIndex(Me.Setup.SqliteCon, TableName, CompositeIndex)

        Next


        '--------------------------------------------------------------------------------
    End Sub

    Public Sub UpdateHerhalingstijdenTable2D(progress As Integer)

        'table depricated
        If Me.Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "HERHALINGSTIJDEN2D") Then Me.Setup.GeneralFunctions.SQLiteDropTable(Me.Setup.SqliteCon, "HERHALINGSTIJDEN2D")
        Exit Sub

        '------------------------------------------------------------------------------------
        '               UPDATE TABEL RESULTATEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table HERHALINGSTIJDEN2D", progress, 100, True)
        Dim Fields As New Dictionary(Of String, clsSQLiteField)

        'Upgrade our HERHALINGSTIJDEN table
        Fields = New Dictionary(Of String, clsSQLiteField)
        Fields.Add("KLIMAATSCENARIO", New clsSQLiteField("KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("DUUR", New clsSQLiteField("DUUR", enmSQLiteDataType.SQLITEINT, True))
        Fields.Add("PARAMETER", New clsSQLiteField("PARAMETER", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("FEATUREIDX", New clsSQLiteField("FEATUREIDX", enmSQLiteDataType.SQLITEINT, True))
        Fields.Add("RUNID", New clsSQLiteField("RUNID", enmSQLiteDataType.SQLITETEXT, True))
        Fields.Add("RUNIDX", New clsSQLiteField("RUNIDX", enmSQLiteDataType.SQLITEINT, True))
        Fields.Add("SEIZOEN", New clsSQLiteField("SEIZOEN", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("VOLUME", New clsSQLiteField("VOLUME", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("PATROON", New clsSQLiteField("PATROON", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("GW", New clsSQLiteField("GW", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("BOUNDARY", New clsSQLiteField("BOUNDARY", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("WIND", New clsSQLiteField("WIND", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("EXTRA1", New clsSQLiteField("EXTRA1", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("EXTRA2", New clsSQLiteField("EXTRA2", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("EXTRA3", New clsSQLiteField("EXTRA3", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("EXTRA4", New clsSQLiteField("EXTRA4", enmSQLiteDataType.SQLITETEXT, False))
        Fields.Add("HERHALINGSTIJD", New clsSQLiteField("HERHALINGSTIJD", enmSQLiteDataType.SQLITEREAL, False))
        Fields.Add("WAARDE", New clsSQLiteField("WAARDE", enmSQLiteDataType.SQLITEREAL, False))
        Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteTable(Me.Setup.SqliteCon, "HERHALINGSTIJDEN2D", Fields)

        Dim CompositeIndex As New List(Of String) From {"KLIMAATSCENARIO", "DUUR", "PARAMETER", "FEATUREIDX", "RUNID"}
        Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteCompositeIndex(Me.Setup.SqliteCon, "HERHALINGSTIJDEN2D", CompositeIndex)

        Dim CompositeIndex2 As New List(Of String) From {"KLIMAATSCENARIO", "DUUR", "PARAMETER", "FEATUREIDX", "RUNIDX"}
        Me.Setup.GeneralFunctions.CreateOrUpdateSQLiteCompositeIndex(Me.Setup.SqliteCon, "HERHALINGSTIJDEN2D", CompositeIndex2)

        '--------------------------------------------------------------------------------
    End Sub


    Public Sub UpdateMeteostationsTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL METEOSTATIONS
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table METEOSTATIONS", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "METEOSTATIONS") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "METEOSTATIONS")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "METEOSTATIONS", "SEASON") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "METEOSTATIONS", "NAAM", enmSQLiteDataType.SQLITETEXT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "METEOSTATIONS", "SOORT") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "METEOSTATIONS", "SOORT", enmSQLiteDataType.SQLITETEXT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "METEOSTATIONS", "GEBIEDSREDUCTIE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "METEOSTATIONS", "GEBIEDSREDUCTIE", enmSQLiteDataType.SQLITETEXT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "METEOSTATIONS", "ARF") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "METEOSTATIONS", "ARF", enmSQLiteDataType.SQLITEREAL)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "METEOSTATIONS", "OPPERVLAK") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "METEOSTATIONS", "OPPERVLAK", enmSQLiteDataType.SQLITEREAL)
    End Sub

    Public Sub UpdateRandknopenTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL RANDKNOPEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table RANDKNOPEN", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "RANDKNOPEN") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "RANDKNOPEN")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDKNOPEN", "SEASON") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDKNOPEN", "KNOOPID", enmSQLiteDataType.SQLITETEXT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDKNOPEN", "SOORT") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDKNOPEN", "MODELID", enmSQLiteDataType.SQLITEINT)
    End Sub

    Public Sub UpdateRandreeksenTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL RANDREEKSEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table RANDREEKSEN", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "RANDREEKSEN") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "RANDREEKSEN")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDREEKSEN", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDREEKSEN", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, "REEKS_SCNARIOIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDREEKSEN", "NODEID") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDREEKSEN", "NODEID", enmSQLiteDataType.SQLITETEXT, "REEKS_NODEIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDREEKSEN", "NAAM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDREEKSEN", "NAAM", enmSQLiteDataType.SQLITETEXT, "REEKS_NAAMIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDREEKSEN", "DUUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDREEKSEN", "DUUR", enmSQLiteDataType.SQLITEINT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDREEKSEN", "MINUUT") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDREEKSEN", "MINUUT", enmSQLiteDataType.SQLITEINT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDREEKSEN", "WAARDE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDREEKSEN", "WAARDE", enmSQLiteDataType.SQLITEREAL)


        'remove the 'old' system: by date. From now on we will work with timesteps expressed in minutes only
        If Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDREEKSEN", "DATUM") Then

            'we still have an old column present containing date. Convert it to timesteps before removing it
            Dim dtNames As New DataTable, dtDurations As New DataTable, dtRecords As DataTable
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT DISTINCT NAAM FROM RANDREEKSEN", dtNames)
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT DISTINCT DUUR FROM RANDREEKSEN", dtDurations)
            For iName = 0 To dtNames.Rows.Count - 1
                For iDur = 0 To dtDurations.Rows.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Converting old date-records to timestep-records for boundary " & dtNames.Rows(iName)(0) & "...", iName + 1, dtNames.Rows.Count, True)
                    dtRecords = New DataTable
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT DATUM, MINUUT FROM RANDREEKSEN WHERE NAAM='" & dtNames.Rows(iName)(0) & "' AND DUUR=" & dtDurations.Rows(iDur)(0) & " ORDER BY DATUM;", dtRecords, False)
                    Using cmd As New SQLite.SQLiteCommand
                        cmd.Connection = Me.Setup.SqliteCon
                        Using transaction = Me.Setup.SqliteCon.BeginTransaction
                            For iRecord = 0 To dtRecords.Rows.Count - 1
                                'Me.Setup.GeneralFunctions.UpdateProgressBar("", iRecord, dtRecords.Rows.Count - 1)
                                cmd.CommandText = "UPDATE RANDREEKSEN SET MINUUT=" & iRecord * 10 & " WHERE NAAM='" & dtNames(iName)(0) & "' AND DUUR=" & dtDurations(iDur)(0) & " AND DATUM='" & dtRecords(iRecord)(0) & "';"
                                cmd.ExecuteNonQuery()
                            Next
                            transaction.Commit()
                        End Using
                    End Using
                Next
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Conversion done.", 0, 10, True)

            If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDREEKSEN", "DATUM") Then Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.SqliteCon, "RANDREEKSEN", "DATUM")
        End If

        'remove the less old system where every node had its own column. From now on we will work with nodeID's as values in records
        'every column we encounter will be read and its contents will be written to a new row
        'first create a list of nodes, by reading them from the RANDKNOPEN table
        Dim query As String = "SELECT DISTINCT KNOOPID from RANDKNOPEN;"
        Dim dtNodes As New DataTable
        Dim NodeID As String
        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtNodes)
        If dtNodes.Rows.Count > 0 Then
            'now check if our randreeksen table contains columns that have our KNOOPID a header
            For i = 0 To dtNodes.Rows.Count - 1
                NodeID = dtNodes.Rows(i)(0)
                If Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDREEKSEN", NodeID) Then
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Randreeks oude stijl gevonden in de database. Ogenblik geduld terwijl die worden geactualiseerd...", i, dtNodes.Rows.Count, True)

                    'read the entire series for this boundary node
                    Dim dtSeries As New DataTable
                    query = "SELECT NAAM, DUUR, MINUUT," & NodeID & " FROM RANDREEKSEN WHERE " & NodeID & " IS NOT NULL;"
                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtSeries, False)

                    'write this series in the new fashion to our RANDREEKSEN table. Do this in a bulk insert to speed things up a bit
                    Using cmd As New SQLite.SQLiteCommand
                        cmd.Connection = Me.Setup.SqliteCon
                        Using transaction = Me.Setup.SqliteCon.BeginTransaction
                            For j = 0 To dtSeries.Rows.Count - 1
                                cmd.CommandText = "INSERT INTO RANDREEKSEN (KLIMAATSCENARIO, NAAM, NODEID, DUUR, MINUUT, WAARDE) VALUES ('" & Me.Setup.StochastenAnalyse.KlimaatScenario.ToString & "','" & dtSeries.Rows(j)(0) & "','" & NodeID & "'," & dtSeries.Rows(j)(1) & "," & dtSeries.Rows(j)(2) & "," & dtSeries.Rows(j)(3) & ");"
                                cmd.ExecuteNonQuery()
                            Next
                            transaction.Commit() 'this is where the bulk insert is finally executed.
                        End Using
                    End Using

                    'remove the old rows
                    query = "DELETE FROM RANDREEKSEN WHERE " & NodeID & " IS NOT NULL;"
                    Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

                    'and finally drop the column
                    Me.Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDREEKSEN", NodeID)

                End If
            Next
        End If

        'finally we can remove some old rubbish. Every instance where klimaatscenario is empty, waarde is empty or nodeid is empty
        query = "DELETE FROM RANDREEKSEN WHERE (KLIMAATSCENARIO IS NULL OR NAAM IS NULL OR DUUR IS NULL OR MINUUT IS NULL OR NODEID IS NULL OR WAARDE IS NULL);"
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

    End Sub

    Public Sub UpdateRandvoorwaardenTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL RANDVOORWAARDEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table RANDVOORWAARDEN", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "RANDVOORWAARDEN")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "NAAM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "NAAM", enmSQLiteDataType.SQLITETEXT, "RANDVOORWAARDEN_NAAMIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "DUUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "DUUR", enmSQLiteDataType.SQLITEINT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "USE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "USE", enmSQLiteDataType.SQLITEINT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KANS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KANS", enmSQLiteDataType.SQLITEREAL)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KANSCORR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KANSCORR", enmSQLiteDataType.SQLITEREAL)

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("RANDVOORWAARDEN", "KLIMAATSCENARIO")

    End Sub

    Public Sub UpdateWindTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL WIND
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table WIND", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "WIND") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "WIND")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WIND", "NAAM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WIND", "NAAM", enmSQLiteDataType.SQLITETEXT, "WIND_NAAMIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WIND", "DUUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WIND", "DUUR", enmSQLiteDataType.SQLITEINT, "WIND_DUURIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WIND", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WIND", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, "WIND_KLIMAATIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WIND", "USE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WIND", "USE", enmSQLiteDataType.SQLITEINT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WIND", "KANS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WIND", "KANS", enmSQLiteDataType.SQLITEREAL)

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("WIND", "KLIMAATSCENARIO")
    End Sub

    Public Sub UpdateWindreeksenTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL WINDREEKSEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table WINDREEKSEN", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "WINDREEKSEN") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "WINDREEKSEN")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WINDREEKSEN", "NAAM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WINDREEKSEN", "NAAM", enmSQLiteDataType.SQLITETEXT, "WR_NAAMIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WINDREEKSEN", "DUUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WINDREEKSEN", "DUUR", enmSQLiteDataType.SQLITEINT, "WR_DUURIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WINDREEKSEN", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WINDREEKSEN", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WINDREEKSEN", "DATUM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WINDREEKSEN", "DATUM", enmSQLiteDataType.SQLITETEXT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WINDREEKSEN", "GRADEN") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WINDREEKSEN", "GRADEN", enmSQLiteDataType.SQLITEREAL)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "WINDREEKSEN", "SNELHEID") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "WINDREEKSEN", "SNELHEID", enmSQLiteDataType.SQLITEREAL)

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("WINDREEKSEN", "KLIMAATSCENARIO")
    End Sub

    Public Sub UpdateNeerslagverloopTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL NEERSLAGVERLOOP
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table NEERSLAGVERLOOP", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "NEERSLAGVERLOOP") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "NEERSLAGVERLOOP")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "PATROON") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "PATROON", enmSQLiteDataType.SQLITETEXT, "VERLOOP_PATIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "SEIZOEN") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "SEIZOEN", enmSQLiteDataType.SQLITETEXT, "VERLOOP_SEIZOENIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "DUUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "DUUR", enmSQLiteDataType.SQLITEINT, "VERLOOP_DUURIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "UUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "UUR", enmSQLiteDataType.SQLITEINT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "FRACTIE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "NEERSLAGVERLOOP", "FRACTIE", enmSQLiteDataType.SQLITEREAL)
    End Sub

    Public Sub UpdateClimateScenariosTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL KLIMAATSCENARIOS
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table KLIMAATSCENARIOS", progress, 100, True)

        ' Ensure table exists
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "KLIMAATSCENARIOS") Then
            Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "KLIMAATSCENARIOS")
        End If

        ' Ensure NAAM column exists
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "KLIMAATSCENARIOS", "NAAM") Then
            Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "KLIMAATSCENARIOS", "NAAM", enmSQLiteDataType.SQLITETEXT, "CLIMIDX")
        End If

        ' Insert or update the record for STOWA20214_HUIDIG
        Dim sql As String = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_HUIDIG')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        ' Insert or update the record for STOWA20214_HUIDIG
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2033L')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2050L')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2050M')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2050H')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2100L')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2100M')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2100H')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2150L')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2150M')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO KLIMAATSCENARIOS (NAAM) VALUES ('STOWA2024_2150H')"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
    End Sub


    Public Sub UpdateDurationsTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL DUREN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table DUREN", progress, 100, True)

        ' Ensure table exists
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "DUREN") Then
            Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "DUREN")
        End If

        ' Ensure NAAM column exists
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "DUREN", "DUUR") Then
            Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "DUREN", "DUUR", enmSQLiteDataType.SQLITEINT, "DURIDX")
        End If

        Dim sql As String = "INSERT OR REPLACE INTO DUREN (DUUR) VALUES (24)"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO DUREN (DUUR) VALUES (48)"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO DUREN (DUUR) VALUES (96)"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO DUREN (DUUR) VALUES (192)"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
        sql = "INSERT OR REPLACE INTO DUREN (DUUR) VALUES (216)"
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, sql)
    End Sub

    Public Sub UpdateSeasonsTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL SEIZOENEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table SEIZOENEN", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "SEIZOENEN") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "SEIZOENEN")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SEIZOENEN", "SEASON") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SEIZOENEN", "SEASON", enmSQLiteDataType.SQLITETEXT, "SEIZ_SEASONIDX")
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SEIZOENEN", "USE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SEIZOENEN", "USE", enmSQLiteDataType.SQLITEINT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SEIZOENEN", "EVENTSTART") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SEIZOENEN", "EVENTSTART", enmSQLiteDataType.SQLITETEXT)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SEIZOENEN", "KANS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SEIZOENEN", "KANS", enmSQLiteDataType.SQLITEREAL)
        If Not Me.Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "SEIZOENEN", "VOLUME_MULTIPLIER") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "SEIZOENEN", "VOLUME_MULTIPLIER", enmSQLiteDataType.SQLITEREAL)
    End Sub

    Public Sub UpdateVolumesTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL VOLUMES
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table VOLUMES", progress, 100, True)

        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "VOLUMES") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "VOLUMES")

        'add required columns
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, "VOLUMES_CLIMATEIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "SEIZOEN") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "SEIZOEN", enmSQLiteDataType.SQLITETEXT, "VOLUMES_SEASONIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "DUUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "DUUR", enmSQLiteDataType.SQLITEINT, "VOLUMES_DURATIONIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "VOLUME") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "VOLUME", enmSQLiteDataType.SQLITEREAL, "VOLUMES_VOLIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "USE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "USE", enmSQLiteDataType.SQLITEINT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "KANS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "KANS", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "KANSCORR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "KANSCORR", enmSQLiteDataType.SQLITEREAL)

        ''each meteo station must get its own column in the volumes table
        'Dim mt As DataTable = getMeteoStations()
        'Dim i As Integer
        'For i = 0 To mt.Rows.Count - 1
        '    If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", mt.Rows(i)(0)) Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", mt.Rows(i)(0), enmSQLiteDataType.SQLITETEXT)
        'Next

        'delete old columns
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "HUIDIG") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "HUIDIG")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "KL2030") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "KL2030")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "GL2050") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "GL2050") ' Me.Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.sqlitecon, "VOLUMES", "GL2050")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "GH2050") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "GH2050") 'Me.Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.sqlitecon, "VOLUMES", "GH2050")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "WL2050") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "WL2050") 'Me.Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.sqlitecon, "VOLUMES", "WL2050")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "WH2050") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "WH2050") 'Me.Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.sqlitecon, "VOLUMES", "WH2050")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "GL2085") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "GL2085") 'Me.Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.sqlitecon, "VOLUMES", "GL2085")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "GH2085") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "GH2085") 'Me.Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.sqlitecon, "VOLUMES", "GH2085")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "WL2085") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "WL2085") 'Me.Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.sqlitecon, "VOLUMES", "WL2085")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "WH2085") Then UpdateVolumeValues(Me.Setup.SqliteCon, "VOLUMES", "WH2085") 'Me.Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.sqlitecon, "VOLUMES", "WH2085")
        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "VOLUMES", "LOCATIONID") Then Me.Setup.GeneralFunctions.SQLiteDeleteColumn(Me.Setup.SqliteCon, "VOLUMES", "VOLUME")

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("VOLUMES", "KLIMAATSCENARIO")
    End Sub

    Public Sub UpdateVolumeValues(ByRef con As SQLite.SQLiteConnection, TableName As String, ClimateScenarioColumn As String)
        Dim query As String = "Select " & ClimateScenarioColumn & ", SEIZOEN, DUUR, VOLUME, USE, KANS " & "FROM " & TableName & ";"
        Dim Kans As Double, KansCorr As Double
        Dim i As Integer
        Dim nAffected As Integer

        'read these records to a datatable
        Dim dt As New DataTable
        Me.Setup.GeneralFunctions.SQLiteQuery(con, query, dt)

        'remove the old records that involve the current climate scenario
        query = "DELETE FROM " & TableName & " WHERE " & ClimateScenarioColumn & ">0;"
        Me.Setup.GeneralFunctions.SQLiteNoQuery(con, query, False)

        'drop the column
        Me.Setup.GeneralFunctions.SQLiteDropColumn(con, TableName, ClimateScenarioColumn)

        'finally insert the selected records back into the database, in the updated forat
        For i = 0 To dt.Rows.Count - 1
            'insert the record in the new format
            If IsDBNull(dt.Rows(i)(5)) Then KansCorr = 0 Else KansCorr = dt.Rows(i)(5)
            If IsDBNull(dt.Rows(i)(0)) Then Kans = KansCorr Else Kans = dt.Rows(i)(0)
            query = "INSERT INTO " & TableName & " (KLIMAATSCENARIO, SEIZOEN, DUUR, VOLUME, USE, KANS, KANSCORR) VALUES ('" & ClimateScenarioColumn & "','" & dt.Rows(i)(1) & "'," & dt.Rows(i)(2) & "," & dt.Rows(i)(3) & "," & dt.Rows(i)(4) & "," & Kans & "," & KansCorr & ");"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(con, query, False,, nAffected)
        Next

    End Sub


    Public Sub UpdatePatternsTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL PATRONEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table PATRONEN", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "PATRONEN") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "PATRONEN")

        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "PATRONEN", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, "PAT_CLIMATEIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "SEIZOEN") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "PATRONEN", "SEIZOEN", enmSQLiteDataType.SQLITETEXT, "PAT_SEASONIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "DUUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "PATRONEN", "DUUR", enmSQLiteDataType.SQLITEINT, "PAT_DURATIONIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "PATROON") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "PATRONEN", "PATROON", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "USE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "PATRONEN", "USE", enmSQLiteDataType.SQLITEINT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "KANS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "PATRONEN", "KANS", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "KANSCORR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "PATRONEN", "KANSCORR", enmSQLiteDataType.SQLITEREAL)

        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "HUIDIG") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "HUIDIG")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "KL2030") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "KL2030")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "GL2050") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "GL2050")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "GH2050") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "GH2050")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "WL2050") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "WL2050")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "WH2050") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "WH2050")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "GL2085") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "GL2085")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "GH2085") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "GH2085")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "WL2085") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "WL2085")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "PATRONEN", "WH2085") Then UpdatePatternValues(Me.Setup.SqliteCon, "PATRONEN", "WH2085")

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("PATRONEN", "KLIMAATSCENARIO")

    End Sub

    Public Sub UpdateBoundariesTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL RANDVOORWAARDEN
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table RANDVOORWAARDEN", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "RANDVOORWAARDEN")

        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, "BND_SCENARIOIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "SEIZOEN") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "NAAM", enmSQLiteDataType.SQLITETEXT, "BND_NAAMIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "NAAM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "DUUR", enmSQLiteDataType.SQLITEINT, "BND_DUURIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "USE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "USE", enmSQLiteDataType.SQLITEINT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KANS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KANS", enmSQLiteDataType.SQLITEREAL)

        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "HUIDIG") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "HUIDIG")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KL2030") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KL2030")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GL2050") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GL2050")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GH2050") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GH2050")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WL2050") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WL2050")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WH2050") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WH2050")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GL2085") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GL2085")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GH2085") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GH2085")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WL2085") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WL2085")
        'If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WH2085") Then UpdateBoundaryValues(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WH2085")

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("RANDVOORWAARDEN", "KLIMAATSCENARIO")

    End Sub

    Public Sub UpdateGroundwaterTable(progress As Integer)
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL GRONDWATER
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table GRONDWATER", progress, 100, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "GRONDWATER") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "GRONDWATER")

        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, "GW_SCENARIOIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "SEIZOEN") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "SEIZOEN", enmSQLiteDataType.SQLITETEXT, "GW_SEASONIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "NAAM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "NAAM", enmSQLiteDataType.SQLITETEXT, "GW_NAAMIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "USE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "USE", enmSQLiteDataType.SQLITEINT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "FOLDER") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "FOLDER", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "RRFILES") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "RRFILES", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "FLOWFILES") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "FLOWFILES", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "RTCFILES") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "RTCFILES", enmSQLiteDataType.SQLITETEXT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "KANS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "KANS", enmSQLiteDataType.SQLITEREAL)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "BASEDIR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "GRONDWATER", "BASEDIR", enmSQLiteDataType.SQLITEREAL)

        If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "GRONDWATER", "BESTAND") Then
            'old structure (pre 2.205) detected. Copy all values from 'BESTAND' to 'RRFILES'
            Dim query As String = "UPDATE GRONDWATER SET RRFILES = BESTAND;"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
            Me.Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "BESTAND")             'now drop the old column 'BESTAND'
        End If

        'upgrade all old climate scenario names
        Setup.GeneralFunctions.UpgradeClimateScenarioInTables("GRONDWATER", "KLIMAATSCENARIO")

    End Sub

    Public Sub UpdateBoundaryTable()
        '------------------------------------------------------------------------------------
        '               UPDATE TABEL GRONDWATER
        '------------------------------------------------------------------------------------
        Setup.GeneralFunctions.UpdateProgressBar("Updating table RANDVOORWAARDEN", 2, 15, True)
        If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN") Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "RANDVOORWAARDEN")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, "RVW_SCENARIOIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "NAAM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "NAAM", enmSQLiteDataType.SQLITETEXT, "RVW_NAAMIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "DUUR") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "DUUR", enmSQLiteDataType.SQLITEINT, "RVW_DUURIDX")
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "USE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "USE", enmSQLiteDataType.SQLITEINT)
        If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KANS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KANS", enmSQLiteDataType.SQLITEREAL)
    End Sub

    Public Sub UpdateExtraTables()
        Try
            '------------------------------------------------------------------------------------
            '               tabellen EXTRA1, 2, 3 en 4
            '------------------------------------------------------------------------------------
            Dim TableName As String, i As Integer
            Dim query As String
            For i = 1 To 4
                TableName = "EXTRA" & i.ToString.Trim
                Setup.GeneralFunctions.UpdateProgressBar("Updating tables EXTRA" & i, 8 + i, 15, True)
                If Not Setup.GeneralFunctions.SQLiteTableExists(Me.Setup.SqliteCon, TableName) Then Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, TableName)
                If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, TableName, "KLIMAATSCENARIO") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, TableName, "KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT, "EXTRA" & i & "_KLIMAATSCENARIOIDX")
                If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, TableName, "SEIZOEN") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, TableName, "SEIZOEN", enmSQLiteDataType.SQLITETEXT, "EXTRA" & i & "_SEIZOENIDX")
                If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, TableName, "NAAM") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, TableName, "NAAM", enmSQLiteDataType.SQLITETEXT)
                If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, TableName, "USE") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, TableName, "USE", enmSQLiteDataType.SQLITEINT)
                If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, TableName, "KANS") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, TableName, "KANS", enmSQLiteDataType.SQLITEREAL)
                If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, TableName, "RRFILES") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, TableName, "RRFILES", enmSQLiteDataType.SQLITETEXT)
                If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, TableName, "FLOWFILES") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, TableName, "FLOWFILES", enmSQLiteDataType.SQLITETEXT)
                If Not Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, TableName, "RTCFILES") Then Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, TableName, "RTCFILES", enmSQLiteDataType.SQLITETEXT)


                If Setup.GeneralFunctions.SQLiteColumnExists(Me.Setup.SqliteCon, TableName, "BESTAND") Then
                    'v2.3.2
                    'old structure (pre 2.3.2) detected. Copy all values from 'BESTAND' to 'FLOWFILES'
                    query = "UPDATE " & TableName & " SET FLOWFILES = BESTAND;"
                    Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                    Me.Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, TableName, "BESTAND")             'now drop the old column 'BESTAND'
                End If

                'upgrade all old climate scenario names
                Setup.GeneralFunctions.UpgradeClimateScenarioInTables(TableName, "KLIMAATSCENARIO")


            Next
            '------------------------------------------------------------------------------------
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub



    Private Sub SOBEKReadHLocations_Click(sender As Object, e As EventArgs)


    End Sub

    Private Sub BtnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click

        Me.Cursor = Cursors.WaitCursor

        Me.Setup.ExcelFile = New clsExcelBook(Me.Setup)
        Me.Setup.ExcelFile.Initialize(Me.Setup.StochastenAnalyse.ResultsDir & $"\{Me.Setup.StochastenAnalyse.KlimaatScenario.ToString}_{Me.Setup.StochastenAnalyse.Duration}_excedancetables_standardized.xlsx")

        If chk1D.Checked Then
            Call Setup.StochastenAnalyse.ExportResults1D()
        End If

        If chk2D.Checked Then
            Call Setup.StochastenAnalyse.ExportResults2DFromCSV(GeneralFunctions.enm2DParameter.waterlevel)
        End If

        ' Save the Excel file
        Me.Setup.ExcelFile.Save(True)

        'Me.Setup.ExcelFile.Path = Setup.StochastenAnalyse.ResultsDir & "\Herhalingstijden_" & Setup.StochastenAnalyse.KlimaatScenario.ToString & "_" & Setup.StochastenAnalyse.Duration.ToString & ".xlsx"
        'If Me.Setup.ExcelFile.Sheets.Count > 0 Then Me.Setup.ExcelFile.Save(False)
        Me.Setup.Log.write(Setup.StochastenAnalyse.ResultsDir & "\logfile.txt", True)

        Me.Setup.GeneralFunctions.UpdateProgressBar("Export complete.", 0, 10, True)

        Me.Cursor = Cursors.Default

        Me.Setup.Log.write(Setup.StochastenAnalyse.ResultsDir & "\logfile.txt", True)

    End Sub


    Private Sub StochastendirectoriesHernoemenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StochastendirectoriesHernoemenToolStripMenuItem.Click
        Dim myForm As New frmDirRename(Me.Setup)
        myForm.lblRootDir.Text = Setup.StochastenAnalyse.InputFilesDir
        myForm.Show()
    End Sub

    Private Sub MnuGoogleMaps_Click(sender As Object, e As EventArgs) Handles mnuGoogleMaps.Click
        SetMapProvider("Google Maps")
    End Sub

    Private Sub MnuOSM_Click(sender As Object, e As EventArgs) Handles mnuOSM.Click
        SetMapProvider("OSM")
    End Sub

    Private Sub MnuBingMaps_Click(sender As Object, e As EventArgs) Handles mnuBingMaps.Click
        SetMapProvider("Bing Maps")
    End Sub

    Private Sub BingHybridToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles mnuHybrid.Click
        SetMapProvider("Bing Hybrid")
    End Sub

    Private Sub MnuNoMap_Click(sender As Object, e As EventArgs) Handles mnuNoMap.Click
        SetMapProvider("")
    End Sub

    Public Sub SetMapProvider(Provider As String)
        'this routine sets the provider for the background map in the application
        'and stores it for the next time the application is loaded.
        My.Settings.MapProvider = Provider
        My.Settings.Save()

        If Provider = "Google Maps" Then
            'Map.MapProvider = GoogleMapProvider.Instance
            mnuGoogleMaps.Checked = True
            mnuOSM.Checked = False
            mnuBingMaps.Checked = False
            mnuHybrid.Checked = False
            mnuNoMap.Checked = False
        ElseIf Provider = "OSM" Then
            'Map.MapProvider = OpenStreetMapProvider.Instance
            mnuGoogleMaps.Checked = False
            mnuOSM.Checked = True
            mnuBingMaps.Checked = False
            mnuHybrid.Checked = False
            mnuNoMap.Checked = False
        ElseIf Provider = "Bing Maps" Then
            'Map.MapProvider = BingMapProvider.Instance
            mnuGoogleMaps.Checked = False
            mnuOSM.Checked = False
            mnuBingMaps.Checked = True
            mnuHybrid.Checked = False
            mnuNoMap.Checked = False
        ElseIf Provider = "Bing Hybrid" Then
            'Map.MapProvider = BingHybridMapProvider.Instance
            mnuGoogleMaps.Checked = False
            mnuOSM.Checked = False
            mnuBingMaps.Checked = False
            mnuHybrid.Checked = True
            mnuNoMap.Checked = False
        Else
            'Map.MapProvider = EmptyProvider.Instance
            mnuGoogleMaps.Checked = False
            mnuOSM.Checked = False
            mnuBingMaps.Checked = False
            mnuHybrid.Checked = False
            mnuNoMap.Checked = True
        End If
    End Sub

    Public Function CreateModelFromDatagridRow(ByRef Row As DataGridViewRow) As STOCHLIB.clsSimulationModel

        'this function creates an instance of clsSimulationModel, based on a row in the grSimulationModels datagrid
        Dim RRResultsFiles As String = ""
        Dim FlowResultsFiles As String = ""
        Dim RTCResultsFiles As String = ""
        Dim ID As Integer
        Dim ModelType As String = ""
        Dim Exec As String = ""
        Dim Args As String = ""
        Dim ModelDir As String = ""
        Dim Casename As String = ""
        Dim TempWorkDir As String = ""

        '20241002 bugfix: if not casename was given, the temporary work dir was not set
        If Not IsDBNull(Row.Cells(0).Value) Then ID = Row.Cells(0).Value
        If Not IsDBNull(Row.Cells(1).Value) Then ModelType = Row.Cells(1).Value
        If Not IsDBNull(Row.Cells(2).Value) Then Exec = Row.Cells(2).Value
        If Not IsDBNull(Row.Cells(3).Value) Then Args = Row.Cells(3).Value
        If Not IsDBNull(Row.Cells(4).Value) Then ModelDir = Row.Cells(4).Value
        If Not IsDBNull(Row.Cells(5).Value) Then Casename = Row.Cells(5).Value
        If Not IsDBNull(Row.Cells(6).Value) Then TempWorkDir = Row.Cells(6).Value
        If Not IsDBNull(Row.Cells(7).Value) Then RRResultsFiles = Row.Cells(7).Value
        If Not IsDBNull(Row.Cells(8).Value) Then FlowResultsFiles = Row.Cells(8).Value
        If Not IsDBNull(Row.Cells(9).Value) Then RTCResultsFiles = Row.Cells(9).Value

        Dim myModel As New STOCHLIB.clsSimulationModel(Me.Setup, ID, ModelType, Exec, Args, ModelDir, Casename, TempWorkDir, RRResultsFiles, FlowResultsFiles, RTCResultsFiles) ', myRow.Cells(6).Value)
        Return myModel

    End Function

    Private Sub BtnPopulateRuns_Click(sender As Object, e As EventArgs) Handles btnPopulateRuns.Click
        Try
            'clear the log
            Me.Setup.Log.Clear()

            'sluit de databaseconnectie
            If Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Close()
            If Not System.IO.Directory.Exists(Setup.StochastenAnalyse.ResultsDir) Then System.IO.Directory.CreateDirectory(Setup.StochastenAnalyse.ResultsDir)

            Dim i As Long
            Dim myModel As STOCHLIB.clsSimulationModel
            Dim myModule As String 'RR or FLOW'
            Dim CheckSum As Double
            Dim myResultsfile As STOCHLIB.clsResultsFile = Nothing
            Dim myParameter As STOCHLIB.clsResultsFileParameter
            Dim myLocation As STOCHLIB.clsResultsFileLocation

            Setup.GeneralFunctions.UpdateProgressBar("Populating runs...", 0, 10, True)
            Setup.StochastenAnalyse.KlimaatScenario = DirectCast([Enum].Parse(GetType(STOCHLIB.GeneralFunctions.enmKlimaatScenario), cmbClimate.Text), STOCHLIB.GeneralFunctions.enmKlimaatScenario)
            Setup.StochastenAnalyse.Duration = cmbDuration.Text

            '-------------------------------------------------------------------------------------------
            'populate the models to simulate and the corresponding results files, parameters & locations
            '-------------------------------------------------------------------------------------------
            i = -1
            Call Setup.StochastenAnalyse.Models.Clear()
            For Each myRow As DataGridViewRow In grModels.Rows
                i += 1
                Setup.GeneralFunctions.UpdateProgressBar("Reading results from simulation model ", i, grModels.Rows.Count, True)

                myModel = CreateModelFromDatagridRow(myRow)

                Dim iLoc As Integer = 0, nLoc As Integer = grOutputLocations.Rows.Count
                For Each locRow As DataGridViewRow In grOutputLocations.Rows
                    Setup.GeneralFunctions.UpdateProgressBar("", iLoc, nLoc)
                    If CType(locRow.Cells("MODELID").Value, Int16) = myModel.Id Then

                        myModule = DirectCast([Enum].Parse(GetType(STOCHLIB.GeneralFunctions.enmHydroModule), locRow.Cells("MODULE").Value.ToString.Trim.ToUpper), STOCHLIB.GeneralFunctions.enmHydroModule)
                        myResultsfile = myModel.ResultsFiles.GetAdd(locRow.Cells("RESULTSFILE").Value, myModule)
                        myParameter = myResultsfile.GetAddParameter(locRow.Cells("MODELPAR").Value)
                        myLocation = myParameter.GetAddLocation(locRow.Cells("LOCATIEID").Value, locRow.Cells("LOCATIEID").Value, Setup.StochastenAnalyse.Duration)
                        If locRow.Cells("RESULTSTYPE").Value.ToString.Trim.ToUpper = "MIN" Then
                            myLocation.ResultsType = enmHydroMathOperation.MIN
                        ElseIf locRow.Cells("RESULTSTYPE").Value.ToString.Trim.ToUpper = "MAX" Then
                            myLocation.ResultsType = enmHydroMathOperation.MAX
                        ElseIf locRow.Cells("RESULTSTYPE").Value.ToString.Trim.ToUpper = "AVG" Then
                            myLocation.ResultsType = enmHydroMathOperation.AVG
                        End If
                    End If
                Next

                'associate the boundaries with this model instance
                For Each bndRow As DataGridViewRow In grBoundaryNodes.Rows
                    If bndRow.Cells("MODELID").Value = myModel.Id Then
                        myModel.Boundaries.Add(bndRow.Cells("KNOOPID").Value)
                    End If
                Next
                Call Setup.StochastenAnalyse.Models.Add(i, myModel)
            Next

            'close the database
            Me.Setup.SqliteCon.Close()

            '------------------------------------------------------------------------------------
            'populate the stochastic classes and subsequently the runs
            '------------------------------------------------------------------------------------
            Setup.GeneralFunctions.UpdateProgressBar("Populating stochasts...", 3, 10, True)
            If Not Setup.StochastenAnalyse.PopulateFromDataGridView() Then Throw New Exception("Error populating stochasts. Check logfile.")

            Setup.GeneralFunctions.UpdateProgressBar("Populating runs...", 4, 10, True)
            Setup.StochastenAnalyse.Runs.PopulateFromDataGridView(Me.Setup.SqliteCon, grRuns, txtExtraFilesDir.Text)

            Setup.GeneralFunctions.UpdateProgressBar("Calculating checksum...", 5, 10, True)
            CheckSum = Setup.StochastenAnalyse.Runs.calcCheckSum()
            'if volumes are inuse then the checksum should be divided by the duration of the simulation
            If Setup.StochastenAnalyse.VolumesAsFrequencies AndAlso Setup.StochastenAnalyse.Runs.VolumesInUse Then
                CheckSum = CheckSum * Setup.StochastenAnalyse.Duration / (365.25 * 24)
            End If
            CheckSum = Math.Round(CheckSum, 5)
            lblCheckSumRuns.Text = "Checksum=" & CheckSum
            lblnRuns.Text = "Aantal runs=" & Setup.StochastenAnalyse.Runs.Runs.Count

            If CheckSum = 1 Then
                btnUitlezen.Enabled = True
            Else
                MsgBox("Waarschuwing: de som van alle kansen is ongelijk aan 1! De knop 'Uitlezen' is daarom gedeactiveerd.")
                btnUitlezen.Enabled = False
            End If

            'refresh the grid me.Setup.containing the runs (reads which runs have already been run)
            Setup.GeneralFunctions.UpdateProgressBar("Rebuilding the runs grid...", 6, 10, True)
            Call Setup.StochastenAnalyse.RefreshRunsGrid(grRuns, btnPostprocessing)

            'finally enable the buttons that are allowed to be enabled, given the availability of results
            EnableButtons(True)

            Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10, True)

        Catch ex As Exception
            Me.Setup.Log.write(Setup.Settings.RootDir & "\logfile.txt", True)
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub HighlightSelectedMarker()
        'ActiveMarker.ChangeColor(Color.Yellow, Color.Yellow)
    End Sub

    'Private Sub Gmap_OnMarkerClick(item As GMapMarkerCircle, e As MouseEventArgs) Handles Map.OnMarkerClick
    '    '============================================================================================
    '    'this routine responds on the user clicking a location on the map in combination with a valid
    '    'combination of stochasts
    '    '============================================================================================

    '    Try
    '        ActiveMarker = item
    '        HighlightSelectedMarker()
    '        If Not ActiveRunRef = "" Then
    '            Call PlotTimeSeries()
    '        Else
    '            Throw New Exception("Selecteer eerst een geldige combinatie van stochasten.")
    '        End If

    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '    End Try
    'End Sub


    Private Sub ReferentiepeilenUitShapefileToevoegenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReferentiepeilenUitShapefileToevoegenToolStripMenuItem.Click
        Dim myForm As New frmAddLevelsFromShapefile(Me.Setup, Me.Setup.SqliteCon)
        myForm.Show()
    End Sub

    Private Sub UpgradeNaarToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UpgradeNaarToolStripMenuItem.Click
        'deze routine upgradet een database om hem geschikt te maken voor versie 5.0 van De Nieuwe Stochastentool

        Try
            dlgOpenFile.Filter = "Access Database|*.mdb"
            dlgOpenFile.ShowDialog()
            dlgSaveFile.Filter = "Access Database|*.mdb"
            dlgSaveFile.ShowDialog()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Copying database...", 0, 10)
            'If System.IO.File.Exists(dlgOpenFile.FileName) Then System.IO.File.Copy(dlgOpenFile.FileName, dlgSaveFile.FileName)
            If System.IO.File.Exists(dlgOpenFile.FileName) Then FileSystem.CopyFile(dlgOpenFile.FileName, dlgSaveFile.FileName, UIOption.AllDialogs)

            Me.Setup.SqliteCon = New SQLite.SQLiteConnection With {
            .ConnectionString = "Data Source=" & dlgSaveFile.FileName & ";Version=3;"
                }

            'drop the table RESULTATENDETAIL (we won't be using it any longer)
            Me.Setup.GeneralFunctions.SQLiteDropTable(Me.Setup.SqliteCon, "RESULTATENDETAIL")

            Me.Setup.GeneralFunctions.UpdateProgressBar("Upgrading seasons table...", 0, 6)
            Call UpgradeSeasons()   'upgrades the table structure for seasons to database version 5.0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Upgrading volumes table...", 1, 6)
            Call UpgradeVolumes()   'upgrades the table structure for volumes to database version 5.0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Upgrading patterns table...", 2, 6)
            Call UpgradePatterns() 'upgrades the table structure for patterns to database version 5.0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Upgrading groundwater table...", 3, 6)
            Call UpgradeGroundwater() 'upgrades the table structure for groundwater to database version 5.0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Upgrading boundaries table...", 4, 6)
            Call UpgradeBoundaries() 'upgrades the table structure for boundaries to database version 5.0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Upgrading extra tables...", 5, 6)
            Call UpgradeExtra() 'upgrades the table structure for stochast extra to database version 5.0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Upgrading volumes table...", 6, 6)
            Call UpgradeVolumes()   'upgrades the table structure for seasons to database version 5.0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)


        Catch ex As Exception

        End Try

    End Sub

    Public Sub UpgradeSeasons()
        'upgrades the database table me.Setup.containing Seasons for use in version 5.0
        Try
            Dim query As String
            Dim i As Long, j As Long
            Dim StartDate As Date

            Call UpdateSeasonsTable(0)    'voegt alle benodigde nieuwe kolommen toe

            'first check in the Patterns table which unique seasons exist
            'for each unique season, create a new row in SEIZOENEN
            query = "SELECT DISTINCT SEIZOEN FROM PATRONEN;"
            Dim dt As New DataTable
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt, False)

            'now check if these seasons already exist in SEIZOENEN
            query = "SELECT DISTINCT SEASON FROM SEIZOENEN;"
            Dim dtExisting As New DataTable
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtExisting, False)

            For i = 0 To dt.Rows.Count - 1

                Dim Found As Boolean = False
                For j = 0 To dtExisting.Rows.Count - 1
                    If dt.Rows(i)("SEIZOEN") = dt.Rows(j)("SEIZOEN") Then
                        Found = True
                        Exit For
                    End If
                Next

                If Not Found Then
                    If InStr(1, dt.Rows(i)("SEIZOEN"), "zomer", CompareMethod.Text) > 0 Then
                        StartDate = New Date(2000, 6, 1)
                    ElseIf InStr(1, dt.Rows(i)("SEIZOEN"), "mrt-okt", CompareMethod.Text) > 0 Then
                        StartDate = New Date(2000, 6, 1)
                    ElseIf InStr(1, dt.Rows(i)("SEIZOEN"), "summer", CompareMethod.Text) > 0 Then
                        StartDate = New Date(2000, 6, 1)
                    ElseIf InStr(1, dt.Rows(i)("SEIZOEN"), "winter", CompareMethod.Text) > 0 Then
                        StartDate = New Date(2000, 1, 1)
                    ElseIf InStr(1, dt.Rows(i)("SEIZOEN"), "nov-feb", CompareMethod.Text) > 0 Then
                        StartDate = New Date(2000, 1, 1)
                    ElseIf InStr(1, dt.Rows(i)("SEIZOEN"), "", CompareMethod.Text) > 0 Then
                        StartDate = New Date(2000, 1, 1)
                    Else
                        StartDate = New Date(2000, 1, 1)
                    End If
                    query = "INSERT INTO SEIZOENEN (SEASON, USE, EVENTSTART, KANS, VOLUME_MULTIPLIER) VALUES (""" & dt.Rows(i)("SEIZOEN") & """," & False & ",'" & StartDate & "',0.5,1);"
                    Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                End If
            Next

            Me.Setup.SqliteCon.Close()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub UpgradeVolumes()
        'upgrades the database table me.Setup.containing Volumes for use in version 5.0
        Try
            Dim curClimate As String = ""
            Dim query As String
            Dim i As Long, j As Long
            Dim kans As Double

            Call UpdateVolumesTable(0) 'voegt de benodigde kolommen toe

            'verplaats de kansen die horen bij elk klimaatscenario naar een nieuwe rij en KANS-kolom
            dtVolume = New DataTable
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT * FROM VOLUMES;", dtVolume)
            For i = 0 To dtVolume.Rows.Count - 1
                For j = 1 To 10
                    Select Case j
                        Case Is = 1
                            curClimate = "HUIDIG"
                        Case Is = 2
                            curClimate = "KL2030"
                        Case Is = 3
                            curClimate = "GL2050"
                        Case Is = 4
                            curClimate = "GH2050"
                        Case Is = 5
                            curClimate = "WL2050"
                        Case Is = 6
                            curClimate = "WH2050"
                        Case Is = 7
                            curClimate = "GL2085"
                        Case Is = 8
                            curClimate = "GH2085"
                        Case Is = 9
                            curClimate = "WL2085"
                        Case Is = 10
                            curClimate = "WH2085"
                    End Select
                    If dtVolume.Columns.Contains(curClimate) Then
                        If Not IsDBNull(dtVolume.Rows(i)(curClimate)) Then
                            kans = dtVolume.Rows(i)(curClimate)
                        Else
                            kans = 0
                        End If
                        query = "INSERT INTO VOLUMES (KLIMAATSCENARIO, SEIZOEN, DUUR, VOLUME, USE, KANS, KANSCORR) VALUES ('" & curClimate & "','" & dtVolume.Rows(i)("SEIZOEN") & "'," & dtVolume.Rows(i)("DUUR") & "," & dtVolume.Rows(i)("VOLUME") & "," & dtVolume.Rows(i)("USE") & "," & dtVolume.Rows(i)(curClimate) & ",0);"
                        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                    End If
                Next
            Next

            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "HUIDIG")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "KL2030")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "GL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "GH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "WL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "WH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "GL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "GH2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "WL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "VOLUMES", "WH2085")

            query = "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO IS NULL;"
            Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, True)
            Me.Setup.SqliteCon.Close()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub UpgradePatterns()
        'upgrades the database table me.Setup.containing Patterns for use in version 5.0
        Try
            Dim curClimate As String = ""
            Dim query As String
            Dim i As Long, j As Long
            Dim kans As Double

            Call UpdatePatternsTable(0) 'voegt de benodigde kolommen toe

            'verplaats de kansen die horen bij elk klimaatscenario naar een nieuwe rij en KANS-kolom
            dtPattern = New DataTable
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT * FROM PATRONEN;", dtPattern)
            For i = 0 To dtPattern.Rows.Count - 1
                For j = 1 To 10
                    Select Case j
                        Case Is = 1
                            curClimate = "HUIDIG"
                        Case Is = 2
                            curClimate = "KL2030"
                        Case Is = 3
                            curClimate = "GL2050"
                        Case Is = 4
                            curClimate = "GH2050"
                        Case Is = 5
                            curClimate = "WL2050"
                        Case Is = 6
                            curClimate = "WH2050"
                        Case Is = 7
                            curClimate = "GL2085"
                        Case Is = 8
                            curClimate = "GH2085"
                        Case Is = 9
                            curClimate = "WL2085"
                        Case Is = 10
                            curClimate = "WH2085"
                    End Select
                    If dtPattern.Columns.Contains(curClimate) Then
                        If Not IsDBNull(dtPattern.Rows(i)(curClimate)) Then
                            kans = dtPattern.Rows(i)(curClimate)
                        Else
                            kans = 0
                        End If
                        query = "INSERT INTO PATRONEN (KLIMAATSCENARIO, SEIZOEN, DUUR, PATROON, USE, KANS, KANSCORR) VALUES ('" & curClimate & "','" & dtPattern.Rows(i)("SEIZOEN") & "'," & dtPattern.Rows(i)("DUUR") & ",'" & dtPattern.Rows(i)("PATROON") & "'," & dtPattern.Rows(i)("USE") & "," & kans & ",0);"
                        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                    End If
                Next
            Next

            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "HUIDIG")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "KL2030")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "GL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "GH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "WL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "WH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "GL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "GH2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "WL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "PATRONEN", "WH2085")

            query = "DELETE FROM PATRONEN WHERE KLIMAATSCENARIO IS NULL;"
            Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, True)
            Me.Setup.SqliteCon.Close()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub UpgradeGroundwater()
        'upgrades the database table me.Setup.containing Patterns for use in version 5.0
        Try
            Dim curClimate As String = ""
            Dim query As String
            Dim i As Long, j As Long
            Dim kans As Double

            Call UpdateGroundwaterTable(0) 'voegt de benodigde kolommen toe

            dtGW = New DataTable
            query = "SELECT * FROM GRONDWATER;"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtGW)

            For i = 0 To dtGW.Rows.Count - 1
                For j = 1 To 10
                    Select Case j
                        Case Is = 1
                            curClimate = "HUIDIG"
                        Case Is = 2
                            curClimate = "KL2030"
                        Case Is = 3
                            curClimate = "GL2050"
                        Case Is = 4
                            curClimate = "GH2050"
                        Case Is = 5
                            curClimate = "WL2050"
                        Case Is = 6
                            curClimate = "WH2050"
                        Case Is = 7
                            curClimate = "GL2085"
                        Case Is = 8
                            curClimate = "GH2085"
                        Case Is = 9
                            curClimate = "WL2085"
                        Case Is = 10
                            curClimate = "WH2085"
                    End Select

                    If dtGW.Columns.Contains(curClimate) Then
                        If Not IsDBNull(dtGW.Rows(i)(curClimate)) Then
                            kans = dtGW.Rows(i)(curClimate)
                        Else
                            kans = 0
                        End If
                        query = "INSERT INTO GRONDWATER (KLIMAATSCENARIO, SEIZOEN, NAAM, USE, FOLDER, RRFILES, FLOWFILES, RTCFILES, KANS) VALUES ('" & curClimate & "','" & dtGW.Rows(i)("SEIZOEN") & "','" & dtGW.Rows(i)("NAAM") & "'," & dtGW.Rows(i)("USE") & ",'" & dtGW.Rows(i)("FOLDER") & "','" & dtGW.Rows(i)("RRFILES") & "','" & dtGW.Rows(i)("FLOWFILES") & "','" & dtGW.Rows(i)("RTCFILES") & "'," & kans & ");"
                        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                    End If
                Next
            Next

            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "HUIDIG")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "KL2030")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "GL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "GH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "WL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "WH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "GL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "GH2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "WL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "GRONDWATER", "WH2085")

            query = "DELETE FROM GRONDWATER WHERE KLIMAATSCENARIO IS NULL;"
            Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, True)
            Me.Setup.SqliteCon.Close()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub


    Public Sub UpgradeBoundaries()
        'upgrades the database table me.Setup.containing Patterns for use in version 5.0
        Try
            Dim curClimate As String = ""
            Dim query As String
            Dim i As Long, j As Long
            Dim kans As Double
            Call UpdateBoundaryTable() 'voegt de benodigde kolommen toe

            dtBoundary = New DataTable
            query = "SELECT * FROM RANDVOORWAARDEN;"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtBoundary)

            For i = 0 To dtBoundary.Rows.Count - 1
                For j = 1 To 10
                    Select Case j
                        Case Is = 1
                            curClimate = "HUIDIG"
                        Case Is = 2
                            curClimate = "KL2030"
                        Case Is = 3
                            curClimate = "GL2050"
                        Case Is = 4
                            curClimate = "GH2050"
                        Case Is = 5
                            curClimate = "WL2050"
                        Case Is = 6
                            curClimate = "WH2050"
                        Case Is = 7
                            curClimate = "GL2085"
                        Case Is = 8
                            curClimate = "GH2085"
                        Case Is = 9
                            curClimate = "WL2085"
                        Case Is = 10
                            curClimate = "WH2085"
                    End Select

                    If dtBoundary.Columns.Contains(curClimate) Then
                        If Not IsDBNull(dtBoundary.Rows(i)(curClimate)) Then
                            kans = dtBoundary.Rows(i)(curClimate)
                        Else
                            kans = 0
                        End If
                        query = "INSERT INTO RANDVOORWAARDEN (KLIMAATSCENARIO, NAAM, DUUR, USE, KANS) VALUES ('" & curClimate & "','" & dtBoundary.Rows(i)("NAAM") & "'," & dtBoundary.Rows(i)("DUUR") & "," & dtBoundary.Rows(i)("USE") & "," & kans & ");"
                        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                    End If

                Next
            Next

            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "HUIDIG")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "KL2030")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "GH2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "RANDVOORWAARDEN", "WH2085")

            query = "DELETE FROM RANDVOORWAARDEN WHERE KLIMAATSCENARIO IS NULL;"
            Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, True)
            Me.Setup.SqliteCon.Close()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub


    Public Sub UpgradeExtra()
        'upgrades the database table me.Setup.containing Extra stochasts for use in version 5.0
        Try
            Dim curClimate As String = ""
            Dim query As String
            Dim i As Long, j As Long
            Dim kans As Double

            'first rename the previous table EXTRA to EXTRA1
            Setup.GeneralFunctions.SqliteRenameTable(Me.Setup.SqliteCon, "EXTRA", "EXTRA1")

            'then update the columns for all EXTRA* tables
            Call UpdateExtraTables()

            'finally only edit/upgrade the table EXTRA1 since in previous versions there was no EXTRA2 trough EXTRA4 table anyway
            dtExtra1 = New DataTable
            query = "SELECT * FROM EXTRA1;"
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtExtra1)

            For i = 0 To dtExtra1.Rows.Count - 1
                For j = 1 To 10
                    Select Case j
                        Case Is = 1
                            curClimate = "HUIDIG"
                        Case Is = 2
                            curClimate = "KL2030"
                        Case Is = 3
                            curClimate = "GL2050"
                        Case Is = 4
                            curClimate = "GH2050"
                        Case Is = 5
                            curClimate = "WL2050"
                        Case Is = 6
                            curClimate = "WH2050"
                        Case Is = 7
                            curClimate = "GL2085"
                        Case Is = 8
                            curClimate = "GH2085"
                        Case Is = 9
                            curClimate = "WL2085"
                        Case Is = 10
                            curClimate = "WH2085"
                    End Select


                    'next we'll add a row for each season that exists the current climate scenario
                    If dtExtra1.Columns.Contains(curClimate) Then
                        If Not IsDBNull(dtExtra1.Rows(i)(curClimate)) Then
                            kans = dtExtra1.Rows(i)(curClimate)
                        Else
                            kans = 0
                        End If
                        query = "INSERT INTO EXTRA1 (KLIMAATSCENARIO, SEIZOEN, NAAM, USE, KANS) VALUES ('" & curClimate & "','" & dtExtra1.Rows(i)("SEIZOEN") & "','" & dtExtra1.Rows(i)("NAAM") & "'," & dtExtra1.Rows(i)("USE") & "," & kans & ");"
                        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                    End If

                Next
            Next

            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "HUIDIG")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "KL2030")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "GL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "GH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "WL2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "WH2050")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "GL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "GH2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "WL2085")
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, "EXTRA1", "WH2085")
            query = "DELETE FROM EXTRA1 WHERE KLIMAATSCENARIO IS NULL;"
            Setup.GeneralFunctions.SQLiteDropColumn(Me.Setup.SqliteCon, query, True)
            Me.Setup.SqliteCon.Close()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub



    Private Sub PolygonenUitShapefileToevoegenToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles PolygonenUitShapefileToevoegenToolStripMenuItem1.Click
        Try
            dlgOpenFile.Filter = "ESRI Shapefile|*.shp"
            dlgOpenFile.ShowDialog()
            If System.IO.File.Exists(dlgOpenFile.FileName) Then
                Dim query As String = "DELETE FROM POLYGONS;"
                Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, True)
                Setup.GeneralFunctions.ShapefileToDatabase(Me.Setup.SqliteCon, dlgOpenFile.FileName, "POLYGONS", "SHAPEIDX", "POINTIDX", "LON", "LAT", True)
            End If
        Catch ex As Exception
            Me.Setup.Log.write(Setup.Settings.RootDir & "\logfile.txt", True)
        End Try

    End Sub

    Private Sub VolumesUitCSVToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VolumesUitCSVToolStripMenuItem.Click
        Dim myFields As New Dictionary(Of String, STOCHLIB.clsDataField) From {
        {"DUUR", New STOCHLIB.clsDataField("DUUR", enmSQLiteDataType.SQLITEINT)},
        {"KANS", New STOCHLIB.clsDataField("KANS", enmSQLiteDataType.SQLITEREAL)},
        {"KANSCORR", New STOCHLIB.clsDataField("KANSCORR", enmSQLiteDataType.SQLITEREAL)},
        {"KLIMAATSCENARIO", New STOCHLIB.clsDataField("KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT)},
        {"SEIZOEN", New STOCHLIB.clsDataField("SEIZOEN", enmSQLiteDataType.SQLITETEXT)},
        {"USE", New STOCHLIB.clsDataField("USE", enmSQLiteDataType.SQLITEINT)},
        {"VOLUME", New STOCHLIB.clsDataField("VOLUME", enmSQLiteDataType.SQLITEREAL)}
        }
        Dim myForm As New STOCHLIB.frmTextFileToSQLite(Me.Setup, Me.Setup.SqliteCon, "VOLUMES", myFields)
        Dim res As DialogResult = myForm.ShowDialog()
        If res = DialogResult.OK Then
            UpdateVolumesTable(0)
        End If

    End Sub

    Public Function AddReferenceLevelToChart(ByRef refLevel As Double, ByVal ColName As String, ByVal SeriesName As String, ByRef dt As DataTable) As Boolean
        Try
            Dim i As Integer
            dt.Columns.Add(New DataColumn(ColName))
            For i = 0 To dt.Rows.Count - 1
                dt.Rows(i)(ColName) = refLevel
            Next

            'chartLocation.Series.Add(SeriesName)
            'SeriesIdx = chartLocation.Series.Count - 1
            'chartLocation.Series(SeriesIdx).XValueMember = "Uren"
            'chartLocation.Series(SeriesIdx).YValueMembers = ColName
            'chartLocation.Series(SeriesIdx).ChartType = DataVisualization.Charting.SeriesChartType.Line
            'chartLocation.Series(SeriesIdx).BorderWidth = 3

            Return True
        Catch ex As Exception
            MsgBox(ex.Message)
            Return False
        End Try
    End Function

    Private Sub BtnAddSeason_Click(sender As Object, e As EventArgs) Handles btnAddSeason.Click
        'since the datagridview is data-bound, we can only add a row by adding it to the database
        'after that refresh the seasons grids
        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "INSERT INTO SEIZOENEN Default VALUES;", True)

        'rebuild all grids from the database
        Call BuildStochasticGrids()
    End Sub

    Private Sub GrSeizoenen_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles grSeizoenen.CellValueChanged
        'If grSeizoenen.Columns(e.ColumnIndex).Name = "USE" OrElse grSeizoenen.Columns(e.ColumnIndex).Name = "KANS" Then
        'End If

        'only update if all columns have been filled in
        Dim Update As Boolean = True
        Dim c As Integer
        For c = 0 To grSeizoenen.Columns.Count - 1
            If IsDBNull(grSeizoenen.Rows(e.RowIndex).Cells(c).Value) Then Update = False
        Next

        If Update Then
            'update the seizoenen table in the database
            Call UpdateSeasons()

            'rebuild all grids!
            Call BuildStochasticGrids()
        End If

    End Sub

    Public Sub UpdateSeasons()
        'this routine updates the Seizoenen table in the database
        Dim query As String, myRow As DataGridViewRow
        Dim EventDate As String, Kans As Double, Volume_Multiplier As Double = 1
        query = "DELETE FROM SEIZOENEN;"
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

        For Each myRow In grSeizoenen.Rows
            If Not IsDBNull(myRow.Cells("EVENTSTART").Value) Then
                EventDate = myRow.Cells("EVENTSTART").Value
            Else
                EventDate = Format(New Date(2000, 1, 1), "yyyy-MM-dd")
            End If
            If IsDBNull(myRow.Cells("KANS").Value) Then
                Kans = 0
            Else
                Kans = myRow.Cells("KANS").Value
            End If

            If IsDBNull(myRow.Cells("VOLUME_MULTIPLIER").Value) Then
                Volume_Multiplier = 1
            Else
                Volume_Multiplier = myRow.Cells("VOLUME_MULTIPLIER").Value
            End If

            query = "INSERT INTO SEIZOENEN (SEASON, USE, EVENTSTART, KANS, VOLUME_MULTIPLIER) VALUES ('" & myRow.Cells("SEASON").Value & "'," & myRow.Cells("USE").Value & ",'" & EventDate & "'," & Kans & "," & Volume_Multiplier & ");"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
        Next
        Me.Setup.SqliteCon.Close()
    End Sub

    'Private Sub WaterstandspuntenImporterenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles WaterstandspuntenImporterenToolStripMenuItem.Click
    '    Try
    '        'this routine expands the database table OUTPUTLOCATIONS with the SOBEK H points
    '        Dim dtModels As New DataTable, dtLocations As New DataTable, i As Integer, j As Integer
    '        Dim query As String, ModelID As String
    '        Dim ModelDir As String, CaseName As String
    '        Dim Lat As Double, Lon As Double
    '        Dim WLPoints As New Dictionary(Of String, STOCHLIB.clsSbkVectorPoint)
    '        Dim WLPoint As STOCHLIB.clsSbkVectorPoint

    '        If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

    '        'first get the list of simulation models
    '        query = "Select * FROM SIMULATIONMODELS;"
    '        Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtModels, False)

    '        'read the subcatchment's shapefile and set the 'beginpointinshapefile' property to allow quick search of target levels
    '        Dim SubcatchmentsSF As New STOCHLIB.clsShapeFile(Me.Setup, txtPeilgebieden.Text)
    '        If Not SubcatchmentsSF.Open() Then Throw New Exception("Error: could not open subcatchments shapefile: " & SubcatchmentsSF.Path)
    '        Dim WPFieldIdx As Integer = SubcatchmentsSF.GetFieldIdx(cmbWinterpeil.Text)
    '        Dim ZPFieldIdx As Integer = SubcatchmentsSF.GetFieldIdx(cmbZomerpeil.Text)
    '        SubcatchmentsSF.sf.BeginPointInShapefile()

    '        For i = 0 To dtModels.Rows.Count - 1
    '            If dtModels.Rows(i)("MODELTYPE") = "SOBEK" Then
    '                ModelID = dtModels.Rows(i)("MODELID")
    '                ModelDir = dtModels.Rows(i)("MODELDIR")
    '                CaseName = dtModels.Rows(i)("CASENAME")

    '                'clean up existing data for the current simulation model
    '                '@siebe: later refine this by distinguishing parameters
    '                query = "DELETE FROM OUTPUTLOCATIONS WHERE MODELID='" & ModelID & "';"
    '                Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

    '                'read the model schematization
    '                If Not Setup.SOBEKData.ReadProject(ModelDir, True) Then Throw New Exception("SOBEK Project could Not be read: " & ModelDir)
    '                If Not Setup.SetAddSobekProject(ModelDir, True, True) Then Throw New Exception("Error adding SOBEK project " & ModelDir)
    '                If Not Setup.SOBEKData.ActiveProject.SetActiveCase(CaseName) Then Throw New Exception("Error setting sobek case as active: " & CaseName)
    '                If Not Setup.SOBEKData.ActiveProject.ActiveCase.Read(False, True, False, False, False, "") Then Throw New Exception("Error reading active SOBEK case: " & CaseName)
    '                If Not Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.ReadCalculationPointsByReach() Then Throw New Exception("Error reading calculation points.")

    '                WLPoints = New Dictionary(Of String, STOCHLIB.clsSbkVectorPoint)
    '                WLPoints = Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.CollectAllWaterLevelPoints
    '                Me.Setup.GeneralFunctions.UpdateProgressBar("Writing points to database...", 0, 10, True)

    '                For j = 0 To WLPoints.Count - 1
    '                    Me.Setup.GeneralFunctions.UpdateProgressBar("", j, WLPoints.Count)
    '                    WLPoint = WLPoints.Values(j)

    '                    'compute the map coordinates of our point
    '                    Setup.GeneralFunctions.RD2WGS84(WLPoint.X, WLPoint.Y, Lat, Lon)

    '                    'retrieve the target levels from our subcatchments shapefile
    '                    Dim ShapeIdx As Integer, WP As Double, ZP As Double
    '                    SubcatchmentsSF.Open()
    '                    SubcatchmentsSF.sf.BeginPointInShapefile()
    '                    SubcatchmentsSF.sf.PointInShape(ShapeIdx, WLPoint.X, WLPoint.Y)
    '                    WP = SubcatchmentsSF.sf.CellValue(WPFieldIdx, ShapeIdx)
    '                    ZP = SubcatchmentsSF.sf.CellValue(ZPFieldIdx, ShapeIdx)

    '                    'insert our location and its parameter in the OUTPUTLOCATIONS table
    '                    query = "INSERT INTO OUTPUTLOCATIONS (LOCATIEID, LOCATIENAAM, MODELID, MODELPAR, RESULTSFILE, X, Y, LAT, LON, ZP, WP) VALUES ('" & WLPoint.ID & "','" & WLPoint.ID & "','" & ModelID & "','" & "Water" & "','calcpnt.his'," & WLPoint.X & "," & WLPoint.Y & "," & Lat & "," & Lon & "," & ZP & "," & WP & ");"
    '                    Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
    '                Next

    '            End If
    '        Next

    '        SubcatchmentsSF.sf.EndPointInShapefile()
    '        SubcatchmentsSF.Close()

    '        'now that we have read all output locations, refresh the datagridview containing our output locations
    '        PopulateOutputLocationsGrid()

    '        Me.Setup.SqliteCon.Close()
    '        Setup.GeneralFunctions.UpdateProgressBar("klaar", 0, 1)
    '        Me.Setup.Log.write(Setup.Settings.RootDir & "\logfile.txt", True)
    '        Me.Setup.Log.Clear()
    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '    End Try

    'End Sub

    Private Sub UitvoerlocatiesImporterenToolStripMenuItem_Click(sender As Object, e As EventArgs)
    End Sub

    Private Sub TrPatroon_MouseHover(sender As Object, e As EventArgs)
        ShowPattern()
    End Sub

    Private Sub btnShapefile_Click(sender As Object, e As EventArgs) Handles btnShapefile.Click
        dlgOpenFile.Filter = "ESRI Shapefile|*.shp"
        dlgOpenFile.ShowDialog()
        txtPeilgebieden.Text = dlgOpenFile.FileName
        Me.Setup.GeneralFunctions.PopulateComboBoxShapeFields(txtPeilgebieden.Text, cmbWinterpeil, enmErrorLevel._Message)
        Me.Setup.GeneralFunctions.PopulateComboBoxShapeFields(txtPeilgebieden.Text, cmbZomerpeil, enmErrorLevel._Message)
    End Sub

    Private Sub SaveXMLToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles SaveXMLToolStripMenuItem1.Click
        Dim attrList As New List(Of String)
        Dim valsList As New List(Of String)

        Dim InputPathRelative As String = ""
        Dim OutputPathRelative As String = ""
        Dim ResultsPathRelative As String = ""
        Dim ExtraFilesPathRelative As String = ""
        Dim DatabasePathRelative As String = ""
        Dim SubcatchmentsPathRelative As String = ""
        Dim RootDir As String


        dlgSaveFile.Filter = "XML file|*.xml"


        Dim Result As DialogResult = dlgSaveFile.ShowDialog()
        If Result = DialogResult.OK Then

            'write all paths as relative to the XML
            RootDir = Me.Setup.GeneralFunctions.DirFromFileName(dlgSaveFile.FileName)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(RootDir, txtInputDir.Text, InputPathRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(RootDir, txtOutputDir.Text, OutputPathRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(RootDir, txtResultatenDir.Text, ResultsPathRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(RootDir, txtExtraFilesDir.Text, ExtraFilesPathRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(RootDir, txtDatabase.Text, DatabasePathRelative)
            Me.Setup.GeneralFunctions.AbsoluteToRelativePath(RootDir, txtPeilgebieden.Text, SubcatchmentsPathRelative)

            Using xmlWriter As New StreamWriter(dlgSaveFile.FileName)
                xmlWriter.WriteLine("<stochastentool>")
                xmlWriter.WriteLine("  <!--************************************************************************************************-->")
                xmlWriter.WriteLine("  <!--In dit xml-bestand configureert u de stochastentool van Hydroconsult.***************************-->")
                xmlWriter.WriteLine("  <!--Copyright Hydroconsult, 2014********************************************************************-->")
                xmlWriter.WriteLine("  <!--Enkele algemene wenken:geef bij voorkeur relatieve paden op. Paden zijn t.o.v. dit xml document -->")
                xmlWriter.WriteLine("  <!--Dit maakt het makkelijke om sommen over verschillende machines te verdelen**********************-->")
                xmlWriter.WriteLine("  <!--************************************************************************************************-->")
                xmlWriter.WriteLine("  <instellingen>")
                xmlWriter.WriteLine("	<!--directory voor de resultaten en maximum aantal parallelle berekeningen-->")
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "invoermap", InputPathRelative, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "uitvoermap", OutputPathRelative, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "resultatenmap", ResultsPathRelative, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "extramodelinputmap", ExtraFilesPathRelative, 4)

                Dim myList As New List(Of String)
                Dim myVals As New List(Of String)
                myList.Add("Flow1D")
                myList.Add("Flow2D")
                myVals.Add(chk1D.Checked)
                myVals.Add(chk2D.Checked)
                Me.Setup.GeneralFunctions.writeXMLElementWithAttributes(xmlWriter, "resultatenmodules", 4, myList, myVals)

                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "maxparallel", txtMaxParallel.Text, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "klimaatscenario", cmbClimate.Text, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "duur", cmbDuration.Text, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "uitloop", txtUitloop.Text, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "stochastenconfigfile", DatabasePathRelative, 4)

                myList = New List(Of String)
                myVals = New List(Of String)
                myList.Add("pad")
                myList.Add("winterpeilveld")
                myList.Add("zomerpeilveld")
                myVals.Add(SubcatchmentsPathRelative)
                myVals.Add(cmbWinterpeil.Text)
                myVals.Add(cmbZomerpeil.Text)
                Me.Setup.GeneralFunctions.writeXMLElementWithAttributes(xmlWriter, "peilgebieden", 4, myList, myVals)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "resultatengecrashtesommentoestaan", chkUseCrashedResults.Checked, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "simulatieverwijderennakopierenresultaten", chkRemoveSimulationAfterCopyingResultsFiles.Checked, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "leesresultatenvanafpercentage", txtResultsStartPercentage.Text, 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "volumesalsfrequenties", "TRUE", 4)
                Me.Setup.GeneralFunctions.writeXMLElement(xmlWriter, "bestaanderesultatenvervangen", "FALSE", 4)
                xmlWriter.WriteLine("  </instellingen>")
                'v2.2.2 removed storing model data in the XML
                'xmlWriter.WriteLine("  <modellen>")
                'xmlWriter.WriteLine("		<!--ondersteuning voor modeltypen SOBEK, DIMR, DHYDROSERVER en Custom-->")

                'attrList = New List(Of String)
                'attrList.Add("id")
                'attrList.Add("type")
                'attrList.Add("executable")
                'attrList.Add("arguments")
                'attrList.Add("modeldir")
                'attrList.Add("casename")
                'attrList.Add("tempworkdir")
                'attrList.Add("resultsfiles_rr")
                'attrList.Add("resultsfiles_flow")
                'For Each myRow As DataGridViewRow In grModels.Rows
                '    valsList = New List(Of String)
                '    valsList.Add(myRow.Cells(0).Value)
                '    valsList.Add(myRow.Cells(1).Value)
                '    valsList.Add(myRow.Cells(2).Value)
                '    valsList.Add(myRow.Cells(3).Value)
                '    valsList.Add(myRow.Cells(4).Value)
                '    valsList.Add(myRow.Cells(5).Value)
                '    valsList.Add(myRow.Cells(6).Value)
                '    valsList.Add(myRow.Cells(7).Value)
                '    valsList.Add(myRow.Cells(8).Value)
                '    Me.Setup.GeneralFunctions.writeXMLElementWithAttributes(xmlWriter, "model", 4, attrList, valsList)

                '    attrList = New List(Of String)
                '    attrList.Add("bestandsnaam")
                '    attrList.Add("parameter")
                '    For Each outRow As DataGridViewRow In grOutputLocations.Rows
                '        valsList = New List(Of String)
                '        valsList.Add(outRow.Cells(0).Value)
                '        valsList.Add(outRow.Cells(1).Value)
                '    Next

                'Next
                'xmlWriter.WriteLine("  </modellen>")
                xmlWriter.WriteLine("</stochastentool>")
            End Using
        End If




    End Sub

    Private Sub TrPatroon_MouseLeave(sender As Object, e As EventArgs)
        HidePattern()
    End Sub

    Private Sub TrPatroonVGL_MouseHover(sender As Object, e As EventArgs)
        ShowPattern()
    End Sub

    Private Sub TrPatroonVGL_MouseLeave(sender As Object, e As EventArgs)
        HidePattern()
    End Sub

    Public Sub ShowPattern()
        frmPattern.Show()
    End Sub
    Public Sub HidePattern()
        frmPattern.Hide()
    End Sub

    Private Sub btnDatabase_Click(sender As Object, e As EventArgs) Handles btnDatabase.Click
        dlgOpenFile.Filter = "SQLite|*.db"
        dlgOpenFile.ShowDialog()
        txtDatabase.Text = dlgOpenFile.FileName
        Me.Setup.SetDatabaseConnection(txtDatabase.Text)
        Setup.StochastenAnalyse.InitializeDatabaseConnection()
        UpgradeDatabase()
        Me.Setup.SqliteCon.Close()
    End Sub

    Private Sub btnResultsDir_Click(sender As Object, e As EventArgs) Handles btnResultsDir.Click
        dlgFolder.ShowDialog()
        txtResultatenDir.Text = dlgFolder.SelectedPath
    End Sub

    Private Sub btnStochastenDir_Click(sender As Object, e As EventArgs) Handles btnInputDir.Click
        dlgFolder.ShowDialog()
        txtInputDir.Text = dlgFolder.SelectedPath
    End Sub


    Private Sub btnUitlezen_Click(sender As Object, e As EventArgs) Handles btnUitlezen.Click

        Me.Setup.SetProgress(prProgress, lblProgress)
        Me.Setup.GeneralFunctions.UpdateProgressBar("Simulatieresultaten uitlezen...", 0, 10, True)
        Me.Cursor = Cursors.WaitCursor

        Me.Setup.StochastenAnalyse.InitializeDatabaseConnection()

        'first populate our list of models & assoaciated results
        'Setup.StochastenAnalyse.PopulateModelsAndLocationsFromDB(Me.Setup.SqliteCon)

        'make sure the results tables are up to date
        'Call UpdateResultsTables2D(0)                  'raw results 2D (depths and levels) for each climate scenario and duration

        'settings regarding postprocessing
        Setup.StochastenAnalyse.ResultsStartPercentage = Me.Setup.GeneralFunctions.ForceNumeric(txtResultsStartPercentage.Text, "Results start percentege", 0)
        Setup.StochastenAnalyse.ReadResults(Me.Setup.SqliteCon, chk1D.Checked, chk2D.Checked)

        Me.Setup.GeneralFunctions.UpdateProgressBar("klaar.", 0, 10, True)
        Me.Cursor = Cursors.Default

        Me.Setup.StochastenAnalyse.CloseDatabaseConnection()

        'now that our model results have been read we can enable the postprocessing button
        btnPostprocessing.Enabled = True

    End Sub

    Private Sub ConverterenVanAccessNaarSQLiteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ConverterenVanAccessNaarSQLiteToolStripMenuItem.Click
        Dim myfrm As New STOCHLIB.frmAccess2SQLite(Me.Setup)
        myfrm.Show()
    End Sub

    Private Sub VolumesSTOWA2014ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VolumesSTOWA2014ToolStripMenuItem.Click
        Me.Cursor = Cursors.WaitCursor
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_HUIDIG';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_2030';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_2050GL';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_2050GH';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_2050WL';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_2050WH';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_2085GL';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_2085GH';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_2085WL';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2014_2085WH';")
        Me.Cursor = Cursors.Default
        MsgBox("Volumes STOWA2014 zijn met succes verwijderd.")
    End Sub

    Private Sub VolumesSTOWA2019ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VolumesSTOWA2019ToolStripMenuItem.Click
        Me.Cursor = Cursors.WaitCursor
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2019_HUIDIG';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2019_2030_LOWER';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2019_2030_CENTER';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2019_2030_UPPER';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2019_2050GL_CENTER';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2019_2050WL_CENTER';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2019_2050GH_CENTER';")
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM VOLUMES WHERE KLIMAATSCENARIO='STOWA2019_2050WH_CENTER';")
        Me.Cursor = Cursors.Default
        MsgBox("Volumes STOWA2019 zijn met succes verwijderd.")
    End Sub

    Private Sub LegeDatabaseCreërenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LegeDatabaseCreërenToolStripMenuItem.Click
        dlgSaveFile.Filter = "SQLite database|*.db"
        dlgSaveFile.ShowDialog()
        Me.Setup.GeneralFunctions.SQLiteCreateDatabase(dlgSaveFile.FileName, True)
    End Sub

    Private Sub btnCopy_Click(sender As Object, e As EventArgs) Handles btnCopy.Click

        'with this option we will copy the settings from another climate scenario to our current one
        Dim myFields As New Dictionary(Of String, STOCHLIB.clsDataField)
        myFields.Add("DUUR", New STOCHLIB.clsDataField("DUUR", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT))
        myFields.Add("KANS", New STOCHLIB.clsDataField("KANS", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL))
        myFields.Add("KLIMAATSCENARIO", New STOCHLIB.clsDataField("KLIMAATSCENARIO", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
        myFields.Add("NAAM", New STOCHLIB.clsDataField("NAAM", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
        myFields.Add("USE", New STOCHLIB.clsDataField("USE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT))
        Dim myForm As New frmCopySettingsFromClimateScenario(Me.Setup, "RANDVOORWAARDEN", myFields, cmbClimate.Text)
        myForm.ShowDialog()
        If myForm.DialogResult = DialogResult.OK Then
            BuildWaterLevelClassesGrid()    'when the user has copied the configuration from another climate scenario we need to rebuild our grids
            BuildWaterLevelSeriesGrid()
        End If

    End Sub

    Private Sub btnWindCopy_Click(sender As Object, e As EventArgs) Handles btnWindCopy.Click

        'with this option we will copy the settings from another climate scenario to our current one
        Dim myFields As New Dictionary(Of String, STOCHLIB.clsDataField)
        myFields.Add("DUUR", New STOCHLIB.clsDataField("DUUR", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT))
        myFields.Add("NAAM", New STOCHLIB.clsDataField("NAAM", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
        myFields.Add("KLIMAATSCENARIO", New STOCHLIB.clsDataField("KLIMAATSCENARIO", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT))
        myFields.Add("USE", New STOCHLIB.clsDataField("USE", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT))
        myFields.Add("KANS", New STOCHLIB.clsDataField("KANS", STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL))
        Dim myForm As New frmCopySettingsFromClimateScenario(Me.Setup, "WIND", myFields, cmbClimate.Text)
        myForm.ShowDialog()
        If myForm.DialogResult = DialogResult.OK Then
            BuildWindGrid()    'when the user has copied the configuration from another climate scenario we need to rebuild our grids
        End If

    End Sub


    Private Sub UitvoerlocatiesImporterenToolStripMenuItem1_Click(sender As Object, e As EventArgs)
        Dim frmImport As New frmImportOutputlocations(Me.Setup, txtPeilgebieden.Text, cmbWinterpeil.Text, cmbZomerpeil.Text)
        frmImport.ShowDialog()
        PopulateOutputLocationsGrid()
    End Sub

    Private Sub UitvoerlocatiesImporterenToolStripMenuItem_Click_1(sender As Object, e As EventArgs)
    End Sub

    Private Sub SOBEKToolStripMenuItem_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub UitvoerlocatiesImporterenToolStripMenuItem2_Click(sender As Object, e As EventArgs)
    End Sub

    Private Sub grWaterLevelClasses_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles grWaterLevelClasses.CellContentClick

    End Sub

    Private Sub RandvoorwaardenUitCSVToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RandvoorwaardenUitCSVToolStripMenuItem.Click
        Dim myFields As New Dictionary(Of String, STOCHLIB.clsDataField) From {
        {"KLIMAATSCENARIO", New STOCHLIB.clsDataField("KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT)},
        {"NAAM", New STOCHLIB.clsDataField("NAAM", enmSQLiteDataType.SQLITETEXT)},
        {"NODEID", New STOCHLIB.clsDataField("NODEID", enmSQLiteDataType.SQLITETEXT)},
        {"DUUR", New STOCHLIB.clsDataField("DUUR", enmSQLiteDataType.SQLITEINT)},
        {"MINUUT", New STOCHLIB.clsDataField("MINUUT", enmSQLiteDataType.SQLITEINT)},
        {"WAARDE", New STOCHLIB.clsDataField("WAARDE", enmSQLiteDataType.SQLITEREAL)}
        }
        Dim myForm As New STOCHLIB.frmTextFileToSQLite(Me.Setup, Me.Setup.SqliteCon, "RANDREEKSEN", myFields)
        myForm.ShowDialog()
    End Sub

    Private Sub SOBEKToolStripMenuItem_Click_1(sender As Object, e As EventArgs) Handles SOBEKToolStripMenuItem.Click
        Dim myForm As New STOCHLIB.frmClassifyGroundWater(Me.Setup)
        myForm.Show()
    End Sub

    Private Sub DHydroToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DHydroToolStripMenuItem.Click
        Dim myForm As New STOCHLIB.frmClassifyGroundwaterDHydro(Me.Setup)
        myForm.Show()
    End Sub

    Private Sub HBVToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles HBVToolStripMenuItem.Click
        Dim myForm As New STOCHLIB.frmClassifyGroundwaterHBV(Me.Setup)
        myForm.Show()
    End Sub

    Private Sub btnAddModel_Click(sender As Object, e As EventArgs) Handles btnAddModel.Click
        Dim myForm As New frmAddModel(Me.Setup)
        myForm.ShowDialog()
        Dim Result As DialogResult = myForm.DialogResult
        If Result = DialogResult.OK Then
            PopulateSimulationModelsGrid()
        End If
    End Sub

    Private Sub btnDeleteModel_Click(sender As Object, e As EventArgs) Handles btnDeleteModel.Click
        For i = grModels.Rows.Count - 1 To 0 Step -1
            If grModels.Rows(i).Selected Then
                Dim query As String = "DELETE FROM SIMULATIONMODELS WHERE MODELID = " & grModels.Rows(i).Cells(0).Value & ";"
                Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
                grModels.Rows.RemoveAt(i)
            End If
        Next
        PopulateSimulationModelsGrid()
    End Sub

    Private Sub BtnViewer_Click(sender As Object, e As EventArgs) Handles btnViewer.Click
        Dim myForm As New frmPublish(Me.Setup, chk1D.Checked, chk2D.Checked, cmbClimate.Text, cmbDuration.Text, txtPeilgebieden.Text)
        myForm.ShowDialog()
    End Sub

    Private Sub ImporterenToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ImporterenToolStripMenuItem1.Click
        Dim frmImport As New frmImportOutputlocations(Me.Setup, txtPeilgebieden.Text, cmbWinterpeil.Text, cmbZomerpeil.Text)
        frmImport.ShowDialog()
        PopulateOutputLocationsGrid()
    End Sub

    Private Sub AlleVerwijderenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AlleVerwijderenToolStripMenuItem.Click
        Dim query As String = "DELETE FROM OUTPUTLOCATIONS;"
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)

        'repopulate the locations grid
        PopulateOutputLocationsGrid()

        MsgBox("Alle uitvoerlocaties zijn verwijderd.")
    End Sub


    Private Sub grdCombinaties_CellContentClick(sender As Object, e As DataGridViewCellEventArgs)

    End Sub

    Private Sub btnExtraFilesDir_Click(sender As Object, e As EventArgs) Handles btnExtraFilesDir.Click
        dlgFolder.ShowDialog()
        txtExtraFilesDir.Text = dlgFolder.SelectedPath
    End Sub

    Private Sub hlpExtraFiles_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Public Sub OpenHelpLink(url As String)
        Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
    End Sub



    Private Sub grSeizoenen_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles grSeizoenen.CellContentClick

    End Sub

    Private Sub ToevoegenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToevoegenToolStripMenuItem.Click
        Dim myForm As New frmAddModel(Me.Setup)
        myForm.ShowDialog()
        Dim Result As DialogResult = myForm.DialogResult
        If Result = DialogResult.OK Then
            PopulateSimulationModelsGrid()
        End If
    End Sub

    Private Sub SumaquasHertogenboschToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SumaquasHertogenboschToolStripMenuItem.Click

        'first we create a dictionary of integer, strLocation where the key is the column index in the .mat file
        Dim locations As New Dictionary(Of Integer, strLocation)

        ' Split the data into an array of names
        ' Create the dictionary using LINQ
        Dim data As String = "Volume_1D Polygoon17;Volume_1D Polygoon23;Volume_1D Polygoon26;Volume_2D Polygoon17;Volume_2D Polygoon23;Volume_2D Polygoon26;Volume1D+Volume_2D Polygoon18;Volume1D+Volume_2D Polygoon19;Volume1D+Volume_2D Polygoon21;Volume1D+Volume_2D Polygoon22;Volume1D+Volume_2D Polygoon24;Volume1D+Volume_2D Polygoon25;Volume1D+Volume_2D Polygoon27;Q_HS AM_Inlaat_WB_DynBeekdal01;Q_HS AM_Inlaat_WB_DynBeekdal02;Q_HS AM_Inlaat_WB_DynBeekdal03;Q_HS inlaat_BBN;Q_HS inlaat_BBZ;Q_HS AM_Normaalregeling_RosmalenseAa;Q_HS AM_INLAAT_HOWABO;Q_HS AM_S116HDW;Q_HS WST_Weir_DO1_st3;Q_HS AM_Crev_Stuw;Q_HS AM_Crev_onderspuier;Q_HS afsluitstuw_BBZ;Q_HS pomp;Q_HS pomp19_21;WL node_ADM_Bossche_Broek;WL node_3686;WL node_3911;WL node_4045;WL node_meas_BBZ;WL node_WST_MS_DO1_st3;WL node_1122;WL node_2728;WL node_AM_mp_VG;WL node_AM_mp_VG3;WL node_AM_mp_VG2;WL node_2316;WL node_AM_M116HDW;WL node_1561;WL node_4036;WL node_4038;WL node_1032;WL node_AM_Crev_boven;WL node_1033;WL node_3813;WL node_3767;WL node_3723;WL node_4002;WL node_4040;WL node_3979;WL node_meas_BBN;WL node_143;WL node_2639;WL node_1837;WL node_4052;WL node_2736;WL node_995;WL node_991;WL node_2499;WL node_985;WL node_AM_Crev_beneden;WL node_2317;WL node_1034;Q_1D2D Polygoon17;Q_1D2D Polygoon23;Q_1D2D Polygoon26;link1D1D_17_19;link1D1D_18_21;link1D1D_23_26;link1D1D_24_27;link1D1D_19_23;Q2d2d_1918;Q2d2d_2423;Q2d2d_2327;Q2d2d_1821;Outfall edge 923;Outfall edge 1598;Outfall edge 986;Outfall edge 2281;GL AM_INLAAT_HOWABO;GL AM_S116HDW;GL AM_Crev_Stuw;GL AM_Crev_onderspuier;GL WST_Weir_DO1_st3;GL inlaat_BBN;GL inlaat_BBZ;GL afsluitstuw_BBZ;GL AM_Inlaat_WB_DynBeekdal01;GL AM_Inlaat_WB_DynBeekdal02;GL AM_Inlaat_WB_DynBeekdal03;GL AM_Normaalregeling_RosmalenseAa;Glel AM_Crev_onderspuier"
        Dim Xdata As String = "157504.473;149057.137;149145.418067405;157504.473;149057.137;149145.418067405;155370.342991181;155488.072999127;153694.893999982;147936.655473;149116.276;150273.977286899;149145;157380.422;154558.759636;153955.123351;148749.996719427;150007.943635935;153976.690999937;148250.320696936;133356.853;148536.500576872;147064.088669223;147074.301943365;151223.856;149211.828284;153880.691;148637.184;149116.276;150406.098243536;149975.034864922;150007.943635935;148536.500576872;146386.348;148250.002714649;147944.917770288;148192.573627917;148257.93771293;133356.853;133356.853;152692.465;148536.500576872;148539.815708015;147074.301943365;147136.092999553;147064.088669223;151244.92446711;149804.857674709;151680.325;150998.812551267;148750.018491675;150134.870144985;148749.996719427;163092.757;157031.141000001;162658.925031525;147962.636399;154480.833692852;155538.485000061;153973.510528674;154007.802;152154.491;147004.820906;148989.709;147004.820906;157504.473;149057.137;149145.418067405;157372.5452;154177.038906103;148572.537591189;150933.682;153357.283;154558.759636;149234.168094333;151797.377;154177.038906103;151398.513999975;150954.054901232;152154.491;153302.723242045;148250.320696936;133356.853;147064.088669223;147074.301943365;148536.500576872;148749.996719427;150007.943635935;151223.856;157380.422;154558.759636;153955.123351;153976.690999937;147074.301943365"
        Dim Ydata As String = "407344.467;411834.341;408788.631070115;407344.467;411834.341;408788.631070115;409274.241983708;408900.64599942;411428.528999981;410088.35023;409880.588;408108.833809909;408788;407476.773;410406.562631;411147.436387;410498.103747917;407597.64579551;411096.198999786;410241.88785169;413867.032;410412.521536655;416083.099545059;416084.013963311;408970.304;408781.410727;411114.531;410223.728999996;409880.588;408274.458669836;407594.398832904;407597.64579551;410412.521536655;411766.886;410241.872872093;410088.336286995;410239.167487044;410282.816982477;413867.032;413867.032;410684.149;410412.521536655;410417.201376867;416084.013963311;416074.777000277;416083.099545059;408987.540836726;409852.008719336;407347.773;409367.863818105;410498.097741942;409128.302181511;410498.103747917;406279.195;407773.115999998;405381.371298199;410088.306386;410399.981885724;409195.283999611;411108.267883591;411070.864;413179.888;416077.793164;410700.973;416077.793164;407344.467;411834.341;408788.631070115;407467.2609;411012.703621718;410352.43132758;409570.015;411243.655;410406.562631;410578.524912764;409813.697;411012.703621718;412713.612999997;414100.330157196;413179.888;411662.505086313;410241.88785169;413867.032;416083.099545059;416084.013963311;410412.521536655;410498.103747917;407597.64579551;408970.304;407476.773;410406.562631;411147.436387;411096.198999786;416084.013963311"

        'Dim data As String = "WL node_ADM_Bossche_Broek;WL node_3686;WL node_3911;WL node_4045;WL node_meas_BBZ;WL node_WST_MS_DO1_st3;WL node_1122;WL node_2728;WL node_AM_mp_VG;WL node_AM_mp_VG3;WL node_AM_mp_VG2;WL node_2316;WL node_AM_M116HDW;WL node_1561;WL node_4036;WL node_4038;WL node_1032;WL node_AM_Crev_boven;WL node_1033;WL node_3813;WL node_3767;WL node_3723;WL node_4002;WL node_4040;WL node_3979;WL node_meas_BBN;WL node_143;WL node_2639;WL node_1837;WL node_4052;WL node_2736;WL node_995;WL node_991;WL node_2499;WL node_985;WL node_AM_Crev_beneden;WL node_2317;WL node_1034"
        'Dim Xdata As String = "148637.184;149116.276;150406.0982;149975.0349;150007.9436;148536.5006;146386.348;148250.0027;147944.9178;148192.5736;148257.9377;133356.853;133356.853;152692.465;148536.5006;148539.8157;147074.3019;147136.093;147064.0887;151244.9245;149804.8577;151680.325;150998.8126;148750.0185;150134.8701;148749.9967;163092.757;157031.141;162658.925;147962.6364;154480.8337;155538.485;153973.5105;154007.802;152154.491;147004.8209;148989.709;147004.8209"
        'dim Ydata As String = "410223.729;409880.588;408274.4587;407594.3988;407597.6458;410412.5215;411766.886;410241.8729;410088.3363;410239.1675;410282.817;413867.032;413867.032;410684.149;410412.5215;410417.2014;416084.014;416074.777;416083.0995;408987.5408;409852.0087;407347.773;409367.8638;410498.0977;409128.3022;410498.1037;406279.195;407773.116;405381.3713;410088.3064;410399.9819;409195.284;411108.2679;411070.864;413179.888;416077.7932;410700.973;416077.7932"


        ' Split the data into an array of names
        Dim names As String() = data.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim XCoords As String() = Xdata.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim YCoords As String() = Ydata.Split({";"c}, StringSplitOptions.RemoveEmptyEntries)

        ' Create the dictionary using LINQ, now including X and Y coordinates
        locations = names.Select(Function(name, index) New With {Key .Key = index, Key .Value = New strLocation With {.Name = name, .Parameter = enm1DParameter.unknown, .X = Double.Parse(XCoords(index)), .Y = Double.Parse(YCoords(index))}}).ToDictionary(Function(x) x.Key, Function(x) x.Value)

        For Each myID As Integer In locations.Keys
            If locations(myID).Name.ToLower.Contains("volume") Then
                'volume
                locations(myID).Parameter = enm1DParameter.volume
            ElseIf locations(myID).Name.ToLower.Contains("wl") Then
                'waterlevel
                locations(myID).Parameter = enm1DParameter.waterlevel
            ElseIf locations(myID).Name.ToLower.Contains("q_1d2d") Then
                'internal exchange of water between 1D and 2D
                locations(myID).Parameter = enm1DParameter.internal_exchange
            ElseIf locations(myID).Name.ToLower.Contains("q") Then
                'discharge
                locations(myID).Parameter = enm1DParameter.discharge
            ElseIf locations(myID).Name.ToLower.Contains("link") Then
                'discharge between two polygons!!!!
                locations(myID).Parameter = enm1DParameter.discharge
            ElseIf locations(myID).Name.ToLower.Contains("glel") Then
                'gate height
                locations(myID).Parameter = enm1DParameter.gate_height
            ElseIf locations(myID).Name.ToLower.Contains("gl") Then
                'crest level
                locations(myID).Parameter = enm1DParameter.crest_level
            Else
                locations(myID).Parameter = enm1DParameter.unknown
            End If
        Next


        Using cmd As New SQLite.SQLiteCommand
            cmd.Connection = Me.Setup.SqliteCon
            Me.Setup.SqliteCon.Open()

            'first clear the table
            Dim query As String = "DELETE FROM OUTPUTLOCATIONS;"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

            ' Assuming cmd is your SQLiteCommand object
            Using transaction = Me.Setup.SqliteCon.BeginTransaction()
                For i = 0 To locations.Count - 1

                    Dim X As Double = locations(i).X
                    Dim Y As Double = locations(i).Y
                    Dim Lat As Double
                    Dim Lon As Double
                    Dim Name As String = locations(i).Name
                    Dim Par As String = locations(i).Parameter.ToString

                    Me.Setup.GeneralFunctions.RD2WGS84(X, Y, Lat, Lon)

                    cmd.CommandText = "INSERT INTO OUTPUTLOCATIONS (LOCATIEID, LOCATIENAAM, MODELID, MODULE, MODELPAR, RESULTSFILE, RESULTSTYPE, X, Y, LAT, LON, WP, ZP) VALUES (@locId, @locName, @modelId, @module, @modelPar, @resultsFile, @resultsType, @x, @y, @lat, @lon, @wp, @zp);"

                    cmd.Parameters.Clear()
                    cmd.Parameters.AddWithValue("@locId", i.ToString())
                    cmd.Parameters.AddWithValue("@locName", Name)
                    cmd.Parameters.AddWithValue("@modelId", "1")
                    cmd.Parameters.AddWithValue("@module", "FLOW1D")
                    cmd.Parameters.AddWithValue("@modelPar", Par)
                    cmd.Parameters.AddWithValue("@resultsFile", "Output_TQ150HM.mat")
                    cmd.Parameters.AddWithValue("@resultsType", "MAX")
                    cmd.Parameters.AddWithValue("@x", X)
                    cmd.Parameters.AddWithValue("@y", Y)
                    cmd.Parameters.AddWithValue("@lat", Lat)
                    cmd.Parameters.AddWithValue("@lon", Lon)
                    cmd.Parameters.AddWithValue("@wp", DBNull.Value)
                    cmd.Parameters.AddWithValue("@zp", DBNull.Value)

                    cmd.ExecuteNonQuery()
                Next
                transaction.Commit()
            End Using
        End Using
        PopulateOutputLocationsGrid()
    End Sub

    Private Sub hlpUitvoermap_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpInvoermap_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#invoermap")
    End Sub

    Private Sub hlpResultatenmap_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpDatabase_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpShapefilePeilgebieden_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpVeldWinterpeil_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpVeldZomerpeil_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpMeteo_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpBerekeningen_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpOutput_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpNabewerking_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
    End Sub

    Private Sub hlpModellen_Click(sender As Object, e As EventArgs) Handles hlpModellen.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/models.html")
    End Sub

    Private Sub hlpSeizoenen_Click(sender As Object, e As EventArgs) Handles hlpSeizoenen.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/seizoenen.html")
    End Sub

    Private Sub hlpInvoermap2_Click(sender As Object, e As EventArgs) Handles hlpInvoermap2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#invoermap")
    End Sub

    Private Sub hlbUitvoermap2_Click(sender As Object, e As EventArgs) Handles hlbUitvoermap2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#uitvoermap")
    End Sub

    Private Sub hlpResultatenmap2_Click(sender As Object, e As EventArgs) Handles hlpResultatenmap2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#resultatenmap")
    End Sub

    Private Sub hlpExtraFiles2_Click(sender As Object, e As EventArgs) Handles hlpExtraFiles2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#map-met-extra-bestanden-per-simulatie")

    End Sub

    Private Sub hlpDatabase2_Click(sender As Object, e As EventArgs) Handles hlpDatabase2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#database")
    End Sub

    Private Sub hlpShapefilePeilgebieden2_Click(sender As Object, e As EventArgs) Handles hlpShapefilePeilgebieden2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#shapefile-peilgebieden")
    End Sub

    Private Sub hlpVeldWinterpeil2_Click(sender As Object, e As EventArgs) Handles hlpVeldWinterpeil2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#veld-winterpeil-en-veld-zomerpeil")
    End Sub

    Private Sub hlpVeldZomerpeil2_Click(sender As Object, e As EventArgs) Handles hlpVeldZomerpeil2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#veld-winterpeil-en-veld-zomerpeil")
    End Sub

    Private Sub hlpMeteo2_Click(sender As Object, e As EventArgs) Handles hlpMeteo2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#meteo")
    End Sub

    Private Sub hlpBerekeningen2_Click(sender As Object, e As EventArgs) Handles hlpBerekeningen2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#berekeningen")
    End Sub

    Private Sub hlpOutput2_Click(sender As Object, e As EventArgs) Handles hlpOutput2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#output")
    End Sub

    Private Sub hlpNabewerking2_Click(sender As Object, e As EventArgs) Handles hlpNabewerking2.Click
        OpenHelpLink("https://siebebosch.github.io/DeNieuweStochastentool/GUI/general.html#nabewerking")
    End Sub

    Private Sub NeerslagpatronenUitCSVToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NeerslagpatronenUitCSVToolStripMenuItem.Click
        Dim myFields As New Dictionary(Of String, STOCHLIB.clsDataField) From {
        {"PATROON", New STOCHLIB.clsDataField("PATROON", enmSQLiteDataType.SQLITETEXT)},
        {"SEIZOEN", New STOCHLIB.clsDataField("SEIZOEN", enmSQLiteDataType.SQLITETEXT)},
        {"DUUR", New STOCHLIB.clsDataField("DUUR", enmSQLiteDataType.SQLITEINT)},
        {"UUR", New STOCHLIB.clsDataField("UUR", enmSQLiteDataType.SQLITEINT)},
        {"FRACTIE", New STOCHLIB.clsDataField("FRACTIE", enmSQLiteDataType.SQLITEREAL)}
        }
        Dim myForm As New STOCHLIB.frmTextFileToSQLite(Me.Setup, Me.Setup.SqliteCon, "NEERSLAGVERLOOP", myFields)
        Dim res As DialogResult = myForm.ShowDialog()
        If res = DialogResult.OK Then
            Me.Setup.SetProgress(prProgress, lblProgress)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Import successful.", 0, 10, True)
        End If
    End Sub

    Private Sub NeerslagpatronenkansUitCSVToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NeerslagpatronenkansUitCSVToolStripMenuItem.Click
        Dim myFields As New Dictionary(Of String, STOCHLIB.clsDataField) From {
        {"KLIMAATSCENARIO", New STOCHLIB.clsDataField("KLIMAATSCENARIO", enmSQLiteDataType.SQLITETEXT)},
        {"SEIZOEN", New STOCHLIB.clsDataField("SEIZOEN", enmSQLiteDataType.SQLITETEXT)},
        {"DUUR", New STOCHLIB.clsDataField("DUUR", enmSQLiteDataType.SQLITEINT)},
        {"PATROON", New STOCHLIB.clsDataField("PATROON", enmSQLiteDataType.SQLITETEXT)},
        {"KANS", New STOCHLIB.clsDataField("KANS", enmSQLiteDataType.SQLITEREAL)},
        {"USE", New STOCHLIB.clsDataField("USE", enmSQLiteDataType.SQLITEINT)},
        {"KANSCORR", New STOCHLIB.clsDataField("KANSCORR", enmSQLiteDataType.SQLITEREAL)}
        }
        Dim myForm As New STOCHLIB.frmTextFileToSQLite(Me.Setup, Me.Setup.SqliteCon, "PATRONEN", myFields)
        Dim res As DialogResult = myForm.ShowDialog()

        If res = DialogResult.OK Then
            'refesh the patterns grids
            RefreshPatternsGrids()
            Me.Setup.SetProgress(prProgress, lblProgress)
            Me.Setup.GeneralFunctions.UpdateProgressBar("Import successful.", 0, 10, True)
        End If

    End Sub

    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        Dim result = Me.Setup.Gebiedsreductie.ComputeARF(10, 24, 10)
        Debug.WriteLine($"Result: {result}")
    End Sub

    Private Sub AlleResultatenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AlleResultatenToolStripMenuItem.Click
        Me.Setup.GeneralFunctions.UpdateProgressBar("Clearing data from table RESULTATEN", 0, 10, True)
        Dim query As String = "DELETE FROM RESULTATEN;"
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
        Me.Setup.GeneralFunctions.UpdateProgressBar("Clearing data from table HERHALINGSTIJDEN", 5, 10, True)
        query = "DELETE FROM HERHALINGSTIJDEN;"
        Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
        Me.Setup.GeneralFunctions.UpdateProgressBar("Cleanup complete.", 0, 10, True)
    End Sub

    Private Sub grWindKlassen_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles grWindKlassen.CellContentClick

    End Sub

    Public Function WriteFouTopographyJSON(path As String) As Boolean
        Try
            Return Me.Setup.StochastenAnalyse.WriteFouTopographyJSON(path)
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function WriteFouTopographyJSON: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub LeesMATFileToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LeesMATFileToolStripMenuItem.Click
        'Dim matFilePath = "c:\SYNC\PROJECTEN\H3116.AAenMaas.Stochasten\03.IO\Input_TQ150QHM.mat"

        'Dim matFilePath = "c:\SYNC\PROJECTEN\H3116.AAenMaas.Stochasten\07.Gevoeligheidsanalyse.DenBosch\03.Model\INPUT\Input_TQ150HM.mat"
        Dim matfilepath = "c:\SYNC\PROJECTEN\H3116.AAenMaas.Stochasten\07.Gevoeligheidsanalyse.DenBosch\03.Model\INPUT\Init_V_GL_TQ150HM.mat"

        'Dim txtfilePath = "c:\temp\Input_TQ150HM.txt"
        Dim txtfilePath = "c:\temp\Init_V_GL_TQ150HM.txt"

        Dim args() As String
        ' create a reader for the file
        Dim mfr As New MatFileReader(matfilepath)

        Using mywriter As New StreamWriter(txtfilePath)
            'mywriter.WriteLine("NDimensions: " & mfr.Content.Values(0).NDimensions)
            mywriter.WriteLine("NDatasets: " & mfr.Content.Values.Count)
            For i = 1 To mfr.Content.Values.Count
                mywriter.WriteLine($"Dataset {i}: {mfr.Content.Values(i - 1).Name}")
                mywriter.WriteLine($"   Dimensions: {mfr.Content.Values(i - 1).NDimensions}")
                For j = 1 To mfr.Content.Values(i - 1).NDimensions
                    mywriter.WriteLine($"       Dimension {j}: {mfr.Content.Values(i - 1).Dimensions(j - 1)}")
                Next
            Next

            'mywriter.Write(mfr.Content.Values(0))
            mywriter.WriteLine("")
            Dim myValues As MLDouble = TryCast(mfr.Content.Values(0), MLDouble)
            If myValues IsNot Nothing Then
                ' now get the double values
                Dim tmp As Double()() = myValues.GetArray()
                'squares = tmp(0)
                For i = 0 To UBound(tmp, 1)
                    Dim myStr As String = ""
                    Dim content As Object = tmp(i)
                    For j = 0 To UBound(content)
                        myStr &= " " & tmp(i)(j)
                    Next
                    mywriter.WriteLine(myStr)
                Next

            End If
        End Using

        ' get a reference to our matlab 'squares' double matrix
        'Dim mlSquares As MLDouble = TryCast(mfr.Content("squares"), MLDouble)
        'If mlSquares IsNot Nothing Then
        '    ' now get the double values
        '    Dim tmp As Double()() = mlSquares.GetArray()
        '    squares = tmp(0)
        'End If
    End Sub

    Private Sub btnSimulate_Click(sender As Object, e As EventArgs) Handles btnSimulate.Click
        Try
            Dim batRunrPath As String
            batRunrPath = "c:\Program Files\Hydroconsult\BAT_RUNR\BAT_RUNR.exe"

            If Not System.IO.File.Exists(batRunrPath) Then
                Throw New Exception($"Batch runner executable BAT_RUNR.exe not found in {batRunrPath}. Please install it.")
            End If

            For Each myModel In Me.Setup.StochastenAnalyse.Models.Values
                ' Define the arguments to pass
                ' Assuming arg1 and arg2 are your internal variables
                Dim arg1 As String = myModel.TempWorkDir & "\simulations.json"
                Dim arg2 As Integer = Me.Setup.GeneralFunctions.ForceNumeric(txtMaxParallel.Text, "Maximum number of simultaneous computations", 4, enmMessageType.Message)
                Dim arguments As String = arg1 & " " & arg2
                Process.Start(batRunrPath, arguments)
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub btnCopyResults_Click(sender As Object, e As EventArgs) Handles btnCopyResults.Click
        Try
            Dim ShowLog As Boolean = True

            'make sure to accept crashed results if specified
            Me.Setup.StochastenAnalyse.AllowCrashedResults = chkUseCrashedResults.Checked

            'tijdens de berekeningen willen we ook op andere machines kunnen draaien, dus verbreek de verbinding met de database
            If Me.Setup.SqliteCon IsNot Nothing Then Me.Setup.SqliteCon.Close()


            'again, read de base case from the schematization(s)
            For Each myModel As STOCHLIB.clsSimulationModel In Me.Setup.StochastenAnalyse.Models.Values
                If myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.SOBEK Then
                    Call Setup.SetAddSobekProject(myModel.ModelDir, Me.Setup.GeneralFunctions.DirFromFileName(myModel.Exec))
                    If Not Setup.SOBEKData.ActiveProject.Cases.ContainsKey(myModel.CaseName.Trim.ToUpper) Then Throw New Exception("Error: could not find case " & myModel.CaseName & " in SOBEK project.")
                    Setup.SetActiveCase(myModel.CaseName)
                    Setup.InitSobekModel(True, True)
                    Setup.ReadSobekDataDetail(False, False, False, False, False, True, False, False, False, False, False)
                ElseIf myModel.ModelType = STOCHLIB.GeneralFunctions.enmSimulationModel.DIMR OrElse myModel.ModelType = enmSimulationModel.DHYDRO OrElse myModel.ModelType = enmSimulationModel.DHYDROSERVER Then
                    'do nothing since D-Hydro does not yet have a case manager
                    Call Setup.SetDIMRProject(myModel.ModelDir)
                    Call Setup.DIMRData.DIMRConfig.Read()
                End If
            Next

            'copy the results to the results directory
            If grRuns.SelectedRows.Count > 0 Then
                If Not Me.Setup.StochastenAnalyse.Runs.CopyResultsFromSelected(grRuns, btnPostprocessing, chkRemoveSimulationAfterCopyingResultsFiles.Checked) Then
                    MsgBox("Fouten bij het kopieren van resultaten uit de geselecteerde runs. Controleer de logfile voor meldingen.")
                    Me.Setup.Log.write(Setup.StochastenAnalyse.ResultsDir & "\logfile.txt", True)
                End If
            Else
                MsgBox("Selecteer de rijen van de simulaties waarvan u de resultaten wilt kopiëren")
                ShowLog = False
            End If

            're-enable the buttons
            EnableButtons(True)

            'afsluiten & logfile schrijven
            Dim logfile As String = Replace(Me.Setup.StochastenAnalyse.XMLFile, ".xml", ".log", , , Microsoft.VisualBasic.CompareMethod.Text)
            Me.Setup.GeneralFunctions.UpdateProgressBar("klaar.", 0, 10, True)
            Me.Setup.Log.write(Me.Setup.Settings.RootDir & "\" & logfile, ShowLog)

        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function btnCopyResults: " & ex.Message)
        End Try

    End Sub


    Private Sub btnOutputDir_Click(sender As Object, e As EventArgs) Handles btnOutputDir.Click
        dlgFolder.ShowDialog()
        txtOutputDir.Text = dlgFolder.SelectedPath
    End Sub





    Private Sub BtnRemoveOutputLocation_Click(sender As Object, e As EventArgs) Handles btnRemoveOutputLocation.Click
        For i = grOutputLocations.Rows.Count - 1 To 0 Step -1
            If grOutputLocations.Rows(i).Selected Then
                grOutputLocations.Rows.RemoveAt(i)
            End If
        Next

        'now that we updated the grid with output locations, write the current state to the database
        UpdateOutputLocationsTable()

    End Sub

    Private Function UpdateOutputLocationsTable() As Boolean
        Try
            Dim i As Integer, myQuery As String
            myQuery = "DELETE FROM OUTPUTLOCATIONS;"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, myQuery)

            Using cmd As New SQLite.SQLiteCommand
                cmd.Connection = Me.Setup.SqliteCon
                Me.Setup.SqliteCon.Open()
                Using transaction = Me.Setup.SqliteCon.BeginTransaction
                    For i = 0 To grOutputLocations.Rows.Count - 1
                        cmd.CommandText = "INSERT INTO OUTPUTLOCATIONS (LOCATIEID, LOCATIENAAM, MODELID, MODULE, MODELPAR, RESULTSFILE, RESULTSTYPE, X, Y, LAT, LON, WP, ZP) VALUES ('" & grOutputLocations.Rows(i).Cells("LOCATIEID").Value & "','" & grOutputLocations.Rows(i).Cells("LOCATIEID").Value & "','" & grOutputLocations.Rows(i).Cells("MODELID").Value & "','" & grOutputLocations.Rows(i).Cells("MODULE").Value & "','" & grOutputLocations.Rows(i).Cells("MODELPAR").Value & "','" & grOutputLocations.Rows(i).Cells("RESULTSFILE").Value & "','" & grOutputLocations.Rows(i).Cells("RESULTSTYPE").Value & "'," & grOutputLocations.Rows(i).Cells("X").Value & "," & grOutputLocations.Rows(i).Cells("Y").Value & "," & grOutputLocations.Rows(i).Cells("LAT").Value & "," & grOutputLocations.Rows(i).Cells("LON").Value & "," & grOutputLocations.Rows(i).Cells("WP").Value & "," & grOutputLocations.Rows(i).Cells("ZP").Value & ");"
                        cmd.ExecuteNonQuery()
                    Next
                    transaction.Commit() 'this is where the bulk insert is finally executed.
                End Using
            End Using

            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Private Sub grModels_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles grModels.CellValueChanged
        Try

            'v2.3.2: restored the functionality to update the database when editing in the grid
            'if all cells of the current row have been filled in, clear the existing table and rebuild it, based on the current grid content
            Dim query As String, Args As String = String.Empty, CaseName As String = String.Empty
            Dim Resultsfiles_RR As String = String.Empty, Resultsfiles_Flow As String = String.Empty, Resultsfiles_RTC As String = String.Empty
            Dim r As Integer

            'v2.3.2. changed the IsDBNUll checks.
            If IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(0).Value) Then Throw New Exception("Specify a Model ID.")
            If IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(1).Value) Then Throw New Exception("Specify the Model type: SOBEK or DIMR.")
            If IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(2).Value) Then Throw New Exception("Specify the path to the model's executable (.bat or .exe)")
            If Not IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(3).Value) Then Args = grModels.Rows.Item(e.RowIndex).Cells(3).Value
            If IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(4).Value) Then Throw New Exception("Specify a Model ID.")
            If Not IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(5).Value) Then CaseName = grModels.Rows.Item(e.RowIndex).Cells(5).Value
            If IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(6).Value) Then Throw New Exception("Specify a temporary work directory for the model.")
            If Not IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(7).Value) Then Resultsfiles_RR = grModels.Rows.Item(e.RowIndex).Cells(7).Value
            If Not IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(8).Value) Then Resultsfiles_Flow = grModels.Rows.Item(e.RowIndex).Cells(8).Value
            If Not IsDBNull(grModels.Rows.Item(e.RowIndex).Cells(9).Value) Then Resultsfiles_RTC = grModels.Rows.Item(e.RowIndex).Cells(9).Value

            'clear the old data
            query = "DELETE FROM SIMULATIONMODELS;"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query)
            For r = 0 To grModels.Rows.Count - 1
                query = "INSERT INTO SIMULATIONMODELS (MODELID, MODELTYPE,EXECUTABLE,ARGUMENTS,MODELDIR,CASENAME,TEMPWORKDIR, RESULTSFILES_RR, RESULTSFILES_FLOW, RESULTSFILES_RTC) VALUES ('" & grModels.Rows(r).Cells(0).Value & "','" & grModels.Rows(r).Cells(1).Value & "','" & grModels.Rows(r).Cells(2).Value & "','" & Args & "','" & grModels.Rows(r).Cells(4).Value & "','" & CaseName & "','" & grModels.Rows(r).Cells(6).Value & "','" & Resultsfiles_RR & "','" & Resultsfiles_Flow & "','" & Resultsfiles_RTC & "');"
                If Not Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False) Then Throw New Exception("Error writing model information to the database.")
            Next

        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            Me.Setup.SqliteCon.Close()
        End Try
    End Sub

    Private Sub cmbClimate_SelectedValueChanged(sender As Object, e As EventArgs) Handles cmbClimate.SelectedValueChanged
        'refresh the volumes grids in order to load the new frequency associated with each volume
        Setup.StochastenAnalyse.KlimaatScenario = DirectCast([Enum].Parse(GetType(STOCHLIB.GeneralFunctions.enmKlimaatScenario), cmbClimate.Text), STOCHLIB.GeneralFunctions.enmKlimaatScenario)
        Call RebuildAllGrids()
    End Sub

End Class

Public Class strLocation
    Public Property Name As String
    Public Property Parameter As GeneralFunctions.enm1DParameter

    Public Property X As Double
    Public Property Y As Double
End Class