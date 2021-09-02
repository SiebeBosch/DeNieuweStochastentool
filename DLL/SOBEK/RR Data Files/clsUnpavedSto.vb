Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsUnpavedSto

  Friend FileContent As New Collection 'of string
  Friend Records As New Dictionary(Of String, clsUnpavedSTORecord)
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each STO As clsUnpavedSTORecord In Records.Values
      STO.ID = Prefix & STO.ID
      STO.nm = Prefix & STO.nm
    Next
  End Sub


  Friend Function getAddRecord(ByVal myStor As Double) As clsUnpavedSTORecord
    Dim myUPSTO As New clsUnpavedSTORecord(Me.setup)
    If Not Records.ContainsKey(myStor) Then
      myUPSTO = New clsUnpavedSTORecord(Me.setup)
      myUPSTO.ID = "STOR" & Records.Count.ToString.Trim
      myUPSTO.nm = myUPSTO.ID
      myUPSTO.il = 0
      myUPSTO.ml = myStor
      Records.Add(myStor, myUPSTO)
      Return myUPSTO
    Else
      Return Records.Item(myStor)
    End If
  End Function

  Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

    'leest unpaved definitions in
    Dim Datafile = New clsSobekDataFile(Me.setup)
    Dim i As Long

    FileContent = Datafile.Read(casedir & "\unpaved.sto", "STDF")
    For i = 1 To FileContent.Count
      Dim STORRecord As clsUnpavedSTORecord
      STORRecord = New clsUnpavedSTORecord(Me.setup)
      STORRecord.Read(FileContent(i))
      If Not Records.ContainsKey(STORRecord.ID.Trim.ToUpper) Then
        Records.Add(STORRecord.ID.Trim.ToUpper, STORRecord)
      Else
        Me.setup.Log.AddWarning("Double instance of unpaved storage definition " & STORRecord.ID & " in " & casedir)
      End If
    Next
  End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using upSTOWriter As New StreamWriter(ExportDir & "\unpaved.sto", Append)
            For Each myRecord As clsUnpavedSTORecord In Records.Values
                myRecord.Write(upSTOWriter)
            Next myRecord
            upSTOWriter.Close()
        End Using
    End Sub
End Class
