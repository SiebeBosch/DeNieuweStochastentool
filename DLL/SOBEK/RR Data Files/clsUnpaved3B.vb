Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsUnpaved3B

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsUnpaved3BRecord) 'of clsUnpaved3BRecord
    Public Stats As New clsRRStats(Me.Setup)

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Function GetAddRecord(ID As String) As clsUnpaved3BRecord
        Dim new3B As clsUnpaved3BRecord
        new3B = GetRecord(ID.Trim.ToUpper)
        If new3B Is Nothing Then
            new3B = New clsUnpaved3BRecord(Me.Setup)
            new3B.ID = ID
            Records.Add(new3B.ID.Trim.ToUpper, new3B)
        End If
        Return new3B
    End Function


    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each Node As clsUnpaved3BRecord In Records.Values
            Node.ID = Prefix & Node.ID
            If Not Node.ed = "" Then Node.ed = Prefix & Node.ed
            If Not Node.ad = "" Then Node.ad = Prefix & Node.ad
            If Not Node.SCurve = "" Then Node.SCurve = Prefix & Node.SCurve
            If Not Node.SP = "" Then Node.SP = Prefix & Node.SP
            If Not Node.sd = "" Then Node.sd = Prefix & Node.sd
            If Not Node.ic = "" Then Node.ic = Prefix & Node.ic
        Next
    End Sub

    Friend Function GetRecord(ByVal ID As String) As clsUnpaved3BRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            Return Records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Function Read() As Boolean
        Try
            'leest unpaved definitions in
            Dim Datafile = New clsSobekDataFile(Me.Setup)
            Dim i As Long

            FileContent = Datafile.Read(SbkCase.CaseDir & "\unpaved.3b", "UNPV")
            For i = 1 To FileContent.Count
                Dim UNPVRecord As clsUnpaved3BRecord
                UNPVRecord = New clsUnpaved3BRecord(Me.Setup)
                UNPVRecord.Read(FileContent(i))
                UNPVRecord.GetTopology(SbkCase)
                If UNPVRecord.Check() Then
                    If Not Records.ContainsKey(UNPVRecord.ID.Trim.ToUpper) Then
                        Records.Add(UNPVRecord.ID.Trim.ToUpper, UNPVRecord)
                    Else
                        Me.Setup.Log.AddWarning("Unpaved.3b contains multiple instances of record with ID " & UNPVRecord.ID)
                    End If
                Else
                    Me.Setup.Log.AddError("Error: could not check unpaved.3b record for node " & UNPVRecord.ID)
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using up3bWriter As New StreamWriter(ExportDir & "\unpaved.3b", Append)
            For Each myRecord As clsUnpaved3BRecord In Records.Values
                If myRecord.InUse Then Call myRecord.Write(up3bWriter)
            Next myRecord
            up3bWriter.Close()
        End Using
    End Sub

    Friend Sub Write(ByVal myPath As String, ByVal Append As Boolean)
        Using up3bWriter As New StreamWriter(myPath, Append)
            For Each myRecord As clsUnpaved3BRecord In Records.Values
                Call myRecord.Write(up3bWriter)
            Next myRecord
            up3bWriter.Close()
        End Using
    End Sub


End Class
