"""FormMaintBoard on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 12 px from the left and 4 px short of
Close's edge, against Record's 8: on the content edges. The grid ends 12 px over the bottom bar (the
section gap the planner keeps there; it stood 15 to 24 px over it). Add Item drew only its icon at 85 px
(as on main): 100 px wide, the buttons after it moved by as much."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridMain", x=8, r=s.right(h))
    s.span(h, "gridMain", b=h.n("butClose").ny - 12)
    # Add Item draws only its icon at 85 px (main too): 100 px, the buttons after it moved by as much, each group's buttons 6 px apart.
    x = h.n("butAdd").nx
    h.set("butAdd", None, None, 100, None)
    x += 100 + 6
    s.move(h, "butEdit", x=x)
    x += h.n("butEdit").nw + 28
    s.row_left(h, ("butHistory", "butHistoryAll"), x)
    x = h.n("butHistoryAll").nx + h.n("butHistoryAll").nw + 28
    s.row_left(h, ("butPrint", "butExport"), x)
