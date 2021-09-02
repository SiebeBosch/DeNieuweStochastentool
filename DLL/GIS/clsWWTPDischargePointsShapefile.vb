Imports System.Windows.Forms
Imports STOCHLIB.General

''' <summary>
''' Author: Siebe Bosch
''' Date: 31-03-2013
''' Description: this class contains all data concerning Waste Water Treatment Plants in our study area
''' </summary>
''' <remarks></remarks>

Public Class clsWWTPDischargePointsShapefile
    Friend Path As String
    Friend sf As New MapWinGIS.Shapefile
    Friend InUse As Boolean

    Friend IdField As String
    Public IdFieldIdx As Integer = -1

    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Public Sub SetIDField(ByVal FieldName As String)
        IdField = FieldName
        IdFieldIdx = setup.GISData.getShapeFieldIdxFromFileName(Path, FieldName)
    End Sub

    Public Sub SetPath(ByVal myPath As String)
        Path = myPath
    End Sub


End Class
