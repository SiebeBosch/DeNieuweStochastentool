Public Class clsDIMRFileOperation
    Dim FileType As STOCHLIB.GeneralFunctions.enmDIMRFileType
    Dim Action As STOCHLIB.GeneralFunctions.enmDIMRFileOperation
    Dim Header As String
    Dim ObjectID As String
    Dim Attribute As String
    Dim Value As String

    Public Sub New()

    End Sub

    Public Function SetFileType(myFileType As String) As Boolean
        Try
            FileType = DirectCast([Enum].Parse(GetType(GeneralFunctions.enmDIMRFileType), myFileType), GeneralFunctions.enmDIMRFileType)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function SetAction(myAction As String) As Boolean
        Try
            Action = DirectCast([Enum].Parse(GetType(GeneralFunctions.enmDIMRFileOperation), myAction), GeneralFunctions.enmDIMRFileOperation)
            Return True
        Catch ex As Exception
            Return False
        End Try
        Action = myAction
    End Function

    Public Sub setValue(myPath As String)
        Value = myPath
    End Sub

    Public Sub setHeader(myHeader As String)
        Header = myHeader
    End Sub

    Public Sub setObjectID(myObjectID As String)
        ObjectID = myObjectID
    End Sub

    Public Sub setAttribute(myAttribute As String)
        Attribute = myAttribute
    End Sub

End Class
