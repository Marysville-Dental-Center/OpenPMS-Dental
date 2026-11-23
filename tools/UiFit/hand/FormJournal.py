"""FormJournal on the grid (UI-12 slice 6, layout script on the planner's tree). The filter row's pairs stood 61, 110, 27 and 67 px apart from
32: from the padding, each pair (label 6 px before its field, the drop button 6 px after the date) 16 px after the one before, (optional)
centred under Amt Max's box, the hidden calendars under their date boxes."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    x = 8
    def pair(lab, fld, after=None):
        nonlocal x
        lw = h.lw(lab)
        h.set(lab, x, None, lw, None)
        fx = x + lw + 6
        h.set(fld, fx, None)
        x = fx + h.n(fld).nw
        if after:
            h.set(after, x + 6, None)
            x += 6 + h.n(after).nw
        x += 16
        return fx
    fx = pair("labelDateFrom", "textDateFrom", "butDropFrom")
    h.set("calendarFrom", fx, None)
    fx = pair("labelDateTo", "textDateTo", "butDropTo")
    h.set("calendarTo", fx, None)
    pair("labelAmout", "textAmountFrom")
    fx = pair("labelAmountMax", "textAmountTo")
    h.set("labelOptional", fx, None, 80, None)
    pair("label4", "textFindText")
    h.set("butRefresh", x - 16 + 6, None)
