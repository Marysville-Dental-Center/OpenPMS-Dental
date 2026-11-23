"""FormRpAccountingProfitLoss on the grid (UI-12 slice 8, layout script on the planner's tree). The From and To calendars stood at x 27
and 260 (6 px apart): from the padding, 8 px apart, their captions over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    c = h.n("monthCalendarStart")
    x2 = 8 + c.nw + 8
    h.set("labelTO", 8, None)
    h.set("monthCalendarStart", 8, None)
    h.set("label1", x2, None)
    h.set("monthCalendarEnd", x2, None)
