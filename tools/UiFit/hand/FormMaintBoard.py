"""FormMaintBoard on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 12 px from the left and 4 px short of
Close's edge, against Record's 8: on the content edges. The grid ends 12 px over the bottom bar (the section gap the planner keeps there; it
stood 15 to 24 px over it). Fix round (the lead's review of 2026-11-23): Export 6 px after Print, as the other pairs of the bottom row (it
stood 10 px after it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridMain", x=8, r=s.right(h))
    s.span(h, "gridMain", b=h.n("butClose").ny - 12)
    p = h.n("butPrint")
    s.move(h, "butExport", x=p.nx + p.nw + 6)
