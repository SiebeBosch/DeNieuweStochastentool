<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPickDelimiter
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPickDelimiter))
        Me.btnChoose = New System.Windows.Forms.Button()
        Me.txtDelimiter = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chkContainsHeader = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'btnChoose
        '
        Me.btnChoose.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnChoose.Location = New System.Drawing.Point(118, 108)
        Me.btnChoose.Name = "btnChoose"
        Me.btnChoose.Size = New System.Drawing.Size(68, 31)
        Me.btnChoose.TabIndex = 0
        Me.btnChoose.Text = "Ok"
        Me.btnChoose.UseVisualStyleBackColor = True
        '
        'txtDelimiter
        '
        Me.txtDelimiter.Location = New System.Drawing.Point(142, 23)
        Me.txtDelimiter.Name = "txtDelimiter"
        Me.txtDelimiter.Size = New System.Drawing.Size(44, 22)
        Me.txtDelimiter.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(25, 23)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(96, 17)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Text delimiter:"
        '
        'chkContainsHeader
        '
        Me.chkContainsHeader.AutoSize = True
        Me.chkContainsHeader.Checked = True
        Me.chkContainsHeader.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkContainsHeader.Location = New System.Drawing.Point(28, 61)
        Me.chkContainsHeader.Name = "chkContainsHeader"
        Me.chkContainsHeader.Size = New System.Drawing.Size(158, 21)
        Me.chkContainsHeader.TabIndex = 3
        Me.chkContainsHeader.Text = "File contains header"
        Me.chkContainsHeader.UseVisualStyleBackColor = True
        '
        'frmPickDelimiter
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(201, 151)
        Me.Controls.Add(Me.chkContainsHeader)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtDelimiter)
        Me.Controls.Add(Me.btnChoose)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmPickDelimiter"
        Me.Text = "Settings"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnChoose As Windows.Forms.Button
    Friend WithEvents txtDelimiter As Windows.Forms.TextBox
    Friend WithEvents Label1 As Windows.Forms.Label
    Friend WithEvents chkContainsHeader As Windows.Forms.CheckBox
End Class
