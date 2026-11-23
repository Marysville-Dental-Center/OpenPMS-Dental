# Port note: IMG-P2 (reassign an image's teeth)

Why: the owner's imaging design (approved 2026-11-22, section 3 and decision D6; D5: standard Open Dental permissions). P2 of four phases,
after IMG-P1 (merged as fc35c0ab7). No schema change, no designer file touched.

## Behaviour

- **Teeth** opens the tooth picker for one image: the selected mount item, or the single image showing. It is reachable from:
  - the Imaging toolbar's Teeth button (after Info; the tooth icon; enabled when Flip is, so for one image);
  - the right-click menu's "Teeth..." (after Info; hidden when no mount item is selected);
  - the T key in the module;
  - the T key in the image viewer window (IMG-P1), which then shows the new teeth.
- **The tooth picker** (`OpenPmsDental/Forms/FormToothPicker.cs`):
  - Layout: four rows as a chart shows them. Permanent upper 1-16 and lower 32-17 at the top and bottom, primary upper A-J and lower T-K in
    the middle, so mixed dentition works. The labels are in the office's numbering (`PrefName.UseInternationalToothNumbers`: Universal, FDI,
    Haderup, Palmer).
  - Choosing teeth: click to toggle a tooth, or drag along a row to set the range to the opposite of the first tooth. UR, UA, UL, LL, LA and
    LR toggle a quadrant's permanent teeth or an arch's six anterior teeth; Clear clears.
  - The text box shows the selection as Item Info does ("13-15") and takes typed teeth, read by `Tooth.ParseRange` as Item Info reads them.
    An error shows in the hint line, in Danger.
  - For a mount item, the slot's own teeth are underlined (Warning), and "Also change this slot's teeth in this mount" (unticked by default)
    also sets the mount item's teeth. The mount's template (MountItemDef) is never changed from here.
  - Enter is OK and Esc is Cancel. The window only returns the result.
- **Saving** (`ControlImageDisplay.EditTeeth`):
  - The same permission check as the other edits here: `ImageSignaturePermissions.IsAuthorizedToEdit` (ImageEdit, plus SignedImageEdit for
    a signed image). These are the standard permissions, set per user group.
  - Logging is the same too: `SecurityLogs.MakeLogEntry(EnumPermType.ImageEdit, ...)` with the old and new teeth, plus
    `LogSignedImageEdited`.
- **An image moved to another slot takes the new slot's teeth** (design D6). This applies to the swap and to the move to an empty slot
  (`ImageTeeth.GetTeethAfterMove`, `OpenPmsDentalBusiness/Imaging/ImageTeeth.cs`).
  - It replaces the old rule, under which teeth followed only when the user had not changed them.
  - When the new slot has no teeth, the image keeps teeth the user gave it and drops the ones the old slot gave it.
  - The drag that moves images stays in Adjust mode: in Pan mode a drag pans the mount. The design's "in any mode" is left out for that
    reason.

## Files

| File | Change |
|---|---|
| `OpenPmsDental/Forms/FormToothPicker.cs` | new; the controls are made in code, so the rollout's designer data needs no rows |
| `OpenPmsDentalBusiness/Imaging/ImageTeeth.cs` | new: GetTeethAfterMove, JoinTeeth, SplitTeeth |
| `OpenPmsDental/User Controls/ControlImageDisplay.cs` | the "Teeth" region (EditTeeth, ToolBarTeeth_Click, GetTeethIdx); the menu item made in the constructor; the T key; the three moves |
| `OpenPmsDental/Main Modules/ControlImages.cs` | TB.Teeth, the button, its enabling, ToolBarTeeth_Click |
| `OpenPmsDental/Forms/FormImageViewer.cs` | the T key, "T: teeth" in its info line |

## Tests

- `tests/ImagingTests/ImageTeethTests.cs` (4): the move rule, the list's order and format.
- `tests/ImagingTests/TeethWiringTests.cs` (6): source checks of the button, menu, keys, permission and logs, moves, and the picker.
- `tests/ImagingTests/WiringTests.cs`: the edit-check counts include Teeth (12 IsAuthorizedToEdit, 13 LogSignedImageEdited).
- `tests/S3aM1Tests/M2ModuleMenuTests.cs` `ControlImageDisplay_PanelMenu`: checks Teeth after Info, then compares the rest with the designer
  as before.
- `tests/UiRefreshTests/ImageViewerGallery.cs`: the picker's pictures (mount item, single image, mixed dentition, FDI). The client area is
  laid out with `SetPanelClient`, each control drawn once.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingTests | 99 | 99 |
| ImagingCaptureTests | 806 | 806 |
| BIMGTests | 28 | 28 |
| VPEARL1Tests | 23 | 23 |
| AttributionTests | 54 | 54 |
| S3aM1Tests | 60 (8 skipped) | 68 |
| NoHqTests | 127 | 127 |

UiRefreshTests fails the same 19 tests as base 647febb65 (the WPF drawing tests on a disconnected desktop session).

## Gallery G-IMG2

These are after pictures only. Item Info's text box is the "before", and it remains in Item Info.

Audit round 1 found the hint line wrapping onto a second line and cut off; it is shorter now. The colour seen on "LL" and "LR" was checked
pixel by pixel: it is ClearType's colour fringe on dark text, the same text colour as the other buttons.

Round 2: no findings.

## Review (one independent review, 2026-11-23)

Fixed:

- The tooth labels' font took the monitor's dpi twice. It now takes only the zoom (`ScaleFontODZoom`), as LayoutManager's rule says.
- `Tooth.ParseRange` read the end of a range with `ToInt`, which maps primary C to permanent 6. So "A-C", which DisplayRange itself writes,
  was refused. That hit the picker's text box and Item Info, which then could not save an image with three primary teeth in a row. It now
  reads the end by ordinal, as it already read the beginning. Tested in `ToothRangeDbTests` (round trips, and backward ranges still
  refused).
- Enter on a focused button other than OK clicks that button. Elsewhere, Enter is OK, reading typed teeth first.
- The picker is placed beside the pointer in its own Load, after FormODBase has sized it for the monitor's dpi, so it stays on the
  screen at 150%.
- A change to the slot's teeth is logged too (ImageEdit, "Mount Edited: ... slot N teeth changed from ... to ...").

Left as they are:

- Haderup labels contain "-", which ParseRange takes for a range. Typing Haderup lower teeth fails, as it already did in Item Info.
  Clicking works.
- "Teeth..." shows on an empty slot and does nothing there, as Flip and Rotate do.
- The teeth a move changes (D6) are not logged, as moves were not before.
