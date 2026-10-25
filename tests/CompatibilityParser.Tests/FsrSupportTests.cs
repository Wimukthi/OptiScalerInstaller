using System.Reflection;
using System.Windows.Forms;
using OptiScalerInstaller;

public static class FsrSupportTests
{
    public static IEnumerable<(string Name, Func<Task> Run)> GetTests()
    {
        yield return ("FSR support classifies desktop RX generations", TestDesktopGenerations);
        yield return ("FSR support classifies APUs without claiming driver support", TestApusStayQualified);
        yield return ("FSR support excludes pre-RDNA, unknown and unrelated adapters", TestNonRdnaAndUnknownNames);
        yield return ("FSR guidance stays separate from package status and manual apply is opt-in", TestSupportUiSeparation);
    }

    private static Task TestDesktopGenerations()
    {
        AssertGeneration("AMD Radeon RX 6800 XT", AmdRdnaGeneration.Rdna2);
        AssertGeneration("AMD Radeon RX 7900 XTX", AmdRdnaGeneration.Rdna3);
        AssertGeneration("AMD Radeon RX 9070 XT", AmdRdnaGeneration.Rdna4);
        AssertGeneration("AMD Radeon RX 9060 XT", AmdRdnaGeneration.Rdna4);
        AssertGeneration("AMD Radeon RX 9050", AmdRdnaGeneration.Rdna4);
        AssertGeneration("AMD Radeon RX 9060XT", AmdRdnaGeneration.Rdna4);
        AssertGeneration("AMD Radeon PRO W7900", AmdRdnaGeneration.Rdna3);
        var workstation = AmdFsrSupportService.BuildGuidance(new[] { "AMD Radeon PRO W7900" });
        AssertContains(workstation, "workstation", "workstation-specific guidance");
        if (workstation.Contains("mobile/APU", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Workstation guidance must not claim the adapter is an APU.");

        var guidance = AmdFsrSupportService.BuildGuidance(new[] { "AMD Radeon RX 6800 XT" });
        AssertContains(guidance, "RX 6000, RX 7000 and RX 9000", "desktop driver guidance");
        AssertContains(guidance, "26.10.1", "RX 6000 driver minimum");
        return Task.CompletedTask;
    }

    private static Task TestApusStayQualified()
    {
        foreach (var apu in new[] { "AMD Radeon 680M", "AMD Radeon 780M", "AMD Radeon 890M", "AMD Radeon 8060S" })
        {
            var generation = AmdFsrSupportService.ClassifyAdapter(apu);
            if (generation is not (AmdRdnaGeneration.Rdna2 or AmdRdnaGeneration.Rdna3))
                throw new InvalidOperationException($"Expected {apu} to be classified as RDNA2/3, got {generation}.");
        }

        var guidance = AmdFsrSupportService.BuildGuidance(new[] { "AMD Radeon 680M" });
        AssertContains(guidance, "exact mobile/APU model", "APU guidance qualification");
        if (guidance.Contains("FSR Upscaling 4.1 across RX 6000", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("An APU generation match must not imply blanket desktop-driver support.");
        return Task.CompletedTask;
    }

    private static Task TestNonRdnaAndUnknownNames()
    {
        AssertGeneration("AMD Radeon RX Vega 64", AmdRdnaGeneration.None);
        AssertGeneration("AMD Radeon RX 590", AmdRdnaGeneration.None);
        AssertGeneration("AMD Radeon RX 580", AmdRdnaGeneration.None);
        AssertGeneration("NVIDIA GeForce RTX 4090", AmdRdnaGeneration.None);
        AssertGeneration("Intel Arc 7800 Graphics", AmdRdnaGeneration.None);
        AssertGeneration("AMD Radeon Graphics", AmdRdnaGeneration.Unknown);
        AssertGeneration("AMD RDNA adapter", AmdRdnaGeneration.Unknown);
        AssertGeneration("AMD Radeon Model780M capable", AmdRdnaGeneration.None);
        AssertGeneration("AMD Radeon RX 78000", AmdRdnaGeneration.None);
        return Task.CompletedTask;
    }

    private static Task TestSupportUiSeparation() => RunStaAsync(() =>
    {
        using var form = new MainForm();
        var initialize = form.GetType().GetMethod("InitializeFsrSupportUi", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(form.GetType().FullName, "InitializeFsrSupportUi");
        initialize.Invoke(form, null);

        var guidance = GetField<Label>(form, "fsrSupportGuidance")
            ?? throw new InvalidOperationException("Hardware guidance label was not initialized.");
        var packageStatus = form.Controls.Find("lblFsr4Status", true).OfType<Label>().FirstOrDefault()
            ?? throw new InvalidOperationException("Package status label was not initialized.");
        var apply = form.Controls.Find("btnFsr4Apply", true).OfType<Button>().FirstOrDefault()
            ?? throw new InvalidOperationException("Manual Apply button was not initialized.");

        if (ReferenceEquals(guidance, packageStatus))
            throw new InvalidOperationException("GPU hardware guidance must not reuse per-game package status.");
        if (!packageStatus.Text.StartsWith("Manual package:", StringComparison.Ordinal))
            throw new InvalidOperationException("Per-game package status text was overwritten by support guidance.");
        if (guidance.Right > guidance.Parent!.ClientSize.Width)
            throw new InvalidOperationException("FSR support guidance extends beyond the panel instead of wrapping.");
        if (apply.Enabled)
            throw new InvalidOperationException("Manual package Apply must remain disabled until the opt-in checkbox is checked.");
    });

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

    private static void AssertGeneration(string name, AmdRdnaGeneration expected)
    {
        var actual = AmdFsrSupportService.ClassifyAdapter(name);
        if (actual != expected)
            throw new InvalidOperationException($"Expected '{name}' => {expected}, got {actual}.");
    }

    private static T? GetField<T>(object instance, string name) where T : class
    {
        for (var type = instance.GetType(); type is not null; type = type.BaseType)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (field?.GetValue(instance) is T value) return value;
        }
        return null;
    }

    private static void AssertContains(string value, string expected, string context)
    {
        if (!value.Contains(expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Expected {context} to contain '{expected}', got: {value}");
    }
}
