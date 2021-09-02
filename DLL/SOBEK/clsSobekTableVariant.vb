Option Explicit On
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Public Class clsSobekTableVariant

  Friend ID As String
  Friend DateValStrings As New Dictionary(Of String, String)
  Friend Dates As New Dictionary(Of String, DateTime)
  Friend XValues As New Dictionary(Of String, Object) 'als het geen tijdtabel is
  Friend Values1 As New Dictionary(Of String, Object)
  Friend Values2 As New Dictionary(Of String, Object)
  Friend Values3 As New Dictionary(Of String, Object)
  Friend Values4 As New Dictionary(Of String, Object)
  Friend Values5 As New Dictionary(Of String, Object)
  Friend Values6 As New Dictionary(Of String, Object)
  Friend Values7 As New Dictionary(Of String, Object)
  Friend Values8 As New Dictionary(Of String, Object)
  Friend pdin1 As Integer '0 = continuous, 1 = block
  Friend pdin2 As Integer '0 = no return period, 1 = return period
  Friend PDINPeriod As String
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Friend Sub buildDateValStrings(Optional ByVal nVals As Integer = 6, Optional ByVal nDecimals As Integer = 7)
    Dim i As Long
    DateValStrings = New Dictionary(Of String, String)
    Dim myStr As String = ""
    Dim myVal As String

    Select Case nDecimals
      Case Is = 1
        myVal = Format(Values1.Values(i), "0.0")
      Case Is = 2
        myVal = Format(Values1.Values(i), "0.00")
      Case Is = 3
        myVal = Format(Values1.Values(i), "0.000")
      Case Is = 4
        myVal = Format(Values1.Values(i), "0.0000")
      Case Is = 5
        myVal = Format(Values1.Values(i), "0.00000")
      Case Is = 6
        myVal = Format(Values1.Values(i), "0.000000")
      Case Else
        myVal = Format(Values1.Values(i), "0.0000000")
    End Select

    For i = 0 To Dates.Count - 1
      Select Case nVals
        Case Is = 1
          myStr = "'" & Year(Dates.Values(i)) & "/" & Format(Month(Dates.Values(i)), "00") & "/" & Format(Day(Dates.Values(i)), "00") & ";" & Format(Hour(Dates.Values(i)), "00") & ":" & Format(Minute(Dates.Values(i)), "00") & ":" & Format(Second(Dates.Values(i)), "00") & "' " & myVal & " <"
        Case Is = 2
          myStr = "'" & Year(Dates.Values(i)) & "/" & Format(Month(Dates.Values(i)), "00") & "/" & Format(Day(Dates.Values(i)), "00") & ";" & Format(Hour(Dates.Values(i)), "00") & ":" & Format(Minute(Dates.Values(i)), "00") & ":" & Format(Second(Dates.Values(i)), "00") & "' " & myVal & " " & Values2.Values(i) & " <"
        Case Is = 3
          myStr = "'" & Year(Dates.Values(i)) & "/" & Format(Month(Dates.Values(i)), "00") & "/" & Format(Day(Dates.Values(i)), "00") & ";" & Format(Hour(Dates.Values(i)), "00") & ":" & Format(Minute(Dates.Values(i)), "00") & ":" & Format(Second(Dates.Values(i)), "00") & "' " & myVal & " " & Values2.Values(i) & " " & Values3.Values(i) & " <"
        Case Is = 4
          myStr = "'" & Year(Dates.Values(i)) & "/" & Format(Month(Dates.Values(i)), "00") & "/" & Format(Day(Dates.Values(i)), "00") & ";" & Format(Hour(Dates.Values(i)), "00") & ":" & Format(Minute(Dates.Values(i)), "00") & ":" & Format(Second(Dates.Values(i)), "00") & "' " & myVal & " " & Values2.Values(i) & " " & Values3.Values(i) & " " & Values4.Values(i) & " <"
        Case Is = 5
          myStr = "'" & Year(Dates.Values(i)) & "/" & Format(Month(Dates.Values(i)), "00") & "/" & Format(Day(Dates.Values(i)), "00") & ";" & Format(Hour(Dates.Values(i)), "00") & ":" & Format(Minute(Dates.Values(i)), "00") & ":" & Format(Second(Dates.Values(i)), "00") & "' " & myVal & " " & Values2.Values(i) & " " & Values3.Values(i) & " " & Values4.Values(i) & " " & Values5.Values(i) & " <"
        Case Is = 6
          myStr = "'" & Year(Dates.Values(i)) & "/" & Format(Month(Dates.Values(i)), "00") & "/" & Format(Day(Dates.Values(i)), "00") & ";" & Format(Hour(Dates.Values(i)), "00") & ":" & Format(Minute(Dates.Values(i)), "00") & ":" & Format(Second(Dates.Values(i)), "00") & "' " & myVal & " " & Values2.Values(i) & " " & Values3.Values(i) & " " & Values4.Values(i) & " " & Values5.Values(i) & " " & Values6.Values(i) & " <"
      End Select
      DateValStrings.Add(i.ToString, myStr)
    Next

  End Sub

  Friend Function InterpolateFromValues(ByVal Val As Double, Optional ByVal ValIdx As Integer = 1) As Single
    Dim i As Integer
    If ValIdx < 0 Then ValIdx = 1
    If ValIdx > 6 Then ValIdx = 6

    'bepaal eerst uit welke tabel we waarden gaan teruggeven
    Dim SearchDict As Dictionary(Of String, Object) = Nothing
    Select Case ValIdx
      Case Is = 1
        SearchDict = Values1
      Case Is = 2
        SearchDict = Values2
      Case Is = 3
        SearchDict = Values3
      Case Is = 4
        SearchDict = Values4
      Case Is = 5
        SearchDict = Values5
      Case Is = 6
        SearchDict = Values6
      Case Is = 7
        SearchDict = Values7
      Case Is = 8
        SearchDict = Values8
    End Select

    If Val <= SearchDict.Values(0) Then
      Return XValues.Values(0)
    ElseIf Val >= SearchDict.Values(SearchDict.Count - 1) Then
      Return XValues.Values(XValues.Values.Count - 1)
    ElseIf SearchDict.Count < 2 Then
      Return setup.GeneralFunctions.Interpolate(SearchDict.Values(0), XValues.Values(0), SearchDict.Values(1), XValues.Values(1), Val)
    Else
      For i = 0 To SearchDict.Values.Count - 2
        If SearchDict.Values(i) <= Val AndAlso Val <= SearchDict.Values(i + 1) Then
          'interpoleer lineair
          Return setup.GeneralFunctions.Interpolate(SearchDict.Values(i), XValues.Values(i), SearchDict.Values(i + 1), XValues.Values(i + 1), Val)
        End If
      Next
    End If

  End Function

  Friend Function InterpolateFromDates(ByVal myDate As Date, Optional ByVal valIdx As Integer = 1) As Single
    Dim i As Integer
    If valIdx < 0 Then valIdx = 1
    If valIdx > 6 Then valIdx = 6

    'bepaal eerst uit welke tabel we waarden gaan teruggeven
    Dim ReturnDict As Dictionary(Of String, Object) = Nothing
    Select Case valIdx
      Case Is = 1
        ReturnDict = Values1
      Case Is = 2
        ReturnDict = Values2
      Case Is = 3
        ReturnDict = Values3
      Case Is = 4
        ReturnDict = Values4
      Case Is = 5
        ReturnDict = Values5
      Case Is = 6
        ReturnDict = Values6
      Case Is = 7
        ReturnDict = Values7
      Case Is = 8
        ReturnDict = Values8
    End Select

    If myDate <= Dates.Values(0) Then
      Return ReturnDict.Values(0)
    ElseIf myDate >= Dates.Values(Dates.Count - 1) Then
      Return ReturnDict.Values(ReturnDict.Values.Count - 1)
    ElseIf Dates.Count < 2 Then
      Return setup.GeneralFunctions.Interpolate(Dates.Values(0).ToOADate, ReturnDict.Values(0), Dates.Values(1).ToOADate, ReturnDict.Values(1), myDate.ToOADate)
    Else
      For i = 0 To Dates.Values.Count - 2
        If Dates.Values(i + 1) = myDate Then
          Return ReturnDict.Values(i + 1)
        ElseIf Dates.Values(i) <= myDate AndAlso Dates.Values(i + 1) >= myDate Then
          'interpoleer lineair
          Return setup.GeneralFunctions.Interpolate(Dates.Values(i).ToOADate, ReturnDict.Values(i), Dates.Values(i + 1).ToOADate, ReturnDict.Values(i + 1), myDate.ToOADate)
        End If
      Next
    End If
  End Function

  Friend Function InterpolateXValues(ByVal Xval As Double, Optional ByVal ValIdx As Integer = 1) As Single
    Dim i As Integer
    If ValIdx < 0 Then ValIdx = 1
    If ValIdx > 6 Then ValIdx = 6

    'bepaal eerst uit welke tabel we waarden gaan teruggeven
    Dim ReturnDict As Dictionary(Of String, Object) = Nothing
    Select Case ValIdx
      Case Is = 1
        ReturnDict = Values1
      Case Is = 2
        ReturnDict = Values2
      Case Is = 3
        ReturnDict = Values3
      Case Is = 4
        ReturnDict = Values4
      Case Is = 5
        ReturnDict = Values5
      Case Is = 6
        ReturnDict = Values6
      Case Is = 7
        ReturnDict = Values7
      Case Is = 8
        ReturnDict = Values8
    End Select

    If XValues.ContainsKey(Str(Xval)) Then
      Return ReturnDict.Item(Str(Xval))
    ElseIf Xval <= XValues.Values(0) Then
      Return ReturnDict.Values(0)
    ElseIf Xval >= XValues.Values(XValues.Count - 1) Then
      Return ReturnDict.Values(ReturnDict.Values.Count - 1)
    ElseIf XValues.Count < 2 Then
      Return setup.GeneralFunctions.Interpolate(XValues.Values(0), ReturnDict.Values(0), XValues.Values(1), ReturnDict.Values(1), Xval)
    Else
      For i = 0 To XValues.Values.Count - 2
        If Xval >= XValues.Values(i) AndAlso Xval <= XValues.Values(i + 1) Then
          If XValues.Values(i) = Xval Then
            Return ReturnDict.Values(i)
          ElseIf XValues.Values(i + 1) = Xval Then
            Return ReturnDict.Values(i + 1)
          ElseIf XValues.Values(i) < Xval AndAlso XValues.Values(i + 1) > Xval Then
            Return setup.GeneralFunctions.Interpolate(XValues.Values(i), ReturnDict.Values(i), XValues.Values(i + 1), ReturnDict.Values(i + 1), Xval)
          End If
        End If
      Next
    End If

  End Function

  Friend Sub Read(ByVal myTable As String)
    Dim myRecords() As String, tmp As String
    Dim myDate As DateTime

    'verwijder alle tokens
    myTable = Replace(myTable, "TBLE", "")
    myTable = Replace(myTable, "tble", "")

    myRecords = Split(myTable, "<")
    For i As Integer = 0 To UBound(myRecords) - 1
      Dim j As Integer = 0
      While Not myRecords(i) = ""
        tmp = Me.setup.GeneralFunctions.ParseString(myRecords(i))
        j += 1
        If j = 1 AndAlso InStr(tmp, "/") > 0 Then
          myDate = Me.setup.GeneralFunctions.ConvertToDateTime(tmp, "yyyy/MM/dd;HH:mm:ss")
          Me.Dates.Add(Str(i).Trim, myDate)
        ElseIf j = 1 Then
          Me.XValues.Add(Str(i).Trim, tmp)
        End If

        If j = 2 Then Values1.Add(Str(i).Trim, tmp)
        If j = 3 AndAlso Not tmp = "<" Then Me.Values2.Add(Str(i).Trim, tmp)
        If j = 4 AndAlso Not tmp = "<" Then Me.Values3.Add(Str(i).Trim, tmp)
        If j = 5 AndAlso Not tmp = "<" Then Me.Values4.Add(Str(i).Trim, tmp)
        If j = 6 AndAlso Not tmp = "<" Then Me.Values5.Add(Str(i).Trim, tmp)
        If j = 7 AndAlso Not tmp = "<" Then Me.Values6.Add(Str(i).Trim, tmp)
        If j = 8 AndAlso Not tmp = "<" Then Me.Values7.Add(Str(i).Trim, tmp)
        If j = 9 AndAlso Not tmp = "<" Then Me.Values8.Add(Str(i).Trim, tmp)
      End While
    Next i
  End Sub
  Friend Function GetPeriod() As Integer()
    Dim DateString As String
    DateString = PDINPeriod
    Dim Values(4) As Integer
    Values(1) = Me.setup.GeneralFunctions.ParseString(DateString, ";")
    Values(2) = Me.setup.GeneralFunctions.ParseString(DateString, ":")
    Values(3) = Me.setup.GeneralFunctions.ParseString(DateString, ":")
    Values(4) = Me.setup.GeneralFunctions.ParseString(DateString, ":")
    GetPeriod = Values
  End Function

  Friend Sub AddDataPair(ByVal nVals As Integer, ByVal xval As Double, ByVal val1 As Double, Optional ByVal val2 As Double = 0, _
                         Optional ByVal val3 As Double = 0, Optional ByVal val4 As Double = 0, _
                         Optional ByVal val5 As Double = 0, Optional ByVal val6 As Double = 0)
    'Dim myStr As String = Str(XValues.Count)
    Dim myStr As String = Str(xval)

    '20121002 siebe: als key een string van de x-waarde ingezet om het zoeken in de tabel sneller te laten lopen
    If Not XValues.ContainsKey(myStr) AndAlso Not Values1.ContainsKey(myStr) Then
      If nVals >= 1 Then XValues.Add(myStr, xval)
      If nVals >= 2 Then Values1.Add(myStr, val1)
      If nVals >= 3 Then Values2.Add(myStr, val2)
      If nVals >= 4 Then Values3.Add(myStr, val3)
      If nVals >= 5 Then Values4.Add(myStr, val4)
      If nVals >= 6 Then Values5.Add(myStr, val5)
      If nVals >= 7 Then Values6.Add(myStr, val6)
    End If

  End Sub

  Friend Sub AddDate(ByVal myDate As DateTime)
    Dim mystr As String = Str(Dates.Count).Trim
    Dates.Add(mystr, myDate)
  End Sub

  Friend Sub AddValue1(ByVal myVal As Double)
    Dim myStr As String = Str(Values1.Count).Trim
    Values1.Add(myStr, myVal)
  End Sub

  Friend Sub AddDatevalPair(ByVal myDate As DateTime, ByVal val1 As Double, Optional ByVal val2 As Double = 0, _
                            Optional ByVal val3 As Double = 0, Optional ByVal val4 As Double = 0, _
                            Optional ByVal val5 As Double = 0, Optional ByVal val6 As Double = 0, Optional ByVal nVals As Integer = 1)
    Dim myStr As String = Str(Dates.Count).Trim
    Dates.Add(myStr, myDate)
    Values1.Add(myStr, val1)
    If nVals >= 2 Then Values2.Add(myStr, val2)
    If nVals >= 3 Then Values3.Add(myStr, val3)
    If nVals >= 4 Then Values4.Add(myStr, val4)
    If nVals >= 5 Then Values5.Add(myStr, val5)
    If nVals >= 6 Then Values6.Add(myStr, val6)
  End Sub

  Friend Function getValue1(ByVal XVal As Double) As Double
    'interpoleert een waarde uit de XValues/Values1-dataset
    Dim i As Integer

    If XVal < XValues.Values(0) Then
      Return Values1.Values(0)
    ElseIf XVal > XValues.Values(XValues.Count - 1) Then
      Return Values1.Values(Values1.Count - 1)
    End If

    For i = 0 To XValues.Count - 2
      If XValues.Values(i) = XVal Then
        Return Values1.Values(i)
      ElseIf XValues.Values(i + 1) = XVal Then
        Return Values1.Values(i + 1)
      ElseIf XValues.Values(i) < XVal AndAlso XValues.Values(i + 1) > XVal Then
        Return setup.GeneralFunctions.Interpolate(XValues.Values(i), Values1.Values(i), XValues.Values(i + 1), Values1.Values(i + 1), XVal)
      End If
    Next
  End Function

End Class
