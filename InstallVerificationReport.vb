Public Class InstallVerificationReport
    ' Post-install health report used for logging and user-facing summaries.
    Public Sub New()
        Passed = New List(Of String)()
        Warnings = New List(Of String)()
        Errors = New List(Of String)()
    End Sub

    Public Property Passed As List(Of String)
    Public Property Warnings As List(Of String)
    Public Property Errors As List(Of String)

    Public ReadOnly Property IsSuccess As Boolean
        Get
            Return Errors.Count = 0
        End Get
    End Property
End Class
