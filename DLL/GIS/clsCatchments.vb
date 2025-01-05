Imports STOCHLIB.General
Imports System.IO
Imports GemBox.Spreadsheet

Public Class clsCatchments
    Public Catchments As Dictionary(Of String, clsCatchment)
    Friend TotalArea As Double
    Public BuiFile As clsBuiFile

    Private SbkCase As clsSobekCase     'in case we'll need to look up SOBEK-areas based on inflow locations
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
        Catchments = New Dictionary(Of String, clsCatchment)
    End Sub

    Public Sub SelectAll()
        For Each myCatchment As clsCatchment In Catchments.Values
            myCatchment.InUse = True
        Next
    End Sub

    Public Function PointInsideCatchment(ByVal X As Double, Y As Double, Optional ByVal ActiveCatchmentsOnly As Boolean = True) As Boolean
        Dim myUtils As New MapWinGIS.Utils
        Dim myPoint As New MapWinGIS.Point
        Try
            myPoint.x = X
            myPoint.y = Y
            For Each myCatchment As clsCatchment In Catchments.Values
                If ActiveCatchmentsOnly = False OrElse myCatchment.InUse Then
                    If myUtils.PointInPolygon(myCatchment.TotalShape, myPoint) Then Return True
                End If
            Next
            Return False
        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try
    End Function



    Public Sub PopulateMeteoStations()
        BuiFile.MeteoStations = New clsMeteoStations(Me.setup)
        For Each myCatchment As clsCatchment In Catchments.Values
            For Each ms As clsMeteoStation In myCatchment.BuiFile.MeteoStations.MeteoStations.Values
                BuiFile.MeteoStations.MeteoStations.Add(ms.ID.Trim.ToUpper, ms)
            Next
        Next
    End Sub

    Public Sub SetSobekCase(ByRef myCase As clsSobekCase)
        SbkCase = myCase
        For Each myCatchment As clsCatchment In Catchments.Values
            myCatchment.setSobekCase(SbkCase)
        Next
    End Sub




    Friend Function getByID(ByVal myID As String) As clsCatchment
        If Not Catchments.ContainsKey(myID.Trim.ToUpper) Then
            Return Nothing
        Else
            Return Catchments.Item(myID.Trim.ToUpper)
        End If
    End Function

    Friend Function getAdd(ByVal myID As String) As clsCatchment
        Dim myCatchment As clsCatchment
        If Not Catchments.ContainsKey(myID.Trim.ToUpper) Then
            myCatchment = New clsCatchment(Me.setup)
            myCatchment.ID = myID
            Catchments.Add(myID.Trim.ToUpper, myCatchment)
            Return myCatchment
        Else
            Return Catchments.Item(myID.Trim.ToUpper)
        End If
    End Function



    Friend Function getCatchmentByXY(ByVal X As Double, ByVal Y As Double, Optional ByVal AcceptNearest As Boolean = False) As clsCatchment
        Dim Utils As New MapWinGIS.Utils
        Dim myPoint As New MapWinGIS.Point
        Dim newPoint As New MapWinGIS.Point
        Dim r As Long, i As Long, j As Long
        myPoint.x = X
        myPoint.y = Y

        For Each myCatchment As clsCatchment In Catchments.Values
            If Utils.PointInPolygon(myCatchment.TotalShape, myPoint) Then
                Return myCatchment
            End If
        Next

        If AcceptNearest Then
            'spiraal naar buiten to r >= 5000
            For i = 1 To 500 '500 rondjes
                r += 10
                For j = 0 To 350 Step 10
                    newPoint.x = myPoint.x + Math.Sin(Me.setup.GeneralFunctions.D2R(j)) * r
                    newPoint.y = myPoint.y + Math.Cos(Me.setup.GeneralFunctions.D2R(j)) * r
                    For Each myCatchment As clsCatchment In Catchments.Values
                        If Utils.PointInPolygon(myCatchment.TotalShape, newPoint) Then
                            Return myCatchment
                        End If
                    Next

                Next
            Next
        End If

        Return Nothing

    End Function

    Public Function calcMeteoStations() As Integer
        Dim n As Integer = 0
        For Each myCatchment In Catchments.Values
            For Each myStation In myCatchment.BuiFile.MeteoStations.MeteoStations.Values
                n += 1
            Next
        Next
        Return n
    End Function

    Public Sub PopulateAndWriteBuiFile(ByVal path As String, Optional ByVal nDigits As Integer = 2)
        '--------------------------------------------------------------------------------------------------------------
        'date: 28 june 2013
        'author: Siebe Bosch
        'description: populates the .bui-file with discharge results for each underlying catchment
        'prerequisites: the .bui file must already contain meteo stations
        'note: since discharges per catchment are stored in m3/s, we need to convert tot mm/ts
        '--------------------------------------------------------------------------------------------------------------
        Dim ms As clsMeteoStation, msIdx As Integer, ts As Integer, myVal As Double
        Dim myRecord As clsTimeTableRecord

        'start with the first meteostation to determine the timestep etc
        ms = BuiFile.MeteoStations.MeteoStations.Values(0)

        Dim i As Integer, myStr As String, nTim As Long
        Dim dagen As Integer, uren As Integer
        Dim Catchment As clsCatchment

        'determine the number formatting for the .bui file (e.g. "0.0000")
        Dim Formatting As String = "#.0"
        If nDigits > 1 Then
            For i = 2 To nDigits
                Formatting &= "#"
            Next
        End If

        'zoek de informatie over tijdstappen op
        'LET OP: we slaan de eerste tijdstap over want is altijd nul. Bovendien moeten we de laterals een tijdstap terugschuiven t.b.v. de bui
        'Edit Siebe juli 2016: die hele zooi verwijderd ivm overstap naar array ipv collection. Bovenstaande nog fixen!

        Using buiWriter As New StreamWriter(path, False)
            'doorloop alle areas en schrijf de 'bui' weg
            buiWriter.WriteLine("*Name of this file: " & path)
            buiWriter.WriteLine("*Date and time of construction: " & Now & ".")
            buiWriter.WriteLine("1")
            buiWriter.WriteLine("*Aantal stations")
            buiWriter.WriteLine(BuiFile.MeteoStations.MeteoStations.Count)
            buiWriter.WriteLine("*Namen van stations")
            For Each myStation In BuiFile.MeteoStations.MeteoStations.Values
                buiWriter.WriteLine(Chr(39) & myStation.ID & Chr(39)) 'de stations
            Next
            buiWriter.WriteLine("*Aantal gebeurtenissen (omdat het 1 bui betreft is dit altijd 1)")
            buiWriter.WriteLine("*en het aantal seconden per waarnemingstijdstap")
            buiWriter.WriteLine(" 1  " & BuiFile.TimeStep.TotalSeconds)
            buiWriter.WriteLine("*Elke commentaarregel wordt begonnen met een * (asterisk).")
            buiWriter.WriteLine("*Eerste record bevat startdatum en -tijd, lengte van de gebeurtenis in dd hh mm ss")
            buiWriter.WriteLine("*Het format is: yyyymmdd:hhmmss:ddhhmmss")
            buiWriter.WriteLine("*Daarna voor elk station de neerslag in mm per tijdstap.")

            dagen = Me.setup.GeneralFunctions.RoundUD(BuiFile.TotalSpan.TotalDays, 0, False)
            uren = BuiFile.TotalSpan.Subtract(New TimeSpan(dagen, 0, 0, 0)).TotalHours

            'schrijf de instellingen voor datum/tijd en tijdstap naar de buifile. Zet het begin op de daadwerkelijke start van de resultaten
            'en vul de tijdstappen met resultaten van een tijdstap verder
            buiWriter.WriteLine(" " & BuiFile.StartDate.Year & " " & BuiFile.StartDate.Month & " " & BuiFile.StartDate.Day & " " & BuiFile.StartDate.Hour & " " & BuiFile.StartDate.Minute & " 0 " & dagen & " " & uren & " 0 0 ")

            'write the meteorological data
            nTim = BuiFile.GetnRecords - 2  'weet nog niet waarom maar buifile werd stelselmatig twee regels te lang
            Me.setup.GeneralFunctions.UpdateProgressBar("Writing .bui file.", 0, 10)

            'now write the data
            For iTim = 0 To nTim
                myStr = ""
                Me.setup.GeneralFunctions.UpdateProgressBar("", iTim, nTim)
                For msIdx = 0 To BuiFile.MeteoStations.MeteoStations.Values.Count - 1
                    ms = BuiFile.MeteoStations.MeteoStations.Values(msIdx)          'get the meteo station
                    Catchment = Catchments.Values(ms.CatchmentIdx)                  'get the corresponding catchment
                    myRecord = Catchment.QTable.Records.Values(iTim)                'get the discharge record for this timestep
                    If ms.RepresentsCSO Then                                        'spilling discharge in m3/s
                        myVal = myRecord.GetValue(1)                                  'convert to mm/ts
                        myVal = ts * Me.setup.GeneralFunctions.m3ps2mmps(myVal, Catchment.ModelAreas.RRPavedCSO)
                    Else
                        'IMPORTANT: in this case do not convert volumes based on specified surface area that WagMod has been calibrated on
                        'since we already have the total m3/s for the catchment. 
                        'Now we'll have to start writing the results based on the actual areas! Therefore GISArea - RRCSOLocationsArea
                        myVal = myRecord.GetValue(0)                                  'drainage flux in m3/s
                        myVal = ms.ConstantFactor * ts * Me.setup.GeneralFunctions.m3ps2mmps(myVal, Catchment.TotalShape.Area - Catchment.ModelAreas.RRPavedCSO) 'convert to mm/ts
                    End If
                    myStr &= (" " & Format(myVal, Formatting))
                Next
                buiWriter.WriteLine(myStr)

            Next
        End Using

    End Sub

    Public Function collectSbkRRNodes(ByVal Unpaved As Boolean, ByVal Paved As Boolean) As Boolean
        Dim myCatchment As clsCatchment
        Dim UpNode As clsRRUnpavedNode, PvNode As clsRRPavedNode
        Dim Up3B As clsUnpaved3BRecord, Pv3B As clsPaved3BRecord

        Try
            For Each myNode As clsRRNodeTPRecord In SbkCase.RRTopo.Nodes.Values
                myCatchment = getCatchmentByXY(myNode.X, myNode.Y)
                If Not myCatchment Is Nothing Then
                    If Unpaved AndAlso myNode.nt.ID = "3B_UNPAVED" Then
                        UpNode = New clsRRUnpavedNode(Me.setup)
                        UpNode.ID = myNode.ID
                        Up3B = SbkCase.RRData.Unpaved3B.GetRecord(myNode.ID)
                        UpNode.NodeTpRecord = myNode
                        UpNode.UnPaved3BRecord = Up3B
                        myCatchment.RRUnpavedNodes.Add(UpNode)
                    ElseIf Paved AndAlso myNode.nt.ID = "3B_PAVED" Then
                        PvNode = New clsRRPavedNode(Me.setup)
                        PvNode.ID = myNode.ID
                        Pv3B = SbkCase.RRData.Paved3B.GetRecord(myNode.ID)
                        PvNode.NodeTpRecord = myNode
                        PvNode.Paved3BRecord = Pv3B
                        myCatchment.RRPavedNodes.Add(PvNode)
                    End If
                End If
            Next
            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error in sub collectSbkRRNodes of class clsCatchments.")
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function


    Public Sub collectSbkInflowNodes(ByVal RRCFConnections As Boolean, ByVal RRCFConnNodes As Boolean, ByVal Laterals As Boolean, Optional ByVal Prefix As String = "", Optional ByVal CSOCollection As Boolean = False)
        Dim myCatchment As clsCatchment, myInflowLocation As clsRRInflowLocation
        Dim i As Long

        If RRCFConnections Then
            'doorloop alle RRCF-connections
            i = 0

            If CSOCollection Then
                Me.setup.GeneralFunctions.UpdateProgressBar("Collecting SOBEK CSO locations from RR on Flow Connections.", 0, 10)
            Else
                Me.setup.GeneralFunctions.UpdateProgressBar("Collecting SOBEK Inflow points from RR on Flow Connections.", 0, 10)
            End If

            For Each myReach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, SbkCase.CFTopo.Reaches.Reaches.Count)
                For Each myReachObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
                    If myReachObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnection Then
                        If Left(myReachObj.ID, Prefix.Length) = Prefix Then

                            myReachObj.calcXY()
                            myCatchment = getCatchmentByXY(myReachObj.X, myReachObj.Y)
                            If Not myCatchment Is Nothing Then
                                myInflowLocation = New clsRRInflowLocation(Me.setup)
                                myInflowLocation.SetSobekCase(SbkCase)
                                myInflowLocation.NodeID = myReachObj.ID
                                myInflowLocation.ID = Right(myReachObj.ID, myReachObj.ID.Length - Prefix.Length)
                                myInflowLocation.X = myReachObj.X
                                myInflowLocation.Y = myReachObj.Y
                                myInflowLocation.ReachObject = myReachObj

                                'determine whether this node should be added to the regular inflow nodes collection or the 
                                'collection of CSO-locations
                                If CSOCollection Then
                                    myCatchment.RRCSOLocations.RRInflowLocations.Add(myInflowLocation.ID.Trim.ToUpper, myInflowLocation)
                                Else
                                    myCatchment.RRInflowLocations.RRInflowLocations.Add(myInflowLocation.ID.Trim.ToUpper, myInflowLocation)
                                End If

                            End If
                        End If
                    End If
                Next
            Next
        End If

        If RRCFConnNodes Then
            'doorloop alle RRCF-connection nodes
            i = 0

            If CSOCollection Then
                Me.setup.GeneralFunctions.UpdateProgressBar("Collecting SOBEK CSO locations from RR on Flow Connection Nodes.", 0, 10)
            Else
                Me.setup.GeneralFunctions.UpdateProgressBar("Collecting SOBEK Inflow points from RR on Flow Connection Nodes.", 0, 10)
            End If

            For Each myNode As clsSbkReachNode In SbkCase.CFTopo.ReachNodes.ReachNodes.Values
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, SbkCase.CFTopo.ReachNodes.ReachNodes.Count)
                If myNode.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeRRCFConnectionNode Then
                    If Left(myNode.ID, Prefix.Length) = Prefix Then

                        myCatchment = getCatchmentByXY(myNode.X, myNode.Y)
                        If Not myCatchment Is Nothing Then
                            myInflowLocation = New clsRRInflowLocation(Me.setup)
                            myInflowLocation.SetSobekCase(SbkCase)
                            myInflowLocation.NodeID = myNode.ID
                            myInflowLocation.ID = Right(myNode.ID, myNode.ID.Length - Prefix.Length)
                            myInflowLocation.X = myNode.X
                            myInflowLocation.Y = myNode.Y
                            myInflowLocation.ReachNode = myNode

                            'determine whether this node should be added to the regular inflow nodes collection or the 
                            'collection of CSO-locations
                            If CSOCollection Then
                                myCatchment.RRCSOLocations.RRInflowLocations.Add(myInflowLocation.ID.Trim.ToUpper, myInflowLocation)
                            Else
                                myCatchment.RRInflowLocations.RRInflowLocations.Add(myInflowLocation.ID.Trim.ToUpper, myInflowLocation)
                            End If

                        End If
                    End If
                End If
            Next
        End If

        If Laterals Then
            'doorloop alle lateral nodes
            i = 0

            If CSOCollection Then
                Me.setup.GeneralFunctions.UpdateProgressBar("Collecting SOBEK CSO locations from Lateral Nodes.", 0, 10)
            Else
                Me.setup.GeneralFunctions.UpdateProgressBar("Collecting SOBEK Inflow points from Lateral Nodes.", 0, 10)
            End If

            For Each myreach As clsSbkReach In SbkCase.CFTopo.Reaches.Reaches.Values
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, SbkCase.CFTopo.Reaches.Reaches.Count)
                For Each myReachObj As clsSbkReachObject In myreach.ReachObjects.ReachObjects.Values
                    If myReachObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFLateral Then
                        If Left(myReachObj.ID, Prefix.Length) = Prefix Then

                            myReachObj.calcXY()
                            myCatchment = getCatchmentByXY(myReachObj.X, myReachObj.Y)
                            If Not myCatchment Is Nothing Then
                                myInflowLocation = New clsRRInflowLocation(Me.setup)
                                myInflowLocation.SetSobekCase(SbkCase)
                                myInflowLocation.NodeID = myReachObj.ID
                                myInflowLocation.ID = Right(myReachObj.ID, myReachObj.ID.Length - Prefix.Length)
                                myInflowLocation.X = myReachObj.X
                                myInflowLocation.Y = myReachObj.Y
                                myInflowLocation.ReachObject = myReachObj

                                'determine whether this node should be added to the regular inflow nodes collection or the 
                                'collection of CSO-locations
                                If CSOCollection Then
                                    myCatchment.RRCSOLocations.RRInflowLocations.Add(myInflowLocation.ID.Trim.ToUpper, myInflowLocation)
                                Else
                                    myCatchment.RRInflowLocations.RRInflowLocations.Add(myInflowLocation.ID.Trim.ToUpper, myInflowLocation)
                                End If

                            End If
                        End If
                    End If
                Next
            Next
        End If

    End Sub

End Class
