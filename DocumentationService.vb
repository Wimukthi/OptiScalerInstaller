Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Public Enum InstallerDocumentation
    Usage
    AmdFsrSupport
    Configuration
    Troubleshooting
End Enum

Public Module DocumentationService
    Private Const RepositoryDocsBase As String = "https://github.com/Wimukthi/OptiScalerInstaller/blob/main/docs/"

    Public Function GetAvailableDocuments() As IReadOnlyList(Of InstallerDocumentation)
        Return Array.AsReadOnly(New InstallerDocumentation() {
            InstallerDocumentation.Usage,
            InstallerDocumentation.AmdFsrSupport,
            InstallerDocumentation.Configuration,
            InstallerDocumentation.Troubleshooting
        })
    End Function

    Public Function GetDocumentTitle(document As InstallerDocumentation) As String
        Select Case document
            Case InstallerDocumentation.Usage : Return "Usage"
            Case InstallerDocumentation.AmdFsrSupport : Return "AMD FSR support"
            Case InstallerDocumentation.Configuration : Return "Configuration"
            Case InstallerDocumentation.Troubleshooting : Return "Troubleshooting"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(document))
        End Select
    End Function

    Public Function GetOnlineUrl(document As InstallerDocumentation) As String
        Return RepositoryDocsBase & GetFileName(document)
    End Function

    Public Function LoadDocument(document As InstallerDocumentation, Optional docsDirectory As String = Nothing) As String
        Dim root As String = If(String.IsNullOrWhiteSpace(docsDirectory), Path.Combine(AppContext.BaseDirectory, "docs"), docsDirectory)
        Dim filePath As String = System.IO.Path.Combine(root, GetFileName(document))
        If Not File.Exists(filePath) Then
            Return "# " & GetDocumentTitle(document) & vbCrLf & vbCrLf &
                   "This document is not bundled with this installation. Use Open online to read it." & vbCrLf & vbCrLf &
                   GetOnlineUrl(document)
        End If

        Try
            Dim markdown As String = File.ReadAllText(filePath)
            Return PrepareMarkdownForDisplay(markdown, document)
        Catch ex As Exception
            Return "# " & GetDocumentTitle(document) & vbCrLf & vbCrLf &
                   "The bundled document could not be read. Use Open online to read it." & vbCrLf & vbCrLf &
                   GetOnlineUrl(document)
        End Try
    End Function

    Public Function PrepareMarkdownForDisplay(markdown As String, document As InstallerDocumentation) As String
        Dim normalized As String = If(markdown, String.Empty).Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        normalized = Regex.Replace(normalized, "!\[(?<alt>[^\]]*)\]\((?<target>[^)\s]+)\)",
            Function(match As Match) "[View screenshot](" & match.Groups("target").Value & ")", RegexOptions.CultureInvariant)
        normalized = Regex.Replace(normalized, "</?kbd\b[^>]*>", String.Empty, RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
        normalized = ConvertMarkdownTables(normalized)
        Return NormalizeMarkdownLinks(normalized, document)
    End Function

    Public Sub Show(owner As IWin32Window, Optional document As InstallerDocumentation = InstallerDocumentation.Usage)
        Using viewer As New frmDocumentation(document)
            viewer.ShowDialog(owner)
        End Using
    End Sub

    Public Function IsAllowedWebUrl(value As String) As Boolean
        Dim uri As Uri = Nothing
        Return Uri.TryCreate(value, UriKind.Absolute, uri) AndAlso
               (uri.Scheme = Uri.UriSchemeHttp OrElse uri.Scheme = Uri.UriSchemeHttps)
    End Function

    Private Function GetFileName(document As InstallerDocumentation) As String
        Select Case document
            Case InstallerDocumentation.Usage : Return "usage.md"
            Case InstallerDocumentation.AmdFsrSupport : Return "amd-fsr-support.md"
            Case InstallerDocumentation.Configuration : Return "configuration.md"
            Case InstallerDocumentation.Troubleshooting : Return "troubleshooting.md"
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(document))
        End Select
    End Function

    Private Function NormalizeMarkdownLinks(markdown As String, document As InstallerDocumentation) As String
        Dim currentUrl As String = GetOnlineUrl(document)
        Return Regex.Replace(If(markdown, String.Empty), "\]\((?<target>[^)\s]+)\)",
            Function(match As Match)
                Dim target As String = match.Groups("target").Value
                If IsAllowedWebUrl(target) Then Return match.Value
                If target.StartsWith("//", StringComparison.Ordinal) OrElse target.StartsWith("\\", StringComparison.Ordinal) Then Return "](#invalid-link)"

                Try
                    Dim baseUri As New Uri(currentUrl)
                    Dim onlineUri As Uri = New Uri(baseUri, target)
                    If onlineUri.Host <> "github.com" OrElse onlineUri.Scheme <> Uri.UriSchemeHttps OrElse
                       Not onlineUri.AbsolutePath.StartsWith("/Wimukthi/OptiScalerInstaller/blob/main/docs/", StringComparison.OrdinalIgnoreCase) Then
                        Return "](#invalid-link)"
                    End If
                    Return "](" & onlineUri.AbsoluteUri & ")"
                Catch ex As UriFormatException
                    Return "](#invalid-link)"
                End Try
            End Function, RegexOptions.CultureInvariant)
    End Function

    Private Function ConvertMarkdownTables(markdown As String) As String
        Dim lines As String() = markdown.Split(ControlChars.Lf)
        Dim output As New List(Of String)()
        Dim inCodeBlock As Boolean = False
        Dim index As Integer = 0
        While index < lines.Length
            Dim line As String = lines(index)
            If line.TrimStart().StartsWith("```", StringComparison.Ordinal) Then
                inCodeBlock = Not inCodeBlock
                output.Add(line)
                index += 1
                Continue While
            End If

            If Not inCodeBlock AndAlso index + 1 < lines.Length AndAlso line.Contains("|"c) AndAlso IsTableDelimiter(lines(index + 1)) Then
                Dim headers As List(Of String) = SplitTableCells(line)
                index += 2
                While index < lines.Length AndAlso lines(index).Contains("|"c)
                    Dim values As List(Of String) = SplitTableCells(lines(index))
                    If values.Any(Function(value) Not String.IsNullOrWhiteSpace(value)) Then
                        Dim fields As New List(Of String)()
                        For column As Integer = 0 To Math.Max(headers.Count, values.Count) - 1
                            Dim heading As String = If(column < headers.Count, headers(column), "Column " & (column + 1).ToString())
                            Dim value As String = If(column < values.Count, values(column), String.Empty)
                            If Not String.IsNullOrWhiteSpace(value) Then fields.Add("**" & heading.Trim() & ":** " & value.Trim())
                        Next
                        If fields.Count > 0 Then output.Add("- " & String.Join("; ", fields))
                    End If
                    index += 1
                End While
                Continue While
            End If

            output.Add(line)
            index += 1
        End While
        Return String.Join(vbLf, output)
    End Function

    Private Function IsTableDelimiter(line As String) As Boolean
        Dim cells As List(Of String) = SplitTableCells(line)
        Return cells.Count > 0 AndAlso cells.All(Function(cell) Regex.IsMatch(cell.Trim(), "^:?-{3,}:?$", RegexOptions.CultureInvariant))
    End Function

    Private Function SplitTableCells(line As String) As List(Of String)
        Dim value As String = If(line, String.Empty).Trim()
        If value.StartsWith("|", StringComparison.Ordinal) Then value = value.Substring(1)
        If value.EndsWith("|", StringComparison.Ordinal) AndAlso Not value.EndsWith("\|", StringComparison.Ordinal) Then value = value.Substring(0, value.Length - 1)
        Dim cells As New List(Of String)()
        Dim current As New System.Text.StringBuilder()
        Dim index As Integer = 0
        While index < value.Length
            Dim ch As Char = value(index)
            If ch = "\"c AndAlso index + 1 < value.Length AndAlso value(index + 1) = "|"c Then
                current.Append("|"c)
                index += 1
            ElseIf ch = "|"c Then
                cells.Add(current.ToString().Trim())
                current.Clear()
            Else
                current.Append(ch)
            End If
            index += 1
        End While
        cells.Add(current.ToString().Trim())
        Return cells
    End Function
End Module
