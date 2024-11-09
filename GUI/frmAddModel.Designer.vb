<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAddModel
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAddModel))
        Me.btnAdd = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbModelType = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtExecutable = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtArguments = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtModelDir = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.txtCaseName = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.txtWorkdir = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.txtResultsdirRR = New System.Windows.Forms.TextBox()
        Me.txtResultsDirFlow = New System.Windows.Forms.TextBox()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.btnExecutable = New System.Windows.Forms.Button()
        Me.btnModeldir = New System.Windows.Forms.Button()
        Me.btnWorkdir = New System.Windows.Forms.Button()
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.hlpAddModel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'btnAdd
        '
        Me.btnAdd.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnAdd.Location = New System.Drawing.Point(535, 270)
        Me.btnAdd.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnAdd.Name = "btnAdd"
        Me.btnAdd.Size = New System.Drawing.Size(108, 44)
        Me.btnAdd.TabIndex = 0
        Me.btnAdd.Text = "Add"
        Me.btnAdd.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(20, 12)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(77, 16)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Model type:"
        '
        'cmbModelType
        '
        Me.cmbModelType.AutoCompleteCustomSource.AddRange(New String() {"SOBEK", "DIMR", "DHYDROSERVER", "CUSTOM"})
        Me.cmbModelType.FormattingEnabled = True
        Me.cmbModelType.Location = New System.Drawing.Point(144, 10)
        Me.cmbModelType.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.cmbModelType.Name = "cmbModelType"
        Me.cmbModelType.Size = New System.Drawing.Size(122, 24)
        Me.cmbModelType.TabIndex = 4
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(20, 40)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(77, 16)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Executable:"
        '
        'txtExecutable
        '
        Me.txtExecutable.Location = New System.Drawing.Point(144, 38)
        Me.txtExecutable.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtExecutable.Name = "txtExecutable"
        Me.txtExecutable.Size = New System.Drawing.Size(510, 22)
        Me.txtExecutable.TabIndex = 6
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(20, 70)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(74, 16)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Arguments:"
        '
        'txtArguments
        '
        Me.txtArguments.Location = New System.Drawing.Point(144, 68)
        Me.txtArguments.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtArguments.Name = "txtArguments"
        Me.txtArguments.Size = New System.Drawing.Size(510, 22)
        Me.txtArguments.TabIndex = 8
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(21, 102)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(63, 16)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Modeldir:"
        '
        'txtModelDir
        '
        Me.txtModelDir.Location = New System.Drawing.Point(144, 99)
        Me.txtModelDir.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtModelDir.Name = "txtModelDir"
        Me.txtModelDir.Size = New System.Drawing.Size(510, 22)
        Me.txtModelDir.TabIndex = 10
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(22, 132)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(76, 16)
        Me.Label6.TabIndex = 11
        Me.Label6.Text = "Casename:"
        '
        'txtCaseName
        '
        Me.txtCaseName.Location = New System.Drawing.Point(144, 130)
        Me.txtCaseName.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtCaseName.Name = "txtCaseName"
        Me.txtCaseName.Size = New System.Drawing.Size(510, 22)
        Me.txtCaseName.TabIndex = 12
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(22, 162)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(57, 16)
        Me.Label7.TabIndex = 13
        Me.Label7.Text = "Workdir:"
        '
        'txtWorkdir
        '
        Me.txtWorkdir.Location = New System.Drawing.Point(144, 160)
        Me.txtWorkdir.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtWorkdir.Name = "txtWorkdir"
        Me.txtWorkdir.Size = New System.Drawing.Size(510, 22)
        Me.txtWorkdir.TabIndex = 14
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(21, 222)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(221, 16)
        Me.Label8.TabIndex = 15
        Me.Label8.Text = "Resultsfiles Flow (use ; as delimiter):"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(21, 194)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(213, 16)
        Me.Label9.TabIndex = 16
        Me.Label9.Text = "Resultsfiles RR (use ; as delimiter):"
        '
        'txtResultsdirRR
        '
        Me.txtResultsdirRR.Location = New System.Drawing.Point(270, 194)
        Me.txtResultsdirRR.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtResultsdirRR.Name = "txtResultsdirRR"
        Me.txtResultsdirRR.Size = New System.Drawing.Size(384, 22)
        Me.txtResultsdirRR.TabIndex = 17
        '
        'txtResultsDirFlow
        '
        Me.txtResultsDirFlow.Location = New System.Drawing.Point(270, 222)
        Me.txtResultsDirFlow.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.txtResultsDirFlow.Name = "txtResultsDirFlow"
        Me.txtResultsDirFlow.Size = New System.Drawing.Size(384, 22)
        Me.txtResultsDirFlow.TabIndex = 18
        '
        'btnExecutable
        '
        Me.btnExecutable.Location = New System.Drawing.Point(672, 38)
        Me.btnExecutable.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnExecutable.Name = "btnExecutable"
        Me.btnExecutable.Size = New System.Drawing.Size(23, 21)
        Me.btnExecutable.TabIndex = 19
        Me.btnExecutable.Text = ".."
        Me.btnExecutable.UseVisualStyleBackColor = True
        '
        'btnModeldir
        '
        Me.btnModeldir.Location = New System.Drawing.Point(672, 97)
        Me.btnModeldir.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnModeldir.Name = "btnModeldir"
        Me.btnModeldir.Size = New System.Drawing.Size(23, 21)
        Me.btnModeldir.TabIndex = 21
        Me.btnModeldir.Text = ".."
        Me.btnModeldir.UseVisualStyleBackColor = True
        '
        'btnWorkdir
        '
        Me.btnWorkdir.Location = New System.Drawing.Point(672, 160)
        Me.btnWorkdir.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnWorkdir.Name = "btnWorkdir"
        Me.btnWorkdir.Size = New System.Drawing.Size(23, 21)
        Me.btnWorkdir.TabIndex = 23
        Me.btnWorkdir.Text = ".."
        Me.btnWorkdir.UseVisualStyleBackColor = True
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(26, 296)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(503, 18)
        Me.prProgress.TabIndex = 24
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(30, 270)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(65, 16)
        Me.lblProgress.TabIndex = 25
        Me.lblProgress.Text = "Progress:"
        '
        'hlpAddModel
        '
        Me.hlpAddModel.BackColor = System.Drawing.Color.Gold
        Me.hlpAddModel.Location = New System.Drawing.Point(649, 270)
        Me.hlpAddModel.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.hlpAddModel.Name = "hlpAddModel"
        Me.hlpAddModel.Size = New System.Drawing.Size(46, 44)
        Me.hlpAddModel.TabIndex = 26
        Me.hlpAddModel.Text = "?"
        Me.hlpAddModel.UseVisualStyleBackColor = False
        '
        'frmAddModel
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(711, 334)
        Me.Controls.Add(Me.hlpAddModel)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnWorkdir)
        Me.Controls.Add(Me.btnModeldir)
        Me.Controls.Add(Me.btnExecutable)
        Me.Controls.Add(Me.txtResultsDirFlow)
        Me.Controls.Add(Me.txtResultsdirRR)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.txtWorkdir)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.txtCaseName)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtModelDir)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtArguments)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtExecutable)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.cmbModelType)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.btnAdd)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.Name = "frmAddModel"
        Me.Text = "Add Model"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnAdd As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents cmbModelType As ComboBox
    Friend WithEvents Label3 As Label
    Friend WithEvents txtExecutable As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents txtArguments As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents txtModelDir As TextBox
    Friend WithEvents Label6 As Label
    Friend WithEvents txtCaseName As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents txtWorkdir As TextBox
    Friend WithEvents Label8 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents txtResultsdirRR As TextBox
    Friend WithEvents txtResultsDirFlow As TextBox
    Friend WithEvents dlgFolder As FolderBrowserDialog
    Friend WithEvents btnExecutable As Button
    Friend WithEvents btnModeldir As Button
    Friend WithEvents btnWorkdir As Button
    Friend WithEvents dlgOpenFile As OpenFileDialog
    Friend WithEvents prProgress As ProgressBar
    Friend WithEvents lblProgress As Label
    Friend WithEvents hlpAddModel As Button
End Class
