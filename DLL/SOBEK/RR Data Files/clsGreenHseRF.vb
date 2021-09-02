Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsGreenHseRF

    Public FileContent As New Collection 'of string
    Public Records As New Dictionary(Of String, clsGreenHseRFRecord)
    Private Setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub


    Friend Sub Read(ByVal casedir As String, ByRef myModel As clsSobekCase)

        'leest unpaved definitions in
        Dim Datafile = New clsSobekDataFile(Me.Setup)
        Dim i As Long

        FileContent = Datafile.Read(casedir & "\greenhse.rf", "STDF")
        For i = 1 To FileContent.Count
            Dim STDFRecord As clsGreenHseRFRecord
            STDFRecord = New clsGreenHseRFRecord(Me.Setup)
            STDFRecord.Read(FileContent(i))
            If Not Records.ContainsKey(STDFRecord.ID.Trim.ToUpper) Then
                Records.Add(STDFRecord.ID.Trim.ToUpper, STDFRecord)
            Else
                Me.Setup.Log.AddWarning("Unpaved.3b contains multiple instances of record with ID " & STDFRecord.ID)
            End If
        Next
    End Sub

    Friend Function getAddGreenHseRFRecord(ByVal myStor As Double, ByVal iniStor As Double) As clsGreenHseRFRecord
        Dim myGRRF As New clsGreenHseRFRecord(Me.Setup)
        If Not Records.ContainsKey(myStor) Then
            myGRRF = New clsGreenHseRFRecord(Me.Setup)
            myGRRF.ID = "STOR" & Records.Count.ToString.Trim
            myGRRF.nm = myGRRF.ID
            myGRRF.mk = myStor
            myGRRF.ik = iniStor
            Records.Add(myStor, myGRRF)
            Return myGRRF
        Else
            Return Records.Item(myStor)
        End If
    End Function

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Using grRFWriter As New StreamWriter(ExportDir & "\greenhse.rf", Append)
            For Each myRecord As clsGreenHseRFRecord In Records.Values
                Call myRecord.Write(grRFWriter)
            Next myRecord
            grRFWriter.Close()
        End Using
    End Sub

End Class
