Option Explicit On
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports System.IO

Public Class clsSobekProject
    Friend Caselist As clsSobekCaseList
    Public Cases As New Dictionary(Of String, clsSobekCase)
    Public ProjectDir As String
    Friend ProgramsDir As String
    Public ActiveCase As clsSobekCase
    Friend IsActiveModel As Boolean

    Private setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByVal myProjectDir As String, ByVal ProgramsDir As String, Optional ByVal readCases As Boolean = True)
        Me.setup = mySetup
        Me.ProjectDir = myProjectDir
        Me.ProgramsDir = ProgramsDir
        If readCases Then ReadCaseList()
    End Sub

    Friend Sub New(ByRef mySetup As clsSetup)
        'creates a totally empty project
        Me.setup = mySetup
        IsActiveModel = True
    End Sub

    Public Function GetCase(ByVal CaseName As String) As clsSobekCase
        If Cases.ContainsKey(CaseName.Trim.ToUpper) Then
            Return Cases.Item(CaseName.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function


    Public Function CountActiveCases() As Integer
        Dim n As Integer = 0
        For Each myCase As clsSobekCase In Cases.Values
            If myCase.InUse Then n += 1
        Next
        Return n
    End Function

    Public Function CloneCaseForCommandLineRun(ByVal SobekDir As String, ByVal CaseName As String, ByVal CopyToDir As String, Optional ByVal BuiFile As String = "", Optional ByVal EvpFile As String = "", Optional ByVal QscFile As String = "", Optional ByVal WdcFile As String = "", Optional ByVal QwcFile As String = "", Optional ByVal TmpFile As String = "", Optional ByVal RnfFile As String = "") As Boolean
        '---------------------------------------------------------------------------------------------------
        'author: Siebe Bosch
        'date: 16-8-2014
        'description: copies a SOBEK data structure in such a way that it can run from the new location
        '---------------------------------------------------------------------------------------------------

        '---------------------------------------------------------------------------------------------------
        'How to enable SOBEK to be run from the command line (roughly)
        '1. Copy the following content to the new dir: <casedir>, <FIXED>, <NewStart>, <WORK> and all loose files
        '2. Copy all content from <casedir> naar <WORK>
        '3. Copy .BUI and .EVP-file to de CMTWORK-DIR (optional; you can also adjust casedesc.cmt and simulate.ini and let them refer to the existing dirs or any other dir for that matter
        '4. Replace some hard paths in CMTWORK to e.g. WDC en QWC. Kan door keihard het pad naar de SOBEK-DIR te maken of door de betreffende bestanden naar CMTWORK te kopieren en dan een relatieve verwijzinga ant emaken.
        '5. Copy simulate.ini and simulate.dat from PROGRAMS and parsen.ini from PROGRAMS/FLOW naar de CMTWORK-dir
        '- Replace in simulate.ini het pad naar de rainfall event onder EventFile=@...
        '- Simulate.ini still contains some path directions starting with @. Replace by the correct relative path, probably @ by ..\WORK\
        '6. Copy casedesc.cmt uit de <casedir> naar CMTWORK en vervang alle relatieve padverwijzingen door ..\WORK
        '7. Run from CMTWORK: c:\SobekXXX\Programs\simulate.exe .\simulate.ini
        '---------------------------------------------------------------------------------------------------

        Try
            Dim myCase As ClsSobekCase
            For Each myCase In Cases.Values
                If myCase.CaseName.Trim.ToUpper = CaseName.Trim.ToUpper Then

                    'we've found the case! Clone it
                    Dim CaseDir As String = CopyToDir & "\" & LastDirFromDir(myCase.CaseDir)
                    Dim HashDir As String = ProjectDir & "\#"
                    Dim WorkDir As String = CopyToDir & "\WORK"
                    Dim CMTWorkDir As String = CopyToDir & "\CMTWORK"
                    Dim FixedDir As String = CopyToDir & "\FIXED"
                    Dim NewStartDir As String = CopyToDir & "\NEWSTART"

                    'first clean up any old directories
                    If Directory.Exists(CaseDir) Then Directory.Delete(CaseDir, True)
                    If Directory.Exists(WorkDir) Then Directory.Delete(WorkDir, True)
                    If Directory.Exists(CMTWorkDir) Then Directory.Delete(CMTWorkDir, True)
                    If Directory.Exists(FixedDir) Then Directory.Delete(FixedDir, True)
                    If Directory.Exists(NewStartDir) Then Directory.Delete(NewStartDir, True)

                    'copy the necessary directories
                    Call DirectoryCopy(myCase.CaseDir, CaseDir, True)
                    Call DirectoryCopy(myCase.CaseDir, WorkDir, True) 'opmerking: dit is CORRECT! de content vd casedir moet naar de nieuwe workdir
                    'Call DirectoryCopy(ProjectDir & "\CMTWORK", CMTWorkDir, True)
                    Call DirectoryCopy(ProjectDir & "\FIXED", FixedDir, False)
                    Call DirectoryCopy(ProjectDir & "\NEWSTART", NewStartDir, True)

                    'copy the project files located directly under .LIT
                    For Each myFile As String In Directory.GetFiles(ProjectDir)
                        Call FileCopy(myFile, CopyToDir & "\" & setup.GeneralFunctions.FileNameFromPath(myFile))
                    Next

                    'next, copy simulate.ini, simulate.dat from Programs to the new CMTWORK
                    'and copy casedesc.cmt from the casedir to the new CMTWORK dir
                    'and copy parsen.ini from the FLOW-dir to CMTWORK
                    If Not Directory.Exists(CMTWorkDir) Then Directory.CreateDirectory(CMTWorkDir)
                    Call FileCopy(ProgramsDir & "\simulate.ini", CMTWorkDir & "\simulate.ini")
                    Call FileCopy(ProgramsDir & "\simulate.dat", CMTWorkDir & "\simulate.dat")
                    Call FileCopy(ProgramsDir & "\FLOW\parsen.ini", CMTWorkDir & "\parsen.ini")
                    Call FileCopy(myCase.CaseDir & "\casedesc.cmt", CMTWorkDir & "\casedesc.cmt")

                    'adjust all absolute paths in files in the cmtwork dir
                    'siebe 14-2-2018 toegevoegd: ook verwijzingen in de WORK-dir aanpassen
                    Dim SubDir As String
                    SubDir = "\CMTWORK"

                    For Each myFile As String In Directory.GetFiles(CopyToDir & SubDir)

                        'replace all semi-absolute paths to the case directory, defined relatively to the root dir by a reference to ..\WORK
                        Call ReplaceStringInFile(myFile, "\" & DirectoryRelativeToRoot(myCase.CaseDir), "..\WORK", CompareMethod.Text)
                        Call ReplaceStringInFile(myFile, "\" & DirectoryRelativeToRoot(HashDir), "..\WORK", CompareMethod.Text)
                        Call ReplaceStringInFile(myFile, "\" & DirectoryRelativeToRoot(ProjectDir & "\WORK"), "..\WORK", CompareMethod.Text)

                        'replace all relative paths to the casedir to a relative reference to ..\WORK
                        Call ReplaceStringInFile(myFile, "..\" & LastDirFromDir(myCase.CaseDir), "..\WORK", CompareMethod.Text)
                        Call ReplaceStringInFile(myFile, "..\#", "..\WORK", CompareMethod.Text)
                        Call ReplaceStringInFile(myFile, "@", "..\WORK\", CompareMethod.Text)

                        'v2.040: removed the replacements below since some references to meteo events were incorrectly replaced
                        'replace references to the models root dir. first the absolute paths; then the relative ones
                        'Call ReplaceStringInFile(myFile, SobekDir, "..\", CompareMethod.Text)
                        'Call ReplaceStringInFile(myFile, " \" & DirectoryRelativeToRoot(SobekDir) & "\", " ..\", CompareMethod.Text)

                        'LET OP: het enige wat hierna nog moet gebeuren is het verwijzen naar de juiste METEO-bestanden.
                    Next

                    If Not BuiFile = "" Then
                        'adjust all paths in the new cmtwork\simulate.ini
                        'relative paths to the meteo event are already given as arguments. Just replace the path references inside the cmt file
                        Call ReplaceStringInFile(CopyToDir & SubDir & "\simulate.ini", "EventFile=..\WORK\event.#", "EventFile=" & BuiFile, CompareMethod.Text)
                        Call ReplaceMeteoReferences(CopyToDir & SubDir & "\casedesc.cmt", BuiFile, EvpFile, QscFile, WdcFile, QwcFile, TmpFile, RnfFile)
                    Else
                        'we'll copy the meteo-events to the FIXED-directory in the temporary location and adjust the file reference in casedesc.cmt
                        Call MoveMeteoEventsAndReferences(CopyToDir & SubDir & "\casedesc.cmt", "..\FIXED", BuiFile)
                        Call ReplaceStringInFile(CopyToDir & SubDir & "\simulate.ini", "EventFile=..\WORK\event.#", "EventFile=" & BuiFile, CompareMethod.Text)
                    End If

                    Return True
                End If
            Next

            'if we end up here, the case was not found
            Throw New Exception("Could not find SOBEK case to clone: " & CaseName & " in the project " & ProjectDir)
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function MoveMeteoEventsAndReferences(ByVal CasedescCmtFile As String, ByVal NewMeteoDir As String, ByRef BuiFileRelative As String) As Boolean

        Dim output As String = ""
        Dim myLine As String
        Dim oldpath As String = "", newpath As String = ""
        Dim Ext As String = ""
        Dim myFile As String = ""

        Try

            'read the file and adjust the paths
            Using myReader As New StreamReader(CasedescCmtFile)
                While Not myReader.EndOfStream
                    myLine = myReader.ReadLine()

                    If InStr(myLine, ".BUI ", CompareMethod.Text) > 0 Then

                        'adjust the path to the .bui file inside the casedesc.cmt record
                        output &= ChangeMeteoPathInCasedescRecord(myLine, ".BUI", NewMeteoDir, oldpath, newpath) & vbCrLf
                        BuiFileRelative = newpath 'return the relative path to the rainfall event for adjustment in simulate.ini

                        'copy the old file to the new location. First make the paths absolute for this
                        oldpath = setup.GeneralFunctions.RelativeToAbsolutePath(oldpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        newpath = setup.GeneralFunctions.RelativeToAbsolutePath(newpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        System.IO.File.Copy(oldpath, newpath, True)

                    ElseIf InStr(myLine, ".EVP ", CompareMethod.Text) > 0 Then

                        'adjust the path to the .bui file inside the casedesc.cmt record
                        output &= ChangeMeteoPathInCasedescRecord(myLine, ".EVP", NewMeteoDir, oldpath, newpath) & vbCrLf

                        'copy the old file to the new location. First make the paths absolute for this
                        oldpath = setup.GeneralFunctions.RelativeToAbsolutePath(oldpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        newpath = setup.GeneralFunctions.RelativeToAbsolutePath(newpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        System.IO.File.Copy(oldpath, newpath, True)

                    ElseIf InStr(myLine, ".WDC ", CompareMethod.Text) > 0 Then

                        'adjust the path to the .bui file inside the casedesc.cmt record
                        output &= ChangeMeteoPathInCasedescRecord(myLine, ".WDC", NewMeteoDir, oldpath, newpath) & vbCrLf

                        'copy the old file to the new location. First make the paths absolute for this
                        oldpath = setup.GeneralFunctions.RelativeToAbsolutePath(oldpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        newpath = setup.GeneralFunctions.RelativeToAbsolutePath(newpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        System.IO.File.Copy(oldpath, newpath, True)

                    ElseIf InStr(myLine, ".QWC ", CompareMethod.Text) > 0 Then

                        'adjust the path to the .bui file inside the casedesc.cmt record
                        output &= ChangeMeteoPathInCasedescRecord(myLine, ".QWC", NewMeteoDir, oldpath, newpath) & vbCrLf

                        'copy the old file to the new location. First make the paths absolute for this
                        oldpath = setup.GeneralFunctions.RelativeToAbsolutePath(oldpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        newpath = setup.GeneralFunctions.RelativeToAbsolutePath(newpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        System.IO.File.Copy(oldpath, newpath, True)

                    ElseIf InStr(myLine, ".QSC ", CompareMethod.Text) > 0 Then

                        'adjust the path to the .bui file inside the casedesc.cmt record
                        output &= ChangeMeteoPathInCasedescRecord(myLine, ".QSC", NewMeteoDir, oldpath, newpath) & vbCrLf

                        'copy the old file to the new location. First make the paths absolute for this
                        oldpath = setup.GeneralFunctions.RelativeToAbsolutePath(oldpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        newpath = setup.GeneralFunctions.RelativeToAbsolutePath(newpath, setup.GeneralFunctions.DirFromFileName(CasedescCmtFile))
                        System.IO.File.Copy(oldpath, newpath, True)

                    Else
                        output &= myLine & vbCrLf
                    End If
                End While
            End Using

            'write the resulting casedesc.cmt file
            Using mywriter As New StreamWriter(CasedescCmtFile)
                mywriter.Write(output)
            End Using

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Public Function ChangeMeteoPathInCasedescRecord(ByVal myLine As String, ByVal meteoFileExtension As String, ByVal NewMeteoDir As String, ByRef oldpath As String, ByRef newpath As String) As String
        Dim newLine As String
        Dim spacepos As Long

        'extract the .bui path from the casedesc line and replace it with the new one
        newLine = setup.GeneralFunctions.ParseString(myLine, " ")
        spacepos = InStr(myLine, meteoFileExtension, CompareMethod.Text) + 4
        oldpath = Left(myLine, spacepos - 1)
        newpath = NewMeteoDir & "\" & setup.GeneralFunctions.FileNameFromPath(oldpath)
        newLine &= " " & newpath & " " & Right(myLine, myLine.Length - spacepos)
        Return newLine
    End Function

    Public Function ReplaceMeteoReferences(ByVal CasedescCmtFile As String, ByVal BuiFile As String, ByVal EvpFile As String, ByVal QscFile As String, ByVal WdcFile As String, ByVal QwcFile As String, ByVal TmpFile As String, ByVal RnfFile As String) As Boolean

        Dim output As String = ""
        Dim myLine As String
        Dim tmpLine As String, tmpStr As String
        Dim newLine As String
        Dim i As Integer, Ext As String = ""
        Dim myFile As String = ""

        Try

            'read the file and adjust the paths
            Using myReader As New StreamReader(CasedescCmtFile)
                While Not myReader.EndOfStream
                    myLine = myReader.ReadLine()

                    'check for each of the seven file extentions
                    For i = 1 To 8

                        'note: a select case caused a complete crash of VS, so we replaced it by an if--then--else
                        If i = 1 Then
                            Ext = ".BUI"
                            myFile = BuiFile
                        ElseIf i = 2 Then
                            Ext = ".RKS"
                            myFile = BuiFile
                        ElseIf i = 3 Then
                            Ext = ".EVP"
                            myFile = EvpFile
                        ElseIf i = 4 Then
                            Ext = ".QSC"
                            myFile = QscFile
                        ElseIf i = 5 Then
                            Ext = ".WDC"
                            myFile = WdcFile
                        ElseIf i = 6 Then
                            Ext = ".QWC"
                            myFile = QwcFile
                        ElseIf i = 7 Then
                            Ext = ".TMP"
                            myFile = TmpFile
                        ElseIf i = 8 Then
                            Ext = ".RNF"
                            myFile = RnfFile
                        End If

                        If InStr(myLine, Ext & " ", CompareMethod.Text) > 0 Then
                            tmpLine = myLine
                            newLine = ""
                            While Not tmpLine = ""
                                tmpStr = setup.GeneralFunctions.ParseString(tmpLine, " ", 0)
                                If InStr(tmpStr, Ext, CompareMethod.Text) > 0 Then tmpStr = myFile 'replace the path by our buifile
                                If newLine = "" Then
                                    newLine = tmpStr
                                Else
                                    newLine &= " " & tmpStr
                                End If
                            End While
                            myLine = newLine.ToUpper
                            Exit For
                        End If
                    Next
                    output &= myLine & vbCrLf
                End While
            End Using

            'write the result
            Using mywriter As New StreamWriter(CasedescCmtFile)
                mywriter.Write(output)
            End Using

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Public Function setReferenceCaseForActiveCase(ByVal CaseName As String) As Boolean
        'finds and sets a reference case for the current activecase. This is necessary for results comparison between two cases
        If Cases.ContainsKey(CaseName.Trim.ToUpper) Then
            ActiveCase.setReferenceCase(Cases.Item(CaseName.Trim.ToUpper))
            Return True
        Else
            Return False
        End If

    End Function

    Public Function AddEmptyCase(ByVal CaseName As String) As Boolean
        Dim myItem = New clsSobekCaseListItem(Me.setup)
        myItem.name = CaseName
        Dim myCase = New clsSobekCase(Me.setup, Me, myItem)

        Try
            myCase.PopulateStandardNodeTypes()
            myCase.PopulateStandardBranchTypes()

            If Not Cases.ContainsKey(myItem.name.Trim.ToUpper) Then
                Cases.Add(myItem.name.Trim.ToUpper, myCase)

                If SetActiveCase(myItem.name) Then
                    Return True
                Else
                    Throw New Exception("Could not set case " & CaseName & " as active case.")
                End If

            Else
                Throw New Exception("Could not add case " & CaseName & " to the SOBEK Project since it was already in the collection.")
            End If

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try


    End Function

    Friend Sub ReadCaseList()
        Dim myItem As clsSobekCaseListItem
        Dim myCase As clsSobekCase
        Caselist = New clsSobekCaseList(Me.setup, Me)
        Caselist.Read()

        For Each myItem In Caselist.Cases
            myCase = New clsSobekCase(Me.setup, Me, myItem)
            myCase.InUse = False              'initialiseer InUse op False. Zet later op True als de case daadwerkelijk ingelezen wordt
            Call Cases.Add(myCase.CaseName.Trim.ToUpper, myCase)
        Next myItem

        'add the work directory
        myItem = New clsSobekCaseListItem(Me.setup)
        myItem.dir = ProjectDir & "\WORK"
        myItem.name = "WORK"
        myCase = New clsSobekCase(Me.setup, Me, myItem)
        Cases.Add(myCase.CaseName.Trim.ToUpper, myCase)
    End Sub
    ''' <summary>
    ''' TODO: Siebe invullen
    ''' </summary>
    ''' <param name="casename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>

    Public Function SetActiveCase(ByVal casename As String) As Boolean
        Dim myCase As clsSobekCase

        'zet alle andere cases (ook uit andere models) op inactive
        For Each myModel As clsSobekProject In setup.SOBEKData.Projects.Values
            For Each myCase In myModel.Cases.Values
                myCase.IsActiveCase = False
            Next
        Next

        'activeeer nu de aangewezen case
        For Each myCase In Cases.Values
            If myCase.CaseName.Trim.ToLower = casename.Trim.ToLower Then
                myCase.IsActiveCase = True
                ActiveCase = myCase
                Return True
            End If
        Next

        Return False
    End Function

    Public Sub StatsByVectorPoint2Excel(ByVal WaterLevels As Boolean, ByVal IncludeTides As Boolean)
        Dim ReferenceCaseFound As Boolean = False
        Dim maxSheet As clsExcelSheet
        Dim Row As Long = 0, Col As Long = 0
        Dim myCase As clsSobekCase, VectorPoints As New Dictionary(Of String, clsSbkVectorPoint)
        Dim myVectorPoint As clsSbkVectorPoint
        Dim upCP As clsSbkReachObject = Nothing, downCP As clsSbkReachObject = Nothing

        'create a worksheet for the results
        maxSheet = setup.ExcelFile.GetAddSheet("Max by vectorpoint")

        'lees nu voor iedere case de resultaten in, en interpoleer naar de vectorpunten uit de REFERENTIECASE om ze weg te schrijven
        Col = 0
        For Each myCase In Cases.Values
            If myCase.InUse Then
                Row = 0
                Col += 1

                'schrijf de header naar het werkblad "Maxima"
                maxSheet.ws.Cells(Row, 0).Value = "VectorID"
                maxSheet.ws.Cells(Row, Col).Value = myCase.CaseName

                Me.setup.GeneralFunctions.UpdateProgressBar("Translating results to vector points", Row, VectorPoints.Count)
                For Each myVectorPoint In VectorPoints.Values
                    Row += 1
                    Me.setup.GeneralFunctions.UpdateProgressBar("", Row, VectorPoints.Count)
                    maxSheet.ws.Cells(Row, 0).Value = myVectorPoint.ID

                    'zoek de boven en benedenstroomse rekenpunten op in het referentiemodel
                    If myCase.CFData.Results.CalcPnt.Stats.Stats.ContainsKey(myVectorPoint.upCP.ID.Trim.ToUpper) AndAlso myCase.CFData.Results.CalcPnt.Stats.Stats.ContainsKey(myVectorPoint.downCP.ID.Trim.ToUpper) Then
                        'rekenpunten gevonden, dus interpoleer
                        maxSheet.ws.Cells(Row, Col).Value = setup.GeneralFunctions.Interpolate(myVectorPoint.upCP.lc, myCase.CFData.Results.CalcPnt.Stats.Stats.Item(myVectorPoint.upCP.ID.Trim.ToUpper).Max, myVectorPoint.downCP.lc, myCase.CFData.Results.CalcPnt.Stats.Stats.Item(myVectorPoint.downCP.ID.Trim.ToUpper).Max, myVectorPoint.Dist)
                    Else
                        'rekenpunten niet gevonden in de case, dus nemen we de waarden uit de referentiecase over
                        Me.setup.Log.AddWarning("Calculation point(s) for reach " & myVectorPoint.ReachID & " not found in case " & myCase.CaseName & ". Model results for reference case were adopted instead.")
                        maxSheet.ws.Cells(Row, Col).Value = setup.GeneralFunctions.Interpolate(myVectorPoint.upCP.lc, myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myVectorPoint.upCP.ID.Trim.ToUpper).Max, myVectorPoint.downCP.lc, myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myVectorPoint.downCP.ID.Trim.ToUpper).Max, myVectorPoint.Dist)
                    End If

                Next
            End If
        Next

    End Sub

    Public Sub StatsByCalculationPoint2Excel(ByVal WaterLevels As Boolean, Optional ByVal IncludeTides As Boolean = False)
        Dim ReferenceCaseFound As Boolean = False
        Dim maxSheet As clsExcelSheet
        Dim Row As Long = 0, Col As Long = 0
        Dim myCase As clsSobekCase
        Dim myResultsPoint As clsSbkVectorPoint

        maxSheet = Me.setup.ExcelFile.GetAddSheet("Max.Calcpnt")

        'lees nu voor iedere case de resultaten in om ze vervolgens weg te schrijven naar het werkblad
        Col = -3
        For Each myCase In Cases.Values
            If myCase.InUse Then
                Row = 0
                Col += 3

                'schrijf de header naar het werkblad "Maxima"
                maxSheet.ws.Cells(Row, Col).Value = "NodeID"
                maxSheet.ws.Cells(Row, Col + 1).Value = myCase.CaseName
                maxSheet.ws.Cells(Row, Col + 2).Value = myCase.ReferenceCase.CaseName

                Me.setup.GeneralFunctions.UpdateProgressBar("Populating statistics of results", Row, myCase.CFTopo.VectorPoints.Count)
                For Each myResultsPoint In myCase.CFTopo.VectorPoints.Values
                    Row += 1
                    Me.setup.GeneralFunctions.UpdateProgressBar("", Row, myCase.CFTopo.VectorPoints.Count)
                    maxSheet.ws.Cells(Row, Col).Value = myResultsPoint.VectorID

                    'haal de waterstanden uit de case
                    If myCase.CFData.Results.CalcPnt.Stats.Stats.ContainsKey(myResultsPoint.ID.Trim.ToUpper) Then
                        maxSheet.ws.Cells(Row, Col + 1).Value = myCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Max
                    Else
                        'niet gevonden, dus neem de waarde uit de referentiecase
                        Me.setup.Log.AddWarning("Calculation point(s) for reach " & myResultsPoint.ReachID & " not found in case " & myCase.CaseName & ". Model results from reference case were adopted instead.")
                        maxSheet.ws.Cells(Row, Col + 1).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Max
                    End If
                    maxSheet.ws.Cells(Row, Col + 2).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Max
                Next
            End If
        Next

    End Sub


    Public Sub StatsByCalculationPoint2Pivot(ByVal WaterLevels As Boolean, Optional ByVal IncludeTides As Boolean = False, Optional ByVal ARIPrefix As String = "", Optional ByVal ScenarioPrefix As String = "", Optional ByVal MaatregelPrefix As String = "", Optional ByVal KlimaatPrefix As String = "", Optional ByVal Separator As String = "")
        Dim statSheet As clsExcelSheet
        Dim Row As Long = 0, Col As Long = 0
        Dim myCase As clsSobekCase
        Dim myResultsPoint As clsSbkVectorPoint
        Dim upCP As clsSbkReachObject = Nothing, downCP As clsSbkReachObject = Nothing
        Dim KlimaatScenario As String = "", Maatregel As String = "", Herhalingstijd As String = "", Scenario As String = ""

        statSheet = Me.setup.ExcelFile.GetAddSheet("Stats")

        'schrijf de topologie (calculation points) naar het werkblad topoSheet
        Me.setup.GeneralFunctions.UpdateProgressBar("Calculating statistics by calculation point.", 0, 10)

        'lees nu voor iedere case de resultaten in om ze vervolgens weg te schrijven naar het werkblad
        Row = 0
        Col = 0

        'schrijf de header naar het werkblad "Stats"
        statSheet.ws.Cells(Row, Col + 0).Value = "VectorID"
        statSheet.ws.Cells(Row, Col + 1).Value = "NodeID"
        statSheet.ws.Cells(Row, Col + 2).Value = "SOBEK Case"
        statSheet.ws.Cells(Row, Col + 3).Value = "Herhalingstijd"
        statSheet.ws.Cells(Row, Col + 4).Value = "Scenario"
        statSheet.ws.Cells(Row, Col + 5).Value = "Maatregel"
        statSheet.ws.Cells(Row, Col + 6).Value = "Klimaatscenario"
        statSheet.ws.Cells(Row, Col + 7).Value = "Max.H"
        statSheet.ws.Cells(Row, Col + 8).Value = "Min.H"
        statSheet.ws.Cells(Row, Col + 9).Value = "Gem.H"
        statSheet.ws.Cells(Row, Col + 10).Value = "Max.H.Ref"
        statSheet.ws.Cells(Row, Col + 11).Value = "Min.H.Ref"
        statSheet.ws.Cells(Row, Col + 12).Value = "Gem.H.Ref"

        For Each myCase In Cases.Values
            If myCase.InUse Then

                If ARIPrefix <> "" Then Herhalingstijd = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, ARIPrefix, Separator)
                If ScenarioPrefix <> "" Then Scenario = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, ScenarioPrefix, Separator)
                If MaatregelPrefix <> "" Then Maatregel = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, MaatregelPrefix, Separator)
                If KlimaatPrefix <> "" Then KlimaatScenario = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, KlimaatPrefix, Separator)

                Me.setup.GeneralFunctions.UpdateProgressBar("Populating statistics of results", Row, myCase.ReferenceCase.CFTopo.WaterLevelPoints.Count)
                For Each myResultsPoint In myCase.ReferenceCase.CFTopo.WaterLevelPoints.Values
                    Row += 1
                    Me.setup.GeneralFunctions.UpdateProgressBar("", Row, myCase.ReferenceCase.CFTopo.WaterLevelPoints.Count)
                    statSheet.ws.Cells(Row, Col + 0).Value = myResultsPoint.VectorID
                    statSheet.ws.Cells(Row, Col + 1).Value = myResultsPoint.ID
                    statSheet.ws.Cells(Row, Col + 2).Value = myCase.CaseName
                    statSheet.ws.Cells(Row, Col + 3).Value = Herhalingstijd
                    statSheet.ws.Cells(Row, Col + 4).Value = Scenario
                    statSheet.ws.Cells(Row, Col + 5).Value = Maatregel
                    statSheet.ws.Cells(Row, Col + 6).Value = KlimaatScenario

                    'haal de waterstanden uit de case
                    If myCase.CFData.Results.CalcPnt.Stats.Stats.ContainsKey(myResultsPoint.ID.Trim.ToUpper) Then
                        statSheet.ws.Cells(Row, Col + 7).Value = myCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Max
                        statSheet.ws.Cells(Row, Col + 8).Value = myCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Min
                        statSheet.ws.Cells(Row, Col + 9).Value = myCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Mean
                        statSheet.ws.Cells(Row, Col + 10).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Max
                        statSheet.ws.Cells(Row, Col + 11).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Min
                        statSheet.ws.Cells(Row, Col + 12).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Mean
                    ElseIf myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.ContainsKey(myResultsPoint.ID.Trim.ToUpper) Then
                        'niet gevonden, dus neem de waarde uit de referentiecase
                        Me.setup.Log.AddWarning("Calculation point(s) for reach " & myResultsPoint.ReachID & " not found in case " & myCase.CaseName & ". Model results for reference case were adopted instead.")
                        statSheet.ws.Cells(Row, Col + 7).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Max
                        statSheet.ws.Cells(Row, Col + 8).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Min
                        statSheet.ws.Cells(Row, Col + 9).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Mean
                        statSheet.ws.Cells(Row, Col + 10).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Max
                        statSheet.ws.Cells(Row, Col + 11).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Min
                        statSheet.ws.Cells(Row, Col + 12).Value = myCase.ReferenceCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Mean
                    Else
                        Me.setup.Log.AddError("Could not find results for object " & myResultsPoint.ID & ", not even in the reference case. Please check.")
                    End If

                Next
            End If
        Next

        'If IncludeTides Then TidesToExcelPivot(myCase.ReferenceCase, myCase.ReferenceCase.CFTopo.VectorPoints, ARIPrefix, ScenarioPrefix, MaatregelPrefix, KlimaatPrefix, Separator)

    End Sub

    Public Sub ResultsByCalculationPoint2Excel(ByVal WaterLevels As Boolean)
        Dim ReferenceCaseFound As Boolean = False
        Dim lvlSheet As clsExcelSheet
        Dim Row As Long = 0, lastRow As Long = 0, Col As Long = 0
        Dim myCase As clsSobekCase
        Dim i As Long
        Dim myResultsPoint As clsSbkVectorPoint
        Dim upCP As clsSbkReachObject = Nothing, downCP As clsSbkReachObject = Nothing
        Dim myResults As clsTimeTable, refResults As clsTimeTable
        Dim myRecord As clsTimeTableRecord, refRecord As clsTimeTableRecord

        lvlSheet = Me.setup.ExcelFile.GetAddSheet("Waterlevels")

        'lees nu voor iedere case de resultaten in om ze vervolgens weg te schrijven naar het werkblad
        Col = -1
        For Each myCase In Cases.Values
            If myCase.InUse Then
                Row = 0
                Col += 3

                'schrijf de header naar het werkblad
                lvlSheet.ws.Cells(Row, Col).Value = "VectorID"
                lvlSheet.ws.Cells(Row, Col + 1).Value = "NodeId"
                lvlSheet.ws.Cells(Row, Col + 2).Value = "Datum"
                lvlSheet.ws.Cells(Row, Col + 3).Value = myCase.CaseName
                lvlSheet.ws.Cells(Row, Col + 4).Value = myCase.ReferenceCase.CaseName

                Me.setup.GeneralFunctions.UpdateProgressBar("Populating results", Row, myCase.CFTopo.VectorPoints.Count)
                For Each myResultsPoint In myCase.CFTopo.VectorPoints.Values

                    'read the results from the current case
                    lastRow = Row
                    myResults = New clsTimeTable(Me.setup)
                    refResults = New clsTimeTable(Me.setup)
                    If Not myCase.CFData.Results.CalcPnt.ReadAddLocationResults(myResultsPoint.ID, "Waterl*", myResults, 0, True) Then
                        Me.setup.Log.AddError("Could not read SOBEK Results for location " & myResultsPoint.ID)
                    Else

                        'also read the results from the reference case
                        If Not myCase.ReferenceCase.CFData.Results.CalcPnt.ReadAddLocationResults(myResultsPoint.ID, "Waterl*", refResults, 0, True) Then
                            refResults = myResults
                            Me.setup.Log.AddError("Could not find node " & myResultsPoint.ID & " in reference case. Active case was used instead.")
                        End If

                        For i = 0 To myResults.Records.Count - 1
                            myRecord = myResults.Records.Values(i)
                            refRecord = refResults.Records.Values(i)

                            Row += 1
                            Me.setup.GeneralFunctions.UpdateProgressBar("", Row, myResults.Records.Count)
                            lvlSheet.ws.Cells(Row, Col).Value = myResultsPoint.VectorID
                            lvlSheet.ws.Cells(Row, Col + 1).Value = myResultsPoint.ID
                            lvlSheet.ws.Cells(Row, Col + 2).Value = myRecord.Datum
                            lvlSheet.ws.Cells(Row, Col + 3).Value = myRecord.GetValue(0)
                            lvlSheet.ws.Cells(Row, Col + 4).Value = refRecord.GetValue(0)
                        Next
                    End If

                Next
            End If
        Next

    End Sub


    Public Sub ResultsByCalculationPoint2Pivot(ByVal WaterLevels As Boolean, Optional ByVal ARIPrefix As String = "", Optional ByVal ScenarioPrefix As String = "", Optional ByVal MaatregelPrefix As String = "", Optional ByVal KlimaatPrefix As String = "", Optional ByVal Separator As String = "")
        Dim ReferenceCaseFound As Boolean = False
        Dim resultsSheet As clsExcelSheet
        Dim Row As Long = 0, Col As Long = 0
        Dim myCase As clsSobekCase
        Dim myResultsPoint As clsSbkVectorPoint
        Dim upCP As clsSbkReachObject = Nothing, downCP As clsSbkReachObject = Nothing
        Dim KlimaatScenario As String = "", Maatregel As String = "", Herhalingstijd As String = "", Scenario As String = ""
        Dim myResults As clsTimeTable, refResults As clsTimeTable
        Dim i As Long, j As Long, n As Long
        Dim myRecord As clsTimeTableRecord, refRecord As clsTimeTableRecord

        resultsSheet = Me.setup.ExcelFile.GetAddSheet("Results.Calcpnt")

        Me.setup.GeneralFunctions.UpdateProgressBar("Populating reachobjects for the reference case.", 0, 1)

        'lees nu voor iedere case de resultaten in om ze vervolgens weg te schrijven naar het werkblad
        Row = 0
        Col = 0

        'schrijf de header naar het werkblad "Stats"
        resultsSheet.ws.Cells(Row, Col + 0).Value = "VectorID"
        resultsSheet.ws.Cells(Row, Col + 1).Value = "NodeID"
        resultsSheet.ws.Cells(Row, Col + 2).Value = "SOBEK Case"
        resultsSheet.ws.Cells(Row, Col + 3).Value = "Referentiecase"
        resultsSheet.ws.Cells(Row, Col + 4).Value = "Herhalingstijd"
        resultsSheet.ws.Cells(Row, Col + 5).Value = "Scenario"
        resultsSheet.ws.Cells(Row, Col + 6).Value = "Maatregel"
        resultsSheet.ws.Cells(Row, Col + 7).Value = "Klimaatscenario"
        resultsSheet.ws.Cells(Row, Col + 8).Value = "Datum"
        resultsSheet.ws.Cells(Row, Col + 9).Value = "H (m NAP)"
        resultsSheet.ws.Cells(Row, Col + 10).Value = "Href (m NAP)"
        resultsSheet.ws.Cells(Row, Col + 11).Value = "Verschil (cm)"

        For Each myCase In Cases.Values
            If myCase.InUse Then

                If ARIPrefix <> "" Then Herhalingstijd = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, ARIPrefix, Separator)
                If ScenarioPrefix <> "" Then Scenario = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, ScenarioPrefix, Separator)
                If MaatregelPrefix <> "" Then Maatregel = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, MaatregelPrefix, Separator)
                If KlimaatPrefix <> "" Then KlimaatScenario = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, KlimaatPrefix, Separator)

                Me.setup.GeneralFunctions.UpdateProgressBar("Populating results", 0, 10)
                n = myCase.ReferenceCase.CFTopo.WaterLevelPoints.Values.Count
                For i = 0 To n - 1
                    myResultsPoint = myCase.ReferenceCase.CFTopo.WaterLevelPoints.Values(i)
                    Me.setup.GeneralFunctions.UpdateProgressBar("", i, n)

                    'now walk through all results for this location
                    myResults = New clsTimeTable(Me.setup)
                    refResults = New clsTimeTable(Me.setup)
                    If Not myCase.CFData.Results.CalcPnt.ReadAddLocationResults(myResultsPoint.ID, "Waterl*", myResults, 0, True) Then
                        Me.setup.Log.AddError("Could not read SOBEK Results for location " & myResultsPoint.ID)
                    Else
                        'also read the results from the reference case
                        Call myCase.ReferenceCase.CFData.Results.CalcPnt.ReadAddLocationResults(myResultsPoint.ID, "Waterl*", refResults, 0, True)

                        For j = 0 To myResults.Records.Values.Count - 1
                            myRecord = myResults.Records.Values(j)
                            refRecord = refResults.Records.Values(j)
                            Row += 1
                            Me.setup.GeneralFunctions.UpdateProgressBar("", Row, myCase.ReferenceCase.CFTopo.WaterLevelPoints.Count)
                            resultsSheet.ws.Cells(Row, Col + 0).Value = myResultsPoint.VectorID
                            resultsSheet.ws.Cells(Row, Col + 1).Value = myResultsPoint.ID
                            resultsSheet.ws.Cells(Row, Col + 2).Value = myCase.CaseName
                            resultsSheet.ws.Cells(Row, Col + 3).Value = myCase.ReferenceCase.CaseName
                            resultsSheet.ws.Cells(Row, Col + 4).Value = Herhalingstijd
                            resultsSheet.ws.Cells(Row, Col + 5).Value = Scenario
                            resultsSheet.ws.Cells(Row, Col + 6).Value = Maatregel
                            resultsSheet.ws.Cells(Row, Col + 7).Value = KlimaatScenario
                            resultsSheet.ws.Cells(Row, Col + 8).Value = myRecord.Datum
                            resultsSheet.ws.Cells(Row, Col + 9).Value = myRecord.GetValue(0)
                            resultsSheet.ws.Cells(Row, Col + 10).Value = refRecord.GetValue(0)
                            resultsSheet.ws.Cells(Row, Col + 11).Value = (resultsSheet.ws.Cells(Row, Col + 9).Value - resultsSheet.ws.Cells(Row, Col + 10).Value) * 100
                        Next
                    End If
                Next
            End If
        Next

    End Sub

    Public Sub TidesToExcelPivot(ByRef RefCase As clsSobekCase, ByRef VectorPoints As Dictionary(Of String, clsSbkVectorPoint), Optional ByVal ARIPrefix As String = "", Optional ByVal ScenarioPrefix As String = "", Optional ByVal MaatregelPrefix As String = "", Optional ByVal KlimaatPrefix As String = "", Optional ByVal Separator As String = "")

        Dim KlimaatScenario As String = "", Maatregel As String = "", Herhalingstijd As String = "", Scenario As String = ""
        Dim Row As Long, Col As Long, n As Long
        Dim tidalSheet As clsExcelSheet

        tidalSheet = Me.setup.ExcelFile.GetAddSheet("Tides")

        'schrijf de header naar het werkblad "Tides"
        tidalSheet.ws.Cells(Row, Col + 0).Value = "VectorID"
        tidalSheet.ws.Cells(Row, Col + 1).Value = "SOBEK Case"
        tidalSheet.ws.Cells(Row, Col + 2).Value = "Herhalingstijd"
        tidalSheet.ws.Cells(Row, Col + 3).Value = "Scenario"
        tidalSheet.ws.Cells(Row, Col + 4).Value = "Maatregel"
        tidalSheet.ws.Cells(Row, Col + 5).Value = "Klimaatscenario"
        tidalSheet.ws.Cells(Row, Col + 6).Value = "Datum"
        tidalSheet.ws.Cells(Row, Col + 7).Value = "Getij"
        tidalSheet.ws.Cells(Row, Col + 8).Value = "Waterhoogte"

        For Each myCase In Cases.Values
            If myCase.InUse Then

                If ARIPrefix <> "" Then Herhalingstijd = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, ARIPrefix, Separator)
                If ScenarioPrefix <> "" Then Scenario = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, ScenarioPrefix, Separator)
                If MaatregelPrefix <> "" Then Maatregel = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, MaatregelPrefix, Separator)
                If KlimaatPrefix <> "" Then KlimaatScenario = Me.setup.GeneralFunctions.IDFROMSTRING(myCase.CaseName, KlimaatPrefix, Separator)

                Me.setup.GeneralFunctions.UpdateProgressBar("Populating statistics of results", Row, VectorPoints.Count)
                For Each myResultsPoint In VectorPoints.Values
                    n += 1
                    Me.setup.GeneralFunctions.UpdateProgressBar("", n, VectorPoints.Count)

                    If myCase.CFData.Results.CalcPnt.Stats.Stats.ContainsKey(myResultsPoint.ID.Trim.ToUpper) Then
                        'haal de getijdendata uit de case
                        For Each myStat As clsHisFileStat In myCase.CFData.Results.CalcPnt.Stats.Stats.Values
                            For Each myTide As clsTide In myStat.HighTides.Values
                                Row += 1
                                tidalSheet.ws.Cells(Row, Col + 0).Value = myResultsPoint.ID
                                tidalSheet.ws.Cells(Row, Col + 1).Value = myCase.CaseName
                                tidalSheet.ws.Cells(Row, Col + 2).Value = Herhalingstijd
                                tidalSheet.ws.Cells(Row, Col + 3).Value = Scenario
                                tidalSheet.ws.Cells(Row, Col + 4).Value = Maatregel
                                tidalSheet.ws.Cells(Row, Col + 5).Value = KlimaatScenario
                                tidalSheet.ws.Cells(Row, Col + 6).Value = myTide.DateTime
                                tidalSheet.ws.Cells(Row, Col + 7).Value = myTide.Tide
                                tidalSheet.ws.Cells(Row, Col + 8).Value = myTide.Value
                            Next

                            For Each myTide As clsTide In myStat.LowTides.Values
                                Row += 1
                                tidalSheet.ws.Cells(Row, Col + 0).Value = myResultsPoint.ID
                                tidalSheet.ws.Cells(Row, Col + 1).Value = myCase.CaseName
                                tidalSheet.ws.Cells(Row, Col + 2).Value = Herhalingstijd
                                tidalSheet.ws.Cells(Row, Col + 3).Value = Scenario
                                tidalSheet.ws.Cells(Row, Col + 4).Value = Maatregel
                                tidalSheet.ws.Cells(Row, Col + 5).Value = KlimaatScenario
                                tidalSheet.ws.Cells(Row, Col + 6).Value = myTide.DateTime
                                tidalSheet.ws.Cells(Row, Col + 7).Value = myTide.Tide
                                tidalSheet.ws.Cells(Row, Col + 8).Value = myTide.Value
                            Next
                        Next
                    Else
                        'niet gevonden, dus neem de waarde uit de referentiecase
                        Me.setup.Log.AddWarning("Calculation point(s) for reach " & myResultsPoint.ReachID & " not found in case " & myCase.CaseName & ". Model results for reference case were adopted instead.")
                        tidalSheet.ws.Cells(Row, Col + 6).Value = RefCase.CFData.Results.CalcPnt.Stats.Stats.Item(myResultsPoint.ID.Trim.ToUpper).Max
                    End If
                Next
            End If
        Next

    End Sub


    Friend Sub ReadActiveCase(ByVal ReadRRNetwork As Boolean, ByVal ReadCFNetwork As Boolean, ByVal ReadRRData As Boolean,
                              ByVal ReadCFData As Boolean, ByVal ReadWQNetwork As Boolean, ByVal StructureReachPrefix As String)
        If Not ActiveCase.Read(ReadRRNetwork, ReadCFNetwork, ReadRRData, ReadCFData, ReadWQNetwork, StructureReachPrefix) Then
            Dim log As String = "Error reading SOBEK Case " & ActiveCase.CaseName
            Me.setup.Log.AddError(log)
            Throw New Exception(log)
        End If
    End Sub
End Class
