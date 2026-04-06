Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Linq

Public Class IniDocument
    Private Enum IniLineKind
        Raw
        Section
        KeyValue
    End Enum

    Private Class IniLine
        Public Property Kind As IniLineKind
        Public Property RawText As String
        Public Property SectionName As String
        Public Property Key As String
        Public Property Value As String
    End Class

    Private ReadOnly _lines As List(Of IniLine)

    Private Sub New(lines As List(Of IniLine))
        _lines = If(lines, New List(Of IniLine)())
    End Sub

    Public Shared Function Load(path As String) As IniDocument
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return New IniDocument(New List(Of IniLine)())
        End If

        Dim content As String = File.ReadAllText(path)
        Return Parse(content)
    End Function

    Public Shared Function Parse(content As String) As IniDocument
        Dim lines As New List(Of IniLine)()
        Dim currentSection As String = ""

        Using reader As New StringReader(If(content, ""))
            While True
                Dim raw As String = reader.ReadLine()
                If raw Is Nothing Then
                    Exit While
                End If

                Dim parsed As IniLine = ParseLine(raw, currentSection)
                If parsed.Kind = IniLineKind.Section Then
                    currentSection = parsed.SectionName
                End If
                lines.Add(parsed)
            End While
        End Using

        Return New IniDocument(lines)
    End Function

    Public Function ToText() As String
        Dim builder As New StringBuilder()
        For i As Integer = 0 To _lines.Count - 1
            If i > 0 Then
                builder.AppendLine()
            End If
            builder.Append(_lines(i).RawText)
        Next

        Return builder.ToString()
    End Function

    Public Function GetEffectiveValues() As List(Of IniUpdate)
        Dim map As New Dictionary(Of String, IniUpdate)(StringComparer.OrdinalIgnoreCase)
        For Each line As IniLine In _lines
            If line.Kind <> IniLineKind.KeyValue Then
                Continue For
            End If

            Dim key As String = BuildMapKey(line.SectionName, line.Key)
            map(key) = New IniUpdate With {
                .Section = line.SectionName,
                .Key = line.Key,
                .Value = line.Value
            }
        Next

        Return map.Values.
            OrderBy(Function(item) item.Section, StringComparer.OrdinalIgnoreCase).
            ThenBy(Function(item) item.Key, StringComparer.OrdinalIgnoreCase).
            ToList()
    End Function

    Public Function TryGetValue(section As String, key As String, ByRef value As String) As Boolean
        value = ""
        For Each line As IniLine In _lines
            If line.Kind <> IniLineKind.KeyValue Then
                Continue For
            End If
            If String.Equals(line.SectionName, section, StringComparison.OrdinalIgnoreCase) AndAlso
               String.Equals(line.Key, key, StringComparison.OrdinalIgnoreCase) Then
                value = line.Value
            End If
        Next

        Return Not String.IsNullOrWhiteSpace(value) OrElse ContainsKey(section, key)
    End Function

    Public Function ContainsKey(section As String, key As String) As Boolean
        For Each line As IniLine In _lines
            If line.Kind <> IniLineKind.KeyValue Then
                Continue For
            End If
            If String.Equals(line.SectionName, section, StringComparison.OrdinalIgnoreCase) AndAlso
               String.Equals(line.Key, key, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Sub SetValue(section As String, key As String, value As String)
        If String.IsNullOrWhiteSpace(section) OrElse String.IsNullOrWhiteSpace(key) Then
            Return
        End If

        For i As Integer = 0 To _lines.Count - 1
            Dim line As IniLine = _lines(i)
            If line.Kind <> IniLineKind.KeyValue Then
                Continue For
            End If

            If String.Equals(line.SectionName, section, StringComparison.OrdinalIgnoreCase) AndAlso
               String.Equals(line.Key, key, StringComparison.OrdinalIgnoreCase) Then
                line.Value = If(value, "")
                line.RawText = BuildKeyLine(key, line.Value)
                Return
            End If
        Next

        Dim sectionIndex As Integer = FindSectionIndex(section)
        If sectionIndex < 0 Then
            If _lines.Count > 0 AndAlso _lines(_lines.Count - 1).RawText.Trim().Length > 0 Then
                _lines.Add(New IniLine With {.Kind = IniLineKind.Raw, .RawText = ""})
            End If

            _lines.Add(New IniLine With {
                       .Kind = IniLineKind.Section,
                       .SectionName = section,
                       .RawText = $"[{section}]"
                   })
            _lines.Add(New IniLine With {
                       .Kind = IniLineKind.KeyValue,
                       .SectionName = section,
                       .Key = key,
                       .Value = If(value, ""),
                       .RawText = BuildKeyLine(key, value)
                   })
            Return
        End If

        Dim insertIndex As Integer = _lines.Count
        For i As Integer = sectionIndex + 1 To _lines.Count - 1
            If _lines(i).Kind = IniLineKind.Section Then
                insertIndex = i
                Exit For
            End If
        Next

        _lines.Insert(insertIndex, New IniLine With {
                      .Kind = IniLineKind.KeyValue,
                      .SectionName = section,
                      .Key = key,
                      .Value = If(value, ""),
                      .RawText = BuildKeyLine(key, value)
                  })
    End Sub

    Private Shared Function ParseLine(raw As String, currentSection As String) As IniLine
        Dim line As String = If(raw, "")
        Dim trimmed As String = line.Trim()

        If trimmed.StartsWith("[") AndAlso trimmed.EndsWith("]") AndAlso trimmed.Length >= 3 Then
            Dim sectionName As String = trimmed.Substring(1, trimmed.Length - 2).Trim()
            Return New IniLine With {
                .Kind = IniLineKind.Section,
                .RawText = line,
                .SectionName = sectionName
            }
        End If

        Dim equalsIndex As Integer = trimmed.IndexOf("="c)
        If equalsIndex > 0 AndAlso Not String.IsNullOrWhiteSpace(currentSection) Then
            Dim key As String = trimmed.Substring(0, equalsIndex).Trim()
            If key.Length > 0 Then
                Dim value As String = trimmed.Substring(equalsIndex + 1).Trim()
                Return New IniLine With {
                    .Kind = IniLineKind.KeyValue,
                    .RawText = line,
                    .SectionName = currentSection,
                    .Key = key,
                    .Value = value
                }
            End If
        End If

        Return New IniLine With {
            .Kind = IniLineKind.Raw,
            .RawText = line
        }
    End Function

    Private Shared Function BuildKeyLine(key As String, value As String) As String
        Return $"{key}={If(value, "")}"
    End Function

    Private Function FindSectionIndex(section As String) As Integer
        For i As Integer = 0 To _lines.Count - 1
            Dim line As IniLine = _lines(i)
            If line.Kind = IniLineKind.Section AndAlso
               String.Equals(line.SectionName, section, StringComparison.OrdinalIgnoreCase) Then
                Return i
            End If
        Next
        Return -1
    End Function

    Private Shared Function BuildMapKey(section As String, key As String) As String
        Return $"{If(section, "").Trim()}|{If(key, "").Trim()}"
    End Function
End Class
