Option Explicit On

Imports System.IO
Imports STOCHLIB.General

Public Class clsFrictionDatCRFRRecords

    Friend records As New Dictionary(Of String, clsFrictionDatCRFRRecord)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each CRFR As clsFrictionDatCRFRRecord In records.Values
            CRFR.ID = Prefix & CRFR.ID
            CRFR.cs = Prefix & CRFR.cs
        Next
    End Sub

    Friend Function getRecord(ID As String) As clsFrictionDatCRFRRecord
        If records.ContainsKey(ID.Trim.ToUpper) Then
            Return records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Sub Write(ByRef datWriter As StreamWriter)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing friction.dat file...", 0, records.Count)
        'schrijf eerst de globale waarde
        For Each record As clsFrictionDatCRFRRecord In records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, records.Count)
            If record.InUse Then record.write(datWriter, False)
        Next record
    End Sub


    Friend Function Read(ByVal myStrings As Collection) As Boolean
        Try
            Dim i As Integer = 0
            Me.setup.GeneralFunctions.UpdateProgressBar("Reading cross section friction records...", 0, myStrings.Count)
            For Each myString As String In myStrings
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
                Dim myRecord As clsFrictionDatCRFRRecord = New clsFrictionDatCRFRRecord(Me.setup)
                If Not myRecord.Read(myString) Then
                    Me.setup.Log.AddError("Error reading friction.dat CRFR Record: " & myString)
                Else
                    myRecord.InUse = True
                    If records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                        setup.Log.AddWarning("Multiple instances of friction record " & myRecord.ID & " found in SOBEK schematization.")
                    Else
                        Call records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                    End If
                End If
            Next myString
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function Read of class clsFrictionDatCRFRRecords")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

End Class
