<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLocationsFromShapefile
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLocationsFromShapefile))
        Me.txtShapefile = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnShapefile = New System.Windows.Forms.Button()
        Me.cmbIDField = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnImport = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtTableName = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtLocationsField = New System.Windows.Forms.TextBox()
        Me.txtXField = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtYField = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.txtLatField = New System.Windows.Forms.TextBox()
        Me.txtLonField = New System.Windows.Forms.TextBox()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtShapefile
        '
        Me.txtShapefile.Location = New System.Drawing.Point(115, 30)
        Me.txtShapefile.Name = "txtShapefile"
        Me.txtShapefile.Size = New System.Drawing.Size(625, 22)
        Me.txtShapefile.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(18, 30)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(71, 17)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Shapefile:"
        '
        'btnShapefile
        '
        Me.btnShapefile.Location = New System.Drawing.Point(746, 27)
        Me.btnShapefile.Name = "btnShapefile"
        Me.btnShapefile.Size = New System.Drawing.Size(24, 23)
        Me.btnShapefile.TabIndex = 2
        Me.btnShapefile.Text = ".."
        Me.btnShapefile.UseVisualStyleBackColor = True
        '
        'cmbIDField
        '
        Me.cmbIDField.FormattingEnabled = True
        Me.cmbIDField.Location = New System.Drawing.Point(115, 67)
        Me.cmbIDField.Name = "cmbIDField"
        Me.cmbIDField.Size = New System.Drawing.Size(121, 24)
        Me.cmbIDField.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(18, 70)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(59, 17)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "ID Field:"
        '
        'btnImport
        '
        Me.btnImport.Location = New System.Drawing.Point(704, 243)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(84, 40)
        Me.btnImport.TabIndex = 5
        Me.btnImport.Text = "Import"
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.txtShapefile)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.btnShapefile)
        Me.GroupBox1.Controls.Add(Me.cmbIDField)
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(776, 100)
        Me.GroupBox1.TabIndex = 6
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Input"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.txtLonField)
        Me.GroupBox2.Controls.Add(Me.txtLatField)
        Me.GroupBox2.Controls.Add(Me.Label8)
        Me.GroupBox2.Controls.Add(Me.Label7)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.txtYField)
        Me.GroupBox2.Controls.Add(Me.Label5)
        Me.GroupBox2.Controls.Add(Me.txtXField)
        Me.GroupBox2.Controls.Add(Me.txtLocationsField)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.txtTableName)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 118)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(776, 100)
        Me.GroupBox2.TabIndex = 7
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Output"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(18, 34)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(83, 17)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Tablename:"
        '
        'txtTableName
        '
        Me.txtTableName.Location = New System.Drawing.Point(115, 31)
        Me.txtTableName.Name = "txtTableName"
        Me.txtTableName.Size = New System.Drawing.Size(171, 22)
        Me.txtTableName.TabIndex = 6
        Me.txtTableName.Text = "OUTPUTLOCATIONS"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(18, 63)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(59, 17)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "ID Field:"
        '
        'txtLocationsField
        '
        Me.txtLocationsField.Location = New System.Drawing.Point(115, 59)
        Me.txtLocationsField.Name = "txtLocationsField"
        Me.txtLocationsField.Size = New System.Drawing.Size(171, 22)
        Me.txtLocationsField.TabIndex = 8
        Me.txtLocationsField.Text = "LOCATIONID"
        '
        'txtXField
        '
        Me.txtXField.Location = New System.Drawing.Point(452, 31)
        Me.txtXField.Name = "txtXField"
        Me.txtXField.Size = New System.Drawing.Size(51, 22)
        Me.txtXField.TabIndex = 9
        Me.txtXField.Text = "X"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(302, 31)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(124, 17)
        Me.Label5.TabIndex = 10
        Me.Label5.Text = "X Coordinate field:"
        '
        'txtYField
        '
        Me.txtYField.Location = New System.Drawing.Point(452, 59)
        Me.txtYField.Name = "txtYField"
        Me.txtYField.Size = New System.Drawing.Size(51, 22)
        Me.txtYField.TabIndex = 11
        Me.txtYField.Text = "Y"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(302, 62)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(124, 17)
        Me.Label6.TabIndex = 12
        Me.Label6.Text = "Y Coordinate field:"
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(527, 34)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(93, 17)
        Me.Label7.TabIndex = 13
        Me.Label7.Text = "Latitude field:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(527, 59)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(105, 17)
        Me.Label8.TabIndex = 14
        Me.Label8.Text = "Longitude field:"
        '
        'txtLatField
        '
        Me.txtLatField.Location = New System.Drawing.Point(666, 31)
        Me.txtLatField.Name = "txtLatField"
        Me.txtLatField.Size = New System.Drawing.Size(51, 22)
        Me.txtLatField.TabIndex = 15
        Me.txtLatField.Text = "Lat"
        '
        'txtLonField
        '
        Me.txtLonField.Location = New System.Drawing.Point(666, 59)
        Me.txtLonField.Name = "txtLonField"
        Me.txtLonField.Size = New System.Drawing.Size(51, 22)
        Me.txtLonField.TabIndex = 16
        Me.txtLonField.Text = "Lon"
        '
        'frmLocationsFromShapefile
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 295)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnImport)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmLocationsFromShapefile"
        Me.Text = "Read locations from shapefile to database"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents txtShapefile As Windows.Forms.TextBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents btnShapefile As Windows.Forms.Button
    Friend WithEvents cmbIDField As Windows.Forms.ComboBox
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents btnImport As Windows.Forms.Button
    Friend WithEvents GroupBox1 As Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As Windows.Forms.GroupBox
    Friend WithEvents Label6 As Windows.Forms.Label
    Friend WithEvents txtYField As Windows.Forms.TextBox
    Friend WithEvents Label5 As Windows.Forms.Label
    Friend WithEvents txtXField As Windows.Forms.TextBox
    Friend WithEvents txtLocationsField As Windows.Forms.TextBox
    Friend WithEvents Label4 As Windows.Forms.Label
    Friend WithEvents txtTableName As Windows.Forms.TextBox
    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents txtLonField As Windows.Forms.TextBox
    Friend WithEvents txtLatField As Windows.Forms.TextBox
    Friend WithEvents Label8 As Windows.Forms.Label
    Friend WithEvents Label7 As Windows.Forms.Label
End Class
