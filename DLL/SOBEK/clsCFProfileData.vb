Option Explicit On
Imports STOCHLIB.General
Imports System.IO

Public Class clsCFProfileData
    Friend ProfileDatRecords As clsProfileDatRecords
    Friend ProfileDefRecords As clsProfileDefRecords
    Private setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        ProfileDatRecords = New clsProfileDatRecords(Me.setup, Me.SbkCase)
        ProfileDefRecords = New clsProfileDefRecords(Me.setup, Me.SbkCase)
    End Sub

    ''' <summary>
    ''' TODO: Siebe invullen
    ''' </summary>
    ''' <remarks></remarks>
    Friend Sub Export(ByVal Append As Boolean, ExportDir As String)
        Try
            Me.ProfileDatRecords.Write(Append, ExportDir)
            Me.ProfileDefRecords.Write(Append, ExportDir)
        Catch ex As Exception
            Dim log As String = "Error in Export CF profile data"
            Me.setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try
    End Sub

    Public Function TabulatedToDatabase(ByRef con As System.Data.SQLite.SQLiteConnection) As Boolean
        'write tabulated profiles to the database
        'introduced in v1.798
        Dim profDat As clsProfileDatRecord = Nothing
        Dim profDef As clsProfileDefRecord = Nothing
        Dim i As Integer, iReach As Integer, nReach As Integer
        Try
            con.Open()
            Me.setup.GeneralFunctions.SQLiteCreateTable(con, "TABULATEDPROFILES")
            Me.setup.GeneralFunctions.SQLiteCreateColumn(con, "TABULATEDPROFILES", "PROFILEID", GeneralFunctions.enmSQLiteDataType.SQLITETEXT, "TABULATEDPROFILEIDX")
            Me.setup.GeneralFunctions.SQLiteCreateColumn(con, "TABULATEDPROFILES", "X", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            Me.setup.GeneralFunctions.SQLiteCreateColumn(con, "TABULATEDPROFILES", "Y", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            Me.setup.GeneralFunctions.SQLiteCreateColumn(con, "TABULATEDPROFILES", "LEVEL", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            Me.setup.GeneralFunctions.SQLiteCreateColumn(con, "TABULATEDPROFILES", "WIDTH", GeneralFunctions.enmSQLiteDataType.SQLITEREAL)
            Me.setup.GeneralFunctions.SQLiteNoQuery(con, "DELETE FROM TABULATEDPROFILES;") 'remove all existing tabulated profiles data

            nReach = Me.SbkCase.CFTopo.Reaches.Reaches.Count
            iReach = 0
            Me.setup.GeneralFunctions.UpdateProgressBar("Writing tabulated cross sections to database...", 0, 10, True)

            Using cmd As New SQLite.SQLiteCommand
                cmd.Connection = con
                If Not con.State = ConnectionState.Open Then con.Open()
                For Each myReach As clsSbkReach In Me.SbkCase.CFTopo.Reaches.Reaches.Values
                    iReach += 1
                    Me.setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                    If myReach.InUse Then
                        For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values

                            If myObj.InUse AndAlso myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE Then
                                If Me.SbkCase.CFData.Data.ProfileData.getProfileRecords(myObj.ID, profDat, profDef) Then
                                    If profDef.ty = GeneralFunctions.enmProfileType.tabulated Then
                                        myObj.calcXY()
                                        'perform a bulk insert into our sqlite database for the entire profile at once
                                        Using transaction = con.BeginTransaction
                                            For i = 0 To profDef.ltlwTable.XValues.Count - 1
                                                cmd.CommandText = "INSERT INTO TABULATEDPROFILES (PROFILEID, X, Y, LEVEL, WIDTH) VALUES ('" & profDat.ID & "'," & myObj.X & "," & myObj.Y & "," & profDef.ltlwTable.XValues.Values(i) + profDat.rl & "," & profDef.ltlwTable.Values1.Values(i) & ");"
                                                cmd.ExecuteNonQuery()
                                            Next
                                            transaction.Commit() 'this is where the bulk insert is finally executed.
                                        End Using
                                    End If
                                End If
                            End If
                        Next
                    End If
                Next
            End Using

            con.Close()
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in function TabulatedToDatabase of class clsCFProfileData.")
            Return False
        End Try
    End Function

    Friend Sub AddPrefix(ByVal Prefix As String)

        ProfileDatRecords.AddPrefix(Prefix)
        ProfileDefRecords.AddPrefix(Prefix)

    End Sub

    Public Function calcWettedPerimeter(ByVal ID As String, ByVal WaterLevel As Double) As Double
        Dim profDat As clsProfileDatRecord = Nothing
        Dim profDef As clsProfileDefRecord = Nothing

        If ProfileDatRecords.Records.ContainsKey(ID.Trim.ToUpper) Then
            profDat = ProfileDatRecords.Records.Item(ID.Trim.ToUpper)
            If ProfileDefRecords.Records.ContainsKey(profDat.di.Trim.ToUpper) Then
                profDef = ProfileDefRecords.Records.Item(profDat.di.Trim.ToUpper)
                Return profDef.getWettedPerimeter(WaterLevel, profDat.rl)
            End If
        End If

        Me.setup.Log.AddError("Error in function calcWettedPerimeter when processing cross section " & ID & ". Could not find profile definition.")

    End Function

    Public Function GetProfileType(ByRef ID As String) As GeneralFunctions.enmProfileType
        Try
            Dim ProfDat As clsProfileDatRecord = Nothing
            Dim ProfDef As clsProfileDefRecord = Nothing
            ProfDat = ProfileDatRecords.Records.Item(ID.Trim.ToUpper)
            If Not ProfDat Is Nothing Then ProfDef = ProfileDefRecords.Records.Item(ProfDat.di.Trim.ToUpper) Else Throw New Exception("Error: no profile.dat record present for cross section " & ID)
            If Not ProfDef Is Nothing Then
                If Left(ProfDef.nm, 2) = "r_" Then
                    'note: rectangular profiles are in fact tabulated, but can be distinguished from those by the prefix r_ in the name
                    Return GeneralFunctions.enmProfileType.closedrectangular
                Else
                    Return ProfDef.ty
                End If
            Else
                Throw New Exception("Error retrieving profile definition for cross section " & ID)
            End If
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return 0
        End Try
    End Function

    Public Function BuildProfileDatRecord(ByVal ID As String, ByVal di As String, ByVal rl As Double, ByVal rs As Double) As clsProfileDatRecord
        Dim PDAT As New clsProfileDatRecord(Me.setup)
        PDAT.ID = ID
        PDAT.di = di
        PDAT.rl = rl
        PDAT.rs = rs
        PDAT.InUse = True
        Return PDAT
    End Function

    Public Function BuildProfileDefRecord(ByVal ID As String, ByVal ty As Integer, ByVal wm As Double, ByVal w1 As Double, ByVal w2 As Double, ByVal sw As Double) As clsProfileDefRecord
        Dim PDEF As New clsProfileDefRecord(Me.setup, SbkCase)
        PDEF.InUse = True
        PDEF.ID = ID
        PDEF.nm = ID
        PDEF.ty = ty
        PDEF.wm = wm
        PDEF.W1 = w1
        PDEF.W2 = w2
        PDEF.sw = sw

        'de overige parameters (nog niet als argument in te voeren)
        PDEF.dk = 0
        PDEF.dc = 99999
        PDEF.db = 99999
        PDEF.df = 99999
        PDEF.dt = 99999
        PDEF.gl = 0
        PDEF.gu = 0

        Return PDEF
    End Function

    Friend Sub Truncate(ByVal myOption As Integer, ByVal WidthShapeFieldIdx As Integer, ByVal ConstWidth As Integer, ByVal toDist As Double, ByVal fromDist As Double, ByVal Tabulated As Boolean, ByVal Trapezium As Boolean, ByVal OpenCircle As Boolean,
                      ByVal Sedredge As Boolean, ByVal ClosedCircle As Boolean, ByVal Type5 As Boolean, ByVal EggShape As Boolean,
                      ByVal EggShape2 As Boolean, ByVal Rectangle As Boolean, ByVal Type9 As Boolean, ByVal YZ As Boolean,
                      ByVal AsymTrapezoidal As Boolean)

        Dim myObj As clsSbkReachObject
        Dim myWidth As Integer
        Dim myShape As MapWinGIS.Shape
        Dim myUtils As New MapWinGIS.Utils
        Dim myPoint As New MapWinGIS.Point
        Dim i As Long

        'doorloop alle profile.dat-records
        For Each myDat As clsProfileDatRecord In ProfileDatRecords.Records.Values

            myWidth = 0
            If WidthShapeFieldIdx = 0 Then
                myWidth = ConstWidth
            Else
                'zoek de ligging van het profiel op, en daarna in de shapefile de gewenste afkapbreedte
                myObj = SbkCase.CFTopo.getReachObject(myDat.ID.Trim.ToUpper, False)
                myObj.calcXY()
                myPoint.x = myObj.X
                myPoint.y = myObj.Y
                For i = 0 To Me.setup.GISData.SubcatchmentDataSource.Shapefile.sf.NumShapes - 1
                    myShape = Me.setup.GISData.SubcatchmentDataSource.Shapefile.sf.Shape(i)
                    If myUtils.PointInPolygon(myShape, myPoint) Then
                        myWidth = Me.setup.GISData.SubcatchmentDataSource.Shapefile.sf.CellValue(WidthShapeFieldIdx, i)
                        Exit For
                    End If
                Next
            End If


            'zoek het bijbehorende profile.def-record
            Dim myDef As clsProfileDefRecord = ProfileDefRecords.Records(myDat.di.Trim.ToUpper)

            Select Case myDef.ty
                Case Is = 0  'tabulated
                    If Tabulated Then Me.setup.Log.AddWarning("Truncating profiles of type tabulated not yet supported.")
                Case Is = 1 'trapezium
                    If Trapezium Then Me.setup.Log.AddWarning("Truncating profiles of type trapezium not yet supported.")
                Case Is = 2
                    If OpenCircle Then Me.setup.Log.AddWarning("Truncating profiles of type open circle not yet supported.")
                Case Is = 3
                    If Sedredge Then Me.setup.Log.AddWarning("Truncating profiles of type sedredge not yet supported.")
                Case Is = 4
                    If ClosedCircle Then Me.setup.Log.AddWarning("Truncating profiles of type closed circle not yet supported.")
                Case Is = 5
                    If Type5 Then Me.setup.Log.AddWarning("Truncating profiles of type 5 not yet supported.")
                Case Is = 6
                    If EggShape Then Me.setup.Log.AddWarning("Truncating profiles of type egg shape not yet supported.")
                Case Is = 7 AndAlso EggShape2
                    If EggShape2 Then Me.setup.Log.AddWarning("Truncating profiles of type egg shape (radius) not yet supported.")
                Case Is = 8
                    If Rectangle Then Me.setup.Log.AddWarning("Truncating profiles of type rectangular not yet supported.")
                Case Is = 9
                    If Type9 Then Me.setup.Log.AddWarning("Truncating profiles of type 9 not yet supported.")
                Case Is = 10
                    If YZ Then
                        Select Case myOption
                            Case Is = 1 'breedte uit shapeveld
                                myDef.TruncateYZ(setup, myWidth)
                            Case Is = 2 'constante breedte
                                myDef.TruncateYZ(setup, myWidth)
                            Case Is = 3 'afkappen tot opgegeven afstand
                                myDef.TruncateYZtoDist(setup, toDist)
                            Case Is = 4 'afkappen vanaf een opgegeven afstand
                                myDef.TruncateYZFromDist(setup, fromDist)
                            Case Is = 5
                                myDef.TruncateYZatEmbankments()
                        End Select
                    End If
                Case Is = 11 AndAlso AsymTrapezoidal
                    Me.setup.Log.AddWarning("Truncating profiles of type asymmetrical trapezium not yet supported.")
            End Select

        Next myDat
    End Sub

    Public Function BuildProfileDatRecord(ID As String, bedlevel As Double, surfacelevel As Double) As clsProfileDatRecord
        Dim ProfDat As New clsProfileDatRecord(Me.setup)
        ProfDat.ID = ID
        ProfDat.di = ID
        ProfDat.rl = bedlevel
        ProfDat.rs = surfacelevel
        ProfDat.InUse = True
        Return ProfDat
    End Function

    Public Function BuildProfileDefRecordForTrapezium(ID As String, bedwidth As Double, leftslope As Double, rightslope As Double, maxflowwidth As Double, GroundLayer As Double, ForceAsymmetrical As Boolean) As clsProfileDefRecord
        If leftslope = rightslope AndAlso Not ForceAsymmetrical Then
            'simply create a symmetrical trapezium
            Dim myRecord As New clsProfileDefRecord(Me.setup, SbkCase)
            myRecord.ID = ID
            myRecord.nm = ID
            myRecord.InUse = True
            myRecord.ty = GeneralFunctions.enmProfileType.trapezium
            myRecord.bw = bedwidth
            myRecord.bs = leftslope
            myRecord.aw = maxflowwidth
            myRecord.gl = GroundLayer
            If myRecord.gl > 0 Then myRecord.gu = True
            Return myRecord
        Else
            Dim myRecord As New clsProfileDefRecord(Me.setup, SbkCase)
            myRecord.ID = ID
            myRecord.nm = ID
            myRecord.InUse = True
            myRecord.ty = GeneralFunctions.enmProfileType.asymmetricaltrapezium
            myRecord.st = 0
            myRecord.ltswMaaiveld = 0
            myRecord.ltyzTable = New clsSobekTable(Me.setup)

            Dim SlopeLen As Double = maxflowwidth - bedwidth
            Dim Height As Double = (maxflowwidth - bedwidth) / (leftslope + rightslope)
            Dim LeftWing As Double = -Height * leftslope - bedwidth / 2     'centralize the profile
            Dim RightWing As Double = Height * rightslope + bedwidth / 2
            myRecord.ltyzTable.AddDataPair(2, LeftWing, Height)
            myRecord.ltyzTable.AddDataPair(2, -bedwidth / 2, 0)
            myRecord.ltyzTable.AddDataPair(2, bedwidth / 2, 0)
            myRecord.ltyzTable.AddDataPair(2, RightWing, Height)
            myRecord.gl = GroundLayer
            If myRecord.gl > 0 Then myRecord.gu = True
            Return myRecord
        End If
    End Function

    Public Function BuildProfileDataForAsymTrapezium(ID As String, surfacelevel As Double, bedwidth As Double, bedlevel As Double, leftslope As Double, rightslope As Double, wbwidth_left As Double, wblevel_left As Double, wbwidth_right As Double, wblevel_right As Double, GroundLayer As Double, FrictionType As GeneralFunctions.enmFrictionType, MainChannelFrictionValue As Double, WetBermFrictionValue As Double) As clsProfileDefRecord
        Dim myRecord As New clsProfileDefRecord(Me.setup, SbkCase)
        Dim fricRecord As New clsFrictionDatCRFRRecord(Me.setup)

        fricRecord.ID = ID
        fricRecord.nm = "Friction"
        fricRecord.cs = ID
        fricRecord.InUse = True

        'we'll center our profile around bedwidth/2
        Dim LeftYNoWetberm As Double = -bedwidth / 2 - (surfacelevel - bedlevel) * leftslope
        Dim RightYNoWetberm As Double = bedwidth / 2 + (surfacelevel - bedlevel) * rightslope

        If Not Double.IsNaN(wbwidth_left) AndAlso Not Double.IsNaN(wblevel_left) AndAlso wbwidth_left > 0 AndAlso wblevel_left > bedlevel AndAlso wblevel_left < surfacelevel Then
            Dim LeftYWetberm As Double = LeftYNoWetberm - wbwidth_left
            myRecord.ltyzTable.AddDataPair(2, LeftYWetberm, surfacelevel)
            myRecord.ltyzTable.AddDataPair(2, LeftYNoWetberm + (surfacelevel - wblevel_left) * leftslope, wblevel_left)
            myRecord.ltyzTable.AddDataPair(2, LeftYNoWetberm + (surfacelevel - wblevel_left) * leftslope + wbwidth_left, wblevel_left)
            myRecord.ltyzTable.AddDataPair(2, -bedwidth / 2, bedlevel)

            'friction for the wet berm
            fricRecord.ltysTable.AddDataPair(2, LeftYWetberm, LeftYNoWetberm + (surfacelevel - wblevel_left) * leftslope)
            fricRecord.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionType), WetBermFrictionValue,,,,,,,, True)
            fricRecord.frysTable.AddDataPair(2, Convert.ToInt32(FrictionType), WetBermFrictionValue,,,,,,,, True)
            fricRecord.ltysTable.AddDataPair(2, LeftYNoWetberm + (surfacelevel - wblevel_left) * leftslope, LeftYNoWetberm + (surfacelevel - wblevel_left) * leftslope + wbwidth_left)
            fricRecord.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionType), WetBermFrictionValue,,,,,,,, True)
            fricRecord.frysTable.AddDataPair(2, Convert.ToInt32(FrictionType), WetBermFrictionValue,,,,,,,, True)
            fricRecord.ltysTable.AddDataPair(2, LeftYNoWetberm + (surfacelevel - wblevel_left) * leftslope + wbwidth_left, -bedwidth / 2)
            fricRecord.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)
            fricRecord.frysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)

        Else
            'no wet berm on left side
            myRecord.ltyzTable.AddDataPair(2, LeftYNoWetberm, surfacelevel)
            myRecord.ltyzTable.AddDataPair(2, -bedwidth / 2, bedlevel)

            'friction for this section
            fricRecord.ltysTable.AddDataPair(2, LeftYNoWetberm, -bedwidth / 2)
            fricRecord.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)
            fricRecord.frysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)
        End If

        'friction for this section
        fricRecord.ltysTable.AddDataPair(2, -bedwidth / 2, bedwidth / 2)
        fricRecord.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)
        fricRecord.frysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)

        If Not Double.IsNaN(wbwidth_right) AndAlso Not Double.IsNaN(wblevel_right) AndAlso wbwidth_right > 0 AndAlso wblevel_right > bedlevel AndAlso wblevel_right < surfacelevel Then
            Dim RightYWetberm As Double = RightYNoWetberm + wbwidth_right
            myRecord.ltyzTable.AddDataPair(2, bedwidth / 2, bedlevel)
            myRecord.ltyzTable.AddDataPair(2, bedwidth / 2 + (wblevel_right - bedlevel) * rightslope, wblevel_right)
            myRecord.ltyzTable.AddDataPair(2, bedwidth / 2 + (wblevel_right - bedlevel) * rightslope + wbwidth_right, wblevel_right)
            myRecord.ltyzTable.AddDataPair(2, RightYWetberm, surfacelevel)

            'friction for this section
            fricRecord.ltysTable.AddDataPair(2, bedwidth / 2, bedwidth / 2 + (wblevel_right - bedlevel) * rightslope)
            fricRecord.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)
            fricRecord.frysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)
            fricRecord.ltysTable.AddDataPair(2, bedwidth / 2 + (wblevel_right - bedlevel) * rightslope, bedwidth / 2 + (wblevel_right - bedlevel) * rightslope + wbwidth_right)
            fricRecord.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionType), WetBermFrictionValue,,,,,,,, True)
            fricRecord.frysTable.AddDataPair(2, Convert.ToInt32(FrictionType), WetBermFrictionValue,,,,,,,, True)
            fricRecord.ltysTable.AddDataPair(2, bedwidth / 2 + (wblevel_right - bedlevel) * rightslope + wbwidth_right, RightYWetberm)
            fricRecord.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionType), WetBermFrictionValue,,,,,,,, True)
            fricRecord.frysTable.AddDataPair(2, Convert.ToInt32(FrictionType), WetBermFrictionValue,,,,,,,, True)

        Else
            'no wet berm on right side
            myRecord.ltyzTable.AddDataPair(2, bedwidth / 2, bedlevel)
            myRecord.ltyzTable.AddDataPair(2, RightYNoWetberm, surfacelevel)

            'friction for this section
            fricRecord.ltysTable.AddDataPair(2, bedwidth / 2, RightYNoWetberm)
            fricRecord.ftysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)
            fricRecord.frysTable.AddDataPair(2, Convert.ToInt32(FrictionType), MainChannelFrictionValue,,,,,,,, True)

        End If


        myRecord.ID = ID
        myRecord.nm = ID
        myRecord.InUse = True
        myRecord.ty = GeneralFunctions.enmProfileType.yztable

        'add the friction record
        If Not SbkCase.CFData.Data.FrictionData.FrictionDatCRFRRecords.records.ContainsKey(fricRecord.ID.Trim.ToUpper) Then
            SbkCase.CFData.Data.FrictionData.FrictionDatCRFRRecords.records.Add(fricRecord.ID.Trim.ToUpper, fricRecord)
        End If

        myRecord.aw = myRecord.ltyzTable.getMaxValue(0) - myRecord.ltyzTable.getMinValue(0)
        myRecord.gl = GroundLayer
        If myRecord.gl > 0 Then myRecord.gu = True
        Return myRecord
    End Function

    Public Function BuildProfileDefRecordForAsymTrapezium(ID As String, Y As Double(), Z As Double(), GroundLayer As Double) As clsProfileDefRecord
        Dim i As Integer
        Dim myRecord As New clsProfileDefRecord(Me.setup, SbkCase)
        myRecord.ID = ID
        myRecord.nm = ID
        myRecord.InUse = True
        myRecord.ty = GeneralFunctions.enmProfileType.yztable
        For i = 0 To Y.Count - 1
            myRecord.ltyzTable.AddDataPair(2, Y(i), Z(i))
        Next
        myRecord.aw = Y(Y.Count - 1) - Y(0)
        myRecord.gl = GroundLayer
        If myRecord.gl > 0 Then myRecord.gu = True
        Return myRecord
    End Function


    Friend Function ValidateTrapeziumData(ByRef ws As clsExcelSheet, AutocorrectDubiousValues As Boolean, AutocorrectCriticalErrors As Boolean, ByVal r As Long, ByVal reachID As String, ByVal Side As String, ByVal TargetLevels As clsTargetLevels, ByRef bl As Double, ByRef bw As Double, ByRef sl As Double, ByRef mv As Double, ByRef sw As Double, ByVal DefaultWidth As Double, ByVal DefaultSlope As Double, ByVal DefaultHeight As Double, DefaultDepth As Double) As Boolean
        Try
            'main section
            ws.ws.Cells(0, 0).Value = "reachID"
            ws.ws.Cells(0, 1).Value = "Profile"
            ws.ws.Cells(0, 2).Value = "Bed level"
            ws.ws.Cells(0, 3).Value = "Bed width"
            ws.ws.Cells(0, 4).Value = "Slope"
            ws.ws.Cells(0, 5).Value = "Surface elevation"
            ws.ws.Cells(0, 6).Value = "Surface width"
            ws.ws.Cells(0, 7).Value = "Bed level overruled with:"
            ws.ws.Cells(0, 8).Value = "Bed width overruled with:"
            ws.ws.Cells(0, 9).Value = "Bed Slope overruled with:"
            ws.ws.Cells(0, 10).Value = "Surface elevation overruled with:"
            ws.ws.Cells(0, 11).Value = "Surface width overruled with:"

            ws.ws.Cells(r, 0).Value = reachID
            ws.ws.Cells(r, 1).Value = Side

            'bed level is absolutely necessary
            If Double.IsNaN(bl) Then
                ws.ws.Cells(r, 2).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
            ElseIf bl = 0 Then
                ws.ws.Cells(r, 2).Value = bl
                ws.ws.Cells(r, 2).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Yellow, Drawing.Color.Yellow)
            Else
                ws.ws.Cells(r, 2).Value = bl
            End If

            If Double.IsNaN(bw) Then
                ws.ws.Cells(r, 3).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
            ElseIf bw = 0 Then
                ws.ws.Cells(r, 3).Value = bw
                ws.ws.Cells(r, 3).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Yellow, Drawing.Color.Yellow)
            Else
                ws.ws.Cells(r, 3).Value = bw
            End If

            If Double.IsNaN(sl) Then
                ws.ws.Cells(r, 4).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
            ElseIf sl <= 0 Then
                ws.ws.Cells(r, 4).Value = sl
                ws.ws.Cells(r, 4).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
            Else
                ws.ws.Cells(r, 4).Value = sl
            End If

            If Double.IsNaN(mv) Then
                ws.ws.Cells(r, 5).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
            ElseIf mv = 0 Then
                ws.ws.Cells(r, 5).Value = mv
                ws.ws.Cells(r, 5).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Yellow, Drawing.Color.Yellow)
            Else
                ws.ws.Cells(r, 5).Value = mv
            End If

            If Double.IsNaN(sw) Then
                ws.ws.Cells(r, 6).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
            ElseIf sw = 0 OrElse sw > 100 Then
                ws.ws.Cells(r, 6).Value = sw
                ws.ws.Cells(r, 6).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Yellow, Drawing.Color.Yellow)
            Else
                ws.ws.Cells(r, 6).Value = sw
            End If

            'first execute the upstream profile
            'based on the information we have, construct a trapezium profile for up- and downstream side
            If Double.IsNaN(bl) Then
                If AutocorrectCriticalErrors Then
                    If TargetLevels.OutletHasValue Then
                        Me.setup.Log.AddWarning("Default depth was applied to profile for reach " & reachID)
                        bl = TargetLevels.getLowestOutletLevel - DefaultDepth
                        ws.ws.Cells(r, 7).Value = bl
                        ws.ws.Cells(r, 7).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                    End If
                End If
            ElseIf bl = 0 Then
                If AutocorrectDubiousValues Then
                    If TargetLevels.OutletHasValue Then
                        Me.setup.Log.AddWarning("Default depth was applied to profile for reach " & reachID)
                        bl = TargetLevels.getLowestOutletLevel - DefaultDepth
                        ws.ws.Cells(r, 7).Value = bl
                        ws.ws.Cells(r, 7).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                    End If
                End If
            End If

            If Double.IsNaN(bw) OrElse bw = 0 Then
                If AutocorrectCriticalErrors Then
                    'try to calculate bed width from the other parameters first
                    If sw > 0 AndAlso mv <> 0 AndAlso sl > 0 Then
                        bw = Math.Max(0, sw - (mv - bl) * sl * 2)
                        Me.setup.Log.AddWarning("Bed Width was constructed from surface width, surface level and slope for trapezium profile on reach " & reachID)
                        ws.ws.Cells(r, 8).Value = bw
                        ws.ws.Cells(r, 8).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                    Else
                        bw = DefaultWidth
                        Me.setup.Log.AddWarning("Default profile width was applied to profile For reach " & reachID)
                        ws.ws.Cells(r, 8).Value = DefaultWidth
                        ws.ws.Cells(r, 8).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                    End If
                End If
            End If

            'validate slope
            If (Double.IsNaN(sl) OrElse sl <= 0) AndAlso AutocorrectCriticalErrors Then
                If sw > 0 AndAlso bw > 0 AndAlso bl <> 0 AndAlso mv > bl Then
                    sl = (sw - bw) / (2 * (mv - bl))
                    Me.setup.Log.AddWarning("Slope was constructed from surface width, bed width and bed level for trapezium profile on reach " & reachID)
                    ws.ws.Cells(r, 9).Value = sl
                    ws.ws.Cells(r, 9).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                Else
                    sl = DefaultSlope
                    Me.setup.Log.AddWarning("Default profile slope was applied to profile For reach " & reachID)
                    ws.ws.Cells(r, 9).Value = DefaultSlope
                    ws.ws.Cells(r, 9).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                End If
            End If

            'validate surface elevation
            If (Double.IsNaN(mv) OrElse mv <= bl) AndAlso AutocorrectCriticalErrors Then
                ws.ws.Cells(r, 5).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                ws.ws.Cells(r, 5).Comment.Text = "invalid surface level: is NaN, <= bed level or = 0."
                If Double.IsNaN(bl) Then
                    If TargetLevels.OutletHasValue Then
                        'we do not have a bed level but we do have a target level. Derive the surface level from target level
                        mv = TargetLevels.getLowestOutletLevel + DefaultHeight
                        Me.setup.Log.AddWarning("Surface level was constructed from target level + default height for reach " & reachID)
                        ws.ws.Cells(r, 10).Comment.Text = "Value constructed from target level and default height."
                        ws.ws.Cells(r, 10).Value = mv
                        ws.ws.Cells(r, 10).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                    Else
                        'there is no way we can construct a valid cross section here. Bed level, Target level AND surface level are missing
                        Return False
                    End If
                Else
                    If bl <> 0 AndAlso sl > 0 AndAlso sw > 0 AndAlso sw > bw Then
                        mv = bl + (sw - bw) / (2 * sl)
                        Me.setup.Log.AddWarning("Surface level was constructed from surface width, bed level and bed width for trapezium profile on reach " & reachID)
                        ws.ws.Cells(r, 10).Comment.Text = "Value constructed from surface width, bed level and bed width."
                        ws.ws.Cells(r, 10).Value = mv
                        ws.ws.Cells(r, 10).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                    Else
                        mv = bl + DefaultHeight
                        If mv < TargetLevels.getZPOutlet Then
                            mv = TargetLevels.getZPOutlet + 0.2
                            Me.setup.Log.AddWarning("Profile surface elevation for reach " & reachID & " was set to target level + 20 cm.")
                        Else
                            Me.setup.Log.AddWarning("Default profile height was applied To profile For reach " & reachID)
                        End If
                        ws.ws.Cells(r, 10).Comment.Text = "Value constructed from bed level + default height."
                        ws.ws.Cells(r, 10).Value = mv
                        ws.ws.Cells(r, 10).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                    End If
                End If
            End If

            'finally validate surface width
            If Double.IsNaN(sw) Then
                ws.ws.Cells(r, 6).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                ws.ws.Cells(r, 6).Comment.Text = "invalid surface width: is NaN."
                If AutocorrectCriticalErrors Then
                    If Not Double.IsNaN(bl) AndAlso Not Double.IsNaN(sl) AndAlso Not Double.IsNaN(bw) AndAlso Not Double.IsNaN(mv) Then
                        'derive the surface width from bed level, bed width, slope and surface level
                        Me.setup.Log.AddWarning("Surface width was constructed from bed level, bed width, slope and surface level for trapezium profile on reach " & reachID)
                        sw = (mv - bl) * sl * 2 + bw
                        ws.ws.Cells(r, 11).Comment.Text = "Value constructed from bed level, bed width, slope and surface elevation."
                        ws.ws.Cells(r, 11).Value = sw
                        ws.ws.Cells(r, 11).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Aquamarine, Drawing.Color.Aquamarine)
                    End If
                End If
            End If
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function ValidateTrapeziumData.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function

    Friend Function ValidateTrapeziumWetBermData(ByRef ws As clsExcelSheet, AutocorrectDubiousValues As Boolean, AutocorrectCriticalErrors As Boolean, ByVal r As Long, ByVal reachID As String, ByVal TargetLevels As clsTargetLevels, ByVal bl As Double, ByVal sl As Double, ByRef wbwidth_left As Double, ByRef wblowest_left As Double, ByRef wbhighest_left As Double, ByRef wbsideslope_left As Double, ByRef wbwidth_right As Double, ByRef wblowest_right As Double, ByRef wbhighest_right As Double, ByRef wbsideslope_right As Double, ByVal DefaultWidth As Double, ByVal DefaultSlope As Double, ByVal DefaultHeight As Double, DefaultDepth As Double) As Boolean
        Try
            'let's continue with the wet berm section!
            ws.ws.Cells(0, 12).Value = "wet berm width left"
            ws.ws.Cells(0, 13).Value = "wet berm lowest left"
            ws.ws.Cells(0, 14).Value = "wet berm highest left"
            ws.ws.Cells(0, 15).Value = "wet berm side slope left"

            ws.ws.Cells(0, 20).Value = "wet berm width left overruled"
            ws.ws.Cells(0, 21).Value = "wet berm lowest left overruled"
            ws.ws.Cells(0, 22).Value = "wet berm highest left overruled"
            ws.ws.Cells(0, 23).Value = "wet berm side slope left overruled"

            'left side
            If Not Double.IsNaN(wbwidth_left) AndAlso wbwidth_left > 0 Then
                ws.ws.Cells(r, 12).Value = wbwidth_left
                ws.ws.Cells(r, 13).Value = wblowest_left
                ws.ws.Cells(r, 14).Value = wbhighest_left
                ws.ws.Cells(r, 15).Value = wbsideslope_left

                'first correct all nodata values if possible
                If Double.IsNaN(wblowest_left) AndAlso Double.IsNaN(wbhighest_left) Then
                    ws.ws.Cells(r, 13).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    ws.ws.Cells(r, 14).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    'skip this berm in its entirity since no elevation is available anyway
                    If AutocorrectCriticalErrors Then
                        wbwidth_left = 0
                        ws.ws.Cells(0, 20).Value = wbwidth_left
                        ws.ws.Cells(r, 20).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                ElseIf Double.IsNaN(wblowest_left) Then
                    ws.ws.Cells(r, 14).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectCriticalErrors Then
                        wblowest_left = wbhighest_left
                        ws.ws.Cells(0, 21).Value = wblowest_left
                        ws.ws.Cells(r, 21).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                ElseIf Double.IsNaN(wbhighest_left) Then
                    ws.ws.Cells(r, 15).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectCriticalErrors Then
                        wbhighest_left = wblowest_left
                        ws.ws.Cells(0, 22).Value = wbhighest_left
                        ws.ws.Cells(r, 22).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                End If

                If wblowest_left < bl Then
                    ws.ws.Cells(r, 14).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectDubiousValues Then
                        wblowest_left = TargetLevels.getWPOutlet
                        ws.ws.Cells(0, 21).Value = wblowest_left
                        ws.ws.Cells(r, 21).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                End If

                If wbhighest_left < bl Then
                    ws.ws.Cells(r, 14).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectDubiousValues Then
                        wblowest_left = TargetLevels.getWPOutlet
                        ws.ws.Cells(0, 22).Value = wblowest_left
                        ws.ws.Cells(r, 22).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                End If

                'side slope wetberm
                If Double.IsNaN(wbsideslope_left) OrElse wbsideslope_left = 0 Then
                    ws.ws.Cells(r, 15).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectCriticalErrors Then
                        wbsideslope_left = sl
                        ws.ws.Cells(0, 23).Value = wbsideslope_left
                        ws.ws.Cells(r, 23).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                End If
            End If

            ws.ws.Cells(0, 16).Value = "wet berm width right"
            ws.ws.Cells(0, 17).Value = "wet berm lowest right"
            ws.ws.Cells(0, 18).Value = "wet berm highest right"
            ws.ws.Cells(0, 19).Value = "wet berm side slope right"

            ws.ws.Cells(0, 24).Value = "wet berm width right overruled"
            ws.ws.Cells(0, 25).Value = "wet berm lowest right overruled"
            ws.ws.Cells(0, 26).Value = "wet berm highest right overruled"
            ws.ws.Cells(0, 27).Value = "wet berm side slope right overruled"

            'right side
            If Not Double.IsNaN(wbwidth_right) AndAlso wbwidth_right > 0 Then
                ws.ws.Cells(r, 16).Value = wbwidth_right
                ws.ws.Cells(r, 17).Value = wblowest_right
                ws.ws.Cells(r, 18).Value = wbhighest_right
                ws.ws.Cells(r, 19).Value = wbsideslope_right

                'first correct all nodata values if possible
                If Double.IsNaN(wblowest_right) AndAlso Double.IsNaN(wbhighest_right) Then
                    ws.ws.Cells(r, 17).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    ws.ws.Cells(r, 18).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    'skip this berm in its entirity since no elevation is available anyway
                    If AutocorrectCriticalErrors Then
                        wbwidth_right = 0
                        ws.ws.Cells(0, 24).Value = wbwidth_right
                        ws.ws.Cells(r, 24).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                ElseIf Double.IsNaN(wblowest_right) Then
                    ws.ws.Cells(r, 17).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectCriticalErrors Then
                        wblowest_right = wbhighest_right
                        ws.ws.Cells(0, 25).Value = wblowest_right
                        ws.ws.Cells(r, 25).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                ElseIf Double.IsNaN(wbhighest_right) Then
                    ws.ws.Cells(r, 18).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectCriticalErrors Then
                        wbhighest_right = wblowest_right
                        ws.ws.Cells(0, 26).Value = wbhighest_right
                        ws.ws.Cells(r, 26).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                End If

                If wblowest_right < bl Then
                    ws.ws.Cells(r, 17).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectDubiousValues Then
                        wblowest_right = TargetLevels.getWPOutlet
                        ws.ws.Cells(0, 25).Value = wblowest_right
                        ws.ws.Cells(r, 25).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                End If

                If wbhighest_right < bl Then
                    ws.ws.Cells(r, 18).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectDubiousValues Then
                        wblowest_right = TargetLevels.getWPOutlet
                        ws.ws.Cells(0, 26).Value = wblowest_right
                        ws.ws.Cells(r, 26).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                End If

                'side slope wetberm
                If Double.IsNaN(wbsideslope_right) OrElse wbsideslope_right = 0 Then
                    ws.ws.Cells(r, 15).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Red, Drawing.Color.Red)
                    If AutocorrectCriticalErrors Then
                        wbsideslope_right = sl
                        ws.ws.Cells(0, 23).Value = wbsideslope_right
                        ws.ws.Cells(r, 23).Style.FillPattern.SetPattern(GemBox.Spreadsheet.FillPatternStyle.Solid, Drawing.Color.Turquoise, Drawing.Color.Turquoise)
                    End If
                End If

            End If

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function ValidateTrapeziumWetBermData.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function



    Friend Function TruncateByFixedIncrement(ByVal Increment As Double, ByVal Tabulated As Boolean, ByVal YZ As Boolean) As Boolean
        Dim Execute As Boolean
        Try
            For Each myDef As clsProfileDefRecord In ProfileDefRecords.Records.Values
                Execute = False
                If myDef.ty = GeneralFunctions.enmProfileType.yztable AndAlso YZ Then Execute = True
                If myDef.ty = GeneralFunctions.enmProfileType.tabulated AndAlso Tabulated Then Execute = True

                If Execute Then
                    If myDef.ty = GeneralFunctions.enmProfileType.yztable Then
                        myDef.TruncateYZByFixedIncrement(Increment)
                    ElseIf myDef.ty = GeneralFunctions.enmProfileType.tabulated Then
                        myDef.TruncateTabulatedByFixedIncrement(Increment)
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function

    Friend Sub TruncateByEmbankment(ByVal Tabulated As Boolean, ByVal YZ As Boolean)
        Dim Execute As Boolean

        For Each myDef As clsProfileDefRecord In ProfileDefRecords.Records.Values
            Execute = False
            If myDef.ty = GeneralFunctions.enmProfileType.yztable AndAlso YZ Then Execute = True
            If myDef.ty = GeneralFunctions.enmProfileType.tabulated AndAlso Tabulated Then Execute = True

            If Execute Then
                If myDef.ty = GeneralFunctions.enmProfileType.yztable Then
                    myDef.TruncateYZatEmbankments(, True)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.tabulated Then
                    myDef.TruncateTabulatedAtEmbankments()
                End If
            End If

        Next
    End Sub

    Friend Function AddGroundLayerToAll(ByVal myValue As Double, Tabulated As Boolean, Trapezium As Boolean, OpenCircle As Boolean, Sedredge As Boolean, ClosedCircle As Boolean, EggShape As Boolean, Rectangular As Boolean, YZ As Boolean, AsymmetricalTrapezium As Boolean) As Boolean
        'v1.794: complete redesign of this routine. Added all possible profile types
        Try
            For Each myDef As clsProfileDefRecord In ProfileDefRecords.Records.Values
                If myDef.ty = GeneralFunctions.enmProfileType.tabulated AndAlso Tabulated Then
                    Call myDef.AddGroundLayer(setup, myValue)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.trapezium AndAlso Trapezium Then
                    Call myDef.AddGroundLayer(setup, myValue)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.opencircle AndAlso OpenCircle Then
                    Call myDef.AddGroundLayer(setup, myValue)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.sedredge AndAlso Sedredge Then
                    Call myDef.AddGroundLayer(setup, myValue)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.closedcircle AndAlso ClosedCircle Then
                    Call myDef.AddGroundLayer(setup, myValue)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.eggshape AndAlso EggShape Then
                    Call myDef.AddGroundLayer(setup, myValue)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.eggshape2 AndAlso EggShape Then
                    Call myDef.AddGroundLayer(setup, myValue)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.closedrectangular AndAlso Rectangular Then
                    Call myDef.AddGroundLayer(setup, myValue)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.yztable AndAlso YZ Then
                    Call myDef.AddGroundLayer(setup, myValue)
                ElseIf myDef.ty = GeneralFunctions.enmProfileType.asymmetricaltrapezium AndAlso AsymmetricalTrapezium Then
                    Call myDef.AddGroundLayer(setup, myValue)
                End If
            Next
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function AddGroundLayer of class clsCFProfileData.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function


    Friend Function AddGroundLayerByPolygon(ByRef Shapefile As ClsPolyShapeFile, ValueField As String, Tabulated As Boolean, Trapezium As Boolean, OpenCircle As Boolean, Sedredge As Boolean, ClosedCircle As Boolean, EggShape As Boolean, Rectangular As Boolean, YZ As Boolean, AsymmetricalTrapezium As Boolean) As Boolean
        'v1.794: newly built
        Try
            Dim ShapeIdx As Integer, Value As Double
            Dim ProfDat As clsProfileDatRecord = Nothing, ProfDef As clsProfileDefRecord = Nothing
            Dim ProfDefsProcessed As New List(Of String)
            Dim iReach As Integer = 0, nReach As Integer = SbkCase.CFTopo.Reaches.Reaches.Count
            Shapefile.Open(True)
            Shapefile.setValueField(ValueField)
            Me.setup.GeneralFunctions.UpdateProgressBar("Adding groundlayer...", 0, 10, True)
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                iReach += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
                If myReach.InUse Then
                    For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                        If myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE Then
                            myObj.calcXY()
                            ShapeIdx = Shapefile.sf.PointInShapefile(myObj.X, myObj.Y)
                            If ShapeIdx >= 0 Then
                                If getProfileRecords(myObj.ID, ProfDat, ProfDef) Then

                                    Dim Execute As Boolean = False
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.tabulated AndAlso Tabulated Then Execute = True
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.trapezium AndAlso Trapezium Then Execute = True
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.opencircle AndAlso OpenCircle Then Execute = True
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.sedredge AndAlso Sedredge Then Execute = True
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.closedcircle AndAlso ClosedCircle Then Execute = True
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.eggshape AndAlso EggShape Then Execute = True
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.eggshape2 AndAlso EggShape Then Execute = True
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.closedrectangular AndAlso Rectangular Then Execute = True
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.yztable AndAlso YZ Then Execute = True
                                    If ProfDef.ty = GeneralFunctions.enmProfileType.asymmetricaltrapezium AndAlso AsymmetricalTrapezium Then Execute = True

                                    If Execute Then
                                        Value = Shapefile.sf.CellValue(Shapefile.ValueFieldIdx, ShapeIdx)
                                        If Not ProfDefsProcessed.Contains(ProfDef.ID) Then
                                            ProfDef.AddGroundLayer(Me.setup, Value)
                                            ProfDefsProcessed.Add(ProfDef.ID)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                End If
            Next
            Me.setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
            Shapefile.Close(True)

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in function AddGroundLayer of class clsCFProfileData.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



    Friend Sub getWidestProfileDefinitionsFromOtherCases(ByRef Cases As List(Of ClsSobekCase), ByVal YZ As Boolean)
        'Deze routine zoekt in de overige cases of er profieldefinities zijn die groter (breder) zijn dan degene in de target case
        'als dat het geval is, zal hij degene in de target case vervangen
        Dim myProfDat As clsProfileDatRecord
        Dim myProfDef As clsProfileDefRecord, myProfDef2 As clsProfileDefRecord
        Dim myCase As ClsSobekCase
        For Each myProfDat In ProfileDatRecords.Records.Values
            myProfDef = ProfileDefRecords.Records.Item(myProfDat.di.Trim.ToUpper)
            If YZ = True AndAlso myProfDef.ty = 10 Then  'yz table
                For Each myCase In Cases
                    If myCase.CFData.Data.ProfileData.ProfileDefRecords.Records.ContainsKey(myProfDat.di.Trim.ToUpper) Then
                        myProfDef2 = myCase.CFData.Data.ProfileData.ProfileDefRecords.Records.Item(myProfDat.di.Trim.ToUpper)
                        If myProfDef2.ty = 10 Then
                            If myProfDef2.getMaximumWidth > myProfDef.getMaximumWidth Then
                                'we hebben een breder profiel met hetzelfde definitie-ID gevonden. Vervangen dus!
                                ProfileDefRecords.Records.Item(myProfDat.di.Trim.ToUpper) = myProfDef2
                            End If
                        End If
                    End If
                Next
            End If
        Next

    End Sub


    Friend Sub MergeProfileDefinitions(ByRef RefProfDef As clsProfileDefRecord, ByRef AddProfDef As clsProfileDefRecord)
        Dim FromZ As Double, ToZ As Double, Width As Double
        Dim i As Long

        If RefProfDef.ty = AddProfDef.ty AndAlso RefProfDef.ty = 0 Then
            'tabulated
            'zoek eerst op welke de laagste waarde heeft
            FromZ = Math.Min(RefProfDef.ltlwTable.XValues.Values(0), AddProfDef.ltlwTable.XValues.Values(0))
            ToZ = Math.Max(RefProfDef.ltlwTable.XValues.Values(RefProfDef.ltlwTable.XValues.Count - 1), AddProfDef.ltlwTable.XValues.Values(AddProfDef.ltlwTable.XValues.Count - 1))

            Dim newTable = New clsSobekTable(setup)
            'loop met stappen van 5 centimeter
            FromZ = FromZ * 100
            ToZ = ToZ * 100
            For i = FromZ To ToZ Step 5
                Width = RefProfDef.ltlwTable.getValue1(i / 100) + AddProfDef.ltlwTable.getValue1(i / 100)
                Call newTable.XValues.Add(i.ToString.Trim, i / 100)
                Call newTable.Values1.Add(i.ToString.Trim, Width)
            Next
            RefProfDef.ltswTable = newTable

        End If
    End Sub



    Friend Function setSurfaceLevelToLowestEmbankment(ByVal YZ As Boolean) As Boolean
        Dim myProfDat As clsProfileDatRecord
        Dim myProfDef As clsProfileDefRecord
        For Each myProfDat In Me.ProfileDatRecords.Records.Values
            Try
                myProfDef = ProfileDefRecords.Records.Item(myProfDat.di.Trim.ToUpper)
                If YZ = True AndAlso myProfDef.ty = 10 Then myProfDat.rs = myProfDef.GetLowestEmbankmentLevel(True)
            Catch ex As Exception
                Me.setup.Log.AddError("No profile definition found for cross section " & myProfDat.ID)
            End Try
        Next
        Return True

    End Function

    Friend Function shiftByElevationGrid() As Boolean

        Dim myReach As clsSbkReach
        Dim myObj As clsSbkReachObject
        Dim myProfDat As clsProfileDatRecord
        Dim iReach As Long, nReach As Long = SbkCase.CFTopo.Reaches.Reaches.Count
        Dim Shift As Double

        Me.setup.GeneralFunctions.UpdateProgressBar("Shifting cross sections by value from elevation grid...", 0, 10)
        For Each myReach In SbkCase.CFTopo.Reaches.Reaches.Values
            iReach += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", iReach, nReach)
            For Each myObj In myReach.ReachObjects.ReachObjects.Values
                If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE Then
                    If ProfileDatRecords.Records.ContainsKey(myObj.ID.Trim.ToUpper) Then
                        myObj.calcXY()
                        Me.setup.GISData.ElevationGrid.GetCellValueFromXY(myObj.X, myObj.Y, Shift)
                        myProfDat = ProfileDatRecords.Records.Item(myObj.ID.Trim.ToUpper)
                        myProfDat.rl += Shift
                        myProfDat.rs += Shift
                    End If
                End If
            Next
        Next

    End Function

    Public Function ShiftToZeroBased() As Boolean
        Try
            Dim ProfDat As clsProfileDatRecord = Nothing, ProfDef As clsProfileDefRecord = Nothing
            Dim LowestDefinitionValue As Double
            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    If myObj.nt = GeneralFunctions.enmNodetype.SBK_PROFILE Then
                        If getProfileRecords(myObj.ID, ProfDat, ProfDef) Then
                            Select Case ProfDef.ty
                                Case Is = GeneralFunctions.enmProfileType.yztable
                                    LowestDefinitionValue = ProfDef.GetLowestLevel()
                                    ProfDef.ShiftVertically(-LowestDefinitionValue)

                                    'now that we shifted a profile definition to zero-based, we'll have to adjust ALL profile.dat records that refer to this definition
                                    For Each ProfDat In ProfileDatRecords.Records.Values
                                        If ProfDat.di.Trim.ToUpper = ProfDef.ID.Trim.ToUpper Then
                                            ProfDat.rl += LowestDefinitionValue
                                        End If
                                    Next
                            End Select
                        Else
                            Me.setup.Log.AddError("error retrieving profile data for object " & myObj.ID)
                        End If


                    End If
                Next
            Next


            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function getProfileRecords(ByVal ID As String, ByRef ProfDat As clsProfileDatRecord, ByRef ProfDef As clsProfileDefRecord) As Boolean
        Try
            If ProfileDatRecords.Records.ContainsKey(ID.Trim.ToUpper) Then
                ProfDat = ProfileDatRecords.Records.Item(ID.Trim.ToUpper)
                If ProfDat.di Is Nothing Then Throw New Exception("No profile definition found for cross section " & ID)
                If ProfileDefRecords.Records.ContainsKey(ProfDat.di.Trim.ToUpper) Then
                    ProfDef = ProfileDefRecords.Records.Item(ProfDat.di.Trim.ToUpper)
                    Return True
                End If
            End If
            Throw New Exception("Error: profile data not complete for profile " & ID)
        Catch ex As Exception
            Return False
        End Try
    End Function

    Friend Function getMaxProfileWidth(ByVal myID As String) As Double
        Dim myProfDat As clsProfileDatRecord, myProfDef As clsProfileDefRecord
        Dim n As Integer
        If ProfileDatRecords.Records.ContainsKey(myID.Trim.ToUpper) Then
            myProfDat = ProfileDatRecords.Records.Item(myID.Trim.ToUpper)

            If ProfileDefRecords.Records.ContainsKey(myProfDat.di.Trim.ToUpper) Then
                myProfDef = ProfileDefRecords.Records.Item(myProfDat.di.Trim.ToUpper)
                Select Case myProfDef.ty
                    Case Is = 0 'tabulated
                        n = myProfDef.ltlwTable.Values1.Count
                        Return myProfDef.ltlwTable.Values1.Values(n - 1)
                    Case Is = 1 'trapezoidal
                        Return myProfDef.aw
                    Case Is = 10 'yz
                        n = myProfDef.ltyzTable.XValues.Count
                        Return (myProfDef.ltyzTable.XValues.Values(n - 1) - myProfDef.ltyzTable.XValues.Values(0))
                End Select
            Else
                Return 0
            End If

        Else
            Return 0
        End If
    End Function


    Friend Sub CheckTypesOnReach(ByRef NodesData As clsCFNodesData)
        'checks if different cross section types are located on the same reach or adjacent reaches with interpolation
        Dim i As Long
        Dim InterpolatedReaches As New List(Of clsSbkReach)
        Dim LastProfDef As clsProfileDefRecord, LastProfDat As clsProfileDatRecord = Nothing
        Dim ProfDef As clsProfileDefRecord = Nothing, ProfDat As clsProfileDatRecord = Nothing

        For i = 0 To SbkCase.CFTopo.Reaches.Reaches.Count - 1

            InterpolatedReaches = SbkCase.CFTopo.Reaches.GetInterpolatedReaches(i, NodesData)
            LastProfDef = Nothing
            LastProfDat = Nothing

            For Each myReach In InterpolatedReaches
                For Each myNode As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    If myNode.nt = GeneralFunctions.enmNodetype.SBK_PROFILE Then
                        If getProfileDefinition(myNode.ID, ProfDat, ProfDef) Then
                            If LastProfDef Is Nothing Then LastProfDef = ProfDef
                            If LastProfDat Is Nothing Then LastProfDat = ProfDat
                            If ProfDef.ty <> LastProfDef.ty Then Me.setup.Log.AddError("Cross section types differ between nodes " & ProfDat.ID & " and " & LastProfDat.ID & ".")
                            LastProfDef = ProfDef
                            LastProfDat = ProfDat
                        End If
                    End If
                Next
            Next
        Next

    End Sub

    Friend Function getProfileDefinition(ByVal ID As String, ByRef ProfDat As clsProfileDatRecord, ByRef ProfDef As clsProfileDefRecord) As Boolean
        If ProfileDatRecords.Records.ContainsKey(ID.Trim.ToUpper) Then
            ProfDat = ProfileDatRecords.Records.Item(ID.Trim.ToUpper)
            If ProfileDefRecords.Records.ContainsKey(ProfDat.di.Trim.ToUpper) Then
                ProfDef = ProfileDefRecords.Records.Item(ProfDat.di.Trim.ToUpper)
                Return True
            Else
                Me.setup.Log.AddError("Profile definition not found for cross section " & ProfDat.ID)
                Return False
            End If
        Else
            Me.setup.Log.AddError("Profile.dat record not found for cross section " & ID)
            Return False
        End If
    End Function

    Public Function GetCrossSectionBedLevel(ByVal ID As String, ByRef BedLevel As Double) As Boolean
        Dim ProfDat As clsProfileDatRecord = Nothing
        Dim ProfDef As clsProfileDefRecord = Nothing

        'zoek profile.dat en profile.def op voor het onderhavige profiel
        If getProfileDefinition(ID, ProfDat, ProfDef) Then
            BedLevel = ProfDat.rl + ProfDef.GetLowestLevel
            Return True
        Else
            Return False
        End If
    End Function


End Class
