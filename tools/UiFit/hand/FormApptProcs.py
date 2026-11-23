"""FormApptProcs on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 32 px from the left, and the planner put
Add (anchored to the bottom and the right) at the top of the button column: the grid from the content edge to 8 px before OK's column,
Add on OK's column at main's 188 px over OK (it moves with the bottom edge, as on main)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    h.set("butAdd", ok.nx, ok.ny - 188, None, None)
    s.span(h, "gridMain", x=8, r=ok.nx - 8)
