<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmIDMapping
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmIDMapping))
        Me.grIDMapping = New System.Windows.Forms.DataGridView()
        Me.btnOk = New System.Windows.Forms.Button()
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.btnRemove = New System.Windows.Forms.Button()
        Me.colMeasID = New System.Windows.Forms.DataGridViewComboBoxColumn()
        Me.colModelID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        CType(Me.grIDMapping, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'grIDMapping
        '
        Me.grIDMapping.AllowUserToAddRows = False
        Me.grIDMapping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.grIDMapping.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colMeasID, Me.colModelID})
        Me.grIDMapping.Location = New System.Drawing.Point(12, 12)
        Me.grIDMapping.Name = "grIDMapping"
        Me.grIDMapping.RowTemplate.Height = 24
        Me.grIDMapping.Size = New System.Drawing.Size(738, 357)
        Me.grIDMapping.TabIndex = 0
        '
        'btnOk
        '
        Me.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnOk.Location = New System.Drawing.Point(673, 386)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(115, 52)
        Me.btnOk.TabIndex = 1
        Me.btnOk.Text = "Ok"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'btnAdd
        '
        Me.btnAdd.Location = New System.Drawing.Point(764, 12)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(24, 23)
        Me.btnAdd.TabIndex = 2
        Me.btnAdd.Text = "+"
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'btnRemove
        '
        Me.btnRemove.Location = New System.Drawing.Point(764, 41)
        Me.btnRemove.Name = "btnRemove"
        Me.btnRemove.Size = New System.Drawing.Size(24, 23)
        Me.btnRemove.TabIndex = 3
        Me.btnRemove.Text = "-"
        Me.btnRemove.UseVisualStyleBackColor = True
        '
        'colMeasID
        '
        Me.colMeasID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colMeasID.HeaderText = "Measurement location ID"
        Me.colMeasID.Name = "colMeasID"
        Me.colMeasID.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colMeasID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'colModelID
        '
        Me.colModelID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.colModelID.HeaderText = "Model location ID"
        Me.colModelID.Name = "colModelID"
        Me.colModelID.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.colModelID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(12, 414)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(655, 23)
        Me.prProgress.TabIndex = 4
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(12, 386)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(65, 17)
        Me.lblProgress.TabIndex = 5
        Me.lblProgress.Text = "Progress"
        '
        'frmIDMapping
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnRemove)
        Me.Controls.Add(Me.btnAdd)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.grIDMapping)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmIDMapping"
        Me.Text = "ID Mapping"
        CType(Me.grIDMapping, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents grIDMapping As Windows.Forms.DataGridView
    Friend WithEvents btnOk As Windows.Forms.Button
    Friend WithEvents btnAdd As Windows.Forms.Button
    Friend WithEvents btnRemove As Windows.Forms.Button
    Friend WithEvents colMeasID As Windows.Forms.DataGridViewComboBoxColumn
    Friend WithEvents colModelID As Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
End Class
