"""UI-12 slice 10: the helpers of the slice's layout scripts, on top of slice 7's (_s7.py). Batch 2 and 6 are mostly list windows whose
grid the planner kept where main drew it (12 to 26 px from the left, 6 to 50 px short of the right edge, 13 to 48 px over the bottom bar),
and a few windows the planner grew past what their content needs. Every number is the grid's (UI-12.md section 1): 8 px padding, 12 px
from a grid to the bottom bar under it (the section gap the planner keeps there), 6 px between the buttons of a row, 4 px in a column."""
import importlib.util
import os

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


def keep_main(h, w, hh, blocks, left=(), right_=()):
    """UI-12F (the final consistency pass; the lead's decision at slice 6, a window never shrinks): the window back to main's client size
    w x hh, the bottom bar on its bottom padding, and the height the content does not fill shared out as equal gaps (as slice 8 did for
    FormBlockoutCutCopyPaste and FormEhrLabResultEdit): the blocks (lists of controls that move together, top down) keep the first one's
    top, and the gaps between consecutive blocks and the one over the bar are equal (the bar's gets the remainder, and at least the
    section gap): no empty band at the bottom."""
    size(h, w, hh)
    y_bar = bar(h, left=left, right_=right_)
    bounds = [(min(h.n(x).ny for x in b), max(h.n(x).ny + h.n(x).nh for x in b)) for b in blocks]
    top = bounds[0][0]
    used = sum(b1 - b0 for b0, b1 in bounds)
    gap = (y_bar - top - used) // len(blocks)
    y = top
    for b, (b0, b1) in zip(blocks, bounds):
        for x in b:
            n = h.n(x)
            h.set(x, None, n.ny + y - b0, None, None)
        y += (b1 - b0) + gap
    last = y_bar - (y - gap)
    assert gap >= 4 and last >= SECTION, "%s: no room for the gaps (%d, %d over the bar)" % (h.window, gap, last)
    return gap
