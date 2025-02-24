﻿Imports STOCHLIB.General

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

    Public RunLocalCopy As Boolean = True 'if true, then a copy of the .EXE file will be made in the working directory and run from there. If false, then the original model's path will be used

    Private Setup As clsSetup


    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myId As Integer, ByVal myType As String, ByVal myExec As String, ByVal myArgs As String, ByVal myModelDir As String, ByVal myCaseName As String, ByVal myTempWorkDir As String, RRResultsFiles As String, FlowResultsFiles As String, RTCResultsFiles As String)
        Dim myFile As String
        Setup = mySetup
        Id = myId
        ModelType = DirectCast([Enum].Parse(GetType(GeneralFunctions.enmSimulationModel), myType.Trim.ToUpper), GeneralFunctions.enmSimulationModel)
        Exec = myExec
        Args = myArgs
        ModelDir = myModelDir
        CaseName = myCaseName
        TempWorkDir = myTempWorkDir

        Select Case ModelType
            Case GeneralFunctions.enmSimulationModel.SOBEK
                RunLocalCopy = False
            Case Else
                RunLocalCopy = True
        End Select

        ResultsFiles = New clsResultsFiles(Me.Setup, Me)

        While Not RRResultsFiles = ""
            myFile = Me.Setup.GeneralFunctions.ParseString(RRResultsFiles, ";")
            ResultsFiles.GetAdd(myFile, GeneralFunctions.enmHydroModule.RR)
        End While

        While Not FlowResultsFiles = ""
            myFile = Me.Setup.GeneralFunctions.ParseString(FlowResultsFiles, ";")
            ResultsFiles.GetAdd(myFile, GeneralFunctions.enmHydroModule.FLOW)
        End While

        While Not RTCResultsFiles = ""
            myFile = Me.Setup.GeneralFunctions.ParseString(RTCResultsFiles, ";")
            ResultsFiles.GetAdd(myFile, GeneralFunctions.enmHydroModule.RTC)
        End While

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
