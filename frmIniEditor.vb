Option Strict On
Option Explicit On

Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.ComponentModel

Friend Class frmIniEditor
    Private Class IniGridRowModel
        Public Property Section As String
        Public Property Key As String
        Public Property Value As String
        Public Property Known As Boolean
        Public Property Description As String
        Public Property AllowedValues As String
        Public Property DefaultValue As String
        Public Property Source As String
        Public Property IsSectionHeader As Boolean
        Public Property DisplaySection As String
    End Class

    Private ReadOnly _iniPath As String
    Private ReadOnly _knownSettings As List(Of IniSettingDefinition)
    Private _document As IniDocument
    Private _originalText As String = ""
    Private _originalValues As Dictionary(Of String, String) = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private _isDirty As Boolean
    Private _ignoreEvents As Boolean
    Private _isDarkTheme As Boolean
    Private ReadOnly _collapsedSections As HashSet(Of String) = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property WasSaved As Boolean

    Public Sub New(iniPath As String)
        InitializeComponent()

        _iniPath = If(iniPath, "").Trim()
        _knownSettings = OptiScalerIniEditorService.GetKnownSettings()
        _isDarkTheme = ThemeSettings.GetPreferredColorMode() = SystemColorMode.Dark

        Try
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmIniEditor.LoadIcon")
        End Try

        ThemeManager.ApplyTheme(Me, ThemeSettings.GetPreferredColorMode())
        ApplyGridTheme()
        LockGridSorting()
        LoadDocumentFromDisk()
    End Sub

    Private Sub LoadDocumentFromDisk()
        _ignoreEvents = True
        Try
            _document = IniDocument.Load(_iniPath)
            _originalText = _document.ToText()
            _originalValues = BuildValueMap(_document)
            lblIniPathValue.Text = _iniPath
            txtFilter.Text = ""
            chkKnownOnly.Checked = False
            PopulateSectionFilter()
            RebuildGrid()
            txtRaw.Text = _document.ToText()
            SetDirty(False)
            WasSaved = False
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmIniEditor.LoadDocument")
            MessageBox.Show(Me, "Failed to load INI file: " & ex.Message, "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            _ignoreEvents = False
        End Try
    End Sub

    Private Sub PopulateSectionFilter()
        If cmbSectionFilter Is Nothing Then
            Return
        End If

        Dim sections As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each setting As IniSettingDefinition In _knownSettings
            If Not String.IsNullOrWhiteSpace(setting.Section) Then
                sections.Add(setting.Section.Trim())
            End If
        Next

        If _document IsNot Nothing Then
            For Each item As IniUpdate In _document.GetEffectiveValues()
                If Not String.IsNullOrWhiteSpace(item.Section) Then
                    sections.Add(item.Section.Trim())
                End If
            Next
        End If

        Dim sortedSections As List(Of String) = sections.
            OrderBy(Function(value) value, StringComparer.OrdinalIgnoreCase).
            ToList()

        cmbSectionFilter.BeginUpdate()
        cmbSectionFilter.Items.Clear()
        cmbSectionFilter.Items.Add("All sections")
        For Each sectionName As String In sortedSections
            cmbSectionFilter.Items.Add(sectionName)
        Next
        cmbSectionFilter.SelectedIndex = 0
        cmbSectionFilter.EndUpdate()
    End Sub

    Private Sub RebuildGrid()
        If _document Is Nothing Then
            Return
        End If

        Dim searchText As String = If(txtFilter.Text, "").Trim().ToLowerInvariant()
        Dim knownOnly As Boolean = chkKnownOnly.Checked
        Dim selectedSection As String = If(TryCast(cmbSectionFilter.SelectedItem, String), "All sections")
        If String.IsNullOrWhiteSpace(selectedSection) Then
            selectedSection = "All sections"
        End If

        Dim currentValues As Dictionary(Of String, String) = BuildValueMap(_document)
        Dim pending As New Dictionary(Of String, String)(currentValues, StringComparer.OrdinalIgnoreCase)

        Dim rows As New List(Of IniGridRowModel)()

        For Each setting As IniSettingDefinition In _knownSettings.OrderBy(Function(item) item.Section, StringComparer.OrdinalIgnoreCase).
                                                            ThenBy(Function(item) item.Key, StringComparer.OrdinalIgnoreCase)
            Dim mapKey As String = BuildMapKey(setting.Section, setting.Key)
            Dim value As String = ""
            If pending.TryGetValue(mapKey, value) Then
                pending.Remove(mapKey)
            End If

            rows.Add(New IniGridRowModel With {
                .Section = setting.Section,
                .Key = setting.Key,
                .Value = value,
                .Known = True,
                .Description = setting.Description,
                .AllowedValues = setting.AllowedValues,
                .DefaultValue = setting.DefaultValue,
                .Source = setting.Source
            })
        Next

        For Each item As KeyValuePair(Of String, String) In pending.OrderBy(Function(entry) entry.Key, StringComparer.OrdinalIgnoreCase)
            Dim splitIndex As Integer = item.Key.IndexOf("|"c)
            If splitIndex <= 0 Then
                Continue For
            End If

            Dim section As String = item.Key.Substring(0, splitIndex)
            Dim key As String = item.Key.Substring(splitIndex + 1)
            rows.Add(New IniGridRowModel With {
                .Section = section,
                .Key = key,
                .Value = item.Value,
                .Known = False,
                .Description = "Custom key from current INI file.",
                .AllowedValues = "",
                .DefaultValue = "",
                .Source = "Current INI"
            })
        Next

        Dim filteredRows As New List(Of IniGridRowModel)()
        For Each rowData As IniGridRowModel In rows
            If knownOnly AndAlso Not rowData.Known Then
                Continue For
            End If

            If Not selectedSection.Equals("All sections", StringComparison.OrdinalIgnoreCase) AndAlso
               Not String.Equals(rowData.Section, selectedSection, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            If Not String.IsNullOrWhiteSpace(searchText) Then
                Dim searchable As String = $"{rowData.Section} {rowData.Key} {rowData.Description} {rowData.Value} {rowData.AllowedValues} {rowData.DefaultValue}".ToLowerInvariant()
                If Not searchable.Contains(searchText) Then
                    Continue For
                End If
            End If

            filteredRows.Add(rowData)
        Next

        Dim finalRows As New List(Of IniGridRowModel)()
        Dim showHeaders As Boolean = selectedSection.Equals("All sections", StringComparison.OrdinalIgnoreCase)
        If showHeaders Then
            For Each sectionGroup In filteredRows.
                GroupBy(Function(row) row.Section, StringComparer.OrdinalIgnoreCase).
                OrderBy(Function(group) group.Key, StringComparer.OrdinalIgnoreCase)
                Dim isCollapsed As Boolean = _collapsedSections.Contains(sectionGroup.Key)

                finalRows.Add(New IniGridRowModel With {
                    .Section = sectionGroup.Key,
                    .Key = "",
                    .Value = "",
                    .Known = True,
                    .Description = "",
                    .AllowedValues = "",
                    .DefaultValue = "",
                    .Source = "",
                    .IsSectionHeader = True,
                    .DisplaySection = $"{If(isCollapsed, "▶", "▼")} [{sectionGroup.Key}] ({sectionGroup.Count()} setting(s))"
                })

                If Not isCollapsed Then
                    finalRows.AddRange(sectionGroup.OrderBy(Function(row) row.Key, StringComparer.OrdinalIgnoreCase))
                End If
            Next
        Else
            finalRows.AddRange(filteredRows.
                OrderBy(Function(row) row.Section, StringComparer.OrdinalIgnoreCase).
                ThenBy(Function(row) row.Key, StringComparer.OrdinalIgnoreCase))
        End If

        dgvSettings.SuspendLayout()
        dgvSettings.Rows.Clear()
        Dim suppressSectionNamesInRows As Boolean = showHeaders OrElse Not selectedSection.Equals("All sections", StringComparison.OrdinalIgnoreCase)
        For Each rowData As IniGridRowModel In finalRows
            Dim sectionCellText As String = rowData.Section
            If suppressSectionNamesInRows AndAlso Not rowData.IsSectionHeader Then
                sectionCellText = ""
            End If

            Dim rowIndex As Integer = dgvSettings.Rows.Add(If(rowData.IsSectionHeader, rowData.DisplaySection, sectionCellText),
                                                           rowData.Key,
                                                           rowData.Value,
                                                           If(rowData.IsSectionHeader, "Section", If(rowData.Known, "Known", "Custom")),
                                                           "",
                                                           BuildDescriptionSummary(rowData.Description))
            Dim row As DataGridViewRow = dgvSettings.Rows(rowIndex)
            row.Tag = rowData
            UpdateRowState(row)
        Next

        If dgvSettings.Rows.Count > 0 Then
            Dim selected As Boolean = False
            For Each row As DataGridViewRow In dgvSettings.Rows
                Dim model As IniGridRowModel = TryCast(row.Tag, IniGridRowModel)
                If model IsNot Nothing AndAlso Not model.IsSectionHeader Then
                    row.Selected = True
                    dgvSettings.CurrentCell = row.Cells(colValue.Index)
                    selected = True
                    Exit For
                End If
            Next

            If Not selected Then
                dgvSettings.ClearSelection()
                dgvSettings.Rows(0).Selected = True
            End If
        End If

        dgvSettings.ResumeLayout()
        UpdateSelectionDetails()
    End Sub

    Private Sub ApplyGridTheme()
        dgvSettings.EnableHeadersVisualStyles = False
        dgvSettings.BorderStyle = BorderStyle.None
        dgvSettings.GridColor = If(_isDarkTheme, Color.FromArgb(68, 68, 68), Color.FromArgb(200, 200, 200))

        If _isDarkTheme Then
            dgvSettings.BackgroundColor = Color.FromArgb(32, 32, 32)
            dgvSettings.DefaultCellStyle.BackColor = Color.FromArgb(32, 32, 32)
            dgvSettings.DefaultCellStyle.ForeColor = Color.Gainsboro
            dgvSettings.DefaultCellStyle.SelectionBackColor = Color.FromArgb(64, 96, 160)
            dgvSettings.DefaultCellStyle.SelectionForeColor = Color.White
            dgvSettings.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(38, 38, 38)
            dgvSettings.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro
            dgvSettings.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(38, 38, 38)
            dgvSettings.RowHeadersDefaultCellStyle.ForeColor = Color.Gainsboro
            txtSelectedDescription.BackColor = Color.FromArgb(32, 32, 32)
            txtSelectedDescription.ForeColor = Color.Gainsboro
        Else
            dgvSettings.BackgroundColor = SystemColors.Window
            dgvSettings.DefaultCellStyle.BackColor = SystemColors.Window
            dgvSettings.DefaultCellStyle.ForeColor = SystemColors.ControlText
            dgvSettings.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight
            dgvSettings.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText
            dgvSettings.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control
            dgvSettings.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText
            dgvSettings.RowHeadersDefaultCellStyle.BackColor = SystemColors.Control
            dgvSettings.RowHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText
            txtSelectedDescription.BackColor = SystemColors.Window
            txtSelectedDescription.ForeColor = SystemColors.WindowText
        End If
    End Sub

    Private Sub LockGridSorting()
        If dgvSettings Is Nothing Then
            Return
        End If

        For Each col As DataGridViewColumn In dgvSettings.Columns
            col.SortMode = DataGridViewColumnSortMode.NotSortable
        Next
    End Sub

    Private Function BuildValueMap(document As IniDocument) As Dictionary(Of String, String)
        Dim map As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        If document Is Nothing Then
            Return map
        End If

        For Each item As IniUpdate In document.GetEffectiveValues()
            map(BuildMapKey(item.Section, item.Key)) = If(item.Value, "")
        Next
        Return map
    End Function

    Private Sub UpdateRowState(row As DataGridViewRow)
        If row Is Nothing Then
            Return
        End If

        Dim model As IniGridRowModel = TryCast(row.Tag, IniGridRowModel)
        If model Is Nothing Then
            Return
        End If

        If model.IsSectionHeader Then
            row.ReadOnly = True
            row.DefaultCellStyle.BackColor = If(_isDarkTheme, Color.FromArgb(42, 42, 42), Color.FromArgb(232, 232, 232))
            row.DefaultCellStyle.ForeColor = If(_isDarkTheme, Color.Gainsboro, Color.Black)
            row.DefaultCellStyle.Font = New Font(dgvSettings.Font, FontStyle.Bold)
            row.Cells(colValidation.Index).Value = ""
            row.Cells(colDescription.Index).Value = "Click to expand/collapse section."
            Return
        End If

        Dim currentValue As String = If(model.Value, "")
        Dim mapKey As String = BuildMapKey(model.Section, model.Key)
        Dim originalValue As String = ""
        Dim hadOriginal As Boolean = _originalValues.TryGetValue(mapKey, originalValue)
        Dim isChanged As Boolean = (hadOriginal AndAlso Not String.Equals(originalValue, currentValue, StringComparison.Ordinal)) OrElse
                                   (Not hadOriginal AndAlso currentValue.Length > 0)

        If isChanged Then
            row.DefaultCellStyle.BackColor = If(_isDarkTheme, Color.FromArgb(45, 60, 45), Color.FromArgb(236, 252, 236))
        Else
            row.DefaultCellStyle.BackColor = If(_isDarkTheme, Color.FromArgb(32, 32, 32), SystemColors.Window)
        End If

        Dim validation As IniValidationResult = OptiScalerIniEditorService.Validate(model.Section, model.Key, currentValue)
        row.Cells(colValidation.Index).Value = If(String.IsNullOrWhiteSpace(validation.Message), "OK", validation.Message)
        Select Case validation.Severity
            Case IniValidationSeverity.ErrorLevel
                row.Cells(colValidation.Index).Style.ForeColor = Color.IndianRed
            Case IniValidationSeverity.Warning
                row.Cells(colValidation.Index).Style.ForeColor = If(_isDarkTheme, Color.Gold, Color.DarkOrange)
            Case Else
                row.Cells(colValidation.Index).Style.ForeColor = If(_isDarkTheme, Color.FromArgb(150, 150, 150), Color.FromArgb(80, 80, 80))
        End Select

        row.DefaultCellStyle.Font = dgvSettings.Font
    End Sub

    Private Function BuildMapKey(section As String, key As String) As String
        Return $"{If(section, "").Trim()}|{If(key, "").Trim()}"
    End Function

    Private Function BuildDescriptionSummary(description As String) As String
        If String.IsNullOrWhiteSpace(description) Then
            Return ""
        End If

        Dim firstLine As String = description.
            Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).
            Select(Function(line) line.Trim()).
            FirstOrDefault()
        Return If(firstLine, "").Trim()
    End Function

    Private Sub UpdateSelectionDetails()
        Dim selectedModel As IniGridRowModel = Nothing
        If dgvSettings IsNot Nothing AndAlso dgvSettings.SelectedRows.Count > 0 Then
            selectedModel = TryCast(dgvSettings.SelectedRows(0).Tag, IniGridRowModel)
        End If

        If selectedModel Is Nothing Then
            lblSelectedSectionValue.Text = "-"
            lblSelectedKeyValue.Text = "-"
            lblSelectedValueValue.Text = "-"
            lblSelectedDefaultValue.Text = "-"
            lblSelectedAllowedValue.Text = "-"
            lblSelectedSourceValue.Text = "-"
            txtSelectedDescription.Text = "Select a setting row to see detailed help."
            Return
        End If

        If selectedModel.IsSectionHeader Then
            lblSelectedSectionValue.Text = selectedModel.Section
            lblSelectedKeyValue.Text = "(section)"
            lblSelectedValueValue.Text = "-"
            lblSelectedDefaultValue.Text = "-"
            lblSelectedAllowedValue.Text = "-"
            lblSelectedSourceValue.Text = "-"
            txtSelectedDescription.Text = "Section header row. Select a key row for details."
            Return
        End If

        lblSelectedSectionValue.Text = If(selectedModel.Section, "")
        lblSelectedKeyValue.Text = If(selectedModel.Key, "")
        lblSelectedValueValue.Text = If(selectedModel.Value, "")
        lblSelectedDefaultValue.Text = If(String.IsNullOrWhiteSpace(selectedModel.DefaultValue), "-", selectedModel.DefaultValue)
        lblSelectedAllowedValue.Text = If(String.IsNullOrWhiteSpace(selectedModel.AllowedValues), "-", selectedModel.AllowedValues)
        lblSelectedSourceValue.Text = If(String.IsNullOrWhiteSpace(selectedModel.Source), "-", selectedModel.Source)
        txtSelectedDescription.Text = If(String.IsNullOrWhiteSpace(selectedModel.Description),
                                         "No description available for this setting.",
                                         selectedModel.Description)
    End Sub

    Private Sub SetDirty(value As Boolean)
        _isDirty = value
        Dim suffix As String = If(_isDirty, " *", "")
        Text = "OptiScaler.ini Editor" & suffix
        btnSave.Enabled = _isDirty
        btnRevert.Enabled = _isDirty
    End Sub

    Private Function IsCurrentStateDirty() As Boolean
        If tabModes.SelectedTab Is tabRaw Then
            Return Not String.Equals(txtRaw.Text, _originalText, StringComparison.Ordinal)
        End If

        Return Not String.Equals(_document.ToText(), _originalText, StringComparison.Ordinal)
    End Function

    Private Function TryApplyRawToDocument() As Boolean
        Try
            _document = IniDocument.Parse(txtRaw.Text)
            Return True
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmIniEditor.ApplyRaw")
            MessageBox.Show(Me, "Raw INI text could not be parsed: " & ex.Message, "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Function SaveCurrent() As Boolean
        Try
            If tabModes.SelectedTab Is tabRaw Then
                If Not TryApplyRawToDocument() Then
                    Return False
                End If
            End If

            Dim currentText As String = _document.ToText()
            Dim backupPath As String = ""
            If File.Exists(_iniPath) Then
                backupPath = OptiScalerIniEditorService.CreateTimestampBackup(_iniPath)
            End If

            OptiScalerIniEditorService.SaveTextAtomically(_iniPath, currentText)
            _originalText = currentText
            _originalValues = BuildValueMap(_document)
            WasSaved = True
            SetDirty(False)
            RebuildGrid()
            txtRaw.Text = currentText

            If Not String.IsNullOrWhiteSpace(backupPath) Then
                MessageBox.Show(Me, "INI saved successfully." & Environment.NewLine & "Backup: " & backupPath, "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show(Me, "INI saved successfully.", "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            Return True
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmIniEditor.Save")
            MessageBox.Show(Me, "Failed to save INI file: " & ex.Message, "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Sub btnReload_Click(sender As Object, e As EventArgs) Handles btnReload.Click
        If _isDirty Then
            Dim result As DialogResult = MessageBox.Show(Me, "Discard unsaved changes and reload from disk?", "INI Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result <> DialogResult.Yes Then
                Return
            End If
        End If

        LoadDocumentFromDisk()
    End Sub

    Private Sub btnOpenFolder_Click(sender As Object, e As EventArgs) Handles btnOpenFolder.Click
        Try
            Dim folder As String = Path.GetDirectoryName(_iniPath)
            If String.IsNullOrWhiteSpace(folder) OrElse Not Directory.Exists(folder) Then
                Return
            End If

            Process.Start(New ProcessStartInfo(folder) With {.UseShellExecute = True})
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmIniEditor.OpenFolder")
        End Try
    End Sub

    Private Sub btnCreateBackup_Click(sender As Object, e As EventArgs) Handles btnCreateBackup.Click
        Try
            Dim backupPath As String = OptiScalerIniEditorService.CreateTimestampBackup(_iniPath)
            If String.IsNullOrWhiteSpace(backupPath) Then
                MessageBox.Show(Me, "INI file does not exist yet. Save first, then create a backup.", "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            MessageBox.Show(Me, "Backup created at:" & Environment.NewLine & backupPath, "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmIniEditor.CreateBackup")
            MessageBox.Show(Me, "Failed to create backup: " & ex.Message, "INI Editor", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtFilter_TextChanged(sender As Object, e As EventArgs) Handles txtFilter.TextChanged
        If _ignoreEvents Then
            Return
        End If
        RebuildGrid()
    End Sub

    Private Sub chkKnownOnly_CheckedChanged(sender As Object, e As EventArgs) Handles chkKnownOnly.CheckedChanged
        If _ignoreEvents Then
            Return
        End If
        RebuildGrid()
    End Sub

    Private Sub cmbSectionFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSectionFilter.SelectedIndexChanged
        If _ignoreEvents Then
            Return
        End If
        If cmbSectionFilter.SelectedIndex > 0 Then
            _collapsedSections.Clear()
        End If
        RebuildGrid()
    End Sub

    Private Sub dgvSettings_SelectionChanged(sender As Object, e As EventArgs) Handles dgvSettings.SelectionChanged
        If _ignoreEvents Then
            Return
        End If
        UpdateSelectionDetails()
    End Sub

    Private Sub dgvSettings_CellBeginEdit(sender As Object, e As DataGridViewCellCancelEventArgs) Handles dgvSettings.CellBeginEdit
        If e.RowIndex < 0 Then
            Return
        End If

        Dim row As DataGridViewRow = dgvSettings.Rows(e.RowIndex)
        Dim model As IniGridRowModel = TryCast(row.Tag, IniGridRowModel)
        If model IsNot Nothing AndAlso model.IsSectionHeader Then
            e.Cancel = True
            Return
        End If

        If e.ColumnIndex <> colValue.Index Then
            e.Cancel = True
        End If
    End Sub

    Private Sub dgvSettings_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvSettings.CellClick
        If _ignoreEvents OrElse e.RowIndex < 0 Then
            Return
        End If

        If ToggleSectionFromRowIndex(e.RowIndex) Then
            Return
        End If
    End Sub

    Private Sub dgvSettings_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvSettings.CellDoubleClick
        If _ignoreEvents OrElse e.RowIndex < 0 Then
            Return
        End If

        If ToggleSectionFromRowIndex(e.RowIndex) Then
            Return
        End If
    End Sub

    Private Function ToggleSectionFromRowIndex(rowIndex As Integer) As Boolean
        If rowIndex < 0 OrElse rowIndex >= dgvSettings.Rows.Count Then
            Return False
        End If

        Dim row As DataGridViewRow = dgvSettings.Rows(rowIndex)
        Dim model As IniGridRowModel = TryCast(row.Tag, IniGridRowModel)
        If model Is Nothing OrElse Not model.IsSectionHeader Then
            Return False
        End If

        Dim sectionName As String = If(model.Section, "").Trim()
        If String.IsNullOrWhiteSpace(sectionName) Then
            Return False
        End If

        If _collapsedSections.Contains(sectionName) Then
            _collapsedSections.Remove(sectionName)
        Else
            _collapsedSections.Add(sectionName)
        End If

        _ignoreEvents = True
        RebuildGrid()
        _ignoreEvents = False
        Return True
    End Function

    Private Sub dgvSettings_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvSettings.CellEndEdit
        If _ignoreEvents OrElse e.RowIndex < 0 Then
            Return
        End If

        If e.ColumnIndex <> colValue.Index Then
            Return
        End If

        Dim row As DataGridViewRow = dgvSettings.Rows(e.RowIndex)
        Dim model As IniGridRowModel = TryCast(row.Tag, IniGridRowModel)
        If model Is Nothing OrElse model.IsSectionHeader Then
            Return
        End If

        Dim updatedValue As String = Convert.ToString(row.Cells(colValue.Index).Value)
        model.Value = If(updatedValue, "")
        row.Cells(colValue.Index).Value = model.Value

        _document.SetValue(model.Section, model.Key, model.Value)
        UpdateRowState(row)
        SetDirty(IsCurrentStateDirty())
        UpdateSelectionDetails()
    End Sub

    Private Sub tabModes_SelectedIndexChanged(sender As Object, e As EventArgs) Handles tabModes.SelectedIndexChanged
        If _ignoreEvents Then
            Return
        End If

        If tabModes.SelectedTab Is tabRaw Then
            _ignoreEvents = True
            txtRaw.Text = _document.ToText()
            _ignoreEvents = False
            SetDirty(IsCurrentStateDirty())
            Return
        End If

        If tabModes.SelectedTab Is tabVisual Then
            If Not TryApplyRawToDocument() Then
                _ignoreEvents = True
                tabModes.SelectedTab = tabRaw
                _ignoreEvents = False
                Return
            End If

            RebuildGrid()
            SetDirty(IsCurrentStateDirty())
        End If
    End Sub

    Private Sub txtRaw_TextChanged(sender As Object, e As EventArgs) Handles txtRaw.TextChanged
        If _ignoreEvents OrElse tabModes.SelectedTab IsNot tabRaw Then
            Return
        End If

        SetDirty(IsCurrentStateDirty())
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        SaveCurrent()
    End Sub

    Private Sub btnRevert_Click(sender As Object, e As EventArgs) Handles btnRevert.Click
        If Not _isDirty Then
            Return
        End If

        Dim result As DialogResult = MessageBox.Show(Me, "Discard unsaved changes and revert to last saved state?", "INI Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result <> DialogResult.Yes Then
            Return
        End If

        _ignoreEvents = True
        _document = IniDocument.Parse(_originalText)
        RebuildGrid()
        txtRaw.Text = _originalText
        _ignoreEvents = False
        SetDirty(False)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Private Sub frmIniEditor_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not _isDirty Then
            Return
        End If

        Dim result As DialogResult = MessageBox.Show(Me,
                                                     "Save changes to OptiScaler.ini before closing?",
                                                     "INI Editor",
                                                     MessageBoxButtons.YesNoCancel,
                                                     MessageBoxIcon.Question)
        If result = DialogResult.Cancel Then
            e.Cancel = True
            Return
        End If

        If result = DialogResult.Yes Then
            e.Cancel = Not SaveCurrent()
        End If
    End Sub
End Class
