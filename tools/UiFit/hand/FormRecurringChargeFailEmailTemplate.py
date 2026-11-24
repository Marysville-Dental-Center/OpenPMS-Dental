"""FormRecurringChargeFailEmailTemplate on the grid (UI-12 slice 9, layout script on the planner's tree): the intro note stood at 12 against
the fields' 8 px window edge: at the padding to the fields' right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    t = h.n("textBody")
    s.span(h, "labelIntro", x=8, r=t.nx + t.nw)
