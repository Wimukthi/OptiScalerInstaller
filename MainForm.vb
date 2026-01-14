Imports System.IO
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices
Imports System.Management

Public Class MainForm
    Private allCompatibilityEntries As List(Of CompatibilityEntry) = New List(Of CompatibilityEntry)()
    Private stableRelease As ReleaseInfo
    Private nightlyRelease As ReleaseInfo
    Private _settingThemeState As Boolean
    Private detectedGames As List(Of DetectedGame) = New List(Of DetectedGame)()
    Private detectedLookup As Dictionary(Of String, DetectedGame) = New Dictionary(Of String, DetectedGame)(StringComparer.OrdinalIgnoreCase)
    Private detectedInstallLookup As Dictionary(Of String, OptiScalerInstallInfo) = New Dictionary(Of String, OptiScalerInstallInfo)(StringComparer.OrdinalIgnoreCase)
    Private lastNormalBounds As Rectangle?
    Private windowSettingsApplied As Boolean
    Private windowSaveTimer As Timer
    Private windowSavePending As Boolean
    Private lastInstallStatusKey As String

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Private Structure DISPLAY_DEVICE
        Public cb As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=32)>
        Public DeviceName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
        Public DeviceString As String
        Public StateFlags As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
        Public DeviceID As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
        Public DeviceKey As String
    End Structure

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function EnumDisplayDevices(lpDevice As String, iDevNum As Integer, ByRef lpDisplayDevice As DISPLAY_DEVICE, dwFlags As Integer) As Boolean
    End Function

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        UpdateWindowTitle()
        InitializeDefaults()
        UpdateInstallStatus()
        LoadCompatibility()
        _settingThemeState = True
        Dim preferredMode As SystemColorMode = ThemeSettings.GetPreferredColorMode()
        DarkThemeCheckBox.Checked = preferredMode = SystemColorMode.Dark
        _settingThemeState = False
        ThemeManager.ApplyTheme(Me, preferredMode)
        InitializeToolTips()
        LoadSettingsUi()
        ApplyWindowSettings()
        InitializeWindowSaveTimer()
        AppendLog("OptiScaler Installer started.")
        BeginInvoke(New Action(AddressOf StartAutoDetection))
    End Sub

    Private Sub UpdateWindowTitle()
        Dim version As Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        If version Is Nothing Then
            Text = "OptiScaler Installer"
            Return
        End If

        Dim build As Integer = If(version.Build >= 0, version.Build, 0)
        Dim revision As Integer = If(version.Revision >= 0, version.Revision, 0)
        Dim versionText As String = $"{version.Major}.{version.Minor}.{build}"
        If revision > 0 Then
            versionText &= "." & revision
        End If

        Text = "OptiScaler Installer v" & versionText
    End Sub

    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        SaveWindowSettings()
        If windowSaveTimer IsNot Nothing Then
            windowSaveTimer.Stop()
        End If
    End Sub

    Private Sub MainForm_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        If Not windowSettingsApplied Then
            ApplyWindowSettings()
        End If
        CaptureNormalBounds()
    End Sub

    Private Sub MainForm_LocationChanged(sender As Object, e As EventArgs) Handles MyBase.LocationChanged
        CaptureNormalBounds()
    End Sub

    Private Sub MainForm_SizeChanged(sender As Object, e As EventArgs) Handles MyBase.SizeChanged
        CaptureNormalBounds()
    End Sub

    Private Sub InitializeDefaults()
        rbStable.Checked = True
        rbGpuNvidia.Checked = True
        chkDlssInputs.Checked = True
        chkEnableReshade.Checked = False
        chkEnableSpecialK.Checked = False
        chkLoadAsiPlugins.Checked = False
        chkCreateSpecialKMarker.Checked = True
        cmbHookName.SelectedIndex = 0
        cmbConflictMode.SelectedIndex = 0
        cmbFgType.SelectedIndex = 0
        cmbDefaultIniMode.SelectedIndex = 0
        ToggleLocalArchive()
        ApplyDetectedGpuVendor()
        UpdateGpuControls()
        chkEnableReshade_CheckedChanged(Me, EventArgs.Empty)
        chkEnableSpecialK_CheckedChanged(Me, EventArgs.Empty)
        chkLoadAsiPlugins_CheckedChanged(Me, EventArgs.Empty)
        btnUseDetected.Enabled = False
    End Sub

    Private Sub ApplyDetectedGpuVendor()
        If rbGpuNvidia Is Nothing OrElse rbGpuAmdIntel Is Nothing Then
            Return
        End If

        Dim adapterNames As List(Of String) = GetGpuAdapterNames()
        If adapterNames.Count > 0 Then
            AppendLog("GPU detection candidates: " & String.Join("; ", adapterNames))
        End If

        Dim vendor As GpuVendor = DetectGpuVendor(adapterNames)
        Select Case vendor
            Case GpuVendor.Nvidia
                rbGpuNvidia.Checked = True
                AppendLog("Detected GPU vendor: NVIDIA.")
            Case GpuVendor.AmdIntel
                rbGpuAmdIntel.Checked = True
                AppendLog("Detected GPU vendor: AMD/Intel.")
            Case Else
                rbGpuNvidia.Checked = True
                AppendLog("GPU vendor detection failed; defaulting to NVIDIA.")
        End Select
    End Sub

    Private Function DetectGpuVendor(adapterNames As IEnumerable(Of String)) As GpuVendor
        Try
            Dim hasNvidia As Boolean = False
            Dim hasAmd As Boolean = False
            Dim hasIntel As Boolean = False
            For Each name As String In adapterNames
                If String.IsNullOrWhiteSpace(name) Then
                    Continue For
                End If

                Dim upper As String = name.ToUpperInvariant()
                If upper.Contains("NVIDIA") Then
                    hasNvidia = True
                End If
                If upper.Contains("AMD") OrElse upper.Contains("RADEON") OrElse upper.Contains("ATI") OrElse upper.Contains("ADVANCED MICRO DEVICES") Then
                    hasAmd = True
                End If
                If upper.Contains("INTEL") OrElse upper.Contains("ARC") Then
                    hasIntel = True
                End If
            Next

            If hasNvidia Then
                Return GpuVendor.Nvidia
            End If
            If hasAmd OrElse hasIntel Then
                Return GpuVendor.AmdIntel
            End If
        Catch
        End Try

        Return GpuVendor.Unknown
    End Function

    Private Function GetGpuAdapterNames() As List(Of String)
        Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Try
            Using searcher As New ManagementObjectSearcher("SELECT Name, AdapterCompatibility FROM Win32_VideoController")
                For Each item As ManagementObject In searcher.Get()
                    Dim name As String = TryCast(item("Name"), String)
                    If Not IsGenericAdapter(name) Then
                        names.Add(name.Trim())
                    End If

                    Dim compat As String = TryCast(item("AdapterCompatibility"), String)
                    If Not IsGenericAdapter(compat) Then
                        names.Add(compat.Trim())
                    End If
                Next
            End Using
        Catch
        End Try

        If names.Count = 0 Then
            Try
                Dim index As Integer = 0
                Dim device As DISPLAY_DEVICE = New DISPLAY_DEVICE()
                device.cb = Marshal.SizeOf(device)

                While EnumDisplayDevices(Nothing, index, device, 0)
                    If Not IsGenericAdapter(device.DeviceString) Then
                        names.Add(device.DeviceString.Trim())
                    End If

                    index += 1
                    device = New DISPLAY_DEVICE()
                    device.cb = Marshal.SizeOf(device)
                End While
            Catch
            End Try
        End If

        Return names.ToList()
    End Function

    Private Function IsGenericAdapter(name As String) As Boolean
        If String.IsNullOrWhiteSpace(name) Then
            Return True
        End If

        Dim upper As String = name.ToUpperInvariant()
        If upper.Contains("MICROSOFT") AndAlso (upper.Contains("BASIC") OrElse upper.Contains("RENDER") OrElse upper.Contains("REMOTE") OrElse upper.Contains("HYPER-V")) Then
            Return True
        End If
        If upper.Contains("VIRTUAL") OrElse upper.Contains("VMWARE") OrElse upper.Contains("VBOX") Then
            Return True
        End If

        Return False
    End Function

    Private Sub LoadCompatibility()
        allCompatibilityEntries = CompatibilityService.LoadCompatibilityList()
        AppendLog("Loaded compatibility list: " & allCompatibilityEntries.Count & " entries.")
        ApplyCompatibilityFilter()
    End Sub

    Private Sub ApplyCompatibilityFilter()
        Dim filter As String = txtGameSearch.Text.Trim()
        lvCompatibility.BeginUpdate()
        lvCompatibility.Items.Clear()

        For Each entry As CompatibilityEntry In allCompatibilityEntries
            If String.IsNullOrWhiteSpace(filter) OrElse entry.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 Then
                Dim normalizedKey As String = NameNormalization.NormalizeRelaxedName(entry.Name)
                Dim detected As DetectedGame = Nothing
                detectedLookup.TryGetValue(normalizedKey, detected)

                Dim isDetected As Boolean = detected IsNot Nothing

                Dim installInfo As OptiScalerInstallInfo = Nothing
                If isDetected Then
                    detectedInstallLookup.TryGetValue(normalizedKey, installInfo)
                End If

                Dim item As New ListViewItem(entry.Name)
                item.SubItems.Add(If(isDetected, "Yes", ""))
                item.SubItems.Add(GetInstallStatusText(isDetected, installInfo))
                item.SubItems.Add(If(isDetected, detected.Platform, ""))
                item.SubItems.Add(If(isDetected, detected.InstallDir, ""))
                item.Tag = New CompatibilityRow With {.Entry = entry, .Detected = detected, .InstallInfo = installInfo}
                ApplyInstallRowColors(item, installInfo, isDetected, lvCompatibility.Items.Count)
                lvCompatibility.Items.Add(item)
            End If
        Next

        lvCompatibility.EndUpdate()
        UpdateUseDetectedState()
    End Sub

    Private Sub txtGameSearch_TextChanged(sender As Object, e As EventArgs) Handles txtGameSearch.TextChanged
        ApplyCompatibilityFilter
    End Sub

    Private Sub btnBrowseGameExe_Click(sender As Object, e As EventArgs) Handles btnBrowseGameExe.Click
        AppendLog("Browsing for game executable.")
        Using dialog As New OpenFileDialog()
            dialog.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*"
            dialog.Title = "Select Game Executable"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtGameExe.Text = dialog.FileName
                AppendLog("Selected game executable: " & dialog.FileName)
            End If
        End Using
    End Sub

    Private Sub txtGameExe_TextChanged(sender As Object, e As EventArgs) Handles txtGameExe.TextChanged
        If File.Exists(txtGameExe.Text) Then
            Dim folder = Path.GetDirectoryName(txtGameExe.Text)
            txtGameFolder.Text = folder
        Else
            txtGameFolder.Text = ""
        End If
    End Sub

    Private Sub txtGameFolder_TextChanged(sender As Object, e As EventArgs) Handles txtGameFolder.TextChanged
        UpdateEngineWarningByFolder(txtGameFolder.Text)
        UpdateInstallStatus()
    End Sub

    Private Sub btnOpenGameFolder_Click(sender As Object, e As EventArgs) Handles btnOpenGameFolder.Click
        If Directory.Exists(txtGameFolder.Text) Then
            Process.Start(New ProcessStartInfo(txtGameFolder.Text) With {.UseShellExecute = True})
            AppendLog("Opened game folder: " & txtGameFolder.Text)
        Else
            AppendLog("Game folder not found: " & txtGameFolder.Text)
        End If
    End Sub

    Private Sub btnBrowseArchive_Click(sender As Object, e As EventArgs) Handles btnBrowseArchive.Click
        AppendLog("Browsing for OptiScaler archive.")
        Using dialog As New OpenFileDialog()
            dialog.Filter = "OptiScaler Archive (*.7z)|*.7z|All files (*.*)|*.*"
            dialog.Title = "Select OptiScaler Archive"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtLocalArchive.Text = dialog.FileName
                AppendLog("Selected OptiScaler archive: " & dialog.FileName)
            End If
        End Using
    End Sub

    Private Sub btnBrowseDefaultIni_Click(sender As Object, e As EventArgs) Handles btnBrowseDefaultIni.Click
        AppendLog("Browsing for default OptiScaler.ini.")
        Using dialog As New OpenFileDialog()
            dialog.Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*"
            dialog.Title = "Select Default OptiScaler.ini"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtDefaultIniPath.Text = dialog.FileName
                AppendLog("Selected default OptiScaler.ini: " & dialog.FileName)
            End If
        End Using
    End Sub

    Private Sub rbStable_CheckedChanged(sender As Object, e As EventArgs) Handles rbStable.CheckedChanged
        ToggleLocalArchive()
    End Sub

    Private Sub rbNightly_CheckedChanged(sender As Object, e As EventArgs) Handles rbNightly.CheckedChanged
        ToggleLocalArchive()
    End Sub

    Private Sub rbLocal_CheckedChanged(sender As Object, e As EventArgs) Handles rbLocal.CheckedChanged
        ToggleLocalArchive()
    End Sub

    Private Sub ToggleLocalArchive()
        If rbLocal Is Nothing OrElse txtLocalArchive Is Nothing OrElse btnBrowseArchive Is Nothing Then
            Return
        End If
        Dim useLocal As Boolean = rbLocal.Checked
        txtLocalArchive.Enabled = useLocal
        btnBrowseArchive.Enabled = useLocal
    End Sub

    Private Sub rbGpuNvidia_CheckedChanged(sender As Object, e As EventArgs) Handles rbGpuNvidia.CheckedChanged
        UpdateGpuControls()
    End Sub

    Private Sub rbGpuAmdIntel_CheckedChanged(sender As Object, e As EventArgs) Handles rbGpuAmdIntel.CheckedChanged
        UpdateGpuControls()
    End Sub

    Private Sub UpdateGpuControls()
        chkDlssInputs.Enabled = rbGpuAmdIntel.Checked
        If Not rbGpuAmdIntel.Checked Then
            chkDlssInputs.Checked = True
        End If
    End Sub

    Private Async Sub btnRefreshReleases_Click(sender As Object, e As EventArgs) Handles btnRefreshReleases.Click
        Try
            AppendLog("Refreshing release info...")
            SetStatus("Fetching release info...")
            stableRelease = Await ReleaseService.GetStableReleaseAsync()
            nightlyRelease = Await ReleaseService.GetNightlyReleaseAsync()
            UpdateReleaseLabels()
            SetStatus("Release info updated.")
            AppendLog("Release info updated.")
        Catch ex As Exception
            AppendLog("Failed to fetch releases: " & ex.Message)
            SetStatus("Release fetch failed.")
        End Try
    End Sub

    Private Sub UpdateReleaseLabels()
        lblStableInfo.Text = FormatReleaseLabel("Stable", stableRelease)
        lblNightlyInfo.Text = FormatReleaseLabel("Nightly", nightlyRelease)
    End Sub

    Private Function FormatReleaseLabel(prefix As String, release As ReleaseInfo) As String
        If release Is Nothing Then
            Return prefix & ": not loaded"
        End If
        Dim sizeText As String = If(release.Size > 0, " - " & FormatBytes(release.Size), "")
        Return prefix & ": " & release.TagName & sizeText
    End Function

    Private Shared Function FormatBytes(value As Long) As String
        Dim sizes As String() = {"B", "KB", "MB", "GB"}
        Dim len As Double = value
        Dim order As Integer = 0
        While len >= 1024 AndAlso order < sizes.Length - 1
            order += 1
            len /= 1024
        End While
        Return String.Format("{0:0.##} {1}", len, sizes(order))
    End Function

    Private Async Sub btnInstall_Click(sender As Object, e As EventArgs) Handles btnInstall.Click
        Try
            btnInstall.Enabled = False
            UpdateProgress(0)
            AppendLog("Starting install...")

            Dim config As InstallerConfig = BuildConfig()
            Dim installInfo As OptiScalerInstallInfo = OptiScalerInstallDetector.Detect(config.GameFolder)
            Dim action As InstallAction = If(installInfo IsNot Nothing AndAlso installInfo.IsInstalled,
                                             PromptInstallAction(installInfo),
                                             InstallAction.Install)

            If action = InstallAction.Cancel Then
                AppendLog("Install canceled.")
                Return
            End If

            If action = InstallAction.Uninstall Then
                Await TryUninstallAsync(config.GameFolder, True)
                Return
            End If

            If action = InstallAction.Reinstall Then
                Dim removed As Boolean = Await TryUninstallAsync(config.GameFolder, False)
                If removed Then
                    AppendLog("Reinstalling OptiScaler...")
                Else
                    AppendLog("Reinstall: manifest not found, proceeding with overwrite.")
                End If
            ElseIf action = InstallAction.Update Then
                AppendLog("Updating existing OptiScaler install...")
            End If

            Await InstallerService.InstallAsync(config, AddressOf AppendLog, AddressOf UpdateProgress)

            MessageBox.Show(Me, "OptiScaler installed successfully.", "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
            AppendLog("Install completed.")
        Catch ex As Exception
            AppendLog("Install failed: " & ex.Message)
            MessageBox.Show(Me, ex.Message, "Install Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnInstall.Enabled = True
            UpdateProgress(0)
            UpdateInstallStatus()
        End Try
    End Sub

    Private Async Sub btnUninstall_Click(sender As Object, e As EventArgs) Handles btnUninstall.Click
        Try
            btnUninstall.Enabled = False
            AppendLog("Starting uninstall...")
            Await TryUninstallAsync(txtGameFolder.Text, True)
        Catch ex As Exception
            AppendLog("Uninstall failed: " & ex.Message)
            MessageBox.Show(Me, ex.Message, "Uninstall Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnUninstall.Enabled = True
            UpdateInstallStatus()
        End Try
    End Sub

    Private Async Function TryUninstallAsync(gameFolder As String, showDialogs As Boolean) As Task(Of Boolean)
        If String.IsNullOrWhiteSpace(gameFolder) Then
            AppendLog("Uninstall skipped: no game folder selected.")
            If showDialogs Then
                MessageBox.Show(Me, "Select a game executable first.", "Uninstall", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            Return False
        End If

        Dim removed As Boolean = Await InstallerService.UninstallAsync(gameFolder, AddressOf AppendLog)
        If showDialogs Then
            If removed Then
                MessageBox.Show(Me, "OptiScaler removed from this folder.", "Uninstall Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show(Me, "No manifest found for this folder.", "Uninstall", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End If

        Return removed
    End Function

    Private Sub btnOpenWiki_Click(sender As Object, e As EventArgs) Handles btnOpenWiki.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Entry Is Nothing Then
            AppendLog("Open wiki skipped: no game selected.")
            Return
        End If

        Dim url As String = BuildWikiUrl(row.Entry.Slug)
        If String.IsNullOrWhiteSpace(url) Then
            MessageBox.Show(Me, "Wiki base URL is not set. Update it in Settings.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        AppendLog("Opening wiki page: " & url)
        Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
    End Sub

    Private Async Sub btnRefreshCompatibility_Click(sender As Object, e As EventArgs) Handles btnRefreshCompatibility.Click
        SetStatus("Updating lists...")
        AppendLog("Refreshing compatibility list...")

        Try
            allCompatibilityEntries = Await CompatibilityService.UpdateCompatibilityListAsync()
            ApplyCompatibilityFilter()
            SetStatus("List updated.")
            AppendLog("List updated.")
        Catch ex As Exception
            AppendLog("Failed to update compatibility list: " & ex.Message)
            SetStatus("List update failed.")
        End Try
    End Sub

    Private Async Sub btnScanDetected_Click(sender As Object, e As EventArgs) Handles btnScanDetected.Click
        Await RunDetectionAsync(False)
    End Sub

    Private Sub btnUseDetected_Click(sender As Object, e As EventArgs) Handles btnUseDetected.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            AppendLog("Use detected skipped: no detected game selected.")
            MessageBox.Show(Me, "Select a detected game first.", "Detected Games", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        UseDetectedGame(row.Detected)
    End Sub

    Private Sub lvCompatibility_DoubleClick(sender As Object, e As EventArgs) Handles lvCompatibility.DoubleClick
        Dim row = GetSelectedCompatibilityRow
        If row Is Nothing OrElse row.Detected Is Nothing Then
            Return
        End If

        UseDetectedGame(row.Detected)
    End Sub

    Private Sub lvCompatibility_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvCompatibility.SelectedIndexChanged
        UpdateUseDetectedState
    End Sub

    Private Sub UpdateUseDetectedState()
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        btnUseDetected.Enabled = row IsNot Nothing AndAlso row.Detected IsNot Nothing
    End Sub

    Private Function GetSelectedCompatibilityRow() As CompatibilityRow
        If lvCompatibility.SelectedItems.Count = 0 Then
            Return Nothing
        End If

        Return TryCast(lvCompatibility.SelectedItems(0).Tag, CompatibilityRow)
    End Function

    Private Function BuildDetectedLookup(results As IEnumerable(Of DetectedGame)) As Dictionary(Of String, DetectedGame)
        Dim map As New Dictionary(Of String, DetectedGame)(StringComparer.OrdinalIgnoreCase)
        For Each game As DetectedGame In results
            If game Is Nothing Then
                Continue For
            End If

            Dim key As String = NameNormalization.NormalizeRelaxedName(game.DisplayName)
            If String.IsNullOrWhiteSpace(key) Then
                Continue For
            End If

            If Not map.ContainsKey(key) Then
                map(key) = game
            End If
        Next
        Return map
    End Function

    Private Function BuildInstallStatusLookup(results As IEnumerable(Of DetectedGame)) As Dictionary(Of String, OptiScalerInstallInfo)
        Dim map As New Dictionary(Of String, OptiScalerInstallInfo)(StringComparer.OrdinalIgnoreCase)
        For Each game As DetectedGame In results
            If game Is Nothing Then
                Continue For
            End If

            Dim key As String = NameNormalization.NormalizeRelaxedName(game.DisplayName)
            If String.IsNullOrWhiteSpace(key) Then
                Continue For
            End If

            If Not map.ContainsKey(key) Then
                map(key) = OptiScalerInstallDetector.Detect(game.InstallDir)
            End If
        Next

        Return map
    End Function

    Private Sub UpdateDetectedStatus()
        If detectedLookup Is Nothing OrElse detectedLookup.Count = 0 Then
            toolDetectedLabel.Text = "Detected: none"
        Else
            toolDetectedLabel.Text = "Detected: " & detectedLookup.Count
        End If
    End Sub

    Private Function GetInstallStatusText(isDetected As Boolean, info As OptiScalerInstallInfo) As String
        If Not isDetected Then
            Return ""
        End If

        If info Is Nothing OrElse Not info.IsInstalled Then
            Return "No"
        End If

        If String.IsNullOrWhiteSpace(info.Version) Then
            Return "Unknown"
        End If

        Return "Yes (" & info.Version & ")"
    End Function

    Private Sub ApplyInstallRowColors(item As ListViewItem, info As OptiScalerInstallInfo, isDetected As Boolean, rowIndex As Integer)
        If item Is Nothing OrElse Not isDetected Then
            Return
        End If

        Dim baseColor As Color = If(rowIndex Mod 2 = 0, lvCompatibility.RowBackColor, lvCompatibility.RowAltBackColor)
        Dim mode As SystemColorMode = ThemeSettings.GetPreferredColorMode()
        Dim tintAlpha As Integer = If(mode = SystemColorMode.Dark, 60, 35)

        If info Is Nothing OrElse Not info.IsInstalled Then
            Dim missingTint As Color = Color.FromArgb(160, 70, 70)
            item.BackColor = BlendColors(baseColor, missingTint, tintAlpha)
            Return
        End If

        Dim installedTint As Color = Color.FromArgb(70, 140, 90)
        item.BackColor = BlendColors(baseColor, installedTint, tintAlpha)
    End Sub

    Private Function BlendColors(baseColor As Color, overlay As Color, alpha As Integer) As Color
        Dim clamped As Integer = Math.Max(0, Math.Min(255, alpha))
        Dim factor As Double = clamped / 255.0R
        Dim r As Integer = CInt(baseColor.R + (overlay.R - baseColor.R) * factor)
        Dim g As Integer = CInt(baseColor.G + (overlay.G - baseColor.G) * factor)
        Dim b As Integer = CInt(baseColor.B + (overlay.B - baseColor.B) * factor)
        Return Color.FromArgb(baseColor.A, r, g, b)
    End Function

    Private Sub UseDetectedGame(game As DetectedGame)
        If game Is Nothing Then
            Return
        End If

        AppendLog("Using detected game: " & game.DisplayName)
        txtGameExe.Text = ""
        txtGameFolder.Text = game.InstallDir
        UpdateEngineWarningByFolder(game.InstallDir)

        Dim exePath As String = FindPreferredExecutable(game.InstallDir, game.DisplayName)
        If String.IsNullOrWhiteSpace(exePath) Then
            Using dialog As New OpenFileDialog()
                dialog.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*"
                dialog.Title = "Select Game Executable"
                If Directory.Exists(game.InstallDir) Then
                    dialog.InitialDirectory = game.InstallDir
                End If

                If dialog.ShowDialog(Me) = DialogResult.OK Then
                    exePath = dialog.FileName
                End If
            End Using
        End If

        If Not String.IsNullOrWhiteSpace(exePath) Then
            txtGameExe.Text = exePath
        End If

        tabMain.SelectedTab = tabInstall
    End Sub

    Private Sub UpdateEngineWarningByFolder(folder As String)
        If String.IsNullOrWhiteSpace(folder) Then
            lblEngineWarning.Visible = False
            Return
        End If

        Dim enginePath As String = Path.Combine(folder, "Engine")
        lblEngineWarning.Visible = Directory.Exists(enginePath)
    End Sub

    Private Sub UpdateInstallStatus()
        If lblInstalledStatus Is Nothing Then
            Return
        End If

        If InvokeRequired Then
            BeginInvoke(New Action(AddressOf UpdateInstallStatus))
            Return
        End If

        Dim info As OptiScalerInstallInfo = OptiScalerInstallDetector.Detect(txtGameFolder.Text)
        Dim statusText As String = BuildInstallStatusText(info)
        lblInstalledStatus.Text = statusText

        btnInstall.Text = If(info IsNot Nothing AndAlso info.IsInstalled, "Update", "Install")

        Dim key As String = If(info Is Nothing, "none", $"{info.IsInstalled}|{info.Version}|{info.Source}")
        If key <> lastInstallStatusKey Then
            lastInstallStatusKey = key
            AppendLog("Install status: " & statusText)
        End If
    End Sub

    Private Function BuildInstallStatusText(info As OptiScalerInstallInfo) As String
        If info Is Nothing OrElse Not info.IsInstalled Then
            Return "Installed: none"
        End If

        Dim versionText As String = If(String.IsNullOrWhiteSpace(info.Version), "unknown", info.Version)
        Dim sourceText As String = If(String.IsNullOrWhiteSpace(info.Source), "", " (" & info.Source & ")")
        Return "Installed: " & versionText & sourceText
    End Function

    Private Enum InstallAction
        Install
        Update
        Reinstall
        Uninstall
        Cancel
    End Enum

    Private Function PromptInstallAction(info As OptiScalerInstallInfo) As InstallAction
        If info Is Nothing OrElse Not info.IsInstalled Then
            Return InstallAction.Install
        End If

        Dim versionText As String = If(String.IsNullOrWhiteSpace(info.Version), "unknown", info.Version)
        Dim sourceText As String = If(String.IsNullOrWhiteSpace(info.Source), "unknown", info.Source)
        Dim message As String = "OptiScaler appears to be installed in this folder." & Environment.NewLine &
            "Version: " & versionText & Environment.NewLine &
            "Source: " & sourceText & Environment.NewLine &
            "Choose how to proceed."

        Try
            Dim page As New TaskDialogPage()
            page.Caption = "Existing Install Detected"
            page.Heading = "OptiScaler is already installed"
            page.Text = message
            page.Icon = TaskDialogIcon.Information

            Dim updateButton As New TaskDialogButton("Update")
            Dim reinstallButton As New TaskDialogButton("Reinstall")
            Dim uninstallButton As New TaskDialogButton("Uninstall")
            Dim cancelButton As New TaskDialogButton("Cancel")

            page.Buttons.Add(updateButton)
            page.Buttons.Add(reinstallButton)
            page.Buttons.Add(uninstallButton)
            page.Buttons.Add(cancelButton)
            page.DefaultButton = updateButton
            page.AllowCancel = True

            Dim result As TaskDialogButton = TaskDialog.ShowDialog(Me, page)
            If result Is updateButton Then
                Return InstallAction.Update
            End If
            If result Is reinstallButton Then
                Return InstallAction.Reinstall
            End If
            If result Is uninstallButton Then
                Return InstallAction.Uninstall
            End If
        Catch
            Dim fallbackText As String = "OptiScaler already installed." & Environment.NewLine &
                "Yes = Update, No = Reinstall, Cancel = Stop (use Uninstall button to remove)."
            Dim result As DialogResult = MessageBox.Show(Me, fallbackText, "Existing Install Detected", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If result = DialogResult.Yes Then
                Return InstallAction.Update
            End If
            If result = DialogResult.No Then
                Return InstallAction.Reinstall
            End If
        End Try

        Return InstallAction.Cancel
    End Function

    Private Function FindPreferredExecutable(installDir As String, displayName As String) As String
        If String.IsNullOrWhiteSpace(installDir) OrElse Not Directory.Exists(installDir) Then
            Return Nothing
        End If

        Dim candidates As New List(Of String)()
        For Each folder As String In GetCandidateExeFolders(installDir)
            If Not Directory.Exists(folder) Then
                Continue For
            End If

            For Each exePath As String In Directory.GetFiles(folder, "*.exe", SearchOption.TopDirectoryOnly)
                Dim exeName As String = Path.GetFileNameWithoutExtension(exePath)
                If ShouldSkipExecutable(exeName) Then
                    Continue For
                End If
                candidates.Add(exePath)
            Next
        Next

        If candidates.Count = 0 Then
            Return Nothing
        End If

        Dim normalizedGame As String = NameNormalization.NormalizeRelaxedName(displayName)
        If Not String.IsNullOrWhiteSpace(normalizedGame) Then
            For Each exePath As String In candidates
                Dim exeName As String = Path.GetFileNameWithoutExtension(exePath)
                Dim normalizedExe As String = NameNormalization.NormalizeRelaxedName(exeName)
                If normalizedExe = normalizedGame Then
                    Return exePath
                End If
            Next
        End If

        If candidates.Count = 1 Then
            Return candidates(0)
        End If

        If Not String.IsNullOrWhiteSpace(displayName) Then
            For Each exePath As String In candidates
                Dim exeName As String = Path.GetFileNameWithoutExtension(exePath)
                If exeName.IndexOf(displayName, StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Return exePath
                End If
            Next
        End If

        Return Nothing
    End Function

    Private Function GetCandidateExeFolders(installDir As String) As List(Of String)
        Dim folders As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim candidates As String() = {
            installDir,
            Path.Combine(installDir, "Binaries"),
            Path.Combine(installDir, "Binaries", "Win64"),
            Path.Combine(installDir, "Binaries", "Win32"),
            Path.Combine(installDir, "bin"),
            Path.Combine(installDir, "Bin"),
            Path.Combine(installDir, "Win64"),
            Path.Combine(installDir, "Win32")
        }

        For Each folder As String In candidates
            If seen.Contains(folder) Then
                Continue For
            End If
            folders.Add(folder)
            seen.Add(folder)
        Next

        Return folders
    End Function

    Private Function ShouldSkipExecutable(exeName As String) As Boolean
        If String.IsNullOrWhiteSpace(exeName) Then
            Return True
        End If

        Dim lower As String = exeName.ToLowerInvariant()
        Dim skipTokens As String() = {
            "unins", "uninstall", "setup", "launcher", "crashreport", "crashreportclient",
            "redist", "vc_redist", "installer", "update", "updater", "patch", "easyanticheat",
            "eac", "battleye", "unitycrashhandler"
        }

        For Each token As String In skipTokens
            If lower.Contains(token) Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Sub btnBrowseFakenvapiFolder_Click(sender As Object, e As EventArgs) Handles btnBrowseFakenvapiFolder.Click
        AppendLog("Browsing for Fakenvapi folder.")
        Using dialog As New FolderBrowserDialog()
            dialog.Description = "Select folder containing nvapi64.dll and fakenvapi.ini"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtFakenvapiFolder.Text = dialog.SelectedPath
                AppendLog("Selected Fakenvapi folder: " & dialog.SelectedPath)
            End If
        End Using
    End Sub

    Private Sub btnBrowseNukemDll_Click(sender As Object, e As EventArgs) Handles btnBrowseNukemDll.Click
        AppendLog("Browsing for Nukem FG DLL.")
        Using dialog As New OpenFileDialog()
            dialog.Filter = "Nukem DLL (dlssg_to_fsr3_amd_is_better.dll)|*.dll|All files (*.*)|*.*"
            dialog.Title = "Select dlssg_to_fsr3_amd_is_better.dll"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtNukemDll.Text = dialog.FileName
                AppendLog("Selected Nukem FG DLL: " & dialog.FileName)
            End If
        End Using
    End Sub

    Private Sub btnBrowseNvngx_Click(sender As Object, e As EventArgs) Handles btnBrowseNvngx.Click
        AppendLog("Browsing for nvngx_dlss.dll.")
        Using dialog As New OpenFileDialog()
            dialog.Filter = "nvngx_dlss.dll|nvngx_dlss.dll|All files (*.*)|*.*"
            dialog.Title = "Select nvngx_dlss.dll"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtNvngxDll.Text = dialog.FileName
                AppendLog("Selected nvngx_dlss.dll: " & dialog.FileName)
            End If
        End Using
    End Sub

    Private Sub chkEnableReshade_CheckedChanged(sender As Object, e As EventArgs) Handles chkEnableReshade.CheckedChanged
        txtReshadeDll.Enabled = chkEnableReshade.Checked
        btnBrowseReshade.Enabled = chkEnableReshade.Checked
    End Sub

    Private Sub btnBrowseReshade_Click(sender As Object, e As EventArgs) Handles btnBrowseReshade.Click
        AppendLog("Browsing for ReShade DLL.")
        Using dialog As New OpenFileDialog()
            dialog.Filter = "Reshade DLL (*.dll)|*.dll|All files (*.*)|*.*"
            dialog.Title = "Select Reshade DLL"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtReshadeDll.Text = dialog.FileName
                AppendLog("Selected ReShade DLL: " & dialog.FileName)
            End If
        End Using
    End Sub

    Private Sub chkEnableSpecialK_CheckedChanged(sender As Object, e As EventArgs) Handles chkEnableSpecialK.CheckedChanged
        txtSpecialKDll.Enabled = chkEnableSpecialK.Checked
        btnBrowseSpecialK.Enabled = chkEnableSpecialK.Checked
        chkCreateSpecialKMarker.Enabled = chkEnableSpecialK.Checked
    End Sub

    Private Sub btnBrowseSpecialK_Click(sender As Object, e As EventArgs) Handles btnBrowseSpecialK.Click
        AppendLog("Browsing for SpecialK DLL.")
        Using dialog As New OpenFileDialog()
            dialog.Filter = "SpecialK DLL (*.dll)|*.dll|All files (*.*)|*.*"
            dialog.Title = "Select SpecialK64.dll"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtSpecialKDll.Text = dialog.FileName
                AppendLog("Selected SpecialK DLL: " & dialog.FileName)
            End If
        End Using
    End Sub

    Private Sub chkLoadAsiPlugins_CheckedChanged(sender As Object, e As EventArgs) Handles chkLoadAsiPlugins.CheckedChanged
        txtPluginsPath.Enabled = chkLoadAsiPlugins.Checked
        btnBrowsePluginsPath.Enabled = chkLoadAsiPlugins.Checked
    End Sub

    Private Sub btnBrowsePluginsPath_Click(sender As Object, e As EventArgs) Handles btnBrowsePluginsPath.Click
        AppendLog("Browsing for ASI plugins folder.")
        Using dialog As New FolderBrowserDialog()
            dialog.Description = "Select plugins folder"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtPluginsPath.Text = dialog.SelectedPath
                AppendLog("Selected plugins folder: " & dialog.SelectedPath)
            End If
        End Using
    End Sub

    Private Function BuildConfig() As InstallerConfig
        If cmbHookName.SelectedItem Is Nothing Then
            Throw New InvalidOperationException("Select a hook filename.")
        End If

        Dim source As ReleaseSource
        If rbStable.Checked Then
            source = ReleaseSource.Stable
        ElseIf rbNightly.Checked Then
            source = ReleaseSource.Nightly
        Else
            source = ReleaseSource.LocalArchive
        End If

        Dim fgSelection As FgTypeSelection = FgTypeSelection.Auto
        Select Case cmbFgType.SelectedIndex
            Case 1
                fgSelection = FgTypeSelection.None
            Case 2
                fgSelection = FgTypeSelection.OptiFg
            Case 3
                fgSelection = FgTypeSelection.Nukem
        End Select

        If fgSelection = FgTypeSelection.Nukem AndAlso String.IsNullOrWhiteSpace(txtNukemDll.Text) Then
            Throw New InvalidOperationException("Nukem FG selected but dlssg_to_fsr3_amd_is_better.dll was not provided.")
        End If

        Dim conflict As ConflictMode = ConflictMode.BackupAndOverwrite
        Select Case cmbConflictMode.SelectedIndex
            Case 1
                conflict = ConflictMode.Overwrite
            Case 2
                conflict = ConflictMode.Skip
        End Select

        Dim gpu As GpuVendor = If(rbGpuAmdIntel.Checked, GpuVendor.AmdIntel, GpuVendor.Nvidia)

        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim defaultIniMode As DefaultIniMode = ParseDefaultIniMode(settings.DefaultIniMode)

        Return New InstallerConfig With {
            .GameExePath = txtGameExe.Text,
            .GameFolder = txtGameFolder.Text,
            .HookName = cmbHookName.SelectedItem.ToString(),
            .ConflictMode = conflict,
            .Source = source,
            .StableRelease = stableRelease,
            .NightlyRelease = nightlyRelease,
            .LocalArchivePath = txtLocalArchive.Text,
            .GpuVendor = gpu,
            .EnableDlssInputs = chkDlssInputs.Checked,
            .FgType = fgSelection,
            .FakenvapiFolder = txtFakenvapiFolder.Text,
            .NukemDllPath = txtNukemDll.Text,
            .NvngxDllPath = txtNvngxDll.Text,
            .EnableReshade = chkEnableReshade.Checked,
            .ReshadeDllPath = txtReshadeDll.Text,
            .EnableSpecialK = chkEnableSpecialK.Checked,
            .SpecialKDllPath = txtSpecialKDll.Text,
            .CreateSpecialKMarker = chkCreateSpecialKMarker.Checked,
            .LoadAsiPlugins = chkLoadAsiPlugins.Checked,
            .PluginsPath = txtPluginsPath.Text,
            .DefaultIniMode = defaultIniMode,
            .DefaultIniPath = If(settings Is Nothing, "", settings.DefaultIniPath)
        }
    End Function

    Private Function ParseDefaultIniMode(value As String) As DefaultIniMode
        Dim mode As DefaultIniMode = DefaultIniMode.Off
        If Not String.IsNullOrWhiteSpace(value) AndAlso [Enum].TryParse(value, True, mode) Then
            Return mode
        End If
        Return DefaultIniMode.Off
    End Function

    Private Function GetDefaultIniModeIndex(mode As DefaultIniMode) As Integer
        Select Case mode
            Case DefaultIniMode.Merge
                Return 1
            Case DefaultIniMode.Replace
                Return 2
            Case Else
                Return 0
        End Select
    End Function

    Private Function GetDefaultIniModeFromIndex(index As Integer) As DefaultIniMode
        Select Case index
            Case 1
                Return DefaultIniMode.Merge
            Case 2
                Return DefaultIniMode.Replace
            Case Else
                Return DefaultIniMode.Off
        End Select
    End Function

    Private Sub AppendLog(message As String)
        If txtLog.InvokeRequired Then
            txtLog.BeginInvoke(New Action(Of String)(AddressOf AppendLog), message)
            Return
        End If

        txtLog.AppendText("[" & DateTime.Now.ToString("HH:mm:ss") & "] " & message & Environment.NewLine)
        txtLog.ScrollToEnd()
    End Sub

    Private Sub DarkThemeCheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles DarkThemeCheckBox.CheckedChanged
        If _settingThemeState Then
            Return
        End If

        Dim mode As SystemColorMode = If(DarkThemeCheckBox.Checked, SystemColorMode.Dark, SystemColorMode.Classic)
        ThemeSettings.SavePreferredColorMode(mode)
        AppendLog("Theme preference set to " & mode.ToString() & ".")

        Dim result As DialogResult = MessageBox.Show(Me, "Theme changes apply after restarting OptiScaler Installer. Restart now?", "Theme", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            Application.Restart()
        End If
    End Sub

    Private Sub btnSaveSettings_Click(sender As Object, e As EventArgs) Handles btnSaveSettings.Click
        Dim settings As AppSettingsModel = AppSettings.Load()
        settings.CompatibilityListUrl = txtCompatibilityListUrl.Text.Trim()
        settings.WikiBaseUrl = txtWikiBaseUrl.Text.Trim()
        settings.StableReleaseUrl = txtStableReleaseUrl.Text.Trim()
        settings.NightlyReleaseUrl = txtNightlyReleaseUrl.Text.Trim()
        settings.InstallerReleaseUrl = txtInstallerReleaseUrl.Text.Trim()
        settings.DefaultIniPath = txtDefaultIniPath.Text.Trim()
        settings.DefaultIniMode = GetDefaultIniModeFromIndex(cmbDefaultIniMode.SelectedIndex).ToString()
        AppSettings.Save(settings)
        lblSettingsPath.Text = "Settings file: " & AppSettings.GetSettingsPath()
        AppendLog("Settings saved.")
        MessageBox.Show(Me, "Settings saved.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnReloadSettings_Click(sender As Object, e As EventArgs) Handles btnReloadSettings.Click
        AppSettings.Reload()
        LoadSettingsUi()
        AppendLog("Settings reloaded.")
    End Sub

    Private Sub btnLoadDefaults_Click(sender As Object, e As EventArgs) Handles btnLoadDefaults.Click
        Dim defaults As AppSettingsModel = AppSettings.LoadDefaults()
        If defaults Is Nothing Then
            MessageBox.Show(Me, "Default settings file not found.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ApplySettingsToUi(defaults)
        AppendLog("Default settings loaded into the editor.")
    End Sub

    Private Sub btnOpenSettingsFile_Click(sender As Object, e As EventArgs) Handles btnOpenSettingsFile.Click
        Dim path As String = AppSettings.GetSettingsPath()
        If Not File.Exists(path) Then
            AppSettings.Save(AppSettings.Load())
        End If

        Process.Start(New ProcessStartInfo(path) With {.UseShellExecute = True})
        AppendLog("Opened settings file: " & path)
    End Sub

    Private Async Sub btnCheckForUpdates_Click(sender As Object, e As EventArgs) Handles btnCheckForUpdates.Click
        Await CheckForUpdatesAsync(True)
    End Sub

    Private Async Function CheckForUpdatesAsync(showUpToDateDialog As Boolean) As Task
        Try
            btnCheckForUpdates.Enabled = False
            AppendLog("Checking for installer updates...")
            SetStatus("Checking for updates...")

            Dim release As UpdateReleaseInfo = Await UpdateService.GetLatestReleaseAsync()
            If release Is Nothing Then
                AppendLog("Update check failed: no release data.")
                SetStatus("Update check failed.")
                If showUpToDateDialog Then
                    MessageBox.Show(Me, "Unable to check for updates right now.", "Updates", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                Return
            End If

            Dim currentVersion As Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
            If currentVersion Is Nothing Then
                currentVersion = New Version(0, 0)
            End If

            If Not UpdateService.IsUpdateAvailable(currentVersion, release.Version) Then
                AppendLog("Installer is up to date.")
                SetStatus("Installer up to date.")
                If showUpToDateDialog Then
                    MessageBox.Show(Me, "You're already up to date.", "Updates", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                Return
            End If

            Using dlg As New frmUpdate(release, currentVersion)
                dlg.ShowDialog(Me)
            End Using
        Catch ex As Exception
            AppendLog("Update check failed: " & ex.Message)
            SetStatus("Update check failed.")
            If showUpToDateDialog Then
                MessageBox.Show(Me, "Update check failed: " & ex.Message, "Updates", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Finally
            btnCheckForUpdates.Enabled = True
        End Try
    End Function

    Private Sub SetStatus(message As String)
        If statusStrip.InvokeRequired Then
            statusStrip.BeginInvoke(New Action(Of String)(AddressOf SetStatus), message)
            Return
        End If

        toolStatusLabel.Text = message
    End Sub

    Private Sub UpdateProgress(value As Integer)
        If statusStrip.InvokeRequired Then
            statusStrip.BeginInvoke(New Action(Of Integer)(AddressOf UpdateProgress), value)
            Return
        End If

        toolProgressBar.Value = Math.Max(toolProgressBar.Minimum, Math.Min(toolProgressBar.Maximum, value))
    End Sub

    Private Sub InitializeToolTips()
        toolTip.AutoPopDelay = 20000
        toolTip.InitialDelay = 400
        toolTip.ReshowDelay = 100
        toolTip.ShowAlways = True

        toolTip.SetToolTip(DarkThemeCheckBox, "Toggle dark theme. You will be asked to restart for the change to apply.")
        toolTip.SetToolTip(tabMain, "Main navigation tabs.")

        toolTip.SetToolTip(txtGameSearch, "Filter the list by game name. Case-insensitive.")
        toolTip.SetToolTip(btnScanDetected, "Scan installed Steam, Epic, and GOG games and mark matches in the list. No files are modified.")
        toolTip.SetToolTip(btnUseDetected, "Use the selected detected game and prefill the Install tab. You can still change settings before installing.")
        toolTip.SetToolTip(btnRefreshCompatibility, "Download the latest compatibility list from the wiki and refresh the table.")
        toolTip.SetToolTip(btnOpenWiki, "Open the selected game's wiki page in your browser.")
        toolTip.SetToolTip(lvCompatibility, "Compatibility list with detection info. Double-click a detected row to prefill the Install tab.")

        toolTip.SetToolTip(txtGameExe, "Path to the game's main executable. Avoid launchers, uninstallers, or setup tools.")
        toolTip.SetToolTip(btnBrowseGameExe, "Browse for the game's executable (.exe).")
        toolTip.SetToolTip(txtGameFolder, "Game install folder, auto-filled from the EXE. Edit only if needed.")
        toolTip.SetToolTip(rbStable, "Install the latest stable OptiScaler release.")
        toolTip.SetToolTip(rbNightly, "Install the latest nightly build (experimental).")
        toolTip.SetToolTip(rbLocal, "Install from a local OptiScaler .7z archive.")
        toolTip.SetToolTip(txtLocalArchive, "Path to the local OptiScaler archive.")
        toolTip.SetToolTip(btnBrowseArchive, "Browse for a local OptiScaler .7z archive.")
        toolTip.SetToolTip(btnRefreshReleases, "Fetch latest release info for stable and nightly.")
        toolTip.SetToolTip(cmbHookName, "DLL filename OptiScaler will use (renames OptiScaler.dll). Choose the hook the game loads.")
        toolTip.SetToolTip(rbGpuNvidia, "Target NVIDIA GPUs. Default DLSS path.")
        toolTip.SetToolTip(rbGpuAmdIntel, "Target AMD/Intel GPUs and allow DLSS input spoofing.")
        toolTip.SetToolTip(chkDlssInputs, "Enable DLSS input spoofing when using AMD/Intel. Unchecked sets Dxgi=false in OptiScaler.ini.")
        toolTip.SetToolTip(cmbFgType, "Frame generation choice. Auto keeps default behavior.")
        toolTip.SetToolTip(cmbConflictMode, "What to do when files already exist in the game folder.")
        toolTip.SetToolTip(btnInstall, "Install OptiScaler and selected add-ons into the game folder.")
        toolTip.SetToolTip(btnUninstall, "Remove OptiScaler files using the install manifest.")
        toolTip.SetToolTip(btnOpenGameFolder, "Open the current game folder in Explorer.")

        toolTip.SetToolTip(chkEnableReshade, "Enable ReShade integration and copy the chosen DLL.")
        toolTip.SetToolTip(txtReshadeDll, "Path to ReShade DLL to install.")
        toolTip.SetToolTip(btnBrowseReshade, "Browse for a ReShade DLL.")

        toolTip.SetToolTip(chkEnableSpecialK, "Enable Special K integration.")
        toolTip.SetToolTip(txtSpecialKDll, "Path to SpecialK64.dll.")
        toolTip.SetToolTip(btnBrowseSpecialK, "Browse for SpecialK64.dll.")
        toolTip.SetToolTip(chkCreateSpecialKMarker, "Create SpecialK.marker to force Special K to load.")

        toolTip.SetToolTip(chkLoadAsiPlugins, "Enable ASI loader support.")
        toolTip.SetToolTip(txtPluginsPath, "Folder containing ASI plugins.")
        toolTip.SetToolTip(btnBrowsePluginsPath, "Browse for the ASI plugins folder.")

        toolTip.SetToolTip(txtNvngxDll, "Optional nvngx_dlss.dll to copy if the game does not include one.")
        toolTip.SetToolTip(btnBrowseNvngx, "Browse for nvngx_dlss.dll.")

        toolTip.SetToolTip(txtNukemDll, "Path to dlssg_to_fsr3_amd_is_better.dll (Nukem FG).")
        toolTip.SetToolTip(btnBrowseNukemDll, "Browse for the Nukem FG DLL.")

        toolTip.SetToolTip(txtFakenvapiFolder, "Folder containing nvapi64.dll and fakenvapi.ini for AMD/Intel.")
        toolTip.SetToolTip(btnBrowseFakenvapiFolder, "Browse for the Fakenvapi folder.")

        toolTip.SetToolTip(txtLog, "Read-only log of installer actions.")
        toolTip.SetToolTip(grpLog, "Log output for all installer actions.")

        toolTip.SetToolTip(txtCompatibilityListUrl, "Raw markdown URL for the compatibility list (used by Refresh lists).")
        toolTip.SetToolTip(txtWikiBaseUrl, "Base URL for wiki pages (used when opening a selected game's page).")
        toolTip.SetToolTip(txtStableReleaseUrl, "GitHub API URL for the latest stable release.")
        toolTip.SetToolTip(txtNightlyReleaseUrl, "GitHub API URL for the nightly release.")
        toolTip.SetToolTip(txtInstallerReleaseUrl, "GitHub API URL for OptiScaler Installer updates.")
        toolTip.SetToolTip(txtDefaultIniPath, "Optional OptiScaler.ini template to apply on install.")
        toolTip.SetToolTip(btnBrowseDefaultIni, "Browse for a default OptiScaler.ini template.")
        toolTip.SetToolTip(cmbDefaultIniMode, "Choose how to apply the default OptiScaler.ini.")
        toolTip.SetToolTip(btnSaveSettings, "Save settings to disk.")
        toolTip.SetToolTip(btnReloadSettings, "Reload settings from disk and discard edits.")
        toolTip.SetToolTip(btnLoadDefaults, "Load defaults from the bundled settings file.")
        toolTip.SetToolTip(btnOpenSettingsFile, "Open the settings file in your default editor.")
        toolTip.SetToolTip(btnCheckForUpdates, "Check for OptiScaler Installer updates.")
        toolTip.SetToolTip(lblInstalledStatus, "Shows detected OptiScaler installation status for the selected game folder.")
    End Sub

    Private Sub LoadSettingsUi()
        Dim settings As AppSettingsModel = AppSettings.Load()
        ApplySettingsToUi(settings)
        lblSettingsPath.Text = "Settings file: " & AppSettings.GetSettingsPath()
        AppendLog("Settings loaded.")
    End Sub

    Private Sub ApplyWindowSettings()
        If windowSettingsApplied Then
            Return
        End If

        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim widthValue As Integer? = settings.WindowWidth
        Dim heightValue As Integer? = settings.WindowHeight
        Dim xValue As Integer? = settings.WindowX
        Dim yValue As Integer? = settings.WindowY

        If widthValue.HasValue AndAlso widthValue.Value <= 0 Then
            widthValue = Nothing
        End If
        If heightValue.HasValue AndAlso heightValue.Value <= 0 Then
            heightValue = Nothing
        End If

        Dim hasSize As Boolean = widthValue.HasValue AndAlso heightValue.HasValue
        Dim hasLocation As Boolean = xValue.HasValue AndAlso yValue.HasValue
        If hasLocation AndAlso Not hasSize AndAlso xValue.Value = 0 AndAlso yValue.Value = 0 Then
            hasLocation = False
        End If

        Dim width As Integer = If(hasSize, widthValue.Value, Width)
        Dim height As Integer = If(hasSize, heightValue.Value, Height)
        Dim x As Integer = If(hasLocation, xValue.Value, Left)
        Dim y As Integer = If(hasLocation, yValue.Value, Top)

        Dim applied As Boolean = False

        If hasSize OrElse hasLocation Then
            StartPosition = FormStartPosition.Manual

            If hasSize AndAlso hasLocation Then
                Dim proposed As New Rectangle(x, y, width, height)
                Dim normalized As Rectangle = NormalizeBoundsToVisible(proposed)
                SetBounds(normalized.X, normalized.Y, normalized.Width, normalized.Height, BoundsSpecified.All)
                lastNormalBounds = normalized
                applied = True
            ElseIf hasSize Then
                Size = New Size(width, height)
                CenterToScreen()
                lastNormalBounds = DesktopBounds
                applied = True
            ElseIf hasLocation Then
                Dim proposed As New Rectangle(x, y, Width, Height)
                Dim normalized As Rectangle = NormalizeBoundsToVisible(proposed)
                Location = New Point(normalized.X, normalized.Y)
                lastNormalBounds = New Rectangle(Location, Size)
                applied = True
            End If
        Else
            CenterToScreen()
        End If

        If applied Then
            AppendLog("Applied saved window layout.")
        End If

        If String.Equals(settings.WindowState, "Maximized", StringComparison.OrdinalIgnoreCase) Then
            WindowState = FormWindowState.Maximized
        ElseIf String.Equals(settings.WindowState, "Normal", StringComparison.OrdinalIgnoreCase) Then
            WindowState = FormWindowState.Normal
        End If

        windowSettingsApplied = True
    End Sub

    Private Sub SaveWindowSettings()
        SaveWindowSettings(True)
    End Sub

    Private Sub SaveWindowSettings(logAction As Boolean)
        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim bounds As Rectangle
        If WindowState = FormWindowState.Normal Then
            bounds = If(lastNormalBounds.HasValue, lastNormalBounds.Value, DesktopBounds)
        Else
            bounds = If(lastNormalBounds.HasValue, lastNormalBounds.Value, RestoreBounds)
        End If

        If bounds.Width <= 0 OrElse bounds.Height <= 0 Then
            If logAction Then
                AppendLog("Window layout not saved (invalid bounds).")
            End If
            Return
        End If

        settings.WindowX = bounds.X
        settings.WindowY = bounds.Y
        settings.WindowWidth = bounds.Width
        settings.WindowHeight = bounds.Height
        settings.WindowState = WindowState.ToString()
        AppSettings.Save(settings)
        If logAction Then
            AppendLog("Window layout saved.")
        End If
    End Sub

    Private Sub CaptureNormalBounds()
        If WindowState <> FormWindowState.Normal Then
            Return
        End If

        Dim bounds As Rectangle = DesktopBounds
        If bounds.Width <= 0 OrElse bounds.Height <= 0 Then
            Return
        End If

        lastNormalBounds = bounds
        ScheduleWindowSave()
    End Sub

    Private Sub InitializeWindowSaveTimer()
        If windowSaveTimer IsNot Nothing Then
            Return
        End If

        windowSaveTimer = New Timer() With {.Interval = 500}
        AddHandler windowSaveTimer.Tick, AddressOf OnWindowSaveTimerTick
    End Sub

    Private Sub ScheduleWindowSave()
        If Not windowSettingsApplied Then
            Return
        End If

        If windowSaveTimer Is Nothing Then
            Return
        End If

        windowSavePending = True
        windowSaveTimer.Stop()
        windowSaveTimer.Start()
    End Sub

    Private Sub OnWindowSaveTimerTick(sender As Object, e As EventArgs)
        windowSaveTimer.Stop()
        If Not windowSavePending Then
            Return
        End If

        windowSavePending = False
        SaveWindowSettings(False)
    End Sub

    Private Function NormalizeBoundsToVisible(bounds As Rectangle) As Rectangle
        Dim targetScreen As Screen = Screen.FromRectangle(bounds)
        Dim working As Rectangle = targetScreen.WorkingArea

        Dim maxX As Integer = working.Right - bounds.Width
        If maxX < working.Left Then
            maxX = working.Left
        End If

        Dim maxY As Integer = working.Bottom - bounds.Height
        If maxY < working.Top Then
            maxY = working.Top
        End If

        Dim x As Integer = Math.Min(Math.Max(bounds.X, working.Left), maxX)
        Dim y As Integer = Math.Min(Math.Max(bounds.Y, working.Top), maxY)

        Return New Rectangle(x, y, bounds.Width, bounds.Height)
    End Function

    Private Async Sub StartAutoDetection()
        Await RunDetectionAsync(True)
    End Sub

    Private Async Function RunDetectionAsync(isAuto As Boolean) As Task
        Dim label As String = If(isAuto, "Auto detection", "Detection")
        Try
            btnScanDetected.Enabled = False
            btnUseDetected.Enabled = False
            toolDetectedLabel.Text = "Detected: scanning..."
            AppendLog(label & " started.")
            detectedGames.Clear()
            detectedLookup.Clear()

            If allCompatibilityEntries Is Nothing OrElse allCompatibilityEntries.Count = 0 Then
                AppendLog("Detection skipped: compatibility list is empty.")
                toolDetectedLabel.Text = "Detected: none"
                Return
            End If

            Dim results As List(Of DetectedGame) = Await Task.Run(Function() DetectionService.DetectSupportedGames(allCompatibilityEntries, AddressOf AppendLog))
            detectedGames = results
            detectedLookup = BuildDetectedLookup(results)
            detectedInstallLookup = Await Task.Run(Function() BuildInstallStatusLookup(results))
            ApplyCompatibilityFilter()
            UpdateDetectedStatus()
            Dim installedCount As Integer = 0
            For Each info As OptiScalerInstallInfo In detectedInstallLookup.Values
                If info IsNot Nothing AndAlso info.IsInstalled Then
                    installedCount += 1
                End If
            Next
            AppendLog("OptiScaler installed in " & installedCount & " detected game(s).")
            AppendLog(label & " finished: " & detectedLookup.Count & " supported game(s) detected.")
        Catch ex As Exception
            AppendLog(label & " failed: " & ex.Message)
            toolDetectedLabel.Text = "Detected: error"
        Finally
            btnScanDetected.Enabled = True
        End Try
    End Function

    Private Sub ApplySettingsToUi(settings As AppSettingsModel)
        If settings Is Nothing Then
            Return
        End If

        txtCompatibilityListUrl.Text = settings.CompatibilityListUrl
        txtWikiBaseUrl.Text = settings.WikiBaseUrl
        txtStableReleaseUrl.Text = settings.StableReleaseUrl
        txtNightlyReleaseUrl.Text = settings.NightlyReleaseUrl
        txtInstallerReleaseUrl.Text = settings.InstallerReleaseUrl
        txtDefaultIniPath.Text = settings.DefaultIniPath
        cmbDefaultIniMode.SelectedIndex = GetDefaultIniModeIndex(ParseDefaultIniMode(settings.DefaultIniMode))
    End Sub

    Private Function BuildWikiUrl(slug As String) As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim baseUrl As String = If(settings.WikiBaseUrl, "").Trim()
        If String.IsNullOrWhiteSpace(baseUrl) OrElse String.IsNullOrWhiteSpace(slug) Then
            Return ""
        End If

        If Not baseUrl.EndsWith("/", StringComparison.Ordinal) Then
            baseUrl &= "/"
        End If

        Dim trimmedSlug As String = slug.TrimStart("/"c)
        Return baseUrl & trimmedSlug
    End Function

    Private Class CompatibilityRow
        Public Property Entry As CompatibilityEntry
        Public Property Detected As DetectedGame
        Public Property InstallInfo As OptiScalerInstallInfo
    End Class
End Class
