# Port note: EHR-4 (the Measure Calc window; EHR-4b adds the measure fixtures)

## EHR-4a: the Measure Calc window

Package EHR-4a of `docs/specs/ehr-rebuild-plan.md` (section 7, sections 9.1 and 9.2), on branch `ehr/ehr4a-1` from `main` `2e5a61a67`.
The owner answered E-1 to E-19 as recommended on 2026-11-08; this package carries out E-1 (the window says "internal review only; this
program is not certified; not for attestation or submission"), E-4 (the window opens from the EHR window with no new gate), E-16 (the 2015
alternate exclusions are not modelled, and the window says so) and E-19 (`EhrMeasures.Update` and the `ehrmeasure` table's Numerator and
Denominator columns stay, unused). No schema change, no new `PrefName`, no new third-party file, no licensed content, and no project file
edited. Synthetic data only. Public basis: the CMS EHR Incentive Program specification sheets for eligible professionals (Stage 1 for 2014,
Stage 2, Modified Stage 2 for 2015 through 2017), the 2015-2017 final rule (80 FR 62762) and Open Dental's public manual page of the
Measure Calc window. Nothing was decompiled; `EHR.dll` was never looked at.

### 0. Design, before the code

- **Engine (`EhrMeasures.cs`).**
  - `EhrMeasureStage` (Stage1, Stage2, ModifiedStage2), an enum at the end of the file; it is never stored.
  - `GetStageMeasureTypes(stage)`: the public list of the measure types each `SelectAll*` returns, in its order.
  - `GetMeasuresToCalc(stage)` (Middle Tier): the `ehrmeasure` rows each `SelectAll*` starts from, its query and its `RemoveAll` moved
    unchanged.
  - `CalcMeasure(stage,ehrMeasure,dateStart,dateEnd,provNum)` (Middle Tier): the body of the three `SelectAll*` loops, moved unchanged,
    for one measure. The three `SelectAll*` call these two, so their results do not change.
  - `GetAuditTable(stage,...)`: `GetTable`, `GetTableMu2` or `GetTableMu2Mod`, for the detail window.
  - The guard: the provider list of the five places that build it becomes `-1` when it is empty, so the `IN()` queries stay valid and
    match nothing.
  - `GetReportingProviders()` (Middle Tier): every provider with an `ehrprovkey` row for their names, with the row group the engine's join
    puts them in.
  - `DropOldWorkTables(maxAgeSeconds)` (Middle Tier): drops this database's `tempehrmeasure%` and `tempehrquality%` tables created more
    than a day earlier (`information_schema.TABLES.CREATE_TIME` against the server's `NOW()`).
- **Window (`FormEhrMeasureCalc`).** Provider, dates (this calendar year), stage (from the provider's Meaningful Use stage or the global
  preference), Refresh, Cancel, Print, Close; the internal-review line. Refresh drops old work tables, then computes one measure at a time
  on an `ODThread`; Cancel sets a flag read between measures. The rules of section 9.2 are pure functions of a second class,
  `EhrMeasureCalcRules`, in the same file.
- **Detail (`FormEhrMeasureCalcDetail`).** The texts, the exclusion count and the audit grid; double-clicking a patient returns them up to
  the EHR window, which closes with `PatientSelect`.
- **Tests.** `tests/EhrTests/MeasureCalcTests.cs` and its fixture builder `tests/EhrTests/Support/MeasureCalcFixture.cs`, on a scratch
  database whose name starts with `ehr4a_`.
