Imports STOCHLIB.General

Public Class clsBound3BTBLRecord
  Friend ID As String
  Friend nm As String
  Friend TimeTable As clsSobekTable
  Friend record As String

  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
    TimeTable = New clsSobekTable(Me.setup)
  End Sub

    Friend Sub New(ByRef mySetup As clsSetup, ByVal myID As String, ByVal ZP As Double, ByVal WP As Double, SeasonTransitions As clsSeasonTransitions)
        setup = mySetup
        TimeTable = New clsSobekTable(Me.setup)
        ID = myID
        nm = ID
        TimeTable.pdin1 = 1
        TimeTable.pdin2 = 1
        TimeTable.PDINPeriod = "'365;00:00:00'"
        TimeTable.AddDatevalPair(New Date(2000, 1, 1), WP, 200)
        TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.WinSumStartMonth, SeasonTransitions.WinSumStartDay), WP, 200)
        TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.WinSumEndMonth, SeasonTransitions.WinSumEndDay), ZP, 200)
        TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.SumWinStartMonth, SeasonTransitions.SumWinStartDay), ZP, 200)
        TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.SumWinEndMonth, SeasonTransitions.SumWinEndDay), WP, 200)
    End Sub

    Friend Sub Build()
    Dim i As Integer
    'deze routine bouwt het paved.3b-record op, op basis van de parameterwaarden
    record = "BN_T id '" & ID & "' nm '" & nm & "' PDIN " & TimeTable.pdin1 & " " & TimeTable.pdin2 & " " & TimeTable.PDINPeriod & " pdin TBLE" & vbCrLf
    For i = 0 To TimeTable.Dates.Count - 1
      record = record & "'" & TimeTable.Dates.Values(i).Year & "/" & Format(TimeTable.Dates.Values(i).Month, "00") & "/" & Format(TimeTable.Dates.Values(i).Day, "00") & ";00:00:00' " & TimeTable.Values1.Values(i) & " " & TimeTable.Values2.Values(i) & " <" & vbCrLf
    Next
    record = record & " tble bn_t"
  End Sub

  Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
    Call Build()
    Call myWriter.WriteLine(record)
  End Sub


  Friend Sub Read(ByVal myRecord As String)
    Dim Done As Boolean, myStr As String, table As String
    Dim PDINDone As Boolean = False
    Dim Pos As Long
    Done = False

    myRecord = Replace(myRecord, vbCrLf, " ")
    While Not myRecord = ""
      myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
      Select Case myStr
        Case "id"
          ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "nm"
          nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "PDIN"
          TimeTable.pdin1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          TimeTable.pdin2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          TimeTable.PDINPeriod = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "TBLE"
          Pos = InStr(1, myRecord, "tble")
          table = Left(myRecord, Pos - 1)
          myRecord = Right(myRecord, myRecord.Length - Pos - 3)
          Call TimeTable.Read(table)
      End Select
    End While
  End Sub

End Class
