Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Text.RegularExpressions

Public Class CompatibilityService
    ' Loads, caches, and parses the OptiScaler compatibility list.
    Private Shared ReadOnly DefaultListPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Compatibility-List.md")
    Private Shared ReadOnly CachePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OptiScalerInstaller", "compatibility.json")
    Private Const MinimumCompatibilityEntries As Integer = 20
    Private Const ValidationMinimumTolerance As Integer = 3
    Private Const ValidationToleranceRatio As Double = 0.01

    Public Shared Function LoadCompatibilityList() As List(Of CompatibilityEntry)
        Dim cached As List(Of CompatibilityEntry) = TryLoadCache()
        If cached IsNot Nothing AndAlso cached.Count > 0 Then
            Return cached
        End If

        If File.Exists(DefaultListPath) Then
            Dim content As String = File.ReadAllText(DefaultListPath)
            Return ParseCompatibilityList(content)
        End If

        Return New List(Of CompatibilityEntry)()
    End Function

    Public Shared Async Function UpdateCompatibilityListAsync() As Task(Of List(Of CompatibilityEntry))
        Dim result As CompatibilityUpdateResult = Await UpdateCompatibilityListWithDiffAsync()
        If result Is Nothing OrElse result.Entries Is Nothing Then
            Return New List(Of CompatibilityEntry)()
        End If

        Return result.Entries
    End Function

    Public Shared Async Function UpdateCompatibilityListWithDiffAsync() As Task(Of CompatibilityUpdateResult)
        Dim listUrl As String = GetCompatibilityListUrl()
        If String.IsNullOrWhiteSpace(listUrl) Then
            Throw New InvalidOperationException("Compatibility list URL is not set. Update it in Settings.")
        End If

        Dim previousEntries As List(Of CompatibilityEntry) = LoadCompatibilityList()
        Dim content As String = Await HttpClientHelper.GetStringWithRetryAsync(listUrl)
        Dim parseResult As CompatibilityParseResult = ParseCompatibilityListWithDiagnostics(content)
        Dim entries As List(Of CompatibilityEntry) = parseResult.Entries

        Dim validationFailure As String = GetValidationFailure(previousEntries.Count, parseResult)
        If Not String.IsNullOrWhiteSpace(validationFailure) Then
            ErrorLogger.LogMessage(
                validationFailure & " Cache preserved.",
                "", "CompatibilityService.UpdateValidation")
            Return BuildUpdateResult(previousEntries, previousEntries, parseResult)
        End If

        SaveCache(entries)
        Return BuildUpdateResult(previousEntries, entries, parseResult)
    End Function

    Private Shared Function GetCompatibilityListUrl() As String
        Dim settings As AppSettingsModel = AppSettings.Load()
        Return settings.CompatibilityListUrl
    End Function

    Private Shared Function TryLoadCache() As List(Of CompatibilityEntry)
        If Not File.Exists(CachePath) Then
            Return Nothing
        End If

        Try
            Dim json As String = File.ReadAllText(CachePath)
            Dim entries As List(Of CompatibilityEntry) = JsonSerializer.Deserialize(Of List(Of CompatibilityEntry))(json)
            Return entries
        Catch ex As Exception
            ErrorLogger.Log(ex, "CompatibilityService.TryLoadCache")
            Return Nothing
        End Try
    End Function

    Private Shared Sub SaveCache(entries As List(Of CompatibilityEntry))
        Dim dir As String = Path.GetDirectoryName(CachePath)
        If Not Directory.Exists(dir) Then
            Directory.CreateDirectory(dir)
        End If

        Dim json As String = JsonSerializer.Serialize(entries, New JsonSerializerOptions With {
            .WriteIndented = True,
            .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        })
        File.WriteAllText(CachePath, json)
    End Sub

    Public Shared Function ParseCompatibilityList(content As String) As List(Of CompatibilityEntry)
        Dim result As CompatibilityParseResult = ParseCompatibilityListWithDiagnostics(content)
        Return result.Entries
    End Function

    Public Shared Function ParseCompatibilityListWithDiagnostics(content As String) As CompatibilityParseResult
        Dim jsonResult As CompatibilityParseResult = Nothing
        If TryParseJsonCompatibilityList(content, jsonResult) Then
            ValidateParsedSource(jsonResult)
            Return jsonResult
        End If

        Dim markdownResult As CompatibilityParseResult = ParseMarkdownCompatibilityList(content)
        ValidateParsedSource(markdownResult)
        Return markdownResult
    End Function

    Private Shared Function ParseMarkdownCompatibilityList(content As String) As CompatibilityParseResult
        ' Handles both table format (remote wiki) and standalone link format (bundled file).
        ' Only extracts game names from the name column/position to avoid cross-references.
        Dim result As New CompatibilityParseResult With {
            .SourceFormat = "markdown",
            .ExpectedCount = TryGetExpectedMarkdownGameCount(content)
        }
        Dim inHtmlComment As Boolean = False
        Dim inCompatibilityTable As Boolean = False

        For Each rawLine As String In content.Split({ControlChars.CrLf, ControlChars.Lf}, StringSplitOptions.None)
            Dim line As String = StripHtmlCommentText(rawLine, inHtmlComment).Trim()
            If String.IsNullOrWhiteSpace(line) Then
                Continue For
            End If

            If IsCompatibilityTableHeader(line) Then
                inCompatibilityTable = True
                Continue For
            End If

            ' Determine which part of the line holds the game name.
            Dim nameCell As String = ""
            Dim isTableRow As Boolean = False

            If line.StartsWith("|", StringComparison.Ordinal) OrElse (inCompatibilityTable AndAlso LooksLikeTableRow(line)) Then
                ' Table row: only search for a game entry in the first data column.
                If Not TryGetFirstTableCell(line, nameCell) Then
                    Continue For
                End If

                If IsTableHeaderOrSeparatorCell(nameCell) Then
                    Continue For
                End If

                isTableRow = True
            ElseIf line.StartsWith("[", StringComparison.Ordinal) Then
                ' Standalone markdown link (bundled file format).
                nameCell = line
            Else
                ' Skip prose, headings, blockquotes, and other non-entry lines.
                Continue For
            End If

            If String.IsNullOrWhiteSpace(nameCell) Then
                Continue For
            End If

            Dim entry As CompatibilityEntry = Nothing
            If TryParseCompatibilityEntry(nameCell, isTableRow, entry) Then
                result.Entries.Add(entry)
            Else
                result.SkippedRows += 1
            End If
        Next

        Return result
    End Function

    Private Shared Function TryParseJsonCompatibilityList(content As String, ByRef result As CompatibilityParseResult) As Boolean
        result = Nothing

        Dim trimmed As String = If(content, "").TrimStart()
        If trimmed.Length = 0 OrElse (trimmed(0) <> "{"c AndAlso trimmed(0) <> "["c) Then
            Return False
        End If

        Try
            Using document As JsonDocument = JsonDocument.Parse(content)
                Dim root As JsonElement = document.RootElement
                Dim entriesElement As JsonElement = Nothing
                Dim expectedCount As Integer? = Nothing

                If root.ValueKind = JsonValueKind.Array Then
                    entriesElement = root
                    expectedCount = root.GetArrayLength()
                ElseIf root.ValueKind = JsonValueKind.Object Then
                    expectedCount = TryGetJsonInteger(root, "expectedCount", "count", "total")
                    If Not TryGetJsonArray(root, entriesElement, "entries", "games", "items", "compatibility") Then
                        Return False
                    End If
                Else
                    Return False
                End If

                result = New CompatibilityParseResult With {
                    .SourceFormat = "json",
                    .ExpectedCount = expectedCount
                }

                For Each item As JsonElement In entriesElement.EnumerateArray()
                    Dim entry As CompatibilityEntry = ParseJsonCompatibilityEntry(item)
                    If entry Is Nothing Then
                        result.SkippedRows += 1
                    Else
                        result.Entries.Add(entry)
                    End If
                Next
            End Using

            Return result IsNot Nothing
        Catch ex As JsonException
            Return False
        Catch ex As NotSupportedException
            Return False
        End Try
    End Function

    Private Shared Function ParseJsonCompatibilityEntry(item As JsonElement) As CompatibilityEntry
        Dim name As String = ""
        Dim slug As String = ""
        Dim aliases As New List(Of String)()

        If item.ValueKind = JsonValueKind.String Then
            name = CleanGameName(item.GetString())
        ElseIf item.ValueKind = JsonValueKind.Object Then
            name = CleanGameName(TryGetJsonString(item, "name", "title", "game"))
            slug = NormalizeCompatibilitySlug(TryGetJsonString(item, "slug", "wikiSlug", "wiki", "wikiUrl", "url"))
            aliases = TryGetJsonStringList(item, "aliases", "alias", "alternateNames", "alternateTitles")
        End If

        If ShouldSkipName(name) Then
            Return Nothing
        End If

        If ShouldSkipLinkTarget(slug) Then
            slug = ""
        End If

        Return New CompatibilityEntry With {
            .Name = name,
            .Slug = slug,
            .Aliases = If(aliases.Count = 0, Nothing, aliases)
        }
    End Function

    Private Shared Function TryGetJsonArray(element As JsonElement,
                                            ByRef value As JsonElement,
                                            ParamArray propertyNames As String()) As Boolean
        If element.ValueKind <> JsonValueKind.Object Then
            Return False
        End If

        For Each prop As JsonProperty In element.EnumerateObject()
            For Each propertyName As String In propertyNames
                If String.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase) AndAlso prop.Value.ValueKind = JsonValueKind.Array Then
                    value = prop.Value
                    Return True
                End If
            Next
        Next

        Return False
    End Function

    Private Shared Function TryGetJsonString(element As JsonElement, ParamArray propertyNames As String()) As String
        If element.ValueKind <> JsonValueKind.Object Then
            Return ""
        End If

        For Each prop As JsonProperty In element.EnumerateObject()
            For Each propertyName As String In propertyNames
                If Not String.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                Select Case prop.Value.ValueKind
                    Case JsonValueKind.String
                        Return If(prop.Value.GetString(), "").Trim()
                    Case JsonValueKind.Number, JsonValueKind.True, JsonValueKind.False
                        Return prop.Value.ToString().Trim()
                End Select
            Next
        Next

        Return ""
    End Function

    Private Shared Function TryGetJsonStringList(element As JsonElement, ParamArray propertyNames As String()) As List(Of String)
        Dim values As New List(Of String)()
        If element.ValueKind <> JsonValueKind.Object Then
            Return values
        End If

        For Each prop As JsonProperty In element.EnumerateObject()
            For Each propertyName As String In propertyNames
                If Not String.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                If prop.Value.ValueKind = JsonValueKind.Array Then
                    For Each item As JsonElement In prop.Value.EnumerateArray()
                        Dim text As String = ""
                        If item.ValueKind = JsonValueKind.String Then
                            text = CleanGameName(item.GetString())
                        ElseIf item.ValueKind = JsonValueKind.Number Then
                            text = CleanGameName(item.ToString())
                        End If

                        If Not ShouldSkipName(text) AndAlso Not ContainsString(values, text) Then
                            values.Add(text)
                        End If
                    Next
                ElseIf prop.Value.ValueKind = JsonValueKind.String Then
                    Dim text As String = CleanGameName(prop.Value.GetString())
                    If Not ShouldSkipName(text) AndAlso Not ContainsString(values, text) Then
                        values.Add(text)
                    End If
                End If
            Next
        Next

        Return values
    End Function

    Private Shared Function TryGetJsonInteger(element As JsonElement, ParamArray propertyNames As String()) As Integer?
        If element.ValueKind <> JsonValueKind.Object Then
            Return Nothing
        End If

        For Each prop As JsonProperty In element.EnumerateObject()
            For Each propertyName As String In propertyNames
                If Not String.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                If prop.Value.ValueKind = JsonValueKind.Number Then
                    Dim count As Integer = 0
                    If prop.Value.TryGetInt32(count) Then
                        Return count
                    End If
                ElseIf prop.Value.ValueKind = JsonValueKind.String Then
                    Dim parsed As Integer = 0
                    If Integer.TryParse(prop.Value.GetString(), parsed) Then
                        Return parsed
                    End If
                End If
            Next
        Next

        Return Nothing
    End Function

    Private Shared Function TryParseCompatibilityEntry(nameCell As String,
                                                       isTableRow As Boolean,
                                                       ByRef entry As CompatibilityEntry) As Boolean
        entry = Nothing

        If String.IsNullOrWhiteSpace(nameCell) Then
            Return False
        End If

        Dim name As String = ""
        Dim slug As String = ""

        Dim link As New MarkdownLink()
        If MarkdownLinkParser.TryGetFirstLink(nameCell, link) Then
            name = CleanGameName(link.Text)
            slug = NormalizeCompatibilitySlug(link.Target)

            If ShouldSkipName(name) Then
                Return False
            End If

            If ShouldSkipLinkTarget(slug) Then
                If Not isTableRow Then
                    Return False
                End If

                slug = ""
            End If
        ElseIf isTableRow Then
            name = CleanGameName(nameCell)
            slug = ""
        Else
            Return False
        End If

        If ShouldSkipName(name) Then
            Return False
        End If

        entry = New CompatibilityEntry With {.Name = name, .Slug = slug}
        Return True
    End Function

    Private Shared Function IsCompatibilityTableHeader(line As String) As Boolean
        Dim firstCell As String = ""
        If Not TryGetFirstTableCell(line, firstCell) Then
            Return False
        End If

        Return firstCell.Equals("Game", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function LooksLikeTableRow(line As String) As Boolean
        If String.IsNullOrWhiteSpace(line) Then
            Return False
        End If

        Return line.IndexOf("|"c) >= 0
    End Function

    Private Shared Function TryGetFirstTableCell(line As String, ByRef cell As String) As Boolean
        cell = ""

        If String.IsNullOrWhiteSpace(line) Then
            Return False
        End If

        Dim startIndex As Integer = If(line.StartsWith("|", StringComparison.Ordinal), 1, 0)
        If startIndex >= line.Length Then
            Return False
        End If

        Dim builder As New StringBuilder()
        Dim inCodeSpan As Boolean = False
        Dim index As Integer = startIndex

        While index < line.Length
            Dim current As Char = line(index)

            If current = "`"c AndAlso Not IsEscaped(line, index) Then
                inCodeSpan = Not inCodeSpan
            End If

            If current = "|"c AndAlso Not inCodeSpan AndAlso Not IsEscaped(line, index) Then
                Exit While
            End If

            builder.Append(current)
            index += 1
        End While

        cell = builder.ToString().Trim()
        Return Not String.IsNullOrWhiteSpace(cell)
    End Function

    Private Shared Function CleanGameName(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return ""
        End If

        Dim text As String = MarkdownLinkParser.ReplaceInlineLinks(value)
        text = Regex.Replace(text, "<[^>]+>", "")
        text = MarkdownLinkParser.UnescapeText(text).Trim()

        While text.Length >= 2 AndAlso IsWrappingMarkdownMarker(text(0)) AndAlso text(text.Length - 1) = text(0)
            text = text.Substring(1, text.Length - 2).Trim()
        End While

        text = Regex.Replace(text, "\s+", " ").Trim()
        Return text
    End Function

    Private Shared Function NormalizeCompatibilitySlug(value As String) As String
        Dim slug As String = MarkdownLinkParser.UnescapeText(If(value, "").Trim())
        If String.IsNullOrWhiteSpace(slug) Then
            Return ""
        End If

        If slug.Length >= 2 AndAlso slug.StartsWith("<", StringComparison.Ordinal) AndAlso slug.EndsWith(">", StringComparison.Ordinal) Then
            slug = slug.Substring(1, slug.Length - 2).Trim()
        End If

        If slug.StartsWith("http", StringComparison.OrdinalIgnoreCase) Then
            Dim uri As Uri = Nothing
            If Not Uri.TryCreate(slug, UriKind.Absolute, uri) Then
                Return ""
            End If

            Dim segments As String() = uri.AbsolutePath.Trim("/"c).Split({"/"c}, StringSplitOptions.RemoveEmptyEntries)
            Dim wikiIndex As Integer = Array.FindIndex(segments, Function(segment) String.Equals(segment, "wiki", StringComparison.OrdinalIgnoreCase))
            If wikiIndex >= 0 AndAlso wikiIndex + 1 < segments.Length Then
                slug = JoinSegments(segments, wikiIndex + 1)
            ElseIf String.Equals(uri.Host, "raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase) AndAlso segments.Length >= 4 AndAlso String.Equals(segments(0), "wiki", StringComparison.OrdinalIgnoreCase) Then
                slug = JoinSegments(segments, 3)
            Else
                Return ""
            End If

            slug = WebUtility.UrlDecode(slug)
        End If

        slug = slug.Trim().TrimStart("/"c)
        If slug.EndsWith(".md", StringComparison.OrdinalIgnoreCase) Then
            slug = slug.Substring(0, slug.Length - 3)
        End If

        Return slug
    End Function

    Private Shared Function JoinSegments(segments As String(), startIndex As Integer) As String
        If segments Is Nothing OrElse startIndex < 0 OrElse startIndex >= segments.Length Then
            Return ""
        End If

        Dim selected As New List(Of String)()
        For index As Integer = startIndex To segments.Length - 1
            selected.Add(segments(index))
        Next

        Return String.Join("/", selected)
    End Function

    Private Shared Function IsWrappingMarkdownMarker(value As Char) As Boolean
        Return value = "*"c OrElse value = "_"c OrElse value = "`"c
    End Function

    Private Shared Function ShouldSkipName(name As String) As Boolean
        If String.IsNullOrWhiteSpace(name) Then
            Return True
        End If

        ' Skip names that are too short to be real game titles.
        If name.Length <= 2 Then
            Return True
        End If

        Return name.Equals("Template", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function ShouldSkipLinkTarget(slug As String) As Boolean
        If String.IsNullOrWhiteSpace(slug) Then
            Return True
        End If

        If slug.StartsWith("http", StringComparison.OrdinalIgnoreCase) Then
            Return True
        End If

        ' Slugs containing '#' are anchor or section references, not game pages.
        If slug.Contains("#"c) Then
            Return True
        End If

        Return slug.Equals("CL-Template", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function IsEscaped(value As String, index As Integer) As Boolean
        Dim slashCount As Integer = 0
        Dim scanIndex As Integer = index - 1

        While scanIndex >= 0 AndAlso value(scanIndex) = "\"c
            slashCount += 1
            scanIndex -= 1
        End While

        Return slashCount Mod 2 = 1
    End Function

    Private Shared Function StripHtmlCommentText(line As String, ByRef inHtmlComment As Boolean) As String
        If String.IsNullOrEmpty(line) Then
            Return ""
        End If

        Dim remaining As String = line
        Dim visible As New StringBuilder()

        While remaining.Length > 0
            If inHtmlComment Then
                Dim commentEnd As Integer = remaining.IndexOf("-->", StringComparison.Ordinal)
                If commentEnd < 0 Then
                    Return visible.ToString()
                End If

                remaining = remaining.Substring(commentEnd + 3)
                inHtmlComment = False
                Continue While
            End If

            Dim commentStart As Integer = remaining.IndexOf("<!--", StringComparison.Ordinal)
            If commentStart < 0 Then
                visible.Append(remaining)
                Exit While
            End If

            If commentStart > 0 Then
                visible.Append(remaining.Substring(0, commentStart))
            End If

            remaining = remaining.Substring(commentStart + 4)
            inHtmlComment = True
        End While

        Return visible.ToString()
    End Function

    Private Shared Function TryGetExpectedMarkdownGameCount(content As String) As Integer?
        If String.IsNullOrWhiteSpace(content) Then
            Return Nothing
        End If

        Dim scanContent As String = content
        Dim tableHeader As Match = Regex.Match(content, "^\s*\|\s*Game\s*\|", RegexOptions.IgnoreCase Or RegexOptions.Multiline)
        If tableHeader.Success Then
            scanContent = content.Substring(0, tableHeader.Index)
        End If

        Dim total As Integer = 0
        For Each rawLine As String In scanContent.Split({ControlChars.CrLf, ControlChars.Lf}, StringSplitOptions.None)
            Dim line As String = rawLine.Trim()
            If line.IndexOf("OptiPatcher", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Continue For
            End If
            If line.IndexOf("Working", StringComparison.OrdinalIgnoreCase) < 0 Then
                Continue For
            End If

            Dim countMatch As Match = Regex.Match(line, "[\u2012\u2013\u2014\u2015-]\s*\*\*(?<count>\d+)\*\*")
            If Not countMatch.Success Then
                Continue For
            End If

            Dim count As Integer = 0
            If Integer.TryParse(countMatch.Groups("count").Value, count) Then
                total += count
            End If
        Next

        If total <= 0 Then
            Return Nothing
        End If

        Return total
    End Function

    Private Shared Sub ValidateParsedSource(result As CompatibilityParseResult)
        If result Is Nothing Then
            Return
        End If

        If result.Warnings Is Nothing Then
            result.Warnings = New List(Of String)()
        End If

        If result.SkippedRows > 0 Then
            result.Warnings.Add($"Skipped {result.SkippedRows} malformed compatibility row(s).")
        End If

        If result.ExpectedCount.HasValue Then
            Dim expected As Integer = result.ExpectedCount.Value
            Dim actual As Integer = If(result.Entries, New List(Of CompatibilityEntry)()).Count
            If expected <> actual Then
                result.Warnings.Add($"Parsed {actual} compatibility entr{If(actual = 1, "y", "ies")}; source advertises {expected}.")
            End If
        End If

        Dim duplicateNames As New List(Of String)()
        Dim seen As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each entry As CompatibilityEntry In If(result.Entries, New List(Of CompatibilityEntry)())
            If entry Is Nothing OrElse String.IsNullOrWhiteSpace(entry.Name) Then
                Continue For
            End If

            Dim key As String = NameNormalization.NormalizeRelaxedName(entry.Name)
            If String.IsNullOrWhiteSpace(key) Then
                Continue For
            End If

            If seen.ContainsKey(key) Then
                If Not ContainsString(duplicateNames, entry.Name) Then
                    duplicateNames.Add(entry.Name)
                End If
            Else
                seen(key) = entry.Name
            End If
        Next

        If duplicateNames.Count > 0 Then
            result.Warnings.Add("Duplicate normalized compatibility names: " & BuildPreview(duplicateNames, 8) & ".")
        End If
    End Sub

    Private Shared Function GetValidationFailure(previousCount As Integer, result As CompatibilityParseResult) As String
        If result Is Nothing OrElse result.Entries Is Nothing Then
            Return "Compatibility parser returned no result."
        End If

        Dim parsedCount As Integer = result.Entries.Count
        If parsedCount < MinimumCompatibilityEntries Then
            Return $"Parsed only {parsedCount} compatibility entries; expected at least {MinimumCompatibilityEntries}."
        End If

        If result.ExpectedCount.HasValue Then
            Dim expected As Integer = result.ExpectedCount.Value
            Dim tolerance As Integer = GetExpectedCountTolerance(expected)
            Dim delta As Integer = Math.Abs(parsedCount - expected)
            If delta > tolerance Then
                Return $"Parsed {parsedCount} compatibility entries but source advertises {expected} (allowed drift: {tolerance})."
            End If
        End If

        If previousCount >= MinimumCompatibilityEntries AndAlso parsedCount < previousCount \ 2 Then
            Return $"Parsed {parsedCount} compatibility entries but expected at least {previousCount \ 2} based on previous cache ({previousCount})."
        End If

        Return ""
    End Function

    Private Shared Function GetExpectedCountTolerance(expectedCount As Integer) As Integer
        Return Math.Max(ValidationMinimumTolerance, CInt(Math.Ceiling(expectedCount * ValidationToleranceRatio)))
    End Function

    Private Shared Function ContainsString(values As IEnumerable(Of String), value As String) As Boolean
        If values Is Nothing Then
            Return False
        End If

        For Each item As String In values
            If String.Equals(item, value, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next

        Return False
    End Function

    Private Shared Function BuildPreview(values As IEnumerable(Of String), maxItems As Integer) As String
        If values Is Nothing Then
            Return ""
        End If

        Dim selected As New List(Of String)()
        For Each value As String In values
            If selected.Count >= maxItems Then
                Exit For
            End If
            selected.Add(value)
        Next

        If selected.Count = 0 Then
            Return ""
        End If

        Return String.Join(", ", selected)
    End Function

    Private Shared Function BuildUpdateResult(previousEntries As List(Of CompatibilityEntry),
                                              newEntries As List(Of CompatibilityEntry),
                                              Optional parseResult As CompatibilityParseResult = Nothing) As CompatibilityUpdateResult
        If previousEntries Is Nothing Then
            previousEntries = New List(Of CompatibilityEntry)()
        End If
        If newEntries Is Nothing Then
            newEntries = New List(Of CompatibilityEntry)()
        End If

        Dim previousByName As New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)
        For Each entry As CompatibilityEntry In previousEntries
            If entry Is Nothing OrElse String.IsNullOrWhiteSpace(entry.Name) Then
                Continue For
            End If

            If Not previousByName.ContainsKey(entry.Name) Then
                previousByName(entry.Name) = entry
            End If
        Next

        Dim newByName As New Dictionary(Of String, CompatibilityEntry)(StringComparer.OrdinalIgnoreCase)
        For Each entry As CompatibilityEntry In newEntries
            If entry Is Nothing OrElse String.IsNullOrWhiteSpace(entry.Name) Then
                Continue For
            End If

            If Not newByName.ContainsKey(entry.Name) Then
                newByName(entry.Name) = entry
            End If
        Next

        Dim added As New List(Of String)()
        Dim removed As New List(Of String)()
        Dim changed As New List(Of String)()

        For Each kvp As KeyValuePair(Of String, CompatibilityEntry) In newByName
            If Not previousByName.ContainsKey(kvp.Key) Then
                added.Add(kvp.Key)
                Continue For
            End If

            Dim oldEntry As CompatibilityEntry = previousByName(kvp.Key)
            If oldEntry Is Nothing Then
                Continue For
            End If

            If Not String.Equals(oldEntry.Slug, kvp.Value.Slug, StringComparison.OrdinalIgnoreCase) Then
                changed.Add(kvp.Key)
            End If
        Next

        For Each kvp As KeyValuePair(Of String, CompatibilityEntry) In previousByName
            If Not newByName.ContainsKey(kvp.Key) Then
                removed.Add(kvp.Key)
            End If
        Next

        added.Sort(StringComparer.OrdinalIgnoreCase)
        removed.Sort(StringComparer.OrdinalIgnoreCase)
        changed.Sort(StringComparer.OrdinalIgnoreCase)

        Dim result As New CompatibilityUpdateResult With {
            .Entries = newEntries,
            .AddedNames = added,
            .RemovedNames = removed,
            .ChangedNames = changed,
            .Warnings = New List(Of String)()
        }

        If parseResult IsNot Nothing Then
            result.SourceFormat = parseResult.SourceFormat
            result.ExpectedCount = parseResult.ExpectedCount
            If parseResult.Warnings IsNot Nothing Then
                result.Warnings.AddRange(parseResult.Warnings)
            End If
        End If

        Return result
    End Function

    Private Shared Function IsTableHeaderOrSeparatorCell(nameCell As String) As Boolean
        Dim normalized As String = If(nameCell, "").Trim()
        If String.IsNullOrWhiteSpace(normalized) Then
            Return True
        End If

        If normalized.Equals("Game", StringComparison.OrdinalIgnoreCase) OrElse
           normalized.Equals("Game Name", StringComparison.OrdinalIgnoreCase) Then
            Return True
        End If

        normalized = normalized.Trim(":"c, "-"c, " "c)
        Return String.IsNullOrWhiteSpace(normalized)
    End Function

End Class
