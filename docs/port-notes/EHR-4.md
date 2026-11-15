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

## EHR-4b: the measure fixtures

Package EHR-4b of `docs/specs/ehr-rebuild-plan.md` (section 7, section 9.3), on branch `ehr/ehr4b-1` from `main` `e9c46c233`. Numbers
that a test has checked against the public specification sheets: for each stage, synthetic patients who meet and who miss each measure the
engine calculates, a recorded run of the three `SelectAll` calls as golden numbers, and the list below of where the engine and the sheets
disagree. **No engine code, schema, project file or other test file was changed**: the files are `tests/EhrTests/MeasureSpecTests.cs`,
`tests/EhrTests/Fixtures/MeasureSpecFixture.cs`, `tests/EhrTests/Fixtures/MeasureSpecGolden.txt` (the test project's default glob compiles
the fixture; the golden file is read from the repository folder, as the CDA tests read theirs), this section and the plan's status line.
Engine fixes are follow-ups the lead assigns. Two earlier attempts were stopped; their proposed deviations (D01 to D14 of the lead's
analysis) were each run here before being listed, and the IDs are kept.

**Public basis.** The CMS EHR Incentive Program specification sheets for eligible professionals (US government works), read as data from the
lead's download batches M and M2 of 2026-11-14; only their file names, dates and the rules the numbers come from are cited, in a comment
of each test, with a short paraphrase. The latest edition of each stage is cited first:

| Stage (engine call) | Latest edition (cited first) | Older edition (compared) |
|---|---|---|
| Stage 1 (`SelectAll`) | "Stage 1 (2014 Definition)", Last updated: May 2014, the 22 sheets of `EP_MU_TableOfContents.pdf` (`1_CPOE_for_Medication_Orders.pdf` and so on) | "Stage 1 (2013 Definition)", Last Updated: April 2013 (`2013DefinitionEP_*.pdf`): only `VitalSigns` follows it (D05) |
| Stage 2 (`SelectAllMu2`) | `Stage2_EPCore_1` to `_17` and `Stage2_EPMenu_1` to `_6`, Date issued October 2012 to November 2014; `Stage2_EPCore_15_SummaryCare.pdf` Last Updated: August, 2015. The last Stage 2 edition (2015 on is Modified Stage 2) | none |
| Modified Stage 2 (`SelectAllMu2Mod`) | "Objectives and Measures for 2018", Updated: July 2018 (`MedicaidEPModStage2_2018_Obj1.pdf` to `Obj10.pdf`) | the 2017 edition (`MedicaidEPStage2_Obj1.pdf` to `Obj10.pdf`, Updated: November 2016): the same numbers for every case here; its only difference that matters, Obj6's time rule (education may count up to the date of attestation), gives the same answer for D10 |

### 1. The fixture and how a case is counted

- **The database.** `Fixtures/MeasureSpecFixture.cs` builds an `ehr4b_<UTC start>_<process>_<random>_main` scratch database on the local
  MariaDB (127.0.0.1:3306) from `db/init/new-database.sql` and the fork migrations, as `Support/MeasureCalcFixture.cs` builds EHR-4a's; it
  is dropped by the class cleanup, leftovers older than 12 hours (`ehr4b_` only) first. `EHR_TEST_DB=skip` skips, `EHR_KEEP_DB=1` keeps it.
  After the runs no `ehr4b_` database was left on the server.
- **The practice.** One EP with an `ehrprovkey` row and a made-up all-digit NPI (the engine puts NPIs into `IN()` unquoted), one provider
  with no row, a made-up visit code `TESTV01`, and 87 patients (81 of them seen by the EP in 2014) under keys such as `Vs.Child2` or
  `Access.TuesdayMonday`, each with only the rows its case reads. Most are adults seen once, on Monday 3 March 2014. Events are written at 11:00, as the program writes them with the
  time they happened (this is what shows D15).
- **The period and the ages.** The reporting period is the calendar year 2014, and every patient is born before 14 November 2013. The engine
  takes two ages on today's date (D03, D04): from 14 November 2026 on, every fixture patient is old enough for both, so the golden numbers do
  not drift with the date. Modified Stage 2 is calculated on the same 2014 period; its sheets' rules do not depend on the year here (the
  window's year rule of section 9.2 is EHR-4a's and is not used).
- **The data rule (plan section 6).** Made-up names; made-up codes starting with `TEST` (`TESTV01`, `TEST0001`, the radiology LOINC-format
  code `TESTRAD1` of class RAD, which the radiology cases need because `RadiologyDateStartedUsing154` is 2021 in a new database);
  CDCREC race and ethnicity codes; the program's own "declined" constants; and the two codes the engine's source compares with (a smoking
  status, the first positive/negative lab result code), read from `EhrMeasures.cs` at run time (`MeasureSpecCodes`), never written. Dates
  for the `ehrlab` text column are built with `ToString("yyyyMMdd")`. `ValueSetSourceTests.Fixtures_HoldNoLiteralOfACodeFromTheEnginesLists`
  passes over the new files.
- **A case's numbers.** The engine's audit table of the measure (`GetAuditTable`: `GetTable`, `GetTableMu2` or `GetTableMu2Mod`, the table
  `CalcMeasure` counts) for the EP and the period, restricted to the case's patients: its rows are the case's denominator, its met rows the
  numerator. `AuditTables_AreWhatTheSelectAllCallsCount` checks, for all 56 measures, that the whole table's rows and met rows are exactly
  the numbers `SelectAll`, `SelectAllMu2` and `SelectAllMu2Mod` return.
- **A disagreement.** `SpecCheck.Deviation` records the sheet's pair and the engine's recorded pair: the test fails if the engine gives
  anything else (so a change is noticed), fails if the engine now gives the sheet's pair (take the ID off this list), and is otherwise
  Inconclusive with the ID. Meet and miss cases that agree are in tests that pass; each deviation has a test of its own.

### 2. The sheet map

Every measure of `GetStageMeasureTypes` (20, 22 and 14) is mapped to its sheet and the sheet's threshold in `GetSheetRules`;
`SheetMap_EveryMeasureOfEachStage_HasItsSheetOrIsListedWithoutOne` checks the map against the engine's lists, and
`Thresholds_EachStageAndMeasure_AreTheSheetsMoreThanPercent` checks every threshold the three calls return against it: **all 56 thresholds
match** ("more than N percent"; Modified Stage 2 `ElectronicCopy`'s "greater than 5 percent" is the same).

| Stage | Measure (engine) | Sheet (rule), threshold |
|---|---|---|
| 1 | ProblemList, MedicationList, AllergyList | `3_Maintain_Problem_ListEP`, `5_Active_Medication_List`, `6_Medication_Allergy_List` (Attestation Requirements), 80 |
| 1 | Demographics | `7_Record_Demographics`, 50 |
| 1 | Education | `6_Patient-Specific_Education_Resources`, 10 |
| 1 | ProvOrderEntry; CPOE_MedOrdersOnly | `1_CPOE_for_Medication_Orders`, NUMERATOR / DENOMINATOR; OPTIONAL NUMERATOR / DENOMINATOR, 30 |
| 1 | CPOE_PreviouslyOrdered | no sheet (D13), 30 |
| 1 | Rx | `4_e-prescribing`, 40 |
| 1 | VitalSigns | `2013DefinitionEP_8_Record_Vital_Signs` (April 2013), the first NUMERATOR / DENOMINATOR, 50 (D05) |
| 1 | VitalSigns2014; VitalSignsBMIOnly; VitalSignsBPOnly | `8_Record_Vital_Signs`: NUMERATOR / DENOMINATOR; EXCLUSION 3; EXCLUSION 4, 50 |
| 1 | Smoking | `9_Record_Smoking_Status`, 50 |
| 1 | Lab | `2_Clinical_Lab_Test_Results`, 40 |
| 1 | ClinicalSummaries | `13_Clinical_Summaries`, 50 |
| 1 | Reminders | `4_Patient_Reminders`, 20 |
| 1 | MedReconcile | `7_Medication_Reconciliation`, 50 |
| 1 | SummaryOfCare | `8_Transition_of_Care_Summary`, 50 |
| 1 | ElectronicCopyAccess | `11_Patient_Electronic_Access-`, 50 |
| 2 | CPOE_MedOrdersOnly; CPOE_RadiologyOrdersOnly; CPOE_LabOrdersOnly | `Stage2_EPCore_1_CPOE_MedicationOrders`, Measure 1, 60; Measure 2, 30; Measure 3, 30 |
| 2 | Rx | `Stage2_EPCore_2_ePrescribing`, 50 |
| 2 | Demographics | `Stage2_EPCore_3_RecordingDemographics`, 80 |
| 2 | VitalSigns; VitalSignsBMIOnly; VitalSignsBPOnly | `Stage2_EPCore_4_RecordVitalSigns`: the measure; EXCLUSIONS (3); EXCLUSIONS (4), 80 |
| 2 | Smoking | `Stage2_EPCore_5_RecordSmokingStatus`, 80 |
| 2 | ElectronicCopyAccess; ElectronicCopy | `Stage2_EPCore_7_PatientElectronicAccess`, Measure 1, 50; Measure 2, 5 |
| 2 | ClinicalSummaries | `Stage2_EPCore_8_ClinicalSummaries`, 50 |
| 2 | Lab | `Stage2_EPCore_10_ClinicalLabTestResults`, 55 |
| 2 | Reminders | `Stage2_EPCore_12_PreventiveCare`, 10 |
| 2 | Education | `Stage2_EPCore_13_PatientSpecificEdRes`, 10 |
| 2 | MedReconcile | `Stage2_EPCore_14_MedicationReconciliation`, 50 |
| 2 | SummaryOfCare; SummaryOfCareElectronic | `Stage2_EPCore_15_SummaryCare`, MEASURE 1, 50; MEASURE 2, 10 |
| 2 | SecureMessaging | `Stage2_EPCore_17_UseSecureElectronicMessaging`, 5 |
| 2 | FamilyHistory; ElectronicNote; LabImages | `Stage2_EPMenu_4_FamilyHealthHistory`, 20; `Stage2_EPMenu_2_ElectronicNotes`, 30; `Stage2_EPMenu_3_ImagingResults`, 10 |
| MS2 | CPOE_MedOrdersOnly; CPOE_LabOrdersOnly; CPOE_RadiologyOrdersOnly | `MedicaidEPModStage2_2018_Obj3`, Measure 1, 60; Measure 2, 30; Measure 3, 30 |
| MS2 | Rx | `MedicaidEPModStage2_2018_Obj4`, 50 |
| MS2 | SummaryOfCareElectronic | `MedicaidEPModStage2_2018_Obj5`, 10 |
| MS2 | Education | `MedicaidEPModStage2_2018_Obj6`, 10 |
| MS2 | MedReconcile | `MedicaidEPModStage2_2018_Obj7`, 50 |
| MS2 | ElectronicCopyAccess; ElectronicCopy | `MedicaidEPModStage2_2018_Obj8`, Measure 1, 50; Measure 2, 5 |
| MS2 | SecureMessaging | `MedicaidEPModStage2_2018_Obj9`, 5 |
| MS2 | SummaryOfCare, VitalSigns, Smoking, Demographics | no sheet (D12); Stage 2's 50, 80, 80, 80 |

### 3. Known deviations (EHR4B-Dnn)

Each numeric deviation was confirmed by a run (the test is Inconclusive with the ID; the numbers are the case's, sheet against engine).
D05, D12, D13 and D14 are entries without a number to compare: an older edition, measures with no sheet, and sheet rules the engine does
not compute. The IDs are those of the lead's analysis; D15 is new (found by this package's run).

| ID | Stages | The engine | The sheet (latest edition; older where it differs) | Case: sheet / engine |
|---|---|---|---|---|
| EHR4B-D01 | 1, 2, MS2 (`ElectronicCopyAccess`) | Deadline: the first visit plus 4 days, plus 2 more only when the visit is after a Tuesday; a Tuesday visit's deadline is the Saturday | Within 4 business days (Monday to Friday): `11_Patient_Electronic_Access-` (May 2014), `Stage2_EPCore_7` (August 2014) Measure 1, `MedicaidEPModStage2_2018_Obj8` Measure 1 | Seen Tuesday 4 March, access Monday 10 March (the fourth business day): 1/1 / 0/1, each stage |
| EHR4B-D02 | 2 (`ClinicalSummaries`) | Each visit is compared with the patient's first summary ever (`MIN(DateTEvent)`), with no lower bound | Visits whose summary is provided within one business day of that visit: `Stage2_EPCore_8_ClinicalSummaries` (November 2014) | Two visits, one summary on the day of the first: 1/2 / 2/2 |
| EHR4B-D03 | 1, 2 (`Smoking`; MS2's has no sheet) | Age 13 or older on today's date | Unique patients age 13 or older seen in the period: `9_Record_Smoking_Status` (May 2014), `Stage2_EPCore_5` (October 2012) | A patient of 12 at the visit and all year: 0/0 / 0/1, both stages |
| EHR4B-D04 | 1 (`VitalSigns`) | Age 2 or older on today's date | Unique patients age 2 or over seen: `2013DefinitionEP_8_Record_Vital_Signs` (April 2013), the first numerator and denominator | An infant of 1 with height and weight: 0/0 / 0/1 (`VitalSigns2014`, Stage 2 and the blood-pressure measure agree with their sheets: 1/1, 1/1, 0/0) |
| EHR4B-D05 | 1 (`VitalSigns`) | Calculates the April 2013 edition's first numerator and denominator (age 2 or over, all three signs), beside `VitalSigns2014` | The May 2014 edition, `8_Record_Vital_Signs`, has only the newer definition (blood pressure from age 3), which `VitalSigns2014` follows | No number: the engine's `VitalSigns` agrees with the 2013 sheet (1/5 on the vital-signs patients) apart from D04 |
| EHR4B-D06 | 1 (`Reminders`) | "5 years old or younger" coded as younger than 5 at the start of the period (`Birthdate > start - 5 years`) | Unique patients 65 years old or older or 5 years old or younger: `4_Patient_Reminders` (May 2014) | A child who is 5 on every day of 2014, reminded: 1/1 / 0/0 |
| EHR4B-D07 | 2 (`Reminders`) | Patients whose primary provider is the EP, with two or more visit dates with any provider in the 24 months before | Two or more office visits with the EP in the 24 months before: `Stage2_EPCore_12_PreventiveCare` (October 2012) | Two visits with the EP, another primary provider, reminded: 1/1 / 0/0; two visits with another provider, the EP primary, reminded: 0/0 / 1/1 |
| EHR4B-D08 | 1, 2, MS2 (`MedReconcile`) | Counts patients seen by the EP in the period who had a transition received; met only when there are as many reconciliations as transitions | Transitions of care received by the EP in the period; those reconciled: `7_Medication_Reconciliation` (May 2014), `Stage2_EPCore_14` (October 2012), `MedicaidEPModStage2_2018_Obj7` | Two transitions, one reconciled: 1/2 / 0/1; a transition received and reconciled for a patient not seen in the period: 1/1 / 0/0; each stage |
| EHR4B-D09 | 2 (`SecureMessaging`) | A message from the patient of any date counts | A message sent by the patient to the EP during the reporting period: `Stage2_EPCore_17_UseSecureElectronicMessaging` (October 2012) | A patient seen whose only message is of the year before: 0/1 / 1/1 |
| EHR4B-D10 | MS2 (`Education`) | Education events of any date count | Within the reporting period when it is a full calendar year (`MedicaidEPModStage2_2018_Obj6`, Additional Information); the 2017 edition: no earlier than the start of the period's calendar year | Resources given only the year before: 0/1 / 1/1. (The Stage 1 and Stage 2 sheets give no time, so those stages are not compared.) |
| EHR4B-D11 | 1, 2 (`Lab`) | Every lab order by the EP in the period is in the denominator | Only tests whose results are a positive or negative affirmation or a number: `2_Clinical_Lab_Test_Results` (May 2014), `Stage2_EPCore_10` (November 2014) | A test with a free-text result: 0/0 / 0/1, both stages |
| EHR4B-D12 | MS2 (`SummaryOfCare`, `VitalSigns`, `Smoking`, `Demographics`) | Calculated, with Stage 2's queries and thresholds (50, 80, 80, 80) | No sheet: the 2015-2017 rule removed these objectives; neither the 2018 nor the 2017 edition has them (Obj5 has only the electronic exchange) | No number: `ModifiedStage2_TheMeasuresWithNoSheet_CountAsStage2Does_D12` checks they equal Stage 2's on the fixture; D03 applies to MS2's `Smoking` too |
| EHR4B-D13 | 1 (`CPOE_PreviouslyOrdered`) | Calculated: the patients for whom the EP previously ordered medications (the engine's comment names CMS FAQ #3257), 30 | No sheet: `1_CPOE_for_Medication_Orders` (May 2014) has the two numerators and denominators only; the FAQ is not in the batches | No number; the golden numbers cover it |
| EHR4B-D14 | all | Not calculated (`SelectAllMu2` and `SelectAllMu2Mod` remove the types the engine has for them) | Yes/no attestations and public health: Stage 1 (May 2014) `1_Drug_Formulary_Checks`, `2_Drug_Interaction_ChecksEP`, `3_Patient_Lists`, `9_Immunization_Registries_Data_Submission`, `10_Syndromic_Surveillance_Data_SubmissionEP`, `11_Clinical_Decision_Support_Rule`, `15_Core_ProtectElectronicHealthInformation`; Stage 2 `Stage2_EPCore_6_ClinicalDecisionSupport`, `_9_ProtectElectronicHealthInfo`, `_11_PatientLists`, `_16_ImmunizationRegistriesDataSubmission`, `Stage2_EPMenu_1_SyndromicSurveillanceDataSub`, `_5_ReportCancerCases`, `_6_ReportSpecificCases`, and `Stage2_EPCore_15` MEASURE 3; MS2 `Obj1` (security risk analysis), `Obj2` (both CDS measures, drug interaction checks), `Obj10` (public health). Rules inside computed measures: e-prescribing's formulary query (`Stage2_EPCore_2`, `MedicaidEPModStage2_2018_Obj4`: the engine counts prescriptions sent electronically and has no record of the query); Stage 2 e-prescribing's second denominator (with controlled substances) is not offered. The 2013 Stage 1 edition's `2013DefinitionEP_12_Electronic_Copy_of_Health_Information` is not calculated, which agrees with the May 2014 edition (it replaced it by patient electronic access) | No number |
| EHR4B-D15 | 1, 2, MS2 (`ElectronicCopyAccess`); 2 (`ClinicalSummaries`) | Compares the event's date and time (`PIn.Date` keeps the time) with a deadline at midnight of the last day, so an event during the last day misses (Stage 1's clinical summaries compare dates in SQL and are not affected) | The last business day counts: within 4 business days (the sheets of D01); within one business day (`Stage2_EPCore_8`) | Seen Monday, access Friday 11:00 (the fourth business day): 1/1 / 0/1, each stage; a summary the next business day at 11:00 (Tuesday after a Monday visit, Monday after a Friday visit): 1/1 / 0/1 each |

**Proposed, checked and not listed.** Attempt A's "Stage 1 access threshold 50 against the 2013 sheet's at least 10": the May 2014 edition
(`11_Patient_Electronic_Access-`) says more than 50 percent, the engine's threshold; the April 2013 sheet's "at least 10 percent"
(`2013DefinitionEP_5_Patient_Electronic_Access`) belongs to the older timely-access measure, which the engine keeps as the deprecated
`TimelyAccess` (10) and does not calculate. The engine follows the latest edition, so this is no deviation. **Checked by a case, no
deviation:** Modified Stage 2 view, download or transmit takes the calendar year (an action of the year before: 0/1, as the 2018 sheet);
Modified Stage 2 secure messaging counts web mail sent by the EP and not a patient's message alone (1/3, as the 2018 sheet); an imaging
result still expected (`DocNum` -1) is missed; an unsigned progress note does not count; the e-prescribing example of the plan (4
prescriptions, 3 electronic: 75 percent) and a controlled substance in no denominator. **By reading only:** family history counts any
`familyhealth` row, and `FamilyRelationship` has only first-degree relatives (parent, sibling, offspring). Not compared with any sheet: the
exclusion counts (the sheets' exclusions are attestation choices); they are in the golden numbers.

### 4. Tests

`tests/EhrTests/MeasureSpecTests.cs`, 36 tests: 24 that pass and 12 deviation tests that are Inconclusive by design (one per row above with a
number, D15 in two: online access and clinical summaries). The meet and miss cases:

| Test | Stages | Meet / miss (sheet numbers) |
|---|---|---|
| `SheetMap_EveryMeasureOfEachStage_HasItsSheetOrIsListedWithoutOne` | all | the map of section 2 against `GetStageMeasureTypes` (no database) |
| `Fixture_TheWeekdaysTheBusinessDayCasesNeed_AndTheEnginesCodesReadAtRunTime` | all | the weekdays, the period, the ages today, the two codes read (no database) |
| `AuditTables_AreWhatTheSelectAllCallsCount` | all | the 56 measures' audit tables against the three calls |
| `Thresholds_EachStageAndMeasure_AreTheSheetsMoreThanPercent` | all | the 56 thresholds |
| `Golden_TheThreeSelectAllCalls_GiveTheRecordedNumbers` | all | `Fixtures/MeasureSpecGolden.txt` (numerator/denominator, threshold and exclusion count of each measure) |
| `ProblemMedicationAllergyLists_Stage1_...` | 1 | an entry, "none known", nothing: 2/3 each |
| `Demographics_...` | 1, 2 | all elements, declined, no ethnicity: 2/3 |
| `VitalSigns_EachNumerator_...` | 1, 2 | five patients: `VitalSigns2014` 2/5, BMI only 3/5, BP only 2/4, the 2013 `VitalSigns` 1/5; Stage 2 2/3 (the patients the sheet's "and/or" does not decide), 3/5, 2/4 |
| `Smoking_...` | 1, 2 | recorded, not recorded: 1/2 |
| `ModifiedStage2_TheMeasuresWithNoSheet_CountAsStage2Does_D12` | MS2 | the four measures equal Stage 2's |
| `Cpoe_...` | 1, 2, MS2 | Stage 1 by patient 1/2 (a patient with no medication in neither); orders 1/2; lab and radiology orders 1/2 each, a radiology order no lab order and the other way round (0/0) |
| `EPrescribing_...` | 1, 2, MS2 | 4 prescriptions, 3 electronic: 3/4 = 75 percent; a controlled substance in none |
| `LabResults_...` | 1, 2 | numeric, coded positive/negative (the engine's code), not incorporated, the older order with and without a panel: 3/5; radiology orders 0/0 |
| `ImagingResults_...` | 2 | attached, expected: 1/2 |
| `OnlineAccess_WithinFourBusinessDays_...` | 1, 2, MS2 | same day, third business day, sixth, none: 2/4 |
| `ViewDownloadTransmit_...` | 2, MS2 | in the period, none: 1/2; MS2 the year before: 0/1 |
| `ClinicalSummaries_WithinTheStagesBusinessDays_...` | 1, 2 | Stage 1: same day, next day, third business day, fourth, none, Friday to Monday: 4/6; two visits one summary 1/2. Stage 2: same day, third, fourth, none: 1/4 |
| `Education_ResourcesProvided_...` | 1, 2, MS2 | given, none: 1/2 |
| `Reminders_Stage1_...` | 1 | two of 68 (one reminded), a child of 3, an adult: 2/3 |
| `PreventiveCare_Stage2_...` | 2 | two prior visits reminded, not reminded, one visit: 1/2 |
| `MedReconcile_...` | 1, 2, MS2 | reconciled, not: 1/2 |
| `SummaryOfCare_...` | 1, 2, MS2 | provided, not, electronic: 2/3 (Stage 1, Stage 2 MEASURE 1); electronic 1/3 (Stage 2 MEASURE 2, MS2 Obj5) |
| `SecureMessaging_...` | 2, MS2 | Stage 2 from the patient, none: 1/2; MS2 web mail sent, none, a patient's message alone: 1/3 |
| `FamilyHistoryAndElectronicNotes_...` | 2 | a parent, none: 1/2; signed, unsigned, none: 1/3 |

The deviation tests: `..._D01`, `..._D02`, `..._D03`, `..._D04`, `..._D06`, `..._D07`, `..._D08`, `..._D09`, `..._D10`, `..._D11`, and
`OnlineAccess_OnTheFourthBusinessDay_IsTimely_D15` and `ClinicalSummaries_Stage2_ASummaryDuringTheNextBusinessDay_IsWithinOne_D15`.

### 5. Build and test record

BUILD_AND_TEST_RECORD

### 6. For the lead

- **Engine fixes** (plan section 15) are the numeric deviations D01-D04 and D06-D11, D15; each test fails when its fix lands, saying to take
  the ID off this list and make the case a sheet case, and the golden file is then recorded again. D01 and D15 together decide the business-day
  arithmetic of three measures. D05, D12 and D13 are choices for the owner (keep the older or extra measures, or drop them); D14 needs none.
- **Release notes**: none; the package adds tests only and changes nothing an office sees.
- **Project baseline**: `tests/EhrTests` gains two `Compile` items by its default glob (`MeasureSpecTests.cs`,
  `Fixtures/MeasureSpecFixture.cs`); the golden file is not an item. `tools/project-baseline` is re-recorded at the next gate.
- **Plan section 10.2** names `tests/EhrTests/Support/MeasureCalcFixture.cs` as EHR-4b's; it was not needed and not edited (EHR-4b builds its
  own fixture under `tests/EhrTests/Fixtures/`).
