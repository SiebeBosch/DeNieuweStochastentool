Public Class clsDIMRScenario
    Public Name As String
    Public ClassName As String

    Public Operations As New List(Of clsDIMRFileOperation)

    Public Sub AddOperation(ByRef myOperation As clsDIMRFileOperation)
        Operations.Add(myOperation)
    End Sub

End Class
