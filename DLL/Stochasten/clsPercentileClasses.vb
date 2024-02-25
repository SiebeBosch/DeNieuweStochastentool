Imports System.Windows.Forms

Public Class clsPercentileClasses
    Public Classes As New List(Of clsPercentileClass) 'list of percentile classes

    Public Sub New()
        'constructor
        Classes = New List(Of clsPercentileClass)
    End Sub

    Public Function AddClass(ByRef PercentileClass As clsPercentileClass) As Boolean
        'adds a class to the list
        Classes.Add(PercentileClass)
        Return True
    End Function

    Public Function ReadFromDataGrid(ByRef myDataGrid As DataGridView) As List(Of clsPercentileClass)
        Dim Classes As New List(Of clsPercentileClass)
        Dim PercentileClass As clsPercentileClass
        Dim lPercentile As Double, uPercentile As Double, repPerc As Double

        'now walk through all rows of the classification datagridview to decide which percentile we'll use
        For Each myRow As DataGridViewRow In myDataGrid.Rows
            PercentileClass = New clsPercentileClass
            'determine the boundaries of the current groundwater class
            lPercentile = myRow.Cells(1).Value
            uPercentile = myRow.Cells(2).Value
            repPerc = (lPercentile + uPercentile) / 2
            Classes.Add(PercentileClass)
        Next
        Return Classes
    End Function


End Class
