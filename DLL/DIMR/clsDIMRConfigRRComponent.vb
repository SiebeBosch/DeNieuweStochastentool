Option Explicit On
Imports STOCHLIB.General
Imports System.IO


Public Class clsDIMRCONFIGFRRComponent
    Inherits clsDIMRConfigComponent

    Dim fnmFile As clsFnmFile 'this is the configuration file

    Public Sub New(ByRef MySetup As clsSetup, ByRef myDIMR As clsDIMR)
        MyBase.New(MySetup, myDIMR)
    End Sub

    Public Function Read() As Boolean
        'this function reads the entire RR component
        Try
            fnmFile = New clsFnmFile(MyBase.Setup, MyBase.DIMR.ProjectDir & "\" & MyBase.SubDir, MyBase.GetInputFile)
            fnmFile.Read()

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function GetBuiFileName() As String
        Return fnmFile.GetBuiFileName
    End Function

    Public Function GetEvpFileName() As String
        Return fnmFile.GetEvpFileName
    End Function


End Class
