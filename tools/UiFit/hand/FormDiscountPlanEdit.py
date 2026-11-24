"""FormDiscountPlanEdit on the grid (UI-12 slice 11, layout script on the planner's tree). The planner left the wide fields on three right
edges (394, 391, 392): the plan note and the patients combo end on the Description box's edge (394); List Patients at the field height on
its row (the code puts it in the patients combo's place when the plan has no patients)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    right = h.n("textDescript").nx + h.n("textDescript").nw
    for name in ("textPlanNote", "comboPatient"):
        n = h.n(name)
        h.set(name, None, None, right - n.nx, None)
    h.set("butListPatients", right + 6, h.n("comboPatient").ny, None, h.H)
