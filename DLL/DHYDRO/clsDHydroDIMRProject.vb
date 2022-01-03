Option Explicit On
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO
Public Class ClsDHydroDIMRProject
    Private Setup As clsSetup
    Dim CaseDir As String
    Dim DIMRConfigFile As clsDIMRConfigFile

    Public Sub New(ByRef mySetup As clsSetup, myCaseDir As String)
        Me.Setup = mySetup
        CaseDir = myCaseDir
        'DIMRConfigFile = New clsDIMRConfigFile(Setup, DIMRConfigFilePath)
    End Sub





End Class
