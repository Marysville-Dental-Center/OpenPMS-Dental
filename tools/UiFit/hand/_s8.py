"""Common steps of the UI-12 slice 8 layout scripts (batch 4, the small windows), on top of _edges.py: the bottom bar on the padding, a
list or box grown down to the bar (no empty band over it), a block of controls moved together, and the height the content does not fill
shared out as equal gaps (a window never shrinks: the lead's decision on slice 6's question)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)

PAD = 8
GAP = 8  # sibling gap, and the gap between the buttons of a bar


def W(h):
    return h.tree.nw


def H(h):
    return h.tree.nh


def bar(h, left=(), right=(), y=None, bh=None):
    """The window's bottom bar: every button bh px tall (default: its own height) with its bottom on the padding (or its top at y); the
    left buttons from the padding rightwards, the right buttons ending on the right padding leftwards (the last name is the rightmost), 8 px
    apart. Returns the bar's top."""
    tops = []
    x = PAD
    for name in left:
        n = h.n(name)
        hh = bh or n.nh
        top = y if y is not None else H(h) - PAD - hh
        h.set(name, x, top, None, hh)
        tops.append(top)
        x += n.nw + GAP
    x = W(h) - PAD
    for name in reversed(right):
        n = h.n(name)
        hh = bh or n.nh
        top = y if y is not None else H(h) - PAD - hh
        x -= n.nw
        h.set(name, x, top, None, hh)
        tops.append(top)
        x -= GAP
    return min(tops) if tops else H(h) - PAD


def bottom_to(h, name, y):
    """The control's bottom at y (its top kept)."""
    n = h.n(name)
    h.set(name, None, None, None, y - n.ny)


def move(h, names, dx=0, dy=0):
    """Moves the controls by dx, dy."""
    for name in names:
        n = h.n(name)
        h.set(name, n.nx + dx, n.ny + dy)


def block_bounds(h, names):
    ns = [h.n(x) for x in names]
    return min(n.ny for n in ns), max(n.ny + n.nh for n in ns)


def spread(h, blocks, top, bottom):
    """Shares the room out as equal gaps: the blocks (lists of control names that move together) stand from top down, the first at top,
    and the gaps between consecutive blocks and under the last one (to bottom, the top of what follows, such as the bottom bar) are equal
    (at least the 8 px sibling gap)."""
    bounds = [block_bounds(h, b) for b in blocks]
    used = sum(b1 - b0 for b0, b1 in bounds)
    n = len(blocks)
    if n == 0:
        return
    gap = max(GAP, (bottom - top - used) // n)
    y = top
    for b, (b0, b1) in zip(blocks, bounds):
        move(h, b, 0, y - b0)
        y += (b1 - b0) + gap


def size(h, w=None, hh=None):
    """The window's client size."""
    h.set(".", None, None, w, hh)
