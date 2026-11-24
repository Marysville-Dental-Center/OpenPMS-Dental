# UI-FOCUS: the input frame's flash on a focus change

The owner, 2026-11-23: "There's shaking when a text box is in focus." Package UI-FOCUS (branch `ui/uifocus-1` from `42cc8ea3f`): find
and fix it in `UiInputFrame` (UI-12's combo-box frame over the stock inputs, `OpenPmsDental/UI/UiInputFrame.cs`) and the looks it attaches.

## Cause

`UiInputFrame` painted the frame *over* Windows: after Windows' own `WM_NCPAINT` (the text box's 3-D edge, in its window frame) and
`WM_PAINT` (the up-down box's, date picker's and combo box's border, drawn in their client area) returned, the frame was painted on the
window DC. Between the two paints Windows' stock border is in the window's DWM surface, and a composition that falls there shows it on
screen for a frame.

That alone would be a rare flicker. What makes it a shake is Windows 11's focus animation: the themed single-line edit and the themed combo
box fade their own focus line in (and out) over about 200 ms, as 4 or 5 repaints, each a `WM_PAINT` and `WM_NCPAINT` (edit) or a `WM_PAINT`
(combo box) with only the border in the update region. Each repaint drew the stock border with its blue line at that step, and the frame
went over it again: the bottom edge alternated between Windows' animated blue line and the frame's Accent line (with the square grey edge
against the frame's 4 px corners) 4 or 5 times on every focus change, in and out.

On top of that, the frame's own focus handler (`GotFocus`/`LostFocus`) redrew the whole window with `RedrawWindow(RDW_INVALIDATE |
RDW_FRAME | RDW_UPDATENOW)`: one more full paint cycle, Windows' border and text first, the frame after. For an up-down box (whose
focus is in its child edit) and a rich text box, which Windows does not repaint on a focus change at all, this cycle was the only flash.

What was checked and is not a cause: the text does not move (the edit's text origin, font and margins are the same focused and unfocused;
the test below compares every pixel above the focus line); the 2 px focus line is drawn inside the frame's 2 px edge only (the frame's clip
excludes the inner rectangle, at 150% too), never over the text; multi-line text boxes have no focus animation, but their focus change
repainted the whole window frame, so the stock scroll bar flashed under UiScrollLook's for a frame.

## Evidence

A probe (a stock `TextBox` and the others in a shown form with visual styles, a subclass that reads the border's pixels through the window
DC right after Windows' default painting and paints a marker frame after it, as `UiInputFrame` did), on this Windows 11 machine at 100%:

| Input | Paints on a focus change (in / out) | Border after Windows' paint |
|---|---|---|
| single-line text box | `WM_PAINT` + `WM_NCPAINT` x4 / x5 | the stock edge, its bottom row `838383` → `9D774C` → `B66C16` → `C06700` (the fading blue line), each time |
| combo box (DropDown) | `WM_PAINT` x4 / x5 | the stock border `8D8D8D` → `AB8452` → `C87C18` → `D47800`, each time |
| date picker | `WM_PAINT` x1 / x1 | the stock border, blue `D47800` focused, grey `8D8D8D` unfocused |
| multi-line text box | `WM_NCPAINT` x1 / x1 (inside `WM_PAINT`) | the stock edge and scroll bar |
| up-down box, rich text box | none from Windows | (only the frame's own `RedrawWindow` cycle repainted them) |

With the fix in the probe (the region passed to the default `WM_NCPAINT` less the frame's area; the frame's area validated before
`WM_PAINT`), every one of those paints left the border exactly as the frame had painted it: 0 changed pixels in every paint, in and out.

## Fix

In `UiInputFrame.WndProc`, except in high contrast (where Windows keeps its own look first and the frame goes over it, as before):

- `WM_NCPAINT`: the default painting gets the update region (`wParam`, or the whole window for 1) less the frame's area (the window less
  `GetInnerRectangle`) and less the frame scroll bars UiScrollLook paints (`UiScrollLook.GetFrameBarRectanglesScreen`; it paints them after
  every `WM_NCPAINT`). The themed edit paints its border, and Windows the scroll bars, only inside the region passed, so the stock edge and
  bars are never drawn under the frame; the scroll bars' corner and anything else outside the frame are painted by Windows as before. The
  original `wParam` is put back for the looks outside, and the new region deleted.
- `WM_PAINT` (`wParam` 0): before the control paints, the frame's area inside the client area is validated (`ValidateRgn`), so the control's
  `BeginPaint` clip leaves it out: the up-down box's, date picker's and combo box's borders (and the combo box's animated line) are never
  drawn under the frame. A text box's frame is all in its window frame: nothing changes for it. The date picker's recolouring reads the update
  region after this, as before.
- A focus change (`GotFocus`/`LostFocus`, and the up-down box's child edit) paints the frame on the window at once (`PaintOnWindow`, the
  whole frame, which covers the old focus line) instead of `RedrawWindow` with `RDW_UPDATENOW`: only what changes is painted. What Windows
  changes itself on a focus change (the animation, a date picker's selected field) comes in its own paints, which now leave the frame alone.
  `EnabledChanged` keeps the full redraw (the fill and the text colours change).
- The frame is still painted after each `WM_NCPAINT` and `WM_PAINT`, so it covers every pixel of its area as before; the window's client
  size, the text's position and every behaviour are Windows' own, unchanged. `WM_PRINT` is unchanged: the galleries' pictures (G-UI3-ui13's
  element pictures among them, printed through `WM_PRINT`) cannot change.

## Tests

`tests/UiRefreshTests/InputFocusTests.cs`, at 100% and at 150% (LayoutManager's zoom): a `FormODBase` window (frames attached by
`InitializeLayoutManager`, as in the program) shown on screen with visual styles, with a single-line and a multi-line text box, a
`ValidDouble`, a rich text box, a stock combo box, an up-down box and a date picker. A `PaintProbe` is attached to each input before the
frame, so it sits under the frame in the message chain: for every `WM_NCPAINT` and `WM_PAINT` during a focus change in and out it reads
the window's pixels before and after Windows' painting, before the frame's, and fails when the frame's area changed (Windows' border on
screen). After the animation, the focused and unfocused pictures must be the same above the focus line's two rows (no text pixel moves; for
the date picker, whose field Windows highlights, only the frame's area), the focused picture must show the Accent line and the unfocused
one not, and the unfocused frame on screen must be the frame `WM_PRINT` draws. Inconclusive when the session draws nothing or the window
cannot take the focus.
