Public Class CompatibilityParseResult
    Public Property Entries As List(Of CompatibilityEntry) = New List(Of CompatibilityEntry)()
    Public Property SourceFormat As String = ""
    Public Property ExpectedCount As Integer?
    Public Property Warnings As List(Of String) = New List(Of String)()
    Public Property SkippedRows As Integer
End Class
