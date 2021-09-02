Option Explicit On
Imports STOCHLIB.General
Imports System.IO

Public Class clsCFInitialData

    Friend InitialDatFLINRecords As clsInitialDatFLINRecords
    Friend GlobalRecord As clsInitialDatFLINRecord

    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase

        ' Init classes:
        InitialDatFLINRecords = New clsInitialDatFLINRecords(Me.setup, Me.SbkCase)
    End Sub

    Public Function Rename(oldID As String, newID As String) As clsInitialDatFLINRecord
        Dim newDat As New clsInitialDatFLINRecord(Me.setup)
        Dim oldDat As clsInitialDatFLINRecord
        If InitialDatFLINRecords.records.ContainsKey(oldID.Trim.ToUpper) Then
            oldDat = InitialDatFLINRecords.records.Item(oldID.Trim.ToUpper)
            newDat = oldDat.Clone(newID)
            oldDat.InUse = False
            newDat.InUse = True
            InitialDatFLINRecords.records.Add(newID.Trim.ToUpper, newDat)
        End If
        Return newDat
    End Function

    Friend Sub Write(ByRef datWriter As StreamWriter)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing initial.dat file...", 0, InitialDatFLINRecords.records.Count)
        If Not GlobalRecord Is Nothing Then GlobalRecord.writeAsGlobalRecord(datWriter)
        For Each record As clsInitialDatFLINRecord In InitialDatFLINRecords.records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, InitialDatFLINRecords.records.Count)
            record.write(datWriter)
        Next record
    End Sub

    Friend Sub Read(ByVal myStrings As Collection)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Reading initial.dat FLIN records...", 0, myStrings.Count)
        For Each myString As String In myStrings
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
            Dim myRecord As clsInitialDatFLINRecord = New clsInitialDatFLINRecord(Me.setup)
            myRecord.read(myString)
            If InitialDatFLINRecords.records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                setup.Log.AddWarning("Multiple instances of initial.dat FLIN record with ID: '" & myRecord.ID & "' found in SOBEK schematization.")
            Else
                Call InitialDatFLINRecords.records.Add(myRecord.ID.Trim.ToUpper, myRecord)
            End If
        Next myString
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        InitialDatFLINRecords.AddPrefix(Prefix)
    End Sub

    Friend Sub InitializeExport(ExportDir As String)
        If System.IO.File.Exists(ExportDir & "\initial.dat") Then System.IO.File.Delete(ExportDir & "\initial.dat")
    End Sub

    ''' <summary>
    ''' TODO: Siebe invullen
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub Export(ByVal Append As Boolean, ExportDir As String)

        Try
            Using initialDatWriter As New StreamWriter(ExportDir & "\initial.dat", Append)
                Write(initialDatWriter)
            End Using
        Catch ex As Exception
            Dim log As String = "Error in Export 1D Flow initialization data"
            Me.setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try
    End Sub

End Class
