Imports STOCHLIB.General
Imports GemBox.Spreadsheet

Public Class clsAreaStats

    Friend ID As String
    Friend BAGArea As Double

    Friend SbkLandUse As New clsSbkLandUseList
    Friend LGNAreas As New Dictionary(Of Integer, Double) 'key = landuse number, value = area

    Friend WP As Double
    Friend ZP As Double

    Friend minBAGElevation As Double = 9999999
    Friend maxBAGElevation As Double = -9999999

    Friend minElevation As Double = 99999999
    Friend maxElevation As Double = -99999999

    Friend SCurve(100) As Integer

    Private Setup As clsSetup
    Private Area As clsSubcatchment

    Public Sub New(ByRef mySetup As clsSetup, ByRef myArea As clsSubcatchment)
        Setup = mySetup
        Area = myArea
    End Sub

    Public Sub SbkLandUse2Excel(ByRef mySheet As clsExcelSheet, ByVal row As Long, ByVal lastCol As Long, ByVal writeHeader As Boolean)
        Dim i As Integer
        For i = 1 To SbkLandUse.SbkLandUseList.Count
            mySheet.ws.Cells(row, lastCol + i).Value = SbkLandUse.getArea(i)
        Next

        If writeHeader Then
            For i = 1 To SbkLandUse.SbkLandUseList.Count
                mySheet.ws.Cells(0, lastCol + i).Value = SbkLandUse.getName(i)
            Next
        End If

    End Sub

    Public Sub AddLGNCode(ByVal LGNCode As Integer, ByVal Area As Double)
        If LGNAreas.ContainsKey(LGNCode) Then
            LGNAreas.Item(LGNCode) += Area
        Else
            LGNAreas.Add(LGNCode, Area)
        End If
    End Sub

End Class
