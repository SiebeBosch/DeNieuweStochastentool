Imports System.Windows.Forms

Public Class frmAddRowsToDatagrid
    Private DataGrid As DataGridView
    Public Sub New(ByRef myGrid As DataGridView)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        DataGrid = myGrid

    End Sub
    Private Sub frmAddRowsToDatagrid_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Dim i As Integer
        For i = 1 To txtnRows.Text
            DataGrid.Rows.Add()
        Next
        Me.Close()
    End Sub
End Class