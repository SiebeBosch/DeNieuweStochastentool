Public Class clsSobekNodeTypes
  Public NodeTypes As New Dictionary(Of String, clsSobekNodeType)

  Public Function Add(ByVal ID As String, ByVal Num As Integer) As clsSobekNodeType
    Dim NewType As clsSobekNodeType
    If NodeTypes.ContainsKey(ID.Trim.ToUpper) Then
      Return NodeTypes.Item(ID.Trim.ToUpper)
    Else
      NewType = New clsSobekNodeType(ID, Num)
      NodeTypes.Add(ID.Trim.ToUpper, NewType)
      Return NodeTypes.Item(ID.Trim.ToUpper)
    End If
  End Function

  Public Function GetNodeType(ByVal ID As String) As clsSobekNodeType
    If NodeTypes.ContainsKey(ID.Trim.ToUpper) Then
      Return NodeTypes.Item(ID.Trim.ToUpper)
    Else
      Return Nothing
    End If
  End Function

  Public Function GetByParentID(ByVal ParentID As String) As clsSobekNodeType
    For Each myType As clsSobekNodeType In NodeTypes.Values
      If myType.ParentID = ParentID Then
        Return myType
      End If
    Next
    Return Nothing
  End Function

  Public Function GetByNum(ByVal NodeNum As Integer) As clsSobekNodeType
    For Each myType As clsSobekNodeType In NodeTypes.Values
      If myType.SbkNum = NodeNum Then
        Return myType
      End If
    Next
    Return Nothing
  End Function

End Class
