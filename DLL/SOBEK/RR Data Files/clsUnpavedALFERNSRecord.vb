Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsUnpavedALFERNSRecord
    Friend ID As String
    Friend nm As String
    Friend cvi As Double
    Friend cvo1 As Double
    Friend cvo2 As Double
    Friend cvo3 As Double
    Friend cvo4 As Double
    Friend lv1 As Double
    Friend lv2 As Double
    Friend lv3 As Double
    Friend cvs As Double
    Friend record As String
    Friend InUse As Boolean
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub Create(ByRef mySetup As clsSetup, myID As String, myName As String, mycvi As Double, mycvo1 As Double, mycvo2 As Double, mycvo3 As Double, mycvo4 As Double, mylv1 As Double, mylv2 As Double, mylv3 As Double, mycvs As Double)
        ID = myID
        nm = myName
        cvi = mycvi
        cvo1 = mycvo1
        cvo2 = mycvo2
        cvo3 = mycvo3
        cvo4 = mycvo4
        lv1 = mylv1
        lv2 = mylv2
        lv3 = mylv3
        cvs = mycvs
        InUse = True
    End Sub

    Friend Sub BuildRecord()
        record = "ERNS id '" & ID & "' nm '" & nm & "' cvi " & cvi & " cvo " & cvo1 & " " & cvo2 & " " & cvo3 & " " & cvo4 & " lv " & lv1 & " " & lv2 & " " & lv3 & " cvs " & cvs & " erns"
    End Sub

    Friend Sub Write(ByVal myWriter As StreamWriter)
        Call BuildRecord()
        myWriter.WriteLine(record)
    End Sub

    Friend Sub Read(ByVal myRecord As String)
        Dim Done As Boolean, myStr As String
        Done = False

        While Not myRecord = ""
            myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            Select Case LCase(myStr)
                Case "id"
                    ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "nm"
                    nm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "cvi"
                    cvi = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "cvo"
                    cvo1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    cvo2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    cvo3 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    cvo4 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "lv"
                    lv1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    lv2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    lv3 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Case "cvs"
                    cvs = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
            End Select
        End While
    End Sub
End Class
