Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsNodesDatNODERecord

    Public ID As String                     'ID for the node
    Friend ty As Integer                       'water on street 1=reservoir, 2=closed, 3=loss
    Friend ws As Double                     'storage area (manhole)
    Friend ss As Double                     'street storage area
    Friend wl As Double                     'bed level storage reservoir (manhole)
    Friend ml As Double                     'street level
    Friend ni As Integer                    'interpolation over reaches yes/no
    Friend r1 As String                     'reach1 ID
    Friend r2 As String                     'reach2 ID
    Friend ctswPDIN1 As Integer
    Friend ctswPDIN2 As Integer
    Friend ctswPeriodicity As String
    Friend ctssPDIN1 As Integer
    Friend ctssPDIN2 As Integer
    Friend ctssPeriodicity As String
    Public ctswTable As clsSobekTable   'table for storage in well
    Friend ctssTable As clsSobekTable   'table for storage on street
    Friend InUse As Boolean
    Private setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        InUse = True

        'Init classes:
        ctswTable = New clsSobekTable(Me.setup)   'table for storage in well
        ctssTable = New clsSobekTable(Me.setup)   'table for storage on street
    End Sub

    Public Function Clone(NewID As String) As clsNodesDatNODERecord
        Dim newDat As New clsNodesDatNODERecord(Me.setup)
        newDat.ID = NewID
        newDat.InUse = InUse
        newDat.ml = ml
        newDat.ni = ni
        newDat.r1 = r1
        newDat.r2 = r2
        newDat.ss = ss
        newDat.ty = ty
        newDat.wl = wl
        newDat.ws = ws
        Return newDat
    End Function

    Friend Sub Read(ByVal myRecord As String)
        Dim myStr As String, tmp As String
        Dim Pos As Integer, Table As String
        Dim activeToken As String = ""

        'Author: Siebe Bosch
        'Last revision: 26-4-2013
        'Description: reads a NODE-record from Nodes.dat

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case Is = "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ty"
                    ty = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ni"
                    ni = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "r1"
                    r1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "r2"
                    r2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ws"
                    ws = Me.setup.GeneralFunctions.ParseString(myRecord, " ")   'storage of manhole at bottom
                Case Is = "ss"
                    ss = Me.setup.GeneralFunctions.ParseString(myRecord, " ")   'storage on street level
                Case Is = "wl"
                    wl = Me.setup.GeneralFunctions.ParseString(myRecord, " ")   'bottom of the manhole
                Case Is = "ml"
                    ml = Me.setup.GeneralFunctions.ParseString(myRecord, " ")   'street level
                Case Is = "ct"
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "sw" Then
                        activeToken = "ctsw"    'table for storage in well
                    ElseIf tmp = "ss" Then
                        activeToken = "ctss"    'table for storage on street
                    End If
                Case Is = "PDIN"
                    If activeToken = "ctsw" Then
                        ctswPDIN1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")         '1=block, 0=linear
                        ctswPDIN2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")         '0=no periodicity, 1=periodicity
                        ctswPeriodicity = Me.setup.GeneralFunctions.ParseString(myRecord, " ")   'Periodicity (between quotes)
                    ElseIf activeToken = "ctss" Then
                        ctssPDIN1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")         '1=block, 0=linear
                        ctssPDIN2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")         '0=no periodicity, 1=periodicity
                        ctssPeriodicity = Me.setup.GeneralFunctions.ParseString(myRecord, " ")   'Periodicity (between quotes)
                    End If
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "TBLE"
                    Pos = InStr(1, myRecord, "tble")
                    Table = Left(myRecord, Pos - 1)
                    myRecord = Right(myRecord, Len(myRecord) - Pos - 3)
                    If activeToken = "ctsw" Then
                        Call ctswTable.Read(Table)
                    ElseIf activeToken = "ctss" Then
                        Call ctssTable.Read(Table)
                    End If
                Case "node"
                Case "NODE"
                Case Else
                    If Not IsNumeric(myStr) Then Me.setup.Log.AddWarning("Unsupported token " & myStr & " found in nodes.dat")
            End Select
        End While
    End Sub

    Public Function CalcStorageArea(Elevation As Double) As Double
        'this function calculates the amount of storage area for a given elevation value inside the current node
        'this means: well storage + street storage

        Dim StorageArea As Double = 0

        'well storage
        If ctswTable.XValues.Count > 0 Then
            StorageArea += ctswTable.InterpolateFromXValues(Elevation, 1, False, True)
        ElseIf Elevation > wl Then
            StorageArea += ws
        End If

        'street storage
        If ctssTable.XValues.Count > 0 Then
            If ctssPDIN1 = 1 Then 'block interpolation
                For i = 0 To ctssTable.XValues.Count - 1
                    If Elevation > ctssTable.XValues.Values(i) Then
                        'we found our elevation in the table. Exit the loop
                        StorageArea += ctssTable.Values1.Values(i)
                        Exit For
                    End If
                Next
            ElseIf ctssPDIN1 = 0 Then 'linear interpolation from the table.
                StorageArea += ctssTable.InterpolateFromXValues(Elevation, 1, False, True)
            End If
        ElseIf Elevation > ml Then
            StorageArea += ss
        End If
        Return StorageArea
    End Function

    Public Sub Write(ByRef datWriter As StreamWriter)
        Dim myStr As String
        Dim i As Long

        myStr = "NODE id '" & ID & "' ty " & ty
        If ni = 1 Then myStr &= " ni 1 r1 '" & r1 & "' r2 '" & r2 & "'"

        If ctswTable.Values1.Count = 0 AndAlso ctswTable.Values1.Count = 0 Then
            myStr &= " ws " & ws & " wl " & wl 'NOTE: in case of a storage table ws 0 wl 0 caused the table not to be read during computation!!!
            myStr &= " ss " & ss & " ml " & ml & " node"
            datWriter.WriteLine(myStr)
        Else
            If ctswTable.Values1.Count > 0 Then
                'table for storage in node
                myStr &= " ct sw PDIN " & ctswPDIN1 & " " & ctswPDIN2 & " '" & ctswPeriodicity & "' pdin"
                datWriter.WriteLine(myStr)
                datWriter.WriteLine("TBLE")
                For i = 0 To ctswTable.Values1.Count - 1
                    datWriter.WriteLine(Format(ctswTable.XValues.Values(i), "0.00") & " " & Format(ctswTable.Values1.Values(i), "0.00") & " <")
                Next
                datWriter.WriteLine("tble ss " & ss & " ml " & ml & " node")
            End If

            If ctssTable.Values1.Count > 0 Then
                'table for storage on street
                myStr &= " ct ss PDIN " & ctssPDIN1 & " " & ctssPDIN2 & " '" & ctssPeriodicity & "' pdin"
                datWriter.WriteLine(myStr)
                datWriter.WriteLine("TBLE")
                For i = 0 To ctssTable.Values1.Count - 1
                    datWriter.WriteLine(Format(ctssTable.XValues.Values(i), "0.00") & " " & Format(ctssTable.Values1.Values(i), "0.00") & " <")
                Next
                datWriter.WriteLine("tble ss " & ss & " ml " & ml & " node")
            End If
        End If

    End Sub

    Public Sub BuildFromStorageTable(ByVal myID As String, ByRef StorageTable As clsSobekTable)
        Dim i As Long
        ID = myID
        ty = 1
        ml = 999999
        For i = 0 To StorageTable.XValues.Count - 1
            ctswTable.AddDataPair(2, StorageTable.XValues.Values(i), StorageTable.Values1.Values(i))
        Next

    End Sub

End Class
