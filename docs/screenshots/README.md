# Screenshots

Pictures of the program for the [README](../../README.md), [How Open PMS - Dental differs from Open Dental](../differences.md) and the
project site (`site/screenshots/` holds the ones the site shows). Every picture shows fictional data only: made-up patients, providers,
staff and an insurance carrier, phone numbers in the 555-01xx range kept for fiction and e-mail addresses at example.com.

| Picture | What it shows | Made by |
|---|---|---|
| `appointments.png` | The Appointments module: a day with three operatories | DocScreenshots |
| `family.png` | The Family module: a family of three, recall and a dental plan | DocScreenshots |
| `treatment-plan.png` | The Treatment Plan module with insurance estimates | DocScreenshots |
| `account.png` | The Account module: a patient's charges and payments, the family's balances | DocScreenshots |
| `manage.png` | The Manage module: the time clock and messaging | DocScreenshots |
| `dialog-patient-edit.png` | The Edit Patient Information window | DocScreenshots |
| `dialog-appointment-edit.png` | The Edit Appointment window with its procedures and fees | DocScreenshots |
| `tooth-chart.png` | The 3D tooth chart (WPF) with a crown, a filling, a sealant, an extraction and a root canal | ToothChartImageTests, gallery G5 of TC-6 |
| `perio-chart.png` | The perio chart of a full exam | ToothChartImageTests, gallery G5 of TC-6 |
| `camera-capture.png` | Intraoral camera capture with a synthetic camera picture | The imaging gallery G-IMG10 |

## How they are made

**DocScreenshots** is the opt-in test `tests/UiRefreshTests/DocScreenshots.cs`. It builds a scratch database `docs_screens_<run>` on the
local MariaDB (127.0.0.1:3306 only) from `db/init/new-database.sql` and the fork migrations, fills it with its fictional practice, draws
the module controls (with the module bar and the main toolbar, as the main window lays them out) and two dialogs off screen at 100%, as
the UI galleries draw windows, and drops the database. Nothing is shown. Run it after a build of `UiRefreshTests`:

```powershell
$env:OPMS_UI_DOC_SCREENSHOTS = "$env:TEMP\doc-screenshots"
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Targeted -Include UiRefreshTests -Platform x64
```

Without `OPMS_UI_DOC_SCREENSHOTS` the test is skipped. Its pictures are drawn before the other UiRefreshTests windows only by chance of order, and it leaves the program caches filled from the scratch database: keep the pictures of that run for these docs only, not for galleries. `OPMS_UI_DOC_SCREENSHOTS_KEEP_DB=1` keeps the database for a look afterwards.
The main window's title bar and menu are not drawn. The Chart and Imaging modules draw their charts and images in WPF, which a session
without a desktop does not draw, so their pictures come from the owner-approved galleries of the tooth chart (TC-6) and imaging (G-IMG10),
which are synthetic by construction.

The Account module (`account.png`) and the Edit Appointment window (`dialog-appointment-edit.png`) were held back until UI-COLS
(docs/port-notes/UI-COLS.md) widened the grid columns the new font had made too narrow (the Account grid's dates wrapped, Edit
Appointment's Fee was cut). DocScreenshots draws the account grid and Edit Appointment's patient grid from their first row.

The pictures are lossless PNGs, each under 400 KB. Before a picture is committed, someone looks at it: fictional names only, no real
person, practice, path or host, nothing blank or cut off.
