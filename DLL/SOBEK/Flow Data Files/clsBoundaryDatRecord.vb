Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Friend Class clsBoundaryDatFLBORecord

    Friend ID As String
    Friend st As Integer '0 = no storage, 1 = storage
    Friend ty As Integer '0 = water level, 1 = discharge
    Friend h_wd As Integer '0 = constant, 1 = alternating, 4 = qh
    Friend h_wd0 As Double 'constant water level

    Friend h_wt As Integer '1 = internal time table, 11 = reference to a table library
    Friend h_wt1 As String 'internal time table
    Friend h_wt11 As String 'reference to table library

    Friend q_dw As Integer '0 = constant, 1 = alternating, 4 = QH
    Friend q_dw0 As Double 'constant discharge
    Friend q_dw1 As Double 'QH-table

    Friend q_dt As Integer
    Friend q_dt1 As Integer '1 = internal time table

    'siebe 15-5-2019: I removed the three points below since the SOBEK Tables already have their own PDIN definition.
    'Friend pdin1 As Double
    'Friend pdin2 As Double
    'Friend pdinPeriod As String

    Friend InUse As Boolean
    Friend QDTTable As clsSobekTable
    Friend HWTTable As clsSobekTable
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup

        'init classes
        QDTTable = New clsSobekTable(Me.setup)
        HWTTable = New clsSobekTable(Me.setup)
        InUse = True

    End Sub

    Friend Function Clone() As clsBoundaryDatFLBORecord
        Dim myRecord As New clsBoundaryDatFLBORecord(Me.setup)
        myRecord.h_wd = h_wd
        myRecord.h_wd0 = h_wd0
        myRecord.h_wt = h_wt
        myRecord.h_wt1 = h_wt1
        myRecord.h_wt11 = h_wt11
        myRecord.HWTTable = HWTTable.Clone
        myRecord.ID = ID
        myRecord.InUse = InUse
        myRecord.HWTTable.pdin1 = HWTTable.pdin1
        myRecord.HWTTable.pdin2 = HWTTable.pdin2
        myRecord.HWTTable.PDINPeriod = HWTTable.PDINPeriod
        myRecord.q_dt = q_dt
        myRecord.q_dt1 = q_dt1
        myRecord.q_dw = q_dw
        myRecord.q_dw0 = q_dw0
        myRecord.q_dw1 = q_dw1
        myRecord.QDTTable = QDTTable.Clone
        myRecord.st = st
        myRecord.ty = ty
        Return myRecord
    End Function

    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, tmp As String
        Dim CurTable As New clsSobekTable(Me.setup)
        Done = False
        InUse = True

        'een paar tijdelijke vlaggetjes ten behoeve van het inlezen
        Dim qdt1 As Boolean  'tijdstabel q
        Dim qdw1 As Boolean 'qh-verband
        Dim hwd1 As Boolean 'qh-verband
        Dim hwt1 As Boolean 'interne tijdstabel h
        Dim hwt11 As Boolean 'externe tijdstabel h

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord)
            Select Case myStr
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "st"
                    st = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "ty"
                    ty = Me.setup.GeneralFunctions.ParseString(myRecord)
                Case "q_"
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord)
                    If tmp = "dt" Then 'interne tijdstabel
                        tmp = Me.setup.GeneralFunctions.ParseString(myRecord)
                        If tmp = "1" Then
                            qdt1 = True

                            'continue parsing until table
                            tmp = Me.setup.GeneralFunctions.ParseString(myRecord)
                            tmp = Me.setup.GeneralFunctions.ParseString(myRecord)

                            'parse the table
                            QDTTable = Me.setup.GeneralFunctions.ParseSobekTable(myRecord)
                            CurTable = QDTTable
                        End If
                    ElseIf tmp = "dw" Then
                        q_dw = Me.setup.GeneralFunctions.ParseString(myRecord)
                        If q_dw = 0 Then
                            q_dw0 = Me.setup.GeneralFunctions.ParseString(myRecord)
                        ElseIf q_dw = 1 Or q_dw = 4 Then 'qh-verband
                            qdw1 = True
                        End If
                    End If
                Case "h_"
                    tmp = Me.setup.GeneralFunctions.ParseString(myRecord)
                    If tmp = "wd" Then
                        h_wd = Me.setup.GeneralFunctions.ParseString(myRecord)
                        If h_wd = 0 Then 'constant water level
                            h_wd0 = Me.setup.GeneralFunctions.ParseString(myRecord)
                            tmp = Me.setup.GeneralFunctions.ParseString(myRecord)
                        ElseIf h_wd = 1 Then  'time dependent water level
                            hwd1 = True
                        End If
                    ElseIf tmp = "wt" Then
                        h_wt = Me.setup.GeneralFunctions.ParseString(myRecord)
                        If h_wt = 1 Then
                            hwt1 = True 'interne tabel waterstanden

                            'continue parsing until table
                            tmp = Me.setup.GeneralFunctions.ParseString(myRecord)
                            tmp = Me.setup.GeneralFunctions.ParseString(myRecord)

                            'parse the table
                            HWTTable = Me.setup.GeneralFunctions.ParseSobekTable(myRecord)
                            CurTable = HWTTable

                        ElseIf h_wt = 11 Then
                            hwt11 = True 'externe tabel waterstanden
                            'Siebe: nog doen
                        End If

                    End If
                Case "PDIN"
                    CurTable.pdin1 = Me.setup.GeneralFunctions.ParseString(myRecord)
                    CurTable.pdin2 = Me.setup.GeneralFunctions.ParseString(myRecord)

                Case "TBLE"
                    If qdt1 Then
                        QDTTable = Me.setup.GeneralFunctions.ParseSobekTable(myRecord)
                    End If
                Case Else
                    'niks doen
            End Select
        End While
    End Sub

    Friend Sub Write(ByRef myWriter As StreamWriter)

        'voorbeeld tabel met waterhoogte
        'FLBO id 'cKGM026' st 0 ty 0 h_ wt 1 0 0 PDIN 0 1 '365;00:00:00' pdin
        'TBLE
        ''1999/11/30;00:00:00' 1 <
        ''2000/10/10;00:00:00' 2 <
        ''2001/10/10;00:00:00' 1 <
        ''2002/10/10;00:00:00' 2 <
        'tble flbo

        'voorbeeld constante waterhoogte
        'FLBO ID  'cGPGKST0921_147' st 0 ty 0 h_ wd 0  -0.92 0 flbo

        If InUse Then
            Dim i As Long

            If ty = 0 Then 'waterlevel

                If h_wt = 1 Then 'timetable
                    myWriter.WriteLine("FLBO id '" & ID & "' st " & st & " ty " & ty & " h_ wt 1 0 0 PDIN " & HWTTable.pdin1 & " " & HWTTable.pdin2 & " " & HWTTable.PDINPeriod & " pdin")
                    myWriter.WriteLine("TBLE")
                    For i = 0 To HWTTable.Dates.Count - 1
                        myWriter.WriteLine("'" & Format(HWTTable.Dates.Values(i), "yyyy/MM/dd;HH:mm:ss") & "' " & HWTTable.Values1.Values(i) & " <")
                    Next
                    myWriter.WriteLine("tble flbo")
                ElseIf h_wd = 0 Then 'constant
                    myWriter.WriteLine("FLBO id '" & ID & "' st " & st & " ty " & ty & " h_ wd 0 " & h_wd0 & " 0 flbo")
                End If

            ElseIf ty = 1 Then 'discharge

                If q_dt = 1 Then 'discharge as timetable
                    myWriter.WriteLine("FLBO id '" & ID & "' st " & st & " ty " & ty & " q_ dt 1 0 0 PDIN " & QDTTable.pdin1 & " " & QDTTable.pdin2 & " " & QDTTable.PDINPeriod & " pdin")
                    myWriter.WriteLine("TBLE")
                    QDTTable.writeTimeTableContentsToFile(myWriter, True, False, False, False, False, False, False, False)
                    myWriter.WriteLine("tble flbo")
                ElseIf q_dw = 0 Then 'constant discharge
                    myWriter.WriteLine("FLBO id '" & ID & "' st " & st & " ty " & ty & " q_ dw 0 " & q_dw0 & " 0 flbo")
                ElseIf q_dw = 1 Then 'QH-table
                    Me.setup.Log.AddWarning("Option q_ dw 1 (QH-table) not yet supported. Data was not written.")
                End If

            End If

        End If
    End Sub

End Class
