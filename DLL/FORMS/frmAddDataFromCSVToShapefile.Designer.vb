<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAddDataFromCSVToShapefile
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAddDataFromCSVToShapefile))
        Me.btnImport = New System.Windows.Forms.Button()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.txtSF = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnSF = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbIDField = New System.Windows.Forms.ComboBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.cmbValueCol = New System.Windows.Forms.ComboBox()
        Me.cmbFieldNameCol = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbIDCol = New System.Windows.Forms.ComboBox()
        Me.txtCSV = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnCSV = New System.Windows.Forms.Button()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.radShapefield = New System.Windows.Forms.RadioButton()
        Me.cmbShapeField = New System.Windows.Forms.ComboBox()
        Me.radShapeFieldCol = New System.Windows.Forms.RadioButton()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnImport
        '
        Me.btnImport.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnImport.Location = New System.Drawing.Point(1104, 297)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(111, 60)
        Me.btnImport.TabIndex = 0
        Me.btnImport.Text = "Import"
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'prProgress
        '
        Me.prProgress.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.prProgress.Location = New System.Drawing.Point(15, 334)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(1073, 23)
        Me.prProgress.TabIndex = 1
        '
        'lblProgress
        '
        Me.lblProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(12, 297)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(65, 17)
        Me.lblProgress.TabIndex = 2
        Me.lblProgress.Text = "Progress"
        '
        'txtSF
        '
        Me.txtSF.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSF.Location = New System.Drawing.Point(96, 41)
        Me.txtSF.Name = "txtSF"
        Me.txtSF.Size = New System.Drawing.Size(623, 22)
        Me.txtSF.TabIndex = 3
        '
        'Label1
        '
        Me.Label1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(9, 44)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(41, 17)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Path:"
        '
        'btnSF
        '
        Me.btnSF.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSF.Location = New System.Drawing.Point(725, 40)
        Me.btnSF.Name = "btnSF"
        Me.btnSF.Size = New System.Drawing.Size(24, 24)
        Me.btnSF.TabIndex = 5
        Me.btnSF.Text = ".."
        Me.btnSF.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(9, 77)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(59, 17)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "ID Field:"
        '
        'cmbIDField
        '
        Me.cmbIDField.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.cmbIDField.FormattingEnabled = True
        Me.cmbIDField.Location = New System.Drawing.Point(96, 74)
        Me.cmbIDField.Name = "cmbIDField"
        Me.cmbIDField.Size = New System.Drawing.Size(121, 24)
        Me.cmbIDField.TabIndex = 7
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtSF)
        Me.GroupBox1.Controls.Add(Me.cmbIDField)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.btnSF)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 152)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(755, 129)
        Me.GroupBox1.TabIndex = 8
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Target shapefile:"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.cmbValueCol)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.cmbIDCol)
        Me.GroupBox2.Controls.Add(Me.txtCSV)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.btnCSV)
        Me.GroupBox2.Location = New System.Drawing.Point(15, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(752, 134)
        Me.GroupBox2.TabIndex = 9
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Source CSV"
        '
        'cmbValueCol
        '
        Me.cmbValueCol.FormattingEnabled = True
        Me.cmbValueCol.Location = New System.Drawing.Point(166, 84)
        Me.cmbValueCol.Name = "cmbValueCol"
        Me.cmbValueCol.Size = New System.Drawing.Size(121, 24)
        Me.cmbValueCol.TabIndex = 14
        '
        'cmbFieldNameCol
        '
        Me.cmbFieldNameCol.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.cmbFieldNameCol.FormattingEnabled = True
        Me.cmbFieldNameCol.Location = New System.Drawing.Point(297, 67)
        Me.cmbFieldNameCol.Name = "cmbFieldNameCol"
        Me.cmbFieldNameCol.Size = New System.Drawing.Size(121, 24)
        Me.cmbFieldNameCol.TabIndex = 13
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(6, 87)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(93, 17)
        Me.Label6.TabIndex = 12
        Me.Label6.Text = "Value column"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(6, 56)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(76, 17)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "ID Column:"
        '
        'cmbIDCol
        '
        Me.cmbIDCol.FormattingEnabled = True
        Me.cmbIDCol.Location = New System.Drawing.Point(166, 53)
        Me.cmbIDCol.Name = "cmbIDCol"
        Me.cmbIDCol.Size = New System.Drawing.Size(121, 24)
        Me.cmbIDCol.TabIndex = 8
        '
        'txtCSV
        '
        Me.txtCSV.Location = New System.Drawing.Point(166, 21)
        Me.txtCSV.Name = "txtCSV"
        Me.txtCSV.Size = New System.Drawing.Size(549, 22)
        Me.txtCSV.TabIndex = 8
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(6, 24)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(41, 17)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "Path:"
        '
        'btnCSV
        '
        Me.btnCSV.Location = New System.Drawing.Point(721, 21)
        Me.btnCSV.Name = "btnCSV"
        Me.btnCSV.Size = New System.Drawing.Size(24, 24)
        Me.btnCSV.TabIndex = 10
        Me.btnCSV.Text = ".."
        Me.btnCSV.UseVisualStyleBackColor = True
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        '
        'radShapefield
        '
        Me.radShapefield.AutoSize = True
        Me.radShapefield.Checked = True
        Me.radShapefield.Location = New System.Drawing.Point(28, 40)
        Me.radShapefield.Name = "radShapefield"
        Me.radShapefield.Size = New System.Drawing.Size(216, 21)
        Me.radShapefield.TabIndex = 15
        Me.radShapefield.TabStop = True
        Me.radShapefield.Text = "One Shapefield for all results:"
        Me.radShapefield.UseVisualStyleBackColor = True
        '
        'cmbShapeField
        '
        Me.cmbShapeField.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.cmbShapeField.FormattingEnabled = True
        Me.cmbShapeField.Location = New System.Drawing.Point(297, 37)
        Me.cmbShapeField.Name = "cmbShapeField"
        Me.cmbShapeField.Size = New System.Drawing.Size(121, 24)
        Me.cmbShapeField.TabIndex = 16
        '
        'radShapeFieldCol
        '
        Me.radShapeFieldCol.AutoSize = True
        Me.radShapeFieldCol.Location = New System.Drawing.Point(28, 70)
        Me.radShapeFieldCol.Name = "radShapeFieldCol"
        Me.radShapeFieldCol.Size = New System.Drawing.Size(235, 21)
        Me.radShapeFieldCol.TabIndex = 17
        Me.radShapeFieldCol.Text = "Shapefields specified in CSV file:"
        Me.radShapeFieldCol.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.radShapeFieldCol)
        Me.GroupBox3.Controls.Add(Me.radShapefield)
        Me.GroupBox3.Controls.Add(Me.cmbShapeField)
        Me.GroupBox3.Controls.Add(Me.cmbFieldNameCol)
        Me.GroupBox3.Location = New System.Drawing.Point(776, 12)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(439, 269)
        Me.GroupBox3.TabIndex = 10
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Settings:"
        '
        'frmAddDataFromCSVToShapefile
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1227, 369)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnImport)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmAddDataFromCSVToShapefile"
        Me.Text = "Add data from csv to shapefile"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnImport As Windows.Forms.Button
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents txtSF As Windows.Forms.TextBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents btnSF As Windows.Forms.Button
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents cmbIDField As Windows.Forms.ComboBox
    Friend WithEvents GroupBox1 As Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As Windows.Forms.GroupBox
    Friend WithEvents cmbValueCol As Windows.Forms.ComboBox
    Friend WithEvents cmbFieldNameCol As Windows.Forms.ComboBox
    Friend WithEvents Label6 As Windows.Forms.Label
    Friend WithEvents Label4 As Windows.Forms.Label
    Friend WithEvents cmbIDCol As Windows.Forms.ComboBox
    Friend WithEvents txtCSV As Windows.Forms.TextBox
    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents btnCSV As Windows.Forms.Button
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents radShapefield As Windows.Forms.RadioButton
    Friend WithEvents cmbShapeField As Windows.Forms.ComboBox
    Friend WithEvents radShapeFieldCol As Windows.Forms.RadioButton
    Friend WithEvents GroupBox3 As Windows.Forms.GroupBox
End Class
