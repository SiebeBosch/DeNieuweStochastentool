Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsStructDatRecord

  Friend ID As String
  Friend nm As String
  Friend dd As String
  Friend ca As Integer
  Friend cj As String = ""
  Private setup As clsSetup
  Friend InUse As Boolean

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Sub Read(ByVal myRecord As String)

    Dim myStr As String

    While Not myRecord = ""
      myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
      Select Case myStr.ToLower
        Case "id"
          ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "nm"
          nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "dd"
          dd = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "ca"
          ca = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "cj"
          cj = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
        Case "stru"
        Case "STRU"
        Case Else
          'doe niets
      End Select
    End While
    InUse = True

  End Sub

  Friend Sub Write(ByRef myWriter As StreamWriter)
    myWriter.WriteLine("STRU id '" & ID & "' nm '" & nm & "' dd '" & dd & "' ca " & ca & " cj '" & cj & "' stru")
  End Sub
End Class
