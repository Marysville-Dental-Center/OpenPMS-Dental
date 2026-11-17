# WINCHK-HEADLESS: the stage 1 launch checks as an opt-in headless test (port note)

`tests/WindowChecksTests` runs the checks of the stage 1 launch checklist (`.git/od-tools/briefs/stage1-launch-checks.md`, outside the
repository) without a person at the screen: the real program and its tools, built from the worktree, started on `winchk_` scratch copies of
the synthetic test database on 127.0.0.1:3306, their windows reached through their own handlers (in process) or by window messages (out of
process), and printed with PrintWindow, DrawToBitmap or RenderTargetBitmap. Nothing sends input (no SendInput) and no UAC prompt is answered.

- Opt-in: `OPMS_WINDOW_CHECKS=1` and `OPMS_WINDOW_CHECKS_DUMP` (a dump of the synthetic database). Without them every test is Inconclusive, so
  Core and Full runs are unchanged. `OPMS_WINDOW_CHECKS_GROUPS` (A to K, comma-separated) splits a run; `OPMS_WINDOW_CHECKS_OUT` is the results
  folder (default `%TEMP%\stage1-windows-headless`); `OPMS_WINDOW_CHECKS_BUDGET_MINUTES` (default 15) is the run's time budget (WINCHK-3).
- The harness writes `REPORT.md` itself at the end of every run (AssemblyCleanup, after Z), from `results.tsv` in the results folder.
- Merged on main as 1173acc6a (WINCHK-HEADLESS, two review rounds). The review notes left over from both rounds are done in WINCHK-3 below.
- Each run's copy is brought to this build's fork schema level before any check starts the program, and RdlDesigner's menu is reached
  through its MenuStrip (WINCHK-4 below).
- On .NET 10 x64 an in-process session is the program's own process (its apphost with TestProbeHost's startup hook), the stage 3b checks
  C16 to C20, F5 and G3 are added, and the stage 3b screenshot pass with its before/after pairs is done (WINCHK-5 below).

## WINCHK-3: the review notes of both rounds

Branch `winchk/w3-1`. Every numbered note of both reviews, and what was done.

### Review 1

1. *Lead decisions followed (A7, D5, J15, F1, REPORT.md).* Records what was verified; nothing to change.
2. *The screenshots match their claims.* Records what was verified; nothing to change.
3. *I4 is PASSED although the copy, the referral, its attachment and the undo are made by the business layer.* The owner has not answered
   the question the implementer asked (`docs/owner-questions.md` has no answer), so the default that changes no status was taken: the report's
   PASSED now means "code verified every item the check scripts; steps it does not script are named under 'Not scripted'". A check records
   such steps with `CheckResult.NotScripted`, and the report lists them in a section of their own ("Not scripted": gaps of the harness, not items
   for a person). I4 names its four: the Copy in Setup > Sheets, the double-click that opens the editor, the referral windows, and the undo
   through those windows. If the owner prefers PARTLY for such checks, it is one line in `CheckResult.Status`.
4. *D5 assumes the last picker item is the D4 copy.* D5 now finds the D4 copy in the picker's own list by its EFormDefNum and asserts that the
   fill window's `EFormCur.EFormDefNum` is that number (D4 records it as a key, so D5 finds it in a new session too).
5. *J12's 'no key text' rests on a count; J12 and J13 read securitylog.* J12 now asserts that no entry says "key" or holds the row's key (read
   before the delete; the checklist's key is empty). The verified texts of J12 and J13 say "securitylog (what Tools > Audit Trail shows for
   ...)", and the comment says the window is opened only for J12.png.
6. *J15 does not assert Cancel.* It does: the Send drop-down lists Cancel (the checklist's third item).
7. *F3 counts dashboardlayout rows after Exit Setup.* The count is taken before the dashboard opens and compared after it closes.
8. *I1, I6 and I11 keep 'prints blank while locked' when the on-screen print worked; the session state is not recorded.* I1 (the big chart),
   I6 (graphical perio) and I11 (a new in-process check ISCREEN, since I11's program runs out of process) run B5's on-screen test on the
   chart's own window and drop the item when it shows the DirectX picture. The test was tightened: the window counts only while the chart
   really draws in DirectX (`WhyNotDirectX`: its engine, no fallback logged, its off-screen render works), because in this run's session a
   fallback engine drew the window and it printed "drawn". The session state (console or remote desktop, locked or unlocked, from the
   session's WTSINFOEX flags, and whether WPF renders) goes into each run's P0 line and the report header. B5 uses the same test (see
   "Found by the runs" below).
9. *The header lists every database in run.log; P0 and Z show only the last run.* Each P0 names its run's groups, database and session; the
   report has a "Group runs" table with every run's P0 and the Z that ended it ("none" when the run was stopped before its clean-up), and the
   databases are those of the runs in the report, each marked dropped or not from run.log.
10. *The table cuts the text at 700 characters.* The report says so above the table and that results.tsv holds each check's full text.
11. *J7 and J12 use patient #1's provider's names, not 'Dentist, Gate'; 0.4 #6 is skipped.* The owner has not answered this question, so
   the checklist's names are left as they are and the harness keeps using the copy's own names. P0's note says that 0.4 #6 is not done and
   why, and that it is an open owner question. The checklist itself lives in `.git/od-tools/briefs`, outside the repository, and is not edited.
12. *I2's second Save to Images is listed for a person.* It is scripted now: File > User Settings > tick "Chart graphics color distinguishes
   current provider" > Save (the ToothChartUsesDiffColorByProv row is asserted), Save to Images again (I2-images-provcolour.png), untick >
   Save (the row is asserted gone). The undo deletes both images the check added, and nothing else.

### Review 2

1. *Group I's time against the runner's 20-minute kill.* Three parts. (a) A run's time budget (`WinChkFixture`, 15 minutes by default):
   once it is spent no in-process session starts and no check runs (each is recorded FAILED "not run: the run's time budget is spent ... run
   this group again on its own"), every check's timeout is cut to the budget left plus a minute, and group I's out-of-process starts (I8, I11)
   check it too, so Z, the drop and the report still fit in the 20 minutes. (b) After a check that does not end, `InProcHost` asks the session
   whether it is stuck (the window's thread does not answer, or a window is left open); group I then starts a new session for the next check
   instead of losing the rest of the group, and the rows earlier checks made reach the new session as keys (below). (c) `ScratchDb` drops, at
   the start of a run, the copies an earlier run with the same results folder created and never dropped (run.log says "Created" or
   "Restored" and no "Dropped"), still on the server and made more than 25 minutes ago, so a run going on beside it is never touched; a copy
   whose restore failed is dropped at once. The test was not split into several test methods: MSTest's order of methods is not guaranteed,
   and the per-assembly kill counts the whole run anyway.
2. *D5's pixelHeight assertion can never fail; CheckFixedDocumentPage can throw the same exception itself.* D5 counts the session's
   unhandled exceptions before Print, waits for Print to return, runs one call through the window's thread's queue, and only then reads the
   exceptions (`Ui.CountExceptions`, `Ui.GetExceptionsFrom`), so the verified line now says what it checked. A page without a size (0, NaN or
   infinite) is reported as "page 1 has no size to render (the harness's own rendering, not the program's)" instead of the harness throwing.
3. *Every ThreadAbortException is downgraded to a note.* Only an abort that ODThread reports (source "ODThread ...") is a note, and one of an
   ODThread's own thread that reaches AppDomain.UnhandledException (its stack runs in `CodeBase.ODThread.Run`): the run of group I showed
   ODThread's QuitSync abort arriving that way. Any other abort fails the check.
4. *ODEnvironment.MachineName is not escaped in IJTests.* It is escaped as in PZTests (`Replace("'","''")`) for I11's three statements.
5. *Fallbacks that could delete the wrong row.* Every check that adds a row and undoes it later takes the table's highest key before and
   takes as its row the one row above it afterwards (`MaxKey`, `AddedKey`; none when there is not exactly one), and deletes only that one:
   I5's widget sheet and I6's exam (deleted by I10, in its own session, by their keys), I4's referral attachment (by its RefAttachNum, and its
   undo compares with patient #1's count before), I3's saved plan, I2's images, IPREP's procedure (deleted by IUNDO by its key), D4's eForm
   def (deleted by D5) and E's device. Without a key nothing is deleted and the check fails. The keys travel from session to session in
   `CheckResult.DictKeys`, kept by `InProcHost` and handed to each session (`ProgramDriver.SetKeys`); the statics I10 read before were always
   0 in its new AppDomain, which is why it fell back to MAX.
6. *H1/H2 restore designerstate.xml with ReadAllText/WriteAllText.* A designerstate.xml that was there is copied aside (into the results
   folder) and copied back, then compared byte for byte; Z2 checks it is again exactly the one H found (and still checks that a state H wrote
   for its start is gone). Run with a designerstate.xml holding a BOM and CRLF line ends: restored byte for byte (same MD5 before and after).
7. *The report mixes older runs' rows under the current commit.* Each results.tsv line now holds the commit the run was built from (short
   HEAD, "+changes" when tracked files differ). The checks table and its counts hold only this commit's results; the last result of each check
   that this commit has not run is listed apart ("Older results", with its commit), not counted. Lines written before the commit field read as
   older.
8. *CloseDashboard's note says 'answered OK (discard)' on the Cancel branch too.* Each branch writes its own note.
9. *Verified as acceptable (hosts, users, A7, printing, J15, RdlDesigner, git, AssemblyCleanup).* Records what was verified; still true.
   J15 still never presses Import and imports, creates or deletes no certificate or key.

### Found by the runs: a locked session

This package's runs were the first in a locked session (remote desktop, locked; recorded in every P0). Two things the harness assumed do not
hold there:

- **WPF renders nothing.** RenderTargetBitmap gives a blank bitmap even for a red test square, so every RenderTargetBitmap picture of groups
  C, D and E came out blank and failed. `Shot.CanRenderWpf` renders that square; a blank RenderTargetBitmap picture taken while it fails gets
  the reason (`ShotInfo.BlankReason`) and is listed for a person instead of failing the check. PrintWindow pictures are unaffected.
- **DirectX has no device.** Every tooth chart picture (`GetBitmap`) throws a NullReferenceException in `ToothChartDirectX.Render`, so every
  host in group I failed, and B5 stopped. IPREP now tests DirectX (`WhyNotDirectX`) and, without a device, switches the copy to the Simple 2D
  chart through File > Graphics, so the hosts' code (Save to Images, the treatment plan prints, the referral letter, the dashboard widget, the
  sheet editor) still runs and is checked; each of those checks then lists the DirectX picture for a person, graphical perio and I7's
  DirectX Save go to a person with what the program said, and IJTests sets GraphicsSimple back to DirectX in the copy before I11. I11's
  start-up search then saves "invalid", which is listed for a person. B5 lists the DirectX chart for a person with the reason.
- **The downgrade applies only to a locked session** (review fix). `WhyNotDirectX` gives the "no device" reason only when GetBitmap throws
  and the session really cannot draw: `Native.GetSessionState` says locked, or `Shot.CanRenderWpf` renders nothing. In a session that can
  draw, GetBitmap throwing is reported as the program's failure with the exception's type and message. IPREP switches to Simple 2D (and
  sets I.Simple2D) only for that reason; for any other one (another engine, a fallback logged, GetBitmap throwing at an unlocked screen) it
  fails and leaves DirectX on, so I1 to I7 and I10 keep their DirectX assertions. B5 keeps "No tooth chart fallback was logged" as an
  Expect and fails when GetBitmap throws, and I11's "invalid" format is for a person only when IPREP found no device, a failure otherwise.

Groups A, C, G and K were not run (none of their checks changed; C's WPF pictures use the same `ShotElement` and get the same handling). No
group imports, creates or deletes a certificate or key; J15 opens the Direct certificate window and never presses Import.

### Runs

All runs used this worktree's own Debug build, `winchk_` copies on 127.0.0.1:3306 restored from the synthetic dump, and the results folder
`winchk-w3` in the worktree (not committed). Every `winchk_` database was dropped (run.log; `SHOW DATABASES LIKE 'winchk%'` empty afterwards).

- Without `OPMS_WINDOW_CHECKS`: WindowChecksTests 29 tests, all skipped (Inconclusive), none failing; NoHqTests 57 passed.
- First runs, before the locked-session handling: D, E, F, H failed only on blank WPF pictures; I failed on the DirectX device. Both led to
  the handling above.
- The budget and the leftover drop, in a separate results folder: a stand-in leftover `winchk_` database, recorded in that folder's run.log
  as made at 15:00, was dropped at the start of the next run; group J with a 1-minute budget recorded J13, J15 to J17 and JLOGONFAIL as
  "not run" (FAILED) and still ran Z, dropped its copy and wrote the report (that run fails on purpose).
- A run with a designerstate.xml beside RdlDesigner (H): restored byte for byte, Z2 passed.
- Final runs on dde7fda10 (the code as merged; this paragraph is the only later change), after the full build (0 errors), one Targeted run per
  group set, each with NoHqTests (57 passed every time):

| Groups | WindowChecksTests | P0 | Z | Time |
|---|---|---|---|---|
| B | passed, none failing | PASSED | PASSED | 101 s |
| D, E, F, H | passed, none failing | PASSED | PASSED | 119 s |
| I | passed, none failing | PASSED | PASSED | 237 s |
| J | passed, none failing | PASSED | PASSED | 119 s |
| none (no opt-in) | 29 skipped (Inconclusive), none failing | | | 3 s |

  The report after the last run: 50 checks on this commit, PASSED 24, PARTLY 25, SKIPPED 1, none FAILED, no older results; every session
  "remote desktop, locked, WPF renders nothing". The PARTLY items are the pictures WPF and DirectX cannot draw while the screen is locked,
  plus the items that were already for a person. Group I takes about 4 minutes, well inside the 15-minute budget.
- After the review fix (the downgrade only in a locked session), on 4a24ff605 after the full build (0 errors): groups B and I in one
  Targeted run with NoHqTests (57 passed), WindowChecksTests passed, none failing, 301 s; P0 and Z PASSED; B5 and I1 to I11 PARTLY with the
  reason "DirectX has no device in this session (remote desktop, locked, WPF renders nothing): its off-screen render (GetBitmap) threw
  NullReferenceException", as the session was locked. Without the opt-in: 29 skipped, none failing. The unlocked branch (GetBitmap throwing
  in a session that can draw is a failure) could not be run here, as every session of this package was locked; it needs a run at an
  unlocked screen.

## WINCHK-4: the copy at this build's fork schema level, RdlDesigner's MenuStrip

Branch `winchk/w4-1`. The stage 3a gate run (lead, 2026-11-15, main 43fc52af1) failed for two reasons that were the harness's, not the
program's. Both are fixed in the harness; the dump, the database it was made from and the program are unchanged.

### 1. The scratch copy is migrated before any check starts the program

The gate's dump predates EHR-2's fork step 2026111401 (`EhrSteps.cs`, the `ehrcode` table): its copy was at fork schema level 2026110801, so
the program's start-up asked "The database will now be updated from schema level 2026110801 to 2026111401" (A1 failed on that message box,
and every in-process start of F1 to F4 and I1 to I7 timed out waiting for the main window behind it).

- `Harness/CopyMigration.cs`: right after `ScratchDb` restores the dump into its `winchk_` copy, the copy is brought to this build's fork
  schema level with the program's own code, as start-up runs it (`PrefL.RunForkMigrations`) but without its prompt: the business layer of
  the test process is connected to the copy (127.0.0.1:3306, the name must start with `winchk_`, and `SELECT DATABASE()` must name it before
  anything is written), `ForkVersionGuard.Evaluate` decides from the copy's markers, and `ForkMigrationHandOff.Runner.RunPending` runs the
  pending steps (the runner's log goes into run.log). The guard is asked again afterwards; nothing may be pending.
- P0 records the line: "Fork schema level: migrated the copy from 2026110801 to 2026111401: steps 2026111401 ..." or "already at <level>".
- When the copy cannot be brought there, P0 fails with the reason and no check runs (the program is never started on that copy, so no
  prompt is ever shown): a dump older than the adoption point (no fork markers: the guard says ProceedCompat or NeedsAdoption), one a newer
  build updated (NeedsNewerBuild), inconsistent markers (Refuse), a step that throws (the step left in progress is named), or a step still
  pending afterwards. The copy is dropped at once; the first check test of the run fails with P0's reason, the others are Inconclusive
  ("Not run: P0 failed"), and REPORT.md is still written.

### 2. H1 and H2 choose RdlDesigner's menu items through its MenuStrip

S3a-M3a turned RdlDesigner's WinForms MainMenu into a MenuStrip. A MenuStrip has no native HMENU, so the Win32 `GetMenu` lookup H1 and H2
used (`Native.FindMenuCommand`, then WM_COMMAND) found nothing: "View > RDL Text is in the menu bar" and "View > Preview is in the menu
bar" failed.

- `Uia.ChooseMenuStripItem` (RdlDesigner runs out of process, so UI Automation): the menu bar among the window's children whose items hold
  the path's first text, the menu expanded (ExpandCollapsePattern: the drop-down shows and its DropDownOpening runs, as when a person opens
  it, so RdlDesigner's `menuView_Popup` sets the items' Enabled states), the item found by its text (ampersands and a tab's shortcut text
  ignored), checked enabled, and invoked (InvokePattern, the item's PerformClick; no SendInput, as the other out-of-process clicks). A menu
  left open is collapsed again. Every UI Automation call has a timeout; an Invoke whose Click shows a modal window counts as chosen after
  10 s. H1 and H2 report the strip's texts as evidence when an item is absent.
- `Native`'s classic-menu region (GetMenu, FindMenuCommand, PostMenuCommand) is removed: nothing else used it.
- The other checks were searched for a MainMenu, ContextMenu, MenuItems or GetMenu lookup that S3a-M1, M2 or M3 made stale: none. Every
  in-process check reaches the program's menus as ToolStripMenuItems (`InProcChecks.GetMenuItem`); J15 reads the Send button's
  `ContextMenuStripDropDown` (changed in S3a-M3) and keeps a `ContextMenu` branch for a control that still has one, which finds nothing
  and fails nothing. K1 (CentralManager, whose main menu S3a-M3a also moved to a MenuStrip) uses no menu.

### Self-tests (no program start, no opt-in)

`HarnessMigrationMenuTests` (WindowChecksTests):

- `CopyMigration_MigratesAnOlderCopy_RefusesUnadoptedAndNewer`: planted databases `winchk_selftest_<12 hex>` on 127.0.0.1 that hold only
  the preference table. An adopted copy with every step but 2026111401 applied (as the gate's dump was) is migrated: exactly that step runs,
  `ehrcode` exists, every level of the build is applied, `ForkSchemaLevel` is the build's latest, nothing is left in progress, and the line
  names both levels and the step; a second run says "already at". With an applied level this build does not have it is refused
  (NeedsNewerBuild) and left unchanged; a 24.3.63 database without fork markers is refused as older than the adoption point, and no row
  and no table is added. A name without `winchk_` is refused before any connection. Both databases are dropped (the name never matches a
  run's copy, `winchk_` and ten digits, so `DropLeftovers` never takes it).
- `MenuStripLookup_ChoosesItemsByText`: a MenuStrip laid out as RdlDesigner's (File; View > Designer, RDL Text, Preview, Properties...,
  with their shortcuts and a DropDownOpening handler that sets Preview's Enabled state) on a form built in the test process on a thread of
  its own, off every screen, never activated and not in the taskbar; its drop-downs are fully transparent while they show (a drop-down is
  moved onto the nearest screen). View > RDL Text is found, its menu opened (the handler ran), its Click run once and the menu closed
  again; a disabled Preview is found, reported disabled and not clicked; an absent item and an absent menu are reported with the texts
  that are there.

### Runs

Worktree build of cb7dcfdf9 (the code as committed; only this section was added after): the full Debug lane build (0 errors) and a Release
lane build of the worktree (`-Configuration Release`, 0 errors) for A7 to A9; the gate's dump `.git/od-tools/winchk-gate3a/opendental_dev.sql`,
`OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18`, one Targeted run per group set with `-RequireBuildOutput`, WindowChecksTests and NoHqTests (112 passed
every time). Every session was "remote desktop, unlocked, WPF renders". Each run's copy was migrated from 2026110801 to 2026111401 (step
2026111401, 1.4 to 1.8 s) and dropped (run.log; `SHOW DATABASES LIKE 'winchk%'` empty afterwards).

| Run | Groups | Results folder (REPORT.md) | WindowChecksTests | Time |
|---|---|---|---|---|
| winchk4-1 | A, B, C, D, E | `%TEMP%\winchk4-1` | 1 failed (A9, see below) | 284 s |
| winchk4-2 | F, G, H | `%TEMP%\winchk4-2` | passed, none failing | 126 s |
| winchk4-3 | I, J, K | `%TEMP%\winchk4-3` | passed, none failing | 465 s |
| winchk4-4 | A, B, C, D, E | `%TEMP%\winchk4-4` | passed, none failing | 284 s |

Run winchk4-1 used a copy of main's Release folder (`OPMS_WINDOW_CHECKS_RELEASE`), as this worktree had no Release build yet. A9 then
failed on that set-up only: it compares the Release `UpdateFileCopier.exe` with the worktree's own `UpdateFileCopier\bin\Release`, which did
not exist. After the worktree's own Release lane build, run winchk4-4 repeated the same groups on `<rel>` (no override): A9 PARTLY, as at the
gate. Neither is a program defect.

Each check's status (P0 and Z PASSED in every run; A from winchk4-4):

| Check | Status | Check | Status | Check | Status | Check | Status |
|---|---|---|---|---|---|---|---|
| A1 | PARTLY (failed at the gate) | B6 | PARTLY | D3 | PARTLY | I4 | PASSED (failed at the gate) |
| A2 | PASSED | B7 | PASSED | D4 | PASSED | I5 | PARTLY (failed at the gate) |
| A3 | PASSED | C1 to C14 | PASSED | D5 | PASSED | I6 | PASSED (failed at the gate) |
| A4 | PASSED | C15 | PARTLY | E1 to E4 | PARTLY | I7 | PARTLY (failed at the gate) |
| A5 | PARTLY | D1 | PASSED | F1 | PARTLY (failed at the gate) | I8 | PARTLY |
| A6 | PASSED | D2 | PASSED | F2 to F4 | PASSED (failed at the gate) | I9 | SKIPPED |
| A7 | PASSED | G1 | PARTLY | H1 | PARTLY (failed at the gate) | I10 | PASSED (failed at the gate) |
| A8 | PASSED | G2 | PASSED | H2 | PARTLY (failed at the gate) | I11 | PASSED |
| A9 | PARTLY | I1 | PARTLY (failed at the gate) | J1 to J17 | PASSED | K1, K4 to K6, K8, K10 | PASSED |
| B1 | PARTLY | I2 | PARTLY (failed at the gate) | K2 | PARTLY | K3, K7, K9 | SKIPPED |
| B2 to B5 | PASSED | I3 | PARTLY (failed at the gate) | | | | |

No check FAILED. The PARTLY items are the ones for a person that were already there (pictures to compare with stage 0, H1's mouse work on
the design surface, H2's preview messages that RdlViewer draws itself, and so on); H1's and H2's menu steps are all verified now
("View > RDL Text is in the menu bar (the MenuStrip's items: System, File, Edit, View, Data, Format, Tools, Window, Help / Designer, RDL
Text, Preview, Show Report in Browser, Properties...)", "is enabled (after its menu opened)", "chosen", then the XML in the editor and the
preview). "System" is the maximized MDI child's own menu, which WinForms merges into the strip. The counts of the reports: winchk4-2
PASSED 4, PARTLY 4; winchk4-3 PASSED 27, PARTLY 7, SKIPPED 4; winchk4-4 PASSED 29, PARTLY 11.

Without `OPMS_WINDOW_CHECKS`: WindowChecksTests 34 tests, 5 passed (the harness self-tests), 29 skipped (Inconclusive), none failing.

## WINCHK-5: the window checks on .NET 10 x64 (the stage 3b screenshot pass)

Branch `net10/winchk5-1` from `net10/switch` (652b9720e). The 3b gate's Full run failed
`HarnessTests.SessionProcess_RunsFromTheProgramFolder_WithItsConfig`: it expected the .NET Framework layout. This package ports the
in-process harness to the .NET 10 layout, fixes the other .NET Framework assumptions the runs found, adds the 3b checks the harness can
do, runs every group at the owner's connected desktop and makes the before/after pairs (section 6 of `docs/specs/net10-plan.md`, the
"Stage 3b (net10)" item, and the 3b full-test gate's "the screenshot pass of section 6 is complete with before/after pairs").

### 1. The in-process session is the program's own process

On .NET 10 the program is self-contained (decision 20): `OpenPmsDental.dll` is its assembly, `OpenPmsDental.exe` the apphost, and the
folder holds its runtime, `OpenPmsDental.runtimeconfig.json` (`includedFrameworks`, the NLS mode of decision 4), `OpenPmsDental.deps.json`
and `OpenPmsDental.dll.config` (only the `applicationSettings`, S3b-4). A configuration file's binding redirects mean nothing on .NET. The
harness's session process was a copy of TestProbeHost in the program folder with `OpenPmsDental.exe.config` as its config; that copy cannot
run there (its apphost takes the folder's `hostfxr.dll`, which looks for the shared frameworks in that folder: "You must install or update
.NET ... No frameworks were found", exit 150), and the program cannot start from a test's folder (S3b-0 item 7).

- `tests/TestProbeHost`: the serving loop moved to `ProbeServer` (Main uses it unchanged); `StartupHook.cs` is a .NET startup hook (a type
  `StartupHook` in no namespace, internal) that serves a test's handler inside a real program's process before its `Main` runs and then
  ends the process (`Main` never runs). It removes `DOTNET_STARTUP_HOOKS` and its own variable first, so no process the program starts
  inherits it; without its variable it returns at once. `ProbeProcess.StartInProgram(exe, handler, timeout)` starts the program's apphost
  from its own folder with that hook: nothing is copied into the folder and nothing deleted. `ProbeProcess.Start` and `RunOnce` are unchanged
  (the other test projects' probes).
- `InProcHost.StartSessionProcess` starts `OpenPmsDental.exe` of the Debug (or Release) folder that way. The session's process is the
  program's: its folder is the application base, the folder's runtime runs it, .NET reads the program's runtimeconfig, deps.json and
  .dll.config, and `OpenPmsDental.dll` is the entry assembly from the start. `ProgramDriver.PatchProcessIdentity` still checks
  Application.StartupPath, ExecutablePath and the entry assembly and reports each as "(own)" or "(patched)": every run reported
  "(own)" for all three.
- The config check: `GetRuntime` reports what .NET read in the session (ProcessPath, BaseDirectory, the folders of System.Private.CoreLib
  and Windows Forms, the framework, the bitness, `APP_CONTEXT_DEPS_FILES`, System.Configuration's file, every runtimeconfig property, the
  Settings values). `HarnessTests.SessionProcess_RunsFromTheProgramFolder_WithItsConfig` (ported, on the real `OpenPmsDental\bin\Debug`;
  inconclusive when the program is not built) asserts: the entry assembly is the folder's `OpenPmsDental.dll` before and after the patch,
  nothing is patched, the process is the folder's apphost, its base is the folder, CoreLib and Windows Forms load from the folder (not a
  machine-wide .NET), the host read the folder's `OpenPmsDental.deps.json`, every property of the runtimeconfig has its value in the
  process (`System.Globalization.UseNls` true), System.Configuration reads `OpenPmsDental.dll.config` and each setting the Settings class
  declares has the file's value, the hook's variable is gone, nothing of TestProbeHost is in the folder, the process is gone after Dispose
  and the folder's files are as before. The old checks that still mean something are kept (the entry assembly, the patch text, the config
  file); the copy of the host and of `.exe.config` are gone, so their checks became the runtimeconfig/deps.json/.dll.config ones.
- The session starts as `ProgramEntry.Main` does on .NET 10 (S3b-4): `Application.SetDefaultFont(Microsoft Sans Serif 8.25)` and
  `SetHighDpiMode(PerMonitorV2)` first. Without them every plain form of the in-process pictures drew in Segoe UI 9. The per-process calls
  are safe now: the session's process is the program's alone.

### 2. The same .NET Framework assumption elsewhere

Found by the runs (each a harness assumption, none a program defect):

- **Form's event keys.** `ProgramDriver.RemoveEventHandlers` (before Stop, and before the harness closes windows left open) looked up
  `EVENT_FORMCLOSING`/`EVENT_FORMCLOSED`; .NET's Windows Forms names them `s_formClosingEvent`/`s_formClosedEvent`. The lookup threw on the
  window's thread, so the main window's closing was never detached and every session's Stop waited 60 s ("The window's thread did not end").
  Both names are tried; `HarnessTests.MainClosing_IsDetachedThroughFormsEventKeys` pins it (a form that is never shown). Group A to E's run
  went from 906 s to 260 s.
- **PrintPreviewControl's pages.** I3 counted the laid-out pages through the field `pageInfo`; .NET names it `_pageInfo`, so both sheet
  previews counted 0 pages. Both names are tried.
- **The build time.** A3 and A7 compared the build stamp with the apphost's write time; the stamp is in the managed assembly, which the
  compile writes after the stamp (`ForkWriteBuildStamp` runs before CoreCompile). Both now take the assembly (`ProgramAssembly`), and A7 takes
  A3's window (0 to 5 minutes, plus the minute the prompt leaves out): the .NET 10 Release compile of the program took 3.0 minutes, the .NET
  Framework's 0.2 to 1.1, which failed A7's old 3-minute rule. REPORT.md's header times the managed assembly too.
- **An exception's text.** K2 knows DocumentationBuilder's pre-existing start-up error (`new Version(major,-1,1)` on the 99.0.0.0 sentinel)
  by the .NET Framework's message; .NET's says "minor ('-1') must be a non-negative value. (Parameter 'minor')". Both are known.
- **Thread.Abort** throws on .NET (SYSLIB0006): RunCheck no longer calls it after a check that does not end (the script's background thread
  is left; its windows are closed and the session is checked for being stuck, as before).
- Searched for the rest: no other lookup of a .NET Framework private name (`Application`'s two path fields already had both names), no
  other `.exe.config`, no other start from the test's own folder (ATests and KTests start the apphosts in their own output folders).

### 3. The 3b checks added

From section 6's "Stage 3b (net10)" item, what the harness did not do yet, each as a picture; nothing is pressed that prints, saves or sends.

| Check | What | Group |
|---|---|---|
| C16 | MsgBoxCopyPaste with a synthetic text; its font | C (`C16toC20_DefaultFontWindows`) |
| C17 | The progress windows FormProgressStatus and FormProgressExtended (ODProgress, on their own thread), closed by their close actions | C |
| C18 | The combo box drop-down the clinic picker and every UI.ComboBox open (FormComboPicker), from Edit Patient Information | C |
| C19 | ToolTipOD | C |
| C20 | The main menu (MenuOD) draws in its window's font; the menu bar | C |
| F5 | Each dashboard graph on its own, after it has loaded (the Windows Forms charting of .NET 10) | F (`F5_DashboardGraphs`) |
| G3 | Manage > Time Cards > Print All: a classic FormPrintPreview (TryPreview only previews), its pages; employee and pay period made and deleted | G (`G3_PrintPreview`) |
| G2 | Now also prints each ODR parameter window (`G2-params-n.png`) | G |

A form that sets no font of its own (C16 to C18) must have Microsoft Sans Serif at 8.25 pt times the text scale factor (decision 9);
C19's and C20's pictures (a line of text: 3 and 5 colours) are checked as text on a background, not by a window's "more than 5 colours".
Already there: the splash (A1), dashboards and their print preview (F1 to F4), the eForm and treatment plan print previews (D5, I3), the
RDL viewer (G2). Not reachable headless and still for a person (section 6): "Make text bigger" above 100 %, 150 % and two monitors of
different scaling, dragging between monitors, real prints to a PDF printer, voice perio, e-mail and clearinghouse sends, the self-update.

### 4. Program defects found and fixed (same behaviour as net48)

- **The main menu in Segoe UI 9** (`OpenPmsDental/UI/MenuOD.cs`). Every MenuOD drew wider in Segoe UI 9 on .NET 10, where the .NET
  Framework drew Microsoft Sans Serif 8.25. MenuOD handed its font to its MenuStripOD only in OnFontChanged. On .NET Framework the move onto
  the form always raised it (Control.DefaultFont, the system's font, never equalled a form's `new Font("Microsoft Sans Serif",8.25f)`: another
  character set); on .NET 10 `Application.SetDefaultFont` makes them equal, OnFontChanged never comes and the strip kept ToolStrip's own
  default. `MenuOD.OnParentChanged` gives the strip its font when they differ. C20 checks it.
- **The dashboard's y axes read "${1:0}"** (`OpenPmsDentalGraph/Base/ChartNumberText.cs`, S3b-FMT's). GraphQuantityOverTime gives its y
  axis a composite format (`{0:0}`, `${0:0.0}`, `{0:0,}K`), which the chart writes with string.Format; ChartNumberText took it for a custom
  number format, so its zeros became digit placeholders ("${1:0}" for 10, "{0:8}" for 8). A format with `{` is written with string.Format
  through a provider whose ICustomFormatter writes each double through `ToStringNetFx`. Listed in `tools/FormatSweep/allowed-sites.txt`
  (routed); `FormatParityTests.ChartLabels_CompositeFormat_IsWrittenAsTheChartWritesIt` pins it (axis labels $0 to $10 with no brace, a
  point label "{0:0.00} pts" of 1.005 is "1.01 pts").

### 5. Self-tests (no program window, no opt-in)

Without `OPMS_WINDOW_CHECKS`: WindowChecksTests 38 tests, 6 passed (the harness self-tests: AssemblyProbe, the session process, the event
keys, the result round trip, the copy migration, the MenuStrip lookup), 32 skipped (Inconclusive), none failing
(`winchk5-self-3.txt`, `winchk5-self-4.txt`). FormatParityTests 21 of 21 (`winchk5-fmt-1.txt`). NoHqTests 115 of 115 in every run.

### 6. The runs

The owner was connected by remote desktop (`query user`: Active before every group set); every P0 recorded "remote desktop, unlocked,
WPF renders". Full Debug x64 lane builds (0 errors) and a Release x64 lane build for A7 to A9; `OPMS_WINDOW_CHECKS=1`,
`OPMS_WINDOW_CHECKS_DUMP` the 3a gate's dump `.git/od-tools/winchk-gate3a/opendental_dev.sql` (restored into a `winchk_` copy per run and
migrated from 2026110801 to 2026111401, step 2026111401), `OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18`, `run-tests.ps1 -Mode Targeted -Platform x64
-RequireBuildOutput -Include WindowChecksTests`, results in `.git/od-tools/test-results/winchk-3b-1` to `-3` (REPORT.md in each) and
`winchk-3b-1.txt` to `-3.txt`. Every copy was dropped (run.log; `SHOW DATABASES LIKE 'winchk%'` empty afterwards). Every in-process
session started the program within 5 s.

| Run | Groups | Commit | WindowChecksTests | Counts (REPORT.md) | Time |
|---|---|---|---|---|---|
| winchk-3b-1 | A, B, C, D, E | 37eeebc3d | passed, none failing | PASSED 34, PARTLY 11 | 260 s |
| winchk-3b-2 | F, G, H | 1a14e4c84 | passed, none failing | PASSED 4, PARTLY 6 | 159 s |
| winchk-3b-3 | I, J, K | 1a14e4c84 | passed, none failing | PASSED 27, PARTLY 7, SKIPPED 4 | 383 s |

The commits after 37eeebc3d touched nothing groups A to E reach (the chart labels, I3's page count, G3, K2). The earlier runs are kept
beside them as evidence of what was fixed: `winchk-3b-1-first` (A7, C18, C19 failed; every Stop waited 60 s), `-1-second` (all passed,
before the menu fix), `-1-third` (C20's picture rule), `winchk-3b-2-first` and `-second` (the chart labels; G3 without its page count),
`winchk-3b-3-first` (I3's page count, K2's message).

No check FAILED. SKIPPED by design: I9 (TC-8 removed the developer switch), K3, K7 and K9 (elevation; the harness never answers a UAC
prompt). **Every PARTLY item, for a person:**

- A1: compare A1-main.png and A1-splash.png with stage 0's pictures (looks only).
- A5: at 100 % the NoDpi pictures should look as A1 (the flag only changes scaling above 100 %).
- A9: the shields in A9-debug-folder.png and A9-release-folder.png.
- B1: B1-appts-day.png and B1-appts-week.png against stage 0's 03b and 03c.
- B6: B6-images.png against stage 0's 01 (tree and toolbar).
- C15: the exit time after a session that opened the Wiki (stage 0 finding 2) needs File > Exit in a real run.
- D3: the red underline's look in D3.png.
- E1 to E3: bold, wrapped and multi-line labels in E1.png to E3.png; E4: the device line, the bold 'Settings in effect' and the wrapped red
  check line in E4.png.
- F1: the graphs in F1.png (empty on synthetic data but the 30-day one); F5: the graphs' axes, legends and fonts in F5-1.png to F5-6.png.
- G1: paging through the report's pages (one page here). G3: the page in G3.png (margins, fonts, the time card's grid).
- H1: adding a Text box to the page header and File > Save (mouse work on RdlDesigner's design surface). H2: the preview's messages in
  H2-preview.png (RdlViewer draws them itself).
- I1: the filling on 30 and the date slider in I1-chart.png (the big chart is the same picture). I2: both Save to Images pictures show the
  chart with the filling. I3: the chart on the classic preview's page. I5: the filling on 30 in I5-reopened-chart.png. I7: the Chart's 3D
  chart after each Save (smoothed, quick live view, smoothed again). I8: sharp, not stretched or blurred after the resize; a corner drag is
  mouse work; the memory after Show Big and after closing it.
- K2: Build OD Documentation is not pressed headless (it opens the written XML in a browser).

These are the items the .NET Framework runs had (WINCHK-4), plus F5's and G3's for the new pictures.

### 7. The before/after pairs

- **Before (net48):** the 3a gate's window-check output, copied from `%TEMP%\winchk-gate3a-1` to `-3` to
  `.git/od-tools/test-results/winchk-before-net48/` before anything else (with WINCHK-4's net48 runs `winchk4-1` to `-4`, which hold the
  pictures the gate's runs lack: its F, H and I checks failed on the stale copy that WINCHK-4 fixed). 111 of the 145 pairs take the gate's
  picture, 34 WINCHK-4's.
- **After:** the three runs above.
- **The page:** `.git/od-tools/test-results/winchk-pairs-3b/index.html` (relative paths to `before/`, `after/` and `diff/` beside it;
  `winchk-pairs-3b.zip`; outside the repository). A summary in plain words for the owner at the top, an overview of every pair with its
  share of changed pixels, then each group's pairs, the most changed first: the net48 picture, the .NET 10 picture and a difference image
  (the after picture faded to grey, pixels that differ by more than 16 in a colour channel in red, an area only one picture has in magenta)
  with its numbers, and the pictures only one set has. Made by a script of the scratchpad (`Make-Pairs.ps1`, System.Drawing).
- **What it shows:** 145 pairs, 53 identical to the pixel; 18 pictures only in the .NET 10 set (the new checks); none only in the net48
  set. Most of the rest differ where the time or the copy's name shows (the Appointments module's "now" line and today, the 30-day graph's
  dates, Help > About's database). Different on purpose: the Release publish prompt (S3b-7's text naming .NET 10), the .NET crash dialog of
  DocumentationBuilder, RdlDesigner's toolbar (S3b-2b's Panel) and its error texts naming the missing file's path. For a person, from the new
  runtime and none changing what a person can do: print previews place and scale the page a few pixels differently (G1, I3, D5, F4), the
  appointment grid sits 1 pixel lower in two of its three pictures (the out-of-process day view does not), the dashboard's cells in edit
  mode are 1 to 2 pixels apart, CrudGenerator's list box scrolls its selected item to the bottom instead of the top (K6). The tooth chart
  pictures (I1, I8, ...) are S3b-5W's. All at 100 % (96 DPI) on the same remote desktop, so no picture shows a DPI or text-scale difference.

### 8. Checks

- Full Debug x64 lane build of `OpenDental.Local.slnf` after the last product change: 0 errors (and again at the end of the package).
- `tools/Test-NoEolFlips.ps1 -Base net10/switch`: no changed text file changed its line ends.
- No `winchk_` database left on 127.0.0.1:3306; no TestProbeHost file left in the program folders.
