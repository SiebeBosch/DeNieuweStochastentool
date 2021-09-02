Option Explicit On

Imports System.IO
Imports STOCHLIB.General
Imports GemBox.Spreadsheet

Public Class clsAreas
    Friend Subcatchments As New Dictionary(Of String, clsSubcatchment)
    'Friend MeteoStations As New clsMeteoStations
    Private Setup As clsSetup

    Public Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Friend Function getAdd(ByVal myID As String) As clsSubcatchment
        Dim mySubcatchment As clsSubcatchment
        If Not Subcatchments.ContainsKey(myID.Trim.ToUpper) Then
            mySubcatchment = New clsSubcatchment(Me.Setup)
            mySubcatchment.ID = myID
            Subcatchments.Add(myID.Trim.ToUpper, mySubcatchment)
            Return mySubcatchment
        Else
            Return Subcatchments.Item(myID.Trim.ToUpper)
        End If
    End Function

    Public Function PopulateFromShapeFile()
        Dim i As Long
        Dim mySubcatchment As clsSubcatchment
        Try
            If Not Me.Setup.GISData.SubcatchmentDataSource.IDsUnique Then Throw New Exception("Error: area ID's in shapefile must be unique!")
            If Me.Setup.GISData.SubcatchmentDataSource.Open() Then
                For i = 0 To Me.Setup.GISData.SubcatchmentDataSource.Shapefile.sf.NumShapes - 1
                    mySubcatchment = New clsSubcatchment(Me.Setup)
                    mySubcatchment.ID = Me.Setup.GISData.SubcatchmentDataSource.GetTextValue(i, GeneralFunctions.enmInternalVariable.ID)
                    Subcatchments.Add(mySubcatchment.ID, mySubcatchment)
                Next
            End If
            Return True
        Catch ex As Exception
            Me.Setup.Log.AddError(ex.Message)
            Me.Setup.Log.AddError("Error in function PopulateFromShapeFile of class clsAreas")
            Return False
        End Try
    End Function

    Public Function calcAreaSum()
        Dim mySum As Double = 0
        Dim mySubcatchment As clsSubcatchment
        Dim i As Integer
        For i = 0 To Subcatchments.Values.Count - 1
            mySubcatchment = Subcatchments.Values(i)
            mySum += mySubcatchment.TotalAttributeArea
        Next
        Return mySum
    End Function

    'Public Sub calcStatistics(ByVal nDigits As Integer, ByVal Elevation As Boolean, ByVal LGN As Boolean, ByVal BAG As Boolean)
    '    'calculates the geo-statistics (elevation distribution, landuse etc) for each area in the collection
    '    Try
    '        If LGN Then Call calcLGNStats()
    '    Catch ex As Exception
    '        Me.Setup.Log.AddError("Error in sub calcStatistics of class clsAreas.")
    '    End Try
    'End Sub

    'Public Sub calcLGNStats()
    '    Dim i As Long
    '    Me.Setup.GeneralFunctions.UpdateProgressBar("Calculating landuse statistics...", 0, 1)
    '    If Not Me.Setup.GISData.LanduseShapeFile.Read Then Throw New Exception("Could not read landuse shapefile.")
    '    For Each mySubcatchment As clsSubcatchment In Subcatchments.Values
    '        i += 1
    '        Me.Setup.GeneralFunctions.UpdateProgressBar("Calculating landuse statistics...", i, Subcatchments.Count)
    '        mySubcatchment.Stats.calcLGN()
    '    Next
    '    Me.Setup.GISData.LanduseShapeFile.Close()
    'End Sub

End Class
