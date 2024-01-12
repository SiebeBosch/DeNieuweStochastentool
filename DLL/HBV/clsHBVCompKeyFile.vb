Public Class clsHBVCompKeyFile

    'The comp.key file appears To be a configuration Or parameter file For a computational run In the IHMS software.
    'example of a computational key file:
    'byear'             2011
    'bmonth'               1
    'bday'                 1
    'bhour'                0
    'eyear'             2012
    'emonth'               2
    'eday'                 1
    'ehour'                0
    'timestep'            24
    'outfield'             1
    'qcout     ' 'totmean   '           1           1  'mean      '
    'prec      ' 'totmean   '           2           1  'sum       '

    Dim byear As Integer
    Dim bmonth As Integer
    Dim bday As Integer
    Dim bhour As Integer
    Dim eyear As Integer
    Dim emonth As Integer
    Dim eday As Integer
    Dim ehour As Integer
    Dim timestep As Integer
    Dim outfield As Integer
    Dim output As List(Of clsHBVCompKeyOutputSpecification)




End Class
