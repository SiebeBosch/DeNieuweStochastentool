Public Class clsMeteoForcing
    Dim Duration As Integer
    Dim Volume As Double
    Dim Pattern As String
    Dim Season As String
    Dim ID As String

    Public Sub New(myDuration As Integer, myVolume As Double, myPattern As String, mySeason As String)
        Duration = myDuration
        Volume = myVolume
        Pattern = myPattern
        Season = mySeason
        ID = Duration & "H_" & Volume & "MM_" & Pattern.ToUpper & "_" & Season.ToUpper
    End Sub

    Public Function GetID() As String
        Return ID
    End Function

    Public Function GetDuration() As Integer
        Return Duration
    End Function

    Public Function GetPattern() As String
        Return Pattern
    End Function

    Public Function GetVolume() As Double
        Return Volume
    End Function

    Public Function GetSeason() As String
        Return Season
    End Function

End Class
