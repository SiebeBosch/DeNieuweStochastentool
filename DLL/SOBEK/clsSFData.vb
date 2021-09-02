Option Explicit On
Imports System.IO
Imports STOCHLIB.General
Imports STOCHLIB.GeneralFunctions
Imports GemBox.Spreadsheet

Public Class clsSFData
    Friend Data As clsSFAttributeData

    Private Setup As clsSetup
    Private SbkCase As clsSobekCase


    Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = myCase

        'Init classes:
        Data = New clsSFAttributeData(Me.Setup, Me.SbkCase)

    End Sub
End Class
