Option Explicit On
Imports System.IO
Imports System.Windows.Forms
Imports STOCHLIB.General

Friend Class clsStructDefRecords

    Friend Records As New Dictionary(Of String, clsStructDefRecord)
    Private setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each Def As clsStructDefRecord In Records.Values
            Def.ID = Prefix & Def.ID
            Def.nm = Prefix & Def.nm
            If Def.si <> "" Then Def.si = Prefix & Def.si
        Next
    End Sub

    Friend Sub InitializeExport(ExportDir As String)
        If System.IO.File.Exists(ExportDir & "\struct.def") Then System.IO.File.Delete(ExportDir & "\struct.def")
    End Sub

    Friend Sub Read(ByVal myStrings As Collection)
        Dim i As Integer = 0
        Dim Found As Boolean = False
        Dim myDat As clsStructDatRecord
        Dim myObj As clsSbkReachObject

        Try
            Me.setup.GeneralFunctions.UpdateProgressBar("Reading structure definitions...", 0, myStrings.Count)
            For Each myString As String In myStrings
                i += 1

                Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
                Dim myRecord As clsStructDefRecord = New clsStructDefRecord(Me.setup)
                myRecord.Read(myString)

                'alleen importeren als hij ook daadwerkelijk door een struct.dat-record wordt aangeroepen 
                'EN van hetzelfde type is als het takobject. Zoek daarom op welke structdat-record hierbij hoort
                myDat = SbkCase.CFData.Data.StructureData.StructDatRecords.FindByDefinitionID(myRecord.ID.Trim.ToUpper)
                If Not myDat Is Nothing Then
                    'zoek op welk objecttype het hier betreft
                    myObj = SbkCase.CFTopo.getReachObject(myDat.ID.Trim.ToUpper, False)
                    If Not myObj Is Nothing Then
                        If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFWeir AndAlso myRecord.ty = 6 Then
                            If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                                Me.setup.Log.AddWarning("Multiple instances of structure definition " & myRecord.ID & " found in SOBEK schematization.")
                            Else
                                Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                            End If
                        ElseIf myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFOrifice AndAlso myRecord.ty = 7 Then
                            If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                                Me.setup.Log.AddWarning("Multiple instances of structure definition " & myRecord.ID & " found in SOBEK schematization.")
                            Else
                                Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                            End If
                        ElseIf myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFPump AndAlso myRecord.ty = 9 Then
                            If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                                Me.setup.Log.AddWarning("Multiple instances of structure definition " & myRecord.ID & " found in SOBEK schematization.")
                            Else
                                Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                            End If
                        ElseIf myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFCulvert AndAlso myRecord.ty = 10 Then
                            If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                                Me.setup.Log.AddWarning("Multiple instances of structure definition " & myRecord.ID & " found in SOBEK schematization.")
                            Else
                                Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                            End If
                        ElseIf myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFUniWeir AndAlso myRecord.ty = 11 Then
                            If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                                Me.setup.Log.AddWarning("Multiple instances of structure definition " & myRecord.ID & " found in SOBEK schematization.")
                            Else
                                Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                            End If
                        ElseIf myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFBridge AndAlso myRecord.ty = 12 Then
                            If Records.ContainsKey(myRecord.ID.Trim.ToUpper) Then
                                Me.setup.Log.AddWarning("Multiple instances of structure definition " & myRecord.ID & " found in SOBEK schematization.")
                            Else
                                Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
                            End If
                        End If
                    End If
                End If
            Next myString
        Catch ex As Exception
            Me.setup.Log.AddError("Error in sub Read of class clsStructDefRecords")
        End Try

    End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Dim i As Integer = 0
        Me.setup.GeneralFunctions.UpdateProgressBar("Writing struct.def file...", 0, Records.Count)
        Using defWriter As New StreamWriter(ExportDir & "\struct.def", Append)
            For Each myRecord As clsStructDefRecord In Records.Values
                i += 1
                Me.setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                If myRecord.InUse Then myRecord.Write(defWriter)
            Next
        End Using
    End Sub

    Friend Function FindByID(ByVal ID As String) As clsStructDefRecord
        If Records.ContainsKey(ID.Trim.ToUpper) Then
            Return Records.Item(ID.Trim.ToUpper)
        Else
            Return Nothing
        End If
    End Function

    Friend Function FindByProfileDefID(ByVal mysi As String) As clsStructDefRecord
        'zoekt het eerste kunstwerk op dat een gegeven profieldefinitie ID gebruikt en geeft dit terug
        'let op: alleen kunstwerken van het type culvert, universal weir en bridge gebruiken dwarsprofieldefinities
        For Each myDef As clsStructDefRecord In Records.Values
            If (myDef.ty = 10 Or myDef.ty = 11 Or myDef.ty = 12) And myDef.si.Trim.ToUpper = mysi.Trim.ToUpper Then
                Return myDef
            End If
        Next myDef
        Return Nothing
    End Function

End Class
