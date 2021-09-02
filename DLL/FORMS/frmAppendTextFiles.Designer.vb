<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAppendTextFiles
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAppendTextFiles))
        Me.grFiles = New System.Windows.Forms.DataGridView()
        Me.colPath = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.btnExecute = New System.Windows.Forms.Button()
        Me.dlgSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.Options = New System.Windows.Forms.GroupBox()
        Me.chkSkipEmptyLines = New System.Windows.Forms.CheckBox()
        CType(Me.grFiles, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Options.SuspendLayout()
        Me.SuspendLayout()
        '
        'grFiles
        '
        Me.grFiles.AllowUserToAddRows = False
        Me.grFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grFiles.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colPath})
        Me.grFiles.Location = New System.Drawing.Point(12, 12)
        Me.grFiles.Name = "grFiles"
        Me.grFiles.RowHeadersWidth = 51
        Me.grFiles.RowTemplate.Height = 24
        Me.grFiles.Size = New System.Drawing.Size(507, 321)
        Me.grFiles.TabIndex = 0
        '
        'colPath
        '
        Me.colPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colPath.HeaderText = "Path"
        Me.colPath.MinimumWidth = 6
        Me.colPath.Name = "colPath"
        '
        'btnAdd
        '
        Me.btnAdd.Location = New System.Drawing.Point(525, 12)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(27, 27)
        Me.btnAdd.TabIndex = 1
        Me.btnAdd.Text = "+"
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(12, 379)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(671, 23)
        Me.prProgress.TabIndex = 2
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(12, 347)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(69, 17)
        Me.lblProgress.TabIndex = 3
        Me.lblProgress.Text = "Progress:"
        '
        'btnExecute
        '
        Me.btnExecute.Location = New System.Drawing.Point(689, 359)
        Me.btnExecute.Name = "btnExecute"
        Me.btnExecute.Size = New System.Drawing.Size(100, 43)
        Me.btnExecute.TabIndex = 4
        Me.btnExecute.Text = "Execute"
        Me.btnExecute.UseVisualStyleBackColor = True
        '
        'Options
        '
        Me.Options.Controls.Add(Me.chkSkipEmptyLines)
        Me.Options.Location = New System.Drawing.Point(568, 12)
        Me.Options.Name = "Options"
        Me.Options.Size = New System.Drawing.Size(220, 321)
        Me.Options.TabIndex = 5
        Me.Options.TabStop = False
        Me.Options.Text = "Options"
        '
        'chkSkipEmptyLines
        '
        Me.chkSkipEmptyLines.AutoSize = True
        Me.chkSkipEmptyLines.Checked = True
        Me.chkSkipEmptyLines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSkipEmptyLines.Location = New System.Drawing.Point(25, 36)
        Me.chkSkipEmptyLines.Name = "chkSkipEmptyLines"
        Me.chkSkipEmptyLines.Size = New System.Drawing.Size(132, 21)
        Me.chkSkipEmptyLines.TabIndex = 0
        Me.chkSkipEmptyLines.Text = "Skip empty lines"
        Me.chkSkipEmptyLines.UseVisualStyleBackColor = True
        '
        'frmAppendTextFiles
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.Options)
        Me.Controls.Add(Me.btnExecute)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnAdd)
        Me.Controls.Add(Me.grFiles)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmAppendTextFiles"
        Me.Text = "Append text files"
        CType(Me.grFiles, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Options.ResumeLayout(False)
        Me.Options.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents grFiles As Windows.Forms.DataGridView
    Friend WithEvents btnAdd As Windows.Forms.Button
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents colPath As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents btnExecute As Windows.Forms.Button
    Friend WithEvents dlgSaveFile As Windows.Forms.SaveFileDialog
    Friend WithEvents Options As Windows.Forms.GroupBox
    Friend WithEvents chkSkipEmptyLines As Windows.Forms.CheckBox
End Class
