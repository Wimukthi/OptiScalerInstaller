Imports System.IO
Imports System.Diagnostics
Imports System.Threading.Tasks
Imports System.Drawing
Imports System.IO.Compression
Imports System.Runtime.InteropServices
Imports System.Management
Imports System.Text
Imports System.Text.Json
Imports System.Net
Imports System.Net.Http
Imports System.Linq

Public Class MainForm
    ' Main UI surface for detection, install, add-ons, settings, and logging.
    Private allCompatibilityEntries As List(Of CompatibilityEntry) = New List(Of CompatibilityEntry)()
    Private stableRelease As ReleaseInfo
    Private nightlyRelease As ReleaseInfo
    Private componentRelease As ReleaseInfo
    Private optiPatcherStableRelease As ReleaseInfo
    Private optiPatcherRollingRelease As ReleaseInfo
    Private optiPatcherAlternateRelease As ReleaseInfo
    Private _settingThemeState As Boolean
    Private _settingDefaultsPreset As Boolean
    Private detectedGames As List(Of DetectedGame) = New List(Of DetectedGame)()
    Private detectedLookup As Dictionary(Of String, DetectedGame) = New Dictionary(Of String, DetectedGame)(StringComparer.OrdinalIgnoreCase)
    Private detectedInstallLookup As Dictionary(Of String, OptiScalerInstallInfo) = New Dictionary(Of String, OptiScalerInstallInfo)(StringComparer.OrdinalIgnoreCase)
    Private detectedOptiPatcherLookup As Dictionary(Of String, OptiPatcherInstallInfo) = New Dictionary(Of String, OptiPatcherInstallInfo)(StringComparer.OrdinalIgnoreCase)
    Private persistedDeepScanGames As List(Of DetectedGame) = New List(Of DetectedGame)()
    Private persistedDeepScanLoaded As Boolean
    Private compatibilityChangedNames As HashSet(Of String) = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private compatibilityBaseNoteText As String = "List shows tested games only. Detected/Anti-cheat columns are best-effort and may be incomplete."
    Private optiPatcherSupportEntries As List(Of OptiPatcherSupportEntry) = New List(Of OptiPatcherSupportEntry)()
    Private optiPatcherSupportLookup As Dictionary(Of String, OptiPatcherSupportEntry) = New Dictionary(Of String, OptiPatcherSupportEntry)(StringComparer.OrdinalIgnoreCase)
    Private lastNormalBounds As Rectangle?
    Private windowSettingsApplied As Boolean
    Private windowSaveTimer As Timer
    Private windowSavePending As Boolean
    Private lastInstallStatusKey As String
    Private lastExperimentalStatusKey As String
    Private latestUpdateRelease As UpdateReleaseInfo
    Private loadingSettingsUi As Boolean
    Private gpuDetectionInitialized As Boolean
    Private gpuDetectionVendor As GpuVendor = GpuVendor.Unknown
    Private gpuDetectionAdapters As List(Of GpuAdapterInfo) = New List(Of GpuAdapterInfo)()
    Private gpuDetectionCandidates As List(Of String) = New List(Of String)()
    Private gpuDetectionLogWritten As Boolean
    Private lastOptiPatcherStatusKey As String
    Private installOperationInProgress As Boolean
    Private uninstallOperationInProgress As Boolean

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

    Private Class GpuAdapterInfo
        Public Property Name As String
        Public Property AdapterCompatibility As String
        Public Property PnpDeviceId As String
        Public Property Source As String
    End Class

    Private Const NvidiaPciVendorId As String = "10DE"
    Private Const AmdPciVendorId As String = "1002"
    Private Const IntelPciVendorId As String = "8086"

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function EnumDisplayDevices(lpDevice As String, iDevNum As Integer, ByRef lpDisplayDevice As DISPLAY_DEVICE, dwFlags As Integer) As Boolean
    End Function

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        UpdateWindowTitle()
        InitializeDefaults()
        If lblCompatibilityNote IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(lblCompatibilityNote.Text) Then
            compatibilityBaseNoteText = lblCompatibilityNote.Text
        End If
        UpdateCompatibilityNote()
        UpdateInstallStatus()
        UpdateOptiPatcherStatus()
        UpdateExperimentalStatus()
        UpdateExperimentalDetectedGamesList()
        LoadCompatibility()
        EnsurePersistedDeepScanGamesLoaded()
        LoadOptiPatcherSupportList()
        _settingThemeState = True
        Dim preferredMode As SystemColorMode = ThemeSettings.GetPreferredColorMode()
        DarkThemeCheckBox.Checked = preferredMode = SystemColorMode.Dark
        _settingThemeState = False
        ThemeManager.ApplyTheme(Me, preferredMode)
        ApplyCompatibilityContextMenuTheme(preferredMode)
        InitializeToolTips()
        LoadSettingsUi()
        PositionSettingsControls()
        ApplyWindowSettings()
        InitializeWindowSaveTimer()
        AppendLog("OptiScaler Installer started.")
        BeginInvoke(New Action(AddressOf StartBackgroundTasks))
    End Sub

    ' Run background refreshes after the UI is ready.
    Private Async Sub StartBackgroundTasks()
        Try
            Dim settings As AppSettingsModel = AppSettings.Load()
            Dim refreshOnStartup As Boolean = settings IsNot Nothing AndAlso settings.AutoRefreshCompatibilityOnStartup.HasValue AndAlso settings.AutoRefreshCompatibilityOnStartup.Value
            Dim checkUpdatesOnStartup As Boolean = settings Is Nothing OrElse Not settings.AutoCheckInstallerUpdates.HasValue OrElse settings.AutoCheckInstallerUpdates.Value
            Dim runInitialDeepScan As Boolean = settings IsNot Nothing AndAlso
                                                (Not settings.HasCompletedInitialDeepScan.HasValue OrElse
                                                 Not settings.HasCompletedInitialDeepScan.Value)
            ' Always populate detected games first so the user sees results quickly.
            Await RunDetectionAsync(True)

            If runInitialDeepScan Then
                AppendLog("First start detected: select drives for one-time deep scan.")
                Dim initialRoots As List(Of String) = PromptForDeepScanRoots("First start deep scan")
                If initialRoots IsNot Nothing AndAlso initialRoots.Count > 0 Then
                    Await RunDetectionAsync(True, initialRoots, True)
                    AppendLog("One-time first-start deep scan completed.")
                ElseIf initialRoots Is Nothing Then
                    AppendLog("One-time deep scan skipped by user.")
                Else
                    AppendLog("One-time deep scan skipped: no scannable drives found.")
                End If

                settings.HasCompletedInitialDeepScan = True
                AppSettings.Save(settings)
            End If

            ' Non-critical startup refreshes run after detection has already rendered.
            If refreshOnStartup Then
                Await RefreshCompatibilityAsync(False, True)
            End If

            Await RefreshReleaseInfoAsync(False)
            Await RefreshOptiPatcherReleaseInfoAsync(False)
            If checkUpdatesOnStartup Then
                Await CheckForUpdatesSilentAsync(True)
            Else
                SetUpdateNoticeVisible(False, "")
                AppendLog("Installer update auto-check disabled.")
            End If
        Catch ex As Exception
            AppendLog("Startup background task failed: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.StartBackgroundTasks")
        End Try
    End Sub

    Private Sub UpdateWindowTitle()
        Dim version As Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        If version Is Nothing Then
            Text = "OptiScaler Installer"
            Return
        End If

        Dim build As Integer = If(version.Build >= 0, version.Build, 0)
        Dim revision As Integer = If(version.Revision >= 0, version.Revision, 0)
        Dim versionText As String = $"{version.Major}.{version.Minor}.{build}.{revision}"
        Dim title As String = "OptiScaler Installer v" & versionText
        Dim gpuTitleModel As String = GetPreferredGpuTitleModel()
        If Not String.IsNullOrWhiteSpace(gpuTitleModel) Then
            title &= " [" & gpuTitleModel & "]"
        End If

        Text = title
    End Sub

    Private Function GetPreferredGpuTitleModel() As String
        If Not gpuDetectionInitialized OrElse gpuDetectionAdapters Is Nothing OrElse gpuDetectionAdapters.Count = 0 Then
            Return ""
        End If

        Dim preferred As GpuAdapterInfo = Nothing
        Select Case gpuDetectionVendor
            Case GpuVendor.Nvidia
                preferred = FindAdapterByVendor(GpuVendor.Nvidia)
            Case GpuVendor.AmdIntel
                preferred = FindAdapterByVendor(GpuVendor.AmdIntel)
        End Select

        If preferred Is Nothing Then
            preferred = gpuDetectionAdapters.FirstOrDefault(Function(adapter) adapter IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(adapter.Name))
        End If
        If preferred Is Nothing Then
            Return ""
        End If

        Dim model As String = If(preferred.Name, "").Trim()
        If String.IsNullOrWhiteSpace(model) Then
            model = If(preferred.AdapterCompatibility, "").Trim()
        End If
        If String.IsNullOrWhiteSpace(model) Then
            Return ""
        End If

        Const maxLength As Integer = 42
        If model.Length > maxLength Then
            model = model.Substring(0, maxLength - 1).TrimEnd() & "..."
        End If

        Return model
    End Function

    Private Function FindAdapterByVendor(vendor As GpuVendor) As GpuAdapterInfo
        If gpuDetectionAdapters Is Nothing Then
            Return Nothing
        End If

        For Each adapter As GpuAdapterInfo In gpuDetectionAdapters
            If adapter Is Nothing OrElse String.IsNullOrWhiteSpace(adapter.Name) Then
                Continue For
            End If

            If DetectVendorFromAdapter(adapter) = vendor Then
                Return adapter
            End If
        Next

        Return Nothing
    End Function

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
        PositionSettingsControls()
    End Sub

    Private Sub grpSettings_Resize(sender As Object, e As EventArgs) Handles grpSettings.Resize
        PositionSettingsControls()
    End Sub

    Private Sub InitializeDefaults()
        ' Set initial selections before applying user settings.
        rbStable.Checked = True
        rbGpuNvidia.Checked = True
        chkHideNonDetected.Checked = False
        chkDlssInputs.Checked = True
        chkEnableReshade.Checked = False
        chkEnableSpecialK.Checked = False
        chkLoadAsiPlugins.Checked = False
        chkInstallOptiPatcher.Checked = False
        chkCreateSpecialKMarker.Checked = True
        cmbHookName.SelectedIndex = 0
        cmbConflictMode.SelectedIndex = 0
        cmbFgType.SelectedIndex = 0
        cmbDefaultIniMode.SelectedIndex = 0
        cmbDefaultPreset.SelectedIndex = 0
        cmbDefaultHookName.SelectedIndex = 0
        cmbDefaultGpuVendor.SelectedIndex = 0
        chkDefaultDlssInputs.Checked = True
        cmbDefaultFgType.SelectedIndex = 0
        cmbDefaultConflictMode.SelectedIndex = 0
        chkFsr4EnableUpdate.Checked = True
        chkFsr4EnableAgility.Checked = False
        chkAutoRefreshCompatibilityOnStartup.Checked = True
        chkAutoCheckInstallerUpdates.Checked = True
        chkShowExperimentalTabOnUnsupportedGpu.Checked = False
        cmbOptiPatcherSource.SelectedIndex = 0
        ToggleOptiPatcherLocalFile()
        ToggleLocalArchive()
        ApplyDetectedGpuVendor(False)
        UpdateExperimentalTabAvailability(False)
        UpdateGpuControls()
        chkEnableReshade_CheckedChanged(Me, EventArgs.Empty)
        chkEnableSpecialK_CheckedChanged(Me, EventArgs.Empty)
        chkLoadAsiPlugins_CheckedChanged(Me, EventArgs.Empty)
        btnUseDetected.Enabled = False
    End Sub

    Private Sub PositionSettingsControls()
        If grpSettings Is Nothing OrElse DarkThemeCheckBox Is Nothing Then
            Return
        End If

        Dim targetY As Integer = 191
        If chkAutoRefreshCompatibilityOnStartup IsNot Nothing Then
            targetY = chkAutoRefreshCompatibilityOnStartup.Top
        End If

        Dim targetX As Integer = grpSettings.ClientSize.Width - DarkThemeCheckBox.Width - 24
        If targetX < 12 Then
            targetX = 12
        End If

        DarkThemeCheckBox.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        DarkThemeCheckBox.Location = New Point(targetX, targetY)
        DarkThemeCheckBox.BringToFront()
        PositionUpdateNotice()
    End Sub

    Private Sub PositionUpdateNotice()
        If grpSettings Is Nothing OrElse btnCheckForUpdates Is Nothing OrElse lblUpdateNotice Is Nothing Then
            Return
        End If

        lblUpdateNotice.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        Dim x As Integer = btnCheckForUpdates.Left - lblUpdateNotice.Width - 8
        Dim y As Integer = btnCheckForUpdates.Top + CInt((btnCheckForUpdates.Height - lblUpdateNotice.Height) / 2)

        If x < 12 Then
            x = btnCheckForUpdates.Right + 10
        End If
        If x + lblUpdateNotice.Width > grpSettings.ClientSize.Width - 10 Then
            x = grpSettings.ClientSize.Width - lblUpdateNotice.Width - 10
        End If
        If y < 12 Then
            y = 12
        End If

        lblUpdateNotice.Location = New Point(x, y)
    End Sub

    Private Sub ApplyDetectedGpuVendor(Optional logAction As Boolean = True)
        ' Auto-select GPU vendor based on detected adapters.
        If rbGpuNvidia Is Nothing OrElse rbGpuAmdIntel Is Nothing Then
            Return
        End If

        EnsureGpuDetectionInitialized()
        Dim shouldLog As Boolean = logAction AndAlso Not gpuDetectionLogWritten
        If shouldLog AndAlso gpuDetectionCandidates.Count > 0 Then
            AppendLog("GPU detection candidates: " & String.Join("; ", gpuDetectionCandidates))
        End If

        Select Case gpuDetectionVendor
            Case GpuVendor.Nvidia
                rbGpuNvidia.Checked = True
                If shouldLog Then
                    AppendLog("Detected GPU vendor: NVIDIA.")
                End If
            Case GpuVendor.AmdIntel
                rbGpuAmdIntel.Checked = True
                If shouldLog Then
                    AppendLog("Detected GPU vendor: AMD/Intel.")
                End If
            Case Else
                rbGpuNvidia.Checked = True
                If shouldLog Then
                    AppendLog("GPU vendor detection failed; defaulting to NVIDIA.")
                End If
        End Select

        If shouldLog Then
            gpuDetectionLogWritten = True
        End If
    End Sub

    Private Sub EnsureGpuDetectionInitialized()
        If gpuDetectionInitialized Then
            Return
        End If

        gpuDetectionAdapters = GetGpuAdapters()
        gpuDetectionCandidates.Clear()

        Dim seenCandidates As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each adapter As GpuAdapterInfo In gpuDetectionAdapters
            Dim candidate As String = BuildGpuCandidate(adapter)
            If Not String.IsNullOrWhiteSpace(candidate) AndAlso seenCandidates.Add(candidate) Then
                gpuDetectionCandidates.Add(candidate)
            End If
        Next

        gpuDetectionVendor = DetectGpuVendor(gpuDetectionAdapters)
        gpuDetectionInitialized = True
        UpdateWindowTitle()
    End Sub

    Private Function DetectGpuVendor(adapters As IEnumerable(Of GpuAdapterInfo)) As GpuVendor
        Try
            Dim hasNvidia As Boolean = False
            Dim hasAmdOrIntel As Boolean = False

            For Each adapter As GpuAdapterInfo In adapters
                Select Case DetectVendorFromAdapter(adapter)
                    Case GpuVendor.Nvidia
                        hasNvidia = True
                    Case GpuVendor.AmdIntel
                        hasAmdOrIntel = True
                End Select
            Next

            If hasNvidia Then
                Return GpuVendor.Nvidia
            End If
            If hasAmdOrIntel Then
                Return GpuVendor.AmdIntel
            End If
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.DetectGpuVendor")
        End Try

        Return GpuVendor.Unknown
    End Function

    Private Function GetGpuAdapterNames() As List(Of String)
        EnsureGpuDetectionInitialized()
        Return New List(Of String)(gpuDetectionCandidates)
    End Function

    Private Function GetGpuAdapters() As List(Of GpuAdapterInfo)
        Dim adapters As New List(Of GpuAdapterInfo)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Try
            Using searcher As New ManagementObjectSearcher("SELECT Name, AdapterCompatibility, PNPDeviceID FROM Win32_VideoController")
                For Each item As ManagementObject In searcher.Get()
                    Dim adapter As New GpuAdapterInfo With {
                        .Name = If(TryCast(item("Name"), String), "").Trim(),
                        .AdapterCompatibility = If(TryCast(item("AdapterCompatibility"), String), "").Trim(),
                        .PnpDeviceId = If(TryCast(item("PNPDeviceID"), String), "").Trim(),
                        .Source = "WMI"
                    }
                    AddGpuAdapter(adapters, seen, adapter)
                Next
            End Using
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.GetGpuAdapters.Wmi")
        End Try

        If adapters.Count = 0 Then
            Try
                Dim index As Integer = 0
                Dim device As DISPLAY_DEVICE = New DISPLAY_DEVICE()
                device.cb = Marshal.SizeOf(device)

                While EnumDisplayDevices(Nothing, index, device, 0)
                    Dim adapter As New GpuAdapterInfo With {
                        .Name = If(device.DeviceString, "").Trim(),
                        .AdapterCompatibility = "",
                        .PnpDeviceId = If(device.DeviceID, "").Trim(),
                        .Source = "EnumDisplayDevices"
                    }
                    AddGpuAdapter(adapters, seen, adapter)
                    index += 1
                    device = New DISPLAY_DEVICE()
                    device.cb = Marshal.SizeOf(device)
                End While
            Catch ex As Exception
                ErrorLogger.Log(ex, "MainForm.GetGpuAdapters.DisplayDevices")
            End Try
        End If

        Return adapters
    End Function

    Private Sub AddGpuAdapter(target As List(Of GpuAdapterInfo), seen As HashSet(Of String), adapter As GpuAdapterInfo)
        If target Is Nothing OrElse seen Is Nothing OrElse adapter Is Nothing Then
            Return
        End If

        If String.IsNullOrWhiteSpace(adapter.Name) AndAlso String.IsNullOrWhiteSpace(adapter.AdapterCompatibility) Then
            Return
        End If

        If IsGenericAdapter(adapter.Name) AndAlso IsGenericAdapter(adapter.AdapterCompatibility) Then
            Return
        End If

        Dim key As String = adapter.Name & "|" & adapter.AdapterCompatibility & "|" & adapter.PnpDeviceId
        If seen.Add(key) Then
            target.Add(adapter)
        End If
    End Sub

    Private Function BuildGpuCandidate(adapter As GpuAdapterInfo) As String
        If adapter Is Nothing Then
            Return ""
        End If

        Dim parts As New List(Of String)()
        If Not String.IsNullOrWhiteSpace(adapter.Name) Then
            parts.Add(adapter.Name)
        End If

        If Not String.IsNullOrWhiteSpace(adapter.AdapterCompatibility) AndAlso Not String.Equals(adapter.AdapterCompatibility, adapter.Name, StringComparison.OrdinalIgnoreCase) Then
            parts.Add(adapter.AdapterCompatibility)
        End If

        Dim vendorId As String = ExtractVendorId(adapter.PnpDeviceId)
        If Not String.IsNullOrWhiteSpace(vendorId) Then
            parts.Add("VEN_" & vendorId)
        End If

        Return String.Join("; ", parts)
    End Function

    Private Function ExtractVendorId(pnpDeviceId As String) As String
        If String.IsNullOrWhiteSpace(pnpDeviceId) Then
            Return ""
        End If

        Dim marker As String = "VEN_"
        Dim upper As String = pnpDeviceId.ToUpperInvariant()
        Dim markerIndex As Integer = upper.IndexOf(marker, StringComparison.Ordinal)
        If markerIndex < 0 Then
            Return ""
        End If

        markerIndex += marker.Length
        If markerIndex + 4 > upper.Length Then
            Return ""
        End If

        Dim vendorId As String = upper.Substring(markerIndex, 4)
        For Each ch As Char In vendorId
            Dim isDigit As Boolean = ch >= "0"c AndAlso ch <= "9"c
            Dim isHexLetter As Boolean = ch >= "A"c AndAlso ch <= "F"c
            If Not isDigit AndAlso Not isHexLetter Then
                Return ""
            End If
        Next

        Return vendorId
    End Function

    Private Function DetectVendorFromAdapter(adapter As GpuAdapterInfo) As GpuVendor
        If adapter Is Nothing Then
            Return GpuVendor.Unknown
        End If

        Dim vendorId As String = ExtractVendorId(adapter.PnpDeviceId)
        If String.Equals(vendorId, NvidiaPciVendorId, StringComparison.OrdinalIgnoreCase) Then
            Return GpuVendor.Nvidia
        End If
        If String.Equals(vendorId, AmdPciVendorId, StringComparison.OrdinalIgnoreCase) OrElse String.Equals(vendorId, IntelPciVendorId, StringComparison.OrdinalIgnoreCase) Then
            Return GpuVendor.AmdIntel
        End If

        Dim combined As String = (If(adapter.Name, "") & " " & If(adapter.AdapterCompatibility, "")).ToUpperInvariant()
        If ContainsNvidiaHint(combined) Then
            Return GpuVendor.Nvidia
        End If
        If ContainsAmdHint(combined) OrElse ContainsIntelHint(combined) Then
            Return GpuVendor.AmdIntel
        End If

        Return GpuVendor.Unknown
    End Function

    Private Function ContainsNvidiaHint(text As String) As Boolean
        If String.IsNullOrWhiteSpace(text) Then
            Return False
        End If

        Return text.Contains("NVIDIA") OrElse text.Contains("GEFORCE") OrElse text.Contains("RTX") OrElse text.Contains("GTX")
    End Function

    Private Function ContainsAmdHint(text As String) As Boolean
        If String.IsNullOrWhiteSpace(text) Then
            Return False
        End If

        Return text.Contains("AMD") OrElse text.Contains("RADEON") OrElse text.Contains("ATI") OrElse text.Contains("ADVANCED MICRO DEVICES")
    End Function

    Private Function ContainsIntelHint(text As String) As Boolean
        If String.IsNullOrWhiteSpace(text) Then
            Return False
        End If

        Return text.Contains("INTEL") OrElse text.Contains("ARC") OrElse text.Contains("IRIS") OrElse text.Contains("UHD")
    End Function

    Private Function IsAmdAdapter(adapter As GpuAdapterInfo) As Boolean
        If adapter Is Nothing Then
            Return False
        End If

        Dim vendorId As String = ExtractVendorId(adapter.PnpDeviceId)
        If String.Equals(vendorId, AmdPciVendorId, StringComparison.OrdinalIgnoreCase) Then
            Return True
        End If

        Dim combined As String = (If(adapter.Name, "") & " " & If(adapter.AdapterCompatibility, "")).ToUpperInvariant()
        Return ContainsAmdHint(combined)
    End Function

    Private Function IsGenericAdapter(name As String) As Boolean
        If String.IsNullOrWhiteSpace(name) Then
            Return True
        End If

        Dim upper As String = name.ToUpperInvariant()
        If upper.Contains("MICROSOFT") AndAlso (upper.Contains("BASIC") OrElse upper.Contains("RENDER") OrElse upper.Contains("REMOTE") OrElse upper.Contains("HYPER-V") OrElse upper.Contains("DISPLAY ADAPTER")) Then
            Return True
        End If
        If upper.Contains("VIRTUAL") OrElse upper.Contains("VMWARE") OrElse upper.Contains("VBOX") OrElse upper.Contains("PARALLELS") OrElse upper.Contains("VIRTIO") Then
            Return True
        End If

        Return False
    End Function

    Private Sub UpdateExperimentalTabAvailability(Optional logAction As Boolean = True)
        If tabExperimental Is Nothing Then
            Return
        End If

        Dim hasAmdRdna As Boolean = IsAmdRdnaDetected()
        Dim showOnUnsupported As Boolean = chkShowExperimentalTabOnUnsupportedGpu IsNot Nothing AndAlso chkShowExperimentalTabOnUnsupportedGpu.Checked
        Dim shouldShowTab As Boolean = hasAmdRdna OrElse showOnUnsupported

        If tabMain IsNot Nothing Then
            Dim containsTab As Boolean = tabMain.TabPages.Contains(tabExperimental)
            If shouldShowTab Then
                If Not containsTab Then
                    Dim insertIndex As Integer = tabMain.TabPages.IndexOf(tabSettings)
                    If insertIndex < 0 Then
                        insertIndex = tabMain.TabPages.Count
                    End If
                    tabMain.TabPages.Insert(insertIndex, tabExperimental)
                    ThemeManager.ApplyTheme(tabExperimental, ThemeSettings.GetPreferredColorMode())
                End If
                tabExperimental.Enabled = True
            Else
                If tabMain.SelectedTab Is tabExperimental Then
                    tabMain.SelectedTab = tabCompatibility
                End If
                If containsTab Then
                    tabMain.TabPages.Remove(tabExperimental)
                End If
            End If
        End If

        If Not hasAmdRdna AndAlso lblFsr4Status IsNot Nothing Then
            If showOnUnsupported Then
                lblFsr4Status.Text = "Experimental package: unsupported GPU (tab manually enabled)."
            Else
                lblFsr4Status.Text = "Experimental package: unavailable (AMD RDNA GPU not detected)."
            End If
        End If

        If logAction Then
            If hasAmdRdna Then
                AppendLog("FSR4 INT8 tab enabled (AMD RDNA GPU detected).")
            ElseIf showOnUnsupported Then
                AppendLog("FSR4 INT8 tab shown by settings override (AMD RDNA GPU not detected).")
            Else
                AppendLog("FSR4 INT8 tab disabled (no AMD RDNA GPU detected).")
            End If
        End If
    End Sub

    Private Function IsAmdRdnaDetected() As Boolean
        EnsureGpuDetectionInitialized()

        For Each adapter As GpuAdapterInfo In gpuDetectionAdapters
            If Not IsAmdAdapter(adapter) Then
                Continue For
            End If

            Dim candidate As String = (If(adapter.Name, "") & " " & If(adapter.AdapterCompatibility, "")).Trim()
            If IsLikelyAmdRdnaAdapter(candidate) Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Function IsLikelyAmdRdnaAdapter(adapterName As String) As Boolean
        If String.IsNullOrWhiteSpace(adapterName) Then
            Return False
        End If

        Dim upper As String = adapterName.ToUpperInvariant()
        If Not upper.Contains("AMD") AndAlso Not upper.Contains("RADEON") Then
            Return False
        End If

        If upper.Contains("RDNA") Then
            Return True
        End If

        Dim rdnaTokens As String() = {
            "5300", "5500", "5600", "5700",
            "6400", "6500", "6600", "6650", "6700", "6800", "6900", "6950",
            "740M", "760M", "780M",
            "7600", "7700", "7800", "7900", "9070", "9080",
            "880M", "890M",
            "RADEON PRO W7", "RADEON PRO W8", "RADEON PRO W9"
        }

        For Each token As String In rdnaTokens
            If upper.Contains(token, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next

        Dim searchIndex As Integer = 0
        While searchIndex < upper.Length
            Dim rxIndex As Integer = upper.IndexOf("RX", searchIndex, StringComparison.Ordinal)
            If rxIndex < 0 Then
                Exit While
            End If

            If rxIndex = 0 OrElse Not Char.IsLetterOrDigit(upper(rxIndex - 1)) Then
                Dim modelNumber As Integer = ParseRxModelNumber(upper, rxIndex + 2)
                If modelNumber >= 5000 Then
                    Return True
                End If
            End If

            searchIndex = rxIndex + 2
        End While

        Return False
    End Function

    Private Function ParseRxModelNumber(text As String, startIndex As Integer) As Integer
        If String.IsNullOrWhiteSpace(text) OrElse startIndex >= text.Length Then
            Return -1
        End If

        Dim index As Integer = startIndex
        While index < text.Length AndAlso (text(index) = " "c OrElse text(index) = "-"c OrElse text(index) = "_"c)
            index += 1
        End While

        Dim digits As New StringBuilder()
        While index < text.Length AndAlso Char.IsDigit(text(index))
            digits.Append(text(index))
            index += 1
            If digits.Length >= 5 Then
                Exit While
            End If
        End While

        If digits.Length < 4 Then
            Return -1
        End If

        Dim model As Integer = 0
        If Integer.TryParse(digits.ToString(), model) Then
            Return model
        End If

        Return -1
    End Function

    Private Sub LoadCompatibility()
        ' Load cached or bundled compatibility data for the detection list.
        allCompatibilityEntries = CompatibilityService.LoadCompatibilityList()
        compatibilityChangedNames.Clear()
        UpdateCompatibilityNote()
        AppendLog("Loaded compatibility list: " & allCompatibilityEntries.Count & " entries.")
        ApplyCompatibilityFilter()
    End Sub

    Private Sub LoadOptiPatcherSupportList()
        Try
            optiPatcherSupportEntries = OptiPatcherSupportService.LoadSupportList()
            optiPatcherSupportLookup = OptiPatcherSupportService.BuildLookup(optiPatcherSupportEntries)
            AppendLog("Loaded OptiPatcher support list: " & optiPatcherSupportEntries.Count & " entries.")
            ApplyCompatibilityFilter()
            UpdateOptiPatcherStatus()
        Catch ex As Exception
            AppendLog("Failed to load OptiPatcher support list: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.LoadOptiPatcherSupportList")
        End Try
    End Sub

    Private Async Function RefreshOptiPatcherSupportListAsync(isAuto As Boolean) As Task
        Try
            AppendLog(If(isAuto, "Auto-refreshing OptiPatcher support list...", "Refreshing OptiPatcher support list..."))
            optiPatcherSupportEntries = Await OptiPatcherSupportService.UpdateSupportListAsync()
            optiPatcherSupportLookup = OptiPatcherSupportService.BuildLookup(optiPatcherSupportEntries)
            AppendLog("OptiPatcher support list updated: " & optiPatcherSupportEntries.Count & " entries.")
            ApplyCompatibilityFilter()
            UpdateOptiPatcherStatus()
        Catch ex As Exception
            AppendLog("Failed to update OptiPatcher support list: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.RefreshOptiPatcherSupportList")
        End Try
    End Function

    Private Sub ApplyCompatibilityFilter()
        Dim filter As String = txtGameSearch.Text.Trim()
        Dim hideNonDetected As Boolean = chkHideNonDetected IsNot Nothing AndAlso chkHideNonDetected.Checked
        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim highlightChanges As Boolean = True
        If settings IsNot Nothing AndAlso settings.HighlightCompatibilityChanges.HasValue Then
            highlightChanges = settings.HighlightCompatibilityChanges.Value
        End If

        lvCompatibility.BeginUpdate()
        lvCompatibility.Items.Clear()

        For Each entry As CompatibilityEntry In allCompatibilityEntries
            If String.IsNullOrWhiteSpace(filter) OrElse entry.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 Then
                Dim normalizedKey As String = NameNormalization.NormalizeRelaxedName(entry.Name)
                Dim detected As DetectedGame = Nothing
                detectedLookup.TryGetValue(normalizedKey, detected)

                Dim isDetected As Boolean = detected IsNot Nothing
                If hideNonDetected AndAlso Not isDetected Then
                    Continue For
                End If

                Dim installInfo As OptiScalerInstallInfo = Nothing
                Dim patcherInfo As OptiPatcherInstallInfo = Nothing
                If isDetected Then
                    Dim lookupKey As String = GetDetectedInstallLookupKey(detected)
                    detectedInstallLookup.TryGetValue(lookupKey, installInfo)
                    detectedOptiPatcherLookup.TryGetValue(lookupKey, patcherInfo)
                End If

                Dim item As New ListViewItem(entry.Name)
                item.SubItems.Add(If(isDetected, "Yes", ""))
                item.SubItems.Add(GetInstallStatusText(isDetected, installInfo))
                item.SubItems.Add(GetOptiPatcherStatusText(entry, isDetected, patcherInfo))
                item.SubItems.Add(If(isDetected, detected.Platform, ""))
                item.SubItems.Add(If(isDetected, GetAntiCheatStatusText(detected), ""))
                item.SubItems.Add(If(isDetected, detected.InstallDir, ""))
                Dim isChanged As Boolean = highlightChanges AndAlso compatibilityChangedNames.Contains(normalizedKey)
                item.Tag = New CompatibilityRow With {.Entry = entry, .Detected = detected, .InstallInfo = installInfo, .OptiPatcherInfo = patcherInfo, .IsRecentlyChanged = isChanged}
                ApplyInstallRowColors(item, installInfo, isDetected, lvCompatibility.Items.Count, isChanged, isDetected AndAlso Not String.IsNullOrWhiteSpace(detected.AntiCheat))
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
        BrowseAndSelectGameExe("Browsing for game executable.")
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
        EnsurePluginsFolderReady(False)
        UpdateInstallStatus()
        UpdateOptiPatcherStatus()
        UpdateExperimentalStatus()
        UpdateExperimentalDetectedGamesList()
    End Sub

    Private Async Sub btnEditIni_Click(sender As Object, e As EventArgs) Handles btnEditIni.Click
        Await OpenIniEditorForCurrentTargetAsync()
    End Sub

    Private Async Function OpenIniEditorForCurrentTargetAsync() As Task
        Dim gameFolder As String = txtGameFolder.Text.Trim()
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            MessageBox.Show(Me, "Select a valid game folder first.", "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim installInfo As OptiScalerInstallInfo = OptiScalerInstallDetector.Detect(gameFolder)
        If installInfo Is Nothing OrElse Not installInfo.IsInstalled Then
            MessageBox.Show(Me, "OptiScaler is not detected in this folder. Install OptiScaler first, then edit the INI.", "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim iniPath As String = Path.Combine(gameFolder, "OptiScaler.ini")
        If Not EnsureIniFileForEditor(iniPath) Then
            Return
        End If

        AppendLog("Opening INI editor: " & iniPath)

        Using editor As New frmIniEditor(iniPath)
            editor.ShowDialog(Me)
            If editor.WasSaved Then
                AppendLog("INI editor: changes saved.")
            Else
                AppendLog("INI editor closed.")
            End If
        End Using

        UpdateInstallStatus()
        UpdateOptiPatcherStatus()
        UpdateExperimentalStatus()
        Await RefreshDetectedInstallStatesAsync(False)
    End Function

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

    Private Sub txtLocalArchive_TextChanged(sender As Object, e As EventArgs) Handles txtLocalArchive.TextChanged
        UpdateInstallActionButtons()
    End Sub

    Private Sub cmbHookName_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbHookName.SelectedIndexChanged
        UpdateInstallActionButtons()
    End Sub

    Private Sub InstallReadinessControlChanged(sender As Object, e As EventArgs) Handles cmbFgType.SelectedIndexChanged,
        txtNukemDll.TextChanged,
        txtFakenvapiFolder.TextChanged,
        txtNvngxDll.TextChanged,
        chkEnableReshade.CheckedChanged,
        txtReshadeDll.TextChanged,
        chkEnableSpecialK.CheckedChanged,
        txtSpecialKDll.TextChanged,
        chkLoadAsiPlugins.CheckedChanged,
        txtPluginsPath.TextChanged
        UpdateInstallActionButtons()
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

    Private Async Sub btnDeepScanDrives_Click(sender As Object, e As EventArgs) Handles btnDeepScanDrives.Click
        If allCompatibilityEntries Is Nothing OrElse allCompatibilityEntries.Count = 0 Then
            MessageBox.Show(Me, "Compatibility list is empty. Refresh lists first.", "Add Game Manually", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        AppendLog("Browsing for game executable to add manually.")
        Using dialog As New OpenFileDialog()
            dialog.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*"
            dialog.Title = "Select Game Executable"
            If File.Exists(txtGameExe.Text) Then
                dialog.FileName = txtGameExe.Text
            ElseIf Directory.Exists(txtGameFolder.Text) Then
                dialog.InitialDirectory = txtGameFolder.Text
            End If

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                AppendLog("Manual add cancelled by user.")
                Return
            End If

            Await AddManualDetectedGameAsync(dialog.FileName)
        End Using
    End Sub

    Private Sub chkHideNonDetected_CheckedChanged(sender As Object, e As EventArgs) Handles chkHideNonDetected.CheckedChanged
        ApplyCompatibilityFilter()
        If loadingSettingsUi Then
            Return
        End If

        Dim settings As AppSettingsModel = AppSettings.Load()
        settings.HideNonDetectedGames = chkHideNonDetected.Checked
        AppSettings.Save(settings)
        AppendLog("Hide non-detected preference set to " & If(chkHideNonDetected.Checked, "On", "Off") & ".")
    End Sub

    Private Sub btnBrowseFsr4PackageFolder_Click(sender As Object, e As EventArgs) Handles btnBrowseFsr4PackageFolder.Click
        AppendLog("Browsing for experimental FSR4 package folder.")
        Using dialog As New FolderBrowserDialog()
            dialog.Description = "Select a local folder containing FSR4 INT8 files"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtFsr4PackageFolder.Text = dialog.SelectedPath
                AppendLog("Selected experimental package folder: " & dialog.SelectedPath)
            End If
        End Using
    End Sub

    Private Sub btnFsr4BrowseGameExe_Click(sender As Object, e As EventArgs) Handles btnFsr4BrowseGameExe.Click
        BrowseAndSelectGameExe("Browsing for game executable from Experimental tab.")
    End Sub

    Private Sub BrowseAndSelectGameExe(startLogMessage As String)
        AppendLog(startLogMessage)
        Using dialog As New OpenFileDialog()
            dialog.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*"
            dialog.Title = "Select Game Executable"
            If File.Exists(txtGameExe.Text) Then
                dialog.FileName = txtGameExe.Text
            ElseIf Directory.Exists(txtGameFolder.Text) Then
                dialog.InitialDirectory = txtGameFolder.Text
            End If

            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtGameExe.Text = dialog.FileName
                AppendLog("Selected game executable: " & dialog.FileName)
            End If
        End Using
    End Sub

    Private Async Sub btnFsr4Apply_Click(sender As Object, e As EventArgs) Handles btnFsr4Apply.Click
        If String.IsNullOrWhiteSpace(txtGameFolder.Text) OrElse Not Directory.Exists(txtGameFolder.Text) Then
            MessageBox.Show(Me, "Select a valid game folder on the Install tab first.", "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If String.IsNullOrWhiteSpace(txtFsr4PackageFolder.Text) OrElse Not Directory.Exists(txtFsr4PackageFolder.Text) Then
            MessageBox.Show(Me, "Select a valid local package folder first.", "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            SetExperimentalActionButtonsEnabled(False)
            AppendLog("Applying experimental FSR4 package...")

            Dim options As New ExperimentalFsr4ApplyOptions With {
                .GameFolder = txtGameFolder.Text,
                .PackageFolder = txtFsr4PackageFolder.Text.Trim(),
                .ConflictMode = GetConflictModeFromIndex(cmbConflictMode.SelectedIndex),
                .EnableFsr4Update = chkFsr4EnableUpdate.Checked,
                .EnableAgilityUpgrade = chkFsr4EnableAgility.Checked
            }

            Dim manifest As ExperimentalFsr4Manifest = Await Task.Run(Function() ExperimentalFsr4Service.Apply(options, AddressOf AppendLog))
            Dim versionText As String = If(manifest Is Nothing OrElse String.IsNullOrWhiteSpace(manifest.PackageVersion), "unknown", manifest.PackageVersion)
            AppendLog("Experimental FSR4 package applied. Version: " & versionText)
            MessageBox.Show(Me, "Experimental FSR4 package applied successfully.", "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            AppendLog("Experimental FSR4 apply failed: " & ex.Message)
            MessageBox.Show(Me, ex.Message, "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.Log(ex, "MainForm.btnFsr4Apply")
        Finally
            SetExperimentalActionButtonsEnabled(True)
            UpdateExperimentalStatus()
        End Try
    End Sub

    Private Async Sub btnFsr4Remove_Click(sender As Object, e As EventArgs) Handles btnFsr4Remove.Click
        If String.IsNullOrWhiteSpace(txtGameFolder.Text) OrElse Not Directory.Exists(txtGameFolder.Text) Then
            MessageBox.Show(Me, "Select a valid game folder on the Install tab first.", "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim status As ExperimentalFsr4Status = ExperimentalFsr4Service.Detect(txtGameFolder.Text)
        If status Is Nothing OrElse Not status.IsInstalled Then
            MessageBox.Show(Me, "No experimental FSR4 package was detected for this game folder.", "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If status.IsInstalled AndAlso Not status.IsManaged Then
            MessageBox.Show(Me,
                            "Experimental markers were detected, but this install is unmanaged." & Environment.NewLine &
                            "To avoid deleting unknown files, automatic remove is blocked.",
                            "Experimental FSR4",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
            AppendLog("Experimental remove blocked: install is unmanaged.")
            Return
        End If

        Dim confirm As DialogResult = MessageBox.Show(Me,
                                                      "Remove the experimental FSR4 package from this game folder?" & Environment.NewLine &
                                                      "Backups and INI values tracked by the installer will be restored.",
                                                      "Experimental FSR4",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question)
        If confirm <> DialogResult.Yes Then
            Return
        End If

        Try
            SetExperimentalActionButtonsEnabled(False)
            AppendLog("Removing experimental FSR4 package...")
            Dim removed As Boolean = Await Task.Run(Function() ExperimentalFsr4Service.Remove(txtGameFolder.Text, AddressOf AppendLog))
            If removed Then
                AppendLog("Experimental FSR4 package removed.")
                MessageBox.Show(Me, "Experimental FSR4 package removed.", "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                AppendLog("Experimental remove skipped (no managed manifest found).")
                MessageBox.Show(Me, "No managed experimental package was found for this folder.", "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            AppendLog("Experimental remove failed: " & ex.Message)
            MessageBox.Show(Me, ex.Message, "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.Log(ex, "MainForm.btnFsr4Remove")
        Finally
            SetExperimentalActionButtonsEnabled(True)
            UpdateExperimentalStatus()
        End Try
    End Sub

    Private Sub btnFsr4RefreshStatus_Click(sender As Object, e As EventArgs) Handles btnFsr4RefreshStatus.Click
        UpdateExperimentalStatus()
    End Sub

    Private Sub btnFsr4PickGame_Click(sender As Object, e As EventArgs) Handles btnFsr4PickGame.Click
        tabMain.SelectedTab = tabInstall
        If txtGameExe IsNot Nothing Then
            txtGameExe.Focus()
        End If
        AppendLog("Switched to Install tab to pick/change target game.")
    End Sub

    Private Async Sub btnFsr4ScanDetectedGames_Click(sender As Object, e As EventArgs) Handles btnFsr4ScanDetectedGames.Click
        Await RunUnifiedDetectionAsync(False)
    End Sub

    Private Sub btnFsr4UseSelectedGame_Click(sender As Object, e As EventArgs) Handles btnFsr4UseSelectedGame.Click
        Dim selectedGame As DetectedGame = GetSelectedExperimentalDetectedGame()
        If selectedGame Is Nothing Then
            MessageBox.Show(Me, "Select a detected game first, or browse for a game EXE.", "Experimental FSR4", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        UseExperimentalDetectedGame(selectedGame)
    End Sub

    Private Sub lvFsr4DetectedGames_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvFsr4DetectedGames.SelectedIndexChanged
        btnFsr4UseSelectedGame.Enabled = GetSelectedExperimentalDetectedGame() IsNot Nothing
    End Sub

    Private Sub lvFsr4DetectedGames_DoubleClick(sender As Object, e As EventArgs) Handles lvFsr4DetectedGames.DoubleClick
        Dim selectedGame As DetectedGame = GetSelectedExperimentalDetectedGame()
        If selectedGame Is Nothing Then
            Return
        End If

        UseExperimentalDetectedGame(selectedGame)
    End Sub

    Private Function GetSelectedExperimentalDetectedGame() As DetectedGame
        If lvFsr4DetectedGames Is Nothing OrElse lvFsr4DetectedGames.SelectedItems.Count = 0 Then
            Return Nothing
        End If

        Return TryCast(lvFsr4DetectedGames.SelectedItems(0).Tag, DetectedGame)
    End Function

    Private Sub UseExperimentalDetectedGame(game As DetectedGame)
        If game Is Nothing Then
            Return
        End If

        txtGameFolder.Text = game.InstallDir

        Dim exePath As String = FindPreferredExecutable(game.InstallDir, game.DisplayName, game.SourceName)
        If Not String.IsNullOrWhiteSpace(exePath) Then
            txtGameExe.Text = exePath
        End If

        AppendLog("Experimental target game set to: " & game.DisplayName)
    End Sub

    Private Sub SetExperimentalActionButtonsEnabled(enabled As Boolean)
        btnFsr4Apply.Enabled = enabled
        btnFsr4Remove.Enabled = enabled
        btnFsr4RefreshStatus.Enabled = enabled
        btnFsr4ScanDetectedGames.Enabled = enabled
        btnFsr4BrowseGameExe.Enabled = enabled
        btnFsr4PickGame.Enabled = enabled
        If enabled Then
            btnFsr4UseSelectedGame.Enabled = GetSelectedExperimentalDetectedGame() IsNot Nothing
        Else
            btnFsr4UseSelectedGame.Enabled = False
        End If
    End Sub

    Private Sub cmbDefaultPreset_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbDefaultPreset.SelectedIndexChanged
        If _settingDefaultsPreset Then
            Return
        End If

        If cmbDefaultPreset.SelectedIndex <= 0 Then
            Return
        End If

        _settingDefaultsPreset = True
        Select Case cmbDefaultPreset.SelectedIndex
            Case 1
                cmbDefaultGpuVendor.SelectedIndex = 1
                chkDefaultDlssInputs.Checked = True
            Case 2
                cmbDefaultGpuVendor.SelectedIndex = 2
                chkDefaultDlssInputs.Checked = False
        End Select
        _settingDefaultsPreset = False
    End Sub

    Private Sub DefaultInstallSettingChanged(sender As Object, e As EventArgs) Handles cmbDefaultHookName.SelectedIndexChanged,
        cmbDefaultGpuVendor.SelectedIndexChanged,
        chkDefaultDlssInputs.CheckedChanged,
        cmbDefaultFgType.SelectedIndexChanged,
        cmbDefaultConflictMode.SelectedIndexChanged
        If _settingDefaultsPreset Then
            Return
        End If

        If cmbDefaultPreset.SelectedIndex < 0 Then
            Return
        End If

        If cmbDefaultPreset.SelectedIndex <> 0 Then
            _settingDefaultsPreset = True
            cmbDefaultPreset.SelectedIndex = 0
            _settingDefaultsPreset = False
        End If
    End Sub

    Private Sub btnApplyDefaults_Click(sender As Object, e As EventArgs) Handles btnApplyDefaults.Click
        ApplyDefaultInstallOptionsFromUi(True)
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
        UpdateInstallActionButtons()
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
        Await RefreshReleaseInfoAsync(True)
    End Sub

    Private Async Function RefreshReleaseInfoAsync(reportStatus As Boolean) As Task
        ' Refresh stable/nightly release metadata without blocking the UI.
        AppendLog("Refreshing release info...")
        If reportStatus Then
            SetStatus("Fetching release info...")
        End If

        Dim stableOk As Boolean = False
        Dim nightlyOk As Boolean = False
        Dim componentOk As Boolean = False
        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim alternateUrl As String = If(settings Is Nothing, "", settings.NightlyReleaseUrl)
        Dim componentUrl As String = If(settings Is Nothing, "", settings.ComponentReleaseUrl)

        Try
            stableRelease = Await ReleaseService.GetStableReleaseAsync()
            stableOk = stableRelease IsNot Nothing
        Catch ex As HttpRequestException When ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = HttpStatusCode.NotFound
            stableRelease = Nothing
            AppendLog("Stable release not found (404). Check the stable release URL in Settings.")
        Catch ex As Exception
            stableRelease = Nothing
            AppendLog("Failed to fetch stable release: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.RefreshReleases.Stable")
        End Try

        If String.IsNullOrWhiteSpace(alternateUrl) Then
            nightlyRelease = Nothing
            AppendLog("Alternate source URL not set; skipping.")
        Else
            Try
                nightlyRelease = Await ReleaseService.GetNightlyReleaseAsync()
                nightlyOk = nightlyRelease IsNot Nothing
            Catch ex As HttpRequestException When ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = HttpStatusCode.NotFound
                nightlyRelease = Nothing
                AppendLog("Alternate source not found (404). It may be unavailable or the URL may be incorrect.")
            Catch ex As Exception
                nightlyRelease = Nothing
                AppendLog("Failed to fetch alternate source release: " & ex.Message)
                ErrorLogger.Log(ex, "MainForm.RefreshReleases.Alternate")
            End Try
        End If

        If String.IsNullOrWhiteSpace(componentUrl) Then
            componentRelease = Nothing
        Else
            Try
                componentRelease = Await ReleaseService.GetComponentReleaseAsync()
                componentOk = componentRelease IsNot Nothing
                If componentOk Then
                    AppendLog("Component source loaded: " & componentRelease.TagName)
                End If
            Catch ex As HttpRequestException When ex.StatusCode.HasValue AndAlso ex.StatusCode.Value = HttpStatusCode.NotFound
                componentRelease = Nothing
                AppendLog("Component source not found (404). Check component URL in settings.json.")
            Catch ex As Exception
                componentRelease = Nothing
                AppendLog("Failed to fetch component source release: " & ex.Message)
                ErrorLogger.Log(ex, "MainForm.RefreshReleases.Component")
            End Try
        End If

        UpdateReleaseLabels()
        If String.IsNullOrWhiteSpace(alternateUrl) Then
            lblNightlyInfo.Text = "Alternate: not configured"
        End If

        If stableOk OrElse nightlyOk OrElse componentOk Then
            If reportStatus Then
                SetStatus("Release info updated.")
            End If
            AppendLog("Release info updated.")
        Else
            If reportStatus Then
                SetStatus("Release fetch failed.")
            End If
            AppendLog("Failed to fetch releases.")
        End If

        UpdateInstallActionButtons()
    End Function

    Private Sub UpdateReleaseLabels()
        lblStableInfo.Text = FormatReleaseLabel("Stable", stableRelease)
        lblNightlyInfo.Text = FormatReleaseLabel("Alternate", nightlyRelease)
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

    Private Async Function RefreshOptiPatcherReleaseInfoAsync(reportStatus As Boolean) As Task
        Try
            If reportStatus Then
                SetStatus("Refreshing OptiPatcher release info...")
            End If

            AppendLog("Refreshing OptiPatcher releases...")

            optiPatcherStableRelease = Await OptiPatcherReleaseService.GetStableReleaseAsync()
            AppendLog("OptiPatcher stable loaded: " & If(optiPatcherStableRelease?.TagName, "n/a"))
        Catch ex As Exception
            optiPatcherStableRelease = Nothing
            AppendLog("Failed to fetch OptiPatcher stable release: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.RefreshOptiPatcher.Stable")
        End Try

        Try
            optiPatcherRollingRelease = Await OptiPatcherReleaseService.GetRollingReleaseAsync()
            AppendLog("OptiPatcher rolling loaded: " & If(optiPatcherRollingRelease?.TagName, "n/a"))
        Catch ex As Exception
            optiPatcherRollingRelease = Nothing
            AppendLog("Failed to fetch OptiPatcher rolling release: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.RefreshOptiPatcher.Rolling")
        End Try

        Dim alternateUrl As String = If(AppSettings.Load()?.OptiPatcherAlternateReleaseUrl, "")
        If String.IsNullOrWhiteSpace(alternateUrl) Then
            optiPatcherAlternateRelease = Nothing
            AppendLog("OptiPatcher alternate release URL not set; skipping.")
        Else
            Try
                optiPatcherAlternateRelease = Await OptiPatcherReleaseService.GetAlternateReleaseAsync()
                AppendLog("OptiPatcher alternate loaded: " & If(optiPatcherAlternateRelease?.TagName, "n/a"))
            Catch ex As Exception
                optiPatcherAlternateRelease = Nothing
                AppendLog("Failed to fetch OptiPatcher alternate release: " & ex.Message)
                ErrorLogger.Log(ex, "MainForm.RefreshOptiPatcher.Alternate")
            End Try
        End If

        UpdateOptiPatcherReleaseLabel()
        If reportStatus Then
            SetStatus("OptiPatcher release info updated.")
        End If
    End Function

    Private Sub UpdateOptiPatcherReleaseLabel()
        If lblOptiPatcherRelease Is Nothing Then
            Return
        End If

        Dim source As OptiPatcherSource = GetOptiPatcherSourceFromUi()
        Dim release As ReleaseInfo = Nothing
        Select Case source
            Case OptiPatcherSource.Stable
                release = optiPatcherStableRelease
            Case OptiPatcherSource.Alternate
                release = optiPatcherAlternateRelease
            Case OptiPatcherSource.LocalFile
                lblOptiPatcherRelease.Text = "OptiPatcher: local file"
                Return
            Case Else
                release = optiPatcherRollingRelease
        End Select

        lblOptiPatcherRelease.Text = FormatReleaseLabel("OptiPatcher", release)
    End Sub

    Private Function GetOptiPatcherSourceFromUi() As OptiPatcherSource
        If cmbOptiPatcherSource Is Nothing Then
            Return OptiPatcherSource.Rolling
        End If

        Select Case cmbOptiPatcherSource.SelectedIndex
            Case 1
                Return OptiPatcherSource.Stable
            Case 2
                Return OptiPatcherSource.Alternate
            Case 3
                Return OptiPatcherSource.LocalFile
            Case Else
                Return OptiPatcherSource.Rolling
        End Select
    End Function

    Private Function GetOptiPatcherSourceIndex(value As String) As Integer
        If String.IsNullOrWhiteSpace(value) Then
            Return 0
        End If

        Select Case value.Trim().ToLowerInvariant()
            Case "stable"
                Return 1
            Case "alternate"
                Return 2
            Case "local", "localfile"
                Return 3
            Case Else
                Return 0
        End Select
    End Function

    Private Function GetOptiPatcherSourceToken(index As Integer) As String
        Select Case index
            Case 1
                Return "Stable"
            Case 2
                Return "Alternate"
            Case 3
                Return "LocalFile"
            Case Else
                Return "Rolling"
        End Select
    End Function

    Private Sub ToggleOptiPatcherLocalFile()
        If cmbOptiPatcherSource Is Nothing Then
            Return
        End If

        Dim isLocal As Boolean = cmbOptiPatcherSource.SelectedIndex = 3
        If txtOptiPatcherLocalFile IsNot Nothing Then
            txtOptiPatcherLocalFile.Enabled = isLocal
        End If
        If btnBrowseOptiPatcherLocal IsNot Nothing Then
            btnBrowseOptiPatcherLocal.Enabled = isLocal
        End If
    End Sub

    Private Function BuildOptiPatcherConfig() As OptiPatcherInstallConfig
        Return New OptiPatcherInstallConfig With {
            .GameFolder = txtGameFolder.Text.Trim(),
            .Source = GetOptiPatcherSourceFromUi(),
            .StableRelease = optiPatcherStableRelease,
            .RollingRelease = optiPatcherRollingRelease,
            .AlternateRelease = optiPatcherAlternateRelease,
            .LocalAsiPath = txtOptiPatcherLocalFile.Text.Trim(),
            .ConflictMode = GetConflictModeFromIndex(cmbConflictMode.SelectedIndex),
            .PluginPathOverride = If(String.IsNullOrWhiteSpace(txtPluginsPath.Text), "", txtPluginsPath.Text.Trim())
        }
    End Function

    Private Async Sub cmbOptiPatcherSource_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbOptiPatcherSource.SelectedIndexChanged
        ToggleOptiPatcherLocalFile()
        UpdateOptiPatcherReleaseLabel()
        SaveOptiPatcherUiSettings()
        Await Task.CompletedTask
    End Sub

    Private Sub SaveOptiPatcherUiSettings()
        If loadingSettingsUi Then
            Return
        End If

        Dim settings As AppSettingsModel = AppSettings.Load()
        settings.OptiPatcherPreferredSource = GetOptiPatcherSourceToken(cmbOptiPatcherSource.SelectedIndex)
        settings.OptiPatcherLocalPath = txtOptiPatcherLocalFile.Text.Trim()
        AppSettings.Save(settings)
    End Sub

    Private Sub btnBrowseOptiPatcherLocal_Click(sender As Object, e As EventArgs) Handles btnBrowseOptiPatcherLocal.Click
        AppendLog("Browsing for local OptiPatcher file.")
        Using dialog As New OpenFileDialog()
            dialog.Filter = "OptiPatcher plugin (*.asi)|*.asi|All files (*.*)|*.*"
            dialog.Title = "Select OptiPatcher.asi"
            If dialog.ShowDialog(Me) = DialogResult.OK Then
                txtOptiPatcherLocalFile.Text = dialog.FileName
                AppendLog("Selected local OptiPatcher file: " & dialog.FileName)
                SaveOptiPatcherUiSettings()
            End If
        End Using
    End Sub

    Private Async Sub btnOptiPatcherRefresh_Click(sender As Object, e As EventArgs) Handles btnOptiPatcherRefresh.Click
        Await RefreshOptiPatcherReleaseInfoAsync(True)
    End Sub

    Private Async Sub btnInstallOptiPatcher_Click(sender As Object, e As EventArgs) Handles btnInstallOptiPatcher.Click
        Try
            btnInstallOptiPatcher.Enabled = False
            Dim installed As Boolean = Await InstallOptiPatcherAsync(True)
            UpdateOptiPatcherStatus()
            If installed Then
                Await RefreshDetectedInstallStatesAsync(False)
            End If
        Finally
            btnInstallOptiPatcher.Enabled = True
        End Try
    End Sub

    Private Async Sub btnRemoveOptiPatcher_Click(sender As Object, e As EventArgs) Handles btnRemoveOptiPatcher.Click
        Try
            btnRemoveOptiPatcher.Enabled = False
            Dim gameFolder As String = txtGameFolder.Text.Trim()
            If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
                MessageBox.Show(Me, "Select a valid game folder first.", "OptiPatcher", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim detected As OptiPatcherInstallInfo = OptiPatcherInstallDetector.Detect(gameFolder)
            If detected Is Nothing OrElse Not detected.IsInstalled Then
                MessageBox.Show(Me, "OptiPatcher is not detected in this game folder.", "OptiPatcher", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If detected.Manifest Is Nothing Then
                Dim unmanagedConfirm As DialogResult = MessageBox.Show(Me,
                                                                       "OptiPatcher appears to be unmanaged. Remove the detected plugin file anyway?",
                                                                       "OptiPatcher",
                                                                       MessageBoxButtons.YesNo,
                                                                       MessageBoxIcon.Question)
                If unmanagedConfirm <> DialogResult.Yes Then
                    AppendLog("OptiPatcher remove canceled (unmanaged install).")
                    Return
                End If
            End If

            Dim removed As Boolean = Await OptiPatcherInstallerService.RemoveAsync(gameFolder, AddressOf AppendLog, True)
            If removed Then
                AppendLog("OptiPatcher removed.")
            Else
                AppendLog("OptiPatcher remove completed with no file changes.")
            End If

            UpdateOptiPatcherStatus()
            Await RefreshDetectedInstallStatesAsync(False)
        Catch ex As Exception
            AppendLog("OptiPatcher remove failed: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.btnRemoveOptiPatcher_Click")
            MessageBox.Show(Me, "OptiPatcher remove failed: " & ex.Message, "OptiPatcher", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            btnRemoveOptiPatcher.Enabled = True
        End Try
    End Sub

    Private Async Sub btnInstall_Click(sender As Object, e As EventArgs) Handles btnInstall.Click
        Try
            installOperationInProgress = True
            UpdateInstallActionButtons()
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
                Await RefreshDetectedInstallStatesAsync(False)
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

            Dim detectedGame As DetectedGame = Nothing
            Dim optiPatcherSupport As OptiPatcherSupportEntry = ResolveCurrentOptiPatcherSupport(detectedGame)
            Dim installOptiPatcherAfterInstall As Boolean = chkInstallOptiPatcher IsNot Nothing AndAlso chkInstallOptiPatcher.Checked
            If installOptiPatcherAfterInstall AndAlso optiPatcherSupport Is Nothing Then
                AppendLog("Install blocked: OptiPatcher option requires a supported detected game.")
                MessageBox.Show(Me,
                                "OptiPatcher can only be installed for supported detected games." & Environment.NewLine &
                                "Select a supported game from Game Detection first.",
                                "Unsupported game",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
                Return
            End If

            If installOptiPatcherAfterInstall AndAlso Not Await ValidateAutoOptiPatcherPreflightAsync() Then
                AppendLog("Install aborted (OptiPatcher preflight).")
                Return
            End If

            If Not ConfirmInstallSummary(action, config, installOptiPatcherAfterInstall, detectedGame, optiPatcherSupport) Then
                AppendLog("Install canceled at summary confirmation.")
                Return
            End If

            If Not RunInstallPreflight(config) Then
                AppendLog("Install aborted (preflight).")
                Return
            End If

            Dim manifest As InstallManifest = Await InstallerService.InstallAsync(config, AddressOf AppendLog, AddressOf UpdateProgress)
            Dim verification As InstallVerificationReport = Await Task.Run(Function() InstallerService.VerifyInstall(config, manifest))
            If manifest IsNot Nothing Then
                manifest.VerificationTimeUtc = DateTime.UtcNow
            End If

            Dim optiPatcherOutcome As String = ""
            If installOptiPatcherAfterInstall Then
                AppendLog("Installing OptiPatcher as part of this install...")
                Dim patcherInstalled As Boolean = Await InstallOptiPatcherAsync(False)
                optiPatcherOutcome = If(patcherInstalled, "OptiPatcher installed.", "OptiPatcher install skipped or failed.")
            End If

            LogVerificationReport(verification)

            If verification IsNot Nothing AndAlso verification.Errors.Count > 0 Then
                MessageBox.Show(Me,
                                "Install finished with verification errors." & Environment.NewLine &
                                "Open diagnostics/log output for details.",
                                "Install Completed With Issues",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
            Else
                Dim warningCount As Integer = If(verification Is Nothing, 0, verification.Warnings.Count)
                Dim message As String = "OptiScaler installed successfully."
                If warningCount > 0 Then
                    message &= Environment.NewLine & warningCount.ToString() & " verification warning(s) were reported."
                End If
                If Not String.IsNullOrWhiteSpace(optiPatcherOutcome) Then
                    message &= Environment.NewLine & optiPatcherOutcome
                End If
                MessageBox.Show(Me, message, "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            AppendLog("Install completed.")
            Await RefreshDetectedInstallStatesAsync(False)
        Catch ex As Exception
            AppendLog("Install failed: " & ex.Message)
            MessageBox.Show(Me, ex.Message, "Install Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.Log(ex, "MainForm.Install")
        Finally
            installOperationInProgress = False
            UpdateProgress(0)
            UpdateInstallStatus()
            UpdateExperimentalStatus()
        End Try
    End Sub

    Private Async Sub btnUninstall_Click(sender As Object, e As EventArgs) Handles btnUninstall.Click
        Try
            uninstallOperationInProgress = True
            UpdateInstallActionButtons()
            AppendLog("Starting uninstall...")
            Await TryUninstallAsync(txtGameFolder.Text, True)
            Await RefreshDetectedInstallStatesAsync(False)
        Catch ex As Exception
            AppendLog("Uninstall failed: " & ex.Message)
            MessageBox.Show(Me, ex.Message, "Uninstall Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.Log(ex, "MainForm.Uninstall")
        Finally
            uninstallOperationInProgress = False
            UpdateInstallStatus()
            UpdateExperimentalStatus()
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

    Private Function RunInstallPreflight(config As InstallerConfig) As Boolean
        If config Is Nothing Then
            MessageBox.Show(Me, "Installer configuration is missing.", "Install", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        Dim errors As New List(Of String)()
        Dim warnings As New List(Of String)()

        If String.IsNullOrWhiteSpace(config.GameExePath) OrElse Not File.Exists(config.GameExePath) Then
            errors.Add("Game executable not found.")
        End If

        If String.IsNullOrWhiteSpace(config.GameFolder) OrElse Not Directory.Exists(config.GameFolder) Then
            errors.Add("Game folder not found.")
        End If

        If String.IsNullOrWhiteSpace(config.HookName) Then
            errors.Add("Hook filename is missing.")
        End If

        If config.Source = ReleaseSource.LocalArchive Then
            If String.IsNullOrWhiteSpace(config.LocalArchivePath) OrElse Not File.Exists(config.LocalArchivePath) Then
                errors.Add("Local OptiScaler archive not found.")
            Else
                Dim extension As String = Path.GetExtension(config.LocalArchivePath).ToLowerInvariant()
                If extension <> ".7z" AndAlso extension <> ".zip" Then
                    warnings.Add("Local archive extension is unusual (" & extension & ").")
                End If

                Try
                    Dim size As Long = New FileInfo(config.LocalArchivePath).Length
                    If size <= 0 Then
                        errors.Add("Local OptiScaler archive is empty.")
                    End If
                Catch ex As Exception
                    ErrorLogger.Log(ex, "MainForm.RunInstallPreflight.LocalArchive")
                End Try
            End If
        End If

        Dim bundledRelease As Boolean = InstallerService.IsLikelyBundledComponentRelease(config)
        If config.FgType = FgTypeSelection.Nukem Then
            If String.IsNullOrWhiteSpace(config.NukemDllPath) OrElse Not File.Exists(config.NukemDllPath) Then
                If bundledRelease Then
                    warnings.Add("Nukem DLL not supplied. This release is expected to bundle frame generation components.")
                Else
                    errors.Add("Nukem frame generation selected but DLL is missing.")
                End If
            ElseIf Not Path.GetExtension(config.NukemDllPath).Equals(".dll", StringComparison.OrdinalIgnoreCase) Then
                errors.Add("Nukem path must be a DLL file.")
            ElseIf Not Path.GetFileName(config.NukemDllPath).Equals("dlssg_to_fsr3_amd_is_better.dll", StringComparison.OrdinalIgnoreCase) Then
                warnings.Add("Nukem DLL filename is unexpected. Expected: dlssg_to_fsr3_amd_is_better.dll.")
            End If
        End If

        If Not String.IsNullOrWhiteSpace(config.FakenvapiFolder) Then
            Dim fakenvapiRoot As String = config.FakenvapiFolder
            If File.Exists(fakenvapiRoot) Then
                fakenvapiRoot = Path.GetDirectoryName(fakenvapiRoot)
            End If

            If String.IsNullOrWhiteSpace(fakenvapiRoot) OrElse Not Directory.Exists(fakenvapiRoot) Then
                errors.Add("Fakenvapi folder is invalid.")
            Else
                Dim nvapiPath As String = Path.Combine(fakenvapiRoot, "nvapi64.dll")
                Dim iniPath As String = Path.Combine(fakenvapiRoot, "fakenvapi.ini")
                If Not File.Exists(nvapiPath) OrElse Not File.Exists(iniPath) Then
                    warnings.Add("Fakenvapi folder is missing nvapi64.dll or fakenvapi.ini.")
                End If
            End If
        End If

        If Not String.IsNullOrWhiteSpace(config.NvngxDllPath) Then
            If Not File.Exists(config.NvngxDllPath) Then
                errors.Add("nvngx_dlss.dll path was set but file was not found.")
            ElseIf Not Path.GetFileName(config.NvngxDllPath).Equals("nvngx_dlss.dll", StringComparison.OrdinalIgnoreCase) Then
                warnings.Add("nvngx override filename is unexpected. Expected: nvngx_dlss.dll.")
            End If
        End If

        If config.EnableReshade Then
            If String.IsNullOrWhiteSpace(config.ReshadeDllPath) OrElse Not File.Exists(config.ReshadeDllPath) Then
                errors.Add("ReShade is enabled but DLL path is missing.")
            ElseIf Not Path.GetExtension(config.ReshadeDllPath).Equals(".dll", StringComparison.OrdinalIgnoreCase) Then
                errors.Add("ReShade path must be a DLL file.")
            End If
        End If

        If config.EnableSpecialK Then
            If String.IsNullOrWhiteSpace(config.SpecialKDllPath) OrElse Not File.Exists(config.SpecialKDllPath) Then
                errors.Add("Special K is enabled but SpecialK64.dll path is missing.")
            ElseIf Not Path.GetExtension(config.SpecialKDllPath).Equals(".dll", StringComparison.OrdinalIgnoreCase) Then
                errors.Add("Special K path must be a DLL file.")
            ElseIf Not Path.GetFileName(config.SpecialKDllPath).Equals("SpecialK64.dll", StringComparison.OrdinalIgnoreCase) Then
                warnings.Add("Special K DLL filename is unexpected. Expected: SpecialK64.dll.")
            End If
        End If

        If config.LoadAsiPlugins Then
            If String.IsNullOrWhiteSpace(config.PluginsPath) OrElse Not Directory.Exists(config.PluginsPath) Then
                errors.Add("ASI plugins are enabled but plugins path is missing.")
            Else
                Dim pluginCount As Integer = Directory.GetFiles(config.PluginsPath, "*.asi", SearchOption.TopDirectoryOnly).Length
                If pluginCount = 0 Then
                    warnings.Add("ASI plugin loading is enabled but no *.asi files were found in the selected folder.")
                End If
            End If
        End If

        TryAutoRetargetUnrealInstall(config)

        If ShouldSkipExecutable(Path.GetFileNameWithoutExtension(config.GameExePath)) Then
            warnings.Add("Selected executable looks like a launcher/helper tool. Prefer the main game executable.")
        End If

        If errors.Count > 0 Then
            Dim message As String = "Fix the following before installing:" & Environment.NewLine & "- " & String.Join(Environment.NewLine & "- ", errors)
            MessageBox.Show(Me, message, "Install validation failed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AppendLog("Preflight errors: " & String.Join("; ", errors))
            Return False
        End If

        If Not String.IsNullOrWhiteSpace(config.GameExePath) AndAlso Not String.IsNullOrWhiteSpace(config.GameFolder) Then
            Dim exeDir As String = Path.GetDirectoryName(config.GameExePath)
            If Not String.IsNullOrWhiteSpace(exeDir) Then
                Dim normalizedExeDir As String = NormalizePathSafe(exeDir)
                Dim normalizedGameDir As String = NormalizePathSafe(config.GameFolder)
                If Not String.Equals(normalizedExeDir, normalizedGameDir, StringComparison.OrdinalIgnoreCase) Then
                    warnings.Add("Game executable is not inside the selected game folder.")
                End If
            End If
        End If

        If Directory.Exists(Path.Combine(config.GameFolder, "Engine")) Then
            warnings.Add("Engine folder detected. Unreal Engine games should target the Win64/WinGDK binaries folder.")
        End If

        Dim antiCheat As AntiCheatScanResult = AntiCheatService.Detect(config.GameFolder)
        If antiCheat IsNot Nothing AndAlso antiCheat.Detected Then
            warnings.Add("Anti-cheat appears to be present (" & antiCheat.Provider & "). Using OptiScaler with anti-cheat protected online games can cause bans.")
        End If

        If Not IsFolderWritable(config.GameFolder) Then
            warnings.Add("Game folder is not writable. Installation may require admin rights.")
        End If

        If warnings.Count > 0 Then
            Dim message As String = "Continue with these warnings?" & Environment.NewLine & "- " & String.Join(Environment.NewLine & "- ", warnings)
            Dim result As DialogResult = MessageBox.Show(Me, message, "Install warnings", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            AppendLog("Preflight warnings: " & String.Join("; ", warnings))
            Return result = DialogResult.Yes
        End If

        Return True
    End Function

    Private Function NormalizePathSafe(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim normalized As String = value.Trim().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        If IsDriveLetterOnlyPath(normalized) Then
            normalized &= Path.DirectorySeparatorChar
        End If

        Try
            normalized = Path.GetFullPath(normalized)
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.NormalizePathSafe")
        End Try

        Return TrimPathExceptRoot(normalized)
    End Function

    Private Function IsDriveLetterOnlyPath(value As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(value) AndAlso
               value.Length = 2 AndAlso
               Char.IsLetter(value(0)) AndAlso
               value(1) = ":"c
    End Function

    Private Function TrimPathExceptRoot(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim normalized As String = value.Trim()
        Dim root As String = ""
        Try
            root = Path.GetPathRoot(normalized)
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.TrimPathExceptRoot")
        End Try

        If Not String.IsNullOrWhiteSpace(root) Then
            Dim normalizedNoSlash As String = normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            Dim rootNoSlash As String = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            If String.Equals(normalizedNoSlash, rootNoSlash, StringComparison.OrdinalIgnoreCase) Then
                Return root
            End If
        End If

        Return normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
    End Function

    Private Function IsFolderWritable(folderPath As String) As Boolean
        If String.IsNullOrWhiteSpace(folderPath) OrElse Not Directory.Exists(folderPath) Then
            Return False
        End If

        Try
            Dim testPath As String = Path.Combine(folderPath, ".write_test_" & Guid.NewGuid().ToString("N"))
            File.WriteAllText(testPath, "x")
            File.Delete(testPath)
            Return True
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.IsFolderWritable")
            Return False
        End Try
    End Function

    Private Sub btnOpenWiki_Click(sender As Object, e As EventArgs) Handles btnOpenWiki.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Entry Is Nothing Then
            AppendLog("Open wiki skipped: no game selected.")
            Return
        End If

        OpenCompatibilityWiki(row.Entry)
    End Sub

    Private Sub OpenCompatibilityWiki(entry As CompatibilityEntry)
        If entry Is Nothing Then
            Return
        End If

        Dim url As String = BuildWikiUrl(entry.Slug)
        If String.IsNullOrWhiteSpace(url) Then
            MessageBox.Show(Me, "Wiki base URL is not set. Update it in Settings.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        AppendLog("Opening wiki page: " & url)
        Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
    End Sub

    Private Async Sub btnRefreshCompatibility_Click(sender As Object, e As EventArgs) Handles btnRefreshCompatibility.Click
        Await RefreshCompatibilityAsync(True, False)
    End Sub

    Private Async Function RefreshCompatibilityAsync(reportStatus As Boolean, isAuto As Boolean) As Task
        If reportStatus Then
            SetStatus("Updating lists...")
        End If

        AppendLog(If(isAuto, "Auto-refreshing compatibility list...", "Refreshing compatibility list..."))

        Try
            Dim updateResult As CompatibilityUpdateResult = Await CompatibilityService.UpdateCompatibilityListWithDiffAsync()
            allCompatibilityEntries = If(updateResult?.Entries, New List(Of CompatibilityEntry)())
            compatibilityChangedNames.Clear()

            If updateResult IsNot Nothing Then
                For Each name As String In updateResult.AddedNames
                    compatibilityChangedNames.Add(NameNormalization.NormalizeRelaxedName(name))
                Next
                For Each name As String In updateResult.ChangedNames
                    compatibilityChangedNames.Add(NameNormalization.NormalizeRelaxedName(name))
                Next

                AppendLog($"Compatibility sync: +{updateResult.AddedNames.Count} added, -{updateResult.RemovedNames.Count} removed, ~{updateResult.ChangedNames.Count} changed.")

                If updateResult.AddedNames.Count > 0 Then
                    AppendLog("Added entries: " & String.Join(", ", updateResult.AddedNames.Take(8)))
                End If
                If updateResult.ChangedNames.Count > 0 Then
                    AppendLog("Changed entries: " & String.Join(", ", updateResult.ChangedNames.Take(8)))
                End If
                If updateResult.RemovedNames.Count > 0 Then
                    AppendLog("Removed entries: " & String.Join(", ", updateResult.RemovedNames.Take(8)))
                End If
            End If

            Await RefreshOptiPatcherSupportListAsync(isAuto)
            UpdateCompatibilityNote()
            ApplyCompatibilityFilter()
            If reportStatus Then
                SetStatus("List updated.")
            End If
            AppendLog("List updated.")
        Catch ex As Exception
            AppendLog("Failed to update compatibility list: " & ex.Message)
            If reportStatus Then
                SetStatus("List update failed.")
            End If
            ErrorLogger.Log(ex, "MainForm.RefreshCompatibility")
        End Try
    End Function

    Private Async Sub btnScanDetected_Click(sender As Object, e As EventArgs) Handles btnScanDetected.Click
        Await RunUnifiedDetectionAsync(False)
    End Sub

    Private Async Function RunUnifiedDetectionAsync(isAuto As Boolean,
                                                    Optional isInitialDeepScan As Boolean = False) As Task
        Dim selectedRoots As List(Of String) = PromptForDeepScanRoots("Detection scan")
        If selectedRoots Is Nothing Then
            AppendLog("Drive selection cancelled; running launcher/registry detection only.")
            Await RunDetectionAsync(isAuto, Nothing, isInitialDeepScan)
            Return
        End If

        If selectedRoots.Count > 0 Then
            Await RunDetectionAsync(isAuto, selectedRoots, isInitialDeepScan)
            Return
        End If

        AppendLog("Drive scan roots unavailable; running launcher/registry detection only.")
        Await RunDetectionAsync(isAuto, Nothing, isInitialDeepScan)
    End Function

    Private Function PromptForDeepScanRoots(scanContext As String) As List(Of String)
        Dim availableRoots As List(Of String) = DetectionService.GetScannableDriveRoots(AddressOf AppendLog)
        If availableRoots Is Nothing OrElse availableRoots.Count = 0 Then
            Return New List(Of String)()
        End If

        Using picker As New frmDriveSelection(availableRoots)
            picker.Text = "Select Drives for " & scanContext
            If picker.ShowDialog(Me) <> DialogResult.OK Then
                Return Nothing
            End If

            Dim selectedRoots As List(Of String) = picker.GetSelectedDriveRoots()
            If selectedRoots Is Nothing Then
                Return New List(Of String)()
            End If

            Return selectedRoots
        End Using
    End Function

    Private Async Function AddManualDetectedGameAsync(exePath As String) As Task
        Try
            btnDeepScanDrives.Enabled = False
            btnUseDetected.Enabled = False

            Dim normalizedExe As String = NormalizePathSafe(exePath)
            If String.IsNullOrWhiteSpace(normalizedExe) OrElse Not File.Exists(normalizedExe) Then
                MessageBox.Show(Me, "Select a valid game executable file.", "Add Game Manually", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            AppendLog("Manual add: analyzing " & normalizedExe)
            SetStatus("Manual add: matching selected game...")

            Dim detected As DetectedGame = Await Task.Run(Function() DetectionService.DetectSupportedGameFromExecutable(allCompatibilityEntries,
                                                                                                                           normalizedExe,
                                                                                                                           "Manual"))
            If detected Is Nothing Then
                AppendLog("Manual add failed: selected executable did not match a supported game.")
                MessageBox.Show(Me,
                                "The selected executable did not match any game in the compatibility list." & Environment.NewLine &
                                "Try selecting the main game EXE from the actual binaries folder.",
                                "Add Game Manually",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information)
                SetStatus("Manual add: no supported match.")
                Return
            End If

            EnsurePersistedDeepScanGamesLoaded()

            Dim beforeCount As Integer = detectedGames.Count
            persistedDeepScanGames = MergeDetectedGames(persistedDeepScanGames, New List(Of DetectedGame) From {detected})
            DeepScanDetectionCacheService.Save(persistedDeepScanGames, AddressOf AppendLog)

            detectedGames = MergeDetectedGames(detectedGames, New List(Of DetectedGame) From {detected})
            detectedInstallLookup = Await Task.Run(Function() BuildInstallStatusLookup(detectedGames))
            detectedLookup = BuildDetectedLookup(detectedGames, detectedInstallLookup)
            detectedOptiPatcherLookup = Await Task.Run(Function() BuildOptiPatcherStatusLookup(detectedGames))

            ApplyCompatibilityFilter()
            UpdateExperimentalDetectedGamesList()
            UpdateDetectedStatus()
            UpdateInstallStatus()
            UpdateOptiPatcherStatus()

            Dim isNewEntry As Boolean = detectedGames.Count > beforeCount
            If isNewEntry Then
                AppendLog("Manual add succeeded: " & detected.DisplayName & " (" & detected.InstallDir & ").")
            Else
                AppendLog("Manual add matched existing detected game: " & detected.DisplayName & ". Status refreshed.")
            End If
            SetStatus("Manual add complete.")
        Catch ex As Exception
            AppendLog("Manual add failed: " & ex.Message)
            SetStatus("Manual add failed.")
            ErrorLogger.Log(ex, "MainForm.AddManualDetectedGame")
        Finally
            btnDeepScanDrives.Enabled = True
            UpdateUseDetectedState()
        End Try
    End Function

    Private Sub btnUseDetected_Click(sender As Object, e As EventArgs) Handles btnUseDetected.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            AppendLog("Use selected skipped: no detected game selected.")
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

    Private Sub lvCompatibility_MouseDown(sender As Object, e As MouseEventArgs) Handles lvCompatibility.MouseDown
        If e.Button <> MouseButtons.Right Then
            Return
        End If

        Dim hitItem As ListViewItem = lvCompatibility.GetItemAt(e.X, e.Y)
        If hitItem Is Nothing Then
            Return
        End If

        If Not hitItem.Selected Then
            lvCompatibility.SelectedItems.Clear()
            hitItem.Selected = True
        End If
        hitItem.Focused = True
        UpdateUseDetectedState()
    End Sub

    Private Sub compatContextMenu_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles compatContextMenu.Opening
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing Then
            e.Cancel = True
            Return
        End If

        Dim isDetected As Boolean = row.Detected IsNot Nothing
        Dim hasEntry As Boolean = row.Entry IsNot Nothing
        Dim hasFolder As Boolean = isDetected AndAlso Not String.IsNullOrWhiteSpace(row.Detected.InstallDir) AndAlso Directory.Exists(row.Detected.InstallDir)
        Dim hasOptiScaler As Boolean = row.InstallInfo IsNot Nothing AndAlso row.InstallInfo.IsInstalled
        Dim hasOptiPatcherInstalled As Boolean = row.OptiPatcherInfo IsNot Nothing AndAlso row.OptiPatcherInfo.IsInstalled
        Dim isPatcherSupported As Boolean = isDetected AndAlso ResolveOptiPatcherSupportForGame(row.Detected) IsNot Nothing
        Dim operationBusy As Boolean = installOperationInProgress OrElse uninstallOperationInProgress

        mnuCompatUseDetected.Enabled = isDetected AndAlso Not operationBusy
        mnuCompatOpenFolder.Enabled = hasFolder
        mnuCompatEditIni.Enabled = isDetected AndAlso hasOptiScaler
        mnuCompatInstallUpdate.Enabled = isDetected AndAlso Not operationBusy
        mnuCompatUninstall.Enabled = isDetected AndAlso hasOptiScaler AndAlso Not operationBusy
        mnuCompatInstallPatcher.Enabled = isDetected AndAlso isPatcherSupported AndAlso Not operationBusy
        mnuCompatRemovePatcher.Enabled = isDetected AndAlso hasOptiPatcherInstalled AndAlso Not operationBusy
        mnuCompatOpenWiki.Enabled = hasEntry
        mnuCompatCopyInfo.Enabled = True

        mnuCompatInstallUpdate.Text = If(hasOptiScaler, "Quick update OptiScaler", "Quick install OptiScaler")
        mnuCompatInstallPatcher.Text = If(hasOptiPatcherInstalled, "Update OptiPatcher", "Install OptiPatcher")
    End Sub

    Private Sub mnuCompatUseDetected_Click(sender As Object, e As EventArgs) Handles mnuCompatUseDetected.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            Return
        End If

        UseDetectedGame(row.Detected)
    End Sub

    Private Sub btnCompatOpenFolder_Click(sender As Object, e As EventArgs) Handles btnCompatOpenFolder.Click
        mnuCompatOpenFolder_Click(sender, e)
    End Sub

    Private Async Sub btnCompatEditIni_Click(sender As Object, e As EventArgs) Handles btnCompatEditIni.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            Return
        End If
        If row.InstallInfo Is Nothing OrElse Not row.InstallInfo.IsInstalled Then
            Return
        End If

        UseDetectedGame(row.Detected)
        Await OpenIniEditorForCurrentTargetAsync()
    End Sub

    Private Async Sub btnCompatInstallUpdate_Click(sender As Object, e As EventArgs) Handles btnCompatInstallUpdate.Click
        Await QuickInstallFromSelectedRowAsync()
    End Sub

    Private Async Sub btnCompatUninstall_Click(sender As Object, e As EventArgs) Handles btnCompatUninstall.Click
        Await UninstallFromSelectedRowAsync()
    End Sub

    Private Sub btnCompatInstallPatcher_Click(sender As Object, e As EventArgs) Handles btnCompatInstallPatcher.Click
        mnuCompatInstallPatcher_Click(sender, e)
    End Sub

    Private Sub btnCompatRemovePatcher_Click(sender As Object, e As EventArgs) Handles btnCompatRemovePatcher.Click
        mnuCompatRemovePatcher_Click(sender, e)
    End Sub

    Private Sub btnCompatCopyInfo_Click(sender As Object, e As EventArgs) Handles btnCompatCopyInfo.Click
        mnuCompatCopyInfo_Click(sender, e)
    End Sub

    Private Sub mnuCompatOpenFolder_Click(sender As Object, e As EventArgs) Handles mnuCompatOpenFolder.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            Return
        End If

        Dim folder As String = row.Detected.InstallDir
        If String.IsNullOrWhiteSpace(folder) OrElse Not Directory.Exists(folder) Then
            MessageBox.Show(Me, "Detected install folder was not found.", "Game Detection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Process.Start(New ProcessStartInfo(folder) With {.UseShellExecute = True})
    End Sub

    Private Async Sub mnuCompatEditIni_Click(sender As Object, e As EventArgs) Handles mnuCompatEditIni.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            Return
        End If
        If row.InstallInfo Is Nothing OrElse Not row.InstallInfo.IsInstalled Then
            Return
        End If

        UseDetectedGame(row.Detected)
        Await OpenIniEditorForCurrentTargetAsync()
    End Sub

    Private Async Sub mnuCompatInstallUpdate_Click(sender As Object, e As EventArgs) Handles mnuCompatInstallUpdate.Click
        Await QuickInstallFromSelectedRowAsync()
    End Sub

    Private Async Function QuickInstallFromSelectedRowAsync() As Task
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            Return
        End If

        If installOperationInProgress OrElse uninstallOperationInProgress Then
            AppendLog("Quick install skipped: another install/uninstall operation is currently running.")
            Return
        End If

        If Not TrySetDetectedGameTarget(row.Detected,
                                        promptForExecutable:=True,
                                        switchToInstallTab:=False,
                                        applyDefaultsFromSettings:=True,
                                        applyTemplate:=False,
                                        requireExecutable:=True,
                                        actionPrefix:="Quick install target: ") Then
            Return
        End If

        Dim gameExePath As String = txtGameExe.Text.Trim()
        Dim config As InstallerConfig = BuildQuickInstallConfig(gameExePath)

        Dim existingInfo As OptiScalerInstallInfo = OptiScalerInstallDetector.Detect(config.GameFolder)
        Dim action As InstallAction = If(existingInfo IsNot Nothing AndAlso existingInfo.IsInstalled, InstallAction.Update, InstallAction.Install)

        Try
            installOperationInProgress = True
            UpdateUseDetectedState()
            UpdateInstallActionButtons(existingInfo)
            UpdateProgress(0)

            AppendLog("Starting quick install...")
            If action = InstallAction.Update Then
                AppendLog("Quick install detected existing OptiScaler install. Proceeding with update.")
            End If

            If Not RunInstallPreflight(config) Then
                AppendLog("Quick install aborted (preflight).")
                Return
            End If

            Dim manifest As InstallManifest = Await InstallerService.InstallAsync(config, AddressOf AppendLog, AddressOf UpdateProgress)
            Dim verification As InstallVerificationReport = Await Task.Run(Function() InstallerService.VerifyInstall(config, manifest))
            If manifest IsNot Nothing Then
                manifest.VerificationTimeUtc = DateTime.UtcNow
            End If

            LogVerificationReport(verification)

            If verification IsNot Nothing AndAlso verification.Errors.Count > 0 Then
                MessageBox.Show(Me,
                                "Quick install finished with verification errors." & Environment.NewLine &
                                "Open diagnostics/log output for details.",
                                "Quick Install Completed With Issues",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
            Else
                Dim warningCount As Integer = If(verification Is Nothing, 0, verification.Warnings.Count)
                Dim message As String = "Quick install completed successfully."
                If warningCount > 0 Then
                    message &= Environment.NewLine & warningCount.ToString() & " verification warning(s) were reported."
                End If
                MessageBox.Show(Me, message, "Quick Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            AppendLog("Quick install completed.")
            Await RefreshDetectedInstallStatesAsync(False)
        Catch ex As Exception
            AppendLog("Quick install failed: " & ex.Message)
            MessageBox.Show(Me, ex.Message, "Quick Install Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.Log(ex, "MainForm.QuickInstall")
        Finally
            installOperationInProgress = False
            UpdateProgress(0)
            UpdateInstallStatus()
            UpdateExperimentalStatus()
            UpdateUseDetectedState()
        End Try
    End Function

    Private Async Sub mnuCompatUninstall_Click(sender As Object, e As EventArgs) Handles mnuCompatUninstall.Click
        Await UninstallFromSelectedRowAsync()
    End Sub

    Private Async Function UninstallFromSelectedRowAsync() As Task
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            Return
        End If
        If row.InstallInfo Is Nothing OrElse Not row.InstallInfo.IsInstalled Then
            Return
        End If
        If uninstallOperationInProgress OrElse installOperationInProgress Then
            AppendLog("Uninstall skipped: another install/uninstall operation is currently running.")
            Return
        End If

        Dim preferredUninstallDir As String = If(row.InstallInfo Is Nothing, row.Detected.InstallDir, row.InstallInfo.InstallFolder)
        If String.IsNullOrWhiteSpace(preferredUninstallDir) Then
            preferredUninstallDir = row.Detected.InstallDir
        End If

        Dim installDir As String = NormalizePathSafe(preferredUninstallDir)
        If String.IsNullOrWhiteSpace(installDir) OrElse Not Directory.Exists(installDir) Then
            MessageBox.Show(Me, "Detected install folder was not found.", "Uninstall", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim confirmation As DialogResult = MessageBox.Show(Me,
                                                           "Uninstall OptiScaler from the selected detected game?" & Environment.NewLine &
                                                           If(row.Entry?.Name, row.Detected.DisplayName),
                                                           "Confirm uninstall",
                                                           MessageBoxButtons.YesNo,
                                                           MessageBoxIcon.Question)
        If confirmation <> DialogResult.Yes Then
            AppendLog("Uninstall canceled.")
            Return
        End If

        txtGameFolder.Text = installDir
        Dim executablePath As String = ResolveDetectedGameExecutable(row.Detected, False)
        If Not String.IsNullOrWhiteSpace(executablePath) Then
            txtGameExe.Text = executablePath
        End If

        Try
            uninstallOperationInProgress = True
            UpdateInstallActionButtons(row.InstallInfo)
            UpdateUseDetectedState()
            AppendLog("Starting uninstall...")
            Await TryUninstallAsync(installDir, True)
            Await RefreshDetectedInstallStatesAsync(False)
        Catch ex As Exception
            AppendLog("Uninstall failed: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.UninstallFromSelectedRow")
            MessageBox.Show(Me, ex.Message, "Uninstall Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            uninstallOperationInProgress = False
            UpdateInstallStatus()
            UpdateExperimentalStatus()
            UpdateUseDetectedState()
        End Try
    End Function

    Private Sub mnuCompatInstallPatcher_Click(sender As Object, e As EventArgs) Handles mnuCompatInstallPatcher.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            Return
        End If

        UseDetectedGame(row.Detected)
        If btnInstallOptiPatcher IsNot Nothing AndAlso btnInstallOptiPatcher.Enabled Then
            btnInstallOptiPatcher.PerformClick()
        Else
            MessageBox.Show(Me, "OptiPatcher is not available for this selected game.", "OptiPatcher", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub mnuCompatRemovePatcher_Click(sender As Object, e As EventArgs) Handles mnuCompatRemovePatcher.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Detected Is Nothing Then
            Return
        End If

        UseDetectedGame(row.Detected)
        If btnRemoveOptiPatcher IsNot Nothing AndAlso btnRemoveOptiPatcher.Enabled Then
            btnRemoveOptiPatcher.PerformClick()
        End If
    End Sub

    Private Sub mnuCompatOpenWiki_Click(sender As Object, e As EventArgs) Handles mnuCompatOpenWiki.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing OrElse row.Entry Is Nothing Then
            Return
        End If

        OpenCompatibilityWiki(row.Entry)
    End Sub

    Private Sub mnuCompatCopyInfo_Click(sender As Object, e As EventArgs) Handles mnuCompatCopyInfo.Click
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        If row Is Nothing Then
            Return
        End If

        Dim installState As String = GetInstallStatusText(row.Detected IsNot Nothing, row.InstallInfo)
        Dim patcherState As String = GetOptiPatcherStatusText(row.Entry, row.Detected IsNot Nothing, row.OptiPatcherInfo)
        Dim detectedState As String = If(row.Detected Is Nothing, "No", "Yes")
        Dim platform As String = If(row.Detected Is Nothing, "", row.Detected.Platform)
        Dim antiCheat As String = If(row.Detected Is Nothing, "", GetAntiCheatStatusText(row.Detected))
        Dim pathValue As String = If(row.Detected Is Nothing, "", row.Detected.InstallDir)
        Dim infoLines As String() = {
            "Game: " & If(row.Entry?.Name, ""),
            "Detected: " & detectedState,
            "OptiScaler: " & installState,
            "OptiPatcher: " & patcherState,
            "Platform: " & platform,
            "Anti-cheat: " & antiCheat,
            "Install path: " & pathValue
        }

        Try
            Clipboard.SetText(String.Join(Environment.NewLine, infoLines))
            AppendLog("Copied game info to clipboard: " & If(row.Entry?.Name, "(unknown)"))
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.mnuCompatCopyInfo")
            MessageBox.Show(Me, "Failed to copy game info: " & ex.Message, "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UpdateUseDetectedState()
        Dim row As CompatibilityRow = GetSelectedCompatibilityRow()
        Dim isDetected As Boolean = row IsNot Nothing AndAlso row.Detected IsNot Nothing
        Dim hasEntry As Boolean = row IsNot Nothing AndAlso row.Entry IsNot Nothing
        Dim hasFolder As Boolean = isDetected AndAlso Not String.IsNullOrWhiteSpace(row.Detected.InstallDir) AndAlso Directory.Exists(row.Detected.InstallDir)
        Dim hasOptiScaler As Boolean = row IsNot Nothing AndAlso row.InstallInfo IsNot Nothing AndAlso row.InstallInfo.IsInstalled
        Dim hasOptiPatcherInstalled As Boolean = row IsNot Nothing AndAlso row.OptiPatcherInfo IsNot Nothing AndAlso row.OptiPatcherInfo.IsInstalled
        Dim isPatcherSupported As Boolean = isDetected AndAlso ResolveOptiPatcherSupportForGame(row.Detected) IsNot Nothing
        Dim operationBusy As Boolean = installOperationInProgress OrElse uninstallOperationInProgress

        btnUseDetected.Enabled = isDetected AndAlso Not operationBusy
        btnOpenWiki.Enabled = hasEntry
        btnCompatOpenFolder.Enabled = hasFolder
        btnCompatEditIni.Enabled = isDetected AndAlso hasOptiScaler
        btnCompatInstallUpdate.Enabled = isDetected AndAlso Not operationBusy
        btnCompatUninstall.Enabled = isDetected AndAlso hasOptiScaler AndAlso Not operationBusy
        btnCompatInstallPatcher.Enabled = isDetected AndAlso isPatcherSupported AndAlso Not operationBusy
        btnCompatRemovePatcher.Enabled = isDetected AndAlso hasOptiPatcherInstalled AndAlso Not operationBusy
        btnCompatCopyInfo.Enabled = row IsNot Nothing

        btnCompatInstallUpdate.Text = If(hasOptiScaler, "Quick update", "Quick install")
        btnCompatInstallPatcher.Text = If(hasOptiPatcherInstalled, "Update OptiPatcher", "Install OptiPatcher")
    End Sub

    Private Function GetSelectedCompatibilityRow() As CompatibilityRow
        If lvCompatibility.SelectedItems.Count = 0 Then
            Return Nothing
        End If

        Return TryCast(lvCompatibility.SelectedItems(0).Tag, CompatibilityRow)
    End Function

    Private Sub EnsurePersistedDeepScanGamesLoaded()
        If persistedDeepScanLoaded Then
            Return
        End If

        persistedDeepScanGames = MergeDetectedGames(DeepScanDetectionCacheService.Load(AddressOf AppendLog), Nothing)
        persistedDeepScanLoaded = True
    End Sub

    Private Function MergeDetectedGames(primary As IEnumerable(Of DetectedGame),
                                        secondary As IEnumerable(Of DetectedGame)) As List(Of DetectedGame)
        Dim merged As New List(Of DetectedGame)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        AddDetectedGamesToList(merged, seen, primary)
        AddDetectedGamesToList(merged, seen, secondary)

        merged.Sort(Function(left, right) StringComparer.OrdinalIgnoreCase.Compare(If(left?.DisplayName, ""), If(right?.DisplayName, "")))
        Return merged
    End Function

    Private Sub AddDetectedGamesToList(target As List(Of DetectedGame),
                                       seen As HashSet(Of String),
                                       source As IEnumerable(Of DetectedGame))
        If target Is Nothing OrElse seen Is Nothing OrElse source Is Nothing Then
            Return
        End If

        For Each game As DetectedGame In source
            If game Is Nothing OrElse String.IsNullOrWhiteSpace(game.DisplayName) OrElse String.IsNullOrWhiteSpace(game.InstallDir) Then
                Continue For
            End If

            Dim installDir As String = NormalizePathSafe(game.InstallDir)
            If String.IsNullOrWhiteSpace(installDir) OrElse Not Directory.Exists(installDir) Then
                Continue For
            End If

            Dim key As String = NameNormalization.NormalizeRelaxedName(game.DisplayName) & "|" & installDir
            If Not seen.Add(key) Then
                Continue For
            End If

            target.Add(New DetectedGame With {
                .DisplayName = game.DisplayName,
                .Platform = If(String.IsNullOrWhiteSpace(game.Platform), "Drive scan", game.Platform),
                .InstallDir = installDir,
                .MatchedEntry = game.MatchedEntry,
                .SourceName = If(String.IsNullOrWhiteSpace(game.SourceName), game.DisplayName, game.SourceName),
                .AntiCheat = If(game.AntiCheat, "")
            })
        Next
    End Sub

    Private Function BuildDetectedLookup(results As IEnumerable(Of DetectedGame),
                                         Optional installLookup As IDictionary(Of String, OptiScalerInstallInfo) = Nothing) As Dictionary(Of String, DetectedGame)
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
            ElseIf IsPreferredDetectedGameCandidate(game, map(key), installLookup) Then
                map(key) = game
            End If
        Next
        Return map
    End Function

    Private Function IsPreferredDetectedGameCandidate(candidate As DetectedGame,
                                                      current As DetectedGame,
                                                      installLookup As IDictionary(Of String, OptiScalerInstallInfo)) As Boolean
        Dim candidateScore As Integer = GetDetectedGameCandidateScore(candidate, installLookup)
        Dim currentScore As Integer = GetDetectedGameCandidateScore(current, installLookup)
        If candidateScore <> currentScore Then
            Return candidateScore > currentScore
        End If

        Dim candidatePath As String = NormalizePathSafe(If(candidate?.InstallDir, ""))
        Dim currentPath As String = NormalizePathSafe(If(current?.InstallDir, ""))
        If candidatePath.Length <> currentPath.Length Then
            Return candidatePath.Length < currentPath.Length
        End If

        Return StringComparer.OrdinalIgnoreCase.Compare(candidatePath, currentPath) < 0
    End Function

    Private Function GetDetectedGameCandidateScore(game As DetectedGame,
                                                   installLookup As IDictionary(Of String, OptiScalerInstallInfo)) As Integer
        If game Is Nothing Then
            Return Integer.MinValue
        End If

        Dim score As Integer = 0

        Dim lookupKey As String = GetDetectedInstallLookupKey(game)
        If installLookup IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(lookupKey) Then
            Dim installInfo As OptiScalerInstallInfo = Nothing
            If installLookup.TryGetValue(lookupKey, installInfo) AndAlso installInfo IsNot Nothing AndAlso installInfo.IsInstalled Then
                score += 10000
            End If
        End If

        Dim platformValue As String = If(game.Platform, "").Trim().ToLowerInvariant()
        Select Case platformValue
            Case "steam", "epic", "gog", "ea app", "ubisoft", "battle.net"
                score += 600
            Case "registry"
                score += 500
            Case "drive scan"
                score += 300
            Case Else
                score += 350
        End Select

        Dim pathValue As String = NormalizePathSafe(If(game.InstallDir, "")).ToLowerInvariant()
        If pathValue.Contains("\backup\") OrElse pathValue.Contains("\backups\") Then
            score -= 120
        End If
        If pathValue.Contains("\documents\") Then
            score -= 40
        End If
        If pathValue.Contains("\bin\x64_dx12") Then
            score += 40
        ElseIf pathValue.Contains("\x64_dx12") Then
            score += 25
        End If

        Return score
    End Function

    Private Function BuildInstallStatusLookup(results As IEnumerable(Of DetectedGame)) As Dictionary(Of String, OptiScalerInstallInfo)
        Dim map As New Dictionary(Of String, OptiScalerInstallInfo)(StringComparer.OrdinalIgnoreCase)
        For Each game As DetectedGame In results
            If game Is Nothing Then
                Continue For
            End If

            Dim key As String = GetDetectedInstallLookupKey(game)
            If String.IsNullOrWhiteSpace(key) Then
                Continue For
            End If

            If Not map.ContainsKey(key) Then
                map(key) = OptiScalerInstallDetector.Detect(game.InstallDir)
            End If
        Next

        Return map
    End Function

    Private Function BuildOptiPatcherStatusLookup(results As IEnumerable(Of DetectedGame)) As Dictionary(Of String, OptiPatcherInstallInfo)
        Dim map As New Dictionary(Of String, OptiPatcherInstallInfo)(StringComparer.OrdinalIgnoreCase)
        For Each game As DetectedGame In results
            If game Is Nothing Then
                Continue For
            End If

            Dim key As String = GetDetectedInstallLookupKey(game)
            If String.IsNullOrWhiteSpace(key) Then
                Continue For
            End If

            If Not map.ContainsKey(key) Then
                map(key) = OptiPatcherInstallDetector.Detect(game.InstallDir)
            End If
        Next

        Return map
    End Function

    Private Function GetDetectedInstallLookupKey(game As DetectedGame) As String
        If game Is Nothing Then
            Return ""
        End If

        Dim nameKey As String = NameNormalization.NormalizeRelaxedName(game.DisplayName)
        If String.IsNullOrWhiteSpace(nameKey) Then
            Return ""
        End If

        Dim pathKey As String = NormalizePathSafe(game.InstallDir)
        Return nameKey & "|" & pathKey
    End Function

    Private Async Function RefreshDetectedInstallStatesAsync(Optional logAction As Boolean = True) As Task
        If detectedGames Is Nothing OrElse detectedGames.Count = 0 Then
            ApplyCompatibilityFilter()
            UpdateDetectedStatus()
            UpdateExperimentalDetectedGamesList()
            Return
        End If

        detectedInstallLookup = Await Task.Run(Function() BuildInstallStatusLookup(detectedGames))
        detectedOptiPatcherLookup = Await Task.Run(Function() BuildOptiPatcherStatusLookup(detectedGames))

        ApplyCompatibilityFilter()
        UpdateDetectedStatus()
        UpdateExperimentalDetectedGamesList()
        UpdateInstallStatus()
        UpdateOptiPatcherStatus()

        If logAction Then
            AppendLog("Detected install states refreshed.")
        End If
    End Function

    Private Sub UpdateDetectedStatus()
        If detectedLookup Is Nothing OrElse detectedLookup.Count = 0 Then
            toolDetectedLabel.Text = "Detected: none"
        Else
            toolDetectedLabel.Text = "Detected: " & detectedLookup.Count
        End If
    End Sub

    Private Sub UpdateExperimentalDetectedGamesList()
        If lvFsr4DetectedGames Is Nothing Then
            Return
        End If

        lvFsr4DetectedGames.BeginUpdate()
        lvFsr4DetectedGames.Items.Clear()
        Dim canonicalByName As Dictionary(Of String, DetectedGame) = Nothing
        If detectedLookup IsNot Nothing AndAlso detectedLookup.Count > 0 Then
            canonicalByName = New Dictionary(Of String, DetectedGame)(detectedLookup, StringComparer.OrdinalIgnoreCase)
        ElseIf detectedGames IsNot Nothing AndAlso detectedGames.Count > 0 Then
            canonicalByName = BuildDetectedLookup(detectedGames, detectedInstallLookup)
        Else
            canonicalByName = New Dictionary(Of String, DetectedGame)(StringComparer.OrdinalIgnoreCase)
        End If

        For Each game As DetectedGame In canonicalByName.Values.OrderBy(Function(entry) entry.DisplayName, StringComparer.OrdinalIgnoreCase)
            If game Is Nothing Then
                Continue For
            End If

            Dim normalizedPath As String = NormalizePathSafe(game.InstallDir)
            If String.IsNullOrWhiteSpace(normalizedPath) Then
                Continue For
            End If

            Dim fsr4Status As ExperimentalFsr4Status = ExperimentalFsr4Service.Detect(normalizedPath)
            Dim item As New ListViewItem(game.DisplayName)
            item.SubItems.Add(If(game.Platform, ""))
            item.SubItems.Add(GetExperimentalListStatusText(fsr4Status))
            item.SubItems.Add(normalizedPath)
            item.Tag = game
            lvFsr4DetectedGames.Items.Add(item)
        Next

        lvFsr4DetectedGames.EndUpdate()

        Dim selectedTarget As String = txtGameFolder.Text.Trim()
        If Not String.IsNullOrWhiteSpace(selectedTarget) Then
            For Each item As ListViewItem In lvFsr4DetectedGames.Items
                Dim game As DetectedGame = TryCast(item.Tag, DetectedGame)
                If game Is Nothing Then
                    Continue For
                End If

                If String.Equals(NormalizePathSafe(game.InstallDir), NormalizePathSafe(selectedTarget), StringComparison.OrdinalIgnoreCase) Then
                    item.Selected = True
                    item.Focused = True
                    Exit For
                End If
            Next
        End If

        Dim count As Integer = lvFsr4DetectedGames.Items.Count
        If count <= 0 Then
            lblFsr4DetectedGames.Text = "Detected supported games (none found, use Scan now or Browse game EXE)"
        Else
            lblFsr4DetectedGames.Text = "Detected supported games (" & count & ")"
        End If

        btnFsr4UseSelectedGame.Enabled = GetSelectedExperimentalDetectedGame() IsNot Nothing
    End Sub

    Private Sub UpdateCompatibilityNote()
        If lblCompatibilityNote Is Nothing Then
            Return
        End If

        Dim changedCount As Integer = If(compatibilityChangedNames Is Nothing, 0, compatibilityChangedNames.Count)
        If changedCount <= 0 Then
            lblCompatibilityNote.Text = compatibilityBaseNoteText
            Return
        End If

        lblCompatibilityNote.Text = $"{compatibilityBaseNoteText} Recently changed entries: {changedCount}."
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

    Private Function GetOptiPatcherStatusText(entry As CompatibilityEntry, isDetected As Boolean, info As OptiPatcherInstallInfo) As String
        If Not isDetected Then
            Return ""
        End If

        If info Is Nothing OrElse Not info.IsInstalled Then
            Return "No"
        End If

        If String.IsNullOrWhiteSpace(info.Version) Then
            Return "Yes"
        End If

        Return "Yes (" & info.Version & ")"
    End Function

    Private Function GetAntiCheatStatusText(game As DetectedGame) As String
        If game Is Nothing Then
            Return ""
        End If

        If String.IsNullOrWhiteSpace(game.AntiCheat) Then
            Return "No"
        End If

        Return game.AntiCheat
    End Function

    Private Sub ApplyInstallRowColors(item As ListViewItem,
                                      info As OptiScalerInstallInfo,
                                      isDetected As Boolean,
                                      rowIndex As Integer,
                                      isRecentlyChanged As Boolean,
                                      antiCheatDetected As Boolean)
        If item Is Nothing Then
            Return
        End If

        Dim baseColor As Color = If(rowIndex Mod 2 = 0, lvCompatibility.RowBackColor, lvCompatibility.RowAltBackColor)
        Dim mode As SystemColorMode = ThemeSettings.GetPreferredColorMode()
        Dim tintAlpha As Integer = If(mode = SystemColorMode.Dark, 60, 35)

        If isRecentlyChanged Then
            Dim changedTint As Color = Color.FromArgb(65, 95, 150)
            baseColor = BlendColors(baseColor, changedTint, If(mode = SystemColorMode.Dark, 55, 40))
        End If

        If antiCheatDetected Then
            Dim antiCheatTint As Color = Color.FromArgb(170, 120, 40)
            baseColor = BlendColors(baseColor, antiCheatTint, If(mode = SystemColorMode.Dark, 70, 50))
        End If

        item.BackColor = baseColor

        If Not isDetected Then
            Return
        End If

        If info Is Nothing OrElse Not info.IsInstalled Then
            Dim missingTint As Color = Color.FromArgb(160, 70, 70)
            item.BackColor = BlendColors(item.BackColor, missingTint, tintAlpha)
            Return
        End If

        Dim installedTint As Color = Color.FromArgb(70, 140, 90)
        item.BackColor = BlendColors(item.BackColor, installedTint, tintAlpha)
    End Sub

    Private Function BlendColors(baseColor As Color, overlay As Color, alpha As Integer) As Color
        ' Defensive blend implementation to avoid overflow on any runtime/theme edge case.
        Dim clamped As Integer
        If alpha <= 0 Then
            Return baseColor
        ElseIf alpha >= 255 Then
            Return Color.FromArgb(baseColor.A, overlay.R, overlay.G, overlay.B)
        Else
            clamped = alpha
        End If

        Dim inverse As Integer = 255 - clamped
        Dim r As Integer = ClampColorChannel(CInt(Math.Round((CDbl(baseColor.R) * inverse + CDbl(overlay.R) * clamped) / 255.0R)))
        Dim g As Integer = ClampColorChannel(CInt(Math.Round((CDbl(baseColor.G) * inverse + CDbl(overlay.G) * clamped) / 255.0R)))
        Dim b As Integer = ClampColorChannel(CInt(Math.Round((CDbl(baseColor.B) * inverse + CDbl(overlay.B) * clamped) / 255.0R)))

        Return Color.FromArgb(baseColor.A, r, g, b)
    End Function

    Private Function ClampColorChannel(value As Integer) As Integer
        If value < 0 Then
            Return 0
        End If
        If value > 255 Then
            Return 255
        End If
        Return value
    End Function

    Private Sub UseDetectedGame(game As DetectedGame)
        TrySetDetectedGameTarget(game,
                                 promptForExecutable:=True,
                                 switchToInstallTab:=True,
                                 applyDefaultsFromSettings:=False,
                                 applyTemplate:=True,
                                 requireExecutable:=False,
                                 actionPrefix:="Using detected game: ")
    End Sub

    Private Function TrySetDetectedGameTarget(game As DetectedGame,
                                              promptForExecutable As Boolean,
                                              switchToInstallTab As Boolean,
                                              applyDefaultsFromSettings As Boolean,
                                              applyTemplate As Boolean,
                                              requireExecutable As Boolean,
                                              actionPrefix As String) As Boolean
        If game Is Nothing Then
            Return False
        End If

        Dim installDir As String = NormalizePathSafe(game.InstallDir)
        If String.IsNullOrWhiteSpace(installDir) OrElse Not Directory.Exists(installDir) Then
            MessageBox.Show(Me, "Detected install folder was not found.", "Game Detection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If

        If Not String.IsNullOrWhiteSpace(actionPrefix) Then
            AppendLog(actionPrefix & game.DisplayName)
        End If

        txtGameExe.Text = ""
        txtGameFolder.Text = installDir
        UpdateEngineWarningByFolder(installDir)

        Dim executablePath As String = ResolveDetectedGameExecutable(game, promptForExecutable)
        If Not String.IsNullOrWhiteSpace(executablePath) Then
            txtGameExe.Text = executablePath
        ElseIf requireExecutable Then
            MessageBox.Show(Me,
                            "No executable was found for this detected game. Select the game EXE manually and try again.",
                            "Game Detection",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)
            Return False
        End If

        If applyDefaultsFromSettings Then
            ApplyDefaultInstallOptionsFromSettings(AppSettings.Load(), False)
        Else
            ApplyDefaultInstallOptionsFromUi(False)
        End If

        If applyTemplate Then
            ApplyGameTemplate(game)
        End If

        If switchToInstallTab Then
            tabMain.SelectedTab = tabInstall
        End If

        UpdateInstallStatus()
        UpdateUseDetectedState()
        Return True
    End Function

    Private Sub TryAutoRetargetUnrealInstall(config As InstallerConfig)
        If config Is Nothing Then
            Return
        End If

        Dim normalizedFolder As String = NormalizePathSafe(config.GameFolder)
        If String.IsNullOrWhiteSpace(normalizedFolder) OrElse Not Directory.Exists(normalizedFolder) Then
            Return
        End If

        If Not Directory.Exists(Path.Combine(normalizedFolder, "Engine")) Then
            Return
        End If

        Dim currentExe As String = NormalizePathSafe(config.GameExePath)
        If String.IsNullOrWhiteSpace(currentExe) OrElse Not File.Exists(currentExe) Then
            Return
        End If

        Dim currentLower As String = currentExe.ToLowerInvariant()
        If currentLower.Contains("\binaries\win64\") OrElse currentLower.Contains("\binaries\wingdk\") Then
            Return
        End If

        Dim currentName As String = Path.GetFileNameWithoutExtension(currentExe)
        Dim retargetedExe As String = FindPreferredExecutable(normalizedFolder, currentName, currentName)
        If String.IsNullOrWhiteSpace(retargetedExe) OrElse Not File.Exists(retargetedExe) Then
            Return
        End If

        Dim retargetedLower As String = retargetedExe.ToLowerInvariant()
        If Not retargetedLower.Contains("\binaries\win64\") AndAlso Not retargetedLower.Contains("\binaries\wingdk\") Then
            Return
        End If

        Dim retargetedFolder As String = NormalizePathSafe(Path.GetDirectoryName(retargetedExe))
        If String.IsNullOrWhiteSpace(retargetedFolder) OrElse Not Directory.Exists(retargetedFolder) Then
            Return
        End If

        If String.Equals(currentExe, retargetedExe, StringComparison.OrdinalIgnoreCase) Then
            Return
        End If

        config.GameExePath = retargetedExe
        config.GameFolder = retargetedFolder

        If txtGameExe IsNot Nothing Then
            txtGameExe.Text = retargetedExe
        End If
        If txtGameFolder IsNot Nothing Then
            txtGameFolder.Text = retargetedFolder
        End If

        AppendLog("Preflight auto-adjusted Unreal target to binaries folder: " & retargetedFolder)
    End Sub

    Private Function ResolveDetectedGameExecutable(game As DetectedGame, promptForExecutable As Boolean) As String
        If game Is Nothing Then
            Return ""
        End If

        Dim installDir As String = NormalizePathSafe(game.InstallDir)
        If String.IsNullOrWhiteSpace(installDir) OrElse Not Directory.Exists(installDir) Then
            Return ""
        End If

        Dim executablePath As String = FindPreferredExecutable(installDir, game.DisplayName, game.SourceName)
        If Not String.IsNullOrWhiteSpace(executablePath) Then
            Return executablePath
        End If

        If Not promptForExecutable Then
            Return ""
        End If

        Using dialog As New OpenFileDialog()
            dialog.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*"
            dialog.Title = "Select Game Executable"
            dialog.InitialDirectory = installDir

            If dialog.ShowDialog(Me) = DialogResult.OK Then
                Return dialog.FileName
            End If
        End Using

        Return ""
    End Function

    Private Sub ApplyGameTemplate(game As DetectedGame)
        If game Is Nothing Then
            Return
        End If

        Dim settings As AppSettingsModel = AppSettings.Load()
        If settings IsNot Nothing AndAlso settings.EnableGameTemplates.HasValue AndAlso Not settings.EnableGameTemplates.Value Then
            Return
        End If

        Dim template As GameWorkaroundTemplate = GameTemplateService.FindTemplate(game.DisplayName)
        If template Is Nothing Then
            Return
        End If

        Dim appliedParts As New List(Of String)()

        If Not String.IsNullOrWhiteSpace(template.HookName) Then
            Dim hookIndex As Integer = GetHookIndex(cmbHookName, template.HookName)
            If hookIndex >= 0 Then
                cmbHookName.SelectedIndex = hookIndex
                appliedParts.Add("hook=" & template.HookName)
            End If
        End If

        If Not String.IsNullOrWhiteSpace(template.GpuVendor) Then
            Select Case GetDefaultGpuVendorIndex(template.GpuVendor)
                Case 1
                    rbGpuNvidia.Checked = True
                    appliedParts.Add("gpu=NVIDIA")
                Case 2
                    rbGpuAmdIntel.Checked = True
                    appliedParts.Add("gpu=AMD/Intel")
            End Select
        End If

        If template.DlssInputs.HasValue Then
            chkDlssInputs.Checked = template.DlssInputs.Value
            appliedParts.Add("dlssInputs=" & If(template.DlssInputs.Value, "on", "off"))
        End If

        If Not String.IsNullOrWhiteSpace(template.FrameGeneration) Then
            cmbFgType.SelectedIndex = GetDefaultFrameGenerationIndex(template.FrameGeneration)
            appliedParts.Add("fg=" & template.FrameGeneration)
        End If

        If Not String.IsNullOrWhiteSpace(template.ConflictMode) Then
            cmbConflictMode.SelectedIndex = GetDefaultConflictModeIndex(template.ConflictMode)
            appliedParts.Add("conflict=" & template.ConflictMode)
        End If

        If appliedParts.Count > 0 Then
            AppendLog("Applied game template: " & template.Name & " (" & String.Join(", ", appliedParts) & ")")
        Else
            AppendLog("Matched game template: " & template.Name)
        End If

        If Not String.IsNullOrWhiteSpace(template.Notes) Then
            AppendLog("Template note: " & template.Notes)
        End If
    End Sub

    Private Sub UpdateEngineWarningByFolder(folder As String)
        If String.IsNullOrWhiteSpace(folder) Then
            lblEngineWarning.Visible = False
            Return
        End If

        Dim enginePath As String = Path.Combine(folder, "Engine")
        lblEngineWarning.Visible = Directory.Exists(enginePath)
    End Sub

    Private Function EnsureIniFileForEditor(iniPath As String) As Boolean
        If String.IsNullOrWhiteSpace(iniPath) Then
            Return False
        End If

        If File.Exists(iniPath) Then
            Return True
        End If

        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim defaultTemplatePath As String = If(settings Is Nothing, "", settings.DefaultIniPath)
        Dim hasTemplate As Boolean = Not String.IsNullOrWhiteSpace(defaultTemplatePath) AndAlso File.Exists(defaultTemplatePath)

        Dim message As String = "OptiScaler.ini was not found in this folder." & Environment.NewLine &
                                "Choose Yes to create from default template, No to create a basic template, or Cancel."
        If Not hasTemplate Then
            message = "OptiScaler.ini was not found and no default template file is configured." & Environment.NewLine &
                      "Choose Yes or No to create a basic template, or Cancel."
        End If

        Dim choice As DialogResult = MessageBox.Show(Me,
                                                     message,
                                                     "Create OptiScaler.ini",
                                                     MessageBoxButtons.YesNoCancel,
                                                     MessageBoxIcon.Question)
        If choice = DialogResult.Cancel Then
            Return False
        End If

        Try
            Directory.CreateDirectory(Path.GetDirectoryName(iniPath))
            If hasTemplate AndAlso choice = DialogResult.Yes Then
                File.Copy(defaultTemplatePath, iniPath, True)
                AppendLog("Created OptiScaler.ini from default template: " & defaultTemplatePath)
            Else
                Dim content As String = OptiScalerIniEditorService.BuildDefaultIniContent()
                OptiScalerIniEditorService.SaveTextAtomically(iniPath, content)
                AppendLog("Created OptiScaler.ini with basic template.")
            End If

            Return True
        Catch ex As Exception
            AppendLog("Failed to create OptiScaler.ini: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.EnsureIniFileForEditor")
            MessageBox.Show(Me, "Failed to create OptiScaler.ini: " & ex.Message, "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

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
        UpdateIniEditorButtonState(info)

        UpdateInstallButtonText(info)
        UpdateInstallActionButtons(info)

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

    Private Sub UpdateInstallButtonText(info As OptiScalerInstallInfo)
        If btnInstall Is Nothing Then
            Return
        End If

        Dim baseText As String = If(info IsNot Nothing AndAlso info.IsInstalled, "Update", "Install")
        If chkInstallOptiPatcher IsNot Nothing AndAlso chkInstallOptiPatcher.Enabled AndAlso chkInstallOptiPatcher.Checked Then
            btnInstall.Text = baseText & " + OptiPatcher"
        Else
            btnInstall.Text = baseText
        End If
    End Sub

    Private Sub UpdateInstallActionButtons(Optional info As OptiScalerInstallInfo = Nothing)
        If btnInstall Is Nothing OrElse btnUninstall Is Nothing Then
            Return
        End If

        Dim folder As String = If(txtGameFolder Is Nothing, "", txtGameFolder.Text.Trim())
        Dim hasFolder As Boolean = Not String.IsNullOrWhiteSpace(folder) AndAlso Directory.Exists(folder)
        If btnOpenGameFolder IsNot Nothing Then
            btnOpenGameFolder.Enabled = hasFolder
        End If

        Dim gameExePath As String = If(txtGameExe Is Nothing, "", txtGameExe.Text.Trim())
        Dim hasGameExe As Boolean = Not String.IsNullOrWhiteSpace(gameExePath) AndAlso File.Exists(gameExePath)

        Dim hasHook As Boolean = cmbHookName IsNot Nothing AndAlso
                                 cmbHookName.SelectedItem IsNot Nothing AndAlso
                                 Not String.IsNullOrWhiteSpace(cmbHookName.SelectedItem.ToString())

        Dim sourceReady As Boolean = IsSelectedInstallSourceReady()
        Dim autoOptiPatcherReady As Boolean = True
        If chkInstallOptiPatcher IsNot Nothing AndAlso chkInstallOptiPatcher.Checked Then
            autoOptiPatcherReady = chkInstallOptiPatcher.Enabled
        End If

        Dim canInstall As Boolean = Not installOperationInProgress AndAlso
                                    Not uninstallOperationInProgress AndAlso
                                    hasFolder AndAlso
                                    hasGameExe AndAlso
                                    hasHook AndAlso
                                    sourceReady AndAlso
                                    autoOptiPatcherReady AndAlso
                                    Not HasBlockingInstallInputErrors()
        btnInstall.Enabled = canInstall

        Dim installInfo As OptiScalerInstallInfo = info
        If installInfo Is Nothing AndAlso hasFolder Then
            installInfo = OptiScalerInstallDetector.Detect(folder)
        End If

        Dim canUninstall As Boolean = Not installOperationInProgress AndAlso
                                      Not uninstallOperationInProgress AndAlso
                                      hasFolder AndAlso
                                      installInfo IsNot Nothing AndAlso
                                      installInfo.IsInstalled
        btnUninstall.Enabled = canUninstall
    End Sub

    Private Function IsSelectedInstallSourceReady() As Boolean
        If rbLocal IsNot Nothing AndAlso rbLocal.Checked Then
            Dim localPath As String = If(txtLocalArchive Is Nothing, "", txtLocalArchive.Text.Trim())
            If String.IsNullOrWhiteSpace(localPath) OrElse Not File.Exists(localPath) Then
                Return False
            End If

            Try
                Return New FileInfo(localPath).Length > 0
            Catch
                Return False
            End Try
        End If

        If rbStable IsNot Nothing AndAlso rbStable.Checked Then
            Return stableRelease IsNot Nothing
        End If

        If rbNightly IsNot Nothing AndAlso rbNightly.Checked Then
            Return nightlyRelease IsNot Nothing
        End If

        Return False
    End Function

    Private Function HasBlockingInstallInputErrors() As Boolean
        If cmbFgType IsNot Nothing AndAlso cmbFgType.SelectedIndex = 3 Then
            Dim nukemPath As String = If(txtNukemDll Is Nothing, "", txtNukemDll.Text.Trim())
            If String.IsNullOrWhiteSpace(nukemPath) OrElse Not File.Exists(nukemPath) Then
                Return True
            End If
            If Not Path.GetExtension(nukemPath).Equals(".dll", StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        End If

        Dim fakenvapiPath As String = If(txtFakenvapiFolder Is Nothing, "", txtFakenvapiFolder.Text.Trim())
        If Not String.IsNullOrWhiteSpace(fakenvapiPath) Then
            Dim fakenvapiRoot As String = fakenvapiPath
            If File.Exists(fakenvapiRoot) Then
                fakenvapiRoot = Path.GetDirectoryName(fakenvapiRoot)
            End If

            If String.IsNullOrWhiteSpace(fakenvapiRoot) OrElse Not Directory.Exists(fakenvapiRoot) Then
                Return True
            End If
        End If

        Dim nvngxPath As String = If(txtNvngxDll Is Nothing, "", txtNvngxDll.Text.Trim())
        If Not String.IsNullOrWhiteSpace(nvngxPath) AndAlso Not File.Exists(nvngxPath) Then
            Return True
        End If

        If chkEnableReshade IsNot Nothing AndAlso chkEnableReshade.Checked Then
            Dim reshadePath As String = If(txtReshadeDll Is Nothing, "", txtReshadeDll.Text.Trim())
            If String.IsNullOrWhiteSpace(reshadePath) OrElse Not File.Exists(reshadePath) Then
                Return True
            End If
            If Not Path.GetExtension(reshadePath).Equals(".dll", StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        End If

        If chkEnableSpecialK IsNot Nothing AndAlso chkEnableSpecialK.Checked Then
            Dim specialKPath As String = If(txtSpecialKDll Is Nothing, "", txtSpecialKDll.Text.Trim())
            If String.IsNullOrWhiteSpace(specialKPath) OrElse Not File.Exists(specialKPath) Then
                Return True
            End If
            If Not Path.GetExtension(specialKPath).Equals(".dll", StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        End If

        If chkLoadAsiPlugins IsNot Nothing AndAlso chkLoadAsiPlugins.Checked Then
            Dim pluginsPath As String = If(txtPluginsPath Is Nothing, "", txtPluginsPath.Text.Trim())
            If String.IsNullOrWhiteSpace(pluginsPath) OrElse Not Directory.Exists(pluginsPath) Then
                Return True
            End If
        End If

        Return False
    End Function

    Private Sub UpdateIniEditorButtonState(info As OptiScalerInstallInfo)
        If btnEditIni Is Nothing Then
            Return
        End If

        Dim folder As String = txtGameFolder.Text.Trim()
        If String.IsNullOrWhiteSpace(folder) OrElse Not Directory.Exists(folder) Then
            btnEditIni.Enabled = False
            Return
        End If

        Dim iniPath As String = Path.Combine(folder, "OptiScaler.ini")
        Dim hasIni As Boolean = File.Exists(iniPath)
        Dim isInstalled As Boolean = info IsNot Nothing AndAlso info.IsInstalled
        btnEditIni.Enabled = isInstalled OrElse hasIni
    End Sub

    Private Sub UpdateOptiPatcherStatus()
        If lblOptiPatcherStatus Is Nothing Then
            Return
        End If

        If InvokeRequired Then
            BeginInvoke(New Action(AddressOf UpdateOptiPatcherStatus))
            Return
        End If

        Dim folder As String = txtGameFolder.Text.Trim()
        If String.IsNullOrWhiteSpace(folder) OrElse Not Directory.Exists(folder) Then
            lblOptiPatcherStatus.Text = "Status: pick a target game folder."
            btnInstallOptiPatcher.Enabled = False
            btnRemoveOptiPatcher.Enabled = False
            If chkInstallOptiPatcher IsNot Nothing Then
                chkInstallOptiPatcher.Checked = False
                chkInstallOptiPatcher.Enabled = False
            End If
            If lblInstallOptiPatcherStatus IsNot Nothing Then
                lblInstallOptiPatcherStatus.Text = "OptiPatcher: select a supported detected game to enable."
            End If
            SyncAutoOptiPatcherUiState(False)
            UpdateInstallButtonText(OptiScalerInstallDetector.Detect(txtGameFolder.Text))
            UpdateInstallActionButtons()
            lastOptiPatcherStatusKey = "no-folder"
            Return
        End If

        Dim installInfo As OptiPatcherInstallInfo = OptiPatcherInstallDetector.Detect(folder)
        btnRemoveOptiPatcher.Enabled = installInfo IsNot Nothing AndAlso installInfo.IsInstalled

        Dim detectedGame As DetectedGame = Nothing
        Dim supportEntry As OptiPatcherSupportEntry = ResolveCurrentOptiPatcherSupport(detectedGame)
        Dim isSupported As Boolean = supportEntry IsNot Nothing
        btnInstallOptiPatcher.Enabled = isSupported
        If chkInstallOptiPatcher IsNot Nothing Then
            If Not isSupported Then
                chkInstallOptiPatcher.Checked = False
            End If
            chkInstallOptiPatcher.Enabled = isSupported
        End If
        If lblInstallOptiPatcherStatus IsNot Nothing Then
            lblInstallOptiPatcherStatus.Text = BuildInstallTabOptiPatcherStatusText(detectedGame, supportEntry, installInfo)
        End If
        SyncAutoOptiPatcherUiState(False)
        UpdateInstallButtonText(OptiScalerInstallDetector.Detect(txtGameFolder.Text))
        UpdateInstallActionButtons()

        Dim statusText As String = BuildOptiPatcherStatusText(installInfo, supportEntry)
        lblOptiPatcherStatus.Text = statusText

        Dim key As String = $"{statusText}|{If(installInfo IsNot Nothing AndAlso installInfo.IsInstalled, installInfo.AsiPath, "")}"
        If key <> lastOptiPatcherStatusKey Then
            lastOptiPatcherStatusKey = key
            AppendLog("OptiPatcher status: " & statusText)
        End If
    End Sub

    Private Function BuildOptiPatcherStatusText(installInfo As OptiPatcherInstallInfo, supportEntry As OptiPatcherSupportEntry) As String
        Dim installText As String
        If installInfo Is Nothing OrElse Not installInfo.IsInstalled Then
            installText = "not installed"
        Else
            Dim versionText As String = If(String.IsNullOrWhiteSpace(installInfo.Version), "unknown", installInfo.Version)
            installText = "installed (" & versionText & ")"
        End If

        Dim supportText As String = "support unknown"
        If supportEntry IsNot Nothing Then
            supportText = "supported"
            If supportEntry.DlssFgSupported.HasValue Then
                supportText &= If(supportEntry.DlssFgSupported.Value, ", DLSS-FG yes", ", DLSS-FG no")
            End If
        End If

        Return $"Status: {installText}, {supportText}"
    End Function

    Private Function BuildInstallTabOptiPatcherStatusText(detectedGame As DetectedGame,
                                                          supportEntry As OptiPatcherSupportEntry,
                                                          installInfo As OptiPatcherInstallInfo) As String
        Dim sourceText As String = GetOptiPatcherSourceDisplayText()
        Dim autoInstallEnabled As Boolean = chkInstallOptiPatcher IsNot Nothing AndAlso chkInstallOptiPatcher.Enabled AndAlso chkInstallOptiPatcher.Checked

        If detectedGame Is Nothing Then
            Return "OptiPatcher: unsupported until game is detected from the supported list. Source: " & sourceText
        End If

        If supportEntry Is Nothing Then
            Return "OptiPatcher: unsupported for " & detectedGame.DisplayName
        End If

        Dim autoText As String = If(autoInstallEnabled, "auto-install enabled", "auto-install disabled")
        If installInfo IsNot Nothing AndAlso installInfo.IsInstalled Then
            Dim versionText As String = If(String.IsNullOrWhiteSpace(installInfo.Version), "unknown", installInfo.Version)
            Return "OptiPatcher: supported for " & detectedGame.DisplayName & " (installed " & versionText & ", " & autoText & ", source " & sourceText & ")"
        End If

        Return "OptiPatcher: supported for " & detectedGame.DisplayName & " (" & autoText & ", source " & sourceText & ")"
    End Function

    Private Function FindDetectedGameByInstallDir(folder As String) As DetectedGame
        If detectedGames Is Nothing OrElse String.IsNullOrWhiteSpace(folder) Then
            Return Nothing
        End If

        Dim normalizedTarget As String = NormalizePathSafe(folder)
        Dim bestMatch As DetectedGame = Nothing
        Dim bestLength As Integer = -1

        For Each game As DetectedGame In detectedGames
            If game Is Nothing Then
                Continue For
            End If

            Dim gamePath As String = NormalizePathSafe(game.InstallDir)
            If String.IsNullOrWhiteSpace(gamePath) Then
                Continue For
            End If

            If String.Equals(gamePath, normalizedTarget, StringComparison.OrdinalIgnoreCase) Then
                Return game
            End If

            Dim targetContainsGame As Boolean = normalizedTarget.StartsWith(gamePath & Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            Dim gameContainsTarget As Boolean = gamePath.StartsWith(normalizedTarget & Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            If targetContainsGame OrElse gameContainsTarget Then
                If gamePath.Length > bestLength Then
                    bestLength = gamePath.Length
                    bestMatch = game
                End If
            End If
        Next

        Return bestMatch
    End Function

    Private Function ResolveCurrentOptiPatcherSupport(ByRef detectedGame As DetectedGame) As OptiPatcherSupportEntry
        detectedGame = Nothing
        Dim folder As String = txtGameFolder.Text.Trim()
        If String.IsNullOrWhiteSpace(folder) Then
            Return Nothing
        End If

        detectedGame = FindDetectedGameByInstallDir(folder)
        If detectedGame Is Nothing Then
            Return Nothing
        End If

        Return OptiPatcherSupportService.FindByGameName(optiPatcherSupportLookup, detectedGame.DisplayName)
    End Function

    Private Function ResolveOptiPatcherSupportForGame(detectedGame As DetectedGame) As OptiPatcherSupportEntry
        If detectedGame Is Nothing Then
            Return Nothing
        End If

        Return OptiPatcherSupportService.FindByGameName(optiPatcherSupportLookup, detectedGame.DisplayName)
    End Function

    Private Async Function InstallOptiPatcherAsync(showDialogs As Boolean) As Task(Of Boolean)
        Try
            Dim config As OptiPatcherInstallConfig = BuildOptiPatcherConfig()
            If String.IsNullOrWhiteSpace(config.GameFolder) OrElse Not Directory.Exists(config.GameFolder) Then
                If showDialogs Then
                    MessageBox.Show(Me, "Select a valid game folder first.", "OptiPatcher", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
                AppendLog("OptiPatcher install blocked: invalid game folder.")
                Return False
            End If

            Dim detectedGame As DetectedGame = Nothing
            Dim support As OptiPatcherSupportEntry = ResolveCurrentOptiPatcherSupport(detectedGame)
            If support Is Nothing Then
                Dim message As String = "OptiPatcher can only be installed for supported detected games." & Environment.NewLine &
                                        "Pick a supported game from Game Detection and use that folder."
                AppendLog("OptiPatcher install blocked: unsupported or undetected game.")
                If showDialogs Then
                    MessageBox.Show(Me, message, "Unsupported game", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
                Return False
            End If

            AppendLog("Starting OptiPatcher install...")
            Dim manifest As OptiPatcherManifest = Await OptiPatcherInstallerService.InstallAsync(config, AddressOf AppendLog)
            If manifest Is Nothing Then
                AppendLog("OptiPatcher install skipped.")
                Return False
            End If

            AppendLog("OptiPatcher install completed.")
            Return True
        Catch ex As Exception
            AppendLog("OptiPatcher install failed: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.InstallOptiPatcherAsync")
            If showDialogs Then
                MessageBox.Show(Me, "OptiPatcher install failed: " & ex.Message, "OptiPatcher", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        Finally
            UpdateOptiPatcherStatus()
        End Try
    End Function

    Private Async Function ValidateAutoOptiPatcherPreflightAsync() As Task(Of Boolean)
        Dim config As OptiPatcherInstallConfig = BuildOptiPatcherConfig()
        Dim errors As New List(Of String)()

        If config Is Nothing Then
            errors.Add("OptiPatcher configuration is missing.")
        Else
            If String.IsNullOrWhiteSpace(config.GameFolder) OrElse Not Directory.Exists(config.GameFolder) Then
                errors.Add("Game folder is missing for OptiPatcher auto-install.")
            End If

            Select Case config.Source
                Case OptiPatcherSource.LocalFile
                    If String.IsNullOrWhiteSpace(config.LocalAsiPath) OrElse Not File.Exists(config.LocalAsiPath) Then
                        errors.Add("Local OptiPatcher .asi file was not found.")
                    ElseIf Not String.Equals(Path.GetExtension(config.LocalAsiPath), ".asi", StringComparison.OrdinalIgnoreCase) Then
                        errors.Add("Local OptiPatcher file must be an .asi file.")
                    End If
                Case OptiPatcherSource.Alternate
                    Dim settings As AppSettingsModel = AppSettings.Load()
                    If settings Is Nothing OrElse String.IsNullOrWhiteSpace(settings.OptiPatcherAlternateReleaseUrl) Then
                        errors.Add("Alternate OptiPatcher source URL is empty in settings.")
                    End If
            End Select

            If errors.Count = 0 AndAlso config.Source <> OptiPatcherSource.LocalFile Then
                Dim selectedRelease As ReleaseInfo = GetSelectedOptiPatcherRelease(config)
                If selectedRelease Is Nothing OrElse String.IsNullOrWhiteSpace(selectedRelease.DownloadUrl) Then
                    AppendLog("OptiPatcher release info missing for selected source. Refreshing now...")
                    Await RefreshOptiPatcherReleaseInfoAsync(False)
                    config = BuildOptiPatcherConfig()
                    selectedRelease = GetSelectedOptiPatcherRelease(config)
                    If selectedRelease Is Nothing OrElse String.IsNullOrWhiteSpace(selectedRelease.DownloadUrl) Then
                        errors.Add("Selected OptiPatcher source does not have a downloadable release asset.")
                    End If
                End If
            End If
        End If

        If errors.Count > 0 Then
            AppendLog("OptiPatcher preflight errors: " & String.Join("; ", errors))
            MessageBox.Show(Me,
                            "Fix the following OptiPatcher issues before installing:" & Environment.NewLine & "- " &
                            String.Join(Environment.NewLine & "- ", errors),
                            "OptiPatcher validation failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return False
        End If

        Return True
    End Function

    Private Function GetSelectedOptiPatcherRelease(config As OptiPatcherInstallConfig) As ReleaseInfo
        If config Is Nothing Then
            Return Nothing
        End If

        Select Case config.Source
            Case OptiPatcherSource.Stable
                Return config.StableRelease
            Case OptiPatcherSource.Alternate
                Return config.AlternateRelease
            Case OptiPatcherSource.Rolling
                Return config.RollingRelease
            Case Else
                Return Nothing
        End Select
    End Function

    Private Function GetOptiPatcherSourceDisplayText() As String
        Select Case GetOptiPatcherSourceFromUi()
            Case OptiPatcherSource.Stable
                Return "Stable"
            Case OptiPatcherSource.Alternate
                Return "Alternate"
            Case OptiPatcherSource.LocalFile
                Dim fileName As String = Path.GetFileName(If(txtOptiPatcherLocalFile.Text, "").Trim())
                If String.IsNullOrWhiteSpace(fileName) Then
                    Return "Local .asi"
                End If
                Return "Local .asi (" & fileName & ")"
            Case Else
                Return "Rolling"
        End Select
    End Function

    Private Function ConfirmInstallSummary(action As InstallAction,
                                           config As InstallerConfig,
                                           installOptiPatcherAfterInstall As Boolean,
                                           detectedGame As DetectedGame,
                                           optiPatcherSupport As OptiPatcherSupportEntry) As Boolean
        Dim addOns As New List(Of String)()
        If Not String.IsNullOrWhiteSpace(config.FakenvapiFolder) Then
            addOns.Add("Fakenvapi")
        End If
        If Not String.IsNullOrWhiteSpace(config.NvngxDllPath) Then
            addOns.Add("nvngx override")
        End If
        If config.FgType = FgTypeSelection.Nukem AndAlso Not String.IsNullOrWhiteSpace(config.NukemDllPath) Then
            addOns.Add("Nukem FG DLL")
        End If
        If config.EnableReshade Then
            addOns.Add("ReShade")
        End If
        If config.EnableSpecialK Then
            addOns.Add("Special K")
        End If
        If config.LoadAsiPlugins Then
            addOns.Add("ASI loading")
        End If

        Dim optiScalerSourceText As String = "Unknown"
        Select Case config.Source
            Case ReleaseSource.Stable
                optiScalerSourceText = "Stable"
            Case ReleaseSource.Nightly
                optiScalerSourceText = "Alternate"
            Case ReleaseSource.LocalArchive
                optiScalerSourceText = "Local archive"
        End Select

        Dim optiPatcherSourceText As String = GetOptiPatcherSourceDisplayText()

        Dim actionText As String = "Install"
        Select Case action
            Case InstallAction.Update
                actionText = "Update"
            Case InstallAction.Reinstall
                actionText = "Reinstall"
        End Select

        Dim patcherLine As String = "No"
        If installOptiPatcherAfterInstall Then
            patcherLine = "Yes (" & optiPatcherSourceText & ")"
            If detectedGame IsNot Nothing AndAlso optiPatcherSupport IsNot Nothing Then
                patcherLine &= " for " & detectedGame.DisplayName
            End If
        End If

        Dim summary As New StringBuilder()
        summary.AppendLine("Review install plan:")
        summary.AppendLine()
        summary.AppendLine("Action: " & actionText)
        summary.AppendLine("OptiScaler source: " & optiScalerSourceText)
        summary.AppendLine("OptiPatcher source: " & optiPatcherSourceText)
        summary.AppendLine("Hook filename: " & config.HookName)
        summary.AppendLine("GPU selection: " & If(config.GpuVendor = GpuVendor.AmdIntel, "AMD/Intel", "NVIDIA"))
        summary.AppendLine("Frame generation: " & config.FgType.ToString())
        summary.AppendLine("Add-ons: " & If(addOns.Count = 0, "None", String.Join(", ", addOns)))
        summary.AppendLine("Install OptiPatcher: " & patcherLine)
        summary.AppendLine()
        summary.AppendLine("Proceed?")

        Return MessageBox.Show(Me,
                               summary.ToString(),
                               "Confirm install",
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question) = DialogResult.Yes
    End Function

    Private Sub UpdateExperimentalStatus()
        If lblFsr4Status Is Nothing Then
            Return
        End If

        If InvokeRequired Then
            BeginInvoke(New Action(AddressOf UpdateExperimentalStatus))
            Return
        End If

        If txtFsr4TargetGameFolder IsNot Nothing Then
            txtFsr4TargetGameFolder.Text = txtGameFolder.Text.Trim()
        End If

        Dim targetGameFolder As String = txtGameFolder.Text.Trim()
        If String.IsNullOrWhiteSpace(targetGameFolder) Then
            lblFsr4Status.Text = "Experimental package: pick a target game first."
            lastExperimentalStatusKey = "no-game"
            Return
        End If

        Dim status As ExperimentalFsr4Status = ExperimentalFsr4Service.Detect(targetGameFolder)
        Dim statusText As String = BuildExperimentalStatusText(status)
        lblFsr4Status.Text = statusText

        Dim key As String = If(status Is Nothing, "none", $"{status.IsInstalled}|{status.IsManaged}|{status.Version}|{status.Source}")
        If key <> lastExperimentalStatusKey Then
            lastExperimentalStatusKey = key
            AppendLog("Experimental status: " & statusText)
        End If
    End Sub

    Private Function BuildExperimentalStatusText(status As ExperimentalFsr4Status) As String
        If status Is Nothing OrElse Not status.IsInstalled Then
            Return "Experimental package: not installed"
        End If

        Dim versionText As String = If(String.IsNullOrWhiteSpace(status.Version), "unknown", status.Version)
        Dim sourceText As String = If(String.IsNullOrWhiteSpace(status.Source), "unknown", status.Source)
        Dim managedText As String = If(status.IsManaged, "managed", "unmanaged")
        Return $"Experimental package: installed ({versionText}, {sourceText}, {managedText})"
    End Function

    Private Function GetExperimentalListStatusText(status As ExperimentalFsr4Status) As String
        If status Is Nothing OrElse Not status.IsInstalled Then
            Return "No"
        End If

        Dim versionText As String = If(String.IsNullOrWhiteSpace(status.Version), "", status.Version.Trim())
        Dim managedText As String = If(status.IsManaged, "Managed", "Unmanaged")
        If String.IsNullOrWhiteSpace(versionText) Then
            Return "Yes (" & managedText & ")"
        End If

        Return "Yes (" & versionText & ", " & managedText & ")"
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
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.PromptInstallAction")
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

    Private Function FindPreferredExecutable(installDir As String, displayName As String, Optional sourceName As String = "") As String
        If String.IsNullOrWhiteSpace(installDir) OrElse Not Directory.Exists(installDir) Then
            Return Nothing
        End If

        Dim candidates As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each folder As String In GetCandidateExeFolders(installDir)
            AddExecutablesFromFolder(folder, candidates)
        Next

        If candidates.Count = 0 Then
            For Each exePath As String In FindExecutablesRecursively(installDir, 4, 1200)
                candidates.Add(exePath)
            Next
        End If

        If candidates.Count = 0 Then
            Return Nothing
        End If

        Dim bestPath As String = ""
        Dim bestScore As Integer = Integer.MinValue
        Dim preferUnrealBinaries As Boolean = Directory.Exists(Path.Combine(installDir, "Engine"))

        For Each exePath As String In candidates
            Dim score As Integer = ScoreExecutableCandidate(exePath, displayName, sourceName, installDir, preferUnrealBinaries)
            If score > bestScore Then
                bestScore = score
                bestPath = exePath
            End If
        Next

        If String.IsNullOrWhiteSpace(bestPath) Then
            Return Nothing
        End If

        Return bestPath
    End Function

    Private Sub AddExecutablesFromFolder(folder As String, candidates As HashSet(Of String))
        If candidates Is Nothing OrElse String.IsNullOrWhiteSpace(folder) OrElse Not Directory.Exists(folder) Then
            Return
        End If

        Try
            For Each exePath As String In Directory.GetFiles(folder, "*.exe", SearchOption.TopDirectoryOnly)
                Dim exeName As String = Path.GetFileNameWithoutExtension(exePath)
                If ShouldSkipExecutable(exeName) Then
                    Continue For
                End If

                candidates.Add(exePath)
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.AddExecutablesFromFolder")
        End Try
    End Sub

    Private Function FindExecutablesRecursively(rootFolder As String, maxDepth As Integer, maxFolders As Integer) As List(Of String)
        Dim results As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If String.IsNullOrWhiteSpace(rootFolder) OrElse Not Directory.Exists(rootFolder) Then
            Return results.ToList()
        End If

        Dim queue As New Queue(Of Tuple(Of String, Integer))()
        Dim visited As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        queue.Enqueue(Tuple.Create(rootFolder, 0))
        Dim scanned As Integer = 0

        While queue.Count > 0 AndAlso scanned < maxFolders
            Dim current As Tuple(Of String, Integer) = queue.Dequeue()
            Dim folder As String = current.Item1
            Dim depth As Integer = current.Item2

            Dim normalized As String = NormalizePathSafe(folder)
            If String.IsNullOrWhiteSpace(normalized) OrElse visited.Contains(normalized) Then
                Continue While
            End If
            visited.Add(normalized)
            scanned += 1

            AddExecutablesFromFolder(normalized, results)

            If depth >= maxDepth Then
                Continue While
            End If

            Dim children As IEnumerable(Of String) = Enumerable.Empty(Of String)()
            Try
                children = Directory.EnumerateDirectories(normalized, "*", SearchOption.TopDirectoryOnly)
            Catch ex As Exception
                ErrorLogger.Log(ex, "MainForm.FindExecutablesRecursively.EnumerateDirs")
            End Try

            For Each child As String In children
                If ShouldSkipFolderForExeSearch(child) Then
                    Continue For
                End If
                queue.Enqueue(Tuple.Create(child, depth + 1))
            Next
        End While

        Return results.ToList()
    End Function

    Private Function ShouldSkipFolderForExeSearch(folderPath As String) As Boolean
        Dim folderName As String = Path.GetFileName(folderPath)
        If String.IsNullOrWhiteSpace(folderName) Then
            Return False
        End If

        Dim lower As String = folderName.ToLowerInvariant()
        Dim skipTokens As String() = {
            "$recycle.bin", "installer", "install", "support", "docs", "doc", "manual",
            "redist", "redistributable", "prereq", "prerequisite", "benchmark", "tool",
            "tools", "crash", "eac", "easyanticheat", "battleye"
        }

        For Each token As String In skipTokens
            If lower.Contains(token) Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Function ScoreExecutableCandidate(exePath As String,
                                              displayName As String,
                                              sourceName As String,
                                              installDir As String,
                                              preferUnrealBinaries As Boolean) As Integer
        If String.IsNullOrWhiteSpace(exePath) OrElse Not File.Exists(exePath) Then
            Return Integer.MinValue
        End If

        Dim exeName As String = Path.GetFileNameWithoutExtension(exePath)
        If ShouldSkipExecutable(exeName) Then
            Return -1000
        End If

        Dim score As Integer = 0
        Dim normalizedExe As String = NameNormalization.NormalizeRelaxedName(exeName)
        Dim normalizedDisplay As String = NameNormalization.NormalizeRelaxedName(displayName)
        Dim normalizedSource As String = NameNormalization.NormalizeRelaxedName(sourceName)

        If Not String.IsNullOrWhiteSpace(normalizedDisplay) Then
            If normalizedExe = normalizedDisplay Then
                score += 220
            ElseIf normalizedExe.Contains(normalizedDisplay, StringComparison.OrdinalIgnoreCase) Then
                score += 130
            End If
            score += ScoreTokenMatches(exeName, displayName, 12)
        End If

        If Not String.IsNullOrWhiteSpace(normalizedSource) Then
            If normalizedExe = normalizedSource Then
                score += 180
            ElseIf normalizedExe.Contains(normalizedSource, StringComparison.OrdinalIgnoreCase) Then
                score += 95
            End If
            score += ScoreTokenMatches(exeName, sourceName, 10)
        End If

        Dim lowerPath As String = exePath.ToLowerInvariant()
        If lowerPath.Contains("\binaries\wingdk\") Then
            score += 55
        End If
        If lowerPath.Contains("\binaries\win64\") Then
            score += 50
        End If
        If lowerPath.Contains("\win64\") OrElse lowerPath.Contains("\x64\") Then
            score += 40
        End If
        If lowerPath.Contains("\binaries\") OrElse lowerPath.Contains("\bin\") Then
            score += 25
        End If

        If preferUnrealBinaries Then
            If lowerPath.Contains("\binaries\wingdk\") Then
                score += 260
            End If
            If lowerPath.Contains("\binaries\win64\") Then
                score += 250
            End If
            If lowerPath.Contains("-shipping") Then
                score += 170
            End If

            Dim normalizedInstallDir As String = NormalizePathSafe(installDir)
            Dim normalizedExeDir As String = NormalizePathSafe(Path.GetDirectoryName(exePath))
            If Not String.IsNullOrWhiteSpace(normalizedInstallDir) AndAlso
               Not String.IsNullOrWhiteSpace(normalizedExeDir) AndAlso
               String.Equals(normalizedInstallDir, normalizedExeDir, StringComparison.OrdinalIgnoreCase) Then
                score -= 220
            End If
        End If

        Try
            Dim length As Long = New FileInfo(exePath).Length
            If length >= 40L * 1024L * 1024L Then
                score += 20
            ElseIf length <= 2L * 1024L * 1024L Then
                score -= 10
            End If
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.ScoreExecutableCandidate.FileInfo")
        End Try

        Return score
    End Function

    Private Function ScoreTokenMatches(exeName As String, reference As String, pointsPerHit As Integer) As Integer
        If String.IsNullOrWhiteSpace(exeName) OrElse String.IsNullOrWhiteSpace(reference) Then
            Return 0
        End If

        Dim score As Integer = 0
        Dim hitCount As Integer = 0
        Dim parts As String() = reference.Split(New Char() {" "c, "_"c, "-"c, "."c, ":"c, "'"c, "("c, ")"c, "["c, "]"c, "/"c, "\"c}, StringSplitOptions.RemoveEmptyEntries)
        For Each part As String In parts
            Dim token As String = part.Trim()
            If token.Length < 3 Then
                Continue For
            End If

            If exeName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0 Then
                hitCount += 1
            End If
        Next

        score = hitCount * pointsPerHit
        If score > 80 Then
            score = 80
        End If

        Return score
    End Function

    Private Function GetCandidateExeFolders(installDir As String) As List(Of String)
        Dim folders As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim baseCandidates As String() = {
            installDir,
            Path.Combine(installDir, "Binaries"),
            Path.Combine(installDir, "Binaries", "Win64"),
            Path.Combine(installDir, "Binaries", "Win32"),
            Path.Combine(installDir, "Binaries", "WinGDK"),
            Path.Combine(installDir, "bin"),
            Path.Combine(installDir, "Bin"),
            Path.Combine(installDir, "bin", "x64"),
            Path.Combine(installDir, "bin", "Win64"),
            Path.Combine(installDir, "Win64"),
            Path.Combine(installDir, "Win32"),
            Path.Combine(installDir, "x64"),
            Path.Combine(installDir, "WinGDK")
        }

        For Each folder As String In baseCandidates
            If seen.Contains(folder) Then
                Continue For
            End If
            folders.Add(folder)
            seen.Add(folder)
        Next

        Try
            For Each child As String In Directory.EnumerateDirectories(installDir, "*", SearchOption.TopDirectoryOnly)
                Dim childCandidates As String() = {
                    child,
                    Path.Combine(child, "Binaries"),
                    Path.Combine(child, "Binaries", "Win64"),
                    Path.Combine(child, "Binaries", "Win32"),
                    Path.Combine(child, "Binaries", "WinGDK"),
                    Path.Combine(child, "bin"),
                    Path.Combine(child, "bin", "x64"),
                    Path.Combine(child, "bin", "Win64"),
                    Path.Combine(child, "Win64"),
                    Path.Combine(child, "Win32"),
                    Path.Combine(child, "x64"),
                    Path.Combine(child, "WinGDK")
                }

                For Each folder As String In childCandidates
                    If seen.Contains(folder) Then
                        Continue For
                    End If
                    folders.Add(folder)
                    seen.Add(folder)
                Next

                For Each grandChild As String In Directory.EnumerateDirectories(child, "*", SearchOption.TopDirectoryOnly)
                    Dim grandChildCandidates As String() = {
                        grandChild,
                        Path.Combine(grandChild, "Binaries"),
                        Path.Combine(grandChild, "Binaries", "Win64"),
                        Path.Combine(grandChild, "Binaries", "Win32"),
                        Path.Combine(grandChild, "Binaries", "WinGDK"),
                        Path.Combine(grandChild, "bin"),
                        Path.Combine(grandChild, "bin", "x64"),
                        Path.Combine(grandChild, "bin", "Win64"),
                        Path.Combine(grandChild, "Win64"),
                        Path.Combine(grandChild, "Win32"),
                        Path.Combine(grandChild, "x64"),
                        Path.Combine(grandChild, "WinGDK")
                    }

                    For Each folder As String In grandChildCandidates
                        If seen.Contains(folder) Then
                            Continue For
                        End If
                        folders.Add(folder)
                        seen.Add(folder)
                    Next
                Next
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.GetCandidateExeFolders")
        End Try

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
        If chkLoadAsiPlugins.Checked Then
            EnsurePluginsFolderReady(True)
        End If
        SyncAutoOptiPatcherUiState(False)
    End Sub

    Private Sub chkInstallOptiPatcher_CheckedChanged(sender As Object, e As EventArgs) Handles chkInstallOptiPatcher.CheckedChanged
        SyncAutoOptiPatcherUiState(True)
        UpdateOptiPatcherStatus()
    End Sub

    Private Sub SyncAutoOptiPatcherUiState(logAction As Boolean)
        If chkLoadAsiPlugins Is Nothing OrElse txtPluginsPath Is Nothing OrElse btnBrowsePluginsPath Is Nothing Then
            Return
        End If

        Dim autoInstallEnabled As Boolean = chkInstallOptiPatcher IsNot Nothing AndAlso chkInstallOptiPatcher.Enabled AndAlso chkInstallOptiPatcher.Checked
        If autoInstallEnabled Then
            If Not chkLoadAsiPlugins.Checked Then
                chkLoadAsiPlugins.Checked = True
            End If

            chkLoadAsiPlugins.Enabled = False
            EnsurePluginsFolderReady(logAction)
            txtPluginsPath.Enabled = False
            btnBrowsePluginsPath.Enabled = False
        Else
            chkLoadAsiPlugins.Enabled = True
            txtPluginsPath.Enabled = chkLoadAsiPlugins.Checked
            btnBrowsePluginsPath.Enabled = chkLoadAsiPlugins.Checked
        End If

        UpdateInstallButtonText(OptiScalerInstallDetector.Detect(txtGameFolder.Text))
    End Sub

    Private Sub EnsurePluginsFolderReady(logAction As Boolean)
        If Not chkLoadAsiPlugins.Checked Then
            Return
        End If

        Dim gameFolder As String = txtGameFolder.Text.Trim()
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return
        End If

        Dim pluginPath As String = txtPluginsPath.Text.Trim()
        If String.IsNullOrWhiteSpace(pluginPath) Then
            pluginPath = Path.Combine(gameFolder, "plugins")
            txtPluginsPath.Text = pluginPath
        ElseIf Not Path.IsPathRooted(pluginPath) Then
            pluginPath = Path.GetFullPath(Path.Combine(gameFolder, pluginPath))
            txtPluginsPath.Text = pluginPath
        End If

        If Not Directory.Exists(pluginPath) Then
            Try
                Directory.CreateDirectory(pluginPath)
                If logAction Then
                    AppendLog("Created plugins folder: " & pluginPath)
                End If
            Catch ex As Exception
                AppendLog("Failed to create plugins folder: " & ex.Message)
                ErrorLogger.Log(ex, "MainForm.EnsurePluginsFolderReady")
            End Try
        End If
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

    Private Function BuildQuickInstallConfig(gameExePath As String) As InstallerConfig
        Dim normalizedExe As String = NormalizePathSafe(gameExePath)
        If String.IsNullOrWhiteSpace(normalizedExe) OrElse Not File.Exists(normalizedExe) Then
            Throw New InvalidOperationException("Quick install requires a valid game executable.")
        End If

        Dim gameFolder As String = NormalizePathSafe(Path.GetDirectoryName(normalizedExe))
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Throw New InvalidOperationException("Quick install requires a valid game folder.")
        End If

        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim hookName As String = ResolveQuickInstallHookName(settings)
        Dim gpuVendor As GpuVendor = ResolveQuickInstallGpuVendor(settings)
        Dim dlssInputs As Boolean = True
        If settings IsNot Nothing AndAlso settings.DefaultDlssInputs.HasValue Then
            dlssInputs = settings.DefaultDlssInputs.Value
        End If

        Dim fgSelection As FgTypeSelection = ResolveQuickInstallFgSelection(settings)
        If fgSelection = FgTypeSelection.Nukem Then
            AppendLog("Quick install: default frame generation is Nukem but requires manual DLL input. Using Auto instead.")
            fgSelection = FgTypeSelection.Auto
        End If

        Dim conflictMode As ConflictMode = GetConflictModeFromIndex(GetDefaultConflictModeIndex(If(settings?.DefaultConflictMode, "")))
        Dim defaultIniMode As DefaultIniMode = ParseDefaultIniMode(If(settings?.DefaultIniMode, ""))

        Return New InstallerConfig With {
            .GameExePath = normalizedExe,
            .GameFolder = gameFolder,
            .HookName = hookName,
            .ConflictMode = conflictMode,
            .Source = ReleaseSource.Stable,
            .StableRelease = stableRelease,
            .NightlyRelease = nightlyRelease,
            .ComponentRelease = componentRelease,
            .LocalArchivePath = "",
            .GpuVendor = gpuVendor,
            .EnableDlssInputs = dlssInputs,
            .FgType = fgSelection,
            .FakenvapiFolder = "",
            .NukemDllPath = "",
            .NvngxDllPath = "",
            .EnableReshade = False,
            .ReshadeDllPath = "",
            .EnableSpecialK = False,
            .SpecialKDllPath = "",
            .CreateSpecialKMarker = False,
            .LoadAsiPlugins = False,
            .PluginsPath = "",
            .DefaultIniMode = defaultIniMode,
            .DefaultIniPath = If(settings Is Nothing, "", settings.DefaultIniPath)
        }
    End Function

    Private Function ResolveQuickInstallHookName(settings As AppSettingsModel) As String
        Dim hookName As String = If(settings?.DefaultHookName, "").Trim()
        If String.IsNullOrWhiteSpace(hookName) Then
            Return "dxgi.dll"
        End If

        Dim supportedHooks As String() = {
            "dxgi.dll",
            "winmm.dll",
            "version.dll",
            "dbghelp.dll",
            "d3d12.dll",
            "wininet.dll",
            "winhttp.dll",
            "OptiScaler.asi"
        }
        For Each supportedHook As String In supportedHooks
            If hookName.Equals(supportedHook, StringComparison.OrdinalIgnoreCase) Then
                Return supportedHook
            End If
        Next

        Return "dxgi.dll"
    End Function

    Private Function ResolveQuickInstallGpuVendor(settings As AppSettingsModel) As GpuVendor
        Select Case GetDefaultGpuVendorIndex(If(settings?.DefaultGpuVendor, ""))
            Case 1
                Return GpuVendor.Nvidia
            Case 2
                Return GpuVendor.AmdIntel
            Case Else
                EnsureGpuDetectionInitialized()
                If gpuDetectionVendor = GpuVendor.Nvidia OrElse gpuDetectionVendor = GpuVendor.AmdIntel Then
                    Return gpuDetectionVendor
                End If
                Return GpuVendor.Nvidia
        End Select
    End Function

    Private Function ResolveQuickInstallFgSelection(settings As AppSettingsModel) As FgTypeSelection
        Select Case GetDefaultFrameGenerationIndex(If(settings?.DefaultFrameGeneration, ""))
            Case 1
                Return FgTypeSelection.None
            Case 2
                Return FgTypeSelection.OptiFg
            Case 3
                Return FgTypeSelection.Nukem
            Case Else
                Return FgTypeSelection.Auto
        End Select
    End Function

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

        Dim conflict As ConflictMode = GetConflictModeFromIndex(cmbConflictMode.SelectedIndex)

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
            .ComponentRelease = componentRelease,
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

    Private Function GetConflictModeFromIndex(index As Integer) As ConflictMode
        Select Case index
            Case 1
                Return ConflictMode.Overwrite
            Case 2
                Return ConflictMode.Skip
            Case Else
                Return ConflictMode.BackupAndOverwrite
        End Select
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

    Private Sub ApplyCompatibilityContextMenuTheme(mode As SystemColorMode)
        If compatContextMenu Is Nothing Then
            Return
        End If

        If mode = SystemColorMode.Dark Then
            compatContextMenu.BackColor = Color.FromArgb(32, 32, 32)
            compatContextMenu.ForeColor = Color.Gainsboro
            For Each item As ToolStripItem In compatContextMenu.Items
                item.BackColor = Color.FromArgb(32, 32, 32)
                item.ForeColor = Color.Gainsboro
            Next
        Else
            compatContextMenu.BackColor = SystemColors.Control
            compatContextMenu.ForeColor = SystemColors.ControlText
            For Each item As ToolStripItem In compatContextMenu.Items
                item.BackColor = SystemColors.Control
                item.ForeColor = SystemColors.ControlText
            Next
        End If
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

    Private Sub chkShowExperimentalTabOnUnsupportedGpu_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowExperimentalTabOnUnsupportedGpu.CheckedChanged
        If loadingSettingsUi Then
            Return
        End If

        UpdateExperimentalTabAvailability(True)
    End Sub

    Private Sub btnSaveSettings_Click(sender As Object, e As EventArgs) Handles btnSaveSettings.Click
        Dim settings As AppSettingsModel = AppSettings.Load()
        settings.CompatibilityListUrl = txtCompatibilityListUrl.Text.Trim()
        settings.WikiBaseUrl = txtWikiBaseUrl.Text.Trim()
        settings.StableReleaseUrl = txtStableReleaseUrl.Text.Trim()
        settings.NightlyReleaseUrl = txtNightlyReleaseUrl.Text.Trim()
        settings.InstallerReleaseUrl = txtInstallerReleaseUrl.Text.Trim()
        settings.AutoRefreshCompatibilityOnStartup = chkAutoRefreshCompatibilityOnStartup.Checked
        settings.AutoCheckInstallerUpdates = chkAutoCheckInstallerUpdates.Checked
        settings.HideNonDetectedGames = chkHideNonDetected.Checked
        settings.ShowExperimentalTabOnUnsupportedGpu = chkShowExperimentalTabOnUnsupportedGpu.Checked
        settings.DefaultIniPath = txtDefaultIniPath.Text.Trim()
        settings.DefaultIniMode = GetDefaultIniModeFromIndex(cmbDefaultIniMode.SelectedIndex).ToString()
        settings.ExperimentalFsr4PackageFolder = txtFsr4PackageFolder.Text.Trim()
        settings.ExperimentalFsr4EnableUpdate = chkFsr4EnableUpdate.Checked
        settings.ExperimentalFsr4EnableAgility = chkFsr4EnableAgility.Checked
        settings.OptiPatcherPreferredSource = GetOptiPatcherSourceToken(cmbOptiPatcherSource.SelectedIndex)
        settings.OptiPatcherLocalPath = txtOptiPatcherLocalFile.Text.Trim()
        settings.DefaultPreset = GetDefaultPresetValue()
        settings.DefaultHookName = GetDefaultHookValue()
        settings.DefaultGpuVendor = GetDefaultGpuVendorValue()
        settings.DefaultDlssInputs = chkDefaultDlssInputs.Checked
        settings.DefaultFrameGeneration = GetDefaultFrameGenerationToken(cmbDefaultFgType.SelectedIndex)
        settings.DefaultConflictMode = GetDefaultConflictModeToken(cmbDefaultConflictMode.SelectedIndex)
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

    Private Async Sub btnAbout_Click(sender As Object, e As EventArgs) Handles btnAbout.Click
        Dim release As UpdateReleaseInfo = latestUpdateRelease
        If release Is Nothing Then
            Try
                release = Await UpdateService.GetLatestReleaseAsync()
                latestUpdateRelease = release
            Catch ex As Exception
                AppendLog("About info refresh failed: " & ex.Message)
                ErrorLogger.Log(ex, "MainForm.btnAbout_Click")
            End Try
        End If

        Dim currentVersion As Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        If currentVersion Is Nothing Then
            currentVersion = New Version(0, 0)
        End If

        Dim repositoryUrl As String = ResolveInstallerRepositoryUrl(release)
        Using dlg As New frmAbout(currentVersion, release, repositoryUrl, "Wimukthi")
            dlg.ShowDialog(Me)
        End Using
    End Sub

    Private Async Sub btnExportDiagnostics_Click(sender As Object, e As EventArgs) Handles btnExportDiagnostics.Click
        Await ExportDiagnosticsAsync()
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
                SetUpdateNoticeVisible(False, "")
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
                SetUpdateNoticeVisible(False, release.TagName)
                If showUpToDateDialog Then
                    MessageBox.Show(Me, "You're already up to date.", "Updates", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
                Return
            End If

            latestUpdateRelease = release
            SetUpdateNoticeVisible(True, release.TagName)
            Using dlg As New frmUpdate(release, currentVersion)
                dlg.ShowDialog(Me)
            End Using
        Catch ex As Exception
            AppendLog("Update check failed: " & ex.Message)
            SetStatus("Update check failed.")
            If showUpToDateDialog Then
                MessageBox.Show(Me, "Update check failed: " & ex.Message, "Updates", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            ErrorLogger.Log(ex, "MainForm.CheckForUpdates")
        Finally
            btnCheckForUpdates.Enabled = True
        End Try
    End Function

    Private Async Function CheckForUpdatesSilentAsync(Optional promptOnAvailable As Boolean = False) As Task
        ' Check for installer updates with optional startup prompt and set the UI hint.
        Try
            AppendLog("Checking for installer updates...")
            Dim release As UpdateReleaseInfo = Await UpdateService.GetLatestReleaseAsync()
            If release Is Nothing Then
                AppendLog("Update check failed: no release data.")
                SetUpdateNoticeVisible(False, "")
                Return
            End If

            latestUpdateRelease = release
            Dim currentVersion As Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
            If currentVersion Is Nothing Then
                currentVersion = New Version(0, 0)
            End If

            Dim isAvailable As Boolean = UpdateService.IsUpdateAvailable(currentVersion, release.Version)
            SetUpdateNoticeVisible(isAvailable, release.TagName)
            If isAvailable Then
                AppendLog("Installer update available: " & release.TagName)
                If promptOnAvailable Then
                    Dim latestLabel As String = If(String.IsNullOrWhiteSpace(release.TagName), release.Version.ToString(), release.TagName)
                    Dim message As String = "A new OptiScaler Installer update is available." & Environment.NewLine &
                                            "Current: " & currentVersion.ToString() & Environment.NewLine &
                                            "Latest: " & latestLabel & Environment.NewLine & Environment.NewLine &
                                            "Do you want to update now?"
                    Dim result As DialogResult = MessageBox.Show(Me,
                                                                  message,
                                                                  "Update Available",
                                                                  MessageBoxButtons.YesNo,
                                                                  MessageBoxIcon.Information)
                    If result = DialogResult.Yes Then
                        Using dlg As New frmUpdate(release, currentVersion)
                            dlg.ShowDialog(Me)
                        End Using
                    Else
                        AppendLog("Startup update prompt dismissed by user.")
                    End If
                End If
            Else
                AppendLog("Installer is up to date.")
            End If
        Catch ex As Exception
            AppendLog("Update check failed: " & ex.Message)
            SetUpdateNoticeVisible(False, "")
            ErrorLogger.Log(ex, "MainForm.CheckForUpdatesSilent")
        End Try
    End Function

    Private Sub SetUpdateNoticeVisible(isVisible As Boolean, latestTag As String)
        ' Toggle the update hint label on the Settings tab.
        If lblUpdateNotice Is Nothing Then
            Return
        End If

        If lblUpdateNotice.InvokeRequired Then
            lblUpdateNotice.BeginInvoke(New Action(Of Boolean, String)(AddressOf SetUpdateNoticeVisible), isVisible, latestTag)
            Return
        End If

        lblUpdateNotice.Visible = isVisible
        If isVisible Then
            Dim tagText As String = If(String.IsNullOrWhiteSpace(latestTag), "", " (" & latestTag & ")")
            lblUpdateNotice.Text = "Update available" & tagText
            lblUpdateNotice.ForeColor = Color.Goldenrod
        End If

        If btnCheckForUpdates IsNot Nothing Then
            btnCheckForUpdates.Text = If(isVisible, "Update now", "Check for updates")
        End If

        PositionUpdateNotice()
    End Sub

    Private Function ResolveInstallerRepositoryUrl(release As UpdateReleaseInfo) As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Dim repoUrl As String = ConvertApiReleaseUrlToRepo(If(settings IsNot Nothing, settings.InstallerReleaseUrl, String.Empty))
        If String.IsNullOrWhiteSpace(repoUrl) AndAlso release IsNot Nothing Then
            repoUrl = ConvertHtmlReleaseUrlToRepo(release.HtmlUrl)
        End If
        If String.IsNullOrWhiteSpace(repoUrl) Then
            repoUrl = "https://github.com/Wimukthi/OptiScalerInstaller"
        End If
        Return repoUrl
    End Function

    Private Function ConvertApiReleaseUrlToRepo(url As String) As String
        If String.IsNullOrWhiteSpace(url) Then
            Return String.Empty
        End If

        Try
            Dim uri As New Uri(url)
            If Not uri.Host.Equals("api.github.com", StringComparison.OrdinalIgnoreCase) Then
                Return ConvertHtmlReleaseUrlToRepo(url)
            End If

            Dim parts As String() = uri.AbsolutePath.Trim("/"c).Split("/"c)
            If parts.Length >= 3 AndAlso parts(0).Equals("repos", StringComparison.OrdinalIgnoreCase) Then
                Return $"https://github.com/{parts(1)}/{parts(2)}"
            End If
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.ConvertApiReleaseUrlToRepo")
        End Try

        Return String.Empty
    End Function

    Private Function ConvertHtmlReleaseUrlToRepo(url As String) As String
        If String.IsNullOrWhiteSpace(url) Then
            Return String.Empty
        End If

        Try
            Dim uri As New Uri(url)
            If Not uri.Host.EndsWith("github.com", StringComparison.OrdinalIgnoreCase) Then
                Return String.Empty
            End If

            Dim parts As String() = uri.AbsolutePath.Trim("/"c).Split("/"c)
            If parts.Length >= 2 Then
                Return $"https://github.com/{parts(0)}/{parts(1)}"
            End If
        Catch ex As Exception
            ErrorLogger.Log(ex, "MainForm.ConvertHtmlReleaseUrlToRepo")
        End Try

        Return String.Empty
    End Function

    Private Async Function ExportDiagnosticsAsync() As Task
        Dim dialog As New SaveFileDialog() With {
            .Filter = "Diagnostics zip (*.zip)|*.zip|All files (*.*)|*.*",
            .Title = "Save diagnostics package",
            .FileName = "OptiScalerInstaller-diagnostics-" & DateTime.Now.ToString("yyyyMMdd-HHmmss") & ".zip"
        }

        If dialog.ShowDialog(Me) <> DialogResult.OK Then
            Return
        End If

        Dim tempRoot As String = Path.Combine(Path.GetTempPath(), "OptiScalerInstallerDiagnostics", Guid.NewGuid().ToString("N"))
        Dim logText As String = txtLog.Text
        Dim settingsPath As String = AppSettings.GetSettingsPath()
        Dim errorPath As String = Path.Combine(Application.StartupPath, "Errors", "Error_Log.txt")
        Dim detectedSnapshot As List(Of DiagnosticsDetectedGame) = BuildDiagnosticsDetectedGames()
        Dim detectedJson As String = JsonSerializer.Serialize(detectedSnapshot, New JsonSerializerOptions With {.WriteIndented = True})
        Dim systemInfo As String = BuildSystemInfo(detectedSnapshot.Count)

        Try
            Await Task.Run(Sub()
                               Directory.CreateDirectory(tempRoot)
                               File.WriteAllText(Path.Combine(tempRoot, "log_output.txt"), logText)
                               File.WriteAllText(Path.Combine(tempRoot, "system_info.txt"), systemInfo)
                               File.WriteAllText(Path.Combine(tempRoot, "detected_games.json"), detectedJson)

                               If File.Exists(settingsPath) Then
                                   File.Copy(settingsPath, Path.Combine(tempRoot, "settings.json"), True)
                               End If

                               If File.Exists(errorPath) Then
                                   File.Copy(errorPath, Path.Combine(tempRoot, "error_log.txt"), True)
                               End If
                           End Sub)

            If File.Exists(dialog.FileName) Then
                File.Delete(dialog.FileName)
            End If

            ZipFile.CreateFromDirectory(tempRoot, dialog.FileName, CompressionLevel.Optimal, False)
            AppendLog("Diagnostics exported: " & dialog.FileName)
            MessageBox.Show(Me, "Diagnostics package saved.", "Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            AppendLog("Diagnostics export failed: " & ex.Message)
            ErrorLogger.Log(ex, "MainForm.ExportDiagnostics")
            MessageBox.Show(Me, "Diagnostics export failed: " & ex.Message, "Diagnostics", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Try
                If Directory.Exists(tempRoot) Then
                    Directory.Delete(tempRoot, True)
                End If
            Catch ex As Exception
                ErrorLogger.Log(ex, "MainForm.ExportDiagnosticsCleanup")
            End Try
        End Try
    End Function

    Private Function BuildDiagnosticsDetectedGames() As List(Of DiagnosticsDetectedGame)
        Dim snapshot As New List(Of DiagnosticsDetectedGame)()

        If detectedGames Is Nothing Then
            Return snapshot
        End If

        For Each game As DetectedGame In detectedGames
            If game Is Nothing Then
                Continue For
            End If

            Dim key As String = NameNormalization.NormalizeRelaxedName(game.DisplayName)
            Dim info As OptiScalerInstallInfo = Nothing
            If detectedInstallLookup IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(key) Then
                detectedInstallLookup.TryGetValue(key, info)
            End If

            If info Is Nothing AndAlso Not String.IsNullOrWhiteSpace(game.InstallDir) Then
                info = OptiScalerInstallDetector.Detect(game.InstallDir)
            End If

            snapshot.Add(New DiagnosticsDetectedGame With {
                .DisplayName = game.DisplayName,
                .Platform = game.Platform,
                .InstallDir = game.InstallDir,
                .SourceName = game.SourceName,
                .AntiCheat = game.AntiCheat,
                .OptiScalerInstalled = If(info IsNot Nothing, info.IsInstalled, False),
                .OptiScalerVersion = If(info IsNot Nothing, info.Version, ""),
                .OptiScalerSource = If(info IsNot Nothing, info.Source, "")
            })
        Next

        Return snapshot
    End Function

    Private Function BuildSystemInfo(detectedCount As Integer) As String
        Dim version As Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        Dim build As Integer = If(version IsNot Nothing AndAlso version.Build >= 0, version.Build, 0)
        Dim revision As Integer = If(version IsNot Nothing AndAlso version.Revision >= 0, version.Revision, 0)
        Dim versionText As String = If(version Is Nothing, "Unknown", $"{version.Major}.{version.Minor}.{build}.{revision}")

        Dim sb As New StringBuilder()
        sb.AppendLine("OptiScaler Installer Diagnostics")
        sb.AppendLine("Generated: " & DateTime.Now.ToString("u"))
        sb.AppendLine("AppVersion: " & versionText)
        sb.AppendLine("OS: " & Environment.OSVersion.ToString())
        sb.AppendLine("Is64BitOS: " & Environment.Is64BitOperatingSystem)
        sb.AppendLine(".NET: " & Environment.Version.ToString())
        sb.AppendLine("DetectedGames: " & detectedCount)
        sb.AppendLine("Theme: " & ThemeSettings.GetPreferredColorMode().ToString())
        sb.AppendLine("GPU Adapters: " & String.Join("; ", GetGpuAdapterNames()))
        Return sb.ToString()
    End Function

    Private Sub LogVerificationReport(report As InstallVerificationReport)
        If report Is Nothing Then
            AppendLog("Post-install verification skipped.")
            Return
        End If

        AppendLog($"Post-install verification: {report.Passed.Count} passed, {report.Warnings.Count} warnings, {report.Errors.Count} errors.")

        For Each message As String In report.Passed
            AppendLog("  [OK] " & message)
        Next
        For Each message As String In report.Warnings
            AppendLog("  [WARN] " & message)
        Next
        For Each message As String In report.Errors
            AppendLog("  [ERR] " & message)
        Next
    End Sub

    Private Function FormatStatusDirectory(pathValue As String) As String
        If String.IsNullOrWhiteSpace(pathValue) Then
            Return "(scanning...)"
        End If

        Dim normalized As String = pathValue.Trim().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        Const maxLength As Integer = 96
        If normalized.Length <= maxLength Then
            Return normalized
        End If

        Return "..." & normalized.Substring(normalized.Length - (maxLength - 3))
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

        toolTip.SetToolTip(DarkThemeCheckBox, "Switch between dark and light UI colors. This only changes appearance, not install behavior. Restart when prompted so all controls redraw correctly.")
        toolTip.SetToolTip(tabMain, "Main pages of the installer. Typical flow: Game Detection -> Install -> Add-ons -> optional FSR4 INT8 -> Settings.")

        toolTip.SetToolTip(txtGameSearch, "Type part of a game name to filter the compatibility table instantly. Search is case-insensitive and does not modify any files.")
        toolTip.SetToolTip(btnScanDetected, "Runs the full scan pipeline: launcher/registry detection first, then drive-scan augmentation. You will be prompted to choose drives each run.")
        toolTip.SetToolTip(btnDeepScanDrives, "Manually add a game by selecting its executable. The installer matches it to the compatibility list and persists it for future sessions.")
        toolTip.SetToolTip(btnUseDetected, "Use selected game for advanced install setup. Switches to Install tab and fills Game EXE/Game folder without starting installation.")
        toolTip.SetToolTip(chkHideNonDetected, "When enabled, only games found on this PC are shown. Disable to view the full supported list again.")
        toolTip.SetToolTip(btnRefreshCompatibility, "Download the latest compatibility list from the configured URL and refresh this table.")
        toolTip.SetToolTip(btnOpenWiki, "Open the wiki page for the currently selected compatibility entry using the configured wiki base URL.")
        toolTip.SetToolTip(btnCompatOpenFolder, "Open the install folder of the selected detected game in Windows Explorer.")
        toolTip.SetToolTip(btnCompatEditIni, "Use the selected detected game as target and open its OptiScaler.ini in the editor.")
        toolTip.SetToolTip(btnCompatInstallUpdate, "Quick install OptiScaler to the selected detected game using saved default install settings.")
        toolTip.SetToolTip(btnCompatUninstall, "Use the selected detected game as target and run OptiScaler uninstall.")
        toolTip.SetToolTip(btnCompatInstallPatcher, "Use the selected detected game as target and install/update OptiPatcher when supported.")
        toolTip.SetToolTip(btnCompatRemovePatcher, "Use the selected detected game as target and remove OptiPatcher.")
        toolTip.SetToolTip(btnCompatCopyInfo, "Copy selected game detection/install information to clipboard.")
        toolTip.SetToolTip(lvCompatibility, "Master game list. Columns show detection state, OptiScaler/OptiPatcher status, platform, anti-cheat hint, and install path. Double-click a detected row to prefill install target.")

        toolTip.SetToolTip(txtGameExe, "Full path to the game executable you want to patch. Prefer the real game .exe in the binaries folder, not launcher/setup/uninstall executables.")
        toolTip.SetToolTip(btnBrowseGameExe, "Browse to a game .exe file and auto-fill related fields.")
        toolTip.SetToolTip(txtGameFolder, "Destination folder where OptiScaler files are copied. Usually auto-filled from Game EXE and should point to the folder that contains the game binary.")
        toolTip.SetToolTip(rbStable, "Download and install the latest official stable OptiScaler release.")
        toolTip.SetToolTip(rbNightly, "Use the alternate release URL source (for forks/mirrors/custom feeds). Leave alternate URL empty to disable this source.")
        toolTip.SetToolTip(rbLocal, "Install from a local OptiScaler archive file (.7z), without downloading from the internet.")
        toolTip.SetToolTip(txtLocalArchive, "Path to the local OptiScaler .7z archive used when Local source is selected.")
        toolTip.SetToolTip(btnBrowseArchive, "Browse to a local OptiScaler archive file.")
        toolTip.SetToolTip(btnRefreshReleases, "Refresh release metadata for OptiScaler sources so version labels and download targets are up to date.")
        toolTip.SetToolTip(cmbHookName, "Select which filename OptiScaler.dll will be renamed to (for example dxgi.dll). Choose the hook the game actually loads.")
        toolTip.SetToolTip(rbGpuNvidia, "Use NVIDIA path defaults. Best for NVIDIA cards and standard DLSS pipelines.")
        toolTip.SetToolTip(rbGpuAmdIntel, "Use AMD/Intel path defaults. Enables options needed for DLSS input spoofing scenarios.")
        toolTip.SetToolTip(chkDlssInputs, "Enable DLSS input spoofing for AMD/Intel mode. If disabled, installer writes Dxgi=false in OptiScaler.ini.")
        toolTip.SetToolTip(cmbFgType, "Frame generation mode preference written during install. Auto keeps default behavior; other values force specific FG handling.")
        toolTip.SetToolTip(cmbConflictMode, "How to handle existing files in the target folder (for example backup/overwrite/skip depending on selected mode).")
        toolTip.SetToolTip(btnInstall, "Install or update OptiScaler into the selected game folder using current options from Install and Add-ons.")
        toolTip.SetToolTip(btnUninstall, "Remove OptiScaler from the selected game folder using installer manifest data, with fallback cleanup paths when possible.")
        toolTip.SetToolTip(btnOpenGameFolder, "Open the currently selected game folder in Windows Explorer.")
        toolTip.SetToolTip(btnEditIni, "Open a feature-rich editor for OptiScaler.ini in the selected game folder. Includes visual fields, raw mode, validation, backup, and atomic save.")
        toolTip.SetToolTip(chkInstallOptiPatcher, "If enabled, OptiPatcher is installed automatically right after OptiScaler install. Only available for supported detected games.")
        toolTip.SetToolTip(lblInstallOptiPatcherStatus, "Read-only status telling whether OptiPatcher auto-install is available for the current target.")

        toolTip.SetToolTip(chkEnableReshade, "Enable ReShade integration for this install and copy the selected ReShade DLL.")
        toolTip.SetToolTip(txtReshadeDll, "Path to the ReShade DLL file that will be copied during install when ReShade is enabled.")
        toolTip.SetToolTip(btnBrowseReshade, "Browse for a ReShade DLL file.")

        toolTip.SetToolTip(chkEnableSpecialK, "Enable Special K loader integration for this install.")
        toolTip.SetToolTip(txtSpecialKDll, "Path to SpecialK64.dll to copy into the game folder when Special K integration is enabled.")
        toolTip.SetToolTip(btnBrowseSpecialK, "Browse for SpecialK64.dll.")
        toolTip.SetToolTip(chkCreateSpecialKMarker, "Create the SpecialK.marker file to force Special K loading in games that require marker-based activation.")

        toolTip.SetToolTip(chkLoadAsiPlugins, "Enable ASI plugin loading in OptiScaler.ini. Installer can auto-create a plugins folder so ASI plugins are discovered.")
        toolTip.SetToolTip(txtPluginsPath, "Folder used for ASI plugins. If empty and ASI loading is enabled, installer uses/creates <GameFolder>\\plugins.")
        toolTip.SetToolTip(btnBrowsePluginsPath, "Browse for an ASI plugins folder.")
        toolTip.SetToolTip(cmbOptiPatcherSource, "Choose where OptiPatcher.asi comes from: online release source or local file.")
        toolTip.SetToolTip(btnOptiPatcherRefresh, "Refresh OptiPatcher release information for selected source.")
        toolTip.SetToolTip(lblOptiPatcherRelease, "Shows currently loaded OptiPatcher release tag/version and size for the selected source.")
        toolTip.SetToolTip(txtOptiPatcherLocalFile, "Local OptiPatcher.asi file path used when source is Local .asi.")
        toolTip.SetToolTip(btnBrowseOptiPatcherLocal, "Browse for a local OptiPatcher.asi file.")
        toolTip.SetToolTip(btnInstallOptiPatcher, "Install or update OptiPatcher.asi into the active plugin path and ensure ASI loading is enabled in OptiScaler.ini.")
        toolTip.SetToolTip(btnRemoveOptiPatcher, "Remove OptiPatcher.asi from the active plugin path. If installer backup exists, it is restored.")
        toolTip.SetToolTip(lblOptiPatcherStatus, "Read-only OptiPatcher detection/support state for the selected game folder.")

        toolTip.SetToolTip(txtNvngxDll, "Optional nvngx_dlss.dll source file. Use this when a game requires nvngx_dlss.dll but does not ship it.")
        toolTip.SetToolTip(btnBrowseNvngx, "Browse for nvngx_dlss.dll.")

        toolTip.SetToolTip(txtNukemDll, "Path to dlssg_to_fsr3_amd_is_better.dll (Nukem frame-generation helper).")
        toolTip.SetToolTip(btnBrowseNukemDll, "Browse for Nukem FG DLL.")

        toolTip.SetToolTip(txtFakenvapiFolder, "Folder that contains nvapi64.dll and fakenvapi.ini for AMD/Intel spoofing scenarios.")
        toolTip.SetToolTip(btnBrowseFakenvapiFolder, "Browse for the Fakenvapi package folder.")

        toolTip.SetToolTip(txtFsr4PackageFolder, "Folder containing experimental FSR4 INT8 files (for example amdxcffx64.dll) that will be copied to selected game.")
        toolTip.SetToolTip(btnBrowseFsr4PackageFolder, "Browse for local FSR4 INT8 package folder.")
        toolTip.SetToolTip(txtFsr4TargetGameFolder, "Target game folder for experimental package apply/remove operations.")
        toolTip.SetToolTip(btnFsr4PickGame, "Jump to Install tab to pick or change the active game target, then return here.")
        toolTip.SetToolTip(btnFsr4ScanDetectedGames, "Run the unified detection pipeline for this tab and choose which drives to include in deep-scan augmentation.")
        toolTip.SetToolTip(btnFsr4UseSelectedGame, "Use selected row from detected games list as FSR4 INT8 target folder.")
        toolTip.SetToolTip(btnFsr4BrowseGameExe, "Manual fallback: choose a game executable directly if automatic detection misses it.")
        toolTip.SetToolTip(lvFsr4DetectedGames, "Detected supported games available for FSR4 INT8 targeting. INT8 column shows per-game package status. Select one and use it, or double-click.")
        toolTip.SetToolTip(lblFsr4DetectedGames, "Shows detected-game count for the experimental tab picker.")
        toolTip.SetToolTip(chkFsr4EnableUpdate, "When checked, installer sets Fsr4Update=true in OptiScaler.ini during apply.")
        toolTip.SetToolTip(chkFsr4EnableAgility, "When checked, installer sets FsrAgilitySDKUpgrade=true to help selected Windows 10 titles.")
        toolTip.SetToolTip(btnFsr4Apply, "Copy experimental package files into target game folder, write selected INI keys, and create installer manifest metadata.")
        toolTip.SetToolTip(btnFsr4Remove, "Remove installer-managed experimental files and restore backed-up files/INI keys where available.")
        toolTip.SetToolTip(btnFsr4RefreshStatus, "Re-check whether experimental package appears installed and whether it is installer-managed.")
        toolTip.SetToolTip(lblFsr4Status, "Read-only status summary for experimental package state in the selected target.")

        toolTip.SetToolTip(txtLog, "Read-only operation log. Every action, warning, and error is written here with timestamp for troubleshooting.")
        toolTip.SetToolTip(grpLog, "Live log panel for installer activity.")

        toolTip.SetToolTip(txtCompatibilityListUrl, "Direct URL to compatibility markdown list. Refresh lists downloads this file and parses supported games from it.")
        toolTip.SetToolTip(txtWikiBaseUrl, "Base wiki URL used when opening per-game pages from Game Detection tab.")
        toolTip.SetToolTip(txtStableReleaseUrl, "GitHub API endpoint for latest stable OptiScaler release metadata.")
        toolTip.SetToolTip(txtNightlyReleaseUrl, "GitHub API endpoint for alternate OptiScaler release metadata. Leave empty to disable alternate source checks.")
        toolTip.SetToolTip(txtInstallerReleaseUrl, "GitHub API endpoint for OptiScaler Installer update checks.")
        toolTip.SetToolTip(chkAutoRefreshCompatibilityOnStartup, "If enabled, compatibility list is refreshed automatically on app start.")
        toolTip.SetToolTip(chkAutoCheckInstallerUpdates, "If enabled, installer checks for updates at startup. When a newer build is found, it shows a Yes/No update prompt and an in-app update notice.")
        toolTip.SetToolTip(chkShowExperimentalTabOnUnsupportedGpu, "Force-show FSR4 INT8 experimental tab even when AMD RDNA GPU is not detected.")
        toolTip.SetToolTip(txtDefaultIniPath, "Optional OptiScaler.ini template file to apply automatically on future installs.")
        toolTip.SetToolTip(btnBrowseDefaultIni, "Browse for default OptiScaler.ini template file.")
        toolTip.SetToolTip(cmbDefaultIniMode, "Select how installer should apply default INI template: disabled, merge, or replace behavior based on selected mode.")
        toolTip.SetToolTip(cmbDefaultPreset, "Preset bundle for default install options. Choose Custom to fine-tune each default below.")
        toolTip.SetToolTip(cmbDefaultHookName, "Default hook filename that new installs should use.")
        toolTip.SetToolTip(cmbDefaultGpuVendor, "Default GPU path choice for new installs. Auto uses detected adapter.")
        toolTip.SetToolTip(chkDefaultDlssInputs, "Default state for DLSS input spoofing option on future installs.")
        toolTip.SetToolTip(cmbDefaultFgType, "Default frame generation mode for future installs.")
        toolTip.SetToolTip(cmbDefaultConflictMode, "Default existing-file conflict behavior for future installs.")
        toolTip.SetToolTip(btnApplyDefaults, "Apply these default options to current Install tab controls immediately.")
        toolTip.SetToolTip(btnSaveSettings, "Save current settings to JSON in the application folder.")
        toolTip.SetToolTip(btnReloadSettings, "Reload settings from disk and discard unsaved edits in current UI.")
        toolTip.SetToolTip(btnLoadDefaults, "Reset current settings UI to bundled defaults.")
        toolTip.SetToolTip(btnOpenSettingsFile, "Open the live settings JSON file in your default text editor.")
        toolTip.SetToolTip(btnCheckForUpdates, "Manually check whether a newer OptiScaler Installer release is available.")
        toolTip.SetToolTip(btnAbout, "Open About window with current version, latest release date, repo links, author, and sponsorship link.")
        toolTip.SetToolTip(btnExportDiagnostics, "Create a diagnostics zip (logs, settings, detection snapshot) for troubleshooting and issue reports.")
        toolTip.SetToolTip(lblInstalledStatus, "Read-only OptiScaler install status for current target folder, including detected version when available.")
    End Sub

    Private Sub LoadSettingsUi()
        Dim settings As AppSettingsModel = AppSettings.Load()
        ApplySettingsToUi(settings)
        ApplyDefaultInstallOptionsFromSettings(settings, False)
        UpdateExperimentalStatus()
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

    Private Async Function RunDetectionAsync(isAuto As Boolean,
                                             Optional selectedDriveRoots As IEnumerable(Of String) = Nothing,
                                             Optional isInitialDeepScan As Boolean = False) As Task
        Dim manualDriveRoots As List(Of String) = New List(Of String)()
        If selectedDriveRoots IsNot Nothing Then
            manualDriveRoots = selectedDriveRoots.
                Where(Function(path) Not String.IsNullOrWhiteSpace(path)).
                Select(Function(path) NormalizePathSafe(path)).
                Where(Function(path) Not String.IsNullOrWhiteSpace(path)).
                Distinct(StringComparer.OrdinalIgnoreCase).
                ToList()
        End If

        Dim isDeepScan As Boolean = manualDriveRoots.Count > 0
        Dim label As String = If(isAuto, "Auto detection", "Detection")
        If isInitialDeepScan Then
            label = "Initial deep scan"
        End If
        Dim progressSync As New Object()
        Dim lastProgressValue As Integer = -1
        Dim lastStatusMessage As String = ""

        Try
            btnScanDetected.Enabled = False
            btnDeepScanDrives.Enabled = False
            btnUseDetected.Enabled = False
            toolDetectedLabel.Text = "Detected: scanning..."
            AppendLog(label & " started.")
            If isDeepScan AndAlso Not isInitialDeepScan Then
                AppendLog("Detection mode: launcher/registry + deep scan augmentation.")
            End If
            detectedGames.Clear()
            detectedLookup.Clear()
            detectedOptiPatcherLookup.Clear()

            SetStatus(label & " in progress...")
            If isDeepScan Then
                UpdateProgress(0)
                lastProgressValue = 0
            End If

            If allCompatibilityEntries Is Nothing OrElse allCompatibilityEntries.Count = 0 Then
                AppendLog("Detection skipped: compatibility list is empty.")
                toolDetectedLabel.Text = "Detected: none"
                SetStatus("Detection skipped.")
                Return
            End If

            EnsurePersistedDeepScanGamesLoaded()
            Dim prunedPersisted As List(Of DetectedGame) = MergeDetectedGames(persistedDeepScanGames, Nothing)
            If prunedPersisted.Count <> persistedDeepScanGames.Count Then
                persistedDeepScanGames = prunedPersisted
                DeepScanDetectionCacheService.Save(persistedDeepScanGames, AddressOf AppendLog)
            End If

            Dim launcherResults As List(Of DetectedGame) = Await Task.Run(Function() DetectionService.DetectSupportedGames(allCompatibilityEntries, AddressOf AppendLog))
            Dim results As List(Of DetectedGame) = launcherResults

            If isDeepScan Then
                AppendLog("Deep scan roots: " & String.Join("; ", manualDriveRoots))

                Dim progressCallback As Action(Of DetectionService.DeepScanProgressInfo) =
                    Sub(info)
                        If info Is Nothing Then
                            Return
                        End If

                        Dim totalRoots As Integer = Math.Max(1, info.TotalRoots)
                        Dim completedRoots As Integer = Math.Min(totalRoots, Math.Max(0, info.RootsCompleted))
                        Dim folderBudget As Integer = Math.Max(1, info.MaxFoldersPerRoot)
                        Dim rootProgress As Double = Math.Min(1.0R, Math.Max(0.0R, CDbl(info.ScannedFoldersInRoot) / CDbl(folderBudget)))

                        Dim progressValue As Integer = CInt(Math.Truncate(((completedRoots + rootProgress) / CDbl(totalRoots)) * 100.0R))
                        If completedRoots < totalRoots AndAlso progressValue > 99 Then
                            progressValue = 99
                        End If
                        progressValue = Math.Max(0, Math.Min(100, progressValue))

                        Dim currentDirectory As String = FormatStatusDirectory(info.CurrentDirectory)
                        Dim statusMessage As String = $"Deep scan [{completedRoots}/{totalRoots}]: {currentDirectory}"

                        Dim shouldUpdateProgress As Boolean = False
                        Dim shouldUpdateStatus As Boolean = False

                        SyncLock progressSync
                            If progressValue < lastProgressValue Then
                                progressValue = lastProgressValue
                            End If

                            If progressValue <> lastProgressValue Then
                                lastProgressValue = progressValue
                                shouldUpdateProgress = True
                            End If

                            If Not String.Equals(statusMessage, lastStatusMessage, StringComparison.Ordinal) Then
                                lastStatusMessage = statusMessage
                                shouldUpdateStatus = True
                            End If
                        End SyncLock

                        If shouldUpdateProgress Then
                            UpdateProgress(progressValue)
                        End If
                        If shouldUpdateStatus Then
                            SetStatus(statusMessage)
                        End If
                    End Sub

                Dim deepScanResults As List(Of DetectedGame) =
                    Await Task.Run(Function() DetectionService.DetectSupportedGamesByDriveScan(allCompatibilityEntries,
                                                                                                manualDriveRoots,
                                                                                                AddressOf AppendLog,
                                                                                                progressCallback))

                If deepScanResults IsNot Nothing AndAlso deepScanResults.Count > 0 Then
                    persistedDeepScanGames = MergeDetectedGames(persistedDeepScanGames, deepScanResults)
                    DeepScanDetectionCacheService.Save(persistedDeepScanGames, AddressOf AppendLog)
                End If

                Dim mergedBefore As Integer = If(launcherResults Is Nothing, 0, launcherResults.Count)
                results = MergeDetectedGames(launcherResults, deepScanResults)
                If results.Count > mergedBefore Then
                    AppendLog("Deep scan augmented launcher detection: +" & (results.Count - mergedBefore).ToString() & " game(s).")
                End If

                UpdateProgress(100)
                SetStatus(label & " finished.")
            Else
                SetStatus(label & " finished.")
            End If

            If persistedDeepScanGames IsNot Nothing AndAlso persistedDeepScanGames.Count > 0 Then
                Dim mergedCountBefore As Integer = If(results Is Nothing, 0, results.Count)
                results = MergeDetectedGames(results, persistedDeepScanGames)
                If results.Count > mergedCountBefore Then
                    AppendLog("Merged persisted deep-scan detections: +" & (results.Count - mergedCountBefore).ToString() & " game(s).")
                End If
            End If

            detectedGames = results
            detectedInstallLookup = Await Task.Run(Function() BuildInstallStatusLookup(results))
            detectedLookup = BuildDetectedLookup(results, detectedInstallLookup)
            detectedOptiPatcherLookup = Await Task.Run(Function() BuildOptiPatcherStatusLookup(results))
            ApplyCompatibilityFilter()
            UpdateExperimentalDetectedGamesList()
            UpdateDetectedStatus()
            Dim installedCount As Integer = 0
            Dim patcherInstalledCount As Integer = 0
            Dim antiCheatCount As Integer = 0
            For Each info As OptiScalerInstallInfo In detectedInstallLookup.Values
                If info IsNot Nothing AndAlso info.IsInstalled Then
                    installedCount += 1
                End If
            Next
            For Each info As OptiPatcherInstallInfo In detectedOptiPatcherLookup.Values
                If info IsNot Nothing AndAlso info.IsInstalled Then
                    patcherInstalledCount += 1
                End If
            Next
            For Each game As DetectedGame In detectedGames
                If game IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(game.AntiCheat) Then
                    antiCheatCount += 1
                End If
            Next
            AppendLog("OptiScaler installed in " & installedCount & " detected game(s).")
            AppendLog("OptiPatcher installed in " & patcherInstalledCount & " detected game(s).")
            AppendLog("Anti-cheat flagged in " & antiCheatCount & " detected game(s).")
            AppendLog(label & " finished: " & detectedLookup.Count & " supported game(s) detected.")
        Catch ex As Exception
            AppendLog(label & " failed: " & ex.Message)
            toolDetectedLabel.Text = "Detected: error"
            SetStatus(label & " failed.")
            UpdateExperimentalDetectedGamesList()
            ErrorLogger.Log(ex, "MainForm.DetectGames")
        Finally
            btnScanDetected.Enabled = True
            btnDeepScanDrives.Enabled = True
            If isDeepScan Then
                UpdateProgress(0)
            End If
        End Try
    End Function

    Private Sub ApplySettingsToUi(settings As AppSettingsModel)
        If settings Is Nothing Then
            Return
        End If

        loadingSettingsUi = True
        Try
            txtCompatibilityListUrl.Text = settings.CompatibilityListUrl
            txtWikiBaseUrl.Text = settings.WikiBaseUrl
            txtStableReleaseUrl.Text = settings.StableReleaseUrl
            txtNightlyReleaseUrl.Text = settings.NightlyReleaseUrl
            txtInstallerReleaseUrl.Text = settings.InstallerReleaseUrl
            chkAutoRefreshCompatibilityOnStartup.Checked = If(settings.AutoRefreshCompatibilityOnStartup.HasValue, settings.AutoRefreshCompatibilityOnStartup.Value, True)
            chkAutoCheckInstallerUpdates.Checked = If(settings.AutoCheckInstallerUpdates.HasValue, settings.AutoCheckInstallerUpdates.Value, True)
            chkHideNonDetected.Checked = If(settings.HideNonDetectedGames.HasValue, settings.HideNonDetectedGames.Value, False)
            chkShowExperimentalTabOnUnsupportedGpu.Checked = If(settings.ShowExperimentalTabOnUnsupportedGpu.HasValue, settings.ShowExperimentalTabOnUnsupportedGpu.Value, False)
            txtDefaultIniPath.Text = settings.DefaultIniPath
            cmbDefaultIniMode.SelectedIndex = GetDefaultIniModeIndex(ParseDefaultIniMode(settings.DefaultIniMode))
            txtFsr4PackageFolder.Text = If(settings.ExperimentalFsr4PackageFolder, "")
            chkFsr4EnableUpdate.Checked = If(settings.ExperimentalFsr4EnableUpdate.HasValue, settings.ExperimentalFsr4EnableUpdate.Value, True)
            chkFsr4EnableAgility.Checked = If(settings.ExperimentalFsr4EnableAgility.HasValue, settings.ExperimentalFsr4EnableAgility.Value, False)
            cmbOptiPatcherSource.SelectedIndex = GetOptiPatcherSourceIndex(settings.OptiPatcherPreferredSource)
            txtOptiPatcherLocalFile.Text = If(settings.OptiPatcherLocalPath, "")
            _settingDefaultsPreset = True
            cmbDefaultPreset.SelectedIndex = GetDefaultPresetIndex(settings.DefaultPreset)
            cmbDefaultHookName.SelectedIndex = GetDefaultHookIndex(settings.DefaultHookName)
            cmbDefaultGpuVendor.SelectedIndex = GetDefaultGpuVendorIndex(settings.DefaultGpuVendor)
            chkDefaultDlssInputs.Checked = If(settings.DefaultDlssInputs.HasValue, settings.DefaultDlssInputs.Value, True)
            cmbDefaultFgType.SelectedIndex = GetDefaultFrameGenerationIndex(settings.DefaultFrameGeneration)
            cmbDefaultConflictMode.SelectedIndex = GetDefaultConflictModeIndex(settings.DefaultConflictMode)
        Finally
            _settingDefaultsPreset = False
            loadingSettingsUi = False
        End Try

        ToggleOptiPatcherLocalFile()
        UpdateOptiPatcherReleaseLabel()
        UpdateExperimentalTabAvailability(False)
    End Sub

    Private Sub ApplyDefaultInstallOptionsFromSettings(settings As AppSettingsModel, Optional logAction As Boolean = True)
        If settings Is Nothing Then
            Return
        End If

        ApplyDefaultInstallOptions(settings.DefaultHookName,
                                   settings.DefaultGpuVendor,
                                   settings.DefaultDlssInputs,
                                   settings.DefaultFrameGeneration,
                                   settings.DefaultConflictMode,
                                   logAction)
    End Sub

    Private Sub ApplyDefaultInstallOptionsFromUi(Optional logAction As Boolean = True)
        Dim hookName As String = GetDefaultHookValue()
        Dim gpuVendor As String = GetDefaultGpuVendorValue()
        Dim fgToken As String = GetDefaultFrameGenerationToken(cmbDefaultFgType.SelectedIndex)
        Dim conflictToken As String = GetDefaultConflictModeToken(cmbDefaultConflictMode.SelectedIndex)
        Dim dlssInputs As Boolean? = chkDefaultDlssInputs.Checked
        ApplyDefaultInstallOptions(hookName, gpuVendor, dlssInputs, fgToken, conflictToken, logAction)
    End Sub

    Private Sub ApplyDefaultInstallOptions(hookName As String,
                                           gpuVendor As String,
                                           dlssInputs As Boolean?,
                                           frameGeneration As String,
                                           conflictMode As String,
                                           Optional logAction As Boolean = True)
        Dim hookIndex As Integer = GetHookIndex(cmbHookName, hookName)
        If hookIndex >= 0 Then
            cmbHookName.SelectedIndex = hookIndex
        End If

        Select Case GetDefaultGpuVendorIndex(gpuVendor)
            Case 1
                rbGpuNvidia.Checked = True
            Case 2
                rbGpuAmdIntel.Checked = True
            Case Else
                ApplyDetectedGpuVendor(logAction)
        End Select

        If dlssInputs.HasValue Then
            chkDlssInputs.Checked = dlssInputs.Value
        End If

        cmbFgType.SelectedIndex = GetDefaultFrameGenerationIndex(frameGeneration)
        cmbConflictMode.SelectedIndex = GetDefaultConflictModeIndex(conflictMode)

        If logAction Then
            AppendLog("Applied default install options.")
        End If
    End Sub

    Private Function GetHookIndex(combo As ComboBox, value As String) As Integer
        If combo Is Nothing OrElse combo.Items Is Nothing Then
            Return 0
        End If

        If String.IsNullOrWhiteSpace(value) Then
            Return 0
        End If

        Dim index As Integer = combo.Items.IndexOf(value)
        If index < 0 Then
            Return 0
        End If

        Return index
    End Function

    Private Function GetDefaultPresetIndex(value As String) As Integer
        If String.IsNullOrWhiteSpace(value) Then
            Return 0
        End If

        Dim normalized As String = value.Trim().ToLowerInvariant()
        If normalized.Contains("nvidia") Then
            Return 1
        End If
        If normalized.Contains("amd") OrElse normalized.Contains("intel") Then
            Return 2
        End If

        Return 0
    End Function

    Private Function GetDefaultHookIndex(value As String) As Integer
        Return GetHookIndex(cmbDefaultHookName, value)
    End Function

    Private Function GetDefaultGpuVendorIndex(value As String) As Integer
        If String.IsNullOrWhiteSpace(value) Then
            Return 0
        End If

        Dim normalized As String = value.Trim().ToLowerInvariant()
        If normalized.Contains("nvidia") Then
            Return 1
        End If
        If normalized.Contains("amd") OrElse normalized.Contains("intel") Then
            Return 2
        End If

        Return 0
    End Function

    Private Function GetDefaultFrameGenerationIndex(value As String) As Integer
        If String.IsNullOrWhiteSpace(value) Then
            Return 0
        End If

        Dim normalized As String = value.Trim().ToLowerInvariant()
        If normalized.Contains("optifg") Then
            Return 2
        End If
        If normalized.Contains("nukem") Then
            Return 3
        End If
        If normalized.Contains("none") Then
            Return 1
        End If

        Return 0
    End Function

    Private Function GetDefaultConflictModeIndex(value As String) As Integer
        If String.IsNullOrWhiteSpace(value) Then
            Return 0
        End If

        Dim normalized As String = value.Trim().ToLowerInvariant()
        If normalized.Contains("skip") Then
            Return 2
        End If
        If normalized.Contains("overwrite") AndAlso Not normalized.Contains("backup") Then
            Return 1
        End If

        Return 0
    End Function

    Private Function GetDefaultFrameGenerationToken(index As Integer) As String
        Select Case index
            Case 1
                Return "None"
            Case 2
                Return "OptiFG"
            Case 3
                Return "Nukem"
            Case Else
                Return "Auto"
        End Select
    End Function

    Private Function GetDefaultConflictModeToken(index As Integer) As String
        Select Case index
            Case 1
                Return "Overwrite"
            Case 2
                Return "Skip"
            Case Else
                Return "BackupAndOverwrite"
        End Select
    End Function

    Private Function GetDefaultPresetValue() As String
        If cmbDefaultPreset.SelectedItem Is Nothing Then
            Return "Custom"
        End If
        Return cmbDefaultPreset.SelectedItem.ToString()
    End Function

    Private Function GetDefaultHookValue() As String
        If cmbDefaultHookName.SelectedItem Is Nothing Then
            Return ""
        End If
        Return cmbDefaultHookName.SelectedItem.ToString()
    End Function

    Private Function GetDefaultGpuVendorValue() As String
        If cmbDefaultGpuVendor.SelectedItem Is Nothing Then
            Return "Auto"
        End If
        Return cmbDefaultGpuVendor.SelectedItem.ToString()
    End Function

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

    Private Class DiagnosticsDetectedGame
        Public Property DisplayName As String
        Public Property Platform As String
        Public Property InstallDir As String
        Public Property SourceName As String
        Public Property AntiCheat As String
        Public Property OptiScalerInstalled As Boolean
        Public Property OptiScalerVersion As String
        Public Property OptiScalerSource As String
    End Class

    Private Class CompatibilityRow
        Public Property Entry As CompatibilityEntry
        Public Property Detected As DetectedGame
        Public Property InstallInfo As OptiScalerInstallInfo
        Public Property OptiPatcherInfo As OptiPatcherInstallInfo
        Public Property IsRecentlyChanged As Boolean
    End Class
End Class
