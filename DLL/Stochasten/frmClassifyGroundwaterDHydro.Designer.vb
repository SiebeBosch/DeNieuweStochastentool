<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmClassifyGroundwaterDHydro
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmClassifyGroundwaterDHydro))
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.groupSbk = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtRRDir = New System.Windows.Forms.TextBox()
        Me.btnRRDir = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.grGrondwaterKlassen = New System.Windows.Forms.DataGridView()
        Me.colNaam = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPercentiel = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTotPerc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnAddGroundwaterClass = New System.Windows.Forms.Button()
        Me.btnDeleteGroundwaterClass = New System.Windows.Forms.Button()
        Me.radZomWin = New System.Windows.Forms.RadioButton()
        Me.radJaarRond = New System.Windows.Forms.RadioButton()
        Me.Instellingen = New System.Windows.Forms.GroupBox()
        Me.cmbDuration = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.dlgSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.btnClassify = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.groupSbk.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Instellingen.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(18, 398)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(74, 17)
        Me.lblProgress.TabIndex = 30
        Me.lblProgress.Text = "Voortgang"
        '
        'groupSbk
        '
        Me.groupSbk.Controls.Add(Me.Label1)
        Me.groupSbk.Controls.Add(Me.txtRRDir)
        Me.groupSbk.Controls.Add(Me.btnRRDir)
        Me.groupSbk.Location = New System.Drawing.Point(18, 15)
        Me.groupSbk.Margin = New System.Windows.Forms.Padding(4)
        Me.groupSbk.Name = "groupSbk"
        Me.groupSbk.Padding = New System.Windows.Forms.Padding(4)
        Me.groupSbk.Size = New System.Drawing.Size(947, 123)
        Me.groupSbk.TabIndex = 28
        Me.groupSbk.TabStop = False
        Me.groupSbk.Text = "Sobek"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(24, 34)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(92, 17)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "RR-directory:"
        '
        'txtRRDir
        '
        Me.txtRRDir.Location = New System.Drawing.Point(120, 31)
        Me.txtRRDir.Margin = New System.Windows.Forms.Padding(4)
        Me.txtRRDir.Name = "txtRRDir"
        Me.txtRRDir.Size = New System.Drawing.Size(765, 22)
        Me.txtRRDir.TabIndex = 0
        '
        'btnRRDir
        '
        Me.btnRRDir.Location = New System.Drawing.Point(895, 28)
        Me.btnRRDir.Margin = New System.Windows.Forms.Padding(4)
        Me.btnRRDir.Name = "btnRRDir"
        Me.btnRRDir.Size = New System.Drawing.Size(29, 27)
        Me.btnRRDir.TabIndex = 2
        Me.btnRRDir.Text = ".."
        Me.btnRRDir.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.grGrondwaterKlassen)
        Me.GroupBox1.Controls.Add(Me.btnAddGroundwaterClass)
        Me.GroupBox1.Controls.Add(Me.btnDeleteGroundwaterClass)
        Me.GroupBox1.Location = New System.Drawing.Point(22, 145)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(551, 228)
        Me.GroupBox1.TabIndex = 27
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Grondwaterklassen"
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
        'radZomWin
        '
        Me.radZomWin.AutoSize = True
        Me.radZomWin.Checked = True
        Me.radZomWin.Location = New System.Drawing.Point(29, 89)
        Me.radZomWin.Margin = New System.Windows.Forms.Padding(4)
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
        Me.radJaarRond.Margin = New System.Windows.Forms.Padding(4)
        Me.radJaarRond.Name = "radJaarRond"
        Me.radJaarRond.Size = New System.Drawing.Size(86, 21)
        Me.radJaarRond.TabIndex = 20
        Me.radJaarRond.Text = "Jaarrond"
        Me.radJaarRond.UseVisualStyleBackColor = True
        '
        'Instellingen
        '
        Me.Instellingen.Controls.Add(Me.radZomWin)
        Me.Instellingen.Controls.Add(Me.radJaarRond)
        Me.Instellingen.Controls.Add(Me.cmbDuration)
        Me.Instellingen.Controls.Add(Me.Label4)
        Me.Instellingen.Location = New System.Drawing.Point(581, 145)
        Me.Instellingen.Margin = New System.Windows.Forms.Padding(4)
        Me.Instellingen.Name = "Instellingen"
        Me.Instellingen.Padding = New System.Windows.Forms.Padding(4)
        Me.Instellingen.Size = New System.Drawing.Size(384, 228)
        Me.Instellingen.TabIndex = 26
        Me.Instellingen.TabStop = False
        Me.Instellingen.Text = "Classificeren voor:"
        '
        'cmbDuration
        '
        Me.cmbDuration.FormattingEnabled = True
        Me.cmbDuration.Location = New System.Drawing.Point(235, 36)
        Me.cmbDuration.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbDuration.Name = "cmbDuration"
        Me.cmbDuration.Size = New System.Drawing.Size(125, 24)
        Me.cmbDuration.TabIndex = 19
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
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(22, 417)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(4)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(821, 28)
        Me.prProgress.TabIndex = 29
        '
        'btnClassify
        '
        Me.btnClassify.BackColor = System.Drawing.Color.Yellow
        Me.btnClassify.Location = New System.Drawing.Point(851, 398)
        Me.btnClassify.Margin = New System.Windows.Forms.Padding(4)
        Me.btnClassify.Name = "btnClassify"
        Me.btnClassify.Size = New System.Drawing.Size(113, 48)
        Me.btnClassify.TabIndex = 25
        Me.btnClassify.Text = "Classificeer!"
        Me.btnClassify.UseVisualStyleBackColor = False
        '
        'frmClassifyGroundwaterDHydro
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(983, 460)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.groupSbk)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Instellingen)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnClassify)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmClassifyGroundwaterDHydro"
        Me.Text = "Grondwaterstanden uit D-Hydro classificeren"
        Me.groupSbk.ResumeLayout(False)
        Me.groupSbk.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Instellingen.ResumeLayout(False)
        Me.Instellingen.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents groupSbk As Windows.Forms.GroupBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents txtRRDir As Windows.Forms.TextBox
    Friend WithEvents btnRRDir As Windows.Forms.Button
    Friend WithEvents GroupBox1 As Windows.Forms.GroupBox
    Public WithEvents grGrondwaterKlassen As Windows.Forms.DataGridView
    Friend WithEvents colNaam As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPercentiel As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colTotPerc As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btnAddGroundwaterClass As Windows.Forms.Button
    Friend WithEvents btnDeleteGroundwaterClass As Windows.Forms.Button
    Friend WithEvents radZomWin As Windows.Forms.RadioButton
    Friend WithEvents radJaarRond As Windows.Forms.RadioButton
    Friend WithEvents Instellingen As Windows.Forms.GroupBox
    Friend WithEvents cmbDuration As Windows.Forms.ComboBox
    Friend WithEvents Label4 As Windows.Forms.Label
    Friend WithEvents dlgSaveFile As Windows.Forms.SaveFileDialog
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents btnClassify As Windows.Forms.Button
    Friend WithEvents dlgFolder As Windows.Forms.FolderBrowserDialog
End Class
