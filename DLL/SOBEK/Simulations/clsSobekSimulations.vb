Imports STOCHLIB.General
Imports System.IO
Imports System.Windows.Forms

Public Class clsSobekSimulations
    Dim Simulations As New Dictionary(Of Integer, clsSobekSimulation) 'key = event number
    Dim Model As clsSimulationModel
    Dim grEvents As DataGridView
    Dim grResultsFiles As DataGridView
    Friend SimulationDir As String
    Friend MeteoDir As String
    Friend ResultsDir As String
    Friend RemoveRunsWhenDone As Boolean
    Friend EvpFile As String
    Friend WinSumDate As New Date
    Friend SumWinDate As New Date
    Friend SummerFiles As New List(Of String)
    Friend WinterFiles As New List(Of String)
    Dim mySim As clsSobekSimulation


    Private Setup As clsSetup
    Public Sub New(ByRef mySetup As clsSetup, ByRef myModel As clsSimulationModel, ByRef myEvents As DataGridView, ByRef myResultsFiles As DataGridView)
        Setup = mySetup
        Model = myModel
        grEvents = myEvents
        grResultsFiles = myEvents
    End Sub

    Public Function setSummerFiles(Files As List(Of String)) As Boolean
        Try
            SummerFiles = Files
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function setWinterFiles(Files As List(Of String)) As Boolean
        Try
            WinterFiles = Files
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function setSeasons(WinSumMonth As Integer, WinSumDay As Integer, SumWinMonth As Integer, SumWinDay As Integer) As Boolean
        Try
            WinSumDate = New Date(2000, WinSumMonth, WinSumDay)
            SumWinDate = New Date(2000, SumWinMonth, SumWinDay)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub SetEvpFile(myEvpFile As String)
        EvpFile = myEvpFile
    End Sub

    Public Sub SetResultsDir(myDir As String)
        ResultsDir = myDir
    End Sub

    Public Sub SetSimulationDir(myDir As String)
        SimulationDir = myDir
        MeteoDir = SimulationDir & "\METEO\"    'for the meteo files we will create a special subdirectory of the simulation dir
    End Sub

    Public Sub SetRemoveRuns(Remove As Boolean)
        RemoveRunsWhenDone = Remove
    End Sub

    Public Sub PopulateFromRKS(ByRef myRKS As clsRksFile)
        'populate both the datagrid and the list containing events
        grEvents.Rows.Clear()
        Simulations.Clear()
        For Each myEvent As STOCHLIB.clsBuiFile In myRKS.Events.Values
            grEvents.Rows.Add(myEvent.number, myEvent.StartDate, False)
            Simulations.Add(myEvent.number, New clsSobekSimulation(Me.Setup, Me.Model, Me, myEvent, Me.SimulationDir & "\Event" & myEvent.number, Me.ResultsDir & "\Event" & myEvent.number))
        Next
    End Sub

    Public Sub UpdateEventsGrid()
        Dim EventNum As Integer, Simulation As clsSobekSimulation
        If Not ResultsDir Is Nothing AndAlso Directory.Exists(ResultsDir) AndAlso Not SimulationDir Is Nothing AndAlso Directory.Exists(SimulationDir) Then
            For Each myRow As DataGridViewRow In grEvents.Rows
                EventNum = CInt(myRow.Cells(0).Value)
                If Simulations.ContainsKey(EventNum) Then
                    Simulation = Simulations.Item(EventNum)
                    If Directory.Exists(Simulation.ResultsDir) AndAlso Directory.GetFiles(Simulation.ResultsDir).Count > 0 Then
                        myRow.Cells(2).Value = True
                    End If
                End If
            Next
        End If

    End Sub

    Public Sub Add(EventNum As Integer, ByRef mySimulation As clsSobekSimulation)
        Simulations.Add(EventNum, mySimulation)
    End Sub

    Public Function RunAll(nParallel As Integer) As Boolean
        Try
            Dim Ready As Boolean
            Dim i As Integer
            Dim Threads As New List(Of Threading.Thread)

            'walk through all simulations that need to be executed
            For Each mySim As clsSobekSimulation In Simulations.Values

                'find out if there's an empty slot for our simulation. If not, wait for one to become available.
                Ready = False
                If Threads.Count < nParallel Then
                    Ready = True
                Else
                    'wait for one of the active threads to complete before starting a new one
                    While Not Ready
                        Dim nActive As Integer = 0
                        For i = Threads.Count - 1 To 0 Step -1
                            If Not Threads(i).ThreadState = Threading.ThreadState.Stopped Then
                                nActive += 1
                            End If
                        Next
                        If nActive < nParallel Then Ready = True
                    End While
                End If

                'we have found a free spot. start a new thread
                Threads.Add(New Threading.Thread(AddressOf mySim.Run))
                Threads(Threads.Count - 1).Start()
                System.Threading.Thread.Sleep(2000)
            Next

            'finally wait for the remaining threads to be completed
            Ready = False
            While Not Ready
                Ready = True
                For i = 0 To Threads.Count - 1
                    If Threads(i).ThreadState = Threading.ThreadState.Running Then Ready = False
                Next
                System.Threading.Thread.Sleep(2000)
            End While

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error running simulations.")
            Return False
        End Try
    End Function


    Public Function RunFromDatagridSelection(nParallel As Integer) As Boolean
        Try
            Dim EventNum As Integer
            Dim RowNum As Integer = 0
            Dim Ready As Boolean
            Dim i As Integer
            Dim Threads As New List(Of Threading.Thread)

            'walk through all simulations that need to be executed
            For Each myRow As DataGridViewRow In grEvents.SelectedRows
                EventNum = myRow.Cells(0).Value

                'find out if there's an empty slot for our simulation. If not, wait for one to become available.
                Ready = False
                If Threads.Count < nParallel Then
                    Ready = True
                Else
                    'wait for one of the active threads to complete before starting a new one
                    While Not Ready
                        Dim nActive As Integer = 0
                        For i = Threads.Count - 1 To 0 Step -1
                            If Not Threads(i).ThreadState = Threading.ThreadState.Stopped Then
                                nActive += 1
                            End If
                        Next
                        If nActive < nParallel Then Ready = True
                    End While
                End If

                'we have found a free spot. start a new thread
                Threads.Add(New Threading.Thread(AddressOf Simulations.Item(EventNum).Run))
                Threads(Threads.Count - 1).Start()
                System.Threading.Thread.Sleep(2000)
            Next

            'finally wait for the remaining threads to be completed
            Ready = False
            While Not Ready
                Ready = True
                For i = 0 To Threads.Count - 1
                    If Threads(i).ThreadState = Threading.ThreadState.Running Then Ready = False
                Next
                System.Threading.Thread.Sleep(2000)
            End While

            UpdateEventsGrid()                      'after each completed simulation we can update the events grid with the fact that we have new results available

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error running simulations.")
            Return False
        End Try
    End Function


End Class
