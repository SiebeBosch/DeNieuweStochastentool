Option Explicit On
Imports System.IO
Imports STOCHLIB.General

Public Class clsUnpavedSep

    Friend FileContent As New Collection 'of string
    Friend Records As New Dictionary(Of String, clsUnpavedSEPRecord)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each SEP As clsUnpavedSEPRecord In Records.Values
            SEP.ID = Prefix & SEP.ID
            SEP.nm = Prefix & SEP.nm
        Next
    End Sub

    Friend Function getAddRecord(ByRef myArea As clsSubcatchment, ByRef SeasonTransitions As clsSeasonTransitions) As clsUnpavedSEPRecord
        Dim myUPSEP As New clsUnpavedSEPRecord(Me.setup)
        If Not Records.ContainsKey(myArea.ID.Trim.ToUpper) Then
            myUPSEP = New clsUnpavedSEPRecord(Me.setup)
            myUPSEP.ID = myArea.ID
            myUPSEP.nm = myUPSEP.ID

            If myArea.SeepageSummer = myArea.SeepageWinter Then
                'constant seepage
                myUPSEP.co = 1
                myUPSEP.sp = myArea.SeepageWinter
            Else
                myUPSEP.co = 4
                myUPSEP.TimeTable = New clsSobekTable(Me.setup)
                myUPSEP.TimeTable.pdin1 = 0
                myUPSEP.TimeTable.pdin2 = 1
                myUPSEP.TimeTable.PDINPeriod = "'365;00:00:00'"
                myUPSEP.TimeTable.AddDatevalPair(New Date(2000, 1, 1), myArea.SeepageWinter)
                myUPSEP.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.WinSumStartMonth, SeasonTransitions.WinSumStartDay), myArea.SeepageWinter)
                myUPSEP.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.WinSumEndMonth, SeasonTransitions.WinSumEndDay), myArea.SeepageSummer)
                myUPSEP.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.SumWinStartMonth, SeasonTransitions.SumWinStartDay), myArea.SeepageSummer)
                myUPSEP.TimeTable.AddDatevalPair(New Date(2000, SeasonTransitions.SumWinEndMonth, SeasonTransitions.SumWinEndDay), myArea.SeepageWinter)
            End If

            Records.Add(myArea.ID.Trim.ToUpper, myUPSEP)
            Return myUPSEP
        Else
            Return Records.Item(myArea.ID.Trim.ToUpper)
        End If
    End Function

    Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

        'leest unpaved definitions in
        Dim Datafile = New clsSobekDataFile(Me.setup)
        Dim i As Long

        FileContent = Datafile.Read(casedir & "\unpaved.sep", "SEEP")
        For i = 1 To FileContent.Count
            Dim SEEPRecord As clsUnpavedSEPRecord
            SEEPRecord = New clsUnpavedSEPRecord(Me.setup)
            SEEPRecord.Read(FileContent(i))
            If Not Records.ContainsKey(SEEPRecord.ID.Trim.ToUpper) Then
                Records.Add(SEEPRecord.ID.Trim.ToUpper, SEEPRecord)
            Else
                Me.setup.Log.AddWarning("Double instances of seepage definition " & SEEPRecord.ID & " in " & casedir)
            End If
        Next
    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using upSepWriter As New StreamWriter(ExportDir & "\unpaved.sep", Append)
            For Each myRecord As clsUnpavedSEPRecord In Records.Values
                myRecord.Write(upSepWriter)
            Next myRecord
            upSepWriter.Close()
        End Using
    End Sub

    Friend Sub AddConstant(ByVal myVal As Double)
        Dim i As Integer = 0
        Dim n As Integer = Records.Count
        Me.setup.GeneralFunctions.UpdateProgressBar("Adjusting unpaved seepage values...", 0, 10, True)
        For Each myRecord As clsUnpavedSEPRecord In Records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, n)
            myRecord.sp += myVal
        Next
        Me.setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
    End Sub

    Friend Sub SetConstant(ByVal myVal As Double)
        Dim i As Integer = 0
        Dim n As Integer = Records.Count
        Me.setup.GeneralFunctions.UpdateProgressBar("Adjusting unpaved seepage values...", 0, 10, True)
        For Each myRecord As clsUnpavedSEPRecord In Records.Values
            i += 1
            Me.setup.GeneralFunctions.UpdateProgressBar("", i, n)
            myRecord.sp = myVal
        Next
        Me.setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
    End Sub



    Friend Sub Write(ByVal myPath As String, ByVal Append As Boolean)
        Dim i As Integer = 0, n As Integer = Records.Count
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing unpaved seepage records...", i, n, True)
        Using upSepWriter As New StreamWriter(myPath, Append)
            For Each myRecord As clsUnpavedSEPRecord In Records.Values
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, n)
                Call myRecord.Write(upSepWriter)
            Next myRecord
            upSepWriter.Close()
            Me.setup.GeneralFunctions.UpdateProgressBar("Operation complete.", 0, 10, True)
        End Using
    End Sub
End Class
