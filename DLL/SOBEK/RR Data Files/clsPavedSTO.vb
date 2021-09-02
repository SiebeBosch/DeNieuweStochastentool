Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsPavedSTO

  Public FileContent As New Collection 'of string
  Public Records As New Dictionary(Of String, clsPavedSTORecord)
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each STO As clsPavedSTORecord In Records.Values
      STO.ID = Prefix & STO.ID
      STO.nm = Prefix & STO.nm
    Next
  End Sub

  Friend Function getAddRecord(ByVal myStor As Double) As clsPavedSTORecord
    Dim myPVSTO As New clsPavedSTORecord(Me.setup)
    If Not Records.ContainsKey(myStor) Then
      myPVSTO = New clsPavedSTORecord(Me.setup)
      myPVSTO.ID = "STOR" & Records.Count.ToString.Trim
      myPVSTO.nm = myPVSTO.ID
      myPVSTO.ms = 0
      myPVSTO.mr1 = myStor
      myPVSTO.mr2 = 0
      Records.Add(myStor, myPVSTO)
      Return myPVSTO
    Else
      Return Records.Item(myStor)
    End If
  End Function

  Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

    'leest unpaved definitions in
    Dim Datafile = New clsSobekDataFile(Me.setup)
    Dim i As Long

    FileContent = Datafile.Read(casedir & "\paved.sto", "STDF")
    For i = 1 To FileContent.Count
      Dim STORecord As clsPavedSTORecord
      STORecord = New clsPavedSTORecord(Me.setup)
      STORecord.Read(FileContent(i))
      If Not Records.ContainsKey(STORecord.ID.Trim.ToUpper) Then
        Records.Add(STORecord.ID.Trim.ToUpper, STORecord)
      Else
        Me.setup.Log.AddWarning("Double instances of paved storage definition " & STORecord.ID & " in " & casedir)
      End If
    Next
  End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using pvstoWriter As New StreamWriter(ExportDir & "\paved.sto", Append)
            For Each myRecord As clsPavedSTORecord In Records.Values
                myRecord.Write(pvstoWriter)
            Next myRecord
            pvstoWriter.Close()
        End Using
    End Sub
End Class
