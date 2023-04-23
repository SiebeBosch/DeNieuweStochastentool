
Imports STOCHLIB.General
Public Class clsSQLiteField

    Private Setup As clsSetup

    Public Property FieldName As String
    Public Property DataType As STOCHLIB.GeneralFunctions.enmSQLiteDataType
    Public Property HasIndex As Boolean

    Public Shared Function SQLiteDataTypeToString(ByVal dataType As STOCHLIB.GeneralFunctions.enmSQLiteDataType) As String
        Select Case dataType
            Case STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITETEXT
                Return "TEXT"
            Case STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEINT
                Return "INTEGER"
            Case STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEREAL
                Return "REAL"
            Case STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITENULL
                Return "NULL"
            Case STOCHLIB.GeneralFunctions.enmSQLiteDataType.SQLITEBLOB
                Return "BLOB"
            Case Else
                Throw New ArgumentOutOfRangeException("Invalid data type.")
        End Select
    End Function

    Public Sub New(myFieldName As String, myDataType As STOCHLIB.GeneralFunctions.enmSQLiteDataType, myHasIndex As Boolean)
        FieldName = myFieldName
        DataType = myDataType
        HasIndex = myHasIndex
    End Sub

End Class