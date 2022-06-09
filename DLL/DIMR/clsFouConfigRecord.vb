Public Class clsFouConfigRecord
    Dim var As String
    Dim tsrts As Integer
    Dim sstop As Integer
    Dim numcyc As Integer
    Dim knfac As Double '??
    Dim v0plu As Integer
    Dim layno As Integer
    Dim elp As String

    Public Sub New(myvar As String, mytsrts As Integer, mysstop As Integer, mynumcyc As Integer, myknfac As Double, myv0plu As String, mylayno As Integer, myelp As String)
        var = myvar
        tsrts = mytsrts
        sstop = mysstop
        numcyc = mynumcyc
        knfac = myknfac
        v0plu = myv0plu
        layno = mylayno
        elp = myelp
    End Sub

End Class
