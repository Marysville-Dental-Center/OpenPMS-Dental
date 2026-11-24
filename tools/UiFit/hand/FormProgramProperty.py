"""FormProgramProperty on the grid (UI-12 slice 9, layout script on the planner's tree): the planner's second pass on its own layout widens
the two boxes from 320 to 322 px (their right edge onto the 4 px grid at 484): no fixed point in one pass. The boxes at 322 px, where the
planner's rules stop."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    for name in ("textProperty", "textValue"):
        h.set(name, None, None, 322, None)
