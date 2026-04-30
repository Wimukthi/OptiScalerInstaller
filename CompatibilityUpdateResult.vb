Public Class CompatibilityUpdateResult
    ' Result payload for compatibility list refresh operations.
    Public Property Entries As List(Of CompatibilityEntry)
    Public Property AddedNames As List(Of String)
    Public Property RemovedNames As List(Of String)
    Public Property ChangedNames As List(Of String)
    Public Property SourceFormat As String
    Public Property ExpectedCount As Integer?
    Public Property Warnings As List(Of String)
End Class
