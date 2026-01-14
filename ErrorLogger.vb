Option Strict On
Option Explicit On

Imports System.IO
Imports System.Windows.Forms

<CLSCompliant(True)>
Friend Module ErrorLogger
    Private ReadOnly SyncRoot As New Object()

    ' Write a structured error entry to Errors\Error_Log.txt next to the executable.
    Public Sub Log(ex As Exception, context As String)
        If ex Is Nothing Then
            Return
        End If

        LogMessage(ex.Message, ex.StackTrace, context, ex.GetType().FullName)
    End Sub

    Public Sub LogMessage(message As String, stackTrace As String, context As String, Optional typeName As String = "")
        Try
            Dim errorDir As String = Path.Combine(Application.StartupPath, "Errors")
            Directory.CreateDirectory(errorDir)

            Dim logPath As String = Path.Combine(errorDir, "Error_Log.txt")
            Dim entry As String = "Title: " & context & vbCrLf &
                "Type: " & typeName & vbCrLf &
                "Message: " & message & vbCrLf &
                "StackTrace: " & stackTrace & vbCrLf &
                "Date/Time: " & DateTime.Now.ToString("u") & vbCrLf &
                New String("="c, 91) & vbCrLf

            SyncLock SyncRoot
                File.AppendAllText(logPath, entry)
            End SyncLock
        Catch
        End Try
    End Sub
End Module
