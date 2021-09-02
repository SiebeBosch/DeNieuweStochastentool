Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsGreenhse3B

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsGreenhse3BRecord) 'of clsGreenhse3Brecord
    Public Stats As New clsRRStats(Me.setup)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function GetAddRecord(ID As String) As clsGreenhse3BRecord
        Dim myRecord As clsGreenhse3BRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            myRecord = Records.Item(ID.Trim.ToUpper)
        Else
            myRecord = New clsGreenhse3BRecord(Me.setup)
            myRecord.ID = ID
            Records.Add(ID.Trim.ToUpper, myRecord)
        End If
        Return myRecord
    End Function


    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each GRH As clsGreenhse3BRecord In Records.Values
            GRH.ID = Prefix & GRH.ID
            If Not GRH.si = "" Then GRH.si = Prefix & GRH.si
            If Not GRH.sd = "" Then GRH.sd = Prefix & GRH.sd
        Next
    End Sub

    Friend Function GetRecord(ByVal ID As String) As clsGreenhse3BRecord
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

        FileContent = Datafile.Read(casedir & "\greenhse.3b", "GRHS")
        For i = 1 To FileContent.Count
            Dim GRHSRecord As clsGreenhse3BRecord
            GRHSRecord = New clsGreenhse3BRecord(Me.setup)
            GRHSRecord.Read(FileContent(i))
            GRHSRecord.GetTopology(myModel)
            If GRHSRecord.Check() Then
                If Not Records.ContainsKey(GRHSRecord.ID.Trim.ToUpper) Then
                    Records.Add(GRHSRecord.ID.Trim.ToUpper, GRHSRecord)
                Else
                    Me.setup.Log.AddWarning("Unpaved.3b contains multiple instances of record with ID " & GRHSRecord.ID)
                End If
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using gr3bWriter As New StreamWriter(ExportDir & "\greenhse.3b", Append)
            For Each myRecord As clsGreenhse3BRecord In Records.Values
                If myRecord.InUse Then Call myRecord.Write(gr3bWriter)
            Next myRecord
            gr3bWriter.Close()
        End Using
    End Sub

    Friend Sub Write(ByVal myPath As String, ByVal Append As Boolean)
        Using gr3bWriter As New StreamWriter(myPath, Append)
            For Each myRecord As clsGreenhse3BRecord In Records.Values
                Call myRecord.Write(gr3bWriter)
            Next myRecord
            gr3bWriter.Close()
        End Using
    End Sub


End Class

