Option Explicit On
Imports STOCHLIB.General
Imports System.IO
Imports System.Windows.Forms

Public Class clsSOBEK
    Friend Dir As String
    Friend Projects As New Dictionary(Of String, clsSobekProject)         'een collectie van SOBEK-modellen (clsSobekProject)
    Public ActiveProject As clsSobekProject
    Friend RRSettings As New clsRRSettings
    Friend CFSettings As New clsCFSettings
    Public DrainDefs As New clsDrainDefs(Me.setup)
    Public CompareSettings As New clsSbkCompareSettings
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup

        'Init classes:
        DrainDefs = New clsDrainDefs(Me.setup)

        Me.Initialize()
    End Sub

    Public Function ReadProject(ByVal ProjectDir As String, ByVal ReadCases As Boolean) As Boolean
        Try
            'remove existing project with the same name
            If Projects.ContainsKey(ProjectDir.Trim.ToUpper) Then Projects.Remove(ProjectDir.Trim.ToUpper)

            'add the project
            If System.IO.Directory.Exists(ProjectDir) Then
                Dim myProject As New clsSobekProject(Me.setup, ProjectDir, ProjectDir, ReadCases)
                Projects.Add(myProject.ProjectDir, myProject)
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function setCasesInUseFromDataGrid(ByRef myGrid As DataGridView, CaseNameColIdx As Integer, InuseColIdx As Integer, ByRef FirstSelectedCase As clsSobekCase) As Boolean
        Try
            'this function walks through all cases inside the active sobek project and sets the cases on Inuse or not, based on a selection in a datagridview
            'read the topology of the first active sobek case
            Dim FirstFound As Boolean = False
            For Each myCase As clsSobekCase In setup.SOBEKData.ActiveProject.Cases.Values
                myCase.InUse = False
                For Each myRow As DataGridViewRow In myGrid.Rows
                    If myCase.CaseName = myRow.Cells(CaseNameColIdx).Value AndAlso myRow.Cells(InuseColIdx).Value = True Then
                        myCase.InUse = True
                        If Not FirstFound Then
                            FirstSelectedCase = myCase
                            FirstFound = True
                        End If
                    End If
                Next
            Next
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function setActiveCasesFromDataGrid.")
            Return False
        End Try
    End Function

    Public Function CreateEmptyProject(myName As String, SetAsActiveProject As Boolean) As clsSobekProject
        Try
            Dim myProject As New clsSobekProject(Me.setup) 'give it a fake path since we need 
            If SetAsActiveProject Then
                myProject.IsActiveModel = True
                ActiveProject = myProject
            End If
            Projects.Add(myName.Trim.ToUpper, myProject)
            Return myProject
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return Nothing
        End Try
    End Function


    Public Sub InitializeExport(ByVal RRData As Boolean, ByVal RRTopo As Boolean, ByVal CFData As Boolean, ByVal CFTopo As Boolean)

        If RRData Then
            'verwijder alle files die we straks ook hier moeten schrijven.
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\unpaved.3b") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\unpaved.3b")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\unpaved.alf") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\unpaved.alf")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\unpaved.sep") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\unpaved.sep")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\unpaved.inf") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\unpaved.inf")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\unpaved.sto") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\unpaved.sto")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\paved.3b") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\paved.3b")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\paved.sto") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\paved.sto")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\paved.dwa") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\paved.dwa")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\bound3b.3b") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\bound3b.3b")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekRR & "\bound3b.tbl") Then System.IO.File.Delete(setup.Settings.ExportDirSobekRR & "\bound3b.tbl")
        End If

        If RRTopo Then
            If System.IO.File.Exists(setup.Settings.ExportDirSobekTopo & "3b_nod.tp") Then System.IO.File.Delete(setup.Settings.ExportDirSobekTopo & "\3b_nod.tp")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekTopo & "3b_link.tp") Then System.IO.File.Delete(setup.Settings.ExportDirSobekTopo & "\3b_link.tp")
            If System.IO.File.Exists(setup.Settings.ExportDirSobekTopo & "network.bbb") Then System.IO.File.Delete(setup.Settings.ExportDirSobekTopo & "\network.bbb")
            Using tpwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\network.bbb", False)
                tpwriter.WriteLine("BBB2.2")
                tpwriter.WriteLine("*")
                tpwriter.WriteLine("* Configuration file")
                tpwriter.WriteLine("* Filename for Rainfall-Runoff:")
                tpwriter.WriteLine("* 1. Node data")
                tpwriter.WriteLine("* 2. Link data")
                tpwriter.WriteLine("* 3. Lateral data")
                tpwriter.WriteLine("*")
                tpwriter.WriteLine("'" & ".\3b_nod.tp" & "'")
                tpwriter.WriteLine("'" & ".\3b_link.tp" & "'")
                tpwriter.Close()
            End Using
            Using tpwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\3b_nod.tp", False)
                tpwriter.WriteLine("BBB2.2")
                tpwriter.Close()
            End Using
            Using tpwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\3b_link.tp", False)
                tpwriter.WriteLine("BBB2.2")
                tpwriter.Close()
            End Using
        End If

        If CFData Then
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\profile.dat") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\profile.dat")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\profile.def") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\profile.def")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\struct.dat") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\struct.dat")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\struct.def") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\struct.def")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\friction.dat") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\friction.dat")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\initial.dat") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\initial.dat")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\control.def") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\control.def")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\lateral.dat") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\lateral.dat")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\boundary.dat") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\boundary.dat")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\boundlat.dat") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\boundlat.dat")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekFlow & "\nodes.dat") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekFlow & "\nodes.dat")
        End If

        If CFTopo Then
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekTopo & "\network.cn") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekTopo & "\network.cn")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekTopo & "\network.tp") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekTopo & "\network.tp")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekTopo & "\network.st") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekTopo & "\network.st")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekTopo & "\network.obi") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekTopo & "\network.obi")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekTopo & "\network.me") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekTopo & "\network.me")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekTopo & "\network.cr") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekTopo & "\network.cr")
            If System.IO.File.Exists(Me.setup.Settings.ExportDirSobekTopo & "\network.cp") Then System.IO.File.Delete(Me.setup.Settings.ExportDirSobekTopo & "\network.cp")

            Using sobwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\network.sob", False)
                sobwriter.WriteLine("SOB1.0")
                sobwriter.WriteLine(Chr(34) & ".\network.tp" & Chr(34))
                sobwriter.WriteLine(Chr(34) & ".\network.cp" & Chr(34))
                sobwriter.WriteLine(Chr(34) & ".\network.cn" & Chr(34))
                sobwriter.WriteLine(Chr(34) & ".\network.cr" & Chr(34))
                sobwriter.WriteLine(Chr(34) & ".\network.me" & Chr(34))
                sobwriter.WriteLine(Chr(34) & ".\network.st" & Chr(34))
                sobwriter.WriteLine(Chr(34) & ".\network.obi" & Chr(34))
                sobwriter.Close()
                sobwriter.Dispose()
            End Using

            Using tpwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\network.tp", False)
                tpwriter.WriteLine("TP_1.0")
                tpwriter.Close()
            End Using
            Using cpwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\network.cp", False)
                cpwriter.WriteLine("CP_1.0")
                cpwriter.Close()
            End Using
            Using cnwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\network.cn", False)
                cnwriter.WriteLine("CN_1.1")
                cnwriter.Close()
            End Using
            Using crwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\network.cr", False)
                crwriter.WriteLine("CR_1.1")
                crwriter.Close()
            End Using
            Using mewriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\network.me", False)
                mewriter.WriteLine("ME_1.0")
                mewriter.Close()
            End Using
            Using stwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\network.st", False)
                stwriter.WriteLine("ST_1.0")
                stwriter.Close()
            End Using
            Using obiwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\network.obi", False)
                obiwriter.WriteLine("OBI1.0")
                obiwriter.Close()
            End Using
            Using initwriter = New StreamWriter(Me.setup.Settings.ExportDirSobekTopo & "\initial.dat", False)
                initwriter.WriteLine("GLIN FLIN id '-1' nm '' ci '-1' ty 0 lv ll 0 2 q_ lq 0 0 flin glin")
                initwriter.Close()
            End Using
        End If

    End Sub

    Friend Function SetActiveProject(ByVal projectDir As String) As Boolean
        Dim Found As Boolean = False
        For Each myProject As clsSobekProject In Projects.Values
            If myProject.ProjectDir = projectDir Then
                ActiveProject = myProject
                myProject.IsActiveModel = True
                Found = True
            Else
                myProject.IsActiveModel = False
            End If
        Next
        Return Found
    End Function

    Private Sub Initialize()
        RRSettings.Initialize()
        CFSettings.Initialize()
    End Sub

    Friend Sub CreateCase(ByVal ProjectDir As String, ProgramsDir As String, ByVal CaseName As String)
        Dim myProject As clsSobekProject
        Dim myItem As New clsSobekCaseListItem(Me.setup)

        myItem.dir = ProjectDir & "\1"  'ahum
        myItem.name = CaseName

        'retrieve or create the sobek project
        If Not Projects.ContainsKey(ProjectDir.Trim.ToUpper) Then
            myProject = New clsSobekProject(Me.setup, ProjectDir, ProgramsDir)
            Projects.Add(myProject.ProjectDir.Trim.ToUpper, myProject)
        Else
            myProject = Projects.Item(ProjectDir.Trim.ToUpper)
        End If

        'add the case to this project
        Dim myCase As New ClsSobekCase(Me.setup, ActiveProject, myItem)
        myCase.CaseName = CaseName
        If Not myProject.Cases.ContainsKey(myCase.CaseName.Trim.ToUpper) Then
            myProject.Cases.Add(myCase.CaseName, myCase)
        Else
            'case already exists
        End If

    End Sub

    Public Function AddEmptyProject(ByVal ProjectDir As String, ProgramsDir As String) As Boolean
        '----------------------------------------------------------------------------------
        'this routine adds an empty sobekproject (.lit) to the collection of sobek projects
        'auteur: Siebe Bosch
        'datum: 14-6-2013
        'copyright 2012 Hydroconsult
        '----------------------------------------------------------------------------------
        Dim myProject As New clsSobekProject(Me.setup, ProjectDir, ProgramsDir, False)

        If Not Projects.ContainsKey(ProjectDir.Trim.ToUpper) Then
            Projects.Add(ProjectDir.Trim.ToUpper, myProject)
        Else
            Me.setup.Log.AddError("Could not add SOBEK Project " & ProjectDir & " since it was already in the collection.")
            Return False
        End If

        'while adding a model, make the first model active by default
        ActiveProject = Me.Projects.Item(ProjectDir.Trim.ToUpper)
        Return True

    End Function

    Friend Function SetAddProject(ByVal ProjectDir As String, ProgramsDir As String, Optional ReadCases As Boolean = True, Optional ByVal SetAsActiveProject As Boolean = True) As Boolean
        '-----------------------------------------------------------------------------
        'deze routine voegt een sobekproject (.lit) toe aan de collectie met models of stelt deze in als hij al in de collectie zit
        'en leest de caselist in
        'auteur: Siebe Bosch
        'datum: 12-5-2012
        'copyright 2012 Hydroconsult
        '-----------------------------------------------------------------------------

        Dim myProject As clsSobekProject

        'create the project and read the cases if requested
        myProject = New clsSobekProject(Me.setup, ProjectDir, ProgramsDir, ReadCases)

        'add the project to the collection
        If Not Projects.ContainsKey(ProjectDir.Trim.ToUpper) Then
            Projects.Add(ProjectDir.Trim.ToUpper, myProject)
        Else
            'project already exists
            Me.setup.Log.AddWarning("SOBEK Project " & ProjectDir & " was already present in the collection.")
        End If

        'set the current model as active if requested
        If SetAsActiveProject Then
            For Each myProject In Projects.Values
                If myProject.ProjectDir.Trim.ToUpper = ProjectDir.Trim.ToUpper Then
                    myProject.IsActiveModel = True
                Else
                    myProject.IsActiveModel = False
                End If
            Next
        End If

        'while adding a model, make the last added model active by default
        ActiveProject = Me.Projects.Values(Me.Projects.Count - 1)
        Return True

    End Function

    Public Sub synchronizeStorageTables(ByRef RefCase As clsSobekCase, ByRef SecondCase As clsSobekCase, ByVal ForceMinimumFromReferenceCaseOnSecondCase As Boolean)
        'this sub synchronizes the step sizes of the storage tables in two sobek cases
        'for inserted elevation steps, it interpolates linearly between the surrounding values
        'Optionally one can choose to force the values of the reference case onto the second case if the reference case has larger value
        Dim BaseRecord As clsNodesDatNODERecord
        Dim SecondRecord As clsNodesDatNODERecord
        Dim i As Long = 0, n As Long = RefCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Count

        Me.setup.GeneralFunctions.UpdateProgressBar("Synchronizing storage tables...", i, n)

        For Each BaseRecord In RefCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, n)
            SecondRecord = SecondCase.CFData.Data.NodesData.NodesDatNodeRecords.GetRecord(BaseRecord.ID.Trim.ToUpper)
            If Not SecondRecord Is Nothing Then
                'similar record found in both cases. Start synchronizing them!
                Call Me.setup.GeneralFunctions.SyncSobekTables(SecondRecord.ctswTable, BaseRecord.ctswTable, True)
            End If
        Next

        'that's it. Finally export the results
        Using baseWriter As New StreamWriter(Me.setup.Settings.ExportDirRoot & "\nodes_" & RefCase.CaseName & ".dat")
            RefCase.CFData.Data.NodesData.NodesDatNodeRecords.Write(baseWriter)
        End Using
        Using targetWriter As New StreamWriter(Me.setup.Settings.ExportDirRoot & "\nodes_" & SecondCase.CaseName & ".dat")
            SecondCase.CFData.Data.NodesData.NodesDatNodeRecords.Write(targetWriter)
        End Using

    End Sub

    Public Sub compareStorageTables(ByRef RefCase As clsSobekCase, ByRef SecondCase As clsSobekCase, ByVal ForceMinimumFromReferenceCaseOnSecondCase As Boolean)
        'this sub compares in detail the storage tables of two sobek cases
        'and writes the result to an Excel file
        Dim BaseRecord As clsNodesDatNODERecord
        Dim SecondRecord As clsNodesDatNODERecord
        Dim mySheet As clsExcelSheet
        Dim BaseArea As Double, SecondArea As Double
        Dim i As Long = 0, j As Long, n As Long = RefCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Count
        Dim r As Long

        Me.setup.GeneralFunctions.UpdateProgressBar("Comparing storage tables...", i, n)

        'create an excel worksheet for the results
        mySheet = Me.setup.ExcelFile.GetAddSheet("StorageCompare")
        r = 0
        mySheet.ws.Cells(r, 1).Value = RefCase.CaseName
        mySheet.ws.Cells(r, 3).Value = SecondCase.CaseName
        r += 1
        mySheet.ws.Cells(r, 0).Value = "ID"
        mySheet.ws.Cells(r, 1).Value = "Min.Area"
        mySheet.ws.Cells(r, 2).Value = "Max.Area"
        mySheet.ws.Cells(r, 3).Value = "Min.Area"
        mySheet.ws.Cells(r, 4).Value = "Max.Area"
        mySheet.ws.Cells(r, 5).Value = "Max.Diff"
        mySheet.ws.Cells(r, 6).Value = "Max.Diff%"

        'read the values
        For Each BaseRecord In RefCase.CFData.Data.NodesData.NodesDatNodeRecords.records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, n)
            SecondRecord = SecondCase.CFData.Data.NodesData.NodesDatNodeRecords.GetRecord(BaseRecord.ID.Trim.ToUpper)
            If Not SecondRecord Is Nothing Then
                'similar record found in both cases. Start synchronizing them in order to allow a comparison
                Call Me.setup.GeneralFunctions.SyncSobekTables(SecondRecord.ctswTable, BaseRecord.ctswTable, True)

                'initialize the stats
                Dim myDiff As Double
                Dim minDiff As Double = 999999999999
                Dim maxDiff As Double = 0
                Dim maxAreaBase As Double = 0
                Dim maxAreaSecond As Double = 0
                Dim minAreaBase As Double = 999999999999
                Dim minAreaSecond As Double = 999999999999

                For j = 0 To SecondRecord.ctswTable.XValues.Count - 1
                    If SecondRecord.ctswTable.XValues.Values(j) <> BaseRecord.ctswTable.XValues.Values(j) Then Throw New Exception("Error in sub compareStorageTables of class clsSOBEK. Storage tables were not properly synchronized.")
                    BaseArea = BaseRecord.ctswTable.Values1.Values(j)
                    SecondArea = SecondRecord.ctswTable.Values1.Values(j)
                    myDiff = SecondArea - BaseArea

                    If myDiff > maxDiff Then maxDiff = myDiff
                    If myDiff < minDiff Then minDiff = myDiff
                    If BaseArea > maxAreaBase Then maxAreaBase = BaseArea
                    If SecondArea > maxAreaSecond Then maxAreaSecond = SecondArea
                    If BaseArea < minAreaBase Then minAreaBase = BaseArea
                    If SecondArea < minAreaSecond Then minAreaSecond = SecondArea
                Next

                r += 1
                mySheet.ws.Cells(r, 0).Value = BaseRecord.ID
                mySheet.ws.Cells(r, 1).Value = minAreaBase
                mySheet.ws.Cells(r, 2).Value = maxAreaBase
                mySheet.ws.Cells(r, 3).Value = minAreaSecond
                mySheet.ws.Cells(r, 4).Value = maxAreaSecond
                mySheet.ws.Cells(r, 5).Value = maxDiff

                If maxAreaBase <= 0 Then
                    mySheet.ws.Cells(r, 6).Value = "NaN"
                Else
                    mySheet.ws.Cells(r, 6).Value = 100 * (maxDiff / maxAreaBase)
                End If

            Else

            End If
        Next

    End Sub

    Friend Sub CompareModels()
        Dim mySheet As clsExcelSheet
        Dim c As Long, caseIdx As Integer
        Dim BaseCase As clsSobekCase = Nothing, CompareCase As clsSobekCase = Nothing
        'vergelijkt twee SOBEK-modellen met elkaar
        'begin met alle algemene kenmerken en schrijf ze naar het tabblad "Summary"

        c = 0
        For Each myModel As clsSobekProject In Projects.Values
            For Each myCase As clsSobekCase In myModel.Cases.Values
                If myCase.InUse Then
                    caseIdx += 1

                    If caseIdx = 1 Then
                        BaseCase = myCase
                    Else
                        CompareCase = myCase
                    End If

                    c += 1
                    'bereken de statistische kentallen en schrijf ze weg naar het werkblad "Summary.RR"
                    If setup.SOBEKData.CompareSettings.RRSummary Then
                        myCase.RRData.calcStats()
                        mySheet = Me.setup.ExcelFile.GetAddSheet("Summary.RR")
                        myCase.RRData.Stats2Worksheet(mySheet, c, myCase.CaseName)
                    End If

                    If setup.SOBEKData.CompareSettings.CFSummary Then
                        myCase.CFData.calcStats()
                        mySheet = Me.setup.ExcelFile.GetAddSheet("Summary.1DFlow")
                        myCase.CFData.Stats2Worksheet(mySheet, c, myCase.CaseName)
                    End If
                End If
            Next
        Next

        'en dan gaan we nu de diepte in. Eerst de unpaved nodes vergelijken
        If CompareSettings.RRUnpaved Then
            'mySheet = Me.setup.ExcelFile.GetAddSheet("Unpaved.RR")
            'mySheet.Write(CompareCFPumps(BaseCase, CompareCase, CompareSettings.MaxSearchRadius))
        End If

        If CompareSettings.RRPaved Then
            'mySheet = Me.setup.ExcelFile.GetAddSheet("Unpaved.RR")
            'mySheet.Write(CompareCFPumps(BaseCase, CompareCase, CompareSettings.MaxSearchRadius))
        End If

        'Dan de Flow-module
        If CompareSettings.CFPumps Then
            mySheet = Me.setup.ExcelFile.GetAddSheet("Pumps.1DFlow")
            mySheet.Write(CompareCFPumps(BaseCase, CompareCase, CompareSettings.MaxSearchRadius))
        End If
        If CompareSettings.CFWeirs Then
            mySheet = Me.setup.ExcelFile.GetAddSheet("Weirs.1DFlow")
            mySheet.Write(CompareCFWeirs(BaseCase, CompareCase, CompareSettings.MaxSearchRadius))
        End If
        If CompareSettings.CFUniversalWeirs Then
            mySheet = Me.setup.ExcelFile.GetAddSheet("UniversalWeirs.1DFlow")
            mySheet.Write(CompareCFUniversalWeirs(BaseCase, CompareCase, CompareSettings.MaxSearchRadius))
        End If
        If CompareSettings.CFOrifices Then
            mySheet = Me.setup.ExcelFile.GetAddSheet("Orifices.1DFlow")
            mySheet.Write(CompareCFOrifices(BaseCase, CompareCase, CompareSettings.MaxSearchRadius))
        End If
        If CompareSettings.CFBridges Then
            mySheet = Me.setup.ExcelFile.GetAddSheet("Bridges.1DFlow")
            mySheet.Write(CompareCFBridges(BaseCase, CompareCase, CompareSettings.MaxSearchRadius))
        End If
        If CompareSettings.CFProfiles Then
            mySheet = Me.setup.ExcelFile.GetAddSheet("CrossSections.1DFlow")
            mySheet.Write(CompareCFProfiles(BaseCase, CompareCase, CompareSettings.MaxSearchRadius))
        End If
        If CompareSettings.CFLaterals Then
            mySheet = Me.setup.ExcelFile.GetAddSheet("Laterals.1DFlow")
            mySheet.Write(CompareCFLaterals(BaseCase, CompareCase, CompareSettings.MaxSearchRadius))
        End If

    End Sub

    Public Function CompareCFPumps(ByRef BaseCase As clsSobekCase, ByRef CompareCase As clsSobekCase, MaxSearchRadius As Double) As Object(,)

        Dim i As Long, j As Long = 0
        Dim BaseDat As clsStructDatRecord, BaseDef As clsStructDefRecord, BaseContr As clsControlDefRecord = Nothing
        Dim CompareDat As clsStructDatRecord, CompareDef As clsStructDefRecord, CompareContr As clsControlDefRecord = Nothing
        Dim CompareObj As clsSbkReachObject, BaseObj As clsSbkReachObject
        Dim Summary(,) As Object

        ReDim Summary(100000, 10)

        'schrijf de headers
        i = 0
        Summary(i, 0) = "Pump ID"
        Summary(i, 1) = "Parameter"
        Summary(i, 2) = "Verdict"
        Summary(i, 3) = BaseCase.CaseName
        Summary(i, 4) = CompareCase.CaseName

        Me.setup.GeneralFunctions.UpdateProgressBar("Comparing 1D Flow pumps...", 0, 10)

        'in detail de takobjecten
        For Each myReach As clsSbkReach In BaseCase.CFTopo.Reaches.Reaches.Values
            j += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", j, BaseCase.CFTopo.Reaches.Reaches.Values.Count)

            For Each BaseObj In myReach.ReachObjects.ReachObjects.Values

                If BaseObj.InUse = False Then Continue For

                'de gemalen
                If BaseObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump Then

                    BaseObj.calcXY()
                    BaseDat = BaseCase.CFData.Data.StructureData.StructDatRecords.Records.Item(BaseObj.ID.Trim.ToUpper)
                    BaseDef = BaseCase.CFData.Data.StructureData.StructDefRecords.Records.Item(BaseDat.dd.Trim.ToUpper)
                    If BaseDat.ca = 1 Then BaseContr = BaseCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(BaseDat.cj.Trim.ToUpper)

                    'zoek hetzelfde gemaal in de andere case
                    CompareObj = CompareCase.CFTopo.getSimilarReachObject(BaseObj, MaxSearchRadius)
                    If Not CompareObj Is Nothing AndAlso CompareObj.InUse Then
                        If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump Then

                            'we hebben een object met een gelijksoortig ID gevonden!
                            CompareObj.calcXY()
                            CompareDat = CompareCase.CFData.Data.StructureData.StructDatRecords.Records.Item(CompareObj.ID.Trim.ToUpper)
                            CompareDef = CompareCase.CFData.Data.StructureData.StructDefRecords.Records.Item(CompareDat.dd.Trim.ToUpper)
                            If CompareDat.ca = 1 Then CompareContr = CompareCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(CompareDat.cj.Trim.ToUpper)

                            CompareCFTopography(Summary, i, BaseObj, BaseDat.ID, BaseDat.nm, CompareObj, CompareDat.ID, CompareDat.nm)

                            'Control direction
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Control direction"
                            If BaseDef.dn <> CompareDef.dn Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.dn
                            Summary(i, 4) = CompareDef.dn

                            'Reduction curve in use
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Reduction curve in use"
                            If BaseDef.rtcr1 <> CompareDef.rtcr1 Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.rtcr1
                            Summary(i, 4) = CompareDef.rtcr1

                            'Pompcapaciteiten
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Number of pump capacities"
                            If BaseDef.ctltTable.Values1.Values.Count <> CompareDef.ctltTable.Values1.Values.Count Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.ctltTable.Values1.Values.Count
                            Summary(i, 4) = CompareDef.ctltTable.Values1.Values.Count

                            If BaseDef.ctltTable.Values1.Values.Count = CompareDef.ctltTable.Values1.Values.Count Then
                                'compare each pump cap
                                For j = 0 To BaseDef.ctltTable.Values1.Values.Count - 1

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "Pump Cap " & j + 1
                                    If BaseDef.ctltTable.XValues.Values(j) <> CompareDef.ctltTable.XValues.Values(j) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.ctltTable.XValues.Values(j)
                                    Summary(i, 4) = CompareDef.ctltTable.XValues.Values(j)

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "On level suction side " & j + 1
                                    If BaseDef.ctltTable.Values1.Values(j) <> CompareDef.ctltTable.Values1.Values(j) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.ctltTable.Values1.Values(j)
                                    Summary(i, 4) = CompareDef.ctltTable.Values1.Values(j)

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "On level pressure side " & j + 1
                                    If BaseDef.ctltTable.Values2.Values(j) <> CompareDef.ctltTable.Values2.Values(j) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.ctltTable.Values2.Values(j)
                                    Summary(i, 4) = CompareDef.ctltTable.Values2.Values(j)

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "On level pressure side " & j + 1
                                    If BaseDef.ctltTable.Values3.Values(j) <> CompareDef.ctltTable.Values3.Values(j) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.ctltTable.Values3.Values(j)
                                    Summary(i, 4) = CompareDef.ctltTable.Values3.Values(j)

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "On level pressure side " & j + 1
                                    If BaseDef.ctltTable.Values4.Values(j) <> CompareDef.ctltTable.Values4.Values(j) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.ctltTable.Values4.Values(j)
                                    Summary(i, 4) = CompareDef.ctltTable.Values4.Values(j)
                                Next
                            End If

                            'Controller active?
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Controller active"
                            If BaseDat.ca <> CompareDat.ca Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDat.ca
                            Summary(i, 4) = CompareDat.ca

                            'Controller ID
                            If BaseDat.ca = 1 AndAlso CompareDat.ca = 1 Then

                                i += 1
                                Summary(i, 0) = BaseDat.ID
                                Summary(i, 1) = "Controller ID"
                                If BaseDat.cj <> CompareDat.cj Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseDat.cj
                                Summary(i, 4) = CompareDat.cj

                                'controller ID's zijn gelijk
                                i += 1
                                Summary(i, 0) = BaseDat.ID
                                Summary(i, 1) = "Controller type"
                                If BaseContr.ct <> CompareContr.ct Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseContr.ct
                                Summary(i, 4) = CompareContr.ct

                                'vergelijk de controllers
                                If BaseContr.ct = CompareContr.ct Then Call CompareCFControllers(Summary, i, BaseDat, BaseContr, CompareContr)
                            End If
                        End If
                    Else
                        i += 1
                        Summary(i, 0) = BaseDat.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = BaseDat.ID
                        Summary(i, 4) = ""
                    End If

                End If
            Next
        Next

        'doorloop nu alle gemalen in de comparecase en noteer degene die niet in de basecase zitten
        For Each myReach As clsSbkReach In CompareCase.CFTopo.Reaches.Reaches.Values
            For Each CompareObj In myReach.ReachObjects.ReachObjects.Values

                'de gemalen
                If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump Then
                    'zoek hetzelfde gemaal in de basiscase
                    BaseObj = BaseCase.CFTopo.getSimilarReachObject(CompareObj, MaxSearchRadius)

                    If BaseObj Is Nothing Then
                        i += 1
                        Summary(i, 1) = CompareObj.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = ""
                        Summary(i, 4) = CompareObj.ID
                    End If
                End If
            Next
        Next

        Return Summary
    End Function


    Public Function CompareCFWeirs(ByRef BaseCase As clsSobekCase, ByRef CompareCase As clsSobekCase, MaxSearchRadius As Double) As Object(,)

        Dim i As Long, j As Long = 0
        Dim BaseDat As clsStructDatRecord, BaseDef As clsStructDefRecord, BaseContr As clsControlDefRecord = Nothing
        Dim CompareDat As clsStructDatRecord, CompareDef As clsStructDefRecord, CompareContr As clsControlDefRecord = Nothing
        Dim CompareObj As clsSbkReachObject, BaseObj As clsSbkReachObject
        Dim Summary(,) As Object
        ReDim Summary(10000, 11)

        'schrijf de headers
        i = 0
        Summary(i, 0) = "Weir ID"
        Summary(i, 1) = "Parameter"
        Summary(i, 2) = "Verdict"
        Summary(i, 3) = BaseCase.CaseName
        Summary(i, 4) = CompareCase.CaseName

        Me.setup.GeneralFunctions.UpdateProgressBar("Comparing 1D Flow weirs...", 0, 10)

        'in detail de takobjecten
        For Each myReach As clsSbkReach In BaseCase.CFTopo.Reaches.Reaches.Values
            j += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", j, BaseCase.CFTopo.Reaches.Reaches.Values.Count)

            For Each BaseObj In myReach.ReachObjects.ReachObjects.Values

                If BaseObj.InUse = False Then Continue For

                'de gemalen
                If BaseObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir Then

                    BaseObj.calcXY()
                    BaseDat = BaseCase.CFData.Data.StructureData.StructDatRecords.Records.Item(BaseObj.ID.Trim.ToUpper)
                    BaseDef = BaseCase.CFData.Data.StructureData.StructDefRecords.Records.Item(BaseDat.dd.Trim.ToUpper)
                    If BaseDat.ca = 1 Then BaseContr = BaseCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(BaseDat.cj.Trim.ToUpper)

                    'zoek dezelfde stuw in de andere case
                    CompareObj = CompareCase.CFTopo.getSimilarReachObject(BaseObj, MaxSearchRadius)
                    If Not CompareObj Is Nothing AndAlso CompareObj.InUse Then
                        If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir Then

                            CompareObj.calcXY()
                            CompareDat = CompareCase.CFData.Data.StructureData.StructDatRecords.Records.Item(CompareObj.ID.Trim.ToUpper)
                            CompareDef = CompareCase.CFData.Data.StructureData.StructDefRecords.Records.Item(CompareDat.dd.Trim.ToUpper)
                            If CompareDat.ca = 1 Then CompareContr = CompareCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(CompareDat.cj.Trim.ToUpper)

                            CompareCFTopography(Summary, i, BaseObj, BaseDat.ID, BaseDat.nm, CompareObj, CompareDat.ID, CompareDat.nm)

                            'kruinhoogte
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Crest level"
                            If BaseDef.cl <> CompareDef.cl Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.cl
                            Summary(i, 4) = CompareDef.cl

                            'kruinbreedte
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Crest width"
                            If BaseDef.cw <> CompareDef.cw Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.cw
                            Summary(i, 4) = CompareDef.cw

                            'afvoercoef
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Discharge coef"
                            If BaseDef.ce <> CompareDef.ce Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.ce
                            Summary(i, 4) = CompareDef.ce

                            'laterale contractiecoef
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Lateral contraction coef"
                            If BaseDef.sc <> CompareDef.sc Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.sc
                            Summary(i, 4) = CompareDef.sc

                            'Stromingsrichting
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Flow direction"
                            If BaseDef.rt <> CompareDef.rt Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.rt
                            Summary(i, 4) = CompareDef.rt

                            'Controller active?
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Controller"
                            If BaseDat.ca <> CompareDat.ca Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDat.ca
                            Summary(i, 4) = CompareDat.ca

                            'Controller ID
                            If BaseDat.ca = 1 AndAlso CompareDat.ca = 1 Then
                                i += 1
                                Summary(i, 0) = BaseDat.ID
                                Summary(i, 1) = "Controller ID"
                                If BaseDat.cj <> CompareDat.cj Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseDat.cj
                                Summary(i, 4) = CompareDat.cj

                                'controller ID's zijn gelijk
                                i += 1
                                Summary(i, 0) = BaseDat.ID
                                Summary(i, 1) = "Controller type"
                                If BaseContr.ct <> CompareContr.ct Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseContr.ct
                                Summary(i, 4) = CompareContr.ct

                                'compare the controllers
                                If BaseContr.ct = CompareContr.ct Then Call CompareCFControllers(Summary, i, BaseDat, BaseContr, CompareContr)
                            End If
                        End If
                    Else
                        i += 1
                        Summary(i, 0) = BaseDat.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = BaseDat.ID
                        Summary(i, 4) = ""
                    End If
                End If
            Next
        Next

        'zoek nu vanuit de referentiecase naar stuwen die niet in de basiscase voorkomen
        For Each myReach As clsSbkReach In CompareCase.CFTopo.Reaches.Reaches.Values
            For Each CompareObj In myReach.ReachObjects.ReachObjects.Values
                If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir Then
                    'zoek dezelfde stuw op in de andere case
                    BaseObj = BaseCase.CFTopo.getSimilarReachObject(CompareObj, MaxSearchRadius)
                    If BaseObj Is Nothing Then
                        i += 1
                        Summary(i, 1) = CompareObj.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = ""
                        Summary(i, 4) = CompareObj.ID
                    End If
                End If
            Next
        Next

        Return Summary
    End Function

    Public Function CompareCFUniversalWeirs(ByRef BaseCase As clsSobekCase, ByRef CompareCase As clsSobekCase, MaxSearchRadius As Double) As Object(,)

        Dim i As Long, j As Long = 0
        Dim BaseDat As clsStructDatRecord, BaseDef As clsStructDefRecord, BaseContr As clsControlDefRecord = Nothing
        Dim CompareDat As clsStructDatRecord, CompareDef As clsStructDefRecord, CompareContr As clsControlDefRecord = Nothing
        Dim CompareObj As clsSbkReachObject, BaseObj As clsSbkReachObject
        Dim Summary(,) As Object
        ReDim Summary(10000, 11)

        'schrijf de headers
        i = 0
        Summary(i, 0) = "Weir ID"
        Summary(i, 1) = "Parameter"
        Summary(i, 2) = "Verdict"
        Summary(i, 3) = BaseCase.CaseName
        Summary(i, 4) = CompareCase.CaseName

        Me.setup.GeneralFunctions.UpdateProgressBar("Comparing 1D Flow Universal weirs...", 0, 10)

        'in detail de takobjecten
        For Each myReach As clsSbkReach In BaseCase.CFTopo.Reaches.Reaches.Values
            j += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", j, BaseCase.CFTopo.Reaches.Reaches.Values.Count)

            For Each BaseObj In myReach.ReachObjects.ReachObjects.Values

                If BaseObj.InUse = False Then Continue For

                'de gemalen
                If BaseObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir Then

                    BaseObj.calcXY()
                    BaseDat = BaseCase.CFData.Data.StructureData.StructDatRecords.Records.Item(BaseObj.ID.Trim.ToUpper)
                    BaseDef = BaseCase.CFData.Data.StructureData.StructDefRecords.Records.Item(BaseDat.dd.Trim.ToUpper)
                    If BaseDat.ca = 1 Then BaseContr = BaseCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(BaseDat.cj.Trim.ToUpper)

                    'zoek dezelfde stuw in de andere case
                    CompareObj = CompareCase.CFTopo.getSimilarReachObject(BaseObj, MaxSearchRadius)
                    If Not CompareObj Is Nothing AndAlso CompareObj.InUse Then
                        If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir Then

                            CompareObj.calcXY()
                            CompareDat = CompareCase.CFData.Data.StructureData.StructDatRecords.Records.Item(CompareObj.ID.Trim.ToUpper)
                            CompareDef = CompareCase.CFData.Data.StructureData.StructDefRecords.Records.Item(CompareDat.dd.Trim.ToUpper)
                            If CompareDat.ca = 1 Then CompareContr = CompareCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(CompareDat.cj.Trim.ToUpper)

                            CompareCFTopography(Summary, i, BaseObj, BaseDat.ID, BaseDat.nm, CompareObj, CompareDat.ID, CompareDat.nm)

                            'kruinhoogte
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Weir Profile Shift"
                            If BaseDef.cl <> CompareDef.cl Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.cl
                            Summary(i, 4) = CompareDef.cl

                            'afvoercoef
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Discharge coef"
                            If BaseDef.ce <> CompareDef.ce Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.ce
                            Summary(i, 4) = CompareDef.ce

                            'Stromingsrichting
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Flow direction"
                            If BaseDef.rt <> CompareDef.rt Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.rt
                            Summary(i, 4) = CompareDef.rt

                            'profieldefinitie
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Profile definition ID"
                            If BaseDef.si <> CompareDef.si Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.si
                            Summary(i, 4) = CompareDef.si

                        End If
                    Else
                        i += 1
                        Summary(i, 0) = BaseDat.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = BaseDat.ID
                        Summary(i, 4) = ""
                    End If
                End If
            Next
        Next

        'zoek nu vanuit de referentiecase naar stuwen die niet in de basiscase voorkomen
        For Each myReach As clsSbkReach In CompareCase.CFTopo.Reaches.Reaches.Values
            For Each CompareObj In myReach.ReachObjects.ReachObjects.Values
                If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir Then
                    'zoek dezelfde stuw op in de andere case
                    BaseObj = BaseCase.CFTopo.getSimilarReachObject(CompareObj, MaxSearchRadius)
                    If BaseObj Is Nothing Then
                        i += 1
                        Summary(i, 1) = CompareObj.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = ""
                        Summary(i, 4) = CompareObj.ID
                    End If
                End If
            Next
        Next

        Return Summary
    End Function

    Public Function CompareCFLaterals(ByRef BaseCase As clsSobekCase, ByRef CompareCase As clsSobekCase, MaxSearchRadius As Double) As Object(,)

        Dim i As Long, j As Long = 0
        Dim BaseFLBR As clsLateralDatFLBRRecord
        Dim CompareFLBR As clsLateralDatFLBRRecord
        Dim CompareObj As clsSbkReachObject, BaseObj As clsSbkReachObject
        Dim Summary(,) As Object
        ReDim Summary(10000, 11)

        'schrijf de headers
        i = 0
        Summary(i, 0) = "Lateral ID"
        Summary(i, 1) = "Parameter"
        Summary(i, 2) = "Verdict"
        Summary(i, 3) = BaseCase.CaseName
        Summary(i, 4) = CompareCase.CaseName

        Me.setup.GeneralFunctions.UpdateProgressBar("Comparing 1D Flow Lateral inflows...", 0, 10)

        'in detail de takobjecten
        For Each myReach As clsSbkReach In BaseCase.CFTopo.Reaches.Reaches.Values
            j += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", j, BaseCase.CFTopo.Reaches.Reaches.Values.Count)

            For Each BaseObj In myReach.ReachObjects.ReachObjects.Values

                If BaseObj.InUse = False Then Continue For
                If BaseObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral Then

                    BaseObj.calcXY()
                    BaseFLBR = BaseCase.CFData.Data.LateralData.LateralDatFLBRRecords.records.Item(BaseObj.ID.Trim.ToUpper)

                    'zoek dezelfde lateral in de andere case
                    CompareObj = CompareCase.CFTopo.getSimilarReachObject(BaseObj, MaxSearchRadius)
                    If Not CompareObj Is Nothing AndAlso CompareObj.InUse Then
                        If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral Then

                            CompareObj.calcXY()
                            CompareFLBR = CompareCase.CFData.Data.LateralData.LateralDatFLBRRecords.records.Item(CompareObj.ID.Trim.ToUpper)

                            CompareCFTopography(Summary, i, BaseObj, BaseFLBR.ID, BaseObj.Name, CompareObj, CompareFLBR.ID, CompareObj.Name)

                            'type of lateral
                            i += 1
                            Summary(i, 0) = BaseFLBR.ID
                            Summary(i, 1) = "Lateral type"
                            If BaseFLBR.dclt1 <> CompareFLBR.dclt1 Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"

                            Select Case BaseFLBR.dclt1
                                Case Is = 0
                                    Summary(i, 3) = "Constant value"
                                Case Is = 1
                                    Summary(i, 3) = "Table"
                                Case Is = 4
                                    Summary(i, 3) = "Lateral structure on branch"
                                Case Is = 5
                                    Summary(i, 3) = "Retention"
                                Case Is = 6
                                    Summary(i, 3) = "Rational method"
                                Case Is = 7
                                    Summary(i, 3) = "From Rainfall"
                                Case Is = 11
                                    Summary(i, 3) = "From Table Library"
                            End Select
                            Select Case CompareFLBR.dclt1
                                Case Is = 0
                                    Summary(i, 4) = "Constant value"
                                Case Is = 1
                                    Summary(i, 4) = "Table"
                                Case Is = 4
                                    Summary(i, 4) = "Lateral structure on branch"
                                Case Is = 5
                                    Summary(i, 4) = "Retention"
                                Case Is = 6
                                    Summary(i, 4) = "Rational method"
                                Case Is = 7
                                    Summary(i, 4) = "From Rainfall"
                                Case Is = 11
                                    Summary(i, 4) = "From Table Library"
                            End Select

                            'constant intensity
                            i += 1
                            Summary(i, 0) = BaseObj.ID
                            Summary(i, 1) = "Constant intensity"
                            If BaseFLBR.ir <> CompareFLBR.ir Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseFLBR.ir
                            Summary(i, 4) = CompareFLBR.ir

                            'meteo station
                            i += 1
                            Summary(i, 0) = BaseObj.ID
                            Summary(i, 1) = "Meteo station"
                            If BaseFLBR.ms <> CompareFLBR.ms Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseFLBR.ms
                            Summary(i, 4) = CompareFLBR.ms

                            'area
                            i += 1
                            Summary(i, 0) = BaseObj.ID
                            Summary(i, 1) = "Area"
                            If BaseFLBR.ar <> CompareFLBR.ar Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseFLBR.ar
                            Summary(i, 4) = CompareFLBR.ar

                        End If
                    Else
                        i += 1
                        Summary(i, 0) = BaseObj.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        If CompareObj Is Nothing Then
                            Summary(i, 3) = "N/A"
                        Else
                            Summary(i, 3) = CompareObj.ID
                        End If
                        Summary(i, 4) = ""
                    End If
                End If
            Next
        Next

        'zoek nu vanuit de referentiecase naar laterals die niet in de basiscase voorkomen
        For Each myReach As clsSbkReach In CompareCase.CFTopo.Reaches.Reaches.Values
            For Each CompareObj In myReach.ReachObjects.ReachObjects.Values
                If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral Then
                    'zoek dezelfde stuw op in de andere case
                    BaseObj = BaseCase.CFTopo.getSimilarReachObject(CompareObj, MaxSearchRadius)
                    If BaseObj Is Nothing Then
                        i += 1
                        Summary(i, 1) = CompareObj.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = ""
                        Summary(i, 4) = CompareObj.ID
                    End If
                End If
            Next
        Next

        Return Summary
    End Function



    Public Function CompareCFTopography(ByRef Summary(,) As Object, ByRef i As Integer, BaseObj As clsSbkReachObject, BaseID As String, BaseName As String, CompareObj As clsSbkReachObject, CompareID As String, CompareName As String) As Boolean
        Try
            'vergelijk eerst de ID's
            i += 1
            Summary(i, 0) = BaseID
            Summary(i, 1) = "Object ID"
            If BaseID <> CompareID Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
            Summary(i, 3) = BaseID
            Summary(i, 4) = CompareID

            'vergelijk dan de naam
            i += 1
            Summary(i, 0) = BaseID
            Summary(i, 1) = "Object Name"
            If BaseName <> CompareName Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
            Summary(i, 3) = BaseName
            Summary(i, 4) = CompareName

            'vergelijk dan de tak-ID
            i += 1
            Summary(i, 0) = BaseID
            Summary(i, 1) = "Reach ID"
            If BaseObj.ci <> CompareObj.ci Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
            Summary(i, 3) = BaseObj.ci
            Summary(i, 4) = CompareObj.ci

            'vergelijk dan de locatie op de tak
            i += 1
            Summary(i, 0) = BaseID
            Summary(i, 1) = "Chainage (m)"
            If BaseObj.lc <> CompareObj.lc Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
            Summary(i, 3) = BaseObj.lc
            Summary(i, 4) = CompareObj.lc

            'vergelijk dan het X-coordinaat
            i += 1
            Summary(i, 0) = BaseID
            Summary(i, 1) = "X-coordinate"
            If Math.Abs(BaseObj.X - CompareObj.X) > 1 Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
            Summary(i, 3) = BaseObj.X
            Summary(i, 4) = CompareObj.X

            'vergelijk dan het Y-coordinaat
            i += 1
            Summary(i, 0) = BaseID
            Summary(i, 1) = "Y-coordinate"
            If Math.Abs(BaseObj.Y - CompareObj.Y) > 1 Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
            Summary(i, 3) = BaseObj.Y
            Summary(i, 4) = CompareObj.Y

            'vergelijk dan het coordinaat
            Dim Difference As Double = Math.Sqrt((CompareObj.X - BaseObj.X) ^ 2 + (CompareObj.Y - BaseObj.Y) ^ 2)
            i += 1
            Summary(i, 0) = BaseID
            Summary(i, 1) = "Distance (m)"
            If Difference > 1 Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
            Summary(i, 3) = 0
            Summary(i, 4) = Difference
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function CompareCFTopography of class clsSOBEK.")
            Return False
        End Try
    End Function

    Public Function CompareCFOrifices(ByRef BaseCase As clsSobekCase, ByRef CompareCase As clsSobekCase, MaxSearchRadius As Double) As Object(,)

        Dim i As Long, j As Long = 0
        Dim BaseDat As clsStructDatRecord, BaseDef As clsStructDefRecord, BaseContr As clsControlDefRecord = Nothing
        Dim CompareDat As clsStructDatRecord, CompareDef As clsStructDefRecord, CompareContr As clsControlDefRecord = Nothing
        Dim CompareObj As clsSbkReachObject, BaseObj As clsSbkReachObject
        Dim Summary(,) As Object
        ReDim Summary(10000, 11)

        'schrijf de headers
        i = 0
        Summary(i, 0) = "Orifice ID"
        Summary(i, 1) = "Parameter"
        Summary(i, 2) = "Verdict"
        Summary(i, 3) = BaseCase.CaseName
        Summary(i, 4) = CompareCase.CaseName

        Me.setup.GeneralFunctions.UpdateProgressBar("Comparing 1D Flow orifices...", 0, 10)

        'in detail de takobjecten
        For Each myReach As clsSbkReach In BaseCase.CFTopo.Reaches.Reaches.Values
            j += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", j, BaseCase.CFTopo.Reaches.Reaches.Values.Count)

            For Each BaseObj In myReach.ReachObjects.ReachObjects.Values

                If BaseObj.InUse = False Then Continue For

                'de gemalen
                If BaseObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice Then

                    BaseObj.calcXY()
                    BaseDat = BaseCase.CFData.Data.StructureData.StructDatRecords.Records.Item(BaseObj.ID.Trim.ToUpper)
                    BaseDef = BaseCase.CFData.Data.StructureData.StructDefRecords.Records.Item(BaseDat.dd.Trim.ToUpper)
                    If BaseDat.ca = 1 Then BaseContr = BaseCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(BaseDat.cj.Trim.ToUpper)

                    'zoek dezelfde stuw in de andere case
                    CompareObj = CompareCase.CFTopo.getSimilarReachObject(BaseObj, MaxSearchRadius)
                    If Not CompareObj Is Nothing AndAlso CompareObj.InUse Then
                        If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice Then

                            CompareObj.calcXY()
                            CompareDat = CompareCase.CFData.Data.StructureData.StructDatRecords.Records.Item(CompareObj.ID.Trim.ToUpper)
                            CompareDef = CompareCase.CFData.Data.StructureData.StructDefRecords.Records.Item(CompareDat.dd.Trim.ToUpper)
                            If CompareDat.ca = 1 Then CompareContr = CompareCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(CompareDat.cj.Trim.ToUpper)

                            CompareCFTopography(Summary, i, BaseObj, BaseDat.ID, BaseDat.nm, CompareObj, CompareDat.ID, CompareDat.nm)

                            'kruinhoogte
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Crest level"
                            If BaseDef.cl <> CompareDef.cl Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.cl
                            Summary(i, 4) = CompareDef.cl

                            'openingshoogte
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Opening height"
                            If BaseDef.gh <> CompareDef.gh Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.gh
                            Summary(i, 4) = CompareDef.gh

                            'kruinbreedte
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Crest width"
                            If BaseDef.cw <> CompareDef.cw Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.cw
                            Summary(i, 4) = CompareDef.cw

                            'afvoercoef
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Discharge coef"
                            If BaseDef.ce <> CompareDef.ce Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.ce
                            Summary(i, 4) = CompareDef.ce

                            'laterale contractiecoef
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Lateral contraction coef"
                            If BaseDef.sc <> CompareDef.sc Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.sc
                            Summary(i, 4) = CompareDef.sc

                            'Stromingsrichting
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Flow direction"
                            If BaseDef.rt <> CompareDef.rt Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.rt
                            Summary(i, 4) = CompareDef.rt

                            'Controller active?
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Controller active"
                            If BaseDat.ca <> CompareDat.ca Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDat.ca
                            Summary(i, 4) = CompareDat.ca

                            'Controller ID
                            If BaseDat.ca = 1 AndAlso CompareDat.ca = 1 Then
                                i += 1
                                Summary(i, 0) = BaseDat.ID
                                Summary(i, 1) = "Controller ID"
                                If BaseDat.cj <> CompareDat.cj Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseDat.cj
                                Summary(i, 4) = CompareDat.cj

                                'controller ID's zijn gelijk
                                i += 1
                                Summary(i, 0) = BaseDat.ID
                                Summary(i, 1) = "Controller type"
                                If BaseContr.ct <> CompareContr.ct Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseContr.ct
                                Summary(i, 4) = CompareContr.ct

                                Call CompareCFControllers(Summary, i, BaseDat, BaseContr, CompareContr)
                            End If
                        End If
                    Else
                        i += 1
                        Summary(i, 0) = BaseDat.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = BaseDat.ID
                        Summary(i, 4) = ""
                    End If
                End If
            Next
        Next

        'zoek nu vanuit de referentiecase naar stuwen die niet in de basiscase voorkomen
        For Each myReach As clsSbkReach In CompareCase.CFTopo.Reaches.Reaches.Values
            For Each CompareObj In myReach.ReachObjects.ReachObjects.Values
                If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice Then
                    'zoek dezelfde onderlaat op in de andere case
                    BaseObj = BaseCase.CFTopo.getSimilarReachObject(CompareObj, MaxSearchRadius)
                    If BaseObj Is Nothing Then
                        i += 1
                        Summary(i, 1) = CompareObj.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = ""
                        Summary(i, 4) = CompareObj.ID
                    End If
                End If
            Next
        Next

        Return Summary
    End Function

    Public Function CompareCFBridges(ByRef BaseCase As clsSobekCase, ByRef CompareCase As clsSobekCase, MaxSearchRadius As Double) As Object(,)

        Dim i As Long, j As Long = 0
        Dim BaseDat As clsStructDatRecord, BaseDef As clsStructDefRecord, BaseContr As clsControlDefRecord = Nothing
        Dim BaseFric As clsFrictionDatSTFRRecord
        Dim CompareDat As clsStructDatRecord, CompareDef As clsStructDefRecord, CompareContr As clsControlDefRecord = Nothing
        Dim CompareFric As clsFrictionDatSTFRRecord
        Dim CompareObj As clsSbkReachObject, BaseObj As clsSbkReachObject
        Dim Summary(,) As Object
        ReDim Summary(10000, 11)

        'schrijf de headers
        i = 0
        Summary(i, 0) = "Bridge ID"
        Summary(i, 1) = "Parameter"
        Summary(i, 2) = "Verdict"
        Summary(i, 3) = BaseCase.CaseName
        Summary(i, 4) = CompareCase.CaseName

        Me.setup.GeneralFunctions.UpdateProgressBar("Comparing 1D Flow bridges...", 0, 10)

        'in detail de takobjecten
        For Each myReach As clsSbkReach In BaseCase.CFTopo.Reaches.Reaches.Values
            j += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", j, BaseCase.CFTopo.Reaches.Reaches.Values.Count)

            For Each BaseObj In myReach.ReachObjects.ReachObjects.Values

                If BaseObj.InUse = False Then Continue For

                'de objecten
                If BaseObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge Then

                    BaseObj.calcXY()
                    BaseDat = BaseCase.CFData.Data.StructureData.StructDatRecords.Records.Item(BaseObj.ID.Trim.ToUpper)
                    BaseDef = BaseCase.CFData.Data.StructureData.StructDefRecords.Records.Item(BaseDat.dd.Trim.ToUpper)
                    BaseFric = BaseCase.CFData.Data.FrictionData.FrictionDatSTFRRecords.GetItem(BaseDef.ID.Trim.ToUpper)

                    If BaseDat.ca = 1 Then BaseContr = BaseCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(BaseDat.cj.Trim.ToUpper)

                    'zoek dezelfde stuw in de andere case
                    CompareObj = CompareCase.CFTopo.getSimilarReachObject(BaseObj, MaxSearchRadius)
                    If Not CompareObj Is Nothing AndAlso CompareObj.InUse Then
                        If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge Then

                            CompareObj.calcXY()
                            CompareDat = CompareCase.CFData.Data.StructureData.StructDatRecords.Records.Item(CompareObj.ID.Trim.ToUpper)
                            CompareDef = CompareCase.CFData.Data.StructureData.StructDefRecords.Records.Item(CompareDat.dd.Trim.ToUpper)
                            CompareFric = CompareCase.CFData.Data.FrictionData.FrictionDatSTFRRecords.GetItem(CompareDef.ID.Trim.ToUpper)
                            If CompareDat.ca = 1 Then CompareContr = CompareCase.CFData.Data.StructureData.ControlDefRecords.Records.Item(CompareDat.cj.Trim.ToUpper)

                            CompareCFTopography(Summary, i, BaseObj, BaseDat.ID, BaseDat.nm, CompareObj, CompareDat.ID, CompareDat.nm)

                            'type brug
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Bridge Type"
                            If BaseDef.tb <> CompareDef.tb Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.tb
                            Summary(i, 4) = CompareDef.tb

                            If BaseDef.tb = CompareDef.tb Then
                                If BaseDef.tb = GeneralFunctions.enmCFBridgeType.PILLAR Then

                                    'heeft alleen total pillar width en shape factor
                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "Pillar Width"
                                    If BaseDef.pw <> CompareDef.pw Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.pw
                                    Summary(i, 4) = CompareDef.pw

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "Shape factor"
                                    If BaseDef.vf <> CompareDef.vf Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.vf
                                    Summary(i, 4) = CompareDef.vf

                                Else
                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "Bottom level"
                                    If BaseDef.rl <> CompareDef.rl Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.rl
                                    Summary(i, 4) = CompareDef.rl

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "Length"
                                    If BaseDef.dl <> CompareDef.dl Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.dl
                                    Summary(i, 4) = CompareDef.dl

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "Inlet Loss Coef"
                                    If BaseDef.li <> CompareDef.li Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.li
                                    Summary(i, 4) = CompareDef.li

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "Outlet Loss Coef"
                                    If BaseDef.lo <> CompareDef.lo Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.lo
                                    Summary(i, 4) = CompareDef.lo

                                    i += 1
                                    Summary(i, 0) = BaseDat.ID
                                    Summary(i, 1) = "Profile definition ID"
                                    If BaseDef.si <> CompareDef.si Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.si
                                    Summary(i, 4) = CompareDef.si

                                End If
                            End If

                            'Stromingsrichting
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Flow direction"
                            If BaseDef.rt <> CompareDef.rt Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.rt
                            Summary(i, 4) = CompareDef.rt

                            'dedicated friction for this structure
                            i += 1
                            Summary(i, 0) = BaseDat.ID
                            Summary(i, 1) = "Dedicated Structure Friction"
                            If (BaseFric Is Nothing AndAlso CompareFric Is Nothing) OrElse (Not BaseFric Is Nothing AndAlso Not CompareFric Is Nothing) Then Summary(i, 2) = "Identical" Else Summary(i, 1) = "Different"
                            Summary(i, 3) = Not IsNothing(BaseFric)
                            Summary(i, 4) = Not IsNothing(CompareFric)

                            If Not BaseFric Is Nothing AndAlso Not CompareFric Is Nothing Then
                                i += 1
                                Summary(i, 0) = BaseDat.ID
                                Summary(i, 1) = "Friction Type"
                                If BaseFric.mf <> CompareFric.mf Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseFric.mf
                                Summary(i, 4) = CompareFric.mf

                                i += 1
                                Summary(i, 0) = BaseDat.ID
                                Summary(i, 1) = "Friction Value"
                                If BaseFric.mtcpConst <> CompareFric.mtcpConst Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseFric.mtcpConst
                                Summary(i, 4) = CompareFric.mtcpConst
                            End If

                        End If
                    Else
                        i += 1
                        Summary(i, 0) = BaseDat.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = BaseDat.ID
                        Summary(i, 4) = ""
                    End If
                End If
            Next
        Next

        'zoek nu vanuit de referentiecase naar bruggen die niet in de basiscase voorkomen
        For Each myReach As clsSbkReach In CompareCase.CFTopo.Reaches.Reaches.Values
            For Each CompareObj In myReach.ReachObjects.ReachObjects.Values
                If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge Then
                    'zoek dezelfde onderlaat op in de andere case
                    BaseObj = BaseCase.CFTopo.getSimilarReachObject(CompareObj, MaxSearchRadius)
                    If BaseObj Is Nothing Then
                        i += 1
                        Summary(i, 1) = CompareObj.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = ""
                        Summary(i, 4) = CompareObj.ID
                    End If
                End If
            Next
        Next

        Return Summary
    End Function


    Public Sub CompareCFControllers(ByRef Summary(,) As Object, ByRef i As Long, ByRef BaseDat As clsStructDatRecord, ByRef BaseContr As clsControlDefRecord, ByRef CompareContr As clsControlDefRecord)

        'controller types zijn gelijk
        If BaseContr.ct = CompareContr.ct Then

            'eerst een paar zaken die in alle controllers voorkomen
            i += 1
            Summary(i, 0) = BaseDat.ID
            Summary(i, 1) = "Control frequency"
            If BaseContr.cf <> CompareContr.cf Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
            Summary(i, 3) = BaseContr.cf
            Summary(i, 4) = CompareContr.cf

            i += 1
            Summary(i, 0) = BaseDat.ID
            Summary(i, 1) = "Controlled parameter"
            If BaseContr.ca <> CompareContr.ca Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
            Summary(i, 3) = BaseContr.ca
            Summary(i, 4) = CompareContr.ca

            If BaseContr.ct = 0 AndAlso CompareContr.ct = 0 Then
                'time controller
                i += 1
                Summary(i, 0) = BaseDat.ID
                Summary(i, 1) = "Controlled parameter"
                If BaseContr.ca <> CompareContr.ca Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                Summary(i, 3) = BaseContr.ca
                Summary(i, 4) = CompareContr.ca

                i += 1
                Summary(i, 0) = BaseDat.ID
                Summary(i, 1) = "Length of controller timetable"
                If BaseContr.TimeTable.Dates.Count <> CompareContr.TimeTable.Dates.Count Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                Summary(i, 3) = BaseContr.TimeTable.Dates.Count
                Summary(i, 4) = CompareContr.TimeTable.Dates.Count

                i += 1
                Summary(i, 0) = BaseDat.ID
                Summary(i, 1) = "Timespan of controller timetable"
                If BaseContr.TimeTable.Dates.Values(0) <> CompareContr.TimeTable.Dates.Values(0) OrElse BaseContr.TimeTable.Dates.Values(BaseContr.TimeTable.Dates.Count - 1) <> CompareContr.TimeTable.Dates.Values(CompareContr.TimeTable.Dates.Count - 1) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                Summary(i, 3) = BaseContr.TimeTable.Dates.Count
                Summary(i, 4) = CompareContr.TimeTable.Dates.Count

                i += 1
                Summary(i, 0) = BaseDat.ID
                Summary(i, 1) = "Values of controller timetable"
                If BaseContr.TimeTable.Values1.Values(0) <> CompareContr.TimeTable.Values1.Values(0) OrElse BaseContr.TimeTable.Values1.Values(BaseContr.TimeTable.Dates.Count - 1) <> CompareContr.TimeTable.Values1.Values(CompareContr.TimeTable.Dates.Count - 1) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                Summary(i, 3) = BaseContr.TimeTable.Dates.Count
                Summary(i, 4) = CompareContr.TimeTable.Dates.Count

            ElseIf BaseContr.ct = 1 AndAlso CompareContr.ct = 1 Then

                'hydraulic controller
                Summary(i, 0) = BaseDat.ID
                Summary(i, 1) = "Measurement Location ID"
                If BaseContr.ml <> CompareContr.ml Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                Summary(i, 3) = BaseContr.TimeTable.Dates.Count
                Summary(i, 4) = CompareContr.TimeTable.Dates.Count

            ElseIf BaseContr.ct = 2 AndAlso CompareContr.ct = 2 Then
                'interval controller
            ElseIf BaseContr.ct = 3 AndAlso CompareContr.ct = 3 Then
                'PID controller
            End If

        End If

    End Sub

    Public Function CompareCFProfiles(ByRef BaseCase As clsSobekCase, ByRef CompareCase As clsSobekCase, MaxSearchRadius As Double) As Object(,)

        Dim i As Long, j As Long = 0
        Dim BaseDat As clsProfileDatRecord, BaseDef As clsProfileDefRecord
        Dim CompareDat As clsProfileDatRecord, CompareDef As clsProfileDefRecord
        Dim CompareObj As clsSbkReachObject, BaseObj As clsSbkReachObject
        Dim BaseCRFR As clsFrictionDatCRFRRecord, BaseBDFR As clsFrictionDatBDFRRecord
        Dim CompareCRFR As clsFrictionDatCRFRRecord, CompareBDFR As clsFrictionDatBDFRRecord
        Dim BaseVal As Double, CompareVal As Double
        Dim Summary(,) As Object
        ReDim Summary(100000, 8)

        'doorloop alle profielen!
        i = 0
        Summary(i, 0) = "Cross Section ID"
        Summary(i, 1) = "Parameter"
        Summary(i, 2) = "Verdict"
        Summary(i, 3) = BaseCase.CaseName
        Summary(i, 4) = CompareCase.CaseName

        Me.setup.GeneralFunctions.UpdateProgressBar("Comparing 1D Flow profiles...", 0, 10)

        'in detail de takobjecten
        For Each myReach As clsSbkReach In BaseCase.CFTopo.Reaches.Reaches.Values
            j += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", j, BaseCase.CFTopo.Reaches.Reaches.Values.Count)

            For Each BaseObj In myReach.ReachObjects.ReachObjects.Values

                If BaseObj.InUse = False Then Continue For

                'de gemalen
                If BaseObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE Then

                    BaseObj.calcXY()
                    BaseDat = BaseCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(BaseObj.ID.Trim.ToUpper)
                    BaseDef = BaseCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(BaseDat.di.Trim.ToUpper)
                    BaseCRFR = BaseCase.CFData.Data.FrictionData.FrictionDatCRFRRecords.getRecord(BaseDef.ID.Trim.ToUpper)
                    BaseBDFR = BaseCase.CFData.Data.FrictionData.FrictionDatBDFRRecords.getRecord(BaseObj.ci.Trim.ToUpper)
                    'zoek hetzelfde profiel in de andere case
                    CompareObj = CompareCase.CFTopo.getSimilarReachObject(BaseObj, MaxSearchRadius)
                    If Not CompareObj Is Nothing AndAlso CompareObj.InUse Then
                        If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE Then

                            'we hebben een object met hetzelfde ID gevonden!
                            CompareObj.calcXY()
                            CompareDat = CompareCase.CFData.Data.ProfileData.ProfileDatRecords.Records.Item(CompareObj.ID.Trim.ToUpper)
                            CompareDef = CompareCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(CompareDat.di.Trim.ToUpper)
                            CompareCRFR = CompareCase.CFData.Data.FrictionData.FrictionDatCRFRRecords.getRecord(CompareDef.ID.Trim.ToUpper)
                            CompareBDFR = CompareCase.CFData.Data.FrictionData.FrictionDatBDFRRecords.getRecord(CompareObj.ci.Trim.ToUpper)

                            CompareCFTopography(Summary, i, BaseObj, BaseDat.ID, BaseObj.Name, CompareObj, CompareDat.ID, CompareObj.Name)

                            i += 1
                            Summary(i, 0) = BaseObj.ID
                            Summary(i, 1) = "Profile definition ID"
                            If BaseDef.ID <> CompareDef.ID Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.ID
                            Summary(i, 4) = CompareDef.ID

                            i += 1
                            Summary(i, 0) = BaseObj.ID
                            Summary(i, 1) = "Profile type"
                            If BaseDef.ty <> CompareDef.ty Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.ty
                            Summary(i, 4) = CompareDef.ty

                            i += 1
                            Summary(i, 0) = BaseObj.ID
                            Summary(i, 1) = "Maximum width"
                            If BaseDef.getMaximumWidth <> CompareDef.getMaximumWidth Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDef.getMaximumWidth
                            Summary(i, 4) = CompareDef.getMaximumWidth

                            'Bodemhoogte
                            Call BaseDat.CalculateLowestBedLevel(BaseDef)
                            Call CompareDat.CalculateLowestBedLevel(CompareDef)
                            i += 1
                            Summary(i, 0) = BaseObj.ID
                            Summary(i, 1) = "Bed level"
                            If BaseDat.CalculateLowestBedLevel(BaseDef) <> CompareDat.CalculateLowestBedLevel(CompareDef) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                            Summary(i, 3) = BaseDat.CalculateLowestBedLevel(BaseDef)
                            Summary(i, 4) = CompareDat.CalculateLowestBedLevel(CompareDef)

                            If BaseDef.ty = CompareDef.ty Then

                                If BaseDef.ty = 10 Then

                                    'Laagste maaiveldhoogte
                                    BaseVal = BaseDef.GetLowestEmbankmentLevel(True)
                                    CompareVal = CompareDef.GetLowestEmbankmentLevel(True)
                                    i += 1
                                    Summary(i, 0) = BaseObj.ID
                                    Summary(i, 1) = "Lowest Embankment"
                                    If BaseVal <> CompareVal Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDat.CalculateLowestBedLevel(BaseDef)
                                    Summary(i, 4) = CompareDat.CalculateLowestBedLevel(CompareDef)

                                    'Hoogste maaiveldhoogte
                                    BaseVal = BaseDef.GetHighestEmbankmentLevel(True)
                                    CompareVal = CompareDef.GetHighestEmbankmentLevel(True)
                                    i += 1
                                    Summary(i, 0) = BaseObj.ID
                                    Summary(i, 1) = "Highest Embankment"
                                    If BaseVal <> CompareVal Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDat.CalculateLowestBedLevel(BaseDef)
                                    Summary(i, 4) = CompareDat.CalculateLowestBedLevel(CompareDef)

                                    'aantal gegevenspunten
                                    i += 1
                                    Summary(i, 0) = BaseObj.ID
                                    Summary(i, 1) = "Number of datapoints"
                                    If BaseVal <> CompareVal Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                    Summary(i, 3) = BaseDef.ltyzTable.XValues.Count
                                    Summary(i, 4) = CompareDef.ltyzTable.XValues.Count

                                End If

                            End If

                            i += 1
                            Summary(i, 0) = BaseObj.ID
                            Summary(i, 1) = "Friction Assignment"
                            If (Not BaseCRFR Is Nothing AndAlso Not CompareCRFR Is Nothing) OrElse (Not BaseBDFR Is Nothing AndAlso Not CompareBDFR Is Nothing) OrElse (BaseCRFR Is Nothing AndAlso BaseBDFR Is Nothing AndAlso CompareCRFR Is Nothing AndAlso CompareBDFR Is Nothing) Then Summary(i, 2) = "Identical" Else Summary(i, 2) = "Different"
                            If Not BaseCRFR Is Nothing Then
                                Summary(i, 3) = "LOCAL FRICTION"
                            ElseIf Not BaseBDFR Is Nothing Then
                                Summary(i, 3) = "FRICTION BY REACH"
                            Else
                                Summary(i, 3) = "GLOBAL FRICTION"
                            End If
                            If Not CompareCRFR Is Nothing Then
                                Summary(i, 4) = "LOCAL FRICTION"
                            ElseIf Not CompareBDFR Is Nothing Then
                                Summary(i, 4) = "FRICTION BY REACH"
                            Else
                                Summary(i, 4) = "GLOBAL FRICTION"
                            End If

                            'handle local friction defs
                            If Not BaseCRFR Is Nothing AndAlso Not CompareCRFR Is Nothing Then

                                'friction table y-segments
                                i += 1
                                Summary(i, 0) = BaseObj.ID
                                Summary(i, 1) = "Number of segments"
                                If BaseCRFR.ltysTable.XValues.Count <> CompareCRFR.ltysTable.XValues.Count Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseCRFR.ltysTable.XValues.Count
                                Summary(i, 4) = CompareCRFR.ltysTable.XValues.Count

                                If BaseCRFR.ltysTable.XValues.Count = CompareCRFR.ltysTable.XValues.Count Then
                                    For j = 0 To BaseCRFR.ltysTable.XValues.Count - 1

                                        'friction type
                                        i += 1
                                        Summary(i, 0) = BaseObj.ID
                                        Summary(i, 1) = "Friction Type Segment " & j + 1
                                        If BaseCRFR.ftysTable.XValues.Values(j) <> CompareCRFR.ftysTable.XValues.Values(j) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                        Summary(i, 3) = CType(BaseCRFR.ftysTable.XValues.Values(j), GeneralFunctions.enmFrictionType)
                                        Summary(i, 4) = CType(CompareCRFR.ftysTable.XValues.Values(j), GeneralFunctions.enmFrictionType)

                                        'friction value
                                        i += 1
                                        Summary(i, 0) = BaseObj.ID
                                        Summary(i, 1) = "Friction Value Segment " & j + 1
                                        If BaseCRFR.ftysTable.Values1.Values(j) <> CompareCRFR.ftysTable.Values1.Values(j) Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                        Summary(i, 3) = BaseCRFR.ftysTable.Values1.Values(j)
                                        Summary(i, 4) = CompareCRFR.ftysTable.Values1.Values(j)
                                    Next
                                End If
                            ElseIf Not BaseBDFR Is Nothing AndAlso Not CompareBDFR Is Nothing Then

                                'friction type
                                i += 1
                                Summary(i, 0) = BaseObj.ID
                                Summary(i, 1) = "Friction Type"
                                If BaseBDFR.mf <> CompareBDFR.mf Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseBDFR.mf
                                Summary(i, 4) = CompareBDFR.mf

                                'friction value
                                i += 1
                                Summary(i, 0) = BaseObj.ID
                                Summary(i, 1) = "Friction Value"
                                If BaseBDFR.mtcpConstant <> CompareBDFR.mtcpConstant Then Summary(i, 2) = "Different" Else Summary(i, 2) = "Identical"
                                Summary(i, 3) = BaseBDFR.mtcpConstant
                                Summary(i, 4) = CompareBDFR.mtcpConstant
                            End If
                        End If
                    Else
                        i += 1
                        Summary(i, 0) = BaseObj.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = "Exists"
                        Summary(i, 4) = "Missing"
                    End If
                End If
            Next
        Next

        'zoek nu vanuit de referentiecase naar stuwen die niet in de basiscase voorkomen
        For Each myReach As clsSbkReach In CompareCase.CFTopo.Reaches.Reaches.Values
            For Each CompareObj In myReach.ReachObjects.ReachObjects.Values
                If CompareObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE Then
                    'zoek hetzelde profiel op in de andere case
                    BaseObj = BaseCase.CFTopo.getSimilarReachObject(CompareObj, MaxSearchRadius)
                    If BaseObj Is Nothing Then
                        i += 1
                        Summary(i, 0) = CompareObj.ID
                        Summary(i, 1) = "Presence"
                        Summary(i, 2) = "Different"
                        Summary(i, 3) = "Missing"
                        Summary(i, 4) = "Exists"
                    End If
                End If
            Next
        Next

        Return Summary
    End Function



    Friend Sub SetFirstModelActive()
        For Each myModel As clsSobekProject In Projects.Values
            If myModel.ProjectDir <> "" Then
                ActiveProject = myModel
            End If
        Next myModel
    End Sub

    Public Function ListAllSobekNodesOfType(ByVal NodeType As STOCHLIB.GeneralFunctions.enmNodetype) As List(Of String)
        Return setup.SOBEKData.ActiveProject.ActiveCase.ReadAllNodesOfType(NodeType)
    End Function

    Public Function populateDataGridViewComboBoxColumnWithSobekNodesOfType(ByRef myCol As System.Windows.Forms.DataGridViewComboBoxColumn, ByVal NodeType As STOCHLIB.GeneralFunctions.enmNodetype) As Boolean
        Try
            Dim myList As New List(Of String)
            myList = ListAllSobekNodesOfType(NodeType)
            Me.setup.GeneralFunctions.populateDataGridViewComboBoxColumnFromList(myCol, myList)
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function populateDataGridViewComboBoxColumnWithSobekNodesOfType of class GeneralFunctions.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function GetSobek_3bFNMFileContent() As String
        'this function simply returns the entire file content for a sobek_3b.fnm file
        Return ("*
* DELFT_3B Version 1.00
* -----------------------------------------------------------------
*
* Last update : March 1995
*
* All input- And output file names (free format)
*
*   Namen Mappix files (*.DIR, *.TST, *.his) mogen NIET gewijzigd worden.
*   Overige filenamen mogen wel gewijzigd worden.
*
*
'delft_3b.ini'                            * 1. Control file                                             I
'3b_nod.tp'                                      * 2. Knoop data                                               I
'3b_link.tp'                                     * 3. Tak data                                                      I
'3brunoff.tp'                                    * 4. Open water data                                               I
'paved.3b'                                       * 5. Verhard gebied algemeen                                       I
'paved.sto'                                      * 6. Verhard gebied storage                                        I
'paved.dwa'                                      * 7. Verhard gebied DWA                                            I
'paved.tbl'                                      * 8. Verhard gebied sewer pump capacity                            I
'pluvius.dwa'                                    * 9. Boundaries                                                    I
'pluvius.3b'                                     *10. Pluvius                                                       I
'pluvius.alg'                                    *11. Pluvius algemeen                                              I
'kasklass'                    *12. Kasklasse                                                     I
'default.bui'                                        *13. buifile                                                  I
'default.evp'                                       *14. verdampingsfile                                          I
'unpaved.3b'                                     *15. unpaved algemeen                                              I
'unpaved.sto'                                    *16. unpaved storage                                               I
'kasinit'                     *17. kasgebied initialisatie (SC)                                  I
'kasgebr'                     *18. kasgebied verbruiksdata (SC)                                  I
'cropfact'                    *19. crop factors gewassen                                         I
'bergcoef'                    *20. tabel bergingscoef=f(ontw.diepte,grondsoort)                  I
'unpaved.alf'                                    *21. Unpaved - alfa factor definities                                I
'sobek_3b.log'                            *22. Run messages                                                  O
'3b_gener.out'                            *23. Overzicht van schematisatie, algemene gegevens                O
'paved.out'                               *24. Output results verhard                                        O
'unpaved.out'                             *25. Output results onverhard                                      O
'grnhous.out'                             *26. Output results kas                                            O
'openwate.out'                            *27. Output results open water                                     O
'struct3b.out'                            *28. Output results kunstwerk                                      O
'bound3b.out'                             *29. Output results boundaries                                     O
'pluvius.out'                             *30. Output results Pluvius                                        O
'unpaved.inf'                                    *31. Unpaved infiltratie definities                                  I
'sobek_3b.dbg'                                    *32. Debugfile                                                     O
'unpaved.sep'                                    *33. Unpaved seepage                                                 I
'unpaved.tbl'                                    *34. Unpaved tabels initial gwl and Scurve                           I
'greenhse.3b'                                    *35. Kassen general data                                             I
'greenhse.rf'                                    *36. Kassen roof storage                                             I
'runoff.out'                                      *37. Pluvius rioolinloop ASCII file                                O
'sbk_rtc.his'                             *38. Invoerfile met variabele peilen op randknopen                 I
'salt.3b'                                 *39. Invoerfile met zoutgegevens                                   I
'crop_ow.prn'                 *40. Invoerfile met cropfactors open water                         I
'RSRR_IN'                                      *41. Restart file input                                            I
'RSRR_OUT'                                     *42. Restart file output                                           O
'3b_input.bin'                            *43. Binary file input                                             I
'sacrmnto.3b'                                    *44. Sacramento input I
'aanvoer.abr'                             *45. Uitvoer ASCII file met debieten van/naar randknopen           O
'saltbnd.out'                             *46. Uitvoer ASCII file met zoutconcentratie op rand               O
'salt.out'                                *47. Zout uitvoer in ASCII file                                    O
'greenhse.sil'                                   *48. Greenhouse silo definitions                                     I
'openwate.3b'                                    *49. Open water general data                                         I
'openwate.sep'                                   *50. Open water seepage definitions                                  I
'openwate.tbl'                                   *51. Open water tables target levels                                 I
'struct3b.dat'                                   *52. General structure data                                          I
'struct3b.def'                                   *53. Structure definitions                                           I
'contr3b.def'                                    *54. Controller definitions                                          I
'struct3b.tbl'                                   *55. Tabellen structures                                             I
'bound3b.3b'                                     *56. Boundary data                                                   I
'bound3b.tbl'                                    *57. Boundary tables                                                 I
'sbk_loc.rtc'                                    *58.                                                                 I
'wwtp.3b'                                        *59. Wwtp data                                                       I
'wwtp.tbl'                                       *60. Wwtp tabellen                                                   I
'industry.3b'                                    *61. Industry general data                                           I
'pvstordt.his'                            *62. Mappix output file detail berging riool verhard gebied per tijdstap  O
'pvflowdt.his'                            *63. Mappix output file detail debiet verhard gebied        per tijdstap  O
'upflowdt.his'                            *64. Mappix output file detail debiet onverhard gebied      per tijdstap  O
'upgwlvdt.his'                            *65. Mappix output file detail grondwaterstand              per tijdstap  O
'grnstrdt.his'                            *66. Mappix output file detail bergingsgraad kasbassins     per tijdstap  O
'grnflodt.his'                            *67. Mappix output file detail uitslag kasbassins           per tijdstap  O
'ow_lvldt.his'                            *68. Mappix output file detail open water peil              per tijdstap  O
'ow_excdt.his'                            *69  Mappix output file detail overschrijdingsduur ref.peil per tijdstap  O
'strflodt.his'                            *70. Mappix output file detail debiet over kunstwerk        per tijdstap  O
'bndflodt.his'                            *71. Mappix output file detail debiet naar rand             per tijdstap  O
'plvstrdt.his'                            *72. Mappix output file max.berging riool Pluvius           per tijdstap  O
'plvflodt.his'                            *73. Mappix output file max.debiet Pluvius                  per tijdstap  O
'balansdt.his'                            *74. Mappix output file detail balans                       per tijdstap  O
'cumbaldt.his'                            *75. Mappix output file detail balans cumulatief            per tijdstap  O
'saltdt.his'                            *76. Mappix output file detail zoutconcentraties            per tijdstap  O
'industry.tbl'                                   *77. Industry tabellen                                               I
'rtc_3b.his'                              *78. Maalstop                                                 I
'default.tmp'                                         *79. Temperature time series I
'rnff.#'                                         *80. Runoff time series
'bndfltot.his'                            *81. Totalen/lozingen op randknopen                           O
'sobek_3b.lng'           *82. Language file                                                   I
'ow_vol.his'                              *83. OW-volume                                                O
'ow_level.his'                            *84. OW_peilen                                                O
'3b_bal.out'                              *85. Balans file                                              O
'3bareas.his'                             *86. 3B-arealen in HIS file                                   O
'3bstrdim.his'                            *87. 3B-structure data in HIS file                            O
'rrrunoff.his'                            *88. RR Runoff his file
'sacrmnto.his'                            *89. Sacramento HIS file
'wwtpdt.his'                              *90. rwzi HIS file                                            O
'industdt.his'                            *91. Industry HIS file                                        O
'ctrl.ini'                                        *92. CTRL.INI                                                 I
'root_sim.inp'                *93. CAPSIM input file                                I
'unsa_sim.inp'                *94. CAPSIM input file                                I
'capsim.msg'                                      *95. CAPSIM message file                                      O
'capsim.dbg'                                      *96. CAPSIM debug file                                        O
'restart1.out'                                    *97. Restart file na 1 uur                                    O
'restart12.out'                                   *98. Restart file na 12 uur                                   O
'RR-ready'                                        *99. Ready                                                    O
'NwrwArea.His'                            *100. NWRW detailed areas                                     O
'3blinks.his'                             *101. Link flows                                              O
'modflow_rr.His'                          *102. Modflow-RR                                              O
'rr_modflow.His'                          *103. RR-Modflow                                              O
'rr_wlmbal.His'                           *104. RR-balance for WLM
'sacrmnto.out'                            *105. Sacramento ASCII output
'pluvius.tbl'                                    *106. Additional NWRW input file with DWA table                      I
'rrbalans.his'                            *107. RR balans
'KasKlasData.dat'             *108. Kasklasse, new format I
'KasInitData.dat'             *109. KasInit, new format I
'KasGebrData.dat'             *110. KasGebr, new format I
'CropData.dat'                *111. CropFact, new format I
'CropOWData.dat'              *112. CropOW, new format I
'SoilData.dat'                *113. Soildata, new format I
'dioconfig.ini'             *114. DioConfig Ini file
'NWRWCONT.#'                                     *115. Buifile voor continue berekening Reeksen
'NwrwSys.His'                                    *116. NWRW output
'3b_rout.3b'                                     *117. RR Routing link definitions I
'3b_cel.3b'                                    *118. Cel input file
'3b_cel.his'                              *119. Cel output file
'sobek3b_progress.txt'                            *120. RR Log file for Simulate
'wqrtc.his'                               *121. coupling WQ salt RTC
'BoundaryConditions.bc'                           *122. RR Boundary conditions file for SOBEK3
'ASCIIRestartOpenDA.txt'                          *123. Optional RR ASCII restart (test) for OpenDA
")
    End Function

End Class
