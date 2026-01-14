Public Class CompatibilityEntry
    ' Represents a wiki compatibility entry (display name + wiki slug).
    Public Property Name As String
    Public Property Slug As String

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
