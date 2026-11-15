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
| The work-table helper, on the server through `Meth` | `DropOldWorkTables(maxAgeSeconds)` (Middle Tier, `Meth.GetObject<List<string>>`) lists this database's `tempehrmeasure%` and `tempehrquality%` tables with `TIMESTAMPDIFF(SECOND,CREATE_TIME,NOW())` from `information_schema.TABLES` (`TABLE_SCHEMA=DATABASE()`, the server's own clock), drops each that `IsOldWorkTable` accepts and returns their names. On the server (after the `Meth` call) an age below `WORK_TABLE_MIN_AGE_SECONDS` (3600, one hour) is raised to it, whatever the client sent (lead fix round, item 5); the body is the private `DropWorkTablesOlderThan`. `IsOldWorkTable(name,ageSeconds,maxAgeSeconds)` is the pure rule: the name is `temp` + `ehrmeasure` or `ehrquality` + letters, digits and underscores only (safe between backticks), the age is known and at least the limit. `WORK_TABLE_MAX_AGE_SECONDS` is 86400 (one day, not "older than this run"). EHR-5 calls it the same way |
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
  nowhere. An error shows through `FriendlyException`, and the progress line then says at which measure the refresh stopped, with the
  error (lead fix round, item 3).
- **Grid.** Measure (`MeasureTypeTitleHelper` for Modified Stage 2, the type's name otherwise, as `FormEHR` does), Numerator, Denominator,
  Percent, Requirement, Exclusion Count. Green (178,255,178) when met. The rules are pure functions of `EhrMeasureCalcRules`, a second class
  in `FormEhrMeasureCalc.cs` (with `EhrMeasureRequirement`, `EhrMeasureRequirementKind` and `EhrMeasureCalcProvider`):

| Rule | Built |
|---|---|
| Met, "more than this percent" | denominator above 0 and numerator × 100 > threshold × denominator, in whole numbers (64-bit) |
| Modified Stage 2 by year (the year of the start date) | `ElectronicCopy` (patient electronic access, measure 2): at least 1 patient in 2015 and 2016. `SecureMessaging`: a yes/no attestation in 2015 ("Yes/no: the capability was fully enabled"; Percent "attestation, not calculated", never green), at least 1 patient in 2016. Every other measure, stage and year: the engine's threshold ("More than N%"); 2017 and later are the engine's 5% for both |
| Period notes | Modified Stage 2 across two calendar years or starting before 2015: one statement of the year whose requirements are shown (`GetRequirementsYearShown`: the start year, 2017 before 2015), with the two-year warning and "began in 2015" in it; Modified Stage 2 always: the 2015 alternate exclusions are not modelled (E-16); every stage: the stage's portal rows by the names the grid shows (lead fix round, items 4 and 6) |
| The removed portal | `ElectronicCopyAccess` in all three stages (lead decision, item 6), `ElectronicCopy` and `SecureMessaging` in Stage 2 and Modified Stage 2 show "depends on the removed patient portal" in Percent when not met, instead of a percent. When old data still meets one, it shows its percent and is green |
| −1 | Numerator and denominator show "not calculated", and so does Percent; an exclusion count of −1 (no count applies) is blank |
| Percent | "no patients in the denominator" for 0; otherwise the percent rounded down to one decimal (2 of 3 is "66.6%"), so a row never shows more than it has and an unmet row never reaches its threshold; a met row whose one decimal would show no more than its threshold gets as many decimals as it takes (4001 of 5000 against 80 is "80.02%"; lead fix round, item 1) |

- **Detail** (double-click a row, `FormEhrMeasureCalcDetail`). Measure name, provider, period and stage, Objective, Measure, the numerator,
  denominator and exclusion explanations, the exclusion count with its description ("No count applies to this measure." for −1), the
  numbers with the requirement, and the audit grid from `GetAuditTable` (PatNum, Patient, Explanation, Met "X"; the engine's columns,
  `EhrMeasures.cs` `GetTable`). The table is the one Measure Calc kept from an earlier opening of the measure, otherwise it is fetched on
  an `ODThread` with Cancel, the grid off meanwhile (lead fix round, item 2). Double-clicking a patient sets `SelectedPatNum` and closes with OK, as `FormEhrQualityMeasureEdit2014` does;
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
  Decided by the lead in the lead fix round (owner question 2): `-1` stays, and the plan's EHR-4a text now says so.
- **New business methods the plan did not name**: `GetMeasuresToCalc` (the plan's "public list" needed the rows, which carry
  `EhrMeasureNum`), `GetAuditTable`, `GetReportingProviders` (so the window groups providers by the engine's own join, not by a C# name
  comparison) and `IsOldWorkTable` (the helper's rule, testable without a day's wait). All at the end of `EhrMeasures.cs` except the moved
  bodies.
- **The reporting year is the start date's year.** Section 9.2 says "by the year of the reporting period"; a period across two years is
  warned about and uses the start year.
- **A met portal row shows its percent** (old data from before the portal was removed); only an unmet one is marked.
- **Print opens a preview** (`isForcedPreview`), as the launch check expects; the preview's own button prints.
- **Observation for the lead (decided in the lead fix round, item 6: marked).** `ElectronicCopyAccess` (patient electronic access, measure 1: timely online access provided)
  counts `EhrMeasureEventType.OnlineAccessProvided` events in all three stages. No code in this build writes that event any more (a search
  of the program and the business library finds it only where the engine reads it; Measure Events edits existing events and adds none), so
  that numerator is 0 unless an older database has the events. The plan marks two measures as
  portal measures; whether this third one should be marked too is a question for the owner (below).

### 4. Tests

`tests/EhrTests/MeasureCalcTests.cs` (18 tests; 25 after the lead fix round, section 7) and its fixture builder `tests/EhrTests/Support/MeasureCalcFixture.cs`. The database tests
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
| TC-4 | `ResultText_NotCalculated_Attestation_Portal_NoPatients_Percent` | The Percent column's texts; the portal mark for the portal rows (seven since the lead fix round: `ElectronicCopyAccess` in all three stages, the other two in Stage 2 and Modified Stage 2); "75%" for Stage 2 e-prescribing 3 of 4; rounding down; the count texts |
| TC-5 | `PeriodNotes_TwoCalendarYears_AlternateExclusions_Portal` | The two-year warning with the year, the 2015 note, the alternate-exclusions note (E-16), the portal note (Stage 1's only note, `ElectronicCopyAccess`, since the lead fix round); the date errors |
| TC-6 | `ProviderChoices_OneEntryPerRowName_TheUsersProviderFirst` | Grouping by `KeyGroup`, the text with the abbreviations, the hidden-provider rule, the user's provider, a provider missing from the list, the default entry |
| TC-7 | `PatientPick_OkWithAPatientOnly` | `IsPatientPicked` (OK with a patient only) and `GetAuditPatNum` |
| TC-8 | `IsOldWorkTable_OnlyTheEnginesTables_OnlyWhenOldEnough` | One day exactly; younger, unknown age, unsafe names, other tables and `null` refused; case ignored as the server's `LIKE` does |
| TC-9 | `StageLists_AreTheRowsEachSelectAllCalculates` | `GetMeasuresToCalc` gives `GetStageMeasureTypes`' types in order for each stage, not yet calculated (scratch database) |
| TC-10 | `PerMeasureMethod_GivesWhatEachSelectAllGives` | `RunCalculation` (the window's path) equals `SelectAll`, `SelectAllMu2` and `SelectAllMu2Mod` field by field on the fixture; each audit table matches its numbers; Stage 1 problem list 2 of 4 (both records with the row's names counted); the same result from the other record; the engine's 5% thresholds of the two year-rule measures; E-19 (`ehrmeasure` still `-1/-1`, `Update` stays) |
| TC-11 | `ProviderWithNoKeyRow_NoSqlError_DenominatorZero` | All 56 measures of the three stages for a provider with no row: no SQL error, numerator and denominator 0 (the medication order with no provider not counted); the five guarded lists in the source; the guard's values |
| TC-12 | `Cancel_BetweenMeasures_StopsThere_AndLeavesNoWorkTable` | Cancelled after two measures: two results, the third never started, no work table in the database; cancelled before the first: none |
| TC-13 | `WorkTables_OnlyOldOnes_OfThisDatabase_AreDropped` | A refresh (one day) keeps tables made just now; `DropOldWorkTables(0)` keeps them too (the floor); the rule behind it at age 0 (`DropWorkTablesOlderThan`, through reflection) drops this database's `tempehrmeasure...` and `tempehrquality...` only; another table and another database's work table stay |
| TC-14 | `MiddleTier_WorkTableHelper_AndOneMeasureAtATime_RunOnTheServer` | Through the Middle Tier mock (`OpenDentalServerMockIIS`, `ClientMT`, a user with a known password): `DropOldWorkTables` (too new at one day, and at age 0, which the server raises to its floor), a whole Modified Stage 2 `RunCalculation` equal to the direct `SelectAllMu2Mod`, `GetReportingProviders`, `GetAuditTable` |
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
3. **Refresh**: the progress line names each measure as it is calculated; rows appear one by one. A met row is green and its Percent is
   more than the number of its Requirement: one decimal, rounded down, and more decimals when one would not show it above (4001 of 5000
   against "More than 80%" shows "80.02%", never "80%"). An unmet row's Percent is at or below that number (79.98% shows "79.9%").
   Exclusion counts show where the measure has one. If a measure fails, the error window shows, and afterwards the progress line says
   "Stopped with an error at measure N of M (name); K measures were calculated. Error: ...", never "Done".
4. Stage **Modified Stage 2**, period 1/1/2016 to 12/31/2016, Refresh: patient electronic access (VDT) and Secure Messaging show "At least
   1 patient"; with no portal data their Percent says "depends on the removed patient portal", and so does Electronic Access's (timely
   online access, in every stage). Period 2015: Secure Messaging shows
   "attestation, not calculated". Period 2017: both "More than 5%". The notes say that the 2015 alternate exclusions are not modelled.
5. Period 10/1/2015 to 3/31/2016 with Modified Stage 2: one note warns that the period spans two calendar years and says the 2015
   requirements are shown. Period 10/1/2014 to 3/31/2015: one note with the warning, "began in 2015" and the 2017 requirements. All notes
   show in full above the progress line.
6. Start a refresh on a database with many patients and click **Cancel** at once: it stops after the measure being calculated and the
   progress line says how many were done. Then in a query window: `SHOW TABLES LIKE 'tempehr%'` lists nothing.
7. Close the window during a refresh: it closes at once, and the program stays responsive.
8. Double-click a measure: the detail shows its objective, measure, the three explanations, the exclusion count and its patients with the
   explanation and "X" for met. While the list loads the hint says so, the grid is off and Cancel is on; Cancel stops waiting. Close and
   open the same measure again: the list shows at once. Double-click a patient: both windows and the EHR window close, and the Chart shows that patient.
9. **Print**: the preview shows the title, provider, period, stage, the internal-review line and the grid; landscape; more pages for a
   long list.
10. Through the Middle Tier (a workstation connected to the Middle Tier server): steps 3, 6 and 8 work the same.

### 5a. For the lead

- **Release notes**: written in `docs/release-notes/next.md` (a bullet in the opening list and the section "EHR: the Meaningful Use
  measures window (Measure Calc)").
- **Project baseline**: the program project gains four `Compile` items and two `EmbeddedResource` items by its default globs, and
  `tests/EhrTests` two `Compile` items; `tools/project-baseline` is re-recorded at the next gate.
- **Owner question 1** (`ElectronicCopyAccess` marked as depending on the removed portal?): decided by the lead, yes (section 7, item 6).
- **Owner question 2** (the guard's `-1` for an empty provider list): decided by the lead, it stays; the plan's EHR-4a text says so.
- **For EHR-4b**: the fixture builder and the ehr4a_ database are in `Support/MeasureCalcFixture.cs`; EHR-4b's spec cases can extend it or
  build their own. The engine's Stage 1 thresholds (`GetThreshold`) and Stage 2 thresholds (`GetThresholdMu2`) were not checked against the
  sheets here; that is EHR-4b's deviation list.
- **For EHR-5**: `EhrMeasures.DropOldWorkTables(EhrMeasures.WORK_TABLE_MAX_AGE_SECONDS)` before a quality-measure run.

### 6. Build and test record

Full lane build of `OpenDental.Local.slnf` (Debug|x86) at `f669d2236`: 0 errors, no warning in a changed file. Targeted runs
(`run-tests.ps1 -Mode Targeted`) after it:

| Assembly | Result |
|---|---|
| EhrTests | 147 of 147 passed (the 18 of section 4 among them; none inconclusive) |
| MiddleTierMockTests | 125 of 125 passed (its Middle Tier rules read the new business methods' IL) |
| NoHqTests | 57 of 57 passed |

The negative check of section 3 (the guard at `0`) was a separate build and run, then put back. `InitSqlTests` and `ForkMigrationTests`
are not affected (no schema change) and were not run. After the runs no `ehr4a_` database was left on the server.

### 7. Lead fix round (`ehr/ehr4a-2`)

From `ehr/ehr4a-1` `9097215dc` (both review rounds passed): the reviews' window findings and two lead decisions. The new tests were
committed first (`828a20ecd`, `6e596f28b`), built and run once on the code before any change; then one commit per change.

| # | Finding or decision | Change | Test (before the change, then after) |
|---|---|---|---|
| 1 | `GetPercentText` rounded down to one decimal, so 4001 of 5000 (80.02%) showed "80%" on a green row beside "More than 80%" | `GetResultText` passes a "more than" row's threshold to `GetPercentText(numerator,denominator,threshold)`: still one decimal rounded down (a value never shows more than it is, and an unmet row never reaches its threshold), and for a met row whose one decimal shows no more than its threshold, as many decimals as it takes, at most ten, which is enough for any `int` denominator ("80.02%"; 1000000001 of 2000000000 against 50 is "50.00000005%"). Launch check 3 says what the window shows | TC-19 `Percent_AJustMetRow_ShowsMoreThanItsThreshold_AnUnmetRowNoMore` (the three cases, the largest denominators, and every numerator of every denominator up to 300 against seven thresholds: met shows more, unmet no more, never more than the real value). Before: failed, "Expected:<80.02%>. Actual:<80%>" |
| 2 | The detail ran `GetAuditTable` on the window's thread | Measure Calc keeps, by measure type, the audit table a detail fetched for the last refresh (cleared by each refresh) and hands it to the detail when the measure is opened again (`TableAudit`, `HasTableAudit`). The main window has no table of its own to give: `CalcMeasure` keeps only the table's counts, and sending every measure's patient list back would carry them all over the Middle Tier on each refresh. Without one, the detail fetches it as the main window calculates: one `ODThread`, a Cancel button and a `volatile` flag, the patient grid off meanwhile, the answer through `InvokeIfNotDisposed`; closing the window stops waiting; an error shows and stays in the hint line | TC-20 `Detail_TheAuditTable_IsTheOneAlreadyFetched_OrComesOffTheWindowsThread` (source: one query, inside the `ODThread` worker; `InvokeIfNotDisposed`; Cancel and its flag; the grid off; Measure Calc passes and keeps the table). Before: failed, "The detail starts an ODThread." |
| 3 | After a measure threw, the progress line said "Done: N measures." | The refresh's thread records the measure it starts and the exception; the exit handler words the line with `EhrMeasureCalcRules.GetProgressEndText`: "Stopped with an error at measure 3 of 14 (Problems); 2 measures were calculated. Error: ..." (the message on one line), or "... before the first measure; none was calculated", which stays after the error window is closed. Cancelled and Done as before. These texts are now English rule texts, like the window's other rule texts | TC-21 `Refresh_AfterAnError_TheProgressLineSaysWhereItStopped_NotDone` (source: the refresh words its end through the rule, with no "Done:" of its own). Before: failed (no `GetProgressEndText`). TC-22 `ProgressEndText_AnErrorSaysWhereItStopped_AndKeepsTheMessage_NeverDone` (behavioural; it calls the new method, so it could not run before) |
| 4 | A Modified Stage 2 period from before 2015 into a later year got two contradicting notes ("the requirements of 2014 are shown", "the requirements of 2017 are shown") | `GetPeriodNotes` gives one statement when the period spans two calendar years or starts before 2015, with the two-year warning and "began in 2015" in it, naming the year `GetRequirementsYearShown` gives (the start year; 2017 before 2015), which is the year whose requirements `GetRequirement` applies. `labelNotes` from 56 to 80 px high, `labelProgress` from 18 to 30 (two lines, for an error), the grid from y 142 and 452 high to y 178 and 416 high | TC-23 `PeriodNotes_ModifiedStage2_OneStatementOfTheYearShown_AndTheLabelShowsEveryNote` (start years 2013-2018, one-year and two-year periods: one statement naming one year, the warning in it, and that year's requirements equal the grid's for all 14 measures; then the notes of every stage and period measured with `TextRenderer` in the window's font, Microsoft Sans Serif 8.25, at the designer's width against the designer's height, and no overlap of the notes, the progress line and the grid). Before: failed, "Expected:<1>. Actual:<2>" for 10/1/2013 to 3/31/2014. Measured: the longest combination (Modified Stage 2, 10/1/2014 to 3/31/2015) is 52 px at 976 px wide, with the old texts and the new; so the old 56 px label held it at the designed width, and the new 80 px holds it down to a notes width of about 550 px |
| 5 | `DropOldWorkTables` is a public Middle Tier method: a client could send 0 and drop a running calculation's work tables | `WORK_TABLE_MIN_AGE_SECONDS` = 3600 (one hour; the window's one day is above it). After the `Meth` call, in the code the server runs, `Math.Max(maxAgeSeconds,WORK_TABLE_MIN_AGE_SECONDS)`; the body moved unchanged to the private `DropWorkTablesOlderThan`, which TC-13 reaches through reflection for its age-0 drop (as TC-11 reaches `GetProvsOrNone`). TC-14's age-0 call through the mock now keeps its fresh table | TC-24 `WorkTables_TheServerKeepsAFloorOnTheAge_NoClientDropsATableInUse` (through the Middle Tier mock with 0, -1, 1, 59 and `int.MinValue`, then directly with 0: nothing dropped, and the table made just now stays; in the source the floor follows the `Meth` call; the floor is at least an hour and at most a day). Before: failed, "Through the Middle Tier, 0 s: the server raises it to the floor.(Different number of elements.)" |
| 6 | Lead decision (owner question 1): `ElectronicCopyAccess` counts `OnlineAccessProvided` events, which only granting portal access wrote (removed in `a99adc70b` and `133f16773`) | `IsPortalMeasure` is true for it in all three stages, under the same rule (a met row from old data shows its percent and is green; an unmet one says "depends on the removed patient portal"). The portal note names the stage's portal rows as the grid does, so Stage 1 now has one note. Plan section 9.2 and the release notes say so | TC-25 `ElectronicCopyAccess_CountsTheRemovedPortalsOnlineAccess_MarkedInEveryStage` (each stage lists it; marked at 0 of 40 and at 50%, its percent at 75%; the note names it; no `.cs` file of the program or the business library assigns the event). Before: failed, "Assert.IsTrue failed. Stage1". TC-4 (seven portal rows) and TC-5 (Stage 1's one note) changed with it |
| 7 | Lead decision (owner question 2): the `-1` guard stays | The plan's EHR-4a Work text says `-1`, decided, and why (section 3) | None needed (TC-11 and the negative check of section 3) |
| 8 | Optional | Done: TC-2 names the public source of the 2015 and 2016 years (80 FR 62762, 16 October 2015; 42 CFR 495.22(e); the CMS EP specification sheets for Modified Stage 2); the release notes' opening bullet in lower case like the others, and the Glidewell sub-bullet's full stop back. Not done: a behavioural test of the patient pick (it needs instances of the two windows, and these tests create none; TC-7 covers the rules and TC-16 the chain in the source) | |

**Found by the runs.** The before run at `6e596f28b` skipped the 14 database tests of EHR-1 (`ScratchDb`): "Could not build the scratch
database: The current user has been removed from the cache". TC-24 is the last Middle Tier test of the class and left the fixture's
Middle Tier user logged on; the EHR-1 database's first cache refresh then did not find that user. `MeasureCalcFixture.End()` now logs it
off (`4e03c8ed8`). TC-14 had not shown it, because a later database test of the class logged user 1 back on.

**Result files** (in `.git/od-tools/test-results/` of the main checkout): `ehr4a2-before.txt`, the new tests on the code before the
change (at `6e596f28b`: EhrTests 153, 133 passed, the six new ones failed as quoted above, 14 skipped as just described; NoHqTests 57 of
57; a first run at `828a20ecd`, which it overwrote, had the same six failures and no skip); `ehr4a2-final.txt`, after the full build.

**Build and Targeted runs.** Full lane build of `OpenDental.Local.slnf` (Debug|x86) at `4e03c8ed8`: 0 errors, no warning in a changed
file. Then `run-tests.ps1 -Mode Targeted -Include EhrTests,MiddleTierMockTests,NoHqTests`:

| Assembly | Result |
|---|---|
| EhrTests | 154 of 154 passed (MeasureCalcTests 25; none skipped or inconclusive) |
| MiddleTierMockTests | 125 of 125 passed (its IL rules accept the private `DropWorkTablesOlderThan`, reached only through the Middle Tier method) |
| NoHqTests | 57 of 57 passed |

No `ehr4a_` or `ehr1_` database was left on the server after the runs. Files changed in this round: `FormEhrMeasureCalc.cs`,
`FormEhrMeasureCalc.designer.cs`, `FormEhrMeasureCalcDetail.cs`, `FormEhrMeasureCalcDetail.designer.cs`, `EhrMeasures.cs` (the floor;
edited byte for byte, so the `0x92` byte and the CRLF line ends are kept), `MeasureCalcTests.cs`, `Support/MeasureCalcFixture.cs`, this
note, the plan (the status line, the EHR-4a Work text, section 9.2) and `docs/release-notes/next.md`.
