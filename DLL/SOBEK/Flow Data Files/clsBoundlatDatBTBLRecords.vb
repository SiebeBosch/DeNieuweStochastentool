Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Friend Class clsBoundlatDatBTBLRecords
  Friend records As New Dictionary(Of String, clsBoundlatDatBTBLRecord)
  Private setup As clsSetup
  Private SbkCase As clsSobekCase

  Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
    Me.setup = mySetup
    Me.SbkCase = myCase
  End Sub

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each BTBL As clsBoundlatDatBTBLRecord In records.Values
      BTBL.ID = Prefix & BTBL.ID
      If Not BTBL.TimeTable Is Nothing Then BTBL.TimeTable.AddPrefix(Prefix)
    Next
  End Sub

  Friend Sub Write(ByRef datWriter As StreamWriter)
    Dim i As Integer = 0
    Me.setup.GeneralFunctions.UpdateProgressBar("Writing Boundlat.dat file...", 0, records.Count)
    For Each record As clsBoundlatDatBTBLRecord In records.Values
      i += 1
      Me.setup.GeneralFunctions.UpdateProgressBar("", i, records.Count)
      If record.InUse Then record.Write(datWriter)
    Next
  End Sub

  Friend Sub Read(ByVal myStrings As Collection)
    Dim i As Long = 0, n As Long = myStrings.Count
    Me.setup.GeneralFunctions.UpdateProgressBar("Reading boundary records...", 0, myStrings.Count)
    For Each myString As String In myStrings
      i += 1
      Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
      Dim myRecord As clsBoundlatDatBTBLRecord = New clsBoundlatDatBTBLRecord(Me.setup)
      myRecord.Read(myString)

      If records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
        Me.setup.Log.AddWarning("Multiple instances of boundary " & myRecord.ID & " found in SOBEK schematization.")
      Else
        Call records.Add(myRecord.ID.Trim.ToUpper, myRecord)
      End If
    Next myString
  End Sub
End Class
