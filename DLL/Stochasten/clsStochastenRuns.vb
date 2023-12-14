Imports System.Windows.Forms
Imports STOCHLIB.General
Imports System.IO
Imports System.Threading

Public Class clsStochastenRuns

    Public Runs As New Dictionary(Of String, clsStochastenRun)

    'index numbers for all columns in the run grid
    Public IDCol As Integer
    Public DurationCol As Integer
    Public SeasonCol As Integer, SeasonPCol As Integer
    Public VolumeCol As Integer, VolumePCol As Integer
    Public PatternCol As Integer, PatternPCol As Integer
    Public GroundwaterCol As Integer, GroundwaterPCol As Integer
    Public WaterLevelCol As Integer, WaterLevelPCol As Integer
    Public WindCol As Integer, WindPCol As Integer
    Public Extra1Col As Integer, Extra1PCol As Integer
    Public Extra2Col As Integer, Extra2PCol As Integer
    Public Extra3Col As Integer, Extra3PCol As Integer
    Public Extra4Col As Integer, Extra4PCol As Integer
    Public DoneCol As Integer   'checkbox column that indicates whether a run is complete
    Public PCol As Integer

    Private StochastenAnalyse As clsStochastenAnalyse
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByRef myAnalyse As clsStochastenAnalyse)
        Setup = mySetup
        StochastenAnalyse = myAnalyse
    End Sub

    Public Sub Clear()
        Runs = New Dictionary(Of String, clsStochastenRun)
    End Sub

    Public Function GetByID(myID As String) As clsStochastenRun
        If Runs.ContainsKey(myID.Trim.ToUpper) Then
            Return Runs.Item(myID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function


    Public Function StochastInUse(ByRef myGrid As DataGridView) As Boolean
        For Each myRow As DataGridViewRow In myGrid.Rows
            If myRow.Cells("USE").Value = True Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Sub PopulateMileageCounters()
        'every season will get its own mileage counter since the number of members can difference per season
        'add the stochasts as digits to the mileage counter
        For Each mySeason As clsStochasticSeasonClass In Setup.StochastenAnalyse.Seasons.Values
            mySeason.MileageCounter = New clsMileageCounter(Setup)
            If mySeason.VolumeUse Then mySeason.MileageCounter.AddDigit(0, mySeason.Volumes.Count - 1, enmStochastType.Volume)
            If mySeason.PatternUse Then mySeason.MileageCounter.AddDigit(0, mySeason.Patterns.Count - 1, enmStochastType.Pattern)
            If mySeason.GroundwaterUse Then mySeason.MileageCounter.AddDigit(0, mySeason.Groundwater.Count - 1, enmStochastType.Groundwater)
            If mySeason.WaterLevelsUse Then mySeason.MileageCounter.AddDigit(0, mySeason.WaterLevels.Count - 1, enmStochastType.WaterLevel)
            If mySeason.WindUse Then mySeason.MileageCounter.AddDigit(0, mySeason.Wind.Count - 1, enmStochastType.Wind)
            If mySeason.Extra1Use Then mySeason.MileageCounter.AddDigit(0, mySeason.Extra1.Count - 1, enmStochastType.Extra1)
            If mySeason.Extra2Use Then mySeason.MileageCounter.AddDigit(0, mySeason.Extra2.Count - 1, enmStochastType.Extra2)
            If mySeason.Extra3Use Then mySeason.MileageCounter.AddDigit(0, mySeason.Extra3.Count - 1, enmStochastType.Extra3)
            If mySeason.Extra4Use Then mySeason.MileageCounter.AddDigit(0, mySeason.Extra4.Count - 1, enmStochastType.Extra4)
        Next


    End Sub

    Public Function GetSetColumn(ByRef dt As DataTable, ByVal ID As String, ByVal myType As System.Type) As Integer
        Dim c As Integer
        For c = 0 To dt.Columns.Count - 1
            If dt.Columns(c).ColumnName = ID Then Return c
        Next
        dt.Columns.Add(ID, myType)
        Return dt.Columns.Count - 1
    End Function


    Public Function PopulateFromDB(con As SQLite.SQLiteConnection) As Boolean
        Try

            '-------------------------------------------------------------------------------------------------
            'this routine populates the current class containing stochastic runs from the database
            'in the mean time it will also populate the used stochastic classes
            'note: the classes will only be populated with the ones that are actually IN USE
            'since this function is based on the list of runs and runs are only built-up from IN USE stochasts
            '-------------------------------------------------------------------------------------------------
            Runs.Clear()
            Dim myQuery As String = "SELECT * FROM RUNS WHERE KLIMAATSCENARIO='" & StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "' AND DUUR=" & StochastenAnalyse.Duration & ";"
            Dim dt As New DataTable, i As Long, myRun As clsStochastenRun
            Setup.GeneralFunctions.SQLiteQuery(con, myQuery, dt)

            For i = 0 To dt.Rows.Count - 1
                myRun = New clsStochastenRun(Setup, Me.StochastenAnalyse)
                myRun.ID = dt.Rows(i)("RUNID")
                myRun.RelativeDir = dt.Rows(i)("RELATIVEDIR")
                myRun.InputFilesDir = Setup.StochastenAnalyse.InputFilesDir & "\" & myRun.RelativeDir
                myRun.OutputFilesDir = Setup.StochastenAnalyse.OutputFilesDir & "\" & myRun.RelativeDir

                'start by retrieving the season class. Each instance of this class contains all other classes
                myRun.SeasonClass = StochastenAnalyse.GetAddSeasonClass(dt.Rows(i)("SEIZOEN"), dt.Rows(i)("SEIZOEN_P"), True)

                With myRun.SeasonClass
                    myRun.VolumeClass = .GetAddVolumeClass(dt.Rows(i)("VOLUME"), dt.Rows(i)("VOLUME_P"))
                    myRun.PatternClass = .GetAddPatternClass(dt.Rows(i)("PATROON"), dt.Rows(i)("PATROON_P"))
                    myRun.GWClass = .GetAddGWClass(dt.Rows(i)("GW"), dt.Rows(i)("GW_P"))
                    myRun.WLClass = .GetAddWLClass(dt.Rows(i)("BOUNDARY"), dt.Rows(i)("BOUNDARY_P"))
                    myRun.WindClass = .GetAddWindClass(dt.Rows(i)("WIND"), dt.Rows(i)("WIND_P"))
                    myRun.Extra1Class = .GetAddExtraClass(1, dt.Rows(i)("EXTRA1"), dt.Rows(i)("EXTRA1_P"))
                    myRun.Extra2Class = .GetAddExtraClass(2, dt.Rows(i)("EXTRA2"), dt.Rows(i)("EXTRA2_P"))
                    myRun.Extra3Class = .GetAddExtraClass(3, dt.Rows(i)("EXTRA3"), dt.Rows(i)("EXTRA3_P"))
                    myRun.Extra4Class = .GetAddExtraClass(4, dt.Rows(i)("EXTRA4"), dt.Rows(i)("EXTRA4_P"))
                End With

                myRun.CalcIDExcepVolume()
                myRun.P = dt.Rows(i)("P")
                myRun.RelativeDir = dt.Rows(i)("RELATIVEDIR")
                Runs.Add(myRun.ID.Trim.ToUpper, myRun)

            Next

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error populating runs from database.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function


    Public Sub PopulateFromDataGridView(ByRef con As SQLite.SQLiteConnection, ByRef grRuns As DataGridView)

        '-----------------------------------------------------------------------------------------------------------------
        ' this routine populates the dictionary of stochastic runs based on the data currently in the grRuns Datagridview
        '-----------------------------------------------------------------------------------------------------------------

        Try
            Dim myRun As clsStochastenRun
            grRuns.Columns.Clear()
            Dim dt As New DataTable
            Dim r As Integer = -1
            Dim i As Integer
            Dim Digit As Integer = -1   'the currently active digit inside the mileage counter
            Runs = New Dictionary(Of String, clsStochastenRun)
            Dim myQuery As String

            'delete all existing enteries for this klimate & duration from the runs table
            myQuery = "DELETE FROM RUNS WHERE KLIMAATSCENARIO='" & StochastenAnalyse.KlimaatScenario.ToString & "' AND DUUR=" & StochastenAnalyse.Duration & ";"
            Setup.GeneralFunctions.SQLiteNoQuery(con, myQuery, False)

            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
            Using cmd As New SQLite.SQLiteCommand
                cmd.Connection = Me.Setup.SqliteCon
                Using transaction = Me.Setup.SqliteCon.BeginTransaction

                    'first set the column index numbers for the runs grid
                    For Each mySeasonClass As clsStochasticSeasonClass In Setup.StochastenAnalyse.Seasons.Values

                        'now walk through all possible combinations and add them as a run to the list
                        mySeasonClass.MileageCounter.Initialize()

                        'set the column index number for all columns to be written to the Runs gridview
                        IDCol = GetSetColumn(dt, "ID", Type.GetType("System.String"))
                        DurationCol = GetSetColumn(dt, "duur", Type.GetType("System.Int32"))
                        SeasonCol = GetSetColumn(dt, "Seizoen", Type.GetType("System.String"))
                        SeasonPCol = GetSetColumn(dt, "P(seizoen)", Type.GetType("System.Double"))
                        If mySeasonClass.GroundwaterUse Then
                            GroundwaterCol = GetSetColumn(dt, "Grondwater", Type.GetType("System.String"))
                            GroundwaterPCol = GetSetColumn(dt, "P(grondwater)", Type.GetType("System.Double"))
                            mySeasonClass.MileageCounter.AddDigit(0, mySeasonClass.Groundwater.Count - 1, enmStochastType.Groundwater)
                        End If
                        If mySeasonClass.PatternUse Then
                            PatternCol = GetSetColumn(dt, "Patroon", Type.GetType("System.String"))
                            PatternPCol = GetSetColumn(dt, "P(patroon)", Type.GetType("System.Double"))
                            mySeasonClass.MileageCounter.AddDigit(0, mySeasonClass.Patterns.Count - 1, enmStochastType.Pattern)
                        End If
                        If mySeasonClass.VolumeUse Then
                            VolumeCol = GetSetColumn(dt, "Volume", Type.GetType("System.Double"))
                            VolumePCol = GetSetColumn(dt, "P(volume)", Type.GetType("System.Double"))
                            mySeasonClass.MileageCounter.AddDigit(0, mySeasonClass.Volumes.Count - 1, enmStochastType.Volume)
                        End If
                        If mySeasonClass.WaterLevelsUse Then
                            WaterLevelCol = GetSetColumn(dt, "Waterhoogte", Type.GetType("System.String"))
                            WaterLevelPCol = GetSetColumn(dt, "P(waterhoogte)", Type.GetType("System.Double"))
                            mySeasonClass.MileageCounter.AddDigit(0, mySeasonClass.WaterLevels.Count - 1, enmStochastType.WaterLevel)
                        End If
                        If mySeasonClass.WindUse Then
                            WindCol = GetSetColumn(dt, "Wind", Type.GetType("System.String"))
                            WindPCol = GetSetColumn(dt, "P(wind)", Type.GetType("System.Double"))
                            mySeasonClass.MileageCounter.AddDigit(0, mySeasonClass.Wind.Count - 1, enmStochastType.Wind)
                        End If
                        If mySeasonClass.Extra1Use Then
                            Extra1Col = GetSetColumn(dt, "Extra1", Type.GetType("System.String"))
                            Extra1PCol = GetSetColumn(dt, "P(extra1)", Type.GetType("System.Double"))
                            mySeasonClass.MileageCounter.AddDigit(0, mySeasonClass.Extra1.Count - 1, enmStochastType.Extra1)
                        End If
                        If mySeasonClass.Extra2Use Then
                            Extra2Col = GetSetColumn(dt, "Extra2", Type.GetType("System.String"))
                            Extra2PCol = GetSetColumn(dt, "P(extra2)", Type.GetType("System.Double"))
                            mySeasonClass.MileageCounter.AddDigit(0, mySeasonClass.Extra2.Count - 1, enmStochastType.Extra2)
                        End If
                        If mySeasonClass.Extra3Use Then
                            Extra3Col = GetSetColumn(dt, "Extra3", Type.GetType("System.String"))
                            Extra3PCol = GetSetColumn(dt, "P(extra3)", Type.GetType("System.Double"))
                            mySeasonClass.MileageCounter.AddDigit(0, mySeasonClass.Extra3.Count - 1, enmStochastType.Extra3)
                        End If
                        If mySeasonClass.Extra4Use Then
                            Extra4Col = GetSetColumn(dt, "Extra4", Type.GetType("System.String"))
                            Extra4PCol = GetSetColumn(dt, "P(Extra4)", Type.GetType("System.Double"))
                            mySeasonClass.MileageCounter.AddDigit(0, mySeasonClass.Extra4.Count - 1, enmStochastType.Extra4)
                        End If
                        DoneCol = GetSetColumn(dt, "DONE", Type.GetType("System.Boolean"))
                        PCol = GetSetColumn(dt, "P", Type.GetType("System.Double"))

                        While mySeasonClass.MileageCounter.MileageOneUp()
                            myRun = New clsStochastenRun(Setup, StochastenAnalyse)

                            'create a query to write this run to the database
                            cmd.CommandText = "INSERT INTO RUNS (KLIMAATSCENARIO, DUUR, SEIZOEN, SEIZOEN_P, GW, GW_P, PATROON, PATROON_P, VOLUME, VOLUME_P, BOUNDARY, BOUNDARY_P, WIND, WIND_P, EXTRA1, EXTRA1_P, EXTRA2, EXTRA2_P, EXTRA3, EXTRA3_P, EXTRA4, EXTRA4_P, P, RELATIVEDIR, RUNID) VALUES ("

                            myRun.Klimaatscenario = StochastenAnalyse.KlimaatScenario
                            myRun.duur = StochastenAnalyse.Duration
                            myRun.SeasonClass = mySeasonClass
                            myRun.ID = StochastenAnalyse.KlimaatScenario.ToString & "_" & mySeasonClass.Name
                            myRun.IDexceptVolume = mySeasonClass.Name
                            myRun.P = mySeasonClass.P    'start by assigning the season's P
                            myRun.RelativeDir = myRun.SeasonClass.Name & "_" & StochastenAnalyse.Duration & "h"
                            cmd.CommandText &= "'" & StochastenAnalyse.KlimaatScenario.ToString & "'," & StochastenAnalyse.Duration & ",'" & myRun.SeasonClass.Name & "'," & myRun.SeasonClass.P

                            Digit = -1
                            r += 1
                            dt.Rows.Add()

                            'wait with assigning the ID to the run until we processed all stochasts
                            dt.Rows(r)(DurationCol) = StochastenAnalyse.Duration
                            dt.Rows(r)(SeasonCol) = mySeasonClass.Name
                            dt.Rows(r)(SeasonPCol) = mySeasonClass.P

                            'populate the run with all necessary information regarding paths, probabilities and names
                            If mySeasonClass.GroundwaterUse Then
                                Digit += 1
                                myRun.GWClass = mySeasonClass.Groundwater.Values(mySeasonClass.MileageCounter.GetDigitValue(Digit))
                                myRun.ID &= "_" & myRun.GWClass.ID
                                myRun.IDexceptVolume &= "_" & myRun.GWClass.ID
                                myRun.RelativeDir &= "\" & "grondwater_" & myRun.GWClass.ID
                                dt.Rows(r)(GroundwaterCol) = myRun.GWClass.ID
                                dt.Rows(r)(GroundwaterPCol) = myRun.GWClass.p
                                myRun.P *= myRun.GWClass.p
                                cmd.CommandText &= ",'" & myRun.GWClass.ID & "'," & myRun.GWClass.p
                            Else
                                cmd.CommandText &= ",'',1"
                            End If

                            If mySeasonClass.PatternUse Then
                                Digit += 1
                                myRun.PatternClass = mySeasonClass.Patterns.Values(mySeasonClass.MileageCounter.GetDigitValue(Digit))
                                myRun.ID &= "_" & myRun.PatternClass.Patroon.ToString
                                myRun.IDexceptVolume &= "_" & myRun.PatternClass.Patroon.ToString
                                myRun.RelativeDir &= "\" & myRun.PatternClass.Patroon.ToString.ToUpper
                                dt.Rows(r)(PatternCol) = myRun.PatternClass.Patroon.ToString
                                dt.Rows(r)(PatternPCol) = myRun.PatternClass.p
                                myRun.P *= myRun.PatternClass.p
                                cmd.CommandText &= ",'" & myRun.PatternClass.Patroon.ToString & "'," & myRun.PatternClass.p
                            Else
                                cmd.CommandText &= ",'',1"
                            End If

                            If mySeasonClass.VolumeUse Then
                                Digit += 1
                                myRun.VolumeClass = mySeasonClass.Volumes.Values(mySeasonClass.MileageCounter.GetDigitValue(Digit))
                                myRun.ID &= "_" & myRun.VolumeClass.Volume.ToString & "mm"
                                myRun.RelativeDir &= "\" & myRun.VolumeClass.Volume.ToString & "mm"
                                dt.Rows(r)(VolumeCol) = myRun.VolumeClass.Volume
                                dt.Rows(r)(VolumePCol) = myRun.VolumeClass.P
                                myRun.P *= myRun.VolumeClass.P
                                cmd.CommandText &= "," & myRun.VolumeClass.Volume & "," & myRun.VolumeClass.P
                            Else
                                cmd.CommandText &= ",0,1"
                            End If

                            If mySeasonClass.WaterLevelsUse Then
                                Digit += 1
                                myRun.WLClass = mySeasonClass.WaterLevels.Values(mySeasonClass.MileageCounter.GetDigitValue(Digit))
                                myRun.ID &= "_" & myRun.WLClass.ID
                                myRun.IDexceptVolume &= "_" & myRun.WLClass.ID
                                myRun.RelativeDir &= "\" & myRun.WLClass.ID
                                dt.Rows(r)(WaterLevelCol) = myRun.WLClass.ID
                                dt.Rows(r)(WaterLevelPCol) = myRun.WLClass.p
                                myRun.P *= myRun.WLClass.p
                                cmd.CommandText &= ",'" & myRun.WLClass.ID & "'," & myRun.WLClass.p
                            Else
                                cmd.CommandText &= ",'',1"
                            End If

                            If mySeasonClass.WindUse Then
                                Digit += 1
                                myRun.WindClass = mySeasonClass.Wind.Values(mySeasonClass.MileageCounter.GetDigitValue(Digit))
                                myRun.ID &= "_" & myRun.WindClass.ID
                                myRun.IDexceptVolume &= "_" & myRun.WindClass.ID
                                myRun.RelativeDir &= "\" & myRun.WindClass.ID
                                dt.Rows(r)(WindCol) = myRun.WindClass.ID
                                dt.Rows(r)(WindPCol) = myRun.WindClass.P
                                myRun.P *= myRun.WindClass.P
                                cmd.CommandText &= ",'" & myRun.WindClass.ID & "'," & myRun.WindClass.P
                            Else
                                cmd.CommandText &= ",'',1"
                            End If

                            If mySeasonClass.Extra1Use Then
                                Digit += 1
                                myRun.Extra1Class = mySeasonClass.Extra1.Values(mySeasonClass.MileageCounter.GetDigitValue(Digit))
                                myRun.ID &= "_" & myRun.Extra1Class.ID
                                myRun.IDexceptVolume &= "_" & myRun.Extra1Class.ID
                                myRun.RelativeDir &= "\" & myRun.Extra1Class.ID
                                dt.Rows(r)(Extra1Col) = myRun.Extra1Class.ID
                                dt.Rows(r)(Extra1PCol) = myRun.Extra1Class.p
                                myRun.P *= myRun.Extra1Class.p
                                cmd.CommandText &= ",'" & myRun.Extra1Class.ID & "'," & myRun.Extra1Class.p
                            Else
                                cmd.CommandText &= ",'',1"
                            End If

                            If mySeasonClass.Extra2Use Then
                                Digit += 1
                                myRun.Extra2Class = mySeasonClass.Extra2.Values(mySeasonClass.MileageCounter.GetDigitValue(Digit))
                                myRun.ID &= "_" & myRun.Extra2Class.ID
                                myRun.IDexceptVolume &= "_" & myRun.Extra2Class.ID
                                myRun.RelativeDir &= "\" & myRun.Extra2Class.ID
                                dt.Rows(r)(Extra2Col) = myRun.Extra2Class.ID
                                dt.Rows(r)(Extra2PCol) = myRun.Extra2Class.p
                                myRun.P *= myRun.Extra2Class.p
                                cmd.CommandText &= ",'" & myRun.Extra2Class.ID & "'," & myRun.Extra2Class.p
                            Else
                                cmd.CommandText &= ",'',1"
                            End If

                            If mySeasonClass.Extra3Use Then
                                Digit += 1
                                myRun.Extra3Class = mySeasonClass.Extra3.Values(mySeasonClass.MileageCounter.GetDigitValue(Digit))
                                myRun.ID &= "_" & myRun.Extra3Class.ID
                                myRun.IDexceptVolume &= "_" & myRun.Extra3Class.ID
                                myRun.RelativeDir &= "\" & myRun.Extra3Class.ID
                                dt.Rows(r)(Extra3Col) = myRun.Extra3Class.ID
                                dt.Rows(r)(Extra3PCol) = myRun.Extra3Class.p
                                myRun.P *= myRun.Extra3Class.p
                                cmd.CommandText &= ",'" & myRun.Extra3Class.ID & "'," & myRun.Extra3Class.p
                            Else
                                cmd.CommandText &= ",'',1"
                            End If

                            If mySeasonClass.Extra4Use Then
                                Digit += 1
                                myRun.Extra4Class = mySeasonClass.Extra4.Values(mySeasonClass.MileageCounter.GetDigitValue(Digit))
                                myRun.ID &= "_" & myRun.Extra4Class.ID
                                myRun.IDexceptVolume &= "_" & myRun.Extra4Class.ID
                                myRun.RelativeDir &= "\" & myRun.Extra4Class.ID
                                dt.Rows(r)(Extra4Col) = myRun.Extra4Class.ID
                                dt.Rows(r)(Extra4PCol) = myRun.Extra4Class.p
                                myRun.P *= myRun.Extra4Class.p
                                cmd.CommandText &= ",'" & myRun.Extra4Class.ID & "'," & myRun.Extra4Class.p
                            Else
                                cmd.CommandText &= ",'',1"
                            End If

                            myRun.InputFilesDir = StochastenAnalyse.InputFilesDir & "\" & myRun.RelativeDir
                            myRun.OutputFilesDir = StochastenAnalyse.OutputFilesDir & "\" & myRun.RelativeDir

                            If Not Directory.Exists(myRun.InputFilesDir) Then Directory.CreateDirectory(myRun.InputFilesDir)
                            If Not Directory.Exists(myRun.OutputFilesDir) Then Directory.CreateDirectory(myRun.OutputFilesDir)

                            cmd.CommandText &= "," & myRun.P & ",'" & myRun.RelativeDir & "','" & myRun.ID & "');"

                            dt.Rows(r)(IDCol) = myRun.ID
                            dt.Rows(r)(DoneCol) = False
                            dt.Rows(r)(PCol) = myRun.P

                            'add this run to the dictionary of runs                
                            Runs.Add(myRun.ID.Trim.ToUpper, myRun)

                            'also add this run to the database
                            cmd.ExecuteNonQuery()

                        End While
                    Next

                    transaction.Commit() 'this is where the bulk insert is finally executed.
                End Using
            End Using

            'close the database
            Me.Setup.SqliteCon.Close()

            'populate the datagridview with the content of th edatatable
            grRuns.DataSource = dt

            'fit the column widths to the data contained
            grRuns.Columns(0).AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            'For i = 0 To grRuns.Columns.Count - 1
            'Next

            'finally assign numbers to the datagrid's row headers
            For i = 0 To grRuns.Rows.Count - 1
                grRuns.Rows(i).HeaderCell.Value = String.Format("{0}", i + 1)
            Next

        Catch ex As Exception
            Me.Setup.Log.Errors.Add("Error populating runs from datagridview")
            Me.Setup.Log.Errors.Add(ex.Message)
        End Try

    End Sub

    Public Function ClearSelected(ByVal grRuns, ByVal btnPostprocessing) As Boolean
        '------------------------------------------------------------------------------------------------------------------------------
        'this routine clears the results of earlier simulations
        '------------------------------------------------------------------------------------------------------------------------------
        Dim ID As String, myRun As clsStochastenRun

        Try
            For Each myrow As DataGridViewRow In grRuns.SelectedRows
                myrow.Cells("DONE").Value = False

                'clear the directory
                ID = myrow.Cells("ID").Value
                myRun = Runs.Item(ID.Trim.ToUpper)
                System.IO.Directory.Delete(myRun.OutputFilesDir, True)

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function ClearSelected.")
            Return False
        End Try

    End Function

    Public Function GetSelectedFlowForcings(ByRef grRuns As DataGridView) As Dictionary(Of String, clsFlowForcing)
        '------------------------------------------------------------------------------------------------------------------------------
        'this routine returns a dictionary containing all unique flow forcings for the selected runs
        '------------------------------------------------------------------------------------------------------------------------------
        Dim ID As String, myRun As clsStochastenRun
        Dim i As Long, n As Long
        Dim myForcing As clsFlowForcing
        Dim myForcings As New Dictionary(Of String, clsFlowForcing)

        Try
            'first figure out how many runs to do
            n = 0
            For Each myrow As DataGridViewRow In grRuns.SelectedRows
                If Not myrow.Cells("DONE").Value = True Then n += 1
            Next

            'loop through all runs and establish which meteo forcing it has
            i = 0
            For Each myRow As DataGridViewRow In grRuns.SelectedRows
                If Not myRow.Cells("DONE").Value = True Then
                    i += 1
                    ID = myRow.Cells("ID").Value
                    myRun = Runs.Item(ID.Trim.ToUpper)
                    myForcing = New clsFlowForcing(myRun.WLClass.ID, myRun.SeasonClass.Name)
                    If Not myForcings.ContainsKey(myForcing.GetID) Then myForcings.Add(myForcing.GetID, myForcing)
                End If
            Next
            Return myForcings
        Catch ex As Exception
            Me.Setup.Log.AddError("Error retrieving meteo forcings for selected runs.")
            Return Nothing
        End Try
    End Function

    Public Function GetSelectedMeteoForcings(ByRef grRuns As DataGridView) As Dictionary(Of String, clsMeteoForcing)

        '------------------------------------------------------------------------------------------------------------------------------
        'this routine returns a dictionary containing all unique meteo forcings for the selected runs
        '------------------------------------------------------------------------------------------------------------------------------
        Dim ID As String, myRun As clsStochastenRun
        Dim i As Long, n As Long
        Dim myForcing As clsMeteoForcing
        Dim myForcings As New Dictionary(Of String, clsMeteoForcing)

        Try
            'first figure out how many runs to do
            n = 0
            For Each myrow As DataGridViewRow In grRuns.SelectedRows
                If Not myrow.Cells("DONE").Value = True Then n += 1
            Next

            'loop through all runs and establish which meteo forcing it has
            i = 0
            For Each myRow As DataGridViewRow In grRuns.SelectedRows
                If Not myRow.Cells("DONE").Value = True Then
                    i += 1
                    ID = myRow.Cells("ID").Value
                    myRun = Runs.Item(ID.Trim.ToUpper)
                    myForcing = myRun.getMeteoForcing()                     'retrieve the meteo forcings for this run
                    If Not myForcings.ContainsKey(myForcing.GetID) Then myForcings.Add(myForcing.GetID, myForcing)
                End If
            Next
            Return myForcings
        Catch ex As Exception
            Me.Setup.Log.AddError("Error retrieving meteo forcings for selected runs.")
            Return Nothing
        End Try
    End Function

    Public Function GetSelected(ByRef grRuns As DataGridView) As Dictionary(Of String, clsStochastenRun)
        'returns a dictionary containing all selected stochastic runs that have no results yet
        Dim Selection As New Dictionary(Of String, clsStochastenRun)
        Dim ID As String, myRun As clsStochastenRun
        Try
            For Each myRow As DataGridViewRow In grRuns.SelectedRows
                If Not myRow.Cells("DONE").Value = True Then
                    ID = myRow.Cells("ID").Value
                    myRun = Runs.Item(ID.Trim.ToUpper)
                    Selection.Add(ID.Trim.ToUpper, myRun)
                End If
            Next
            Return Selection
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function GetSelectedRuns of class clsStochastenRuns: " & ex.Message)
            Return Selection
        End Try
    End Function

    Public Function CopyResultsFromSelected(ByRef grRuns As DataGridView, ByRef btnCharts As Button) As Boolean

        '------------------------------------------------------------------------------------------------------------------------------
        'this routine copies the requested results from the selected runs to the output directory
        '------------------------------------------------------------------------------------------------------------------------------
        Dim ID As String, myRun As clsStochastenRun
        Dim i As Long, n As Long
        Dim Done As Boolean

        Try

            'first figure out how many runs to copy results from
            n = 0
            For Each myrow As DataGridViewRow In grRuns.SelectedRows
                If Not myrow.Cells("DONE").Value = True Then n += 1
            Next

            'copy results
            i = 0
            For Each myRow As DataGridViewRow In grRuns.SelectedRows
                If Not myRow.Cells("DONE").Value = True Then
                    i += 1
                    ID = myRow.Cells("ID").Value
                    myRun = Runs.Item(ID.Trim.ToUpper)

                    'copy the results files
                    If Not myRun.CopyResultsFiles(i, n) Then Me.Setup.Log.AddError("Error copying model results for stochast combination " & ID)

                    'set "DONE"to true if all output files for this run are present
                    Done = True
                    For Each myModel In StochastenAnalyse.Models.Values
                        For Each myFile In myModel.ResultsFiles.Files.Values
                            If Not File.Exists(myRun.OutputFilesDir & myFile.FileName) Then
                                Done = False
                            End If
                        Next
                    Next
                    myRow.Cells("DONE").Value = Done
                End If
            Next

            'after all runs are complete, refresh the entire grid
            Call StochastenAnalyse.RefreshRunsGrid(grRuns, btnCharts)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function BuildSelected(ByRef grRuns As DataGridView, ByRef btnCharts As Button) As Boolean

        '------------------------------------------------------------------------------------------------------------------------------
        'this routine actually creates a new instance of the model schematisation(s) to be run and writes them to the temporary workdir
        'it then copies the necessary files into the workdir and kicks off the model(s)
        '------------------------------------------------------------------------------------------------------------------------------
        Dim ID As String, myRun As clsStochastenRun
        Dim i As Long, n As Long
        Dim Done As Boolean

        Try

            'first figure out how many runs to do
            n = 0
            For Each myrow As DataGridViewRow In grRuns.SelectedRows
                If Not myrow.Cells("DONE").Value = True Then n += 1
            Next

            'execute
            i = 0
            For Each myRow As DataGridViewRow In grRuns.SelectedRows
                If Not myRow.Cells("DONE").Value = True Then
                    i += 1
                    ID = myRow.Cells("ID").Value
                    myRun = Runs.Item(ID.Trim.ToUpper)

                    'execute the run
                    If Not myRun.Build(i, n) Then Me.Setup.Log.AddError("Error running model for stochast combination " & ID)

                    'set "DONE"to true if all output files for this run are present
                    Done = True
                    For Each myModel In StochastenAnalyse.Models.Values
                        For Each myFile In myModel.ResultsFiles.Files.Values
                            If Not File.Exists(myRun.OutputFilesDir & myFile.FileName) Then
                                Done = False
                            End If
                        Next
                    Next
                    myRow.Cells("DONE").Value = Done
                End If
            Next

            'after all runs are complete, refresh the entire grid
            Call StochastenAnalyse.RefreshRunsGrid(grRuns, btnCharts)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Sub RunsToGrid(ByRef grRuns As DataGridView)

        'first clear the grid
        Dim i As Long = 0
        Me.Setup.GeneralFunctions.UpdateProgressBar("Populating grid with runs...", 0, 10)
        grRuns.Rows.Clear()

        'populate the grid
        For Each myRun As clsStochastenRun In Runs.Values
            i += 1
            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, Runs.Count)
            myRun.AddToGrid(grRuns)
        Next

        'write a row number in the header
        Setup.GeneralFunctions.NumberGridViewRows(grRuns, True) 'writes the row number in the row headers

    End Sub

    Public Function calcCheckSum() As Double
        Dim cumP As Double
        For Each myRun As clsStochastenRun In Runs.Values
            cumP += myRun.calcP
        Next
        Return cumP
    End Function

End Class
