<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAddStochast
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
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAddStochast))
    Me.txtStochastFile = New System.Windows.Forms.TextBox
    Me.txtStochastNaam = New System.Windows.Forms.TextBox
    Me.Label10 = New System.Windows.Forms.Label
    Me.Label3 = New System.Windows.Forms.Label
    Me.Label6 = New System.Windows.Forms.Label
    Me.txtStochastKans = New System.Windows.Forms.TextBox
    Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog
    Me.btnCancel = New System.Windows.Forms.Button
    Me.btnOk = New System.Windows.Forms.Button
    Me.SuspendLayout()
    '
    'txtStochastFile
    '
    Me.txtStochastFile.Location = New System.Drawing.Point(76, 14)
    Me.txtStochastFile.Name = "txtStochastFile"
    Me.txtStochastFile.Size = New System.Drawing.Size(243, 20)
    Me.txtStochastFile.TabIndex = 17
    '
    'txtStochastNaam
    '
    Me.txtStochastNaam.Location = New System.Drawing.Point(76, 44)
    Me.txtStochastNaam.Name = "txtStochastNaam"
    Me.txtStochastNaam.Size = New System.Drawing.Size(243, 20)
    Me.txtStochastNaam.TabIndex = 13
    '
    'Label10
    '
    Me.Label10.AutoSize = True
    Me.Label10.Location = New System.Drawing.Point(9, 19)
    Me.Label10.Name = "Label10"
    Me.Label10.Size = New System.Drawing.Size(49, 13)
    Me.Label10.TabIndex = 18
    Me.Label10.Text = "Bestand:"
    '
    'Label3
    '
    Me.Label3.AutoSize = True
    Me.Label3.Location = New System.Drawing.Point(9, 47)
    Me.Label3.Name = "Label3"
    Me.Label3.Size = New System.Drawing.Size(38, 13)
    Me.Label3.TabIndex = 14
    Me.Label3.Text = "Naam:"
    '
    'Label6
    '
    Me.Label6.AutoSize = True
    Me.Label6.Location = New System.Drawing.Point(9, 80)
    Me.Label6.Name = "Label6"
    Me.Label6.Size = New System.Drawing.Size(34, 13)
    Me.Label6.TabIndex = 15
    Me.Label6.Text = "Kans:"
    '
    'txtStochastKans
    '
    Me.txtStochastKans.Location = New System.Drawing.Point(76, 77)
    Me.txtStochastKans.Name = "txtStochastKans"
    Me.txtStochastKans.Size = New System.Drawing.Size(59, 20)
    Me.txtStochastKans.TabIndex = 16
    '
    'btnCancel
    '
    Me.btnCancel.BackColor = System.Drawing.Color.IndianRed
    Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.btnCancel.Location = New System.Drawing.Point(172, 105)
    Me.btnCancel.Name = "btnCancel"
    Me.btnCancel.Size = New System.Drawing.Size(70, 35)
    Me.btnCancel.TabIndex = 26
    Me.btnCancel.Text = "Afbreken"
    Me.btnCancel.UseVisualStyleBackColor = False
    '
    'btnOk
    '
    Me.btnOk.BackColor = System.Drawing.Color.MediumSeaGreen
    Me.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK
    Me.btnOk.Location = New System.Drawing.Point(248, 105)
    Me.btnOk.Name = "btnOk"
    Me.btnOk.Size = New System.Drawing.Size(70, 35)
    Me.btnOk.TabIndex = 25
    Me.btnOk.Text = "OK"
    Me.btnOk.UseVisualStyleBackColor = False
    '
    'frmAddStochast
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(330, 149)
    Me.Controls.Add(Me.btnCancel)
    Me.Controls.Add(Me.btnOk)
    Me.Controls.Add(Me.txtStochastFile)
    Me.Controls.Add(Me.txtStochastNaam)
    Me.Controls.Add(Me.Label10)
    Me.Controls.Add(Me.Label3)
    Me.Controls.Add(Me.Label6)
    Me.Controls.Add(Me.txtStochastKans)
    Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
    Me.Name = "frmAddStochast"
    Me.Text = "Stochast toevoegen"
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents txtStochastFile As System.Windows.Forms.TextBox
  Friend WithEvents txtStochastNaam As System.Windows.Forms.TextBox
  Friend WithEvents Label10 As System.Windows.Forms.Label
  Friend WithEvents Label3 As System.Windows.Forms.Label
  Friend WithEvents Label6 As System.Windows.Forms.Label
  Friend WithEvents txtStochastKans As System.Windows.Forms.TextBox
  Friend WithEvents dlgOpenFile As System.Windows.Forms.OpenFileDialog
  Friend WithEvents btnCancel As System.Windows.Forms.Button
  Friend WithEvents btnOk As System.Windows.Forms.Button
End Class
