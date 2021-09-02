<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmReadTextFileGeneric
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmReadTextFileGeneric))
        Me.grFields = New System.Windows.Forms.DataGridView()
        Me.colField = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colDataType = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colTextFileField = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtTextFile = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnReadTextfile = New System.Windows.Forms.Button()
        Me.btnRead = New System.Windows.Forms.Button()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.grSettings = New System.Windows.Forms.GroupBox()
        Me.chkRemoveBoundingQuotes = New System.Windows.Forms.CheckBox()
        CType(Me.grFields, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grSettings.SuspendLayout()
        Me.SuspendLayout()
        '
        'grFields
        '
        Me.grFields.AllowUserToAddRows = False
        Me.grFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grFields.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colField, Me.colDataType, Me.colTextFileField})
        Me.grFields.Location = New System.Drawing.Point(12, 60)
        Me.grFields.Name = "grFields"
        Me.grFields.RowTemplate.Height = 24
        Me.grFields.Size = New System.Drawing.Size(549, 311)
        Me.grFields.TabIndex = 0
        '
        'colField
        '
        Me.colField.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colField.HeaderText = "Field"
        Me.colField.Name = "colField"
        '
        'colDataType
        '
        Me.colDataType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colDataType.HeaderText = "Datatype"
        Me.colDataType.Name = "colDataType"
        Me.colDataType.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colDataType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'colTextFileField
        '
        Me.colTextFileField.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colTextFileField.HeaderText = "Textfile field"
        Me.colTextFileField.Name = "colTextFileField"
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(12, 415)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(801, 23)
        Me.ProgressBar1.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(9, 386)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(65, 17)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Progress"
        '
        'txtTextFile
        '
        Me.txtTextFile.Location = New System.Drawing.Point(90, 22)
        Me.txtTextFile.Name = "txtTextFile"
        Me.txtTextFile.Size = New System.Drawing.Size(795, 22)
        Me.txtTextFile.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 25)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(61, 17)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Text file:"
        '
        'btnReadTextfile
        '
        Me.btnReadTextfile.Location = New System.Drawing.Point(891, 22)
        Me.btnReadTextfile.Name = "btnReadTextfile"
        Me.btnReadTextfile.Size = New System.Drawing.Size(28, 26)
        Me.btnReadTextfile.TabIndex = 5
        Me.btnReadTextfile.Text = ".."
        Me.btnReadTextfile.UseVisualStyleBackColor = True
        '
        'btnRead
        '
        Me.btnRead.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnRead.Location = New System.Drawing.Point(819, 386)
        Me.btnRead.Name = "btnRead"
        Me.btnRead.Size = New System.Drawing.Size(100, 52)
        Me.btnRead.TabIndex = 6
        Me.btnRead.Text = "Read"
        Me.btnRead.UseVisualStyleBackColor = True
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        '
        'grSettings
        '
        Me.grSettings.Controls.Add(Me.chkRemoveBoundingQuotes)
        Me.grSettings.Location = New System.Drawing.Point(567, 60)
        Me.grSettings.Name = "grSettings"
        Me.grSettings.Size = New System.Drawing.Size(352, 311)
        Me.grSettings.TabIndex = 7
        Me.grSettings.TabStop = False
        Me.grSettings.Text = "Settings"
        '
        'chkRemoveBoundingQuotes
        '
        Me.chkRemoveBoundingQuotes.AutoSize = True
        Me.chkRemoveBoundingQuotes.Checked = True
        Me.chkRemoveBoundingQuotes.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkRemoveBoundingQuotes.Location = New System.Drawing.Point(10, 31)
        Me.chkRemoveBoundingQuotes.Name = "chkRemoveBoundingQuotes"
        Me.chkRemoveBoundingQuotes.Size = New System.Drawing.Size(308, 21)
        Me.chkRemoveBoundingQuotes.TabIndex = 0
        Me.chkRemoveBoundingQuotes.Text = "Remove bounding quotes from string values"
        Me.chkRemoveBoundingQuotes.UseVisualStyleBackColor = True
        '
        'frmReadTextFileGeneric
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(937, 450)
        Me.Controls.Add(Me.grSettings)
        Me.Controls.Add(Me.btnRead)
        Me.Controls.Add(Me.btnReadTextfile)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtTextFile)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.ProgressBar1)
        Me.Controls.Add(Me.grFields)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmReadTextFileGeneric"
        Me.Text = "Read data from a text file"
        CType(Me.grFields, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grSettings.ResumeLayout(False)
        Me.grSettings.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents grFields As Windows.Forms.DataGridView
    Friend WithEvents colField As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colDataType As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colTextFileField As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents ProgressBar1 As Windows.Forms.ProgressBar
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents txtTextFile As Windows.Forms.TextBox
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents btnReadTextfile As Windows.Forms.Button
    Friend WithEvents btnRead As Windows.Forms.Button
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents grSettings As Windows.Forms.GroupBox
    Friend WithEvents chkRemoveBoundingQuotes As Windows.Forms.CheckBox
End Class
