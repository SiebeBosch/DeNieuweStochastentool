Option Explicit On

Imports STOCHLIB.General

Public Class clsFrictionDatSTFRRecord

    Public ID As String 'ID
    Public ci As String 'structure id
    Public mf As GeneralFunctions.enmFrictionType 'friction type
    Public mtcp As Integer '0=constant, 1=table
    Public mtcpConst As Double
    Public mtcp3 As Double
    Public mrcp As Integer '0=constant, 1=table
    Public mrcpConst As Double
    Public mrcp3 As Double
    Public s1 As Integer
    Public s2 As Integer
    Public sf As GeneralFunctions.enmFrictionType
    Public stcp As Integer '0=constant, 1=table
    Public stcpConst As Double
    Public stcp3 As Integer
    Public srcp As Integer '0=constant, 1=table
    Public srcpConst As Double
    Public srcp3 As Integer

    Public InUse As Boolean

    Private record As String
    Private Setup As clsSetup

    Public Sub New(ByVal mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Friend Sub build()

        record = "STFR id '" & ID & "' ci '" & ci & "' mf " & mf & " "
        record &= "mt cp " & mtcp & " " & mtcpConst & " " & mtcp3 & " "
        record &= "mr cp " & mrcp & " " & mrcpConst & " " & mrcp3 & " "
        record &= "s1 " & s1 & " s2 " & s2 & " sf " & sf & " "
        record &= "st cp " & stcp & " " & stcpConst & " " & stcp3 & " "
        record &= "sr cp " & srcp & " " & srcpConst & " " & srcp3 & " stfr"

    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim myStr As String
        record = myRecord

        While Not myRecord = ""
            myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case Is = "id"
                    ID = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ci"
                    ci = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "mf"
                    mf = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "mt"
                    myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If myStr = "cp" Then
                        mtcp = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If mtcp = 0 Then
                            mtcpConst = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                            mtcp3 = 0
                        End If
                    End If
                Case Is = "mr"
                    myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If myStr = "cp" Then
                        mrcp = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If mrcp = 0 Then
                            mrcpConst = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                            mrcp3 = 0
                        End If
                    End If
                Case Is = "s1"
                    s1 = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "s2"
                    s2 = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "sf"
                    sf = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "st"
                    myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If myStr = "cp" Then
                        stcp = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If stcp = 0 Then
                            stcpConst = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                            stcp3 = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                        End If
                    End If
                Case Is = "sr"
                    myStr = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If myStr = "cp" Then
                        srcp = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                        srcpConst = Me.Setup.GeneralFunctions.ParseString(myRecord, " ")
                        srcp3 = 0
                    End If
            End Select
        End While

    End Sub


    Friend Sub write(ByRef datWriter As System.IO.StreamWriter)
        Call build()
        datWriter.WriteLine(record)
    End Sub

End Class
