"""FormXChargeReconcile on the grid (UI-12 slice 6, layout script on the planner's tree). The two calendars draw at 190 x 176 (not the
designer's 178 x 155) and covered View Imported and Payments: Import and its note at the top, the calendars 8 px under it from the padding
16 px apart, then the five buttons (one width) with their notes 6 px after them on a 28 px pitch 8 px under the calendars' drawn bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    CW, CH = 190, 176
    h.set("butImport", 8, 8, 85, None)
    h.set("label5", 8 + 85 + 6, 10)
    h.set("date1", 8, 40)
    h.set("date2", 8 + CW + 16, 40)
    y = 40 + CH + 8
    for but, lab in (("butViewImported", "label4"), ("butPayments", "label7"), ("butValidate", "label1"), ("butMissing", "label2"), ("butExtra", "label3")):
        h.set(but, 8, y, 85, 24)
        h.set(lab, 8 + 85 + 6, y + 2)
        y += 28
