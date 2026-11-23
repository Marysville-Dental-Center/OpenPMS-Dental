"""FormDisplayFieldsOrthoChart on the grid (UI-12 slice 7, layout script on the planner's tree). The Available Fields list stood 25 px under
the grid's top and the left column 12 px from the left against Up's 8: the list on the grid's top with its caption 2 px over it, the left
column on the content edge (Tab, its combo box 6 px after it and Setup Tabs 6 px after the combo box). The grid down to 12 px over the
bottom bar (it stood 37 px over it). Fix round (the lead's review of 2026-11-23): the right column (the list, the new field box and their
captions) to Save's right edge (it stopped 34 px short, the box 2 px past the list), the arrows centred between the grid and the list."""
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
    s.span(h, "gridMain", x=8, b=h.n("butUp").ny - 12)
    g = h.n("gridMain")
    sv = h.n("butSave")
    E = sv.nx + sv.nw
    lst = h.n("listAvailable")
    s.span(h, "listAvailable", y=g.ny, r=E)
    h.set("labelAvailable", lst.nx, g.ny - 2 - 17, lst.nw, 17)
    h.text_align("labelAvailable", "BottomLeft")
    for n in ("textCustomField", "labelCustomField"):
        s.span(h, n, x=lst.nx, r=E)
    ge = g.nx + g.nw
    for n in ("butLeft", "butRight"):
        b = h.n(n)
        s.move(h, n, x=ge + (lst.nx - ge - b.nw) // 2)
