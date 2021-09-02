Imports STOCHLIB.General

Public Class clsNTWHeader
    Public Version As String
    Public ntrpluviniPath As String
    Public Rest As String

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub Parse(myString As String)
        Version = Setup.GeneralFunctions.ParseString(myString, ",", 2)
        ntrpluviniPath = Setup.GeneralFunctions.ParseString(myString, ",", 2)
        Rest = myString
    End Sub


End Class
