<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAddStochastenNode
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
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAddStochastenNode))
    Me.cmbModelID = New System.Windows.Forms.ComboBox
    Me.Label1 = New System.Windows.Forms.Label
    Me.Label2 = New System.Windows.Forms.Label
    Me.txtNodeID = New System.Windows.Forms.TextBox
    Me.btnCancel = New System.Windows.Forms.Button
    Me.btnOk = New System.Windows.Forms.Button
    Me.SuspendLayout()
    '
    'cmbModelID
    '
    Me.cmbModelID.FormattingEnabled = True
    Me.cmbModelID.Location = New System.Drawing.Point(65, 19)
    Me.cmbModelID.Name = "cmbModelID"
    Me.cmbModelID.Size = New System.Drawing.Size(170, 21)
    Me.cmbModelID.TabIndex = 0
    '
    'Label1
    '
    Me.Label1.AutoSize = True
    Me.Label1.Location = New System.Drawing.Point(12, 22)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(47, 13)
    Me.Label1.TabIndex = 1
    Me.Label1.Text = "ModelID"
    '
    'Label2
    '
    Me.Label2.AutoSize = True
    Me.Label2.Location = New System.Drawing.Point(12, 54)
    Me.Label2.Name = "Label2"
    Me.Label2.Size = New System.Drawing.Size(49, 13)
    Me.Label2.TabIndex = 2
    Me.Label2.Text = "KnoopID"
    '
    'txtNodeID
    '
    Me.txtNodeID.Location = New System.Drawing.Point(65, 51)
    Me.txtNodeID.Name = "txtNodeID"
    Me.txtNodeID.Size = New System.Drawing.Size(170, 20)
    Me.txtNodeID.TabIndex = 3
    '
    'btnCancel
    '
    Me.btnCancel.BackColor = System.Drawing.Color.IndianRed
    Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
    Me.btnCancel.Location = New System.Drawing.Point(89, 87)
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
    Me.btnOk.Location = New System.Drawing.Point(165, 87)
    Me.btnOk.Name = "btnOk"
    Me.btnOk.Size = New System.Drawing.Size(70, 35)
    Me.btnOk.TabIndex = 27
    Me.btnOk.Text = "OK"
    Me.btnOk.UseVisualStyleBackColor = False
    '
    'fromAddStochastenNode
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.ClientSize = New System.Drawing.Size(247, 136)
    Me.Controls.Add(Me.btnCancel)
    Me.Controls.Add(Me.btnOk)
    Me.Controls.Add(Me.txtNodeID)
    Me.Controls.Add(Me.Label2)
    Me.Controls.Add(Me.Label1)
    Me.Controls.Add(Me.cmbModelID)
    Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
    Me.Name = "fromAddStochastenNode"
    Me.Text = "Randknoop toevoegen"
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents cmbModelID As System.Windows.Forms.ComboBox
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents txtNodeID As System.Windows.Forms.TextBox
  Friend WithEvents btnCancel As System.Windows.Forms.Button
  Friend WithEvents btnOk As System.Windows.Forms.Button
End Class
