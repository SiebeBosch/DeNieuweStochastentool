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
                    FileFormat = Integer.Parse(GetNumericalValueFromLine(line))
                ElseIf line.StartsWith("'basin'") Then
                    currentBasin = New clsHBVSubBasin(Me.Setup, GetStringValueFromLine(line))
                    Basins.Add(currentBasin.BasinName, currentBasin)
                ElseIf currentBasin IsNot Nothing Then
                    If line.StartsWith("'basindir'") Then
                        currentBasin.BasinDir = GetStringValueFromLine(line)
                    ElseIf line.StartsWith("'bstatus'") Then
                        currentBasin.bstatus = GetStringValueFromLine(line)
                    ElseIf line.StartsWith("'pstatus'") Then
                        currentBasin.pstatus = GetStringValueFromLine(line)
                    ElseIf line.StartsWith("'btype'") Then
                        currentBasin.btype = GetStringValueFromLine(line)
                    ElseIf line.StartsWith("'cregion'") Then
                        currentBasin.cregion = GetNumericalValueFromLine(line)
                    ElseIf line.StartsWith("'outletx'") Then
                        currentBasin.outletx = GetNumericalValueFromLine(line)
                    ElseIf line.StartsWith("'outlety'") Then
                        currentBasin.outletx = GetNumericalValueFromLine(line)
                    ElseIf line.StartsWith("'bcode'") Then
                        currentBasin.bcode = GetNumericalValueFromLine(line)
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
    Private Function GetNumericalValueFromLine(line As String) As Double
        Try
            Dim parts As String() = line.Split("'")
            If parts.Length > 1 Then
                ' Assuming the number is always after the second apostrophe and followed by a space.
                Dim numberPart As String = parts(1).Trim()
                ' Extract the number from the string.
                Dim number As Integer = Double.Parse(numberPart)
                Return number
            Else
                ' Return a default value or handle the error as appropriate
                Return 0
            End If
        Catch ex As Exception
            ' Handle parsing error or return a default value
            Return 0
        End Try
    End Function

    Private Function GetStringValueFromLine(line As String) As String
        Return line.Split("'")(3).Trim()
    End Function
End Class
