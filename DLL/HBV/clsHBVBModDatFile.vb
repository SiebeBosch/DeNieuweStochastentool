Imports STOCHLIB.General
Public Class clsHBVBModDatFile
    Private Setup As clsSetup

    Dim Groups As List(Of clsHBVModDatGroup)

    'example of bmod.dat file
    'group'
    'byear     ' 2008
    'bmonth    ' 10
    'bday      ' 1
    'eyear     ' 2009
    'emonth    ' 4
    'eday      ' 1
    'beta' 1.199
    'perc' 2.832
    'group'
    'byear     ' 2009
    'bmonth    ' 1
    'bday      ' 1
    'eyear     ' 2024
    'emonth    ' 1
    'eday      ' 1
    'alfa' 0.702
    'cevpfo' 0
    'cevpl' 0
    'cflux' 1
    'cfmax' 3.724
    'cfr' 0.05
    'critstep' 1
    'dttm' -0.374
    'ecalt' 0
    'ecorr' 1
    'fc' 255.379
    'focfmax' 0.6
    'fosfcf' 0.8
    'hq' 1.715
    'k4' 0.011
    'khq' 0.543
    'lp' 0.8
    'maxbaz' 0
    'pcalt' 0
    'pcorr' 1
    'recstep' 1
    'rfcf' 0.649
    'sclass' 1
    'sfcf' 1
    'sfdistfi' 0.2
    'sfdistfo' 0.5
    'tcalt' 0
    'tt' 0
    'ttint' 1
    'whc' 0.1

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

End Class

Public Class clsHBVModDatGroup
    Private Setup As clsSetup

    Dim GroupNumber As Integer
    Dim StartDate As Date
    Dim EndDate As Date
    Dim Beta As Double
    Dim Perc As Double
    Dim Alfa As Double
    Dim Cevpfo As Double
    Dim Cevpl As Double
    Dim Cflux As Double
    Dim Cfmax As Double
    Dim Cfr As Double
    Dim Critstep As Double
    Dim Dttm As Double
    Dim Ecalt As Double
    Dim Ecorr As Double
    Dim Fc As Double
    Dim Focfmax As Double
    Dim Fosfcf As Double
    Dim Hq As Double
    Dim K4 As Double
    Dim Khq As Double
    Dim Lp As Double
    Dim Maxbaz As Double
    Dim Pcalt As Double
    Dim Pcorr As Double
    Dim Recstep As Double
    Dim Rfcf As Double
    Dim Sclass As Double
    Dim Sfcf As Double
    Dim Sfdistfi As Double
    Dim Sfdistfo As Double
    Dim Tcalt As Double
    Dim Tt As Double
    Dim Ttint As Double
    Dim Whc As Double

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

End Class
