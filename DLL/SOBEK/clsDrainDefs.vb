Imports System.IO
Imports STOCHLIB.General

Public Class clsDrainDefs
    Public DrainDefs As New Dictionary(Of String, clsDrainageProfiel)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Public Sub Add(ByVal myDef As clsDrainageProfiel)
        If Not DrainDefs.ContainsKey(myDef.id.Trim.ToUpper) Then
            DrainDefs.Add(myDef.id.Trim.ToUpper, myDef)
        Else
            Me.setup.Log.AddWarning("The collection already contained a drainage definition named " & myDef.id.Trim.ToUpper & ".")
        End If
    End Sub


    Public Sub write(ByVal Append As Boolean, ExportDir As String)
        Using upAlfWriter = New StreamWriter(ExportDir & "\unpaved.alf", Append)
            For Each myDef As clsDrainageProfiel In DrainDefs.Values
                myDef.write(upAlfWriter)
            Next
            upAlfWriter.Close()
        End Using

    End Sub

End Class
