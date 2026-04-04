Imports System.IO
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Linq
Imports System.Collections.Concurrent
Imports System.Threading.Tasks
Imports System.Threading
Imports Microsoft.Win32

Public Class DetectionService
    Public Class DeepScanProgressInfo
        Public Property RootPath As String
        Public Property CurrentDirectory As String
        Public Property ScannedFoldersInRoot As Integer
        Public Property MaxFoldersPerRoot As Integer
        Public Property TotalRoots As Integer
        Public Property RootsCompleted As Integer
    End Class

    ' Scans known launchers and matches installs against the compatibility list.
    Public Shared Function DetectSupportedGames(entries As IEnumerable(Of CompatibilityEntry),
                                                log As Action(Of String)) As List(Of DetectedGame)
        Dim results As New List(Of DetectedGame)()
        If entries Is Nothing Then
            Return results
        End If

        Dim matcher As New CompatibilityMatcher(entries)
        Dim seenPaths As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        AddSteamGames(matcher, results, seenPaths, log)
        AddEpicGames(matcher, results, seenPaths, log)
        AddGogGames(matcher, results, seenPaths, log)
        AddEaGames(matcher, results, seenPaths, log)
        AddUbisoftGames(matcher, results, seenPaths, log)

        results.Sort(Function(left, right) StringComparer.OrdinalIgnoreCase.Compare(left.DisplayName, right.DisplayName))
        Return results
    End Function

    Private Shared Sub AddSteamGames(matcher As CompatibilityMatcher, results As List(Of DetectedGame), seenPaths As HashSet(Of String), log As Action(Of String))
        ' Steam installs are read from libraryfolders.vdf and appmanifest_*.acf.
        Dim steamPath As String = GetSteamPath()
        If String.IsNullOrWhiteSpace(steamPath) OrElse Not Directory.Exists(steamPath) Then
            If log IsNot Nothing Then
                log("Steam not detected.")
            End If
            Return
        End If

        If log IsNot Nothing Then
            log("Scanning Steam libraries...")
        End If

        Dim libraryPaths As List(Of String) = GetSteamLibraries(steamPath)
        For Each libraryPath As String In libraryPaths
            Dim steamApps As String = Path.Combine(libraryPath, "steamapps")
            If Not Directory.Exists(steamApps) Then
                Continue For
            End If

            For Each manifestPath As String In Directory.GetFiles(steamApps, "appmanifest_*.acf")
                Dim content As String
                Try
                    content = File.ReadAllText(manifestPath)
                Catch ex As Exception
                    ErrorLogger.Log(ex, "DetectionService.AddSteamGames.ReadManifest")
                    Continue For
                End Try

                Dim name As String = TryGetVdfValue(content, "name")
                Dim installDir As String = TryGetVdfValue(content, "installdir")
                If String.IsNullOrWhiteSpace(name) OrElse String.IsNullOrWhiteSpace(installDir) Then
                    Continue For
                End If

                Dim gamePath As String = Path.Combine(steamApps, "common", installDir)
                AddIfSupported(matcher, results, seenPaths, name, gamePath, "Steam")
            Next
        Next
    End Sub

    Private Shared Sub AddEpicGames(matcher As CompatibilityMatcher, results As List(Of DetectedGame), seenPaths As HashSet(Of String), log As Action(Of String))
        ' Epic installs are read from the launcher manifest JSON files.
        Dim manifestRoot As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic", "EpicGamesLauncher", "Data", "Manifests")
        If Not Directory.Exists(manifestRoot) Then
            If log IsNot Nothing Then
                log("Epic Games Launcher not detected.")
            End If
            Return
        End If

        If log IsNot Nothing Then
            log("Scanning Epic manifests...")
        End If

        For Each itemPath As String In Directory.GetFiles(manifestRoot, "*.item")
            Try
                Using doc As JsonDocument = JsonDocument.Parse(File.ReadAllText(itemPath))
                    Dim name As String = TryGetJsonString(doc.RootElement, "DisplayName")
                    Dim installDir As String = TryGetJsonString(doc.RootElement, "InstallLocation")
                    AddIfSupported(matcher, results, seenPaths, name, installDir, "Epic")
                End Using
            Catch ex As Exception
                ErrorLogger.Log(ex, "DetectionService.AddEpicGames.ReadManifest")
                Continue For
            End Try
        Next
    End Sub

    Private Shared Sub AddGogGames(matcher As CompatibilityMatcher, results As List(Of DetectedGame), seenPaths As HashSet(Of String), log As Action(Of String))
        ' GOG installs are discovered through registry keys.
        Dim roots As String() = {
            "Software\GOG.com\Games",
            "Software\WOW6432Node\GOG.com\Games"
        }

        Dim foundAny As Boolean = False
        For Each rootPath As String In roots
            Using rootKey As RegistryKey = Registry.LocalMachine.OpenSubKey(rootPath)
                If rootKey Is Nothing Then
                    Continue For
                End If

                For Each subKeyName As String In rootKey.GetSubKeyNames()
                    Using gameKey As RegistryKey = rootKey.OpenSubKey(subKeyName)
                        If gameKey Is Nothing Then
                            Continue For
                        End If

                        Dim name As String = Convert.ToString(gameKey.GetValue("gameName"))
                        If String.IsNullOrWhiteSpace(name) Then
                            name = Convert.ToString(gameKey.GetValue("name"))
                        End If

                        Dim installDir As String = Convert.ToString(gameKey.GetValue("path"))
                        AddIfSupported(matcher, results, seenPaths, name, installDir, "GOG")
                        foundAny = True
                    End Using
                Next
            End Using
        Next

        If log IsNot Nothing AndAlso Not foundAny Then
            log("GOG Galaxy not detected.")
        End If
    End Sub

    Private Shared Sub AddEaGames(matcher As CompatibilityMatcher, results As List(Of DetectedGame), seenPaths As HashSet(Of String), log As Action(Of String))
        ' EA installs are discovered through registry keys.
        Dim roots As String() = {
            "Software\EA Games",
            "Software\WOW6432Node\EA Games",
            "Software\Electronic Arts\EA Games",
            "Software\WOW6432Node\Electronic Arts\EA Games"
        }

        Dim foundAny As Boolean = False
        For Each rootPath As String In roots
            Using rootKey As RegistryKey = Registry.LocalMachine.OpenSubKey(rootPath)
                If rootKey Is Nothing Then
                    Continue For
                End If

                foundAny = True
                For Each subKeyName As String In rootKey.GetSubKeyNames()
                    Using gameKey As RegistryKey = rootKey.OpenSubKey(subKeyName)
                        If gameKey Is Nothing Then
                            Continue For
                        End If

                        Dim name As String = Convert.ToString(gameKey.GetValue("DisplayName"))
                        If String.IsNullOrWhiteSpace(name) Then
                            name = Convert.ToString(gameKey.GetValue("name"))
                        End If
                        If String.IsNullOrWhiteSpace(name) Then
                            name = Convert.ToString(gameKey.GetValue("gameName"))
                        End If

                        Dim installDir As String = Convert.ToString(gameKey.GetValue("Install Dir"))
                        If String.IsNullOrWhiteSpace(installDir) Then
                            installDir = Convert.ToString(gameKey.GetValue("InstallDir"))
                        End If
                        If String.IsNullOrWhiteSpace(installDir) Then
                            installDir = Convert.ToString(gameKey.GetValue("InstallLocation"))
                        End If
                        If String.IsNullOrWhiteSpace(installDir) Then
                            installDir = Convert.ToString(gameKey.GetValue("Install Path"))
                        End If

                        AddIfSupported(matcher, results, seenPaths, name, installDir, "EA App")
                    End Using
                Next
            End Using
        Next

        If log IsNot Nothing AndAlso Not foundAny Then
            log("EA App not detected.")
        End If
    End Sub

    Private Shared Sub AddUbisoftGames(matcher As CompatibilityMatcher, results As List(Of DetectedGame), seenPaths As HashSet(Of String), log As Action(Of String))
        ' Ubisoft installs are discovered through Ubisoft Connect registry keys.
        Dim roots As String() = {
            "Software\Ubisoft\Launcher\Installs",
            "Software\WOW6432Node\Ubisoft\Launcher\Installs"
        }

        Dim foundAny As Boolean = False
        For Each rootPath As String In roots
            Using rootKey As RegistryKey = Registry.LocalMachine.OpenSubKey(rootPath)
                If rootKey Is Nothing Then
                    Continue For
                End If

                foundAny = True
                For Each subKeyName As String In rootKey.GetSubKeyNames()
                    Using gameKey As RegistryKey = rootKey.OpenSubKey(subKeyName)
                        If gameKey Is Nothing Then
                            Continue For
                        End If

                        Dim installDir As String = Convert.ToString(gameKey.GetValue("InstallDir"))
                        If String.IsNullOrWhiteSpace(installDir) Then
                            installDir = Convert.ToString(gameKey.GetValue("InstallLocation"))
                        End If
                        If String.IsNullOrWhiteSpace(installDir) Then
                            installDir = Convert.ToString(gameKey.GetValue("Path"))
                        End If

                        Dim name As String = Convert.ToString(gameKey.GetValue("DisplayName"))
                        If String.IsNullOrWhiteSpace(name) Then
                            name = Convert.ToString(gameKey.GetValue("Name"))
                        End If
                        If String.IsNullOrWhiteSpace(name) AndAlso Not String.IsNullOrWhiteSpace(installDir) Then
                            name = Path.GetFileName(installDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                        End If

                        AddIfSupported(matcher, results, seenPaths, name, installDir, "Ubisoft")
                    End Using
                Next
            End Using
        Next

        If log IsNot Nothing AndAlso Not foundAny Then
            log("Ubisoft Connect not detected.")
        End If
    End Sub

    Private Shared Sub AddIfSupported(matcher As CompatibilityMatcher, results As List(Of DetectedGame), seenPaths As HashSet(Of String), displayName As String, installDir As String, platform As String)
        If String.IsNullOrWhiteSpace(displayName) OrElse String.IsNullOrWhiteSpace(installDir) Then
            Return
        End If

        Dim trimmedPath As String = NormalizeInstallPath(installDir)
        If String.IsNullOrWhiteSpace(trimmedPath) OrElse Not Directory.Exists(trimmedPath) Then
            Return
        End If

        If seenPaths.Contains(trimmedPath) Then
            Return
        End If

        Dim match As CompatibilityEntry = matcher.Match(displayName)
        If match Is Nothing Then
            Return
        End If

        Dim antiCheat As AntiCheatScanResult = AntiCheatService.Detect(trimmedPath)
        Dim antiCheatProvider As String = ""
        If antiCheat IsNot Nothing AndAlso antiCheat.Detected Then
            antiCheatProvider = antiCheat.Provider
        End If

        results.Add(New DetectedGame With {
            .DisplayName = match.Name,
            .Platform = platform,
            .InstallDir = trimmedPath,
            .MatchedEntry = match,
            .SourceName = displayName,
            .AntiCheat = antiCheatProvider
        })
        seenPaths.Add(trimmedPath)
    End Sub

    Public Shared Function GetScannableDriveRoots(log As Action(Of String)) As List(Of String)
        Dim roots As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each drive As DriveInfo In DriveInfo.GetDrives()
            Try
                If Not drive.IsReady Then
                    Continue For
                End If

                Select Case drive.DriveType
                    Case DriveType.Fixed, DriveType.Removable, DriveType.Network
                        Dim root As String = NormalizeInstallPath(drive.RootDirectory.FullName)
                        If String.IsNullOrWhiteSpace(root) OrElse seen.Contains(root) Then
                            Continue For
                        End If

                        roots.Add(root)
                        seen.Add(root)
                End Select
            Catch ex As Exception
                ErrorLogger.Log(ex, "DetectionService.GetScannableDriveRoots")
            End Try
        Next

        roots.Sort(StringComparer.OrdinalIgnoreCase)
        If log IsNot Nothing Then
            log("Scannable drives: " & If(roots.Count = 0, "none", String.Join(", ", roots)))
        End If

        Return roots
    End Function

    Public Shared Function DetectSupportedGamesByDriveScan(entries As IEnumerable(Of CompatibilityEntry),
                                                           driveRoots As IEnumerable(Of String),
                                                           log As Action(Of String),
                                                           Optional progress As Action(Of DeepScanProgressInfo) = Nothing) As List(Of DetectedGame)
        Dim results As New List(Of DetectedGame)()
        If entries Is Nothing OrElse driveRoots Is Nothing Then
            Return results
        End If

        Dim roots As List(Of String) = driveRoots.
            Where(Function(path) Not String.IsNullOrWhiteSpace(path)).
            Select(Function(path) NormalizeInstallPath(path)).
            Where(Function(path) Not String.IsNullOrWhiteSpace(path) AndAlso Directory.Exists(path)).
            Distinct(StringComparer.OrdinalIgnoreCase).
            ToList()

        If roots.Count = 0 Then
            If log IsNot Nothing Then
                log("Deep scan skipped: no valid drive roots selected.")
            End If
            Return results
        End If

        Dim matcher As New CompatibilityMatcher(entries)
        Dim seenPaths As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim candidates As New ConcurrentDictionary(Of String, DeepScanCandidate)(StringComparer.OrdinalIgnoreCase)
        Dim options As New ParallelOptions With {
            .MaxDegreeOfParallelism = Math.Max(1, Math.Min(Environment.ProcessorCount, 6))
        }
        Dim totalRoots As Integer = roots.Count
        Dim completedRoots As Integer = 0

        If log IsNot Nothing Then
            log("Deep scan started for drives: " & String.Join(", ", roots))
        End If

        Parallel.ForEach(roots, options, Sub(root)
                                             ScanDriveRoot(matcher,
                                                          root,
                                                          candidates,
                                                          log,
                                                          totalRoots,
                                                          Function() Volatile.Read(completedRoots),
                                                          progress)
                                             Interlocked.Increment(completedRoots)
                                         End Sub)

        For Each candidate As DeepScanCandidate In candidates.Values.OrderBy(Function(item) item.DisplayName, StringComparer.OrdinalIgnoreCase)
            AddIfSupported(matcher, results, seenPaths, candidate.DisplayName, candidate.InstallDir, candidate.Platform)
        Next

        results.Sort(Function(left, right) StringComparer.OrdinalIgnoreCase.Compare(left.DisplayName, right.DisplayName))
        Return results
    End Function

    Private Shared Sub ScanDriveRoot(matcher As CompatibilityMatcher,
                                     root As String,
                                     candidates As ConcurrentDictionary(Of String, DeepScanCandidate),
                                     log As Action(Of String),
                                     totalRoots As Integer,
                                     completedRootsProvider As Func(Of Integer),
                                     progress As Action(Of DeepScanProgressInfo))
        Const maxDepth As Integer = 9
        Const progressUpdateInterval As Integer = 200
        Dim queue As New Queue(Of ScanNode)()
        queue.Enqueue(New ScanNode With {.FolderPath = root, .Depth = 0})
        Dim visited As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim scannedFolders As Integer = 0
        Dim localHits As Integer = 0
        Dim nextProgressFolderCount As Integer = 1

        If log IsNot Nothing Then
            log("Deep scan: scanning " & root)
        End If
        ReportDeepScanProgress(progress, root, root, scannedFolders, totalRoots, completedRootsProvider, False)

        While queue.Count > 0 AndAlso scannedFolders < ScanRootFolderBudget
            Dim node As ScanNode = queue.Dequeue()
            Dim normalized As String = NormalizeInstallPath(node.FolderPath)
            If String.IsNullOrWhiteSpace(normalized) OrElse visited.Contains(normalized) Then
                Continue While
            End If
            visited.Add(normalized)

            If ShouldSkipScanDirectory(normalized, node.Depth = 0) Then
                Continue While
            End If

            scannedFolders += 1
            If scannedFolders >= nextProgressFolderCount Then
                ReportDeepScanProgress(progress, root, normalized, scannedFolders, totalRoots, completedRootsProvider, False)
                nextProgressFolderCount = scannedFolders + progressUpdateInterval
            End If

            Dim folderName As String = Path.GetFileName(normalized)
            If String.IsNullOrWhiteSpace(folderName) AndAlso node.Depth = 0 Then
                folderName = normalized
            End If

            Dim folderMatch As CompatibilityEntry = matcher.Match(folderName)
            If folderMatch IsNot Nothing Then
                Dim installDir As String = ResolveMatchedFolderInstallDir(normalized)
                If Not String.IsNullOrWhiteSpace(installDir) Then
                    If candidates.TryAdd(installDir, New DeepScanCandidate With {
                                         .DisplayName = folderMatch.Name,
                                         .InstallDir = installDir,
                                         .Platform = "Drive scan"
                                     }) Then
                        localHits += 1
                    End If
                End If
            End If

            Dim executables As IEnumerable(Of String) = Enumerable.Empty(Of String)()
            Try
                executables = Directory.EnumerateFiles(normalized, "*.exe", SearchOption.TopDirectoryOnly)
            Catch ex As Exception
                ErrorLogger.Log(ex, "DetectionService.ScanDriveRoot.EnumerateExe")
            End Try

            For Each exePath As String In executables
                Dim exeName As String = Path.GetFileNameWithoutExtension(exePath)
                If ShouldSkipExecutable(exeName) Then
                    Continue For
                End If

                Dim exeMatch As CompatibilityEntry = matcher.Match(exeName)
                If exeMatch Is Nothing Then
                    Continue For
                End If

                Dim installDir As String = NormalizeInstallPath(Path.GetDirectoryName(exePath))
                If String.IsNullOrWhiteSpace(installDir) Then
                    Continue For
                End If

                If candidates.TryAdd(installDir, New DeepScanCandidate With {
                                     .DisplayName = exeMatch.Name,
                                     .InstallDir = installDir,
                                     .Platform = "Drive scan"
                                 }) Then
                    localHits += 1
                End If
            Next

            If node.Depth >= maxDepth Then
                Continue While
            End If

            Dim children As IEnumerable(Of String) = Enumerable.Empty(Of String)()
            Try
                children = Directory.EnumerateDirectories(normalized, "*", SearchOption.TopDirectoryOnly)
            Catch ex As Exception
                ErrorLogger.Log(ex, "DetectionService.ScanDriveRoot.EnumerateDirs")
            End Try

            For Each child As String In children
                Dim childName As String = Path.GetFileName(child)
                If ShouldSkipScanDirectoryName(childName) Then
                    Continue For
                End If

                Try
                    Dim attributes As FileAttributes = File.GetAttributes(child)
                    If (attributes And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then
                        Continue For
                    End If
                Catch ex As Exception
                    ErrorLogger.Log(ex, "DetectionService.ScanDriveRoot.CheckAttributes")
                End Try

                queue.Enqueue(New ScanNode With {.FolderPath = child, .Depth = node.Depth + 1})
            Next
        End While
        ReportDeepScanProgress(progress, root, root, scannedFolders, totalRoots, completedRootsProvider, True)

        If log IsNot Nothing Then
            If scannedFolders >= ScanRootFolderBudget Then
                log("Deep scan: " & root & " reached folder budget (" & ScanRootFolderBudget & ").")
            End If
            log("Deep scan: finished " & root & " (" & scannedFolders & " folders, " & localHits & " candidate match(es)).")
        End If
    End Sub

    Private Shared Sub ReportDeepScanProgress(progress As Action(Of DeepScanProgressInfo),
                                              root As String,
                                              currentDirectory As String,
                                              scannedFolders As Integer,
                                              totalRoots As Integer,
                                              completedRootsProvider As Func(Of Integer),
                                              includeCurrentRootCompletion As Boolean)
        If progress Is Nothing Then
            Return
        End If

        Dim completedRoots As Integer = 0
        If completedRootsProvider IsNot Nothing Then
            completedRoots = Math.Max(0, completedRootsProvider())
        End If
        If includeCurrentRootCompletion Then
            completedRoots += 1
        End If

        progress(New DeepScanProgressInfo With {
                 .RootPath = root,
                 .CurrentDirectory = currentDirectory,
                 .ScannedFoldersInRoot = Math.Max(0, scannedFolders),
                 .MaxFoldersPerRoot = ScanRootFolderBudget,
                 .TotalRoots = Math.Max(1, totalRoots),
                 .RootsCompleted = Math.Min(Math.Max(1, totalRoots), completedRoots)
                 })
    End Sub

    Private Shared Function ResolveMatchedFolderInstallDir(folderPath As String) As String
        If String.IsNullOrWhiteSpace(folderPath) OrElse Not Directory.Exists(folderPath) Then
            Return ""
        End If

        If HasUsableExecutable(folderPath) Then
            Return folderPath
        End If

        Dim probeFolders As String() = {
            Path.Combine(folderPath, "Binaries", "Win64"),
            Path.Combine(folderPath, "Binaries", "Win32"),
            Path.Combine(folderPath, "Binaries", "WinGDK"),
            Path.Combine(folderPath, "bin"),
            Path.Combine(folderPath, "bin", "x64"),
            Path.Combine(folderPath, "bin", "x64_dx12"),
            Path.Combine(folderPath, "bin", "x64_dx11"),
            Path.Combine(folderPath, "bin", "Win64"),
            Path.Combine(folderPath, "x64"),
            Path.Combine(folderPath, "x64_dx12"),
            Path.Combine(folderPath, "x64_dx11"),
            Path.Combine(folderPath, "Win64"),
            Path.Combine(folderPath, "Win32"),
            Path.Combine(folderPath, "WinGDK")
        }

        For Each probe As String In probeFolders
            Dim normalized As String = NormalizeInstallPath(probe)
            If String.IsNullOrWhiteSpace(normalized) OrElse Not Directory.Exists(normalized) Then
                Continue For
            End If

            If HasUsableExecutable(normalized) Then
                Return normalized
            End If
        Next

        Dim nestedMatch As String = ProbeCommonExecutableSubfolders(folderPath)
        If Not String.IsNullOrWhiteSpace(nestedMatch) Then
            Return nestedMatch
        End If

        Return ""
    End Function

    Private Shared Function ProbeCommonExecutableSubfolders(folderPath As String) As String
        Dim containers As String() = {
            folderPath,
            Path.Combine(folderPath, "bin"),
            Path.Combine(folderPath, "binaries"),
            Path.Combine(folderPath, "Binaries")
        }

        For Each container As String In containers
            Dim normalizedContainer As String = NormalizeInstallPath(container)
            If String.IsNullOrWhiteSpace(normalizedContainer) OrElse Not Directory.Exists(normalizedContainer) Then
                Continue For
            End If

            Dim children As IEnumerable(Of String) = Enumerable.Empty(Of String)()
            Try
                children = Directory.EnumerateDirectories(normalizedContainer, "*", SearchOption.TopDirectoryOnly)
            Catch ex As Exception
                ErrorLogger.Log(ex, "DetectionService.ProbeCommonExecutableSubfolders.EnumerateChildren")
                Continue For
            End Try

            For Each child As String In children
                Dim normalizedChild As String = NormalizeInstallPath(child)
                If String.IsNullOrWhiteSpace(normalizedChild) Then
                    Continue For
                End If

                If HasUsableExecutable(normalizedChild) Then
                    Return normalizedChild
                End If

                Dim grandChildren As IEnumerable(Of String) = Enumerable.Empty(Of String)()
                Try
                    grandChildren = Directory.EnumerateDirectories(normalizedChild, "*", SearchOption.TopDirectoryOnly)
                Catch ex As Exception
                    ErrorLogger.Log(ex, "DetectionService.ProbeCommonExecutableSubfolders.EnumerateGrandChildren")
                    Continue For
                End Try

                For Each grandChild As String In grandChildren
                    Dim normalizedGrandChild As String = NormalizeInstallPath(grandChild)
                    If String.IsNullOrWhiteSpace(normalizedGrandChild) Then
                        Continue For
                    End If

                    If HasUsableExecutable(normalizedGrandChild) Then
                        Return normalizedGrandChild
                    End If
                Next
            Next
        Next

        Return ""
    End Function

    Private Shared Function HasUsableExecutable(folderPath As String) As Boolean
        If String.IsNullOrWhiteSpace(folderPath) OrElse Not Directory.Exists(folderPath) Then
            Return False
        End If

        Try
            For Each exePath As String In Directory.EnumerateFiles(folderPath, "*.exe", SearchOption.TopDirectoryOnly)
                Dim exeName As String = Path.GetFileNameWithoutExtension(exePath)
                If Not ShouldSkipExecutable(exeName) Then
                    Return True
                End If
            Next
        Catch ex As Exception
            ErrorLogger.Log(ex, "DetectionService.HasUsableExecutable")
        End Try

        Return False
    End Function

    Private Shared Function ShouldSkipScanDirectory(pathValue As String, isRoot As Boolean) As Boolean
        If String.IsNullOrWhiteSpace(pathValue) Then
            Return True
        End If
        If isRoot Then
            Return False
        End If

        Dim name As String = Path.GetFileName(pathValue)
        Return ShouldSkipScanDirectoryName(name)
    End Function

    Private Shared Function ShouldSkipScanDirectoryName(folderName As String) As Boolean
        If String.IsNullOrWhiteSpace(folderName) Then
            Return False
        End If

        Dim blockedNames As String() = {
            "$recycle.bin",
            "system volume information",
            "windows",
            "programdata",
            "$winreagent",
            "$windows.~bt",
            "$windows.~ws",
            "msocache",
            "winsxs",
            "appdata",
            "temp",
            "tmp",
            "__pycache__",
            "node_modules"
        }

        Dim lowered As String = folderName.Trim().ToLowerInvariant()
        If lowered.StartsWith(".") Then
            Return True
        End If

        For Each blocked As String In blockedNames
            If lowered = blocked Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Shared Function ShouldSkipExecutable(exeName As String) As Boolean
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

    Private Shared Function NormalizeInstallPath(value As String) As String
        ' Normalize separators and resolve to a full path when possible.
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim trimmed As String = value.Trim()
        Dim normalized As String = trimmed.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        If IsDriveLetterOnlyPath(normalized) Then
            normalized &= Path.DirectorySeparatorChar
        End If

        Try
            normalized = Path.GetFullPath(normalized)
        Catch ex As Exception
            ErrorLogger.Log(ex, "DetectionService.NormalizeInstallPath")
            normalized = trimmed
        End Try

        Return TrimPathExceptRoot(normalized)
    End Function

    Private Shared Function IsDriveLetterOnlyPath(value As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(value) AndAlso
               value.Length = 2 AndAlso
               Char.IsLetter(value(0)) AndAlso
               value(1) = ":"c
    End Function

    Private Shared Function TrimPathExceptRoot(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim normalized As String = value.Trim()
        Dim root As String = ""
        Try
            root = Path.GetPathRoot(normalized)
        Catch ex As Exception
            ErrorLogger.Log(ex, "DetectionService.TrimPathExceptRoot")
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

    Private Shared Function GetSteamPath() As String
        Dim steamPath As String = TryGetRegistryValue(Registry.CurrentUser, "Software\Valve\Steam", "SteamPath")
        If String.IsNullOrWhiteSpace(steamPath) Then
            steamPath = TryGetRegistryValue(Registry.LocalMachine, "Software\Valve\Steam", "InstallPath")
        End If
        If String.IsNullOrWhiteSpace(steamPath) Then
            steamPath = TryGetRegistryValue(Registry.LocalMachine, "Software\WOW6432Node\Valve\Steam", "InstallPath")
        End If

        If String.IsNullOrWhiteSpace(steamPath) Then
            Dim defaultPath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam")
            If Directory.Exists(defaultPath) Then
                steamPath = defaultPath
            Else
                defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam")
                If Directory.Exists(defaultPath) Then
                    steamPath = defaultPath
                End If
            End If
        End If

        Return steamPath
    End Function

    Private Shared Function GetSteamLibraries(steamPath As String) As List(Of String)
        ' Parse the Steam libraryfolders.vdf file for extra library roots.
        Dim libraries As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Dim rootPath As String = steamPath.Trim()
        If Directory.Exists(rootPath) Then
            libraries.Add(rootPath)
            seen.Add(rootPath)
        End If

        Dim vdfPath As String = Path.Combine(rootPath, "steamapps", "libraryfolders.vdf")
        If Not File.Exists(vdfPath) Then
            Return libraries
        End If

        Dim content As String
        Try
            content = File.ReadAllText(vdfPath)
        Catch ex As Exception
            ErrorLogger.Log(ex, "DetectionService.GetSteamLibraries.ReadLibraryFolders")
            Return libraries
        End Try

        For Each match As Match In Regex.Matches(content, """path""\s*""(?<path>[^""]+)""", RegexOptions.IgnoreCase)
            Dim raw As String = match.Groups("path").Value
            If String.IsNullOrWhiteSpace(raw) Then
                Continue For
            End If

            Dim pathValue As String = raw.Replace("\\", "\").Trim()
            If Not Directory.Exists(pathValue) Then
                Continue For
            End If

            If Not seen.Contains(pathValue) Then
                libraries.Add(pathValue)
                seen.Add(pathValue)
            End If
        Next

        Return libraries
    End Function

    Private Shared Function TryGetVdfValue(content As String, key As String) As String
        Dim pattern As String = """" & Regex.Escape(key) & """\s*""(?<value>[^""]*)"""
        Dim match As Match = Regex.Match(content, pattern, RegexOptions.IgnoreCase)
        If match.Success Then
            Return match.Groups("value").Value
        End If
        Return Nothing
    End Function

    Private Shared Function TryGetJsonString(element As JsonElement, propertyName As String) As String
        Dim value As JsonElement
        If element.TryGetProperty(propertyName, value) AndAlso value.ValueKind = JsonValueKind.String Then
            Return value.GetString()
        End If
        Return Nothing
    End Function

    Private Shared Function TryGetRegistryValue(root As RegistryKey, subKeyPath As String, valueName As String) As String
        Try
            Using key As RegistryKey = root.OpenSubKey(subKeyPath)
                If key Is Nothing Then
                    Return Nothing
                End If
                Return Convert.ToString(key.GetValue(valueName))
            End Using
        Catch ex As Exception
            ErrorLogger.Log(ex, "DetectionService.TryGetRegistryValue")
            Return Nothing
        End Try
    End Function

    Private Class CompatibilityMatcher
        ' Matching uses exact, normalized, and relaxed string maps.
        Private ReadOnly exactMap As Dictionary(Of String, CompatibilityEntry)
        Private ReadOnly normalizedMap As Dictionary(Of String, CompatibilityEntry)
        Private ReadOnly relaxedMap As Dictionary(Of String, CompatibilityEntry)
        Private ReadOnly relaxedTokenEntries As List(Of RelaxedTokenEntry)

        Public Sub New(entries As IEnumerable(Of CompatibilityEntry))
            exactMap = New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)
            normalizedMap = New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)
            relaxedMap = New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)
            relaxedTokenEntries = New List(Of RelaxedTokenEntry)()

            For Each entry As CompatibilityEntry In entries
                If entry Is Nothing OrElse String.IsNullOrWhiteSpace(entry.Name) Then
                    Continue For
                End If

                If Not exactMap.ContainsKey(entry.Name) Then
                    exactMap(entry.Name) = entry
                End If

                Dim normalized As String = NormalizeName(entry.Name)
                If Not String.IsNullOrWhiteSpace(normalized) AndAlso Not normalizedMap.ContainsKey(normalized) Then
                    normalizedMap(normalized) = entry
                End If

                Dim relaxed As String = NormalizeRelaxedName(entry.Name)
                If Not String.IsNullOrWhiteSpace(relaxed) AndAlso Not relaxedMap.ContainsKey(relaxed) Then
                    relaxedMap(relaxed) = entry
                End If

                Dim relaxedTokens As List(Of String) = NameNormalization.TokenizeRelaxed(entry.Name)
                If relaxedTokens.Count > 0 Then
                    relaxedTokenEntries.Add(New RelaxedTokenEntry With {
                                           .Entry = entry,
                                           .Tokens = relaxedTokens
                    })
                End If
            Next
        End Sub

        Public Function Match(name As String) As CompatibilityEntry
            If String.IsNullOrWhiteSpace(name) Then
                Return Nothing
            End If

            Dim matchedEntry As CompatibilityEntry = Nothing
            If exactMap.TryGetValue(name, matchedEntry) Then
                Return matchedEntry
            End If

            Dim normalized As String = NormalizeName(name)
            If Not String.IsNullOrWhiteSpace(normalized) AndAlso normalizedMap.TryGetValue(normalized, matchedEntry) Then
                Return matchedEntry
            End If

            Dim relaxed As String = NormalizeRelaxedName(name)
            If Not String.IsNullOrWhiteSpace(relaxed) AndAlso relaxedMap.TryGetValue(relaxed, matchedEntry) Then
                Return matchedEntry
            End If

            Dim inputRelaxedTokens As List(Of String) = NameNormalization.TokenizeRelaxed(name)
            matchedEntry = MatchByRelaxedTokenPrefix(inputRelaxedTokens)
            If matchedEntry IsNot Nothing Then
                Return matchedEntry
            End If

            Return Nothing
        End Function

        Private Function MatchByRelaxedTokenPrefix(inputTokens As List(Of String)) As CompatibilityEntry
            If inputTokens Is Nothing OrElse inputTokens.Count < 2 Then
                Return Nothing
            End If

            Dim matched As CompatibilityEntry = Nothing
            For Each candidate As RelaxedTokenEntry In relaxedTokenEntries
                If candidate Is Nothing OrElse candidate.Entry Is Nothing OrElse candidate.Tokens Is Nothing Then
                    Continue For
                End If

                If IsPrefix(inputTokens, candidate.Tokens) OrElse IsPrefix(candidate.Tokens, inputTokens) Then
                    If matched Is Nothing Then
                        matched = candidate.Entry
                    ElseIf Not String.Equals(matched.Name, candidate.Entry.Name, StringComparison.OrdinalIgnoreCase) Then
                        ' Ambiguous prefix match; do not guess.
                        Return Nothing
                    End If
                End If
            Next

            Return matched
        End Function

        Private Shared Function IsPrefix(prefixTokens As List(Of String), fullTokens As List(Of String)) As Boolean
            If prefixTokens Is Nothing OrElse fullTokens Is Nothing Then
                Return False
            End If
            If prefixTokens.Count = 0 OrElse prefixTokens.Count > fullTokens.Count Then
                Return False
            End If

            For i As Integer = 0 To prefixTokens.Count - 1
                If Not String.Equals(prefixTokens(i), fullTokens(i), StringComparison.OrdinalIgnoreCase) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Private Shared Function NormalizeName(value As String) As String
            Return NameNormalization.NormalizeName(value)
        End Function

        Private Shared Function NormalizeRelaxedName(value As String) As String
            Return NameNormalization.NormalizeRelaxedName(value)
        End Function

        Private Class RelaxedTokenEntry
            Public Property Entry As CompatibilityEntry
            Public Property Tokens As List(Of String)
        End Class
    End Class

    Private Class DeepScanCandidate
        Public Property DisplayName As String
        Public Property InstallDir As String
        Public Property Platform As String
    End Class

    Private Class ScanNode
        Public Property FolderPath As String
        Public Property Depth As Integer
    End Class

    Private Const ScanRootFolderBudget As Integer = 180000
End Class
