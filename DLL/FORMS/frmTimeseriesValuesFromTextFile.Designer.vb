<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTimeseriesValuesFromTextFile
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTimeseriesValuesFromTextFile))
        Me.btnImport = New System.Windows.Forms.Button()
        Me.txtCSVFile = New System.Windows.Forms.TextBox()
        Me.txtID = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtPar = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.pckStartDate = New System.Windows.Forms.DateTimePicker()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtTimestepSizeSeconds = New System.Windows.Forms.TextBox()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.chkRemoveExisting = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'btnImport
        '
        Me.btnImport.Location = New System.Drawing.Point(765, 21)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(30, 23)
        Me.btnImport.TabIndex = 21
        Me.btnImport.Text = ".."
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'txtCSVFile
        '
        Me.txtCSVFile.Location = New System.Drawing.Point(12, 22)
        Me.txtCSVFile.Name = "txtCSVFile"
        Me.txtCSVFile.Size = New System.Drawing.Size(747, 22)
        Me.txtCSVFile.TabIndex = 20
        '
        'txtID
        '
        Me.txtID.Location = New System.Drawing.Point(229, 52)
        Me.txtID.Name = "txtID"
        Me.txtID.Size = New System.Drawing.Size(249, 22)
        Me.txtID.TabIndex = 39
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(9, 55)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(79, 17)
        Me.Label2.TabIndex = 40
        Me.Label2.Text = "Location ID"
        '
        'txtPar
        '
        Me.txtPar.Location = New System.Drawing.Point(229, 80)
        Me.txtPar.Name = "txtPar"
        Me.txtPar.Size = New System.Drawing.Size(249, 22)
        Me.txtPar.TabIndex = 41
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(9, 83)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(74, 17)
        Me.Label1.TabIndex = 42
        Me.Label1.Text = "Parameter"
        '
        'pckStartDate
        '
        Me.pckStartDate.CustomFormat = ""
        Me.pckStartDate.Location = New System.Drawing.Point(229, 108)
        Me.pckStartDate.Name = "pckStartDate"
        Me.pckStartDate.Size = New System.Drawing.Size(252, 22)
        Me.pckStartDate.TabIndex = 43
        Me.pckStartDate.Value = New Date(2000, 1, 1, 0, 0, 0, 0)
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(9, 113)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(66, 17)
        Me.Label3.TabIndex = 44
        Me.Label3.Text = "Startdate"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(9, 139)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(166, 17)
        Me.Label4.TabIndex = 46
        Me.Label4.Text = "Timestep size (seconds):"
        '
        'txtTimestepSizeSeconds
        '
        Me.txtTimestepSizeSeconds.Location = New System.Drawing.Point(229, 136)
        Me.txtTimestepSizeSeconds.Name = "txtTimestepSizeSeconds"
        Me.txtTimestepSizeSeconds.Size = New System.Drawing.Size(252, 22)
        Me.txtTimestepSizeSeconds.TabIndex = 45
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(12, 243)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(65, 17)
        Me.lblProgress.TabIndex = 49
        Me.lblProgress.Text = "Progress"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(12, 266)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(648, 22)
        Me.prProgress.TabIndex = 48
        '
        'btnOK
        '
        Me.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnOK.Location = New System.Drawing.Point(688, 243)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(107, 45)
        Me.btnOK.TabIndex = 47
        Me.btnOK.Text = "Ok"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        Me.dlgOpenFile.Multiselect = True
        '
        'chkRemoveExisting
        '
        Me.chkRemoveExisting.AutoSize = True
        Me.chkRemoveExisting.Checked = True
        Me.chkRemoveExisting.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkRemoveExisting.Location = New System.Drawing.Point(12, 174)
        Me.chkRemoveExisting.Name = "chkRemoveExisting"
        Me.chkRemoveExisting.Size = New System.Drawing.Size(306, 21)
        Me.chkRemoveExisting.TabIndex = 50
        Me.chkRemoveExisting.Text = "Remove existing results for this combination"
        Me.chkRemoveExisting.UseVisualStyleBackColor = True
        '
        'frmTimeseriesValuesFromTextFile
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 300)
        Me.Controls.Add(Me.chkRemoveExisting)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtTimestepSizeSeconds)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.pckStartDate)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtPar)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtID)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.txtCSVFile)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmTimeseriesValuesFromTextFile"
        Me.Text = "Timeseries values from textfile"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnImport As Windows.Forms.Button
    Friend WithEvents txtCSVFile As Windows.Forms.TextBox
    Friend WithEvents txtID As Windows.Forms.TextBox
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents txtPar As Windows.Forms.TextBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents pckStartDate As Windows.Forms.DateTimePicker
    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents Label4 As Windows.Forms.Label
    Friend WithEvents txtTimestepSizeSeconds As Windows.Forms.TextBox
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents btnOK As Windows.Forms.Button
    Friend WithEvents dlgOpenFile As Windows.Forms.OpenFileDialog
    Friend WithEvents chkRemoveExisting As Windows.Forms.CheckBox
End Class
