Public Class clsIniFileAttribute
    Public Name As String
    Dim Values As List(Of String)

    Public Sub New(myName As String)
        Name = myName
        Values = New List(Of String)
    End Sub

    Public Function CountValues() As Integer
        Return Values.Count
    End Function

    Public Sub AddValue(myValue As String)
        Values.Add(myValue)
    End Sub

    Public Function GetValues() As List(Of String)
        Return Values
    End Function

    Public Function GetValue(Optional ByVal ValIdx As Integer = 0) As String
        If Values.Count > ValIdx Then
            Return Values(ValIdx)
        Else
            Return ""
        End If
    End Function
    Public Function GetValueAsDouble(Optional ByVal ValIdx As Integer = 0) As Double
        If Values.Count > ValIdx Then
            Return Convert.ToDouble(Values(ValIdx))
        Else
            Return Double.NaN
        End If
    End Function

    Public Function GetValueAsInt32(Optional ByVal ValIdx As Integer = 0) As Int32
        If Values.Count > ValIdx Then
            Return Convert.ToInt32(Values(ValIdx))
        Else
            Return 0
        End If
    End Function

    Public Function GetValueAsInt64(Optional ByVal ValIdx As Integer = 0) As Int64
        If Values.Count > ValIdx Then
            Return Convert.ToInt64(Values(ValIdx))
        Else
            Return 0
        End If
    End Function

    Public Function GetValuesAsListOfDouble() As List(Of Double)
        Dim myList As New List(Of Double)
        For i = 0 To Values.Count - 1
            myList.Add(Convert.ToDouble(Values(i)))
        Next
        Return myList
    End Function

    Public Function ContainsValue(SearchValue As String) As Boolean
        For Each myValue As String In Values
            If myValue = SearchValue Then Return True
        Next
        Return False
    End Function
End Class
