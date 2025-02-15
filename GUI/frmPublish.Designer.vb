<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPublish
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPublish))
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtConfigurationName = New System.Windows.Forms.TextBox()
        Me.radExistingWebviewer = New System.Windows.Forms.RadioButton()
        Me.radNewWebviewer = New System.Windows.Forms.RadioButton()
        Me.btnPubliceren = New System.Windows.Forms.Button()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtIP = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtPort = New System.Windows.Forms.TextBox()
        Me.radAPI = New System.Windows.Forms.RadioButton()
        Me.radGeoJSON = New System.Windows.Forms.RadioButton()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.chkParameters1D = New System.Windows.Forms.CheckedListBox()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.txtConfigurationName)
        Me.GroupBox1.Controls.Add(Me.radExistingWebviewer)
        Me.GroupBox1.Controls.Add(Me.radNewWebviewer)
        Me.GroupBox1.Location = New System.Drawing.Point(15, 50)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(776, 137)
        Me.GroupBox1.TabIndex = 0
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Opties"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(16, 95)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(119, 16)
        Me.Label1.TabIndex = 3
        Me.Label1.Text = "Naam configuratie:"
        '
        'txtConfigurationName
        '
        Me.txtConfigurationName.Location = New System.Drawing.Point(141, 92)
        Me.txtConfigurationName.Name = "txtConfigurationName"
        Me.txtConfigurationName.Size = New System.Drawing.Size(617, 22)
        Me.txtConfigurationName.TabIndex = 2
        Me.txtConfigurationName.Text = "Basis"
        '
        'radExistingWebviewer
        '
        Me.radExistingWebviewer.AutoSize = True
        Me.radExistingWebviewer.Location = New System.Drawing.Point(19, 59)
        Me.radExistingWebviewer.Name = "radExistingWebviewer"
        Me.radExistingWebviewer.Size = New System.Drawing.Size(283, 20)
        Me.radExistingWebviewer.TabIndex = 1
        Me.radExistingWebviewer.Text = "Toevoegen aan een bestaande webviewer"
        Me.radExistingWebviewer.UseVisualStyleBackColor = True
        '
        'radNewWebviewer
        '
        Me.radNewWebviewer.AutoSize = True
        Me.radNewWebviewer.Checked = True
        Me.radNewWebviewer.Location = New System.Drawing.Point(19, 33)
        Me.radNewWebviewer.Name = "radNewWebviewer"
        Me.radNewWebviewer.Size = New System.Drawing.Size(173, 20)
        Me.radNewWebviewer.TabIndex = 0
        Me.radNewWebviewer.TabStop = True
        Me.radNewWebviewer.Text = "In een nieuwe webviewer"
        Me.radNewWebviewer.UseVisualStyleBackColor = True
        '
        'btnPubliceren
        '
        Me.btnPubliceren.Location = New System.Drawing.Point(674, 380)
        Me.btnPubliceren.Name = "btnPubliceren"
        Me.btnPubliceren.Size = New System.Drawing.Size(114, 58)
        Me.btnPubliceren.TabIndex = 1
        Me.btnPubliceren.Text = "Publiceren"
        Me.btnPubliceren.UseVisualStyleBackColor = True
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(12, 415)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(656, 23)
        Me.prProgress.TabIndex = 2
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(12, 392)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(70, 16)
        Me.lblProgress.TabIndex = 4
        Me.lblProgress.Text = "Voorgang:"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.Label3)
        Me.GroupBox3.Controls.Add(Me.txtIP)
        Me.GroupBox3.Controls.Add(Me.Label2)
        Me.GroupBox3.Controls.Add(Me.txtPort)
        Me.GroupBox3.Controls.Add(Me.radAPI)
        Me.GroupBox3.Controls.Add(Me.radGeoJSON)
        Me.GroupBox3.Location = New System.Drawing.Point(323, 193)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(465, 181)
        Me.GroupBox3.TabIndex = 6
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Opties 2D"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(49, 93)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(61, 16)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "IP-adres:"
        '
        'txtIP
        '
        Me.txtIP.Location = New System.Drawing.Point(145, 90)
        Me.txtIP.Name = "txtIP"
        Me.txtIP.Size = New System.Drawing.Size(104, 22)
        Me.txtIP.TabIndex = 6
        Me.txtIP.Text = "localhost"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(49, 120)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(90, 16)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Poortnummer:"
        '
        'txtPort
        '
        Me.txtPort.Location = New System.Drawing.Point(145, 117)
        Me.txtPort.Name = "txtPort"
        Me.txtPort.Size = New System.Drawing.Size(104, 22)
        Me.txtPort.TabIndex = 4
        Me.txtPort.Text = "8000"
        '
        'radAPI
        '
        Me.radAPI.AutoSize = True
        Me.radAPI.Location = New System.Drawing.Point(25, 64)
        Me.radAPI.Name = "radAPI"
        Me.radAPI.Size = New System.Drawing.Size(221, 20)
        Me.radAPI.TabIndex = 5
        Me.radAPI.Text = "Via API (voor grote 2D-modellen)"
        Me.radAPI.UseVisualStyleBackColor = True
        '
        'radGeoJSON
        '
        Me.radGeoJSON.AutoSize = True
        Me.radGeoJSON.Checked = True
        Me.radGeoJSON.Location = New System.Drawing.Point(25, 38)
        Me.radGeoJSON.Name = "radGeoJSON"
        Me.radGeoJSON.Size = New System.Drawing.Size(170, 20)
        Me.radGeoJSON.TabIndex = 4
        Me.radGeoJSON.TabStop = True
        Me.radGeoJSON.Text = "In memory (aanbevolen)"
        Me.radGeoJSON.UseVisualStyleBackColor = True
        '
        'MenuStrip1
        '
        Me.MenuStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.HelpToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(800, 24)
        Me.MenuStrip1.TabIndex = 7
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        Me.HelpToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
        Me.HelpToolStripMenuItem.Text = "Help"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.chkParameters1D)
        Me.GroupBox2.Location = New System.Drawing.Point(7, 193)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(310, 181)
        Me.GroupBox2.TabIndex = 5
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Opties 1D"
        '
        'chkParameters1D
        '
        Me.chkParameters1D.FormattingEnabled = True
        Me.chkParameters1D.Location = New System.Drawing.Point(20, 38)
        Me.chkParameters1D.Name = "chkParameters1D"
        Me.chkParameters1D.Size = New System.Drawing.Size(271, 123)
        Me.chkParameters1D.TabIndex = 6
        '
        'frmPublish
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnPubliceren)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "frmPublish"
        Me.Text = "Uitkomsten publiceren"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents Label1 As Label
    Friend WithEvents txtConfigurationName As TextBox
    Friend WithEvents radExistingWebviewer As RadioButton
    Friend WithEvents radNewWebviewer As RadioButton
    Friend WithEvents btnPubliceren As Button
    Friend WithEvents dlgFolder As FolderBrowserDialog
    Friend WithEvents prProgress As ProgressBar
    Friend WithEvents lblProgress As Label
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents radAPI As RadioButton
    Friend WithEvents radGeoJSON As RadioButton
    Friend WithEvents Label2 As Label
    Friend WithEvents txtPort As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents txtIP As TextBox
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents HelpToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents chkParameters1D As CheckedListBox
End Class
