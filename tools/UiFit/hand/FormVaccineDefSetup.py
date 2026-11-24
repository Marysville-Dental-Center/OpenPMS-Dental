"""FormVaccineDefSetup on the grid (UI-12 slice 9, layout script on the planner's tree): the list and Add stood at 28 with 63 px between
them: from the padding to the right content edge, 12 px over Add."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = h.n(".").nw
    a = h.n("butAdd")
    s.move(h, "butAdd", x=8)
    h.set("listMain", 8, 8, W - 16, a.ny - 12 - 8)
