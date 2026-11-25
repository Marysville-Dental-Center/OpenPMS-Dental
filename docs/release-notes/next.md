# Open PMS - Dental, next version: release notes (unreleased)

Open PMS - Dental is a fork of Open Dental (c) Open Dental Software, Inc. and contributors, licensed under the GNU GPL v2. It is not affiliated with or endorsed by Open Dental Software, Inc.

This file collects the notes of the version after 0.1 until it is released; the release renames it to `v<version>.md`. The notes of 0.1
are [v0.1.md](v0.1.md).

## Adoption

- **An Open Dental database labelled 25.3.47.0 / 25.3.54.0 can be adopted.** Open Dental changes `DataBaseVersion` only when a convert step
  runs, so a database on Open Dental 25.3.54 reads `DataBaseVersion` 25.3.47.0 and `ProgramVersion` 25.3.54.0 while its schema is that of
  25.3.48. Adoption now accepts exactly that pair, and only when the schema equals the Open Dental 25.3.48 reference exactly (the office's
  `patient._portal_active_email` aside); otherwise it refuses and names the differences. It is adopted as 25.3.48, and the adoption record
  (`ForkAdoptedFrom`), the adoption log and the security log keep the real labels. 24.3.63 and 25.3.48 databases are adopted as before.
- The adoption tools follow the same rule: `AdoptionValidation migration-preview` says in its first line whether the versions are adoptable
  (with the schema check), `adopt` accepts the pair as its `--original`, and `startup-check` also models an Open Dental 25.3.54 client.
  `SyntheticData generate` and the rehearsal (`Invoke-AdoptionRehearsal.ps1 -VersionLabels office`) can label a synthetic database the same
  way. Runbook step 0.2 now expects 25.3.47.0 / 25.3.54.0.
