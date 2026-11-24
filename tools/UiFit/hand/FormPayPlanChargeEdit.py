"""FormPayPlanChargeEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The clinic picker's box ended 4 px short of the
provider combo over it: on its right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    p = h.n("comboBoxProv")
    s.move(h, "comboBoxClinic", x=p.nx + p.nw - h.n("comboBoxClinic").nw)
