Public Class clsIniFileChapter
    Public Name As String
    Friend Attributes As Dictionary(Of String, clsIniFileAttribute)

    Public Sub New(myName As String)
        Name = myName
        Attributes = New Dictionary(Of String, clsIniFileAttribute)
    End Sub

    Public Function AddAttribute(ByRef myAttribute As clsIniFileAttribute) As Boolean
        If Not Attributes.ContainsKey(myAttribute.Name.Trim.ToUpper) Then
            Attributes.Add(myAttribute.Name.Trim.ToUpper, myAttribute)
        End If
    End Function

End Class
