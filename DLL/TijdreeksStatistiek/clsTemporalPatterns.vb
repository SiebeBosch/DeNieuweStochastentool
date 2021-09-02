Imports STOCHLIB.General
Public Class clsTemporalPatterns
    Public Patterns As Dictionary(Of String, clsTemporalPattern)
    Dim Duration As Integer
    Dim LocationID As String
    Dim Parameter As String

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, myDuration As Integer)
        Setup = mySetup
        Duration = myDuration
        Patterns = New Dictionary(Of String, clsTemporalPattern)
    End Sub


    Public Sub New(ByRef mySetup As clsSetup, myDuration As Integer, myParameter As String, myLocation As String)
        Setup = mySetup
        Duration = myDuration
        Parameter = myParameter
        LocationID = myLocation
        Patterns = New Dictionary(Of String, clsTemporalPattern)
    End Sub

    Public Function CreateOnePeakPatterns() As Boolean
        Dim nPatterns As Integer
        Dim PeakPercentage As Integer
        Dim PeakCenterStepSize As Integer
        Dim Skewness As Integer

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Generating one-peak patterns...", 0, 100, True)
            For PeakPercentage = 0 To 90 Step 10
                Me.Setup.GeneralFunctions.UpdateProgressBar("", PeakPercentage, 100, True)
                Dim minWidth As Integer = 3
                Dim maxWidth As Integer = Me.Setup.GeneralFunctions.MakeOdd(GetDuration, False)

                'vary the single peak width
                For PeakWidth = minWidth To maxWidth Step 2
                    Dim minCenterIdx As Integer = (PeakWidth - 1) / 2
                    Dim maxCenterIdx As Integer = (GetDuration() - 1) - (PeakWidth - 1) / 2

                    'vary the starting moment of this peak
                    PeakCenterStepSize = (GetDuration() - PeakWidth) / 5
                    If PeakCenterStepSize >= 1 Then
                        For PeakCenter = minCenterIdx To maxCenterIdx Step PeakCenterStepSize
                            'create five degrees of skewnesses

                            'pinnacle all the way to the left of the peak
                            nPatterns += 1
                            Skewness = -(PeakWidth - 1) / 2
                            CreateOnePeakPattern(PeakPercentage, PeakWidth, PeakCenter, Skewness)

                            'pinnacle at one fifth of the peak's width, tilted to the left
                            If PeakWidth >= 5 Then
                                nPatterns += 1
                                Skewness = -(PeakWidth - 1) / 2 + (PeakWidth) * 1 / 5
                                CreateOnePeakPattern(PeakPercentage, PeakWidth, PeakCenter, Skewness)
                            End If

                            'pinnacle exactly in the center of the peak
                            nPatterns += 1
                            Skewness = -(PeakWidth) / 5
                            CreateOnePeakPattern(PeakPercentage, PeakWidth, PeakCenter, 0)

                            'pinnacle at four fifths of the peak
                            If PeakWidth >= 5 Then
                                nPatterns += 1
                                Skewness = (PeakWidth - 1) / 2 - (PeakWidth) * 1 / 5
                                CreateOnePeakPattern(PeakPercentage, PeakWidth, PeakCenter, Skewness)
                            End If

                            'pinnacle all the way to the right of the peak
                            nPatterns += 1
                            Skewness = (PeakWidth - 1) / 2
                            CreateOnePeakPattern(PeakPercentage, PeakWidth, PeakCenter, Skewness)
                        Next
                    End If
                Next
            Next


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function RedistributeAndDecreaseSubselection(MaxPatterns As Integer, ByRef Events As clsHydroEvents, Duration As Integer) As Boolean
        'this function will de-activate all lesser used patterns.
        'it will also clear all assigned events
        'it does so by sorting the patterns in descending order by the number of assigned events
        Try
            Dim PatternsSorted As New Dictionary(Of String, clsTemporalPattern)
            Dim iPattern As Integer
            PatternsSorted = (From entry In Patterns Order By entry.Value.Events.Count Descending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
            Dim iEvent As Integer, nEvents As Integer

            'set all lesser used patterns to InUse=False
            For iPattern = MaxPatterns To PatternsSorted.Count - 1
                PatternsSorted.Values(iPattern).InUse = False
            Next

            'clear all previously assigned events, allowing us to reclassify all events on just the remaining patterns.
            For iPattern = 0 To PatternsSorted.Count - 1
                PatternsSorted.Values(iPattern).Events = New List(Of clsHydroEvent)
            Next

            'now we're ready to re-assign to just the remaining active events
            Me.Setup.GeneralFunctions.UpdateProgressBar("Classifying events...", 0, 10, True)
            iEvent = 0
            For Each myEvent As clsHydroEvent In Events.Events.Values
                iEvent += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iEvent, nEvents)
                ClassifyEvent(myEvent)
            Next

            'replace our dictionary of patterns by the newly organized and sorted patterns dictionary
            Patterns = PatternsSorted

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return True
        End Try
    End Function

    Public Function ClassifyEvents(ByRef Events As clsHydroEvents) As Boolean
        Try
            'next we can classify the events by pattern
            Dim iEvent As Integer = 0, nEvents As Integer = Events.Events.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Classifying events...", 0, nEvents, True)
            For Each myEvent As clsHydroEvent In Events.Events.Values
                iEvent += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iEvent, nEvents)
                ClassifyEvent(myEvent)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Classification complete", 0, nEvents, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ClassifyEvents of class clsTemporalPatterns.")
            Return False
        End Try
    End Function

    Public Function ClassifyEvent(ByRef myEvent As clsHydroEvent) As clsTemporalPattern
        Try
            Dim minRMSE As Double = 9.0E+99
            Dim myRMSE As Double, bestPattern As clsTemporalPattern = Nothing
            Dim nPatternsInUse As Integer = 0
            For Each myPattern As clsTemporalPattern In Patterns.Values
                If myPattern.InUse Then
                    nPatternsInUse += 1
                    If myPattern.calcRMSE(myEvent.Values, myEvent.EventSum, myRMSE) Then
                        If myRMSE < minRMSE Then
                            minRMSE = myRMSE
                            bestPattern = myPattern
                        End If
                    Else
                        Throw New Exception("Error calculating Root Mean Square Error for event: " & myEvent.GetLocationID & "," & myEvent.GetParameter & "," & myEvent.getDuration)
                    End If
                Else
                    'Me.Setup.Log.AddError("Error computing RMSE for pattern " & myPattern.GetID & " and event " & EventNum)
                End If
            Next

            If bestPattern Is Nothing Then
                Me.Setup.Log.AddError("Error assigning event to best matching pattern. " & nPatternsInUse & " are in use and have been analyzed but no match was found.")
                Throw New Exception("Error assigning event to best matching pattern. No best pattern was available.")
            End If

            'now that we've processed all patterns, assign the given event to the appropriate pattern
            bestPattern.AddEvent(myEvent)

            Return bestPattern
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function

    Public Function ClassificationResultsToExcel() As Boolean
        Try
            Dim ws As clsExcelSheet
            Dim c As Integer, ts As Integer, r As Integer, ipattern As Integer



            For Each myPattern As clsTemporalPattern In Patterns.Values
                ipattern += 1
                If myPattern.Events.Count > 0 Then
                    ws = Me.Setup.ExcelFile.GetAddSheet("Pattern_" & ipattern)
                    c = 1
                    ws.ws.Cells(0, 0).Value = myPattern.GetID
                    ws.ws.Cells(0, 1).Value = "Fraction"
                    For ts = 1 To myPattern.Fractions.Count
                        ws.ws.Cells(r + ts, 0).Value = ts
                        ws.ws.Cells(r + ts, 1).Value = myPattern.Fractions(ts - 1)
                    Next
                    For Each myEvent As clsHydroEvent In myPattern.Events
                        c += 1
                        ws.ws.Cells(r, c).Value = myEvent.GetEventNum
                        For ts = 1 To myEvent.Values.Count
                            ws.ws.Cells(r + ts, c).Value = myEvent.Values(ts - 1) / myEvent.EventSum
                        Next
                    Next
                End If
            Next
            Me.Setup.ExcelFile.Save(True)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ReadFromDatabase(TableName As String) As Boolean
        Try
            Dim i As Integer, j As Integer
            Dim pt As New DataTable
            Dim myPattern As clsTemporalPattern
            Dim query As String = "SELECT DISTINCT CLASSID FROM " & TableName & " WHERE DURATION=" & Duration & ";"

            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, pt)

            'clear the collection of patterns from memory (if present)
            Patterns = New Dictionary(Of String, clsTemporalPattern)

            Me.Setup.GeneralFunctions.UpdateProgressBar("Reading patterns from database...", 0, 10, True)

            pt = New DataTable
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT CLASSID, TIMESTEP, FRACTION FROM " & TableName & " WHERE DURATION=" & Duration & " ORDER BY CLASSID, TIMESTEP;", pt)
            For i = 0 To pt.Rows.Count - Duration Step Duration
                myPattern = New clsTemporalPattern(Me.Setup, pt.Rows(i)(0))
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, pt.Rows.Count)
                For j = i To i + Duration - 1
                    myPattern.Fractions.Add(pt.Rows(j)(2))
                Next
                Patterns.Add(myPattern.GetID.Trim.ToUpper, myPattern)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Patterns succesfully read from database", 0, 10, True)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function writePatternClassesToDatabase(TableName As String) As Boolean
        Try
            Dim queryStart As String = "INSERT INTO " & TableName & " (CLASSID,DURATION,TIMESTEP,FRACTION) VALUES "
            Dim query As String = ""
            Dim i As Integer, iPat As Integer = 0

            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing pattern classes to database...", 0, 10, True)
            Me.Setup.SqliteCon.Open()
            Using cmd As New SQLite.SQLiteCommand
                cmd.Connection = Me.Setup.SqliteCon
                Using transaction = Me.Setup.SqliteCon.BeginTransaction
                    For Each myPattern As clsTemporalPattern In Patterns.Values
                        iPat += 1
                        Me.Setup.GeneralFunctions.UpdateProgressBar("", iPat, Patterns.Count)
                        For i = 0 To myPattern.Fractions.Count - 1
                            query = queryStart & "('" & myPattern.GetID & "'," & myPattern.Fractions.Count & "," & i + 1 & "," & myPattern.Fractions(i) & ");"
                            cmd.CommandText = query
                            cmd.ExecuteNonQuery()
                        Next
                    Next
                    transaction.Commit() 'this is where the bulk insert is finally executed.
                End Using
            End Using
            Me.Setup.SqliteCon.Close()
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function writePatternClassesToDatabase: " & ex.Message)
            Return False
        End Try
    End Function


    Public Function WriteToDatabase(TableName As String, IncludeLocationAndParameter As Boolean) As Boolean
        Try
            Dim i As Integer
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing patterns to database...", 0, 10, True)
            For Each myPattern As clsTemporalPattern In Patterns.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Patterns.Count)
                If IncludeLocationAndParameter Then
                    myPattern.WriteToDatabase(TableName, LocationID, Parameter)
                Else
                    myPattern.WriteToDatabase(TableName)
                End If
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Patterns stored successfully.", 0, 10, True)
            Me.Setup.SqliteCon.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function WriteClassesToDatabase(TableName As String) As Boolean
        Try
            Dim i As Integer
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing pattern classes to database...", 0, 10, True)
            For Each myPattern As clsTemporalPattern In Patterns.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Patterns.Count)
                myPattern.WriteClassToDatabase(TableName)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Pattern classes stored successfully.", 0, 10, True)
            Me.Setup.SqliteCon.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ClearDatabase(TableName As String) As Boolean
        Try
            Dim query As String = "DELETE FROM " & TableName & " WHERE DURATION=" & Duration & ";"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GetDuration() As Integer
        Return Duration
    End Function

    Public Function GetCenterTimestepIdx() As Integer
        Return (Duration - 1) / 2
    End Function

    Public Sub CreateUniform()
        'creates a uniform pattern
        Dim myPattern As New clsTemporalPattern(Me.Setup, "Uniform")
        For i = 0 To Duration - 1
            myPattern.AddValue(1 / Duration)
        Next
        Patterns.Add("Uniform", myPattern)
    End Sub

    Public Sub CreateAscendingDescending(EndStartMultiplier As Double)
        'this creates as uniformly ascending or descending pattern. The multiplier indicates the ration between the last and first value
        Dim Name As String
        If EndStartMultiplier >= 1 Then Name = "Ascending_" Else Name = "Descending_"
        Dim myPattern As New clsTemporalPattern(Me.Setup, Name & EndStartMultiplier)
        For i = 0 To Duration - 1
            Dim myVal As Double = Setup.GeneralFunctions.Interpolate(0, 1, Duration - 1, EndStartMultiplier, i)
            myPattern.AddValue(myVal)
        Next

        'now scale back so that the sum over the entire event matches 1 and add to the dictionary
        myPattern.MakeUniform()
        Patterns.Add(myPattern.GetID.Trim.ToUpper, myPattern)
    End Sub

    Public Function CreateOnePeakPattern(BasePercentage As Double, PeakWidthTimesteps As Integer, PeakCenterTimestepIdx As Integer, Skewness As Integer) As Boolean
        Try
            'this creates a one-peak pattern where the base volume is given. The peak volume is then calculated by using the given peak width
            'timesteps * base_height = base_percentage
            'peak_percentage = 100 - base_percentage
            'peak_width * peak_extra_height * 1/2 = peak_percentage
            'peak_extra_height = peak_percentage * 2 / peak width

            If Setup.GeneralFunctions.IsEven(PeakWidthTimesteps) Then Throw New Exception("Error: Peak Width must be an odd number.")
            Dim PeakStartTimeStepIdx As Integer = PeakCenterTimestepIdx - (PeakWidthTimesteps - 1) / 2
            Dim PeakEndTimestepIdx As Integer = PeakCenterTimestepIdx + (PeakWidthTimesteps - 1) / 2

            If PeakStartTimeStepIdx < 0 Then Throw New Exception("Error generating one peak pattern since the required center point was too close to the start of the event.")
            If PeakEndTimestepIdx > Duration - 1 Then Throw New Exception("Error generating one peak pattern since the required center point was too close to the end of the event.")

            Dim baseHeight As Double = BasePercentage / Duration
            Dim peakExtraHeight As Double = (100 - BasePercentage) * 2 / PeakWidthTimesteps
            Dim myPattern As New clsTemporalPattern(Me.Setup, "OnePeak BasePerc=" & BasePercentage & ";width=" & PeakWidthTimesteps & ";center=" & PeakCenterTimestepIdx & ";skew=" & Skewness)

            For i = 0 To Duration - 1
                Dim myValue As Double = baseHeight
                Dim addValue As Double = 0
                If i < (PeakCenterTimestepIdx + Skewness) Then
                    addValue = Math.Max(0, Me.Setup.GeneralFunctions.Interpolate(PeakStartTimeStepIdx, 0, PeakCenterTimestepIdx + Skewness, peakExtraHeight, i))
                ElseIf i > PeakCenterTimestepIdx + Skewness Then
                    addValue = Math.Max(0, Me.Setup.GeneralFunctions.Interpolate(PeakCenterTimestepIdx + Skewness, peakExtraHeight, PeakEndTimestepIdx, 0, i))
                ElseIf i = PeakCenterTimestepIdx + Skewness Then
                    addValue = peakExtraHeight
                End If
                myPattern.AddValue(myValue + addValue)
            Next
            myPattern.MakeUniform()
            If Patterns.ContainsKey(myPattern.GetID.Trim.ToUpper) Then Throw New Exception("Error: pattern already exists in collection and was skipped: " & myPattern.GetID)
            Patterns.Add(myPattern.GetID.Trim.ToUpper, myPattern)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

End Class
