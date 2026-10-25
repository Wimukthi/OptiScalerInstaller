using OptiScalerInstaller;

public static class DocumentationTests
{
    public static IEnumerable<(string Name, Func<Task> Run)> GetTests()
    {
        yield return ("Documentation viewer lists only four bundled guides", TestAllowlist);
        yield return ("Documentation service loads bundled guide content", TestBundledUsageLoads);
        yield return ("Documentation service gives a graceful missing-file fallback", TestMissingGuideFallback);
        yield return ("Documentation external links allow only HTTP and HTTPS", TestWebUrlAllowlist);
        yield return ("Documentation tables render as readable rows and preserve escaped pipes", TestTableConversion);
        yield return ("Documentation removes keyboard tags and maps local links online", TestTagAndRelativeLinkConversion);
        yield return ("Invalid local schemes are sanitized without losing the guide", TestInvalidLinkSanitization);
    }

    private static Task TestAllowlist()
    {
        var documents = DocumentationService.GetAvailableDocuments();
        var expected = new[]
        {
            InstallerDocumentation.Usage,
            InstallerDocumentation.AmdFsrSupport,
            InstallerDocumentation.Configuration,
            InstallerDocumentation.Troubleshooting
        };
        if (!documents.SequenceEqual(expected))
            throw new InvalidOperationException("Documentation allowlist does not match the four supported guides.");
        return Task.CompletedTask;
    }

    private static Task TestBundledUsageLoads()
    {
        var markdown = DocumentationService.LoadDocument(InstallerDocumentation.Usage);
        if (markdown.Contains("not bundled with this installation", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Usage guide was not copied to the application output docs directory.");
        if (string.IsNullOrWhiteSpace(markdown) || markdown.Length < 100)
            throw new InvalidOperationException("Bundled usage guide content was empty or unexpectedly short.");
        return Task.CompletedTask;
    }

    private static Task TestMissingGuideFallback()
    {
        var directory = Path.Combine(Path.GetTempPath(), "OptiScalerInstaller-DocsMissing-" + Guid.NewGuid().ToString("N"));
        var markdown = DocumentationService.LoadDocument(InstallerDocumentation.AmdFsrSupport, directory);
        if (!markdown.Contains("not bundled with this installation", StringComparison.OrdinalIgnoreCase) ||
            !markdown.Contains("https://github.com/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Missing guide did not produce a readable online fallback.");
        return Task.CompletedTask;
    }

    private static Task TestWebUrlAllowlist()
    {
        if (!DocumentationService.IsAllowedWebUrl("https://github.com/example/repo"))
            throw new InvalidOperationException("HTTPS URL should be allowed.");
        if (!DocumentationService.IsAllowedWebUrl("http://example.com"))
            throw new InvalidOperationException("HTTP URL should be allowed.");
        foreach (var rejected in new[] { "file:///C:/secret.txt", "javascript:alert(1)", "C:\\secret.txt", "//example.com/path" })
            if (DocumentationService.IsAllowedWebUrl(rejected))
                throw new InvalidOperationException($"Non-web URL should be rejected: {rejected}");
        return Task.CompletedTask;
    }

    private static Task TestTableConversion()
    {
        const string source = "| File | Purpose |\n| --- | --- |\n| settings.json | User \\| machine preferences |\n| OptiScaler.ini | Game configuration |\n| C:\\Games\\OptiScaler | Windows path |";
        var rendered = DocumentationService.PrepareMarkdownForDisplay(source, InstallerDocumentation.Usage);
        if (!rendered.Contains("**File:** settings.json", StringComparison.Ordinal) ||
            !rendered.Contains("**Purpose:** User | machine preferences", StringComparison.Ordinal) ||
            rendered.Contains("| --- |", StringComparison.Ordinal) ||
            !rendered.Contains(@"C:\Games\OptiScaler", StringComparison.Ordinal))
            throw new InvalidOperationException("Markdown table was not converted into readable, escaped-pipe-preserving rows.");
        return Task.CompletedTask;
    }

    private static Task TestTagAndRelativeLinkConversion()
    {
        const string source = "Use <kbd>Ctrl</kbd>+<kbd>F</kbd>. [Setup](configuration.md#setup)\n![screen](images/setup.png)";
        var rendered = DocumentationService.PrepareMarkdownForDisplay(source, InstallerDocumentation.Usage);
        if (rendered.Contains("<kbd", StringComparison.OrdinalIgnoreCase) ||
            !rendered.Contains("Ctrl+F", StringComparison.Ordinal) ||
            !rendered.Contains("https://github.com/Wimukthi/OptiScalerInstaller/blob/main/docs/configuration.md#setup", StringComparison.Ordinal) ||
            !rendered.Contains("[View screenshot](https://github.com/Wimukthi/OptiScalerInstaller/blob/main/docs/images/setup.png)", StringComparison.Ordinal))
            throw new InvalidOperationException("Keyboard tags, relative guide links, or screenshot links were not normalized for the viewer.");
        return Task.CompletedTask;
    }

    private static Task TestInvalidLinkSanitization()
    {
        var rendered = DocumentationService.PrepareMarkdownForDisplay("Guide text [local](file:///C:/secret.txt) and [malformed](http://[).", InstallerDocumentation.Usage);
        if (!rendered.Contains("Guide text", StringComparison.Ordinal) ||
            rendered.Contains("file:///C:/secret.txt", StringComparison.OrdinalIgnoreCase) ||
            rendered.Contains("http://[", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Unsafe or malformed link text was not sanitized while preserving the guide.");
        return Task.CompletedTask;
    }
}
