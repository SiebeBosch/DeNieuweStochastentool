Imports STOCHLIB.General

Public Class clsTimeSeriesEvent

  Public TimeTable As clsTimeTable
  Public Sum As Double
  Public StartTs As Long 'the index number (0-based) of the timestep where this event starts in the original time series
  Public Pattern As STOCHLIB.GeneralFunctions.enmNeerslagPatroon

  Public MeteoHalfYear As STOCHLIB.GeneralFunctions.enmSeason
  Public MeteoSeason As STOCHLIB.GeneralFunctions.enmSeason
  Public HydroHalfYear As STOCHLIB.GeneralFunctions.enmSeason

  Private Setup As clsSetup
  Private Series As clsModelTimeSeries
  Private Season As clsSeason
  Private Duration As clsDuration

  Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsModelTimeSeries, ByRef mySeason As clsSeason, ByRef myDuration As clsDuration)
    TimeTable = New clsTimeTable(Me.Setup)
    Setup = mySetup
    Series = mySeries
    Season = mySeason
    Duration = myDuration
  End Sub

  Public Sub New(ByRef mySetup As clsSetup, ByRef mySeries As clsModelTimeSeries, ByRef mySeason As clsSeason)
    TimeTable = New clsTimeTable(Me.Setup)
    Setup = mySetup
    Series = mySeries
    Season = mySeason
  End Sub

    Public Function GetParameterValue(Parameter As GeneralFunctions.enmModelParameter, timestepStatistic As GeneralFunctions.enmTimestepStatistic) As Double
        Try
            Select Case timestepStatistic
                Case GeneralFunctions.enmTimestepStatistic.first
                    Return TimeTable.Records.Values(0).GetValue(Parameter)
                Case GeneralFunctions.enmTimestepStatistic.last
                    Return TimeTable.Records.Values(TimeTable.Records.Count - 1).GetValue(Parameter)
                Case GeneralFunctions.enmTimestepStatistic.sum
                    Return CalculateSum(Parameter)
                Case GeneralFunctions.enmTimestepStatistic.mean
                    Return CalculateSum(Parameter) / TimeTable.Records.Count
                Case GeneralFunctions.enmTimestepStatistic.max
                    Return getMax(Parameter)
                Case GeneralFunctions.enmTimestepStatistic.min
                    Return getMin(Parameter)
                Case GeneralFunctions.enmTimestepStatistic.median
                    Return CalculateMedian(Parameter)
            End Select
            Throw New Exception("Unknown timestep statistic")
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function getParameterValue of class clsTimeSeriesEvent: " & ex.Message)
            Return Double.NaN
        End Try


    End Function


    Public Function TimestepPartOfEvent(ByVal ts As Long) As Boolean
    If ts >= StartTs AndAlso ts <= StartTs + TimeTable.Records.Count - 1 Then
      Return True
    Else
      Return False
    End If
  End Function

  Public Function InSeason(ByVal mySeason As STOCHLIB.GeneralFunctions.enmSeason) As Boolean
    Dim myDate As Date = TimeTable.Records.Values(TimeTable.Records.Count / 2 - 1).Datum
    Return Me.Setup.GeneralFunctions.DateInSeason(myDate, mySeason)
  End Function

  Public Function AnalyzePattern() As Boolean
    'classifies this event into one of the seven rainfall patterns according to STOWA, 2004
    Dim i As Long, j As Long, Sum As Double
    Dim A As New Dictionary(Of Integer, Double), V As New Dictionary(Of Integer, Double), c As New Dictionary(Of Integer, Double)
    Dim D As New Dictionary(Of Integer, Double)

    Try
      'berekent eerst kansen op uniform daarna hoog t/m uniform en tenslotte lang/kort
      'en doet dit CUMULATIEF
      Sum = 0
      For i = 0 To TimeTable.Records.Count - 1
        Sum = Sum + TimeTable.Records.Values(i).GetValue(0)
      Next

      'deel de periode op in acht vakken en bepaal de fractie van de neerslagsom voor elk van die achtsten
      Dim nStep As Integer = Duration.DurationHours / 8
      For j = 1 To 8
        For i = (j - 1) * nStep To j * nStep - 1
          If A.ContainsKey(j) Then
            A.Item(j) += TimeTable.Records.Values(i).GetValue(0) / Sum
          Else
            A.Add(j, TimeTable.Records.Values(i).GetValue(0) / Sum)
          End If
        Next
      Next

      'maak ook een dictionary met een lopend paar van twee achtsten en drie achtsten
      For j = 1 To 7
        V.Add(j, A.Item(j) + A.Item(j + 1))
      Next
      For j = 1 To 6
        D.Add(j, A.Item(j) + A.Item(j + 1) + A.Item(j + 2))
      Next

      'check if the sum of probabilities equals 1
      Dim checkSum As Double
      For j = 1 To 8
        checkSum += A.Item(j)
      Next
      If Not Math.Abs(1 - checkSum) < 0.01 Then
        MsgBox("Error: sum of probabilities for patterns does not add up to 1.")
      End If

      'sort the dictionary continaing the eight parts in descending order. do the same for the moving pairs of eights
      Dim ASorted = From entry In A Order By entry.Value Descending
      Dim VSorted = From entry In V Order By entry.Value Descending
      Dim DSorted = From entry In D Order By entry.Value Descending

      'start with 2 peaks; lang and kort than 1 peak, starting high and finally uniform
      'if the distance between the two highest peaks >= 6 then "lang"
      If Duration.DurationHours = 24 Then
        If Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 6 AndAlso ASorted(1).Value > 0.17 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LANG
        ElseIf Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 4 AndAlso ASorted(1).Value > 0.17 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.KORT
        ElseIf VSorted(0).Value > 0.812 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.HOOG
        ElseIf VSorted(0).Value > 0.679 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELHOOG
        ElseIf VSorted(0).Value > 0.564 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELLAAG
        ElseIf VSorted(0).Value > 0.46 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LAAG
        Else
          Pattern = GeneralFunctions.enmNeerslagPatroon.UNIFORM
        End If

      ElseIf Duration.DurationHours = 48 Then
        If Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 6 AndAlso ASorted(1).Value > 0.205 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LANG
        ElseIf Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 4 AndAlso ASorted(1).Value > 0.177 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.KORT
        ElseIf VSorted(0).Value > 0.767 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.HOOG
        ElseIf VSorted(0).Value > 0.658 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELHOOG
        ElseIf VSorted(0).Value > 0.562 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELLAAG
        ElseIf VSorted(0).Value > 0.453 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LAAG
        Else
          Pattern = GeneralFunctions.enmNeerslagPatroon.UNIFORM
        End If

      ElseIf Duration.DurationHours = 96 Then
        If Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 6 AndAlso ASorted(1).Value > 0.205 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LANG
        ElseIf Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 4 AndAlso ASorted(1).Value > 0.185 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.KORT
        ElseIf VSorted(0).Value > 0.716 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.HOOG
        ElseIf VSorted(0).Value > 0.605 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELHOOG
        ElseIf VSorted(0).Value > 0.508 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELLAAG
        ElseIf VSorted(0).Value > 0.43 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LAAG
        Else
          Pattern = GeneralFunctions.enmNeerslagPatroon.UNIFORM
        End If
      ElseIf Duration.DurationHours = 192 Then
        If Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 6 AndAlso ASorted(1).Value > 0.222 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LANG
        ElseIf Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 4 AndAlso ASorted(1).Value > 0.205 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.KORT
        ElseIf VSorted(0).Value > 0.66 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.HOOG
        ElseIf VSorted(0).Value > 0.555 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELHOOG
        ElseIf VSorted(0).Value > 0.487 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELLAAG
        ElseIf VSorted(0).Value > 0.42 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LAAG
        Else
          Pattern = GeneralFunctions.enmNeerslagPatroon.UNIFORM
        End If
      ElseIf Duration.DurationHours = 216 Then
        If Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 6 AndAlso ASorted(1).Value > 0.218 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LANG
        ElseIf Math.Abs(ASorted(0).Key - ASorted(1).Key) >= 4 AndAlso ASorted(1).Value > 0.208 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.KORT
        ElseIf VSorted(0).Value > 0.648 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.HOOG
        ElseIf VSorted(0).Value > 0.551 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELHOOG
        ElseIf VSorted(0).Value > 0.487 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.MIDDELLAAG
        ElseIf VSorted(0).Value > 0.423 Then
          Pattern = GeneralFunctions.enmNeerslagPatroon.LAAG
        Else
          Pattern = GeneralFunctions.enmNeerslagPatroon.UNIFORM
        End If
      End If
      Return True
    Catch ex As Exception
      Me.Setup.Log.AddError("Error analyzing rainfall patterns.")
      Return False
    End Try

  End Function


    Public Function CalculateSum(ModelParameter As GeneralFunctions.enmModelParameter) As Double
        Sum = 0
        For Each myRecord As clsTimeTableRecord In TimeTable.Records.Values
            Sum += myRecord.GetValue(ModelParameter)
        Next
        Return Sum
    End Function

    Public Function CalculateMedian(Modelparameter As GeneralFunctions.enmModelParameter) As Double
        Dim Values As New List(Of Double)
        For Each myRecord As clsTimeTableRecord In TimeTable.Records.Values
            Values.Add(myRecord.GetValue(Modelparameter))
        Next
        Values.Sort()
        Return Me.Setup.GeneralFunctions.PercentileFromList(Values, 0.5)
    End Function

    Public Function getMax(ModelParameter As GeneralFunctions.enmModelParameter) As Double
        Dim Max As Double = Double.NaN
        For Each myRecord As clsTimeTableRecord In TimeTable.Records.Values
            If Double.IsNaN(Max) Then
                Max = myRecord.GetValue(ModelParameter)
            Else
                If myRecord.GetValue(ModelParameter) > Max Then Max = myRecord.GetValue(ModelParameter)
            End If
        Next
    End Function

    Public Function getMin(ModelParameter As GeneralFunctions.enmModelParameter) As Double
        Dim Min As Double = Double.NaN
        For Each myRecord As clsTimeTableRecord In TimeTable.Records.Values
            If Double.IsNaN(Min) Then
                Min = myRecord.GetValue(ModelParameter)
            Else
                If myRecord.GetValue(ModelParameter) < Min Then Min = myRecord.GetValue(ModelParameter)
            End If
        Next
    End Function

End Class
