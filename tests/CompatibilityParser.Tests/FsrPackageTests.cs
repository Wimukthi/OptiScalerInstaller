using OptiScalerInstaller;
using System.Text.Json;

internal static class FsrPackageTests
{
    public static IEnumerable<(string Name, Func<Task> Run)> GetTests()
    {
        yield return ("Stable FSR package edits and restores only the FSR section", TestStableApplyAndRestoreAsync);
        yield return ("Modern FSR schema rejects obsolete controls before copying", TestModernRejectsLegacyToggleAsync);
        yield return ("Legacy preflight is captured before package INI overwrite", TestSchemaPreflightBeforePackageIniAsync);
        yield return ("Duplicate FSR keys reject apply before copying", TestDuplicateFsrKeyAsync);
        yield return ("Duplicate package INI keys reject overwrite before copying", TestDuplicatePackageIniKeyAsync);
        yield return ("Skip mode ignores duplicate package INI that will not replace target", TestDuplicatePackageIniSkippedAsync);
        yield return ("Modern schema copies/removes Vulkan marker and preserves skipped files", TestModernVulkanApplyRemoveAsync);
        yield return ("Wrong-section lookalikes do not satisfy FSR schema", TestWrongSectionKeyAsync);
        yield return ("Explicit modern model/GPU selection is recognized", TestModernSelectionDetectionAsync);
        yield return ("Legacy manifests restore/remove flat keys without losing duplicates", TestLegacyManifestRemovalAsync);
    }

    private static Task TestStableApplyAndRestoreAsync()
    {
        WithTempFolders((game, package) =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[FSR]
Fsr4Update=false
Fsr4ForceEnableInt8=0
FsrAgilitySDKUpgrade=false

[Other]
Fsr4Update=other-value
""");
            WritePackage(package, "amdvkffx64.dll", "vulkan-marker");

            var manifest = ExperimentalFsr4Service.Apply(new ExperimentalFsr4ApplyOptions
            {
                GameFolder = game,
                PackageFolder = package,
                ConflictMode = ConflictMode.BackupAndOverwrite,
                EnableFsr4Update = true,
                EnableAgilityUpgrade = true,
                EnableForceInt8 = true
            }, _ => { });

            var applied = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(applied, "Fsr4Update=true", "FSR update should be enabled");
            AssertContains(applied, "Fsr4ForceEnableInt8=true", "force INT8 should be enabled");
            AssertContains(applied, "FsrAgilitySDKUpgrade=true", "Agility upgrade should be enabled");
            AssertContains(applied, "Fsr4Update=other-value", "same-named key in another section must be unchanged");
            AssertContains(manifest.InstalledFiles, "amdvkffx64.dll", "Vulkan marker should be tracked");

            if (!ExperimentalFsr4Service.Remove(game, _ => { }))
                throw new InvalidOperationException("Expected managed package removal to report changes.");

            var restored = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(restored, "Fsr4Update=false", "original FSR update value should be restored");
            AssertContains(restored, "Fsr4ForceEnableInt8=0", "original force INT8 value should be restored");
            AssertContains(restored, "FsrAgilitySDKUpgrade=false", "original Agility value should be restored");
            AssertContains(restored, "Fsr4Update=other-value", "other-section value should remain unchanged");
            AssertFileMissing(Path.Combine(game, "amdvkffx64.dll"), "managed marker should be removed");
        });

        return Task.CompletedTask;
    }

    private static Task TestModernRejectsLegacyToggleAsync()
    {
        WithTempFolders((game, package) =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[FSR]
Fsr4GpuSupport=0
Fsr4Model=auto
""");
            WritePackage(package, "amdvkffx64.dll", "vulkan-marker");

            var options = new ExperimentalFsr4ApplyOptions
            {
                GameFolder = game,
                PackageFolder = package,
                ConflictMode = ConflictMode.BackupAndOverwrite,
                EnableForceInt8 = true
            };
            try
            {
                ExperimentalFsr4Service.Apply(options, _ => { });
                throw new InvalidOperationException("Expected modern schema to reject legacy force-INT8 option.");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not supported", StringComparison.OrdinalIgnoreCase))
            {
                // Expected: validation happens before package files are copied.
            }

            AssertFileMissing(Path.Combine(game, "amdvkffx64.dll"), "incompatible package must not be copied");
            AssertFileMissing(Path.Combine(game, "OptiScalerInstaller.experimental-fsr4.json"), "failed preflight must not create a manifest");
        });

        return Task.CompletedTask;
    }

    private static Task TestModernVulkanApplyRemoveAsync()
    {
        WithTempFolders((game, package) =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[FSR]
Fsr4GpuSupport=0
Fsr4Model=0
""");
            WritePackage(package, "amdvkffx64.dll", "vulkan-marker");
            File.WriteAllText(Path.Combine(game, "unmanaged.cfg"), "user-owned");
            File.WriteAllText(Path.Combine(package, "unmanaged.cfg"), "package-copy");

            ExperimentalFsr4Service.Apply(new ExperimentalFsr4ApplyOptions
            {
                GameFolder = game,
                PackageFolder = package,
                ConflictMode = ConflictMode.Skip
            }, _ => { });

            AssertEqual("vulkan-marker", File.ReadAllText(Path.Combine(game, "amdvkffx64.dll")), "Vulkan marker content");
            AssertEqual("user-owned", File.ReadAllText(Path.Combine(game, "unmanaged.cfg")), "skipped unmanaged file content");
            if (!ExperimentalFsr4Service.Remove(game, _ => { }))
                throw new InvalidOperationException("Expected managed Vulkan package removal to report changes.");

            AssertFileMissing(Path.Combine(game, "amdvkffx64.dll"), "managed Vulkan marker should be removed");
            AssertEqual("user-owned", File.ReadAllText(Path.Combine(game, "unmanaged.cfg")), "unmanaged file must survive removal");
            AssertContains(File.ReadAllText(Path.Combine(game, "OptiScaler.ini")), "Fsr4GpuSupport=0", "default GPU support selection should remain unchanged");
        });

        return Task.CompletedTask;
    }

    private static Task TestSchemaPreflightBeforePackageIniAsync()
    {
        WithTempFolders((game, package) =>
        {
            var originalIni = """
[FSR]
Fsr4Update=false
""";
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), originalIni);
            File.WriteAllText(Path.Combine(package, "OptiScaler.ini"), """
[FSR]
Fsr4GpuSupport=0
Fsr4Model=0
""");
            WritePackage(package, "amdvkffx64.dll", "vulkan-marker");

            ExperimentalFsr4Service.Apply(new ExperimentalFsr4ApplyOptions
            {
                GameFolder = game,
                PackageFolder = package,
                ConflictMode = ConflictMode.BackupAndOverwrite,
                EnableFsr4Update = true
            }, _ => { });

            var packageIni = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(packageIni, "Fsr4GpuSupport=0", "package INI should have been copied after schema preflight");
            if (packageIni.Contains("Fsr4Update=true", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Legacy key must not be injected into the modern package INI.");

            ExperimentalFsr4Service.Remove(game, _ => { });
            AssertEqual(originalIni.Trim(), File.ReadAllText(Path.Combine(game, "OptiScaler.ini")).Trim(), "original INI should be restored from backup");
        });
        return Task.CompletedTask;
    }

    private static Task TestWrongSectionKeyAsync()
    {
        WithTempFolders((game, package) =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[Other]
Fsr4Update=other-value

[FSR]
Fsr4Update=false
""");
            WritePackage(package, "amdvkffx64.dll", "vulkan-marker");

            var options = new ExperimentalFsr4ApplyOptions
            {
                GameFolder = game,
                PackageFolder = package,
                ConflictMode = ConflictMode.BackupAndOverwrite,
                EnableFsr4Update = true
            };
            ExperimentalFsr4Service.Apply(options, _ => { });
            var ini = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(ini, "Fsr4Update=true", "requested FSR section should be enabled");
            AssertContains(ini, "Fsr4Update=other-value", "wrong-section lookalike should be untouched");
            ExperimentalFsr4Service.Remove(game, _ => { });
            ini = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(ini, "Fsr4Update=other-value", "wrong-section value should remain after restore");
        });

        return Task.CompletedTask;
    }

    private static Task TestModernSelectionDetectionAsync()
    {
        WithTempFolders((game, _) =>
        {
            var iniPath = Path.Combine(game, "OptiScaler.ini");
            File.WriteAllText(iniPath, """
[FSR]
Fsr4GpuSupport=0
Fsr4Model=auto
""");
            AssertFalse(ExperimentalFsr4Service.IsFsr4AlreadyEnabled(game), "zero auto/default selectors are not proof of enablement");

            File.WriteAllText(iniPath, """
[FSR]
Fsr4GpuSupport=2
Fsr4Model=0
""");
            AssertTrue(ExperimentalFsr4Service.IsFsr4AlreadyEnabled(game), "explicit GPU support selection should be recognized");

            File.WriteAllText(iniPath, """
[FSR]
Fsr4GpuSupport=0
Fsr4Model=0
""");
            AssertTrue(ExperimentalFsr4Service.IsFsr4AlreadyEnabled(game), "explicit FP8 model selection should be recognized");

            File.WriteAllText(iniPath, """
[FSR]
Fsr4GpuSupport=5
Fsr4Model=auto
""");
            AssertFalse(ExperimentalFsr4Service.IsFsr4AlreadyEnabled(game), "out-of-range GPU support values should not count as explicit selections");
        });
        return Task.CompletedTask;
    }

    private static Task TestDuplicateFsrKeyAsync()
    {
        WithTempFolders((game, package) =>
        {
            var ini = """
[FSR]
Fsr4Update=false
Fsr4Update=true
""";
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), ini);
            WritePackage(package, "amdvkffx64.dll", "vulkan-marker");

            try
            {
                ExperimentalFsr4Service.Apply(new ExperimentalFsr4ApplyOptions
                {
                    GameFolder = game,
                    PackageFolder = package,
                    ConflictMode = ConflictMode.BackupAndOverwrite,
                    EnableFsr4Update = true
                }, _ => { });
                throw new InvalidOperationException("Expected duplicate [FSR] key rejection.");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("duplicate entries", StringComparison.OrdinalIgnoreCase))
            {
                // Expected: ambiguous duplicate values are rejected before copy.
            }

            AssertFileMissing(Path.Combine(game, "amdvkffx64.dll"), "duplicate-key preflight must occur before package copy");
            AssertEqual(ini.Trim(), File.ReadAllText(Path.Combine(game, "OptiScaler.ini")).Trim(), "duplicate INI should remain byte-content unchanged");
        });
        return Task.CompletedTask;
    }

    private static Task TestDuplicatePackageIniKeyAsync()
    {
        WithTempFolders((game, package) =>
        {
            var originalIni = """
[FSR]
Fsr4Update=false
""";
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), originalIni);
            File.WriteAllText(Path.Combine(package, "OptiScaler.ini"), """
[FSR]
Fsr4Update=false
Fsr4Update=true
""");
            WritePackage(package, "amdvkffx64.dll", "vulkan-marker");

            try
            {
                ExperimentalFsr4Service.Apply(new ExperimentalFsr4ApplyOptions
                {
                    GameFolder = game,
                    PackageFolder = package,
                    ConflictMode = ConflictMode.BackupAndOverwrite,
                    EnableFsr4Update = true
                }, _ => { });
                throw new InvalidOperationException("Expected duplicate package [FSR] key rejection.");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("package OptiScaler.ini has duplicate", StringComparison.OrdinalIgnoreCase))
            {
                // Expected: package INI is preflighted before any package files move.
            }

            AssertEqual(originalIni.Trim(), File.ReadAllText(Path.Combine(game, "OptiScaler.ini")).Trim(), "target INI must remain unchanged");
            AssertFileMissing(Path.Combine(game, "amdvkffx64.dll"), "duplicate package INI preflight must happen before marker copy");
            AssertFileMissing(Path.Combine(game, "OptiScalerInstaller.experimental-fsr4.json"), "duplicate package INI preflight must not create a manifest");
            if (Directory.GetFiles(game, "OptiScaler.ini.bak_*", SearchOption.TopDirectoryOnly).Length != 0)
                throw new InvalidOperationException("Duplicate package INI preflight must happen before creating a backup.");
        });
        return Task.CompletedTask;
    }

    private static Task TestDuplicatePackageIniSkippedAsync()
    {
        WithTempFolders((game, package) =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[FSR]
Fsr4Update=false
""");
            File.WriteAllText(Path.Combine(package, "OptiScaler.ini"), """
[FSR]
Fsr4Update=false
Fsr4Update=true
""");
            WritePackage(package, "amdvkffx64.dll", "vulkan-marker");

            ExperimentalFsr4Service.Apply(new ExperimentalFsr4ApplyOptions
            {
                GameFolder = game,
                PackageFolder = package,
                ConflictMode = ConflictMode.Skip,
                EnableFsr4Update = true
            }, _ => { });

            AssertContains(File.ReadAllText(Path.Combine(game, "OptiScaler.ini")), "Fsr4Update=true", "legacy option should update the target INI that Skip preserved");
            ExperimentalFsr4Service.Remove(game, _ => { });
            AssertContains(File.ReadAllText(Path.Combine(game, "OptiScaler.ini")), "Fsr4Update=false", "original target INI should be restored");
            AssertFileMissing(Path.Combine(game, "amdvkffx64.dll"), "managed marker should be removed");
        });
        return Task.CompletedTask;
    }

    private static Task TestLegacyManifestRemovalAsync()
    {
        WithTempFolders((game, _) =>
        {
            File.WriteAllText(Path.Combine(game, "OptiScaler.ini"), """
[Other]
Fsr4Update=true
Fsr4Update=unrelated-duplicate

[Tail]
Fsr4ForceEnableInt8=true
""");

            var legacyManifest = new ExperimentalFsr4Manifest
            {
                InstallerVersion = "1.0.0.0",
                GameFolder = game,
                InstalledFiles = new List<string>(),
                BackupFiles = new Dictionary<string, string>(),
                IniKeys = new Dictionary<string, ExperimentalIniKeyState>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Fsr4Update"] = new ExperimentalIniKeyState { Existed = true, Value = "false" },
                    ["Fsr4ForceEnableInt8"] = new ExperimentalIniKeyState { Existed = false, Value = "" }
                }
            };
            File.WriteAllText(
                Path.Combine(game, "OptiScalerInstaller.experimental-fsr4.json"),
                JsonSerializer.Serialize(legacyManifest, new JsonSerializerOptions { WriteIndented = true }));

            if (!ExperimentalFsr4Service.Remove(game, _ => { }))
                throw new InvalidOperationException("Expected legacy manifest removal to report INI changes.");

            var restored = File.ReadAllText(Path.Combine(game, "OptiScaler.ini"));
            AssertContains(restored, "Fsr4Update=false", "legacy existing key should be restored in its original section");
            AssertContains(restored, "Fsr4Update=unrelated-duplicate", "duplicate same-name key must remain untouched");
            if (restored.Contains("Fsr4ForceEnableInt8=true", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Legacy installer-appended force key should be removed.");
            AssertContains(restored, "[Tail]", "legacy removal should preserve section layout");
        });
        return Task.CompletedTask;
    }

    private static void WithTempFolders(Action<string, string> action)
    {
        var root = Path.Combine(Path.GetTempPath(), "OptiScalerInstaller-FsrTests", Guid.NewGuid().ToString("N"));
        var game = Path.Combine(root, "game");
        var package = Path.Combine(root, "package");
        Directory.CreateDirectory(game);
        Directory.CreateDirectory(package);
        try { action(game, package); }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static void WritePackage(string package, string name, string content) =>
        File.WriteAllText(Path.Combine(package, name), content);

    private static void AssertContains(string actual, string expected, string label)
    {
        if (!actual.Contains(expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"{label}: expected to find '{expected}'.");
    }

    private static void AssertContains(IEnumerable<string>? actual, string expected, string label)
    {
        if (actual is null || !actual.Any(path => path.EndsWith(expected, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"{label}: expected '{expected}' in tracked paths.");
    }

    private static void AssertFileMissing(string path, string label)
    {
        if (File.Exists(path)) throw new InvalidOperationException($"{label}: file still exists at {path}.");
    }

    private static void AssertEqual<T>(T expected, T actual, string label)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"{label}: expected '{expected}', got '{actual}'.");
    }

    private static void AssertTrue(bool value, string label)
    {
        if (!value) throw new InvalidOperationException($"{label}: expected true.");
    }

    private static void AssertFalse(bool value, string label)
    {
        if (value) throw new InvalidOperationException($"{label}: expected false.");
    }
}
