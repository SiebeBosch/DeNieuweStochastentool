<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAddMeteoStation
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
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAddMeteoStation))
    Me.txtName = New System.Windows.Forms.TextBox
    Me.Label1 = New System.Windows.Forms.Label
    Me.Label2 = New System.Windows.Forms.Label
    Me.cmbMeteoSoort = New System.Windows.Forms.ComboBox
    Me.txtArf = New System.Windows.Forms.TextBox
    Me.Label3 = New System.Windows.Forms.Label
    Me.btnOk = New System.Windows.Forms.Button
    Me.btnCancel = New System.Windows.Forms.Button
    Me.SuspendLayout()
    '
    'txtName
    '
    Me.txtName.Location = New System.Drawing.Point(135, 12)
    Me.txtName.Name = "txtName"
    Me.txtName.Size = New System.Drawing.Size(92, 20)
    Me.txtName.TabIndex = 0
    '
    'Label1
    '
    Me.Label1.AutoSize = True
    Me.Label1.Location = New System.Drawing.Point(12, 15)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(71, 13)
    Me.Label1.TabIndex = 1
    Me.Label1.Text = "Stationsnaam"
    '
    'Label2
    '
    Me.Label2.AutoSize = True
    Me.Label2.Location = New System.Drawing.Point(12, 41)
    Me.Label2.Name = "Label2"
    Me.Label2.Size = New System.Drawing.Size(35, 13)
    Me.Label2.TabIndex = 2
    Me.Label2.Text = "Soort:"
    '
    'cmbMeteoSoort
    '
    Me.cmbMeteoSoort.FormattingEnabled = True
    Me.cmbMeteoSoort.Location = New System.Drawing.Point(135, 38)
    Me.cmbMeteoSoort.Name = "cmbMeteoSoort"
    Me.cmbMeteoSoort.Size = New System.Drawing.Size(92, 21)
    Me.cmbMeteoSoort.TabIndex = 3
    '
    'txtArf
    '
    Me.txtArf.Location = New System.Drawing.Point(135, 65)
    Me.txtArf.Name = "txtArf"
    Me.txtArf.Size = New System.Drawing.Size(92, 20)
    Me.txtArf.TabIndex = 4
    Me.txtArf.Text = "1"
    '
    'Label3
    '
    Me.Label3.AutoSize = True
    Me.Label3.Location = New System.Drawing.Point(12, 68)
    Me.Label3.Name = "Label3"
    Me.Label3.Size = New System.Drawing.Size(114, 13)
    Me.Label3.TabIndex = 5
    Me.Label3.Text = "Gebiedsreductiefactor:"
    '
    'btnOk
    '
    Me.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK
    Me.btnOk.Location = New System.Drawing.Point(172, 100)
    Me.btnOk.Name = "btnOk"
    Me.btnOk.Size = New System.Drawing.Size(55, 24)
    Me.btnOk.TabIndex = 6
    Me.btnOk.Text = "Ok"
    Me.btnOk.UseVisualStyleBackColor = True
    '
    'btnCancel
    '
    Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.btnCancel.Location = New System.Drawing.Point(111, 100)
    Me.btnCancel.Name = "btnCancel"
    Me.btnCancel.Size = New System.Drawing.Size(55, 24)
    Me.btnCancel.TabIndex = 7
    Me.btnCancel.Text = "Cancel"
    Me.btnCancel.UseVisualStyleBackColor = True
    '
    'frmAddMeteoStation
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(239, 135)
    Me.Controls.Add(Me.btnCancel)
    Me.Controls.Add(Me.btnOk)
    Me.Controls.Add(Me.Label3)
    Me.Controls.Add(Me.txtArf)
    Me.Controls.Add(Me.cmbMeteoSoort)
    Me.Controls.Add(Me.Label2)
    Me.Controls.Add(Me.Label1)
    Me.Controls.Add(Me.txtName)
    Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
    Me.Name = "frmAddMeteoStation"
    Me.Text = "Meteostation"
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents txtName As System.Windows.Forms.TextBox
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents cmbMeteoSoort As System.Windows.Forms.ComboBox
  Friend WithEvents txtArf As System.Windows.Forms.TextBox
  Friend WithEvents Label3 As System.Windows.Forms.Label
  Friend WithEvents btnOk As System.Windows.Forms.Button
  Friend WithEvents btnCancel As System.Windows.Forms.Button
End Class
