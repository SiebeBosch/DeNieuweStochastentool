Imports STOCHLIB.General
Imports System.IO

Public Class frmRemoveFolder
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
    End Sub

    Private Sub btnRemoveDirs_Click(sender As Object, e As EventArgs) Handles btnRemoveDirs.Click
        If Not txtDirNamePart.Text = "" Then
            Call RemoveStochastDirs(lblRootDir.Text, txtDirNamePart.Text)
        Else
            MsgBox("Specificeer in het tekstvak een (deel van) de naam van directories die verwijderd moeten worden.")
        End If
        Close()
    End Sub

    Private Sub frmRemoveFolder_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblRootDir.Text = Setup.StochastenAnalyse.StochastenDir
        Setup.GeneralFunctions.UpdateProgressBar("klaar", 0, 10)
    End Sub

    Public Sub RemoveStochastDirs(StartDir As String, NameContains As String)
        Dim DirList As String(), i As Integer

        DirList = Directory.GetDirectories(StartDir)
        For i = 0 To DirList.Count - 1
            Setup.GeneralFunctions.UpdateProgressBar("Searching dir " & DirList(i), i, DirList.Count - 1)
            If InStr(DirList(i).Trim.ToUpper, NameContains.Trim.ToUpper) > 0 Then
                Directory.Delete(DirList(i), True)
            Else
                'move over to its subdirs and follow the same procedure
                RemoveStochastDirs(DirList(i), NameContains)
            End If
        Next
        Setup.GeneralFunctions.UpdateProgressBar("klaar", 0, 10)
    End Sub
End Class