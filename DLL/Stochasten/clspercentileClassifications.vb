Imports STOCHLIB.General
Public Class clspercentileClassifications
    'this class describes classifications of events by one or more(!) percentileClasses
    Private Setup As clsSetup

    Public Classifications As New Dictionary(Of String, clspercentileClassification) 'key = classification name, consisting of all parameters and classes involved

    Public Sub New(ByRef mySetup As clsSetup)
        Me.Setup = mySetup
    End Sub

    Public Function Add(myClassifications As List(Of clsPercentileClass)) As Boolean
        Try
            Dim myClassification As New clspercentileClassification
            For Each MyPercentileClass As clsPercentileClass In myClassifications
                myClassification.Name &= MyPercentileClass.Name & "_"
                myClassification.Classes.Add(MyPercentileClass)
            Next

            ' Remove the last underscore from the name
            myClassification.Name = myClassification.Name.TrimEnd(New Char() {"_"c})

            Classifications.Add(myClassification.Name.Trim.ToUpper, myClassification)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function



End Class
