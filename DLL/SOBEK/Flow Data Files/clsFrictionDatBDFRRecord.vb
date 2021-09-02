Option Explicit On

Imports STOCHLIB.General

Public Class clsFrictionDatBDFRRecord

    Public hosttype As String
    Public ID As String
    Public ci As String
    Public s1 As Integer
    Public s2 As Integer
    Public mf As STOCHLIB.GeneralFunctions.enmFrictionType
    Public mtcp As Integer '0=const,1=function of dist
    Public mtcpConstant As Double
    Public mrcp As Integer  'reverse direction
    Public mrcpConstant As Double
    Public sf As STOCHLIB.GeneralFunctions.enmFrictionType 'ground layer
    Public stcp As Integer 'ground layer friction 0=const,1=function of dist
    Public stcpConstant As Double
    Public srcp As Integer 'ground layer fricion in reverse direction
    Public srcpConstant As Double
    Public record As String

    Friend InUse As Boolean

    Private Setup As clsSetup

    Public Sub New(ByVal mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Public Function Clone(newID As String) As clsFrictionDatBDFRRecord
        Dim newDat As New clsFrictionDatBDFRRecord(Me.Setup)
        newDat.hosttype = hosttype
        newDat.ID = newID
        newDat.ci = newID
        newDat.s1 = s1
        newDat.s2 = s2
        newDat.mf = mf
        newDat.mtcp = mtcp
        newDat.mtcpConstant = mtcpConstant
        newDat.mrcp = mrcp
        newDat.mrcpConstant = mrcpConstant
        newDat.sf = sf
        newDat.stcp = stcp
        newDat.stcpConstant = stcpConstant
        newDat.srcp = srcp
        newDat.srcpConstant = srcpConstant
        newDat.build()
        newDat.InUse = True
        Return newDat
    End Function

    Friend Sub build()
        If sf <> mf Then
            Me.Setup.Log.AddWarning("Groundlayer friction was different from main friction for reach " & ID & " which is not allowed. The groundlayer friction has been replaced by the bed friction fix this.")
            sf = mf
            stcp = mtcp
            stcpConstant = mtcpConstant
            srcp = mrcp
            srcpConstant = mrcpConstant
        End If
        record = "BDFR id '" & ID & "' ci '" & ci & "' mf " & mf & " mt cp " & mtcp & " " & mtcpConstant & " 0 mr cp " & mrcp & " " & mrcpConstant & " 0 s1 " & s1 & " s2 " & s2 & " sf " & sf & " st cp " & stcp & " " & stcpConstant & " 0 sr cp " & srcp & " " & srcpConstant & " 0 bdfr"
    End Sub

    Friend Sub write(ByRef datWriter As System.IO.StreamWriter, Optional ByVal IsGlobal As Boolean = False)
        Call build()
        If IsGlobal Then record = "GLFR " & record & " glfr"
        datWriter.WriteLine(record)
    End Sub

    Public Sub read(ByRef Setup As clsSetup, ByVal myRecord As String, ByVal myHost As String)
        Dim Done As Boolean, myStr As String, tmp As String
        Done = False

        hosttype = myHost
        Dim sfProcessed As Boolean = False

        While Not Done
            myStr = Setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case myStr
                Case Is = "id"
                    ID = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "ci"
                    ci = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "s1"
                    s1 = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "s2"
                    s2 = Setup.GeneralFunctions.ParseString(myRecord, " ")
                Case Is = "mf"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    Select Case tmp
                        Case Is = "0"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.Chezy
                        Case Is = "1"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.Manning
                        Case Is = "2"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.StricklerKN
                        Case Is = "3"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.StricklerKS
                        Case Is = "4"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.WhiteColebrook
                        Case Is = "7"
                            mf = STOCHLIB.GeneralFunctions.enmFrictionType.BosBijkerk
                    End Select
                Case Is = "sf"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    sfProcessed = True
                    Select Case tmp
                        Case Is = 0
                            sf = STOCHLIB.GeneralFunctions.enmFrictionType.Chezy
                        Case Is = 1
                            sf = STOCHLIB.GeneralFunctions.enmFrictionType.Manning
                        Case Is = 2
                            sf = STOCHLIB.GeneralFunctions.enmFrictionType.StricklerKN
                        Case Is = 3
                            sf = STOCHLIB.GeneralFunctions.enmFrictionType.StricklerKS
                        Case Is = 4
                            sf = STOCHLIB.GeneralFunctions.enmFrictionType.WhiteColebrook
                        Case Is = "7"
                            sf = STOCHLIB.GeneralFunctions.enmFrictionType.BosBijkerk
                    End Select
                Case Is = "mt"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "cp" Then
                        tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If tmp = "0" Then mtcpConstant = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
                Case Is = "mr"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "cp" Then
                        tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If tmp = "0" Then mrcpConstant = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
                Case Is = "st"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "cp" Then
                        tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If tmp = "0" Then stcpConstant = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
                Case Is = "sr"
                    tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    If tmp = "cp" Then
                        tmp = Setup.GeneralFunctions.ParseString(myRecord, " ")
                        If tmp = "0" Then srcpConstant = Setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
                Case Is = ""
                    Done = True
            End Select
        End While

        'v1.880 if groundlayer friction has not been processed, assume same as main friction
        If Not sfProcessed Then
            sf = mf
            stcp = mtcp
            stcpConstant = mtcpConstant
        End If

    End Sub

End Class
