# Port note: EHR-4 (the Measure Calc window; EHR-4b adds the measure fixtures)

## EHR-4a: the Measure Calc window

Package EHR-4a of `docs/specs/ehr-rebuild-plan.md` (section 7, sections 9.1 and 9.2), on branch `ehr/ehr4a-1` from `main` `2e5a61a67`.
The owner answered E-1 to E-19 as recommended on 2026-11-08; this package carries out E-1 (the window and its printout say "for internal
review only; this program is not certified; these numbers cannot be used to attest or to submit"), E-4 (the window opens from the EHR window
with no gate of its own, as in Open Dental), E-16 (the 2015 alternate exclusions are not modelled, and the window says so) and E-19 (below).
No schema change, no new `PrefName`, no new third-party file, no licensed content, and no project file edited (the program's and the test
project's default globs pick the new files up). Synthetic data only.

**Public basis.** The CMS EHR Incentive Program specification sheets for eligible professionals (Stage 1 for 2014, Stage 2, Modified Stage 2
for 2015 through 2017), the 2015-2017 final rule (80 FR 62762) for the year rule of section 9.2, and Open Dental's public manual page of the
Measure Calc window for what the window shows. Nothing was decompiled; `EHR.dll` was never looked at. What the old window did is known only
from the code that called it (`8c1703bdc^`: `EHR.FormEhrMeasures`, its `SelectedPatNum`, and the `PatientSelect` result of the EHR window).

### 1. The engine (`OpenPmsDentalBusiness/Data Interface/EhrMeasures.cs`)

| Plan item | Built |
|---|---|
| The bodies of the three `SelectAll*` move, unchanged, into a per-measure method safe on the Middle Tier | `GetMeasuresToCalc(stage)` holds `SelectAll`'s query and `SelectAllMu2`'s and `SelectAllMu2Mod`'s `GetMU2List`/`GetMU2ModList` plus their `RemoveAll`, word for word. `CalcMeasure(stage,ehrMeasure,dateStart,dateEnd,provNum)` holds the loop body (objective, measure, threshold, table, numerator and denominator, the three explanations, the exclusion count and its description, the elapsed time), the same calls in the same order for each stage. Both are Middle Tier methods (`Meth.GetObject`); `CalcMeasure` fills in the row it is given and returns it, or a copy over the Middle Tier. `SelectAll`, `SelectAllMu2` and `SelectAllMu2Mod` keep their signatures and their Middle Tier check and now call the two, so their results do not change (a test compares every field) |
| A public list of the measure types of each stage | `GetStageMeasureTypes(stage)`: 20 types for Stage 1 (`SelectAll`'s `ORDER BY MeasureType`), 22 for Stage 2 and 14 for Modified Stage 2 (the `ORDER BY FIELD` lists less the removed types). A test checks it against the rows `GetMeasuresToCalc` returns on a database built from `db/init/new-database.sql` |
| The engine guard | The five places that build the provider list (`GetTable`, `GetExclusionCount`, `GetTableMu2`, `GetExclusionCountMu2`, `GetTableMu2Mod`) now read `string provs=GetProvsOrNone(Db.GetScalar(command));`. An empty list (the provider's names have no `ehrprovkey` row) becomes **`-1`**, so the 107 `IN()` queries stay valid and match nothing. See section 3 for why not `0` |
| The work-table helper, on the server through `Meth` | `DropOldWorkTables(maxAgeSeconds)` (Middle Tier, `Meth.GetObject<List<string>>`) lists this database's `tempehrmeasure%` and `tempehrquality%` tables with `TIMESTAMPDIFF(SECOND,CREATE_TIME,NOW())` from `information_schema.TABLES` (`TABLE_SCHEMA=DATABASE()`, the server's own clock), drops each that `IsOldWorkTable` accepts and returns their names. `IsOldWorkTable(name,ageSeconds,maxAgeSeconds)` is the pure rule: the name is `temp` + `ehrmeasure` or `ehrquality` + letters, digits and underscores only (safe between backticks), the age is known and at least the limit. `WORK_TABLE_MAX_AGE_SECONDS` is 86400 (one day, not "older than this run"). EHR-5 calls it the same way |
| For the detail and the provider list | `GetAuditTable(stage,...)` returns `GetTable`, `GetTableMu2` or `GetTableMu2Mod` (each a Middle Tier method of its own). `GetReportingProviders()` (Middle Tier, `Meth.GetTable`) returns `ProvNum` and `KeyGroup` (the smallest `EhrProvKeyNum` the provider's names match) for every provider with a row, by the same `LName`/`FName` join as the engine, so the list groups providers exactly as the engine counts them (the collation: case-insensitive, trailing spaces ignored) |
| The stage | `EhrMeasureStage` (Stage1, Stage2, ModifiedStage2), an enum at the end of the file. It is never stored, so `docs/port-notes/FORK-ENUMS.md` does not apply |
| E-19 | `EhrMeasures.Update` and the `ehrmeasure` table's `Numerator` and `Denominator` columns stay, unused: nothing in the window or the engine writes them (a test reads them back as `-1/-1` after every stage was calculated). The table's rows stay the list of measures each stage calculates |

The file holds one byte that is not UTF-8 (`0x92`, a Windows-1252 apostrophe in a Stage 1 measure text, unchanged since Open Dental). The
edits were applied byte for byte, so that byte and the CRLF line ends are as they were.

### 2. The window (`OpenPmsDental/Forms/FormEhrMeasureCalc.*`, `FormEhrMeasureCalcDetail.*`, `FormEHR.cs`)

- **Opening it.** `FormEHR.butMeasures_Click` (the plan's lines 526-529) no longer says "not available in this build": it opens
  `FormEhrMeasureCalc`, and when that closes with OK and a `SelectedPatNum`, sets `PatNum`, `EhrFormResultOnClose=PatientSelect` and closes
  with OK, as the old reflection call did; the Chart switches (`ControlChart.cs`, unchanged). Otherwise it refills its grid. No permission
  check of its own (E-4).
- **Top of the window** (section 9.1). Provider: one entry per `KeyGroup`, "Last, First (Abbr1, Abbr2)"; the number passed to the engine
  is the logged-on user's provider when it is one of the entry's, otherwise the first that is not hidden; the entry of the user's provider
  is selected. With no row at all the window says where to add one and Refresh stays off. Dates: 1 January to 31 December of this year.
  Stage: `GetDefaultStage(provider.EhrMuStage,PrefName.MeaningfulUseTwo)`, the rule of `FormEHR.FillGridMu` (1, 2, 3 give Stage 1, Stage 2,
  Modified Stage 2; any other value the preference: 1 Stage 2, 2 Modified Stage 2, otherwise Stage 1), set again when the provider changes.
  Refresh, Cancel, Print, Close, and the internal-review line in red under them.
- **Refresh.** Checks the dates (valid, both given, end not before start) and the provider, writes the period notes, then starts one
  `ODThread` that runs `FormEhrMeasureCalc.RunCalculation`: `DropOldWorkTables(WORK_TABLE_MAX_AGE_SECONDS)`, `GetMeasuresToCalc(stage)`,
  then for each row, if the cancel flag is not set, `CalcMeasure`. Each finished measure is added to the grid (`InvokeIfNotDisposed`), and
  the progress line names the measure being calculated. Cancel sets a `volatile` flag that the loop reads before each measure; there is no
  `ProgressWin`, no `Join` and no `Thread.Abort`. Closing the window sets the flag too; the thread ends after its measure and its results go
  nowhere. An error shows through `FriendlyException`.
- **Grid.** Measure (`MeasureTypeTitleHelper` for Modified Stage 2, the type's name otherwise, as `FormEHR` does), Numerator, Denominator,
  Percent, Requirement, Exclusion Count. Green (178,255,178) when met. The rules are pure functions of `EhrMeasureCalcRules`, a second class
  in `FormEhrMeasureCalc.cs` (with `EhrMeasureRequirement`, `EhrMeasureRequirementKind` and `EhrMeasureCalcProvider`):

| Rule | Built |
|---|---|
| Met, "more than this percent" | denominator above 0 and numerator × 100 > threshold × denominator, in whole numbers (64-bit) |
| Modified Stage 2 by year (the year of the start date) | `ElectronicCopy` (patient electronic access, measure 2): at least 1 patient in 2015 and 2016. `SecureMessaging`: a yes/no attestation in 2015 ("Yes/no: the capability was fully enabled"; Percent "attestation, not calculated", never green), at least 1 patient in 2016. Every other measure, stage and year: the engine's threshold ("More than N%"); 2017 and later are the engine's 5% for both |
| Period notes | Modified Stage 2 across two calendar years: a warning naming the year whose requirements are shown; Modified Stage 2 before 2015: it began in 2015 and the 2017 requirements are shown; Modified Stage 2 always: the 2015 alternate exclusions are not modelled (E-16); Stage 2 and Modified Stage 2: the two portal measures |
| The removed portal | `ElectronicCopy` and `SecureMessaging` in Stage 2 and Modified Stage 2 show "depends on the removed patient portal" in Percent when not met, instead of a 0%. When old data still meets one, it shows its percent and is green |
| −1 | Numerator and denominator show "not calculated", and so does Percent; an exclusion count of −1 (no count applies) is blank |
| Percent | "no patients in the denominator" for 0; otherwise the percent with at most one decimal, rounded down (2 of 3 is "66.6%"), so a row never shows more than it has |

- **Detail** (double-click a row, `FormEhrMeasureCalcDetail`). Measure name, provider, period and stage, Objective, Measure, the numerator,
  denominator and exclusion explanations, the exclusion count with its description ("No count applies to this measure." for −1), the
  numbers with the requirement, and the audit grid from `GetAuditTable` (PatNum, Patient, Explanation, Met "X"; the engine's columns,
  `EhrMeasures.cs` `GetTable`). Double-clicking a patient sets `SelectedPatNum` and closes with OK, as `FormEhrQualityMeasureEdit2014` does;
  Measure Calc then passes it up with OK (and stops a running refresh).
- **Print.** `PrinterL.TryPrintOrDebugRpPreview` with a forced preview (landscape): the title, provider, period, stage, the internal-review
  line, then the grid through `GridOD.PrintPage`.
- **Texts.** The window's own labels and messages go through `Lan`; the rule texts and the engine's texts are English, as the engine's are.
- **Rules of 3a, kept in advance** (section 6): no WinForms `ContextMenu`, `MenuItem` or `DataGrid`, no process, no `Thread.Abort`, no
  `Encoding.Default` or `GetEncoding`; numbers machine-read nowhere. The `.resx` files are the empty template of the other new windows (no
  resources).

### 3. Deviations and choices the plan left open

- **The guard is `-1`, not the plan's `0`.** The plan says an empty list "becomes `0`, which matches nothing". It does not: rows with no
  provider hold `ProvNum` 0. A medication entered in the medical window has `medicationpat.ProvNum` 0 and `MedicationNum` 0, so with `0`
  Stage 2's `CPOE_MedOrdersOnly` counted it for a provider with no row. Recorded check: with the guard changed to `0`,
  `ProviderWithNoKeyRow_NoSqlError_DenominatorZero` failed ("Expected:<0>. Actual:<1>. CPOE_MedOrdersOnly"); with `-1` it passes. No
  provider column holds −1. The window lists only providers with a row, so the guard matters only for other callers and stale names.
- **New business methods the plan did not name**: `GetMeasuresToCalc` (the plan's "public list" needed the rows, which carry
  `EhrMeasureNum`), `GetAuditTable`, `GetReportingProviders` (so the window groups providers by the engine's own join, not by a C# name
  comparison) and `IsOldWorkTable` (the helper's rule, testable without a day's wait). All at the end of `EhrMeasures.cs` except the moved
  bodies.
- **The reporting year is the start date's year.** Section 9.2 says "by the year of the reporting period"; a period across two years is
  warned about and uses the start year.
- **A met portal row shows its percent** (old data from before the portal was removed); only an unmet one is marked.
- **Print opens a preview** (`isForcedPreview`), as the launch check expects; the preview's own button prints.
- **Observation for the lead (not changed).** `ElectronicCopyAccess` (patient electronic access, measure 1: timely online access provided)
  counts `EhrMeasureEventType.OnlineAccessProvided` events in all three stages. No code in this build writes that event any more (a search
  of the program and the business library finds it only where the engine reads it; Measure Events edits existing events and adds none), so
  that numerator is 0 unless an older database has the events. The plan marks two measures as
  portal measures; whether this third one should be marked too is a question for the owner (below).

### 4. Tests

`tests/EhrTests/MeasureCalcTests.cs` (18 tests) and its fixture builder `tests/EhrTests/Support/MeasureCalcFixture.cs`. The database tests
share one scratch database on the local MariaDB (127.0.0.1:3306): `ehr4a_<UTC start>_<process>_<random>_main`, built by the first test that
needs it from `db/init/new-database.sql` (statement by statement) and the fork migrations, plus an empty `..._other` for the "another
database" check; both dropped by the class cleanup; leftovers older than 12 hours are dropped first, only names starting with `ehr4a_`.
`EHR_TEST_DB=skip` skips (inconclusive), `EHR_KEEP_DB=1` keeps them. The fixture: a provider with an `ehrprovkey` row and a second provider
record with the same names, a provider with no row, a made-up procedure code `TESTEX1`, four patients seen by the first two in 2016 and one
by the third, a problem definition with the made-up code `TEST0001` on two of them, a medication order by the keyed provider, and a
medication order with no provider (ProvNum 0) for the third provider's patient. Made-up names; no code of any licensed system is written.

The test cases:

| # | Test | What it checks |
|---|---|---|
| TC-1 | `DefaultStage_FollowsTheEhrWindow_ProviderStageFirst_ThenTheGlobalSetting` | The stage default for provider stages 0-3 and the preference 0, 1, 2 and an unknown value |
| TC-2 | `Requirement_ForEachStageMeasureAndYear_FollowsThe2015To2017Rule` | Section 9.2 for every stage, every measure of `GetStageMeasureTypes` and every year 2013-2019 (392 cases); the requirement texts; Stage 2 unaffected; the reporting year |
| TC-3 | `IsMet_MoreThanTheThreshold_AtLeastOnePatient_NeverAnAttestation` | 75% met against 50, 50% not, 0/0 and −1 not, whole-number boundaries, 5% not more than 5%, at least one patient, an attestation never; the green colour |
| TC-4 | `ResultText_NotCalculated_Attestation_Portal_NoPatients_Percent` | The Percent column's texts; the portal mark for the four portal rows (none in Stage 1); "75%" for Stage 2 e-prescribing 3 of 4; rounding down; the count texts |
| TC-5 | `PeriodNotes_TwoCalendarYears_AlternateExclusions_Portal` | The two-year warning with the year, the 2015 note, the alternate-exclusions note (E-16), the portal note, none for Stage 1; the date errors |
| TC-6 | `ProviderChoices_OneEntryPerRowName_TheUsersProviderFirst` | Grouping by `KeyGroup`, the text with the abbreviations, the hidden-provider rule, the user's provider, a provider missing from the list, the default entry |
| TC-7 | `PatientPick_OkWithAPatientOnly` | `IsPatientPicked` (OK with a patient only) and `GetAuditPatNum` |
| TC-8 | `IsOldWorkTable_OnlyTheEnginesTables_OnlyWhenOldEnough` | One day exactly; younger, unknown age, unsafe names, other tables and `null` refused; case ignored as the server's `LIKE` does |
| TC-9 | `StageLists_AreTheRowsEachSelectAllCalculates` | `GetMeasuresToCalc` gives `GetStageMeasureTypes`' types in order for each stage, not yet calculated (scratch database) |
| TC-10 | `PerMeasureMethod_GivesWhatEachSelectAllGives` | `RunCalculation` (the window's path) equals `SelectAll`, `SelectAllMu2` and `SelectAllMu2Mod` field by field on the fixture; each audit table matches its numbers; Stage 1 problem list 2 of 4 (both records with the row's names counted); the same result from the other record; the engine's 5% thresholds of the two year-rule measures; E-19 (`ehrmeasure` still `-1/-1`, `Update` stays) |
| TC-11 | `ProviderWithNoKeyRow_NoSqlError_DenominatorZero` | All 56 measures of the three stages for a provider with no row: no SQL error, numerator and denominator 0 (the medication order with no provider not counted); the five guarded lists in the source; the guard's values |
| TC-12 | `Cancel_BetweenMeasures_StopsThere_AndLeavesNoWorkTable` | Cancelled after two measures: two results, the third never started, no work table in the database; cancelled before the first: none |
| TC-13 | `WorkTables_OnlyOldOnes_OfThisDatabase_AreDropped` | A refresh (one day) keeps tables made just now; age 0 drops this database's `tempehrmeasure...` and `tempehrquality...` only; another table and another database's work table stay |
| TC-14 | `MiddleTier_WorkTableHelper_AndOneMeasureAtATime_RunOnTheServer` | Through the Middle Tier mock (`OpenDentalServerMockIIS`, `ClientMT`, a user with a known password): `DropOldWorkTables` (too new, then age 0), a whole Modified Stage 2 `RunCalculation` equal to the direct `SelectAllMu2Mod`, `GetReportingProviders`, `GetAuditTable` |
| TC-15 | `ReportingProviders_TheRecordsWithTheRowsNames_InOneGroup` | Both records with the row's names in one entry, the provider with no row absent, the entry's text |
| TC-16 | `PatientPick_TheAuditPatient_GoesUpToTheEhrWindowAsPatientSelect` | The audit table's patients are the fixture's four; in the source, the chain detail (OK and `SelectedPatNum`) → Measure Calc → `FormEHR` (`PatientSelect`, no new gate, the old message gone) → `ControlChart` |
| TC-17 | `Source_TheWindows_NoMenuNoProcessNoProgressWinNoJoin_OneThreadAndAFlag` | The four window files: no WinForms menu or `DataGrid`, no `Process.Start` or `new Process(`, no `ProgressWin`, `Thread.Abort`, `Encoding.Default`/`GetEncoding` or `Join`; one `ODThread`, the `volatile` flag read in the loop before `CalcMeasure`, the work tables dropped before the loop; the internal-review line on the window, the detail and the printout; `GridOD.PrintPage` |
| TC-18 | `Source_TheEngine_SelectAllCallsThePerMeasureMethod` | Each `SelectAll*` keeps its Middle Tier call and calls `GetMeasuresToCalc` and `CalcMeasure` for its stage; the Middle Tier calls of `CalcMeasure` and `DropOldWorkTables`; the helper stays in its own database |

Targeted runs (`run-tests.ps1 -Mode Targeted`, NoHqTests always included), after the full lane build: see section 6.

### 5. Launch checks (for a person, on a test database; screenshots)

The program was never started for this package. On a test database with EHR on (Setup, Show Features) and synthetic patients:

1. Setup, Chart, EHR, Provider Keys: add a row for a provider. Open a patient's Chart, click EHR, then **Measure Calc**. The window opens
   with that provider selected (or the logged-on user's provider), 1 January to 31 December of this year, the stage of the provider's
   Meaningful Use stage (Edit Provider) or of the global setting, and the red internal-review line.
2. With no provider row at all: the window says to add one in Provider Keys, and Refresh is off.
3. **Refresh**: the progress line names each measure as it is calculated; rows appear one by one; a met row is green, its Percent is more
   than its Requirement. Exclusion counts show where the measure has one.
4. Stage **Modified Stage 2**, period 1/1/2016 to 12/31/2016, Refresh: patient electronic access (VDT) and Secure Messaging show "At least
   1 patient"; with no portal data their Percent says "depends on the removed patient portal". Period 2015: Secure Messaging shows
   "attestation, not calculated". Period 2017: both "More than 5%". The notes say that the 2015 alternate exclusions are not modelled.
5. Period 10/1/2015 to 3/31/2016 with Modified Stage 2: the warning that the period spans two calendar years.
6. Start a refresh on a database with many patients and click **Cancel** at once: it stops after the measure being calculated and the
   progress line says how many were done. Then in a query window: `SHOW TABLES LIKE 'tempehr%'` lists nothing.
7. Close the window during a refresh: it closes at once, and the program stays responsive.
8. Double-click a measure: the detail shows its objective, measure, the three explanations, the exclusion count and its patients with the
   explanation and "X" for met. Double-click a patient: both windows and the EHR window close, and the Chart shows that patient.
9. **Print**: the preview shows the title, provider, period, stage, the internal-review line and the grid; landscape; more pages for a
   long list.
10. Through the Middle Tier (a workstation connected to the Middle Tier server): steps 3, 6 and 8 work the same.

### 5a. For the lead

- **Release notes**: written in `docs/release-notes/next.md` (a bullet in the opening list and the section "EHR: the Meaningful Use
  measures window (Measure Calc)").
- **Project baseline**: the program project gains four `Compile` items and two `EmbeddedResource` items by its default globs, and
  `tests/EhrTests` two `Compile` items; `tools/project-baseline` is re-recorded at the next gate.
- **Owner question**: should `ElectronicCopyAccess` (patient electronic access measure 1) also be marked as depending on the removed portal,
  since nothing writes its `OnlineAccessProvided` events any more (section 3)?
- **For EHR-4b**: the fixture builder and the ehr4a_ database are in `Support/MeasureCalcFixture.cs`; EHR-4b's spec cases can extend it or
  build their own. The engine's Stage 1 thresholds (`GetThreshold`) and Stage 2 thresholds (`GetThresholdMu2`) were not checked against the
  sheets here; that is EHR-4b's deviation list.
- **For EHR-5**: `EhrMeasures.DropOldWorkTables(EhrMeasures.WORK_TABLE_MAX_AGE_SECONDS)` before a quality-measure run.

### 6. Build and test record

(Filled in after the full build.)
