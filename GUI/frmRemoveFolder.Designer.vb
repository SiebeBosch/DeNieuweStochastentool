<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmRemoveFolder
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmRemoveFolder))
        Me.txtDirNamePart = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnRemoveDirs = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblRootDir = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'txtDirNamePart
        '
        Me.txtDirNamePart.Location = New System.Drawing.Point(245, 50)
        Me.txtDirNamePart.Name = "txtDirNamePart"
        Me.txtDirNamePart.Size = New System.Drawing.Size(100, 20)
        Me.txtDirNamePart.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(20, 50)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(143, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Alle mappen met in de naam:"
        '
        'btnRemoveDirs
        '
        Me.btnRemoveDirs.Location = New System.Drawing.Point(270, 88)
        Me.btnRemoveDirs.Name = "btnRemoveDirs"
        Me.btnRemoveDirs.Size = New System.Drawing.Size(75, 23)
        Me.btnRemoveDirs.TabIndex = 2
        Me.btnRemoveDirs.Text = "Uitvoeren"
        Me.btnRemoveDirs.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(20, 20)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(47, 13)
        Me.Label2.TabIndex = 3
        Me.Label2.Text = "Root dir:"
        '
        'lblRootDir
        '
        Me.lblRootDir.AutoSize = True
        Me.lblRootDir.Location = New System.Drawing.Point(73, 20)
        Me.lblRootDir.Name = "lblRootDir"
        Me.lblRootDir.Size = New System.Drawing.Size(10, 13)
        Me.lblRootDir.TabIndex = 4
        Me.lblRootDir.Text = "-"
        '
        'frmRemoveFolder
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(357, 123)
        Me.Controls.Add(Me.lblRootDir)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.btnRemoveDirs)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtDirNamePart)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmRemoveFolder"
        Me.Text = "Directories verwijderen"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtDirNamePart As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents btnRemoveDirs As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents lblRootDir As Label
End Class
