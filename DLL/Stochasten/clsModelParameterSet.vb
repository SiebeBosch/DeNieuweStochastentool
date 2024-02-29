Public Class clsModelParameterSet

    Dim ParameterValues As Dictionary(Of GeneralFunctions.enmModelParameter, Double)

    Public Sub New()
        ParameterValues = New Dictionary(Of GeneralFunctions.enmModelParameter, Double)
    End Sub

    Public Sub AddParameter(ByVal Parameter As GeneralFunctions.enmModelParameter, ByVal Value As Double)
        If Not ParameterValues.ContainsKey(Parameter) Then ParameterValues.Add(Parameter, Value)
    End Sub

End Class
