# UI-OVF: toolbar overflow and WPF keyboard focus

Package UI-OVF (the lead, 2026-11-24): two open items of the UI refresh ([ui-refresh-plan.md](../specs/ui-refresh-plan.md) section 7).
Branch `ui/uiovf-1` from `dd1d262f8`. Gallery G-UI3-uiovf (`.git/od-tools/test-results/G-UI3-uiovf/`), audit
[UI-OVF-audit.md](UI-OVF-audit.md).

## 1. Toolbar overflow

Until now a toolbar did not wrap or scroll: what did not fit was cut off at the right edge (UI-6, UQ-9; `Data/toolbar-overflow.tsv`).
Now the buttons that do not fit go into a **» overflow button** at the toolbar's right end.

- **Where.** `ToolBarOD` (every WinForms toolbar of the program), the WPF `ToolBar` (the Imaging module's main toolbar) and every stock
  `ToolStrip` the program draws with `UiToolStripRenderer` (Windows Forms' own overflow, restyled).
- **The rule.** Only when a button (not only a separator) runs past the right edge. The overflow button takes 24 px at 96 DPI
  (`UiToolStripRenderer.WIDTH_OVERFLOW_BUTTON96`, scaled) at the right end; every button from the first one that does not fit left of it
  goes into its drop-down, and a separator just before the cut goes with them. The buttons keep the width formula and order of UI-6
  (section 3.3); the overflowed ones are neither drawn nor hit. Nothing changes on a toolbar whose buttons fit.
- **The look.** The tool button's shapes (transparent at rest, `SubtleHover` on hover, `SubtlePressed` pressed and while its drop-down is
  open, 4 px corners) and the glyph », two 12 px Fluent `ChevronRight` (UI-7a) 4 px apart, centred, in `Text`
  (`UiToolStripRenderer.DrawOverflowGlyph`; the WPF button draws the same two paths). The tool strips' own overflow button
  (`OnRenderOverflowButtonBackground`) draws the same shape and glyph instead of the professional renderer's gradient and arrow.
- **The drop-down.** Built again each time it opens, right-aligned under the button. Each overflowed button is an item with the same text
  (an icon-only button: its tooltip), picture (the icon at the menu's 16 px, scaled), enabled state and tooltip; a toggle button is checked
  while pushed; a click does what a click on the button does (a toggle flips first; `ButtonClick` or the button's own handler). A label is a
  disabled item; a drop-down's notification text shows at the item's right; an error (`IsRed`) button's text is `Danger`. Separators stay
  between groups (none first or last). The page navigation (print previews) is left out: its text box cannot move into a menu.
  - WinForms drop-down button: the item's sub-menu **is** the button's own `ContextMenuStripDropDown` (same items, handlers and `Opening`
    event), given back to the button when the drop-down closes (after the click that closed it) or is built again; a click on the item itself
    is the button's main part. A drop-down button whose arrow opens a window (`ButtonDropDownClick`, the Account module's Statement since
    P262-BILLING) gets one sub-item, "<text>...", for the arrow.
  - WPF drop-down button: a WPF menu item cannot have two parents, so the sub-menu is the button's main part, a separator, then copies of
    its menu's items (the menu's `Opened` handlers run first, so a menu filled on opening, like the Camera menu, is filled); a copy's click
    raises the original item's `Click` (a checkable one flips first).
- **Keyboard.** `ToolBarOD` was already a tab stop (a UserControl without child controls) with no keys of its own. While it has the focus
  and Windows shows keyboard cues (after Tab), Left and Right move between its enabled buttons and the overflow button (wrapping), Home and
  End go to the first and the last, Enter and Space click (open the overflow button's drop-down with its first item selected), Down opens a
  drop-down button's menu. The 2 px `Accent` focus ring (section 3.6) shows on that button. A toolbar focused by a mouse click leaves Enter to
  the window's default button and the arrows to the next control, as before. The WPF toolbar is one tab stop (`TabNavigation=Once`); its
  buttons are focusable with the shared focus visual style, Left/Right/Home/End move between them and the overflow button, Enter and Space
  click, Down opens a drop-down button's menu or the overflow drop-down.
- **Accessibility.** `ToolBarOD` has an accessible object of role `ToolBar` whose children are its buttons on the bar (name: the text, or
  the tooltip of an icon-only button; role push button, drop-down button, check button or static text; states unavailable, checked,
  focused, has popup, hot) and the overflow button: name "More buttons" (`Lan.g`), role `ButtonMenu`, state has popup, default action
  "Open". Moving with the arrows raises the accessibility focus event, so a screen reader announces each button. The overflow button's
  tooltip is "More buttons" too. The WPF overflow button is a focusable `ToolBarButton` with that tooltip; the tool strips' overflow button
  is Windows Forms' own (accessible as before).

### toolbar-overflow.tsv

`tests/UiRefreshTests/Data/toolbar-overflow.tsv` keeps its rows (toolbar, font, window width at 100%, captions) but they now name the
buttons **in the toolbar's overflow button** (the toolbar's own decision: `ToolBarOD.GetButtonsOverflowed`, the WPF `ToolBarItemsPanel`),
which are reachable, not lost. `ToolBarFitTests` also checks that each of them is an item of the drop-down, that no button left on the bar
runs under the overflow button, and that the main menu (which has no overflow button) fits. Because the overflow button takes 24 px, two
rows gained a button: the Chart toolbar at 1366 px in today's font (`hl7DefMedLab.Description | Layout`) and the Imaging toolbar at
1024 px (`Copy | Paste | Forms`).

## 2. WPF keyboard focus

The WPF `Button`, `CheckBox`, `RadioButton` (and the toolbar buttons, above) were not focusable (UserControl's default; UI-4a section 3 left
it as a behaviour change). Now they take the focus with Tab like the WinForms ones and show the program's focus ring only with keyboard cues
(the shared `FocusVisualStyle` of UI-4a, which WPF draws only when the focus came from the keyboard: UI-4a's rule, plan section 3.6).

- **Keys.** Enter and Space click a focused button and the key is handled, so a Frm's own Enter (its bubbling `KeyDown`, FrmODBase's
  pattern) does not run as well: the focused button wins, as in WinForms. Space toggles a check box (unchecked, checked, indeterminate when
  three-state). Space checks a radio button; Up/Left and Down/Right move to the previous or next enabled radio button of the same parent and
  check it (wrapping). Esc is the window's, unchanged (FrmODBase closes); Enter on any other control is unchanged.
- **Frms whose PreviewKeyDown takes Enter** (it runs before a focused control): two in the program. `FrmApptProvPrompt` now leaves Enter to
  a focused button (else Enter would answer the default and then the focused button); `FrmCaptureSession` takes Enter only while tooth
  numbers are typed, and handles it. A test keeps the list.
- **Tab order (`UiTabOrder`).** WPF tabs in tree order when no `TabIndex` is set, and the converted Frms list their controls in no particular
  order. Measured on the 155 Frms of `WpfControlsOD/Frms` (XAML order against reading order, top to bottom and left to right): with only the
  inputs as tab stops 93 tabbed in reading order; with the buttons, check boxes and radio buttons added only 42 would have. So
  `FrmODBase`, once loaded (after `SetFocusToFirst`, so the first focused control is the same as before), gives a Frm that sets no tab order
  of its own a reading order: rows top to bottom (tops within 8 DIP are one row), left to right in a row. `GroupBox`, `Panel` and
  `PanelAutoScroll` (their own tab groups) are ordered inside the same way and take their place by their top left corner. A Frm that sets
  `TabIndexOD` (49 XAML files) keeps its order (its new stops follow the ordered ones in XAML order); office content (the eForm fill surface,
  `ContentLook.IsPinned`) keeps its fields' own order. A control hidden when the order is made takes the index of the visible one before it
  in the tree, so it tabs next to it once shown. A list's items and a tab control's pages keep their own order.

## 3. Tests

`UiRefreshTests`: `Toolbars/ToolBarOverflowTests` (ToolBarOD: fits, separators only, the overflowed buttons and the drop-down's items at
100% and 150%, each item does what its button does and the button's own menu is given back, the keys, the accessible objects, the glyph and
nothing drawn past the last button; a stock tool strip's overflow button; the WPF toolbar's overflow and drop-down; the WPF toolbar button's
keys), `WpfFocusTests` (tab stops with the focus visual, Button/CheckBox/RadioButton keys, the reading order, `UiTabOrder.Apply` with a nested
group, hidden controls, an own order and office content, a real Tab walk in a window shown off screen (inconclusive when the session gives
it no keyboard focus), the Frms' PreviewKeyDown Enter list), `Toolbars/ToolBarFitTests` (above), `Toolbars/OverflowGallery` (opt-in).
`Data/coverage.tsv`: `UiTabOrder` (no-drawing).

Targeted run (x64, 2026-11-24, `.git/od-tools/test-results/uiovf-run3.txt`): UiRefreshTests 337 passed, 49 skipped (the WPF
pictures of a disconnected session and opt-in galleries), 0 failed; WindowChecksTests 30/44 skipped; UnitTests 1728/3 skipped; PrintGuardTests
19/1 skipped; NoHqTests 127; the real Tab walk passed.

## 4. Gallery G-UI3-uiovf

`OPMS_UI_OVF_GALLERY=<folder>` with a Targeted UiRefreshTests run; `tools/New-UiGallery.ps1` makes the page. For each toolbar and font of
`toolbar-overflow.tsv` at 1024 px, at 100% and 150%: `<toolbar>-<font>-<scale>-before.png` (laid out wide and cut to the width: main's
picture), `-after.png` (the overflow button) and `-after-open.png` (its drop-down drawn under it, right-aligned). `wpf-focus-<scale>.png`: a
WPF button, the default button, a check box, a radio button and a toolbar's overflow button with the focus ring (the shared style laid over
each, as WPF's adorner draws it). The WPF pictures (the Imaging toolbar and `wpf-focus`) need a session WPF draws in: this run's session was
disconnected, so `notes.txt` lists them as not drawn; a connected run of the same command draws them.
