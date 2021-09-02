
Option Explicit On

Imports System.IO
Imports STOCHLIB.General
Public Class clsFrictionDatD2FRRecords

    Friend records As New Dictionary(Of String, clsFrictionDatD2FRRecord)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function getRecord(ID As String) As clsFrictionDatD2FRRecord
        If records.ContainsKey(ID.Trim.ToUpper) Then
            Return records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function


    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each BDFR As clsFrictionDatD2FRRecord In records.Values
            If Not BDFR.ID = "0" Then
                BDFR.ID = Prefix & BDFR.ID
                BDFR.ci = Prefix & BDFR.ci
            End If
        Next
    End Sub

    Friend Sub Write(ByRef datWriter As StreamWriter)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing friction.dat file...", 0, records.Count)
        For Each record As clsFrictionDatD2FRRecord In records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, records.Count)
            If record.InUse Then record.write(datWriter)
        Next record
    End Sub


    Friend Sub Read(ByVal myStrings As Collection)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Reading friction.dat D2FR records...", 0, myStrings.Count)
        For Each myString As String In myStrings
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
            Dim myRecord As clsFrictionDatD2FRRecord = New clsFrictionDatD2FRRecord(Me.setup)
            myRecord.read(Me.setup, myString)
            myRecord.InUse = True

            If records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                setup.Log.AddWarning("Multiple instances of friction record " & myRecord.ID & " found in SOBEK schematization.")
            Else
                Call records.Add(myRecord.ID.Trim.ToUpper, myRecord)
            End If
        Next myString
    End Sub


End Class



