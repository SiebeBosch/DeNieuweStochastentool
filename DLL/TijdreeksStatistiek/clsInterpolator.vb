Imports System.Collections.Generic
Imports System.Data
Imports System.Linq

Public Class clsInterpolator
    Friend ReadOnly _xValues As New List(Of Double)
    Friend ReadOnly _yValues As New List(Of Double)

    Public Sub New(data As DataRow(), xColumnName As String, yColumnName As String)
        For Each row As DataRow In data
            _xValues.Add(CDbl(row(xColumnName)))
            _yValues.Add(CDbl(row(yColumnName)))
        Next
        ' Ensure the data is sorted by x values
        Dim sortedIndices = Enumerable.Range(0, _xValues.Count).ToArray()
        Array.Sort(sortedIndices, Function(a, b) _xValues(a).CompareTo(_xValues(b)))
        _xValues = sortedIndices.Select(Function(i) _xValues(i)).ToList()
        _yValues = sortedIndices.Select(Function(i) _yValues(i)).ToList()
    End Sub

    Public Function Interpolate(x As Double) As Double
        If x <= _xValues(0) Then
            Return _yValues(0)
        ElseIf x >= _xValues(_xValues.Count - 1) Then
            Return _yValues(_yValues.Count - 1)
        Else
            Dim index As Integer = _xValues.BinarySearch(x)
            If index < 0 Then
                index = -(index + 1)
            End If
            Dim x1 As Double = _xValues(index - 1)
            Dim x2 As Double = _xValues(index)
            Dim y1 As Double = _yValues(index - 1)
            Dim y2 As Double = _yValues(index)
            Return y1 + (y2 - y1) * (x - x1) / (x2 - x1)
        End If
    End Function
End Class
