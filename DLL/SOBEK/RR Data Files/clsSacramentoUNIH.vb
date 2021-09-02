Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsSacramentoUNIH

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsSACRMNTO3BUNIHRecord) 'of clsSACRMNTO3BUNIHRecord
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function GetAddRecord(ID As String) As clsSACRMNTO3BUNIHRecord
        Dim myRecord As clsSACRMNTO3BUNIHRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            myRecord = Records.Item(ID.Trim.ToUpper)
        Else
            myRecord = New clsSACRMNTO3BUNIHRecord(Me.setup)
            myRecord.ID = ID
            Records.Add(ID.Trim.ToUpper, myRecord)
        End If
        Return myRecord
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each UNIH As clsSACRMNTO3BUNIHRecord In Records.Values
            UNIH.ID = Prefix & UNIH.ID
        Next
    End Sub

    Friend Function GetRecord(ByVal ID As String) As clsSACRMNTO3BUNIHRecord
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

        FileContent = Datafile.Read(casedir & "\sacrmnto.3b", "UNIH")
        For i = 1 To FileContent.Count
            Dim UNIHRecord As clsSACRMNTO3BUNIHRecord
            UNIHRecord = New clsSACRMNTO3BUNIHRecord(Me.setup)
            UNIHRecord.Read(FileContent(i))
            If Records.ContainsKey(UNIHRecord.ID.Trim.ToUpper) Then
                Me.setup.Log.AddWarning("Multiple records found for sacramento.3b UNIH definition " & UNIHRecord.ID)
            Else
                Records.Add(UNIHRecord.ID.Trim.ToUpper, UNIHRecord)
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using sacrmntoWriter As New StreamWriter(ExportDir & "\sacrmnto.3b", Append)
            For Each myRecord As clsSACRMNTO3BUNIHRecord In Records.Values
                If myRecord.InUse Then myRecord.Write(sacrmntoWriter)
            Next myRecord
            sacrmntoWriter.Close()
        End Using
    End Sub
End Class
