Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Linq

Public Enum IniValidationSeverity
    None = 0
    Warning = 1
    ErrorLevel = 2
End Enum

Public Class IniSettingDefinition
    Public Property Section As String
    Public Property Key As String
    Public Property Label As String
    Public Property Description As String
    Public Property AllowedValues As String
    Public Property DefaultValue As String
    Public Property Source As String
End Class

Public Class IniValidationResult
    Public Property Severity As IniValidationSeverity
    Public Property Message As String
End Class

Public Module OptiScalerIniEditorService
    Private Const ReferenceIniFileName As String = "OptiScaler-Reference.ini"
    Private Const ConfigMarkdownFileName As String = "OptiScaler-Config.md"
    Private Const SpoofingMarkdownFileName As String = "OptiScaler-Spoofing.md"

    Private ReadOnly BooleanTokens As HashSet(Of String) = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "true", "false", "1", "0", "yes", "no", "on", "off", "auto"
    }

    Private ReadOnly _cacheLock As New Object()
    Private _knownSettingsCache As List(Of IniSettingDefinition)
    Private _knownLookupCache As Dictionary(Of String, IniSettingDefinition)

    Public Function GetKnownSettings() As List(Of IniSettingDefinition)
        EnsureSettingsCache()
        Return _knownSettingsCache.
            Select(Function(item) CloneDefinition(item)).
            ToList()
    End Function

    Public Function TryGetSettingDefinition(section As String, key As String, ByRef definition As IniSettingDefinition) As Boolean
        definition = Nothing
        EnsureSettingsCache()

        Dim mapKey As String = BuildMapKey(section, key)
        Dim found As IniSettingDefinition = Nothing
        If _knownLookupCache.TryGetValue(mapKey, found) Then
            definition = CloneDefinition(found)
            Return True
        End If

        Return False
    End Function

    Public Function BuildDefaultIniContent() As String
        Dim referencePath As String = ResolveDataFilePath(ReferenceIniFileName)
        If Not String.IsNullOrWhiteSpace(referencePath) AndAlso File.Exists(referencePath) Then
            Return File.ReadAllText(referencePath)
        End If

        Dim builder As New StringBuilder()
        builder.AppendLine("; OptiScaler.ini created by OptiScaler Installer editor")
        builder.AppendLine("; Full reference template file was not found; basic defaults are used.")
        builder.AppendLine()
        builder.AppendLine("[Spoofing]")
        builder.AppendLine("Dxgi=auto")
        builder.AppendLine()
        builder.AppendLine("[FrameGen]")
        builder.AppendLine("FGInput=auto")
        builder.AppendLine("FGOutput=auto")
        builder.AppendLine()
        builder.AppendLine("[Plugins]")
        builder.AppendLine("LoadAsiPlugins=false")
        builder.AppendLine("Path=plugins")
        builder.AppendLine("LoadReShade=false")
        builder.AppendLine("LoadSpecialK=false")
        builder.AppendLine()
        builder.AppendLine("[Main]")
        builder.AppendLine("Fsr4Update=false")
        builder.AppendLine("FsrAgilitySDKUpgrade=false")
        Return builder.ToString()
    End Function

    Public Function Validate(section As String, key As String, value As String) As IniValidationResult
        Dim result As New IniValidationResult With {
            .Severity = IniValidationSeverity.None,
            .Message = ""
        }

        Dim normalizedKey As String = If(key, "").Trim()
        Dim normalizedValue As String = If(value, "").Trim()
        If String.IsNullOrWhiteSpace(normalizedKey) Then
            result.Severity = IniValidationSeverity.ErrorLevel
            result.Message = "Key name is empty."
            Return result
        End If

        Dim definition As IniSettingDefinition = Nothing
        Dim hasDefinition As Boolean = TryGetSettingDefinition(section, key, definition)

        If hasDefinition AndAlso IsLikelyBooleanDefinition(definition) Then
            If Not BooleanTokens.Contains(normalizedValue) Then
                result.Severity = IniValidationSeverity.Warning
                result.Message = "Expected boolean-like value (true/false/auto/1/0)."
                Return result
            End If
        End If

        If hasDefinition Then
            Dim rangeValidation As IniValidationResult = ValidateNumericRange(definition, normalizedValue)
            If rangeValidation IsNot Nothing Then
                Return rangeValidation
            End If
        End If

        Return result
    End Function

    Public Function CreateTimestampBackup(filePath As String) As String
        If String.IsNullOrWhiteSpace(filePath) OrElse Not File.Exists(filePath) Then
            Return ""
        End If

        Dim folder As String = Path.GetDirectoryName(filePath)
        Dim fileName As String = Path.GetFileName(filePath)
        Dim backupName As String = $"{fileName}.editorbak_{DateTime.Now:yyyyMMddHHmmss}"
        Dim backupPath As String = Path.Combine(folder, backupName)
        File.Copy(filePath, backupPath, True)
        Return backupPath
    End Function

    Public Sub SaveTextAtomically(filePath As String, content As String)
        Dim directoryPath As String = Path.GetDirectoryName(filePath)
        If String.IsNullOrWhiteSpace(directoryPath) Then
            Throw New InvalidOperationException("Invalid target path.")
        End If
        Directory.CreateDirectory(directoryPath)

        Dim tempPath As String = filePath & ".tmp"
        Dim replaceBackupPath As String = filePath & ".replacebak"
        File.WriteAllText(tempPath, If(content, ""), New UTF8Encoding(False))

        Try
            If File.Exists(filePath) Then
                If File.Exists(replaceBackupPath) Then
                    File.Delete(replaceBackupPath)
                End If
                File.Replace(tempPath, filePath, replaceBackupPath, True)
                If File.Exists(replaceBackupPath) Then
                    File.Delete(replaceBackupPath)
                End If
            Else
                File.Move(tempPath, filePath)
            End If
        Finally
            If File.Exists(tempPath) Then
                File.Delete(tempPath)
            End If
        End Try
    End Sub

    Private Sub EnsureSettingsCache()
        If _knownSettingsCache IsNot Nothing AndAlso _knownLookupCache IsNot Nothing Then
            Return
        End If

        SyncLock _cacheLock
            If _knownSettingsCache IsNot Nothing AndAlso _knownLookupCache IsNot Nothing Then
                Return
            End If

            Dim settings As List(Of IniSettingDefinition) = LoadKnownSettingsFromSources()
            _knownSettingsCache = settings.
                OrderBy(Function(item) item.Section, StringComparer.OrdinalIgnoreCase).
                ThenBy(Function(item) item.Key, StringComparer.OrdinalIgnoreCase).
                ToList()

            _knownLookupCache = _knownSettingsCache.
                GroupBy(Function(item) BuildMapKey(item.Section, item.Key), StringComparer.OrdinalIgnoreCase).
                ToDictionary(Function(group) group.Key,
                             Function(group) group.First(),
                             StringComparer.OrdinalIgnoreCase)
        End SyncLock
    End Sub

    Private Function LoadKnownSettingsFromSources() As List(Of IniSettingDefinition)
        Dim settings As List(Of IniSettingDefinition) = ParseDefinitionsFromReferenceIni()
        If settings.Count = 0 Then
            settings = BuildFallbackDefinitions()
        End If

        MergeMarkdownDescriptions(settings, ConfigMarkdownFileName, "OptiScaler-Config.md")
        MergeMarkdownDescriptions(settings, SpoofingMarkdownFileName, "OptiScaler-Spoofing.md")
        Return settings
    End Function

    Private Function ParseDefinitionsFromReferenceIni() As List(Of IniSettingDefinition)
        Dim result As New List(Of IniSettingDefinition)()
        Dim path As String = ResolveDataFilePath(ReferenceIniFileName)
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return result
        End If

        Dim lines As String() = File.ReadAllLines(path)
        Dim section As String = ""
        Dim pendingComments As New List(Of String)()
        Dim lastDescription As String = ""
        Dim lastWasKey As Boolean = False

        For Each raw As String In lines
            Dim line As String = If(raw, "")
            Dim trimmed As String = line.Trim()

            If String.IsNullOrWhiteSpace(trimmed) Then
                pendingComments.Clear()
                lastDescription = ""
                lastWasKey = False
                Continue For
            End If

            If trimmed.StartsWith(";", StringComparison.Ordinal) Then
                pendingComments.Add(trimmed.TrimStart(";"c).Trim())
                Continue For
            End If

            Dim sectionMatch As Match = Regex.Match(trimmed, "^\[(.+)\]$")
            If sectionMatch.Success Then
                section = sectionMatch.Groups(1).Value.Trim()
                pendingComments.Clear()
                lastDescription = ""
                lastWasKey = False
                Continue For
            End If

            Dim keyValueMatch As Match = Regex.Match(trimmed, "^([A-Za-z][A-Za-z0-9_]*)\s*=\s*(.*)$")
            If keyValueMatch.Success AndAlso Not String.IsNullOrWhiteSpace(section) Then
                Dim key As String = keyValueMatch.Groups(1).Value.Trim()
                Dim defaultValue As String = keyValueMatch.Groups(2).Value.Trim()
                Dim description As String = BuildDescription(pendingComments, lastDescription, lastWasKey)
                Dim allowedValues As String = ExtractAllowedValues(description)
                Dim defaultFromDescription As String = ExtractDefaultValue(description)

                Dim definition As New IniSettingDefinition With {
                    .Section = section,
                    .Key = key,
                    .Label = key,
                    .Description = If(String.IsNullOrWhiteSpace(description),
                                      "No official inline description found in OptiScaler reference INI.",
                                      description),
                    .AllowedValues = allowedValues,
                    .DefaultValue = If(String.IsNullOrWhiteSpace(defaultFromDescription), defaultValue, defaultFromDescription),
                    .Source = "OptiScaler-Reference.ini"
                }
                result.Add(definition)

                If Not String.IsNullOrWhiteSpace(description) Then
                    lastDescription = description
                End If

                pendingComments.Clear()
                lastWasKey = True
                Continue For
            End If

            pendingComments.Clear()
            lastDescription = ""
            lastWasKey = False
        Next

        Return result
    End Function

    Private Sub MergeMarkdownDescriptions(settings As List(Of IniSettingDefinition), fileName As String, sourceName As String)
        If settings Is Nothing OrElse settings.Count = 0 Then
            Return
        End If

        Dim path As String = ResolveDataFilePath(fileName)
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return
        End If

        Dim markdownMap As Dictionary(Of String, String) = ParseMarkdownDescriptions(path)
        If markdownMap.Count = 0 Then
            Return
        End If

        For Each setting As IniSettingDefinition In settings
            Dim mapKey As String = BuildMapKey(setting.Section, setting.Key)
            Dim markdownDescription As String = ""
            If Not markdownMap.TryGetValue(mapKey, markdownDescription) Then
                Continue For
            End If

            If String.IsNullOrWhiteSpace(markdownDescription) Then
                Continue For
            End If

            If String.IsNullOrWhiteSpace(setting.Description) OrElse
               setting.Description.StartsWith("No official inline description", StringComparison.OrdinalIgnoreCase) Then
                setting.Description = markdownDescription
                setting.Source = sourceName
                If String.IsNullOrWhiteSpace(setting.AllowedValues) Then
                    setting.AllowedValues = ExtractAllowedValues(markdownDescription)
                End If
                If String.IsNullOrWhiteSpace(setting.DefaultValue) Then
                    setting.DefaultValue = ExtractDefaultValue(markdownDescription)
                End If
            End If
        Next
    End Sub

    Private Function ParseMarkdownDescriptions(path As String) As Dictionary(Of String, String)
        Dim map As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        Dim lines As String() = File.ReadAllLines(path)

        Dim inCodeBlock As Boolean = False
        Dim section As String = ""
        Dim heading As String = ""
        Dim comments As New List(Of String)()

        For Each raw As String In lines
            Dim line As String = If(raw, "")
            Dim trimmed As String = line.Trim()

            If trimmed.StartsWith("### ", StringComparison.Ordinal) Then
                heading = trimmed.Substring(4).Trim()
                Continue For
            End If

            If trimmed.StartsWith("```", StringComparison.Ordinal) Then
                inCodeBlock = Not inCodeBlock
                section = ""
                comments.Clear()
                Continue For
            End If

            If Not inCodeBlock Then
                Continue For
            End If

            If String.IsNullOrWhiteSpace(trimmed) Then
                comments.Clear()
                Continue For
            End If

            If trimmed.StartsWith(";", StringComparison.Ordinal) Then
                comments.Add(trimmed.TrimStart(";"c).Trim())
                Continue For
            End If

            Dim sectionMatch As Match = Regex.Match(trimmed, "^\[(.+)\]$")
            If sectionMatch.Success Then
                section = sectionMatch.Groups(1).Value.Trim()
                comments.Clear()
                Continue For
            End If

            Dim keyValueMatch As Match = Regex.Match(trimmed, "^([A-Za-z][A-Za-z0-9_]*)\s*=\s*(.*)$")
            If keyValueMatch.Success AndAlso Not String.IsNullOrWhiteSpace(section) Then
                Dim key As String = keyValueMatch.Groups(1).Value.Trim()
                Dim detail As String = NormalizeDescriptionLines(comments)
                If String.IsNullOrWhiteSpace(detail) AndAlso Not String.IsNullOrWhiteSpace(heading) Then
                    detail = "Reference section: " & heading
                ElseIf Not String.IsNullOrWhiteSpace(detail) AndAlso Not String.IsNullOrWhiteSpace(heading) Then
                    detail &= Environment.NewLine & "Reference section: " & heading
                End If

                map(BuildMapKey(section, key)) = detail
                comments.Clear()
            End If
        Next

        Return map
    End Function

    Private Function BuildFallbackDefinitions() As List(Of IniSettingDefinition)
        Return New List(Of IniSettingDefinition) From {
            New IniSettingDefinition With {.Section = "Spoofing", .Key = "Dxgi", .Label = "DXGI Spoofing", .Description = "Controls spoofing mode. Expected: auto, true, or false.", .AllowedValues = "auto, true, false", .DefaultValue = "auto", .Source = "Fallback"},
            New IniSettingDefinition With {.Section = "FrameGen", .Key = "FGInput", .Label = "Frame Generation Input", .Description = "Selects frame generation input source.", .DefaultValue = "auto", .Source = "Fallback"},
            New IniSettingDefinition With {.Section = "FrameGen", .Key = "FGOutput", .Label = "Frame Generation Output", .Description = "Selects frame generation output backend.", .DefaultValue = "auto", .Source = "Fallback"},
            New IniSettingDefinition With {.Section = "Plugins", .Key = "LoadAsiPlugins", .Label = "Load ASI Plugins", .Description = "Enable loading *.asi plugins from Plugins.Path.", .AllowedValues = "true, false, auto", .DefaultValue = "false", .Source = "Fallback"},
            New IniSettingDefinition With {.Section = "Plugins", .Key = "Path", .Label = "Plugins Path", .Description = "Folder path for ASI plugins.", .DefaultValue = "plugins", .Source = "Fallback"},
            New IniSettingDefinition With {.Section = "Main", .Key = "Fsr4Update", .Label = "FSR4 Update", .Description = "Experimental flag used by the FSR4 INT8 workflow.", .AllowedValues = "true, false, auto", .DefaultValue = "false", .Source = "Fallback"},
            New IniSettingDefinition With {.Section = "Main", .Key = "FsrAgilitySDKUpgrade", .Label = "FSR Agility SDK Upgrade", .Description = "Experimental flag for some Windows 10 titles.", .AllowedValues = "true, false, auto", .DefaultValue = "false", .Source = "Fallback"}
        }
    End Function

    Private Function BuildDescription(pendingComments As List(Of String), lastDescription As String, lastWasKey As Boolean) As String
        Dim explicitDescription As String = NormalizeDescriptionLines(pendingComments)
        If Not String.IsNullOrWhiteSpace(explicitDescription) Then
            Return explicitDescription
        End If

        If lastWasKey AndAlso Not String.IsNullOrWhiteSpace(lastDescription) Then
            Return lastDescription
        End If

        Return ""
    End Function

    Private Function NormalizeDescriptionLines(commentLines As IEnumerable(Of String)) As String
        If commentLines Is Nothing Then
            Return ""
        End If

        Dim cleaned As List(Of String) = commentLines.
            Select(Function(line) If(line, "").Trim()).
            Where(Function(line) line.Length > 0).
            ToList()
        If cleaned.Count = 0 Then
            Return ""
        End If

        Return String.Join(Environment.NewLine, cleaned)
    End Function

    Private Function ExtractAllowedValues(description As String) As String
        If String.IsNullOrWhiteSpace(description) Then
            Return ""
        End If

        Dim lines As String() = description.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
        For Each rawLine As String In lines
            Dim line As String = rawLine.Trim()
            If line.Length = 0 Then
                Continue For
            End If

            If line.IndexOf("true or false", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return "true, false, auto"
            End If

            Dim enumMatch As Match = Regex.Match(line, "^([A-Za-z0-9_./+\-\s,]+?)\s*-\s*Default", RegexOptions.IgnoreCase)
            If enumMatch.Success Then
                Dim candidate As String = enumMatch.Groups(1).Value.Trim()
                If candidate.IndexOf(","c) >= 0 Then
                    Return candidate
                End If
            End If

            Dim rangeMatch As Match = Regex.Match(line, "(\d+(?:\.\d+)?)\s*-\s*(\d+(?:\.\d+)?)")
            If rangeMatch.Success AndAlso line.IndexOf("Default", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return $"Range: {rangeMatch.Groups(1).Value} - {rangeMatch.Groups(2).Value}"
            End If
        Next

        Return ""
    End Function

    Private Function ExtractDefaultValue(description As String) As String
        If String.IsNullOrWhiteSpace(description) Then
            Return ""
        End If

        Dim match As Match = Regex.Match(description, "Default\s*\((?:auto|[^)]*)\)\s*is\s*([^\r\n.;]+)", RegexOptions.IgnoreCase)
        If match.Success Then
            Return match.Groups(1).Value.Trim()
        End If

        match = Regex.Match(description, "Default\s*is\s*([^\r\n.;]+)", RegexOptions.IgnoreCase)
        If match.Success Then
            Return match.Groups(1).Value.Trim()
        End If

        Return ""
    End Function

    Private Function ValidateNumericRange(definition As IniSettingDefinition, value As String) As IniValidationResult
        If String.IsNullOrWhiteSpace(value) OrElse String.Equals(value, "auto", StringComparison.OrdinalIgnoreCase) Then
            Return Nothing
        End If

        Dim sourceText As String = $"{definition.AllowedValues} {definition.Description}"
        Dim rangeMatch As Match = Regex.Match(sourceText, "(\d+(?:\.\d+)?)\s*-\s*(\d+(?:\.\d+)?)", RegexOptions.IgnoreCase)
        If Not rangeMatch.Success Then
            Return Nothing
        End If

        Dim minValue As Double = 0
        Dim maxValue As Double = 0
        If Not Double.TryParse(rangeMatch.Groups(1).Value, minValue) OrElse Not Double.TryParse(rangeMatch.Groups(2).Value, maxValue) Then
            Return Nothing
        End If

        Dim numericValue As Double = 0
        If Not Double.TryParse(value, numericValue) Then
            Return New IniValidationResult With {
                .Severity = IniValidationSeverity.Warning,
                .Message = $"Expected numeric value in range {minValue} - {maxValue}."
            }
        End If

        If numericValue < minValue OrElse numericValue > maxValue Then
            Return New IniValidationResult With {
                .Severity = IniValidationSeverity.Warning,
                .Message = $"Value is outside expected range {minValue} - {maxValue}."
            }
        End If

        Return Nothing
    End Function

    Private Function IsLikelyBooleanDefinition(definition As IniSettingDefinition) As Boolean
        If definition Is Nothing Then
            Return False
        End If

        Dim text As String = $"{definition.AllowedValues} {definition.Description}"
        Return text.IndexOf("true or false", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               text.IndexOf("true, false", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Function ResolveDataFilePath(fileName As String) As String
        If String.IsNullOrWhiteSpace(fileName) Then
            Return ""
        End If

        Dim candidates As String() = {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", fileName),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName),
            Path.Combine(Application.StartupPath, "Data", fileName),
            Path.Combine(Application.StartupPath, fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Data", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), fileName)
        }

        For Each candidate As String In candidates
            If File.Exists(candidate) Then
                Return candidate
            End If
        Next

        Return ""
    End Function

    Private Function BuildMapKey(section As String, key As String) As String
        Return $"{If(section, "").Trim()}|{If(key, "").Trim()}"
    End Function

    Private Function CloneDefinition(value As IniSettingDefinition) As IniSettingDefinition
        If value Is Nothing Then
            Return Nothing
        End If

        Return New IniSettingDefinition With {
            .Section = value.Section,
            .Key = value.Key,
            .Label = value.Label,
            .Description = value.Description,
            .AllowedValues = value.AllowedValues,
            .DefaultValue = value.DefaultValue,
            .Source = value.Source
        }
    End Function
End Module
