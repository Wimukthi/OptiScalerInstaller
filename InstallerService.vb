Imports System.IO
Imports System.Net.Http
Imports System.Text.Json
Imports System.Diagnostics
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports System.Linq
Imports SharpCompress.Common
Imports SharpCompress.Archives
Imports SharpCompress.Archives.SevenZip

Public Class InstallerService
    ' Orchestrates OptiScaler install/uninstall operations.
    Private Shared ReadOnly ManifestName As String = "OptiScalerInstaller.manifest.json"
    Private Shared ReadOnly DownloadTimeout As TimeSpan = TimeSpan.FromMinutes(15)
    Private Const MaxDownloadAttempts As Integer = 3

    Private Class ArchiveResult
        Public Property ArchivePath As String
        Public Property Release As ReleaseInfo
        Public Property SourceUrl As String
        Public Property ExpectedSize As Long
        Public Property AssetName As String
    End Class

    Private Class ArchiveCacheMetadata
        Public Property SourceChannel As String
        Public Property TagName As String
        Public Property AssetName As String
        Public Property DownloadUrl As String
        Public Property Size As Long
        Public Property Digest As String
        Public Property CachedFileName As String
        Public Property CachedAtUtc As DateTime
    End Class

    Public Shared Function IsLikelyBundledComponentRelease(config As InstallerConfig) As Boolean
        If config Is Nothing Then
            Return False
        End If

        If config.Source = ReleaseSource.LocalArchive Then
            Return False
        End If

        Dim sourceRelease As ReleaseInfo = GetSelectedRelease(config)
        If sourceRelease Is Nothing Then
            Return False
        End If

        Dim version As Version = ParseOptiScalerVersion(sourceRelease.TagName)
        If version Is Nothing Then
            Return False
        End If

        Return version >= New Version(0, 9, 0, 0)
    End Function

    Public Shared Async Function InstallAsync(config As InstallerConfig, log As Action(Of String), progress As Action(Of Integer)) As Task(Of InstallManifest)
        ValidateConfig(config)

        ' Stage downloads and extraction in a temp folder to avoid partial installs.
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
            manifest.ArchiveFileName = Path.GetFileName(archiveResult.AssetName)
            manifest.ArchiveSourceUrl = archiveResult.SourceUrl
            manifest.ArchiveSizeBytes = GetFileSizeSafe(archivePath)
            manifest.ArchiveSha256 = ComputeSha256(archivePath)
            VerifyReleaseDigest(archiveResult.Release, manifest.ArchiveSha256, log)
            log?.Invoke("Archive fingerprint (SHA-256): " & ShortHash(manifest.ArchiveSha256))
            progress?.Invoke(25)

            extractRoot = Path.Combine(tempRoot, "extract_" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(extractRoot)
            Await Task.Run(Sub() ExtractArchive(archivePath, extractRoot, log))
            progress?.Invoke(45)

            ' Determine the actual root that contains OptiScaler.dll.
            Dim packageRoot As String = ResolvePackageRoot(extractRoot)
            manifest.PackageRoot = packageRoot
            Await Task.Run(Sub()
                               log?.Invoke("Copying OptiScaler files to game folder...")
                               CopyDirectory(packageRoot, config.GameFolder, config.ConflictMode, manifest, log)
                               progress?.Invoke(70)

                               RenameOptiScalerDll(config, manifest, log)
                               UpdateIni(config, log)
                               CopyAddOns(config, manifest, log)
                               CleanupPackageArtifacts(config.GameFolder, log)
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
                Catch ex As Exception
                    ErrorLogger.Log(ex, "InstallerService.CleanupExtractRoot")
                End Try
            End If
        End Try
    End Function

    Public Shared Async Function UninstallAsync(gameFolder As String, log As Action(Of String)) As Task(Of Boolean)
        ' Managed uninstall path: use manifest with root-constrained file operations.
        Dim manifestPath As String = Path.Combine(gameFolder, ManifestName)
        If Not File.Exists(manifestPath) Then
            Return TryUninstallLegacy(gameFolder, log)
        End If

        Dim gameRoot As String = NormalizePath(gameFolder)
        If String.IsNullOrWhiteSpace(gameRoot) OrElse Not Directory.Exists(gameRoot) Then
            log?.Invoke("Game folder not found. Uninstall aborted.")
            Return False
        End If

        Dim manifest As InstallManifest = Nothing
        Try
            Dim json As String = Await File.ReadAllTextAsync(manifestPath)
            manifest = JsonSerializer.Deserialize(Of InstallManifest)(json)
        Catch ex As Exception
            log?.Invoke("Failed to read manifest: " & ex.Message)
            ErrorLogger.Log(ex, "InstallerService.ReadManifest")
            Return False
        End Try

        If manifest Is Nothing Then
            Return False
        End If

        log?.Invoke("Removing installed files...")
        If manifest.InstalledFiles IsNot Nothing Then
            For Each filePath As String In manifest.InstalledFiles
                Dim resolvedFilePath As String = ResolveManifestPath(gameRoot, filePath)
                If String.IsNullOrWhiteSpace(resolvedFilePath) Then
                    log?.Invoke("Skipping out-of-scope manifest file entry: " & If(filePath, "(empty)"))
                    Continue For
                End If

                Try
                    If File.Exists(resolvedFilePath) Then
                        File.Delete(resolvedFilePath)
                    End If
                Catch ex As Exception
                    log?.Invoke("Failed to delete " & resolvedFilePath & ": " & ex.Message)
                    ErrorLogger.Log(ex, "InstallerService.DeleteInstalledFile")
                End Try
            Next
        End If

        log?.Invoke("Restoring backups...")
        If manifest.BackupFiles IsNot Nothing Then
            For Each kvp As KeyValuePair(Of String, String) In manifest.BackupFiles
                Dim destinationPath As String = ResolveManifestPath(gameRoot, kvp.Key)
                Dim backupPath As String = ResolveManifestPath(gameRoot, kvp.Value)
                If String.IsNullOrWhiteSpace(destinationPath) OrElse String.IsNullOrWhiteSpace(backupPath) Then
                    log?.Invoke("Skipping out-of-scope backup entry: " & If(kvp.Key, "(empty)"))
                    Continue For
                End If

                Try
                    If File.Exists(backupPath) Then
                        If File.Exists(destinationPath) Then
                            File.Delete(destinationPath)
                        End If
                        File.Move(backupPath, destinationPath)
                    End If
                Catch ex As Exception
                    log?.Invoke("Failed to restore backup for " & destinationPath & ": " & ex.Message)
                    ErrorLogger.Log(ex, "InstallerService.RestoreBackup")
                End Try
            Next
        End If

        Try
            If File.Exists(manifestPath) Then
                File.Delete(manifestPath)
            End If
        Catch ex As Exception
            log?.Invoke("Failed to delete manifest: " & ex.Message)
            ErrorLogger.Log(ex, "InstallerService.DeleteManifest")
        End Try

        log?.Invoke("Uninstall complete.")
        Return True
    End Function

    Private Shared Function TryUninstallLegacy(gameFolder As String, log As Action(Of String)) As Boolean
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            Return False
        End If

        Dim removedAny As Boolean = False

        removedAny = DeleteIfExists(Path.Combine(gameFolder, "OptiScaler.log"), log) OrElse removedAny
        removedAny = DeleteIfExists(Path.Combine(gameFolder, "OptiScaler.ini"), log) OrElse removedAny

        Dim hookFromBat As String = GetHookNameFromUninstallBat(gameFolder)
        If Not String.IsNullOrWhiteSpace(hookFromBat) Then
            removedAny = DeleteIfExists(Path.Combine(gameFolder, hookFromBat), log) OrElse removedAny
        Else
            For Each hook As String In GetKnownHookNames()
                Dim hookPath As String = Path.Combine(gameFolder, hook)
                If File.Exists(hookPath) Then
                    removedAny = DeleteIfExists(hookPath, log) OrElse removedAny
                End If
            Next
        End If

        removedAny = DeleteDirectoryIfExists(Path.Combine(gameFolder, "D3D12_Optiscaler"), log) OrElse removedAny
        removedAny = DeleteDirectoryIfExists(Path.Combine(gameFolder, "DlssOverrides"), log) OrElse removedAny
        removedAny = DeleteDirectoryIfExists(Path.Combine(gameFolder, "Licenses"), log) OrElse removedAny

        If removedAny Then
            log?.Invoke("Legacy uninstall complete.")
        End If

        Return removedAny
    End Function

    Private Shared Function GetHookNameFromUninstallBat(gameFolder As String) As String
        Dim batPath As String = Path.Combine(gameFolder, "Remove OptiScaler.bat")
        If Not File.Exists(batPath) Then
            Return ""
        End If

        Try
            For Each line As String In File.ReadAllLines(batPath)
                Dim trimmed As String = line.Trim()
                If Not trimmed.StartsWith("del ", StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                Dim target As String = trimmed.Substring(4).Trim()
                target = target.Trim(""""c)
                Dim fileName As String = Path.GetFileName(target)
                If String.IsNullOrWhiteSpace(fileName) Then
                    Continue For
                End If

                If fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) OrElse fileName.EndsWith(".asi", StringComparison.OrdinalIgnoreCase) Then
                    Return fileName
                End If
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "InstallerService.ParseUninstallBat")
        End Try

        Return ""
    End Function

    Private Shared Function GetKnownHookNames() As String()
        Return New String() {
            "OptiScaler.dll",
            "dxgi.dll",
            "winmm.dll",
            "version.dll",
            "dbghelp.dll",
            "d3d12.dll",
            "wininet.dll",
            "winhttp.dll",
            "OptiScaler.asi"
        }
    End Function

    Private Shared Function DeleteIfExists(filePath As String, log As Action(Of String)) As Boolean
        If String.IsNullOrWhiteSpace(filePath) OrElse Not File.Exists(filePath) Then
            Return False
        End If

        Try
            File.Delete(filePath)
            log?.Invoke("Removed file: " & Path.GetFileName(filePath))
            Return True
        Catch ex As Exception
            log?.Invoke("Failed to remove " & filePath & ": " & ex.Message)
            ErrorLogger.Log(ex, "InstallerService.DeleteFile")
            Return False
        End Try
    End Function

    Private Shared Function DeleteDirectoryIfExists(folderPath As String, log As Action(Of String)) As Boolean
        If String.IsNullOrWhiteSpace(folderPath) OrElse Not Directory.Exists(folderPath) Then
            Return False
        End If

        Try
            Directory.Delete(folderPath, True)
            log?.Invoke("Removed folder: " & Path.GetFileName(folderPath))
            Return True
        Catch ex As Exception
            log?.Invoke("Failed to remove folder " & folderPath & ": " & ex.Message)
            ErrorLogger.Log(ex, "InstallerService.DeleteFolder")
            Return False
        End Try
    End Function

    Private Shared Sub ValidateConfig(config As InstallerConfig)
        ' Hard validation before any download/copy operation starts.
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
        ' Resolves release metadata/local archive selection into a concrete archive path.
        If config.Source = ReleaseSource.LocalArchive Then
            log?.Invoke("Using local archive: " & config.LocalArchivePath)
            ValidateArchiveFile(config.LocalArchivePath, 0, log)
            Return New ArchiveResult With {
                .ArchivePath = config.LocalArchivePath,
                .Release = Nothing,
                .SourceUrl = "",
                .ExpectedSize = 0,
                .AssetName = Path.GetFileName(config.LocalArchivePath)
            }
        End If

        Dim release As ReleaseInfo = Await ResolveRemoteReleaseAsync(config, log)

        If release Is Nothing OrElse String.IsNullOrWhiteSpace(release.DownloadUrl) Then
            Throw New InvalidOperationException("Release download URL not available.")
        End If

        Dim sourceChannel As String = GetArchiveSourceChannel(config.Source)
        Dim safeName As String = If(String.IsNullOrWhiteSpace(release.AssetName), "OptiScaler.7z", release.AssetName)

        Dim cachedArchivePath As String = TryResolveCachedArchive(release, sourceChannel, log)
        If Not String.IsNullOrWhiteSpace(cachedArchivePath) Then
            ValidateArchiveFile(cachedArchivePath, release.Size, log)
            Return New ArchiveResult With {
                .ArchivePath = cachedArchivePath,
                .Release = release,
                .SourceUrl = release.DownloadUrl,
                .ExpectedSize = release.Size,
                .AssetName = safeName
            }
        End If

        log?.Invoke("Cached package not found or out-of-date. Downloading latest archive.")
        Dim destination As String = Path.Combine(tempRoot, Guid.NewGuid().ToString("N") & "_" & safeName)
        log?.Invoke("Downloading " & release.TagName & "...")
        Await DownloadFileAsync(release.DownloadUrl, destination, log, progress)
        ValidateArchiveFile(destination, release.Size, log)

        Dim finalArchivePath As String = destination
        Dim cachedPath As String = StoreArchiveInCache(destination, release, sourceChannel, log)
        If Not String.IsNullOrWhiteSpace(cachedPath) Then
            finalArchivePath = cachedPath
        End If

        Return New ArchiveResult With {
            .ArchivePath = finalArchivePath,
            .Release = release,
            .SourceUrl = release.DownloadUrl,
            .ExpectedSize = release.Size,
            .AssetName = safeName
        }
    End Function

    Private Shared Async Function ResolveRemoteReleaseAsync(config As InstallerConfig, log As Action(Of String)) As Task(Of ReleaseInfo)
        If config Is Nothing Then
            Return Nothing
        End If

        If config.Source = ReleaseSource.Stable Then
            Try
                Return Await ReleaseService.GetStableReleaseAsync()
            Catch ex As Exception
                ErrorLogger.Log(ex, "InstallerService.ResolveRemoteRelease.Stable")
                If config.StableRelease IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(config.StableRelease.DownloadUrl) Then
                    log?.Invoke("Stable release refresh failed; using already-loaded metadata.")
                    Return config.StableRelease
                End If
                Throw
            End Try
        End If

        If config.Source = ReleaseSource.Nightly Then
            Try
                Return Await ReleaseService.GetNightlyReleaseAsync()
            Catch ex As Exception
                ErrorLogger.Log(ex, "InstallerService.ResolveRemoteRelease.Nightly")
                If config.NightlyRelease IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(config.NightlyRelease.DownloadUrl) Then
                    log?.Invoke("Alternate release refresh failed; using already-loaded metadata.")
                    Return config.NightlyRelease
                End If
                Throw
            End Try
        End If

        Return Nothing
    End Function

    Private Shared Function GetArchiveSourceChannel(source As ReleaseSource) As String
        If source = ReleaseSource.Nightly Then
            Return "alternate"
        End If
        Return "stable"
    End Function

    Private Shared Function TryResolveCachedArchive(release As ReleaseInfo, sourceChannel As String, log As Action(Of String)) As String
        If release Is Nothing OrElse String.IsNullOrWhiteSpace(sourceChannel) Then
            Return ""
        End If

        Dim metadataPath As String = GetArchiveCacheMetadataPath(sourceChannel)
        Dim metadata As ArchiveCacheMetadata = TryLoadArchiveCacheMetadata(metadataPath)
        If metadata Is Nothing OrElse Not IsArchiveCacheMatch(metadata, release) Then
            Return ""
        End If
        If Not String.IsNullOrWhiteSpace(metadata.SourceChannel) AndAlso
           Not String.Equals(metadata.SourceChannel, sourceChannel, StringComparison.OrdinalIgnoreCase) Then
            Return ""
        End If

        Dim cacheRoot As String = GetArchiveCacheRoot()
        Dim cachedFileName As String = If(metadata.CachedFileName, "").Trim()
        If String.IsNullOrWhiteSpace(cachedFileName) Then
            Return ""
        End If

        Dim cachedPath As String = Path.Combine(cacheRoot, cachedFileName)
        If Not File.Exists(cachedPath) Then
            Return ""
        End If

        Try
            ValidateArchiveFile(cachedPath, release.Size, Nothing)

            If Not String.IsNullOrWhiteSpace(release.AssetDigest) Then
                Dim cachedSha As String = ComputeSha256(cachedPath)
                VerifyReleaseDigest(release, cachedSha, Nothing)
            End If
        Catch ex As Exception
            log?.Invoke("Cached package validation failed; downloading a fresh archive.")
            ErrorLogger.Log(ex, "InstallerService.TryResolveCachedArchive")
            Return ""
        End Try

        log?.Invoke("Using cached package: " & Path.GetFileName(cachedPath))
        Return cachedPath
    End Function

    Private Shared Function StoreArchiveInCache(archivePath As String, release As ReleaseInfo, sourceChannel As String, log As Action(Of String)) As String
        If String.IsNullOrWhiteSpace(archivePath) OrElse Not File.Exists(archivePath) Then
            Return ""
        End If
        If release Is Nothing OrElse String.IsNullOrWhiteSpace(sourceChannel) Then
            Return ""
        End If

        Try
            Dim cacheRoot As String = GetArchiveCacheRoot()
            Directory.CreateDirectory(cacheRoot)

            Dim cachedFileName As String = BuildCacheFileName(sourceChannel, release.AssetName)
            Dim cachedPath As String = Path.Combine(cacheRoot, cachedFileName)
            File.Copy(archivePath, cachedPath, True)

            Dim metadata As New ArchiveCacheMetadata With {
                .SourceChannel = sourceChannel,
                .TagName = If(release.TagName, ""),
                .AssetName = If(release.AssetName, ""),
                .DownloadUrl = If(release.DownloadUrl, ""),
                .Size = release.Size,
                .Digest = If(release.AssetDigest, ""),
                .CachedFileName = cachedFileName,
                .CachedAtUtc = DateTime.UtcNow
            }

            SaveArchiveCacheMetadata(GetArchiveCacheMetadataPath(sourceChannel), metadata)
            CleanupStaleCachedPackages(cacheRoot, sourceChannel, cachedFileName)
            log?.Invoke("Cached package updated: " & cachedFileName)
            Return cachedPath
        Catch ex As Exception
            ErrorLogger.Log(ex, "InstallerService.StoreArchiveInCache")
            Return ""
        End Try
    End Function

    Private Shared Function GetArchiveCacheRoot() As String
        Return Path.Combine(AppContext.BaseDirectory, "Cache", "OptiScaler")
    End Function

    Private Shared Function GetArchiveCacheMetadataPath(sourceChannel As String) As String
        Return Path.Combine(GetArchiveCacheRoot(), sourceChannel & ".cache.json")
    End Function

    Private Shared Function TryLoadArchiveCacheMetadata(filePath As String) As ArchiveCacheMetadata
        If String.IsNullOrWhiteSpace(filePath) OrElse Not File.Exists(filePath) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(filePath)
            Return JsonSerializer.Deserialize(Of ArchiveCacheMetadata)(json)
        Catch ex As Exception
            ErrorLogger.Log(ex, "InstallerService.TryLoadArchiveCacheMetadata")
            Return Nothing
        End Try
    End Function

    Private Shared Sub SaveArchiveCacheMetadata(filePath As String, metadata As ArchiveCacheMetadata)
        If String.IsNullOrWhiteSpace(filePath) OrElse metadata Is Nothing Then
            Return
        End If

        Dim folder As String = Path.GetDirectoryName(filePath)
        If Not String.IsNullOrWhiteSpace(folder) Then
            Directory.CreateDirectory(folder)
        End If

        Dim options As New JsonSerializerOptions With {
            .WriteIndented = True
        }
        Dim json As String = JsonSerializer.Serialize(metadata, options)
        File.WriteAllText(filePath, json)
    End Sub

    Private Shared Function IsArchiveCacheMatch(metadata As ArchiveCacheMetadata, release As ReleaseInfo) As Boolean
        If metadata Is Nothing OrElse release Is Nothing Then
            Return False
        End If

        If Not String.Equals(If(metadata.TagName, ""), If(release.TagName, ""), StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If
        If Not String.Equals(If(metadata.AssetName, ""), If(release.AssetName, ""), StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If
        If Not String.Equals(If(metadata.DownloadUrl, ""), If(release.DownloadUrl, ""), StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If
        If metadata.Size <> release.Size Then
            Return False
        End If
        If Not String.Equals(If(metadata.Digest, ""), If(release.AssetDigest, ""), StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If

        Return True
    End Function

    Private Shared Function BuildCacheFileName(sourceChannel As String, assetName As String) As String
        Dim safeAssetName As String = If(String.IsNullOrWhiteSpace(assetName), "OptiScaler.7z", assetName.Trim())
        For Each invalidChar As Char In Path.GetInvalidFileNameChars()
            safeAssetName = safeAssetName.Replace(invalidChar, "_"c)
        Next

        Return sourceChannel & "_" & safeAssetName
    End Function

    Private Shared Sub CleanupStaleCachedPackages(cacheRoot As String, sourceChannel As String, keepFileName As String)
        If String.IsNullOrWhiteSpace(cacheRoot) OrElse Not Directory.Exists(cacheRoot) Then
            Return
        End If

        Dim prefix As String = sourceChannel & "_"
        For Each filePath As String In Directory.GetFiles(cacheRoot, prefix & "*")
            Dim fileName As String = Path.GetFileName(filePath)
            If fileName.Equals(keepFileName, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            Try
                File.Delete(filePath)
            Catch ex As Exception
                ErrorLogger.Log(ex, "InstallerService.CleanupStaleCachedPackages")
            End Try
        Next
    End Sub

    Private Shared Async Function DownloadFileAsync(url As String, destination As String, log As Action(Of String), progress As Action(Of Integer)) As Task
        ' Streamed download with bounded retries for transient HTTP/network errors.
        Dim delay As TimeSpan = TimeSpan.FromMilliseconds(500)

        For attempt As Integer = 1 To MaxDownloadAttempts
            Dim retry As Boolean = False
            Try
                Using client As New HttpClient() With {.Timeout = DownloadTimeout}
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
                Return
            Catch ex As Exception
                If IsTransientDownloadException(ex) AndAlso attempt < MaxDownloadAttempts Then
                    retry = True
                    log?.Invoke($"Download attempt {attempt} failed. Retrying... ({ex.Message})")
                    ErrorLogger.Log(ex, "InstallerService.DownloadFileAsync.Retry")
                Else
                    Throw
                End If
            End Try

            If retry Then
                Await Task.Delay(delay)
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2)
            End If
        Next

        Throw New InvalidOperationException("Download failed after multiple attempts.")
    End Function

    Private Shared Sub ExtractArchive(archivePath As String, outputDir As String, log As Action(Of String))
        log?.Invoke("Extracting archive...")
        ' Use SharpCompress for .7z and fallback to generic archive open for other formats.

        Dim extension As String = Path.GetExtension(archivePath).ToLowerInvariant()
        If extension = ".7z" Then
            Using archive As SevenZipArchive = SevenZipArchive.Open(archivePath)
                ExtractEntries(archive.Entries, outputDir, log)
            End Using
        Else
            Using archive As IArchive = ArchiveFactory.Open(archivePath)
                ExtractEntries(archive.Entries, outputDir, log)
            End Using
        End If
        log?.Invoke("Extraction complete.")
    End Sub

    Private Shared Sub ExtractEntries(entries As IEnumerable(Of IArchiveEntry), outputDir As String, log As Action(Of String))
        Dim extractionRoot As String = NormalizePath(outputDir)
        For Each entry As IArchiveEntry In entries
            If entry.IsDirectory Then
                Continue For
            End If

            Dim safeDestination As String = ""
            Dim safeKey As String = ""
            If Not TryGetSafeExtractionPath(extractionRoot, entry.Key, safeDestination, safeKey) Then
                log?.Invoke("Skipping unsafe archive entry: " & If(entry.Key, "(null)"))
                Continue For
            End If

            Dim hasStream As Boolean = EntryHasStream(entry)
            If Not hasStream Then
                If entry.Size = 0 Then
                    CreateEmptyEntry(safeDestination, safeKey, log)
                Else
                    log?.Invoke("Skipping entry without stream: " & entry.Key)
                End If
                Continue For
            End If

            Try
                Dim destinationDir As String = Path.GetDirectoryName(safeDestination)
                If Not String.IsNullOrWhiteSpace(destinationDir) Then
                    Directory.CreateDirectory(destinationDir)
                End If

                entry.WriteToFile(safeDestination, New ExtractionOptions With {
                    .ExtractFullPath = False,
                    .Overwrite = True
                })
            Catch ex As Exception
                log?.Invoke("Failed to extract " & entry.Key & ": " & ex.Message)
                ErrorLogger.Log(ex, "InstallerService.ExtractEntry")
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
            Catch ex As Exception
                ErrorLogger.Log(ex, "InstallerService.EntryHasStream")
            End Try
        End If

        Return True
    End Function

    Private Shared Sub CreateEmptyEntry(destination As String, safeKey As String, log As Action(Of String))
        If String.IsNullOrWhiteSpace(destination) Then
            Return
        End If

        Dim folder As String = Path.GetDirectoryName(destination)
        If Not String.IsNullOrWhiteSpace(folder) Then
            Directory.CreateDirectory(folder)
        End If

        If Not File.Exists(destination) Then
            File.WriteAllText(destination, "")
            log?.Invoke("Created empty file: " & If(safeKey, Path.GetFileName(destination)))
        End If
    End Sub

    ' Resolves an archive entry path and rejects traversal/rooted entries.
    Private Shared Function TryGetSafeExtractionPath(extractionRoot As String,
                                                     entryKey As String,
                                                     ByRef destination As String,
                                                     ByRef safeKey As String) As Boolean
        destination = ""
        safeKey = ""

        If String.IsNullOrWhiteSpace(extractionRoot) OrElse String.IsNullOrWhiteSpace(entryKey) Then
            Return False
        End If

        Dim normalizedKey As String = entryKey.Trim().Replace("/"c, Path.DirectorySeparatorChar).Replace("\"c, Path.DirectorySeparatorChar)
        normalizedKey = normalizedKey.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        If String.IsNullOrWhiteSpace(normalizedKey) Then
            Return False
        End If

        If Path.IsPathRooted(normalizedKey) Then
            Return False
        End If

        Try
            Dim fullDestination As String = Path.GetFullPath(Path.Combine(extractionRoot, normalizedKey))
            If Not IsPathInsideRoot(fullDestination, extractionRoot) Then
                Return False
            End If

            destination = fullDestination
            safeKey = normalizedKey
            Return True
        Catch ex As Exception
            ErrorLogger.Log(ex, "InstallerService.TryGetSafeExtractionPath")
            Return False
        End Try
    End Function

    ' Ensures manifest references only target files inside the selected game folder.
    Private Shared Function ResolveManifestPath(gameRoot As String, manifestPathValue As String) As String
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
            ErrorLogger.Log(ex, "InstallerService.ResolveManifestPath")
            Return ""
        End Try
    End Function

    Private Shared Function NormalizePath(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Try
            Dim fullPath As String = Path.GetFullPath(value.Trim())
            Return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Catch ex As Exception
            ErrorLogger.Log(ex, "InstallerService.NormalizePath")
            Return ""
        End Try
    End Function

    Private Shared Function IsPathInsideRoot(candidatePath As String, rootPath As String) As Boolean
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

    Private Shared Function IsTransientDownloadException(ex As Exception) As Boolean
        If ex Is Nothing Then
            Return False
        End If

        If TypeOf ex Is HttpRequestException Then
            Return True
        End If

        If TypeOf ex Is TaskCanceledException Then
            Return True
        End If

        Return False
    End Function

    Private Shared Function ResolvePackageRoot(extractRoot As String) As String
        ' Prefer the folder that directly contains OptiScaler.dll, plus OptiScaler.ini when available.
        Dim directDll As String = Path.Combine(extractRoot, "OptiScaler.dll")
        If File.Exists(directDll) Then
            Return extractRoot
        End If

        Dim matches As String() = Directory.GetFiles(extractRoot, "OptiScaler.dll", SearchOption.AllDirectories)
        If matches.Length = 0 Then
            Throw New InvalidOperationException("OptiScaler.dll was not found in the extracted archive.")
        End If

        If matches.Length = 1 Then
            Return Path.GetDirectoryName(matches(0))
        End If

        Dim bestMatch As String = matches(0)
        Dim bestDepth As Integer = GetPathDepth(extractRoot, bestMatch)
        Dim bestHasIni As Boolean = File.Exists(Path.Combine(Path.GetDirectoryName(bestMatch), "OptiScaler.ini"))

        For Each match As String In matches
            Dim depth As Integer = GetPathDepth(extractRoot, match)
            Dim hasIni As Boolean = File.Exists(Path.Combine(Path.GetDirectoryName(match), "OptiScaler.ini"))

            If hasIni AndAlso Not bestHasIni Then
                bestMatch = match
                bestDepth = depth
                bestHasIni = True
                Continue For
            End If

            If hasIni = bestHasIni AndAlso depth < bestDepth Then
                bestMatch = match
                bestDepth = depth
            End If
        Next

        Return Path.GetDirectoryName(bestMatch)
    End Function

    Private Shared Function GetPathDepth(root As String, fullPath As String) As Integer
        Dim relative As String = Path.GetRelativePath(root, fullPath)
        Dim parts As String() = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Return parts.Length
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

        ' Handle existing files according to the selected conflict mode.
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

        ' Apply runtime overrides for spoofing, frame gen, and plugin toggles.
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
        ' Copy optional addon files if they are configured and present.
        Dim bundledRelease As Boolean = IsLikelyBundledComponentRelease(config)
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
                If bundledRelease AndAlso File.Exists(Path.Combine(config.GameFolder, "nvapi64.dll")) Then
                    log?.Invoke("Fakenvapi files not found in selected folder. Using bundled nvapi64.dll from OptiScaler package.")
                Else
                    log?.Invoke("Fakenvapi files not found in selected folder.")
                End If
            End If
        ElseIf bundledRelease AndAlso config.GpuVendor = GpuVendor.AmdIntel AndAlso File.Exists(Path.Combine(config.GameFolder, "nvapi64.dll")) Then
            log?.Invoke("Using bundled Fakenvapi/nvapi files for AMD/Intel mode.")
        End If

        If Not String.IsNullOrWhiteSpace(config.NukemDllPath) Then
            If File.Exists(config.NukemDllPath) Then
                Dim dest As String = Path.Combine(config.GameFolder, "dlssg_to_fsr3_amd_is_better.dll")
                CopyFileWithConflict(config.NukemDllPath, dest, config.ConflictMode, manifest, log)
                log?.Invoke("Copied Nukem dlssg_to_fsr3_amd_is_better.dll.")
            Else
                log?.Invoke("Nukem DLL path not found.")
            End If
        ElseIf config.FgType = FgTypeSelection.Nukem Then
            Dim bundledNukem As String = Path.Combine(config.GameFolder, "dlssg_to_fsr3_amd_is_better.dll")
            If File.Exists(bundledNukem) Then
                log?.Invoke("Using bundled Nukem dlssg_to_fsr3_amd_is_better.dll.")
            Else
                log?.Invoke("Nukem frame generation selected but dll was not found after install.")
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

    Private Shared Sub CleanupPackageArtifacts(gameFolder As String, log As Action(Of String))
        Dim names As String() = {
            "!! EXTRACT ALL FILES TO GAME FOLDER !!",
            "setup_windows.bat",
            "setup.bat",
            "setup_linux.sh"
        }

        For Each name As String In names
            Dim artifactPath As String = Path.Combine(gameFolder, name)
            If File.Exists(artifactPath) Then
                Try
                    File.Delete(artifactPath)
                    log?.Invoke("Removed setup artifact: " & name)
                Catch ex As Exception
                    log?.Invoke("Failed to remove setup artifact " & name & ": " & ex.Message)
                    ErrorLogger.Log(ex, "InstallerService.CleanupPackageArtifacts")
                End Try
            End If
        Next
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
        Catch ex As Exception
            ErrorLogger.Log(ex, "InstallerService.TryGetFileVersion")
            Return ""
        End Try
    End Function

    Private Shared Sub SaveManifest(gameFolder As String, manifest As InstallManifest)
        Dim manifestPath As String = Path.Combine(gameFolder, ManifestName)
        Dim json As String = JsonSerializer.Serialize(manifest, New JsonSerializerOptions With {.WriteIndented = True})
        File.WriteAllText(manifestPath, json)
    End Sub

    Public Shared Function VerifyInstall(config As InstallerConfig, manifest As InstallManifest) As InstallVerificationReport
        Dim report As New InstallVerificationReport()
        If config Is Nothing Then
            report.Errors.Add("Installer configuration was missing for verification.")
            Return report
        End If

        Dim gameFolder As String = config.GameFolder
        If String.IsNullOrWhiteSpace(gameFolder) OrElse Not Directory.Exists(gameFolder) Then
            report.Errors.Add("Game folder is missing after install.")
            Return report
        End If

        Dim hookPath As String = Path.Combine(gameFolder, config.HookName)
        If File.Exists(hookPath) Then
            report.Passed.Add("Hook file present: " & config.HookName)
        Else
            report.Errors.Add("Hook file missing: " & config.HookName)
        End If

        Dim iniPath As String = Path.Combine(gameFolder, "OptiScaler.ini")
        If File.Exists(iniPath) Then
            report.Passed.Add("OptiScaler.ini present.")
        Else
            report.Errors.Add("OptiScaler.ini missing.")
        End If

        Dim manifestPath As String = Path.Combine(gameFolder, ManifestName)
        If File.Exists(manifestPath) Then
            report.Passed.Add("Installer manifest written.")
        Else
            report.Warnings.Add("Installer manifest was not found after install.")
        End If

        If Not String.IsNullOrWhiteSpace(manifest?.ArchiveSha256) Then
            report.Passed.Add("Archive fingerprint recorded: " & ShortHash(manifest.ArchiveSha256))
        End If

        If File.Exists(iniPath) Then
            Dim iniValues As Dictionary(Of String, String) = ReadIniMap(iniPath)
            ValidateIniExpectation(iniValues, "FrameGen", "FGType", ExpectedFgType(config), report)

            Dim expectedDxgi As String = "auto"
            If config.GpuVendor = GpuVendor.AmdIntel Then
                expectedDxgi = If(config.EnableDlssInputs, "auto", "false")
            End If
            ValidateIniExpectation(iniValues, "Spoofing", "Dxgi", expectedDxgi, report)
            ValidateIniExpectation(iniValues, "Plugins", "LoadAsiPlugins", ToIniBool(config.LoadAsiPlugins), report)
            ValidateIniExpectation(iniValues, "Plugins", "LoadReshade", ToIniBool(config.EnableReshade), report)
            ValidateIniExpectation(iniValues, "Plugins", "LoadSpecialK", ToIniBool(config.EnableSpecialK), report)
        End If

        If config.FgType = FgTypeSelection.Nukem Then
            Dim nukemPath As String = Path.Combine(gameFolder, "dlssg_to_fsr3_amd_is_better.dll")
            If File.Exists(nukemPath) Then
                report.Passed.Add("Nukem DLL present.")
            Else
                report.Warnings.Add("Nukem frame generation selected but dlssg_to_fsr3_amd_is_better.dll was not found.")
            End If
        End If

        If config.EnableReshade Then
            Dim reshadePath As String = Path.Combine(gameFolder, "ReShade64.dll")
            If File.Exists(reshadePath) Then
                report.Passed.Add("ReShade64.dll present.")
            Else
                report.Warnings.Add("ReShade integration enabled but ReShade64.dll was not found.")
            End If
        End If

        If config.EnableSpecialK Then
            Dim specialKPath As String = Path.Combine(gameFolder, "SpecialK64.dll")
            If File.Exists(specialKPath) Then
                report.Passed.Add("SpecialK64.dll present.")
            Else
                report.Warnings.Add("Special K enabled but SpecialK64.dll was not found.")
            End If
        End If

        If config.LoadAsiPlugins AndAlso String.IsNullOrWhiteSpace(config.PluginsPath) Then
            report.Warnings.Add("ASI plugins enabled with an empty plugin path.")
        End If

        If report.Errors.Count = 0 Then
            report.Passed.Add("Post-install verification passed.")
        End If

        Return report
    End Function

    Private Shared Function ReadIniMap(path As String) As Dictionary(Of String, String)
        Dim map As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Dim values As List(Of IniUpdate) = IniFile.ReadValues(path)
        For Each update As IniUpdate In values
            If update Is Nothing OrElse String.IsNullOrWhiteSpace(update.Section) OrElse String.IsNullOrWhiteSpace(update.Key) Then
                Continue For
            End If

            Dim mapKey As String = update.Section.Trim() & "|" & update.Key.Trim()
            map(mapKey) = If(update.Value, "")
        Next

        Return map
    End Function

    Private Shared Sub ValidateIniExpectation(map As Dictionary(Of String, String),
                                              section As String,
                                              key As String,
                                              expected As String,
                                              report As InstallVerificationReport)
        If map Is Nothing OrElse report Is Nothing Then
            Return
        End If

        Dim mapKey As String = section & "|" & key
        Dim actual As String = ""
        If Not map.TryGetValue(mapKey, actual) Then
            report.Warnings.Add($"INI key missing: [{section}] {key}")
            Return
        End If

        If String.Equals(actual, expected, StringComparison.OrdinalIgnoreCase) Then
            report.Passed.Add($"INI [{section}] {key}={actual}")
        Else
            report.Warnings.Add($"INI [{section}] {key} expected '{expected}' but found '{actual}'.")
        End If
    End Sub

    Private Shared Function ExpectedFgType(config As InstallerConfig) As String
        Select Case config.FgType
            Case FgTypeSelection.None
                Return "nofg"
            Case FgTypeSelection.OptiFg
                Return "optifg"
            Case FgTypeSelection.Nukem
                Return "nukems"
            Case Else
                Return "auto"
        End Select
    End Function

    Private Shared Sub ValidateArchiveFile(filePath As String, expectedSize As Long, log As Action(Of String))
        If String.IsNullOrWhiteSpace(filePath) OrElse Not File.Exists(filePath) Then
            Throw New FileNotFoundException("OptiScaler archive file was not found.")
        End If

        Dim extension As String = Path.GetExtension(filePath).ToLowerInvariant()
        Dim supported As String() = {".7z", ".zip", ".rar", ".tar", ".gz", ".xz", ".bz2", ".lz"}
        If Not supported.Contains(extension, StringComparer.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Unsupported archive extension: " & extension)
        End If

        Dim actualSize As Long = GetFileSizeSafe(filePath)
        If actualSize <= 0 Then
            Throw New InvalidOperationException("Downloaded archive is empty.")
        End If

        If expectedSize > 0 AndAlso actualSize <> expectedSize Then
            Throw New InvalidOperationException($"Archive size mismatch. Expected {expectedSize} bytes, got {actualSize} bytes.")
        End If

        log?.Invoke("Archive size: " & actualSize.ToString() & " bytes")
    End Sub

    ' Validates a downloaded archive against the release digest (when provided).
    Private Shared Sub VerifyReleaseDigest(release As ReleaseInfo, archiveSha256 As String, log As Action(Of String))
        If release Is Nothing Then
            Return
        End If

        Dim digestText As String = If(release.AssetDigest, "").Trim()
        If String.IsNullOrWhiteSpace(digestText) Then
            log?.Invoke("Release digest not provided by source; skipping digest validation.")
            Return
        End If

        Dim expectedSha As String = ""
        If Not TryExtractSha256Digest(digestText, expectedSha) Then
            log?.Invoke("Release digest format not recognized: " & digestText)
            Return
        End If

        If String.IsNullOrWhiteSpace(archiveSha256) Then
            Throw New InvalidOperationException("Archive checksum could not be computed.")
        End If

        If Not archiveSha256.Equals(expectedSha, StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException("Downloaded archive digest mismatch. Installation aborted.")
        End If

        log?.Invoke("Archive digest verified.")
    End Sub

    Private Shared Function TryExtractSha256Digest(digestText As String, ByRef sha256 As String) As Boolean
        sha256 = ""
        If String.IsNullOrWhiteSpace(digestText) Then
            Return False
        End If

        Dim trimmed As String = digestText.Trim()
        If trimmed.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) Then
            trimmed = trimmed.Substring("sha256:".Length)
        End If

        trimmed = trimmed.Trim().ToLowerInvariant()
        If Regex.IsMatch(trimmed, "^[a-f0-9]{64}$", RegexOptions.CultureInvariant) Then
            sha256 = trimmed
            Return True
        End If

        Return False
    End Function

    Private Shared Function GetFileSizeSafe(path As String) As Long
        Try
            If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
                Return 0
            End If

            Dim info As New FileInfo(path)
            Return info.Length
        Catch ex As Exception
            ErrorLogger.Log(ex, "InstallerService.GetFileSizeSafe")
            Return 0
        End Try
    End Function

    Private Shared Function ComputeSha256(path As String) As String
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return ""
        End If

        Using stream As FileStream = File.OpenRead(path)
            Using sha As SHA256 = SHA256.Create()
                Dim hash As Byte() = sha.ComputeHash(stream)
                Return Convert.ToHexString(hash).ToLowerInvariant()
            End Using
        End Using
    End Function

    Private Shared Function ShortHash(hash As String) As String
        If String.IsNullOrWhiteSpace(hash) Then
            Return "n/a"
        End If

        If hash.Length <= 12 Then
            Return hash
        End If

        Return hash.Substring(0, 12)
    End Function

    Private Shared Function GetSelectedRelease(config As InstallerConfig) As ReleaseInfo
        If config Is Nothing Then
            Return Nothing
        End If

        If config.Source = ReleaseSource.Stable Then
            Return config.StableRelease
        End If
        If config.Source = ReleaseSource.Nightly Then
            Return config.NightlyRelease
        End If

        Return Nothing
    End Function

    Private Shared Function ParseOptiScalerVersion(text As String) As Version
        If String.IsNullOrWhiteSpace(text) Then
            Return Nothing
        End If

        Dim cleaned As String = text.Trim()
        Dim match As Match = Regex.Match(cleaned, "(?<version>\d+(\.\d+){1,3})")
        If Not match.Success Then
            Return Nothing
        End If

        Dim versionText As String = match.Groups("version").Value
        Dim parts As String() = versionText.Split("."c)
        Dim major As Integer = SafeParseVersionPart(parts, 0)
        Dim minor As Integer = SafeParseVersionPart(parts, 1)
        Dim build As Integer = SafeParseVersionPart(parts, 2)
        Dim revision As Integer = SafeParseVersionPart(parts, 3)
        Return New Version(major, minor, build, revision)
    End Function

    Private Shared Function SafeParseVersionPart(parts As String(), index As Integer) As Integer
        If parts Is Nothing OrElse index < 0 OrElse index >= parts.Length Then
            Return 0
        End If

        Dim value As Integer
        If Integer.TryParse(parts(index), value) Then
            Return value
        End If
        Return 0
    End Function

    Private Shared Function ToIniBool(value As Boolean) As String
        Return If(value, "true", "false")
    End Function
End Class
