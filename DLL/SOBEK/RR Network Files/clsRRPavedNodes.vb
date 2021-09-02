Imports STOCHLIB.General

Public Class clsRRPavedNodes

  Friend RRPavedNodes As New Dictionary(Of String, clsRRPavedNode) 'de paved-knopen
  Friend RRCFPaved As New Dictionary(Of String, clsSbkReachObject) 'de rrcf-connecties

  Private setup As clsSetup

  Friend Area As clsSubcatchment  'de parent

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Function Add(ByRef myNode As clsRRPavedNode) As Boolean

    If Not RRPavedNodes.ContainsKey(myNode.ID.Trim.ToUpper) Then
      RRPavedNodes.Add(myNode.ID.Trim.ToUpper, myNode)
      Return True
    Else
      Me.setup.Log.AddError("Collection already containes paved node with id " & myNode.ID)
      Return False
    End If
  End Function

    Friend Function GetSewageArea(ByVal ID As String) As clsRRPavedNode
    If RRPavedNodes.ContainsKey(ID.Trim.ToUpper) Then
      Return RRPavedNodes(ID.Trim.ToUpper)
    Else
      Return Nothing
    End If
  End Function




End Class
