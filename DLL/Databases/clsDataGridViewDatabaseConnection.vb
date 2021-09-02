Imports System.Data.SqlClient
Imports System.Windows.Forms

Public Class clsDataGridViewDatabaseConnection

  Dim sCommand As SqlCommand
  Dim sAdapter As SqlDataAdapter
  Dim sBuilder As SqlCommandBuilder
  Dim sDs As DataSet
  Dim sTable As DataTable

  Private DataGridView1 As DataGridView

  Public Sub New(ByRef myDataGridView As DataGridView)
    DataGridView1 = myDataGridView
  End Sub

  Private Sub load()
    Dim connectionString As String = "Data Source=.;Initial Catalog=pubs;Integrated Security=True"
    Dim sql As String = "SELECT * FROM Stores"
    Dim connection As New SqlConnection(connectionString)
    connection.Open()
    sCommand = New SqlCommand(sql, connection)
    sAdapter = New SqlDataAdapter(sCommand)
    sBuilder = New SqlCommandBuilder(sAdapter)
    sDs = New DataSet()
    sAdapter.Fill(sDs, "Stores")
    sTable = sDs.Tables("Stores")
    connection.Close()
    DataGridView1.DataSource = sDs.Tables("Stores")
    DataGridView1.ReadOnly = True
    DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect

  End Sub

  Private Sub delete()
    If MessageBox.Show("Do you want to delete this row ?", "Delete", MessageBoxButtons.YesNo) = DialogResult.Yes Then
      DataGridView1.Rows.RemoveAt(DataGridView1.SelectedRows(0).Index)
      sAdapter.Update(sTable)
    End If
  End Sub

  Private Sub save()
    sAdapter.Update(sTable)
    'DataGridView1.[ReadOnly] = True
  End Sub
End Class
