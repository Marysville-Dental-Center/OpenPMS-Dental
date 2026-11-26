# Installing in an office

How an office installs Open PMS - Dental: the database server, the workstations and updates. Building from source is in [building.md](../develop/building.md); the release process is [release-process.md](../develop/release-process.md).


A release is a setup program, `OpenPmsDental-<version>-Setup.exe`, with its `SHA256SUMS.txt` and release notes ([docs/release-notes/v0.1.md](../release-notes/v0.1.md) for 0.1, [v0.0.md](../release-notes/v0.0.md) for 0.0). `tools/Build-Release.ps1` builds it from a **Release | x64** build ([docs/develop/release-process.md](../develop/release-process.md#building-the-installer)). Nothing here contacts Open Dental's servers, and nothing is installed on the database server by this program.

## Database server

The office installs [MariaDB Server](https://mariadb.org/download/) **10.11** itself. These settings go in the `[mysqld]` section of `my.ini` (Windows) or `/etc/mysql/mariadb.conf.d/50-server.cnf` (Debian and Ubuntu):

```ini
[mysqld]
sql_mode=
default-storage-engine=MyISAM
max_allowed_packet=40M
```

- `sql_mode` must be blank. If it isn't, the program runs `SET GLOBAL sql_mode=''` at startup, which needs an account with that privilege.
- The tables are MyISAM; with another default engine, new tables end up in a different engine.
- `max_allowed_packet` must be at least 40 MB. Smaller values make large inserts (images, releases stored in the database) fail with "Please add the following to your my.ini file: max_allowed_packet=40000000".
- Give each office its own database account, and back the database up.
- A server the workstations reach over the network must have TLS with a certificate from the office's own CA, and each workstation needs that CA certificate and the client certificate: see [docs/admin/db-connection-security.md](db-connection-security.md), which also gives the order for an office that runs version 0.0.

**A new office's database:** see [db/README.md](../../db/README.md) and the script `db/init/new-database.sql`.

**An existing Open Dental database** (Open Dental 24.3.63, 25.3.48 or 26.2 only) is adopted once, on a restored copy: see [Adopting a database](../develop/release-process.md#adopting-a-database) and the office procedure in [docs/admin/adoption-runbook.md](adoption-runbook.md).

## Workstations

Prerequisites:

- **Windows 11, 64-bit** (the baseline). The setup also runs on Windows 10 version 1903 or later, after a warning: Microsoft supports .NET 10 on Windows 10 only in some editions ([its list](https://learn.microsoft.com/en-us/dotnet/core/install/windows)).
- **No .NET to install.** The program, its tools and the 32-bit helper carry their own .NET 10 runtime (x64 beside the programs, x86 in the `Helper32` sub folder); the setup installs them with the program, also in a silent install.
- **Microsoft Visual C++ 2015-2022 Redistributable, x64** (`vc_redist.x64.exe`; `Direct2dWrapper.dll` needs it)
- **Microsoft Edge WebView2 Runtime** (Evergreen; Windows 11 already has it). Every window that shows a page uses it: the wiki, email previews, PDFs in the Imaging module, vendor pages. The program keeps its WebView2 data per Windows user in `%LOCALAPPDATA%\OpenPMS-Dental\WebView2`.
- For scanners and sensors: the device's own 64-bit TWAIN driver. The setup installs the TWAIN Working Group's 64-bit TWAIN data source manager (`TWAINDSM.dll`, into the Windows system folder) when it is missing or older.

When the Visual C++ runtime is missing, the setup offers to download Microsoft's installer from Microsoft and run it, or to skip it (`/PREREQS=ask|download|skip`). The setup only checks for the WebView2 Runtime: when it is missing, the setup says so and how to install it (Microsoft's Evergreen Bootstrapper or Standalone Installer, Windows Update or device management), and it never downloads it.

Version 0.0, the .NET Framework 4.8 build, needed the **.NET Framework 4.8**, the **Microsoft Visual C++ 2015-2022 Redistributable, x86** (`vc_redist.x86.exe`) and the **Microsoft Edge WebView2 Runtime, x86**, and installed into `C:\Program Files (x86)\Open PMS - Dental\` ([docs/release-notes/v0.0.md](../release-notes/v0.0.md)). The .NET 10 setup is a separate 64-bit installation; no office runs 0.0, so nothing moves from it.

Steps:

1. Install the program on the **update server** first: the workstation named in the `WebServiceServerName` preference (Setup > Update Setup).
2. Check the setup's SHA-256 against `SHA256SUMS.txt`, then run it as an administrator. It installs into `C:\Program Files\Open PMS - Dental\` (64-bit), creates the Start menu shortcut "Open PMS - Dental", and writes `FreeDentalConfig.xml` from its database connection page (server, database, user; never a password). For a database server over the network the next page asks for the office CA certificate and the client certificate (`.pfx`). Setup then opens the connection settings tool, `OpenPmsDentalDbSettings.exe`, which imports the client certificate or stores the database password on this computer with Windows DPAPI; for a database on the same computer it opens in password mode (Cancel when the database user has no password). The setup is not signed, so SmartScreen may warn.
3. Start the program from the Start menu. It never asks for the database password: to change the connection later, an administrator runs `OpenPmsDentalDbSettings.exe` in the program folder.
4. On the update server, the program offers to publish the release at that first start, so the other workstations update to it.
5. On the other workstations repeat steps 2 and 3. A workstation that runs an older build pulls the published one when it starts.

Silent installs, the setup's switches and the uninstall are described in the release notes.

## Updates

A new build reaches the office through the release channel described in [docs/develop/release-process.md](../develop/release-process.md): install it on the update server and start it as an administrator; it publishes the release into the database, and every other workstation pulls it at its next start and restarts through `UpdateFileCopier.exe`. Debug builds never publish or pull. A workstation that is not the update server refuses to run a build newer than the published one (exit code 174), so always update the update server first.
