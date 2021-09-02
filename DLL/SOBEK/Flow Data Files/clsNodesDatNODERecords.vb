Imports System.IO
Imports STOCHLIB.General

Public Class clsNodesDatNODERecords
    Friend records As New Dictionary(Of String, clsNodesDatNODERecord)
    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Public Function GetRecord(ByVal ID As String) As clsNodesDatNODERecord
        Try
            If records.ContainsKey(ID.Trim.ToUpper) Then
                Return records.Item(ID.Trim.ToUpper)
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function GetRecord while retrieving nodes.dat record for node " & ID)
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each NODE As clsNodesDatNODERecord In records.Values
            NODE.ID = Prefix & NODE.ID
            If Not NODE.ctssTable Is Nothing Then NODE.ctssTable.AddPrefix(Prefix)
            If Not NODE.ctswTable Is Nothing Then NODE.ctswTable.AddPrefix(Prefix)
        Next
    End Sub
    Friend Sub Read(ByVal myStrings As Collection)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Reading nodes.dat file...", 0, myStrings.Count)
        For Each myString As String In myStrings
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
            Dim myRecord As clsNodesDatNODERecord = New clsNodesDatNODERecord(Me.setup)
            myRecord.Read(myString)

            If records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                Me.setup.Log.AddWarning("Multiple instances of " & myRecord.ID & " found in nodes.dat.")
            Else
                Call records.Add(myRecord.ID.Trim.ToUpper, myRecord)
            End If
        Next myString
    End Sub

    Public Sub Write(ByRef datWriter As StreamWriter)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing nodes.dat file...", 0, records.Count)
        For Each myrecord As clsNodesDatNODERecord In records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, records.Count)
            If myrecord.InUse Then myrecord.Write(datWriter)
        Next myrecord
    End Sub


    Public Sub AddFromStorageTable(ByVal ID As String, ByRef StorageTable As clsSobekTable)
        Dim myRecord As New clsNodesDatNODERecord(Me.setup)
        Dim i As Long

        myRecord.ID = ID
        myRecord.ty = 1
        For i = 0 To StorageTable.XValues.Count - 1
            myRecord.ctswTable.AddDataPair(2, StorageTable.XValues.Values(i), StorageTable.Values1.Values(i))
        Next
        records.Add(myRecord.ID, myRecord)

    End Sub

End Class
