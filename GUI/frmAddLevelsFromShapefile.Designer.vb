<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAddLevelsFromShapefile
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAddLevelsFromShapefile))
        Me.txtShapeFile = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnShapeFile = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbZP = New System.Windows.Forms.ComboBox()
        Me.cmbWP = New System.Windows.Forms.ComboBox()
        Me.cmbMTP = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cmbMV = New System.Windows.Forms.ComboBox()
        Me.btnExecute = New System.Windows.Forms.Button()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.SuspendLayout()
        '
        'txtShapeFile
        '
        Me.txtShapeFile.Enabled = False
        Me.txtShapeFile.Location = New System.Drawing.Point(113, 31)
        Me.txtShapeFile.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtShapeFile.Name = "txtShapeFile"
        Me.txtShapeFile.Size = New System.Drawing.Size(789, 22)
        Me.txtShapeFile.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(16, 34)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(67, 16)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Shapefile:"
        '
        'btnShapeFile
        '
        Me.btnShapeFile.Location = New System.Drawing.Point(912, 28)
        Me.btnShapeFile.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnShapeFile.Name = "btnShapeFile"
        Me.btnShapeFile.Size = New System.Drawing.Size(31, 28)
        Me.btnShapeFile.TabIndex = 2
        Me.btnShapeFile.Text = ".."
        Me.btnShapeFile.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(583, 68)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(71, 16)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Zomerpeil:"
        '
        'cmbZP
        '
        Me.cmbZP.FormattingEnabled = True
        Me.cmbZP.Location = New System.Drawing.Point(741, 64)
        Me.cmbZP.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmbZP.Name = "cmbZP"
        Me.cmbZP.Size = New System.Drawing.Size(160, 24)
        Me.cmbZP.TabIndex = 4
        '
        'cmbWP
        '
        Me.cmbWP.FormattingEnabled = True
        Me.cmbWP.Location = New System.Drawing.Point(741, 97)
        Me.cmbWP.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmbWP.Name = "cmbWP"
        Me.cmbWP.Size = New System.Drawing.Size(160, 24)
        Me.cmbWP.TabIndex = 5
        '
        'cmbMTP
        '
        Me.cmbMTP.FormattingEnabled = True
        Me.cmbMTP.Location = New System.Drawing.Point(741, 130)
        Me.cmbMTP.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmbMTP.Name = "cmbMTP"
        Me.cmbMTP.Size = New System.Drawing.Size(160, 24)
        Me.cmbMTP.TabIndex = 6
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(583, 101)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(70, 16)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "Winterpeil:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(583, 134)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(135, 16)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Max. toelaatbaar peil:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(583, 169)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(66, 16)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Maaiveld:"
        '
        'cmbMV
        '
        Me.cmbMV.FormattingEnabled = True
        Me.cmbMV.Location = New System.Drawing.Point(741, 165)
        Me.cmbMV.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmbMV.Name = "cmbMV"
        Me.cmbMV.Size = New System.Drawing.Size(160, 24)
        Me.cmbMV.TabIndex = 10
        '
        'btnExecute
        '
        Me.btnExecute.Location = New System.Drawing.Point(839, 217)
        Me.btnExecute.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnExecute.Name = "btnExecute"
        Me.btnExecute.Size = New System.Drawing.Size(104, 42)
        Me.btnExecute.TabIndex = 11
        Me.btnExecute.Text = "Uitvoeren"
        Me.btnExecute.UseVisualStyleBackColor = True
        '
        'frmAddLevelsFromShapefile
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(959, 273)
        Me.Controls.Add(Me.btnExecute)
        Me.Controls.Add(Me.cmbMV)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.cmbMTP)
        Me.Controls.Add(Me.cmbWP)
        Me.Controls.Add(Me.cmbZP)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.btnShapeFile)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtShapeFile)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Name = "frmAddLevelsFromShapefile"
        Me.Text = "Referentiepeilen toevoegen"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtShapeFile As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents btnShapeFile As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents cmbZP As ComboBox
    Friend WithEvents cmbWP As ComboBox
    Friend WithEvents cmbMTP As ComboBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents cmbMV As ComboBox
    Friend WithEvents btnExecute As Button
    Friend WithEvents dlgOpenFile As OpenFileDialog
End Class
