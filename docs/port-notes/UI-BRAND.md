# UI-BRAND: the program no longer calls itself OD or Open Dental

The owner (2026-11-24): "small brand change. It still says OD programs. We're no longer called OD." Branch `ui/uibrand-1` from
`642322a74`. The product is "Open PMS - Dental" (`CodeBase.VendorDirect.ProductIdentity.ProductName`, `AssemblyProduct`), "Open PMS" for
short. Gallery G-UI3-uibrand, audit [UI-BRAND-audit.md](UI-BRAND-audit.md).

## 1. The scan

Every string literal of the shipped sources was read: C# literals (regular, verbatim, interpolated, raw), `.resx` strings, XAML attribute
values and texts, the setup script and pages (`installer/`), and the help texts of `OpenPmsDental/Resources/PrefInfos.xml` (its
`ControlText` and `Details` texts, each link read on its own). `PrefInfos.xml` is an embedded string resource
(`OpenPmsDental/Properties/Resources.resx`, `Properties.Resources.PrefInfos`): `FormPreferences` reads it and `FormWebBrowserPrefs`
shows each `Details` text as the info pop-up of the Preferences window. Of the files that `OpenPmsDental` and `OpenPmsDentalBusiness`
embed as string resources, only `PrefInfos.xml` and `THIRD-PARTY-NOTICES.txt` (attribution) name OD or Open Dental.
Left out: tests, developer tools (xCrudGenerator, DocumentationBuilder, PluginExample, tools), documentation, `db/`, third-party and build input. A hit is a literal with "OD" as a word (case-sensitive) or "Open
Dental", "OpenDental", "Open-Dental" in any case.

- At the branch point: **734 hits**, 397 with OD and 337 with Open Dental (59 of them in `PrefInfos.xml`: 54 links to
  opendental.com and 5 texts). 362 of the OD hits are the designers' `[Category("OD")]`
  attributes (the Visual Studio property grid's category of the program's own controls; developer-only), left out by rule.
- Each other hit was decided by its context. **9 texts in 6 files** were the program naming itself (section 2) and changed.
- The rest stay: **363 hits in 260 rows** of `NoHqTests/Data/brand-kept.tsv`, each with its reason (section 3).

## 2. What changed

Text lines and string literals only; no layout line changed.

| Where | Before | After |
|---|---|---|
| `FormWarnToCloseComputers` label2 (report allocator warning) | Close Other OD Programs Now!! | Close Other Open PMS Programs Now!! |
| `FormWarnToCloseComputers` lblMessage (resx) | ... close all running opendental programs connected to this database ... | ... close all running Open PMS programs ... |
| `FormInstallAllocator_Provider` richTextBox1 (resx) | ... provide you Opendental program with the ability ... | ... provide you Open PMS program ... |
| `FormXChargeReconcile` label2 | View X-Charge transactions with no payment in OD. | View X-Charge transactions not paid in Open PMS. |
| `FormXChargeReconcile` label3 | View payments in OD with no X-Charge transaction. | View Open PMS payments not found in X-Charge. |
| `FormPath` label1 (resx, Paths window) | example folders `\\SERVER\OpenDentalExport\`, `C:\OpenDentalExport\` | `\\SERVER\OpenPmsExport\`, `C:\OpenPmsExport\` |
| `FormPath` label3 (resx) | example folders `\\server\OpenDentalLetters\`, `C:\OpenDentalLetters\` | `\\server\OpenPmsLetters\`, `C:\OpenPmsLetters\` |
| `FormQueryMonitor` log file header | ..., OD User: ... | ..., Open PMS User: ... |
| `ControlAppt` blockout error text (shown in the error window) | OD Version: ... | Open PMS Version: ... |

**Fit.** Measured with TextRenderer in the program's font (Segoe UI 9, the warning's heading Segoe UI 13.5 bold): the X-Charge labels'
first wording ("... with no payment in Open PMS.", "View payments in Open PMS with no X-Charge transaction.") was 320 px in labels of 296
and 288 px, so they were reworded to 274 and 271 px. The warning's heading is 347 of 368 px (it was 285). The resx texts are shorter than
before or sit in multi-line labels and a scrolling text box. **No text is listed for the lead: every changed text fits.**

## 3. What stays, and why

`brand-kept.tsv` (file, text, reason; the text is the whole literal, or a part of at least 20 characters of a long one, as the source
writes it). Rows by reason:

| Rows | Reason |
|---:|---|
| 56 | Origin notes: adoption of an Open Dental database, schema parity steps, data written by Open Dental, features that were Open Dental's hosted ones |
| 35 | Open Dental's database conversion history (`ConvertDatabases*`): stored values, preference names, program property keys |
| 25 | Code identifiers: `FormOpenDental` (class, Lans category, plug-in hooks, log names), settings names |
| 21 | Open Dental's own Windows service (OpenDentalService), which this program does not ship: its alerts, setup and errors name it correctly (3 of them Preferences pop-up texts) |
| 18 | File, folder and service names on disk (`OpenDentalServerConfig.xml`, `OpenDentalService.exe`, ...) |
| 14 | HL7 fields (sending application, assigning authority, MSH) |
| 12 | Open Dental hosted services and programs (eConnector, eServices, mobile apps) named in alert and log descriptions |
| 10 | Open Dental HQ-only code and texts, never reached in an office (one: the help text of the hidden `checkSubmitExceptions`, section 4) |
| 2 | The 54 links of the Preferences pop-ups to Open Dental's online manual (52) and unit test documentation (2): flagged, section 4 |
| 8 | GPL attribution and copyright notices (About box, `SharedAssemblyInfo.cs`, setup) |
| 8 | Developer-only texts (logs, developer exceptions, designer descriptions, development paths) |
| 7 | Vendor bridge fields (DemandForce, Sirona, Vipersoft, Oryx, EHG, CallFire, Canadian certificate names) |
| 5 | Setup script comments |
| 5 | Generated settings names and the default Middle Tier URL path |
| 5 | The database name: the default database is still called `opendental` |
| 29 | Others, one to three rows each: QuickBooks' application name, the Open Dental Cloud note of a Preferences pop-up, program property keys, a preference name, the Practice by Numbers link (section 4), the About box's matching of Open Dental's stored preference defaults, Open Dental's host names (the block list), DICOM, X12 and NewCrop codes "OD", Infobutton and Direct protocol fields, the Credential Manager key, the Middle Tier URL path, ServiceManager's "(Open Dental)" mark of a legacy service, the EHR test message, the sheet import compatibility note |

Also out of the scan: the `OpenDent`
prefix and the `OpenDentImages` folder name (section 5).

## 4. Links that send the office to Open Dental (flagged)

- **Practice by Numbers** (`WpfControlsOD/Bridges/PracticeByNumbers.cs`): its button opens
  `http://www.opendental.com/manual/portalpracticebynumbers.html`, Open Dental's manual page. Kept; the owner decides where it should
  point (no page of ours exists yet).
- **The Preferences window's info pop-ups** (`PrefInfos.xml`, embedded, shown by `FormWebBrowserPrefs`): **54 links** to Open Dental's
  site, 52 to its online manual (`opendental.com/manual/...`, e.g. "See Report Server", "see Auto Ortho") and 2 to its unit test
  documentation (`opendental.com/resources/UnitTestsDocumentation.xml`), shown as if they were this program's help. Their texts also
  name Open Dental's service ("Open Dental Service", "OpenDentalService"), "Open Dental Cloud users", and, for the
  `SendUnhandledExceptionsToHQ` option, "Unhandled Exception errors will automatically be sent to Open Dental headquarters"; that
  option's check box (`checkSubmitExceptions` in `UserControlMainWindowMisc`) is hidden and nothing is sent, so that pop-up never shows.
  Kept as they are; question 6 for the owner.
- Open Dental's email templates of `ConvertDatabases6` (logos on opendental.com) and the slave-monitor message of `ConvertDatabases7`
  are conversion history: only a database that went through those Open Dental versions carries the templates, and this program never
  runs those conversions.

## 5. Questions for the owner

The owner answered all six on 2026-11-24 by taking every recommendation; UI-BRAND-2 carried them out (section 10).

1. **QuickBooks' application name** is still "Open Dental" (`QuickBooksModule`, `QuickBooksInProcess`, `QuickBooksLateBound`, and the
   tests' reference copy). QuickBooks shows it when it asks the administrator to allow the connection and keeps the authorization under it
   in the company file. Change it to "Open PMS - Dental"? The office's QuickBooks administrator then allows the program once more.
2. **Program property descriptions that show in Program Links** and are also the property's key in the database: Dental Studio's
   "UserName (clear to use OD username)" and Podium's "Enter 0 to use Open Dental for sending review invitations, or 1 to use the
   Service". Renaming them needs a ForkMigration step that updates `programproperty.PropertyDesc` with the code. Do it?
3. **Practice by Numbers' help link** (section 4): where should it point?
4. **`OpenDentImages`**: the A to Z folder tool (`FrmAtoZFoldersCreate`) creates a folder of that name by default, and the Paths window,
   the Backup window and UpdateFileCopier use it as the example. It is Open Dental's folder name, not "OD" or "Open Dental", so this
   package left it. ServiceManager's help text still says services "begin with OpenDent..." (they begin with OpenPmsDental, or OpenDent
   for Open Dental's). Rename both?
5. Protocol fields that name the program to a vendor were kept as they are (Infobutton requester "Open Dental Software, version...",
   Direct MDN reporting agent "OpenDental", DemandForce `managementSystemName`, CallFire campaign name, Sirona and Vipersoft station
   names). Change any of them?
6. **The Preferences window's info pop-ups** (section 4) send the office to Open Dental's online manual through 54 links (52 manual
   pages, 2 to its unit test documentation) and name Open Dental's service and Open Dental Cloud. Strip the links (keep the texts),
   point them to pages of ours once they exist, or keep them? And the help text of the hidden `checkSubmitExceptions` ("errors will
   automatically be sent to Open Dental headquarters"): reword it to say nothing is sent, or leave it, since the check box never shows?

## 6. Translations

`Lan.F`/`Lan.g` use the English text as the key. The init database (`db/init/new-database.sql`) has no translated row (`languageforeign`
is empty; the 12 rows of `language` are obsolete ones, none of them affected), so **0 shipped translated rows** are affected. An office
database adopted from Open Dental may hold its own translations of the seven changed window texts (section 2, the first seven rows); those
no longer apply and show in English until translated again. The brand wins (the brief). The Query Monitor log header and the blockout
error text are not translated.

## 7. The test

`NoHqTests/BrandTests.cs` reads the same files with the same rules (its own C# lexer, the resx and XAML readers, and the `PrefInfos.xml`
reader, which reads each link of a help text on its own and the text without its links, so a kept link never keeps the words around it):

- `EveryLiteralThatNamesOdOrOpenDental_IsKeptWithAReason`: a literal that names OD or Open Dental and is not kept fails.
- `EveryKeptRow_IsStillInTheSource`: a kept row that no literal has any more fails (cut the list).
- `EveryKeptRow_HasAReason_AndIsListedOnce`: no row without a reason, no row twice, each row names the brand.
- `Lexer_FindsLiteralsOfEveryKind_AndSkipsCommentsAndCategories`: the lexer's own check.
- `PrefInfosLexer_ReadsHelpTextsAndLinksApart`: the `PrefInfos.xml` reader's own check, and that the file is still an embedded string
  resource that the scan reads.

A new text where the program calls itself OD or Open Dental therefore fails NoHqTests: say "Open PMS" (or "Open PMS - Dental"), or, when
the text is not the program naming itself, add a row with its reason.

## 8. Gallery G-UI3-uibrand

`.git/od-tools/test-results/G-UI3-uibrand` (zip beside it): 4 pictures, 4 changed, at 100%. The before pictures are drawn from this
branch's build with `OPMS_UI_BRAND_GALLERY_BEFORE` (`tests/UiRefreshTests/BrandGallery.cs` puts the old words back). `<W>` is
`.git/od-tools/test-results/G-UI3-uibrand-src`.

```
OPMS_UI_BRAND_GALLERY=<W> run-tests.ps1 -Mode Targeted -Platform x64 -Include UiRefreshTests
   (Gallery_UiBrand_BeforeAndAfter draws <W>/before/windows-100 and <W>/after/windows-100 through FitGallery in the program's font)
tools/New-UiGallery.ps1 -Before <W>/before -After <W>/after -Name G-UI3 -Out .git/od-tools/test-results/G-UI3-uibrand
```

## 9. Results

Full x64 build through the lane at `aa78eaee8`: 0 errors. Targeted run `test-results/uibrand-targeted-1.txt` (`-Platform x64`: this
worktree's test folders still hold the x86 test hosts of an older build, which make an x86 run load nothing): P262BillingTests 54 passed;
PrintGuardTests 19 passed, 1 skipped; NoHqTests 134 passed; WindowChecksTests 31 passed, 44 skipped (desktop-only); UnitTests 1728
passed, 3 skipped; UiRefreshTests 374 passed, 52 skipped (opt-in galleries and desktop pictures; the UI-BRAND gallery ran); 0 failed.

Fix round (`ui/uibrand-2`, the review's finding: `PrefInfos.xml` ships): full x64 build through the lane at `9d3c8c4a1`: 0 errors.
Targeted run `test-results/uibrand-targeted-2.txt` (`-Platform x64`): P262BillingTests 54 passed; PrintGuardTests 19 passed, 1 skipped;
NoHqTests 135 passed (the new `PrefInfosLexer_ReadsHelpTextsAndLinksApart`); WindowChecksTests 31 passed, 44 skipped; UnitTests 1728
passed, 3 skipped; UiRefreshTests 373 passed, 53 skipped (the opt-in UI-BRAND gallery not run again: no window text changed); 0 failed.
Test-NoEolFlips against `642322a74`: 0 files changed their line ends.

## 10. The owner's answers (UI-BRAND-2)

The owner answered the six questions of section 5 on 2026-11-24 by taking every recommendation. Branch `ui/uibrand2-1` from `b1fa6c0d4`.

| Question | Answer | What changed |
|---|---|---|
| 1. QuickBooks' application name | "Open PMS - Dental" | The four `SessionManager.OpenConnection` calls (`QuickBooksModule` in the 32-bit helper, `QuickBooksInProcess`, `QuickBooksLateBound`, the tests' reference copy `QuickBooksToday`) and the Helper32Tests expectations. The office's QuickBooks administrator allows the program once more: the Setup QuickBooks window (`FormQuickBooksSetup.labelConnectQB`) and the release notes (`next.md`, QuickBooks Desktop; `ui-refresh.md`, the name) say so. |
| 2. Program property descriptions | Rename with a fork migration step | Step 2026112451, `ProgramPropertyDescRenameStep` (README of the fork migrations): Dental Studio's `UserName (clear to use OD username)` becomes `UserName (clear to use the Open PMS username)`, Podium's `Enter 0 to use Open Dental for sending review invitations, or 1 to use the Service` becomes `Enter 0 to use Open PMS for sending review invitations, or 1 to use the Service`. PropertyDesc only, by key, so the office's values stay; idempotent. `DentalStudio.SendData` and `Podium.PropertyDescs.UseService` read the new texts (`ProgramPropertyDescRenames`). The adoption tools plan it (`ProgramPropertyDescPlan`, printed by migration-preview), adopt's row check holds `programproperty` to the plan (`AdoptionRowCheck.CheckProgramProperty`), and `allowed-differences.adoption.json` allows `programproperty.PropertyDesc`. |
| 3. Practice by Numbers' link | Practice by Numbers' own site | The button opens `https://www.practicebynumbers.com/`, their public home page, in place of Open Dental's manual page. |
| 4. `OpenDentImages` and ServiceManager | Rename both | `ProgramNames.DefaultAtoZFolderName` = `OpenPmsImages`: the Create AtoZ Folder window's default name and the share it creates (`FrmAtoZFoldersCreate`), the examples of the Paths window (`C:\OpenPmsImages`, `\\SERVER\OpenPmsImages`) and the Backup window (`C:\OpenPmsImages\`), and UpdateFileCopier's debugging default. An office's configured folder (`DocPath`) is read as it is: no move, no rename. ServiceManager's help text names the services as `ServicesHelper.GetAllOpenDentServices` finds them: "begin with "OpenPmsDental..." (this program's) or "OpenDent..." (Open Dental's, marked so in the list)" (`ProgramNames.ServicePrefix` and `LegacyServicePrefix`; the list marks Open Dental's with " (Open Dental)"). |
| 5. Vendor and protocol fields | Keep until the vendor confirms | DemandForce `managementSystemName`, the CallFire campaign name, the Sirona and Vipersoft station names, the HL7 fields, the Direct MDN reporting agent and the Infobutton requester stay; their 21 rows in `brand-kept.tsv` say "kept until the vendor confirms another name". |
| 6. The Preferences pop-ups | Strip the links, keep the texts readable | `PrefInfos.xml`: the 54 links to opendental.com are gone (52 manual pages, 2 unit test documentation), each link's text kept as plain text; the texts that were only a pointer ("see Auto Ortho", "See Report Server") say what the option does, and pointer sentences ("See Claim Snapshot.", "See Troubleshooting Slowness ...", "See Payment Plan Q and A ...") are dropped. The hidden `checkSubmitExceptions` says "Nothing is sent. This program never sends unhandled exception errors anywhere; this option has no effect." The Open Dental service (claim reports, Claim Snapshot, the radiology alerts) and Open Dental Cloud (Read-Only Server) are named as Open Dental's, which this program does not ship. |

**Not changed, on purpose.** The init database's `DocPath` seed (`C:\OpenDentImages\`, `db/init/new-database.sql`, written by the seed
dump and `tools/DevDbTool/InitSqlScrub.SeedDocPath`): a new install sets its A to Z folder in the Paths window or with Create AtoZ
Folder, whose default is now OpenPmsImages; the seed value is the coordinator's call when the init file is regenerated (question below).
Open Dental's conversion history (`ConvertDatabases3` and `4` write the old property texts; this program never runs them) and code
comments that mention `OpenDentImages` stay. The Program Links property texts in the init database are renamed by the step when the
init file is regenerated.

**Fit.** Measured with TextRenderer: the Setup QuickBooks label (397 x 47) holds the new text in 3 lines, 391 x 45 px in Segoe UI 9 and 392 x
39 in its designer font (Microsoft Sans Serif 8.25), as the old text did (388 x 45). ServiceManager's help label (337 x 155) gains one
line. The Backup window's example box and the Create AtoZ Folder name box are as wide as before and the new name is shorter.

**Translations.** The changed window texts (Setup QuickBooks, ServiceManager, Paths, Backup) have no translated row in the init database;
an office's own translations of them show in English until translated again. The two Program Links property texts are keys, not
translated texts.

**Tests.** `NoHqTests.BrandTests`: `QuickBooks_ConnectsUnderTheProductName` (every QuickBooks connection, found by its connect call, uses
`ProductIdentity.ProductName`) and `PrefInfos_LinkToNoOpenDentalPage_AndSendNothing`; `brand-kept.tsv` 255 rows (was 260: the
QuickBooks, Dental Studio, Podium, Practice by Numbers rows and the `PrefInfos.xml` link and headquarters rows went; the step's two old
texts, ServiceManager's help text and two `PrefInfos.xml` texts that now say what belongs to Open Dental came). `ForkMigrationTests.ProgramPropertyDescRenameStepTests`:
a new database (the init file's `program` and `programproperty` tables) and an existing office's rows, each run twice, in scratch databases
`uibrand2_*` that are dropped afterwards, and the plan on rows alone. `UiRefreshTests.Brand2Tests.AtoZFolder_NewInstallsDefaultToOpenPmsImages`
(the Create AtoZ Folder window's default built in the test, and the other sites). Helper32Tests' expected calls name "Open PMS - Dental".

**Gallery G-UI3-uibrand2.** `.git/od-tools/test-results/G-UI3-uibrand2` (zip beside it), at 100%: Setup QuickBooks, Paths, Backup,
ServiceManager and Create AtoZ Folder, before and after. The before pictures come from this branch's build with
`OPMS_UI_BRAND2_GALLERY_BEFORE` (`tests/UiRefreshTests/Brand2Gallery.cs` puts the old words back). `<W>` is
`.git/od-tools/test-results/G-UI3-uibrand2-src`.

```
OPMS_UI_BRAND2_GALLERY=<W> run-tests.ps1 -Mode Targeted -Platform x64 -Include UiRefreshTests
tools/New-UiGallery.ps1 -Before <W>/before -After <W>/after -Name G-UI3 -Out .git/od-tools/test-results/G-UI3-uibrand2
```

**Question for the coordinator.** Should the regenerated init database's `DocPath` seed become `C:\OpenPmsImages\`
(`InitSqlScrub.SeedDocPath` and the InitSqlTests that expect the seed)? UI-BRAND-2 left it, because the brief lists only the four sites.
