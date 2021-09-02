'Option Explicit Off

''Imports odssvr20

'Imports System.IO
'Imports STOCHLIB.General

'Public Class clsHisFile
'  Friend Path As String
'    'Friend myHis As clsODSServer
'    'Friend ODSFile As clsODSFile
'    Friend nLoc As Integer
'  Friend nPar As Integer
'  Friend nTim As Integer
'  Friend Values As System.Array
'  Public Loc As New Object
'  Public Par As New Object
'  Public Tim As New Object
'  Friend ResultsTable As clsSobekTable
'  Friend Stats As clsHisFileStats

'  'Friend Locations As New Collection
'  'Friend Parameters As New Collection
'  'Friend Times As New Collection

'  Private Setup As clsSetup
'  Private SbkCase As clsSobekCase

'  Friend Sub New(ByRef mySetup As clsSetup, ByRef myCase As clsSobekCase, ByVal FileName As String)
'    Me.Setup = mySetup
'    Me.SbkCase = myCase
'    Me.ResultsTable = New clsSobekTable(Me.Setup)
'    Me.Path = myCase.CaseDir & "\" & FileName
'    Me.Stats = New clsHisFileStats(Me.Setup)
'  End Sub

'  Friend Sub clear()
'    Values = Nothing
'    Loc = Nothing
'    Par = Nothing
'    Tim = Nothing
'  End Sub

'  Friend Function getParIdx(ByVal PartOfParName As String) As Integer
'    Dim iPar As Integer
'    For iPar = 1 To nPar
'      If InStr(Par(iPar).ToString.Trim.ToUpper, PartOfParName.Trim.ToUpper) > 0 Then Return iPar
'    Next
'    Return 0
'  End Function

'  Friend Function getLocIdx(ByVal LocID As String) As Integer
'    Dim iLoc As Integer
'    For iLoc = 1 To nLoc
'      If Loc(iLoc).ToString.Trim.ToUpper = LocID.Trim.ToUpper Then Return iLoc
'    Next
'    Return 0
'  End Function

'  Public Function Initialize() As Boolean
'    'Initialiseert het uitlezen van een hisfile, en slaat ook meteen de structuur van locaties, parameters en tijdstappen op

'    Try
'      myHis = New clsODSServer
'      myHis.KeepFilesOpen = True
'      Dim oFileInfo As New FileInfo(Path)
'      If Not oFileInfo.Exists Then
'        Throw New Exception("Hisfile not found: " & Path)
'      End If
'      ODSFile = myHis.Add(Path, Path, True, False)

'      'first read the locations
'      Dim iRes As Long

'      iRes = myHis.GetLoc(Path, , nLoc, , Loc)
'      If iRes <> 0 Then
'        MsgBox("Function call GetLoc not successful.")
'      End If

'      ' read the parameters
'      iRes = myHis.GetPar(Path, , nPar, , Par)
'      If iRes <> 0 Then
'        MsgBox("Function call GetPar not successful.")
'      End If

'      ' read the dates/times
'      iRes = myHis.GetTime(Path, , nTim, , , Tim)
'      If iRes <> 0 Then
'        MsgBox("Function call GetTime not successful.")
'      End If

'      Return True

'    Catch ex As Exception
'      Me.Setup.Log.AddError("Error in sub Initialize of class clsHisFile")
'      Me.Setup.Log.AddError(ex.Message)
'      Return False
'    End Try

'  End Function

'  Public Function getSpecificResults(ByRef IDs As Dictionary(Of String, String), ByVal PartOfParName As String) As Boolean
'    Dim iRes As Long, iLoc As Long, iPar As Long, iTim As Long
'    Dim firstDone As Boolean = False
'    Dim nLocSelected As Integer, nParSelected As Integer, nTimSelected As Integer

'    'zoek de juiste locatie
'    ResultsTable = New clsSobekTable(Me.Setup)
'    Call Initialize()

'    'leg de dimensies van de arrays vast (moeten 1-based zijn)
'    Dim Lengths() As Integer = {1}
'    Dim Lbounds() As Integer = {1}
'    Dim ValLenghts() As Integer = {1, 1, nTim}
'    Dim ValLBounds() As Integer = {1, 1, 1}

'    'maak twee 1-based arrays aan!
'    Dim LocIdx As Array = Array.CreateInstance(GetType(Integer), Lengths, Lbounds)
'    Dim ParIdx As Array = Array.CreateInstance(GetType(Integer), Lengths, Lbounds)
'    Dim StrLoc As Array = Array.CreateInstance(GetType(String), Lengths, Lbounds)
'    Dim StrPar As Array = Array.CreateInstance(GetType(String), Lengths, Lbounds)
'    Dim myValues As Array = Array.CreateInstance(GetType(Single), ValLenghts, ValLBounds)


'    iPar = Me.getParIdx(PartOfParName)

'    For iLoc = 1 To nLoc
'      If IDs.ContainsKey(Loc(iLoc).ToString.Trim.ToUpper) Then
'        'locatie gevonden. Zoek de juiste parameter

'        'parameter gevonden! Doorloop nu alle tijdstappen.
'        LocIdx(1) = iLoc
'        ParIdx(1) = iPar
'        StrLoc(1) = Loc(iLoc)
'        StrPar(1) = Par(iPar)

'        'iRes = myHis.GetData(myValues, nLocSelected, nParSelected, nTimSelected, myPath, , LocIdx, , ParIdx)
'        'iRes = myHis.GetData(myValues, nLocSelected, nParSelected, nTimSelected, Path, , , StrLoc, ParIdx) 'ik vertrouw de afhandeling van LocIdx niet, dus even zo
'        iRes = myHis.GetData2(myValues, nLocSelected, nParSelected, nTimSelected, Path, , , StrLoc(), , StrPar())

'        If iRes <> 0 Then
'          Me.Setup.Log.AddError("Could not read results for par " & Par(iPar) & " and location " & Loc(iLoc) & " from hisfile " & Path)
'          Return Nothing
'        End If

'        If firstDone = False Then
'          firstDone = True
'          For iTim = 1 To nTim
'            ResultsTable.Dates.Add(iTim.ToString.Trim, Date.FromOADate(Tim(iTim)))
'            ResultsTable.Values1.Add(iTim.ToString.Trim, myValues(1, 1, iTim))
'          Next
'        Else
'          For iTim = 1 To nTim
'            ResultsTable.Values1.Item(iTim.ToString.Trim) += myValues(1, 1, iTim)
'          Next
'        End If

'        Exit For
'      End If
'    Next

'  End Function

'  Public Sub ReadAll()

'    Dim iRes As Integer

'    Try
'      myHis = New clsODSServer

'      myHis.KeepFilesOpen = True ' nodig om te voorkomen dat ODSSVR zichzelf continue gaat initialiseren, eigenlijk een foutje in ODSSVR zelf
'      Dim oFileInfo As New FileInfo(Path)
'      If Not oFileInfo.Exists Then
'        Throw New Exception("Hisfile not found: " & Path)
'      End If
'      Dim ODSFile As clsODSFile
'      ODSFile = myHis.Add(Path, Path, True, True)

'      'lees domweg de hele file
'      Dim lngIndex As Integer
'      lngIndex = 0
'      iRes = myHis.GetAllData(Values, nLoc, nPar, nTim, Path, lngIndex, Loc, Par, Tim)
'      If iRes <> 0 Then Throw New Exception("Function call GetData not successful.")
'    Catch ex As Exception

'    End Try

'  End Sub


'  ''' <summary>
'  ''' Maakt op basis van één gekozen locatie en (deel van) parameternaam een sobektable aan uit de beschikbare resultaten
'  ''' </summary>
'  ''' <param name="ID"></param>
'  ''' <param name="Parameter"></param>
'  ''' <returns>clsSobektable</returns>
'  ''' <remarks></remarks>
'  Friend Function toTimeTable(ByVal ID As String, ByVal Parameter As String, ByRef myTable As clsSobekTable) As Boolean
'    Dim iPar As Integer, iLoc As Integer, iTim As Single
'    Dim LocFound As Boolean, ParFound As Boolean

'    myTable.ID = ID

'    'zoek de locatie in de hisfile
'    For iLoc = 1 To nLoc
'      If Loc(iLoc).ToString.Trim.ToUpper = ID.Trim.ToUpper Then
'        LocFound = True
'        Exit For
'      End If
'    Next

'    'zoek de parameter in de hisfile
'    For iPar = 1 To nPar
'      If InStr(Par(iPar).ToString.Trim.ToUpper, Parameter.Trim.ToUpper) > 0 Then
'        ParFound = True
'        Exit For
'      End If
'    Next

'    'foutafhandeling
'    If Not ParFound Then
'      Me.setup.Log.AddError("Parameter " & Parameter & " not found in hisfile.")
'      Return False
'    End If

'    If Not LocFound Then
'      Me.setup.Log.AddError("Location " & ID & " not found in hisfile.")
'      Return False
'    End If

'    'vul de tabel met waarden uit de hisfile
'    For iTim = 1 To nTim
'      myTable.AddDatevalPair(Date.FromOADate(Tim(iTim)), Values(iLoc, iPar, iTim))
'    Next
'    Return True

'  End Function

'End Class
