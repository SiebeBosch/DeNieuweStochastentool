Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.Research
Imports sds = Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative


Public Class clsDIMR
    Private Setup As clsSetup
    Public DIMRConfig As clsDIMRConfigFile
    Public FlowFM As clsFlowFMComponent

    Public ProjectDir As String

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myProjectDir As String)
        Setup = mySetup
        ProjectDir = myProjectDir
        DIMRConfig = New clsDIMRConfigFile(Me.Setup, ProjectDir, Me)
        DIMRConfig.Read()
        FlowFM = New clsFlowFMComponent(Me.Setup, DIMRConfig)
    End Sub

    Public Function SetProject(myProjectDir As String) As Boolean
        Try
            ProjectDir = myProjectDir
            DIMRConfig = New clsDIMRConfigFile(Me.Setup, ProjectDir, Me)
            FlowFM = New clsFlowFMComponent(Me.Setup, DIMRConfig)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error creating DIMR Project " + ex.Message)
            Return False
        End Try
    End Function

    Public Function ReadAll() As Boolean
        'reads the entire project
        Try
            If Not DIMRConfig.Read() Then Throw New Exception("Error reading DIMR Config File.")

            FlowFM.ReadMDU       'read the MDU file. This contains references to e.g. our _net.nc file we must read
            FlowFM.ReadNetwork()             'read the network file
            FlowFM.ReadObservationPoints()  'read all observation points in the model


            Return True
        Catch ex As Exception
            Return False
            Me.Setup.Log.AddError("Error reading DIMR Project: " & ex.Message)
        End Try
    End Function




End Class
