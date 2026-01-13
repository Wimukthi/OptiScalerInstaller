Friend Module Program

    <STAThread()>
    Friend Sub Main(args As String())
        Application.SetColorMode(ThemeSettings.GetPreferredColorMode())
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainForm())
    End Sub

End Module
