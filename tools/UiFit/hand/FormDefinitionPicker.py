"""FormDefinitionPicker on the grid (UI-12 slice 7, layout script on the planner's tree). None stood 21 px right of the Show hidden check box
above it: on the check box's x, 4 px under it. Fix round (the lead's review of 2026-11-23): the side column on OK's right edge: None at OK's
x, Show hidden ending on that edge, None 4 px under it; the grid from the content edge (it stood at 12) to 8 px before the check box."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    E = ok.nx + ok.nw
    c = h.n("checkShowHidden")
    s.move(h, "checkShowHidden", x=E - c.nw)
    h.set("butNone", ok.nx, c.ny + c.nh + 4, ok.nw, None)
    s.span(h, "gridMain", x=8, r=c.nx - 8)
