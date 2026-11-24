"""FormProcCodes on the grid (UI-12 slice 11, layout script on the planner's tree; its designer file is FormProcCodes2). The planner grew
the window from main's 1014 x 696 to 1150 x 900 (the three Fee groups 348 wide around 184 px combos, the procedure code buttons 113 px
under the search group), put the fee schedule group combos (stacked on the clinic pickers, shown in their place by Show Fee Schedule
Groups; the code moved them to the literal (14, 96), now to the clinic picker's place) beside the fee schedule combos, and left the fee
schedule combos 47 px narrower than the clinic and provider combos. Three columns: the search group over the Procedure Codes group on the
left (173 wide, the buttons' group on the bottom padding), the grid in the middle from the top to the bottom padding, and a 250 px right
column ending on the right padding: the Compare caption, the three Fee groups (one height; each caption 2 px over its box; the combos one
width, their pick buttons 6 px after them), the Fee Colors group (swatches and their names on two rows and two columns), Fee Scheds and
Fee Tools, and the bottom bar's row with Fees as of and OK. The window grows 71 px taller (1014 x 767) for the Fee groups at the row
pitch; it keeps main's width."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

WW, WH = 1014, 767
LEFT_W = 173
COL_W = 250
CAP = 17


def fee_group(h, g, i, y):
    inner = COL_W - 16
    pick = h.n("butPickSched%d" % i).nw
    cw = inner - pick - 6
    h.set(g, None, y, COL_W, None)
    h.set("checkGroups%d" % i, 8, 23)
    yy = 23 + h.n("checkGroups%d" % i).nh + 4
    for cap, boxes, but in (("labelSched%d", ("comboFeeSched%d",), "butPickSched%d"),
                            ("labelClinic%d", ("comboClinic%d", "comboFeeSchedGroup%d"), "butPickClinic%d"),
                            ("labelProvider%d", ("comboProvider%d",), "butPickProv%d")):
        h.text_align(cap % i, "BottomLeft")
        h.set(cap % i, 8, yy, inner, CAP)
        yy += CAP + 2
        for b in boxes:
            h.set(b % i, 8, yy, cw, h.H)
        h.set(but % i, 8 + cw + 6, yy, None, h.H)
        yy += h.H + 4
    gh = yy - 4 + 8
    h.set(g, None, None, None, gh)
    return gh


def layout(h):
    h.set(".", None, None, WW, WH)
    R = WW - 8
    B = WH - 8
    # the left column
    p = "groupProcCodeSetup"
    bw = (LEFT_W - 16 - 8) // 2
    for name, x, y in (("butImport", 8, 23), ("butExport", 8 + bw + 8, 23), ("butProcTools", 8, 23 + 24 + 4), ("butNew", 8 + bw + 8, 23 + 24 + 4)):
        h.set(name, x, y, bw, 24)
    ph = 23 + 24 + 4 + 24 + 8
    h.set(p, 8, B - ph, LEFT_W, ph)
    g = "groupBox1"
    gh = B - ph - 8 - 8
    h.set(g, 8, 8, LEFT_W, gh)
    fx = LEFT_W - 8 - h.n("textCode").nw
    y = 23
    for label, field in (("label4", "textAbbreviation"), ("label2", "textDescription"), ("label3", "textCode"), ("label6", "comboSort")):
        h.set(field, fx, y, None, h.H)
        s.label_before(h, label, field)
        y += h.H + 4
    h.set("butAll", fx, y, LEFT_W - 8 - fx, h.H)
    s.label_before(h, "label1", "butAll")
    y += h.H + 4
    last = gh - 8 - h.H
    h.set("butShowHiddenDefault", LEFT_W - 8 - h.n("butShowHiddenDefault").nw, last, None, h.H)
    c = h.n("checkShowHidden")
    h.set("checkShowHidden", 8, last + (h.H - c.nh) // 2)
    ec = last - 4 - 24
    h.set("butEditCategories", 8, ec, None, 24)
    h.set("listCategories", 8, y, LEFT_W - 16, ec - 4 - y)
    # the grid
    gx = 8 + LEFT_W + 8
    cx = R - COL_W
    h.set("gridMain", gx, 8, cx - 8 - gx, B - 8)
    # the right column
    h.text_align("label5", "BottomLeft")
    h.set("label5", cx, 8, COL_W, CAP)
    y = 8 + CAP + 2
    for grp, i in (("groupBox2", 1), ("groupBox3", 2), ("groupBox4", 3)):
        h.set(grp, cx, None)
        y += fee_group(h, grp, i, y) + 8
    yb = B - 24
    ok = h.n("butOK")
    h.set("butOK", R - ok.nw, yb, None, 24)
    lw = h.lw("labelFeesAsOf")
    h.text_align("labelFeesAsOf", "MiddleRight")
    h.set("labelFeesAsOf", cx, yb, lw, h.H)
    h.set("textFeesAsOf", cx + lw + 6, yb, None, h.H)
    yr = yb - 4 - 24
    h.set("butEditFeeSched", cx, yr, None, 24)
    h.set("butTools", cx + h.n("butEditFeeSched").nw + 8, yr, None, 24)
    # Fee Colors: two rows of a swatch and its name, two columns (the second after the first's longer name)
    sww = h.n("butColorDefault").nw
    lw1 = max(h.lw("label19"), h.lw("label22"))
    x2 = 8 + sww + 6 + lw1 + 8
    lw2 = COL_W - 8 - x2 - sww - 6
    for sw, lab, col, row in (("butColorDefault", "label19", 0, 0), ("butColorClinic", "label21", 1, 0),
                              ("butColorProvider", "label22", 0, 1), ("butColorClinicProv", "label20", 1, 1)):
        x = x2 if col else 8
        yy = 23 + row * (h.H + 4)
        h.set(sw, x, yy, None, h.H)
        h.text_align(lab, "MiddleLeft")
        h.set(lab, x + sww + 6, yy, lw2 if col else lw1, h.H)
    ch = 23 + 2 * h.H + 4 + 8
    h.set("groupBox5", cx, yr - 8 - ch, COL_W, ch)
