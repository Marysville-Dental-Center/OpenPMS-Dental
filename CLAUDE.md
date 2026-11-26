# Standing rules: OpenPMS-Dental

Rules for every session in this repository. They override defaults. Owner decisions behind them are in
[docs/owner-questions.md](docs/owner-questions.md); add a rule here only when the owner makes one, keep each to one or two lines.

## Product

- **Practice-neutral.** No practice, person, server, host or developer-path names in tracked files (a test checks). Office material goes
  to the private OpenPMS-Dental-Office repository.
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
- **Releases:** cut `release/<version>` from `main` when a version is released, and tag it `v<version>`
  ([docs/release-process.md](docs/release-process.md)).
- **When a feature is finished, commit and push it** (after the build and its tests pass). Commit only your own files: other sessions may
  have uncommitted work in the same tree.
- Keep line endings and BOMs as the file has them (most C# files are CRLF with a BOM); never run `sed -i` on them.

## Every change

- A package: a plan, a port note in [docs/port-notes/](docs/port-notes/), its own tests, a full build and the suites passing.
- **A new user-facing feature gets a how-to guide with screenshots** in [docs/how-to/](docs/how-to/README.md), linked from its index.
- Record owner decisions in [docs/owner-questions.md](docs/owner-questions.md) with the date and the owner's words.

## Docs and the project site

- **Links must be valid.** Never link a file, heading or page that does not exist. `python -I tools/check_doc_links.py` must pass
  (the Docs workflow runs it on every push).
- Keep the docs tree structured: how-to guides in `docs/how-to/`, reference in `docs/`, specs in `docs/specs/`, history in `docs/port-notes/`.
- **The project site (`site/`, GitHub Pages) stays current:** update it when a feature, release or doc it links changes.
- How-to guides are concise: the goal, the steps, a screenshot, links to the reference. Screenshots follow [docs/screenshots/README.md](docs/screenshots/README.md).

## Build and test

- Offline packages: set `OPMS_NUGET_OFFLINE` to the package folder; build `Debug | x64` ([docs/building.md](docs/building.md)).
- Tests need MariaDB 10.11 on `127.0.0.1:3306` (root, blank password): `tools\run-tests.ps1` ([docs/testing.md](docs/testing.md)).
- If the tree does not build because of another session's uncommitted edits, build a clean copy (`git archive HEAD` plus your files and
  the `third-party/` submodules) instead of touching their files.
