"""FormReportsMore on the grid (UI-12 slice 11, layout script on the planner's tree). The planner kept main's columns: the first 1 px off
the padding (x 9), a 30 px gutter to the second and a 9 px gutter to the third (Business Analytics and the two EHR buttons the code shows).
Three columns at one 16 px gutter: the first on the padding, the lists 242 wide; the third column's EHR buttons as wide as the Business
Analytics group. The list heights stay: the code shrinks each list to its reports (MoveHeight)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

GUTTER = 16


def layout(h):
    lw = h.n("listProdInc").nw
    x1, x2 = 8, 8 + lw + GUTTER
    x3 = x2 + lw + GUTTER
    for name in ("butUserQuery", "labelProdInc", "listProdInc", "labelDaily", "listDaily", "labelMonthly", "listMonthly", "label6"):
        h.set(name, x1, None)
    h.set("butPW", x1 + h.n("butUserQuery").nw + 6, None)
    for name in ("butLaserLabels", "labelLists", "listLists", "labelPublicHealth", "listPublicHealth", "labelArizonaPrimaryCare",
                 "listArizonaPrimaryCare"):
        h.set(name, x2, None)
    g = h.n("groupBusiness")
    h.set("groupBusiness", x3, None)
    for name in ("butPatList", "butPatExport"):
        h.set(name, x3, None, g.nw, None)
