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
        Me.Label1.Location = New System.Drawing.Point(9, 11)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(57, 16)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Root dir:"
        '
        'lblRootDir
        '
        Me.lblRootDir.AutoSize = True
        Me.lblRootDir.Location = New System.Drawing.Point(80, 11)
        Me.lblRootDir.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblRootDir.Name = "lblRootDir"
        Me.lblRootDir.Size = New System.Drawing.Size(11, 16)
        Me.lblRootDir.TabIndex = 1
        Me.lblRootDir.Text = "-"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(9, 41)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(236, 16)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Hernoem alle (sub)mappen met naam:"
        '
        'txtFromDir
        '
        Me.txtFromDir.Location = New System.Drawing.Point(264, 37)
        Me.txtFromDir.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtFromDir.Name = "txtFromDir"
        Me.txtFromDir.Size = New System.Drawing.Size(140, 22)
        Me.txtFromDir.TabIndex = 3
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(413, 41)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(37, 16)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "naar:"
        '
        'txtToDir
        '
        Me.txtToDir.Location = New System.Drawing.Point(465, 37)
        Me.txtToDir.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtToDir.Name = "txtToDir"
        Me.txtToDir.Size = New System.Drawing.Size(140, 22)
        Me.txtToDir.TabIndex = 5
        '
        'btnMultiDirRename
        '
        Me.btnMultiDirRename.Location = New System.Drawing.Point(507, 82)
        Me.btnMultiDirRename.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnMultiDirRename.Name = "btnMultiDirRename"
        Me.btnMultiDirRename.Size = New System.Drawing.Size(100, 28)
        Me.btnMultiDirRename.TabIndex = 6
        Me.btnMultiDirRename.Text = "Uitvoeren"
        Me.btnMultiDirRename.UseVisualStyleBackColor = True
        '
        'frmDirRename
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(619, 127)
        Me.Controls.Add(Me.btnMultiDirRename)
        Me.Controls.Add(Me.txtToDir)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtFromDir)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.lblRootDir)
        Me.Controls.Add(Me.Label1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
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
