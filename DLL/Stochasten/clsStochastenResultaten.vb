Imports STOCHLIB.General
Imports System.Windows.Forms
Imports System.IO

Public Class clsStochastenResultaten

    Public Sub New(ByRef mySetup As clsSetup, ByRef myLocation As clsResultsFileLocation)
        Setup = mySetup
        Location = myLocation
    End Sub

    'deze klasse bevat het volledige resultaat van een stochastenanalyse voor één specifieke locatie
    Public Results As New Dictionary(Of String, clsStochastenResultaat)
    Public ResultsSorted As New Dictionary(Of String, clsStochastenResultaat)
    Public ReturnPeriods As New Dictionary(Of Double, Double) 'list of resulting ARI's (herhalingstijden)

    Private Setup As clsSetup
    Private Location As clsResultsFileLocation

    Public Sub AddResult(ByRef myRun As clsStochastenRun, ByVal myResult As clsStochastenResultaat)

        'add the results of this run to the dictionary
        Results.Add(myRun.ID, myResult)

    End Sub

    Public Function calcReturnPeriod(ByVal myReturnPeriod As Double, ByVal Duration As Integer) As Boolean
        Dim i As Long, n As Long, myVal As Double

        Try
            n = ResultsSorted.Count

            If ResultsSorted.Values(0).T >= myReturnPeriod Then
                ReturnPeriods.Add(myReturnPeriod, -999)
                Me.Setup.Log.AddError("Error calculating water level for return period " & myReturnPeriod & " since it is lower than any of the available ones.")
            ElseIf ResultsSorted.Values(n - 1).T <= myReturnPeriod Then
                ReturnPeriods.Add(myReturnPeriod, -999)
                Me.Setup.Log.AddError("Error calculating water level for return period " & myReturnPeriod & " since it exceeds any of the available ones.")
            Else
                For i = 0 To ResultsSorted.Count - 2
                    If ResultsSorted.Values(i).T <= myReturnPeriod AndAlso ResultsSorted.Values(i + 1).T >= myReturnPeriod Then
                        'interpolate linearly (for now). Siebe: add logarithmic trendline between T10 and T100 in the future!
                        myVal = Me.Setup.GeneralFunctions.Interpolate(ResultsSorted.Values(i).T, ResultsSorted.Values(i).Max, ResultsSorted.Values(i + 1).T, ResultsSorted.Values(i + 1).Max, myReturnPeriod)
                        ReturnPeriods.Add(myReturnPeriod, myVal)
                    End If
                Next
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in sub calcReturnPeriod of class clsStochastenResultaten.")
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function




End Class
