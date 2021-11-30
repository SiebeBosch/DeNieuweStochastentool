Option Explicit On
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO
Public Class clsDIMRConfigFile
    Private Setup As clsSetup
    Dim Path As String

    Public Sub New(ByRef mySetup As clsSetup, ByRef myPath As String)
        Setup = mySetup
        Path = myPath
    End Sub

End Class
