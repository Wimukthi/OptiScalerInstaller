Option Strict On
Option Explicit On

Imports System.IO
Imports System.Linq

Friend Partial Class frmDriveSelection
    Inherits Form

    Private ReadOnly _driveRoots As List(Of String)

    Private Class DriveChoice
        Public Property Root As String
        Public Property Display As String

        Public Overrides Function ToString() As String
            Return Display
        End Function
    End Class

    Public Sub New(driveRoots As IEnumerable(Of String))
        InitializeComponent()
        _driveRoots = If(driveRoots, Enumerable.Empty(Of String)()).
            Where(Function(path) Not String.IsNullOrWhiteSpace(path)).
            Select(Function(path) path.Trim()).
            Distinct(StringComparer.OrdinalIgnoreCase).
            OrderBy(Function(path) path, StringComparer.OrdinalIgnoreCase).
            ToList()

        Try
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmDriveSelection.LoadIcon")
        End Try

        ThemeManager.ApplyTheme(Me, ThemeSettings.GetPreferredColorMode())
        PopulateDrives()
    End Sub

    Public Function GetSelectedDriveRoots() As List(Of String)
        Dim selected As New List(Of String)()
        For Each item As ListViewItem In lvDrives.Items
            If item Is Nothing OrElse Not item.Checked Then
                Continue For
            End If

            Dim choice As DriveChoice = TryCast(item.Tag, DriveChoice)
            If choice Is Nothing Then
                Continue For
            End If

            selected.Add(choice.Root)
        Next

        selected.Sort(StringComparer.OrdinalIgnoreCase)
        Return selected
    End Function

    Private Sub PopulateDrives()
        lvDrives.BeginUpdate()
        lvDrives.Items.Clear()

        For Each root As String In _driveRoots
            Dim display As String = BuildDriveDisplay(root)
            Dim choice As New DriveChoice With {
                .Root = root,
                .Display = display
            }

            Dim item As New ListViewItem(display) With {
                .Tag = choice,
                .Checked = True
            }
            lvDrives.Items.Add(item)
        Next

        lvDrives.EndUpdate()
        UpdateDriveColumnWidth()
        btnScan.Enabled = lvDrives.Items.Count > 0
    End Sub

    Private Function BuildDriveDisplay(root As String) As String
        Dim normalizedRoot As String = root
        Try
            normalizedRoot = Path.GetPathRoot(root)
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmDriveSelection.BuildDriveDisplay.GetPathRoot")
        End Try

        Try
            Dim drive As New DriveInfo(normalizedRoot)
            If drive.IsReady Then
                Dim freeText As String = FormatSize(drive.AvailableFreeSpace)
                Dim totalText As String = FormatSize(drive.TotalSize)
                Return $"{drive.RootDirectory.FullName} ({drive.DriveType}, {freeText} free of {totalText})"
            End If

            Return $"{normalizedRoot} ({drive.DriveType}, not ready)"
        Catch ex As Exception
            ErrorLogger.Log(ex, "frmDriveSelection.BuildDriveDisplay")
            Return normalizedRoot
        End Try
    End Function

    Private Function FormatSize(value As Long) As String
        Dim units As String() = {"B", "KB", "MB", "GB", "TB"}
        Dim size As Double = value
        Dim index As Integer = 0

        While size >= 1024.0 AndAlso index < units.Length - 1
            size /= 1024.0
            index += 1
        End While

        If index = 0 Then
            Return size.ToString("0", Globalization.CultureInfo.InvariantCulture) & " " & units(index)
        End If

        Return size.ToString("0.0", Globalization.CultureInfo.InvariantCulture) & " " & units(index)
    End Function

    Private Sub btnSelectAll_Click(sender As Object, e As EventArgs) Handles btnSelectAll.Click
        For Each item As ListViewItem In lvDrives.Items
            item.Checked = True
        Next
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        For Each item As ListViewItem In lvDrives.Items
            item.Checked = False
        Next
    End Sub

    Private Sub btnScan_Click(sender As Object, e As EventArgs) Handles btnScan.Click
        Dim hasChecked As Boolean = lvDrives.Items.Cast(Of ListViewItem)().Any(Function(item) item.Checked)
        If Not hasChecked Then
            MessageBox.Show(Me, "Select at least one drive.", "Deep Scan", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub frmDriveSelection_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        UpdateDriveColumnWidth()
    End Sub

    Private Sub UpdateDriveColumnWidth()
        If lvDrives Is Nothing OrElse lvDrives.Columns.Count = 0 Then
            Return
        End If

        Dim width As Integer = lvDrives.ClientSize.Width - 8
        If width < 160 Then
            width = 160
        End If
        lvDrives.Columns(0).Width = width
    End Sub
End Class
