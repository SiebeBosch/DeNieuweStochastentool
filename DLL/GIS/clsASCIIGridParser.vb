Imports System.IO
Imports STOCHLIB.General

Public Class clsASCIIGridParser

    Dim Path As String
    Dim Setup As clsSetup

    Public Sub New(myPath As String, ByRef mySetup As clsSetup)
        Path = myPath
        Setup = mySetup
    End Sub

    Public Function ReadBlock(ByRef myReader As StreamReader, BlockSize As Integer) As List(Of String())
        'reads a block of data from asc file and writes it to a list of arrays
        'each array contains an entire row
        Dim myString As String, myStrings() As String
        Dim myList As New List(Of String())

        Try
            For r = 1 To BlockSize
                If Not myReader.EndOfStream Then
                    myString = myReader.ReadLine.Trim
                    myStrings = myString.Split(" ")
                    myList.Add(myStrings)
                End If
            Next
            Return myList
        Catch ex As Exception
            Console.WriteLine("Error in function ReadBlock: " & ex.Message)
            Return Nothing
        End Try

    End Function

    Public Function ReadLine(ByRef myReader As StreamReader) As String()
        'reads a block of data from asc file and writes it to a list of arrays
        'each array contains an entire row
        Dim myString As String, myStrings() As String = Nothing

        Try
            If Not myReader.EndOfStream Then
                myString = myReader.ReadLine.Trim
                myStrings = myString.Split(" ")
            End If
            Return myStrings
        Catch ex As Exception
            Console.WriteLine("Error in function ReadLine: " & ex.Message)
            Return Nothing
        End Try

    End Function

    Public Sub WriteBlock(ByRef myWriter As StreamWriter, DataBlock As List(Of String()), BlockSize As Integer)
        Dim i As Integer
        For i = 0 To DataBlock.Count - 1
            myWriter.WriteLine(String.Join(" ", DataBlock.Item(i)))
        Next
    End Sub

    Public Sub WriteLine(ByRef myWriter As StreamWriter, myLine As String())
        myWriter.WriteLine(String.Join(" ", myLine))
    End Sub

    Public Sub WriteAscHeader(ByRef myWriter As StreamWriter, ByRef nCols As Integer, ByRef nrows As Integer, ByRef xll As Double, ByRef yll As Double, ByRef cellsize As Integer, ByRef nodata_value As Double)
        myWriter.WriteLine("ncols " & nCols)
        myWriter.WriteLine("nrows " & nrows)
        myWriter.WriteLine("xllcorner " & xll)
        myWriter.WriteLine("yllcorner " & yll)
        myWriter.WriteLine("cellsize " & cellsize)
        myWriter.WriteLine("nodata_value " & nodata_value)
    End Sub

    Public Sub ReadAscHeader(ByRef myReader As StreamReader, ByRef nCols As Integer, ByRef nrows As Integer, ByRef xll As Double, ByRef yll As Double, ByRef cellsize As Integer, ByRef nodata_value As String)
        Dim myString As String
        Dim i As Integer
        For i = 1 To 6
            myString = myReader.ReadLine
            If InStr(myString, "ncols") > 0 Then
                Call Setup.GeneralFunctions.ParseString(myString, " ")
                nCols = Setup.GeneralFunctions.ParseString(myString, " ")
            ElseIf InStr(myString, "nrows") > 0 Then
                Call Setup.GeneralFunctions.ParseString(myString, " ")
                nrows = Setup.GeneralFunctions.ParseString(myString, " ")
            ElseIf InStr(myString, "cellsize") > 0 Then
                Call Setup.GeneralFunctions.ParseString(myString, " ")
                cellsize = Setup.GeneralFunctions.ParseString(myString, " ")
            ElseIf InStr(myString, "nodata_value", CompareMethod.Text) > 0 Then
                Call Setup.GeneralFunctions.ParseString(myString, " ")
                nodata_value = Setup.GeneralFunctions.ParseString(myString, " ")
            ElseIf InStr(myString, "xllcorner") > 0 Then
                Call Setup.GeneralFunctions.ParseString(myString, " ")
                xll = Setup.GeneralFunctions.ParseString(myString, " ")
            ElseIf InStr(myString, "yllcorner") > 0 Then
                Call Setup.GeneralFunctions.ParseString(myString, " ")
                yll = Setup.GeneralFunctions.ParseString(myString, " ")
            End If
        Next
    End Sub


End Class
