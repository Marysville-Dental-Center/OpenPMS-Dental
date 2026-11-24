"""FormRecallList on the grid (UI-12 slice 13, layout script on the planner's tree; the harness builds it with its clinic pickers in their
no-database mode, Scan/SampleData.cs). The planner kept main's 729 px View group, so Group Families, Show Conflicting Types and Show Do Not
Contact wrapped onto three lines in 99 px (the text cut), Include Reminded and Exclude Inactive stood alone on a fourth row, and the
bottom buttons kept main's 6 px gaps and three left edges per column. On the Recalls and Reactivations pages: the View group's columns
from its content edge (the date range, the label + box rows of Show and Recall Type or Billing Type and Sort, the rows of Provider, Clinic
and Site, the check box list), Refresh List under the date range on the Sort row, Set Status and Email From in a column at the page's right
padding with View's bottom on Email From's; the grid under them down to the two button rows, the buttons on seven columns 8 px apart,
Patient Count at the right on the first row. Reminders: the date range, the clinic picker and Refresh List on one row. The window grows
56 px wider so the check boxes stand on one line."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

COMBO_B = 104
COMBO_C = 140
RIGHT_W = 228  # Set Status, Email From


def view_group(h, group, picker, rows_b, rows_c, checks, refresh):
    """The View group's content; returns its width."""
    h.set(picker, 8, 23)
    pw = h.n(picker).nw
    lwb = max(h.lw(lb) for lb, _ in rows_b)
    bx = 8 + pw + 16 + lwb + 6
    for i, (lb, f) in enumerate(rows_b):
        h.text_align(lb, "MiddleRight")
        h.set(lb, bx - 6 - lwb, 23 + 27 * i, lwb, h.H)
        h.set(f, bx, 23 + 27 * i, COMBO_B, h.H)
    lwc = max(h.lw(lb) for lb, _ in rows_c if lb)
    cx = bx + COMBO_B + 16 + lwc + 6
    for i, (lb, f) in enumerate(rows_c):
        y = 23 + 27 * i
        if lb is None:  # the clinic picker: its box on the column
            cw = h.clinic_w(f)
            h.set(f, cx - cw, y, cw + COMBO_C, h.H)
            continue
        h.text_align(lb, "MiddleRight")
        h.set(lb, cx - 6 - lwc, y, lwc, h.H)
        h.set(f, cx, y, COMBO_C, h.H)
    dx = cx + COMBO_C + 16
    width = 0
    for i, c in enumerate(checks):
        h.check_left(c)
        cw = 16 + h.n(c).text_w + 4
        h.set(c, dx, 23 + i * (h.CH + 4), cw, h.CH)
        width = max(width, dx + cw)
    h.set(refresh, 8, 23 + 27 * 2, None, h.H)
    return width + 8


def buttons(h, rows, page_h):
    """rows: two rows of seven names (None: an empty place); the columns as wide as their widest, 8 px apart, the rows 4 px apart on the
    page's bottom padding."""
    widths = [max(h.n(r[i]).nw for r in rows if r[i]) for i in range(7)]
    y2 = page_h - 8 - 24
    y1 = y2 - 28
    x = 8
    for i in range(7):
        for r, y in zip(rows, (y1, y2)):
            if r[i]:
                h.set(r[i], x, y, widths[i], 24)
        x += widths[i] + 8
    return y1


def layout(h):
    page_h = h.n("tabPageRecalls").nh
    w1 = view_group(h, "groupViewRecalls", "datePickerRecalls",
                    (("labelShowReminders", "comboShowReminders"), ("labelRecallTypes", "comboRecallTypes"), ("labelSortRecalls", "comboSortRecalls")),
                    (("labelProviderRecalls", "comboProviderRecalls"), (None, "comboClinicRecalls"), ("labelSiteRecalls", "comboSiteRecalls")),
                    ("checkGroupFamiliesRecalls", "checkShowConflictingTypes", "checkIncludeReminded"), "butRefreshRecalls")
    w2 = view_group(h, "groupViewReact", "datePickerReact",
                    (("labelShowReactivate", "comboShowReactivate"), ("labelBillingTypes", "comboBillingTypes"), ("labelSortByReact", "comboSortReact")),
                    (("labelProviderReact", "comboProviderReact"), (None, "comboClinicReact"), ("labelSiteReact", "comboSiteReact")),
                    ("checkGroupFamiliesReact", "checkShowDoNotContact", "checkExcludeInactive"), "butRefreshReact")
    vw = max(w1, w2)
    page_w = 8 + vw + 8 + RIGHT_W + 8
    tab_w = page_w + 4
    client_w = 8 + tab_w + 8
    h.set(".", None, None, client_w, None)
    h.set("tabControl", 8, 8, tab_w, None)
    rx = page_w - 8 - RIGHT_W
    view_h = 54 + 8 + 54
    for view, status, combo, but, email, email_combo, grid, count, rows in (
            ("groupViewRecalls", "groupSetStatusRecalls", "comboSetStatusRecalls", "butSetStatusRecalls", "groupEmailFromRecalls",
             "comboEmailFromRecalls", "gridRecalls", "labelPatientCount",
             (("butPostcards", "butLabelOne", "butECards", "butReport", "butGotoFamily", None, "butSchedPat"),
              ("butUndo", "butLabels", "butEmail", "butPrint", "butGotoAccount", "butCommlog", "butSchedFam"))),
            ("groupViewReact", "groupSetStatusReact", "comboSetStatusReact", "butSetStatusReact", "groupEmailFromReact",
             "comboEmailFromReact", "gridReactivations", "labelReactPatCount",
             (("butReactPostcardPreview", "butReactSingleLabels", None, None, "butReactGoToFam", None, "butReactSchedPat"),
              (None, "butReactLabelPreview", "butReactEmail", "butReactPrint", "butReactGoToAcct", "butReactComm", "butReactSchedFam")))):
        h.set(view, 8, 8, vw, view_h)
        h.set(status, rx, 8, RIGHT_W, 54)
        bw = h.n(but).nw
        h.set(combo, 8, 23, RIGHT_W - 16 - bw - 6, h.H)
        h.set(but, RIGHT_W - 8 - bw, 23, None, h.H)
        h.set(email, rx, 8 + 54 + 8, RIGHT_W, 54)
        h.set(email_combo, 8, 23, RIGHT_W - 16, h.H)
        y1 = buttons(h, rows, page_h)
        gy = 8 + view_h + 8
        h.set(grid, 8, gy, page_w - 16, y1 - 8 - gy)
        c = h.n(count)
        cw = h.lw(count)
        h.text_align(count, "MiddleRight")
        h.set(count, page_w - 8 - cw, y1, cw, 24)
    # Reminders: the date range, the clinic picker and Refresh List on one row
    h.set("datePickerRemind", 8, 23)
    x = 8 + h.n("datePickerRemind").nw + 16
    h.set("comboClinicRemind", x, 23, h.clinic_w("comboClinicRemind") + COMBO_C, h.H)
    x += h.n("comboClinicRemind").nw + 8
    h.set("butRefreshRemind", x, 23, None, h.H)
    gw = x + h.n("butRefreshRemind").nw + 8
    gh = 23 + h.n("datePickerRemind").nh + 8
    h.set("groupViewRemind", 8, 8, gw, gh)
    h.set("gridReminders", 8, 8 + gh + 8, page_w - 16, page_h - 8 - (8 + gh + 8))
