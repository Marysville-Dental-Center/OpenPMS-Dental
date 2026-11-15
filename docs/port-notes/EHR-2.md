# Port note: EHR-2 (value sets)

Package EHR-2 of `docs/specs/ehr-rebuild-plan.md` (section 7, with section 8), on branch `ehr/ehr2-1` from `main` `2e5a61a67`. The owner
answered E-1 to E-19 as recommended on 2026-11-08; this package carries out E-7 (value sets by file import of VSAC's June 2013 EP release
only, the Excel workbook or the SVS XML, no API), the import part of E-4 (the value-set and CPT imports need Setup) and E-17 (the
code-system tables stay "loaded directly", with this package's status view). It is the one EHR package with a schema change: the fork-only
table `ehrcode`, created empty by step 2026111401. No new `PrefName` (the release row is read by string), no new third-party file, no
download, no project file edited, no VSAC, UMLS, CPT, CDT or SNOMED CT content anywhere in the repository. Synthetic data only.

## 0. Step 5 (the record of the real files): not done

The plan's step 5 is documentation only: someone with a UMLS licence opens the archived June 2013 EP package once and records, here, its
file names and formats, the workbook's sheet and header names and one SVS file's element names (E-7). That needs a UMLS sign-in, which
this package does not have and may not use; nothing was downloaded. So the readers are built from the public descriptions of section 8.2
and VSAC's SVS layout, and they accept the names below. **What the record must say, and what then changes:**

| To record from the real package | What the reader does now | If the record differs |
|---|---|---|
| The files in the download (names, `.xlsx` or `.xml`, one file or a `.zip`, folders) | Any `.xlsx` or `.xml` at any depth of a `.zip`; a `.zip` inside the `.zip` is named and not read; other entries named and skipped | Nothing, unless the package nests zips |
| Each workbook's sheet names, and the rows above the header | Every sheet is read; the header row is the first of the first 30 rows that names at least two known columns, one of them Code or Value Set OID; a sheet without one (a "Read Me") is skipped and named | The search depth |
| The header names, exactly | Compared without white space and case: CMS ID (also CMS IDs, Measure ID(s), eMeasure ID, CMS eMeasure ID); Value Set Name; Value Set OID (also OID); QDM Category (also Category); Code (also Code Value); Descriptor (also Description, Code Description, Display Name); Code System (also Code System Name); Code System OID; Definition Version (also Value Set Version, Expansion Version, Version, read for the report only). Required: Value Set OID, Code, and Code System or Code System OID | `EhrValueSetImport._dictHeaders` and the synthetic workbooks' default headers (`ValueSetFixtures.ArrayHeadersDefault`) |
| How a code cell is stored (text, or a number for numeric codes such as CPT) | A number is taken as the sheet stores it (`<v>`), not reformatted | A code that Excel stores as a number with a lost leading zero would need the column's format |
| One SVS file's element and attribute names, and its Group names | `DescribedValueSet` or `ValueSet` (`ID`, `displayName`, `version`), `Concept` (`code`, `codeSystem`, `codeSystemName`, `displayName`), `Group` (`displayName`) with `Keyword`s: CATEGORY gives the QDM category, a name containing "CMS" or "Measure" gives the measures; namespaces ignored | The Group names |

The lead or the owner records this (owner question 1 below). The manifest's names and categories should be checked against CMS's "2013
June eCQM Specifications for Eligible Professionals" (download batch E lists it; it was not in `batch-e-20261109`): section 5.

## 1. The table and the step

- `EhrSteps.cs` (new, globbed): `EhrTables`, a `ForkTableSet` ("EHR value sets", level 2026111401), with the DDL exactly as `SHOW CREATE
  TABLE` prints it on MariaDB 10.11 (checked by a test against the live server):

  ```
  CREATE TABLE `ehrcode` (
    `EhrCodeNum` bigint(20) NOT NULL AUTO_INCREMENT,
    `MeasureIds` varchar(255) NOT NULL,  `ValueSetName` varchar(255) NOT NULL,  `ValueSetOID` varchar(255) NOT NULL,
    `QDMCategory` varchar(255) NOT NULL, `CodeValue` varchar(255) NOT NULL,     `Description` text NOT NULL,
    `CodeSystem` varchar(255) NOT NULL,  `CodeSystemOID` varchar(255) NOT NULL,
    PRIMARY KEY (`EhrCodeNum`), KEY `ValueSetOID` (`ValueSetOID`), KEY `CodeValueCodeSystem` (`CodeValue`(100),`CodeSystem`(50))
  ) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci
  ```

  The columns of `EhrCode` except `IsInDb`. `Description` is `text`, because value-set descriptions (RxNorm names among them) can pass 255
  characters. The key on (`CodeValue`,`CodeSystem`) uses prefixes: the two columns in full are 1530 bytes in utf8mb3, past MyISAM's
  1000-byte key limit; codes and code-system names are far shorter than the prefixes.
- `EhrCodeTableStep` (2026111401, "EHR value sets: add the ehrcode table") creates it with `CreateTableIfMissing`. A table of that name
  with another definition stops the step with a message that names the differences (`GetDefinitionError`, through
  `SchemaVerifier.Compare` of that one table) and changes nothing. No row is written. 2026111402 to 2026111410 are kept for the EHR packages.
  It is above every level on `main` (the highest was 2026110801).
- `EhrCode`: `[CrudTable(IsMissingInGeneral=true)]` became `[CrudTable(HasBatchWriteMethods=true)]` (the import's `InsertMany`), `IsInDb`
  is `[CrudColumn(IsNotDbColumn=true)]`, `Description` is `IsText`. `EhrCodeCrud.cs` regenerated with `CrudGenerator.exe crud=EhrCode`
  (headless, `xCrudGenerator\bin\Debug`): no `IsInDb`, the description as a parameter, `InsertMany`.
- `SchemaVerifier.ArrayHqOnlyTables`: `ehrcode` left the list (53 tables now; the comment says why), so it is excepted once, as a fork
  table. README: the steps table row and "The 53 Open Dental HQ-only tables". `allowed-differences.adoption.json`: one `TableAdded` rule
  (adoption leaves it empty; `AdoptionRowCheck` lists fork tables from `ForkTables` and requires them empty).

## 2. Loading

- `EhrCodes.Listt` reads the table the first time it is asked for (`GetAllFromDb`: `Meth.GetObject` on a Middle Tier client, a `SELECT`
  elsewhere), then works out `IsInDb` (`updateCodeExistsHelper`, as before). A database the step has not run on (a tool's or a test's) has
  no table: an empty list, as before EHR-2. `UpdateList` reads the table again, then works out `IsInDb`. `ClearList` drops the list.
- `InvalidType.EhrCodes`: on a direct connection and on the Middle Tier server `Cache.GetCacheDs` calls `UpdateList` (unchanged). A Middle
  Tier client runs `GetCacheDs` on the server and then its own `FillCache`, which now has an `EhrCodes` branch: `EhrCodes.ClearList()`, so
  the next read of `Listt` fetches the new rows through `Meth` (and works out `IsInDb` from the server's counts).
- The thresholds and counts behind `IsInDb` are now `GetCodeCountMinimum` and `GetCodeCountInDb` (the same numbers and tables as before:
  CDCREC 5, CDT 10, CPT 300, CVX 5, HCPCS 20, ICD9CM 1500, ICD10CM 2000, LOINC 20, RXNORM 40, SNOMEDCT 700, SOP 100; a table counts when
  it has more rows; AdministrativeSex always; a code system without a table never). Each table is counted once per load.
- **Load time** (`ValueSetDbTests.LoadTime_AFullSyntheticRelease_LoadsInUnderASecond`, on the ehr2_ scratch database built from the seed
  file, 60,000 synthetic rows: a third in the 79 manifest value sets, the rest in 400 other value sets, more than the June 2013 EP release
  holds for all its measures): first load (`Listt`) 475, 549 and 591 ms in three runs, `UpdateList` 449, 542 and 556 ms, one value-set
  lookup 4-9 ms; reading and checking the 32 MB workbook 1.5-2.1 s, the import 3.8-4.7 s. **Under 1 second, so no background load comes back**; the
  test fails if the first load passes 1 second.

## 3. The import (section 8.2)

- `EhrValueSetImport` (a second class at the end of `EhrCodes.cs`, with its small data classes `EhrValueSetFile`, `EhrValueSetRelease`,
  `EhrValueSetManifestItem` and `EhrCodeSystemStatus`).
- `Parse(fileName,bytes)` reads the whole file first and changes nothing: a workbook (`.xlsx`; also a workbook named `.zip`, found by its
  `xl/workbook.xml`), an SVS file (`.xml`), or a `.zip` of them. The workbook is read with `ZipArchive` and the sheet XML (shared strings,
  rich-text runs without their phonetic runs, inline strings, numbers as stored; cells placed by their reference), each sheet's header found
  and matched by name (section 0). XML is read with DTDs prohibited and no resolver, one record at a time. Limits: 512 MB per file and per
  uncompressed part (counted as it is read), 2 GB in all, 20,000 zip entries, 5,000,000 rows. A damaged file is refused with the reason.
- Rows without a value-set OID or a code are skipped and counted. A value set listed under several measures (the package sorted by CMS ID
  repeats it) is one row per value set, code system and code, with every measure that lists it (`MeasureIds` as `EhrCode` writes them,
  "68v3", the nine in the engine's order first). A row with a value over 255 characters (other than the description) is not imported, and
  the report says how many.
- **Code systems** by their HL7 OID, as the engine and the `codesystem` table use them: SNOMEDCT `.6.96`, ICD9CM `.6.103` and `.6.104`,
  ICD10CM `.6.90`, LOINC `.6.1`, RXNORM `.6.88`, CPT `.6.12`, CDT `.6.13`, HCPCS `.6.285`, CVX `.12.292`, CDCREC `.6.238`, SOP
  `.3.221.5`, AdministrativeSex `.5.1` and `.18.2`. A code system the list does not know (for example ICD-10-PCS `.6.4`) keeps the file's
  name without spaces; it is imported and never counts as in the database. A name without an OID gets the code list's OID.
- `QDMCategory`, `MeasureIds` and the value-set name come from the file when it has them, otherwise from the manifest (section 5).
- **The version check.** When the file names measures: each of the nine missing, or at another version, gives a warning that names it (for
  example "The file has CMS74v4 where this program reports CMS74v3."); a file with none of the nine at the right version is refused, naming
  what it has and the release this program needs. When it names none (an SVS file without `Group` elements): its value-set OIDs are checked
  against the manifest; a file with none of them is refused, and the report says that the measure versions could not be checked.
- **The report** (`GetReport`): file name and size, SHA-256, the parts read and skipped, the rows read, skipped and merged, the codes and
  value sets, "Value sets the measures use, found: N of 79" and the missing ones by OID, name and measures, the measures at the right
  version, the code systems without a table, the value-set versions in the file, the warnings.
- `Import(file)` refuses under the Middle Tier (as the CPT import does), then checks Setup without a message (the standard reason; also
  with no user), then refuses a refused file. Otherwise it deletes every row of `ehrcode`, writes the file's rows (`InsertMany`), counts
  them, and records the release in the fork-owned preference row `ForkEhrValueSetRelease` (`PrefC.UpsertRaw`, JSON: label, file name,
  SHA-256, UTC date, codes, value sets, manifest value sets found, the measure check) and one Setup security log entry ("EHR value sets
  imported (Code System Importer): file '...', SHA-256 ..., N codes in M value sets, K of the 79 value sets the measures use; 9 of 9
  measures at the versions this program reports. Every earlier value-set code was replaced."; no code is logged). MyISAM has no
  transactions: a failure while writing returns the reason ("Import the file again"), and the release is not recorded.
- **The window** (`FormCodeSystemsImport`, now titled "Code System Importer"): a row "eCQM value sets (VSAC, June 2013 EP release)" next to
  CPT, with the last import's date and file, and Import Value Sets: refused under the Middle Tier; Setup checked with the standard message;
  the licence prompt (the office's own download under its own UMLS license; CPT of the AMA, CDT of the ADA and SNOMED CT under the UMLS
  terms, for the office's own use; nothing is shipped); the file chosen; read and checked in a progress window; the report shown (copy and
  paste); for a file that is not refused, a confirmation ("Replace every value-set code in the database with the N codes of this file?");
  the import in a progress window; then `DataValid.SetInvalid(InvalidType.EhrCodes,InvalidType.Prefs)`, which reads the codes again here
  and tells every other workstation and the Middle Tier server. Both buttons are enabled only with Setup; the CPT import now checks Setup
  in its click as well (section 5.1). The setup window already hides the Code System Importer button without Setup (EHR-1).

## 4. Code-system status (step 4)

A Code-System Status grid in the same window, from `EhrCodes.GetCodeSystemStatuses()`: each code system the list knows, then each other
code system of the imported value sets, with its rows in the database, the minimum expected (the threshold plus one, the first count that
counts), the value-set codes of that system, and the status: "Loaded", "Load this code system's table directly", "No table needed"
(AdministrativeSex) or "No table in this program: these codes are never counted as in the database". A label above it says that a code
counts only when its code system's table is loaded, and that the tables are loaded directly (E-17). It refreshes after each import.

## 5. The manifest

`EhrValueSetImport._arrayManifest`: the 79 value-set OIDs the program's source names (78 under `2.16.840.1.113883.3`, in
`QualityMeasures.cs`, `DiseaseDefs.cs` and the EHR windows, without the legacy OID root of `OIDInternals` and the SOP code system
`2.16.840.1.113883.3.221.5`; plus the PHDSC Payer value set `2.16.840.1.114222.4.11.3591` of `QualityMeasures.cs:3240`), each with:

- **the name** the source gives beside the OID, without "Grouping Value Set" and the code-system words ("BMI LOINC Value", "Above Normal
  Medications"); "Tobacco Use Cessation Counseling" (`.526.3.509`), which the source names only through the window's "TobaccoCessation
  Counseling", "Systolic Blood Pressure" and "Diastolic Blood Pressure" from the QRDA comments, and "Payer";
- **the QDM category** from how the engine uses the value set (encounters, problems, procedures, medications, interventions, physical
  exams, attributes for the reasons, Individual Characteristic for tobacco user and payer, Risk Category/Assessment for tobacco screening).
  **Not checked against CMS's specifications** (they were not available here): the lead checks them against the June 2013 EP package of
  download batch E. They matter only for a file without categories (an SVS file without Groups); nothing in the program reads
  `QDMCategory` today except one method that is commented out;
- **the measures** whose sections of `QualityMeasures.GetEhrCqmData` and `ClassifyPatients` name the OID (the Payer value set: all nine,
  as a supplemental data element).

Tests hold it to the source: the nine measure IDs equal `GetEMeasureNum` and `GetEMeasureVersion` read by reflection (so
`QualityMeasures.cs` is not edited), and its OIDs equal every value-set OID in the program's string literals. The two surveys counted 79 and
81; this count is 79. The OIDs and the measures are facts of the public specifications and of the source; the names come from the source;
no value-set member is in the program (section 8.3).

## 6. Deviations and choices the plan left open

- **Every value set of the file is imported**, not only the manifest's 79: the release sorted by CMS ID holds the value sets of every EP
  measure, and the lookups by code (`GetMeasureIdsForCode`, `GetForEventTypeByUse`, `GetValueSetOIDsForCode`) use any of them; the report
  counts the others. The 60,000-row load time above covers this.
- **A sheet without a header row is skipped, not refused** (a "Read Me" sheet); a header row that lacks a required column refuses the
  file, naming the sheet and the missing columns. A workbook with no value-set sheet at all is refused.
- **The window's title** changed from "Import CPT Codes" to "Code System Importer" (the name of the setup button that opens it), since it
  imports both.
- **No Meth `Insert` for `EhrCodes`** (the commented-out stubs stay commented out): the import writes with `InsertMany` on a direct
  connection only, so `MiddleTierRoundTripTests` (which no longer skips the type now that `IsMissingInGeneral` is gone) finds no insert
  method of it to call.
- **Release label.** The row records `VSAC June 2013 EP Only Release eCQM Value Sets (MU2 EP Update 2013-06-14)` and, separately, how many
  of the nine measures were at the right version (or that they could not be checked). A file does not carry a release label of its own.
- **`Cache.ClearCache`'s EhrCodes branch** (`Cache.cs:977`, "(skipped)") is left as it was; the plan names only `FillCache`.
- **Tests' own zip writer.** `tests/EhrTests` does not reference `System.IO.Compression` and this package may not edit its project file, so
  the synthetic workbooks and zips are written by `Support/ValueSetFixtures.cs`' `StoredZip` (stored entries with their CRC-32); the
  importer reads them with `ZipArchive` as any zip.
- **A test that section 6 asks for, written here:** `Fixtures_HoldNoLiteralOfACodeFromTheEnginesLists` takes the codes of the engine's own
  lists from `EhrMeasures.cs` and `QualityMeasures.cs` at run time (numbers of five to nine digits, CDT codes) and fails on any of them in a
  string literal of `tests/EhrTests` (or its golden files). The fixture builders also refuse any code that does not start with TEST.

## 7. Tests

`tests/EhrTests`, 42 new tests (171 in all), in new files only (the project file is not edited; the SDK-style project compiles them):

- `ValueSetParseTests` (19, no database): `Workbook_ReadsTheRows_UnderATitle_WithTheirFieldsAndCodeSystems`,
  `Workbook_HeadersMatch_IgnoringCaseAndSpaces_InAnyOrder_AndInlineStrings`, `Workbook_AValueSetUnderSeveralMeasures_IsOneRowWithEveryMeasure`,
  `Workbook_ASheetWithoutAHeaderRow_IsSkippedAndNamed`, `Workbook_AHeaderRowWithoutTheRequiredColumns_IsRefused_NamingThem`,
  `Workbook_RowsWithoutAnOidOrACode_AreSkippedAndCounted`, `Workbook_AValueLongerThanItsColumn_IsNotImported_AndWarned_ADescriptionMayBeLong`,
  `Svs_WithGroups_ReadsTheConcepts_TheCategoryAndTheMeasures`,
  `Svs_WithoutGroups_Imports_WithTheManifestsMeasures_AndSaysTheVersionsWereNotChecked`,
  `Svs_WithoutGroups_NoneOfWhoseOidsIsInTheManifest_IsRefused`, `Versions_TheJune2013Release_HasAllNine_AndEveryManifestValueSet`,
  `Versions_AMeasureAtThe2014UpdatesVersion_GivesTheVersionWarning` (CMS74v4), `Versions_TheWhole2014Update_IsRefused_NoneOfTheNineAtItsVersion`
  (workbook and SVS), `ParseMeasureIds_ReadsCmsIdsAndShortIds_IgnoresOtherText`, `CodeSystems_ByTheirHl7Oids_AsTheEngineNamesThem` (and the
  `codesystem` rows of the convert script agree), `CodeSystems_WithoutATable_AreImportedAndNamedInTheReport`,
  `Zip_OfAWorkbookAndAnSvsFile_ReadsBoth_AndSkipsTheRest`, `RefusedFiles_EmptyUnreadableNotSvsAndXmlWithADtd`,
  `Report_NamesTheFileItsHashAndTheCounts`.
- `ValueSetDbTests` (13, on the scratch database `ehr2_<UTC start>_<process>_<random>_main`, built from `db/init/new-database.sql` and the
  fork migrations, plus an `..._empty` one for the step; both dropped; leftovers older than 12 hours dropped first, only names starting with
  `ehr2_`): `Step_OnTheScratchDatabase_CreatedTheTableOnce_AsDeclared_AndTheSchemaStillMatchesOpenDental` (the change is the step's, the
  live `SHOW CREATE TABLE` equals the declaration, `ForkTables.VerifyDatabase` clean, the schema verifier matches with `ehrcode` excepted
  once), `Step_OnAnAlreadyMigratedDatabase_ChangesNothing_AndKeepsTheRows`,
  `Step_OnAnEmptyDatabase_CreatesTheEmptyTable_ThenChangesNothing_AndATableOfAnotherDefinitionStopsIt`,
  `Import_ReplacesEveryRow_RecordsTheRelease_AndLogsSetup`, `Import_ARefusedFile_LeavesTheOldRowsAndTheRelease`,
  `Import_WithoutSetup_IsRefused_NothingWrittenOrLogged` (a user without Setup, and no user), `Import_ThroughTheMiddleTier_IsRefused_AsTheCptImportIs`
  (and the window's order: Middle Tier, Setup, licence prompt, file, import, signal; the CPT click checks Setup),
  `Listt_ReadsTheTable_AndUpdateListReadsItAgain`, `IsInDb_FollowsTheCodeTablesThresholds_AndTheStatusShowsThem` (700 SNOMED CT rows not
  enough, 701 enough, 41 RxNorm enough, AdministrativeSex always, ICD10PCS never; the status rows and texts),
  `TobaccoLists_FillFromTheImportedValueSets_WhenTheCodeTablesAreLoaded` (the five value sets of the tobacco tab and `FormEhrPatientSmoking`,
  and both windows' calls), `Pregnancy_DiseaseDefsFindsAProblemWhoseCodeIsInBothPregnancyValueSets` (`DiseaseDefs.GetAllPregDiseaseDefs`:
  a code in both sets found, one in one set not, a SNOMED CT code without its table not),
  `MiddleTier_AClientReadsTheListThroughTheServer_AndInvalidTypeEhrCodesBringsTheNewRows` (`OpenDentalServerMockIIS`, a Setup user with a
  known password: `GetAllFromDb` and `Listt` through the server with `IsInDb`, a stale client list kept until `Cache.FillCache` with
  `InvalidType.EhrCodes` drops it, the full `Cache.Refresh`), `LoadTime_AFullSyntheticRelease_LoadsInUnderASecond`.
- `ValueSetSourceTests` (10, no database): `Manifest_TheNineMeasures_AreTheVersionsTheEngineReports`,
  `Manifest_ItsOids_AreExactlyTheValueSetOidsTheSourceNames`, `Manifest_EachItemHasANameACategoryAndOnlyTheNineMeasures_Once`,
  `Step_HasTheFirstEhrLevel_TheRangeIsKept_AndTheLevelsAreValid`, `Table_IsAForkTable_DeclaredOnce_AndNoLongerHqOnly`,
  `TableClass_MapsTheDeclarationColumnByColumn_IsInDbIsNotAColumn`, `AdoptionRule_Readme_AndTheVerifiersList_NameTheTable`,
  `StaleComments_NameTheImport_NotEhrDll`, `Fixtures_HoldNoLiteralOfACodeFromTheEnginesLists`, `Fixtures_RefuseACodeThatIsNotATestCode`.

Synthetic files only (`Support/ValueSetFixtures.cs`): codes `TEST...`, the real value-set OIDs (from the manifest) and code-system OIDs
(from the code list), made-up value sets under the made-up arc `2.16.840.1.113883.3.999999`, made-up measures `CMS1000v3` and up.

Targeted runs (`run-tests.ps1 -Mode Targeted`, after building the test projects through the lane):

| Assembly | Result |
|---|---|
| EhrTests | 171 of 171 passed |
| ForkMigrationTests | 37 passed, 2 skipped (they need the schema-only dump, `FORKMIGRATION_REFERENCE_DUMP`), **1 failed as expected**: `InitSqlRegenerationTests.Generator_WritesExactlyTheCommittedFile` ("Committed: ... 457 tables ... fork schema level 2026110801"; "Generated: ... 458 tables ... fork schema level 2026111401"), because `db/init/new-database.sql` is not regenerated in this package (the lead regenerates it after the merge). An earlier run passed it only because `tools/DevDbTool` had not been rebuilt with this package yet |
| M201Tests | 24 passed, 2 skipped (they need Open Dental's published documents, `M201_OD_DOCS_DIR`), 0 failed |
| P262SchemaTests | 24 passed, 4 skipped (`P262_OD_DOCS_DIR`), 0 failed |
| SyntheticData.Tests | 57 passed, 1 skipped (`RealDump_Has422Tables`, the real dump), 0 failed |
| MiddleTierMockTests | 125 of 125 passed (the Middle Tier rules read the new methods' IL) |
| NoHqTests | 57 of 57 passed |

The table is the run after the full build (`ehr2-run3.txt`). The skips are the same environment skips as on `main`. InitSqlTests was not run and `db/init/new-database.sql` was not regenerated (the
lead does both after the merge). Running EhrTests also builds and drops the EHR-1 (`ehr1_`) and EHR-3 (`ehr3_`) scratch databases of the
existing tests; after the runs no `ehr` database was left on the server.

**The DevDbTool schema check** (`SchemaCheck.cs:127` now covers `ehrcode`): `tools\DevDbTool\bin\Debug\DevDbTool.exe --database
ehr2_devdb_261115012137` (seed dump, convert scripts, adoption, every fork migration: "32 step(s) run ... Fork schema level 2026111401",
"The schema matches Open Dental 26.2.0"), then `--verify-only` on the same database: "Checked 443 tables and 4612 columns (52 HQ-only table
types skipped). Verification passed: adopted database at fork schema level 2026111401, schema of Open Dental 26.2.0." Exit code 0 both
times. Before EHR-2 `ehrcode` was one of the skipped HQ-only types. The database was dropped afterwards.

## 8. For the lead

- **Init file.** Regenerate `db/init/new-database.sql` after the merge (it gains the empty `ehrcode` table and the 2026111401 level) and run
  InitSqlTests, and ForkMigrationTests' `Generator_WritesExactlyTheCommittedFile`, which fails until then (section 7). Until then a database
  made from the committed file gets the table from the step, as the tests' scratch databases do.
- **Release notes.** Written into `docs/release-notes/next.md`: a line in the summary, "14 tables" (one for the EHR value sets), and the
  section "EHR: the value sets from your own VSAC download".
- **Launch checks (section 11), on a test database:**
  1. Setup > Chart > EHR > Code System Importer as a user without Setup: the button is hidden in the EHR setup window (EHR-1); opened
     another way, both import buttons are disabled.
  2. As a user with Setup: the window shows the value-set row ("not imported") and the Code-System Status grid; Import Value Sets shows the
     licence prompt, then the file dialog.
  3. A synthetic workbook (one can be written with `ValueSetFixtures.MakeWorkbook` from a test) imports: the report, the confirmation, "Import
     complete", the row shows the date and file, the status grid's value-set codes column fills, the security log has the Setup entry.
  4. With SNOMED CT rows loaded (more than 700): the medical window's Tobacco Use tab is enabled and its lists show the imported codes.
  5. A file of the 2014 update (measure IDs `CMS..v3`/`v4`): refused, nothing changes.
  6. Through the Middle Tier: Import Value Sets says the import cannot run there; after an import on a direct workstation, a Middle Tier
     workstation's tobacco lists show the new codes without a restart.
- **Owner and counsel questions** (below).

### Owner questions

1. **Step 5 (E-7):** can someone with a UMLS licence open VSAC's archived June 2013 EP release once and record, in this note, the file
   names and formats, each workbook's sheet and header names, how code cells are stored, and one SVS file's element and Group names
   (section 0)? Nothing from it enters the repository. The readers' names are provisional until then.
2. **The manifest's names and QDM categories** (section 5) come from the source's comments and the engine's use of each value set. Should
   the lead check them against CMS's "2013 June eCQM Specifications for Eligible Professionals" (download batch E, approved but not in
   `batch-e-20261109`) before the bridge release?
