Imports STOCHLIB.General
Imports System.Data.SQLite

Public Class frmPatroon
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup, ByVal Duration As Integer)


        'Dim Series As ChartSeries
        Dim i As Integer, j As Integer
        Dim newRow As DataRow

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Setup = mySetup

        'chtPatroon.Series.Clear()
        'chtPatroon.Legends(0).Position = ChartDock.Right

        Dim dtPat As New DataTable
        dtPat.Columns.Add(New DataColumn("UUR", Type.GetType("System.Int32")))
        dtPat.Columns.Add(New DataColumn("HOOG", Type.GetType("System.String")))
        dtPat.Columns.Add(New DataColumn("MIDDELHOOG", Type.GetType("System.String")))
        dtPat.Columns.Add(New DataColumn("MIDDELLAAG", Type.GetType("System.String")))
        dtPat.Columns.Add(New DataColumn("LAAG", Type.GetType("System.String")))
        dtPat.Columns.Add(New DataColumn("KORT", Type.GetType("System.String")))
        dtPat.Columns.Add(New DataColumn("LANG", Type.GetType("System.String")))
        dtPat.Columns.Add(New DataColumn("UNIFORM", Type.GetType("System.String")))

        For i = 1 To Duration
            newRow = dtPat.NewRow()
            newRow(0) = i
        Next

        For j = 1 To 7
            Dim dt As New DataTable
            'Series = New ChartSeries
            'Series.Type = ChartSeriesType.Line

            Select Case j
                Case Is = 1
                    'Series.Name = ("HOOG")
                    'Series.Text = Series.Name
                    Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT FRACTIE FROM NEERSLAGVERLOOP WHERE DUUR=" & Duration & " AND PATROON='" & "HOOG';", dt, False)
                Case Is = 2
                    'Series.Name = ("MIDDELHOOG")
                    'Series.Text = Series.Name
                    Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT FRACTIE FROM NEERSLAGVERLOOP WHERE DUUR=" & Duration & " AND PATROON='" & "MIDDELHOOG';", dt, False)
                Case Is = 3
                    'Series.Name = ("MIDDELLAAG")
                    'Series.Text = Series.Name
                    Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT FRACTIE FROM NEERSLAGVERLOOP WHERE DUUR=" & Duration & " AND PATROON='" & "MIDDELLAAG';", dt, False)
                Case Is = 4
                    'Series.Name = ("LAAG")
                    'Series.Text = Series.Name
                    Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT FRACTIE FROM NEERSLAGVERLOOP WHERE DUUR=" & Duration & " AND PATROON='" & "LAAG';", dt, False)
                Case Is = 5
                    'Series.Name = ("KORT")
                    'Series.Text = Series.Name
                    Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT FRACTIE FROM NEERSLAGVERLOOP WHERE DUUR=" & Duration & " AND PATROON='" & "KORT';", dt, False)
                Case Is = 6
                    'Series.Name = ("LANG")
                    'Series.Text = Series.Name
                    Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT FRACTIE FROM NEERSLAGVERLOOP WHERE DUUR=" & Duration & " AND PATROON='" & "LANG';", dt, False)
                Case Is = 7
                    'Series.Name = ("UNIFORM")
                    'Series.Text = Series.Name
                    Setup.GeneralFunctions.SQLiteQuery(Me.Setup.SqliteCon, "SELECT FRACTIE FROM NEERSLAGVERLOOP WHERE DUUR=" & Duration & " AND PATROON='" & "UNIFORM';", dt, True)
            End Select

            If dt.Rows.Count >= Duration Then
                For i = 1 To Duration
                    'Series.Points.Add(i, dt.Rows(i - 1)(0))
                Next
                'chtPatroon.Series.Add(Series)
            Else
                'Me.Setup.Log.AddError("Error populating patterns for duration " & Duration & " and series " & Series.Name & ". Insufficient number of temporal records present in database.")
            End If
        Next


    End Sub
End Class