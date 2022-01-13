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
        Me.colNaam = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPercentiel = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTotPerc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnDeleteGroundwaterClass = New System.Windows.Forms.Button()
        Me.btnAddGroundwaterClass = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.btnClassify = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbDuration = New System.Windows.Forms.ComboBox()
        Me.dlgSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.Instellingen = New System.Windows.Forms.GroupBox()
        Me.radZomWin = New System.Windows.Forms.RadioButton()
        Me.radJaarRond = New System.Windows.Forms.RadioButton()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.groupSbk = New System.Windows.Forms.GroupBox()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Instellingen.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.groupSbk.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtSobekProject
        '
        Me.txtSobekProject.Location = New System.Drawing.Point(120, 31)
        Me.txtSobekProject.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtSobekProject.Name = "txtSobekProject"
        Me.txtSobekProject.Size = New System.Drawing.Size(765, 22)
        Me.txtSobekProject.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(24, 34)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(56, 17)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Project:"
        '
        'btnSbkProject
        '
        Me.btnSbkProject.Location = New System.Drawing.Point(895, 28)
        Me.btnSbkProject.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnSbkProject.Name = "btnSbkProject"
        Me.btnSbkProject.Size = New System.Drawing.Size(29, 27)
        Me.btnSbkProject.TabIndex = 2
        Me.btnSbkProject.Text = ".."
        Me.btnSbkProject.UseVisualStyleBackColor = True
        '
        'cmbSobekCases
        '
        Me.cmbSobekCases.FormattingEnabled = True
        Me.cmbSobekCases.Location = New System.Drawing.Point(120, 63)
        Me.cmbSobekCases.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmbSobekCases.Name = "cmbSobekCases"
        Me.cmbSobekCases.Size = New System.Drawing.Size(803, 24)
        Me.cmbSobekCases.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(24, 66)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(44, 17)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Case:"
        '
        'grGrondwaterKlassen
        '
        Me.grGrondwaterKlassen.AllowUserToAddRows = False
        Me.grGrondwaterKlassen.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grGrondwaterKlassen.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colNaam, Me.colPercentiel, Me.colTotPerc})
        Me.grGrondwaterKlassen.Location = New System.Drawing.Point(24, 31)
        Me.grGrondwaterKlassen.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.grGrondwaterKlassen.Name = "grGrondwaterKlassen"
        Me.grGrondwaterKlassen.RowHeadersWidth = 51
        Me.grGrondwaterKlassen.Size = New System.Drawing.Size(465, 162)
        Me.grGrondwaterKlassen.TabIndex = 5
        '
        'colNaam
        '
        Me.colNaam.HeaderText = "klassenaam"
        Me.colNaam.MinimumWidth = 6
        Me.colNaam.Name = "colNaam"
        Me.colNaam.Width = 125
        '
        'colPercentiel
        '
        Me.colPercentiel.HeaderText = "van percentiel"
        Me.colPercentiel.MinimumWidth = 6
        Me.colPercentiel.Name = "colPercentiel"
        Me.colPercentiel.Width = 125
        '
        'colTotPerc
        '
        Me.colTotPerc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colTotPerc.HeaderText = "tot percentiel"
        Me.colTotPerc.MinimumWidth = 6
        Me.colTotPerc.Name = "colTotPerc"
        '
        'btnDeleteGroundwaterClass
        '
        Me.btnDeleteGroundwaterClass.BackColor = System.Drawing.Color.IndianRed
        Me.btnDeleteGroundwaterClass.Location = New System.Drawing.Point(497, 66)
        Me.btnDeleteGroundwaterClass.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
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
        Me.btnAddGroundwaterClass.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnAddGroundwaterClass.Name = "btnAddGroundwaterClass"
        Me.btnAddGroundwaterClass.Size = New System.Drawing.Size(33, 31)
        Me.btnAddGroundwaterClass.TabIndex = 12
        Me.btnAddGroundwaterClass.Text = "+"
        Me.btnAddGroundwaterClass.UseVisualStyleBackColor = False
        '
        'btnClassify
        '
        Me.btnClassify.BackColor = System.Drawing.Color.Yellow
        Me.btnClassify.Location = New System.Drawing.Point(853, 398)
        Me.btnClassify.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
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
        Me.Label4.Size = New System.Drawing.Size(188, 17)
        Me.Label4.TabIndex = 18
        Me.Label4.Text = "Duur gebeurtenissen (uren):"
        '
        'cmbDuration
        '
        Me.cmbDuration.FormattingEnabled = True
        Me.cmbDuration.Location = New System.Drawing.Point(235, 36)
        Me.cmbDuration.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmbDuration.Name = "cmbDuration"
        Me.cmbDuration.Size = New System.Drawing.Size(125, 24)
        Me.cmbDuration.TabIndex = 19
        '
        'Instellingen
        '
        Me.Instellingen.Controls.Add(Me.radZomWin)
        Me.Instellingen.Controls.Add(Me.radJaarRond)
        Me.Instellingen.Controls.Add(Me.cmbDuration)
        Me.Instellingen.Controls.Add(Me.Label4)
        Me.Instellingen.Location = New System.Drawing.Point(583, 145)
        Me.Instellingen.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Instellingen.Name = "Instellingen"
        Me.Instellingen.Padding = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Instellingen.Size = New System.Drawing.Size(384, 228)
        Me.Instellingen.TabIndex = 20
        Me.Instellingen.TabStop = False
        Me.Instellingen.Text = "Classificeren voor:"
        '
        'radZomWin
        '
        Me.radZomWin.AutoSize = True
        Me.radZomWin.Checked = True
        Me.radZomWin.Location = New System.Drawing.Point(29, 89)
        Me.radZomWin.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.radZomWin.Name = "radZomWin"
        Me.radZomWin.Size = New System.Drawing.Size(183, 21)
        Me.radZomWin.TabIndex = 21
        Me.radZomWin.TabStop = True
        Me.radZomWin.Text = "Zomer- en winterhalfjaar"
        Me.radZomWin.UseVisualStyleBackColor = True
        '
        'radJaarRond
        '
        Me.radJaarRond.AutoSize = True
        Me.radJaarRond.Location = New System.Drawing.Point(29, 117)
        Me.radJaarRond.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.radJaarRond.Name = "radJaarRond"
        Me.radJaarRond.Size = New System.Drawing.Size(86, 21)
        Me.radJaarRond.TabIndex = 20
        Me.radJaarRond.Text = "Jaarrond"
        Me.radJaarRond.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.grGrondwaterKlassen)
        Me.GroupBox1.Controls.Add(Me.btnAddGroundwaterClass)
        Me.GroupBox1.Controls.Add(Me.btnDeleteGroundwaterClass)
        Me.GroupBox1.Location = New System.Drawing.Point(24, 145)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.GroupBox1.Size = New System.Drawing.Size(551, 228)
        Me.GroupBox1.TabIndex = 21
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Grondwaterklassen"
        '
        'groupSbk
        '
        Me.groupSbk.Controls.Add(Me.Label1)
        Me.groupSbk.Controls.Add(Me.txtSobekProject)
        Me.groupSbk.Controls.Add(Me.btnSbkProject)
        Me.groupSbk.Controls.Add(Me.cmbSobekCases)
        Me.groupSbk.Controls.Add(Me.Label2)
        Me.groupSbk.Location = New System.Drawing.Point(20, 15)
        Me.groupSbk.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.groupSbk.Name = "groupSbk"
        Me.groupSbk.Padding = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.groupSbk.Size = New System.Drawing.Size(947, 123)
        Me.groupSbk.TabIndex = 22
        Me.groupSbk.TabStop = False
        Me.groupSbk.Text = "Sobek"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(24, 417)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(821, 28)
        Me.prProgress.TabIndex = 23
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(20, 398)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(74, 17)
        Me.lblProgress.TabIndex = 24
        Me.lblProgress.Text = "Voortgang"
        '
        'frmClassifyGroundWater
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(983, 460)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.groupSbk)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Instellingen)
        Me.Controls.Add(Me.btnClassify)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Name = "frmClassifyGroundWater"
        Me.Text = "Initiële grondwaterstanden classificeren"
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Instellingen.ResumeLayout(False)
        Me.Instellingen.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.groupSbk.ResumeLayout(False)
        Me.groupSbk.PerformLayout()
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
  Friend WithEvents colNaam As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colPercentiel As System.Windows.Forms.DataGridViewTextBoxColumn
  Friend WithEvents colTotPerc As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents radZomWin As Windows.Forms.RadioButton
    Friend WithEvents radJaarRond As Windows.Forms.RadioButton
End Class
