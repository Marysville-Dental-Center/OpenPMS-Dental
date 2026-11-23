"""FormDisplayFieldsOrthoChart on the grid (UI-12 slice 7, layout script on the planner's tree). The Available Fields list stood 25 px under
the grid's top and the left column 12 px from the left against Up's 8: the list on the grid's top with its caption 2 px over it, the left
column on the content edge (Tab, its combo box 6 px after it and Setup Tabs 6 px after the combo box)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.move(h, "labelCategory", x=8)
    lw = h.lw("label1")
    h.set("label1", 8, None, lw, None)
    s.row_left(h, ("comboOrthoChartTabs", "butSetupTabs"), 8 + lw + 6)
    g = h.n("gridMain")
    s.span(h, "gridMain", x=8)
    lst = h.n("listAvailable")
    s.span(h, "listAvailable", y=g.ny)
    h.set("labelAvailable", lst.nx, g.ny - 2 - 17, lst.nw, 17)
    h.text_align("labelAvailable", "BottomLeft")
