Imports System.IO
Imports System.Text.Json

Public Class GameWorkaroundTemplate
    ' Optional per-game recommendations that can auto-fill install controls.
    Public Property Name As String
    Public Property Aliases As List(Of String)
    Public Property HookName As String
    Public Property GpuVendor As String
    Public Property DlssInputs As Boolean?
    Public Property FrameGeneration As String
    Public Property ConflictMode As String
    Public Property Notes As String
End Class

Public Module GameTemplateService
    ' Loads optional per-game install presets from Data\GameTemplates.json.
    Private ReadOnly TemplatesPath As String = Path.Combine(AppContext.BaseDirectory, "Data", "GameTemplates.json")
    Private ReadOnly SyncRoot As New Object()
    Private _templates As List(Of GameWorkaroundTemplate)

    ' Matches by display name and aliases using relaxed normalization tokens.
    Public Function FindTemplate(gameName As String) As GameWorkaroundTemplate
        If String.IsNullOrWhiteSpace(gameName) Then
            Return Nothing
        End If

        Dim templates As List(Of GameWorkaroundTemplate) = LoadTemplates()
        If templates.Count = 0 Then
            Return Nothing
        End If

        Dim inputKey As String = NameNormalization.NormalizeRelaxedName(gameName)
        For Each template As GameWorkaroundTemplate In templates
            If template Is Nothing Then
                Continue For
            End If

            If IsTemplateMatch(template.Name, inputKey) Then
                Return template
            End If

            If template.Aliases Is Nothing Then
                Continue For
            End If

            For Each aliasValue As String In template.Aliases
                If IsTemplateMatch(aliasValue, inputKey) Then
                    Return template
                End If
            Next
        Next

        Return Nothing
    End Function

    Private Function IsTemplateMatch(value As String, inputKey As String) As Boolean
        If String.IsNullOrWhiteSpace(value) OrElse String.IsNullOrWhiteSpace(inputKey) Then
            Return False
        End If

        Dim key As String = NameNormalization.NormalizeRelaxedName(value)
        Return String.Equals(key, inputKey, StringComparison.OrdinalIgnoreCase)
    End Function

    ' Cached template load to avoid repeated JSON parsing during grid interactions.
    Private Function LoadTemplates() As List(Of GameWorkaroundTemplate)
        SyncLock SyncRoot
            If _templates IsNot Nothing Then
                Return _templates
            End If

            _templates = New List(Of GameWorkaroundTemplate)()
            If Not File.Exists(TemplatesPath) Then
                Return _templates
            End If

            Try
                Dim json As String = File.ReadAllText(TemplatesPath)
                Dim loaded As List(Of GameWorkaroundTemplate) = JsonSerializer.Deserialize(Of List(Of GameWorkaroundTemplate))(json)
                If loaded IsNot Nothing Then
                    _templates = loaded
                End If
            Catch ex As Exception
                ErrorLogger.Log(ex, "GameTemplateService.LoadTemplates")
            End Try

            Return _templates
        End SyncLock
    End Function
End Module
