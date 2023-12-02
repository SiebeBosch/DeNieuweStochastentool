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
            Dim LocationsList As List(Of String) = Setup.SOBEKData.ActiveProject.ActiveCase.RRResults.UPFLODT.ReadAllLocations(False)

            'POT analysis settings
            Setup.InitializeTijdreeksStatistiek()
            Setup.TijdreeksStatistiek.MinTimeStepsBetweenEvents = Setup.GeneralFunctions.ForceNumeric(txtMinTimestepsBetweenEvents.Text, "Minimum aantal tijdstappen tussen events", 0, enmMessageType.ErrorMessage)
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
                'bugfix: 2014-01-21: In case of unpaved.3b records that do not exist in the his file this routine crashed. 
                'therefore I replaced the line below with the next line in which I take the first ID that actually exists in the his file.
                'myRecord = Setup.SOBEKData.ActiveProject.ActiveCase.RRData.Unpaved3B.Records.Values(0)
                If Not Setup.TijdreeksStatistiek.addRainfallSeriesFromHisFile(Setup.SOBEKData.ActiveProject.ActiveCase.CaseDir & "\upflowdt.his", LocationsList(0), "Rainfall") Then Throw New Exception("Error reading precipitation from hisfile.")
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
                        Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterBySeason(seizoen, cmbDuration.Text, Me.Setup.SOBEKData.ActiveProject.ActiveCase, grGrondwaterKlassen, Seizoensnaam, Dates, chkSeepage.checked, ExportDir)
                    Next
                ElseIf radGroeiseizoen.Checked Then

                    'for each 3b Record perform a POT analysis for growth season (march through october) and outside growth season (nov through feb)
                    For i = 1 To 2
                        'set the current season to process
                        If i = 1 Then
                            seizoen = enmSeason.growthseason
                            Seizoensnaam = "groeiseizoen"
                        Else
                            seizoen = enmSeason.outsidegrowthseason
                            Seizoensnaam = "buitengroeiseizoen"
                        End If
                        Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterBySeason(seizoen, cmbDuration.Text, Me.Setup.SOBEKData.ActiveProject.ActiveCase, grGrondwaterKlassen, Seizoensnaam, Dates, chkSeepage.checked, ExportDir)
                    Next

                ElseIf radJaarRond.Checked Then

                    seizoen = enmSeason.yearround
                    Seizoensnaam = "jaarrond"
                    Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterBySeason(seizoen, cmbDuration.Text, Me.Setup.SOBEKData.ActiveProject.ActiveCase, grGrondwaterKlassen, Seizoensnaam, Dates, chkseepage.checked, ExportDir)

                ElseIf radAprilAugust.Checked Then
                    For i = 1 To 2
                        'set the current season to process
                        If i = 1 Then
                            seizoen = enmSeason.aprilthroughaugust
                            Seizoensnaam = "zomer"
                        Else
                            seizoen = enmSeason.septemberthroughmarch
                            Seizoensnaam = "winter"
                        End If
                        Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterBySeason(seizoen, cmbDuration.Text, Me.Setup.SOBEKData.ActiveProject.ActiveCase, grGrondwaterKlassen, Seizoensnaam, Dates, chkSeepage.Checked, ExportDir)
                    Next
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

    Private Sub cmbSobekCases_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSobekCases.SelectedIndexChanged

    End Sub

    Private Sub btnGroeiseizoenHelp_Click(sender As Object, e As EventArgs) Handles btnGroeiseizoenHelp.Click
        MsgBox("Deze periode sluit aan bij de seizoenen zoals gepubliceerd in de neerslagstatistieken 2004 door STOWA en zoals ook in De Nieuwe Stochastentool geïmplementerd.")
    End Sub
End Class