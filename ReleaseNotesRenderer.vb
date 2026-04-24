Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Friend Module ReleaseNotesRenderer
    Public Sub Render(markdown As String, box As RichTextBox, mode As SystemColorMode)
        If box Is Nothing Then
            Return
        End If

        Dim palette As ReleaseNotesPalette = ReleaseNotesPalette.FromMode(mode)
        Dim originalReadOnly As Boolean = box.ReadOnly

        box.SuspendLayout()
        Try
            box.ReadOnly = False
            box.Clear()
            box.BackColor = palette.Field
            box.ForeColor = palette.Text
            box.DetectUrls = True

            Dim normalized As String = If(markdown, String.Empty).Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
            Dim lines As String() = normalized.Split(New Char() {ControlChars.Lf}, StringSplitOptions.None)
            Dim inCodeBlock As Boolean = False
            Dim pendingBlankLine As Boolean = False

            For Each rawLine As String In lines
                Dim line As String = rawLine.TrimEnd()
                Dim trimmedStart As String = line.TrimStart()

                If trimmedStart.StartsWith("```", StringComparison.Ordinal) Then
                    inCodeBlock = Not inCodeBlock
                    If inCodeBlock AndAlso box.TextLength > 0 AndAlso Not EndsWithNewLine(box) Then
                        AppendLineBreak(box, palette)
                    End If
                    pendingBlankLine = False
                    Continue For
                End If

                If inCodeBlock Then
                    AppendCodeLine(box, line, palette)
                    pendingBlankLine = False
                    Continue For
                End If

                If String.IsNullOrWhiteSpace(line) Then
                    pendingBlankLine = True
                    Continue For
                End If

                If pendingBlankLine AndAlso box.TextLength > 0 Then
                    AppendLineBreak(box, palette)
                End If
                pendingBlankLine = False

                Dim heading As Match = Regex.Match(line, "^\s{0,3}(#{1,6})\s+(.+?)\s*#*\s*$", RegexOptions.CultureInvariant)
                If heading.Success Then
                    AppendHeading(box, heading.Groups(2).Value, heading.Groups(1).Value.Length, palette)
                    Continue For
                End If

                Dim bullet As Match = Regex.Match(line, "^\s{0,12}[-*+]\s+(.+)$", RegexOptions.CultureInvariant)
                If bullet.Success Then
                    AppendBullet(box, bullet.Groups(1).Value, palette)
                    Continue For
                End If

                Dim ordered As Match = Regex.Match(line, "^\s{0,12}(\d+)[.)]\s+(.+)$", RegexOptions.CultureInvariant)
                If ordered.Success Then
                    AppendOrderedItem(box, ordered.Groups(1).Value, ordered.Groups(2).Value, palette)
                    Continue For
                End If

                If trimmedStart.StartsWith(">", StringComparison.Ordinal) Then
                    AppendQuote(box, trimmedStart.TrimStart(">"c).TrimStart(), palette)
                    Continue For
                End If

                If Regex.IsMatch(line.Trim(), "^(-{3,}|\*{3,}|_{3,})$", RegexOptions.CultureInvariant) Then
                    AppendDivider(box, palette)
                    Continue For
                End If

                AppendInlineMarkdown(box, line.Trim(), 9.0F, FontStyle.Regular, palette.Text, palette)
                AppendLineBreak(box, palette)
            Next

            box.Select(0, 0)
            box.ScrollToCaret()
        Finally
            box.ReadOnly = originalReadOnly
            box.ResumeLayout()
        End Try
    End Sub

    Private Sub AppendHeading(box As RichTextBox, value As String, level As Integer, palette As ReleaseNotesPalette)
        If box.TextLength > 0 AndAlso Not EndsWithNewLine(box) Then
            AppendLineBreak(box, palette)
        End If

        Dim fontSize As Single
        Select Case level
            Case 1
                fontSize = 13.5F
            Case 2
                fontSize = 12.0F
            Case Else
                fontSize = 10.5F
        End Select

        AppendInlineMarkdown(box, value.Trim(), fontSize, FontStyle.Bold, palette.Text, palette)
        AppendLineBreak(box, palette)
    End Sub

    Private Sub AppendBullet(box As RichTextBox, value As String, palette As ReleaseNotesPalette)
        box.Select(box.TextLength, 0)
        box.BulletIndent = 12
        box.SelectionIndent = 14
        box.SelectionBullet = True
        AppendInlineMarkdown(box, value.Trim(), 9.0F, FontStyle.Regular, palette.Text, palette)
        AppendLineBreak(box, palette)
        box.SelectionBullet = False
        box.SelectionIndent = 0
    End Sub

    Private Sub AppendOrderedItem(box As RichTextBox, numberText As String, value As String, palette As ReleaseNotesPalette)
        box.Select(box.TextLength, 0)
        box.SelectionIndent = 14
        AppendStyledText(box, numberText & ". ", 9.0F, FontStyle.Regular, palette.MutedText, "Segoe UI")
        AppendInlineMarkdown(box, value.Trim(), 9.0F, FontStyle.Regular, palette.Text, palette)
        AppendLineBreak(box, palette)
        box.SelectionIndent = 0
    End Sub

    Private Sub AppendQuote(box As RichTextBox, value As String, palette As ReleaseNotesPalette)
        box.Select(box.TextLength, 0)
        box.SelectionIndent = 14
        AppendInlineMarkdown(box, value.Trim(), 9.0F, FontStyle.Italic, palette.MutedText, palette)
        AppendLineBreak(box, palette)
        box.SelectionIndent = 0
    End Sub

    Private Sub AppendDivider(box As RichTextBox, palette As ReleaseNotesPalette)
        AppendStyledText(box, New String("-"c, 48), 9.0F, FontStyle.Regular, palette.MutedText, "Segoe UI")
        AppendLineBreak(box, palette)
    End Sub

    Private Sub AppendCodeLine(box As RichTextBox, value As String, palette As ReleaseNotesPalette)
        Dim text As String = If(value.Length = 0, " ", value)
        AppendStyledText(box, text, 9.0F, FontStyle.Regular, palette.CodeText, "Consolas", palette.CodeBack)
        AppendStyledText(box, Environment.NewLine, 9.0F, FontStyle.Regular, palette.CodeText, "Consolas", palette.CodeBack)
    End Sub

    Private Sub AppendLineBreak(box As RichTextBox, palette As ReleaseNotesPalette)
        AppendStyledText(box, Environment.NewLine, 9.0F, FontStyle.Regular, palette.Text, "Segoe UI")
    End Sub

    Private Sub AppendInlineMarkdown(box As RichTextBox, value As String, fontSize As Single, baseStyle As FontStyle, color As Color, palette As ReleaseNotesPalette)
        Dim text As String = NormalizeInlineMarkdown(value)
        Dim index As Integer = 0

        While index < text.Length
            If index <= text.Length - 2 AndAlso text.Substring(index, 2) = "**" Then
                Dim closeIndex As Integer = text.IndexOf("**", index + 2, StringComparison.Ordinal)
                If closeIndex > index Then
                    AppendStyledText(box, text.Substring(index + 2, closeIndex - index - 2), fontSize, baseStyle Or FontStyle.Bold, color, "Segoe UI")
                    index = closeIndex + 2
                    Continue While
                End If
            End If

            If text(index) = "`"c Then
                Dim closeIndex As Integer = text.IndexOf("`", index + 1, StringComparison.Ordinal)
                If closeIndex > index Then
                    AppendStyledText(box, text.Substring(index + 1, closeIndex - index - 1), 9.0F, FontStyle.Regular, palette.CodeText, "Consolas", palette.CodeBack)
                    index = closeIndex + 1
                    Continue While
                End If
            End If

            If text(index) = "*"c Then
                Dim closeIndex As Integer = text.IndexOf("*", index + 1, StringComparison.Ordinal)
                If closeIndex > index Then
                    AppendStyledText(box, text.Substring(index + 1, closeIndex - index - 1), fontSize, baseStyle Or FontStyle.Italic, color, "Segoe UI")
                    index = closeIndex + 1
                    Continue While
                End If
            End If

            Dim nextIndex As Integer = FindNextInlineMarker(text, index)
            If nextIndex = index Then
                nextIndex += 1
            End If

            AppendStyledText(box, text.Substring(index, nextIndex - index), fontSize, baseStyle, color, "Segoe UI")
            index = nextIndex
        End While
    End Sub

    Private Function NormalizeInlineMarkdown(value As String) As String
        If String.IsNullOrEmpty(value) Then
            Return String.Empty
        End If

        Dim text As String = value.Trim()
        text = Regex.Replace(text, "<(https?://[^>\s]+)>", "$1", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
        text = Regex.Replace(text, "!\[([^\]]*)\]\(([^)]+)\)", AddressOf ReplaceMarkdownLink, RegexOptions.CultureInvariant)
        text = Regex.Replace(text, "\[([^\]]+)\]\(([^)]+)\)", AddressOf ReplaceMarkdownLink, RegexOptions.CultureInvariant)
        text = text.Replace("\*", "*").Replace("\_", "_").Replace("\`", "`").Replace("\[", "[").Replace("\]", "]")
        Return text
    End Function

    Private Function ReplaceMarkdownLink(match As Match) As String
        Dim caption As String = match.Groups(1).Value.Trim()
        Dim url As String = match.Groups(2).Value.Trim()

        If String.IsNullOrWhiteSpace(caption) Then
            Return url
        End If
        If String.Equals(caption, url, StringComparison.OrdinalIgnoreCase) Then
            Return url
        End If

        Return $"{caption} ({url})"
    End Function

    Private Function FindNextInlineMarker(text As String, startIndex As Integer) As Integer
        Dim nextIndex As Integer = text.Length

        For Each marker As String In New String() {"**", "`", "*"}
            Dim markerIndex As Integer = text.IndexOf(marker, startIndex, StringComparison.Ordinal)
            If markerIndex >= 0 AndAlso markerIndex < nextIndex Then
                nextIndex = markerIndex
            End If
        Next

        Return nextIndex
    End Function

    Private Sub AppendStyledText(box As RichTextBox, value As String, fontSize As Single, style As FontStyle, color As Color, fontFamily As String, Optional backColor As Color = Nothing)
        If String.IsNullOrEmpty(value) Then
            Return
        End If

        box.Select(box.TextLength, 0)
        Using font As New Font(fontFamily, fontSize, style)
            box.SelectionFont = font
            box.SelectionColor = color
            box.SelectionBackColor = If(backColor.IsEmpty, box.BackColor, backColor)
            box.AppendText(value)
        End Using
    End Sub

    Private Function EndsWithNewLine(box As RichTextBox) As Boolean
        Return box.TextLength > 0 AndAlso (box.Text.EndsWith(vbLf, StringComparison.Ordinal) OrElse box.Text.EndsWith(vbCr, StringComparison.Ordinal))
    End Function

    Private Structure ReleaseNotesPalette
        Public Property Field As Color
        Public Property Text As Color
        Public Property MutedText As Color
        Public Property CodeText As Color
        Public Property CodeBack As Color

        Public Shared Function FromMode(mode As SystemColorMode) As ReleaseNotesPalette
            If mode = SystemColorMode.Dark Then
                Return New ReleaseNotesPalette With {
                    .Field = Color.FromArgb(45, 45, 45),
                    .Text = Color.Gainsboro,
                    .MutedText = Color.FromArgb(150, 150, 150),
                    .CodeText = Color.FromArgb(235, 235, 235),
                    .CodeBack = Color.FromArgb(35, 35, 35)
                }
            End If

            Return New ReleaseNotesPalette With {
                .Field = SystemColors.Window,
                .Text = SystemColors.ControlText,
                .MutedText = SystemColors.GrayText,
                .CodeText = SystemColors.ControlText,
                .CodeBack = Color.FromArgb(245, 245, 245)
            }
        End Function
    End Structure
End Module
