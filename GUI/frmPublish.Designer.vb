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
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.txtConfigurationName)
        Me.GroupBox1.Controls.Add(Me.radExistingWebviewer)
        Me.GroupBox1.Controls.Add(Me.radNewWebviewer)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
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
        Me.radExistingWebviewer.Size = New System.Drawing.Size(286, 20)
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
        Me.radNewWebviewer.Size = New System.Drawing.Size(176, 20)
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
        'frmPublish
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnPubliceren)
        Me.Controls.Add(Me.GroupBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmPublish"
        Me.Text = "Uitkomsten publiceren"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
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
End Class
