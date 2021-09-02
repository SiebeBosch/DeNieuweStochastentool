Imports STOCHLIB.General

Public Class frmAddStochast

  Public FileName As String
  Public Kans As Double
  Public Naam As String

  Private Sub frmAddStochast_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
    dlgOpenFile.Filter = "*|*"
    dlgOpenFile.ShowDialog()
    txtStochastFile.Text = dlgOpenFile.FileName
  End Sub

  Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
    FileName = txtStochastFile.Text
    Kans = Convert.ToDouble(txtStochastKans.Text)
    Naam = txtStochastNaam.Text
    Me.Close()
  End Sub
End Class