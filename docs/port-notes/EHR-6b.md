# Port note: EHR-6b (the 2014 and 2011 quality-measure code deleted)

Package EHR-6b of `docs/specs/ehr-2027-measures-plan.md` (section 2.1, section 3, the EHR-6b row of section 1 and its package text in
section 12; section 10.1 for the fixture guard), on branch `ehr/ehr6b-1` from `main` `e9c46c233`. It carries out the owner's decisions 2
and 4 of 2026-11-14: the 2014 versions and the 2011 engine, its window and its PQRI XML export are deleted on purpose, with everything only
they use, their tests and their project-file entries. They are deliberate deletions, so the standing rule (keep and port everything) does
not apply to them; it applies to everything else, and nothing else was cut. No table, column or saved row is touched: no schema change, no
fork step, no `PrefName`, no third-party file, no licensed content. The translation rows of the deleted forms stay in the database, unread.

**The lead's changes to the package** (2026-11-14), all applied:

- (a) Branched from `main`, not from EHR-6a, so EHR-6b merges before stage 3a wave 1 without waiting for E-20. EHR-6a's branch
  (`ehr/ehr6a-5`) was read, never edited; section 7 says what its merge after this one needs.
- (b) This note is `docs/port-notes/EHR-6b.md` (EHR-6a writes `EHR-6.md` on its branch; the lead merges the two later).
- (c) `tests/EhrTests/ValueSetSourceTests.cs` loses only what the deletion needs (section 5); the rest of the file is as on `main`.
- (d) The release-note line is one added line in `docs/release-notes/next.md`'s EHR section (section 6).

The deletion's history: `main` before this branch (`e9c46c233`, or the plan's `5737519bf`) keeps all of it readable, for example
`git show e9c46c233:"OpenPmsDentalBusiness/Db Multi Table/QualityMeasures.cs"`. EHR-5b and EHR-5c may read its XML helpers and checks
there (section 3 of the plan).

## 1. What was deleted

| File | Lines | What it held |
|---|---|---|
| `OpenPmsDentalBusiness/Db Multi Table/QualityMeasures.cs` | 9,100 | Both engines: the 2011 one (`GetAll`, `GetId`, `GetPQRIMeasureNumber`, `GetDescript`, `GetTable`, `CalcNumerator`, `CalcExclusions`, the explanations) and the 2014 one (`GetAll2014`, `GetId2014`, `GetDescript2014`, `GetEhrCqmData`, the data queries, `ClassifyPatients`, the counts, the explanations, the eMeasure identifiers `GetEMeasureNum`, `GetEMeasureVersion` and the rest, and the QRDA I and III writers `GenerateQRDA` with their checks) |
| `OpenPmsDentalBusiness/Db Multi Table/QualityMeasure.cs` | 319 | `QualityMeasure`, `QualityType`, `QualityType2014`, `CqmItemAbbreviation`, `EhrCqmPatient`, `EhrCqmEncounter`, `EhrCqmIntervention`, `EhrCqmProblem`, `EhrCqmMedicationPat`, `EhrCqmNotPerf`, `EhrCqmMeasEvent`, `EhrCqmProc`, `EhrCqmVitalsign` |
| `OpenPmsDental/Forms/FormEhrQualityMeasures2014.cs`, `.Designer.cs`, `.resx` | | The 2014 window ("Quality Meas"), with its QRDA export |
| `OpenPmsDental/Forms/FormEhrQualityMeasureEdit2014.cs`, `.Designer.cs`, `.resx` | | Its measure detail |
| `OpenPmsDental/Forms/FormEhrQualityMeasures.cs`, `.Designer.cs`, `.resx` | | The 2011 window (no button since Open Dental), with the PQRI XML show and submit (`GeneratePQRS_xml`) |
| `OpenPmsDental/Forms/FormEhrQualityMeasureEdit.cs`, `.Designer.cs`, `.resx` | | Its measure detail |

`OpenPmsDental/Forms/FormEHR.cs`:

- `butQuality_Click` (the dead 2011 handler: no button in the designer, as in Open Dental) deleted.
- `but2014CQM_Click` ("Quality Meas") now shows `MsgBox.Show(this,"The 2027 quality measures are not in this build yet.")` until EHR-5a
  opens the 2027 window. The button, its text and its click binding in `FormEHR.Designer.cs` are unchanged.
- `ProvKeyIsValid` deleted: its only callers were the two deleted windows (`FormEhrQualityMeasures.cs:38`, `FormEhrQualityMeasures2014.cs:43`).

The two kept parts of the 2014 window that are not measure logic, the legal-authenticator pick (`FormEhrQualityMeasures2014.cs:149-165`) and
the OID registry fallback (`:193-197`, which opens `FormOIDRegistryInternal`; that window stays, opened from `FormEhrSetup`), are EHR-5b's
to rebuild in the 2027 export (plan section 3); they are readable at `e9c46c233`.

## 2. The removed project items (for the lead's re-record of `tools/project-baseline`)

`OpenPmsDental/OpenPmsDental.csproj` lost exactly eight elements (24 lines: 1043-1054 and 3906-3917 at `e9c46c233`), nothing else:

| Item | Include | Metadata |
|---|---|---|
| `Compile Update` | `Forms\FormEhrQualityMeasureEdit.Designer.cs` | `DependentUpon=FormEhrQualityMeasureEdit.cs` |
| `Compile Update` | `Forms\FormEhrQualityMeasureEdit2014.Designer.cs` | `DependentUpon=FormEhrQualityMeasureEdit2014.cs` |
| `Compile Update` | `Forms\FormEhrQualityMeasures.Designer.cs` | `DependentUpon=FormEhrQualityMeasures.cs` |
| `Compile Update` | `Forms\FormEhrQualityMeasures2014.Designer.cs` | `DependentUpon=FormEhrQualityMeasures2014.cs` |
| `EmbeddedResource Update` | `Forms\FormEhrQualityMeasureEdit.resx` | `DependentUpon=FormEhrQualityMeasureEdit.cs` |
| `EmbeddedResource Update` | `Forms\FormEhrQualityMeasureEdit2014.resx` | `DependentUpon=FormEhrQualityMeasureEdit2014.cs` |
| `EmbeddedResource Update` | `Forms\FormEhrQualityMeasures.resx` | `DependentUpon=FormEhrQualityMeasures.cs` |
| `EmbeddedResource Update` | `Forms\FormEhrQualityMeasures2014.resx` | `DependentUpon=FormEhrQualityMeasures2014.cs` |

The committed record (`tools/project-baseline`, not edited here) then holds 18 lines that the build no longer evaluates. The lead's
re-record drops them, and only them (line numbers of the record at `e9c46c233`; the plan's 4527-4528, 6589-6596, 8421-8424 are from an
older record):

- `items.txt:5307-5308`, the two business files (the business project globs; the record lists its evaluated items):
  `OpenPmsDentalBusiness|Compile|Db Multi Table\QualityMeasure.cs|` and `...\QualityMeasures.cs|`.
- `items.txt:7403-7410`, the eight Compile items of the four forms (each form's `.cs`, from the glob, and its `.Designer.cs` with
  `DependentUpon`).
- `items.txt:9237-9240`, the four EmbeddedResource items (`.resx` with `DependentUpon`).
- `resources.txt:285-288`: `OpenPmsDental.FormEhrQualityMeasureEdit.resources`, `...FormEhrQualityMeasureEdit2014.resources`,
  `...FormEhrQualityMeasures.resources`, `...FormEhrQualityMeasures2014.resources`.

No other file of the record (`files.txt`, `configs.txt`, `config-texts.txt`) names them. `ProjectBaselineTests` is opt-in
(`OPMS_CHECK_PROJECT_BASELINE=1`) and was Inconclusive in this package's run, as on `main`; the lead's re-record and Release dry run (plan
section 10.4) check it. The release's staged file list should not change: the deleted forms were compiled into the program, never shipped
as files.

## 3. The grep of `main` for the deleted names (the package's Work)

Every type, method and form name of the fourteen files (the thirteen types of `QualityMeasure.cs`, `QualityMeasures` and its public
methods, the four forms with their public members, `GeneratePQRS_xml`, `ProvKeyIsValid`, `butQuality_Click`, and the reflected
`GetEMeasureNum` and `GetEMeasureVersion`) was grepped in every tracked file of `main` at `e9c46c233` except the fourteen files, `docs/` and
comments. The hits are exactly section 2.1's outside callers:

| Hit | Fate |
|---|---|
| `FormEHR.cs:547-551` (`butQuality_Click`), `:553-564` ("Quality Meas"), `:624-628` (`ProvKeyIsValid`) | Section 1 |
| `OpenPmsDental.csproj`, the eight items | Section 2 |
| `tests/EhrTests/ProvKeyRuleTests.cs:336-341`, `ValueSetSourceTests.cs:2`, `26-32`, `192` | Section 5 |
| `tools/project-baseline/items.txt`, `resources.txt` | Section 2 (the lead's re-record) |
| `EhrMeasures.cs:223`, `245`, `267` `CalcNumerator(` | Not a caller: `EhrMeasures.CalcNumerator` (`EhrMeasures.cs:1584`), its own method of the same name |

So the grep finds no caller beyond section 2.1, and the package is not split: there is no EHR-6c.

**What the deleted code called, and whether anything else still calls it.** Every static method the deleted files called was counted
outside them (code, comments left out). All have other callers and stay, among them the four section 2.1 names:
`EmailMessages.SendTestUnsecure` (`FormEhrEncryption.cs:159`, `FormEhrHash.cs:27`, `FormEhrLabPanels.cs:97`, `FormEhrVaccines.cs:130`),
`MsgBoxCopyPaste` (CodeBase's and the program's, many callers), `EhrMeasures.DropOldWorkTables` with its `tempehrquality` pattern
(`FormEhrMeasureCalc.cs:188`; it still clears a work table a stopped 2011 or 2014 run left) and `ODZip.GetDefaultEncodingBytes` (`Ramq.cs:175`,
and `ODZip.CreateArchiveFile` with it). Two had no caller outside the deleted code in the count, and both are **kept**:

- `EhrCodes.GetCodeAndCodeSystem` (`EhrCodes.cs:259`; its only caller was the 2014 engine's tobacco query, `QualityMeasures.cs:3294`). Kept:
  it is a general lookup of the imported value sets (codes and code systems of a list of OIDs), not 2014 logic, `EhrCodes.cs` is EHR-6a's
  file (plan section 13.3), and EHR-6a's branch keeps it (`EhrCodes.cs:286` there). The 2027 engine (EHR-8a1) may use it. `RemovalTests`
  holds it in place.
- `PayorTypes.GetCurrentType` (`PayorTypes.cs:32`). Kept: `PayorTypes.cs:24` calls it too, so the count above missed nothing; it only
  looked like an orphan through the `PayorTypes.` prefix.

`FormOIDRegistryInternal`, which the 2014 window opened when QRDA failed, stays (`FormEhrSetup.cs`, and `OIDInternalsTests`).

## 4. Stale comments left for their files' owners

Comments name the deleted code in files this package does not own; none is code, and the tests leave comments out:

- `OpenPmsDentalBusiness/Data Interface/EhrMeasures.cs:7247`: `DropOldWorkTables` is "Called ... by the quality measure windows (EHR-5)".
  Those windows are deleted; the 2027 window (EHR-5a) may call it again. For that file's next owner (the plan names this line). Also
  `:7242` names "tempehrquality..., QualityMeasures" as the engine of those work tables: still true of the tables a stopped old run left,
  so only the tense is stale.
- `OpenPmsDentalBusiness/Data Interface/EhrCodes.cs:538`, `570`, `611`, `614` name `QualityMeasures`, `GetEMeasureNum` and `GetEhrCqmData` as
  the source of the June 2013 manifest. EHR-6a rewrites those lines (its branch names none of them).

## 5. Tests

**New `tests/EhrTests/RemovalTests.cs`** (no database; six tests):

- `Source_NamesNoneOfTheDeletedCode_CommentsLeftOut`: no file of the program's source (`TestPaths.ProgramSourceFiles`, the test projects
  left out) names `QualityMeasures`, `QualityMeasure`, `QualityType`, `QualityType2014`, `GetAll2014`, `GetEhrCqmData`, `GenerateQRDA`,
  `GetPQRIMeasureNumber`, `GeneratePQRS_xml`, `EhrCqm`, `FormEhrQualityMeasures`, `FormEhrQualityMeasureEdit` or `ProvKeyIsValid` outside
  comments. A name matches at a word start (not after a letter, digit or underscore), so the longer names that begin with one are covered too
  (`EhrCqmPatient`, `QualityMeasureDocument`), while the 2027 names that only hold one inside theirs (`FormEhrCqm`, `FormEhrCqmDetail` of
  EHR-5a, which EHR-5b and EHR-5c edit) do not trip it; the test checks a sample of both kinds.
- `Files_TheTwelveFormFilesAndTheTwoBusinessFiles_AreGone`: none of the fourteen files is on disk.
- `ProjectFiles_NameNoneOfTheFourForms_AndKeepTheirNeighbours`: `OpenPmsDental.csproj` (comments left out) names none of the four forms, and
  the items next to the removed ones (`FormEhrProviderKeys`, `FormEhrReminders`) stay; the business project names neither file.
- `Assemblies_HoldNoneOfTheDeletedTypesOrMethods`: neither built assembly holds a deleted type (by name, and no `EhrCqm*`, `QualityMeasure*`,
  `QualityType*` or `FormEhrQualityMeasure*` type), `FormEHR` has no `ProvKeyIsValid` or `butQuality_Click` and keeps `but2014CQM_Click`,
  and the program holds no `FormEhrQualityMeasure*` resource.
- `QualityMeas_ShowsTheMessage_UntilTheWindowOfEhr5a`: the handler shows the message and opens no window; the designer's button and binding
  are unchanged; the Measure Calc window's `PatientSelect` stays.
- `KeptHelpers_Stay_WithTheirOtherCallers`: section 3's kept methods exist and their other callers still call them, the work-table rule still
  takes a `tempehrquality` table, and the tables the measures read keep their types. That `DropOldWorkTables` still drops an old
  `tempehrquality` table on the scratch database is `MeasureCalcTests.WorkTables_OnlyOldOnes_OfThisDatabase_AreDropped`, unchanged.

**`ProvKeyRuleTests.cs`**: `TheEhrWindow_LosesOnlyTheTwoDeliberateRemovals` is now `TheEhrWindow_LosesOnlyTheDeliberateRemovals`; its last
part (336-341) checks that `FormEHR.cs` names neither `ProvKeyIsValid` nor `butQuality_Click` and that no program source file names
`ProvKeyIsValid`, as it checks `QuarterlyKeyIsValid`. The header comment names the new removals.

**`ValueSetSourceTests.cs`**, only what the deletion needs (lead change c; there is no OID allowlist on `main`, that is EHR-6a's):

- The header comment (lines 1-3) no longer says the nine measures are read from the engine by reflection.
- `Manifest_TheNineMeasures_AreTheVersionsTheEngineReports` is now `Manifest_TheNineMeasures_AreThe2014VersionsTheDeletedEngineReported`:
  the reflection of `QualityType2014`, `GetEMeasureNum` and `GetEMeasureVersion` (26-32 and the comparison with it) is gone; the literal
  list of the nine 2014 IDs, which `EhrValueSetImport.ArrayMeasureIds` still holds until EHR-6a, stays.
- `Manifest_ItsOids_AreExactlyTheValueSetOidsTheSourceNames` is now `Manifest_ItsOids_AreTheValueSetOidsTheSourceNames_AndTheDeletedEnginesOwn`.
  Every value-set OID the source names is still in the manifest (unchanged), and the manifest still holds 79 (unchanged). What changed: the
  manifest OIDs the source no longer names must be exactly **53** (`COUNT_OIDS_ONLY_THE_DELETED_ENGINE_NAMED`), the value sets that only the
  deleted 2014 engine named (the PHDSC Payer value set `2.16.840.1.114222.4.11.3591` among them). The run listed them: 14 under
  `2.16.840.1.113883.3.464.1003.101.12`, one under `.104.12`, four under `.109.12`, three under `.110.12`, three under `.121.12`, three under
  `.125.12`, 21 under `2.16.840.1.113883.3.526.3`, three under `2.16.840.1.113883.3.600.1`, and the Payer set. The other 26 of the 79 are
  still named by the data-entry windows and `DiseaseDefs`.
- The fixture guard `Fixtures_HoldNoLiteralOfACodeFromTheEnginesLists` reads `EhrMeasures.cs` only (its path list at 192 loses
  `QualityMeasures.cs`), and its floor is lowered from `Count>50` to `Count>=21` (`COUNT_ENGINE_CODES_FLOOR`): **21** codes is what
  `EhrMeasures.cs` alone yields by the guard's pattern in this package's run (the plan estimated about 20). The CDT check (a code starting
  with `D` among them) stays and passes. Its summary comment no longer names the RxNorm and CPT lists of `QualityMeasures`.

**Runs** (Targeted, after the full lane build of the branch): EhrTests, NoHqTests, ProjectFileTests, AttributionTests, MiddleTierMockTests;
see the package's report for the counts. MiddleTierMockTests pass with the removed methods (no Middle Tier test named them).
InitSqlTests and ForkMigrationTests were left out, as the lead asked; nothing here touches the schema or `db/init/new-database.sql`.

## 6. Release note

One line added at the end of the EHR section of `docs/release-notes/next.md` ("EHR: the Meaningful Use measures window (Measure Calc)", the
section of the EHR window's buttons), placed there so that it does not touch the lines EHR-6a rewrites in "EHR: the value sets from your own
VSAC download" (26-28, 492-517). It says what section 3 of the plan says: the 2014 versions and their QRDA export, and the 2011 window (no
button since Open Dental) with its PQRI XML export, removed on the owner's decisions of 2026-11-14, the 2027 versions replacing them, the
"Quality Meas" button saying so until then, and no saved data changed. The top summary's line about "the 2014 quality measures have their
codes again" (next.md:26-28) is EHR-6a's to rewrite (plan section 3); if a release is cut before EHR-6a merges, the lead fixes it first.

The plan's status paragraph (`docs/specs/ehr-2027-measures-plan.md:32-34`) has a sentence for EHR-6b on two new lines after it; EHR-6a's
branch appends its own sentence there, so the merge joins the two.

## 7. Merging EHR-6a after this package

EHR-6a's branch builds on the engine's absence: nothing in it calls the deleted code (its catalog, importer and tests name no deleted type;
`ehr/ehr6a-5` was grepped). Its merge after this one needs, in files both touch:

- `tests/EhrTests/ValueSetSourceTests.cs`: take EHR-6a's version of the manifest region (its allowlist replaces this package's count, so
  `COUNT_OIDS_ONLY_THE_DELETED_ENGINE_NAMED` goes), with `OpenPmsDentalBusiness\Db Multi Table\QualityMeasures.cs` taken off its
  `_arrayOidAllowlist` (the file is gone, and its stale-entry check would fail). The fixture guard keeps this package's single path and
  floor of 21 with EHR-6a's GUID skip; `EhrMeasures.cs` holds no GUID literal, so the count stays 21 (a run confirms it).
- `tests/EhrTests/ProvKeyRuleTests.cs`: EHR-6a does not touch it; this package's version stands.
- `docs/specs/ehr-2027-measures-plan.md`: both status sentences stay.
- `docs/release-notes/next.md`: no overlap (section 6).
- `OpenPmsDental/Forms/FormEHR.cs`, `OpenPmsDental.csproj`: EHR-6a does not touch them.

## 8. Owner checks (plan, EHR-6b)

With a test database and EHR turned on: the EHR window's "Quality Meas" gives the message; the other EHR buttons open as before. The release
dry run's staged file list is unchanged.
