Imports STOCHLIB.General
Imports System.Windows.Forms

Public Class frmClassifyTidesByStatistics

    Dim Classificatie As clsTidalClassification
    Dim CSVFile As clsCSVFile

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub

    Private Sub frmClassifyTides_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim i As Integer
        Dim myStr As String
        Dim Naam As String, lBound As Object, uBound As Object

        cmbDuur.Items.Clear()
        cmbDuur.Items.Add("24")
        cmbDuur.Items.Add("48")
        cmbDuur.Items.Add("96")
        cmbDuur.Items.Add("192")
        cmbDuur.Items.Add("216")

        cmbPercentage.Items.Clear()
        cmbPercentage.Items.Add("35")
        cmbPercentage.Items.Add("50")
        cmbPercentage.Items.Add("100")

        cmbTidalComponent.Items.Add("hoogwater")
        cmbTidalComponent.Items.Add("middenstand")
        cmbTidalComponent.Items.Add("laagwater")

        Setup.SetProgress(prProgress, lblProgress)
        Setup.InitializeTijdreeksStatistiek()

        If Not My.Settings.ElevationClasses Is Nothing Then
            For i = 0 To My.Settings.ElevationClasses.Count - 1
                myStr = My.Settings.ElevationClasses.Item(i)
                Naam = Me.Setup.GeneralFunctions.ParseString(myStr, ";")
                lBound = Me.Setup.GeneralFunctions.ParseString(myStr, ";")
                uBound = Me.Setup.GeneralFunctions.ParseString(myStr, ";")
                If IsNumeric(lBound) AndAlso IsNumeric(uBound) Then
                    grPercentileClasses.Rows.Add(Naam, lBound, uBound)
                End If
            Next
        End If

        If Not My.Settings.AmplitudeClasses Is Nothing Then
            For i = 0 To My.Settings.AmplitudeClasses.Count - 1
                myStr = My.Settings.AmplitudeClasses.Item(i)
                Naam = Me.Setup.GeneralFunctions.ParseString(myStr, ";")
                lBound = Me.Setup.GeneralFunctions.ParseString(myStr, ";")
                uBound = Me.Setup.GeneralFunctions.ParseString(myStr, ";")
                If IsNumeric(lBound) AndAlso IsNumeric(uBound) Then
                    grAmplitudeKlassen.Rows.Add(Naam, lBound, uBound)
                End If
            Next
        End If

        txtSeriesName.Text = My.Settings.SeriesName
        txtDateNotation.Text = My.Settings.DateFormatting
        txtTimeNotation.Text = My.Settings.TimeFormatting
        cmbDuur.Text = My.Settings.Duration
        txtUitloop.Text = My.Settings.Uitloop
        cmbPercentage.Text = My.Settings.PercentageDuur
        cmbTidalComponent.Text = My.Settings.TidalComponent
        txtMultiplier.Text = My.Settings.Multiplier

        If My.Settings.TidalClassificationAdvanced Then
            radUitgebreid.Checked = True
        Else
            radEenvoudig.Checked = True
        End If

    End Sub

    Private Sub btnOpenCSV_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpenCSV.Click
        dlgOpenFile.Filter = "csv file|*.csv"
        dlgOpenFile.Title = "Kies een lange reeks met getijden"
        dlgOpenFile.ShowDialog()
        txtCSVFile.Text = dlgOpenFile.FileName
        Dim frmDel As New frmPickDelimiter()
        frmDel.ShowDialog()
        If frmDel.DialogResult = DialogResult.OK Then
            CSVFile = New clsCSVFile(Me.Setup, txtCSVFile.Text, True, frmDel.Delimiter)
            CSVFile.ReadHeader()
            CSVFile.PopulateComboBoxByHeader(cmbDateField)
            CSVFile.PopulateComboBoxByHeader(cmbTimeField)
            CSVFile.PopulateComboBoxByHeader(cmbDataField)
        End If
    End Sub

    Private Sub btnClassify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExport.Click

        dlgSaveFile.InitialDirectory = Me.Setup.GeneralFunctions.DirFromFileName(txtCSVFile.Text)
        dlgSaveFile.Title = "Sla het resultaat op"
        dlgSaveFile.Filter = "Excel-document|*.xlsx"
        dlgSaveFile.ShowDialog()

        If Not dlgSaveFile.FileName = "" Then Me.Setup.ExcelFile.Path = dlgSaveFile.FileName
        Me.Setup.ExcelFile.Save()

    End Sub

    Private Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnImport.Click
        Try
            'store the settings
            My.Settings.DateFormatting = txtDateNotation.Text
            My.Settings.TimeFormatting = txtTimeNotation.Text
            My.Settings.SeriesName = txtSeriesName.Text
            My.Settings.CSVFile = txtCSVFile.Text
            If IsNumeric(txtMultiplier.Text) Then My.Settings.multiplier = txtMultiplier.Text
            My.Settings.Save()

            If txtSeriesName.Text = "" Then Throw New Exception("Specify a name for the series first.")

            Dim Multiplier As Double
            If IsNumeric(txtMultiplier.Text) Then Multiplier = txtMultiplier.Text Else Multiplier = 1

            'read a precipitation series from Excel and write its content directly to a new temporary access database
            If Not Setup.TijdreeksStatistiek.readWaterLevelsFromCSV(txtCSVFile.Text, CSVFile.Delimiter, txtDateNotation.Text, txtTimeNotation.Text, txtSeriesName.Text, cmbDataField.Text, Multiplier, cmbDateField.Text, cmbTimeField.Text) Then
                MsgBox("Error: fout bij het lezen van de CSV-file. Check de instellingen.")
                Me.Setup.Log.write(Setup.Settings.ExportDirRoot & "\logfile.txt", True)
            Else
                btnAnalyze.Enabled = True
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub btnAnalyze_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAnalyze.Click
        Dim PercentileClasses As New Collection
        Dim i As Long

        Try

            'initialize the progress bar
            Setup.SetProgress(prProgress, lblProgress)
            Setup.GeneralFunctions.UpdateProgressBar("Analyzing tidal timeseries...", 0, 10)

            'store my settings
            My.Settings.Duration = cmbDuur.Text
            My.Settings.Uitloop = txtUitloop.Text
            My.Settings.PercentageDuur = cmbPercentage.Text
            My.Settings.TidalComponent = cmbTidalComponent.Text
            If radEenvoudig.Checked Then
                My.Settings.TidalClassificationAdvanced = False
            Else
                My.Settings.TidalClassificationAdvanced = True
            End If

            'clear the existing boundary conditions from the database.


            'create an empty excel document
            Me.Setup.ExcelFile = New STOCHLIB.clsExcelBook(Me.Setup)

            '------------------------------------------------------------------------------------------------------------------------
            'check the validity of the amplitude classes
            If grAmplitudeKlassen.Rows(0).Cells(1).Value <> 0 Then Throw New Exception("Fout: ondergrens van de eerste amplitudeklasse moet 0 bedragen.")
            If grAmplitudeKlassen.Rows(grAmplitudeKlassen.Rows.Count - 1).Cells(2).Value <> 1 Then
                MsgBox("Invoerfout: bovengrens van de laatste amplitudeklasse moet 1 bedragen.")
                Exit Sub
            End If
            For i = 0 To grAmplitudeKlassen.Rows.Count - 2
                If grAmplitudeKlassen.Rows(i + 1).Cells(1).Value <> grAmplitudeKlassen.Rows(i).Cells(2).Value Then
                    MsgBox("Invoerfout: ondergrens van amplitudeklasse " & grAmplitudeKlassen.Rows(i + 1).Cells(0).Value & " moet gelijk zijn aan de bovengrens van " & grAmplitudeKlassen.Rows(i).Cells(0).Value & ".")
                    Exit Sub
                End If
            Next
            '------------------------------------------------------------------------------------------------------------------------

            '------------------------------------------------------------------------------------------------------------------------
            'check the validity of the elevation classes
            If grPercentileClasses.Rows(0).Cells(1).Value <> 0 Then
                MsgBox("Fout: ondergrens van de eerste verhogingsklasse moet 0 bedragen.")
                Exit Sub
            End If
            If grPercentileClasses.Rows(grPercentileClasses.Rows.Count - 1).Cells(2).Value <> 1 Then
                MsgBox("Fout: bovengrens van de laatste verhogingsklasse moet 1 bedragen.")
                Exit Sub
            End If
            For i = 0 To grPercentileClasses.Rows.Count - 2
                If grPercentileClasses.Rows(i + 1).Cells(1).Value <> grPercentileClasses.Rows(i).Cells(2).Value Then
                    MsgBox("Fout: ondergrens van verhogingsklasse " & grPercentileClasses.Rows(i + 1).Cells(0).Value & " moet gelijk zijn aan de bovengrens van " & grPercentileClasses.Rows(i).Cells(0).Value & ".")
                    Exit Sub
                End If
            Next
            '------------------------------------------------------------------------------------------------------------------------

            'store the amplitude classes in memory
            My.Settings.AmplitudeClasses = New System.Collections.Specialized.StringCollection
            For Each myRow As DataGridViewRow In grAmplitudeKlassen.Rows
                My.Settings.AmplitudeClasses.Add(myRow.Cells(0).Value & ";" & myRow.Cells(1).Value & ";" & myRow.Cells(2).Value)
            Next

            'store the elevation classes in memory
            My.Settings.ElevationClasses = New System.Collections.Specialized.StringCollection
            For Each myRow As DataGridViewRow In grPercentileClasses.Rows
                My.Settings.ElevationClasses.Add(myRow.Cells(0).Value & ";" & myRow.Cells(1).Value & ";" & myRow.Cells(2).Value)
            Next

            My.Settings.Save()

            'initialize the classification and store all settings in memory
            Classificatie = New clsTidalClassification(Me.Setup)
            Classificatie.ResponseTime = cmbDuur.Text

            Select Case cmbTidalComponent.Text.Trim.ToLower
                Case Is = "hoogwater"
                    Classificatie.TidalComponent = GeneralFunctions.enmTidalComponent.VerhoogdHoogwater
                Case Is = "middenstand"
                    Classificatie.TidalComponent = GeneralFunctions.enmTidalComponent.VerhoogdeMiddenstand
                Case Is = "laagwater"
                    Classificatie.TidalComponent = GeneralFunctions.enmTidalComponent.VerhoogdLaagwater
            End Select

            'analyze the timeseries and extract all tidal waves
            Classificatie.Analyze()

            'classify the events
            If Not Classificatie.Classify(grAmplitudeKlassen, grPercentileClasses, cmbDuur.Text, txtUitloop.Text, cmbPercentage.Text, radUitgebreid.Checked) Then Throw New Exception("Error classifying tidal series.")
            btnExport.Enabled = True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.write("c:\logfile.txt", True)
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub btnAddMeteoStation_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddElevationClass.Click
        grPercentileClasses.Rows.Add()
    End Sub

    Private Sub btnRemoveMeteoStation_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveElevationClass.Click
        For Each myRow As DataGridViewRow In grPercentileClasses.SelectedRows
            grPercentileClasses.Rows.RemoveAt(myRow.Index)
        Next
    End Sub

    Private Sub btnAddAmplitudeClass_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAddAmplitudeClass.Click
        grAmplitudeKlassen.Rows.Add()
    End Sub

    Private Sub btnRemoveAmplitudeClass_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemoveAmplitudeClass.Click
        For Each myRow As DataGridViewRow In grAmplitudeKlassen.SelectedRows
            grAmplitudeKlassen.Rows.RemoveAt(myRow.Index)
        Next
    End Sub

    Private Sub btnInfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInfo.Click
        MsgBox("Hier classificeert u de mate waarin het hoogwater, laagwater of de middenstand verhoogd is. Let op: de laagste verhogingsklasse wordt behandeld als een klasse zonder enige verhoging. Voor deze klasse wordt per amplitudeklasse slechts een ontwerpgetij aangemaakt, met het gemiddelde hoog- en laagwater.")
    End Sub

    Private Sub btnQAMethod_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnQAMethod.Click
        MsgBox("De eenvoudige methode classificeert uitsluitend achtereenvolgende verhogingen binnen dezelfde klasse, wat tot onderschatting kan leiden. De geavanceerde methode onderscheidt binnen elke gebeurtenis ook waterstanden in verschillende verhogingsklassen. Dit leidt tot een nauwkeuriger kansverdeling maar meer klassen.")
    End Sub

    Private Sub btnDateNotation_Click(sender As Object, e As EventArgs) Handles btnDateNotation.Click
        MsgBox("Datumnotatie als bijv. yyyy/MM/dd HH:mm:ss. Let op: hoofdletters M voor maand.")
    End Sub

    Private Sub btnTimeNotation_Click(sender As Object, e As EventArgs) Handles btnTimeNotation.Click
        MsgBox("Veld optioneel: aparte tijdnotatie als bijv. HH:mm:ss. Let op: hoofdletters H voor uren in 24-uursnotatie.")
    End Sub

    Private Sub BtnWriteDatabase_Click(sender As Object, e As EventArgs)

    End Sub
End Class