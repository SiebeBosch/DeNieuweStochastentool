Public Class frmAbout

  Private Sub frmAbout_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'lblVersion.Text = My.Application.Info.Version.ToString
        'lblVersion.Text = My.Application.Info.Version.Build.ToString

        'openstaande wensen:
        'OPENSTAAND
        'waarschuwing wanneer een csv met getijdenreeksen geen of geen geldige header bevat

        'OPGELOST
        '20150103 rijnummering in het grid met runs
        '20150103 copy/paste in het grid met kansen voor randvoorwaarden.
        '1.0.22: 20150108 het was mogelijk om de naam van een klasse van randvoorwaarden te veranderen. dat mag niet en is afgevangen.
        '1.0.22: 20150108 copy/paste tijdreeksen randvoorwaarden enorm versneld
        '1.0.23: 20150122 volumes in het volumegrid werden niet altijd gesorteerd van laag naar hoog
        '1.0.24: 20150123 oude exceldata uit geheugen verwijderd bij analyse getijdenreeks
        '1.0.24: 20150126 toevoegen nieuwe grondwaterklassen winter ging niet goed. Fout SQL-statement
        '1.0.24: 20150127 fout ontdekt in de toewijzing van frequenties aan de neerslagvolumes. gecorrigeerd in de database, maar ook in de berekening van de kansen van de gebeurtenissen
        '1.0.25: 20150128 foutafhandeling toegevoegd: bij instabiele berekeningen kunnen hoogfrequente gebeurtenissen onterecht in de hoge regionen terechtkomen en resulteren in onrealistische overschrijdingsgrafieken
        '1.0.25: 20150128 overschrijdingsgrafiek bevat vanaf nu ook de laatste run, ook al leidt die tot een deling door nul. Dit om de frequentiesom goed te kunnen verifieren.
        '1.0.25: 20150129 progress bar update tijdens het wegschrijven van csv-files
        '1.0.26: mislukte build
        '1.0.27: 20150212 advanced getijdeklassificatie toegevoegd met verloop verhogingen over verschillende klassen
        '1.0.28: bug bij populate runs wanneer output type = mean. Opgelost, met dank voor melding aan Jacob Luijendijk.
        '1.0.29: bug bij uitvoerlocaties met een id > 20 karakters. .HIA file was ook noodzakelijk. Vanaf nu wordt die ook naar de resultatenmap gekopieerd. Met dank aan Jacob Luijendijk
        '4.5: eigenlijk geen veranderingen aan de software. Wel aan de tabellen
        '4.6: nieuwe functionaliteit: controle of waterhoogtes oplopen met toenemend neerslagvolume
        '4.7: verbeteringen aan de nieuwe controlefunctionaliteit: boxje trekken en sommige exceptions afgevangen
        '4.8: resultaten + controleresultaat worden nu icm klimaatscenario en duur opgeslagen in de database. Oude resultaten kunnen dus opnieuw worden opgeroepen
        '4.8: resultaten worden nu in een submapje met Klimaatscenario en Duur opgeslagen. Ook worden beide genoemd in het Excelbestand en de logfile
        '5.0: grote aanpassingen. Nieuw is de kaart, maar vooral de keuze om zomer- en winterhalfjaar al dan niet te draaien
        Dim assembly As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
        Dim fvi As System.Diagnostics.FileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location)
        Dim version As String = fvi.FileVersion
        lblVersion.Text = version
    End Sub
End Class