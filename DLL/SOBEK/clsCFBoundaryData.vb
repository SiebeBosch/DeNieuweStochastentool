Option Explicit On
Imports STOCHLIB.General
Imports System.IO

Public Class clsCFBoundaryData
    Friend BoundaryDatFLBORecords As clsBoundaryDatFLBORecords
    Friend BoundlatDatBTBLRecords As clsBoundlatDatBTBLRecords 'of clsBoundlatDatBTBLRecord
    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        BoundaryDatFLBORecords = New clsBoundaryDatFLBORecords(Me.Setup, Me.SbkCase)
        BoundlatDatBTBLRecords = New clsBoundlatDatBTBLRecords(Me.Setup, Me.SbkCase)

    End Sub

    Public Function CreateConstantHBoundary(ID As String, H As Double) As Boolean
        Dim BoundDat = New clsBoundaryDatFLBORecord(Me.Setup)
        BoundDat.ty = 0
        BoundDat.ID = ID
        BoundDat.InUse = True
        BoundDat.h_wd = 0
        BoundDat.h_wd0 = H
        BoundDat.h_wt = 0
        Setup.SOBEKData.ActiveProject.ActiveCase.CFData.Data.BoundaryData.BoundaryDatFLBORecords.records.Add(BoundDat.ID.Trim.ToUpper, BoundDat)
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)
        BoundaryDatFLBORecords.AddPrefix(Prefix)
        BoundlatDatBTBLRecords.AddPrefix(Prefix)
    End Sub

    Public Function ExportCSV(Path As String, Optional ByVal Delimiter As String = ";") As Boolean
        Try
            Dim BoundWriter As New StreamWriter(Path)
            BoundWriter.WriteLine("NodeID;NodeName;BoundaryType;Date;Value")
            For Each myNode As clsSbkReachNode In SbkCase.CFTopo.ReachNodes.ReachNodes.Values
                If myNode.InUse Then
                    Select Case myNode.nt
                        Case Is = GeneralFunctions.enmNodetype.NodeCFBoundary
                            If myNode.SubType = GeneralFunctions.enmNodeSubType.HBoundary Then
                                If BoundaryDatFLBORecords.records.ContainsKey(myNode.ID.Trim.ToUpper) Then
                                    Dim BoundDat As clsBoundaryDatFLBORecord = BoundaryDatFLBORecords.records.Item(myNode.ID.Trim.ToUpper)
                                    If BoundDat.h_wd = 0 Then
                                        BoundWriter.WriteLine(myNode.ID & Delimiter & myNode.Name & Delimiter & "H" & Delimiter & "01/01/1900" & Delimiter & BoundDat.h_wd0)
                                    Else
                                        For i = 0 To BoundDat.HWTTable.XValues.Count - 1
                                            BoundWriter.WriteLine(myNode.ID & Delimiter & myNode.Name & Delimiter & "H" & Delimiter & BoundDat.HWTTable.XValues.Values(i) & Delimiter & BoundDat.HWTTable.Values1.Values(i))
                                        Next
                                    End If
                                End If
                            ElseIf myNode.SubType = GeneralFunctions.enmNodeSubType.QBoundary Then
                                If BoundaryDatFLBORecords.records.ContainsKey(myNode.ID.Trim.ToUpper) Then
                                    Dim BoundDat As clsBoundaryDatFLBORecord = BoundaryDatFLBORecords.records.Item(myNode.ID.Trim.ToUpper)
                                    If BoundDat.q_dw = 0 Then
                                        BoundWriter.WriteLine(myNode.ID & Delimiter & myNode.Name & Delimiter & "Q" & Delimiter & "01/01/1900" & Delimiter & BoundDat.q_dw0)
                                    Else
                                        For i = 0 To BoundDat.HWTTable.XValues.Count - 1
                                            BoundWriter.WriteLine(myNode.ID & Delimiter & myNode.Name & Delimiter & "Q" & Delimiter & BoundDat.QDTTable.XValues.Values(i) & Delimiter & BoundDat.QDTTable.Values1.Values(i))
                                        Next
                                    End If
                                End If
                            End If
                    End Select
                End If
            Next
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    ''' <summary>
    ''' TODO: siebe invullen
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub Export(ByVal Append As Boolean, ExportDir As String)

        Try
            Using bounddatwriter = New StreamWriter(ExportDir & "\boundary.dat", Append)
                Using boundlatwriter = New StreamWriter(ExportDir & "\boundlat.dat", Append)
                    Me.BoundaryDatFLBORecords.Write(bounddatwriter)
                    Me.BoundlatDatBTBLRecords.Write(boundlatwriter)
                    bounddatwriter.Close()
                    boundlatwriter.Close()
                End Using
            End Using

        Catch ex As Exception
            Dim log As String = "Error in Export CF boundary data"
            Me.Setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try

    End Sub

    Friend Function isQBoundary(ByVal ID As String) As Boolean

        Dim myRecord As clsBoundaryDatFLBORecord
        myRecord = Me.BoundaryDatFLBORecords.GetByID(ID)
        If Not myRecord Is Nothing Then
            If myRecord.ty = 1 Then
                Return True
            Else
                Return False
            End If
        Else
            Me.Setup.Log.AddWarning("Could not determine boundary type for boundary node " & ID)
            Return False
        End If

    End Function

    Friend Function isHBoundary(ByVal ID As String) As Boolean

        Dim myRecord As clsBoundaryDatFLBORecord
        myRecord = Me.BoundaryDatFLBORecords.GetByID(ID)
        If Not myRecord Is Nothing Then
            If myRecord.ty = 0 Then
                Return True
            Else
                Return False
            End If
        Else
            Me.Setup.Log.AddWarning("Could not determine boundary type for boundary node " & ID)
            Return False
        End If

    End Function

End Class
