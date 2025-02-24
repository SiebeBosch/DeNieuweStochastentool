﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDataTableFromTextFile
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDataTableFromTextFile))
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.btnRead = New System.Windows.Forms.Button()
        Me.btnImport = New System.Windows.Forms.Button()
        Me.txtCSVFile = New System.Windows.Forms.TextBox()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.grFields = New System.Windows.Forms.DataGridView()
        Me.colItem = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colDataType = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colUsage = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colDateFormatting = New System.Windows.Forms.DataGridViewTextBoxColumn()
        CType(Me.grFields, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(15, 441)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(65, 17)
        Me.lblProgress.TabIndex = 27
        Me.lblProgress.Text = "Progress"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(16, 465)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(879, 19)
        Me.prProgress.TabIndex = 26
        '
        'btnRead
        '
        Me.btnRead.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnRead.Location = New System.Drawing.Point(901, 433)
        Me.btnRead.Name = "btnRead"
        Me.btnRead.Size = New System.Drawing.Size(100, 51)
        Me.btnRead.TabIndex = 19
        Me.btnRead.Text = "Read"
        Me.btnRead.UseVisualStyleBackColor = True
        '
        'btnImport
        '
        Me.btnImport.Location = New System.Drawing.Point(971, 12)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(30, 23)
        Me.btnImport.TabIndex = 18
        Me.btnImport.Text = ".."
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'txtCSVFile
        '
        Me.txtCSVFile.Location = New System.Drawing.Point(12, 12)
        Me.txtCSVFile.Name = "txtCSVFile"
        Me.txtCSVFile.Size = New System.Drawing.Size(953, 22)
        Me.txtCSVFile.TabIndex = 17
        '
        'grFields
        '
        Me.grFields.AllowUserToAddRows = False
        Me.grFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grFields.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colItem, Me.colDataType, Me.colUsage, Me.colDateFormatting})
        Me.grFields.Location = New System.Drawing.Point(16, 40)
        Me.grFields.Name = "grFields"
        Me.grFields.RowHeadersWidth = 51
        Me.grFields.RowTemplate.Height = 24
        Me.grFields.Size = New System.Drawing.Size(985, 372)
        Me.grFields.TabIndex = 28
        '
        'colItem
        '
        Me.colItem.HeaderText = "Item"
        Me.colItem.MinimumWidth = 6
        Me.colItem.Name = "colItem"
        Me.colItem.Width = 120
        '
        'colDataType
        '
        Me.colDataType.HeaderText = "Datatype"
        Me.colDataType.MinimumWidth = 6
        Me.colDataType.Name = "colDataType"
        Me.colDataType.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colDataType.Width = 120
        '
        'colUsage
        '
        Me.colUsage.HeaderText = "CSV Field"
        Me.colUsage.MinimumWidth = 6
        Me.colUsage.Name = "colUsage"
        Me.colUsage.Width = 120
        '
        'colDateFormatting
        '
        Me.colDateFormatting.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colDateFormatting.HeaderText = "Date formatting"
        Me.colDateFormatting.MinimumWidth = 6
        Me.colDateFormatting.Name = "colDateFormatting"
        '
        'frmDataTableFromTextFile
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1013, 496)
        Me.Controls.Add(Me.grFields)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnRead)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.txtCSVFile)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmDataTableFromTextFile"
        Me.Text = "Datatable from CSV"
        CType(Me.grFields, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents btnRead As Windows.Forms.Button
    Friend WithEvents btnImport As Windows.Forms.Button
    Friend WithEvents txtCSVFile As Windows.Forms.TextBox
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents grFields As Windows.Forms.DataGridView
    Friend WithEvents colItem As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colDataType As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colUsage As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colDateFormatting As Windows.Forms.DataGridViewTextBoxColumn
End Class
