"""FormReconcileAllergy on the grid (UI-12 slice 4, layout script on the planner's tree): the reconcile windows' common layout
(hand/_reconcile.py). The planner left the source grids 3 to 10 px short of the reconciled grid's right edge and the Add buttons 51 px wide
(their text against the icon)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_reconcile", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_reconcile.py"))
_rec = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_rec)


def layout(h):
    _rec.layout(h, "gridAllergyExisting", "gridAllergyImport", "gridAllergyReconcile")
