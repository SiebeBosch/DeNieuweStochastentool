Imports STOCHLIB.General
Public Class clsHBVResDatFile
    Private Setup As clsSetup

    Dim areasoil As Double
    Dim areauz As Double
    Dim arealz As Double
    Dim areamod As Double
    Dim areatot As Double
    Dim areafield As Double
    Dim rareasoil As Double
    Dim rareauz As Double
    Dim rarealz As Double
    Dim rareamod As Double
    Dim rareatot As Double
    Dim r2loc As Double
    Dim r2sum As Double
    Dim r2inl As Double
    Dim r2out As Double

    'example of res.dat file
    'areasoil  '          1.45000
    'areauz    '          1.45000
    'arealz    '          1.45000
    'areamod   '          1.45000
    'areatot   '          1.45000
    'areafield '          1.45000
    'rareasoil '          1.45000
    'rareauz   '          1.45000
    'rarealz   '          1.45000
    'rareamod  '          1.45000
    'rareatot  '          1.45000
    'r2loc     '       -100.00000
    'r2sum     '       -100.00000
    'r2inl     '       -100.00000
    'r2out     '       -100.00000

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub


End Class
