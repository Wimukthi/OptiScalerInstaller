<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmDriveSelection
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblIntro = New Label()
        lvDrives = New ThemedListView()
        colDrive = New ColumnHeader()
        btnSelectAll = New Button()
        btnClear = New Button()
        btnScan = New Button()
        btnCancel = New Button()
        SuspendLayout()
        '
        ' lblIntro
        '
        lblIntro.AutoSize = True
        lblIntro.Location = New Point(12, 12)
        lblIntro.Name = "lblIntro"
        lblIntro.Size = New Size(355, 30)
        lblIntro.TabIndex = 0
        lblIntro.Text = "Select one or more drives for deep scan." & vbCrLf & "This scan is launcher-independent and may take longer on large drives."
        '
        ' lvDrives
        '
        lvDrives.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvDrives.BorderStyle = BorderStyle.None
        lvDrives.CheckBoxes = True
        lvDrives.Columns.AddRange(New ColumnHeader() {colDrive})
        lvDrives.FullRowSelect = True
        lvDrives.HeaderStyle = ColumnHeaderStyle.None
        lvDrives.HideSelection = False
        lvDrives.Location = New Point(12, 50)
        lvDrives.MultiSelect = False
        lvDrives.Name = "lvDrives"
        lvDrives.OwnerDraw = False
        lvDrives.ShowGroups = False
        lvDrives.Size = New Size(536, 303)
        lvDrives.TabIndex = 1
        lvDrives.UseCompatibleStateImageBehavior = False
        lvDrives.View = View.Details
        '
        ' colDrive
        '
        colDrive.Text = "Drive"
        colDrive.Width = 520
        '
        ' btnSelectAll
        '
        btnSelectAll.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnSelectAll.Location = New Point(12, 362)
        btnSelectAll.Name = "btnSelectAll"
        btnSelectAll.Size = New Size(96, 28)
        btnSelectAll.TabIndex = 2
        btnSelectAll.Text = "Select all"
        btnSelectAll.UseVisualStyleBackColor = True
        '
        ' btnClear
        '
        btnClear.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnClear.Location = New Point(114, 362)
        btnClear.Name = "btnClear"
        btnClear.Size = New Size(96, 28)
        btnClear.TabIndex = 3
        btnClear.Text = "Clear"
        btnClear.UseVisualStyleBackColor = True
        '
        ' btnScan
        '
        btnScan.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnScan.Location = New Point(350, 362)
        btnScan.Name = "btnScan"
        btnScan.Size = New Size(96, 28)
        btnScan.TabIndex = 4
        btnScan.Text = "Start scan"
        btnScan.UseVisualStyleBackColor = True
        '
        ' btnCancel
        '
        btnCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Location = New Point(452, 362)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(96, 28)
        btnCancel.TabIndex = 5
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        '
        ' frmDriveSelection
        '
        AcceptButton = btnScan
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(560, 402)
        Controls.Add(btnCancel)
        Controls.Add(btnScan)
        Controls.Add(btnClear)
        Controls.Add(btnSelectAll)
        Controls.Add(lvDrives)
        Controls.Add(lblIntro)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "frmDriveSelection"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Select Drives for Deep Scan"
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents lblIntro As Label
    Friend WithEvents lvDrives As ThemedListView
    Friend WithEvents colDrive As ColumnHeader
    Friend WithEvents btnSelectAll As Button
    Friend WithEvents btnClear As Button
    Friend WithEvents btnScan As Button
    Friend WithEvents btnCancel As Button
End Class
