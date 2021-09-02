Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsSacramentoCAPS

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsSACRMNTO3BCAPSRecord) 'of clsSACRMNTO3BCAPSRecord
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function GetAddRecord(ID As String) As clsSACRMNTO3BCAPSRecord
        Dim myRecord As clsSACRMNTO3BCAPSRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            myRecord = Records.Item(ID.Trim.ToUpper)
        Else
            myRecord = New clsSACRMNTO3BCAPSRecord(Me.setup)
            myRecord.ID = ID
            Records.Add(ID.Trim.ToUpper, myRecord)
        End If
        Return myRecord
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each CAPS As clsSACRMNTO3BCAPSRecord In Records.Values
            CAPS.ID = Prefix & CAPS.ID
        Next
    End Sub

    Friend Function GetRecord(ByVal ID As String) As clsSACRMNTO3BCAPSRecord
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

        FileContent = Datafile.Read(casedir & "\sacrmnto.3b", "CAPS")
        For i = 1 To FileContent.Count
            Dim CAPSRecord As clsSACRMNTO3BCAPSRecord
            CAPSRecord = New clsSACRMNTO3BCAPSRecord(Me.setup)
            CAPSRecord.Read(FileContent(i))
            If Records.ContainsKey(CAPSRecord.ID.Trim.ToUpper) Then
                Me.setup.Log.AddWarning("Multiple records found for sacramento.3b CAPS deefinition " & CAPSRecord.ID)
            Else
                Records.Add(CAPSRecord.ID.Trim.ToUpper, CAPSRecord)
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using sacrmntoWriter As New StreamWriter(ExportDir & "\sacrmnto.3b", Append)
            For Each myRecord As clsSACRMNTO3BCAPSRecord In Records.Values
                If myRecord.InUse Then myRecord.Write(sacrmntoWriter)
            Next myRecord
            sacrmntoWriter.Close()
        End Using
    End Sub
End Class
