"""FormPayPlanTemplates on the grid (UI-12 slice 10, layout script on the planner's tree). The clinic picker and the grid stood 26 px from the
left, the grid 38 px short of the right and 48 px over OK: on the padding, 12 px over OK; the hidden Add on the right padding, the hidden
Show Hidden 6 px after the picker."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    h.set("comboBoxClinic", 8, None, None, None)
    h.set("checkShowHidden", 8 + h.n("comboBoxClinic").nw + 6, None, None, None)
    s.move(h, "butAddTemplate", x=R - h.n("butAddTemplate").nw)
    s.fill(h, "gridPayPlanTemplates", y=35, over="butOK")
