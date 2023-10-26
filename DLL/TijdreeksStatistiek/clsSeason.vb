Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Public Class clsSeason

  'within the given season
  Public Season As STOCHLIB.GeneralFunctions.enmSeason
  Public Durations As New Dictionary(Of Integer, clsDuration)
  Public DailySums As Dictionary(Of Date, clsDailyPrecipitationSum)

  Private Setup As clsSetup
  Private Series As clsRainfallSeries

  Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsRainfallSeries, ByVal mySeason As enmSeason)
    Setup = mySetup
    Series = mySeries
    Season = mySeason
    Durations = New Dictionary(Of Integer, clsDuration)
  End Sub

  Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsRainfallSeries, ByVal mySeason As String)
    Try
      Setup = mySetup
      Series = mySeries
      If Not Setup.GeneralFunctions.SeasonFromString(mySeason, Season) Then Throw New Exception("Error getting season from description " & mySeason)
    Catch ex As Exception
      Me.Setup.Log.AddError(ex.Message)
    End Try
  End Sub


  Public Function GetAddDuration(ByVal myDuration As Integer) As clsDuration
    Dim NewDuration As clsDuration
    If Not Durations.ContainsKey(myDuration) Then
      NewDuration = New clsDuration(Me.Setup, Series, Me, myDuration)
      Durations.Add(myDuration, NewDuration)
      Return NewDuration
    Else
      Return Durations.Item(myDuration)
    End If
  End Function

  Public Function AddDuration(ByVal myDuration As Integer) As clsDuration
    Dim Duration As New clsDuration(Me.Setup, Me.Series, Me, myDuration)
    Durations.Add(myDuration, Duration)
    Return Duration
  End Function

  Public Function AggregatePrecipitationByDay() As Boolean
    'This calculates the daily precipitation sum of the current rainfall series, within the given season
    Dim ts As Long, myDate As Date
    Dim mySums As New Dictionary(Of Date, clsDailyPrecipitationSum)
    Dim mySum As clsDailyPrecipitationSum

    Try
      Me.Setup.GeneralFunctions.UpdateProgressBar("Aggregating rainfall series from hour to day", 0, 10)
      mySums = New Dictionary(Of Date, clsDailyPrecipitationSum)

      'make a dictionary with all daily precipitation sums
      For ts = 0 To Series.Values.Count - 1
        If DateInSeason(Series.Dates(ts)) Then
          myDate = New Date(Year(Series.Dates(ts)), Month(Series.Dates(ts)), Day(Series.Dates(ts))) 'make a new date with just year, month and day
          If Not mySums.ContainsKey(myDate) Then
            mySum = New clsDailyPrecipitationSum
            mySum.myDate = myDate
            mySum.StartTs = ts
            mySum.Sum = Series.Values(ts)
            mySums.Add(myDate, mySum)
          Else
            mySum = mySums.Item(myDate)
            mySum.Sum += Series.Values(ts)
          End If
        End If
      Next

      'sort the dictionary in descending order and replace the 
      DailySums = New Dictionary(Of Date, clsDailyPrecipitationSum)
      Dim Sorted = From entry In mySums Order By entry.Value.Sum Descending
      For Each entry In Sorted
        DailySums.Add(entry.Key, entry.Value)
      Next
      mySums = Nothing
      Sorted = Nothing

      Return True

    Catch ex As Exception
      Me.Setup.Log.AddError("Error in sub AggregatePrecipitationByDay of class clsSeason.")
      Me.Setup.Log.AddError(ex.Message)
      Return False
    End Try

  End Function

  Public Function DateInSeason(ByRef myDate As Date) As Boolean
        'checks if a given date falls inside the current season
        If Season = enmSeason.yearround Then
            Return True
        ElseIf Season = enmSeason.meteosummerhalfyear AndAlso Me.Setup.GeneralFunctions.MeteorologischHalfJaar(myDate) = enmSeason.meteosummerhalfyear Then
            Return True
        ElseIf Season = enmSeason.meteowinterhalfyear AndAlso Me.Setup.GeneralFunctions.MeteorologischHalfJaar(myDate) = enmSeason.meteowinterhalfyear Then
            Return True
        ElseIf Season = enmSeason.meteosummerquarter AndAlso Me.Setup.GeneralFunctions.MeteorologischSeizoen(myDate) = enmSeason.meteosummerquarter Then
            Return True
        ElseIf Season = enmSeason.meteoautumnquarter AndAlso Me.Setup.GeneralFunctions.MeteorologischSeizoen(myDate) = enmSeason.meteoautumnquarter Then
            Return True
        ElseIf Season = enmSeason.meteowinterquarter AndAlso Me.Setup.GeneralFunctions.MeteorologischSeizoen(myDate) = enmSeason.meteowinterquarter Then
            Return True
        ElseIf Season = enmSeason.meteospringquarter AndAlso Me.Setup.GeneralFunctions.MeteorologischSeizoen(myDate) = enmSeason.meteospringquarter Then
            Return True
        ElseIf Season = enmSeason.hydrosummerhalfyear AndAlso Me.Setup.GeneralFunctions.HydrologischHalfJaar(myDate) = enmSeason.hydrosummerhalfyear Then
            Return True
        ElseIf Season = enmSeason.hydrowinterhalfyear AndAlso Me.Setup.GeneralFunctions.HydrologischHalfJaar(myDate) = enmSeason.hydrowinterhalfyear Then
            Return True
        ElseIf Season = enmSeason.marchthroughoctober AndAlso (Month(myDate) >= 3 AndAlso Month(myDate) <= 10) Then
            Return True
        ElseIf Season = enmSeason.novemberthroughfebruary AndAlso (Month(myDate) <= 2 OrElse Month(myDate) >= 11) Then
            Return True
        ElseIf Season = enmSeason.aprilthroughaugust AndAlso (Month(myDate) >= 4 AndAlso Month(myDate) <= 8) Then
            Return True
        ElseIf Season = enmSeason.septemberthroughmarch AndAlso (Month(myDate) <= 3 OrElse Month(myDate) >= 9) Then
            Return True
        ElseIf Season = enmSeason.growthseason AndAlso (Month(myDate) >= 3 AndAlso Month(myDate) <= 10) Then
            Return True
        ElseIf Season = enmSeason.outsidegrowthseason AndAlso (Month(myDate) <= 2 OrElse Month(myDate) >= 11) Then
            Return True
        Else
            Return False
    End If
  End Function


  Public Function GetDuration(ByVal myDuration As Integer) As clsDuration
    If Not Durations.ContainsKey(myDuration) Then
      Return Nothing
    Else
      Return Durations.Item(myDuration)
    End If
  End Function



End Class
