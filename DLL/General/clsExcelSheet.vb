Imports GemBox.Spreadsheet
Imports STOCHLIB.General

Public Class clsExcelSheet
    Public SheetName As String
    Public ws As ExcelWorksheet

    Public lastRowUsed As Long 'keeps track of the last row that has been used
    Private Setup As clsSetup

    Public Sub New(ByRef wb As ExcelFile, ByVal Name As String, ByRef mySetup As clsSetup)

        'make sure our worksheet name is valid and unique
        Name = Replace(Name, "|", "_")
        Name = Replace(Name, ":", "_")
        Name = Replace(Name, "[", "(")
        Name = Replace(Name, "]", ")")
        Name = Replace(Name, "/", "div")
        If Name.Length > 31 Then
            Dim Names As New List(Of String)
            For Each ws In wb.Worksheets
                Names.Add(ws.Name)
            Next
            Name = mySetup.GeneralFunctions.MakeUniqueID(Names, Name, 31, False)
        End If



        'add the worksheet
        ws = wb.Worksheets.Add(Name)
    End Sub

    Public Sub Write(ByRef Data(,) As Object)
        Dim r As Long, c As Long
        For r = LBound(Data, 1) To UBound(Data, 1)
            For c = LBound(Data, 2) To UBound(Data, 2)
                ws.Cells(r, c).Value = Data(r, c)
            Next
        Next
    End Sub

    Public Sub AutoSizeColumns()
        Dim c As Integer = -1
        While Not ws.Cells(0, c + 1).Value = ""
            c += 1
            ws.Columns(c).AutoFit(1, ws.Rows(0), ws.Rows(ws.Rows.Count - 1))
        End While
    End Sub


    Public Sub WriteYZProfilesHeader()
        'writes the header for YZ cross section data in the current worksheet
        ws.Cells(0, 0).Value = "ID"
        ws.Cells(0, 1).Value = "X"
        ws.Cells(0, 2).Value = "Y"
        ws.Cells(0, 3).Value = "Dist"
        ws.Cells(0, 4).Value = "Elev"
        ws.Cells(0, 5).Value = "v.shift"
        lastRowUsed = 0
    End Sub

    Public Sub WriteYZProfileData(ByVal ID As String, ByVal X As Double, ByVal Y As Double, ByRef myTable As clsSobekTable, VerticalShift As Double)
        Dim i As Long
        For i = 0 To myTable.XValues.Values.Count - 1
            lastRowUsed += 1
            ws.Cells(lastRowUsed, 0).Value = ID
            ws.Cells(lastRowUsed, 1).Value = X
            ws.Cells(lastRowUsed, 2).Value = Y
            ws.Cells(lastRowUsed, 3).Value = myTable.XValues.Values(i)
            ws.Cells(lastRowUsed, 4).Value = myTable.Values1.Values(i)
            ws.Cells(lastRowUsed, 5).Value = VerticalShift
        Next
    End Sub

End Class
