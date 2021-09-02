Option Explicit On
Imports System.Windows.Forms
Imports System.IO
Imports STOCHLIB.General

Friend Class clsControlDefRecords
    Friend Records As New Dictionary(Of String, clsControlDefRecord)
    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each Con As clsControlDefRecord In Records.Values
            Con.ID = Prefix & Con.ID
            If Con.SetPointTableID <> "" Then Con.SetPointTableID = Prefix & Con.SetPointTableID
            If Con.ControlTableID <> "" Then Con.ControlTableID = Prefix & Con.ControlTableID
            If Not Con.ControlTable Is Nothing Then Con.ControlTable.AddPrefix(Prefix)
        Next
    End Sub

    Friend Sub InitializeExport(ExportDir As String)
        If System.IO.File.Exists(ExportDir & "\control.def") Then System.IO.File.Delete(ExportDir & "\control.def")
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing control.def file...", 0, Records.Count)
        Using defWriter As New StreamWriter(ExportDir & "\control.def", Append)
            For Each myRecord As clsControlDefRecord In Records.Values
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                If myRecord.InUse Then myRecord.Write(defWriter)
            Next
        End Using
    End Sub

    Friend Function Read(ByVal myStrings As Collection) As Boolean
        Try
            Dim i As Integer = 0
            Me.setup.GeneralFunctions.UpdateProgressBar("Reading controller definitions...", 0, myStrings.Count)
            For Each myString As String In myStrings
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
                Dim myRecord As clsControlDefRecord = New clsControlDefRecord(Me.setup)
                myRecord.Read(myString)
                myRecord.InUse = True

                If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                    Me.setup.Log.AddWarning("Multiple instances of controller definition " & myRecord.ID & " found in SOBEK schematization.")
                Else

                    'alleen importeren als hij ook daadwerkelijk door een struct.dat-record wordt aangeroepen
                    If Not SbkCase.CFData.Data.StructureData.StructDatRecords.FindByControllerID(myRecord.ID.Trim.ToUpper) Is Nothing Then
                        Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                    End If
                End If
            Next myString
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function read of class clsControlDefRecords.")
            Return False
        End Try
    End Function

End Class
