"""FormRpReferrals on the grid (UI-12 slice 8, layout script on the planner's tree). The tab control and the SQL box stood at x 14 (the
box 128 px short of the right padding): both from the padding to the right padding, the SQL box down to 8 px over OK.
Fix round (ui/ui12-8-2): on the SELECT page, All and None ended 18 px short of the list and the page's bottom padding was 25 px: All and
None share the list's width (6 px apart, None ending on the list's right edge) with their bottom on the page's padding, the list down to
4 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, right=("butOK",))
    h.set("tabReferrals", 8, None, R - 8, None)
    ts = h.n("textSQL")
    h.set("textSQL", 8, None, R - 8, yb - 8 - ts.ny)
    page = h.n("tabData")
    ls = h.n("listSelect")
    bw = (ls.nw - 6) // 2
    by = page.nh - 8 - h.n("butCheckAll").nh
    h.set("butCheckAll", ls.nx, by, bw, None)
    h.set("butClear", ls.nx + ls.nw - bw, by, bw, None)
    h.set("listSelect", None, None, None, by - 4 - ls.ny)
