"""FormDefinitionPicker on the grid (UI-12 slice 7, layout script on the planner's tree). None stood 21 px right of the Show hidden check box
above it: on the check box's x, 4 px under it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    c = h.n("checkShowHidden")
    h.set("butNone", c.nx, c.ny + c.nh + 4, None, None)
