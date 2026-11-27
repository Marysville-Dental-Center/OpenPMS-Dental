# Standing rules: OpenPMS-Dental

Rules for every session in this repository. They override defaults. Owner decisions behind them are in
[docs/decisions/owner-questions.md](docs/decisions/owner-questions.md); add a rule here only when the owner makes one, keep each to one or two lines.

## Product

- **Practice-neutral.** No practice, person, server, host or developer-path names in tracked files (a test checks). Office material goes
  to the private OpenPMS-Dental-Office repository.
- **No office progress.** Tracked files never describe a particular office's status or plans (its switch-over, its Open Dental version, its
  devices, what it uses or has tested); describe what the product does for any office. That material goes to OpenPMS-Dental-Office.
- **Synthetic data only** in tests, screenshots and docs: fictional patients, 555-01xx phones, example.com addresses; scratch databases are dropped.
- **Nothing phones home.** No calls to Open Dental's servers (`NoHqTests`); integrations stay off until the office turns them on.
- **Nothing is cut.** Every feature and test is kept and ported unless the owner deliberately removes it.
- **Open all the way down.** Every shipped file has published source; nothing is decompiled.
- **Current and native.** .NET 10 x64; newest stable libraries from official sources; a vendor's native 64-bit interface first.
- **Native UI.** A visual opens in an ordinary standalone window (minimise, maximise, close); no special modes or flows unless the owner asks.

## Database schema

- **`main` tracks the database schema of the current stable Open Dental release** (now 26.2; move to 26.3 when Open Dental marks it
  stable). Re-implement its features from Open Dental's public documents; never merge Open Dental code after 24.3.63.
- **Fork schema changes compose with Open Dental's.** New data goes in fork-only tables (`ForkTableSet`); never add, change or drop
  columns, keys or rows of Open Dental's tables. Open Dental's next schema must apply on top, and a migrated database must still match
  Open Dental's reference schema (`SchemaVerifier`).
- Every schema change is a fork migration step ([README](OpenPmsDentalBusiness/Misc/ForkMigrations/README.md)); regenerate
  `db/init/new-database.sql` with it.

## Git

- **Work on `main`.** A feature that takes several days may use a remote branch `feature/<name>`: keep it short-lived, merge it into
  `main` when done, then delete it locally and on GitHub.
- **Releases:** tag the release commit on `main` `v<version>` and build it from the tag; no release branch. Only a fix for an older
  release after `main` has moved on gets `release/<version>`, made from its tag then ([docs/develop/release-process.md](docs/develop/release-process.md)).
- **When a feature is finished, commit and push it** (after the build and its tests pass). Commit only your own files: other sessions may
  have uncommitted work in the same tree.
- Keep line endings and BOMs as the file has them (most C# files are CRLF with a BOM); never run `sed -i` on them.

## Every change

- **Design before code.** Every new feature or package starts with a design document, written and reviewed before implementation, in its
  folder: a feature or plan in [docs/specs/](docs/specs/) (Open Dental parity in `features-<version>.md`, schema in `schema-changes-*.md`),
  listed in [docs/README.md](docs/README.md); its port note links it.
- A package: a plan, a port note in [docs/port-notes/](docs/port-notes/), its own tests, a full build and the suites passing.
- **A new user-facing feature gets a how-to guide with screenshots** in [docs/how-to/](docs/how-to/README.md), linked from its index.
- Record owner decisions in [docs/decisions/owner-questions.md](docs/decisions/owner-questions.md) with the date and the owner's words.

## Fast iterations

- **Inner loop: build only what you touched.** Restore once, then build the edited project or its test project with
  `-restore:false -m` (its references come along); build the whole `OpenDental.Local.slnf` only at the gate. See
  [docs/develop/building.md](docs/develop/building.md), "Fast inner loop".
- **Test what the change affects:** `tools\run-tests.ps1 -Mode Targeted -Include <the suites that cover it>` while iterating; the gate is a
  full build plus the main suites; run `-Mode Full` after any wide change (UI, layout, schema, formats) and before every release.
- **One gate run per worktree at a time.** Parallel work goes in separate worktrees, with its own scratch-database prefix.
- **Measure before tuning the build:** a binary log (`-bl`) and `-clp:PerformanceSummary`; never change a build default on a guess.
- **Keep shared signatures stable while iterating:** CodeBase and OpenPmsDentalBusiness expose internals to other projects, so a changed
  signature there rebuilds most of the solution; edits inside method bodies do not.
- **Skipping analyzers is allowed in the inner loop** (`-p:RunAnalyzersDuringBuild=false`), never at the gate.
- **Size the process to the change:** a small fix or follow-up is one person or agent, one build, one targeted run; only a new package gets
  a separate review. No release builds or setup packages unless the owner asks.
- **New test assemblies declare their parallelism:** `[assembly: Parallelize(Scope = ExecutionScope.ClassLevel)]` when they use no
  database or shared files, `[assembly: DoNotParallelize]` when they do.

## UI

- **Fonts and colours come from `UiTheme` and its tokens,** never literals (`element_inventory.py --check`, `colour_literals.py --check`).
- **Every window is on the layout grid** ([docs/port-notes/UI-12.md](docs/port-notes/UI-12.md); `GridRulesTests`, `FinalSweepTests`): a
  window never shrinks, nothing is cut at 100% or 150%, grid columns hold their values (`GridColumnFit`).
- **User-visible text says "Open PMS" / "Open PMS - Dental",** never OD or Open Dental, except the GPL attribution and vendor identifiers (`BrandTests`).
- **A visual change ships with a before/after gallery at 100% and 150%,** audited and fixed before the owner sees it; owner approvals go to
  [tests/WindowChecksTests/UiApproved/approved.tsv](tests/WindowChecksTests/UiApproved/approved.tsv). A test that pins a size or picture
  names the approved change that set it.

## C# (new code; leave legacy Open Dental code in its style unless you are changing it anyway)

- Never block on a task on the UI thread (`.Result`, `.Wait()`); `async void` only for event handlers, with a try/catch inside.
- `ConfigureAwait(false)` in library code (CodeBase, Business, DataConnectionBase), not in UI code.
- Touch controls only on their UI thread (`Control.BeginInvoke`/`InvokeAsync`, WPF `Dispatcher.InvokeAsync`).
- `using` every Font, Brush, Pen, Bitmap, Graphics, stream, DbConnection, DbCommand and DataReader you create; never dispose shared
  brushes or the theme's fonts.
- No hard-coded pixel sizes: scale with the control's DPI and the program's layout scaling (PerMonitorV2).
- Machine-readable text (files, SQL literals, HL7, XML, vendor bridges) is formatted and parsed invariantly, numbers through
  `ToStringNetFx` (`FormatParityTests`); only what a person reads uses the current culture. Compare identifiers and paths ordinally.
- SQL values go in as parameters in new code; in legacy SQL strings, every value goes through `POut`. Never concatenate raw input.
- **Device, vendor, file and HL7 input is untrusted:** bound sizes and lengths, allow only known types and characters, read XML with
  `DtdProcessing.Prohibit`, keep paths inside their base folder; refuse a bad input as a whole, with a reason in the log.
- Catch specific exceptions, rethrow with `throw;`, never leave a catch empty, and log with context.
- No new static mutable state; keep logic out of form code-behind so it is testable without a window or database; take time from a
  `TimeProvider` where a test needs to control it.
- `#nullable enable` in new files only (the project needs C# 8 or later); no project-wide switch in legacy projects.
- No BinaryFormatter (removed in .NET 9).

## Docs and the project site

- **Docs always reflect the current state.** A change that alters behaviour, setup, versions, paths or status updates every document that
  describes it in the same commit; no doc describes something that is no longer true.
- **Links must be valid.** Never link a file, heading or page that does not exist. `python -I tools/check_doc_links.py` must pass
  (the Docs workflow runs it on every push).
- Keep the docs tree by reader ([docs/README.md](docs/README.md)): `how-to/`, `admin/`, `develop/`, `specs/`, `decisions/`, `reports/`,
  `release-notes/`, `port-notes/`. A new document goes in its folder and in the index.
- **The project site (`site/`, GitHub Pages) stays current:** update it when a feature, release or doc it links changes.
- How-to guides are concise: the goal, the steps, a screenshot, links to the reference. Screenshots follow [docs/screenshots/README.md](docs/screenshots/README.md).

## Build and test

- Offline packages: set `OPMS_NUGET_OFFLINE` to the package folder; build `Debug | x64` ([docs/develop/building.md](docs/develop/building.md)).
- Tests need MariaDB 10.11 on `127.0.0.1:3306` (root, blank password): `tools\run-tests.ps1` ([docs/develop/testing.md](docs/develop/testing.md)).
- If the tree does not build because of another session's uncommitted edits, build a clean copy (`git archive HEAD` plus your files and
  the `third-party/` submodules) instead of touching their files.
