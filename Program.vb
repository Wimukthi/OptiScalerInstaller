Imports System.Threading

Friend Module Program
    ' App entry point with global exception logging and DPI-aware WinForms setup.
    <STAThread()>
    Friend Sub Main(args As String())
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf OnThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException

        Application.SetColorMode(ThemeSettings.GetPreferredColorMode())
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainForm())
    End Sub

    Private Sub OnThreadException(sender As Object, e As ThreadExceptionEventArgs)
        ErrorLogger.Log(e.Exception, "Application.ThreadException")
    End Sub

    Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex As Exception = TryCast(e.ExceptionObject, Exception)
        ErrorLogger.Log(ex, "AppDomain.UnhandledException")
    End Sub
End Module
