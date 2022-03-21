Imports STOCHLIB.General
Imports System.IO
Public Class clsMDUFile
    Private Setup As clsSetup
    Private DIMR As clsDIMR
    Dim path As String

    Friend NetFile As String   'specifies the name of the geometry file (.nc)
    Friend HisFile As String   'specifies the name of the his results file (usually _his.nc)
    Friend MapFile As String   'specifies the name of the map results file (usually _map_nc)
    Friend ObsFile As String   'specifies the name of the observationpoints file

    Public Sub New(ByRef mySetup As clsSetup, ByRef myDIMR As clsDIMR, myPath As String)
        Setup = mySetup
        DIMR = myDIMR
        path = myPath
    End Sub

    Public Function getFilename() As String
        Return Me.Setup.GeneralFunctions.GetFileNameFromPath(path)
    End Function

    Public Function Read() As Boolean
        Try
            Dim myLine As String
            Using mduReader As New StreamReader(path)
                While Not mduReader.EndOfStream
                    myLine = mduReader.ReadLine.Trim.ToLower
                    If Left(myLine.Trim.ToLower, 7) = "netfile" Then
                        NetFile = DIMR.ProjectDir & "\" & DIMR.DIMRConfig.Flow1D.SubDir & "\" & Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
                    ElseIf Left(myLine.Trim.ToLower, 7) = "obsfile" Then
                        ObsFile = DIMR.ProjectDir & "\" & DIMR.DIMRConfig.Flow1D.SubDir & "\" & Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)
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
