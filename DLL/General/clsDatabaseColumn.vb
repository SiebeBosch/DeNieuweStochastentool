Public Class clsDatabaseColumn
    Public ID As String
    Public Prefix As String
    Public DataType As GeneralFunctions.enmOleDBDataType
    Public DateFormatting As String
    Public SourceDataColumnIdx As Integer
    Public ConstantVal As Object

    Public Sub New(ByVal myID As String, ByVal myPrefix As String, mydataType As GeneralFunctions.enmOleDBDataType, Optional ByVal mySourceDataColumnIdx As Integer = -1, Optional ByVal myDateFormatting As String = "", Optional ByVal myConstantVal As Object = Nothing)
        ID = myID
        Prefix = myPrefix
        DataType = mydataType
        DateFormatting = myDateFormatting
        SourceDataColumnIdx = mySourceDataColumnIdx
        ConstantVal = myConstantVal
    End Sub
End Class
