Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsbound3BTBL

  Public FileContent As New Collection 'of string
  Public Records As New Dictionary(Of String, clsBound3BTBLRecord)
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each BND As clsBound3BTBLRecord In Records.Values
      BND.ID = Prefix & BND.ID
      BND.nm = Prefix & BND.nm
      If Not BND.TimeTable Is Nothing Then BND.TimeTable.AddPrefix(Prefix)
    Next
  End Sub

  Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

    'leest unpaved definitions in
    Dim Datafile = New clsSobekDataFile(Me.setup)
    Dim i As Long

    FileContent = Datafile.Read(casedir & "\bound3b.tbl", "BN_T")
    For i = 1 To FileContent.Count
      Dim TBLRecord As clsBound3BTBLRecord
      TBLRecord = New clsBound3BTBLRecord(Me.setup)
      TBLRecord.Read(FileContent(i))
      If Not Records.ContainsKey(TBLRecord.ID.Trim.ToUpper) Then
        Records.Add(TBLRecord.ID.Trim.ToUpper, TBLRecord)
      Else
        Me.setup.Log.AddWarning("Double instances of bound3b.tbl definition " & TBLRecord.ID & " in " & casedir)
      End If
    Next
  End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using tblWriter As New StreamWriter(ExportDir & "\bound3b.tbl", Append)
            For Each myRecord As clsBound3BTBLRecord In Records.Values
                myRecord.Write(tblWriter)
            Next myRecord
            tblWriter.Close()
        End Using
    End Sub

    Friend Function GetRecord(ByVal myID As String) As clsBound3BTBLRecord
    If Records.ContainsKey(myID.Trim.ToUpper) Then
      Return Records.Item(myID.Trim.ToUpper)
    Else
      Return Nothing
    End If
  End Function

  Friend Function getAddRecord(ByVal myID As String)
    Dim myRecord As clsBound3BTBLRecord
    If Records.ContainsKey(myID.Trim.ToUpper) Then
      Return Records.Item(myID.Trim.ToUpper)
    Else
      myRecord = New clsBound3BTBLRecord(Me.setup)
      myRecord.ID = myID
      Records.Add(myID.Trim.ToUpper, myRecord)
      Return myRecord
    End If
  End Function

End Class

