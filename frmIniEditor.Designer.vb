<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmIniEditor
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
        tableRoot = New TableLayoutPanel()
        grpFile = New ThemedGroupBox()
        btnCreateBackup = New Button()
        btnOpenFolder = New Button()
        btnReload = New Button()
        chkKnownOnly = New CheckBox()
        txtFilter = New ThemedTextBox()
        cmbSectionFilter = New ComboBox()
        lblSectionFilter = New Label()
        lblFilter = New Label()
        lblIniPathValue = New Label()
        lblIniPathTitle = New Label()
        tabModes = New TabControl()
        tabVisual = New TabPage()
        splitVisual = New SplitContainer()
        grpSelection = New ThemedGroupBox()
        tableSelectionRoot = New TableLayoutPanel()
        tableSelectionMeta = New TableLayoutPanel()
        txtSelectedDescription = New ThemedTextBox()
        lblSelectedDescriptionTitle = New Label()
        lblSelectedSourceValue = New Label()
        lblSelectedSourceTitle = New Label()
        lblSelectedAllowedValue = New Label()
        lblSelectedAllowedTitle = New Label()
        lblSelectedDefaultValue = New Label()
        lblSelectedDefaultTitle = New Label()
        lblSelectedValueValue = New Label()
        lblSelectedValueTitle = New Label()
        lblSelectedKeyValue = New Label()
        lblSelectedKeyTitle = New Label()
        lblSelectedSectionValue = New Label()
        lblSelectedSectionTitle = New Label()
        dgvSettings = New DataGridView()
        colSection = New DataGridViewTextBoxColumn()
        colKey = New DataGridViewTextBoxColumn()
        colValue = New DataGridViewTextBoxColumn()
        colKnown = New DataGridViewTextBoxColumn()
        colValidation = New DataGridViewTextBoxColumn()
        colDescription = New DataGridViewTextBoxColumn()
        tabRaw = New TabPage()
        txtRaw = New ThemedTextBox()
        flowButtons = New FlowLayoutPanel()
        btnClose = New Button()
        btnRevert = New Button()
        btnSave = New Button()
        tableRoot.SuspendLayout()
        grpFile.SuspendLayout()
        tabModes.SuspendLayout()
        tabVisual.SuspendLayout()
        CType(splitVisual, System.ComponentModel.ISupportInitialize).BeginInit()
        splitVisual.Panel1.SuspendLayout()
        splitVisual.Panel2.SuspendLayout()
        splitVisual.SuspendLayout()
        grpSelection.SuspendLayout()
        tableSelectionRoot.SuspendLayout()
        tableSelectionMeta.SuspendLayout()
        CType(dgvSettings, System.ComponentModel.ISupportInitialize).BeginInit()
        tabRaw.SuspendLayout()
        flowButtons.SuspendLayout()
        SuspendLayout()
        '
        'tableRoot
        '
        tableRoot.ColumnCount = 1
        tableRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tableRoot.Controls.Add(grpFile, 0, 0)
        tableRoot.Controls.Add(tabModes, 0, 1)
        tableRoot.Controls.Add(flowButtons, 0, 2)
        tableRoot.Dock = DockStyle.Fill
        tableRoot.Location = New Point(0, 0)
        tableRoot.Name = "tableRoot"
        tableRoot.Padding = New Padding(10)
        tableRoot.RowCount = 3
        tableRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 104.0F))
        tableRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tableRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0F))
        tableRoot.Size = New Size(1080, 720)
        tableRoot.TabIndex = 0
        '
        'grpFile
        '
        grpFile.Controls.Add(btnCreateBackup)
        grpFile.Controls.Add(btnOpenFolder)
        grpFile.Controls.Add(btnReload)
        grpFile.Controls.Add(chkKnownOnly)
        grpFile.Controls.Add(txtFilter)
        grpFile.Controls.Add(cmbSectionFilter)
        grpFile.Controls.Add(lblSectionFilter)
        grpFile.Controls.Add(lblFilter)
        grpFile.Controls.Add(lblIniPathValue)
        grpFile.Controls.Add(lblIniPathTitle)
        grpFile.Dock = DockStyle.Fill
        grpFile.Location = New Point(13, 13)
        grpFile.Name = "grpFile"
        grpFile.Size = New Size(1054, 98)
        grpFile.TabIndex = 0
        grpFile.TabStop = False
        grpFile.Text = "INI File"
        '
        'btnCreateBackup
        '
        btnCreateBackup.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnCreateBackup.Location = New Point(938, 70)
        btnCreateBackup.Name = "btnCreateBackup"
        btnCreateBackup.Size = New Size(104, 24)
        btnCreateBackup.TabIndex = 7
        btnCreateBackup.Text = "Create backup"
        btnCreateBackup.UseVisualStyleBackColor = True
        '
        'btnOpenFolder
        '
        btnOpenFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnOpenFolder.Location = New Point(828, 70)
        btnOpenFolder.Name = "btnOpenFolder"
        btnOpenFolder.Size = New Size(104, 24)
        btnOpenFolder.TabIndex = 6
        btnOpenFolder.Text = "Open folder"
        btnOpenFolder.UseVisualStyleBackColor = True
        '
        'btnReload
        '
        btnReload.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnReload.Location = New Point(718, 70)
        btnReload.Name = "btnReload"
        btnReload.Size = New Size(104, 24)
        btnReload.TabIndex = 5
        btnReload.Text = "Reload"
        btnReload.UseVisualStyleBackColor = True
        '
        'chkKnownOnly
        '
        chkKnownOnly.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        chkKnownOnly.AutoSize = True
        chkKnownOnly.Location = New Point(307, 73)
        chkKnownOnly.Name = "chkKnownOnly"
        chkKnownOnly.Size = New Size(117, 19)
        chkKnownOnly.TabIndex = 4
        chkKnownOnly.Text = "Show known only"
        chkKnownOnly.UseVisualStyleBackColor = True
        '
        'txtFilter
        '
        txtFilter.BackColor = SystemColors.Window
        txtFilter.ForeColor = SystemColors.WindowText
        txtFilter.Location = New Point(50, 70)
        txtFilter.MinimumSize = New Size(0, 24)
        txtFilter.Multiline = False
        txtFilter.Name = "txtFilter"
        txtFilter.Padding = New Padding(6, 3, 6, 3)
        txtFilter.ReadOnly = False
        txtFilter.ScrollBars = ScrollBars.None
        txtFilter.Size = New Size(250, 24)
        txtFilter.TabIndex = 3
        txtFilter.WordWrap = True
        '
        'cmbSectionFilter
        '
        cmbSectionFilter.DropDownStyle = ComboBoxStyle.DropDownList
        cmbSectionFilter.FormattingEnabled = True
        cmbSectionFilter.Location = New Point(487, 70)
        cmbSectionFilter.Name = "cmbSectionFilter"
        cmbSectionFilter.Size = New Size(225, 23)
        cmbSectionFilter.TabIndex = 9
        '
        'lblSectionFilter
        '
        lblSectionFilter.AutoSize = True
        lblSectionFilter.Location = New Point(431, 74)
        lblSectionFilter.Name = "lblSectionFilter"
        lblSectionFilter.Size = New Size(46, 15)
        lblSectionFilter.TabIndex = 8
        lblSectionFilter.Text = "Section"
        '
        'lblFilter
        '
        lblFilter.AutoSize = True
        lblFilter.Location = New Point(10, 74)
        lblFilter.Name = "lblFilter"
        lblFilter.Size = New Size(33, 15)
        lblFilter.TabIndex = 2
        lblFilter.Text = "Filter"
        '
        'lblIniPathValue
        '
        lblIniPathValue.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblIniPathValue.AutoEllipsis = True
        lblIniPathValue.Location = New Point(50, 20)
        lblIniPathValue.Name = "lblIniPathValue"
        lblIniPathValue.Size = New Size(992, 24)
        lblIniPathValue.TabIndex = 1
        lblIniPathValue.Text = "-"
        '
        'lblIniPathTitle
        '
        lblIniPathTitle.AutoSize = True
        lblIniPathTitle.Location = New Point(10, 20)
        lblIniPathTitle.Name = "lblIniPathTitle"
        lblIniPathTitle.Size = New Size(30, 15)
        lblIniPathTitle.TabIndex = 0
        lblIniPathTitle.Text = "Path"
        '
        'tabModes
        '
        tabModes.Controls.Add(tabVisual)
        tabModes.Controls.Add(tabRaw)
        tabModes.Dock = DockStyle.Fill
        tabModes.Location = New Point(13, 117)
        tabModes.Name = "tabModes"
        tabModes.SelectedIndex = 0
        tabModes.Size = New Size(1054, 534)
        tabModes.TabIndex = 1
        '
        'tabVisual
        '
        tabVisual.Controls.Add(splitVisual)
        tabVisual.Location = New Point(4, 24)
        tabVisual.Name = "tabVisual"
        tabVisual.Padding = New Padding(4)
        tabVisual.Size = New Size(1046, 506)
        tabVisual.TabIndex = 0
        tabVisual.Text = "Form"
        tabVisual.UseVisualStyleBackColor = True
        '
        'splitVisual
        '
        splitVisual.Dock = DockStyle.Fill
        splitVisual.Location = New Point(4, 4)
        splitVisual.Name = "splitVisual"
        splitVisual.Orientation = Orientation.Horizontal
        '
        'splitVisual.Panel1
        '
        splitVisual.Panel1.Controls.Add(dgvSettings)
        '
        'splitVisual.Panel2
        '
        splitVisual.Panel2.Controls.Add(grpSelection)
        splitVisual.Size = New Size(1038, 498)
        splitVisual.SplitterDistance = 322
        splitVisual.TabIndex = 1
        '
        'grpSelection
        '
        grpSelection.Controls.Add(tableSelectionRoot)
        grpSelection.Dock = DockStyle.Fill
        grpSelection.Location = New Point(0, 0)
        grpSelection.Name = "grpSelection"
        grpSelection.Size = New Size(1038, 172)
        grpSelection.TabIndex = 0
        grpSelection.TabStop = False
        grpSelection.Text = "Selected Setting Details"
        '
        'tableSelectionRoot
        '
        tableSelectionRoot.ColumnCount = 1
        tableSelectionRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tableSelectionRoot.Controls.Add(tableSelectionMeta, 0, 0)
        tableSelectionRoot.Controls.Add(lblSelectedDescriptionTitle, 0, 1)
        tableSelectionRoot.Controls.Add(txtSelectedDescription, 0, 2)
        tableSelectionRoot.Dock = DockStyle.Fill
        tableSelectionRoot.Location = New Point(3, 19)
        tableSelectionRoot.Margin = New Padding(0)
        tableSelectionRoot.Name = "tableSelectionRoot"
        tableSelectionRoot.Padding = New Padding(8, 6, 8, 8)
        tableSelectionRoot.RowCount = 3
        tableSelectionRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 46.0F))
        tableSelectionRoot.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tableSelectionRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tableSelectionRoot.Size = New Size(1032, 150)
        tableSelectionRoot.TabIndex = 0
        '
        'tableSelectionMeta
        '
        tableSelectionMeta.ColumnCount = 6
        tableSelectionMeta.ColumnStyles.Add(New ColumnStyle())
        tableSelectionMeta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333321F))
        tableSelectionMeta.ColumnStyles.Add(New ColumnStyle())
        tableSelectionMeta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333321F))
        tableSelectionMeta.ColumnStyles.Add(New ColumnStyle())
        tableSelectionMeta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.3333321F))
        tableSelectionMeta.Controls.Add(lblSelectedSectionTitle, 0, 0)
        tableSelectionMeta.Controls.Add(lblSelectedSectionValue, 1, 0)
        tableSelectionMeta.Controls.Add(lblSelectedKeyTitle, 2, 0)
        tableSelectionMeta.Controls.Add(lblSelectedKeyValue, 3, 0)
        tableSelectionMeta.Controls.Add(lblSelectedValueTitle, 4, 0)
        tableSelectionMeta.Controls.Add(lblSelectedValueValue, 5, 0)
        tableSelectionMeta.Controls.Add(lblSelectedDefaultTitle, 0, 1)
        tableSelectionMeta.Controls.Add(lblSelectedDefaultValue, 1, 1)
        tableSelectionMeta.Controls.Add(lblSelectedAllowedTitle, 2, 1)
        tableSelectionMeta.Controls.Add(lblSelectedAllowedValue, 3, 1)
        tableSelectionMeta.Controls.Add(lblSelectedSourceTitle, 4, 1)
        tableSelectionMeta.Controls.Add(lblSelectedSourceValue, 5, 1)
        tableSelectionMeta.Dock = DockStyle.Fill
        tableSelectionMeta.Location = New Point(8, 6)
        tableSelectionMeta.Margin = New Padding(0)
        tableSelectionMeta.Name = "tableSelectionMeta"
        tableSelectionMeta.RowCount = 2
        tableSelectionMeta.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
        tableSelectionMeta.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
        tableSelectionMeta.Size = New Size(1016, 46)
        tableSelectionMeta.TabIndex = 0
        '
        'txtSelectedDescription
        '
        txtSelectedDescription.Dock = DockStyle.Fill
        txtSelectedDescription.BackColor = SystemColors.Window
        txtSelectedDescription.ForeColor = SystemColors.WindowText
        txtSelectedDescription.Location = New Point(8, 72)
        txtSelectedDescription.Margin = New Padding(0)
        txtSelectedDescription.MinimumSize = New Size(0, 24)
        txtSelectedDescription.Multiline = True
        txtSelectedDescription.Name = "txtSelectedDescription"
        txtSelectedDescription.Padding = New Padding(6, 3, 6, 3)
        txtSelectedDescription.ReadOnly = True
        txtSelectedDescription.ScrollBars = ScrollBars.Vertical
        txtSelectedDescription.Size = New Size(1016, 70)
        txtSelectedDescription.TabIndex = 2
        txtSelectedDescription.WordWrap = True
        '
        'lblSelectedDescriptionTitle
        '
        lblSelectedDescriptionTitle.Anchor = AnchorStyles.Left
        lblSelectedDescriptionTitle.AutoSize = True
        lblSelectedDescriptionTitle.Location = New Point(8, 58)
        lblSelectedDescriptionTitle.Margin = New Padding(0, 6, 0, 4)
        lblSelectedDescriptionTitle.Name = "lblSelectedDescriptionTitle"
        lblSelectedDescriptionTitle.Size = New Size(67, 15)
        lblSelectedDescriptionTitle.TabIndex = 1
        lblSelectedDescriptionTitle.Text = "Description"
        '
        'lblSelectedSourceValue
        '
        lblSelectedSourceValue.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        lblSelectedSourceValue.AutoEllipsis = True
        lblSelectedSourceValue.Location = New Point(731, 27)
        lblSelectedSourceValue.Margin = New Padding(0, 2, 0, 2)
        lblSelectedSourceValue.Name = "lblSelectedSourceValue"
        lblSelectedSourceValue.Size = New Size(285, 19)
        lblSelectedSourceValue.TabIndex = 11
        lblSelectedSourceValue.Text = "-"
        lblSelectedSourceValue.TextAlign = ContentAlignment.MiddleLeft
        '
        'lblSelectedSourceTitle
        '
        lblSelectedSourceTitle.Anchor = AnchorStyles.Left
        lblSelectedSourceTitle.AutoSize = True
        lblSelectedSourceTitle.Location = New Point(682, 29)
        lblSelectedSourceTitle.Margin = New Padding(0, 2, 6, 2)
        lblSelectedSourceTitle.Name = "lblSelectedSourceTitle"
        lblSelectedSourceTitle.Size = New Size(43, 15)
        lblSelectedSourceTitle.TabIndex = 10
        lblSelectedSourceTitle.Text = "Source"
        '
        'lblSelectedAllowedValue
        '
        lblSelectedAllowedValue.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        lblSelectedAllowedValue.AutoEllipsis = True
        lblSelectedAllowedValue.Location = New Point(373, 27)
        lblSelectedAllowedValue.Margin = New Padding(0, 2, 12, 2)
        lblSelectedAllowedValue.Name = "lblSelectedAllowedValue"
        lblSelectedAllowedValue.Size = New Size(297, 19)
        lblSelectedAllowedValue.TabIndex = 9
        lblSelectedAllowedValue.Text = "-"
        lblSelectedAllowedValue.TextAlign = ContentAlignment.MiddleLeft
        '
        'lblSelectedAllowedTitle
        '
        lblSelectedAllowedTitle.Anchor = AnchorStyles.Left
        lblSelectedAllowedTitle.AutoSize = True
        lblSelectedAllowedTitle.Location = New Point(319, 29)
        lblSelectedAllowedTitle.Margin = New Padding(0, 2, 6, 2)
        lblSelectedAllowedTitle.Name = "lblSelectedAllowedTitle"
        lblSelectedAllowedTitle.Size = New Size(48, 15)
        lblSelectedAllowedTitle.TabIndex = 8
        lblSelectedAllowedTitle.Text = "Allowed"
        '
        'lblSelectedDefaultValue
        '
        lblSelectedDefaultValue.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        lblSelectedDefaultValue.AutoEllipsis = True
        lblSelectedDefaultValue.Location = New Point(60, 27)
        lblSelectedDefaultValue.Margin = New Padding(0, 2, 12, 2)
        lblSelectedDefaultValue.Name = "lblSelectedDefaultValue"
        lblSelectedDefaultValue.Size = New Size(247, 19)
        lblSelectedDefaultValue.TabIndex = 7
        lblSelectedDefaultValue.Text = "-"
        lblSelectedDefaultValue.TextAlign = ContentAlignment.MiddleLeft
        '
        'lblSelectedDefaultTitle
        '
        lblSelectedDefaultTitle.Anchor = AnchorStyles.Left
        lblSelectedDefaultTitle.AutoSize = True
        lblSelectedDefaultTitle.Location = New Point(0, 29)
        lblSelectedDefaultTitle.Margin = New Padding(0, 2, 6, 2)
        lblSelectedDefaultTitle.Name = "lblSelectedDefaultTitle"
        lblSelectedDefaultTitle.Size = New Size(45, 15)
        lblSelectedDefaultTitle.TabIndex = 6
        lblSelectedDefaultTitle.Text = "Default"
        '
        'lblSelectedValueValue
        '
        lblSelectedValueValue.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        lblSelectedValueValue.AutoEllipsis = True
        lblSelectedValueValue.Location = New Point(731, 2)
        lblSelectedValueValue.Margin = New Padding(0, 2, 0, 2)
        lblSelectedValueValue.Name = "lblSelectedValueValue"
        lblSelectedValueValue.Size = New Size(285, 19)
        lblSelectedValueValue.TabIndex = 5
        lblSelectedValueValue.Text = "-"
        lblSelectedValueValue.TextAlign = ContentAlignment.MiddleLeft
        '
        'lblSelectedValueTitle
        '
        lblSelectedValueTitle.Anchor = AnchorStyles.Left
        lblSelectedValueTitle.AutoSize = True
        lblSelectedValueTitle.Location = New Point(682, 4)
        lblSelectedValueTitle.Margin = New Padding(0, 2, 6, 2)
        lblSelectedValueTitle.Name = "lblSelectedValueTitle"
        lblSelectedValueTitle.Size = New Size(41, 15)
        lblSelectedValueTitle.TabIndex = 4
        lblSelectedValueTitle.Text = "Current"
        '
        'lblSelectedKeyValue
        '
        lblSelectedKeyValue.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        lblSelectedKeyValue.AutoEllipsis = True
        lblSelectedKeyValue.Location = New Point(373, 2)
        lblSelectedKeyValue.Margin = New Padding(0, 2, 12, 2)
        lblSelectedKeyValue.Name = "lblSelectedKeyValue"
        lblSelectedKeyValue.Size = New Size(297, 19)
        lblSelectedKeyValue.TabIndex = 3
        lblSelectedKeyValue.Text = "-"
        lblSelectedKeyValue.TextAlign = ContentAlignment.MiddleLeft
        '
        'lblSelectedKeyTitle
        '
        lblSelectedKeyTitle.Anchor = AnchorStyles.Left
        lblSelectedKeyTitle.AutoSize = True
        lblSelectedKeyTitle.Location = New Point(319, 4)
        lblSelectedKeyTitle.Margin = New Padding(0, 2, 6, 2)
        lblSelectedKeyTitle.Name = "lblSelectedKeyTitle"
        lblSelectedKeyTitle.Size = New Size(25, 15)
        lblSelectedKeyTitle.TabIndex = 2
        lblSelectedKeyTitle.Text = "Key"
        '
        'lblSelectedSectionValue
        '
        lblSelectedSectionValue.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        lblSelectedSectionValue.AutoEllipsis = True
        lblSelectedSectionValue.Location = New Point(60, 2)
        lblSelectedSectionValue.Margin = New Padding(0, 2, 12, 2)
        lblSelectedSectionValue.Name = "lblSelectedSectionValue"
        lblSelectedSectionValue.Size = New Size(247, 19)
        lblSelectedSectionValue.TabIndex = 1
        lblSelectedSectionValue.Text = "-"
        lblSelectedSectionValue.TextAlign = ContentAlignment.MiddleLeft
        '
        'lblSelectedSectionTitle
        '
        lblSelectedSectionTitle.Anchor = AnchorStyles.Left
        lblSelectedSectionTitle.AutoSize = True
        lblSelectedSectionTitle.Location = New Point(0, 4)
        lblSelectedSectionTitle.Margin = New Padding(0, 2, 6, 2)
        lblSelectedSectionTitle.Name = "lblSelectedSectionTitle"
        lblSelectedSectionTitle.Size = New Size(46, 15)
        lblSelectedSectionTitle.TabIndex = 0
        lblSelectedSectionTitle.Text = "Section"
        '
        'dgvSettings
        '
        dgvSettings.AllowUserToAddRows = False
        dgvSettings.AllowUserToDeleteRows = False
        dgvSettings.AllowUserToResizeRows = False
        dgvSettings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvSettings.Columns.AddRange(New DataGridViewColumn() {colSection, colKey, colValue, colKnown, colValidation, colDescription})
        dgvSettings.Dock = DockStyle.Fill
        dgvSettings.Location = New Point(0, 0)
        dgvSettings.MultiSelect = False
        dgvSettings.Name = "dgvSettings"
        dgvSettings.RowHeadersVisible = False
        dgvSettings.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvSettings.Size = New Size(1038, 322)
        dgvSettings.TabIndex = 0
        '
        'colSection
        '
        colSection.HeaderText = "Section"
        colSection.Name = "colSection"
        colSection.ReadOnly = True
        colSection.Width = 120
        '
        'colKey
        '
        colKey.HeaderText = "Key"
        colKey.Name = "colKey"
        colKey.ReadOnly = True
        colKey.Width = 170
        '
        'colValue
        '
        colValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colValue.FillWeight = 140.0!
        colValue.HeaderText = "Value"
        colValue.Name = "colValue"
        '
        'colKnown
        '
        colKnown.HeaderText = "Type"
        colKnown.Name = "colKnown"
        colKnown.ReadOnly = True
        colKnown.Width = 80
        '
        'colValidation
        '
        colValidation.HeaderText = "Validation"
        colValidation.Name = "colValidation"
        colValidation.ReadOnly = True
        colValidation.Width = 180
        '
        'colDescription
        '
        colDescription.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colDescription.FillWeight = 180.0!
        colDescription.HeaderText = "Description"
        colDescription.Name = "colDescription"
        colDescription.ReadOnly = True
        '
        'tabRaw
        '
        tabRaw.Controls.Add(txtRaw)
        tabRaw.Location = New Point(4, 24)
        tabRaw.Name = "tabRaw"
        tabRaw.Padding = New Padding(4)
        tabRaw.Size = New Size(1046, 506)
        tabRaw.TabIndex = 1
        tabRaw.Text = "Raw"
        tabRaw.UseVisualStyleBackColor = True
        '
        'txtRaw
        '
        txtRaw.BackColor = SystemColors.Window
        txtRaw.Dock = DockStyle.Fill
        txtRaw.ForeColor = SystemColors.WindowText
        txtRaw.Location = New Point(4, 4)
        txtRaw.MinimumSize = New Size(0, 24)
        txtRaw.Multiline = True
        txtRaw.Name = "txtRaw"
        txtRaw.Padding = New Padding(6, 3, 6, 3)
        txtRaw.ReadOnly = False
        txtRaw.ScrollBars = ScrollBars.Both
        txtRaw.Size = New Size(1038, 498)
        txtRaw.TabIndex = 0
        txtRaw.WordWrap = False
        '
        'flowButtons
        '
        flowButtons.Controls.Add(btnClose)
        flowButtons.Controls.Add(btnRevert)
        flowButtons.Controls.Add(btnSave)
        flowButtons.Dock = DockStyle.Fill
        flowButtons.FlowDirection = FlowDirection.RightToLeft
        flowButtons.Location = New Point(13, 671)
        flowButtons.Name = "flowButtons"
        flowButtons.Size = New Size(1054, 36)
        flowButtons.TabIndex = 2
        '
        'btnClose
        '
        btnClose.Location = New Point(966, 3)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(85, 28)
        btnClose.TabIndex = 0
        btnClose.Text = "Close"
        btnClose.UseVisualStyleBackColor = True
        '
        'btnRevert
        '
        btnRevert.Location = New Point(875, 3)
        btnRevert.Name = "btnRevert"
        btnRevert.Size = New Size(85, 28)
        btnRevert.TabIndex = 1
        btnRevert.Text = "Revert"
        btnRevert.UseVisualStyleBackColor = True
        '
        'btnSave
        '
        btnSave.Location = New Point(784, 3)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(85, 28)
        btnSave.TabIndex = 2
        btnSave.Text = "Save"
        btnSave.UseVisualStyleBackColor = True
        '
        'frmIniEditor
        '
        AutoScaleDimensions = New SizeF(7.0!, 15.0!)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1080, 720)
        Controls.Add(tableRoot)
        MinimizeBox = False
        MinimumSize = New Size(900, 620)
        Name = "frmIniEditor"
        StartPosition = FormStartPosition.CenterParent
        Text = "OptiScaler.ini Editor"
        tableRoot.ResumeLayout(False)
        grpFile.ResumeLayout(False)
        grpFile.PerformLayout()
        tabModes.ResumeLayout(False)
        tabVisual.ResumeLayout(False)
        splitVisual.Panel1.ResumeLayout(False)
        splitVisual.Panel2.ResumeLayout(False)
        CType(splitVisual, System.ComponentModel.ISupportInitialize).EndInit()
        splitVisual.ResumeLayout(False)
        tableSelectionMeta.ResumeLayout(False)
        tableSelectionMeta.PerformLayout()
        tableSelectionRoot.ResumeLayout(False)
        tableSelectionRoot.PerformLayout()
        grpSelection.ResumeLayout(False)
        CType(dgvSettings, System.ComponentModel.ISupportInitialize).EndInit()
        tabRaw.ResumeLayout(False)
        flowButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tableRoot As TableLayoutPanel
    Friend WithEvents grpFile As ThemedGroupBox
    Friend WithEvents lblIniPathTitle As Label
    Friend WithEvents lblIniPathValue As Label
    Friend WithEvents lblFilter As Label
    Friend WithEvents txtFilter As ThemedTextBox
    Friend WithEvents chkKnownOnly As CheckBox
    Friend WithEvents cmbSectionFilter As ComboBox
    Friend WithEvents lblSectionFilter As Label
    Friend WithEvents btnReload As Button
    Friend WithEvents btnOpenFolder As Button
    Friend WithEvents btnCreateBackup As Button
    Friend WithEvents tabModes As TabControl
    Friend WithEvents tabVisual As TabPage
    Friend WithEvents splitVisual As SplitContainer
    Friend WithEvents grpSelection As ThemedGroupBox
    Friend WithEvents tableSelectionRoot As TableLayoutPanel
    Friend WithEvents tableSelectionMeta As TableLayoutPanel
    Friend WithEvents txtSelectedDescription As ThemedTextBox
    Friend WithEvents lblSelectedDescriptionTitle As Label
    Friend WithEvents lblSelectedSourceValue As Label
    Friend WithEvents lblSelectedSourceTitle As Label
    Friend WithEvents lblSelectedAllowedValue As Label
    Friend WithEvents lblSelectedAllowedTitle As Label
    Friend WithEvents lblSelectedDefaultValue As Label
    Friend WithEvents lblSelectedDefaultTitle As Label
    Friend WithEvents lblSelectedValueValue As Label
    Friend WithEvents lblSelectedValueTitle As Label
    Friend WithEvents lblSelectedKeyValue As Label
    Friend WithEvents lblSelectedKeyTitle As Label
    Friend WithEvents lblSelectedSectionValue As Label
    Friend WithEvents lblSelectedSectionTitle As Label
    Friend WithEvents dgvSettings As DataGridView
    Friend WithEvents colSection As DataGridViewTextBoxColumn
    Friend WithEvents colKey As DataGridViewTextBoxColumn
    Friend WithEvents colValue As DataGridViewTextBoxColumn
    Friend WithEvents colKnown As DataGridViewTextBoxColumn
    Friend WithEvents colValidation As DataGridViewTextBoxColumn
    Friend WithEvents colDescription As DataGridViewTextBoxColumn
    Friend WithEvents tabRaw As TabPage
    Friend WithEvents txtRaw As ThemedTextBox
    Friend WithEvents flowButtons As FlowLayoutPanel
    Friend WithEvents btnClose As Button
    Friend WithEvents btnRevert As Button
    Friend WithEvents btnSave As Button
End Class
