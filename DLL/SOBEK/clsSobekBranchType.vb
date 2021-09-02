Public Class clsSobekBranchType

    Public SbkModule As String
    Public SbkNum As Integer
    Public ID As String
    Public ParentID As String
    Public ParentReachType As GeneralFunctions.enmReachtype

    'hier maken we een lijst met knooptypes, bijbehorende sobek nummering en sobek module
    Public Sub New()

    End Sub

    Public Sub SetParentType(ByRef myRt As GeneralFunctions.enmReachtype)
        ParentReachType = myRt
    End Sub

    Public Function GetParentType() As GeneralFunctions.enmReachtype
        Return ParentReachType
    End Function

    Public Sub New(ByVal iID As String, ByVal iSbkNum As Integer)
        SbkNum = iSbkNum
        ID = iID
        If Not iSbkNum = 0 Then SbkNum = iSbkNum
    End Sub

End Class
