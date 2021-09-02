Public Class clsParameterValue
    Public Value As Double
    Public InUse As Boolean
    Public OriginalStatus As GeneralFunctions.enmParameterStatus
    Public Verdict As Integer 'rapportcijfer
    Public VerdictStr As String 'toelichting

    Public Sub New(ByVal myValue As Double, myInUse As Boolean)
        Value = myValue
        InUse = myInUse
        Verdict = 10
        OriginalStatus = GeneralFunctions.enmParameterStatus.OK
    End Sub
End Class
