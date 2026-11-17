# The EHR quality measures move to the 2027 versions: package plan

**Summary for the owner.**
- **What changes.** The seven measures CMS still publishes (CMS68, 69, 74, 75, 138, 155, 165) move to their 2027 versions, calculated
  by new open code from each measure's logic, with a new window and new QRDA III and QRDA I exports. The office imports the 2026-05-14
  value-set release itself (three files, one pick); no code ships.
- **What is deleted.** The 2014 engine, its windows and its QRDA writers (your decision 2), and the 2011 engine, its window and its PQRI
  XML export (your decision 4, E-21 answered 2026-11-14), all in EHR-6b with their tests and project entries; CMS127 and CMS147
  (retired). Saved database rows stay. EHR-6a rewrites the merged release notes that tell offices to import the June 2013 release.
- **The NCQA measures.** CMS138, CMS155 and CMS165 are built in the normal order (your decision 5, E-10b answered 2026-11-14). NCQA's
  copyright and disclaimer statements go with them into the catalog, the window, its print-out and both QRDA files (section 11.1).
- **Fixed after the check.** Every medication row counts as active over its dates and as an order on its start date (E-39); a problem
  set Resolved or Inactive without a stop date ends on its last change (E-26); the vital-sign windows take the age on the exam date, so
  CMS155's 3-17 are covered (E-35); the not-performed list no longer crashes on an unknown row (EHR-7); the agent-day sums match the table.
- **Packages** (agent-days with reviews). Stage 4, after S4-1 merges: EHR-6a catalog of all seven measures and import 1.25, EHR-6b
  deletion of the 2014 and 2011 code 0.75, EHR-7 data entry 1.25 (3.25 in all). 3a, one EHR writer: EHR-8a1 0.75, 8a2 0.75, 8b 0.75,
  EHR-5a window 0.5, 8c 1, 8d 0.75, 5b QRDA III 0.75, 5c1 and 5c2 QRDA I 1.5 (6.75 in all; 6.0 if 8a1 fits in stage 4). EHR-9 0.75,
  optional. About 10 (11 with EHR-9), 10-13 with fix rounds. Nothing merges in 3b; the rest waits for the switch. EHR-5d is gone with the
  2011 window.
- **Waits for counsel.** Nothing in this plan. E-8 (extended) is applied when counsel answers; E-10 gates nothing here any more (E-40).
- **Downloads (approved).** Batch Q (section 15) was approved by the owner on 2026-11-14 with E-33: CMS 2027 QRDA III guide with
  schematron and sample; HL7 QRDA III STU 2.1 and QRDA I STU 5.3; QDM 5.6; the CQL specification; CMS eCQM logic guidance; the test
  cases only if public. The 2027 table PDF is kept, as one reference copy (the lead's decision).
- **Questions** (section 14). All answered by the owner on 2026-11-14, "all as recommended". Data and calculation: E-22, E-24, E-26, E-27,
  E-28, E-29, E-34, E-35, E-39. Codes and import: E-20, E-23, E-30, E-32, E-37. Scope and names: E-25, E-31, E-33, E-38, E-40. Schedule:
  E-36. E-21 and E-10b were answered earlier the same day (owner decisions 4 and 5).

**What this is.** The package plan for replacing the program's 2014 clinical quality measures (CQMs) with CMS's 2027 eligible-clinician
eCQM versions. It extends `docs/specs/ehr-rebuild-plan.md` (below: "the EHR plan"), keeps its rules for EHR packages (its section 6), and
replaces its EHR-5 and its section 8 release. Package briefs are cut from it the same way.

**Date.** 2026-11-14. **Status.** Decided (2026-11-14): the owner's five decisions and every question of section 14 answered as
recommended. The draft was revised the same day after a critique (appendix, first table), again after a check and the owner's answers to
E-21 and E-10b (appendix, second table), and then decided (appendix, third table). Nothing waits for counsel. **EHR-6a written**
(2026-11-14, branch `ehr/ehr6a-1`, `docs/port-notes/EHR-6.md`): the catalog, the three-file import and the LOINC notice. **E-20's record
made** (2026-11-16, from the owner's own download; `docs/port-notes/EHR-6.md` section 0) **and the readers fixed to it** (EHR-6a-R, branch
`ehr/ehr6a-r-1`, section 14 there): the by-CMS workbook, E-30's manifest-only import with a 1,024 MB total, the owner's three files imported
on a scratch database at 7 of 7; EHR-6a merges with it.
**EHR-6b written** (2026-11-14, branch `ehr/ehr6b-1` from `main`, `docs/port-notes/EHR-6b.md`): the 2014 and 2011 code deleted as section 3
says, with no caller beyond section 2.1's list, so no EHR-6c; it merges before 3a wave 1, without waiting for EHR-6a.

**Owner decisions (2026-11-14).**
1. The 2027 versions of all nine of today's measures that CMS still publishes for eligible clinicians. A measure CMS retired is dropped and
   named.
2. The 2014 versions are deleted deliberately. They are not kept alongside the 2027 versions.
3. Value sets come from the office's own VSAC download of "eCQM Value Sets for Eligible Clinicians", published 2026-05-14 (Excel and SVS,
   sorted by CMS ID), plus the "Direct Reference Codes" and "Code System Versions" files. The codes never enter the repository.
4. (E-21, answered 2026-11-14.) The 2011 quality-measure engine and its window are deleted deliberately too, with the 2014 versions: the
   2011 code of `QualityMeasures.cs`, its two windows, the PQRI XML export, everything only they use, their tests and their project-file
   and baseline entries (EHR-6b). Saved database rows stay untouched. Every caller is checked so that nothing else loses code it needs.
5. (E-10b, answered 2026-11-14.) Building the NCQA measures CMS138, CMS155 and CMS165 in this open-source program is okay; nothing waits
   for counsel on this. NCQA's copyright and disclaimer statements from the measure packages stay with those measures wherever they are
   shown, printed or written to QRDA (section 11.1). The three follow the normal package order.

The standing rule still applies: keep and port everything; that an office does not use a feature is no reason to cut it. Decisions 2 and
4 are the owner deleting the 2014 versions and the 2011 engine on purpose. They cover nothing else: saved rows, the Measure Calc window
(EHR-4a), its work-table cleanup and the other EHR windows stay.

**This document carries no code of any code system.** It names value sets by OID and name, and direct reference codes by measure, code
system and role. The codes are in the CQL files cited, which stay outside the repository.

**Basis.**
- Three read-only research lanes (2026-11-14). They downloaded CMS's public 2027 specification packages and measure pages from eCQI under
  the owner's approval of 2026-11-14, into the lead's download folder, outside the repository (one folder per CMS ID), with SHA-256 for
  each file. Their per-measure notes and the critique were working notes, outside the repository. The facts this plan uses are repeated
  here with their sources, so the plan does not depend on those notes.
- `main` at `5737519bf`: EHR-0, EHR-1, EHR-3, EHR-4a, EHR-2 (merged from `ehr/ehr2-2` at `8f44b0892`) and TC-4 merged. Line numbers are
  from that commit. Checked again at `main` `9bf1de770` for the second revision: every file cited is unchanged since `5737519bf` except
  `docs/specs/net10-plan.md`, whose change is after its line 1464 and moves no cited line. The files this plan cites are unchanged since `d33a8f330`, except the files EHR-2 changed (`EhrCodes.cs`,
  `FormCodeSystemsImport.*`, its tests), whose lines equal `8f44b0892`'s, and three one-line comment edits of EHR-2 that move no line (`FormMedical.cs:880`,
  `FormEhrPatientSmoking.cs:109`, `FormEhrNotPerformedEdit.cs:50`).
- Stage state: stage 4 is running (S4-1 open). `docs/specs/net10-plan.md` and `docs/specs/wpf-tooth-chart-plan.md` at `5737519bf` give
  the windows of section 13.
- Nothing was built, run or tested. Nothing else was downloaded: no UMLS or VSAC sign-in, no value-set content, no implementation guide.
  The guides of section 9 are named from public knowledge, marked "(not opened)", and are download batch Q, which the owner approved on
  2026-11-14 (E-33, section 15).

---

## 1. The plan in one page

| Package | Does | After | Earliest window (section 13) | Size (agent-days) |
|---|---|---|---|---|
| **EHR-6a The 2027 catalog and import** | All seven measures, with the NCQA three's copyright and disclaimer statements, and the pick-list value sets as data (`CqmCatalog`); the importer reads the 2026-05-14 release with its Direct Reference Codes and Code System Versions files in one pick, as one swap, and checks the versions; the release notes' June 2013 text rewritten | E-20's record before merge; E-23 | Stage 4: written now, merged after S4-1 merges | L, 1.25 |
| **EHR-6b The 2014 and 2011 code deleted** | Both engines of `QualityMeasures.cs` (the whole file) and `QualityMeasure.cs`, the four windows, the 2014 QRDA writers and the 2011 PQRI XML export, `FormEHR.ProvKeyIsValid` and the dead 2011 handler, with their eight project-file items, baseline lines and tests | EHR-6a | Stage 4; before 3a wave 1 (S3a-3) and S3a-13 | M, 0.75 |
| **EHR-7 Data entry on the 2027 catalog** | The data-entry windows that name 2014 value sets or hard-code measure codes move to the catalog and the imported roles; the recommended encounter and pregnancy codes come from the import; the not-performed grid keeps each row's item in the row; E-34 and E-35 (with the vital signs' age rule) as answered | EHR-6b | Stage 4, or 3a wave 1; merged before S3a-M3 starts | L, 1.25 |
| **EHR-8a1 CQL operators and the data layer** | `CqmCql` and `CqmData` with their tests | EHR-7 | Stage 4 if a slot is left, otherwise 3a wave 1 | M-L, 0.75 |
| **EHR-8a2 The engine and the dental measures** | `CqmEngine` with the whole result shape (strata, population groups, episode members) and the Middle Tier; the CQMCommon and Hospice libraries; CMS74v16 and CMS75v15 | EHR-8a1 | 3a, waves 1-3 | M-L, 0.75 |
| **EHR-8b CMS68v16 and CMS69v15** | The episode-based measure; BMI with the palliative-care library | EHR-8a2 | 3a | M-L, 0.75 |
| **EHR-5a The 2027 CQM window** | The window and its detail on the new engine (replaces the 2014 window), with the NCQA statements in the notes, the detail and the print-out | EHR-8a2 (and 8b if merged) | 3a | M, 0.5 |
| **EHR-8c CMS138v15 and CMS155v15** (NCQA) | Tobacco, three population groups; children's weight, three groups and two strata | EHR-8b | 3a | M-L, 1 |
| **EHR-8d CMS165v15** (NCQA) | Blood pressure, with the frailty, advanced-illness and adult-outpatient libraries | EHR-8c | 3a | M-L, 0.75 |
| **EHR-5b QRDA III 2027** | The aggregate report per CMS's 2027 QRDA III guide for eligible clinicians, for every measure in the catalog, with the NCQA statements | EHR-5a, EHR-8d; download batch Q | 3a | M-L, 0.75 |
| **EHR-5c1 QRDA I: the document, encounters and diagnoses** | The patient document frame per HL7 QRDA I STU 5.3 (QDM 5.6 templates), patient data, payer, encounters, diagnoses, the NCQA statements | EHR-5b | 3a | M-L, 0.75 |
| **EHR-5c2 QRDA I: the other templates** | Procedures, interventions and medications with negation, physical exams, assessments | EHR-5c1 | 3a | M-L, 0.75 |
| **EHR-9 The measures' own test cases** (optional) | CMS's test cases for the seven measures, run from a folder outside the repository | download batch Q (if public); EHR-8d | 3a, or after the switch | M-L, 0.75 |

EHR-5d (the 2011 window's gate) is gone: the 2011 window is deleted in EHR-6b (decision 4), and `ProvKeyIsValid` with it.

**Total.** About 10 agent-days of writing and review (11 with EHR-9), at the measured pace (a medium package takes about half a day of
agent time with its reviews): stage 4's three packages 3.25 (1.25 + 0.75 + 1.25), the nine 3a packages 6.75 (0.75 x 7 + 0.5 + 1),
EHR-9 0.75. EHR-2 needed four fix rounds; with rounds like that on the larger packages, plan for 10-13. Office and owner time is extra:
the record of the three files (E-20), the talk-through.

**Units.** Agent-days here are agent time with reviews. The EHR plan's section 17 counts rough developer-days, and it rates its M
packages at 4-7 dev-days, which took about half an agent-day each. The two units are never added together. Section 17 states this plan's
total in agent-days and names the unit (section 16).

**Windows.** The EHR track holds at most one writer slot (the EHR plan's decision 7). Stage 4 takes EHR-6a, 6b and 7. 3a waves 1-3 take
the engine, the seven measures, the window and QRDA, with E-36 deciding which slot they may take. **Nothing EHR merges during 3b.** A package not merged when
3b starts waits until `net10/switch` has merged to `main` and is then finished on .NET 10 (section 13).

**Decisions in short (decided 2026-11-14).**
1. Each 2027 measure is a C# translation of its CQL, one method per CQL `define`, on the program's own data. No CQL engine (E-22, section 5).
2. Seven measures: CMS68v16, CMS69v15, CMS74v16, CMS75v15, CMS138v15, CMS155v15, CMS165v15. CMS127 and CMS147 are retired for
   eligible clinicians and are dropped (section 4.2).
3. The 2014 engine, its windows and its QRDA writers are deleted (EHR-6b, owner decision 2), and so are the 2011 engine, its window and
   its PQRI XML export (EHR-6b, owner decision 4).
4. The measures are data in the catalog, and every value-set OID in the program comes from it, so a later yearly update edits data, not
   code paths. File and class names carry no year (E-31). One catalog file holds all seven measures (EHR-6a).
5. Direct reference codes are imported from the office's file and found by role. Until EHR-7 merges, the data-entry windows still hold the
   code literals listed in section 8. After EHR-7, the only measure code literals left are LOINC ones (E-37) and the two over- and
   underweight literals that the vital-sign windows write (E-35, E-8).
6. No schema change. Direct reference codes are `ehrcode` rows, and the three files are imported together in one swap. The fork-owned
   `ForkEhrValueSetRelease` row records the release, its three files, its code-system versions and the role table.
7. The window stays labelled for internal review only (E-1). The program is not certified, and the numbers cannot be submitted.
8. The three NCQA measures (CMS138, CMS155, CMS165) are built in the normal order (owner decision 5): their catalog data in EHR-6a, their
   classes in EHR-8c and EHR-8d, before QRDA. NCQA's copyright and disclaimer statements are kept with them wherever they are shown,
   printed or written to QRDA (section 11.1).

---

## 2. What the engine does today

### 2.1 Where the 2014 measures live

All of it is in `OpenPmsDentalBusiness/Db Multi Table/QualityMeasures.cs` (9,100 lines) and `QualityMeasure.cs` (319 lines). The file holds
two separate engines.

| Part | Lines in `QualityMeasures.cs` | What it is |
|---|---|---|
| 2011 engine | 58-89 `GetAll`, 242-308 `GetId`, 359-383 `GetPQRIMeasureNumber`, 384-450 `GetDescript`, 500-2332 `GetTable` (SQL per measure, with work tables at 513-546), 4887-4898 `CalcNumerator`, 4935-4946 `CalcExclusions`, 4998-5175 the explanations | The 2011 Edition measures (`QualityType`, `QualityMeasure.cs:73-104`), shown by `FormEhrQualityMeasures` with its PQRI XML export. It reads no value set: its code lists are literals (the EHR plan's 8.3, `:1408`, `:1539`) |
| 2014 engine | 91-241 `GetAll2014`, 309-358 `GetId2014`, 451-499 `GetDescript2014`, 2333-3094 `GetEhrCqmData` (per measure: 2359 MedicationsEntered, 2394 Weight, 2523 CariesPrevent, 2587 ChildCaries, 2623 Pneumonia, 2669 TobaccoCessation, 2738 Influenza, 2904 WeightChild, 2984 BloodPressureManage), 3095-4000 the data queries, 4001-4886 `ClassifyPatients`, 4899-4934 and 4947-4997 the counts, 5176-5332 the explanations, 5333-5763 the eMeasure identifiers, 5764-9100 QRDA (5815 Category III, 6973 Category I, 8919-9100 the checks) | The nine 2014 versions, listed in the EHR plan's 8.1: CMS68v3, CMS69v2, CMS74v3, CMS75v2, CMS127v2, CMS138v2, CMS147v2, CMS155v2, CMS165v2 (`GetEMeasureNum` 5371, `GetEMeasureVersion` 5485) |

- **The 2014 data model.** `QualityMeasure.Type2014` (`QualityMeasure.cs:12`), `QualityType2014` (106-142), `CqmItemAbbreviation` (144-164)
  and the `EhrCqm*` classes (167-318) are used only by the 2014 engine and its windows.
- **Its data.** `GetEhrCqmData` takes the provider's records by name (2338-2341), the encounters of the period by that provider
  (2350-2356), and then each patient's data from `encounter`, `procedurelog`/`procedurecode` (3868-3929), `disease`/`diseasedef`
  (3609-3708), `intervention` (3351), `medicationpat` (3443), `ehrnotperformed` (3510), `ehrmeasureevent` (tobacco 3275, current
  medications 3710), `vitalsign` (BMI 3773, blood pressure 3830), `vaccinepat` (3931), `patientrace` and `payortype` (3222-3273).
- **Its rules on declined and medication rows.** Intervention rows are kept by code; a declined pharmacotherapy row is kept on purpose,
  because the intervention window stores declined medications as interventions (`:3370-3378`; `FormInterventionEdit.cs:197`, which makes a
  `medicationpat` only when the patient did not decline, and `:232`). A `medicationpat` with a `PatNote` is an Order, one without is Active
  (`:3442`, used at `:4358`). `PatNote` is a free dosage note ("10 mg tablets, two tabs 3 times per day", `TableTypes/MedicationPat.cs:15-17`),
  so the 2027 engine does not keep that rule (section 5.3, E-39).
- **Its value sets.** Every list comes from `EhrCodes.GetForValueSetOIDs`, so it needs EHR-2's import.
- **Its windows.** `FormEhrQualityMeasures2014` (206 lines) with `FormEhrQualityMeasureEdit2014` (152 lines), opened by "Quality Meas"
  (`FormEHR.Designer.cs:102-104`, `FormEHR.cs:553-564`). The window lists providers only when `FormEHR.ProvKeyIsValid` is true
  (`FormEhrQualityMeasures2014.cs:43`), which it never is, so today it lists none. Two parts of it are not measure logic: the pick of the
  legal authenticator when the practice default provider is not a person (`:149-165`), and the OID registry window opened when QRDA fails
  (`:193-197`).
- **The 2011 window** (`FormEhrQualityMeasures`, 260 lines, and `FormEhrQualityMeasureEdit`, 72 lines) has no button: the handler
  `FormEHR.cs:547-551` is dead, as in Open Dental. It uses the same key gate (`FormEhrQualityMeasures.cs:38`).
- **Tests.** No test covers either engine. `ProvKeyRuleTests.cs:341` names the two windows as `ProvKeyIsValid`'s callers.
  `ValueSetSourceTests.cs:26-32` (EHR-2) reads `GetEMeasureNum` and `GetEMeasureVersion` by reflection.
- **Project files.** `OpenPmsDental.csproj` has `Compile Update` and `EmbeddedResource Update` items (DependentUpon) for the four forms:
  the 2014 two at 1046-1048, 1052-1054, 3909-3911, 3915-3917, the 2011 two at 1043-1045, 1049-1051, 3906-3908, 3912-3914.
  `tools/project-baseline` records them (`items.txt:6589-6596`, `8421-8424`; `resources.txt:285-288`) and the business project's two files
  (`items.txt:4527-4528`, although that project globs). A new form needs no item (the file's header comment; EHR-4a's `FormEhrMeasureCalc`
  has none).
- **Outside callers** (a grep of `main` for every type and method of the two files and the four forms, comments and `docs/` left out):
  `FormEHR.cs:547-551` (the dead 2011 handler) and `553-564` ("Quality Meas"); `FormEHR.ProvKeyIsValid` (`FormEHR.cs:624-628`), whose only
  callers are the 2011 and 2014 windows; `ProvKeyRuleTests.cs:336-341`; `ValueSetSourceTests.cs:2`, `26-32` and the literal guard's path
  list at `192`; the project items and baseline lines above. `EmailMessages.SendTestUnsecure`, which the PQRI submit uses
  (`FormEhrQualityMeasures.cs:249`), has four other callers (`FormEhrEncryption.cs:159`, `FormEhrHash.cs:27`, `FormEhrLabPanels.cs:97`,
  `FormEhrVaccines.cs:130`) and stays; so do `MsgBoxCopyPaste` and EHR-4a's `EhrMeasures.DropOldWorkTables`, which `FormEhrMeasureCalc.cs:188`
  calls and which also clears old `tempehrquality` work tables left by a stopped run (`EhrMeasures.cs:7242-7291`). `ODZip.GetDefaultEncodingBytes`
  keeps its caller `Ramq.cs:175`.

### 2.2 How encounters reach the measures

- The 2014 engine counts only `encounter` rows (`QualityMeasures.cs:2350-2353`, `:3104-3106`).
- A dental office rarely enters encounters by hand. Completing a procedure inserts a "default encounter" with the code of
  `PrefName.CQMDefaultEncounterCodeValue` (`Encounters.cs:28-51`, called from `Procedures.cs:2297`, `:4476`, `ChartModules.cs:2136` and
  `HL7/MessageParser.cs:1870`). The Encounter Tool adds them for past procedure dates (`FormEncounterTool.cs:152`).
- The recommended default codes are nine SNOMED CT literals chosen so that the encounters would reach "the 9 clinical quality measures"
  (`Pref.cs:768`; `FormEhrSettings.cs:105-117`, `FormEncounterTool.cs:43-55`). They were picked for the 2014 versions. The recommended
  pregnancy codes (`PregnancyDefaultCodeValue`) are nine more SNOMED CT literals, picked for the 2014 pregnancy value sets
  (`FormEhrSettings.cs:120-133`).

### 2.3 Faults in the 2014 code (found by the research; not fixed, since the code goes)

- Influenza: the denominator query appends the patient filter and the order to `command` instead of `encCommand` (2738 region).
- Blood pressure: no lower age bound (`encsWhere18To85` at 2995-2997 leaves out `encounterWhereAdults`), and the numerator loop reads the
  most recent reading only when it falls on the most recent encounter's date (4768 region).
- The engine's `GetEMeasureSetId` values (5516) are not the measures' set IDs. The 2027 HQMF set ID of each measure equals the engine's
  version-neutral ID (`GetEMeasureVNeutralId`, 5447).

---

## 3. What is deleted, what stays, what moves

| Item | Fate | Package |
|---|---|---|
| The 2014 engine parts of `QualityMeasures.cs` (section 2.1 table) | Deleted (owner decision 2) | EHR-6b |
| The 2011 engine parts of `QualityMeasures.cs` (section 2.1 table: 58-89, 242-308, 359-450, 500-2332, 4887-4898, 4935-4946, 4998-5175), so with the 2014 parts the whole file (9,100 lines; its head, 1-57, holds only private fields of the two engines) | Deleted (owner decision 4) | EHR-6b |
| The 2014 QRDA I and III writers (5764-9100), with their identity faults (EHR plan, E-15) | Deleted. The 2027 writers (EHR-5b, EHR-5c1, EHR-5c2) are new, and keep E-15's rules | EHR-6b |
| `QualityMeasure.cs` (319 lines: `QualityMeasure`, `QualityType`, `QualityType2014`, `CqmItemAbbreviation`, the `EhrCqm*` classes) | Deleted whole. No code outside the two engines and the four windows reads any of its types (section 2.1, outside callers) | EHR-6b |
| `FormEhrQualityMeasures2014`, `FormEhrQualityMeasureEdit2014`, `FormEhrQualityMeasures` (with its PQRI XML show and submit, `GeneratePQRS_xml`), `FormEhrQualityMeasureEdit` (`.cs`, `.Designer.cs`, `.resx` each), and their eight `Update` items in `OpenPmsDental.csproj` (section 2.1) | Deleted. The port note names the removed Compile and EmbeddedResource items and the two business files, so the lead's baseline re-record accepts them (`tools/project-baseline/README.md`) | EHR-6b |
| `FormEHR.cs:547-551` (`butQuality_Click`, no button) and `FormEHR.ProvKeyIsValid` (`:624-628`, no caller once the windows go) | Deleted | EHR-6b |
| The legal-authenticator pick and the OID-registry fallback of the 2014 window (`FormEhrQualityMeasures2014.cs:149-165`, `:193-197`) | Kept: rebuilt in the 2027 export | EHR-5b |
| "Quality Meas" (`FormEHR.cs:553-564`) | Shows "The 2027 quality measures are not in this build yet" until EHR-5a opens the new window | EHR-6b, EHR-5a |
| `EmailMessages.SendTestUnsecure`, `MsgBoxCopyPaste`, `EhrMeasures.DropOldWorkTables` (with its `tempehrquality` pattern, which still clears work tables a stopped 2011 or 2014 run left), `ODZip.GetDefaultEncodingBytes` | Stay: each has other callers (section 2.1) | none |
| The June 2013 release in EHR-2 (label, nine versions, manifest of 79 OIDs) | Replaced by the 2026-05-14 release | EHR-6a |
| The release notes that EHR-2 merged (`docs/release-notes/next.md:26-28`, `492-517`): download the "June 2013 EP Only Release", and "the 2014 quality measures have their codes again" | Rewritten for the 2026-05-14 release and its three files; the 2014 claim goes. If a release is cut before EHR-6a merges, the lead fixes these lines first | EHR-6a |
| Data-entry pick lists on 2014 value sets, and the measure code literals in data entry | Move to the 2027 catalog and the imported roles (section 8) | EHR-7 |
| Saved data (`encounter`, `intervention`, `ehrnotperformed`, `ehrmeasureevent`, `vitalsign` and the rest) | Untouched. The 2027 engine reads the same tables | none |
| The deletion's history | `main` before EHR-6b keeps the 2014 and 2011 code readable (`git show 5737519bf:"OpenPmsDentalBusiness/Db Multi Table/QualityMeasures.cs"`); EHR-5b and EHR-5c may read its XML helpers and checks there | none |

The release notes say: the 2014 CQM versions and their QRDA export, and the 2011 Edition quality-measure window (no button since Open
Dental) with its PQRI XML export, were removed on the owner's decisions of 2026-11-14; the 2027 versions replace them.

---

## 4. The measures

### 4.1 The seven 2027 versions

Sources: each measure's 2027 page `https://ecqi.healthit.gov/ecqm/ec/2027/cms00NNvMM`, its package
`https://ecqi.healthit.gov/sites/default/files/ecqm/measures/CMSNN-vMM.0.000-QDM.zip` (HQMF, CQL, ELM), its human-readable HTML and its
technical release notes (`-QDM-TRN.xlsx`), all saved in the lead's download folder, outside the repository, one folder per measure,
and the 2027 EC eCQM table (`https://ecqi.healthit.gov/sites/default/files/2027-EligibleClinician-eCQM-Table.pdf`, May 2026, 49 CMS IDs;
SHA-256 `a6c31b48...`; one reference copy kept, section 15). All seven are QDM 5.6 with CQL; all share `CQMCommonQDM` 9.0.000.

| Measure | 2014 engine | 2027 | Steward | Basis | Period anchor for age | Strata and groups | MIPS | Set ID (= engine's version-neutral ID) / 2027 version-specific ID |
|---|---|---|---|---|---|---|---|---|
| Documentation of Current Medications | CMS68v3 (MedicationsEntered) | **CMS68v16** (`CMS68DocumentationCurrentMeds` 16.0.000) | CMS (AIR) | Episode (each qualifying encounter) | none (no age limit) | none | 130 | `9a032d9c-3d9b-11e1-8634-00237d5bf174` / `a58f1abe-de03-4ea2-a3ec-48d2a06b3bc5` |
| BMI Screening and Follow-Up Plan | CMS69v2 (WeightOver65, WeightAdult) | **CMS69v15** (`CMS69BMIScreenAndFollowUp`, with `HospiceQDM` 7.0.000, `PalliativeCareQDM` 6.0.000) | CMS (AIR) | Patient | 18+ on the encounter date | none (2014 had two populations) | 128 | `9a031bb8-3d9b-11e1-8634-00237d5bf174` / `d62e8d03-9687-4097-8351-1a2bad79c9b9` |
| Primary Caries Prevention Intervention as Offered by Dentists | CMS74v3 (CariesPrevent, _1, _2, _3) | **CMS74v16** (`CMS74DentalCariesPrevention`, with `HospiceQDM`) | CMS (AIR) | Patient | 1-20 at the start | 3 strata: 1-5, 6-12, 13-20 at the start | 379 | `0b81b6ba-3b30-41bf-a2f3-95bdc9f558f2` / `a79e99a5-8583-4041-8fa4-30c2d98c1d8a` |
| Children Who Have Dental Decay or Cavities | CMS75v2 (ChildCaries) | **CMS75v15** (`CMS75ChildrenDentalDecay`, with `HospiceQDM`) | CMS (AIR) | Patient; outcome, lower is better | 1-20 at the start | none | 378 | `61947125-4376-4a7b-ab7a-ac2be9bd9138` / `1e4f85db-b6c0-43d8-9d2b-594960bc5ac6` |
| Tobacco Use: Screening and Cessation Intervention | CMS138v2 (TobaccoCessation) | **CMS138v15** (`CMS138PreventiveTobaccoCessation`, with `HospiceQDM`) | NCQA | Patient | 12+ at the start | 3 population groups (screened; users given an intervention; both) | 226 (rate 2) | `e35791df-5b25-41bb-b260-673337bc44a8` / `f8fd38f2-e26c-48ad-a920-eff52f483956` |
| Weight Assessment and Counseling for Children and Adolescents | CMS155v2 (WeightChild_1_1 to _3_3) | **CMS155v15** (`CMS155WeightAssessandCounselforKids`, with `HospiceQDM`) | NCQA | Patient | 3-17 at the **end** | 3 groups (BMI percentile, height and weight; nutrition; physical activity) x 2 strata (3-11, 12-17 at the end) | 239 | `0b63f730-25d6-4248-b11f-8c09c66a04eb` / `63b284d2-a9b2-4f6d-86f9-40650e49a17b` |
| Controlling High Blood Pressure | CMS165v2 (BloodPressureManage) | **CMS165v15** (`CMS165ControllingHighBloodPressure`, with `AdultOutpatientEncountersQDM` 4.0.000, `AdvancedIllnessandFrailtyQDM` 11.0.000, `HospiceQDM`, `PalliativeCareQDM`) | NCQA | Patient; intermediate outcome | 18-85 at the **end** | none | 236 | `abdc37cc-bac6-4156-9b91-d1be2c8b7268` / `b83d59fa-65cd-4ccb-9889-70fbc81bcc6e` |

- Each measure's population IDs (for QRDA III) are in its HQMF file, `x/CMSNN-vMM.0.000-QDM.xml`. EHR-6a copies them for all seven
  measures into the catalog from there and records each file's SHA-256 in the port note. The NCQA three's copyright and disclaimer
  statements come from the same files (section 11.1).
- Every eCQI 2027 page reads "Reporting Options: Not yet available" for 2027, so the 2027 program use is not published yet.
- CMS68 and CMS138 are telehealth-eligible; CMS69, CMS74, CMS75 are not (measure pages).

### 4.2 The two dropped measures (owner decision 1)

| Measure | 2014 engine | Why it is dropped | Source |
|---|---|---|---|
| CMS127 Pneumococcal Vaccination Status for Older Adults | CMS127v2 (`QualityType2014.Pneumonia`) | CMS removed it from the eligible-clinician eCQMs from the 2024 performance period on; it is not in the 2027 table | eCQI, "Revised eCQM files and measures table, 2024 EC performance period", `https://ecqi.healthit.gov/revised-ecqm-files-and-measures-table-2024-ec-performance-period`; the 2027 table PDF; 2027 measure URLs return 404 |
| CMS147 Preventive Care and Screening: Influenza Immunization | CMS147v2 (`QualityType2014.Influenza`) | Removed with CMS127 and CMS161 from 2024 on; not in the 2027 table. Its 2024 version (v13) was withdrawn before reporting | The same eCQI notice; CY 2024 PFS final rule, 88 FR 78818, section C.4 (summary); ONC ticket CQM-6711 (summary) |

- There is no 2027 eCQM in their place. The only immunization eCQM in the 2027 table is CMS117 (childhood). Whether MIPS #110 survives in
  other collection types was not checked.
- Their data stays: vaccines (`vaccinepat`), the not-performed rows for influenza. What the not-performed window does with the influenza
  item is E-25.

---

## 5. How the 2027 measures are calculated

### 5.1 The approaches compared

| | A. C# translation of each CQL (recommended) | B1. A CQL engine for .NET (for example the Firely/NCQA CQL SDK) | B2. An ELM interpreter written here |
|---|---|---|---|
| What runs | One class per measure; one method per CQL `define`, named after it, in the order of the CQL file | The measure's ELM, compiled by the library | The measure's ELM JSON, walked by our own code |
| Data model | QDM 5.6 records built from the program's tables (section 5.3) | That SDK works on FHIR R4 resources (QI-Core), not QDM (not checked in detail). The program's data would have to become FHIR resources, and the measures their FHIR versions, which this plan has not checked exist for 2027 | QDM records, as A |
| Runtime | net48 today (business project, `LangVersion` 8.0, `OpenPmsDentalBusiness.csproj:10,14`) and .NET 10 later | Targets current .NET (not checked which); not net48. Would wait for the .NET 10 switch | Either |
| New library | none | Yes: an owner download decision, a licence check, notices, and the .NET stages' third-party rules | none |
| Middle Tier | The calculation runs on the server through `Meth`, as `GetEhrCqmData` does (`:2334-2336`) | Same, if the library loads on the server | Same |
| Per-patient reasons for the detail window | Natural: each define can say why a patient is in or out | Only what the engine exposes | Possible |
| Yearly update | Read the technical release notes and the CQL diff; change the methods and the catalog (about M per year) | Swap the ELM files; still map any new data element | Swap the ELM; implement any new operator |
| Risk | A misread CQL clause. Mitigated by one test per define and by the measures' own test cases (EHR-9) | Data-model mapping bugs; a large dependency | Hundreds of ELM operators; L-XL just to start |

Recommendation (E-22): **A**. Seven measures, about 25 defines each, are a bounded translation. B1 needs a new library, a FHIR mapping and
.NET 10 first, and it would still need the same data map. Reconsider B1 if CMS moves eligible-clinician reporting to FHIR measures.

### 5.2 The engine's shape (new files, version-neutral names)

All in `OpenPmsDentalBusiness/Db Multi Table/Cqm/` (the SDK-style business project compiles every `.cs` under its folder,
`OpenPmsDentalBusiness.csproj:2-7`, so no project file is edited). The EHR plan's section 6 still says new business classes go at the end
of existing files; that rule dates from before the project was globbed, and EHR-2 already added `Misc/ForkMigrations/EhrSteps.cs`. The
lead confirms the new folder before EHR-6a starts.

| File | Package | What |
|---|---|---|
| `CqmCatalog.cs` | EHR-6a | The release this build reports (label, publication date); per measure (all seven): CMS ID, version, title, steward, set ID, version-specific ID, population groups, strata and their HQMF IDs, scoring basis, improvement notation; for CMS138, CMS155 and CMS165 also NCQA's copyright and disclaimer statements, verbatim (section 11.1); their value sets (OID, name, QDM category, measures) as named constants; their direct-reference-code roles (section 7.3); the pick-list value sets of section 8 (OID and name, no measure). No code of any code system. The engine's `GetMeasures()` lists a catalog measure only when its class is in the build, so the window and QRDA show only measures that are calculated |
| `CqmCql.cs` | EHR-8a1 | The CQL semantics the measures use, at day precision: `AgeInYearsAt`, `during day of`, `overlaps day of`, `starts before or on day of`, `ends before day of`, `same day as`, `Interval` with open or closed ends, `NormalizeInterval`, `EarliestOf`, `HasEnd`, `Coalesce`, the "most recent" and "count of distinct dates" patterns. Pure functions, each tested against the CQL specification's examples |
| `CqmData.cs` | EHR-8a1 | The QDM records (section 5.3) for a set of patients, loaded in batches by `PatNum` with one query per table, value-set membership resolved from `EhrCodes` |
| `CqmLibraries.cs` | EHR-8a2 (Hospice, CQMCommon), EHR-8b (PalliativeCare), EHR-8d (AdvancedIllnessandFrailty, AdultOutpatientEncounters) | The shared CQL libraries, one static class each |
| `CqmEngine.cs` | EHR-8a2 | `GetMeasures()`; `Calculate(cmsId,dateStart,dateEnd,provNum)` through `Meth.GetObject` on a Middle Tier client. The whole result shape from the start: one row per population group and stratum, with counts and members (patients, or encounters for an episode-based measure), and each member's populations and reason. Strata (CMS74), groups (CMS138), groups x strata (CMS155) and episodes (CMS68) all fit it, so later packages add only measure classes |
| `CqmCms74.cs`, `CqmCms75.cs`, `CqmCms68.cs`, `CqmCms69.cs`, `CqmCms138.cs`, `CqmCms155.cs`, `CqmCms165.cs` | EHR-8a2-8d | One measure each |
| `CqmQrda3.cs`, `CqmQrda1.cs` | EHR-5b, EHR-5c1 (5c2 adds templates) | The writers |

- **No work tables.** The 2014 and 2011 engines created real tables; the 2027 engine works in memory, batch by batch. EHR-4a's
  `EhrMeasures.DropOldWorkTables` keeps its `tempehrquality` pattern, so a table a stopped old run left is still cleared.
- **Who is counted.** As in 2014: a patient (or, for CMS68, an encounter) enters through a qualifying encounter of the selected provider's
  records (every provider with the same last and first name, `QualityMeasures.cs:2338-2341`, the rule of the EHR plan's section 5). Every
  other datum of that patient counts whoever recorded it. CQL has no provider; this is how the program reports per clinician.
- **One thread, cancel between measures**, as EHR-4a's window does (`docs/port-notes/EHR-4.md`, section 2).
- **Middle Tier.** `DtoProcessor` finds a `Meth` class as the assembly name plus "." plus the class name (`Remoting/DtoProcessor.cs:353-359`),
  so `CqmEngine` and every class called through `Meth` are in the namespace `OpenPmsDentalBusiness`, not a sub-namespace. Result types are
  XML-serialisable (`SerializableDictionary`, not `Dictionary`), as the 2014 engine's were.
- **Value-set reads.** The engine and the role resolution call `EhrCodes.GetForValueSetOIDs(...,false)`. With `true`, codes are dropped
  when their code system's table is not loaded or does not exist (HL7 ActCode always; CPT until it is imported). The pick lists keep
  `true`.

### 5.3 The data map (QDM 5.6 datatype to the program's tables)

| QDM datatype (2027 use) | Program source | Timing taken | Notes |
|---|---|---|---|
| Encounter, Performed (every measure) | `encounter` (`CodeValue`, `CodeSystem`, `DateEncounter`, `ProvNum`) | the day of `DateEncounter` | Plus completed procedures whose code is in the encounter value set (E-24) |
| Encounter class (CMS69: not virtual) | none | — | Nothing is marked virtual, so every encounter counts; virtual visits appear only through codes in "Virtual Encounter" (CMS138, CMS165) |
| Procedure, Performed (CMS74 fluoride varnish; CMS165 kidney transplant and dialysis services) | `procedurelog` with status complete (2) joined to `procedurecode` (`ProcDate`), as `QualityMeasures.cs:3868-3929`. For CMS165 also `intervention` rows coded in "Kidney Transplant" or "Dialysis Services" and not declined (E-28) | the day of `ProcDate`, or of the intervention's `DateEntry` | CDT and CPT codes as the office's procedure codes hold them. CMS165 reads these two value sets only as Procedure, Performed (`CMS165ControllingHighBloodPressure-15.0.000.cql:57-60`); the intervention rows are a listed deviation |
| Procedure or Intervention, Performed (CMS68 the current-medications documentation code) | `ehrmeasureevent` with `EventType` CurrentMedsDocumented (`EhrMeasureEvent.cs:67`) and `CodeValueEvent` the role's code, as `QualityMeasures.cs:3710-3734`; also `intervention` rows with that code, not declined | the day of `DateTEvent` | A point in time, so "completed" (`HasEnd`) holds |
| Diagnosis (CMS75 caries, pregnancy, hospice, palliative, frailty, advanced illness, ESRD, CKD 5, hypertension, overweight, underweight, the ICD-10-CM direct codes) | `disease` joined to `diseasedef` (`SnomedCode`, `Icd10Code`, `ICD9Code`), as `:3609-3708`. For "Overweight or Obese" and "Underweight" also `vitalsign.WeightCode` when it is in that value set (E-35) | prevalence `DateStart`..`DateStop` by the status rule below (E-26); author time E-26. A `WeightCode` diagnosis runs from its `DateTaken` to the next BMI vital sign's `DateTaken` (open if none) | **Status rule.** `ProbStatus` (Active, Resolved, Inactive) decides whether a problem is active, and `DateStop` is minval while it is (`TableTypes/Disease.cs:25-30`). The edit window saves status and dates separately (`FrmDiseaseEdit.xaml.cs:127`, `178`, through `Diseases.SetDiseaseFields`, `Diseases.cs:284-292`), so a Resolved or Inactive row can have no `DateStop`. Such a row ends on its `DateTStamp` date (recommended, E-26; on its `DateStart` day only if `DateTStamp` falls before `DateStart`); the alternative is prevalent on its `DateStart` day only. A row with a `DateStop` ends there whatever its status; an Active row with none is open at the end. An unset `DateStart` (minval, "if unknown", `Disease.cs:27-28`) is open at the start, as the 2014 engine read it (`QualityMeasures.cs:3646`); for "authored" E-26 takes `DateTStamp` instead. Plus ICD-10 diagnostic codes on completed procedures (E-26). The program's BMI entry never creates a problem: it stores the over- or underweight finding as `vitalsign.WeightCode` (`Vitalsigns.cs:717-733`, `FormVitalsignEdit2014.cs:821`, `EFormVitalSigns.cs:330-334`, `TableTypes/Vitalsign.cs:33-34`) |
| Symptom (CMS165 frailty symptom) | `disease`, as a diagnosis | as above | The program has no separate symptom record |
| Intervention, Order and Performed (CMS69 follow-up, CMS155 counseling, CMS138 counseling, hospice and palliative care) | `intervention` (`CodeValue`, `CodeSystem`, `DateEntry`) with `IsPatDeclined` false | the day of `DateEntry`, as both an order and a performance (as 2014 took it) | Counted by code, whatever its `CodeSet` (E-28). The `reason` attribute is never present, so CMS69's "reason in Overweight or Obese" branch is always false. Its diagnosis branch needs a `disease` row or E-35's `WeightCode` |
| Patient-declined intervention (`intervention.IsPatDeclined`, `TableTypes/Intervention.cs:26`), including declined medications stored as interventions (`FormInterventionEdit.cs:197`, `:232`) | `intervention` with `IsPatDeclined` true | — | **Never an Order or a Performed.** At most it is Not Ordered or Not Performed with a patient reason and no coded rationale. No 2027 criterion this program reads accepts that: CMS69's follow-up exception needs "Medical Reason", CMS138 has no exceptions, and CMS69's "Patient Declined" applies only to the BMI exam (`ehrnotperformed`). Listed as a deviation (the 2014 engine kept declined pharmacotherapy, `QualityMeasures.cs:3370-3378`) |
| Physical Exam, Not Performed (CMS69 BMI exam); Intervention, Not Performed (CMS68 current medications), with negation rationale | `ehrnotperformed` (`CodeValue`, `CodeValueReason`, `DateEntry`), items BMIExam and DocumentCurrentMeds | author time = `DateEntry` | |
| Intervention, Not Ordered / Not Performed; Medication, Not Ordered (CMS69 follow-up exception) | **none** today. `EhrNotPerformedItem` has four items: BMI exam, influenza vaccination, tobacco screening, current medications (`TableTypes/EhrNotPerformed.cs:35-43`, `FormEhrNotPerformedEdit.cs:30-46`), and an intervention has no reason code | — | "Has Medical Reason For Not Documenting A Follow Up Plan For Low Or High BMI" (`CMS69BMIScreenAndFollowUp-15.0.000.cql:163-173`) is never true today. E-34: a new not-performed item fills it from `ehrnotperformed` (Medication, Not Ordered for an RxNorm code; otherwise Intervention, Not Ordered and Not Performed) |
| Physical Exam, Performed: BMI (CMS69) | `vitalsign` with `Height`>0 and `Weight`>0, BMI computed from them, as `:3773-3828` ("assumed" to be the BMI exam, as 2014 did) | the day of `DateTaken` | Every BMI of the period counts (2027), not only the latest |
| Physical Exam, Performed: height, weight, BMI percentile (CMS155) | `vitalsign` with `HeightExamCode`, `WeightExamCode`, `BMIExamCode` in the Height, Weight and BMI percentile value sets and a value (`Height`>0, `Weight`>0, `BMIPercentile`>=0); by E-35 also a row with the value and an empty exam code | the day of `DateTaken` | Three separate exams anywhere in the period (2014 needed one row with all three). The windows record a percentile only at ages 3-16 before January 1 of the exam year (`FormVitalsignEdit2014.cs:48`, `305`; `EFormVitalSigns.cs:314-315`) and write an exam code only when the LOINC table holds it (`FormVitalsignEdit2014.cs:314`, `Vitalsigns.cs:705`, `EFormVitalSigns.cs:306-325`); CMS155 takes ages 3-17 at the end of the period (`CMS155WeightAssessandCounselforKids-15.0.000.cql:101-104`). EHR-7 moves the windows to the age on `DateTaken` (E-35) |
| Physical Exam, Performed: blood pressure (CMS165) | `vitalsign.BpSystolic`, `BpDiastolic` (>0), unit mm[Hg] | the day of `DateTaken` | Readings on a day with an `encounter` row in "Encounter Inpatient" `2.16.840.1.113883.3.666.5.307` or "Emergency Department Evaluation and Management Visit" `2.16.840.1.113883.3.464.1003.101.12.1010` are left out, as the CQL says (`CMS165ControllingHighBloodPressure-15.0.000.cql:107-115`) |
| Assessment, Performed: tobacco screening (CMS138) | `ehrmeasureevent` with `EventType` TobaccoUseAssessed (`EhrMeasureEvent.cs:63`): `CodeValueEvent` in "Tobacco Use Screening", `CodeValueResult` in "Tobacco User" or "Tobacco Non User", as `:3275-3349` | the day of `DateTEvent` | |
| Assessment, Performed: hospice MDS, housing status, FACIT-Pal, medical equipment used | none | — | Always absent (E-27) |
| Medication, Active (CMS69 v15, CMS138 pharmacotherapy, CMS165 dementia medications) | **every** `medicationpat` (RxCui of the row or of its `medication`), whatever its `PatNote`, which is a free dosage note (`TableTypes/MedicationPat.cs:15-17`) | `DateStart`..`DateStop`; an unset `DateStop` is ongoing (open at the end, `MedicationPat.cs:23-24`); an unset `DateStart` is open at the start, as for problems | The 2014 rule (`PatNote` makes an order, `:3442`) is not kept. CMS138's active-pharmacotherapy definition needs the interval to fall **during** the window from six months before the period to its end (`CMS138PreventiveTobaccoCessation-15.0.000.cql:50-52`), so a medication with no stop date never qualifies there; CMS69 and CMS165 test overlap, which it can pass |
| Medication, Order (CMS69, CMS138) | the same `medicationpat` rows, read a second time, by E-39 (recommended: every row with a set `DateStart`; the intervention window's orders carry no other mark, `FormInterventionEdit.cs:209-214`) | author time = `DateStart` | A row the patient reported is then an order on its start date; the deviation list says so. A row with no `DateStart` is active only |
| Device, Order or Applied (CMS165 frailty device) | none | — | Always absent (E-27) |
| Encounter, Performed with discharge disposition (hospice discharge) | none (no inpatient stays) | — | Always absent |
| Patient characteristics | `patient.Birthdate`, `patient.Gender` | | Federal Administrative Sex from `Gender` (male, female; others have no sex code in the SDE) |
| SDE race and ethnicity | `patientrace` (`CdcrecCode`, `IsEthnicity`) against "Race" `2.16.840.1.114222.4.11.836` and "Ethnicity" `.837` | | As `:3222-3273` |
| SDE payer | `payortype` (`SopCode`, `DateStart`; `PayorTypes.GetCurrentType`) against "Payer Type" `2.16.840.1.114222.4.11.3591` | | 2027 v15/v16 deleted 42 SOP codes from that set (TRNs); the import decides. QRDA III's payer grouping is section 9.2 |

Everything marked "none", and every row marked as a deviation, is listed in the window's notes for each measure and in the known-deviation
list of each EHR-8 port note. The deviations of E-24 (one day, one encounter) are listed there too.

### 5.4 Each measure: what it needs and where it lives

Population names are the CQL `define` names.

- **CMS68v16 (EHR-8b).** Episode-based: every member is an encounter.
  - "Initial Population" = "Denominator": each Encounter, Performed in "Encounter to Document Medications" `2.16.840.1.113883.3.600.1.1834`
    during the period. No age limit (dropped in v14), so the birthdate is not read (section 6).
  - "Numerator": a current-medications documentation (the CMS68 SNOMED CT direct code, role `Cms68MedsDocumented`) on the encounter's
    day: `ehrmeasureevent` CurrentMedsDocumented or an `intervention`.
  - "Denominator Exceptions": the same code not performed on that day, with a reason in "Acute Health Crisis"
    `2.16.840.1.113762.1.4.1248.429` (`ehrnotperformed`).
  - No exclusions, no strata. Telehealth-eligible.
- **CMS69v15 (EHR-8b).**
  - "Initial Population": a qualifying encounter ("Encounter to Evaluate BMI" `2.16.840.1.113883.3.600.1.1751`, not virtual) at age 18 or
    more on that day. One population (2014 had two, with different normal ranges).
  - "Denominator Exclusions": hospice (`HospiceQDM`), palliative care in the period (`PalliativeCareQDM`), or a pregnancy diagnosis
    ("Pregnancy" `2.16.840.1.113883.3.526.3.378`) overlapping the period.
  - "Numerator": normal BMI (18.5 to under 25, and no high or low BMI in the period), or a high or low BMI with follow-up in the period,
    with a diagnosis of "Overweight or Obese" `2.16.840.1.113762.1.4.1047.502` or "Underweight" `.526.3.1563` active at the follow-up's
    date (started on or before it and not ended before it, CQL:92-113). The follow-up is one of three things. It can be an intervention
    order in "Follow Up for Above Normal BMI" `.600.1.1525` (or "Below" `.600.1.1528`) or in "Referrals Where Weight Assessment May Occur"
    `.600.1.1527`. It can be a medication order in "Medications for Above/Below Normal BMI" `2.16.840.1.113883.3.526.3.1561`/`.1562`. Or
    it can be a performed intervention in "Follow Up for Above/Below Normal BMI" only: "High BMI Interventions Performed" (CQL:103-109)
    does not take referrals, which count as orders. A fourth route is an active medication of those sets with such a diagnosis (new in
    v15). In this program that diagnosis is a `disease` row or E-35's `WeightCode`.
  - "Denominator Exceptions", on a qualifying encounter's day: the BMI exam not performed with "Medical Reason" `.526.3.1007` or "Patient
    Declined" `.526.3.1582` (CQL:175-180); a follow-up (intervention or medication) not ordered or not performed with "Medical Reason" only
    (CQL:163-173). A follow-up the patient declined is no exception. The follow-up branch has no data until E-34.
  - BMI is computed from height and weight. The program stores inches and pounds (the 2014 engine's formula, `:3790-3828`, is reused).
- **CMS74v16 (EHR-8a2).**
  - "Initial Population": age 1-20 at the start of the period and an Encounter, Performed in "Clinical Oral Evaluation"
    `2.16.840.1.113883.3.464.1003.125.12.1003`. That value set holds CDT codes, which offices record as completed procedures (E-24).
  - "Denominator Exclusions": hospice.
  - "Numerator": fluoride varnish ("Fluoride Varnish Application for Children" `.125.12.1002`, `procedurelog`) on **two or more distinct
    dates** in the period (2014 needed one).
  - Three strata by age at the start: 1-5, 6-12, 13-20. No exceptions.
- **CMS75v15 (EHR-8a2).**
  - "Initial Population": as CMS74. "Denominator Exclusions": hospice.
  - "Numerator": a diagnosis in "Dental Caries" `.125.12.1004` authored in the period (v15; 2014 took one active at any time in it). Which
    date stands for "authored" is E-26.
  - Outcome measure: a lower rate is better. No strata, no exceptions.
- **CMS138v15 (EHR-8c, NCQA).**
  - "Initial Population": age 12+ at the start, and two or more qualifying visits in the period or one preventive visit in the period
    (the CQL's two visit definitions, over the encounter value sets the CQL lists, 19 of them, among them Telephone Visits and Virtual Encounter,
    plus four CPT direct codes, role `Cms138QualifyingVisitCpt`). The CQL counts encounter records, so two encounters on one day count
    twice; E-24's "one day, one encounter" departs from that (deviation list).
  - Three groups: "Denominator 1" = all; "Denominator 2" = those whose most recent screening in the period says tobacco user; "Denominator
    3" = all. "Denominator Exclusion": hospice. No exceptions (v15 removed them).
  - "Numerator 1": the most recent screening in the period has a user or non-user result. "Numerator 2": counseling ("Tobacco Use
    Cessation Counseling" `.526.3.509`, or the ICD-10-CM counseling direct code as a diagnosis, role `Cms138CounselingDx`), a
    pharmacotherapy order ("Tobacco Use Cessation Pharmacotherapy" `.526.3.1190`), or active pharmacotherapy, from six months before the
    period to its end. "Numerator 3": non-user, or user with one of those. Declined counseling or medication counts for none.
  - MIPS scores rate 2. Telehealth-eligible.
- **CMS155v15 (EHR-8c, NCQA).**
  - "Initial Population": age 3-17 at the **end** of the period and a qualifying encounter (Office Visit, Preventive Care counseling and
    0-17 sets, Home Healthcare, Telephone Visits; Face-to-Face Interaction dropped).
  - The guidance limits the visit to primary-care and obstetric-gynecologic clinicians (`CMS155-v15.0.000-QDM.html`, in summary). The
    CQL does not test it, but the
    window's notes say that the measure's intended clinicians are not dentists.
  - "Denominator Exclusions": hospice, or pregnancy overlapping the period.
  - "Numerator 1": BMI percentile, height and weight exams, each with a result, anywhere in the period. "Numerator 2": "Counseling for
    Nutrition" `.195.12.1003`. "Numerator 3": "Counseling for Physical Activity" `.118.12.1035` (interventions, not declined).
  - Two strata by age at the end: 3-11, 12-17 (2014: 3-11 and 12-16 at the start).
- **CMS165v15 (EHR-8d, NCQA).**
  - "Initial Population": age 18-85 at the **end**, "Essential Hypertension" `.104.12.1011` overlapping the first six months, and a
    qualifying encounter from `AdultOutpatientEncountersQDM`.
  - "Denominator Exclusions": hospice; frailty with advanced illness or dementia medications (66-80); frailty alone (81+); long-term
    nursing home (66+, an assessment: always absent here); palliative care; ESRD, CKD stage 5 or the kidney-transplant-status direct code
    (role `Cms165KidneyTransplantDx`) starting on or before the period's end; kidney transplant or dialysis services on or before the end
    (Procedure, Performed: `procedurelog`, and intervention rows by E-28); pregnancy overlapping the period.
  - "Numerator": on the most recent day of the period with both a systolic and a diastolic reading (not during inpatient or ED), the lowest
    systolic under 140 and the lowest diastolic under 90. No reading means not controlled.

---

## 6. Measurement period, strata, exclusions and exceptions

- **The period.** The 2027 specifications fix January 1 to December 31, 2027 (HQMF `measurePeriod` 20270101 closed to 20280101 open,
  `CMS68-v16.0.000-QDM.xml:140-149`). The CQL takes it as a parameter, so the logic works for any interval. The window offers a reporting
  year (default: this calendar year) and custom dates (E-29).
- **Day precision.** The program stores dates (and some date-times); every comparison is "during day of" or its relatives, as the CQL
  writes them. `CqmCql` implements the operators once.
- **Age.** `AgeInYearsAt` as CQL defines it (whole years, birthday reached on the anniversary day), at the anchor each measure names
  (section 4.1). For the measures with an age criterion (CMS69, CMS74, CMS75, CMS138, CMS155, CMS165), a patient with no valid birthdate
  (year before 1880, as `QualityMeasures.cs:2351` tests) is in no population. CMS68 has no age criterion (`CMS68DocumentationCurrentMeds-
  16.0.000.cql:48-49`): its encounters count whatever the birthdate. The 2014 rule came from the shared encounter query of a version
  set in which every measure had an age limit.
- **Order of populations (proportion measures).** Initial population, denominator, denominator exclusions, numerator, denominator
  exceptions, as the QDM measures define: an excluded member is not counted in the numerator; an exception is counted only for a member
  that is in the denominator, not excluded and not in the numerator.
- **Performance rate.** Numerator / (denominator − exclusions − exceptions), shown with one decimal; "lower is better" for CMS75. Strata
  and groups are rows of their own.
- **Supplemental data.** Ethnicity, payer, race and sex counts for every population and stratum (QRDA III needs them).

| Measure | Exclusions | Exceptions | Strata or groups |
|---|---|---|---|
| CMS68v16 | none | acute health crisis | none |
| CMS69v15 | hospice, palliative care, pregnancy | BMI exam not performed: medical reason or patient declined; follow-up not ordered or not performed: medical reason only (E-34) | none |
| CMS74v16 | hospice | none | 3 strata |
| CMS75v15 | hospice | none | none |
| CMS138v15 | hospice | none | 3 groups |
| CMS155v15 | hospice, pregnancy | none | 3 groups x 2 strata |
| CMS165v15 | hospice, frailty and advanced illness, frailty 81+, nursing home, palliative care, ESRD/CKD 5/transplant, dialysis or transplant procedures, pregnancy | none | none |

---

## 7. Value sets: what changes in EHR-2's importer

### 7.1 The release

- The office downloads, from VSAC's Downloadable Resources (`https://vsac.nlm.nih.gov/download/ecqm`, UMLS sign-in, the office's own
  licence), the release "eCQM Value Sets for Eligible Clinicians" published 2026-05-14 for the 2027 performance period, sorted by CMS ID,
  in Excel or SVS; plus that release's "Direct Reference Codes" and "Code System Versions" files (owner decision 3).
- The release replaces EHR-2's June 2013 EP release. `RELEASE_LABEL` (`EhrCodes.cs:548`) becomes the 2026-05-14 release, with the expansion
  profile or version that the files name (recorded in E-20's step). Done in EHR-6a-R: "VSAC eCQM Value Sets (EC, HOSPIP and HOSPOP),
  eCQM Update 2026-05-14, 2027 performance period".
- **One pick, one swap.** The office picks the three files together. The importer parses all three, shows one report, and writes the
  value-set rows and the direct-code rows in one swap (EHR-2's scratch table and `RENAME TABLE`, `EhrCodes.cs:1775-1782`). A pick without
  all three is refused before anything changes. A later import replaces both kinds together. So the two kinds of row never come from
  different releases, and no import deletes the other kind's rows: today's `Import` "replaces every row of ehrcode"
  (`EhrCodes.cs:536-542`), and `EhrValueSetRelease` holds one label, one file and one SHA-256 (`:522-535`). The release record gains the
  three files' names and SHA-256 and the code-system versions (section 7.4).
- **First step, documentation only (E-20).** Someone with a UMLS licence opens the three files once and records in the port note: file
  names and formats; the sizes of each file and of each workbook sheet uncompressed; sheet names and the rows above the headers; the header
  names exactly; how code cells are stored; one SVS file's element and `Group` names; the two other files' headers; and the version string
  of each code system in the Code System Versions file. Nothing from them enters the repository. The parsers' header names (`_dictHeaders`,
  `EhrCodes.cs:701`) are fixed from that record. EHR-6a may be written before the record, with provisional names, as EHR-2 was; it merges
  after it. This replaces EHR-2's step 5 for June 2013, which is no longer needed.

### 7.2 The manifest and the version check

- **The manifest** (`_arrayManifest`, `EhrCodes.cs:617-698`, 79 OIDs of the 2014 measures) is built from the catalog: the value sets of the
  measures in the build plus the pick-list value sets of section 8. The seven measures' CQL files declare **68 distinct value sets outside
  `CQMCommonQDM`**, counted from their `valueset` lines only (the packages' `x/cql/*.cql`; the five `codesystem` OIDs of HL7 ActCode,
  LOINC, CPT, ICD-10-CM and SNOMED CT are not value sets). `CQMCommonQDM` declares five, for its hospital functions, which the seven do not
  call. One of them, "Encounter Inpatient" `2.16.840.1.113883.3.666.5.307`, is among the 68, because `HospiceQDM` and CMS165 declare it
  themselves. So the CQL files hold 72 distinct value sets in all. EHR-6a lists each value set with the measures that use it, and leaves
  out the ones declared but unused, with a note.
- Compared with the engine: of the 81 value-set OID literals of `QualityMeasures.cs` (code-system and template OIDs left out), 41 are not in
  the 2027 measures, and 28 of the 68 are new (a comparison of those literals with the CQL `valueset` lines; EHR-6a gives the exact lists).
- **All seven at once.** The catalog carries the seven measures' value sets and the pick-list value sets from EHR-6a on (owner decision
  5 undid the split that kept the NCQA three's part apart). The tobacco and counseling sets the windows use are also pick-list sets, by
  OID and name only, as today's source already names them. So one import serves every later package, whatever E-30 decides.
- **The version check** (`Finish`, `EhrCodes.cs:1592`, the check at 1678-1716; `ArrayMeasureIds`, `:571-573`) compares the file's CMS IDs
  with the catalog's measures: CMS68v16, CMS69v15, CMS74v16, CMS75v15, CMS138v15, CMS155v15 and CMS165v15. A missing one, or another
  version (for example the 2026 release's CMS74v15), gives a warning that names it. A file with none of them at its 2027 version is
  refused, naming the release this program needs. A file that names no measure is checked by OIDs, as today. `MeasureIds` keeps only the
  measures this program reports (EHR-2's fix round, item 1). The report reads "7 of 7 measures at the 2027 versions".
- **The test that holds the manifest to the source** (`ValueSetSourceTests`) changes. The catalog holds the seven measures of section 4.1.
  The catalog's per-measure OID lists equal the CQL files' `valueset` lines and nothing else. Every value-set OID literal in the program's
  source is in the catalog file and nowhere else, with a shrinking allowlist: the 2014 engine until EHR-6b, the data-entry
  windows until EHR-7, then empty.
- **Size.** The 2026-05-14 release covers about 49 measures of the 2027 table. The June 2013 EP release, which EHR-2's limits were set for
  (64 MB per part, 256 MB in all, 1,000,000 rows, `EhrCodes.cs:552-557`), covered about 64 EP measures: EHR-2's own comment says it lists the
  supplemental value sets "under about 64 measures of the release" (`EhrCodes.cs:1629-1635`). So the 2027 release is not larger for having
  more measures. Any growth comes from larger value-set expansions (more codes per set, newer code-system versions). A workbook sorted by
  CMS ID repeats the supplemental value sets under every measure. E-20's record gives the real sizes, and they decide E-30. EHR-6a then
  does one of two things. It keeps only the rows of the manifest's value sets as it reads, so memory stays small and the other measures'
  value sets are not imported (a change from EHR-2's "every value set is imported"). Or it raises the per-part limit, which the streaming
  reader allows.

### 7.3 Direct reference codes without codes in the repository

The seven measures' CQL declares 20 direct reference codes (counted from the `code` lines of the CQL files): LOINC 7, SNOMED CT 6, CPT 4,
ICD-10-CM 2, HL7 ActCode 1. The engine needs a few of them by role, and the QRDA I writer needs the codes of the exams it writes.

- **Storage.** Each code of the office's Direct Reference Codes file becomes an `ehrcode` row: `ValueSetOID` a fork key `drc:<CMS ID>:<role>`,
  `QDMCategory` "Direct Reference Code", the rest from the file. No schema change; `GetForValueSetOIDs` finds them by the key (with `false`,
  section 5.2). They are written in the same swap as the value-set rows (section 7.1).
- **Roles.** The catalog names each role by measure, code system and meaning, never by code or code name. The importer resolves a role
  from the office's file. Every role, the NCQA measures' among them, is in `CqmCatalog` (EHR-6a).

| Role (catalog) | Measure | Code system | Used for | How the row is found | Catalog file |
|---|---|---|---|---|---|
| `Cms68MedsDocumented` | CMS68 | SNOMED CT | numerator and exception (section 5.4); what the medical window writes and filters by (section 8) | the only SNOMED CT direct code of CMS68 | `CqmCatalog` |
| `Cms69Bmi` | CMS69 | LOINC | BMI exam not performed (pick list and labels); QRDA I | E-23 | `CqmCatalog` |
| `Cms69Virtual` | CMS69 | HL7 ActCode | the "not virtual" test | the only ActCode code | `CqmCatalog` |
| `Cms138QualifyingVisitCpt` (4 codes) | CMS138 | CPT | qualifying visits | every CPT direct code of CMS138 (one role) | `CqmCatalog` |
| `Cms138CounselingDx` | CMS138 | ICD-10-CM | counseling as a diagnosis | the only ICD-10-CM direct code of CMS138 | `CqmCatalog` |
| `Cms165Systolic`, `Cms165Diastolic` | CMS165 | LOINC | QRDA I | E-23 | `CqmCatalog` |
| `Cms165KidneyTransplantDx` | CMS165 | ICD-10-CM | exclusion | the only ICD-10-CM direct code of CMS165 | `CqmCatalog` |
| hospice discharge (2), hospice MDS and its "Yes" result, FACIT-Pal, housing status, nursing home, medical equipment, one declared but unused in CMS69 | several | SNOMED CT, LOINC | criteria with no data here (section 5.3) | imported and listed; no role needed | — |

- LOINC is the one code system where a measure has several codes of different meaning that the program needs (CMS69 has three LOINC codes,
  CMS165 six). E-23 chooses how those are told apart.
- A role that matches no row, or several, gives a warning naming the role; the measure then marks the criterion "not calculated".

### 7.4 The Code System Versions file

- Read and recorded in `ForkEhrValueSetRelease` (code system and version), in the same import as the other two files.
- The Code-System Status grid (`EhrCodes.GetCodeSystemStatuses`, `EhrCodes.cs:188`; `FormCodeSystemsImport.cs:91`) gains a column "In the
  release" next to the loaded version (`codesystem.VersionCur`, `TableTypes/CodeSystem.cs`). The release's version strings and
  `VersionCur` do not use one format; each code system writes its versions its own way. So EHR-6a maps each code system's two formats to
  a comparable value, from E-20's record of the file and the program's own `VersionCur` values. It flags a loaded table as older only where
  that mapping exists, and shows both strings where it does not. The flag means that codes of the value sets newer than the office's table
  are missing from its pick lists.
- The QRDA writers take code-system versions from it where the guides ask for them (section 9).

### 7.5 What the office does (replaces the EHR plan's 8.4)

- It holds its own UMLS licence and sends NLM the yearly usage report.
- It downloads the three files of the 2026-05-14 release.
- It imports them in Setup > Chart > EHR > Code System Importer (Setup permission), all three in one pick (section 7.1). The catalog
  holds all seven measures from EHR-6a on, so no later package needs a second import.
- It loads its code-system tables (E-17 unchanged).

---

## 8. Pick lists and data entry (EHR-7)

The data-entry windows name 2014 value sets directly. After a 2027 import those OIDs are empty unless the 2027 measures still use them.
Several windows also hard-code the codes the measures read, or sort rows by 2014 code lists. EHR-7 owns both.

**Value-set OIDs.**

| Where | 2014 OID (name) | In 2027? | Becomes |
|---|---|---|---|
| `FormInterventionEdit.cs:31,39,33,41` | `.600.1.1525`, `.1528`, `.1527` (BMI follow-up, referrals) | yes (CMS69) | catalog constants |
| `FormInterventionEdit.cs:35,43`, `FormInterventions.cs:101`, `FormVitalsignEdit2014.cs:394` | `.600.1.1498`, `.1499` (BMI medications) | no | "Medications for Above/Below Normal BMI" `.526.3.1561`, `.1562` |
| `FormInterventionEdit.cs:47,49` | `.195.12.1003`, `.118.12.1035` (counseling) | yes (CMS155) | catalog constants (pick-list part) |
| `FormInterventionEdit.cs:53,55`, `FormInterventions.cs:101`, `FormMedical.cs:937-940,1037`, `FormEhrPatientSmoking.cs:158-161,238` | `.526.3.509`, `.1190` (tobacco) | yes (CMS138) | catalog constants (pick-list part) |
| `FormInterventionEdit.cs:59,61` | `.109.12.1016`, `.1015` (dialysis education, other dialysis services) | no | "Dialysis Services" `.109.12.1013` (CMS165), read as Procedure, Performed by E-28 |
| `FormMedical.cs:879,899,901`, `FormEhrPatientSmoking.cs:108,125,127` | `.526.3.1278`, `.1170`, `.1189` (tobacco screening, user, non-user) | yes (CMS138) | catalog constants (pick-list part) |
| `FormEhrNotPerformedEdit.cs:32` (BMI exam) | `.600.1.681` | no | the `Cms69Bmi` direct code |
| `FormEhrNotPerformedEdit.cs:35` (influenza vaccination) | `.526.3.402`, `.1254` | no (CMS147 retired) | E-25 |
| `FormEhrNotPerformedEdit.cs:41` (tobacco screening) | `.526.3.1278` | yes | catalog constant (CMS138v15 has no exceptions; the item stays) |
| `FormEhrNotPerformedEdit.cs:44` (current medications) | `.600.1.462` | no | the `Cms68MedsDocumented` direct code |
| `FormEhrNotPerformedEdit.cs:101-106` (reasons) | `.526.3.1007` yes; `.1008`, `.1009`, `.600.1.1503`, `.1502`, `.526.3.1259` no | partly | per item: CMS68 "Acute Health Crisis" `.1248.429`; CMS69 BMI exam "Medical Reason" `.526.3.1007`, "Patient Declined" `.526.3.1582`; the new follow-up item (E-34) "Medical Reason" only; the influenza item E-25 |
| New item in `FormEhrNotPerformedEdit.cs` (E-34) | — | — | "BMI follow-up": codes from `.600.1.1525`, `.600.1.1528`, `.600.1.1527`, `.526.3.1561`, `.526.3.1562` |
| `DiseaseDefs.cs:404` (pregnancy problems, "in all sets") | `.526.3.378` yes, `.600.1.1623` no | partly | "Pregnancy" `.526.3.378` alone |

**Code literals and code-list sorting** (the codes themselves are not repeated here).

| Where | What it hard-codes | Becomes |
|---|---|---|
| `FormMedical.cs:1350` | writes the CMS68 SNOMED CT direct code into `ehrmeasureevent` when "current medications documented" is Yes | the `Cms68MedsDocumented` role's code from the import; with no role resolved, the window says to import the release and writes nothing |
| `FormMedical.cs:104` | filters the not-performed rows by that code | the same role |
| `FormEhrNotPerformed.cs:52-66` | labels rows by the BMI, current-medications and three tobacco code literals; every other row is labelled "influenza" | labels by role (`Cms69Bmi`, `Cms68MedsDocumented`), by catalog membership ("Tobacco Use Screening", the follow-up sets of E-34) and by E-25's CVX list; anything else is "other", not "influenza". Each row keeps its item, or null for "other", in the row's `Tag`. The double-click handler reads the `Tag` instead of parsing the label: today it runs `Enum.Parse` on the label text (`FormEhrNotPerformed.cs:123`), so an "other" row would throw. An "other" row opens the edit window read-only (code, reason, date and note shown; no save), and a row whose code resolves to an item opens with that item, so `FormEhrNotPerformedEdit.cs:30-46` never gets the index -1 for a saved row |
| `FormInterventions.cs:120-125` | labels medication rows by the RxCui literals of the 2014 BMI medication sets; everything else is "tobacco" | labels by which imported value set holds the RxCui (`.526.3.1561`, `.1562`, `.526.3.1190`) |
| `Vitalsigns.cs:682-691` (`GetLoincsBMIExam`, `GetLoincsBMIPercentile`), `:694-714` (`SetFields`), `EFormVitalSigns.cs:306-325`, `FormVitalsignEdit2014.cs:314,343,507,524` | LOINC codes of the BMI, BMI percentile, height and weight exams | kept (E-37): LOINC's licence allows them; the engine reads them by value-set membership, never by literal |
| `Vitalsigns.cs:717-733` (`SetWeightCodes`), `EFormVitalSigns.cs:327-334` | the two SNOMED CT over- and underweight codes written to `vitalsign.WeightCode` | kept until counsel's E-8 says otherwise (E-35); the engine reads `WeightCode` by membership in "Overweight or Obese" and "Underweight" |
| `Vitalsigns.cs:116-136` (`GetBMIInterventionCode`) | the 2014 normal BMI ranges: 18.5 to under 25 below 65, 23 to under 30 from 65 on | the 2027 range for every adult, 18.5 to under 25 (E-35), so `WeightCode` and the offered follow-up list match CMS69v15 |
| `FormVitalsignEdit2014.cs:17`, `48`, `305-306`, `328`, `335`, `350`, `532`, `688`, `708`; `EFormVitalSigns.cs:314-315`, `329`; `Vitalsigns.cs:116-117` | the age "before January 1" of the exam year (the year difference minus one): the BMI percentile only at 3-16 by it, the over- or underweight class from 18 by it, the child counseling group under 17 by it, so a 17-year-old is in neither group | the age on `DateTaken` (whole years, as CQL's `AgeInYearsAt`, E-35): the percentile at 3-17, which the LMS table covers (36 to 215 months, `GetListLMS`, `Vitalsigns.cs:140`), the class at 18 and over, the child group under 18. `GetBMIPercentile` (`Vitalsigns.cs:621-655`) returns -1 instead of throwing when no LMS row matches |
| `FormEhrSettings.cs:105-117`, `FormEncounterTool.cs:43-55` | nine recommended encounter codes (SNOMED CT) | the SNOMED CT codes of the office's imported qualifying-encounter value sets that its `snomed` table holds (E-32). The nine literals go (fewer licensed literals, E-8) |
| `FormEhrSettings.cs:120-133` | nine recommended pregnancy codes (SNOMED CT) for `PregnancyDefaultCodeValue`, picked for the 2014 pregnancy sets | the SNOMED CT codes of the imported "Pregnancy" `.526.3.378` that the `snomed` table holds (E-32). The nine literals go |

After EHR-7 the allowlist of section 7.2 is empty: no value-set OID literal outside the catalog files. The code literals left are those of
E-35 and E-37.

---

## 9. QRDA: what EHR-5 becomes

The EHR plan's EHR-5 reopened the 2014 and 2011 windows and fixed their QRDA identity. With both versions gone (owner decisions 2 and 4),
it becomes four packages: EHR-5a (the window), EHR-5b (QRDA III), EHR-5c1 and EHR-5c2 (QRDA I). The EHR plan's 2011 part (EHR-5d in the
first draft of this plan) has nothing left to do. The window comes after EHR-8a2 and shows the measures calculated so far; the QRDA
packages come after EHR-8d, so they are written and tested for all seven measures at once.

### 9.1 The guides (not opened; download batch Q, section 15)

| Guide | For | Where |
|---|---|---|
| CMS 2027 QRDA III Implementation Guide for Eligible Clinicians (and its schematron and sample file) | EHR-5b | eCQI QRDA page `https://ecqi.healthit.gov/qrda` (linked from every 2027 measure page). If the 2027 guide is not published yet, the 2026 one is the working basis and the 2027 one replaces it before EHR-5b merges |
| HL7 CDA R2 Implementation Guide: Quality Reporting Document Architecture, Category III, STU Release 2.1 (US Realm) | EHR-5b (the base CMS's guide constrains) | HL7 `https://www.hl7.org/implement/standards/product_brief.cfm?product_id=286` |
| HL7 CDA R2 Implementation Guide: QRDA Category I, Release 1, STU Release 5.3 (US Realm), the QDM 5.6-based templates | EHR-5c1, EHR-5c2 | HL7 `https://www.hl7.org/implement/standards/product_brief.cfm?product_id=35` |
| CMS QRDA I guide for eligible clinicians | — | CMS publishes its QRDA I guide for hospital quality reporting only, as far as known (not checked); eligible clinicians submit QRDA III. EHR-5c follows the HL7 base guide (E-33) |
| QDM 5.6 | EHR-5c1, EHR-5c2, EHR-8 | `https://ecqi.healthit.gov/qdm` |
| CQL specification (operators, intervals, age) | EHR-8a1 | `https://cql.hl7.org` |
| CMS eCQM logic and implementation guidance (current version) | EHR-8, EHR-5 | eCQI, eCQM resources |

### 9.2 EHR-5b QRDA III

- One document for the provider, the period and the measures calculated: per measure its reference (set ID, version-specific ID, the HQMF
  population IDs from the catalog), the counts of each population per group and stratum, the supplemental data counts, and the
  performance rate where the guide asks for it.
- CMS's 2027 guide's own requirements (program identification, TIN and NPI, the performance period) are read from the guide when it is
  downloaded; the 2014 writer's practice and provider identifiers (`5737519bf`, `QualityMeasures.cs:5862-5927`) are the starting point.
- **Payer.** The 2014 writer grouped payers by the first SOP digit (`QualityMeasures.cs:8014-8040`). CMS's QRDA III guides for eligible
  clinicians report CMS payer groupings (Medicare, Medicaid, Private, Other), as publicly known but not checked in the downloads. EHR-5b
  takes the mapping from SOP codes to those groupings from the 2027 guide and records it in its port note.
- **Kept from the 2014 window.** The legal authenticator: when the practice default provider is not a person, the export asks for the
  provider legally responsible (`FormEhrQualityMeasures2014.cs:149-165`, as at `5737519bf`). The OID registry window opens when the export
  fails for a missing root (`:193-197`).
- **NCQA's statements.** For CMS138, CMS155 and CMS165, the measure's section of the document carries NCQA's copyright and disclaimer
  statements from the catalog, verbatim, in the section's narrative block (its human-readable `text`). The guide decides the exact place
  inside that section and is read for it when it is downloaded; if it allows no narrative there, the statements go in the document's
  narrative next to the measure's reference, and the port note says where.
- Validated against CMS's 2027 schematron in a test (the schematron is a download; if it may not enter the repository, the test reads it
  from a folder outside, as `M201_OD_DOCS_DIR` does, and is inconclusive without it).

### 9.3 EHR-5c1 and EHR-5c2 QRDA I

- One document per patient, with the data of every calculated measure whose initial population holds the patient, and one zip per measure
  holding the documents of that measure's patients, as the 2014 writer did (`QualityMeasures.cs:6973-6990`, `7850-7861`).
- The QDM 5.6 templates of the data each measure reads (section 5.3). EHR-5c1 writes the document frame, patient characteristics, payer,
  encounters and diagnoses. EHR-5c2 writes procedures, interventions and medications with negation, physical exams and assessments.
- Codes come from the office's imported rows (value sets and direct codes); `sdtc:valueSet` names the value-set OID.
- **NCQA's statements.** In a document that holds data for CMS138, CMS155 or CMS165, the measure section's narrative carries NCQA's
  copyright and disclaimer statements for that measure, verbatim from the catalog (EHR-5c1), as in QRDA III (section 9.2).
- Entries are stored as UTF-8 without a BOM (the EHR plan's EHR-5 encoding item). `ODZip.GetDefaultEncodingBytes` is no longer called for
  QRDA: `QualityMeasures.cs:7860-7861` goes in EHR-6b, and the method keeps its caller `Ramq.cs:175`.

### 9.4 Kept from the EHR plan's EHR-5 (E-1, E-13, E-15)

- **Identity (E-15).** The author's ID root is the office's root (`OIDInternals`, `IdentifierType.Root`) with the extension `OpenPmsDental`;
  no Open Dental arc, address, phone or name; the legacy-root warning (`OIDInternals.IsLegacyRoot`) before export, with the OID window.
  The 2014 faults (`:5866-5868`, `:5927`, `:7209-7214`, `:7273`) are deleted with the code and are not repeated.
- **Stylesheets (E-13).** The CDA-core-xsl stylesheet of EHR-3 (`GetEhrResource("CCD")` and its companions) is written beside the files and
  into each zip, under the names the documents point to. No HTML.
- **Caption (E-1).** Internal review only; not certified; not for attestation or submission.
- **Numbers.** `InvariantCulture` for every number written.

---

## 10. Tests

### 10.1 Rules (the EHR plan's section 6, unchanged)

- Synthetic codes starting with `TEST` under the real OIDs; codes of CDCREC, CVX and HCPCS Level II only where needed; nothing of CPT, CDT,
  SNOMED CT, LOINC, ICD-10-CM or SOP. A new catalog guard: a test fails if a string literal of `CqmCatalog.cs` that is not an OID or a
  GUID looks like a code (a run of 5 to 18 digits, the CDT pattern, the LOINC pattern of digits, a hyphen and a check digit, the ICD-10
  pattern of a letter and two digits). NCQA's statements pass it (checked against the three HQMF files' text). The fixture guard
  (`Fixtures_HoldNoLiteralOfACodeFromTheEnginesLists`, EHR-2, `ValueSetSourceTests.cs:184-220`) reads the code lists of `EhrMeasures.cs`
  and `QualityMeasures.cs`; EHR-6b deletes the second file, so the guard reads `EhrMeasures.cs` only. That file alone yields about 20
  codes by the guard's pattern (an estimate from its string literals, not a test run), so EHR-6b lowers the `Count>50` floor to what the
  file yields (the port note records the number) and keeps the CDT check.
- A synthetic 2027 release for the tests: a workbook and an SVS file in the shapes of E-20's record, with every catalog value set; a Direct
  Reference Codes file with one `TEST` code per role; a Code System Versions file. Written by the existing `ValueSetFixtures`.
- Database tests on the scratch-database helper (`tests/EhrTests/Support/ScratchDb.cs`), Middle Tier tests with the mock server, as EHR-1,
  EHR-2 and EHR-4a do.

### 10.2 Synthetic patients per population (EHR-8a2 to 8d)

For each measure, one fixture patient per edge, built by a new `Support/CqmFixture.cs`. It follows the pattern of EHR-4a's
`Support/MeasureCalcFixture.cs` and does not edit that file, which EHR-4b uses.

- in the initial population only (denominator, not numerator), and one just outside it at each age boundary (the day before and the day of
  the birthday at the anchor) and each date boundary (December 31 of the year before, January 1, December 31, January 1 after);
- in the numerator by each branch. For CMS69: normal BMI; high BMI with an intervention, with a referral, with a medication order and with
  an active medication; low BMI likewise; a normal and a high BMI in the same period, which is not normal. Each high or low case once with a
  `disease` row and once with only `vitalsign.WeightCode` (E-35);
- excluded by each exclusion branch the program can hold (hospice by diagnosis, by encounter, by intervention; palliative care; pregnancy;
  frailty and advanced illness; ESRD; for CMS165 a dialysis intervention row, E-28), and the branches it cannot hold shown as "not
  calculated" in the notes;
- each exception branch, and the near misses. For CMS69: BMI exam not performed with "Patient Declined" (an exception); follow-up not
  ordered with "Medical Reason" (an exception, E-34); a follow-up the patient declined (`IsPatDeclined`: neither numerator nor exception);
- declined rows: declined tobacco counseling (CMS138, not numerator 2); declined nutrition counseling (CMS155, not numerator 2); a declined
  medication stored as an intervention (not an order);
- medications: a `medicationpat` with a `DateStart` in the period, read both as active over its dates and as an order on its start date
  (E-39), with and without a `PatNote` (no difference); one with no `DateStart` (active only, never an order); for CMS138 an active
  pharmacotherapy with no stop date (not "during" the window, so not numerator 2) and one stopped inside the window (numerator 2);
- problems (E-26): an Active row with no `DateStop` (open); a Resolved row with no `DateStop` whose `DateTStamp` falls before the period
  (not prevalent in it) and one whose `DateTStamp` falls inside it (prevalent up to that day); a Resolved row with a `DateStop`; a row with
  no `DateStart` (open at the start);
- vital signs (E-35, CMS155): a percentile recorded at age 17 on `DateTaken` and one for a child who turns 3 in the period, both counted;
  a percentile row with an empty exam code (counted, deviation list);
- each stratum and population group, with its counts;
- for CMS68: one patient with three encounters (two documented on the day, one with an acute-crisis not-performed), giving 3/2/1; and a
  patient with no valid birthdate whose encounter counts;
- for CMS74: one varnish date (not numerator), two on the same date (not numerator), two on two dates (numerator);
- for CMS138: two encounters on one day (one visit under E-24, against the CQL's count of two; the deviation list says so);
- for CMS165: the most recent day with only a systolic reading (skipped), the lowest of two readings on one day, a reading on an ED
  encounter's day (left out);
- a provider with no key row (nobody listed), a second provider with the same names (both records count).

Each fixture's expected numbers are written in the test with the CQL `define` they come from. Golden numbers: one recorded run of all
measures in the build on the whole fixture.

### 10.3 The measures' own test cases (EHR-9, optional)

- eCQI's 2027 measure pages link no test cases (the saved pages hold no such link). CMS's measure test cases live in MADiE
  (`https://madie.cms.gov`), which needs a HARP sign-in, and earlier in Bonnie (`https://bonnie.healthit.gov`), likewise.
- If CMS publishes them without a sign-in, they are download batch Q's last item. They hold real codes, so they never enter the repository:
  a test-only loader reads their QRDA I patients into a scratch database from a folder named by `CQM_TESTCASES_DIR`, with the office's real
  release from `CQM_VSAC_DIR`, and compares each case's expected populations. Without the folders the tests are inconclusive, as
  `M201_OD_DOCS_DIR` makes M201Tests.
- If they need a sign-in, EHR-9 is not written unless the owner arranges the access (E-33).

### 10.4 Targeted runs per package

Every package: `run-tests.ps1 -Mode Targeted` with EhrTests and NoHqTests (always included), plus:

| Package | Also |
|---|---|
| EHR-6a | MiddleTierMockTests (new Meth methods); AttributionTests and LibWebTests' notice checks if it adds LOINC's notice (E-23, E-37); SyntheticData.Tests not needed (no schema) |
| EHR-6b | MiddleTierMockTests (methods and types removed; `MiddleTierRoundTripTests`), BSHELLTests (the EHR window, both deleted windows' entry points); the lead's project-baseline re-record and Release dry run (removed items named) |
| EHR-7 | MiddleTierMockTests (`DiseaseDefs.GetAllPregDiseaseDefs`), BSHELLTests; RegistryTests if E-34 adds an enum value |
| EHR-8a1-8d | MiddleTierMockTests (`CqmEngine.Calculate` through the mock, from 8a2) |
| EHR-5a | MiddleTierMockTests, BSHELLTests |
| EHR-5b, EHR-5c1, EHR-5c2 | ReleaseFlowTests (`ProgramNames` in the software name, `ProgramNamesTests.cs:217-240` if a name is spelled), LibWebTests (the stylesheet resources), AttributionTests if a notice changes |
| EHR-9 | none beyond EhrTests |

No package changes the schema, so ForkMigrationTests, M201Tests and P262SchemaTests are not needed. Full runs stay at the lead's gates,
one at a time.

---

## 11. Counsel and licences

### 11.1 Measure stewards (E-10b answered by the owner, 2026-11-14)

| Measures | Owner of the measure | Terms (the 2027 HTML's copyright section) |
|---|---|---|
| CMS68, CMS69, CMS74, CMS75 | CMS ("owned and stewarded by the Centers for Medicare & Medicaid Services"), developed by AIR | No licence condition for use; "limited proprietary coding is contained in the Measure specifications for user convenience" |
| CMS138, CMS155, CMS165 | NCQA | In summary (NCQA's text is not repeated here): internal, noncommercial use needs no approval from NCQA; other uses, commercial use and vendors' products that calculate the measures among them, need NCQA's approval and may need its licence (`CMS138-v15.0.000-QDM.html`, copyright section; the same in CMS155 and CMS165) |

- **E-10b, answered by the owner (2026-11-14).** The question was whether distributing, free and as open source, a program that calculates
  CMS138, CMS155 and CMS165 needs NCQA's approval. The owner's answer: building the three NCQA measures in this open-source program is
  okay, and nothing waits for counsel on this. The split that kept their catalog part (`CqmCatalogNcqa.cs`) and EHR-8c and EHR-8d behind
  counsel is undone: the catalog of EHR-6a holds all seven measures, and EHR-8c and EHR-8d follow EHR-8b in the normal order (section 13.1).
- **NCQA's statements, kept with the measures.** The owner's condition: NCQA's copyright and disclaimer statements from the measure
  packages stay with those measures wherever they are shown, printed or written to QRDA. The plan carries their sources, not their text.
  - **Source.** Each NCQA measure's HQMF holds the two statements as `measureAttribute` entries whose display names are "Copyright" and
    "Disclaimer": CMS138 `x/CMS138-v15.0.000-QDM.xml:446-465`, CMS155 `x/CMS155-v15.0.000-QDM.xml:315-334`, CMS165
    `x/CMS165-v15.0.000-QDM.xml:494-513`; the HTML shows the same text in its Copyright and Disclaimer rows (for example
    `x/CMS155-v15.0.000-QDM.html:778`, `793`). Each statement is HTML paragraphs in an attribute value.
  - **Held.** EHR-6a copies both statements of each NCQA measure verbatim (unescaped, paragraph breaks kept, no edit) into `CqmCatalog`,
    beside the measure's title, and records the HQMF files' SHA-256 and each statement's SHA-256 in the port note. A test holds each
    statement's text to its recorded SHA-256, so an edit shows. The text holds no code (section 10.1's catalog guard passes).
  - **Shown.** The window (EHR-5a): the notes of each NCQA measure show both statements in full, and the detail window of an NCQA measure
    shows them under its members. The grid row itself shows the title only, as for the CMS measures.
  - **Printed.** The window's print-out (EHR-5a): after the grid, both statements of every NCQA measure on the page, in full.
  - **Written to QRDA.** QRDA III (EHR-5b) and QRDA I (EHR-5c1): the narrative of the measure's section in each document that reports an
    NCQA measure (sections 9.2 and 9.3).
  - **Elsewhere.** The release notes and port notes name the three measures by CMS ID and title only and say that the statements are shown
    with them in the window. A later place that shows an NCQA measure's results (a report, an export) shows both statements too.
  - Whether the four CMS measures' statements are shown the same way is E-38.
- The value-set OIDs and names that the data-entry windows already use (the tobacco and counseling sets) are VSAC's, named in today's
  source, and stay in `CqmCatalog`'s pick-list part as well as under their measures.
- **E-10** (counsel: the steward terms of the 2014 measures' logic and of the 2011 window, and the manifest's names and categories) gates
  nothing in this plan any more. The 2014 and 2011 code is deleted (EHR-6b), the 2027 NCQA measures are the owner's decision 5, and the
  catalog's names and categories are E-8's (section 11.2). Whether to withdraw it from counsel's list is E-40.

### 11.2 Code systems

| Code system | Where in the 2027 measures | Terms | Rule here |
|---|---|---|---|
| CDT (ADA) | "Clinical Oral Evaluation" and "Fluoride Varnish Application for Children" (CMS74, CMS75); "Encounter to Evaluate BMI" now holds "Clinical Oral Evaluation" (CMS69 v15 TRN) | "All uses of the CDT codes require a license from the ADA ... Resale, transmission, or distribution of copies of the CDT or other portions of the CDT is also not allowed" (`CMS74-v16.0.000-QDM.html`, copyright; also CMS75) | Never in the repository, never in a fixture, never in a document (this one included). The office's VSAC import and its own CDT procedure codes only. E-9's question on the two CDT numbers in the Demographics measure's SQL (`EhrMeasures`) is unchanged |
| CPT (AMA) | Encounter value sets; four CMS138 direct codes | "CPT only Copyright 2025 American Medical Association" (each HTML); the UMLS licence's level 3 | Import only. The direct codes' descriptors are AMA text and are never written either |
| SNOMED CT | Value sets; six direct codes | Used "by permission of IHTSDO" (each HTML); each office's affiliate licence (EHR plan 8.3) | Import only. EHR-7 removes the CMS68 direct code that `FormMedical.cs` writes, the nine recommended encounter codes and the nine recommended pregnancy codes; the two `WeightCode` literals wait for E-8 (E-35) |
| LOINC | Value sets; seven direct codes; exam codes in the vital-sign windows (section 8) | "available at no cost under the license at http://loinc.org/license" (each HTML) | Value sets and direct codes: import only, per owner decision 3. The vital-sign windows' LOINC literals stay (E-37). The repository holds LOINC codes today with no LOINC notice (`THIRD-PARTY-NOTICES.txt` has none), so EHR-6a adds LOINC's notice if E-23(a) or E-37 is answered as recommended |
| ICD-10-CM | Value sets; two direct codes | "ICD-10 is copyright 2025 World Health Organization" (each HTML) | Import only; treated as licensed until counsel answers E-8 |
| SOP (payer) | "Payer Type" | E-8 | Import only |
| HL7 ActCode | one direct code (CMS69) | HL7 terminology | Import only, for one rule for every direct code |
| CDCREC, CVX, HCPCS Level II | race and ethnicity; vaccines | US government | As the EHR plan's section 6 |

- **The specifications themselves** (HQMF, CQL, ELM, HTML, TRN) carry the stewards' notices and the code systems' codes. They stay in
  the lead's download folder, outside the repository. The catalog takes from them only facts: OIDs, value-set names, QDM categories,
  measure identifiers, titles and population names (E-8), and for the three NCQA measures their copyright and disclaimer statements,
  verbatim (owner decision 5, section 11.1).
- **E-8 (counsel), extended.** Is the catalog (OIDs, names, categories, identifiers, roles of direct codes) outside UMLS §3-§5, as the June
  2013 manifest was expected to be? Is the office's import of the Direct Reference Codes and Code System Versions files its own internal use
  under §12? May the two SNOMED CT `WeightCode` literals stay (E-35)?

---

## 12. Packages

Each package writes `docs/port-notes/EHR-n.md` and gives its release-note lines to the lead. EHR-6a also owns the release notes' EHR
value-set section itself (section 3). Each follows the EHR plan's section 6. Only EHR-6b edits a project file. Both projects glob
(`OpenPmsDentalBusiness.csproj:2-7`, `OpenPmsDental.csproj:2-5`; a new form needs no item, section 2.1), and the test project is
SDK-style since S1-6. Each package gives its window (section 13).

### EHR-6a The 2027 catalog and import

- **Goal.** The importer takes the 2026-05-14 release and its two companion files in one pick, checks the 2027 versions, and resolves the
  direct-code roles. The catalog holds all seven measures, the NCQA three's copyright and disclaimer statements, and the pick-list value
  sets as data.
- **Owns.**
  - new `OpenPmsDentalBusiness/Db Multi Table/Cqm/CqmCatalog.cs` (the catalog and its API, all seven measures)
  - `OpenPmsDentalBusiness/Data Interface/EhrCodes.cs` (`EhrValueSetImport`: 548, 571-573, 617-698 from the catalog, 701 from E-20's
    record, the version check 1678-1716, the report 1723, the import 1782 with both kinds of row in one swap; `EhrValueSetRelease` 522-535
    gains the three files and the code-system versions; two new readers at the end of the file; `GetCodeSystemStatuses` 188)
  - `OpenPmsDental/Forms/FormCodeSystemsImport.cs` and `.designer.cs` (the row's text "eCQM value sets (VSAC, eligible clinicians,
    2026-05-14)", the pick of three files, the status grid's new column)
  - `tests/EhrTests/ValueSetSourceTests.cs`, `ValueSetParseTests.cs`, `ValueSetDbTests.cs`, `Support/ValueSetFixtures.cs`; new
    `DirectCodeTests.cs`, `CatalogTests.cs`
  - `docs/release-notes/next.md` (26-28 and 492-517: the download instructions name the 2026-05-14 release and its three files; the claim
    that the 2014 measures have their codes again goes)
  - if E-23(a) or E-37 is answered as recommended: `THIRD-PARTY-NOTICES.txt` (LOINC's notice) and
    `tests/AttributionTests/ReleaseIdentityTests.cs` (its component list, 18-22, and, if LOINC's terms go in as their own section, the
    licence-title check of `ThirdPartyNotices_NamesEveryShippedFileAndSource`). The embedded copy follows the file
    (`OpenPmsDental/Properties/Resources.resx:463-465` links it)
  - `docs/port-notes/EHR-6.md` (6a part)
- **Public basis.** The seven packages (HQMF for IDs and, for the NCQA three, the two statements; CQL for value sets and direct codes),
  the 2027 table, VSAC's download help page. No licensed content.
- **Work.** Sections 7.1-7.4 and 11.1. The catalog's per-measure OID lists equal the CQL files' `valueset` lines, and only those lines
  (section 7.2). The writer checks them against the downloads and records them with the files' SHA-256 in the port note, and a test holds
  the catalog to that record. The NCQA statements copied verbatim, with their SHA-256 (section 11.1). The Code System Versions mapping per
  code system (section 7.4). The release-note rewrite.
- **Tests.** The synthetic 2027 release imports: report "7 of 7 measures at the 2027 versions", every role resolved. The 2026
  release's versions give the warning. A file with none of the seven is refused. A pick without all three files is refused, and nothing
  changes. A second import of the three replaces both kinds of row together, and the roles still resolve. An import that fails part way
  leaves every old row of both kinds. A Direct Reference Codes file with a role missing, and one with a role twice, warn. A Code System
  Versions file with a version newer than the loaded `codesystem` row flags it where the mapping exists. The catalog holds no code-like
  literal. The catalog holds the seven measures of section 4.1, and each NCQA statement equals its recorded SHA-256. The OID allowlist test, and the count of 68 value sets from `valueset` lines only.
  Through the Middle Tier mock: the client's list has the direct codes after `InvalidType.EhrCodes`.
- **Owner checks (launch, test database).** Import the synthetic release (from a test's writer): the report, the roles, the status grid's
  new column. On the office's computer, the real three files: "7 of 7", the roles, the sizes (the import stays under the limits, or the
  report says which).
- **Window.** Stage 4. It may be written now, while S4-1 runs. It merges after S4-1 merges, rebased on its MSTest 4 changes, because S4-1
  owns every test `.cs` that fails to compile and no EHR test edit merges while S4-1 runs. It also merges after E-20's record. Its notice
  edit is in stage 4, after S2-4 and TC-4 (both merged), before S3a-8 (`ReleaseIdentityTests.cs:96`, 3a wave 1), S3a-13, TC-9b and S3b-7.
- **Size.** L, about 1.25 agent-days (the NCQA three's catalog part, 0.25, moved here from EHR-8c).

### EHR-6b The 2014 and 2011 code deleted

- **Goal.** Owner decisions 2 and 4: the 2014 versions and the 2011 engine, its window and its PQRI XML export deleted on purpose, with
  everything only they use, their tests and their project-file and baseline entries. Saved database rows stay untouched.
- **Owns.**
  - `OpenPmsDentalBusiness/Db Multi Table/QualityMeasures.cs` (deleted: both engines' ranges of section 2.1 cover it; lines 1-57 are
    private fields of the two)
  - `OpenPmsDentalBusiness/Db Multi Table/QualityMeasure.cs` (deleted: `QualityMeasure`, `QualityType`, `QualityType2014`,
    `CqmItemAbbreviation`, the `EhrCqm*` classes)
  - `OpenPmsDental/Forms/FormEhrQualityMeasures2014.*`, `FormEhrQualityMeasureEdit2014.*`, `FormEhrQualityMeasures.*` (the 2011 window,
    with the PQRI XML show and submit and `GeneratePQRS_xml`), `FormEhrQualityMeasureEdit.*` (deleted; `.cs`, `.Designer.cs`, `.resx`)
  - `OpenPmsDental/OpenPmsDental.csproj` (the eight `Update` elements of the four forms: 1043-1054 and 3906-3917; nothing else)
  - `OpenPmsDental/Forms/FormEHR.cs` (547-551: the dead `butQuality_Click` deleted; 553-564: the message until EHR-5a; 624-628:
    `ProvKeyIsValid` deleted, its two callers being the deleted windows)
  - `tests/EhrTests/ProvKeyRuleTests.cs` (336-341: `ProvKeyIsValid` is in no source file, as for `QuarterlyKeyIsValid` at 328-330),
    `ValueSetSourceTests.cs` (the header comment at 2; the reflection of `GetEMeasureNum` and `GetEMeasureVersion` at 26-32 goes; the
    allowlist loses the engine; the fixture guard at 184-220 reads `EhrMeasures.cs` only, with its floor lowered, section 10.1)
  - new `tests/EhrTests/RemovalTests.cs`
  - `docs/port-notes/EHR-6.md` (6b part, naming the removed Compile and EmbeddedResource items and the two business files so the lead's
    re-record of `tools/project-baseline` (`items.txt:4527-4528`, `6589-6596`, `8421-8424`; `resources.txt:285-288`) accepts them; and the
    one stale comment it leaves, `EhrMeasures.cs:7247`, which names "the quality measure windows (EHR-5)", for that file's next owner)
- **Work.** Delete; the release-note line of section 3. Before deleting, the writer greps `main` for every type, method and form name of
  the deleted files (outside comments and `docs/`) and compares the result with section 2.1's list of outside callers; anything not on it
  is either deleted with the code (if only the deleted code uses it) or kept, and the port note says which. Kept, as section 2.1 found:
  `EmailMessages.SendTestUnsecure` (four other callers), `MsgBoxCopyPaste`, EHR-4a's `EhrMeasures.DropOldWorkTables` and its
  `tempehrquality` pattern (it still clears a work table a stopped old run left), `ODZip.GetDefaultEncodingBytes` (`Ramq.cs:175`). No
  table, column or saved row is touched; the translation rows of the deleted forms stay in the database, unread.
- **Split if large.** If the grep finds more callers than section 2.1 lists, or the package passes about one agent-day, the 2011 part
  (its engine ranges, its two forms, their four project items, `butQuality_Click`, `ProvKeyIsValid`) becomes EHR-6c, written right after
  EHR-6b in the same window, and EHR-6b keeps `QualityMeasures.cs` with the 2011 ranges only.
- **Tests.** A source scan of the program's source (comments left out): no `QualityMeasures`, `QualityMeasure`, `QualityType`,
  `QualityType2014`, `GetAll2014`, `GetEhrCqmData`, `GenerateQRDA`, `GetPQRIMeasureNumber`, `GeneratePQRS_xml`, `EhrCqm`,
  `FormEhrQualityMeasures`, `FormEhrQualityMeasureEdit` or `ProvKeyIsValid`; none of the twelve form files and neither business file
  exists; the project file names none of the four forms; "Quality Meas" shows the message; `EhrMeasures.DropOldWorkTables` still drops an
  old `tempehrquality` table on the scratch database (MeasureCalcTests pass unchanged); the fixture guard passes with its new floor;
  MiddleTierMockTests pass with the removed methods.
- **Owner checks.** The EHR window's "Quality Meas" gives the message; the other EHR buttons open as before. The release dry run's staged
  file list is unchanged.
- **Window.** Stage 4, after EHR-6a. Before 3a wave 1, because S3a-3 named `QualityMeasures.cs:7860-7861` (section 16). Its
  `OpenPmsDental.csproj` edit comes before the file's next owners: S3a-13 and S3a-14 (3a wave 3), TC-9b and S3b-4. No stage 4 package
  owns the file. An EHR-6c, if the split is needed, has the same window.
- **Size.** M, about 0.75 agent-day (0.5 for the 2014 part, 0.25 for the 2011 part and its callers).

### EHR-7 Data entry on the 2027 catalog

- **Goal.** Section 8.
- **Owns.** `FormInterventionEdit.cs`, `FormInterventions.cs`, `FormEhrNotPerformedEdit.cs`, `FormEhrNotPerformed.cs`,
  `FormVitalsignEdit2014.cs` (394; 314-524 only if E-37 is answered otherwise; 821 with E-35; the age rule at 17, 48, 305-306, 328, 335,
  350, 532, 688, 708), `FormMedical.cs` (104, 879-940, 1037, 1350;
  before S3a-M3, which owns the file in 3a wave 2), `FormEhrPatientSmoking.cs`, `DiseaseDefs.cs` (398-420), `FormEhrSettings.cs` (105-133
  and the default-code handling), `FormEncounterTool.cs` (43-55), `OpenPmsDentalBusiness/Data Interface/Vitalsigns.cs` (116-136, 621-655,
  682-733) and `EForms/EFormVitalSigns.cs` (306-334, the age at 314-315 and 329 among them) for E-35; `TableTypes/EhrNotPerformed.cs` (35-43) and `docs/port-notes/FORK-ENUMS.md` if E-34
  adds the item; `tests/EhrTests/PickListTests.cs`; `ValueSetSourceTests.cs` (allowlist empty); `ValueSetDbTests.cs` (396-403 assert the
  exact `GetForValueSetOIDs(new List<string> { "<oid>" },true)` calls of `FormMedical.cs` and `FormEhrPatientSmoking.cs`; 407-426 assert
  `DiseaseDefs.cs`'s two-set call and a pregnancy code in both sets, both of which EHR-7 changes).
- **Work.** Each OID becomes a catalog constant or a direct-code role; each code literal of section 8 is decided as listed there. E-25, E-28,
  E-32, E-34, E-35 and E-37 as the owner answers. With E-34, `EhrNotPerformedItem` gains a fifth value, 4. The enum is not stored: the rows
  store codes. The windows index it by position (`FormEhrNotPerformed.cs:123-131`, `FormEhrNotPerformedEdit.cs:65`), so a fork-only value
  from 10000 would break them. FORK-ENUMS.md gets the enum's own rule. The not-performed grid keeps each row's item, or null, in the row's
  `Tag` and the double-click reads it, never the label (`FormEhrNotPerformed.cs:123` parses the label today, which throws on "other"); an
  "other" row opens the edit window read-only (section 8). The vital-sign windows and the eForm import take the age on `DateTaken` for
  the percentile (3-17), the over- or underweight class (18 and over) and the child counseling group (under 18), by one business-layer
  helper the three call (E-35, section 8).
- **Tests.** Each window's list fills from the synthetic release (source checks plus data tests where the method is in the business layer);
  pregnancy problems from "Pregnancy" alone; the medical window writes the role's code and nothing without a role; the not-performed rows
  are labelled by role and catalog (an unknown code is "other"); the item of each row comes from its `Tag`, the handler holds no
  `Enum.Parse` (a source check), an "other" row opens read-only and a known row with its item (the pure mapping from code to item, or null,
  tested on its own); the intervention window labels medications by value set; no value-set OID literal outside the catalog; no SNOMED CT
  literal left in the encounter and pregnancy recommendations; with E-35, the 2027 BMI range at 65 and over, and the age helper on the
  day before and the day of the 3rd, 17th, 18th and 65th birthdays (a percentile at 17 years and 11 months; none at 18; a class at 18;
  the child group at 17), the same through `EFormVitalSigns`; `GetBMIPercentile` gives -1, not an exception, outside the LMS table.
- **Owner checks.** With the synthetic release and synthetic SNOMED rows: the Tobacco Use tab, the intervention window's lists, the
  not-performed window for BMI, current medications and (E-34) the BMI follow-up, the recommended encounter and pregnancy codes.
- **Window.** Stage 4 after EHR-6b, or 3a wave 1. It must merge before S3a-M3 starts (3a wave 2, `FormMedical.cs`). None of its other
  files uses WinForms menus (a grep at `5737519bf`).
- **Size.** L, about 1.25 agent-days (re-sized after section 8's inventory, and again for the not-performed grid and the age rule).

### EHR-8a1 CQL operators and the data layer

- **Goal.** The pieces every measure uses, tested on their own.
- **Owns.** new `Cqm/CqmCql.cs`, `CqmData.cs`; `tests/EhrTests/CqlTests.cs`, `CqmDataTests.cs`, `Support/CqmFixture.cs`.
- **Public basis.** The CQL specification; QDM 5.6; `CQMCommonQDM-9.0.000.cql`.
- **Work.** Section 5.2's operators; section 5.3's data map for every table, with the rules on declined rows, the two readings of a
  `medicationpat` (E-39), the problem status rule (E-26), `WeightCode` and the exam rows with an empty code (E-35) and dialysis
  interventions (E-28); E-24 and E-26 as the owner answers.
- **Tests.** `CqmCql` against the CQL specification's examples (intervals, "during day of", "same day as", age on the birthday and the day
  before, leap days, open-ended intervals); the data layer per table and per rule above.
- **Owner checks.** None (no window); the lead reads the port note.
- **Window.** Stage 4 if the EHR slot is free after EHR-7, otherwise 3a wave 1. New files only.
- **Size.** M-L, about 0.75 agent-day.

### EHR-8a2 The engine and the dental measures

- **Goal.** `CqmEngine` with CMS74v16 and CMS75v15.
- **Owns.** new `Cqm/CqmLibraries.cs` (CQMCommon, Hospice), `CqmEngine.cs`, `CqmCms74.cs`, `CqmCms75.cs`;
  `tests/EhrTests/CqmEngineTests.cs`, `Cqm74Tests.cs`, `Cqm75Tests.cs`.
- **Public basis.** The two packages' CQL, `HospiceQDM-7.0.000.cql`.
- **Work.** Sections 5.2, 5.4 and 6. The whole result shape (section 5.2), so later packages add only measure classes. The class is in the
  namespace `OpenPmsDentalBusiness` and its result types are XML-serialisable (section 5.2, Middle Tier). A known-deviation list in the port
  note (criteria with no data).
- **Tests.** Section 10.2's patients for both measures and the hospice library; strata counts; the result shape with a synthetic group
  and an episode member; a Middle Tier round trip; the same result directly and through the mock.
- **Owner checks.** None in the program yet (no window); the lead reads the port note's deviation list.
- **Window.** 3a, waves 1-3, by E-36. New files only.
- **Size.** M-L, about 0.75 agent-day.

### EHR-8b CMS68v16 and CMS69v15

- **Owns.** new `CqmCms68.cs`, `CqmCms69.cs`; `CqmLibraries.cs` (PalliativeCare, appended); `tests/EhrTests/Cqm68Tests.cs`,
  `Cqm69Tests.cs`.
- **Work.** Section 5.4, with the age rule of section 6 (CMS68 reads no birthdate), the exceptions as corrected and E-34 and E-35 as
  answered.
- **Tests.** Section 10.2's patients; CMS68's 3/2/1 encounter case and its no-birthdate case; CMS69's BMI branches with and without a
  `disease` row, its exceptions and near misses.
- **Window.** 3a, after EHR-8a2.
- **Size.** M-L, about 0.75 agent-day.

### EHR-5a The 2027 CQM window

- **Goal.** The window behind "Quality Meas", on `CqmEngine`, with the measures merged so far.
- **Owns.** new `OpenPmsDental/Forms/FormEhrCqm.cs`, `.designer.cs`, `.resx`, `FormEhrCqmDetail.cs`, `.designer.cs`, `.resx`;
  `FormEHR.cs` (553-564); `tests/EhrTests/CqmWindowTests.cs`.
- **Work.**
  - Provider list by the EHR plan's rule (`EhrProvKeys.HasProvHadKey`, `EhrProvKeys.cs:51`), with the message that says where to add rows.
  - Reporting year (default this year) or custom dates (E-29); Refresh on an `ODThread`, Cancel between measures; Print through
    `GridOD.PrintPage`.
  - Grid: measure (CMS ID and version, title), group or stratum, initial population, denominator, exclusions, numerator, exceptions, not
    met, performance rate ("lower is better" for CMS75); "not calculated" for a criterion whose role did not resolve.
  - Notes per measure: the criteria this program has no data for and the deviations (section 5.3); for CMS155 the intended clinicians;
    for CMS138, CMS155 and CMS165 NCQA's copyright and disclaimer statements in full, from the catalog (section 11.1).
  - Detail: members (patients, or encounters for CMS68) with their populations and reason; double-click selects the patient
    (`PatientSelect`, as `FormEHR.cs:556-561` does today). For an NCQA measure the detail shows both statements under its members.
  - Print: after the grid, both statements of every NCQA measure printed, in full.
  - Caption: E-1. No new gate (E-4): the window opens from the EHR window as today.
- **Tests.** The provider rule; the grid's rows equal the engine's result on the fixture; the patient-pick chain in the source; no WinForms
  menu, no `Process.Start`; with a synthetic NCQA measure class (a test double), its statements appear in the notes, the detail and the
  text the print-out is built from, and a CMS measure shows none (unless E-38 says otherwise).
- **Owner checks.** Provider, year, Refresh, rows per stratum, the notes, the detail, the patient pick, print preview; once EHR-8c has
  merged, the NCQA statements in the notes, the detail and the print preview.
- **Window.** 3a, after EHR-8a2 (and 8b if it has merged), before EHR-8c in the order of 13.1. `FormEHR.cs` has no stage or chart owner.
- **Size.** M, about 0.5 agent-day.

### EHR-8c CMS138v15 and CMS155v15 (NCQA)

- **Owns.** new `CqmCms138.cs`, `CqmCms155.cs`; `tests/EhrTests/Cqm138Tests.cs`, `Cqm155Tests.cs`. The catalog data of all three NCQA
  measures, their statements among them, is EHR-6a's (owner decision 5).
- **Work.** Section 5.4, on the catalog entries EHR-6a made; any correction to those entries found while writing goes into `CqmCatalog.cs`
  here, checked against the packages as EHR-6a checked them.
- **Tests.** Section 10.2's patients; the three groups of each; CMS155's two strata at the end-of-period age, its percentile rows by E-35;
  CMS138's six-month window, its active-pharmacotherapy "during" rule, its medication orders by E-39 and its two-encounters-on-one-day
  case; the NCQA roles resolve; the window lists the two measures with their statements.
- **Window.** 3a, after EHR-8b and EHR-5a, in the order of 13.1.
- **Size.** M-L, about 1 agent-day.

### EHR-8d CMS165v15 (NCQA)

- **Owns.** new `CqmCms165.cs`; `CqmLibraries.cs` (AdvancedIllnessandFrailty, AdultOutpatientEncounters, appended);
  `tests/EhrTests/Cqm165Tests.cs`.
- **Tests.** Section 10.2's patients; each exclusion branch the program can hold, the dialysis intervention row among them (E-28), the
  problem status rule on the hypertension and ESRD diagnoses (E-26); the most-recent-day rule.
- **Window.** 3a, after EHR-8c.
- **Size.** M-L, about 0.75 agent-day.

### EHR-5b QRDA III 2027

- **Owns.** new `Cqm/CqmQrda3.cs`; `FormEhrCqm.cs` (the export button, its folder choice, the legal-authenticator pick and the OID-registry
  fallback); `tests/EhrTests/Qrda3Tests.cs`.
- **Public basis.** Section 9.1 (download batch Q).
- **Work.** Sections 9.2 and 9.4, for every measure in the catalog, all seven, with the NCQA statements (section 9.2).
- **Tests.** Structure (template IDs of the 2027 guide); counts equal the window's; every population ID from the catalog, per group and
  stratum; the supplemental counts with the guide's payer groupings; the identity checks of the EHR plan's EHR-5 (no Open Dental arc,
  address, phone or name; a synthetic root and a legacy root); `InvariantCulture`; the legal-authenticator rule when the default provider
  is not a person; the schematron when its folder is present; each NCQA measure's section carries both statements, verbatim, and no CMS
  measure's does (unless E-38 says otherwise).
- **Owner checks.** Export, open the file: the software name is Open PMS - Dental, the practice's address, the office's root; the legacy-root
  warning on a database with such a root.
- **Window.** 3a, after EHR-5a, EHR-8d and the downloads, so it is written and tested for all seven measures.
- **Size.** M-L, about 0.75 agent-day.

### EHR-5c1 QRDA I: the document, encounters and diagnoses

- **Owns.** new `Cqm/CqmQrda1.cs`; `FormEhrCqm.cs` (the export writes both); `tests/EhrTests/Qrda1Tests.cs`.
- **Work.** Sections 9.3 and 9.4: the document frame, patient characteristics, payer, encounters, diagnoses, the measure sections with
  the NCQA statements, and the zips per measure.
- **Tests.** One document per fixture patient with these templates; codes only from the imported rows; a non-ASCII patient name read back
  from a zip; the identity checks; a document of an NCQA measure's patient carries both statements in that measure's section.
- **Window.** 3a, after EHR-5b.
- **Size.** M-L, about 0.75 agent-day.

### EHR-5c2 QRDA I: the other templates

- **Owns.** `Cqm/CqmQrda1.cs` (appended); `tests/EhrTests/Qrda1TemplateTests.cs`.
- **Work.** Procedures; interventions and medications, with negation and the rationale; physical exams; assessments (section 9.3).
- **Tests.** Each template on its fixture patient; a not-performed item writes its negation and reason; codes only from the imported rows.
- **Window.** 3a, after EHR-5c1.
- **Size.** M-L, about 0.75 agent-day.

### EHR-9 The measures' own test cases (optional)

- **Owns.** `tests/EhrTests/CqmSpecCaseTests.cs`, `Support/QrdaTestCaseLoader.cs` (test-only).
- **Window.** After the downloads (if public) and EHR-8d; 3a, or after the switch.
- **Size.** M-L, about 0.75 agent-day.

---

## 13. Sequencing

### 13.1 Order

- **One EHR writer at a time** (the EHR plan's section 6). The order: EHR-6a, EHR-6b (and EHR-6c if it is split), EHR-7, EHR-8a1,
  EHR-8a2, EHR-8b, EHR-5a, EHR-8c, EHR-8d, EHR-5b, EHR-5c1, EHR-5c2. The NCQA measures follow the normal order (owner decision 5): after
  the other measures and the window, before QRDA, so that QRDA is written once for all seven. EHR-9 comes when the test cases are in
  hand. EHR-5d is gone (owner decision 4).
- **EHR-4b** (the Meaningful Use fixtures of the EHR plan, planned for stage 4) shares no file with these packages, but it takes the same
  single EHR slot. The lead places it between two of them; after EHR-7 is recommended, so that EHR-6a, 6b and 7 meet their stage 4 and
  3a wave 1 limits.

### 13.2 Windows against the .NET 10 stages and the chart track

| Stage window | Stage packages (`net10-plan.md`) | Chart track (`wpf-tooth-chart-plan.md` section 2) | EHR | Why it fits |
|---|---|---|---|---|
| Stage 4, now: S4-1 open | S4-1 (owns every test `.cs` that fails to compile, `net10-plan.md:633`); S4-2 prepared | TC-5 and TC-7 wait for S4-1's merge (chart test edits) | **EHR-6a** written, with provisional header names until E-20 | Writing in its own worktree takes the EHR slot; nothing EHR merges while S4-1 runs (EHR plan 10.2, `tests/EhrTests/*`) |
| Stage 4, after S4-1 merges | S4-2 | TC-5, TC-7 | **EHR-6a** merges (after E-20), then **EHR-6b**, **EHR-7**; EHR-8a1 if the slot is still free; EHR-4b where the lead places it | Three writers: S4-2, one chart package, one EHR package. No stage 4 package owns `OpenPmsDental.csproj`, `THIRD-PARTY-NOTICES.txt` or `ReleaseIdentityTests.cs`, and S4-1's test files are free again |
| 3a, wave 1 | S3a-1 to S3a-11 (at most three writers at once, `net10-plan.md:701-702`) | TC-6, TC-8 | What stage 4 left of 6b and 7 first (6b before S3a-3 takes `QualityMeasures.cs`; 7 before wave 2), then **EHR-8a1, 8a2, 8b, 5a** by E-36 | The EHR packages from 8a1 on add new files, plus `FormEHR.cs` (no other owner). S3a-8 owns `ReleaseIdentityTests.cs` here, so EHR-6a's notice edit is done before |
| 3a, wave 2 | S3a-M1, then M2 and M3 | TC-6, TC-8 | **EHR-8c, 8d, 5b**, by E-36; EHR-7 must have merged before M3 starts | `FormMedical.cs` is M3's. These packages add new files and edit only `CqmLibraries.cs` and `FormEhrCqm.cs`, which are EHR's |
| 3a, wave 3 | S3a-12 to S3a-16 (three writers across waves 2 and 3) | TC-10, TC-9a, TC-9b | The rest of the list (**EHR-5c1, 5c2**), by E-36 | S3a-13, S3a-14 and TC-9b own `OpenPmsDental.csproj` and `THIRD-PARTY-NOTICES.txt` now; EHR-6a and EHR-6b finished with them in stage 4 |
| 3a gate, bridge release | – | G4; TC-8 and TC-9a merged | **EHR-6a and EHR-6b required**, EHR-7 recommended; the rest if merged | The release notes on `main` name the June 2013 release since EHR-2's merge; EHR-6a rewrites them. None of these packages changes the schema, so none is needed for the rollback rule |
| 3b | S3b-0 to S3b-7 | none | **none** | "No feature work on `main` during 3b except fixes" (`net10-plan.md:852`). Open EHR packages stop |
| After the switch | – | – | Every package not merged, in the order of 13.1 | Rebased on the switched `main` and finished there in .NET 10 C# (the business library's language version after S3b-3); the Middle Tier path is the ASP.NET Core host's (S3b), so EHR-8a2's mock tests follow S3b's harness |

- **Agent time per window** (the sizes of section 1). Stage 4 needs about 3.25 agent-days for EHR-6a, 6b and 7 (1.25 + 0.75 + 1.25). If
  stage 4 ends first, they move to 3a wave 1 in the same order, within their limits (6b before S3a-3, 7 before S3a-M3). 3a needs about
  6.75 for the other nine (EHR-8a1, 8a2, 8b, 8d, 5b, 5c1 and 5c2 at 0.75 each, EHR-5a 0.5, EHR-8c 1); if EHR-8a1 fits in stage 4, stage 4
  needs 4.0 and 3a 6.0. EHR-9 (0.75, optional) is extra. Nothing in 3a waits for counsel any more.
- **The bridge release.** If any release is cut before EHR-6a merges, the lead first fixes `docs/release-notes/next.md:26-28` and
  `492-517`, so that it does not tell offices to import the June 2013 release for quality measures that list no provider and are being
  deleted.
- **The .NET 10 switch.** The new code is C# 8 on net48 (`LangVersion` 8.0) and has no platform call, so it moves with the business
  library (S3b-3). A CQL engine (E-22, B1) would wait for .NET 10.

### 13.3 Shared files

| File | Packages here | Other owners | Order |
|---|---|---|---|
| `Data Interface/EhrCodes.cs`, `FormCodeSystemsImport.*` | EHR-6a | none (EHR-2 merged) | EHR-6a |
| `docs/release-notes/next.md` (26-28, 492-517) | EHR-6a | the lead (release notes of every package) | EHR-6a; the lead first, if a release is cut before |
| `THIRD-PARTY-NOTICES.txt` (if E-23(a) or E-37) | EHR-6a | S2-4 and TC-4 (merged), S3a-13, TC-9b, S3b-7; EHR-3 (merged) | EHR-6a in stage 4, before S3a-13 and TC-9b, never while S3b-7 runs |
| `tests/AttributionTests/ReleaseIdentityTests.cs` (18-22, the licence-title check; if E-23(a) or E-37) | EHR-6a | S3a-8 (96, 3a wave 1), TC-9b, S4-1 (if it fails to compile) | EHR-6a after S4-1, before 3a wave 1 |
| `OpenPmsDental/OpenPmsDental.csproj` (the eight items of the four 2014 and 2011 forms, 1043-1054 and 3906-3917) | EHR-6b | S3a-13, S3a-14 (wave 3), TC-9b, S3b-4 | EHR-6b in stage 4 or 3a waves 1-2 |
| `tools/project-baseline/*` | EHR-6b (names the removed items) | the lead (re-record) | the lead re-records after EHR-6b's merge |
| `Db Multi Table/QualityMeasures.cs`, `QualityMeasure.cs` (deleted) | EHR-6b | S3a-3 named the first for the QRDA zip encoding | EHR-6b first; then S3a-3 has nothing to do there |
| `OpenPmsDental/Forms/FormEHR.cs` | EHR-6b (547-564, 624-628), EHR-5a (553-564) | none | as listed |
| `Db Multi Table/Cqm/CqmCatalog.cs` | EHR-6a; EHR-8c only for a correction | none | EHR-6a |
| `Db Multi Table/Cqm/CqmLibraries.cs` | EHR-8a2, EHR-8b, EHR-8d (appended) | none | as listed |
| `FormMedical.cs` | EHR-7 | S3a-M3 (3a wave 2) | EHR-7 before 3a wave 2 |
| `tests/EhrTests/ValueSetSourceTests.cs` | EHR-6a, EHR-6b, EHR-7 | S4-1 (if it fails to compile) | as listed, after S4-1 |
| `tests/EhrTests/ValueSetDbTests.cs` | EHR-6a, EHR-7 | S4-1 (if it fails to compile) | EHR-6a, then EHR-7 |
| `tests/EhrTests/ProvKeyRuleTests.cs` | EHR-6b | none | EHR-6b |
| `tests/EhrTests/Support/MeasureCalcFixture.cs` | none (EHR-8a1 writes a new `CqmFixture.cs` beside it) | EHR-4b | not edited here |
| `docs/port-notes/FORK-ENUMS.md` (if E-34) | EHR-7 | any package adding a fork-only enum value | EHR-7 |
| `tests/EhrTests/*` | every package (new files) | S4-1 (MSTest 4) | no EHR test edit while S4-1 runs |

---

## 14. Owner questions

Every question is answered. On 2026-11-14 the owner answered all of them "all as recommended", with no exception; download batch Q is
approved with E-33. Where this plan says "if E-n is answered as recommended" or "as the owner answers", the recommendation below is the
answer. The extension of E-8 is counsel's (section 11.2); nothing in this plan waits for it. E-21 and E-10b were answered earlier the same
day (end of this section).

**Data and calculation**

| # | Question | Recommendation and answer |
|---|---|---|
| **E-22** | Calculate by a C# translation of each measure's CQL (A), or bring in a CQL engine (B1: a new library, FHIR data, after .NET 10), or write an ELM interpreter (B2)? | **Answered as recommended (2026-11-14).** A (section 5.1) |
| **E-24** | Count completed procedures whose code is in a qualifying encounter value set as encounters (Encounter, Performed), besides `encounter` rows? Without it CMS74 and CMS75 find only patients whose default encounter code happens to be in "Clinical Oral Evaluation", which the 2014-era codes are not designed for | **Answered as recommended (2026-11-14).** Yes, for every measure. A procedure and its default encounter on the same day count once (one day, one encounter). This departs from the CQL in two places, both on the deviation list: CMS138 counts encounter records (`Count >= 2`), so two encounters on one day count twice there; and CMS68 is episode-based, so its members are the encounter records |
| **E-26** | Diagnoses: also read ICD-10 diagnostic codes on completed procedures (`procedurelog.DiagnosticCode`..`DiagnosticCode4` with `IcdVersion` 10) as diagnoses on the procedure date? Which date is a problem's "authored" time (CMS75v15 counts caries authored in the period): its `DateStart`, or its `DateTStamp` (last change)? And the prevalence period: `ProbStatus` decides whether a problem is active and `DateStop` stays minval while it is (`TableTypes/Disease.cs:25-30`), but the edit window saves status and dates separately (`FrmDiseaseEdit.xaml.cs:127`, `178`; `Diseases.SetDiseaseFields`, `Diseases.cs:284-292`), so a Resolved or Inactive row can have no `DateStop`. Does such a row end on its `DateTStamp` date, or is it prevalent on its `DateStart` day only? How is an unset `DateStart` (minval, "if unknown") read? | **Answered as recommended (2026-11-14).** Yes to procedure codes. Authored: `DateStart` when set, otherwise `DateTStamp`; the window's notes say so. Prevalence (section 5.3): a Resolved or Inactive row with no `DateStop` ends on its `DateTStamp` date, the nearest record of when the status was set (on its `DateStart` day only if `DateTStamp` falls before it); a later edit of the row moves that date, which the notes say. A row with a `DateStop` ends there; an Active row with none is open at the end. An unset `DateStart` is open at the start, as the 2014 engine read it (`QualityMeasures.cs:3646`) |
| **E-27** | Data the program has no place for: inpatient discharges; assessments such as the hospice MDS item, housing status, FACIT-Pal and medical equipment; devices; encounter class. Also hospice and palliative-care interventions, which no pick list offers (`FormInterventionEdit.cs:28-62` offers only BMI follow-up, referral and medication, nutrition and physical-activity counseling, tobacco and dialysis). Treat all of these as absent and list them, or add data entry in a later package? | **Answered as recommended (2026-11-14).** Absent and listed now. A later package only if the owner wants it |
| **E-28** | Interventions by code: count interventions by code whatever their `CodeSet`? The "Dialysis" pick list moves to "Dialysis Services", but CMS165 reads that set, and "Kidney Transplant", only as Procedure, Performed (`CMS165...cql:57-60`). So should an intervention row coded in either set be read as Procedure, Performed on its `DateEntry`, or should the pick list be for data entry only? A new `InterventionCodeSet` value for hospice would be a change to an Open Dental enum (`docs/port-notes/FORK-ENUMS.md`) | **Answered as recommended (2026-11-14).** Count by code; map Dialysis to "Dialysis Services"; read intervention rows in those two sets, when not declined, as Procedure, Performed (a listed deviation, with a CMS165 fixture), since the intervention window is the program's only place to record dialysis; no new enum value now |
| **E-29** | The period: a calendar year by default; allow custom dates? QRDA for a part year? | **Answered as recommended (2026-11-14).** Allow custom dates with a warning that the 2027 versions report a calendar year; QRDA writes the dates chosen, with the same warning |
| **E-34** | CMS69's follow-up exception ("Medical Reason" for a follow-up or BMI medication not ordered or not performed) has no data: no not-performed item exists for it, and an intervention has no reason code (section 5.3). Should a fifth not-performed item, "BMI follow-up", be added to `EhrNotPerformedItem`? It would pick from the follow-up, referral and BMI medication sets, with "Medical Reason" as its only reason. The enum is not stored, so it would take value 4, with its own rule in FORK-ENUMS.md. Or should the exception stay always false and be listed? | **Answered as recommended (2026-11-14).** Add it in EHR-7. It is small, and without it a criterion of a CMS measure can never be met. The old rows are unaffected |
| **E-35** | CMS69 counts a follow-up only with an active "Overweight or Obese" or "Underweight" diagnosis, but the program's BMI entry never creates a problem: it stores the finding in `vitalsign.WeightCode`. (a) Count a `WeightCode` that is in either value set as a Diagnosis, from its `DateTaken` to the next BMI vital sign (open if none). Change `GetBMIInterventionCode` to the 2027 range for every adult (18.5 to under 25; today 23 to under 30 from 65 on, the 2014 rule), and keep its two SNOMED CT literals until counsel's E-8. (b) Have the BMI window offer to add the problem. (c) Neither, and list the deviation. **Also (the vital signs' age rule):** the windows record `BMIPercentile` and `BMIExamCode` only at ages 3-16 before January 1 of the exam year (`FormVitalsignEdit2014.cs:48`, `305`; `EFormVitalSigns.cs:314-315`), take the over- or underweight class from that age too (`Vitalsigns.cs:116-117`), and write an exam code only when the LOINC table holds it; CMS155 takes ages 3-17 at the end of the period (`CMS155WeightAssessandCounselforKids-15.0.000.cql:101-104`). Should EHR-7 move them to the age on `DateTaken`, and should the engine read a row with a value but an empty exam code as that exam? | **Answered as recommended (2026-11-14).** (a). It matches how offices record BMI here, needs no new data entry, and the window's notes and deviation list say so. Rows saved before EHR-7 keep their 2014-range `WeightCode` and their percentiles (the notes say that too). And yes to the age rule: EHR-7 computes the percentile and the over- or underweight class from the age on `DateTaken` (CQL's `AgeInYearsAt`), the percentile at 3-17 (the LMS table covers 36 to 215 months), the class at 18 and over, the child counseling group under 18; it owns those files and lines (section 8, EHR-7). The engine reads a height, weight or percentile row with its value and an empty exam code as that exam, as it assumes the BMI exam (section 5.3), on the deviation list |
| **E-39** | Every `medicationpat` is read as Medication, Active over `DateStart`..`DateStop` (unset `DateStop`: ongoing), whatever its `PatNote`, which is a free dosage note (`TableTypes/MedicationPat.cs:15-17`; the 2014 rule that a `PatNote` makes an order is dropped). When is a row also a Medication, Order authored on its `DateStart` (CMS69's medication follow-up, CMS138's pharmacotherapy order)? (a) Every row with a set `DateStart`. (b) Only rows the program marks as prescribed: an eRx row (`ErxGuid` set) or a CPOE row (`IsCpoe`); this leaves out the orders the intervention window makes, which set only the patient, provider, medication and `DateStart` (`FormInterventionEdit.cs:209-214`). (c) Only rows with a provider and a `DateStart` (the type's own comment, `MedicationPat.cs:5`, without its `PatNote` condition); every new row gets a provider by default, so it tells little | **Answered as recommended (2026-11-14).** (a). The intervention window's medication orders carry no other mark, and the value sets that read orders are narrow (BMI medications, tobacco pharmacotherapy). A row with no `DateStart` is active only. A medication the patient reported then counts as an order on its start date: the deviation list and the window's notes say so |

**Codes and import**

| # | Question | Recommendation and answer |
|---|---|---|
| **E-20** | Can someone with a UMLS licence open the three files of the 2026-05-14 release once and record their names, formats, sizes (each sheet uncompressed), sheet and header names, code cells, SVS elements, the companions' headers and the code-system version strings (section 7.1)? Nothing enters the repository | **Answered as recommended (2026-11-14).** Yes. It gates EHR-6a's merge (its readers, the size decision and the version mapping). **Done (2026-11-16):** the owner downloaded the release with the office's own UMLS licence; the record is `docs/port-notes/EHR-6.md` section 0 (nothing of the files in the repository); the office picks the workbook sorted by CMS ID, the other two sortings are not needed; EHR-6a-R fixed the readers to it |
| **E-23** | Direct codes must not be in the repository. Where a measure has several LOINC direct codes the program needs (CMS69's BMI; CMS165's systolic and diastolic), how does the importer tell them apart? (a) The catalog holds those three LOINC display names, as the specifications print them, with LOINC's notice (LOINC's licence allows it); codes stay out. (b) The catalog holds a SHA-256 of each needed code; the importer hashes the file's codes (no code text, but short codes can be guessed from a hash; counsel). (c) The office picks the role for those rows once in the import window | **Answered as recommended (2026-11-14).** (a). It ships no code, and the display names are free text under LOINC's licence. CMS69's name and CMS165's two go in `CqmCatalog` (EHR-6a). EHR-6a adds LOINC's notice to `THIRD-PARTY-NOTICES.txt` and the component list of `ReleaseIdentityTests.cs` (section 13.3). Every other role is found by being the only one of its code system in its measure (section 7.3) |
| **E-30** | If the release's workbook is too large for the 32-bit importer: import only the manifest's value sets (the other measures' value sets are then not in `ehrcode`), or raise the limits? | **Answered as recommended (2026-11-14).** Decide from E-20's sizes. The comparison is not "49 against 9 measures": the June 2013 release already covered about 64 (section 7.2). If a sheet passes 64 MB uncompressed, keep only the manifest's value sets; nothing in the program reads the others for a CQM. The catalog holds all seven measures from EHR-6a on, so no second import is needed. **Decided from E-20's sizes (2026-11-16, the lead under this answer):** no sheet of the by-CMS workbook passes 64 MB (the largest 33.1 MB), but the workbook reads about 377 MB in all, over EHR-2's 256 MB: the importer keeps only the manifest's value sets as it streams, and the total limit is 1,024 MB (EHR-6a-R, `docs/port-notes/EHR-6.md` section 14) |
| **E-32** | The recommended default codes: replace the nine encounter codes (SNOMED CT literals for the 2014 versions) with the SNOMED CT codes of the office's imported qualifying-encounter value sets that its `snomed` table holds? And replace the nine recommended pregnancy codes (`FormEhrSettings.cs:120-133`) with the SNOMED CT codes of the imported "Pregnancy" `.526.3.378` that the table holds? | **Answered as recommended (2026-11-14).** Yes to both. Every recommended pregnancy code then falls in "Pregnancy", which CMS69 and CMS155 read |
| **E-37** | The vital-sign windows hold LOINC literals for the height, weight, BMI and BMI-percentile exams (`Vitalsigns.cs:682-714`, `EFormVitalSigns.cs:306-325`, `FormVitalsignEdit2014.cs:314-524`). Keep them, adding LOINC's notice, which the repository lacks today? Or move them to imported value sets and roles? | **Answered as recommended (2026-11-14).** Keep them, with LOINC's notice (the same one as E-23(a)). LOINC's licence allows them, they are data-entry defaults the program has always had, and the engine reads them by value-set membership |

**Scope and names**

| # | Question | Recommendation and answer |
|---|---|---|
| **E-25** | The not-performed "Influenza Vaccination" item fed only CMS147, which is retired. Keep it (old rows readable; new rows pick from the program's CVX table, US-government codes, with a note that no 2027 measure uses it), or remove it deliberately? | **Answered as recommended (2026-11-14).** Keep, with the CVX list and the note |
| **E-31** | Names without a year (`CqmCatalog`, `FormEhrCqm`), so a yearly update edits data and code but adds no files? And is a yearly update (the 2028 versions, May 2027) planned as its own package? | **Answered as recommended (2026-11-14).** Yes to both; a yearly package after each May release, sized from its technical release notes |
| **E-33** | QRDA I: build it although eligible clinicians submit QRDA III (keep everything; the 2014 build had both)? Approve download batch Q (section 15), including the measures' test cases only if CMS offers them without a sign-in? | **Answered as recommended (2026-11-14).** Yes to both |
| **E-38** | The owner's decision 5 keeps NCQA's copyright and disclaimer statements with CMS138, CMS155 and CMS165. The four CMS measures' HQMF files hold the same two kinds of statement (for example `x/CMS68-v16.0.000-QDM.xml:167-186`). Show those the same way (window notes, detail, print-out, QRDA), or only NCQA's? | **Answered as recommended (2026-11-14).** The same way for all seven: one rule, no extra code, and CMS's statements name the code systems' notices too. EHR-6a copies them with NCQA's |
| **E-40** | E-10 (counsel: the steward terms of the 2014 measures' logic and of the 2011 window, and of the manifest's names and categories) gates nothing in this plan now: that code is deleted (decisions 2 and 4) and E-10b is answered (decision 5). Withdraw E-10 from counsel's list? | **Answered as recommended (2026-11-14).** Withdraw it. The catalog's names and categories stay in E-8 (extended, section 11.2). The EHR plan's E-10 row says so (section 16) |

**Schedule**

| # | Question | Recommendation and answer |
|---|---|---|
| **E-36** | In 3a, at most three writers run at once, and the chart track already holds one. Should an EHR package take a writer slot from the stage packages, as the EHR plan's decision 7 allows (one slot)? Or only a slot that no stage or chart package is ready for? | **Answered as recommended (2026-11-14).** Only a free slot. No stage or chart package waits for EHR. Packages that have not merged when 3b starts wait for the switch (section 13.2). At the measured pace the EHR list needs about 6.75 agent-days of 3a's slots (6.0 if EHR-8a1 fits in stage 4; section 13.2), against 3a's 30-47 dev-days of stage work, so free slots should come |

**Answered by the owner (2026-11-14)**

| # | Question | Answer |
|---|---|---|
| **E-21** | The 2011 engine and window (2011 Edition measures, PQRI XML; no button since Open Dental; independent of the 2014 code): keep, or delete deliberately with the 2014 versions? | **Deleted deliberately too**, with the 2014 versions (owner decision 4). Planned in EHR-6b (or EHR-6c if EHR-6b gets too large): the 2011 code of `QualityMeasures.cs` (section 2.1's ranges), its two windows, the PQRI XML export, everything only they use (`QualityMeasure.cs`, `butQuality_Click`, `ProvKeyIsValid`), their tests and their project-file and baseline entries. Saved database rows stay untouched. Every caller is checked (section 2.1). EHR-5d is gone |
| **E-10b** | Does distributing, free and as open source, a program that calculates CMS138, CMS155 and CMS165 need NCQA's approval, and must the three wait for counsel? | **Building them is okay; nothing waits for counsel on this** (owner decision 5). NCQA's copyright and disclaimer statements from the packages stay with those measures wherever they are shown, printed or written to QRDA (section 11.1). The split that kept `CqmCatalogNcqa.cs`, EHR-8c and EHR-8d behind counsel is undone: one catalog in EHR-6a, the normal order in 13.1, the windows and sums in 13.2 |

**Unchanged and superseded**

| # | Question | Recommendation |
|---|---|---|
| **E-1, E-4, E-13, E-15, E-17** (answered 2026-11-08) | Still hold for the 2027 window and QRDA | Unchanged |
| **E-16** (answered 2026-11-08: "leave out current eCQM versions") | Reversed by the owner's decision of 2026-11-14 | The EHR plan's E-16 row and 9.4 say so (section 16) |
| EHR-2's own owner questions 1 and 2 (`docs/port-notes/EHR-2.md`, section 8) | Step 5 (record of the June 2013 package) and the check of the manifest against the June 2013 specifications | Both superseded: E-20 records the 2026-05-14 files, and EHR-6a checks the catalog against the 2027 packages already downloaded |

---

## 15. Downloads (batch Q, approved by the owner on 2026-11-14 with E-33; nothing is fetched by this plan)

The packages that need an item fetch it under this approval, into the lead's download folder, outside the repository.

| Item | Source | Use | Enters the repository? |
|---|---|---|---|
| CMS 2027 QRDA III Implementation Guide for Eligible Clinicians, its schematron and sample file | `https://ecqi.healthit.gov/qrda` | EHR-5b | No (CMS document; the schematron's licence is checked first) |
| HL7 QRDA Category III STU 2.1 | HL7, product brief 286 | EHR-5b | No |
| HL7 QRDA Category I STU 5.3 | HL7, product brief 35 | EHR-5c1, EHR-5c2 | No |
| QDM 5.6 | `https://ecqi.healthit.gov/qdm` | EHR-8, EHR-5c1, EHR-5c2 | No |
| CQL specification (the version the 2027 measures use) | `https://cql.hl7.org` | EHR-8a1 | No |
| CMS eCQM logic and implementation guidance | eCQI | EHR-8, EHR-5 | No |
| The measures' test cases, only if public without a sign-in | MADiE / eCQI | EHR-9 | No: read from `CQM_TESTCASES_DIR` |

- Already downloaded under the approval of 2026-11-14: the seven 2027 packages, measure pages, HTML and TRN files, in the lead's
  download folder, outside the repository. The 2027 EC table PDF is kept there as one reference copy (the lead's decision): it is a
  table, not a measure's package or page, and it is the source of section 4.2's retired measures.
- HL7 documents may need a free HL7 account; whether a sign-in is acceptable is asked when the package that needs the document starts.
- No VSAC, UMLS, CPT, CDT or SNOMED CT file is ever downloaded for the repository. The office's three files stay on the office's computer.

---

## 16. Changes to other documents (lead, with this plan)

Made with this plan on 2026-11-14, except the two that belong to packages: `docs/port-notes/FORK-ENUMS.md` (EHR-7) and the
release notes (EHR-6a).

- **`docs/specs/ehr-rebuild-plan.md`:** the status line names this plan; EHR-2's status line says it merged (`5737519bf`); section 1's
  EHR-5 row points here; section 7's EHR-5 (the 2014 and 2011 windows reopened) is replaced by EHR-5a-5c2, and its 2011 part is gone
  with the 2011 window (owner decision 4); 8.1-8.4 name the 2026-05-14 release (or point to section 7 here); 9.4 and E-16 record the
  owner's decision of 2026-11-14; section 10.1's stage 4 and 3a rows take section 13.2's EHR entries, and 10.2 takes the shared-file rows
  of 13.3; section 13 gains E-20 to E-40, with E-21 and E-10b marked answered on 2026-11-14 (section 14), and its E-10 row says that E-10
  gates nothing now (E-40); line 266's note that the CQM windows keep calling `ProvKeyIsValid` until EHR-5 says that EHR-6b deletes both
  windows and the method; section 17 gives this plan's total as "about 10 agent-days (11 with EHR-9; 10-13 with fix rounds)", labelled
  in agent-days. It says that its own figures are dev-days and that the two are not added.
- **`docs/specs/net10-plan.md`:** S3a-3's EHR-5 note becomes "EHR-6b deletes the 2014 QRDA I zip writer; the 2027 writer (EHR-5c1) stores
  UTF-8 entries itself". The 3a order note says that EHR packages take only free writer slots (E-36).
- **`docs/specs/wpf-tooth-chart-plan.md`, section 3:** EHR-6a joins the "other owners" of `THIRD-PARTY-NOTICES.txt` and
  `ReleaseIdentityTests.cs`, and EHR-6b those of `OpenPmsDental.csproj`, each in stage 4, before TC-9b.
- **`docs/specs/vendor-direct/newcrop.md:236`:** it names `FormEhrQualityMeasures2014.cs:197` as an entry point of the OID registry; after
  EHR-6b that entry point is EHR-5b's export in `FormEhrCqm.cs`.
- **`docs/port-notes/EHR-2.md`:** its owner questions 1 and 2 are superseded (section 14).
- **`docs/port-notes/FORK-ENUMS.md`:** the rule for `EhrNotPerformedItem`, if E-34 adds the item (EHR-7).
- **`docs/owner-questions.md`:** E-20 to E-40; E-21 and E-10b as answered by the owner on 2026-11-14 (section 14).
- **Release notes** (`docs/release-notes/next.md`): EHR-6a rewrites EHR-2's section (section 3); the removal line of section 3, which
  names the 2011 window and its PQRI XML export as well; the NCQA measures by CMS ID and title only, with the pointer to their statements
  in the window (section 11.1).

---

## 17. Risks

- **A misread CQL clause.** One method per `define`, named after it; a test per define; the measures' own cases if available (EHR-9); the
  known-deviation lists.
- **Data recorded the program's way, not the measure's way.** Declined interventions, `WeightCode` findings, dialysis interventions,
  medication rows (read as active and as orders, E-39), problems resolved without a stop date (E-26) and vital signs recorded by the age
  before January 1 (E-35) are mapped explicitly (section 5.3, E-28), each with fixtures, so none counts by accident.
- **Encounters never reach the dental measures** (default encounter codes outside "Clinical Oral Evaluation"). E-24 and E-32.
- **The release is too large for the 32-bit importer.** E-20's sizes and E-30.
- **Direct-code roles that do not resolve** (the office's file differs from the expectation). The import warns per role; the criterion is
  "not calculated", never silently false.
- **Mixed releases in `ehrcode`.** One pick and one swap for the three files (section 7.1).
- **NCQA's statements lost or edited.** The owner's condition (decision 5) holds only if every place that shows, prints or writes an NCQA
  measure carries both statements. The catalog holds them verbatim with their SHA-256, and EHR-5a, EHR-5b and EHR-5c1 each test their
  places (section 11.1).
- **A deletion takes code something else needs.** EHR-6b's caller check against section 2.1's list, the source scan, BSHELLTests and the
  Release dry run; EHR-6c if the check finds more than expected.
- **Licensed codes in the repository**, including in this document and in port notes. The catalog guard covers `CqmCatalog.cs`; port
  notes cite CQL file paths, never codes.
- **The 2027 QRDA III guide is not published yet.** EHR-5b starts from the 2026 guide and does not merge until the 2027 one is checked.
- **3b arrives before the EHR list is done.** Nothing merges in 3b; the rest is finished after the switch (section 13.2).
- **The yearly update.** CMS publishes new versions every May; the catalog and per-define methods keep the update small (E-31).
- **Numbers that look official.** The caption (E-1), the notes on criteria with no data, and no submission path.

---

## 18. Sources

| Group | Source |
|---|---|
| 2027 measure pages | `https://ecqi.healthit.gov/ecqm/ec/2027/cms0068v16`, `cms0069v15`, `cms0074v16`, `cms0075v15`, `cms0138v15`, `cms0155v15`, `cms0165v15` |
| 2027 packages | `https://ecqi.healthit.gov/sites/default/files/ecqm/measures/CMS68-v16.0.000-QDM.zip`, `CMS69-v15.0.000-QDM.zip`, `CMS74-v16.0.000-QDM.zip`, `CMS75-v15.0.000-QDM.zip`, `CMS138-v15.0.000-QDM.zip`, `CMS155-v15.0.000-QDM.zip`, `CMS165-v15.0.000-QDM.zip`, with their `.html` and `-QDM-TRN.xlsx` |
| 2027 table | `https://ecqi.healthit.gov/sites/default/files/2027-EligibleClinician-eCQM-Table.pdf` |
| Retired measures | `https://ecqi.healthit.gov/revised-ecqm-files-and-measures-table-2024-ec-performance-period`; CY 2024 PFS final rule (88 FR 78818) (summary); `https://oncprojectracking.healthit.gov/support/browse/CQM-6711` (summary) |
| Local copies | The lead's download folder, outside the repository, one folder per CMS ID (file list and SHA-256 per file in the research results of 2026-11-14) |
| QRDA, QDM, CQL (not opened) | `https://ecqi.healthit.gov/qrda`; HL7 product briefs 286 and 35; `https://ecqi.healthit.gov/qdm`; `https://cql.hl7.org` |
| Test cases (not opened) | `https://madie.cms.gov`; `https://bonnie.healthit.gov` |
| VSAC | `https://vsac.nlm.nih.gov/download/ecqm`; the EHR plan's section 18 |
| Repository | `main` `5737519bf`; `docs/specs/ehr-rebuild-plan.md`; `docs/specs/net10-plan.md` (stage 4, 3a order 701-702, 3b 852); `docs/specs/wpf-tooth-chart-plan.md` (sections 2, 3, 6); `docs/port-notes/EHR-0.md`, `EHR-1.md`, `EHR-2.md`, `EHR-3.md`, `EHR-4.md`; `docs/port-notes/FORK-ENUMS.md`; `tools/project-baseline/README.md` |

---

## Appendix. Changes from the critique, the check and the owner's answers (2026-11-14)

**First revision: the critique.**

| # | Finding | Where it is handled |
|---|---|---|
| 1 | Old base: EHR-2 shown as running; the merged release notes tell offices to import June 2013 | Basis on `5737519bf`; the EHR-2 rows of sections 1 and 14 deleted; EHR-6a owns `next.md:26-28`, `492-517` (sections 3, 12, 13.3); the lead's fix if a release comes first (13.2) |
| 2 | CMS69's exceptions: "Patient Declined" only for the BMI exam | Sections 5.4 and 6; fixture in 10.2 |
| 3 | CMS69's follow-up exception has no data | Section 5.3 row "none"; E-34; EHR-7; deviation list |
| 4 | Over- and underweight recorded as `vitalsign.WeightCode`, never a problem | Section 5.3 Diagnosis row; E-35 (with the 2014 BMI ranges); section 8; fixtures in 10.2 |
| 5 | Declined interventions counted as performed | Section 5.3 row; 5.4 (CMS138, CMS155); fixtures in 10.2 |
| 6 | "Dialysis Services" interventions never read by CMS165 | E-28 (read as Procedure, Performed); sections 5.3, 5.4; fixture in 10.2 |
| 7 | The birthdate rule removes CMS68 encounters | Section 6 (only measures with an age criterion); 5.4; fixture in 10.2 |
| 8 | Data-entry code literals missing from section 8 | Section 8's second table; EHR-7's Owns and size; decision 5 rewritten; E-32 extended; E-35, E-37 |
| 9 | `ValueSetDbTests.cs` not in EHR-7's Owns | EHR-7's Owns; section 13.3 row (EHR-6a, then EHR-7) |
| 10 | Direct-code rows lost by a later value-set import | Section 7.1 (one pick, one swap); 7.3; 7.5; EHR-6a's tests |
| 11 | "Nine measures" of the June 2013 release | Section 7.2 Size (about 64); E-30 |
| 12 | 73 and 33 included five code-system OIDs | Section 7.2 (68, 72 in all, 28 new; 41 of 81 gone); EHR-6a's test counts `valueset` lines only |
| 13 | NCQA content in EHR-6a before E-10b | `CqmCatalogNcqa.cs` in EHR-8c; sections 5.2, 7.2, 7.3, 11.1; report "4 of 4", then "7 of 7" (superseded by D5 below) |
| 14 | Population groups in EHR-8c | The whole result shape in EHR-8a2 (5.2); EHR-5a-5c do not wait for counsel (section 9, 13.1); since D5 nothing waits |
| 15 | No windows against the .NET 10 stages | Section 1's column; each package's Window; section 13.2; E-36 |
| 16 | `THIRD-PARTY-NOTICES.txt` and AttributionTests without an owner | EHR-6a's Owns; section 13.3 rows; E-23, E-37 |
| 17 | Project-file and baseline entries of the deleted forms | Sections 2.1, 3; EHR-6b's Owns and Window; section 13.3 rows |
| 18 | Sizes: EHR-8a, EHR-5c, EHR-7, units | EHR-8a1 and 8a2; EHR-5c1 and 5c2; EHR-7 L; EHR-5b M-L; total 10 (11), 10-13 with fix rounds; the units paragraph in section 1 and section 16 |
| notes | Legal authenticator and OID registry; `newcrop.md:236`; medication Order and Active; QRDA III payer; Middle Tier namespace; `GetForValueSetOIDs(...,false)`; E-24's deviations; CMS155's clinicians; hospice interventions with no pick list; version formats; `MeasureCalcFixture.cs`; 9.3's zips; CMS69's performed follow-up | Sections 3, 9.2, 16; 5.3; 9.2; 5.2; 5.2; E-24 and 5.4; 5.4 and EHR-5a; E-27; 7.4; 10.2 and 13.3; 9.3; 5.4 |

**Second revision: the check (findings 1-5) and the owner's answers to E-21 and E-10b.** Each finding was verified against `main`
(`9bf1de770`) before it was fixed.

| # | Finding or decision | Where it is handled |
|---|---|---|
| C1 | `medicationpat` sorted into Order and Active by `PatNote`, a free dosage note (`MedicationPat.cs:15-17`) | Section 5.3: every row is Medication, Active over `DateStart`..`DateStop` and also Medication, Order on `DateStart`; E-39 (when the Order reading applies); 2.1's note; fixtures in 10.2; EHR-8a1's work; risks |
| C2 | Diagnosis prevalence ignored `ProbStatus` (`Disease.cs:25-30`; `FrmDiseaseEdit.xaml.cs:127`, `178`; `Diseases.SetDiseaseFields`) | Section 5.3's status rule; E-26 extended (`DateTStamp` end recommended; unset `DateStart` open at the start); fixtures in 10.2; EHR-8a1, EHR-8d |
| C3 | Percentile and exam code recorded only at 3-16 before January 1 and only with the LOINC table loaded; CMS155 is 3-17 at the end | E-35 extended; section 5.3's CMS155 row; section 8's new row; EHR-7's Owns, Work, Tests and size (1.25); fixtures in 10.2 |
| C4 | Not-performed "other" rows would throw on double-click (`FormEhrNotPerformed.cs:123`, `Enum.Parse` of the label) | Section 8's row: the item in the row's `Tag`, "other" opens read-only; EHR-7's Work and Tests |
| C5 | 3a's agent-days did not match section 1's sizes | Sums restated from the table: stage 4 3.25, 3a 6.75 (4.0 and 6.0 if EHR-8a1 lands in stage 4), total 10 (section 1, 13.2, E-36, summary) |
| D4 | E-21 answered: the 2011 engine and window deleted too | Owner decision 4; sections 1, 2.1 (outside callers), 3, 5.2, 9, 10.1 (fixture guard floor), 12 (EHR-6b, EHR-6c if split; EHR-5d removed), 13, 14 (answered), 16, 17 |
| D5 | E-10b answered: the NCQA measures built; their statements kept | Owner decision 5; `CqmCatalogNcqa.cs` folded into `CqmCatalog.cs` (EHR-6a, 1.25; EHR-8c 1); report "7 of 7" from EHR-6a; the NCQA packages before QRDA (13.1); section 11.1 (where the statements are held, shown, printed and written); 9.2, 9.3; EHR-5a, 5b, 5c1; E-38, E-40 new |

**Third revision: the owner's answers to section 14 (2026-11-14).**

| # | Decision | Where it is handled |
|---|---|---|
| D6 | Every question of section 14 answered "all as recommended" (E-20, E-22 to E-40) | Status line; summary; section 14 (each row marked answered, and the conditions of the form "if E-n is answered as recommended" hold); section 16 carried out in the other documents |
| D7 | Download batch Q approved with E-33; the 2027 table PDF kept as one reference copy (the lead's decision) | Summary; Basis; sections 4.1 and 15 |
| D8 | Paths of the lead's machine and tools left out of the document | Basis, sections 4.1, 7.2, 11.2, 15 and 18 name the lead's download folder, outside the repository |
| D9 | NCQA's own text not repeated in the document (only titles and population names) | Section 11.1's terms and CMS155's clinician note in summary; CMS138's visit and pharmacotherapy definitions described, not named (sections 5.3, 5.4) |
