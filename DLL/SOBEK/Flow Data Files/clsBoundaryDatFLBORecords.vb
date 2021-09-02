Option Explicit On
Imports System.Windows.Forms
Imports System.IO
Imports STOCHLIB.General

Friend Class clsBoundaryDatFLBORecords
  Friend records As New Dictionary(Of String, clsBoundaryDatFLBORecord)
  Private setup As clsSetup
  Private SbkCase As clsSobekCase

  Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
    Me.setup = mySetup
    Me.SbkCase = myCase
  End Sub

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Function Clone() As clsBoundaryDatFLBORecords
    Dim myRecords As New clsBoundaryDatFLBORecords(Me.setup)
    For Each myRecord As clsBoundaryDatFLBORecord In records.Values
      myRecords.records.Add(myRecord.ID.Trim.ToUpper, myRecord.clone)
    Next
    Return myRecords
  End Function

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each FLBO As clsBoundaryDatFLBORecord In records.Values
      FLBO.ID = Prefix & FLBO.ID
      If Not FLBO.HWTTable Is Nothing Then FLBO.HWTTable.AddPrefix(Prefix)
      If Not FLBO.QDTTable Is Nothing Then FLBO.QDTTable.AddPrefix(Prefix)
    Next
  End Sub


  Friend Sub Read(ByVal myStrings As Collection)
    Dim i As Long = 0, n As Long = myStrings.Count
    setup.GeneralFunctions.UpdateProgressBar("Reading boundary records...", 0, n)
    For Each myString As String In myStrings
      i += 1
      setup.GeneralFunctions.UpdateProgressBar("", i, n)
      Dim myRecord As clsBoundaryDatFLBORecord = New clsBoundaryDatFLBORecord(Me.setup)
      myRecord.Read(myString)

      If records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
        Me.setup.Log.AddWarning("Multiple instances of boundary " & myRecord.ID & " found in SOBEK schematization.")
      Else
        Me.records.Add(myRecord.ID.Trim.ToUpper, myRecord)
      End If
    Next myString
  End Sub

  Friend Function getAddRecord(ByVal ID As String) As clsBoundaryDatFLBORecord
    Dim myRecord As clsBoundaryDatFLBORecord
    If Not records.ContainsKey(ID.Trim.ToUpper) Then
      myRecord = New clsBoundaryDatFLBORecord(Me.setup)
      myRecord.ID = ID
      records.Add(myRecord.ID.Trim.ToUpper, myRecord)
      Return myRecord
    Else
      Return records.Item(ID.Trim.ToUpper)
    End If
  End Function

  Friend Sub Write(ByRef datWriter As StreamWriter)

    Dim i As Integer = 0
    Me.setup.GeneralFunctions.UpdateProgressBar("Writing boundary.dat file...", 0, records.Count)
    For Each myRecord As clsBoundaryDatFLBORecord In records.Values
      i += 1
      Me.setup.GeneralFunctions.UpdateProgressBar("", i, records.Count)
      If myRecord.InUse Then myRecord.Write(datWriter)
    Next myRecord
  End Sub

  Friend Function GetByID(ByVal ID As String) As clsBoundaryDatFLBORecord
    If records.ContainsKey(ID.Trim.ToUpper) Then
      Return records.Item(ID.Trim.ToUpper)
    Else
      Return Nothing
    End If
  End Function


End Class
