"""FormCertificationEdit on the grid (UI-12 slice 8, layout script on the planner's tree). A 30 px empty band stood between Hidden and
the bottom bar (the window keeps main's size): the Category list grows down so that Hidden stands 8 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, left=("butDelete",), right=("butSave",))
    ck = h.n("checkIsHidden")
    yc = yb - 8 - ck.nh
    h.set("checkIsHidden", None, yc)
    lb = h.n("listBoxCategories")
    h.set("listBoxCategories", None, None, None, yc - 4 - lb.ny)
