<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmClassifyTidesByStatistics
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmClassifyTidesByStatistics))
        Me.btnOpenCSV = New System.Windows.Forms.Button()
        Me.txtCSVFile = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtMultiplier = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnTimeNotation = New System.Windows.Forms.Button()
        Me.btnDateNotation = New System.Windows.Forms.Button()
        Me.txtTimeNotation = New System.Windows.Forms.TextBox()
        Me.txtDateNotation = New System.Windows.Forms.TextBox()
        Me.cmbTimeField = New System.Windows.Forms.ComboBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.cmbDataField = New System.Windows.Forms.ComboBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.cmbDateField = New System.Windows.Forms.ComboBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.txtSeriesName = New System.Windows.Forms.TextBox()
        Me.btnImport = New System.Windows.Forms.Button()
        Me.cmbPercentage = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtUitloop = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.btnAnalyze = New System.Windows.Forms.Button()
        Me.cmbDuur = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.dlgSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.grClassificatie = New System.Windows.Forms.GroupBox()
        Me.btnInfo = New System.Windows.Forms.Button()
        Me.btnRemoveElevationClass = New System.Windows.Forms.Button()
        Me.btnAddElevationClass = New System.Windows.Forms.Button()
        Me.grPercentileClasses = New System.Windows.Forms.DataGridView()
        Me.colNaam = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.collBound = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.coluBound = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.cmbTidalComponent = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.btnQAMethod = New System.Windows.Forms.Button()
        Me.radEenvoudig = New System.Windows.Forms.RadioButton()
        Me.radUitgebreid = New System.Windows.Forms.RadioButton()
        Me.grAmplitudeKlassen = New System.Windows.Forms.DataGridView()
        Me.colClassName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colVanPerc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTotPerc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.btnRemoveAmplitudeClass = New System.Windows.Forms.Button()
        Me.btnAddAmplitudeClass = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout()
        Me.grClassificatie.SuspendLayout()
        CType(Me.grPercentileClasses, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox2.SuspendLayout()
        CType(Me.grAmplitudeKlassen, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox3.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnOpenCSV
        '
        Me.btnOpenCSV.Location = New System.Drawing.Point(490, 23)
        Me.btnOpenCSV.Margin = New System.Windows.Forms.Padding(4)
        Me.btnOpenCSV.Name = "btnOpenCSV"
        Me.btnOpenCSV.Size = New System.Drawing.Size(24, 22)
        Me.btnOpenCSV.TabIndex = 0
        Me.btnOpenCSV.Text = ".."
        Me.btnOpenCSV.UseVisualStyleBackColor = True
        '
        'txtCSVFile
        '
        Me.txtCSVFile.Location = New System.Drawing.Point(125, 23)
        Me.txtCSVFile.Margin = New System.Windows.Forms.Padding(4)
        Me.txtCSVFile.Name = "txtCSVFile"
        Me.txtCSVFile.Size = New System.Drawing.Size(357, 22)
        Me.txtCSVFile.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(8, 32)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(91, 17)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "CSV-bestand"
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtMultiplier)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.btnTimeNotation)
        Me.GroupBox1.Controls.Add(Me.btnDateNotation)
        Me.GroupBox1.Controls.Add(Me.txtTimeNotation)
        Me.GroupBox1.Controls.Add(Me.txtDateNotation)
        Me.GroupBox1.Controls.Add(Me.cmbTimeField)
        Me.GroupBox1.Controls.Add(Me.Label11)
        Me.GroupBox1.Controls.Add(Me.cmbDataField)
        Me.GroupBox1.Controls.Add(Me.Label10)
        Me.GroupBox1.Controls.Add(Me.cmbDateField)
        Me.GroupBox1.Controls.Add(Me.Label9)
        Me.GroupBox1.Controls.Add(Me.Label8)
        Me.GroupBox1.Controls.Add(Me.txtSeriesName)
        Me.GroupBox1.Controls.Add(Me.btnImport)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.btnOpenCSV)
        Me.GroupBox1.Controls.Add(Me.txtCSVFile)
        Me.GroupBox1.Location = New System.Drawing.Point(16, 15)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(535, 218)
        Me.GroupBox1.TabIndex = 5
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Gegevens"
        '
        'txtMultiplier
        '
        Me.txtMultiplier.Location = New System.Drawing.Point(125, 173)
        Me.txtMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMultiplier.Name = "txtMultiplier"
        Me.txtMultiplier.Size = New System.Drawing.Size(183, 22)
        Me.txtMultiplier.TabIndex = 40
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 179)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(68, 17)
        Me.Label2.TabIndex = 39
        Me.Label2.Text = "Multiplier:"
        '
        'btnTimeNotation
        '
        Me.btnTimeNotation.BackColor = System.Drawing.Color.Gold
        Me.btnTimeNotation.Location = New System.Drawing.Point(490, 114)
        Me.btnTimeNotation.Margin = New System.Windows.Forms.Padding(4)
        Me.btnTimeNotation.Name = "btnTimeNotation"
        Me.btnTimeNotation.Size = New System.Drawing.Size(24, 22)
        Me.btnTimeNotation.TabIndex = 38
        Me.btnTimeNotation.Text = "?"
        Me.btnTimeNotation.UseVisualStyleBackColor = False
        '
        'btnDateNotation
        '
        Me.btnDateNotation.BackColor = System.Drawing.Color.Gold
        Me.btnDateNotation.Location = New System.Drawing.Point(490, 82)
        Me.btnDateNotation.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDateNotation.Name = "btnDateNotation"
        Me.btnDateNotation.Size = New System.Drawing.Size(24, 22)
        Me.btnDateNotation.TabIndex = 37
        Me.btnDateNotation.Text = "?"
        Me.btnDateNotation.UseVisualStyleBackColor = False
        '
        'txtTimeNotation
        '
        Me.txtTimeNotation.Location = New System.Drawing.Point(315, 114)
        Me.txtTimeNotation.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTimeNotation.Name = "txtTimeNotation"
        Me.txtTimeNotation.Size = New System.Drawing.Size(167, 22)
        Me.txtTimeNotation.TabIndex = 36
        '
        'txtDateNotation
        '
        Me.txtDateNotation.Location = New System.Drawing.Point(315, 82)
        Me.txtDateNotation.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDateNotation.Name = "txtDateNotation"
        Me.txtDateNotation.Size = New System.Drawing.Size(167, 22)
        Me.txtDateNotation.TabIndex = 35
        '
        'cmbTimeField
        '
        Me.cmbTimeField.FormattingEnabled = True
        Me.cmbTimeField.Location = New System.Drawing.Point(125, 112)
        Me.cmbTimeField.Name = "cmbTimeField"
        Me.cmbTimeField.Size = New System.Drawing.Size(183, 24)
        Me.cmbTimeField.TabIndex = 34
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(9, 115)
        Me.Label11.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(61, 17)
        Me.Label11.TabIndex = 33
        Me.Label11.Text = "Tijdveld:"
        '
        'cmbDataField
        '
        Me.cmbDataField.FormattingEnabled = True
        Me.cmbDataField.Location = New System.Drawing.Point(125, 142)
        Me.cmbDataField.Name = "cmbDataField"
        Me.cmbDataField.Size = New System.Drawing.Size(183, 24)
        Me.cmbDataField.TabIndex = 32
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(9, 145)
        Me.Label10.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(103, 17)
        Me.Label10.TabIndex = 31
        Me.Label10.Text = "Gegevensveld:"
        '
        'cmbDateField
        '
        Me.cmbDateField.FormattingEnabled = True
        Me.cmbDateField.Location = New System.Drawing.Point(125, 82)
        Me.cmbDateField.Name = "cmbDateField"
        Me.cmbDateField.Size = New System.Drawing.Size(183, 24)
        Me.cmbDateField.TabIndex = 30
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(9, 85)
        Me.Label9.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(79, 17)
        Me.Label9.TabIndex = 29
        Me.Label9.Text = "Datumveld:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(12, 58)
        Me.Label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(87, 17)
        Me.Label8.TabIndex = 28
        Me.Label8.Text = "Reeksnaam:"
        '
        'txtSeriesName
        '
        Me.txtSeriesName.Location = New System.Drawing.Point(125, 53)
        Me.txtSeriesName.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSeriesName.Name = "txtSeriesName"
        Me.txtSeriesName.Size = New System.Drawing.Size(183, 22)
        Me.txtSeriesName.TabIndex = 27
        '
        'btnImport
        '
        Me.btnImport.BackColor = System.Drawing.Color.LightSeaGreen
        Me.btnImport.Location = New System.Drawing.Point(411, 152)
        Me.btnImport.Margin = New System.Windows.Forms.Padding(4)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(107, 49)
        Me.btnImport.TabIndex = 26
        Me.btnImport.Text = "Import"
        Me.btnImport.UseVisualStyleBackColor = False
        '
        'cmbPercentage
        '
        Me.cmbPercentage.FormattingEnabled = True
        Me.cmbPercentage.Location = New System.Drawing.Point(228, 92)
        Me.cmbPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbPercentage.Name = "cmbPercentage"
        Me.cmbPercentage.Size = New System.Drawing.Size(137, 24)
        Me.cmbPercentage.TabIndex = 32
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(16, 95)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(194, 17)
        Me.Label5.TabIndex = 31
        Me.Label5.Text = "Verhogingen in het laatste %:"
        '
        'txtUitloop
        '
        Me.txtUitloop.Location = New System.Drawing.Point(228, 60)
        Me.txtUitloop.Margin = New System.Windows.Forms.Padding(4)
        Me.txtUitloop.Name = "txtUitloop"
        Me.txtUitloop.Size = New System.Drawing.Size(137, 22)
        Me.txtUitloop.TabIndex = 29
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(16, 63)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(201, 17)
        Me.Label4.TabIndex = 30
        Me.Label4.Text = "Uitloop gebeurtenissen (uren):"
        '
        'btnAnalyze
        '
        Me.btnAnalyze.BackColor = System.Drawing.Color.MediumOrchid
        Me.btnAnalyze.Enabled = False
        Me.btnAnalyze.Location = New System.Drawing.Point(852, 577)
        Me.btnAnalyze.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAnalyze.Name = "btnAnalyze"
        Me.btnAnalyze.Size = New System.Drawing.Size(107, 49)
        Me.btnAnalyze.TabIndex = 29
        Me.btnAnalyze.Text = "Analyze"
        Me.btnAnalyze.UseVisualStyleBackColor = False
        '
        'cmbDuur
        '
        Me.cmbDuur.FormattingEnabled = True
        Me.cmbDuur.Location = New System.Drawing.Point(228, 27)
        Me.cmbDuur.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbDuur.Name = "cmbDuur"
        Me.cmbDuur.Size = New System.Drawing.Size(137, 24)
        Me.cmbDuur.TabIndex = 26
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(16, 31)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(188, 17)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Duur gebeurtenissen (uren):"
        '
        'btnExport
        '
        Me.btnExport.BackColor = System.Drawing.Color.Gold
        Me.btnExport.Enabled = False
        Me.btnExport.Location = New System.Drawing.Point(967, 576)
        Me.btnExport.Margin = New System.Windows.Forms.Padding(4)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(107, 49)
        Me.btnExport.TabIndex = 24
        Me.btnExport.Text = "Export"
        Me.btnExport.UseVisualStyleBackColor = False
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(16, 592)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(4)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(828, 28)
        Me.prProgress.TabIndex = 25
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(16, 572)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(78, 17)
        Me.lblProgress.TabIndex = 11
        Me.lblProgress.Text = "Voortgang:"
        '
        'grClassificatie
        '
        Me.grClassificatie.Controls.Add(Me.btnInfo)
        Me.grClassificatie.Controls.Add(Me.btnRemoveElevationClass)
        Me.grClassificatie.Controls.Add(Me.btnAddElevationClass)
        Me.grClassificatie.Controls.Add(Me.grPercentileClasses)
        Me.grClassificatie.Location = New System.Drawing.Point(559, 240)
        Me.grClassificatie.Margin = New System.Windows.Forms.Padding(4)
        Me.grClassificatie.Name = "grClassificatie"
        Me.grClassificatie.Padding = New System.Windows.Forms.Padding(4)
        Me.grClassificatie.Size = New System.Drawing.Size(523, 329)
        Me.grClassificatie.TabIndex = 38
        Me.grClassificatie.TabStop = False
        Me.grClassificatie.Text = "Verhogingsklassen"
        '
        'btnInfo
        '
        Me.btnInfo.BackColor = System.Drawing.Color.DodgerBlue
        Me.btnInfo.Location = New System.Drawing.Point(475, 107)
        Me.btnInfo.Margin = New System.Windows.Forms.Padding(4)
        Me.btnInfo.Name = "btnInfo"
        Me.btnInfo.Size = New System.Drawing.Size(33, 31)
        Me.btnInfo.TabIndex = 45
        Me.btnInfo.Text = "i"
        Me.btnInfo.UseVisualStyleBackColor = False
        '
        'btnRemoveElevationClass
        '
        Me.btnRemoveElevationClass.BackColor = System.Drawing.Color.IndianRed
        Me.btnRemoveElevationClass.Location = New System.Drawing.Point(475, 69)
        Me.btnRemoveElevationClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnRemoveElevationClass.Name = "btnRemoveElevationClass"
        Me.btnRemoveElevationClass.Size = New System.Drawing.Size(33, 31)
        Me.btnRemoveElevationClass.TabIndex = 44
        Me.btnRemoveElevationClass.Text = "-"
        Me.btnRemoveElevationClass.UseVisualStyleBackColor = False
        '
        'btnAddElevationClass
        '
        Me.btnAddElevationClass.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnAddElevationClass.Location = New System.Drawing.Point(475, 31)
        Me.btnAddElevationClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAddElevationClass.Name = "btnAddElevationClass"
        Me.btnAddElevationClass.Size = New System.Drawing.Size(33, 31)
        Me.btnAddElevationClass.TabIndex = 43
        Me.btnAddElevationClass.Text = "+"
        Me.btnAddElevationClass.UseVisualStyleBackColor = False
        '
        'grPercentileClasses
        '
        Me.grPercentileClasses.AllowUserToAddRows = False
        Me.grPercentileClasses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grPercentileClasses.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colNaam, Me.collBound, Me.coluBound})
        Me.grPercentileClasses.Location = New System.Drawing.Point(8, 31)
        Me.grPercentileClasses.Margin = New System.Windows.Forms.Padding(4)
        Me.grPercentileClasses.Name = "grPercentileClasses"
        Me.grPercentileClasses.Size = New System.Drawing.Size(460, 277)
        Me.grPercentileClasses.TabIndex = 36
        '
        'colNaam
        '
        Me.colNaam.HeaderText = "klassenaam"
        Me.colNaam.Name = "colNaam"
        '
        'collBound
        '
        Me.collBound.HeaderText = "van percentiel"
        Me.collBound.Name = "collBound"
        '
        'coluBound
        '
        Me.coluBound.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.coluBound.HeaderText = "tot percentiel"
        Me.coluBound.Name = "coluBound"
        '
        'cmbTidalComponent
        '
        Me.cmbTidalComponent.FormattingEnabled = True
        Me.cmbTidalComponent.Location = New System.Drawing.Point(228, 126)
        Me.cmbTidalComponent.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbTidalComponent.Name = "cmbTidalComponent"
        Me.cmbTidalComponent.Size = New System.Drawing.Size(139, 24)
        Me.cmbTidalComponent.TabIndex = 46
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(18, 129)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(74, 17)
        Me.Label6.TabIndex = 45
        Me.Label6.Text = "Verhogen:"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.btnQAMethod)
        Me.GroupBox2.Controls.Add(Me.radEenvoudig)
        Me.GroupBox2.Controls.Add(Me.radUitgebreid)
        Me.GroupBox2.Controls.Add(Me.cmbPercentage)
        Me.GroupBox2.Controls.Add(Me.cmbTidalComponent)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.cmbDuur)
        Me.GroupBox2.Controls.Add(Me.Label5)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.txtUitloop)
        Me.GroupBox2.Location = New System.Drawing.Point(559, 16)
        Me.GroupBox2.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox2.Size = New System.Drawing.Size(523, 217)
        Me.GroupBox2.TabIndex = 39
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Instellingen"
        '
        'btnQAMethod
        '
        Me.btnQAMethod.BackColor = System.Drawing.Color.Gold
        Me.btnQAMethod.Location = New System.Drawing.Point(475, 176)
        Me.btnQAMethod.Margin = New System.Windows.Forms.Padding(4)
        Me.btnQAMethod.Name = "btnQAMethod"
        Me.btnQAMethod.Size = New System.Drawing.Size(33, 31)
        Me.btnQAMethod.TabIndex = 46
        Me.btnQAMethod.Text = "?"
        Me.btnQAMethod.UseVisualStyleBackColor = False
        '
        'radEenvoudig
        '
        Me.radEenvoudig.AutoSize = True
        Me.radEenvoudig.Checked = True
        Me.radEenvoudig.Location = New System.Drawing.Point(21, 176)
        Me.radEenvoudig.Margin = New System.Windows.Forms.Padding(4)
        Me.radEenvoudig.Name = "radEenvoudig"
        Me.radEenvoudig.Size = New System.Drawing.Size(96, 21)
        Me.radEenvoudig.TabIndex = 48
        Me.radEenvoudig.TabStop = True
        Me.radEenvoudig.Text = "Eenvoudig"
        Me.radEenvoudig.UseVisualStyleBackColor = True
        '
        'radUitgebreid
        '
        Me.radUitgebreid.AutoSize = True
        Me.radUitgebreid.Location = New System.Drawing.Point(147, 176)
        Me.radUitgebreid.Margin = New System.Windows.Forms.Padding(4)
        Me.radUitgebreid.Name = "radUitgebreid"
        Me.radUitgebreid.Size = New System.Drawing.Size(94, 21)
        Me.radUitgebreid.TabIndex = 47
        Me.radUitgebreid.Text = "Uitgebreid"
        Me.radUitgebreid.UseVisualStyleBackColor = True
        '
        'grAmplitudeKlassen
        '
        Me.grAmplitudeKlassen.AllowUserToAddRows = False
        Me.grAmplitudeKlassen.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grAmplitudeKlassen.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colClassName, Me.colVanPerc, Me.colTotPerc})
        Me.grAmplitudeKlassen.Location = New System.Drawing.Point(8, 31)
        Me.grAmplitudeKlassen.Margin = New System.Windows.Forms.Padding(4)
        Me.grAmplitudeKlassen.Name = "grAmplitudeKlassen"
        Me.grAmplitudeKlassen.Size = New System.Drawing.Size(465, 277)
        Me.grAmplitudeKlassen.TabIndex = 45
        '
        'colClassName
        '
        Me.colClassName.HeaderText = "klassenaam"
        Me.colClassName.Name = "colClassName"
        '
        'colVanPerc
        '
        Me.colVanPerc.HeaderText = "van percentiel"
        Me.colVanPerc.Name = "colVanPerc"
        '
        'colTotPerc
        '
        Me.colTotPerc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colTotPerc.HeaderText = "tot percentiel"
        Me.colTotPerc.Name = "colTotPerc"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.btnRemoveAmplitudeClass)
        Me.GroupBox3.Controls.Add(Me.grAmplitudeKlassen)
        Me.GroupBox3.Controls.Add(Me.btnAddAmplitudeClass)
        Me.GroupBox3.Location = New System.Drawing.Point(20, 240)
        Me.GroupBox3.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox3.Size = New System.Drawing.Size(531, 329)
        Me.GroupBox3.TabIndex = 46
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Amplitudeklassen"
        '
        'btnRemoveAmplitudeClass
        '
        Me.btnRemoveAmplitudeClass.BackColor = System.Drawing.Color.IndianRed
        Me.btnRemoveAmplitudeClass.Location = New System.Drawing.Point(481, 69)
        Me.btnRemoveAmplitudeClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnRemoveAmplitudeClass.Name = "btnRemoveAmplitudeClass"
        Me.btnRemoveAmplitudeClass.Size = New System.Drawing.Size(33, 31)
        Me.btnRemoveAmplitudeClass.TabIndex = 46
        Me.btnRemoveAmplitudeClass.Text = "-"
        Me.btnRemoveAmplitudeClass.UseVisualStyleBackColor = False
        '
        'btnAddAmplitudeClass
        '
        Me.btnAddAmplitudeClass.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnAddAmplitudeClass.Location = New System.Drawing.Point(481, 31)
        Me.btnAddAmplitudeClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAddAmplitudeClass.Name = "btnAddAmplitudeClass"
        Me.btnAddAmplitudeClass.Size = New System.Drawing.Size(33, 31)
        Me.btnAddAmplitudeClass.TabIndex = 45
        Me.btnAddAmplitudeClass.Text = "+"
        Me.btnAddAmplitudeClass.UseVisualStyleBackColor = False
        '
        'frmClassifyTidesByStatistics
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1092, 631)
        Me.Controls.Add(Me.btnAnalyze)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.grClassificatie)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnExport)
        Me.Controls.Add(Me.GroupBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "frmClassifyTidesByStatistics"
        Me.Text = "Getijden statistisch classificeren"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.grClassificatie.ResumeLayout(False)
        CType(Me.grPercentileClasses, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        CType(Me.grAmplitudeKlassen, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox3.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnOpenCSV As System.Windows.Forms.Button
    Friend WithEvents txtCSVFile As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents dlgOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents btnExport As System.Windows.Forms.Button
    Friend WithEvents dlgSaveFile As System.Windows.Forms.SaveFileDialog
    Friend WithEvents prProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As System.Windows.Forms.Label
    Friend WithEvents btnImport As System.Windows.Forms.Button
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents txtSeriesName As System.Windows.Forms.TextBox
    Friend WithEvents cmbDuur As System.Windows.Forms.ComboBox
    Friend WithEvents btnAnalyze As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtUitloop As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents cmbPercentage As System.Windows.Forms.ComboBox
    Friend WithEvents grClassificatie As System.Windows.Forms.GroupBox
    Friend WithEvents grPercentileClasses As System.Windows.Forms.DataGridView
    Friend WithEvents btnRemoveElevationClass As System.Windows.Forms.Button
    Friend WithEvents btnAddElevationClass As System.Windows.Forms.Button
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents cmbTidalComponent As System.Windows.Forms.ComboBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents grAmplitudeKlassen As System.Windows.Forms.DataGridView
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents btnRemoveAmplitudeClass As System.Windows.Forms.Button
    Friend WithEvents btnAddAmplitudeClass As System.Windows.Forms.Button
    Friend WithEvents colNaam As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents collBound As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents coluBound As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colClassName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colVanPerc As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colTotPerc As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btnInfo As System.Windows.Forms.Button
    Friend WithEvents btnQAMethod As System.Windows.Forms.Button
    Friend WithEvents radEenvoudig As System.Windows.Forms.RadioButton
    Friend WithEvents radUitgebreid As System.Windows.Forms.RadioButton
    Friend WithEvents cmbDataField As Windows.Forms.ComboBox
    Friend WithEvents Label10 As Windows.Forms.Label
    Friend WithEvents cmbDateField As Windows.Forms.ComboBox
    Friend WithEvents Label9 As Windows.Forms.Label
    Friend WithEvents btnDateNotation As Windows.Forms.Button
    Friend WithEvents txtTimeNotation As Windows.Forms.TextBox
    Friend WithEvents txtDateNotation As Windows.Forms.TextBox
    Friend WithEvents cmbTimeField As Windows.Forms.ComboBox
    Friend WithEvents Label11 As Windows.Forms.Label
    Friend WithEvents btnTimeNotation As Windows.Forms.Button
    Friend WithEvents txtMultiplier As Windows.Forms.TextBox
    Friend WithEvents Label2 As Windows.Forms.Label
End Class
