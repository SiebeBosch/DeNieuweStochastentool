Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.Windows.Forms
Imports System.IO

Public Class frmClassifyGroundWater

    Dim myProject As STOCHLIB.clsSobekProject
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub

    Private Sub btnSbkProject_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSbkProject.Click
        dlgFolder.Description = "Selecteer uw SOBEK-project (.LIT)"
        dlgFolder.ShowDialog()

        If System.IO.Directory.Exists(dlgFolder.SelectedPath) Then
            txtSobekProject.Text = dlgFolder.SelectedPath
            Setup.SetAddSobekProject(dlgFolder.SelectedPath, dlgFolder.SelectedPath)
            Setup.PopulateComboBoxSobekCases(cmbSobekCases)
        End If
    End Sub

    Private Sub btnDeleteGroundwaterClass_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeleteGroundwaterClass.Click
        For Each myRow As DataGridViewRow In grGrondwaterKlassen.SelectedRows
            grGrondwaterKlassen.Rows.Remove(myRow)
        Next
    End Sub

    Private Sub btnAddGroundwaterClass_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddGroundwaterClass.Click
        grGrondwaterKlassen.Rows.Add()
    End Sub

    Private Sub btnClassify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClassify.Click
        Dim myRecord As clsUnpaved3BRecord
        Dim Seizoensnaam As String
        Dim i As Long
        Dim Dates As New List(Of Date)

        Try
            'store the settings
            My.Settings.GroundwaterClasses = New System.Collections.Specialized.StringCollection
            For i = 0 To grGrondwaterKlassen.Rows.Count - 1
                My.Settings.GroundwaterClasses.Add(grGrondwaterKlassen.Rows(i).Cells(0).Value & ";" & grGrondwaterKlassen.Rows(i).Cells(1).Value & ";" & grGrondwaterKlassen.Rows(i).Cells(2).Value)
            Next
            My.Settings.SobekDir = txtSobekProject.Text
            My.Settings.Save()

            'initialize the progress bar on this form
            Setup.SetProgress(prProgress, lblProgress)

            'set the active case and read the unpaved data
            Setup.SetActiveCase(cmbSobekCases.Text)
            Setup.SOBEKData.ActiveProject.ActiveCase.RRData.Unpaved3B.Read()
            Setup.SOBEKData.ActiveProject.ActiveCase.RRResults.UPFLODT = New STOCHLIB.clsHisFileBinaryReader(Setup.SOBEKData.ActiveProject.ActiveCase.CaseDir & "\upflowdt.his", Me.Setup)

            'POT analysis settings
            Setup.InitializeTijdreeksStatistiek()
            Setup.TijdreeksStatistiek.MinTimeStepsBetweenEvents = 24
            Setup.TijdreeksStatistiek.POTFrequency = 10

            dlgFolder.Description = "Uitvoermap voor de grondwaterklassen."
            dlgFolder.ShowDialog()
            Dim ExportDir As String = dlgFolder.SelectedPath
            Me.Setup.Settings.SetExportDirs(ExportDir, True, True, False, False, False)

            Dim seizoen As enmSeason

            If cmbDuration.Text = "" Then
                MsgBox("Selecteer welke neerslagduur van toepassing is.")
            ElseIf grGrondwaterKlassen.Rows.Count = 0 Then
                MsgBox("Maak eerst grondwaterklassen aan.")
            Else

                'pick the first location to extract the rainfall series from and then read the precipitation
                myRecord = Setup.SOBEKData.ActiveProject.ActiveCase.RRData.Unpaved3B.Records.Values(0)
                If Not Setup.TijdreeksStatistiek.addRainfallSeriesFromHisFile(Setup.SOBEKData.ActiveProject.ActiveCase.CaseDir & "\upflowdt.his", myRecord.ID, "Rainfall") Then Throw New Exception("Error reading time series from hisfile.")
                Dates = Setup.TijdreeksStatistiek.NeerslagReeksen.Values(0).Dates 'local copy of the dates

                'Setup.TijdreeksStatistiek.NeerslagReeksen.Values(0).WriteCSV(myRecord.ID)

                If radZomWin.Checked Then

                    'for each 3b Record perform a POT analysis for both summer and winter halfyear
                    For i = 1 To 2
                        'set the current season to process
                        If i = 1 Then
                            seizoen = enmSeason.hydrosummerhalfyear
                            Seizoensnaam = "zomer"
                        Else
                            seizoen = enmSeason.hydrowinterhalfyear
                            Seizoensnaam = "winter"
                        End If
                        Call ClassifyGroundwaterBySeason(seizoen, Seizoensnaam, Dates, ExportDir)
                    Next

                ElseIf radJaarRond.Checked Then

                    seizoen = enmSeason.yearround
                    Seizoensnaam = "jaarrond"
                    Call ClassifyGroundwaterBySeason(seizoen, Seizoensnaam, Dates, ExportDir)

                End If
            End If

            'finally write the POT-values to Excel for future reference
            If Not Setup.TijdreeksStatistiek.POTValuesToExcel() Then Throw New Exception("Error writing POT-values to Excel.")

            Me.Setup.ExcelFile.Path = ExportDir & "\grondwaterclassificatie.xlsx"
            Me.Setup.ExcelFile.Save(True)

            Me.Close()
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            MsgBox("Error: could not classify groundwater levels from hisfile contents.")
        End Try

    End Sub

    Public Function ClassifyGroundwaterBySeason(ByVal seizoen As enmSeason, seizoensnaam As String, ByRef Dates As List(Of Date), ExportDir As String) As Boolean
        Try
            Dim mySeason As clsSeason
            Dim myDuration As clsDuration
            Dim TimeSteps As List(Of Long)
            Dim ts As Long, j As Long, k As Long, n As Long
            Dim myDate As Date, myPar As String
            Dim ws As clsExcelSheet
            Dim lPercentile As Double      'lower boundary percentile for this class
            Dim uPercentile As Double      'upper boundary percentile for this class
            Dim repPerc As Double          'percentile for the class
            Dim repVal As Double          'representative value for the class


            'carry out the POT analysis on the rainfall volumes
            If Not Setup.TijdreeksStatistiek.CalcPOTEvents(cmbDuration.Text, seizoen.ToString, False) Then Throw New Exception("Error executing a POT-analysis on precipitation volumes for the " & seizoen.ToString)

            'now store the starting timesteps for each rainfall event in a list
            'note: these time step indices are valid for ALL unpaved.3b records
            Dim myTijdreeks As clsRainfallSeries = Setup.TijdreeksStatistiek.NeerslagReeksen.Values(0)
            mySeason = myTijdreeks.Seasons.GetByEnum(seizoen)
            myDuration = mySeason.GetDuration(cmbDuration.Text)
            TimeSteps = New List(Of Long)
            TimeSteps = myDuration.POTEvents.GetStartingTimeSteps

            'read the his results for all location and the previously defined initial timesteps per event
            Dim myResult As New List(Of STOCHLIB.HisDataRow)
            Dim myResults As New List(Of List(Of STOCHLIB.HisDataRow))
            Setup.SOBEKData.ActiveProject.ActiveCase.RRResults.UPFLODT.OpenFile()
            myPar = Setup.SOBEKData.ActiveProject.ActiveCase.RRResults.UPFLODT.getParName("Groundw.Level")
            For ts = 0 To TimeSteps.Count - 1
                myDate = Dates(TimeSteps(ts))
                myResult = Setup.SOBEKData.ActiveProject.ActiveCase.RRResults.UPFLODT.ReadTimeStep(myDate, myPar)
                myResults.Add(myResult)
            Next
            Setup.SOBEKData.ActiveProject.ActiveCase.RRResults.UPFLODT.Close()

            'now walk through all rows of the classification datagridview to decide which percentile we'll use
            For Each myRow As DataGridViewRow In grGrondwaterKlassen.Rows

                'determine the boundaries of the current groundwater class
                lPercentile = myRow.Cells(1).Value
                uPercentile = myRow.Cells(2).Value
                repPerc = (lPercentile + uPercentile) / 2

                n = Setup.SOBEKData.ActiveProject.ActiveCase.RRData.Unpaved3B.Records.Count
                j = 0

                'now that we have the starting time index numbers for the POT-events, we can get the groundwater levels from the starting times for each event
                For Each myRecord In Setup.SOBEKData.ActiveProject.ActiveCase.RRData.Unpaved3B.Records.Values

                    j += 1
                    k = -1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Groundwater classification for season " & seizoen.ToString & " and class " & myRow.Cells("colNaam").Value & ".", j, n)
                    ws = Me.Setup.ExcelFile.GetAddSheet(seizoensnaam)
                    ws.ws.Cells(0, 0).Value = "Datum"
                    ws.ws.Cells(0, j).Value = myRecord.ID

                    'get the results for this record from the lists
                    Dim GW(0 To TimeSteps.Count - 1) As Double
                    For Each myResult In myResults
                        For Each Result As STOCHLIB.HisDataRow In myResult
                            If Result.LocationName = myRecord.ID Then
                                k += 1
                                GW(k) = Result.Value 'store the results in a temporary array to allow percentile computation later
                                ws.ws.Cells(k + 1, 0).Value = Result.TimeStep
                                ws.ws.Cells(k + 1, j).Value = Result.Value
                            End If
                        Next
                    Next

                    'update the unpaved.3b record
                    repVal = Me.Setup.GeneralFunctions.Percentile(GW, repPerc)
                    myRecord.ig = 0
                    myRecord.igconst = Math.Round(myRecord.lv - repVal, 2)

                    'write the statistics to Excel
                    ws = Me.Setup.ExcelFile.GetAddSheet(seizoensnaam & ".Stats")
                    Dim r As Integer = myRow.Index * 3
                    ws.ws.Cells(0, j + 1).Value = myRecord.ID
                    ws.ws.Cells(r + 1, 0).Value = myRow.Cells(0).Value
                    ws.ws.Cells(r + 1, 1).Value = "Percentiel " & repPerc
                    ws.ws.Cells(r + 1, j + 1).Value = Math.Round(repVal, 2)
                    ws.ws.Cells(r + 2, 0).Value = myRow.Cells(0).Value
                    ws.ws.Cells(r + 2, 1).Value = "Maaiveldhoogte (m NAP)"
                    ws.ws.Cells(r + 2, j + 1).Value = myRecord.lv
                    ws.ws.Cells(r + 3, 0).Value = myRow.Cells(0).Value
                    ws.ws.Cells(r + 3, 1).Value = "Grondwaterdiepte (m)"
                    ws.ws.Cells(r + 3, j + 1).Value = Math.Round(myRecord.lv - repVal, 2)
                Next

                'set path to the adjusted unpaved.3b file and initialize writing the groundwater level values
                Dim mypath As String = ExportDir & "\" & seizoen.ToString & "_" & myRow.Cells(0).Value & "\"
                If Not System.IO.Directory.Exists(mypath) Then System.IO.Directory.CreateDirectory(mypath)
                Setup.SOBEKData.ActiveProject.ActiveCase.RRData.Unpaved3B.Write(mypath & "unpaved.3b", False)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ClassifyGroundwaterBySeason of class frmClassifyGroundWater.")
            Return False
        End Try


    End Function

    Private Sub frmClassifyGroundWater_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim i As Integer, myStr As String
        Dim Name As String, lBound As Object, uBound As Object

        cmbDuration.Items.Clear()
        cmbDuration.Items.Add(24)
        cmbDuration.Items.Add(48)
        cmbDuration.Items.Add(96)
        cmbDuration.Items.Add(192)
        cmbDuration.Items.Add(216)

        If Not My.Settings.GroundwaterClasses Is Nothing Then
            For i = 0 To My.Settings.GroundwaterClasses.Count - 1
                myStr = My.Settings.GroundwaterClasses(i)
                Name = Me.Setup.GeneralFunctions.ParseString(myStr, ";")
                lBound = Me.Setup.GeneralFunctions.ParseString(myStr, ";")
                uBound = Me.Setup.GeneralFunctions.ParseString(myStr, ";")
                If IsNumeric(lBound) AndAlso IsNumeric(uBound) Then
                    grGrondwaterKlassen.Rows.Add(Name, lBound, uBound)
                End If
            Next
        End If
        dlgFolder.SelectedPath = My.Settings.SobekDir

    End Sub

    Private Sub Instellingen_Enter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Instellingen.Enter

    End Sub
End Class