<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAccess2SQLite
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAccess2SQLite))
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.btnConvert = New System.Windows.Forms.Button()
        Me.btnSQLite = New System.Windows.Forms.Button()
        Me.txtSQLite = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnAccess = New System.Windows.Forms.Button()
        Me.txtAccess = New System.Windows.Forms.TextBox()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.dlgSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.SuspendLayout()
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(20, 115)
        Me.lblProgress.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(69, 17)
        Me.lblProgress.TabIndex = 17
        Me.lblProgress.Text = "Progress:"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(20, 138)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(4)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(923, 28)
        Me.prProgress.TabIndex = 16
        '
        'btnConvert
        '
        Me.btnConvert.Location = New System.Drawing.Point(951, 115)
        Me.btnConvert.Margin = New System.Windows.Forms.Padding(4)
        Me.btnConvert.Name = "btnConvert"
        Me.btnConvert.Size = New System.Drawing.Size(100, 52)
        Me.btnConvert.TabIndex = 15
        Me.btnConvert.Text = "Convert"
        Me.btnConvert.UseVisualStyleBackColor = True
        '
        'btnSQLite
        '
        Me.btnSQLite.Location = New System.Drawing.Point(1021, 68)
        Me.btnSQLite.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSQLite.Name = "btnSQLite"
        Me.btnSQLite.Size = New System.Drawing.Size(29, 25)
        Me.btnSQLite.TabIndex = 14
        Me.btnSQLite.Text = ".."
        Me.btnSQLite.UseVisualStyleBackColor = True
        '
        'txtSQLite
        '
        Me.txtSQLite.Location = New System.Drawing.Point(147, 69)
        Me.txtSQLite.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSQLite.Name = "txtSQLite"
        Me.txtSQLite.Size = New System.Drawing.Size(865, 22)
        Me.txtSQLite.TabIndex = 13
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(16, 73)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(118, 17)
        Me.Label2.TabIndex = 12
        Me.Label2.Text = "SQLite database:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(16, 36)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(120, 17)
        Me.Label1.TabIndex = 11
        Me.Label1.Text = "Access database:"
        '
        'btnAccess
        '
        Me.btnAccess.Location = New System.Drawing.Point(1021, 32)
        Me.btnAccess.Margin = New System.Windows.Forms.Padding(4)
        Me.btnAccess.Name = "btnAccess"
        Me.btnAccess.Size = New System.Drawing.Size(29, 25)
        Me.btnAccess.TabIndex = 10
        Me.btnAccess.Text = ".."
        Me.btnAccess.UseVisualStyleBackColor = True
        '
        'txtAccess
        '
        Me.txtAccess.Location = New System.Drawing.Point(147, 32)
        Me.txtAccess.Margin = New System.Windows.Forms.Padding(4)
        Me.txtAccess.Name = "txtAccess"
        Me.txtAccess.Size = New System.Drawing.Size(865, 22)
        Me.txtAccess.TabIndex = 9
        '
        'frmAccess2SQLite
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1067, 199)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnConvert)
        Me.Controls.Add(Me.btnSQLite)
        Me.Controls.Add(Me.txtSQLite)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnAccess)
        Me.Controls.Add(Me.txtAccess)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmAccess2SQLite"
        Me.Text = "Access to SQLite"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents btnConvert As Windows.Forms.Button
    Friend WithEvents btnSQLite As Windows.Forms.Button
    Friend WithEvents txtSQLite As Windows.Forms.TextBox
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents btnAccess As Windows.Forms.Button
    Friend WithEvents txtAccess As Windows.Forms.TextBox
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents dlgSaveFile As Windows.Forms.SaveFileDialog
End Class
