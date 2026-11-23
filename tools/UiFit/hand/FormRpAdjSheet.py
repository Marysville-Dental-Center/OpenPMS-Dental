"""FormRpAdjSheet on the grid (UI-12 slice 4 fix round, layout script on the planner's tree; hand/_calendar_report.py). The Clinics caption
and All box stood 23 px right of their list, both captions 2 px under the calendars' Today band, and the From calendar 3 px right of the
Adjustment Types list under it: the From calendar on that list's x and TO centred between the calendars' drawn edges; both captions (one
height) 8 px under the calendars' drawn bottom, each column's All box and list under its caption, the Clinics column on its list's x."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_calendar_report", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_calendar_report.py"))
_cal = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_cal)


def layout(h):
    lt = h.n("listType")
    h.set("date1", lt.nx, None, None, None)
    d2 = h.n("date2")
    to = h.n("labelTO")
    h.set("labelTO", (lt.nx + _cal.CAL_W + d2.nx - to.nw) // 2, None, None, None)
    h.set("labelClin", None, None, None, h.n("label2").nh)
    _cal.clinics_column(h, "date1", h.n("listClin").nx, others=("label2", "checkAllAdjs", "listType"))
