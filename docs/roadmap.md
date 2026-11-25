# Roadmap

Planned work after v0.1, as agreed with the owner. Each item is discussed and scoped with the owner before work starts.

## Next

Status on 2026-11-21. Of the list scheduled with the owner on 2026-11-07, the Open Dental 26.2 features, **.NET 10** (64-bit, merged on
2026-11-17, `docs/specs/net10-plan.md`), **the tooth chart's 3D drawing** on WPF (TC-0 to TC-10, `docs/specs/wpf-tooth-chart-plan.md`)
and **EHR again** (EHR-1 to EHR-7, `docs/specs/ehr-rebuild-plan.md`; the CQM reports wait for counsel's answer to E-10) are done. The
compressed DICOM decoders stay parked until such files turn up.

Now: **the UI refresh** (owner, 2026-11-17: "Modernize icons, buttons, styles. Keep it clean and professional."); the plan is
`docs/specs/ui-refresh-plan.md`. Done: the tokens and design sheet, the window frame, the controls, the toolbars, every button picture as a
Fluent icon (UI-9a, UI-9b-1 to UI-9b-4), text fit (UI-10) and Segoe UI 9 pt (UI-2b, merged 2026-11-21). After the owner's review of
2026-11-21 ("UI lacking consistence. choice of spacing not symetrical") every window is laid out again on one spacing grid, with the
checkbox on the left of its text (UI-12: a pilot, then every window in slices); this replaces "every window keeping its layout". Then UI-11
(the last colours and the final sweep), the toolbar overflow button and keyboard focus on the WPF buttons.

Version 0.1 was released with all of this (below, "Released in 0.1"). Not scheduled yet: the next release, the office switch-over (after the 64-bit TWAIN drivers are tested on the clinic's sensor), the server
components on Linux, and Open Dental 26.3. The GitHub repository was renamed to OpenPMS-Dental on 2026-11-21 (the old address redirects)
and has a project site on GitHub Pages, built from `site/`.

The owner questions of each package are recorded with their answers in `docs/owner-questions.md`.

## Done in the next version

Built for the version after 0.1; what each one changes is in its release notes, `docs/release-notes/next.md`.

Nothing yet.

## Released in 0.1

Built for 0.1, the first release after 0.0; what each one changes is in its release notes, `docs/release-notes/v0.1.md`. Besides
the items below: .NET 10, 64-bit and self-contained, the tooth chart's 3D drawing on WPF, EHR rebuilt as open code (all three under "Next",
above, and "Plans", below), the UI refresh (`docs/release-notes/ui-refresh.md`) and the imaging work of the owner's imaging design of
2026-11-22 with the sensor driver's twain-contract 1.3 and 1.4.

1. **Library updates (security) and client-side database security**: the tier 1 library updates (SSH.NET, MailKit/MimeKit 4, Newtonsoft.Json
   13, MSAL and the Google APIs) with remembered SSH host keys, and encrypted database connections with the program's own database account.
2. **Library updates (maintenance)**: WebView2 in place of the Internet Explorer control, the managed WeCantSpell.Hunspell spell checker,
   PDFsharp/MigraDoc 6.5 (the spell checker and PDFsharp/MigraDoc built from their source), and the in-house DICOM reader in place of
   fo-dicom.
3. **Routine maintenance around the practice**.
4. **Inventory tracking**.
5. **Storage rename**: `OpenPmsDentalCloud` becomes `OpenPmsDentalStorage`, since it holds only the SFTP storage option.
6. **Fixes before the switch-over, from the owner's answers of 2026-11-07**: a declined PayConnect 2 charge is a decline, an edited X-ray
   attached to a claim is saved, the Insurance History step leaves the frequency code groups unchanged, NDde.dll is removed (the DentX,
   Dexis Integrator and Vipersoft links use an in-house DDE client since 2026-11-08), the capture session is on for new installs, and the
   backup to another server works.
7. **Open Dental 26.2 features**: the changes the owner chose on 2026-11-07, on Open Dental's 26.2.0 database schema (appointments, billing
   and statements, claims and insurance, the perio chart, tools, reports, eForms and recalls, program links, and Glidewell lab cases without
   a connection to Glidewell yet), with Open Dental 25.3's settings read under their own names and its eight options built. The extraction
   graphic and the "text below the chart" paint type stay with the tooth chart.

## Plans

### .NET Framework 4.8 to .NET 10

**Status (2026-11-16): the switch is done on the integration branch `net10/switch`, and reaches `main` with the stage 3b gate.** The
program, its libraries, tools and services are `net10.0-windows` x64, self-contained on one x64 .NET runtime in the program folder, with the
32-bit helper self-contained on its own x86 runtime in `Helper32\` (S3b-1 to S3b-4); the Middle Tier is an ASP.NET Core server (S3b-6); the
release, the 64-bit setup and the self-update are built for it (S3b-7, `docs/port-notes/S3b-7.md`): Build-Release stages the x64 release with
the runtime files read from the runtime packs' own lists and refuses mixed or old runtimes, the setup installs into `Program Files` in
64-bit mode with no .NET runtime to install and the 64-bit TWAIN data source manager, and the release channel publishes the runtime as a
part of its own, pulled only when it changes. Still open before the gate: the test projects' move (S3b-5), the number formatting of
decision 24 (S3b-FMT), the full test run, the Release dry run with the owner's elevated `Test-Installer.ps1`, and the launch checks.

Target: .NET 10, the long-term release, supported until November 14, 2028 (.NET 8 and .NET 9 support ends on November 10, 2026; .NET 12, the
next long-term release, is expected in late 2027). Gains: faster start-up and memory handling, current C#, modern project files and NuGet,
monthly security servicing, better high-DPI support in WinForms, and current libraries (Helix Toolkit 3.x for the tooth chart among them).
Still Windows-only for WinForms and WPF, which fits: the desktop program stays a Windows program.

Decided with the owner on 2026-11-07:

- **Everything moves**, with no project left on .NET Framework and nothing built for both. That includes the Middle Tier: its classic
  ASP.NET web service (one method that hands each request to the business code) becomes a small ASP.NET Core service over HTTPS on
  Windows; the program reaches it with a plain HTTPS call instead of the generated SOAP proxy. The 4,458 business methods that can run
  on the Middle Tier keep their messages unchanged. A Linux Middle Tier is deferred to the Linux server plan (the business library would
  first need its Windows-only parts moved out: pictures, fonts and drawing, WinForms, WPF, the registry).
- **Deployment**: self-contained (owner, 2026-11-08, replacing "framework-dependent"): the program and its 32-bit helper carry their
  own .NET 10 runtime, so nothing is installed for .NET. The updates stored in the database stay small, because a release keeps the
  runtime as a separate part, published and pulled only when the runtime version changes. The runtime's security fixes then come only
  with this program's releases: a release follows each monthly .NET 10 patch.
- **Libraries**: NuGet packages replace the DLL files in `Required dlls`, from a local package folder the build lane restores from offline,
  with lock files; each batch of downloads is approved by the owner. The generated SOAP clients of services the fork removed are deleted and
  the rest regenerated or replaced; any BinaryFormatter use is replaced; tests built on AppDomains are rewritten; COM interop stays.
- **Stages**, in this order (owner, 2026-11-08), each ending with a full test run: a clean-up of dead code and unused references; SDK-style
  project files, still on .NET Framework 4.8; NuGet packages; MSTest v1 to MSTest 4; the code changes that also work on
  .NET Framework, the 64-bit preparation among them, ending with the last .NET Framework release (the "bridge release"); then the
  program, the business library, the tests and the Middle Tier on .NET 10 and 64-bit in one step (owner, 2026-11-08: "no go directly to
  64."), with no 32-bit .NET 10 version (see "32-bit to 64-bit").
- **Switching the office**: the new 64-bit setup once on every workstation; it installs into `Program Files`, with the program's own
  .NET 10 runtime (nothing is downloaded for .NET), and has no "install for me only" mode. Windows 11 is the baseline. Changed on
  2026-11-15 (decisions 21 and 22 of the plan): no bridge release, and no OpenPMS install to migrate (nothing in production runs this
  program; the office switches from Open Dental once every workstream is done), so the setup is built for new installs and the
  self-update for updates from one .NET 10 release to the next.

The stage plan, made from a read-only survey of every project, library, API and tool the move touches and an independent critique, is
`docs/specs/net10-plan.md`. Its owner questions were answered on 2026-11-08 (`docs/owner-questions.md`, "The move to .NET 10"). The
owner's standing rule: nothing is cut, tests included, except what the owner deliberately deleted. So every integration moves to .NET 10,
QuickBooks Desktop, Schick, Topaz, the CallFire, Divvy and BCBS Georgia dial-up links, the report engine's SQL Server, ODBC and OLE DB
data sources, Direct secure messaging, Change Healthcare (Emdeon) medical claims, voice perio charting and the plugin loader among them.

### The tooth chart's 3D drawing

Today the chart has three drawing engines in `SparksToothChart`: DirectX 9 through SharpDX (unmaintained since 2019; needs Microsoft's legacy
DirectX End-User Runtime, which a clean Windows does not have), OpenGL through Tao (unmaintained since about 2008) and a flat 2D chart. Open
Dental's newer Sparks3D engine was closed source and was removed, so free text and orthodontic hardware are not drawn.

Plan (owner, 2026-11-07): a WPF 3D chart built with Helix Toolkit 3.x (MIT, the most widely used 3D toolkit for .NET desktop programs), on
.NET 10, replacing the DirectX and OpenGL engines; the flat 2D chart stays as an option. WPF draws 3D through Windows' own DirectX and falls
back to software drawing on any PC, so no extra runtime is needed. It reuses the existing tooth models (`.obj`) and the chart's selection and
procedure-colour logic, matches today's 3D look, adds free text, orthodontic hardware, Open Dental 26.2's extraction graphic and its "text
below the chart" paint type, and covers the Chart and Treatment Plan modules, the big chart window, the graphical perio chart and charts
printed on sheets. A saved DirectX or OpenGL choice moves to the new chart. It retires SharpDX, Tao and the DirectX runtime requirement, and
it does not depend on 32-bit. Tests render the chart to images and compare them, and the owner checks a gallery of rendered charts before it
reaches main.

### 32-bit to 64-bit

The program is built as 32-bit today. Decided: go to full 64-bit, with no interim large-address-aware flag (owner, 2026-11-07), and
directly, as part of the switch to .NET 10 (owner, 2026-11-08: "no go directly to 64."). 64-bit is no longer a later stage: there is no
32-bit .NET 10 version. The office installs the last .NET Framework release, then the 64-bit .NET 10 setup once on every workstation.
The 64-bit preparation that also works on .NET Framework is done before the switch, while the program is still 32-bit.

- **TWAIN**: a 64-bit program uses the 64-bit TWAIN data source manager (TWAINDSM 64-bit, free) and needs 64-bit TWAIN data sources. The
  clinic's sensor and camera drivers are a thin 32-bit data source (`.ds`) that starts a separate 32-bit helper program for the device work,
  which uses the sensor vendor's 32-bit libraries. The TWAIN driver project builds 64-bit `.ds` files that start the same 32-bit helpers;
  the helpers and the vendor libraries stay 32-bit, because processes of different bitness can talk to each other (the driver project
  confirmed on 2026-11-07 that nothing in its design stops this). Installed side by side with the 32-bit ones. Decided (owner,
  2026-11-07): this program's installer installs the 64-bit TWAINDSM; the driver package only checks that it is there. The 64-bit data
  sources (version 1.2) are built; they are packaged and tested on the clinic's sensor before the office gets the 64-bit release.
- **Other 32-bit-only components** (QuickBooks Desktop's SDK, the Schick CDR interface, Topaz, and Guru's interface DLL): kept (owner
  correction, 2026-11-08), although the office uses none of them. Each gets a small 32-bit helper program that the 64-bit program talks
  to, or a 64-bit replacement where the vendor has one (checked per vendor). The helper is built and tested before the switch, while the
  program is still 32-bit.
- **Other TWAIN devices** (document scanners, a second sensor) need 64-bit drivers; most current devices ship both. For a device with only a
  32-bit driver, an optional 32-bit TWAIN bridge helper loads that driver and passes the images on.
- **Native DLLs**: the library updates removed most of them (the spell checker is managed); `Direct2dWrapper` is built 64-bit from our
  source; WebView2 has 64-bit versions. fo-dicom and its native codecs are gone: the DICOM reader is managed code of our own.
- **Code**: P/Invoke signatures and structures (the TWAIN structures above all) are made pointer-size-safe, with tests that run in both
  bitnesses, before the switch and still on .NET Framework; registry reads of 32-bit vendor keys name the 32-bit view instead of relying
  on `WOW6432Node` redirection.
- **Installer and updates**: 64-bit install folder and prerequisites, and a defined way for an office to move from a 32-bit install to a
  64-bit one, including a mixed set of workstations during the switch: the new setup carries the settings file over and removes the old
  32-bit install, and a workstation not switched yet stays on the last .NET Framework release, which refuses the 64-bit release and says
  to run the new setup.

Prerequisites of the switch: the 64-bit preparation before it; the 64-bit `.ds` files from the TWAIN driver project, tested on the
clinic's sensor; and the owner's approval of the TWAINDSM download, asked when the switch starts. The old DirectX and OpenGL chart engines
are managed code for any processor and stay in 64-bit until the WPF tooth chart replaces them; a mode that cannot start falls back, down
to the 2D drawing, as today.

### Compressed DICOM decoders

Only if such files turn up: JPEG Extended and Lossless, JPEG 2000 and JPEG-LS, through our own thin layer over libjpeg-turbo, OpenJPEG and
CharLS, plugged into the in-house reader's decoder hook. Today the reader opens uncompressed DICOM, RLE and JPEG Baseline, and names the
format of any other compressed file. Background: docs/reviews/2026-11-02-dicom-codecs-research.md.

### Not scheduled yet

- The next release and the office switch-over (`docs/adoption-runbook.md`).
- Server components on Linux, new ones in Rust: a plan first (image storage, backups on the server, publishing releases from the server,
  scheduled jobs). The Middle Tier is the exception: it moves to .NET 10 on Windows now, and a Linux Middle Tier waits for this plan (owner,
  2026-11-07: "Defer Linux middle tier").
- Open Dental 26.3, once Open Dental marks it stable.
- A sensor's driver settings saved per device in the database (owner, 2026-11-24). Today Capture settings keeps them in each computer's
  capture.ini, so a USB sensor moved between computers needs the same `Source.*` values entered on each one. They belong to the sensor,
  so storing them with the imaging device would let every computer use them after one save. Only what is truly local would stay per
  computer: the driver install, the sensor's calibration files, and the hardware binding. Needs moving any values already saved in
  capture.ini, and keeping the gated-settings check and its log working across computers.
