Imports STOCHLIB.General
Public Class clsHBVSubBasin
    Private Setup As clsSetup

    'attributes from the basin.par file for this subbasin
    Friend BasinName As String
    Friend BasinDir As String
    Friend bstatus As String
    Friend pstatus As String
    Friend btype As String
    Friend cregion As Double
    Friend outletx As Double
    Friend outlety As Double
    Friend bcode As Integer

    Dim CompKeyFile As clsHBVCompKeyFile            'contains configuration or parameter file for a computational run in the IHMS software
    Dim SimulationResults As clsHBVCompTxtFile      'contains the simulation results for this subbasin
    'dos_comp.dat is a binary file
    Dim ResDatFile As clsHBVResDatFile              'contains areas and such
    Dim InstateDatFile As clsHBVInstateDatFile      'contains initial state variables for various points in time
    Dim bModDatFile As clsHBVBModDatFile            'contains the model parameters  
    'abstr.par
    Dim IndataParFile As clsHBVIndataParFile        'contains the input data from stations (Precip, Temp, Evap) for this subbasin
    'zon.par

    Public Sub New(ByRef mySetup As clsSetup, MyBasinName As String)
        Setup = mySetup
        BasinName = MyBasinName
    End Sub

End Class
