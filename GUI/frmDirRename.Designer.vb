<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmDirRename
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmDirRename))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblRootDir = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtFromDir = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtToDir = New System.Windows.Forms.TextBox()
        Me.btnMultiDirRename = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(7, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(47, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Root dir:"
        '
        'lblRootDir
        '
        Me.lblRootDir.AutoSize = True
        Me.lblRootDir.Location = New System.Drawing.Point(60, 9)
        Me.lblRootDir.Name = "lblRootDir"
        Me.lblRootDir.Size = New System.Drawing.Size(10, 13)
        Me.lblRootDir.TabIndex = 1
        Me.lblRootDir.Text = "-"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(7, 33)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(185, 13)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Hernoem alle (sub)mappen met naam:"
        '
        'txtFromDir
        '
        Me.txtFromDir.Location = New System.Drawing.Point(198, 30)
        Me.txtFromDir.Name = "txtFromDir"
        Me.txtFromDir.Size = New System.Drawing.Size(106, 20)
        Me.txtFromDir.TabIndex = 3
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(310, 33)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(31, 13)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "naar:"
        '
        'txtToDir
        '
        Me.txtToDir.Location = New System.Drawing.Point(349, 30)
        Me.txtToDir.Name = "txtToDir"
        Me.txtToDir.Size = New System.Drawing.Size(106, 20)
        Me.txtToDir.TabIndex = 5
        '
        'btnMultiDirRename
        '
        Me.btnMultiDirRename.Location = New System.Drawing.Point(380, 67)
        Me.btnMultiDirRename.Name = "btnMultiDirRename"
        Me.btnMultiDirRename.Size = New System.Drawing.Size(75, 23)
        Me.btnMultiDirRename.TabIndex = 6
        Me.btnMultiDirRename.Text = "Uitvoeren"
        Me.btnMultiDirRename.UseVisualStyleBackColor = True
        '
        'frmDirRename
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(464, 103)
        Me.Controls.Add(Me.btnMultiDirRename)
        Me.Controls.Add(Me.txtToDir)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtFromDir)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.lblRootDir)
        Me.Controls.Add(Me.Label1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmDirRename"
        Me.Text = "Multi Rename Directory"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents lblRootDir As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents txtFromDir As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents txtToDir As TextBox
    Friend WithEvents btnMultiDirRename As Button
End Class
