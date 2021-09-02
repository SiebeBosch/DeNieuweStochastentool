Option Explicit On
Imports STOCHLIB.General
Imports System.IO

Public Class clsRRResults

    Friend QLAT As clsHisFileBinaryReader
    Friend UPFLODT As clsHisFileBinaryReader
    Friend OWLVLDT As clsHisFileBinaryReader
    Friend LINK3B As clsHisFileBinaryReader

    Public Unpaved As clsHisFileBinaryReader


    'distinguish all RR network nodes into their respective type
    Public UnpavedNodes As New Dictionary(Of String, ClsRRUnpavedNode)
    Public OpenwaterNodes As New Dictionary(Of String, clsRROpenwaterNode)

    Friend Times As New List(Of Double) 'contains all dates/times for our timeseries
    Friend QSUM As clsDischargeSum  'een hulpmiddel om de totale afvoer te kunnen berekenen

    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
        QSUM = New clsDischargeSum(Me.Setup)
        Unpaved = New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\upflowdt.his", Me.Setup)
    End Sub

    Public Sub Initialize()
        'initializes the RR Results by first making separate collections of all rr nodes with their respective results
        Dim UP As ClsRRUnpavedNode, OW As clsRROpenwaterNode

        For Each myNode As clsRRNodeTPRecord In SbkCase.RRTopo.Nodes.Values
            If myNode.nt.ParentID = "3B_UNPAVED" Then
                UP = New ClsRRUnpavedNode(Me.Setup)
            ElseIf myNode.nt.ParentID = "3B_OPENWATER" Then
                OW = New clsRROpenwaterNode(Me.Setup, myNode.ID, myNode.nm, myNode.X, myNode.Y, myNode.nt)
            End If
        Next

    End Sub

    Public Sub readRRLinkDischarge(Optional ByVal MakeSummary As Boolean = False)
        'This routine reads the discharges on RR all Links in the current case

        Dim IDs As New Dictionary(Of String, String)
        Dim myTable As clsTimeTable
        Dim n As Long, i As Long

        LINK3B = New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\3blinks.his", Me.Setup)
        Me.Setup.GeneralFunctions.UpdateProgressBar("Reading link flows.", 0, 10)

        'first open the file & HIA-file & read the header, which contains all locations
        Call LINK3B.OpenFile()

        'walk through all links and read the discharges into a Timetable
        n = SbkCase.RRTopo.Links.Count
        i = 0

        LINK3B.ReadAllData()

        For Each myLink As clsRRBrchTPRecord In SbkCase.RRTopo.Links.Values
            'read the link flows for this RR link
            myTable = New clsTimeTable(Me.Setup)
            i += 1

            Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)

            LINK3B.ReadAddLocationResults(myLink.ID, "flow", myTable, 0)

            'add the timetable to the RR link and calculate a summary per year
            myLink.Flow = myTable 'add the timetable with flows to the link itself
            myLink.Flow.calculateAnnualSummary(0, GeneralFunctions.enmHydroMathOperation.SUM, True)     'also make a summary by year
        Next

        Call LINK3B.Close()

    End Sub

    Public Sub readRROpenWaterResults(ByVal WaterLevels As Boolean, ByVal Volume As Boolean, ByVal Seepage As Boolean, ByVal Rainfall As Boolean, Optional ByVal SkipfirstTimestepPercentage As Decimal = 0)

        Dim myTable As clsTimeTable

        OWLVLDT = New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\ow_lvldt.his", Me.Setup)
        OWLVLDT.OpenFile()

        'walk through all openwater nodes and read the results
        For Each myNode As clsRRNodeTPRecord In SbkCase.RRTopo.Nodes.Values
            If myNode.nt.ParentID = "3B_OPENWATER" Then

                If WaterLevels Then
                    myTable = New clsTimeTable(Me.Setup)
                    If Not OWLVLDT.ReadAddLocationResults(myNode.ID, "water level", myTable, 0) Then Throw New Exception("Could not read water levels for RR Openwater node " & myNode.ID)
                    myNode.WaterLevels = myTable
                End If

                If Volume Then
                    myTable = New clsTimeTable(Me.Setup)
                    If Not OWLVLDT.ReadAddLocationResults(myNode.ID, "openwater vol", myTable, 0) Then Throw New Exception("Could not read water volumes for RR Openwater node " & myNode.ID)
                    myNode.Volumes = myTable
                End If

                If Seepage Then
                    myTable = New clsTimeTable(Me.Setup)
                    If Not OWLVLDT.ReadAddLocationResults(myNode.ID, "seepage", myTable, 0) Then Throw New Exception("Could not read water volumes for RR Openwater node " & myNode.ID)
                    myNode.Seepage = myTable
                End If

                If Rainfall Then
                    myTable = New clsTimeTable(Me.Setup)
                    If Not OWLVLDT.ReadAddLocationResults(myNode.ID, "rainfall", myTable, 0) Then Throw New Exception("Could not read water volumes for RR Openwater node " & myNode.ID)
                    myNode.Rainfall = myTable
                End If

            End If
        Next

        OWLVLDT.Close()

    End Sub

    Public Sub readRRUnpavedResults(ByVal Volume As Boolean, ByVal Seepage As Boolean, ByVal Rainfall As Boolean, Optional ByVal SkipfirstTimestepPercentage As Decimal = 0)

        Dim myTable As clsTimeTable

        UPFLODT = New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\upflowdt.his", Me.Setup)
        UPFLODT.OpenFile()

        'walk through all openwater nodes and read the results
        For Each myNode As clsRRNodeTPRecord In SbkCase.RRTopo.Nodes.Values

            If myNode.nt.ParentID = "3B_UNPAVED" Then
                If Volume Then
                    myTable = New clsTimeTable(Me.Setup)
                    If Not OWLVLDT.ReadAddLocationResults(myNode.ID, "groundw.volume", myTable, 0) Then Throw New Exception("Could not read water levels for RR Openwater node " & myNode.ID)
                    myNode.Volumes = myTable
                End If

                If Seepage Then
                    myTable = New clsTimeTable(Me.Setup)
                    If Not OWLVLDT.ReadAddLocationResults(myNode.ID, "seepage", myTable, 0) Then Throw New Exception("Could not read water levels for RR Openwater node " & myNode.ID)
                    myNode.Volumes = myTable
                End If

                If Rainfall Then
                    myTable = New clsTimeTable(Me.Setup)
                    If Not OWLVLDT.ReadAddLocationResults(myNode.ID, "rainfall", myTable, 0) Then Throw New Exception("Could not read water levels for RR Openwater node " & myNode.ID)
                    myNode.Volumes = myTable
                End If

            End If
        Next

        UPFLODT.Close()


    End Sub

End Class
