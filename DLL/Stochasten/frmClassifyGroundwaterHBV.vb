Imports System.Windows.Forms
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports GemBox.Spreadsheet

Public Class frmClassifyGroundwaterHBV
    Private Setup As clsSetup

    Public Sub New(ByRef Setup As clsSetup)
        InitializeComponent()
        Me.Setup = Setup
    End Sub

    Private Sub btnExcel_Click(sender As Object, e As EventArgs) Handles btnExcel.Click
        dlgOpenFile.Title = "Select the Excel file met langjarige grondwaterstanden en bodemvocht uit HBV (ten minste prec, lz, uz en sm)"
        dlgOpenFile.Filter = "Excel files|*.xls;*.xlsx"
        Dim Res As DialogResult = dlgOpenFile.ShowDialog
        If Res = DialogResult.OK Then
            txtExcelFile.Text = dlgOpenFile.FileName
        End If
    End Sub

    Private Sub frmClassifyGroundwaterHBV_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim i As Integer, myStr As String
        Dim Name As String, lBound As Object, uBound As Object

        txtExcelFile.Text = My.Settings.HBVReportPath
        txtProjectDir.Text = My.Settings.HBVModelDir


        cmbDuration.Items.Clear()
        cmbDuration.Items.Add(12)
        cmbDuration.Items.Add(24)
        cmbDuration.Items.Add(48)
        cmbDuration.Items.Add(96)
        cmbDuration.Items.Add(192)
        cmbDuration.Items.Add(216)

        If My.Settings.GroundwaterClasses IsNot Nothing Then
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
    End Sub

    Private Sub hlpHBVRapport_Click(sender As Object, e As EventArgs) Handles hlpHBVRapport.Click
        MsgBox("HBV heeft de mogelijkheid om een rapport in Excel-formaat te exporteren. Ieder stroomgebied heeft daarin zijn eigen tabblad.")
    End Sub

    Private Sub btnAddGroundwaterClass_Click(sender As Object, e As EventArgs) Handles btnAddGroundwaterClass.Click
        grGrondwaterKlassen.Rows.Add()
    End Sub

    Private Sub btnDeleteGroundwaterClass_Click(sender As Object, e As EventArgs) Handles btnDeleteGroundwaterClass.Click
        For Each myRow As DataGridViewRow In grGrondwaterKlassen.SelectedRows
            grGrondwaterKlassen.Rows.Remove(myRow)
        Next
    End Sub

    Private Sub btnProjectDir_Click(sender As Object, e As EventArgs) Handles btnProjectDir.Click
        dlgFolder.Description = "Select the project directory"
        Dim res As DialogResult = dlgFolder.ShowDialog
        If res = DialogResult.OK Then
            txtProjectDir.Text = dlgFolder.SelectedPath
        End If
    End Sub

    Private Sub hlpProjectDir_Click(sender As Object, e As EventArgs) Handles hlpProjectDir.Click
        MsgBox("Om te kunnen uitvinden voor welke (deel)stroomgebieden initiële grondwatercondities moeten worden weggeschreven, is het nodig de map met het model aan te wijzen.")
    End Sub

    Private Sub btnClassify_Click(sender As Object, e As EventArgs) Handles btnClassify.Click
        Dim Seizoensnaam As String
        Dim i As Long
        Dim Dates As New List(Of Date)

        Try

            'v2.4.3: error handling for wrong percentile classes
            For i = 0 To grGrondwaterKlassen.Rows.Count - 1
                If grGrondwaterKlassen.Rows(i).Cells(1).Value > 1 Then Throw New Exception("Percentielklassen moeten tussen 0 en 1 liggen.")
                If grGrondwaterKlassen.Rows(i).Cells(1).Value < 0 Then Throw New Exception("Percentielklassen moeten tussen 0 en 1 liggen.")
                If grGrondwaterKlassen.Rows(i).Cells(2).Value > 1 Then Throw New Exception("Percentielklassen moeten tussen 0 en 1 liggen.")
                If grGrondwaterKlassen.Rows(i).Cells(2).Value < 0 Then Throw New Exception("Percentielklassen moeten tussen 0 en 1 liggen.")
            Next

            'store the settings
            My.Settings.GroundwaterClasses = New System.Collections.Specialized.StringCollection
            For i = 0 To grGrondwaterKlassen.Rows.Count - 1
                My.Settings.GroundwaterClasses.Add(grGrondwaterKlassen.Rows(i).Cells(0).Value & ";" & grGrondwaterKlassen.Rows(i).Cells(1).Value & ";" & grGrondwaterKlassen.Rows(i).Cells(2).Value)
            Next
            My.Settings.HBVReportPath = txtExcelFile.Text
            My.Settings.HBVModelDir = txtProjectDir.Text

            My.Settings.Save()

            dlgFolder.Description = "Uitvoermap voor de grondwaterklassen."
            dlgFolder.ShowDialog()
            Dim ExportDir As String = dlgFolder.SelectedPath
            Me.Setup.Settings.SetExportDirs(ExportDir, False, False, False, False, False)
            Me.Setup.ExcelFile.Path = ExportDir & "\grondwaterclassificatie.xlsx"

            Dim ModelParametersClass As clsModelParameterClass
            If cmbHBVPars.Text = "lz" Then
                'primary parameter is lower zone, no secondary parameter. Upper zone and Soil moisture are side parameters on the top level
                ModelParametersClass = New clsModelParameterClass(Me.Setup, enmModelParameter.lz, New List(Of enmModelParameter) From {enmModelParameter.uz, enmModelParameter.sm}, enmModelParameter.none, New List(Of enmModelParameter))
            ElseIf cmbHBVPars.Text = "uz" Then
                'primary parameter is lower zone; soil moisture and lower zone are side parameters
                ModelParametersClass = New clsModelParameterClass(Me.Setup, enmModelParameter.uz, New List(Of enmModelParameter) From {enmModelParameter.sm, enmModelParameter.lz}, enmModelParameter.none, New List(Of enmModelParameter))
            ElseIf cmbHBVPars.Text = "sm" Then
                'primary parameter is soil moisture, upper zone and lower zone are side parameters
                ModelParametersClass = New clsModelParameterClass(Me.Setup, enmModelParameter.sm, New List(Of enmModelParameter) From {enmModelParameter.uz, enmModelParameter.lz}, enmModelParameter.none, New List(Of enmModelParameter))
            ElseIf cmbHBVPars.Text = "lz + sm" Then
                'primary parameter is lower zone, secondary parameter is soil moisture, with upper zone as side parameter. Reason for this order is that soil moisture has more diverse values and uz can be zero
                ModelParametersClass = New clsModelParameterClass(Me.Setup, enmModelParameter.lz, New List(Of enmModelParameter), enmModelParameter.sm, New List(Of enmModelParameter) From {enmModelParameter.uz})
            Else
                Throw New Exception("No valid combination of model parameters selected for classification.")
            End If

            'initialize the progress bar on this form
            Setup.SetProgress(prProgress, lblProgress)

            'set the active case and read the unpaved data
            Me.Setup.HBVData = New clsHBVProject(Me.Setup, txtProjectDir.Text, True)
            Me.Setup.HBVData.ReadAll()

            'now we must read the timeseries from our Excel file
            'we know that each worksheet contains the timeseries for a certain catchment and that the name of the worksheet corresponds with the name of the catchment
            'we also know that the first column contains the dates and that the row header is in the fourth row
            'given row 4, we must search the columns for the following parameters: prec, lz, uz, sm
            'notice that the parameter names can be followed by a string such as totmean, so we'll have to search for the first four characters only

            'Setup.SOBEKData.ActiveProject.ActiveCase.RRData.Unpaved3B.Read()
            'Setup.SOBEKData.ActiveProject.ActiveCase.RRResults.UPFLODT = New STOCHLIB.clsHisFileBinaryReader(Setup.SOBEKData.ActiveProject.ActiveCase.CaseDir & "\upflowdt.his", Me.Setup)
            'Dim LocationsList As New List(Of String) ' = Setup.SOBEKData.ActiveProject.ActiveCase.RRResults.UPFLODT.ReadAllLocations(False)

            'POT analysis settings
            Me.Setup.GeneralFunctions.UpdateProgressBar("Initializing...", 0, 10, True)
            Setup.InitializeTijdreeksStatistiek()
            Setup.TijdreeksStatistiek.MinTimeStepsBetweenEvents = Setup.GeneralFunctions.ForceNumeric(txtMinTimestepsBetweenEvents.Text, "Minimum aantal tijdstappen tussen events", 0, enmMessageType.ErrorMessage)
            Setup.TijdreeksStatistiek.POTFrequency = 10

            'now it's time to read the timeseries from the Excel file. We'll do this for each worksheet
            Dim HBVReport As New clsExcelBook(Me.Setup, txtExcelFile.Text)

            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading HBV Report...", 1, 10, True)
            Me.Cursor = Cursors.WaitCursor
            HBVReport.Read()
            Me.Cursor = Cursors.Default

            'read the timeseries from the Excel file. Each catchment gets its own timeseries
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building timeseries from HBV Report...", 2, 10, True)
            If Not Me.Setup.TijdreeksStatistiek.addDataSeriesFromHBVReport(HBVReport) Then Throw New Exception("Error reading timeseries from Excel file.")

            Dim seizoen As enmSeason

            If cmbDuration.Text = "" Then
                MsgBox("Selecteer welke neerslagduur van toepassing Is.")
            ElseIf grGrondwaterKlassen.Rows.Count = 0 Then
                MsgBox("Maak eerst grondwaterklassen aan.")
            Else

                Dates = Setup.TijdreeksStatistiek.NeerslagReeksen.Values(0).Dates 'local copy of the dates

                If radZomWin.Checked Then

                    'for each catchment perform a POT analysis for both summer and winter halfyear
                    For i = 1 To 2
                        'set the current season to process
                        If i = 1 Then
                            seizoen = enmSeason.hydrosummerhalfyear
                            Seizoensnaam = "zomer"
                        Else
                            seizoen = enmSeason.hydrowinterhalfyear
                            Seizoensnaam = "winter"
                        End If
                        Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterHBVBySeason(ModelParametersClass, enmTimestepStatistic.first, seizoen, cmbDuration.Text, txtExcelFile.Text, grGrondwaterKlassen, Seizoensnaam, Dates, ExportDir)
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
                        Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterHBVBySeason(ModelParametersClass, enmTimestepStatistic.first, seizoen, cmbDuration.Text, txtExcelFile.Text, grGrondwaterKlassen, Seizoensnaam, Dates, ExportDir)
                    Next

                ElseIf radJaarRond.Checked Then

                    seizoen = enmSeason.yearround
                    Seizoensnaam = "jaarrond"
                    Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterHBVBySeason(ModelParametersClass, enmTimestepStatistic.first, seizoen, cmbDuration.Text, txtExcelFile.Text, grGrondwaterKlassen, Seizoensnaam, Dates, ExportDir)

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
                        Call Me.Setup.StochastenAnalyse.ClassifyGroundwaterHBVBySeason(ModelParametersClass, enmTimestepStatistic.first, seizoen, cmbDuration.Text, txtExcelFile.Text, grGrondwaterKlassen, Seizoensnaam, Dates, ExportDir)
                    Next
                End If
            End If

            'finally write the POT-values to Excel for future reference
            If Not Setup.TijdreeksStatistiek.POTValuesToExcel() Then Throw New Exception("Error writing POT-values to Excel.")

            'Me.Setup.ExcelFile.Path = ExportDir & "\grondwaterclassificatie.xlsx"
            Me.Setup.ExcelFile.Save(True)

            Me.Close()
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            MsgBox("Error: could Not classify groundwater levels from hisfile contents.")
        End Try

    End Sub

    Private Sub cmbGrootheden_SelectedIndexChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub btnGroeiseizoenHelp_Click(sender As Object, e As EventArgs) Handles btnGroeiseizoenHelp.Click

    End Sub

    Private Sub btnGrootheden_Click(sender As Object, e As EventArgs) Handles btnGrootheden.Click
        MsgBox("Keuze welke grootheid of grootheden te classificeren: lz (lower zone) en/of uz (upper zone) + sm (soil moisture). In het geval van beide wordt de afhankelijkheid tussen upper zone + soil moisture en lower zone expliciet vastgelegd maar resulteert dit wel in meer klassen dan het opgegeven aantal.")
    End Sub

    Private Sub btnPercentilesHelp_Click(sender As Object, e As EventArgs) Handles btnPercentilesHelp.Click
        MsgBox("Definieer hier klassen voor de initiële grondwaterstand. Hanteer getallen tussen 0 en 1 voor de percentielwaarden. Voorbeeld: klasse 'nat' van percentiel 0.75 tot 1.00.")
    End Sub
End Class