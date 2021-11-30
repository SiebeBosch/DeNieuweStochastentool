Option Explicit On
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO
Public Class ClsDHydroDIMRProject
    Private Setup As clsSetup
    Dim DIMRConfigFile As clsDIMRConfigFile

    Public Sub New(ByRef mySetup As clsSetup, DIMRConfigFilePath As String)
        Me.Setup = mySetup
        DIMRConfigFile = New clsDIMRConfigFile(Setup, DIMRConfigFilePath)
    End Sub




End Class
