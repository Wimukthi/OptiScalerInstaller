
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
        colCompatPlatform = New ColumnHeader()
        colCompatPath = New ColumnHeader()
        compatHeaderPanel = New Panel()
        compatHeaderLeftPanel = New TableLayoutPanel()
        compatHeaderRightPanel = New FlowLayoutPanel()
        btnOpenWiki = New Button()
        btnRefreshCompatibility = New Button()
        btnUseDetected = New Button()
        btnScanDetected = New Button()
        lblSearch = New Label()
        txtGameSearch = New ThemedTextBox()
        compatFooterPanel = New Panel()
        lblCompatibilityNote = New Label()
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
        lblBehaviorHint = New Label()
        grpActions = New ThemedGroupBox()
        btnInstall = New Button()
        btnUninstall = New Button()
        btnOpenGameFolder = New Button()
        lblInstalledStatus = New Label()
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
        lblDefaultIniPath = New Label()
        txtDefaultIniPath = New ThemedTextBox()
        btnBrowseDefaultIni = New Button()
        lblDefaultIniMode = New Label()
        cmbDefaultIniMode = New ComboBox()
        btnSaveSettings = New Button()
        btnReloadSettings = New Button()
        btnLoadDefaults = New Button()
        btnOpenSettingsFile = New Button()
        btnCheckForUpdates = New Button()
        lblUpdateNotice = New Label()
        lblSettingsPath = New Label()
        DarkThemeCheckBox = New CheckBox()
        grpLog = New ThemedGroupBox()
        txtLog = New ThemedTextBox()
        statusStrip = New StatusStrip()
        toolStatusLabel = New ToolStripStatusLabel()
        toolDetectedLabel = New ToolStripStatusLabel()
        toolProgressBar = New ToolStripProgressBar()
        toolTip = New ToolTip(components)
        mainLayout.SuspendLayout()
        tabMain.SuspendLayout()
        tabCompatibility.SuspendLayout()
        compatHeaderPanel.SuspendLayout()
        compatHeaderLeftPanel.SuspendLayout()
        compatHeaderRightPanel.SuspendLayout()
        compatFooterPanel.SuspendLayout()
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
        tabSettings.SuspendLayout()
        grpSettings.SuspendLayout()
        grpLog.SuspendLayout()
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
        lvCompatibility.Columns.AddRange(New ColumnHeader() {colCompatName, colCompatDetected, colCompatInstalled, colCompatPlatform, colCompatPath})
        lvCompatibility.Dock = DockStyle.Fill
        lvCompatibility.FullRowSelect = True
        lvCompatibility.Location = New Point(3, 43)
        lvCompatibility.MultiSelect = False
        lvCompatibility.Name = "lvCompatibility"
        lvCompatibility.OwnerDraw = True
        lvCompatibility.Size = New Size(1240, 609)
        lvCompatibility.TabIndex = 6
        lvCompatibility.UseCompatibleStateImageBehavior = False
        lvCompatibility.View = View.Details
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
        ' colCompatPlatform
        ' 
        colCompatPlatform.Text = "Platform"
        colCompatPlatform.Width = 110
        ' 
        ' colCompatPath
        ' 
        colCompatPath.Text = "Install Path"
        colCompatPath.Width = 420
        ' 
        ' compatHeaderPanel
        ' 
        compatHeaderPanel.Controls.Add(compatHeaderLeftPanel)
        compatHeaderPanel.Controls.Add(compatHeaderRightPanel)
        compatHeaderPanel.Dock = DockStyle.Top
        compatHeaderPanel.Location = New Point(3, 3)
        compatHeaderPanel.Name = "compatHeaderPanel"
        compatHeaderPanel.Padding = New Padding(6, 6, 6, 6)
        compatHeaderPanel.Size = New Size(1240, 44)
        compatHeaderPanel.TabIndex = 0
        ' 
        ' compatHeaderLeftPanel
        ' 
        compatHeaderLeftPanel.ColumnCount = 2
        compatHeaderLeftPanel.ColumnStyles.Add(New ColumnStyle())
        compatHeaderLeftPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        compatHeaderLeftPanel.Controls.Add(lblSearch, 0, 0)
        compatHeaderLeftPanel.Controls.Add(txtGameSearch, 1, 0)
        compatHeaderLeftPanel.Dock = DockStyle.Fill
        compatHeaderLeftPanel.Location = New Point(6, 6)
        compatHeaderLeftPanel.Margin = New Padding(0)
        compatHeaderLeftPanel.Name = "compatHeaderLeftPanel"
        compatHeaderLeftPanel.RowCount = 1
        compatHeaderLeftPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        compatHeaderLeftPanel.Size = New Size(835, 32)
        compatHeaderLeftPanel.TabIndex = 0
        ' 
        ' compatHeaderRightPanel
        ' 
        compatHeaderRightPanel.AutoSize = True
        compatHeaderRightPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink
        compatHeaderRightPanel.Controls.Add(btnScanDetected)
        compatHeaderRightPanel.Controls.Add(btnUseDetected)
        compatHeaderRightPanel.Controls.Add(btnRefreshCompatibility)
        compatHeaderRightPanel.Controls.Add(btnOpenWiki)
        compatHeaderRightPanel.Dock = DockStyle.Right
        compatHeaderRightPanel.FlowDirection = FlowDirection.LeftToRight
        compatHeaderRightPanel.Location = New Point(841, 6)
        compatHeaderRightPanel.Margin = New Padding(0)
        compatHeaderRightPanel.Name = "compatHeaderRightPanel"
        compatHeaderRightPanel.Padding = New Padding(0)
        compatHeaderRightPanel.Size = New Size(393, 32)
        compatHeaderRightPanel.TabIndex = 1
        compatHeaderRightPanel.WrapContents = False
        ' 
        ' btnOpenWiki
        ' 
        btnOpenWiki.Anchor = AnchorStyles.None
        btnOpenWiki.Location = New Point(419, 2)
        btnOpenWiki.Margin = New Padding(6, 0, 0, 0)
        btnOpenWiki.Name = "btnOpenWiki"
        btnOpenWiki.Size = New Size(140, 27)
        btnOpenWiki.TabIndex = 5
        btnOpenWiki.Text = "Open wiki page"
        btnOpenWiki.UseVisualStyleBackColor = True
        ' 
        ' btnRefreshCompatibility
        ' 
        btnRefreshCompatibility.Anchor = AnchorStyles.None
        btnRefreshCompatibility.Location = New Point(283, 2)
        btnRefreshCompatibility.Margin = New Padding(6, 0, 0, 0)
        btnRefreshCompatibility.Name = "btnRefreshCompatibility"
        btnRefreshCompatibility.Size = New Size(130, 27)
        btnRefreshCompatibility.TabIndex = 4
        btnRefreshCompatibility.Text = "Refresh lists"
        btnRefreshCompatibility.UseVisualStyleBackColor = True
        ' 
        ' btnUseDetected
        ' 
        btnUseDetected.Anchor = AnchorStyles.None
        btnUseDetected.Location = New Point(147, 2)
        btnUseDetected.Margin = New Padding(6, 0, 0, 0)
        btnUseDetected.Name = "btnUseDetected"
        btnUseDetected.Size = New Size(130, 27)
        btnUseDetected.TabIndex = 3
        btnUseDetected.Text = "Use detected"
        btnUseDetected.UseVisualStyleBackColor = True
        ' 
        ' btnScanDetected
        ' 
        btnScanDetected.Anchor = AnchorStyles.None
        btnScanDetected.Location = New Point(0, 2)
        btnScanDetected.Margin = New Padding(0)
        btnScanDetected.Name = "btnScanDetected"
        btnScanDetected.Size = New Size(150, 27)
        btnScanDetected.TabIndex = 2
        btnScanDetected.Text = "Scan installed games"
        btnScanDetected.UseVisualStyleBackColor = True
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
        txtGameSearch.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtGameSearch.BackColor = SystemColors.Window
        txtGameSearch.Dock = DockStyle.Fill
        txtGameSearch.ForeColor = SystemColors.WindowText
        txtGameSearch.Location = New Point(81, 4)
        txtGameSearch.Margin = New Padding(0, 4, 0, 4)
        txtGameSearch.MinimumSize = New Size(0, 24)
        txtGameSearch.Name = "txtGameSearch"
        txtGameSearch.Padding = New Padding(6, 3, 6, 3)
        txtGameSearch.Size = New Size(533, 24)
        txtGameSearch.TabIndex = 1
        ' 
        ' compatFooterPanel
        ' 
        compatFooterPanel.Controls.Add(lblCompatibilityNote)
        compatFooterPanel.Dock = DockStyle.Bottom
        compatFooterPanel.Location = New Point(3, 652)
        compatFooterPanel.Name = "compatFooterPanel"
        compatFooterPanel.Size = New Size(1240, 24)
        compatFooterPanel.TabIndex = 2
        ' 
        ' lblCompatibilityNote
        ' 
        lblCompatibilityNote.AutoSize = True
        lblCompatibilityNote.Location = New Point(12, 4)
        lblCompatibilityNote.Name = "lblCompatibilityNote"
        lblCompatibilityNote.Size = New Size(458, 15)
        lblCompatibilityNote.TabIndex = 8
        lblCompatibilityNote.Text = "List shows tested games only. Detected column is best-effort and may be incomplete."
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
        rbNightly.Size = New Size(64, 19)
        rbNightly.TabIndex = 1
        rbNightly.TabStop = True
        rbNightly.Text = "Nightly"
        rbNightly.UseVisualStyleBackColor = True
        ' 
        ' lblNightlyInfo
        ' 
        lblNightlyInfo.AutoSize = True
        lblNightlyInfo.Location = New Point(100, 50)
        lblNightlyInfo.Name = "lblNightlyInfo"
        lblNightlyInfo.Size = New Size(109, 15)
        lblNightlyInfo.TabIndex = 4
        lblNightlyInfo.Text = "Nightly: not loaded"
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
        ' lblBehaviorHint
        ' 
        lblBehaviorHint.AutoSize = True
        lblBehaviorHint.Location = New Point(12, 64)
        lblBehaviorHint.Name = "lblBehaviorHint"
        lblBehaviorHint.Size = New Size(267, 15)
        lblBehaviorHint.TabIndex = 2
        lblBehaviorHint.Text = "Backup adds .bak_ timestamp before overwriting."
        ' 
        ' grpActions
        ' 
        installLayout.SetColumnSpan(grpActions, 2)
        grpActions.Controls.Add(btnInstall)
        grpActions.Controls.Add(btnUninstall)
        grpActions.Controls.Add(btnOpenGameFolder)
        grpActions.Controls.Add(lblInstalledStatus)
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
        ' lblInstalledStatus
        ' 
        lblInstalledStatus.AutoSize = True
        lblInstalledStatus.Location = New Point(12, 74)
        lblInstalledStatus.Name = "lblInstalledStatus"
        lblInstalledStatus.Size = New Size(84, 15)
        lblInstalledStatus.TabIndex = 3
        lblInstalledStatus.Text = "Installed: none"
        ' 
        ' lblOnlineWarning
        ' 
        lblOnlineWarning.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblOnlineWarning.AutoSize = True
        lblOnlineWarning.ForeColor = Color.DarkRed
        lblOnlineWarning.Location = New Point(12, 94)
        lblOnlineWarning.Name = "lblOnlineWarning"
        lblOnlineWarning.Size = New Size(438, 15)
        lblOnlineWarning.TabIndex = 4
        lblOnlineWarning.Text = "Warning: Do not use OptiScaler with online games (anti-cheat risk, possible bans)."
        ' 
        ' lblActionNote
        ' 
        lblActionNote.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblActionNote.AutoSize = True
        lblActionNote.Location = New Point(12, 120)
        lblActionNote.Name = "lblActionNote"
        lblActionNote.Size = New Size(503, 15)
        lblActionNote.TabIndex = 5
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
        grpAsi.Dock = DockStyle.Fill
        grpAsi.Location = New Point(628, 456)
        grpAsi.Margin = New Padding(8)
        grpAsi.Name = "grpAsi"
        grpAsi.Size = New Size(604, 209)
        grpAsi.TabIndex = 5
        grpAsi.TabStop = False
        grpAsi.Text = "ASI Plugins"
        ' 
        ' chkLoadAsiPlugins
        ' 
        chkLoadAsiPlugins.AutoSize = True
        chkLoadAsiPlugins.Location = New Point(12, 24)
        chkLoadAsiPlugins.Name = "chkLoadAsiPlugins"
        chkLoadAsiPlugins.Size = New Size(161, 19)
        chkLoadAsiPlugins.TabIndex = 0
        chkLoadAsiPlugins.Text = "Enable ASI plugin loading"
        chkLoadAsiPlugins.UseVisualStyleBackColor = True
        ' 
        ' lblPluginsPath
        ' 
        lblPluginsPath.AutoSize = True
        lblPluginsPath.Location = New Point(12, 59)
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
        txtPluginsPath.Location = New Point(120, 56)
        txtPluginsPath.MinimumSize = New Size(0, 24)
        txtPluginsPath.Name = "txtPluginsPath"
        txtPluginsPath.Padding = New Padding(6, 3, 6, 3)
        txtPluginsPath.Size = New Size(389, 24)
        txtPluginsPath.TabIndex = 2
        ' 
        ' btnBrowsePluginsPath
        ' 
        btnBrowsePluginsPath.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowsePluginsPath.Location = New Point(514, 56)
        btnBrowsePluginsPath.Name = "btnBrowsePluginsPath"
        btnBrowsePluginsPath.Size = New Size(70, 23)
        btnBrowsePluginsPath.TabIndex = 3
        btnBrowsePluginsPath.Text = "Browse"
        btnBrowsePluginsPath.UseVisualStyleBackColor = True
        ' 
        ' lblAsiHint
        ' 
        lblAsiHint.AutoSize = True
        lblAsiHint.Location = New Point(12, 112)
        lblAsiHint.Name = "lblAsiHint"
        lblAsiHint.Size = New Size(262, 15)
        lblAsiHint.TabIndex = 4
        lblAsiHint.Text = "OptiScaler loads *.asi files from the plugins path."
        ' 
        ' tabSettings
        ' 
        tabSettings.Controls.Add(grpSettings)
        tabSettings.Location = New Point(4, 24)
        tabSettings.Name = "tabSettings"
        tabSettings.Padding = New Padding(8)
        tabSettings.Size = New Size(1246, 679)
        tabSettings.TabIndex = 3
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
        grpSettings.Controls.Add(lblDefaultIniPath)
        grpSettings.Controls.Add(txtDefaultIniPath)
        grpSettings.Controls.Add(btnBrowseDefaultIni)
        grpSettings.Controls.Add(lblDefaultIniMode)
        grpSettings.Controls.Add(cmbDefaultIniMode)
        grpSettings.Controls.Add(btnSaveSettings)
        grpSettings.Controls.Add(btnReloadSettings)
        grpSettings.Controls.Add(btnLoadDefaults)
        grpSettings.Controls.Add(btnOpenSettingsFile)
        grpSettings.Controls.Add(btnCheckForUpdates)
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
        grpSettings.Text = "Update & Links"
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
        lblNightlyReleaseUrl.Size = New Size(109, 15)
        lblNightlyReleaseUrl.TabIndex = 10
        lblNightlyReleaseUrl.Text = "Nightly release URL"
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
        ' lblDefaultIniPath
        ' 
        lblDefaultIniPath.AutoSize = True
        lblDefaultIniPath.Location = New Point(12, 192)
        lblDefaultIniPath.Name = "lblDefaultIniPath"
        lblDefaultIniPath.Size = New Size(113, 15)
        lblDefaultIniPath.TabIndex = 14
        lblDefaultIniPath.Text = "Default INI template"
        ' 
        ' txtDefaultIniPath
        ' 
        txtDefaultIniPath.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtDefaultIniPath.BackColor = SystemColors.Window
        txtDefaultIniPath.ForeColor = SystemColors.WindowText
        txtDefaultIniPath.Location = New Point(180, 188)
        txtDefaultIniPath.MinimumSize = New Size(0, 24)
        txtDefaultIniPath.Name = "txtDefaultIniPath"
        txtDefaultIniPath.Padding = New Padding(6, 3, 6, 3)
        txtDefaultIniPath.Size = New Size(910, 24)
        txtDefaultIniPath.TabIndex = 15
        ' 
        ' btnBrowseDefaultIni
        ' 
        btnBrowseDefaultIni.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnBrowseDefaultIni.Location = New Point(1098, 188)
        btnBrowseDefaultIni.Name = "btnBrowseDefaultIni"
        btnBrowseDefaultIni.Size = New Size(100, 24)
        btnBrowseDefaultIni.TabIndex = 16
        btnBrowseDefaultIni.Text = "Browse"
        btnBrowseDefaultIni.UseVisualStyleBackColor = True
        ' 
        ' lblDefaultIniMode
        ' 
        lblDefaultIniMode.AutoSize = True
        lblDefaultIniMode.Location = New Point(12, 224)
        lblDefaultIniMode.Name = "lblDefaultIniMode"
        lblDefaultIniMode.Size = New Size(112, 15)
        lblDefaultIniMode.TabIndex = 17
        lblDefaultIniMode.Text = "Default INI behavior"
        ' 
        ' cmbDefaultIniMode
        ' 
        cmbDefaultIniMode.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDefaultIniMode.FormattingEnabled = True
        cmbDefaultIniMode.Items.AddRange(New Object() {"Off", "Merge", "Replace"})
        cmbDefaultIniMode.Location = New Point(180, 220)
        cmbDefaultIniMode.Name = "cmbDefaultIniMode"
        cmbDefaultIniMode.Size = New Size(220, 23)
        cmbDefaultIniMode.TabIndex = 18
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
        ' lblUpdateNotice
        ' 
        lblUpdateNotice.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        lblUpdateNotice.AutoSize = True
        lblUpdateNotice.Location = New Point(732, 604)
        lblUpdateNotice.Name = "lblUpdateNotice"
        lblUpdateNotice.Size = New Size(94, 15)
        lblUpdateNotice.TabIndex = 24
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
        DarkThemeCheckBox.AutoSize = True
        DarkThemeCheckBox.Location = New Point(12, 256)
        DarkThemeCheckBox.Name = "DarkThemeCheckBox"
        DarkThemeCheckBox.Size = New Size(87, 19)
        DarkThemeCheckBox.TabIndex = 5
        DarkThemeCheckBox.Text = "Dark theme"
        DarkThemeCheckBox.UseVisualStyleBackColor = True
        ' 
        ' grpLog
        ' 
        grpLog.Controls.Add(txtLog)
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
        ' txtLog
        ' 
        txtLog.BackColor = SystemColors.Window
        txtLog.Dock = DockStyle.Fill
        txtLog.ForeColor = SystemColors.WindowText
        txtLog.Location = New Point(8, 40)
        txtLog.MinimumSize = New Size(0, 24)
        txtLog.Multiline = True
        txtLog.Name = "txtLog"
        txtLog.Padding = New Padding(6, 3, 6, 3)
        txtLog.ReadOnly = True
        txtLog.ScrollBars = ScrollBars.Vertical
        txtLog.Size = New Size(1238, 104)
        txtLog.TabIndex = 0
        ' 
        ' statusStrip
        ' 
        statusStrip.Items.AddRange(New ToolStripItem() {toolStatusLabel, toolDetectedLabel, toolProgressBar})
        statusStrip.Location = New Point(0, 875)
        statusStrip.Name = "statusStrip"
        statusStrip.Size = New Size(1270, 22)
        statusStrip.TabIndex = 1
        statusStrip.Text = "statusStrip"
        ' 
        ' toolStatusLabel
        ' 
        toolStatusLabel.Name = "toolStatusLabel"
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
        compatHeaderLeftPanel.ResumeLayout(False)
        compatHeaderLeftPanel.PerformLayout()
        compatHeaderRightPanel.ResumeLayout(False)
        compatHeaderPanel.ResumeLayout(False)
        compatHeaderPanel.PerformLayout()
        compatFooterPanel.ResumeLayout(False)
        compatFooterPanel.PerformLayout()
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
        tabSettings.ResumeLayout(False)
        grpSettings.ResumeLayout(False)
        grpSettings.PerformLayout()
        grpLog.ResumeLayout(False)
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
    Friend WithEvents tabSettings As System.Windows.Forms.TabPage
    Friend WithEvents installLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents addonsLayout As System.Windows.Forms.TableLayoutPanel
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
    Friend WithEvents lblDefaultIniPath As System.Windows.Forms.Label
    Friend WithEvents txtDefaultIniPath As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents btnBrowseDefaultIni As System.Windows.Forms.Button
    Friend WithEvents lblDefaultIniMode As System.Windows.Forms.Label
    Friend WithEvents cmbDefaultIniMode As System.Windows.Forms.ComboBox
    Friend WithEvents btnSaveSettings As System.Windows.Forms.Button
    Friend WithEvents btnReloadSettings As System.Windows.Forms.Button
    Friend WithEvents btnLoadDefaults As System.Windows.Forms.Button
    Friend WithEvents btnOpenSettingsFile As System.Windows.Forms.Button
    Friend WithEvents btnCheckForUpdates As System.Windows.Forms.Button
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
    Friend WithEvents cmbConflictMode As System.Windows.Forms.ComboBox
    Friend WithEvents lblConflictMode As System.Windows.Forms.Label
    Friend WithEvents grpActions As OptiScalerInstaller.ThemedGroupBox
    Friend WithEvents lblActionNote As System.Windows.Forms.Label
    Friend WithEvents lblOnlineWarning As System.Windows.Forms.Label
    Friend WithEvents DarkThemeCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents btnOpenGameFolder As System.Windows.Forms.Button
    Friend WithEvents btnUninstall As System.Windows.Forms.Button
    Friend WithEvents btnInstall As System.Windows.Forms.Button
    Friend WithEvents lblInstalledStatus As System.Windows.Forms.Label
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
    Friend WithEvents compatFooterPanel As System.Windows.Forms.Panel
    Friend WithEvents compatHeaderPanel As System.Windows.Forms.Panel
    Friend WithEvents compatHeaderLeftPanel As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents compatHeaderRightPanel As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblCompatibilityNote As System.Windows.Forms.Label
    Friend WithEvents btnOpenWiki As System.Windows.Forms.Button
    Friend WithEvents btnRefreshCompatibility As System.Windows.Forms.Button
    Friend WithEvents lvCompatibility As OptiScalerInstaller.ThemedListView
    Friend WithEvents colCompatName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatDetected As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatPlatform As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatPath As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCompatInstalled As System.Windows.Forms.ColumnHeader
    Friend WithEvents txtGameSearch As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents lblSearch As System.Windows.Forms.Label
    Friend WithEvents btnScanDetected As System.Windows.Forms.Button
    Friend WithEvents btnUseDetected As System.Windows.Forms.Button
    Friend WithEvents txtLog As OptiScalerInstaller.ThemedTextBox
    Friend WithEvents statusStrip As System.Windows.Forms.StatusStrip
    Friend WithEvents toolStatusLabel As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents toolDetectedLabel As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents toolProgressBar As System.Windows.Forms.ToolStripProgressBar
    Friend WithEvents toolTip As System.Windows.Forms.ToolTip
End Class

