Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports GemBox.Spreadsheet

Public Class clsCFLateraldata

  Friend LateralDatFLBRRecords As clsLateralDatFLBRRecords
  Friend LateraldatFLNORecords As clsLateralDatFLNORecords
  Friend NodesDatNODERecords As clsNodesDatNODERecords
  Friend BoundlatDatBTBLRecords As clsBoundlatDatBTBLRecords
  Private setup As clsSetup
  Private SbkCase As clsSobekCase

  Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
    Me.setup = mySetup
    Me.SbkCase = myCase

    ' Init classes:
    LateralDatFLBRRecords = New clsLateralDatFLBRRecords(Me.setup, Me.SbkCase)
    LateraldatFLNORecords = New clsLateralDatFLNORecords(Me.setup, Me.SbkCase)
    NodesDatNODERecords = New clsNodesDatNODERecords(Me.setup, Me.SbkCase)
    BoundlatDatBTBLRecords = New clsBoundlatDatBTBLRecords(Me.setup, Me.SbkCase)

  End Sub

    Public Sub Write(ByRef datWriter As StreamWriter)
        LateralDatFLBRRecords.Write(datWriter)
        LateraldatFLNORecords.Write(datWriter)
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
    LateralDatFLBRRecords.AddPrefix(Prefix)
    LateraldatFLNORecords.AddPrefix(Prefix)
    NodesDatNODERecords.AddPrefix(Prefix)
    BoundlatDatBTBLRecords.AddPrefix(Prefix)
  End Sub

    ''' <summary>
    ''' TODO: Siebe invullen
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub Export(ByVal Append As Boolean, ExportDir As String)
        Try
            Using LateralDatWriter As New StreamWriter(ExportDir & "\lateral.dat", Append)
                Using NodesDatWriter As New StreamWriter(ExportDir & "\nodes.dat", Append)
                    Using BoundLatWriter As New StreamWriter(ExportDir & "\boundlat.dat", Append)
                        Call LateraldatFLNORecords.Write(LateralDatWriter)
                        Call LateralDatFLBRRecords.Write(LateralDatWriter)
                        Call BoundlatDatBTBLRecords.Write(BoundLatWriter)
                        Call NodesDatNODERecords.Write(NodesDatWriter)
                        LateralDatWriter.Close()
                        NodesDatWriter.Close()
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Dim log As String = "Error in Export CF lateral data"
            Me.setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try

    End Sub

    Friend Function BuildLateralDatFLBRRecord(ID As String, Area As Double, Seepage As Double, RainfallStation As String, SetInuse As Boolean) As clsLateralDatFLBRRecord
        Try
            Dim myRecord As New clsLateralDatFLBRRecord(Me.setup, Me.SbkCase)
            myRecord.ID = ID
            myRecord.ar = Area
            myRecord.dclt1 = 7
            myRecord.ii = Seepage
            myRecord.ms = RainfallStation
            myRecord.InUse = SetInuse
            Return myRecord
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function BuildLateralDatFLBRRecord of class clsCFLateralData")
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function


    Friend Function BuildFromTimeTable(ByVal NodeID As String, ByVal InflowTable As clsTimeTable, ByVal FieldIdx As Integer) As Boolean
    Dim i As Long
    Dim Dat As clsLateralDatFLBRRecord
    Dim BLat As clsBoundlatDatBTBLRecord
    Dim myRecord As clsTimeTableRecord

    Dat = LateralDatFLBRRecords.records.Item(NodeID.Trim.ToUpper)
    Dat.ID = NodeID
    Dat.sc = 0
    Dat.lt = 0
    Dat.dclt1 = 11
    Dat.LibTableID = Dat.ID

    If BoundlatDatBTBLRecords.records.ContainsKey(NodeID.Trim.ToUpper) Then
      BLat = BoundlatDatBTBLRecords.records.Item(NodeID.Trim.ToUpper)
    Else
      BLat = New clsBoundlatDatBTBLRecord(Me.setup)
      BLat.ID = NodeID
      BoundlatDatBTBLRecords.records.Add(BLat.ID.Trim.ToUpper, BLat)
    End If

    BLat.sc = 0
    BLat.lt = 0
    BLat.pdin1 = 1
    BLat.pdin2 = 0

    For i = 0 To InflowTable.Records.Count - 1
      myRecord = InflowTable.Records.Values(i)
      BLat.TimeTable.AddDatevalPair(myRecord.Datum, myRecord.Fields.Item(FieldIdx))
    Next


  End Function


End Class
