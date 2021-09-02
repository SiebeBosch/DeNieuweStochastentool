Imports STOCHLIB.General

Public Class clsSACRMNTO3BCAPSRecord

    Friend ID As String
    Friend Name As String

    'werkelijke .3B-content
    Friend uztwm As Double
    Friend uztwc As Double
    Friend uzfwm As Double
    Friend uzfwc As Double
    Friend lztwm As Double
    Friend lztwc As Double
    Friend lzfsm As Double
    Friend lzfsc As Double
    Friend lzfpm As Double
    Friend lzfpc As Double
    Friend uzk As Double
    Friend lzsk As Double
    Friend lzpk As Double

    Friend InUse As Boolean
    Friend record As String   'het gehele record

    Private setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub Create(ByRef mySetup As clsSetup, myID As String, myName As String, myuztwm As Double, myuztwc As Double, myuzfwm As Double, myuzfwc As Double, mylztwm As Double, mylztwc As Double, mylzfsm As Double, mylzfsc As Double, mylzfpm As Double, mylzfpc As Double, myuzk As Double, mylzsk As Double, mylzpk As Double)
        ID = myID
        Name = myName
        uztwm = myuztwm
        uztwc = myuztwc
        uzfwm = myuzfwm
        uzfwc = myuzfwc
        lztwm = mylztwm
        lztwc = mylztwc
        lzfsm = mylzfsm
        lzfsc = mylzfsc
        lzfpm = mylzfpm
        lzfpc = mylzfpc
        uzk = myuzk
        lzsk = mylzsk
        lzpk = mylzpk
        InUse = True
    End Sub

    Friend Sub Build()
        'deze routine bouwt het unpaved.3b-record op, op basis van de parameterwaarden
        record = "CAPS id '" & ID & "' nm '" & Name & "' uztwm " & uztwm & " uztwc " & uztwc & " uzfwm " & uzfwm & " uzfwc " & uzfwc & " lztwm " & lztwm & " lztwc " & lztwc & " lzfsm " & lzfsm & " lzfsc " & lzfsc & " lzfpm " & lzfpm & " lzfpc " & lzfpc & " uzk " & uzk & " lzsk " & lzsk & " lzpk " & lzpk & " caps"
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
                    Case "uztwm"
                        uztwm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "uztwc"
                        uztwc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "uzfwm"
                        uzfwm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "uzfwc"
                        uzfwc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "lztwm"
                        lztwm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "lztwc"
                        lztwc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "lzfsm"
                        lzfsm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "lzfsc"
                        lzfsc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "lzfpm"
                        lzfpm = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "lzfpc"
                        lzfpc = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "uzk"
                        uzk = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "lzsk"
                        lzsk = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                    Case "lzpk"
                        lzpk = Me.setup.GeneralFunctions.ParseString(myRecord, " ")
                End Select
            End While

            Return True
        Catch ex As Exception
            Me.setup.Log.AddError("Error while reading sacrmnto3b CAPS record " & ID)
            Me.setup.Log.AddError(ex.Message)
            Return False
        End Try

    End Function
End Class
