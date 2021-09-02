Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsControlDefRecord

    Friend ID As String
    Friend nm As String
    Friend bl As GeneralFunctions.enmInterpolationType '0 = block, 1 = linear
    Friend cb As String  'control branche ID (not in rural, I believe)
    Friend cl As Double  'distance on control branch id (not in rural, I believe)
    Friend ml As String  'measurement location (ID Measurement station)
    Friend u0 As Double
    Friend ui As Double
    Friend ua As Double
    Friend cn As GeneralFunctions.enmSetpointType  'interval type 0=fixed, 1=variable
    Friend mc As Double   'max change dValue/dt
    Friend du As Double
    Friend dt As Integer   'dead band type
    Friend d_ As Double    'dead band step size
    Friend if_ As Double   'ki
    Friend pf As Double    'kp
    Friend df As Double    'kd
    Friend va As Double    'max change
    Friend cv As Double    'control velocity
    Friend ct As GeneralFunctions.enmSobekControllerType   'controller type 0=time, 1 = hydraulic 2 = interval 3 = PID, 4 = relative time, 99 = follow target level (eigen bedenksel)
    Friend ac As Integer   '0 = inactive 1= active
    Friend cp As GeneralFunctions.enmObservedParameter   'measured parametr: 0 = water level, 1 = discharge
    Friend ca As GeneralFunctions.enmControlledParameter   'controlled parameter 0 = crest level, 1 = crest width, 2 = gate height, 3 = pumpcap
    Friend cf As Integer   'update frequency
    Friend sptc As GeneralFunctions.enmSetpointType 'constant or variable target level 0=const 1 =variable
    Friend hcht As Integer 'controltable 1 = table
    Friend SetPointType As String       'no setpoint, constant, summer/winter, time table
    Friend SetPointValue As Double      'eigen dataformat
    Friend SetPointTableID As String    'eigen dataformat
    Friend ControlTableID As String     'eigen dataformaat
    Friend setpointsummer As Double     'zelf toegevoegd om een automatisch streefpeilvolgcontroller te maken
    Friend setpointwinter As Double     'zelf toegevoegd om een automatisch streefpeilvolgcontroller te maken

    'om bij te houden welke tabel we moeten importeren
    Friend titv As Boolean = False

    'friend TimeTable As New clsSobekTable
    Friend ControlTable As New clsSobekTable(Me.setup)
    Friend TimeTable As New clsSobekTable(Me.setup)
    Friend InUse As Boolean

    Private setup As clsSetup

    Public Function GetFirstSetpointValue() As Double
        Select Case ct
            Case Is = 0 'timecontroller
                Return TimeTable.Values1.Values(0)
            Case Is = 1 'hydraulic
                Return ControlTable.Values1.Values(0)
            Case Is = 2, 3 'interval or PID
                If sptc = 0 Then
                    Return SetPointValue
                ElseIf sptc = 1 Then
                    Return TimeTable.Values1.Values(0)
                End If
        End Select
    End Function


    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup

        'Init classes:
        ControlTable = New clsSobekTable(Me.setup)
        TimeTable = New clsSobekTable(Me.setup)

    End Sub
    'de andere controllers hebben geen verdere input nodig
    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, Pos1 As Integer, Pos2 As Integer, Table As String, tmp As String
        Done = False

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord)
            Select Case myStr
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "nm"
                    nm = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "bl"
                    bl = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "mc"
                    mc = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "cb"
                    cb = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "cl"
                    cl = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "ml"
                    ml = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "dt"
                    dt = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "d_"
                    d_ = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "ct"
                    ct = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "du"
                    du = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "cp"
                    cp = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "cn"
                    cn = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "ac"
                    ac = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "u0"
                    u0 = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "ui"
                    ui = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "ua"
                    ua = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "pf"
                    pf = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "if"
                    if_ = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "df"
                    df = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "va"
                    va = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "cv"
                    cv = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "ca"
                    ca = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "cf"
                    cf = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "hc"
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord)
                    If tmp = "ht" Then
                        hcht = Me.setup.GeneralFunctions.ParseString(myRecord)
                        If hcht = 1 Then
                            Pos1 = InStr(1, myRecord, "tble")
                            Table = Left(myRecord, Pos1 + 3)
                            myRecord = Right(myRecord, Len(myRecord) - Pos1 - 3)
                            ControlTable.ID = ID
                            ControlTable.Read(Table)
                        End If
                    End If
                Case Is = "ti"
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord)
                    If tmp = "tv" Then
                        titv = True
                    End If
                Case Is = "sp"
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord)
                    If tmp = "tc" Then
                        sptc = Me.setup.GeneralFunctions.ParseString(myRecord)
                        If sptc = 0 Then                'constant setpoint
                            SetPointValue = Me.setup.GeneralFunctions.ParseString(myRecord)
                        ElseIf sptc = 1 Then            'variable setpoint
                            Pos2 = InStr(1, myRecord, "tble")
                            Table = Left(myRecord, Pos2 - 1)
                            Pos1 = InStr(1, Table, "TBLE")
                            Table = Right(Table, Table.Length - Pos1 - 3)
                            myRecord = Right(myRecord, Len(myRecord) - Pos2 - 3)
                            SetPointTableID = ID          'zit in sobek intern in de file zelf. wij maken er nu even een aparte tabel met bij behorende id van
                            TimeTable.ID = ID
                            TimeTable.Read(Table)
                            'CFData.TimeTables.Add Item:=myTable 'Siebe 20110714 vroeger lazen we hem meteen in in timetables, maar niet alle controllers worden gebruikt, dus doen we dit nu in de aanroepende routine
                        End If
                    End If
                Case Is = "PDIN"
                    TimeTable.ID = ID
                    TimeTable.pdin1 = Me.setup.GeneralFunctions.ParseString(myRecord)
                    TimeTable.pdin2 = Me.setup.GeneralFunctions.ParseString(myRecord)
                    If TimeTable.pdin2 = 1 Then
                        TimeTable.PDINPeriod = Me.setup.GeneralFunctions.ParseString(myRecord)
                    End If
                Case Is = "pdin"
                Case Is = "TBLE"
                    Pos1 = InStr(1, myRecord, "tble")
                    Table = Left(myRecord, Pos1 + 3)
                    myRecord = Right(myRecord, Len(myRecord) - Pos1 - 3)
                    SetPointTableID = ID          'zit in sobek intern in de file zelf. wij maken er nu even een aparte tabel met bij behorende id van
                    TimeTable.ID = ID
                    TimeTable.Read(Table)
                Case Is = "CNTL"
                Case Is = "cntl"
                Case Else
                    If Not myStr = "" AndAlso Not IsNumeric(myStr) Then
                        Me.setup.Log.AddWarning("Unsupported token found in control.def file: " & myStr)
                    End If
            End Select
        End While

    End Sub

    Friend Sub setAsTargetLevelController(ByVal myID As String, ByVal WP As Double, ByVal ZP As Double)

        ID = myID             'controller id
        nm = myID
        ct = 0                'controller type target level
        ac = 1                'controlled active
        ca = 0                '0 = crest level
        cf = 1                'update frequency
        mc = 0                'max change per second. 0 = instantaneous
        bl = 1                'block interpolation
        Call TimeTable.BuildFromTargetLevels(WP, ZP)

    End Sub


    Friend Sub Write(ByRef myWriter As StreamWriter)
        Dim i As Long, n As Long, tmpStr As String
        Select Case ct

            Case Is = 0 'time controller
                myWriter.WriteLine("CNTL id '" & ID & "' nm '" & nm & "' ct " & ct & " ac " & ac & " ca " & ca & " cf " & cf & " mc " & mc & " bl " & bl & " ti tv PDIN " & TimeTable.pdin1 & " " & TimeTable.pdin2 & " '" & TimeTable.PDINPeriod & "' pdin")
                myWriter.WriteLine("TBLE")
                n = TimeTable.Dates.Count
                Me.setup.GeneralFunctions.UpdateProgressBar("Writing timetable " & ID & " to control.def record.", 0, 10, True)
                'v1.797: speeding up the writing process of long tables by formatting the date string in one go
                For i = 0 To n - 1
                    Me.setup.GeneralFunctions.UpdateProgressBar("", i, n)
                    myWriter.WriteLine("'" & Format(TimeTable.Dates.Values(i), "yyyy/MM/dd;HH:mm:ss") & "' " & TimeTable.Values1.Values(i) & " <")
                Next
                myWriter.WriteLine("tble cntl")
                Me.setup.GeneralFunctions.UpdateProgressBar("control.def record for " & ID & " written.", 0, 10, True)

            Case Is = 1 'hydraulic controller
                tmpStr = "CNTL id '" & ID & "' nm '" & nm & "' ct " & ct & " ac " & ac & " ca " & ca & " cf " & cf & " ml '" & ml & "' cp " & cp & " bl " & bl & " hc ht " & hcht
                If hcht = 0 Then 'constant
                    tmpStr = tmpStr & " cntl"
                    myWriter.WriteLine(tmpStr)
                ElseIf hcht = 1 Then
                    myWriter.WriteLine(tmpStr)
                    myWriter.WriteLine("TBLE")
                    For i = 0 To ControlTable.XValues.Count - 1
                        myWriter.WriteLine(ControlTable.XValues.Values(i) & " " & ControlTable.Values1.Values(i) & " <")
                    Next
                    myWriter.WriteLine("tble cntl")
                End If

            Case Is = 2 'interval controller
                tmpStr = "CNTL id '" & ID & "' nm '" & nm & "' ct " & ct & " ac " & ac & " ca " & ca & " cf " & cf & " ml '" & ml & "' cp " & cp & " ui " & ui & " ua " & ua & " cn " & cn & " du " & du & " cv " & cv & " dt " & dt & " d_ " & d_ & " bl " & bl & " sp tc " & sptc
                If sptc = 0 Then 'constant
                    tmpStr = tmpStr & " " & SetPointValue & " 0 cntl"
                    myWriter.WriteLine(tmpStr)
                ElseIf sptc = 1 Then 'variabel
                    myWriter.WriteLine(tmpStr)
                    myWriter.WriteLine("TBLE")
                    For i = 0 To TimeTable.Dates.Count - 1
                        myWriter.WriteLine("'" & Year(TimeTable.Dates.Values(i)) & "/" & Format(Month(TimeTable.Dates.Values(i)), "00") & "/" & Format(Day(TimeTable.Dates.Values(i)), "00") & ";" & Format(Hour(TimeTable.Dates.Values(i)), "00") & ":" & Format(Minute(TimeTable.Dates.Values(i)), "00") & ":" & Format(Second(TimeTable.Dates.Values(i)), "00") & "' " & TimeTable.Values1.Values(i) & " <")
                    Next
                    myWriter.WriteLine("tble cntl")
                End If

            Case Is = 3 'PID controller
                tmpStr = "CNTL id '" & ID & "' nm '" & nm & "' ct " & ct & " ac " & ac & " ca " & ca & " cf " & cf & " ml '" & ml & "' cp " & cp & " ui " & ui & " ua " & ua & " u0 " & u0 & " pf " & pf & " if " & if_ & " df " & df & " va " & va & " bl " & bl & " sp tc " & sptc
                If sptc = 0 Then 'constant
                    tmpStr = tmpStr & " " & SetPointValue & " 0 cntl"
                    myWriter.WriteLine(tmpStr)
                ElseIf sptc = 1 Then 'variabel
                    myWriter.WriteLine(tmpStr)
                    myWriter.WriteLine("TBLE")
                    For i = 0 To TimeTable.Dates.Count - 1
                        myWriter.WriteLine("'" & Year(TimeTable.Dates.Values(i)) & "/" & Format(Month(TimeTable.Dates.Values(i)), "00") & "/" & Format(Day(TimeTable.Dates.Values(i)), "00") & ";" & Format(Hour(TimeTable.Dates.Values(i)), "00") & ":" & Format(Minute(TimeTable.Dates.Values(i)), "00") & ":" & Format(Second(TimeTable.Dates.Values(i)), "00") & "' " & TimeTable.Values1.Values(i) & " <")
                    Next
                    myWriter.WriteLine("tble cntl")
                End If

        End Select
    End Sub
End Class
