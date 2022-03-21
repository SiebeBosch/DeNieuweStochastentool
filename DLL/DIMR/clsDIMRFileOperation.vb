Imports STOCHLIB.General
Imports System.IO
Public Class clsDIMRFileOperation
    Dim ReplacementAction As GeneralFunctions.enmDIMRIniFileReplacementOperation
    Dim Header As String
    Dim ObjectID As String
    Dim Attribute As String
    Dim Value As String
    Dim ModuleName As String        'the name of our hydro-module as named in dimr_config.xml
    Dim FileName As String          'the actual name of the file we need to make adjustments to

    Dim IdentifierAttributeName As String    'the attribute by which we will decide which section to adjust
    Dim IdentifierAttributeValue As String   'the attribute value

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Function getReplacementAction() As GeneralFunctions.enmDIMRIniFileReplacementOperation
        Return ReplacementAction
    End Function

    Public Sub setFileName(myFileName As String)
        FileName = myFileName
    End Sub

    Public Function getFileName() As String
        Return FileName
    End Function

    Public Sub setModuleName(myModuleName As String)
        ModuleName = myModuleName
    End Sub

    Public Function getModuleName() As String
        Return ModuleName
    End Function

    Public Function SetAction(myAction As String) As Boolean
        Try
            ReplacementAction = DirectCast([Enum].Parse(GetType(GeneralFunctions.enmDIMRIniFileReplacementOperation), myAction), GeneralFunctions.enmDIMRIniFileReplacementOperation)
            Return True
        Catch ex As Exception
            Return False
        End Try
        ReplacementAction = myAction
    End Function

    Public Function GetAction() As GeneralFunctions.enmDIMRIniFileReplacementOperation
        Return ReplacementAction
    End Function

    Public Sub setValue(myValue As String)
        Value = myValue
    End Sub

    Public Function getValue() As String
        Return Value
    End Function

    Public Sub setHeader(myHeader As String)
        Header = myHeader
    End Sub

    Public Function getHeader() As String
        Return Header
    End Function

    Public Sub setIdentifierAttributeName(myIdentifierAttributeName As String)
        IdentifierAttributeName = myIdentifierAttributeName
    End Sub

    Public Function getIdentifierAttributeName() As String
        Return IdentifierAttributeName
    End Function

    Public Sub setAttributeName(myAttribute As String)
        Attribute = myAttribute
    End Sub

    Public Function getAttributeName() As String
        Return Attribute
    End Function

    Public Sub setIdentifierAttributeValue(myIdentifierAttributeValue As String)
        IdentifierAttributeValue = myIdentifierAttributeValue
    End Sub

    Public Function getIdentifierAttributeValue() As String
        Return IdentifierAttributeValue
    End Function

    Public Sub setObjectID(myObjectID As String)
        ObjectID = myObjectID
    End Sub


End Class
