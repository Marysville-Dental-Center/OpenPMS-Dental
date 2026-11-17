# Port note: EHR-6 (the 2027 catalog and import; the 2014 and 2011 code deleted)

Packages EHR-6a and EHR-6b of `docs/specs/ehr-2027-measures-plan.md` (below: "the plan"), section 12. This note has the EHR-6a part; EHR-6b
adds its own part below it.

## EHR-6a The 2027 catalog and import

On branch `ehr/ehr6a-1` from `main` `b8e931cea` (S4-1, MSTest 4, merged). The owner decided the plan on 2026-11-14 (all of section 14 as
recommended). No schema change, no new `PrefName`, no project file edited (the business project globs the new `Db Multi Table/Cqm` folder),
no package or third-party file added, no download, no code of any code system anywhere in the repository. Synthetic data only.

### 0. E-20's record of the 2026-05-14 files (2026-11-16)

**The record** (the plan, sections 7.1 and 12, E-20). The owner downloaded the release on 2026-11-16 from VSAC's Downloadable Resources
with the office's own UMLS licence; the lead copied the five files into a folder outside the repository. They were read there only, with
`python -I` (the standard library's `zipfile` and `xml.etree`, the sheets streamed) and with the program's own importer (section 14). This
section holds the layout only: names, sizes, sheets, headers, row positions, how cells are stored, the code systems' names, OIDs and
version strings, counts. No code, description, value-set row or file of the release is in the repository, in a commit message, in a test
fixture or in a log that is committed.

**The files.** Each is a `.zip` holding one `.xlsx` of the same name without `.zip` (not three files in one `.zip`, and no SVS file).

| File | Size | SHA-256 | Inside | Sheets | Sheets uncompressed |
|---|---|---|---|---|---|
| `ec_hospip_hospop_cms_20260514.xlsx.zip` (value sets sorted by CMS ID) | 17,457,241 | `0d7b181e5c449f5d77323fc41fec48338601a8ef995dee46d153897672cf862b` | `ec_hospip_hospop_cms_20260514.xlsx`, 20,857,597 bytes | 73 | 355,706,131 bytes (all parts 357,721,535) |
| `ec_hospip_hospop_category_20260514.xlsx.zip` (by QDM category) | 16,313,389 | `533539f8b939cb6ff5bab6097df3aeea7eff39aee973a93d890a0d29dab86e8a` | `.xlsx`, 18,288,723 bytes | 17 | 343,371,344 bytes |
| `ec_hospip_hospop_unique_vs_20260514.xlsx.zip` (unique value sets) | 8,753,314 | `52c0a478ae0f806d0e56b63c8af14a26de6cabd4b5f509a9db62a8aea78912cb` | `.xlsx`, 9,541,858 bytes | 1 | 184,839,681 bytes |
| `drc_cms_20260514.xlsx.zip` (Direct Reference Codes) | 71,869 | `11662824e096b83ba926bb54b0652fbb93cca41882ae1cb944556da4ca6912ec` | `drc_cms_20260514.xlsx`, 72,967 bytes | 1 | 461,862 bytes (all parts 499,043) |
| `cs_versions_cms_20260514.xlsx.zip` (Code System Versions) | 30,083 | `d17b6ff2c08c729e541f892d78adaf5e4ac5a3456096b8ac7076326c2408b555` | `cs_versions_cms_20260514.xlsx`, 30,964 bytes | 1 | 10,000 bytes (all parts 46,994) |

The release is VSAC's combined "EC, HOSPIP and HOSPOP" release: the eligible clinicians' measures with the hospital inpatient and outpatient
ones (73 measures in the by-CMS workbook, 64 in the Direct Reference Codes file). **The office picks three files:** the by-CMS workbook, the
Direct Reference Codes file and the Code System Versions file. **The other two sortings are not needed:** they hold the same value sets in
another order, and the importer refuses them (a sheet of each is past the 64 MB part limit, and the refusal names the by-CMS workbook).

**How the cells are stored (all five).** Every text cell is an inline string (`t="inlineStr"`, `<is><t>`): 4,048,794 in the by-CMS
workbook, 6,319 in the Direct Reference Codes file, 126 in the Code System Versions file. Each sheet's cell A1 is an empty styled cell (row 1
holds nothing else). Each workbook has a `xl/sharedStrings.xml` part that holds no string (137 bytes in the by-CMS workbook). Cells are placed by
their reference. The by-CMS workbook also has a drawing and a 25,284-byte JPEG image per sheet, which the importer does not read.

**The value sets sorted by CMS ID** (`ec_hospip_hospop_cms_20260514.xlsx`):

- 73 sheets, one per measure, named by the measure's CMS ID and version (`CMS<number>v<version>`, for example `CMS68v16`); the CMS ID cell
  of every row is its sheet's name. All seven of the catalog's measures are there, at their 2027 versions.
- Row 1 empty; **the header on row 2**, 18 columns: `CMS ID`, `Program Candidate`, `CBE Number`, `Value Set Name`, `Value Set OID`,
  `QDM Category`, `Definition Version`, `Expansion Version`, `Purpose: Clinical Focus`, `Purpose: Data Element Scope`, `Purpose: Inclusion
  Criteria`, `Purpose: Exclusion Criteria`, `Code`, `Description`, `Code System`, `Code System OID`, `Code System Version`, `Expansion ID`.
- 224,860 data rows. A value set used by several measures is listed on each of their sheets (the supplemental data elements on all 73).
- The sheets uncompressed: 355,706,131 bytes; the smallest 208,422 (`CMS1056v4`), the largest 33,097,217 (`CMS1061v1`). The seven:

| Sheet | Bytes | Rows | Value sets |
|---|---|---|---|
| `CMS68v16` | 607,196 | 382 | 6 |
| `CMS69v15` | 3,831,775 | 2,611 | 22 |
| `CMS74v16` | 294,017 | 190 | 10 |
| `CMS75v15` | 404,854 | 267 | 10 |
| `CMS138v15` | 919,527 | 581 | 32 |
| `CMS155v15` | 3,452,168 | 2,357 | 21 |
| `CMS165v15` | 8,472,392 | 5,036 | 31 |

- The versions: `Expansion Version` is `eCQM Update 2026-05-14` and `Expansion ID` `20260514` on every row; `Definition Version` is an
  eight-digit number of the value set's own definition, which differs from one value set to the next. **The value set's version the
  importer records is the Expansion Version** (the Expansion ID only in a file without it, the Definition Version only in a file with
  neither, as EHR-2 read the June 2013 workbook).
- `QDM Category`: one category, or several joined by commas (for example `Assessment,Intervention`), or empty (60,049 rows).
- The code systems (name, OID, the `Code System Version` the rows name, rows):

| Code System | Code System OID | Versions (rows) |
|---|---|---|
| SNOMEDCT | 2.16.840.1.113883.6.96 | 2025-09 (93,026), 2025-03 (128), 2024-09 (269), 2024-03 (487), 2023-09 (84), 2023-03 (795), 2022-09 (395), 2022-03 (175), 2021-09 (46), 2021-03 (42), 2020-09 (96), 2020-03 (20), 2019-09 (12), 2019-03 (5), 2018-09 (32), 2018-03 (87), 2017-09 (5), 2017-03 (1), 2016-09 (1), 2015-09 (3), 2012-07 (1) |
| ICD10CM | 2.16.840.1.113883.6.90 | 2026 (74,828) |
| ICD10PCS | 2.16.840.1.113883.6.4 | 2026 (26,022) |
| SOPT | 2.16.840.1.113883.3.221.5 | 9.2 (9,175) |
| LOINC | 2.16.840.1.113883.6.1 | 2.81 (7,263) |
| RXNORM | 2.16.840.1.113883.6.88 | 2026-01 (7,053) |
| CPT | 2.16.840.1.113883.6.12 | 2026 (2,960), 2024 (75), 2021 (1), 2020 (1), 2006 (2) |
| HCPCS Level II | 2.16.840.1.113883.6.285 | 2026 (948), 2025 (30), 2024 (16) |
| CDCREC | 2.16.840.1.113883.6.238 | 1.3 (584) |
| HSLOC | 2.16.840.1.113883.6.259 | 2022 (76) |
| CVX | 2.16.840.1.113883.12.292 | 2025-11-19 (54) |
| CDT | 2.16.840.1.113883.6.13 | 2025 (35) |
| PresentOnAdmission | 2.16.840.1.113883.6.301.11 | 2021 (23) |
| RoleCode | 2.16.840.1.113883.5.111 | 3.0.0 (4) |

  So one code system has several versions in one release: each row names the version its own code comes from, an older one for a code
  that is no longer in the newest.

**The two other sortings** (not picked): `ec_hospip_hospop_category_20260514.xlsx`, 17 sheets named by QDM category (`Adverse Event`,
`Allergy-Intolerance`, `Assessment`, `Communication`, `Condition-Diagnosis-Problem`, `Device`, `Diagnostic Study`, `Encounter`,
`Immunization`, `Individual Characteristic`, `Intervention`, `Laboratory Test`, `Medication`, `Physical Exam`, `Procedure`, `Symptom`, `No
QDM Category Assigned`), row 1 empty, the header on row 2 with the 18 columns less `QDM Category` (17), the largest sheet
`Condition-Diagnosis-Problem` 138,102,070 bytes; `ec_hospip_hospop_unique_vs_20260514.xlsx`, one sheet `EC, HOSPIP and HOSPOP` of
184,839,681 bytes, row 1 empty, the header on row 2 with 14 columns (`Value Set Name`, `Value Set OID`, `Definition Version`, `Expansion
Version`, the four Purpose columns, `Code`, `Description`, `Code System`, `Code System OID`, `Code System Version`, `Expansion ID`: no
measure and no QDM category).

**The Direct Reference Codes file** (`drc_cms_20260514.xlsx`): one sheet `drc-2026-05-14`, 488 rows. Row 1 empty; row 2 an explanatory
paragraph in one cell; **the header on row 3**, 13 columns: `Code`, `Description`, `Code System`, `Code System Version`, `Code System OID`,
`eCQM Type`, `eCQM Title`, `eCQM ID`, `CBE Number`, `eCQM Version`, `Measure GUID`, `Care Setting`, `Prior Value Set OID`. 485 data rows, one
per measure and code.

- **The measure is `eCQM ID`**, with its version (`CMS68v16`); `eCQM Version` repeats the version alone (`16`). 64 measures. `eCQM Type`
  Eligible Clinicians (386 rows), Hospital Inpatient (63), Hospital Outpatient (36); `Care Setting` Ambulatory (422), Acute (63); `CBE
  Number` "Not Applicable" or a number; `Prior Value Set OID` filled on every row.
- The catalog's seven, 40 rows: CMS68v16 1 (SNOMEDCT_US 1), CMS69v15 7 (SNOMEDCT_US 3, LOINC 3, ActCode 1), CMS74v16 4 (SNOMEDCT_US 3,
  LOINC 1), CMS75v15 4 (the same), CMS138v15 9 (SNOMEDCT_US 3, LOINC 1, CPT 4, ICD10CM 1), CMS155v15 4 (SNOMEDCT_US 3, LOINC 1), CMS165v15 11
  (SNOMEDCT_US 4, LOINC 6, ICD10CM 1). The other 445 rows are the other measures'.
- The code systems (name, OID, version, rows): **SNOMEDCT_US** 2.16.840.1.113883.6.96 2025-09 (210), LOINC 2.16.840.1.113883.6.1 2.81
  (210), CPT 2.16.840.1.113883.6.12 2026 (21), RXNORM 2.16.840.1.113883.6.88 2026-01 (18), ICD10CM 2.16.840.1.113883.6.90 2026 (13), ActCode
  2.16.840.1.113883.5.4 9.0.0 (6), HSLOC 2.16.840.1.113883.6.259 2022 (2), HCPCS 2.16.840.1.113883.6.285 2026 (2), CVX
  2.16.840.1.113883.12.292 2025-11-19 (1), CDCREC 2.16.840.1.113883.6.238 1.3 (1), ICD10PCS 2.16.840.1.113883.6.4 2026 (1). So SNOMED CT is
  `SNOMEDCT_US` here and `SNOMEDCT` in the two other files, and HCPCS is `HCPCS` here and in the Code System Versions file but `HCPCS Level
  II` in the value sets.

**The Code System Versions file** (`cs_versions_cms_20260514.xlsx`): one sheet `Code System Versions`, 45 rows. Row 1 empty; row 2 a title
in one cell; row 3 empty; row 4 `Publication Date`, `May 14, 2026`; **the header on row 5**, 3 columns: `Code System Name`, `Code System
Version`, `Code System OID`. 40 rows, one per code system and version, the versions the release's value sets use:

| Code System Name | Code System OID | Code System Version |
|---|---|---|
| CDCREC | 2.16.840.1.113883.6.238 | 1.3 |
| CDT | 2.16.840.1.113883.6.13 | 2025 |
| CPT | 2.16.840.1.113883.6.12 | 2006, 2020, 2021, 2024, 2026 |
| CVX | 2.16.840.1.113883.12.292 | 2025-11-19 |
| HCPCS | 2.16.840.1.113883.6.285 | 2024, 2025, 2026 |
| HSLOC | 2.16.840.1.113883.6.259 | 2022 |
| ICD10CM | 2.16.840.1.113883.6.90 | 2026 |
| ICD10PCS | 2.16.840.1.113883.6.4 | 2026 |
| LOINC | 2.16.840.1.113883.6.1 | 2.81 |
| PresentOnAdmission | 2.16.840.1.113883.6.301.11 | 2021 |
| RXNORM | 2.16.840.1.113883.6.88 | 2026-01 |
| RoleCode | 2.16.840.1.113883.5.111 | 3.0.0 |
| SNOMEDCT | 2.16.840.1.113883.6.96 | 2012-07, 2015-09, 2016-09, 2017-03, 2017-09, 2018-03, 2018-09, 2019-03, 2019-09, 2020-03, 2020-09, 2021-03, 2021-09, 2022-03, 2022-09, 2023-03, 2023-09, 2024-03, 2024-09, 2025-03, 2025-09 |
| SOPT | 2.16.840.1.113883.3.221.5 | 9.2 |

Every version the value-set rows name is in this list, and so is every version the Direct Reference Codes file names but ActCode
9.0.0 (the list is that of the value sets).

**What each reader does now** (EHR-6a's provisional table, fixed to the record; the changes are section 14):

| The record | The reader (`EhrCodes.cs`) |
|---|---|
| Three `.zip` files of one `.xlsx` each | EHR-2's `.zip` reader opens the `.xlsx` inside (in memory, within the 64 MB part limit) and reads its sheets; a pick of these three names is accepted (`Record_ThePickOfTheThreeFilesByTheirNames_IsAccepted_SevenOfSeven`) |
| The headers on rows 2, 3 and 5, below empty, title, paragraph and date rows | The header search of the first 30 rows (`ReadSheet`): a row above the header names no column and is not read as a row |
| The by-CMS header | `_dictHeaders`: CMS ID, Value Set Name, Value Set OID, QDM Category, Code, Description, Code System, Code System OID, Code System Version read; Expansion Version, Expansion ID and Definition Version are three fields (the value set's version from the first that a file has); the other columns not read. `GetSheetKind`: Value Set OID, so value sets |
| The Direct Reference Codes header | `eCQM ID` names the measure (`eCQM Version` joined to an ID without a version); `GetSheetKind`: a Code without a value set; required eCQM ID (or CMS ID), Code, Code System (or its OID) |
| The Code System Versions header | Code System Name, Code System Version, Code System OID (`GetSheetKind`: a code system and a version without a code); EHR-6a's column Version still read |
| Inline strings, an empty shared-strings part | EHR-2's cell reader reads inline strings and works without the part (`Record_AWorkbookOfInlineStrings_WithoutASharedStringsPart_IsRead`) |
| 73 measures' value sets, 377 MB to read | E-30: only the manifest's value sets are kept as the sheets stream (the others counted); the total limit 1,024 MB (section 14) |
| 64 measures' direct reference codes | `FinishDirectCodes` keeps only the catalog's seven at their 2027 versions (the others counted); `ResolveDirectCodes` resolves the eight roles from them |
| SNOMEDCT_US, HCPCS Level II, SOPT | `GetCodeSystemName`: by the OID first (every row of the record has one), else by name: SNOMEDCT, HCPCS, SOP |
| Several versions of one code system, each row naming its own | The versions the imported codes name are counted per code system (`ListCodeSystemVersionsUsed`, in the release record); the status grid flags a loaded table older only where codes name a newer version than it (`CountCodesNewerThanLoaded`); a value-set version the Code System Versions file does not list warns |
| Version formats: dates `yyyy-MM` (SNOMEDCT, RXNORM) and `yyyy-MM-dd` (CVX), `2.81` (LOINC), years (CPT, HCPCS, ICD10CM, ICD10PCS, CDT, HSLOC), others (`1.3`, `9.2`, `9.0.0`, `3.0.0`) | `GetComparableVersion` as in section 4: dates, LOINC's release number, the year for ICD10CM, CPT and HCPCS; none for the others (both strings shown, nothing flagged) |
| Expansion Version `eCQM Update 2026-05-14` | Reported ("Value-set versions in the file"); the release label stays `CqmCatalog.RELEASE_LABEL` |

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
- **Reading the pick within EHR-2's limits** (`ParsePick(listFileNames,funcOpenStream)`, review fix in `ehr/ehr6a-2`): EHR-2's limits
  (`MAX_ROWS` 1,000,000, 64 MB per part, 256 MB per file) are for one parsed value-set file in memory in a 32-bit process, so the pick never
  holds more. A pick of other than three files is refused before any file is opened (the window checks the count before it reads anything
  too). The three are read one after another under one `MAX_ROWS` for the whole pick ("The files of the pick have more rows together than the
  importer reads"); a second file of a kind already read is refused at its first row of that kind ("The pick has more than one file with the
  value sets: this file and ..."), so its rows are never held; a refused file keeps only its report. `MakePick` then checks the three as
  before. An office that picks the workbook and the SVS `.zip` of the same release with the two companions (four files) is refused unread;
  one that picks two value-set files and one companion holds one value-set parse.
- **The release record** (`ForkEhrValueSetRelease`, `EhrValueSetRelease`): as EHR-2 (label, the value-set file's name and SHA-256, date,
  counts, the measure check), plus the three files (kind, name, SHA-256, size), the code-system versions, the roles and how each resolved, the
  number of direct reference codes, and the Direct Reference Codes file's own measure check (`MeasureCheckDirectCodes`, section 13). A record
  written by EHR-2 still reads (the new lists are empty), and the window says it is the old release (section 13). The Setup security log entry names
  the release, the three files with their SHA-256, the counts, the measure check, the roles resolved and the versions recorded; no code.
- **Sizes (E-30):** decided from E-20's record (section 14): only the manifest's value sets are kept as the file is read (a change from
  EHR-2's "every value set is imported"), and the total of a file's uncompressed parts is 1,024 MB (from 256 MB); the part limit stays 64 MB.

### 4. The Code System Versions file and the status grid (section 7.4)

`EhrCodes.GetCodeSystemStatuses` adds, per code system, the loaded table's version (`codesystem.VersionCur`, through
`CodeSystems.GetForCurrentVersion`) and the versions the imported codes name (since EHR-6a-R, section 14: newest first, at most three; for an
older record the Code System Versions file's), and flags a loaded table older where imported codes name a newer version than it
(`CountCodesNewerThanLoaded`, with their count). `EhrValueSetImport.GetComparableVersion` maps each code system's formats to a comparable
value, checked against E-20's record (section 0: SNOMEDCT and RXNORM `yyyy-MM`, CVX `yyyy-MM-dd`, LOINC `2.81`, CPT, HCPCS and ICD10CM a
year) and the program's own `VersionCur` values (its loaders write one only for CPT, a year):

| Code system | Comparable value |
|---|---|
| LOINC | the release number major.minor (2.80; 2.9 is before 2.80) |
| SNOMEDCT, RXNORM, CVX | the last date the string names: yyyy-MM-dd, yyyyMMdd, yyyy-MM or yyyyMM (a missing day is the first) |
| ICD10CM, CPT, HCPCS | the year |
| any other (CDCREC, SOP, CDT, ...) | none: both strings are shown, nothing is flagged |

E-20's record shows several versions of one code system in one release (SNOMEDCT from 2012-07 to 2025-09, CPT from 2006 to 2026), each
row naming its own code's version: a table is older for the program only by the codes that name a newer version than it, not by the
newest version the release lists (`CompareCodeSystemVersions`, which compares two strings with the newest counting, stays for that). Value-Set Codes counts the value-set rows only; the direct
reference codes (the `drc:` rows) are counted apart, in Direct Codes (section 13). The window (`FormCodeSystemsImport`) gains the columns
Direct Codes, Loaded Version and In the Release, and the status "Loaded, but older than the release's version: codes of the newer value sets are missing from the
pick lists" (since EHR-6a-R with the number of codes of a newer version).

### 5. The window

`FormCodeSystemsImport` (the plan's Owns): the value-set row reads "eCQM value sets (VSAC, eligible clinicians, 2026-05-14)"; Import Value
Sets keeps EHR-2's order (Middle Tier refused, Setup, the licence prompt, now naming the release and its three files, the file dialog, the
import, the signal), with the dialog in multi-select: a pick of other than three files is refused before any is read; the three are read
each as a stream (never whole) through `ParsePick` (section 3), picked together, the pick's
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
  tobacco, counseling and BMI follow-up lists keep working (their OIDs are 2027 value sets). The release notes say so. Since EHR-6a-R only
  the catalog's value sets are kept (E-30), so the two of these the 2026-05-14 release does hold, under other measures (`.526.3.1008`,
  `.526.3.1259`), stay empty too (section 14).
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

`tests/EhrTests`, new files `CatalogTests.cs` (11), `DirectCodeTests.cs` (8; 11 after the review fix, section 11), `DirectCodeDbTests.cs` (5) and `Support/ReleaseDb.cs`;
`ValueSetParseTests.cs`, `ValueSetSourceTests.cs`, `ValueSetDbTests.cs` and `Support/ValueSetFixtures.cs` moved to the 2027 catalog;
`tests/AttributionTests/ReleaseIdentityTests.cs` (the LOINC component). No project file edited.

- `CatalogTests`: the seven measures of section 4.1 (IDs, stewards, set and version IDs, CMS68 episode-based, CMS75 lower is better, the
  retired CMS127 and CMS147 absent); groups and strata (GUIDs, none twice, the defines); the 68 value sets, each measure's list equal to the
  record, the 72 in all, the four declared but unused; the 17 pick lists; the eight roles (display names only for LOINC); each statement
  equal to its recorded SHA-256; `GetStatementText`; the code-like guard; namespace and ASCII; the optional comparison with the downloads.
- `DirectCodeTests`: the synthetic release gives 7 of 7 with every role resolved and every row under its key; a pick without all three
  files, with two of a kind, with a fourth or with none is refused (through `ParsePick`, as the window reads it: a count other than three
  unread, the missing kind named for three files; `ParseEachThenPick` keeps `MakePick`'s file-by-file checks); a four-file pick with two
  value-set files opens no file; a second value-set file keeps no rows (one value-set file's rows at most); the row budget is the whole
  pick's (review fix, three tests); a file of two kinds is refused; a role missing and a role twice warn (the
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

### 11. Review fix (`ehr/ehr6a-2`): the pick within EHR-2's memory limits

The review found that the window parsed every file of the multi-select, keeping each parse, before `MakePick` refused a pick of other
than three files or of two files of one kind: a pick of the workbook and the SVS `.zip` of one release with the two companions held two
value-set parses, a whole download folder N, in a 32-bit process. Fixed by `ParsePick` (section 3): a count other than three refused before
any file is read (in the window and in `ParsePick`), one `MAX_ROWS` for the whole pick, a second file of a kind refused at its first row
with its rows not kept, and a refused file's rows dropped. `DirectCodeTests` gains three tests (four files with two value-set files open no
file; a second value-set file keeps no rows; the row budget is the pick's); `ValueSetSourceTests` checks the window reads through
`ParsePick` with the count checked first.

Full build through the lane: 0 errors, no warning in a changed file. Targeted runs: `ehr6a-2-run1.txt` (all four) and `ehr6a-2-run2.txt`
(EhrTests and NoHqTests again):

| Assembly | Result |
|---|---|
| EhrTests | run 2: 235: 234 passed, 1 skipped (`Catalog_EqualsTheDownloadedPackages_WhenCqmSpecsDirIsSet`, as in section 10), 0 failed. Run 1 had one failure, `LoadTime_AFullSyntheticRelease_LoadsInUnderASecond` (first load 1,572 ms against its 1 s bound while the other assemblies ran beside it); this fix does not touch the load, and it passed in run 2 and in the EhrTests-only run before it |
| AttributionTests | run 1: 50 of 50 passed |
| MiddleTierMockTests | run 1: 125 of 125 passed |
| NoHqTests | runs 1 and 2: 57 of 57 passed |

No `ehr6a_` database was left on the server after the runs.

### 12. Review fix (`ehr/ehr6a-3`): a pick refused for its files' content opens with their refusal

The review found that `MakePick` counted a kind only among the files that were not refused, so a pick of the 2026 release's three files
(CMS74v15 and so on; the likeliest real mistake) was refused with "Pick the three files of the release together ... Files picked: 3." first,
then the two files' own refusals ("None of the seven measures this program reports is in the file at its 2027 version ... This program needs
the release for the 2027 performance period: ..."), then "The pick has no file with the value sets." and "The pick has no file with the
Direct Reference Codes file.", although all three were picked; the window's status line (`ListRefusals[0]`) showed only the first, misleading
line. Fixed in `MakePick`: a kind is missing only when no file of that kind is in the pick, refused or not ("more than one" still counts the
files not refused, as before), and the count text goes first only when the pick itself is wrong (other than three files, a kind missing or
doubled). A pick whose only fault is a file refused for its content now opens with that file's refusal, naming the release this program
needs (section 7.2), and the status line shows it. `DirectCodeTests` gains
`Pick_TheThreeFilesOfThe2026Release_IsRefused_FirstNamingTheReleaseThisProgramNeeds` (the three files with every measure one version back,
through `ParsePick` and through `MakePick` of the files read one by one: the first refusal is the value-set file's own and names
`RELEASE_LABEL`, the Direct Reference Codes file's is the second and last, no "no file with" and no "Files picked: 3").

Full build through the lane: 0 errors. Targeted run `ehr6a-3-targeted.txt`:

| Assembly | Result |
|---|---|
| EhrTests | 236: 235 passed, 1 skipped (`Catalog_EqualsTheDownloadedPackages_WhenCqmSpecsDirIsSet`, as in section 10), 0 failed |
| AttributionTests | 50 of 50 passed |
| MiddleTierMockTests | 125 of 125 passed |
| NoHqTests | 57 of 57 passed |

No `ehr6a_` database was left on the server after the run.

### 13. Lead fix round 4 (`ehr/ehr6a-4`)

On top of the lead's 9935e7380 (LOINC's notice with the licence's copyright years, 1995-2026):

- **A pick `MakePick` did not build** (`Import`): a pick that is not refused but lacks `FileValueSets`, `FileDirectCodes` or
  `FileCodeSystemVersions`, or has other than three files (a `new EhrValueSetPick()`), is refused before `DropLeftoverTables` and any table
  work: "The pick is not the release's three files as MakePick reads them. Nothing was imported, and the value sets in the database are
  unchanged." Before, such a pick without its value-set file passed the count check (0 of 0 written), swapped in a table without the
  value-set rows, and only then threw at the release record: data loss, not just an exception.
- **The count-mismatch refusal** says "Import the three files again."
- **A failure after the swap**: the release record (`PrefC.UpsertRaw`, the Setup log entry) is in a try/catch. A failure returns
  `GetRecordFailedText` ("The codes of the three files replaced the old ones, but the release record was not saved: ... Import the three
  files again."), and `Import(pick,out areRowsReplaced)` tells the window, which then shows "Codes replaced, release not recorded!" (not
  "Import failed!") and signals `InvalidType.EhrCodes` and `InvalidType.Prefs`. `Import(pick)` stays for the callers that need no flag.
  The test seam is `ActionBeforeRecord`.
- **Both measure checks recorded**: `EhrValueSetRelease.MeasureCheckDirectCodes` keeps the Direct Reference Codes file's own check next to
  the value-set file's `MeasureCheck`; the log entry gives it after the value sets' ("...; 7 of 7 measures at the 2027 versions; direct
  reference codes: 7 of 7 measures at the 2027 versions; ..."), "not recorded" for a record written before.
- **The status grid**: `CountValueSetCodes` (Value-Set Codes) leaves out the `drc:` rows; the new `CountDirectCodes` (column Direct Codes)
  counts them, and a code system only the direct reference codes use is still listed (ActCode).
- **A second file of a kind** (refused in `AddRawRow` or `ParsePick`) has `IsKindTaken`, and its report gives only its name, SHA-256,
  "Holds" and the refusal: no "Rows read", "found: 0 of N" or "Measures" line for a file that was never read.
- **The old release**: `GetValueSetVersionText` of a record whose `Label` is not `RELEASE_LABEL` (EHR-2's June 2013 import, in the same
  preference row) reads "old release loaded (its label, imported date: file). Import the release this program needs: [RELEASE_LABEL]."
- **Tests**: `ReleaseIdentityTests` matches the LOINC trademark sentence across the notice's wrap (`\s+` between "registered" and
  "United"). `CatalogTests` holds no NCQA words (the NCQA measures by `IsNcqa`; CMS155's eight paragraphs derived from the catalog's own
  field; the recorded SHA-256 still pins each statement), and the guard's self-test samples are built at run time in shapes no code system
  publishes (five zeros, a LOINC number with the wrong mod-10 check digit, a reserved ICD-10 letter, D0000, 0000F), each of the guard's four
  patterns firing on one. `LoadTime_AFullSyntheticRelease_LoadsInUnderASecond` keeps its 1 s bound on the best of three cold loads, all three
  in the timing text. New: `DirectCodeDbTests` `Import_APickMakePickDidNotBuild_IsRefusedBeforeAnyTableWork_TheRowsAndTheReleaseUnchanged`,
  `Import_AFailureWhileTheReleaseIsRecorded_SaysTheCodesWereReplaced_NotThatTheImportFailed`,
  `Import_RecordsTheDirectReferenceCodesFilesOwnMeasureCheck_ApartFromTheValueSetFiles` and
  `Status_TheValueSetCodes_LeaveOutTheDirectReferenceCodes_CountedApart`; `DirectCodeTests` checks the second file's report;
  `ValueSetDbTests` checks the HCPCS direct code counted apart and the June 2013 record's text.
- **Comments**: `EhrSteps.cs` names the 2026-05-14 release and its three files. No migration behaviour changes.

Kept for E-20, as before: one `.zip` or three files, the Code System Versions file's release check, the header names, E-30's sizes.

Full build through the lane: 0 errors, no warning in a changed file. Targeted run `ehr6a4-fix-r1.txt`:

| Assembly | Result |
|---|---|
| EhrTests | 240: 239 passed, 1 skipped (`Catalog_EqualsTheDownloadedPackages_WhenCqmSpecsDirIsSet`, as in section 10), 0 failed |
| AttributionTests | 50 of 50 passed |
| MiddleTierMockTests | 125 of 125 passed |
| NoHqTests | 57 of 57 passed |

No `ehr6a_` database was left on the server after the run.

Review fix (`ehr/ehr6a-5`): the round's business-layer commit a65f13bdf had rewritten every line end of `EhrCodes.cs` from CRLF to LF (the
blob is CRLF on `main` and `ehr/ehr6a-3`), so its real change (66 insertions, 11 deletions with `--ignore-all-space`) showed as some 5,200
changed lines and every other branch touching the file would conflict on each line. e1de85bf1 puts the CRLF back with no other change (`git
diff --ignore-cr-at-eol ehr/ehr6a-4` is empty); `core.autocrlf` is true here and the index blob was LF, so the file was staged with
`core.autocrlf=false` to keep its CRLF in the blob. `git diff ehr/ehr6a-3..ehr/ehr6a-5` on the file is now 85 insertions and 30
deletions (the release record's lines re-indented inside the new try block). No other file of the round changed its line ends. Full build
through the lane: 0 errors. Targeted run `ehr6a-5-crlf.txt`: EhrTests 240 (239 passed, 1 skipped as in section 10), AttributionTests 50,
MiddleTierMockTests 125, NoHqTests 57, all passed.

### 14. EHR-6a-R: the readers fixed to E-20's record (`ehr/ehr6a-r-1`)

On branch `ehr/ehr6a-r-1` from `ehr/ehr6a-5`, with `main` merged in first (12a7ab125: stage 3a, the tooth chart work, EHR-6b, EHR-4b; the
three conflicts, the release notes' EHR line, the plan's status and `ValueSetSourceTests`, resolved as the EHR-7 step 0 merge b0fec2645
did, keeping both sides). No schema change, no project file, no package, no download; synthetic data only in the repository.

**The readers** (`OpenPmsDentalBusiness/Data Interface/EhrCodes.cs`), each from the record of section 0:

- **The headers** (`_dictHeaders`). `eCQM ID` (also `eCQM IDs`) fills the measure, besides CMS ID; `eCQM Version` is its own field, joined
  to an ID without a version (`CMS68` and `16` give CMS68v16; an ID with a version keeps it). `Expansion Version`, `Expansion ID` and
  `Definition Version` are three fields, so the two versions of one row never collide in the header map (EHR-6a mapped both to one field,
  and the first column, Definition Version, won). **The choice:** the value set's version is the Expansion Version; the Expansion ID only in
  a file without it; the Definition Version only in a file with neither (EHR-2's reading of the June 2013 workbook, kept). The report says
  "Value-set versions in the file: eCQM Update 2026-05-14". The Direct Reference Codes refusal for a header without the measure names
  "eCQM ID (or CMS ID)". The header search (30 rows) already found rows 3 and 5; the tests now hold it there.
- **The code systems' names** (`GetCodeSystemName`): `SNOMEDCT_US`, `HCPCS Level II` and `SOPT` (also `SNOMEDCT-US`, `SNOMED-CT`) are the
  code list's SNOMEDCT, HCPCS and SOP where a row has no OID; the OID still decides first, and every row of the record has one.
- **Inline strings**: EHR-2's cell reader already read them (`t="inlineStr"`) and needs no shared-strings part; a test now holds a
  workbook without the part.
- **E-30 (the lead's decision under the owner's answer).** No sheet of the by-CMS workbook passes the 64 MB part limit (the largest
  33.1 MB), but the workbook reads about 377 MB (355.7 MB of sheets and the 20.9 MB `.xlsx` inside the `.zip`), over EHR-2's 256 MB total.
  So: (1) the importer **keeps only the manifest's value sets** as it streams (`AddRawRow`: a row of another value set is counted,
  `CountRowsOtherValueSets`, and its OID kept in a set, `CountOtherValueSets`; the measures every row names still go to the version check);
  the SVS reader the same; (2) **`MAX_TOTAL_BYTES` is 1,024 MB** (from 256 MB): room for the release to grow nearly threefold. The memory
  stays small because the total does not decide it: every part is read forward one row at a time; the one part held whole is the `.xlsx`
  inside the `.zip` (20.9 MB, within the unchanged 64 MB part limit, `MAX_FILE_BYTES`); and what is kept is the catalog's 68 value sets,
  whatever the size of the rest. The total only bounds how long a file made to inflate without end is read before it is refused. The
  per-part limit, `MAX_ROWS` (1,000,000 rows read, the whole pick 225,385) and `MAX_COLUMNS` are unchanged. A part past the limit names the
  workbook to pick: "Of VSAC's value-set workbooks, pick the one sorted by CMS ID (one sheet per measure)."
- **The direct reference codes** (`FinishDirectCodes`): only the rows of the catalog's seven at their 2027 versions are kept (the file
  lists 64 measures' rows, hospital ones among them); the others are counted (`CountRowsOtherMeasures`, "rows of other measures or versions
  (not imported)"), and the measure check still sees every measure (a CMS74v15 row warns). The check runs before the "no codes" refusal,
  so a file of another release is refused naming the release this program needs. `ResolveDirectCodes` is unchanged: its roles take the
  kept rows.
- **The code-system versions** (section 4, rewritten): each value-set row and each direct reference code names its code's version, so the
  kept codes are counted per code system and version (`EhrValueSetFile.ListCodeSystemVersionsUsed`, `EhrCodeSystemVersion.CountCodes`, the
  pick's sum, and the release record's new `ListCodeSystemVersionsUsed`; the report's line "Code-system versions the codes to import
  name"). A version the value sets name that the Code System Versions file does not list warns that the files may be of different
  releases (`CheckCodeSystemVersionsListed`; the direct reference codes are not checked so, as the record shows ActCode 9.0.0 outside the
  list). The SVS reader reads `codeSystemVersion` too.
- **The status grid** (`GetCodeSystemStatuses`): In the Release shows the versions the imported codes name, newest first, at most three
  ("2025-09, 2025-03, 2024-09 and 2 older"); `CountCodesNewerThanLoaded` counts the codes whose row names a newer version than the loaded
  table, and the table is flagged older only when that count is above 0 and the two versions compare (`CountCodesNewer`; the window says
  "Loaded, but older than the release's version: 2,992 codes of the value sets are of a newer version and may be missing from the pick
  lists"). The program can really tell only where both strings map (section 4): this program's own loaders write `VersionCur` only for CPT
  (a year); a SNOMED CT, LOINC or RxNorm table keeps the string its database had, which is compared only when it is a date or a release
  number. A CPT table of 2025 against the record's CPT 2026 (331 codes) and 2024 (3) is older by 331 codes; the 2006 version CPT also has
  in the list never flags a 2025 table. A record written before EHR-6a-R (no list of used versions) shows the Code System Versions file's
  versions and is not compared.
- **The window** (`FormCodeSystemsImport`): the licence prompt asks for the value sets sorted by CMS ID ("the Excel workbook, as VSAC gives
  it in a .zip, or the SVS XML") with the two other files, and says the two other sortings are not needed; the older-table status gives
  the count. The release label stays `CqmCatalog.RELEASE_LABEL` (the 2027 performance period, published 2026-05-14).

**For EHR-7.** Of the 14 OIDs the data-entry windows and `DiseaseDefs` still name outside the catalog (section 7), 12 are nowhere in the
2026-05-14 release; two are, under other measures: `2.16.840.1.113883.3.526.3.1008` (108 rows) and `.526.3.1259` (6 rows). With E-30 their
rows are not kept, so those lists stay empty until EHR-7 moves the windows to the catalog, as for the other twelve.
`ValueSetDbTests.Pregnancy_...` now holds that the 2014 diagnosis set (`.600.1.1623`, not in the release) is not kept.

**The local check (C)**, never committed: the x86 lane build, then `RealReleaseTests.RealRelease_TheOfficesThreeFiles_ImportSevenOfSeven_WhenTheFolderIsSet`
with `EHR_VSAC_RELEASE_DIR` naming the folder of the owner's files, in the Targeted EhrTests run (`testhost.x86`, a 32-bit process). It
reads the three files as the window does (`ParsePick`, FileStreams) and imports them through `Import` into its own scratch database
(`ehr6ar_` plus the run's id, built from `db/init/new-database.sql` and the fork migrations as `Support/ReleaseDb.cs` does, dropped after
the test; none was left on the server). Without the variable it is inconclusive. Its output, counts only:

| | Result |
|---|---|
| The pick | not refused; no pick warning; the value-set file, the Direct Reference Codes file and the Code System Versions file read as their kinds (74, 2 and 2 parts read, none skipped) |
| Rows read | value sets 224,860; Direct Reference Codes 485; Code System Versions 40 |
| Measure check | value sets: **7 of 7 measures at the 2027 versions** (73 measures in the file); Direct Reference Codes: 7 of 7 (445 rows of other measures not imported) |
| Value sets kept | **68 of 68** of the catalog, 6,225 codes (36,630 repeated rows merged; 595 other value sets with 182,005 rows not kept; none too long) |
| CMS68v16 | value sets 6 of 6, 382 codes; direct codes 1 (Cms68MedsDocumented 1) |
| CMS69v15 | value sets 22 of 22, 2,611 codes; direct codes 7 (Cms69Bmi 1, Cms69Virtual 1, listed 5) |
| CMS74v16 | value sets 10 of 10, 190 codes; direct codes 4 (listed 4) |
| CMS75v15 | value sets 10 of 10, 267 codes; direct codes 4 (listed 4) |
| CMS138v15 | value sets 32 of 32, 581 codes; direct codes 9 (Cms138QualifyingVisitCpt 4, Cms138CounselingDx 1, listed 4) |
| CMS155v15 | value sets 21 of 21, 2,357 codes; direct codes 4 (listed 4) |
| CMS165v15 | value sets 31 of 31, 5,036 codes; direct codes 11 (Cms165Systolic 1, Cms165Diastolic 1, Cms165KidneyTransplantDx 1, listed 8) |
| Roles | **8 of 8 resolved**, each with the number it takes (the three LOINC ones by their display names, E-23) |
| Versions the codes name | CDCREC 1.3 (8); SNOMEDCT 2025-09 (2,992), 2025-03 (6), 2024-09 (5), 2024-03 (1), 2018-03 (2); SOP 9.2 (125); CPT 2026 (331), 2024 (3); HCPCS 2026 (88), 2025 (1), 2024 (1); ICD10CM 2026 (2,518); CDT 2025 (18); RXNORM 2026-01 (125); LOINC 2.81 (34); ICD10PCS 2026 (6); ActCode 9.0.0 (1) |
| Version check | every version of the value sets listed in the Code System Versions file (no warning). With made-up loaded versions in the scratch database: CPT `2025` compared, 331 codes newer; SNOMEDCT `2025-03-01` compared, 2,992 newer, In the Release "2025-09, 2025-03, 2024-09 and 2 older"; LOINC `2.81` compared, 0 newer; CDCREC, CDT, HCPCS, ICD10CM, RXNORM, SOP not compared (nothing loaded), ActCode and ICD10PCS without a table |
| Report notes | "Value-set versions in the file: eCQM Update 2026-05-14"; no warning in any file |
| The two other sortings | each refused alone by the part limit (the category workbook's sheet 5, the unique workbook's sheet 1), naming the by-CMS workbook |
| Time | read and checked 6.2 s and 6.8 s (two runs), import 0.47 s and 0.49 s |
| Memory (second run) | managed heap 16 MB before, peak 76 MB while reading and importing (the `.xlsx` copy and garbage not yet collected), 20 MB after with the pick held; private bytes 94 MB before, peak 144 MB |

**Tests** (`tests/EhrTests`; no project file changed, so no ProjectFileTests):

- `Support/ValueSetFixtures.cs`: `ArrayHeadersDefault` (the by-CMS workbook's 18 columns), `ArrayHeadersDirectCodes` (13) and
  `ArrayHeadersCodeSystemVersions` (3) as recorded, EHR-6a's provisional one kept as `ArrayHeadersCodeSystemVersionsProvisional`; the rows
  above each header (row 1 empty, the paragraph, the title and the publication date: made up); `MakeByCmsWorkbook` (a sheet per measure,
  named by its CMS ID, each row under one measure); inline strings; `ZipAsVsacGivesIt`; `MakeRelease`'s three files in `.zip`s named
  `..._TEST.xlsx.zip`; made-up `CMS9001v1` and `CMS9002v4` rows in the Direct Reference Codes file (one under `SNOMEDCT_US`), not imported;
  the made-up code-system version of each code system on the value-set and direct-code rows (and in SVS); a Definition Version and an
  Expansion ID of their own; `MakeWorkbook(false,...)` without a shared-strings part. Every code is still a made-up TEST code.
- New in `DirectCodeTests`: `Record_ThePickOfTheThreeFilesByTheirNames_IsAccepted_SevenOfSeven` (the three names of the record),
  `Record_TheHeaderRows_AreFoundBelowTheRowsAboveThem`, `Record_TheValueSetsVersion_IsTheExpansionVersion_NeverTheDefinitionVersion`,
  `Record_CodeSystemNames_MapAcrossTheFilesTwoForms`, `Record_TheDirectCodesMeasure_IsItsEcqmId_WithTheEcqmVersionWhereTheIdHasNone`,
  `Record_AWorkbookOfInlineStrings_WithoutASharedStringsPart_IsRead`, `Record_AVersionTheValueSetsName_ThatTheCodeSystemVersionsFileDoesNotList_Warns`,
  `CountCodesNewerThanLoaded_CountsTheCodesOfNewerVersions_WhereTheProgramCanTell`; in `ValueSetParseTests`
  `Zip_PartsOver256MbTogether_AreRead_WithinTheTotalOfE30`; `RealReleaseTests` (the local check).
- Changed: the counts of the direct codes (the other measures' rows), the version check (`Status_ALoadedTableOlderThanTheRelease_...`: a
  SNOMED CT code of 2024-03 beside those of 2025-09, the count of newer codes, a table as new as the newest), the value sets of other
  measures counted and not kept (`Svs_WithoutGroups_...`, `Workbook_AValueSetListedUnder64Measures_...`, `CodeSystems_WithoutATable_...`,
  `IsInDb_...`, `Pregnancy_...`, `LoadTime_...` with 66,000 rows read and 60,000 kept), the total limit at 1,024 MB
  (`Zip_PartsThatTogetherInflatePastTheTotalLimit_...` with 17 parts of 62 MB, `Limits_AreWhatA32BitProcessHolds`), the part-limit refusal's
  hint.
