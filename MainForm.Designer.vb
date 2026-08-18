
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
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
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        mainLayout = New TableLayoutPanel()
        tabMain = New TabControl()
        tabCompatibility = New TabPage()
        lvCompatibility = New ThemedListView()
        colCompatName = New ColumnHeader()
        colCompatDetected = New ColumnHeader()
        colCompatInstalled = New ColumnHeader()
        colCompatPatcher = New ColumnHeader()
        colCompatPlatform = New ColumnHeader()
        colCompatAntiCheat = New ColumnHeader()
        colCompatPath = New ColumnHeader()
        compatFooterPanel = New Panel()
        compatActionsPanel = New FlowLayoutPanel()
        btnCompatOpenFolder = New Button()
        btnCompatEditIni = New Button()
        btnCompatInstallUpdate = New Button()
        btnCompatUninstall = New Button()
        btnCompatInstallPatcher = New Button()
        btnCompatRemovePatcher = New Button()
        btnCompatCopyInfo = New Button()
        compatStatusPanel = New Panel()
        lblCompatibilityNote = New Label()
        chkHideNonDetected = New CheckBox()
        compatHeaderPanel = New Panel()
        compatHeaderLeftPanel = New TableLayoutPanel()
        lblSearch = New Label()
        txtGameSearch = New ThemedTextBox()
        compatHeaderRightPanel = New FlowLayoutPanel()
        btnScanDetected = New Button()
        btnDeepScanDrives = New Button()
        btnBulkActions = New Button()
        btnUseDetected = New Button()
        btnRefreshCompatibility = New Button()
        btnOpenWiki = New Button()
        tabInstall = New TabPage()
        installLayout = New TableLayoutPanel()
        grpGame = New ThemedGroupBox()
        lblGameExe = New Label()
        txtGameExe = New ThemedTextBox()
        btnBrowseGameExe = New Button()
        lblGameFolderLabel = New Label()
        txtGameFolder = New ThemedTextBox()
        lblEngineWarning = New Label()
        grpSource = New ThemedGroupBox()
        rbStable = New RadioButton()
        lblStableInfo = New Label()
        rbNightly = New RadioButton()
        lblNightlyInfo = New Label()
        rbLocal = New RadioButton()
        txtLocalArchive = New ThemedTextBox()
        btnBrowseArchive = New Button()
        btnRefreshReleases = New Button()
        grpHook = New ThemedGroupBox()
        lblHookName = New Label()
        cmbHookName = New ComboBox()
        lblHookHint = New Label()
        grpGpu = New ThemedGroupBox()
        rbGpuNvidia = New RadioButton()
        rbGpuAmdIntel = New RadioButton()
        chkDlssInputs = New CheckBox()
        lblDlssHint = New Label()
        grpFg = New ThemedGroupBox()
        lblFgType = New Label()
        cmbFgType = New ComboBox()
        lblFgHint = New Label()
        grpBehavior = New ThemedGroupBox()
        lblConflictMode = New Label()
        cmbConflictMode = New ComboBox()
        chkPreserveIni = New CheckBox()
        lblBehaviorHint = New Label()
        grpActions = New ThemedGroupBox()
        btnInstall = New Button()
        btnUninstall = New Button()
        btnOpenGameFolder = New Button()
        btnEditIni = New Button()
        lblInstalledStatus = New Label()
        chkInstallOptiPatcher = New CheckBox()
        lblInstallOptiPatcherStatus = New Label()
        lblOnlineWarning = New Label()
        lblActionNote = New Label()
        tabAddons = New TabPage()
        addonsLayout = New TableLayoutPanel()
        grpFakenvapi = New ThemedGroupBox()
        lblFakenvapiFolder = New Label()
        txtFakenvapiFolder = New ThemedTextBox()
        btnBrowseFakenvapiFolder = New Button()
        lblFakenvapiHint = New Label()
        grpNukem = New ThemedGroupBox()
        lblNukemDll = New Label()
        txtNukemDll = New ThemedTextBox()
        btnBrowseNukemDll = New Button()
        lblNukemHint = New Label()
        grpNvngx = New ThemedGroupBox()
        lblNvngxDll = New Label()
        txtNvngxDll = New ThemedTextBox()
        btnBrowseNvngx = New Button()
        lblNvngxHint = New Label()
        grpReshade = New ThemedGroupBox()
        chkEnableReshade = New CheckBox()
        lblReshadeDll = New Label()
        txtReshadeDll = New ThemedTextBox()
        btnBrowseReshade = New Button()
        lblReshadeHint = New Label()
        grpSpecialK = New ThemedGroupBox()
        chkEnableSpecialK = New CheckBox()
        lblSpecialKDll = New Label()
        txtSpecialKDll = New ThemedTextBox()
        btnBrowseSpecialK = New Button()
        chkCreateSpecialKMarker = New CheckBox()
        lblSpecialKHint = New Label()
        grpAsi = New ThemedGroupBox()
        chkLoadAsiPlugins = New CheckBox()
        lblPluginsPath = New Label()
        txtPluginsPath = New ThemedTextBox()
        btnBrowsePluginsPath = New Button()
        lblAsiHint = New Label()
        lblOptiPatcherSource = New Label()
        cmbOptiPatcherSource = New ComboBox()
        btnOptiPatcherRefresh = New Button()
        lblOptiPatcherRelease = New Label()
        txtOptiPatcherLocalFile = New ThemedTextBox()
        btnBrowseOptiPatcherLocal = New Button()
        btnInstallOptiPatcher = New Button()
        btnRemoveOptiPatcher = New Button()
        lblOptiPatcherStatus = New Label()
        tabExperimental = New TabPage()
        experimentalLayout = New TableLayoutPanel()
        grpFsr4Package = New ThemedGroupBox()
        lblFsr4PackageFolder = New Label()
        txtFsr4PackageFolder = New ThemedTextBox()
        btnBrowseFsr4PackageFolder = New Button()
        lblFsr4PackageHint = New Label()
        grpFsr4Options = New ThemedGroupBox()
        chkFsr4EnableUpdate = New CheckBox()
        chkFsr4EnableAgility = New CheckBox()
        chkFsr4ForceInt8 = New CheckBox()
        lblFsr4OptionsHint = New Label()
        grpFsr4Actions = New ThemedGroupBox()
        lblFsr4TargetGame = New Label()
        txtFsr4TargetGameFolder = New ThemedTextBox()
        btnFsr4PickGame = New Button()
        btnFsr4Apply = New Button()
        btnFsr4Remove = New Button()
        btnFsr4RefreshStatus = New Button()
        lblFsr4Status = New Label()
        lblFsr4ActionHint = New Label()
        lblFsr4DetectedGames = New Label()
        btnFsr4ScanDetectedGames = New Button()
        btnFsr4UseSelectedGame = New Button()
        btnFsr4BrowseGameExe = New Button()
        lvFsr4DetectedGames = New ThemedListView()
        colFsr4Game = New ColumnHeader()
        colFsr4Platform = New ColumnHeader()
        colFsr4Installed = New ColumnHeader()
        colFsr4DetectedPath = New ColumnHeader()
        tabSettings = New TabPage()
        grpSettings = New ThemedGroupBox()
        lblCompatibilityListUrl = New Label()
        txtCompatibilityListUrl = New ThemedTextBox()
        lblWikiBaseUrl = New Label()
        txtWikiBaseUrl = New ThemedTextBox()
        lblStableReleaseUrl = New Label()
        txtStableReleaseUrl = New ThemedTextBox()
        lblNightlyReleaseUrl = New Label()
        txtNightlyReleaseUrl = New ThemedTextBox()
        lblInstallerReleaseUrl = New Label()
        txtInstallerReleaseUrl = New ThemedTextBox()
        lblGameProfilesCatalogUrl = New Label()
        txtGameProfilesCatalogUrl = New ThemedTextBox()
        flpSettingsToggles = New FlowLayoutPanel()
        chkAutoRefreshCompatibilityOnStartup = New CheckBox()
        chkAutoRefreshGameProfilesOnStartup = New CheckBox()
        chkAutoCheckInstallerUpdates = New CheckBox()
        chkShowExperimentalTabOnUnsupportedGpu = New CheckBox()
        lblDefaultIniPath = New Label()
        txtDefaultIniPath = New ThemedTextBox()
        btnBrowseDefaultIni = New Button()
        lblDefaultIniMode = New Label()
        cmbDefaultIniMode = New ComboBox()
        grpDefaultInstall = New ThemedGroupBox()
        lblDefaultPreset = New Label()
        cmbDefaultPreset = New ComboBox()
        lblDefaultHookName = New Label()
        cmbDefaultHookName = New ComboBox()
        lblDefaultGpuVendor = New Label()
        cmbDefaultGpuVendor = New ComboBox()
        lblDefaultDlssInputs = New Label()
        chkDefaultDlssInputs = New CheckBox()
        lblDefaultFgType = New Label()
        cmbDefaultFgType = New ComboBox()
        lblDefaultConflictMode = New Label()
        cmbDefaultConflictMode = New ComboBox()
        btnApplyDefaults = New Button()
        btnSaveSettings = New Button()
        btnReloadSettings = New Button()
        btnLoadDefaults = New Button()
        btnOpenSettingsFile = New Button()
        btnCheckForUpdates = New Button()
        btnAbout = New Button()
        btnExportDiagnostics = New Button()
        lblUpdateNotice = New Label()
        lblSettingsPath = New Label()
        DarkThemeCheckBox = New CheckBox()
        grpLog = New ThemedGroupBox()
        txtLog = New ThemedRichTextBox()
        logHeaderPanel = New FlowLayoutPanel()
        lblLogFilter = New Label()
        txtLogFilter = New ThemedTextBox()
        lblLogSeverity = New Label()
        cmbLogSeverity = New ComboBox()
        btnLogCopy = New Button()
        btnLogSave = New Button()
        btnLogClear = New Button()
        statusStrip = New StatusStrip()
        toolStatusLabel = New ToolStripStatusLabel()
        toolDetectedLabel = New ToolStripStatusLabel()
        toolProgressBar = New ToolStripProgressBar()
        toolCancelButton = New ToolStripButton()
        toolTip = New ToolTip(components)
        compatContextMenu = New ContextMenuStrip(components)
        mnuCompatUseDetected = New ToolStripMenuItem()
        mnuCompatOpenFolder = New ToolStripMenuItem()
        mnuCompatEditIni = New ToolStripMenuItem()
        mnuCompatSep1 = New ToolStripSeparator()
        mnuCompatInstallUpdate = New ToolStripMenuItem()
        mnuCompatUninstall = New ToolStripMenuItem()
        mnuCompatInstallPatcher = New ToolStripMenuItem()
        mnuCompatRemovePatcher = New ToolStripMenuItem()
        mnuCompatSep2 = New ToolStripSeparator()
        mnuCompatOpenWiki = New ToolStripMenuItem()
        mnuCompatCopyInfo = New ToolStripMenuItem()
        mainLayout.SuspendLayout()
        tabMain.SuspendLayout()
        tabCompatibility.SuspendLayout()
        compatFooterPanel.SuspendLayout()
        compatActionsPanel.SuspendLayout()
        compatStatusPanel.SuspendLayout()
        compatHeaderPanel.SuspendLayout()
        compatHeaderLeftPanel.SuspendLayout()
        compatHeaderRightPanel.SuspendLayout()
        tabInstall.SuspendLayout()
        installLayout.SuspendLayout()
        grpGame.SuspendLayout()
        grpSource.SuspendLayout()
        grpHook.SuspendLayout()
        grpGpu.SuspendLayout()
        grpFg.SuspendLayout()
        grpBehavior.SuspendLayout()
        grpActions.SuspendLayout()
        tabAddons.SuspendLayout()
        addonsLayout.SuspendLayout()
        grpFakenvapi.SuspendLayout()
        grpNukem.SuspendLayout()
        grpNvngx.SuspendLayout()
        grpReshade.SuspendLayout()
        grpSpecialK.SuspendLayout()
        grpAsi.SuspendLayout()
        tabExperimental.SuspendLayout()
        experimentalLayout.SuspendLayout()
        grpFsr4Package.SuspendLayout()
        grpFsr4Options.SuspendLayout()
        grpFsr4Actions.SuspendLayout()
        tabSettings.SuspendLayout()
        grpSettings.SuspendLayout()
        flpSettingsToggles.SuspendLayout()
        grpDefaultInstall.SuspendLayout()
        grpLog.SuspendLayout()
        logHeaderPanel.SuspendLayout()
        statusStrip.SuspendLayout()
        SuspendLayout()
        ' 
        ' mainLayout
        ' 
        mainLayout.ColumnCount = 1
        mainLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        mainLayout.Controls.Add(tabMain, 0, 0)
        mainLayout.Controls.Add(grpLog, 0, 1)
        mainLayout.Dock = DockStyle.Fill
        mainLayout.Location = New Point(0, 0)
        mainLayout.Name = "mainLayout"
        mainLayout.Padding = New Padding(8, 0, 8, 8)
        mainLayout.RowCount = 2
        mainLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        mainLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 160F))
        mainLayout.Size = New Size(1270, 875)
        mainLayout.TabIndex = 0
        ' 
        ' tabMain
        ' 
        tabMain.Controls.Add(tabCompatibility)
        tabMain.Controls.Add(tabInstall)
        tabMain.Controls.Add(tabAddons)
        tabMain.Controls.Add(tabExperimental)
        tabMain.Controls.Add(tabSettings)
        tabMain.Dock = DockStyle.Fill
        tabMain.Location = New Point(8, 0)
        tabMain.Margin = New Padding(0)
        tabMain.Name = "tabMain"
        tabMain.SelectedIndex = 0
        tabMain.Size = New Size(1254, 707)
        tabMain.TabIndex = 1
        ' 
        ' tabCompatibility
        ' 
        tabCompatibility.Controls.Add(lvCompatibility)
        tabCompatibility.Controls.Add(compatFooterPanel)
        tabCompatibility.Controls.Add(compatHeaderPanel)
        tabCompatibility.Location = New Point(4, 24)
        tabCompatibility.Name = "tabCompatibility"
        tabCompatibility.Padding = New Padding(3)
        tabCompatibility.Size = New Size(1246, 679)
        tabCompatibility.TabIndex = 0
        tabCompatibility.Text = "Game Detection"
        tabCompatibility.UseVisualStyleBackColor = True
        ' 
        ' lvCompatibility
        ' 
        lvCompatibility.Columns.AddRange(New ColumnHeader() {colCompatName, colCompatDetected, colCompatInstalled, colCompatPatcher, colCompatPlatform, colCompatAntiCheat, colCompatPath})
        lvCompatibility.Dock = DockStyle.Fill
        lvCompatibility.FullRowSelect = True
        lvCompatibility.Location = New Point(3, 47)
        lvCompatibility.MultiSelect = False
        lvCompatibility.Name = "lvCompatibility"
        lvCompatibility.OwnerDraw = True
        lvCompatibility.Size = New Size(1240, 605)
        lvCompatibility.TabIndex = 6
        lvCompatibility.ContextMenuStrip = compatContextMenu
        lvCompatibility.UseCompatibleStateImageBehavior = False
        lvCompatibility.View = View.Details
        '
        ' compatContextMenu
        '
        compatContextMenu.Items.AddRange(New ToolStripItem() {mnuCompatUseDetected, mnuCompatOpenFolder, mnuCompatEditIni, mnuCompatSep1, mnuCompatInstallUpdate, mnuCompatUninstall, mnuCompatInstallPatcher, mnuCompatRemovePatcher, mnuCompatSep2, mnuCompatOpenWiki, mnuCompatCopyInfo})
        compatContextMenu.Name = "compatContextMenu"
        compatContextMenu.Size = New Size(211, 214)
        '
        ' mnuCompatUseDetected
        '
        mnuCompatUseDetected.Name = "mnuCompatUseDetected"
        mnuCompatUseDetected.Size = New Size(210, 22)
        mnuCompatUseDetected.Text = "Use selected"
        '
        ' mnuCompatOpenFolder
        '
        mnuCompatOpenFolder.Name = "mnuCompatOpenFolder"
        mnuCompatOpenFolder.Size = New Size(210, 22)
        mnuCompatOpenFolder.Text = "Open game folder"
        '
        ' mnuCompatEditIni
        '
        mnuCompatEditIni.Name = "mnuCompatEditIni"
        mnuCompatEditIni.Size = New Size(210, 22)
        mnuCompatEditIni.Text = "Edit OptiScaler.ini"
        '
        ' mnuCompatSep1
        '
        mnuCompatSep1.Name = "mnuCompatSep1"
        mnuCompatSep1.Size = New Size(207, 6)
        '
        ' mnuCompatInstallUpdate
        '
        mnuCompatInstallUpdate.Name = "mnuCompatInstallUpdate"
        mnuCompatInstallUpdate.Size = New Size(210, 22)
        mnuCompatInstallUpdate.Text = "Quick install OptiScaler"
        '
        ' mnuCompatUninstall
        '
        mnuCompatUninstall.Name = "mnuCompatUninstall"
        mnuCompatUninstall.Size = New Size(210, 22)
        mnuCompatUninstall.Text = "Uninstall OptiScaler"
        '
        ' mnuCompatInstallPatcher
        '
        mnuCompatInstallPatcher.Name = "mnuCompatInstallPatcher"
        mnuCompatInstallPatcher.Size = New Size(210, 22)
        mnuCompatInstallPatcher.Text = "Install/Update OptiPatcher"
        '
        ' mnuCompatRemovePatcher
        '
        mnuCompatRemovePatcher.Name = "mnuCompatRemovePatcher"
        mnuCompatRemovePatcher.Size = New Size(210, 22)
        mnuCompatRemovePatcher.Text = "Remove OptiPatcher"
        '
        ' mnuCompatSep2
        '
        mnuCompatSep2.Name = "mnuCompatSep2"
        mnuCompatSep2.Size = New Size(207, 6)
        '
        ' mnuCompatOpenWiki
        '
        mnuCompatOpenWiki.Name = "mnuCompatOpenWiki"
        mnuCompatOpenWiki.Size = New Size(210, 22)
        mnuCompatOpenWiki.Text = "Open wiki page"
        '
        ' mnuCompatCopyInfo
        '
        mnuCompatCopyInfo.Name = "mnuCompatCopyInfo"
        mnuCompatCopyInfo.Size = New Size(210, 22)
        mnuCompatCopyInfo.Text = "Copy game info"
        ' 
        ' colCompatName
        ' 
        colCompatName.Text = "Game"
        colCompatName.Width = 280
        ' 
        ' colCompatDetected
        ' 
        colCompatDetected.Text = "Detected"
        colCompatDetected.Width = 80
        ' 
        ' colCompatInstalled
        ' 
        colCompatInstalled.Text = "OptiScaler"
        colCompatInstalled.Width = 150
        ' 
        ' colCompatPatcher
        ' 
        colCompatPatcher.Text = "OptiPatcher"
        colCompatPatcher.Width = 140
        ' 
        ' colCompatPlatform
        ' 
        colCompatPlatform.Text = "Platform"
        colCompatPlatform.Width = 110
        ' 
        ' colCompatAntiCheat
        ' 
        colCompatAntiCheat.Text = "Anti-cheat"
        colCompatAntiCheat.Width = 130
        ' 
        ' colCompatPath
        ' 
        colCompatPath.Text = "Install Path"
        colCompatPath.Width = 360
        ' 
        ' compatFooterPanel
        '
        compatFooterPanel.Controls.Add(compatActionsPanel)
        compatFooterPanel.Controls.Add(compatStatusPanel)
        compatFooterPanel.Dock = DockStyle.Bottom
        compatFooterPanel.Location = New Point(3, 612)
        compatFooterPanel.Name = "compatFooterPanel"
        compatFooterPanel.Size = New Size(1240, 64)
        compatFooterPanel.TabIndex = 2
        '
        ' compatActionsPanel
        ' 
        compatActionsPanel.Controls.Add(btnUseDetected)
        compatActionsPanel.Controls.Add(btnCompatOpenFolder)
        compatActionsPanel.Controls.Add(btnCompatEditIni)
        compatActionsPanel.Controls.Add(btnCompatInstallUpdate)
        compatActionsPanel.Controls.Add(btnCompatUninstall)
        compatActionsPanel.Controls.Add(btnCompatInstallPatcher)
        compatActionsPanel.Controls.Add(btnCompatRemovePatcher)
        compatActionsPanel.Controls.Add(btnCompatCopyInfo)
        compatActionsPanel.Dock = DockStyle.Fill
        compatActionsPanel.Location = New Point(0, 0)
        compatActionsPanel.Name = "compatActionsPanel"
        compatActionsPanel.Padding = New Padding(8, 4, 8, 4)
        compatActionsPanel.Size = New Size(1240, 40)
        compatActionsPanel.TabIndex = 10
        compatActionsPanel.WrapContents = False
        '
        ' btnUseDetected
        '
        btnUseDetected.Anchor = AnchorStyles.None
        btnUseDetected.Location = New Point(8, 6)
        btnUseDetected.Margin = New Padding(0)
        btnUseDetected.Name = "btnUseDetected"
        btnUseDetected.Size = New Size(130, 27)
        btnUseDetected.TabIndex = 6
        btnUseDetected.Text = "Use selected"
        btnUseDetected.UseVisualStyleBackColor = True
        '
        ' btnCompatOpenFolder
        '
        btnCompatOpenFolder.Anchor = AnchorStyles.None
        btnCompatOpenFolder.Location = New Point(144, 6)
        btnCompatOpenFolder.Margin = New Padding(6, 0, 0, 0)
        btnCompatOpenFolder.Name = "btnCompatOpenFolder"
        btnCompatOpenFolder.Size = New Size(130, 27)
        btnCompatOpenFolder.TabIndex = 7
        btnCompatOpenFolder.Text = "Open game folder"
        btnCompatOpenFolder.UseVisualStyleBackColor = True
        '
        ' btnCompatEditIni
        '
        btnCompatEditIni.Anchor = AnchorStyles.None
        btnCompatEditIni.Location = New Point(280, 6)
        btnCompatEditIni.Margin = New Padding(6, 0, 0, 0)
        btnCompatEditIni.Name = "btnCompatEditIni"
        btnCompatEditIni.Size = New Size(130, 27)
        btnCompatEditIni.TabIndex = 8
        btnCompatEditIni.Text = "Edit OptiScaler.ini"
        btnCompatEditIni.UseVisualStyleBackColor = True
        '
        ' btnCompatInstallUpdate
        '
        btnCompatInstallUpdate.Anchor = AnchorStyles.None
        btnCompatInstallUpdate.Location = New Point(416, 6)
        btnCompatInstallUpdate.Margin = New Padding(6, 0, 0, 0)
        btnCompatInstallUpdate.Name = "btnCompatInstallUpdate"
        btnCompatInstallUpdate.Size = New Size(145, 27)
        btnCompatInstallUpdate.TabIndex = 9
        btnCompatInstallUpdate.Text = "Quick install"
        btnCompatInstallUpdate.UseVisualStyleBackColor = True
        '
        ' btnCompatUninstall
        '
        btnCompatUninstall.Anchor = AnchorStyles.None
        btnCompatUninstall.Location = New Point(567, 6)
        btnCompatUninstall.Margin = New Padding(6, 0, 0, 0)
        btnCompatUninstall.Name = "btnCompatUninstall"
        btnCompatUninstall.Size = New Size(130, 27)
        btnCompatUninstall.TabIndex = 10
        btnCompatUninstall.Text = "Uninstall OptiScaler"
        btnCompatUninstall.UseVisualStyleBackColor = True
        '
        ' btnCompatInstallPatcher
        '
        btnCompatInstallPatcher.Anchor = AnchorStyles.None
        btnCompatInstallPatcher.Location = New Point(703, 6)
        btnCompatInstallPatcher.Margin = New Padding(6, 0, 0, 0)
        btnCompatInstallPatcher.Name = "btnCompatInstallPatcher"
        btnCompatInstallPatcher.Size = New Size(145, 27)
        btnCompatInstallPatcher.TabIndex = 11
        btnCompatInstallPatcher.Text = "Install/Update OptiPatcher"
        btnCompatInstallPatcher.UseVisualStyleBackColor = True
        '
        ' btnCompatRemovePatcher
        '
        btnCompatRemovePatcher.Anchor = AnchorStyles.None
        btnCompatRemovePatcher.Location = New Point(854, 6)
        btnCompatRemovePatcher.Margin = New Padding(6, 0, 0, 0)
        btnCompatRemovePatcher.Name = "btnCompatRemovePatcher"
        btnCompatRemovePatcher.Size = New Size(145, 27)
        btnCompatRemovePatcher.TabIndex = 12
        btnCompatRemovePatcher.Text = "Remove OptiPatcher"
        btnCompatRemovePatcher.UseVisualStyleBackColor = True
        '
        ' btnCompatCopyInfo
        '
        btnCompatCopyInfo.Anchor = AnchorStyles.None
        btnCompatCopyInfo.Location = New Point(1005, 6)
        btnCompatCopyInfo.Margin = New Padding(6, 0, 0, 0)
        btnCompatCopyInfo.Name = "btnCompatCopyInfo"
        btnCompatCopyInfo.Size = New Size(130, 27)
        btnCompatCopyInfo.TabIndex = 13
        btnCompatCopyInfo.Text = "Copy game info"
        btnCompatCopyInfo.UseVisualStyleBackColor = True
        '
        ' compatStatusPanel
        '
        compatStatusPanel.Controls.Add(lblCompatibilityNote)
        compatStatusPanel.Controls.Add(chkHideNonDetected)
        compatStatusPanel.Dock = DockStyle.Bottom
        compatStatusPanel.Location = New Point(0, 40)
        compatStatusPanel.Name = "compatStatusPanel"
        compatStatusPanel.Size = New Size(1240, 24)
        compatStatusPanel.TabIndex = 11
        ' 
        ' lblCompatibilityNote
        ' 
        lblCompatibilityNote.AutoSize = True
        lblCompatibilityNote.Location = New Point(12, 4)
        lblCompatibilityNote.Name = "lblCompatibilityNote"
        lblCompatibilityNote.Size = New Size(532, 15)
        lblCompatibilityNote.TabIndex = 8
        lblCompatibilityNote.Text = "List shows tested games only. Detected/Anti-cheat columns are best-effort and may be incomplete."
        ' 
        ' chkHideNonDetected
        ' 
        chkHideNonDetected.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        chkHideNonDetected.AutoSize = True
        chkHideNonDetected.Location = New Point(1111, 2)
        chkHideNonDetected.Margin = New Padding(0)
        chkHideNonDetected.Name = "chkHideNonDetected"
        chkHideNonDetected.Size = New Size(121, 19)
        chkHideNonDetected.TabIndex = 9
        chkHideNonDetected.Text = "Hide non-detected"
        chkHideNonDetected.UseVisualStyleBackColor = True
        ' 
        ' compatHeaderPanel
        ' 
        compatHeaderPanel.Controls.Add(compatHeaderLeftPanel)
        compatHeaderPanel.Controls.Add(compatHeaderRightPanel)
        compatHeaderPanel.Dock = DockStyle.Top
        compatHeaderPanel.Location = New Point(3, 3)
        compatHeaderPanel.Name = "compatHeaderPanel"
        compatHeaderPanel.Padding = New Padding(6)
        compatHeaderPanel.Size = New Size(1240, 44)
        compatHeaderPanel.TabIndex = 0
        ' 
        ' compatHeaderLeftPanel
        ' 
        compatHeaderLeftPanel.ColumnCount = 2
        compatHeaderLeftPanel.ColumnStyles.Add(New ColumnStyle())
        compatHeaderLeftPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        compatHeaderLeftPanel.Controls.Add(txtGameSearch, 1, 0)
        compatHeaderLeftPanel.Controls.Add(lblSearch, 0, 0)
        compatHeaderLeftPanel.Dock = DockStyle.Fill
        compatHeaderLeftPanel.Location = New Point(6, 6)
        compatHeaderLeftPanel.Margin = New Padding(0)
        compatHeaderLeftPanel.Name = "compatHeaderLeftPanel"
        compatHeaderLeftPanel.RowCount = 1
        compatHeaderLeftPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        compatHeaderLeftPanel.Size = New Size(540, 32)
        compatHeaderLeftPanel.TabIndex = 0
        ' 
        ' lblSearch
        ' 
        lblSearch.AutoSize = True
        lblSearch.Location = New Point(0, 8)
        lblSearch.Margin = New Padding(0, 8, 6, 0)
        lblSearch.Name = "lblSearch"
        lblSearch.Size = New Size(75, 15)
        lblSearch.TabIndex = 0
        lblSearch.Text = "Search game"
        ' 
        ' txtGameSearch
        ' 
        txtGameSearch.BackColor = SystemColors.Window
        txtGameSearch.Dock = DockStyle.Fill
        txtGameSearch.ForeColor = SystemColors.WindowText
        txtGameSearch.Location = New Point(81, 4)
        txtGameSearch.Margin = New Padding(0, 4, 0, 4)
        txtGameSearch.MinimumSize = New Size(0, 24)
        txtGameSearch.Name = "txtGameSearch"
        txtGameSearch.Padding = New Padding(6, 3, 6, 3)
        txtGameSearch.Size = New Size(459, 24)
        txtGameSearch.TabIndex = 1
        ' 
        ' compatHeaderRightPanel
        ' 
        compatHeaderRightPanel.AutoSize = True
        compatHeaderRightPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink
        compatHeaderRightPanel.Controls.Add(btnScanDetected)
        compatHeaderRightPanel.Controls.Add(btnDeepScanDrives)
        compatHeaderRightPanel.Controls.Add(btnBulkActions)
        compatHeaderRightPanel.Controls.Add(btnRefreshCompatibility)
        compatHeaderRightPanel.Controls.Add(btnOpenWiki)
        compatHeaderRightPanel.Dock = DockStyle.Right
        compatHeaderRightPanel.Location = New Point(555, 6)
        compatHeaderRightPanel.Margin = New Padding(0)
        compatHeaderRightPanel.Name = "compatHeaderRightPanel"
        compatHeaderRightPanel.Size = New Size(679, 32)
        compatHeaderRightPanel.TabIndex = 1
        compatHeaderRightPanel.WrapContents = False
        ' 
        ' btnScanDetected
        ' 
        btnScanDetected.Anchor = AnchorStyles.None
        btnScanDetected.Location = New Point(0, 0)
        btnScanDetected.Margin = New Padding(0)
        btnScanDetected.Name = "btnScanDetected"
        btnScanDetected.Size = New Size(134, 27)
        btnScanDetected.TabIndex = 2
        btnScanDetected.Text = "Scan installed games"
        btnScanDetected.UseVisualStyleBackColor = True
        ' 
        ' btnDeepScanDrives
        ' 
        btnDeepScanDrives.Anchor = AnchorStyles.None
        btnDeepScanDrives.Location = New Point(140, 0)
        btnDeepScanDrives.Margin = New Padding(6, 0, 0, 0)
        btnDeepScanDrives.Name = "btnDeepScanDrives"
        btnDeepScanDrives.Size = New Size(130, 27)
        btnDeepScanDrives.TabIndex = 3
        btnDeepScanDrives.Text = "Add game manually"
        btnDeepScanDrives.UseVisualStyleBackColor = True
        '
        ' btnBulkActions
        '
        btnBulkActions.Anchor = AnchorStyles.None
        btnBulkActions.Location = New Point(276, 0)
        btnBulkActions.Margin = New Padding(6, 0, 0, 0)
        btnBulkActions.Name = "btnBulkActions"
        btnBulkActions.Size = New Size(110, 27)
        btnBulkActions.TabIndex = 4
        btnBulkActions.Text = "Bulk actions"
        btnBulkActions.UseVisualStyleBackColor = True
        '
        ' btnRefreshCompatibility
        '
        btnRefreshCompatibility.Anchor = AnchorStyles.None
        btnRefreshCompatibility.Location = New Point(392, 0)
        btnRefreshCompatibility.Margin = New Padding(6, 0, 0, 0)
        btnRefreshCompatibility.Name = "btnRefreshCompatibility"
        btnRefreshCompatibility.Size = New Size(130, 27)
        btnRefreshCompatibility.TabIndex = 5
        btnRefreshCompatibility.Text = "Refresh lists"
        btnRefreshCompatibility.UseVisualStyleBackColor = True
        ' 
        ' btnOpenWiki
        '
        btnOpenWiki.Anchor = AnchorStyles.None
        btnOpenWiki.Location = New Point(528, 0)
        btnOpenWiki.Margin = New Padding(6, 0, 0, 0)
        btnOpenWiki.Name = "btnOpenWiki"
        btnOpenWiki.Size = New Size(140, 27)
        btnOpenWiki.TabIndex = 6
        btnOpenWiki.Text = "Open wiki page"
        btnOpenWiki.UseVisualStyleBackColor = True
        ' 
        ' tabInstall
        ' 
        tabInstall.AutoScroll = True
        tabInstall.Controls.Add(installLayout)
        tabInstall.Location = New Point(4, 24)
        tabInstall.Name = "tabInstall"
        tabInstall.Padding = New Padding(3)
        tabInstall.Size = New Size(1246, 679)
        tabInstall.TabIndex = 1
        tabInstall.Text = "Install"
        tabInstall.UseVisualStyleBackColor = True
        ' 
        ' installLayout
        ' 
        installLayout.ColumnCount = 2
        installLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        installLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        installLayout.Controls.Add(grpGame, 0, 0)
        installLayout.Controls.Add(grpSource, 1, 0)
        installLayout.Controls.Add(grpHook, 0, 1)
        installLayout.Controls.Add(grpGpu, 1, 1)
        installLayout.Controls.Add(grpFg, 0, 2)
        installLayout.Controls.Add(grpBehavior, 1, 2)
        installLayout.Controls.Add(grpActions, 0, 3)
        installLayout.Dock = DockStyle.Fill
        installLayout.Location = New Point(3, 3)
        installLayout.Name = "installLayout"
        installLayout.RowCount = 4
        installLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        installLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        installLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        installLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        installLayout.Size = New Size(1240, 673)
        installLayout.TabIndex = 0
        ' 
        ' grpGame
        ' 
        grpGame.Controls.Add(lblGameExe)
        grpGame.Controls.Add(txtGameExe)
        grpGame.Controls.Add(btnBrowseGameExe)
        grpGame.Controls.Add(lblGameFolderLabel)
        grpGame.Controls.Add(txtGameFolder)
        grpGame.Controls.Add(lblEngineWarning)
        grpGame.Dock = DockStyle.Fill
        grpGame.Location = New Point(8, 8)
        grpGame.Margin = New Padding(8)
        grpGame.Name = "grpGame"
        grpGame.Size = New Size(604, 152)
        grpGame.TabIndex = 0
        grpGame.TabStop = False
        grpGame.Text = "Game"
        ' 
        ' lblGameExe
        ' 
        lblGameExe.AutoSize = True
        lblGameExe.Location = New Point(12, 25)
        lblGameExe.Name = "lblGameExe"
        lblGameExe.Size = New Size(60, 15)
        lblGameExe.TabIndex = 0
        lblGameExe.Text = "Game EXE"
        ' 
        ' txtGameExe
        ' 
        txtGameExe.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtGameExe.BackColor = SystemColors.Window
        txtGameExe.ForeColor = SystemColors.WindowText
        txtGameExe.Location = New Point(120, 22)
        txtGameExe.MinimumSize = New Size(0, 24)
        txtGameExe.Name = "txtGameExe"
        txtGameExe.Padding = New Padding(6, 3, 6, 3)
        txtGameExe.Size = New Size(389, 24)
        txtGameExe.TabIndex = 1
        ' 
        ' btnBrowseGameExe
        ' 
        btnBrowseGameExe.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseGameExe.Location = New Point(524, 22)
        btnBrowseGameExe.Name = "btnBrowseGameExe"
        btnBrowseGameExe.Size = New Size(60, 23)
        btnBrowseGameExe.TabIndex = 2
        btnBrowseGameExe.Text = "Browse"
        btnBrowseGameExe.UseVisualStyleBackColor = True
        ' 
        ' lblGameFolderLabel
        ' 
        lblGameFolderLabel.AutoSize = True
        lblGameFolderLabel.Location = New Point(12, 61)
        lblGameFolderLabel.Name = "lblGameFolderLabel"
        lblGameFolderLabel.Size = New Size(72, 15)
        lblGameFolderLabel.TabIndex = 3
        lblGameFolderLabel.Text = "Game folder"
        ' 
        ' txtGameFolder
        ' 
        txtGameFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtGameFolder.BackColor = SystemColors.Window
        txtGameFolder.ForeColor = SystemColors.WindowText
        txtGameFolder.Location = New Point(120, 56)
        txtGameFolder.MinimumSize = New Size(0, 24)
        txtGameFolder.Name = "txtGameFolder"
        txtGameFolder.Padding = New Padding(6, 3, 6, 3)
        txtGameFolder.Size = New Size(389, 24)
        txtGameFolder.TabIndex = 4
        ' 
        ' lblEngineWarning
        ' 
        lblEngineWarning.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblEngineWarning.AutoSize = True
        lblEngineWarning.ForeColor = Color.DarkRed
        lblEngineWarning.Location = New Point(12, 96)
        lblEngineWarning.Name = "lblEngineWarning"
        lblEngineWarning.Size = New Size(540, 15)
        lblEngineWarning.TabIndex = 5
        lblEngineWarning.Text = "Engine folder detected. For Unreal games use the Win64/WinGDK binaries folder next to the main exe."
        lblEngineWarning.Visible = False
        ' 
        ' grpSource
        ' 
        grpSource.Controls.Add(rbStable)
        grpSource.Controls.Add(lblStableInfo)
        grpSource.Controls.Add(rbNightly)
        grpSource.Controls.Add(lblNightlyInfo)
        grpSource.Controls.Add(rbLocal)
        grpSource.Controls.Add(txtLocalArchive)
        grpSource.Controls.Add(btnBrowseArchive)
        grpSource.Controls.Add(btnRefreshReleases)
        grpSource.Dock = DockStyle.Fill
        grpSource.Location = New Point(628, 8)
        grpSource.Margin = New Padding(8)
        grpSource.Name = "grpSource"
        grpSource.Size = New Size(604, 152)
        grpSource.TabIndex = 1
        grpSource.TabStop = False
        grpSource.Text = "OptiScaler Source"
        ' 
        ' rbStable
        ' 
        rbStable.AutoSize = True
        rbStable.Location = New Point(12, 26)
        rbStable.Name = "rbStable"
        rbStable.Size = New Size(57, 19)
        rbStable.TabIndex = 0
        rbStable.TabStop = True
        rbStable.Text = "Stable"
        rbStable.UseVisualStyleBackColor = True
        ' 
        ' lblStableInfo
        ' 
        lblStableInfo.AutoSize = True
        lblStableInfo.Location = New Point(100, 26)
        lblStableInfo.Name = "lblStableInfo"
        lblStableInfo.Size = New Size(102, 15)
        lblStableInfo.TabIndex = 3
        lblStableInfo.Text = "Stable: not loaded"
        ' 
        ' rbNightly
        ' 
        rbNightly.AutoSize = True
        rbNightly.Location = New Point(12, 50)
        rbNightly.Name = "rbNightly"
        rbNightly.Size = New Size(111, 19)
        rbNightly.TabIndex = 1
        rbNightly.TabStop = True
        rbNightly.Text = "Alternate source"
        rbNightly.UseVisualStyleBackColor = True
        ' 
        ' lblNightlyInfo
        ' 
        lblNightlyInfo.AutoSize = True
        lblNightlyInfo.Location = New Point(129, 52)
        lblNightlyInfo.Name = "lblNightlyInfo"
        lblNightlyInfo.Size = New Size(118, 15)
        lblNightlyInfo.TabIndex = 4
        lblNightlyInfo.Text = "Alternate: not loaded"
        ' 
        ' rbLocal
        ' 
        rbLocal.AutoSize = True
        rbLocal.Location = New Point(12, 74)
        rbLocal.Name = "rbLocal"
        rbLocal.Size = New Size(70, 19)
        rbLocal.TabIndex = 2
        rbLocal.TabStop = True
        rbLocal.Text = "Local .7z"
        rbLocal.UseVisualStyleBackColor = True
        ' 
        ' txtLocalArchive
        ' 
        txtLocalArchive.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtLocalArchive.BackColor = SystemColors.Window
        txtLocalArchive.ForeColor = SystemColors.WindowText
        txtLocalArchive.Location = New Point(100, 72)
        txtLocalArchive.MinimumSize = New Size(0, 24)
        txtLocalArchive.Name = "txtLocalArchive"
        txtLocalArchive.Padding = New Padding(6, 3, 6, 3)
        txtLocalArchive.Size = New Size(399, 24)
        txtLocalArchive.TabIndex = 5
        ' 
        ' btnBrowseArchive
        ' 
        btnBrowseArchive.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseArchive.Location = New Point(514, 72)
        btnBrowseArchive.Name = "btnBrowseArchive"
        btnBrowseArchive.Size = New Size(70, 23)
        btnBrowseArchive.TabIndex = 6
        btnBrowseArchive.Text = "Browse"
        btnBrowseArchive.UseVisualStyleBackColor = True
        ' 
        ' btnRefreshReleases
        ' 
        btnRefreshReleases.Location = New Point(12, 108)
        btnRefreshReleases.Name = "btnRefreshReleases"
        btnRefreshReleases.Size = New Size(120, 30)
        btnRefreshReleases.TabIndex = 7
        btnRefreshReleases.Text = "Refresh"
        btnRefreshReleases.UseVisualStyleBackColor = True
        ' 
        ' grpHook
        ' 
        grpHook.Controls.Add(lblHookName)
        grpHook.Controls.Add(cmbHookName)
        grpHook.Controls.Add(lblHookHint)
        grpHook.Dock = DockStyle.Fill
        grpHook.Location = New Point(8, 176)
        grpHook.Margin = New Padding(8)
        grpHook.Name = "grpHook"
        grpHook.Size = New Size(604, 152)
        grpHook.TabIndex = 2
        grpHook.TabStop = False
        grpHook.Text = "Hook Filename"
        ' 
        ' lblHookName
        ' 
        lblHookName.AutoSize = True
        lblHookName.Location = New Point(12, 31)
        lblHookName.Name = "lblHookName"
        lblHookName.Size = New Size(87, 15)
        lblHookName.TabIndex = 0
        lblHookName.Text = "Rename DLL to"
        ' 
        ' cmbHookName
        ' 
        cmbHookName.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cmbHookName.DropDownStyle = ComboBoxStyle.DropDownList
        cmbHookName.FormattingEnabled = True
        cmbHookName.Items.AddRange(New Object() {"dxgi.dll", "winmm.dll", "version.dll", "dbghelp.dll", "d3d12.dll", "wininet.dll", "winhttp.dll", "OptiScaler.asi"})
        cmbHookName.Location = New Point(160, 28)
        cmbHookName.Name = "cmbHookName"
        cmbHookName.Size = New Size(264, 23)
        cmbHookName.TabIndex = 1
        ' 
        ' lblHookHint
        ' 
        lblHookHint.AutoSize = True
        lblHookHint.Location = New Point(12, 64)
        lblHookHint.Name = "lblHookHint"
        lblHookHint.Size = New Size(550, 15)
        lblHookHint.TabIndex = 2
        lblHookHint.Text = "Supported: dxgi.dll, winmm.dll, version.dll, dbghelp.dll, d3d12.dll, wininet.dll, winhttp.dll, OptiScaler.asi"
        ' 
        ' grpGpu
        ' 
        grpGpu.Controls.Add(rbGpuNvidia)
        grpGpu.Controls.Add(rbGpuAmdIntel)
        grpGpu.Controls.Add(chkDlssInputs)
        grpGpu.Controls.Add(lblDlssHint)
        grpGpu.Dock = DockStyle.Fill
        grpGpu.Location = New Point(628, 176)
        grpGpu.Margin = New Padding(8)
        grpGpu.Name = "grpGpu"
        grpGpu.Size = New Size(604, 152)
        grpGpu.TabIndex = 3
        grpGpu.TabStop = False
        grpGpu.Text = "GPU Selection"
        ' 
        ' rbGpuNvidia
        ' 
        rbGpuNvidia.AutoSize = True
        rbGpuNvidia.Location = New Point(12, 24)
        rbGpuNvidia.Name = "rbGpuNvidia"
        rbGpuNvidia.Size = New Size(59, 19)
        rbGpuNvidia.TabIndex = 0
        rbGpuNvidia.TabStop = True
        rbGpuNvidia.Text = "Nvidia"
        rbGpuNvidia.UseVisualStyleBackColor = True
        ' 
        ' rbGpuAmdIntel
        ' 
        rbGpuAmdIntel.AutoSize = True
        rbGpuAmdIntel.Location = New Point(120, 24)
        rbGpuAmdIntel.Name = "rbGpuAmdIntel"
        rbGpuAmdIntel.Size = New Size(80, 19)
        rbGpuAmdIntel.TabIndex = 1
        rbGpuAmdIntel.TabStop = True
        rbGpuAmdIntel.Text = "AMD/Intel"
        rbGpuAmdIntel.UseVisualStyleBackColor = True
        ' 
        ' chkDlssInputs
        ' 
        chkDlssInputs.AutoSize = True
        chkDlssInputs.Location = New Point(12, 54)
        chkDlssInputs.Name = "chkDlssInputs"
        chkDlssInputs.Size = New Size(184, 19)
        chkDlssInputs.TabIndex = 2
        chkDlssInputs.Text = "Enable DLSS inputs (spoofing)"
        chkDlssInputs.UseVisualStyleBackColor = True
        ' 
        ' lblDlssHint
        ' 
        lblDlssHint.AutoSize = True
        lblDlssHint.Location = New Point(12, 78)
        lblDlssHint.Name = "lblDlssHint"
        lblDlssHint.Size = New Size(311, 15)
        lblDlssHint.TabIndex = 3
        lblDlssHint.Text = "Unchecked on AMD/Intel sets Dxgi=false in OptiScaler.ini."
        ' 
        ' grpFg
        ' 
        grpFg.Controls.Add(lblFgType)
        grpFg.Controls.Add(cmbFgType)
        grpFg.Controls.Add(lblFgHint)
        grpFg.Dock = DockStyle.Fill
        grpFg.Location = New Point(8, 344)
        grpFg.Margin = New Padding(8)
        grpFg.Name = "grpFg"
        grpFg.Size = New Size(604, 152)
        grpFg.TabIndex = 4
        grpFg.TabStop = False
        grpFg.Text = "Frame Generation"
        ' 
        ' lblFgType
        ' 
        lblFgType.AutoSize = True
        lblFgType.Location = New Point(12, 31)
        lblFgType.Name = "lblFgType"
        lblFgType.Size = New Size(100, 15)
        lblFgType.TabIndex = 0
        lblFgType.Text = "Frame generation"
        ' 
        ' cmbFgType
        ' 
        cmbFgType.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cmbFgType.DropDownStyle = ComboBoxStyle.DropDownList
        cmbFgType.FormattingEnabled = True
        cmbFgType.Items.AddRange(New Object() {"Auto (no change)", "None", "OptiFG (DX12 only)", "Nukem's dlssg-to-fsr3"})
        cmbFgType.Location = New Point(160, 28)
        cmbFgType.Name = "cmbFgType"
        cmbFgType.Size = New Size(404, 23)
        cmbFgType.TabIndex = 1
        ' 
        ' lblFgHint
        ' 
        lblFgHint.AutoSize = True
        lblFgHint.Location = New Point(12, 64)
        lblFgHint.Name = "lblFgHint"
        lblFgHint.Size = New Size(387, 15)
        lblFgHint.TabIndex = 2
        lblFgHint.Text = "OptiFG is DX12 only. Nukem requires native DLSS-FG games + mod DLL."
        ' 
        ' grpBehavior
        ' 
        grpBehavior.Controls.Add(lblConflictMode)
        grpBehavior.Controls.Add(cmbConflictMode)
        grpBehavior.Controls.Add(chkPreserveIni)
        grpBehavior.Controls.Add(lblBehaviorHint)
        grpBehavior.Dock = DockStyle.Fill
        grpBehavior.Location = New Point(628, 344)
        grpBehavior.Margin = New Padding(8)
        grpBehavior.Name = "grpBehavior"
        grpBehavior.Size = New Size(604, 152)
        grpBehavior.TabIndex = 5
        grpBehavior.TabStop = False
        grpBehavior.Text = "Install Behavior"
        ' 
        ' lblConflictMode
        ' 
        lblConflictMode.AutoSize = True
        lblConflictMode.Location = New Point(12, 31)
        lblConflictMode.Name = "lblConflictMode"
        lblConflictMode.Size = New Size(90, 15)
        lblConflictMode.TabIndex = 0
        lblConflictMode.Text = "On existing files"
        ' 
        ' cmbConflictMode
        ' 
        cmbConflictMode.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cmbConflictMode.DropDownStyle = ComboBoxStyle.DropDownList
        cmbConflictMode.FormattingEnabled = True
        cmbConflictMode.Items.AddRange(New Object() {"Backup and overwrite", "Overwrite", "Skip existing"})
        cmbConflictMode.Location = New Point(160, 28)
        cmbConflictMode.Name = "cmbConflictMode"
        cmbConflictMode.Size = New Size(264, 23)
        cmbConflictMode.TabIndex = 1
        '
        ' chkPreserveIni
        '
        chkPreserveIni.AutoSize = True
        chkPreserveIni.Checked = True
        chkPreserveIni.CheckState = CheckState.Checked
        chkPreserveIni.Location = New Point(160, 60)
        chkPreserveIni.Name = "chkPreserveIni"
        chkPreserveIni.Size = New Size(176, 19)
        chkPreserveIni.TabIndex = 2
        chkPreserveIni.Text = "Keep existing OptiScaler.ini"
        chkPreserveIni.UseVisualStyleBackColor = True
        '
        ' lblBehaviorHint
        '
        lblBehaviorHint.AutoSize = True
        lblBehaviorHint.Location = New Point(12, 92)
        lblBehaviorHint.Name = "lblBehaviorHint"
        lblBehaviorHint.Size = New Size(267, 15)
        lblBehaviorHint.TabIndex = 3
        lblBehaviorHint.Text = "Backup adds .bak_ timestamp before overwriting."
        ' 
        ' grpActions
        ' 
        installLayout.SetColumnSpan(grpActions, 2)
        grpActions.Controls.Add(btnInstall)
        grpActions.Controls.Add(btnUninstall)
        grpActions.Controls.Add(btnOpenGameFolder)
        grpActions.Controls.Add(btnEditIni)
        grpActions.Controls.Add(lblInstalledStatus)
        grpActions.Controls.Add(chkInstallOptiPatcher)
        grpActions.Controls.Add(lblInstallOptiPatcherStatus)
        grpActions.Controls.Add(lblOnlineWarning)
        grpActions.Controls.Add(lblActionNote)
        grpActions.Dock = DockStyle.Fill
        grpActions.Location = New Point(8, 512)
        grpActions.Margin = New Padding(8)
        grpActions.Name = "grpActions"
        grpActions.Size = New Size(1224, 153)
        grpActions.TabIndex = 6
        grpActions.TabStop = False
        grpActions.Text = "Actions"
        ' 
        ' btnInstall
        ' 
        btnInstall.Location = New Point(12, 30)
        btnInstall.Name = "btnInstall"
        btnInstall.Size = New Size(140, 40)
        btnInstall.TabIndex = 0
        btnInstall.Text = "Install"
        btnInstall.UseVisualStyleBackColor = True
        ' 
        ' btnUninstall
        ' 
        btnUninstall.Location = New Point(160, 30)
        btnUninstall.Name = "btnUninstall"
        btnUninstall.Size = New Size(140, 40)
        btnUninstall.TabIndex = 1
        btnUninstall.Text = "Uninstall"
        btnUninstall.UseVisualStyleBackColor = True
        ' 
        ' btnOpenGameFolder
        ' 
        btnOpenGameFolder.Location = New Point(308, 30)
        btnOpenGameFolder.Name = "btnOpenGameFolder"
        btnOpenGameFolder.Size = New Size(180, 40)
        btnOpenGameFolder.TabIndex = 2
        btnOpenGameFolder.Text = "Open game folder"
        btnOpenGameFolder.UseVisualStyleBackColor = True
        ' 
        ' btnEditIni
        ' 
        btnEditIni.Location = New Point(496, 30)
        btnEditIni.Name = "btnEditIni"
        btnEditIni.Size = New Size(140, 40)
        btnEditIni.TabIndex = 3
        btnEditIni.Text = "Edit OptiScaler.ini"
        btnEditIni.UseVisualStyleBackColor = True
        ' 
        ' lblInstalledStatus
        ' 
        lblInstalledStatus.AutoSize = True
        lblInstalledStatus.Location = New Point(12, 74)
        lblInstalledStatus.Name = "lblInstalledStatus"
        lblInstalledStatus.Size = New Size(84, 15)
        lblInstalledStatus.TabIndex = 3
        lblInstalledStatus.Text = "Installed: none"
        ' 
        ' chkInstallOptiPatcher
        ' 
        chkInstallOptiPatcher.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        chkInstallOptiPatcher.AutoSize = True
        chkInstallOptiPatcher.Location = New Point(756, 36)
        chkInstallOptiPatcher.Name = "chkInstallOptiPatcher"
        chkInstallOptiPatcher.Size = New Size(253, 19)
        chkInstallOptiPatcher.TabIndex = 4
        chkInstallOptiPatcher.Text = "Install OptiPatcher after OptiScaler install"
        chkInstallOptiPatcher.UseVisualStyleBackColor = True
        ' 
        ' lblInstallOptiPatcherStatus
        ' 
        lblInstallOptiPatcherStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblInstallOptiPatcherStatus.AutoEllipsis = True
        lblInstallOptiPatcherStatus.Location = New Point(756, 58)
        lblInstallOptiPatcherStatus.Name = "lblInstallOptiPatcherStatus"
        lblInstallOptiPatcherStatus.Size = New Size(456, 17)
        lblInstallOptiPatcherStatus.TabIndex = 5
        lblInstallOptiPatcherStatus.Text = "OptiPatcher: select a supported detected game to enable."
        ' 
        ' lblOnlineWarning
        ' 
        lblOnlineWarning.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblOnlineWarning.AutoSize = True
        lblOnlineWarning.ForeColor = Color.DarkRed
        lblOnlineWarning.Location = New Point(12, 94)
        lblOnlineWarning.Name = "lblOnlineWarning"
        lblOnlineWarning.Size = New Size(438, 15)
        lblOnlineWarning.TabIndex = 6
        lblOnlineWarning.Text = "Warning: Do not use OptiScaler with online games (anti-cheat risk, possible bans)."
        ' 
        ' lblActionNote
        ' 
        lblActionNote.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblActionNote.AutoSize = True
        lblActionNote.Location = New Point(12, 120)
        lblActionNote.Name = "lblActionNote"
        lblActionNote.Size = New Size(503, 15)
        lblActionNote.TabIndex = 7
        lblActionNote.Text = "Tip: Press Insert in-game to open the OptiScaler overlay. Try Alt+Insert if it closes immediately."
        ' 
        ' tabAddons
        ' 
        tabAddons.AutoScroll = True
        tabAddons.Controls.Add(addonsLayout)
        tabAddons.Location = New Point(4, 24)
        tabAddons.Name = "tabAddons"
        tabAddons.Padding = New Padding(3)
        tabAddons.Size = New Size(1246, 679)
        tabAddons.TabIndex = 2
        tabAddons.Text = "Add-ons"
        tabAddons.UseVisualStyleBackColor = True
        ' 
        ' addonsLayout
        ' 
        addonsLayout.ColumnCount = 2
        addonsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        addonsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        addonsLayout.Controls.Add(grpFakenvapi, 0, 0)
        addonsLayout.Controls.Add(grpNukem, 1, 0)
        addonsLayout.Controls.Add(grpNvngx, 0, 1)
        addonsLayout.Controls.Add(grpReshade, 1, 1)
        addonsLayout.Controls.Add(grpSpecialK, 0, 2)
        addonsLayout.Controls.Add(grpAsi, 1, 2)
        addonsLayout.Dock = DockStyle.Fill
        addonsLayout.Location = New Point(3, 3)
        addonsLayout.Name = "addonsLayout"
        addonsLayout.RowCount = 3
        addonsLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 33.33333F))
        addonsLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 33.33333F))
        addonsLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 33.33333F))
        addonsLayout.Size = New Size(1240, 673)
        addonsLayout.TabIndex = 0
        ' 
        ' grpFakenvapi
        ' 
        grpFakenvapi.Controls.Add(lblFakenvapiFolder)
        grpFakenvapi.Controls.Add(txtFakenvapiFolder)
        grpFakenvapi.Controls.Add(btnBrowseFakenvapiFolder)
        grpFakenvapi.Controls.Add(lblFakenvapiHint)
        grpFakenvapi.Dock = DockStyle.Fill
        grpFakenvapi.Location = New Point(8, 8)
        grpFakenvapi.Margin = New Padding(8)
        grpFakenvapi.Name = "grpFakenvapi"
        grpFakenvapi.Size = New Size(604, 208)
        grpFakenvapi.TabIndex = 0
        grpFakenvapi.TabStop = False
        grpFakenvapi.Text = "Fakenvapi (AMD/Intel)"
        ' 
        ' lblFakenvapiFolder
        ' 
        lblFakenvapiFolder.AutoSize = True
        lblFakenvapiFolder.Location = New Point(12, 57)
        lblFakenvapiFolder.Name = "lblFakenvapiFolder"
        lblFakenvapiFolder.Size = New Size(40, 15)
        lblFakenvapiFolder.TabIndex = 0
        lblFakenvapiFolder.Text = "Folder"
        ' 
        ' txtFakenvapiFolder
        ' 
        txtFakenvapiFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtFakenvapiFolder.BackColor = SystemColors.Window
        txtFakenvapiFolder.ForeColor = SystemColors.WindowText
        txtFakenvapiFolder.Location = New Point(120, 54)
        txtFakenvapiFolder.MinimumSize = New Size(0, 24)
        txtFakenvapiFolder.Name = "txtFakenvapiFolder"
        txtFakenvapiFolder.Padding = New Padding(6, 3, 6, 3)
        txtFakenvapiFolder.Size = New Size(389, 24)
        txtFakenvapiFolder.TabIndex = 1
        ' 
        ' btnBrowseFakenvapiFolder
        ' 
        btnBrowseFakenvapiFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseFakenvapiFolder.Location = New Point(514, 54)
        btnBrowseFakenvapiFolder.Name = "btnBrowseFakenvapiFolder"
        btnBrowseFakenvapiFolder.Size = New Size(70, 23)
        btnBrowseFakenvapiFolder.TabIndex = 2
        btnBrowseFakenvapiFolder.Text = "Browse"
        btnBrowseFakenvapiFolder.UseVisualStyleBackColor = True
        ' 
        ' lblFakenvapiHint
        ' 
        lblFakenvapiHint.AutoSize = True
        lblFakenvapiHint.Location = New Point(12, 96)
        lblFakenvapiHint.Name = "lblFakenvapiHint"
        lblFakenvapiHint.Size = New Size(302, 15)
        lblFakenvapiHint.TabIndex = 3
        lblFakenvapiHint.Text = "Copy nvapi64.dll and fakenvapi.ini into the game folder."
        ' 
        ' grpNukem
        ' 
        grpNukem.Controls.Add(lblNukemDll)
        grpNukem.Controls.Add(txtNukemDll)
        grpNukem.Controls.Add(btnBrowseNukemDll)
        grpNukem.Controls.Add(lblNukemHint)
        grpNukem.Dock = DockStyle.Fill
        grpNukem.Location = New Point(628, 8)
        grpNukem.Margin = New Padding(8)
        grpNukem.Name = "grpNukem"
        grpNukem.Size = New Size(604, 208)
        grpNukem.TabIndex = 1
        grpNukem.TabStop = False
        grpNukem.Text = "Nukem Frame Generation"
        ' 
        ' lblNukemDll
        ' 
        lblNukemDll.AutoSize = True
        lblNukemDll.Location = New Point(12, 57)
        lblNukemDll.Name = "lblNukemDll"
        lblNukemDll.Size = New Size(69, 15)
        lblNukemDll.TabIndex = 0
        lblNukemDll.Text = "Nukem DLL"
        ' 
        ' txtNukemDll
        ' 
        txtNukemDll.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtNukemDll.BackColor = SystemColors.Window
        txtNukemDll.ForeColor = SystemColors.WindowText
        txtNukemDll.Location = New Point(120, 54)
        txtNukemDll.MinimumSize = New Size(0, 24)
        txtNukemDll.Name = "txtNukemDll"
        txtNukemDll.Padding = New Padding(6, 3, 6, 3)
        txtNukemDll.Size = New Size(389, 24)
        txtNukemDll.TabIndex = 1
        ' 
        ' btnBrowseNukemDll
        ' 
        btnBrowseNukemDll.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseNukemDll.Location = New Point(514, 54)
        btnBrowseNukemDll.Name = "btnBrowseNukemDll"
        btnBrowseNukemDll.Size = New Size(70, 23)
        btnBrowseNukemDll.TabIndex = 2
        btnBrowseNukemDll.Text = "Browse"
        btnBrowseNukemDll.UseVisualStyleBackColor = True
        ' 
        ' lblNukemHint
        ' 
        lblNukemHint.AutoSize = True
        lblNukemHint.Location = New Point(12, 96)
        lblNukemHint.Name = "lblNukemHint"
        lblNukemHint.Size = New Size(400, 15)
        lblNukemHint.TabIndex = 3
        lblNukemHint.Text = "Only copy dlssg_to_fsr3_amd_is_better.dll. Requires native DLSS-FG games."
        ' 
        ' grpNvngx
        ' 
        grpNvngx.Controls.Add(lblNvngxDll)
        grpNvngx.Controls.Add(txtNvngxDll)
        grpNvngx.Controls.Add(btnBrowseNvngx)
        grpNvngx.Controls.Add(lblNvngxHint)
        grpNvngx.Dock = DockStyle.Fill
        grpNvngx.Location = New Point(8, 232)
        grpNvngx.Margin = New Padding(8)
        grpNvngx.Name = "grpNvngx"
        grpNvngx.Size = New Size(604, 208)
        grpNvngx.TabIndex = 2
        grpNvngx.TabStop = False
        grpNvngx.Text = "nvngx_dlss.dll (Optional)"
        ' 
        ' lblNvngxDll
        ' 
        lblNvngxDll.AutoSize = True
        lblNvngxDll.Location = New Point(12, 57)
        lblNvngxDll.Name = "lblNvngxDll"
        lblNvngxDll.Size = New Size(80, 15)
        lblNvngxDll.TabIndex = 0
        lblNvngxDll.Text = "nvngx_dlss.dll"
        ' 
        ' txtNvngxDll
        ' 
        txtNvngxDll.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtNvngxDll.BackColor = SystemColors.Window
        txtNvngxDll.ForeColor = SystemColors.WindowText
        txtNvngxDll.Location = New Point(120, 54)
        txtNvngxDll.MinimumSize = New Size(0, 24)
        txtNvngxDll.Name = "txtNvngxDll"
        txtNvngxDll.Padding = New Padding(6, 3, 6, 3)
        txtNvngxDll.Size = New Size(389, 24)
        txtNvngxDll.TabIndex = 1
        ' 
        ' btnBrowseNvngx
        ' 
        btnBrowseNvngx.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseNvngx.Location = New Point(514, 54)
        btnBrowseNvngx.Name = "btnBrowseNvngx"
        btnBrowseNvngx.Size = New Size(70, 23)
        btnBrowseNvngx.TabIndex = 2
        btnBrowseNvngx.Text = "Browse"
        btnBrowseNvngx.UseVisualStyleBackColor = True
        ' 
        ' lblNvngxHint
        ' 
        lblNvngxHint.AutoSize = True
        lblNvngxHint.Location = New Point(12, 96)
        lblNvngxHint.Name = "lblNvngxHint"
        lblNvngxHint.Size = New Size(367, 15)
        lblNvngxHint.TabIndex = 3
        lblNvngxHint.Text = "Provide nvngx_dlss.dll for FSR-only games if you want DLSS outputs."
        ' 
        ' grpReshade
        ' 
        grpReshade.Controls.Add(chkEnableReshade)
        grpReshade.Controls.Add(lblReshadeDll)
        grpReshade.Controls.Add(txtReshadeDll)
        grpReshade.Controls.Add(btnBrowseReshade)
        grpReshade.Controls.Add(lblReshadeHint)
        grpReshade.Dock = DockStyle.Fill
        grpReshade.Location = New Point(628, 232)
        grpReshade.Margin = New Padding(8)
        grpReshade.Name = "grpReshade"
        grpReshade.Size = New Size(604, 208)
        grpReshade.TabIndex = 3
        grpReshade.TabStop = False
        grpReshade.Text = "ReShade"
        ' 
        ' chkEnableReshade
        ' 
        chkEnableReshade.AutoSize = True
        chkEnableReshade.Location = New Point(12, 30)
        chkEnableReshade.Name = "chkEnableReshade"
        chkEnableReshade.Size = New Size(134, 19)
        chkEnableReshade.TabIndex = 0
        chkEnableReshade.Text = "Enable Reshade load"
        chkEnableReshade.UseVisualStyleBackColor = True
        ' 
        ' lblReshadeDll
        ' 
        lblReshadeDll.AutoSize = True
        lblReshadeDll.Location = New Point(12, 69)
        lblReshadeDll.Name = "lblReshadeDll"
        lblReshadeDll.Size = New Size(74, 15)
        lblReshadeDll.TabIndex = 1
        lblReshadeDll.Text = "Reshade DLL"
        ' 
        ' txtReshadeDll
        ' 
        txtReshadeDll.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtReshadeDll.BackColor = SystemColors.Window
        txtReshadeDll.ForeColor = SystemColors.WindowText
        txtReshadeDll.Location = New Point(120, 66)
        txtReshadeDll.MinimumSize = New Size(0, 24)
        txtReshadeDll.Name = "txtReshadeDll"
        txtReshadeDll.Padding = New Padding(6, 3, 6, 3)
        txtReshadeDll.Size = New Size(389, 24)
        txtReshadeDll.TabIndex = 2
        ' 
        ' btnBrowseReshade
        ' 
        btnBrowseReshade.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseReshade.Location = New Point(514, 66)
        btnBrowseReshade.Name = "btnBrowseReshade"
        btnBrowseReshade.Size = New Size(70, 23)
        btnBrowseReshade.TabIndex = 3
        btnBrowseReshade.Text = "Browse"
        btnBrowseReshade.UseVisualStyleBackColor = True
        ' 
        ' lblReshadeHint
        ' 
        lblReshadeHint.AutoSize = True
        lblReshadeHint.Location = New Point(12, 106)
        lblReshadeHint.Name = "lblReshadeHint"
        lblReshadeHint.Size = New Size(263, 15)
        lblReshadeHint.TabIndex = 4
        lblReshadeHint.Text = "Copied as ReShade64.dll and LoadReshade=true."
        ' 
        ' grpSpecialK
        ' 
        grpSpecialK.Controls.Add(chkEnableSpecialK)
        grpSpecialK.Controls.Add(lblSpecialKDll)
        grpSpecialK.Controls.Add(txtSpecialKDll)
        grpSpecialK.Controls.Add(btnBrowseSpecialK)
        grpSpecialK.Controls.Add(chkCreateSpecialKMarker)
        grpSpecialK.Controls.Add(lblSpecialKHint)
        grpSpecialK.Dock = DockStyle.Fill
        grpSpecialK.Location = New Point(8, 456)
        grpSpecialK.Margin = New Padding(8)
        grpSpecialK.Name = "grpSpecialK"
        grpSpecialK.Size = New Size(604, 209)
        grpSpecialK.TabIndex = 4
        grpSpecialK.TabStop = False
        grpSpecialK.Text = "Special K"
        ' 
        ' chkEnableSpecialK
        ' 
        chkEnableSpecialK.AutoSize = True
        chkEnableSpecialK.Location = New Point(12, 24)
        chkEnableSpecialK.Name = "chkEnableSpecialK"
        chkEnableSpecialK.Size = New Size(108, 19)
        chkEnableSpecialK.TabIndex = 0
        chkEnableSpecialK.Text = "Enable SpecialK"
        chkEnableSpecialK.UseVisualStyleBackColor = True
        ' 
        ' lblSpecialKDll
        ' 
        lblSpecialKDll.AutoSize = True
        lblSpecialKDll.Location = New Point(12, 59)
        lblSpecialKDll.Name = "lblSpecialKDll"
        lblSpecialKDll.Size = New Size(74, 15)
        lblSpecialKDll.TabIndex = 1
        lblSpecialKDll.Text = "SpecialK DLL"
        ' 
        ' txtSpecialKDll
        ' 
        txtSpecialKDll.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtSpecialKDll.BackColor = SystemColors.Window
        txtSpecialKDll.ForeColor = SystemColors.WindowText
        txtSpecialKDll.Location = New Point(120, 56)
        txtSpecialKDll.MinimumSize = New Size(0, 24)
        txtSpecialKDll.Name = "txtSpecialKDll"
        txtSpecialKDll.Padding = New Padding(6, 3, 6, 3)
        txtSpecialKDll.Size = New Size(389, 24)
        txtSpecialKDll.TabIndex = 2
        ' 
        ' btnBrowseSpecialK
        ' 
        btnBrowseSpecialK.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseSpecialK.Location = New Point(514, 56)
        btnBrowseSpecialK.Name = "btnBrowseSpecialK"
        btnBrowseSpecialK.Size = New Size(70, 23)
        btnBrowseSpecialK.TabIndex = 3
        btnBrowseSpecialK.Text = "Browse"
        btnBrowseSpecialK.UseVisualStyleBackColor = True
        ' 
        ' chkCreateSpecialKMarker
        ' 
        chkCreateSpecialKMarker.AutoSize = True
        chkCreateSpecialKMarker.Location = New Point(12, 92)
        chkCreateSpecialKMarker.Name = "chkCreateSpecialKMarker"
        chkCreateSpecialKMarker.Size = New Size(172, 19)
        chkCreateSpecialKMarker.TabIndex = 4
        chkCreateSpecialKMarker.Text = "Create SpecialK.dxgi marker"
        chkCreateSpecialKMarker.UseVisualStyleBackColor = True
        ' 
        ' lblSpecialKHint
        ' 
        lblSpecialKHint.AutoSize = True
        lblSpecialKHint.Location = New Point(12, 122)
        lblSpecialKHint.Name = "lblSpecialKHint"
        lblSpecialKHint.Size = New Size(300, 15)
        lblSpecialKHint.TabIndex = 5
        lblSpecialKHint.Text = "Creates SpecialK.dxgi marker and enables LoadSpecialK."
        ' 
        ' grpAsi
        ' 
        grpAsi.Controls.Add(chkLoadAsiPlugins)
        grpAsi.Controls.Add(lblPluginsPath)
        grpAsi.Controls.Add(txtPluginsPath)
        grpAsi.Controls.Add(btnBrowsePluginsPath)
        grpAsi.Controls.Add(lblAsiHint)
        grpAsi.Controls.Add(lblOptiPatcherSource)
        grpAsi.Controls.Add(cmbOptiPatcherSource)
        grpAsi.Controls.Add(btnOptiPatcherRefresh)
        grpAsi.Controls.Add(lblOptiPatcherRelease)
        grpAsi.Controls.Add(txtOptiPatcherLocalFile)
        grpAsi.Controls.Add(btnBrowseOptiPatcherLocal)
        grpAsi.Controls.Add(btnInstallOptiPatcher)
        grpAsi.Controls.Add(btnRemoveOptiPatcher)
        grpAsi.Controls.Add(lblOptiPatcherStatus)
        grpAsi.Dock = DockStyle.Fill
        grpAsi.Location = New Point(628, 456)
        grpAsi.Margin = New Padding(8)
        grpAsi.Name = "grpAsi"
        grpAsi.Size = New Size(604, 209)
        grpAsi.TabIndex = 5
        grpAsi.TabStop = False
        grpAsi.Text = "OptiPatcher"
        ' 
        ' chkLoadAsiPlugins
        ' 
        chkLoadAsiPlugins.AutoSize = True
        chkLoadAsiPlugins.Location = New Point(12, 24)
        chkLoadAsiPlugins.Name = "chkLoadAsiPlugins"
        chkLoadAsiPlugins.Size = New Size(300, 19)
        chkLoadAsiPlugins.TabIndex = 0
        chkLoadAsiPlugins.Text = "Enable ASI plugin loading (auto-create plugins folder)"
        chkLoadAsiPlugins.UseVisualStyleBackColor = True
        ' 
        ' lblPluginsPath
        ' 
        lblPluginsPath.AutoSize = True
        lblPluginsPath.Location = New Point(12, 52)
        lblPluginsPath.Name = "lblPluginsPath"
        lblPluginsPath.Size = New Size(73, 15)
        lblPluginsPath.TabIndex = 1
        lblPluginsPath.Text = "Plugins path"
        ' 
        ' txtPluginsPath
        ' 
        txtPluginsPath.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtPluginsPath.BackColor = SystemColors.Window
        txtPluginsPath.ForeColor = SystemColors.WindowText
        txtPluginsPath.Location = New Point(120, 48)
        txtPluginsPath.MinimumSize = New Size(0, 24)
        txtPluginsPath.Name = "txtPluginsPath"
        txtPluginsPath.Padding = New Padding(6, 3, 6, 3)
        txtPluginsPath.Size = New Size(389, 24)
        txtPluginsPath.TabIndex = 2
        ' 
        ' btnBrowsePluginsPath
        ' 
        btnBrowsePluginsPath.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowsePluginsPath.Location = New Point(514, 48)
        btnBrowsePluginsPath.Name = "btnBrowsePluginsPath"
        btnBrowsePluginsPath.Size = New Size(70, 23)
        btnBrowsePluginsPath.TabIndex = 3
        btnBrowsePluginsPath.Text = "Browse"
        btnBrowsePluginsPath.UseVisualStyleBackColor = True
        ' 
        ' lblAsiHint
        ' 
        lblAsiHint.AutoSize = True
        lblAsiHint.Location = New Point(12, 80)
        lblAsiHint.Name = "lblAsiHint"
        lblAsiHint.Size = New Size(350, 15)
        lblAsiHint.TabIndex = 4
        lblAsiHint.Text = "OptiPatcher needs ASI loading. Folder is created automatically."
        ' 
        ' lblOptiPatcherSource
        ' 
        lblOptiPatcherSource.AutoSize = True
        lblOptiPatcherSource.Location = New Point(12, 108)
        lblOptiPatcherSource.Name = "lblOptiPatcherSource"
        lblOptiPatcherSource.Size = New Size(80, 15)
        lblOptiPatcherSource.TabIndex = 5
        lblOptiPatcherSource.Text = "Patcher source"
        ' 
        ' cmbOptiPatcherSource
        ' 
        cmbOptiPatcherSource.DropDownStyle = ComboBoxStyle.DropDownList
        cmbOptiPatcherSource.FormattingEnabled = True
        cmbOptiPatcherSource.Items.AddRange(New Object() {"Rolling", "Stable", "Alternate", "Local .asi"})
        cmbOptiPatcherSource.Location = New Point(120, 104)
        cmbOptiPatcherSource.Name = "cmbOptiPatcherSource"
        cmbOptiPatcherSource.Size = New Size(130, 23)
        cmbOptiPatcherSource.TabIndex = 6
        ' 
        ' btnOptiPatcherRefresh
        ' 
        btnOptiPatcherRefresh.Location = New Point(258, 104)
        btnOptiPatcherRefresh.Name = "btnOptiPatcherRefresh"
        btnOptiPatcherRefresh.Size = New Size(70, 23)
        btnOptiPatcherRefresh.TabIndex = 7
        btnOptiPatcherRefresh.Text = "Refresh"
        btnOptiPatcherRefresh.UseVisualStyleBackColor = True
        ' 
        ' lblOptiPatcherRelease
        ' 
        lblOptiPatcherRelease.AutoEllipsis = True
        lblOptiPatcherRelease.Location = New Point(336, 108)
        lblOptiPatcherRelease.Name = "lblOptiPatcherRelease"
        lblOptiPatcherRelease.Size = New Size(248, 17)
        lblOptiPatcherRelease.TabIndex = 8
        lblOptiPatcherRelease.Text = "OptiPatcher: not loaded"
        ' 
        ' txtOptiPatcherLocalFile
        ' 
        txtOptiPatcherLocalFile.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtOptiPatcherLocalFile.BackColor = SystemColors.Window
        txtOptiPatcherLocalFile.ForeColor = SystemColors.WindowText
        txtOptiPatcherLocalFile.Location = New Point(120, 132)
        txtOptiPatcherLocalFile.MinimumSize = New Size(0, 24)
        txtOptiPatcherLocalFile.Name = "txtOptiPatcherLocalFile"
        txtOptiPatcherLocalFile.Padding = New Padding(6, 3, 6, 3)
        txtOptiPatcherLocalFile.Size = New Size(389, 24)
        txtOptiPatcherLocalFile.TabIndex = 9
        ' 
        ' btnBrowseOptiPatcherLocal
        ' 
        btnBrowseOptiPatcherLocal.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseOptiPatcherLocal.Location = New Point(514, 132)
        btnBrowseOptiPatcherLocal.Name = "btnBrowseOptiPatcherLocal"
        btnBrowseOptiPatcherLocal.Size = New Size(70, 23)
        btnBrowseOptiPatcherLocal.TabIndex = 10
        btnBrowseOptiPatcherLocal.Text = "Browse"
        btnBrowseOptiPatcherLocal.UseVisualStyleBackColor = True
        ' 
        ' btnInstallOptiPatcher
        ' 
        btnInstallOptiPatcher.Location = New Point(120, 164)
        btnInstallOptiPatcher.Name = "btnInstallOptiPatcher"
        btnInstallOptiPatcher.Size = New Size(102, 24)
        btnInstallOptiPatcher.TabIndex = 11
        btnInstallOptiPatcher.Text = "Install/Update"
        btnInstallOptiPatcher.UseVisualStyleBackColor = True
        ' 
        ' btnRemoveOptiPatcher
        ' 
        btnRemoveOptiPatcher.Location = New Point(228, 164)
        btnRemoveOptiPatcher.Name = "btnRemoveOptiPatcher"
        btnRemoveOptiPatcher.Size = New Size(82, 24)
        btnRemoveOptiPatcher.TabIndex = 12
        btnRemoveOptiPatcher.Text = "Remove"
        btnRemoveOptiPatcher.UseVisualStyleBackColor = True
        ' 
        ' lblOptiPatcherStatus
        ' 
        lblOptiPatcherStatus.AutoEllipsis = True
        lblOptiPatcherStatus.Location = New Point(318, 168)
        lblOptiPatcherStatus.Name = "lblOptiPatcherStatus"
        lblOptiPatcherStatus.Size = New Size(272, 15)
        lblOptiPatcherStatus.TabIndex = 13
        lblOptiPatcherStatus.Text = "Status: not installed"
        ' 
        ' tabExperimental
        ' 
        tabExperimental.Controls.Add(experimentalLayout)
        tabExperimental.Location = New Point(4, 24)
        tabExperimental.Name = "tabExperimental"
        tabExperimental.Padding = New Padding(8)
        tabExperimental.Size = New Size(1246, 679)
        tabExperimental.TabIndex = 3
        tabExperimental.Text = "FSR4 INT8 (Manual)"
        tabExperimental.UseVisualStyleBackColor = True
        ' 
        ' experimentalLayout
        ' 
        experimentalLayout.ColumnCount = 1
        experimentalLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        experimentalLayout.Controls.Add(grpFsr4Package, 0, 0)
        experimentalLayout.Controls.Add(grpFsr4Options, 0, 1)
        experimentalLayout.Controls.Add(grpFsr4Actions, 0, 2)
        experimentalLayout.Dock = DockStyle.Fill
        experimentalLayout.Location = New Point(8, 8)
        experimentalLayout.Name = "experimentalLayout"
        experimentalLayout.RowCount = 3
        experimentalLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 124F))
        experimentalLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 132F))
        experimentalLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        experimentalLayout.Size = New Size(1230, 663)
        experimentalLayout.TabIndex = 0
        ' 
        ' grpFsr4Package
        ' 
        grpFsr4Package.Controls.Add(lblFsr4PackageFolder)
        grpFsr4Package.Controls.Add(txtFsr4PackageFolder)
        grpFsr4Package.Controls.Add(btnBrowseFsr4PackageFolder)
        grpFsr4Package.Controls.Add(lblFsr4PackageHint)
        grpFsr4Package.Dock = DockStyle.Fill
        grpFsr4Package.Location = New Point(0, 0)
        grpFsr4Package.Margin = New Padding(0, 0, 0, 8)
        grpFsr4Package.Name = "grpFsr4Package"
        grpFsr4Package.Size = New Size(1230, 116)
        grpFsr4Package.TabIndex = 0
        grpFsr4Package.TabStop = False
        grpFsr4Package.Text = "Manual FSR4 INT8 Package (RDNA2 / APUs)"
        ' 
        ' lblFsr4PackageFolder
        ' 
        lblFsr4PackageFolder.AutoSize = True
        lblFsr4PackageFolder.Location = New Point(12, 32)
        lblFsr4PackageFolder.Name = "lblFsr4PackageFolder"
        lblFsr4PackageFolder.Size = New Size(87, 15)
        lblFsr4PackageFolder.TabIndex = 0
        lblFsr4PackageFolder.Text = "Package folder"
        ' 
        ' txtFsr4PackageFolder
        ' 
        txtFsr4PackageFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtFsr4PackageFolder.BackColor = SystemColors.Window
        txtFsr4PackageFolder.ForeColor = SystemColors.WindowText
        txtFsr4PackageFolder.Location = New Point(120, 28)
        txtFsr4PackageFolder.MinimumSize = New Size(0, 24)
        txtFsr4PackageFolder.Name = "txtFsr4PackageFolder"
        txtFsr4PackageFolder.Padding = New Padding(6, 3, 6, 3)
        txtFsr4PackageFolder.Size = New Size(1002, 24)
        txtFsr4PackageFolder.TabIndex = 1
        ' 
        ' btnBrowseFsr4PackageFolder
        ' 
        btnBrowseFsr4PackageFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseFsr4PackageFolder.Location = New Point(1128, 28)
        btnBrowseFsr4PackageFolder.Name = "btnBrowseFsr4PackageFolder"
        btnBrowseFsr4PackageFolder.Size = New Size(90, 24)
        btnBrowseFsr4PackageFolder.TabIndex = 2
        btnBrowseFsr4PackageFolder.Text = "Browse"
        btnBrowseFsr4PackageFolder.UseVisualStyleBackColor = True
        ' 
        ' lblFsr4PackageHint
        ' 
        lblFsr4PackageHint.AutoSize = True
        lblFsr4PackageHint.Location = New Point(12, 68)
        lblFsr4PackageHint.Name = "lblFsr4PackageHint"
        lblFsr4PackageHint.Size = New Size(622, 15)
        lblFsr4PackageHint.TabIndex = 3
        lblFsr4PackageHint.Text = "Select a local folder that contains FSR4 INT8 files (for example amdxcffx64.dll). Files are copied with the selected install conflict behavior."
        ' 
        ' grpFsr4Options
        ' 
        grpFsr4Options.Controls.Add(chkFsr4EnableUpdate)
        grpFsr4Options.Controls.Add(chkFsr4EnableAgility)
        grpFsr4Options.Controls.Add(chkFsr4ForceInt8)
        grpFsr4Options.Controls.Add(lblFsr4OptionsHint)
        grpFsr4Options.Dock = DockStyle.Fill
        grpFsr4Options.Location = New Point(0, 124)
        grpFsr4Options.Margin = New Padding(0, 0, 0, 8)
        grpFsr4Options.Name = "grpFsr4Options"
        grpFsr4Options.Size = New Size(1230, 124)
        grpFsr4Options.TabIndex = 1
        grpFsr4Options.TabStop = False
        grpFsr4Options.Text = "INI Options"
        ' 
        ' chkFsr4EnableUpdate
        ' 
        chkFsr4EnableUpdate.AutoSize = True
        chkFsr4EnableUpdate.Location = New Point(12, 24)
        chkFsr4EnableUpdate.Name = "chkFsr4EnableUpdate"
        chkFsr4EnableUpdate.Size = New Size(219, 19)
        chkFsr4EnableUpdate.TabIndex = 0
        chkFsr4EnableUpdate.Text = "Set Fsr4Update=true in OptiScaler.ini"
        chkFsr4EnableUpdate.UseVisualStyleBackColor = True
        ' 
        ' chkFsr4EnableAgility
        ' 
        chkFsr4EnableAgility.AutoSize = True
        chkFsr4EnableAgility.Location = New Point(12, 49)
        chkFsr4EnableAgility.Name = "chkFsr4EnableAgility"
        chkFsr4EnableAgility.Size = New Size(324, 19)
        chkFsr4EnableAgility.TabIndex = 1
        chkFsr4EnableAgility.Text = "Set FsrAgilitySDKUpgrade=true (helps some Win10 titles)"
        chkFsr4EnableAgility.UseVisualStyleBackColor = True
        '
        ' chkFsr4ForceInt8
        '
        chkFsr4ForceInt8.AutoSize = True
        chkFsr4ForceInt8.Location = New Point(12, 74)
        chkFsr4ForceInt8.Name = "chkFsr4ForceInt8"
        chkFsr4ForceInt8.Size = New Size(336, 19)
        chkFsr4ForceInt8.TabIndex = 2
        chkFsr4ForceInt8.Text = "Set Fsr4ForceEnableInt8=true (force INT8 on RDNA2/APUs)"
        chkFsr4ForceInt8.UseVisualStyleBackColor = True
        '
        ' lblFsr4OptionsHint
        ' 
        ' Own line below the checkboxes: the option captions are wide enough to reach
        ' well past the middle of the group, so a side-by-side hint gets clipped.
        lblFsr4OptionsHint.AutoSize = True
        lblFsr4OptionsHint.Location = New Point(12, 99)
        lblFsr4OptionsHint.Name = "lblFsr4OptionsHint"
        lblFsr4OptionsHint.Size = New Size(398, 15)
        lblFsr4OptionsHint.TabIndex = 3
        lblFsr4OptionsHint.Text = "Keys are restored on remove when this feature was applied by the installer."
        ' 
        ' grpFsr4Actions
        ' 
        grpFsr4Actions.Controls.Add(lblFsr4TargetGame)
        grpFsr4Actions.Controls.Add(txtFsr4TargetGameFolder)
        grpFsr4Actions.Controls.Add(btnFsr4PickGame)
        grpFsr4Actions.Controls.Add(btnFsr4Apply)
        grpFsr4Actions.Controls.Add(btnFsr4Remove)
        grpFsr4Actions.Controls.Add(btnFsr4RefreshStatus)
        grpFsr4Actions.Controls.Add(lblFsr4Status)
        grpFsr4Actions.Controls.Add(lblFsr4ActionHint)
        grpFsr4Actions.Controls.Add(lblFsr4DetectedGames)
        grpFsr4Actions.Controls.Add(btnFsr4ScanDetectedGames)
        grpFsr4Actions.Controls.Add(btnFsr4UseSelectedGame)
        grpFsr4Actions.Controls.Add(btnFsr4BrowseGameExe)
        grpFsr4Actions.Controls.Add(lvFsr4DetectedGames)
        grpFsr4Actions.Dock = DockStyle.Fill
        grpFsr4Actions.Location = New Point(0, 256)
        grpFsr4Actions.Margin = New Padding(0)
        grpFsr4Actions.Name = "grpFsr4Actions"
        grpFsr4Actions.Size = New Size(1230, 407)
        grpFsr4Actions.TabIndex = 2
        grpFsr4Actions.TabStop = False
        grpFsr4Actions.Text = "Apply / Remove"
        ' 
        ' lblFsr4TargetGame
        ' 
        lblFsr4TargetGame.AutoSize = True
        lblFsr4TargetGame.Location = New Point(12, 30)
        lblFsr4TargetGame.Name = "lblFsr4TargetGame"
        lblFsr4TargetGame.Size = New Size(100, 15)
        lblFsr4TargetGame.TabIndex = 0
        lblFsr4TargetGame.Text = "Target game folder"
        ' 
        ' txtFsr4TargetGameFolder
        ' 
        txtFsr4TargetGameFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtFsr4TargetGameFolder.BackColor = SystemColors.Window
        txtFsr4TargetGameFolder.ForeColor = SystemColors.WindowText
        txtFsr4TargetGameFolder.Location = New Point(120, 26)
        txtFsr4TargetGameFolder.MinimumSize = New Size(0, 24)
        txtFsr4TargetGameFolder.Name = "txtFsr4TargetGameFolder"
        txtFsr4TargetGameFolder.Padding = New Padding(6, 3, 6, 3)
        txtFsr4TargetGameFolder.ReadOnly = True
        txtFsr4TargetGameFolder.Size = New Size(1002, 24)
        txtFsr4TargetGameFolder.TabIndex = 1
        txtFsr4TargetGameFolder.TabStop = False
        ' 
        ' btnFsr4PickGame
        ' 
        btnFsr4PickGame.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnFsr4PickGame.Location = New Point(1128, 26)
        btnFsr4PickGame.Name = "btnFsr4PickGame"
        btnFsr4PickGame.Size = New Size(90, 24)
        btnFsr4PickGame.TabIndex = 2
        btnFsr4PickGame.Text = "Pick game"
        btnFsr4PickGame.UseVisualStyleBackColor = True
        ' 
        ' btnFsr4Apply
        ' 
        btnFsr4Apply.Location = New Point(12, 64)
        btnFsr4Apply.Name = "btnFsr4Apply"
        btnFsr4Apply.Size = New Size(140, 36)
        btnFsr4Apply.TabIndex = 3
        btnFsr4Apply.Text = "Apply package"
        btnFsr4Apply.UseVisualStyleBackColor = True
        ' 
        ' btnFsr4Remove
        ' 
        btnFsr4Remove.Location = New Point(160, 64)
        btnFsr4Remove.Name = "btnFsr4Remove"
        btnFsr4Remove.Size = New Size(140, 36)
        btnFsr4Remove.TabIndex = 4
        btnFsr4Remove.Text = "Remove package"
        btnFsr4Remove.UseVisualStyleBackColor = True
        ' 
        ' btnFsr4RefreshStatus
        ' 
        btnFsr4RefreshStatus.Location = New Point(308, 64)
        btnFsr4RefreshStatus.Name = "btnFsr4RefreshStatus"
        btnFsr4RefreshStatus.Size = New Size(140, 36)
        btnFsr4RefreshStatus.TabIndex = 5
        btnFsr4RefreshStatus.Text = "Refresh status"
        btnFsr4RefreshStatus.UseVisualStyleBackColor = True
        ' 
        ' lblFsr4Status
        ' 
        lblFsr4Status.AutoSize = True
        lblFsr4Status.Location = New Point(12, 112)
        lblFsr4Status.Name = "lblFsr4Status"
        lblFsr4Status.Size = New Size(180, 15)
        lblFsr4Status.TabIndex = 6
        lblFsr4Status.Text = "Experimental package: not installed"
        ' 
        ' lblFsr4ActionHint
        ' 
        lblFsr4ActionHint.AutoSize = True
        lblFsr4ActionHint.Location = New Point(12, 136)
        lblFsr4ActionHint.Name = "lblFsr4ActionHint"
        lblFsr4ActionHint.Size = New Size(843, 15)
        lblFsr4ActionHint.TabIndex = 7
        lblFsr4ActionHint.Text = "Workflow: pick game on Install tab (or from Game Detection -> Use selected), return here, choose package folder, then Apply package."
        ' 
        ' lblFsr4DetectedGames
        ' 
        lblFsr4DetectedGames.AutoSize = True
        lblFsr4DetectedGames.Location = New Point(12, 171)
        lblFsr4DetectedGames.Name = "lblFsr4DetectedGames"
        lblFsr4DetectedGames.Size = New Size(145, 15)
        lblFsr4DetectedGames.TabIndex = 8
        lblFsr4DetectedGames.Text = "Detected supported games"
        ' 
        ' btnFsr4ScanDetectedGames
        ' 
        btnFsr4ScanDetectedGames.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnFsr4ScanDetectedGames.Location = New Point(827, 166)
        btnFsr4ScanDetectedGames.Name = "btnFsr4ScanDetectedGames"
        btnFsr4ScanDetectedGames.Size = New Size(120, 24)
        btnFsr4ScanDetectedGames.TabIndex = 9
        btnFsr4ScanDetectedGames.Text = "Scan now"
        btnFsr4ScanDetectedGames.UseVisualStyleBackColor = True
        ' 
        ' btnFsr4UseSelectedGame
        ' 
        btnFsr4UseSelectedGame.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnFsr4UseSelectedGame.Location = New Point(953, 166)
        btnFsr4UseSelectedGame.Name = "btnFsr4UseSelectedGame"
        btnFsr4UseSelectedGame.Size = New Size(120, 24)
        btnFsr4UseSelectedGame.TabIndex = 10
        btnFsr4UseSelectedGame.Text = "Use selected"
        btnFsr4UseSelectedGame.UseVisualStyleBackColor = True
        ' 
        ' btnFsr4BrowseGameExe
        ' 
        btnFsr4BrowseGameExe.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnFsr4BrowseGameExe.Location = New Point(1079, 166)
        btnFsr4BrowseGameExe.Name = "btnFsr4BrowseGameExe"
        btnFsr4BrowseGameExe.Size = New Size(139, 24)
        btnFsr4BrowseGameExe.TabIndex = 11
        btnFsr4BrowseGameExe.Text = "Browse game EXE"
        btnFsr4BrowseGameExe.UseVisualStyleBackColor = True
        ' 
        ' lvFsr4DetectedGames
        ' 
        lvFsr4DetectedGames.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lvFsr4DetectedGames.Columns.AddRange(New ColumnHeader() {colFsr4Game, colFsr4Platform, colFsr4Installed, colFsr4DetectedPath})
        lvFsr4DetectedGames.FullRowSelect = True
        lvFsr4DetectedGames.Location = New Point(12, 196)
        lvFsr4DetectedGames.MultiSelect = False
        lvFsr4DetectedGames.Name = "lvFsr4DetectedGames"
        lvFsr4DetectedGames.OwnerDraw = True
        lvFsr4DetectedGames.Size = New Size(1206, 227)
        lvFsr4DetectedGames.TabIndex = 12
        lvFsr4DetectedGames.UseCompatibleStateImageBehavior = False
        lvFsr4DetectedGames.View = View.Details
        ' 
        ' colFsr4Game
        ' 
        colFsr4Game.Text = "Game"
        colFsr4Game.Width = 280
        ' 
        ' colFsr4Platform
        ' 
        colFsr4Platform.Text = "Platform"
        colFsr4Platform.Width = 120
        '
        ' colFsr4Installed
        '
        colFsr4Installed.Text = "INT8"
        colFsr4Installed.Width = 160
        '
        ' colFsr4DetectedPath
        '
        colFsr4DetectedPath.Text = "Install Path"
        colFsr4DetectedPath.Width = 600
        ' 
        ' tabSettings
        ' 
        tabSettings.Controls.Add(grpSettings)
        tabSettings.Location = New Point(4, 24)
        tabSettings.Name = "tabSettings"
        tabSettings.Padding = New Padding(8)
        tabSettings.Size = New Size(1246, 679)
        tabSettings.TabIndex = 4
        tabSettings.Text = "Settings"
        tabSettings.UseVisualStyleBackColor = True
        ' 
        ' grpSettings
        ' 
        grpSettings.Controls.Add(lblCompatibilityListUrl)
        grpSettings.Controls.Add(txtCompatibilityListUrl)
        grpSettings.Controls.Add(lblWikiBaseUrl)
        grpSettings.Controls.Add(txtWikiBaseUrl)
        grpSettings.Controls.Add(lblStableReleaseUrl)
        grpSettings.Controls.Add(txtStableReleaseUrl)
        grpSettings.Controls.Add(lblNightlyReleaseUrl)
        grpSettings.Controls.Add(txtNightlyReleaseUrl)
        grpSettings.Controls.Add(lblInstallerReleaseUrl)
        grpSettings.Controls.Add(txtInstallerReleaseUrl)
        grpSettings.Controls.Add(lblGameProfilesCatalogUrl)
        grpSettings.Controls.Add(txtGameProfilesCatalogUrl)
        grpSettings.Controls.Add(flpSettingsToggles)
        grpSettings.Controls.Add(lblDefaultIniPath)
        grpSettings.Controls.Add(txtDefaultIniPath)
        grpSettings.Controls.Add(btnBrowseDefaultIni)
        grpSettings.Controls.Add(lblDefaultIniMode)
        grpSettings.Controls.Add(cmbDefaultIniMode)
        grpSettings.Controls.Add(grpDefaultInstall)
        grpSettings.Controls.Add(btnSaveSettings)
        grpSettings.Controls.Add(btnReloadSettings)
        grpSettings.Controls.Add(btnLoadDefaults)
        grpSettings.Controls.Add(btnOpenSettingsFile)
        grpSettings.Controls.Add(btnCheckForUpdates)
        grpSettings.Controls.Add(btnAbout)
        grpSettings.Controls.Add(btnExportDiagnostics)
        grpSettings.Controls.Add(lblUpdateNotice)
        grpSettings.Controls.Add(lblSettingsPath)
        grpSettings.Controls.Add(DarkThemeCheckBox)
        grpSettings.Dock = DockStyle.Fill
        grpSettings.Location = New Point(8, 8)
        grpSettings.Margin = New Padding(8)
        grpSettings.Name = "grpSettings"
        grpSettings.Size = New Size(1230, 663)
        grpSettings.TabIndex = 0
        grpSettings.TabStop = False
        grpSettings.Text = "Update && Links"
        ' 
        ' lblCompatibilityListUrl
        ' 
        lblCompatibilityListUrl.AutoSize = True
        lblCompatibilityListUrl.Location = New Point(12, 32)
        lblCompatibilityListUrl.Name = "lblCompatibilityListUrl"
        lblCompatibilityListUrl.Size = New Size(121, 15)
        lblCompatibilityListUrl.TabIndex = 0
        lblCompatibilityListUrl.Text = "Compatibility list URL"
        ' 
        ' txtCompatibilityListUrl
        ' 
        txtCompatibilityListUrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtCompatibilityListUrl.BackColor = SystemColors.Window
        txtCompatibilityListUrl.ForeColor = SystemColors.WindowText
        txtCompatibilityListUrl.Location = New Point(180, 28)
        txtCompatibilityListUrl.MinimumSize = New Size(0, 24)
        txtCompatibilityListUrl.Name = "txtCompatibilityListUrl"
        txtCompatibilityListUrl.Padding = New Padding(6, 3, 6, 3)
        txtCompatibilityListUrl.Size = New Size(1018, 24)
        txtCompatibilityListUrl.TabIndex = 1
        ' 
        ' lblWikiBaseUrl
        ' 
        lblWikiBaseUrl.AutoSize = True
        lblWikiBaseUrl.Location = New Point(12, 64)
        lblWikiBaseUrl.Name = "lblWikiBaseUrl"
        lblWikiBaseUrl.Size = New Size(81, 15)
        lblWikiBaseUrl.TabIndex = 4
        lblWikiBaseUrl.Text = "Wiki base URL"
        ' 
        ' txtWikiBaseUrl
        ' 
        txtWikiBaseUrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtWikiBaseUrl.BackColor = SystemColors.Window
        txtWikiBaseUrl.ForeColor = SystemColors.WindowText
        txtWikiBaseUrl.Location = New Point(180, 60)
        txtWikiBaseUrl.MinimumSize = New Size(0, 24)
        txtWikiBaseUrl.Name = "txtWikiBaseUrl"
        txtWikiBaseUrl.Padding = New Padding(6, 3, 6, 3)
        txtWikiBaseUrl.Size = New Size(1018, 24)
        txtWikiBaseUrl.TabIndex = 5
        ' 
        ' lblStableReleaseUrl
        ' 
        lblStableReleaseUrl.AutoSize = True
        lblStableReleaseUrl.Location = New Point(12, 96)
        lblStableReleaseUrl.Name = "lblStableReleaseUrl"
        lblStableReleaseUrl.Size = New Size(102, 15)
        lblStableReleaseUrl.TabIndex = 8
        lblStableReleaseUrl.Text = "Stable release URL"
        ' 
        ' txtStableReleaseUrl
        ' 
        txtStableReleaseUrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtStableReleaseUrl.BackColor = SystemColors.Window
        txtStableReleaseUrl.ForeColor = SystemColors.WindowText
        txtStableReleaseUrl.Location = New Point(180, 92)
        txtStableReleaseUrl.MinimumSize = New Size(0, 24)
        txtStableReleaseUrl.Name = "txtStableReleaseUrl"
        txtStableReleaseUrl.Padding = New Padding(6, 3, 6, 3)
        txtStableReleaseUrl.Size = New Size(1018, 24)
        txtStableReleaseUrl.TabIndex = 9
        ' 
        ' lblNightlyReleaseUrl
        ' 
        lblNightlyReleaseUrl.AutoSize = True
        lblNightlyReleaseUrl.Location = New Point(12, 128)
        lblNightlyReleaseUrl.Name = "lblNightlyReleaseUrl"
        lblNightlyReleaseUrl.Size = New Size(118, 15)
        lblNightlyReleaseUrl.TabIndex = 10
        lblNightlyReleaseUrl.Text = "Alternate release URL"
        ' 
        ' txtNightlyReleaseUrl
        ' 
        txtNightlyReleaseUrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtNightlyReleaseUrl.BackColor = SystemColors.Window
        txtNightlyReleaseUrl.ForeColor = SystemColors.WindowText
        txtNightlyReleaseUrl.Location = New Point(180, 124)
        txtNightlyReleaseUrl.MinimumSize = New Size(0, 24)
        txtNightlyReleaseUrl.Name = "txtNightlyReleaseUrl"
        txtNightlyReleaseUrl.Padding = New Padding(6, 3, 6, 3)
        txtNightlyReleaseUrl.Size = New Size(1018, 24)
        txtNightlyReleaseUrl.TabIndex = 11
        ' 
        ' lblInstallerReleaseUrl
        ' 
        lblInstallerReleaseUrl.AutoSize = True
        lblInstallerReleaseUrl.Location = New Point(12, 160)
        lblInstallerReleaseUrl.Name = "lblInstallerReleaseUrl"
        lblInstallerReleaseUrl.Size = New Size(111, 15)
        lblInstallerReleaseUrl.TabIndex = 12
        lblInstallerReleaseUrl.Text = "Installer release URL"
        ' 
        ' txtInstallerReleaseUrl
        ' 
        txtInstallerReleaseUrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtInstallerReleaseUrl.BackColor = SystemColors.Window
        txtInstallerReleaseUrl.ForeColor = SystemColors.WindowText
        txtInstallerReleaseUrl.Location = New Point(180, 156)
        txtInstallerReleaseUrl.MinimumSize = New Size(0, 24)
        txtInstallerReleaseUrl.Name = "txtInstallerReleaseUrl"
        txtInstallerReleaseUrl.Padding = New Padding(6, 3, 6, 3)
        txtInstallerReleaseUrl.Size = New Size(1018, 24)
        txtInstallerReleaseUrl.TabIndex = 13
        ' 
        ' lblGameProfilesCatalogUrl
        ' 
        lblGameProfilesCatalogUrl.AutoSize = True
        lblGameProfilesCatalogUrl.Location = New Point(12, 192)
        lblGameProfilesCatalogUrl.Name = "lblGameProfilesCatalogUrl"
        lblGameProfilesCatalogUrl.Size = New Size(113, 15)
        lblGameProfilesCatalogUrl.TabIndex = 14
        lblGameProfilesCatalogUrl.Text = "Game profiles URL"
        ' 
        ' txtGameProfilesCatalogUrl
        ' 
        txtGameProfilesCatalogUrl.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtGameProfilesCatalogUrl.BackColor = SystemColors.Window
        txtGameProfilesCatalogUrl.ForeColor = SystemColors.WindowText
        txtGameProfilesCatalogUrl.Location = New Point(180, 188)
        txtGameProfilesCatalogUrl.MinimumSize = New Size(0, 24)
        txtGameProfilesCatalogUrl.Name = "txtGameProfilesCatalogUrl"
        txtGameProfilesCatalogUrl.Padding = New Padding(6, 3, 6, 3)
        txtGameProfilesCatalogUrl.Size = New Size(1018, 24)
        txtGameProfilesCatalogUrl.TabIndex = 15
        '
        ' flpSettingsToggles
        '
        ' Startup toggles share one row; the flow layout keeps them from overlapping
        ' when their AutoSize width changes with the text, font or DPI.
        flpSettingsToggles.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        flpSettingsToggles.Controls.Add(chkAutoRefreshCompatibilityOnStartup)
        flpSettingsToggles.Controls.Add(chkAutoRefreshGameProfilesOnStartup)
        flpSettingsToggles.Controls.Add(chkAutoCheckInstallerUpdates)
        flpSettingsToggles.Controls.Add(chkShowExperimentalTabOnUnsupportedGpu)
        flpSettingsToggles.Location = New Point(12, 220)
        flpSettingsToggles.Margin = New Padding(0)
        flpSettingsToggles.Name = "flpSettingsToggles"
        flpSettingsToggles.Size = New Size(1078, 26)
        flpSettingsToggles.TabIndex = 16
        flpSettingsToggles.WrapContents = False
        '
        ' chkAutoRefreshCompatibilityOnStartup
        '
        chkAutoRefreshCompatibilityOnStartup.AutoSize = True
        chkAutoRefreshCompatibilityOnStartup.Margin = New Padding(0, 3, 24, 3)
        chkAutoRefreshCompatibilityOnStartup.Name = "chkAutoRefreshCompatibilityOnStartup"
        chkAutoRefreshCompatibilityOnStartup.Size = New Size(289, 19)
        chkAutoRefreshCompatibilityOnStartup.TabIndex = 0
        chkAutoRefreshCompatibilityOnStartup.Text = "Auto-refresh compatibility list on application start"
        chkAutoRefreshCompatibilityOnStartup.UseVisualStyleBackColor = True
        '
        ' chkAutoRefreshGameProfilesOnStartup
        '
        chkAutoRefreshGameProfilesOnStartup.AutoSize = True
        chkAutoRefreshGameProfilesOnStartup.Margin = New Padding(0, 3, 24, 3)
        chkAutoRefreshGameProfilesOnStartup.Name = "chkAutoRefreshGameProfilesOnStartup"
        chkAutoRefreshGameProfilesOnStartup.Size = New Size(273, 19)
        chkAutoRefreshGameProfilesOnStartup.TabIndex = 1
        chkAutoRefreshGameProfilesOnStartup.Text = "Auto-refresh game profiles on application start"
        chkAutoRefreshGameProfilesOnStartup.UseVisualStyleBackColor = True
        '
        ' chkAutoCheckInstallerUpdates
        '
        chkAutoCheckInstallerUpdates.AutoSize = True
        chkAutoCheckInstallerUpdates.Margin = New Padding(0, 3, 24, 3)
        chkAutoCheckInstallerUpdates.Name = "chkAutoCheckInstallerUpdates"
        chkAutoCheckInstallerUpdates.Size = New Size(234, 19)
        chkAutoCheckInstallerUpdates.TabIndex = 2
        chkAutoCheckInstallerUpdates.Text = "Auto-check installer updates on startup"
        chkAutoCheckInstallerUpdates.UseVisualStyleBackColor = True
        '
        ' chkShowExperimentalTabOnUnsupportedGpu
        '
        chkShowExperimentalTabOnUnsupportedGpu.AutoSize = True
        chkShowExperimentalTabOnUnsupportedGpu.Margin = New Padding(0, 3, 0, 3)
        chkShowExperimentalTabOnUnsupportedGpu.Name = "chkShowExperimentalTabOnUnsupportedGpu"
        chkShowExperimentalTabOnUnsupportedGpu.Size = New Size(136, 19)
        chkShowExperimentalTabOnUnsupportedGpu.TabIndex = 3
        chkShowExperimentalTabOnUnsupportedGpu.Text = "Force-show FSR4 tab"
        chkShowExperimentalTabOnUnsupportedGpu.UseVisualStyleBackColor = True
        ' 
        ' lblDefaultIniPath
        ' 
        lblDefaultIniPath.AutoSize = True
        lblDefaultIniPath.Location = New Point(12, 258)
        lblDefaultIniPath.Name = "lblDefaultIniPath"
        lblDefaultIniPath.Size = New Size(113, 15)
        lblDefaultIniPath.TabIndex = 20
        lblDefaultIniPath.Text = "Default INI template"
        ' 
        ' txtDefaultIniPath
        ' 
        txtDefaultIniPath.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtDefaultIniPath.BackColor = SystemColors.Window
        txtDefaultIniPath.ForeColor = SystemColors.WindowText
        txtDefaultIniPath.Location = New Point(180, 254)
        txtDefaultIniPath.MinimumSize = New Size(0, 24)
        txtDefaultIniPath.Name = "txtDefaultIniPath"
        txtDefaultIniPath.Padding = New Padding(6, 3, 6, 3)
        txtDefaultIniPath.Size = New Size(910, 24)
        txtDefaultIniPath.TabIndex = 21
        ' 
        ' btnBrowseDefaultIni
        ' 
        btnBrowseDefaultIni.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseDefaultIni.Location = New Point(1098, 254)
        btnBrowseDefaultIni.Name = "btnBrowseDefaultIni"
        btnBrowseDefaultIni.Size = New Size(100, 24)
        btnBrowseDefaultIni.TabIndex = 22
        btnBrowseDefaultIni.Text = "Browse"
        btnBrowseDefaultIni.UseVisualStyleBackColor = True
        ' 
        ' lblDefaultIniMode
        ' 
        lblDefaultIniMode.AutoSize = True
        lblDefaultIniMode.Location = New Point(12, 290)
        lblDefaultIniMode.Name = "lblDefaultIniMode"
        lblDefaultIniMode.Size = New Size(112, 15)
        lblDefaultIniMode.TabIndex = 23
        lblDefaultIniMode.Text = "Default INI behavior"
        ' 
        ' cmbDefaultIniMode
        ' 
        cmbDefaultIniMode.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDefaultIniMode.FormattingEnabled = True
        cmbDefaultIniMode.Items.AddRange(New Object() {"Off", "Merge", "Replace"})
        cmbDefaultIniMode.Location = New Point(180, 286)
        cmbDefaultIniMode.Name = "cmbDefaultIniMode"
        cmbDefaultIniMode.Size = New Size(220, 23)
        cmbDefaultIniMode.TabIndex = 24
        ' 
        ' grpDefaultInstall
        ' 
        grpDefaultInstall.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        grpDefaultInstall.Controls.Add(lblDefaultPreset)
        grpDefaultInstall.Controls.Add(cmbDefaultPreset)
        grpDefaultInstall.Controls.Add(lblDefaultHookName)
        grpDefaultInstall.Controls.Add(cmbDefaultHookName)
        grpDefaultInstall.Controls.Add(lblDefaultGpuVendor)
        grpDefaultInstall.Controls.Add(cmbDefaultGpuVendor)
        grpDefaultInstall.Controls.Add(lblDefaultDlssInputs)
        grpDefaultInstall.Controls.Add(chkDefaultDlssInputs)
        grpDefaultInstall.Controls.Add(lblDefaultFgType)
        grpDefaultInstall.Controls.Add(cmbDefaultFgType)
        grpDefaultInstall.Controls.Add(lblDefaultConflictMode)
        grpDefaultInstall.Controls.Add(cmbDefaultConflictMode)
        grpDefaultInstall.Controls.Add(btnApplyDefaults)
        grpDefaultInstall.Location = New Point(12, 330)
        grpDefaultInstall.Name = "grpDefaultInstall"
        grpDefaultInstall.Size = New Size(1186, 250)
        grpDefaultInstall.TabIndex = 25
        grpDefaultInstall.TabStop = False
        grpDefaultInstall.Text = "Default Install Options"
        ' 
        ' lblDefaultPreset
        ' 
        lblDefaultPreset.AutoSize = True
        lblDefaultPreset.Location = New Point(12, 32)
        lblDefaultPreset.Name = "lblDefaultPreset"
        lblDefaultPreset.Size = New Size(39, 15)
        lblDefaultPreset.TabIndex = 0
        lblDefaultPreset.Text = "Preset"
        ' 
        ' cmbDefaultPreset
        ' 
        cmbDefaultPreset.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cmbDefaultPreset.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDefaultPreset.FormattingEnabled = True
        cmbDefaultPreset.Items.AddRange(New Object() {"Custom", "NVIDIA + DLSS Inputs", "AMD/Intel (DLSS Inputs Off)"})
        cmbDefaultPreset.Location = New Point(180, 28)
        cmbDefaultPreset.Name = "cmbDefaultPreset"
        cmbDefaultPreset.Size = New Size(480, 23)
        cmbDefaultPreset.TabIndex = 1
        ' 
        ' lblDefaultHookName
        ' 
        lblDefaultHookName.AutoSize = True
        lblDefaultHookName.Location = New Point(12, 64)
        lblDefaultHookName.Name = "lblDefaultHookName"
        lblDefaultHookName.Size = New Size(85, 15)
        lblDefaultHookName.TabIndex = 2
        lblDefaultHookName.Text = "Hook filename"
        ' 
        ' cmbDefaultHookName
        ' 
        cmbDefaultHookName.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cmbDefaultHookName.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDefaultHookName.FormattingEnabled = True
        cmbDefaultHookName.Items.AddRange(New Object() {"dxgi.dll", "winmm.dll", "version.dll", "dbghelp.dll", "d3d12.dll", "wininet.dll", "winhttp.dll", "OptiScaler.asi"})
        cmbDefaultHookName.Location = New Point(180, 60)
        cmbDefaultHookName.Name = "cmbDefaultHookName"
        cmbDefaultHookName.Size = New Size(480, 23)
        cmbDefaultHookName.TabIndex = 3
        ' 
        ' lblDefaultGpuVendor
        ' 
        lblDefaultGpuVendor.AutoSize = True
        lblDefaultGpuVendor.Location = New Point(12, 96)
        lblDefaultGpuVendor.Name = "lblDefaultGpuVendor"
        lblDefaultGpuVendor.Size = New Size(80, 15)
        lblDefaultGpuVendor.TabIndex = 4
        lblDefaultGpuVendor.Text = "GPU selection"
        ' 
        ' cmbDefaultGpuVendor
        ' 
        cmbDefaultGpuVendor.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cmbDefaultGpuVendor.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDefaultGpuVendor.FormattingEnabled = True
        cmbDefaultGpuVendor.Items.AddRange(New Object() {"Auto (detect)", "NVIDIA", "AMD/Intel"})
        cmbDefaultGpuVendor.Location = New Point(180, 92)
        cmbDefaultGpuVendor.Name = "cmbDefaultGpuVendor"
        cmbDefaultGpuVendor.Size = New Size(480, 23)
        cmbDefaultGpuVendor.TabIndex = 5
        ' 
        ' lblDefaultDlssInputs
        ' 
        lblDefaultDlssInputs.AutoSize = True
        lblDefaultDlssInputs.Location = New Point(12, 128)
        lblDefaultDlssInputs.Name = "lblDefaultDlssInputs"
        lblDefaultDlssInputs.Size = New Size(69, 15)
        lblDefaultDlssInputs.TabIndex = 6
        lblDefaultDlssInputs.Text = "DLSS inputs"
        ' 
        ' chkDefaultDlssInputs
        ' 
        chkDefaultDlssInputs.AutoSize = True
        chkDefaultDlssInputs.Location = New Point(180, 126)
        chkDefaultDlssInputs.Name = "chkDefaultDlssInputs"
        chkDefaultDlssInputs.Size = New Size(171, 19)
        chkDefaultDlssInputs.TabIndex = 7
        chkDefaultDlssInputs.Text = "Enable DLSS input spoofing"
        chkDefaultDlssInputs.UseVisualStyleBackColor = True
        ' 
        ' lblDefaultFgType
        ' 
        lblDefaultFgType.AutoSize = True
        lblDefaultFgType.Location = New Point(12, 160)
        lblDefaultFgType.Name = "lblDefaultFgType"
        lblDefaultFgType.Size = New Size(100, 15)
        lblDefaultFgType.TabIndex = 8
        lblDefaultFgType.Text = "Frame generation"
        ' 
        ' cmbDefaultFgType
        ' 
        cmbDefaultFgType.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cmbDefaultFgType.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDefaultFgType.FormattingEnabled = True
        cmbDefaultFgType.Items.AddRange(New Object() {"Auto (no change)", "None", "OptiFG (DX12 only)", "Nukem's dlssg-to-fsr3"})
        cmbDefaultFgType.Location = New Point(180, 156)
        cmbDefaultFgType.Name = "cmbDefaultFgType"
        cmbDefaultFgType.Size = New Size(480, 23)
        cmbDefaultFgType.TabIndex = 9
        ' 
        ' lblDefaultConflictMode
        ' 
        lblDefaultConflictMode.AutoSize = True
        lblDefaultConflictMode.Location = New Point(12, 192)
        lblDefaultConflictMode.Name = "lblDefaultConflictMode"
        lblDefaultConflictMode.Size = New Size(71, 15)
        lblDefaultConflictMode.TabIndex = 10
        lblDefaultConflictMode.Text = "Existing files"
        ' 
        ' cmbDefaultConflictMode
        ' 
        cmbDefaultConflictMode.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cmbDefaultConflictMode.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDefaultConflictMode.FormattingEnabled = True
        cmbDefaultConflictMode.Items.AddRange(New Object() {"Backup and overwrite", "Overwrite", "Skip existing"})
        cmbDefaultConflictMode.Location = New Point(180, 188)
        cmbDefaultConflictMode.Name = "cmbDefaultConflictMode"
        cmbDefaultConflictMode.Size = New Size(480, 23)
        cmbDefaultConflictMode.TabIndex = 11
        ' 
        ' btnApplyDefaults
        ' 
        btnApplyDefaults.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnApplyDefaults.Location = New Point(1054, 210)
        btnApplyDefaults.Name = "btnApplyDefaults"
        btnApplyDefaults.Size = New Size(120, 30)
        btnApplyDefaults.TabIndex = 12
        btnApplyDefaults.Text = "Apply defaults"
        btnApplyDefaults.UseVisualStyleBackColor = True
        ' 
        ' btnSaveSettings
        ' 
        btnSaveSettings.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnSaveSettings.Location = New Point(12, 597)
        btnSaveSettings.Name = "btnSaveSettings"
        btnSaveSettings.Size = New Size(120, 30)
        btnSaveSettings.TabIndex = 19
        btnSaveSettings.Text = "Save settings"
        btnSaveSettings.UseVisualStyleBackColor = True
        ' 
        ' btnReloadSettings
        ' 
        btnReloadSettings.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnReloadSettings.Location = New Point(140, 597)
        btnReloadSettings.Name = "btnReloadSettings"
        btnReloadSettings.Size = New Size(120, 30)
        btnReloadSettings.TabIndex = 20
        btnReloadSettings.Text = "Reload"
        btnReloadSettings.UseVisualStyleBackColor = True
        ' 
        ' btnLoadDefaults
        ' 
        btnLoadDefaults.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnLoadDefaults.Location = New Point(268, 597)
        btnLoadDefaults.Name = "btnLoadDefaults"
        btnLoadDefaults.Size = New Size(120, 30)
        btnLoadDefaults.TabIndex = 21
        btnLoadDefaults.Text = "Load defaults"
        btnLoadDefaults.UseVisualStyleBackColor = True
        ' 
        ' btnOpenSettingsFile
        ' 
        btnOpenSettingsFile.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnOpenSettingsFile.Location = New Point(396, 597)
        btnOpenSettingsFile.Name = "btnOpenSettingsFile"
        btnOpenSettingsFile.Size = New Size(160, 30)
        btnOpenSettingsFile.TabIndex = 22
        btnOpenSettingsFile.Text = "Open settings file"
        btnOpenSettingsFile.UseVisualStyleBackColor = True
        ' 
        ' btnCheckForUpdates
        ' 
        btnCheckForUpdates.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnCheckForUpdates.Location = New Point(564, 597)
        btnCheckForUpdates.Name = "btnCheckForUpdates"
        btnCheckForUpdates.Size = New Size(160, 30)
        btnCheckForUpdates.TabIndex = 23
        btnCheckForUpdates.Text = "Check for updates"
        btnCheckForUpdates.UseVisualStyleBackColor = True
        ' 
        ' btnAbout
        ' 
        btnAbout.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnAbout.Location = New Point(892, 597)
        btnAbout.Name = "btnAbout"
        btnAbout.Size = New Size(138, 30)
        btnAbout.TabIndex = 24
        btnAbout.Text = "About"
        btnAbout.UseVisualStyleBackColor = True
        ' 
        ' btnExportDiagnostics
        ' 
        btnExportDiagnostics.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnExportDiagnostics.Location = New Point(1038, 597)
        btnExportDiagnostics.Name = "btnExportDiagnostics"
        btnExportDiagnostics.Size = New Size(160, 30)
        btnExportDiagnostics.TabIndex = 25
        btnExportDiagnostics.Text = "Export diagnostics"
        btnExportDiagnostics.UseVisualStyleBackColor = True
        ' 
        ' lblUpdateNotice
        ' 
        lblUpdateNotice.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        lblUpdateNotice.AutoSize = True
        lblUpdateNotice.Location = New Point(594, 579)
        lblUpdateNotice.Name = "lblUpdateNotice"
        lblUpdateNotice.Size = New Size(94, 15)
        lblUpdateNotice.TabIndex = 25
        lblUpdateNotice.Text = "Update available"
        lblUpdateNotice.Visible = False
        ' 
        ' lblSettingsPath
        ' 
        lblSettingsPath.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        lblSettingsPath.AutoSize = True
        lblSettingsPath.Location = New Point(12, 637)
        lblSettingsPath.Name = "lblSettingsPath"
        lblSettingsPath.Size = New Size(139, 15)
        lblSettingsPath.TabIndex = 24
        lblSettingsPath.Text = "Settings file: (not loaded)"
        ' 
        ' DarkThemeCheckBox
        ' 
        DarkThemeCheckBox.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        DarkThemeCheckBox.AutoSize = True
        DarkThemeCheckBox.Location = New Point(1111, 223)
        DarkThemeCheckBox.Name = "DarkThemeCheckBox"
        DarkThemeCheckBox.Size = New Size(87, 19)
        DarkThemeCheckBox.TabIndex = 27
        DarkThemeCheckBox.Text = "Dark theme"
        DarkThemeCheckBox.UseVisualStyleBackColor = True
        ' 
        ' grpLog
        ' 
        grpLog.Controls.Add(txtLog)
        grpLog.Controls.Add(logHeaderPanel)
        grpLog.Dock = DockStyle.Fill
        grpLog.Location = New Point(8, 715)
        grpLog.Margin = New Padding(0, 8, 0, 0)
        grpLog.Name = "grpLog"
        grpLog.Padding = New Padding(8, 24, 8, 8)
        grpLog.Size = New Size(1254, 152)
        grpLog.TabIndex = 1
        grpLog.TabStop = False
        grpLog.Text = "Log Output"
        ' 
        ' logHeaderPanel
        ' 
        logHeaderPanel.Controls.Add(lblLogFilter)
        logHeaderPanel.Controls.Add(txtLogFilter)
        logHeaderPanel.Controls.Add(lblLogSeverity)
        logHeaderPanel.Controls.Add(cmbLogSeverity)
        logHeaderPanel.Controls.Add(btnLogCopy)
        logHeaderPanel.Controls.Add(btnLogSave)
        logHeaderPanel.Controls.Add(btnLogClear)
        logHeaderPanel.Dock = DockStyle.Top
        logHeaderPanel.FlowDirection = FlowDirection.LeftToRight
        logHeaderPanel.Location = New Point(8, 24)
        logHeaderPanel.Margin = New Padding(0)
        logHeaderPanel.Name = "logHeaderPanel"
        logHeaderPanel.Padding = New Padding(0, 0, 0, 6)
        logHeaderPanel.Size = New Size(1238, 34)
        logHeaderPanel.TabIndex = 0
        logHeaderPanel.WrapContents = False
        ' 
        ' lblLogFilter
        ' 
        lblLogFilter.AccessibleName = "Log filter label"
        lblLogFilter.AutoSize = True
        lblLogFilter.Margin = New Padding(0, 7, 6, 0)
        lblLogFilter.Name = "lblLogFilter"
        lblLogFilter.Size = New Size(36, 15)
        lblLogFilter.TabIndex = 0
        lblLogFilter.Text = "Filter"
        ' 
        ' txtLogFilter
        ' 
        txtLogFilter.AccessibleName = "Filter log messages"
        txtLogFilter.AccessibleDescription = "Show only log lines containing this text."
        txtLogFilter.Margin = New Padding(0, 2, 14, 0)
        txtLogFilter.MinimumSize = New Size(0, 24)
        txtLogFilter.Name = "txtLogFilter"
        txtLogFilter.Padding = New Padding(6, 3, 6, 3)
        txtLogFilter.Size = New Size(240, 24)
        txtLogFilter.TabIndex = 1
        ' 
        ' lblLogSeverity
        ' 
        lblLogSeverity.AccessibleName = "Log level label"
        lblLogSeverity.AutoSize = True
        lblLogSeverity.Margin = New Padding(0, 7, 6, 0)
        lblLogSeverity.Name = "lblLogSeverity"
        lblLogSeverity.Size = New Size(36, 15)
        lblLogSeverity.TabIndex = 2
        lblLogSeverity.Text = "Level"
        ' 
        ' cmbLogSeverity
        ' 
        cmbLogSeverity.AccessibleName = "Minimum log level"
        cmbLogSeverity.AccessibleDescription = "Hide log lines below the selected severity."
        cmbLogSeverity.DropDownStyle = ComboBoxStyle.DropDownList
        cmbLogSeverity.FormattingEnabled = True
        cmbLogSeverity.Items.AddRange(New Object() {"All", "Warnings and errors", "Errors only"})
        cmbLogSeverity.Margin = New Padding(0, 2, 14, 0)
        cmbLogSeverity.Name = "cmbLogSeverity"
        cmbLogSeverity.Size = New Size(170, 23)
        cmbLogSeverity.TabIndex = 3
        ' 
        ' btnLogCopy
        ' 
        btnLogCopy.AccessibleName = "Copy log"
        btnLogCopy.AccessibleDescription = "Copy the visible log lines to the clipboard."
        btnLogCopy.Margin = New Padding(0, 1, 6, 0)
        btnLogCopy.Name = "btnLogCopy"
        btnLogCopy.Size = New Size(70, 26)
        btnLogCopy.TabIndex = 4
        btnLogCopy.Text = "Copy"
        btnLogCopy.UseVisualStyleBackColor = True
        ' 
        ' btnLogSave
        ' 
        btnLogSave.AccessibleName = "Save log"
        btnLogSave.AccessibleDescription = "Save the visible log lines to a text file."
        btnLogSave.Margin = New Padding(0, 1, 6, 0)
        btnLogSave.Name = "btnLogSave"
        btnLogSave.Size = New Size(70, 26)
        btnLogSave.TabIndex = 5
        btnLogSave.Text = "Save"
        btnLogSave.UseVisualStyleBackColor = True
        ' 
        ' btnLogClear
        ' 
        btnLogClear.AccessibleName = "Clear log"
        btnLogClear.AccessibleDescription = "Remove all log lines from this session's view."
        btnLogClear.Margin = New Padding(0, 1, 6, 0)
        btnLogClear.Name = "btnLogClear"
        btnLogClear.Size = New Size(70, 26)
        btnLogClear.TabIndex = 6
        btnLogClear.Text = "Clear"
        btnLogClear.UseVisualStyleBackColor = True
        ' 
        ' txtLog
        ' 
        txtLog.AccessibleName = "Log output"
        txtLog.AccessibleDescription = "Running record of everything the installer has done this session."
        txtLog.BackColor = SystemColors.Window
        txtLog.Dock = DockStyle.Fill
        txtLog.ForeColor = SystemColors.WindowText
        txtLog.Location = New Point(8, 58)
        txtLog.MinimumSize = New Size(0, 24)
        txtLog.Name = "txtLog"
        txtLog.Padding = New Padding(6, 3, 6, 3)
        txtLog.ReadOnly = True
        txtLog.ScrollBars = RichTextBoxScrollBars.Vertical
        txtLog.Size = New Size(1238, 86)
        txtLog.TabIndex = 1
        txtLog.WordWrap = False
        ' 
        ' statusStrip
        ' 
        statusStrip.Items.AddRange(New ToolStripItem() {toolDetectedLabel, toolProgressBar, toolCancelButton, toolStatusLabel})
        statusStrip.Location = New Point(0, 875)
        statusStrip.Name = "statusStrip"
        statusStrip.Size = New Size(1270, 22)
        statusStrip.TabIndex = 1
        statusStrip.Text = "statusStrip"
        ' 
        ' toolStatusLabel
        ' 
        toolStatusLabel.Name = "toolStatusLabel"
        toolStatusLabel.Spring = True
        toolStatusLabel.Size = New Size(39, 17)
        toolStatusLabel.Text = "Ready"
        ' 
        ' toolDetectedLabel
        ' 
        toolDetectedLabel.Name = "toolDetectedLabel"
        toolDetectedLabel.Size = New Size(87, 17)
        toolDetectedLabel.Text = "Detected: none"
        ' 
        ' toolProgressBar
        ' 
        toolProgressBar.Name = "toolProgressBar"
        toolProgressBar.Size = New Size(200, 16)
        ' 
        ' toolCancelButton
        ' 
        toolCancelButton.AccessibleName = "Cancel the running operation"
        toolCancelButton.DisplayStyle = ToolStripItemDisplayStyle.Text
        toolCancelButton.Name = "toolCancelButton"
        toolCancelButton.Size = New Size(52, 20)
        toolCancelButton.Text = "Cancel"
        toolCancelButton.ToolTipText = "Stop the running scan or download. Work already completed is kept."
        toolCancelButton.Visible = False
        ' 
        ' MainForm
        ' 
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1270, 897)
        Controls.Add(mainLayout)
        Controls.Add(statusStrip)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MinimumSize = New Size(1286, 936)
        Name = "MainForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "OptiScaler Installer"
        mainLayout.ResumeLayout(False)
        tabMain.ResumeLayout(False)
        tabCompatibility.ResumeLayout(False)
        compatFooterPanel.ResumeLayout(False)
        compatActionsPanel.ResumeLayout(False)
        compatStatusPanel.ResumeLayout(False)
        compatStatusPanel.PerformLayout()
        compatHeaderPanel.ResumeLayout(False)
        compatHeaderPanel.PerformLayout()
        compatHeaderLeftPanel.ResumeLayout(False)
        compatHeaderLeftPanel.PerformLayout()
        compatHeaderRightPanel.ResumeLayout(False)
        tabInstall.ResumeLayout(False)
        installLayout.ResumeLayout(False)
        grpGame.ResumeLayout(False)
        grpGame.PerformLayout()
        grpSource.ResumeLayout(False)
        grpSource.PerformLayout()
        grpHook.ResumeLayout(False)
        grpHook.PerformLayout()
        grpGpu.ResumeLayout(False)
        grpGpu.PerformLayout()
        grpFg.ResumeLayout(False)
        grpFg.PerformLayout()
        grpBehavior.ResumeLayout(False)
        grpBehavior.PerformLayout()
        grpActions.ResumeLayout(False)
        grpActions.PerformLayout()
        tabAddons.ResumeLayout(False)
        addonsLayout.ResumeLayout(False)
        grpFakenvapi.ResumeLayout(False)
        grpFakenvapi.PerformLayout()
        grpNukem.ResumeLayout(False)
        grpNukem.PerformLayout()
        grpNvngx.ResumeLayout(False)
        grpNvngx.PerformLayout()
        grpReshade.ResumeLayout(False)
        grpReshade.PerformLayout()
        grpSpecialK.ResumeLayout(False)
        grpSpecialK.PerformLayout()
        grpAsi.ResumeLayout(False)
        grpAsi.PerformLayout()
        tabExperimental.ResumeLayout(False)
        experimentalLayout.ResumeLayout(False)
        grpFsr4Package.ResumeLayout(False)
        grpFsr4Package.PerformLayout()
        grpFsr4Options.ResumeLayout(False)
        grpFsr4Options.PerformLayout()
        grpFsr4Actions.ResumeLayout(False)
        grpFsr4Actions.PerformLayout()
        tabSettings.ResumeLayout(False)
        grpSettings.ResumeLayout(False)
        grpSettings.PerformLayout()
        flpSettingsToggles.ResumeLayout(False)
        flpSettingsToggles.PerformLayout()
        grpDefaultInstall.ResumeLayout(False)
        grpDefaultInstall.PerformLayout()
        grpLog.ResumeLayout(False)
        logHeaderPanel.ResumeLayout(False)
        logHeaderPanel.PerformLayout()
        statusStrip.ResumeLayout(False)
        statusStrip.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents mainLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents tabMain As System.Windows.Forms.TabControl
    Friend WithEvents tabInstall As System.Windows.Forms.TabPage
    Friend WithEvents tabAddons As System.Windows.Forms.TabPage
    Friend WithEvents tabCompatibility As System.Windows.Forms.TabPage
    Friend WithEvents tabExperimental As System.Windows.Forms.TabPage
    Friend WithEvents tabSettings As System.Windows.Forms.TabPage
    Friend WithEvents installLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents addonsLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents experimentalLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents grpLog As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents grpSettings As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblCompatibilityListUrl As System.Windows.Forms.Label
    Friend WithEvents txtCompatibilityListUrl As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblWikiBaseUrl As System.Windows.Forms.Label
    Friend WithEvents txtWikiBaseUrl As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblStableReleaseUrl As System.Windows.Forms.Label
    Friend WithEvents txtStableReleaseUrl As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblNightlyReleaseUrl As System.Windows.Forms.Label
    Friend WithEvents txtNightlyReleaseUrl As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblInstallerReleaseUrl As System.Windows.Forms.Label
    Friend WithEvents txtInstallerReleaseUrl As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblGameProfilesCatalogUrl As System.Windows.Forms.Label
    Friend WithEvents txtGameProfilesCatalogUrl As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents flpSettingsToggles As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents chkAutoRefreshCompatibilityOnStartup As System.Windows.Forms.CheckBox
    Friend WithEvents chkAutoRefreshGameProfilesOnStartup As System.Windows.Forms.CheckBox
    Friend WithEvents chkAutoCheckInstallerUpdates As System.Windows.Forms.CheckBox
    Friend WithEvents chkShowExperimentalTabOnUnsupportedGpu As System.Windows.Forms.CheckBox
    Friend WithEvents lblDefaultIniPath As System.Windows.Forms.Label
    Friend WithEvents txtDefaultIniPath As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents btnBrowseDefaultIni As System.Windows.Forms.Button
    Friend WithEvents lblDefaultIniMode As System.Windows.Forms.Label
    Friend WithEvents cmbDefaultIniMode As System.Windows.Forms.ComboBox
    Friend WithEvents grpDefaultInstall As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblDefaultPreset As System.Windows.Forms.Label
    Friend WithEvents cmbDefaultPreset As System.Windows.Forms.ComboBox
    Friend WithEvents lblDefaultHookName As System.Windows.Forms.Label
    Friend WithEvents cmbDefaultHookName As System.Windows.Forms.ComboBox
    Friend WithEvents lblDefaultGpuVendor As System.Windows.Forms.Label
    Friend WithEvents cmbDefaultGpuVendor As System.Windows.Forms.ComboBox
    Friend WithEvents lblDefaultDlssInputs As System.Windows.Forms.Label
    Friend WithEvents chkDefaultDlssInputs As System.Windows.Forms.CheckBox
    Friend WithEvents lblDefaultFgType As System.Windows.Forms.Label
    Friend WithEvents cmbDefaultFgType As System.Windows.Forms.ComboBox
    Friend WithEvents lblDefaultConflictMode As System.Windows.Forms.Label
    Friend WithEvents cmbDefaultConflictMode As System.Windows.Forms.ComboBox
    Friend WithEvents btnApplyDefaults As System.Windows.Forms.Button
    Friend WithEvents btnSaveSettings As System.Windows.Forms.Button
    Friend WithEvents btnReloadSettings As System.Windows.Forms.Button
    Friend WithEvents btnLoadDefaults As System.Windows.Forms.Button
    Friend WithEvents btnOpenSettingsFile As System.Windows.Forms.Button
    Friend WithEvents btnCheckForUpdates As System.Windows.Forms.Button
    Friend WithEvents btnAbout As System.Windows.Forms.Button
    Friend WithEvents btnExportDiagnostics As System.Windows.Forms.Button
    Friend WithEvents lblUpdateNotice As System.Windows.Forms.Label
    Friend WithEvents lblSettingsPath As System.Windows.Forms.Label
    Friend WithEvents grpGame As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblEngineWarning As System.Windows.Forms.Label
    Friend WithEvents txtGameFolder As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblGameFolderLabel As System.Windows.Forms.Label
    Friend WithEvents btnBrowseGameExe As System.Windows.Forms.Button
    Friend WithEvents txtGameExe As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblGameExe As System.Windows.Forms.Label
    Friend WithEvents grpSource As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents btnRefreshReleases As System.Windows.Forms.Button
    Friend WithEvents btnBrowseArchive As System.Windows.Forms.Button
    Friend WithEvents txtLocalArchive As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblNightlyInfo As System.Windows.Forms.Label
    Friend WithEvents lblStableInfo As System.Windows.Forms.Label
    Friend WithEvents rbLocal As System.Windows.Forms.RadioButton
    Friend WithEvents rbNightly As System.Windows.Forms.RadioButton
    Friend WithEvents rbStable As System.Windows.Forms.RadioButton
    Friend WithEvents grpHook As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblHookHint As System.Windows.Forms.Label
    Friend WithEvents cmbHookName As System.Windows.Forms.ComboBox
    Friend WithEvents lblHookName As System.Windows.Forms.Label
    Friend WithEvents grpGpu As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblDlssHint As System.Windows.Forms.Label
    Friend WithEvents chkDlssInputs As System.Windows.Forms.CheckBox
    Friend WithEvents rbGpuAmdIntel As System.Windows.Forms.RadioButton
    Friend WithEvents rbGpuNvidia As System.Windows.Forms.RadioButton
    Friend WithEvents grpFg As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblFgHint As System.Windows.Forms.Label
    Friend WithEvents cmbFgType As System.Windows.Forms.ComboBox
    Friend WithEvents lblFgType As System.Windows.Forms.Label
    Friend WithEvents grpBehavior As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblBehaviorHint As System.Windows.Forms.Label
    Friend WithEvents chkPreserveIni As System.Windows.Forms.CheckBox
    Friend WithEvents cmbConflictMode As System.Windows.Forms.ComboBox
    Friend WithEvents lblConflictMode As System.Windows.Forms.Label
    Friend WithEvents grpActions As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblActionNote As System.Windows.Forms.Label
    Friend WithEvents lblOnlineWarning As System.Windows.Forms.Label
    Friend WithEvents DarkThemeCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents btnOpenGameFolder As System.Windows.Forms.Button
    Friend WithEvents btnEditIni As System.Windows.Forms.Button
    Friend WithEvents btnUninstall As System.Windows.Forms.Button
    Friend WithEvents btnInstall As System.Windows.Forms.Button
    Friend WithEvents lblInstalledStatus As System.Windows.Forms.Label
    Friend WithEvents chkInstallOptiPatcher As System.Windows.Forms.CheckBox
    Friend WithEvents lblInstallOptiPatcherStatus As System.Windows.Forms.Label
    Friend WithEvents grpFakenvapi As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblFakenvapiHint As System.Windows.Forms.Label
    Friend WithEvents btnBrowseFakenvapiFolder As System.Windows.Forms.Button
    Friend WithEvents txtFakenvapiFolder As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblFakenvapiFolder As System.Windows.Forms.Label
    Friend WithEvents grpNukem As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblNukemHint As System.Windows.Forms.Label
    Friend WithEvents btnBrowseNukemDll As System.Windows.Forms.Button
    Friend WithEvents txtNukemDll As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblNukemDll As System.Windows.Forms.Label
    Friend WithEvents grpNvngx As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblNvngxHint As System.Windows.Forms.Label
    Friend WithEvents btnBrowseNvngx As System.Windows.Forms.Button
    Friend WithEvents txtNvngxDll As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblNvngxDll As System.Windows.Forms.Label
    Friend WithEvents grpReshade As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblReshadeHint As System.Windows.Forms.Label
    Friend WithEvents btnBrowseReshade As System.Windows.Forms.Button
    Friend WithEvents txtReshadeDll As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblReshadeDll As System.Windows.Forms.Label
    Friend WithEvents chkEnableReshade As System.Windows.Forms.CheckBox
    Friend WithEvents grpSpecialK As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblSpecialKHint As System.Windows.Forms.Label
    Friend WithEvents chkCreateSpecialKMarker As System.Windows.Forms.CheckBox
    Friend WithEvents btnBrowseSpecialK As System.Windows.Forms.Button
    Friend WithEvents txtSpecialKDll As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblSpecialKDll As System.Windows.Forms.Label
    Friend WithEvents chkEnableSpecialK As System.Windows.Forms.CheckBox
    Friend WithEvents grpAsi As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblAsiHint As System.Windows.Forms.Label
    Friend WithEvents btnBrowsePluginsPath As System.Windows.Forms.Button
    Friend WithEvents txtPluginsPath As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblPluginsPath As System.Windows.Forms.Label
    Friend WithEvents chkLoadAsiPlugins As System.Windows.Forms.CheckBox
    Friend WithEvents lblOptiPatcherSource As System.Windows.Forms.Label
    Friend WithEvents cmbOptiPatcherSource As System.Windows.Forms.ComboBox
    Friend WithEvents btnOptiPatcherRefresh As System.Windows.Forms.Button
    Friend WithEvents lblOptiPatcherRelease As System.Windows.Forms.Label
    Friend WithEvents txtOptiPatcherLocalFile As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents btnBrowseOptiPatcherLocal As System.Windows.Forms.Button
    Friend WithEvents btnInstallOptiPatcher As System.Windows.Forms.Button
    Friend WithEvents btnRemoveOptiPatcher As System.Windows.Forms.Button
    Friend WithEvents lblOptiPatcherStatus As System.Windows.Forms.Label
    Friend WithEvents grpFsr4Package As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblFsr4PackageFolder As System.Windows.Forms.Label
    Friend WithEvents txtFsr4PackageFolder As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents btnBrowseFsr4PackageFolder As System.Windows.Forms.Button
    Friend WithEvents lblFsr4PackageHint As System.Windows.Forms.Label
    Friend WithEvents grpFsr4Options As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents chkFsr4EnableUpdate As System.Windows.Forms.CheckBox
    Friend WithEvents chkFsr4EnableAgility As System.Windows.Forms.CheckBox
    Friend WithEvents chkFsr4ForceInt8 As System.Windows.Forms.CheckBox
    Friend WithEvents lblFsr4OptionsHint As System.Windows.Forms.Label
    Friend WithEvents grpFsr4Actions As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblFsr4TargetGame As System.Windows.Forms.Label
    Friend WithEvents txtFsr4TargetGameFolder As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents btnFsr4PickGame As System.Windows.Forms.Button
    Friend WithEvents btnFsr4Apply As System.Windows.Forms.Button
    Friend WithEvents btnFsr4Remove As System.Windows.Forms.Button
    Friend WithEvents btnFsr4RefreshStatus As System.Windows.Forms.Button
    Friend WithEvents lblFsr4Status As System.Windows.Forms.Label
    Friend WithEvents lblFsr4ActionHint As System.Windows.Forms.Label
    Friend WithEvents lblFsr4DetectedGames As System.Windows.Forms.Label
    Friend WithEvents btnFsr4ScanDetectedGames As System.Windows.Forms.Button
    Friend WithEvents btnFsr4UseSelectedGame As System.Windows.Forms.Button
    Friend WithEvents btnFsr4BrowseGameExe As System.Windows.Forms.Button
    Friend WithEvents lvFsr4DetectedGames As OptiScalerInstaller.ThemedListView
    Friend WithEvents colFsr4Game As System.Windows.Forms.ColumnHeader
    Friend WithEvents colFsr4Platform As System.Windows.Forms.ColumnHeader
    Friend WithEvents colFsr4Installed As System.Windows.Forms.ColumnHeader
    Friend WithEvents colFsr4DetectedPath As System.Windows.Forms.ColumnHeader
    Friend WithEvents compatFooterPanel As System.Windows.Forms.Panel
    Friend WithEvents compatActionsPanel As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents compatStatusPanel As System.Windows.Forms.Panel
    Friend WithEvents compatHeaderPanel As System.Windows.Forms.Panel
    Friend WithEvents compatHeaderLeftPanel As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents compatHeaderRightPanel As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblCompatibilityNote As System.Windows.Forms.Label
    Friend WithEvents chkHideNonDetected As System.Windows.Forms.CheckBox
    Friend WithEvents btnOpenWiki As System.Windows.Forms.Button
    Friend WithEvents btnRefreshCompatibility As System.Windows.Forms.Button
    Friend WithEvents lvCompatibility As OptiScalerInstaller.ThemedListView
    Friend WithEvents colCompatName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatDetected As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatPlatform As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatAntiCheat As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatPath As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatInstalled As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatPatcher As System.Windows.Forms.ColumnHeader
    Friend WithEvents txtGameSearch As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblSearch As System.Windows.Forms.Label
    Friend WithEvents btnScanDetected As System.Windows.Forms.Button
    Friend WithEvents btnDeepScanDrives As System.Windows.Forms.Button
    Friend WithEvents btnBulkActions As System.Windows.Forms.Button
    Friend WithEvents btnUseDetected As System.Windows.Forms.Button
    Friend WithEvents btnCompatOpenFolder As System.Windows.Forms.Button
    Friend WithEvents btnCompatEditIni As System.Windows.Forms.Button
    Friend WithEvents btnCompatInstallUpdate As System.Windows.Forms.Button
    Friend WithEvents btnCompatUninstall As System.Windows.Forms.Button
    Friend WithEvents btnCompatInstallPatcher As System.Windows.Forms.Button
    Friend WithEvents btnCompatRemovePatcher As System.Windows.Forms.Button
    Friend WithEvents btnCompatCopyInfo As System.Windows.Forms.Button
    Friend WithEvents txtLog As OptiScalerInstaller.ThemedRichTextBox
    Friend WithEvents logHeaderPanel As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblLogFilter As System.Windows.Forms.Label
    Friend WithEvents txtLogFilter As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblLogSeverity As System.Windows.Forms.Label
    Friend WithEvents cmbLogSeverity As System.Windows.Forms.ComboBox
    Friend WithEvents btnLogCopy As System.Windows.Forms.Button
    Friend WithEvents btnLogSave As System.Windows.Forms.Button
    Friend WithEvents btnLogClear As System.Windows.Forms.Button
    Friend WithEvents statusStrip As System.Windows.Forms.StatusStrip
    Friend WithEvents toolStatusLabel As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents toolDetectedLabel As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents toolProgressBar As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents toolCancelButton As System.Windows.Forms.ToolStripButton
    Friend WithEvents toolTip As System.Windows.Forms.ToolTip
    Friend WithEvents compatContextMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuCompatUseDetected As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCompatOpenFolder As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCompatEditIni As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCompatSep1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuCompatInstallUpdate As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCompatUninstall As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCompatInstallPatcher As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCompatRemovePatcher As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCompatSep2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuCompatOpenWiki As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuCompatCopyInfo As System.Windows.Forms.ToolStripMenuItem
End Class

