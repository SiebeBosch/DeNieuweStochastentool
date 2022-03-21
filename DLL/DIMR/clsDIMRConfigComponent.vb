Option Explicit On
Imports STOCHLIB.General
Imports System.IO

Public Class clsDIMRConfigComponent
    Friend Setup As clsSetup
    Friend DIMR As clsDIMR

    Public InUse As Boolean
    Public SubDir As String

    Dim Library As String
    Dim InputFile As String

    Public Sub New(ByRef mySetup As clsSetup, ByRef MyDIMR As clsDIMR)
        Setup = mySetup
        DIMR = MyDIMR
    End Sub

    Public Sub SetSubDir(myDir As String)
        SubDir = myDir
    End Sub

    Public Function GetSubDir() As String
        Return SubDir
    End Function

    Public Function GetFullDir() As String
        Return DIMR.ProjectDir & "\" & GetSubDir()
    End Function

    Public Sub SetLibrary(myLibrary As String)
        Library = myLibrary
    End Sub

    Public Sub SetInputFile(myInputFile As String)
        InputFile = myInputFile
    End Sub

    Public Function GetInputFile() As String
        Return InputFile
    End Function
End Class
