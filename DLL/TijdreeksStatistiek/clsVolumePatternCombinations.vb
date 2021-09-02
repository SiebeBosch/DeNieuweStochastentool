Imports STOCHLIB.General

Public Class clsVolumePatternCombinations
    'this class contains all combinations of a temporal pattern & volume class
    Dim Combinations As Dictionary(Of String, clsVolumePatternCombination)
    Dim VolumeClasses As clsVolumeClasses
    Dim Patterns As clsTemporalPatterns
    Dim Events As clsHydroEvents

    Dim LocationID As String
    Dim Parameter As String
    Dim Duration As Integer

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByRef myEvents As clsHydroEvents, myLocation As String, myParameter As String, myDuration As Integer, ByRef myVolumeClasses As clsVolumeClasses, ByRef myPatterns As clsTemporalPatterns)
        Setup = mySetup
        LocationID = myLocation
        Parameter = myParameter
        Duration = myDuration

        'set the list of volume classes and patterns
        VolumeClasses = myVolumeClasses
        Patterns = myPatterns
        Events = myEvents

        'establish the dictionary of unique combinations of volume class and pattern
        Combinations = New Dictionary(Of String, clsVolumePatternCombination)
    End Sub


    Public Function ClassifyEvents(ByRef Events As clsHydroEvents, LocationID As String, Parameter As String, duration As Integer, WriteToDatabase As Boolean) As Boolean
        Try
            'start by cleaning up the 'old' classes
            Dim query As String
            query = "DELETE FROM VOLUMEPATTERNCOMBINATIONS WHERE LOCATIONID='" & LocationID & "' AND PARAMETER='" & Parameter & "' AND DURATION=" & duration & ";"
            Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqLiteCon, query)

            Dim iEvent As Integer = 0, nEvents As Integer = Events.Events.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Classifying events by volume and pattern.", 0, 10, True)
            For Each myEvent As clsHydroEvent In Events.Events.Values
                iEvent += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iEvent, nEvents)
                ClassifyEvent(myEvent, WriteToDatabase)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Classification complete.", 0, 10, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function ClassifyEvents of class clsVolumePatternCombinations.")
            Return False
        End Try
    End Function

    Public Function ClassifyEvent(ByRef myEvent As clsHydroEvent, WriteToDatabase As Boolean) As Boolean
        Try
            Dim myVol As clsVolumeClass = VolumeClasses.GetByVolume(myEvent.EventSum)
            Dim myPat As clsTemporalPattern = Patterns.ClassifyEvent(myEvent)
            Dim Combi As clsVolumePatternCombination
            Combi = GetAdd(myVol, myPat, myEvent.getDuration)
            Combi.AddEvent(myEvent)

            If WriteToDatabase Then
                Dim query As String = "INSERT INTO VOLUMEPATTERNCOMBINATIONS (LOCATIONID, PARAMETER, DURATION, VOLUMECLASS, PATTERNID, EVENTNUM) VALUES ('" & LocationID & "','" & Parameter & "'," & Duration & ",'" & myVol.getID & "','" & myPat.GetID & "'," & myEvent.GetEventNum & ");"
                Me.Setup.GeneralFunctions.SQLitenoQuery(Me.Setup.SqliteCon, query, False)
            End If
            Me.Setup.SqliteCon.Close()
            Return True
        Catch ex As Exception
            Return False
        Finally
            'Me.Setup.SqLiteCon.Close()
        End Try
    End Function


    Public Function GetAdd(ByRef myVolumeClass As clsVolumeClass, ByRef myPattern As clsTemporalPattern, ByVal myDuration As Integer) As clsVolumePatternCombination
        Dim myKey As String = myVolumeClass.getID.Trim.ToUpper & "_" & myPattern.GetID.Trim.ToUpper

        If Combinations.ContainsKey(myKey.Trim.ToUpper) Then
            Return Combinations.Item(myKey.Trim.ToUpper)
        Else
            Dim myCombination As New clsVolumePatternCombination(Me.Setup, myVolumeClass, myPattern, myDuration)
            Combinations.Add(myKey.Trim.ToUpper, myCombination)
        End If

        Return Combinations.Item(myKey.Trim.ToUpper)
    End Function

    Public Function writeToExcelWorkbook()
        Try
            Dim ws As clsExcelSheet
            Dim c As Integer, ts As Integer, iCombi As Integer, r As Integer, myKey As String
            Dim p As Integer, v As Integer
            Dim myPattern As clsTemporalPattern, myVol As clsVolumeClass

            '---------------------------------------------------------------------------------------------------------
            'first write a worksheet with statistics
            'write the patterns to the rows and volume classes to the columns
            '---------------------------------------------------------------------------------------------------------
            ws = Me.Setup.ExcelFile.GetAddSheet("Probability")
            c = 0
            r = 1
            ws.ws.Cells(0, 0).Value = "Volume class ID"
            ws.ws.Cells(1, 0).Value = "Representative volume"
            For p = 0 To Patterns.Patterns.Count - 1
                myPattern = Patterns.Patterns.Values(p)
                If myPattern.InUse Then
                    r += 1
                    c = 0
                    ws.ws.Cells(r, 0).Value = myPattern.GetID
                    For v = 0 To VolumeClasses.Classes.Count - 1
                        c += 1
                        myVol = VolumeClasses.Classes.Values(v)
                        ws.ws.Cells(0, c).Value = myVol.getID
                        ws.ws.Cells(1, c).Value = myVol.GetRepresentativeValue
                        myKey = myVol.getID.Trim.ToUpper & "_" & myPattern.GetID.Trim.ToUpper
                        If Combinations.ContainsKey(myKey) Then
                            ws.ws.Cells(r, c).Value = Combinations.Item(myKey).CountEvents / Events.Events.Count
                        Else
                            ws.ws.Cells(r, c).Value = 0
                        End If
                    Next
                End If
            Next

            '---------------------------------------------------------------------------------------------------------
            'then write the patterns that are in use
            '---------------------------------------------------------------------------------------------------------
            ws = Me.Setup.ExcelFile.GetAddSheet("Patterns")
            c = 0
            ws.ws.Cells(0, 0).Value = "Tijdstap"
            For p = 0 To Patterns.Patterns.Count - 1
                myPattern = Patterns.Patterns.Values(p)
                If myPattern.InUse Then
                    c += 1
                    ws.ws.Cells(0, c).Value = myPattern.GetID
                    For r = 1 To myPattern.Fractions.Count
                        ws.ws.Cells(r, 0).Value = r
                        ws.ws.Cells(r, c).Value = myPattern.Fractions(r - 1)
                    Next
                End If
            Next

            '---------------------------------------------------------------------------------------------------------
            'for each combination of volume & pattern, write the results for all locations and parameters
            '---------------------------------------------------------------------------------------------------------
            iCombi = 0
            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing results for all locations and parameters...", 0, 10, True)
            For Each myCombi As clsVolumePatternCombination In Combinations.Values
                iCombi += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iCombi, Combinations.Count, True)
                If myCombi.CountEvents > 0 Then
                    myPattern = myCombi.GetPattern
                    myVol = myCombi.getVolumeClass

                    ws = Me.Setup.ExcelFile.GetAddSheet("Combination " & iCombi)
                    ws.ws.Cells(0, 0).Value = "Pattern ID"
                    ws.ws.Cells(0, 1).Value = myCombi.GetPatternID
                    ws.ws.Cells(1, 0).Value = "Volume Class"
                    ws.ws.Cells(1, 1).Value = myCombi.GetVolumeClassID
                    ws.ws.Cells(2, 0).Value = "Member Events"

                    'write the member event numbers for this combination
                    For c = 1 To myCombi.CountEvents
                        ws.ws.Cells(2, c).Value = myCombi.GetEvent(c - 1).GetEventNum
                    Next

                    'we also need a list of all locations/parameters we have, apart from the one that was used for the classification itself
                    myCombi.AddSecondaryLocationParameterCombinationsFromDatabase()

                    'from here on we will start writing timesteps with values
                    For ts = 1 To Duration
                        r = 4 + ts

                        'a column for the timestep number
                        ws.ws.Cells(3, 0).Value = myCombi.GetID
                        ws.ws.Cells(4, 0).Value = "Timestep"
                        ws.ws.Cells(r, 0).Value = ts

                        'a column for the fraction of the total event volume for each timestep
                        ws.ws.Cells(4, 1).Value = "Fraction"
                        ws.ws.Cells(r, 1).Value = myPattern.Fractions(ts - 1)

                        'a column for the volumes of the representative location & parameter that this whole classification was based on in the first place
                        ws.ws.Cells(3, 2).Value = myCombi.GetPatternID
                        ws.ws.Cells(4, 2).Value = myCombi.GetVolumeClassID
                        ws.ws.Cells(r, 2).Value = myPattern.Fractions(ts - 1) * myVol.GetRepresentativeValue

                        'a column for each of the secondary location/parameter combinations for the given volume/pattern combination
                        c = 2
                        For Each LocParCombi As clsLocationParameterCombination In myCombi.SecondaryLocationParameterCombinations.Combinations.Values
                            c += 1
                            ws.ws.Cells(3, c).Value = LocParCombi.GetLocationID
                            ws.ws.Cells(4, c).Value = LocParCombi.GetParameter
                            ws.ws.Cells(r, c).Value = LocParCombi.GetValue(ts - 1)
                        Next
                    Next
                End If
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)



            ''---------------------------------------------------------------------------------------------------------
            ''then proceed to writing each combination of volume & pattern
            ''---------------------------------------------------------------------------------------------------------
            'For Each myCombi As clsVolumePatternCombination In Combinations.Values
            '    iCombi += 1
            '    If myCombi.CountEvents > 0 Then
            '        myPattern = myCombi.GetPattern
            '        myVol = myCombi.getVolumeClass

            '        ws = Me.Setup.ExcelFile.GetAddSheet("Classification " & iCombi)
            '        ws.ws.Cells(0, 0).Value = "Pattern ID"
            '        ws.ws.Cells(0, 1).Value = myCombi.GetPatternID
            '        ws.ws.Cells(1, 0).Value = "Volume Class"
            '        ws.ws.Cells(1, 1).Value = myCombi.GetVolumeClassID

            '        ws.ws.Cells(2, 0).Value = myCombi.GetID
            '        ws.ws.Cells(2, 1).Value = "Fraction"

            '        r = 2
            '        c = 1
            '        For ts = 1 To myPattern.Fractions.Count
            '            ws.ws.Cells(r + ts, 0).Value = ts
            '            ws.ws.Cells(r + ts, 1).Value = myPattern.Fractions(ts - 1)
            '        Next
            '        For Each myEvent As clsHydroEvent In myPattern.Events
            '            c += 1
            '            ws.ws.Cells(r, c).Value = myEvent.GetEventNum
            '            For ts = 1 To myEvent.Values.Count
            '                ws.ws.Cells(r + ts, c).Value = myEvent.Values(ts - 1) / myEvent.EventSum
            '            Next
            '        Next
            '    End If
            'Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


End Class
