Imports System.IO

Public Class IniUpdate
    Public Property Section As String
    Public Property Key As String
    Public Property Value As String
End Class

Public Class IniFile
    Public Shared Sub SetValues(path As String, updates As List(Of IniUpdate))
        Dim lines As New List(Of String)(File.ReadAllLines(path))
        For Each update In updates
            SetValue(lines, update.Section, update.Key, update.Value)
        Next
        File.WriteAllLines(path, lines)
    End Sub

    Public Shared Function ReadValues(path As String) As List(Of IniUpdate)
        Dim results As New Dictionary(Of String, IniUpdate)(StringComparer.OrdinalIgnoreCase)
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then
            Return New List(Of IniUpdate)(results.Values)
        End If

        Dim currentSection As String = ""
        For Each rawLine As String In File.ReadAllLines(path)
            Dim line As String = rawLine.Trim()
            If line.Length = 0 OrElse line.StartsWith(";"c) OrElse line.StartsWith("#"c) Then
                Continue For
            End If

            If line.StartsWith("[") AndAlso line.EndsWith("]") Then
                currentSection = line.Substring(1, line.Length - 2).Trim()
                Continue For
            End If

            Dim index As Integer = line.IndexOf("="c)
            If index <= 0 Then
                Continue For
            End If

            If String.IsNullOrWhiteSpace(currentSection) Then
                Continue For
            End If

            Dim key As String = line.Substring(0, index).Trim()
            Dim value As String = line.Substring(index + 1).Trim()
            If String.IsNullOrWhiteSpace(key) Then
                Continue For
            End If

            Dim dictKey As String = currentSection & "|" & key
            results(dictKey) = New IniUpdate With {
                .Section = currentSection,
                .Key = key,
                .Value = value
            }
        Next

        Return New List(Of IniUpdate)(results.Values)
    End Function

    Private Shared Sub SetValue(lines As List(Of String), section As String, key As String, value As String)
        Dim sectionIndex As Integer = -1
        Dim insertIndex As Integer = lines.Count

        For i As Integer = 0 To lines.Count - 1
            Dim line As String = lines(i).Trim()
            If line.StartsWith("[") AndAlso line.EndsWith("]") Then
                Dim sectionName As String = line.Substring(1, line.Length - 2)
                If sectionName.Equals(section, StringComparison.OrdinalIgnoreCase) Then
                    sectionIndex = i
                    insertIndex = i + 1
                    For j As Integer = i + 1 To lines.Count - 1
                        Dim inner As String = lines(j).Trim()
                        If inner.StartsWith("[") AndAlso inner.EndsWith("]") Then
                            insertIndex = j
                            Exit For
                        End If
                        If inner.StartsWith(key & "=", StringComparison.OrdinalIgnoreCase) Then
                            lines(j) = key & "=" & value
                            Return
                        End If
                    Next
                    Exit For
                End If
            End If
        Next

        If sectionIndex = -1 Then
            lines.Add("")
            lines.Add("[" & section & "]")
            lines.Add(key & "=" & value)
        Else
            lines.Insert(insertIndex, key & "=" & value)
        End If
    End Sub
End Class
