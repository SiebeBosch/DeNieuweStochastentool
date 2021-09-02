Imports STOCHLIB.General

Public Class clsExtremeValuesStatParameterLocations
    Private Setup As clsSetup
    Private Statistics As clsExtremeValuesStatistics
    Public ExtremeValuesStatLocations As New Dictionary(Of String, clsExtremeValuesStatParameterLocation)

    Public Sub New(ByRef mySetup As clsSetup, ByRef myStatistics As clsExtremeValuesStatistics)
        Setup = mySetup
        Statistics = myStatistics
    End Sub

    Public Function GetLocation(ByVal ID As String) As clsExtremeValuesStatParameterLocation
        If ExtremeValuesStatLocations.ContainsKey(ID.Trim.ToUpper) Then
            Return ExtremeValuesStatLocations.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetAddLocation(ByVal ID As String) As clsExtremeValuesStatParameterLocation
        If ExtremeValuesStatLocations.ContainsKey(ID.Trim.ToUpper) Then
            Return ExtremeValuesStatLocations.Item(ID.Trim.ToUpper)
        Else
            Dim myLoc As New clsExtremeValuesStatParameterLocation(Me.Setup, Statistics, ID)
            ExtremeValuesStatLocations.Add(ID.Trim.ToUpper, myLoc)
            Return myLoc
        End If
    End Function

    Public Function PopulateFromDatabase(TableName As String, IDCOLUMN As String, ByRef LocationsList As Dictionary(Of String, String)) As Boolean
        'this function populates the extremevaluesstatlocations class instance with locations from the model schematization
        'it uses the locationslist provided to do so. 
        'note: if the locatonslist is empty, the routine will assume ALL locations must be read.
        Dim query As String = ""
        Dim et As DataTable, i As Long, j As Long
        Dim myLoc As clsExtremeValuesStatParameterLocation
        Try
            ExtremeValuesStatLocations = New Dictionary(Of String, clsExtremeValuesStatParameterLocation)
            'Setup.SqLiteCon.Open()
            Me.Setup.Log.AddMessage("Database connection opened successfully.")

            'walk through all locations and read the data values
            If LocationsList.Count = 0 Then
                Throw New Exception("No valid locations found that match the selection criteria.")
            Else
                Me.Setup.Log.AddMessage(LocationsList.Count & " locations found.")
                For i = 0 To LocationsList.Count - 1
                    Me.Setup.Log.AddMessage("Processing location with index number " & i.ToString)
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i + 1, LocationsList.Count, True)
                    Me.Setup.Log.AddMessage("Retrieving location from dictionary: " & i.ToString)
                    myLoc = GetAddLocation(LocationsList.Values(i))
                    If Not myLoc Is Nothing Then Me.Setup.Log.AddMessage("Location object successfully retrieved.")
                    myLoc.ClearValues(True, True)
                    Me.Setup.Log.AddMessage("Existing values successfully removed from location object.")

                    If Me.Setup.ExtremeValuesStatistics.Duration = 0 Then
                        'duration = 0 is a flag for 'all durations'
                        Select Case Statistics.ResultsType
                            Case GeneralFunctions.enmModelResultsAspect.Maximum
                                query = "SELECT STARTDATE, EVENTMAX, EVENTNUM FROM EVENTS WHERE LOCATIONID='" & LocationsList.Values(i).Trim & "' AND PARAMETER='" & Me.Setup.ExtremeValuesStatistics.Parameter & "' ORDER BY EVENTNUM;"
                            Case GeneralFunctions.enmModelResultsAspect.Minimum
                                query = "SELECT STARTDATE, EVENTMIN, EVENTNUM FROM EVENTS WHERE LOCATIONID='" & LocationsList.Values(i).Trim & "' AND PARAMETER='" & Me.Setup.ExtremeValuesStatistics.Parameter & "' ORDER BY EVENTNUM;"
                            Case GeneralFunctions.enmModelResultsAspect.Sum
                                query = "SELECT STARTDATE, EVENTSUM, EVENTNUM FROM EVENTS WHERE LOCATIONID='" & LocationsList.Values(i).Trim & "' AND PARAMETER='" & Me.Setup.ExtremeValuesStatistics.Parameter & "' ORDER BY EVENTNUM;"
                        End Select
                    Else
                        Select Case Statistics.ResultsType
                            Case GeneralFunctions.enmModelResultsAspect.Maximum
                                query = "SELECT STARTDATE, EVENTMAX, EVENTNUM FROM EVENTS WHERE LOCATIONID='" & LocationsList.Values(i).Trim & "' AND DURATION=" & Me.Setup.ExtremeValuesStatistics.Duration & " AND PARAMETER='" & Me.Setup.ExtremeValuesStatistics.Parameter & "' ORDER BY EVENTNUM;"
                            Case GeneralFunctions.enmModelResultsAspect.Minimum
                                query = "SELECT STARTDATE, EVENTMIN, EVENTNUM FROM EVENTS WHERE LOCATIONID='" & LocationsList.Values(i).Trim & "' AND DURATION=" & Me.Setup.ExtremeValuesStatistics.Duration & " AND PARAMETER='" & Me.Setup.ExtremeValuesStatistics.Parameter & "' ORDER BY EVENTNUM;"
                            Case GeneralFunctions.enmModelResultsAspect.Sum
                                query = "SELECT STARTDATE, EVENTSUM, EVENTNUM FROM EVENTS WHERE LOCATIONID='" & LocationsList.Values(i).Trim & "' AND DURATION=" & Me.Setup.ExtremeValuesStatistics.Duration & " AND PARAMETER='" & Me.Setup.ExtremeValuesStatistics.Parameter & "' ORDER BY EVENTNUM;"
                        End Select
                    End If

                    et = New DataTable
                    Me.Setup.Log.AddMessage("Query created.")

                    Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, et, False)
                    Me.Setup.Log.AddMessage("Query successfully executed.")

                    'loop through all events and store the desired result
                    For j = 0 To et.Rows.Count - 1
                        myLoc.AddValue(et.Rows(j)(2), et.Rows(j)(0), et.Rows(j)(1))
                    Next

                Next
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        Finally
            Me.Setup.SqLiteCon.Close()
        End Try
    End Function

End Class
