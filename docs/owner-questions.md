# Owner questions from the 24.4-26.1 port

**2026-11-07:** every other open question is accepted at today's behaviour (owner, 2026-11-07: "skip the rest"). The questions the owner answered that day say so below ("Answered (2026-11-07)").

This list holds every question the 55 package notes on `port/integration` still leave open for you, plus the extra questions in the phase B results. Questions that a later note or integration section already settled are left out; they are listed at the end. Each question names its package and the question number in that package's note (for example "B-ERA 2"), and says what the program does today if you change nothing.

**[SWITCH-OVER]** marks a question that changes the office's data during the switch-over, or that you should settle before it: one-time conversions, steps that change the office's existing data, and defaults that change billing or claims.

The v0.0 release work added the open questions of its package notes (ORG-CLEANUP, INIT-SQL, BIN-A, BIN-B, TWAIN-W, REL-FILES, REN-PROOF, CLAIM-DCM, RDL-SRC, M16-INTEGRATION and RENAME), each under its area, and the section below: the decisions that were taken by default for v0.0.

---

## Decided by default for v0.0

These are in v0.0 as described. Each one is a default, not a final answer: **change it if you disagree**, and say which.

1. **The rename identifiers** (RENAME, REN-PROOF 1 and 2). The code names are `OpenPmsDental` (`OpenPmsDental.exe`, `OpenPmsDentalBusiness.dll`, `OpenPmsDentalGraph`, `OpenPmsDentalStorage` (`OpenPmsDentalCloud` in v0.0; the storage rename of 2026-11-02), `OpenPmsDentalServer`, `OpenPmsDentalHL7`). The run-time folders are `OpenPMS-Dental`, the temp folder `openpms-dental`, the Event Log source `OpenPMS-Dental`. The solution files keep their names (`OpenDental.sln`, `OpenDental.Local.slnf`), and so do `FreeDentalConfig.xml`, the database and every stored value. Window sizes, Query Monitor filters and the Waiting Room position start fresh; they are not read from the folder that builds before the rename used under `%LOCALAPPDATA%`. Change it if you disagree.
2. **The product name at display time** (REL-FILES D2 and question 6). Where an adopted database still holds Open Dental's default main window title or software name, or one of the two values the Benco switch wrote, the program shows "Open PMS - Dental". An office's own title is shown as stored. The rows are only read, never rewritten. Change it if you disagree.
3. **The Benco branding toggle is removed** (REL-FILES D7). Enabling the Benco program link no longer switches the title and software name; the link itself is unchanged. Change it if you disagree.
4. **Third-party plug-ins built for Open Dental no longer load** (RENAME, D5). The adoption tool lists the program rows that name a plug-in DLL. Change it if you disagree.
5. **Suni direct capture is removed** (BIN-A 1, CAP design 2 (a)). Suni sensors work through Suni's own TWAIN driver. Change it if you disagree.
6. **Four bridges say "not available"** (BIN-A 2 and question 1): Dr. Ceph, eClinicalWorks Rx, Patterson Imaging and Schick CDR 4. Their vendors publish no interface, and they needed Open Dental's closed `VBbridges.dll`. Schick CDR DICOM (version 5) still works. The alternatives: hide or remove the four, or get the vendor's interface so it can be implemented from its documentation. Change it if you disagree. **Superseded for Schick (2026-11-08):** Schick CDR is not used and is removed completely in the clean-up stage of the .NET 10 move.
7. **Sparks3D is removed, with the OpenGL fallback** (BIN-A 3 and 4, question 4). The tooth chart is SparksToothChart: DirectX 9 where Microsoft's DirectX End-User Runtime (`d3dx9_43.dll`) is installed, otherwise OpenGL, otherwise Simple 2D. The setup does not install the DirectX runtime; the graphical perio chart needs it and says so. The alternatives: the setup chains Microsoft's DirectX installer, or new computers default to OpenGL. Change it if you disagree. **Superseded (TC-8, 2026-11-15):** the tooth chart is the WPF 3D chart on every computer stored as DirectX or OpenGL, drawn by the program in software, so no DirectX runtime is needed, and the setup ships or chains none (your answer of 2026-11-15, "we can get rid of Directx after our switch"); a computer stored as Simple 2D stays 2D; the graphical perio chart opens on the 3D chart on every computer and no longer asks for DirectX; File > Graphics offers "3D chart" and "Simple 2D chart" with a "Draw in software" switch ([port-notes/TC-8.md](port-notes/TC-8.md)). The DirectX and OpenGL charts are deleted by TC-9a and TC-9b. **Revised by TC-8b (your "Match Open Dental 26.2", 2026-11-15; waiting for your gallery approval):** File > Graphics has Open Dental 26.2's layout, the 3D chart is chosen by Open Dental's own DirectX 11 settings (so a computer stored as Simple 2D gets the 3D chart unless set to "Do not use"), the smoothed picture is the default on screen, and the graphical perio chart opens only on a computer that uses the 3D chart ([port-notes/TC-8b.md](port-notes/TC-8b.md)).
8. **Topaz is loaded only when installed** (BIN-B 1). The program ships no `SigPlusNET.dll` and loads Topaz's from the program folder, a `Topaz Systems` folder or `%windir%\SigPlus`. Without it, a Topaz signature shows "Topaz signature pad software is not installed." Change it if you disagree (the open Topaz questions are under Vendor integrations). **Superseded (2026-11-08):** not used; removed in the clean-up stage of the .NET 10 move.
9. **The Word letter merge uses late binding** (BIN-B 2). No Office interop assembly and no `stdole.dll` ship; the merge needs Word installed, as before. Change it if you disagree.
10. **RS232 and DotNetZip are replaced** (BIN-B 5 and 6): the BCBS Georgia dial-up path uses .NET's `SerialPort`, and every zip (the release channel too) uses `System.IO.Compression`. Change it if you disagree. **Superseded for the dial-up path (2026-11-08):** the BCBS Georgia dial-up terminal is removed in the clean-up stage of the .NET 10 move; the DotNetZip replacement stands.
11. **fo-dicom, NDde and the Google APIs client libraries ship in v0.0, pending the licence question** (REL-FILES 1 to 3; the questions are under "Release v0.0" below). Change it if you disagree. Since then fo-dicom was replaced (2026-11-02) and NDde removed (2026-11-07, REL-FILES 2); the three program links that used NDde came back on an in-house DDE client (2026-11-08, S0-R).
12. **README.md says "GNU GPL v2 or later"** (REL-FILES 6), as the source headers do ("either version 2 of the License, or (at your option) any later version"). Change it if you disagree.
13. **The side programs are not shipped**: the HL7 service, the Middle Tier server, CentralManager, DatabaseIntegrityCheck, MySQLManager and the replication service. The setup installs the program, `ServiceManager.exe` and `UpdateFileCopier.exe` (the program uses both). The Middle Tier comes with a licence condition: the PDFsharp built here is a modified AGPL work (route B, your decision of 2026-11-02: PDFsharp/MigraDoc 6.5 kept under the AGPL and built from empira's GitHub source as a git submodule, with an open `PdfSharpLicense.cs` in place of empira's closed `PdfSharp.License.dll`; LIB-PDF under "Release v0.0" below), so a hosted Middle Tier, even one without local changes, falls under the AGPL's network clause (LIB-PDF, point 3). Before the Middle Tier is ever shipped or hosted, it must prominently offer its remote users the Corresponding Source, such as a link to the release's `OpenPmsDental-<version>-source.zip`; it has no such offer today. **Decided (2026-11-02):** the offer is built when the Middle Tier is first shipped, not before ("only when it's first shipped"). Change it if you disagree.
14. **The installer is not signed** (your decision for v0.0). SmartScreen warns; the release notes tell the office to compare the SHA-256 with `SHA256SUMS.txt`. Change it if you disagree.
15. **Inno Setup builds the installer.** Its author asks commercial users to buy a licence (since 6.5.0); its FAQ says this is not strictly required. Change it if you disagree (the fallback is a PowerShell script in an IExpress or 7-Zip self-extractor).
16. **The public repository's history still contains the old proprietary DLLs** that main no longer has (Open Dental's closed binaries and the commercial and interop files BIN-A and BIN-B deleted). Rewriting the history removes them from every clone's future but changes every commit id; that is your call. **Today:** the history is unchanged.
17. **The installer's own defaults** (this release). The program folder is `C:\Program Files (x86)\Open PMS - Dental`. The setup refuses a folder that holds other files, because uninstalling removes the whole program folder (also files updates added); it keeps a folder inside it that looks like an A to Z folder. The database connection page defaults to `localhost`, `opendental` and, from the version after 0.0, `opmsd` (the program's own account in [db-connection-security.md](db-connection-security.md), owner decision of 2026-11-02: not `root` any more) and never stores a password. A missing Visual C++ x86 runtime is downloaded from Microsoft's own address only after the user agrees (or with `/PREREQS=download` in a silent install), and runs only with a valid Microsoft signature (kept, your decision of 2026-11-02, LIB-WEB 1); a missing WebView2 Runtime is only reported, with how an administrator installs it (since the WebView2 update, setup never downloads it). Setup never starts the program. Change it if you disagree.
18. **The spell checker ships under the LGPL 2.1 or later** (v0.0: NHunspell and Hunspell; since the library updates: WeCantSpell.Hunspell 7.0.1, the managed port you approved on 2026-11-02, built from its source since your decision of the same day). Its licence is an MPL 1.1 / GPL 2.0 or later / LGPL 2.1 or later tri-licence at the user's choice; `THIRD-PARTY-NOTICES.txt` says the release uses the GPL / LGPL option and distributes it under the LGPL 2.1 or later. The setup installs the full LGPL 2.1 text, `LICENSE-LGPL-2.1.txt`, which also covers the RDL Project (`RdlEngine.dll`, `RdlViewer.dll`, LGPL 2.1 or later) and the en_US word list. No MPL 1.1 text ships: **decided by you on 2026-11-02**, no longer a default.

---

## Release v0.0: licences, packaging and the first start

- (REL-FILES 1, TWAIN-W 1) **fo-dicom** (`Dicom.Core.dll`, `Dicom.Native.dll`) is MS-PL, which is not compatible with the GNU GPL. Keep it, replace it, or drop DICOM? The in-house reader (TWAIN-W) already opens the simple files the capture writes; it could take more (signed grayscale with an offset, the first frame of multi-frame files) so that fewer files need fo-dicom, or some files could stop being read so fo-dicom can go. **Today:** decided on 2026-11-02 (option C): fo-dicom is removed, and the program's own full reader reads the other DICOM files (`docs/imaging/dicom-reader.md`). JPEG Extended (12-bit JPEG), JPEG Lossless, JPEG-LS and JPEG 2000 images do not open: decoders for compressed DICOM are deferred (docs/roadmap.md, Later), to be built only if such files turn up.
- (REL-FILES 2) **NDde** (`NDde.dll`) has its own shared source licence in the style of MS-PL. Acceptable next to the GPL? **Answered (2026-11-07):** no; NDde is removed, with the three program links that used it (DentX, Dexis Integrator and Vipersoft, which the office does not use). Their rows stay in the database and show as "(removed)" in Program Links; setup deletes `NDde.dll` on an upgrade (`docs/port-notes/REL-FILES.md`, "NDde removed"). **Revisited (2026-11-08):** "Don't make the assumption that if the office doesn't use, we'd cut it. ... DentX, Dexis Integrator and Vipersoft links, and NDde.dll we should keep if we can." The three links are back (S0-R). NDde itself still cannot ship: its licence (clauses 2, 5 and 7: source only under its own licence, a patent termination, export rules) puts restrictions on recipients that GPL v2 section 6 and GPL v3 sections 7 and 10 do not allow, the structure of the MS-PL, which the FSF lists as incompatible with the GPL. So the links send their DDE commands through a small in-house client on Windows' DDEML (`CodeBase/Utilities/ODDdeClient.cs`) that sends what NDde sent; `NDde.dll` stays out of the program, and setup still deletes an old copy (`docs/port-notes/S0-R.md`). **Open:** if counsel ever finds NDde acceptable, it could come back; nothing needs it now.
- (REL-FILES 3) The **Google APIs** client libraries are Apache 2.0: compatible with GPL v3, not v2. The program is "v2 or later", so it can be distributed under v3. Is that the intended reading, or should the Gmail integration go? **Today:** ships.
- (LIB-PDF, your decision of 2026-11-02 to use PDFsharp/MigraDoc 6.5; route B, chosen on 2026-11-02, builds them from empira's source, see the next item) **What the AGPL changes.** PDFsharp and MigraDoc 6.5 are under the GNU AGPL version 3 (the NuGet packages say AGPL-3.0-only; the source's LICENSE says "version 3 or any later version"); 6.2.4 was the last MIT release. The AGPL can be combined only with GPL version 3 (section 13 of each), not with version 2. The program's code is "GPL v2 or later", so the program as distributed with PDFsharp is conveyed under GPL v3, and PDFsharp and MigraDoc keep the AGPL. In practice: (1) whoever distributes the program must offer its complete source under GPL v3 (as today, but v3's terms, including its patent and installation-information clauses, now apply to the whole); (2) the program's own source files keep "v2 or later", so they can still be taken out and used under v2 apart from PDFsharp; (3) the network clause covers the whole program: GPL v3 section 13 says the AGPL's section 13 requirements "concerning interaction through a network will apply to the combination as such". Those requirements apply to a modified version, and since route B the program as this project distributes it already is one: the PDFsharp built here replaces empira's `PdfSharpLicense.cs` with `third-party/build/PdfSharp-gdi/PdfSharpLicense.cs` (AGPL section 5a, see the next item). So whoever lets users interact with **any version** of the program remotely through a network, not only a version someone else has modified, must prominently offer those users the Corresponding Source of the whole program they use (from a network server, at no charge), not only that of PDFsharp and MigraDoc; for a version with further changes, that is the source of the whole modified program. The question open before route B, whether a change to the GPL part alone (say, to a sheet or a report) triggers it, is not settled, but it no longer decides anything for a build of this repository, which always holds the modified PDFsharp. The case is in this repository: the Middle Tier (`OpenPmsDentalServer`; its `DtoProcessor` now calls `PdfSharpSetup.Init`) is left out of the release only by a default (decision 13 above), and an office or vendor that hosted the Middle Tier would be in it, **even with no local changes**: before the Middle Tier is ever shipped or hosted, it must prominently offer its remote users the Corresponding Source, such as a link to the release's `OpenPmsDental-<version>-source.zip`. The workstation program, which each office runs on its own computers, is not offered to users over a network and is not affected; (4) every PDF's Producer and Creator properties say "(AGPL license)"; nothing is drawn on the pages, no key is needed and, by test (`tests/PdfLibraryTests`), no network connection is opened; (5) GPL v3 settles REL-FILES 3: the Apache 2.0 Google APIs libraries are compatible with v3. fo-dicom (MS-PL) and NDde (MS-PL style) stay incompatible under v3 as under v2; the MIT, BSD, public-domain and LGPL 2.1 components are fine with v3 and with the AGPL; no shipped component is GPL-v2-only (the RDL engine's files are LGPL 2.1 or later, and eight of them say GPL v2 or later, which v3 also covers). The release ships `LICENSE-GPL-3.0.txt` and `LICENSE-AGPL-3.0.txt`; README, NOTICE, THIRD-PARTY-NOTICES.txt, the setup's information page and Help > About > View Licenses say so, and the setup's License Agreement page shows the GPL v3 text, after a header that names the AGPL parts and the "v2 or later" of the program's own source files. **empira reads its licence differently.** Its pricing page (https://pdfsharp.com/Pricing) says the AGPL option "Requires your project to be licensed with AGPL, too" and lists "Use in closed-source projects prohibited". This project instead keeps the program under GPL v3 and combines it with the AGPL parts under section 13 of both licences. The licence texts permit that combination, and the program is not closed-source, but empira's own statement does not describe it; if empira ever disputed it, the program could not simply be relicensed under the AGPL (its code is GPL "v2 or later", and the AGPL is not a later version of the GPL), so the ways out would be the commercial licence or 6.2.4. Please weigh that risk before relying on this route. **Answered (2026-11-07): counsel reviewed it; PDFsharp and MigraDoc 6.5 are kept; closed.** (Before that answer:) **This stays open as a dispute risk for counsel** after route B: building PDFsharp from its AGPL source does not change empira's statement, which is about how the program that uses PDFsharp must be licensed, not about where PDFsharp comes from. Route B adds one point for counsel: the program as distributed now contains a modified version of PDFsharp, so the AGPL's network clause applies to any copy of the program that users reach over a network, a hosted Middle Tier without local changes included (point 3); counsel should confirm that the source offer the Middle Tier must carry before it is ever shipped or hosted (decision 13; you decided on 2026-11-02 that it is built when the Middle Tier is first shipped) meets section 13. The alternatives: empira's commercial licence (EUR 750 excluding VAT, for business customers only (B2B), perpetual, no subscription, with all updates of the 6.5.x and 7.0.x lines, and no AGPL terms), or PDFsharp/MigraDoc 6.2.4 under MIT (the code of this package works with it unchanged except for the licence call). **Today:** 6.5 under the AGPL, distributed under GPL v3.
- (LIB-PDF; **resolved on 2026-11-02 by building from source**) **`PdfSharp.License.dll` had no published source.** You chose route B on 2026-11-02, and the question is resolved: PDFsharp and MigraDoc 6.5.0 are built from empira's source, the git submodule `third-party/PDFsharp` at tag v6.5.0 (commit `96d5ef81d304`), by this project's own project files (`third-party/build`), and the one upstream file that needs the closed library, the wrapper `src/foundation/src/PDFsharp/src/PdfSharp/root/PdfSharpLicense.cs`, is replaced by an open file of ours (`third-party/build/PdfSharp-gdi/PdfSharpLicense.cs`, a modified version of empira's wrapper under AGPL section 5a, with empira's copyright and a dated notice). It behaves as the AGPL path did: no key, no network connection, no message, `SetAgplLicense()` allows use and the PDFs' Producer and Creator still say "(AGPL license)"; it never enables anything of the commercial licence. No `PdfSharp.License.dll` is referenced, kept in `Required dlls` or shipped (`tools/Build-Release.ps1` denies it), so the release blocker and `-PdfSharpLicenceResolved` are gone, and every release publishes its Corresponding Source, the submodules included, as `OpenPmsDental-<version>-source.zip` ([third-party-source-builds.md](third-party-source-builds.md)). The question as it stood: PdfSharp-gdi 6.5 calls it before it creates, reads or writes any PDF (`PdfSharpLicense.CheckLicense`), and refuses to work without it; the program's own code calls it too (`PdfSharpSetup.Init` calls `PdfSharpLicense.SetAgplLicense`). Its source is not in empira's public repository: tag v6.5.0 holds only the wrapper `src/foundation/src/PDFsharp/src/PdfSharp/root/PdfSharpLicense.cs`, which passes every call to a `PdfSharpLicenseImpl` that the repository does not contain, and the build downloads the DLL ready-made (https://assets.pdfsharp.net/license/license.zip, from `dev/download-assets.ps1`). Its product version, 7.0.0-dev-7.0.0-preview-2-review-for-release-285, differs from the rest of the package, and the package has no `.pdb` or `.xml` for it. GPL v3 and AGPL v3 sections 1 and 6 ask whoever conveys the program in object form for the Corresponding Source of the whole, which includes a library the work is specifically designed to require, so this project cannot give its recipients the source of this one file (NOTICE says so). empira, as its copyright holder, is not bound by that, but we are. This is a new licence problem of the PDFsharp 6.5 update, not one inherited from Open Dental like fo-dicom or NDde. Ask empira for its source under the AGPL (or a written permission to convey it as it is), ask counsel, buy the commercial licence, or go back to 6.2.4 (MIT, no licence file)? Nothing in its documentation or behaviour shows a key, a watermark, a message or a network call (the tests check the last three). No release was made with it.
- (REL-FILES 4) `AssemblyCompany`: "Open PMS - Dental contributors" (also the setup's publisher), or the practice's name? **Today:** "Open PMS - Dental contributors".
- (REL-FILES 5) The Hunspell en_US dictionary embedded in `WpfControlsOD` is the OpenOffice.org dictionary (48,245 words); its terms are taken from that dictionary's README (word list LGPL, affix file BSD-style). Please confirm, or replace it with a current SCOWL build.
- (this release; library updates; **decided on 2026-11-02: built from source**) **The source of the spell checker.** v0.0 shipped the prebuilt `NHunspell.dll` (0.9.6) and `Hunspellx86.dll` (Hunspell 1.2); the library updates replace both with `WeCantSpell.Hunspell.dll` 7.0.1 under the LGPL 2.1 or later, which (sections 4 and 6) asks whoever distributes the binary to offer its corresponding source. You decided on 2026-11-02 to build it from source: the git submodule `third-party/WeCantSpell.Hunspell` at tag 7.0.1 (commit `ab5709d95b2d`), the library project only, unmodified, compiled by `third-party/build`; the NuGet file left `Required dlls`. Every release publishes the source with the setup, in `OpenPmsDental-<version>-source.zip` ([third-party-source-builds.md](third-party-source-builds.md)). The MPL question is decided too: no MPL 1.1 text ships (item 18 above).
- (BIN-B 5) `Required dlls/Windows.winmd` (a Windows SDK file) is in the repository only to compile `PasswordVaultWrapper`; it does not ship. Keep it, or reference the installed Windows SDK's `UnionMetadata\Windows.winmd`?
- (RDL-SRC 4) `Required dlls/RdlDesigner.exe` is the prebuilt report designer; it does not ship, and `RdlDesign/` holds its source outside the solution. Delete it, or build RdlDesign from source? **Answered (2026-11-08):** built from source. Decision 17 of [specs/net10-plan.md](specs/net10-plan.md) keeps RdlDesign; since S1-1 of the .NET 10 move it is in `OpenDental.sln` and `OpenDental.Local.slnf` and builds against the repository's own `RdlEngine` and `RdlViewer`. It still does not ship, and the prebuilt `RdlDesigner.exe` had already left `Required dlls` (ORG-CLEANUP).
- (this release) **Tag and publish v0.0?** May the developer tag `v0.0` and make the GitHub release with the setup, `SHA256SUMS.txt`, the release notes and the source archive? The release notes name the tag as the source of the release (GPL section 3). **Today:** nothing is tagged or published; the files wait in the developer's release folder.
- (this release) **Issue reports.** The release notes send reports to the repository's GitHub issues. A fork has Issues turned off by default: turn them on, or name another contact?
- (RENAME, stage 3) **The first start after the installer.** The rename's checks that need the running program are still open: start the program from an installed folder and walk through the modules, CentralManager (not shipped), the RDL reports, the cursors, a second instance and the Event Log source. Who does this, and on which PC? **Answered (2026-11-07):** the developer tests an administrator install against a test database; the owner schedules a walk-through on an office PC.

---

## Switch-over and data conversions

**1. [SWITCH-OVER] Settings that will not carry over from Open Dental** (B-ACCT 1, B-APPT 1, B-CLAIM 1, B-ERA 1, B-INSH 2, B-PAY 1, B-PERIO 1, B-SERVER 1, B-SHELL 1, B-WIKI 5, V-BRIDGE1 1). **Answered (2026-11-07):** you gave the setting names (names only) of the office's Open Dental 25.3.48 database and the names of its four program links. Built as [port-notes/SWMAP.md](port-notes/SWMAP.md) says; no office value was read or used.

Open Dental does not publish the internal names of these settings, so release 0.0 stored each one under a name of its own, and after the switch-over each was at its default. **Now:** this program reads each setting the office database has under Open Dental's own name, so the switch-over carries the office's value over as it is, without changing the row; `migration-preview` lists them, with what this program will show, before the day (runbook step 0.2):

| Setting in Open Dental | Package | After the switch-over now |
|---|---|---|
| Sales-tax procedure code (24.4) | B-ACCT 1 | Carried over: Open Dental's name is `SalesTaxProcCode`, the name this program already used |
| Provider time bars on the operatory background (25.2), and the secondary time-bar color when no provider time is set (25.3) | B-APPT 1 | Carried over: `ApptProvbarShowOnOps`, `ApptUseSecProvTimeIfEmpty` |
| "Prompt for Medical EOB on primary claims" (25.2) | B-CLAIM 1 | Carried over: `ClaimCheckForMedicalEOB` |
| "Auto split ERAs with CARC denial codes listed above" | B-ERA 1 | Carried over: `EraAutoProcessSplitByCarcCodes` |
| Blue Book percentage of the provider fee | B-INSH 2 | Carried over: the office database still has `InsBlueBookUcrFeePercent`, the 24.3 name this program reads |
| "Payment plans are not affected by the global lock date" | B-PAY 1 | Carried over, **converted**: Open Dental's `PayPlansIncludeInGlobalLock` says the opposite (1 = payment plans are included in the global lock date), so this program's box is ticked only when it is 0 |
| Perio "Recession" wording (24.4) | B-PERIO 1 | Carried over: `PerioRecessionInsteadOfGM` |
| Incomplete Procedure Notes report "include all procedures with no notes" default, and the two replication delay settings | B-SERVER 1 | The read delay after a write carries over from `ReplicationWriteReadDelayEnabled` and `ReplicationWriteReadDelayMS`. The office database has no setting for the report default or for this program's look-back for signals (only `ReportsIncompleteProcsNoNotes`, `...Unsigned` and `...ExcludeCodes`, which carry over as before), so those two start at their defaults, as before |
| Blur the screen when switching modules | B-SHELL 1 | Carried over: `BlurModuleOnLoad` |
| Read-only wiki copy (if Open Dental 25.2 had a setting for it) | B-WIKI 5 | Open Dental 25.3.48 has no setting for it; the program's own setting starts off |
| Program links for Overjet, DTX Studio, DTX Studio Acquire and IAP | V-BRIDGE1 1 | The office's rows are named `Overjet`, `DTXStudio`, `DTXStudioAcquire` and `IAP`, the names this program uses, so it reuses them with their settings and adds no second link |

The settings saved in release 0.0 under its own names move to Open Dental's names in the update (fork migration step 2026110731); an Open Dental database has none of them. Names of the office database for features this program left out (chat, Canadian RAMQ, Open Dental's hosted services, cloud, mass e-mail, Message-to-Pay, the Pearl timestamp and the like) are left alone; SWMAP lists each with the reason. The five settings of this program that Open Dental has no name for stay in runbook step 0.11.

**Answered (2026-11-07):** the owner exports the settings list from the office database himself: the setting names, plus the values of a few of them. The file stays out of the repository.

**2. [SWITCH-OVER] Which Open Dental version the office switches from** (M2-02 1, M2-EF 1, V-BRIDGE2 5)
The switch-over is built and rehearsed for an Open Dental **25.3.48** database. Will the office stay on 25.3.48 until the switch? If Open Dental gets updated to 25.4 or later first, three things would not map without extra conversion work: settings added in 25.4/26.1, eForm links to patient fields, and the Clerri and Clever One program links. **Today:** only 25.3.48 is supported. **Answered (2026-11-07):** the office is held at Open Dental 25.3.48 until switch-over day.

**3. [SWITCH-OVER] Blockout permissions** (M2-02 2)
26.1 splits "edit blockouts" into Add, Delete and Edit. **Today:** at the switch-over, every user group that can edit blockouts also gets Blockout Add and Blockout Delete, so nobody loses access. An administrator can narrow this afterwards. Please confirm.

**4. [SWITCH-OVER] The Insurance History setup moves into code groups** (M2-HIST 3, 5)
**Today:** at the switch-over, the ten Insurance History code boxes in Preferences are moved into code groups. Each matching code group is marked "Show In History" and gets the codes from those boxes. Where no matching group exists, a new one is created. The old settings are kept, not deleted.
- (M2-HIST 3) Each new group is given the standard category it stands for (for example Exam), so sheet fields such as "date of last exam" start finding it. Confirm, or create the groups without a category?
- (Combined review) The codes from the old boxes are added to the matching **frequency** code group, which also widens that group's frequency limitation in insurance estimates (for example, more codes count toward "2 exams per year"). Keep that, or put them in a separate group that only feeds Insurance History? **Today:** added to the frequency group; the step's log lists the added codes but does not mention the estimate effect.
- (M2-HIST 5) A box that was blank used to show a "Not Set" row. After the switch-over that category has no row until a group is marked "Show In History". Acceptable?

**Answered (2026-11-07):** the existing frequency-limit code groups stay **unchanged**; the codes of the old Insurance History settings go into a code group used only for Insurance History (Show In History; not in frequency or age limits). Built in the step itself (level 2026110104, which no office database ran): one new group per category with codes ("Bitewing History", "Exam History", ...), with no fixed category and in no benefit grid; no existing group changes; the step's log, the adoption preview and the rehearsal say so (`docs/port-notes/M2-HIST.md`, "Owner decision of 2026-11-07"). That also settles M2-HIST 3 (the new groups carry no category); M2-HIST 5 stays as built.

**5. [SWITCH-OVER] One-time "Do Not Bill to Insurance" fill** (M2-EST, rule 4 and question 5)
**Today:** this step is written and tested but switched off, so it does nothing. If turned on, it would find procedures where staff unchecked "bill insurance" on every estimate (treatment-planned or completed, not yet on a claim) and mark the procedure itself "Do Not Bill to Ins". Without it, if one of those estimates is rebuilt later (for example after a plan change), it goes back to billing insurance.
- Should it run at the switch-over? Turning it on also needs work in the switch-over checking tools.
- (M2-EST 5) If yes: with the setting "exclusions mark Do Not Bill" on, the step skips estimates of plans that have exclusions, because the program already marks those automatically and they cannot be told apart from a staff choice. Acceptable?

**6. [SWITCH-OVER] The DentalXChange Patient Credit Score link** (B-SHELL 5)
This vendor service was discontinued. **Today:** the switch-over turns the link off and removes its toolbar buttons, even on the office's Open Dental database (in case Open Dental's own update left them). Acceptable?

**7. [SWITCH-OVER] The new UDS+ report** (M2-UDS 4)
**Today:** the switch-over adds the UDS+ report to the report list right after "FQHC Dental Sealant Measure". It is visible unless that report is hidden, and the groups that can run the sealant report also get permission for it. Should an office that is not an FQHC see it at all?

**8. [SWITCH-OVER] The Denti-Cal DC217_2019 claim form** (M2-CLAIMFORM 1)
**Today:** the form is not added automatically. Staff can add it from Setup, Claim Forms (internal list, Copy). Should the switch-over add it to the office's claim forms? This only matters if the office bills Medi-Cal.

**9. [SWITCH-OVER] Payment plan version** (B-PAY 2)
Which payment plan version does the office database hold? If Open Dental's 25.1 update forced the newer "Age Credits and Debits" mode, should the program do the same for databases that never had that update? **Answered (2026-11-07, with item 1):** the office database has `PayPlansVersion`, the name this program reads, so the office's payment plan version carries over unchanged. Nothing converts it, on the office database or on a database from 24.3.

**10. [SWITCH-OVER] Automatic recurring and repeating charges** (M2-RECUR 5)
The program has no automatic runner for recurring card charges or repeating charges, because the Open Dental background service was removed. The "automated" settings are still shown, but nothing runs them. **Today:** staff must run the charges by hand. With backdating off, the Repeating Charges tool must be run on each billing date (with backdating on, the default, a late run posts the missed dates). Does the office rely on automatic charges today, and do you want a scheduled runner on an office computer? **Answered (2026-11-07):** the office runs no automatic recurring charges today: nothing to build.

**11. [SWITCH-OVER] Checks of the new database structure** (M2-01 1, 2, 6)
- (M2-01 1) Parts of the 26.1 structure (required fields, defaults, indexes) are worked out from Open Dental's published documents. Is a structure-only copy (no data) of any real Open Dental 26.1 database available to confirm them? **Today:** they stay worked out from the documents.
- (M2-01 2) The claim payment note becomes a long text field. Empty (blank database) notes become blank text, and the field can never be empty in the database sense. Fine, or should empty be allowed? **Today:** never empty.
- (M2-01 6) Fixes Open Dental made after 26.1.0 (26.1.x) have no published document. Should someone check the 26.2 change document for them before the switch? **Today:** not checked.

---

## Insurance and estimates

- (B-INSH 1) Insurance History: when both dates in a category are already completed procedures, should staff still be able to add a newer "existing other" date, for example with an "Add date" button? **Today:** completed dates can't be changed and nothing can be added (Open Dental 25.2 behaviour).
- (B-INSH 3) Edit Benefit: please confirm which boxes each benefit type should allow, especially quantity and qualifier on per-visit copayments, and an amount on copayments. **Today:** both are allowed, as before.
- (M2-HIST 1) Insurance History labels for the four scaling quadrants: "Perio Scaling UL" and so on for the standard scaling group, and the group's own name for any other four-quadrant group. Or always the group name? **Today:** as just described.
- (M2-HIST 2) A hidden code group that is marked "Show In History" keeps its Insurance History row, and a note says so when it is hidden. Confirm? (The spec contradicts itself.)
- (M2-HIST 4) Should eligibility responses that mention FLUORIDE or SEALANTS fill the Fluoride and Sealant history rows? **Today:** they match only by procedure code. (Sealant history without a tooth is of little use.)
- (M2-HIST 6) Eligibility lines that match no history row are only written to a detailed log. Should the eligibility request in Edit Insurance Plan tell the user which lines were skipped? **Today:** no message.
- (M2-HIST 7) A fix: dates in eligibility responses were all read as January. They are now read correctly, so imports write different (correct) dates than before. Confirm.
- (M2-HIST, phase B) Reset Code Groups gives the seven standard history categories their Insurance History rows (first default code as the history code, scaling with four quadrant rows). Keep? **Today:** yes (our choice; Open Dental's defaults are not public).
- (B-PROC 3) Reset Code Groups: groups still used by benefits that match no default are kept as hidden groups, so benefits and estimates keep working, instead of being deleted. Keep? **Today:** kept hidden, and the reset runs only after the user accepts.
- (M2-EST 2) Treatment Plan estimates for scheduled procedures use the patient's age on the appointment date. Should unscheduled procedures follow the plan's date picker too? **Today:** they use today's age.
- (M2-EST 3) Should fees of scheduled procedures also follow the appointment date (dated fee schedules), as their estimates now do? **Today:** no.
- (M2-EST 4) In the Treatment Plan module there is no estimate when coverage ended before the appointment. Should Chart and Appointment estimates do the same? And should a coverage start date after the appointment also mean no estimate? **Today:** Treatment Plan module only, end date only.
- (M2-EST 6) Should the "Estimated as of (date)" line in the treatment plan description stay, or become an optional "Est. Date" column? **Today:** a description line (when Show Max/Deductions is on).
- (M2-CARRIER 1) Carriers that require preauthorization: **today** "[PreAuth]" shows in red on the treatment plan description, and the carrier's appointment color is a highlight behind the appointment tag. Is that the look you want?
- (M2-CARRIER 2) A procedure marked Do Not Bill to Ins for a plan gets no [PreAuth] flag for that plan. Or show it anyway? **Today:** no flag.
- (M2-CARRIER 4) When both primary and secondary want a preauthorization, should each plan get one (primary first), or only the primary? **Today:** one per plan.
- (M2-CARRIER, phase B) Procedures added through the Treatment Plan "Add Procs" window get only the [PreAuth] marker, without the reminder that charting shows. Enough? **Today:** marker only.
- (M2-INSPLAN 1) The redesigned Edit Insurance Plan window fits a 1366x768 screen at 100% scaling. Is that the smallest screen in the office? A 1920x1080 screen at 150% does not fit without scrolling (the old window didn't either).
- (M2-INSPLAN 2) The window's layout is our own design. Please check it on screen.
- (M2-INSPLAN 3) The audit text for deleting a plan now includes the plan number. Confirm.
- (M2-INSPLAN 4) A few plan drops write no audit entry (Trojan import, clone sync, a sheet import replacing a plan). Should they? **Today:** not logged, as before.
- (M2-INSPLAN 5) The empty "Import Benefits" group is hidden when neither Trojan nor IAP is turned on. Confirm.
- (M2-INSV 1) Insurance Verification List: are the "Min/Max days until appointment" labels and their layout (Min above Max) fine?
- (M2-INSV 2) Should setup have a default for Min? **Today:** Min starts at 0 each time; Max comes from the existing setup.
- (M2-INSV 4) Auto-unassign after 30 days counts whole calendar days. Should it be exactly 30x24 hours now that the time is stored? **Today:** whole days.
- (M2-INSV 5) Status Date is blank for rows never assigned (the old program showed 1/1/0001). Keep?
- (M2-TERM 2) Blue Book: the grid row says "Standard provider fee", but the double-click message still says "Provider Fee". Change the message to match? **Today:** they differ.
- (M2-TERM 3) Three preference check boxes now say "(no write-offs)" instead of "(zero out write-offs)" so the text fits. Acceptable, or widen the boxes?

---

## Claims and ERA

- (B-CLAIM 2) Medical EOB prompt (off by default): when a medical claim is received, the program asks about attaching its EOB to the patient's unsent primary dental claims. Is that the right reading? The spec allows two others.
- (B-CLAIM 3) Attaching the medical EOB also ticks the claim's EOB paperwork box. Keep, or leave it to staff? **Today:** ticked automatically.
- (B-CLAIM 4) Audit entries for overrides on received claim procedures are filed under "Claim Procedure Received Edit", which can't be picked in the Audit Trail's filter. Add it to the filter, or file them all under "Proc Edit"? **Today:** as in 24.3.
- (B-CLAIM 5) Should overrides that the program recalculates by itself (ortho cases) also be logged? **Today:** no.
- (B-CLAIM 6, see also the M2-CLAIMRCV integration notes) When later claims are recalculated automatically after a primary claim is received, their saved attachments (X-rays, the medical EOB) can be deleted if the claim was loaded without them. M2-CLAIMRCV made this reach tertiary claims too. **Today:** unchanged from 24.3. The developers see this as a defect with a one-line fix. May they fix it?
- (B-ERA 2) Auto split: a line that has a listed denial code but still carries an insurance payment is never split off. Instead the claim is held back with a reason. Should such lines be split too?
- (B-ERA 3) The claim split off by the auto split is Unsent. Or should it keep the original's status, like a manual split? **Today:** Unsent (as the spec says).
- (B-ERA 4) Are the three extra ERA columns (Subscriber ID, Allowed, Matched Claim) useful, or should only the 24.3 set be offered? **Today:** offered.
- (B-ERA 5) Should the manual Split Claim button also write the audit entry? **Today:** only the automatic split does.
- (M2-ERA 2) When a claim payment is deleted, the link between its claim procedures and the ERA claim is kept (the spec says to clear it). Confirm.
- **[SWITCH-OVER]** (M2-ERA 3) Automatic ERA processing now holds back a claim whose payment came from another ERA with the same trace number and payer (a duplicate import). Keep this, or post such claims as before? **Today:** held back.
- **[SWITCH-OVER]** (M2-CLAIMRCV 1 and the phase B extra) After a claim is received, the prompt offers the claim of "the next insurance that has a claim, not yet received, for those procedures", not strictly the next plan in order. When all plans are still on the patient, plan order decides. For example, with medical first, then dental primary, then secondary: receiving the medical claim (with the medical-to-primary prompt off) now offers the dental primary claim, where 24.3 offered the secondary. Confirm.
- (M2-CLAIMRCV 2) Claims on Hold or In Process are recalculated, but the prompt never changes, sends or prints them. Confirm.
- (M2-CLAIMRCV 3) If a claim the prompt sent is then received automatically, should another prompt follow for the insurance after it? **Today:** no, as in 24.3.
- (M2-CLAIMFORM 2) Please print a test claim on DC-217 (10/19) stock and report any shift. If you can supply the handbook's section 6 PDF (it is public), the positions can be checked against it.
- (M2-CLAIMFORM 3) Denti-Cal form, arch and quadrant: should they print as area codes (01, 10, ...) or as letters (UA/LA, UR/UL/LL/LR)? **Today:** area codes.
- (M2-CLAIMFORM 4) Boxes not printed on the Denti-Cal form: share of cost, other-coverage amount, a separate billing-provider signature date, TAR/NOA control numbers. Are any needed?
- (CLAIM-DCM 1) An existing bug, not changed: in the claim attachment picker (`FormImageSelectClaimAttach.SaveAttachment`), a non-DICOM image with crop, rotation, flip or windowing set, on a local A to Z folder, gets a claim attachment row but no file. The branch looks inverted (it renders only for cloud storage). DICOM does not go through it. Fix it in a follow-up? **Answered (2026-11-07): fixed.** The edited image is always written where the attachment points, for a local images folder and cloud storage alike (`docs/port-notes/CLAIM-DCM.md`, section 6).
- (CLAIM-DCM 2) Edit Claim's copy of an X-ray: JPEG (quality 100) or PNG (lossless, larger)? CDAnet converts it to JPEG anyway. **Today:** JPEG.
- (CLAIM-DCM 3) Should the DentalXChange picker default the image type to X-Rays for a DICOM radiograph? **Today:** the user always picks the type.
- (CLAIM-DCM 4) CDAnet allows DICOM (`DIC`) attachments, but a DICOM file is never sent, so CDAnet gets the JPEG copy. Confirm.
- (CLAIM-DCM 6) Should Edit Claim's DICOM copy include the drawings? **Today:** no Edit Claim copy does.
- (BIN-B 4) BCBS Georgia dial-up claims: does any office still use them? If not, the clearinghouse path and `TerminalConnector` can go. **Today:** kept, on `SerialPort`, not tested with a modem. **Answered (2026-11-08):** cut in the clean-up stage of the .NET 10 move.

**Clearinghouses**
- (V-EDS 1) Can the office get EDS's documentation and a test login for the checks EDS added for 26.1? **Today:** those checks stay off.
- (V-EDS 2) Should the online check also run when the EDS attachment service is off? **Today:** no.
- (V-EDS 3) Add an "Update EDS payer list" action? **Today:** nothing loads the list, so the payer-list checks rarely have anything to compare with.
- (V-EDS 4) A blank or too-short payer ID: should it be a warning, or an error that stops the claim (and blocks attachments, and can make claim creation refuse the claim)? **Today:** a warning; the claim goes out with the paper-claim ID 06126, as before.
- (V-EDS 5) Should the last check result be kept after the program restarts? **Today:** no.
- (V-EDS 6) In Send Claims, should Close still send the claims without problems (the XConnect rule), or cancel the whole send? **Today:** it sends them.
- (V-EDS 7) Narratives: which procedures need one is not public. Options: the office keeps its own list, or the program remembers EDS's answers per payer and code, or it stays as it is. **Today:** it only repeats EDS's last answer for the claim, and never fires when the attachment service is off.
- (V-DXC2 1) What is DentalXChange's real size limit for PDF attachments? **Today:** 10 MB.
- (V-DXC2 2) Should PDFs that have only an owner password (they open without one) be allowed? **Today:** refused.
- (V-DXC2 3) Is the Send File button in Select Image wanted, or should PDFs from disk go into the patient's images first and be sent from there? **Today:** the button is there.

---

## Billing, payments and statements

- (B-ACCT 3) Ortho Case window: two long instruction texts were removed and the window made smaller. Please check it on screen.
- (B-ACCT 5) Sales tax: should the sales-tax procedure be excluded from discount plans? Should procedures on an ortho case be refused, as the adjustment method refuses them? **Today:** a discount plan can apply to it, and ortho-case procedures are taxed with the procedure method.
- (B-PAY 3) On your current Open Dental 25.3, please check how payment splits attach to payment plans in these cases: a split from another family when the plan's guarantor is in the paying family, plans that include the procedure or adjustment as production, and the confirmation for closed plans. **Today:** all are allowed (our reading).
- (B-PAY 4) Should a "Payment Plan Prepay" split also attach to old-style patient payment plans? **Today:** only to dynamic plans, because only those apply prepayments.
- (B-PRINT 2) Statement inserter barcode: can the office's folding/inserting machine read a Code 39 barcode with sequence, page, page count and end-of-set? Or does it need another format? Is the top-right margin a usable spot for its scanner?
- (B-PRINT 3) Should the statement e-mail subject also carry the patient's last name? **Today:** only the attachment name does.
- (M2-ADJ 1) Percentage adjustments that land on exactly half a cent: round away from zero, or to the nearest even cent? **Today:** away from zero.
- (M2-ADJ 2) Should a type's default percentage also apply when the procedure is attached after an amount was already typed? **Today:** only when the amount is zero.
- (M2-ADJ 3) Show the default percentage as a column in Definitions, Adj Types? **Today:** not shown.
- (M2-ADJ 4) Should Add Multiple Adjustments also limit percentages to 0 to 1000 with two decimals, as Edit Adjustment does? **Today:** no limit there.
- (M2-ADJ 5) Hide the Default % box for discount-plan adjustment types? **Today:** shown.
- (M2-PAYW 1) With the income-transfer limit turned on (off by default): money that older transfers moved from insurance or adjustments into unearned now counts as patient money. Acceptable, or exclude transferred money too (which would block reallocating any transferred payment)?
- (M2-PAYW 2) The Payment window's automatic Transfer button is not limited by that setting. Confirm.
- (M2-PAYW 3) When the Payment window is opened from an invoice, should that invoice be picked already? **Today:** nothing is picked.
- (M2-PAYW 4) The invoice drop-down shows each invoice's full total. Show what is still owed instead? **Today:** the full total.
- (M2-RECUR 1) The failure e-mail for recurring charges (off by default) goes only for card declines, not for office setup problems or no answer from the processor. Should those e-mail the patient too? **Today:** declines only.
- (M2-RECUR, phase B) Please confirm which processor answers count as card declines. **Today:** EdgeExpress/XWeb codes 001, 009 and 010; an X-Charge result containing DECLINE; a PayConnect 4xx answer other than 408, 409 and 429, or a status of Declined.
- (M2-RECUR 2) The same card is not e-mailed again for 7 days. Is that right, or should it follow the office's retry schedule?
- (M2-RECUR 3) Should the failure e-mail have its own on/off and template per clinic? **Today:** one for the office.
- (M2-RECUR 4) The Repeating Charges tool has never checked lock dates. Should it start refusing dates before a lock date? That would change things for offices that keep backdating on. **Today:** no check.
- (M2-RECUR 6, affects payment records) When PayConnect 2 answers a recurring sale with "success" but a status of Declined, the charge is recorded as paid and no failure e-mail goes out. Should the recurring run treat that as a decline? **Answered (2026-11-07): yes; fixed.** A PayConnect 2 sale, authorization or terminal charge answered with any status that is not an approval is a decline everywhere it is recorded: the recurring run marks it declined (with the $0 record payment every decline gets, never a payment of the amount) and e-mails the patient for a Declined status like other declines, and the payment window records it as declined. The other processors' recurring paths were checked: none records a decline as paid.
- (M2-DUN 1) To decide whether a statement e-mail is plain or HTML, the program matches the statement's message to the dunning message it came from. Acceptable? The alternative needs a field 26.1 does not have.
- (M2-DUN 2) Should HTML e-mails (statements and all others) also carry a plain-text version? **Today:** HTML only, as before. This would be its own small change.
- (M2-DUN 3) If an HTML statement body cannot be converted, should that patient be skipped in billing with an error? **Today:** it is sent as plain text.
- (M2-SDV 1) New statements made in the Account module start in whatever view the grid is in. Or should they always start in the regular view unless Statement Options says otherwise? **Today:** the grid's view.
- (M2-SDV 2) Statement lines for credits keep the report wording ("PatPay- Check", "InsPay- ...", "W/O- ..."). Use friendlier wording for patients? **Today:** report wording.
- (M2-SDV 3) The Service Date View has no plain "Balance" (charges less credits) column, only the patient-portion balance. Add one? **Today:** none.
- (M2-SDV 4) Date ranges in the Service Date View filter by date of service, with a balance forward (credits travel with their service). Or should a statement list the transactions dated in the range, as the regular view does? **Today:** by date of service.
- (M2-SDV 6) Should the Billing default for the Service Date View also be kept per clinic, like the other Billing defaults? **Today:** one setting.
- (M2-SDV 7) Electronic statements stay in the regular layout. Acceptable?

---

## Clinical, chart and treatment plans

- (B-TP 1) The Treatment Plan module's Add Procs button needs the Chart Module permission. Or should it need no extra permission, or Edit Treatment Plan instead? **Today:** Chart Module.
- (B-TP 2) Should the Chart's procedure buttons (including the auto-code buttons) also appear in Add Procs? **Today:** only Quick Buttons.
- (B-TP 3) Should the priority default to the one selected in the module's side list? **Today:** no priority.
- (B-TP 4) Add Procs refuses surface letters that don't fit the tooth, instead of silently dropping them as the Chart does. Keep?
- (B-PROC 2) Which licensed CDT file will the office import (for example the ADA's spreadsheet export)? If its column names differ from the ones the program knows, they can be added. **Answered (2026-11-07):** the owner buys the ADA CDT 2027 file and sends only its header row.
- (B-PROC 4) Should the D Codes import keep overwriting descriptions the office edited, as Open Dental does, or update only blank descriptions? **Today:** it overwrites them.
- (B-PERIO 2) With the Recession wording on, the "Ging Marg +" check box reads "Recession +", although it enters overgrown-gum values. Keep, or leave it "Ging Marg +"? **Today:** "Recession +".
- (B-PERIO 3) Is the "auto Att Ging" row in the right place, between Ging Marg and MGJ? Or should it go on the other side of MGJ, next to the probing rows?
- (B-MED 1) Converting a patient-entered "Other" allergy or problem: the "Other" entry is pointed to the chosen definition and set inactive. Or leave it as an inactive "Other" row? **Today:** pointed and set inactive.
- (B-MED 2) In the Medical window's right-click convert, is a Yes/No question about keeping the typed text fine, or should it open the edit window? **Today:** Yes/No question.
- (B-MED 3) Medication search by RxNorm code: should a partial code (the first digits) match, or only the exact code? **Today:** the first digits match.
- (M2-MED 1) A problem merge only counts the sheet and eForm fields that name the discarded problem and asks the office to edit them. Should it rewrite them instead?
- (M2-MED 2) When the discarded problem is the one set as "Problems indicate none", the setting moves to the kept problem. Right, or should the merge refuse?
- (M2-MED 3) Should the allergy merge also ask which allergy to keep, as the problem merge now does? **Today:** no.
- (M2-MED 4) The Database Maintenance check reuses a hidden "UNKNOWN ALLERGY" entry when it is the only one. Should it unhide it?
- (M2-NOTES 1) Procedure notes with two signatures: each signature is tied to its own signer. Or should the second be tied to the first signature exactly (then changing the first box's user would clear the second)? **Today:** each to its own signer.
- (M2-NOTES 2) Who may edit a note signed by two people? **Today:** only a user with Group Note Edit Signed. Should either signer be allowed (which clears the other's signature, with an alert)?
- (M2-NOTES 3) The merge question appears on any save that writes a new note version, not only when the text changed. Keep? Asking only on text changes could put older text back over newer text. Another option: don't save a new version when only line breaks differ, which would also stop the duplicate versions the old program writes.
- (B-SHELL 3) Ortho chart: should the edit lock be released after a period of no activity? **Today:** no.
- (B-SHELL 4) Ortho chart: a forced or automatic log-off saves open ortho charts without asking, as 24.3 did on automatic shutdown. Wanted, or should unsaved edits be thrown away? **Today:** saved.
- (M2-SHELL 3) Family module Gender row: it now shows the same text as the new Chart row (blank instead of "Unknown"). Keep, or bring the old text back?
- (M2-MISC 1) Automation "clinic mismatch" on Procedure Complete also looks at other procedures completed earlier the same day. Should only the procedures just completed be checked (a one-line change in four places)? **Today:** the whole day's procedures.
- (M2-MISC 2) A procedure with no clinic on a patient who has a clinic counts as a mismatch. Right? **Today:** yes.
- (BIN-A 2) The business layer's tooth chart image (the treatment plan and perio PDFs of the API and mobile paths, which v0.0 does not ship) is a plain "Tooth chart image not available" picture since Sparks3D went. Acceptable for v0.0, or draw it another way (for example a 2D renderer in the business layer)? The program's own sheets and printouts draw the chart as before.
- (BIN-A 3) Free text and ortho hardware on the tooth chart: only Sparks3D drew them. Hide the Draw tab's text controls (Add, Move Text) and the ortho hardware drawing, or keep them (saved but not drawn)? **Today:** kept. **Answered by the chart track (TC-5, TC-6; every computer's chart since TC-8):** kept and drawn. The WPF chart draws free text (moved with Move Text, recoloured with Change Color, both saved) and the orthodontic hardware (brackets, wires and elastics, and ortho mode), as Open Dental does; no control is hidden.

---

## Scheduling, staff and time cards

- (B-APPT 2) "Provider time bars on operatory background" (25.2): **today** each operatory column is tinted where the provider is scheduled. Or should the far-left provider bar's time be drawn on the operatory background instead?
- (B-APPT 3) Reopening a completed task while the appointment already has an open task: keep it silent, or warn in Task Edit? **Today:** silent.
- (B-APPT 4) Should the far-left provider columns also use the secondary color setting? **Today:** no.
- (B-APPT 5) Is the toolbar the right place for "Wait Rm", and is remembering its window position per Windows user on each computer right? **Today:** both as described.
- (M2-APPT 1) Right-click Cut of a blockout needs Blockout Delete only. Or Delete and Add? **Today:** Delete only.
- (M2-APPT 2) Should the Family Recall toolbar button also follow "Make Recall automatically searches for openings"? **Today:** only "Recall for Family" in Other Appointments follows it.
- (M2-APPT 3) Replacing overlapping blockouts when pasting or editing now needs Blockout Delete (or Blockouts Flagged) for the replaced ones. Keep?
- (M2-APPT 4) Keep the extra audit entry under Blockout Delete when the Cut-Copy-Paste window pastes with Replace Existing?
- (B-TASK 1) Task filter: Clinic A (or Region 1) still shows tasks with no clinic, as before, so "Unassigned" is only useful on its own. Or should no-clinic tasks show only when Unassigned is picked? That would change what every default filter shows. **Today:** shown.
- (B-TASK 2) With the Region default filter and a clinic that has no region, should the list default to Region: Unassigned? **Today:** no region filter.
- (B-TIME 1) Should the Decimal/Colon choice in the Time Card window also change the time card grid itself? **Today:** totals only.
- (B-TIME 2) Should Reports To also list the supervisor themselves, or people who report to them indirectly? **Today:** direct reports only.
- (B-TIME 4) Should Manage Time Cards get the Reports To filter, and print with the new single-format totals row? **Today:** no.
- (M2-EMP 1) While a clock-out time is in the future, should the status read "Working" instead of "Manual Entry"? **Today:** "Manual Entry".
- (M2-EMP 2) A clock-out on a later day spans two dates, which Calc Daily reports as an error. Acceptable, or limit future clock-outs to later today plus overnight shifts? **Today:** later days are allowed.
- (M2-EMP 3) Should every clock-event edit be logged with old and new times? **Today:** only future clock-outs.
- (M2-EMP 4) Should the Employees list search also match job titles? **Today:** no.
- (M2-EMP 5) Printed time card with clinics: should the Clinic column give up room so a long later-day date always prints in full? **Today:** there, only the time may fit.

---

## Forms, sheets and printing

- (B-EF 1) Forms that eClipboard and Web Forms left in the office data show as "Waiting for patient" and "Submitted online" (not the spec's "In eClipboard" and "Web form", since those services are gone). Acceptable?
- (B-EF 2) Should scanned form documents show under the Sheets filter, or only under Both? **Today:** Sheets.
- (B-EF 3) An eForm with no status: show "None", or a blank cell? **Today:** "None".
- (B-EF 4) Forms imported before the switch-over (including by Open Dental 25.3) don't show as Imported. If the office database shows that Open Dental's import audit entries carry the form's number, they can be counted too (the same database look as Switch-over item 1). **Answered (2026-11-07, with Switch-over item 1):** setting names do not show audit entries, so the switch-over converts none: the audit trail is copied as it is. Patient Forms shows an eForm as Imported when an `EFormImport` audit entry carries its number and says "imported", the test it applies to its own entries; an Open Dental entry written that way counts, and an eForm without such an entry shows its status.
- (B-EF 6) When radio button items are renamed, should the conditional fields that depend on them be updated too (the Ins Pick editor already does this)? **Today:** no.
- (M2-EF 2) In the import window, should a blank answer clear a stored patient field by default? **Today:** unchecked (not cleared).
- (M2-EF 3) Should patient fields be allowed as Auto Import fields? **Today:** no.
- (M2-EF 4) The Submitted box shows in Fill Sheet only for patient forms, medical histories and consents. Other sheet types too?
- (M2-SHEETS 1) Right-to-left text (Hebrew, Arabic) in a left-aligned field stays on the left, as designed. Or treat "left" as "start" so it sits at the right edge? **Today:** stays left.
- (M2-SHEETS 2) Right-to-left, Indic and Chinese/Japanese text in sheet PDFs is a picture (it can't be searched or copied) until the PDF library is replaced. Acceptable?
- (M2-SHEETS 3) The duplicate-field warning also covers output fields (patient name, date), which are often placed twice on purpose, so every such add asks. Keep?
- (M2-SHEETS 4) The language list adds the native name for non-Latin languages ("Russian (русский)"). Keep, or English only as before?
- (B-PRINT 4) Kiosk-filled sheets are copied to Imaging when the patient saves them at the kiosk. Acceptable, and can every kiosk computer reach the image folder? A copy that fails is only noted in the audit trail. Waiting for staff review instead would need a new field.
- (B-PRINT 5) Print Options appears when a print situation has Prompt on, and from every print preview's Print button. Should it also appear for prints that go straight to the printer?
- (B-PRINT 6) Should the remembered print choices be cleared at log-off? **Today:** kept until the program closes.
- (M2-MISC 4) Is the Print Options wording right: "From Statements setting", "Last used this session (...)", "Chosen in this window (...)", "Status: Offline"?

---

## Imaging

- (B-IMG 1) Should "Copy without drawings" also be added to the old Images module, if any computer still uses it? **Today:** new module only. **Settled (2026-11-08) by the removal:** the old Images module and its option ("Use old Imaging module interface, pre 2020") are removed in the clean-up stage of the .NET 10 move (S0-6), so every computer uses the current module, which has "Copy without drawings". A database with the option on gets the current module; the setting stays stored but is no longer read.
- (B-IMG 2) Audit text: "... copied to clipboard without drawings". Fine, or should both copy commands log the same text?
- (B-IMG 3) Please read the scanning setup Details message once and say whether the wording fits the office.
- (M2-IMG 1) The "allow PDF JavaScript" setting covers PDFs in the Imaging module only. Should it, or a second setting, also cover PDFs shown in eForms, statements and sheets?
- (M2-IMG 2) True 16-bit X-ray images still open with the full range, without automatic brightness windowing. Should they get the same automatic windowing as 12-bit images? That needs a real 16-bit sensor sample to tune.
- (M2-IMG 3) 12-bit images whose stored windowing is the import fallback (4095) now show the full range instead of an almost black picture. This only changes images that were unreadable before. Confirm.
- (TWAIN-W 2) Should the windowing slider show a legacy stored value above 255 the way the picture now reads it (divided by 16)? That is a change in `ControlImageDisplay`. **Today:** the picture reads 256 to 4094 on the 12-bit scale, while the slider shows any stored value above 255 as 255.
- (TWAIN-W 3) Should the 12-bit import store 255 instead of 4095 when it finds no edge, so new rows are on the 0-255 scale? They look the same either way. **Today:** 4095.
- (TWAIN-W 4) Who runs TWAIN-W's read-only query on the clinic database before the display change ships, and should staff be told about the two groups of images whose display changes?
- (TWAIN-W 5) An unreadable `.dcm` dropped into a patient's folder is now attached without windowing, instead of stopping the folder scan with an error. Confirm.
- (CLAIM-DCM 5) A document whose name ends in `.DCM` (upper case) is imported as DICOM, and claims accept it, but the Imaging module does not show it as DICOM (its display path tests `EndsWith(".dcm")`). Should the display path accept any case too?

---

## Imaging capture (native X-ray and camera capture)

The questions about the new capture work (the capture session, unassigned captures, 16-bit X-rays, the camera, mount templates) are in their own list, [imaging-capture-owner-questions.md](imaging-capture-owner-questions.md). The ones marked [BEFORE PATIENT USE] must be settled before the capture session is turned on for patients. Settled since that list was written: 16-bit is the default once a computer's sensor passes Test capture (until then that computer captures 8-bit), and a 16-bit X-ray that cannot go back into its own slot is filed as an 8-bit copy with the 16-bit original kept. Answered on 2026-11-07 (that list, "Answered on 2026-11-07"): counsel reviewed the capture's regulatory position with no changes; the capture session is on by default for new installs; 16 bits stay the default; patient use waits for the phantom comparison; the sensor is a Twain Radiograph device only.

**First clinic run (OP1, 2026-11-22; test build 1f46c8deâ¦bf31 from main 127cc2bdf, local MariaDB, made-up patient).** The 64-bit route works with the real k-Sensor: KPTwain 1.2 64-bit opened through the setup's TWAINDSM.dll 2.5.1.0, and Test capture passed (16-bit, polarity and byte order right, metadata through). Launch check 6 is not passed yet: no verification record was written, because the run sheet (W:\OpenPMS\INSTRUCTIONS.md) did not say to click **Write verification record**, so Acquire saved 8-bit ("no Test capture record"). The run sheet is corrected; the next visit repeats Test capture with the record, then Acquire, which should save 16-bit. Follow-ups:
  - **Code signing (needs the owner).** Windows 11 Smart App Control, on by default on new PCs, blocks the unsigned build (DataConnectionBase.dll; the connection tool exited -532462766), and turning it off cannot be undone without a reset. Office PCs with it on cannot run an unsigned release. **Question:** get a code-signing certificate or a signing service for the setup and the program's files before the office release? Recommended: yes.
  - The Test capture window makes the record step easy to miss: it should say plainly after a passed test that no record is written until the button is clicked. Small fix, next imaging package.
  - MariaDB 13.0.2 was used (the run sheet said 10.11): the schema loaded and the program ran, but 10.11 stays the supported server; the run sheet now says so.
  - Main is still version 0.0, so a dry run carries v0.0's release notes (.NET Framework 4.8, 32-bit). The next test or release build gets version 0.1 and its own notes; the stale notes are off the share.
  - new-database.sql still has the "eClipboard" image category, a feature the fork excludes: scrub it in the init SQL (InitSqlScrub).

The review that put the capture work on main added these:
- **The capture design document.** The original names the clinic's vendor products and a private driver project, so main has a neutral edition, [imaging-capture-design.md](imaging-capture-design.md), with those names replaced by their role; the original stays in the private coordination notes. Is the neutral edition fine to keep in the public repository? **Today:** the neutral edition is on main.
- **Deleting an empty session mount.** When the capture session made a mount and saved nothing into it, the mount is deleted and logged under Image Create, as the design says. Log it under Image Delete instead, as the Imaging module does for an empty mount? **Today:** Image Create.
- **Patient names in local capture files.** Unassigned captures and the journal store their notes on the computer with the patient number. Should they also carry the patient's name (easier recovery), or only the number (less patient data on disk)? **Today:** patient number only.
- **Reassign.** Moving an unassigned capture to another patient needs Image Edit and a reason. Should it also need Image Create, since it adds an image to that patient's chart? **Today:** Image Edit only.

## Vendor integrations

- (V-BRIDGE1 3) Overjet: opening a patient directly needs parameter names from Overjet. Once known, ship a default template or leave it to the office? **Today:** left to the office.
- (V-BRIDGE1 4) DTX Studio: we need the DTXsync.exe command-line specification from DTX/Envista, including how Acquire differs. Ship default patient arguments or leave them to the office? **Today:** left to the office.
- (V-BRIDGE1 5) IAP: which sign-in address should the office enter? Should the IAP button also show when a plan is edited from the insurance plan list? **Today:** hidden there, as in 24.3.
- (V-BRIDGE2 1) Clerri: is a plain launch useful without the data sync? If yes, we need Clerri's dashboard address, and whether it takes the key and the patient number.
- (V-BRIDGE2 2) Clerri launches are logged to a file on the computer. Should they go to the security log instead? **Today:** local file.
- (V-BRIDGE2 3) Clever One: please confirm the hand-off file with VATECH (file name, folder, fields). The default folder is under Program Files, which ordinary users often can't write to.
- (V-BRIDGE2 4) Trophy (Enhanced): should we build the Chart Number option and the Carestream Imaging 8 setup (blank path) from the current public page? **Today:** not built.
- (V-GP 1) Will Global Payments give the office Cloud Integration access with its own App ID and App Key, or only through a partner such as Open Dental? If only through a partner, V2 stays set up but turned off, like EdgeExpress.
- (V-GP 2) Please ask Global Payments (sandbox) how a terminal sale returns a reusable card, and for the full list of device statuses and a declined/cancelled answer, so our reading of them can be confirmed.
- (V-GP 3) Program names: keep the neutral "Global Payments V1" / "Global Payments V2", or use Open Dental's "Global Payments from www.globalpaymentsintegrated.com"? **Today:** neutral.
- (V-GP 4) An office with both X-Charge and EdgeExpress turned on must turn one off before it can save either setup window. Acceptable, or check only when turning one on?
- (V-GP 5) Should V2 also appear among the prepaid-card buttons in the insurance payment window? **Today:** no.
- (V-PEARL1 1, V-PEARL2 2) The Pearl client secret is stored as plain text, as Open Dental stored it. Should it be encrypted, masked and high-security like the other vendor secrets (a small follow-up)? **Today:** plain text.
- (V-PEARL1 2) A Pearl request left "Polling" or "Uploading" by a computer that closed mid-request stays stuck; its image can't be sent again until the image is deleted. Should such requests time out, for example after 15 minutes? **Today:** stuck, as in 24.3.
- (V-PEARL1 3) Polling stops at the first failed poll (staff send again by hand). Should it retry a few times first?
- (V-PEARL2 3) Are our readings of the Pearl log settings acceptable: level 1 means uploads and results, values above 2 mean verbose, and one setting for the whole practice?
- (V-PEARL2 4) Error text that Pearl returns (with private details removed) goes into the local log, and it could echo request details. Or log only the error code?
- (M2-02 3) PaySuite stays hidden. Should PaySuite be in scope at all for a US office?
- (B-DBM 4) Database Maintenance shows the X-Charge "Tokens" tool even when X-Charge is not turned on, which goes against the rule of no vendor screens before the office turns them on. Hide it unless X-Charge is on? **Today:** always shown.
- (BIN-B 1) Topaz: where does the clinic's Topaz software put `SigPlusNET.dll`? If it is not under a `Topaz Systems` folder or `%windir%\SigPlus`, the office copies it into the program folder (a release then carries it to the other workstations), or the search gets that folder added. **Superseded (2026-11-08):** not used; removed in the clean-up stage of the .NET 10 move.
- (BIN-B 7) Signatures on a workstation without Topaz: a signature of hex digits only that the program's own box does not accept says "Topaz signature pad software is not installed."; every other failed signature says "Invalid Signature". If the office database holds Topaz signatures, do they all consist of 0-9 and A-F only? And is the wording right, or should it say both ("Invalid Signature, or a Topaz signature: Topaz signature pad software is not installed.")? **Superseded (2026-11-08):** not used; removed in the clean-up stage of the .NET 10 move.
- (REL-FILES 7) DemandForce's extract still names "Open Dental" as the management system (the name DemandForce's import expects). Keep it, or send the product name? **Today:** "Open Dental".

---

## Reports

- (B-ACCT 4) Daily Payments merchant fees: keep the 24.3 signs (PayConnect fees shown as stored; "after fees" = amount plus fee)? Include every payment processor except "None"? **Today:** both yes.
- (B-SERVER 2) Incomplete Procedure Notes, "Include all procedures with no notes": a procedure in a group note that has text counts as noted. Right?
- (B-SERVER 3) Should a note of only spaces count as "no note"? **Today:** yes (for the new option).
- (B-SERVER 6) Report Setup: should checking the "all procedures" default also lock the same-day default, as the report window does? **Today:** they are separate.
- (M2-TERM 1) Report column headers: "Standard Fee" (fits, or wraps to two lines in Capitation) or the full "Standard Provider Fee" (wraps everywhere)? Exported files keep the old names ("UCR Fee" and so on). Using new names in exports too is against the spec and needs your explicit approval.
- (M2-UDS 2) The UDS+ patient file carries the internal patient number so the health center's tools can group a patient's rows (no name or other identifier). Acceptable, or use a made-up number per file?
- (M2-UDS 3) Should the insurance category in UDS+ come from a mapping the office can edit, instead of the fixed list? **Today:** fixed list.
- (RDL-SRC 1) An office's own RDL report can show its parameter window two or three times (the 2.9.3 engine with ODR, unchanged). Should ODR remember the parameters it was given for the same report run, for example the last values per query for a few seconds, or should the viewer build the report once?
- (RDL-SRC 2) Three report resources are embedded but no code opens them (`Unsched_Treat_Plans`, and the two accounting reports hard-coded to 2007 with an unfilled `@@@ConnectionString@@@`). Keep them as samples, or delete them? Only an office's own `.rdl` files reach the RDL viewer.
- (RDL-SRC 3) Delete the 8 old files in `RdlEngine/Definition` that are not compiled? One carries an older GPL header instead of the LGPL one.
- (RDL-SRC 5) ODR's error handling: `ODR.DataConnection.GetTable` shows a failed query in a message box and returns an empty table, and the `Integer` parameter type stores a `long` and unboxes it as `int`, which throws. Leave them, or fix them?

---

## Other

**Audit trail and privacy**
- (B-AUDIT 1) Should a sheet or eForm import also log the other patient fields it changes (name, address, phones), as Edit Patient does? **Today:** only preferred name and clinic, plus the existing "imported" entries.
- (B-AUDIT 2) Should the PT Dental (Paperless Technology) bridge log the patient changes it imports? **Today:** no entry.
- (B-AUDIT 3) Moving patients in the Clinics window: one audit entry per patient, so each patient's own audit trail shows it? Or the single summary entry only? **Today:** one per patient.
- (M2-AUDIT 1) Birthdate changes read "changed from '' to '3/4/1985'" when a birthdate is added or removed. Fine, or "birthdate added" / "birthdate removed"?
- (M2-AUDIT 2) Should a patient created with a birthdate get an "added" entry? **Today:** yes.
- (M2-AUDIT 3) The Audit Trail shows real birthdates, even to users who see them masked in Edit Patient Information. Acceptable, given who has the Audit Trail permission?
- (M2-SHELL 1) The "patient selected" audit is one on/off switch in Global Security Settings, off by default. Enough, or per user group?
- (M2-SHELL 2) Should picking the patient who is already loaded also write an entry after 60 seconds? **Today:** only a change of patient is logged.
- (M2-MISC 5) Help, About (diagnostics) shows the patient number and the MySQL user name, as before. Keep both?

**Database Maintenance, server and setup**
- (B-DBM 1) The Invalid Characters tool cleans a chosen list of fields. Should it also clean patient notes, procedure text fields, wiki history or eForm answers? Each one is a single line to add.
- (B-DBM 2) The Raw Emails tool also removes two invisible joining characters, which changes some emoji and some scripts (for example Persian). Keep?
- (B-DBM 3) The Base64 Documents tool deletes placeholder rows without an image-delete log entry, as the Images module does. Should it log one?
- (B-SERVER 4) Replication read delay: should it start after any write by the program (including background writes), or only after a user saves? **Today:** any write.
- (B-SERVER 5) Does the office run a Middle Tier server? If so, is starting the read delay when a signal is sent acceptable?
  **Answered (2026-11-08):** no Middle Tier server in the office; the Middle Tier is still moved to .NET 10.
- (B-SHELL 2) Should the File Monitor keep requiring the Setup permission? Open Dental leaves it open to everyone. **Today:** Setup required.
- (M2-MISC 3) Referral phone: a plain 10-digit number is stored as 10 digits (anything else as typed). Or store exactly what was typed? **Today:** 10 digits.
- (REN-PROOF 3) The HL7 service (not shipped in v0.0) has two default names, as in Open Dental: `OpenPmsDentalHL7` (`HL7ServiceName`) and the run-time fallback `OpenDentalHL7` (`HL7ServiceFallbackName`), which adopted HL7 setups saved. Unify them later? That needs a migration of the saved `HL7Def.HL7ServiceName` and eCW `HL7ServiceName` values, or both names accepted at start. **Today:** two names.

**New office database** (`db/init/new-database.sql`)
- (INIT-SQL 1) The adoption record of a new database is `new=1;source=24.3.63.0;level=...;adopted=db/init/new-database.sql`. Should later steps that depend on the database's origin treat a new database as one of the 24.3.63 line (this choice), or as its own origin?
- (INIT-SQL 2) Keep the ADA claim form layouts (ADA 2006, 2012 and 2024 rows, which name the image files `ADA2006.gif`, `ADA2012.gif`, `ADA2024_J430.gif`), the clearinghouse list and the Canadian network rows, or start without some of them? **Today:** all kept.
- (INIT-SQL 3) The two placeholder providers ("Dentist" `DOC`, "Hygienist" `HYG`) and six operatories: keep them as a starting point, or start with one provider, or none?
- (INIT-SQL 4) The seed's legacy tables (`anesth*`, `files`, `instructor`) are documented schema exceptions and are in the file. Drop them with a fork migration step one day?
- (INIT-SQL 5) The development seed still has the eServices notices and Open Dental's mass e-mail templates (with links to Open Dental's site); only the new database file drops them. Remove them from the seed too?

**Wiki**
- (B-WIKI 1) The read-only wiki copy has one folder for the whole office, by default inside the A-to-Z folder (usually on the same server as the database). Should there be a folder per computer, or a second copy, so it is still there when the server is down? **Today:** one folder; Wiki Setup warns about this.
- (B-WIKI 2) Should linked files outside the wiki folder also be copied into the read-only copy (with a size cap)? **Today:** they stay links.
- (B-WIKI 3) Is "off" the right default for the wiki search limit?
- (B-WIKI 6) Is Setup the right permission for the read-only copy settings? Should users who are not administrators see drafts of locked pages (as the spec says; they cannot edit them)?

**Remembering choices after the program closes** (B-PROC 5, B-TIME 3, M2-ERA 1, M2-INSV 3, M2-NOTES 4)
**Today**, each of these is remembered only until the program closes, or as noted. Which of them should be remembered longer?
- Fee Tools: the last tab used (B-PROC 5).
- Time Clock: the Reports To choice (B-TIME 3).
- ERAs Automatically Processed report: Show PreAuths and Show $0 Payments (M2-ERA 1).
- Insurance Verification List: the carrier pick (M2-INSV 3).
- Procedure note window size: remembered per user on each computer. Or per user everywhere (M2-NOTES 4)?

**Canada-only items** (hidden outside Canadian English and French settings; "not needed" is a fine answer)
- (M2-CAN 7) Is the Canadian part needed at all for a US office?
- (B-PROC 1) Which Canadian codes should the default Canadian code groups hold? Only Fluoride (12111) and Sealant (13401) are filled in, and a reset keeps the office's current codes for the others. When several groups match one default, merge their codes instead of taking the group with the most benefits?
- (M2-CAN 1) Canadian code update: the confirmation word "UPDATE CODES" is shown in the prompt. Keep it visible, or hide it and document it elsewhere?
- (M2-CAN 2) Updates match existing codes by number in any category, so a CDHA or DAC file can also rename a CDA code with the same number. Limit updates to the system's own category?
- (M2-CAN 3) RAMQ: printed claims drop the trailing R by category. Or for every R code that the RAMQ import created, even if moved to another category?
- (M2-CAN 4) Pay Labs: labs are paid first, up to their billed fee, and the procedure keeps the rest. Is that what the carriers need?
- (M2-CAN 5) DAC English and French share one "DAC" category. Separate them?
- (M2-CAN 6) Codes with commas or hyphens from before 26.1 can only be replaced, not renamed. Is a rename tool wanted, or is the Database Maintenance list enough?

**Dental school** (excluded by your earlier decision)
- (M2-02 4) If dental school features ever come back, permission 81 has a much wider meaning in 25.4, and a list of the groups that hold it would be needed first. No action today.
- (M2-01 3) The dental school grade fields default to -1 in the database. Keep, or set it only in code? This has no effect on the office.

**Developer housekeeping** ("your call" is a fine answer for all of these)
- (B-PRINT 7) Remove the old grid text/".xls" export, which nothing uses now, in a later cleanup?
- (M2-HIST 8) Remove the ten unused Insurance History help entries from the preference help file?
- (M2-CARRIER 3) "Create Preauth Now" repeats the Treatment Plan module's preauthorization steps. Make the Treatment Plan button use the shared steps so the two can't drift apart? (A small change in a busy file.)
- (V-GP, phase B) One Global Payments question box uses the plain Windows message box, because the program's own box can't set the default button. Acceptable, or add that option to the program's box?
- (M2-01 4) Mark Open Dental's remaining headquarters-only values as reserved in a follow-up?
- (B-EF 5, B-WIKI 4, M2-SDV 5) How the program numbers its own saved per-user settings: values from 100 up, the existing value 25 with Fkey 1 (wiki search limit), or AcctProcBreakdown with Fkey 1 and 2 (Service Date View). Keep these, or register values of the program's own?
- (ORG-CLEANUP 1) Delete `D:\Unversioned` on the development PC? Earlier Debug builds of Direct2dWrapper created it; nothing writes there any more.
- (ORG-CLEANUP 2) Should the fork's merge driver drop configuration lines that one side removed (as with `ODXam`)? Until then, the strip step in ORG-CLEANUP is needed after merges that touch `OpenDental.sln`.
- (ORG-CLEANUP 3) Keep `OpenDental.Local.slnf`, or point the test runner and the host check at `OpenDental.sln` and delete the filter?
- (BIN-A 5) Delete the DirectX 11 preference (`DirectX11ToothChartUseIfAvail`) and `computerpref.GraphicsUseDirectX11` in a future fork migration, or keep them? Nothing reads them since Sparks3D went. **Answered on 2026-11-15: keep them** (File > Graphics to match Open Dental 26.2; TC-8b reads them again).
- (BIN-B 2) Remove the always-false AvaTax branches in the shared billing files (Adjustments, Procedures, RepeatCharges and their windows), and the disabled stub with them, in a later cleanup?
- (BIN-B 3) RAMQ and QRDA zips store their XML in the system code page, as DotNetZip did. Keep that, or store UTF-8?
- (this release) `Direct2dWrapper.dll` is neither one of the build id files (`ForkReleases.ListBuildIdFiles`) nor an input of the build time stamped into `OpenPmsDental.exe`, so a release that changes only that native DLL has the build id of the release before it, and no workstation pulls it. Add it (it must then build before `OpenPmsDental`)? **Today:** not part of the build id; it changes rarely. (The same release work fixed its build: Debug and Release now have their own output folders, so a Release build can no longer pick up the Debug DLL.)

---

## Added by the combined review (2026-11-01)

These came out of the review of all 55 packages together. Items 4, 5, 8 and 10 above also came up again there.

- **Setting names (B-PRINT, B-ACCT, B-PAY, B-PERIO, B-ERA, B-CLAIM, B-APPT, B-SERVER, B-WIKI, B-SHELL, M2-SHELL).** For settings whose Open Dental name isn't confirmed, four are stored under a guessed Open Dental name and the rest under a name starting with "Fork". Use one rule? The reviewer suggests renaming the four to "Fork" names now, while no office database has them, and adding a test that keeps it that way. **Today:** mixed.
- **Pearl secret (V-PEARL1, V-PEARL2).** Same as the Pearl question under Vendor integrations: encrypt it like the other vendor secrets? It is a small change.
- **CareCredit 25.2/25.3 items (B-ACCT, V-PC).** The CareCredit bridge was rebuilt during the decoupling work, but four 25.2/25.3 CareCredit items (merchant fee adjustments, self-service, the Net Amount column, the refund adjustment type) were never built, and the planning documents disagree about whether they are excluded. Build them in a vendor package, or record them as excluded? **Today:** not built.
- **PaySuite and Clerri (V-BRIDGE2, M2-02).** Record in the owner decisions that PaySuite is not built and that Clerri is built only as a plain launch, turned off. **Today:** that's how they are built, but it isn't written down as your decision.

---

## Inventory tracking (INV, 2026-11-02)

From `docs/specs/inventory-tracking.md`. Each item says what the program does today. You answered them on 2026-11-02 ("all as recommended"): every item below is decided except INV 16, which is settled when the packages are merged. Only INV 11 changed the program.

- (INV 1) **[SWITCH-OVER]** Inventory tracking is off on every database, new databases too, until the office turns it on (Supply Inventory, Inventory, Setup). Should a new database start with it on? **Today:** off everywhere. **Decided (2026-11-02):** kept as built.
- (INV 2) The Materials used window opens by itself only when at least one of the completed procedures has default materials. Open it for every completed procedure instead? **Today:** a procedure without default materials gets materials only from the procedure edit window (Materials). **Decided (2026-11-02):** kept as built.
- (INV 3) Completions without a user (another thread of the program, an interface, the Middle Tier server's own work) and a cancelled Materials used window deduct nothing; the procedures wait on Supply Inventory, Inventory, Awaiting Materials. **Today:** as described; no default materials are ever deducted without a user confirming them. **Decided (2026-11-02):** kept as built.
- (INV 4) A use is never blocked: stock can go below zero, and a use with no lot in stock is recorded without a lot. **Today:** allowed. **Decided (2026-11-02):** kept as built.
- (INV 5) Stock without a lot (opening balances, receipts without a lot number) is used after every lot, and lots without an expiry date after dated lots. **Today:** as described. **Decided (2026-11-02):** kept as built.
- (INV 6) Any change away from complete (to treatment planned, existing, referred, deleted...) puts back what the procedure used, also while tracking is off. **Today:** as described. **Decided (2026-11-02):** kept as built.
- (INV 7) The first time the office turns tracking on, the new Inventory Adjust permission (counts, adjustments, the repair) is granted to every user group that has Supplier Edit, unless a group has it already. No grant instead? **Today:** granted then (the update itself grants nothing). **Decided (2026-11-02):** kept as built.
- (INV 8) Changing the materials of a procedure that does not await materials (already confirmed) needs Completed Procedure Edit Misc for the procedure's date; without it the window is read-only. **Today:** as described. **Decided (2026-11-02):** kept as built.
- (INV 9) The low-stock alert uses a new per-supply reorder level (blank: no alert), not the existing Stock Level (the level an order fills up to). **Today:** as described. **Decided (2026-11-02):** kept as built.
- (INV 10) The expiring alert also lists lots that have already expired; the lead time is 30 days (Setup; 0 turns it off). **Today:** as described. **Decided (2026-11-02):** kept as built.
- (INV 11) A stock count was kept only while its window was open; closing it (after a warning) lost an unposted count. Save counts in progress? **Decided (2026-11-02):** saved, and built: each counted line is saved as soon as it is entered (a closed window, a crash or a power cut loses nothing), the office has at most one open count at a time, which any user allowed to count can resume, a supply counted again replaces its earlier line with a new snapshot of the program's quantity, posting works as before and closes the count, and an open count can be discarded after a confirmation, with a security log entry. Supply Inventory shows an open count (who started it, when, how many lines) and offers to resume it (`docs/specs/inventory-tracking.md`, 2.7 and 3.5; step 2026110213).
- (INV 12) Dispensing through the dental-school dispensary deducts stock while tracking is on, but the program has no window that dispenses. **Today:** only the business layer. **Decided (2026-11-02):** kept as built.
- (INV 13) The window opens once the program is idle and the window the user worked in is active again; while a message box of the same action, or another program, is in front, it waits. **Today:** as described. **Decided (2026-11-02):** kept as built.
- (INV 14) Quantities have at most two decimals (the data layer writes decimal numbers with two). **Today:** rounded to two. **Decided (2026-11-02):** kept as built.
- (INV 15) A supply with stock history (receipts, uses, counts, dispensing) cannot be deleted; it can be hidden. **Today:** refused with that message. **Decided (2026-11-02):** kept as built.
- (INV 16) **[DECIDED BY THE RULE, CONFIRM]** The task gave inventory tracking the fork range 10050-10099 for new permissions and alert types. Alert types (and the window an alert opens) are stored in `tinyint(4)` columns of Open Dental's tables, which hold at most 127, so 10050 cannot be stored there, and widening Open Dental's columns would break the schema match. The alert values were therefore moved to the tinyint rule shared with routine maintenance (`docs/port-notes/FORK-ENUMS.md`: counting down from 127; inventory has 119-112): Inventory Low Stock 119, Inventory Expiring 118, the Supply Inventory window 119. The permission Inventory Adjust keeps 10050. Values stored in released databases can never be renumbered. **Today:** 119, 118 and 119.
- (INV 17) An implant deducted without a lot number (no lot in stock, for example stock that came in as an opening balance) breaks traceability. The Materials used window asks for the lot number printed on the package (typed or scanned), and moves that quantity from the stock without a lot into the lot; leaving it without a lot needs a reason, which goes to the audit trail. Block it outright instead? **Today:** a lot number, or a reason in the audit trail. **Decided (2026-11-02):** kept as built.
- (INV 18) Quantities typed into the inventory windows (Materials used, Receive, Stock Count) above 9999 are refused, because a barcode scanned into a quantity cell instead of the Scan box looks like that; Materials used asks first above 99, Receive above 999 or the ordered quantity, Stock Count above 999 or ten times the program's quantity. **Today:** as described. **Decided (2026-11-02):** kept as built.

---

## Remembered SSH host keys (SSH-KEYS, 2026-11-02)

From `docs/specs/ssh-host-keys.md`, section 11: your decision of 2026-11-02 (tier 1, question 1) is built (stored on first use, a changed key refused before credentials, accepted or removed in Setup, SSH Host Keys by Security Admin). These details were taken as the simplest option consistent with it. Change any you disagree with. **Answered (2026-11-07): SSH-KEYS 1 to 7 are kept as built.**

- (SSH-KEYS 1) The key a connection refused is recorded on the server's row, and Accept Changed Key accepts exactly that key (the business layer refuses when another key was refused since the window was filled). There is no "check the server now" button and no typing of a fingerprint. **Today:** as described.
- (SSH-KEYS 2) One audit entry per refused key, not per refused connection: the HL7 service retries every minute. **Today:** as described; every refusal still updates the time on the row.
- (SSH-KEYS 3) A refusal raises no alert; the caller's error message, the row and the audit trail say it. **Today:** no alert.
- (SSH-KEYS 4) The first key is stored after the connection signed in, so a server that refuses the password does not get its key stored. **Today:** as described.
- (SSH-KEYS 5) The table is not cached: each connection reads its server's row, so an accepted or removed key counts on every workstation from its next connection. **Today:** as described.
- (SSH-KEYS 6) Servers are told apart by host name and port, the name compared without case; a server's name and its IP address are two servers, as in OpenSSH's `known_hosts`. **Today:** as described.
- (SSH-KEYS 7) The audit type is a new fork-only permission value, SSH Host Keys (10100, the first of the reserved range 10100-10149 in `docs/port-notes/FORK-ENUMS.md`), audit only: never granted and not in the security tree; the window and both changes need Security Admin. **Today:** as described.

---

## The program's database account (DB-SEC, 2026-11-02)

From [specs/db-connection-security.md](specs/db-connection-security.md), section 8. Your answers of 2026-11-02 settled the other points of that section; these two were answered on 2026-11-07. DB-SEC 3 was decided by the lead on 2026-11-11; you can still choose otherwise.


- (DB-SEC 1) Every backup asks whether the server replicates (`SHOW MASTER STATUS`, `SHOW SLAVE STATUS`: the server-wide rights `BINLOG MONITOR` and `REPLICA MONITOR`) and runs `FLUSH TABLE` on each table it copies (the server-wide right `RELOAD`). Should "access denied" on the replication question count as "no replication" when the program's replication table is empty, and the flush be skipped, so that the program's account `opmsd` needs no server-wide right at all? **Today:** all three are granted ([db-connection-security.md](db-connection-security.md), section 5); without any one of them every backup fails ([specs/db-connection-security.md](specs/db-connection-security.md), section 9.5). **Answered (2026-11-07):** keep the three server-wide rights; the backup is unchanged.
- (DB-SEC 2) The Backup window's archive tab can back up to another server first ("back up first"), with an account of that server typed in the window. That path copies the rows with a bulk load (`LOAD DATA LOCAL INFILE`), which MySqlConnector refuses without `AllowLoadLocalInfile=true` in the connection string, so it fails at the first table with rows. Should the program set `AllowLoadLocalInfile=true` for that connection only (MySqlConnector keeps it off by default because a server may then ask the client for local files), copy the rows with `INSERT` statements instead, or drop the option of a backup to another server? **Answered (2026-11-07), the recommendation: `INSERT` statements.** Built: the backup to another server copies each table's rows with ordinary `INSERT` statements, in the same batches, with the same progress lines and row checks; no connection allows local infile.
- (DB-SEC 3, decided by the lead as (c) on 2026-11-11, from the MySqlConnector 2.6.2 upgrade, [port-notes/S2-2.md](port-notes/S2-2.md) section 3.6) Your decision Q5 says the database server's certificate must chain to the office CA before anything is trusted. MySqlConnector 2.6.2 (the security update of the .NET 10 move) no longer does that by itself for logins with a database password: it sends the login first and checks the certificate after, and it accepts any certificate from a server that proves it has a copy of the account's stored password hash. Someone in the middle of the network could then collect the password's challenge response and try to guess the password offline, or, with a copy of that hash (for example from a backup of the database server's files), pass as the server and see all patient data. The choices were: (a) accept the risk and write it into the connection rule; (b) allow only client-certificate logins over the network; (c) the program checks the server's certificate itself before it logs in. **The lead decided (c), because it keeps your decision Q5 exactly as you made it** and keeps passwords working (the password fallback of Q2, the backup to another server, the read-only and reporting servers): every connection over the network that names a CA is now checked by the program during the encrypted handshake, before any part of the login is sent. The certificate must chain to exactly the office CA (no other root, not one of Windows' own), be within its dates, be a server certificate, not be revoked when a revocation list says so, and, for the main connection, name the server the computers connect to. The stored-hash shortcut can no longer be reached, so the same change also closes it for MariaDB 11.4 servers, which send that proof. Built in S2-2's follow-up round and pinned by tests (`tests/DbConnectionSecurityTests/UnverifiedServerTests.cs`, `OfficeCaServerVerifierTests.cs`). **You can still choose otherwise** ((a) or (b)); nothing in the office set-up changes with (c). **Answered (2026-11-12):** a server
certificate with no revocation information (the guide's certificates publish none) is still accepted; one that a revocation list says
is revoked is refused.

---

## Library updates and the DICOM reader (answered on 2026-11-02)

The questions of the library updates and of the in-house DICOM reader, with your answers of 2026-11-02. They are all decided; the program is as each answer says. SSH-KEYS 1 to 7 and DB-SEC 1 and 2 above, from the same work, were answered on 2026-11-07.

**Tier 1 libraries (LIB-T1)**
- (LIB-T1 1) Remember the host key of every SSH server? **Decided:** yes. Built: "Remembered SSH host keys" above and [specs/ssh-host-keys.md](specs/ssh-host-keys.md).
- (LIB-T1 2) SSH.NET does not offer DSA host keys (`ssh-dss`), so a server that has only a DSA key cannot be reached. **Decided:** accepted.
- (LIB-T1 3) MailKit, which replaced OpenPop.NET for POP3, checks the server's certificate and its revocation. **Decided:** the revocation check stays.
- (LIB-T1 4) Nothing in this program checks POP3 inboxes in the background. Add a POP3 poller? **Decided:** no POP3 polling.
- (LIB-T1 5) The release channel copies a release over the installation folder and never deletes a file a release dropped (the retired files are never published again and setup deletes them on an upgrade, `ForkReleases.ListRetiredFiles`). **Decided:** it stays that way.
- (LIB-T1 6) MSAL 4.92 reads the Microsoft 365 token caches MSAL 4.47 wrote, but writes its own format from then on. **Decided:** the format change is accepted.

**The DICOM reader** ([imaging/dicom-reader.md](imaging/dicom-reader.md))
- Compressed DICOM that the reader does not decode (JPEG Extended, JPEG Lossless, JPEG-LS, JPEG 2000 and the other formats it names) shows a message naming the format. **Decided:** keep the message; decoders are deferred ([roadmap.md](roadmap.md), Later).
- Signed 8-bit and 16-bit RGB images, which version 0.0 showed scrambled, now show correctly. **Decided:** keep the corrected display.
- Progressive JPEG under JPEG Baseline opens, with at most 32 scans. **Decided:** keep both.
- A JPEG Baseline image whose JPEG data is another size than Rows and Columns is declined. **Decided:** keep declining it.
- JPEG data without an end-of-image marker is declined. **Decided:** keep declining it.

**WebView2 (LIB-WEB, [port-notes/LIB-WEB.md](port-notes/LIB-WEB.md), "Owner questions")**
- (LIB-WEB 1) Keep the installer's optional Visual C++ download? **Decided:** kept.
- (LIB-WEB 2) Pages written to temporary files while shown? **Decided:** no; pages (and the CCD) are served from memory. Built.
- (LIB-WEB 3) Remove the old WebView2 profile folder (`%LOCALAPPDATA%\WindowsFormsWebView2`) on upgrade? **Decided:** no, it is left alone.
- (LIB-WEB 4) A received email loads web images only after Show Images. **Decided:** kept.
- (LIB-WEB 5) The browser keys (find, Ctrl+P, zoom). **Decided:** all kept.
- (LIB-WEB 6) A CCD with a stylesheet shows as XML. **Decided:** the XML view is enough.
- (LIB-WEB 7) Two error dialogs when the WebView2 Runtime is missing or fails. **Decided:** one dialog. Built.

**PDFsharp and the spell checker**: route B (decision 13 above, and the LIB-PDF items under "Release v0.0") and the spell checker built from its GitHub source as a git submodule, with no MPL text shipped (item 18 and "The source of the spell checker").

---

## Open Dental 25.3 options built (ODOPTS, 2026-11-07)

Your answer of 2026-11-07 ("why not just build it?") to the options this program had built without ([port-notes/SWMAP.md](port-notes/SWMAP.md)).
All eight are built and read under Open Dental's own setting names, so the office's values carry over with no conversion
([port-notes/ODOPTS.md](port-notes/ODOPTS.md)). Open Dental does not publish what most of them mean, so this program does what each name says;
please confirm each by trying the setting in the office's Open Dental 25.3 (on a copy, or on a test patient) before the switch-over, and say
where Open Dental behaves differently.

**Decided as built (owner, 2026-11-07, "all as recommended"):** every ODOPTS question below is settled as built. ODOPTS 6 and 7
(Message-to-Pay): the safeguard stays as built, a stored "always" acts as "ask" until Statement Send Setup is saved in this program.

- (ODOPTS 1) `PerioRedAttGing`: an auto Att Ging value **at or below** the number is red, like MGJ. A blank or missing row means no threshold
  (never red). **Confirm:** the direction, and what Open Dental shows on a new database (its default). **Today:** as described.
- (ODOPTS 2) `DomainLoginForSigBoxes`: on means the Change User of signature boxes (procedure notes, group notes, sheets, treatment plan
  signing, the ortho chart) offers domain login while domain login is on; off, those prompts ask for the program's own password. A missing row
  reads as on. **Confirm:** that this is what the switch does in Open Dental, and its default. **Today:** as described.
- (ODOPTS 3) `ClaimUpdateFeeForSecondary`: on means that when a procedure's fee changes while the procedure is on a primary claim, each of its
  secondary claims that is not sent or received is recalculated with the new fee (the primary claim is not touched). This program does it
  wherever an existing procedure's fee changes: the procedure window, Edit All (a new provider re-prices the selected procedures), and a provider
  change on an appointment that is not complete (moving it, or saving the appointment window), which can re-price a completed procedure on
  it. Missing row: off. **Confirm:** when Open Dental updates the secondary claim's fee (from the procedure window, from Edit All, from an
  appointment's provider change, also when the primary claim is received, or from the claim), and whether it updates estimates too.
  **Today:** as described.
- (ODOPTS 4) `EFormImportAllergiesAsOther` and `EFormImportProblemsAsOther`: on means a typed allergy or problem that matches nothing in the
  list is imported as "Other" with the patient's text (as before); off means it is not imported: auto import records each such entry in the
  form's Audit Trail entry ("Allergy not imported ...", "Problem not imported ...") and tells the user who saved the form, and the manual import
  lists it unticked. Missing rows: on. **Confirm:** what Open Dental does with such an entry when the setting is off (skip it, or add a new list
  entry with the patient's text). **Settle before the switch-over** if the office's rows are 0: from that day a typed allergy or problem that
  matches no list entry is not auto-imported (it stays on the form and in the Audit Trail). **Today:** skipped and recorded.
- (ODOPTS 5) `InsHistPerioScalingCode`: a procedure code that the perio scaling rows of Insurance History use to record dates (and count as
  history); blank keeps each perio scaling code group's own History Proc Code. **Confirm:** that this is the code Open Dental uses when a perio
  scaling date is entered, and whether it applies to the four quadrants alike. **Today:** as described.
- (ODOPTS 6) `StatementAutoSendEmail`: **the meaning itself was changed, not only the values.** The only public source (the 25.1 release
  notes, `v25_1-eservices-msgtopay-on-statement-creation` in `docs/specs/features-25.1.md`) says Open Dental's Statement Send Setup window sends
  **Message-to-Pay** links automatically when statements are created, a hosted Open Dental service this program does not have. So in Open
  Dental this row most likely turns on automatic Message-to-Pay messages; it probably never e-mailed the statement PDF itself. This program
  uses the row for something else (**INFERRED**): the new statements whose mode is E-mail are e-mailed as soon as billing creates them
  (Billing Options, Create List), without the Send click. **Decide first, before the switch-over:** (a) does the office use this setting in
  Open Dental for Message-to-Pay, and (b) do you want it to mean "e-mail the statement when billing creates it" in this program? Only then:
  (c) which value the office's Open Dental shows for which choice in its Statement Send Setup window (read here as 0 never, 1 ask, 2 always;
  any other value never sends). **Until you decide:** a stored "always" asks before e-mailing, and says why, until someone saves Statement
  Send Setup in this program (Billing Defaults, Send Setup), so nothing is e-mailed on the strength of Open Dental's Message-to-Pay setting
  alone. **Today:** as described.
- (ODOPTS 7) `StatementAutoSendSMS`: in Open Dental most likely Message-to-Pay by text (the same public source). The same values, but no text
  is sent: Open Dental's hosted texting was removed and this program has no texting provider. The Billing List says the statements were not
  texted, and the setup window says texting needs a provider. **Decide:** whether to choose a texting provider (the hook is
  `StatementAutoSend.TextSender`). **Today:** nothing is texted.

---

## Open Dental 26.2 port (P262, answered on 2026-11-07)

The questions of the 26.2 packages (port notes `P262-SCHEMA`, `P262-APPT`, `P262-BILLING`, `P262-CLAIMS`, `P262-PERIO`, `P262-TOOLS`,
`P262-REPORTS`, `P262-MISC`, `P262-LINKS` and `P262-GLIDEWELL`), with SWMAP's and ODOPTS's. Your answer of 2026-11-07: "all as
recommended". Built on the integration branch `next/int262-1`.

- (P262-GLIDEWELL 1, 2 and 8) **Glidewell lab cases.** The office itself asks Glidewell for API access (its account number and API key, and a
  test account); revisit if Glidewell deals only with software vendors. Sending patient data to Glidewell stays **blocked until a business
  associate (data) agreement with Glidewell is recorded** in the Glidewell setup, the same rule as the postcard service; status checks wait
  for it too. The Glidewell lab cases program link is **hidden in Program Links until a transport exists** (this version has none). See
  [port-notes/P262-GLIDEWELL.md](port-notes/P262-GLIDEWELL.md), "Owner decisions".
- (P262-LINKS 2 and 3) **Allied Star Connect and OrthoComposite.** Their file layouts and arguments are not public. If the office uses either
  program, the office asks that vendor; otherwise both links stay switched off. No code change. **Update (vendor follow-up, 2026-11-07):**
  OrthoComposite's arguments were then found public in its Microsoft Store listing and ship as the link's default; only Allied Star Connect's
  file layout is still unpublished.
- (P262-REPORTS 5) **Patient Portion Uncollected.** The adjustment subquery inherited from 26.1 counted an adjustment once per payment split
  attached to it. **Fixed:** each adjustment counts once, less its splits; the totals of such rows change from 26.1. Tested.
- (P262-CLAIMS 2) **Default claim form per provider.** A new claim takes the **billing** provider's default claim form first, then the
  treating provider's, then the existing default (the plan's). Tested.
- (ODOPTS 6 and 7) **Message-to-Pay** (`StatementAutoSendEmail`, `StatementAutoSendSMS`): the safeguard stays as built; "always" acts as
  "ask" until Statement Send Setup is saved in this program.
- (P262-SCHEMA 2) **Paint type TextBelow** (`ToothPaintingType` 18): hidden from the procedure code paint type lists until the tooth chart
  work draws it; a procedure code that already has it keeps it. Tested.
- **Every other 26.2, SWMAP and ODOPTS question: decided as built** (owner, 2026-11-07). Each port note says so with a dated line.

---

## The move to .NET 10 (answered on 2026-11-08)

The questions of the .NET 10 stage plan, [specs/net10-plan.md](specs/net10-plan.md), made from a read-only survey of every project and an
independent critique. You answered on 2026-11-08 in several messages: "As recommended"; then the facts (the office runs Windows 11, uses
no plugins and does not use Direct secure messaging) and one change (Change Healthcare (Emdeon) medical claims are kept and made to work:
"no, but make it work"); "Keep signed email"; "Have them rebuilt"; "don't cut" (the DentalXChange eligibility path); the Middle Tier
answer; the standing rule below, with its correction; "Don't need 'Install for me only'"; "no go directly to 64."; and "c self-contained
for 64bit as well".

- **Standing rule (2026-11-08).** "Overall decisions wrt .net 10: don't cut anything including test. Cut test only if it's testing
  something that we deliberately deleted." You confirmed the reading ("Yes"): what you deliberately removed stays removed, and everything
  else is kept and moved to .NET 10. Your correction the same day: "Don't make the assumption that if the office doesn't use, we'd cut it.
  1-8 we want to keep it and have them ported. DentX, Dexis Integrator and Vipersoft links, and NDde.dll we should keep if we can." So
  these are kept and ported, each port still to be planned: QuickBooks Desktop, Schick CDR, Topaz signature pads, the CallFire SMS link,
  the Divvy postcards link, the BCBS Georgia dial-up terminal, the report engine's SQL Server, ODBC and OLE DB data sources, and Direct
  secure messaging. The DentX, Dexis Integrator and Vipersoft links and NDde come back if that can be done. What stays removed: the old
  2020 Imaging module (after its window is rebuilt), the earlier removals tied to Open Dental's own services and eServices and to closed
  parts with no published interface (Sparks3D, Suni, the "not available" bridges), and dead code. Tests go only with these.
- **Build tools.** Visual Studio Build Tools 2026 are installed beside Build Tools 2022, with the MSVC v143 C++ toolset added
  (2026-11-08), so the C++ project and the setup's Visual C++ check stay as they are. The build lane and the test runners move to them
  first in the SDK-style project stage, before any project changes format. That step also pins the .NET 10 SDK in `global.json`,
  because the SDK 9.0.3xx line leaves support on 2026-11-10.
- **Downloads.** Approved for every download batch of the plan (section 5: A, B, C and D, with the packages the standing rule adds)
  ("You get approval for download for all batches", 2026-11-08). Each package comes from nuget.org by its exact id, into the local
  package folder, at the stage that needs it. The 64-bit TWAIN data source manager is not a plan batch; it is asked when the switch
  to .NET 10 starts.
  Also approved, standing ("Approve monthly", 2026-11-08): each month's .NET 10 SDK patch and runtime packages for the monthly
  runtime release.
- **Testing database encryption.** Locally, with encryption ("database encryption: do local testing with encryption", then "A"): the
  test starts a temporary second MariaDB server on `127.0.0.1:3307` with test certificates it makes itself, and removes it afterwards.
  Your database server on port 3306 is not touched, and no administrator rights are needed.
- **Encrypted local test database.** Yes: the local MariaDB on port 3306 is switched to encryption (one script you run as
  administrator), everything on this PC then connects encrypted, the server refuses unencrypted connections, and the old test
  database is rebuilt. This replaces the temporary second server above.
- **Scheduled Processes and HIE export.** A runner is built ("Yes build it in a way that these workflow only run on a single
  workstation in the practice, so no race conditions"): only the workstation named in setup runs these jobs, and a database lock stops
  a second one from running a job twice. It can be turned off ("Also have option for background test to be turned off"): one setting
  for the whole practice and one per job. It starts off, also after an upgrade, so jobs an office set up under Open Dental do not start
  running by surprise.
- **Direct certificates.** The Register certificate button, which registered with Open Dental's servers, is replaced by a local import
  ("Yes, load Direct provider's certificate from a file"): the certificate your Direct provider issued is loaded from a file, with its
  password, into the Direct certificate store.
- **QuickBooks Desktop, Schick CDR and Topaz signature pads.** The office does not use them. They were to be removed in the clean-up stage
  (owner, 2026-11-07); now kept and ported (owner correction, 2026-11-08).
- **Order of the stages.** A clean-up stage first; then SDK-style project files, NuGet packages and the test framework, all still on .NET
  Framework 4.8; then the code changes that also work on .NET Framework, ending with a last .NET Framework release (the "bridge release");
  then the switch to .NET 10, straight to 64-bit.
- **64-bit.** The move goes straight to 64-bit ("no go directly to 64."): no 32-bit .NET 10 version; the office installs the .NET
  Framework bridge release, then the 64-bit .NET 10 setup once on every workstation.
- **Test framework.** MSTest 4 instead of MSTest 3, because every MSTest 3 version is marked deprecated; MSTest 4 still runs on .NET
  Framework.
- **Text sorting and search.** The first .NET 10 release keeps Windows' own rules (NLS), as today. The move to .NET's default (ICU) comes
  later as a release of its own.
- **Switching the office.** The bridge release first, then the new setup once on every workstation. The bridge release also stops a
  workstation that is not switched yet from publishing its old build over the new one.
- **The .NET 10 runtime.** The program and the 32-bit helper ship with their own .NET 10 runtime ("c self-contained for 64bit as
  well"), so nothing is installed for .NET on the workstations. The runtime's security fixes then no longer come from Microsoft
  Update: they come with this program's releases, one after each monthly .NET 10 patch. Updates stay small, because a release keeps
  the runtime as a separate part, published and pulled only when the runtime changes. The first install and each runtime change carry
  the whole runtime, and the program folder grows to about 400 MB. The runtime packages are part of the approved download batch D.
- **The 32-bit helper's runtime** (open point O-15). Answered: (c) self-contained ("c self-contained for 64bit as well"). The helper
  for QuickBooks Desktop, Schick CDR, Topaz and Guru moves to 32-bit .NET 10 with its own runtime, in its own folder, so no 32-bit
  runtime is installed. Each vendor part it loads must be shown to load on .NET 10; if one does not, you are told, and it does not
  quietly fall back to .NET Framework.
- **"Install for me only".** The setup drops this mode. Confirmed after the standing rule ("Don't need 'Install for me only'"). Its
  first reason, that such a setup cannot install the runtime, is gone, since the setup now installs none.
- **Windows versions.** Windows 11 is the baseline, and the office's workstations run it; the setup warns below it. Windows 10 is
  supported by .NET 10 only in some editions.
- **Text size.** With Windows "Make text bigger" above 100%, 72 older windows show slightly larger text. Accepted.
- **Dashboard graphs.** The community port of Microsoft's chart control (`WinForms.DataVisualization`, MIT licence).
- **The CallFire SMS link, the Divvy postcards link, the BCBS Georgia dial-up terminal, and the report engine's SQL Server, ODBC and OLE DB
  data sources.** First cut instead of ported; now kept and ported (owner correction, 2026-11-08).
- **Kept.** Voice perio charting. Change Healthcare (Emdeon) medical claims: the office does not use them, but they are kept and made to
  work on .NET 10. The plugin loader stays, although the office uses no plugins; plugins must be rebuilt for .NET 10, and .NET Framework
  plugins stop working.
- **DentalXChange eligibility, Practice-Web path.** Kept ("don't cut") and made to work on .NET 10 with the other web-service calls.
- **Middle Tier.** The office runs no Middle Tier server, but the Middle Tier is still moved to .NET 10
  ("no, but port middle tier to .net 10").
- **Reports with custom VB code.** They keep working: on .NET 10 the program compiles their code with Microsoft's current compiler
  (Roslyn). Changed by the standing rule; before, they were to show a clear error instead of running the code. The reports that ship
  have none.
- **Direct secure messaging (HISP).** The office does not use it. It was to be removed in the clean-up stage; now kept and ported (owner
  correction, 2026-11-08). It comes back on top of the new e-mail code: received e-mail stays read by MimeKit, which the program already
  ships, instead of the old Direct libraries.
- **Signed e-mail.** Kept (owner, 2026-11-08: "Keep signed email"): sending signed e-mail moves to MimeKit with the rest of e-mail in the
  clean-up stage.
- **Projects that are not shipped.** The legacy UnitTests project, the report designer (RdlDesign), DocumentationBuilder and Central
  Manager are kept and moved to .NET 10. Changed by the standing rule; before, the first three were to be deleted and Central Manager
  retired. The legacy UnitTests project becomes a running test suite, and its developer windows stay.
- **The XmlSerializers files** (`OpenPmsDental.XmlSerializers.dll`, `OpenPmsDentalBusiness.XmlSerializers.dll`). Kept (standing rule):
  they ship again, and on .NET 10 they are built with Microsoft's serializer generator for .NET.
- **The old 2020 Imaging module.** Retired completely. Its code also draws the window behind Scan EOB / View EOB on insurance payments and
  preauthorizations and Scan on EHR amendments; that window is rebuilt on the new Imaging module first (owner, 2026-11-08: "Have them
  rebuilt"), then the old module, its "Use old Imaging module interface, pre 2020" option and its code are removed.

---

## The WPF tooth chart (answered on 2026-11-08)

The questions of the chart plan, [specs/wpf-tooth-chart-plan.md](specs/wpf-tooth-chart-plan.md), section 10 (O-17 to O-31). Your answer
of 2026-11-08: "1. As recommended 2. Ship dlls we need. Use Helix. It's decided. Rest as recommended."

- **Helix Toolkit.** The download of its 20 packages (batch H) is approved. Helix is used; the setup ships only the DLLs the program
  needs, each with its notice, and nothing waits for counsel.
- **Everything else as recommended:** the on-screen chart is the software-rendered image if it is fast enough (else a live view with a
  software switch); the extraction graphic is an office-wide setting, off by default, drawing a big X; text below the chart goes inside
  the image along its edges; the Text paint type draws its text; retained root and space maintainer are drawn; ortho mode keeps today's
  layout; each place keeps its colours; SharpDX and Tao go after the switch package passes an office check; each package merges hidden
  after you approve its gallery; today's DirectX pictures are recorded on the build PC, which has the DirectX runtime; the business
  layer's chart placeholder stays; computers set to Simple 2D stay 2D; graphical perio opens on every computer.
- **The recording of today's chart (O-32 and O-33, asked with TC-0's results).** Keep all of it in the repository (715 pictures,
  about 81 MB), since the old engines cannot record again once they are gone. DirectX stays recorded without edge smoothing for the
  tests, and about 10 smoothed pictures go in the gallery to show the look offices see.
- **Gallery G1 and O-34 (answered on 2026-11-09: "G1 go with recommended").** The 145 re-recorded pictures are approved (the white
  centre line and the highlight boxes on the pixels DirectX fills). O-34: the program keeps drawing every WPF window in software, and
  the quick live view is the default on-screen chart, with a per-computer "Draw in software" switch for the smoothed picture (b1).
  The dashboard's copy of the chart is made in the background by a later package, TC-3b, before the WPF chart becomes every
  computer's chart.
- **The extraction graphic and the tooth chart rule (2026-11-14).** After G2's first versions: "Keep it the same as open dental, which is
  a setting to toggle showing it or not." and "Overall for tc changes, keep it the same as OD latest version." Every tooth chart change
  now follows the latest Open Dental's public behaviour (26.2 stable, 26.3 beta); the audit is in `docs/specs/wpf-tooth-chart-plan.md`,
  section "Open Dental parity (2026-11-14)". So O-20 (on by default; the tooth hidden with a thin X, as Open Dental's 26.2 screenshot
  shows; no other exceptions), O-21 (TextBelow in a one-line box under the date slider, not in the tooth picture) and O-22 (paint text
  beyond the root tip, in the W band) changed.
- **Half-cent rounding on .NET 10 (your decision, 2026-11-16: "Keep Open Dental's 1.01").** .NET 10 formats numbers exactly (1.005 with
  two decimals gives 1.00) where .NET Framework rounded to 15 digits first (1.01). The program keeps Open Dental's text in reports and
  wherever it formats money (net10-plan decision 24; package S3b-FMT after S3b-4).
- **S3a-M3b before EHR-7 (your decision, 2026-11-15: "yes, go ahead").** FormMedical's last two WinForms menus are converted now, on
  `main`, so stage 3a's gate and stage 3b no longer wait for the E-20 download; EHR-7 (`ehr/ehr7-3`, about 28 lines of FormMedical) is
  redone on top when E-20 arrives.
- **S3a-11, the elevated installer test (run on 2026-11-15 by you):** 97 checks passed, 0 failed, exit code 0 on the build PC; your 0.0
  installation's Apps entry and Event Log source were written back and its shortcuts are unchanged. Nothing more is asked here; you run
  the same line again at each stage gate.
- **Gallery G5: the chart on .NET 10, 64-bit (S3b-5W). Answered on 2026-11-16: "approve G5"; the pictures are approved as drawn, so the 64-bit one-pixel text rounding (the "5" at 410x307 and the perio legend's second row) stays as the 64-bit program draws it (the default of question 2). S3b-5W is merged into `net10/switch`.** Branch `net10/s3b5w-2` (the
  review fix round of `net10/s3b5w-1`: the tests' DirectX comparisons now check the .NET 10 pictures; no picture changed); the
  pictures were recorded in `7c95e3a54` and the gallery was made at that commit. The page `index.html` (in the main checkout's
  `.git/od-tools/test-results/g5-gallery/`, also zipped as `g5-gallery.zip`) shows each of the 370 pictures the chart tests keep (326 of the
  WPF chart, 35 of the Simple 2D chart, 9 that the program's own windows make: sheets, Treatment Plan, referral letter, dashboard, big
  chart, sheet editor preview, perio chart) as .NET Framework 4.8 (64-bit) draws it, as .NET 10 (64-bit) draws it, and the difference,
  the changed ones first: 193 identical, 173 within the tests' tolerance, 4 beyond it. What you would see: the chart looks the same; the
  four beyond the tolerance are host pictures that only had a 32-bit picture to compare with, and each is a 64-bit difference already seen
  on .NET Framework (TC-10): the perio legend's second row one pixel higher, the tooth number "5" one pixel to the right at the dashboard's
  size, and the 2D sheet's text and teeth anti-aliased a little differently; the saved perio image blends its left and top edge pixel a
  little differently. One drawing change: the "W" watch marks are drawn in tooth-number order, because .NET 10 shuffled their order from run
  to run ([port-notes/S3b-5W.md](port-notes/S3b-5W.md)). Questions: (1) approve these pictures as the chart's look on .NET 10, or name
  scenes to change; (2) the one-pixel moves of the "5" and of the perio legend row: keep them as the 64-bit program draws them (the
  default), or make the rounding give the 32-bit position again (a small drawing change with its own gallery). S3b-5W merges into
  `net10/switch` only after your answer.
- **Gallery G4, TC-8's part (answered on 2026-11-15: "Approve TC-8").** Every host on the WPF chart, File > Graphics with "3D chart",
  "Simple 2D chart", "Draw in software" and a live preview, graphical perio on every computer, Simple 2D computers kept on 2D. TC-8 merged.
  **File > Graphics on Open Dental 26.2's layout: your decision (2026-11-15): "Match Open Dental 26.2".** Package TC-8b builds the
  parity rows ENG-1, ENG-2, ENG-3, ENG-5 and NEW-1 of the chart plan after TC-9a: an office-wide "3D tooth chart on all computers"
  setting, per computer "Use the global setting / Use if available / Do not use", a status line, Simple 2D in an "older technology"
  group; Open Dental's stored `DirectX11ToothChartUseIfAvail` and `computerpref.GraphicsUseDirectX11` reused (no schema change; this
  answers BIN-A 5: keep them); the smoothed picture as the on-screen default with the live view kept as a per-computer option; graphical
  perio refused on a 2D computer with a message naming File > Graphics. This revises your earlier answers to O-30, O-31 and O-34 (b1),
  kept below as history. ENG-6 (whether Setup > Computers' "Use Simple Graphics" also sets "Do not use") is built as the closest
  documented reading and listed as unconfirmed. TC-8b's gallery comes before it merges.
- **Gallery TC-8b: File > Graphics as Open Dental 26.2 has it (asked 2026-11-15; built on `chart/tc8b-1`, fixed on `chart/tc8b-2`).
  Answered on 2026-11-15: "1. approve tc-8b. 2. yes. 3. what you have is good 4. yes".** TC-8b merged; "Use Simple Graphics" also sets
  "Do not use" (ENG-6, kept); no red "This window is obsolete!" line (the neutral explanation kept); a computer set to "Do not use" shows
  "Available" when the 3D chart can start (NEW-1, kept). The question as asked: The page `tc8b.html` (in the
  main checkout's `.git/od-tools/test-results/tc8b-gallery/`, also zipped) shows Open Dental's own Graphics window next to this program's,
  before (TC-8's two choices) and after in seven states: the default, the global "Do not use", this computer's "Use if available" and "Do not
  use", the quick live view, a computer whose 3D chart fell back ("Fell back: ..."), and a medical practice. What you would see: the same
  groups and words as Open Dental with "3D Tooth Chart" in DirectX 11's place, Simple 2D as the only older technology, "Draw in software"
  ticked by default (the smooth picture; unticked gives the faster, slightly jagged live view), and the preview kept. Graphical perio on a 2D
  computer now says "In the Graphics setup window (File > Graphics), you must first select the 3D tooth chart." Unconfirmed against Open
  Dental 26.2 and built as the closest documented reading: "Use Simple Graphics" in Setup > Computers also sets that computer to "Do not
  use" (ENG-6), the message's exact 26.2 wording, the status of a computer set to "Do not use" (shown as "Available" when the 3D chart can
  start), and the medical wording; and the top of the window explains its two halves instead of Open Dental's red "This window is
  obsolete!" (say if you want the red line).
- **Gallery G2 part 2, TC-6 (answered on 2026-11-15: "TC-6 approved").** Ortho mode with the facial rows only, brackets with a slot,
  thin wires and elastics, root-only retained roots and flat space-maintainer bars, as Open Dental shows them, are approved. TC-6
  merged. Its build folder is named G5; the page called it G2 part 2, and the later 3b gallery keeps the name G5.
- **Gallery G2 (answered on 2026-11-15: "G2 is good. Merge tc5").** TC-5's drawing, fourth version, is approved: free text, paint text,
  TextBelow's box and the completed-extraction X as in Open Dental 26.2. TC-5 merged.

---

## The EHR rebuild (answered on 2026-11-08)

The questions of the EHR plan, [specs/ehr-rebuild-plan.md](specs/ehr-rebuild-plan.md), section 13 (E-1 to E-19). Your answer of
2026-11-08: "E-1 to E-19 as recommended".

- **Turning EHR on:** one confirmation, the same in Show Features and the Setup Wizard, no password (E-2). The measures window and
  the CQM reports say "internal review only; this program is not certified" (E-1).
- **The time check:** Off (the default) or Warn; the program never sets the clock by itself, Windows Time does, and the manual
  window sets it only on a button, with checks (E-3).
- **Providers and permissions:** a provider reports EHR when a key row with their name exists (the key text is optional and never
  checked); key rows, the imports and the SNOMED tools need the Setup permission; the quarterly-key stub with no caller goes and its
  table stays (E-4, E-5, E-6).
- **Value sets:** each office imports its own download of VSAC's June 2013 EP release (Excel workbook or SVS XML); no automatic
  fetch for now (E-7). **One step for the office:** someone with a UMLS licence opens that release once and records its files and
  headers, so the importer reads the real layout; nothing from it enters the repository.
- **CCD and CCR display:** the free HL7 and AAFP stylesheets ship with their notices (the CCR one as a copy marked as modified),
  run only in a short-lived child process of the program's own exe, and a stylesheet that comes with an e-mail still never runs;
  file exports also write a rendered HTML (E-11 to E-14). Download batch E is approved.
- **Identity:** QRDA files carry this program's name and the office's identity instead of Open Dental's; HL7 follows later (E-15).
- **Left out:** current measure versions, Stage 3 / MIPS and the newer C-CDA guide, which only count with certification (E-16);
  other EHR gaps, the radiology-orders alert and the unused measure columns become follow-ups or stay as they are (E-17 to E-19).
- **Counsel (E-6, E-8 to E-11):** the work goes ahead and nothing licensed is shipped; the CQM reports (EHR-5) no longer wait for
  counsel: the owner decided on 2026-11-14 that the NCQA measures may be built (E-10b) and E-10 is withdrawn (2027 plan, E-40); the other counsel answers are applied when they come (the code lists already in the engine stay until
  counsel answers E-8 and E-9).

---

## Direct certificates follow-up (DIRECT-CERT, answered on 2026-11-09)

The questions the DIRECT-CERT package (open point O-13, [port-notes/DIRECT-CERT.md](port-notes/DIRECT-CERT.md)) left open. You
approved all three parts of the follow-up package, DIRECT-CERT-FOLLOWUP
([port-notes/DIRECT-CERT-FOLLOWUP.md](port-notes/DIRECT-CERT-FOLLOWUP.md)).

- **The connection settings tool's private keys.** The tool now deletes the private key of a client certificate it replaces also when
  Windows keeps that key the older way (a CAPI key container, as it stores a `.pfx` that names no key provider), and gives the computer's
  users read access to such a key. Before, the certificate was removed but that kind of key stayed on the computer. The same fix as the
  Direct certificate import's. Done. Also, as the Direct certificate import does: the tool deletes the keys a `.pfx` brings that it does
  not use (a CA certificate's in the same file), and a `.pfx` whose certificate the computer already has with a working key (setup run
  again with the same file) leaves that certificate and its key as they are. Before, each such re-run left one more copy of the private
  key on the computer, readable by its users, that no tool ever removed, against your decision of 2026-11-02. Done.
- **Open Dental's "Customer Updates" web service.** Its empty stand-in (`ICustomerUpdates`, `CustomerUpdatesNoHq` and
  `CustomerUpdatesProxy`) is deleted, as you decided; the tests that checked it was still there now check that it is gone. Done.
- **The computer's users keep read access to the Direct private key**, as built: the program runs as the signed-in Windows user, who signs
  and decrypts with it.
- **A replaced Direct certificate's private key is deleted only if the certificate window imported it**, as built; the key of a
  certificate put there another way stays on the computer.

---

## Pending items answered on 2026-11-09 ("The rest as recommended", "Do headless window test")

- **The database TLS script** (`tools/Enable-LocalDbTls.ps1`, DB-TLS) runs with `-WithClientCertificate`. You run it elevated, when
  the lead says the database is idle.
- **The Insurance Verification list** loses a row when a plan's number equals a patient plan's number; it is fixed in its own
  package (INSVERIFY).
- **The database client certificate store** gets the same key-deletion fix as the Direct certificates, in the Direct certificate
  follow-up package, which also records the two Direct answers that keep the behaviour as built and removes the empty customer-updates
  shell.
- **The stage 1 window checks** are done by a headless test (WINCHK-HEADLESS) instead of a session driving the desktop, because
  the PC's screen stays locked. Checks only a person at the screen can do are listed in its report for a later on-screen pass.

## The connection settings tool's client certificate (DIRECT-CERT-FOLLOWUP, last round, answered on 2026-11-13)

The last round of [port-notes/DIRECT-CERT-FOLLOWUP.md](port-notes/DIRECT-CERT-FOLLOWUP.md) made the tool pick the client certificate of a
`.pfx` by what it is, read key locations without opening keys, check that a deleted key is gone, record every key it creates, undo an import
whose save fails, and never give the users read access to a key it did not create. You answered all three as recommended ("all three as
recommended", 2026-11-13); one small package (DIRECT-CERT-FOLLOWUP round 4) builds them. All three are done (branch `fix/dcfu4-1`, the
port note's "Round 4").

- **The Direct certificate window's import** (`DirectCertificateImport`) still opens the private keys of the store's other certificates to
  compare key containers, and counts a store entry as usable without checking which key set its key is in. Its store can be the computer's
  (`LocalMachine\My`), where a smart-card or TPM key could ask for a PIN or fail. Should it get the same two changes as the connection
  settings tool? **Recommended: yes**, as one small package. **Answered: yes. Done:** the import compares keys by each certificate's key
  provider information, read by the same code as the tool's (now in CodeBase), and never opens another certificate's key to do so; a store
  entry counts as already there only when its key is in the import's key set, and only then is its key opened.
- **Undoing an import removes the whole store entry.** When the computer already had the client certificate (put there by an administrator,
  say) and the import gave that entry the tool's key, a failed save, or a later save that stops using the certificate, removes the entry with
  the tool's key; the entry's earlier key stays on the computer and the message says where. The choices: (a) as built, remove the entry and
  say so; (b) put the entry's earlier key back on it, so the store is as it was (the tool then writes to a certificate it did not import);
  (c) leave the entry and delete only the tool's key (the entry is then left pointing at no key). **Recommended: (b)**, since it leaves the
  computer exactly as the administrator had it. **Answered: (b). Done:** the entry's earlier key provider information is kept in the tool's
  record; the undo of a failed save, and a later save that stops using the certificate, write it back onto the entry, check it, and only
  then delete the tool's key. The entry stays, the earlier key is never touched, and the message says what was put back, and exactly what
  became of the tool's key (deleted, or left and why). This covers every earlier key the tool did not create: one in another key set, a
  working one, and one in the tool's key set that does not open (a smart card that is not inserted, a TPM or other key that needs a PIN, a
  key the administrator's rights keep out, a deleted key file); the import no longer offers the command that deletes an earlier key it
  records (the result asks that it be kept, since the tool names it on the entry again), and says what to do if it was deleted anyway. Only an entry that had no key at all (no key provider information), or whose only key was the tool's own from an earlier
  import, is removed with the tool's key, as before.
- **The tool's record holds only the certificate's thumbprint.** If an administrator later gives a recorded certificate another key by hand,
  the tool treats that key as its own: it gives the users read access to it on a re-import and deletes it on a later save. Should the record
  also hold the key's container and provider? That changes the format of `imported-client-certificate.txt` (a record in the old format would
  still be read). **Recommended: yes.** **Answered: yes. Done:** the record (format 2, versioned by its first line) holds the thumbprint,
  the key the tool created and the entry's earlier key. The tool claims an entry's key only when the record names that very key: a key an
  administrator gave a recorded certificate by hand gets no read access for the users and is never deleted, and the result names it. A
  record in the old format is still read, as before, and the next save rewrites it in the new format, naming the certificate's key only
  when the old rule would have claimed it (a working key of the computer's key set); otherwise it names no key, so a key the tool never
  made is never recorded as the tool's.

## DIRECT-CERT-FOLLOWUP round 4's review questions (decided by the lead on 2026-11-14)

The review of round 4 and its two fix rounds ([port-notes/DIRECT-CERT-FOLLOWUP.md](port-notes/DIRECT-CERT-FOLLOWUP.md), "Round 4, lead
fix round") left four questions. The lead decided them under your rule (the tools never leave a Users-readable key behind and never touch
a key they did not create) and your answer (b) of 2026-11-13; all are built (branch `fix/dcfu4-4`). You can still choose otherwise.

- **A put-back that fails while the certificate still uses the tool's key.** The tool leaves both keys so that the certificate keeps
  working; until now the save then deleted its record, so no record named the tool's Users-readable key. **Decided: keep the record**
  (after a failed save, write one), so that a later save tries the put-back again, and say so. **Done.** One record names one certificate:
  when it must name the new certificate the settings use, the result says that no later save tries again for the old one, and how to put
  it right by hand. The undo of a failed save follows the same rule (fix round 5, branch `fix/dcfu4-5`): when it records the certificate
  the settings use over a record kept for another certificate's put-back, the error names that other certificate and the steps by hand.
  You could choose the other way (keep the older record and leave the settings' certificate unrecorded); the lead's rule keeps the
  certificate the settings use recorded, so a later save can still give its copy its earlier key back.
- **The undo of a failed save whose settings already name the certificate.** The import had given the computer's copy of the certificate
  the tool's key and was recorded at once. **Decided: your answer (b) covers this undo too**: the copy gets its earlier key back, the key
  the save gave it is deleted, and the record is as it was before the save. **Done.** No case was found where this breaks a connection the
  settings as they were depend on: the copy gets back exactly the key those settings used.
- **A copy of the certificate the tool did not record, whose key does not open** (a smart card that is not inserted, a key that needs a
  PIN, a deleted key file). **Answered by round 4's first fix round:** such a key is left aside with a warning, recorded, and put back by
  the undo or a later save, as for every other earlier key.
- **A record of the old format whose certificate an administrator gave another key by hand before the upgrade.** A save with the password
  without a re-import still removes that copy with whatever key it names, as the old rule did. **Decided: as built** (the old rule), and
  kept documented: such records exist only on test installs, because database connection security is new in the next version.

Also built in the same round, as the lead decided: a record that names a copy's earlier key but not the tool's key (only an earlier
build, unreleased, could write one) gets the earlier key back and no key deleted, since the tool cannot tell whether the key it names is
its own, and the result says which key that is and how an administrator deletes it; the import no longer writes such a record. The
Direct certificate window now says so, naming the key, when it imports over a copy whose key is in another key set and leaves that key
untouched. A put-back the tool cannot check (the store cannot be read again) deletes no key and says so.

## The database TLS test environment (DB-TLS-ENV, answered on 2026-11-14)

S2-2's database TLS check (a `REQUIRE X509` login with `SslMode=VerifyFull` over an address of this computer that is not loopback, once with
a CAPI key and once with a CNG key; [port-notes/S2-2.md](port-notes/S2-2.md) section 6) needed three things the TLS script did not make: a
listener on such an address, a server certificate that names it, and two client certificates in the computer's store.

- **Your decision (2026-11-14):** a Windows virtual loopback network adapter (the Microsoft KM-TEST Loopback Adapter, Windows' own driver)
  with one private address from the documentation range 192.0.2.0/24, which no other computer can reach; the local MariaDB also listens on
  that address; nothing is exposed on the office network; `-Revert` removes the adapter again. You run the script elevated; the agents never
  run it elevated, and never install an adapter or change a network setting, the firewall or the MariaDB service on this PC.
- **The exception to "only 127.0.0.1:3306":** the tests may also connect to that one local test address on port 3306
  (`OPMS_DB_TLS_TEST_SERVER`), and to nothing else. Every other connection of the tests and tools stays on `127.0.0.1:3306`. The test
  enforces it: before any connection it fails unless the value is an address of 192.0.2.1 to 192.0.2.254 that this computer has.
- **Built** (DB-TLS-ENV, branch `tools/dbtls-env-1`, [port-notes/DB-TLS.md](port-notes/DB-TLS.md) section 11): `tools/Enable-LocalDbTls.ps1
  -WithTestNetwork 192.0.2.x` adds the adapter (created through Windows' SetupAPI from the driver Windows ships, no download and no
  devcon), its address with no gateway and no DNS, only IPv4 bound, the second `bind-address`, two firewall rules for port 3306 on that
  address only from this computer, the address and the computer name in the server certificate, and the two client certificates
  (`LocalMachine\My`, one key in a CAPI provider, one in the Microsoft Software Key Storage Provider, `BUILTIN\Users` read access as the
  connection settings tool gives). It prints the three values the test needs. `-Revert` undoes every step, and under your rule never leaves
  a key behind that the users can read: both certificates go with their keys (also a key whose certificate was deleted by hand: the
  record in `my.ini` names each key file), and the `.pfx` files are deleted right after their import. Before it makes each item the run
  writes it into a journal (Administrators only), so `-Revert` also removes what a run that was interrupted or failed half-way had made;
  it removes the adapter first and the firewall rules last, so the test address is never without its block rule.
  Your steps, the lead's test run and the undo, with what to check after each, are in that section.

## The EHR quality measures move to the 2027 versions (answered on 2026-11-14)

The questions of the 2027 measure plan, [specs/ehr-2027-measures-plan.md](specs/ehr-2027-measures-plan.md), section 14 (E-20 to E-40,
and E-10b), where each is set out in full with its recommendation. You first answered E-21 ("delete the 2011 engine too") and E-10b
("open-source program is okay"), and then all the others: "all as recommended". The EHR plan's section 13 records the same answers.

- **What is built:** the 2027 versions of the seven measures CMS still publishes for eligible clinicians (CMS68, CMS69, CMS74, CMS75,
  CMS138, CMS155, CMS165), each calculated by a C# translation of its measure logic, with no CQL engine (E-22). Names carry no year, and a
  yearly update after each May release is its own package (E-31). QRDA I is built as well as QRDA III (E-33).
- **What is deleted on purpose:** the 2014 versions (your decision) and, with them, the 2011 engine, its window and its PQRI XML export,
  everything only they use and their tests (E-21: "delete the 2011 engine too"). Saved database rows stay.
- **The NCQA measures** (CMS138, CMS155, CMS165): built in this open-source program in the normal order, and nothing waits for counsel
  (E-10b: "open-source program is okay"). NCQA's copyright and disclaimer statements stay with them wherever they are shown, printed or
  written to QRDA, and the four CMS measures' statements are shown the same way (E-38). E-10 is withdrawn from counsel's list; the
  catalog's names and categories stay in counsel's E-8 (E-40).
- **Value sets and codes:** someone with a UMLS licence records the three files of the 2026-05-14 release once, and nothing from them
  enters the repository; that record comes before the import package merges (E-20). The import tells several LOINC codes of one measure
  apart by their display names, and the vital-sign windows keep their LOINC codes, with LOINC's notice added (E-23, E-37). If the
  release is too large for the 32-bit importer, only the value sets the program needs are kept (E-30). The recommended encounter and
  pregnancy codes come from your own import instead of fixed codes (E-32).
- **How the office's data is read:** completed procedures count as visits, one per day (E-24); diagnosis codes on procedures count, and a
  problem marked Resolved or Inactive without a stop date ends on the day it was last changed (E-26); interventions count by code, and
  dialysis interventions count for the blood-pressure measure (E-28); every medication counts as taken over its dates and as an order on
  its start date (E-39); the BMI finding the vital-signs window stores counts as the over- or underweight diagnosis, the adult BMI range
  is the 2027 one, and the vital-sign windows use the age on the exam date (E-35). Data the program has no place for is shown as
  missing in the window's notes (E-27).
- **Data entry:** a new not-performed item, "BMI follow-up", for the BMI measure's medical-reason exception (E-34); the influenza item
  stays, with a note that no 2027 measure uses it (E-25).
- **The window:** a calendar year by default, other dates allowed with a warning, in the window and in QRDA (E-29).
- **Schedule:** an EHR package takes only a writer slot that no stage or chart package is ready for (E-36).
- **Downloads:** batch Q is approved (E-33): CMS's 2027 QRDA III guide with its schematron and sample, the HL7 QRDA III and QRDA I guides,
  QDM 5.6, the CQL specification, CMS's eCQM guidance, and the measures' test cases only if they are public without a sign-in. The 2027
  measure table PDF is kept as one reference copy (the lead's decision). Nothing downloaded enters the repository.

---

## Open questions (2026-11-15)

- **Gallery G3: the graphical perio chart (TC-7, branch `chart/tc7-1`, both reviews pass).** Answered on 2026-11-15: "G3 looks good".
  TC-7 is merged.
- **The 837 claim sender when a clearinghouse has no Sender TIN.** `X12Generator.GetISA06` and `GetGS02` write Open Dental Inc's TIN
  (810624427) as the sender of the claim batch when the clearinghouse's Sender TIN is blank (upstream's behaviour). For this fork that names
  a company the office has no agreement with. Recommended: refuse to build the batch while the Sender TIN is blank, with a message that
  points to Setup > Clearinghouses. The other choice is to use the practice's own TIN. **Your decision (2026-11-15): "send your practice's
  own TIN".** Wherever upstream names Open Dental Inc as the sender, the practice is named instead: ISA06, GS02, 1000A NM1 and PER, 837
  4010 and 5010, the 270, and the Emdeon Medical variant. That means the practice's TIN, name and telephone. When one of them is missing,
  the batch is refused with a message saying what to fill in, so Open Dental Inc's TIN is never sent. Package X12-SENDER. **Done** on
  branch `claims/x12-sender-1` ([port-notes/X12-SENDER.md](port-notes/X12-SENDER.md)): the TIN of the practice's billing provider, the
  practice title and the telephone the claims send; with clinics, the clinic's billing provider and phone.
- **A damaged release manifest** (S3a-10, [port-notes/S3a-10.md](port-notes/S3a-10.md) section 8). Every workstation now refuses to publish
  over a manifest it cannot read. If the row were ever really damaged, someone would delete the `documentmisc` rows of types 100 and 101 on
  the database by hand. Should Recopy on the named update server offer that, after a confirmation? **Your decision (2026-11-15): yes.**
  Recopy on the named update server, when the published manifest cannot be read, offers to replace it after a confirmation (package
  S3a-10f). **Done** on branch `net10/s3a10f-1` ([port-notes/S3a-10.md](port-notes/S3a-10.md) section 12): Recopy on the named update
  server asks Yes/No (No is the default); Yes deletes the `documentmisc` rows of types 100 and 101 and publishes the running build as for
  a first release, with a security log entry; No, a silent run and every other workstation change nothing.
- **The program after an elevated self-update** (S3a-10 section 8). After an over-the-shoulder elevation, the restarted program runs as the
  account that gave the rights. Keep that, or should 3b start it without those rights? **Your decision (2026-11-15): the user's own
  account.** In 3b (S3b-7), the copier starts the program again without the rights it was given, as the user who was working.
- **For your information, no answer needed (S3a-7):** ServiceManager now installs every program it offers with the HL7 service's
  settings, Open Dental's own service programs included: own process, automatic start, LocalSystem, display name = service name. InstallUtil
  used to refuse a program that had no installer of its own.

- **E-20 (the UMLS record of the 2026-05-14 value set files): answered (2026-11-16).** You downloaded the release yourself, with your own
  UMLS licence (the value sets in VSAC's three sortings, the Direct Reference Codes file and the Code System Versions file). The record is in
  `docs/port-notes/EHR-6.md`, section 0: names, sizes, sheets, headers, row positions, code systems and versions, counts. Nothing of the files
  is in the repository. The office picks the workbook sorted by CMS ID with the two other files; the other two sortings are not needed.
  EHR-6a-R fixed the importer to the record (E-30: only the seven measures' value sets are kept) and imported your three files on a scratch
  database: 7 of 7 measures at the 2027 versions, all 68 value sets, the 8 direct-code roles resolved. EHR-6a then merges, and EHR-7 is redone
  on top.

- **LOINC's notice years (EHR-6a, `THIRD-PARTY-NOTICES.txt`):** "1995-2026" confirmed by you (2026-11-15).

- **The practice as the X12 sender: three details** (X12-SENDER, [port-notes/X12-SENDER.md](port-notes/X12-SENDER.md)). The sender TIN is
  the TIN of the provider the claims bill under (the Default Insurance Billing Provider; 0 means the Default Provider; Treating Provider
  falls back to the clinic's, then the practice's, Default Provider).
  - **A billing provider marked SSN, not TIN, is refused as the sender.** Upstream sent Open Dental Inc's TIN then. Recommended: keep the
    refusal, because a person's SSN in the file's envelope is not what a TIN field is for. Such an office enters a TIN or the submitter ID
    its clearinghouse gave it under Setup > Clearinghouses. Or should the SSN be sent? **Answered on 2026-11-15: refuse.**
  - **The sender name is the practice title, also for clinics.** Clinics that are separate businesses can enter their own Sender TIN, name
    and phone on their clinic's clearinghouse. Recommended: keep it. **Answered on 2026-11-15: keep the practice title.**
  - **Emdeon Dental's eligibility request (270)** writes ISA06 as `316:` plus the clearinghouse login, as upstream does; 316 may be Open
    Dental Inc's vendor number at Emdeon. Recommended: leave it until an office uses Emdeon for eligibility, then confirm with Emdeon.
    **Answered on 2026-11-15: "Login only".** (Blank is not possible: ISA06 is a required 15-character field.) ISA06 is now the office's
    Emdeon login alone, padded to 15, with no "316:"; an office that uses Emdeon for eligibility confirms the format with Emdeon first.

- **The bridge release:** your decision (2026-11-15): "skip Bridge release". Stage 3a ends with its gate and no .NET Framework release.
  The office goes from 0.0 to the .NET 10 setup, uninstalling 0.0 on every workstation first. Net10-plan decision 21 lists the
  consequences: no newer .NET Framework rollback installer, the x64 release is never published while a 0.0 install remains, and the
  bridge-only launch checks move to 3b.

- **No OpenPMS install to migrate:** your statement (2026-11-15): "nothing in prod has openpms install. Don't worry about migration.
  The office will only migrate once all workstreams are done". From now on no upgrade path from an OpenPMS install (0.0, .NET Framework,
  32-bit, mixed workstations) is designed or tested. The office's switch from Open Dental comes at the end, with the switch-over questions
  above. Migration code already built stays (net10-plan decision 22).

- **Gallery G4, its TC-3c part: the plain "W", the centre line in the Text colour, half-transparent unerupted teeth beneath the
  opaque ones** (branch `chart/tc3c-2`, both reviews pass;
  [port-notes/TC-3c.md](port-notes/TC-3c.md)). Answered on 2026-11-15: "G4 approved". TC-3c is merged.

- **Native 64-bit first for every integration:** your decision (2026-11-15): "For any integration, use native 64bit first. If the
  option is not available, fall back to 32bit". The 32-bit helper becomes the fallback (net10-plan decision 23; answers O-14). Topaz
  (AnyCPU) loads in process, Schick CDR is called directly, QuickBooks Desktop gets the 64-bit QBFC path (QuickBooks 2022 and later; the
  SDK download needs your Intuit sign-in; you downloaded the newest, SDK 18.0, on 2026-11-15) with the helper kept for older QuickBooks, Guru stays on the helper, and 32-bit-only TWAIN
  drivers get a helper path in 3b.

- **Treatment plan grid's right-click menu** (S3a-M3a, [port-notes/S3a-M3.md](port-notes/S3a-M3.md)). In the Treatment Plan edit window,
  upstream's code meant to offer "Go To Appointment" and "Go To Planned Appointment" on a right-click, but an empty menu hid them, so only
  Copy Cell Text and Copy Rows showed. After the menu conversion they show, above Copy Cell Text. **Answered on 2026-11-15: keep them.**

- **Survey section 5 (answered on 2026-11-15).** "32-bit TWAIN manager: yes. System.IO.Ports: approve." and "we can get rid of Directx
  after our switch". So: S3b-7's setup installs the 32-bit TWAINDSM 2.5.1 into SysWOW64 as well as the 64-bit one into System32; the
  System.IO.Ports package is approved (Topaz serial pads on .NET 10, and the dial-up terminal); and no DirectX runtime is shipped or
  chained. DirectX was only for the old DirectX tooth chart and the graphical perio gate: TC-8 moves both to the WPF chart (drawn in
  software on every PC), TC-9a and TC-9b then delete the old engines with SharpDX and Tao, and with no bridge release all three land on
  `main` before the 64-bit build, which never carries DirectX. TC-9a follows TC-8 directly; the office check (the chart's image tests
  on one PC of the office's graphics type, no database) stays as a launch check before the office switches.

- **Approved on 2026-11-19 (2)**: UI-3, the window frame, checked live by the owner ("frame OK"): rounded corners by default ("corners yes", 2026-11-18; OPENPMS_UI3_DWM_CORNERS=off turns them off), the caption hover fix (the close button's red fills to the top and into the corner) and its follow-ups; the frame gallery g-ui2-frame-ui3-2 (commit e6c1e2b45) rows in approved.tsv. Still to check on the desktop: the frame at 125% and 150%.

- **Approved on 2026-11-21 (owner: "yes, also approve")**: UI-9b-4, the last ImageList slice, its icon choices as shown in G-UI3-ui9b4 (https://claude.ai/artifact/Ln8kvGPWHFAqwKMbrzFBzD; 39 pictures: the Accounting, CareCredit, Send Claims, Email Edit, Journal, report, Wiki and Wiki Edit toolbars and four grids). Approved as fixed, like the UI-9a-7 export pictures: the lead's audit found, and a fix round makes, one size box for every icon at 150% and 200% (Copy, Paste, Save As and Print were larger, the Send Claims document icons smaller), the Wiki strip drawn whole (icon 14 was clipped), H1/H2/H3 and B matched in weight to the icons, and a clearer icon for Wiki 6; the redrawn gallery goes to the same link and its rows go into approved.tsv then.

- **Approved on 2026-11-21 (owner: "approve")**: UI-2b, the program font Segoe UI 9 pt (ui/ui2b-1, merged b58c693c3), with the recommended answers: (a) SignatureBox: the signature on screen follows the window font while its saved and printed picture keeps Microsoft Sans Serif 8.25 (paper never changes); (b) the 50 items of docs/port-notes/UI-2b-needs-owner.tsv go to UI-12, whose grid gives the texts room; (c) the desktop check stays: open a payment at Windows text size 125% (steps in docs/port-notes/UI-2b.md). Its gallery G-UI2-ui2b is not reviewed on its own: the same windows are redrawn in the new font with the spacing grid in the UI-12 pilot gallery.

- **Approved on 2026-11-21 (owner: "Both approve")**: the UI-9b-4 fix, merged 84dc2edda; both galleries' 158 pictures are rows in approved.tsv (the 119 earlier-approved ones keep their gallery names), and the four recommended answers stand after its review and the full verify. Galleries: G-UI3-ui9b4-v2 (https://claude.ai/artifact/QEt1TJ6SSALq9eV4ZpoVua) and the 119 earlier-approved pictures that change under the one-box rule, G-UI3-ui9b4-v2-approved (https://claude.ai/artifact/XhBE4nugsYDJwNJn5C6fJT). The answers: (1) Wiki Rename as Fluent Rename A; (2) H1-H3, B and I matched to one cap height (B lighter, H1-H3 larger); (3) at 100% I stays Fluent's own 20 px design (about 2 px taller than B) to keep its serifs crisp; (4) the wiki toolbar, 55 px wider than its 944 px window since UI-2b, is fixed by widening the window when UI-12 reaches it.
- **Waiting for the owner's look (2026-11-22)**: the UI-12 pilot, the layout grid, merged b078f9a61 after seven rounds of audit and fix and the full verify. Gallery G-UI3-ui12p (https://claude.ai/artifact/MddutH5966gD5sWtnzgNEc): the 38 owner rows of G-UI3 first, then the 47 pilot windows and the control styles at 100% and 150%. Decided by the lead under the owner's 'apply general styling sense': the Preferences field column at 50% of the page; FormApptEdit 1332 x 771; at 150% drop-downs and text boxes take the height Windows gives a text box at the scaled font (30 px), so every input in a row has one height; FormEmailInbox's filter groups one height with 50 px source lists; FormInsPlan's adjustments list 329 px wide, keeping the request group on the To-date column. Left for a look on screen: inputs drawn through WM_PRINT (rich text, up-down, date boxes) show a dark corner pixel under the rounded frame in the pictures. After approval: the rollout to the other windows in slices of about 50.
- **Changes asked on 2026-11-22** on G-UI3-ui12p (docs/port-notes/UI-12-owner-review-2.tsv): modernize the Chart's horizontal slider; FormProcCodeEdit's time pattern looks odd; scroll bars still old style (FormProvEdit, ControlAccount); spacing of ControlAccount's top-left aging strip; and overall: "make sure you modernize all element types". Next package UI-13 inventories every element type and restyles every unstyled one; the rollout of the grid follows it. The other pictures got no answer and are not recorded as approved.
- **Approved on 2026-11-22 (owner: "approve")**: UI-13, every element type in the new look, merged b0b51e61f. Gallery G-UI3-ui13 (https://claude.ai/artifact/QbWMJ3BSc6CNBLHDuxohSr): the four owner rows of 2026-11-22 resolved, one style sheet per element type at 100% and 150%, the 47 UI-12 pilot windows redrawn with the new looks, and one window per element type outside the pilot; its 103 pictures are rows in approved.tsv (they supersede the pilot's G-UI3-ui12p pictures, which UI-13 redrew). Recorded limits (UI-13.md section 4): stock frame scroll bars keep Windows' 17 px metric in the 150% pictures; a focused date picker keeps Windows' black value while focused. Next: the grid rollout to the remaining windows in slices of about 50.
- **Approved by the owner (2026-11-23, "slices 1, 2 and 4 approve"; the after pictures are rows of tests/WindowChecksTests/UiApproved/approved.tsv); was waiting since (2026-11-22)**: grid rollout slice 1, merged 1a90930fb after its reviews, the lead's audit and the full verify. Gallery G-UI3-ui12-1 (https://claude.ai/artifact/WUdqoaFQrKuub6ybGEF9bF): 47 windows before and after at 100%, 12 at 150%, the style pictures. Decided by the lead: the clinic picker's label area sized from its text (outer bounds unchanged, so windows not yet on the grid get a narrower box until their slice); the report windows on the 8 px margin. The 12 cut texts the new check found in already-approved pilot windows are fixed in slice 2.
- **Approved by the owner (2026-11-23, "slices 1, 2 and 4 approve"; the after pictures are rows of tests/WindowChecksTests/UiApproved/approved.tsv); was waiting since (2026-11-23)**: grid rollout slice 2, merged 00f10cbc8 after its code review, the visual review (11 findings, all fixed in one round), the lead's look and the full verify. Gallery G-UI3-ui12-2 (https://claude.ai/artifact/XzpNdKMtvUUyHdDWq1ueY9): 51 windows before and after at 100% and 150%, window states (ControlAccount, ControlChart, FormBackup), the style pictures, and a 'pilot follow-up' section with the 12 cut texts of the pilot windows fixed. Decided by the lead: the 150% pictures scale check boxes, radio buttons, buttons and group titles with the labels (one text size per picture); FormClaimsSend's gray toolbar frame keeps its 27 px so the toolbar is not cut. The UI-12T tooling (merged ce1019d0d) re-plans the rest as 10 batches (docs/port-notes/UI-12-batches.tsv).
- **Approved by the owner (2026-11-23, "slices 1, 2 and 4 approve"; the after pictures are rows of tests/WindowChecksTests/UiApproved/approved.tsv); was waiting since (2026-11-23)**: grid rollout slice 4 (batch 2), merged ab84ef8b1 after the code review (4 findings), the visual review (31 findings), one fix round, the lead's look and the full verify. Gallery G-UI3-ui12-4 (https://claude.ai/artifact/FEvcKHJYwPE2h43Ny3bTeb): 104 windows before and after at 100%, the 16 densest at 150%. Decided by the lead: FormReconcileProblem's leftover button (no text, no handler, a designer leftover the other two reconcile windows lack) is removed; the plain-exe windows (CentralManager's plain Forms, DatabaseIntegrityCheck, MySQLManager, DocumentationBuilder) and the OpenPmsDentalGraph files get their own final slice; 13 bottom-anchored controls in earlier slices' windows and the grid dump's anchor are fixed in slice 5.
- **Waiting for the owner's look (2026-11-23)**: grid rollout slice 3 (batch 1), merged f5e74073c after the code review (1 finding: FormUserQuery's Print Orientation choices cut by the code's splitter distance) and the visual review (11 findings), one fix round, the lead's look and the full verify. Gallery G-UI3-ui12-3 (https://claude.ai/artifact/1wx4Jt8E7gkawJ7YCbkVRh): 37 large windows before and after at 100% with their tab pages and views, the densest at 150%. 12 files left as on main, each with its reason in UI-12.md (Central Manager forms, ControlAppt, the dashboard graphs, FormPrintImage, FormPrintSettings, FormClaimFormEdit).

- **Answered on 2026-11-21 (UI-12, after the lead's look at the first pilot round)**: the smallest workstation screen is 1920x1080, so a window's client area may grow to about 1000 px high (FormPreferences grows rather than scrolling its pages where that fits); the gap between stacked fields is 4 px (fields 23 px, rows every 27 px). The first round was not shown to the owner: its fields did not share columns, FormPatientEdit and FormProcCodeEdit had new faults, and the seven busiest windows were not laid out; a second round fixes them before the gallery is reviewed.

- **Changes asked on 2026-11-21 (G-UI3, slices 1 and 2 of UI-10)**: "overall not good. UI lacking consistence. choice of spacing not symetrical"; 38 rows in docs/port-notes/UI-12-owner-review.tsv (texts cut off, checkboxes to the left, text boxes too tall with no spacing, uneven spacing between fields, group titles covered, the doubled drawing in the FormClaimEdit picture, the drop-down/number/date styles in FormPayment, the Chart's procedure buttons and the progress notes page buttons). The owner's answers to the two questions that followed: every window is laid out again on one spacing grid by a tool (this replaces "restyle only, layouts unchanged" of 2026-11-17: 8 px padding in containers, 6 px from a label to its field, a fixed row height, fields in a column share both edges, group titles never covered, the same controls and window sizes where they fit), a pilot on the slice 1 windows and the windows of these rows first, then every window; and the checkbox box on the left of its text everywhere. Package UI-12; the UI-10 galleries stay pending only as the record of the text fit.

- **Pending the owner's final review (owner, 2026-11-21: "get everything built. don't be block by my approval I'll review at the end")**: from now on the UI refresh packages merge after their review and the full verify, without waiting for the gallery; each one is listed here with its gallery and questions (the recommended answers applied), to approve or change at the end. (1) UI-2a-2, no container scales by font (ui/ui2a2-2, merged c3c610458): the owner's desktop check at 150% (before copy .git/od-tools/ui2a2-check/before). (2) UI-9b-4, the last ImageList slice (ui/ui9b4-2): gallery `.git/od-tools/test-results/G-UI3-ui9b4/index.html` (39 pictures, 35 changed; each converted toolbar list as a toolbar, and the four grid columns before and after at 100/150/200%). The 19 new icons (EnumIcons, manifest rows with their purpose): DocumentText = Claims, Get Reports (a page of text lines); Link = the email and wiki editors' internal link buttons; LinkExternal = their Ext Link; Heading1, Heading2, Heading3 = their heading buttons (the Fluent text-format icons, an H1 to H3); Bold, Italic = their Bold and Italic; TextColor = their Color (an A with a colour bar); TextFont = their Font; SaveDraft = their Save as Draft; Settings = Setup, the master page and styles (the gears); LockOpen = the wiki editor's Unlock (open padlock); LockClosed = its Lock (closed padlock); Home = Wiki Home; Rename = Wiki Rename; History = Wiki Hist, the page history; Stop = Record audio, stop (the red square stop.gif); Archive = Wiki Archive (a new Archive box icon, no red cross). Questions, recommended answers applied: the editors' unused "cancel" picture is a trash can (DeleteX); the dashboard's highlighted delete picture is the delete icon with a ring in the danger colour; FormErx's Back and Forward buttons have no pictures today and keep none (give them arrows, or leave?); the unused lists (calendar arrows, permission pictures, account pick, web browser prefs) are kept as arrays only. GridOD: a grid can take a DPI-sized column list and draws it pixel for pixel (sharp at 150 and 200%: tasks, reminders, the setup wizard's "?" column, the info buttons; the four grid pictures at 100% are unchanged); a caller's 96 DPI list draws as before; row heights and column widths unchanged. Check at the end: the editors, the wiki, the claims window, a task list and the dashboard's edit mode at 125% and 150% on the connected desktop. (3) UI-10-1, text fit slice 1 (ui/ui10-1, merged 68f703dd5): gallery `.git/od-tools/test-results/G-UI3-ui101-v2/index.html`; 546 size fixes in 30 designer files measured at Segoe UI 9 pt; 118 needs-owner items in docs/port-notes/UI-10-1-needs-owner.tsv. Questions, recommended answers applied: 54 labels have no free space next to them (widen the group box, shorten the caption, or accept the cut? applied: left as they are and listed); the date picker's text box and drop button (ODDatePicker) are fixed in the control's own package, not by size here; the DashIndividualDiscount text boxes keep their size (the control lays them out itself); the 1.2 zoom step is not chased. (4) UI-10-2, text fit slice 2 (ui/ui10-2-1, merged 565df0ef3): gallery `.git/od-tools/test-results/G-UI3-ui102-v2/index.html` (125 pictures); 1,052 fixes in 60 designer files (529 resized, 523 set to a fixed size instead of sizing themselves); 259 needs-owner items in docs/port-notes/UI-10-2-needs-owner.tsv (65 texts longer than their room with no free space, each outlined in red; 55 overflows of one or two pixels with the letters whole; 22 self-sizing items in other designers or a control's own layout; 109 bounds differences from a control's own layout such as grid scroll bars and date pickers). Questions, recommended answers applied: the 65 texts with no room are left as they are (widen, shorten or accept?); the 1 and 2 pixel overflows with whole letters are accepted; 1 px rows at 125-200% where the free space is smaller than the need are left, as in slice 1. (5) UI-10-3, text fit slice 3 (ui/ui10-3-1, merged 6ecd04f7e): gallery `.git/od-tools/test-results/G-UI3-ui103-v2/index.html` (117 pictures); 456 fixes in 58 designer files (338 resized, 118 set to a fixed size instead of sizing themselves); the fit tool now also reads designers written without spaces around '='; 149 needs-owner items in docs/port-notes/UI-10-3-needs-owner.tsv (44 texts with no free space and 37 self-sizing controls that touch a neighbour, with pictures). Questions, recommended answers applied: the texts with no room are left as they are (wider group box, shorter caption, or accept?); at 150% and 175% about 190 overflows remain, mostly 1-2 px of height with the letters whole, and are accepted (66 and 86 of them visibly cut: look at them in the gallery after the flip). (6) UI-10-5, text fit slice 5 (ui/ui10-5-1, merged 38acf80cf): gallery `.git/od-tools/test-results/G-UI3-ui105-v2/index.html`; ranks 203 to 265 of the ranking, 195 fixes in 59 designer files (152 resized, 43 set to a fixed size instead of sizing themselves; ODDateRangePicker needed none); 90 needs-owner items in docs/port-notes/UI-10-5-needs-owner.tsv (33 texts with no free space, 6 with the letters whole, 6 self-sizing overlaps, 45 bounds differences that are a control's own layout). Questions, recommended answers applied: the texts with no room are left as they are (wider group box, shorter caption, or accept?). (7) UI-10-4, text fit slice 4 (ui/ui10-4-1, merged 252a3883e): gallery `.git/od-tools/test-results/G-UI3-ui104-v2/index.html`; ranks 139 to 202 of the ranking, 277 fixes in 57 designer files (221 resized, 56 set to a fixed size instead of sizing themselves; ControlFamily, FormInsEditPatLog and FormRpPatPortionUncollected needed none); 115 needs-owner items in docs/port-notes/UI-10-4-needs-owner.tsv (40 labels, radio buttons and check boxes with no free space, 39 with pictures; one combo box whose height comes from the font; 75 parts of other controls such as grid scroll bars and ODDatePicker boxes). Questions, recommended answers applied: the texts with no room are left as they are (wider group box, shorter caption, or accept?). (8) UI-10-6, text fit slice 6 (ui/ui10-6-1, merged c60c5d0ab): gallery `.git/od-tools/test-results/G-UI3-ui106-v2/index.html`; ranks 266 to 327 of the ranking, 117 fixes in 48 designer files (108 resized, 9 set to a fixed size instead of sizing themselves; 12 files needed none); 76 needs-owner items in docs/port-notes/UI-10-6-needs-owner.tsv (24 texts with no free space, with pictures; 52 parts of a control's own layout or another designer). Questions, recommended answers applied: the texts with no room are left as they are (wider group box, shorter caption, or accept?). (9) UI-10-7, text fit slice 7 (ui/ui10-7-1, merged 03ce21374): gallery `.git/od-tools/test-results/G-UI3-ui107-v2/index.html`; ranks 328 to 387 of the ranking, 95 fixes in 48 designer files (82 resized, 13 set to a fixed size instead of sizing themselves; 12 files needed none); 69 needs-owner items in docs/port-notes/UI-10-7-needs-owner.tsv (15 texts with no free space, with pictures; 54 grid scroll bars). Questions, recommended answers applied: the texts with no room are left as they are (wider group box, shorter caption, or accept?). (10) UI-10-8, text fit slice 8 (ui/ui10-8-1, merged 81bf8779f): gallery `.git/od-tools/test-results/G-UI3-ui108-v2/index.html`; ranks 388 to 449 of the ranking, 105 fixes in 53 designer files (88 resized, 17 set to a fixed size instead of sizing themselves; 7 files needed none); 65 needs-owner items in docs/port-notes/UI-10-8-needs-owner.tsv (15 texts with no free space, with pictures, among them FormRepeatChargeEdit label10, widened to the window's edge and still 24 px short; 37 grid scroll bars; 13 parts of other designers). Questions, recommended answers applied: the texts with no room are left as they are (wider group box, shorter caption, or accept?). (11) UI-10-9, text fit slice 9 (ui/ui10-9-1, merged 69e83fbaa): gallery `.git/od-tools/test-results/G-UI3-ui109-v2/index.html` (every picture shows as changed from anti-aliasing noise between the two runs; the summary says so); ranks 450 to 509 of the ranking, 42 fixes in 25 designer files (35 resized, 7 set to a fixed size instead of sizing themselves; 35 files needed none); docs/port-notes/UI-10-9-needs-owner.tsv: 5 labels with no free space, with pictures (FormClaimFormItemEdit.labelFormatString, FormDentalSchoolSetup.label3, FormDiscountPlanSubEdit.label5, FormDiseaseDefEdit.label3, FormEhrExportCCD.labelSections), and 43 grid scroll bars. Questions, recommended answers applied: the five labels keep the room they got (wider container, shorter caption, or accept?). (12) UI-10-10, text fit slice 10 (ui/ui10-10-1, merged 8d0978d7d): gallery `.git/od-tools/test-results/G-UI3-ui1010-v2/index.html` (17 windows changed, 42 the same); ranks 510 to 569 of the ranking, 26 fixes in 22 designer files (38 files needed none); docs/port-notes/UI-10-10-needs-owner.tsv: 4 labels with no free space, with pictures (FormEhrPatientConfidentialPrefEdit.label3, FormEmailCertRegister.labelAddressNote, FormMountDefGenerate.label5, FormMountDefs.label1), and 45 grid scroll bars. Questions, recommended answers applied: the four labels keep the room they got (wider container, shorter caption, or accept?). (13) UI-10-11, text fit slice 11, the last (ui/ui10-11-1, merged 6792a43d9): ranks 570 to 674 of the ranking (105 designer files), 97 fixes in 45 designer files (87 resized, 10 set to a fixed size instead of sizing themselves; 60 files needed none); docs/port-notes/UI-10-11-needs-owner.tsv: 10 texts with no free space, with pictures (FormPerioGraphicalSetup.label5, FormProcTools.checkProcButtons, FormProgramLinkHideClinics.label3, FormRepeatChargeEditMulti.label6, FormRxAlertEdit.labelMessage, FormScheduleEdit.labelOps, FormSupplyOrderEdit.label3, FormTaxAddress.label1, FormTestLatency.label3, FormInstallAllocator_Provider.rbIhaveNotRead), and 73 grid scroll bars. Questions, recommended answers applied: the ten keep the room they got (wider group box, shorter caption, or accept?). UI-10 COMPLETE: every one of the 675 ranked windows is fixed (3,004 rows in fit-fixes.tsv) or listed. THE UI-10 GALLERIES ARE REDRAWN (2026-11-21): the slices drew theirs with a test folder that could hold a stale copy of the program (found by UI-10-10); all eleven are redrawn from two fresh builds (main 5c81c4fb5 before UI-10 and 6792a43d9 after every slice, the test folder's copy checked identical) as `.git/od-tools/test-results/G-UI3-ui10<n>-v2`, and the published links show the redrawn ones (slice 11: G-UI3-ui1011-v2). Each page lists the windows whose designer was edited but whose picture at 100% is the same (55 in all): their fixes add height a transparent label does not show, keep a text box at today's size with AutoSize off, sit on a tab page the picture does not show, or matter only at 125% to 200%. Still owed from earlier: the UI-3 frame at 125% and 150% on the owner's desktop.
- **Galleries approved on 2026-11-20 (4)**: G-UI3 the other trees (UI-9b-3: tasks with the reminders grid, the security tree, auto notes, the email inbox buttons, the setup wizard; 24 pictures at 100/150/200%; commit b468014c1); rows in approved.tsv. As recommended: the task states (green check box done, empty box read, solid orange square new, solid orange list for a list with new tasks), the security tree (folder headings, empty box, green check box, filled green box for partly granted; rows 16 px instead of 15), the wizard info circle and its solid accent twin, chevrons for Skip/Next/Back, the email buttons (Edit, Sync, Reply, Reply All, Forward, Search), the email mailboxes list kept and the three unused setup wizard lists removed. Grid column pictures soft at 150/200%: fix in UI-9b-4 (GridOD takes a DPI-sized list).
- **Galleries approved on 2026-11-20 (3)**: G-UI3 the imaging module (UI-9b-2: its two image toolbars and the tree picture list, 9 pictures at 100/150/200%; commit 8ed4133c1); rows in approved.tsv. As recommended: the scanner picture Scan (a viewfinder frame), the memory stick Usb Stick, the scan pictures reusing the approved icons, and ControlImages.ImageListTree kept (no control uses it today; kept for plug-ins, the no-cut rule).
- **Galleries approved on 2026-11-20 (2)**: G-UI3 the main toolbar and the module toolbars (UI-9b-1: 18 pictures, 6 toolbars at 100/150/200%; commit 82525008b); rows in approved.tsv. The new icons as shown: Payment Money, Adjustment Add Subtract Circle, Appts Lists Document Bullet List, Rapid Call Call, Unsched Calendar Arrow Right, New Rx/eRx Pill, Perio Chart the custom dental PerioChart icon, the unused anesthesia picture Pulse; Statement Print, Update Fees Sync, New Claim and Add Insurance the umbrella; captions about 2 px further right. Icon sizes at 150/200% kept as shown (the largest Fluent design that fits, so sizes can differ on one toolbar); the owner answered "approve" to the question of one size per toolbar without choosing that, so the 3.5 rule stays.
- **Galleries approved on 2026-11-20 (1)**: G-UI3 the other programs (UI-9a-7: 34 pictures in Central Manager, the report designer, MsgBoxCopyPaste and ComboBoxMulti; commits 441138e13 and ada2dd54d); rows in approved.tsv. The owner asked to change the XML, HTML and MHT export pictures ("icon position is off": the wide pictures put the icon against the hidden buttons' text); they are 16 x 16 since ada2dd54d and approved as fixed. The 18 new icons as shown: Table, Line, Image, Rectangle, Subreport Document Multiple, List Text Bullet List, Chart Data Bar Vertical, Textbox, Matrix Grid, Save, Open Folder Open, New Document Add, Undo Arrow Undo, Cut, PDF Document Pdf, XML Code, HTML Globe, MHT Document Globe; Central Manager's reused icons as shown.
- **Galleries approved on 2026-11-19 (1)**: G-UI3 the modules and shared projects (UI-9a-6: 45 pictures in 18 windows and controls, 11 WPF buttons in five windows; commit c4ad3a0bc); rows in approved.tsv. The 14 new icons as shown: clear pinboard Pin Off, accounting Calculator, backup Database Arrow Up, deposit Building Bank, send claims Umbrella, tasks Clipboard Task List, move cell Arrow Move, the four dashboard table row/column icons, add chart Data Bar Vertical Add, Topaz Pen, digital sign Signature; the reused arrows, DeleteX, Sync, Preview and Edit as shown.

- **Galleries approved on 2026-11-18 (4)**: G-UI3 Forms R-S (UI-9a-4, 66 button pictures in 36 windows; commit 6a978f854) and G-UI3 Forms T-Z and Forms Reports (UI-9a-5, 77 button pictures in 35 windows; commit ad23a6bce); rows in approved.tsv. Icons as shown: Start recording as Record, the recall pinboard as Pin, the 20 px print preview as Document Search, remove/delete buttons as DeleteX; Excel export as Document Table, Query view as Table Search, copy and paste as the imaging toolbar's icons, the time card's previous/next as arrows, and the laser labels' 17 px icons unstretched.

- **Galleries approved on 2026-11-18 (3)**: G-UI1 part 2e (UI-4c-1, imaging and appointment chrome; commit 95f20d774), G-UI1 part 2f (UI-4c-2, dialogs, progress and the rest; commit 0111b3a82) and G-UI3 Forms M-Q (UI-9a-3, 68 button pictures in 30 windows; commit 97e7cc1cb); rows in approved.tsv. Answers: 2e: the provider slider's typing cursor stays Accent blue; the image selector's item icons move 2 px right, clear of the selection bar, and its category lines become StrokeStrong (darker; both changed after the approval at the owner's request, so those pictures show as changed since approval); the unmounted images' yellow/orange marks, the zoom slider's full-height dividers and the dock header's white Windows button stay as shown. 2f: all as shown (the no-colour swatch, the print preview surround #E0E0E0, the amber integrity triangle with a white mark, the colour button's input outline, and the undefined states: a disabled progress bar in TextDisabled, the indeterminate Accent segment, the swatch's hover/focus/disabled). M-Q: show/hide triangles as Fluent chevrons; Close Case/Plan as a door with an arrow, Dictate as a microphone, Zoom In/Out, Ortho Chart Setup's add as Add.

- **Gallery G-UI3, Forms D-L, approved on 2026-11-18** (UI-9a-2; 61 button pictures in 36 windows to icons; 93 pictures, commit 6065c1862), with the icons as shown: Definition Edit's clear-value red X as DeleteX, the eCW advanced query's green go circle as a plain ArrowRight, Fee Schedule Groups' add plus as Add; rows in approved.tsv.

- **Gallery G-UI1 part 2d approved on 2026-11-18** (UI-4b-3, calendars, date pickers and text boxes; 30 pictures, commit 3750b99d7), with the details as shown: text boxes square (as the approved design sheet draws them; combo boxes keep 4 px corners), the text box's blue focus line whenever it has the cursor (also after a mouse click, as Windows does), the calendar's hovered day SubtleHover, neighbouring months' days TextDisabled, a white inner ring when today is the selected day, and today's ring Accent also in a disabled calendar; rows in approved.tsv.

- **Galleries approved on 2026-11-18 (2)**: G-UI1 part 2c (UI-4b-2, tabs and splitters; 15 pictures, commit c1949bee6) and G-UI3's Forms A-C slice (UI-9a-1, 62 button pictures to icons in 30 windows; 92 pictures, commit 62385b329), with the icons as shown: mailing labels as Contact Card, print preview as Document Search, Update TSI with a refresh arrow (Sync, Fluent arrow_sync; the owner's answer the same day; Send to TSI keeps Send), and the 16 x 14 and 18 x 9 pictures as 16 px icons in their old bitmap size; rows in approved.tsv.

- **Galleries approved on 2026-11-18**: G-UI1 part 2b (UI-4b-1, combo boxes, pickers and lists; 25 pictures, commit 6a9a3f655), G-UI2's toolbar part (UI-6, toolbars, menus and the module bar; 45 pictures, commit 4b0b90736) and G-UI1 part 2's grids (UI-5; 5 pictures at 100-200%, commit 251560343); rows in approved.tsv. Answers with them: a toolbar **overflow button** for buttons past the right edge is **for a later package** (new behaviour, not a restyle; today the Chart toolbar is about 1348 px wide); the **module bar captions stay Arial 8 until UI-2b**; grids: cell gridlines stay 1 device pixel at every scale (only the border and the selection bar grow), the 2 px selection bar may cover the first 2 px of a picture in the first column, the progress notes grid (GridOld) keeps no hover, and **UI-2b drops the India Arial screen switch** of the grids (Segoe UI draws the rupee sign; the print switch stays).

- **Gallery G-UI1 part 4, the custom dental icons** (UI-8, [port-notes/UI-8.md](port-notes/UI-8.md)). **Approved on 2026-11-17**: all 241 pictures (commit 790b97d50): the 20 dental icons, the module bar's Chart button as the new tooth (outline at rest, filled when selected; the medical layout as the medical chart), the 8 new default procedure-button pictures for new databases (Crown-PFM and Crown-Ceramic now differ). The procedure-code tools' "Set to default" for procedure buttons **also gives the new pictures** (owner, 2026-11-17).

- **Drop the India Arial grid font at UI-2b?** (UI-5, [port-notes/UI-5.md](port-notes/UI-5.md) section 7). The grids (GridOD and GridOld) switch their screen font to Arial 8.5 when Windows is set to an Indian culture (en-IN and the other "-IN" cultures); upstream's comment does not know why ("Maybe MS not avail in India?"). The plan (section 5.1) says the switch goes if Segoe UI draws the rupee sign, the likely reason; it does, and so does today's Microsoft Sans Serif, on Windows 10 and 11 (a test checks it). UI-5 keeps the switch, since today's fonts stay until UI-2b. Should UI-2b drop the screen switch, so grids in India show Segoe UI 9 like everywhere else? Recommended: yes. The print switch (printouts in Arial in India) stays either way, since printouts keep their look. **Open:** until you answer, the switch stays.

- **UI-4a's questions** (core controls, [port-notes/UI-4a.md](port-notes/UI-4a.md)), answered 2026-11-17: (1) the 12 default buttons that are not OK or Save (Print in 5 windows, Send in 2, Run Report, Create List, Check, Edit, Try Again) **stay blue** (the accent default button); (2) making the WPF Button, CheckBox and RadioButton focusable (so the focus ring shows; it changes tab order) is **for a later package**, not UI-4a (to be planned with UI-11); (3) the WinForms check box's focus ring cut at its unchanged bounds is **acceptable**; (4) PanelOD stays square, only its border colour changes: **acceptable**. **Gallery G-UI1 part 2 approved on 2026-11-17** (all 30 pictures, commit 896f2ccde; rows in approved.tsv).

- **Gallery G-UI1 part 3, the IconLibrary icons** (UI-7b, [port-notes/UI-7b.md](port-notes/UI-7b.md)). **Approved on 2026-11-17**: all 688 pictures (commit 7a680318a), with the icon choices they show: the module bar's Account as Receipt, Tx Plan as Clipboard Text LTR and the medical Tx Plan as Heart Pulse (the plan's examples have no 32 px design); Tx Plan and medical Tx Plan drawn at 32 px in the 48 px slots at 150% and 200% (Fluent has no 48 px design); Chart32 as Clipboard Pulse until UI-8; Probe as Ruler, Mount / Acquire as Scan Camera, Set guarantor as Person Money. Rows in `tests/WindowChecksTests/UiApproved/approved.tsv`.

- **Gallery G-UI1 part 1, the design sheet** (UI-1, [port-notes/UI-1.md](port-notes/UI-1.md)). **Approved on 2026-11-17**: all 19 pictures (colours, contrast, type, sizes, every control state at 100-200%, the UQ options, high contrast, and the four windows in both font options) answered "Approve" at commit ec88aae10. The one question on it, the selection bar of a grid or list without focus: **the darker grey #616161** (4.9:1 against the selection), not #8A8A8A. Every later package follows this look. The WPF window FrmProviderPick (the 20th picture, drawn at the connected session after UI-1b) approved the same day. All 20 are in `tests/WindowChecksTests/UiApproved/approved.tsv`; the contrast and controls pages redrawn with the darker bar show as "changed since approval" until the owner looks at them again.

- **The UI refresh** ([specs/ui-refresh-plan.md](specs/ui-refresh-plan.md)). Asked on 2026-11-17 ("Do ui refresh. Modernize icons, buttons, styles. Keep it clean and professional"): restyle with every layout unchanged; the new look only (the classic look removed); Segoe UI everywhere (replaces net10-plan decision 9); Fluent UI System Icons. The plan's questions UQ-1 to UQ-9, answered the same day, each as recommended: Segoe UI 9 pt; grid rows follow the text (17 px); the program's blue title bar, with an office's own Misc Colors kept item by item; grey icons with a few role colours; printouts unchanged; the perio chart's drawing keeps Open Dental's look; new-style procedure-button pictures for new databases only; the appointment book's text unchanged; toolbar, menu and tab items grow with their text.

- **The launch checks' two scripts** (LAUNCH-3b, [launch-checks-3b.md](launch-checks-3b.md)). Answered on 2026-11-17: Windows Sandbox is not turned on, and will not be ("Don't want to turn on sandboxing"): the clean-install check is done by a person on a spare PC or VM without .NET ([launch-checks-3b.md](launch-checks-3b.md) section 3); `Test-CleanInstall.ps1` stays unused. `Test-MiddleTierService.ps1`'s client session stays manual (run without `-ClientSession`, the default), so no test certificate ever enters Trusted Root; the program's login through the service is checked by hand at a test PC.

- **When the .NET 10 branch merges into main** (net10-plan decision 25). Answered on 2026-11-16: "B": merge once the main merge and a Full x64 run are green; the gate's person checks become launch checks before the office release (`docs/launch-checks-3b.md`). Asked next: can the person checks be automated? Answered "yes" to the three that can (a WebView2 window check, `tools/Test-MiddleTierService.ps1` run elevated by the owner, `tools/Test-CleanInstall.ps1` in Windows Sandbox once the owner turns it on).

- **The elevated installer test on .NET 10** (S3b-7, [port-notes/S3b-7.md](port-notes/S3b-7.md) section 6). `tools/Test-Installer.ps1` starts the installed connection settings tool briefly, with no arguments (it connects to nothing), to read which runtime it loaded, then closes it; it removes the `TWAINDSM.dll` the test installs only when the PC had none before, and an older one stays replaced by 2.5.1. **Answered on 2026-11-16: yes.** The same handling applies to the 32-bit TWAIN manager that S3b-8 adds.

- **Trojan's registry key names** (N64-A, [port-notes/N64-A.md](port-notes/N64-A.md)). Trojan's install folder is now looked up under
  Software\TROJAN BENEFIT SERVICE (as before) and also under Software\Trojan (the name Open Dental's manual gives), in the 32-bit and then
  the 64-bit registry view. **Answered on 2026-11-15: keep both.**

## Not asked again (already settled)

- How package migration levels are numbered (B-ACCT 2, B-PRINT 1, B-SHELL 6, M2-EST 1, M2-UDS 1, V-BRIDGE1 2, V-PEARL2 1, M2-SHELL's old question 4): decided on 2026-11-01 and done: the package steps now have dated levels 2026110101 to 2026110104, and new steps always go above the highest one.
- M2-01 5 (hide Employee Titles until the title feature ships): M2-EMP shipped employee titles, so it stays visible.
- M2-01's question about test databases with fixed names clashing in parallel runs: fixed on `port/int-4`.
- B-APPT's request that the integrator pick one approach for setting names that aren't confirmed yet: settled on the integration branch. All packages use the program's own setting rows.
- BIN-B 6 (the licence list still naming "Drifty", "Stdole", "Zxing" and "Tao_Freeglut"): REL-FILES removed those entries.
- REL-FILES 8 (build RdlEngine and RdlViewer from source before v0.0): done in RDL-SRC; the setup checks that both are the files built from this repository.
- REN-PROOF 1 (`TempFolderName`) and 2 (window sizes from the pre-rename folder), REL-FILES 6 (Benco values), BIN-A 1 (the four bridges) and BIN-A 4 (the DirectX runtime): now items 1, 2, 6 and 7 of "Decided by default for v0.0".
- M16-INTEGRATION has no questions of its own; the capture questions it points to are in `imaging-capture-owner-questions.md`.