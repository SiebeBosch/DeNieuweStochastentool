<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmClassifyGroundWater
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmClassifyGroundWater))
        Me.txtSobekProject = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnSbkProject = New System.Windows.Forms.Button()
        Me.cmbSobekCases = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.grGrondwaterKlassen = New System.Windows.Forms.DataGridView()
        Me.btnDeleteGroundwaterClass = New System.Windows.Forms.Button()
        Me.btnAddGroundwaterClass = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.btnClassify = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbDuration = New System.Windows.Forms.ComboBox()
        Me.dlgSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.Instellingen = New System.Windows.Forms.GroupBox()
        Me.btnGroeiseizoenHelp = New System.Windows.Forms.Button()
        Me.radGroeiseizoen = New System.Windows.Forms.RadioButton()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtMinTimestepsBetweenEvents = New System.Windows.Forms.TextBox()
        Me.radAprilAugust = New System.Windows.Forms.RadioButton()
        Me.radZomWin = New System.Windows.Forms.RadioButton()
        Me.radJaarRond = New System.Windows.Forms.RadioButton()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.btnHelpSobekPercentiles = New System.Windows.Forms.Button()
        Me.groupSbk = New System.Windows.Forms.GroupBox()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.chkSeepage = New System.Windows.Forms.CheckBox()
        Me.colNaam = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPercentiel = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTotPerc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Instellingen.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.groupSbk.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtSobekProject
        '
        Me.txtSobekProject.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSobekProject.Location = New System.Drawing.Point(120, 31)
        Me.txtSobekProject.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSobekProject.Name = "txtSobekProject"
        Me.txtSobekProject.Size = New System.Drawing.Size(375, 22)
        Me.txtSobekProject.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(24, 34)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(52, 16)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Project:"
        '
        'btnSbkProject
        '
        Me.btnSbkProject.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSbkProject.Location = New System.Drawing.Point(503, 29)
        Me.btnSbkProject.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSbkProject.Name = "btnSbkProject"
        Me.btnSbkProject.Size = New System.Drawing.Size(29, 27)
        Me.btnSbkProject.TabIndex = 2
        Me.btnSbkProject.Text = ".."
        Me.btnSbkProject.UseVisualStyleBackColor = True
        '
        'cmbSobekCases
        '
        Me.cmbSobekCases.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmbSobekCases.FormattingEnabled = True
        Me.cmbSobekCases.Location = New System.Drawing.Point(120, 63)
        Me.cmbSobekCases.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbSobekCases.Name = "cmbSobekCases"
        Me.cmbSobekCases.Size = New System.Drawing.Size(412, 24)
        Me.cmbSobekCases.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(24, 66)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(42, 16)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Case:"
        '
        'grGrondwaterKlassen
        '
        Me.grGrondwaterKlassen.AllowUserToAddRows = False
        Me.grGrondwaterKlassen.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grGrondwaterKlassen.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colNaam, Me.colPercentiel, Me.colTotPerc})
        Me.grGrondwaterKlassen.Location = New System.Drawing.Point(24, 31)
        Me.grGrondwaterKlassen.Margin = New System.Windows.Forms.Padding(4)
        Me.grGrondwaterKlassen.Name = "grGrondwaterKlassen"
        Me.grGrondwaterKlassen.RowHeadersWidth = 51
        Me.grGrondwaterKlassen.Size = New System.Drawing.Size(465, 162)
        Me.grGrondwaterKlassen.TabIndex = 5
        '
        'btnDeleteGroundwaterClass
        '
        Me.btnDeleteGroundwaterClass.BackColor = System.Drawing.Color.IndianRed
        Me.btnDeleteGroundwaterClass.Location = New System.Drawing.Point(497, 66)
        Me.btnDeleteGroundwaterClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnDeleteGroundwaterClass.Name = "btnDeleteGroundwaterClass"
        Me.btnDeleteGroundwaterClass.Size = New System.Drawing.Size(33, 31)
        Me.btnDeleteGroundwaterClass.TabIndex = 13
        Me.btnDeleteGroundwaterClass.Text = "-"
        Me.btnDeleteGroundwaterClass.UseVisualStyleBackColor = False
        '
        'btnAddGroundwaterClass
        '
        Me.btnAddGroundwaterClass.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnAddGroundwaterClass.Location = New System.Drawing.Point(497, 31)
        Me.btnAddGroundwaterClass.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAddGroundwaterClass.Name = "btnAddGroundwaterClass"
        Me.btnAddGroundwaterClass.Size = New System.Drawing.Size(33, 31)
        Me.btnAddGroundwaterClass.TabIndex = 12
        Me.btnAddGroundwaterClass.Text = "+"
        Me.btnAddGroundwaterClass.UseVisualStyleBackColor = False
        '
        'btnClassify
        '
        Me.btnClassify.BackColor = System.Drawing.Color.Yellow
        Me.btnClassify.Location = New System.Drawing.Point(980, 399)
        Me.btnClassify.Margin = New System.Windows.Forms.Padding(4)
        Me.btnClassify.Name = "btnClassify"
        Me.btnClassify.Size = New System.Drawing.Size(113, 48)
        Me.btnClassify.TabIndex = 14
        Me.btnClassify.Text = "Classificeer!"
        Me.btnClassify.UseVisualStyleBackColor = False
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(25, 39)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(171, 16)
        Me.Label4.TabIndex = 18
        Me.Label4.Text = "Duur gebeurtenissen (uren):"
        '
        'cmbDuration
        '
        Me.cmbDuration.FormattingEnabled = True
        Me.cmbDuration.Location = New System.Drawing.Point(362, 35)
        Me.cmbDuration.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbDuration.Name = "cmbDuration"
        Me.cmbDuration.Size = New System.Drawing.Size(125, 24)
        Me.cmbDuration.TabIndex = 19
        '
        'Instellingen
        '
        Me.Instellingen.Controls.Add(Me.btnGroeiseizoenHelp)
        Me.Instellingen.Controls.Add(Me.radGroeiseizoen)
        Me.Instellingen.Controls.Add(Me.Label3)
        Me.Instellingen.Controls.Add(Me.txtMinTimestepsBetweenEvents)
        Me.Instellingen.Controls.Add(Me.radAprilAugust)
        Me.Instellingen.Controls.Add(Me.radZomWin)
        Me.Instellingen.Controls.Add(Me.radJaarRond)
        Me.Instellingen.Controls.Add(Me.cmbDuration)
        Me.Instellingen.Controls.Add(Me.Label4)
        Me.Instellingen.Location = New System.Drawing.Point(583, 145)
        Me.Instellingen.Margin = New System.Windows.Forms.Padding(4)
        Me.Instellingen.Name = "Instellingen"
        Me.Instellingen.Padding = New System.Windows.Forms.Padding(4)
        Me.Instellingen.Size = New System.Drawing.Size(510, 228)
        Me.Instellingen.TabIndex = 20
        Me.Instellingen.TabStop = False
        Me.Instellingen.Text = "Classificeren voor:"
        '
        'btnGroeiseizoenHelp
        '
        Me.btnGroeiseizoenHelp.BackColor = System.Drawing.Color.Yellow
        Me.btnGroeiseizoenHelp.Location = New System.Drawing.Point(464, 104)
        Me.btnGroeiseizoenHelp.Name = "btnGroeiseizoenHelp"
        Me.btnGroeiseizoenHelp.Size = New System.Drawing.Size(23, 23)
        Me.btnGroeiseizoenHelp.TabIndex = 26
        Me.btnGroeiseizoenHelp.Text = "?"
        Me.btnGroeiseizoenHelp.UseVisualStyleBackColor = False
        '
        'radGroeiseizoen
        '
        Me.radGroeiseizoen.AutoSize = True
        Me.radGroeiseizoen.Checked = True
        Me.radGroeiseizoen.Location = New System.Drawing.Point(28, 107)
        Me.radGroeiseizoen.Margin = New System.Windows.Forms.Padding(4)
        Me.radGroeiseizoen.Name = "radGroeiseizoen"
        Me.radGroeiseizoen.Size = New System.Drawing.Size(358, 20)
        Me.radGroeiseizoen.TabIndex = 25
        Me.radGroeiseizoen.TabStop = True
        Me.radGroeiseizoen.Text = "Groeiseizoen (maart t/m oktober) en buiten groeiseizoen"
        Me.radGroeiseizoen.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(25, 73)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(285, 16)
        Me.Label3.TabIndex = 24
        Me.Label3.Text = "Minimum tijd tussen events (aantal tijdstappen):"
        '
        'txtMinTimestepsBetweenEvents
        '
        Me.txtMinTimestepsBetweenEvents.Location = New System.Drawing.Point(362, 69)
        Me.txtMinTimestepsBetweenEvents.Name = "txtMinTimestepsBetweenEvents"
        Me.txtMinTimestepsBetweenEvents.Size = New System.Drawing.Size(125, 22)
        Me.txtMinTimestepsBetweenEvents.TabIndex = 23
        Me.txtMinTimestepsBetweenEvents.Text = "0"
        '
        'radAprilAugust
        '
        Me.radAprilAugust.AutoSize = True
        Me.radAprilAugust.Location = New System.Drawing.Point(28, 161)
        Me.radAprilAugust.Margin = New System.Windows.Forms.Padding(4)
        Me.radAprilAugust.Name = "radAprilAugust"
        Me.radAprilAugust.Size = New System.Drawing.Size(204, 20)
        Me.radAprilAugust.TabIndex = 22
        Me.radAprilAugust.Text = "April t/m aug en sept t/m maart"
        Me.radAprilAugust.UseVisualStyleBackColor = True
        '
        'radZomWin
        '
        Me.radZomWin.AutoSize = True
        Me.radZomWin.Location = New System.Drawing.Point(28, 134)
        Me.radZomWin.Margin = New System.Windows.Forms.Padding(4)
        Me.radZomWin.Name = "radZomWin"
        Me.radZomWin.Size = New System.Drawing.Size(167, 20)
        Me.radZomWin.TabIndex = 21
        Me.radZomWin.Text = "Zomer- en winterhalfjaar"
        Me.radZomWin.UseVisualStyleBackColor = True
        '
        'radJaarRond
        '
        Me.radJaarRond.AutoSize = True
        Me.radJaarRond.Location = New System.Drawing.Point(28, 188)
        Me.radJaarRond.Margin = New System.Windows.Forms.Padding(4)
        Me.radJaarRond.Name = "radJaarRond"
        Me.radJaarRond.Size = New System.Drawing.Size(79, 20)
        Me.radJaarRond.TabIndex = 20
        Me.radJaarRond.Text = "Jaarrond"
        Me.radJaarRond.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.btnHelpSobekPercentiles)
        Me.GroupBox1.Controls.Add(Me.grGrondwaterKlassen)
        Me.GroupBox1.Controls.Add(Me.btnAddGroundwaterClass)
        Me.GroupBox1.Controls.Add(Me.btnDeleteGroundwaterClass)
        Me.GroupBox1.Location = New System.Drawing.Point(24, 145)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(551, 228)
        Me.GroupBox1.TabIndex = 21
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Grondwaterklassen"
        '
        'btnHelpSobekPercentiles
        '
        Me.btnHelpSobekPercentiles.BackColor = System.Drawing.Color.Yellow
        Me.btnHelpSobekPercentiles.Location = New System.Drawing.Point(497, 102)
        Me.btnHelpSobekPercentiles.Name = "btnHelpSobekPercentiles"
        Me.btnHelpSobekPercentiles.Size = New System.Drawing.Size(33, 31)
        Me.btnHelpSobekPercentiles.TabIndex = 27
        Me.btnHelpSobekPercentiles.Text = "?"
        Me.btnHelpSobekPercentiles.UseVisualStyleBackColor = False
        '
        'groupSbk
        '
        Me.groupSbk.Controls.Add(Me.Label1)
        Me.groupSbk.Controls.Add(Me.txtSobekProject)
        Me.groupSbk.Controls.Add(Me.btnSbkProject)
        Me.groupSbk.Controls.Add(Me.cmbSobekCases)
        Me.groupSbk.Controls.Add(Me.Label2)
        Me.groupSbk.Location = New System.Drawing.Point(20, 15)
        Me.groupSbk.Margin = New System.Windows.Forms.Padding(4)
        Me.groupSbk.Name = "groupSbk"
        Me.groupSbk.Padding = New System.Windows.Forms.Padding(4)
        Me.groupSbk.Size = New System.Drawing.Size(555, 123)
        Me.groupSbk.TabIndex = 22
        Me.groupSbk.TabStop = False
        Me.groupSbk.Text = "Sobek"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(24, 417)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(4)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(948, 28)
        Me.prProgress.TabIndex = 23
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(20, 398)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(70, 16)
        Me.lblProgress.TabIndex = 24
        Me.lblProgress.Text = "Voortgang"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.chkSeepage)
        Me.GroupBox2.Location = New System.Drawing.Point(583, 15)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(510, 123)
        Me.GroupBox2.TabIndex = 25
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Opties"
        '
        'chkSeepage
        '
        Me.chkSeepage.AutoSize = True
        Me.chkSeepage.Location = New System.Drawing.Point(28, 31)
        Me.chkSeepage.Name = "chkSeepage"
        Me.chkSeepage.Size = New System.Drawing.Size(189, 20)
        Me.chkSeepage.TabIndex = 0
        Me.chkSeepage.Text = "Classificeer kwel eveneens"
        Me.chkSeepage.UseVisualStyleBackColor = True
        '
        'colNaam
        '
        Me.colNaam.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colNaam.HeaderText = "klassenaam"
        Me.colNaam.MinimumWidth = 6
        Me.colNaam.Name = "colNaam"
        '
        'colPercentiel
        '
        Me.colPercentiel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colPercentiel.HeaderText = "van percentiel"
        Me.colPercentiel.MinimumWidth = 6
        Me.colPercentiel.Name = "colPercentiel"
        '
        'colTotPerc
        '
        Me.colTotPerc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colTotPerc.HeaderText = "tot percentiel"
        Me.colTotPerc.MinimumWidth = 6
        Me.colTotPerc.Name = "colTotPerc"
        '
        'frmClassifyGroundWater
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1106, 460)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.groupSbk)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Instellingen)
        Me.Controls.Add(Me.btnClassify)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "frmClassifyGroundWater"
        Me.Text = "Initiële grondwaterstanden uit D-Hydro classificeren"
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Instellingen.ResumeLayout(False)
        Me.Instellingen.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.groupSbk.ResumeLayout(False)
        Me.groupSbk.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtSobekProject As System.Windows.Forms.TextBox
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents btnSbkProject As System.Windows.Forms.Button
  Friend WithEvents cmbSobekCases As System.Windows.Forms.ComboBox
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents btnDeleteGroundwaterClass As System.Windows.Forms.Button
  Friend WithEvents btnAddGroundwaterClass As System.Windows.Forms.Button
  Friend WithEvents dlgFolder As System.Windows.Forms.FolderBrowserDialog
  Friend WithEvents btnClassify As System.Windows.Forms.Button
  Friend WithEvents Label4 As System.Windows.Forms.Label
  Friend WithEvents cmbDuration As System.Windows.Forms.ComboBox
  Friend WithEvents dlgSaveFile As System.Windows.Forms.SaveFileDialog
  Public WithEvents grGrondwaterKlassen As System.Windows.Forms.DataGridView
  Friend WithEvents Instellingen As System.Windows.Forms.GroupBox
  Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
  Friend WithEvents groupSbk As System.Windows.Forms.GroupBox
  Friend WithEvents prProgress As System.Windows.Forms.ProgressBar
  Friend WithEvents lblProgress As System.Windows.Forms.Label
    Friend WithEvents radZomWin As Windows.Forms.RadioButton
    Friend WithEvents radJaarRond As Windows.Forms.RadioButton
    Friend WithEvents radAprilAugust As Windows.Forms.RadioButton
    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents txtMinTimestepsBetweenEvents As Windows.Forms.TextBox
    Friend WithEvents radGroeiseizoen As Windows.Forms.RadioButton
    Friend WithEvents btnGroeiseizoenHelp As Windows.Forms.Button
    Friend WithEvents GroupBox2 As Windows.Forms.GroupBox
    Friend WithEvents chkSeepage As Windows.Forms.CheckBox
    Friend WithEvents btnHelpSobekPercentiles As Windows.Forms.Button
    Friend WithEvents colNaam As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPercentiel As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colTotPerc As Windows.Forms.DataGridViewTextBoxColumn
End Class
