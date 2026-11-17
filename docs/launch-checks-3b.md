# Launch checks of stage 3b (.NET 10 x64): the checklist for the owner

The stage 3b gate (`docs/specs/net10-plan.md`, the 3b full-test gate paragraph and section 6's "Stage 3b (net10)" item) has checks that
no automatic test run can make: they need an elevated PowerShell, a clean Windows, the desktop, real hardware or a vendor's software. With
`net10/switch` merged into `main` (decision 25), they are **launch checks**: each is done once before the office release, on a test PC,
never at the office. This page lists every one of them, made as automatic as the owner's rules allow (the scripts here are run by the
owner, never by an agent: they install a service, start Windows Sandbox or answer to a UAC prompt).

For each check: what it proves, the exact command or steps, how long it takes, what it changes on the PC and how that is undone, and what
to send back.

| # | Check | Who | Automatic? |
|---|---|---|---|
| 1 | [WebView2 windows (window check D7)](#1-webview2-windows-window-check-d7) | the lead, at the next connected desktop session | yes (opt-in test) |
| 2 | [The Middle Tier as a Windows service](#2-the-middle-tier-as-a-windows-service-test-middletierserviceps1) | the owner, elevated | yes (`tools\Test-MiddleTierService.ps1`) |
| 3 | [Clean install on a Windows without .NET](#3-clean-install-in-windows-sandbox-test-cleaninstallps1) | the owner (turns Windows Sandbox on once) | yes (`tools\Test-CleanInstall.ps1`) |
| 4 | [Self-update from a standard user](#4-self-update-from-a-standard-user) | a person, with an administrator for the UAC prompt | no |
| 5 | [A Direct message through the LDAP resolver](#5-a-direct-message-through-the-ldap-resolver) | a person, with the office's Direct provider | no |
| 6 | [TWAIN capture with the clinic's real sensor](#6-twain-capture-with-the-clinics-real-sensor) | a person, at the sensor | no |
| 7 | [Each helper link with the vendor's software](#7-each-helper-link-with-the-vendors-software) | a person, on a test PC with the software | no |
| 8 | [The screenshot pass's items for a person](#8-the-screenshot-passs-items-for-a-person) | a person, at a desktop | no |

The installer test of S3b-7 (`tools\Test-Installer.ps1`, the owner's elevated run on this PC) is not repeated here: its run of
2026-11-16 is recorded in `docs/port-notes/S3b-7.md`. Check 3 is its counterpart on a clean Windows.

## 1. WebView2 windows (window check D7)

**What it proves.** That the program's WebView2 windows work on .NET 10 x64: the e-mail preview (the program's `ODWebView2`, which is
Microsoft.Web.WebView2.WinForms) and the program's WPF WebView2 control (`WpfControls.UI.WebView2` around `Microsoft.Web.WebView2.Wpf`'s
control). `Microsoft.Web.WebView2.Wpf` references WinRT.Runtime and Microsoft.Windows.SDK.NET, which neither the release nor .NET 10's shared
framework has (LibraryBindingTests' known gaps, `tests/LibraryBindingTests/ReleaseListTests.cs`); the check proves at run time that the
program never needs them: both browsers start, their pages load, the three WebView2 assemblies load from the program folder and no
reference of theirs goes unresolved.

**Who and when.** The lead, at the next session where the owner's desktop is connected (WebView2 renders only in a connected session; the
in-process window checks run in the program's own process on the desktop, `docs/port-notes/WINCHK-HEADLESS.md`).

**Steps.** In the worktree of `main` (or the branch to check), after a full x64 lane build of `OpenDental.Local.slnf`:

```
set OPMS_WINDOW_CHECKS=1
set OPMS_WINDOW_CHECKS_DUMP=<the synthetic dump of the window-check runs (WINCHK-5 section 6)>
set OPMS_WINDOW_CHECKS_GROUPS=D
set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests
```

D7 (`DETests.D7_WebView2Windows`, the script `tests/WindowChecksTests/InProc/InProcChecks.D7.cs`) opens a synthetic received HTML e-mail
in Edit Email (patient #1 of the scratch copy, never saved or sent) and `FrmTestSplitters`, the program's Frm with the WPF WebView2 control
and a local page; waits until each page has loaded (its `readyState` and text, read through `ExecuteScriptAsync`); pictures each window
with PrintWindow (`D7-email.png`, `D7-wpf.png`) and each page with the browser's own `CapturePreviewAsync` (`D7-email-page.png`,
`D7-wpf-page.png`). It sends no input and loads nothing from the network.

**How long.** About 3 minutes for group D (D1 to D7), once the build is there.

**What it changes.** Like every window-check run: a `winchk_` scratch copy of the synthetic database on 127.0.0.1:3306, dropped at the
end; the results folder (`%TEMP%\stage1-windows-headless` or `OPMS_WINDOW_CHECKS_OUT`), kept. D7 saves nothing (it checks that the number
of e-mails is unchanged). The browsers use the program's shared WebView2 data folder (`%LOCALAPPDATA%\OpenPMS-Dental\WebView2`), as the
program does.

**Send back.** The results file of the run, the D7 row of `REPORT.md` (PASSED, PARTLY or FAILED, with its lines) and the four D7 pictures.
Without the opt-in, D7 is inconclusive and its self-tests (`WebView2PagesTests`) run in every test run.

## 2. The Middle Tier as a Windows service (Test-MiddleTierService.ps1)

**What it proves.** The gate's "the new Middle Tier is checked for real on the build PC: started as a Windows service on a loopback address
with a test certificate, with the host round-trip tests (S3b-6) and a client session run against it" (`docs/port-notes/S3b-6.md`
section 7, which the script automates): the self-contained x64 server runs as a Windows service, Kestrel serves HTTPS with a certificate
from a PFX file, the endpoint answers (405 for a GET, 413 above the size limit), and MiddleTierHostTests' round trips pass through it.

**Before.** Build the server and the tests: the full x64 lane build of `OpenDental.Local.slnf`, or the projects `OpenPmsDentalServer` and
`MiddleTierHostTests` (a Release build of the server works too; the script takes `OpenPmsDentalServer\bin\Release` when it is there, else
`bin\Debug`). The local MariaDB answers on 127.0.0.1:3306 as root with a blank password.

**Command.** Open Windows PowerShell with "Run as administrator" (64-bit, from the Start menu) and paste this one line, with `<repo>` the
repository folder:

```
powershell -NoProfile -ExecutionPolicy Bypass -File "<repo>\tools\Test-MiddleTierService.ps1"
```

**The owner's choice (2026-11-17): run it without `-ClientSession`**, so no test certificate enters Trusted Root; the program's login
through a Middle Tier is checked by hand at a test PC instead. For reference only: `-ClientSession` adds the client session: the script then waits while you start the program
(`<repo>\OpenPmsDental\bin\Debug\OpenPmsDental.exe`), choose the Middle Tier address it prints, log in as `s3b6usera` (its password is in
`tests\MiddleTierHostTests\HostTestRun.cs`), open a patient and close the program, and asks you whether that worked. `-PlanOnly` prints
every step with its names and changes nothing (no elevation needed).

**How long.** About 3 to 5 minutes (copying the server folder, the service start, the test run of about a minute); with the client
session, the time you take.

**What it changes, and undoes** (also after a failure or Ctrl+C; the script's header lists it):
- a new temp folder `%TEMP%\OpenPmsDentalServer-svctest-<id>` with the server copy, the test certificate as a PFX file (made in memory with
  .NET, written only there, never put into a certificate store), `appsettings.json` (https://127.0.0.1 on a free port) and
  `OpenDentalServerConfig.xml`: deleted at the end;
- the Windows service `OpenPmsDentalServer-Test` (sc.exe, start on demand, LocalSystem): stopped and deleted at the end. If a service of
  that name exists before the run, the script refuses and changes nothing;
- a scratch database `mtsvc_<time>_<number>` on 127.0.0.1:3306, built by the host tests: dropped at the end;
- the Event Log source key of `OpenPmsDentalServer`, if the service registers it: deleted at the end when it was not there before (the
  service's events stay in the Application log and are saved in the logs' folder);
- with `-ClientSession` only: the certificate's public part in `Cert:\LocalMachine\Root` while you test: removed at the end;
- the logs' folder `%TEMP%\OpenPmsDentalServer-svctest-logs-<id>` (or `-OutDir`), kept.

Windows keeps the key of a PFX that a service loads in a machine key folder; the script names any key file that is new there at the end
("note" line) and deletes nothing.

**Send back.** The last lines (`N checks passed, M failed. Logs: <folder>` and the exit code), any FAIL line, and the logs' folder:
`host-tests.txt` (with its `.logs` folder), `service-events.txt` and the `sc-*.txt` files. Expected: every check `ok`, MiddleTierHostTests
16 passed and 3 inconclusive (the in-process-only tests), NoHqTests without a failure.

## 3. Clean install in Windows Sandbox (Test-CleanInstall.ps1)

**What it proves.** The gate's "the release is built, installed on a clean VM without any .NET": the 64-bit setup installs on a Windows
that has no .NET runtime at all, installs exactly the staged files, the installed programs run on their own runtime (coreclr.dll and
hostfxr.dll from the install folder), setup's `/PREREQS=download` installs the x64 Visual C++ runtime, and the uninstall removes the
installation. Windows Sandbox is that clean Windows: a fresh copy of this PC's Windows without its programs, discarded when it closes.

**Once: turn Windows Sandbox on.** It needs Windows 11 Pro (or Enterprise or Education) and virtualization on in the firmware. In Windows
PowerShell with "Run as administrator":

```
Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All
```

Windows asks for a restart. The script checks this itself (`WindowsSandbox.exe` in System32) and prints the same command when the feature
is off, changing nothing.

**Before.** A Release dry run: `tools\Build-Release.ps1 -OutDir <dryrun>` (as for Test-Installer), which writes `<dryrun>\staging` and
`<dryrun>\OpenPmsDental-<version>-Setup.exe`. The sandbox needs the network (setup downloads the Visual C++ runtime from Microsoft).

**Command.** In Windows PowerShell (no "Run as administrator" needed), with `<repo>` the repository folder and `<dryrun>` the dry run's
output folder:

```
powershell -NoProfile -ExecutionPolicy Bypass -File "<repo>\tools\Test-CleanInstall.ps1" -Setup "<dryrun>\OpenPmsDental-<version>-Setup.exe" -Staging "<dryrun>\staging"
```

The Windows Sandbox window opens and works by itself: inside it, `Test-CleanInstall-InSandbox.ps1` records that there is no .NET runtime,
runs the setup silently (`/VERYSILENT /SUPPRESSMSGBOXES /PREREQS=download`, a database server on the sandbox's own computer so no
connection settings tool runs), compares the install folder with the staging folder (SHA-256), runs the installed
`OpenPmsDentalDbSettings.exe /check` and then its window (which connects to nothing) to see where coreclr.dll and hostfxr.dll load from,
checks the 64-bit TWAINDSM.dll, uninstalls silently, writes `results.txt` and shuts the sandbox down. Leave the window alone until it
closes. `-PlanOnly` prints the steps and the `.wsb` file and changes nothing; `-KeepSandboxOpen` leaves the sandbox open at the end;
`-TimeoutMinutes` (30) is how long the script waits.

**How long.** About 10 to 20 minutes (the sandbox's start, the Visual C++ download, 557 files installed and hashed, the uninstall).

**What it changes, and undoes.** Nothing outside a new folder `%TEMP%\OpenPmsDental-cleaninstall-<id>`: everything the setup does happens
in the sandbox, which Windows discards when it shuts down. The script deletes the folder's `in\` (the in-sandbox script and `host.txt`)
and `cleaninstall.wsb` at the end and keeps `results\`. The setup and staging folders are mapped read-only.

**Send back.** The last lines (`N checks passed, M failed. Logs: <folder>` and the exit code), any FAIL line, and the results folder
(`results.txt`, `setup.log`, `uninstall.log`, `installed-files.txt`).

## 4. Self-update from a standard user

**What it proves.** The self-update between two .NET 10 builds works from a standard (non-administrator) Windows account: the program
pulls the release, UpdateFileCopier runs elevated from its temp folder with its own runtime after an administrator answers the UAC prompt,
and the program restarts on the new build; with an unchanged runtime only the program part is pulled, with a new runtime patch both parts.
No script can do this: it needs a standard account, a UAC prompt answered by an administrator, a test update server and two builds.

**Steps** (`docs/port-notes/S3b-7.md` section 10, steps 2 to 4, on a test PC and a test update server, never the office's):
1. Publish build A on the test update server; install build A on the test PC (its setup, as an administrator).
2. Log on to the test PC with a **standard** account; start the program once on A.
3. Publish build B with the same runtime. Start the program: it offers the update; the message says nothing about a new runtime. The UAC
   prompt for UpdateFileCopier appears: an administrator answers it. `%TEMP%\openpms-dental\ForkReleaseCopier` holds the copier and its
   runtime (about 218 files) and nothing of the program; the program restarts on B (Help > About).
4. Build C on the next .NET 10 patch (the monthly runtime release); publish; start the program: the message names the new .NET runtime;
   after the update every runtime file of the program folder is C's (Help > About; `ForkRelease.manifest` names C's key), and the helper
   (`Helper32\`) runs on it.
5. A process holding a runtime file: start the connection settings tool from the program folder, then let the program pull: the copier
   ends it first.

**How long.** About an hour with the three builds and their publishing.

**What it changes.** The test PC's installation and the test update server; nothing else. Remove the test installation afterwards if the
test PC is shared.

**Send back.** For each step: what the update message said, that the UAC prompt appeared and who answered it, Help > About's version and
runtime after it, and the file count of the copier's temp folder.

## 5. A Direct message through the LDAP resolver

**What it proves.** The second known gap of LibraryBindingTests: `Health.Direct.ResolverPlugins` (a .NET Framework file) references the .NET
Framework's System.ServiceModel, which .NET 10 does not have. The program uses only its `LdapCertResolver`; the check proves that a Direct
message is sent with a recipient certificate found through LDAP on .NET 10 x64.

**Steps.** On a test PC with a test database (not the office's): Setup > Manage > E-mail, the Direct address of the office's Direct
provider with its certificate (as set up for EHR, `docs/specs/ehr-rebuild-plan.md` section 11); send a Direct message with a synthetic
text to the provider's test recipient whose certificate is published in LDAP (no patient data); check that it is sent and that the
provider confirms delivery (an MDN arrives in the inbox).

**How long.** About 15 minutes once the Direct address is set up.

**What it changes.** One sent Direct message and its MDN in the test database's e-mail tables.

**Send back.** Whether it was sent, the MDN (or the error message, word for word), and the program's log lines of the send.

## 6. TWAIN capture with the clinic's real sensor

**What it proves.** The office release's entry condition for TWAIN (decision 19): capture from the x64 program through the 64-bit
TWAINDSM with the driver project's 64-bit data sources and the clinic's real sensor, a document scan through a scanner's 64-bit driver,
and, if one exists on the test PC, a TWAIN source with only a 32-bit driver through the 32-bit helper (S3b-8). The fake-mode and window
checks (E1 to E4, ImagingCaptureTests) cover everything but the hardware.

**Steps.** On the clinic's test PC with the sensor and the driver project's 64-bit sources installed: in the Images module of a synthetic
patient, select the sensor's source and take an image; then a page with the document scanner; then, if a 32-bit-only source is installed,
take one through it (the program routes it through the helper). Check each image in the Images module (orientation, size, 16-bit depth for
the sensor).

**How long.** About 20 minutes.

**What it changes.** The captured images in the synthetic patient's folder; delete them afterwards.

**Send back.** For each source: its name, whether the image arrived, the image's size and depth, and any message word for word.

## 7. Each helper link with the vendor's software

**What it proves.** Each link that runs through the 32-bit helper (`Helper32\`) from the x64 program works with the vendor's real software
(decision 23: the vendor's 64-bit way first, the helper as the fallback): QuickBooks Desktop, Schick CDR, a Topaz signature pad and Guru.
Helper32Tests cover the helper's protocol without the vendors' software.

**Steps.** On each test PC that has the vendor's software, from the x64 program on a test database: QuickBooks Desktop (send a synthetic
deposit), Schick CDR (open the bridge for a synthetic patient and take or show an image), a Topaz pad (sign in the helper's Topaz window and
show the stored signature), Guru (open the bridge for a synthetic patient). Skip a link whose software no test PC has, and say so.

**How long.** About 10 minutes per link.

**What it changes.** The synthetic rows each link writes in the test database and the vendor's own test data.

**Send back.** For each link: done or skipped, and any message word for word.

## 8. The screenshot pass's items for a person

**What it proves.** The parts of section 6's "Stage 3b (net10)" item that the window checks cannot do (the "For a person" column of the
coverage table in `docs/port-notes/WINCHK-HEADLESS.md`, WINCHK-5 section 3) and the PARTLY items of its runs (WINCHK-5 section 6).

**Steps.** At a desktop with the x64 build and a test database:
- with Windows "Make text bigger" above 100 %: the default-font windows (splash, clinic picker, progress windows, MsgBoxCopyPaste, ODR
  parameters, dashboard and RDL dialogs);
- at 150 % and on two monitors of different scaling: drag a window between them; WebView2 panes (the Wiki, the e-mail preview) above 100 %;
- real prints to a PDF printer: a sheet, a statement, a claim form and an RDL report (margins, fonts, page counts);
- voice perio charting with a microphone; e-mail receive and send; a clearinghouse send to its sandbox; a Word letter merge;
- RdlDesign: add a text box on the design surface and save; DocumentationBuilder's Build;
- the DbSettings, ServiceManager and UpdateFileCopier windows (each asks for administrator rights);
- the Snipping Tool start of the claim attachment windows and the on-screen keyboard from the patient search;
- the PARTLY items: compare A1's and the B pictures with stage 0's (A1, A5, A9, B1, B6), C15's exit time after the Wiki (File > Exit),
  C22's DateTimePicker drop-down, C24's text and red underline, C26's EOB viewer strips, D3's underline, D6's keyboard focus in a Frm,
  E1 to E4's labels, F1 and F5's graphs, G1 paging, G3 and G4's pages, H1's text box and save, H2's preview messages, I1 to I8's chart
  pictures and the corner drag, K2's Build.

**How long.** About half a day for the whole list.

**What it changes.** Rows in the test database (prints, sends, merges); nothing on the office's systems (use sandboxes and test addresses).

**Send back.** For each item: as before, or what differs (a picture helps).

## For the lead

- Package LAUNCH-3b (branch `net10/launch3b-1`): the scripts of checks 2 and 3 (`tools/Test-MiddleTierService.ps1`,
  `tools/Test-CleanInstall.ps1`, `tools/Test-CleanInstall-InSandbox.ps1`), window check D7 with its self-tests, MiddleTierHostTests'
  `mtsvc_` database prefix, and this page with pointers from `docs/port-notes/S3b-6.md` section 7 and the WINCHK-5 coverage table.
- Tested without anything elevated, no service, no Windows Sandbox start and no certificate store: the scripts' plain-code parts
  (dot-sourced: the `.wsb` XML, the results reader and writer, the settings and the PFX file, the test-run reader), their `-PlanOnly`
  steps and their refusals (`tests/ReleaseFlowTests/LaunchCheckScriptTests.cs`), D7's self-tests (`WebView2PagesTests`). The owner's
  scripts are in the ASCII and no-secret check (`OwnerScripts_AreAscii_AndHoldNoSecret`).
- Not run: D7 itself (the owner's desktop was disconnected), the two scripts for real (elevated, a service, Windows Sandbox).
