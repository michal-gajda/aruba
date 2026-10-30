# Windows installer

## Build

Requires Windows x64, PowerShell 7, .NET 10 SDK, WiX 7.0.1, and full Git history.
Review https://docs.firegiant.com/wix/osmf/ before accepting the WiX EULA.
The build script sets `WIX_EULA_ACCEPTED=wix7` and passes `-acceptEula wix7` to WiX.
Running it accepts the WiX 7 EULA; no separate per-machine acceptance is required.

From the repository root:

```powershell
./installer/Build-Installer.ps1
```

The self-contained MSI is written to `artifacts/installer`. The target machine
does not need a separate .NET runtime. MSI uses the first three numeric components
of the NBGV-generated file version; prerelease builds are for testing only.
Keep the UpgradeCode in Aruba.wxs unchanged for future upgrades.

## GitHub Actions and GitFlow

The workflow pins WiX to 7.0.1 and sets `WIX_EULA_ACCEPTED=wix7` in the build job.
No repository variable is required. This acceptance does not waive applicable OSMF fees.

Pushes to develop, master and release/** build MSI artifacts. Stable tags vMajor.Minor.Patch
also publish the MSI to GitHub Releases. Develop remains the next prerelease line;
do not replace its version with the stable release version.

The release commit must contain the installer files and Windows Service support.
Do not rebuild old tags that predate these files or move an already published tag.
Create a new release commit and tag instead. The workflow can also be dispatched
from master with release_tag set to an existing stable tag containing these files.
The script rejects a release tag that does not match the compiled application version.

## Installation

Run the MSI as an administrator. Files are installed under Program Files/Aruba.
The Aruba service runs automatically as NT AUTHORITY/LocalService and listens on
http://localhost:5000. No firewall rule or external network access is enabled.
Stop/start and service removal are handled by MSI during upgrades/uninstallation.
Application files, including appsettings.json, are installer-managed; do not keep
mutable configuration or data there. Configure deployment-specific settings using
machine-level ASPNETCORE_ environment variables and restart the service.

Example commands from an elevated terminal (replace the filename):

```powershell
msiexec.exe /i Aruba-1.1.3-win-x64.msi /L*v install.log
Get-Service Aruba
Invoke-RestMethod http://localhost:5000/weatherforecast
msiexec.exe /x Aruba-1.1.3-win-x64.msi /L*v uninstall.log
```

The MSI is unsigned. Production distribution may require a code-signing certificate.
Verify installation, upgrade, service startup and uninstallation on a disposable
Windows machine before shipping.