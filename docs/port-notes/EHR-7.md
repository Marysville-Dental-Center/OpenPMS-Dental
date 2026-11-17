# Port note: EHR-7 (data entry on the 2027 catalog)

Package EHR-7 of `docs/specs/ehr-2027-measures-plan.md` (below: "the plan"): its row in section 1, section 8, the package text in section 12,
and the owner's answers to E-25, E-28, E-32, E-34, E-35 and E-37 (all as recommended, 2026-11-14). Branch `ehr/ehr7-1`, **stacked on EHR-6a**
(`ehr/ehr6a-5`, `f5137aa21`) with `main` merged in. No schema change, no fork step, no `PrefName`, no project file edited (the business
project and `tests/EhrTests` glob their files), no package, no download, no code of any code system added anywhere. Synthetic data and
`TEST` codes only.

**Merge gate.** EHR-7 merges **with EHR-6a, and so only after E-20's record** (the owner's record of the three files of the 2026-05-14
release; `docs/port-notes/EHR-6.md`, section 0). It builds on EHR-6a's catalog, roles and three-file import, and must merge before S3a-M3
starts (3a wave 2 owns `FormMedical.cs`).

## 0. Step 0: `main` merged into EHR-6a (`b0fec2645`)

`main` brought EHR-6b (the 2014 and 2011 quality-measure code deleted) and EHR-4b. Three files conflicted; each keeps both packages' intents,
as `docs/port-notes/EHR-6b.md` section 7 asks:

- `docs/release-notes/next.md`: EHR-6a's summary line (the 2027 release, its three files, the catalog) with EHR-6b's parenthesis (the 2014
  and 2011 quality-measure windows removed). EHR-6b's own line in the Measure Calc section merged without conflict.
- `docs/specs/ehr-2027-measures-plan.md`: both status sentences.
- `tests/EhrTests/ValueSetSourceTests.cs`: EHR-6a's header, manifest test and allowlist, with `QualityMeasures.cs` taken off the allowlist
  (the file is gone; the stale-entry check would fail) and EHR-6b's `COUNT_OIDS_ONLY_THE_DELETED_ENGINE_NAMED` and its 2014 manifest test
  dropped (EHR-6a's catalog replaces both). No reflection of the deleted engine. The fixture guard keeps EHR-6b's single path
  (`EhrMeasures.cs`) and floor of 21 (`COUNT_ENGINE_CODES_FLOOR`) with EHR-6a's GUID skip; the run confirms 21 still holds.

Also checked from EHR-6b's note: `EhrCodes.GetCodeAndCodeSystem` is still at `EhrCodes.cs:286` on this branch and `RemovalTests` pins it
(passes); the stale comments EHR-6b named in `EhrCodes.cs` (`:538`, `570`, `611`, `614` on `main`) are EHR-6a's rewritten lines, which name
none of the deleted code. `RemovalTests` passes on the merge (no deleted name outside comments).

Runs on the merge alone (full lane build, 0 errors), `ehr7-merge.txt`: EhrTests 282 (269 passed, 13 skipped: the EHR-4 deviation tests
`*_D01` to `*_D15`, skipped on `main` too), NoHqTests 57, ProjectFileTests 150 (149 passed, 1 skipped as on `main`), AttributionTests 50,
MiddleTierMockTests 125; none failed.

## 1. The catalog's constants (`OpenPmsDentalBusiness/Db Multi Table/Cqm/CqmCatalog.cs`)

The package's Work ("each OID becomes a catalog constant or a direct-code role") needs the constants in the catalog file, the only file the
allowlist test lets name a value-set OID, so this EHR-6a file gains (nothing of it changes otherwise):

- 17 `OID_*` constants, one per pick list of section 8 (EHR-6a's `_arrayPickLists`, which now uses them): `OID_BMI_FOLLOW_UP_ABOVE`,
  `OID_BMI_FOLLOW_UP_BELOW`, `OID_BMI_REFERRALS`, `OID_BMI_MEDICATIONS_ABOVE`, `OID_BMI_MEDICATIONS_BELOW`, `OID_COUNSELING_NUTRITION`,
  `OID_COUNSELING_PHYSICAL_ACTIVITY`, `OID_TOBACCO_COUNSELING`, `OID_TOBACCO_PHARMACOTHERAPY`, `OID_DIALYSIS_SERVICES`,
  `OID_TOBACCO_SCREENING`, `OID_TOBACCO_USER`, `OID_TOBACCO_NON_USER`, `OID_PREGNANCY`, `OID_REASON_ACUTE_HEALTH_CRISIS`,
  `OID_REASON_MEDICAL`, `OID_REASON_PATIENT_DECLINED`.
- `ROLE_CMS68_MEDS_DOCUMENTED` and `ROLE_CMS69_BMI`, the two roles the windows read (the role table uses them).
- `GetQualifyingEncounterValueSets()` (E-32): the 22 Encounter value sets of the seven measures that count a visit. Left out are the five
  Encounter value sets that only exclude a patient: Hospice Encounter, Palliative Care Encounter, Frailty Encounter, Encounter Inpatient
  and Emergency Department Evaluation and Management Visit (CMS165's acute-inpatient-or-ED exclusion). The plan does not list the sets; this
  is the package's reading of "qualifying-encounter value sets", for the owner to confirm (section 7).

## 2. The data-entry rules (`OpenPmsDentalBusiness/Db Multi Table/Cqm/CqmDataEntry.cs`, new)

One business class for what the windows read, so the rules are tested without a window; each lookup has a form that takes the value-set
rows (tested in memory) and one that reads `EhrCodes.Listt`. It names no OID and no code. Every method is `Meth.NoCheckMiddleTierRole` and
calls only methods that pass the Middle Tier themselves (`EhrCodes.Listt`, `Snomeds.GetByCodes`, `Cvxs.GetAll`); no new Middle Tier method.

- `GetDirectCode(role)`: the role's one code from the import (`drc:` key), or null when the role matched none or more than one (the import's
  report warns of both). The same code listed twice is one code.
- Not performed: `GetNotPerformedRole`, `GetNotPerformedValueSetOids`, `GetNotPerformedReasonOids(item,isPatientReason)`,
  `GetNotPerformedCodes(item)` and `GetNotPerformedItem(code,codeSystem)` (section 4).
- Interventions: `GetInterventionValueSets(codeSet,isAllTypes)` (the window's list, label and value set), `GetMedicationInterventionOids()`,
  `GetMedicationCodeSet(rxCui)`.
- Recommended codes (E-32): `GetRecommendedEncounterCodes()` and `GetRecommendedPregnancyCodes()`: the SNOMED CT codes of the imported
  qualifying-encounter value sets, or of "Pregnancy", that the office's `snomed` table holds, each once, in code order, read in queries of at
  most 500 codes (`Snomeds.GetByCodes`). `GetSnomedCodes` is the in-memory part.

## 3. Value-set OIDs (section 8's first table): every window on the constants

| Where | Before | Now |
|---|---|---|
| `FormInterventionEdit.cs` | the 2014 BMI follow-up, referral, medication (`.600.1.1498`, `.1499`), counseling, tobacco and dialysis (`.109.12.1016`, `.1015`) OIDs | `CqmDataEntry.GetInterventionValueSets`: the 2027 medications `.526.3.1561`, `.1562`; one list "Dialysis Services" `.109.12.1013` (E-28); the rest as before |
| `FormInterventions.cs:101` | `.600.1.1498`, `.1499`, `.526.3.1190` | `CqmDataEntry.GetMedicationInterventionOids()` |
| `FormVitalsignEdit2014.cs:394` | `.600.1.1498`, `.1499` | `OID_BMI_MEDICATIONS_ABOVE`, `OID_BMI_MEDICATIONS_BELOW` |
| `FormMedical.cs:879-940,1037`, `FormEhrPatientSmoking.cs:108-161,238` | the five tobacco OIDs | `OID_TOBACCO_SCREENING`, `_USER`, `_NON_USER`, `_COUNSELING`, `_PHARMACOTHERAPY` |
| `FormEhrNotPerformedEdit.cs:30-46,101-106` | the BMI exam `.600.1.681`, influenza `.526.3.402`/`.1254`, tobacco `.1278`, current medications `.600.1.462`, six reason sets | section 4 |
| `DiseaseDefs.cs:404` | `GetCodesExistingInAllSets` of `.600.1.1623` and `.526.3.378` | `GetCodesExistingInAllSets(new List<string> { CqmCatalog.OID_PREGNANCY })`: "Pregnancy" alone |

`ValueSetSourceTests`' allowlist is now empty (and a test holds it empty): no value-set OID literal outside `CqmCatalog.cs`.

## 4. The not-performed windows (section 8, E-25, E-34)

- **The fifth item (E-34).** `EhrNotPerformedItem.BMIFollowUp` = 4 (`TableTypes/EhrNotPerformed.cs`). The enum is not stored and the windows
  index it by position, so the fork-only value is the next number, not one from 10000; `docs/port-notes/FORK-ENUMS.md` has its own rule and
  lists it. It picks from the BMI follow-up, referral and BMI medication sets (five; a code in two is listed once) with "Medical Reason" as its
  only reason. Old rows are unaffected.
- **Codes per item** (`GetNotPerformedCodes`): BMIExam the `Cms69Bmi` code; DocumentCurrentMeds the `Cms68MedsDocumented` code; TobaccoScreening
  "Tobacco Use Screening" (the item stays; CMS138v15 has no exceptions); BMIFollowUp the five sets; InfluenzaVaccination (E-25) every code of
  the program's CVX table, with the note "no 2027 quality measure uses it" in the item box and the pick prompt.
- **Reasons per item** (`GetNotPerformedReasonOids`): BMI exam "Medical Reason" and "Patient Declined"; current medications "Acute Health
  Crisis"; BMI follow-up and tobacco screening "Medical Reason"; influenza "Medical Reason" or (its Patient radio) "Patient Declined". The
  System radio is hidden: the 2027 release has no value set of system reasons.
- **A saved reason outside the 2027 lists** (a 2014 reason, a system reason) is added to the reason list as the row's own, so saving the row
  again never drops it (before, the reason combo fell back to "none" and the save cleared it).
- **The grid** (`FormEhrNotPerformed`): each row is labelled by `GetNotPerformedItem` (the two roles, "Tobacco Use Screening", the five
  follow-up sets, a CVX code), read once from `EhrCodes.Listt` for the grid, by membership (not `IsInDb`); anything else is "other" (it was
  "influenza"). The row's `Tag` holds the item, or null. The double-click reads the `Tag` (`as EhrNotPerformedItem?`); the `Enum.Parse` of
  the label, which threw on "other", is gone. The grid also describes HCPCS, ICD-10-CM and RxNorm codes (the follow-up item's).
- **Read-only "other"** (`FormEhrNotPerformedEdit.IsReadOnly`): an "other" row, or any call with no valid item index, opens with its code,
  code system, reason, date and note, with Save, Delete and the reason list disabled, before the item index is used, so no index -1 reaches
  the item switch. A new row of an item whose list is empty is refused with the message to run the Code System Importer, as before; a saved
  row opens even when its item's list is empty.
- Before the office imports the 2027 release, every saved row but the CVX ones is "other": their codes are known only from the import
  (`GetNotPerformedItem` reads roles and value sets). The release notes say so, and so do the windows (section 11):
  `CqmDataEntry.IsReleaseImported()` (the last import's record is the 2027 release, by `CqmCatalog.RELEASE_LABEL`) is false, the grid
  labels such a row "other (2027 value sets not imported)" and the read-only window's title and description say the value sets are not
  imported and where to import them. The plain "other" and "read-only, the code is of no item" stay for an office that has imported it.

## 5. Code literals (section 8's second table)

| Where | Now |
|---|---|
| `FormMedical.cs:1350` (writes the CMS68 SNOMED CT code) | writes `GetDirectCode(ROLE_CMS68_MEDS_DOCUMENTED)`'s code and code system; with no role resolved it shows "Current medications documented was not recorded: its code comes from the EHR value sets of the 2027 release, which are not imported. ..." and writes nothing (the rest of the save goes on) |
| `FormMedical.cs:104` (filters the not-performed rows by that code) | the same role's code and code system; none without it |
| `FormEhrNotPerformed.cs:52-66` (the BMI, current-medications and three tobacco literals; every other row "influenza") | section 4 |
| `FormInterventions.cs:120-125` (RxCui literals of the 2014 BMI medication sets; every other "tobacco") | `GetMedicationCodeSet`: the imported value set holding the RxCui (`.1561`, `.1562`, `.1190`) |
| `FormEhrSettings.cs:105-133`, `FormEncounterTool.cs:43-55` (nine encounter and nine pregnancy SNOMED CT literals) | `GetRecommendedEncounterCodes` / `GetRecommendedPregnancyCodes` (E-32). Both windows list only codes the table holds, so the per-code `Snomeds.CodeExists` loop is gone, and with it a mismatch of list and combo indexes in `FormEhrSettings` when a code was missing. An empty list says to import the value sets and SNOMED CT. A default saved before keeps working (shown in the code box) |
| `Vitalsigns.cs:682-714`, `EFormVitalSigns.cs:306-325`, `FormVitalsignEdit2014.cs:314,343,507,524` (LOINC exam codes) | kept (E-37), with LOINC's notice (EHR-6a) |
| `Vitalsigns.cs:717-733`, `EFormVitalSigns.cs:327-334` (the two SNOMED CT weight codes) | kept until counsel's E-8 (E-35) |
| `Vitalsigns.cs:116-136` (`GetBMIInterventionCode`) | the 2027 range for every adult, 18.5 to under 25 (E-35) |

## 6. The vital signs' age rule (E-35)

`Vitalsigns.GetAgeOnDateTaken(birthdate,dateTaken)`: the age in whole years on the day taken, as CQL's `AgeInYearsAt` (less one before that
year's birthday; a February 29 birthday counts from March 1 in a common year); -1 without a birth date (before 1880) or for a day before the
birth. Its groups: `IsBMIPercentileAge` 3 to 17 (`AGE_BMI_PERCENTILE_MIN`, `_MAX`; the LMS table covers 36 to 215 months),
`IsWeightClassAge` 18 and over (`AGE_ADULT`), `IsChildCounselingAge` 0 to 17. The vital sign window (`FormVitalsignEdit2014`: its public
`AgeBeforeJanFirst` is now `AgeOnDateTaken`, set on load and when the date changes, and every use at 305, 328, 335, 350, 688 and 708 goes
through the helper) and the eForm import (`EFormVitalSigns.CreateVitalsign`) both call it; `GetBMIInterventionCode` takes it.
`GetBMIPercentile` returns -1, not a `NullReferenceException`, when no LMS row matches (and for a BMI of 0 or no table).

So a 17-year-old (by the old rule 16 before January 1, or 17 and in neither group) now gets a percentile and the child counseling group; an
18-year-old the class. Rows saved before keep their percentile and weight code until they are saved again (E-35's answer).

**Superseded by EHR-7-R (section 12):** the vital sign window classes a patient without a birth date as an adult again. The deviation
as first written: a patient without a birth date is now neither a child nor an adult (`GetBMIInterventionCode` gives None, no
percentile, no intervention group). Before, the window's age before January 1 of a missing birth date was about 2,000 years, so such a
patient was classed as an adult. The measures cannot count a patient without a birth date either way.

What the engine does with these rows (a value with an empty exam code read as that exam; `WeightCode` by membership; E-28's intervention
rows as Procedure, Performed) is EHR-8's, not this package's.

## 7. Choices and open points

- `CqmCatalog.cs` is EHR-6a's file and not in this package's Owns; section 1's additions are the "catalog constants" the Work names, and the
  file is on this stacked branch, which merges with EHR-6a. `CqmDataEntry.cs`, `PickListTests.cs`, `PickListDbTests.cs`,
  `VitalSignAgeTests.cs` and `Support/PickListDb.cs` are new files.
- The qualifying-encounter value sets (section 1) are the package's reading of E-32; the owner may want fewer (for example only the dental
  and office-visit sets).
- The influenza item's reasons (section 4) are the package's choice: E-25 keeps the item but names no reasons; "Patient Declined" stands
  in for the 2014 "Patient Reason", and system reasons have no 2027 set.
- (Superseded by EHR-7-R, section 12: Delete is enabled again.) The read-only "other" row has Delete disabled too (the plan says "read-only ... no save"). An office that wants such a row gone deletes it
  after importing the release, when its code usually resolves to an item.
- `FormEncounterTool`'s Snomed button compares from index 1 of its combo, which has no "none" row, so picking the first recommended code
  through the SNOMED CT list does not select it in the combo. Open Dental's code; left as it was (not section 8's).
- `FormVitalsignEdit2014.cs:314-524` (the LOINC literals) were not touched (E-37 answered as recommended); line 821 (`SetWeightCodes` on
  save) needs no change, since `GetBMIInterventionCode` now gives the 2027 range.

## 8. Tests

New, no database:

- `PickListTests` (12): the 17 constants equal the catalog's pick lists; the 22 qualifying-encounter sets without the five exclusions; a
  role's code (none before the import, none for two codes); `EhrNotPerformedItem` 0 to 4 and FORK-ENUMS.md's row; each item's roles, value
  sets and reasons, all catalog pick lists; the item of a saved row by role, catalog and CVX, "other" (null) for an unknown code, a wrong
  code system, another value set, or before the import; source checks of the grid (`row.Tag=itemNotPerformed`, "other", no influenza
  default), the double-click (the `Tag`, no `Enum.Parse`, no cell read, `IsReadOnly`), the edit window (read-only first, Save and Delete
  disabled); the intervention lists (Dialysis Services alone, every label's first word a code set) and the medication labels; the medical
  window (the role's code, the insert only in the else of the null check, the message); no code literal and no value-set OID in the
  windows; the recommended codes picked from the rows.
- `VitalSignAgeTests` (6): the age on the day before and the day of the 3rd, 17th, 18th and 65th birthdays (and February 29); the groups;
  the 2027 range at 18, 40, 64, 65 and 80 (22 is normal and 27 overweight at 65 and over); a percentile at 17 years and 11 months and at 3,
  -1 at 18, at 35 months, at 40, with an empty table, a BMI of 0 or an unknown sex; the eForm import at 17, 18, 3 and the day before 3, and at
  70; the window's source.

New, on the `ehr7_` scratch database (`Support/PickListDb.cs`, as EHR-6a's `ReleaseDb` with its own prefix; built from
`db/init/new-database.sql` and the fork steps, dropped at the end):

- `PickListDbTests` (4): from an imported synthetic release (three files) and synthetic code tables, each not-performed item's codes and
  reasons, the CVX list, the item of each imported code, and no role code once the rows are gone; the intervention window's eleven lists,
  the medication labels and the Tobacco Use lists; the recommended encounter codes (not the CPT row, not a code missing from the table, not
  Hospice Encounter's) and pregnancy codes, and 1,203 codes over three queries; on a Middle Tier client (mock server) the role's code, both
  recommended lists, the CVX list and the pregnancy problems.

Changed: `ValueSetSourceTests` (the allowlist empty, held empty); `ValueSetDbTests` (the tobacco windows by the catalog's constants, the
constants equal the OIDs, and the windows name no OID; pregnancy from "Pregnancy" alone: a code in it is found with or without the 2014
set, a code only in the 2014 set is not, and `DiseaseDefs.cs` no longer names that set).

## 9. Runs

Full build through the lane: 0 errors, no warning in a changed file. Targeted runs, `ehr7-1-run2.txt` (after the full build):

| Assembly | Result |
|---|---|
| EhrTests | 304: 291 passed, 13 skipped (the EHR-4 deviation tests, as on `main`), 0 failed |
| NoHqTests | 57 of 57 passed |
| ProjectFileTests | 150: 149 passed, 1 skipped (as on `main`) |
| AttributionTests | 50 of 50 passed |
| MiddleTierMockTests | 125 of 125 passed |
| RegistryTests | 39 of 39 passed (the enum's new value, FORK-ENUMS.md) |
| P262MiscTests | 75 of 75 passed (`EFormVitalSigns` changed) |

`ehr7-1-run1.txt` (EhrTests and NoHqTests, before the docs) had the same EhrTests result. No `ehr7_` database was left on the server.
InitSqlTests and ForkMigrationTests were left out: nothing here touches the schema or `db/init/new-database.sql`.

## 10. Owner checks (plan, EHR-7)

With a test database, EHR on, the synthetic release imported (`ValueSetFixtures.MakeRelease`, written from a test) and synthetic SNOMED CT,
LOINC and RxNorm rows: the medical window's Tobacco Use tab; the intervention window's lists (BMI medications, Dialysis Services); the
not-performed window: BMI exam, current medications and the new BMI follow-up item, a row of an unknown code shown as "other" and opening
read-only; Setup > EHR settings and the encounter tool: the recommended encounter and pregnancy codes; a vital sign of a 17-year-old (a
percentile and the child group) and of an 18-year-old (the class).

## 11. Fix round (`ehr/ehr7-3`, from `ehr/ehr7-1`)

The review of `ehr/ehr7-1` (`eb7364c2c`) asked for a fix branch. No branch `ehr/ehr7-2` was ever made and no record of a first round's
findings exists in the repository or the worktree, so this round fixes the two findings the review names, on `ehr/ehr7-3` as the fix task
named it; the round-2 check reads `ehr/ehr7-1..ehr/ehr7-3`.

- **The not-performed windows before the import** ("empty lists must say why"). `CqmDataEntry.IsReleaseImported()` and
  `IsReleaseImported(EhrValueSetRelease)` (true only for a record of the 2027 release; no record, or EHR-2's June 2013 record, is false).
  `FormEhrNotPerformed.FillGrid` reads it once and labels an "other" row "other (2027 value sets not imported)" while it is false;
  `FormEhrNotPerformedEdit.FillReadOnly` then titles the window "read-only, the 2027 value sets are not imported" and writes in the item's
  description: "The EHR value sets of the 2027 release are not imported (Setup | Chart | EHR, Code System Importer); until then this row's
  item is not known." Delete stays disabled (section 7).
- **The intervention window's empty list and a saved code outside the 2027 lists.** `FormInterventionEdit.FillGrid`: when the selected
  lists give no code, the grid's title adds "none: the codes of these intervention lists come from the EHR value sets of the 2027 release
  and the code-system tables; run the Code System Importer in Setup | Chart | EHR."; on load of a new entry (selection mode) with no code in
  any list the same message is shown once. A saved intervention (`!IsNew`) whose code is not in the list (a 2014 Dialysis Education or
  Related Services row after E-28, a row saved before the import) is added as a row of its own, marked "(saved code, not in this list)",
  and selected like any other, so Save keeps it, as `FillReasonList` keeps a saved reason. Save with "All" of a code in no list keeps the
  row's code set (it read `listVSFound[0]` of an empty list). The description lookup moved to `GetDescription(EhrCode)`.
- **Tests.** `PickListTests` (15): `ReleaseImported_OnlyThe2027ReleasesRecord`, `NotPerformed_OtherRows_SayWhenTheReleaseIsNotImported`
  (both labels and both titles, in order), `Interventions_AnEmptyListSaysWhy_AndASavedCodeOutsideTheListsIsKept`. `PickListDbTests`: not
  imported before the synthetic import, imported after it.
- **Runs.** Full build through the lane: 0 errors. Targeted run `ehr7-3-targeted.txt`: EhrTests 307 (294 passed, 13 skipped, the EHR-4
  deviation tests as before, 0 failed), NoHqTests 57/57, ProjectFileTests 150 (149 passed, 1 skipped), AttributionTests 50/50,
  MiddleTierMockTests 125/125, RegistryTests 39/39, P262MiscTests 75/75.
- The merge gate is unchanged: EHR-7 merges with EHR-6a, after E-20's record.

## 12. Pre-merge round EHR-7-R (`ehr/ehr7r-1`, from `ehr/ehr7-3`)

E-20's record is made and EHR-6a is in `main` (`31b088f03`, with EHR-6a-R's record and reader fixes, `docs/port-notes/EHR-6.md` sections 0
and 14), so EHR-7 merges now. This round merges `main` in, checks EHR-7 against EHR-6a-R's API, and makes the two fixes the lead decided on
2026-11-15 (`.git/od-tools/briefs/ehr6a-fix4.md`, tail). The other three EHR-7 decisions there stay as they are: E-32's 22
qualifying-encounter sets, E-25's influenza reasons (System Reason hidden, saved ones kept), and the empty-intervention message on load for
new entries only. No schema change, no fork step, no project file, no package, no download; synthetic `TEST` codes only.

**The merge of `main`** (`579d12f75`). `main` brought EHR-6a with EHR-6a-R, stage 3a (S3a-M3b among it) and the stage 4 work. Four files
conflicted; each keeps both sides:

- `docs/release-notes/next.md`: the summary line keeps EHR-7's list (tobacco use, interventions, not performed with the BMI follow-up item,
  pregnancy, the recommended default codes, the vital signs' age and BMI range) on EHR-6a-R's wording. The Code System Importer section took
  EHR-6a-R's new code-system status text and EHR-7's "Every EHR pick list reads the 2027 value sets" without a conflict.
- `docs/specs/ehr-2027-measures-plan.md`: EHR-6a-R's status (E-20's record, the readers fixed) and EHR-6b's sentence, then EHR-7's.
- `tests/EhrTests/ValueSetSourceTests.cs`: EHR-7's side whole: the allowlist is empty and held empty. `main`'s side differed only in the
  allowlist (the seven EHR-7 files) and its comment.
- `tests/EhrTests/ValueSetDbTests.cs`: EHR-7's pregnancy test ("Pregnancy" alone, by the catalog's constant; the 2014 diagnosis set gone
  from `DiseaseDefs.cs`) with EHR-6a-R's two checks that the import keeps "Pregnancy" and not the 2014 set (E-30 keeps only the catalog's
  value sets).

`OpenPmsDental/Forms/FormMedical.cs` merged without a conflict. S3a-M3b's lines (`InitOtherConvertMenus` and its two fields: the
`ToolStripMenuItem`s, the `ContextMenuStrip`s, the `Opening` lambdas, `DoAddGridItemsToContextMenuStrip=true`) are byte for byte as on
`main` (no menu line differs from `main`); EHR-7's lines (the role's code of CMS68, the tobacco constants, the not-performed filter) are all
outside that region, as `docs/port-notes/S3a-M3.md` (section "S3a-M3b", "For EHR-7's later merge") expected. The merge built through the x86
lane with 0 errors before any change.

**EHR-7's calls into EHR-6a after EHR-6a-R.** Each re-checked; none needed a change:

- `CqmCatalog.RELEASE_LABEL` is now the record's release title ("VSAC eCQM Value Sets (EC, HOSPIP and HOSPOP), eCQM Update 2026-05-14,
  2027 performance period"). `CqmDataEntry.IsReleaseImported` compares with the constant, and `PickListTests` uses the constant and a
  made-up "TEST earlier release", so nothing names the old label.
- E-30 (the import keeps only the catalog's value sets): every OID the windows and `CqmDataEntry` read is one of the 17 `OID_*` constants
  or `GetQualifyingEncounterValueSets()`, all catalog value sets, so every list still fills. The two window OIDs EHR-6a-R named as present
  in the release under other measures (`.526.3.1008`, `.526.3.1259`) are gone from the windows since EHR-7 (the allowlist is empty).
- `ValueSetFixtures` (`MakeRelease`, `MakeWorkbook`, `ParsePick`, the direct-code rows) keep their signatures; `PickListDb` copies
  `ReleaseDb`, which EHR-6a-R did not change. `EhrValueSetImport.Import`, `GetReport`, `GetRelease` and `PREF_RELEASE` are unchanged.
- The direct reference codes' rows of other measures are counted and not imported (`FinishDirectCodes`); `ResolveDirectCodes`, and so
  `GetDirectCode(ROLE_CMS68_MEDS_DOCUMENTED)` and `GetDirectCode(ROLE_CMS69_BMI)`, are unchanged. `PickListDbTests` imports through the new
  readers (the record's headers, the Expansion Version, the code-system versions) and passes.

**Fix 1: an "other" not-performed row can be deleted** (`d4a775a92`). `FormEhrNotPerformedEdit.FillReadOnly` enables Delete
(`butDelete.Enabled=true`; it disabled it); Save, the reason list, the date and the note stay read-only. `butDelete_Click` reads no item
index and no pick list (it unlinks a vital sign that names the row and deletes the row), so it works before and after the import. Section
7's bullet is superseded.

**Fix 2: a patient without a birth date is an adult in the vital sign window** (`d4a775a92`). New `Vitalsigns.GetAgeForVitalSignWindow`
(birth date, day taken): `AGE_ADULT` (18) when the birth date is before 1880 (none), else `GetAgeOnDateTaken`. `FormVitalsignEdit2014`
takes `AgeOnDateTaken` from it on load and when the date changes, so such a patient gets no BMI percentile, the adult interventions and the
over- or underweight class, as before EHR-7 (whose age "before January 1" of a missing birth date was about 2,000 years); the range is the
2027 one at every adult age (E-35). A day taken before a known birth date stays -1 (not classed; the window refuses to save it anyway).
`GetAgeOnDateTaken` still returns -1 without a birth date, and the eForm import keeps its own rule (no percentile and no weight code without
a birth date, as before EHR-7). Section 6's deviation is superseded.

**Tests** (`2a84f82a7`):

- `PickListTests.NotPerformed_AnOtherRow_KeepsDelete_TheOfficesOwnData` (new): `FillReadOnly` enables Delete and keeps Save disabled,
  nothing in the window's code disables Delete, the read-only load returns right after `FillReadOnly`, and `butDelete_Click` reads no item,
  `IsReadOnly`, reason list or combo. `NotPerformedGrid_...` no longer expects Delete disabled.
- `PickListDbTests.NotPerformed_AnOtherRow_IsDeleted_BeforeAndAfterTheImport` (new): an "other" row (a Tobacco User code) is deleted
  before the import and after it.
- `VitalSignAgeTests.VitalSignWindow_APatientWithoutABirthDate_IsAnAdult` (new): no birth date (`DateTime.MinValue`, 1800, 1879-12-31)
  gives 18: no percentile, not the child group, the class (27 over, 18 under, 22 normal); `GetAgeOnDateTaken` still -1; a known birth date
  gives the age on the day taken (around the 3rd and 18th birthdays, the 17th and 65th), a day before the birth -1; the eForm import
  without a birth date sets no percentile and no weight code. `VitalSignWindow_TakesTheAgeFromTheBusinessLayersHelper` finds the new
  helper twice and no `GetAgeOnDateTaken` call in the window.

Each new test fails without its fix: the Delete test on `butDelete.Enabled=false`, the vital-sign tests on the missing helper and on the
window's old call.

**Runs.** Full build through the x86 lane: 0 errors, no warning in a changed file. Targeted run with `-RequireBuildOutput`,
`ehr7r-1-run1.txt` (after the full build of the fixes):

| Assembly | Result |
|---|---|
| EhrTests | 330: 328 passed, 2 skipped (`Catalog_EqualsTheDownloadedPackages_WhenCqmSpecsDirIsSet` and `RealRelease_TheOfficesThreeFiles_ImportSevenOfSeven_WhenTheFolderIsSet`, inconclusive without their folders, as on `main`), 0 failed; `LoadTime_AFullSyntheticRelease_LoadsInUnderASecond` passed in the parallel run |
| AttributionTests | 50 of 50 passed |
| ProjectFileTests | 151: 150 passed, 1 skipped (as on `main`) |
| NoHqTests | 112 of 112 passed |

EhrTests' database tests use their support classes' fixed prefixes (`ehr1_`, `ehr2_`, `ehr4a_`, `ehr6a_`, `ehr7_`); no scratch database
was left on the server. InitSqlTests and ForkMigrationTests were left out: nothing here touches the schema or `db/init/new-database.sql`.

**For the lead.** `ehr/ehr7r-1` merges into `main` as it is (EHR-6a is already there). With it, `docs/port-notes/S3a-M3.md`'s "For EHR-7's
later merge" is done: FormMedical holds both packages' lines.
