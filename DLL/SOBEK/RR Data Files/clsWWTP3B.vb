Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsWWTP3B

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsWWTP3BRecord)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each WWTP As clsWWTP3BRecord In Records.Values
            WWTP.ID = Prefix & WWTP.ID
            If WWTP.TableID <> "" Then WWTP.TableID = Prefix & WWTP.TableID
        Next
    End Sub

    Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

        'leest unpaved definitions in
        Dim Datafile = New clsSobekDataFile(Me.setup)
        Dim i As Long

        FileContent = Datafile.Read(casedir & "\wwtp.3b", "WWTP")
        For i = 1 To FileContent.Count
            Dim WWTP3BRecord As clsWWTP3BRecord
            WWTP3BRecord = New clsWWTP3BRecord(Me.setup)
            WWTP3BRecord.Read(FileContent(i), True)
            If Not Records.ContainsKey(WWTP3BRecord.ID.Trim.ToUpper) Then
                Records.Add(WWTP3BRecord.ID.Trim.ToUpper, WWTP3BRecord)
            Else
                Me.setup.Log.AddWarning("Double instances of wwtp3b.3b definition " & WWTP3BRecord.ID & " in " & casedir)
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using wwtp3bWriter As New StreamWriter(ExportDir & "\wwtp.3b", Append)
            For Each myRecord As clsWWTP3BRecord In Records.Values
                If myRecord.InUse Then myRecord.Write(wwtp3bWriter)
            Next myRecord
            wwtp3bWriter.Close()
        End Using
    End Sub

    Friend Function getAddRecord(ByVal myID As String)
        Dim myRecord As clsWWTP3BRecord
        If Records.ContainsKey(myID.Trim.ToUpper) Then
            Return Records.Item(myID.Trim.ToUpper)
        Else
            myRecord = New clsWWTP3BRecord(Me.setup)
            myRecord.ID = myID
            Records.Add(myID.Trim.ToUpper, myRecord)
            Return myRecord
        End If
    End Function

End Class

