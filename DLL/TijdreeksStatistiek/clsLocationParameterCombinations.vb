Imports STOCHLIB.General

Public Class clsLocationParameterCombinations

    Public Combinations As Dictionary(Of String, clsLocationParameterCombination)

    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
        Combinations = New Dictionary(Of String, clsLocationParameterCombination)
    End Sub

    Public Function GetAdd(LocationID As String, Parameter As String) As clsLocationParameterCombination
        Dim myCombi As clsLocationParameterCombination
        Dim myKey As String = LocationID.Trim.ToUpper & "_" & Parameter.Trim.ToUpper
        If Combinations.ContainsKey(myKey) Then
            myCombi = Combinations.Item(myKey)
        Else
            myCombi = New clsLocationParameterCombination(LocationID, Parameter)
            Combinations.Add(myKey, myCombi)
        End If
        Return myCombi
    End Function

End Class
