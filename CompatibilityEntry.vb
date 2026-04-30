Public Class CompatibilityEntry
    ' Represents a compatibility entry (display name, optional wiki slug, and optional aliases).
    Public Property Name As String
    Public Property Slug As String
    Public Property Aliases As List(Of String)

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
