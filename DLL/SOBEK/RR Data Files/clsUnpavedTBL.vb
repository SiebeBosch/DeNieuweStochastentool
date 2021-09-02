Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsUnpavedTBL

    Public FileContent As New Collection 'of string
    Public IG_TRecords As New Dictionary(Of String, clsUnpavedTBLIG_TRecord)
    Public SC_TRecords As New Dictionary(Of String, clsUnpavedTBLSC_TRecord)
    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each TBL As clsUnpavedTBLIG_TRecord In IG_TRecords.Values
            TBL.ID = Prefix & TBL.ID
            TBL.nm = Prefix & TBL.nm
        Next
        For Each TBL As clsUnpavedTBLSC_TRecord In SC_TRecords.Values
            TBL.ID = Prefix & TBL.ID
            TBL.nm = Prefix & TBL.nm
        Next
    End Sub

    Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

        'leest unpaved definitions in
        Dim Datafile = New clsSobekDataFile(Me.setup)
        Dim i As Long

        FileContent = Datafile.Read(casedir & "\unpaved.tbl", "IG_T")
        For i = 1 To FileContent.Count
            Dim TBLRecord As clsUnpavedTBLIG_TRecord
            TBLRecord = New clsUnpavedTBLIG_TRecord(Me.setup)
            TBLRecord.Read(FileContent(i))
            IG_TRecords.Add(TBLRecord.ID.Trim.ToUpper, TBLRecord)
        Next
        FileContent = Datafile.Read(casedir & "\unpaved.tbl", "SC_T")
        For i = 1 To FileContent.Count
            Dim TBLRecord As clsUnpavedTBLSC_TRecord
            TBLRecord = New clsUnpavedTBLSC_TRecord(Me.setup)
            TBLRecord.Read(FileContent(i))
            SC_TRecords.Add(TBLRecord.ID.Trim.ToUpper, TBLRecord)
        Next

    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using upTBLWriter As New StreamWriter(ExportDir & "\unpaved.tbl", Append)
            For Each myRecord As clsUnpavedTBLIG_TRecord In IG_TRecords.Values
                Call myRecord.Write(upTBLWriter)
            Next myRecord
            For Each myRecord As clsUnpavedTBLSC_TRecord In SC_TRecords.Values
                Call myRecord.Write(upTBLWriter)
            Next myRecord
            upTBLWriter.Close()
        End Using
    End Sub

    Friend Sub Write(ByVal myPath As String, ByVal Append As Boolean)
        Using upTBLWriter As New StreamWriter(myPath, Append)
            For Each myRecord As clsUnpavedTBLIG_TRecord In IG_TRecords.Values
                Call myRecord.Write(upTBLWriter)
            Next myRecord
            For Each myRecord As clsUnpavedTBLSC_TRecord In SC_TRecords.Values
                Call myRecord.Write(upTBLWriter)
            Next myRecord
            upTBLWriter.Close()
        End Using
    End Sub
End Class
