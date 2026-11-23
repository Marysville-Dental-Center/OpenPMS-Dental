"""UI-12 slice 7: small helpers the slice's layout scripts share (the windows of batch 3 are mostly list windows: a grid, its buttons, a
filter row). Every number is the grid's (UI-12.md section 1): 8 px padding, labels 6 px before their field, 6 px between the buttons of a
row, 4 px between the buttons of a column."""
PAD = 8
GAP = 6


def right(h, name="."):
    """The content's right edge of a container (the window by default)."""
    return h.n(name).nw - PAD


def bottom(h, name="."):
    """The content's bottom edge of a container (the window by default)."""
    return h.n(name).nh - PAD


def span(h, name, x=None, r=None, y=None, b=None):
    """Sets a control's edges: left x, right r, top y, bottom b (None keeps that edge where the planner put it)."""
    n = h.n(name)
    x0 = n.nx if x is None else x
    r0 = n.nx + n.nw if r is None else r
    y0 = n.ny if y is None else y
    b0 = n.ny + n.nh if b is None else b
    return h.set(name, x0, y0, r0 - x0, b0 - y0)


def move(h, name, x=None, y=None):
    """Moves a control (its size kept)."""
    return h.set(name, x, y, None, None)


def label_before(h, label, field, gap=GAP, w=None):
    """A label right-aligned gap px before its field, on the field's row and height (MiddleRight)."""
    f = h.n(field)
    lw = w if w is not None else h.lw(label)
    h.set(label, f.nx - gap - lw, f.ny, lw, f.nh)
    h.text_align(label, "MiddleRight")


def row_right(h, names, r, y=None, gap=GAP):
    """Buttons on one row ending at r, gap px apart, in the order given (the last ends at r)."""
    x = r
    for name in reversed(list(names)):
        n = h.n(name)
        x -= n.nw
        h.set(name, x, y, None, None)
        x -= gap


def row_left(h, names, x, y=None, gap=GAP):
    """Controls on one row from x, gap px apart."""
    for name in names:
        n = h.n(name)
        h.set(name, x, y, None, None)
        x += n.nw + gap
    return x - gap


def column(h, names, x, y, gap=4, w=None):
    """Buttons one under the other from y, gap px apart, at x (and w wide when given). Returns the bottom of the last."""
    for name in names:
        n = h.n(name)
        h.set(name, x, y, w, None)
        y += n.nh + gap
    return y - gap


def bar_top(h, grid):
    """The top of the bottom bar under a grid: the highest visible button of the window standing under it (None when there is none)."""
    g = h.n(grid)
    gb = g.ny + g.nh
    tops = [k.ny for k in h.tree.children if k.kind == "button" and k.visible and k.ny >= gb]
    return min(tops) if tops else None


def grid_bottom(h, grid, side=12):
    """A grid down to 12 px over the bottom bar (the section gap the planner keeps there), or to its side margin over the bottom."""
    t = bar_top(h, grid)
    return span(h, grid, b=(t - 12) if t is not None else h.tree.nh - side)
