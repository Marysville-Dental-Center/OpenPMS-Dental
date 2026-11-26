# Testing

Open PMS - Dental is tested far more than the code it started from. At the fork point (Open Dental 24.3.63) Open Dental's public source
had about 2,000 test methods, nearly all in `UnitTests`. Today there are 128 more test projects under `tests/`,
plus `NoHqTests`, about 269,000 lines and about 11,500 test methods in all. Open Dental's own `UnitTests` are kept and run with them.

Counts are `[TestMethod]`/`[DataTestMethod]` attributes in the repository on 2026-11-24; data-driven tests run more cases than that.

## What the tests cover

| Area | Test projects | About |
|---|---|---|
| Open Dental's own unit tests | `UnitTests` (with `UnitTestsCore`) | 1,700 test methods, kept from the fork point |
| Features re-implemented from Open Dental 24.4 to 26.1 | `B*Tests` (appointments, claims, payments, perio, treatment plans, ...), `M2*Tests` (estimates, insurance plans, eClaims, sheets, ...), `M201Tests`, `M202Tests` | 2,200 |
| The chosen Open Dental 26.2 changes | `P262*Tests` (appointments, billing, claims, Glidewell lab cases, program links, perio, reports, tools, schema) | 430 |
| EHR, rebuilt in place of Open Dental's closed `EHR.dll` | `EhrTests` | 330 |
| Imaging and capture | `ImagingCaptureTests`, `ImagingDeviceManagerTests`, `ImagingTests`, `Helper32Tests`, `DicomReaderTests`, `CLAIMDCMTests`, `M2IMGTests` | 1,900 |
| Vendor links (bridges) | `VendorDirectTests`, `VBRIDGE*`, `VDXC2`, `VEDS`, `VGP`, `VPEARL*` | 1,350 |
| The tooth chart | `ToothChartImageTests` (pictures compared with approved ones), `ToothChartHostTests` | 300 |
| Independence from Open Dental Software, Inc. | `NoHqTests`: nothing calls Open Dental's servers, no registration keys, the brand texts | 140 |
| Security | `DbConnectionSecurityTests`, `DbGrantsTests`, `CryptoTests`, `SshHostKeyTests`, `RowHashTests`, `PrintGuardTests` | 380 |
| The database | `ForkMigrationTests`, `InitSqlTests`, `P262SchemaTests`, `MaintenanceTests`, `InventoryTests` | 300 |
| The look and the layout | `UiRefreshTests` (every window on the layout grid, fonts and colours from the theme, calendars), `WindowChecksTests` (the program started and its windows checked) | 500 |
| Build, libraries and release | `ReleaseFlowTests`, `ProjectFileTests`, `LibraryUpdateTests`, `LibraryBindingTests`, `NativeFirstTests`, `AttributionTests` | 700 |

## How they run

- **Synthetic data only.** Tests that need a database build a scratch database on the local MariaDB (`127.0.0.1:3306`) from the
  committed init file and the migrations, fill it with fictional data, and drop it at the end. No test reads office data, and none
  connects to another server.
- **No vendor or Open Dental servers.** `NoHqTests` runs in every mode and fails if any code path would call Open Dental Software's
  servers.
- **Pictures compared, not just code.** The tooth chart and the windows are drawn and compared with pictures the owner approved
  (`tests/WindowChecksTests/UiApproved/`).
- **Every change is checked in full before it merges.** Each change is a small package: its own tests, an independent review, and then
  a full build and run of the 19 main suites on `main` before it is pushed.

Run them with `tools/run-tests.ps1` (MariaDB 10.11 on `127.0.0.1:3306`, user `root`, blank password; see the [README](../README.md)):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Core -Platform x64 -RequireBuildOutput
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Targeted -Include ImagingCaptureTests -Platform x64
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Full -Platform x64
```

`Targeted` runs `NoHqTests` plus the named projects, `Core` runs everything but the slow migration and adoption projects, and `Full`
runs everything.

## Documentation links

`python -I tools/check_doc_links.py` checks every link of the Markdown files and of the project site (`site/index.html`): a relative link
must name a file of the repository and a `#fragment` a heading of the file it points to. The Docs workflow (`.github/workflows/docs.yml`)
runs it on every push to `main` that changes a document. `--external` also fetches the web addresses (slow; not part of the routine check).
