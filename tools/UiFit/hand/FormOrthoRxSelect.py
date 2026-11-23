"""FormOrthoRxSelect on the grid (UI-12 slice 8, layout script on the planner's tree). The lists stood at x 44 and 394 (44 px short of
the right padding) with 49 px empty under them, two widths, Select and Remove not centred between them: the lists one width from the
padding to the right padding, the Select and Remove column 8 px from each, centred on the lists' height; the lists down to 8 px over OK;
the captions 2 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, right=("butOK",))
    bw = h.n("butSelect").nw
    w = (R - 8 - bw - 16) // 2
    xb = 8 + w + 8
    xr = xb + bw + 8
    top = h.n("listBoxAvail").ny
    bot = yb - 8
    h.set("label1", 8, None)
    h.set("label2", xr, None)
    h.set("listBoxAvail", 8, top, w, bot - top)
    h.set("listBoxSelected", xr, top, R - xr, bot - top)
    mid = (top + bot) // 2
    h.set("butSelect", xb, mid - 2 - 24, None, 24)
    h.set("butRemove", xb, mid + 2, None, 24)
