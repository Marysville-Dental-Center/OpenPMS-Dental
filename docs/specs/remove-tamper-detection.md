# Remove tamper detection

Owner decision of 2026-11-24 (the roadmap entry "Remove tamper detection"): remove Open Dental's "Database Integrity" row hashes, the
computing of the `SecurityHash` columns, the warning triangles, the Security Hashes tool in Database Maintenance and their tests. The
separate Database Integrity Check tool (table check and repair) stays. Package **NOHASH**.

## What Open Dental does, and what this program does today

Open Dental 24.1 to 25.2 added a `SecurityHash varchar(255) NOT NULL` column to ten tables (`appointment`, `claim`, `claimproc`,
`histappointment`, `inssub`, `insverify`, `insverifyhist`, `patient`, `payplan`, `paysplit`; `histappointment` copies the appointment's
value and was never checked). Each save writes a keyed hash of some of the row's fields. When a form
opens a row whose hash does not match, a triangle (`WarningIntegrity`) shows in the form, and a module can show a popup
(`DatabaseIntegrities`). Open Dental's key is proprietary, so this program computes its own hash (`CodeBase/Crypto/TamperHash.cs`,
`SaltedHash.cs`, format `osh1`), treats Open Dental's hashes as "legacy" (never reported), and rehashes after adoption
(`SecurityHash.RehashNow`: Database Maintenance, Tools, Security Hashes; runbook step 5). The 25.1 DBM method
`SecurityHashRehashPatientTable` rehashes the whole patient table.

What the hash protects is weak here: the key is in the source code, so anyone who can write SQL can compute a valid hash. It costs a
second `UPDATE` on every patient insert, a hash on every save of nine tables, a rehash step in adoption, and false warnings after edits
made outside the program.

## The change

**Kept (schema rule):** the ten `SecurityHash` columns stay, with their definitions, so the schema still matches Open Dental's and Open
Dental's next schema applies on top. The program neither computes nor checks them any more:

- A new row gets the value its object holds: `''` for a new object. The columns have no default, and the CRUD insert writes the field
  explicitly, so an object copied from a stored row (`Copy()` then `Insert`) carries that row's value into the new row. That is harmless:
  nothing reads the value.
- An updated row keeps its stored value: the CRUD update writes the field only when it changed, and nothing changes it. Where the program
  updates a row from a freshly built object (`InsVerifies` resetting a row for a key), it copies the stored value first.
- The `Patient`, `Appointment`, ... classes keep their `SecurityHash` field (the CRUD layer and the Middle Tier serializer use it), with a
  summary that says the program no longer uses it.

**Removed:**

| Part | Where |
|---|---|
| Hash computing and checking | `HashFields`, `Is...HashValid`, `ArePaySplitHashesValid` and every `x.SecurityHash=HashFields(x)` in `Appointments`, `Claims`, `ClaimProcs`, `InsSubs`, `InsVerifies`, `InsVerifyHists`, `Patients`, `PayPlans`, `PaySplits`, `Payments`, `Etranss`, `DatabaseMaintenances` (the claimproc fixes), `DatabaseMaintenances.ConversionPayments` |
| The hashing runner | `OpenPmsDentalBusiness/Misc/SecurityHash.cs` (UpdateHashing, RehashNow, RehashPatientTable, the workers) |
| The hash function | `CodeBase/Crypto/TamperHash.cs` and `SaltedHash.cs` (nothing else uses them); `HmacSha256` stays, because `SecretBox` uses it |
| The triangles | `OpenPmsDental/UI/DatabaseIntegrity/WarningIntegrity*` (WinForms) and `WpfControlsOD/UI/DatabaseIntegrity/` (`WarningIntegrity.xaml`, `DatabaseIntegrity`, `DatabaseIntegrities`); the control on FormApptEdit, FormClaimEdit, FormClaimProc, FormInsVerificationList, FormPatientEdit, FormPayPlan, FormPayPlanDynamic and FormPayment; the module popups (`WpfControlsOD/Frms/FrmDatabaseIntegrity`) in ControlAccount, ControlAppt, ControlChart, ControlFamily, ControlImages and ControlTreat; the control in the test-all-controls windows (`UnitTests/UI Testing/FormTestAllControls`, `WpfControlsOD/Frms/FrmTestAllControls`); the `OpenPmsDental.csproj` designer entry; the WpfConverter case; `UiDialogPaint`'s warning fill; the layout, coverage, colour-literal and approved-picture records of the triangle (`tests/UiRefreshTests/Data`, `tests/WindowChecksTests/UiApproved/baseline.tsv`, `tools/UiFit`, `docs/port-notes/UI-13-inventory.tsv`, `tools/project-baseline`) |
| The tool | Database Maintenance, Tools, "Security Hashes" (FormDatabaseMaintenance) |
| The DBM method | `SecurityHashRehashPatientTable`. Its `databasemaintenance` row (Open Dental's, if present) stays; Database Maintenance lists only the methods the program has, so the row is ignored |
| Adoption's rehash | `HeadlessAdoption` step "Rehash" and its security log entry, `Rehearsal`'s rehash checks (H1 and I3 now check that every value is unchanged), `AdoptionRowCheck`'s rehash log entry (one security log entry per adoption, none for a second pass), `BusinessLayer`'s hash window, `allowed-differences.adoption.json`'s `SecurityHash` rule and the rehash in its `securitylog` reason, the runbook's step 5 and its checks. The runbook keeps the number 5 as "(No longer needed)", so the many references to step 6.1 stay right |
| Tests | `tests/RowHashTests` (the whole assembly), `tests/CryptoTests/TamperHashTests.cs`, the triangle's picture tests in `UiRefreshTests` (`DialogStateTests`), and the hash assertions in other suites: `P262ToolsTests` (conversion payment dates, redundant verifications), `PayPlanTests`, `BPAYTests`, `M2INSVTests`, `AdoptionValidation.Tests` (`RehearsalIntegrationTests`, `Migration26_1Tests`), `SyntheticData.Tests`. They now assert that the value is kept. `NoHqTests.CdtRetiredTests` asserted the Security Hashes tool exists; it now asserts it does not |
| Preference | `DatabaseIntegritiesWhiteList` stays in `PrefName` (it is Open Dental's row; removing an enum value would change no data, but the name list is checked against Open Dental's), unused |

**Not part of this change:** the security log's own hash (`securityloghash`, Audit Trail), which is a different feature; the Database
Integrity Check tool (`DatabaseIntegrityCheck/`); encrypted secrets (`Secrets`, `SecretBox`).

**Legacy conversion code** (`ConvertDatabases7.cs`, `ConvertDatabases8.cs`): its `SecurityHash.UpdateHashing()` calls are already commented
out; `ResetPatientHashing()` is the one live call, and goes. The conversion steps that add the columns stay.

## Effect on adoption and on data

- Adoption no longer rehashes. An adopted database keeps Open Dental's hash values as they are, and they are never read. Adoption's row
  check then expects no `SecurityHash` change at all (the allowed differences lose those entries), and the runbook loses step 5.
- A database that release 0.1 rehashed keeps its `osh1` values; nothing reads them.
- Nothing is migrated: there is no fork migration step and no schema change.

## Tests

- The removed tests go with their code (`RowHashTests`, `TamperHashTests`).
- New `NoHqTests.TamperDetectionRemovedTests`, by reflection over the built assemblies (a source scan would also hit the fork migrations,
  the schema code and the test fixtures, where `SecurityHash` legitimately stays): `TamperHash`, `SaltedHash` and `Misc.SecurityHash` are
  gone and `HmacSha256` stays; the ten data-interface classes have no `HashFields`, `Is...HashValid`, `SetHashForUpdate` or
  `RehashAfterDirectUpdate`; `DatabaseMaintenances` has no `SecurityHashRehashPatientTable`; the ten table types keep their `SecurityHash`
  field; neither program assembly has a `WarningIntegrity`, `DatabaseIntegrities` or `FrmDatabaseIntegrity` type.
- Database tests that save rows (`PayPlanTests`, `BPAYTests`, `M2INSVTests`, `P262ToolsTests`): a stored value is kept unchanged through
  updates, history copies and the conversion-payment tool.
- Adoption suites (`AdoptionValidation.Tests`, `ReleaseFlowTests`, `SyntheticData.Tests`) pass with the rehash step removed.
- UI suites (`UiRefreshTests`, `WindowChecksTests`): the forms that lose the triangle keep their layout (the triangle sat in a corner
  and nothing moves into its place).

## Documents

Port note `docs/port-notes/NOHASH.md`; the features-25.1/25.2/25.4, schema-changes and port-backlog items for the hashes marked
removed; `CodeBase/Crypto/README.md`; the adoption runbook (step 5 a stub, its checks updated); `docs/develop/release-process.md`,
`repository-layout.md` and `testing.md`; the AdoptionValidation and SyntheticData READMEs; the roadmap entry moves to "Done". Port notes
and reports of earlier packages are records of what those packages did and stay as written.

## Review

Reviewed on 2026-11-26 by a separate review agent before implementation. Its findings, all applied above: ten tables, not nine
(`histappointment`); a new row gets its object's value, not a column default; the tests, the adoption row check and the UI records the
first draft missed; the reflection-based removal test; `HmacSha256` is used by `SecretBox`; the runbook keeps the number of step 5.

## Open points

None for the owner: the decision names the parts. The one judgment call is keeping the columns, which the schema rule requires.
