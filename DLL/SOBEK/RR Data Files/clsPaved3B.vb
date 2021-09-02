Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsPaved3B

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsPaved3BRecord) 'of clsPaved3BRecord
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function GetAddRecord(ID As String) As clsPaved3BRecord
        Dim myRecord As clsPaved3BRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            myRecord = Records.Item(ID.Trim.ToUpper)
        Else
            myRecord = New clsPaved3BRecord(Me.setup)
            myRecord.ID = ID
            Records.Add(ID.Trim.ToUpper, myRecord)
        End If
        Return myRecord
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each PAV As clsPaved3BRecord In Records.Values
            PAV.ID = Prefix & PAV.ID
            If Not PAV.sd = "" Then PAV.sd = Prefix & PAV.sd
            If Not PAV.dw = "" Then PAV.dw = Prefix & PAV.dw
            If Not PAV.PumpCapTable = "" Then PAV.PumpCapTable = Prefix & PAV.PumpCapTable
        Next
    End Sub

    Friend Function GetRecord(ByVal ID As String) As clsPaved3BRecord
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

        FileContent = Datafile.Read(casedir & "\paved.3b", "PAVE")
        For i = 1 To FileContent.Count
            Dim PAVERecord As clsPaved3BRecord
            PAVERecord = New clsPaved3BRecord(Me.setup)
            PAVERecord.Read(FileContent(i))
            PAVERecord.GetTopology(myModel)
            If PAVERecord.Check() Then
                If Records.ContainsKey(PAVERecord.ID.Trim.ToUpper) Then
                    Me.setup.Log.AddWarning("Multiple records found for paved node " & PAVERecord.ID)
                End If
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using pv3bWriter As New StreamWriter(ExportDir & "\paved.3b", Append)
            For Each myRecord As clsPaved3BRecord In Records.Values
                If myRecord.InUse Then myRecord.Write(pv3bWriter)
            Next myRecord
            pv3bWriter.Close()
        End Using
    End Sub

    Friend Sub Write(ByVal myPath As String, ByVal Append As Boolean)
        Using pv3bWriter As New StreamWriter(myPath, Append)
            For Each myRecord As clsPaved3BRecord In Records.Values
                Call myRecord.Write(pv3bWriter)
            Next myRecord
            pv3bWriter.Close()
        End Using
    End Sub


End Class
