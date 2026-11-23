"""FormEhrMedicalOrderRadEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The Instructions caption stood 5 px over
its box: 2 px over it, as every caption."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    tb = h.n("textDescription")
    ld = h.n("labelDescription")
    h.set("labelDescription", None, tb.ny - 2 - ld.nh)
