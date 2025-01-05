Imports STOCHLIB.General
Public Class frmAddMeteoStation
    Private Setup As clsSetup

    Public naam As String
    Public soort As String
    Public gebiedsreductie As STOCHLIB.GeneralFunctions.enmGebiedsreductie
    Public ARF As Double        'constante gebiedsreductiefactor
    Public oppervlak As Double  'gebiedsreductiefactor afhankelijk van oppervlak

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
    End Sub

    Private Sub frmAddMeteoStation_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        cmbMeteoSoort.Items.Clear()
        cmbMeteoSoort.Items.Add("neerslag")
        cmbMeteoSoort.Items.Add("verdamping")

        Me.Setup.GeneralFunctions.PopulateComboBoxColumnWithEnumNames(cmbGebiedsreductie, GetType(STOCHLIB.GeneralFunctions.enmGebiedsreductie))
    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        naam = txtName.Text
        soort = cmbMeteoSoort.Text
        ARF = txtARF.Text
        oppervlak = txtOppervlak.Text
    End Sub
End Class