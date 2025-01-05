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
        Me.txtName = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbMeteoSoort = New System.Windows.Forms.ComboBox()
        Me.txtOppervlak = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnOk = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtARF = New System.Windows.Forms.TextBox()
        Me.cmbGebiedsreductie = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'txtName
        '
        Me.txtName.Location = New System.Drawing.Point(188, 15)
        Me.txtName.Margin = New System.Windows.Forms.Padding(4)
        Me.txtName.Name = "txtName"
        Me.txtName.Size = New System.Drawing.Size(113, 22)
        Me.txtName.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(16, 18)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(89, 16)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Stationsnaam"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(16, 50)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(42, 16)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Soort:"
        '
        'cmbMeteoSoort
        '
        Me.cmbMeteoSoort.FormattingEnabled = True
        Me.cmbMeteoSoort.Location = New System.Drawing.Point(188, 47)
        Me.cmbMeteoSoort.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbMeteoSoort.Name = "cmbMeteoSoort"
        Me.cmbMeteoSoort.Size = New System.Drawing.Size(113, 24)
        Me.cmbMeteoSoort.TabIndex = 3
        '
        'txtOppervlak
        '
        Me.txtOppervlak.Location = New System.Drawing.Point(188, 140)
        Me.txtOppervlak.Margin = New System.Windows.Forms.Padding(4)
        Me.txtOppervlak.Name = "txtOppervlak"
        Me.txtOppervlak.Size = New System.Drawing.Size(113, 22)
        Me.txtOppervlak.TabIndex = 4
        Me.txtOppervlak.Text = "100"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(16, 144)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(159, 16)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Gebiedsoppervlak (km2):"
        '
        'btnOk
        '
        Me.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnOk.Location = New System.Drawing.Point(228, 190)
        Me.btnOk.Margin = New System.Windows.Forms.Padding(4)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(73, 30)
        Me.btnOk.TabIndex = 6
        Me.btnOk.Text = "Ok"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Location = New System.Drawing.Point(147, 190)
        Me.btnCancel.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(73, 30)
        Me.btnCancel.TabIndex = 7
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(16, 113)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(70, 16)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Constante:"
        '
        'txtARF
        '
        Me.txtARF.Location = New System.Drawing.Point(188, 110)
        Me.txtARF.Margin = New System.Windows.Forms.Padding(4)
        Me.txtARF.Name = "txtARF"
        Me.txtARF.Size = New System.Drawing.Size(113, 22)
        Me.txtARF.TabIndex = 9
        Me.txtARF.Text = "1"
        '
        'cmbGebiedsreductie
        '
        Me.cmbGebiedsreductie.FormattingEnabled = True
        Me.cmbGebiedsreductie.Location = New System.Drawing.Point(188, 79)
        Me.cmbGebiedsreductie.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbGebiedsreductie.Name = "cmbGebiedsreductie"
        Me.cmbGebiedsreductie.Size = New System.Drawing.Size(113, 24)
        Me.cmbGebiedsreductie.TabIndex = 10
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(16, 82)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(164, 16)
        Me.Label5.TabIndex = 11
        Me.Label5.Text = "Methode gebiedsreductie:"
        '
        'frmAddMeteoStation
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(319, 233)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.cmbGebiedsreductie)
        Me.Controls.Add(Me.txtARF)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtOppervlak)
        Me.Controls.Add(Me.cmbMeteoSoort)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtName)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "frmAddMeteoStation"
        Me.Text = "Meteostation"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtName As System.Windows.Forms.TextBox
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents cmbMeteoSoort As System.Windows.Forms.ComboBox
  Friend WithEvents txtOppervlak As System.Windows.Forms.TextBox
  Friend WithEvents Label3 As System.Windows.Forms.Label
  Friend WithEvents btnOk As System.Windows.Forms.Button
  Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents Label4 As Windows.Forms.Label
    Friend WithEvents txtARF As Windows.Forms.TextBox
    Friend WithEvents cmbGebiedsreductie As Windows.Forms.ComboBox
    Friend WithEvents Label5 As Windows.Forms.Label
End Class
