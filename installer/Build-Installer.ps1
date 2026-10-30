param(
    [string] $ExpectedTag
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$env:WIX_EULA_ACCEPTED = 'wix7'

if (-not $IsWindows) { throw 'The MSI build requires Windows.' }
Get-Command wix -ErrorAction Stop | Out-Null

$root = Split-Path $PSScriptRoot -Parent
$publishDir = Join-Path $root 'artifacts/windows/publish'
$outputDir = Join-Path $root 'artifacts/installer'
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }
New-Item $outputDir -ItemType Directory -Force | Out-Null

dotnet publish (Join-Path $root 'src/WebApi/Aruba.WebApi.csproj') --configuration Release --runtime win-x64 --self-contained true --output $publishDir
if ($LASTEXITCODE -ne 0) { throw 'Publishing the Windows application failed.' }

$metadata = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $publishDir 'Aruba.WebApi.dll'))
$version = '{0}.{1}.{2}' -f $metadata.FileMajorPart, $metadata.FileMinorPart, $metadata.FileBuildPart
if ($metadata.FileMajorPart -gt 255 -or $metadata.FileMinorPart -gt 255 -or $metadata.FileBuildPart -gt 65535) {
    throw "Version $version exceeds MSI version limits."
}
if ($ExpectedTag) {
    if ($ExpectedTag -cnotmatch '^v\d+\.\d+\.\d+$' -or $ExpectedTag -cne "v$version") {
        throw "Release tag $ExpectedTag does not match the stable MSI version v$version."
    }
    if ($metadata.ProductVersion -notmatch ('^' + [regex]::Escape($version) + '(\+|$)')) {
        throw 'Only stable application builds can be published as a GitHub Release.'
    }
}

wix build (Join-Path $PSScriptRoot 'Aruba.wxs') -arch x64 -d "PublishDir=$publishDir" -d "ProductVersion=$version" -o (Join-Path $outputDir "Aruba-$version-win-x64.msi") -acceptEula $env:WIX_EULA_ACCEPTED
if ($LASTEXITCODE -ne 0) { throw 'Building the MSI failed.' }