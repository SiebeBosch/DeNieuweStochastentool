Imports STOCHLIB.General

Public Class clsNetworkNTWASCIIGrid
    Inherits clsRaster

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase
    Public ID As String

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        MyBase.New(mySetup)
        Setup = mySetup
        SbkCase = myCase
        Grid = New MapWinGIS.Grid
    End Sub

    Public Function ReadNetworkNTWRecord(Record As String) As Boolean
        Try
            Dim myStr As String, i As Integer = -1
            While Not Record = ""
                myStr = Me.Setup.GeneralFunctions.ParseString(Record, ",", 2)
                i += 1
                Select Case i
                    Case Is = 0
                        ID = myStr
                    Case Is = 5
                        Path = Me.Setup.GeneralFunctions.RelativeToAbsolutePath(myStr, SbkCase.CaseDir)
                        If Not System.IO.File.Exists(Path) Then Throw New Exception("Error: path to 2D Grid does not exist: " & Path)
                End Select
            End While
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ReadNetworkNTWRecord.")
            Return False
        End Try

    End Function

End Class
