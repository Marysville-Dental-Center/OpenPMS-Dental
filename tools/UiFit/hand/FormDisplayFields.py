"""FormDisplayFields on the grid (UI-12 slice 7, layout script on the planner's tree). The Available Fields list stood 23 px under the grid's
top and ended 10 px under it, its caption 213 px wide (it grew the window from 549 to 597 px), and the left column stood 12 px from the
left against Up's 8: the left column on the content edge, the list on the grid's top and bottom with its caption 2 px over it as wide as
the list, the window back at main's 549 px with Save on its edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    for n in ("labelCategory", "butDefault"):
        s.move(h, n, x=8)
    s.move(h, "labeldefault", x=8 + h.n("butDefault").nw + 6)
    g = h.n("gridMain")
    s.span(h, "gridMain", x=8)
    lst = h.n("listAvailable")
    h.set("listAvailable", None, g.ny, None, g.nh)
    h.set("labelAvailable", lst.nx, g.ny - 2 - 17, lst.nw, 17)
    h.text_align("labelAvailable", "BottomLeft")
    W = 549
    h.set(".", None, None, W, None)
    h.set("butSave", W - 8 - h.n("butSave").nw, None, None, None)
