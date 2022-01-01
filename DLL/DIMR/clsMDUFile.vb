Imports STOCHLIB.General
Imports System.IO
Public Class clsMDUFile
    Private Setup As clsSetup
    Private DimrConfig As clsDIMRConfigFile
    Dim path As String

    Friend NetFile As String   'specifies the name of the geometry file (.nc)
    Friend HisFile As String   'specifies the name of the his results file (usually _his.nc)
    Friend MapFile As String   'specifies the name of the map results file (usually _map_nc)

    Public Sub New(ByRef mySetup As clsSetup, ByRef myDimrConfig As clsDIMRConfigFile, myPath As String)
        Setup = mySetup
        DimrConfig = myDimrConfig
        path = myPath
    End Sub

    Public Function Read() As Boolean
        Try
            Dim myLine As String
            Using mduReader As New StreamReader(path)
                While Not mduReader.EndOfStream
                    myLine = mduReader.ReadLine.Trim.ToLower
                    If Left(myLine, 7) = "netfile" Then
                        NetFile = DimrConfig.Dir & "\" & "\" & DimrConfig.Flow1D.SubDir & "\" & Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
                    End If
                End While
            End Using
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading MDU file: " & ex.Message)
            Return False
        End Try

    End Function




End Class
