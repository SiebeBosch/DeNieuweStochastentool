Imports STOCHLIB.General

Public Class frmDirRename
    Private Setup As clsSetup

    Public Sub New(ByRef MySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = MySetup
    End Sub

    Private Sub btnMultiDirRename_Click(sender As Object, e As EventArgs) Handles btnMultiDirRename.Click
        'get all subdirs starting from the root dir
        Setup.GeneralFunctions.MultiRenameSubDirs(lblRootDir.Text, txtFromDir.Text, txtToDir.Text)
        Me.Close()
    End Sub



End Class