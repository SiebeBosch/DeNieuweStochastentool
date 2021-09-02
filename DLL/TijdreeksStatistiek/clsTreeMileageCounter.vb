Imports STOCHLIB.General

Public Class clsTreeMileageCounter

    Dim BranchIndexTable As List(Of List(Of Integer)) 'this table contains for each cell the index number of the branch it belongs to
    Dim ValIndexTable As List(Of List(Of Integer)) 'our tree structure also needs to keep track which values are currently active/selected.
    Dim BlockSizeTable As List(Of List(Of Integer))

    'Dim FromIndexArray(,) As Integer 'each cell can be given an upper and lower bound for its values
    'Dim ToIndexArray(,) As Integer 'each cell can be given an upper and lower bound for its values

    'hard coded for now: increments of 10%
    Dim PercentageValues As New List(Of Double)


    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, Duration As Integer, Divisions As List(Of Integer), PercentageList As List(Of Double))
        Try
            Setup = mySetup
            PercentageValues = PercentageList
            Initialize(Duration, Divisions)
        Catch ex As Exception

        End Try

    End Sub


    Public Function Initialize(Duration As Integer, Divisions As List(Of Integer)) As Boolean
        Try
            'initialize the tables
            BranchIndexTable = New List(Of List(Of Integer))
            ValIndexTable = New List(Of List(Of Integer))
            BlockSizeTable = New List(Of List(Of Integer))
            Dim myDivision As Integer = 1

            'pouplate the branch index table, the blocksize table and the valueindex table
            Dim BranchList As List(Of Integer)
            Dim BlockList As List(Of Integer)
            Dim ValueIndexList As List(Of Integer)
            BranchList = New List(Of Integer)
            BlockList = New List(Of Integer)
            ValueIndexList = New List(Of Integer)
            BranchList.Add(0)
            BranchIndexTable.Add(BranchList)
            ValueIndexList.Add(-1)
            ValIndexTable.Add(ValueIndexList)
            BlockList.Add(Duration)
            BlockSizeTable.Add(BlockList)
            For i = 0 To Divisions.Count - 1
                BranchList = New List(Of Integer)
                BlockList = New List(Of Integer)
                ValueIndexList = New List(Of Integer)
                myDivision *= Divisions(i)
                For j = 0 To myDivision - 1
                    BranchList.Add(j)
                    BlockList.Add(Duration / myDivision)
                    ValueIndexList.Add(-1)
                Next
                BranchIndexTable.Add(BranchList)
                BlockSizeTable.Add(BlockList)
                ValIndexTable.Add(ValueIndexList)
            Next

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function InitializeValuesIndexArray() As Boolean
        'set the values index array to the first possible value
        Try
            For Each ValIndexRow As List(Of Integer) In ValIndexTable
                For i = 0 To ValIndexRow.Count - 1
                    ValIndexRow(i) = 0
                Next
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function OneUp() As Boolean
        Try
            Dim r As Integer, c As Integer
            'returns true if succeeds in adding one to the combination
            For r = ValIndexTable.Count - 1 To 0 Step -1
                For c = ValIndexTable(r).Count - 1 To 0 Step -1
                    If ValIndexTable(r)(c) < 0 Then
                        InitializeValuesIndexArray()
                    ElseIf ValIndexTable(r)(c) < PercentageValues.Count - 1 Then
                        'we have found an open space to increase. all further values have already been reset to 0
                        ValIndexTable(r)(c) += 1
                        Return True
                    ElseIf ValIndexTable(r)(c) >= PercentageValues.Count - 1 Then
                        'the current digit is at its maximum, so reset to index 0 and move on to the previous digit
                        ValIndexTable(r)(c) = 0
                    End If
                Next
            Next
            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function currentValid(CheckSum As Double) As Boolean
        Dim r As Integer, c As Integer
        For r = 0 To ValIndexTable.Count - 1
            Dim curSum As Double
            For c = 0 To ValIndexTable(r).Count - 1
                curSum += ValIndexTable(r)(c)
            Next
            If Not curSum = CheckSum Then Return False
        Next
        Return True
    End Function

    Public Function currentToDatabase(TABLENAME As String, PatternID As String, Parameter As String, Duration As Integer) As Boolean
        Try
            Dim r As Integer, c As Integer, i As Integer
            Dim query As String = "INSERT INTO " & TABLENAME & " (PATTERNID, PARAMETER, DURATION, TIMESTEP, FRACTION) VALUES "
            r = ValIndexTable.Count - 1
            For c = 0 To ValIndexTable(r).Count - 1
                For i = 0 To BlockSizeTable(r)(c) - 1
                    query &= "('" & PatternID & "','" & Parameter & "'," & Duration & "," & i + 1 & "," & PercentageValues(ValIndexTable(r)(c) / 100) & "),"
                Next
                'finalize the query
                query = Left(query, query.Length - 1) & ";"
                Me.Setup.GeneralFunctions.SQLitenoQuery(Me.Setup.SqliteCon, query, False)
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
