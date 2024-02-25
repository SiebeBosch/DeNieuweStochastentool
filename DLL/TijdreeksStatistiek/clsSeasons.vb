Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Public Class clsSeasons

  Public Seasons As New Dictionary(Of String, clsSeason)

  Private Setup As clsSetup
  Private Series As clsModelTimeSeries

  Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsModelTimeSeries)
    Setup = mySetup
    Series = mySeries
  End Sub

  Public Function GetAdd(ByVal mySeason As enmSeason) As clsSeason
    Dim newSeason As clsSeason
    If Not Seasons.ContainsKey(mySeason.ToString.Trim.ToUpper) Then
      newSeason = New clsSeason(Me.Setup, Me.Series, mySeason)
      Seasons.Add(mySeason.ToString.Trim.ToUpper, newSeason)
      Return newSeason
    Else
      Return Seasons.Item(mySeason.ToString.Trim.ToUpper)
    End If
  End Function

  Public Function GetAddByDescription(ByVal myDescription As String) As clsSeason

    Dim Season As STOCHLIB.GeneralFunctions.enmSeason
    Try
      If Not Me.Setup.GeneralFunctions.SeasonFromString(myDescription, Season) Then Throw New Exception("Error reading season from description " & myDescription)
      If Not Seasons.ContainsKey(Season.ToString.Trim.ToUpper) Then
        Dim newSeason As clsSeason
        newSeason = New clsSeason(Me.Setup, Me.Series, Season)
        Seasons.Add(Season.ToString.Trim.ToUpper, newSeason)
        Return newSeason
      Else
        Return Seasons.Item(Season.ToString.Trim.ToUpper)
      End If
    Catch ex As Exception
      Me.Setup.Log.AddError(ex.Message)
      Return Nothing
    End Try
  End Function

  Public Function Add(ByRef mySetup As clsSetup, ByRef mySeries As clsModelTimeSeries, ByVal mySeason As String) As clsSeason
    Try
      Setup = mySetup
      Series = mySeries

      Dim Season As New STOCHLIB.GeneralFunctions.enmSeason
      If Not Setup.GeneralFunctions.SeasonFromString(mySeason, Season) Then Throw New Exception("Error creating season object from string.")

      Dim newSeason As New clsSeason(Me.Setup, Me.Series, Season)
      Seasons.Add(mySeason.ToString.Trim.ToUpper, newSeason)
      Return newSeason
    Catch ex As Exception
      Me.Setup.Log.AddError(ex.Message)
      Return Nothing
    End Try
  End Function

  Public Function GetByEnum(ByVal mySeasonEnm As STOCHLIB.GeneralFunctions.enmSeason) As clsSeason
    For Each mySeason As clsSeason In Seasons.Values
      If mySeason.Season = mySeasonEnm Then
        Return mySeason
      End If
    Next
    Return Nothing
  End Function

  Public Function GetByName(ByVal mySeason As String) As clsSeason
    If Seasons.ContainsKey(mySeason.Trim.ToUpper) Then
      Return Seasons.Item(mySeason.Trim.ToUpper)
    Else
      Return Nothing
    End If
  End Function

End Class
