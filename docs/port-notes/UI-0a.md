# Port note: UI-0a (the gallery builder of the UI refresh)

Package UI-0a of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-0a row and section 8): branch
`ui/ui0a-1` from `main` (`e7f87b1d8`). Tools and tests only: no program code, no project of the program, no schema change, no change of look
or behaviour. Nothing was downloaded; no program was started; no database was used.

Files:

| File | What |
|---|---|
| `tools/New-UiGallery.ps1` | the gallery builder (the in-repository successor of the lost `Make-Pairs.ps1` of WINCHK-5); ASCII, Windows PowerShell 5.1 |
| `tools/UiGallery/UiPictureMask.cs` | the one masking and hashing rule, the comparison, the mask and approval file readers |
| `tools/ui-gallery-masks.tsv` | the masks: the clock, dates, the copy's database name |
| `tests/WindowChecksTests/UiApproved/approved.tsv` | the owner's approvals; empty (its header only) |
| `tests/ReleaseFlowTests/UiGalleryScriptTests.cs` | the self-tests (section 6); `ReleaseFlowTests.csproj` links `UiPictureMask.cs` |

## 1. How to run it

    powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <before folder> -After <after folder> -Name G-UI3

- `-Before`, `-After`: folders of `*.png` pictures, matched by their path relative to the folder (the harness writes a flat folder:
  `A1-main.png`, `U12-claims.png`). `-Before` may be left out for a first gallery: every picture is then "only after".
- `-Out`: the gallery folder; default `%TEMP%\UiGallery-<Name>`. A new or empty folder, or an earlier gallery (it holds `ui-gallery.txt`),
  which is emptied first. Any other folder is refused and left as it is, and so is a folder that overlaps a picture folder.
- `-Name`: the gallery's name, G-UI1 to G-UI4 (never shortened: the tooth chart's galleries are G0 to G5).
- `-Masks`: default `tools\ui-gallery-masks.tsv`. `-Approved`: default `tests\WindowChecksTests\UiApproved\approved.tsv`.
- `-ClipNotes`: a TSV of clip notes; default `clip-notes.tsv` in the after folder when it has one (section 4).
- `-Commit` (default `git rev-parse --short HEAD`), `-Note` (a line for the owner at the top), `-Threshold` (a pixel counts as changed when
  a value differs by more than this; default 0, any difference; WINCHK-5's pairs page used 16), `-NoZip`.

It writes into `-Out`: `before/`, `after/` (the pictures, copied as they are), `diff/` (one difference picture per changed picture),
`gallery.tsv`, `masks.tsv` (the mask file used), `index.html`, `ui-gallery.txt` (the marker), and `-Out.zip` beside the folder. Exit code
0, or 1 with `New-UiGallery: <reason>`. It needs no program, no database and no network; a run over the 182 WINCHK-5 pictures (145 pairs) takes
about 15 seconds without the zip.

The pictures are synthetic only: the harness's `winchk_` copies of the synthetic dump (section 8 of the plan), or test pictures.

## 2. The masking and hashing rule (one function)

**The rule.** `UiPictureMask.GetMaskedPixelSha256(picture pixels, picture name, mask rows)`:
1. the picture's pixels as 32-bit BGRA, row by row with no row padding (from any pixel format; the layout of
   `ChartImageCompare.GetPixelsArgb` in `tests/ToothChartImageTests`, so a 24-bit and a 32-bit PNG of the same pixels hash alike);
2. every pixel covered by a mask row that applies to this picture (its pattern matches the name and its size column is `*` or the
   picture's size) set to 0 in all four bytes (transparent black, which an opaque screen picture never has);
3. the SHA-256 of those bytes, in lower-case hex.

The size is not part of the hash: `approved.tsv` keeps it in its own column, and the check compares both.

**Where it lives and who calls it.** `tools/UiGallery/UiPictureMask.cs`, namespace `UiGalleryTools`, one copy:
- `tools/New-UiGallery.ps1` compiles it with `Add-Type -Path ... -ReferencedAssemblies System.Drawing`. Windows PowerShell 5.1 compiles with
  C# 5, so the file uses no newer syntax (no string interpolation, `nameof`, `?.` or expression-bodied members) and only the .NET Framework
  and `System.Drawing`.
- A test project compiles it in as a linked file. ReleaseFlowTests does today; UI-0c adds the same lines to
  `tests/WindowChecksTests/WindowChecksTests.csproj` for its check U0:

      <Compile Include="..\..\tools\UiGallery\UiPictureMask.cs">
        <Link>UiPictureMask.cs</Link>
      </Compile>

  and U0 calls, per picture it takes:

      List<UiMaskRow> listMasks=UiPictureMask.ReadMasks(<repo>\tools\ui-gallery-masks.tsv);
      List<UiApprovedRow> listApproved=UiPictureMask.ReadApproved(<repo>\tests\WindowChecksTests\UiApproved\approved.tsv);
      string sha=UiPictureMask.GetMaskedPixelSha256(bitmap,"U12-claims.png",listMasks);   //or the picture file's path
      bool isUnchanged=UiPictureMask.IsApproved(listApproved,"U12-claims.png",bitmap.Width,bitmap.Height,sha);

  The picture name is the one the gallery uses: the path relative to the picture folder with forward slashes (for the harness's flat
  folder, the file name).

The self-test `Builder_MakesTheGallery_FromSyntheticPictures` proves the two uses agree: the SHA-256 the script's Add-Type copy writes into
`gallery.tsv` equals the one the copy compiled into ReleaseFlowTests computes.

**The comparison** (`UiPictureMask.Compare`, used by the builder; U0 needs only the hash): over the larger of the two pictures' areas, a
pixel masked in either picture is not compared; a pixel only one picture has counts as changed; any other pixel counts as changed when a
B, G, R or A value differs by more than the threshold. % changed = changed / compared. The difference picture, written only when something
changed: the after picture faded to grey, changed pixels red, the area only one picture has magenta (the look of the WINCHK-5 pairs page),
and masked areas hatched in blue.

## 3. The masks

`tools/ui-gallery-masks.tsv`, tab-separated, `#` comment lines and a header line skipped:

| Column | Meaning |
|---|---|
| picture | a wildcard (`*`, `?`, any case); without a slash it matches the file name, with one the path relative to the picture folder |
| size | `*` for any size, or `WIDTHxHEIGHT`: the row applies only to pictures of that size |
| anchor | `top-left`, `top-right`, `bottom-left` or `bottom-right`: the corner x and y are measured from |
| x, y | pixels from that corner's two edges to the rectangle's nearest sides |
| width, height | the rectangle's size |
| what | `clock: ...`, `date: ...`, `database: ...`, `path: ...` or `machine state: ...` |

A malformed row stops the builder with its line number (never skipped). A row whose name matches a picture but whose size does not is
listed under that picture on the page ("Mask row for another size, not applied"), so a window that changed size gets its masks looked at.

**The rows** (15), measured on the WINCHK-5 pictures of 2026-11-16 (`.git/od-tools/test-results/winchk-pairs-3b`, 100 %, 96 DPI) by
finding where the pairs differ and reading the pictures; each checked by building the masked comparison of the pair again:
- Help > About (`A2-about.png`, `A7-release-about.png` 644x444; `A6-nocustomborders-about.png` with the standard frame, 5 pixels lower): the
  Database Name value. With it masked the About box's remaining differences are the runtime's (the logo, the title icon).
- Help > About > Diagnostics (`A2-diagnostics.png`, 718x683, a fixed-pitch text box, 14-pixel lines): the program path, the workstation's and
  the server's date and time and the seconds between them, free memory, free disk (two lines), the image folder (it names the copy) and the
  database name. Left unmasked on purpose: the runtime, process and framework lines, which are real differences.
- The Appointments module (`A1-main.png`, `A5-nodpi-appts.png`, `B1-appts-*.png`, any size, anchored top-right because the panel keeps its
  width when the window grows): the date above the month calendar, the calendar (today's mark and the month) and the "Today:" line.

**Not masked, for UI-0c.** Some pictures change with the day in ways a rectangle cannot hide without hiding what the gallery is for: the
Appointments module's week view shows the week of the run (its grid differed almost everywhere between two days), the "now" line crosses the
grid when the harness runs during the shown hours, and the dashboard's 30-day graph (`G1.png`) moves with the date. UI-0c's group U takes its
pictures at fixed window sizes (1366x768 and 1920x1080) and either fixes the date the harness shows or adds rows for its own pictures; the
masks of the main window above already work at any size.

**What a mask does not do.** The pictures on the page are the real ones, masks only leave pixels out of the hash, the share of changed
pixels and the difference picture. A masked area still shows in the before and after pictures, so a restyled calendar is still seen by the
owner even where U0 cannot see it.

## 4. Clip notes

A TSV: the picture (its relative path or its file name), then the note; more fields are joined with "; " (so a walker can write
`picture, control, extra pixels, fixable` and they read as one note). `#` lines and a header whose first field is `picture` are skipped.
The builder takes `-ClipNotes`, or `clip-notes.tsv` in the after folder. UI-0b's scan and UI-0c's walker write their reports; the lead (or
UI-0c's walker) turns the rows of the pictured windows into this file. Each note shows under its picture, the overview counts them, and the
page's "Show: with clip notes" filter lists those pictures.

## 5. The page and the answers

`index.html` stands alone (relative links to `before/`, `after/`, `diff/`; no script or style from the network) and is ASCII:
- **At 1:1 device pixels.** Each picture's CSS size is its size divided by `devicePixelRatio` (set by the page's script, again when the
  scaling changes), with `image-rendering: pixelated`, so a 1-pixel line stays 1 screen pixel at 125 % or 150 %. **Zoom** cycles 1:1, 2x and
  4x for every picture; a click on one picture doubles just that one. Wide pictures scroll inside their frame; the page itself does not.
  The line under the bar says the screen's scaling and how pictures are shown. Without the script the pictures show at their size in CSS
  pixels.
- **Order and grouping.** An overview table of every picture, the changed ones first (most changed first), then those only in the after
  set, those only in the before set, and the unchanged ones; then one section per group (the picture's first folder, or the letters before
  the first digit of its name: `A1-main.png` is group A, `U12-claims.png` group U), in the same order inside each group. An unchanged picture
  shows folded (the after picture only).
- **Filters.** Group; show all, changed, only in one set, the same, with clip notes, not approved yet, not answered; a name search.
- **Per picture.** Status and % changed, the sizes, changed and compared pixel counts with the rectangle around the changes, the masks
  applied, the masked pixel SHA-256 (first 12 digits, all of it on hover), whether the owner approved exactly this picture before (from
  `approved.tsv`: "approved", "changed since approval", "not approved yet"), mask rows for another size, the clip notes; then before, after
  and the difference.
- **The Approve/Change column.** In the overview table and under each picture (the two stay in step): Approve or Change, and a note. The
  answers stay in the browser (localStorage, per gallery and commit; the page works without it). **Download answers** saves
  `answers-<gallery>.tsv`, **Copy answers** puts the same text on the clipboard:

      picture  size  sha256  gallery  commit  date  answer  note

  The first six columns of the Approve rows are `approved.tsv` rows as they are (`picture  size  sha256  gallery  commit  date`), so the lead
  appends those rows to `tests/WindowChecksTests/UiApproved/approved.tsv` and the gallery's entry to `docs/owner-questions.md` (plan,
  section 8). When the lead publishes the gallery as a page whose answers reach the lead some other way, the same columns apply.

`gallery.tsv` columns: picture, group, status (`changed`, `same`, `only after`, `only before`), before_size, after_size, changed, compared,
masked, percent (4 decimals), sha256_after, sha256_before (both masked), approved (`same`, `differs`, `none`), clip_notes (their count).

**Reused, not invented.** From the WINCHK-5 pairs page (WINCHK-HEADLESS.md, WINCHK-5 section 7): the overview most changed first, the
groups, the difference picture's colours, the page's colours and its plain-words summary. From the tooth chart galleries
(`tests/ToothChartImageTests/Support/WpfGallery.cs`, `tools/New-ToothChartGallery.ps1`): the marker file and the rule that only a gallery
folder is emptied, the group filter, the pixel SHA-256 over 32-bit pixels without padding, the grey fade of the difference picture, the
pixelated enlargement, and the zip with forward-slash entry names (the same function).

## 6. Tests

`tests/ReleaseFlowTests/UiGalleryScriptTests.cs` (no program, no database; every picture drawn into a temp folder):
- the rule written out by hand equals `GetMaskedPixelSha256`; a masked pixel does not change it, any unmasked pixel (alpha included) does,
  another picture's mask does not apply; 24-bit and 32-bit files and the bitmap give the same hash, and reading keeps no file open;
- the four anchors, clipping and an empty rectangle; name matching with and without a folder, wildcards, any case; sized rows and rows of
  another size;
- malformed mask rows and approval rows are refused with their line number; the repository's mask file parses and covers the clock,
  dates and the database name; `approved.tsv` holds its header only, and its rows parse with the last approval of a picture counting;
- the comparison: counts, masks in either picture, the strip only one picture has, the threshold, and the difference picture's colours;
- the builder end to end: the copies, the difference pictures, `gallery.tsv` (statuses, counts, %, order, approvals, clip notes), the
  hash the script wrote equal to the one computed in the test process, `index.html` (1:1 device pixels, pixelated, zoom, filters, order,
  Approve/Change, the answers' columns, encoded notes, no local path, ASCII) and the zip (forward slashes, every file);
- the builder refuses a folder that is not a gallery and one that overlaps a picture folder, empties and rewrites an earlier gallery,
  honours `-NoZip` and `-Threshold`, works without a before folder, and refuses a run without `-After`; its helpers dot-sourced;
- the ASCII and no-secret checks of the owner's scripts (`LaunchCheckScriptTests.OwnerScripts_AreAscii_AndHoldNoSecret`: no thumbprint,
  password, key, address other than 127.0.0.1 and 192.0.2.x, host name or developer-PC path) on the script, the mask file and the rule's
  source, and the script parses without errors. The builder is a lead's and agent's tool, not one of the owner's scripts, so it has its own
  test with those checks rather than a place in that list (which also asserts the owner-only header).

## 7. Checks

- A full Debug x64 lane build of `OpenDental.Local.slnf` at the end: 0 errors.
- Targeted ReleaseFlowTests (with NoHqTests): passed (`.git/od-tools/test-results/ui0a-final.txt`).
- `tools/Test-NoEolFlips.ps1 -Base main`: no changed text file changed its line ends.
- A trial on six WINCHK-5 pairs and two single pictures (outside the repository): the masked About and Diagnostics pictures keep only the
  runtime's differences; the page was rendered once in a headless browser at 150 % scaling to check that its script sizes the pictures to
  1:1 device pixels (no window was shown). A trial over all WINCHK-5 pictures: 145 pairs, 126 changed at threshold 0 (WINCHK-5 counted
  92 at 16; those pairs compare .NET Framework with .NET 10, so most differences are the runtime's, not noise), 19 the same, 37 in one set.
