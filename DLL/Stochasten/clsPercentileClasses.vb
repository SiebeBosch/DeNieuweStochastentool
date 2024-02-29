Imports System.Windows.Forms
Imports STOCHLIB.General
Public Class clsPercentileClasses
    Public Classes As New Dictionary(Of String, clsPercentileClass) 'list of percentile classes
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        'constructor
        Setup = mySetup
        Classes = New Dictionary(Of String, clsPercentileClass)
    End Sub

    Public Function AddClass(ByRef PercentileClass As clsPercentileClass) As Boolean
        'adds a class to the list
        Classes.Add(PercentileClass.Name, PercentileClass)
        Return True
    End Function

    Public Function ReadFromDataGrid(ByRef myDataGrid As DataGridView) As Boolean
        Try
            Classes = New Dictionary(Of String, clsPercentileClass)
            Dim PercentileClass As clsPercentileClass

            'now walk through all rows of the classification datagridview to decide which percentile we'll use
            For Each myRow As DataGridViewRow In myDataGrid.Rows
                PercentileClass = New clsPercentileClass
                'determine the boundaries of the current groundwater class
                PercentileClass.Name = myRow.Cells(0).Value
                PercentileClass.LBoundPercentile = myRow.Cells(1).Value
                PercentileClass.UboundPercentile = myRow.Cells(2).Value
                PercentileClass.RepresentativePercentile = (PercentileClass.LBoundPercentile + PercentileClass.UboundPercentile) / 2
                Classes.Add(PercentileClass.Name, PercentileClass)
            Next
            Return True
        Catch ex As Exception
            Me.setup.log.AddError("Error reading percentile classes from datagridview: " & ex.Message)
            Return False
        End Try

    End Function


End Class
