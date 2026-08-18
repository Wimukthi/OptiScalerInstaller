Param(
    [string]$Configuration = "Release",
    [string]$ProjectFile = "OptiScalerInstaller.vbproj",
    [switch]$SkipClean,
    [switch]$SkipBuild,
    [switch]$SkipCompatibilityValidation
)

$ErrorActionPreference = "Stop"

function Invoke-DotNet {
    Param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory
    )

    Push-Location $WorkingDirectory
    try {
        & dotnet @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

$projectPath = Resolve-Path $ProjectFile
$projectDir = Split-Path -Parent $projectPath
$projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
$targetFramework = "net10.0-windows"

$buildOutputDir = Join-Path $projectDir "bin/$Configuration/$targetFramework"
$stagingRoot = Join-Path $projectDir "artifacts/staging"
$releaseDir = Join-Path $projectDir "Release"

if (-not $SkipCompatibilityValidation) {
    $compatibilityTestScript = Join-Path $projectDir "scripts/TestCompatibilityParser.ps1"
    if (-not (Test-Path $compatibilityTestScript)) {
        throw "Compatibility parser test script not found: $compatibilityTestScript"
    }

    Write-Host "Validating compatibility parser against fixtures and live wiki..."
    & $compatibilityTestScript -Live -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Compatibility parser validation failed with exit code $LASTEXITCODE"
    }
}

if (-not $SkipBuild) {
    if (-not $SkipClean) {
        Write-Host "Cleaning project output..."
        Invoke-DotNet -Arguments @("clean", $projectPath, "-c", $Configuration) -WorkingDirectory $projectDir
    }

    Write-Host "Building project ($Configuration)..."
    Invoke-DotNet -Arguments @("build", $projectPath, "-c", $Configuration) -WorkingDirectory $projectDir
}

$exePath = Join-Path $buildOutputDir "$projectName.exe"
if (-not (Test-Path $exePath)) {
    throw "Expected build output not found: $exePath"
}

$fileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($exePath).FileVersion
if ([string]::IsNullOrWhiteSpace($fileVersion)) {
    throw "Unable to read file version from $exePath"
}

$packageVersion = $fileVersion.Trim()
$packageRootName = "$projectName-v$packageVersion-win-x64"
$packageStagingDir = Join-Path $stagingRoot $packageRootName
$packagePath = Join-Path $releaseDir "$packageRootName.zip"

if (Test-Path $packageStagingDir) {
    Remove-Item -LiteralPath $packageStagingDir -Recurse -Force
}
New-Item -ItemType Directory -Path $packageStagingDir -Force | Out-Null

$requiredFiles = @(
    "$projectName.exe",
    "$projectName.dll",
    "$projectName.deps.json",
    "$projectName.runtimeconfig.json",
    "SharpCompress.dll",
    "System.Management.dll",
    "ZstdSharp.dll"
)

$optionalFiles = @(
    "$projectName.pdb"
)

$requiredDirectories = @(
    "Data",
    "runtimes"
)

Write-Host "Staging release payload for version $packageVersion..."
foreach ($file in $requiredFiles) {
    $source = Join-Path $buildOutputDir $file
    if (-not (Test-Path $source)) {
        throw "Required file missing from build output: $source"
    }

    Copy-Item -LiteralPath $source -Destination (Join-Path $packageStagingDir $file)
}

foreach ($file in $optionalFiles) {
    $source = Join-Path $buildOutputDir $file
    if (Test-Path $source) {
        Copy-Item -LiteralPath $source -Destination (Join-Path $packageStagingDir $file)
    }
}

foreach ($dirName in $requiredDirectories) {
    $sourceDir = Join-Path $buildOutputDir $dirName
    if (-not (Test-Path $sourceDir)) {
        throw "Required directory missing from build output: $sourceDir"
    }

    Copy-Item -LiteralPath $sourceDir -Destination (Join-Path $packageStagingDir $dirName) -Recurse
}

$projectLicense = Join-Path $projectDir "LICENSE"
if (Test-Path $projectLicense) {
    Copy-Item -LiteralPath $projectLicense -Destination (Join-Path $packageStagingDir "LICENSE")
}

$projectReadme = Join-Path $projectDir "README.md"
if (Test-Path $projectReadme) {
    Copy-Item -LiteralPath $projectReadme -Destination (Join-Path $packageStagingDir "README.md")
}

# Remove volatile runtime files that can appear after local app execution.
Get-ChildItem -Path $packageStagingDir -Recurse -File | Where-Object {
    $_.Name -like "*.settings.json" -or
    $_.Name -like "*.log" -or
    $_.Name -eq "error.log" -or
    $_.Name -eq "errors.log"
} | Remove-Item -Force

$topLevelEntries = Get-ChildItem -Path $packageStagingDir -Force | Select-Object -ExpandProperty Name
if ($topLevelEntries -contains "win-x64") {
    throw "Unexpected stale top-level folder detected in staged package: win-x64"
}

if (-not (Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
}

if (Test-Path $packagePath) {
    Remove-Item -LiteralPath $packagePath -Force
}

Write-Host "Creating archive: $packagePath"
Compress-Archive -Path (Join-Path $packageStagingDir "*") -DestinationPath $packagePath -CompressionLevel Optimal

# Publish a checksum next to the package. The executable is unsigned, so this is how
# someone verifies the download is the file that was actually built here.
$checksumPath = "$packagePath.sha256"
$hash = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash *$([System.IO.Path]::GetFileName($packagePath))" | Set-Content -LiteralPath $checksumPath -Encoding ascii

Write-Host "Release package created successfully."
Write-Host "Package:  $packagePath"
Write-Host "SHA-256:  $hash"
Write-Host "Checksum: $checksumPath"
Write-Host "Contents:"
Get-ChildItem -Path $packageStagingDir -Force | ForEach-Object { Write-Host " - $($_.Name)" }
