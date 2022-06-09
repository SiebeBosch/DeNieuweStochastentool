Imports STOCHLIB.General
Imports System.IO
Public Class clsFouConfigFile
    Private FlowFM As clsFlowFMComponent
    Private Setup As clsSetup

    Dim Records As New Dictionary(Of String, clsFouConfigRecord)
    Dim Filename As String
    Dim Path As String

    Public Sub New(ByRef mySetup As clsSetup, ByRef myFlowFM As clsFlowFMComponent, myFileName As String)
        FlowFM = myFlowFM
        Setup = mySetup
        Filename = myFileName
        Path = myFlowFM.getDirectory & "\" * myFileName
    End Sub

    Public Function Read() As Boolean
        Try
            Dim myLine As String
            Using myReader As New StreamReader(Path)
                While Not myReader.EndOfStream
                    myLine = myReader.ReadLine.Trim
                    If Left(myLine, 1) = "*" Then
                        'comment line. skip
                    Else
                        If Left(myLine, 2) = "wl" Then
                            'water levels
                            Dim var As String = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim tsrts As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim sstop As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim numcyc As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim knfac As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim v0plu As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim layno As Integer = 0
                            Dim elp As String = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim newRecord As New clsFouConfigRecord(var, tsrts, sstop, numcyc, knfac, v0plu, layno, elp)
                            Records.Add(var, newRecord)
                        ElseIf Left(myLine, 2) = "uc" Then
                            'water levels
                            Dim var As String = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim tsrts As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim sstop As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim numcyc As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim knfac As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim v0plu As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim layno As Integer = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim elp As String = Me.Setup.GeneralFunctions.ParseString(myLine)
                            Dim newRecord As New clsFouConfigRecord(var, tsrts, sstop, numcyc, knfac, v0plu, layno, elp)
                            Records.Add(var, newRecord)
                        Else
                            Me.Setup.Log.AddWarning("Nog all variables in the Fourier Configuration file are supported yet: " & Left(myLine, 2))
                        End If
                    End If

                End While
            End Using

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function




End Class
