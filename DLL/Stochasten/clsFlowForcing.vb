Public Class clsFlowForcing
    Dim ClassID As String 'the stochastic class id
    Dim Season As String
    Dim ID As String      'the unique combination for boundary id + stochastic class id

    Public Sub New(myClassID As String, mySeason As String)
        ClassID = myClassID
        Season = mySeason
        ID = ClassID.Trim.ToUpper & "_" & Season.Trim.ToUpper
    End Sub

    Public Function GetClassID() As String
        Return ClassID
    End Function

    Public Function GetSeason() As String
        Return Season
    End Function

    Public Function GetID() As String
        Return ID
    End Function

End Class
