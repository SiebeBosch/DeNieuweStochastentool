<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAddRows
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
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAddRows))
    Me.btnCancel = New System.Windows.Forms.Button
    Me.btnOk = New System.Windows.Forms.Button
    Me.Label6 = New System.Windows.Forms.Label
    Me.txtAantal = New System.Windows.Forms.TextBox
    Me.SuspendLayout()
    '
    'btnCancel
    '
    Me.btnCancel.BackColor = System.Drawing.Color.IndianRed
    Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.btnCancel.Location = New System.Drawing.Point(58, 41)
    Me.btnCancel.Name = "btnCancel"
    Me.btnCancel.Size = New System.Drawing.Size(70, 35)
    Me.btnCancel.TabIndex = 28
    Me.btnCancel.Text = "Afbreken"
    Me.btnCancel.UseVisualStyleBackColor = False
    '
    'btnOk
    '
    Me.btnOk.BackColor = System.Drawing.Color.MediumSeaGreen
    Me.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK
    Me.btnOk.Location = New System.Drawing.Point(134, 41)
    Me.btnOk.Name = "btnOk"
    Me.btnOk.Size = New System.Drawing.Size(70, 35)
    Me.btnOk.TabIndex = 27
    Me.btnOk.Text = "OK"
    Me.btnOk.UseVisualStyleBackColor = False
    '
    'Label6
    '
    Me.Label6.AutoSize = True
    Me.Label6.Location = New System.Drawing.Point(55, 14)
    Me.Label6.Name = "Label6"
    Me.Label6.Size = New System.Drawing.Size(40, 13)
    Me.Label6.TabIndex = 29
    Me.Label6.Text = "Aantal:"
    '
    'txtAantal
    '
    Me.txtAantal.Location = New System.Drawing.Point(134, 11)
    Me.txtAantal.Name = "txtAantal"
    Me.txtAantal.Size = New System.Drawing.Size(70, 20)
    Me.txtAantal.TabIndex = 30
    Me.txtAantal.Text = "1"
    '
    'frmAddRows
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(216, 87)
    Me.Controls.Add(Me.Label6)
    Me.Controls.Add(Me.txtAantal)
    Me.Controls.Add(Me.btnCancel)
    Me.Controls.Add(Me.btnOk)
    Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
    Me.Name = "frmAddRows"
    Me.Text = "Rijen toevoegen"
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents btnCancel As System.Windows.Forms.Button
  Friend WithEvents btnOk As System.Windows.Forms.Button
  Friend WithEvents Label6 As System.Windows.Forms.Label
  Friend WithEvents txtAantal As System.Windows.Forms.TextBox
End Class
