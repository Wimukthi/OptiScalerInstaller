Imports System.IO
Imports System.Text.Json

Friend Module DeepScanDetectionCacheService
    ' Stores deep-scan detections so launcher-independent games remain detected across app restarts.
    Private ReadOnly CachePath As String = Path.Combine(AppContext.BaseDirectory, "OptiScalerInstaller.deep-scan-cache.json")

    Public Function Load(log As Action(Of String)) As List(Of DetectedGame)
        Dim results As New List(Of DetectedGame)()
        If Not File.Exists(CachePath) Then
            Return results
        End If

        Try
            Dim json As String = File.ReadAllText(CachePath)
            Dim model As DeepScanDetectionCacheModel = JsonSerializer.Deserialize(Of DeepScanDetectionCacheModel)(json)
            If model Is Nothing OrElse model.Games Is Nothing Then
                Return results
            End If

            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each entry As DeepScanDetectionCacheEntry In model.Games
                If entry Is Nothing OrElse String.IsNullOrWhiteSpace(entry.DisplayName) OrElse String.IsNullOrWhiteSpace(entry.InstallDir) Then
                    Continue For
                End If

                Dim normalizedPath As String = NormalizePath(entry.InstallDir)
                If String.IsNullOrWhiteSpace(normalizedPath) OrElse Not Directory.Exists(normalizedPath) Then
                    Continue For
                End If

                Dim key As String = BuildIdentityKey(entry.DisplayName, normalizedPath)
                If Not seen.Add(key) Then
                    Continue For
                End If

                results.Add(New DetectedGame With {
                    .DisplayName = entry.DisplayName,
                    .Platform = If(String.IsNullOrWhiteSpace(entry.Platform), "Drive scan", entry.Platform),
                    .InstallDir = normalizedPath,
                    .SourceName = If(String.IsNullOrWhiteSpace(entry.SourceName), entry.DisplayName, entry.SourceName),
                    .AntiCheat = If(entry.AntiCheat, "")
                })
            Next

            If log IsNot Nothing Then
                log("Loaded persisted deep-scan detections: " & results.Count & " game(s).")
            End If
        Catch ex As Exception
            If log IsNot Nothing Then
                log("Failed to load deep-scan cache: " & ex.Message)
            End If
            ErrorLogger.Log(ex, "DeepScanDetectionCacheService.Load")
        End Try

        Return results
    End Function

    Public Sub Save(games As IEnumerable(Of DetectedGame), log As Action(Of String))
        If games Is Nothing Then
            Return
        End If

        Try
            Dim entries As New List(Of DeepScanDetectionCacheEntry)()
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each game As DetectedGame In games
                If game Is Nothing OrElse String.IsNullOrWhiteSpace(game.DisplayName) OrElse String.IsNullOrWhiteSpace(game.InstallDir) Then
                    Continue For
                End If

                Dim normalizedPath As String = NormalizePath(game.InstallDir)
                If String.IsNullOrWhiteSpace(normalizedPath) OrElse Not Directory.Exists(normalizedPath) Then
                    Continue For
                End If

                Dim key As String = BuildIdentityKey(game.DisplayName, normalizedPath)
                If Not seen.Add(key) Then
                    Continue For
                End If

                entries.Add(New DeepScanDetectionCacheEntry With {
                    .DisplayName = game.DisplayName,
                    .Platform = If(String.IsNullOrWhiteSpace(game.Platform), "Drive scan", game.Platform),
                    .InstallDir = normalizedPath,
                    .SourceName = If(String.IsNullOrWhiteSpace(game.SourceName), game.DisplayName, game.SourceName),
                    .AntiCheat = If(game.AntiCheat, "")
                })
            Next

            Dim model As New DeepScanDetectionCacheModel With {
                .SavedAtUtc = DateTime.UtcNow,
                .Games = entries
            }

            Dim json As String = JsonSerializer.Serialize(model, New JsonSerializerOptions With {.WriteIndented = True})
            File.WriteAllText(CachePath, json)

            If log IsNot Nothing Then
                log("Persisted deep-scan detections: " & entries.Count & " game(s).")
            End If
        Catch ex As Exception
            If log IsNot Nothing Then
                log("Failed to save deep-scan cache: " & ex.Message)
            End If
            ErrorLogger.Log(ex, "DeepScanDetectionCacheService.Save")
        End Try
    End Sub

    Private Function NormalizePath(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim normalized As String = value.Trim().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
        Try
            normalized = Path.GetFullPath(normalized)
        Catch ex As Exception
            ErrorLogger.Log(ex, "DeepScanDetectionCacheService.NormalizePath")
        End Try

        Return normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
    End Function

    Private Function BuildIdentityKey(displayName As String, installDir As String) As String
        Dim nameKey As String = NameNormalization.NormalizeRelaxedName(displayName)
        Dim pathKey As String = NormalizePath(installDir)
        Return nameKey & "|" & pathKey
    End Function

    Private Class DeepScanDetectionCacheModel
        Public Property SavedAtUtc As DateTime?
        Public Property Games As List(Of DeepScanDetectionCacheEntry)
    End Class

    Private Class DeepScanDetectionCacheEntry
        Public Property DisplayName As String
        Public Property Platform As String
        Public Property InstallDir As String
        Public Property SourceName As String
        Public Property AntiCheat As String
    End Class
End Module
