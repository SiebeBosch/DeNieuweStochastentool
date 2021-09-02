
Option Explicit On
Imports STOCHLIB.General
Imports System.IO

Public Class clsCFResults
    'Friend QLat As clsHisFile
    Friend QLatBinary As clsHisFileBinaryReader
    'Friend BndFlodt As clsHisFile
    Friend BndFlodtBinary As clsHisFileBinaryReader
    Public CalcPnt As clsHisFileBinaryReader
    Public ReachSeg As clsHisFileBinaryReader
    Public Structures As clsHisFileBinaryReader
    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
        'QLat = New clsHisFile(Me.Setup, Me.SbkCase, "qlat.his")
        QLatBinary = New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\qlat.his", Me.Setup)
        'BndFlodt = New clsHisFile(Me.Setup, Me.SbkCase, "bndflodt.his")
        BndFlodtBinary = New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\bndflodt.his", Me.Setup)
        CalcPnt = New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\calcpnt.his", Me.Setup)
        ReachSeg = New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\reachseg.his", Me.Setup)
        Structures = New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\struc.his", Me.Setup)
    End Sub

    Public Function LateralsByReachToInfoWorks(FolderPath As String) As Boolean
        'this function writes computed lateral flows to a csv file on a reach-by-reach basis
        'in other words: all laterals on a reach are being summed up and the result is being written
        Dim ObjList As New List(Of clsSbkReachObject)
        Dim myObj As clsSbkReachObject
        Dim hisReader As New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\qlat.his", Me.Setup)
        Dim dt As New DataTable
        Dim dtDict As New Dictionary(Of String, DataTable)
        Dim iReach As Integer = 0, nReach As Integer = Me.SbkCase.CFTopo.Reaches.Reaches.Count

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Converting lateral inflows into one table per reach.", 0, nReach, True)
            For Each myReach As clsSbkReach In Me.SbkCase.CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                If myReach.InUse Then
                    ObjList = New List(Of clsSbkReachObject)
                    'start by building a collection of Laterals
                    For Each myObj In myReach.ReachObjects.ReachObjects.Values
                        If myObj.nt = GeneralFunctions.enmNodetype.NodeCFLateral OrElse myObj.nt = GeneralFunctions.enmNodetype.NodeRRCFConnection Then
                            ObjList.Add(myObj)
                        End If
                    Next

                    If ObjList.Count > 0 Then
                        'now that we have the collection, create a sobek table from the influx for the first object
                        Dim dtList As New Dictionary(Of String, DataTable)
                        For Each myObj In ObjList
                            dt = New DataTable
                            dt.Columns.Add("Date")
                            dt.Columns.Add("Value")
                            hisReader.ReadLocationResultsToDataTable(myObj.ID, "Lateral", dt)
                            dtList.Add(myObj.ID.Trim.ToUpper, dt)
                        Next
                        dt = Me.Setup.GeneralFunctions.AggregateDataTablesFromDictionary(dtList, 1)
                        dtDict.Add(myReach.Id, dt)
                    End If
                End If
            Next

            Dim myStr As String
            Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting lateral data.", 0, 10, True)
            Using latWriter As New StreamWriter(FolderPath & "\laterals.csv", False)
                latWriter.WriteLine("Date")
                For i = 0 To dtDict.Values.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, dtDict.Values.Count, True)
                    latWriter.WriteLine(dtDict.Keys(i))
                Next
                For r = 0 To dtDict.Values(0).Rows.Count - 1
                    myStr = Format(dtDict.Values(0).Rows(r)(0), "yyyy-MM-dd hh:mm:ss")
                    myStr = dtDict.Values(0).Rows(r)(0)
                    For i = 0 To dtDict.Values.Count - 1
                        myStr &= "," & dtDict.Values(i).Rows(r)(1)
                    Next
                    latWriter.WriteLine(myStr)
                Next
            End Using
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)


            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error writing Laterals to Infoworks ICM.")
            Return False
        End Try
    End Function

    Public Function LateralsToCSV(FileName As String, AggregateByReach As Boolean) As Boolean
        'this function writes computed lateral flows to a csv file on a reach-by-reach basis
        'in other words: all laterals on a reach are being summed up and the result is being written
        Dim ObjList As New List(Of clsSbkReachObject)
        Dim myObj As clsSbkReachObject
        Dim hisReader As New clsHisFileBinaryReader(Me.SbkCase.CaseDir & "\qlat.his", Me.Setup)
        Dim dt As New DataTable
        Dim dtDict As New Dictionary(Of String, DataTable)
        Dim iReach As Integer = 0, nReach As Integer = Me.SbkCase.CFTopo.Reaches.Reaches.Count

        Try
            Me.Setup.GeneralFunctions.UpdateProgressBar("Converting lateral inflows into one table per reach.", 0, nReach, True)
            For Each myReach As clsSbkReach In Me.SbkCase.CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.Setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                If myReach.InUse Then
                    ObjList = New List(Of clsSbkReachObject)
                    'start by building a collection of Laterals
                    For Each myObj In myReach.ReachObjects.ReachObjects.Values
                        If myObj.nt = GeneralFunctions.enmNodetype.NodeCFLateral OrElse myObj.nt = GeneralFunctions.enmNodetype.NodeRRCFConnection Then
                            ObjList.Add(myObj)
                        End If
                    Next

                    If ObjList.Count > 0 Then
                        'now that we have the collection, create a sobek table from the influx for the first object
                        Dim dtList As New Dictionary(Of String, DataTable)
                        For Each myObj In ObjList
                            dt = New DataTable
                            dt.Columns.Add("Date")
                            dt.Columns.Add("Value")
                            hisReader.ReadLocationResultsToDataTable(myObj.ID, "Lateral", dt)

                            'if we aggregate by reach, then add the datatable to the list that we manage for this reach
                            'else, add it to the datatable dictionary directly
                            If AggregateByReach Then
                                dtList.Add(myObj.ID.Trim.ToUpper, dt)
                            Else
                                dtDict.Add(myObj.ID, dt)
                            End If

                        Next

                        'if we aggregate by reach, merge all datatables from the list into one and add the result to the dictionary
                        If AggregateByReach Then
                            dt = Me.Setup.GeneralFunctions.AggregateDataTablesFromDictionary(dtList, 1)
                            dtDict.Add(myReach.Id, dt)
                        End If
                    End If
                End If
            Next

            Dim myStr As String = ""
            Me.Setup.GeneralFunctions.UpdateProgressBar("Exporting lateral data.", 0, 10, True)
            Using latWriter As New StreamWriter(FileName, False)
                myStr = "Date"
                For i = 0 To dtDict.Values.Count - 1
                    Me.Setup.GeneralFunctions.UpdateProgressBar("", i, dtDict.Values.Count, True)
                    myStr &= "," & dtDict.Keys(i)
                Next
                latWriter.WriteLine(myStr)
                For r = 0 To dtDict.Values(0).Rows.Count - 1
                    myStr = dtDict.Values(0).Rows(r)(0)
                    For i = 0 To dtDict.Values.Count - 1
                        myStr &= "," & dtDict.Values(i).Rows(r)(1)
                    Next
                    latWriter.WriteLine(myStr)
                Next
            End Using
            Me.Setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)

            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error writing Laterals to CSV.")
            Return False
        End Try
    End Function

End Class
