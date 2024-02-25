Public Class clsTimeTableRecord

    Public Datum As DateTime
    'edit Siebe: 19-05-2016. Changed Double into Single for memory reasons. 
    Public Fields As New Dictionary(Of GeneralFunctions.enmModelParameter, Single) 'index 0-based

    'Note: if multiple models are used, the folowing indices are reserved for the models:

    Public Sub AddToValue(ModelParameter As GeneralFunctions.enmModelParameter, ByVal myVal As Single)
        Dim i As Integer
        If Not Fields.ContainsKey(ModelParameter) Then
            Fields.Add(ModelParameter, 0)
        End If
        Fields.Item(ModelParameter) += myVal
    End Sub

    Public Sub SetValue(ByVal ModelParameter As GeneralFunctions.enmModelParameter, ByVal myVal As Single)
        If Not Fields.ContainsKey(ModelParameter) Then
            Fields.Add(ModelParameter, 0)
        End If
        Fields.Item(ModelParameter) = myVal
    End Sub

    Public Sub ClearValue(ModelParameter As GeneralFunctions.enmModelParameter)
        Fields.Item(ModelParameter) = 0
    End Sub

    Public Function GetValue(ModelParameter As GeneralFunctions.enmModelParameter) As Single
        If Fields.ContainsKey(ModelParameter) Then
            Return Fields.Item(ModelParameter)
        Else
            Return 0
        End If
    End Function

End Class
