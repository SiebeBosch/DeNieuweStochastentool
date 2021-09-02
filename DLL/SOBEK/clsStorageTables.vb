Public Class clsStorageTables
    'this class describes a generic storage table for complete SOBEK modelschematizations
    Dim Tables As New Dictionary(Of String, clsStorageTable)

    Public Function GetTable(ID As String) As clsStorageTable
        If Tables.ContainsKey(ID.Trim.ToUpper) Then
            Return Tables.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Public Function AddTable(ID As String, myTable As clsStorageTable, X As Double, Y As Double) As Boolean
        Try
            If Not Tables.ContainsKey(ID.Trim.ToUpper) Then
                Tables.Add(ID.Trim.ToUpper, myTable)
            Else
                Throw New Exception("Error adding storage table: table with the same ID already present: " & ID)
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
