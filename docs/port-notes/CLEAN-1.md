# Port note: CLEAN-1 (the port notes' later cleanups)

Package CLEAN-1. Branch `net10/clean1-1` from `main` (`319e659b4`). No schema change, no package, nothing downloaded, no new DLL, no new
test project (the solution files belong to TC-8 in this wave, so the new tests live in existing test projects). No test starts Word, a vendor
program or a helper other than its own copy, opens a real registry key or shows a window.

## 1. How the items were found

`grep` over `docs/port-notes/*.md` for "later cleanup", "Later cleanups", "For a later cleanup" and "Left for a later cleanup", each list read
whole; also "Left for later" and "for strict x86 parity, later" (same kind of list, other wording). N64-A3.md has no such list: its section 8
("Notes for the lead") names no cleanup, only cases that need no code (a 64-bit server failing later with another HRESULT, a DLL surrogate,
lone surrogates in strings, no release note, nothing for 3b). Lists that S3a-FU2 already worked through are in the table as "done earlier".

Sorting: **(a)** a real defect or a user-visible rough edge in a file no parallel stream owns: fixed, smallest change, with a test that
fails before and passes after; **(b)** a docs-only correction: fixed; **(c)** left, with the reason (a file TC-8 or S3a-11 owns, the
DirectX or OpenGL engines that TC-9a deletes, stage 3b, a behaviour the owner decided or must decide, more than a small fix, or not a defect).

## 2. The items

| # | Source (note:line on `main`) | Item | Sort | Done, or why left | Test |
|---|---|---|---|---|---|
| 1 | S3a-M3.md:237, :234 | `RdlDesign/SortingCtl.SwapRow` swaps a third column of a two-column table (Up/Down threw); `GroupingCtl.SwapRow` swaps a second and third column of a one-column table (Up/Down threw) | (a) | Fixed: each swaps only its table's columns. Also: Up on the new row at the end threw (the row has no table row yet); it now does nothing, as Down already did | S3aM1Tests `M3RdlDesignGridTests.SortingCtl_UpDown_SwapTheRows_NewRowIsAscending`, `GroupingCtl_UpDown_SwapTheRows` |
| 2 | S3a-M3.md:237 | A new sort row with only an expression shows Direction unchecked (as if descending) while Apply writes it Ascending | (a) | Fixed: the table's Direction column defaults to true, so a new sort row is checked (Ascending), which is what Apply writes; Apply unchanged | S3aM1Tests `SortingCtl_UpDown_SwapTheRows_NewRowIsAscending` |
| 3 | N64-A.md:121 | `LetterMerges.GetWordApp` starts Word again after the first call on a just-started Word fails with a registration error, so a broken registration starts two Word processes before the message | (a) | Fixed: a Word started by this call whose first call fails with a registration error is released and the clear message thrown, with no second start. A stale object of an earlier call (Word closed) still starts Word again; a new object that then fails with a registration error is released too. Test seams `FuncCreateWordAppForTests`, `ActionReleaseWordAppForTests` (internal; `InternalsVisibleTo("N64ATests")`) | N64ATests `WordTests.GetWordApp_*` (3), `Source_CreateWordApp_AndTheCallers` |
| 4 | N64-A.md:121 | Plugins' hook-problem set is not capped at 100 like the problem list | (a) | Fixed: the set keeps the last 100 (a queue in the order written drops the oldest); a problem among them is still logged once | N64ATests `PluginLoadTests.HookTime_ProblemsKeptForLoggingOnce_AreTheLast100` |
| 5 | N64-A.md:121 | `FormLetterMergeEdit` shows the registration message with a raw `MessageBox` instead of `FriendlyException` | (a) | Fixed: `FriendlyException.Show(ex.Message,ex)`, as the other four callers | N64ATests `WordTests.Source_CreateWordApp_AndTheCallers` |
| 6 | N64-A.md:121, :113 | Survey row 10 still says "Required32Bit, Preferred32Bit or x86" is refused | (b) | Fixed in `docs/specs/native64-survey.md` row 10: 32BITREQUIRED without 32BITPREFERRED, mixed-mode x86 or PE32+ of another machine | none (text) |
| 7 | S3a-1.md:541 | `ImageStore.Import(Bitmap,...)` and `ImportEobAttach(Bitmap,...)` read `ComputerPrefs.LocalComputer` between the insert unit and the try that deletes the row on a failure | (a) | Fixed: the scan quality and the JPEG encoder are worked out before the insert unit, so the try follows the unit at once | ImagingTests `WiringTests.ImportBitmap_QualityReadBeforeTheRowInsert_TheTryFollowsTheUnit` |
| 8 | S3a-1.md:543 | Sections 9.9 and 9.10 are out of order | (b) | Fixed: 9.9 moved before 9.10 | none (text) |
| 9 | S3a-1.md:544 | FormTrackNext leaves the grid empty after a cancelled fill | (c) | Left: the callers replace `_listAppointmentsPlanned` (RefreshAptList) before FillGrid, so keeping the old rows after a cancel would pair them with the new list and a double-click would open another appointment. Keeping both needs the list read moved into the fill, which is more than a small fix; the empty grid is safe (Refresh fills it) | |
| 10 | S3a-1.md:545 | FormBackup's long-path question can still be open when a cancel arrives | (c) | Left: no wrong outcome (No stops the backup, Yes stops at the next cancel check); closing a message box shown from the worker thread is more than a small fix | |
| 11 | S3a-1.md:546 | `FormAdvertisingPostcardsSend.RefreshFromVendor` takes the worker's lists before checking the cancel | (a) | Fixed: returns false at once when the call was not OK, before taking the lists | M2MISCTests `ProgressWinCancelMessageTests.Source_PostcardsRefreshFromVendor_TakesTheListsOnlyFromACallThatWasOk` |
| 12 | S3a-1.md:548 | `FrmProgressAuto._actionAfterJoinTimeoutForTests` is a private hook set by reflection | (c) | Left: not a defect (test plumbing only); an internal hook would add an `InternalsVisibleTo` to WpfControlsOD for one test | |
| 13 | S3a-12.md:359 | `ControlImages.cs` passes `(int)this.Handle` as EZTwain's ignored `PDF_SetCompression` argument | (c) | Left: not a defect (EZTwain ignores the argument; S3a-FU2 changed FormImages' line for consistency only) | |
| 14 | S3a-12.md:363 | `TwainCaptureCapabilityTests` takes the TW_INFO size inline | done earlier | S3a-FU2 (S3a-12.md section 14) | |
| 15 | S3a-12.md:364 | Scan rule 3 accepts a bitness branch anywhere in the file | done earlier | S3a-FU2 (S3a-12.md section 14) | |
| 16 | S3a-12.md:365 | S3a-12's new files are LF beside CRLF neighbours | (c) | Left: changing a file's line ends rewrites every line of it (lost blame, conflicts with every branch that touches it); the repository has no eol rule that asks for it | |
| 17 | S3a-12.md:366 | Two comment lines in `FormODBase.cs` and `FormFrame.cs` (both already mixed) became CRLF | (c) | Left: the same; the files were mixed before | |
| 18 | S3a-12.md:367 | The lane could print its time limit and add vstest `/Blame` | (c) | Left: the lane's scripts are not in this repository | |
| 19 | S3a-13.md:343 | The helper is not in `$BuildIdFiles` or `ForkReleases.ListBuildIdFiles` | (c) | Left: the note says to add it with the next change of either list; the build id is read by S3a-11's installer tests (`tests/ReleaseFlowTests/InstallerTests.cs`) in this wave | |
| 20 | S3a-13.md:345 | The shipping helper keeps the `--test-module` hook | (c) | Left: waits for the install-location policy, as the note says | |
| 21 | S3a-13.md:347 | `QuickBooksDesktopHelperTests` should delete its helper copy in a nested `finally` and end a copy still running first | (a) | Fixed in the test: the copy's own process (by its random name) is ended if it is still running, and the copy and its config are deleted in a nested `finally`, so a failing check no longer leaves them | VendorDirectTests (the test itself) |
| 22 | S3a-13.md:349 | Pipe rights could be narrower than `GW` | (c) | Left: not a defect (the note: harmless, same user, one instance at most) | |
| 23 | S3a-13.md:351 | Parent watch could wait on one handle opened at lookup time | (c) | Left: not a defect (the job object already covers the pid-reuse window) | |
| 24 | S3a-15.md:83 | `OpenGLWinFormsControl.ReadFrontBuffer` never frees its `GCHandle` | (c) | Left: the OpenGL engine, which TC-9a deletes; nothing calls the method | |
| 25 | S3a-15.md:120 | The DirectX format search can take minutes in the test | done earlier | S3a-FU2 (S3a-15.md, "S3a-FU2: the format search") | |
| 26 | S3a-2.md:317 | `ODFileUtilsStartTests`' failure-path hygiene | done earlier | S3a-FU2 (S3a-2.md section 8) | |
| 27 | S3a-8b.md:117 | Five test items (entry assembly, `ListNotScripted`, argument quoting, the bounded wait, `HelixPackageTests` comment) | done earlier | S3a-FU2 (S3a-8b.md section 7) | |
| 28 | X12-SENDER.md:158 | The 270 is refused for a missing title or phone although it writes only the TIN | (c) | Left: the note leaves it to the owner ("relax it if the owner prefers"); the owner kept the rule on 2026-11-15 | |
| 29 | X12-SENDER.md:160 | Name the Treating Provider case in the refusal text | (a) | Fixed: with Treating Provider billing the TIN entry adds one whole sentence naming whose Default Provider's TIN is used (the clinic's or the practice's) | BCLAIMTests `X12SenderTests.TreatingProviderBills_Missing_TheEntryNamesTheCase` |
| 30 | X12-SENDER.md:161 | Build the refusal from whole translatable sentences | (c) | Left: every entry and the frame of the message would be reworded and their translation keys changed; more than a small fix. The new sentence of item 29 is whole | |
| 31 | X12-SENDER.md:162 | Add a `gold_` test with clinics turned on | (c) | Left: GoldenOutputTests' fixture database (`gold_`) is outside this package's database rule (scratch names `clean1_`) | |
| 32 | B-MED.md:30 | `MedicationPats.GetAllMissingMedications` has no caller | (c) | Left: removing code is the owner's call (never cut a feature) | |
| 33 | B-PRINT.md:206 | `GridOD.Export` has no callers: remove it? | (c) | Left: an owner question; never cut a feature | |
| 34 | BIN-A.md:104 | A migration could delete the unread DirectX 11 preference and computer setting | (c) | Left: DirectX, which TC-9a removes (owner, 2026-11-15: no DirectX after the switch) | |
| 35 | BIN-B.md:127 | Remove the always-false AvaTax branches and the stub | (c) | Left: an owner question; never cut a feature | |
| 36 | S3a-14.md:795 | S3a-14g's early signature reads in four forms also run for in-process Topaz boxes | (c) | Left: not a defect (the review traced each and found the saved data the same as on main); four windows, and the Topaz path changes again in 3b | |
| 37 | REL-FILES.md:132 | "Left for later": readers of `Application.ProductVersion` and `SoftwareName`, visible "Open Dental" texts, DemandForce's name, the decoupling plan's line, OpenDentHL7 | (c) | Left: the rename run's items and an owner question, not cleanups of this kind | |
| 38 | S2-2.md:508 | "Left for later": show the TLS refusal's reason in place of the bare 1042 | (c) | Left: a new message in the connection path, more than a small fix | |

## 3. Parallel streams

Nothing of TC-8 (the tooth chart engine switch and its files, the solution files) or S3a-11 (the installer, `tools/Test-Installer.ps1`,
`tests/ReleaseFlowTests/InstallerTests.cs`, `docs/release-process.md`, `README.md`) is touched; item 19 waits for S3a-11's file, and items
24 and 34 for TC-9a.

## 4. Build and test record

(Filled in at the end.)
