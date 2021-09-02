Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsLateralDatFLNORecord

    Friend ID As String
    Friend nm As String
    Friend ar As Double  'area rational method
    Friend ii As Double  'seepage/infiltration intensity
    Friend ms As String  'meteo station
    Friend ir As Double  'constant rainfall intensity
    Friend sc As Double  'voor 2D morflogie: 0=left (=main channel, = default), 1=right
    Friend lt As Double  'length of discharge: 0=point
    Friend dclt1 As Double 'dc lt 0 = constant, dc lt 1 = table time/discharge, dc lt 2 = table h Q, dc lt 6 = constant intensity, dc lt 7 = from meteo station
    Friend dclt2 As Double
    Friend dclt3 As Double

    Friend pdin1 As Integer    '0=continuous interpolation, 1=block
    Friend pdin2 As Integer '0=no periodicity, 1=periodicity
    Friend pdin3 As String 'periodicity

    Friend nLevelAreaRecords As Integer
    Friend Table As clsSobekTable       'hoogte-oppervlaktetabel

    'Friend TimeTableID As String            'bij het inlezen vanaf het werkblad stoppen we de tabel niet in de onderstaande structuur maar verwijzen we naaar cfdat.timetables
    Friend dcltTimeTable As clsSobekTable       'discharge as a function of time (when dc lt 1)

    Friend InUse As Boolean

    Private setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByVal myCase As ClsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase
        InUse = True

        'Init classes:
        Table = New clsSobekTable(Me.setup)
        dcltTimeTable = New clsSobekTable(Me.setup)
    End Sub
    Friend Sub Read(ByVal myRecord As String)
        Dim myStr As String, tmp As String
        Dim Pos As Integer, Table As String

        Try
            While Not myRecord = ""
                myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Select Case myStr
                    Case Is = "id"
                        ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "ar"
                        ar = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "ii"
                        ii = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "ms"
                        ms = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "ir"
                        ir = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "sc"
                        sc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "lt"
                        lt = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "dc"
                        tmp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        If tmp = "lt" Then
                            dclt1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                            If dclt1 = 0 Then dclt2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                            '          dclt2 = Me.setup.GeneralFunctions.ParseString(myrecord, " ")
                            '          dclt3 = Me.setup.GeneralFunctions.ParseString(myrecord, " ")
                        End If
                    Case Is = "PDIN"
                        pdin1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        pdin2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                        If pdin2 = 1 Then pdin3 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case Is = "TBLE"
                        Pos = InStr(1, myRecord, "tble")
                        Table = Left(myRecord, Pos - 1)
                        myRecord = Right(myRecord, Len(myRecord) - Pos - 3)
                        If dclt1 = 1 Then Call dcltTimeTable.Read(Table) 'discharge as a function of time
                    Case "flno"
                    Case "FLNO"
                    Case "pdin"
                    Case Else
                        If Not IsNumeric(myStr) Then Me.setup.Log.AddWarning("Unsupported token " & myStr & " found in lateral.dat")
                End Select
            End While
        Catch ex As Exception
            Me.setup.Log.AddError("Error in sub Read of class clsLateralDatFLNORecord.")
            Me.setup.Log.AddError(ex.Message)
        End Try



    End Sub

    Friend Sub Write(ByVal datWriter As StreamWriter)

        Try
            Dim i As Long

            If ar > 0 Then  'rational method
                If ms <> "" Then
                    datWriter.WriteLine("FLNO id '" & ID & "' sc 0 lt 0 dc lt 7 ir " & Replace(ir / 24 / 3600 & " ms '" & ms & "' ii " & ii / 24 / 3600 & " ar " & Math.Round(ar, 5) & " flno", ",", "."))
                Else
                    datWriter.WriteLine("FLNO id '" & ID & "' sc 0 lt 0 dc lt 6 ir " & Replace(ir / 24 / 3600 & " ms '" & ms & "' ii " & ii / 24 / 3600 & " ar " & Math.Round(ar, 5) & " flno", ",", "."))
                End If

            ElseIf dclt1 = 1 Then
                If Not dcltTimeTable Is Nothing Then                                'time table

                    datWriter.WriteLine("FLNO id '" & ID & "' sc 0 lt 0 dc lt 1 0 0 PDIN " & dcltTimeTable.pdin1 & " 0 " & dcltTimeTable.PDINPeriod & " pdin")
                    datWriter.WriteLine("TBLE")
                    If dcltTimeTable.DateValStrings.Count > 0 Then                    'mooi zo, ze zijn al beschikbaar als strings voor sobek
                        For i = 0 To dcltTimeTable.DateValStrings.Count - 1
                            datWriter.WriteLine(dcltTimeTable.DateValStrings.Values(i))
                        Next
                    Else
                        For i = 0 To dcltTimeTable.Dates.Count - 1
                            datWriter.WriteLine("'" & Format(dcltTimeTable.Dates.Values(i), "yyyy/MM/dd;HH:mm:ss") & "' " & dcltTimeTable.Values1.Values(i) & " <")
                        Next
                    End If
                    datWriter.WriteLine("tble flno")
                Else
                    Me.setup.Log.AddWarning("Could not find timetable for lateral node " & ID)
                End If
            ElseIf dclt1 = 0 Then
                datWriter.WriteLine("FLNO id '" & ID & "' sc 0 lt 0 dc lt 0 " & dclt2 & " 0 flno")
            End If

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error writing lateral.dat FLNO record for node " & ID)
        End Try


    End Sub
End Class
