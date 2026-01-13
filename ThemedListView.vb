Option Strict On
Option Explicit On

Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

<DefaultEvent("SelectedIndexChanged")>
Friend Class ThemedListView
    Inherits ListView

    Private _headerBackColor As Color = SystemColors.Control
    Private _headerForeColor As Color = SystemColors.ControlText
    Private _rowBackColor As Color = SystemColors.Window
    Private _rowAltBackColor As Color = SystemColors.Window
    Private _rowForeColor As Color = SystemColors.WindowText
    Private _selectedBackColor As Color = SystemColors.Highlight
    Private _selectedForeColor As Color = SystemColors.HighlightText
    Private _gridLineColor As Color = SystemColors.ControlDark
    Private _showGridLines As Boolean = True

    Public Sub New()
        OwnerDraw = True
        DoubleBuffered = True
        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
    End Sub

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property HeaderBackColor As Color
        Get
            Return _headerBackColor
        End Get
        Set(value As Color)
            _headerBackColor = value
            Invalidate()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property HeaderForeColor As Color
        Get
            Return _headerForeColor
        End Get
        Set(value As Color)
            _headerForeColor = value
            Invalidate()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property RowBackColor As Color
        Get
            Return _rowBackColor
        End Get
        Set(value As Color)
            _rowBackColor = value
            Invalidate()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property RowAltBackColor As Color
        Get
            Return _rowAltBackColor
        End Get
        Set(value As Color)
            _rowAltBackColor = value
            Invalidate()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property RowForeColor As Color
        Get
            Return _rowForeColor
        End Get
        Set(value As Color)
            _rowForeColor = value
            Invalidate()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedBackColor As Color
        Get
            Return _selectedBackColor
        End Get
        Set(value As Color)
            _selectedBackColor = value
            Invalidate()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedForeColor As Color
        Get
            Return _selectedForeColor
        End Get
        Set(value As Color)
            _selectedForeColor = value
            Invalidate()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property GridLineColor As Color
        Get
            Return _gridLineColor
        End Get
        Set(value As Color)
            _gridLineColor = value
            Invalidate()
        End Set
    End Property

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ShowGridLines As Boolean
        Get
            Return _showGridLines
        End Get
        Set(value As Boolean)
            _showGridLines = value
            Invalidate()
        End Set
    End Property

    Protected Overrides Sub OnDrawColumnHeader(e As DrawListViewColumnHeaderEventArgs)
        Using backBrush As New SolidBrush(_headerBackColor)
            e.Graphics.FillRectangle(backBrush, e.Bounds)
        End Using

        Dim textFlags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis
        Select Case e.Header.TextAlign
            Case HorizontalAlignment.Center
                textFlags = textFlags Or TextFormatFlags.HorizontalCenter
            Case HorizontalAlignment.Right
                textFlags = textFlags Or TextFormatFlags.Right
            Case Else
                textFlags = textFlags Or TextFormatFlags.Left
        End Select

        Dim textBounds As Rectangle = e.Bounds
        textBounds.Inflate(-4, 0)
        TextRenderer.DrawText(e.Graphics, e.Header.Text, Font, textBounds, _headerForeColor, textFlags)

        If _showGridLines Then
            Using pen As New Pen(_gridLineColor)
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom)
            End Using
        End If
    End Sub

    Protected Overrides Sub OnDrawItem(e As DrawListViewItemEventArgs)
        If View <> View.Details Then
            Dim backColor As Color = If(e.Item.Selected, _selectedBackColor, ResolveItemBackColor(e.Item, e.ItemIndex))
            Dim foreColor As Color = If(e.Item.Selected, _selectedForeColor, ResolveItemForeColor(e.Item))
            Using backBrush As New SolidBrush(backColor)
                e.Graphics.FillRectangle(backBrush, e.Bounds)
            End Using
            TextRenderer.DrawText(e.Graphics, e.Item.Text, Font, e.Bounds, foreColor, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter)
            Return
        End If

        If e.Item.Focused AndAlso e.Item.Selected Then
            Dim rowBounds As New Rectangle(0, e.Bounds.Top, ClientSize.Width, e.Bounds.Height)
            ControlPaint.DrawFocusRectangle(e.Graphics, rowBounds, _selectedForeColor, _selectedBackColor)
        End If
    End Sub

    Protected Overrides Sub OnDrawSubItem(e As DrawListViewSubItemEventArgs)
        Dim rowIndex As Integer = e.ItemIndex
        Dim isSelected As Boolean = e.Item.Selected
        Dim backColor As Color = If(isSelected, _selectedBackColor, ResolveItemBackColor(e.Item, rowIndex))
        Dim foreColor As Color = If(isSelected, _selectedForeColor, ResolveItemForeColor(e.Item))

        Using backBrush As New SolidBrush(backColor)
            e.Graphics.FillRectangle(backBrush, e.Bounds)
        End Using

        Dim textBounds As Rectangle = e.Bounds
        textBounds.Inflate(-4, 0)
        TextRenderer.DrawText(e.Graphics, e.SubItem.Text, Font, textBounds, foreColor, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)

        If _showGridLines Then
            Using pen As New Pen(_gridLineColor)
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
                e.Graphics.DrawLine(pen, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom)
            End Using
        End If
    End Sub

    Private Function ResolveItemBackColor(item As ListViewItem, rowIndex As Integer) As Color
        If item IsNot Nothing AndAlso Not item.BackColor.IsEmpty AndAlso item.BackColor <> Color.Transparent Then
            Return item.BackColor
        End If
        Return If(rowIndex Mod 2 = 0, _rowBackColor, _rowAltBackColor)
    End Function

    Private Function ResolveItemForeColor(item As ListViewItem) As Color
        If item IsNot Nothing AndAlso Not item.ForeColor.IsEmpty AndAlso item.ForeColor <> Color.Transparent Then
            Return item.ForeColor
        End If
        Return _rowForeColor
    End Function
End Class
