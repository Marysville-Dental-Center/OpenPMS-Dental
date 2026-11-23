"""FormClaimReports on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 324 to 386
px for the Retrieving note hidden in the designer and kept the combo at x 12: main's width, the combo from the padding to 8 px before
Retrieve on the right padding, the note under them as wide as the content (its text fits on one line)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W = 324


def layout(h):
    s.size(h, MAIN_W, None)
    R = MAIN_W - 8
    rb = h.n("butRetrieve")
    h.set("butRetrieve", R - rb.nw, 8)
    h.set("comboClearhouse", 8, 8, R - rb.nw - 8 - 8, None)
    cb = h.n("comboClearhouse")
    h.set("labelRetrieving", 8, cb.ny + cb.nh + 8, R - 8, None)
