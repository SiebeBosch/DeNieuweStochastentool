Public Class frmAddMeteoStation

  Public naam As String
  Public soort As String
  Public ARF As Double

  Private Sub frmAddMeteoStation_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    cmbMeteoSoort.Items.Clear()
    cmbMeteoSoort.Items.Add("neerslag")
    cmbMeteoSoort.Items.Add("verdamping")
  End Sub

  Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
    NAAM = txtName.Text
    SOORT = cmbMeteoSoort.Text
    ARF = txtArf.Text
  End Sub
End Class