Imports STOCHLIB.General
Public Class clsHBVInstateDatFile
    Private Setup As clsSetup

    Dim States As Dictionary(Of Integer, clsHBVInstateDatStateRecord)

    'this file contains multiple initial states (as a function of time)
    'a block with one state is surrounded by '!!'
    'sm = soil moisture
    'uz = upper zone
    'lz = lower zone
    'wcomp = water in the conceptual reservoir
    'wstr = water in the conceptual reservoir

    'example of instate.dat file
    '!!'        
    'state'                1
    'year'              2013
    'month'                7
    'day'                  1
    'hour'                 1
    '!!'        
    'sm'            1       180.00000
    'uz'            1         0.00000
    'lz'            1         0.00000
    'wcomp'         1         0.00000
    'wstr'          1         0.00000
    '!!'        
    '!!'        
    'state'                2
    'year'              2014
    'month'                1
    'day'                  1
    'hour'                 1
    '!!'        
    'sm'            1       131.35660
    'uz'            1        25.29184
    'lz'            1         0.00000
    'wcomp'         1         0.00000
    'wstr'          1         0.00000
    '!!'        

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

End Class

Public Class clsHBVInstateDatStateRecord
    Private Setup As clsSetup

    Dim StateNumber As Integer
    Dim Year As Integer
    Dim Month As Integer
    Dim Day As Integer
    Dim Hour As Integer

    Dim sm As Double
    Dim uz As Double
    Dim lz As Double
    Dim wcomp As Double
    Dim wstr As Double

    Public Sub New(ByRef mySetup As clsSetup)
        Setup = mySetup
    End Sub

End Class
