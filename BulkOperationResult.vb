Option Strict On
Option Explicit On

Friend Class BulkOperationResult
    Public Property Plan As BulkGameOperationPlan
    Public Property Success As Boolean
    Public Property Skipped As Boolean
    Public Property Message As String
End Class
