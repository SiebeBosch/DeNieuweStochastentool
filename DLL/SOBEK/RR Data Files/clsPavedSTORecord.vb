Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsPavedSTORecord
  Friend ID As String
  Friend nm As String
  Friend ms As Double
  Friend is_ As Double
  Friend mr1 As Double
  Friend mr2 As Double
  Friend ir1 As Double
  Friend ir2 As Double

  Friend record As String
  Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub Create(ByRef mySetup As clsSetup, myID As String, myName As String, myms As String, myis_ As Double, mymr1 As Double, mymr2 As Double, myir1 As Double, myir2 As Double)
        ID = myID
        nm = myName
        ms = myms
        is_ = myis_
        mr1 = mymr1
        mr2 = mymr2
        ir1 = myir1
        ir2 = myir2
    End Sub

    Friend Sub BuildRecord()
    record = "STDF id '" & ID & "' nm '" & nm & "' ms " & ms & " is " & is_ & " mr " & mr1 & " " & mr2 & " ir " & ir1 & " " & ir2 & " stdf"
  End Sub
  Friend Sub Write(ByVal myWriter As StreamWriter)
    Call BuildRecord()
    myWriter.WriteLine(record)
  End Sub

  Friend Sub Read(ByVal myRecord As String)
    Dim Done As Boolean, myStr As String
    Done = False

    While Not myRecord = ""
      myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
      Select Case LCase(myStr)
        Case "id"
          ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "nm"
          nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "ms"
          ms = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "is"
          is_ = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "mr"
          mr1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          mr2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "ir"
          ir1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
          ir2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
      End Select
    End While
  End Sub
End Class
