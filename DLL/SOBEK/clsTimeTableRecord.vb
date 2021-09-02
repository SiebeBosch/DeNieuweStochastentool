Public Class clsTimeTableRecord

    Public Datum As DateTime
    'edit Siebe: 19-05-2016. Changed Double into Single for memory reasons. 
    Public Fields As New Dictionary(Of Integer, Single) 'index 0-based

    'Note: if multiple models are used, the folowing indices are reserved for the models:
    'FieldIdx = 0: SOBEK and Wageningenmodel (m3/s)
    'FieldIdx = 1: SOBEK Sewage Overflows (m3/s)
    'FieldIdx = 2: Precipitation (mm/h)

    Public Sub AddToValue(ByVal FieldIdx As Integer, ByVal myVal As Single)
        Dim i As Integer
        If Not Fields.ContainsKey(FieldIdx) Then
            For i = Fields.Count To FieldIdx
                Fields.Add(i, 0)
            Next
        End If
        Fields.Item(FieldIdx) += myVal
    End Sub

    Public Sub SetValue(ByVal FieldIdx As Integer, ByVal myVal As Single)
        Dim i As Integer
        If Not Fields.ContainsKey(FieldIdx) Then
            For i = Fields.Count To FieldIdx
                Fields.Add(i, 0)
            Next
        End If
        Fields.Item(FieldIdx) = myVal
    End Sub

    Public Sub ClearValue(ByVal myIdx As Integer)
        Fields.Item(myIdx) = 0
    End Sub

    Public Function GetValue(ByVal myIdx As Integer) As Single
        If Fields.ContainsKey(myIdx) Then
            Return Fields.Item(myIdx)
        Else
            Return 0
        End If
    End Function

End Class
