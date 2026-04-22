Imports System.Net.Http

Friend Module HttpClientHelper
    ' Shared HttpClient for API calls. A single instance avoids socket exhaustion
    ' that occurs when creating/disposing HttpClient per request.
    Public ReadOnly ApiClient As New HttpClient() With {
        .Timeout = TimeSpan.FromSeconds(30)
    }

    Private Const MaxRetryAttempts As Integer = 3

    Sub New()
        ApiClient.DefaultRequestHeaders.UserAgent.ParseAdd("OptiScalerInstaller")
        ApiClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json")
    End Sub

    ' Retries transient HTTP failures with exponential backoff.
    Public Async Function GetStringWithRetryAsync(url As String, Optional maxAttempts As Integer = MaxRetryAttempts) As Task(Of String)
        Dim delay As TimeSpan = TimeSpan.FromMilliseconds(500)

        For attempt As Integer = 1 To maxAttempts
            Dim retry As Boolean = False
            Try
                Return Await ApiClient.GetStringAsync(url)
            Catch ex As HttpRequestException
                If attempt < maxAttempts Then
                    retry = True
                Else
                    Throw
                End If
            Catch ex As TaskCanceledException
                If attempt < maxAttempts Then
                    retry = True
                Else
                    Throw
                End If
            End Try

            If retry Then
                Await Task.Delay(delay)
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2)
            End If
        Next

        ' Unreachable: the final attempt either returns or throws.
        Throw New InvalidOperationException("All retry attempts exhausted.")
    End Function
End Module
