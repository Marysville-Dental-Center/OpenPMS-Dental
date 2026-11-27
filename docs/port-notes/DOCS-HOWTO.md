# DOCS-HOWTO: how-to guides, standing rules and the documentation link check

Package DOCS-HOWTO, on `main`, from the owner's request of 2026-11-26: documentation for setting up a database from scratch and for moving
from the current stable Open Dental, a tree of concise how-to guides with screenshots (one for every new feature, backfilled for the features
built apart from Open Dental), standing rules for every repository, and documentation links that are always valid.

## What changed

### 1. The how-to guides (`docs/how-to/`)

- `README.md`, the index, grouped (Set up, Imaging, Running the practice), with the rules for writing a guide.
- Seven guides: a new database, moving from Open Dental, X-rays with a sensor, intraoral photos, visuals on a second monitor, inventory,
  routine maintenance. Each is under a page, with steps and a screenshot, and links to the reference.
- **Moving from Open Dental** says plainly what adoption accepts today (24.3.63, 25.3.48 and the 25.3.47/25.3.54 label pair) and that a
  database of Open Dental's current stable version, 26.2, is not adoptable yet.
- Linked from the README's "More", the project site (navigation and Get started) and `docs/screenshots/README.md`.

### 2. The screenshots

- `tests/UiRefreshTests/HowToScreenshots.cs`: opt-in like DocScreenshots (`OPMS_UI_DOC_SCREENSHOTS`), on the same kind of local scratch database
  with DocScreenshots' fictional practice, plus fictional supplies, lots, default materials, a stock count and maintenance items with records,
  steps and fields. Draws Inventory setup, Materials used, Stock Count, the maintenance board, item and record windows, Adopt Database (its two
  status labels set to what the preflight writes for an Open Dental 25.3.48 copy) and the connection settings tool (with its title bar).
  `DocScreenshots.Save`, `DrawDialog` and `Settle` became `internal` for it.
- `tests/ImagingCaptureTests/HowToCaptureScreenshots.cs`: the X-ray capture session on the capture tests' fake sensor (two of four slots
  exposed with synthetic pictures, fictional patient and providers, no database) and the last image in its own window.
- Ten `docs/screenshots/howto-*.png`, each looked at before commit: fictional data only.
- A fix the pictures showed: Materials used cut its last column's header to "On Han"; the Procedure column is 16 pixels narrower, so the
  six columns fit beside the scroll bar (`FormMaterialsUsed.FillGrid`).

### 3. Standing rules

`CLAUDE.md` at the root of this repository, with the owner's rules of 2026-11-26 added to the existing ones, grouped (Product, Database
schema, Git, Every change, Docs and the project site, Build and test):

- `main` tracks the database schema of Open Dental's current stable release; fork schema changes compose with Open Dental's (fork-only
  tables, never a change to Open Dental's tables), so Open Dental's next schema applies on top.
- Valid, well-structured documentation links and a current project site.
- Work on `main`; a short-lived `feature/<name>` branch only for a multi-day feature, merged and deleted when done; each release a tag
  `v<version>` on `main` (since 2026-11-26 no `release/<version>` branch unless an older release needs a fix).
- A finished feature is committed and pushed.

The branch rules are also in [repository-layout.md](../develop/repository-layout.md#branches) and [release-process.md](../develop/release-process.md).
The other repositories got their own `CLAUDE.md` in the same change.

### 4. The link check

- `tools/check_doc_links.py`: every relative link and image of every Markdown file and of `site/index.html` must name a file of the
  repository, and a `#fragment` a heading (GitHub's anchors) of the Markdown file it names; the repository's own GitHub blob and tree
  addresses are checked as relative links. `--external` also fetches web addresses. No broken link was found when it was added.
- `.github/workflows/docs.yml` runs it on every push to `main` (and pull request) that changes a document, the site or the script.
- [testing.md](../develop/testing.md#documentation-links) describes it.

### 5. Docs corrected

`db/README.md` said the new-database file was at the Open Dental 26.1.70 schema; it is at 26.2.0 since P262-SCHEMA.

## Tests

- `python -I tools/check_doc_links.py`: 0 broken links.
- The two screenshot tests run (opt-in) and make every picture; UiRefreshTests, ImagingCaptureTests and NoHqTests pass.
