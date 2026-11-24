"""FormSheetFieldComboBox on the grid (UI-12 slice 11, layout script on the planner's tree). The pre-set options list cut its fifth row
over a 120 px empty band, Tab Order's box stood 2 px left of Reportable Name's, and Remove's icon and text touched in its 69 px. The list
grows to whole rows of UI.ListBox's 17 px line down to the section gap over the bar; Tab Order on the column; Remove 80 wide and the
arrows sharing the rest of the list's width, 6 px apart."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


LINE = 17
BORDER = 4


def layout(h):
    yb = s.bar(h, left=("butDelete",), right=("butSave",))
    l = h.n("listComboType")
    n = (yb - s.SECTION - l.ny - BORDER) // LINE
    h.set("listComboType", None, None, None, BORDER + LINE * n)
    r = h.n("textReportable")
    h.set("textTabOrder", r.nx, None, r.nw, h.H)
    s.label_before(h, "label2", "textTabOrder")
    o = h.n("listboxComboOptions")
    rw = 80
    aw = (o.nw - rw - 12) // 2
    y = h.n("butRemove").ny
    h.set("butRemove", o.nx, y, rw, None)
    h.set("butUp", o.nx + rw + 6, y, aw, None)
    h.set("butDown", o.nx + o.nw - aw, y, aw, None)
