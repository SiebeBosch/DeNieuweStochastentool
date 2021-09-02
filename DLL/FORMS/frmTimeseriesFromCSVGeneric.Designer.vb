<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmTimeseriesFromCSVGeneric
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTimeseriesFromCSVGeneric))
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtDateFormatting = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbValCol = New System.Windows.Forms.ComboBox()
        Me.cmbDateCol = New System.Windows.Forms.ComboBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.btnImport = New System.Windows.Forms.Button()
        Me.txtCSVFile = New System.Windows.Forms.TextBox()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.btnDateFormatting = New System.Windows.Forms.Button()
        Me.cmbIDCol = New System.Windows.Forms.ComboBox()
        Me.cmbParCol = New System.Windows.Forms.ComboBox()
        Me.radIDByColumn = New System.Windows.Forms.RadioButton()
        Me.radCustomID = New System.Windows.Forms.RadioButton()
        Me.txtID = New System.Windows.Forms.TextBox()
        Me.grLocationID = New System.Windows.Forms.GroupBox()
        Me.grParameter = New System.Windows.Forms.GroupBox()
        Me.txtPar = New System.Windows.Forms.TextBox()
        Me.radCustomPar = New System.Windows.Forms.RadioButton()
        Me.radParByColumn = New System.Windows.Forms.RadioButton()
        Me.grDates = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtTimestepSizeSeconds = New System.Windows.Forms.TextBox()
        Me.pckDate = New System.Windows.Forms.DateTimePicker()
        Me.radCustomStartDate = New System.Windows.Forms.RadioButton()
        Me.radDateByColumn = New System.Windows.Forms.RadioButton()
        Me.grValues = New System.Windows.Forms.GroupBox()
        Me.grOptions = New System.Windows.Forms.GroupBox()
        Me.chkClearExisting = New System.Windows.Forms.CheckBox()
        Me.txtSeriesID = New System.Windows.Forms.TextBox()
        Me.grSeriesID = New System.Windows.Forms.GroupBox()
        Me.cmbSeriesIDCol = New System.Windows.Forms.ComboBox()
        Me.radCustomSeriesID = New System.Windows.Forms.RadioButton()
        Me.radSeriesIDbyColumn = New System.Windows.Forms.RadioButton()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.grLocationID.SuspendLayout()
        Me.grParameter.SuspendLayout()
        Me.grDates.SuspendLayout()
        Me.grValues.SuspendLayout()
        Me.grOptions.SuspendLayout()
        Me.grSeriesID.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(36, 54)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(109, 17)
        Me.Label3.TabIndex = 26
        Me.Label3.Text = "Date formatting:"
        '
        'txtDateFormatting
        '
        Me.txtDateFormatting.Location = New System.Drawing.Point(261, 51)
        Me.txtDateFormatting.Name = "txtDateFormatting"
        Me.txtDateFormatting.Size = New System.Drawing.Size(226, 22)
        Me.txtDateFormatting.TabIndex = 25
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(8, 24)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(100, 17)
        Me.Label2.TabIndex = 24
        Me.Label2.Text = "Values column"
        '
        'cmbValCol
        '
        Me.cmbValCol.FormattingEnabled = True
        Me.cmbValCol.Location = New System.Drawing.Point(353, 21)
        Me.cmbValCol.Name = "cmbValCol"
        Me.cmbValCol.Size = New System.Drawing.Size(161, 24)
        Me.cmbValCol.TabIndex = 22
        '
        'cmbDateCol
        '
        Me.cmbDateCol.FormattingEnabled = True
        Me.cmbDateCol.Location = New System.Drawing.Point(261, 21)
        Me.cmbDateCol.Name = "cmbDateCol"
        Me.cmbDateCol.Size = New System.Drawing.Size(252, 24)
        Me.cmbDateCol.TabIndex = 21
        '
        'btnOK
        '
        Me.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnOK.Location = New System.Drawing.Point(964, 423)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(107, 61)
        Me.btnOK.TabIndex = 20
        Me.btnOK.Text = "Import"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'btnImport
        '
        Me.btnImport.Location = New System.Drawing.Point(1041, 12)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(30, 23)
        Me.btnImport.TabIndex = 19
        Me.btnImport.Text = ".."
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'txtCSVFile
        '
        Me.txtCSVFile.Location = New System.Drawing.Point(12, 13)
        Me.txtCSVFile.Name = "txtCSVFile"
        Me.txtCSVFile.Size = New System.Drawing.Size(1023, 22)
        Me.txtCSVFile.TabIndex = 18
        '
        'btnDateFormatting
        '
        Me.btnDateFormatting.BackColor = System.Drawing.Color.Gold
        Me.btnDateFormatting.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnDateFormatting.Location = New System.Drawing.Point(493, 51)
        Me.btnDateFormatting.Name = "btnDateFormatting"
        Me.btnDateFormatting.Size = New System.Drawing.Size(19, 22)
        Me.btnDateFormatting.TabIndex = 31
        Me.btnDateFormatting.Text = "?"
        Me.btnDateFormatting.UseVisualStyleBackColor = False
        '
        'cmbIDCol
        '
        Me.cmbIDCol.FormattingEnabled = True
        Me.cmbIDCol.Location = New System.Drawing.Point(261, 23)
        Me.cmbIDCol.Name = "cmbIDCol"
        Me.cmbIDCol.Size = New System.Drawing.Size(249, 24)
        Me.cmbIDCol.TabIndex = 29
        '
        'cmbParCol
        '
        Me.cmbParCol.FormattingEnabled = True
        Me.cmbParCol.Location = New System.Drawing.Point(349, 29)
        Me.cmbParCol.Name = "cmbParCol"
        Me.cmbParCol.Size = New System.Drawing.Size(165, 24)
        Me.cmbParCol.TabIndex = 32
        '
        'radIDByColumn
        '
        Me.radIDByColumn.AutoSize = True
        Me.radIDByColumn.Checked = True
        Me.radIDByColumn.Location = New System.Drawing.Point(11, 26)
        Me.radIDByColumn.Name = "radIDByColumn"
        Me.radIDByColumn.Size = New System.Drawing.Size(168, 21)
        Me.radIDByColumn.TabIndex = 36
        Me.radIDByColumn.TabStop = True
        Me.radIDByColumn.Text = "Location ID by column"
        Me.radIDByColumn.UseVisualStyleBackColor = True
        '
        'radCustomID
        '
        Me.radCustomID.AutoSize = True
        Me.radCustomID.Location = New System.Drawing.Point(11, 54)
        Me.radCustomID.Name = "radCustomID"
        Me.radCustomID.Size = New System.Drawing.Size(146, 21)
        Me.radCustomID.TabIndex = 37
        Me.radCustomID.Text = "Custom location ID"
        Me.radCustomID.UseVisualStyleBackColor = True
        '
        'txtID
        '
        Me.txtID.Location = New System.Drawing.Point(261, 53)
        Me.txtID.Name = "txtID"
        Me.txtID.Size = New System.Drawing.Size(249, 22)
        Me.txtID.TabIndex = 38
        '
        'grLocationID
        '
        Me.grLocationID.Controls.Add(Me.cmbIDCol)
        Me.grLocationID.Controls.Add(Me.txtID)
        Me.grLocationID.Controls.Add(Me.radIDByColumn)
        Me.grLocationID.Controls.Add(Me.radCustomID)
        Me.grLocationID.Location = New System.Drawing.Point(10, 147)
        Me.grLocationID.Name = "grLocationID"
        Me.grLocationID.Size = New System.Drawing.Size(527, 106)
        Me.grLocationID.TabIndex = 39
        Me.grLocationID.TabStop = False
        Me.grLocationID.Text = "Location ID's"
        '
        'grParameter
        '
        Me.grParameter.Controls.Add(Me.txtPar)
        Me.grParameter.Controls.Add(Me.radCustomPar)
        Me.grParameter.Controls.Add(Me.radParByColumn)
        Me.grParameter.Controls.Add(Me.cmbParCol)
        Me.grParameter.Location = New System.Drawing.Point(545, 43)
        Me.grParameter.Name = "grParameter"
        Me.grParameter.Size = New System.Drawing.Size(527, 106)
        Me.grParameter.TabIndex = 40
        Me.grParameter.TabStop = False
        Me.grParameter.Text = "Parameters"
        '
        'txtPar
        '
        Me.txtPar.Location = New System.Drawing.Point(349, 59)
        Me.txtPar.Name = "txtPar"
        Me.txtPar.Size = New System.Drawing.Size(165, 22)
        Me.txtPar.TabIndex = 39
        '
        'radCustomPar
        '
        Me.radCustomPar.AutoSize = True
        Me.radCustomPar.Location = New System.Drawing.Point(11, 60)
        Me.radCustomPar.Name = "radCustomPar"
        Me.radCustomPar.Size = New System.Drawing.Size(145, 21)
        Me.radCustomPar.TabIndex = 40
        Me.radCustomPar.Text = "Custom parameter"
        Me.radCustomPar.UseVisualStyleBackColor = True
        '
        'radParByColumn
        '
        Me.radParByColumn.AutoSize = True
        Me.radParByColumn.Checked = True
        Me.radParByColumn.Location = New System.Drawing.Point(11, 30)
        Me.radParByColumn.Name = "radParByColumn"
        Me.radParByColumn.Size = New System.Drawing.Size(163, 21)
        Me.radParByColumn.TabIndex = 39
        Me.radParByColumn.TabStop = True
        Me.radParByColumn.Text = "Parameter by column"
        Me.radParByColumn.UseVisualStyleBackColor = True
        '
        'grDates
        '
        Me.grDates.Controls.Add(Me.Label1)
        Me.grDates.Controls.Add(Me.txtTimestepSizeSeconds)
        Me.grDates.Controls.Add(Me.pckDate)
        Me.grDates.Controls.Add(Me.radCustomStartDate)
        Me.grDates.Controls.Add(Me.radDateByColumn)
        Me.grDates.Controls.Add(Me.cmbDateCol)
        Me.grDates.Controls.Add(Me.Label3)
        Me.grDates.Controls.Add(Me.txtDateFormatting)
        Me.grDates.Controls.Add(Me.btnDateFormatting)
        Me.grDates.Location = New System.Drawing.Point(10, 259)
        Me.grDates.Name = "grDates"
        Me.grDates.Size = New System.Drawing.Size(527, 158)
        Me.grDates.TabIndex = 41
        Me.grDates.TabStop = False
        Me.grDates.Text = "Dates"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(41, 112)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(166, 17)
        Me.Label1.TabIndex = 44
        Me.Label1.Text = "Timestep size (seconds):"
        '
        'txtTimestepSizeSeconds
        '
        Me.txtTimestepSizeSeconds.Location = New System.Drawing.Point(261, 112)
        Me.txtTimestepSizeSeconds.Name = "txtTimestepSizeSeconds"
        Me.txtTimestepSizeSeconds.Size = New System.Drawing.Size(252, 22)
        Me.txtTimestepSizeSeconds.TabIndex = 43
        '
        'pckDate
        '
        Me.pckDate.CustomFormat = ""
        Me.pckDate.Location = New System.Drawing.Point(261, 84)
        Me.pckDate.Name = "pckDate"
        Me.pckDate.Size = New System.Drawing.Size(252, 22)
        Me.pckDate.TabIndex = 42
        Me.pckDate.Value = New Date(2019, 4, 30, 12, 40, 55, 0)
        '
        'radCustomStartDate
        '
        Me.radCustomStartDate.AutoSize = True
        Me.radCustomStartDate.Location = New System.Drawing.Point(10, 83)
        Me.radCustomStartDate.Name = "radCustomStartDate"
        Me.radCustomStartDate.Size = New System.Drawing.Size(140, 21)
        Me.radCustomStartDate.TabIndex = 41
        Me.radCustomStartDate.Text = "Custom start date"
        Me.radCustomStartDate.UseVisualStyleBackColor = True
        '
        'radDateByColumn
        '
        Me.radDateByColumn.AutoSize = True
        Me.radDateByColumn.Checked = True
        Me.radDateByColumn.Location = New System.Drawing.Point(11, 24)
        Me.radDateByColumn.Name = "radDateByColumn"
        Me.radDateByColumn.Size = New System.Drawing.Size(134, 21)
        Me.radDateByColumn.TabIndex = 41
        Me.radDateByColumn.TabStop = True
        Me.radDateByColumn.Text = "Dates by column"
        Me.radDateByColumn.UseVisualStyleBackColor = True
        '
        'grValues
        '
        Me.grValues.Controls.Add(Me.cmbValCol)
        Me.grValues.Controls.Add(Me.Label2)
        Me.grValues.Location = New System.Drawing.Point(545, 155)
        Me.grValues.Name = "grValues"
        Me.grValues.Size = New System.Drawing.Size(527, 98)
        Me.grValues.TabIndex = 42
        Me.grValues.TabStop = False
        Me.grValues.Text = "Values"
        '
        'grOptions
        '
        Me.grOptions.Controls.Add(Me.chkClearExisting)
        Me.grOptions.Location = New System.Drawing.Point(545, 259)
        Me.grOptions.Name = "grOptions"
        Me.grOptions.Size = New System.Drawing.Size(527, 158)
        Me.grOptions.TabIndex = 43
        Me.grOptions.TabStop = False
        Me.grOptions.Text = "Options"
        '
        'chkClearExisting
        '
        Me.chkClearExisting.AutoSize = True
        Me.chkClearExisting.Location = New System.Drawing.Point(11, 39)
        Me.chkClearExisting.Name = "chkClearExisting"
        Me.chkClearExisting.Size = New System.Drawing.Size(338, 21)
        Me.chkClearExisting.TabIndex = 43
        Me.chkClearExisting.Text = "Clear existing series with same ID from database."
        Me.chkClearExisting.UseVisualStyleBackColor = True
        '
        'txtSeriesID
        '
        Me.txtSeriesID.Location = New System.Drawing.Point(261, 52)
        Me.txtSeriesID.Name = "txtSeriesID"
        Me.txtSeriesID.Size = New System.Drawing.Size(249, 22)
        Me.txtSeriesID.TabIndex = 39
        '
        'grSeriesID
        '
        Me.grSeriesID.Controls.Add(Me.txtSeriesID)
        Me.grSeriesID.Controls.Add(Me.cmbSeriesIDCol)
        Me.grSeriesID.Controls.Add(Me.radCustomSeriesID)
        Me.grSeriesID.Controls.Add(Me.radSeriesIDbyColumn)
        Me.grSeriesID.Location = New System.Drawing.Point(10, 41)
        Me.grSeriesID.Name = "grSeriesID"
        Me.grSeriesID.Size = New System.Drawing.Size(529, 100)
        Me.grSeriesID.TabIndex = 44
        Me.grSeriesID.TabStop = False
        Me.grSeriesID.Text = "Series ID"
        '
        'cmbSeriesIDCol
        '
        Me.cmbSeriesIDCol.FormattingEnabled = True
        Me.cmbSeriesIDCol.Location = New System.Drawing.Point(261, 22)
        Me.cmbSeriesIDCol.Name = "cmbSeriesIDCol"
        Me.cmbSeriesIDCol.Size = New System.Drawing.Size(249, 24)
        Me.cmbSeriesIDCol.TabIndex = 39
        '
        'radCustomSeriesID
        '
        Me.radCustomSeriesID.AutoSize = True
        Me.radCustomSeriesID.Location = New System.Drawing.Point(11, 50)
        Me.radCustomSeriesID.Name = "radCustomSeriesID"
        Me.radCustomSeriesID.Size = New System.Drawing.Size(137, 21)
        Me.radCustomSeriesID.TabIndex = 1
        Me.radCustomSeriesID.Text = "Custom Series ID"
        Me.radCustomSeriesID.UseVisualStyleBackColor = True
        '
        'radSeriesIDbyColumn
        '
        Me.radSeriesIDbyColumn.AutoSize = True
        Me.radSeriesIDbyColumn.Checked = True
        Me.radSeriesIDbyColumn.Location = New System.Drawing.Point(11, 23)
        Me.radSeriesIDbyColumn.Name = "radSeriesIDbyColumn"
        Me.radSeriesIDbyColumn.Size = New System.Drawing.Size(154, 21)
        Me.radSeriesIDbyColumn.TabIndex = 0
        Me.radSeriesIDbyColumn.TabStop = True
        Me.radSeriesIDbyColumn.Text = "Series ID by column"
        Me.radSeriesIDbyColumn.UseVisualStyleBackColor = True
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(12, 460)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(946, 23)
        Me.prProgress.TabIndex = 45
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(17, 430)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(65, 17)
        Me.lblProgress.TabIndex = 46
        Me.lblProgress.Text = "Progress"
        '
        'frmTimeseriesFromCSVGeneric
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1083, 495)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.grSeriesID)
        Me.Controls.Add(Me.grOptions)
        Me.Controls.Add(Me.grValues)
        Me.Controls.Add(Me.grDates)
        Me.Controls.Add(Me.grParameter)
        Me.Controls.Add(Me.grLocationID)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.txtCSVFile)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmTimeseriesFromCSVGeneric"
        Me.Text = "Timeseries from text file"
        Me.grLocationID.ResumeLayout(False)
        Me.grLocationID.PerformLayout()
        Me.grParameter.ResumeLayout(False)
        Me.grParameter.PerformLayout()
        Me.grDates.ResumeLayout(False)
        Me.grDates.PerformLayout()
        Me.grValues.ResumeLayout(False)
        Me.grValues.PerformLayout()
        Me.grOptions.ResumeLayout(False)
        Me.grOptions.PerformLayout()
        Me.grSeriesID.ResumeLayout(False)
        Me.grSeriesID.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents txtDateFormatting As Windows.Forms.TextBox
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents cmbValCol As Windows.Forms.ComboBox
    Friend WithEvents cmbDateCol As Windows.Forms.ComboBox
    Friend WithEvents btnOK As Windows.Forms.Button
    Friend WithEvents btnImport As Windows.Forms.Button
    Friend WithEvents txtCSVFile As Windows.Forms.TextBox
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents btnDateFormatting As Windows.Forms.Button
    Friend WithEvents cmbIDCol As Windows.Forms.ComboBox
    Friend WithEvents cmbParCol As Windows.Forms.ComboBox
    Friend WithEvents radIDByColumn As Windows.Forms.RadioButton
    Friend WithEvents radCustomID As Windows.Forms.RadioButton
    Friend WithEvents txtID As Windows.Forms.TextBox
    Friend WithEvents grLocationID As Windows.Forms.GroupBox
    Friend WithEvents grParameter As Windows.Forms.GroupBox
    Friend WithEvents txtPar As Windows.Forms.TextBox
    Friend WithEvents radCustomPar As Windows.Forms.RadioButton
    Friend WithEvents radParByColumn As Windows.Forms.RadioButton
    Friend WithEvents grDates As Windows.Forms.GroupBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents txtTimestepSizeSeconds As Windows.Forms.TextBox
    Friend WithEvents pckDate As Windows.Forms.DateTimePicker
    Friend WithEvents radCustomStartDate As Windows.Forms.RadioButton
    Friend WithEvents radDateByColumn As Windows.Forms.RadioButton
    Friend WithEvents grValues As Windows.Forms.GroupBox
    Friend WithEvents grOptions As Windows.Forms.GroupBox
    Friend WithEvents txtSeriesID As Windows.Forms.TextBox
    Friend WithEvents grSeriesID As Windows.Forms.GroupBox
    Friend WithEvents cmbSeriesIDCol As Windows.Forms.ComboBox
    Friend WithEvents radCustomSeriesID As Windows.Forms.RadioButton
    Friend WithEvents radSeriesIDbyColumn As Windows.Forms.RadioButton
    Friend WithEvents chkClearExisting As Windows.Forms.CheckBox
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents lblProgress As Windows.Forms.Label
End Class
