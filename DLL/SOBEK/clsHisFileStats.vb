Imports STOCHLIB.General

Public Class clsHisFileStats
  Private Setup As clsSetup

  Public Stats As New Dictionary(Of String, clsHisFileStat)

  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup
  End Sub

  Public Function GetAdd(ByVal myID As String) As clsHisFileStat
    Dim myStat As clsHisFileStat
    If Not Stats.ContainsKey(myID.Trim.ToUpper) Then
      myStat = New clsHisFileStat
      Stats.Add(myID.Trim.ToUpper, myStat)
      Return myStat
    Else
      Return Stats.Item(myID.Trim.ToUpper)
    End If
  End Function

  Public Sub AddMinMaxMean(ByVal myLocation As String, ByRef myTable As clsTimeTable, ByVal FieldIdx As Integer)
    Dim myStat As clsHisFileStat
    Dim mySum As Single, myVal As Double
    Dim myRecord As clsTimeTableRecord

    'add this stat to the dictionary
    myStat = GetAdd(myLocation.Trim.ToUpper)

    myStat.Max = -999
    myStat.Min = 999

    For Each myRecord In myTable.Records.Values
      myVal = myRecord.GetValue(FieldIdx)
      mySum += myVal
      If myVal > myStat.Max Then myStat.Max = myVal
      If myVal < myStat.Min Then myStat.Min = myVal
    Next

    myStat.Mean = mySum / myTable.Records.Count

  End Sub

  Public Sub AddTides(ByVal myLocation As String, ByRef myTable As clsTimeTable, ByVal FieldIdx As Integer)
    Dim myStat As clsHisFileStat
    'Dim mySum As Single, myVal As Double, , k As Long
    Dim myRecord As clsTimeTableRecord, nextRecord As clsTimeTableRecord
    Dim tsSeconds As Integer = myTable.GetTimestepSeconds
    Dim SearchRadius As Integer, i As Long, j As Long
    Dim curVal As Double, nextVal As Double, curDate As Date
    Dim IsMin As Boolean, IsMax As Boolean
    Dim myTide As clsTide

    myStat = GetAdd(myLocation.Trim.ToUpper)

    Select Case tsSeconds
      Case Is <= 600 '10 minutes
        SearchRadius = 30  '+5 and -5 hours search radius
      Case Is <= 900 '15 minutes
        SearchRadius = 30  '+5 and -5 hours search radius
      Case Is <= 3600 '1 hour
        SearchRadius = 30  '+5 and -5 hours search radius
      Case Else
        Me.Setup.Log.AddError("Could not extract tidal statistics for location " & myLocation)
        Exit Sub
    End Select

    Me.Setup.GeneralFunctions.UpdateProgressBar("Retrieving tidal statistics for " & myLocation, 0, 10)
    For i = 0 + SearchRadius To myTable.Records.Values.Count - 1 - SearchRadius
      Me.Setup.GeneralFunctions.UpdateProgressBar("", i, myTable.Records.Values.Count)
      myRecord = myTable.Records.Values(i)
      curVal = myRecord.GetValue(FieldIdx)
      curDate = myRecord.Datum

      IsMin = True
      IsMax = True

      'search backward
      For j = i - 1 To i - SearchRadius Step -1
        nextRecord = myTable.Records.Values(j)
        nextVal = nextRecord.GetValue(FieldIdx)
        If nextVal >= curVal Then IsMax = False 'note: the >= here and the > in the next section is importand in case of equal values!
        If nextVal <= curVal Then IsMin = False 'note: the <= here and the < in the next section is importand in case of equal values!
        If IsMax = False And IsMin = False Then Exit For
      Next

      'search forward
      For j = i + 1 To i + SearchRadius Step 1
        nextRecord = myTable.Records.Values(j)
        nextVal = nextRecord.GetValue(FieldIdx)
        If nextVal > curVal Then IsMax = False
        If nextVal < curVal Then IsMin = False
        If IsMax = False And IsMin = False Then Exit For
      Next

      'identify whether this point is a tidal min or max
      If IsMin Or IsMax Then
        myTide = New clsTide
        myTide.Value = curVal
        myTide.DateTime = curDate
        If IsMin Then
          myTide.Tide = clsTide.enmTide.Low
          myStat.LowTides.Add(myTide.DateTime.ToString, myTide)
        End If
        If IsMax Then
          myTide.Tide = clsTide.enmTide.High
          myStat.HighTides.Add(myTide.DateTime.ToString, myTide)
        End If
      End If
    Next

  End Sub

End Class
