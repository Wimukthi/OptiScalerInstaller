Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.IO
Imports System.Text.Json
Imports System.Linq

Public Class ExperimentalFsr4ApplyOptions
    ' User-selected options for applying the experimental FSR4 INT8 package.
    Public Property GameFolder As String
    Public Property PackageFolder As String
    Public Property ConflictMode As ConflictMode
    Public Property EnableFsr4Update As Boolean
    Public Property EnableAgilityUpgrade As Boolean
    ' Fsr4ForceEnableInt8 forces the INT8 model on GPUs OptiScaler does not auto-enable
    ' (RDNA2, unofficial APUs). Introduced in OptiScaler 0.9.4 / FFX 2.3.
    Public Property EnableForceInt8 As Boolean
End Class

Public Class ExperimentalFsr4Status
    ' Current installation state for the experimental FSR4 INT8 package.
    Public Property IsInstalled As Boolean
    Public Property IsManaged As Boolean
    Public Property Version As String
    Public Property Source As String
End Class

Public Class ExperimentalFsr4Manifest
    ' Tracks copied files and INI edits for reliable remove/restore behavior.
    Public Property InstallerVersion As String
    Public Property AppliedTimeUtc As DateTime
    Public Property GameFolder As String
    Public Property PackageFolder As String
    Public Property PackageVersion As String
    Public Property EnableFsr4Update As Boolean
    Public Property EnableAgilityUpgrade As Boolean
    Public Property EnableForceInt8 As Boolean
    Public Property InstalledFiles As List(Of String)
    Public Property BackupFiles As Dictionary(Of String, String)
    Public Property IniKeys As Dictionary(Of String, ExperimentalIniKeyState)
End Class

Public Class ExperimentalIniKeyState
    ' Captures an INI key state before we edit it so remove can restore it.
    Public Property Existed As Boolean
    Public Property Value As String
    ' Older manifests omit this field; restore those using the legacy flat-key behavior.
    Public Property SectionName As String
End Class

Public Module ExperimentalFsr4Service
    ' Applies/removes the optional FSR4 INT8 package using manifest-backed file tracking.
    Private Const ManifestFileName As String = "OptiScalerInstaller.experimental-fsr4.json"

    Private ReadOnly KnownMarkerFiles As String() = {
        "amdxcffx64.dll",
        "amdxc64.dll",
        "amdvkffx64.dll",
        "amd_fidelityfx_dx12.dll",
        "amd_fidelityfx_framegeneration_dx12.dll",
        "amd_fidelityfx_upscaler_dx12.dll",
        "amd_fidelityfx_vk.dll"
    }

    Private ReadOnly KnownMarkerFolders As String() = {
        "D3D12_Optiscaler"
    }

    Private ReadOnly ManagedIniKeys As String() = {
        "Fsr4Update",
        "FsrAgilitySDKUpgrade",
        "Fsr4ForceEnableInt8"
    }

    Private ReadOnly BlockedCopyExtensions As HashSet(Of String) = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        ".exe",
        ".bat",
        ".cmd",
        ".ps1",
        ".vbs",
        ".js",
        ".jse",
        ".msi",
        ".com",
        ".scr"
    }

    Public Function Detect(gameFolder As String) As ExperimentalFsr4Status
        Dim status As New ExperimentalFsr4Status()
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return status
        End If

        Dim manifest As ExperimentalFsr4Manifest = TryLoadManifest(gameFolder)
        If manifest IsNot Nothing AndAlso IsManifestValid(gameFolder, manifest) Then
            status.IsInstalled = True
            status.IsManaged = True
            status.Source = "Manifest"
            status.Version = GetVersionFromFolder(gameFolder)
            Return status
        End If

        Dim fsr4Enabled As Boolean = IsIniKeyEnabled(gameFolder, "Fsr4Update")
        Dim agilityEnabled As Boolean = IsIniKeyEnabled(gameFolder, "FsrAgilitySDKUpgrade")
        Dim hasMarkers As Boolean = HasKnownMarker(gameFolder)

        If hasMarkers AndAlso (fsr4Enabled OrElse agilityEnabled) Then
            status.IsInstalled = True
            status.IsManaged = False
            status.Source = "Manual markers"
            status.Version = GetVersionFromFolder(gameFolder)
        End If

        Return status
    End Function

    ' True when OptiScaler.ini has an explicit legacy or modern FSR4 selection.
    ' Automatic/no-override values are not treated as proof of a configured path.
    Public Function IsFsr4AlreadyEnabled(gameFolder As String) As Boolean
        If IsIniKeyEnabled(gameFolder, "Fsr4Update") OrElse IsIniKeyEnabled(gameFolder, "Fsr4ForceEnableInt8") Then
            Return True
        End If

        ' Current OptiScaler builds use explicit model/GPU selections instead of the
        ' older Fsr4Update/Fsr4ForceEnableInt8 toggles. GPU support zero means no
        ' override; model zero is the concrete FP8 selection checked below.
        Return IsExplicitModernFsr4Selection(gameFolder)
    End Function

    Public Function Apply(options As ExperimentalFsr4ApplyOptions, log As Action(Of String)) As ExperimentalFsr4Manifest
        ValidateApplyOptions(options)
        Dim normalizedGameFolder As String = NormalizePath(options.GameFolder)

        Dim resolvedPackageFolder As String = ResolvePackageFolder(options.PackageFolder)
        Dim filesToCopy As List(Of String) = EnumerateCopyFiles(resolvedPackageFolder)

        If filesToCopy.Count = 0 Then
            Throw New InvalidOperationException("No copyable files were found in the selected package folder.")
        End If

        ' Check the installed configuration before package files can replace it.
        ' Older packages expose the legacy controls; modern schemas do not.
        Dim iniPath As String = Path.Combine(normalizedGameFolder, "OptiScaler.ini")
        Dim preApplyIniStates As Dictionary(Of String, ExperimentalIniKeyState) = ValidateAndCaptureRequestedIniOptions(iniPath, options)
        ValidatePackageIniOverwrite(resolvedPackageFolder, iniPath, options.ConflictMode, preApplyIniStates.Keys)

        Dim manifest As New ExperimentalFsr4Manifest With {
            .InstallerVersion = GetInstallerVersion(),
            .AppliedTimeUtc = DateTime.UtcNow,
            .GameFolder = normalizedGameFolder,
            .PackageFolder = resolvedPackageFolder,
            .InstalledFiles = New List(Of String)(),
            .BackupFiles = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase),
            .IniKeys = New Dictionary(Of String, ExperimentalIniKeyState)(StringComparer.OrdinalIgnoreCase)
        }

        For Each sourcePath As String In filesToCopy
            Dim relative As String = Path.GetRelativePath(resolvedPackageFolder, sourcePath)
            If String.IsNullOrWhiteSpace(relative) Then
                Continue For
            End If

            Dim destination As String = ResolveManifestPath(normalizedGameFolder, relative)
            If String.IsNullOrWhiteSpace(destination) Then
                log?.Invoke("Skipping out-of-scope package entry: " & relative)
                Continue For
            End If

            If CopyFileWithConflict(sourcePath, destination, options.ConflictMode, manifest, log) Then
                log?.Invoke("Copied experimental file: " & relative)
            End If
        Next

        If File.Exists(iniPath) Then
            If options.EnableFsr4Update Then
                If UpdateManagedIniKey(iniPath, "Fsr4Update", "true", preApplyIniStates("Fsr4Update"), manifest) Then
                    manifest.EnableFsr4Update = True
                    log?.Invoke("Set Fsr4Update=true in OptiScaler.ini.")
                Else
                    log?.Invoke("Package OptiScaler.ini does not expose the legacy Fsr4Update key; it was not injected.")
                End If
            End If

            If options.EnableAgilityUpgrade Then
                If UpdateManagedIniKey(iniPath, "FsrAgilitySDKUpgrade", "true", preApplyIniStates("FsrAgilitySDKUpgrade"), manifest) Then
                    manifest.EnableAgilityUpgrade = True
                    log?.Invoke("Set FsrAgilitySDKUpgrade=true in OptiScaler.ini.")
                Else
                    log?.Invoke("Package OptiScaler.ini does not expose the legacy FsrAgilitySDKUpgrade key; it was not injected.")
                End If
            End If

            If options.EnableForceInt8 Then
                If UpdateManagedIniKey(iniPath, "Fsr4ForceEnableInt8", "true", preApplyIniStates("Fsr4ForceEnableInt8"), manifest) Then
                    manifest.EnableForceInt8 = True
                    log?.Invoke("Set Fsr4ForceEnableInt8=true in OptiScaler.ini.")
                Else
                    log?.Invoke("Package OptiScaler.ini does not expose the legacy Fsr4ForceEnableInt8 key; it was not injected.")
                End If
            End If
        Else
            log?.Invoke("OptiScaler.ini not found. INI options were skipped.")
        End If

        manifest.PackageVersion = GetVersionFromFolder(normalizedGameFolder)

        SaveManifest(normalizedGameFolder, manifest)
        log?.Invoke("Experimental FSR4 package apply complete.")
        Return manifest
    End Function

    Public Function Remove(gameFolder As String, log As Action(Of String)) As Boolean
        ' Remove only files recorded by our manifest and only within the selected game root.
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return False
        End If

        Dim gameRoot As String = NormalizePath(gameFolder)
        If String.IsNullOrWhiteSpace(gameRoot) Then
            Return False
        End If

        Dim manifest As ExperimentalFsr4Manifest = TryLoadManifest(gameFolder)
        If manifest Is Nothing Then
            Dim unmanaged As ExperimentalFsr4Status = Detect(gameFolder)
            If unmanaged IsNot Nothing AndAlso unmanaged.IsInstalled AndAlso Not unmanaged.IsManaged Then
                log?.Invoke("Experimental FSR4 markers found but no installer manifest exists. Remove is blocked to avoid deleting unmanaged files.")
            End If
            Return False
        End If

        Dim removedAny As Boolean = False

        If manifest.InstalledFiles IsNot Nothing Then
            For Each installedPath As String In manifest.InstalledFiles.OrderByDescending(Function(value) If(value, ""), StringComparer.OrdinalIgnoreCase)
                Dim resolvedInstalledPath As String = ResolveManifestPath(gameRoot, installedPath)
                If String.IsNullOrWhiteSpace(resolvedInstalledPath) Then
                    log?.Invoke("Skipping out-of-scope file entry from experimental manifest: " & If(installedPath, "(empty)"))
                    Continue For
                End If

                Try
                    If File.Exists(resolvedInstalledPath) Then
                        File.Delete(resolvedInstalledPath)
                        removedAny = True
                        log?.Invoke("Removed experimental file: " & Path.GetFileName(resolvedInstalledPath))
                    End If
                Catch ex As Exception
                    log?.Invoke("Failed to remove file " & resolvedInstalledPath & ": " & ex.Message)
                    ErrorLogger.Log(ex, "ExperimentalFsr4Service.Remove.DeleteFile")
                End Try
            Next
        End If

        If manifest.BackupFiles IsNot Nothing Then
            For Each entry As KeyValuePair(Of String, String) In manifest.BackupFiles
                Dim destination As String = ResolveManifestPath(gameRoot, entry.Key)
                Dim backupPath As String = ResolveManifestPath(gameRoot, entry.Value)
                If String.IsNullOrWhiteSpace(destination) OrElse String.IsNullOrWhiteSpace(backupPath) Then
                    log?.Invoke("Skipping out-of-scope backup entry from experimental manifest.")
                    Continue For
                End If

                Try
                    If File.Exists(backupPath) Then
                        If File.Exists(destination) Then
                            File.Delete(destination)
                        End If

                        Dim destinationDir As String = Path.GetDirectoryName(destination)
                        If Not String.IsNullOrWhiteSpace(destinationDir) Then
                            Directory.CreateDirectory(destinationDir)
                        End If

                        File.Move(backupPath, destination)
                        removedAny = True
                        log?.Invoke("Restored backup: " & Path.GetFileName(destination))
                    End If
                Catch ex As Exception
                    log?.Invoke("Failed to restore backup for " & destination & ": " & ex.Message)
                    ErrorLogger.Log(ex, "ExperimentalFsr4Service.Remove.RestoreBackup")
                End Try
            Next
        End If

        Dim iniPath As String = Path.Combine(gameRoot, "OptiScaler.ini")
        If File.Exists(iniPath) AndAlso manifest.IniKeys IsNot Nothing Then
            For Each keyName As String In ManagedIniKeys
                Dim state As ExperimentalIniKeyState = Nothing
                If manifest.IniKeys.TryGetValue(keyName, state) Then
                    RestoreManagedIniKey(iniPath, keyName, state)
                    removedAny = True
                    log?.Invoke("Restored INI key: " & keyName)
                End If
            Next
        End If

        Dim manifestPath As String = Path.Combine(gameRoot, ManifestFileName)
        Try
            If File.Exists(manifestPath) Then
                File.Delete(manifestPath)
                removedAny = True
            End If
        Catch ex As Exception
            log?.Invoke("Failed to remove experimental manifest: " & ex.Message)
            ErrorLogger.Log(ex, "ExperimentalFsr4Service.Remove.DeleteManifest")
        End Try

        Return removedAny
    End Function

    Private Function ValidateAndCaptureRequestedIniOptions(iniPath As String,
                                                            options As ExperimentalFsr4ApplyOptions) As Dictionary(Of String, ExperimentalIniKeyState)
        Dim requested As New List(Of String)()
        If options.EnableFsr4Update Then requested.Add("Fsr4Update")
        If options.EnableAgilityUpgrade Then requested.Add("FsrAgilitySDKUpgrade")
        If options.EnableForceInt8 Then requested.Add("Fsr4ForceEnableInt8")

        Dim states As New Dictionary(Of String, ExperimentalIniKeyState)(StringComparer.OrdinalIgnoreCase)
        If requested.Count = 0 Then Return states

        Dim document As IniDocument = IniDocument.Load(iniPath)
        Dim unsupported As List(Of String) = requested.
            Where(Function(keyName) Not document.ContainsKey("FSR", keyName)).
            ToList()
        If unsupported.Count > 0 Then
            Throw New InvalidOperationException(
                "The selected legacy FSR controls are not supported by this OptiScaler.ini schema: " &
                String.Join(", ", unsupported) & ". Use an OptiScaler build with matching legacy FSR settings or uncheck these options.")
        End If

        Dim duplicated As List(Of String) = requested.
            Where(Function(keyName) CountIniKeyInSection(iniPath, "FSR", keyName) > 1).
            ToList()
        If duplicated.Count > 0 Then
            Throw New InvalidOperationException(
                "The selected legacy FSR controls have duplicate entries in the [FSR] section: " &
                String.Join(", ", duplicated) & ". Resolve the duplicates in OptiScaler.ini before applying the package.")
        End If

        For Each keyName As String In requested
            states(keyName) = CaptureIniKeyState(iniPath, keyName)
        Next
        Return states
    End Function

    Private Sub ValidatePackageIniOverwrite(packageRoot As String,
                                            targetIniPath As String,
                                            conflictMode As ConflictMode,
                                            requestedKeys As IEnumerable(Of String))
        Dim packageIniPath As String = Path.Combine(packageRoot, "OptiScaler.ini")
        If Not File.Exists(packageIniPath) OrElse requestedKeys Is Nothing OrElse Not requestedKeys.Any() Then Return

        ' Skip mode leaves an existing target INI in place, so the package schema
        ' cannot affect the subsequent legacy-key edits in that case.
        If conflictMode = ConflictMode.Skip AndAlso File.Exists(targetIniPath) Then Return

        Dim duplicated As List(Of String) = requestedKeys.
            Where(Function(keyName) CountIniKeyInSection(packageIniPath, "FSR", keyName) > 1).
            ToList()
        If duplicated.Count > 0 Then
            Throw New InvalidOperationException(
                "The package OptiScaler.ini has duplicate entries in the [FSR] section for requested legacy controls: " &
                String.Join(", ", duplicated) & ". Resolve the duplicates before applying the package.")
        End If
    End Sub

    Private Sub ValidateApplyOptions(options As ExperimentalFsr4ApplyOptions)
        If options Is Nothing Then
            Throw New ArgumentNullException(NameOf(options))
        End If

        If String.IsNullOrWhiteSpace(options.GameFolder) OrElse Not Directory.Exists(options.GameFolder) Then
            Throw New InvalidOperationException("Game folder not found.")
        End If

        If String.IsNullOrWhiteSpace(options.PackageFolder) OrElse Not Directory.Exists(options.PackageFolder) Then
            Throw New InvalidOperationException("Experimental package folder not found.")
        End If
    End Sub

    Private Function ResolvePackageFolder(packageFolder As String) As String
        Dim normalizedRoot As String = NormalizePath(packageFolder)
        If String.IsNullOrWhiteSpace(normalizedRoot) OrElse Not Directory.Exists(normalizedRoot) Then
            Throw New DirectoryNotFoundException("Experimental package folder not found.")
        End If

        If ContainsKnownMarkers(normalizedRoot) Then
            Return normalizedRoot
        End If

        Dim bestMatch As String = ""
        Dim bestDepth As Integer = Integer.MaxValue

        For Each markerFile As String In KnownMarkerFiles
            For Each fileMatch As String In Directory.GetFiles(normalizedRoot, markerFile, SearchOption.AllDirectories)
                Dim candidate As String = Path.GetDirectoryName(fileMatch)
                Dim depth As Integer = GetPathDepth(normalizedRoot, candidate)
                If depth < bestDepth Then
                    bestDepth = depth
                    bestMatch = candidate
                End If
            Next
        Next

        For Each markerFolder As String In KnownMarkerFolders
            For Each dirMatch As String In Directory.GetDirectories(normalizedRoot, markerFolder, SearchOption.AllDirectories)
                Dim parent As String = Path.GetDirectoryName(dirMatch)
                Dim depth As Integer = GetPathDepth(normalizedRoot, parent)
                If depth < bestDepth Then
                    bestDepth = depth
                    bestMatch = parent
                End If
            Next
        Next

        If Not String.IsNullOrWhiteSpace(bestMatch) AndAlso Directory.Exists(bestMatch) Then
            Return bestMatch
        End If

        Throw New InvalidOperationException("Selected folder does not look like an FSR4 INT8 package. Expected amdxcffx64.dll (or related files).")
    End Function

    Private Function EnumerateCopyFiles(packageRoot As String) As List(Of String)
        Dim files As New List(Of String)()
        If String.IsNullOrWhiteSpace(packageRoot) OrElse Not Directory.Exists(packageRoot) Then
            Return files
        End If

        For Each filePath As String In Directory.GetFiles(packageRoot, "*", SearchOption.AllDirectories)
            If IsBlockedFileType(filePath) Then
                Continue For
            End If

            If Path.GetFileName(filePath).Equals(ManifestFileName, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            files.Add(filePath)
        Next

        Return files
    End Function

    Private Function IsBlockedFileType(filePath As String) As Boolean
        Dim extension As String = Path.GetExtension(filePath)
        Return BlockedCopyExtensions.Contains(extension)
    End Function

    Private Function CopyFileWithConflict(source As String,
                                          destination As String,
                                          conflictMode As ConflictMode,
                                          manifest As ExperimentalFsr4Manifest,
                                          log As Action(Of String)) As Boolean
        Dim destinationDir As String = Path.GetDirectoryName(destination)
        If Not String.IsNullOrWhiteSpace(destinationDir) Then
            Directory.CreateDirectory(destinationDir)
        End If

        If File.Exists(destination) Then
            Select Case conflictMode
                Case ConflictMode.Skip
                    log?.Invoke("Skipping existing file: " & destination)
                    Return False
                Case ConflictMode.BackupAndOverwrite
                    Dim backupPath As String = destination & ".bak_" & DateTime.UtcNow.ToString("yyyyMMddHHmmss")
                    File.Move(destination, backupPath)
                    manifest.BackupFiles(destination) = backupPath
                Case ConflictMode.Overwrite
                    File.Delete(destination)
            End Select
        End If

        File.Copy(source, destination, True)
        If manifest.InstalledFiles Is Nothing Then
            manifest.InstalledFiles = New List(Of String)()
        End If
        manifest.InstalledFiles.Add(destination)
        Return True
    End Function

    Private Function ContainsKnownMarkers(folderPath As String) As Boolean
        If String.IsNullOrWhiteSpace(folderPath) OrElse Not Directory.Exists(folderPath) Then
            Return False
        End If

        For Each name As String In KnownMarkerFiles
            If File.Exists(Path.Combine(folderPath, name)) Then
                Return True
            End If
        Next

        For Each name As String In KnownMarkerFolders
            If Directory.Exists(Path.Combine(folderPath, name)) Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Function HasKnownMarker(gameFolder As String) As Boolean
        Return ContainsKnownMarkers(gameFolder)
    End Function

    Private Function GetPathDepth(rootPath As String, candidatePath As String) As Integer
        If String.IsNullOrWhiteSpace(rootPath) OrElse String.IsNullOrWhiteSpace(candidatePath) Then
            Return Integer.MaxValue
        End If

        Try
            Dim relative As String = Path.GetRelativePath(rootPath, candidatePath)
            Dim parts As String() = relative.Split(New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries)
            Return parts.Length
        Catch ex As Exception
            ErrorLogger.Log(ex, "ExperimentalFsr4Service.GetPathDepth")
            Return Integer.MaxValue
        End Try
    End Function

    Private Sub SaveManifest(gameFolder As String, manifest As ExperimentalFsr4Manifest)
        Dim manifestPath As String = Path.Combine(gameFolder, ManifestFileName)
        Dim json As String = JsonSerializer.Serialize(manifest, New JsonSerializerOptions With {.WriteIndented = True})
        File.WriteAllText(manifestPath, json)
    End Sub

    Private Function TryLoadManifest(gameFolder As String) As ExperimentalFsr4Manifest
        Dim manifestPath As String = Path.Combine(gameFolder, ManifestFileName)
        If Not File.Exists(manifestPath) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(manifestPath)
            Return JsonSerializer.Deserialize(Of ExperimentalFsr4Manifest)(json)
        Catch ex As Exception
            ErrorLogger.Log(ex, "ExperimentalFsr4Service.TryLoadManifest")
            Return Nothing
        End Try
    End Function

    Private Function IsManifestValid(gameFolder As String, manifest As ExperimentalFsr4Manifest) As Boolean
        ' Validation checks for at least one live file/INI indicator under the expected game root.
        If manifest Is Nothing OrElse String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return False
        End If

        Dim gameRoot As String = NormalizePath(gameFolder)
        If String.IsNullOrWhiteSpace(gameRoot) Then
            Return False
        End If

        If manifest.InstalledFiles IsNot Nothing Then
            For Each installedPath As String In manifest.InstalledFiles
                Dim fullPath As String = ResolveManifestPath(gameRoot, installedPath)
                If String.IsNullOrWhiteSpace(fullPath) Then
                    Continue For
                End If

                Try
                    If File.Exists(fullPath) OrElse Directory.Exists(fullPath) Then
                        Return True
                    End If
                Catch ex As Exception
                    ErrorLogger.Log(ex, "ExperimentalFsr4Service.IsManifestValid.Path")
                End Try
            Next
        End If

        If manifest.BackupFiles IsNot Nothing Then
            For Each entry As KeyValuePair(Of String, String) In manifest.BackupFiles
                Dim destination As String = ResolveManifestPath(gameRoot, entry.Key)
                Dim backup As String = ResolveManifestPath(gameRoot, entry.Value)
                If (Not String.IsNullOrWhiteSpace(destination) AndAlso File.Exists(destination)) OrElse
                   (Not String.IsNullOrWhiteSpace(backup) AndAlso File.Exists(backup)) Then
                    Return True
                End If
            Next
        End If

        If manifest.IniKeys IsNot Nothing AndAlso manifest.IniKeys.Count > 0 Then
            Dim iniPath As String = Path.Combine(gameRoot, "OptiScaler.ini")
            If File.Exists(iniPath) Then
                For Each keyName As String In ManagedIniKeys
                    If IsIniKeyEnabled(gameFolder, keyName) Then
                        Return True
                    End If
                Next
            End If
        End If

        Return False
    End Function

    Private Function GetInstallerVersion() As String
        Dim version As Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        If version Is Nothing Then
            Return ""
        End If

        Dim build As Integer = If(version.Build >= 0, version.Build, 0)
        Dim revision As Integer = If(version.Revision >= 0, version.Revision, 0)
        Return $"{version.Major}.{version.Minor}.{build}.{revision}"
    End Function

    Private Function GetVersionFromFolder(folderPath As String) As String
        Dim candidates As String() = {
            Path.Combine(folderPath, "amdxcffx64.dll"),
            Path.Combine(folderPath, "amdxc64.dll"),
            Path.Combine(folderPath, "amdvkffx64.dll"),
            Path.Combine(folderPath, "amd_fidelityfx_upscaler_dx12.dll")
        }

        For Each path As String In candidates
            Dim version As String = TryGetFileVersion(path)
            If Not String.IsNullOrWhiteSpace(version) Then
                Return version
            End If
        Next

        Return ""
    End Function

    Private Function TryGetFileVersion(path As String) As String
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return ""
        End If

        Try
            Dim info As FileVersionInfo = FileVersionInfo.GetVersionInfo(path)
            Dim version As String = info.FileVersion
            If String.IsNullOrWhiteSpace(version) Then
                version = info.ProductVersion
            End If
            Return If(version, "")
        Catch ex As Exception
            ErrorLogger.Log(ex, "ExperimentalFsr4Service.TryGetFileVersion")
            Return ""
        End Try
    End Function

    Private Function NormalizePath(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim normalized As String = value.Trim().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        Try
            normalized = Path.GetFullPath(normalized)
        Catch ex As Exception
            ErrorLogger.Log(ex, "ExperimentalFsr4Service.NormalizePath")
        End Try

        Return normalized.TrimEnd(Path.DirectorySeparatorChar)
    End Function

    ' Restricts manifest file operations to the selected game root.
    Private Function ResolveManifestPath(gameRoot As String, manifestPathValue As String) As String
        If String.IsNullOrWhiteSpace(gameRoot) OrElse String.IsNullOrWhiteSpace(manifestPathValue) Then
            Return ""
        End If

        Try
            Dim candidate As String = manifestPathValue.Trim().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            Dim fullPath As String
            If Path.IsPathRooted(candidate) Then
                fullPath = Path.GetFullPath(candidate)
            Else
                fullPath = Path.GetFullPath(Path.Combine(gameRoot, candidate))
            End If

            If Not IsPathInsideRoot(fullPath, gameRoot) Then
                Return ""
            End If

            Return fullPath
        Catch ex As Exception
            ErrorLogger.Log(ex, "ExperimentalFsr4Service.ResolveManifestPath")
            Return ""
        End Try
    End Function

    Private Function IsPathInsideRoot(candidatePath As String, rootPath As String) As Boolean
        If String.IsNullOrWhiteSpace(candidatePath) OrElse String.IsNullOrWhiteSpace(rootPath) Then
            Return False
        End If

        Dim normalizedRoot As String = rootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) & Path.DirectorySeparatorChar
        Dim normalizedCandidate As String = candidatePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)

        If normalizedCandidate.Equals(normalizedRoot.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) Then
            Return True
        End If

        Return normalizedCandidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function UpdateManagedIniKey(path As String,
                                         keyName As String,
                                         newValue As String,
                                         originalState As ExperimentalIniKeyState,
                                         manifest As ExperimentalFsr4Manifest) As Boolean
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return False
        End If

        Dim document As IniDocument = IniDocument.Load(path)
        ' A package may carry a newer OptiScaler.ini. Do not inject obsolete
        ' legacy controls into it after preflight against the installed schema.
        If Not document.ContainsKey("FSR", keyName) Then Return False

        If manifest.IniKeys Is Nothing Then
            manifest.IniKeys = New Dictionary(Of String, ExperimentalIniKeyState)(StringComparer.OrdinalIgnoreCase)
        End If
        manifest.IniKeys(keyName) = originalState
        document.SetValue("FSR", keyName, newValue)
        File.WriteAllText(path, document.ToText())
        Return True
    End Function

    Private Sub RestoreManagedIniKey(path As String, keyName As String, state As ExperimentalIniKeyState)
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) OrElse state Is Nothing Then
            Return
        End If

        Dim lines As List(Of String) = File.ReadAllLines(path).ToList()
        Dim sectionName As String = If(String.IsNullOrWhiteSpace(state.SectionName), "", state.SectionName)
        Dim index As Integer = If(sectionName.Length = 0,
                                  FindIniKeyLineIndexAnySection(lines, keyName),
                                  FindIniKeyLineIndex(lines, sectionName, keyName))
        If state.Existed Then
            Dim restored As String = keyName & "=" & state.Value
            If index >= 0 Then
                lines(index) = restored
            ElseIf sectionName.Length = 0 Then
                ' Legacy manifests did not record a section. Preserve their old
                ' EOF-append behavior if the key disappeared after installation.
                lines.Add(restored)
            Else
                InsertIniKeyIntoSection(lines, sectionName, restored)
            End If
        ElseIf index >= 0 Then
            lines.RemoveAt(index)
        End If

        File.WriteAllLines(path, lines)
    End Sub

    Private Function CaptureIniKeyState(path As String, keyName As String) As ExperimentalIniKeyState
        Dim state As New ExperimentalIniKeyState With {
            .Existed = False,
            .Value = "",
            .SectionName = "FSR"
        }

        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) OrElse String.IsNullOrWhiteSpace(keyName) Then
            Return state
        End If

        Dim lines As List(Of String) = File.ReadAllLines(path).ToList()
        Dim index As Integer = FindIniKeyLineIndex(lines, "FSR", keyName)
        If index < 0 Then
            Return state
        End If

        Dim parsedValue As String = ""
        If TryParseIniLine(lines(index), keyName, parsedValue) Then
            state.Existed = True
            state.Value = parsedValue
        End If

        Return state
    End Function

    Private Function IsIniKeyEnabled(gameFolder As String, keyName As String) As Boolean
        If String.IsNullOrWhiteSpace(gameFolder) OrElse String.IsNullOrWhiteSpace(keyName) Then
            Return False
        End If

        Dim iniPath As String = Path.Combine(gameFolder, "OptiScaler.ini")
        If Not File.Exists(iniPath) Then
            Return False
        End If

        Try
            Dim document As IniDocument = IniDocument.Load(iniPath)
            Dim value As String = ""
            Return document.TryGetValue("FSR", keyName, value) AndAlso ParseIniBool(value)
        Catch ex As Exception
            ErrorLogger.Log(ex, "ExperimentalFsr4Service.IsIniKeyEnabled")
        End Try

        Return False
    End Function

    Private Function IsExplicitModernFsr4Selection(gameFolder As String) As Boolean
        If String.IsNullOrWhiteSpace(gameFolder) Then Return False
        Dim iniPath As String = Path.Combine(gameFolder, "OptiScaler.ini")
        If Not File.Exists(iniPath) Then Return False

        Try
            Dim document As IniDocument = IniDocument.Load(iniPath)
            Dim value As String = ""
            If document.TryGetValue("FSR", "Fsr4GpuSupport", value) AndAlso IsExplicitNonDefaultSelection(value) Then Return True
            If document.TryGetValue("FSR", "Fsr4Model", value) AndAlso IsExplicitFsr4Model(value) Then Return True
        Catch ex As Exception
            ErrorLogger.Log(ex, "ExperimentalFsr4Service.IsExplicitModernFsr4Selection")
        End Try
        Return False
    End Function

    Private Function IsExplicitNonDefaultSelection(value As String) As Boolean
        Dim selected As Integer
        Return Integer.TryParse(If(value, "").Trim(), selected) AndAlso selected >= 1 AndAlso selected <= 4
    End Function

    Private Function IsExplicitFsr4Model(value As String) As Boolean
        ' Model zero is a concrete FP8 model choice (unlike GPU support zero,
        ' which means no support override). "auto" and unknown values are not proof.
        Dim selected As Integer
        Return Integer.TryParse(If(value, "").Trim(), selected) AndAlso selected >= 0 AndAlso selected <= 3
    End Function

    Private Function FindIniKeyLineIndex(lines As List(Of String), sectionName As String, keyName As String) As Integer
        If lines Is Nothing OrElse String.IsNullOrWhiteSpace(sectionName) OrElse String.IsNullOrWhiteSpace(keyName) Then
            Return -1
        End If

        Dim currentSection As String = ""
        For i As Integer = 0 To lines.Count - 1
            Dim trimmed As String = lines(i).Trim()
            If trimmed.StartsWith("[") AndAlso trimmed.EndsWith("]") Then
                currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim()
                Continue For
            End If
            If Not currentSection.Equals(sectionName, StringComparison.OrdinalIgnoreCase) Then Continue For
            Dim ignored As String = ""
            If TryParseIniLine(lines(i), keyName, ignored) Then
                Return i
            End If
        Next

        Return -1
    End Function

    Private Function FindIniKeyLineIndexAnySection(lines As List(Of String), keyName As String) As Integer
        If lines Is Nothing OrElse String.IsNullOrWhiteSpace(keyName) Then Return -1
        For i As Integer = 0 To lines.Count - 1
            Dim ignored As String = ""
            If TryParseIniLine(lines(i), keyName, ignored) Then Return i
        Next
        Return -1
    End Function

    Private Function CountIniKeyInSection(path As String, sectionName As String, keyName As String) As Integer
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then Return 0
        Dim count As Integer = 0
        Dim currentSection As String = ""
        For Each line As String In File.ReadAllLines(path)
            Dim trimmed As String = line.Trim()
            If trimmed.StartsWith("[") AndAlso trimmed.EndsWith("]") Then
                currentSection = trimmed.Substring(1, trimmed.Length - 2).Trim()
                Continue For
            End If
            If Not currentSection.Equals(sectionName, StringComparison.OrdinalIgnoreCase) Then Continue For
            Dim ignored As String = ""
            If TryParseIniLine(line, keyName, ignored) Then count += 1
        Next
        Return count
    End Function

    Private Sub InsertIniKeyIntoSection(lines As List(Of String), sectionName As String, keyLine As String)
        Dim sectionIndex As Integer = -1
        Dim insertIndex As Integer = lines.Count
        For i As Integer = 0 To lines.Count - 1
            Dim trimmed As String = lines(i).Trim()
            If trimmed.Equals("[" & sectionName & "]", StringComparison.OrdinalIgnoreCase) Then
                sectionIndex = i
                insertIndex = lines.Count
                Continue For
            End If
            If sectionIndex >= 0 AndAlso trimmed.StartsWith("[") AndAlso trimmed.EndsWith("]") Then
                insertIndex = i
                Exit For
            End If
        Next

        If sectionIndex < 0 Then
            If lines.Count > 0 AndAlso lines(lines.Count - 1).Trim().Length > 0 Then lines.Add("")
            lines.Add("[" & sectionName & "]")
            lines.Add(keyLine)
        Else
            lines.Insert(insertIndex, keyLine)
        End If
    End Sub

    Private Function TryParseIniLine(line As String, expectedKey As String, ByRef value As String) As Boolean
        value = ""
        If String.IsNullOrWhiteSpace(line) OrElse String.IsNullOrWhiteSpace(expectedKey) Then
            Return False
        End If

        Dim trimmed As String = line.Trim()
        If trimmed.Length = 0 OrElse trimmed.StartsWith(";", StringComparison.Ordinal) OrElse trimmed.StartsWith("#", StringComparison.Ordinal) Then
            Return False
        End If

        Dim equalsIndex As Integer = trimmed.IndexOf("="c)
        If equalsIndex <= 0 Then
            Return False
        End If

        Dim keyName As String = trimmed.Substring(0, equalsIndex).Trim()
        If Not keyName.Equals(expectedKey, StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If

        value = trimmed.Substring(equalsIndex + 1).Trim()
        Return True
    End Function

    Private Function ParseIniBool(value As String) As Boolean
        If String.IsNullOrWhiteSpace(value) Then
            Return False
        End If

        Dim normalized As String = value.Trim().ToLowerInvariant()
        Return normalized = "1" OrElse normalized = "true" OrElse normalized = "yes" OrElse normalized = "on"
    End Function
End Module
