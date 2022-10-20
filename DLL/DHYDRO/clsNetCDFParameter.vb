Imports STOCHLIB.General
Imports Microsoft.Research.Science.Data
Imports Microsoft.Research.Science.Data.Imperative

Public Class clsNetCDFParameter
    Dim Idx As Integer = -1
    Dim start_index As Integer
    Dim name As String
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Function GetIdx() As Integer
        Return Idx
    End Function

    Public Function getStartIndex() As Integer
        Return start_index
    End Function

    Public Function getName() As String
        Return name
    End Function

    Public Sub setIdx(myIdx As Integer)
        Idx = myIdx
    End Sub

    Public Sub setStartIndex(myStartIndex As Integer)
        start_index = myStartIndex
    End Sub

    Public Sub setName(myname As String)
        name = myname
    End Sub

End Class
