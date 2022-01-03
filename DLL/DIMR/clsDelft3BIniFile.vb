Imports STOCHLIB.General
Imports System.IO

Public Class clsDelft3BIniFile
    Private Setup As clsSetup
    Private DimrConfig As clsDIMRConfigFile
    Dim path As String

    Public Sub New(ByRef mySetup As clsSetup, ByRef myDimrConfig As clsDIMRConfigFile, myPath As String)
        Setup = mySetup
        DimrConfig = myDimrConfig
        path = myPath
    End Sub

End Class
