Public Class CompatibilityEntry
    Public Property Name As String
    Public Property Slug As String

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
