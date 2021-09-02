Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsUnpavedInf

  Friend FileContent As New Collection 'of string
  Friend Records As New Dictionary(Of String, clsUnpavedINFRecord)
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each INF As clsUnpavedINFRecord In Records.Values
      INF.ID = Prefix & INF.ID
      INF.nm = Prefix & INF.nm
    Next
  End Sub


  Friend Function getAddRecord(ByVal myInf As Double) As clsUnpavedINFRecord
    Dim myUPINF As New clsUnpavedINFRecord(Me.setup)
    If Not Records.ContainsKey(myInf) Then
      myUPINF = New clsUnpavedINFRecord(Me.setup)
      myUPINF.ID = "STOR" & Records.Count.ToString.Trim
      myUPINF.nm = myUPINF.ID
      myUPINF.ic = myInf
      Records.Add(myInf, myUPINF)
      Return myUPINF
    Else
      Return Records.Item(myInf)
    End If
  End Function

  Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

    'leest unpaved definitions in
    Dim Datafile = New clsSobekDataFile(Me.setup)
    Dim i As Long

    FileContent = Datafile.Read(casedir & "\unpaved.inf", "INFC")
    For i = 1 To FileContent.Count
      Dim INFRecord As clsUnpavedINFRecord
      INFRecord = New clsUnpavedINFRecord(Me.setup)
      INFRecord.Read(FileContent(i))
      If Not Records.ContainsKey(INFRecord.ID.Trim.ToUpper) Then
        Records.Add(INFRecord.ID.Trim.ToUpper, INFRecord)
      Else
        Me.setup.Log.AddWarning("Double instance of infiltration definition " & INFRecord.ID & " in " & casedir)
      End If
    Next
  End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using upInfWriter As New StreamWriter(ExportDir & "\unpaved.inf", Append)
            For Each myRecord As clsUnpavedINFRecord In Records.Values
                myRecord.Write(upInfWriter)
            Next myRecord
            upInfWriter.Close()
        End Using
    End Sub
End Class
