Option Explicit On
Imports STOCHLIB.General
Imports System.IO

Public Class clsFnmFile
    Private Setup As clsSetup

    Dim SourceDir As String
    Dim FileName As String
    Dim BuiFile As String
    Dim EvpFile As String

    Public Sub New(ByRef mySetup As clsSetup, mySourceDir As String, myFileName As String)
        Setup = mySetup
        SourceDir = mySourceDir
        FileName = myFileName
    End Sub

    Public Function Read()
        Try
            Dim myLine As String
            Using fnmReader As New StreamReader(SourceDir & "\" & FileName)
                While Not fnmReader.EndOfStream
                    myLine = fnmReader.ReadLine
                    If InStr(myLine, ".bui", CompareMethod.Text) > 0 Then
                        BuiFile = Me.Setup.GeneralFunctions.ParseString(myLine, " ", 1)
                    ElseIf InStr(myLine, ".evp", CompareMethod.Text) > 0 Then
                        EvpFile = Me.Setup.GeneralFunctions.ParseString(myLine, " ", 1)
                    End If
                End While
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error Reading fnm file " + ex.Message)
            Return False
        End Try
    End Function

    Public Function GetBuiFileName() As String
        Return BuiFile
    End Function

    Public Function GetEvpFileName() As String
        Return EvpFile
    End Function

End Class
