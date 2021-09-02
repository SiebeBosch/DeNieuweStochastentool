Imports STOCHLIB.General

Public Class clsRRUnpavedNodes

    Friend RRUnPavedNodes As New Dictionary(Of String, clsRRUnpavedNode)
    Private setup As clsSetup
    Friend Area As clsSubcatchment  'de parent

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Function Add(ByRef myNode As clsRRUnpavedNode) As Boolean

        If Not RRUnPavedNodes.ContainsKey(myNode.ID.Trim.ToUpper) Then
            RRUnPavedNodes.Add(myNode.ID.Trim.ToUpper, myNode)
            Return True
        Else
            Me.setup.Log.AddError("Collection already containes unpaved node with id " & myNode.ID)
            Return False
        End If
    End Function

    Friend Function getAdd(ByVal myName As String) As clsRRUnpavedNode

        'retrieves a Sewage Area given it's ID. If not found, it will create a new one
        'HOWEVER: A sewage area is only useful when the corresponding CSO (Combined Sewer Overflow) actually exists.
        'If it doesn't, we'll return Nothing and the paved area can later be added to the normal Paved-node.
        Dim myRRUnPavedNode As clsRRUnpavedNode

        'Automatically create the first Sewage Area, which represents the area directly spilling in its own dummy reach
        If RRUnPavedNodes.Count = 0 Then
            myRRUnPavedNode = New clsRRUnpavedNode(Me.setup)
            myRRUnPavedNode.Area = Me.Area.TotalAttributeArea
            myRRUnPavedNode.ID = "PV_" & Me.Area.ID
            myRRUnPavedNode.Name = Me.Area.ID
            RRUnPavedNodes.Add(myRRUnPavedNode.ID.Trim.ToUpper, myRRUnPavedNode)
        End If

        If RRUnPavedNodes.ContainsKey(myName.Trim.ToUpper) Then
            Return RRUnPavedNodes.Item(myName.Trim.ToUpper)
        Else
            myRRUnPavedNode = New clsRRUnpavedNode(Me.setup)
            myRRUnPavedNode.Area = Me.Area.TotalAttributeArea
            myRRUnPavedNode.ID = "PV" & RRUnPavedNodes.Count.ToString.Trim & "_" & Area.ID
            myRRUnPavedNode.Name = myName
            RRUnPavedNodes.Add(myName.Trim.ToUpper, myRRUnPavedNode)
        End If

        Return myRRUnPavedNode

    End Function

    Friend Function GetSewageArea(ByVal ID As String) As clsRRUnpavedNode
        If RRUnPavedNodes.ContainsKey(ID.Trim.ToUpper) Then
            Return RRUnPavedNodes(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function


End Class
