"""UI-12 slice 10: the helpers of the slice's layout scripts, on top of slice 7's (_s7.py). Batch 2 and 6 are mostly list windows whose
grid the planner kept where main drew it (12 to 26 px from the left, 6 to 50 px short of the right edge, 13 to 48 px over the bottom bar),
and a few windows the planner grew past what their content needs. Every number is the grid's (UI-12.md section 1): 8 px padding, 12 px
from a grid to the bottom bar under it (the section gap the planner keeps there), 6 px between the buttons of a row, 4 px in a column."""
import importlib.util
import os

import grid_tools as g

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s7 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s7)

PAD = 8
GAP = 6
SECTION = 12

span = s7.span
move = s7.move
label_before = s7.label_before
row_left = s7.row_left
row_right = s7.row_right
column = s7.column
right = s7.right
bottom = s7.bottom


def size(h, w, hh):
    """The window's (or user control's) size: main's, where the planner grew it past what the content needs."""
    h.set(".", None, None, w, hh)


def bar_y(h, name, parent="."):
    """The top of a bottom-bar control: PAD over the container's bottom."""
    return h.n(parent).nh - PAD - h.n(name).nh


def bar(h, left=(), right_=(), parent="."):
    """The bottom bar: the left buttons from the padding, the right ones ending on it, all on one line (the tallest's top)."""
    names = list(left) + list(right_)
    y = min(bar_y(h, n, parent) for n in names)
    if left:
        row_left(h, left, PAD, y)
    if right_:
        row_right(h, right_, h.n(parent).nw - PAD, y)
    return y


def vcenter(h, name, on):
    """A control centred on another's row (a note or check box beside a button or field)."""
    n, o = h.n(name), h.n(on)
    h.set(name, None, o.ny + (o.nh - n.nh) // 2, None, None)


def fill(h, name, x=PAD, y=None, r=None, b=None, over=None):
    """A grid (or list, box) from x to r (default: the right padding), from y (kept) down to b, or to SECTION over the control `over`."""
    if r is None:
        r = right(h)
    if over is not None:
        b = h.n(over).ny - SECTION
    elif b is None:
        b = bottom(h)
    return span(h, name, x=x, r=r, y=y, b=b)


LINE = 16  # a line of Segoe UI 9 (the program's font) in a wrapping label
INK_TOP = 3  # a line's capitals and ascenders start 3 px under the line's top
INK_BOTTOM = 15  # and its descenders end 15 px under it (gjpqy and the like)
INK_BOTTOM_PLAIN = 12  # a line without descenders ends 12 px under it
DESCENDERS = set("gjpqy,;()[]{}|/@_Q")


CHECK_GLYPH = 13  # a check box's box, centred on its height


def drawn(h, name):
    """UI-12F fix round: (top, bottom) of what a control draws. A label's or check box's text: its lines (the measured wrap; one line
    for a check box) at LINE px, placed by the text's alignment (top, middle or bottom of the box), from the first line's capitals to the
    last line's descenders (or its baseline when it has none), never past the box; a check box's box too. Any other control: its box.
    A label's box is as tall as its lines and the text check's slack, and a middle-aligned one is taller still, so box-equal gaps read
    3 to 6 px uneven (the review of G-UI3-ui12f)."""
    n = h.n(name)
    box = (n.ny, n.ny + n.nh)
    if n.kind not in ("label", "check") or not (n.text or "").strip():
        return box
    ta = n.new_text_align or n.text_align or ("MiddleLeft" if n.kind == "check" else "TopLeft")
    lines = [n.text]
    if n.kind == "label":
        lines = g.measured_wrap(n.text, n.text_w, n.nw - 2) or lines
    hh = LINE * len(lines)
    if ta.startswith("Top"):
        off = 0
    elif ta.startswith("Middle"):
        off = (n.nh - hh) // 2
    else:
        off = n.nh - hh
    ink = INK_BOTTOM if any(c in DESCENDERS for c in lines[-1]) else INK_BOTTOM_PLAIN
    top = n.ny + off + INK_TOP
    bottom = n.ny + off + LINE * (len(lines) - 1) + ink
    if n.kind == "check":
        glyph = n.ny + (n.nh - CHECK_GLYPH + 1) // 2
        top, bottom = min(top, glyph), max(bottom, glyph + CHECK_GLYPH)
    return max(box[0], top), min(box[1], bottom)


def drawn_top(h, name):
    return drawn(h, name)[0]


def drawn_bottom(h, name):
    return drawn(h, name)[1]


def keep_main(h, w, hh, blocks, left=(), right_=()):
    """UI-12F (the final consistency pass; the lead's decision at slice 6, a window never shrinks): the window back to main's client size
    w x hh, the bottom bar on its bottom padding, and the height the content does not fill shared out as equal gaps (as slice 8 did for
    FormBlockoutCutCopyPaste and FormEhrLabResultEdit): the blocks (lists of controls that move together, top down) keep the first one's
    top, and the gaps between consecutive blocks and the one over the bar are equal (the bar's gets the remainder, and at least the
    section gap): no empty band at the bottom. The fix round: the gaps are the drawn ones (drawn_top, drawn_bottom: a description label's
    text, not its taller box), so every gap reads the same; the bar still stands the section gap under the last block's boxes."""
    size(h, w, hh)
    y_bar = bar(h, left=left, right_=right_)
    bounds = [(min(drawn_top(h, x) for x in b), max(drawn_bottom(h, x) for x in b)) for b in blocks]
    top = bounds[0][0]
    used = sum(b1 - b0 for b0, b1 in bounds)
    free = y_bar - top - used
    gap = free // len(blocks)
    # The bar stands the section gap under the last block's boxes (GridRulesTests reads boxes): where a description's box reaches past its
    # text, the gaps between the blocks give way to it (equal still), and the bar's gets the rest.
    excess = max(h.n(x).ny + h.n(x).nh for x in blocks[-1]) - bounds[-1][1]
    if len(blocks) > 1 and free - gap * (len(blocks) - 1) - excess < SECTION:
        gap = (free - SECTION - excess) // (len(blocks) - 1)
    y = top
    for b, (b0, b1) in zip(blocks, bounds):
        for x in b:
            n = h.n(x)
            h.set(x, None, n.ny + y - b0, None, None)
        y += (b1 - b0) + gap
    last = y_bar - (y - gap)
    box_last = y_bar - max(h.n(x).ny + h.n(x).nh for x in blocks[-1])
    assert gap >= 4 and last >= SECTION and box_last >= SECTION, "%s: no room for the gaps (%d, %d over the bar, %d by the boxes)" % (
        h.window, gap, last, box_last)
    return gap
