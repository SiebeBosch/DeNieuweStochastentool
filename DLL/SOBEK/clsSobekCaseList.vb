Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Friend Class clsSobekCaseList
  Friend Cases As New List(Of clsSobekCaseListItem)
  Private setup As clsSetup
  Private SobekProject As clsSobekProject

  Friend Sub New(ByRef mySetup As clsSetup, ByRef myProject As clsSobekProject)
    Me.setup = mySetup
    Me.SobekProject = myProject
  End Sub

  Friend Sub Read()
    'deze routine zoekt uit wat de casedirectory van de opgegeven sobekcase is
    Dim cNum As Integer, cName As String, cDir As String

    If System.IO.File.Exists(Me.SobekProject.ProjectDir & "\caselist.cmt") Then
      Using CaseList As New StreamReader(Me.SobekProject.ProjectDir & "\caselist.cmt")
        While Not CaseList.EndOfStream
          Dim myRecord As String = CaseList.ReadLine
          If Not myRecord = "" Then
            cNum = Me.setup.GeneralFunctions.ParseString(myRecord, " ", 1)
                        cName = Me.setup.GeneralFunctions.RemoveSurroundingQuotes(myRecord, True, False)
                        cDir = Me.SobekProject.ProjectDir & "\" & cNum
            Dim myItem = New clsSobekCaseListItem(Me.setup)
            myItem.dir = cDir
            myItem.name = cName
            Me.Cases.Add(myItem)
          End If
        End While
        CaseList.Close()
      End Using
    Else
      MsgBox("Error: could not find file caselist.cmt in specified directory " & Me.SobekProject.ProjectDir & ".")
    End If
  End Sub
End Class
