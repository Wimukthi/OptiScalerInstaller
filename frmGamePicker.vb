Option Strict On
Option Explicit On

Imports System.Linq

Friend Partial Class frmGamePicker
    Inherits Form

    Private ReadOnly _allEntries As List(Of CompatibilityEntry)
    Private ReadOnly _candidates As List(Of CompatibilityEntry)
    Private ReadOnly _hasCandidates As Boolean

    ' The compatibility entry the user confirmed (Nothing when cancelled).
    Public ReadOnly Property SelectedEntry As CompatibilityEntry

    Public Sub New(candidates As IEnumerable(Of CompatibilityEntry),
                   allEntries As IEnumerable(Of CompatibilityEntry),
                   contextText As String)
        InitializeComponent()

        _candidates = NormalizeEntries(candidates)
        _allEntries = NormalizeEntries(allEntries)
        _hasCandidates = _candidates.Count > 0

        If Not String.IsNullOrWhiteSpace(contextText) Then
            lblIntro.Text = contextText
        End If

        Try
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmGamePicker.LoadIcon")
        End Try

        ' Without suggested matches there is nothing to narrow down, so force the full list.
        chkShowAll.Checked = Not _hasCandidates
        chkShowAll.Enabled = _hasCandidates

        ThemeManager.ApplyTheme(Me, ThemeSettings.GetPreferredColorMode())
        PopulateGames()
    End Sub

    Private Shared Function NormalizeEntries(entries As IEnumerable(Of CompatibilityEntry)) As List(Of CompatibilityEntry)
        If entries Is Nothing Then
            Return New List(Of CompatibilityEntry)()
        End If

        Return entries.
            Where(Function(entry) entry IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(entry.Name)).
            GroupBy(Function(entry) entry.Name, StringComparer.OrdinalIgnoreCase).
            Select(Function(group) group.First()).
            OrderBy(Function(entry) entry.Name, StringComparer.OrdinalIgnoreCase).
            ToList()
    End Function

    Private Function ActiveSource() As List(Of CompatibilityEntry)
        If chkShowAll.Checked OrElse Not _hasCandidates Then
            Return _allEntries
        End If

        Return _candidates
    End Function

    Private Sub PopulateGames()
        Dim filter As String = txtSearch.Text.Trim()
        Dim source As List(Of CompatibilityEntry) = ActiveSource()

        Dim visible As IEnumerable(Of CompatibilityEntry) = source
        If Not String.IsNullOrWhiteSpace(filter) Then
            visible = source.Where(Function(entry) entry.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
        End If

        lvGames.BeginUpdate()
        lvGames.Items.Clear()
        For Each entry As CompatibilityEntry In visible
            Dim item As New ListViewItem(entry.Name) With {
                .Tag = entry
            }
            lvGames.Items.Add(item)
        Next
        lvGames.EndUpdate()

        If lvGames.Items.Count > 0 Then
            lvGames.Items(0).Selected = True
        End If

        UpdateColumnWidth()
        UpdateAddState()
    End Sub

    Private Sub UpdateAddState()
        btnAdd.Enabled = lvGames.SelectedItems.Count > 0
    End Sub

    Private Function GetSelectedEntry() As CompatibilityEntry
        If lvGames.SelectedItems.Count = 0 Then
            Return Nothing
        End If

        Return TryCast(lvGames.SelectedItems(0).Tag, CompatibilityEntry)
    End Function

    Private Sub CommitSelection()
        Dim entry As CompatibilityEntry = GetSelectedEntry()
        If entry Is Nothing Then
            MessageBox.Show(Me, "Select a game from the list.", "Add Game Manually", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        _SelectedEntry = entry
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        PopulateGames()
    End Sub

    Private Sub chkShowAll_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowAll.CheckedChanged
        PopulateGames()
    End Sub

    Private Sub lvGames_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvGames.SelectedIndexChanged
        UpdateAddState()
    End Sub

    Private Sub lvGames_DoubleClick(sender As Object, e As EventArgs) Handles lvGames.DoubleClick
        CommitSelection()
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        CommitSelection()
    End Sub

    Private Sub frmGamePicker_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        UpdateColumnWidth()
    End Sub

    Private Sub UpdateColumnWidth()
        If lvGames Is Nothing OrElse lvGames.Columns.Count = 0 Then
            Return
        End If

        Dim width As Integer = lvGames.ClientSize.Width - 8
        If width < 160 Then
            width = 160
        End If

        lvGames.Columns(0).Width = width
    End Sub
End Class
