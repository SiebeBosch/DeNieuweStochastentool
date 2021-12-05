Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports System.Windows.Forms

Public Class clsDIMR
    Private Setup As clsSetup
    Public DIMRConfig As clsDIMRConfigFile
    Public ProjectDir As String

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myProjectDir As String)
        Setup = mySetup
        DIMRConfig = New clsDIMRConfigFile(Me.Setup, Me)
        DIMRConfig.Read()
    End Sub

    Public Function SetProject(myProjectDir As String) As Boolean
        Try
            ProjectDir = myProjectDir
            DIMRConfig = New clsDIMRConfigFile(Me.Setup, Me)
            'DIMRConfig.Read()       'reads the dimr_config
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error creating DIMR Project " + ex.Message)
            Return False
        End Try
    End Function

End Class
