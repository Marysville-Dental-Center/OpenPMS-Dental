"""FormSupplyProcPendings on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 760 x 482 -> 768 x 486
and kept the note and the grid 12 px from the left: main's size, on the padding, the grid 12 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 760, 482)
    R = s.right(h)
    s.bar(h, left=("butMaterials", "butNoMaterials"), right_=("butClose",))
    h.set("labelInstructions", 8, None, R - 8, None)
    s.fill(h, "gridMain", y=42, over="butClose")
