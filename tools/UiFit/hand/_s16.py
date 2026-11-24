"""Common steps of the UI-12 slice 16 layout scripts (the windows built around a surface the code draws, and the shared controls): the
surface keeps the grid's padding (8 px) on every side the window allows, a section gap (12 px) over the bottom bar, a sibling gap (8 px)
beside a column of buttons; toolbars docked on top stay edge to edge (window chrome). The numbers are those of UiGrid (CodeBase/UI)."""

PAD = 8
SECTION = 12
GAP = 8
ITEM = 6
BUTTON_H = 24


def surface(h, name, top=0, bottom=None, right=None):
    """The surface from PAD (under a toolbar of height top: top + PAD) to the root's right edge less PAD (or right), down to bottom (the
    root's height less PAD when None)."""
    root = h.tree
    x = PAD
    y = top + PAD
    r = root.w - PAD if right is None else right
    b = root.h - PAD if bottom is None else bottom
    h.set(name, x, y, r - x, b - y)


def bar_right(h, names, widths=None, y=None, h_=BUTTON_H):
    """Buttons on one line at the bottom right, PAD from the edges and from each other (the last name rightmost). Returns the bar's top."""
    root = h.tree
    yy = root.h - PAD - h_ if y is None else y
    x = root.w - PAD
    for i, name in enumerate(reversed(names)):
        w = h.n(name).nw if widths is None else widths[len(names) - 1 - i]
        x -= w
        h.set(name, x, yy, w, h_)
        x -= GAP
    return yy
