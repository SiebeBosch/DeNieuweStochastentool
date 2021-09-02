Public Class clsSobekNodeType

    Public SbkModule As String
    Public SbkNum As Integer
    Public ID As String
    Public ParentID As String

    'hier maken we een lijst met knooptypes, bijbehorende sobek nummering en sobek module
    Public Sub New()

    End Sub

    Public Function isArea() As Boolean
        'returns true if the current node under investigation is either an unpaved, paved or greehouse node
        Select Case ParentID
            Case Is = "3B_UNPAVED"
                Return True
            Case Is = "3B_SACRAMENTO"
                Return True
            Case Is = "3B_PAVED"
                Return True
            Case Is = "3B_GREENHOUSE"
                Return True
            Case Is = "3B_OPENWATER"
                Return True
            Case Else
                Return False
        End Select

    End Function
    Public Sub New(ByVal iID As String, ByVal iSbkNum As Integer)
        SbkNum = iSbkNum
        ID = iID
        If Not iSbkNum = 0 Then SbkNum = iSbkNum
    End Sub

    Public Function ParentIsRRStructure() As Boolean
        'figures out whether the parent type of this node type is an RR structure
        If ParentID = "3B_FRICTION" OrElse ParentID = "3B_ORIFICE" _
         OrElse ParentID = "3B_WEIR" OrElse ParentID = "3B_PUMP" OrElse
          ParentID = "3B_QH-RELATION" Then
            Return True
        Else
            Return False
        End If
    End Function

End Class


Public Class SobekUserDefinedNodeType
    Inherits clsSobekNodeType

    Public UserDefNum As Integer  'uit eerste kolom ntrpluv.obj

    'hier maken we een lijst met knooptypes, bijbehorende sobek nummering en sobek module
    Public Sub New(ByVal iID As String, ByVal myNum As Integer, ByVal iParentID As String, ByVal iUserDefNum As Integer)
        MyBase.New(iID, myNum)
        ID = iID
        SbkNum = myNum
        ParentID = iParentID
        UserDefNum = iUserDefNum
    End Sub

End Class


