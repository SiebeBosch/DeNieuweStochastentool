Imports System.Windows.Forms
Imports STOCHLIB.General
Public Class frmAppendTextFiles
    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

    End Sub

    Private Sub frmAppendTextFiles_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        grFiles.Rows.Add()
    End Sub

    Private Sub grFiles_CellMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles grFiles.CellMouseClick
        If e.ColumnIndex = 0 Then
            dlgOpenFile.Filter = "Text files|*.*"
            dlgOpenFile.ShowDialog()
            grFiles.Rows(e.RowIndex).Cells(e.ColumnIndex).Value = dlgOpenFile.FileName
        End If
    End Sub

    Private Sub btnExecute_Click(sender As Object, e As EventArgs) Handles btnExecute.Click
        Dim i As Integer
        Dim SkipEmptyLines As Boolean = chkSkipEmptyLines.Checked
        dlgSaveFile.Title = "Save file as"
        dlgSaveFile.ShowDialog()

        Me.Setup.SetProgress(prProgress, lblProgress)
        Me.Setup.GeneralFunctions.UpdateProgressBar("Appending text files...", 0, 10, True)

        Using myWriter As New System.IO.StreamWriter(dlgSaveFile.FileName)
            For i = 0 To grFiles.Rows.Count - 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, grFiles.Rows.Count)

                Using myReader As New System.IO.StreamReader(grFiles.Rows(0).Cells(0).Value.ToString)
                    Dim myStr As String
                    While Not myReader.EndOfStream
                        myStr = myReader.ReadLine
                        If myStr <> "" OrElse Not SkipEmptyLines Then
                            myWriter.WriteLine(myStr)
                        End If
                    End While
                End Using
            Next
        End Using
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

    End Sub

    Private Sub grFiles_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles grFiles.CellContentClick

    End Sub
End Class