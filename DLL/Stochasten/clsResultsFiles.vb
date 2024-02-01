Imports STOCHLIB.General

Public Class clsResultsFiles

  Public Files As New Dictionary(Of String, clsResultsFile)
  Private Setup As clsSetup
  Private Model As clsSimulationModel

  Public Sub New(ByRef mySetup As clsSetup, ByRef myModel As clsSimulationModel)
    Setup = mySetup
    Model = myModel
  End Sub

    Public Function GetAdd(ByVal myFile As String, myModule As STOCHLIB.GeneralFunctions.enmHydroModule) As clsResultsFile
        If Not Files.ContainsKey(myFile.Trim.ToUpper) Then Files.Add(myFile.Trim.ToUpper, New clsResultsFile(Me.Setup, Model, myModule, myFile))
        Return Files(myFile.Trim.ToUpper)
    End Function

    Public Function getFourierFile() As clsResultsFile
        For Each myFile As clsResultsFile In Files.Values
            If myFile.FileName.Contains("fou.nc") Then
                Return myFile
            End If
        Next
        Return Nothing
    End Function

End Class
