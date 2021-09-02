Imports STOCHLIB.General
Public Class frmSTOWA2019
    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
    End Sub
    Private Sub FrmSTOWA2019_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Setup.GeneralFunctions.PopulateComboBoxWithEnumNames(cmbYear, GetType(GeneralFunctions.enmSTOWA2019ScenarioYear))
        Me.Setup.GeneralFunctions.PopulateComboBoxWithEnumNames(cmbScenario, GetType(GeneralFunctions.enmSTOWA2019Scenario))
        Me.Setup.GeneralFunctions.PopulateComboBoxWithEnumNames(cmbSeason, GetType(GeneralFunctions.enmSTOWA2019Season))

        cmbYear.Text = "ANNO2019"
        cmbScenario.Text = "NONE"
        cmbSeason.Text = "JAARROND"

    End Sub

    Private Sub BtnCalculate_Click(sender As Object, e As EventArgs) Handles btnCalculate.Click
        Me.Setup.InitializeIDFs()
        Me.Setup.IDFs.CalculateFromSTOWA2019(cmbYear.Text, cmbScenario.Text, cmbSeason.Text, txtArea.Text)
    End Sub
End Class