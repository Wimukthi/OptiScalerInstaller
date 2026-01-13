Imports System.IO
Imports System.Net.Http
Imports System.Text.Json
Imports System.Diagnostics
Imports SharpCompress.Common
Imports SharpCompress.Archives
Imports SharpCompress.Archives.SevenZip

Public Class InstallerService
    Private Shared ReadOnly ManifestName As String = "OptiScalerInstaller.manifest.json"

    Private Class ArchiveResult
        Public Property ArchivePath As String
        Public Property Release As ReleaseInfo
    End Class

    Public Shared Async Function InstallAsync(config As InstallerConfig, log As Action(Of String), progress As Action(Of Integer)) As Task(Of InstallManifest)
        ValidateConfig(config)

        Dim tempRoot As String = Path.Combine(Path.GetTempPath(), "OptiScalerInstaller")
        Directory.CreateDirectory(tempRoot)

        Dim archivePath As String = Nothing
        Dim extractRoot As String = Nothing
        Dim manifest As New InstallManifest With {
            .InstallerVersion = GetInstallerVersion(),
            .InstallTimeUtc = DateTime.UtcNow,
            .GameFolder = config.GameFolder,
            .HookName = config.HookName,
            .InstalledFiles = New List(Of String)(),
            .BackupFiles = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        }

        Try
            progress?.Invoke(0)
            Dim archiveResult As ArchiveResult = Await ResolveArchiveAsync(config, tempRoot, log, progress)
            archivePath = archiveResult.ArchivePath
            manifest.OptiScalerSource = config.Source.ToString()
            If archiveResult.Release IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(archiveResult.Release.TagName) Then
                manifest.OptiScalerVersion = archiveResult.Release.TagName
            End If
            progress?.Invoke(25)

            extractRoot = Path.Combine(tempRoot, "extract_" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(extractRoot)
            Await Task.Run(Sub() ExtractArchive(archivePath, extractRoot, log))
            progress?.Invoke(45)

            Dim packageRoot As String = ResolvePackageRoot(extractRoot)
            Await Task.Run(Sub()
                               log?.Invoke("Copying OptiScaler files to game folder...")
                               CopyDirectory(packageRoot, config.GameFolder, config.ConflictMode, manifest, log)
                               progress?.Invoke(70)

                               RenameOptiScalerDll(config, manifest, log)
                               UpdateIni(config, log)
                               CopyAddOns(config, manifest, log)
                               UpdateManifestVersion(config, manifest, log)
                               SaveManifest(config.GameFolder, manifest)
                           End Sub)

            progress?.Invoke(100)
            log?.Invoke("Install complete.")
            Return manifest
        Finally
            If extractRoot IsNot Nothing AndAlso Directory.Exists(extractRoot) Then
                Try
                    Directory.Delete(extractRoot, True)
                Catch
                End Try
            End If
        End Try
    End Function

    Public Shared Async Function UninstallAsync(gameFolder As String, log As Action(Of String)) As Task(Of Boolean)
        Dim manifestPath As String = Path.Combine(gameFolder, ManifestName)
        If Not File.Exists(manifestPath) Then
            Return False
        End If

        Dim manifest As InstallManifest = Nothing
        Try
            Dim json As String = Await File.ReadAllTextAsync(manifestPath)
            manifest = JsonSerializer.Deserialize(Of InstallManifest)(json)
        Catch ex As Exception
            log?.Invoke("Failed to read manifest: " & ex.Message)
            Return False
        End Try

        If manifest Is Nothing Then
            Return False
        End If

        log?.Invoke("Removing installed files...")
        For Each filePath As String In manifest.InstalledFiles
            Try
                If File.Exists(filePath) Then
                    File.Delete(filePath)
                End If
            Catch ex As Exception
                log?.Invoke("Failed to delete " & filePath & ": " & ex.Message)
            End Try
        Next

        log?.Invoke("Restoring backups...")
        For Each kvp As KeyValuePair(Of String, String) In manifest.BackupFiles
            Try
                If File.Exists(kvp.Value) Then
                    If File.Exists(kvp.Key) Then
                        File.Delete(kvp.Key)
                    End If
                    File.Move(kvp.Value, kvp.Key)
                End If
            Catch ex As Exception
                log?.Invoke("Failed to restore backup for " & kvp.Key & ": " & ex.Message)
            End Try
        Next

        Try
            If File.Exists(manifestPath) Then
                File.Delete(manifestPath)
            End If
        Catch ex As Exception
            log?.Invoke("Failed to delete manifest: " & ex.Message)
        End Try

        log?.Invoke("Uninstall complete.")
        Return True
    End Function

    Private Shared Sub ValidateConfig(config As InstallerConfig)
        If config Is Nothing Then
            Throw New ArgumentNullException(NameOf(config))
        End If

        If String.IsNullOrWhiteSpace(config.GameExePath) OrElse Not File.Exists(config.GameExePath) Then
            Throw New InvalidOperationException("Please choose a valid game executable.")
        End If

        If String.IsNullOrWhiteSpace(config.GameFolder) OrElse Not Directory.Exists(config.GameFolder) Then
            Throw New InvalidOperationException("Game folder not found.")
        End If

        If String.IsNullOrWhiteSpace(config.HookName) Then
            Throw New InvalidOperationException("Please choose a hook filename.")
        End If

        If config.Source = ReleaseSource.LocalArchive Then
            If String.IsNullOrWhiteSpace(config.LocalArchivePath) OrElse Not File.Exists(config.LocalArchivePath) Then
                Throw New InvalidOperationException("Please select a local OptiScaler .7z archive.")
            End If
        End If
    End Sub

    Private Shared Function GetInstallerVersion() As String
        Dim version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
        If version Is Nothing Then
            Return "0.0.0"
        End If
        Return version.ToString()
    End Function

    Private Shared Async Function ResolveArchiveAsync(config As InstallerConfig, tempRoot As String, log As Action(Of String), progress As Action(Of Integer)) As Task(Of ArchiveResult)
        If config.Source = ReleaseSource.LocalArchive Then
            log?.Invoke("Using local archive: " & config.LocalArchivePath)
            Return New ArchiveResult With {
                .ArchivePath = config.LocalArchivePath,
                .Release = Nothing
            }
        End If

        Dim release As ReleaseInfo = Nothing
        If config.Source = ReleaseSource.Stable Then
            release = config.StableRelease
            If release Is Nothing OrElse String.IsNullOrWhiteSpace(release.DownloadUrl) Then
                release = Await ReleaseService.GetStableReleaseAsync()
            End If
        ElseIf config.Source = ReleaseSource.Nightly Then
            release = config.NightlyRelease
            If release Is Nothing OrElse String.IsNullOrWhiteSpace(release.DownloadUrl) Then
                release = Await ReleaseService.GetNightlyReleaseAsync()
            End If
        End If

        If release Is Nothing OrElse String.IsNullOrWhiteSpace(release.DownloadUrl) Then
            Throw New InvalidOperationException("Release download URL not available.")
        End If

        Dim safeName As String = If(String.IsNullOrWhiteSpace(release.AssetName), "OptiScaler.7z", release.AssetName)
        Dim destination As String = Path.Combine(tempRoot, safeName)
        log?.Invoke("Downloading " & release.TagName & "...")
        Await DownloadFileAsync(release.DownloadUrl, destination, log, progress)
        Return New ArchiveResult With {
            .ArchivePath = destination,
            .Release = release
        }
    End Function

    Private Shared Async Function DownloadFileAsync(url As String, destination As String, log As Action(Of String), progress As Action(Of Integer)) As Task
        Using client As New HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
            Using response As HttpResponseMessage = Await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                response.EnsureSuccessStatusCode()

                Dim total As Nullable(Of Long) = response.Content.Headers.ContentLength
                Using input As Stream = Await response.Content.ReadAsStreamAsync()
                    Using output As FileStream = File.Create(destination)
                        Dim buffer(81919) As Byte
                        Dim read As Integer
                        Dim totalRead As Long = 0

                        Do
                            read = Await input.ReadAsync(buffer, 0, buffer.Length)
                            If read = 0 Then
                                Exit Do
                            End If

                            Await output.WriteAsync(buffer, 0, read)
                            totalRead += read

                            If total.HasValue AndAlso total.Value > 0 Then
                                Dim percent As Integer = CInt((totalRead * 100) / total.Value)
                                progress?.Invoke(Math.Min(100, Math.Max(0, percent)))
                            End If
                        Loop
                    End Using
                End Using
            End Using
        End Using

        log?.Invoke("Download complete.")
    End Function

    Private Shared Sub ExtractArchive(archivePath As String, outputDir As String, log As Action(Of String))
        log?.Invoke("Extracting archive...")
        Dim options As New ExtractionOptions With {
            .ExtractFullPath = True,
            .Overwrite = True
        }

        Dim extension As String = Path.GetExtension(archivePath).ToLowerInvariant()
        If extension = ".7z" Then
            Using archive As SevenZipArchive = SevenZipArchive.Open(archivePath)
                ExtractEntries(archive.Entries, outputDir, options, log)
            End Using
        Else
            Using archive As IArchive = ArchiveFactory.Open(archivePath)
                ExtractEntries(archive.Entries, outputDir, options, log)
            End Using
        End If
        log?.Invoke("Extraction complete.")
    End Sub

    Private Shared Sub ExtractEntries(entries As IEnumerable(Of IArchiveEntry), outputDir As String, options As ExtractionOptions, log As Action(Of String))
        For Each entry As IArchiveEntry In entries
            If entry.IsDirectory Then
                Continue For
            End If

            Dim hasStream As Boolean = EntryHasStream(entry)
            If Not hasStream Then
                If entry.Size = 0 Then
                    CreateEmptyEntry(outputDir, entry.Key, log)
                Else
                    log?.Invoke("Skipping entry without stream: " & entry.Key)
                End If
                Continue For
            End If

            Try
                entry.WriteToDirectory(outputDir, options)
            Catch ex As Exception
                log?.Invoke("Failed to extract " & entry.Key & ": " & ex.Message)
            End Try
        Next
    End Sub

    Private Shared Function EntryHasStream(entry As IArchiveEntry) As Boolean
        If entry Is Nothing Then
            Return False
        End If

        Dim prop As System.Reflection.PropertyInfo = entry.GetType().GetProperty("HasStream")
        If prop IsNot Nothing Then
            Try
                Dim value As Object = prop.GetValue(entry)
                If TypeOf value Is Boolean Then
                    Return CBool(value)
                End If
            Catch
            End Try
        End If

        Return True
    End Function

    Private Shared Sub CreateEmptyEntry(outputDir As String, key As String, log As Action(Of String))
        If String.IsNullOrWhiteSpace(key) Then
            Return
        End If

        Dim safeKey As String = key.Replace("/"c, Path.DirectorySeparatorChar)
        Dim destination As String = Path.Combine(outputDir, safeKey)
        Dim folder As String = Path.GetDirectoryName(destination)
        If Not String.IsNullOrWhiteSpace(folder) Then
            Directory.CreateDirectory(folder)
        End If

        If Not File.Exists(destination) Then
            File.WriteAllText(destination, "")
            log?.Invoke("Created empty file: " & safeKey)
        End If
    End Sub

    Private Shared Function ResolvePackageRoot(extractRoot As String) As String
        Dim directDll As String = Path.Combine(extractRoot, "OptiScaler.dll")
        If File.Exists(directDll) Then
            Return extractRoot
        End If

        Dim dirs As String() = Directory.GetDirectories(extractRoot)
        If dirs.Length = 1 Then
            Dim candidate As String = Path.Combine(dirs(0), "OptiScaler.dll")
            If File.Exists(candidate) Then
                Return dirs(0)
            End If
        End If

        Return extractRoot
    End Function

    Private Shared Sub CopyDirectory(sourceDir As String, targetDir As String, conflictMode As ConflictMode, manifest As InstallManifest, log As Action(Of String))
        For Each dirPath As String In Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories)
            Dim relative As String = Path.GetRelativePath(sourceDir, dirPath)
            Dim destination As String = Path.Combine(targetDir, relative)
            Directory.CreateDirectory(destination)
        Next

        For Each filePath As String In Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories)
            Dim relative As String = Path.GetRelativePath(sourceDir, filePath)
            Dim destination As String = Path.Combine(targetDir, relative)
            If CopyFileWithConflict(filePath, destination, conflictMode, manifest, log) Then
                ' File recorded inside copy helper.
            End If
        Next
    End Sub

    Private Shared Function CopyFileWithConflict(source As String, destination As String, conflictMode As ConflictMode, manifest As InstallManifest, log As Action(Of String)) As Boolean
        Directory.CreateDirectory(Path.GetDirectoryName(destination))

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
                    ' Overwrite without backup.
            End Select
        End If

        File.Copy(source, destination, True)
        manifest.InstalledFiles.Add(destination)
        Return True
    End Function

    Private Shared Sub RenameOptiScalerDll(config As InstallerConfig, manifest As InstallManifest, log As Action(Of String))
        Dim sourcePath As String = Path.Combine(config.GameFolder, "OptiScaler.dll")
        Dim destinationPath As String = Path.Combine(config.GameFolder, config.HookName)

        If String.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase) Then
            Return
        End If

        If Not File.Exists(sourcePath) Then
            log?.Invoke("OptiScaler.dll was not found after extraction. Skipping rename.")
            Return
        End If

        If File.Exists(destinationPath) Then
            Select Case config.ConflictMode
                Case ConflictMode.Skip
                    log?.Invoke("Hook name already exists. Skipping rename: " & destinationPath)
                    Return
                Case ConflictMode.BackupAndOverwrite
                    Dim backupPath As String = destinationPath & ".bak_" & DateTime.UtcNow.ToString("yyyyMMddHHmmss")
                    File.Move(destinationPath, backupPath)
                    manifest.BackupFiles(destinationPath) = backupPath
                Case ConflictMode.Overwrite
                    File.Delete(destinationPath)
            End Select
        End If

        File.Move(sourcePath, destinationPath)
        manifest.InstalledFiles.RemoveAll(Function(path) path.Equals(sourcePath, StringComparison.OrdinalIgnoreCase))
        manifest.InstalledFiles.Add(destinationPath)
        log?.Invoke("Renamed OptiScaler.dll to " & config.HookName & ".")
    End Sub

    Private Shared Sub UpdateIni(config As InstallerConfig, log As Action(Of String))
        Dim iniPath As String = Path.Combine(config.GameFolder, "OptiScaler.ini")
        ApplyDefaultIni(config, iniPath, log)

        If Not File.Exists(iniPath) Then
            log?.Invoke("OptiScaler.ini not found. Skipping INI updates.")
            Return
        End If

        Dim updates As New List(Of IniUpdate)()
        Dim dxgiValue As String = "auto"

        If config.GpuVendor = GpuVendor.AmdIntel Then
            dxgiValue = If(config.EnableDlssInputs, "auto", "false")
        End If

        updates.Add(New IniUpdate With {.Section = "Spoofing", .Key = "Dxgi", .Value = dxgiValue})

        Dim fgValue As String = "auto"
        Select Case config.FgType
            Case FgTypeSelection.None
                fgValue = "nofg"
            Case FgTypeSelection.OptiFg
                fgValue = "optifg"
            Case FgTypeSelection.Nukem
                fgValue = "nukems"
        End Select
        updates.Add(New IniUpdate With {.Section = "FrameGen", .Key = "FGType", .Value = fgValue})

        updates.Add(New IniUpdate With {.Section = "Plugins", .Key = "LoadAsiPlugins", .Value = ToIniBool(config.LoadAsiPlugins)})
        updates.Add(New IniUpdate With {.Section = "Plugins", .Key = "LoadReshade", .Value = ToIniBool(config.EnableReshade)})
        updates.Add(New IniUpdate With {.Section = "Plugins", .Key = "LoadSpecialK", .Value = ToIniBool(config.EnableSpecialK)})

        If Not String.IsNullOrWhiteSpace(config.PluginsPath) Then
            updates.Add(New IniUpdate With {.Section = "Plugins", .Key = "Path", .Value = config.PluginsPath})
        End If

        IniFile.SetValues(iniPath, updates)
        log?.Invoke("Updated OptiScaler.ini settings.")
    End Sub

    Private Shared Sub CopyAddOns(config As InstallerConfig, manifest As InstallManifest, log As Action(Of String))
        If Not String.IsNullOrWhiteSpace(config.FakenvapiFolder) Then
            Dim folder As String = config.FakenvapiFolder
            If File.Exists(folder) Then
                folder = Path.GetDirectoryName(folder)
            End If

            Dim nvapiPath As String = Path.Combine(folder, "nvapi64.dll")
            Dim iniPath As String = Path.Combine(folder, "fakenvapi.ini")

            If File.Exists(nvapiPath) AndAlso File.Exists(iniPath) Then
                CopyFileWithConflict(nvapiPath, Path.Combine(config.GameFolder, "nvapi64.dll"), config.ConflictMode, manifest, log)
                CopyFileWithConflict(iniPath, Path.Combine(config.GameFolder, "fakenvapi.ini"), config.ConflictMode, manifest, log)
                log?.Invoke("Copied Fakenvapi files.")
            Else
                log?.Invoke("Fakenvapi files not found in selected folder.")
            End If
        End If

        If Not String.IsNullOrWhiteSpace(config.NukemDllPath) Then
            If File.Exists(config.NukemDllPath) Then
                Dim dest As String = Path.Combine(config.GameFolder, "dlssg_to_fsr3_amd_is_better.dll")
                CopyFileWithConflict(config.NukemDllPath, dest, config.ConflictMode, manifest, log)
                log?.Invoke("Copied Nukem dlssg_to_fsr3_amd_is_better.dll.")
            Else
                log?.Invoke("Nukem DLL path not found.")
            End If
        End If

        If Not String.IsNullOrWhiteSpace(config.NvngxDllPath) Then
            If File.Exists(config.NvngxDllPath) Then
                Dim dest As String = Path.Combine(config.GameFolder, "nvngx_dlss.dll")
                CopyFileWithConflict(config.NvngxDllPath, dest, config.ConflictMode, manifest, log)
                log?.Invoke("Copied nvngx_dlss.dll.")
            Else
                log?.Invoke("nvngx_dlss.dll path not found.")
            End If
        End If

        If config.EnableReshade AndAlso Not String.IsNullOrWhiteSpace(config.ReshadeDllPath) Then
            If File.Exists(config.ReshadeDllPath) Then
                Dim dest As String = Path.Combine(config.GameFolder, "ReShade64.dll")
                CopyFileWithConflict(config.ReshadeDllPath, dest, config.ConflictMode, manifest, log)
                log?.Invoke("Copied ReShade64.dll.")
            Else
                log?.Invoke("Reshade DLL path not found.")
            End If
        End If

        If config.EnableSpecialK AndAlso Not String.IsNullOrWhiteSpace(config.SpecialKDllPath) Then
            If File.Exists(config.SpecialKDllPath) Then
                Dim dest As String = Path.Combine(config.GameFolder, "SpecialK64.dll")
                CopyFileWithConflict(config.SpecialKDllPath, dest, config.ConflictMode, manifest, log)
                log?.Invoke("Copied SpecialK64.dll.")

                If config.CreateSpecialKMarker Then
                    Dim markerPath As String = Path.Combine(config.GameFolder, "SpecialK.dxgi")
                    If Not File.Exists(markerPath) Then
                        File.WriteAllText(markerPath, "")
                        manifest.InstalledFiles.Add(markerPath)
                        log?.Invoke("Created SpecialK.dxgi marker.")
                    End If
                End If
            Else
                log?.Invoke("SpecialK DLL path not found.")
            End If
        End If
    End Sub

    Private Shared Sub ApplyDefaultIni(config As InstallerConfig, iniPath As String, log As Action(Of String))
        If config Is Nothing Then
            Return
        End If

        If config.DefaultIniMode = DefaultIniMode.Off Then
            Return
        End If

        Dim sourcePath As String = config.DefaultIniPath
        If String.IsNullOrWhiteSpace(sourcePath) Then
            log?.Invoke("Default OptiScaler.ini path not set. Skipping defaults.")
            Return
        End If

        If Not File.Exists(sourcePath) Then
            log?.Invoke("Default OptiScaler.ini not found. Skipping defaults: " & sourcePath)
            Return
        End If

        If config.DefaultIniMode = DefaultIniMode.Replace Then
            File.Copy(sourcePath, iniPath, True)
            log?.Invoke("Applied default OptiScaler.ini (replace).")
            Return
        End If

        If Not File.Exists(iniPath) Then
            File.Copy(sourcePath, iniPath, True)
            log?.Invoke("Applied default OptiScaler.ini (merge base created).")
            Return
        End If

        Dim updates As List(Of IniUpdate) = IniFile.ReadValues(sourcePath)
        If updates.Count = 0 Then
            log?.Invoke("Default OptiScaler.ini had no settings to merge.")
            Return
        End If

        IniFile.SetValues(iniPath, updates)
        log?.Invoke("Merged default OptiScaler.ini settings.")
    End Sub

    Private Shared Sub UpdateManifestVersion(config As InstallerConfig, manifest As InstallManifest, log As Action(Of String))
        If config Is Nothing OrElse manifest Is Nothing Then
            Return
        End If

        Dim version As String = TryGetInstalledFileVersion(config)
        If String.IsNullOrWhiteSpace(version) Then
            Return
        End If

        manifest.OptiScalerVersion = version
        log?.Invoke("Detected OptiScaler version: " & version)
    End Sub

    Private Shared Function TryGetInstalledFileVersion(config As InstallerConfig) As String
        Dim hookPath As String = ""
        If Not String.IsNullOrWhiteSpace(config.HookName) Then
            hookPath = Path.Combine(config.GameFolder, config.HookName)
        End If

        Dim version As String = TryGetFileVersion(hookPath)
        If String.IsNullOrWhiteSpace(version) Then
            version = TryGetFileVersion(Path.Combine(config.GameFolder, "OptiScaler.dll"))
        End If

        Return version
    End Function

    Private Shared Function TryGetFileVersion(path As String) As String
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
        Catch
            Return ""
        End Try
    End Function

    Private Shared Sub SaveManifest(gameFolder As String, manifest As InstallManifest)
        Dim manifestPath As String = Path.Combine(gameFolder, ManifestName)
        Dim json As String = JsonSerializer.Serialize(manifest, New JsonSerializerOptions With {.WriteIndented = True})
        File.WriteAllText(manifestPath, json)
    End Sub

    Private Shared Function ToIniBool(value As Boolean) As String
        Return If(value, "true", "false")
    End Function
End Class
