Option Strict On
Option Explicit On

Imports System.Text

Friend Structure MarkdownLink
    Public Property Text As String
    Public Property Target As String
    Public Property IsImage As Boolean
End Structure

Friend Module MarkdownLinkParser
    Public Function UnescapeText(value As String) As String
        Return UnescapeMarkdown(value)
    End Function

    Public Function TryGetFirstLink(value As String, ByRef link As MarkdownLink, Optional allowImages As Boolean = False) As Boolean
        Dim startIndex As Integer = 0
        Dim linkStart As Integer = 0
        Dim linkEnd As Integer = 0
        Return TryFindNextLink(value, startIndex, link, linkStart, linkEnd, allowImages)
    End Function

    Public Function ReplaceInlineLinks(value As String) As String
        If String.IsNullOrEmpty(value) Then
            Return String.Empty
        End If

        Dim result As New StringBuilder()
        Dim scanIndex As Integer = 0
        Dim link As New MarkdownLink()
        Dim linkStart As Integer = 0
        Dim linkEnd As Integer = 0

        While TryFindNextLink(value, scanIndex, link, linkStart, linkEnd, True)
            If linkStart > scanIndex Then
                result.Append(value.Substring(scanIndex, linkStart - scanIndex))
            End If

            result.Append(FormatLinkReplacement(link))
            scanIndex = linkEnd + 1
        End While

        If scanIndex < value.Length Then
            result.Append(value.Substring(scanIndex))
        End If

        Return result.ToString()
    End Function

    Private Function TryFindNextLink(value As String,
                                     startIndex As Integer,
                                     ByRef link As MarkdownLink,
                                     ByRef linkStart As Integer,
                                     ByRef linkEnd As Integer,
                                     allowImages As Boolean) As Boolean
        link = New MarkdownLink()
        linkStart = -1
        linkEnd = -1

        If String.IsNullOrEmpty(value) OrElse startIndex >= value.Length Then
            Return False
        End If

        Dim scanIndex As Integer = Math.Max(0, startIndex)
        While scanIndex < value.Length
            Dim openBracket As Integer = FindNextUnescaped(value, "["c, scanIndex)
            If openBracket < 0 Then
                Return False
            End If

            Dim imageStart As Integer = openBracket - 1
            Dim isImage As Boolean = imageStart >= 0 AndAlso value(imageStart) = "!"c AndAlso Not IsEscaped(value, imageStart)
            If isImage AndAlso Not allowImages Then
                scanIndex = openBracket + 1
                Continue While
            End If

            Dim closeBracket As Integer = FindNextUnescaped(value, "]"c, openBracket + 1)
            If closeBracket < 0 Then
                Return False
            End If

            Dim openParen As Integer = closeBracket + 1
            If openParen >= value.Length OrElse value(openParen) <> "("c Then
                scanIndex = closeBracket + 1
                Continue While
            End If

            Dim closeParen As Integer = FindClosingDestinationParen(value, openParen + 1)
            If closeParen < 0 Then
                Return False
            End If

            link = New MarkdownLink With {
                .Text = UnescapeMarkdown(value.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim()),
                .Target = NormalizeTarget(value.Substring(openParen + 1, closeParen - openParen - 1).Trim()),
                .IsImage = isImage
            }
            linkStart = If(isImage, imageStart, openBracket)
            linkEnd = closeParen
            Return True
        End While

        Return False
    End Function

    Private Function FindClosingDestinationParen(value As String, startIndex As Integer) As Integer
        Dim depth As Integer = 0
        Dim index As Integer = startIndex

        While index < value.Length
            If IsEscaped(value, index) Then
                index += 1
                Continue While
            End If

            Select Case value(index)
                Case "("c
                    depth += 1
                Case ")"c
                    If depth = 0 Then
                        Return index
                    End If
                    depth -= 1
            End Select

            index += 1
        End While

        Return -1
    End Function

    Private Function FindNextUnescaped(value As String, target As Char, startIndex As Integer) As Integer
        Dim index As Integer = Math.Max(0, startIndex)
        While index < value.Length
            If value(index) = target AndAlso Not IsEscaped(value, index) Then
                Return index
            End If
            index += 1
        End While

        Return -1
    End Function

    Private Function IsEscaped(value As String, index As Integer) As Boolean
        Dim slashCount As Integer = 0
        Dim scanIndex As Integer = index - 1

        While scanIndex >= 0 AndAlso value(scanIndex) = "\"c
            slashCount += 1
            scanIndex -= 1
        End While

        Return slashCount Mod 2 = 1
    End Function

    Private Function NormalizeTarget(value As String) As String
        Dim target As String = UnescapeMarkdown(If(value, "").Trim())
        If target.Length >= 2 AndAlso target.StartsWith("<", StringComparison.Ordinal) AndAlso target.EndsWith(">", StringComparison.Ordinal) Then
            target = target.Substring(1, target.Length - 2).Trim()
        End If

        Return target
    End Function

    Private Function UnescapeMarkdown(value As String) As String
        If String.IsNullOrEmpty(value) Then
            Return String.Empty
        End If

        Dim result As New StringBuilder(value.Length)
        Dim index As Integer = 0
        While index < value.Length
            If value(index) = "\"c AndAlso index + 1 < value.Length AndAlso IsMarkdownEscapable(value(index + 1)) Then
                index += 1
            End If

            result.Append(value(index))
            index += 1
        End While

        Return result.ToString()
    End Function

    Private Function IsMarkdownEscapable(value As Char) As Boolean
        Const escapable As String = "\`*{}[]()#+-.!_|<>"
        Return escapable.IndexOf(value) >= 0
    End Function

    Private Function FormatLinkReplacement(link As MarkdownLink) As String
        Dim caption As String = If(link.Text, "").Trim()
        Dim target As String = If(link.Target, "").Trim()

        If String.IsNullOrWhiteSpace(caption) Then
            Return target
        End If
        If String.IsNullOrWhiteSpace(target) OrElse String.Equals(caption, target, StringComparison.OrdinalIgnoreCase) Then
            Return caption
        End If

        Return $"{caption} ({target})"
    End Function
End Module
