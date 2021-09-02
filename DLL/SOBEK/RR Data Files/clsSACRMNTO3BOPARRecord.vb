Imports STOCHLIB.General

Public Class clsSACRMNTO3BOPARRecord

    Friend ID As String
    Friend Name As String

    'werkelijke .3B-content
    Friend zperc As Double
    Friend rexp As Double
    Friend pfree As Double
    Friend rserv As Double
    Friend pctim As Double
    Friend adimp As Double
    Friend sarva As Double
    Friend side As Double
    Friend ssout As Double
    Friend pm As Double
    Friend pt1 As Double
    Friend pt2 As Double

    Friend InUse As Boolean
    Friend record As String   'het gehele record

    Private setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub


    Friend Sub Create(ByRef mySetup As clsSetup, myID As String, myName As String, myzperc As Double, myrexp As Double, mypfree As Double, myrserv As Double, mypctim As Double, myadimp As Double, mysarva As Double, myside As Double, myssout As Double, mypm As Double, mypt1 As Double, mypt2 As Double)
        ID = myID
        Name = myName
        zperc = myzperc
        rexp = myrexp
        pfree = mypfree
        rserv = myrserv
        pctim = mypctim
        adimp = myadimp
        sarva = mysarva
        side = myside
        ssout = myssout
        pm = mypm
        pt1 = mypt1
        pt2 = mypt2
        InUse = True
    End Sub


    Friend Sub Build()
        'deze routine bouwt het unpaved.3b-record op, op basis van de parameterwaarden
        record = "OPAR id '" & ID & "' nm '" & Name & "' zperc " & zperc & " rexp " & rexp & " pfree " & pfree & " rserv " & rserv & " pctim " & pctim & " adimp " & adimp & " sarva " & sarva & " side " & side & " ssout " & ssout & " pm " & pm & " pt1 " & pt1 & " pt2 " & pt2 & " opar"
    End Sub

    Friend Sub Write(ByVal myWriter As System.IO.StreamWriter)
        Call Build()
        Call myWriter.WriteLine(record)
    End Sub

    Friend Function Read(ByVal myRecord As String) As Boolean
        Dim Done As Boolean, myStr As String
        Done = False
        Try
            'initialize tokens that might be missing

            While Not myRecord = ""
                myStr = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                Select Case LCase(myStr)
                    Case "id"
                        ID = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "nm"
                        Name = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "zperc"
                        zperc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "rexp"
                        rexp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "pfree"
                        pfree = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "rserv"
                        rserv = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "pctim"
                        pctim = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "adimp"
                        adimp = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "sarva"
                        sarva = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "side"
                        side = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "ssout"
                        ssout = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "pm"
                        pm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "pt1"
                        pt1 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "pt2"
                        pt2 = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                End Select
            End While

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error while reading sacrmnto3b OPAR record " & ID)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function
End Class
