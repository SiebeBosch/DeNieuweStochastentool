Option Explicit On

Imports STOCHLIB.General

Friend Class clsNetworkTPBrchRecord
  Friend ID As String
  Friend Name As String
  Friend bn As String
  Friend en As String
  Friend al As Double
  Friend Record As String
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub
  Friend Sub Build()
    Record = "BRCH id '" & ID & "' nm '' bn '" & bn & "' en '" & en & "' al '" & al & " vc_opt 0 vc_equi -1 vc_len 500 brch"
  End Sub

  Friend Function Duplicate(ByVal newReachID As String) As clsNetworkTPBrchRecord
    Dim newRecord As New clsNetworkTPBrchRecord(Me.setup)
    newRecord.ID = newReachID
    newRecord.Name = Name
    newRecord.bn = bn
    newRecord.en = en
    newRecord.al = al
    newRecord.Record = Record
    Return newRecord
  End Function

  Friend Sub Read(ByVal myRecord As String)
    Dim Done As Boolean, myStr As String
    Done = False

    Record = myRecord
    While Not Done
      myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
      Select Case LCase(myStr)
        Case "id"
          ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "nm"
          Name = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "bn"
          bn = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "en"
          en = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case ""
          Done = True
      End Select
    End While
  End Sub
End Class
