"""FormEhrProviderKeys on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 41 px from the left under a note at 12,
and 35 px over the bottom: the note and the grid on the content edge, the grid 8 px over the bottom (6 px before Add kept). Fix round (the
lead's review of 2026-11-23): Add and the note on the content's right edge (they stood 24 and 4 px short of it), the grid to 8 px before
Add."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.span(h, "label1", x=8, r=R)
    a = h.n("butAdd")
    s.move(h, "butAdd", x=R - a.nw)
    s.span(h, "gridMain", x=8, r=a.nx - 8, b=s.bottom(h))
