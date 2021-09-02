Imports System.IO
Imports STOCHLIB.General

Public Class clsSobekStructures

  Private Setup As clsSetup
  Public Structures As New Dictionary(Of String, clsSobekStructure)

  Public Sub New(ByRef mySetup As clssetup)
    Setup = mySetup
  End Sub

  Public Sub WriteBNA(ByVal Path As String)
    Dim myStr As String
    Using bnaWriter As New StreamWriter(Path, False)
      For Each myStruc As clsSobekStructure In Structures.Values
        myStr = Me.Setup.GeneralFunctions.BNAString(myStruc.ID, myStruc.name, myStruc.x, myStruc.y)
        bnaWriter.WriteLine(myStr)
      Next
    End Using
  End Sub
End Class
