# Port note: NOHASH, tamper detection removed

Design: [specs/remove-tamper-detection.md](../specs/remove-tamper-detection.md). Owner decision of 2026-11-24 (the roadmap item "Remove
tamper detection"); scope and review chosen by the owner on 2026-11-26 ([owner-questions.md](../decisions/owner-questions.md)).

## What changed

- **The hash code is gone.** `HashFields`, the `Is...HashValid` checks, `SetHashForUpdate`, `RehashAfterDirectUpdate` and every assignment of
  a computed hash, in `Appointments`, `Claims`, `ClaimProcs`, `Etranss`, `InsSubs`, `InsVerifies`, `InsVerifyHists`, `Patients`, `PayPlans`,
  `PaySplits`, `Payments` and `DatabaseMaintenances` (the claimproc fixes, the conversion payment tool, `SecurityHashRehashPatientTable`).
  The database reads that only fed a rehash went with it (the old claim before a claim update, the paysplits after a patient merge, the
  subscribers before a plan merge, and the like). `OpenPmsDentalBusiness/Misc/SecurityHash.cs`, `CodeBase/Crypto/TamperHash.cs` and
  `SaltedHash.cs` are deleted; `HmacSha256` stays for `SecretBox`. The one live legacy conversion call (`ResetPatientHashing`) is removed.
- **The columns stay.** The ten `SecurityHash` columns and the table types' fields are Open Dental's schema. A stored value is kept as it
  is; a new object writes `''`. `InsVerifies` copies the stored value when it resets a row from a new object, so the reset does not blank
  it.
- **The warnings are gone.** The WinForms and WPF `WarningIntegrity` triangles, `DatabaseIntegrities`, the module popup
  (`FrmDatabaseIntegrity`), the control on eight forms and the two test-all-controls windows, `UiDialogPaint`'s warning fill and the
  WpfConverter case. The forms keep their layout: the triangle sat in a corner and nothing moves into its place.
- **The tool is gone.** Database Maintenance, Tools, Security Hashes.
- **Adoption no longer rehashes.** `AdoptionValidation adopt` and the rehearsal run no rehash and write no rehash log entry; instead they
  count every table's `SecurityHash` values with a checksum before and after, and fail if adoption changed one (rehearsal checks H1 and I3).
  `allowed-differences.adoption.json` no longer allows a `SecurityHash` change. The runbook's step 5 is a stub, so the later step numbers
  stay.

## Tests

- Removed with their code: `tests/RowHashTests` (out of the solution and the solution filter), `CryptoTests/TamperHashTests.cs`, the
  triangle's three picture tests in `UiRefreshTests.DialogStateTests`.
- New: `NoHqTests.TamperDetectionRemovedTests` (by reflection: the hash types and methods are gone, the fields stay, no triangle or popup
  type is left). `CdtRetiredTests` now asserts that Database Maintenance has no Security Hashes tool.
- Changed to assert that a value is kept: `P262ToolsTests` (the conversion payment tool keeps every split's value; the redundant
  verification fix copies it into the history), `PayPlanTests`, `BPAYTests`, `M2INSVTests`, `SyntheticData.Tests` (a changed value is an
  unexpected difference), `AdoptionValidation.Tests`.
- The UI records lose the triangle's rows: `tests/UiRefreshTests/Data` (coverage, colour literals, layout snapshots, grid scope and hand
  rules), `tests/WindowChecksTests/UiApproved/baseline.tsv`, `tools/UiFit` and `docs/port-notes/UI-13-inventory.tsv`;
  `tools/project-baseline` loses the deleted files and the RowHashTests project.

## Review

The design was reviewed by a separate agent before implementation; its findings and how they were applied are in the design's Review
section.
