Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsOpenwate3B

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsOpenwate3BRecord) 'of clsOpenwate3BRecord
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function GetAddRecord(ID As String) As clsOpenwate3BRecord
        Dim myRecord As clsOpenwate3BRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            myRecord = Records.Item(ID.Trim.ToUpper)
        Else
            myRecord = New clsOpenwate3BRecord(Me.setup)
            myRecord.ID = ID
            Records.Add(ID.Trim.ToUpper, myRecord)
        End If
        Return myRecord
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each OW As clsOpenwate3BRecord In Records.Values
            OW.id = Prefix & OW.id
            If Not OW.sp = "" Then OW.sp = Prefix & OW.sp
        Next
    End Sub

    Friend Function GetRecord(ByVal ID As String) As clsOpenwate3BRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            Return Records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Sub Read(ByVal casedir As String, ByRef myModel As ClsSobekCase)

        'leest unpaved definitions in
        Dim Datafile = New clsSobekDataFile(Me.setup)
        Dim i As Long

        FileContent = Datafile.Read(casedir & "\openwate.3b", "OPWA")
        For i = 1 To FileContent.Count
            Dim OPWARecord As clsOpenwate3BRecord
            OPWARecord = New clsOpenwate3BRecord(Me.setup)
            OPWARecord.Read(FileContent(i))
            OPWARecord.GetTopology(myModel)
            If OPWARecord.Check() Then
                If Records.ContainsKey(OPWARecord.id.Trim.ToUpper) Then
                    Me.setup.Log.AddWarning("Multiple records found for openwater node " & OPWARecord.id)
                Else
                    'Call OPWARecord.GetAreaIDs(myModel)         'zoek uit bij welke GPG of GFE hij hoort
                    Records.Add(OPWARecord.id.Trim.ToUpper, OPWARecord)
                End If
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using ow3bWriter As New StreamWriter(ExportDir & "\openwate.3b", Append)
            For Each myRecord As clsOpenwate3BRecord In Records.Values
                myRecord.Write(ow3bWriter)
            Next myRecord
            ow3bWriter.Close()
        End Using
    End Sub

    Friend Sub Write(ByVal myPath As String, ByVal Append As Boolean)
        Using ow3bWriter As New StreamWriter(myPath, Append)
            For Each myRecord As clsOpenwate3BRecord In Records.Values
                Call myRecord.Write(ow3bWriter)
            Next myRecord
            ow3bWriter.Close()
        End Using
    End Sub


End Class
