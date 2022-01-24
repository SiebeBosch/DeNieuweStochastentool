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
        Me.chkObservationPoints = New System.Windows.Forms.CheckBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmbModelID
        '
        Me.cmbModelID.FormattingEnabled = True
        Me.cmbModelID.Location = New System.Drawing.Point(176, 37)
        Me.cmbModelID.Name = "cmbModelID"
        Me.cmbModelID.Size = New System.Drawing.Size(121, 24)
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
        Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(776, 164)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Modelselectie"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(316, 104)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(350, 17)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Wildcards toegestaan. Gebruik ; voor meerdere filters."
        '
        'txtIDFilter
        '
        Me.txtIDFilter.Location = New System.Drawing.Point(176, 101)
        Me.txtIDFilter.Name = "txtIDFilter"
        Me.txtIDFilter.Size = New System.Drawing.Size(121, 22)
        Me.txtIDFilter.TabIndex = 6
        Me.txtIDFilter.Text = "*"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(8, 104)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(79, 17)
        Me.Label3.TabIndex = 5
        Me.Label3.Text = "Filter by ID:"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(8, 70)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(140, 17)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Filter modelresultaat:"
        '
        'cmbResultsFilter
        '
        Me.cmbResultsFilter.FormattingEnabled = True
        Me.cmbResultsFilter.Location = New System.Drawing.Point(176, 67)
        Me.cmbResultsFilter.Name = "cmbResultsFilter"
        Me.cmbResultsFilter.Size = New System.Drawing.Size(121, 24)
        Me.cmbResultsFilter.TabIndex = 3
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(6, 40)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(67, 17)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Model ID:"
        '
        'chkCalculationPoints
        '
        Me.chkCalculationPoints.AutoSize = True
        Me.chkCalculationPoints.Checked = True
        Me.chkCalculationPoints.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCalculationPoints.Location = New System.Drawing.Point(21, 33)
        Me.chkCalculationPoints.Name = "chkCalculationPoints"
        Me.chkCalculationPoints.Size = New System.Drawing.Size(115, 21)
        Me.chkCalculationPoints.TabIndex = 1
        Me.chkCalculationPoints.Text = "Rekenpunten"
        Me.chkCalculationPoints.UseVisualStyleBackColor = True
        '
        'btnImport
        '
        Me.btnImport.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnImport.Location = New System.Drawing.Point(695, 383)
        Me.btnImport.Name = "btnImport"
        Me.btnImport.Size = New System.Drawing.Size(93, 55)
        Me.btnImport.TabIndex = 2
        Me.btnImport.Text = "Import"
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'prProgress
        '
        Me.prProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.prProgress.Location = New System.Drawing.Point(21, 415)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(668, 23)
        Me.prProgress.TabIndex = 3
        '
        'lblProgress
        '
        Me.lblProgress.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(18, 383)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(69, 17)
        Me.lblProgress.TabIndex = 3
        Me.lblProgress.Text = "Progress:"
        '
        'chkObservationPoints
        '
        Me.chkObservationPoints.AutoSize = True
        Me.chkObservationPoints.Checked = True
        Me.chkObservationPoints.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkObservationPoints.Location = New System.Drawing.Point(21, 64)
        Me.chkObservationPoints.Name = "chkObservationPoints"
        Me.chkObservationPoints.Size = New System.Drawing.Size(258, 21)
        Me.chkObservationPoints.TabIndex = 8
        Me.chkObservationPoints.Text = "Observation points (alleen D-Hydro)"
        Me.chkObservationPoints.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox2.Controls.Add(Me.chkObservationPoints)
        Me.GroupBox2.Controls.Add(Me.chkCalculationPoints)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 182)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(776, 188)
        Me.GroupBox2.TabIndex = 4
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Objecten"
        '
        'frmImportOutputlocations
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.GroupBox1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
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
    Friend WithEvents chkObservationPoints As CheckBox
    Friend WithEvents GroupBox2 As GroupBox
End Class
