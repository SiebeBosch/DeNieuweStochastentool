
Imports STOCHLIB.General
Public Class frmImportOutputlocations
    Private Setup As clsSetup
    Private SubcatchmentShapefile As String
    Private WPField As String
    Private ZPField As String

    Public Sub New(ByRef mySetup As clsSetup, mySubcatchmentShapefile As String, myWPField As String, myZPField As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup
        SubcatchmentShapefile = mySubcatchmentShapefile
        WPField = myWPField
        ZPField = myZPField

    End Sub

    Private Sub FrmImportOutputlocations_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Setup.SetProgress(prProgress, lblProgress)

        Dim query As String = "SELECT DISTINCT MODELID FROM SIMULATIONMODELS;"
        Dim dt As New DataTable
        Me.Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dt)

        'populate the combobox with all available model id's
        cmbModelID.Items.Clear()
        For i = 0 To dt.Rows.Count - 1
            cmbModelID.Items.Add(dt.Rows(i)(0))
        Next
        Me.Setup.GeneralFunctions.PopulateComboBoxWithTimeseriesStatisticOptions(cmbResultsFilter)

    End Sub

    Private Sub BtnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click
        Try
            Dim i As Integer, j As Integer
            Dim ModelID As String
            Dim ModelDir As String
            Dim ModelType As String
            Dim CaseName As String
            Dim query As String

            Dim IDPatterns As New List(Of String)
            Dim TmpStr As String = txtIDFilter.Text
            While Not TmpStr = ""
                IDPatterns.Add(Me.Setup.GeneralFunctions.ParseString(TmpStr, ";"))
            End While

            If Not Me.Setup.SqliteCon.State = ConnectionState.Open Then Me.Setup.SqliteCon.Open()

            'remove all existing locations first
            query = "DELETE FROM OUTPUTLOCATIONS;"
            Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

            'first get the list of simulation models
            query = "SELECT DISTINCT MODELID, MODELTYPE, MODELDIR, CASENAME FROM SIMULATIONMODELS;"
            Dim dtModels As New DataTable
            Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, query, dtModels, False)

            For i = 0 To dtModels.Rows.Count - 1
                If cmbModelID.Text = dtModels.Rows(i)("MODELID") Then
                    ModelID = dtModels.Rows(i)("MODELID")
                    ModelDir = dtModels.Rows(i)("MODELDIR")
                    ModelType = dtModels.Rows(i)("MODELTYPE")
                    CaseName = dtModels.Rows(i)("CASENAME")

                    'clean up existing data for the current simulation model
                    '@siebe: later refine this by distinguishing parameters
                    query = "DELETE FROM OUTPUTLOCATIONS WHERE MODELID='" & ModelID & "';"
                    Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)

                    'this is the model we will be importing locations for
                    'this routine expands the database table OUTPUTLOCATIONS with the SOBEK H points
                    Dim dtLocations As New DataTable
                    Dim Lat As Double, Lon As Double

                    'read the subcatchment's shapefile and set the 'beginpointinshapefile' property to allow quick search of target levels
                    Dim SubcatchmentsSF As New STOCHLIB.clsShapeFile(Me.Setup, SubcatchmentShapefile)
                    If Not SubcatchmentsSF.Open() Then Throw New Exception("Error: could not open subcatchments shapefile: " & SubcatchmentsSF.Path)
                    Dim WPFieldIdx As Integer = SubcatchmentsSF.GetFieldIdx(WPField)
                    Dim ZPFieldIdx As Integer = SubcatchmentsSF.GetFieldIdx(ZPField)
                    SubcatchmentsSF.sf.BeginPointInShapefile()

                    If dtModels.Rows(i)("MODELTYPE") = "SOBEK" Then
                        'read the model schematization
                        If Not Setup.SOBEKData.ReadProject(ModelDir, True) Then Throw New Exception("SOBEK Project could Not be read: " & ModelDir)
                        If Not Setup.SetAddSobekProject(ModelDir, True, True) Then Throw New Exception("Error adding SOBEK project " & ModelDir)
                        If Not Setup.SOBEKData.ActiveProject.SetActiveCase(CaseName) Then Throw New Exception("Error setting sobek case as active: " & CaseName)
                        If Not Setup.SOBEKData.ActiveProject.ActiveCase.Read(False, True, False, False, False, "") Then Throw New Exception("Error reading active SOBEK case: " & CaseName)

                        If chkH.Checked Then
                            Dim WLPoints As New Dictionary(Of String, STOCHLIB.clsSbkVectorPoint)
                            Dim WLPoint As STOCHLIB.clsSbkVectorPoint

                            'read all waterlevel points from the model schematization
                            If Not Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.ReadCalculationPointsByReach() Then Throw New Exception("Error reading calculation points.")

                            WLPoints = New Dictionary(Of String, STOCHLIB.clsSbkVectorPoint)
                            WLPoints = Setup.SOBEKData.ActiveProject.ActiveCase.CFTopo.CollectAllWaterLevelPoints
                            Me.Setup.GeneralFunctions.UpdateProgressBar("Writing points to database...", 0, 10, True)

                            For j = 0 To WLPoints.Count - 1
                                Me.Setup.GeneralFunctions.UpdateProgressBar("", j, WLPoints.Count)
                                WLPoint = WLPoints.Values(j)

                                'apply the selection by ID
                                If txtIDFilter.Text = "" OrElse Me.Setup.GeneralFunctions.TextMatchUsingWildcards(IDPatterns, WLPoint.ID, True) Then

                                    'compute the map coordinates of our point
                                    Setup.GeneralFunctions.RD2WGS84(WLPoint.X, WLPoint.Y, Lat, Lon)

                                    'retrieve the target levels from our subcatchments shapefile
                                    Dim ShapeIdx As Integer, WP As Double, ZP As Double
                                    'SubcatchmentsSF.Open()
                                    'SubcatchmentsSF.sf.BeginPointInShapefile()
                                    ShapeIdx = SubcatchmentsSF.sf.PointInShapefile(WLPoint.X, WLPoint.Y)
                                    If ShapeIdx > 0 Then
                                        'retrieve the target levels and insert our location and its parameter in the OUTPUTLOCATIONS table
                                        WP = SubcatchmentsSF.sf.CellValue(WPFieldIdx, ShapeIdx)
                                        ZP = SubcatchmentsSF.sf.CellValue(ZPFieldIdx, ShapeIdx)
                                        query = "INSERT INTO OUTPUTLOCATIONS (LOCATIEID, LOCATIENAAM, MODELID, MODELPAR, RESULTSFILE, RESULTSTYPE, X, Y, LAT, LON, ZP, WP) VALUES ('" & WLPoint.ID & "','" & WLPoint.ID & "','" & ModelID & "','" & "Water" & "','calcpnt.his','" & cmbResultsFilter.Text & "'," & WLPoint.X & "," & WLPoint.Y & "," & Lat & "," & Lon & "," & Math.Round(ZP, 2) & "," & Math.Round(WP, 2) & ");"
                                        Setup.GeneralFunctions.SQLiteNoQuery(Me.Setup.SqliteCon, query, False)
                                    Else
                                        'could not find target levels. Only write the location and parameters
                                        query = "INSERT INTO OUTPUTLOCATIONS (LOCATIEID, LOCATIENAAM, MODELID, MODELPAR, RESULTSFILE, RESULTSTYPE, X, Y, LAT, LON) VALUES ('" & WLPoint.ID & "','" & WLPoint.ID & "','" & ModelID & "','" & "Water" & "','calcpnt.his','" & cmbResultsFilter.Text & "'," & WLPoint.X & "," & WLPoint.Y & "," & Lat & "," & Lon & ");"
                                        Me.Setup.Log.AddError("Could not retrieve underlying shape for waterlevel location " & WLPoint.ID)
                                    End If
                                End If

                            Next

                        End If

                    End If

                    SubcatchmentsSF.sf.EndPointInShapefile()
                    SubcatchmentsSF.Close()

                    'now that we have read all output locations, refresh the datagridview containing our output locations

                    Me.Setup.SqliteCon.Close()
                    Setup.GeneralFunctions.UpdateProgressBar("klaar", 0, 1)
                    Me.Setup.Log.write(Setup.Settings.RootDir & "\logfile.txt", True)
                    Me.Setup.Log.Clear()

                End If
            Next
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            Me.Close()
        End Try
    End Sub
End Class