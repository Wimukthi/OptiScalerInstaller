using System.Reflection;
using OptiScalerInstaller;

public static class FrameGenerationTests
{
    public static IEnumerable<(string Name, Func<Task> Run)> GetTests()
    {
        yield return ("Stable frame-generation selections map and verify shipped keys", TestStableMappingsAndVerificationAsync);
        yield return ("Auto frame-generation selection preserves customized values", TestAutoPreservesValuesAsync);
        yield return ("Current frame-generation schema maps Nukem to nvngxfg", TestCurrentMasterNukemMappingAsync);
        yield return ("One-sided nvngxfg documentation does not guess the other token", TestOneSidedNvngxfgDocumentationAsync);
        yield return ("Legacy FGType schema remains supported", TestLegacyFgTypeAsync);
        yield return ("Unknown frame-generation schema is warned without inventing keys", TestUnknownSchemaAsync);
        yield return ("Modern schema without Enabled does not gain that key", TestMissingEnabledAsync);
    }

    private static Task TestStableMappingsAndVerificationAsync()
    {
        CheckModernSelection(FgTypeSelection.None, "false", "nofg", "nofg");
        CheckModernSelection(FgTypeSelection.OptiFg, "true", "upscaler", "fsrfg");
        CheckModernSelection(FgTypeSelection.Nukem, "true", "nukems", "nukems");
        return Task.CompletedTask;
    }

    private static void CheckModernSelection(FgTypeSelection selection, string enabled, string input, string output)
    {
        WithGameFolder(game =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[framegen]
enabled = auto
fginput = auto
fgoutput = auto
[Spoofing]
Dxgi=auto
[Plugins]
LoadAsiPlugins=false
LoadReshade=false
LoadSpecialK=false
""");
            var config = CreateConfig(game, selection);
            InvokeUpdateIni(config, new List<string>());

            var ini = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(ini, $"Enabled={enabled}", "Enabled mapping");
            AssertContains(ini, $"FGInput={input}", "FGInput mapping");
            AssertContains(ini, $"FGOutput={output}", "FGOutput mapping");
            if (ini.Contains("FGType=", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Modern schema must not gain the obsolete FGType key.");
            var report = InstallerService.VerifyInstall(config, new InstallManifest());
            AssertNoErrors(report, selection.ToString());
            if (report.Warnings.Any(warning => warning.Contains("duplicate", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Whitespace and case changes must not create duplicate FG keys.");
            AssertPassed(report, $"INI [FrameGen] Enabled={enabled}");
            AssertPassed(report, $"INI [FrameGen] FGInput={input}");
            AssertPassed(report, $"INI [FrameGen] FGOutput={output}");
        });
    }

    private static Task TestAutoPreservesValuesAsync()
    {
        WithGameFolder(game =>
        {
            const string source = """
[FrameGen]
Enabled=true
FGInput=nukems
FGOutput=fsrfg
""";
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), source);
            var config = CreateConfig(game, FgTypeSelection.Auto);
            InvokeUpdateIni(config, new List<string>());

            var ini = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(ini, "Enabled=true", "customized Enabled should remain");
            AssertContains(ini, "FGInput=nukems", "customized input should remain");
            AssertContains(ini, "FGOutput=fsrfg", "customized output should remain");
            if (ini.Contains("FGType=", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Auto must not add the obsolete FGType key.");

            var report = InstallerService.VerifyInstall(config, new InstallManifest());
            AssertNoErrors(report, "Auto verification");
            if (report.Passed.Any(value => value.Contains("[FrameGen]", StringComparison.OrdinalIgnoreCase)) ||
                report.Warnings.Any(value => value.Contains("[FrameGen]", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Auto verification should not claim or require a frame-generation override.");
        });
        return Task.CompletedTask;
    }

    private static Task TestCurrentMasterNukemMappingAsync()
    {
        CheckNukemMapping("""
[FrameGen]
Enabled=auto
; Selected FG Input/Source
; nvngxfg - current input token
FGInput=auto
; Selected FG Output
; nvngxfg - current output token
FGOutput=auto
[FSR]
Fsr4GpuSupport=0
""");
        CheckNukemMapping("""
[FrameGen]
Enabled=auto
; Selected FG Input/Source
; nvngxfg - current input token
FGInput=auto
; Selected FG Output
; nvngxfg - current output token
FGOutput=auto
""");
        return Task.CompletedTask;
    }

    private static void CheckNukemMapping(string iniText)
    {
        WithGameFolder(game =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), iniText);
            var config = CreateConfig(game, FgTypeSelection.Nukem);
            InvokeUpdateIni(config, new List<string>());

            var ini = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(ini, "FGInput=nvngxfg", "current Nukem input token");
            AssertContains(ini, "FGOutput=nvngxfg", "current Nukem output token");
            var report = InstallerService.VerifyInstall(config, new InstallManifest());
            AssertNoErrors(report, "current-schema verification");
            AssertPassed(report, "INI [FrameGen] FGInput=nvngxfg");
            AssertPassed(report, "INI [FrameGen] FGOutput=nvngxfg");
        });
    }

    private static Task TestOneSidedNvngxfgDocumentationAsync()
    {
        WithGameFolder(game =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[FrameGen]
Enabled=auto
; FGInput: nvngxfg is available here
FGInput=custom-input
; FGOutput: only fsrfg is documented
FGOutput=custom-output
""");
            var logs = new List<string>();
            var config = CreateConfig(game, FgTypeSelection.Nukem);
            InvokeUpdateIni(config, logs);

            var ini = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(ini, "Enabled=auto", "ambiguous Nukem mapping must preserve Enabled");
            AssertContains(ini, "FGInput=custom-input", "ambiguous Nukem mapping must preserve input");
            AssertContains(ini, "FGOutput=custom-output", "ambiguous Nukem mapping must preserve output");
            if (!logs.Any(value => value.Contains("documented for only one", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("One-sided nvngxfg documentation should produce a warning.");
        });
        return Task.CompletedTask;
    }

    private static Task TestLegacyFgTypeAsync()
    {
        WithGameFolder(game =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[FrameGen]
FGType=auto
[Plugins]
LoadAsiPlugins=false
LoadReshade=false
LoadSpecialK=false
""");
            var config = CreateConfig(game, FgTypeSelection.Nukem);
            InvokeUpdateIni(config, new List<string>());

            var ini = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(ini, "FGType=nukems", "legacy FGType selection");
            if (ini.Contains("FGInput=", StringComparison.OrdinalIgnoreCase) || ini.Contains("Enabled=", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Legacy schema must not gain modern frame-generation keys.");
            var report = InstallerService.VerifyInstall(config, new InstallManifest());
            AssertNoErrors(report, "legacy verification");
            AssertPassed(report, "INI [FrameGen] FGType=nukems");
        });
        return Task.CompletedTask;
    }

    private static Task TestUnknownSchemaAsync()
    {
        WithGameFolder(game =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[FrameGen]
CustomOption=preserve
""");
            var logs = new List<string>();
            var config = CreateConfig(game, FgTypeSelection.OptiFg);
            InvokeUpdateIni(config, logs);

            var ini = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(ini, "CustomOption=preserve", "unknown custom frame-generation setting");
            foreach (var invented in new[] { "FGType=", "Enabled=", "FGInput=", "FGOutput=" })
                if (ini.Contains(invented, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Unknown schema must not receive invented key {invented}.");
            if (!logs.Any(value => value.Contains("schema is unknown", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Unknown explicit selection should produce a visible warning.");

            var report = InstallerService.VerifyInstall(config, new InstallManifest());
            AssertNoErrors(report, "unknown-schema verification");
            if (!report.Warnings.Any(value => value.Contains("schema is unknown", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Verification should report that the explicit selection was not applied.");
        });
        return Task.CompletedTask;
    }

    private static Task TestMissingEnabledAsync()
    {
        WithGameFolder(game =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[FrameGen]
FGInput=auto
FGOutput=auto
""");
            var logs = new List<string>();
            var config = CreateConfig(game, FgTypeSelection.OptiFg);
            InvokeUpdateIni(config, logs);

            var ini = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(ini, "FGInput=auto", "partial schema should preserve input");
            AssertContains(ini, "FGOutput=auto", "partial schema should preserve output");
            if (ini.Contains("Enabled=", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Missing Enabled must not be added to the schema.");
            if (!logs.Any(value => value.Contains("cannot fully apply this selection", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Missing Enabled should produce a visible no-change warning.");

            var report = InstallerService.VerifyInstall(config, new InstallManifest());
            AssertNoErrors(report, "partial-schema verification");
            if (report.Passed.Any(value => value.Contains("[FrameGen]", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Verification must not claim frame-generation settings were applied to a partial schema.");
            if (!report.Warnings.Any(value => value.Contains("cannot fully apply this selection", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Verification should retain the partial-schema warning.");
        });
        return Task.CompletedTask;
    }

    private static InstallerConfig CreateConfig(string gameFolder, FgTypeSelection selection) => new()
    {
        GameFolder = gameFolder,
        HookName = "dxgi.dll",
        GpuVendor = GpuVendor.Nvidia,
        EnableDlssInputs = true,
        FgType = selection,
        DefaultIniMode = DefaultIniMode.Off,
        PluginsPath = "plugins"
    };

    private static void InvokeUpdateIni(InstallerConfig config, List<string> logs)
    {
        var updateIni = typeof(InstallerService).GetMethod("UpdateIni", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(typeof(InstallerService).FullName, "UpdateIni");
        updateIni.Invoke(null, new object?[] { config, (Action<string>)logs.Add });
    }

    private static void WithGameFolder(Action<string> action)
    {
        var game = Path.Combine(Path.GetTempPath(), "OptiScalerInstaller-FrameGen-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(game);
        try
        {
            File.WriteAllText(Path.Combine(game, "dxgi.dll"), "hook");
            action(game);
        }
        finally
        {
            if (Directory.Exists(game)) Directory.Delete(game, recursive: true);
        }
    }

    private static void AssertNoErrors(InstallVerificationReport report, string context)
    {
        if (report.Errors.Count > 0)
            throw new InvalidOperationException($"{context}: unexpected install verification errors: {string.Join(" | ", report.Errors)}");
    }

    private static void AssertPassed(InstallVerificationReport report, string expected)
    {
        if (!report.Passed.Contains(expected, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Expected verification pass '{expected}'. Actual: {string.Join(" | ", report.Passed)}");
    }

    private static void AssertContains(string actual, string expected, string context)
    {
        if (!actual.Contains(expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Expected {context} to contain '{expected}'.");
    }
}
