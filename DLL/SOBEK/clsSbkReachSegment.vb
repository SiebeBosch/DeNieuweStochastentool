Option Explicit On
Imports STOCHLIB.General
Public Class clsSbkReachSegment

    Public ID As String
    Friend ci As String                 'reachID

    Private Setup As clsSetup
    Private SbkCase As ClsSobekCase

    Public Function GetID() As String
        Return ID
    End Function

    Public Function getReachID() As String
        Return ci
    End Function



    Friend Sub New(ByRef mySetup As clsSetup, ByRef mySbkCase As ClsSobekCase)
        Me.Setup = mySetup
        Me.SbkCase = mySbkCase
    End Sub


End Class
