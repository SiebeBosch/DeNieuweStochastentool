Option Explicit On
Option Strict Off

Imports STOCHLIB.General

Public Class clsExtremeValuesStatParameterLocation
    'this class describes all Extreme Values-related issues regarding ONE parameter and ONE location
    Private Setup As clsSetup
    Private Statistics As clsExtremeValuesStatistics

    Friend ID As String

    'the results of the fit
    Public FitParameters As New clsFitParameters                    'the parameters of a succesful fit

    Public Values As Dictionary(Of Double, clsExtremeValue)         'the datatable containing the original data. key = event number
    Public AnnualMax As Dictionary(Of Double, clsExtremeValue)      'the datatable containing only the annual maxima. key = year
    Public Patterns As Dictionary(Of Integer, clsPatternClasses)    'key = pattern nummer

    Public Dist As clsProbabilityDistribution '                        'contains the probability distribution that applies to this location, including all parameters and its cdf
    Public DistLower As clsProbabilityDistribution                     'lower confidence interval
    Public DistUpper As clsProbabilityDistribution                     'upper confidence interval
    Public DistLeft As clsProbabilityDistribution                      'fit results left of threshold
    Public DistRight As clsProbabilityDistribution                     'fit results right of threshold
    Public nEventsFit As Integer

    Public fitResults As Dictionary(Of Double, clsExtremeValue)         'contains a dictionary of extremes, sampled from the distribution for the results
    Public fitResultsLeft As Dictionary(Of Double, clsExtremeValue)     'contains a dictionary of extremes, sampled from the manually fit distribution LEFT of the threshold
    Public fitResultsRight As Dictionary(Of Double, clsExtremeValue)    'contains a dictionary of extremes, sampled from the manually fit distribution RIGHT of the threshold

    Public GumbelLocLeft As Double, GumbelScaleLeft As Double
    Public GumbelLocRight As Double, GumbelScaleRight As Double
    Public ThresholdValue As Double                                             'for this location the exact elevation of the threshold. To be used in e.g. Pareto

    Public BootstrapData As New List(Of Dictionary(Of Double, clsExtremeValue)) 'the datatables that contain the bootstrapped data
    Public BootstrapDistributions As New List(Of clsProbabilityDistribution)       'the probablity distribution functions fitted to the bootstrapdata
    Public BSLower As Dictionary(Of Double, clsExtremeValue)                    'the lower end confidence interval
    Public BSUpper As Dictionary(Of Double, clsExtremeValue)                    'the upper end confidence interval
    Public BSLowerFit As clsProbabilityDistribution                                        'the probability distribution lower end interval fit result
    Public BSUpperFit As clsProbabilityDistribution                                         'the probability distribution jupper end interval fit result

    Public ThresholdsFitResults As New Dictionary(Of Double, clsFitResult)          'a dictionary containing the fit results for various chosen thresholds
    Public IsUniform As Boolean = False                         'a flag that keeps track whether the distribution is exactly uniform (thus cannot be fitted)

    Public Sub New(ByRef mySetup As clsSetup, ByRef myStatistics As clsExtremeValuesStatistics, ByVal myID As String)
        Setup = mySetup
        Statistics = myStatistics
        ID = myID
        Values = New Dictionary(Of Double, clsExtremeValue)       'initialize the dictionary that contains all values per event
        AnnualMax = New Dictionary(Of Double, clsExtremeValue)     'initialize the dictionary that contains the annual maxima
        Dist = New clsProbabilityDistribution(Me.Setup)
    End Sub

    Public Function GetMinMax(ByRef myMin As Double, ByRef myMax As Double) As Boolean
        Try
            Dim myVal As Double
            myMin = Values.Values(0).Value
            myMax = myMin
            For i = 1 To Values.Count - 1
                myVal = Values.Values(i).Value
                If myVal > myMax Then myMax = myVal
                If myVal < myMin Then myMin = myVal
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function getMinMax in class clsExtremeValuesStatParameterLocation.")
            Return False
        End Try
    End Function
    Public Sub ClearValues(ClearPOTValues As Boolean, ClearAnnualMax As Boolean)
        If ClearPOTValues Then Values = New Dictionary(Of Double, clsExtremeValue)
        If ClearAnnualMax Then AnnualMax = New Dictionary(Of Double, clsExtremeValue)
    End Sub

    Public Function GetValueFromExceedanceProbability(p_exceedance As Double) As Double
        Try
            If IsUniform = False Then
                Return Dist.inverse(1 - p_exceedance)
            Else
                Return Values.Values(0).Value
            End If
        Catch ex As Exception
            Me.Setup.Log.AddError("Error computing return period for location " & ID)
            Return Nothing
        End Try
    End Function

    Public Function GetRepresentativeEvent(myValue As Double) As Integer
        'returns the number for the event that is mostly representative for the given value
        Dim minDiff As Double = 9.0E+99, curDiff As Double, RepEvent As Integer = 0
        For i = 0 To Values.Values.Count - 1
            curDiff = Math.Abs(myValue - Values.Values(i).Value)
            If curDiff < minDiff Then
                minDiff = curDiff
                RepEvent = Values.Keys(i)
            End If
        Next
        Return RepEvent
    End Function

    Public Function stepReturnPeriods(StartHerh As Double, EndHerh As Double, ByRef curHerh As Double) As Boolean
        If curHerh < 2 Then
            curHerh += 0.1
            curHerh = Math.Round(curHerh, 1)
        Else
            curHerh += 1
        End If
        If curHerh >= EndHerh Then Return True Else Return False
    End Function

    Public Function BuildDataCollectionFromThresholdAnalysis(ReturnPeriod As Double) As Dictionary(Of Double, clsExtremeValue)
        Dim HerhCorr As Double = ReturnPeriod
        Dim p As Double

        Try
            Dim FitResults = New Dictionary(Of Double, clsExtremeValue)
            'walk through the fit results for each given threshold value and plot the required return period
            For Each myFit As clsFitResult In ThresholdsFitResults.Values
                Select Case myFit.dist.DistributionType
                    Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                        'threshold analysis is not applicable to these types of distributions
                    Case Else
                        'calculate the exceedance probability. compensate for the fact that not all events were part of the pdf
                        p = 1 / HerhCorr * Statistics.nYearsObserved / nEventsFit
                        FitResults.Add(myFit.Threshold, New clsExtremeValue(HerhCorr, myFit.dist.inverse(1 - p)))
                End Select
            Next
            Return FitResults
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function GetValueForReturnPeriod(ByRef myDist As clsProbabilityDistribution, ReturnPeriod As Double, nEventsFit As Integer) As clsExtremeValue
        'this function retrieves the exceedance value for a given probability distribution and return period
        Try
            Dim HerhCorr As Double, p As Double
            HerhCorr = ReturnPeriod 'the corrected one includes e.g. langbein transformation, but we won't have that one influence our stepping through

            Select Case myDist.DistributionType
                Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                    'calculate the exceedance probability in a random year
                    'if requested transform the return period according to Langbein. The transformed value is what will be plotted
                    p = 1 / HerhCorr
                    If Statistics.Langbein AndAlso p < 1 Then HerhCorr = 1 / -Math.Log(1 - p)
                Case Else
                    'calculate the exceedance probability. compensate for the fact that not all events were part of the pdf
                    p = 1 / HerhCorr * Statistics.nYearsObserved / nEventsFit
            End Select

            If p < 1 Then   'safety measure agains return periods more frequent than the base of the probability distribution
                Return New clsExtremeValue(HerhCorr, myDist.inverse(1 - p))
            Else
                Return Nothing
            End If

            Throw New Exception("Error in function GetValueForReturnPeriod of class clsExtremeValuesStatLocation.")
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function

    Public Sub BuildAllDatacollectionsFromDistributions()
        fitResults = BuildDataCollectionFromDistribution(Dist)

        'if need be, also build a collection for the manual fit by plotting positions
        'BuildDataCollectionFromManualFit()

    End Sub

    Public Function BuildDataCollectionFromDistribution(ByRef myDist As clsProbabilityDistribution) As Dictionary(Of Double, clsExtremeValue)
        'this function builds a dictionary of values that obey a given probability distribution function
        Dim p As Double
        Dim Warned As Boolean = False

        Dim CurrentHerh As Double = 0
        Dim HerhCorr As Double = 0 'langbein and for POT analyses to compensate for the not-fitted part
        Dim Done As Boolean = False

        Try
            Dim FitResults = New Dictionary(Of Double, clsExtremeValue)
            'note: the chosen return period can never be more frequent than the basis of the probability distribution itself
            'e.g. a return period of 10 years cannot be retrieved when only the 10 extremes were extracted from 109 years
            'the tricky part is that we want return periods < 1 and also > 1. So figure out a way to do so

            While Not Done
                'calculation of the value is done via the untransformed return period
                Done = stepReturnPeriods(0.1, 200, CurrentHerh)
                If myDist.IsValid AndAlso Statistics.ProbabilityFromReturnPeriod(myDist, CurrentHerh, Me, p) Then
                    'transform the return period using langbein, if required
                    HerhCorr = CurrentHerh 'the corrected one includes e.g. langbein transformation, but we won't have that one influence our stepping through
                    If Statistics.Langbein AndAlso p < 1 Then HerhCorr = 1 / -Math.Log(1 - p)

                    If p <= 0 Then
                        Me.Setup.Log.AddWarning("No result could be computed for some return periods (e.g. " & CurrentHerh & ") at location " & ID)
                        Warned = True
                    ElseIf p < 1 Then   'safety measure agains return periods more frequent than the base of the probability distribution
                        'FitResults.Add(Herh, New clsExtremeValue(New Date(1900 + Herh, 1, 1), dist.Inverse(1 - p)))
                        FitResults.Add(HerhCorr, New clsExtremeValue(HerhCorr, myDist.inverse(1 - p)))
                    ElseIf Warned = False Then
                        Me.Setup.Log.AddWarning("No result could be computed for some return periods (e.g. " & CurrentHerh & ") at location " & ID)
                        Warned = True
                    End If
                End If


            End While

            Return FitResults
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function


    Public Function BuildDataCollectionFromManualFit() As Boolean
        'this function builds a dictionary of values that obey a given probability distribution function
        Dim p As Double
        Dim Warned As Boolean = False

        Dim CurrentHerh As Double = 0
        Dim HerhCorr As Double = 0 'langbein and for POT analyses to compensate for the not-fitted part
        Dim Done As Boolean = False

        Try
            'note: the chosen return period can never be more frequent than the basis of the probability distribution itself
            'e.g. a return period of 10 years cannot be retrieved when only the 10 extremes were extracted from 109 years
            'the tricky part is that we want return periods < 1 and also > 1. So figure out a way to do so
            fitResultsLeft = New Dictionary(Of Double, clsExtremeValue)
            fitResultsRight = New Dictionary(Of Double, clsExtremeValue)

            While Not Done
                Done = stepReturnPeriods(0.1, 200, CurrentHerh)
                HerhCorr = CurrentHerh 'the corrected one includes e.g. langbein transformation, but we won't have that one influence our stepping through
                Select Case Dist.DistributionType
                    Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                        'calculate the exceedance probability in a random year
                        'if requested transform the return period according to Langbein. The transformed value is what will be plotted
                        p = 1 / HerhCorr
                        If Statistics.Langbein AndAlso p < 1 Then HerhCorr = 1 / -Math.Log(1 - p)
                    Case Else
                        Throw New Exception("Distribution not supported for manual fit.")
                End Select

                If p < 1 Then   'safety measure agains return periods more frequent than the base of the probability distribution
                    'FitResults.Add(Herh, New clsExtremeValue(New Date(1900 + Herh, 1, 1), dist.Inverse(1 - p)))
                    fitResultsRight.Add(HerhCorr, New clsExtremeValue(HerhCorr, Setup.GeneralFunctions.GumbelInverse(1 - p, GumbelLocRight, GumbelScaleRight)))
                    fitResultsLeft.Add(HerhCorr, New clsExtremeValue(HerhCorr, Setup.GeneralFunctions.GumbelInverse(1 - p, GumbelLocLeft, GumbelScaleLeft)))
                ElseIf Warned = False Then
                    Me.Setup.Log.AddWarning("No result could be computed for some return periods (e.g. " & CurrentHerh & ") at location " & ID)
                    Warned = True
                End If

            End While

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ReturnPeriodFromExceedanceProbability(ByRef myDist As GeneralFunctions.EnmProbabilityDistribution, p As Double) As Double
        Select Case myDist
            Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                'if requested transform the return period according to Langbein. The transformed value is what will be plotted
                If Statistics.Langbein AndAlso p < 1 Then
                    Return 1 / -Math.Log(1 - p)
                Else
                    Return 1 / p
                End If
            Case Else
                'calculate the exceedance probability. compensate for the fact that not all events were part of the pdf
                Select Case Statistics.CensoringType
                    Case Is = GeneralFunctions.enmStatisticalCensoring.HighestNEvents
                        Return 1 / p * nEventsFit / Statistics.nYearsObserved
                    Case Is = GeneralFunctions.enmStatisticalCensoring.AutoDetect
                        Throw New Exception("Error: data left censoring type not yet supported: AutoDetect")
                    Case Is = GeneralFunctions.enmStatisticalCensoring.None
                        Return 1 / p * Values.Count / Statistics.nYearsObserved
                End Select
        End Select
    End Function


    Public Function FitByPlottingPositions(ByRef myMaxima As Dictionary(Of Double, clsExtremeValue), myDistType As GeneralFunctions.EnmProbabilityDistribution) As clsProbabilityDistribution

        'start by determining the mean and standard deviation for our dataset
        Dim myValues(0 To myMaxima.Count - 1) As Double
        For i = 0 To myMaxima.Count - 1
            myValues(i) = myMaxima.Values(i).Value
        Next

        Dim StDev As Double = Setup.GeneralFunctions.StandardDeviation(myValues)
        Dim Avg As Double = Setup.GeneralFunctions.Average(myValues)

        Dim newDist As New clsProbabilityDistribution(Me.Setup)
        newDist.DistributionType = myDistType
        Return newDist
    End Function

    Public Function Fit(ByRef myMaxima As Dictionary(Of Double, clsExtremeValue), ByRef myDist As clsProbabilityDistribution) As Boolean
        Try
            Dim data(myMaxima.Count - 1) As Double, i As Integer

            'sort the dictionary that contains the annual maxima in descending order
            Dim sorted = From pair In myMaxima Order By pair.Value.Value Descending
            Dim sortedDictionary = sorted.ToDictionary(Function(p) p.Key, Function(p) p.Value)

            'in case of auto censoring option we'll have to start by finding the optimal threshold
            'this is done by fitting two Gumbel functions on the plotting positions
            If Statistics.CensoringType = GeneralFunctions.enmStatisticalCensoring.AutoDetect Then
                If Not ThresholdDetection(sortedDictionary) Then Throw New Exception("Error detecting threshold value for fitting location: " & ID)
            End If

            'now move the data into an array apply any censoring if applicable already
            If Statistics.CensoringType = GeneralFunctions.enmStatisticalCensoring.None Then

                'create an array with the values to fit, sorted in descending order
                nEventsFit = sortedDictionary.Count
                ReDim data(0 To sortedDictionary.Count - 1)
                For i = 0 To sortedDictionary.Count - 1
                    data(i) = sortedDictionary.Values(i).Value
                Next

                '-----------------------------------------------------------------------------------------
                ' THIS BLOCK Is APPLIED FOR FITTING USING OUR OWN MAXIMUM LIKELIHOOD FUNCTIONS (Not RECOMMENDED)
                '-----------------------------------------------------------------------------------------
                Select Case myDist.DistributionType
                    Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax
                        'this is the only distribution type we will fit by using our own routine
                        If Not FitGumbelMLE(data) Then Throw New Exception("Error fitting dataset.")
                        myDist.SetScalePar(FitParameters.getScale)
                        myDist.SetLocPar(FitParameters.getLoc)
                    Case Is = GeneralFunctions.EnmProbabilityDistribution.GEV
                        If Not FitGEVMLE(data) Then Throw New Exception("Error fitting dataset.")
                        myDist.SetLocPar(FitParameters.getLoc)
                        myDist.SetScalePar(FitParameters.getScale)
                        myDist.SetShapePar(FitParameters.getShape)
                    Case Else
                        Throw New Exception("Error: probability distribution is not yet supported.")
                End Select


            ElseIf Statistics.CensoringType = GeneralFunctions.enmStatisticalCensoring.HighestNEvents Then

                'create an array with the values to fit, sorted in descending order
                nEventsFit = Statistics.nEventsFit
                ReDim data(0 To nEventsFit - 1)
                For i = 0 To nEventsFit - 1
                    data(i) = sortedDictionary.Values(i).Value
                Next

                'set the threshold value
                ThresholdValue = data(nEventsFit - 1)

                '-----------------------------------------------------------------------------------------
                '   THIS BLOCK IS APPLIED FOR FITTING USING OUR OWN MAXIMUM LIKELIHOOD FUNCTIONS
                '-----------------------------------------------------------------------------------------
                Select Case myDist.DistributionType
                    Case Is = GeneralFunctions.EnmProbabilityDistribution.GenPareto
                        FitDatasetMLE(data, myDist)
                    Case Is = GeneralFunctions.EnmProbabilityDistribution.Exponential
                        FitDatasetMLE(data, myDist)
                    Case Else
                        Throw New Exception("Error: probability distribution type not yet supported for this subselection of datapoints.")
                End Select
                '--------------------------------------------------------------------------------------------------------------

            ElseIf Statistics.CensoringType = GeneralFunctions.enmStatisticalCensoring.TargetLevel Then
                Dim targetLevel As Double = GetTargetLevelFromDatabase()
                If targetLevel = Nothing Then
                    ReDim data(0 To sortedDictionary.Count - 1)
                    For i = 0 To sortedDictionary.Count - 1
                        data(i) = sortedDictionary.Values(i).Value
                    Next
                Else
                    'convert the threshold value into its equivalent volume by using the storage table
                    ThresholdValue = targetLevel + Statistics.TargetLevelCensoringMargin / 100

                    ReDim data(0 To sortedDictionary.Count - 1)
                    For i = 0 To sortedDictionary.Count - 1
                        If sortedDictionary.Values(i).Value >= ThresholdValue Then
                            data(i) = sortedDictionary.Values(i).Value
                        Else
                            ReDim Preserve data(0 To i - 1)
                            Exit For
                        End If
                    Next
                    nEventsFit = data.Count
                End If
                Throw New Exception("Error: probability distribution type not yet supported for this subselection of datapoints.")
            ElseIf Statistics.CensoringType = GeneralFunctions.enmStatisticalCensoring.AutoDetect Then
                ReDim data(0 To sortedDictionary.Count - 1)
                For i = 0 To sortedDictionary.Count - 1
                    If sortedDictionary.Values(i).Value >= ThresholdValue Then
                        data(i) = sortedDictionary.Values(i).Value
                    Else
                        ReDim Preserve data(0 To i - 1)
                        nEventsFit = data.Count
                        Exit For
                    End If
                Next

                If FitDatasetMLE(data, myDist) Then
                    Me.Setup.Log.AddMessage("Fitting distribution successful for " & ID & ", using MLE function on " & data.Count & " datapoints.")
                Else
                    Me.Setup.Log.AddError("Fitting distribution unsuccessful for " & ID & ", using " & data.Count & " datapoints.")
                End If

            End If

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function WaterLevelToVolume(Waterlevel As Double, ByRef Volume As Double) As Boolean
        'this function converts a given waterlevel to its equivalent volume, by using the storage table
        'first look up the area that represents our current location
        Try
            Dim AreaID As String = ""
            Dim query As String, lt As New DataTable

            If Not GetAreaIDForLocation(AreaID) Then Throw New Exception("Error retrieving area ID for location " & ID)

            'now fill a datatable with all locations
            query = "SELECT ELEVATION, VOLUME FROM STORAGETABLES WHERE AREAID='" & AreaID & "' ORDER BY ELEVATION;"
            Setup.GeneralFunctions.SQLiteQuery(Setup.SqLiteCon, query, lt)

            'read the target level
            If lt.Rows.Count > 0 Then
                For i = 0 To lt.Rows.Count - 2
                    If lt.Rows(i + 1)(0) >= Waterlevel Then
                        Volume = Setup.GeneralFunctions.Interpolate(lt.Rows(i)(0), lt.Rows(i)(1), lt.Rows(i + 1)(0), lt.Rows(i + 1)(1), Waterlevel, False)
                        Return True
                    End If
                Next
            Else
                Throw New Exception("No Area ID found in database for location " & ID)
            End If
            Return True

            Return False
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return True
        Finally
            Setup.SqLiteCon.Close()
        End Try

    End Function

    Public Function VolumeToWaterlevelDictionary(ByRef myDict As Dictionary(Of Double, clsExtremeValue)) As Dictionary(Of Double, clsExtremeValue)
        Try
            Dim newDict As New Dictionary(Of Double, clsExtremeValue)
            Dim newExtreme As clsExtremeValue
            Dim AreaID As String = "", query As String, st As New DataTable

            'get the area id for the current location
            If Not GetAreaIDForLocation(AreaID) Then Throw New Exception("Error retrieving area ID for location " & ID)

            'now fill a datatable with all locations
            query = "SELECT VOLUME, ELEVATION FROM STORAGETABLES WHERE AREAID='" & AreaID & "' ORDER BY ELEVATION;"
            Setup.GeneralFunctions.SQLiteQuery(Setup.SqLiteCon, query, st)

            Dim myWL As Double
            For i = 0 To myDict.Values.Count - 1
                myWL = Me.Setup.GeneralFunctions.InterpolateFromDataTable(st, myDict.Values(i).Value, 0, 1)
                newExtreme = New clsExtremeValue(myDict.Values(i).EventDate, myWL)
                newExtreme.ReturnPeriod = myDict.Values(i).ReturnPeriod
                newExtreme.PPBosLevenbach = myDict.Values(i).PPBosLevenbach
                newExtreme.PPGringorten = myDict.Values(i).PPGringorten
                newExtreme.PPWeibull = myDict.Values(i).PPWeibull
                newDict.Add(myDict.Keys(i), newExtreme)
            Next
            Return newDict
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return Nothing
        End Try

    End Function


    Public Function VolumeToWaterlevel(Volume As Double, ByRef WaterLevel As Double) As Boolean
        'this function converts a given volume to its equivalent waterlevel, by using the storage table
        'first look up the area that represents our current location
        Try
            Dim AreaID As String = ""
            Dim query As String, lt As New DataTable

            If Not GetAreaIDForLocation(AreaID) Then Throw New Exception("Error retrieving area ID for location " & ID)

            'now fill a datatable with all locations
            query = "SELECT VOLUME, ELEVATION FROM STORAGETABLES WHERE AREAID='" & AreaID & "' ORDER BY ELEVATION;"
            Setup.GeneralFunctions.SQLiteQuery(Setup.SqLiteCon, query, lt)

            WaterLevel = Setup.GeneralFunctions.InterpolateFromDataTable(lt, Volume, 0, 1)
            Return True

        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return True
        Finally
            Setup.SqLiteCon.Close()
        End Try

    End Function

    Public Function GetAreaIDForLocation(ByRef AreaID As String) As Boolean
        Try
            Dim query As String, lt As New DataTable
            Setup.SqLiteCon.Open()

            'now fill a datatable with all locations
            query = "SELECT AREAID FROM LOCATIONS WHERE OBJECTID='" & ID & "';"
            Setup.GeneralFunctions.SQLiteQuery(Setup.SqLiteCon, query, lt)

            'read the target level
            If lt.Rows.Count > 0 Then
                AreaID = lt.Rows(0)(0)
            Else
                Throw New Exception("No Area ID found in database for location " & ID)
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function GetTargetLevelFromDatabase() As Double
        Try
            Dim query As String, lt As New DataTable
            Setup.SqLiteCon.Open()

            'now fill a datatable with all locations
            query = "SELECT TARGETLEVEL FROM TARGETLEVELS WHERE OBJECTID='" & ID & "';"
            Setup.GeneralFunctions.SQLiteQuery(Setup.SqLiteCon, query, lt)

            'read the target level
            If lt.Rows.Count > 0 Then
                Return lt.Rows(0)(0)
            Else
                Throw New Exception
            End If

        Catch ex As Exception
            Me.Setup.Log.AddError("Error: no target level known for location " & ID & ". No threshold was applied for this location.")
            Return Nothing
        Finally
            Setup.SqLiteCon.Close()
        End Try

    End Function

    Public Function ThresholdDetection(ByRef SortedDictionary As Dictionary(Of Double, clsExtremeValue)) As Boolean
        'this function fits the dataset provided by its plotting positions. We use GUMBEL to do so
        'notice that this is not the standard scientifically approved method but it seems to be the only practical way discontinuities in the dataset can be accounted for
        Try
            Dim RMSEUpper As Double, RMSEUpperBest As Double = 9000000000.0, muUpper As Double, sigmaUpper As Double
            Dim RMSELower As Double, RMSELowerBest As Double = 9000000000.0, muLower As Double, sigmaLower As Double
            Dim RMSECombi As Double, RMSEBest As Double = 9000000000.0, IdxBest As Integer
            Dim ReturnPeriods As List(Of Double)
            Dim Values As List(Of Double)

            'we set the minimum number of datapoints to 10
            For ThresholdIdx = 10 To SortedDictionary.Values.Count - 10

                ReturnPeriods = New List(Of Double)
                Values = New List(Of Double)
                For i = 0 To ThresholdIdx
                    ReturnPeriods.Add(SortedDictionary.Values(i).PPWeibull)
                    Values.Add(SortedDictionary.Values(i).Value)
                Next
                If Not Setup.GeneralFunctions.FitGumbelByPP(ReturnPeriods, Values, Statistics.Langbein, muUpper, sigmaUpper, RMSEUpper) Then Continue For

                ReturnPeriods = New List(Of Double)
                Values = New List(Of Double)
                For i = ThresholdIdx To SortedDictionary.Count - 1
                    ReturnPeriods.Add(SortedDictionary.Values(i).PPWeibull)
                    Values.Add(SortedDictionary.Values(i).Value)
                Next
                If Not Setup.GeneralFunctions.FitGumbelByPP(ReturnPeriods, Values, Statistics.Langbein, muLower, sigmaLower, RMSELower) Then Continue For
                RMSECombi = RMSEUpper + RMSELower 'we 'punish' the upper fit twice as hard since it is of higher importance
                If RMSECombi < RMSEBest Then
                    RMSEBest = RMSECombi
                    RMSELowerBest = RMSELower
                    RMSEUpperBest = RMSEUpper
                    IdxBest = ThresholdIdx
                    GumbelLocLeft = muLower
                    GumbelScaleLeft = sigmaLower
                    GumbelLocRight = muUpper
                    GumbelScaleRight = sigmaUpper
                End If
            Next

            'set the optimal threshold value for this location and the number of waterlevels fit
            ThresholdValue = SortedDictionary.Values(IdxBest).Value
            nEventsFit = IdxBest + 1

            'set the distributions for this location
            DistLeft = New clsProbabilityDistribution(Me.Setup)
            DistRight = New clsProbabilityDistribution(Me.Setup)
            DistLeft.DistributionType = GeneralFunctions.EnmProbabilityDistribution.GumbelMax
            DistLeft.SetTitle("Gumbel")
            DistLeft.SetLocPar(GumbelLocLeft)
            DistLeft.SetScalePar(GumbelScaleLeft)
            DistRight.DistributionType = GeneralFunctions.EnmProbabilityDistribution.GumbelMax
            DistRight.SetTitle("Gumbel")
            DistRight.SetLocPar(GumbelLocRight)
            DistRight.SetScalePar(GumbelScaleRight)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function FitGenParetoMLE(ByVal data As Double()) As Boolean
        'this routine uses Maximum Likelihood Estimation to fit a Generalized Pareto probability distribution to the given dataset
        'it returns the outcome by writing it to the MathWave distribution instance (temporary solution)
        Try
            Dim locMax As Double, locMin As Double, scaleMax As Double, scaleMin As Double, shapeMax As Double, shapeMin As Double
            Dim iLoc As Integer, iScale As Integer, iShape As Integer, Loc As Double, Scale As Double, Shape As Double
            Dim LogLikelihood As Double, bestLogLikelihood As Double, BestLocIdx As Integer, bestScaleIdx As Integer, bestShapeIdx As Integer
            Dim bestLoc As Double, bestScale As Double, bestShape As Double, iIter As Integer
            Dim AIC As Double, iterationCriterion As Double = 0.00001

            'initialize mu and sigma. Notice that, in case of Gen.Pareto the location parameter is always lower than X!
            locMax = data.Min
            locMin = data.Min - (data.Max - data.Min)
            scaleMax = locMax - locMin
            scaleMin = 0
            shapeMin = -1
            shapeMax = 1
            bestLogLikelihood = -9000000000.0

            For iIter = 1 To Me.Setup.ExtremeValuesStatistics.MaxIterations
                For iLoc = 1 To 10
                    Loc = locMin + (locMax - locMin) / 10 * (iLoc - 0.5) 'take the centerpoint from the current section
                    For iScale = 1 To 10
                        Scale = scaleMin + (scaleMax - scaleMin) / 10 * (iScale - 0.5) 'take the centerpoint from the current selection
                        For iShape = 1 To 10
                            Shape = shapeMin + (shapeMax - shapeMin) / 10 * (iShape - 0.5) 'take the centerpoint from the current selection

                            'for testing purposes
                            'Loc = -1.0748
                            'Scale = 0.24401
                            'Shape = -0.02879


                            'make sure the values remain inside the domain
                            'Loc <x <inf for shape >= 0
                            'loc <= x <= loc - scale/shape 'for shape < 0
                            'see http://www.mathwave.com/help/easyfit/html/analyses/distributions/gen_pareto.html
                            If Loc > data.Min Then Continue For
                            If Scale <= 0 Then Continue For
                            If Shape < 0 AndAlso Not (Loc <= data.Min AndAlso data.Max <= Loc - Scale / Shape) Then Continue For

                            If GenParetoLogLikelihood(data, Loc, Scale, Shape, LogLikelihood) Then
                                If LogLikelihood > bestLogLikelihood Then
                                    BestLocIdx = iLoc
                                    bestScaleIdx = iScale
                                    bestShapeIdx = iShape
                                    bestLoc = Loc
                                    bestScale = Scale
                                    bestShape = Shape
                                    bestLogLikelihood = LogLikelihood
                                End If
                            End If
                        Next
                    Next
                Next

                'check if the iteration criterion has been met
                If Math.Abs(locMin - locMax) < iterationCriterion AndAlso Math.Abs(scaleMin - scaleMax) < iterationCriterion AndAlso Math.Abs(shapeMin - shapeMax) < iterationCriterion Then
                    Exit For
                End If

                'another iteration complete. Narrow down based on the best result
                locMin = locMin + (locMax - locMin) / 10 * (BestLocIdx - 1.5)
                locMax = locMin + (locMax - locMin) / 10 * (BestLocIdx + 1.5)
                scaleMin = scaleMin + (scaleMax - scaleMin) / 10 * (bestScaleIdx - 1.5)
                scaleMax = scaleMin + (scaleMax - scaleMin) / 10 * (bestScaleIdx + 1.5)
                shapeMin = shapeMin + (shapeMax - shapeMin) / 10 * (bestShapeIdx - 1.5)
                shapeMax = shapeMin + (shapeMax - shapeMin) / 10 * (bestShapeIdx + 1.5)
            Next

            'for testing purposes
            'bestLoc = -1.0748
            'bestScale = 0.24401
            'bestShape = -0.02879
            GenParetoLogLikelihood(data, bestLoc, bestScale, bestShape, LogLikelihood)
            'GenParetoLogLikelihood(data, Loc, Scale, Shape, LogLikelihood)



            'store the computed values in the FitParameters class
            FitParameters.setLoc(bestLoc)
            FitParameters.setScale(bestScale)
            FitParameters.setShape(bestShape)
            FitParameters.setMLE(bestLogLikelihood)
            If Setup.GeneralFunctions.Akaike(bestLogLikelihood, 3, AIC) Then
                FitParameters.setAIC(bestLogLikelihood)
            End If

            FitParameters.setKS(GenParetoKolmorgorovSmirnov(data, bestLoc, bestScale, bestShape))

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FitGumbelMLE.")
            Return False
        End Try

    End Function

    Public Function FitExptoMLE(ByVal data As Double(), threshold As Double) As Boolean
        'this routine uses Maximum Likelihood Estimation to fit an Exponential probability distribution to the given dataset
        'it returns the outcome by writing it to the MathWave distribution instance (temporary solution)
        Try
            Dim locMax As Double, locMin As Double, scaleMax As Double, scaleMin As Double
            Dim iLoc As Integer, iScale As Integer, Loc As Double, Scale As Double
            Dim LogLikelihood As Double, bestLogLikelihood As Double, BestLocIdx As Integer, bestScaleIdx As Integer
            Dim bestLoc As Double, bestScale As Double, iIter As Integer
            Dim AIC As Double, iterationCriterion As Double = 0.00001

            'initialize the pdf's parameters
            locMax = data.Min 'this is a prerequisite. The location parameter cannot exceed any of the values
            locMin = data.Min - (data.Max - data.Min)
            scaleMax = (locMax - locMin) * 10
            scaleMin = 0
            bestLogLikelihood = -9000000000.0

            For iIter = 1 To Me.Setup.ExtremeValuesStatistics.MaxIterations
                For iLoc = 1 To 10
                    Loc = locMin + (locMax - locMin) / 10 * (iLoc - 0.5) 'take the centerpoint from the current section
                    For iScale = 1 To 10
                        Scale = scaleMin + (scaleMax - scaleMin) / 10 * (iScale - 0.5) 'take the centerpoint from the current selection

                        'make sure the values remain inside the domain
                        'Loc <x <inf for shape >= 0
                        'loc <= x <= loc - scale/shape 'for shape < 0
                        'see http://www.mathwave.com/help/easyfit/html/analyses/distributions/gen_pareto.html
                        If Loc > data.Min Then Continue For
                        If ExponentialLogLikelihood(data, Loc, Scale, LogLikelihood) Then
                            If LogLikelihood > bestLogLikelihood Then
                                BestLocIdx = iLoc
                                bestScaleIdx = iScale
                                bestLoc = Loc
                                bestScale = Scale
                                bestLogLikelihood = LogLikelihood
                            End If
                        End If
                    Next
                Next

                'check if the iteration criterion has been met
                If Math.Abs(locMin - locMax) < iterationCriterion AndAlso Math.Abs(scaleMin - scaleMax) < iterationCriterion Then
                    Exit For
                End If

                'another iteration complete. Narrow down based on the best result
                locMin = locMin + (locMax - locMin) / 10 * (BestLocIdx - 1.5)
                locMax = locMin + (locMax - locMin) / 10 * (BestLocIdx + 1.5)
                scaleMin = scaleMin + (scaleMax - scaleMin) / 10 * (bestScaleIdx - 1.5)
                scaleMax = scaleMin + (scaleMax - scaleMin) / 10 * (bestScaleIdx + 1.5)
            Next

            'store the computed values in the FitParameters class
            FitParameters.setLoc(bestLoc)
            FitParameters.setScale(bestScale)
            FitParameters.setMLE(bestLogLikelihood)
            If Setup.GeneralFunctions.Akaike(bestLogLikelihood, 3, AIC) Then
                FitParameters.setAIC(bestLogLikelihood)
            End If
            FitParameters.setKS(ExponentialKolmorgorovSmirnov(data, bestLoc, bestScale))

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FitGumbelMLE.")
            Return False
        End Try

    End Function


    Public Function FitGumbelMLE(ByVal data As Double()) As Boolean
        'this routine uses Maximum Likelihood Estimation to fit a Gumbel probability distribution to the given dataset
        'it returns the outcome by writing it to the MathWave distribution instance (temporary solution)
        'this function fits a gumbel distribution by finding the maximum likelihood
        'it sets the cell value for mu, sigma and the maximum likelihood
        Try
            Dim muMax As Double, muMin As Double, sMax As Double, sMin As Double
            Dim iMu As Integer, iSigma As Integer, mu As Double, sigma As Double
            Dim Likelihood As Double, bestLikelihood As Double, BestMuIdx As Integer, bestSigmaIdx As Integer
            Dim bestMu As Double, bestSigma As Double, iIter As Integer
            Dim AIC As Double, iterationCriterion As Double = 0.00001

            'initialize mu and sigma
            muMax = data.Max
            muMin = data.Min
            sMax = muMax - muMin
            sMin = 0
            bestLikelihood = -9000000000.0

            For iIter = 1 To 50
                For iMu = 1 To 10
                    mu = muMin + (muMax - muMin) / 10 * (iMu - 0.5) 'take the centerpoint from the current section
                    For iSigma = 1 To 10
                        sigma = sMin + (sMax - sMin) / 10 * (iSigma - 0.5) 'take the centerpoint from the current selection
                        If GumbelLogLikelihood(data, mu, sigma, Likelihood) Then
                            If Likelihood > bestLikelihood Then
                                BestMuIdx = iMu
                                bestSigmaIdx = iSigma
                                bestMu = mu
                                bestSigma = sigma
                                bestLikelihood = Likelihood
                            End If
                        End If
                    Next
                Next

                'check if the iteration criterion has been met
                If Math.Abs(muMin - muMax) < iterationCriterion AndAlso Math.Abs(sMin - sMax) < iterationCriterion Then
                    Exit For
                End If

                'another iteration complete. Narrow down based on the best result
                muMin = muMin + (muMax - muMin) / 10 * (BestMuIdx - 1.5)
                muMax = muMin + (muMax - muMin) / 10 * (BestMuIdx + 1.5)
                sMin = sMin + (sMax - sMin) / 10 * (bestSigmaIdx - 1.5)
                sMax = sMin + (sMax - sMin) / 10 * (bestSigmaIdx + 1.5)
            Next

            'store the computed values in the FitParameters class
            FitParameters.setLoc(bestMu)
            FitParameters.setScale(bestSigma)
            FitParameters.setMLE(bestLikelihood)
            If Setup.GeneralFunctions.Akaike(bestLikelihood, 2, AIC) Then FitParameters.setAIC(AIC) 'calculate and set Akaike
            FitParameters.setKS(GumbelKolmorgorovSmirnov(data, bestMu, bestSigma))                  'calculate and set Kolmorogov-Smirnov

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FitGumbelMLE.")
            Return False
        End Try

    End Function

    Public Function FitGEVMLE(ByVal data As Double()) As Boolean
        'this routine uses Maximum Likelihood Estimation to fit a GEV probability distribution to the given dataset
        'it returns the outcome by writing it to the MathWave distribution instance (temporary solution)
        'it sets the cell value for mu, sigma and kappa and returns the maximum likelihood
        Try
            Dim muMax As Double, muMin As Double, sMax As Double, sMin As Double, kMax As Double, kMin As Double
            Dim iMu As Integer, iSigma As Integer, iKappa As Integer, mu As Double, sigma As Double, kappa As Double
            Dim Likelihood As Double, bestLikelihood As Double, BestMuIdx As Integer, bestSigmaIdx As Integer, bestKappaIdx As Integer
            Dim bestMu As Double, bestSigma As Double, bestKappa As Double, iIter As Integer
            Dim AIC As Double, iterationCriterion As Double = 0.00001

            'initialize mu and sigma
            muMax = data.Max
            muMin = data.Min
            sMax = muMax - muMin
            sMin = 0
            kMax = 5
            kMin = -5
            bestLikelihood = -9000000000.0


            'mu = -1.0755
            'sigma = 0.0478
            'kappa = 0.54425

            For iIter = 1 To 50
                For iMu = 1 To 10
                    mu = muMin + (muMax - muMin) / 10 * (iMu - 0.5) 'take the centerpoint from the current section
                    For iSigma = 1 To 10
                        sigma = sMin + (sMax - sMin) / 10 * (iSigma - 0.5) 'take the centerpoint from the current selection
                        For iKappa = 1 To 10
                            kappa = kMin + (kMax - kMin) / 10 * (iKappa - 0.5) 'take the centerpoint from the current selection
                            If GEVLogLikelihood(data, mu, sigma, kappa, Likelihood) Then
                                If Likelihood > bestLikelihood Then
                                    BestMuIdx = iMu
                                    bestSigmaIdx = iSigma
                                    bestKappaIdx = iKappa
                                    bestMu = mu
                                    bestSigma = sigma
                                    bestKappa = kappa
                                    bestLikelihood = Likelihood
                                End If
                            End If
                        Next
                    Next
                Next

                'check if the iteration criterion has been met
                If Math.Abs(muMin - muMax) < iterationCriterion AndAlso Math.Abs(sMin - sMax) < iterationCriterion AndAlso Math.Abs(kMin - kMax) < iterationCriterion Then
                    Exit For
                End If

                'another iteration complete. Narrow down based on the best result
                muMin = muMin + (muMax - muMin) / 10 * (BestMuIdx - 1.5)
                muMax = muMin + (muMax - muMin) / 10 * (BestMuIdx + 1.5)
                sMin = sMin + (sMax - sMin) / 10 * (bestSigmaIdx - 1.5)
                sMax = sMin + (sMax - sMin) / 10 * (bestSigmaIdx + 1.5)
                kMin = kMin + (kMax - kMin) / 10 * (bestKappaIdx - 1.5)
                kMax = kMin + (kMax - kMin) / 10 * (bestKappaIdx + 1.5)
            Next

            'store the computed values in the FitParameters class
            FitParameters.setLoc(bestMu)
            FitParameters.setScale(bestSigma)
            FitParameters.setShape(bestKappa)
            FitParameters.setMLE(bestLikelihood)
            If Setup.GeneralFunctions.Akaike(bestLikelihood, 3, AIC) Then FitParameters.setAIC(AIC) 'calculate and set Akaike
            FitParameters.setKS(GEVKolmorgorovSmirnov(data, bestMu, bestSigma, bestKappa))       'calculate and set Kolmorogov-Smirnov

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function FitGEVMLE.")
            Return False
        End Try

    End Function

    Public Function GumbelKolmorgorovSmirnov(ByRef data As Double(), mu As Double, sigma As Double) As Double
        Try
            'this function computes Kolmogorov-Smirnoff goodness-of-fit based on a given dataset and a given Gumbel probability Distribution Function
            'https://en.wikipedia.org/wiki/Kolmogorov%E2%80%93Smirnov_test
            Dim i As Integer, j As Integer, Fn(data.Count - 1) As Double, F(data.Count - 1) As Double, mySum As Double
            Dim curDiff As Double, maxDiff As Double = 0

            'start by populating a dataset with both empirical and theoretical data
            'we assume that data has already been sorted in DESCENDING ORDER!
            For i = 0 To data.Count - 1
                mySum = 0
                For j = 0 To i
                    mySum += 1
                Next
                Fn(i) = 1 / data.Count * mySum
                If Not Setup.GeneralFunctions.GumbelCDF(data(i), mu, sigma, F(i)) Then Throw New Exception("Error in function GumbelKolmorogovSmirnov of class clsExtremeValuesStatLocation.")
                F(i) = 1 - F(i)
                curDiff = Math.Abs(Fn(i) - F(i))
                If curDiff > maxDiff Then maxDiff = curDiff
            Next
            Return maxDiff
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function GEVKolmorgorovSmirnov(ByRef data As Double(), mu As Double, sigma As Double, kappa As Double) As Double
        Try
            'this function computes Kolmogorov-Smirnoff goodness-of-fit based on a given dataset and a given Gumbel probability Distribution Function
            'https://en.wikipedia.org/wiki/Kolmogorov%E2%80%93Smirnov_test
            Dim i As Integer, j As Integer, Fn(data.Count - 1) As Double, F(data.Count - 1) As Double, mySum As Double
            Dim curDiff As Double, maxDiff As Double = 0

            'start by populating a dataset with both empirical and theoretical data
            'we assume that data has already been sorted in DESCENDING ORDER!
            For i = 0 To data.Count - 1
                mySum = 0
                For j = 0 To i
                    mySum += 1
                Next
                Fn(i) = 1 / data.Count * mySum
                If Not Setup.GeneralFunctions.GEVCDF(data(i), mu, sigma, kappa, F(i)) Then Throw New Exception("Error in function GEVKolmogorovSmirnov of class clsExtremeValuesStatLocation.")
                F(i) = 1 - F(i)
                curDiff = Math.Abs(Fn(i) - F(i))
                If curDiff > maxDiff Then maxDiff = curDiff
            Next
            Return maxDiff
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function GenParetoKolmorgorovSmirnov(ByRef data As Double(), loc As Double, scale As Double, shape As Double) As Double
        Try
            'this function computes Kolmogorov-Smirnoff goodness-of-fit based on a given dataset and a given generalized Pareto probability Distribution Function
            'https://en.wikipedia.org/wiki/Kolmogorov%E2%80%93Smirnov_test
            Dim i As Integer, j As Integer, Fn(data.Count - 1) As Double, F(data.Count - 1) As Double, mySum As Double
            Dim curDiff As Double, maxDiff As Double = 0

            'start by populating a dataset with both empirical and theoretical data
            'we assume that the data has been ordered in descending order
            For i = 0 To data.Count - 1
                mySum = 0
                For j = 0 To data.Count - 1
                    If data(j) <= data(i) Then mySum += 1
                Next
                Fn(i) = 1 / data.Count * mySum
                If Not Setup.GeneralFunctions.GenParetoCDF(data(i), loc, scale, shape, F(i)) Then Throw New Exception("Error in function GenParetoKolmogorovSmirnov of class clsExtremeValuesStatLocation.")
                F(i) = F(i)
                curDiff = Math.Abs(Fn(i) - F(i))
                If curDiff > maxDiff Then maxDiff = curDiff
            Next
            Return maxDiff
        Catch ex As Exception
            Return 0
        End Try

    End Function

    Public Function ExponentialKolmorgorovSmirnov(ByRef data As Double(), loc As Double, scale As Double) As Double
        Try
            'this function computes Kolmogorov-Smirnoff goodness-of-fit based on a given dataset and a given generalized Pareto probability Distribution Function
            'https://en.wikipedia.org/wiki/Kolmogorov%E2%80%93Smirnov_test
            Dim i As Integer, j As Integer, Fn(data.Count - 1) As Double, F(data.Count - 1) As Double, mySum As Double
            Dim curDiff As Double, maxDiff As Double = 0

            'start by populating a dataset with both empirical and theoretical data
            'we assume that the data has been ordered in descending order
            For i = 0 To data.Count - 1
                mySum = 0
                For j = 0 To i
                    mySum += 1
                Next
                Fn(i) = 1 / data.Count * mySum
                If Not Setup.GeneralFunctions.ExponentialCDF(data(i), loc, scale, F(i)) Then Throw New Exception("Error in function ExponentialKolmogorovSmirnov of class clsExtremeValuesStatLocation.")
                F(i) = 1 - F(i)
                curDiff = Math.Abs(Fn(i) - F(i))
                If curDiff > maxDiff Then maxDiff = curDiff
            Next
            Return maxDiff
        Catch ex As Exception
            Return 0
        End Try

    End Function

    Public Function GumbelLikelihood(xVals As Double(), mu As Double, sigma As Double) As Double
        'this function computes the log likelihood for a given dataset and Gumbel distribution
        Try
            Dim myResult As Double
            Dim p As Double
            Dim r As Integer, z As Double
            For r = 0 To xVals.Count - 1
                z = Math.Exp(-(xVals(r) - mu) / sigma)
                p = 1 / sigma * z * Math.Exp(-z)
                If p <= 0 Then
                    Throw New Exception("Error calculating probability for datapoint " & xVals(r) & " in Gumbel fit with parameters: " & mu & "," & sigma)
                Else
                    If r = 0 Then myResult = p Else myResult *= p
                End If
            Next
            Return myResult
        Catch ex As Exception
            Return -9000000000.0
        End Try
    End Function

    Public Function GumbelLogLikelihood(xVals As Double(), mu As Double, sigma As Double, ByRef MLE As Double, Optional ByVal MakeAverage As Boolean = False) As Boolean
        'this function computes the log likelihood for a given dataset and Gumbel distribution
        Try
            MLE = 0
            Dim f As Double                                     'probability density function value
            Dim r As Integer
            For r = 0 To xVals.Count - 1
                If Not Setup.GeneralFunctions.GumbelPDF(xVals(r), mu, sigma, f) Then Throw New Exception("Error in function GumbelLogLikelihood of class clsExtremeValuesStatLocation.")
                If f <= 0 Then
                    Throw New Exception("Error calculating probability for datapoint " & xVals(r) & " in Gumbel fit with parameters: " & mu & "," & sigma)
                Else
                    MLE = MLE + Math.Log(f)
                End If
            Next
            If MakeAverage Then MLE = MLE / xVals.Count   'convert to the average log likelihood
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GEVLogLikelihood(xVals As Double(), mu As Double, sigma As Double, kappa As Double, ByRef MLE As Double, Optional ByVal MakeAverage As Boolean = False) As Boolean
        'this function computes the log likelihood for a given dataset and Gumbel distribution
        Try
            MLE = 0
            Dim f As Double                                     'probability density function value
            Dim r As Integer
            For r = 0 To xVals.Count - 1
                If Not Setup.GeneralFunctions.GEVPDF(xVals(r), mu, sigma, kappa, f) Then Return False
                If f <= 0 Then
                    Throw New Exception("Error calculating probability for datapoint " & xVals(r) & " in GEV fit with parameters: " & mu & "," & sigma)
                Else
                    MLE = MLE + Math.Log(f)
                End If
            Next
            If MakeAverage Then MLE = MLE / xVals.Count   'convert to the average log likelihood
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GenParetoLikelihood(Values As Double(), LocPar As Double, ScalePar As Double, ShapePar As Double, ByRef Likelihood As Double) As Boolean
        'this function computes the log likelihood for a given dataset and Gumbel distribution
        Try
            Dim p As Double
            Dim r As Integer, z As Double
            For r = 0 To Values.Count - 1
                'only use the values that actually exceed the threshold
                If ScalePar = 0 OrElse ShapePar = 0 Then Return False 'prevent division by zero
                z = (Values(r) - LocPar) / ScalePar
                If (-1 - 1 / ShapePar) < 1 AndAlso (1 + ShapePar * z) < 0 Then Return False
                If (1 + ShapePar * z) < 0 Then Return False
                p = 1 / ScalePar * (1 + ShapePar * z) ^ (-1 - 1 / ShapePar)
                If p <= 0 Then Return False
                If r = 0 Then Likelihood = p Else Likelihood *= p
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
        'If (-1 - 1 / ShapePar) < 1 AndAlso (1 + ShapePar * z) < 0 Then Return False 'prevent raising a negative number to the power of a fraction e.g. -1^2.5
    End Function

    Public Function GenParetoLogLikelihood(Values As Double(), LocPar As Double, ScalePar As Double, ShapePar As Double, ByRef Likelihood As Double) As Boolean
        'this function computes the log-likelihood for a given dataset and Generalized Pareto distribution function
        Try
            Likelihood = 0
            Dim L As Double, i As Integer
            For i = 0 To Values.Count - 1
                If Setup.GeneralFunctions.GenParetoPDF(Values(i), LocPar, ScalePar, ShapePar, L) Then
                    If L <= 0 Then Return False                                                 'prevent negative likelihood values
                    Likelihood += Math.Log(L)
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function ExponentialLogLikelihood(Values As Double(), LocPar As Double, ScalePar As Double, ByRef Likelihood As Double) As Boolean
        'this function computes the log-likelihood for a given dataset and Generalized Pareto distribution function
        Try
            Likelihood = 0
            Dim L As Double, i As Integer
            For i = 0 To Values.Count - 1
                If Setup.GeneralFunctions.ExponentialPDF(Values(i), LocPar, ScalePar, L) Then
                    If L <= 0 Then Return False                                                 'prevent negative likelihood values
                    Likelihood += Math.Log(L)
                End If
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function FitDatasetMLE(data As Double(), ByRef myDist As clsProbabilityDistribution) As Boolean
        Try
            Select Case myDist.DistributionType
                Case Is = GeneralFunctions.EnmProbabilityDistribution.GenPareto
                    If Not FitGenParetoMLE(data) Then Throw New Exception("Error fitting dataset.")
                    myDist.SetShapePar(FitParameters.getShape)
                    myDist.SetScalePar(FitParameters.getScale)
                    myDist.SetLocPar(FitParameters.getLoc)
                    Return True
                Case Is = GeneralFunctions.EnmProbabilityDistribution.Exponential
                    If Not FitExptoMLE(data, data(data.Count - 1)) Then Throw New Exception("Error fitting dataset.")
                    myDist.SetScalePar(FitParameters.getScale)
                    myDist.SetLocPar(FitParameters.getLoc)
                Case Else
                    Throw New Exception("Distribution type not yet supported: " & myDist.DistributionType)
            End Select

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Sub AddValue(ByVal EventNum As Integer, ByVal myDate As Date, ByVal myVal As Double)
        'this function adds a maximum value to the respective dictionaries
        'this includes the dictionary containing annual maxima
        Dim newVal As New clsExtremeValue(myDate, myVal)
        Dim myYear As Integer = Year(myDate)
        Values.Add(EventNum, newVal)                       'add this maximum to the dictionary with all maxima

        'also add this maximum to the dictionary containing annual maxima, provided it actually represents an annual maximum
        If AnnualMax.ContainsKey(myYear) Then
            If newVal.Value > AnnualMax.Item(myYear).Value Then AnnualMax.Item(myYear) = newVal
        Else
            AnnualMax.Add(myYear, newVal)
        End If
    End Sub

    Public Function calcPlottingPositions() As Boolean
        'this function calculates the plotting positions for all maxima

        Try
            Dim i As Integer

            Select Case Statistics.DistributionType
                Case Is = GeneralFunctions.EnmProbabilityDistribution.GEV, GeneralFunctions.EnmProbabilityDistribution.GumbelMax
                    AnnualMax = (From entry In AnnualMax Order By entry.Value.Value Descending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
                    'also sort the annual maxima in descending order and assign the plotting positions. Optionally apply the Langbein transformation
                    For i = 1 To AnnualMax.Count
                        AnnualMax.Values(i - 1).PPWeibull = Setup.GeneralFunctions.WeibullPlottingPosition(i, AnnualMax.Values.Count, Statistics.Langbein)
                        AnnualMax.Values(i - 1).PPGringorten = Setup.GeneralFunctions.GringortenPlottingPosition(i, AnnualMax.Values.Count, Statistics.Langbein)
                        AnnualMax.Values(i - 1).PPBosLevenbach = Setup.GeneralFunctions.BosLevenbachPlottingPosition(i, AnnualMax.Values.Count, Statistics.Langbein)
                    Next
                Case Else
                    Values = (From entry In Values Order By entry.Value.Value Descending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
                    'sort the maxima in descending order and assign the plotting positions. notice that Langbein transformation can NOG be applied for POT values!
                    For i = 1 To Values.Count
                        Values.Values(i - 1).PPWeibull = Setup.GeneralFunctions.WeibullPlottingPosition(i, AnnualMax.Values.Count, False)
                        Values.Values(i - 1).PPGringorten = Setup.GeneralFunctions.GringortenPlottingPosition(i, AnnualMax.Values.Count, False)
                        Values.Values(i - 1).PPBosLevenbach = Setup.GeneralFunctions.BosLevenbachPlottingPosition(i, AnnualMax.Values.Count, False)
                    Next
            End Select


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Public Function BootstrapFromPDF() As Boolean
        'this function creates a new table with the same number of values as in the original data
        'but based on a resample from the fitted probability distribution function. This is meant for bootstrapping (ucnertainty analysis)
        Dim BS As Dictionary(Of Double, clsExtremeValue)
        Dim BSSorted As New Dictionary(Of Double, clsExtremeValue)
        Dim p As Double, Value As Double, nMax As Integer

        Try
            Select Case Dist.DistributionType
                Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                    nMax = AnnualMax.Count
                Case Else
                    nMax = Values.Count
            End Select

            Select Case Statistics.CensoringType
                Case Is = GeneralFunctions.enmStatisticalCensoring.AutoDetect
                    Throw New Exception("Error: censoring type AutoDetect not yet supported.")
                Case Is = GeneralFunctions.enmStatisticalCensoring.HighestNEvents
                    nMax = Math.Min(nMax, nEventsFit)
            End Select

            'write the results to a datatable
            BS = New Dictionary(Of Double, clsExtremeValue)
            For i = 1 To nMax
                'take a random exceedance probability between 0 and 1
                p = Rnd()

                'calculate the underlying value from the PDF and add it to the table
                Value = Dist.inverse(1 - p)
                BS.Add(i, New clsExtremeValue(i, Value))
            Next

            'next sort the data in descending order and add the newly created table to the list
            BSSorted = (From entry In BS Order By entry.Value.Value Descending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
            BootstrapData.Add(BSSorted)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Function BootstrapFromMaxima() As Boolean
        'this function creates a new list of dictionaries that each contain the same number of values as in the original data
        'except now based on a resample of the original data. This is meant for bootstrapping (ucnertainty analysis)
        Dim Idx As Integer, Maxima As Dictionary(Of Double, clsExtremeValue)
        Dim myDist As New clsProbabilityDistribution(Me.Setup)
        myDist.DistributionType = Dist.DistributionType 'copy the settings from the chosen distribution

        Try
            'first choose the appropriate collection of maxima for this bootstrapping. This depends on the type of distribution
            Select Case myDist.DistributionType
                Case Is = GeneralFunctions.EnmProbabilityDistribution.GumbelMax, GeneralFunctions.EnmProbabilityDistribution.GEV
                    Maxima = AnnualMax
                Case Else
                    Maxima = Values
            End Select

            'perform the bootstrapping procedure nIteration times
            Dim BS As New Dictionary(Of Double, clsExtremeValue)
            Dim BSSorted As New Dictionary(Of Double, clsExtremeValue)

            'sample the dataste by randomizing the index number
            For i = 0 To Maxima.Values.Count - 1
                'take a random sample from the original data table and add it to this bootstrap table
                Idx = Setup.GeneralFunctions.GetRandom(0, Maxima.Values.Count - 1)
                BS.Add(i, Maxima.Values(Idx))
            Next

            'next sort the data in descending order and add the newly created table to the list
            BSSorted = (From entry In BS Order By entry.Value.Value Descending).ToDictionary(Function(pair) pair.Key, Function(pair) pair.Value)
            BootstrapData.Add(BSSorted)

            'finally fit our pdf to the bootstrapped data
            myDist = New clsProbabilityDistribution(Me.Setup)
            myDist.DistributionType = Statistics.DistributionType
            Fit(BSSorted, myDist)
            BootstrapDistributions.Add(myDist)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function



    'Public Function BuildFitDataTable(nYearsObserved As Integer, nEventsFit As Integer, FitMethod As STOCHLIB.GeneralFunctions.enmStatisticalMethod) As Boolean
    '    'NOTE: obsolete. replaced by BuildCollectionFromPDF
    '    Dim p As Double, Herh As Integer, Herh2 As Double
    '    Dim Warned As Boolean = False

    '    Try
    '        Fit = New DataTable
    '        Fit.Columns.Add("Return period (years)", Type.GetType("System.Double"))
    '        Fit.Columns.Add("Fitted value", Type.GetType("System.Double"))

    '        'create a curve for 200 datapoints
    '        'note: the chosen return period can never be more frequent than the basis of the probability distribution itself
    '        'e.g. a return period of 10 years cannot be retrieved when only the 10 extremes were extracted from 109 years

    '        Select Case FitMethod
    '            Case Is = GeneralFunctions.enmStatisticalMethod.Gen_Pareto
    '                For Herh = 1 To 200
    '                    Herh2 = Herh * nEventsFit / nYearsObserved   'correction for the fact that Gen. Pareto only describes the TAIL above the threshold of nObs/nYears
    '                    p = 1 / Herh2   'exceedance probability
    '                    If p < 1 Then   'safety measure agains return periods more frequent than the base of the probability distribution
    '                        Fit.Rows.Add(Herh, dist.Inverse(1 - p))
    '                    ElseIf Warned = False Then
    '                        Me.Setup.Log.AddWarning("No result could be computed for some return periods (e.g. " & Herh & ") at location " & ID)
    '                        Warned = True
    '                    End If
    '                    'Debug.Print(dist.Inverse(1 - p))
    '                Next
    '            Case Is = GeneralFunctions.enmStatisticalMethod.Gumbel, GeneralFunctions.enmStatisticalMethod.Weibull
    '                For Herh = 1 To 200
    '                    p = 1 / Herh   'exceedance probability
    '                    If p < 1 Then   'safety measure agains return periods more frequent than the base of the probability distribution
    '                        Fit.Rows.Add(Herh, dist.Inverse(1 - p))
    '                    ElseIf Warned = False Then
    '                        Me.Setup.Log.AddWarning("No result could be computed for some return periods (e.g. " & Herh & ") at location " & ID)
    '                        Warned = True
    '                    End If
    '                Next
    '        End Select


    '        Return True
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError(ex.Message)
    '        Return False
    '    End Try
    'End Function
End Class
