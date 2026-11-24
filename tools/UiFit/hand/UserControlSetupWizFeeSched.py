"""UserControlSetupWizFeeSched on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 21 px from the left and 33
over the bottom, the buttons 7 px after it from 16 px under its top: the grid on the padding, the buttons a column 8 px after it from its
top (4 px apart), each info icon 6 px after its button and centred on it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    g = s.fill(h, "gridMain", y=8, r=8 + h.n("gridMain").nw)
    x = 8 + g.nw + 8
    s.column(h, ("butAdd", "butEditFee", "butImport"), x, 8)
    for b, l in (("butAdd", "labelAdd"), ("butEditFee", "labelEdit"), ("butImport", "labelImport")):
        h.set(l, x + h.n(b).nw + 6, None, None, None)
        s.vcenter(h, l, b)
