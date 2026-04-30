using OptiScalerInstaller;

var liveUrl = GetArgumentValue(args, "--live-url");
var runLive = args.Any(arg => string.Equals(arg, "--live", StringComparison.OrdinalIgnoreCase)) ||
              !string.IsNullOrWhiteSpace(liveUrl);

var tests = new List<(string Name, Func<Task> Run)>
{
    ("Markdown table parses linked and plain rows", TestMarkdownTableAsync),
    ("Standalone bundled links parse parenthesized slugs", TestStandaloneLinksAsync),
    ("JSON catalog parses entries and aliases", TestJsonCatalogAsync),
    ("Bundled compatibility list parses", TestBundledListAsync)
};

if (runLive)
{
    tests.Add(("Live official compatibility list canary", () => TestLiveListAsync(liveUrl)));
}

var failed = 0;
foreach (var test in tests)
{
    try
    {
        await test.Run();
        Console.WriteLine($"PASS {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {test.Name}: {ex.Message}");
    }
}

if (failed > 0)
{
    Environment.Exit(1);
}

static Task TestMarkdownTableAsync()
{
    const string markdown = """
> :muscle: **Working** — **3**
> :skull: **Not working** — **1**
<!--
| GAME NAME | status | inputs |
-->
| Game | Compatibility | Inputs |
| ---- | :-----------: | ----- |
| [Dead Space (2023)](Dead-Space-(2023)) | ✅ | DLSS |
| Alone in the Dark (2024) | ✅ | DLSS |
| Foo \| Bar | ❌ | XeSS |
| [External Docs](https://example.com/game) | ✅ | FSR |
""";

    var result = CompatibilityService.ParseCompatibilityListWithDiagnostics(markdown);

    AssertEqual("markdown", result.SourceFormat, "source format");
    AssertEqual(4, result.ExpectedCount, "expected count");
    AssertEqual(4, result.Entries.Count, "entry count");
    AssertMissing(result, "GAME NAME");
    AssertEntry(result, "Dead Space (2023)", "Dead-Space-(2023)");
    AssertEntry(result, "Alone in the Dark (2024)", "");
    AssertEntry(result, "Foo | Bar", "");
    AssertEntry(result, "External Docs", "");

    return Task.CompletedTask;
}

static Task TestStandaloneLinksAsync()
{
    const string markdown = """
## D
[Dead Space (2023)](Dead-Space-(2023))
[Escape from Tarkov (SPT)](Escape-from-Tarkov-(SPT))
""";

    var result = CompatibilityService.ParseCompatibilityListWithDiagnostics(markdown);

    AssertEqual(2, result.Entries.Count, "entry count");
    AssertEntry(result, "Dead Space (2023)", "Dead-Space-(2023)");
    AssertEntry(result, "Escape from Tarkov (SPT)", "Escape-from-Tarkov-(SPT)");

    return Task.CompletedTask;
}

static Task TestJsonCatalogAsync()
{
    const string json = """
{
  "expectedCount": 2,
  "entries": [
    {
      "name": "Dead Space (2023)",
      "wikiUrl": "https://github.com/optiscaler/OptiScaler/wiki/Dead-Space-(2023)",
      "aliases": ["Dead Space Remake"]
    },
    {
      "game": "63 Days"
    }
  ]
}
""";

    var result = CompatibilityService.ParseCompatibilityListWithDiagnostics(json);

    AssertEqual("json", result.SourceFormat, "source format");
    AssertEqual(2, result.ExpectedCount, "expected count");
    AssertEqual(2, result.Entries.Count, "entry count");
    AssertEntry(result, "Dead Space (2023)", "Dead-Space-(2023)");
    AssertEntry(result, "63 Days", "");

    var deadSpace = FindEntry(result, "Dead Space (2023)");
    if (deadSpace.Aliases is null || !deadSpace.Aliases.Contains("Dead Space Remake", StringComparer.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Expected JSON aliases to be preserved.");
    }

    return Task.CompletedTask;
}

static Task TestBundledListAsync()
{
    var repoRoot = FindRepoRoot();
    var bundledPath = Path.Combine(repoRoot, "Data", "Compatibility-List.md");
    if (!File.Exists(bundledPath))
    {
        throw new InvalidOperationException($"Bundled compatibility list not found: {bundledPath}");
    }

    var result = CompatibilityService.ParseCompatibilityListWithDiagnostics(File.ReadAllText(bundledPath));
    if (result.Entries.Count < 100)
    {
        throw new InvalidOperationException($"Expected bundled list to parse at least 100 entries, parsed {result.Entries.Count}.");
    }

    AssertMissing(result, "Template");
    AssertEntry(result, "Dead Space (2023)", "Dead-Space-Remake");
    AssertEntry(result, "Escape from Tarkov (SPT)", "Escape-from-Tarkov-(SPT)");

    return Task.CompletedTask;
}

static async Task TestLiveListAsync(string? liveUrl)
{
    liveUrl = string.IsNullOrWhiteSpace(liveUrl)
        ? "https://raw.githubusercontent.com/wiki/optiscaler/OptiScaler/Compatibility-List.md"
        : liveUrl;

    using var client = new HttpClient();
    var content = await client.GetStringAsync(liveUrl);
    var result = CompatibilityService.ParseCompatibilityListWithDiagnostics(content);

    if (result.Entries.Count < 600)
    {
        throw new InvalidOperationException($"Expected at least 600 live entries, parsed {result.Entries.Count}.");
    }

    if (result.ExpectedCount.HasValue)
    {
        var expected = result.ExpectedCount.Value;
        var tolerance = Math.Max(3, (int)Math.Ceiling(expected * 0.01));
        var delta = Math.Abs(result.Entries.Count - expected);
        if (delta > tolerance)
        {
            throw new InvalidOperationException($"Live parser drift too large: parsed {result.Entries.Count}, source advertises {expected}, tolerance {tolerance}.");
        }
    }

    AssertMissing(result, "GAME NAME");
    AssertEntry(result, "63 Days", "");
    AssertEntry(result, "Dead Space (2023)", "Dead-Space-(2023)");
}

static string? GetArgumentValue(string[] args, string name)
{
    for (var index = 0; index < args.Length - 1; index++)
    {
        if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[index + 1];
        }
    }

    return null;
}

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "OptiScalerInstaller.sln")) &&
            Directory.Exists(Path.Combine(dir.FullName, "Data")))
        {
            return dir.FullName;
        }

        dir = dir.Parent;
    }

    throw new InvalidOperationException("Unable to locate repository root.");
}

static CompatibilityEntry FindEntry(CompatibilityParseResult result, string name)
{
    var entry = result.Entries.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    if (entry is null)
    {
        throw new InvalidOperationException($"Missing entry: {name}");
    }

    return entry;
}

static void AssertEntry(CompatibilityParseResult result, string name, string slug)
{
    var entry = FindEntry(result, name);
    AssertEqual(slug, entry.Slug ?? "", $"slug for {name}");
}

static void AssertMissing(CompatibilityParseResult result, string name)
{
    if (result.Entries.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
    {
        throw new InvalidOperationException($"Unexpected entry: {name}");
    }
}

static void AssertEqual<T>(T expected, T actual, string label)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{label}: expected '{expected}', got '{actual}'.");
    }
}
