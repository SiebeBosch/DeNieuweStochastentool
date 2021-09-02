Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Friend Class clsStructDatRecords

  Friend Records As New Dictionary(Of String, clsStructDatRecord)
  Private setup As clsSetup
  Private SbkCase As clsSobekCase

  Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
    Me.setup = mySetup
    Me.SbkCase = myCase
  End Sub

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each Dat As clsStructDatRecord In Records.Values
      Dat.ID = Prefix & Dat.ID        'structure id
      Dat.dd = Prefix & Dat.dd                        'structure definition id
      If Dat.cj <> "" Then Dat.cj = Prefix & Dat.cj 'control definition id
    Next
  End Sub

    Friend Sub InitializeExport(ExportDir As String)
        If System.IO.File.Exists(ExportDir & "\struct.dat") Then System.IO.File.Delete(ExportDir & "\struct.dat")
    End Sub

    Friend Sub Read(ByVal myStrings As Collection)
    Dim i As Integer = 0
    Me.setup.GeneralFunctions.UpdateProgressBar("Reading structures...", 0, myStrings.Count)
    For Each myString As String In myStrings
      Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
      Dim myRecord As clsStructDatRecord = New clsStructDatRecord(Me.setup)
      myRecord.Read(myString)

      If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
        Me.setup.Log.AddWarning("Multiple instances of structure " & myRecord.ID & " found in SOBEK schematization.")
      Else
        'als het netwerk ook is ingelezen, hoeven we alleen die records in te lezen waarvan het object ook daadwerkelijk in het netwerk voorkomt
        If sbkcase.CFTopo.Reaches.Reaches.Count > 0 Then
          If sbkcase.CFTopo.ReachObjectExists(myRecord.ID.Trim.ToUpper) Then
            Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
          End If
        Else
          Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
        End If
      End If
    Next myString
  End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing struct.dat file...", 0, Records.Count)
        Using datWriter As New StreamWriter(ExportDir & "\struct.dat", Append)
            For Each myrecord As clsStructDatRecord In Records.Values
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                If myrecord.InUse Then myrecord.Write(datWriter)
            Next
        End Using
    End Sub

    Friend Function FindByID(ByVal myID As String, ByVal Prefix As String, ByVal AutoDetectPrefix As Boolean) As clsStructDatRecord

    'zoekt het eerste kunstwerk op dat een gegeven definitie ID gebruikt en geeft dit terug
    For Each myDat As clsStructDatRecord In Records.Values
      If AutoDetectPrefix Then
        If Right(myDat.ID.Trim.ToUpper, myID.Trim.Length) = myID.Trim.ToUpper Then Return myDat
      ElseIf Prefix <> "" Then
        If myDat.ID.Trim.ToUpper = Prefix.Trim.ToUpper & myID.Trim.ToUpper Then Return myDat
      Else
        If myDat.ID.Trim.ToUpper = myID.Trim.ToUpper Then Return myDat
      End If
    Next myDat
    Return Nothing

  End Function

  Friend Function FindByDefinitionID(ByVal mydd As String) As clsStructDatRecord
    'zoekt het eerste kunstwerk op dat een gegeven definitie ID gebruikt en geeft dit terug
    For Each myDat As clsStructDatRecord In Records.Values
      If myDat.dd.Trim.ToUpper = mydd.Trim.ToUpper Then
        Return myDat
      End If
    Next myDat
    Return Nothing
  End Function

  Friend Function FindByControllerID(ByVal mycj As String) As clsStructDatRecord
    'zoekt het eerste kunstwerk op dat een gegeven definitie ID gebruikt en geeft dit terug
    For Each myDat As clsStructDatRecord In Records.Values
      If myDat.cj.Trim.ToUpper = mycj.Trim.ToUpper And myDat.ca = 1 Then
        Return myDat
      End If
    Next myDat
    Return Nothing
  End Function
End Class
