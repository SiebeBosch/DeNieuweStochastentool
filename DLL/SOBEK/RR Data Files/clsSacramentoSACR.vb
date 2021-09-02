Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsSacramentoSACR

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsSACRMNTO3BSACRRecord) 'of clsSACRMNTO3BSACRRecord
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function GetAddRecord(ID As String) As clsSACRMNTO3BSACRRecord
        Dim myRecord As clsSACRMNTO3BSACRRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            myRecord = Records.Item(ID.Trim.ToUpper)
        Else
            myRecord = New clsSACRMNTO3BSACRRecord(Me.setup)
            myRecord.ID = ID
            Records.Add(ID.Trim.ToUpper, myRecord)
        End If
        Return myRecord
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each SACR As clsSACRMNTO3BSACRRecord In Records.Values
            SACR.ID = Prefix & SACR.ID
            If Not SACR.ca = "" Then SACR.ca = Prefix & SACR.ca
            If Not SACR.uh = "" Then SACR.uh = Prefix & SACR.uh
            If Not SACR.op = "" Then SACR.op = Prefix & SACR.op
        Next
    End Sub

    Friend Function GetRecord(ByVal ID As String) As clsSACRMNTO3BSACRRecord
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

        FileContent = Datafile.Read(casedir & "\sacrmnto.3b", "SACR")
        For i = 1 To FileContent.Count
            Dim SACRRecord As clsSACRMNTO3BSACRRecord
            SACRRecord = New clsSACRMNTO3BSACRRecord(Me.setup)
            SACRRecord.Read(FileContent(i))
            SACRRecord.GetTopology(myModel)
            If Records.ContainsKey(SACRRecord.ID.Trim.ToUpper) Then
                Me.setup.Log.AddWarning("Multiple records found for paved node " & SACRRecord.ID)
            Else
                Records.Add(SACRRecord.ID.Trim.ToUpper, SACRRecord)
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using sacrmntoWriter As New StreamWriter(ExportDir & "\sacrmnto.3b", Append)
            For Each myRecord As clsSACRMNTO3BSACRRecord In Records.Values
                If myRecord.InUse Then myRecord.Write(sacrmntoWriter)
            Next myRecord
            sacrmntoWriter.Close()
        End Using
    End Sub
End Class
