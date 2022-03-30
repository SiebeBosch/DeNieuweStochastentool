Public Class cls1DBranchObject

    Public ID As String
    Friend Branch As cls1DBranch
    Friend Chainage As Double
    Private Network As clsNetworkFile

    Public Sub New(ByRef myNetwork As clsNetworkFile)
        Network = myNetwork
    End Sub

    Public Sub New(ByRef myNetwork As clsNetworkFile, myID As String, ByRef myBranch As cls1DBranch, myChainage As Double)
        Network = myNetwork
        ID = myID
        Branch = myBranch
        Chainage = myChainage
    End Sub

    Public Sub SetBranch(ByRef myBranch As cls1DBranch)
        Branch = myBranch
    End Sub

    Public Sub SetChainage(myChainage As Double)
        Chainage = myChainage
    End Sub

    Public Function SetBranchByID(BranchID As String) As Boolean
        Try
            For Each myBranch As cls1DBranch In Network.Branches.Values
                If myBranch.ID.Trim.ToUpper = BranchID.Trim.ToUpper Then
                    Branch = myBranch
                    Exit For
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
