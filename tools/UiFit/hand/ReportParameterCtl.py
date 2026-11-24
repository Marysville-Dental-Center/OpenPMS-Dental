"""The report designer's ReportParameterCtl on the grid (UI-12 slice 13, layout script on main's layout in the planner's tree; its designer
code is in ReportParameterCtl.cs, read and edited in InitializeComponent's body). PropertyDialog docks it in a 450 x 300 tab page. The
planner reached no fixed point (its last pass spread the groups' rows over 2936 px). Three rows at the top: the parameter list over two
rows with its Up and Down buttons beside them and Add and Remove under it; Name and Datatype, Prompt with Multi Value after it, Allow null
and Allow blank, on one label column 16 px after the arrows; Default Values and Valid Values over the width, each radio button row with
its box (Values with "..." on the content's right edge, Data Set Name with its box and Value Field), Valid Values' Display Field under
Value Field. Remove as wide as its text (main cut it). Everything fits the page's 450 x 300 (the control 456 x 298, main
456 x 296; the planner made it 2936 wide)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def lab(h, name, x, y, w):
    h.text_align(name, "MiddleRight")
    h.set(name, x, y, w, h.H)


def chk(h, name, x, y):
    h.check_left(name)
    w = 16 + h.n(name).text_w + 4
    h.set(name, x, y + (h.H - h.CH) // 2, w, h.CH)
    return x + w


def layout(h):
    R = 448
    h.set(".", None, None, 456, 298)
    h.set("lbParameters", 8, 8, 120, 27 + h.H)
    h.set("bParmUp", 134, 8, 20, h.H)
    h.set("bParmDown", 134, 35, 20, h.H)
    h.set("bAdd", 8, 62, 54, h.H)
    h.set("bRemove", 66, 62, 62, h.H)
    lw = max(h.lw(n) for n in ("lParmName", "lParmPrompt"))
    lx = 134 + 20 + 16
    fx = lx + lw + 6
    lab(h, "lParmName", lx, 8, lw)
    h.set("tbParmName", fx, 8, 80, h.H)
    tw = h.lw("lParmType")
    lab(h, "lParmType", fx + 80 + 16, 8, tw)
    h.set("cbParmType", fx + 80 + 16 + tw + 6, 8, R - (fx + 80 + 16 + tw + 6), h.H)
    lab(h, "lParmPrompt", lx, 35, lw)
    mw = 16 + h.n("ckbParmMultiValue").text_w + 4
    h.set("tbParmPrompt", fx, 35, R - mw - 16 - fx, h.H)
    chk(h, "ckbParmMultiValue", R - mw, 35)
    x = chk(h, "ckbParmAllowNull", fx, 62)
    chk(h, "ckbParmAllowBlank", x + 16, 62)
    # the two groups
    rw = max(16 + h.n(n).text_w + 4 for n in ("rbDefaultValues", "rbDefaultDataSetName", "rbValues", "rbDataSet"))
    bx = 8 + rw + 6
    gw = R - 8
    vw = max(h.lw(n) for n in ("label4", "label1", "label2"))
    y = 62 + h.H + 8
    for g, rv, tb, bdots, rd, cbd, lv, cbv, ld, cbdisp in (
            ("groupBox2", "rbDefaultValues", "tbParmDefaultValue", "bDefaultValues", "rbDefaultDataSetName", "cbDefaultDataSets", "label4",
             "cbDefaultValueField", None, None),
            ("groupBox1", "rbValues", "tbParmValidValues", "bValidValues", "rbDataSet", "cbValidDataSets", "label1", "cbValidFields", "label2",
             "cbValidDisplayField")):
        for r, yy in ((rv, 23), (rd, 50)):
            h.check_left(r)
            h.set(r, 8, yy + (h.H - h.CH) // 2, rw, h.CH)
        h.set(bdots, gw - 8 - 24, 23, 24, h.H)
        h.set(tb, bx, 23, gw - 8 - 24 - 6 - bx, h.H)
        h.set(cbd, bx, 50, 112, h.H)
        vx = bx + 112 + 16
        lab(h, lv, vx, 50, vw)
        h.set(cbv, vx + vw + 6, 50, gw - 8 - (vx + vw + 6), h.H)
        rows = 2
        if ld:
            lab(h, ld, vx, 77, vw)
            h.set(cbdisp, vx + vw + 6, 77, gw - 8 - (vx + vw + 6), h.H)
            rows = 3
        gh = 23 + (rows - 1) * 27 + h.H + 8
        h.set(g, 8, y, gw, gh)
        y += gh + 8
