Imports STOCHLIB.General
Imports System.IO

Public Class clsExtremeValuesStatistics
    Private Setup As clsSetup

    Friend nYearsObserved As Integer        'the total number of years we've actually analyzed
    Friend nEventsFit As Integer            'the number of events we'll extract from the results and fit to our probability distribution
    Friend Langbein As Boolean
    Public ResultsType As GeneralFunctions.enmModelResultsAspect 'the type of data we will have to fit
    Public Parameter As String                                      'the name of the parameter to fit
    Public Duration As Integer

    Friend DistributionType As GeneralFunctions.EnmProbabilityDistribution
    Friend MaxIterations As Integer
    Friend CensoringType As GeneralFunctions.enmStatisticalCensoring
    Friend TargetLevelCensoringMargin As Double
    Friend ExtremeValuesStatLocations As clsExtremeValuesStatParameterLocations
    Friend PatternClasses As clsPatternClasses
    Public LocationsList As New Dictionary(Of String, String)

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        ExtremeValuesStatLocations = New clsExtremeValuesStatParameterLocations(Me.Setup, Me)
    End Sub



    Public Sub SetParameter(myParName As String)
        Parameter = myParName
    End Sub

    Public Sub SetDuration(myDuration As Integer)
        Duration = myDuration
    End Sub

    Public Sub SetDistributionType(myType As GeneralFunctions.EnmProbabilityDistribution)
        DistributionType = myType
    End Sub
    Public Function GetDistributionType() As GeneralFunctions.EnmProbabilityDistribution
        Return DistributionType
    End Function

    Public Function GetDistributionTypeString() As String
        Return DistributionType.ToString
    End Function

    Public Function exportPatternStatistics(ExcelFile As String)
        Try
            Dim c As Integer
            Setup.ExcelFile.Path = ExcelFile
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                Dim ws As clsExcelSheet = Setup.ExcelFile.GetAddSheet(myLoc.ID)
                c = -2

                '    For Each EventPattern As clsPatternEventClass In myLoc.Patterns.Classes.Values
                '        r = 0
                '        c += 2
                '        ws.ws.Cells(r, c).Value = "ID:"
                '        ws.ws.Cells(r, c + 1).Value = EventPattern.getID
                '        r += 1
                '        ws.ws.Cells(r, c).Value = "Events:"
                '        ws.ws.Cells(r, c + 1).Value = EventPattern.CountEvents
                '        r += 1
                '        ws.ws.Cells(r, c).Value = "From percentage"
                '        ws.ws.Cells(r, c + 1).Value = "To percentage"
                '        For Each Segment As clsPatternSegment In EventPattern.GetSegments
                '            r += 1
                '            ws.ws.Cells(r, c).Value = Segment.GetLowerBound
                '            ws.ws.Cells(r, c + 1).Value = Segment.GetUpperBound
                '        Next
                '    Next
            Next
            Setup.ExcelFile.Save(True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function exportPatternStatistics of class clsExtremeValueStatistics.")
            Return False
        End Try

    End Function

    Public Function DefineTemporalPatterns(Divisions As List(Of Integer), Percentages As List(Of Integer)) As Boolean
        Try

            ''this function walks through all events of the given parameter and location and classifies their temporal pattern
            'Dim Values As New List(Of Double)
            'Dim i As Integer, eventSum As Double
            'For i = 0 To dt.Rows.Count - 1
            '    Values.Add(dt.Rows(i)(0))
            '    eventSum += dt.Rows(i)(0)
            'Next
            'Dim PatternBlock As New clsTemporalPatternBlock(Me.Setup, Values)
            'Dim PatternString As String = ""
            'PatternBlock.Analyze(Divisions, Percentages)
            'Return True

        Catch ex As Exception

        End Try

    End Function

    Public Sub setStatisticalMethod(ByVal myDistributionType As GeneralFunctions.EnmProbabilityDistribution, myMaxIterations As Integer)
        DistributionType = myDistributionType
        MaxIterations = myMaxIterations
    End Sub

    Public Sub setResultsType(ByVal myResultsType As GeneralFunctions.enmModelResultsAspect)
        ResultsType = myResultsType
    End Sub

    Public Sub setDatasetInformation(ByVal YearsObserved As Integer, ByVal EventsFit As Integer)
        nYearsObserved = YearsObserved
        nEventsFit = EventsFit
    End Sub

    Public Sub ReadModelLocations()

        'rebuild the entire LOCATIONS table
        Setup.GeneralFunctions.SQLiteDropTable(Me.Setup.SqliteCon, "LOCATIONS")
        Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "LOCATIONS")
        Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "LOCATIONS", "OBJECTID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
        Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "LOCATIONS", "XCOORD", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
        Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "LOCATIONS", "YCOORD", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)

        'import the water level points to the database
        Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.WaterlevelPointsToDatabase(Me.Setup.SqLiteCon, "LOCATIONS", "OBJECTID", "XCOORD", "YCOORD")
        Me.Setup.SqLiteCon.Close()
        Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete", 0, 10, True)

    End Sub

    Public Sub RebuildVolumesTable()
        'rebuild the entire VOLUMES table
        Setup.GeneralFunctions.SQLiteDropTable(Me.Setup.SqliteCon, "VOLUMES")
        Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "VOLUMES")
        Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "OBJECTID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
        Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "EVENTDATE", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
        Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "VOLUMES", "MAXVAL", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
    End Sub

    Public Function ToExcel() As Boolean
        Dim r As Long = 0, c As Long = 0

        Try
            Call MaximaToExcel()
            Call PlottingPositionsToExcel()

            If CensoringType = GeneralFunctions.enmStatisticalCensoring.AutoDetect AndAlso DistributionType = GeneralFunctions.EnmProbabilityDistribution.GumbelMax Then
                'actually export the two distributions that were used to estimate the threshold value
                Call ReturnPeriodsDetailToExcel(GeneralFunctions.enmStatisticalFitSection.LEFTSECTION)
                Call ReturnPeriodsDetailToExcel(GeneralFunctions.enmStatisticalFitSection.RIGHTSECTION)
            Else
                Call ReturnPeriodsToExcel()
                Call ReturnPeriodsDetailToExcel(GeneralFunctions.enmStatisticalFitSection.ALL)
            End If
            Call BootstrappedDataToExcel()

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function MaximaToExcel() As Boolean
        Try
            Dim r As Long, c As Long
            'this function exports the current results to Excel. 
            'please notice that we cannot
            Dim ws As clsExcelSheet = Setup.ExcelFile.GetAddSheet("Maxima")
            ws.ws.Cells(0, 0).Value = "Event"

            'note: the values as they are present in the locations have been sorted by value, in descending order
            'however, for writing to this summary sheet we want to write them by event number, in ascending order
            'therefore we will sort the dictionary by key
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                r += 1
                ws.ws.Cells(r, 0).Value = myLoc.ID
                Dim sortedDict As New Dictionary(Of Double, clsExtremeValue)
                sortedDict = (From entry In myLoc.Values Order By entry.Key Ascending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)

                'walk through each event and write the computed value to the worksheet
                For c = 0 To sortedDict.Values.Count - 1
                    ws.ws.Cells(0, c + 1).Value = sortedDict.Keys(c)
                    ws.ws.Cells(r, c + 1).Value = sortedDict.Values(c).Value
                Next
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function PlottingPositionsToExcel() As Boolean
        Try
            Dim c As Long, r As Long
            Dim ws As clsExcelSheet

            ws = Setup.ExcelFile.GetAddSheet("Plotting Positions")
            c = 0
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                r = 0
                ws.ws.Cells(r, c).Value = myLoc.ID
                ws.ws.Cells(r, c + 1).Value = myLoc.Dist.DistributionType.ToString
                r += 1
                ws.ws.Cells(r, c).Value = "Weibull"
                ws.ws.Cells(r, c + 1).Value = "Gringorten"
                ws.ws.Cells(r, c + 2).Value = "Bos-Levenbach"
                ws.ws.Cells(r, c + 3).Value = "Value"

                Select Case myLoc.Dist.DistributionType
                    Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                        For i = 0 To myLoc.AnnualMax.Count - 1
                            r += 1
                            ws.ws.Cells(r, c).Value = myLoc.AnnualMax.Values(i).PPWeibull
                            ws.ws.Cells(r, c + 1).Value = myLoc.AnnualMax.Values(i).PPGringorten
                            ws.ws.Cells(r, c + 2).Value = myLoc.AnnualMax.Values(i).PPBosLevenbach
                            ws.ws.Cells(r, c + 3).Value = myLoc.AnnualMax.Values(i).Value
                        Next
                    Case Is = GeneralFunctions.EnmProbabilityDistribution.Exponential, GeneralFunctions.EnmProbabilityDistribution.LogNormal, GeneralFunctions.EnmProbabilityDistribution.GenPareto
                        For i = 0 To myLoc.Values.Count - 1
                            r += 1
                            ws.ws.Cells(r, c).Value = myLoc.Values.Values(i).PPWeibull
                            ws.ws.Cells(r, c + 1).Value = myLoc.Values.Values(i).PPGringorten
                            ws.ws.Cells(r, c + 2).Value = myLoc.Values.Values(i).PPBosLevenbach
                            ws.ws.Cells(r, c + 3).Value = myLoc.Values.Values(i).Value
                        Next
                End Select
                c += 4
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function ReturnPeriodsDetailToExcel(FitSection As GeneralFunctions.enmStatisticalFitSection) As Boolean
        Try
            Dim c As Long, r As Long, i As Integer
            Dim ws As clsExcelSheet, SheetName As String = ""
            Dim myDist As clsProbabilityDistribution = Nothing

            'fitNumber is an indicator to specify which fit to export to excel:
            '0 = the overall fit
            '1 = the fit left of the threshold
            '2 = the fit right of the threshold

            Select Case FitSection
                Case Is = GeneralFunctions.enmStatisticalFitSection.ALL
                    SheetName = "Return Periods Detail"
                Case Is = GeneralFunctions.enmStatisticalFitSection.LEFTSECTION
                    SheetName = "Return Periods Detail Left"
                Case Is = GeneralFunctions.enmStatisticalFitSection.RIGHTSECTION
                    SheetName = "Return Periods Detail Right"
            End Select

            c = 0
            ws = Setup.ExcelFile.GetAddSheet(SheetName)
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values

                Select Case FitSection
                    Case Is = GeneralFunctions.enmStatisticalFitSection.ALL
                        myDist = myLoc.dist
                    Case Is = GeneralFunctions.enmStatisticalFitSection.LEFTSECTION
                        myDist = myLoc.DistLeft
                    Case Is = GeneralFunctions.enmStatisticalFitSection.RIGHTSECTION
                        myDist = myLoc.DistRight
                End Select

                r = 0
                ws.ws.Cells(r, c).Value = myLoc.ID
                ws.ws.Cells(r, c + 1).Value = myLoc.Dist.DistributionType.ToString

                'write the parameter values for the fit
                For i = 1 To myDist.ParamCount
                    r += 1
                    Select Case i
                        Case 1
                            ws.ws.Cells(r, c).Value = "locationpar"
                            ws.ws.Cells(r, c + 1).Value = myDist.GetLocPar
                        Case 2
                            ws.ws.Cells(r, c).Value = "scalepar"
                            ws.ws.Cells(r, c + 1).Value = myDist.GetScalePar
                        Case 3
                            ws.ws.Cells(r, c).Value = "shapepar"
                            ws.ws.Cells(r, c + 1).Value = myDist.GetShapePar
                    End Select
                Next
                r += 1
                ws.ws.Cells(r, c).Value = "Max Likelihood"
                ws.ws.Cells(r, c + 1).Value = myLoc.FitParameters.getMLE
                r += 1
                ws.ws.Cells(r, c).Value = "Akaike (AIC)"
                ws.ws.Cells(r, c + 1).Value = myLoc.FitParameters.getAIC
                r += 1
                ws.ws.Cells(r, c).Value = "Kolmogorov-Smirnov (KS)"
                ws.ws.Cells(r, c + 1).Value = myLoc.FitParameters.getKS

                'for POT distributions write the threshold value
                If IsPOTDistribution(myDist.DistributionType) Then
                    r += 1
                    ws.ws.Cells(r, c).Value = "Threshold"
                    Select Case FitSection
                        Case Is = GeneralFunctions.enmStatisticalFitSection.ALL
                            ws.ws.Cells(r, c + 1).Value = myLoc.ThresholdValue
                    End Select
                End If

                r += 1
                ws.ws.Cells(r, c).Value = "Return Period"
                ws.ws.Cells(r, c + 1).Value = "Value"

                Dim myRes As New Dictionary(Of Double, clsExtremeValue)
                Dim tmp As String = ""

                'build a collection of sample data for the distributions
                Call myLoc.BuildAllDatacollectionsFromDistributions()

                Select Case FitSection
                    Case Is = GeneralFunctions.enmStatisticalFitSection.ALL
                        tmp = "ALL"
                        myRes = myLoc.fitResults
                    Case Is = GeneralFunctions.enmStatisticalFitSection.LEFTSECTION
                        myRes = myLoc.fitResultsLeft
                        tmp = "Left"
                    Case Is = GeneralFunctions.enmStatisticalFitSection.RIGHTSECTION
                        myRes = myLoc.fitResultsRight
                        tmp = "Right"
                End Select

                If Not myRes Is Nothing Then
                    For i = 0 To myRes.Values.Count - 1
                        r += 1
                        ws.ws.Cells(r, c).Value = myRes.Values(i).ReturnPeriod
                        ws.ws.Cells(r, c + 1).Value = myRes.Values(i).Value
                        Me.Setup.Log.AddMessage("to Excel " & tmp & "," & myRes.Values(i).ReturnPeriod & "," & myRes.Values(i).Value)
                    Next
                End If
                c += 2
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function IsPOTDistribution(myDist As GeneralFunctions.EnmProbabilityDistribution) As Boolean
        Select Case myDist
            Case Is = GeneralFunctions.EnmProbabilityDistribution.GenPareto
                Return True
            Case Is = GeneralFunctions.EnmProbabilityDistribution.Exponential
                Return True
            Case Is = GeneralFunctions.EnmProbabilityDistribution.LogNormal
                Return True
        End Select
        Return False
    End Function


    Public Function ReturnPeriodsToExcel() As Boolean
        Try
            Dim r As Long, c As Long, Herh As Double, HerhCorr As Double, p_exceedance As Double, myVal As Double
            Dim myQuery As String, dt As New DataTable
            Dim repEventNum As Integer
            Dim ws As STOCHLIB.clsExcelSheet
            ws = Setup.ExcelFile.GetAddSheet("Fit results by threshold")
            c = 0

            'read all locations from database
            myQuery = "SELECT * FROM LOCATIONS;"
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, myQuery, dt, False)

            'next write the T=10,T=25,T=50 and T=100 water levels
            r = 0
            ws = Setup.ExcelFile.GetAddSheet("Return Periods")
            For Each myloc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                ws.ws.Cells(0, 0).Value = "Location ID"
                ws.ws.Cells(0, 1).Value = "XCOORD"
                ws.ws.Cells(0, 2).Value = "YCOORD"
                ws.ws.Cells(0, 3).Value = "T=10"
                ws.ws.Cells(0, 4).Value = "T=25"
                ws.ws.Cells(0, 5).Value = "T=50"
                ws.ws.Cells(0, 6).Value = "T=100"
                ws.ws.Cells(0, 7).Value = "T10-Repr.Event"
                ws.ws.Cells(0, 8).Value = "T25-Repr.Event"
                ws.ws.Cells(0, 9).Value = "T50-Repr.Event"
                ws.ws.Cells(0, 10).Value = "T100-Repr.Event"
                r += 1
                ws.ws.Cells(r, 0).Value = myloc.ID

                'schrijf de coordinaten van het modelobject
                'v1.798: made an explicit reference to the OBJECTID, XCOORD and YCOORD columns. This replaces the reference to column index since it produced wrong results.
                For i = 0 To dt.Rows.Count - 1
                    If dt.Rows(i)("OBJECTID") = myloc.ID Then
                        ws.ws.Cells(r, 1).Value = dt.Rows(i)("XCOORD")
                        ws.ws.Cells(r, 2).Value = dt.Rows(i)("YCOORD")
                        Exit For
                    End If
                Next

                For c = 3 To 6
                    Select Case c
                        'first calculate a corrected return period, given the fact that our dataset only contains the tail
                        'then calculate the exceedance probability and subsequently the value that corresponds to it
                        Case Is = 3
                            Herh = 10
                        Case Is = 4
                            Herh = 25
                        Case Is = 5
                            Herh = 50
                        Case Is = 6
                            Herh = 100
                        Case Else
                            Throw New Exception("Error writing return periods.")
                    End Select
                    HerhCorr = Herh * myloc.nEventsFit / nYearsObserved
                    p_exceedance = 1 / HerhCorr
                    myVal = myloc.GetValueFromExceedanceProbability(p_exceedance)
                    ws.ws.Cells(r, c).Value = myVal

                    'also look up which event represents this return period best
                    repEventNum = myloc.GetRepresentativeEvent(myVal)
                    ws.ws.Cells(r, c + 4).Value = repEventNum
                Next
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function FitByThresholdToExcel() As Boolean
        Try
            Dim r As Long, c As Long, i As Long, nPar As Integer
            Dim ws As STOCHLIB.clsExcelSheet
            ws = Setup.ExcelFile.GetAddSheet("Fit results by threshold")
            c = 0
            For Each myloc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                r = 0
                ws.ws.Cells(r, c).Value = "ID"
                ws.ws.Cells(r, c + 1).Value = myloc.ID
                nPar = myloc.ThresholdsFitResults.Values(0).dist.ParamCount

                'header
                r += 1
                ws.ws.Cells(r, c).Value = "waarde"
                For i = 1 To nPar
                    Select Case i
                        Case 1
                            ws.ws.Cells(r, c + i).Value = myloc.ThresholdsFitResults.Values(r).dist.GetLocPar
                        Case 2
                            ws.ws.Cells(r, c + i).Value = myloc.ThresholdsFitResults.Values(r).dist.GetScalePar
                        Case 3
                            ws.ws.Cells(r, c + i).Value = myloc.ThresholdsFitResults.Values(r).dist.GetShapePar
                    End Select
                Next

                For r = 0 To myloc.ThresholdsFitResults.Count - 1
                    ws.ws.Cells(r + 2, c).Value = myloc.ThresholdsFitResults.Values(r).Threshold
                    nPar = myloc.ThresholdsFitResults.Values(r).dist.ParamCount
                    For i = 1 To nPar
                        Select Case i
                            Case 1
                                ws.ws.Cells(r + 2, c + i).Value = myloc.ThresholdsFitResults.Values(r).dist.GetLocPar
                            Case 2
                                ws.ws.Cells(r + 2, c + i).Value = myloc.ThresholdsFitResults.Values(r).dist.GetScalePar
                            Case 3
                                ws.ws.Cells(r + 2, c + i).Value = myloc.ThresholdsFitResults.Values(r).dist.GetShapePar
                        End Select
                    Next
                Next
                c += nPar + 1
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function BootstrappedDataToExcel() As Boolean
        Try
            Dim r As Long, c As Long
            Dim ws As STOCHLIB.clsExcelSheet
            ws = Setup.ExcelFile.GetAddSheet("Confidence Intervals")
            c = 0
            'write the bootstrapping results (the sampled values)
            For Each myloc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                If myloc.BSLower IsNot Nothing Then
                    r = 0
                    'write the 2.5 and 97.5 confidence intervals
                    ws.ws.Cells(r, c).Value = myloc.ID
                    ws.ws.Cells(r, c + 1).Value = "lower 2.5%"
                    ws.ws.Cells(r, c + 2).Value = "upper 97.5%"
                    For r = 0 To myloc.BSLower.Count - 1
                        ws.ws.Cells(r + 1, c).Value = myloc.BSLower.Values(r).ReturnPeriod
                        ws.ws.Cells(r + 1, c + 1).Value = myloc.BSLower.Values(r).Value
                        ws.ws.Cells(r + 1, c + 2).Value = myloc.BSUpper.Values(r).Value
                    Next
                    c += 3
                End If

            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function getLocationResults(LocationName As String) As clsExtremeValuesStatParameterLocation
        Return ExtremeValuesStatLocations.GetLocation(LocationName)
    End Function

    Public Function ReadHisfilesDirResults(ByVal HisFilesDir As String, ByVal Hisfile As GeneralFunctions.enmSobekResultsObjectTypes, PartOfParName As GeneralFunctions.enmSobekResultsType, SkipPercentage As Integer, ReadLocationsSubset As Boolean, Optional ByVal LocStartIdx As Integer = 0, Optional ByVal LocEndIdx As Integer = 0, Optional ByVal SpecificLocation As Boolean = False, Optional ByVal LocationIDs As String = "", Optional ByVal ByWildcard As Boolean = False, Optional ByVal StringWithWildcards As String = "") As Boolean
        Try
            Dim calcReader As clsHisFileBinaryReader = Nothing
            Dim tmpLocations As New List(Of String)
            Dim defLocations As New List(Of String)
            Dim FirstCaseRead As Boolean = False
            Dim i As Long, EventIdx As Long = 0, nEvents As Long
            Dim startIdx As Long, EventNum As Long
            Dim idx As Long
            Dim Dirs As String(), myDir As String
            Dim query As String
            Dim dt As New DataTable
            Dim HisFileFullPath As String
            Dim HiaFileFullPath As String
            'note: the maxLocations variable is meant for debugging purposes!
            'its default is set to 0, which means that all locations are read.

            'reads results from one or more SOBEK-cases that contain a series of events
            Setup.GeneralFunctions.UpdateProgressBar("Reading SOBEK results from hisfile dir.", 0, 10, True)
            If Directory.Exists(HisFilesDir) Then
                Dirs = Directory.GetDirectories(HisFilesDir)
                nEvents = Dirs.Count
                For Each myDir In Dirs

                    EventIdx += 1

                    Me.Setup.GeneralFunctions.UpdateCustomProgressBar(Me.Setup.progressBar2, EventIdx, nEvents, True)

                    If Not Me.Setup.GeneralFunctions.GetIntFromString(myDir, EventNum) Then
                        EventNum = EventIdx
                    End If

                    Setup.GeneralFunctions.UpdateProgressBar("Processing event " & EventIdx & " of " & nEvents, EventIdx, nEvents, True, 2)

                    'v1.72 corrected an error: here calcpnt.his was hard coded
                    HisFileFullPath = myDir & "\" & Hisfile
                    HiaFileFullPath = Replace(HisFileFullPath, ".his", ".hia",,, CompareMethod.Text)
                    calcReader = New clsHisFileBinaryReader(HisFileFullPath, Setup)

                    If Not System.IO.File.Exists(HisFileFullPath) Then
                        Me.Setup.Log.AddError("Error: file " & Hisfile & " not found in directory " & myDir & ".. Directory was ignored.")
                        Continue For
                    ElseIf Not System.IO.File.Exists(HiaFileFullPath) Then
                        Me.Setup.Log.AddWarning("Error: file not found in directory " & myDir & ": " & HiaFileFullPath & ". Long location ID's (> 22 characters) cannot be processed.")
                    End If

                    calcReader.OpenFile()
                    calcReader.ReadHisHeader()

                    'read all locations from the hisfile
                    If Not FirstCaseRead Then
                        If SpecificLocation Then
                            tmpLocations = Setup.GeneralFunctions.ParseStringToList(LocationIDs, ";")
                        ElseIf ByWildcard Then
                            tmpLocations = calcReader.ReadLocationsByWildcard(StringWithWildcards)
                        ElseIf Not ReadLocationsSubset Then
                            tmpLocations = calcReader.ReadAllLocations()
                        Else
                            tmpLocations = calcReader.ReadLocationSubset(LocStartIdx, LocEndIdx)
                        End If

                        'only copy those locations to the final list that do Not yet have existing results in the database
                        query = "SELECT DISTINCT LOCATIONID FROM EVENTS;"
                        Call Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt)
                        Dim databaseResults As List(Of String) = Setup.GeneralFunctions.dataTable2ListOfString(dt)
                        For Each myLocation As String In tmpLocations
                            If Not databaseResults.Contains(myLocation) AndAlso Not defLocations.Contains(myLocation) Then defLocations.Add(myLocation)
                        Next

                        'set the flag that the first file has been read to prevent re-reading it
                        FirstCaseRead = True
                    End If

                    '-------------------------------------------------------------------------
                    ' variant om het sneller te maken
                    '-------------------------------------------------------------------------
                    Dim resTable As New DataTable()
                    Dim timesteps As New List(Of DateTime)
                    resTable = calcReader.ReadAllDataOneParameterToDataTable(PartOfParName, 0)
                    timesteps = calcReader.ReadAllTimesteps
                    Dim skip As Integer = SkipPercentage / 100 * timesteps.Count
                    Dim locationIdx As Integer

                    'now add all locations to the extreme values class
                    idx = -1
                    Dim nLocs As Integer = defLocations.Count
                    Dim nTimesteps As Integer = timesteps.Count
                    Dim CurVal As Double = -9.0E+99
                    Dim CurMax As Double = -9.0E+99
                    Dim EventDate As DateTime
                    Dim EventSum As Double

                    If Me.Setup.SqliteCon.State = ConnectionState.Closed Then Me.Setup.SqliteCon.Open()

                    'N.B. The method using BeginTransaction and Commit has been implemented in v1.72
                    'it has resulted in a dramatic speed-up of the process that writes maximum values to the database!
                    'https://www.jokecamp.com/blog/make-your-sqlite-bulk-inserts-very-fast-in-c/
                    Using cmd As New SQLite.SQLiteCommand
                        cmd.Connection = Me.Setup.SqliteCon
                        Using transaction = Me.Setup.SqliteCon.BeginTransaction

                            'prepare the parameters for our insert query
                            cmd.Parameters.Clear()
                            cmd.Parameters.Add("@LOC", DbType.String)
                            cmd.Parameters.Add("@DATE", DbType.DateTime)
                            cmd.Parameters.Add("@PAR", DbType.String)
                            cmd.Parameters.Add("@MAX", DbType.Double)
                            cmd.Parameters.Add("@EVENTNUM", DbType.Int16)
                            cmd.Parameters.Add("@EVENTSUM", DbType.Double)
                            cmd.Parameters.Add("@TS", DbType.Int16)

                            Setup.GeneralFunctions.UpdateProgressBar("Processing locations", 0, nLocs, True)
                            startIdx = Math.Max(0, nTimesteps * SkipPercentage / 100 - 1)
                            For Each myLocation As String In defLocations
                                idx += 1

                                Setup.GeneralFunctions.UpdateProgressBar("", idx + 1, nLocs)
                                CurVal = -9.0E+99
                                CurMax = -9.0E+99

                                'get the hisfile location index
                                calcReader.GetLocationIdx(myLocation, locationIdx)
                                EventSum = 0
                                EventDate = timesteps(0)
                                For i = startIdx To nTimesteps - 1
                                    CurVal = resTable.Rows(i)(locationIdx)
                                    EventSum += resTable.Rows(i)(locationIdx)
                                    If CurVal > CurMax Then
                                        CurMax = CurVal
                                    End If
                                Next

                                'store the event's maximum in the database
                                'siebe v1.72: implemented this method in order to speed up the process. The query is being created just once; the values are adjusted for each location
                                'this is called a parameterized query
                                cmd.Parameters(0).Value = myLocation
                                cmd.Parameters(1).Value = EventDate
                                cmd.Parameters(2).Value = PartOfParName
                                cmd.Parameters(3).Value = CurMax
                                cmd.Parameters(4).Value = EventNum
                                cmd.Parameters(5).Value = EventSum
                                cmd.Parameters(6).Value = timesteps.Count
                                cmd.CommandText = "INSERT INTO EVENTS (LOCATIONID, DATEANDTIME, PARAMETER, DATAVALUE, EVENTNUM, EVENTSUM, DURATION) VALUES (@LOC,@DATE,@PAR,@MAX,@EVENTNUM,@EVENTSUM,@TS);"
                                cmd.ExecuteNonQuery()
                            Next
                            transaction.Commit() 'this is where the bulk insert is finally executed.
                        End Using
                    End Using


                    calcReader.Close()
                Next
            End If

            Setup.GeneralFunctions.UpdateProgressBar("Ready.", 0, 10)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function SetLocationsAndPopulateCombobox(Subset As Boolean, LocationsStr As String, SubsetByIndex As Boolean, FromNum As Integer, ToNum As Integer, TableName As String, ByRef myComboBox As Windows.Forms.ComboBox) As Boolean
        Dim query As String
        Try
            'this function sets the location ID's to be processed
            myComboBox.Items.Clear()
            LocationsList = New Dictionary(Of String, String)

            If Duration = 0 Then
                'duration 0 is a flag for 'all durations'
                query = "SELECT DISTINCT LOCATIONID FROM EVENTS;"
            Else
                query = "SELECT DISTINCT LOCATIONID FROM EVENTS WHERE DURATION=" & Duration & ";"
            End If

            Dim ID As String, i As Integer
            If Subset Then
                While Not LocationsStr = ""
                    ID = Me.Setup.GeneralFunctions.ParseString(LocationsStr, ";")
                    If Not LocationsList.ContainsKey(ID.Trim.ToUpper) Then
                        LocationsList.Add(ID.Trim.ToUpper, ID)
                    End If
                End While
                Me.Setup.GeneralFunctions.populateComboBoxFromDictionary(LocationsList, myComboBox)
            ElseIf SubsetByIndex Then
                Dim dt As New DataTable
                Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, dt)
                For i = Math.Max(0, FromNum - 1) To Math.Min(dt.Rows.Count - 1, ToNum - 1)
                    LocationsList.Add(dt.Rows(i)(0).ToString.Trim.ToUpper, dt.Rows(i)(0))
                Next
                Me.Setup.GeneralFunctions.populateComboBoxFromDictionary(LocationsList, myComboBox)
            Else
                Dim dt As New DataTable
                Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, dt)
                For i = 0 To dt.Rows.Count - 1
                    LocationsList.Add(dt.Rows(i)(0).ToString.Trim.ToUpper, dt.Rows(i)(0))
                Next
                Me.Setup.GeneralFunctions.populateComboBoxFromDictionary(LocationsList, myComboBox)
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function Preprocessing() As Boolean
        Try
            Dim LocDT As New DataTable
            Dim StorDT As New DataTable
            Dim WlDT As New DataTable

            'If FitDataType = GeneralFunctions.enmModelResultsType.Volume Then
            'we'll need to transform observed waterlevels to volumes, using the STORAGETABLES table

            'first drop the existing table containing volumes and rebuild it
            Setup.GeneralFunctions.SQLiteDropTable(Setup.SqliteCon, "VOLUMES")
            Setup.GeneralFunctions.SQLiteCreateTable(Setup.SqliteCon, "VOLUMES")
            Setup.GeneralFunctions.SQLiteCreateColumn(Setup.SqliteCon, "VOLUMES", "OBJECTID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "OBJECTIDX")
            Setup.GeneralFunctions.SQLiteCreateColumn(Setup.SqliteCon, "VOLUMES", "EVENTDATE", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            Setup.GeneralFunctions.SQLiteCreateColumn(Setup.SqliteCon, "VOLUMES", "MAXVAL", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)

            'get a list of all unique object id's
            Dim query As String = "SELECT DISTINCT OBJECTID, AREAID FROM LOCATIONS;"
            Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, LocDT)

            'for each object, retrieve the storagetable, calculate the volumes that belong to each water level and write them to the VOLUMES table
            For i = 0 To LocDT.Rows.Count - 1
                StorDT = New DataTable
                WlDT = New DataTable
                query = "SELECT * FROM STORAGETABLES WHERE AREAID = '" & LocDT.Rows(i)(1) & "';"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, StorDT)

                query = "SELECT EVENTDATE, MAXVAL FROM WATERLEVELS WHERE LOCATIONID = '" & LocDT.Rows(i)(0) & "';"
                Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqLiteCon, query, WlDT)



            Next




            'Else
            '    Throw New Exception("Error: results type not yet supported: " & FitDataType.ToString)
            'End If
            Return True

        Catch ex As Exception

        End Try
    End Function


    'Public Function ResultsToDatabase() As Boolean
    '    Try
    '        Dim query As String, TableName As String = ""
    '        If Not GetTableName(TableName) Then Throw New Exception("Error: results type not yet supported: " & ModelOutcomeType.ToString)

    '        Setup.GeneralFunctions.MDBDropTable(Setup.SqLiteCon, TableName)
    '        Setup.GeneralFunctions.MDBCreateTable(Setup.SqLiteCon, TableName)
    '        Setup.GeneralFunctions.MDBCreateColumn(Setup.SqLiteCon, TableName, "OBJECTID", GeneralFunctions.enmSQLDataType.SQLVARCHAR, "OBJECTID")
    '        Setup.GeneralFunctions.MDBCreateColumn(Setup.SqLiteCon, TableName, "EVENTDATE", GeneralFunctions.enmSQLDataType.SQLDATE)
    '        Setup.GeneralFunctions.MDBCreateColumn(Setup.SqLiteCon, TableName, "MAXVAL", GeneralFunctions.enmSQLDataType.SQLDOUBLE)

    '        For Each myLocation As clsExtremeValuesStatLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
    '            For Each Maxval As clsExtremeValue In myLocation.MaxVals.Values
    '                query = "INSERT INTO " & TableName & " (OBJECTID, EVENTDATE, MAXVAL) VALUES ('" & myLocation.ID.Trim & "','" & String.Format(Maxval.EventDate, "yyyy/MM/dd hh:mm:ss") & "'," & Maxval.Value & ");"
    '                If Not Setup.GeneralFunctions.SQLiteNoQuery(Setup.SqLiteCon, query, False) Then Me.Setup.Log.AddError("Error executing query " & query)
    '            Next
    '        Next
    '        Setup.SqLiteCon.Close()
    '        Return True
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error in function ResultsToDatabase of class clsExtremeValuesStatistics.")
    '        Me.Setup.Log.AddError(ex.Message)
    '        Return False
    '    End Try
    'End Function

    Public Function ReadSobekEventsResults(ByVal ResultsAt As GeneralFunctions.enmSobekResultsObjectTypes, ResultsType As GeneralFunctions.enmSobekNodeResultsTypes, WriteToEventsTable As Boolean, WriteToTimeseriesTable As Boolean, RemoveExisting As Boolean, SkipPercentage As Integer, ReadLocationsSubset As Boolean, Optional ByVal LocStartIdx As Integer = 0, Optional ByVal LocEndIdx As Integer = 0, Optional ByVal SpecificLocations As Boolean = False, Optional ByVal LocationIDs As String = "", Optional ByVal myAliases As String = "") As Boolean
        Try
            Dim calcReader As clsHisFileBinaryReader = Nothing
            Dim Locations As New List(Of String)
            Dim Aliases As New List(Of String)
            Dim FirstCaseRead As Boolean = False
            Dim ResultsTable As clsTimeTable
            Dim myExtremeValueLocation As clsExtremeValuesStatParameterLocation
            Dim i As Long, EventNum As Long = 0, nCases As Long = Setup.SOBEKData.ActiveProject.CountActiveCases
            Dim startIdx As Long
            Dim idx As Long, nActiveCases As Integer = Setup.SOBEKData.ActiveProject.CountActiveCases
            Dim CaseIdx As Integer

            'note: the maxLocations variable is meant for debugging purposes!
            'its default is set to 0, which means that all locations are read.

            'reads results from one or more SOBEK-cases that contain a series of events
            For Each myCase As ClsSobekCase In Setup.SOBEKData.ActiveProject.Cases.Values
                If myCase.InUse Then
                    CaseIdx += 1

                    Setup.GeneralFunctions.UpdateProgressBar("Reading SOBEK results for case " & CaseIdx, CaseIdx, nActiveCases, True)

                    Me.Setup.Log.AddMessage("Analyzing case " & myCase.CaseName)
                    'Debug.Print(myCase.CaseName)

                    EventNum += 1
                    Setup.GeneralFunctions.UpdateProgressBar("", EventNum - 1, nCases)

                    Dim hisFile As String = Me.Setup.GeneralFunctions.GetHisFileFromResultsAt(ResultsAt)
                    Dim ParName As String = Me.Setup.GeneralFunctions.gethisParFromResultsType(ResultsType)
                    calcReader = New clsHisFileBinaryReader(myCase.CaseDir & "\" & hisFile, Setup)
                    calcReader.OpenFile()

                    'read all locations from the hisfile
                    If Not FirstCaseRead Then
                        If SpecificLocations Then
                            Locations = Me.Setup.GeneralFunctions.ParseStringToList(LocationIDs, ";")
                            Aliases = Me.Setup.GeneralFunctions.ParseStringToList(myAliases, ";")
                            If Aliases.Count > 0 AndAlso Locations.Count <> Aliases.Count Then
                                Throw New Exception("Number of aliases must be equal to the number of locations.")
                            End If
                        ElseIf Not ReadLocationsSubset Then
                            Locations = calcReader.ReadAllLocations()
                        Else
                            Locations = calcReader.ReadLocationSubset(LocStartIdx, LocEndIdx)
                        End If
                        FirstCaseRead = True
                        'LocationsCombobox.DataSource = Locations
                    End If

                    'now add all locations to the extreme values class
                    idx = -1
                    Setup.GeneralFunctions.UpdateProgressBar("", 0, 100, True)

                    For LocIdx = 0 To Locations.Count - 1

                        'For Each myLocation As String In Locations
                        Dim myLocation As String = Locations(LocIdx)
                        Dim myAlias As String
                        If Aliases.Count = Locations.Count Then
                            myAlias = Aliases(LocIdx)
                        Else
                            myAlias = Locations(LocIdx)
                        End If

                        idx += 1
                        Setup.GeneralFunctions.UpdateProgressBar("", idx + 1, Locations.Count, True)

                        ResultsTable = New clsTimeTable(Me.Setup)
                        myExtremeValueLocation = ExtremeValuesStatLocations.GetAddLocation(myAlias)

                        'read the SOBEK results
                        If calcReader.ReadAddLocationResults(myLocation, ParName, ResultsTable, 0) Then
                            'get the maximum value for each event inside the series and write it to the location
                            'now there are two options: every event already has only the maximum written, or every event contains the entire timeseries!
                            Dim Timestep As TimeSpan
                            Dim CurMax As Double, curVal As Double, EventDate As Date
                            Timestep = ResultsTable.Records.Values(1).Datum.Subtract(ResultsTable.Records.Values(0).Datum)

                            'determine the first timestep to read results from
                            If SkipPercentage >= 100 Then Throw New Exception("Error: skipping results percentage must be between 0 and 100")
                            startIdx = Math.Max(0, ResultsTable.Records.Count * SkipPercentage / 100)

                            'initialize the maxmum to the result of the first timestep
                            CurMax = ResultsTable.Records.Values(startIdx).GetValue(0)
                            For i = startIdx + 1 To ResultsTable.Records.Count - 1
                                curVal = ResultsTable.Records.Values(i).GetValue(0)
                                If curVal > CurMax Then
                                    CurMax = curVal
                                    EventDate = ResultsTable.Records.Values(i).Datum
                                End If
                            Next
                            'store the maximum
                            myExtremeValueLocation.AddValue(EventNum, EventDate, CurMax)

                            If WriteToEventsTable Then
                                'clear old results for this location and parameter
                                If RemoveExisting Then Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM EVENTS WHERE LOCATIONID='" & myAlias & "' AND PARAMETER='" & ResultsType.ToString & "' AND CASENAME='" & myCase.CaseName & "';")

                                'write the new results to the database
                                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                                Setup.GeneralFunctions.UpdateProgressBar("Writing to Events table...", idx + 1, Locations.Count, True)
                                Using cmd As New SQLite.SQLiteCommand
                                    cmd.Connection = Me.Setup.SqliteCon
                                    Using transaction = Me.Setup.SqliteCon.BeginTransaction
                                        For i = startIdx + 1 To ResultsTable.Records.Count - 1
                                            Setup.GeneralFunctions.UpdateProgressBar("", i + 1, ResultsTable.Records.Count)
                                            cmd.CommandText = "INSERT INTO EVENTS (LOCATIONID, CASENAME, EVENTNUM, DATEANDTIME, PARAMETER, DATAVALUE) VALUES ('" & myAlias & "','" & myCase.CaseName & "',1,'" & Me.Setup.GeneralFunctions.FormatDateAsISO8601(ResultsTable.Records.Values(i).Datum) & "','" & ResultsType.ToString & "'," & ResultsTable.Records.Values(i).GetValue(0) & ");"
                                            cmd.ExecuteNonQuery()
                                        Next
                                        transaction.Commit() 'this is where the bulk insert is finally executed.
                                    End Using
                                End Using
                            End If

                            If WriteToTimeseriesTable Then
                                'clear old results for this location and parameter
                                If RemoveExisting Then Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, "DELETE FROM TIMESERIES WHERE LOCATIONID='" & myAlias & "' AND PARAMETER='" & ResultsType.ToString & "' AND CASENAME='" & myCase.CaseName & "';")

                                'write the new results to the database
                                If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()
                                Setup.GeneralFunctions.UpdateProgressBar("Writing to Timeseries table...", idx + 1, Locations.Count, True)
                                Using cmd As New SQLite.SQLiteCommand
                                    cmd.Connection = Me.Setup.SqliteCon
                                    Using transaction = Me.Setup.SqliteCon.BeginTransaction
                                        For i = startIdx + 1 To ResultsTable.Records.Count - 1
                                            Setup.GeneralFunctions.UpdateProgressBar("", i + 1, ResultsTable.Records.Count)
                                            cmd.CommandText = "INSERT INTO TIMESERIES (LOCATIONID, CASENAME, DATEANDTIME, PARAMETER, DATAVALUE) VALUES ('" & myAlias & "','" & myCase.CaseName & "','" & Me.Setup.GeneralFunctions.FormatDateAsISO8601(ResultsTable.Records.Values(i).Datum) & "','" & ResultsType.ToString & "'," & ResultsTable.Records.Values(i).GetValue(0) & ");"
                                            cmd.ExecuteNonQuery()
                                        Next
                                        transaction.Commit() 'this is where the bulk insert is finally executed.
                                    End Using
                                End Using
                            End If

                        End If
                    Next
                    calcReader.Close()
                End If
            Next
            Setup.GeneralFunctions.UpdateProgressBar("Ready.", 0, 10)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function ReadSobekSeriesResults(ByVal Hisfile As GeneralFunctions.enmSobekResultsObjectTypes, PartOfParName As GeneralFunctions.enmSobekResultsType, SkipPercentage As Integer, ReadLocationsSubset As Boolean, Optional ByVal LocStartIdx As Integer = 0, Optional ByVal LocEndIdx As Integer = 0, Optional ByVal SpecificLocation As Boolean = False, Optional ByVal LocationIDs As String = "", Optional ByVal ByWildcard As Boolean = False, Optional ByVal StringWithWildcards As String = "") As Boolean
        Try
            Dim calcReader As clsHisFileBinaryReader = Nothing
            Dim tmpLocations As New List(Of String)
            Dim defLocations As New List(Of String)
            Dim Locations As New List(Of String)
            Dim FirstCaseRead As Boolean = False
            Dim ResultsTable As New clsTimeTable(Me.Setup)
            Dim myExtremeValueLocation As clsExtremeValuesStatParameterLocation
            Dim i As Long, EventNum As Long = 0
            Dim nCases As Long = Setup.SOBEKData.ActiveProject.CountActiveCases
            Dim query As String
            Dim dt As New DataTable

            Dim EventValues As New List(Of Double)

            Dim locIdx As Integer
            Dim nActiveCases As Integer = Setup.SOBEKData.ActiveProject.CountActiveCases
            Dim CaseIdx As Integer
            Dim ParIdx As Integer


            'reads results from one or more SOBEK-cases that contain a series of events
            For Each myCase As ClsSobekCase In Setup.SOBEKData.ActiveProject.Cases.Values
                If myCase.InUse Then

                    CaseIdx += 1

                    Setup.GeneralFunctions.UpdateProgressBar("Reading SOBEK results for case " & CaseIdx, CaseIdx, nActiveCases, True)
                    Me.Setup.Log.AddMessage("Analyzing case " & myCase.CaseName)

                    EventNum += 1
                    Setup.GeneralFunctions.UpdateProgressBar("", EventNum - 1, nCases)

                    calcReader = New clsHisFileBinaryReader(myCase.CaseDir & "\" & Hisfile, Setup)
                    calcReader.OpenFile()
                    calcReader.ReadHisHeader()

                    If Not calcReader.GetParameterIdx(PartOfParName, ParIdx) Then Throw New Exception("Cannot find parameter " & PartOfParName & " in SOBEK resultsfile " & Hisfile)

                    If Not FirstCaseRead Then
                        If SpecificLocation Then
                            tmpLocations = Setup.GeneralFunctions.ParseStringToList(LocationIDs, ";")
                        ElseIf ByWildcard Then
                            tmpLocations = calcReader.ReadLocationsByWildcard(StringWithWildcards)
                        ElseIf Not ReadLocationsSubset Then
                            tmpLocations = calcReader.ReadAllLocations()
                        Else
                            tmpLocations = calcReader.ReadLocationSubset(LocStartIdx, LocEndIdx)
                        End If

                        'only copy those locations to the final list that do Not yet have existing results in the database
                        query = "SELECT DISTINCT LOCATIONID FROM EVENTS;"
                        Call Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt)
                        Dim databaseResults As List(Of String) = Setup.GeneralFunctions.dataTable2ListOfString(dt)
                        For Each myLocation As String In tmpLocations
                            If Not databaseResults.Contains(myLocation) AndAlso Not defLocations.Contains(myLocation) Then defLocations.Add(myLocation)
                        Next

                        'set the flag that the first file has been read to prevent re-reading it
                        FirstCaseRead = True

                    End If

                    'now add all locations to the extreme values class
                    Me.Setup.GeneralFunctions.UpdateProgressBar("Reading results from series...", 0, 10, True)
                    For Each myLocation As String In defLocations
                        ResultsTable = New clsTimeTable(Me.Setup)

                        Me.Setup.GeneralFunctions.UpdateProgressBar("", locIdx + 1, defLocations.Count)

                        locIdx += 1
                        EventNum = 0
                        myExtremeValueLocation = ExtremeValuesStatLocations.GetAddLocation(myLocation)

                        'read the SOBEK results
                        calcReader.ReadAddLocationResults(myLocation, PartOfParName, ResultsTable, 0)

                        'get the maximum value for each event inside the series and write it to the location
                        'now there are two options: every event already has only the maximum written, or every event contains the entire timeseries!
                        Dim Timestep1 As TimeSpan, Timestep2 As TimeSpan
                        Dim CurMax As Double, EventDate As Date
                        Dim nTimesteps As Integer, EventSum As Double, EventMax As Double
                        Timestep1 = ResultsTable.Records.Values(1).Datum.Subtract(ResultsTable.Records.Values(0).Datum)
                        Timestep2 = ResultsTable.Records.Values(2).Datum.Subtract(ResultsTable.Records.Values(1).Datum)

                        If Timestep1 = Timestep2 Then
                            'every event has multiple values. 

                            'Initialize the first event's values
                            EventValues = New List(Of Double)
                            EventDate = ResultsTable.Records.Values(0).Datum
                            EventValues.Add(ResultsTable.Records.Values(0).GetValue(0))
                            EventNum = 1
                            For i = 1 To ResultsTable.Records.Count - 1
                                If ResultsTable.Records.Values(i).Datum.Subtract(ResultsTable.Records.Values(i - 1).Datum) <> Timestep1 Then

                                    Call WrapUpEvent(SkipPercentage, EventValues, EventSum, EventMax)
                                    Call StoreEventInDatabase(EventDate, myLocation, PartOfParName, EventNum, EventMax, EventSum, EventValues.Count)
                                    'myExtremeValueLocation.AddValue(EventNum, EventDate, CurMax)   'this is the old way

                                    'start the next event!
                                    EventNum += 1
                                    EventValues = New List(Of Double)
                                    EventDate = ResultsTable.Records.Values(i).Datum
                                    EventValues.Add(ResultsTable.Records.Values(i).GetValue(0))
                                Else
                                    'still inside the same event so add the value
                                    EventValues.Add(ResultsTable.Records.Values(i).GetValue(0))
                                End If
                            Next
                            'the final event has not yet been stored
                            Call WrapUpEvent(SkipPercentage, EventValues, EventSum, EventMax)
                            myExtremeValueLocation.AddValue(EventNum, EventDate, CurMax)
                        Else
                            'every event contains only one value (usually the maximum)
                            For i = 0 To ResultsTable.Records.Count - 1
                                EventNum += 1
                                CurMax = ResultsTable.Records.Values(i).GetValue(0)
                                EventSum += CurMax
                                nTimesteps += 1
                                myExtremeValueLocation.AddValue(EventNum, ResultsTable.Records.Values(i).Datum, CurMax)
                            Next

                            Call StoreEventInDatabase(EventDate, myLocation, PartOfParName, EventNum, EventMax, EventSum, EventValues.Count)

                        End If

                    Next
                    calcReader.Close()
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function
    Public Function WrapUpEvent(SkipPercentage As Double, ByRef EventValues As List(Of Double), ByRef EventSum As Double, ByRef EventMax As Double) As Boolean
        Try
            'a new event starts here, but first wrap up and store the previous event
            Dim startIdx As Integer = SkipPercentage / 100 * EventValues.Count
            EventMax = -9.0E+99
            EventSum = 0
            For j = startIdx To EventValues.Count - 1
                EventSum += EventValues(j)
                If EventValues(j) > EventMax Then EventMax = EventValues(j)
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function StoreEventInDatabase(EventDate As DateTime, myLocation As String, PartOfParName As String, EventNum As Integer, EventMax As Double, EventSum As Double, nTimesteps As Integer) As Boolean
        Try
            'store the event's maximum in the database
            Dim query As String
            Dim Pars As New List(Of String)
            Dim Dates As New List(Of DateTime)
            Pars.Add("@DATE")
            Dates.Add(EventDate)
            query = "INSERT INTO EVENTS (LOCATIONID, DATEANDTIME, PARAMETER, DATAVALUE, EVENTNUM, EVENTSUM, DURATION) VALUES ('" & myLocation & "',@DATE,'" & PartOfParName & "'," & EventMax & "," & EventNum & "," & EventSum & "," & nTimesteps & ");"
            Me.Setup.GeneralFunctions.SQLiteDatesNoQuery(Me.Setup.SqliteCon, query, Pars, Dates, False)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function WaterlevelsToVolumes(ByRef SfPath As String, ByVal IDField As String) As Boolean
        'This function converts waterlevels to their corresponding volumes by:
        '1. finding the underlying area from a shapefile
        '2. looking up the volumes table for that area in the database
        '3. interpolating the given waterlevel in that table and returning the result
        Try
            Dim X As Double, Y As Double
            Dim ShapeIdx As Integer, AreaID As String
            Dim query As String, dt As DataTable
            Dim myVol As Double

            'set the shapefile
            Dim mySf As New clsShapeFile(Me.Setup)
            mySf.Path = SfPath
            mySf.Open()
            Dim IDFieldIdx As Integer = mySf.GetFieldIdx(IDField)
            mySf.sf.BeginPointInShapefile()

            'create a new table for the volumes
            Setup.GeneralFunctions.SQLiteDropTable(Setup.SqliteCon, "VOLUMES")
            Setup.GeneralFunctions.SQLiteCreateTable(Setup.SqliteCon, "VOLUMES")
            Setup.GeneralFunctions.SQLiteCreateColumn(Setup.SqliteCon, "VOLUMES", "OBJECTID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "OBJECTID")
            Setup.GeneralFunctions.SQLiteCreateColumn(Setup.SqliteCon, "VOLUMES", "EVENTDATE", GeneralFunctions.enmSQLiteDataType.SQLITETEXT) 'v1.72: in SQLite dates are stored as text
            Setup.GeneralFunctions.SQLiteCreateColumn(Setup.SqliteCon, "VOLUMES", "MAXVAL", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)

            'note: we assume that the class instance ExtremeValuesStatLocations has already been populated from the database
            'so we can resume converting the already present values
            'assign the corresponding X and Y coordinates from SOBEK
            For Each myExtreme As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                If Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.GetNodeXY(myExtreme.ID, X, Y) Then
                    ShapeIdx = mySf.sf.PointInShapefile(X, Y)
                    AreaID = mySf.sf.CellValue(IDFieldIdx, ShapeIdx)

                    'now that we have our area ID, lookup the corresponding Volumes Table in the database
                    query = "SELECT ELEVATION, VOLUME FROM STORAGETABLES WHERE AREAID = '" & AreaID & "' ORDER BY ELEVATION;"
                    dt = New DataTable
                    Setup.GeneralFunctions.SQLiteQuery(Setup.SqLiteCon, query, dt, False)

                    If dt.Rows.Count > 0 Then
                        'convert every waterlevel for this location and write it to the database
                        For Each myMax As clsExtremeValue In myExtreme.Values.Values
                            myVol = Me.Setup.GeneralFunctions.InterpolateFromDataTable(dt, myMax.Value, 0, 1)
                            query = "INSERT INTO VOLUMES (OBJECTID, EVENTDATE, MAXVAL) VALUES ('" & myExtreme.ID.Trim & "','" & String.Format(myMax.EventDate, "yyyy/MM/dd hh:mm:ss") & "'," & myVol & ");"
                            Me.Setup.GeneralFunctions.SQLiteNoQuery(Setup.SqLiteCon, query, False)
                        Next
                    Else
                        Me.Setup.Log.AddError("No storage table in the database for area " & AreaID)
                    End If
                Else
                    Me.Setup.Log.AddWarning("Coordinates not found for node: " & myExtreme.ID)
                End If
            Next

            mySf.sf.EndPointInShapefile()
            mySf.Close()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        Finally

        End Try

    End Function

    Public Sub setLangbein(myLangbein As Boolean, DistributionType As GeneralFunctions.EnmProbabilityDistribution)
        Select Case DistributionType
            Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                Langbein = myLangbein
            Case Else
                'distribution functions based on POT-values never use langbein
                Langbein = False
        End Select
    End Sub

    Public Sub SetCensoring(NoCensor As Boolean, nHighest As Boolean, TargetLevel As Boolean, AutoCensor As Boolean, Optional ByVal myTargetLevelMargin As Double = 0)
        If NoCensor Then
            CensoringType = GeneralFunctions.enmStatisticalCensoring.None
        ElseIf nHighest Then
            CensoringType = GeneralFunctions.enmStatisticalCensoring.HighestNEvents
        ElseIf TargetLevel Then
            CensoringType = GeneralFunctions.enmStatisticalCensoring.TargetLevel
            TargetLevelCensoringMargin = myTargetLevelMargin
        ElseIf AutoCensor Then
            CensoringType = GeneralFunctions.enmStatisticalCensoring.AutoDetect
        End If
    End Sub


    Public Function PopulateFromDatabase() As Boolean
        Try
            If Not ExtremeValuesStatLocations.PopulateFromDatabase("EVENTS", "LOCATIONID", LocationsList) Then Throw New Exception("Error populating statistics From database.")
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function CalcPlottingPositions() As Boolean
        'this function calculates the plotting positions for each of the maximum values associated with this location
        Dim i As Long = 0, n As Long = ExtremeValuesStatLocations.ExtremeValuesStatLocations.Count
        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Calculating plotting positions...", i, n)
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                If Not myLoc.calcPlottingPositions() Then Throw New Exception("Error calculating plotting positions for location " & myLoc.ID)
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ProbabilityFromReturnPeriod(ByRef myDist As clsProbabilityDistribution, ByVal Herh As Double, ByRef myLoc As clsExtremeValuesStatParameterLocation, ByRef p As Double) As Boolean
        Try
            Select Case myDist.DistributionType
                Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                    'calculate the exceedance probability in a random year
                    'if requested transform the return period according to Langbein. The transformed value is what will be plotted
                    p = 1 / Herh
                    If Langbein AndAlso p < 1 Then Herh = 1 / -Math.Log(1 - p)
                Case Else
                    'calculate the exceedance probability. compensate for the fact that not all events were part of the pdf
                    If myLoc.nEventsFit = 0 Then Throw New Exception("Error: could not fit data for location " & myLoc.ID)

                    Select Case CensoringType
                        Case Is = GeneralFunctions.enmStatisticalCensoring.HighestNEvents
                            p = 1 / Herh * nYearsObserved / myLoc.nEventsFit
                        Case Is = GeneralFunctions.enmStatisticalCensoring.TargetLevel
                            p = 1 / Herh * nYearsObserved / myLoc.nEventsFit
                        Case Is = GeneralFunctions.enmStatisticalCensoring.AutoDetect
                            p = 1 / Herh * nYearsObserved / myLoc.nEventsFit
                        Case Is = GeneralFunctions.enmStatisticalCensoring.None
                            p = 1 / Herh * nYearsObserved / myLoc.nEventsFit
                    End Select
            End Select
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function BootstrapDataTablesFromValues(nIterations As Integer) As Boolean
        'this function bootstraps from the original underlying values
        Dim i As Long = 0, n As Long = ExtremeValuesStatLocations.ExtremeValuesStatLocations.Count
        Dim iter As Integer

        Try
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("Bootstrapping for " & myLoc.ID, i, n, True)
                'first clear all previous bootstra results
                myLoc.BootstrapData.Clear()
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                For iter = 1 To nIterations
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", iter, nIterations)
                    If Not myLoc.BootstrapFromMaxima() Then Throw New Exception("Error building datatable for bootstrapping of location: " & myLoc.ID)
                Next
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Bootstrapping complete.", 0, n, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function BootstrapDataTablesFromPDF(nIterations As Integer) As Boolean
        'this function bootstraps from the fitted Probability Distribution Function
        Dim i As Long = 0, n As Long = ExtremeValuesStatLocations.ExtremeValuesStatLocations.Count
        Dim iter As Integer

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Building datatable...", i, n)
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                'first clear all previous bootstra results
                myLoc.BootstrapData.Clear()
                i += 1
                For iter = 1 To nIterations
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
                    If Not myLoc.BootstrapFromPDF() Then Throw New Exception("Error building datatable for bootstrapping of location: " & myLoc.ID)
                Next
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function CalcParetoExceedances() As Boolean
        Dim myPareto As MathNet.Numerics.Distributions.Pareto

        Try
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                myPareto = New MathNet.Numerics.Distributions.Pareto(1, 0.1)
            Next
        Catch ex As Exception

        End Try
    End Function

    Public Function FitConfidenceInterval() As Boolean
        Try
            Dim i As Long = 0, n As Long = ExtremeValuesStatLocations.ExtremeValuesStatLocations.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Fitting confidence interval...", i, n)
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                i += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Complete", 0, n)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function



    Public Function FitAllLocations() As Boolean
        'this function fits the given probability distribution function to the data
        Try
            'start by creating/cleaning up the database table FITRESULTS
            'rebuild the entire LOCATIONS table
            Dim query As String = ""
            Setup.GeneralFunctions.SQLiteDropTable(Me.Setup.SqliteCon, "FITRESULTS")
            Setup.GeneralFunctions.SQLiteCreateTable(Me.Setup.SqliteCon, "FITRESULTS")
            Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "FITRESULTS", "OBJECTID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "FITRESULTS", "DISTRIBUTION", GeneralFunctions.enmSQLiteDataType.SQLITETEXT)
            Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "FITRESULTS", "LOCPAR", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "FITRESULTS", "SCALEPAR", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            Setup.GeneralFunctions.SQLiteCreateColumn(Me.Setup.SqliteCon, "FITRESULTS", "SHAPEPAR", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)

            Dim i As Long = 0, n As Long = ExtremeValuesStatLocations.ExtremeValuesStatLocations.Count
            Me.Setup.GeneralFunctions.UpdateProgressBar("Fitting probability distribution for " & n & " locations...", i, n, True)
            For Each myLoc As clsExtremeValuesStatParameterLocation In ExtremeValuesStatLocations.ExtremeValuesStatLocations.Values
                i += 1

                'set the distribution type for the current location
                myLoc.Dist.DistributionType = DistributionType

                'initialize the progress bar
                Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)

                Select Case DistributionType
                    Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                        'only fit if the results make sense
                        If myLoc.AnnualMax.Values(0).Value <> myLoc.AnnualMax.Values(myLoc.AnnualMax.Values.Count - 1).Value Then
                            myLoc.Fit(myLoc.AnnualMax, myLoc.Dist)
                        Else
                            Me.Setup.Log.AddMessage("Could not fit probability distribution for location " & myLoc.ID)
                            Continue For
                        End If
                    Case Else
                        If myLoc.Values.Values(0).Value <> myLoc.Values.Values(myLoc.Values.Values.Count - 1).Value Then
                            myLoc.Fit(myLoc.Values, myLoc.Dist)
                        Else
                            Me.Setup.Log.AddMessage("Could not fit probability distribution for location " & myLoc.ID)
                            Continue For
                        End If
                End Select

                query = "INSERT INTO FITRESULTS (OBJECTID, DISTRIBUTION, LOCPAR, SCALEPAR, SHAPEPAR) VALUES ('" & myLoc.Dist.DistributionType.ToString & "','" & myLoc.ID & "'," & myLoc.Dist.GetLocPar & "," & myLoc.Dist.GetScalePar & "," & myLoc.Dist.GetShapePar & ");"
                Me.Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqLiteCon, query, False)
            Next
            Me.Setup.GeneralFunctions.UpdateProgressBar("Done.", 0, n, True)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function



End Class

