Public Class frmPickDelimiter
    Public Delimiter As String
    Public ContainsHeader As Boolean

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        txtDelimiter.Text = My.Settings.Delimiter
    End Sub

    Private Sub btnChoose_Click(sender As Object, e As EventArgs) Handles btnChoose.Click
        Delimiter = txtDelimiter.Text
        ContainsHeader = chkContainsHeader.Checked
        My.Settings.Delimiter = txtDelimiter.Text
        My.Settings.ContainsHeader = chkContainsHeader.Checked
        My.Settings.Save()
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub frmPickDelimiter_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        txtDelimiter.Text = My.Settings.Delimiter
        chkContainsHeader.Checked = My.Settings.ContainsHeader
    End Sub
End Class