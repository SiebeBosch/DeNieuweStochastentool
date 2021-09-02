Option Explicit On

Imports STOCHLIB.General

''' <summary>
''' Geen constructor nodig
''' </summary>
''' <remarks></remarks>
Public Class clsSbkVectorPoint
    Public X As Double
    Public Y As Double
    Public Dist As Double
    Public Angle As Double
    Public ID As String       'ID van het vectorpunt, gebaseerd op takID en indexnummer
    Public Idx As Long        'indexnummer van het vectorpunt op de tak
    Public ReachID As String  'ID van de tak waarvan dit punt onderdeel uitmaakt
    Public VectorID As String 'voor export naar pivot tables en Tableau is een vectorID handig
    Public nt As STOCHLIB.GeneralFunctions.enmNodetype

    Public WP As Double         'winter target level for postprocessing purposes
    Public ZP As Double         'summer target level for postprocessing purposes
    Public InUse As Boolean     'inuse

    Public upCP As clsSbkReachObject  'dichtstbijzijnde bovenstrooms gelegen rekenpunt
    Public downCP As clsSbkReachObject 'dichtstbijzijnde benedenstoroms gelegen rekenpunt

    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

    Public Sub New(ByRef mySetup As clsSetup, ByVal myID As String, ByVal myX As Double, ByVal myY As Double)
        Setup = mySetup
        X = myX
        Y = myY
        ID = myID
    End Sub

    Public Function Duplicate(ByVal newReachID As String) As clsSbkVectorPoint
        Dim newPoint As New clsSbkVectorPoint(Me.Setup)
        newPoint.X = X
        newPoint.Y = Y
        newPoint.Dist = Dist
        newPoint.Angle = Angle
        newPoint.ID = ID
        newPoint.Idx = Idx
        newPoint.ReachID = newReachID

        newPoint.VectorID = VectorID
        newPoint.upCP = upCP
        newPoint.downCP = downCP
        Return newPoint
    End Function

    'schrijf de topologie van dit vectorpunt weg naar het Excel-werkblad
    Public Sub toWorkSheet(ByRef TopoSheet As clsExcelSheet, ByVal row As Long)
        Dim Lat As Double, Lon As Double
        Call Me.Setup.GeneralFunctions.RD2WGS84(X, Y, Lat, Lon)
        TopoSheet.ws.Cells(row, 0).Value = ID
        TopoSheet.ws.Cells(row, 1).Value = nt
        TopoSheet.ws.Cells(row, 2).Value = ReachID
        TopoSheet.ws.Cells(row, 3).Value = VectorID
        TopoSheet.ws.Cells(row, 4).Value = Idx
        TopoSheet.ws.Cells(row, 5).Value = Dist
        TopoSheet.ws.Cells(row, 6).Value = X
        TopoSheet.ws.Cells(row, 7).Value = Y
        TopoSheet.ws.Cells(row, 8).Value = Lat
        TopoSheet.ws.Cells(row, 9).Value = Lon
    End Sub
End Class
