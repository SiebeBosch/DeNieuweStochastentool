Public Class clsStochastBoundaryNode
  Public Sub New(ByVal myID As String, ByVal myIdx As Integer, ByVal DataGridViewColIdx As Integer)
    id = myID
    Idx = myIdx
    ColIdx = DataGridViewColIdx
  End Sub

  Public ColIdx As Integer 'column index in the datagrid
  Public Idx As Integer    'the indexnumber of this node in the dictionary/collection
  Public id As String      'ID of the boundary node
End Class
