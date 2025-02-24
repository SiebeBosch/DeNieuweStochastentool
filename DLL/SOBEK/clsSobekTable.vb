﻿Option Explicit On
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports GemBox.Spreadsheet

Public Class clsSobekTable

    Public ID As String
    Public DateValStrings As New Dictionary(Of String, String)
    Public Dates As New Dictionary(Of String, DateTime)
    Public XValues As New Dictionary(Of String, Single) 'als het geen tijdtabel is
    Public Values1 As New Dictionary(Of String, Single)
    Public Values2 As New Dictionary(Of String, Single)
    Public Values3 As New Dictionary(Of String, Single)
    Public Values4 As New Dictionary(Of String, Single)
    Public Values5 As New Dictionary(Of String, Single)
    Public Values6 As New Dictionary(Of String, Single)
    Public Values7 As New Dictionary(Of String, Single)
    Public Values8 As New Dictionary(Of String, Single)
    Public pdin1 As Integer '0 = continuous, 1 = block
    Public pdin2 As Integer '0 = no return period, 1 = return period
    Public PDINPeriod As String

    Public TimeStepSeconds As Integer

    'here comes a list of objects that is used for preprocessing of elevation data (collecting and sorting data)
    'Elevationcollection stores elevation values (= the KEY!) and the number of times they occur (= VALUE!)
    Friend ElevationCollection As New Dictionary(Of Single, Long)
    Friend SortedElevation As New Dictionary(Of Single, Long)

    Private setup As clsSetup

    Friend Sub AddPrefix(ByVal Prefix As String)
        ID = Prefix & ID
    End Sub

    Public Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Public Function GetLowestValue(Optional ByVal DictIdx As Integer = 1) As Double
        Dim Lowest As Double = 9.0E+99
        Dim Dict As Dictionary(Of String, Single) = getDictionary(DictIdx)
        For i = 0 To Dict.Values.Count - 1
            If Dict.Values(i) < Lowest Then
                Lowest = Dict.Values(i)
            End If
        Next
        Return Lowest
    End Function
    Public Function GetLowestIdx(ByVal DictIdx As Integer) As Integer
        Dim Lowest As Double = 9.0E+99
        Dim LowestIdx As Integer = -1
        Dim Dict As Dictionary(Of String, Single) = getDictionary(DictIdx)
        For i = 0 To Dict.Values.Count - 1
            If Dict.Values(i) < Lowest Then
                Lowest = Dict.Values(i)
                LowestIdx = i
            End If
        Next
        Return LowestIdx
    End Function

    Public Function GetHighestValue(Optional ByVal DictIdx As Integer = 1) As Double
        Dim Highest As Double = -9.0E+99
        Dim Dict As Dictionary(Of String, Single) = getDictionary(DictIdx)
        For i = 0 To Dict.Values.Count - 1
            If Dict.Values(i) > Highest Then
                Highest = Dict.Values(i)
            End If
        Next
        Return Highest
    End Function

    Public Function GetHighestIdx(ByVal DictIdx As Integer) As Integer
        Dim Highest As Double = -9.0E+99
        Dim HighestIdx As Integer = -1
        Dim Dict As Dictionary(Of String, Single) = getDictionary(DictIdx)
        For i = 0 To Dict.Values.Count - 1
            If Dict.Values(i) > Highest Then
                Highest = Dict.Values(i)
                HighestIdx = i
            End If
        Next
        Return HighestIdx
    End Function

    Public Sub addToValues(ByVal FieldIdx As Integer, ByVal addValue As Double)
        Dim myDict As Dictionary(Of String, Single)
        myDict = getDictionary(FieldIdx)
        Dim newDict As New Dictionary(Of String, Single)
        For Each myKey As String In myDict.Keys
            newDict.Add(myKey, myDict.Item(myKey) + addValue)
        Next
        setdictionary(FieldIdx, newDict)
    End Sub

    Public Function IsIncreasing() As Boolean
        Dim i As Long
        For i = 0 To XValues.Values.Count - 2
            If XValues.Values(i + 1) < XValues.Values(i) Then Return False
        Next
        For i = 0 To Values1.Values.Count - 2
            If Values1.Values(i + 1) < Values1.Values(i) Then Return False
        Next
        Return True
    End Function

    Public Sub calcTimeStepSize()
        If Dates.Count > 1 Then TimeStepSeconds = Dates.Values(1).Subtract(Dates.Values(0)).TotalSeconds
    End Sub

    Public Function getnDataCols() As Integer
        Dim n As Integer = 0
        If Values1.Count > 0 Then n += 1
        If Values2.Count > 0 Then n += 1
        If Values3.Count > 0 Then n += 1
        If Values4.Count > 0 Then n += 1
        If Values5.Count > 0 Then n += 1
        If Values6.Count > 0 Then n += 1
        If Values7.Count > 0 Then n += 1
        If Values8.Count > 0 Then n += 1
        Return n
    End Function

    Friend Function SumOf(ByVal ColIdx As Integer) As Double
        Dim myDict As Dictionary(Of String, Single) = getDictionary(ColIdx)
        Dim mySum As Double, i As Integer
        For i = 0 To myDict.Count - 1
            mySum += myDict.Item(i)
        Next
    End Function

    Private Function getDictionary(ByVal myNum As Integer) As Dictionary(Of String, Single)
        Select Case myNum
            Case Is = 0
                Return XValues
            Case Is = 1
                Return Values1
            Case Is = 2
                Return Values2
            Case Is = 3
                Return Values3
            Case Is = 4
                Return Values4
            Case Is = 5
                Return Values5
            Case Is = 6
                Return Values6
            Case Is = 7
                Return Values7
            Case Is = 8
                Return Values8
            Case Else
                Return Nothing
        End Select
    End Function

    Private Function setDictionary(ByVal myNum As Integer, ByRef myDict As Dictionary(Of String, Single)) As Boolean
        Try
            Select Case myNum
                Case Is = 0
                    XValues = myDict
                Case Is = 1
                    Values1 = myDict
                Case Is = 2
                    Values2 = myDict
                Case Is = 3
                    Values3 = myDict
                Case Is = 4
                    Values4 = myDict
                Case Is = 5
                    Values5 = myDict
                Case Is = 6
                    Values6 = myDict
                Case Is = 7
                    Values7 = myDict
                Case Is = 8
                    Values8 = myDict
                Case Else
                    Throw New Exception("Dictionary number " & myNum & " does not exist in class clsSobekTable.")
            End Select
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function IsSymmetric(ByVal DictNum As Long) As Boolean
        'finds out if the numbers in the given dictionary are symmetric
        'so walk from left to right and vice versa
        Dim i As Long, j As Long
        Dim myDict As Dictionary(Of String, Single)
        myDict = getDictionary(DictNum)

        For i = 0 To myDict.Values.Count - 1
            j = myDict.Values.Count - i - 1
            If myDict.Values(i) <> myDict.Values(j) Then
                Return False
            ElseIf i >= j Then
                Return True
            End If
        Next

        Return True

    End Function



    Public Function RemoveDuplicates(ByVal TableNum As Integer) As Boolean
        Dim myDict As Dictionary(Of String, Single) = getDictionary(TableNum)
        Dim i As Long, Done As Boolean, NoDoublesFound As Boolean = True

        While Not Done
            Done = True
            For i = 0 To myDict.Values.Count - 2
                If myDict.Values(i) = myDict.Values(i + 1) Then
                    If XValues.Count > 0 Then XValues.Remove(myDict.Keys(i))
                    If Values1.Count > 0 Then Values1.Remove(myDict.Keys(i))
                    If Values2.Count > 0 Then Values2.Remove(myDict.Keys(i))
                    If Values3.Count > 0 Then Values3.Remove(myDict.Keys(i))
                    If Values4.Count > 0 Then Values4.Remove(myDict.Keys(i))
                    If Values5.Count > 0 Then Values5.Remove(myDict.Keys(i))
                    If Values6.Count > 0 Then Values6.Remove(myDict.Keys(i))
                    If Values7.Count > 0 Then Values7.Remove(myDict.Keys(i))
                    If Values8.Count > 0 Then Values8.Remove(myDict.Keys(i))
                    NoDoublesFound = False
                    Done = False
                    Exit For
                End If
            Next
        End While
        Return NoDoublesFound
    End Function

    Public Function getYforGivenX(ByVal yDict As Integer, ByVal xDict As Integer, ByVal xVal As Double) As Double
        Dim xTable As Dictionary(Of String, Single) = getDictionary(xDict)
        Dim yTable As Dictionary(Of String, Single) = getDictionary(yDict)
        Dim i As Long
        Dim x1 As Double, x2 As Double, y1 As Double, y2 As Double

        Try
            'date: 20-2-2015
            'author: Siebe Bosch
            'description: gives the corresponding value from another dictionary, given a value in the first dictionary
            'e.g. gets the elevation value that belongs to a given distance in an yz-table

            For i = 0 To xTable.Values.Count - 2
                x1 = xTable.Values(i)
                x2 = xTable.Values(i + 1)
                y1 = yTable.Values(i)
                y2 = yTable.Values(i + 1)

                If xVal >= x1 AndAlso xVal <= x2 Then
                    Return Me.setup.GeneralFunctions.Interpolate(x1, y1, x2, y2, xVal)
                End If
            Next

            'if we end up here, it's outside the range
            If xVal < xTable.Values(0) Then
                Return yTable.Values(0)
            ElseIf xVal > xTable.Values(xTable.Values.Count - 1) Then
                Return yTable.Values(yTable.Values.Count - 1)
            Else
                Throw New Exception("Error in function getYforGivenX in class clsSobekTable. Requested value Not found in table.")
            End If
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function getYForLowestX(ByVal yDict As Integer, ByVal xDict As Integer)
        Dim xTable As Dictionary(Of String, Single) = getDictionary(xDict)
        Dim yTable As Dictionary(Of String, Single) = getDictionary(yDict)
        Dim i As Long, minVal As Double = 9999999999999999, minIdx As Long = -1
        Dim nLowest As Integer, yVal As Double

        'date: 20-2-2015
        'author: Siebe Bosch
        'description: retrieves the corresponding value from another dictionary to the lowest value from one dictionary
        'e.g. gets the distance that belongs to the lowest elevation in an yz-table

        For i = 0 To xTable.Values.Count - 1
            If xTable.Values(i) < minVal Then
                minVal = xTable.Values(i)
                minIdx = i
            End If
        Next

        'lowest found. Store the corresponding y-value
        yVal = yTable.Values(minIdx)
        nLowest = 1

        'check if value to the left has the same minimum
        If minIdx > 0 AndAlso xTable.Values(minIdx - 1) = minVal Then
            nLowest += 1
            yVal += yTable.Values(minIdx - 1)
        End If

        'check if value to the right has the same minimum
        If minIdx < xTable.Values.Count - 1 AndAlso xTable.Values(minIdx + 1) = minVal Then
            nLowest += 1
            yVal += yTable.Values(minIdx + 1)
        End If

        'compute the final y-value for the minimum
        yVal = yVal / nLowest
        Return yVal

    End Function

    Public Function getValueForDate(ByVal myDate As Date, ByVal myNum As Integer, ByVal Extrapolate As Boolean) As Double
        Dim myDict As Dictionary(Of String, Single)
        Dim i As Long
        myDict = getDictionary(myNum)

        If myDate < Dates(0) Then
            If Extrapolate Then
                Return myDict(0)
            Else
                Return 0
            End If
        ElseIf myDate > Dates(Dates.Count - 1) Then
            If Extrapolate Then
                Return myDict(myDict.Count - 1)
            Else
                Return 0
            End If
        Else
            For i = 0 To Dates.Count - 1
                If Dates(i) >= myDate Then
                    Return myDict(i)
                End If
            Next
        End If

    End Function


    Public Sub PopulateFromDataTable(Description As String, ByRef dt As DataTable, ByVal keyColumnIndex As Integer)
        Dim r As Long, key As String
        XValues.Clear()
        Values1.Clear()
        Values2.Clear()
        Values3.Clear()
        Values4.Clear()
        Values5.Clear()
        Values6.Clear()
        Values7.Clear()
        Values8.Clear()
        For r = 0 To dt.Rows.Count - 1
            key = Str(dt.Rows(r)(keyColumnIndex))  'creates a key for the tables based on a given column in the datatable
            If Not XValues.ContainsKey(key) Then
                XValues.Add(key, dt.Rows(r)(0))
                If dt.Columns.Count >= 2 Then Values1.Add(key, dt.Rows(r)(1))
                If dt.Columns.Count >= 3 Then Values2.Add(key, dt.Rows(r)(2))
                If dt.Columns.Count >= 4 Then Values3.Add(key, dt.Rows(r)(3))
                If dt.Columns.Count >= 5 Then Values4.Add(key, dt.Rows(r)(4))
                If dt.Columns.Count >= 6 Then Values5.Add(key, dt.Rows(r)(5))
                If dt.Columns.Count >= 7 Then Values6.Add(key, dt.Rows(r)(6))
                If dt.Columns.Count >= 8 Then Values7.Add(key, dt.Rows(r)(7))
                If dt.Columns.Count >= 9 Then Values8.Add(key, dt.Rows(r)(8))
            End If
        Next
    End Sub


    Public Sub DisaggregateTimeSeries(ByVal Divider As Integer, ByVal Val1 As Boolean, ByVal Val2 As Boolean, ByVal Val3 As Boolean, ByVal Val4 As Boolean, ByVal Val5 As Boolean, ByVal val6 As Boolean, val7 As Boolean, val8 As Boolean)
        Dim i As Long, j As Long, k As Long
        Dim newDates As New Dictionary(Of String, Date)
        Dim newValues1 As New Dictionary(Of String, Single)
        Dim newValues2 As New Dictionary(Of String, Single)
        Dim newValues3 As New Dictionary(Of String, Single)
        Dim newValues4 As New Dictionary(Of String, Single)
        Dim newValues5 As New Dictionary(Of String, Single)
        Dim newValues6 As New Dictionary(Of String, Single)
        Dim newValues7 As New Dictionary(Of String, Single)
        Dim newValues8 As New Dictionary(Of String, Single)

        k = -1
        For i = 0 To Dates.Count - 1
            For j = 0 To Divider - 1
                k += 1
                newDates.Add(Str(k).Trim, Dates(i).Add(TimeSpan.FromSeconds(j * TimeStepSeconds / Divider)))
                If Val1 Then newValues1.Add(Str(k).Trim, Values1(i))
                If Val2 Then newValues1.Add(Str(k).Trim, Values2(i))
                If Val3 Then newValues1.Add(Str(k).Trim, Values3(i))
                If Val4 Then newValues1.Add(Str(k).Trim, Values4(i))
                If Val5 Then newValues1.Add(Str(k).Trim, Values5(i))
                If val6 Then newValues1.Add(Str(k).Trim, Values6(i))
                If val7 Then newValues1.Add(Str(k).Trim, Values7(i))
                If val8 Then newValues1.Add(Str(k).Trim, Values8(i))
            Next
        Next

        Dates = newDates
        If Val1 Then Values1 = newValues1
        If Val2 Then Values2 = newValues2
        If Val3 Then Values3 = newValues3
        If Val4 Then Values4 = newValues4
        If Val5 Then Values5 = newValues5
        If val6 Then Values6 = newValues6
        If val7 Then Values6 = newValues7
        If val8 Then Values6 = newValues8

        Call calcTimeStepSize()

    End Sub

    Public Sub AggregateTimeSeries(ByVal Multiplier As Integer, ByVal Val1 As Boolean, ByVal Val2 As Boolean, ByVal Val3 As Boolean, ByVal Val4 As Boolean, ByVal Val5 As Boolean, ByVal val6 As Boolean, val7 As Boolean, val8 As Boolean)
        Dim i As Long, j As Long, k As Long
        Dim newDates As New Dictionary(Of String, Date)
        Dim newValues1 As New Dictionary(Of String, Single)
        Dim newValues2 As New Dictionary(Of String, Single)
        Dim newValues3 As New Dictionary(Of String, Single)
        Dim newValues4 As New Dictionary(Of String, Single)
        Dim newValues5 As New Dictionary(Of String, Single)
        Dim newValues6 As New Dictionary(Of String, Single)
        Dim newValues7 As New Dictionary(Of String, Single)
        Dim newValues8 As New Dictionary(Of String, Single)
        Dim Sum1 As Double, Sum2 As Double, Sum3 As Double, Sum4 As Double, Sum5 As Double, Sum6 As Double, sum7 As Double, sum8 As Double

        k = -1
        For i = 0 To Dates.Count - 1 Step Multiplier
            k += 1
            newDates.Add(Str(k).Trim, Dates(i))
            Sum1 = 0
            Sum2 = 0
            Sum3 = 0
            Sum4 = 0
            Sum5 = 0
            Sum6 = 0
            sum7 = 0
            sum8 = 0

            For j = 0 To Multiplier - 1
                If Val1 Then Sum1 += Values1(k + j)
                If Val2 Then Sum2 += Values2(k + j)
                If Val3 Then Sum3 += Values3(k + j)
                If Val4 Then Sum4 += Values4(k + j)
                If Val5 Then Sum5 += Values5(k + j)
                If val6 Then Sum6 += Values6(k + j)
                If val7 Then sum7 += Values7(k + j)
                If val8 Then sum8 += Values8(k + j)
            Next

            If Val1 Then newValues1.Add(Str(k).Trim, Val1 / Multiplier)
            If Val2 Then newValues2.Add(Str(k).Trim, Val2 / Multiplier)
            If Val3 Then newValues3.Add(Str(k).Trim, Val3 / Multiplier)
            If Val4 Then newValues4.Add(Str(k).Trim, Val4 / Multiplier)
            If Val5 Then newValues5.Add(Str(k).Trim, Val5 / Multiplier)
            If val6 Then newValues6.Add(Str(k).Trim, val6 / Multiplier)
            If val7 Then newValues6.Add(Str(k).Trim, val7 / Multiplier)
            If val8 Then newValues6.Add(Str(k).Trim, val8 / Multiplier)
        Next

        Dates = newDates
        If Val1 Then Values1 = newValues1
        If Val2 Then Values2 = newValues2
        If Val3 Then Values3 = newValues3
        If Val4 Then Values4 = newValues4
        If Val5 Then Values5 = newValues5
        If val6 Then Values6 = newValues6
        If val7 Then Values6 = newValues7
        If val8 Then Values6 = newValues8

        Call calcTimeStepSize()

    End Sub


    Public Function Smooth(ByVal ValIdx As Integer, ByVal Degree As Integer, ByVal Cumulative As Boolean) As Boolean
        'in this funtion we'll call the Savitzky Golay Smoothing algorithm
        Dim Values As Dictionary(Of String, Single) = getDictionary(ValIdx)

        If Degree < 1 Then
            Me.setup.Log.AddError("Error in function Smooth of class clsSobekTable. Degree for smoothing must be >=1")
            Return False
        End If

        Dim i As Long, XVals(0 To Values.Count - 1) As Double, YVals(0 To Values.Count - 1) As Double
        Dim YSmooth(0 To Values.Count - 1) As Double

        'first convert the dates and values into arrays
        For i = 0 To Values.Count - 1
            XVals(i) = Dates(i).ToOADate
            YVals(i) = Values(i)
        Next

        'perform the smoothing
        YSmooth = Me.setup.SmoothSavGoy.Calculate(XVals, YVals, Degree, Cumulative)

        'copy the results back into the influx table
        For i = 0 To Dates.Count - 1
            Values(i) = YSmooth(i)
        Next
        Return True

    End Function

    Public Sub FromArray(ByVal Vals() As Single, ByVal ValIdx As Integer)
        'puts the values of an array in the requested values list
        Dim Values As Dictionary(Of String, Single), i As Long
        Values = getDictionary(ValIdx)

        For i = 0 To Values.Count - 1
            Values(i) = Vals(i)
        Next

    End Sub

    Public Function getPercentageValue(ByVal myPerc As Integer, ByVal SearchListNum As Integer, ByVal ReturnListNum As Integer) As Single
        'this function searches a percentage in one list and returns the corresponding values from another list
        Dim SearchList As New Dictionary(Of String, Single)
        Dim ReturnList As New Dictionary(Of String, Single)

        SearchList = getDictionary(SearchListNum)
        ReturnList = getDictionary(ReturnListNum)

        Dim maxVal As Single = getMaxValue(SearchListNum)
        Dim minVal As Single = getMinValue(SearchListNum)
        Dim X3 As Single = myPerc / 100 * maxVal
        Dim X1 As Single, X2 As Single, Y1 As Single, Y2 As Single
        Dim i As Long

        For i = 0 To SearchList.Count - 2
            X1 = SearchList.Values(i)
            X2 = SearchList.Values(i + 1)
            Y1 = ReturnList.Values(i)
            Y2 = ReturnList.Values(i + 1)
            If X3 <= X2 AndAlso X3 >= X1 Then
                Return setup.GeneralFunctions.Interpolate(X1, Y1, X2, Y2, X3)
            End If
        Next

        If X3 <= minVal Then Return ReturnList.Values(0)
        If X3 >= maxVal Then Return ReturnList.Values(ReturnList.Values.Count - 1)

    End Function

    Public Function getMaxValue(ByVal ValuesListNumber As Integer) As Single
        Select Case ValuesListNumber
            Case Is = 0
                Return XValues.Values(getMaxValueIdx(ValuesListNumber))
            Case Is = 1
                Return Values1.Values(getMaxValueIdx(ValuesListNumber))
            Case Is = 2
                Return Values2.Values(getMaxValueIdx(ValuesListNumber))
            Case Is = 3
                Return Values3.Values(getMaxValueIdx(ValuesListNumber))
            Case Is = 4
                Return Values4.Values(getMaxValueIdx(ValuesListNumber))
            Case Is = 5
                Return Values5.Values(getMaxValueIdx(ValuesListNumber))
            Case Is = 6
                Return Values6.Values(getMaxValueIdx(ValuesListNumber))
            Case Is = 7
                Return Values7.Values(getMaxValueIdx(ValuesListNumber))
            Case Is = 8
                Return Values8.Values(getMaxValueIdx(ValuesListNumber))
        End Select

    End Function

    Public Function getMinValue(ByVal ValuesListNumber As Integer) As Single
        Select Case ValuesListNumber
            Case Is = 0
                Return XValues.Values(GetMinValueIdx(ValuesListNumber))
            Case Is = 1
                Return Values1.Values(GetMinValueIdx(ValuesListNumber))
            Case Is = 2
                Return Values2.Values(GetMinValueIdx(ValuesListNumber))
            Case Is = 3
                Return Values3.Values(GetMinValueIdx(ValuesListNumber))
            Case Is = 4
                Return Values4.Values(GetMinValueIdx(ValuesListNumber))
            Case Is = 5
                Return Values5.Values(GetMinValueIdx(ValuesListNumber))
            Case Is = 6
                Return Values6.Values(GetMinValueIdx(ValuesListNumber))
            Case Is = 7
                Return Values7.Values(GetMinValueIdx(ValuesListNumber))
            Case Is = 8
                Return Values8.Values(GetMinValueIdx(ValuesListNumber))
        End Select

    End Function

    Friend Sub ClearValues(ByVal ValIdx As Integer)

        Dim Values As New Dictionary(Of String, Single)
        Dim i As Long

        Select Case ValIdx
            Case Is = 1
                Values = Values1
            Case Is = 2
                Values = Values2
            Case Is = 3
                Values = Values3
            Case Is = 4
                Values = Values4
            Case Is = 5
                Values = Values5
            Case Is = 6
                Values = Values6
            Case Is = 7
                Values = Values7
            Case Is = 8
                Values = Values8
        End Select

        For i = 0 To Values.Count - 1
            Values(i) = 0
        Next

    End Sub

    Friend Sub MovingAverage(ByVal ValIdx As Long, ByVal nSteps As Long)
        Dim myAvg As New Dictionary(Of String, Single)
        Dim Values As New Dictionary(Of String, Single)
        Dim i As Long, j As Long, mySum As Single
        Dim radius As Integer = Me.setup.GeneralFunctions.RoundUD(nSteps / 2, 0, False)

        Select Case ValIdx
            Case Is = 1
                Values = Values1
            Case Is = 2
                Values = Values2
            Case Is = 3
                Values = Values3
            Case Is = 4
                Values = Values4
            Case Is = 5
                Values = Values5
            Case Is = 6
                Values = Values6
            Case Is = 7
                Values = Values7
            Case Is = 8
                Values = Values8
        End Select

        'calculate the moving average
        For i = 0 To radius - 1
            myAvg(i) = Values(i)
        Next
        For i = radius To Values.Count - 1 - radius
            mySum = 0
            For j = i - radius To i + radius
                mySum += Values(j)
            Next
            myAvg(i) = mySum / (2 * radius + 1)
        Next
        For i = Values.Count - radius To Values.Count - 1
            myAvg(i) = Values(i)
        Next

        'now copy the moving average back to the values
        For i = 0 To Values.Count - 1
            Values(i) = myAvg(i)
        Next


    End Sub

    Friend Function getavgValue(ByVal ValIdx As Long, Optional ByVal startTS As Long = 0, Optional ByVal EndTS As Long = 0) As Double
        Dim Values As New Dictionary(Of String, Single)
        Dim i As Long, Sum As Double

        Select Case ValIdx
            Case Is = 1
                Values = Values1
            Case Is = 2
                Values = Values2
            Case Is = 3
                Values = Values3
            Case Is = 4
                Values = Values4
            Case Is = 5
                Values = Values5
            Case Is = 6
                Values = Values6
            Case Is = 7
                Values = Values7
            Case Is = 8
                Values = Values8
        End Select

        If EndTS = 0 Then
            EndTS = Values.Count - 1
        ElseIf EndTS > Values.Count - 1 Then
            EndTS = Values.Count - 1
        End If

        For i = startTS To EndTS
            Sum += Values(i)
        Next

        If Values.Count > 0 Then
            Return Sum / Values.Count
        Else
            Return 0
        End If

    End Function

    Friend Function ClonePart(StartIdx As Integer, EndIdx As Integer, SkipIdx As List(Of Integer)) As clsSobekTable
        Dim newTable As New clsSobekTable(Me.setup)
        Dim myKey As String
        Dim i As Long

        newTable.ID = ID
        newTable.pdin1 = pdin1
        newTable.pdin2 = pdin2
        newTable.PDINPeriod = PDINPeriod

        If XValues.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = XValues.Keys(i)
                    newTable.XValues.Add(myKey, XValues.Item(myKey))
                End If
            Next
        End If


        If DateValStrings.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = DateValStrings.Keys(i)
                    newTable.DateValStrings.Add(myKey, DateValStrings.Item(myKey))
                End If
            Next
        End If

        If Dates.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = Dates.Keys(i)
                    newTable.Dates.Add(myKey, Dates.Item(myKey))
                End If
            Next
        End If

        If Values1.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = Values1.Keys(i)
                    newTable.Values1.Add(myKey, Values1.Item(myKey))
                End If
            Next
        End If

        If Values2.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = Values2.Keys(i)
                    newTable.Values2.Add(myKey, Values2.Item(myKey))
                End If
            Next
        End If

        If Values3.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = Values3.Keys(i)
                    newTable.Values3.Add(myKey, Values3.Item(myKey))
                End If
            Next
        End If

        If Values4.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = Values4.Keys(i)
                    newTable.Values4.Add(myKey, Values4.Item(myKey))
                End If
            Next
        End If

        If Values5.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = Values5.Keys(i)
                    newTable.Values5.Add(myKey, Values5.Item(myKey))
                End If
            Next
        End If

        If Values6.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = Values6.Keys(i)
                    newTable.Values6.Add(myKey, Values6.Item(myKey))
                End If
            Next
        End If

        If Values7.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = Values7.Keys(i)
                    newTable.Values7.Add(myKey, Values7.Item(myKey))
                End If
            Next
        End If

        If Values8.Count > EndIdx Then
            For i = StartIdx To EndIdx
                If Not SkipIdx.Contains(i) Then
                    myKey = Values8.Keys(i)
                    newTable.Values8.Add(myKey, Values8.Item(myKey))
                End If
            Next
        End If

        newTable.TimeStepSeconds = TimeStepSeconds
        Return newTable

    End Function
    Public Function Clone(Optional ByVal XShift As Double = 0, Optional ByVal AddSurroundingVerticalWall As Boolean = False, Optional CleanupDoubleXValues As Boolean = False) As clsSobekTable

        Dim newTable As New clsSobekTable(Me.setup)
        Dim myKey As String = ""
        Dim SkipIdx As New List(Of Integer)
        Dim i As Long

        newTable.ID = ID
        newTable.pdin1 = pdin1
        newTable.pdin2 = pdin2
        newTable.PDINPeriod = PDINPeriod

        If CleanupDoubleXValues Then
            For i = 0 To XValues.Count - 2
                If XValues.Values(i) = XValues.Values(i + 1) Then SkipIdx.Add(i + 1)
            Next
        End If

        For i = 0 To DateValStrings.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = DateValStrings.Keys(i)
                newTable.DateValStrings.Add(myKey, DateValStrings.Item(myKey))
            End If
        Next

        For i = 0 To Dates.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = Dates.Keys(i)
                newTable.Dates.Add(myKey, Dates.Item(myKey))
            End If
        Next

        If AddSurroundingVerticalWall Then newTable.XValues.Add("leftwall", XValues.Values(0))
        For i = 0 To XValues.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = XValues.Keys(i)
                newTable.XValues.Add(myKey, XValues.Item(myKey) + XShift)
            End If
        Next

        If AddSurroundingVerticalWall Then newTable.XValues.Add("rightwall", XValues.Values(XValues.Count - 1))
        If AddSurroundingVerticalWall Then newTable.Values1.Add("leftwall", Values1.Values(0) + 9000000000.0)

        For i = 0 To Values1.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = Values1.Keys(i)
                newTable.Values1.Add(myKey, Values1.Item(myKey))
            End If
        Next
        If AddSurroundingVerticalWall Then newTable.Values1.Add("rightwall", Values1.Values(Values1.Count - 1) + 9000000000.0)

        For i = 0 To Values2.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = Values2.Keys(i)
                newTable.Values2.Add(myKey, Values2.Item(myKey))
            End If
        Next

        For i = 0 To Values3.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = Values3.Keys(i)
                newTable.Values3.Add(myKey, Values3.Item(myKey))
            End If
        Next

        For i = 0 To Values4.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = Values4.Keys(i)
                newTable.Values4.Add(myKey, Values4.Item(myKey))
            End If
        Next

        For i = 0 To Values5.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = Values5.Keys(i)
                newTable.Values5.Add(myKey, Values5.Item(myKey))
            End If
        Next

        For i = 0 To Values6.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = Values6.Keys(i)
                newTable.Values6.Add(myKey, Values6.Item(myKey))
            End If
        Next

        For i = 0 To Values7.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = Values7.Keys(i)
                newTable.Values7.Add(myKey, Values7.Item(myKey))
            End If
        Next

        For i = 0 To Values8.Count - 1
            If Not SkipIdx.Contains(i) Then
                myKey = Values8.Keys(i)
                newTable.Values8.Add(myKey, Values8.Item(myKey))
            End If
        Next

        newTable.TimeStepSeconds = TimeStepSeconds
        Return newTable

    End Function

    Friend Sub AjustValuesByPercentage(ByVal Perc As Double, ByVal ValIdx As Integer, Optional ByVal mystartts As Long = -1, Optional ByVal myendts As Long = -1)
        'adjusts all values of a given collection by a given percentage
        Dim Values As New Dictionary(Of String, Single)
        Dim i As Long, startts As Long, endts As Long

        'select the values collection that needs adjustment
        Select Case ValIdx
            Case Is = 0
                Values = XValues
            Case Is = 1
                Values = Values1
            Case Is = 2
                Values = Values2
            Case Is = 3
                Values = Values3
            Case Is = 4
                Values = Values4
            Case Is = 5
                Values = Values5
            Case Is = 6
                Values = Values6
            Case Is = 7
                Values = Values7
            Case Is = 8
                Values = Values8
        End Select

        'set the starting timestep for the adjustments
        If mystartts < 0 Then
            startts = 0
        Else
            startts = mystartts
        End If

        'set the end timestep for the adjustments
        If myendts < 0 Then
            endts = Values.Count - 1
        Else
            endts = myendts
        End If

        'perform the actual adjustments
        For i = startts To endts
            Values(i) = Values(i) * (100 + Perc) / 100
        Next

    End Sub

    Friend Function Shift(ByVal ts As Integer, ByVal valIdx As Integer) As Boolean

        Dim i As Long, j As Long
        Dim tmpStor() As Single 'a container for the temporarily stored values that need shifting over the edges

        Dim Values As New Dictionary(Of String, Single)
        Select Case valIdx
            Case Is = 1
                Values = Values1
            Case Is = 2
                Values = Values2
            Case Is = 3
                Values = Values3
            Case Is = 4
                Values = Values4
            Case Is = 5
                Values = Values5
            Case Is = 6
                Values = Values6
            Case Is = 7
                Values = Values7
            Case Is = 8
                Values = Values8
        End Select

        If ts < 0 Then
            'first remember the part that will be cut off. it has to be glued back to the end after shifting
            ReDim tmpStor(-ts - 1)
            For i = 0 To -ts - 1
                tmpStor(i) = Values.Values(i)
            Next
            'now move all other items to the left
            For i = -ts To Values.Count - 1
                Values(i - -ts) = Values(i)
            Next
            'glue back the cutoff part
            j = -1
            For i = Values1.Count - -ts To Values1.Count - 1
                j += 1
                Values(i) = tmpStor(j)
            Next
            Return True
        ElseIf ts > 0 Then
            'first remember the part that will be cut off. it has to be glued back to the start after shifting
            ReDim tmpStor(ts - 1)
            j = -1
            For i = Values.Count - ts To Values.Count - 1
                j += 1
                tmpStor(j) = Values.Values(i)
            Next
            'now move all other items to the right
            For i = Values.Count - 1 To ts Step -1
                Values(i) = Values(i - ts)
            Next
            'glue back the cutoff part
            j = -1
            For i = 0 To ts - 1
                j += 1
                Values(i) = tmpStor(j)
            Next
            Return True
        End If

    End Function

    Public Function SortElevationData(ByVal ElevationMultiplier As Double, ByVal CellSize As Double, Optional ByVal DivideByReachLength As Double = 1) As Boolean

        'this routine sorts the collected elevation data that has not yet been stored in the
        'standard objects Xvalues, Values1 etc., but that's still in the ElevationData dictionary
        'after sorting it will assign the results to the appropriate dictionaries
        Dim i As Long, nCum As Long
        If ElevationCollection.Count > 0 Then

            'sort the dictionary containing collected data by key
            Dim Sorted = From pair In ElevationCollection Order By pair.Key
            SortedElevation = Sorted.ToDictionary(Function(p) p.Key, Function(p) p.Value)

            'write all elevation data to a storage table, do this in the stepsize defined above
            For i = 0 To SortedElevation.Count - 1
                nCum += SortedElevation.Values(i)
                AddDataPair(2, SortedElevation.Keys(i) * ElevationMultiplier, nCum * CellSize * CellSize / DivideByReachLength)
            Next

            'de lijst met elevationdata mag worden leeggemaakt. Wordt hierna niet langer gebruikt
            ElevationCollection = Nothing
            SortedElevation = Nothing
            Return True
        Else
            Return False
        End If

    End Function

    Public Function SortStorageData(ByVal ElevationMultiplier As Integer, ByVal CellSize As Double, Optional ByVal DivideByReachLength As Double = 1) As Boolean

        'this routine sorts the collected elevation data that has not yet been stored in the
        'standard objects Xvalues, Values1 etc., but that's still in the ElevationData dictionary
        'after sorting it will assign the results to the appropriate dictionaries
        Dim i As Long, nCum As Long, volCum As Double
        If ElevationCollection.Count > 0 Then

            'sort the dictionary containing collected data by key
            Dim Sorted = From pair In ElevationCollection Order By pair.Key
            SortedElevation = Sorted.ToDictionary(Function(p) p.Key, Function(p) p.Value)

            'write all elevation data to a storage table, do this in the stepsize defined above
            nCum = SortedElevation.Values(0)
            volCum = 0
            AddDataPair(3, SortedElevation.Keys(0) * ElevationMultiplier, nCum * CellSize * CellSize / DivideByReachLength, volCum)
            For i = 1 To SortedElevation.Count - 1
                volCum += nCum * CellSize * CellSize / DivideByReachLength * (SortedElevation.Keys(i) - SortedElevation.Keys(i - 1)) * ElevationMultiplier
                nCum += SortedElevation.Values(i)
                AddDataPair(3, SortedElevation.Keys(i) * ElevationMultiplier, nCum * CellSize * CellSize / DivideByReachLength, volCum)
            Next

            'de lijst met elevationdata mag worden leeggemaakt. Wordt hierna niet langer gebruikt
            ElevationCollection = Nothing
            SortedElevation = Nothing
            Return True
        Else
            Return False
        End If

    End Function

    Friend Function GetStatistic(ByVal ValIdx As Integer, ByVal opt As STOCHLIB.GeneralFunctions.enmHydroMathOperation) As Double

        Dim n As Long, Max As Double = -999999, Min As Double = 999999
        Dim myVal As Single, mySum As Single

        'This function gets a statistical property from the timetable
        If ValIdx < 0 Then ValIdx = 1
        If ValIdx > 6 Then ValIdx = 6

        'bepaal eerst uit welke tabel we waarden gaan teruggeven
        Dim SearchDict As Dictionary(Of String, Single) = Nothing
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

        If opt = GeneralFunctions.enmHydroMathOperation.MAX Then
            For Each myVal In SearchDict.Values
                If myVal > Max Then Max = myVal
            Next
            Return Max
        ElseIf opt = GeneralFunctions.enmHydroMathOperation.Min Then
            For Each myVal In SearchDict.Values
                If myVal < Min Then Min = myVal
            Next
            Return Min
        ElseIf opt = GeneralFunctions.enmHydroMathOperation.Avg Then
            For Each myVal In SearchDict.Values
                n += 1
                mySum += myVal
            Next
            If n > 0 Then
                Return mySum / n
            Else
                Return 0
            End If
        ElseIf opt = GeneralFunctions.enmHydroMathOperation.Sum Then
            For Each myVal In SearchDict.Values
                mySum += myVal
            Next
            Return mySum
        Else
            Me.setup.Log.AddError("Error in function GetStatistic of class clsSobekTable.")
            Me.setup.Log.AddError("Statistical property " & opt.ToString & " Not yet supported.")
            Return 0
        End If


    End Function

    Friend Function CalculateAreaUnderValueFromTabulated(ByVal myDepth As Double) As Double
        'this function calculates the wetted area A for a given water depth in the current table
        Dim TotalArea As Double = 0
        Dim curWidth As Double, prevWidth As Double
        For i = 1 To XValues.Count - 1
            If myDepth <= XValues.Values(i) AndAlso myDepth > XValues.Values(i - 1) Then
                'previous section is partially filled. find the corresponding width
                prevWidth = Values1.Values(i - 1)
                curWidth = InterpolateFromXValues(myDepth)
                TotalArea += (myDepth - XValues.Values(i - 1)) * (Math.Min(curWidth, prevWidth) + Math.Abs(curWidth - prevWidth) / 2)
            ElseIf myDepth > XValues.Values(i) Then
                'previous section is completely filled. Add it to the total area
                prevWidth = Values1.Values(i - 1)
                curWidth = Values1.Values(i)
                TotalArea += (XValues.Values(i) - XValues.Values(i - 1)) * (Math.Min(curWidth, prevWidth) + Math.Abs(curWidth - prevWidth) / 2)
            End If
        Next
        If myDepth > XValues.Values(XValues.Count - 1) Then
            TotalArea += (myDepth - XValues.Values(XValues.Count - 1)) * (Values1.Values(Values1.Count - 1))
        End If
        Return TotalArea
    End Function


    Friend Function CalculateWettedPerimeterUnderValueFromTabulated(ByVal myDepth As Double) As Double
        'this function calculates the wetted Peremiter P for a given water depth in the current table
        Dim TotalP As Double = 0
        Dim curWidth As Double, prevWidth As Double

        'start with the first segment since it involves the bottom of the channel
        If myDepth >= XValues.Values(0) Then TotalP += Values1.Values(0)
        For i = 1 To XValues.Count - 1
            If myDepth <= XValues.Values(i) AndAlso myDepth > XValues.Values(i - 1) Then
                'previous segment is partially filled
                prevWidth = Values1.Values(i - 1)
                curWidth = InterpolateFromXValues(myDepth)
                Dim halfdw As Double = Math.Abs(curWidth - prevWidth) / 2
                Dim dh As Double = XValues.Values(i) - XValues.Values(i - 1)
                'wetted perimeter only increases when the flow width actually exceeds 0
                If Math.Max(curWidth, prevWidth) > 0 Then
                    TotalP += 2 * Me.setup.GeneralFunctions.Pythagoras(XValues.Values(i - 1), 0, myDepth, Math.Abs(curWidth - prevWidth) / 2)
                End If
            ElseIf myDepth > XValues.Values(i) Then
                'previous segment is completely filled.
                prevWidth = Values1.Values(i - 1)
                curWidth = Values1.Values(i)
                Dim halfdw As Double = Math.Abs(curWidth - prevWidth) / 2
                Dim dh As Double = XValues.Values(i) - XValues.Values(i - 1)
                'wetted perimeter only increases when the flow width actually exceeds 0
                If Math.Max(curWidth, prevWidth) > 0 Then
                    TotalP += 2 * Math.Sqrt(halfdw ^ 2 + dh ^ 2)
                End If
            End If
        Next
        Return TotalP
    End Function

    Friend Function CalculateAreaUnderValueFromYZTable(ByVal myVal As Double) As Double
        'Author: Siebe Bosch
        'Date: 17-9-2013
        'Description: calculates the area under a certain y-value
        Dim DeepestIdx As Long = GetMinValueIdx(1)
        Dim i As Long
        Dim curY As Double, nextY As Double
        Dim curZ As Double, nextZ As Double, LowestZ As Double, HighestZ As Double
        Dim myArea As Double = 0
        Dim IntersectionY As Double

        For i = 0 To XValues.Count - 2
            curY = XValues.Values(i)
            nextY = XValues.Values(i + 1)
            curZ = Values1.Values(i)
            nextZ = Values1.Values(i + 1)
            LowestZ = Math.Min(curZ, nextZ)
            HighestZ = Math.Max(curZ, nextZ)

            If myVal >= curZ AndAlso myVal >= nextZ Then
                'triangle plus a rectangle on top of it
                myArea += (nextY - curY) * (myVal - HighestZ) + (nextY - curY) * (HighestZ - LowestZ) / 2
            ElseIf myVal >= curZ AndAlso myVal < nextZ Then
                'just a triangle. Find the intersection point first!
                IntersectionY = setup.GeneralFunctions.Interpolate(curZ, curY, nextZ, nextY, myVal)
                myArea += (IntersectionY - curY) * (myVal - curZ) * 1 / 2
            ElseIf myVal >= nextZ AndAlso myVal < curZ Then
                'just a triangle. Find the intersection point first!
                IntersectionY = setup.GeneralFunctions.Interpolate(curZ, curY, nextZ, nextY, myVal)
                myArea += (nextY - IntersectionY) * (myVal - nextZ) * 1 / 2
            End If
        Next

        Return myArea

    End Function


    Friend Function GetMinValueIdx(ByVal ValuesListNum As Integer, Optional ByVal StartIdx As Integer = -1, Optional ByVal EndIdx As Integer = -1) As Long
        Dim Values As New Dictionary(Of String, Single)
        Dim i As Long, minVal As Single = 99999999, minIdx As Long

        Values = getDictionary(ValuesListNum) 'get the dictionary of values that applies
        If StartIdx < 0 Then StartIdx = 0
        If EndIdx < 0 Then EndIdx = Values.Count - 1

        For i = StartIdx To EndIdx
            If Values.Values(i) < minVal Then
                minVal = Values.Values(i)
                minIdx = i
            End If
        Next
        Return minIdx
    End Function

    Public Function getMaxValueIdx(ByVal ValuesListNum As Integer, Optional ByVal StartIdx As Integer = -1, Optional ByVal EndIdx As Integer = -1) As Long

        Dim i As Long, myMax As Double = -99999999999, myIdx As Long = 0
        Dim Values As Dictionary(Of String, Single)

        Select Case ValuesListNum
            Case Is = 0
                Values = XValues
            Case Is = 1
                Values = Values1
            Case Is = 2
                Values = Values2
            Case Is = 3
                Values = Values3
            Case Is = 4
                Values = Values4
            Case Is = 5
                Values = Values5
            Case Is = 6
                Values = Values6
            Case Is = 7
                Values = Values7
            Case Is = 8
                Values = Values8
            Case Else
                Return 0
        End Select

        If StartIdx < 0 Then StartIdx = 0
        If EndIdx < 0 Then EndIdx = Values.Count - 1

        For i = StartIdx To EndIdx
            If Values.Values(i) > myMax Then
                myMax = Values.Values(i)
                myIdx = i
            End If
        Next
        Return myIdx

    End Function

    Friend Function getIntersectionXVal(ByVal ValIdx As Long, ByVal Upward As Boolean, ByVal Downward As Boolean, ByVal SearchValue As Double, ByVal StartIdx As Long, ByVal EndIdx As Long, ByRef XVal As Double) As Boolean
        'Searches the surrounding index points for a given Value
        Dim Values As New Dictionary(Of String, Single)
        Dim DeepestIdx As Integer, HighestIdx As Integer
        Dim DeepestVal As Double, HighestVal As Double
        Dim i As Long

        Select Case ValIdx
            Case Is = 1
                Values = Values1
            Case Is = 2
                Values = Values2
            Case Is = 3
                Values = Values3
            Case Is = 4
                Values = Values4
            Case Is = 5
                Values = Values5
            Case Is = 6
                Values = Values6
            Case Is = 7
                Values = Values7
            Case Is = 8
                Values = Values8
        End Select

        DeepestIdx = GetMinValueIdx(1, StartIdx, EndIdx)
        HighestIdx = getMaxValueIdx(1, StartIdx, EndIdx)
        DeepestVal = Values.Values(DeepestIdx)
        HighestVal = Values.Values(HighestIdx)

        If SearchValue > HighestVal Then
            Me.setup.Log.AddWarning("Could Not interpolate in table " & ID & ": value " & SearchValue & " exceeds highest value in table.")
            Return False
        ElseIf SearchValue < DeepestVal Then
            Me.setup.Log.AddWarning("Could not interpolate in table " & ID & ": value " & SearchValue & " is lower than lowest value in table.")
            Return False
        End If

        If Upward Then
            For i = EndIdx - 1 To StartIdx Step -1 'move from right to left!
                If Values.Values(i) <= SearchValue And Values.Values(i + 1) >= SearchValue Then
                    XVal = setup.GeneralFunctions.Interpolate(Values.Values(i), XValues.Values(i), Values.Values(i + 1), XValues.Values(i + 1), SearchValue)
                    Return True
                End If
            Next
        End If

        If Downward Then
            For i = StartIdx To EndIdx - 1
                If Values.Values(i) >= SearchValue And Values.Values(i + 1) <= SearchValue Then
                    XVal = setup.GeneralFunctions.Interpolate(Values.Values(i), XValues.Values(i), Values.Values(i + 1), XValues.Values(i + 1), SearchValue)
                    Return True
                End If
            Next
        End If

        Return False

    End Function

    Friend Sub mmph2m3ps(ByVal AreaM2 As Double)
        'converteer alle waarden in een SobekTable van mm/h naar m3/s
        'de laatstgenoemde komt in Values1 te staan, bron staat in Values2
        Dim i As Long, n As Long

        Me.setup.GeneralFunctions.UpdateProgressBar("Converting discharge from mm/h to m3/s", 0, 10)

        If Values1.Count > 0 Then
            Me.setup.Log.AddError("Conversion of discharges from mm/h to m3/s went wrong because collection for result was already filled.")
            Exit Sub
        Else
            n = Values2.Count - 1
            For i = 0 To Values2.Count - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, n)
                Values1.Add(Values2.Keys(i), Me.setup.GeneralFunctions.unitConversion.mmph2m3ps(Values2.Values(i), AreaM2))
            Next
        End If

    End Sub

    Public Function getSlopeFromValues1(ByVal fromIdx As Integer, ByVal toIdx As Integer) As Double
        Dim Val1 As Double = Values1.Values(fromIdx)
        Dim Val2 As Double = Values1.Values(toIdx)
        Dim X1 As Double = XValues.Values(fromIdx)
        Dim X2 As Double = XValues.Values(toIdx)

        If Not (X2 - X1) = 0 Then
            Return (Val2 - Val1) / (X2 - X1)
        Else
            Return 0
        End If

    End Function

    Public Function getTotalLength(ByVal FromIdx As Integer, ByVal ToIdx As Integer) As Double

        'Returns the total length of a segment as calculated by pythagoras
        'SQR(dX^2 + dY^2)
        Dim Val1 As Double = Values1.Values(FromIdx)
        Dim Val2 As Double = Values1.Values(ToIdx)
        Dim X1 As Double = XValues.Values(FromIdx)
        Dim X2 As Double = XValues.Values(ToIdx)

        Return Math.Sqrt((X2 - X1) ^ 2 + (Val2 - Val1) ^ 2)

    End Function

    Public Function getBaseLength(ByVal FromIdx As Integer, ByVal ToIdx As Integer) As Double
        'Returns the length of the base (XValues) between two given index points
        Dim X1 As Double = XValues.Values(FromIdx)
        Dim X2 As Double = XValues.Values(ToIdx)

        Return X2 - X1

    End Function

    Public Function getLastValues1Value() As Double
        Return Values1.Values(Values1.Count - 1)
    End Function


    Friend Sub m3ps2mmph(ByVal AreaM2 As Double)
        'converteer alle waarden in een SobekTable van de m3/s naar mm/h.
        'de laatstgenoemde komt in Values2 te staan
        Dim i As Long, n As Long

        Me.setup.GeneralFunctions.UpdateProgressBar("Converting discharge from m3/s to mm/h", 0, 10)

        If Values2.Count > 0 Then
            Me.setup.Log.AddError("Conversion of discharges from m3/s to mm/h went wrong because collection for result was already filled.")
            Exit Sub
        Else
            n = Values1.Count - 1
            For i = 0 To Values1.Count - 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, n)
                Values2.Add(Values1.Keys(i), Me.setup.GeneralFunctions.unitConversion.m3ps2mmph(Values1.Values(i), AreaM2))
            Next
        End If
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

    Friend Function InterpolateXValueFromValues(ByVal Val As Double, Optional ByVal ValIdx As Integer = 1, Optional ByVal ExtrapolationBelow As GeneralFunctions.enmExtrapolationMethod = GeneralFunctions.enmExtrapolationMethod.MakeZero, Optional ByVal ExtrapolationAbove As GeneralFunctions.enmExtrapolationMethod = GeneralFunctions.enmExtrapolationMethod.KeepConstant) As Single
        Try
            Dim i As Integer
            If ValIdx < 0 Then ValIdx = 1
            If ValIdx > 6 Then ValIdx = 6

            'bepaal eerst uit welke tabel we waarden gaan teruggeven
            Dim SearchDict As Dictionary(Of String, Single) = Nothing
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

            If Val < SearchDict.Values(0) Then
                If ExtrapolationBelow = enmExtrapolationMethod.MakeZero Then
                    Return 0
                ElseIf ExtrapolationBelow = enmExtrapolationMethod.KeepConstant Then
                    Return XValues.Values(0)
                ElseIf ExtrapolationBelow = enmExtrapolationMethod.ExtrapolateLinear AndAlso XValues.Count > 1 Then
                    Return Me.setup.GeneralFunctions.Interpolate(Values1.Values(0), XValues.Values(0), Values1.Values(1), XValues.Values(1), Val,, True)
                Else
                    Me.setup.Log.AddError("Error interpolating from table: " & ID & ". Interpolate value " & Val & " failed.")
                    Return 0
                End If
            ElseIf Val > SearchDict.Values(SearchDict.Count - 1) Then
                If ExtrapolationAbove = enmExtrapolationMethod.MakeZero Then
                    Return 0
                ElseIf ExtrapolationAbove = enmExtrapolationMethod.KeepConstant Then
                    Return XValues.Values(XValues.Values.Count - 1)
                ElseIf ExtrapolationAbove = enmExtrapolationMethod.ExtrapolateLinear AndAlso XValues.Count > 1 Then
                    Return Me.setup.GeneralFunctions.Interpolate(Values1.Values(Values1.Values.Count - 1), XValues.Values(XValues.Values.Count - 1), Values1.Values(Values1.Values.Count - 2), XValues.Values(XValues.Values.Count - 2), Val,, True)
                Else
                    Me.setup.Log.AddError("Error interpolating from table: " & ID & ". Interpolate value " & Val & " failed.")
                    Return 0
                End If
            ElseIf pdin1 = 0 Then
                'linear interpolation
                If SearchDict.Count = 1 Then
                    Return XValues.Values(0)
                Else
                    For i = 0 To SearchDict.Values.Count - 2
                        If SearchDict.Values(i) <= Val <= SearchDict.Values(i + 1) Then
                            'interpoleer lineair
                            Return setup.GeneralFunctions.Interpolate(SearchDict.Values(i), XValues.Values(i), SearchDict.Values(i + 1), XValues.Values(i + 1), Val)
                        End If
                    Next
                End If
            ElseIf pdin1 = 1 Then
                'block interpolation
                For i = 0 To SearchDict.Values.Count - 1
                    If SearchDict.Values(i) > Val Then
                        Return XValues.Values(i)
                    End If
                Next
                Return XValues.Values(XValues.Values.Count - 1)
            End If
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function InterpolateXValueFromValues of class clsSobekTable.")
            Return Nothing
        End Try

    End Function

    Friend Function InterpolateFromDates(ByVal myDate As Date, Optional ByVal valIdx As Integer = 1) As Single
        Dim i As Integer
        If valIdx < 0 Then valIdx = 1
        If valIdx > 6 Then valIdx = 6

        'bepaal eerst uit welke tabel we waarden gaan teruggeven
        Dim ReturnDict As Dictionary(Of String, Single) = Nothing
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

    Friend Function InterpolateFromXValues(ByVal Xval As Double, Optional ByVal ValIdx As Integer = 1, Optional ByVal ExtrapolateBelow As GeneralFunctions.enmExtrapolationMethod = enmExtrapolationMethod.MakeZero, Optional ByVal ExtrapolateAbove As GeneralFunctions.enmExtrapolationMethod = enmExtrapolationMethod.KeepConstant) As Single
        Try
            Dim i As Integer
            If ValIdx < 0 Then ValIdx = 1
            If ValIdx > 6 Then ValIdx = 6

            'bepaal eerst uit welke tabel we waarden gaan teruggeven
            Dim ReturnDict As Dictionary(Of String, Single) = Nothing
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
                If ExtrapolateBelow = enmExtrapolationMethod.MakeZero Then
                    Return 0
                ElseIf ExtrapolateBelow = enmExtrapolationMethod.KeepConstant Then
                    Return ReturnDict.Values(0)
                ElseIf ExtrapolateBelow = enmExtrapolationMethod.ExtrapolateLinear AndAlso XValues.Count > 1 Then
                    Return Me.setup.GeneralFunctions.Interpolate(XValues.Values(0), ReturnDict.Values(0), XValues.Values(1), ReturnDict.Values(1), Xval,, True)
                Else
                    Me.setup.Log.AddError("Error extrapolating value " & Xval & " from table " & ID & ".")
                    Return 0
                End If
            ElseIf Xval >= XValues.Values(XValues.Count - 1) Then
                If ExtrapolateAbove = enmExtrapolationMethod.MakeZero Then
                    Return 0
                ElseIf ExtrapolateAbove = enmExtrapolationMethod.KeepConstant Then
                    Return ReturnDict.Values(ReturnDict.Values.Count - 1)
                ElseIf ExtrapolateAbove = enmExtrapolationMethod.ExtrapolateLinear AndAlso XValues.Count > 1 Then
                    Return Me.setup.GeneralFunctions.Interpolate(XValues.Values(XValues.Values.Count - 1), ReturnDict.Values(ReturnDict.Values.Count - 1), XValues.Values(XValues.Values.Count - 2), ReturnDict.Values(ReturnDict.Values.Count - 2), Xval,, True)
                Else
                    Me.setup.Log.AddError("Error extrapolating value " & Xval & " from table " & ID & ".")
                    Return 0
                End If
            ElseIf pdin1 = 0 Then
                'linear interpolation
                If XValues.Count < 2 Then
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
            ElseIf pdin1 = 1 Then
                'block interpolation
                For i = 0 To XValues.Values.Count - 1
                    If Xval >= XValues.Values(i) Then
                        Return ReturnDict.Values(i)
                    End If
                Next
                Return ReturnDict.Values(ReturnDict.Values.Count - 1)
            End If

        Catch ex As Exception
            Me.setup.Log.AddError("Error in function InterpolateFromXValues of class clsSobekTable.")
            Return Nothing
        End Try

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

    Friend Sub ReadFast(ByVal myTable As String)
        'NOTE: THIS ROUTINE SEEMS NOT TO BE MUCH FSTER THAN THE ONE ABOVE
        Dim myRecords() As String
        Dim myRecord() As String
        Dim myDate As DateTime

        'verwijder alle tokens
        myTable = Replace(myTable, "TBLE", "")
        myTable = Replace(myTable, "tble", "")
        myTable = Replace(myTable, vbTab, " ") 'replace tabs by spaces
        While InStr(myTable, "  ") > 0
            myTable = Replace(myTable, "  ", " ") 'replace double spaces by single spaces
        End While

        myRecords = Split(myTable, "<")
        For i As Integer = 0 To UBound(myRecords) - 1
            myRecord = Split(myRecords(i).Trim, " ")
            If InStr(myRecord(0), "/") > 0 Then
                myRecord(0) = Replace(myRecord(0), "'", "")
                myDate = Me.setup.GeneralFunctions.ConvertToDateTime(myRecord(0), "yyyy/MM/dd;HH:mm:ss")
            Else
                XValues.Add(Str(i).Trim, myRecord(0))
            End If
            Values1.Add(Str(i).Trim, myRecord(1))
            If myRecord.Count > 2 AndAlso IsNumeric(myRecord(2)) Then Values2.Add(Str(i).Trim, myRecord(2))
            If myRecord.Count > 3 AndAlso IsNumeric(myRecord(3)) Then Values3.Add(Str(i).Trim, myRecord(3))
            If myRecord.Count > 4 AndAlso IsNumeric(myRecord(4)) Then Values4.Add(Str(i).Trim, myRecord(4))
            If myRecord.Count > 5 AndAlso IsNumeric(myRecord(5)) Then Values5.Add(Str(i).Trim, myRecord(5))
            If myRecord.Count > 6 AndAlso IsNumeric(myRecord(6)) Then Values6.Add(Str(i).Trim, myRecord(6))
            If myRecord.Count > 7 AndAlso IsNumeric(myRecord(7)) Then Values6.Add(Str(i).Trim, myRecord(7))
            If myRecord.Count > 8 AndAlso IsNumeric(myRecord(8)) Then Values6.Add(Str(i).Trim, myRecord(8))
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

    Friend Sub BuildFromTargetLevels(ByVal WP As Double, ByVal ZP As Double)

        pdin1 = 1
        pdin2 = 1
        PDINPeriod = "365;00:00:00"

        AddDatevalPair(New Date(2000, 1, 1), WP)
        AddDatevalPair(New Date(2000, 4, 15), ZP)
        AddDatevalPair(New Date(2000, 10, 15), WP)

    End Sub

    Friend Sub AddDataPairKeyByCount(ByVal nVals As Integer, ByVal xval As Double, ByVal val1 As Double, Optional ByVal val2 As Double = 0,
                         Optional ByVal val3 As Double = 0, Optional ByVal val4 As Double = 0,
                         Optional ByVal val5 As Double = 0, Optional ByVal val6 As Double = 0,
                         Optional ByVal val7 As Double = 0, Optional ByVal val8 As Double = 0)
        Dim myStr As String = Str(XValues.Count)

        'voegt data aan de tabel toe waarbij de key telkens gelijk is aan het aantal items in de tabel
        'dit zoekt TRAAG, maar maakt het mogelijk om meerdere malen dezelfde X-waarde in de tabel te hebben
        If Not XValues.ContainsKey(myStr) AndAlso Not Values1.ContainsKey(myStr) Then
            If nVals >= 1 Then XValues.Add(myStr, xval)
            If nVals >= 2 Then Values1.Add(myStr, val1)
            If nVals >= 3 Then Values2.Add(myStr, val2)
            If nVals >= 4 Then Values3.Add(myStr, val3)
            If nVals >= 5 Then Values4.Add(myStr, val4)
            If nVals >= 6 Then Values5.Add(myStr, val5)
            If nVals >= 7 Then Values6.Add(myStr, val6)
            If nVals >= 8 Then Values6.Add(myStr, val7)
            If nVals >= 9 Then Values6.Add(myStr, val8)
        End If
    End Sub

    Friend Sub AddDataPair(ByVal nVals As Integer, ByVal xval As Double, ByVal val1 As Double, Optional ByVal val2 As Double = 0,
                           Optional ByVal val3 As Double = 0, Optional ByVal val4 As Double = 0,
                           Optional ByVal val5 As Double = 0, Optional ByVal val6 As Double = 0, Optional ByVal val7 As Double = 0, Optional ByVal val8 As Double = 0, Optional ByVal ForceUniqueKey As Boolean = False)

        Dim myStr As String
        If ForceUniqueKey Then
            myStr = Str(XValues.Count)
        Else
            myStr = Str(Math.Round(xval, 10)).Trim
        End If
        'Dim myStr As String = Str(XValues.Count)

        '20121002 siebe: als key een string van de x-waarde ingezet om het zoeken in de tabel sneller te laten lopen
        '20130913 siebe: als key toch maar xvalues.count ingebracht omdat twee records met dezelfde x-waarden anders niet worden ondersteund
        '20131004 siebe: als key toch maar weer de x-waarde ingebracht omdat de zoeksnelheid anders een drama wordt
        If Not XValues.ContainsKey(myStr) AndAlso Not Values1.ContainsKey(myStr) Then
            If nVals >= 1 Then XValues.Add(myStr, xval)
            If nVals >= 2 Then Values1.Add(myStr, val1)
            If nVals >= 3 Then Values2.Add(myStr, val2)
            If nVals >= 4 Then Values3.Add(myStr, val3)
            If nVals >= 5 Then Values4.Add(myStr, val4)
            If nVals >= 6 Then Values5.Add(myStr, val5)
            If nVals >= 7 Then Values6.Add(myStr, val6)
            If nVals >= 8 Then Values7.Add(myStr, val7)
            If nVals >= 9 Then Values8.Add(myStr, val8)
        End If

    End Sub

    Friend Sub ShiftXValues(ByVal ShiftByVal As Double)
        For Each myXVal As Double In XValues.Values
            myXVal += ShiftByVal
        Next
    End Sub

    Friend Sub AddDate(ByVal myDate As DateTime)
        Dim mystr As String = Str(Dates.Count).Trim
        Dates.Add(mystr, myDate)
    End Sub

    Friend Sub AddValue1(ByVal myVal As Double)
        Dim myStr As String = Str(Values1.Count).Trim
        Values1.Add(myStr, myVal)
    End Sub

    Friend Sub AddValue2(ByVal myVal As Double)
        Dim myStr As String = Str(Values2.Count).Trim
        Values2.Add(myStr, myVal)
    End Sub

    Friend Sub AddDatevalPair(ByVal myDate As DateTime, ByVal val1 As Double, Optional ByVal val2 As Double = 0,
                              Optional ByVal val3 As Double = 0, Optional ByVal val4 As Double = 0,
                              Optional ByVal val5 As Double = 0, Optional ByVal val6 As Double = 0, Optional ByVal val7 As Double = 0, Optional ByVal val8 As Double = 0, Optional ByVal nVals As Integer = 1)
        Dim myStr As String = Str(Dates.Count).Trim
        Dates.Add(myStr, myDate)
        Select Case nVals
            Case Is = 1
                Values1.Add(myStr, val1)
            Case Is = 2
                Values1.Add(myStr, val1)
                Values2.Add(myStr, val2)
            Case Is = 3
                Values1.Add(myStr, val1)
                Values2.Add(myStr, val2)
                Values3.Add(myStr, val3)
            Case Is = 4
                Values1.Add(myStr, val1)
                Values2.Add(myStr, val2)
                Values3.Add(myStr, val3)
                Values4.Add(myStr, val4)
            Case Is = 5
                Values1.Add(myStr, val1)
                Values2.Add(myStr, val2)
                Values3.Add(myStr, val3)
                Values4.Add(myStr, val4)
                Values5.Add(myStr, val5)
            Case Is = 6
                Values1.Add(myStr, val1)
                Values2.Add(myStr, val2)
                Values3.Add(myStr, val3)
                Values4.Add(myStr, val4)
                Values5.Add(myStr, val5)
                Values6.Add(myStr, val6)
            Case Is = 7
                Values1.Add(myStr, val1)
                Values2.Add(myStr, val2)
                Values3.Add(myStr, val3)
                Values4.Add(myStr, val4)
                Values5.Add(myStr, val5)
                Values6.Add(myStr, val6)
                Values7.Add(myStr, val7)
            Case Is = 8
                Values1.Add(myStr, val1)
                Values2.Add(myStr, val2)
                Values3.Add(myStr, val3)
                Values4.Add(myStr, val4)
                Values5.Add(myStr, val5)
                Values6.Add(myStr, val6)
                Values7.Add(myStr, val7)
                Values8.Add(myStr, val8)
        End Select

    End Sub

    Friend Sub UpdateDateValPair(ByVal myDate As DateTime, ByVal val1 As Double, Optional ByVal val2 As Double = 0,
                              Optional ByVal val3 As Double = 0, Optional ByVal val4 As Double = 0,
                              Optional ByVal val5 As Double = 0, Optional ByVal val6 As Double = 0, Optional ByVal val7 As Double = 0, Optional ByVal val8 As Double = 0, Optional ByVal nVals As Integer = 1)
        Dim i As Long, mykey As String
        For i = 0 To Dates.Count - 1
            If Dates.Values(i) = myDate Then
                mykey = Dates.Keys(i)
                If nVals >= 1 Then Values1.Item(mykey) = val1
                If nVals >= 2 Then Values2.Item(mykey) = val2
                If nVals >= 3 Then Values3.Item(mykey) = val3
                If nVals >= 4 Then Values4.Item(mykey) = val4
                If nVals >= 5 Then Values5.Item(mykey) = val5
                If nVals >= 6 Then Values6.Item(mykey) = val6
                If nVals >= 7 Then Values6.Item(mykey) = val7
                If nVals >= 8 Then Values6.Item(mykey) = val8
                Exit Sub
            End If
        Next

    End Sub


    Friend Sub UpdateDateValPairByKey(ByVal myKey As String, ByVal val1 As Double, Optional ByVal val2 As Double = 0,
                              Optional ByVal val3 As Double = 0, Optional ByVal val4 As Double = 0,
                              Optional ByVal val5 As Double = 0, Optional ByVal val6 As Double = 0, Optional ByVal val7 As Double = 0, Optional ByVal val8 As Double = 0, Optional ByVal nVals As Integer = 1)
        If nVals >= 1 Then Values1.Item(myKey) = val1
        If nVals >= 2 Then Values2.Item(myKey) = val2
        If nVals >= 3 Then Values3.Item(myKey) = val3
        If nVals >= 4 Then Values4.Item(myKey) = val4
        If nVals >= 5 Then Values5.Item(myKey) = val5
        If nVals >= 6 Then Values6.Item(myKey) = val6
        If nVals >= 7 Then Values6.Item(myKey) = val7
        If nVals >= 8 Then Values6.Item(myKey) = val8
    End Sub

    Friend Function getValue1(ByVal XVal As Double) As Double
        'interpoleert een waarde uit de XValues/Values1-dataset
        Dim i As Integer
        Dim myStr As String = Str(Math.Round(XVal, 10)).Trim

        If Values1.ContainsKey(myStr) Then Return Values1.Item(myStr)

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

    Public Function getCenterIdx(DictNum As Integer) As Integer
        'retrieves the index for the value that is closest to the centervalue of the list
        Dim myDict As Dictionary(Of String, Single)
        myDict = getDictionary(DictNum)
        Dim myMin As Double = 9.0E+99
        Dim myIdx As Integer = -1

        Dim startval As Double, endval As Double, targetval As Double
        startval = myDict.Values(0)
        endval = myDict.Values(myDict.Values.Count - 1)
        targetval = (endval + startval) / 2
        For i = 0 To myDict.Count - 1
            If Math.Abs(myDict.Values(i) - targetval) < myMin Then
                myMin = Math.Abs(myDict.Values(i) - targetval)
                myIdx = i
            End If
        Next
        Return myIdx
    End Function

    Public Function getIdxFromValue(ByVal DictNum As Integer, ByVal myVal As Double) As Integer
        'Author: Siebe Bosch
        'Date: 17-6-2013
        'Description: Finds the indexnumber for the record that closest matches a given value in a given table
        Dim i As Integer, minDist As Double = 9999999999, Dist As Double
        Dim myIdx As Integer

        Dim myDict As Dictionary(Of String, Single)
        myDict = getDictionary(DictNum)

        For i = 0 To myDict.Count - 1
            Dist = Math.Abs(myDict.Values(i) - myVal)
            If Dist < minDist Then
                myIdx = i
                minDist = Dist
            End If
        Next

        Return myIdx

    End Function

    Public Function getNearestMaxFromValues1(ByVal StartIdx As Integer) As Integer
        'Author: Siebe Bosch
        'Date: 17-6-2013
        'Description: Finds the indexnumber for the maximumvalue from Values1 that lies closest to a given startindex
        Dim minDist As Double = 99999999
        Dim prevVal As Double, curVal As Double, nextVal As Double
        Dim curDist As Double, PeakIdx As Integer = StartIdx
        Dim i As Long

        For i = 1 To Values1.Values.Count - 2
            prevVal = Values1.Values(i - 1)
            curVal = Values1.Values(i)
            nextVal = Values1.Values(i + 1)
            If curVal > prevVal AndAlso curVal > nextVal Then
                curDist = Math.Abs(XValues.Values(i) - XValues.Values(StartIdx))
                If curDist < minDist Then
                    PeakIdx = i
                    minDist = curDist
                End If
            End If
        Next

        Return PeakIdx

    End Function

    Public Function getFrontValleyIdx(ByVal ListNumber As Integer, ByVal StartIdx As Integer, ByVal minDist As Double) As Integer
        Dim prevVal As Double, curVal As Double, nextVal As Double, curDist As Double
        Dim PeakIdx As Integer = StartIdx
        Dim myDict As New Dictionary(Of String, Single)
        Dim i As Long

        Select Case ListNumber
            Case Is = 0
                myDict = XValues
            Case Is = 1
                myDict = Values1
            Case Is = 2
                myDict = Values2
            Case Is = 3
                myDict = Values3
            Case Is = 4
                myDict = Values4
            Case Is = 5
                myDict = Values5
            Case Is = 6
                myDict = Values6
            Case Is = 7
                myDict = Values7
            Case Is = 8
                myDict = Values8
        End Select

        If StartIdx > 0 Then
            For i = StartIdx - 1 To 0 Step -1
                prevVal = myDict.Values(i + 1)
                curVal = myDict.Values(i)
                nextVal = myDict.Values(i - 1)
                curDist = Math.Abs(XValues.Values(StartIdx) - XValues.Values(i))
                If curVal <= prevVal AndAlso curVal < nextVal AndAlso curDist >= minDist Then
                    Return i
                End If
            Next
        End If
        Return 0

    End Function

    Public Function getBackValleyIdx(ByVal ListNumber As Integer, ByVal StartIdx As Integer, ByVal minDist As Double) As Integer
        Dim prevVal As Double, curVal As Double, nextVal As Double, curDist As Double
        Dim PeakIdx As Integer = StartIdx
        Dim myDict As New Dictionary(Of String, Single)
        Dim i As Long

        Select Case ListNumber
            Case Is = 0
                myDict = XValues
            Case Is = 1
                myDict = Values1
            Case Is = 2
                myDict = Values2
            Case Is = 3
                myDict = Values3
            Case Is = 4
                myDict = Values4
            Case Is = 5
                myDict = Values5
            Case Is = 6
                myDict = Values6
            Case Is = 7
                myDict = Values7
            Case Is = 8
                myDict = Values8
        End Select

        If StartIdx < XValues.Count - 1 Then
            For i = StartIdx + 1 To XValues.Count - 1
                prevVal = Values1.Values(i - 1)
                curVal = Values1.Values(i)
                nextVal = Values1.Values(i + 1)
                curDist = Math.Abs(XValues.Values(StartIdx) - XValues.Values(i))
                If curVal <= prevVal AndAlso curVal < nextVal AndAlso curDist >= minDist Then
                    Return i
                End If
            Next
        End If
        Return XValues.Count - 1

    End Function

    Public Function MovingAverageValues1() As clsSobekTable
        Dim newTable = New clsSobekTable(Me.setup)
        Dim myAvg As Double
        Dim i As Long

        'simply add the values that don't add up to complete moving averages
        For i = 0 To 0
            myAvg = Values1.Values(i)
            newTable.AddDataPair(2, XValues.Values(i), myAvg)
        Next

        'calculate the actual moving average
        For i = 1 To XValues.Count - 2
            myAvg = (Values1.Values(i - 1) + Values1.Values(i) + Values1.Values(i + 1)) / 3
            newTable.AddDataPair(2, XValues.Values(i), myAvg)
        Next

        'simply add the values that don't add up to complete moving averages
        For i = XValues.Count - 1 To XValues.Count - 1
            myAvg = Values1.Values(i)
            newTable.AddDataPair(2, XValues.Values(i), myAvg)
        Next

        Return newTable
    End Function

    Public Function GetLastValue(ByVal ValuesListNum As Integer) As Double
        Dim myValues As Dictionary(Of String, Single)
        Dim Idx As Long

        Select Case ValuesListNum
            Case Is = 0
                myValues = XValues
            Case Is = 1
                myValues = Values1
            Case Is = 2
                myValues = Values2
            Case Is = 3
                myValues = Values3
            Case Is = 4
                myValues = Values4
            Case Is = 5
                myValues = Values5
            Case Is = 6
                myValues = Values6
            Case Is = 7
                myValues = Values7
            Case Is = 8
                myValues = Values8
            Case Else
                Return 0
        End Select

        Idx = myValues.Count - 1
        Return myValues.Values(Idx)

    End Function

    Public Function getMaxValueIdxFromStartPoint(ByVal ValuesListNum As Integer, ByVal StartIdx As Long, ByVal Left As Boolean, ByVal Right As Boolean) As Long

        Dim i As Long, myMaxLeft As Double = -99999999999, myMaxRight As Double = -9999999999, maxLeftIdx As Long = 0, maxRightIdx As Long = 0
        Dim myValues As Dictionary(Of String, Single)

        Select Case ValuesListNum
            Case Is = 1
                myValues = Values1
            Case Is = 2
                myValues = Values2
            Case Is = 3
                myValues = Values3
            Case Is = 4
                myValues = Values4
            Case Is = 5
                myValues = Values5
            Case Is = 6
                myValues = Values6
            Case Is = 7
                myValues = Values7
            Case Is = 8
                myValues = Values8
            Case Else
                Return 0
        End Select

        If Left Then
            For i = StartIdx - 1 To 0 Step -1
                If myValues.Values(i) > myMaxLeft Then
                    myMaxLeft = myValues.Values(i)
                    maxLeftIdx = i
                End If
            Next
        End If

        If Right Then
            For i = StartIdx + 1 To myValues.Values.Count - 1
                If myValues.Values(i) > myMaxRight Then
                    myMaxRight = myValues.Values(i)
                    maxRightIdx = i
                End If
            Next
        End If

        If Left And Right Then
            If myMaxLeft > myMaxRight Then
                Return maxLeftIdx
            Else
                Return maxRightIdx
            End If
        ElseIf Left Then
            Return maxLeftIdx
        ElseIf Right Then
            Return maxRightIdx
        End If

    End Function

    Public Function getRightMinSlopeIdx(ByVal StartIdx As Long, ByVal XValuesDictionaryNumber As Integer, ByVal YValuesDictionaryNumber As Integer, ByRef mySlope As Double) As Long

        'returns the indexnumber for the location in the table with the minimum gradient ((y2-y1)/(x2-x1))
        'starting at a given indexpoint
        Dim XValues As New Dictionary(Of String, Single)
        Dim YValues As New Dictionary(Of String, Single)
        Dim Min As Double = 9999999999
        Dim minIdx As Integer
        Dim dX As Double, dY As Double
        Dim i As Long

        Select Case XValuesDictionaryNumber
            Case Is = 1
                XValues = Values1
            Case Is = 2
                XValues = Values2
            Case Is = 3
                XValues = Values3
            Case Is = 4
                XValues = Values4
            Case Is = 5
                XValues = Values5
            Case Is = 6
                XValues = Values6
            Case Is = 7
                XValues = Values8
            Case Is = 7
                XValues = Values8
        End Select

        Select Case YValuesDictionaryNumber
            Case Is = 1
                YValues = Values1
            Case Is = 2
                YValues = Values2
            Case Is = 3
                YValues = Values3
            Case Is = 4
                YValues = Values4
            Case Is = 5
                YValues = Values5
            Case Is = 6
                YValues = Values6
            Case Is = 7
                YValues = Values7
            Case Is = 8
                YValues = Values8
        End Select

        For i = StartIdx To XValues.Count - 2
            dX = XValues.Values(i + 1) - XValues.Values(i)
            dY = YValues.Values(i + 1) - YValues.Values(i)
            If Not dX = 0 Then
                If dY / dX > Min Then
                    Min = dY / dX
                    minIdx = i
                    mySlope = Min
                End If
            End If
        Next

        Return minIdx

    End Function


    Public Function getRightToeIdx(ByVal StartIdx As Long, ByVal XValuesDictionaryNumber As Integer, ByVal YValuesDictionaryNumber As Integer) As Long

        'specially for dike profiles
        'returns the indexnumber for the location in the table where the dike's toe is located (dy/dx > 0)
        'starting at a given indexpoint
        'if not found, the last value is returned
        Dim XValues As New Dictionary(Of String, Single)
        Dim YValues As New Dictionary(Of String, Single)
        Dim dX As Double, dY As Double
        Dim i As Long

        Select Case XValuesDictionaryNumber
            Case Is = 1
                XValues = Values1
            Case Is = 2
                XValues = Values2
            Case Is = 3
                XValues = Values3
            Case Is = 4
                XValues = Values4
            Case Is = 5
                XValues = Values5
            Case Is = 6
                XValues = Values6
            Case Is = 7
                XValues = Values7
            Case Is = 8
                XValues = Values8
        End Select

        Select Case YValuesDictionaryNumber
            Case Is = 1
                YValues = Values1
            Case Is = 2
                YValues = Values2
            Case Is = 3
                YValues = Values3
            Case Is = 4
                YValues = Values4
            Case Is = 5
                YValues = Values5
            Case Is = 6
                YValues = Values6
            Case Is = 7
                YValues = Values7
            Case Is = 8
                YValues = Values8
        End Select

        For i = StartIdx To XValues.Count - 2
            dX = XValues.Values(i + 1) - XValues.Values(i)
            dY = YValues.Values(i + 1) - YValues.Values(i)
            If Not dX = 0 Then
                If dY / dX > 0 Then
                    Return i
                End If
            End If
        Next

        'not found, so assume the last point represents the toe
        Return XValues.Count - 1

    End Function

    Public Sub writeToExcelWorkSheet(ByRef ws As ExcelWorksheet, ByVal r As Long, ByVal c As Long, ByVal XValName As String, ByVal Val1Name As String)

        Dim i As Long

        If XValues.Count > 0 Then
            ws.Cells(r, c).Value = XValName
            For i = 0 To XValues.Count - 1
                ws.Cells(r + i + 1, c).Value = XValues.Values(i)
            Next
        End If

        If Values1.Count > 0 Then
            c += 1
            ws.Cells(r, c).Value = Val1Name
            For i = 0 To Values1.Count - 1
                ws.Cells(r + i + 1, c).Value = Values1.Values(i)
            Next
        End If

    End Sub

    Public Sub writeTimeTableContentsToFile(ByRef myWriter As System.IO.StreamWriter, ByVal col1 As Boolean, ByVal col2 As Boolean, ByVal col3 As Boolean, ByVal col4 As Boolean, ByVal col5 As Boolean, ByVal col6 As Boolean, col7 As Boolean, col8 As Boolean)
        'v1.797: actually made this function work at all. Until now it was no good
        Dim i As Long, mystr As String = String.Empty
        For i = 0 To Dates.Count - 1
            mystr &= "'" & Format(Dates.Values(i), "yyyy/MM/dd;HH:mm:ss") & "'"
            If col1 Then mystr &= " " & Values1.Values(i)
            If col2 Then mystr &= " " & Values2.Values(i)
            If col3 Then mystr &= " " & Values3.Values(i)
            If col4 Then mystr &= " " & Values4.Values(i)
            If col5 Then mystr &= " " & Values5.Values(i)
            If col6 Then mystr &= " " & Values6.Values(i)
            If col7 Then mystr &= " " & Values7.Values(i)
            If col8 Then mystr &= " " & Values8.Values(i)
            mystr &= " <" & vbCrLf
        Next
        myWriter.Write(mystr)
    End Sub

    Public Function GetLeftIntersectionWithTable(ByVal ID As String, ByRef Table2 As clsSobekTable, ByVal xDict As Integer, ByVal yDict As Integer) As clsXY
        'date: 21-2-2015
        'author: Siebe Bosch
        'description: this routine searches the first LEFT SIDE intersection of the current sobek table with another table and returns the coordinates
        'to be used to find the intersection of e.g. two YZ-cross sections
        'IMPORTANT: prerequisites are that both tables are aligned and that their center is at Dist=0!!!

        Try
            Dim i As Long, j As Long
            Dim Table1Line As clsLineDefinition
            Dim Table2Line As clsLineDefinition
            Dim Intersection As New clsXY

            'create a shortcut to each of the dictionaries with values for the tables
            Dim xTable1 As Dictionary(Of String, Single) = getDictionary(xDict)
            Dim yTable1 As Dictionary(Of String, Single) = getDictionary(yDict)
            Dim xTable2 As Dictionary(Of String, Single) = Table2.getDictionary(xDict)
            Dim yTable2 As Dictionary(Of String, Single) = Table2.getDictionary(yDict)

            'find the index number for the lowest y-point in both tables
            Dim Table1StartIdx As Long = getIdxFromValue(0, 0)
            Dim Table2StartIdx As Long = Table2.getIdxFromValue(0, 0)

            'walk through all line segments of the current table, in left direction
            For i = Table1StartIdx To 1 Step -1
                Table1Line = New clsLineDefinition(Me.setup)
                Table1Line.Calculate(xTable1.Values(i - 1), yTable1.Values(i - 1), xTable1.Values(i), yTable1.Values(i))

                'walk through all line segments of table2, in left direction and seach for a line intersection with the current line from table1
                For j = Table2StartIdx To 1 Step -1
                    Table2Line = New clsLineDefinition(Me.setup)
                    Table2Line.Calculate(xTable2.Values(j - 1), yTable2.Values(j - 1), xTable2.Values(j), yTable2.Values(j))

                    If Me.setup.GeneralFunctions.LineIntersection(Table1Line.a, Table1Line.b, Table2Line.a, Table2Line.b, Intersection.X, Intersection.Y) Then
                        If Intersection.X >= xTable1.Values(i - 1) AndAlso Intersection.X <= xTable1.Values(i) _
                        AndAlso Intersection.X >= xTable2.Values(j - 1) AndAlso Intersection.X <= xTable2.Values(j) Then
                            Return Intersection
                        End If
                    End If
                Next
            Next

            'if we end up here, no intersection was found. So now it's allowed to extrapolate the leftmost branch and try again
            Table1Line = New clsLineDefinition(Me.setup)
            Table1Line.Calculate(xTable1.Values(0), yTable1.Values(0), xTable1.Values(1), yTable1.Values(1))
            For j = Table2StartIdx To 1 Step -1
                Table2Line = New clsLineDefinition(Me.setup)
                Table2Line.Calculate(xTable2.Values(j - 1), yTable2.Values(j - 1), xTable2.Values(j), yTable2.Values(j))
                If Me.setup.GeneralFunctions.LineIntersection(Table1Line.a, Table1Line.b, Table2Line.a, Table2Line.b, Intersection.X, Intersection.Y) Then
                    If Intersection.X >= xTable2.Values(j - 1) AndAlso Intersection.X <= xTable2.Values(j) Then
                        Return Intersection
                    End If
                End If
            Next

            Throw New Exception("Error finding left side intersection between two sobek tables for object " & ID)
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function GetLeftIntersectionWithTable of class clsSobekTable.")
            Return Nothing
        End Try

    End Function

    Public Function GetRightIntersectionWithTable(ByVal ID As String, ByRef Table2 As clsSobekTable, ByVal xDict As Integer, ByVal yDict As Integer) As clsXY
        'date: 21-2-2015
        'author: Siebe Bosch
        'description: this routine searches the first RIGHT SIDE intersection of the current sobek table with another table and returns the coordinates
        'to be used to find the intersection of e.g. two YZ-cross sections
        'IMPORTANT: prerequisites are that both tables have comparable axes. In case of YZ-tables: both centered around the lowest point, which must have distance = 0

        Try
            Dim i As Long, j As Long
            Dim Table1Line As clsLineDefinition
            Dim Table2Line As clsLineDefinition
            Dim Intersection As New clsXY

            'create a shortcut to each of the dictionaries with values for the tables
            Dim xTable1 As Dictionary(Of String, Single) = getDictionary(xDict)
            Dim yTable1 As Dictionary(Of String, Single) = getDictionary(yDict)
            Dim xTable2 As Dictionary(Of String, Single) = Table2.getDictionary(xDict)
            Dim yTable2 As Dictionary(Of String, Single) = Table2.getDictionary(yDict)

            'find the index number for the lowest y-point in both tables
            Dim Table1StartIdx As Long = getIdxFromValue(0, 0)
            Dim Table2StartIdx As Long = Table2.getIdxFromValue(0, 0)

            'walk through all line segments of the current table, in right direction
            For i = Table1StartIdx To xTable1.Values.Count - 2
                Table1Line = New clsLineDefinition(Me.setup)
                Table1Line.Calculate(xTable1.Values(i), yTable1.Values(i), xTable1.Values(i + 1), yTable1.Values(i + 1))

                'walk through all line segments of table2, in right direction and seach for a line intersection with the current line from table1
                For j = Table2StartIdx To xTable2.Values.Count - 2
                    Table2Line = New clsLineDefinition(Me.setup)
                    Table2Line.Calculate(xTable2.Values(j), yTable2.Values(j), xTable2.Values(j + 1), yTable2.Values(j + 1))

                    If Me.setup.GeneralFunctions.LineIntersection(Table1Line.a, Table1Line.b, Table2Line.a, Table2Line.b, Intersection.X, Intersection.Y) Then
                        If Intersection.X >= xTable1.Values(i) AndAlso Intersection.X <= xTable1.Values(i + 1) _
                        AndAlso Intersection.X >= xTable2.Values(j) AndAlso Intersection.X <= xTable2.Values(j + 1) Then
                            Return Intersection
                        End If
                    End If
                Next
            Next

            'if we end up here, no intersection was found. So now it's allowed to extrapolate the rightmost branch and try again
            Table1Line = New clsLineDefinition(Me.setup)
            Table1Line.Calculate(xTable1.Values(xTable1.Values.Count - 2), yTable1.Values(yTable1.Values.Count - 2), xTable1.Values(xTable1.Values.Count - 1), yTable1.Values(yTable1.Values.Count - 1))
            For j = Table2StartIdx To Table2.XValues.Count - 2
                Table2Line = New clsLineDefinition(Me.setup)
                Table2Line.Calculate(xTable2.Values(j), yTable2.Values(j), xTable2.Values(j + 1), yTable2.Values(j + 1))
                If Me.setup.GeneralFunctions.LineIntersection(Table1Line.a, Table1Line.b, Table2Line.a, Table2Line.b, Intersection.X, Intersection.Y) Then
                    If Intersection.X >= xTable2.Values(j) AndAlso Intersection.X <= xTable2.Values(j + 1) Then
                        Return Intersection
                    End If
                End If
            Next

            Throw New Exception("Error finding left side intersection between two sobek tables for object " & ID)
            Return Nothing
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function GetRightIntersectionWithTable of class clsSobekTable.")
            Return Nothing
        End Try

    End Function

End Class
