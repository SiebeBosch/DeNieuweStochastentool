Option Explicit On

Public Class clsDrainageProfiel
    Public id As String
    Public soilclass As clsSoilClass
    Public landuseclass As clsLanduseClass
    Public w1 As Double
    Public w2 As Double
    Public w3 As Double
    Public w4 As Double
    Public d1 As Double
    Public d2 As Double
    Public d3 As Double
    Public sr As Double
    Public inf As Double
    Public sto As Double 'max storage on land

    Public record As String

    Public Sub New()

    End Sub

    Public Sub New(soilclass As String, landuseclass As String)
        id = soilclass.Trim & "_" & landuseclass.Trim
    End Sub

    Public Sub build()
        record = "ERNS id '" & id & "' nm '" & id & "' cvi " & inf & " cvo " & w1 & " " & w2 & " " & w3 & " " & w4 & " lv " & d1 & " " & d2 & " " & d3 & " cvs " & sr & " erns"
    End Sub

    Public Sub write(ByVal myWriter As System.IO.StreamWriter)
        Call build()
        myWriter.WriteLine(record)
    End Sub

End Class
