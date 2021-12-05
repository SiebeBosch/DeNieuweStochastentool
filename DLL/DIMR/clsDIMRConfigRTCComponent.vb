Option Explicit On
Imports STOCHLIB.General
Imports System.IO


Public Class clsDIMRCONFIGRTCComponent
    Inherits clsDIMRConfigComponent

    Public Sub New(ByRef MySetup As clsSetup, ByRef myDIMR As clsDIMR)
        MyBase.New(MySetup, myDIMR)
    End Sub

End Class
