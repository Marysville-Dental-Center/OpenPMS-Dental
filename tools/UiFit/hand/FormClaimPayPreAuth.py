"""FormClaimPayPreAuth on the grid (UI-12 slice 7, layout script on the planner's tree). The grid ended 9 px short of Save's edge: to it. The grid ends 12 px over the bottom bar (the
section gap the planner keeps there; it stood 15 to 24 px over it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridMain", r=s.right(h))
    s.span(h, "gridMain", b=h.n("butSave").ny - 12)
