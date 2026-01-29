Imports System.IO
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports Microsoft.Win32

Public Class DetectionService
    ' Scans known launchers and matches installs against the compatibility list.
    Public Shared Function DetectSupportedGames(entries As IEnumerable(Of CompatibilityEntry), log As Action(Of String)) As List(Of DetectedGame)
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

        results.Add(New DetectedGame With {
            .DisplayName = match.Name,
            .Platform = platform,
            .InstallDir = trimmedPath,
            .MatchedEntry = match,
            .SourceName = displayName
        })
        seenPaths.Add(trimmedPath)
    End Sub

    Private Shared Function NormalizeInstallPath(value As String) As String
        ' Normalize separators and resolve to a full path when possible.
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim trimmed As String = value.Trim()
        Dim normalized As String = trimmed.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        Try
            normalized = Path.GetFullPath(normalized)
        Catch ex As Exception
            ErrorLogger.Log(ex, "DetectionService.NormalizeInstallPath")
            normalized = trimmed
        End Try

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

        Public Sub New(entries As IEnumerable(Of CompatibilityEntry))
            exactMap = New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)
            normalizedMap = New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)
            relaxedMap = New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)

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

            Return Nothing
        End Function

        Private Shared Function NormalizeName(value As String) As String
            Return NameNormalization.NormalizeName(value)
        End Function

        Private Shared Function NormalizeRelaxedName(value As String) As String
            Return NameNormalization.NormalizeRelaxedName(value)
        End Function
    End Class
End Class
