# INITSQL-COPYRIGHT: no third-party copyrighted content in the new database file

Package INITSQL-COPYRIGHT, on branch `feature/initsql-copyright` from `main`. The owner asked on 2026-11-26: "db/init/new-database.sql
shouldn't have any copyright materials" ([owner-questions.md](../decisions/owner-questions.md), item 15). Data only: no table, column or key
changes, no fork migration step.

## Audit

The file was loaded into a scratch database and every text column of every table with rows was checked (values over 60 characters read,
short labels listed per table), and the file was searched for notice words (copyright, licence, trademark signs, the names of the code
system owners).

| Kind | Table and rows | Decision |
|---|---|---|
| ICD-9-CM code titles (NCHS and CMS, built on the World Health Organization's ICD-9) | `icd9`, 15,651 rows from the seed dump | **Removed**: the table stays, empty |
| Vendor logos (22x22 pictures of CareCredit, Pearl, Jazz Classic and DentalTek) | `program.ButtonImage`, 6 rows (`DentalTekSmartOfficePhone`, `CareCredit`, `JazzClassicCapture`, `JazzClassicExamView`, `JazzClassicPatientUpdate`, `Pearl`) | **Removed**: blank, the button shows its text |
| ADA CDT descriptors | `procedurecode`, 577 CDT-style codes | Already neutral since INIT-SQL (`Procedure Dxxxx`, abbreviation the code, no layman term or notes); the generator now enforces it |
| Other medical code systems (CPT, SNOMED CT, LOINC, RxNorm, CVX, HCPCS, ICD-10-CM, UCUM, CDC race and ethnicity, payer typology) | `cpt`, `snomed`, `loinc`, `rxnorm`, `cvx`, `hcpcs`, `icd10`, `ucum`, `cdcrec`, `sop`: no rows | Kept empty; the generator empties them if a row ever appears |
| Code system names and HL7 OIDs | `codesystem`, 13 rows (names, OIDs, no versions, no content) | Kept: identifiers, not content |
| Procedure codes as identifiers (`D0120` and the rest) | `procedurecode`, codegroup, autocode, procedure button and preference code lists | Kept, as INIT-SQL and B-PROC decided (codes without descriptors) |
| Claim form layouts (ADA 2006, 2012, 2024, UB04, 1500) | `claimform` 6, `claimformitem` 1,194 | Kept: field positions and the names of background image files that are not in the file (owner question INIT-SQL 2 still open) |
| Payer, clearinghouse and network lists; X12 claim filing codes | `electid` 718, `clearinghouse` 22, `canadiannetwork` 10, `insfilingcode` 23 | Kept: names and identifiers (facts), with Open Dental's short labels |

Open Dental Software's own texts, GPL-licensed with the program, are kept. The notable ones:

- message templates in `preference` (recall, confirmation, reminder, Web Sched, ASAP, statements, payment links), the treatment plan note
  ("THIS IS AN ESTIMATE ONLY"), `PayPlanTermsAndConditions`, `EmailDisclaimerTemplate`, and the HTML and CSS of `EmailMasterTemplate`
  and of the wiki's `_Master` page (`wikipage`, 2 rows);
- `apptreminderrule` templates (3 rules), definitions (286 rows of labels, three with a sentence: "Insurance Over 90" billing note and two
  eClipboard photo prompts), quick paste notes, user queries (36), report rows, toolbar items, alert categories;
- program link setup notes (`program.Note`, 24 rows of one or two sentences about each vendor's program) and their names and web addresses
  (`ProgDesc`, for example "CareCredit from https://www.carecredit.com/");
- the 21 recorded voice sounds of the signal buttons (`sigelementdef.Sound`, WAV, "Doctor", "Assistant", "To Op 1" and the others). They
  come with Open Dental's seed dump; who recorded them is not documented.

No row holds a copyright, licence or trademark notice; the only "copyright" in the file is the column name `ExternalCopyrightNotice`.

## What changed

- `tools/DevDbTool/InitSqlScrub.cs`: a new step, `RemoveThirdPartyContent`, after the emptied tables: empties the medical code system tables
  (`ArrayCodeSystemTables`), blanks every `program.ButtonImage` and names every CDT-style code by its code only (a no-op today; a convert
  script or a later seed cannot bring a descriptor back). `CheckDatabase` stops the export when any of the three is left.
- `tools/DevDbTool/InitSqlWriter.cs`: the header says the file holds no third-party copyrighted content.
- `db/init/new-database.sql` regenerated: the `icd9` INSERT statements (15,651 rows) are gone, six `program` rows have a blank
  `ButtonImage`, and the header has the new sentence. No other row changed.
- `db/README.md`: what is not in the file, what the generator removes, what the tests check.
- Development and test databases are not changed: the seed dump keeps its ICD-9-CM list and the program rows keep their pictures, since
  tests rely on the seed as it is.

## What the program does with it

- ICD-9: the list (Setup, ICD-9 codes) opens empty; codes can be added by hand. ICD-9-CM has not been used for US claims since 2015, and
  the fork has no ICD-9 code system import (the download from Open Dental's servers was removed). `EhrCodes` counts the code system as not
  loaded, as it does for the other empty code systems.
- Program links: `ProgramL` adds a toolbar button without a picture when `ButtonImage` is blank, as for the 124 links that had none. The
  picture can be set in the link's setup window.

## Tests

- `tests/InitSqlTests/InitSqlFileTests.cs`: `File_HasNoCodeSystemTitles` (each code system table is created and has no INSERT),
  `File_ProgramLinksHaveNoVendorLogo` (every program row's `ButtonImage` is blank), `File_RowsHaveNoThirdPartyNotice` (no row holds "Current
  Dental Terminology", "American Dental Association", "Current Procedural Terminology", "American Medical Association", "SNOMED
  International", "IHTSDO", "Regenstrief", "World Health Organization", "All rights reserved", "Copyright", the copyright, registered or
  trademark sign). The existing `File_ProcedureCodesAreNamedByCodeOnly` and `Seed_ProcedureCodesAreNamedByCodeOnly` keep guarding the CDT
  descriptors.
- `tests/InitSqlTests/InitSqlDatabaseTests.cs`: `Rows_NoCodeSystemTitlesAndNoVendorLogos` (the loaded file: each code system table there
  and empty, no program picture, `ICD9s.GetAll` reads the empty list).
- `tests/ForkMigrationTests/InitSqlRegenerationTests.cs` compares the generator's output with the committed file, so the scrub and the file
  stay together.
- No test expected the removed rows.

## Owner questions

**Answered by the owner on 2026-11-26:** "1-3, no rewrite" (sounds kept, texts kept, no ICD-9 import); CDT-style codes kept with neutral names
(they come from Open Dental's public seed dump, not from any office database); "make the password blank": the generator now blanks every
program link password property (3 rows; test `File_ProgramLinkPasswordsAreBlank`).

1. The 21 signal button sounds (recorded voices) come with Open Dental's seed dump and are distributed with its GPL source, but who recorded
   them is not documented. Keep them, or replace them with sounds of our own (or none: the signal buttons then stay silent until the office
   adds its own)?
2. Open Dental's default message templates, notes and labels (listed above) are kept as GPL-licensed text of the program. Should any of them
   be rewritten in our own words anyway (for example the treatment plan note or the payment plan terms)?
3. The ICD-9-CM list is removed. Should the program get an import for a code file the office supplies (as for CPT and CDT), or is adding
   codes by hand enough for a code system that is no longer used for claims?
