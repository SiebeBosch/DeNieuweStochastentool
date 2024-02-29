<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmClassifyGroundwaterHBV
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmClassifyGroundwaterHBV))
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.hlpProjectDir = New System.Windows.Forms.Button()
        Me.btnProjectDir = New System.Windows.Forms.Button()
        Me.txtProjectDir = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.hlpHBVRapport = New System.Windows.Forms.Button()
        Me.btnExcel = New System.Windows.Forms.Button()
        Me.txtExcelFile = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Instellingen = New System.Windows.Forms.GroupBox()
        Me.chkLZ = New System.Windows.Forms.CheckBox()
        Me.chkUZ = New System.Windows.Forms.CheckBox()
        Me.btnGrootheden = New System.Windows.Forms.Button()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.btnGroeiseizoenHelp = New System.Windows.Forms.Button()
        Me.radGroeiseizoen = New System.Windows.Forms.RadioButton()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtMinTimestepsBetweenEvents = New System.Windows.Forms.TextBox()
        Me.radAprilAugust = New System.Windows.Forms.RadioButton()
        Me.radZomWin = New System.Windows.Forms.RadioButton()
        Me.radJaarRond = New System.Windows.Forms.RadioButton()
        Me.cmbDuration = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.btnClassify = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.grGrondwaterKlassen = New System.Windows.Forms.DataGridView()
        Me.colNaam = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colPercentiel = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colTotPerc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnAddGroundwaterClass = New System.Windows.Forms.Button()
        Me.btnDeleteGroundwaterClass = New System.Windows.Forms.Button()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.GroupBox1.SuspendLayout()
        Me.Instellingen.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.hlpProjectDir)
        Me.GroupBox1.Controls.Add(Me.btnProjectDir)
        Me.GroupBox1.Controls.Add(Me.txtProjectDir)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.hlpHBVRapport)
        Me.GroupBox1.Controls.Add(Me.btnExcel)
        Me.GroupBox1.Controls.Add(Me.txtExcelFile)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(1083, 100)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Langjarige reeks"
        '
        'hlpProjectDir
        '
        Me.hlpProjectDir.BackColor = System.Drawing.Color.Gold
        Me.hlpProjectDir.Location = New System.Drawing.Point(1054, 58)
        Me.hlpProjectDir.Name = "hlpProjectDir"
        Me.hlpProjectDir.Size = New System.Drawing.Size(23, 23)
        Me.hlpProjectDir.TabIndex = 31
        Me.hlpProjectDir.Text = "?"
        Me.hlpProjectDir.UseVisualStyleBackColor = False
        '
        'btnProjectDir
        '
        Me.btnProjectDir.Location = New System.Drawing.Point(1023, 58)
        Me.btnProjectDir.Name = "btnProjectDir"
        Me.btnProjectDir.Size = New System.Drawing.Size(25, 23)
        Me.btnProjectDir.TabIndex = 30
        Me.btnProjectDir.Text = ".."
        Me.btnProjectDir.UseVisualStyleBackColor = True
        '
        'txtProjectDir
        '
        Me.txtProjectDir.Location = New System.Drawing.Point(218, 58)
        Me.txtProjectDir.Name = "txtProjectDir"
        Me.txtProjectDir.Size = New System.Drawing.Size(799, 22)
        Me.txtProjectDir.TabIndex = 29
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(21, 61)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(186, 16)
        Me.Label2.TabIndex = 28
        Me.Label2.Text = "Map met modelschematisatie:"
        '
        'hlpHBVRapport
        '
        Me.hlpHBVRapport.BackColor = System.Drawing.Color.Gold
        Me.hlpHBVRapport.Location = New System.Drawing.Point(1054, 30)
        Me.hlpHBVRapport.Name = "hlpHBVRapport"
        Me.hlpHBVRapport.Size = New System.Drawing.Size(23, 23)
        Me.hlpHBVRapport.TabIndex = 27
        Me.hlpHBVRapport.Text = "?"
        Me.hlpHBVRapport.UseVisualStyleBackColor = False
        '
        'btnExcel
        '
        Me.btnExcel.Location = New System.Drawing.Point(1023, 29)
        Me.btnExcel.Name = "btnExcel"
        Me.btnExcel.Size = New System.Drawing.Size(25, 23)
        Me.btnExcel.TabIndex = 2
        Me.btnExcel.Text = ".."
        Me.btnExcel.UseVisualStyleBackColor = True
        '
        'txtExcelFile
        '
        Me.txtExcelFile.Location = New System.Drawing.Point(218, 30)
        Me.txtExcelFile.Name = "txtExcelFile"
        Me.txtExcelFile.Size = New System.Drawing.Size(799, 22)
        Me.txtExcelFile.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(20, 33)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(148, 16)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "HBV rapport file (Excel):"
        '
        'Instellingen
        '
        Me.Instellingen.Controls.Add(Me.chkLZ)
        Me.Instellingen.Controls.Add(Me.chkUZ)
        Me.Instellingen.Controls.Add(Me.btnGrootheden)
        Me.Instellingen.Controls.Add(Me.Label5)
        Me.Instellingen.Controls.Add(Me.btnGroeiseizoenHelp)
        Me.Instellingen.Controls.Add(Me.radGroeiseizoen)
        Me.Instellingen.Controls.Add(Me.Label3)
        Me.Instellingen.Controls.Add(Me.txtMinTimestepsBetweenEvents)
        Me.Instellingen.Controls.Add(Me.radAprilAugust)
        Me.Instellingen.Controls.Add(Me.radZomWin)
        Me.Instellingen.Controls.Add(Me.radJaarRond)
        Me.Instellingen.Controls.Add(Me.cmbDuration)
        Me.Instellingen.Controls.Add(Me.Label4)
        Me.Instellingen.Location = New System.Drawing.Point(585, 119)
        Me.Instellingen.Margin = New System.Windows.Forms.Padding(4)
        Me.Instellingen.Name = "Instellingen"
        Me.Instellingen.Padding = New System.Windows.Forms.Padding(4)
        Me.Instellingen.Size = New System.Drawing.Size(510, 274)
        Me.Instellingen.TabIndex = 21
        Me.Instellingen.TabStop = False
        Me.Instellingen.Text = "Classificeren voor:"
        '
        'chkLZ
        '
        Me.chkLZ.AutoSize = True
        Me.chkLZ.Location = New System.Drawing.Point(335, 90)
        Me.chkLZ.Name = "chkLZ"
        Me.chkLZ.Size = New System.Drawing.Size(35, 20)
        Me.chkLZ.TabIndex = 31
        Me.chkLZ.Text = "lz"
        Me.chkLZ.UseVisualStyleBackColor = True
        '
        'chkUZ
        '
        Me.chkUZ.AutoSize = True
        Me.chkUZ.Location = New System.Drawing.Point(335, 112)
        Me.chkUZ.Name = "chkUZ"
        Me.chkUZ.Size = New System.Drawing.Size(70, 20)
        Me.chkUZ.TabIndex = 30
        Me.chkUZ.Text = "uz + sm"
        Me.chkUZ.UseVisualStyleBackColor = True
        '
        'btnGrootheden
        '
        Me.btnGrootheden.BackColor = System.Drawing.Color.Gold
        Me.btnGrootheden.Location = New System.Drawing.Point(464, 87)
        Me.btnGrootheden.Name = "btnGrootheden"
        Me.btnGrootheden.Size = New System.Drawing.Size(23, 23)
        Me.btnGrootheden.TabIndex = 29
        Me.btnGrootheden.Text = "?"
        Me.btnGrootheden.UseVisualStyleBackColor = False
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(25, 90)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(239, 16)
        Me.Label5.TabIndex = 28
        Me.Label5.Text = "Classificeren op basis van parameters:"
        '
        'btnGroeiseizoenHelp
        '
        Me.btnGroeiseizoenHelp.BackColor = System.Drawing.Color.Gold
        Me.btnGroeiseizoenHelp.Location = New System.Drawing.Point(464, 170)
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
        Me.radGroeiseizoen.Location = New System.Drawing.Point(28, 173)
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
        Me.Label3.Location = New System.Drawing.Point(25, 62)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(285, 16)
        Me.Label3.TabIndex = 24
        Me.Label3.Text = "Minimum tijd tussen events (aantal tijdstappen):"
        '
        'txtMinTimestepsBetweenEvents
        '
        Me.txtMinTimestepsBetweenEvents.Location = New System.Drawing.Point(335, 59)
        Me.txtMinTimestepsBetweenEvents.Name = "txtMinTimestepsBetweenEvents"
        Me.txtMinTimestepsBetweenEvents.Size = New System.Drawing.Size(117, 22)
        Me.txtMinTimestepsBetweenEvents.TabIndex = 23
        Me.txtMinTimestepsBetweenEvents.Text = "0"
        '
        'radAprilAugust
        '
        Me.radAprilAugust.AutoSize = True
        Me.radAprilAugust.Location = New System.Drawing.Point(28, 219)
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
        Me.radZomWin.Location = New System.Drawing.Point(28, 196)
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
        Me.radJaarRond.Location = New System.Drawing.Point(28, 242)
        Me.radJaarRond.Margin = New System.Windows.Forms.Padding(4)
        Me.radJaarRond.Name = "radJaarRond"
        Me.radJaarRond.Size = New System.Drawing.Size(79, 20)
        Me.radJaarRond.TabIndex = 20
        Me.radJaarRond.Text = "Jaarrond"
        Me.radJaarRond.UseVisualStyleBackColor = True
        '
        'cmbDuration
        '
        Me.cmbDuration.FormattingEnabled = True
        Me.cmbDuration.Location = New System.Drawing.Point(335, 28)
        Me.cmbDuration.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbDuration.Name = "cmbDuration"
        Me.cmbDuration.Size = New System.Drawing.Size(117, 24)
        Me.cmbDuration.TabIndex = 19
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(25, 31)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(171, 16)
        Me.Label4.TabIndex = 18
        Me.Label4.Text = "Duur gebeurtenissen (uren):"
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(13, 401)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(70, 16)
        Me.lblProgress.TabIndex = 29
        Me.lblProgress.Text = "Voortgang"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(17, 420)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(4)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(948, 28)
        Me.prProgress.TabIndex = 28
        '
        'btnClassify
        '
        Me.btnClassify.BackColor = System.Drawing.Color.Gold
        Me.btnClassify.Location = New System.Drawing.Point(981, 401)
        Me.btnClassify.Margin = New System.Windows.Forms.Padding(4)
        Me.btnClassify.Name = "btnClassify"
        Me.btnClassify.Size = New System.Drawing.Size(113, 48)
        Me.btnClassify.TabIndex = 27
        Me.btnClassify.Text = "Classificeer!"
        Me.btnClassify.UseVisualStyleBackColor = False
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.grGrondwaterKlassen)
        Me.GroupBox2.Controls.Add(Me.btnAddGroundwaterClass)
        Me.GroupBox2.Controls.Add(Me.btnDeleteGroundwaterClass)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 119)
        Me.GroupBox2.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox2.Size = New System.Drawing.Size(565, 274)
        Me.GroupBox2.TabIndex = 30
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Klassen"
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
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        '
        'frmClassifyGroundwaterHBV
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1107, 460)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.Instellingen)
        Me.Controls.Add(Me.btnClassify)
        Me.Controls.Add(Me.GroupBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmClassifyGroundwaterHBV"
        Me.Text = "Grondwater en bodemvocht uit HBV classificeren"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.Instellingen.ResumeLayout(False)
        Me.Instellingen.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        CType(Me.grGrondwaterKlassen, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents GroupBox1 As Windows.Forms.GroupBox
    Friend WithEvents btnExcel As Windows.Forms.Button
    Friend WithEvents txtExcelFile As Windows.Forms.TextBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents Instellingen As Windows.Forms.GroupBox
    Friend WithEvents btnGroeiseizoenHelp As Windows.Forms.Button
    Friend WithEvents radGroeiseizoen As Windows.Forms.RadioButton
    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents txtMinTimestepsBetweenEvents As Windows.Forms.TextBox
    Friend WithEvents radAprilAugust As Windows.Forms.RadioButton
    Friend WithEvents radZomWin As Windows.Forms.RadioButton
    Friend WithEvents radJaarRond As Windows.Forms.RadioButton
    Friend WithEvents cmbDuration As Windows.Forms.ComboBox
    Friend WithEvents Label4 As Windows.Forms.Label
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents btnClassify As Windows.Forms.Button
    Friend WithEvents GroupBox2 As Windows.Forms.GroupBox
    Public WithEvents grGrondwaterKlassen As Windows.Forms.DataGridView
    Friend WithEvents colNaam As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colPercentiel As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colTotPerc As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btnAddGroundwaterClass As Windows.Forms.Button
    Friend WithEvents btnDeleteGroundwaterClass As Windows.Forms.Button
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents hlpHBVRapport As Windows.Forms.Button
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents btnProjectDir As Windows.Forms.Button
    Friend WithEvents txtProjectDir As Windows.Forms.TextBox
    Friend WithEvents hlpProjectDir As Windows.Forms.Button
    Friend WithEvents dlgFolder As Windows.Forms.FolderBrowserDialog
    Friend WithEvents Label5 As Windows.Forms.Label
    Friend WithEvents btnGrootheden As Windows.Forms.Button
    Friend WithEvents chkLZ As Windows.Forms.CheckBox
    Friend WithEvents chkUZ As Windows.Forms.CheckBox
End Class
