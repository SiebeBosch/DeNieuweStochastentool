Public Class clsSobekBranchTypes
  Public BranchTypes As New Dictionary(Of String, clsSobekBranchType)

  Public Function Add(ByVal ID As String, ByVal Num As Integer) As clsSobekBranchType
    Dim NewType As clsSobekBranchType
    If BranchTypes.ContainsKey(ID.Trim.ToUpper) Then
      Return BranchTypes.Item(ID.Trim.ToUpper)
    Else
      NewType = New clsSobekBranchType(ID, Num)
      BranchTypes.Add(ID.Trim.ToUpper, NewType)
      Return BranchTypes.Item(ID.Trim.ToUpper)
    End If
  End Function

  Public Function GetBranchType(ByVal ID As String) As clsSobekBranchType
    If BranchTypes.ContainsKey(ID.Trim.ToUpper) Then
      Return BranchTypes.Item(ID.Trim.ToUpper)
    Else
      Return Nothing
    End If
  End Function

  Public Function GetByParentID(ByVal ParentID As String) As clsSobekBranchType
    For Each myType As clsSobekBranchType In BranchTypes.Values
      If myType.ParentID = ParentID Then
        Return myType
      End If
    Next
    Return Nothing
  End Function

  Public Function GetByNum(ByVal NodeNum As Integer) As clsSobekBranchType
    For Each myType As clsSobekBranchType In BranchTypes.Values
      If myType.SbkNum = NodeNum Then
        Return myType
      End If
    Next
    Return Nothing
  End Function

End Class
