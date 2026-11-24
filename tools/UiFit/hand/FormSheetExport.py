"""FormSheetExport on the grid (UI-12 slice 10, layout script on the planner's tree). The note and the grid stood 12 px from the left, Export
12 from the right and 39 px under the grid's top, the grid 14 px over the bottom: on the padding, Export on the grid's top, the grid 8 px
before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    h.set("label1", 8, None, R - 8, None)
    g = h.n("gridCustomSheet")
    s.move(h, "butExport", x=R - h.n("butExport").nw, y=g.ny)
    s.fill(h, "gridCustomSheet", r=h.n("butExport").nx - 8)
