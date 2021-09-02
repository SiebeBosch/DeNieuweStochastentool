Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsSacramentoOPAR

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsSACRMNTO3BOPARRecord) 'of clsSACRMNTO3BOPARRecord
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function GetAddRecord(ID As String) As clsSACRMNTO3BOPARRecord
        Dim myRecord As clsSACRMNTO3BOPARRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            myRecord = Records.Item(ID.Trim.ToUpper)
        Else
            myRecord = New clsSACRMNTO3BOPARRecord(Me.setup)
            myRecord.ID = ID
            Records.Add(ID.Trim.ToUpper, myRecord)
        End If
        Return myRecord
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each OPAR As clsSACRMNTO3BOPARRecord In Records.Values
            OPAR.ID = Prefix & OPAR.ID
        Next
    End Sub

    Friend Function GetRecord(ByVal ID As String) As clsSACRMNTO3BOPARRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            Return Records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

        'leest unpaved definitions in
        Dim Datafile = New clsSobekDataFile(Me.setup)
        Dim i As Long

        FileContent = Datafile.Read(casedir & "\sacrmnto.3b", "OPAR")
        For i = 1 To FileContent.Count
            Dim OPARRecord As clsSACRMNTO3BOPARRecord
            OPARRecord = New clsSACRMNTO3BOPARRecord(Me.setup)
            OPARRecord.Read(FileContent(i))
            If Records.ContainsKey(OPARRecord.ID.Trim.ToUpper) Then
                Me.setup.Log.AddWarning("Multiple records found for sacramento.3b OPAR definition " & OPARRecord.ID)
            Else
                Records.Add(OPARRecord.ID.Trim.ToUpper, OPARRecord)
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using sacrmntoWriter As New StreamWriter(ExportDir & "\sacrmnto.3b", Append)
            For Each myRecord As clsSACRMNTO3BOPARRecord In Records.Values
                If myRecord.InUse Then myRecord.Write(sacrmntoWriter)
            Next myRecord
            sacrmntoWriter.Close()
        End Using
    End Sub
End Class
