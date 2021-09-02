Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsValveTabVLVERecords
    Friend Records As New Dictionary(Of String, clsValveTabVLVERecord)
    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Sub InitializeExport(ExportDir As String)
        If System.IO.File.Exists(ExportDir & "\valve.tab") Then System.IO.File.Delete(ExportDir & "\valve.tab")
    End Sub

    Friend Sub Read(ByVal myStrings As Collection)
        Dim i As Integer = 0
        Dim Found As Boolean = False

        Try
            Me.setup.GeneralFunctions.UpdateProgressBar("Reading culvert valve definitions...", 0, myStrings.Count)
            For Each myString As String In myStrings
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
                Dim myRecord As clsValveTabVLVERecord = New clsValveTabVLVERecord(Me.setup)
                myRecord.Read(myString)
                myRecord.InUse = True
                Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
            Next myString
        Catch ex As Exception
            Me.setup.Log.AddError("Error in sub Read of class clsValveTabVLVERecords")
        End Try

    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing valve.tab file...", 0, Records.Count)
        Using tabWriter As New StreamWriter(ExportDir & "\valve.tab", Append)
            For Each myRecord As clsValveTabVLVERecord In Records.Values
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                If myRecord.InUse Then myRecord.write(tabWriter)
            Next
        End Using
    End Sub

    Friend Function GetAdd(ByVal ID As String) As clsValveTabVLVERecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            Return Records.Item(ID.Trim.ToUpper)
        Else
            Dim newRecord As New clsValveTabVLVERecord(Me.setup)
            newRecord = BuildDefault()
            newRecord.ID = ID
            newRecord.nm = ID
            Records.Add(newRecord.ID.Trim.ToUpper, newRecord)
            Return newRecord
        End If
    End Function

    Private Function BuildDefault() As clsValveTabVLVERecord
        Dim newRecord As New clsValveTabVLVERecord(Me.setup)
        newRecord.InUse = True
        newRecord.ltlcTable.AddDataPair(2, 0, 2.1)
        newRecord.ltlcTable.AddDataPair(2, 0.1, 1.96)
        newRecord.ltlcTable.AddDataPair(2, 0.2, 1.8)
        newRecord.ltlcTable.AddDataPair(2, 0.3, 1.74)
        newRecord.ltlcTable.AddDataPair(2, 0.4, 1.71)
        newRecord.ltlcTable.AddDataPair(2, 0.5, 1.71)
        newRecord.ltlcTable.AddDataPair(2, 0.6, 1.71)
        newRecord.ltlcTable.AddDataPair(2, 0.7, 1.64)
        newRecord.ltlcTable.AddDataPair(2, 0.8, 1.51)
        newRecord.ltlcTable.AddDataPair(2, 0.9, 1.36)
        newRecord.ltlcTable.AddDataPair(2, 1.0, 1.19)
        Return newRecord
    End Function

End Class
