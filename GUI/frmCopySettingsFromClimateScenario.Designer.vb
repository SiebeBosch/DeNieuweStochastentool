<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmCopySettingsFromClimateScenario
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCopySettingsFromClimateScenario))
        Me.btnRun = New System.Windows.Forms.Button()
        Me.cmbScenarios = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'btnRun
        '
        Me.btnRun.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnRun.Location = New System.Drawing.Point(427, 93)
        Me.btnRun.Name = "btnRun"
        Me.btnRun.Size = New System.Drawing.Size(100, 42)
        Me.btnRun.TabIndex = 0
        Me.btnRun.Text = "Uitvoeren"
        Me.btnRun.UseVisualStyleBackColor = True
        '
        'cmbScenarios
        '
        Me.cmbScenarios.FormattingEnabled = True
        Me.cmbScenarios.Location = New System.Drawing.Point(26, 23)
        Me.cmbScenarios.Name = "cmbScenarios"
        Me.cmbScenarios.Size = New System.Drawing.Size(501, 24)
        Me.cmbScenarios.TabIndex = 1
        '
        'frmCopySettingsFromClimateScenario
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(539, 147)
        Me.Controls.Add(Me.cmbScenarios)
        Me.Controls.Add(Me.btnRun)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmCopySettingsFromClimateScenario"
        Me.Text = "Kopieer instellingen van scenario:"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnRun As Button
    Friend WithEvents cmbScenarios As ComboBox
End Class
