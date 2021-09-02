Imports STOCHLIB.General
Imports System.IO
Imports System.Diagnostics

Public Class clsSobekRun


    Dim myProcess As Process
    Dim SimulationDir As String
    Dim Exe As String
    Dim Args As String

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, mySimulationDir As String, myExe As String, myArgs As String)
        Setup = mySetup
        SimulationDir = mySimulationDir
        Exe = myExe
        Args = myArgs
    End Sub

    Public Sub Start()
        myProcess = New Process
        myProcess.StartInfo.WorkingDirectory = SimulationDir & "\CMTWORK"
        myProcess.StartInfo.FileName = exe
        myProcess.StartInfo.Arguments = args
        myProcess.Start()
    End Sub



End Class
