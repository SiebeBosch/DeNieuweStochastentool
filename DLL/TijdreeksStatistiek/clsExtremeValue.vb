Imports STOCHLIB

Public Class clsExtremeValue

    Public Sub New(myDate As Date, newVal As Double)
        EventDate = myDate
        Value = newVal
    End Sub

    Public Sub New(myReturnPeriod As Double, newVal As Double)
        ReturnPeriod = myReturnPeriod
        Value = newVal
    End Sub

    Public EventDate As Date
    Public ReturnPeriod As Double   'the return period as described 
    Public Value As Double          'the value of the variable X that is distributed according to a certain distribution
    Public PPWeibull As Double      'plotting position, expressed as return value (years)
    Public PPGringorten As Double
    Public PPBosLevenbach As Double

End Class
