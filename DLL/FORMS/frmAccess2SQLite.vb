Imports STOCHLIB.General

Public Class frmAccess2SQLite
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

        Setup.SetProgress(prProgress, lblProgress)

    End Sub

    Private Sub BtnAccess_Click(sender As Object, e As EventArgs) Handles btnAccess.Click
        dlgOpenFile.Filter = "Access 2003 database|*.mdb"
        dlgOpenFile.ShowDialog()
        txtAccess.Text = dlgOpenFile.FileName
    End Sub

    Private Sub BtnSQLite_Click(sender As Object, e As EventArgs) Handles btnSQLite.Click
        dlgSaveFile.Filter = "SQLite database|*.db"
        dlgSaveFile.ShowDialog()
        txtSQLite.Text = dlgSaveFile.FileName
    End Sub

    Private Sub BtnConvert_Click(sender As Object, e As EventArgs) Handles btnConvert.Click
        Try
            Dim RootDir As String = Me.Setup.GeneralFunctions.DirFromFileName(txtSQLite.Text)
            Me.Setup.Log.AddMessage("Rootdirectory set to : " & RootDir)
            Me.Setup.SetProgress(prProgress, lblProgress)
            Me.Setup.Log.AddMessage("Progress bar set")
            Me.Setup.Access2Sqlite(txtAccess.Text, txtSQLite.Text)
            Me.Setup.Log.write(RootDir & "\logfile.txt", True)
        Catch ex As Exception
            Me.Setup.Log.AddError("Error converting database: " & ex.Message)
        End Try
    End Sub

End Class