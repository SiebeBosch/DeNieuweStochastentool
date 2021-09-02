Option Explicit On
Imports System.IO
Imports STOCHLIB.General
Friend Class clsProfileDatRecords
  Friend Records As New Dictionary(Of String, clsProfileDatRecord)
  Private setup As clsSetup
  Private SbkCase As clsSobekCase

  Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase)
    Me.setup = mySetup
    Me.SbkCase = myCase
  End Sub

  Friend Sub AddPrefix(ByVal Prefix As String)
    For Each Dat As clsProfileDatRecord In Records.Values
      'add the prefix to the profile id and its definition id
      Dat.ID = Prefix & Dat.ID
      Dat.di = Prefix & Dat.di
    Next
  End Sub

    Friend Sub InitializeExport(ExportDir As String)
        If System.IO.File.Exists(ExportDir & "\profile.dat") Then System.IO.File.Delete(ExportDir & "\profile.dat")
    End Sub

    Friend Function setElevationFromGrid() As Boolean
    'Sets the surface level elevation for each profile based on the elevation grid and the perpendicular shapefile
    'Note: elevation grid and perpendicular shapefile must be present!
    Dim ProfDat As clsProfileDatRecord, myShape As MapWinGIS.Shape, i As Long
    Dim myID As String

    Try
      If Not Me.setup.GISData.PerpendicularShapeFile.Open() Then Throw New Exception("Could not open shapefile with perpendicular lines to cross sections.")
      If Not Me.setup.GISData.ElevationGrid.Read(False) Then Throw New Exception("Could not open elevation grid.")

      Me.setup.GeneralFunctions.UpdateProgressBar("Extracting surface levels from grid.", 0, 10)
      With Me.setup.GISData.PerpendicularShapeFile
        For i = 0 To .sf.NumShapes - 1
          Me.setup.GeneralFunctions.UpdateProgressBar("", i + 1, .sf.NumShapes)
          myShape = .sf.Shape(i)
          myID = .getID(i)

          ProfDat = Records.Item(myID.Trim.ToUpper)
          If Not ProfDat Is Nothing Then
            ProfDat.rs = Me.setup.GISData.ElevationGrid.MaxElevationForShape(myID, ProfDat.rs, myShape, 1)
          End If
        Next
      End With

      Return True
    Catch ex As Exception
      Me.setup.Log.AddError(ex.Message)
      Return False
    End Try

  End Function

  Friend Sub Read(ByVal myStrings As Collection)
    Dim i As Integer = 0
    Me.setup.GeneralFunctions.UpdateProgressBar("Reading profiles...", 0, myStrings.Count)
    For Each myString As String In myStrings
      i += 1
      Me.setup.GeneralFunctions.UpdateProgressBar("", i, myStrings.Count)
      Dim myRecord As clsProfileDatRecord = New clsProfileDatRecord(Me.setup)
      myRecord.Read(myString)

      If Records.ContainsKey(myRecord.id.Trim.ToUpper) Then
        Me.setup.Log.AddWarning("Multiple instances of profile " & myRecord.id & " found in SOBEK schematization.")
      Else

        'als het netwerk ook is ingelezen, hoeven we alleen die records in te lezen waarvan het object ook daadwerkelijk in het netwerk voorkomt
        If sbkcase.CFTopo.Reaches.Reaches.Count > 0 Then
          If sbkcase.CFTopo.ReachObjectExists(myRecord.ID.Trim.ToUpper) Then
            Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
          End If
        Else
          Call Records.Add(myRecord.ID.Trim.ToUpper, myRecord)
        End If
      End If
    Next myString
  End Sub

    Friend Sub Write(ByVal Append As Boolean, ExportDir As String)
        Dim i As Integer = 0, path As String

        If Not ExportDir Is Nothing Then
            path = ExportDir & "\profile.dat"
        Else
            path = ExportDir & "\profile.dat"
        End If

        setup.GeneralFunctions.UpdateProgressBar("Writing profile.dat file...", 0, Records.Count)
        Using datWriter = New StreamWriter(path, Append)
            For Each myRecord As clsProfileDatRecord In Records.Values
                i += 1
                setup.GeneralFunctions.UpdateProgressBar("", i, Records.Count)
                If myRecord.InUse Then myRecord.Write(datWriter)
            Next myRecord
        End Using
    End Sub

    Friend Function FindByDefinitionID(ByVal mydi As String) As clsProfileDatRecord
    'zoekt het eerste kunstwerk op dat een gegeven definitie ID gebruikt en geeft dit terug
    For Each myDat As clsProfileDatRecord In Records.Values
      If Not myDat.di Is Nothing AndAlso myDat.di.Trim.ToUpper = mydi.Trim.ToUpper Then
        Return myDat
      End If
    Next myDat
    Return Nothing
  End Function
End Class
