<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAddBoundaryClass
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
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAddBoundaryClass))
    Me.txtNaam = New System.Windows.Forms.TextBox
    Me.Label1 = New System.Windows.Forms.Label
    Me.btnCancel = New System.Windows.Forms.Button
    Me.btnOk = New System.Windows.Forms.Button
    Me.SuspendLayout()
    '
    'txtNaam
    '
    Me.txtNaam.Location = New System.Drawing.Point(85, 19)
    Me.txtNaam.Name = "txtNaam"
    Me.txtNaam.Size = New System.Drawing.Size(241, 20)
    Me.txtNaam.TabIndex = 0
    '
    'Label1
    '
    Me.Label1.AutoSize = True
    Me.Label1.Location = New System.Drawing.Point(21, 22)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(38, 13)
    Me.Label1.TabIndex = 1
    Me.Label1.Text = "Naam:"
    '
    'btnCancel
    '
    Me.btnCancel.BackColor = System.Drawing.Color.IndianRed
    Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.btnCancel.Location = New System.Drawing.Point(180, 58)
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
    Me.btnOk.Location = New System.Drawing.Point(256, 58)
    Me.btnOk.Name = "btnOk"
    Me.btnOk.Size = New System.Drawing.Size(70, 35)
    Me.btnOk.TabIndex = 27
    Me.btnOk.Text = "OK"
    Me.btnOk.UseVisualStyleBackColor = False
    '
    'frmAddBoundaryClass
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(338, 105)
    Me.Controls.Add(Me.btnCancel)
    Me.Controls.Add(Me.btnOk)
    Me.Controls.Add(Me.Label1)
    Me.Controls.Add(Me.txtNaam)
    Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
    Me.Name = "frmAddBoundaryClass"
    Me.Text = "Randvoorwaardenklasse toevoegen"
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents txtNaam As System.Windows.Forms.TextBox
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents btnCancel As System.Windows.Forms.Button
  Friend WithEvents btnOk As System.Windows.Forms.Button
End Class
