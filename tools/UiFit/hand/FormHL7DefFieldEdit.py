"""FormHL7DefFieldEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 678 to
747 px for the internal-definition note hidden in the designer (the code shows it, Delete disabled, for an internal definition) and left
Delete at x 9: main's height; the bottom bar on the padding; the note on the padding 8 px over Delete; the field list down to 8 px over
the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 678


def layout(h):
    s.size(h, None, MAIN_H)
    yb = s.bar(h, left=("butDelete",), right=("butSave",))
    ld = h.n("labelDelete")
    h.set("labelDelete", 8, yb - 8 - ld.nh)
    lf = h.n("listFieldNames")
    h.set("listFieldNames", None, None, None, yb - 8 - lf.ny)
