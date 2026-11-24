"""FormEquipmentEdit on the grid (UI-12 slice 11, layout script on the planner's tree). The Status box ended 11 px short of the Description
and Location boxes: on their right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    d = h.n("textDescription")
    n = h.n("textStatus")
    h.set("textStatus", d.nx, None, d.nx + d.nw - d.nx, None)
