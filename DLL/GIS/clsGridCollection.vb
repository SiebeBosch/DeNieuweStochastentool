Imports STOCHLIB.General
Imports System.IO
Imports System.Xml

Public Class clsGridCollection
  Private Setup As clsSetup
  Public ActiveRaster As clsRaster
  Public Grids As New Dictionary(Of String, clsRaster)

  Public Sub New(ByRef mySetup As clsSetup)
    Setup = mySetup
    ActiveRaster = New clsRaster(Me.Setup)
  End Sub

  Public Sub Clear()
    'clear the contents of this collection
    Grids = Nothing
    ActiveRaster = Nothing
  End Sub

  Public Function Populate(ByVal path As String, ByVal SubDirs As Boolean, ByVal Extension As String, ByVal UseXML As Boolean) As Boolean
    Dim xmlPath As String = path & "\gridcollection.xml"

    If UseXML AndAlso System.IO.File.Exists(path & "\gridcollection.xml") Then
      If Not PopulatefromXML(xmlPath) Then
        If Not PopulateFromDir(path, SubDirs, Extension) Then
          Return False
        Else
          writeXML(xmlPath)
          Return True
        End If
      Else
        Return True
      End If
    Else
      If Not PopulateFromDir(path, SubDirs, Extension) Then
        Return False
      Else
        writexml(xmlPath)
        Return True
      End If
    End If

  End Function

  Public Function writeTXT(ByVal txtPath As String) As Boolean
    Try
      Dim myGrid As clsRaster
      Using txtWriter As New StreamWriter(txtPath, False)
        For Each myGrid In Grids.Values
          txtWriter.WriteLine(myGrid.Path)
        Next
      End Using
      Return True
    Catch ex As Exception
      Me.Setup.Log.AddError(ex.Message)
      Return False
    End Try
  End Function

  Public Sub writeXML(ByVal xmlPath As String)
    Dim myGrid As clsRaster
    Using xmlWriter As New StreamWriter(xmlPath, False)
      xmlWriter.WriteLine("<gridcollection>")
      For Each myGrid In Grids.Values
        xmlWriter.WriteLine("  <grid path=" & Chr(34) & myGrid.Path & Chr(34) & " ncols=" & Chr(34) & myGrid.nCol & Chr(34) & " nrows=" & Chr(34) & myGrid.nRow & Chr(34) & " xllcorner=" & Chr(34) & myGrid.XLLCorner & Chr(34) & " yllcorner=" & Chr(34) & myGrid.YLLCorner & Chr(34) & " dx=" & Chr(34) & myGrid.dX & Chr(34) & " dy=" & Chr(34) & myGrid.dY & Chr(34) & " nodata_value=" & Chr(34) & myGrid.NoDataVal & Chr(34) & " />")
      Next
      xmlWriter.WriteLine("</gridcollection>")
    End Using
  End Sub

  Public Function PopulatefromXML(ByVal xmlPath As String) As Boolean

    Dim m_xmld As XmlDocument
    Dim m_nodelist As XmlNodeList
    Dim m_node As XmlNode
    Dim n_node As XmlNode
    Dim o_node As XmlNode
    Dim myRaster As clsRaster

    Try
      If System.IO.File.Exists(xmlPath) Then

        m_xmld = New XmlDocument()        'Create the XML Document
        m_xmld.Load(xmlPath)        'Load the Xml file
        m_nodelist = m_xmld.SelectNodes("/gridcollection")        'Get the list of general-nodes 

        'Loop through the nodes
        For Each m_node In m_nodelist
          For Each n_node In m_node.ChildNodes
            If n_node.Name = "grid" Then
              myRaster = New clsRaster(Me.Setup)
              o_node = n_node.Attributes.GetNamedItem("path")
              myRaster.Path = o_node.InnerText
              o_node = n_node.Attributes.GetNamedItem("ncols")
              myRaster.nCol = o_node.InnerText
              o_node = n_node.Attributes.GetNamedItem("nrows")
              myRaster.nRow = o_node.InnerText
              o_node = n_node.Attributes.GetNamedItem("xllcorner")
              myRaster.XLLCorner = o_node.InnerText
              o_node = n_node.Attributes.GetNamedItem("yllcorner")
              myRaster.YLLCorner = o_node.InnerText
              o_node = n_node.Attributes.GetNamedItem("dx")
              myRaster.dX = o_node.InnerText
              o_node = n_node.Attributes.GetNamedItem("dy")
              myRaster.dY = o_node.InnerText
              o_node = n_node.Attributes.GetNamedItem("nodata_value")
              myRaster.NoDataVal = o_node.InnerText
              myRaster.CompleteMetaHeaderWithoutReading()
              Grids.Add(myRaster.Path, myRaster)
            End If
          Next
        Next
        Return True
      Else
        Return False
      End If
    Catch ex As Exception
      Me.Setup.Log.AddError(ex.Message)
      Return False
    End Try
  End Function

  Public Function PopulateFromDir(ByVal path As String, ByVal SubDirs As Boolean, ByVal Extension As String) As Boolean
    Dim dirInfo As New IO.DirectoryInfo(path)
    Dim fileObject As FileSystemInfo
    Dim myRaster As clsRaster
    Dim i As Long = 0, n As Long = 1

    Me.Setup.GeneralFunctions.UpdateProgressBar("Populating grid collection...", 0, 1)
    n = dirInfo.GetFileSystemInfos.Count

    If SubDirs = True Then
      For Each fileObject In dirInfo.GetFileSystemInfos()
        i += 1
        Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
        If System.IO.Directory.Exists(fileObject.FullName) Then
          PopulateFromDir(fileObject.FullName, SubDirs, Extension)
        Else
          If Right(fileObject.FullName, Extension.Length).ToUpper = Extension.ToUpper Then
            myRaster = New clsRaster(Me.Setup)
            myRaster.Initialize(fileObject.FullName)
            myRaster.CompleteMetaHeader()
            Grids.Add(myRaster.Path, myRaster)
          End If
        End If
      Next
    Else
      For Each fileObject In dirInfo.GetFileSystemInfos()
        i += 1
        Me.Setup.GeneralFunctions.UpdateProgressBar("", i, n)
        If Not System.IO.Directory.Exists(fileObject.FullName) Then
          If Right(fileObject.FullName, Extension.Length).ToUpper = Extension.ToUpper Then
            myRaster = New clsRaster(Me.Setup)
            myRaster.Path = fileObject.FullName
            myRaster.CompleteMetaHeader()
            Grids.Add(myRaster.Path, myRaster)
          End If
        End If
      Next
    End If
    Return True
  End Function

  Public Function getValueFromXY(ByVal X As Double, ByVal Y As Double, ByRef Val As Double, Optional ByVal AllowZero As Boolean = True) As Boolean

    'This function retrieves a value from the grid collection given an X and Y coordinate
    'first make sure we have the correct raster before us
    If Not ActiveRaster.PointInsideGrid(X, Y) Then
      ActiveRaster.Close()
      ActiveRaster = GetByCoordinate(X, Y)
      If Not ActiveRaster.Read(False) Then Return False
    End If

    If Not ActiveRaster Is Nothing Then
      If Not ActiveRaster.GetCellValueFromXY(X, Y, Val) Then Return False
      If Val = ActiveRaster.NoDataVal OrElse (Val = 0 AndAlso AllowZero = False) Then
        Return False
      Else
        Return True
      End If
    Else
      Return False
    End If

  End Function

  Public Function GetByCoordinate(ByVal X As Double, ByVal Y As Double) As clsRaster
    Dim myGrid As clsRaster
    For Each myGrid In Grids.Values
      If myGrid.PointInsideGrid(X, Y) Then Return myGrid
    Next
    Return Nothing
  End Function

  Public Sub closeActiveGrid()
    ActiveRaster.Close()
  End Sub

End Class
