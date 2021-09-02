Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsUnpavedAlf

    Friend FileContent As New Collection 'of string
    Friend ERNSRecords As New Dictionary(Of String, clsUnpavedALFERNSRecord)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each ERNS As clsUnpavedALFERNSRecord In ERNSRecords.Values
            ERNS.ID = Prefix & ERNS.ID
            ERNS.nm = Prefix & ERNS.nm
        Next
    End Sub

    Friend Sub Read(ByVal casedir As String, ByRef myModel As ClsSobekCase)

        'leest unpaved definitions in
        Dim Datafile = New clsSobekDataFile(Me.setup)
        Dim i As Long

        FileContent = Datafile.Read(casedir & "\unpaved.alf", "ERNS")
        For i = 1 To FileContent.Count
            Dim ERNSRecord As clsUnpavedALFERNSRecord
            ERNSRecord = New clsUnpavedALFERNSRecord(Me.setup)
            ERNSRecord.Read(FileContent(i))
            If Not ERNSRecords.ContainsKey(ERNSRecord.ID.Trim.ToUpper) Then
                ERNSRecords.Add(ERNSRecord.ID.Trim.ToUpper, ERNSRecord)
            Else
                Me.setup.Log.AddError("Error: multiple instances of ERNST Drainage definition '" & ERNSRecord.ID & "' in Sobek Case " & myModel.CaseName & ".")
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using upAlfWriter As New StreamWriter(ExportDir & "\unpaved.alf", Append)
            For Each myRecord As clsUnpavedALFERNSRecord In ERNSRecords.Values
                myRecord.Write(upAlfWriter)
            Next myRecord
            upAlfWriter.Close()
        End Using
    End Sub

    Friend Sub Write(ByVal Path As String, ByVal Append As Boolean)
        Using upAlfWriter As New StreamWriter(Path, Append)
            For Each myRecord As clsUnpavedALFERNSRecord In ERNSRecords.Values
                myRecord.Write(upAlfWriter)
            Next myRecord
            upAlfWriter.Close()
        End Using
    End Sub

End Class
