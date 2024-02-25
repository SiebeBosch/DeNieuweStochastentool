Imports STOCHLIB.General
Public Class clsParameterClassifications

    Dim Setup As clsSetup
    Dim PercentileClasses As clsPercentileClasses 'contains the list of percentile classes by which we classify the parameters
    Dim Parameter As GeneralFunctions.enmModelParameter

    Public Sub New(ByRef mySetup As clsSetup, myParameter As GeneralFunctions.enmModelParameter, ByRef myPercentileClasses As clsPercentileClasses)
        'constructor
        Setup = mySetup
        Parameter = myParameter
        PercentileClasses = myPercentileClasses
    End Sub

    Public Function Calculate(ByRef Events As clsRainfallEvents, timestepStatistic As GeneralFunctions.enmTimestepStatistic) As Boolean
        'here we calculate the classification for the parameter
        'first, for the given parameter we will create a list of all the values
        Dim Values As New List(Of Double)
        For Each myEvent As clsTimeSeriesEvent In Events.Events.Values
            Values.Add(myEvent.GetParameterValue(Parameter, timestepStatistic))
        Next

        'now loop throug each of the percentile classes and determine which events belong to which class
        For Each percentileClass As clsPercentileClass In PercentileClasses.Classes
            percentileClass.EventNums.Clear()

            'first set the boundary values for the class
            percentileClass.LBoundValue = Setup.GeneralFunctions.PercentileFromList(Values, percentileClass.LBoundPercentile)

            For i = 0 To Values.Count - 1
                If Values(i) >= percentileClass.LBoundValue And Values(i) <= percentileClass.UboundValue Then
                    percentileClass.EventNums.Add(i)
                End If
            Next
        Next

    End Function

    Public Function AddPercentileClass(ByRef PercentileClass As clsPercentileClass) As Boolean
        PercentileClasses.AddClass(PercentileClass)
    End Function


End Class
