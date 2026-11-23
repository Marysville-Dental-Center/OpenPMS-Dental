"""FormRpProcSheet on the grid (UI-12 slice 4 fix round, layout script on the planner's tree; hand/_calendar_report.py). The Clinics caption
stood 2 px under the To calendar's Today band and its column lined up with nothing: the column (caption, All, list) under the To calendar's
left edge, 8 px under its drawn bottom; the procedure code caption on the same row and height (as wide as its text, so it ends before the
column) with its box on the All row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_calendar_report", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_calendar_report.py"))
_cal = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_cal)


def layout(h):
    cap = h.n("labelClin")
    _cal.clinics_column(h, "date2", h.n("date2").nx)
    h.set("label2", None, cap.ny, h.lw("label2"), cap.nh)
    h.set("textCode", None, h.n("checkAllClin").ny, None, None)
