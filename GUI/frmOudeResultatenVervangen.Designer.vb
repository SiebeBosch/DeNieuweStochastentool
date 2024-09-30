<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmOudeResultatenVervangen
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmOudeResultatenVervangen))
        Me.btnStop = New System.Windows.Forms.Button()
        Me.btnRun = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnStop
        '
        Me.btnStop.BackColor = System.Drawing.Color.IndianRed
        Me.btnStop.DialogResult = System.Windows.Forms.DialogResult.No
        Me.btnStop.Location = New System.Drawing.Point(44, 87)
        Me.btnStop.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(120, 55)
        Me.btnStop.TabIndex = 27
        Me.btnStop.Text = "Nee"
        Me.btnStop.UseVisualStyleBackColor = False
        '
        'btnRun
        '
        Me.btnRun.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnRun.DialogResult = System.Windows.Forms.DialogResult.Yes
        Me.btnRun.Location = New System.Drawing.Point(221, 87)
        Me.btnRun.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnRun.Name = "btnRun"
        Me.btnRun.Size = New System.Drawing.Size(120, 55)
        Me.btnRun.TabIndex = 26
        Me.btnRun.Text = "Ja"
        Me.btnRun.UseVisualStyleBackColor = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(24, 11)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.MaximumSize = New System.Drawing.Size(364, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(332, 48)
        Me.Label1.TabIndex = 28
        Me.Label1.Text = "De nabewerking is onder Settings zo ingesteld dat alle bestaande resultaten in de" &
    " database zullen worden vervangen. Doorgaan?"
        '
        'frmOudeResultatenVervangen
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(393, 166)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnStop)
        Me.Controls.Add(Me.btnRun)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Name = "frmOudeResultatenVervangen"
        Me.Text = "Bestaand resultaat vervangen?"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnStop As Button
    Friend WithEvents btnRun As Button
    Friend WithEvents Label1 As Label
End Class
