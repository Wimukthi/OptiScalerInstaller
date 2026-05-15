Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Drawing
Imports System.Linq

Friend Partial Class frmBulkOperations
    Inherits Form

    Private Class OperationChoice
        Public Property Kind As BulkOperationKind
        Public Property Text As String

        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    Private ReadOnly _plans As List(Of BulkGameOperationPlan)
    Private ReadOnly _isDarkTheme As Boolean
    Private _updatingGrid As Boolean

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedOperation As BulkOperationKind

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedPlans As List(Of BulkGameOperationPlan) = New List(Of BulkGameOperationPlan)()

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property StopOnFirstError As Boolean
        Get
            Return chkStopOnError.Checked
        End Get
    End Property

    Public Sub New(plans As IEnumerable(Of BulkGameOperationPlan))
        InitializeComponent()
        _plans = If(plans, Enumerable.Empty(Of BulkGameOperationPlan)()).
            Where(Function(plan) plan IsNot Nothing).
            OrderBy(Function(plan) plan.GameName, StringComparer.OrdinalIgnoreCase).
            ToList()
        _isDarkTheme = ThemeSettings.GetPreferredColorMode() = SystemColorMode.Dark

        Try
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmBulkOperations.LoadIcon")
        End Try

        ThemeManager.ApplyTheme(Me, ThemeSettings.GetPreferredColorMode())
        ApplyGridTheme()
        InitializeOperations()
        PopulateGrid()
    End Sub

    Private Sub InitializeOperations()
        cmbOperation.Items.Clear()
        cmbOperation.Items.Add(New OperationChoice With {.Kind = BulkOperationKind.QuickInstallOptiScaler, .Text = "Quick install missing OptiScaler"})
        cmbOperation.Items.Add(New OperationChoice With {.Kind = BulkOperationKind.UpdateOptiScaler, .Text = "Update installed OptiScaler"})
        cmbOperation.Items.Add(New OperationChoice With {.Kind = BulkOperationKind.InstallOrUpdateOptiPatcher, .Text = "Install/update OptiPatcher"})
        cmbOperation.SelectedIndex = 0
    End Sub

    Private Sub ApplyGridTheme()
        dgvPlans.EnableHeadersVisualStyles = False
        dgvPlans.BorderStyle = BorderStyle.FixedSingle
        dgvPlans.GridColor = If(_isDarkTheme, Color.FromArgb(48, 48, 48), SystemColors.ControlLight)
        dgvPlans.BackgroundColor = If(_isDarkTheme, Color.FromArgb(32, 32, 32), SystemColors.Window)
        dgvPlans.DefaultCellStyle.BackColor = If(_isDarkTheme, Color.FromArgb(32, 32, 32), SystemColors.Window)
        dgvPlans.DefaultCellStyle.ForeColor = If(_isDarkTheme, Color.Gainsboro, SystemColors.ControlText)
        dgvPlans.DefaultCellStyle.SelectionBackColor = If(_isDarkTheme, Color.FromArgb(68, 82, 124), SystemColors.Highlight)
        dgvPlans.DefaultCellStyle.SelectionForeColor = If(_isDarkTheme, Color.White, SystemColors.HighlightText)
        dgvPlans.ColumnHeadersDefaultCellStyle.BackColor = If(_isDarkTheme, Color.FromArgb(25, 25, 25), SystemColors.Control)
        dgvPlans.ColumnHeadersDefaultCellStyle.ForeColor = If(_isDarkTheme, Color.White, SystemColors.ControlText)
        dgvPlans.RowHeadersVisible = False
        dgvPlans.AllowUserToAddRows = False
        dgvPlans.AllowUserToDeleteRows = False
        dgvPlans.AllowUserToResizeRows = False
    End Sub

    Private Sub cmbOperation_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbOperation.SelectedIndexChanged
        PopulateGrid()
    End Sub

    Private Sub chkIncludeAntiCheat_CheckedChanged(sender As Object, e As EventArgs) Handles chkIncludeAntiCheat.CheckedChanged
        PopulateGrid()
    End Sub

    Private Sub PopulateGrid()
        If dgvPlans Is Nothing OrElse cmbOperation.SelectedItem Is Nothing Then
            Return
        End If

        _updatingGrid = True
        Try
            Dim choice As OperationChoice = DirectCast(cmbOperation.SelectedItem, OperationChoice)
            SelectedOperation = choice.Kind
            dgvPlans.Rows.Clear()

            For Each plan As BulkGameOperationPlan In _plans
                Dim reason As String = ""
                Dim eligible As Boolean = IsPlanEligible(plan, choice.Kind, reason)
                Dim rowIndex As Integer = dgvPlans.Rows.Add(eligible, plan.GameName, GetActionText(choice.Kind, plan), plan.OptiScalerStatus, plan.OptiPatcherStatus, plan.Platform, If(String.IsNullOrWhiteSpace(plan.AntiCheat), "No", plan.AntiCheat), plan.InstallPath, If(eligible, "Ready", reason))
                Dim row As DataGridViewRow = dgvPlans.Rows(rowIndex)
                row.Tag = plan

                If Not eligible Then
                    row.Cells(colBulkSelected.Index).Value = False
                    row.Cells(colBulkSelected.Index).ReadOnly = True
                    row.DefaultCellStyle.ForeColor = If(_isDarkTheme, Color.FromArgb(140, 140, 140), SystemColors.GrayText)
                ElseIf plan.HasAntiCheatWarning Then
                    row.DefaultCellStyle.BackColor = If(_isDarkTheme, Color.FromArgb(55, 41, 28), Color.FromArgb(255, 246, 214))
                    row.DefaultCellStyle.SelectionBackColor = If(_isDarkTheme, Color.FromArgb(92, 65, 35), Color.FromArgb(255, 220, 150))
                End If
            Next
        Finally
            _updatingGrid = False
            UpdateSummary()
        End Try
    End Sub

    Private Function IsPlanEligible(plan As BulkGameOperationPlan, operation As BulkOperationKind, ByRef reason As String) As Boolean
        reason = ""
        If plan Is Nothing Then
            reason = "Missing plan."
            Return False
        End If

        If plan.HasAntiCheatWarning AndAlso Not chkIncludeAntiCheat.Checked Then
            reason = "Anti-cheat flagged. Enable the checkbox to include it."
            Return False
        End If

        Select Case operation
            Case BulkOperationKind.QuickInstallOptiScaler
                If plan.IsOptiScalerInstalled Then
                    reason = "OptiScaler is already installed."
                    Return False
                End If
                If String.IsNullOrWhiteSpace(plan.GameExePath) Then
                    reason = "Game executable could not be resolved. Use the advanced/manual path."
                    Return False
                End If
                Return True

            Case BulkOperationKind.UpdateOptiScaler
                If Not plan.IsOptiScalerInstalled Then
                    reason = "OptiScaler is not installed."
                    Return False
                End If
                If Not plan.IsOptiScalerUpdateAvailable Then
                    reason = "Installed OptiScaler is already current for its source."
                    Return False
                End If
                If String.IsNullOrWhiteSpace(plan.GameExePath) Then
                    reason = "Game executable could not be resolved. Use the advanced/manual path."
                    Return False
                End If
                Return True

            Case BulkOperationKind.InstallOrUpdateOptiPatcher
                If Not plan.IsOptiPatcherSupported Then
                    reason = "OptiPatcher is not listed as supported for this game."
                    Return False
                End If
                If Not plan.IsOptiScalerInstalled Then
                    reason = "OptiScaler must be installed before OptiPatcher."
                    Return False
                End If
                If plan.IsOptiPatcherInstalled AndAlso Not plan.IsOptiPatcherUpdateAvailable Then
                    reason = "OptiPatcher is already current."
                    Return False
                End If
                Return True
        End Select

        reason = "Unknown operation."
        Return False
    End Function

    Private Function GetActionText(operation As BulkOperationKind, plan As BulkGameOperationPlan) As String
        Select Case operation
            Case BulkOperationKind.QuickInstallOptiScaler
                Return "Install OptiScaler"
            Case BulkOperationKind.UpdateOptiScaler
                Return "Update OptiScaler"
            Case BulkOperationKind.InstallOrUpdateOptiPatcher
                If plan IsNot Nothing AndAlso plan.IsOptiPatcherInstalled Then
                    Return "Update OptiPatcher"
                End If
                Return "Install OptiPatcher"
            Case Else
                Return ""
        End Select
    End Function

    Private Sub dgvPlans_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs) Handles dgvPlans.CurrentCellDirtyStateChanged
        If dgvPlans.IsCurrentCellDirty Then
            dgvPlans.CommitEdit(DataGridViewDataErrorContexts.Commit)
        End If
    End Sub

    Private Sub dgvPlans_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dgvPlans.CellValueChanged
        If _updatingGrid OrElse e.RowIndex < 0 OrElse e.ColumnIndex <> colBulkSelected.Index Then
            Return
        End If
        UpdateSummary()
    End Sub

    Private Sub btnSelectEligible_Click(sender As Object, e As EventArgs) Handles btnSelectEligible.Click
        For Each row As DataGridViewRow In dgvPlans.Rows
            If row Is Nothing OrElse row.Cells(colBulkSelected.Index).ReadOnly Then
                Continue For
            End If
            row.Cells(colBulkSelected.Index).Value = True
        Next
        UpdateSummary()
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        For Each row As DataGridViewRow In dgvPlans.Rows
            row.Cells(colBulkSelected.Index).Value = False
        Next
        UpdateSummary()
    End Sub

    Private Sub btnRun_Click(sender As Object, e As EventArgs) Handles btnRun.Click
        SelectedPlans = GetCheckedPlans()
        If SelectedPlans.Count = 0 Then
            MessageBox.Show(Me, "Select at least one eligible game.", "Bulk Operations", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Function GetCheckedPlans() As List(Of BulkGameOperationPlan)
        Dim selected As New List(Of BulkGameOperationPlan)()
        For Each row As DataGridViewRow In dgvPlans.Rows
            If row Is Nothing Then
                Continue For
            End If

            Dim checkedValue As Boolean = False
            If row.Cells(colBulkSelected.Index).Value IsNot Nothing Then
                Boolean.TryParse(row.Cells(colBulkSelected.Index).Value.ToString(), checkedValue)
            End If

            If Not checkedValue Then
                Continue For
            End If

            Dim plan As BulkGameOperationPlan = TryCast(row.Tag, BulkGameOperationPlan)
            If plan IsNot Nothing Then
                selected.Add(plan)
            End If
        Next

        Return selected
    End Function

    Private Sub UpdateSummary()
        If lblSummary Is Nothing Then
            Return
        End If

        Dim eligibleCount As Integer = 0
        Dim selectedCount As Integer = 0
        For Each row As DataGridViewRow In dgvPlans.Rows
            If row Is Nothing Then
                Continue For
            End If
            If Not row.Cells(colBulkSelected.Index).ReadOnly Then
                eligibleCount += 1
            End If
            Dim checkedValue As Boolean = False
            If row.Cells(colBulkSelected.Index).Value IsNot Nothing Then
                Boolean.TryParse(row.Cells(colBulkSelected.Index).Value.ToString(), checkedValue)
            End If
            If checkedValue Then
                selectedCount += 1
            End If
        Next

        lblSummary.Text = $"{eligibleCount} eligible, {selectedCount} selected. Review the Notes column before running."
        btnRun.Enabled = selectedCount > 0
    End Sub
End Class
