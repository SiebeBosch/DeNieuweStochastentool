Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Friend Class clsLateralDatFLNORecords

    Friend records As New Dictionary(Of String, clsLateralDatFLNORecord)
    Private setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each FLNO As clsLateralDatFLNORecord In records.Values
            FLNO.ID = Prefix & FLNO.ID
            If Not FLNO.Table Is Nothing Then FLNO.Table.AddPrefix(Prefix)
        Next
    End Sub

    Friend Sub Write(ByRef datWriter As StreamWriter)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing lateral.dat file...", 0, records.Count)
        For Each record As clsLateralDatFLNORecord In records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, records.Count)
            If record.InUse Then record.Write(datWriter)
        Next record
    End Sub
    Friend Function Read(ByVal myStrings As Collection) As Boolean
        Dim i As Integer = 0
        Dim myString As String = ""
        Try
            Me.setup.GeneralFunctions.UpdateProgressBar("Reading lateral.dat FLNO records...", 0, myStrings.Count)
            For Each myString In myStrings

                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
                Dim myRecord As clsLateralDatFLNORecord = New clsLateralDatFLNORecord(Me.setup, Me.SbkCase)
                myRecord.Read(myString)

                If records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                    Me.setup.Log.AddWarning("Multiple instances of lateral node " & myRecord.ID & " found in SOBEK schematization.")
                Else
                    Call records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                End If
            Next myString
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error reading Lateral.dat FLNO Record " & myString)
            Return False
        End Try
    End Function
End Class
