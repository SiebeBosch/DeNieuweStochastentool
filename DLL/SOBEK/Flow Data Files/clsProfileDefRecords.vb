Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions

Friend Class clsProfileDefRecords
    Friend Records As New Dictionary(Of String, clsProfileDefRecord)
    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each Def As clsProfileDefRecord In Records.Values
            Def.ID = Prefix & Def.ID
            Def.nm = Prefix & Def.nm
        Next
    End Sub

    Public Function GetRecord(ID As String) As clsProfileDefRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            Return Records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Sub InitializeExport(ExportDir As String)
        If System.IO.File.Exists(ExportDir & "\profile.def") Then System.IO.File.Delete(ExportDir & "\profile.def")
    End Sub

    Friend Function Read(ByVal myStrings As Collection) As Boolean
        Dim myRecord As clsProfileDefRecord = Nothing

        Try
            Dim i As Integer = 0
            Me.setup.GeneralFunctions.UpdateProgressBar("Reading profile definitions...", 0, myStrings.Count)
            For Each myString As String In myStrings
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
                myRecord = New clsProfileDefRecord(Me.setup, Me.SbkCase)
                myRecord.Read(myString)

                If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                    Me.setup.Log.AddWarning("Multiple instances of profile definition " & myRecord.ID & " found in SOBEK schematization.")
                Else
                    'alleen importeren als hij ook daadwerkelijk door een profile.dat of struct.def-record wordt aangeroepen
                    If Not SbkCase.CFData.Data.ProfileData.ProfileDatRecords.FindByDefinitionID(myRecord.ID.Trim.ToUpper) Is Nothing Then
                        Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                    ElseIf SbkCase.CFData.Data.StructureData.StructDefRecords.FindByProfileDefID(myRecord.ID.Trim.ToUpper) Is Nothing Then
                        Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                    End If
                End If
            Next myString
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function Read of class clsProfileDefRecords while processing " & myRecord.ID)
            Return False
        End Try

    End Function

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Dim i As Integer = 0, path As String

        If Not ExportDir Is Nothing Then
            path = ExportDir & "\profile.def"
        Else
            path = ExportDir & "\profile.def"
        End If

        Me.setup.GeneralFunctions.UpdateProgressBar("Writing profile.def file...", 0, Records.Count)
        Using profileDefWriter = New StreamWriter(path, Append)
            For Each myRecord As clsProfileDefRecord In Records.Values
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                If myRecord.InUse Then myRecord.Write(profileDefWriter)
            Next myRecord
            Me.setup.GeneralFunctions.UpdateProgressBar("Done.", 0, Records.Count)
        End Using
    End Sub

    Friend Sub AddVerticalSlot(Optional ByVal Depth As Integer = 2, Optional ByVal Width As Double = 0.2)
        Dim i As Integer = 0

        Me.setup.GeneralFunctions.UpdateProgressBar("Adding vertical slot to profile definitions.", 0, 10)
        For Each myRecord As clsProfileDefRecord In Records.Values
            i += 1
            myRecord.AddVerticalSlot(Depth, Width)
        Next
        Me.setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub

    Friend Sub FixNonAscending()
        Dim i As Integer = 0, j As Integer
        Dim myTable As clsSobekTable, AddLevel As Double = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Fixing non-ascending elevations in profile.def.", 0, 10)
        For Each myRecord As clsProfileDefRecord In Records.Values
            i += 1
            AddLevel = 0
            myTable = New clsSobekTable(Me.setup)
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
            If myRecord.ty = 0 Then 'tabulated
                For j = 0 To myRecord.ltlwTable.XValues.Count - 2
                    If myRecord.ltlwTable.XValues.Values(j + 1) <> myRecord.ltlwTable.XValues.Values(j) Then
                        myTable.XValues.Add(j, myRecord.ltlwTable.XValues.Values(j))
                        If myRecord.ltlwTable.Values1.Count > 0 Then myTable.Values1.Add(j, myRecord.ltlwTable.Values1.Values(j))
                        If myRecord.ltlwTable.Values2.Count > 0 Then myTable.Values2.Add(j, myRecord.ltlwTable.Values2.Values(j))
                    ElseIf j = myRecord.ltlwTable.XValues.Count - 2 Then
                        'we zitten op de een na laatste, dus kunnen de laatste simpelweg 1 mm verhogen
                        AddLevel = 0.001
                        myTable.XValues.Add(j, myRecord.ltlwTable.XValues.Values(j))
                        If myRecord.ltlwTable.Values1.Count > 0 Then myTable.Values1.Add(j, myRecord.ltlwTable.Values1.Values(j))
                        If myRecord.ltlwTable.Values2.Count > 0 Then myTable.Values2.Add(j, myRecord.ltlwTable.Values2.Values(j))
                    Else
                        Me.setup.Log.AddWarning("Non-ascending level found in profile definition " & myRecord.ID & " level " & myRecord.ltlwTable.XValues.Values(j))
                    End If
                Next
                'en de laatste
                myTable.XValues.Add(j, myRecord.ltlwTable.XValues.Values(j) + AddLevel)
                If myRecord.ltlwTable.Values1.Count > 0 Then myTable.Values1.Add(j, myRecord.ltlwTable.Values1.Values(j))
                If myRecord.ltlwTable.Values1.Count > 0 Then myTable.Values2.Add(j, myRecord.ltlwTable.Values2.Values(j))
                myRecord.ltlwTable = myTable
            End If
        Next
        Me.setup.GeneralFunctions.UpdateProgressBar("Done.", 0, 10)
    End Sub

    Friend Sub exportYZProfileSectionsToShapeFile(ByVal FileNameBase As String, ByVal OnlyStartEnd As Boolean)
        'Date: 30-5-2013
        'Author: Siebe Bosch
        'Description: this routine exports cross section
        'Note: for this function to work the lyyz-table must not only contain YZ-values (Xvalues and Values1)
        'but also X and Y-coordinates (Values2 and Values3)
        Dim newSF As New MapWinGIS.Shapefile, newShape As MapWinGIS.Shape, FieldIdx As Integer
        Dim ShapeIdx As Integer, i As Long, SFPath As String, LastIdx As Long

        Try

            'create a new instance of a MapWinGIS Shapefile
            SFPath = setup.Settings.ExportDirRoot & "\" & FileNameBase & ".shp"
            If System.IO.File.Exists(SFPath) Then Me.setup.GeneralFunctions.deleteShapeFile(SFPath)
            If Not newSF.CreateNew(SFPath, MapWinGIS.ShpfileType.SHP_POLYLINE) Then Throw New Exception("Error creating shapefile from cross section lines.")

            'set editing mode
            newSF.StartEditingShapes()
            newSF.StartEditingTable()
            FieldIdx = newSF.EditAddField("ID", MapWinGIS.FieldType.STRING_FIELD, 1, 50)  'creates an ID field and returns field index

            'walk through all records
            For Each myRecord As clsProfileDefRecord In Records.Values
                If myRecord.ty = enmProfileType.yztable Then 'is YZ-cross section type
                    If myRecord.ltyzTable.XValues.Count > 0 AndAlso myRecord.ltyzTable.Values2.Count = myRecord.ltyzTable.XValues.Count AndAlso myRecord.ltyzTable.Values3.Count = myRecord.ltyzTable.Values2.Count Then

                        'we have found a profile definition that actually contains XY-coordinates. Let's start building a shape for it
                        newShape = New MapWinGIS.Shape
                        newShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)

                        If OnlyStartEnd Then
                            'add only the first and last point
                            LastIdx = myRecord.ltyzTable.Values2.Count - 1
                            newShape.AddPoint(myRecord.ltyzTable.Values2.Values(0), myRecord.ltyzTable.Values3.Values(0))
                            newShape.AddPoint(myRecord.ltyzTable.Values2.Values(LastIdx), myRecord.ltyzTable.Values3.Values(LastIdx))
                        Else
                            'add all the points that make up our cross section to the shape
                            For i = 0 To myRecord.ltyzTable.Values2.Count - 1
                                newShape.AddPoint(myRecord.ltyzTable.Values2.Values(i), myRecord.ltyzTable.Values3.Values(i))
                            Next
                        End If

                        'add the drapeline to the shapefile
                        ShapeIdx = newSF.EditAddShape(newShape)
                        newSF.EditCellValue(FieldIdx, ShapeIdx, myRecord.ID)

                    End If
                End If
            Next

            'save and close the new shapefile
            newSF.StopEditingTable()
            newSF.StopEditingShapes()
            newSF.Save()
            newSF.Close()

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
        End Try


    End Sub

End Class
