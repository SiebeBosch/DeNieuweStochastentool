Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsPavedDWA

  Public FileContent As New Collection 'of string
  Public Records As New Dictionary(Of String, clsPavedDWARecord)
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each DWA As clsPavedDWARecord In Records.Values
      DWA.ID = Prefix & DWA.ID
      DWA.nm = Prefix & DWA.nm
    Next
  End Sub


  Friend Function getAddRecord(ByVal myDWF As Double) As clsPavedDWARecord
    Dim myPVDWF As New clsPavedDWARecord(Me.setup)
    If Not Records.ContainsKey(myDWF) Then
      myPVDWF = New clsPavedDWARecord(Me.setup)
      myPVDWF.ID = "DWF" & Records.Count.ToString.Trim
      myPVDWF.nm = myPVDWF.ID
      myPVDWF.do_ = 1
      myPVDWF.wc = 0
      myPVDWF.wd = 120
      Records.Add(myDWF, myPVDWF)
      Return myPVDWF
    Else
      Return Records.Item(myDWF)
    End If
  End Function

  Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

    'leest unpaved definitions in
    Dim Datafile = New clsSobekDataFile(Me.setup)
    Dim i As Long

    FileContent = Datafile.Read(casedir & "\paved.dwa", "DWA")
    For i = 1 To FileContent.Count
      Dim DWARecord As clsPavedDWARecord
      DWARecord = New clsPavedDWARecord(Me.setup)
      DWARecord.Read(FileContent(i))
      If Not Records.ContainsKey(DWARecord.ID.Trim.ToUpper) Then
        Records.Add(DWARecord.ID.Trim.ToUpper, DWARecord)
      Else
        Me.setup.Log.AddWarning("Double instances of DWF definition " & DWARecord.ID & " in " & casedir)
      End If
    Next
  End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using pvdwaWriter As New StreamWriter(ExportDir & "\paved.dwa", Append)
            For Each myRecord As clsPavedDWARecord In Records.Values
                myRecord.Write(pvdwaWriter)
            Next myRecord
            pvdwaWriter.Close()
        End Using
    End Sub
End Class
