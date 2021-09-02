Option Explicit On
Imports STOCHLIB.General
Imports System.IO

Public Class clsCFNodesData
    Friend NodesDatNodeRecords As clsNodesDatNODERecords
    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Public Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        NodesDatNodeRecords = New clsNodesDatNODERecords(Me.setup, Me.SbkCase)

    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        NodesDatNodeRecords.AddPrefix(Prefix)
    End Sub

    Friend Sub CheckAndFix()
        Dim reach1 As clsSbkReach, reach2 As clsSbkReach

        'makes sure all reach interpolations use existing reaches. If not, the interpolation is removed
        For Each myRecord As clsNodesDatNODERecord In NodesDatNodeRecords.records.Values
            If SbkCase.CFTopo.getReachNode(myRecord.ID) Is Nothing Then
                Me.setup.Log.AddWarning("Nodes.dat record for node " & myRecord.ID & " was removed since no such node was found in the network.")
                myRecord.InUse = False
            Else
                If myRecord.ni = 1 Then
                    If myRecord.r1 = "" OrElse myRecord.r2 = "" Then
                        myRecord.ni = 0
                    Else
                        reach1 = SbkCase.CFTopo.Reaches.GetReach(myRecord.r1)
                        reach2 = SbkCase.CFTopo.Reaches.GetReach(myRecord.r2)
                        If reach1 Is Nothing OrElse reach2 Is Nothing Then
                            Me.setup.Log.AddError("Interpolation over connection node " & myRecord.ID & " was removed since one of both surrounding reaches did not exist.")
                            myRecord.ni = 0
                        ElseIf reach1.InUse = False OrElse reach2.InUse = False Then
                            Me.setup.Log.AddWarning("Interpolation over connection node " & myRecord.ID & " was removed since one of both surrounding reaches was excluded from the schematization.")
                            myRecord.ni = 0
                        End If
                    End If
                End If
            End If
        Next
    End Sub

    Friend Sub Read(ByVal myStrings As Collection)
        For Each myString As String In myStrings
            Dim myRecord = New clsNodesDatNODERecord(Me.setup)
            myRecord.Read(myString)
            Me.NodesDatNodeRecords.records.Add(myRecord.ID.Trim.ToUpper, myRecord)
        Next myString
    End Sub

    Public Sub UpdateInterpolationReferencesAfterSplit(ByRef oldReach As clsSbkReach, ByRef newUpReach As clsSbkReach, ByRef newDownReach As clsSbkReach)
        Dim NodesDat As clsNodesDatNODERecord

        'update the upstream connection node
        If NodesDatNodeRecords.records.ContainsKey(oldReach.bn.ID.Trim.ToUpper) Then
            NodesDat = NodesDatNodeRecords.records.Item(oldReach.bn.ID.Trim.ToUpper)
            If NodesDat.ni <> 0 Then
                If NodesDat.r1.Trim.ToUpper = oldReach.Id.Trim.ToUpper Then NodesDat.r1 = newUpReach.Id
                If NodesDat.r2.Trim.ToUpper = oldReach.Id.Trim.ToUpper Then NodesDat.r2 = newUpReach.Id
            End If
        End If

        'update the downstream connection node
        If NodesDatNodeRecords.records.ContainsKey(oldReach.en.ID.Trim.ToUpper) Then
            NodesDat = NodesDatNodeRecords.records.Item(oldReach.en.ID.Trim.ToUpper)
            If NodesDat.ni <> 0 Then
                If NodesDat.r1.Trim.ToUpper = oldReach.Id.Trim.ToUpper Then NodesDat.r1 = newDownReach.Id
                If NodesDat.r2.Trim.ToUpper = oldReach.Id.Trim.ToUpper Then NodesDat.r2 = newDownReach.Id
            End If
        End If

    End Sub


    Public Sub CreateNodesDatNodeRecordForInterpolation(NodeID As String, UpReachID As String, DownReachID As String)
        Dim nodeDat As New clsNodesDatNODERecord(Me.setup)

        If Not NodesDatNodeRecords.records.ContainsKey(NodeID.Trim.ToUpper) Then
            nodeDat = New clsNodesDatNODERecord(Me.setup)
            nodeDat.ID = NodeID
            nodeDat.InUse = True
            nodeDat.ty = 0
            nodeDat.ni = 1
            nodeDat.r1 = UpReachID
            nodeDat.r2 = DownReachID
            NodesDatNodeRecords.records.Add(nodeDat.ID.Trim.ToUpper, nodeDat)
        Else
            nodeDat = NodesDatNodeRecords.records.Item(nodeDat.ID.Trim.ToUpper)
            nodeDat.ID = NodeID
            nodeDat.InUse = True
            nodeDat.ty = 0
            nodeDat.ni = 1
            nodeDat.r1 = UpReachID
            nodeDat.r2 = DownReachID
        End If
    End Sub

    ''' <summary>
    ''' TODO: siebe invullen
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub Export(ByVal Append As Boolean, ExportDir As String)

        Try
            Using datwriter = New StreamWriter(ExportDir & "\nodes.dat", Append)
                CheckAndFix()
                Me.NodesDatNodeRecords.Write(datwriter)
                datwriter.Close()
            End Using

        Catch ex As Exception
            Dim log As String = "Error in Export Flow data to nodes.dat"
            Me.setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try

    End Sub

End Class
