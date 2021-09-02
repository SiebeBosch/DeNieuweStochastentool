Option Explicit On
Imports STOCHLIB.General

Friend Class clsDischargeSum
  Friend TimeTable As clsSobekTable
  Private setup As clsSetup

  Friend Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
    TimeTable = New clsSobekTable(Me.setup)
  End Sub

    'Friend Sub AddNodeResult(ByRef HisFile As clsHisFile, ByVal Loc As String, ByVal Par As String)
    '  Dim iTim As Long
    '  Dim iLoc As Long
    '  Dim iPar As Long

    '  'iLoc = HisFile.getlocIdx(Loc)
    '  'iPar = HisFile.getParIdx(Par)

    '  If iLoc > 0 And iPar > 0 Then
    '    For iTim = 1 To HisFile.nTim
    '      If TimeTable.Dates.ContainsKey(iTim.ToString) Then
    '      Else
    '        'TimeTable.AddDatevalPair(HisFile.Tim(itim)

    '      End If
    '    Next
    '  End If

    'End Sub

End Class
