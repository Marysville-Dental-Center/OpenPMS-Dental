"""FormPayPlanChargeEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The clinic picker's box ended 4 px short of the
provider combo over it: the box (after the picker's own 35 px label) on the field column and the combo's right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    p = h.n("comboBoxProv")
    x = p.nx - 35
    h.set("comboBoxClinic", x, None, p.nx + p.nw - x, None)
