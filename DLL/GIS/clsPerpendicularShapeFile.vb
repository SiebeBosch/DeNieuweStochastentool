
Imports System.Windows.Forms
Imports STOCHLIB.General

Public Class clsPerpendicularShapeFile
  Friend Path As String
  Public sf As New MapWinGIS.Shapefile
  Friend InUse As Boolean

  Friend IDField As String
  Friend IDFieldIdx As Integer = -1
  Friend SideField As String
  Friend SideFieldIdx As Integer = -1
  Friend MaxElevationField As String
  Friend MaxElevationFieldIdx As Integer = -1
  Friend XField As String
  Friend XFieldIdx As Integer = -1
  Friend YField As String
  Friend YFieldIdx As Integer = -1

  Private setup As clsSetup

  Public Sub New(ByRef mySetup As clsSetup)
    Me.setup = mySetup
  End Sub

  Public Function CreateNew(ByVal mypath As String) As Boolean
    Path = mypath
    IDField = "ID"
    SideField = "ZIJDE"
    MaxElevationField = "MaxLevel"
    XField = "X"
    YField = "Y"

    If System.IO.File.Exists(mypath) Then
      Me.setup.GeneralFunctions.deleteShapeFile(mypath)
    End If

    If Not sf.CreateNew(Path, MapWinGIS.ShpfileType.SHP_POLYLINE) Then Return False
    If Not MakeIDField(IDField) Then Return False
    If Not MakeSideField(SideField) Then Return False
    If Not MakeMaxElevationField(MaxElevationField) Then Return False
    If Not makeXYFields(XField, YField) Then Return False

    Return True

  End Function

  Public Function getID(ByVal Idx As Long) As String
    Return sf.CellValue(IDFieldIdx, Idx)
  End Function
  Public Function getXOrigin(ByVal Idx As Long) As Double
    Return sf.CellValue(XFieldIdx, Idx)
  End Function
  Public Function getYOrigin(ByVal Idx As Long) As Double
    Return sf.CellValue(YFieldIdx, Idx)
  End Function
  Public Function getSide(ByVal Idx As Long) As String
    Return sf.CellValue(SideFieldIdx, Idx)
  End Function

  Public Function Open() As Boolean
    If Not sf.Open(Path) Then
      Return False
    Else
      Return True
    End If
  End Function

  Public Sub close()
    sf.Close()
  End Sub

  Public Function BuildFromReachObjectsDoubleVersion(ByVal Profiles As Boolean, ByVal CalcPoints As Boolean, ByVal Radius As Integer, ByRef myCase As clsSobekCase) As Boolean
    Dim myShape As MapWinGIS.Shape, myPoint As MapWinGIS.Point
    Dim ShapeIdx As Integer
    Dim myAngle As Double

    Try
      For Each myReach As clsSbkReach In myCase.CFTopo.Reaches.Reaches.Values
        For Each myObj As clsSbkReachObject In myReach.ReachObjects.ReachObjects.Values
          If (Profiles AndAlso myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE) OrElse (CalcPoints AndAlso (myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpoint OrElse myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.NodeCFGridpointFixed)) Then
            myObj.calcXY()
            myObj.calcAngle()

            'Left
            myShape = New MapWinGIS.Shape
            myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)
            myPoint = New MapWinGIS.Point
            myPoint.x = myObj.X
            myPoint.y = myObj.Y
            myShape.InsertPoint(myPoint, 0)

            myAngle = Me.setup.GeneralFunctions.D2R(Me.setup.GeneralFunctions.NormalizeAngle(myObj.Angle - 90)) 'deduct 90 degrees, normalize and convert to radials
            myPoint = New MapWinGIS.Point
            myPoint.x = myObj.X + Radius * Math.Sin(myAngle)
            myPoint.y = myObj.Y + Radius * Math.Cos(myAngle)
            myShape.InsertPoint(myPoint, 1)

            ShapeIdx = sf.EditAddShape(myShape)
            sf.EditCellValue(IDFieldIdx, ShapeIdx, myObj.ID)
            sf.EditCellValue(SideFieldIdx, ShapeIdx, "Links")

            'right
            myShape = New MapWinGIS.Shape
            myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)
            myPoint = New MapWinGIS.Point
            myPoint.x = myObj.X
            myPoint.y = myObj.Y
            myShape.InsertPoint(myPoint, 0)

            myAngle = Me.setup.GeneralFunctions.D2R(Me.setup.GeneralFunctions.NormalizeAngle(myObj.Angle + 90)) 'add 90 degrees, normalize and convert to radials
            myPoint = New MapWinGIS.Point
            myPoint.x = myObj.X + Radius * Math.Sin(myAngle)
            myPoint.y = myObj.Y + Radius * Math.Cos(myAngle)
            myShape.InsertPoint(myPoint, 1)

            ShapeIdx = sf.EditAddShape(myShape)
            sf.EditCellValue(IDFieldIdx, ShapeIdx, myObj.ID)
            sf.EditCellValue(SideFieldIdx, ShapeIdx, "Rechts")
          End If
        Next
      Next

      sf.StopEditingTable()
      sf.StopEditingShapes()
      close()
      Return True
    Catch ex As Exception
      Me.setup.Log.AddError(ex.Message)
      Me.setup.Log.AddError("Error in function BuildPerpendicularShapeFile")
      Return False
    End Try

  End Function

  Public Function buildAccrossFromProfiles(ByVal myCase As clsSobekCase, ByVal SearchRadius As Double)
    Dim myReach As clsSbkReach, myObj As clsSbkReachObject
    Dim myShape As MapWinGIS.Shape, myPoint As MapWinGIS.Point, ShapeIdx As Long
    Dim myAngle As Double

    Try
      For Each myReach In myCase.CFTopo.Reaches.Reaches.Values
        For Each myObj In myReach.ReachObjects.ReachObjects.Values
          If myObj.nt = STOCHLIB.GeneralFunctions.enmNodetype.SBK_PROFILE Then
            myObj.calcAngle() 'calculate the angle for this profile
            myObj.calcXY()    'calculate the xy-coordinate for this profile
            myShape = New MapWinGIS.Shape
            myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)

            myAngle = Me.setup.GeneralFunctions.D2R(Me.setup.GeneralFunctions.NormalizeAngle(myObj.Angle + 90)) 'add 90 degrees, normalize and convert to radials
            myPoint = New MapWinGIS.Point
            myPoint.x = myObj.X + SearchRadius * Math.Sin(myAngle)
            myPoint.y = myObj.Y + SearchRadius * Math.Cos(myAngle)
            myShape.InsertPoint(myPoint, 1)

            myAngle = Me.setup.GeneralFunctions.D2R(Me.setup.GeneralFunctions.NormalizeAngle(myObj.Angle - 90)) 'deduct 90 degrees, normalize and convert to radials
            myPoint = New MapWinGIS.Point
            myPoint.x = myObj.X + SearchRadius * Math.Sin(myAngle)
            myPoint.y = myObj.Y + SearchRadius * Math.Cos(myAngle)
            myShape.InsertPoint(myPoint, 1)

            'add the shape to the shapefile and set the cell values
            ShapeIdx = sf.EditAddShape(myShape)
            sf.EditCellValue(IDFieldIdx, ShapeIdx, myObj.ID)  'set profile ID
            sf.EditCellValue(SideFieldIdx, ShapeIdx, "Beide")
            sf.EditCellValue(XFieldIdx, ShapeIdx, myObj.X)
            sf.EditCellValue(YFieldIdx, ShapeIdx, myObj.Y)
          End If
        Next
      Next

      sf.StopEditingShapes()
      sf.StopEditingTable()
      sf.Close()
      Return True

    Catch ex As Exception
      Me.setup.Log.AddError(ex.Message)
      Me.setup.Log.AddError("Error in sub buildFromProfilesSingleVersion")
      Return False
    End Try

  End Function

  Public Function buildAccrossFromCalcPoints(ByVal myCase As clsSobekCase, ByVal SearchRadius As Double)
    'this function builds a shapefile with perpendicular lines to each calculation point in the currently active schematization
    'notice that it uses the calculation points stored at clsCFTopo level, so all in one go
    Dim myShape As MapWinGIS.Shape, myPoint As MapWinGIS.Point, ShapeIdx As Long
    Dim myAngle As Double


    Try
      If Not sf.Open(Path) Then Throw New Exception("Could not open generated shapefile containing perpendicular lines per calulation point.")
      If Not sf.StartEditingShapes(True) Then Throw New Exception("Could not start editing table of generated shapefile containing perpendicular lines per calculation point.")

            For Each myObj As clsSbkVectorPoint In myCase.CFTopo.WaterLevelPoints.Values

                myShape = New MapWinGIS.Shape
                myShape.Create(MapWinGIS.ShpfileType.SHP_POLYLINE)

                myAngle = Me.setup.GeneralFunctions.D2R(Me.setup.GeneralFunctions.NormalizeAngle(myObj.Angle + 90)) 'add 90 degrees, normalize and convert to radials
                myPoint = New MapWinGIS.Point
                myPoint.x = myObj.X + SearchRadius * Math.Sin(myAngle)
                myPoint.y = myObj.Y + SearchRadius * Math.Cos(myAngle)
                myShape.InsertPoint(myPoint, 1)

                myAngle = Me.setup.GeneralFunctions.D2R(Me.setup.GeneralFunctions.NormalizeAngle(myObj.Angle - 90)) 'deduct 90 degrees, normalize and convert to radials
                myPoint = New MapWinGIS.Point
                myPoint.x = myObj.X + SearchRadius * Math.Sin(myAngle)
                myPoint.y = myObj.Y + SearchRadius * Math.Cos(myAngle)
                myShape.InsertPoint(myPoint, 1)

                'add the shape to the shapefile and set the cell values
                ShapeIdx = sf.EditAddShape(myShape)
                sf.EditCellValue(IDFieldIdx, ShapeIdx, myObj.ID)  'set profile ID
                sf.EditCellValue(SideFieldIdx, ShapeIdx, "Beide")
                sf.EditCellValue(XFieldIdx, ShapeIdx, myObj.X)
                sf.EditCellValue(YFieldIdx, ShapeIdx, myObj.Y)
            Next

            sf.StopEditingShapes()
      sf.StopEditingTable()
      sf.Close()
      Return True

    Catch ex As Exception
      Me.setup.Log.AddError(ex.Message)
      Me.setup.Log.AddError("Error in sub buildFromProfilesSingleVersion")
      Return False
    End Try

  End Function

  Public Function MakeIDField(ByVal FieldName As String) As Boolean
    IDField = FieldName
    IDFieldIdx = sf.EditAddField(FieldName, MapWinGIS.FieldType.STRING_FIELD, 20, 20)
    Return True
  End Function

  Public Function MakeSideField(ByVal FieldName As String) As Boolean
    SideField = FieldName
    SideFieldIdx = sf.EditAddField(FieldName, MapWinGIS.FieldType.STRING_FIELD, 20, 20)
    Return True
  End Function

  Public Function MakeMaxElevationField(ByVal FieldName As String) As Boolean
    MaxElevationField = FieldName
    MaxElevationFieldIdx = sf.EditAddField(FieldName, MapWinGIS.FieldType.STRING_FIELD, 20, 20)
    Return True
  End Function

  Public Function MakeXYFields(ByVal myXField As String, ByVal myYField As String) As Boolean
    XField = myXField
    YField = myYField
    XFieldIdx = sf.EditAddField(XField, MapWinGIS.FieldType.DOUBLE_FIELD, 20, 20)
    YFieldIdx = sf.EditAddField(YField, MapWinGIS.FieldType.DOUBLE_FIELD, 20, 20)
    Return True

  End Function

End Class
