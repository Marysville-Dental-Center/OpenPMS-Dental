"""FormSchedule on the grid (UI-12 slice 13, layout script on the planner's tree; the harness builds it with an empty week, Scan/SampleData.cs).
The planner kept the three groups of the left column 206 px wide, so the two holiday check boxes wrapped onto two lines each (main cut them),
and left the schedule grid 28 px short of the right padding and 162 px short of the bar. The left column 230 px wide (every check box on
one line), each group's rows from its content edge one under another (Refresh, the date captions over the two date boxes side by side, the
check box list, the clinic picker, Limit to Ops, the provider and employee tabs, Clear Week, Week Filter), the fields to the content's
right edge; the grid from the column's gap to the right padding and down to the column's bottom; Print on the bar under the section gap."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

GW = 230  # the left column's groups
CW = GW - 16  # their content width


def layout(h):
    lp = h.CH + 4  # the check box list pitch
    # Filters
    y = 23
    h.set("butRefresh", 8, y)
    y += h.n("butRefresh").nh + 8
    half = (CW - 8) // 2
    cap_h = 17
    h.set("label1", 8, y, half, cap_h)
    h.set("label2", 8 + half + 8, y, half, cap_h)
    h.text_align("label1", "BottomLeft")
    h.text_align("label2", "BottomLeft")
    y += cap_h + 2
    h.set("textDateFrom", 8, y, half, h.H)
    h.set("textDateTo", 8 + half + 8, y, half, h.H)
    y += h.H + 8
    for c in ("checkPracticeNotes", "checkClinicNotes"):
        h.check_left(c)
        h.set(c, 8, y, CW, h.CH)
        y += lp
    y += 8 - 4
    h.set("comboClinic", 8, y, CW, h.H)
    y += h.H + 8
    h.check_left("checkShowClinicSchedules")
    h.set("checkShowClinicSchedules", 8, y, CW, h.CH)
    y += h.CH + 8
    h.set("tabControl1", 8, y, CW, None)
    y += h.n("tabControl1").nh + 8
    h.set("butClearWeek", 8, y)
    y += h.n("butClearWeek").nh + 8
    h.set("groupBoxWeekFilter", 8, y, CW, None)
    y += h.n("groupBoxWeekFilter").nh + 8
    h.set("groupBoxFilter", 8, 8, GW, y)
    y = 8 + y + 8
    # Copy
    h.set("groupCopy", 8, y, GW, None)
    h.set("textClipboard", 8, None, CW, None)
    y += h.n("groupCopy").nh + 8
    # Paste
    h.set("groupPaste", 8, y, GW, None)
    bottom = y + h.n("groupPaste").nh
    # the grid beside the column, down to the column's bottom; Print on the bar under the section gap
    gx = 8 + GW + 8
    h.set("gridMain", gx, 8, s.W(h) - 8 - gx, bottom - 8)
    h.set(".", None, None, None, bottom + s.SECTION + h.n("butPrint").nh + 8)
    s.bar(h, right=("butPrint",))
