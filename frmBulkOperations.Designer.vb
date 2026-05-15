<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmBulkOperations
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
        lblOperation = New Label()
        cmbOperation = New ComboBox()
        chkIncludeAntiCheat = New CheckBox()
        chkStopOnError = New CheckBox()
        dgvPlans = New DataGridView()
        colBulkSelected = New DataGridViewCheckBoxColumn()
        colBulkGame = New DataGridViewTextBoxColumn()
        colBulkAction = New DataGridViewTextBoxColumn()
        colBulkOptiScaler = New DataGridViewTextBoxColumn()
        colBulkOptiPatcher = New DataGridViewTextBoxColumn()
        colBulkPlatform = New DataGridViewTextBoxColumn()
        colBulkAntiCheat = New DataGridViewTextBoxColumn()
        colBulkPath = New DataGridViewTextBoxColumn()
        colBulkNotes = New DataGridViewTextBoxColumn()
        btnSelectEligible = New Button()
        btnClear = New Button()
        lblSummary = New Label()
        btnRun = New Button()
        btnCancel = New Button()
        CType(dgvPlans, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' lblIntro
        '
        lblIntro.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblIntro.Location = New Point(12, 12)
        lblIntro.Name = "lblIntro"
        lblIntro.Size = New Size(1030, 38)
        lblIntro.TabIndex = 0
        lblIntro.Text = "Bulk operations run sequentially against detected supported games. Select one operation, review eligibility, then choose the games to process. Anti-cheat flagged games are excluded unless explicitly included."
        '
        ' lblOperation
        '
        lblOperation.AutoSize = True
        lblOperation.Location = New Point(12, 61)
        lblOperation.Name = "lblOperation"
        lblOperation.Size = New Size(60, 15)
        lblOperation.TabIndex = 1
        lblOperation.Text = "Operation"
        '
        ' cmbOperation
        '
        cmbOperation.DropDownStyle = ComboBoxStyle.DropDownList
        cmbOperation.FormattingEnabled = True
        cmbOperation.Location = New Point(88, 57)
        cmbOperation.Name = "cmbOperation"
        cmbOperation.Size = New Size(278, 23)
        cmbOperation.TabIndex = 2
        '
        ' chkIncludeAntiCheat
        '
        chkIncludeAntiCheat.AutoSize = True
        chkIncludeAntiCheat.Location = New Point(392, 59)
        chkIncludeAntiCheat.Name = "chkIncludeAntiCheat"
        chkIncludeAntiCheat.Size = New Size(177, 19)
        chkIncludeAntiCheat.TabIndex = 3
        chkIncludeAntiCheat.Text = "Include anti-cheat warnings"
        chkIncludeAntiCheat.UseVisualStyleBackColor = True
        '
        ' chkStopOnError
        '
        chkStopOnError.AutoSize = True
        chkStopOnError.Checked = True
        chkStopOnError.CheckState = CheckState.Checked
        chkStopOnError.Location = New Point(591, 59)
        chkStopOnError.Name = "chkStopOnError"
        chkStopOnError.Size = New Size(119, 19)
        chkStopOnError.TabIndex = 4
        chkStopOnError.Text = "Stop on first error"
        chkStopOnError.UseVisualStyleBackColor = True
        '
        ' dgvPlans
        '
        dgvPlans.AllowUserToAddRows = False
        dgvPlans.AllowUserToDeleteRows = False
        dgvPlans.AllowUserToResizeRows = False
        dgvPlans.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        dgvPlans.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvPlans.Columns.AddRange(New DataGridViewColumn() {colBulkSelected, colBulkGame, colBulkAction, colBulkOptiScaler, colBulkOptiPatcher, colBulkPlatform, colBulkAntiCheat, colBulkPath, colBulkNotes})
        dgvPlans.Location = New Point(12, 90)
        dgvPlans.MultiSelect = False
        dgvPlans.Name = "dgvPlans"
        dgvPlans.RowHeadersVisible = False
        dgvPlans.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvPlans.Size = New Size(1030, 466)
        dgvPlans.TabIndex = 5
        '
        ' colBulkSelected
        '
        colBulkSelected.HeaderText = "Run"
        colBulkSelected.Name = "colBulkSelected"
        colBulkSelected.Width = 45
        '
        ' colBulkGame
        '
        colBulkGame.HeaderText = "Game"
        colBulkGame.Name = "colBulkGame"
        colBulkGame.ReadOnly = True
        colBulkGame.Width = 220
        '
        ' colBulkAction
        '
        colBulkAction.HeaderText = "Action"
        colBulkAction.Name = "colBulkAction"
        colBulkAction.ReadOnly = True
        colBulkAction.Width = 145
        '
        ' colBulkOptiScaler
        '
        colBulkOptiScaler.HeaderText = "OptiScaler"
        colBulkOptiScaler.Name = "colBulkOptiScaler"
        colBulkOptiScaler.ReadOnly = True
        colBulkOptiScaler.Width = 130
        '
        ' colBulkOptiPatcher
        '
        colBulkOptiPatcher.HeaderText = "OptiPatcher"
        colBulkOptiPatcher.Name = "colBulkOptiPatcher"
        colBulkOptiPatcher.ReadOnly = True
        colBulkOptiPatcher.Width = 130
        '
        ' colBulkPlatform
        '
        colBulkPlatform.HeaderText = "Platform"
        colBulkPlatform.Name = "colBulkPlatform"
        colBulkPlatform.ReadOnly = True
        colBulkPlatform.Width = 100
        '
        ' colBulkAntiCheat
        '
        colBulkAntiCheat.HeaderText = "Anti-cheat"
        colBulkAntiCheat.Name = "colBulkAntiCheat"
        colBulkAntiCheat.ReadOnly = True
        colBulkAntiCheat.Width = 110
        '
        ' colBulkPath
        '
        colBulkPath.HeaderText = "Target"
        colBulkPath.Name = "colBulkPath"
        colBulkPath.ReadOnly = True
        colBulkPath.Width = 260
        '
        ' colBulkNotes
        '
        colBulkNotes.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colBulkNotes.HeaderText = "Notes"
        colBulkNotes.Name = "colBulkNotes"
        colBulkNotes.ReadOnly = True
        '
        ' btnSelectEligible
        '
        btnSelectEligible.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnSelectEligible.Location = New Point(12, 568)
        btnSelectEligible.Name = "btnSelectEligible"
        btnSelectEligible.Size = New Size(118, 28)
        btnSelectEligible.TabIndex = 6
        btnSelectEligible.Text = "Select eligible"
        btnSelectEligible.UseVisualStyleBackColor = True
        '
        ' btnClear
        '
        btnClear.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnClear.Location = New Point(138, 568)
        btnClear.Name = "btnClear"
        btnClear.Size = New Size(96, 28)
        btnClear.TabIndex = 7
        btnClear.Text = "Clear"
        btnClear.UseVisualStyleBackColor = True
        '
        ' lblSummary
        '
        lblSummary.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lblSummary.Location = New Point(250, 574)
        lblSummary.Name = "lblSummary"
        lblSummary.Size = New Size(526, 17)
        lblSummary.TabIndex = 8
        lblSummary.Text = "0 eligible, 0 selected."
        '
        ' btnRun
        '
        btnRun.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnRun.Location = New Point(820, 568)
        btnRun.Name = "btnRun"
        btnRun.Size = New Size(108, 28)
        btnRun.TabIndex = 9
        btnRun.Text = "Run selected"
        btnRun.UseVisualStyleBackColor = True
        '
        ' btnCancel
        '
        btnCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Location = New Point(934, 568)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(108, 28)
        btnCancel.TabIndex = 10
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        '
        ' frmBulkOperations
        '
        AcceptButton = btnRun
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnCancel
        ClientSize = New Size(1054, 608)
        Controls.Add(btnCancel)
        Controls.Add(btnRun)
        Controls.Add(lblSummary)
        Controls.Add(btnClear)
        Controls.Add(btnSelectEligible)
        Controls.Add(dgvPlans)
        Controls.Add(chkStopOnError)
        Controls.Add(chkIncludeAntiCheat)
        Controls.Add(cmbOperation)
        Controls.Add(lblOperation)
        Controls.Add(lblIntro)
        MinimumSize = New Size(920, 520)
        Name = "frmBulkOperations"
        StartPosition = FormStartPosition.CenterParent
        Text = "Bulk Operations"
        CType(dgvPlans, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblIntro As System.Windows.Forms.Label
    Friend WithEvents lblOperation As System.Windows.Forms.Label
    Friend WithEvents cmbOperation As System.Windows.Forms.ComboBox
    Friend WithEvents chkIncludeAntiCheat As System.Windows.Forms.CheckBox
    Friend WithEvents chkStopOnError As System.Windows.Forms.CheckBox
    Friend WithEvents dgvPlans As System.Windows.Forms.DataGridView
    Friend WithEvents colBulkSelected As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents colBulkGame As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colBulkAction As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colBulkOptiScaler As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colBulkOptiPatcher As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colBulkPlatform As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colBulkAntiCheat As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colBulkPath As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colBulkNotes As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents btnSelectEligible As System.Windows.Forms.Button
    Friend WithEvents btnClear As System.Windows.Forms.Button
    Friend WithEvents lblSummary As System.Windows.Forms.Label
    Friend WithEvents btnRun As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
End Class
