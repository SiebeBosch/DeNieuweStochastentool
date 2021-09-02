Option Explicit On

Imports System.IO
Imports STOCHLIB.General

Public Class clsHiaFile
    Friend Path As String
    'Friend LongLocationsByID As New Dictionary(Of String, Integer)
    'Friend LongLocationsByNum As New Dictionary(Of Integer, String)

    Friend LongLocations As New Dictionary(Of Integer, clsLongLocation)

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByVal myPath As String)
        Setup = mySetup
        Path = myPath
    End Sub

    Public Sub Clear()
        LongLocations = New Dictionary(Of Integer, clsLongLocation)
    End Sub

    Public Function Read() As Boolean
        Dim myStr As String, myNum As Integer
        Dim InsideLongLocations As Boolean = False
        Try
            LongLocations = New Dictionary(Of Integer, clsLongLocation)
            If System.IO.File.Exists(Path) Then
                Using myReader As New StreamReader(Path)
                    While Not myReader.EndOfStream
                        myStr = myReader.ReadLine
                        If myStr.ToLower = "[long locations]" Then
                            InsideLongLocations = True
                        ElseIf myStr.ToLower = "[" OrElse myStr.Trim = "" Then
                            InsideLongLocations = False
                        ElseIf InsideLongLocations Then
                            myNum = Me.Setup.GeneralFunctions.ParseString(myStr, "=")
                            LongLocations.Add(myNum - 1, New clsLongLocation(myStr, myNum - 1))  'let op: het indexnummer in de HIA-file correspondeert niet met die in de binaryreader. Heeft te maken met base 0 in DotNet
                        End If
                    End While
                End Using
            Else
                Return True
            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading .hia file.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GetByID(ID As String) As clsLongLocation
        For Each myLoc As clsLongLocation In LongLocations.Values
            If myLoc.ID.Trim.ToUpper = ID.Trim.ToUpper Then Return myLoc
        Next
        Return Nothing
    End Function

End Class
