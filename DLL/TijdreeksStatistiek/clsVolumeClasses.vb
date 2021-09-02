Imports STOCHLIB.General
Public Class clsVolumeClasses
    Dim Duration As Integer
    Dim Parameter As String

    Public Classes As Dictionary(Of String, clsVolumeClass)


    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup, ByVal myDuration As Integer, ByVal myParameter As String)
        Setup = mySetup
        Duration = myDuration
        Parameter = myParameter
        Classes = New Dictionary(Of String, clsVolumeClass)
    End Sub

    Public Function GetByVolume(myVolume As Double) As clsVolumeClass
        For Each myVol As clsVolumeClass In Classes.Values
            If myVol.GetLBound <= myVolume AndAlso myVol.GetUBound > myVolume Then
                Return myVol
            End If
        Next
        Return Nothing
    End Function

    Public Function ReadFromDatabase(TableName As String) As Boolean
        Try
            Dim query As String = "SELECT VOLUMECLASS, LBOUND, UBOUND, REPRESENTATIVE FROM " & TableName & " WHERE PARAMETER='" & Parameter & "' AND DURATION=" & Duration & ";"
            Dim dt As New DataTable, i As Integer, Vol As clsVolumeClass
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, dt, False)
            For i = 0 To dt.Rows.Count - 1
                Vol = New clsVolumeClass(dt.Rows(i)(0), dt.Rows(i)(1), dt.Rows(i)(2), dt.Rows(i)(3))
                Classes.Add(Vol.getID.Trim.ToUpper, Vol)
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
