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
        Me.btnGroeiseizoenHelp = New System.Windows.Forms.Button()
        Me.radAprilAugust = New System.Windows.Forms.RadioButton()
        Me.radGroeiseizoen = New System.Windows.Forms.RadioButton()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtMinTimestepsBetweenEvents = New System.Windows.Forms.TextBox()
        Me.cmbDuration = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.dlgSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.btnClassify = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.grOptions = New System.Windows.Forms.GroupBox()
        Me.chkSeepage = New System.Windows.Forms.CheckBox()
        Me.groupSbk.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Instellingen.SuspendLayout()
        Me.grOptions.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(19, 398)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(70, 16)
        Me.lblProgress.TabIndex = 30
        Me.lblProgress.Text = "Voortgang"
        '
        'groupSbk
        '
        Me.groupSbk.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.groupSbk.Controls.Add(Me.Label1)
        Me.groupSbk.Controls.Add(Me.txtRRDir)
        Me.groupSbk.Controls.Add(Me.btnRRDir)
        Me.groupSbk.Location = New System.Drawing.Point(19, 15)
        Me.groupSbk.Margin = New System.Windows.Forms.Padding(4)
        Me.groupSbk.Name = "groupSbk"
        Me.groupSbk.Padding = New System.Windows.Forms.Padding(4)
        Me.groupSbk.Size = New System.Drawing.Size(555, 123)
        Me.groupSbk.TabIndex = 28
        Me.groupSbk.TabStop = False
        Me.groupSbk.Text = "D-Hydro"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(24, 34)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(86, 16)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "RR-directory:"
        '
        'txtRRDir
        '
        Me.txtRRDir.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtRRDir.Location = New System.Drawing.Point(120, 31)
        Me.txtRRDir.Margin = New System.Windows.Forms.Padding(4)
        Me.txtRRDir.Name = "txtRRDir"
        Me.txtRRDir.Size = New System.Drawing.Size(391, 22)
        Me.txtRRDir.TabIndex = 0
        '
        'btnRRDir
        '
        Me.btnRRDir.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnRRDir.Location = New System.Drawing.Point(517, 30)
        Me.btnRRDir.Margin = New System.Windows.Forms.Padding(4)
        Me.btnRRDir.Name = "btnRRDir"
        Me.btnRRDir.Size = New System.Drawing.Size(29, 27)
        Me.btnRRDir.TabIndex = 2
        Me.btnRRDir.Text = ".."
        Me.btnRRDir.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Controls.Add(Me.grGrondwaterKlassen)
        Me.GroupBox1.Controls.Add(Me.btnAddGroundwaterClass)
        Me.GroupBox1.Controls.Add(Me.btnDeleteGroundwaterClass)
        Me.GroupBox1.Location = New System.Drawing.Point(21, 145)
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
        Me.grGrondwaterKlassen.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.grGrondwaterKlassen.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grGrondwaterKlassen.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colNaam, Me.colPercentiel, Me.colTotPerc})
        Me.grGrondwaterKlassen.Location = New System.Drawing.Point(24, 31)
        Me.grGrondwaterKlassen.Margin = New System.Windows.Forms.Padding(4)
        Me.grGrondwaterKlassen.Name = "grGrondwaterKlassen"
        Me.grGrondwaterKlassen.RowHeadersWidth = 51
        Me.grGrondwaterKlassen.Size = New System.Drawing.Size(465, 177)
        Me.grGrondwaterKlassen.TabIndex = 5
        '
        'colNaam
        '
        Me.colNaam.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colNaam.FillWeight = 15.58441!
        Me.colNaam.HeaderText = "klassenaam"
        Me.colNaam.MinimumWidth = 6
        Me.colNaam.Name = "colNaam"
        '
        'colPercentiel
        '
        Me.colPercentiel.HeaderText = "van percentiel"
        Me.colPercentiel.MinimumWidth = 6
        Me.colPercentiel.Name = "colPercentiel"
        Me.colPercentiel.Width = 110
        '
        'colTotPerc
        '
        Me.colTotPerc.FillWeight = 184.4156!
        Me.colTotPerc.HeaderText = "tot percentiel"
        Me.colTotPerc.MinimumWidth = 6
        Me.colTotPerc.Name = "colTotPerc"
        Me.colTotPerc.Width = 110
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
        Me.radZomWin.Location = New System.Drawing.Point(28, 132)
        Me.radZomWin.Margin = New System.Windows.Forms.Padding(4)
        Me.radZomWin.Name = "radZomWin"
        Me.radZomWin.Size = New System.Drawing.Size(170, 20)
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
        Me.radJaarRond.Size = New System.Drawing.Size(82, 20)
        Me.radJaarRond.TabIndex = 20
        Me.radJaarRond.Text = "Jaarrond"
        Me.radJaarRond.UseVisualStyleBackColor = True
        '
        'Instellingen
        '
        Me.Instellingen.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Instellingen.Controls.Add(Me.btnGroeiseizoenHelp)
        Me.Instellingen.Controls.Add(Me.radAprilAugust)
        Me.Instellingen.Controls.Add(Me.radGroeiseizoen)
        Me.Instellingen.Controls.Add(Me.Label3)
        Me.Instellingen.Controls.Add(Me.radZomWin)
        Me.Instellingen.Controls.Add(Me.txtMinTimestepsBetweenEvents)
        Me.Instellingen.Controls.Add(Me.radJaarRond)
        Me.Instellingen.Controls.Add(Me.cmbDuration)
        Me.Instellingen.Controls.Add(Me.Label4)
        Me.Instellingen.Location = New System.Drawing.Point(581, 145)
        Me.Instellingen.Margin = New System.Windows.Forms.Padding(4)
        Me.Instellingen.Name = "Instellingen"
        Me.Instellingen.Padding = New System.Windows.Forms.Padding(4)
        Me.Instellingen.Size = New System.Drawing.Size(512, 228)
        Me.Instellingen.TabIndex = 26
        Me.Instellingen.TabStop = False
        Me.Instellingen.Text = "Classificeren voor:"
        '
        'btnGroeiseizoenHelp
        '
        Me.btnGroeiseizoenHelp.BackColor = System.Drawing.Color.Yellow
        Me.btnGroeiseizoenHelp.Location = New System.Drawing.Point(463, 101)
        Me.btnGroeiseizoenHelp.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnGroeiseizoenHelp.Name = "btnGroeiseizoenHelp"
        Me.btnGroeiseizoenHelp.Size = New System.Drawing.Size(23, 23)
        Me.btnGroeiseizoenHelp.TabIndex = 31
        Me.btnGroeiseizoenHelp.Text = "?"
        Me.btnGroeiseizoenHelp.UseVisualStyleBackColor = False
        '
        'radAprilAugust
        '
        Me.radAprilAugust.AutoSize = True
        Me.radAprilAugust.Location = New System.Drawing.Point(28, 160)
        Me.radAprilAugust.Margin = New System.Windows.Forms.Padding(4)
        Me.radAprilAugust.Name = "radAprilAugust"
        Me.radAprilAugust.Size = New System.Drawing.Size(207, 20)
        Me.radAprilAugust.TabIndex = 34
        Me.radAprilAugust.Text = "April t/m aug en sept t/m maart"
        Me.radAprilAugust.UseVisualStyleBackColor = True
        '
        'radGroeiseizoen
        '
        Me.radGroeiseizoen.AutoSize = True
        Me.radGroeiseizoen.Checked = True
        Me.radGroeiseizoen.Location = New System.Drawing.Point(28, 103)
        Me.radGroeiseizoen.Margin = New System.Windows.Forms.Padding(4)
        Me.radGroeiseizoen.Name = "radGroeiseizoen"
        Me.radGroeiseizoen.Size = New System.Drawing.Size(361, 20)
        Me.radGroeiseizoen.TabIndex = 33
        Me.radGroeiseizoen.TabStop = True
        Me.radGroeiseizoen.Text = "Groeiseizoen (maart t/m oktober) en buiten groeiseizoen"
        Me.radGroeiseizoen.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(25, 69)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(285, 16)
        Me.Label3.TabIndex = 32
        Me.Label3.Text = "Minimum tijd tussen events (aantal tijdstappen):"
        '
        'txtMinTimestepsBetweenEvents
        '
        Me.txtMinTimestepsBetweenEvents.Location = New System.Drawing.Point(363, 65)
        Me.txtMinTimestepsBetweenEvents.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtMinTimestepsBetweenEvents.Name = "txtMinTimestepsBetweenEvents"
        Me.txtMinTimestepsBetweenEvents.Size = New System.Drawing.Size(125, 22)
        Me.txtMinTimestepsBetweenEvents.TabIndex = 31
        Me.txtMinTimestepsBetweenEvents.Text = "0"
        '
        'cmbDuration
        '
        Me.cmbDuration.FormattingEnabled = True
        Me.cmbDuration.Location = New System.Drawing.Point(361, 31)
        Me.cmbDuration.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbDuration.Name = "cmbDuration"
        Me.cmbDuration.Size = New System.Drawing.Size(125, 24)
        Me.cmbDuration.TabIndex = 19
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(25, 34)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(171, 16)
        Me.Label4.TabIndex = 18
        Me.Label4.Text = "Duur gebeurtenissen (uren):"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(21, 417)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(4)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(943, 28)
        Me.prProgress.TabIndex = 29
        '
        'btnClassify
        '
        Me.btnClassify.BackColor = System.Drawing.Color.Yellow
        Me.btnClassify.Location = New System.Drawing.Point(980, 392)
        Me.btnClassify.Margin = New System.Windows.Forms.Padding(4)
        Me.btnClassify.Name = "btnClassify"
        Me.btnClassify.Size = New System.Drawing.Size(113, 55)
        Me.btnClassify.TabIndex = 25
        Me.btnClassify.Text = "Classificeer!"
        Me.btnClassify.UseVisualStyleBackColor = False
        '
        'grOptions
        '
        Me.grOptions.Controls.Add(Me.chkSeepage)
        Me.grOptions.Location = New System.Drawing.Point(581, 15)
        Me.grOptions.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.grOptions.Name = "grOptions"
        Me.grOptions.Padding = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.grOptions.Size = New System.Drawing.Size(512, 123)
        Me.grOptions.TabIndex = 31
        Me.grOptions.TabStop = False
        Me.grOptions.Text = "Opties"
        '
        'chkSeepage
        '
        Me.chkSeepage.AutoSize = True
        Me.chkSeepage.Location = New System.Drawing.Point(28, 34)
        Me.chkSeepage.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.chkSeepage.Name = "chkSeepage"
        Me.chkSeepage.Size = New System.Drawing.Size(233, 20)
        Me.chkSeepage.TabIndex = 0
        Me.chkSeepage.Text = "Kwel meenemen in de classificatie"
        Me.chkSeepage.UseVisualStyleBackColor = True
        '
        'frmClassifyGroundwaterDHydro
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1107, 460)
        Me.Controls.Add(Me.grOptions)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.groupSbk)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.Instellingen)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnClassify)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.Name = "frmClassifyGroundwaterDHydro"
        Me.Text = "Grondwaterstanden uit D-Hydro classificeren"
        Me.groupSbk.ResumeLayout(False)
        Me.groupSbk.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Instellingen.ResumeLayout(False)
        Me.Instellingen.PerformLayout()
        Me.grOptions.ResumeLayout(False)
        Me.grOptions.PerformLayout()
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
    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents txtMinTimestepsBetweenEvents As Windows.Forms.TextBox
    Friend WithEvents radGroeiseizoen As Windows.Forms.RadioButton
    Friend WithEvents radAprilAugust As Windows.Forms.RadioButton
    Friend WithEvents btnGroeiseizoenHelp As Windows.Forms.Button
    Friend WithEvents colNaam As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPercentiel As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colTotPerc As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents grOptions As Windows.Forms.GroupBox
    Friend WithEvents chkSeepage As Windows.Forms.CheckBox
End Class
