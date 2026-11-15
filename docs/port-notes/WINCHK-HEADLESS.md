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
