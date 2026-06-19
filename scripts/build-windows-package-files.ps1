[CmdletBinding()]
param(
    [string]$Version = "10.0.4"
)

$ErrorActionPreference = "Stop"

$rootDirectory = Split-Path -Parent $PSScriptRoot
$project = Join-Path $rootDirectory "src\Plugin.InAppBilling\Plugin.InAppBilling.csproj"
$targetFramework = "net10.0-windows10.0.19041.0"
$buildDirectory = Join-Path $rootDirectory "src\Plugin.InAppBilling\bin\Release\$targetFramework"
$packageDirectory = Join-Path $rootDirectory "nuget\package-files\lib\$targetFramework"

dotnet build $project `
    --configuration Release `
    "-p:TargetFrameworks=$targetFramework" `
    "-p:Version=$Version" `
    "-p:PackageVersion=$Version" `
    "-p:AssemblyVersion=$Version.0" `
    "-p:AssemblyFileVersion=$Version.0" `
    -p:PackOnBuild=false `
    -p:GeneratePackageOnBuild=false

if ($LASTEXITCODE -ne 0) {
    throw "Windows billing assembly build failed with exit code $LASTEXITCODE."
}

New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null

foreach ($fileName in @("Plugin.InAppBilling.dll", "Plugin.InAppBilling.xml")) {
    $source = Join-Path $buildDirectory $fileName
    if (-not (Test-Path -LiteralPath $source)) {
        throw "Expected Windows build output was not found: $source"
    }

    Copy-Item -LiteralPath $source -Destination $packageDirectory -Force
}

$assemblyPath = Join-Path $packageDirectory "Plugin.InAppBilling.dll"
$assemblyVersion = [System.Reflection.AssemblyName]::GetAssemblyName($assemblyPath).Version.ToString()
$expectedAssemblyVersion = "$Version.0"
if ($assemblyVersion -ne $expectedAssemblyVersion) {
    throw "Staged assembly version is $assemblyVersion; expected $expectedAssemblyVersion."
}

Write-Host "Windows NuGet files staged in $packageDirectory"
Get-ChildItem -LiteralPath $packageDirectory | Select-Object Name, Length, LastWriteTime
