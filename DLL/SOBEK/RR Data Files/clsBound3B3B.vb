Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsBound3B3B

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsBound3B3BRecord)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each BND As clsBound3B3BRecord In Records.Values
            BND.ID = Prefix & BND.ID
            If BND.TableID <> "" Then BND.TableID = Prefix & BND.TableID
        Next
    End Sub

    Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

        'leest unpaved definitions in
        Dim Datafile = New clsSobekDataFile(Me.setup)
        Dim i As Long

        FileContent = Datafile.Read(casedir & "\bound3b.3b", "BOUN")
        For i = 1 To FileContent.Count
            Dim B3BRecord As clsBound3B3BRecord
            B3BRecord = New clsBound3B3BRecord(Me.setup)
            B3BRecord.Read(FileContent(i), True)
            If Not Records.ContainsKey(B3BRecord.ID.Trim.ToUpper) Then
                Records.Add(B3BRecord.ID.Trim.ToUpper, B3BRecord)
            Else
                Me.setup.Log.AddWarning("Double instances of bound3b.3b definition " & B3BRecord.ID & " in " & casedir)
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using b3bWriter As New StreamWriter(ExportDir & "\bound3b.3b", Append)
            For Each myRecord As clsBound3B3BRecord In Records.Values
                If myRecord.InUse Then myRecord.Write(b3bWriter)
            Next myRecord
            b3bWriter.Close()
        End Using
    End Sub

    Friend Function GetRecord(ByVal myID As String)
        If Records.ContainsKey(myID.Trim.ToUpper) Then
            Return Records.Item(myID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function


    Friend Function getAddRecord(ByVal myID As String)
        Dim myRecord As clsBound3B3BRecord
        If Records.ContainsKey(myID.Trim.ToUpper) Then
            Return Records.Item(myID.Trim.ToUpper)
        Else
            myRecord = New clsBound3B3BRecord(Me.setup)
            myRecord.ID = myID
            Records.Add(myID.Trim.ToUpper, myRecord)
            Return myRecord
        End If
    End Function

End Class

