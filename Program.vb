Imports System.Threading
Imports System.IO

Friend Module Program
    ' App entry point with global exception logging and DPI-aware WinForms setup.
    Private fatalDialogShown As Integer

    <STAThread()>
    Friend Sub Main(args As String())
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf OnThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException

        Application.SetColorMode(ThemeSettings.GetPreferredColorMode())
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Try
            Application.Run(New MainForm())
        Catch ex As Exception
            ErrorLogger.Log(ex, "Program.Main")
            ShowFatalStartupError(ex, "Program.Main")
            Environment.Exit(1)
        End Try
    End Sub

    Private Sub OnThreadException(sender As Object, e As ThreadExceptionEventArgs)
        ErrorLogger.Log(e.Exception, "Application.ThreadException")
        ShowFatalStartupError(e.Exception, "Application.ThreadException")
        Application.ExitThread()
    End Sub

    Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex As Exception = TryCast(e.ExceptionObject, Exception)
        ErrorLogger.Log(ex, "AppDomain.UnhandledException")
        ShowFatalStartupError(ex, "AppDomain.UnhandledException")
        Environment.Exit(1)
    End Sub

    Private Sub ShowFatalStartupError(ex As Exception, source As String)
        If Threading.Interlocked.Exchange(fatalDialogShown, 1) <> 0 Then
            Return
        End If

        Dim logPath As String = Path.Combine(Application.StartupPath, "Errors", "Error_Log.txt")
        Dim message As String =
            "OptiScaler Installer hit a startup error and will close." & Environment.NewLine & Environment.NewLine &
            "Source: " & source & Environment.NewLine &
            "Error: " & If(ex Is Nothing, "Unknown", ex.Message) & Environment.NewLine & Environment.NewLine &
            "Diagnostic log: " & logPath

        Try
            MessageBox.Show(message, "OptiScaler Installer", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch
            ' Avoid secondary failures while reporting fatal startup errors.
        End Try
    End Sub
End Module
