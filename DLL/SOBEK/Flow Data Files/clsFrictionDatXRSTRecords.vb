Option Explicit On

Imports System.IO
Imports STOCHLIB.General

Public Class clsFrictionDatXRSTRecords

    Friend records As New Dictionary(Of String, clsFrictionDatXRSTRecord)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each STFR As clsFrictionDatXRSTRecord In records.Values
            STFR.ID = Prefix & STFR.ID
            STFR.nm = Prefix & STFR.nm
        Next
    End Sub
    Friend Sub Write(ByRef datWriter As StreamWriter)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing structure friction records...", 0, records.Count)
        'schrijf eerst de globale waarde
        For Each record As clsFrictionDatXRSTRecord In records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, records.Count)
            If record.InUse Then record.write(datWriter)
        Next record
    End Sub

    Friend Sub Read(ByVal myStrings As Collection)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Reading extra friction structure records...", 0, myStrings.Count)
        For Each myString As String In myStrings
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
            Dim myRecord As clsFrictionDatXRSTRecord = New clsFrictionDatXRSTRecord(Me.setup)
            myRecord.Read(myString)
            myRecord.InUse = True

            If records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                setup.Log.AddWarning("Multiple instances of friction record " & myRecord.ID & " found in SOBEK schematization.")
            Else
                Call records.Add(myRecord.ID.Trim.ToUpper, myRecord)
            End If
        Next myString
    End Sub
End Class
