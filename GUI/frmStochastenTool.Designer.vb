﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmStochasten
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStochasten))
        Me.mnuMenu = New System.Windows.Forms.MenuStrip()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenXMLToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SaveXMLToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.SaveXMLToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ModelsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ModelToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToevoegenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UitvoerlocatiesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ImporterenToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.AlleVerwijderenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.VoorgedefinieerdToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SumaquasHertogenboschToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.EditToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PasteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GrondwatersClassificerenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SOBEKToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DHydroToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.HBVToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GetijdenClassificerenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DirectoriesHernoemenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.MappenBeherenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.StochastendirectoriesHernoemenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.MappenVerwijderenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.KaartToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.VerschilkaartToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToonPolygonenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AchtergrondkaartToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuGoogleMaps = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuOSM = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuBingMaps = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuHybrid = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuNoMap = New System.Windows.Forms.ToolStripMenuItem()
        Me.GrafiekenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ReferentiepeilenUitShapefileToevoegenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DatabaseToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.UpgradeNaarToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PolygonenUitShapefileToevoegenToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ImporterenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.VolumesUitCSVToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NeerslagpatronenUitCSVToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.NeerslagpatronenkansUitCSVToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.RandvoorwaardenUitCSVToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ConverterenVanAccessNaarSQLiteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LegeDatabaseCreërenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.VerwijderenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.VolumesSTOWA2014ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.VolumesSTOWA2019ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AlleResultatenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.AboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.BetafunctiesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.LeesMATFileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TestGebiedsreductieToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TestNetCDFFileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.tabStochastentool = New System.Windows.Forms.TabControl()
        Me.tabSettings = New System.Windows.Forms.TabPage()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.hlpOutput2 = New System.Windows.Forms.Button()
        Me.chk2D = New System.Windows.Forms.CheckBox()
        Me.chk1D = New System.Windows.Forms.CheckBox()
        Me.grMeteo = New System.Windows.Forms.GroupBox()
        Me.hlpMeteo2 = New System.Windows.Forms.Button()
        Me.btnRemoveMeteoStation = New System.Windows.Forms.Button()
        Me.grMeteoStations = New System.Windows.Forms.DataGridView()
        Me.btnAddMeteoStation = New System.Windows.Forms.Button()
        Me.grNabewerking = New System.Windows.Forms.GroupBox()
        Me.hlpNabewerking2 = New System.Windows.Forms.Button()
        Me.chkRemoveSimulationAfterCopyingResultsFiles = New System.Windows.Forms.CheckBox()
        Me.chkUseCrashedResults = New System.Windows.Forms.CheckBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.txtResultsStartPercentage = New System.Windows.Forms.TextBox()
        Me.cmbClimate = New System.Windows.Forms.ComboBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.grBerekeningen = New System.Windows.Forms.GroupBox()
        Me.txtMaxWaitTimeHours = New System.Windows.Forms.TextBox()
        Me.Label25 = New System.Windows.Forms.Label()
        Me.hlpBerekeningen2 = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtUitloop = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cmbDuration = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtMaxParallel = New System.Windows.Forms.TextBox()
        Me.grBestanden = New System.Windows.Forms.GroupBox()
        Me.hlpVeldZomerpeil2 = New System.Windows.Forms.Button()
        Me.hlpVeldWinterpeil2 = New System.Windows.Forms.Button()
        Me.hlpShapefilePeilgebieden2 = New System.Windows.Forms.Button()
        Me.hlpDatabase2 = New System.Windows.Forms.Button()
        Me.hlpExtraFiles2 = New System.Windows.Forms.Button()
        Me.hlpResultatenmap2 = New System.Windows.Forms.Button()
        Me.hlbUitvoermap2 = New System.Windows.Forms.Button()
        Me.hlpInvoermap2 = New System.Windows.Forms.Button()
        Me.btnExtraFilesDir = New System.Windows.Forms.Button()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.txtExtraFilesDir = New System.Windows.Forms.TextBox()
        Me.btnOutputDir = New System.Windows.Forms.Button()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.txtOutputDir = New System.Windows.Forms.TextBox()
        Me.btnInputDir = New System.Windows.Forms.Button()
        Me.btnResultsDir = New System.Windows.Forms.Button()
        Me.btnDatabase = New System.Windows.Forms.Button()
        Me.btnShapefile = New System.Windows.Forms.Button()
        Me.Label37 = New System.Windows.Forms.Label()
        Me.Label36 = New System.Windows.Forms.Label()
        Me.cmbZomerpeil = New System.Windows.Forms.ComboBox()
        Me.cmbWinterpeil = New System.Windows.Forms.ComboBox()
        Me.txtPeilgebieden = New System.Windows.Forms.TextBox()
        Me.Label35 = New System.Windows.Forms.Label()
        Me.txtResultatenDir = New System.Windows.Forms.TextBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtInputDir = New System.Windows.Forms.TextBox()
        Me.txtDatabase = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.tabSobek = New System.Windows.Forms.TabPage()
        Me.hlpModellen = New System.Windows.Forms.Button()
        Me.btnDeleteModel = New System.Windows.Forms.Button()
        Me.btnAddModel = New System.Windows.Forms.Button()
        Me.grModels = New System.Windows.Forms.DataGridView()
        Me.tabSeizoenen = New System.Windows.Forms.TabPage()
        Me.hlpSeizoenen = New System.Windows.Forms.Button()
        Me.btnRemoveSeason = New System.Windows.Forms.Button()
        Me.btnAddSeason = New System.Windows.Forms.Button()
        Me.lblCheckSumSeizoenen = New System.Windows.Forms.Label()
        Me.grSeizoenen = New System.Windows.Forms.DataGridView()
        Me.tabVolumes = New System.Windows.Forms.TabPage()
        Me.tabPatronen = New System.Windows.Forms.TabPage()
        Me.tabGrondwater = New System.Windows.Forms.TabPage()
        Me.tabBoundaryNodes = New System.Windows.Forms.TabPage()
        Me.GroupBox23 = New System.Windows.Forms.GroupBox()
        Me.btnDeleteBoundaryNode = New System.Windows.Forms.Button()
        Me.btnAddBoundaryNode = New System.Windows.Forms.Button()
        Me.grBoundaryNodes = New System.Windows.Forms.DataGridView()
        Me.tabWaterlevels = New System.Windows.Forms.TabPage()
        Me.grWLChart = New System.Windows.Forms.GroupBox()
        Me.chartBoundaries = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.grWLClasses = New System.Windows.Forms.GroupBox()
        Me.btnCopy = New System.Windows.Forms.Button()
        Me.grWaterLevelSeries = New System.Windows.Forms.DataGridView()
        Me.lblBoundaryChecksum = New System.Windows.Forms.Label()
        Me.btnDeleteBoundaryClass = New System.Windows.Forms.Button()
        Me.btnAddBoundaryClass = New System.Windows.Forms.Button()
        Me.grWaterLevelClasses = New System.Windows.Forms.DataGridView()
        Me.tabWind = New System.Windows.Forms.TabPage()
        Me.GroupBox22 = New System.Windows.Forms.GroupBox()
        Me.grWindSeries = New System.Windows.Forms.DataGridView()
        Me.GroupBox24 = New System.Windows.Forms.GroupBox()
        Me.btnWindCopy = New System.Windows.Forms.Button()
        Me.lblWindChecksum = New System.Windows.Forms.Label()
        Me.btnDeleteWindClass = New System.Windows.Forms.Button()
        Me.btnAddWindClass = New System.Windows.Forms.Button()
        Me.grWindKlassen = New System.Windows.Forms.DataGridView()
        Me.tabExtra = New System.Windows.Forms.TabPage()
        Me.tabPostprocessing = New System.Windows.Forms.TabPage()
        Me.tabOutput = New System.Windows.Forms.TabControl()
        Me.tab1D = New System.Windows.Forms.TabPage()
        Me.grOutputLocations = New System.Windows.Forms.DataGridView()
        Me.btnRemoveOutputLocation = New System.Windows.Forms.Button()
        Me.tab2D = New System.Windows.Forms.TabPage()
        Me.radFou = New System.Windows.Forms.RadioButton()
        Me.TabRuns = New System.Windows.Forms.TabPage()
        Me.btnCopyResults = New System.Windows.Forms.Button()
        Me.btnSimulate = New System.Windows.Forms.Button()
        Me.btnUitlezen = New System.Windows.Forms.Button()
        Me.btnViewer = New System.Windows.Forms.Button()
        Me.btnWissen = New System.Windows.Forms.Button()
        Me.btnPopulateRuns = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.btnPostprocessing = New System.Windows.Forms.Button()
        Me.btnBuild = New System.Windows.Forms.Button()
        Me.lblSelected = New System.Windows.Forms.Label()
        Me.lblnRuns = New System.Windows.Forms.Label()
        Me.lblCheckSumRuns = New System.Windows.Forms.Label()
        Me.grRuns = New System.Windows.Forms.DataGridView()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.GroupBox10 = New System.Windows.Forms.GroupBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.GroupBox11 = New System.Windows.Forms.GroupBox()
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.GroupBox12 = New System.Windows.Forms.GroupBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.ComboBox2 = New System.Windows.Forms.ComboBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.TextBox3 = New System.Windows.Forms.TextBox()
        Me.GroupBox13 = New System.Windows.Forms.GroupBox()
        Me.TextBox4 = New System.Windows.Forms.TextBox()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.TextBox5 = New System.Windows.Forms.TextBox()
        Me.TextBox6 = New System.Windows.Forms.TextBox()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewComboBoxColumn1 = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn5 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.GroupBox14 = New System.Windows.Forms.GroupBox()
        Me.DataGridView2 = New System.Windows.Forms.DataGridView()
        Me.GroupBox15 = New System.Windows.Forms.GroupBox()
        Me.DataGridView3 = New System.Windows.Forms.DataGridView()
        Me.TabPage4 = New System.Windows.Forms.TabPage()
        Me.GroupBox16 = New System.Windows.Forms.GroupBox()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.DataGridView4 = New System.Windows.Forms.DataGridView()
        Me.GroupBox17 = New System.Windows.Forms.GroupBox()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.DataGridView5 = New System.Windows.Forms.DataGridView()
        Me.TabPage5 = New System.Windows.Forms.TabPage()
        Me.GroupBox18 = New System.Windows.Forms.GroupBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.DataGridView6 = New System.Windows.Forms.DataGridView()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.GroupBox19 = New System.Windows.Forms.GroupBox()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.DataGridView7 = New System.Windows.Forms.DataGridView()
        Me.TabPage6 = New System.Windows.Forms.TabPage()
        Me.DataGridView8 = New System.Windows.Forms.DataGridView()
        Me.GroupBox20 = New System.Windows.Forms.GroupBox()
        Me.GroupBox21 = New System.Windows.Forms.GroupBox()
        Me.DataGridView9 = New System.Windows.Forms.DataGridView()
        Me.DataGridView10 = New System.Windows.Forms.DataGridView()
        Me.TabPage7 = New System.Windows.Forms.TabPage()
        Me.DataGridView11 = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn7 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn8 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn9 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn10 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn11 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewComboBoxColumn2 = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.TabPage8 = New System.Windows.Forms.TabPage()
        Me.Button5 = New System.Windows.Forms.Button()
        Me.Label23 = New System.Windows.Forms.Label()
        Me.Button6 = New System.Windows.Forms.Button()
        Me.Label24 = New System.Windows.Forms.Label()
        Me.Button7 = New System.Windows.Forms.Button()
        Me.DataGridView12 = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn12 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn13 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn14 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn15 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn16 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn17 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn18 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn19 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn20 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn21 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn22 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewCheckBoxColumn1 = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.Button8 = New System.Windows.Forms.Button()
        Me.TabPage9 = New System.Windows.Forms.TabPage()
        Me.ComboBox3 = New System.Windows.Forms.ComboBox()
        Me.Button9 = New System.Windows.Forms.Button()
        Me.DataGridView13 = New System.Windows.Forms.DataGridView()
        Me.DataGridView14 = New System.Windows.Forms.DataGridView()
        Me.Button10 = New System.Windows.Forms.Button()
        Me.Button11 = New System.Windows.Forms.Button()
        Me.Button12 = New System.Windows.Forms.Button()
        Me.Button13 = New System.Windows.Forms.Button()
        Me.DataGridView15 = New System.Windows.Forms.DataGridView()
        Me.dlgSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.mnuMenu.SuspendLayout()
        Me.tabStochastentool.SuspendLayout()
        Me.tabSettings.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.grMeteo.SuspendLayout()
        CType(Me.grMeteoStations, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grNabewerking.SuspendLayout()
        Me.grBerekeningen.SuspendLayout()
        Me.grBestanden.SuspendLayout()
        Me.tabSobek.SuspendLayout()
        CType(Me.grModels, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabSeizoenen.SuspendLayout()
        CType(Me.grSeizoenen, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabBoundaryNodes.SuspendLayout()
        Me.GroupBox23.SuspendLayout()
        CType(Me.grBoundaryNodes, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabWaterlevels.SuspendLayout()
        Me.grWLChart.SuspendLayout()
        CType(Me.chartBoundaries, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grWLClasses.SuspendLayout()
        CType(Me.grWaterLevelSeries, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.grWaterLevelClasses, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabWind.SuspendLayout()
        Me.GroupBox22.SuspendLayout()
        CType(Me.grWindSeries, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox24.SuspendLayout()
        CType(Me.grWindKlassen, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabPostprocessing.SuspendLayout()
        Me.tabOutput.SuspendLayout()
        Me.tab1D.SuspendLayout()
        CType(Me.grOutputLocations, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tab2D.SuspendLayout()
        Me.TabRuns.SuspendLayout()
        CType(Me.grRuns, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage1.SuspendLayout()
        Me.GroupBox10.SuspendLayout()
        Me.GroupBox11.SuspendLayout()
        Me.GroupBox12.SuspendLayout()
        Me.GroupBox13.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage3.SuspendLayout()
        Me.GroupBox14.SuspendLayout()
        CType(Me.DataGridView2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox15.SuspendLayout()
        CType(Me.DataGridView3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage4.SuspendLayout()
        Me.GroupBox16.SuspendLayout()
        CType(Me.DataGridView4, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox17.SuspendLayout()
        CType(Me.DataGridView5, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage5.SuspendLayout()
        Me.GroupBox18.SuspendLayout()
        CType(Me.DataGridView6, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox19.SuspendLayout()
        CType(Me.DataGridView7, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage6.SuspendLayout()
        CType(Me.DataGridView8, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox21.SuspendLayout()
        CType(Me.DataGridView9, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DataGridView10, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage7.SuspendLayout()
        CType(Me.DataGridView11, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage8.SuspendLayout()
        CType(Me.DataGridView12, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage9.SuspendLayout()
        CType(Me.DataGridView13, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DataGridView14, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DataGridView15, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'mnuMenu
        '
        Me.mnuMenu.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.mnuMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.ModelsToolStripMenuItem, Me.EditToolStripMenuItem, Me.ToolsToolStripMenuItem, Me.MappenBeherenToolStripMenuItem, Me.KaartToolStripMenuItem, Me.GrafiekenToolStripMenuItem, Me.DatabaseToolStripMenuItem1, Me.AboutToolStripMenuItem, Me.BetafunctiesToolStripMenuItem})
        Me.mnuMenu.Location = New System.Drawing.Point(0, 0)
        Me.mnuMenu.Name = "mnuMenu"
        Me.mnuMenu.Padding = New System.Windows.Forms.Padding(5, 2, 0, 2)
        Me.mnuMenu.Size = New System.Drawing.Size(1709, 28)
        Me.mnuMenu.TabIndex = 4
        Me.mnuMenu.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenXMLToolStripMenuItem, Me.SaveXMLToolStripMenuItem1, Me.SaveXMLToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(76, 24)
        Me.FileToolStripMenuItem.Text = "Bestand"
        '
        'OpenXMLToolStripMenuItem
        '
        Me.OpenXMLToolStripMenuItem.Name = "OpenXMLToolStripMenuItem"
        Me.OpenXMLToolStripMenuItem.Size = New System.Drawing.Size(224, 26)
        Me.OpenXMLToolStripMenuItem.Text = "XML openen"
        '
        'SaveXMLToolStripMenuItem1
        '
        Me.SaveXMLToolStripMenuItem1.Name = "SaveXMLToolStripMenuItem1"
        Me.SaveXMLToolStripMenuItem1.Size = New System.Drawing.Size(224, 26)
        Me.SaveXMLToolStripMenuItem1.Text = "XML opslaan"
        '
        'SaveXMLToolStripMenuItem
        '
        Me.SaveXMLToolStripMenuItem.Name = "SaveXMLToolStripMenuItem"
        Me.SaveXMLToolStripMenuItem.Size = New System.Drawing.Size(224, 26)
        Me.SaveXMLToolStripMenuItem.Text = "Afsluiten"
        '
        'ModelsToolStripMenuItem
        '
        Me.ModelsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ModelToolStripMenuItem, Me.UitvoerlocatiesToolStripMenuItem})
        Me.ModelsToolStripMenuItem.Name = "ModelsToolStripMenuItem"
        Me.ModelsToolStripMenuItem.Size = New System.Drawing.Size(86, 24)
        Me.ModelsToolStripMenuItem.Text = "Modellen"
        '
        'ModelToolStripMenuItem
        '
        Me.ModelToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToevoegenToolStripMenuItem})
        Me.ModelToolStripMenuItem.Name = "ModelToolStripMenuItem"
        Me.ModelToolStripMenuItem.Size = New System.Drawing.Size(257, 26)
        Me.ModelToolStripMenuItem.Text = "Model"
        '
        'ToevoegenToolStripMenuItem
        '
        Me.ToevoegenToolStripMenuItem.Name = "ToevoegenToolStripMenuItem"
        Me.ToevoegenToolStripMenuItem.Size = New System.Drawing.Size(165, 26)
        Me.ToevoegenToolStripMenuItem.Text = "Toevoegen"
        '
        'UitvoerlocatiesToolStripMenuItem
        '
        Me.UitvoerlocatiesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ImporterenToolStripMenuItem1, Me.AlleVerwijderenToolStripMenuItem, Me.VoorgedefinieerdToolStripMenuItem})
        Me.UitvoerlocatiesToolStripMenuItem.Name = "UitvoerlocatiesToolStripMenuItem"
        Me.UitvoerlocatiesToolStripMenuItem.Size = New System.Drawing.Size(257, 26)
        Me.UitvoerlocatiesToolStripMenuItem.Text = "Uitvoerlocaties 0D en 1D"
        '
        'ImporterenToolStripMenuItem1
        '
        Me.ImporterenToolStripMenuItem1.Name = "ImporterenToolStripMenuItem1"
        Me.ImporterenToolStripMenuItem1.Size = New System.Drawing.Size(208, 26)
        Me.ImporterenToolStripMenuItem1.Text = "Importeren"
        '
        'AlleVerwijderenToolStripMenuItem
        '
        Me.AlleVerwijderenToolStripMenuItem.Name = "AlleVerwijderenToolStripMenuItem"
        Me.AlleVerwijderenToolStripMenuItem.Size = New System.Drawing.Size(208, 26)
        Me.AlleVerwijderenToolStripMenuItem.Text = "Verwijderen"
        '
        'VoorgedefinieerdToolStripMenuItem
        '
        Me.VoorgedefinieerdToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SumaquasHertogenboschToolStripMenuItem})
        Me.VoorgedefinieerdToolStripMenuItem.Name = "VoorgedefinieerdToolStripMenuItem"
        Me.VoorgedefinieerdToolStripMenuItem.Size = New System.Drawing.Size(208, 26)
        Me.VoorgedefinieerdToolStripMenuItem.Text = "Voorgedefinieerd"
        '
        'SumaquasHertogenboschToolStripMenuItem
        '
        Me.SumaquasHertogenboschToolStripMenuItem.Name = "SumaquasHertogenboschToolStripMenuItem"
        Me.SumaquasHertogenboschToolStripMenuItem.Size = New System.Drawing.Size(275, 26)
        Me.SumaquasHertogenboschToolStripMenuItem.Text = "Sumaqua 's-Hertogenbosch"
        '
        'EditToolStripMenuItem
        '
        Me.EditToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.PasteToolStripMenuItem})
        Me.EditToolStripMenuItem.Name = "EditToolStripMenuItem"
        Me.EditToolStripMenuItem.Size = New System.Drawing.Size(87, 24)
        Me.EditToolStripMenuItem.Text = "Bewerken"
        '
        'PasteToolStripMenuItem
        '
        Me.PasteToolStripMenuItem.Name = "PasteToolStripMenuItem"
        Me.PasteToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.V), System.Windows.Forms.Keys)
        Me.PasteToolStripMenuItem.Size = New System.Drawing.Size(193, 26)
        Me.PasteToolStripMenuItem.Text = "Plakken"
        '
        'ToolsToolStripMenuItem
        '
        Me.ToolsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.GrondwatersClassificerenToolStripMenuItem, Me.GetijdenClassificerenToolStripMenuItem, Me.DirectoriesHernoemenToolStripMenuItem})
        Me.ToolsToolStripMenuItem.Name = "ToolsToolStripMenuItem"
        Me.ToolsToolStripMenuItem.Size = New System.Drawing.Size(58, 24)
        Me.ToolsToolStripMenuItem.Text = "Tools"
        '
        'GrondwatersClassificerenToolStripMenuItem
        '
        Me.GrondwatersClassificerenToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SOBEKToolStripMenuItem, Me.DHydroToolStripMenuItem, Me.HBVToolStripMenuItem})
        Me.GrondwatersClassificerenToolStripMenuItem.Name = "GrondwatersClassificerenToolStripMenuItem"
        Me.GrondwatersClassificerenToolStripMenuItem.Size = New System.Drawing.Size(300, 26)
        Me.GrondwatersClassificerenToolStripMenuItem.Text = "Grondwater classificeren"
        '
        'SOBEKToolStripMenuItem
        '
        Me.SOBEKToolStripMenuItem.Name = "SOBEKToolStripMenuItem"
        Me.SOBEKToolStripMenuItem.Size = New System.Drawing.Size(150, 26)
        Me.SOBEKToolStripMenuItem.Text = "SOBEK"
        '
        'DHydroToolStripMenuItem
        '
        Me.DHydroToolStripMenuItem.Name = "DHydroToolStripMenuItem"
        Me.DHydroToolStripMenuItem.Size = New System.Drawing.Size(150, 26)
        Me.DHydroToolStripMenuItem.Text = "D-Hydro"
        '
        'HBVToolStripMenuItem
        '
        Me.HBVToolStripMenuItem.Name = "HBVToolStripMenuItem"
        Me.HBVToolStripMenuItem.Size = New System.Drawing.Size(150, 26)
        Me.HBVToolStripMenuItem.Text = "HBV"
        '
        'GetijdenClassificerenToolStripMenuItem
        '
        Me.GetijdenClassificerenToolStripMenuItem.Name = "GetijdenClassificerenToolStripMenuItem"
        Me.GetijdenClassificerenToolStripMenuItem.Size = New System.Drawing.Size(300, 26)
        Me.GetijdenClassificerenToolStripMenuItem.Text = "Getijden statistisch classificeren"
        '
        'DirectoriesHernoemenToolStripMenuItem
        '
        Me.DirectoriesHernoemenToolStripMenuItem.Enabled = False
        Me.DirectoriesHernoemenToolStripMenuItem.Name = "DirectoriesHernoemenToolStripMenuItem"
        Me.DirectoriesHernoemenToolStripMenuItem.Size = New System.Drawing.Size(300, 26)
        Me.DirectoriesHernoemenToolStripMenuItem.Text = "Directories hernoemen"
        '
        'MappenBeherenToolStripMenuItem
        '
        Me.MappenBeherenToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StochastendirectoriesHernoemenToolStripMenuItem, Me.MappenVerwijderenToolStripMenuItem})
        Me.MappenBeherenToolStripMenuItem.Enabled = False
        Me.MappenBeherenToolStripMenuItem.Name = "MappenBeherenToolStripMenuItem"
        Me.MappenBeherenToolStripMenuItem.Size = New System.Drawing.Size(95, 24)
        Me.MappenBeherenToolStripMenuItem.Text = "Directories"
        '
        'StochastendirectoriesHernoemenToolStripMenuItem
        '
        Me.StochastendirectoriesHernoemenToolStripMenuItem.Name = "StochastendirectoriesHernoemenToolStripMenuItem"
        Me.StochastendirectoriesHernoemenToolStripMenuItem.Size = New System.Drawing.Size(325, 26)
        Me.StochastendirectoriesHernoemenToolStripMenuItem.Text = "In stochastendirectory hernoemen"
        '
        'MappenVerwijderenToolStripMenuItem
        '
        Me.MappenVerwijderenToolStripMenuItem.Name = "MappenVerwijderenToolStripMenuItem"
        Me.MappenVerwijderenToolStripMenuItem.Size = New System.Drawing.Size(325, 26)
        Me.MappenVerwijderenToolStripMenuItem.Text = "Uit stochastendirectory verwijderen"
        '
        'KaartToolStripMenuItem
        '
        Me.KaartToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.VerschilkaartToolStripMenuItem, Me.ToonPolygonenToolStripMenuItem, Me.AchtergrondkaartToolStripMenuItem})
        Me.KaartToolStripMenuItem.Enabled = False
        Me.KaartToolStripMenuItem.Name = "KaartToolStripMenuItem"
        Me.KaartToolStripMenuItem.Size = New System.Drawing.Size(58, 24)
        Me.KaartToolStripMenuItem.Text = "Kaart"
        '
        'VerschilkaartToolStripMenuItem
        '
        Me.VerschilkaartToolStripMenuItem.Checked = True
        Me.VerschilkaartToolStripMenuItem.CheckOnClick = True
        Me.VerschilkaartToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.VerschilkaartToolStripMenuItem.Name = "VerschilkaartToolStripMenuItem"
        Me.VerschilkaartToolStripMenuItem.Size = New System.Drawing.Size(305, 26)
        Me.VerschilkaartToolStripMenuItem.Text = "Toon verschil met referentiecase"
        '
        'ToonPolygonenToolStripMenuItem
        '
        Me.ToonPolygonenToolStripMenuItem.CheckOnClick = True
        Me.ToonPolygonenToolStripMenuItem.Name = "ToonPolygonenToolStripMenuItem"
        Me.ToonPolygonenToolStripMenuItem.Size = New System.Drawing.Size(305, 26)
        Me.ToonPolygonenToolStripMenuItem.Text = "Toon polygonen als overlay"
        '
        'AchtergrondkaartToolStripMenuItem
        '
        Me.AchtergrondkaartToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuGoogleMaps, Me.mnuOSM, Me.mnuBingMaps, Me.mnuHybrid, Me.mnuNoMap})
        Me.AchtergrondkaartToolStripMenuItem.Name = "AchtergrondkaartToolStripMenuItem"
        Me.AchtergrondkaartToolStripMenuItem.Size = New System.Drawing.Size(305, 26)
        Me.AchtergrondkaartToolStripMenuItem.Text = "Achtergrondkaart"
        '
        'mnuGoogleMaps
        '
        Me.mnuGoogleMaps.Checked = True
        Me.mnuGoogleMaps.CheckOnClick = True
        Me.mnuGoogleMaps.CheckState = System.Windows.Forms.CheckState.Checked
        Me.mnuGoogleMaps.Name = "mnuGoogleMaps"
        Me.mnuGoogleMaps.Size = New System.Drawing.Size(203, 26)
        Me.mnuGoogleMaps.Text = "Google Maps"
        '
        'mnuOSM
        '
        Me.mnuOSM.Name = "mnuOSM"
        Me.mnuOSM.Size = New System.Drawing.Size(203, 26)
        Me.mnuOSM.Text = "OpenStreetMaps"
        '
        'mnuBingMaps
        '
        Me.mnuBingMaps.CheckOnClick = True
        Me.mnuBingMaps.Name = "mnuBingMaps"
        Me.mnuBingMaps.Size = New System.Drawing.Size(203, 26)
        Me.mnuBingMaps.Text = "Bing Maps"
        '
        'mnuHybrid
        '
        Me.mnuHybrid.CheckOnClick = True
        Me.mnuHybrid.Name = "mnuHybrid"
        Me.mnuHybrid.Size = New System.Drawing.Size(203, 26)
        Me.mnuHybrid.Text = "Bing Hybrid"
        '
        'mnuNoMap
        '
        Me.mnuNoMap.Name = "mnuNoMap"
        Me.mnuNoMap.Size = New System.Drawing.Size(203, 26)
        Me.mnuNoMap.Text = "Geen"
        '
        'GrafiekenToolStripMenuItem
        '
        Me.GrafiekenToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ReferentiepeilenUitShapefileToevoegenToolStripMenuItem})
        Me.GrafiekenToolStripMenuItem.Enabled = False
        Me.GrafiekenToolStripMenuItem.Name = "GrafiekenToolStripMenuItem"
        Me.GrafiekenToolStripMenuItem.Size = New System.Drawing.Size(86, 24)
        Me.GrafiekenToolStripMenuItem.Text = "Grafieken"
        '
        'ReferentiepeilenUitShapefileToevoegenToolStripMenuItem
        '
        Me.ReferentiepeilenUitShapefileToevoegenToolStripMenuItem.Name = "ReferentiepeilenUitShapefileToevoegenToolStripMenuItem"
        Me.ReferentiepeilenUitShapefileToevoegenToolStripMenuItem.Size = New System.Drawing.Size(361, 26)
        Me.ReferentiepeilenUitShapefileToevoegenToolStripMenuItem.Text = "Referentiepeilen uit shapefile toevoegen"
        '
        'DatabaseToolStripMenuItem1
        '
        Me.DatabaseToolStripMenuItem1.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.UpgradeNaarToolStripMenuItem, Me.PolygonenUitShapefileToevoegenToolStripMenuItem1, Me.ImporterenToolStripMenuItem, Me.ConverterenVanAccessNaarSQLiteToolStripMenuItem, Me.LegeDatabaseCreërenToolStripMenuItem, Me.VerwijderenToolStripMenuItem})
        Me.DatabaseToolStripMenuItem1.Name = "DatabaseToolStripMenuItem1"
        Me.DatabaseToolStripMenuItem1.Size = New System.Drawing.Size(86, 24)
        Me.DatabaseToolStripMenuItem1.Text = "Database"
        '
        'UpgradeNaarToolStripMenuItem
        '
        Me.UpgradeNaarToolStripMenuItem.Name = "UpgradeNaarToolStripMenuItem"
        Me.UpgradeNaarToolStripMenuItem.Size = New System.Drawing.Size(327, 26)
        Me.UpgradeNaarToolStripMenuItem.Text = "Upgrade naar v5.0"
        '
        'PolygonenUitShapefileToevoegenToolStripMenuItem1
        '
        Me.PolygonenUitShapefileToevoegenToolStripMenuItem1.Name = "PolygonenUitShapefileToevoegenToolStripMenuItem1"
        Me.PolygonenUitShapefileToevoegenToolStripMenuItem1.Size = New System.Drawing.Size(327, 26)
        Me.PolygonenUitShapefileToevoegenToolStripMenuItem1.Text = "Polygonen uit shapefile toevoegen"
        '
        'ImporterenToolStripMenuItem
        '
        Me.ImporterenToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.VolumesUitCSVToolStripMenuItem, Me.NeerslagpatronenUitCSVToolStripMenuItem, Me.NeerslagpatronenkansUitCSVToolStripMenuItem, Me.RandvoorwaardenUitCSVToolStripMenuItem})
        Me.ImporterenToolStripMenuItem.Name = "ImporterenToolStripMenuItem"
        Me.ImporterenToolStripMenuItem.Size = New System.Drawing.Size(327, 26)
        Me.ImporterenToolStripMenuItem.Text = "Importeren"
        '
        'VolumesUitCSVToolStripMenuItem
        '
        Me.VolumesUitCSVToolStripMenuItem.Name = "VolumesUitCSVToolStripMenuItem"
        Me.VolumesUitCSVToolStripMenuItem.Size = New System.Drawing.Size(327, 26)
        Me.VolumesUitCSVToolStripMenuItem.Text = "Neerslagvolumes uit CSV"
        '
        'NeerslagpatronenUitCSVToolStripMenuItem
        '
        Me.NeerslagpatronenUitCSVToolStripMenuItem.Name = "NeerslagpatronenUitCSVToolStripMenuItem"
        Me.NeerslagpatronenUitCSVToolStripMenuItem.Size = New System.Drawing.Size(327, 26)
        Me.NeerslagpatronenUitCSVToolStripMenuItem.Text = "Neerslagpatronen (verloop) uit CSV"
        '
        'NeerslagpatronenkansUitCSVToolStripMenuItem
        '
        Me.NeerslagpatronenkansUitCSVToolStripMenuItem.Name = "NeerslagpatronenkansUitCSVToolStripMenuItem"
        Me.NeerslagpatronenkansUitCSVToolStripMenuItem.Size = New System.Drawing.Size(327, 26)
        Me.NeerslagpatronenkansUitCSVToolStripMenuItem.Text = "Neerslagpatronen (kans) uit CSV"
        '
        'RandvoorwaardenUitCSVToolStripMenuItem
        '
        Me.RandvoorwaardenUitCSVToolStripMenuItem.Name = "RandvoorwaardenUitCSVToolStripMenuItem"
        Me.RandvoorwaardenUitCSVToolStripMenuItem.Size = New System.Drawing.Size(327, 26)
        Me.RandvoorwaardenUitCSVToolStripMenuItem.Text = "Randvoorwaarden uit CSV"
        '
        'ConverterenVanAccessNaarSQLiteToolStripMenuItem
        '
        Me.ConverterenVanAccessNaarSQLiteToolStripMenuItem.Name = "ConverterenVanAccessNaarSQLiteToolStripMenuItem"
        Me.ConverterenVanAccessNaarSQLiteToolStripMenuItem.Size = New System.Drawing.Size(327, 26)
        Me.ConverterenVanAccessNaarSQLiteToolStripMenuItem.Text = "Converteren van Access naar SQLite"
        '
        'LegeDatabaseCreërenToolStripMenuItem
        '
        Me.LegeDatabaseCreërenToolStripMenuItem.Name = "LegeDatabaseCreërenToolStripMenuItem"
        Me.LegeDatabaseCreërenToolStripMenuItem.Size = New System.Drawing.Size(327, 26)
        Me.LegeDatabaseCreërenToolStripMenuItem.Text = "Nieuwe database creëren"
        '
        'VerwijderenToolStripMenuItem
        '
        Me.VerwijderenToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.VolumesSTOWA2014ToolStripMenuItem, Me.VolumesSTOWA2019ToolStripMenuItem, Me.AlleResultatenToolStripMenuItem})
        Me.VerwijderenToolStripMenuItem.Name = "VerwijderenToolStripMenuItem"
        Me.VerwijderenToolStripMenuItem.Size = New System.Drawing.Size(327, 26)
        Me.VerwijderenToolStripMenuItem.Text = "Verwijderen"
        '
        'VolumesSTOWA2014ToolStripMenuItem
        '
        Me.VolumesSTOWA2014ToolStripMenuItem.Name = "VolumesSTOWA2014ToolStripMenuItem"
        Me.VolumesSTOWA2014ToolStripMenuItem.Size = New System.Drawing.Size(237, 26)
        Me.VolumesSTOWA2014ToolStripMenuItem.Text = "Volumes STOWA 2014"
        '
        'VolumesSTOWA2019ToolStripMenuItem
        '
        Me.VolumesSTOWA2019ToolStripMenuItem.Name = "VolumesSTOWA2019ToolStripMenuItem"
        Me.VolumesSTOWA2019ToolStripMenuItem.Size = New System.Drawing.Size(237, 26)
        Me.VolumesSTOWA2019ToolStripMenuItem.Text = "Volumes STOWA 2019"
        '
        'AlleResultatenToolStripMenuItem
        '
        Me.AlleResultatenToolStripMenuItem.Name = "AlleResultatenToolStripMenuItem"
        Me.AlleResultatenToolStripMenuItem.Size = New System.Drawing.Size(237, 26)
        Me.AlleResultatenToolStripMenuItem.Text = "Alle resultaten"
        '
        'AboutToolStripMenuItem
        '
        Me.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem"
        Me.AboutToolStripMenuItem.Size = New System.Drawing.Size(54, 24)
        Me.AboutToolStripMenuItem.Text = "Over"
        '
        'BetafunctiesToolStripMenuItem
        '
        Me.BetafunctiesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LeesMATFileToolStripMenuItem, Me.TestGebiedsreductieToolStripMenuItem, Me.TestNetCDFFileToolStripMenuItem})
        Me.BetafunctiesToolStripMenuItem.Name = "BetafunctiesToolStripMenuItem"
        Me.BetafunctiesToolStripMenuItem.Size = New System.Drawing.Size(81, 24)
        Me.BetafunctiesToolStripMenuItem.Text = "Sandbox"
        '
        'LeesMATFileToolStripMenuItem
        '
        Me.LeesMATFileToolStripMenuItem.Name = "LeesMATFileToolStripMenuItem"
        Me.LeesMATFileToolStripMenuItem.Size = New System.Drawing.Size(229, 26)
        Me.LeesMATFileToolStripMenuItem.Text = "Lees .MAT file"
        '
        'TestGebiedsreductieToolStripMenuItem
        '
        Me.TestGebiedsreductieToolStripMenuItem.Name = "TestGebiedsreductieToolStripMenuItem"
        Me.TestGebiedsreductieToolStripMenuItem.Size = New System.Drawing.Size(229, 26)
        Me.TestGebiedsreductieToolStripMenuItem.Text = "Test gebiedsreductie"
        '
        'TestNetCDFFileToolStripMenuItem
        '
        Me.TestNetCDFFileToolStripMenuItem.Name = "TestNetCDFFileToolStripMenuItem"
        Me.TestNetCDFFileToolStripMenuItem.Size = New System.Drawing.Size(229, 26)
        Me.TestNetCDFFileToolStripMenuItem.Text = "Test NetCDF file"
        '
        'tabStochastentool
        '
        Me.tabStochastentool.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tabStochastentool.Controls.Add(Me.tabSettings)
        Me.tabStochastentool.Controls.Add(Me.tabSobek)
        Me.tabStochastentool.Controls.Add(Me.tabSeizoenen)
        Me.tabStochastentool.Controls.Add(Me.tabVolumes)
        Me.tabStochastentool.Controls.Add(Me.tabPatronen)
        Me.tabStochastentool.Controls.Add(Me.tabGrondwater)
        Me.tabStochastentool.Controls.Add(Me.tabBoundaryNodes)
        Me.tabStochastentool.Controls.Add(Me.tabWaterlevels)
        Me.tabStochastentool.Controls.Add(Me.tabWind)
        Me.tabStochastentool.Controls.Add(Me.tabExtra)
        Me.tabStochastentool.Controls.Add(Me.tabPostprocessing)
        Me.tabStochastentool.Controls.Add(Me.TabRuns)
        Me.tabStochastentool.Location = New System.Drawing.Point(13, 33)
        Me.tabStochastentool.Margin = New System.Windows.Forms.Padding(4)
        Me.tabStochastentool.Name = "tabStochastentool"
        Me.tabStochastentool.SelectedIndex = 0
        Me.tabStochastentool.Size = New System.Drawing.Size(1683, 619)
        Me.tabStochastentool.TabIndex = 22
        '
        'tabSettings
        '
        Me.tabSettings.Controls.Add(Me.GroupBox1)
        Me.tabSettings.Controls.Add(Me.grMeteo)
        Me.tabSettings.Controls.Add(Me.grNabewerking)
        Me.tabSettings.Controls.Add(Me.grBerekeningen)
        Me.tabSettings.Controls.Add(Me.grBestanden)
        Me.tabSettings.Location = New System.Drawing.Point(4, 25)
        Me.tabSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.tabSettings.Name = "tabSettings"
        Me.tabSettings.Padding = New System.Windows.Forms.Padding(4)
        Me.tabSettings.Size = New System.Drawing.Size(1675, 590)
        Me.tabSettings.TabIndex = 1
        Me.tabSettings.Text = "Algemeen"
        Me.tabSettings.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Controls.Add(Me.hlpOutput2)
        Me.GroupBox1.Controls.Add(Me.chk2D)
        Me.GroupBox1.Controls.Add(Me.chk1D)
        Me.GroupBox1.Location = New System.Drawing.Point(701, 491)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.GroupBox1.Size = New System.Drawing.Size(328, 88)
        Me.GroupBox1.TabIndex = 41
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Output"
        '
        'hlpOutput2
        '
        Me.hlpOutput2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpOutput2.BackColor = System.Drawing.Color.Gold
        Me.hlpOutput2.Location = New System.Drawing.Point(288, 21)
        Me.hlpOutput2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpOutput2.Name = "hlpOutput2"
        Me.hlpOutput2.Size = New System.Drawing.Size(33, 31)
        Me.hlpOutput2.TabIndex = 73
        Me.hlpOutput2.Text = "?"
        Me.hlpOutput2.UseVisualStyleBackColor = False
        '
        'chk2D
        '
        Me.chk2D.AutoSize = True
        Me.chk2D.Location = New System.Drawing.Point(15, 55)
        Me.chk2D.Margin = New System.Windows.Forms.Padding(4)
        Me.chk2D.Name = "chk2D"
        Me.chk2D.Size = New System.Drawing.Size(107, 20)
        Me.chk2D.TabIndex = 40
        Me.chk2D.Text = "resultaten 2D"
        Me.chk2D.UseVisualStyleBackColor = True
        '
        'chk1D
        '
        Me.chk1D.AutoSize = True
        Me.chk1D.Checked = True
        Me.chk1D.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chk1D.Location = New System.Drawing.Point(15, 27)
        Me.chk1D.Margin = New System.Windows.Forms.Padding(4)
        Me.chk1D.Name = "chk1D"
        Me.chk1D.Size = New System.Drawing.Size(107, 20)
        Me.chk1D.TabIndex = 38
        Me.chk1D.Text = "resultaten 1D"
        Me.chk1D.UseVisualStyleBackColor = True
        '
        'grMeteo
        '
        Me.grMeteo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.grMeteo.AutoSize = True
        Me.grMeteo.Controls.Add(Me.hlpMeteo2)
        Me.grMeteo.Controls.Add(Me.btnRemoveMeteoStation)
        Me.grMeteo.Controls.Add(Me.grMeteoStations)
        Me.grMeteo.Controls.Add(Me.btnAddMeteoStation)
        Me.grMeteo.Location = New System.Drawing.Point(8, 314)
        Me.grMeteo.Margin = New System.Windows.Forms.Padding(4)
        Me.grMeteo.Name = "grMeteo"
        Me.grMeteo.Padding = New System.Windows.Forms.Padding(4)
        Me.grMeteo.Size = New System.Drawing.Size(685, 267)
        Me.grMeteo.TabIndex = 40
        Me.grMeteo.TabStop = False
        Me.grMeteo.Text = "Meteo"
        '
        'hlpMeteo2
        '
        Me.hlpMeteo2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpMeteo2.BackColor = System.Drawing.Color.Gold
        Me.hlpMeteo2.Location = New System.Drawing.Point(644, 22)
        Me.hlpMeteo2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpMeteo2.Name = "hlpMeteo2"
        Me.hlpMeteo2.Size = New System.Drawing.Size(33, 31)
        Me.hlpMeteo2.TabIndex = 71
        Me.hlpMeteo2.Text = "?"
        Me.hlpMeteo2.UseVisualStyleBackColor = False
        '
        'btnRemoveMeteoStation
        '
        Me.btnRemoveMeteoStation.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnRemoveMeteoStation.BackColor = System.Drawing.Color.IndianRed
        Me.btnRemoveMeteoStation.Location = New System.Drawing.Point(644, 96)
        Me.btnRemoveMeteoStation.Margin = New System.Windows.Forms.Padding(4)
        Me.btnRemoveMeteoStation.Name = "btnRemoveMeteoStation"
        Me.btnRemoveMeteoStation.Size = New System.Drawing.Size(33, 31)
        Me.btnRemoveMeteoStation.TabIndex = 42
        Me.btnRemoveMeteoStation.Text = "-"
        Me.btnRemoveMeteoStation.UseVisualStyleBackColor = False
        '
        'grMeteoStations
        '
        Me.grMeteoStations.AllowUserToAddRows = False
        Me.grMeteoStations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grMeteoStations.Location = New System.Drawing.Point(16, 23)
        Me.grMeteoStations.Margin = New System.Windows.Forms.Padding(4)
        Me.grMeteoStations.Name = "grMeteoStations"
        Me.grMeteoStations.RowHeadersWidth = 51
        Me.grMeteoStations.Size = New System.Drawing.Size(620, 218)
        Me.grMeteoStations.TabIndex = 0
        '
        'btnAddMeteoStation
        '
        Me.btnAddMeteoStation.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnAddMeteoStation.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnAddMeteoStation.Location = New System.Drawing.Point(644, 59)
        Me.btnAddMeteoStation.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAddMeteoStation.Name = "btnAddMeteoStation"
        Me.btnAddMeteoStation.Size = New System.Drawing.Size(33, 31)
        Me.btnAddMeteoStation.TabIndex = 41
        Me.btnAddMeteoStation.Text = "+"
        Me.btnAddMeteoStation.UseVisualStyleBackColor = False
        '
        'grNabewerking
        '
        Me.grNabewerking.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grNabewerking.AutoSize = True
        Me.grNabewerking.Controls.Add(Me.hlpNabewerking2)
        Me.grNabewerking.Controls.Add(Me.chkRemoveSimulationAfterCopyingResultsFiles)
        Me.grNabewerking.Controls.Add(Me.chkUseCrashedResults)
        Me.grNabewerking.Controls.Add(Me.Label9)
        Me.grNabewerking.Controls.Add(Me.txtResultsStartPercentage)
        Me.grNabewerking.Controls.Add(Me.cmbClimate)
        Me.grNabewerking.Controls.Add(Me.Label8)
        Me.grNabewerking.Location = New System.Drawing.Point(1037, 314)
        Me.grNabewerking.Margin = New System.Windows.Forms.Padding(4)
        Me.grNabewerking.Name = "grNabewerking"
        Me.grNabewerking.Padding = New System.Windows.Forms.Padding(4)
        Me.grNabewerking.Size = New System.Drawing.Size(627, 266)
        Me.grNabewerking.TabIndex = 39
        Me.grNabewerking.TabStop = False
        Me.grNabewerking.Text = "Nabewerking"
        '
        'hlpNabewerking2
        '
        Me.hlpNabewerking2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpNabewerking2.BackColor = System.Drawing.Color.Gold
        Me.hlpNabewerking2.Location = New System.Drawing.Point(587, 20)
        Me.hlpNabewerking2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpNabewerking2.Name = "hlpNabewerking2"
        Me.hlpNabewerking2.Size = New System.Drawing.Size(33, 31)
        Me.hlpNabewerking2.TabIndex = 74
        Me.hlpNabewerking2.Text = "?"
        Me.hlpNabewerking2.UseVisualStyleBackColor = False
        '
        'chkRemoveSimulationAfterCopyingResultsFiles
        '
        Me.chkRemoveSimulationAfterCopyingResultsFiles.AutoSize = True
        Me.chkRemoveSimulationAfterCopyingResultsFiles.Location = New System.Drawing.Point(12, 129)
        Me.chkRemoveSimulationAfterCopyingResultsFiles.Margin = New System.Windows.Forms.Padding(4)
        Me.chkRemoveSimulationAfterCopyingResultsFiles.Name = "chkRemoveSimulationAfterCopyingResultsFiles"
        Me.chkRemoveSimulationAfterCopyingResultsFiles.Size = New System.Drawing.Size(348, 20)
        Me.chkRemoveSimulationAfterCopyingResultsFiles.TabIndex = 38
        Me.chkRemoveSimulationAfterCopyingResultsFiles.Text = "Simulatie verwijderen na kopiëren resultaatbestanden"
        Me.chkRemoveSimulationAfterCopyingResultsFiles.UseVisualStyleBackColor = True
        '
        'chkUseCrashedResults
        '
        Me.chkUseCrashedResults.AutoSize = True
        Me.chkUseCrashedResults.Location = New System.Drawing.Point(12, 101)
        Me.chkUseCrashedResults.Margin = New System.Windows.Forms.Padding(4)
        Me.chkUseCrashedResults.Name = "chkUseCrashedResults"
        Me.chkUseCrashedResults.Size = New System.Drawing.Size(288, 20)
        Me.chkUseCrashedResults.TabIndex = 33
        Me.chkUseCrashedResults.Text = "Ook resultaten gecrashte sommen toestaan"
        Me.chkUseCrashedResults.UseVisualStyleBackColor = True
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(8, 68)
        Me.Label9.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(206, 16)
        Me.Label9.TabIndex = 33
        Me.Label9.Text = "Lees resultaten vanaf percentage"
        '
        'txtResultsStartPercentage
        '
        Me.txtResultsStartPercentage.Location = New System.Drawing.Point(260, 64)
        Me.txtResultsStartPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtResultsStartPercentage.Name = "txtResultsStartPercentage"
        Me.txtResultsStartPercentage.Size = New System.Drawing.Size(59, 22)
        Me.txtResultsStartPercentage.TabIndex = 37
        '
        'cmbClimate
        '
        Me.cmbClimate.FormattingEnabled = True
        Me.cmbClimate.Location = New System.Drawing.Point(204, 30)
        Me.cmbClimate.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbClimate.Name = "cmbClimate"
        Me.cmbClimate.Size = New System.Drawing.Size(275, 24)
        Me.cmbClimate.TabIndex = 35
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(8, 32)
        Me.Label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(103, 16)
        Me.Label8.TabIndex = 36
        Me.Label8.Text = "Klimaatscenario"
        '
        'grBerekeningen
        '
        Me.grBerekeningen.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.grBerekeningen.AutoSize = True
        Me.grBerekeningen.Controls.Add(Me.txtMaxWaitTimeHours)
        Me.grBerekeningen.Controls.Add(Me.Label25)
        Me.grBerekeningen.Controls.Add(Me.hlpBerekeningen2)
        Me.grBerekeningen.Controls.Add(Me.Label2)
        Me.grBerekeningen.Controls.Add(Me.txtUitloop)
        Me.grBerekeningen.Controls.Add(Me.Label1)
        Me.grBerekeningen.Controls.Add(Me.cmbDuration)
        Me.grBerekeningen.Controls.Add(Me.Label4)
        Me.grBerekeningen.Controls.Add(Me.txtMaxParallel)
        Me.grBerekeningen.Location = New System.Drawing.Point(701, 314)
        Me.grBerekeningen.Margin = New System.Windows.Forms.Padding(4)
        Me.grBerekeningen.Name = "grBerekeningen"
        Me.grBerekeningen.Padding = New System.Windows.Forms.Padding(4)
        Me.grBerekeningen.Size = New System.Drawing.Size(328, 171)
        Me.grBerekeningen.TabIndex = 38
        Me.grBerekeningen.TabStop = False
        Me.grBerekeningen.Text = "Berekeningen"
        '
        'txtMaxWaitTimeHours
        '
        Me.txtMaxWaitTimeHours.Location = New System.Drawing.Point(193, 124)
        Me.txtMaxWaitTimeHours.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxWaitTimeHours.Name = "txtMaxWaitTimeHours"
        Me.txtMaxWaitTimeHours.Size = New System.Drawing.Size(73, 22)
        Me.txtMaxWaitTimeHours.TabIndex = 74
        Me.txtMaxWaitTimeHours.Text = "1"
        '
        'Label25
        '
        Me.Label25.AutoSize = True
        Me.Label25.Location = New System.Drawing.Point(12, 126)
        Me.Label25.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label25.Name = "Label25"
        Me.Label25.Size = New System.Drawing.Size(159, 16)
        Me.Label25.TabIndex = 73
        Me.Label25.Text = "Maximale wachttijd (uren):"
        '
        'hlpBerekeningen2
        '
        Me.hlpBerekeningen2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpBerekeningen2.BackColor = System.Drawing.Color.Gold
        Me.hlpBerekeningen2.Location = New System.Drawing.Point(288, 20)
        Me.hlpBerekeningen2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpBerekeningen2.Name = "hlpBerekeningen2"
        Me.hlpBerekeningen2.Size = New System.Drawing.Size(33, 31)
        Me.hlpBerekeningen2.TabIndex = 72
        Me.hlpBerekeningen2.Text = "?"
        Me.hlpBerekeningen2.UseVisualStyleBackColor = False
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 64)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(143, 16)
        Me.Label2.TabIndex = 32
        Me.Label2.Text = "Uitloop neerslag (uren)"
        '
        'txtUitloop
        '
        Me.txtUitloop.Enabled = False
        Me.txtUitloop.Location = New System.Drawing.Point(193, 62)
        Me.txtUitloop.Margin = New System.Windows.Forms.Padding(4)
        Me.txtUitloop.Name = "txtUitloop"
        Me.txtUitloop.Size = New System.Drawing.Size(73, 22)
        Me.txtUitloop.TabIndex = 9
        Me.txtUitloop.Text = "48"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(15, 33)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(126, 16)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "Neerslagduur (uren)"
        '
        'cmbDuration
        '
        Me.cmbDuration.FormattingEnabled = True
        Me.cmbDuration.Location = New System.Drawing.Point(193, 29)
        Me.cmbDuration.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbDuration.Name = "cmbDuration"
        Me.cmbDuration.Size = New System.Drawing.Size(73, 24)
        Me.cmbDuration.TabIndex = 0
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 95)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(150, 16)
        Me.Label4.TabIndex = 31
        Me.Label4.Text = "Parallelle berekeningen"
        '
        'txtMaxParallel
        '
        Me.txtMaxParallel.Location = New System.Drawing.Point(193, 93)
        Me.txtMaxParallel.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxParallel.Name = "txtMaxParallel"
        Me.txtMaxParallel.Size = New System.Drawing.Size(73, 22)
        Me.txtMaxParallel.TabIndex = 28
        Me.txtMaxParallel.Text = "4"
        '
        'grBestanden
        '
        Me.grBestanden.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grBestanden.Controls.Add(Me.hlpVeldZomerpeil2)
        Me.grBestanden.Controls.Add(Me.hlpVeldWinterpeil2)
        Me.grBestanden.Controls.Add(Me.hlpShapefilePeilgebieden2)
        Me.grBestanden.Controls.Add(Me.hlpDatabase2)
        Me.grBestanden.Controls.Add(Me.hlpExtraFiles2)
        Me.grBestanden.Controls.Add(Me.hlpResultatenmap2)
        Me.grBestanden.Controls.Add(Me.hlbUitvoermap2)
        Me.grBestanden.Controls.Add(Me.hlpInvoermap2)
        Me.grBestanden.Controls.Add(Me.btnExtraFilesDir)
        Me.grBestanden.Controls.Add(Me.Label16)
        Me.grBestanden.Controls.Add(Me.txtExtraFilesDir)
        Me.grBestanden.Controls.Add(Me.btnOutputDir)
        Me.grBestanden.Controls.Add(Me.Label15)
        Me.grBestanden.Controls.Add(Me.txtOutputDir)
        Me.grBestanden.Controls.Add(Me.btnInputDir)
        Me.grBestanden.Controls.Add(Me.btnResultsDir)
        Me.grBestanden.Controls.Add(Me.btnDatabase)
        Me.grBestanden.Controls.Add(Me.btnShapefile)
        Me.grBestanden.Controls.Add(Me.Label37)
        Me.grBestanden.Controls.Add(Me.Label36)
        Me.grBestanden.Controls.Add(Me.cmbZomerpeil)
        Me.grBestanden.Controls.Add(Me.cmbWinterpeil)
        Me.grBestanden.Controls.Add(Me.txtPeilgebieden)
        Me.grBestanden.Controls.Add(Me.Label35)
        Me.grBestanden.Controls.Add(Me.txtResultatenDir)
        Me.grBestanden.Controls.Add(Me.Label11)
        Me.grBestanden.Controls.Add(Me.Label5)
        Me.grBestanden.Controls.Add(Me.txtInputDir)
        Me.grBestanden.Controls.Add(Me.txtDatabase)
        Me.grBestanden.Controls.Add(Me.Label7)
        Me.grBestanden.Location = New System.Drawing.Point(8, 10)
        Me.grBestanden.Margin = New System.Windows.Forms.Padding(4)
        Me.grBestanden.Name = "grBestanden"
        Me.grBestanden.Padding = New System.Windows.Forms.Padding(4)
        Me.grBestanden.Size = New System.Drawing.Size(1656, 292)
        Me.grBestanden.TabIndex = 37
        Me.grBestanden.TabStop = False
        Me.grBestanden.Text = "Bestanden en Mappen"
        '
        'hlpVeldZomerpeil2
        '
        Me.hlpVeldZomerpeil2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpVeldZomerpeil2.BackColor = System.Drawing.Color.Gold
        Me.hlpVeldZomerpeil2.Location = New System.Drawing.Point(1623, 238)
        Me.hlpVeldZomerpeil2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpVeldZomerpeil2.Name = "hlpVeldZomerpeil2"
        Me.hlpVeldZomerpeil2.Size = New System.Drawing.Size(33, 31)
        Me.hlpVeldZomerpeil2.TabIndex = 70
        Me.hlpVeldZomerpeil2.Text = "?"
        Me.hlpVeldZomerpeil2.UseVisualStyleBackColor = False
        '
        'hlpVeldWinterpeil2
        '
        Me.hlpVeldWinterpeil2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpVeldWinterpeil2.BackColor = System.Drawing.Color.Gold
        Me.hlpVeldWinterpeil2.Location = New System.Drawing.Point(1623, 206)
        Me.hlpVeldWinterpeil2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpVeldWinterpeil2.Name = "hlpVeldWinterpeil2"
        Me.hlpVeldWinterpeil2.Size = New System.Drawing.Size(33, 31)
        Me.hlpVeldWinterpeil2.TabIndex = 69
        Me.hlpVeldWinterpeil2.Text = "?"
        Me.hlpVeldWinterpeil2.UseVisualStyleBackColor = False
        '
        'hlpShapefilePeilgebieden2
        '
        Me.hlpShapefilePeilgebieden2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpShapefilePeilgebieden2.BackColor = System.Drawing.Color.Gold
        Me.hlpShapefilePeilgebieden2.Location = New System.Drawing.Point(1623, 174)
        Me.hlpShapefilePeilgebieden2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpShapefilePeilgebieden2.Name = "hlpShapefilePeilgebieden2"
        Me.hlpShapefilePeilgebieden2.Size = New System.Drawing.Size(33, 31)
        Me.hlpShapefilePeilgebieden2.TabIndex = 68
        Me.hlpShapefilePeilgebieden2.Text = "?"
        Me.hlpShapefilePeilgebieden2.UseVisualStyleBackColor = False
        '
        'hlpDatabase2
        '
        Me.hlpDatabase2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpDatabase2.BackColor = System.Drawing.Color.Gold
        Me.hlpDatabase2.Location = New System.Drawing.Point(1623, 143)
        Me.hlpDatabase2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpDatabase2.Name = "hlpDatabase2"
        Me.hlpDatabase2.Size = New System.Drawing.Size(33, 31)
        Me.hlpDatabase2.TabIndex = 67
        Me.hlpDatabase2.Text = "?"
        Me.hlpDatabase2.UseVisualStyleBackColor = False
        '
        'hlpExtraFiles2
        '
        Me.hlpExtraFiles2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpExtraFiles2.BackColor = System.Drawing.Color.Gold
        Me.hlpExtraFiles2.Location = New System.Drawing.Point(1623, 112)
        Me.hlpExtraFiles2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpExtraFiles2.Name = "hlpExtraFiles2"
        Me.hlpExtraFiles2.Size = New System.Drawing.Size(33, 31)
        Me.hlpExtraFiles2.TabIndex = 66
        Me.hlpExtraFiles2.Text = "?"
        Me.hlpExtraFiles2.UseVisualStyleBackColor = False
        '
        'hlpResultatenmap2
        '
        Me.hlpResultatenmap2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpResultatenmap2.BackColor = System.Drawing.Color.Gold
        Me.hlpResultatenmap2.Location = New System.Drawing.Point(1623, 81)
        Me.hlpResultatenmap2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpResultatenmap2.Name = "hlpResultatenmap2"
        Me.hlpResultatenmap2.Size = New System.Drawing.Size(33, 31)
        Me.hlpResultatenmap2.TabIndex = 65
        Me.hlpResultatenmap2.Text = "?"
        Me.hlpResultatenmap2.UseVisualStyleBackColor = False
        '
        'hlbUitvoermap2
        '
        Me.hlbUitvoermap2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlbUitvoermap2.BackColor = System.Drawing.Color.Gold
        Me.hlbUitvoermap2.Location = New System.Drawing.Point(1623, 50)
        Me.hlbUitvoermap2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlbUitvoermap2.Name = "hlbUitvoermap2"
        Me.hlbUitvoermap2.Size = New System.Drawing.Size(33, 31)
        Me.hlbUitvoermap2.TabIndex = 64
        Me.hlbUitvoermap2.Text = "?"
        Me.hlbUitvoermap2.UseVisualStyleBackColor = False
        '
        'hlpInvoermap2
        '
        Me.hlpInvoermap2.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpInvoermap2.BackColor = System.Drawing.Color.Gold
        Me.hlpInvoermap2.Location = New System.Drawing.Point(1623, 20)
        Me.hlpInvoermap2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpInvoermap2.Name = "hlpInvoermap2"
        Me.hlpInvoermap2.Size = New System.Drawing.Size(33, 31)
        Me.hlpInvoermap2.TabIndex = 63
        Me.hlpInvoermap2.Text = "?"
        Me.hlpInvoermap2.UseVisualStyleBackColor = False
        '
        'btnExtraFilesDir
        '
        Me.btnExtraFilesDir.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnExtraFilesDir.Location = New System.Drawing.Point(1583, 112)
        Me.btnExtraFilesDir.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnExtraFilesDir.Name = "btnExtraFilesDir"
        Me.btnExtraFilesDir.Size = New System.Drawing.Size(33, 31)
        Me.btnExtraFilesDir.TabIndex = 54
        Me.btnExtraFilesDir.Text = ".."
        Me.btnExtraFilesDir.UseVisualStyleBackColor = True
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(13, 123)
        Me.Label16.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(237, 16)
        Me.Label16.TabIndex = 53
        Me.Label16.Text = "Map met extra bestanden per simulatie"
        '
        'txtExtraFilesDir
        '
        Me.txtExtraFilesDir.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtExtraFilesDir.Location = New System.Drawing.Point(316, 117)
        Me.txtExtraFilesDir.Margin = New System.Windows.Forms.Padding(4)
        Me.txtExtraFilesDir.Name = "txtExtraFilesDir"
        Me.txtExtraFilesDir.Size = New System.Drawing.Size(1259, 22)
        Me.txtExtraFilesDir.TabIndex = 52
        '
        'btnOutputDir
        '
        Me.btnOutputDir.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOutputDir.Location = New System.Drawing.Point(1583, 50)
        Me.btnOutputDir.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnOutputDir.Name = "btnOutputDir"
        Me.btnOutputDir.Size = New System.Drawing.Size(33, 31)
        Me.btnOutputDir.TabIndex = 51
        Me.btnOutputDir.Text = ".."
        Me.btnOutputDir.UseVisualStyleBackColor = True
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Location = New System.Drawing.Point(12, 59)
        Me.Label15.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(77, 16)
        Me.Label15.TabIndex = 50
        Me.Label15.Text = "Uitvoermap"
        '
        'txtOutputDir
        '
        Me.txtOutputDir.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtOutputDir.Location = New System.Drawing.Point(316, 55)
        Me.txtOutputDir.Margin = New System.Windows.Forms.Padding(4)
        Me.txtOutputDir.Name = "txtOutputDir"
        Me.txtOutputDir.Size = New System.Drawing.Size(1259, 22)
        Me.txtOutputDir.TabIndex = 49
        '
        'btnInputDir
        '
        Me.btnInputDir.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnInputDir.Location = New System.Drawing.Point(1583, 20)
        Me.btnInputDir.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnInputDir.Name = "btnInputDir"
        Me.btnInputDir.Size = New System.Drawing.Size(33, 31)
        Me.btnInputDir.TabIndex = 48
        Me.btnInputDir.Text = ".."
        Me.btnInputDir.UseVisualStyleBackColor = True
        '
        'btnResultsDir
        '
        Me.btnResultsDir.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnResultsDir.Location = New System.Drawing.Point(1583, 81)
        Me.btnResultsDir.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnResultsDir.Name = "btnResultsDir"
        Me.btnResultsDir.Size = New System.Drawing.Size(33, 31)
        Me.btnResultsDir.TabIndex = 47
        Me.btnResultsDir.Text = ".."
        Me.btnResultsDir.UseVisualStyleBackColor = True
        '
        'btnDatabase
        '
        Me.btnDatabase.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnDatabase.Location = New System.Drawing.Point(1583, 143)
        Me.btnDatabase.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnDatabase.Name = "btnDatabase"
        Me.btnDatabase.Size = New System.Drawing.Size(33, 31)
        Me.btnDatabase.TabIndex = 46
        Me.btnDatabase.Text = ".."
        Me.btnDatabase.UseVisualStyleBackColor = True
        '
        'btnShapefile
        '
        Me.btnShapefile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnShapefile.Location = New System.Drawing.Point(1583, 174)
        Me.btnShapefile.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnShapefile.Name = "btnShapefile"
        Me.btnShapefile.Size = New System.Drawing.Size(33, 31)
        Me.btnShapefile.TabIndex = 45
        Me.btnShapefile.Text = ".."
        Me.btnShapefile.UseVisualStyleBackColor = True
        '
        'Label37
        '
        Me.Label37.AutoSize = True
        Me.Label37.Location = New System.Drawing.Point(13, 251)
        Me.Label37.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label37.Name = "Label37"
        Me.Label37.Size = New System.Drawing.Size(97, 16)
        Me.Label37.TabIndex = 44
        Me.Label37.Text = "Veld zomerpeil"
        '
        'Label36
        '
        Me.Label36.AutoSize = True
        Me.Label36.Location = New System.Drawing.Point(13, 219)
        Me.Label36.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label36.Name = "Label36"
        Me.Label36.Size = New System.Drawing.Size(94, 16)
        Me.Label36.TabIndex = 43
        Me.Label36.Text = "Veld winterpeil"
        '
        'cmbZomerpeil
        '
        Me.cmbZomerpeil.FormattingEnabled = True
        Me.cmbZomerpeil.Location = New System.Drawing.Point(316, 241)
        Me.cmbZomerpeil.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbZomerpeil.Name = "cmbZomerpeil"
        Me.cmbZomerpeil.Size = New System.Drawing.Size(169, 24)
        Me.cmbZomerpeil.TabIndex = 42
        '
        'cmbWinterpeil
        '
        Me.cmbWinterpeil.FormattingEnabled = True
        Me.cmbWinterpeil.Location = New System.Drawing.Point(316, 209)
        Me.cmbWinterpeil.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbWinterpeil.Name = "cmbWinterpeil"
        Me.cmbWinterpeil.Size = New System.Drawing.Size(169, 24)
        Me.cmbWinterpeil.TabIndex = 33
        '
        'txtPeilgebieden
        '
        Me.txtPeilgebieden.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtPeilgebieden.Location = New System.Drawing.Point(316, 178)
        Me.txtPeilgebieden.Margin = New System.Windows.Forms.Padding(4)
        Me.txtPeilgebieden.Name = "txtPeilgebieden"
        Me.txtPeilgebieden.Size = New System.Drawing.Size(1259, 22)
        Me.txtPeilgebieden.TabIndex = 41
        '
        'Label35
        '
        Me.Label35.AutoSize = True
        Me.Label35.Location = New System.Drawing.Point(13, 187)
        Me.Label35.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label35.Name = "Label35"
        Me.Label35.Size = New System.Drawing.Size(147, 16)
        Me.Label35.TabIndex = 40
        Me.Label35.Text = "Shapefile peilgebieden"
        '
        'txtResultatenDir
        '
        Me.txtResultatenDir.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtResultatenDir.Location = New System.Drawing.Point(316, 86)
        Me.txtResultatenDir.Margin = New System.Windows.Forms.Padding(4)
        Me.txtResultatenDir.Name = "txtResultatenDir"
        Me.txtResultatenDir.Size = New System.Drawing.Size(1259, 22)
        Me.txtResultatenDir.TabIndex = 39
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(13, 91)
        Me.Label11.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(98, 16)
        Me.Label11.TabIndex = 38
        Me.Label11.Text = "Resultatenmap"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(13, 27)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(71, 16)
        Me.Label5.TabIndex = 26
        Me.Label5.Text = "Invoermap"
        '
        'txtInputDir
        '
        Me.txtInputDir.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtInputDir.Location = New System.Drawing.Point(316, 25)
        Me.txtInputDir.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInputDir.Name = "txtInputDir"
        Me.txtInputDir.Size = New System.Drawing.Size(1259, 22)
        Me.txtInputDir.TabIndex = 25
        '
        'txtDatabase
        '
        Me.txtDatabase.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtDatabase.Location = New System.Drawing.Point(316, 148)
        Me.txtDatabase.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDatabase.Name = "txtDatabase"
        Me.txtDatabase.Size = New System.Drawing.Size(1259, 22)
        Me.txtDatabase.TabIndex = 34
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(13, 155)
        Me.Label7.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(67, 16)
        Me.Label7.TabIndex = 32
        Me.Label7.Text = "Database"
        '
        'tabSobek
        '
        Me.tabSobek.Controls.Add(Me.hlpModellen)
        Me.tabSobek.Controls.Add(Me.btnDeleteModel)
        Me.tabSobek.Controls.Add(Me.btnAddModel)
        Me.tabSobek.Controls.Add(Me.grModels)
        Me.tabSobek.Location = New System.Drawing.Point(4, 25)
        Me.tabSobek.Margin = New System.Windows.Forms.Padding(4)
        Me.tabSobek.Name = "tabSobek"
        Me.tabSobek.Size = New System.Drawing.Size(1675, 590)
        Me.tabSobek.TabIndex = 2
        Me.tabSobek.Text = "Modellen"
        Me.tabSobek.UseVisualStyleBackColor = True
        '
        'hlpModellen
        '
        Me.hlpModellen.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpModellen.BackColor = System.Drawing.Color.Gold
        Me.hlpModellen.Location = New System.Drawing.Point(1637, 10)
        Me.hlpModellen.Margin = New System.Windows.Forms.Padding(4)
        Me.hlpModellen.Name = "hlpModellen"
        Me.hlpModellen.Size = New System.Drawing.Size(33, 31)
        Me.hlpModellen.TabIndex = 22
        Me.hlpModellen.Text = "?"
        Me.hlpModellen.UseVisualStyleBackColor = False
        '
        'btnDeleteModel
        '
        Me.btnDeleteModel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnDeleteModel.BackColor = System.Drawing.Color.IndianRed
        Me.btnDeleteModel.Location = New System.Drawing.Point(1637, 76)
        Me.btnDeleteModel.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDeleteModel.Name = "btnDeleteModel"
        Me.btnDeleteModel.Size = New System.Drawing.Size(33, 31)
        Me.btnDeleteModel.TabIndex = 21
        Me.btnDeleteModel.Text = "-"
        Me.btnDeleteModel.UseVisualStyleBackColor = False
        '
        'btnAddModel
        '
        Me.btnAddModel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnAddModel.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnAddModel.Location = New System.Drawing.Point(1637, 43)
        Me.btnAddModel.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAddModel.Name = "btnAddModel"
        Me.btnAddModel.Size = New System.Drawing.Size(33, 31)
        Me.btnAddModel.TabIndex = 20
        Me.btnAddModel.Text = "+"
        Me.btnAddModel.UseVisualStyleBackColor = False
        '
        'grModels
        '
        Me.grModels.AllowUserToAddRows = False
        Me.grModels.AllowUserToDeleteRows = False
        Me.grModels.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grModels.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grModels.Location = New System.Drawing.Point(4, 4)
        Me.grModels.Margin = New System.Windows.Forms.Padding(4)
        Me.grModels.Name = "grModels"
        Me.grModels.RowHeadersWidth = 51
        Me.grModels.Size = New System.Drawing.Size(1627, 580)
        Me.grModels.TabIndex = 19
        '
        'tabSeizoenen
        '
        Me.tabSeizoenen.Controls.Add(Me.hlpSeizoenen)
        Me.tabSeizoenen.Controls.Add(Me.btnRemoveSeason)
        Me.tabSeizoenen.Controls.Add(Me.btnAddSeason)
        Me.tabSeizoenen.Controls.Add(Me.lblCheckSumSeizoenen)
        Me.tabSeizoenen.Controls.Add(Me.grSeizoenen)
        Me.tabSeizoenen.Location = New System.Drawing.Point(4, 25)
        Me.tabSeizoenen.Margin = New System.Windows.Forms.Padding(4)
        Me.tabSeizoenen.Name = "tabSeizoenen"
        Me.tabSeizoenen.Size = New System.Drawing.Size(1675, 590)
        Me.tabSeizoenen.TabIndex = 15
        Me.tabSeizoenen.Text = "Seizoenen"
        Me.tabSeizoenen.UseVisualStyleBackColor = True
        '
        'hlpSeizoenen
        '
        Me.hlpSeizoenen.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.hlpSeizoenen.BackColor = System.Drawing.Color.Gold
        Me.hlpSeizoenen.Location = New System.Drawing.Point(1637, 9)
        Me.hlpSeizoenen.Margin = New System.Windows.Forms.Padding(4)
        Me.hlpSeizoenen.Name = "hlpSeizoenen"
        Me.hlpSeizoenen.Size = New System.Drawing.Size(33, 31)
        Me.hlpSeizoenen.TabIndex = 23
        Me.hlpSeizoenen.Text = "?"
        Me.hlpSeizoenen.UseVisualStyleBackColor = False
        '
        'btnRemoveSeason
        '
        Me.btnRemoveSeason.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnRemoveSeason.BackColor = System.Drawing.Color.IndianRed
        Me.btnRemoveSeason.Location = New System.Drawing.Point(1637, 76)
        Me.btnRemoveSeason.Margin = New System.Windows.Forms.Padding(4)
        Me.btnRemoveSeason.Name = "btnRemoveSeason"
        Me.btnRemoveSeason.Size = New System.Drawing.Size(33, 31)
        Me.btnRemoveSeason.TabIndex = 10
        Me.btnRemoveSeason.Text = "-"
        Me.btnRemoveSeason.UseVisualStyleBackColor = False
        '
        'btnAddSeason
        '
        Me.btnAddSeason.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnAddSeason.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnAddSeason.Location = New System.Drawing.Point(1637, 43)
        Me.btnAddSeason.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAddSeason.Name = "btnAddSeason"
        Me.btnAddSeason.Size = New System.Drawing.Size(33, 31)
        Me.btnAddSeason.TabIndex = 9
        Me.btnAddSeason.Text = "+"
        Me.btnAddSeason.UseVisualStyleBackColor = False
        '
        'lblCheckSumSeizoenen
        '
        Me.lblCheckSumSeizoenen.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblCheckSumSeizoenen.AutoSize = True
        Me.lblCheckSumSeizoenen.Location = New System.Drawing.Point(4, 553)
        Me.lblCheckSumSeizoenen.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblCheckSumSeizoenen.Name = "lblCheckSumSeizoenen"
        Me.lblCheckSumSeizoenen.Size = New System.Drawing.Size(77, 16)
        Me.lblCheckSumSeizoenen.TabIndex = 8
        Me.lblCheckSumSeizoenen.Text = "Checksum="
        '
        'grSeizoenen
        '
        Me.grSeizoenen.AllowUserToAddRows = False
        Me.grSeizoenen.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grSeizoenen.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grSeizoenen.Location = New System.Drawing.Point(8, 9)
        Me.grSeizoenen.Margin = New System.Windows.Forms.Padding(4)
        Me.grSeizoenen.Name = "grSeizoenen"
        Me.grSeizoenen.RowHeadersWidth = 51
        Me.grSeizoenen.Size = New System.Drawing.Size(1621, 553)
        Me.grSeizoenen.TabIndex = 0
        '
        'tabVolumes
        '
        Me.tabVolumes.Location = New System.Drawing.Point(4, 25)
        Me.tabVolumes.Margin = New System.Windows.Forms.Padding(4)
        Me.tabVolumes.Name = "tabVolumes"
        Me.tabVolumes.Size = New System.Drawing.Size(1675, 590)
        Me.tabVolumes.TabIndex = 6
        Me.tabVolumes.Text = "Volumes"
        Me.tabVolumes.UseVisualStyleBackColor = True
        '
        'tabPatronen
        '
        Me.tabPatronen.Location = New System.Drawing.Point(4, 25)
        Me.tabPatronen.Margin = New System.Windows.Forms.Padding(4)
        Me.tabPatronen.Name = "tabPatronen"
        Me.tabPatronen.Size = New System.Drawing.Size(1675, 590)
        Me.tabPatronen.TabIndex = 7
        Me.tabPatronen.Text = "Patronen"
        Me.tabPatronen.UseVisualStyleBackColor = True
        '
        'tabGrondwater
        '
        Me.tabGrondwater.Location = New System.Drawing.Point(4, 25)
        Me.tabGrondwater.Margin = New System.Windows.Forms.Padding(4)
        Me.tabGrondwater.Name = "tabGrondwater"
        Me.tabGrondwater.Size = New System.Drawing.Size(1675, 590)
        Me.tabGrondwater.TabIndex = 8
        Me.tabGrondwater.Text = "Grondwater"
        Me.tabGrondwater.UseVisualStyleBackColor = True
        '
        'tabBoundaryNodes
        '
        Me.tabBoundaryNodes.Controls.Add(Me.GroupBox23)
        Me.tabBoundaryNodes.Location = New System.Drawing.Point(4, 25)
        Me.tabBoundaryNodes.Margin = New System.Windows.Forms.Padding(4)
        Me.tabBoundaryNodes.Name = "tabBoundaryNodes"
        Me.tabBoundaryNodes.Size = New System.Drawing.Size(1675, 590)
        Me.tabBoundaryNodes.TabIndex = 11
        Me.tabBoundaryNodes.Text = "Randknopen"
        Me.tabBoundaryNodes.UseVisualStyleBackColor = True
        '
        'GroupBox23
        '
        Me.GroupBox23.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox23.Controls.Add(Me.btnDeleteBoundaryNode)
        Me.GroupBox23.Controls.Add(Me.btnAddBoundaryNode)
        Me.GroupBox23.Controls.Add(Me.grBoundaryNodes)
        Me.GroupBox23.Location = New System.Drawing.Point(11, 6)
        Me.GroupBox23.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox23.Name = "GroupBox23"
        Me.GroupBox23.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox23.Size = New System.Drawing.Size(528, 564)
        Me.GroupBox23.TabIndex = 13
        Me.GroupBox23.TabStop = False
        Me.GroupBox23.Text = "Randknopen"
        '
        'btnDeleteBoundaryNode
        '
        Me.btnDeleteBoundaryNode.BackColor = System.Drawing.Color.IndianRed
        Me.btnDeleteBoundaryNode.Location = New System.Drawing.Point(479, 58)
        Me.btnDeleteBoundaryNode.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDeleteBoundaryNode.Name = "btnDeleteBoundaryNode"
        Me.btnDeleteBoundaryNode.Size = New System.Drawing.Size(33, 31)
        Me.btnDeleteBoundaryNode.TabIndex = 11
        Me.btnDeleteBoundaryNode.Text = "-"
        Me.btnDeleteBoundaryNode.UseVisualStyleBackColor = False
        '
        'btnAddBoundaryNode
        '
        Me.btnAddBoundaryNode.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnAddBoundaryNode.Location = New System.Drawing.Point(479, 20)
        Me.btnAddBoundaryNode.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAddBoundaryNode.Name = "btnAddBoundaryNode"
        Me.btnAddBoundaryNode.Size = New System.Drawing.Size(33, 31)
        Me.btnAddBoundaryNode.TabIndex = 10
        Me.btnAddBoundaryNode.Text = "+"
        Me.btnAddBoundaryNode.UseVisualStyleBackColor = False
        '
        'grBoundaryNodes
        '
        Me.grBoundaryNodes.AllowUserToAddRows = False
        Me.grBoundaryNodes.AllowUserToDeleteRows = False
        Me.grBoundaryNodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grBoundaryNodes.Location = New System.Drawing.Point(8, 20)
        Me.grBoundaryNodes.Margin = New System.Windows.Forms.Padding(4)
        Me.grBoundaryNodes.Name = "grBoundaryNodes"
        Me.grBoundaryNodes.RowHeadersVisible = False
        Me.grBoundaryNodes.RowHeadersWidth = 51
        Me.grBoundaryNodes.Size = New System.Drawing.Size(463, 537)
        Me.grBoundaryNodes.TabIndex = 1
        Me.grBoundaryNodes.Tag = "Zomer"
        '
        'tabWaterlevels
        '
        Me.tabWaterlevels.Controls.Add(Me.grWLChart)
        Me.tabWaterlevels.Controls.Add(Me.grWLClasses)
        Me.tabWaterlevels.Location = New System.Drawing.Point(4, 25)
        Me.tabWaterlevels.Margin = New System.Windows.Forms.Padding(4)
        Me.tabWaterlevels.Name = "tabWaterlevels"
        Me.tabWaterlevels.Size = New System.Drawing.Size(1675, 590)
        Me.tabWaterlevels.TabIndex = 10
        Me.tabWaterlevels.Text = "Waterhoogtes"
        Me.tabWaterlevels.UseVisualStyleBackColor = True
        '
        'grWLChart
        '
        Me.grWLChart.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grWLChart.Controls.Add(Me.chartBoundaries)
        Me.grWLChart.Location = New System.Drawing.Point(617, 10)
        Me.grWLChart.Margin = New System.Windows.Forms.Padding(4)
        Me.grWLChart.Name = "grWLChart"
        Me.grWLChart.Padding = New System.Windows.Forms.Padding(4)
        Me.grWLChart.Size = New System.Drawing.Size(1045, 566)
        Me.grWLChart.TabIndex = 9
        Me.grWLChart.TabStop = False
        Me.grWLChart.Text = "Grafiek"
        '
        'chartBoundaries
        '
        Me.chartBoundaries.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        ChartArea1.Name = "ChartArea1"
        Me.chartBoundaries.ChartAreas.Add(ChartArea1)
        Legend1.Name = "Legend1"
        Me.chartBoundaries.Legends.Add(Legend1)
        Me.chartBoundaries.Location = New System.Drawing.Point(8, 20)
        Me.chartBoundaries.Margin = New System.Windows.Forms.Padding(4)
        Me.chartBoundaries.Name = "chartBoundaries"
        Series1.ChartArea = "ChartArea1"
        Series1.Legend = "Legend1"
        Series1.Name = "Series1"
        Me.chartBoundaries.Series.Add(Series1)
        Me.chartBoundaries.Size = New System.Drawing.Size(1029, 537)
        Me.chartBoundaries.TabIndex = 0
        Me.chartBoundaries.Text = "Grafiek"
        '
        'grWLClasses
        '
        Me.grWLClasses.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.grWLClasses.Controls.Add(Me.btnCopy)
        Me.grWLClasses.Controls.Add(Me.grWaterLevelSeries)
        Me.grWLClasses.Controls.Add(Me.lblBoundaryChecksum)
        Me.grWLClasses.Controls.Add(Me.btnDeleteBoundaryClass)
        Me.grWLClasses.Controls.Add(Me.btnAddBoundaryClass)
        Me.grWLClasses.Controls.Add(Me.grWaterLevelClasses)
        Me.grWLClasses.Location = New System.Drawing.Point(5, 10)
        Me.grWLClasses.Margin = New System.Windows.Forms.Padding(4)
        Me.grWLClasses.Name = "grWLClasses"
        Me.grWLClasses.Padding = New System.Windows.Forms.Padding(4)
        Me.grWLClasses.Size = New System.Drawing.Size(604, 566)
        Me.grWLClasses.TabIndex = 3
        Me.grWLClasses.TabStop = False
        Me.grWLClasses.Text = "Klasse-indeling"
        '
        'btnCopy
        '
        Me.btnCopy.BackColor = System.Drawing.Color.Gold
        Me.btnCopy.Location = New System.Drawing.Point(561, 98)
        Me.btnCopy.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCopy.Name = "btnCopy"
        Me.btnCopy.Size = New System.Drawing.Size(33, 31)
        Me.btnCopy.TabIndex = 13
        Me.btnCopy.Text = "c"
        Me.btnCopy.UseVisualStyleBackColor = False
        '
        'grWaterLevelSeries
        '
        Me.grWaterLevelSeries.AllowUserToAddRows = False
        Me.grWaterLevelSeries.AllowUserToDeleteRows = False
        Me.grWaterLevelSeries.AllowUserToResizeRows = False
        Me.grWaterLevelSeries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grWaterLevelSeries.Location = New System.Drawing.Point(8, 267)
        Me.grWaterLevelSeries.Margin = New System.Windows.Forms.Padding(4)
        Me.grWaterLevelSeries.Name = "grWaterLevelSeries"
        Me.grWaterLevelSeries.RowHeadersWidth = 51
        Me.grWaterLevelSeries.Size = New System.Drawing.Size(547, 270)
        Me.grWaterLevelSeries.TabIndex = 2
        Me.grWaterLevelSeries.Tag = ""
        '
        'lblBoundaryChecksum
        '
        Me.lblBoundaryChecksum.AutoSize = True
        Me.lblBoundaryChecksum.Location = New System.Drawing.Point(4, 550)
        Me.lblBoundaryChecksum.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblBoundaryChecksum.Name = "lblBoundaryChecksum"
        Me.lblBoundaryChecksum.Size = New System.Drawing.Size(77, 16)
        Me.lblBoundaryChecksum.TabIndex = 12
        Me.lblBoundaryChecksum.Text = "Checksum="
        '
        'btnDeleteBoundaryClass
        '
        Me.btnDeleteBoundaryClass.BackColor = System.Drawing.Color.IndianRed
        Me.btnDeleteBoundaryClass.Location = New System.Drawing.Point(561, 59)
        Me.btnDeleteBoundaryClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDeleteBoundaryClass.Name = "btnDeleteBoundaryClass"
        Me.btnDeleteBoundaryClass.Size = New System.Drawing.Size(33, 31)
        Me.btnDeleteBoundaryClass.TabIndex = 11
        Me.btnDeleteBoundaryClass.Text = "-"
        Me.btnDeleteBoundaryClass.UseVisualStyleBackColor = False
        '
        'btnAddBoundaryClass
        '
        Me.btnAddBoundaryClass.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnAddBoundaryClass.Location = New System.Drawing.Point(561, 20)
        Me.btnAddBoundaryClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAddBoundaryClass.Name = "btnAddBoundaryClass"
        Me.btnAddBoundaryClass.Size = New System.Drawing.Size(33, 31)
        Me.btnAddBoundaryClass.TabIndex = 10
        Me.btnAddBoundaryClass.Text = "+"
        Me.btnAddBoundaryClass.UseVisualStyleBackColor = False
        '
        'grWaterLevelClasses
        '
        Me.grWaterLevelClasses.AllowUserToAddRows = False
        Me.grWaterLevelClasses.AllowUserToDeleteRows = False
        Me.grWaterLevelClasses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grWaterLevelClasses.Location = New System.Drawing.Point(8, 20)
        Me.grWaterLevelClasses.Margin = New System.Windows.Forms.Padding(4)
        Me.grWaterLevelClasses.Name = "grWaterLevelClasses"
        Me.grWaterLevelClasses.RowHeadersWidth = 51
        Me.grWaterLevelClasses.Size = New System.Drawing.Size(547, 240)
        Me.grWaterLevelClasses.TabIndex = 1
        Me.grWaterLevelClasses.Tag = "Zomer"
        '
        'tabWind
        '
        Me.tabWind.Controls.Add(Me.GroupBox22)
        Me.tabWind.Controls.Add(Me.GroupBox24)
        Me.tabWind.Location = New System.Drawing.Point(4, 25)
        Me.tabWind.Margin = New System.Windows.Forms.Padding(4)
        Me.tabWind.Name = "tabWind"
        Me.tabWind.Size = New System.Drawing.Size(1675, 590)
        Me.tabWind.TabIndex = 12
        Me.tabWind.Text = "Wind"
        Me.tabWind.UseVisualStyleBackColor = True
        '
        'GroupBox22
        '
        Me.GroupBox22.Controls.Add(Me.grWindSeries)
        Me.GroupBox22.Location = New System.Drawing.Point(544, 14)
        Me.GroupBox22.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox22.Name = "GroupBox22"
        Me.GroupBox22.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox22.Size = New System.Drawing.Size(495, 553)
        Me.GroupBox22.TabIndex = 10
        Me.GroupBox22.TabStop = False
        Me.GroupBox22.Text = "Tijdreeks"
        '
        'grWindSeries
        '
        Me.grWindSeries.AllowUserToAddRows = False
        Me.grWindSeries.AllowUserToDeleteRows = False
        Me.grWindSeries.AllowUserToResizeRows = False
        Me.grWindSeries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grWindSeries.Location = New System.Drawing.Point(8, 20)
        Me.grWindSeries.Margin = New System.Windows.Forms.Padding(4)
        Me.grWindSeries.Name = "grWindSeries"
        Me.grWindSeries.RowHeadersWidth = 51
        Me.grWindSeries.Size = New System.Drawing.Size(483, 498)
        Me.grWindSeries.TabIndex = 2
        Me.grWindSeries.Tag = ""
        '
        'GroupBox24
        '
        Me.GroupBox24.Controls.Add(Me.btnWindCopy)
        Me.GroupBox24.Controls.Add(Me.lblWindChecksum)
        Me.GroupBox24.Controls.Add(Me.btnDeleteWindClass)
        Me.GroupBox24.Controls.Add(Me.btnAddWindClass)
        Me.GroupBox24.Controls.Add(Me.grWindKlassen)
        Me.GroupBox24.Location = New System.Drawing.Point(11, 12)
        Me.GroupBox24.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox24.Name = "GroupBox24"
        Me.GroupBox24.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox24.Size = New System.Drawing.Size(521, 558)
        Me.GroupBox24.TabIndex = 8
        Me.GroupBox24.TabStop = False
        Me.GroupBox24.Text = "Wind"
        '
        'btnWindCopy
        '
        Me.btnWindCopy.BackColor = System.Drawing.Color.Gold
        Me.btnWindCopy.Location = New System.Drawing.Point(480, 101)
        Me.btnWindCopy.Margin = New System.Windows.Forms.Padding(4)
        Me.btnWindCopy.Name = "btnWindCopy"
        Me.btnWindCopy.Size = New System.Drawing.Size(33, 31)
        Me.btnWindCopy.TabIndex = 14
        Me.btnWindCopy.Text = "c"
        Me.btnWindCopy.UseVisualStyleBackColor = False
        '
        'lblWindChecksum
        '
        Me.lblWindChecksum.AutoSize = True
        Me.lblWindChecksum.Location = New System.Drawing.Point(8, 538)
        Me.lblWindChecksum.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblWindChecksum.Name = "lblWindChecksum"
        Me.lblWindChecksum.Size = New System.Drawing.Size(77, 16)
        Me.lblWindChecksum.TabIndex = 13
        Me.lblWindChecksum.Text = "Checksum="
        '
        'btnDeleteWindClass
        '
        Me.btnDeleteWindClass.BackColor = System.Drawing.Color.IndianRed
        Me.btnDeleteWindClass.Location = New System.Drawing.Point(480, 62)
        Me.btnDeleteWindClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDeleteWindClass.Name = "btnDeleteWindClass"
        Me.btnDeleteWindClass.Size = New System.Drawing.Size(33, 31)
        Me.btnDeleteWindClass.TabIndex = 11
        Me.btnDeleteWindClass.Text = "-"
        Me.btnDeleteWindClass.UseVisualStyleBackColor = False
        '
        'btnAddWindClass
        '
        Me.btnAddWindClass.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnAddWindClass.Location = New System.Drawing.Point(480, 23)
        Me.btnAddWindClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAddWindClass.Name = "btnAddWindClass"
        Me.btnAddWindClass.Size = New System.Drawing.Size(33, 31)
        Me.btnAddWindClass.TabIndex = 10
        Me.btnAddWindClass.Text = "+"
        Me.btnAddWindClass.UseVisualStyleBackColor = False
        '
        'grWindKlassen
        '
        Me.grWindKlassen.AllowUserToAddRows = False
        Me.grWindKlassen.AllowUserToDeleteRows = False
        Me.grWindKlassen.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grWindKlassen.Location = New System.Drawing.Point(8, 21)
        Me.grWindKlassen.Margin = New System.Windows.Forms.Padding(4)
        Me.grWindKlassen.Name = "grWindKlassen"
        Me.grWindKlassen.RowHeadersWidth = 51
        Me.grWindKlassen.Size = New System.Drawing.Size(464, 498)
        Me.grWindKlassen.TabIndex = 1
        Me.grWindKlassen.Tag = "Zomer"
        '
        'tabExtra
        '
        Me.tabExtra.Location = New System.Drawing.Point(4, 25)
        Me.tabExtra.Margin = New System.Windows.Forms.Padding(4)
        Me.tabExtra.Name = "tabExtra"
        Me.tabExtra.Size = New System.Drawing.Size(1675, 590)
        Me.tabExtra.TabIndex = 13
        Me.tabExtra.Text = "Extra stochasten"
        Me.tabExtra.UseVisualStyleBackColor = True
        '
        'tabPostprocessing
        '
        Me.tabPostprocessing.Controls.Add(Me.tabOutput)
        Me.tabPostprocessing.Location = New System.Drawing.Point(4, 25)
        Me.tabPostprocessing.Margin = New System.Windows.Forms.Padding(4)
        Me.tabPostprocessing.Name = "tabPostprocessing"
        Me.tabPostprocessing.Padding = New System.Windows.Forms.Padding(4)
        Me.tabPostprocessing.Size = New System.Drawing.Size(1675, 590)
        Me.tabPostprocessing.TabIndex = 4
        Me.tabPostprocessing.Text = "Uitvoer"
        Me.tabPostprocessing.UseVisualStyleBackColor = True
        '
        'tabOutput
        '
        Me.tabOutput.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tabOutput.Controls.Add(Me.tab1D)
        Me.tabOutput.Controls.Add(Me.tab2D)
        Me.tabOutput.Location = New System.Drawing.Point(5, 10)
        Me.tabOutput.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.tabOutput.Name = "tabOutput"
        Me.tabOutput.SelectedIndex = 0
        Me.tabOutput.Size = New System.Drawing.Size(1692, 577)
        Me.tabOutput.TabIndex = 2
        '
        'tab1D
        '
        Me.tab1D.Controls.Add(Me.grOutputLocations)
        Me.tab1D.Controls.Add(Me.btnRemoveOutputLocation)
        Me.tab1D.Location = New System.Drawing.Point(4, 25)
        Me.tab1D.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.tab1D.Name = "tab1D"
        Me.tab1D.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.tab1D.Size = New System.Drawing.Size(1684, 548)
        Me.tab1D.TabIndex = 0
        Me.tab1D.Text = "1D"
        Me.tab1D.UseVisualStyleBackColor = True
        '
        'grOutputLocations
        '
        Me.grOutputLocations.AllowUserToAddRows = False
        Me.grOutputLocations.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grOutputLocations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grOutputLocations.Location = New System.Drawing.Point(5, 6)
        Me.grOutputLocations.Margin = New System.Windows.Forms.Padding(4)
        Me.grOutputLocations.Name = "grOutputLocations"
        Me.grOutputLocations.RowHeadersWidth = 51
        Me.grOutputLocations.Size = New System.Drawing.Size(1629, 527)
        Me.grOutputLocations.TabIndex = 0
        '
        'btnRemoveOutputLocation
        '
        Me.btnRemoveOutputLocation.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnRemoveOutputLocation.BackColor = System.Drawing.Color.IndianRed
        Me.btnRemoveOutputLocation.Location = New System.Drawing.Point(1643, 6)
        Me.btnRemoveOutputLocation.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnRemoveOutputLocation.Name = "btnRemoveOutputLocation"
        Me.btnRemoveOutputLocation.Size = New System.Drawing.Size(37, 32)
        Me.btnRemoveOutputLocation.TabIndex = 1
        Me.btnRemoveOutputLocation.Text = "-"
        Me.btnRemoveOutputLocation.UseVisualStyleBackColor = False
        '
        'tab2D
        '
        Me.tab2D.Controls.Add(Me.radFou)
        Me.tab2D.Location = New System.Drawing.Point(4, 25)
        Me.tab2D.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.tab2D.Name = "tab2D"
        Me.tab2D.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.tab2D.Size = New System.Drawing.Size(1684, 548)
        Me.tab2D.TabIndex = 1
        Me.tab2D.Text = "2D"
        Me.tab2D.UseVisualStyleBackColor = True
        '
        'radFou
        '
        Me.radFou.AutoSize = True
        Me.radFou.Checked = True
        Me.radFou.Location = New System.Drawing.Point(19, 18)
        Me.radFou.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.radFou.Name = "radFou"
        Me.radFou.Size = New System.Drawing.Size(152, 20)
        Me.radFou.TabIndex = 0
        Me.radFou.TabStop = True
        Me.radFou.Text = "Fourier file (D-Hydro)"
        Me.radFou.UseVisualStyleBackColor = True
        '
        'TabRuns
        '
        Me.TabRuns.Controls.Add(Me.btnCopyResults)
        Me.TabRuns.Controls.Add(Me.btnSimulate)
        Me.TabRuns.Controls.Add(Me.btnUitlezen)
        Me.TabRuns.Controls.Add(Me.btnViewer)
        Me.TabRuns.Controls.Add(Me.btnWissen)
        Me.TabRuns.Controls.Add(Me.btnPopulateRuns)
        Me.TabRuns.Controls.Add(Me.btnExport)
        Me.TabRuns.Controls.Add(Me.btnPostprocessing)
        Me.TabRuns.Controls.Add(Me.btnBuild)
        Me.TabRuns.Controls.Add(Me.lblSelected)
        Me.TabRuns.Controls.Add(Me.lblnRuns)
        Me.TabRuns.Controls.Add(Me.lblCheckSumRuns)
        Me.TabRuns.Controls.Add(Me.grRuns)
        Me.TabRuns.Location = New System.Drawing.Point(4, 25)
        Me.TabRuns.Margin = New System.Windows.Forms.Padding(4)
        Me.TabRuns.Name = "TabRuns"
        Me.TabRuns.Size = New System.Drawing.Size(1675, 590)
        Me.TabRuns.TabIndex = 5
        Me.TabRuns.Text = "Simulaties"
        Me.TabRuns.UseVisualStyleBackColor = True
        '
        'btnCopyResults
        '
        Me.btnCopyResults.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnCopyResults.BackColor = System.Drawing.Color.YellowGreen
        Me.btnCopyResults.Location = New System.Drawing.Point(1523, 233)
        Me.btnCopyResults.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCopyResults.Name = "btnCopyResults"
        Me.btnCopyResults.Size = New System.Drawing.Size(133, 50)
        Me.btnCopyResults.TabIndex = 39
        Me.btnCopyResults.Text = "Kopiëren"
        Me.btnCopyResults.UseVisualStyleBackColor = False
        '
        'btnSimulate
        '
        Me.btnSimulate.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSimulate.BackColor = System.Drawing.Color.Gold
        Me.btnSimulate.Location = New System.Drawing.Point(1523, 178)
        Me.btnSimulate.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSimulate.Name = "btnSimulate"
        Me.btnSimulate.Size = New System.Drawing.Size(133, 50)
        Me.btnSimulate.TabIndex = 38
        Me.btnSimulate.Text = "Simuleren"
        Me.btnSimulate.UseVisualStyleBackColor = False
        '
        'btnUitlezen
        '
        Me.btnUitlezen.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnUitlezen.BackColor = System.Drawing.Color.GreenYellow
        Me.btnUitlezen.Enabled = False
        Me.btnUitlezen.Location = New System.Drawing.Point(1523, 287)
        Me.btnUitlezen.Margin = New System.Windows.Forms.Padding(4)
        Me.btnUitlezen.Name = "btnUitlezen"
        Me.btnUitlezen.Size = New System.Drawing.Size(133, 50)
        Me.btnUitlezen.TabIndex = 37
        Me.btnUitlezen.Text = "Uitlezen"
        Me.btnUitlezen.UseVisualStyleBackColor = False
        '
        'btnViewer
        '
        Me.btnViewer.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnViewer.BackColor = System.Drawing.Color.CornflowerBlue
        Me.btnViewer.Location = New System.Drawing.Point(1523, 395)
        Me.btnViewer.Margin = New System.Windows.Forms.Padding(4)
        Me.btnViewer.Name = "btnViewer"
        Me.btnViewer.Size = New System.Drawing.Size(133, 50)
        Me.btnViewer.TabIndex = 36
        Me.btnViewer.Text = "Publiceren"
        Me.btnViewer.UseVisualStyleBackColor = False
        '
        'btnWissen
        '
        Me.btnWissen.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnWissen.BackColor = System.Drawing.Color.DarkSalmon
        Me.btnWissen.Location = New System.Drawing.Point(1523, 71)
        Me.btnWissen.Margin = New System.Windows.Forms.Padding(4)
        Me.btnWissen.Name = "btnWissen"
        Me.btnWissen.Size = New System.Drawing.Size(133, 50)
        Me.btnWissen.TabIndex = 35
        Me.btnWissen.Text = "Wissen"
        Me.btnWissen.UseVisualStyleBackColor = False
        '
        'btnPopulateRuns
        '
        Me.btnPopulateRuns.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPopulateRuns.BackColor = System.Drawing.Color.IndianRed
        Me.btnPopulateRuns.Enabled = False
        Me.btnPopulateRuns.Location = New System.Drawing.Point(1523, 17)
        Me.btnPopulateRuns.Margin = New System.Windows.Forms.Padding(4)
        Me.btnPopulateRuns.Name = "btnPopulateRuns"
        Me.btnPopulateRuns.Size = New System.Drawing.Size(133, 50)
        Me.btnPopulateRuns.TabIndex = 34
        Me.btnPopulateRuns.Text = "Samenstellen"
        Me.btnPopulateRuns.UseVisualStyleBackColor = False
        '
        'btnExport
        '
        Me.btnExport.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnExport.BackColor = System.Drawing.Color.SlateBlue
        Me.btnExport.Location = New System.Drawing.Point(1523, 449)
        Me.btnExport.Margin = New System.Windows.Forms.Padding(4)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(133, 50)
        Me.btnExport.TabIndex = 33
        Me.btnExport.Text = "Exporteren"
        Me.btnExport.UseVisualStyleBackColor = False
        '
        'btnPostprocessing
        '
        Me.btnPostprocessing.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPostprocessing.BackColor = System.Drawing.Color.LightSeaGreen
        Me.btnPostprocessing.Location = New System.Drawing.Point(1523, 341)
        Me.btnPostprocessing.Margin = New System.Windows.Forms.Padding(4)
        Me.btnPostprocessing.Name = "btnPostprocessing"
        Me.btnPostprocessing.Size = New System.Drawing.Size(133, 50)
        Me.btnPostprocessing.TabIndex = 32
        Me.btnPostprocessing.Text = "Nabewerken"
        Me.btnPostprocessing.UseVisualStyleBackColor = False
        '
        'btnBuild
        '
        Me.btnBuild.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnBuild.BackColor = System.Drawing.Color.Orange
        Me.btnBuild.Location = New System.Drawing.Point(1523, 126)
        Me.btnBuild.Margin = New System.Windows.Forms.Padding(4)
        Me.btnBuild.Name = "btnBuild"
        Me.btnBuild.Size = New System.Drawing.Size(133, 50)
        Me.btnBuild.TabIndex = 31
        Me.btnBuild.Text = "Bouwen"
        Me.btnBuild.UseVisualStyleBackColor = False
        '
        'lblSelected
        '
        Me.lblSelected.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblSelected.AutoSize = True
        Me.lblSelected.Location = New System.Drawing.Point(13, 559)
        Me.lblSelected.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSelected.Name = "lblSelected"
        Me.lblSelected.Size = New System.Drawing.Size(151, 16)
        Me.lblSelected.TabIndex = 30
        Me.lblSelected.Text = "Geen runs geselecteerd"
        '
        'lblnRuns
        '
        Me.lblnRuns.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblnRuns.AutoSize = True
        Me.lblnRuns.Location = New System.Drawing.Point(13, 535)
        Me.lblnRuns.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblnRuns.Name = "lblnRuns"
        Me.lblnRuns.Size = New System.Drawing.Size(121, 16)
        Me.lblnRuns.TabIndex = 29
        Me.lblnRuns.Text = "Totaal aantal runs="
        '
        'lblCheckSumRuns
        '
        Me.lblCheckSumRuns.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblCheckSumRuns.AutoSize = True
        Me.lblCheckSumRuns.Location = New System.Drawing.Point(13, 511)
        Me.lblCheckSumRuns.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblCheckSumRuns.Name = "lblCheckSumRuns"
        Me.lblCheckSumRuns.Size = New System.Drawing.Size(77, 16)
        Me.lblCheckSumRuns.TabIndex = 28
        Me.lblCheckSumRuns.Text = "Checksum="
        '
        'grRuns
        '
        Me.grRuns.AllowUserToAddRows = False
        Me.grRuns.AllowUserToDeleteRows = False
        Me.grRuns.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grRuns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grRuns.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.grRuns.Location = New System.Drawing.Point(17, 16)
        Me.grRuns.Margin = New System.Windows.Forms.Padding(4)
        Me.grRuns.Name = "grRuns"
        Me.grRuns.RowHeadersWidth = 60
        Me.grRuns.Size = New System.Drawing.Size(1497, 478)
        Me.grRuns.TabIndex = 1
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.GroupBox10)
        Me.TabPage1.Controls.Add(Me.GroupBox11)
        Me.TabPage1.Controls.Add(Me.GroupBox12)
        Me.TabPage1.Controls.Add(Me.GroupBox13)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(957, 374)
        Me.TabPage1.TabIndex = 1
        Me.TabPage1.Text = "Algemeen"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'GroupBox10
        '
        Me.GroupBox10.Controls.Add(Me.Label3)
        Me.GroupBox10.Controls.Add(Me.TextBox1)
        Me.GroupBox10.Location = New System.Drawing.Point(6, 223)
        Me.GroupBox10.Name = "GroupBox10"
        Me.GroupBox10.Size = New System.Drawing.Size(184, 145)
        Me.GroupBox10.TabIndex = 40
        Me.GroupBox10.TabStop = False
        Me.GroupBox10.Text = "Voorbewerking"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(9, 37)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(140, 16)
        Me.Label3.TabIndex = 32
        Me.Label3.Text = "Gebiedsreductiefactor"
        '
        'TextBox1
        '
        Me.TextBox1.Enabled = False
        Me.TextBox1.Location = New System.Drawing.Point(140, 34)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(38, 22)
        Me.TextBox1.TabIndex = 10
        Me.TextBox1.Text = "1"
        '
        'GroupBox11
        '
        Me.GroupBox11.Controls.Add(Me.ComboBox1)
        Me.GroupBox11.Controls.Add(Me.Label6)
        Me.GroupBox11.Location = New System.Drawing.Point(751, 223)
        Me.GroupBox11.Name = "GroupBox11"
        Me.GroupBox11.Size = New System.Drawing.Size(200, 145)
        Me.GroupBox11.TabIndex = 39
        Me.GroupBox11.TabStop = False
        Me.GroupBox11.Text = "Nabewerking"
        '
        'ComboBox1
        '
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Location = New System.Drawing.Point(93, 33)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(98, 24)
        Me.ComboBox1.TabIndex = 35
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(6, 36)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(103, 16)
        Me.Label6.TabIndex = 36
        Me.Label6.Text = "Klimaatscenario"
        '
        'GroupBox12
        '
        Me.GroupBox12.Controls.Add(Me.Label10)
        Me.GroupBox12.Controls.Add(Me.TextBox2)
        Me.GroupBox12.Controls.Add(Me.Label12)
        Me.GroupBox12.Controls.Add(Me.ComboBox2)
        Me.GroupBox12.Controls.Add(Me.Label13)
        Me.GroupBox12.Controls.Add(Me.TextBox3)
        Me.GroupBox12.Location = New System.Drawing.Point(196, 223)
        Me.GroupBox12.Name = "GroupBox12"
        Me.GroupBox12.Size = New System.Drawing.Size(544, 145)
        Me.GroupBox12.TabIndex = 38
        Me.GroupBox12.TabStop = False
        Me.GroupBox12.Text = "Berekeningen"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(9, 54)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(143, 16)
        Me.Label10.TabIndex = 32
        Me.Label10.Text = "Uitloop neerslag (uren)"
        '
        'TextBox2
        '
        Me.TextBox2.Enabled = False
        Me.TextBox2.Location = New System.Drawing.Point(132, 51)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(56, 22)
        Me.TextBox2.TabIndex = 9
        Me.TextBox2.Text = "48"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(9, 27)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(126, 16)
        Me.Label12.TabIndex = 6
        Me.Label12.Text = "Neerslagduur (uren)"
        '
        'ComboBox2
        '
        Me.ComboBox2.FormattingEnabled = True
        Me.ComboBox2.Location = New System.Drawing.Point(132, 27)
        Me.ComboBox2.Name = "ComboBox2"
        Me.ComboBox2.Size = New System.Drawing.Size(56, 24)
        Me.ComboBox2.TabIndex = 0
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(9, 80)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(150, 16)
        Me.Label13.TabIndex = 31
        Me.Label13.Text = "Parallelle berekeningen"
        '
        'TextBox3
        '
        Me.TextBox3.Enabled = False
        Me.TextBox3.Location = New System.Drawing.Point(132, 77)
        Me.TextBox3.Name = "TextBox3"
        Me.TextBox3.Size = New System.Drawing.Size(45, 22)
        Me.TextBox3.TabIndex = 28
        Me.TextBox3.Text = "4"
        '
        'GroupBox13
        '
        Me.GroupBox13.Controls.Add(Me.TextBox4)
        Me.GroupBox13.Controls.Add(Me.Label14)
        Me.GroupBox13.Controls.Add(Me.Label17)
        Me.GroupBox13.Controls.Add(Me.TextBox5)
        Me.GroupBox13.Controls.Add(Me.TextBox6)
        Me.GroupBox13.Controls.Add(Me.Label18)
        Me.GroupBox13.Location = New System.Drawing.Point(6, 8)
        Me.GroupBox13.Name = "GroupBox13"
        Me.GroupBox13.Size = New System.Drawing.Size(945, 202)
        Me.GroupBox13.TabIndex = 37
        Me.GroupBox13.TabStop = False
        Me.GroupBox13.Text = "Bestanden en Directories"
        '
        'TextBox4
        '
        Me.TextBox4.Enabled = False
        Me.TextBox4.Location = New System.Drawing.Point(177, 54)
        Me.TextBox4.Name = "TextBox4"
        Me.TextBox4.Size = New System.Drawing.Size(759, 22)
        Me.TextBox4.TabIndex = 39
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(9, 57)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(88, 16)
        Me.Label14.TabIndex = 38
        Me.Label14.Text = "ResultatenDir"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(9, 31)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(91, 16)
        Me.Label17.TabIndex = 26
        Me.Label17.Text = "StochastenDir"
        '
        'TextBox5
        '
        Me.TextBox5.Enabled = False
        Me.TextBox5.Location = New System.Drawing.Point(177, 28)
        Me.TextBox5.Name = "TextBox5"
        Me.TextBox5.Size = New System.Drawing.Size(759, 22)
        Me.TextBox5.TabIndex = 25
        '
        'TextBox6
        '
        Me.TextBox6.Enabled = False
        Me.TextBox6.Location = New System.Drawing.Point(177, 80)
        Me.TextBox6.Name = "TextBox6"
        Me.TextBox6.Size = New System.Drawing.Size(759, 22)
        Me.TextBox6.TabIndex = 34
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(9, 83)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(195, 16)
        Me.Label18.TabIndex = 32
        Me.Label18.Text = "Configuratiebestand stochasten"
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.DataGridView1)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Size = New System.Drawing.Size(957, 374)
        Me.TabPage2.TabIndex = 2
        Me.TabPage2.Text = "Modellen"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'DataGridView1
        '
        Me.DataGridView1.AllowUserToAddRows = False
        Me.DataGridView1.AllowUserToDeleteRows = False
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewComboBoxColumn1, Me.DataGridViewTextBoxColumn2, Me.DataGridViewTextBoxColumn3, Me.DataGridViewTextBoxColumn4, Me.DataGridViewTextBoxColumn5, Me.DataGridViewTextBoxColumn6})
        Me.DataGridView1.Location = New System.Drawing.Point(0, 0)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.RowHeadersWidth = 51
        Me.DataGridView1.Size = New System.Drawing.Size(954, 334)
        Me.DataGridView1.TabIndex = 19
        '
        'DataGridViewTextBoxColumn1
        '
        Me.DataGridViewTextBoxColumn1.HeaderText = "ID"
        Me.DataGridViewTextBoxColumn1.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.DataGridViewTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.DataGridViewTextBoxColumn1.Width = 30
        '
        'DataGridViewComboBoxColumn1
        '
        Me.DataGridViewComboBoxColumn1.FillWeight = 80.0!
        Me.DataGridViewComboBoxColumn1.HeaderText = "Type"
        Me.DataGridViewComboBoxColumn1.Items.AddRange(New Object() {"SOBEK", "Custom"})
        Me.DataGridViewComboBoxColumn1.MinimumWidth = 6
        Me.DataGridViewComboBoxColumn1.Name = "DataGridViewComboBoxColumn1"
        Me.DataGridViewComboBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.DataGridViewComboBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.DataGridViewComboBoxColumn1.Width = 80
        '
        'DataGridViewTextBoxColumn2
        '
        Me.DataGridViewTextBoxColumn2.HeaderText = "Executable"
        Me.DataGridViewTextBoxColumn2.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.Width = 140
        '
        'DataGridViewTextBoxColumn3
        '
        Me.DataGridViewTextBoxColumn3.HeaderText = "Arguments"
        Me.DataGridViewTextBoxColumn3.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.Width = 120
        '
        'DataGridViewTextBoxColumn4
        '
        Me.DataGridViewTextBoxColumn4.HeaderText = "ModelDir"
        Me.DataGridViewTextBoxColumn4.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn4.Name = "DataGridViewTextBoxColumn4"
        Me.DataGridViewTextBoxColumn4.Width = 120
        '
        'DataGridViewTextBoxColumn5
        '
        Me.DataGridViewTextBoxColumn5.HeaderText = "CaseName"
        Me.DataGridViewTextBoxColumn5.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn5.Name = "DataGridViewTextBoxColumn5"
        Me.DataGridViewTextBoxColumn5.Width = 120
        '
        'DataGridViewTextBoxColumn6
        '
        Me.DataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn6.HeaderText = "Temporary WorkDir"
        Me.DataGridViewTextBoxColumn6.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn6.Name = "DataGridViewTextBoxColumn6"
        '
        'TabPage3
        '
        Me.TabPage3.Controls.Add(Me.GroupBox14)
        Me.TabPage3.Controls.Add(Me.GroupBox15)
        Me.TabPage3.Location = New System.Drawing.Point(4, 22)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Size = New System.Drawing.Size(957, 374)
        Me.TabPage3.TabIndex = 6
        Me.TabPage3.Text = "Volumes"
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'GroupBox14
        '
        Me.GroupBox14.Controls.Add(Me.DataGridView2)
        Me.GroupBox14.Location = New System.Drawing.Point(478, 3)
        Me.GroupBox14.Name = "GroupBox14"
        Me.GroupBox14.Size = New System.Drawing.Size(470, 363)
        Me.GroupBox14.TabIndex = 2
        Me.GroupBox14.TabStop = False
        Me.GroupBox14.Text = "Winter"
        '
        'DataGridView2
        '
        Me.DataGridView2.AllowUserToAddRows = False
        Me.DataGridView2.AllowUserToDeleteRows = False
        Me.DataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView2.Location = New System.Drawing.Point(4, 18)
        Me.DataGridView2.Name = "DataGridView2"
        Me.DataGridView2.RowHeadersWidth = 51
        Me.DataGridView2.Size = New System.Drawing.Size(460, 336)
        Me.DataGridView2.TabIndex = 1
        Me.DataGridView2.Tag = "Zomer"
        '
        'GroupBox15
        '
        Me.GroupBox15.Controls.Add(Me.DataGridView3)
        Me.GroupBox15.Location = New System.Drawing.Point(6, 3)
        Me.GroupBox15.Name = "GroupBox15"
        Me.GroupBox15.Size = New System.Drawing.Size(470, 363)
        Me.GroupBox15.TabIndex = 1
        Me.GroupBox15.TabStop = False
        Me.GroupBox15.Text = "GroupBox15"
        '
        'DataGridView3
        '
        Me.DataGridView3.AllowUserToAddRows = False
        Me.DataGridView3.AllowUserToDeleteRows = False
        Me.DataGridView3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView3.Location = New System.Drawing.Point(6, 19)
        Me.DataGridView3.Name = "DataGridView3"
        Me.DataGridView3.RowHeadersWidth = 51
        Me.DataGridView3.Size = New System.Drawing.Size(460, 335)
        Me.DataGridView3.TabIndex = 0
        Me.DataGridView3.Tag = "Zomer"
        '
        'TabPage4
        '
        Me.TabPage4.Controls.Add(Me.GroupBox16)
        Me.TabPage4.Controls.Add(Me.GroupBox17)
        Me.TabPage4.Location = New System.Drawing.Point(4, 22)
        Me.TabPage4.Name = "TabPage4"
        Me.TabPage4.Size = New System.Drawing.Size(957, 374)
        Me.TabPage4.TabIndex = 7
        Me.TabPage4.Text = "Patronen"
        Me.TabPage4.UseVisualStyleBackColor = True
        '
        'GroupBox16
        '
        Me.GroupBox16.Controls.Add(Me.Label19)
        Me.GroupBox16.Controls.Add(Me.DataGridView4)
        Me.GroupBox16.Location = New System.Drawing.Point(479, 6)
        Me.GroupBox16.Name = "GroupBox16"
        Me.GroupBox16.Size = New System.Drawing.Size(470, 363)
        Me.GroupBox16.TabIndex = 4
        Me.GroupBox16.TabStop = False
        Me.GroupBox16.Text = "Winter"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(6, 344)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(77, 16)
        Me.Label19.TabIndex = 2
        Me.Label19.Text = "Checksum="
        '
        'DataGridView4
        '
        Me.DataGridView4.AllowUserToAddRows = False
        Me.DataGridView4.AllowUserToDeleteRows = False
        Me.DataGridView4.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView4.Location = New System.Drawing.Point(4, 18)
        Me.DataGridView4.Name = "DataGridView4"
        Me.DataGridView4.RowHeadersWidth = 51
        Me.DataGridView4.Size = New System.Drawing.Size(460, 310)
        Me.DataGridView4.TabIndex = 1
        Me.DataGridView4.Tag = "Zomer"
        '
        'GroupBox17
        '
        Me.GroupBox17.Controls.Add(Me.Label20)
        Me.GroupBox17.Controls.Add(Me.DataGridView5)
        Me.GroupBox17.Location = New System.Drawing.Point(7, 6)
        Me.GroupBox17.Name = "GroupBox17"
        Me.GroupBox17.Size = New System.Drawing.Size(470, 363)
        Me.GroupBox17.TabIndex = 3
        Me.GroupBox17.TabStop = False
        Me.GroupBox17.Text = "Zomer"
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(3, 344)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(77, 16)
        Me.Label20.TabIndex = 1
        Me.Label20.Text = "Checksum="
        '
        'DataGridView5
        '
        Me.DataGridView5.AllowUserToAddRows = False
        Me.DataGridView5.AllowUserToDeleteRows = False
        Me.DataGridView5.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView5.Location = New System.Drawing.Point(6, 19)
        Me.DataGridView5.Name = "DataGridView5"
        Me.DataGridView5.RowHeadersWidth = 51
        Me.DataGridView5.Size = New System.Drawing.Size(460, 309)
        Me.DataGridView5.TabIndex = 0
        Me.DataGridView5.Tag = "Zomer"
        '
        'TabPage5
        '
        Me.TabPage5.Controls.Add(Me.GroupBox18)
        Me.TabPage5.Controls.Add(Me.GroupBox19)
        Me.TabPage5.Location = New System.Drawing.Point(4, 22)
        Me.TabPage5.Name = "TabPage5"
        Me.TabPage5.Size = New System.Drawing.Size(957, 374)
        Me.TabPage5.TabIndex = 8
        Me.TabPage5.Text = "Grondwater"
        Me.TabPage5.UseVisualStyleBackColor = True
        '
        'GroupBox18
        '
        Me.GroupBox18.Controls.Add(Me.Button1)
        Me.GroupBox18.Controls.Add(Me.DataGridView6)
        Me.GroupBox18.Controls.Add(Me.Button2)
        Me.GroupBox18.Controls.Add(Me.Label21)
        Me.GroupBox18.Location = New System.Drawing.Point(479, 6)
        Me.GroupBox18.Name = "GroupBox18"
        Me.GroupBox18.Size = New System.Drawing.Size(470, 362)
        Me.GroupBox18.TabIndex = 6
        Me.GroupBox18.TabStop = False
        Me.GroupBox18.Text = "Winter"
        '
        'Button1
        '
        Me.Button1.BackColor = System.Drawing.Color.IndianRed
        Me.Button1.Location = New System.Drawing.Point(439, 331)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(25, 25)
        Me.Button1.TabIndex = 5
        Me.Button1.Text = "-"
        Me.Button1.UseVisualStyleBackColor = False
        '
        'DataGridView6
        '
        Me.DataGridView6.AllowUserToAddRows = False
        Me.DataGridView6.AllowUserToDeleteRows = False
        Me.DataGridView6.AllowUserToResizeRows = False
        Me.DataGridView6.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView6.Location = New System.Drawing.Point(4, 19)
        Me.DataGridView6.Name = "DataGridView6"
        Me.DataGridView6.RowHeadersWidth = 51
        Me.DataGridView6.Size = New System.Drawing.Size(460, 306)
        Me.DataGridView6.TabIndex = 2
        Me.DataGridView6.Tag = "Zomer"
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.Button2.Location = New System.Drawing.Point(411, 331)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(25, 25)
        Me.Button2.TabIndex = 4
        Me.Button2.Text = "+"
        Me.Button2.UseVisualStyleBackColor = False
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(6, 346)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(77, 16)
        Me.Label21.TabIndex = 2
        Me.Label21.Text = "Checksum="
        '
        'GroupBox19
        '
        Me.GroupBox19.Controls.Add(Me.Button3)
        Me.GroupBox19.Controls.Add(Me.Button4)
        Me.GroupBox19.Controls.Add(Me.Label22)
        Me.GroupBox19.Controls.Add(Me.DataGridView7)
        Me.GroupBox19.Location = New System.Drawing.Point(7, 6)
        Me.GroupBox19.Name = "GroupBox19"
        Me.GroupBox19.Size = New System.Drawing.Size(470, 365)
        Me.GroupBox19.TabIndex = 5
        Me.GroupBox19.TabStop = False
        Me.GroupBox19.Text = "Zomer"
        '
        'Button3
        '
        Me.Button3.BackColor = System.Drawing.Color.IndianRed
        Me.Button3.Location = New System.Drawing.Point(439, 331)
        Me.Button3.Name = "Button3"
        Me.Button3.Size = New System.Drawing.Size(25, 25)
        Me.Button3.TabIndex = 7
        Me.Button3.Text = "-"
        Me.Button3.UseVisualStyleBackColor = False
        '
        'Button4
        '
        Me.Button4.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.Button4.Location = New System.Drawing.Point(411, 331)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(25, 25)
        Me.Button4.TabIndex = 6
        Me.Button4.Text = "+"
        Me.Button4.UseVisualStyleBackColor = False
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(6, 349)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(77, 16)
        Me.Label22.TabIndex = 1
        Me.Label22.Text = "Checksum="
        '
        'DataGridView7
        '
        Me.DataGridView7.AllowUserToAddRows = False
        Me.DataGridView7.AllowUserToDeleteRows = False
        Me.DataGridView7.AllowUserToResizeRows = False
        Me.DataGridView7.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView7.Location = New System.Drawing.Point(6, 19)
        Me.DataGridView7.Name = "DataGridView7"
        Me.DataGridView7.RowHeadersWidth = 51
        Me.DataGridView7.Size = New System.Drawing.Size(460, 306)
        Me.DataGridView7.TabIndex = 0
        Me.DataGridView7.Tag = "Zomer"
        '
        'TabPage6
        '
        Me.TabPage6.Controls.Add(Me.DataGridView8)
        Me.TabPage6.Controls.Add(Me.GroupBox20)
        Me.TabPage6.Controls.Add(Me.GroupBox21)
        Me.TabPage6.Controls.Add(Me.DataGridView10)
        Me.TabPage6.Location = New System.Drawing.Point(4, 22)
        Me.TabPage6.Name = "TabPage6"
        Me.TabPage6.Size = New System.Drawing.Size(957, 374)
        Me.TabPage6.TabIndex = 10
        Me.TabPage6.Text = "Randvoorwaarden"
        Me.TabPage6.UseVisualStyleBackColor = True
        '
        'DataGridView8
        '
        Me.DataGridView8.AllowUserToAddRows = False
        Me.DataGridView8.AllowUserToDeleteRows = False
        Me.DataGridView8.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView8.Location = New System.Drawing.Point(9, 23)
        Me.DataGridView8.Name = "DataGridView8"
        Me.DataGridView8.RowHeadersWidth = 51
        Me.DataGridView8.Size = New System.Drawing.Size(463, 93)
        Me.DataGridView8.TabIndex = 2
        Me.DataGridView8.Tag = "Zomer"
        '
        'GroupBox20
        '
        Me.GroupBox20.Location = New System.Drawing.Point(3, 4)
        Me.GroupBox20.Name = "GroupBox20"
        Me.GroupBox20.Size = New System.Drawing.Size(519, 118)
        Me.GroupBox20.TabIndex = 4
        Me.GroupBox20.TabStop = False
        Me.GroupBox20.Text = "ModelKnopen"
        '
        'GroupBox21
        '
        Me.GroupBox21.Controls.Add(Me.DataGridView9)
        Me.GroupBox21.Location = New System.Drawing.Point(3, 128)
        Me.GroupBox21.Name = "GroupBox21"
        Me.GroupBox21.Size = New System.Drawing.Size(519, 243)
        Me.GroupBox21.TabIndex = 3
        Me.GroupBox21.TabStop = False
        Me.GroupBox21.Text = "Klassen"
        '
        'DataGridView9
        '
        Me.DataGridView9.AllowUserToAddRows = False
        Me.DataGridView9.AllowUserToDeleteRows = False
        Me.DataGridView9.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView9.Location = New System.Drawing.Point(6, 19)
        Me.DataGridView9.Name = "DataGridView9"
        Me.DataGridView9.RowHeadersWidth = 51
        Me.DataGridView9.Size = New System.Drawing.Size(507, 197)
        Me.DataGridView9.TabIndex = 1
        Me.DataGridView9.Tag = "Zomer"
        '
        'DataGridView10
        '
        Me.DataGridView10.AllowUserToAddRows = False
        Me.DataGridView10.AllowUserToDeleteRows = False
        Me.DataGridView10.AllowUserToResizeRows = False
        Me.DataGridView10.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView10.Location = New System.Drawing.Point(528, 0)
        Me.DataGridView10.Name = "DataGridView10"
        Me.DataGridView10.RowHeadersWidth = 51
        Me.DataGridView10.Size = New System.Drawing.Size(429, 306)
        Me.DataGridView10.TabIndex = 2
        Me.DataGridView10.Tag = "Zomer"
        '
        'TabPage7
        '
        Me.TabPage7.Controls.Add(Me.DataGridView11)
        Me.TabPage7.Location = New System.Drawing.Point(4, 22)
        Me.TabPage7.Name = "TabPage7"
        Me.TabPage7.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage7.Size = New System.Drawing.Size(957, 374)
        Me.TabPage7.TabIndex = 4
        Me.TabPage7.Text = "Uitvoer"
        Me.TabPage7.UseVisualStyleBackColor = True
        '
        'DataGridView11
        '
        Me.DataGridView11.AllowUserToAddRows = False
        Me.DataGridView11.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView11.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn7, Me.DataGridViewTextBoxColumn8, Me.DataGridViewTextBoxColumn9, Me.DataGridViewTextBoxColumn10, Me.DataGridViewTextBoxColumn11, Me.DataGridViewComboBoxColumn2})
        Me.DataGridView11.Location = New System.Drawing.Point(0, 0)
        Me.DataGridView11.Name = "DataGridView11"
        Me.DataGridView11.RowHeadersWidth = 51
        Me.DataGridView11.Size = New System.Drawing.Size(957, 374)
        Me.DataGridView11.TabIndex = 0
        '
        'DataGridViewTextBoxColumn7
        '
        Me.DataGridViewTextBoxColumn7.HeaderText = "ModelID"
        Me.DataGridViewTextBoxColumn7.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn7.Name = "DataGridViewTextBoxColumn7"
        Me.DataGridViewTextBoxColumn7.Width = 125
        '
        'DataGridViewTextBoxColumn8
        '
        Me.DataGridViewTextBoxColumn8.HeaderText = "Bestandsnaam"
        Me.DataGridViewTextBoxColumn8.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn8.Name = "DataGridViewTextBoxColumn8"
        Me.DataGridViewTextBoxColumn8.Width = 125
        '
        'DataGridViewTextBoxColumn9
        '
        Me.DataGridViewTextBoxColumn9.HeaderText = "Parameter"
        Me.DataGridViewTextBoxColumn9.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn9.Name = "DataGridViewTextBoxColumn9"
        Me.DataGridViewTextBoxColumn9.Width = 125
        '
        'DataGridViewTextBoxColumn10
        '
        Me.DataGridViewTextBoxColumn10.HeaderText = "Location ID"
        Me.DataGridViewTextBoxColumn10.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn10.Name = "DataGridViewTextBoxColumn10"
        Me.DataGridViewTextBoxColumn10.Width = 125
        '
        'DataGridViewTextBoxColumn11
        '
        Me.DataGridViewTextBoxColumn11.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn11.HeaderText = "Alias"
        Me.DataGridViewTextBoxColumn11.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn11.Name = "DataGridViewTextBoxColumn11"
        '
        'DataGridViewComboBoxColumn2
        '
        Me.DataGridViewComboBoxColumn2.HeaderText = "Uitvoersoort"
        Me.DataGridViewComboBoxColumn2.Items.AddRange(New Object() {"max", "min", "mean"})
        Me.DataGridViewComboBoxColumn2.MinimumWidth = 6
        Me.DataGridViewComboBoxColumn2.Name = "DataGridViewComboBoxColumn2"
        Me.DataGridViewComboBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.DataGridViewComboBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.DataGridViewComboBoxColumn2.Width = 125
        '
        'TabPage8
        '
        Me.TabPage8.Controls.Add(Me.Button5)
        Me.TabPage8.Controls.Add(Me.Label23)
        Me.TabPage8.Controls.Add(Me.Button6)
        Me.TabPage8.Controls.Add(Me.Label24)
        Me.TabPage8.Controls.Add(Me.Button7)
        Me.TabPage8.Controls.Add(Me.DataGridView12)
        Me.TabPage8.Controls.Add(Me.Button8)
        Me.TabPage8.Location = New System.Drawing.Point(4, 22)
        Me.TabPage8.Name = "TabPage8"
        Me.TabPage8.Size = New System.Drawing.Size(957, 374)
        Me.TabPage8.TabIndex = 5
        Me.TabPage8.Text = "Runs"
        Me.TabPage8.UseVisualStyleBackColor = True
        '
        'Button5
        '
        Me.Button5.BackColor = System.Drawing.Color.MediumOrchid
        Me.Button5.Enabled = False
        Me.Button5.Location = New System.Drawing.Point(532, 318)
        Me.Button5.Name = "Button5"
        Me.Button5.Size = New System.Drawing.Size(100, 50)
        Me.Button5.TabIndex = 25
        Me.Button5.Text = "Samenstellen"
        Me.Button5.UseVisualStyleBackColor = False
        '
        'Label23
        '
        Me.Label23.AutoSize = True
        Me.Label23.Location = New System.Drawing.Point(158, 352)
        Me.Label23.Name = "Label23"
        Me.Label23.Size = New System.Drawing.Size(80, 16)
        Me.Label23.TabIndex = 3
        Me.Label23.Text = "Aantal runs="
        '
        'Button6
        '
        Me.Button6.BackColor = System.Drawing.Color.IndianRed
        Me.Button6.Enabled = False
        Me.Button6.Location = New System.Drawing.Point(746, 318)
        Me.Button6.Name = "Button6"
        Me.Button6.Size = New System.Drawing.Size(100, 50)
        Me.Button6.TabIndex = 24
        Me.Button6.Text = "Stoppen"
        Me.Button6.UseVisualStyleBackColor = False
        '
        'Label24
        '
        Me.Label24.AutoSize = True
        Me.Label24.Location = New System.Drawing.Point(3, 352)
        Me.Label24.Name = "Label24"
        Me.Label24.Size = New System.Drawing.Size(77, 16)
        Me.Label24.TabIndex = 2
        Me.Label24.Text = "Checksum="
        '
        'Button7
        '
        Me.Button7.BackColor = System.Drawing.Color.Gold
        Me.Button7.Enabled = False
        Me.Button7.Location = New System.Drawing.Point(854, 318)
        Me.Button7.Name = "Button7"
        Me.Button7.Size = New System.Drawing.Size(100, 50)
        Me.Button7.TabIndex = 23
        Me.Button7.Text = "Nabewerken"
        Me.Button7.UseVisualStyleBackColor = False
        '
        'DataGridView12
        '
        Me.DataGridView12.AllowUserToAddRows = False
        Me.DataGridView12.AllowUserToDeleteRows = False
        Me.DataGridView12.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView12.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn12, Me.DataGridViewTextBoxColumn13, Me.DataGridViewTextBoxColumn14, Me.DataGridViewTextBoxColumn15, Me.DataGridViewTextBoxColumn16, Me.DataGridViewTextBoxColumn17, Me.DataGridViewTextBoxColumn18, Me.DataGridViewTextBoxColumn19, Me.DataGridViewTextBoxColumn20, Me.DataGridViewTextBoxColumn21, Me.DataGridViewTextBoxColumn22, Me.DataGridViewCheckBoxColumn1})
        Me.DataGridView12.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        Me.DataGridView12.Location = New System.Drawing.Point(0, 0)
        Me.DataGridView12.Name = "DataGridView12"
        Me.DataGridView12.RowHeadersWidth = 51
        Me.DataGridView12.Size = New System.Drawing.Size(954, 312)
        Me.DataGridView12.TabIndex = 1
        '
        'DataGridViewTextBoxColumn12
        '
        Me.DataGridViewTextBoxColumn12.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.DataGridViewTextBoxColumn12.HeaderText = "ID"
        Me.DataGridViewTextBoxColumn12.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn12.Name = "DataGridViewTextBoxColumn12"
        '
        'DataGridViewTextBoxColumn13
        '
        Me.DataGridViewTextBoxColumn13.HeaderText = "Seizoen"
        Me.DataGridViewTextBoxColumn13.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn13.Name = "DataGridViewTextBoxColumn13"
        Me.DataGridViewTextBoxColumn13.Width = 60
        '
        'DataGridViewTextBoxColumn14
        '
        Me.DataGridViewTextBoxColumn14.HeaderText = "Duur"
        Me.DataGridViewTextBoxColumn14.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn14.Name = "DataGridViewTextBoxColumn14"
        Me.DataGridViewTextBoxColumn14.Width = 60
        '
        'DataGridViewTextBoxColumn15
        '
        Me.DataGridViewTextBoxColumn15.HeaderText = "Volume"
        Me.DataGridViewTextBoxColumn15.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn15.Name = "DataGridViewTextBoxColumn15"
        Me.DataGridViewTextBoxColumn15.Width = 60
        '
        'DataGridViewTextBoxColumn16
        '
        Me.DataGridViewTextBoxColumn16.HeaderText = "Patroon"
        Me.DataGridViewTextBoxColumn16.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn16.Name = "DataGridViewTextBoxColumn16"
        Me.DataGridViewTextBoxColumn16.Width = 80
        '
        'DataGridViewTextBoxColumn17
        '
        Me.DataGridViewTextBoxColumn17.HeaderText = "ARF"
        Me.DataGridViewTextBoxColumn17.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn17.Name = "DataGridViewTextBoxColumn17"
        Me.DataGridViewTextBoxColumn17.Width = 60
        '
        'DataGridViewTextBoxColumn18
        '
        Me.DataGridViewTextBoxColumn18.HeaderText = "Grondwater"
        Me.DataGridViewTextBoxColumn18.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn18.Name = "DataGridViewTextBoxColumn18"
        Me.DataGridViewTextBoxColumn18.Width = 80
        '
        'DataGridViewTextBoxColumn19
        '
        Me.DataGridViewTextBoxColumn19.HeaderText = "Extra"
        Me.DataGridViewTextBoxColumn19.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn19.Name = "DataGridViewTextBoxColumn19"
        Me.DataGridViewTextBoxColumn19.Width = 80
        '
        'DataGridViewTextBoxColumn20
        '
        Me.DataGridViewTextBoxColumn20.HeaderText = "Randvoorwaarden"
        Me.DataGridViewTextBoxColumn20.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn20.Name = "DataGridViewTextBoxColumn20"
        Me.DataGridViewTextBoxColumn20.Width = 80
        '
        'DataGridViewTextBoxColumn21
        '
        Me.DataGridViewTextBoxColumn21.HeaderText = "Wind"
        Me.DataGridViewTextBoxColumn21.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn21.Name = "DataGridViewTextBoxColumn21"
        Me.DataGridViewTextBoxColumn21.Width = 80
        '
        'DataGridViewTextBoxColumn22
        '
        Me.DataGridViewTextBoxColumn22.HeaderText = "Freq."
        Me.DataGridViewTextBoxColumn22.MinimumWidth = 6
        Me.DataGridViewTextBoxColumn22.Name = "DataGridViewTextBoxColumn22"
        Me.DataGridViewTextBoxColumn22.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.DataGridViewTextBoxColumn22.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        Me.DataGridViewTextBoxColumn22.Width = 80
        '
        'DataGridViewCheckBoxColumn1
        '
        Me.DataGridViewCheckBoxColumn1.HeaderText = "DONE"
        Me.DataGridViewCheckBoxColumn1.MinimumWidth = 6
        Me.DataGridViewCheckBoxColumn1.Name = "DataGridViewCheckBoxColumn1"
        Me.DataGridViewCheckBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.DataGridViewCheckBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.DataGridViewCheckBoxColumn1.Width = 60
        '
        'Button8
        '
        Me.Button8.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.Button8.Enabled = False
        Me.Button8.Location = New System.Drawing.Point(638, 318)
        Me.Button8.Name = "Button8"
        Me.Button8.Size = New System.Drawing.Size(100, 50)
        Me.Button8.TabIndex = 3
        Me.Button8.Text = "Starten"
        Me.Button8.UseVisualStyleBackColor = False
        '
        'TabPage9
        '
        Me.TabPage9.Controls.Add(Me.ComboBox3)
        Me.TabPage9.Location = New System.Drawing.Point(4, 22)
        Me.TabPage9.Name = "TabPage9"
        Me.TabPage9.Size = New System.Drawing.Size(957, 374)
        Me.TabPage9.TabIndex = 9
        Me.TabPage9.Text = "Grafieken"
        Me.TabPage9.UseVisualStyleBackColor = True
        '
        'ComboBox3
        '
        Me.ComboBox3.FormattingEnabled = True
        Me.ComboBox3.Location = New System.Drawing.Point(17, 17)
        Me.ComboBox3.Name = "ComboBox3"
        Me.ComboBox3.Size = New System.Drawing.Size(154, 24)
        Me.ComboBox3.TabIndex = 0
        '
        'Button9
        '
        Me.Button9.BackColor = System.Drawing.Color.Yellow
        Me.Button9.Location = New System.Drawing.Point(384, 19)
        Me.Button9.Name = "Button9"
        Me.Button9.Size = New System.Drawing.Size(25, 25)
        Me.Button9.TabIndex = 10
        Me.Button9.Text = ".."
        Me.Button9.UseVisualStyleBackColor = False
        '
        'DataGridView13
        '
        Me.DataGridView13.AllowUserToAddRows = False
        Me.DataGridView13.AllowUserToDeleteRows = False
        Me.DataGridView13.AllowUserToResizeRows = False
        Me.DataGridView13.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView13.Location = New System.Drawing.Point(6, 16)
        Me.DataGridView13.Name = "DataGridView13"
        Me.DataGridView13.RowHeadersWidth = 51
        Me.DataGridView13.Size = New System.Drawing.Size(370, 342)
        Me.DataGridView13.TabIndex = 2
        Me.DataGridView13.Tag = "Zomer"
        '
        'DataGridView14
        '
        Me.DataGridView14.AllowUserToAddRows = False
        Me.DataGridView14.AllowUserToDeleteRows = False
        Me.DataGridView14.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView14.Location = New System.Drawing.Point(6, 16)
        Me.DataGridView14.Name = "DataGridView14"
        Me.DataGridView14.RowHeadersWidth = 51
        Me.DataGridView14.Size = New System.Drawing.Size(476, 93)
        Me.DataGridView14.TabIndex = 2
        Me.DataGridView14.Tag = "Zomer"
        '
        'Button10
        '
        Me.Button10.BackColor = System.Drawing.Color.IndianRed
        Me.Button10.Location = New System.Drawing.Point(488, 47)
        Me.Button10.Name = "Button10"
        Me.Button10.Size = New System.Drawing.Size(25, 25)
        Me.Button10.TabIndex = 9
        Me.Button10.Text = "-"
        Me.Button10.UseVisualStyleBackColor = False
        '
        'Button11
        '
        Me.Button11.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.Button11.Location = New System.Drawing.Point(488, 16)
        Me.Button11.Name = "Button11"
        Me.Button11.Size = New System.Drawing.Size(25, 25)
        Me.Button11.TabIndex = 8
        Me.Button11.Text = "+"
        Me.Button11.UseVisualStyleBackColor = False
        '
        'Button12
        '
        Me.Button12.BackColor = System.Drawing.Color.IndianRed
        Me.Button12.Location = New System.Drawing.Point(488, 50)
        Me.Button12.Name = "Button12"
        Me.Button12.Size = New System.Drawing.Size(25, 25)
        Me.Button12.TabIndex = 11
        Me.Button12.Text = "-"
        Me.Button12.UseVisualStyleBackColor = False
        '
        'Button13
        '
        Me.Button13.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.Button13.Location = New System.Drawing.Point(488, 19)
        Me.Button13.Name = "Button13"
        Me.Button13.Size = New System.Drawing.Size(25, 25)
        Me.Button13.TabIndex = 10
        Me.Button13.Text = "+"
        Me.Button13.UseVisualStyleBackColor = False
        '
        'DataGridView15
        '
        Me.DataGridView15.AllowUserToAddRows = False
        Me.DataGridView15.AllowUserToDeleteRows = False
        Me.DataGridView15.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView15.Location = New System.Drawing.Point(6, 19)
        Me.DataGridView15.Name = "DataGridView15"
        Me.DataGridView15.RowHeadersWidth = 51
        Me.DataGridView15.Size = New System.Drawing.Size(476, 197)
        Me.DataGridView15.TabIndex = 1
        Me.DataGridView15.Tag = "Zomer"
        '
        'lblProgress
        '
        Me.lblProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(19, 661)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(73, 16)
        Me.lblProgress.TabIndex = 23
        Me.lblProgress.Text = "Voortgang:"
        '
        'prProgress
        '
        Me.prProgress.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.prProgress.Location = New System.Drawing.Point(19, 692)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(4)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(1675, 28)
        Me.prProgress.TabIndex = 24
        '
        'frmStochasten
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1709, 735)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.tabStochastentool)
        Me.Controls.Add(Me.mnuMenu)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.mnuMenu
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "frmStochasten"
        Me.Text = "De Nieuwe StochastenTool"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.mnuMenu.ResumeLayout(False)
        Me.mnuMenu.PerformLayout()
        Me.tabStochastentool.ResumeLayout(False)
        Me.tabSettings.ResumeLayout(False)
        Me.tabSettings.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.grMeteo.ResumeLayout(False)
        CType(Me.grMeteoStations, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grNabewerking.ResumeLayout(False)
        Me.grNabewerking.PerformLayout()
        Me.grBerekeningen.ResumeLayout(False)
        Me.grBerekeningen.PerformLayout()
        Me.grBestanden.ResumeLayout(False)
        Me.grBestanden.PerformLayout()
        Me.tabSobek.ResumeLayout(False)
        CType(Me.grModels, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabSeizoenen.ResumeLayout(False)
        Me.tabSeizoenen.PerformLayout()
        CType(Me.grSeizoenen, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabBoundaryNodes.ResumeLayout(False)
        Me.GroupBox23.ResumeLayout(False)
        CType(Me.grBoundaryNodes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabWaterlevels.ResumeLayout(False)
        Me.grWLChart.ResumeLayout(False)
        CType(Me.chartBoundaries, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grWLClasses.ResumeLayout(False)
        Me.grWLClasses.PerformLayout()
        CType(Me.grWaterLevelSeries, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.grWaterLevelClasses, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabWind.ResumeLayout(False)
        Me.GroupBox22.ResumeLayout(False)
        CType(Me.grWindSeries, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox24.ResumeLayout(False)
        Me.GroupBox24.PerformLayout()
        CType(Me.grWindKlassen, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabPostprocessing.ResumeLayout(False)
        Me.tabOutput.ResumeLayout(False)
        Me.tab1D.ResumeLayout(False)
        CType(Me.grOutputLocations, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tab2D.ResumeLayout(False)
        Me.tab2D.PerformLayout()
        Me.TabRuns.ResumeLayout(False)
        Me.TabRuns.PerformLayout()
        CType(Me.grRuns, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage1.ResumeLayout(False)
        Me.GroupBox10.ResumeLayout(False)
        Me.GroupBox10.PerformLayout()
        Me.GroupBox11.ResumeLayout(False)
        Me.GroupBox11.PerformLayout()
        Me.GroupBox12.ResumeLayout(False)
        Me.GroupBox12.PerformLayout()
        Me.GroupBox13.ResumeLayout(False)
        Me.GroupBox13.PerformLayout()
        Me.TabPage2.ResumeLayout(False)
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage3.ResumeLayout(False)
        Me.GroupBox14.ResumeLayout(False)
        CType(Me.DataGridView2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox15.ResumeLayout(False)
        CType(Me.DataGridView3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage4.ResumeLayout(False)
        Me.GroupBox16.ResumeLayout(False)
        Me.GroupBox16.PerformLayout()
        CType(Me.DataGridView4, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox17.ResumeLayout(False)
        Me.GroupBox17.PerformLayout()
        CType(Me.DataGridView5, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage5.ResumeLayout(False)
        Me.GroupBox18.ResumeLayout(False)
        Me.GroupBox18.PerformLayout()
        CType(Me.DataGridView6, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox19.ResumeLayout(False)
        Me.GroupBox19.PerformLayout()
        CType(Me.DataGridView7, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage6.ResumeLayout(False)
        CType(Me.DataGridView8, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox21.ResumeLayout(False)
        CType(Me.DataGridView9, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DataGridView10, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage7.ResumeLayout(False)
        CType(Me.DataGridView11, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage8.ResumeLayout(False)
        Me.TabPage8.PerformLayout()
        CType(Me.DataGridView12, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage9.ResumeLayout(False)
        CType(Me.DataGridView13, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DataGridView14, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DataGridView15, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents mnuMenu As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenXMLToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SaveXMLToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents tabStochastentool As System.Windows.Forms.TabControl
    Friend WithEvents tabSettings As System.Windows.Forms.TabPage
    Friend WithEvents cmbDuration As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents tabSobek As System.Windows.Forms.TabPage
    Friend WithEvents tabPostprocessing As System.Windows.Forms.TabPage
    Friend WithEvents dlgOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents txtUitloop As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents txtInputDir As System.Windows.Forms.TextBox
    Friend WithEvents grModels As System.Windows.Forms.DataGridView
    Friend WithEvents txtDatabase As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents TabRuns As System.Windows.Forms.TabPage
    Friend WithEvents grRuns As System.Windows.Forms.DataGridView
    Friend WithEvents grBerekeningen As System.Windows.Forms.GroupBox
    Friend WithEvents grBestanden As System.Windows.Forms.GroupBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents cmbClimate As System.Windows.Forms.ComboBox
    Friend WithEvents grMeteo As System.Windows.Forms.GroupBox
    Friend WithEvents grNabewerking As System.Windows.Forms.GroupBox
    Friend WithEvents txtResultatenDir As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents grOutputLocations As System.Windows.Forms.DataGridView
    Friend WithEvents tabVolumes As System.Windows.Forms.TabPage
    Friend WithEvents tabPatronen As System.Windows.Forms.TabPage
    Friend WithEvents tabGrondwater As System.Windows.Forms.TabPage
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents tabWaterlevels As System.Windows.Forms.TabPage
    Friend WithEvents grWaterLevelClasses As System.Windows.Forms.DataGridView
    Friend WithEvents grWLClasses As System.Windows.Forms.GroupBox
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox10 As System.Windows.Forms.GroupBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox11 As System.Windows.Forms.GroupBox
    Friend WithEvents ComboBox1 As System.Windows.Forms.ComboBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents GroupBox12 As System.Windows.Forms.GroupBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents ComboBox2 As System.Windows.Forms.ComboBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents TextBox3 As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox13 As System.Windows.Forms.GroupBox
    Friend WithEvents TextBox4 As System.Windows.Forms.TextBox
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents TextBox5 As System.Windows.Forms.TextBox
    Friend WithEvents TextBox6 As System.Windows.Forms.TextBox
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewComboBoxColumn1 As System.Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn4 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn5 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn6 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox14 As System.Windows.Forms.GroupBox
    Friend WithEvents DataGridView2 As System.Windows.Forms.DataGridView
    Friend WithEvents GroupBox15 As System.Windows.Forms.GroupBox
    Friend WithEvents DataGridView3 As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage4 As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox16 As System.Windows.Forms.GroupBox
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents DataGridView4 As System.Windows.Forms.DataGridView
    Friend WithEvents GroupBox17 As System.Windows.Forms.GroupBox
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents DataGridView5 As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage5 As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox18 As System.Windows.Forms.GroupBox
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents DataGridView6 As System.Windows.Forms.DataGridView
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents GroupBox19 As System.Windows.Forms.GroupBox
    Friend WithEvents Button3 As System.Windows.Forms.Button
    Friend WithEvents Button4 As System.Windows.Forms.Button
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents DataGridView7 As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage6 As System.Windows.Forms.TabPage
    Friend WithEvents DataGridView8 As System.Windows.Forms.DataGridView
    Friend WithEvents GroupBox20 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox21 As System.Windows.Forms.GroupBox
    Friend WithEvents DataGridView9 As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridView10 As System.Windows.Forms.DataGridView
    Friend WithEvents TabPage7 As System.Windows.Forms.TabPage
    Friend WithEvents DataGridView11 As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridViewTextBoxColumn7 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn8 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn9 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn10 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn11 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewComboBoxColumn2 As System.Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents TabPage8 As System.Windows.Forms.TabPage
    Friend WithEvents Button5 As System.Windows.Forms.Button
    Friend WithEvents Label23 As System.Windows.Forms.Label
    Friend WithEvents Button6 As System.Windows.Forms.Button
    Friend WithEvents Label24 As System.Windows.Forms.Label
    Friend WithEvents Button7 As System.Windows.Forms.Button
    Friend WithEvents DataGridView12 As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridViewTextBoxColumn12 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn13 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn14 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn15 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn16 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn17 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn18 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn19 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn20 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn21 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewTextBoxColumn22 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents DataGridViewCheckBoxColumn1 As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents Button8 As System.Windows.Forms.Button
    Friend WithEvents TabPage9 As System.Windows.Forms.TabPage
    Friend WithEvents ComboBox3 As System.Windows.Forms.ComboBox
    Friend WithEvents btnDeleteBoundaryClass As System.Windows.Forms.Button
    Friend WithEvents btnAddBoundaryClass As System.Windows.Forms.Button
    Friend WithEvents Button9 As System.Windows.Forms.Button
    Friend WithEvents DataGridView13 As System.Windows.Forms.DataGridView
    Friend WithEvents DataGridView14 As System.Windows.Forms.DataGridView
    Friend WithEvents Button10 As System.Windows.Forms.Button
    Friend WithEvents Button11 As System.Windows.Forms.Button
    Friend WithEvents Button12 As System.Windows.Forms.Button
    Friend WithEvents Button13 As System.Windows.Forms.Button
    Friend WithEvents DataGridView15 As System.Windows.Forms.DataGridView
    Friend WithEvents grWLChart As System.Windows.Forms.GroupBox
    Friend WithEvents grWaterLevelSeries As System.Windows.Forms.DataGridView
    Friend WithEvents tabBoundaryNodes As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox23 As System.Windows.Forms.GroupBox
    Friend WithEvents btnDeleteBoundaryNode As System.Windows.Forms.Button
    Friend WithEvents btnAddBoundaryNode As System.Windows.Forms.Button
    Friend WithEvents grBoundaryNodes As System.Windows.Forms.DataGridView
    Friend WithEvents tabWind As System.Windows.Forms.TabPage
    Friend WithEvents GroupBox24 As System.Windows.Forms.GroupBox
    Friend WithEvents btnDeleteWindClass As System.Windows.Forms.Button
    Friend WithEvents btnAddWindClass As System.Windows.Forms.Button
    Friend WithEvents grWindKlassen As System.Windows.Forms.DataGridView
    Friend WithEvents EditToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PasteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GroupBox22 As System.Windows.Forms.GroupBox
    Friend WithEvents grWindSeries As System.Windows.Forms.DataGridView
    Friend WithEvents tabExtra As System.Windows.Forms.TabPage
    Friend WithEvents lblBoundaryChecksum As System.Windows.Forms.Label
    Friend WithEvents lblWindChecksum As System.Windows.Forms.Label
    Friend WithEvents ToolsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GrondwatersClassificerenToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GetijdenClassificerenToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents chartBoundaries As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents btnRemoveMeteoStation As System.Windows.Forms.Button
    Friend WithEvents btnAddMeteoStation As System.Windows.Forms.Button
    Friend WithEvents grMeteoStations As System.Windows.Forms.DataGridView
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents txtResultsStartPercentage As System.Windows.Forms.TextBox
    Friend WithEvents AboutToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents chkUseCrashedResults As System.Windows.Forms.CheckBox
    Friend WithEvents dlgSaveFile As System.Windows.Forms.SaveFileDialog
    Friend WithEvents tabSeizoenen As TabPage
    Friend WithEvents btnRemoveSeason As Button
    Friend WithEvents btnAddSeason As Button
    Friend WithEvents lblCheckSumSeizoenen As Label
    Friend WithEvents grSeizoenen As DataGridView
    Friend WithEvents lblSelected As Label
    Friend WithEvents lblnRuns As Label
    Friend WithEvents lblCheckSumRuns As Label
    Friend WithEvents DirectoriesHernoemenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents MappenBeherenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents StochastendirectoriesHernoemenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents MappenVerwijderenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents KaartToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents VerschilkaartToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents AchtergrondkaartToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents mnuGoogleMaps As ToolStripMenuItem
    Friend WithEvents mnuBingMaps As ToolStripMenuItem
    Friend WithEvents mnuHybrid As ToolStripMenuItem
    Friend WithEvents btnWissen As Button
    Friend WithEvents btnPopulateRuns As Button
    Friend WithEvents btnExport As Button
    Friend WithEvents btnPostprocessing As Button
    Friend WithEvents btnBuild As Button
    Friend WithEvents mnuNoMap As ToolStripMenuItem
    Friend WithEvents mnuOSM As ToolStripMenuItem
    'Friend WithEvents rngYAxis As Syncfusion.Windows.Forms.Tools.RangeSlider
    Friend WithEvents GrafiekenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ReferentiepeilenUitShapefileToevoegenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DatabaseToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents UpgradeNaarToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToonPolygonenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents PolygonenUitShapefileToevoegenToolStripMenuItem1 As ToolStripMenuItem
    'Friend WithEvents AxMap1 As AxMapWinGIS.AxMap
    Friend WithEvents ImporterenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents VolumesUitCSVToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    'Friend WithEvents AxMap1 As AxMapWinGIS.AxMap
    Friend WithEvents ConverterenVanAccessNaarSQLiteToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents LegeDatabaseCreërenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents btnViewer As Button
    Friend WithEvents dlgFolder As FolderBrowserDialog
    Friend WithEvents Label37 As Label
    Friend WithEvents Label36 As Label
    Friend WithEvents cmbZomerpeil As ComboBox
    Friend WithEvents cmbWinterpeil As ComboBox
    Friend WithEvents txtPeilgebieden As TextBox
    Friend WithEvents Label35 As Label
    Friend WithEvents btnRemoveOutputLocation As Button
    Friend WithEvents ModelsToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents btnShapefile As Button
    Friend WithEvents SaveXMLToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents btnDatabase As Button
    Friend WithEvents btnResultsDir As Button
    Friend WithEvents btnInputDir As Button
    Friend WithEvents btnUitlezen As Button
    Friend WithEvents VerwijderenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents VolumesSTOWA2014ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents VolumesSTOWA2019ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents btnCopy As Button
    Friend WithEvents btnWindCopy As Button
    Friend WithEvents lblProgress As Label
    Friend WithEvents prProgress As ProgressBar
    Friend WithEvents RandvoorwaardenUitCSVToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SOBEKToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DHydroToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents BetafunctiesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents btnDeleteModel As Button
    Friend WithEvents btnAddModel As Button
    Friend WithEvents btnOutputDir As Button
    Friend WithEvents Label15 As Label
    Friend WithEvents txtOutputDir As TextBox
    Friend WithEvents UitvoerlocatiesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ImporterenToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents AlleVerwijderenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ModelToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ToevoegenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents tabOutput As TabControl
    Friend WithEvents tab1D As TabPage
    Friend WithEvents tab2D As TabPage
    Friend WithEvents radFou As RadioButton
    Friend WithEvents AlleResultatenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents chk2D As CheckBox
    Friend WithEvents chk1D As CheckBox
    Friend WithEvents LeesMATFileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents btnSimulate As Button
    Friend WithEvents btnCopyResults As Button
    Friend WithEvents HBVToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents chkRemoveSimulationAfterCopyingResultsFiles As CheckBox
    Friend WithEvents btnExtraFilesDir As Button
    Friend WithEvents Label16 As Label
    Friend WithEvents txtExtraFilesDir As TextBox
    Friend WithEvents VoorgedefinieerdToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents SumaquasHertogenboschToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents Label4 As Label
    Friend WithEvents txtMaxParallel As TextBox
    Friend WithEvents hlpModellen As Button
    Friend WithEvents hlpSeizoenen As Button
    Friend WithEvents hlpInvoermap2 As Button
    Friend WithEvents hlbUitvoermap2 As Button
    Friend WithEvents hlpResultatenmap2 As Button
    Friend WithEvents hlpExtraFiles2 As Button
    Friend WithEvents hlpDatabase2 As Button
    Friend WithEvents hlpShapefilePeilgebieden2 As Button
    Friend WithEvents hlpVeldWinterpeil2 As Button
    Friend WithEvents hlpVeldZomerpeil2 As Button
    Friend WithEvents hlpMeteo2 As Button
    Friend WithEvents hlpBerekeningen2 As Button
    Friend WithEvents hlpOutput2 As Button
    Friend WithEvents hlpNabewerking2 As Button
    Friend WithEvents NeerslagpatronenUitCSVToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents NeerslagpatronenkansUitCSVToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents TestGebiedsreductieToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents TestNetCDFFileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents txtMaxWaitTimeHours As TextBox
    Friend WithEvents Label25 As Label
End Class
