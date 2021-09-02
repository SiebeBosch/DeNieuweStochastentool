
Option Explicit On
Imports STOCHLIB.General

Public Class clsFrictionDatD2FRRecord

    'v2.000 introduced support for 2DGrid friction records
    Public ID As String
    Public ci As String
    Public mf As GeneralFunctions.enmFrictionType
    Public mtcp As Integer '0=const, 2 = path
    Public mtcpConst As Integer
    Public mtcpPath As String
    Public mwcp As Integer 'wall friction. 0=const,2=path
    Public mwcpConst As Integer
    Public mwcpPath As String
    Public record As String
    Friend InUse As Boolean

    Private Setup As clsSetup

    Public Sub New(ByVal mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Public Function Clone(newID As String) As clsFrictionDatD2FRRecord
        Dim newDat As New clsFrictionDatD2FRRecord(Me.Setup)
        newDat.ID = newID
        newDat.ci = newID
        newDat.mf = mf
        newDat.mtcp = mtcp 'can be constant or refer to a file
        newDat.mtcpPath = mtcpPath
        newDat.mwcpConst = mtcpConst
        newDat.mwcp = mwcp
        newDat.mwcpPath = mwcpPath
        newDat.mwcpConst = mwcpConst
        newDat.build()
        newDat.InUse = True
        Return newDat
    End Function

    Friend Sub build()
        record = "D2FR id '" & ID & "' ci '" & ci & "' mf " & mf & " mt cp " & mtcp & " "
        If mtcp = 2 Then
            record &= "'" & mtcpPath & "' "
        ElseIf mtcp = 0 Then
            record &= mtcpConst & " 0 "
        End If

        record &= "mw cp " & mwcp & " "
        If mwcp = 2 Then
            record &= " '" & mtcpPath & "' "
        ElseIf mwcp = 0 Then
            record &= mwcpConst & " 0 "
        End If
        record &= " d2fr"
    End Sub

    Friend Sub write(ByRef datWriter As System.IO.StreamWriter)
        Call build()
        datWriter.WriteLine(record)
    End Sub

    Public Sub read(ByRef Setup As clsSetup, ByVal myRecord As String)
        Dim Done As Boolean, myStr As String, tmp As String
        Done = False
        While Not Done
            myStr = Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case Is = "id"
                    ID = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ci"
                    ci = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "mf"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    Select Case tmp
                        Case Is = "0"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.CHEZY
                        Case Is = "1"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.MANNING
                        Case Is = "2"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.STRICKLERKN
                        Case Is = "3"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.STRICKLERKS
                        Case Is = "4"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.WHITECOLEBROOK
                        Case Is = "7"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.BOSBIJKERK
                    End Select
                Case Is = "mt"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "cp" Then
                        tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        mtcp = CType(tmp, Int16)
                        If tmp = "0" Then
                            mtcpConst = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        ElseIf tmp = "2" Then
                            mtcpPath = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        End If
                    End If
                Case Is = "mw"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "cp" Then
                        tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        mwcp = CType(tmp, Int16)
                        If tmp = "0" Then
                            mwcpConst = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        ElseIf tmp = "2" Then
                            mwcpPath = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        End If
                    End If
                Case Is = ""
                    Done = True
            End Select
        End While

    End Sub
End Class



