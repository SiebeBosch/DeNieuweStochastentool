Imports STOCHLIB.General
Public Class clsBoundaryCondition
    Private Setup As clsSetup
    Friend name As String
    Friend bcfunction As String
    Friend timeInterpolation As String
    Friend quantity As String

    Friend quantities As New Dictionary(Of String, clsQuantity)
    Friend DataTable As New clsSobekTable(Setup)

    Public Sub New()

    End Sub

    Public Sub New(ByRef mySetup As clsSetup, myName As String)
        Setup = mySetup
        name = myName
    End Sub

    Public Sub SetDatatable(myDataTable As clsSobekTable)
        DataTable = myDataTable
    End Sub

    Public Sub AddQuantity(myQuantityName As String, myQuantity As clsQuantity)
        quantities.Add(myQuantityName.Trim.ToUpper, myQuantity)
    End Sub

    Public Sub AddValuePair(XVal As Double, YVal As Double)
        DataTable.AddDataPair(2, XVal, YVal)
    End Sub

    Public Sub setFunction(myFunction As String)
        bcfunction = myFunction
    End Sub

    Public Sub setTimeInterpolation(myTimeInterpolation As String)
        timeInterpolation = myTimeInterpolation
    End Sub

End Class
