"""Edges of a window's content (UI-12 slice 6, the layout scripts' common steps). The planner keeps a list window's grid and the controls on
its edges where main drew them (12 to 30 px from the left, 4 to 20 px short of the right edge, 20 to 40 px over the bottom), so the
content's frame was not the 8 px padding of the grid (section 1). frame() moves the content's left, right and bottom edges to the padding;
to() and under() place a control's edge against another's, with the grid's gaps."""

PAD = 8
GAP = 8  # sibling gap (containers and blocks side by side or stacked)


def kids(h, parent=None):
    p = h.tree if parent in (None, ".") else h.n(parent)
    return [k for k in p.children if k.dock in ("None", "") and k.w > 0 and k.h > 0 and k.name]


def wide(k):
    return k.nw >= 120 and k.kind != "button"


def frame(h, left=True, right=True, bottom=False, tol=4, parent=None, skip=()):
    """Moves the content's left edge to the padding and its right edge to the container's width less the padding: the controls whose left
    edge is within tol px of the leftmost shown one move to the padding (a wide one, 120 px or more and not a button, keeps its right edge);
    the controls whose right edge is within tol px of the rightmost shown one end on the right padding (a wide one grows, others move).
    bottom: the controls whose bottom is within tol px of the lowest shown one end on the bottom padding (a button moves, others grow)."""
    c = h.tree if parent in (None, ".") else h.n(parent)
    ks = [k for k in kids(h, parent) if k.name not in skip]
    shown = [k for k in ks if k.visible]
    if not shown:
        return
    pad_top = PAD
    if left:
        L = min(k.nx for k in shown)
        for k in ks:
            if k.nx <= L + tol and k.nx != PAD:
                dx = k.nx - PAD
                h.set(k.name, PAD, None, k.nw + dx if wide(k) else None, None)
    if right:
        W = c.nw
        Rm = max(k.nx + k.nw for k in shown)
        for k in ks:
            r = k.nx + k.nw
            if r >= Rm - tol and r != W - PAD:
                if wide(k):
                    h.set(k.name, None, None, W - PAD - k.nx, None)
                else:
                    h.set(k.name, W - PAD - k.nw, None, None, None)
    if bottom:
        H = c.nh
        Bm = max(k.ny + k.nh for k in shown)
        for k in ks:
            b = k.ny + k.nh
            if b >= Bm - tol and b != H - PAD:
                if k.kind == "button" or k.nh <= 30:
                    h.set(k.name, None, H - PAD - k.nh, None, None)
                else:
                    h.set(k.name, None, None, None, H - PAD - k.ny)
    return pad_top


def icon_buttons(h):
    """UI-12 slice 6 fix round: a button with an Icon (this.<name>.Icon = ... in the designer) keeps the 24 px button height: at 23 its icon
    paints over the bottom-left border (main draws them 24). The button's top stays."""
    import re
    for name in re.findall(r"this\.(\w+)\.Icon ?= ?", h.designer.text):
        if name in h.by_name and h.n(name).kind == "button" and h.n(name).nh < 24:
            h.set(name, None, None, None, 24)


def bottom_to(h, name, y):
    """The control's bottom edge at y (its top kept)."""
    n = h.n(name)
    h.set(name, None, None, None, y - n.ny)


def over(h, name, other, gap=GAP):
    """The control's bottom edge gap px over the other's top (its top kept: a grid grows down to a button row)."""
    bottom_to(h, name, h.n(other).ny - gap)


def right_to(h, name, x):
    """The control's right edge at x: a wide control grows or shrinks (its left kept), another moves."""
    n = h.n(name)
    if wide(n):
        h.set(name, None, None, x - n.nx, None)
    else:
        h.set(name, x - n.nw, None, None, None)


def right_edge(h, parent=None):
    c = h.tree if parent in (None, ".") else h.n(parent)
    return c.nw - PAD


def bottom_edge(h, parent=None):
    c = h.tree if parent in (None, ".") else h.n(parent)
    return c.nh - PAD
