Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.Windows.Forms
Public Class frmClassifyGroundwaterDHydro

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub

    Private Sub btnRRDir_Click(sender As Object, e As EventArgs) Handles btnRRDir.Click
        Dim Result As System.Windows.Forms.DialogResult = dlgFolder.ShowDialog()
        If Result = Windows.Forms.DialogResult.OK Then
            txtRRDir.Text = dlgFolder.SelectedPath
        End If
    End Sub

    Private Sub btnClassify_Click(sender As Object, e As EventArgs) Handles btnClassify.Click
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
            My.Settings.Save()

            Dim CaseListItem As New clsSobekCaseListItem(Me.Setup)
            CaseListItem.name = "RR"
            CaseListItem.dir = txtRRDir.Text

            Dim myProj As New clsSobekProject(Me.Setup)
            Dim myCase As New ClsSobekCase(Me.Setup, myProj, CaseListItem)


            'initialize the progress bar on this form
            Setup.SetProgress(prProgress, lblProgress)

            'set the active case and read the unpaved data
            'Setup.SetActiveCase(cmbSobekCases.Text)
            myCase.RRData.Unpaved3B.Read()
            myCase.RRResults.UPFLODT = New STOCHLIB.clsHisFileBinaryReader(myCase.CaseDir & "\upflowdt.his", Me.Setup)

            'POT analysis settings
            Setup.InitializeTijdreeksStatistiek()
            Setup.TijdreeksStatistiek.MinTimeStepsBetweenEvents = 24
            Setup.TijdreeksStatistiek.POTFrequency = 10

            dlgFolder.Description = "Uitvoermap voor de grondwaterklassen."
            dlgFolder.ShowDialog()
            Dim ExportDir As String = dlgFolder.SelectedPath
            Me.Setup.Settings.SetExportDirs(ExportDir, False, False, False, False, False)

            Dim seizoen As enmSeason

            If cmbDuration.Text = "" Then
                MsgBox("Selecteer welke neerslagduur van toepassing is.")
            ElseIf grGrondwaterKlassen.Rows.Count = 0 Then
                MsgBox("Maak eerst grondwaterklassen aan.")
            Else

                'pick the first location to extract the rainfall series from and then read the precipitation
                myRecord = myCase.RRData.Unpaved3B.Records.Values(0)
                If Not Setup.TijdreeksStatistiek.addRainfallSeriesFromHisFile(myCase.CaseDir & "\upflowdt.his", myRecord.ID, "Rainfall") Then Throw New Exception("Error reading time series from hisfile.")
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
                        Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterBySeason(seizoen, cmbDuration.Text, myCase, grGrondwaterKlassen, Seizoensnaam, Dates, ExportDir)
                    Next

                ElseIf radJaarRond.Checked Then

                    seizoen = enmSeason.yearround
                    Seizoensnaam = "jaarrond"
                    Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterBySeason(seizoen, cmbDuration.Text, myCase, grGrondwaterKlassen, Seizoensnaam, Dates, ExportDir)

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

    Private Sub frmClassifyGroundwaterDHydro_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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

    Private Sub btnAddGroundwaterClass_Click(sender As Object, e As EventArgs) Handles btnAddGroundwaterClass.Click
        grGrondwaterKlassen.Rows.Add()
    End Sub

    Private Sub btnDeleteGroundwaterClass_Click(sender As Object, e As EventArgs) Handles btnDeleteGroundwaterClass.Click
        For Each myRow As DataGridViewRow In grGrondwaterKlassen.SelectedRows
            grGrondwaterKlassen.Rows.Remove(myRow)
        Next
    End Sub
End Class