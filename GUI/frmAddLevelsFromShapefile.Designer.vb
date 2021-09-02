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
        Me.txtShapeFile.Location = New System.Drawing.Point(85, 25)
        Me.txtShapeFile.Name = "txtShapeFile"
        Me.txtShapeFile.Size = New System.Drawing.Size(593, 20)
        Me.txtShapeFile.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 28)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(54, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Shapefile:"
        '
        'btnShapeFile
        '
        Me.btnShapeFile.Location = New System.Drawing.Point(684, 23)
        Me.btnShapeFile.Name = "btnShapeFile"
        Me.btnShapeFile.Size = New System.Drawing.Size(23, 23)
        Me.btnShapeFile.TabIndex = 2
        Me.btnShapeFile.Text = ".."
        Me.btnShapeFile.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(437, 55)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(56, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Zomerpeil:"
        '
        'cmbZP
        '
        Me.cmbZP.FormattingEnabled = True
        Me.cmbZP.Location = New System.Drawing.Point(556, 52)
        Me.cmbZP.Name = "cmbZP"
        Me.cmbZP.Size = New System.Drawing.Size(121, 21)
        Me.cmbZP.TabIndex = 4
        '
        'cmbWP
        '
        Me.cmbWP.FormattingEnabled = True
        Me.cmbWP.Location = New System.Drawing.Point(556, 79)
        Me.cmbWP.Name = "cmbWP"
        Me.cmbWP.Size = New System.Drawing.Size(121, 21)
        Me.cmbWP.TabIndex = 5
        '
        'cmbMTP
        '
        Me.cmbMTP.FormattingEnabled = True
        Me.cmbMTP.Location = New System.Drawing.Point(556, 106)
        Me.cmbMTP.Name = "cmbMTP"
        Me.cmbMTP.Size = New System.Drawing.Size(121, 21)
        Me.cmbMTP.TabIndex = 6
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(437, 82)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(57, 13)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "Winterpeil:"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(437, 109)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(108, 13)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Max. toelaatbaar peil:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(437, 137)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(53, 13)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Maaiveld:"
        '
        'cmbMV
        '
        Me.cmbMV.FormattingEnabled = True
        Me.cmbMV.Location = New System.Drawing.Point(556, 134)
        Me.cmbMV.Name = "cmbMV"
        Me.cmbMV.Size = New System.Drawing.Size(121, 21)
        Me.cmbMV.TabIndex = 10
        '
        'btnExecute
        '
        Me.btnExecute.Location = New System.Drawing.Point(629, 176)
        Me.btnExecute.Name = "btnExecute"
        Me.btnExecute.Size = New System.Drawing.Size(78, 34)
        Me.btnExecute.TabIndex = 11
        Me.btnExecute.Text = "Uitvoeren"
        Me.btnExecute.UseVisualStyleBackColor = True
        '
        'frmAddLevelsFromShapefile
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(719, 222)
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
