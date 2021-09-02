Imports STOCHLIB.General
Public Class clsBoundaries

    Friend Boundaries As New Dictionary(Of String, clsBoundary)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function GetBoundary(ByVal ID As String) As clsBoundary
        If Boundaries.ContainsKey(ID.Trim.ToUpper) Then
            Return Boundaries(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

End Class
