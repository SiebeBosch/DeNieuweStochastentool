Option Explicit On

Imports System.IO
Imports STOCHLIB.General

Public Class clsInitialDatFLINRecords

    Friend records As New Dictionary(Of String, clsInitialDatFLINRecord)

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase

    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase
    End Sub

    Friend Sub AddPrefix(ByVal Prefix As String)
        For Each FLIN As clsInitialDatFLINRecord In records.Values
            FLIN.ID = Prefix & FLIN.ID
        Next
    End Sub

End Class

