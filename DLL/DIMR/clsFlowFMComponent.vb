﻿Imports STOCHLIB.General
Imports System.IO
Public Class clsFlowFMComponent
    Private Setup As clsSetup
    Private DimrConfig As clsDIMRConfigFile

    Friend MDUFile As clsMDUFile                                    'all model settings (e.g. filenames)
    Public Network As clsNetworkFile                                'the complete topological network for our model
    Public ObservationPoints As New Dictionary(Of String, clsXY)    'the location of observation points in our model

    Public Sub New(ByRef mySetup As clsSetup, ByRef myDimrConfig As clsDIMRConfigFile)
        Setup = mySetup
        DimrConfig = myDimrConfig

    End Sub

    Public Function ReadNetwork() As Boolean
        Try
            If Not System.IO.File.Exists(MDUFile.NetFile) Then Throw New Exception("Network topology file not found: " & Network.Path)
            Network = New clsNetworkFile(MDUFile.NetFile, Me.Setup)
            Return Network.Read()
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadMDU() As Boolean
        Try
            'this function reads all relevant content from the .MDU file
            'for now it just retrieves the name of our networkfile
            Dim path As String = DimrConfig.Dir & "\" & DimrConfig.Flow1D.SubDir & "\" & DimrConfig.Flow1D.GetInputFile
            If Not System.IO.File.Exists(path) Then Throw New Exception("Path does not exist: " & path)
            MDUFile = New clsMDUFile(Me.Setup, DimrConfig, path)
            MDUFile.Read()
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error reading MDU file: " & ex.Message)
            Return False
        End Try
    End Function

    Public Function GetHisResultsFileName() As String
        Dim HisResultsFileName As String = Strings.Replace(DimrConfig.Flow1D.GetInputFile(), ".mdu", "_his.nc")
        Return HisResultsFileName
    End Function

    Public Function ReadObservationPoints() As Boolean
        Try
            Dim myLoc As clsXY
            Dim X As Double, Y As Double
            Dim myLine As String
            Dim ID As String, BranchID As String, Chainage As Double

            ObservationPoints = New Dictionary(Of String, clsXY)
            Using iniReader As New StreamReader("d:\GITHUB\PILOT_HENA\dellen_rr_fm_rtc_T1\dflowfm\observationPoints.ini")
                While Not iniReader.EndOfStream
                    myLine = iniReader.ReadLine()
                    If myLine.Trim.ToLower = "[observationpoint]" Then

                        myLine = iniReader.ReadLine()
                        ID = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)

                        myLine = iniReader.ReadLine()
                        BranchID = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)

                        myLine = iniReader.ReadLine()
                        Chainage = Me.Setup.GeneralFunctions.ReadIniFileProperty(myLine)

                        'since the ini file does not contain any information about the network topology we will need to deduct the x and y coords
                        Network.GetCoordsFromChainage(BranchID, Chainage, X, Y)

                        myLoc = New clsXY(ID, ID, X, Y)
                        ObservationPoints.Add(myLoc.ID.Trim.ToUpper, myLoc)

                    End If
                End While
            End Using

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



End Class