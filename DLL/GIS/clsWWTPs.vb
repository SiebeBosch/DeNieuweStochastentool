Imports STOCHLIB.General
Imports System.IO

Public Class clsWWTPs

    Friend WWTPs As New Dictionary(Of String, clsWWTP)
    Private setup As clsSetup

    Friend Sub New(ByRef mySetup As clsSetup)
        Me.setup = mySetup
    End Sub

    Friend Sub WriteTopo(ByVal Append As Boolean, ExportDir As String)

        Dim i As Integer
        Using nodtpwriter As New StreamWriter(ExportDir & "\3b_nod.tp", Append)
            Using lnktpwriter As New StreamWriter(ExportDir & "\3b_link.tp", Append)

                If Append = False Then
                    'write the header (necessary!!)
                    nodtpwriter.WriteLine("BBB2.2")
                    lnktpwriter.WriteLine("BBB2.2")
                End If

                'write the WWTP's and their boundaries
                For Each myWWTP As clsWWTP In Me.setup.WWTPs.WWTPs.Values
                    nodtpwriter.WriteLine("NODE id '" & myWWTP.ID & "' ri '-1' mt 1 '14' nt 56 ObID '3B_WWTP' px " & myWWTP.X & " py " & myWWTP.Y & " node")
                    nodtpwriter.WriteLine("NODE id 'bnd" & myWWTP.ID & "' ri '-1' mt 1 '6' nt 47 ObID '3B_BOUNDARY' px " & myWWTP.X & " py " & myWWTP.Y - 500 & " node")
                Next

                'write the link between WWTP and boundary
                i = 0
                For Each mywwtp As clsWWTP In Me.setup.WWTPs.WWTPs.Values
                    i += 1
                    lnktpwriter.WriteLine("BRCH id 'WWTP2BND_" & Str(i).Trim & "' ri '-1' mt 1 '1' bt 18 ObID '3B_LINK_RWZI' bn '" & mywwtp.ID & "' en 'bnd" & mywwtp.ID & "'  brch")
                Next

            End Using
        End Using
    End Sub

    Friend Function GetAdd(ByVal ID As String) As clsWWTP
        Dim WWTP As clsWWTP
        If WWTPs.ContainsKey(ID.Trim.ToUpper) Then
            Return WWTPs(ID.Trim.ToUpper)
        Else
            WWTP = New clsWWTP(Me.setup)
            WWTP.ID = ID
            WWTP.InUse = True
            WWTPs.Add(ID.Trim.ToUpper, WWTP)
            Me.setup.Log.AddWarning("Waste Water Treatment Plant " & ID & " not found in shapefile. A new one was created")
            Return WWTP
        End If
    End Function




End Class
