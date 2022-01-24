Imports STOCHLIB.General

Public Class clsSimulationModel
    'This class describes all simulation models that can be used inside a stochastic analysis

    Public Id As Integer
    Public Exec As String
    Public Args As String
    Public ModelDir As String
    Public CaseName As String
    Public TempWorkDir As String
    Public Boundaries As New List(Of String) 'a list of boundary nodes that need to be filled for this model
    Public ResultsFiles As clsResultsFiles
    Public ModelType As GeneralFunctions.enmSimulationModel

    Private Setup As clsSetup


    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myId As String, ByVal myType As String, ByVal myExec As String, ByVal myArgs As String, ByVal myModelDir As String, ByVal myCaseName As String, ByVal myTempWorkDir As String)
        Setup = mySetup
        Id = myId
        Select Case myType.Trim.ToUpper
            Case Is = "SOBEK"
                ModelType = GeneralFunctions.enmSimulationModel.SOBEK
            Case Is = "CUSTOM"
                ModelType = GeneralFunctions.enmSimulationModel.Custom
            Case Is = "DIMR"
                ModelType = GeneralFunctions.enmSimulationModel.DIMR
            Case Is = "DHYDRO"
                ModelType = GeneralFunctions.enmSimulationModel.DHYDRO
            Case Is = "DHYDROSERVER"
                ModelType = GeneralFunctions.enmSimulationModel.DHYDROSERVER
        End Select
        Exec = myExec
        Args = myArgs
        ModelDir = myModelDir
        CaseName = myCaseName
        TempWorkDir = myTempWorkDir
        ResultsFiles = New clsResultsFiles(Me.Setup, Me)

    End Sub

    Public Function ResultsToTimeTable(ResultsFilePath As String, LocationID As String, Parameter As String, FieldIdx As Integer, Multiplier As Double) As clsTimeTable
        Dim myTable As New clsTimeTable(Me.Setup)
        Select Case ModelType
            Case GeneralFunctions.enmSimulationModel.SOBEK
                Dim myReader As New clsHisFileBinaryReader(ResultsFilePath, Me.Setup)
                myReader.ReadHisHeader()
                myReader.ReadAddLocationResults(LocationID, Parameter, myTable, FieldIdx, Multiplier, True)
            Case Else
                Me.Setup.Log.AddError("Error: postprocessing not yet supported for modeltype " & ModelType.ToString)
        End Select
        Return myTable
    End Function

    Public Function CopyResultsFile(FileName As String, SourceDir As String, TargetDir As String) As String
        If Not System.IO.Directory.Exists(TargetDir) Then System.IO.Directory.CreateDirectory(TargetDir)
        Select Case ModelType
            Case GeneralFunctions.enmSimulationModel.SOBEK
                'in case of sobek we'll also copy the .hia file
                Dim hiaFileName As String = Me.Setup.GeneralFunctions.replaceFileExtensionInPath(FileName, ".his", ".hia")
                Dim hisPath As String = TargetDir & "\" & FileName
                Dim hiaPath As String = TargetDir & "\" & hiaFileName
                System.IO.File.Copy(SourceDir & "\WORK\" & FileName, hisPath, True)
                System.IO.File.Copy(SourceDir & "\WORK\" & hiaFileName, hiaPath, True)
                Return hisPath
        End Select
        Return ""
    End Function

End Class
