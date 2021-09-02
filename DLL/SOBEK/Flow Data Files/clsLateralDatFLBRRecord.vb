Option Explicit On
Imports System.IO
Imports STOCHLIB.General
Imports System.Globalization

Public Class clsLateralDatFLBRRecord
    Friend ID As String
    Friend sc As Integer = 0
    Friend lt As Integer = 0
    Friend dclt1 As GeneralFunctions.enmlateraltype
    Friend dclt2 As Integer
    Friend dclt3 As Integer
    Friend ir As Double 'constant intensity
    Friend ms As String 'meteo station
    Friend ii As String 'seepage/inf
    Friend ar As Double 'area
    Friend pdin1 As Integer
    Friend pdin2 As Integer
    Friend pdin3 As Integer
    Friend LibTableID As String
    Friend TimeTable As clsSobekTable
    Friend InUse As Boolean

    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase
        InUse = True

        'init classes
        TimeTable = New clsSobekTable(Me.setup)

    End Sub
    Friend Sub Write(ByVal datWriter As StreamWriter)
        Dim i As Long, Val As Double
        Select Case dclt1
            Case Is = 0
                datWriter.WriteLine("FLBR id '" & ID & "' sc 0 lt 0 dc lt 0 " & dclt2 & " " & dclt3 & " flbr")
            Case Is = 1
                datWriter.WriteLine("FLBR id '" & ID & "' sc 0 lt 0 dc lt 1 0 0 PDIN 0 0 pdin")
                datWriter.WriteLine("TBLE")
                For i = 0 To TimeTable.Dates.Count - 1
                    Val = TimeTable.Values1.Values(i)
                    If Val < 0.1 Then
                        'format in scientific notation with seven digits
                        datWriter.WriteLine("'" & Format(TimeTable.Dates.Values(i), "yyyy/MM/dd;HH:mm:ss") & "' " & TimeTable.Values1.Values(i).ToString("E6", CultureInfo.InvariantCulture) & " <")
                    Else
                        'regular number formatting
                        datWriter.WriteLine("'" & Format(TimeTable.Dates.Values(i), "yyyy/MM/dd;HH:mm:ss") & "' " & Format(TimeTable.Values1.Values(i), ".0######") & " <")
                    End If
                Next
                datWriter.WriteLine("tble flbr")
            Case Is = 6
                datWriter.WriteLine("FLBR id '" & ID & "' sc 0 lt 0 dc lt 6 ir " & ir & " ms '" & ms & "' ii " & ii & " ar " & ar & " flbr")
            Case Is = 7
                datWriter.WriteLine("FLBR id '" & ID & "' sc 0 lt 0 dc lt 7 ir " & ir & " ms '" & ms & "' ii 0 ar " & ar & " flbr")
            Case Is = 11
                datWriter.WriteLine("FLBR id '" & ID & "' sc 0 lt 0 dc lt 11 '" & LibTableID & "' flbr")
        End Select
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim myStr As String, tmp As String
        Dim Pos As Integer, Table As String

        Try

        Catch ex As Exception
            Me.setup.Log.AddError(ex.Message)
            Me.setup.Log.AddError("Error in sub read of class clsLateralDatFLBRRecord.")
        End Try

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
                    Call TimeTable.Read(Table)
                    TimeTable.ID = ID
                    SbkCase.CFData.Data.TimeTables.AddTable(TimeTable)
                Case "flno"
                Case "FLNO"
                Case "pdin"
                Case Else
                    If Not IsNumeric(myStr) Then
                        Me.setup.Log.AddWarning("Unsupported token " & myStr & " found in lateral.dat")
                    End If
            End Select
        End While
    End Sub

End Class
