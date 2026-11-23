"""FormEtrans834Import on the grid (UI-12 slice 7, layout script on the planner's tree). The note, the path row and the grid stood 12 px from
the left and 8 px short of Save's edge, the path box and its two buttons 20 px tall: all on the content edges, the path row at the field
height (the box to 6 px before the pick button, Refresh on the right edge), the grid 4 px under it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.span(h, "label2", x=8, r=R)
    s.move(h, "label1", x=8)
    y = h.n("textImportPath").ny
    h.set("butRefresh", R - 75, y, 75, h.H)
    h.set("butImportPathPick", R - 75 - 6 - 30, y, 30, h.H)
    s.span(h, "textImportPath", x=8, r=R - 75 - 6 - 30 - 6, b=y + h.H)
    s.span(h, "gridInsPlanFiles", x=8, r=R, y=y + h.H + 4)
