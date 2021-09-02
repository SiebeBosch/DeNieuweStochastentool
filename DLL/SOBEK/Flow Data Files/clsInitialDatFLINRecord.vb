Option Explicit On
Imports STOCHLIB.General

Public Class clsInitialDatFLINRecord

    Friend nm As String
    Friend ss As Integer
    Friend ID As String
    Friend ci As String
    Friend lc As Double
    Friend q_lq1 As Integer '0=constant initial q, 2=initial q as function of location
    Friend q_lq2 As Double 'constant initial discharge value
    Friend q_lq3 As Double '9.9999e+009
    Friend ty As Integer '1=water level, 0=water depth
    Friend lv_ll1 As Integer '0=initial constant level/depth, 2=as function of location
    Friend lv_ll2 As Double  'constant initial water level/depth
    Friend lv_ll3 As Double  '9.9999e+009

    Friend InUse As Boolean

    Friend record As String
    Private Setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Public Function Clone(newID As String) As clsInitialDatFLINRecord
        Dim newDat As New clsInitialDatFLINRecord(Me.Setup)
        newDat.nm = nm
        newDat.ss = ss
        newDat.ID = ID
        newDat.ci = ci
        newDat.lc = lc
        newDat.q_lq1 = q_lq1
        newDat.q_lq2 = q_lq2
        newDat.q_lq3 = q_lq3
        newDat.ty = ty
        newDat.lv_ll1 = lv_ll1
        newDat.lv_ll2 = lv_ll2
        newDat.lv_ll3 = lv_ll3
        newDat.build()
        Return newDat
    End Function

    Public Sub build()
        record = "FLIN id '" & ID & "' nm '" & nm & "' ci '" & ci & "' ty " & ty & " lv ll " & lv_ll1 & " " & lv_ll2 & " q_ lq 0 0 flin"
    End Sub

    Public Sub buildglobal()
        record = "GLIN fi 0 fr '(null)' FLIN id '-1' nm '" & nm & "' ci '-1' ty " & ty & " lv ll " & lv_ll1 & " " & lv_ll2 & " q_ lq 0 0 flin glin"
    End Sub

    Public Sub write(ByRef myWriter As System.IO.StreamWriter)
        Call build()
        myWriter.WriteLine(record)
    End Sub

    Public Sub writeAsGlobalRecord(ByRef mywriter As System.IO.StreamWriter)
        Call buildglobal()
        mywriter.WriteLine(record)
    End Sub

    Public Sub read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, tmp As String, tmpRecord As String
        Done = False

        While Not Done
            myStr = Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case Is = "nm"
                    nm = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ss"
                    ss = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "id"
                    ID = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If ID = "-1" Then
                        ci = "-1"
                        Done = True 'dit is het global record
                    End If
                Case Is = "ci"
                    ci = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ty"
                    ty = Val(Setup.GeneralFunctions.ParseString(myRecord, " "))
                Case Is = "lc"
                    lc = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "q_"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "lq" Then
                        tmpRecord = myRecord
                        tmp = Setup.GeneralFunctions.ParseString(tmpRecord, " ")
                        If IsNumeric(tmp) Then q_lq1 = tmp
                        tmp = Setup.GeneralFunctions.ParseString(tmpRecord, " ")
                        If IsNumeric(tmp) Then q_lq2 = tmp
                        tmp = Setup.GeneralFunctions.ParseString(tmpRecord, " ")
                        If IsNumeric(tmp) Then q_lq2 = tmp
                    End If
                Case Is = "lv"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "ll" Then
                        tmpRecord = myRecord
                        tmp = Setup.GeneralFunctions.ParseString(tmpRecord, " ")
                        If IsNumeric(tmp) Then lv_ll1 = tmp
                        tmp = Setup.GeneralFunctions.ParseString(tmpRecord, " ")
                        If IsNumeric(tmp) Then lv_ll2 = tmp
                        tmp = Setup.GeneralFunctions.ParseString(tmpRecord, " ")
                        If IsNumeric(tmp) Then lv_ll3 = tmp
                    End If
                Case Is = "FLIN"
          'start record
                Case Is = "flin"
                    Done = True
                Case Is = ""
                    Done = True
            End Select
        End While
        inuse = True

    End Sub


End Class
