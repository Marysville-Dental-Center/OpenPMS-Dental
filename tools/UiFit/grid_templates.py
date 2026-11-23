"""UI-12T item 3: window-family templates for the grid planner (docs/port-notes/UI-12.md, "Tooling (UI-12T)").

Three families, detected from a window's control tree (GridDump) alone:
  list    a list window: one grid, list or tree view filling most of the window (35% of its client area or more), with a button column beside
          it or a bottom bar, and at most a filter row above it; no group box, panel or tab control of its own besides docked ones.
  report  a report filter: date pickers or date fields, clinic or provider pickers, check boxes, and a Run / OK button; no big grid.
  edit    a simple edit dialog: label + field rows, check boxes and a bottom bar of buttons, nothing else (no container, grid or tall box).
detect() names a window's family (or None). The planner (grid_tools.py plan --templates, the default) uses a template for a window the
general rules leave with unplaced rows, when the window has no layout script, no hand rows and no scope: the template lays its root out
(template_layout below) and the general rules then run inside its parts, so no layout script is needed. A window the general rules settle
keeps their plan, so no window already laid out moves.
"""
import collections

GRID_HINTS = ("GridOD", "ODGrid", "DataGridView", "ListBox", "ListView", "TreeView", "CheckedListBox")
DATE_HINTS = ("DateTimePicker", "ODDatePicker", "ODDateRangePicker", "MonthCalendar", "ValidDate")
PICKER_HINTS = ("ComboBoxClinicPicker", "ComboBoxProv", "ComboBoxMultiProv")
RUN_NAMES = ("butOK", "butRun", "butReport", "butPrint", "butRefresh", "butExport", "butPreview")
INFORMATIONAL = ("the code sizes the window at run time", "by hand", "template ")


def short(n):
    return n.type.split(".")[-1]


def is_grid(n):
    return any(h in short(n) for h in GRID_HINTS)


def is_date(n):
    return any(h in short(n) for h in DATE_HINTS) or "date" in (n.name or "").lower() and n.kind == "field"


def is_picker(n):
    return any(h in short(n) for h in PICKER_HINTS) or (is_grid(n) and "prov" in (n.name or "").lower())


def is_text(n):
    return "TextBox" in short(n)


def is_small_list(n, area):
    """A list or grid of less than 15% of the client area: a picker or an option list, a part like a field."""
    return is_grid(n) and n.w * n.h < 0.15 * area


def client_area(tree):
    return max(1, (tree.cw or tree.w) * (tree.ch or tree.h))


def parts(tree):
    """The root's laid-out children: visible or not, not docked, with a size."""
    return [c for c in tree.children if c.dock in ("None", "") and c.w > 0 and c.h > 0]


SIMPLE_KINDS = ("button", "label", "check", "radio", "field")


def detect(tree):
    """'list', 'report', 'edit' or None, from the root's parts and the parts of its group boxes and panels (one level)."""
    kids = parts(tree)
    if not kids or any(c.kind == "tabs" for c in kids):
        return None
    inner = [x for c in kids if c.kind in ("group", "panel") for x in c.children if x.dock in ("None", "")]
    if any(x.kind in ("group", "panel", "tabs") for x in inner):
        return None  # nested containers: not a simple family
    flat = kids + inner
    area = client_area(tree)
    buttons = [c for c in flat if c.kind == "button"]
    m = main_grid(tree)
    # Parts that are none of the simple kinds, date controls, pickers, text boxes (a note) or small lists: an owner-drawn surface, a
    # second big grid, a control of another kind.
    odd = [c for c in flat if c is not m and c.kind not in SIMPLE_KINDS + ("group", "panel")
           and not (is_date(c) or is_picker(c) or is_text(c) or is_small_list(c, area))]
    if m is not None:
        # A list window: the grid, its buttons, and filters above or beside it (simple parts, or group boxes of simple parts and pickers).
        return "list" if buttons and not odd else None
    if any(is_grid(c) and not is_small_list(c, area) for c in flat):
        return None
    dates = [c for c in flat if is_date(c)]
    pickers = [c for c in flat if is_picker(c) or is_small_list(c, area)]
    runs = [c for c in buttons if c.name in RUN_NAMES]
    if (dates or pickers) and runs and not odd:
        return "report"
    tall = [c for c in flat if c.kind == "field" and c.h > 30]
    kinds = collections.Counter(c.kind for c in flat)
    if not odd and not tall and kinds["field"] + kinds["check"] + kinds["radio"] >= 1 and buttons and len(flat) <= 60:
        return "edit"
    return None


def _grid():
    import grid_tools  # the planner imports this module: imported when a template runs
    return grid_tools


def _ink(n):
    return _grid().ink_x_new(n)


def _overlap(a, b):
    ax, aw = _ink(a)
    bx, bw = _ink(b)
    return min(ax + aw, bx + bw) - max(ax, bx), min(a.y + a.h, b.y + b.h) - max(a.y, b.y)


def _set_drawn(n, x=None, y=None, w=None, h=None):
    """Moves a node in the tree the passes start from (not the drawn tree the rows are measured against: plan_window keeps that)."""
    if x is not None:
        n.x = n.nx = x
    if y is not None:
        n.y = n.ny = y
    if w is not None:
        n.w = n.nw = w
    if h is not None:
        n.h = n.nh = h


def untangle(tree, family):
    """The template's first step, on the window as drawn: the overlaps the general rules keep as drawn (a blob) are taken apart, so the
    cut finds clean rows. In every container the template lays out: a label that runs into the field on its right ends LABEL_GAP before it
    (or moves left by as much when its text needs the width), and controls of one column stacked a few px into each other (radio buttons,
    check boxes, date pickers drawn 1-8 px too close) are moved apart, the lower one down, top to bottom. Returns the names it moved."""
    g = _grid()
    gap = g.GRID["label_gap"]
    moved = []
    for c in g.walk(tree):
        if c.kind == "field" or not c.children or c.fixed:
            continue
        kids = [k for k in c.children if k.dock in ("None", "") and k.visible and k.editable and not k.fixed and k.w > 0 and k.h > 0]
        for lb in [k for k in kids if k.kind == "label"]:
            for f in kids:
                if f is lb or f.kind not in ("field", "other"):
                    continue
                beside = f.x <= lb.x and f.y <= lb.y and lb.y + lb.h <= f.y + f.h  # a label after a tall control, within its height
                if abs((f.y + f.h / 2.0) - (lb.y + lb.h / 2.0)) > 6 and not beside:
                    continue
                ix, iy = _overlap(lb, f)
                if ix <= 0 or iy <= 0:
                    continue
                if f.x <= lb.x:
                    # A label after a field ("to" between two dates): it starts ITEM_GAP after the field's ink.
                    fx, fw = _ink(f)
                    _set_drawn(lb, x=fx + fw + g.GRID["item_gap"])
                    moved.append(lb.name)
                    continue
                room = f.x - gap - lb.x
                if room >= (lb.text_w or 0) + 4:
                    _set_drawn(lb, w=room)
                elif f.x - gap - lb.w >= g.GRID["pad"]:
                    _set_drawn(lb, x=f.x - gap - lb.w)
                else:
                    continue
                moved.append(lb.name)
        # Check boxes or radio buttons of one row drawn into each other: the left one ends ITEM_GAP before the right one where its text
        # fits, otherwise the right one moves right.
        boxes = sorted([k for k in kids if k.kind in ("check", "radio")], key=lambda k: k.x)
        for i, a in enumerate(boxes):
            for b in boxes[i + 1:]:
                if abs((a.y + a.h / 2.0) - (b.y + b.h / 2.0)) > 6 or b.x <= a.x:
                    continue
                if min(a.x + a.w, b.x + b.w) - max(a.x, b.x) <= 0:
                    continue
                room = b.x - g.GRID["item_gap"] - a.x
                if room >= g.check_width(a):
                    _set_drawn(a, w=room)
                    moved.append(a.name)
                else:
                    _set_drawn(b, x=a.x + g.check_width(a) + g.GRID["item_gap"])
                    moved.append(b.name)
        col = sorted([k for k in kids if k.kind in ("radio", "check", "field", "label")], key=lambda k: (k.y, k.x))
        for i, a in enumerate(col):
            for b in col[i + 1:]:
                if b.y <= a.y:
                    continue
                ix, iy = _overlap(a, b)
                if ix > 0 and 0 < iy <= 8 and abs(a.x - b.x) <= g.GRID["column_snap"]:
                    _set_drawn(b, y=a.y + a.h)
                    moved.append(b.name)
    return moved


def main_grid(tree):
    """The list window's grid: the one grid, list or tree view of the root that fills 35% or more of the client area."""
    area = client_area(tree)
    big = [c for c in parts(tree) if is_grid(c) and c.w * c.h >= 0.35 * area]
    return big[0] if len(big) == 1 else None


def list_frame(planner, tree):
    """The list window's frame, after a pass of the general rules: the grid (anchored to all four sides, so the window's resizing keeps it
    so) fills the room right to the content's edge, or to the button column beside it (8 px before the column), and down to the bottom
    bar (8 px above it) or the content's bottom. Nothing it would cover moves into it: the room ends at the nearest control beside or
    under it."""
    g = _grid()
    m = main_grid(tree)
    if m is None or not m.editable or m.fixed or (m.anchor & 15) != 15:
        return
    pad, gap = g.GRID["pad"], g.GRID["sibling_gap"]
    others = [c for c in parts(tree) if c is not m and c.visible]
    right = tree.nw - pad
    beside = [c for c in others if c.nx >= m.nx + m.nw - 2 and min(c.ny + c.nh, m.ny + m.nh) - max(c.ny, m.ny) > 0]
    if beside:
        right = min(right, min(c.nx for c in beside) - gap)
    bottom = tree.nh - pad
    under = [c for c in others if c.ny >= m.ny + m.nh - 2 and min(c.nx + c.nw, m.nx + m.nw) - max(c.nx, m.nx) > 0]
    if under:
        bottom = min(bottom, min(c.ny for c in under) - gap)
    w, h = right - m.nx, bottom - m.ny
    if w >= m.nw and h >= m.nh and (w, h) != (m.nw, m.nh):
        m.nw, m.nh = w, h
        m.add_rule("Size", "template (list window): the grid fills the room beside its button column and above its bar")


def settled(unplaced_rows):
    """A window is settled when the planner lists nothing for it but the informational rows (the code sizes the window, by hand)."""
    return not [r for r in unplaced_rows if not any(r[2].startswith(x) for x in INFORMATIONAL)]
