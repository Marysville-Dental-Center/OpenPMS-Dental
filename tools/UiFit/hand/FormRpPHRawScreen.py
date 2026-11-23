"""FormRpPHRawScreen on the grid (UI-12 slice 8, layout script on the planner's tree). The From calendar stood at x 34: a month calendar
takes 190 x 176 px when its handle is made (slice 2), so the From calendar on the padding, the TO caption 8 px after its drawn width and
the To calendar 6 px after the caption."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


CAL_W = 190


def layout(h):
    h.set("date1", 8, None)
    lt = h.n("labelTO")
    x = 8 + CAL_W + 8
    h.set("labelTO", x, None)
    h.set("date2", x + lt.nw + 6, None)
