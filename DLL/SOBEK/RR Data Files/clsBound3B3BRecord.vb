Option Explicit On

Imports STOCHLIB.General

Public Class clsBound3B3BRecord
    Friend ID As String
    Friend bl As Integer '0=constant 1=variable
    Friend bl2 As Double 'constant boundary level
    Friend TableID As String 'only if bl = 1
    Friend is_ As Integer
    Friend record As String
    Friend InUse As Boolean

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub Create(ByRef mysetup As clsSetup, ByVal myID As String, ByVal ConstValue As Double, Optional ByVal SetInuse As Boolean = True)
        setup = mysetup
        ID = myID
        bl = 0
        bl2 = ConstValue
        is_ = 0
        InUse = SetInuse
    End Sub

    Friend Sub New(ByRef mySetup As clsSetup, ByVal myID As String, ByVal ZP As Double, ByVal WP As Double)
        setup = mySetup
        ID = myID
        InUse = True
        If ZP = WP Then
            'summer and winter target level are equal, so assign constant value
            bl = 0
            bl2 = ZP
            is_ = 0
        Else
            'summmer and winter target level differ, so we'll have to create a table (in a BoundTBL record)
            bl = 1
            TableID = myID
            is_ = 0
        End If
        InUse = True
    End Sub

    Friend Sub Build()
        'deze routine bouwt het paved.3b-record op, op basis van de parameterwaarden
        If bl = 0 Then
            record = "BOUN id '" & ID & "' bl " & bl & " " & bl2 & " is " & is_ & " boun"
        Else
            record = "BOUN id '" & ID & "' bl " & bl & " '" & TableID & "' is " & is_ & " boun"
        End If
    End Sub

    Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub


    Friend Sub Read(ByVal myRecord As String, Optional ByVal SetInuse As Boolean = True)
        Dim Done As Boolean, myStr As String
        Done = False
        InUse = SetInuse

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "bl"
                    bl = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    If bl = 0 Then 'constant boundary value
                        bl2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    ElseIf bl = 1 Then
                        TableID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    End If
                Case "is"
                    is_ = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            End Select
        End While
    End Sub

End Class
