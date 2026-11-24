# UI-BRAND: the program no longer calls itself OD or Open Dental

The owner (2026-11-24): "small brand change. It still says OD programs. We're no longer called OD." Branch `ui/uibrand-1` from
`642322a74`. The product is "Open PMS - Dental" (`CodeBase.VendorDirect.ProductIdentity.ProductName`, `AssemblyProduct`), "Open PMS" for
short. Gallery G-UI3-uibrand, audit [UI-BRAND-audit.md](UI-BRAND-audit.md).

## 1. The scan

Every string literal of the shipped sources was read: C# literals (regular, verbatim, interpolated, raw), `.resx` strings, XAML attribute
values and texts, and the setup script and pages (`installer/`). Left out: tests, developer tools (xCrudGenerator, DocumentationBuilder,
PluginExample, tools), documentation, `db/`, third-party and build input. A hit is a literal with "OD" as a word (case-sensitive) or "Open
Dental", "OpenDental", "Open-Dental" in any case.

- At the branch point: **675 hits**, 397 with OD and 278 with Open Dental. 362 of the OD hits are the designers' `[Category("OD")]`
  attributes (the Visual Studio property grid's category of the program's own controls; developer-only), left out by rule.
- Each other hit was decided by its context. **9 texts in 6 files** were the program naming itself (section 2) and changed.
- The rest stay: **304 hits in 253 rows** of `NoHqTests/Data/brand-kept.tsv`, each with its reason (section 3).

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
| 18 | Open Dental's own Windows service (OpenDentalService), which this program does not ship: its alerts, setup and errors name it correctly |
| 18 | File, folder and service names on disk (`OpenDentalServerConfig.xml`, `OpenDentalService.exe`, ...) |
| 14 | HL7 fields (sending application, assigning authority, MSH) |
| 12 | Open Dental hosted services and programs (eConnector, eServices, mobile apps) named in alert and log descriptions |
| 9 | Open Dental HQ-only code and texts, never reached in an office |
| 8 | GPL attribution and copyright notices (About box, `SharedAssemblyInfo.cs`, setup) |
| 8 | Developer-only texts (logs, developer exceptions, designer descriptions, development paths) |
| 7 | Vendor bridge fields (DemandForce, Sirona, Vipersoft, Oryx, EHG, CallFire, Canadian certificate names) |
| 5 | Setup script comments |
| 5 | Generated settings names and the default Middle Tier URL path |
| 5 | The database name: the default database is still called `opendental` |
| 28 | Others, one to three rows each: QuickBooks' application name, program property keys, a preference name, the Practice by Numbers link (section 4), the About box's matching of Open Dental's stored preference defaults, Open Dental's host names (the block list), DICOM, X12 and NewCrop codes "OD", Infobutton and Direct protocol fields, the Credential Manager key, the Middle Tier URL path, ServiceManager's "(Open Dental)" mark of a legacy service, the EHR test message, the sheet import compatibility note |

Also out of the scan: `OpenPmsDental/Resources/PrefInfos.xml` (read only by DocumentationBuilder, not shipped); the `OpenDent`
prefix and the `OpenDentImages` folder name (section 5).

## 4. Links that send the office to Open Dental (flagged)

- **Practice by Numbers** (`WpfControlsOD/Bridges/PracticeByNumbers.cs`): its button opens
  `http://www.opendental.com/manual/portalpracticebynumbers.html`, Open Dental's manual page. Kept; the owner decides where it should
  point (no page of ours exists yet).
- `PrefInfos.xml` links every preference to Open Dental's manual and says errors are "sent to Open Dental headquarters"; it is not
  shipped (DocumentationBuilder only).
- Open Dental's email templates of `ConvertDatabases6` (logos on opendental.com) and the slave-monitor message of `ConvertDatabases7`
  are conversion history: only a database that went through those Open Dental versions carries the templates, and this program never
  runs those conversions.

## 5. Questions for the owner

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

## 6. Translations

`Lan.F`/`Lan.g` use the English text as the key. The init database (`db/init/new-database.sql`) has no translated row (`languageforeign`
is empty; the 12 rows of `language` are obsolete ones, none of them affected), so **0 shipped translated rows** are affected. An office
database adopted from Open Dental may hold its own translations of the seven changed window texts (section 2, the first seven rows); those
no longer apply and show in English until translated again. The brand wins (the brief). The Query Monitor log header and the blockout
error text are not translated.

## 7. The test

`NoHqTests/BrandTests.cs` reads the same files with the same rules (its own C# lexer, the resx and XAML readers):

- `EveryLiteralThatNamesOdOrOpenDental_IsKeptWithAReason`: a literal that names OD or Open Dental and is not kept fails.
- `EveryKeptRow_IsStillInTheSource`: a kept row that no literal has any more fails (cut the list).
- `EveryKeptRow_HasAReason_AndIsListedOnce`: no row without a reason, no row twice, each row names the brand.
- `Lexer_FindsLiteralsOfEveryKind_AndSkipsCommentsAndCategories`: the lexer's own check.

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
