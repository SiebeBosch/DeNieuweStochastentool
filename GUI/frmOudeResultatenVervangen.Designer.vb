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
        Me.btnStop.Location = New System.Drawing.Point(33, 71)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(90, 45)
        Me.btnStop.TabIndex = 27
        Me.btnStop.Text = "Nee"
        Me.btnStop.UseVisualStyleBackColor = False
        '
        'btnRun
        '
        Me.btnRun.BackColor = System.Drawing.Color.MediumSeaGreen
        Me.btnRun.DialogResult = System.Windows.Forms.DialogResult.Yes
        Me.btnRun.Location = New System.Drawing.Point(166, 71)
        Me.btnRun.Name = "btnRun"
        Me.btnRun.Size = New System.Drawing.Size(90, 45)
        Me.btnRun.TabIndex = 26
        Me.btnRun.Text = "Ja"
        Me.btnRun.UseVisualStyleBackColor = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(18, 9)
        Me.Label1.MaximumSize = New System.Drawing.Size(273, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(265, 39)
        Me.Label1.TabIndex = 28
        Me.Label1.Text = "De nabewerking is onder Settings zo ingesteld dat alle bestaande resultaten in de" &
    " database zullen worden vervangen. Doorgaan?"
        '
        'frmOudeResultatenVervangen
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(295, 135)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnStop)
        Me.Controls.Add(Me.btnRun)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmOudeResultatenVervangen"
        Me.Text = "Bestaand resultaat vervangen?"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnStop As Button
    Friend WithEvents btnRun As Button
    Friend WithEvents Label1 As Label
End Class
