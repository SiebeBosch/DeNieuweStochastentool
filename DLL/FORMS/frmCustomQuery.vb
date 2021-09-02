Imports STOCHLIB.General
Imports System.Windows.Forms

Public Class frmCustomQuery
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
    End Sub

    Private Sub btnExecute_Click(sender As Object, e As EventArgs) Handles btnExecute.Click
        Dim query As String = txtQuery.Text
        Dim nAffected As Long = 0
        Me.Cursor = Cursors.WaitCursor
        lblProgress.Text = "Executing query..."

        If Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, True, False) Then
            lblProgress.Text = "Query complete."
        Else
            lblProgress.Text = "Error executing query."
        End If

        Me.Cursor = Cursors.Default
    End Sub
End Class