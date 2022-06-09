<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmImportOutputlocations
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmImportOutputlocations))
        Me.cmbModelID = New System.Windows.Forms.ComboBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtIDFilter = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmbResultsFilter = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chkCalculationPoints = New System.Windows.Forms.CheckBox()
        Me.btnImport = New System.Windows.Forms.Button()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.chkObservationPoints1D = New System.Windows.Forms.CheckBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.chkFouFiles = New System.Windows.Forms.CheckBox()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmbModelID
        '
        Me.cmbModelID.FormattingEnabled = True
        Me.cmbModelID.Location = New System.Drawing.Point(198, 46)
        Me.cmbModelID.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.cmbModelID.Name = "cmbModelID"
        Me.cmbModelID.Size = New System.Drawing.Size(136, 28)
        Me.cmbModelID.TabIndex = 0
        '
        'GroupBox1
        '
        Me.GroupBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.txtIDFilter)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.cmbResultsFilter)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Controls.Add(Me.cmbModelID)
        Me.GroupBox1.Location = New System.Drawing.Point(14, 15)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox1.Size = New System.Drawing.Size(873, 205)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Modelselectie"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(356, 130)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(387, 20)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Wildcards toegestaan. Gebruik ; voor meerdere filters."
        '
        'txtIDFilter
        '
        Me.txtIDFilter.Location = New System.Drawing.Point(198, 126)
        Me.txtIDFilter.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.txtIDFilter.Name = "txtIDFilter"
        Me.txtIDFilter.Size = New System.Drawing.Size(136, 26)
        Me.txtIDFilter.TabIndex = 6
        Me.txtIDFilter.Text = "*"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(9, 130)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(89, 20)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Filter by ID:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(9, 88)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(157, 20)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Filter modelresultaat:"
        '
        'cmbResultsFilter
        '
        Me.cmbResultsFilter.FormattingEnabled = True
        Me.cmbResultsFilter.Location = New System.Drawing.Point(198, 84)
        Me.cmbResultsFilter.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.cmbResultsFilter.Name = "cmbResultsFilter"
        Me.cmbResultsFilter.Size = New System.Drawing.Size(136, 28)
        Me.cmbResultsFilter.TabIndex = 3
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(7, 50)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(77, 20)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Model ID:"
        '
        'chkCalculationPoints
        '
        Me.chkCalculationPoints.AutoSize = True
        Me.chkCalculationPoints.Checked = True
        Me.chkCalculationPoints.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCalculationPoints.Location = New System.Drawing.Point(24, 41)
        Me.chkCalculationPoints.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.chkCalculationPoints.Name = "chkCalculationPoints"
        Me.chkCalculationPoints.Size = New System.Drawing.Size(157, 24)
        Me.chkCalculationPoints.TabIndex = 1
        Me.chkCalculationPoints.Text = "Rekenpunten 1D"
        Me.chkCalculationPoints.UseVisualStyleBackColor = True
        '
        'btnImport
        '
        Me.btnImport.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnImport.Location = New System.Drawing.Point(782, 479)
        Me.btnImport.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(105, 69)
        Me.btnImport.TabIndex = 2
        Me.btnImport.Text = "Import"
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'prProgress
        '
        Me.prProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.prProgress.Location = New System.Drawing.Point(24, 519)
        Me.prProgress.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(752, 29)
        Me.prProgress.TabIndex = 3
        '
        'lblProgress
        '
        Me.lblProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(20, 479)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(76, 20)
        Me.lblProgress.TabIndex = 3
        Me.lblProgress.Text = "Progress:"
        '
        'chkObservationPoints1D
        '
        Me.chkObservationPoints1D.AutoSize = True
        Me.chkObservationPoints1D.Checked = True
        Me.chkObservationPoints1D.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkObservationPoints1D.Location = New System.Drawing.Point(24, 75)
        Me.chkObservationPoints1D.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.chkObservationPoints1D.Name = "chkObservationPoints1D"
        Me.chkObservationPoints1D.Size = New System.Drawing.Size(311, 24)
        Me.chkObservationPoints1D.TabIndex = 8
        Me.chkObservationPoints1D.Text = "Observation points 1D (alleen D-Hydro)"
        Me.chkObservationPoints1D.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox2.Controls.Add(Me.chkFouFiles)
        Me.GroupBox2.Controls.Add(Me.chkObservationPoints1D)
        Me.GroupBox2.Controls.Add(Me.chkCalculationPoints)
        Me.GroupBox2.Location = New System.Drawing.Point(14, 228)
        Me.GroupBox2.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Padding = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GroupBox2.Size = New System.Drawing.Size(873, 235)
        Me.GroupBox2.TabIndex = 4
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Objecten"
        '
        'chkFouFiles
        '
        Me.chkFouFiles.AutoSize = True
        Me.chkFouFiles.Checked = True
        Me.chkFouFiles.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkFouFiles.Location = New System.Drawing.Point(24, 107)
        Me.chkFouFiles.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.chkFouFiles.Name = "chkFouFiles"
        Me.chkFouFiles.Size = New System.Drawing.Size(138, 24)
        Me.chkFouFiles.TabIndex = 10
        Me.chkFouFiles.Text = "Centroïden 2D"
        Me.chkFouFiles.UseVisualStyleBackColor = True
        '
        'frmImportOutputlocations
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(900, 562)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.GroupBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Name = "frmImportOutputlocations"
        Me.Text = "Uitvoerlocaties model importeren"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents cmbModelID As ComboBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents Label1 As Label
    Friend WithEvents chkCalculationPoints As CheckBox
    Friend WithEvents btnImport As Button
    Friend WithEvents prProgress As ProgressBar
    Friend WithEvents lblProgress As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents cmbResultsFilter As ComboBox
    Friend WithEvents txtIDFilter As TextBox
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents chkObservationPoints1D As CheckBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents chkFouFiles As CheckBox
End Class
