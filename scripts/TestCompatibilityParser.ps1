Param(
    [switch]$Live,
    [string]$LiveUrl = "https://raw.githubusercontent.com/wiki/optiscaler/OptiScaler/Compatibility-List.md",
    [string]$Configuration = "Debug",
    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$testProject = Join-Path $repoRoot "tests/CompatibilityParser.Tests/CompatibilityParser.Tests.csproj"
$versionFile = Join-Path $env:TEMP ("OptiScalerInstallerBuildVersion_" + [guid]::NewGuid().ToString("N") + ".txt")
$sourceVersionFile = Join-Path $repoRoot "BuildVersion.txt"

if (Test-Path $sourceVersionFile) {
    Get-Content -LiteralPath $sourceVersionFile | Set-Content -LiteralPath $versionFile
}
else {
    Set-Content -LiteralPath $versionFile -Value "0"
}

try {
    $dotnetArgs = @(
        "run",
        "--project", $testProject,
        "-c", $Configuration,
        "-p:VersionFile=$versionFile"
    )

    if ($NoRestore) {
        $dotnetArgs += "--no-restore"
    }

    $dotnetArgs += "--"
    if ($Live) {
        $dotnetArgs += @("--live", "--live-url", $LiveUrl)
    }

    Push-Location $repoRoot
    try {
        & dotnet @dotnetArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Compatibility parser tests failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}
finally {
    Remove-Item -LiteralPath $versionFile -Force -ErrorAction SilentlyContinue
}
