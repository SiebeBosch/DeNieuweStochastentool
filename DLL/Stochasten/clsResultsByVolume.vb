Imports STOCHLIB.General

Public Class clsResultsByVolume

    Public Results As New Dictionary(Of String, clsResultByVolume)
    Public Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub Add(ByVal LocationName As String, ByRef myRun As clsStochastenRun, ByVal myMin As Double, ByVal myMax As Double, ByVal myAVG As Double)
        Dim myKey As String = CreateKey(LocationName, myRun) 'a unique key for each combi of location and stochastic run
        Dim myResult As clsResultByVolume

        If Not Results.ContainsKey(myKey) Then
            myResult = New clsResultByVolume(Setup)
            myResult.Key = myKey
            myResult.LocationName = LocationName
            myResult.AddResult(myRun, myMin, myMax, myAVG)
            Results.Add(myKey, myResult)
        Else
            myResult = Results.Item(myKey)
            myResult.AddResult(myRun, myMin, myMax, myAVG)
        End If
    End Sub

    Public Function GetResult(ByVal LocationID As String, ByRef Run As clsStochastenRun) As clsResultByVolume
        Dim myKey As String = CreateKey(LocationID, Run)
        Return Results.Item(myKey)
    End Function

    Public Function GetAsDataTableByLocationAndRunIDexceptVolume(ByVal myLocID As String, ByVal myRunIDexceptVolume As String) As DataTable
        For Each myResult As clsResultByVolume In Results.Values
            If myResult.LocationName = myLocID AndAlso myResult.RunIDexceptVolume = myRunIDexceptVolume Then
                Return myResult.GetAsDataTable
            End If
        Next
        Return Nothing
    End Function

    Public Function GetByLocationAndRunIDexceptVolume(ByVal myLocID As String, ByVal myRunIDexceptVolume As String) As clsResultByVolume
        For Each myResult As clsResultByVolume In Results.Values
            If myResult.LocationName = myLocID AndAlso myResult.RunIDexceptVolume = myRunIDexceptVolume Then
                Return myResult
            End If
        Next
        Return Nothing
    End Function


    Public Function CreateKey(ByVal LocationID As String, ByRef Run As STOCHLIB.clsStochastenRun) As String
        Dim myKey As String
        myKey = LocationID.Trim.ToUpper
        myKey &= "_" & Run.GetIDexceptVolume
        Return myKey
    End Function


End Class
