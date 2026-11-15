# Port note: EHR-6 (the 2027 catalog and import; the 2014 and 2011 code deleted)

Packages EHR-6a and EHR-6b of `docs/specs/ehr-2027-measures-plan.md` (below: "the plan"), section 12. This note has the EHR-6a part; EHR-6b
adds its own part below it.

## EHR-6a The 2027 catalog and import

On branch `ehr/ehr6a-1` from `main` `b8e931cea` (S4-1, MSTest 4, merged). The owner decided the plan on 2026-11-14 (all of section 14 as
recommended). No schema change, no new `PrefName`, no project file edited (the business project globs the new `Db Multi Table/Cqm` folder),
no package or third-party file added, no download, no code of any code system anywhere in the repository. Synthetic data only.

### 0. Merge gate: E-20's record (not done here)

**This package merges only after E-20's record exists** (the plan, sections 7.1 and 12, E-20): someone with a UMLS licence opens the three
files of the 2026-05-14 release once and records here what the readers need. That needs a UMLS sign-in, which this package does not have and
may not use. So the readers are built from the plan's section 7 and VSAC's public descriptions, tested with synthetic `TEST` files only, and
**every header name below is provisional**.

| To record from the real files | What the readers do now (provisional) | If the record differs |
|---|---|---|
| The three files' names and formats (`.xlsx`, `.xml`, `.csv`, one file or a `.zip`) | The value sets: `.xlsx`, SVS `.xml` or a `.zip` of them (EHR-2's readers). The Direct Reference Codes and Code System Versions files: `.xlsx` (or a `.zip` holding one). A `.csv` is not read | A CSV reader for the two companions |
| Whether VSAC gives the three as separate files (the plan's reading) or one `.zip` of all three | Three files, picked together. One `.zip` holding the three is read as one file whose parts are of three kinds, and refused ("more than one of the release's three files") | `MakePick` takes the parts of one `.zip` as the three |
| Each file's sheets, the rows above the header, the header names exactly | The kind of each sheet is its header row (`EhrValueSetImport.GetSheetKind`): a Value Set OID (or a value-set name or QDM category beside a code) is the value sets; a Code without a value set is the Direct Reference Codes file; a code system and a version without a code is the Code System Versions file. Headers compared without white space and case: CMS ID (also CMS IDs, Measure ID(s), eMeasure ID, Measure); Value Set Name; Value Set OID (also OID); QDM Category (also Category); Code (also Code Value, Direct Reference Code); Description (also Descriptor, Code Description, Display Name, Code Display Name, Display, Code Name); Code System (also Code System Name); Code System OID; Code System Version; Expansion Version (also Definition Version, Value Set Version, Version: the value set's version, or in the Code System Versions file the code system's) | `_dictHeaders` and `GetSheetKind` in `EhrCodes.cs`; the fixtures' `ArrayHeadersDefault`, `ArrayHeadersDirectCodes`, `ArrayHeadersCodeSystemVersions` |
| How the Direct Reference Codes file names a measure (CMS68v16, CMS68 or another form) and the display of each code | One row per measure, code system and code; the measure as `ParseMeasureIds` reads it (CMS68v16 or 68v16); the role's LOINC display name compared with the Description column, without case and with white space collapsed | `ResolveDirectCodes` |
| The version string of each code system in the Code System Versions file, and the program's own `codesystem.VersionCur` values | The mapping of section 4 below | `GetComparableVersion` |
| The sizes of each file and each workbook sheet uncompressed | EHR-2's limits unchanged (64 MB per file and part, 256 MB in all, 1,000,000 rows); every value set of the file is still imported | E-30: past 64 MB a sheet, keep only the manifest's value sets |
| The expansion profile or version the files name | Reported ("Value-set versions in the file"); the release label is `CqmCatalog.RELEASE_LABEL` | The label may add it |

Nothing from the files enters the repository. EHR-2's own step 5 (the June 2013 record) is no longer needed (EHR-2's owner question 1 is
superseded by E-20).

### 1. The catalog: `OpenPmsDentalBusiness/Db Multi Table/Cqm/CqmCatalog.cs`

One file for all seven measures (decision 4, owner decision 5), in the namespace `OpenPmsDentalBusiness` (so the Middle Tier finds a later
`Meth` class of the folder, section 5.2), with its data classes at the end (`CqmMeasure`, `CqmPopulationGroup`, `CqmPopulation`,
`CqmStratum`, `CqmValueSet`, `CqmDirectCodeRole`; public fields, XML-serialisable). The file is plain ASCII: the statements' typographic
characters (curly quotes, a narrow no-break space) are `\u` escapes. Every public method is `Meth.NoCheckMiddleTierRole` (data only).

- **The release.** `RELEASE_LABEL` "VSAC eCQM Value Sets for Eligible Clinicians, 2027 performance period, published 2026-05-14",
  `RELEASE_DATE`, `PERFORMANCE_YEAR` 2027. `EhrValueSetImport.RELEASE_LABEL` is the catalog's.
- **Per measure** (section 4.1): CMS ID and version (`MeasureId` CMS74v16, `MeasureIdShort` 74v16), the CQL library, title, steward (the
  HQMF custodian) and developer (the author: AIR for the CMS four, NCQA for its three), set ID, version-specific ID, episode-based (CMS68),
  scoring (Proportion), measure type, improvement notation (CMS75 lower is better), the population groups with the HQMF ID of each section,
  population (code IPOP, DENOM, DENEX, NUMER, DENEXCEP; ID; CQL define) and stratum (ID, define, the HQMF's description), the value sets
  and the direct-code roles of the measure, and the copyright and disclaimer statements.
- **The statements** (section 11.1 for NCQA's, E-38 for CMS's): copied verbatim from each HQMF file's `measureAttribute` entries "Copyright"
  and "Disclaimer": the attribute value with its XML escaping undone, so HTML paragraphs (`<p>`, with `&amp;` and `&nbsp;` as the HTML holds
  them) separated by line feeds, no edit. `CqmCatalog.GetStatementText` gives one as plain text for a window or print-out that cannot show
  HTML (EHR-5a); the statement itself is never changed. Identical statements share a constant (CMS74 and CMS75's disclaimer; CMS138, CMS155
  and CMS165's disclaimer; CMS155 and CMS165's copyright), each measure still holds its own.
- **The value sets** (section 7.2): the `valueset` lines of each package's `x/cql` files outside `CQMCommonQDM-9.0.000.cql`: **68 distinct**.
  Each with its OID, its name as the CQL gives it, its QDM category and the QDM datatypes and attributes the CQL reads it as (from the CQL's
  retrieves `["Datatype": "Value Set"]` and `.negationRationale`/`.result`/`.reason in "Value Set"`; a value set read only as an attribute
  has the category Attribute), and the measures that declare it (the measure's own library or one it includes; "Encounter Inpatient" is
  there for the six measures that include HospiceQDM, which declares it, and for CMS165, which declares it itself). **Declared but unused**:
  the four value sets `CQMCommonQDM` declares for its hospital functions, which no measure calls ("Intensive Care Unit"
  `2.16.840.1.113762.1.4.1029.206`, "Outpatient Surgery Service" `2.16.840.1.113762.1.4.1110.38`, "Observation Services"
  `2.16.840.1.113762.1.4.1111.143`, "Emergency Department Visit" `2.16.840.1.113883.3.117.1.7.1.292`), held apart
  (`GetValueSetsDeclaredButUnused`) and not in the manifest; with "Encounter Inpatient", the fifth, the CQL files hold 72 in all.
- **The pick-list value sets** (section 8): 17, OID and use, each also a value set of a measure: the BMI follow-up, referral and medication
  sets, the two counseling sets, the tobacco screening, user, non-user, counseling and pharmacotherapy sets, "Dialysis Services", "Pregnancy"
  and the not-performed reasons "Acute Health Crisis", "Medical Reason" and "Patient Declined". The windows keep their 2014 OIDs until EHR-7.
- **The direct-code roles** (section 7.3): eight, by measure, code system and meaning, never a code: `Cms68MedsDocumented` (CMS68, SNOMEDCT),
  `Cms69Bmi` (CMS69, LOINC, "Body mass index (BMI) [Ratio]"), `Cms69Virtual` (CMS69, ActCode), `Cms138QualifyingVisitCpt` (CMS138, CPT, four
  codes), `Cms138CounselingDx` (CMS138, ICD10CM), `Cms165Systolic` (CMS165, LOINC, "Systolic blood pressure"), `Cms165Diastolic` (CMS165,
  LOINC, "Diastolic blood pressure"), `Cms165KidneyTransplantDx` (CMS165, ICD10CM). The three LOINC display names (E-23(a)) are the only
  names of codes in the program's catalog; LOINC's notice is in `THIRD-PARTY-NOTICES.txt` (section 6).

**The record** (the downloads of 2026-11-14 in the lead's download folder, outside the repository; one folder per CMS ID, `x/` the unzipped
package). `CatalogTests` holds the catalog to it: the per-measure counts and list hashes, and the statement hashes.

| Measure | HQMF file, SHA-256 | Value sets (CQL outside CQMCommonQDM) | SHA-256 of their OIDs (sorted by number, joined by line feeds, ASCII) |
|---|---|---|---|
| CMS68v16 | `CMS68-v16.0.000-QDM.xml` `2a7e2a0a8ba0160c5d09d167da1e047feb1c3a9f5d62e7cda9d522e0b2da146b` | 6 | `3f09743df7b69257e1df560be8295a5627df24ccccfead18181da5d05d415a66` |
| CMS69v15 | `CMS69-v15.0.000-QDM.xml` `a97dcf94da7a7063bf36a1dfc6838aa150690f62615cfaaefc37213264f2155a` | 22 | `2d80774475ab8681c861bd9a9dfda6319e3f67957a2482d6e1536acec98293a4` |
| CMS74v16 | `CMS74-v16.0.000-QDM.xml` `d75a9f8a8ba238684f523c26c7049dde32b46e3239ab121fe5b5087ee2824f97` | 10 | `d44309819b069ecd5383cc5410678de04e6abe5ac524a645ca5f84bbd0f41509` |
| CMS75v15 | `CMS75-v15.0.000-QDM.xml` `cc16d9a56e4917a5df87e4977e3972c12e9a49989fe771167980a2df3edb44fa` | 10 | `202eb614d7ec7dc0f482b8a745b4c945c68a5903ac0d694e734da7938d65542f` |
| CMS138v15 | `CMS138-v15.0.000-QDM.xml` `7670c83024eaf310e11a6198083dbd469cf58d4fac9ddc6d0296ab3723da6c23` | 32 | `1a4388142e3ba267d9fb8c6e6aa5638712f247dcc22ebc7358782acf1be178b0` |
| CMS155v15 | `CMS155-v15.0.000-QDM.xml` `1e44f75140645a22671cd246eddbb028503eae5b174172cc4df7a60be2f0fefb` | 21 | `6b70976fba8f215bceea0dbf45bfb4d0c29fab0300d052c0fb7a986ea7a9f113` |
| CMS165v15 | `CMS165-v15.0.000-QDM.xml` `320d94a90a7e71196528649b975ae90c577f1143e0f5d6f3cd70bc67a7e7a6a8` | 31 | `f13008198a5e26c47532c9d453bc71c3862d814b3bd2f1a2e6e6085e924e1fc9` |

The CQL files read (SHA-256): `CMS68DocumentationCurrentMeds-16.0.000.cql` `2643187f9111782c549e0b95612cf6fa96c8029a5dbc6238bfcb0a2891e5aa98`,
`CMS69BMIScreenAndFollowUp-15.0.000.cql` `7dce6f49088fc6093b75c993a563350082abbef6992a8dc2cc07e94c3e562322`,
`CMS74DentalCariesPrevention-16.0.000.cql` `58f272bd5527d946f02062c0ee97cd67a2e63b878ab925080bc527d88e84028a`,
`CMS75ChildrenDentalDecay-15.0.000.cql` `361ea8a32589482b74d207424ff8bdb6a469915513d199c4f4e2a3ed26c570de`,
`CMS138PreventiveTobaccoCessation-15.0.000.cql` `b75cf876920ba7b675e16593a5053ddc5fa95d11e5bbdc53ece1eaf937fe26db`,
`CMS155WeightAssessandCounselforKids-15.0.000.cql` `7a9ef204145e5c8dda001dab786579b3a14b528bced79d87cf5252049fcdef5d`,
`CMS165ControllingHighBloodPressure-15.0.000.cql` `0bf2b7335bba80b97a397db6801c4c3b6922da5741048dd26919578d3c941c47`,
`CQMCommonQDM-9.0.000.cql` `7b78f301d298fa86f976882e08e871196d55fd1a60347b7fd3467bce3ebd31bf` (the same in all seven),
`HospiceQDM-7.0.000.cql` `19fd725f536e35dafac6c4b0a95505dca377dcaf68cad3a4879c3173093bc6c6`,
`PalliativeCareQDM-6.0.000.cql` `9d95946ebc053c77ab4f0f17a21534ec27842c4aa4ce2084f9308becd9813059`,
`AdultOutpatientEncountersQDM-4.0.000.cql` `491a5202621e08ba8299aa5ce30881d2dd35b9a43e446ee31ea3204d750a5c45`,
`AdvancedIllnessandFrailtyQDM-11.0.000.cql` `439ff0afa6e64bc2e9a554281e80f0d1994dfa375a930e628ab8a60105ec50ad`.

| Measure | Copyright statement, SHA-256 (UTF-8) | Disclaimer statement, SHA-256 (UTF-8) |
|---|---|---|
| CMS68 | `7bbfc6f482b64ad00b0eb84751e4918b361bae70aef41b87719246a9935a9c52` | `84cd5619877e6b07e917f613f3454e63bcb97dfecf2ae9a6f13794616f6af1d8` |
| CMS69 | `41ab2c79ed3c19069415b86a5516ecdd491e806436dd1e2449e3b3a0a1afe62f` | `baccfc321e12c54b01fa8459b18d330c8c116aa6c42708d4bbe2cf810a3eb9cc` |
| CMS74 | `07a6edc274b6757f2823f02b9fca743c98f275445d033beefae2ba20b3aa4930` | `4d82961204383d1b691dcd8037a9fae17dcb8037787ae685c0026de092ded2a5` |
| CMS75 | `fb472279d6d0d16c9abed7f32275a4e7594de2cf8d784aadaaca0bf2eb41ea15` | `4d82961204383d1b691dcd8037a9fae17dcb8037787ae685c0026de092ded2a5` |
| CMS138 (NCQA) | `a2468c0577e7c5ff6b3f6366f982849f7f760e69fe59f4b584ffe17d99717914` | `32f0807b74dfc688f332af48c2f9fb6501b926a384e40486ac75ce2e9b675806` |
| CMS155 (NCQA) | `835263c6306f513098681f83bb18b52ebd6e0069325fcc77e208be877869c88b` | `32f0807b74dfc688f332af48c2f9fb6501b926a384e40486ac75ce2e9b675806` |
| CMS165 (NCQA) | `835263c6306f513098681f83bb18b52ebd6e0069325fcc77e208be877869c88b` | `32f0807b74dfc688f332af48c2f9fb6501b926a384e40486ac75ce2e9b675806` |

The three NCQA measures are CMS138v15 "Preventive Care and Screening: Tobacco Use: Screening and Cessation Intervention", CMS155v15 "Weight
Assessment and Counseling for Nutrition and Physical Activity for Children/Adolescents" and CMS165v15 "Controlling High Blood Pressure"; their
statements are kept with them in the catalog and are shown with them wherever their results are shown, printed or written to QRDA (EHR-5a,
EHR-5b, EHR-5c1; section 11.1). This note carries the statements' sources and hashes, not their text.

`CatalogTests.Catalog_EqualsTheDownloadedPackages_WhenCqmSpecsDirIsSet` reads the packages again when `CQM_SPECS_DIR` names the download
folder (one folder per CMS ID with its `x/` folder) and compares every fact: IDs, titles, steward and developer, both statements verbatim,
the improvement notation, each population and stratum with its ID and define, and each measure's `valueset` lines with their names. Run on
2026-11-14 against the downloads: passed. Without the folder it is inconclusive.

**Compared with EHR-2's manifest** (79 OIDs of the 2014 engine, the windows and `DiseaseDefs`): 40 are in both, 39 are gone and **28 are new**.

- New (28): `2.16.840.1.113762.1.4.1021.121` Federal Administrative Sex; `.113762.1.4.1047.502` Overweight or Obese; `.113762.1.4.1248.429`
  Acute Health Crisis; `.113883.3.464.1003.1003` Hospice Encounter; `.1003.1006` Nutrition Services; `.1003.101.12.1010` Emergency Department
  Evaluation and Management Visit; `.1003.101.12.1080` Telephone Visits; `.1003.101.12.1088` Frailty Encounter; `.1003.101.12.1089` Virtual
  Encounter; `.1003.101.12.1090` Palliative Care Encounter; `.1003.110.12.1082` Advanced Illness; `.1003.113.12.1074` Frailty Diagnosis;
  `.1003.113.12.1075` Frailty Symptom; `.1003.1165` Hospice Diagnosis; `.1003.1167` Palliative Care Diagnosis; `.1003.118.12.1300` Frailty
  Device; `.1003.196.12.1510` Dementia Medications; `.1003.198.12.1135` Palliative Care Intervention; `.113883.3.526.3.1022` Physical Therapy
  Evaluation; `.526.3.1530` Speech and Hearing Evaluation; `.526.3.1561` and `.1562` Medications for Above/Below Normal BMI; `.526.3.1563`
  Underweight; `.526.3.1582` Patient Declined; `.526.3.1584` Hospice Care Ambulatory; `.113883.3.666.5.307` Encounter Inpatient;
  `2.16.840.1.114222.4.11.836` Race; `.837` Ethnicity.
- Gone (39, the 2014 versions' and the two retired measures'): `2.16.840.1.113883.3.464.1003.101.12.1008`, `.1012`, `.1013`, `.1014`,
  `.1030`, `.1048`; `.1003.109.12.1011`, `.1014`, `.1015`, `.1016`; `.1003.110.12.1027`, `.1028`, `.1034`; `.113883.3.526.3.1008`, `.1009`,
  `.1012`, `.1020`, `.1032`, `.1033`, `.1083`, `.1084`, `.1185`, `.1245`, `.1252`, `.1253`, `.1254`, `.1255`, `.1256`, `.1257`, `.1259`, `.402`;
  `.113883.3.600.1.1498`, `.1499`, `.1502`, `.1503`, `.1579`, `.1623`, `.462`, `.681`.

### 2. Direct reference codes (section 7.3)

- Each code of the office's Direct Reference Codes file is one `ehrcode` row per measure that lists it: `ValueSetOID` the fork key
  `drc:<CMS ID>:<role>` (`CqmCatalog.GetDirectCodeKey`; the CMS ID without its version, so the engine's key does not change with the yearly
  update), or `drc:<CMS ID>:Listed` for a code no role takes (hospice discharge, the hospice MDS item and its "Yes", FACIT-Pal, housing status,
  nursing home, medical equipment, CMS69's unused one); `ValueSetName` "Direct reference code: <role>"; `QDMCategory` "Direct Reference Code";
  `MeasureIds` the measure when it is one of the seven at its 2027 version; the code, description, code system and its OID from the file.
- **Roles** (`EhrValueSetImport.ResolveDirectCodes`, in `MakePick`): the codes of the role's measure at the catalog's version in the role's
  code system, and for a LOINC role the one whose description is the role's display name (case and white space ignored). A role of one code
  resolves when exactly one matches; CMS138's CPT role takes every CPT code of CMS138 and resolves with at least one. A role that matches a
  number other than it takes gives a warning that names it; an unresolved role's codes stay listed, and its criterion is "not calculated"
  (the engine's work, EHR-8). The pick's report lists each role with its count; the release record keeps them.
- HL7 ActCode (`2.16.840.1.113883.5.4`) is named by the importer (also from "HL7 ActCode"); it has no table here, so its code never counts as
  in the database, and the status grid lists it as a code system without a table.

### 3. The import (sections 7.1 and 7.2)

- `EhrValueSetImport.Parse(fileName,stream)` reads any of the three files and sets `EhrValueSetFile.Kind` from the sheets it read
  (`ValueSets`, `DirectReferenceCodes`, `CodeSystemVersions`; a file whose sheets are of two kinds is `Mixed` and refused; `EhrValueSetFileKind`
  is not stored). The value sets keep EHR-2's readers and checks; the two companions are read by the same workbook reader, as rows
  (`ListDirectCodes`, `ListCodeSystemVersions`). A Direct Reference Codes row without a measure is skipped and warned; a header row without
  its required columns (Direct Reference Codes: CMS ID, Code, Code System or its OID; Code System Versions: Code System or its OID and a
  version) refuses the file, naming them; a file of the kind with no rows is refused.
- **The version check** (`CheckMeasureVersions`) of a file that names measures (the value sets and the Direct Reference Codes): each of the
  seven missing, or at another version ("The file has CMS74v15 where this program reports CMS74v16."), warns; none of the seven at its 2027
  version refuses the file, naming the release this program needs. An SVS file without groups is checked by OIDs, as in EHR-2. The report and
  the record say "7 of 7 measures at the 2027 versions".
- **The manifest** (`GetManifest`) is the catalog's 68 value sets with their measures ("69v15,165v15"); `ArrayMeasureIds` the catalog's
  seven; `MeasureIds` of a row holds only the seven (EHR-2's rule).
- **One pick** (`MakePick(listFiles)`): exactly three files, one of each kind, none refused; otherwise the pick is refused, naming what is
  missing, twice or refused, and nothing is written. **One swap** (`Import(EhrValueSetPick)`): the value-set rows and the direct-reference-code
  rows together into EHR-2's scratch table, one `RENAME TABLE`, so a later import replaces both kinds together and an import that fails part
  way leaves every old row of both kinds. `Import(EhrValueSetFile)` is gone: no import of one file is left.
- **The release record** (`ForkEhrValueSetRelease`, `EhrValueSetRelease`): as EHR-2 (label, the value-set file's name and SHA-256, date,
  counts, the measure check), plus the three files (kind, name, SHA-256, size), the code-system versions, the roles and how each resolved, and
  the number of direct reference codes. A record written by EHR-2 still reads (the new lists are empty). The Setup security log entry names
  the release, the three files with their SHA-256, the counts, the measure check, the roles resolved and the versions recorded; no code.
- **Sizes (E-30):** unchanged until E-20's record; every value set of the file is still imported (EHR-2's choice).

### 4. The Code System Versions file and the status grid (section 7.4)

`EhrCodes.GetCodeSystemStatuses` adds, per code system, the loaded table's version (`codesystem.VersionCur`, through
`CodeSystems.GetForCurrentVersion`) and the release's (all versions it names, ", " between), and flags a loaded table older than the release.
`EhrValueSetImport.CompareCodeSystemVersions` maps each code system's two formats to a comparable value; **provisional** until E-20's record
and the offices' own `VersionCur` values:

| Code system | Comparable value |
|---|---|
| LOINC | the release number major.minor (2.80; 2.9 is before 2.80) |
| SNOMEDCT, RXNORM, CVX | the last date the string names: yyyy-MM-dd, yyyyMMdd, yyyy-MM or yyyyMM (a missing day is the first) |
| ICD10CM, CPT, HCPCS | the year |
| any other (CDCREC, SOP, CDT, ...) | none: both strings are shown, nothing is flagged |

The release may name several versions of one code system: the newest counts. The window (`FormCodeSystemsImport`) gains the columns Loaded
Version and In the Release, and the status "Loaded, but older than the release's version: codes of the newer value sets are missing from the
pick lists".

### 5. The window

`FormCodeSystemsImport` (the plan's Owns): the value-set row reads "eCQM value sets (VSAC, eligible clinicians, 2026-05-14)"; Import Value
Sets keeps EHR-2's order (Middle Tier refused, Setup, the licence prompt, now naming the release and its three files, the file dialog, the
import, the signal), with the dialog in multi-select: the three files are read each as a stream (never whole), picked together, the pick's
report shown (copy and paste), the confirmation names the codes of both kinds, and the import writes them in one swap. The status label in
`.designer.cs` names the new column.

### 6. LOINC's notice (E-23, E-37)

`THIRD-PARTY-NOTICES.txt` gains the component "LOINC" (after the AAFP CCR stylesheet): no file ships; the program's own files hold LOINC codes
(the vital-sign windows' exam codes, E-37) and the three display names of the catalog (E-23); copyright Regenstrief Institute, Inc. and the
LOINC Committee; licence at `https://loinc.org/license`; and LOINC's notice, in the words CMS's 2027 statements print it. `ReleaseIdentityTests`
lists the component (and checks its notice); no licence-title section is added, so `ThirdPartyNotices_NamesEveryShippedFileAndSource` is
unchanged. The embedded copy follows the file (`Resources.resx` links it). **For the lead:** LOINC's licence page could not be opened here
(no internet); check that the notice is the one the licence requires, with the copyright years if it asks for them.

### 7. Choices and deviations

- **The data-entry windows still name 2014 value sets** (EHR-7 owns them; section 8). After an office imports the 2026-05-14 release, the
  value sets that are not in the 2027 measures are empty, so until EHR-7 merges: the intervention window's BMI-medication and dialysis lists
  (`.600.1.1498`, `.1499`, `.109.12.1016`, `.1015`), the not-performed window's BMI exam, influenza and current-medications codes and its
  reasons other than "Medical Reason" (`.600.1.681`, `.526.3.402`, `.1254`, `.600.1.462`, `.526.3.1008`, `.1009`, `.600.1.1502`, `.1503`,
  `.526.3.1259`), and `DiseaseDefs.GetAllPregDiseaseDefs`, which takes the codes in both `.600.1.1623` and `.526.3.378` and so finds none. The
  tobacco, counseling and BMI follow-up lists keep working (their OIDs are 2027 value sets). The release notes say so.
- **`ValueSetSourceTests`**: the reflection of `QualityMeasures.GetEMeasureNum`/`GetEMeasureVersion` went with the 2014 manifest (the manifest
  is no longer the engine's), here rather than in EHR-6b; the manifest test is now the plan's: every value-set OID literal of the source is in
  `CqmCatalog.cs` and nowhere else, except the allowlist (`QualityMeasures.cs` until EHR-6b; `FormEhrNotPerformedEdit.cs`,
  `FormEhrPatientSmoking.cs`, `FormInterventionEdit.cs`, `FormInterventions.cs`, `FormMedical.cs`, `FormVitalsignEdit2014.cs` and
  `DiseaseDefs.cs` until EHR-7), and a file on the allowlist that names none fails the test, so the list only shrinks. The arc
  `2.16.840.1.113762.1.4` (VSAC-authored sets) is now among the OIDs it reads. The fixture guard (`Fixtures_HoldNoLiteralOfACodeFromTheEnginesLists`)
  skips GUIDs: the 2014 engine names the measures' set IDs, and so do the catalog's tests.
- **The catalog guard** (`Catalog_HoldsNoCodeLikeLiteral`, section 10.1) takes the LOINC pattern with three to seven digits before the
  hyphen, so the strata's age ranges ("1-5") are not codes; it also checks the statements as the catalog gives them, and four- and five-place
  CDT, HCPCS and CPT Category II shapes.
- The scratch database of the new database tests is `ehr6a_...` (`Support/ReleaseDb.cs`); the EHR-2 tests keep their `ehr2_` one and now
  import through a pick (their one direct reference code is a made-up HCPCS code, so the counts they test are unchanged).
  `ReleaseDb.End` clears the logged-on user, so the next fixture's cache refresh does not refuse a user of another scratch database.
- `EhrValueSetImport.ArrayMeasureIds` comes from the catalog; `SortMeasureIds` therefore orders by the catalog.
- `TableTypes/EhrCode.cs`'s summary names the 2026-05-14 release and the drc: keys (a comment).

### 8. Tests

`tests/EhrTests`, new files `CatalogTests.cs` (11), `DirectCodeTests.cs` (8), `DirectCodeDbTests.cs` (5) and `Support/ReleaseDb.cs`;
`ValueSetParseTests.cs`, `ValueSetSourceTests.cs`, `ValueSetDbTests.cs` and `Support/ValueSetFixtures.cs` moved to the 2027 catalog;
`tests/AttributionTests/ReleaseIdentityTests.cs` (the LOINC component). No project file edited.

- `CatalogTests`: the seven measures of section 4.1 (IDs, stewards, set and version IDs, CMS68 episode-based, CMS75 lower is better, the
  retired CMS127 and CMS147 absent); groups and strata (GUIDs, none twice, the defines); the 68 value sets, each measure's list equal to the
  record, the 72 in all, the four declared but unused; the 17 pick lists; the eight roles (display names only for LOINC); each statement
  equal to its recorded SHA-256; `GetStatementText`; the code-like guard; namespace and ASCII; the optional comparison with the downloads.
- `DirectCodeTests`: the synthetic release gives 7 of 7 with every role resolved and every row under its key; a pick without all three
  files, with two of a kind, with a fourth or with none is refused; a file of two kinds is refused; a role missing and a role twice warn (the
  twice is not resolved, both codes stay listed), CMS138's CPT role with three codes warns and resolves; the LOINC roles by display name (case
  and spaces ignored, not at another version); the Direct Reference Codes file's version check, rows without a measure, missing columns, an
  empty file; the Code System Versions file by name or OID and either header; the version mapping.
- `DirectCodeDbTests` (ehr6a_): the pick writes both kinds in one swap and records the three files, versions and roles, with the Setup log
  entry (no code); a second pick replaces both kinds together and the roles resolve again; a failure part way and a pick of two files leave
  every old row of both kinds and no scratch table; the status grid flags an older SNOMED CT table, not an equal LOINC one, and shows both
  strings for CDCREC; a Middle Tier client cannot import, and its list has the direct reference codes after `InvalidType.EhrCodes`.
- `ValueSetParseTests`: the 2027 versions (all seven; CMS74v15 of the 2026 release warns; the whole 2026 release refused, workbook and SVS),
  the catalog's measures in `MeasureIds`, ActCode; `ValueSetDbTests`: as before, through the pick; `LoadTime_...` looks up without `IsInDb`
  (the synthetic large release's rows of that value set fall on code systems whose tables are not loaded, now that the manifest has 68 sets).

Targeted runs (`run-tests.ps1 -Mode Targeted`, after building the test projects through the lane), `ehr6a-1-run2.txt` (with
`CQM_SPECS_DIR` set to the download folder):

| Assembly | Result |
|---|---|
| EhrTests | 232 of 232 passed |
| AttributionTests | 50 of 50 passed |
| MiddleTierMockTests | 125 of 125 passed |
| NoHqTests | 57 of 57 passed |

The final runs after the full build are in section 10.

### 9. For the lead

- **Merge after E-20's record** (section 0), and after S4-1 (merged). No schema change: `db/init/new-database.sql`, InitSqlTests and
  ForkMigrationTests are not touched.
- **Release notes:** written into `docs/release-notes/next.md` (the summary line and the section "EHR: the value sets from your own VSAC
  download", rewritten for the 2026-05-14 release and its three files; the 2014 claim is gone).
- **Launch checks** (owner, test database): import a synthetic release (the three files of `ValueSetFixtures.MakeRelease`, written from a
  test): the report ("7 of 7 measures at the 2027 versions", the roles 8 of 8), the confirmation, the status grid's Loaded Version and In the
  Release columns; pick two files only: refused, nothing changes. On the office's computer, the real three files: "7 of 7", the roles, the
  sizes (under the limits, or the report says which).
- LOINC's notice: section 6.

### 10. Final runs

Full build through the lane: 0 errors, no warning in a changed file. Then the Targeted runs, `ehr6a-1-run3.txt` (without `CQM_SPECS_DIR`):

| Assembly | Result |
|---|---|
| EhrTests | 232: 231 passed, 1 skipped (`Catalog_EqualsTheDownloadedPackages_WhenCqmSpecsDirIsSet`, inconclusive without the download folder; it passed in run 2 with it), 0 failed |
| AttributionTests | 50 of 50 passed |
| MiddleTierMockTests | 125 of 125 passed |
| NoHqTests | 57 of 57 passed |

No `ehr6a_` or `ehr2_` database was left on the server after the runs. One `ehr3_..._ccd` database of the EHR-3 tests' own fixture was
there after the run (not this package's; its stale clean-up drops it after 12 hours), and was left alone.
