# Icons

The program's icons are Fluent UI System Icons (Microsoft, MIT licence), drawn as vectors from path data compiled into `CodeBase.dll`.
No icon font and no picture files. The plan is `docs/specs/ui-refresh-plan.md`, sections 3.5, 4.1 to 4.3; the pipeline came with package
UI-7a (`docs/port-notes/UI-7a.md`).

## The files

| File | What |
|---|---|
| `icons/manifest.json` | every icon the program draws: one row per purpose (below) |
| `icons/custom/` | the program's own SVGs, for purposes Fluent has no icon for (none yet; the dental icons are UI-8's) |
| `third-party/fluentui-system-icons/icons/` | the Fluent SVGs the manifest names, unchanged, in the npm package's layout `<name>_<size>_<style>.svg` |
| `third-party/fluentui-system-icons/LICENSE`, `SOURCE.txt` | the MIT licence; the version, download, integrity hash, commit and each file's SHA-256 |
| `tools/New-IconData.ps1` | the generator: manifest and SVGs in, `CodeBase/UI/Icons/IconData.g.cs` out (deterministic, committed) |
| `CodeBase/UI/Icons/IconData.g.cs` | generated: the `UiGlyph` enum and the table of designs (path data in design pixels) |
| `CodeBase/UI/Icons/UiIcons*.cs` | the renderer: the design for a slot, GDI+, `GetBitmap`, `GetImageList`, WPF, and `IIconPathSink` for Direct2D |
| `OpenPmsDental/UI/Direct2d/Direct2d.cs` | `FillGlyph`, `FillIcon` (Direct2D path calls) and `DrawGlyph` (Direct2D into a bitmap, GDI+ with `IconLibrary.OnlyGDI`) |

The version is the newest `@fluentui/svg-icons` on the day it was taken (the owner's rule: always the latest). Today: **1.1.346**
(published 2026-11-13, commit `ba34950ee`, release commit `67e6dec1e`). The repository has no GitHub Releases; its versions are the npm
packages. The approved download is `.git/od-tools/downloads/ui7a-fluentui-svg-icons-20261117/` (SOURCES, SHA256SUMS, MANIFEST).

## A manifest row

```json
{ "enum": "UiGlyph", "name": "ChevronDown", "fluent": "chevron_down", "style": "regular", "sizes": [ 12, 16, 20, 24, 32, 48 ],
  "role": "Text", "purpose": "drop-down arrows of combo boxes, drop-down buttons and menus; expand (UI-4a to UI-6)" }
```

- `enum`: `UiGlyph` (the control glyphs; the generator writes this enum, in manifest order after `None`) or `EnumIcons` (the toolbar
  and module icons; UI-7b adds those rows, keyed by the member's name, and keeps the enum in `IconLibrary.cs`).
- `name`: the enum member. One row per purpose: a purpose drawn in many places is one row.
- `fluent`: the Fluent file name stem, or `custom`: a stem under `icons/custom` (`<custom>_<size>.svg`). Exactly one of the two.
- `style`: `regular`, or `filled` for "on" states (the selected module, a toggled toolbar button), section 4.3.
- `sizes`: the design sizes compiled in, rising: 12, 16, 20, 24, 32, 48 (section 3.5; the 24 design serves 24 to 31 px, so 28 is not
  used). Take every size Fluent has for the icon. A slot whose design is missing draws the largest smaller one, centred; a slot below
  the smallest design draws the smallest shrunk to the slot.
- `role`: the default colour token (`Text`, `Accent`, `Danger`, `Success`, `Warning`). The drawing call may pass its own colour: a white
  check mark on the accent, the title bar's text colour on a caption button, `TextDisabled` for a disabled icon.
- `purpose`: where it is drawn.

## Adding or changing an icon

1. Find the icon in the npm package's `icons/` folder of the approved download (or a newer one: see "A new version").
2. Copy its files for the sizes into `third-party/fluentui-system-icons/icons/` unchanged, and add their SHA-256 lines to `SOURCE.txt`.
3. Add the manifest row.
4. Run `powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-IconData.ps1` and commit the manifest, the SVGs, `SOURCE.txt`
   and `IconData.g.cs` together. `tools\New-IconData.ps1 -Check` (and `UiRefreshTests`) fails while they disagree.

The generator refuses an SVG that no row names, a missing file, an SVG whose `viewBox` is not `0 0 <size> <size>`, and SVGs with
anything but `<path d>` elements (groups, clip paths, transforms, fills, even-odd rules). In 1.1.346, 49 of the regular and filled
SVGs set a fill, an opacity or a transform and 5 hold groups and clip paths; none uses the even-odd rule. Teach the generator what such
an icon needs when one is chosen, or pick another.

## A new version

Download the newest `@fluentui/svg-icons` into a new dated folder under `.git/od-tools/downloads` with SOURCES, SHA256SUMS and MANIFEST,
copy the manifest's files again, update `SOURCE.txt`, `fluentVersion` in the manifest and the version in `THIRD-PARTY-NOTICES.txt`,
run the generator, and compare the drawings (UiRefreshTests draws every row; the gallery shows them).

## Drawing

```csharp
UiIcons.Draw(g,UiGlyph.ChevronDown,rectangleSlot,EnumUiColor.Text);          //GDI+, the slot in device pixels
UiIcons.DrawDisabled(g,UiGlyph.ChevronDown,rectangleSlot);                     //TextDisabled
Bitmap bitmap=UiIcons.GetBitmap(UiGlyph.Dismiss,16,LayoutManager.ScaleMy(),color); //a new 32-bit bitmap at the DPI size; the caller owns it
ImageList imageList=UiIcons.GetImageList(arrayGlyphs,sizePixels,EnumUiColor.Text); //rebuild when the DPI changes
direct2d.FillGlyph(UiGlyph.Checkmark,rectangleSlot,color);                     //between BeginDraw and EndDraw
Direct2d.DrawGlyph(g,UiGlyph.Checkmark,rectangleSlot,color,colorBack);         //Direct2D into a bitmap; GDI+ with NoD2D.txt
UiIcons.DrawWpf(drawingContext,UiGlyph.ChevronDown,rectSlot,brush,scale);      //WPF; scale = device pixels per DIP
Geometry geometry=UiIcons.GetGeometryWpf(UiGlyph.ChevronDown,12,scale);        //frozen, for a Path in XAML code
```

## The mapping (section 4.3)

One row per purpose. UI-7b (the `EnumIcons`), UI-8 (the dental icons) and the UI-9 slices (pictures to icons) add their rows here and to
the manifest; G-UI1 part 3 shows them.

| Purpose | Today | Icon | Source | Package |
|---|---|---|---|---|
| Caption button: minimize | the control's own drawing | Subtract (regular) | Fluent | UI-7a (drawn from UI-3) |
| Caption button: maximize | the control's own drawing | Maximize (regular) | Fluent | UI-7a (UI-3) |
| Caption button: restore | the control's own drawing | Square Multiple (regular) | Fluent | UI-7a (UI-3) |
| Caption button: close; close marks of tabs and panels | the control's own drawing | Dismiss (regular) | Fluent | UI-7a (UI-3) |
| Up arrows: spin and scroll buttons, collapse | the control's own drawing | Chevron Up (regular) | Fluent | UI-7a (UI-4a to UI-6) |
| Drop-down arrows; expand | the control's own drawing | Chevron Down (regular) | Fluent | UI-7a (UI-4a to UI-6) |
| Previous: month calendar, tab scroll | the control's own drawing | Chevron Left (regular) | Fluent | UI-7a (UI-4b, UI-4c) |
| Next: month calendar, tab scroll, sub-menus | the control's own drawing | Chevron Right (regular) | Fluent | UI-7a (UI-4b, UI-4c, UI-6) |
| Check box check mark; checked menu items | the control's own drawing | Checkmark (regular) | Fluent | UI-7a (UI-4a, UI-6) |
| Indeterminate check box | the control's own drawing | Line Horizontal 1 (regular), "the dash" | Fluent | UI-7a (UI-4a) |
