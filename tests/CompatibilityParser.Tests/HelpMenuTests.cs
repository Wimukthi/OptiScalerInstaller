using System.Reflection;
using System.Windows.Forms;
using OptiScalerInstaller;

// STA UI regressions for Help actions that must work while Settings is not selected.
public static class HelpMenuTests
{
    public static Task RunAsync() => RunStaAsync(() => TestAboutOpensWithSettingsHidden().GetAwaiter().GetResult());

    public static Task TestUpdateReentrancyGuard() => RunStaAsync(() =>
    {
        using var main = new MainForm();
        var updateItem = GetMenuItem(main, "menuHelpCheckUpdates");
        var inProgress = main.GetType().GetField("manualUpdateCheckInProgress", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(main.GetType().FullName, "manualUpdateCheckInProgress");
        inProgress.SetValue(main, true);

        var action = main.GetType().GetMethod("RunManualUpdateCheckAsync", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(main.GetType().FullName, "RunManualUpdateCheckAsync");
        var task = (Task?)action.Invoke(main, null)
            ?? throw new InvalidOperationException("Update action did not return a Task.");
        task.GetAwaiter().GetResult();

        if (!updateItem.Enabled)
            throw new InvalidOperationException("A rejected duplicate update check must not disable the Help item.");
        if (!(bool)inProgress.GetValue(main)!)
            throw new InvalidOperationException("A rejected duplicate update check must not clear the active guard.");
    });

    public static Task TestAboutOpensWithSettingsHidden()
    {
        using var main = new MainForm();
        var tabs = GetControl<TabControl>(main, "tabMain");
        var settingsTab = GetControl<TabPage>(main, "tabSettings");
        var aboutButton = GetControl<Button>(main, "btnAbout");
        var aboutItem = GetMenuItem(main, "menuHelpAbout");

        tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().First(page => page.Name == "tabCompatibility");
        if (aboutButton.CanSelect)
            throw new InvalidOperationException("About button should not be selectable while Settings is hidden.");
        if (settingsTab.Parent != tabs)
            throw new InvalidOperationException("Settings tab is not attached to the main tab control.");

        var observed = false;
        using var timer = new System.Windows.Forms.Timer { Interval = 40 };
        var timeout = DateTime.UtcNow.AddSeconds(5);
        timer.Tick += (_, _) =>
        {
            var about = Application.OpenForms.Cast<Form>().FirstOrDefault(form => form.GetType().Name == "frmAbout");
            if (about is not null)
            {
                observed = true;
                about.Close();
            }
            else if (DateTime.UtcNow >= timeout)
            {
                foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
                    if (form != main) form.Close();
            }
        };
        timer.Start();
        aboutItem.PerformClick();
        timer.Stop();

        if (!observed)
            throw new InvalidOperationException("Help > About did not open About while Settings was hidden.");
        return Task.CompletedTask;
    }

    private static Task RunStaAsync(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { error = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return error is null ? Task.CompletedTask : Task.FromException(error);
    }

    private static T GetControl<T>(Control root, string name) where T : Control
    {
        return root.Controls.Find(name, true).OfType<T>().FirstOrDefault()
            ?? throw new InvalidOperationException($"Control {name} was not found.");
    }

    private static ToolStripMenuItem GetMenuItem(Form form, string name)
    {
        static IEnumerable<ToolStripItem> AllItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                yield return item;
                if (item is ToolStripDropDownItem dropDown)
                    foreach (var child in AllItems(dropDown.DropDownItems)) yield return child;
            }
        }

        return form.Controls.OfType<MenuStrip>().SelectMany(strip => AllItems(strip.Items))
            .OfType<ToolStripMenuItem>().FirstOrDefault(item => item.Name == name)
            ?? throw new InvalidOperationException($"Menu item {name} was not found.");
    }
}
