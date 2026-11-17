# S3b-MAIN: main merged into net10/switch after EHR-6a and EHR-7 (2026-11-16)

Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.

The branch takes merges from main (net10-plan, stage 3b). Main had 46 commits the branch lacked: EHR-6a (the 2027 catalog and import,
with EHR-6a-R's record of E-20) and EHR-7 (the 2027 data-entry windows), 41 files.

## The merge

- Branch `net10/mainmerge-1` from `net10/switch` (`deff7849c`), `git merge main` committed as `0b907fc9f`.
- One conflict, in `tests/EhrTests/ValueSetParseTests.cs` (`Zip_AnEntryThatInflatesPastThePartLimit_...`): the .NET 10 side's pinned
  refusal for the understated entry (S3b-5's accepted .NET 10 message) stays, with its list check moved before the branch; main's E-20
  assertion ("pick the one sorted by CMS ID") comes in on the too-large path, where `GetPartLimitText` adds that hint.
- No .NET 10 API difference in the new code: the full filter builds x64 with 0 errors as merged.

## Decision 24 for the new code

The new EHR code writes no `double` or `float` as text and has no fractional time addition (`CqmCatalog.cs` and `CqmDataEntry.cs` hold none;
the vital-sign changes are age logic; `EhrCodes.cs` writes ints with the invariant culture). FormatParityTests' guard, which scans every
shipped project, reports 0 unrouted sites (21/21).

## A test fixed on the way (not from this merge)

`LibraryBindingTests.ReleaseListTests.Installer_InstallsTheStagingFolderAsAWhole` (S3b-5) knew only the 64-bit TWAINDSM's `[Files]` line
(`{#TwainDsmFile}`). S3b-8 added the 32-bit one (`setup|x86\TWAINDSM.dll`, `{#TwainDsm32File}`, `{sys}` with the `32bit` flag), so the test
failed on `net10/switch` since S3b-8. The test now maps that setup file to its define and also requires the `32bit` flag for it.

## Runs (x64, `-RequireBuildOutput`)

`mainmerge3b-lead.txt`: EhrTests 328 passed, 2 skipped (the real-file and package checks without their folders, as on main),
FormatParityTests 21/21, ProjectFileTests 167 + 1 skipped, AttributionTests 50/50, MiddleTierMockTests 151/151, ReleaseFlowTests 236 + 7
skipped (no Release build in this worktree), UnitTests 1728 + 3 skipped, NoHqTests 115/115; LibraryBindingTests 17/18 there and 18/18 after
the fix above (`mainmerge3b-lbt.txt`).
