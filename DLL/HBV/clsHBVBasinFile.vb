Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports System.Collections.Generic

Public Class clsHBVBasinFile
    Private Setup As clsSetup
    Private Path As String

    Dim FileFormat As Integer
    Public Basins As Dictionary(Of String, clsHBVSubBasin)

    Public Sub New(ByVal Setup As clsSetup, myPath As String)
        Me.Setup = Setup
        Path = myPath
        Basins = New Dictionary(Of String, clsHBVSubBasin)()
    End Sub

    Public Sub Read()
        Using sr As New StreamReader(Path)
            Dim line As String
            Dim currentBasin As clsHBVSubBasin = Nothing

            While Not sr.EndOfStream
                line = sr.ReadLine().Trim()

                If line.StartsWith("'fileformat'") Then
                    FileFormat = Integer.Parse(GetValueFromLine(line))
                ElseIf line.StartsWith("'basin'") Then
                    currentBasin = New clsHBVSubBasin(Me.Setup, CStr(GetValueFromLine(line)))
                    Basins.Add(currentBasin.BasinName, currentBasin)
                ElseIf currentBasin IsNot Nothing Then
                    If line.StartsWith("'basindir'") Then
                        currentBasin.BasinDir = CStr(GetValueFromLine(line))
                    ElseIf line.StartsWith("'bstatus'") Then
                        currentBasin.bstatus = CStr(GetValueFromLine(line))
                    ElseIf line.StartsWith("'pstatus'") Then
                        currentBasin.pstatus = CStr(GetValueFromLine(line))
                    ElseIf line.StartsWith("'btype'") Then
                        currentBasin.btype = CStr(GetValueFromLine(line))
                    ElseIf line.StartsWith("'cregion'") Then
                        currentBasin.cregion = Integer.Parse(GetValueFromLine(line))
                    ElseIf line.StartsWith("'outletx'") Then
                        currentBasin.outletx = Double.Parse(GetValueFromLine(line))
                    ElseIf line.StartsWith("'outlety'") Then
                        currentBasin.outletx = Double.Parse(GetValueFromLine(line))
                    ElseIf line.StartsWith("'bcode'") Then
                        currentBasin.bcode = Integer.Parse(GetValueFromLine(line))
                    End If

                End If
            End While

        End Using
    End Sub

    Public Sub Write()
        Using sw As New StreamWriter(Path)
            sw.WriteLine("'fileformat' " & FileFormat)
            For Each basinPair In Basins
                Dim basin As clsHBVSubBasin = basinPair.Value
                sw.WriteLine("'basin' '" & basin.BasinName & "'")
                sw.WriteLine("'basindir' '" & basin.BasinDir & "'")
                sw.WriteLine("'bstatus' '" & basin.bstatus & "'")
                sw.WriteLine("'pstatus' '" & basin.pstatus & "'")
                sw.WriteLine("'btype' '" & basin.btype & "'")
                sw.WriteLine("'cregion' '" & basin.cregion & "'")
                sw.WriteLine("'outletx' " & basin.outletx.ToString())
                sw.WriteLine("'outlety' " & basin.outlety.ToString())
                sw.WriteLine("'bcode' " & basin.bcode.ToString())
            Next
        End Using
    End Sub
    Public Function GetValueFromLine(Line As String) As Object
        ' Replace tabs with spaces
        Line = Line.Replace(vbTab, " ")

        ' Remove all double spaces
        While Line.Contains("  ")
            Line = Line.Replace("  ", " ")
        End While

        ' Trim the line to remove leading and trailing spaces
        Line = Line.Trim()

        ' Split the line into parts using space as separator
        Dim parts As String() = Line.Split(" "c)

        ' Check if there are enough parts to get a value
        If parts.Length >= 2 Then
            Dim value As String = parts(1)

            ' Remove quotes if present
            If value.StartsWith("'") AndAlso value.EndsWith("'") Then
                value = value.Substring(1, value.Length - 2)
            End If

            ' Return the value, you might want to further process this to 
            ' convert to the appropriate data type (integer, string, etc.)
            Return value
        Else
            ' Return nothing or throw an exception if the line format is not correct
            Return Nothing
        End If
    End Function


End Class
