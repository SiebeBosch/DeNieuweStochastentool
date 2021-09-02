Option Explicit On
Imports STOCHLIB.General
Imports System.IO

Public Class clsCFFrictionData
    Friend FrictionDatBDFRRecords As clsFrictionDatBDFRRecords 'also includes the GLFR record
    Friend FrictionDatSTFRRecords As clsFrictionDatSTFRRecords
    Friend FrictionDatCRFRRecords As clsFrictionDatCRFRRecords
    Friend FrictionDatXRSTRecords As clsFrictionDatXRSTRecords 'extra resistance objects
    Friend FrictionDatD2FRRecords As clsFrictionDatD2FRRecords
    Private setup As clsSetup
    Private SbkCase As ClsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As ClsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase

        ' Init classes:
        FrictionDatBDFRRecords = New clsFrictionDatBDFRRecords(Me.setup)
        FrictionDatSTFRRecords = New clsFrictionDatSTFRRecords(Me.setup)
        FrictionDatCRFRRecords = New clsFrictionDatCRFRRecords(Me.setup)
        FrictionDatXRSTRecords = New clsFrictionDatXRSTRecords(Me.setup)
        FrictionDatD2FRRecords = New clsFrictionDatD2FRRecords(Me.setup)
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        FrictionDatBDFRRecords.AddPrefix(Prefix)
        FrictionDatSTFRRecords.AddPrefix(Prefix)
        FrictionDatCRFRRecords.AddPrefix(Prefix)
        FrictionDatXRSTRecords.AddPrefix(Prefix)
        FrictionDatD2FRRecords.AddPrefix(Prefix)
    End Sub

    Friend Sub InitializeExport(ExportDir As String)
        If System.IO.File.Exists(ExportDir & "\friction.dat") Then System.IO.File.Delete(ExportDir & "\friction.dat")
    End Sub

    ''' <summary>
    ''' TODO: Siebe invullen
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Export(ByVal Append As Boolean, ExportDir As String)

        Try
            Using frictionDatWriter As New StreamWriter(ExportDir & "\friction.dat", Append)
                Me.FrictionDatBDFRRecords.WriteGlobal(frictionDatWriter)
                Me.FrictionDatSTFRRecords.Write(frictionDatWriter)
                Me.FrictionDatCRFRRecords.Write(frictionDatWriter)
                Me.FrictionDatBDFRRecords.Write(frictionDatWriter)
                Me.FrictionDatXRSTRecords.Write(frictionDatWriter)
                Me.FrictionDatD2FRRecords.Write(frictionDatWriter)
                frictionDatWriter.Close()
            End Using
        Catch ex As Exception
            Dim log As String = "Error in Export CF friction data"
            Me.setup.Log.AddError(log + ": " + ex.Message)
            Throw New Exception(log, ex)
        End Try
    End Sub

    Public Sub BuildGlobalFrictionData(ByVal FrictionType As STOCHLIB.GeneralFunctions.enmFrictionType, FrictionVal As Double)
        FrictionDatBDFRRecords.GLFRRecord = New clsFrictionDatBDFRRecord(Me.setup)
        FrictionDatBDFRRecords.GLFRRecord.ID = "0"
        FrictionDatBDFRRecords.GLFRRecord.ci = "0"
        FrictionDatBDFRRecords.GLFRRecord.mf = FrictionType
        FrictionDatBDFRRecords.GLFRRecord.mtcp = 0
        FrictionDatBDFRRecords.GLFRRecord.mtcpConstant = FrictionVal
        FrictionDatBDFRRecords.GLFRRecord.mrcp = 0
        FrictionDatBDFRRecords.GLFRRecord.mrcpConstant = FrictionVal
        FrictionDatBDFRRecords.GLFRRecord.sf = FrictionType
        FrictionDatBDFRRecords.GLFRRecord.stcp = 0
        FrictionDatBDFRRecords.GLFRRecord.stcpConstant = FrictionVal
        FrictionDatBDFRRecords.GLFRRecord.srcp = 0
        FrictionDatBDFRRecords.GLFRRecord.srcpConstant = FrictionVal
    End Sub

    Public Function GetAddXRSTRecord(myID As String) As clsFrictionDatXRSTRecord
        If Not FrictionDatXRSTRecords.records.ContainsKey(myID.Trim.ToUpper) Then
            Dim myRecord As New clsFrictionDatXRSTRecord(Me.setup)
            myRecord.ID = myID
            myRecord.nm = myID
            FrictionDatXRSTRecords.records.Add(myRecord.ID.Trim.ToUpper, myRecord)
        End If
        Return FrictionDatXRSTRecords.records.Item(myID.Trim.ToUpper)
    End Function

    Public Function getCRFRRecordByProfileDefinition(ProfID As String) As clsFrictionDatCRFRRecord
        For Each myRecord As clsFrictionDatCRFRRecord In FrictionDatCRFRRecords.records.Values
            If myRecord.cs.Trim.ToUpper = ProfID.Trim.ToUpper Then
                Return myRecord
            End If
        Next
        Return Nothing
    End Function

End Class
