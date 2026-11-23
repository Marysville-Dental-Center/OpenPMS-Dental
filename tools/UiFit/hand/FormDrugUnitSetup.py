"""FormDrugUnitSetup on the grid (UI-12 slice 8, layout script on the planner's tree). The list and Add stood at x 28 (20 px short of
the right padding) with a 71 px empty band between them: the list from the padding to the right padding and down to 8 px over Add, Add
on the bottom padding (the window closes by its title bar)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, left=("butAdd",))
    h.set("listMain", 8, 8, s.W(h) - 16, yb - 8 - 8)
