<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSTOWA2019
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
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSTOWA2019))
        Me.cmbYear = New System.Windows.Forms.ComboBox()
        Me.Year = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cmbScenario = New System.Windows.Forms.ComboBox()
        Me.cmbSeason = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.tabReturnPeriods = New System.Windows.Forms.TabPage()
        Me.Chart1 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.tabVolumes = New System.Windows.Forms.TabPage()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtArea = New System.Windows.Forms.TextBox()
        Me.prProgress = New System.Windows.Forms.ProgressBar()
        Me.btnCalculate = New System.Windows.Forms.Button()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.TabControl1.SuspendLayout()
        Me.tabReturnPeriods.SuspendLayout()
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'cmbYear
        '
        Me.cmbYear.FormattingEnabled = True
        Me.cmbYear.Location = New System.Drawing.Point(123, 43)
        Me.cmbYear.Name = "cmbYear"
        Me.cmbYear.Size = New System.Drawing.Size(121, 24)
        Me.cmbYear.TabIndex = 0
        '
        'Year
        '
        Me.Year.AutoSize = True
        Me.Year.Location = New System.Drawing.Point(28, 46)
        Me.Year.Name = "Year"
        Me.Year.Size = New System.Drawing.Size(38, 17)
        Me.Year.TabIndex = 1
        Me.Year.Text = "Year"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(28, 76)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(64, 17)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Scenario"
        '
        'cmbScenario
        '
        Me.cmbScenario.FormattingEnabled = True
        Me.cmbScenario.Location = New System.Drawing.Point(123, 73)
        Me.cmbScenario.Name = "cmbScenario"
        Me.cmbScenario.Size = New System.Drawing.Size(121, 24)
        Me.cmbScenario.TabIndex = 3
        '
        'cmbSeason
        '
        Me.cmbSeason.FormattingEnabled = True
        Me.cmbSeason.Location = New System.Drawing.Point(123, 103)
        Me.cmbSeason.Name = "cmbSeason"
        Me.cmbSeason.Size = New System.Drawing.Size(121, 24)
        Me.cmbSeason.TabIndex = 4
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(28, 106)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(56, 17)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "Season"
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.tabReturnPeriods)
        Me.TabControl1.Controls.Add(Me.tabVolumes)
        Me.TabControl1.Location = New System.Drawing.Point(266, 12)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(1073, 446)
        Me.TabControl1.TabIndex = 6
        '
        'tabReturnPeriods
        '
        Me.tabReturnPeriods.Controls.Add(Me.Chart1)
        Me.tabReturnPeriods.Location = New System.Drawing.Point(4, 25)
        Me.tabReturnPeriods.Name = "tabReturnPeriods"
        Me.tabReturnPeriods.Padding = New System.Windows.Forms.Padding(3)
        Me.tabReturnPeriods.Size = New System.Drawing.Size(1065, 417)
        Me.tabReturnPeriods.TabIndex = 0
        Me.tabReturnPeriods.Text = "Return periods"
        Me.tabReturnPeriods.UseVisualStyleBackColor = True
        '
        'Chart1
        '
        ChartArea1.Name = "ChartArea1"
        Me.Chart1.ChartAreas.Add(ChartArea1)
        Legend1.Name = "Legend1"
        Me.Chart1.Legends.Add(Legend1)
        Me.Chart1.Location = New System.Drawing.Point(3, 6)
        Me.Chart1.Name = "Chart1"
        Series1.ChartArea = "ChartArea1"
        Series1.Legend = "Legend1"
        Series1.Name = "Series1"
        Me.Chart1.Series.Add(Series1)
        Me.Chart1.Size = New System.Drawing.Size(1056, 405)
        Me.Chart1.TabIndex = 0
        Me.Chart1.Text = "Chart1"
        '
        'tabVolumes
        '
        Me.tabVolumes.Location = New System.Drawing.Point(4, 25)
        Me.tabVolumes.Name = "tabVolumes"
        Me.tabVolumes.Padding = New System.Windows.Forms.Padding(3)
        Me.tabVolumes.Size = New System.Drawing.Size(1065, 417)
        Me.tabVolumes.TabIndex = 1
        Me.tabVolumes.Text = "Volumes"
        Me.tabVolumes.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(28, 138)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(78, 17)
        Me.Label3.TabIndex = 7
        Me.Label3.Text = "Area (km2)"
        '
        'txtArea
        '
        Me.txtArea.Location = New System.Drawing.Point(123, 135)
        Me.txtArea.Name = "txtArea"
        Me.txtArea.Size = New System.Drawing.Size(121, 22)
        Me.txtArea.TabIndex = 8
        Me.txtArea.Text = "0"
        '
        'prProgress
        '
        Me.prProgress.Location = New System.Drawing.Point(25, 496)
        Me.prProgress.Name = "prProgress"
        Me.prProgress.Size = New System.Drawing.Size(1310, 23)
        Me.prProgress.TabIndex = 9
        '
        'btnCalculate
        '
        Me.btnCalculate.Location = New System.Drawing.Point(123, 172)
        Me.btnCalculate.Name = "btnCalculate"
        Me.btnCalculate.Size = New System.Drawing.Size(121, 37)
        Me.btnCalculate.TabIndex = 10
        Me.btnCalculate.Text = "Calculate"
        Me.btnCalculate.UseVisualStyleBackColor = True
        '
        'lblProgress
        '
        Me.lblProgress.AutoSize = True
        Me.lblProgress.Location = New System.Drawing.Point(22, 464)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(65, 17)
        Me.lblProgress.TabIndex = 11
        Me.lblProgress.Text = "Progress"
        '
        'frmSTOWA2019
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1351, 531)
        Me.Controls.Add(Me.lblProgress)
        Me.Controls.Add(Me.btnCalculate)
        Me.Controls.Add(Me.prProgress)
        Me.Controls.Add(Me.txtArea)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.cmbSeason)
        Me.Controls.Add(Me.cmbScenario)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Year)
        Me.Controls.Add(Me.cmbYear)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmSTOWA2019"
        Me.Text = "Rainfall statistics by STOWA 2019"
        Me.TabControl1.ResumeLayout(False)
        Me.tabReturnPeriods.ResumeLayout(False)
        CType(Me.Chart1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents cmbYear As Windows.Forms.ComboBox
    Friend WithEvents Year As Windows.Forms.Label
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents cmbScenario As Windows.Forms.ComboBox
    Friend WithEvents cmbSeason As Windows.Forms.ComboBox
    Friend WithEvents Label2 As Windows.Forms.Label
    Friend WithEvents TabControl1 As Windows.Forms.TabControl
    Friend WithEvents tabReturnPeriods As Windows.Forms.TabPage
    Friend WithEvents tabVolumes As Windows.Forms.TabPage
    Friend WithEvents Label3 As Windows.Forms.Label
    Friend WithEvents txtArea As Windows.Forms.TextBox
    Friend WithEvents prProgress As Windows.Forms.ProgressBar
    Friend WithEvents btnCalculate As Windows.Forms.Button
    Friend WithEvents lblProgress As Windows.Forms.Label
    Friend WithEvents Chart1 As Windows.Forms.DataVisualization.Charting.Chart
End Class
