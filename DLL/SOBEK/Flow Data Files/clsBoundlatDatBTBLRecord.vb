Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsBoundlatDatBTBLRecord
  Public ID As String
  Public sc As Integer
  Public lt As Integer
  Public pdin1 As Integer
  Public pdin2 As Integer
  Public PDINPeriod As String
  Public TimeTable As clsSobekTable

  Friend InUse As Boolean

  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
    Me.TimeTable = New clsSobekTable(setup)
    InUse = True
  End Sub
  Public Sub Read(ByVal myRecord As String)
    Dim myStr As String
    Dim Pos As Integer, Table As String

    While Not myRecord = ""
      myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
      Select Case myStr
        Case Is = "id"
          ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "sc"
          sc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "lt"
          lt = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "PDIN"
          pdin1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          pdin2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          If pdin2 = 1 Then PDINPeriod = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case Is = "TBLE"
          Pos = InStr(1, myRecord, "tble")
          Table = Left(myRecord, Pos - 1)
          myRecord = Right(myRecord, Len(myRecord) - Pos - 3)
          Call TimeTable.Read(Table)
          TimeTable.ID = ID
        Case Is = "btbl"
          'afsluitend record
        Case Else
          If Not IsNumeric(myStr) Then
            Me.setup.Log.AddWarning("Unsupported token " & myStr & " found in boundlat.dat")
          End If
      End Select
    End While
  End Sub

  Public Sub Write(ByVal datWriter As StreamWriter)
    Dim i As Long
    datWriter.WriteLine("BTBL id '" & ID & "' sc " & sc & " lt " & lt & " PDIN " & pdin1 & " " & pdin2 & " '" & PDINPeriod & "' pdin")
    datWriter.WriteLine("TBLE")
    For i = 0 To TimeTable.Dates.Count - 1
      datWriter.WriteLine("'" & Format(TimeTable.Dates.Values(i), "yyyy/MM/dd;HH:mm:ss") & "' " & TimeTable.Values1.Values(i) & " <")
    Next
    datWriter.WriteLine("tble btbl")
  End Sub
End Class

