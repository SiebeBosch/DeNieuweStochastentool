Imports System.IO
Imports STOCHLIB.General

Public Class clsResultsFileLocation
    Public ID As String
    Public Name As String
    Public Duration As Integer
    Public Results As clsStochastenResultaten
    Public ResultsType As STOCHLIB.GeneralFunctions.enmHydroMathOperation

    Private Setup As clsSetup
    Private Model As clsSimulationModel
    Private Parameter As clsResultsFileParameter

    Public Sub New(ByRef mySetup As clsSetup, ByRef myModel As clsSimulationModel, ByRef myPar As clsResultsFileParameter)
        Setup = mySetup
        Results = New clsStochastenResultaten(Me.Setup, Me)
        Model = myModel
        Parameter = myPar
    End Sub

    Public Function WriteMDB(ByRef con As OleDb.OleDbConnection) As Boolean
        Dim i As Long
        Dim myResult As clsStochastenResultaat

        Try
            'add the missing rows to the database table
            Dim cmd As New OleDb.OleDbCommand
            cmd.Connection = con

            'write the exceedance values per simulation and node to the database
            For i = 0 To Results.ResultsSorted.Values.Count - 2 'we'll leave the last one behind to avoid /0
                myResult = Results.ResultsSorted.Values(i)
                cmd.CommandText = "INSERT INTO RESULTATEN (KLIMAATSCENARIO, DUUR, LOCATIENAAM, RUNID, MAXVAL, MINVAL, AVGVAL) VALUES ('" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "'," & Setup.StochastenAnalyse.Duration.ToString.Trim & ",'" & Name & "'," & myResult.Run.ID & "," & myResult.Max & "," & myResult.Min & "," & myResult.Mean & ");"
                cmd.ExecuteNonQuery()
            Next


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

End Class
