Imports GemBox.Spreadsheet
Imports STOCHLIB.General

Public Class clsExcelBook
    Public Path As String
    Public Sheets As Collection 'of clsExcelBook
    Dim oExcel As ExcelFile
    Private Setup As clsSetup


    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup

        ' TODO: If using GemBox.Spreadsheet Professional, put your serial key below.
        ' Otherwise, if you are using GemBox.Spreadsheet Free, comment out the 
        ' following line (Free version doesn't have SetLicense method). 
        'Dim LicensePath As String = System.Windows.Forms.Application.StartupPath + "\licenses\gembox.txt"
        Dim LicensePath As String

        If Debugger.IsAttached Then
            'in debug mode we will retrieve the zip file from our GITHUB directory
            LicensePath = "d:\GITHUB\DeNieuweStochastentool\licenses\gembox.txt"
            Console.WriteLine("Path to external licenses set to: " & LicensePath)
        Else
            'in release mode we will retrieve the zip file from within our application directory
            LicensePath = My.Application.Info.DirectoryPath & "\licenses\gembox.txt"
            Console.WriteLine("Path to external licenses set to: " & LicensePath)
        End If

        If System.IO.File.Exists(LicensePath) Then
            Using licenseReader As New System.IO.StreamReader(LicensePath)
                Dim myLicense As String = licenseReader.ReadToEnd
                SpreadsheetInfo.SetLicense(myLicense)
            End Using
        Else
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY")
            MsgBox("No license detected for Gembox Spreadsheet: please write your key in a text file: " & LicensePath)
            Me.Setup.Log.AddError("No license detected for Gembox Spreadsheet: please write your key in a text file: " & LicensePath)
            Console.WriteLine("No license detected for Gembox Spreadsheet: please write your key in a text file here: " & LicensePath)
        End If

        oExcel = New ExcelFile
        Sheets = New Collection
    End Sub

    Public Sub New(myPath As String)
        ' TODO: If using GemBox.Spreadsheet Professional, put your serial key below.
        ' Otherwise, if you are using GemBox.Spreadsheet Free, comment out the 
        ' following line (Free version doesn't have SetLicense method). 
        Dim LicensePath As String = System.Windows.Forms.Application.StartupPath + "\licenses\gembox.txt"
        Path = myPath

        If System.IO.File.Exists(LicensePath) Then
            Using licenseReader As New System.IO.StreamReader(LicensePath)
                Dim myLicense As String = licenseReader.ReadToEnd
                SpreadsheetInfo.SetLicense(myLicense)
            End Using
        ElseIf System.IO.File.Exists("d:\GITHUB\DeNieuweStochastentool\licenses\gembox.txt") Then
            Using licenseReader As New System.IO.StreamReader("d:\GITHUB\DeNieuweStochastentool\licenses\gembox.txt")
                Dim myLicense As String = licenseReader.ReadToEnd
                SpreadsheetInfo.SetLicense(myLicense)
            End Using
        Else
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY")
            Me.Setup.Log.AddError("No license detected for Gembox Spreadsheet: please write your key in a text file: " & LicensePath)
        End If

        oExcel = New ExcelFile
        Sheets = New Collection
    End Sub

    Public Sub Initialize(myPath As String)

        Dim LicensePath As String = System.Windows.Forms.Application.StartupPath + "\licenses\gembox.txt"
        Path = myPath

        If System.IO.File.Exists(LicensePath) Then
            Using licenseReader As New System.IO.StreamReader(LicensePath)
                Dim myLicense As String = licenseReader.ReadToEnd
                SpreadsheetInfo.SetLicense(myLicense)
            End Using
        ElseIf System.IO.File.Exists("d:\GITHUB\DeNieuweStochastentool\licenses\gembox.txt") Then
            Using licenseReader As New System.IO.StreamReader("d:\GITHUB\DeNieuweStochastentool\licenses\gembox.txt")
                Dim myLicense As String = licenseReader.ReadToEnd
                SpreadsheetInfo.SetLicense(myLicense)
            End Using
        Else
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY")
            Me.Setup.Log.AddError("No license detected for Gembox Spreadsheet: please write your key in a text file: " & LicensePath)
        End If

        oExcel = New ExcelFile
        Sheets = New Collection
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myPath As String)

        Dim LicensePath As String = System.Windows.Forms.Application.StartupPath + "\licenses\gembox.txt"
        Path = myPath
        Setup = mySetup

        If System.IO.File.Exists(LicensePath) Then
            Using licenseReader As New System.IO.StreamReader(LicensePath)
                Dim myLicense As String = licenseReader.ReadToEnd
                SpreadsheetInfo.SetLicense(myLicense)
            End Using
        ElseIf System.IO.File.Exists("d:\GITHUB\DeNieuweStochastentool\licenses\gembox.txt") Then
            Using licenseReader As New System.IO.StreamReader("d:\GITHUB\DeNieuweStochastentool\licenses\gembox.txt")
                Dim myLicense As String = licenseReader.ReadToEnd
                SpreadsheetInfo.SetLicense(myLicense)
            End Using
        Else
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY")
            Me.Setup.Log.AddError("No license detected for Gembox Spreadsheet: please write your key in a text file: " & LicensePath)
        End If

        oExcel = New ExcelFile
        Sheets = New Collection

    End Sub

    Public Function GetSheet(SheetIdx As Integer) As clsExcelSheet
        Dim mySheet As New clsExcelSheet(Me.oExcel, "TEST", Setup) ' oExcel.Worksheets(SheetIdx).Name)
        mySheet.ws = oExcel.Worksheets(SheetIdx)
        Return mySheet
    End Function


    Public Function GetSheetByName(SheetName As String) As ExcelWorksheet
        For i = 0 To oExcel.Worksheets.Count - 1
            If oExcel.Worksheets.Item(i).Name.Trim.ToUpper = SheetName.Trim.ToUpper Then
                Return oExcel.Worksheets.Item(i)
            End If
        Next
        Return Nothing
    End Function

    Public Function GetAddSheet(ByVal myName As String) As clsExcelSheet
        If Not Sheets.Contains(myName.Trim.ToUpper) Then
            Dim mySheet As New clsExcelSheet(Me.oExcel, myName, Setup)
            Sheets.Add(mySheet, myName.Trim.ToUpper)
            Return mySheet
        Else
            Return Sheets(myName.Trim.ToUpper)
        End If
    End Function

    Public Function GetFieldIdx(SheetName As String, FieldName As String, LastIndexPrimaryDataSource As Integer) As Integer
        '!!!!note: the first column does not count since it should match the ID field of the primary datasource!!!!
        Dim mySheet As ExcelWorksheet = GetSheetByName(SheetName)
        Dim c As Integer = 0
        If Not mySheet Is Nothing Then
            While Not mySheet.Cells(0, c + 1).Value = ""
                c += 1
                If mySheet.Cells(0, c).Value.ToString.Trim.ToUpper = FieldName.Trim.ToUpper Then
                    Return LastIndexPrimaryDataSource + c
                End If
            End While
            Return -1
        Else
            Return -1
        End If
    End Function
    Public Function GetTextValue(SheetName As String, ID As String, ColIdx As Integer) As String
        Dim mySheet As ExcelWorksheet = GetSheetByName(SheetName)
        Dim r As Integer = -1
        If Not mySheet Is Nothing Then
            While Not mySheet.Cells(r + 1, 0).Value = ""
                r += 1
                If mySheet.Cells(r, 0).Value.ToString.Trim.ToUpper = ID.Trim.ToUpper Then
                    Return mySheet.Cells(r, ColIdx).Value
                End If
            End While
            Return ""
        Else
            Return ""
        End If
    End Function

    Public Function GetNumericalValue(SheetName As String, ID As String, FieldIdx As Integer, PrimaryDataSourceLastFieldIdx As Integer) As Double
        Dim mySheet As ExcelWorksheet = GetSheetByName(SheetName)
        Dim r As Integer = -1
        Dim ColNum As Integer = FieldIdx - PrimaryDataSourceLastFieldIdx 'since the first column does not count (it contains the ID), we use colNUM here instead of colidx
        If Not mySheet Is Nothing Then
            While Not mySheet.Cells(r + 1, 0).Value = ""
                r += 1
                If mySheet.Cells(r, 0).Value.ToString.Trim.ToUpper = ID.Trim.ToUpper Then
                    Return mySheet.Cells(r, ColNum).Value
                End If
            End While
            Return Double.NaN
        Else
            Return Double.NaN
        End If
    End Function

    Public Sub Save(Optional ByVal Show As Boolean = True)
        If oExcel.Worksheets.Count > 0 Then
            oExcel.Save(Path, SaveOptions.XlsxDefault)
            If Show Then TryToDisplayGeneratedFile(Path)
        Else
            MsgBox("Error writing Excel file: contains no worksheets.")
        End If
    End Sub

    Public Function Read() As Boolean
        Try
            oExcel = ExcelFile.Load(Path)
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError("Error in function Read of clsExcelBook while reading " & Path & ": " & ex.Message)
            Return False
        End Try
    End Function

    Sub TryToDisplayGeneratedFile(ByVal fileName As String)
        Try
            System.Diagnostics.Process.Start(fileName)
        Catch
            Console.WriteLine(fileName + " created in application folder.")
        End Try
    End Sub

End Class
