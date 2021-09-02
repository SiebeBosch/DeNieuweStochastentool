Public Class frmChooseSobekCase
    Private Setup As General.clsSetup
    Public Sub New(ByRef mySetup As General.clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub

    Private Sub btnSetSobek_Click(sender As Object, e As EventArgs) Handles btnSetSobek.Click

        'first set the sobek project and case
        'Setup.SetProgress(prProgress, lblProgress)
        If cmbSobekCases.Text <> "" Then
            Setup.InitSobekModel(True, True)
            Setup.SetActiveCase(cmbSobekCases.Text)
        End If
        Me.Close()
    End Sub

    Private Sub btnSbkProject_Click(sender As Object, e As EventArgs) Handles btnSbkProject.Click
        dlgFolder.RootFolder = Environment.SpecialFolder.MyComputer
        dlgFolder.ShowDialog()
        txtSbkProject.Text = dlgFolder.SelectedPath
        If Not txtSbkProject.Text = "" Then
            Setup.SetAddSobekProject(txtSbkProject.Text, txtSbkProject.Text)
            Setup.PopulateComboBoxSobekCases(cmbSobekCases)
            My.Settings.SobekDir = dlgFolder.SelectedPath
            My.Settings.Save()
        End If
    End Sub
End Class