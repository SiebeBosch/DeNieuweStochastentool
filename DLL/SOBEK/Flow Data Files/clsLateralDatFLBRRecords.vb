Option Explicit On

Imports System.IO
Imports STOCHLIB.General

Friend Class clsLateralDatFLBRRecords

    Friend records As New Dictionary(Of String, clsLateralDatFLBRRecord)

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each record As clsLateralDatFLBRRecord In records.Values
            record.ID = Prefix & record.ID
            If Not record.ID = "" Then record.LibTableID = Prefix & record.LibTableID
            If Not record.TimeTable Is Nothing Then record.TimeTable.AddPrefix(Prefix)
        Next
    End Sub

    Friend Sub Write(ByRef datWriter As StreamWriter)
        Dim i As Integer = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Writing lateral.dat file...", 0, records.Count)
        For Each record As clsLateralDatFLBRRecord In records.Values
            i += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, records.Count)
            If record.InUse Then record.Write(datWriter)
        Next record
    End Sub

    Friend Function Read(ByVal myStrings As Collection) As Boolean
        Try
            Dim i As Integer = 0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading lateral.dat FLBR records...", 0, myStrings.Count)
            For Each myString As String In myStrings
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
                Dim myRecord As clsLateralDatFLBRRecord = New clsLateralDatFLBRRecord(Me.Setup, Me.SbkCase)
                myRecord.Read(myString)

                If records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                    Setup.Log.AddWarning("Multiple instances of lateral node " & myRecord.ID & " found in SOBEK schematization.")
                Else
                    Call records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                End If
            Next myString
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function Read of class clsLateralDatFLBRRecords.")
            Return False
        End Try
    End Function
End Class
