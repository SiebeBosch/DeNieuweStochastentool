Imports STOCHLIB.General

Public Class clsStochastenResultaat
    'deze klasse bevat het resultaat van een stochastenrun voor één locatie
    Public Min As Double
    Public Max As Double
    Public Mean As Double
    Public tsMin As Long
    Public tsMax As Long

    Public Path As String           'bevat het relatieve pad naar het resultatenbestand
    Public ModelID As String        'het ID van het simulatiemodel waarvoor dit resultaat geldt.
    Public ParameterName As String  'de parameternaam
    Public LocationID As String     'bevat het ID van de locatie
    Public LocationName As String

    Public P As Double     'kans van de gebeurtenis zelf
    Public CumP As Double  'onderschrijdingskans van deze gebeurtenis (som van alle resultaten met <= deze waterhoogte)
    Public T As Double        'herhalingstijd
    Public Run As clsStochastenRun 'de run onderliggend dit resultaat

    Private Setup As clsSetup

    Public Sub New(ByVal myMin As Double, ByVal mytsMin As Long, ByVal myMax As Double, ByVal mytsMax As Long, ByVal myMean As Double, ByVal myP As Double, ByRef myRun As clsStochastenRun, ByVal myPath As String, myModelID As String, myPar As String, myLoc As String, myName As String, ByRef mySetup As clsSetup)
        Setup = mySetup
        Run = myRun
        Min = myMin
        tsMin = mytsMin
        Max = myMax
        tsMax = mytsMax
        Mean = myMean
        P = myP
        Path = myPath
        ModelID = myModelID
        ParameterName = myPar
        LocationID = myLoc
        LocationName = myName
    End Sub

    Public Sub New(ByVal myMin As Double, ByVal myMax As Double, ByVal myMean As Double, ByRef myRun As clsStochastenRun, ByRef mySetup As clsSetup)
        Run = myRun
        Min = myMin
        Max = myMax
        Mean = myMean
        Setup = mySetup
    End Sub

    'Public Sub DetailedResultToDB(ByRef con As SQLite.SQLiteConnection)
    '    'add the missing rows to the database table
    '    Dim cmd As New SQLite.SQLiteCommand
    '    cmd.Connection = con
    '    If con.State = ConnectionState.Closed Then con.Open()

    '    'write the exceedance values for every combination of run and output location to the database
    '    cmd.CommandText = "INSERT INTO RESULTATEN (KLIMAATSCENARIO, DUUR, LOCATIENAAM, RUNID, MAXVAL, MINVAL, AVGVAL) VALUES ('" & Setup.StochastenAnalyse.KlimaatScenario.ToString.Trim.ToUpper & "'," & Setup.StochastenAnalyse.Duration & ",'" & LocationName & "','" & Run.ID & "'," & Max & "," & Min & "," & Mean & ");"
    '    cmd.ExecuteNonQuery()
    'End Sub




End Class
