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

## Fix round (`ui/uiovf-2`)

The review found that the drop-down's pictures did not show what ships: the gallery's drop-down buttons had no menu, so Family's Discount
Plan (a drop-down button with `contextMenuDiscount` in the program) was a plain item, and no sub-menu was ever drawn. The gallery now gives
each drop-down button a small menu ("Menu item one", "two", a separator, "three") and draws one more picture per toolbar,
`-after-open-sub.png`, with the first drop-down button's menu item selected and its sub-menu open beside the drop-down. Run
`.git/od-tools/test-results/uiovf2-run2.txt` (x64) wrote 44 ToolBarOD pictures (family and chart: before, after, after-open,
after-open-sub; the image toolbar has no drop-down button, so no -sub picture, `notes.txt`).

| Picture(s) | Seen | Fix |
|---|---|---|
| `family-*-after-open.png`, `chart-*-after-open.png` | a drop-down button is now two items: the main part ("Discount Plan", "Layout") and, under it, the same text with the sub-menu arrow; the arrow is the menu's own chevron in `Text`, centred on the text line at 100% and 150% | none (by design since the fix round: Enter, Space, Right and a click on an item with a sub-menu open it in Windows Forms, so the main part needs its own item) |
| `family-*-after-open-sub.png`, `chart-*-after-open-sub.png` | the menu item selected (the menus' selection fill), the sub-menu opened at the drop-down's right edge, its top level with the selected item's top, its items in the button menu's font and size at 150%, the separator inset as in every menu | none |
| `chart-*-after-open-sub.png` at 1024 px | the sub-menu runs past the toolbar's right edge in the picture; on screen Windows Forms opens it to the left when there is no room on the right | none (picture only) |

Not drawn again: the WPF pictures (`imaging-today-100/150-before/after/after-open/after-open-sub`, `wpf-focus-100/150`). The run's session
was still disconnected (`query session`: this user's session `Disc`), where WPF's `RenderTargetBitmap` draws nothing (`ControlPictures.RendersWpf`
is false; `notes.txt` says so). They need the same Targeted x64 UiRefreshTests run with `OPMS_UI_OVF_GALLERY` from a connected session, and
an audit of those 10 pictures here, before the owner looks. The WPF behaviour they would show is covered by tests: the overflow drop-down and
its sub-menus (`WpfToolBar_Overflow_*`), the tab stops (`ToolBar_TabStops_*`) and a real Tab walk into a narrow WPF toolbar in a window
shown off screen (`ToolBar_RealTabWalk_*`, passed in this run).
