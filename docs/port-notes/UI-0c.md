# Port note: UI-0c (harness group U of the UI refresh)

Package UI-0c of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-0c row, sections 5.4, 8 and
9), branch `ui/ui0c-1` from `main` (`117b0edd0`, UI-0a and UI-0b merged). Tools and tests only: no program code, no project of the program, no
schema change, no change of look or behaviour. Nothing was downloaded; no package was added.

Group U is a group of the window checks of [WINCHK-HEADLESS.md](WINCHK-HEADLESS.md): opt-in, on a `winchk_` scratch copy of the synthetic
dump, in the program's own process (the in-process session of WINCHK-5), no input sent, no UAC prompt answered.

## 1. Summary

| File | What |
|---|---|
| `tests/WindowChecksTests/UiGallery/windows.tsv` | group U's rows: each window, how the program opens it, the tables it could write |
| `tests/WindowChecksTests/UTests.cs` | the test side: the rows run in in-process sessions, the windows the dump cannot fill recorded SKIPPED |
| `tests/WindowChecksTests/InProc/InProcChecks.U.cs` | the session side: opening, walking, printing and closing each row's window; the main window; the dashboard |
| `tests/WindowChecksTests/Harness/UiWindows.cs` | the reader of windows.tsv |
| `tests/WindowChecksTests/Harness/UiWalker.cs` | the walker (section 3) |
| `tests/WindowChecksTests/Harness/UiApprovalCheck.cs` | the check U0 (section 4) |
| `tests/WindowChecksTests/UiHarnessTests.cs` | the self-tests (section 6) |
| `tests/WindowChecksTests/WindowChecksTests.csproj` | links UI-0a's `tools/UiGallery/UiPictureMask.cs` and UI-0b's `tests/UiRefreshTests/Shared/TextFit*.cs` (no copies) |
| `tests/UiRefreshTests/Shared/TextFitMeasurer.cs` | the free-space rule and the "fixable" verdict moved here from `Scan/TierOneScan.cs`, so both tiers use one copy |
| `tools/ui-gallery-masks.tsv` | the date panel row of group U's main window in the Appointments module |
| `InProcChecks.cs`, `WinChkFixture.cs`, `WinChkEnv.cs` | the dispatch of U ids to windows.tsv's rows; U0 at the end of a run with group U; the group list |

- 60 common windows the other groups do not reach, 8 more the synthetic dump cannot fill (listed with the reason), the main window at
  1366x768 and 1920x1080 with each of the seven modules (with and without a patient at 1366x768, with patient #1 at 1920x1080: 21 pictures),
  and the patient dashboard with a widget: 90 rows, 82 pictures.
- Group U was **not run** in this package: `query user` showed the owner's session disconnected (`Disc`), and the group needs the owner's
  connected, unlocked session (WPF draws nothing in a locked or disconnected one; WINCHK-3). The lead runs it with the before set (section 5).

## 2. The windows and how each is opened

Each row of `windows.tsv` is one check (`U<n>`, `UM<n>`, `UD<n>`) and one picture (`<id>-<name>.png`, the name the galleries and
`approved.tsv` use; ids and names are kept once given). A check:

1. selects patient #1 when the row says so (as the patient selection does: `GlobalFormOpenDental.PatientSelected`), and goes to the row's
   module (the module button's click, as group B);
2. counts the rows of the row's guard tables;
3. opens the window the way a person's click opens it: the main menu item's `PerformClick` (`menu`), the module's own handler method
   (`handler`, for the Appointments lists and the Chart's toolbar items, which the program calls from its toolbar), a module button's
   `PerformClick` (`button`), a double-click on a module grid's row (`grid`, `accountrow`: the grid's `CellDoubleClick` with the row
   selected, as groups C and I do), or a script that opens it from the window before it (`script`);
4. checks the window's type, waits for its Load, runs the walker (section 3), prints it (`PrintWindow`) under the row's picture name;
5. closes it as Cancel does (a "save changes?" question is answered No), and any window a script opened on the way;
6. compares the guard tables' row counts: nothing may be saved.

A message box instead of the window fails the check with its text; a handler that does not return after its window closed fails it too.

| Id | Window | Opened | Guard tables |
|---|---|---|---|
| U1 | `FormClaimEdit` | Account: double-click the Account grid's row with a ClaimNum | claim, claimproc, claimpayment |
| U2 | `FormClaimsSend` | Manage: button `controlManage.butSendClaims` | claim, etrans |
| U3 | `FormClaimPayList` | Manage: button `controlManage.butClaimPay` | claimpayment |
| U4 | `FormClearinghouses` | menu Setup > Family / Insurance > Clearinghouses | clearinghouse |
| U5 | `FormInsCatsSetup` | menu Setup > Family / Insurance > Insurance Categories | covcat, covspan |
| U6 | `FormCarriers` | menu Lists > Insurance Carriers | carrier |
| U7 | `FormInsPlans` | menu Lists > Insurance Plans | insplan |
| U8 | `FormElectIDs` | menu Setup > Family / Insurance > Payer IDs | electid |
| U9 | `FormRecallList` | Appointments: handler `controlAppt.ListRecall_Click` | recall, commlog |
| U10 | `FormRecallSetup` | menu Setup > Appointments > Recall | preference, recalltype |
| U11 | `FormRecallTypes` | menu Setup > Appointments > Recall Types | recalltype, recalltrigger |
| U12 | `FormRecallsPat` | Family: handler `controlFamily.gridRecall_DoubleClick` | recall |
| U13 | `FormConfirmList` | Appointments: handler `controlAppt.ListConfirm_Click` | appointment, commlog |
| U14 | `FormConfirmationSetup` | menu Setup > Appointments > Confirmations | preference, apptreminderrule |
| U15 | `FormSchedule` | menu Setup > Schedules | schedule |
| U16 | `FormApptViews` | menu Setup > Appointments > Appointment Views | apptview, apptviewitem |
| U17 | `FormOperatories` | menu Setup > Appointments > Operatories | operatory |
| U18 | `FormApptTypes` | menu Setup > Appointments > Appointment Types | appointmenttype |
| U19 | `FormUnsched` | Appointments: handler `controlAppt.ListUnsched_Click` | appointment |
| U20 | `FormTrackNext` | Appointments: handler `controlAppt.ListPlanned_Click` | appointment |
| U21 | `FormASAP` | Appointments: handler `controlAppt.ListASAP_Click` | appointment |
| U22 | `FormInsVerificationList` | Appointments: handler `controlAppt.ListInsVerify_Click` | insverify |
| U23 | `FormPayPlanTemplates` | menu Setup > Account > Pay Plan Templates | payplan |
| U24 | `FormFinanceCharges` | menu Tools > Billing/Finance Charges | adjustment, preference |
| U25 | `FormDefinitions` | menu Setup > Definitions | definition |
| U26 | `FormProcCodes` | menu Lists > Procedure Codes | procedurecode, fee |
| U27 | `FormProcCodeEdit` | script `ScriptUProcCodeEdit`: Lists > Procedure Codes, double-click D0120 | procedurecode, fee |
| U28 | `FormFeeScheds` | menu Setup > Fee Schedules | feesched |
| U29 | `FormSecurity` | menu Setup > Security | userod, usergroup, usergroupattach, grouppermission |
| U30 | `FormTaskPreferences` | menu Setup > Tasks | preference, userodpref |
| U31 | `FormTasks` | Manage: button `controlManage.butTasks` | task, tasklist |
| U32 | `FormMedical` | Chart: double-click `controlChart.gridPtInfo`, the first row tagged tabMedical, tabMedications, tabProblems or tabAllergies | patientnote, medicationpat, disease, allergy |
| U33 | `FormAllergySetup` | menu Lists > Allergies | allergydef |
| U34 | `FormMedications` | menu Lists > Medications | medication |
| U35 | `FormDiseaseDefs` | menu Lists > Problems | diseasedef |
| U36 | `FormRxSelect` | Chart: handler `controlChart.Tool_Rx_Click` | rxpat |
| U37 | `FormRxSetup` | menu Lists > Prescriptions | rxdef |
| U38 | `FormPharmacies` | menu Lists > Pharmacies | pharmacy |
| U39 | `FormLabCases` | menu Lists > Lab Cases | labcase |
| U40 | `FormLaboratories` | menu Setup > Laboratories | laboratory |
| U41 | `FormExamSheets` | Chart: handler `controlChart.Tool_ExamSheet_Click` | sheet, sheetfield |
| U42 | `FormProviderSetup` | menu Lists > Providers | provider |
| U43 | `FormEmployeeSelect` | menu Lists > Employees | employee |
| U44 | `FormEmployers` | menu Lists > Employers | employer |
| U45 | `FrmReferralSelect` | menu Lists > Referrals | referral |
| U46 | `FormContacts` | menu Lists > Contacts | contact |
| U47 | `FormDiscountPlans` | menu Lists > Discount Plans | discountplan |
| U48 | `FrmZipCodes` | menu Lists > Zip Codes | zipcode |
| U49 | `FormPreferences` | menu Setup > Preferences | preference |
| U50 | `FormPractice` | menu Setup > Practice | preference, provider |
| U51 | `FormDisplayFieldCategories` | menu Setup > Display Fields | displayfield |
| U52 | `FormProgramLinks` | menu Setup > Program Links | program, programproperty |
| U53 | `FormReportSetup` | menu Setup > Reports | displayreport, preference |
| U54 | `FormComputers` | menu Setup > Advanced Setup > Computers | computer, computerpref |
| U55 | `FormPath` | menu Setup > Data Paths | preference |
| U56 | `FormProcButtons` | menu Setup > Chart > Procedure Buttons | procbutton, procbuttonitem |
| U57 | `FormAutoCode` | menu Setup > Auto Codes | autocode, autocodeitem |
| U58 | `FormAutomation` | menu Setup > Automation | automation |
| U59 | `FrmQuickPaste` | menu Setup > Quick Paste Notes | quickpastenote, quickpastecat |
| U60 | `FormAlertCategorySetup` | menu Setup > Alert Categories | alertcategory, alertcategorylink |

U1, U12, U32, U36 and U41 select patient #1 first (the synthetic "Gatetest, Stage" of the dump); the others take the patient the session
started with. The windows the other groups already picture (about 45: Edit Patient Information, Edit Procedure, Payment, the sheets,
Insurance Benefits, Edit Appointment, the claim forms, the billing options, the perio chart, the dashboards and others) are not repeated.

**The main window** (UM1 to UM21): restored from maximized and set to 1366x768 or 1920x1080 at the screen's top-left corner; the module's
button clicked; with patient #1, or with no patient (as the program's start without one: `PatNumCur` 0, the module refreshed, the patient
button emptied; patient #1 is selected again afterwards). The Appointments module is paged to 2026-11-09 (B1's day, with the dump's
appointments) by its day arrows, so its pictures do not move with the date; only its date panel does, which the new mask row hides. The
window's size and state are put back after each check. A size Windows refuses (a screen smaller than the size) is listed for a person with
both sizes, not failed. The walker measures the main window but never swaps its fonts (it stays open for the next checks).

**The patient dashboard** (UD1): always its own synthetic widget sheet definition, even when the copy holds others (so the picture does not depend on run order: group I's I5 copy is never used), and the delete falls back to the key InsertOrUpdate returned. The synthetic one
is added to the scratch copy by the business layer: a copy of the internal "Patient Dashboard Default" (the patient picture, labels, the
appointment grid, the individual and family insurance fields) named "UI-0c synthetic widget", with two more labels, one with the font name
"Microsoft Sans Serif" and one without (section 5.1's content fonts). Admin's group gets the Dashboard Widget permission for it when it has
none. Tools > Patient Dashboards > the widget (its menu item's `PerformClick`; the program's "You will probably need to restart" message
is read and closed), the main window at 1366x768 measured and printed, the dashboard closed again by its checked item. The sheet
definition, its fields and the permission are deleted by their keys in the same check, and the guard tables (sheetdef, sheetfielddef,
grouppermission, userodpref) are compared, so Z's counts are as at P0. Nothing is ever added to `opendental_dev`: the check runs only
in a session on a `winchk_` copy.

## 3. The walker

`Harness/UiWalker.cs`, on the window's thread once the window has run its Load, so text set at run time is measured (section 5.4's tier 2):

- **Measuring**: every control whose own Visible state is on under visible parents (a tab page counts as visible, as tier 1 walks), with
  UI-0b's `TextFitMeasurer.Measure` (TextRenderer for stock labels, radio buttons, check boxes, buttons and group boxes; MeasureString for
  the program's `UI.Button`, `UI.CheckBox` and `UI.GroupBox`; the ink check for 1-2 px height cases), at 100%, with the free space to the
  right and below inside the parent (`TextFitMeasurer.GetFreeSpace`, the rule tier 1 uses; moved into the shared file by this package)
  and the verdict "fixable" or "needs the owner". In today's font a control that overflows already overflows today: it is listed, not
  counted as new (as tier 1 counts it apart).
- **The bounds record**: `TextFitBoundsRecord.Collect` of the whole window, in UI-0b's format, the window key being the picture's name
  (`U12-patient-recalls`), so the before set's record and a later run's compare window by window.
- **The font option** (`OPMS_WINDOW_CHECKS_UI_FONT=segoe9` or `segoe825`): after the picture, the window's fonts are swapped by UI-0b's
  `TextFitFontSwap` (the rule of section 3.4, the 0.96 factor through LayoutManager), measured again, and the bounds compared with the same
  window in today's font (`TextFitBoundsRecord.Compare`; `fit-fixes.tsv` allowed for the window's type): new overflows and bounds
  differences. Only on a window the check closes afterwards (never the main window). From UI-2a-1 on, the token override takes the swap's
  place (section 5.4); this option is the tier 2 counterpart of tier 1's font options until then.
- **Dates and the clock**: a control that shows today's date (its text, in this culture's short or long form or yyyy-MM-dd), a time
  within 30 minutes of now, a date picker on today or a month calendar gives a suggested mask rectangle in the picture (from the screen;
  anchored top-left at the picture's size). The Manage module's server clock and the date range boxes of the list windows are such
  controls. The lead checks each against the before set's pictures and adds it to `tools/ui-gallery-masks.tsv`. Dates drawn inside a grid
  (`GridOD` cells are not controls) are not found; with the synthetic data the lists that show them are empty or hold the dump's fixed
  dates.
- **WPF**: a Frm's content (U45, U48, U59; `FormFrame`'s `ElementHost`) is printed but not measured: the WPF controls already use Segoe UI
  11.5 DIP (plan, section 2); the count of WPF hosts is in the notes.

Files, appended into `<results folder>\ui-walker\` (UTF-8 without a BOM, a header when a file is new): `text-fit.tsv` (every measured
control: picture, window type, font, path, type, method, kind, the pixels missing, the ink check, the free space, the verdict, the text),
`bounds-record.tsv` (today's font), `bounds-record-<font>.tsv` and `bounds-<font>.tsv` (with the font option), `masks-suggested.tsv` (rows
in the mask file's format) and `clip-notes.tsv` (picture, control, what, verdict: the `-ClipNotes` input of `tools/New-UiGallery.ps1`).
Each check's notes carry the walker's counts.

## 4. The check U0

`Harness/UiApprovalCheck.cs`, once at the end of a run that includes group U (or one of its parts), after Z and before REPORT.md: every
picture that this commit's results name (results.tsv, all groups of the run) is hashed by UI-0a's rule (`UiPictureMask.GetMaskedPixelSha256`
with the masks of `tools/ui-gallery-masks.tsv`) and compared with `tests/WindowChecksTests/UiApproved/approved.tsv` by name, size and hash:

- **approved**: the picture the owner last approved; **changed since approval**: approved before, different now: listed for the owner (the
  check is PARTLY) for the package's gallery; **not approved yet**: never approved; **missing**: named in results.tsv, not in the folder.
- `approved.tsv` holds no row until the owner approves the before set in a gallery, so today U0 reports every picture as not approved yet,
  and says so ("approved.tsv holds no approval yet, so every picture is reported as not approved yet, not as a failure ..."); it never
  fails for that.
- `U0.tsv` (each picture's size, hash and state, and the approval it was compared with) and `U0-approved-rows.tsv` (`approved.tsv` rows for
  the pictures not approved, the gallery's name left as `G-UI?`, for the lead to append once the owner approves them) go into the results
  folder.
- With `OPMS_WINDOW_CHECKS_UI_BEFORE=<the before set's results folder>` U0 also compares the walker's `bounds-record.tsv` with the before
  set's, window by window for the windows both hold (the last record of each window in a folder), by section 5.4's rules with
  `fit-fixes.tsv` allowed: `U0-bounds.tsv`, and the differences for a person.

## 5. How to run it (the lead, with the before set)

At the owner's connected desktop (`query user`: Active), after a full Debug x64 lane build:

    set OPMS_WINDOW_CHECKS=1
    set OPMS_WINDOW_CHECKS_DUMP=<od-tools>\winchk-gate3a\opendental_dev.sql
    set OPMS_WINDOW_CHECKS_GROUPS=U
    set OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui0c-groupU
    set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
    powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests

- The parts `UW` (the windows), `UM` (the main window) and `UD` (the dashboard) are group names too, for a run that would not fit the
  runner's 20 minutes (`OPMS_WINDOW_CHECKS_GROUPS=UW`, then `UM,UD`, into the same results folder; U0 runs at the end of each).
- `OPMS_WINDOW_CHECKS_UI_FONT=segoe9` adds the font option's measurements (section 3); the pictures are taken before the swap, so they are
  the same.
- For the before set (plan, section 7): the full run of groups A to K and U into one results folder, frozen as
  `.git/od-tools/test-results/ui-before/` with the commit, the dump's hash, the DPI and `ui-walker\bounds-record.tsv`; then the rows of
  `ui-walker\masks-suggested.tsv` checked against the pictures and added to `tools/ui-gallery-masks.tsv`, and `ui-walker\clip-notes.tsv`
  given to `tools/New-UiGallery.ps1 -ClipNotes` for G-UI2 and G-UI3.
- Report each check as REPORT.md does: PASSED, PARTLY (with what is for a person), FAILED with the evidence, or SKIPPED with the reason the
  dump cannot fill the window (section 7 of this note).

## 6. Self-tests (no opt-in, no program, no database, no window)

`tests/WindowChecksTests/UiHarnessTests.cs`:

- `WindowsTsv_Parses_WithTheWindowsThePlanAsksFor`: unique ids and pictures, at least 50 openable windows, each of the plan's themes
  (claims, recall, confirmation, schedules, pay plans, billing, procedure codes, definitions, security, tasks, medical, Rx, labs) with a
  window, the not-filled rows with a reason, the main window in every module at both sizes with a patient and at 1366x768 without one,
  the dashboard; the test method's name says the last window id.
- `WindowsTsv_RefusesMalformedRows`: a wrong field count, id, picture, window type, patient, module, way of opening, target or guard is
  refused with its line.
- `WindowsTsv_EveryWindowTypeExists`: each window is a Form of the program or a Frm of WpfControlsOD.
- `WindowsTsv_MenuRows_AreTheMainMenusItems_AndOpenTheirWindow`: from `FormOpenDentalMenus.cs` (each `Layout*` method's items under the
  item it fills), every menu path is the main menu's, item by item, and the leaf's handler creates the row's window (`new <Window>(`, in
  the handler or a method it calls).
- `WindowsTsv_ModuleRows_OpenTheirWindow`: the main window's module field exists; the handler exists with no arguments or (sender, e); the
  button's `Click` and the grid's `CellDoubleClick` are wired (designer) to a handler; each creates the window; the Account grid's
  double-click reads the row's column; the script exists with its signature.
- `WindowsTsv_GuardTables_AreTablesOfTheSchema`: every guard table is created by `db/init/new-database.sql`.
- `Walker_MeasuresATestForm_RecordsBounds_SuggestsMasks_AndMeasuresTheFontOption`: a test form built on an STA thread and never shown:
  the bounds record (every control and the form), the measured controls (a label on the unselected tab page too, a hidden one not), an
  overflow and its free space, the date and clock rectangles at their places, the files (the masks parse with UI-0a's mask reader, one
  header, no BOM, the clip notes), and with Segoe UI 9 the label that fits today and not in the new font.
- `Walker_FindsTodaysDateAndTheClock_Only`: today's date in its forms and a time near now; the dump's 2026-11-09, a time four hours off and
  money are not.
- `U0_ComparesSyntheticPicturesWithApprovedRows`: approved (a masked pixel changed), changed since approval (PARTLY, for the owner), not
  approved yet, missing; U0.tsv and the approved.tsv rows (they parse and approve the picture once appended); an empty approved.tsv
  reports every picture as not approved yet and passes.
- `U0_TakesThePicturesOfThisCommitsResults`, `U0_ComparesTheBoundsRecordWithTheBeforeSet`,
  `U0_RepositoryFiles_Parse_AndMaskGroupUsAppointmentsPictures`.

## 7. The windows the dump cannot fill

The synthetic dump (`.git/od-tools/winchk-gate3a/opendental_dev.sql`) holds one patient with two claims, two appointments, a treatment plan
and an insurance plan, and no row in the tables below. These windows open only on such a row, so each check is SKIPPED with the reason;
making the rows would mean saving through other windows, which group U never does.

| Id | Window | Why |
|---|---|---|
| U61 | `FormPayPlan` | no payment plan (payplan is empty); Account > Payment Plan would make one, which writes rows |
| U62 | `FormRxEdit` | no prescription (rxpat is empty), so the Chart's progress notes list none to open |
| U63 | `FormLabCaseEdit` | no lab case (labcase is empty), so the Chart's progress notes list none to open |
| U64 | `FormTaskEdit` | no task (task is empty) to double-click in a task list |
| U65 | `FormRecallEdit` | no recall (recall is empty), so patient #1's recalls list none to open |
| U66 | `FrmReferralEdit` | no referral (referral is empty) to open from Lists > Referrals |
| U67 | `FormAllergyEdit` | no patient allergy (allergy is empty) to open from the medical window |
| U68 | `FormStatementOptions` | no statement (statement is empty) in the Account grid |

The list windows of the same data (U9 recall list, U12 patient recalls, U31 tasks, U37 prescriptions, U39 lab cases, U45 referrals) open
empty and are pictured; a dump with such rows fills the rows above (change `none` to the way the window opens).

## 8. Checks

- Full Debug x64 lane build of `OpenDental.Local.slnf`: 0 errors.
- Targeted (x64, `-RequireBuildOutput`) WindowChecksTests, UiRefreshTests and ReleaseFlowTests (UI-0a's gallery self-tests read the mask
  file this package changed), with NoHqTests: passed, none failing (`.git/od-tools/test-results/ui0c-final.txt`). WindowChecksTests 66
  tests: 26 passed (12 of them this package's self-tests), 40 skipped (the opt-in groups, group U's three among them); UiRefreshTests 23
  (22 passed, the opt-in scan skipped); ReleaseFlowTests 267 (260 passed, 7 skipped); NoHqTests 115 of 115.
- Group U itself: not run here (the owner's session was disconnected); see section 5.
- `tools/Test-NoEolFlips.ps1 -Base main`: no changed text file changed its line ends.
