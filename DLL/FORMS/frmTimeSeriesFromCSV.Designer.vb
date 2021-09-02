<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTimeSeriesFromCSV
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTimeSeriesFromCSV))
        Me.txtCSVFile = New System.Windows.Forms.TextBox()
        Me.btnImport = New System.Windows.Forms.Button()
        Me.btnRead = New System.Windows.Forms.Button()
        Me.cmbDateCol = New System.Windows.Forms.ComboBox()
        Me.cmbValCol = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtDateFormatting = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.cmbIDCol = New System.Windows.Forms.ComboBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.btnDateFormatting = New System.Windows.Forms.Button()
        Me.chkRemoveExisting = New System.Windows.Forms.CheckBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cmbPeriodicity = New System.Windows.Forms.ComboBox()
        Me.txtMultiplier = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'txtCSVFile
        '
        Me.txtCSVFile.Location = New System.Drawing.Point(12, 12)
        Me.txtCSVFile.Name = "txtCSVFile"
        Me.txtCSVFile.Size = New System.Drawing.Size(465, 22)
        Me.txtCSVFile.TabIndex = 0
        '
        'btnImport
        '
        Me.btnImport.Location = New System.Drawing.Point(483, 11)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(30, 23)
        Me.btnImport.TabIndex = 1
        Me.btnImport.Text = ".."
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'btnRead
        '
        Me.btnRead.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnRead.Location = New System.Drawing.Point(438, 231)
        Me.btnRead.Name = "btnRead"
        Me.btnRead.Size = New System.Drawing.Size(75, 39)
        Me.btnRead.TabIndex = 2
        Me.btnRead.Text = "Read"
        Me.btnRead.UseVisualStyleBackColor = True
        '
        'cmbDateCol
        '
        Me.cmbDateCol.FormattingEnabled = True
        Me.cmbDateCol.Location = New System.Drawing.Point(141, 73)
        Me.cmbDateCol.Name = "cmbDateCol"
        Me.cmbDateCol.Size = New System.Drawing.Size(161, 24)
        Me.cmbDateCol.TabIndex = 4
        '
        'cmbValCol
        '
        Me.cmbValCol.FormattingEnabled = True
        Me.cmbValCol.Location = New System.Drawing.Point(141, 103)
        Me.cmbValCol.Name = "cmbValCol"
        Me.cmbValCol.Size = New System.Drawing.Size(161, 24)
        Me.cmbValCol.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(9, 76)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(87, 17)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "Date column"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(9, 106)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(93, 17)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "Value column"
        '
        'txtDateFormatting
        '
        Me.txtDateFormatting.Location = New System.Drawing.Point(141, 134)
        Me.txtDateFormatting.Name = "txtDateFormatting"
        Me.txtDateFormatting.Size = New System.Drawing.Size(161, 22)
        Me.txtDateFormatting.TabIndex = 8
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(9, 137)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(109, 17)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "Date formatting:"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(14, 251)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(415, 19)
        Me.prProgress.TabIndex = 12
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(13, 227)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(65, 17)
        Me.lblProgress.TabIndex = 13
        Me.lblProgress.Text = "Progress"
        '
        'cmbIDCol
        '
        Me.cmbIDCol.FormattingEnabled = True
        Me.cmbIDCol.Location = New System.Drawing.Point(141, 43)
        Me.cmbIDCol.Name = "cmbIDCol"
        Me.cmbIDCol.Size = New System.Drawing.Size(161, 24)
        Me.cmbIDCol.TabIndex = 14
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(9, 47)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(70, 17)
        Me.Label4.TabIndex = 15
        Me.Label4.Text = "ID column"
        '
        'btnDateFormatting
        '
        Me.btnDateFormatting.BackColor = System.Drawing.Color.Gold
        Me.btnDateFormatting.Location = New System.Drawing.Point(308, 134)
        Me.btnDateFormatting.Name = "btnDateFormatting"
        Me.btnDateFormatting.Size = New System.Drawing.Size(24, 22)
        Me.btnDateFormatting.TabIndex = 16
        Me.btnDateFormatting.Text = "?"
        Me.btnDateFormatting.UseVisualStyleBackColor = False
        '
        'chkRemoveExisting
        '
        Me.chkRemoveExisting.AutoSize = True
        Me.chkRemoveExisting.Location = New System.Drawing.Point(308, 46)
        Me.chkRemoveExisting.Name = "chkRemoveExisting"
        Me.chkRemoveExisting.Size = New System.Drawing.Size(185, 21)
        Me.chkRemoveExisting.TabIndex = 17
        Me.chkRemoveExisting.Text = "Remove existing records"
        Me.chkRemoveExisting.UseVisualStyleBackColor = True
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(9, 165)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(77, 17)
        Me.Label5.TabIndex = 21
        Me.Label5.Text = "Periodicity:"
        '
        'cmbPeriodicity
        '
        Me.cmbPeriodicity.FormattingEnabled = True
        Me.cmbPeriodicity.Location = New System.Drawing.Point(141, 165)
        Me.cmbPeriodicity.Name = "cmbPeriodicity"
        Me.cmbPeriodicity.Size = New System.Drawing.Size(161, 24)
        Me.cmbPeriodicity.TabIndex = 22
        '
        'txtMultiplier
        '
        Me.txtMultiplier.Location = New System.Drawing.Point(382, 106)
        Me.txtMultiplier.Name = "txtMultiplier"
        Me.txtMultiplier.Size = New System.Drawing.Size(95, 22)
        Me.txtMultiplier.TabIndex = 23
        Me.txtMultiplier.Text = "1"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(308, 109)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(68, 17)
        Me.Label6.TabIndex = 24
        Me.Label6.Text = "Multiplier:"
        '
        'frmTimeSeriesFromCSV
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(525, 282)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtMultiplier)
        Me.Controls.Add(Me.cmbPeriodicity)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.chkRemoveExisting)
        Me.Controls.Add(Me.btnDateFormatting)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.cmbIDCol)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtDateFormatting)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.cmbValCol)
        Me.Controls.Add(Me.cmbDateCol)
        Me.Controls.Add(Me.btnRead)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.txtCSVFile)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmTimeSeriesFromCSV"
        Me.Text = "Import time series from CSV"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtCSVFile As Windows.Forms.TextBox
    Friend WithEvents btnImport As Windows.Forms.Button
    Friend WithEvents btnRead As Windows.Forms.Button
    Friend WithEvents cmbDateCol As Windows.Forms.ComboBox
    Friend WithEvents cmbValCol As Windows.Forms.ComboBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents txtDateFormatting As Windows.Forms.TextBox
    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents cmbIDCol As Windows.Forms.ComboBox
    Friend WithEvents Label4 As Windows.Forms.Label
    Friend WithEvents btnDateFormatting As Windows.Forms.Button
    Friend WithEvents chkRemoveExisting As Windows.Forms.CheckBox
    Friend WithEvents Label5 As Windows.Forms.Label
    Friend WithEvents cmbPeriodicity As Windows.Forms.ComboBox
    Friend WithEvents txtMultiplier As Windows.Forms.TextBox
    Friend WithEvents Label6 As Windows.Forms.Label
End Class
