Option Explicit On
Imports STOCHLIB.General

Public Class clsChannelCategory
    Public ID As String
    Public WP As Double
    Public InUse As Boolean
    'Public IsStructure As Boolean   'whether or not this reach is in fact a long structure
    Public Depth As Double
    Public BedWidth As Double
    Public Slope As Double
    Public Length As Double         'for situations where the channel shapefile is a line shapefile
    Public Area As Double           'for situations where the channel shapefile is a polygon shapefile
    Public Perimeter As Double      'for situations where the channel shapefile is a polygon shapefile
    Public ChannelUsage As GeneralFunctions.enmChannelUsage

    Friend Table As clsSobekTable
    Private setup As clsSetup


    Public Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub calcStorageTable()
        'deze routine bouwt een tabel voor openwaterberging
        Table = New clsSobekTable(setup)

        'add storage based on polygons
        If Area > 0 AndAlso Perimeter > 0 Then
            Table.AddDataPair(2, WP - Depth, Area)
            Table.AddDataPair(2, WP, Area)
            Table.AddDataPair(2, WP + 2, Area + 2 * Slope * Perimeter)
        End If

        'add storage based on polylines
        If Length > 0 Then
            Table.AddDataPair(2, WP - Depth, BedWidth * Length)                    'bed level
            Table.AddDataPair(2, WP, BedWidth * Length)                            'winter target level
            Table.AddDataPair(2, WP + 2, (BedWidth + (2 * Slope * 2)) * Length)    'slope
        End If


    End Sub
    Friend Sub addLength(ByVal myLength As Double)
        Length += myLength
    End Sub

    Friend Sub addPerimeter(ByVal myPerimeter As Double)
        Perimeter += myPerimeter
    End Sub

    Friend Sub addArea(ByVal myArea As Double)
        Area += myArea
    End Sub

End Class
