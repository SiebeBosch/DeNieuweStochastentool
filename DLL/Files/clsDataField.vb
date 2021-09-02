Public Class clsDataField
    Friend ID As String
    Friend TextFileColIdx As Integer = -1        'index number of the column inside e.g. a CSV file
    Friend FieldType As GeneralFunctions.enmDataFieldType
    Friend SQLiteFieldType As GeneralFunctions.enmSQLiteDataType
    Friend DataType As System.Type
    Friend DateFormatting As String 'in case we're dealing with a datetime datatype
    Friend ConstantValue As Object  'in case we're reading a constant value instead of a data field

    Friend StartDate As DateTime        'for situations where no date column is given but a startdate instead
    Friend TimestepSeconds As Integer   'for situations where no date column is given but a startdate instead

    Public Sub New()

    End Sub

    Public Sub New(myID As String)
        ID = myID
    End Sub

    Public Sub New(myID As String, myDataType As System.Type)
        ID = myID
        DataType = myDataType
    End Sub
    Public Sub New(myID As String, myFieldType As STOCHLIB.GeneralFunctions.enmDataFieldType)
        ID = myID
        FieldType = myFieldType
    End Sub
    Public Sub New(myID As String, myFieldType As STOCHLIB.GeneralFunctions.enmSQLiteDataType)
        ID = myID
        SQliteFieldType = myFieldType
    End Sub

    Public Sub New(myID As String, myFieldType As GeneralFunctions.enmDataFieldType, myConst As Object)
        ID = myID
        FieldType = myFieldType
        ConstantValue = myConst
    End Sub

    Public Sub New(myID As String, myDataType As System.Type, myFieldType As GeneralFunctions.enmDataFieldType)
        ID = myID
        DataType = myDataType
        FieldType = myFieldType
    End Sub


    Public Sub New(myID As String, myFieldType As GeneralFunctions.enmDataFieldType, myStartDate As DateTime, myTimestepSeconds As Integer)
        ID = myID
        FieldType = myFieldType
        StartDate = myStartDate
        TimestepSeconds = myTimestepSeconds
    End Sub

    Public Sub New(myID As String, myDataType As System.Type, myFieldType As GeneralFunctions.enmDataFieldType, myFieldIdx As Integer, Optional myDateFormatting As String = "")
        ID = myID
        TextFileColIdx = myFieldIdx
        DataType = myDataType
        FieldType = myFieldType
        DateFormatting = myDateFormatting
    End Sub

    Public Function GetID() As String
        Return ID
    End Function

    Public Function GetSQLiteType() As GeneralFunctions.enmSQLiteDataType
        Return SQLiteFieldType
    End Function
    Public Function GetValue(records As List(Of String)) As Object
        If TextFileColIdx >= 0 Then
            Return records(TextFileColIdx)
        Else
            Return ConstantValue
        End If
    End Function

End Class
