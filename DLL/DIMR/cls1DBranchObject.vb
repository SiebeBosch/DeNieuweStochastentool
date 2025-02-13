Imports STOCHLIB.General
Public Class cls1DBranchObject
    Private Setup As clsSetup
    Public X As Double = Double.NaN
    Public Y As Double = Double.NaN

    Public ID As String
    Friend Branch As cls1DBranch
    Friend Chainage As Double
    Private Network As clsNetworkFile

    Public Sub New(ByRef mySetup As clssetup, ByRef myNetwork As clsNetworkFile)
        Setup = mySetup
        Network = myNetwork
    End Sub

    Public Sub New(ByRef myNetwork As clsNetworkFile, myID As String, ByRef myBranch As cls1DBranch, myChainage As Double)
        Network = myNetwork
        ID = myID
        Branch = myBranch
        Chainage = myChainage
    End Sub

    Public Function getBranch() As cls1DBranch
        Return Branch
    End Function

    Public Function CalcCoordinates() As Boolean
        Try
            Dim myChainage As Double = 0
            Dim Seglength As Double = 0
            For i = 1 To Branch.VectorPoints.Count - 1
                Seglength = Setup.GeneralFunctions.Pythagoras(Branch.VectorPoints.Values(i - 1).X, Branch.VectorPoints.Values(i - 1).Y, Branch.VectorPoints.Values(i).X, Branch.VectorPoints.Values(i).Y)
                If myChainage + Seglength >= Chainage Then
                    X = Setup.GeneralFunctions.Interpolate(myChainage, Branch.VectorPoints.Values(i - 1).X, myChainage + Seglength, Branch.VectorPoints.Values(i).X, Chainage)
                    Y = Setup.GeneralFunctions.Interpolate(myChainage, Branch.VectorPoints.Values(i - 1).Y, myChainage + Seglength, Branch.VectorPoints.Values(i).Y, Chainage)
                    Return True
                Else
                    myChainage += Seglength
                End If
            Next
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function CalcCoordinates of class cls1DBranchObject: " & ex.Message)
            Return False
        End Try
        'this function computes the coordinates of our branchobject

    End Function

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
