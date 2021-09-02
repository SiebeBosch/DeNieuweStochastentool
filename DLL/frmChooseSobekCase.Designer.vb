<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmChooseSobekCase
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmChooseSobekCase))
        Me.btnSbkProject = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtSbkProject = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbSobekCases = New System.Windows.Forms.ComboBox()
        Me.btnSetSobek = New System.Windows.Forms.Button()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        'Me.AxMap1 = New AxMapWinGIS.AxMap()
        'CType(Me.AxMap1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnSbkProject
        '
        Me.btnSbkProject.Location = New System.Drawing.Point(655, 13)
        Me.btnSbkProject.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSbkProject.Name = "btnSbkProject"
        Me.btnSbkProject.Size = New System.Drawing.Size(26, 22)
        Me.btnSbkProject.TabIndex = 6
        Me.btnSbkProject.Text = ".."
        Me.btnSbkProject.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(18, 16)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(56, 17)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "Project:"
        '
        'txtSbkProject
        '
        Me.txtSbkProject.Location = New System.Drawing.Point(142, 13)
        Me.txtSbkProject.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSbkProject.Name = "txtSbkProject"
        Me.txtSbkProject.Size = New System.Drawing.Size(505, 22)
        Me.txtSbkProject.TabIndex = 4
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(18, 55)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(44, 17)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "Case:"
        '
        'cmbSobekCases
        '
        Me.cmbSobekCases.FormattingEnabled = True
        Me.cmbSobekCases.Location = New System.Drawing.Point(142, 48)
        Me.cmbSobekCases.Name = "cmbSobekCases"
        Me.cmbSobekCases.Size = New System.Drawing.Size(539, 24)
        Me.cmbSobekCases.TabIndex = 8
        '
        'btnSetSobek
        '
        Me.btnSetSobek.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnSetSobek.Location = New System.Drawing.Point(585, 89)
        Me.btnSetSobek.Name = "btnSetSobek"
        Me.btnSetSobek.Size = New System.Drawing.Size(96, 47)
        Me.btnSetSobek.TabIndex = 9
        Me.btnSetSobek.Text = "Ok"
        Me.btnSetSobek.UseVisualStyleBackColor = True
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(21, 113)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(558, 23)
        Me.prProgress.TabIndex = 10
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(23, 89)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(51, 17)
        Me.lblProgress.TabIndex = 11
        Me.lblProgress.Text = "Label3"
        '
        'AxMap1
        ''
        'Me.AxMap1.Enabled = True
        'Me.AxMap1.Location = New System.Drawing.Point(503, 79)
        'Me.AxMap1.Name = "AxMap1"
        'Me.AxMap1.OcxState = CType(resources.GetObject("AxMap1.OcxState"), System.Windows.Forms.AxHost.State)
        'Me.AxMap1.Size = New System.Drawing.Size(76, 27)
        'Me.AxMap1.TabIndex = 12
        '
        'frmChooseSobekCase
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(696, 148)
        'Me.Controls.Add(Me.AxMap1)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnSetSobek)
        Me.Controls.Add(Me.cmbSobekCases)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.btnSbkProject)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtSbkProject)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmChooseSobekCase"
        Me.Text = "Choose SOBEK Project and Case"
        'CType(Me.AxMap1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnSbkProject As Windows.Forms.Button
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents txtSbkProject As Windows.Forms.TextBox
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents cmbSobekCases As Windows.Forms.ComboBox
    Friend WithEvents btnSetSobek As Windows.Forms.Button
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents dlgFolder As Windows.Forms.FolderBrowserDialog
    'Friend WithEvents AxMap1 As AxMapWinGIS.AxMap
End Class
