Option Explicit On

''' <summary>
''' Geen contructor nodig
''' </summary>
''' <remarks></remarks>
Friend Class clsSbkLinkageNodes
  Friend LinkageNodes As New Dictionary(Of String, clsNetworkTPNdlkRecord)

  'TODO: Converteer naar struct

  Friend Sub Add(ByVal myLinkageNode As clsNetworkTPNdlkRecord)
    Call LinkageNodes.Add(myLinkageNode.ID.Trim.ToUpper, myLinkageNode)
  End Sub

End Class
