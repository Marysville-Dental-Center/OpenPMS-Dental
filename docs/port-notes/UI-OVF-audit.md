# UI-OVF audit (gallery G-UI3-uiovf)

The pictures of `.git/od-tools/test-results/G-UI3-uiovf/` (OverflowGallery, a Targeted UiRefreshTests run with `OPMS_UI_OVF_GALLERY`) were
opened before the owner sees them: the ToolBarOD toolbars that overflow at a 1024 px window (family, chart and the image toolbar, today's
font and Segoe UI 9, 100% and 150%; 36 pictures). Package notes: [UI-OVF.md](UI-OVF.md).

## First pass

| Picture(s) | Seen | Fix |
|---|---|---|
| `*-after-open.png` | the overflow button drawn at rest while its drop-down is open; the program shows it pressed | the gallery sets the pressed state as `ShowOverflowMenu` does (ToolBarOD and WPF) |
| `*-before.png` | the last buttons cut off at the right edge (chart: "Commlo..."), as on main | none: that is the before |
| `*-after.png` at 100% and 150% | the » glyph centred in the 24 px (36 px) button, in `Text`, no button drawn under it or between the last button and it | none |
| `*-after-open.png` | the drop-down right-aligned under the button, the items in the toolbar's order with their icons (Commlog), the menu's font and icon size scaled at 150% | none |

## Not drawn in this run

The WPF pictures (the Imaging toolbar at 1024 px in today's font, before, after and open; `wpf-focus-100/150`: the focused button, default
button, check box, radio button and toolbar overflow button) were not drawn: the session was disconnected and WPF draws nothing then
(`notes.txt`). The WPF overflow and focus behaviour is checked by `WpfToolBar_Overflow_*`, `WpfFocusTests` (including a real Tab walk in a
window shown off screen, which passed). A connected run of the same command draws the missing pictures for the owner's look.
