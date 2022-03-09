Imports STOCHLIB.General

Public Class clsResultsFile

    Public FileName As String
    Public FullPath As String
    Public HydroModule As STOCHLIB.GeneralFunctions.enmHydroModule 'RR or FLOW1D or FLOW2D or RTC or WQ


    Public Parameters As New Dictionary(Of String, clsResultsFileParameter)
    Private Setup As clsSetup
    Private Model As clsSimulationModel

    Public Sub New(ByRef mySetup As clsSetup, ByRef myModel As clsSimulationModel, MyModule As STOCHLIB.GeneralFunctions.enmHydroModule, ByVal myFileName As String)
        Setup = mySetup
        FileName = myFileName
        Model = myModel
        HydroModule = MyModule
    End Sub

    Public Function GetAddParameter(ByVal parName As String) As clsResultsFileParameter
        If Not Parameters.ContainsKey(parName.Trim.ToUpper) Then
            Dim myPar As New clsResultsFileParameter(Me.Setup, Model)
            myPar.Name = parName
            Parameters.Add(myPar.Name.Trim.ToUpper, myPar)
        End If
        Return Parameters.Item(parName.Trim.ToUpper)
    End Function

End Class
