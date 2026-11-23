"""FormRpPHRawScreen on the grid (UI-12 slice 8, layout script on the planner's tree). The From calendar stood at x 34: a month calendar
takes 190 x 176 px when its handle is made (slice 2), so the From calendar on the padding, the TO caption 8 px after its drawn width and
the To calendar 6 px after the caption.
Fix round (ui/ui12-8-2): the calendars stood in the top-left corner over a 93 px empty band with 136 px empty to their right: the row of
the two calendars and TO centred in the content area, between the paddings across and between the top padding and OK down (as main
had it; equal bands on each side)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


CAL_W = 190


CAL_H = 176


def layout(h):
    W = h.tree.nw
    yb = h.n("butOK").ny
    lt = h.n("labelTO")
    cw = CAL_W + 8 + lt.nw + 6 + CAL_W
    x0 = 8 + (W - 16 - cw) // 2
    y0 = 8 + (yb - 8 - CAL_H) // 2
    h.set("date1", x0, y0)
    x = x0 + CAL_W + 8
    h.set("labelTO", x, y0)
    h.set("date2", x + lt.nw + 6, y0)
