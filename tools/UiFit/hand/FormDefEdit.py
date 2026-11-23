"""FormDefEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 423 to 462 px
for the color column, kept the content at x 12, the Value caption 5 px higher than the Name caption, and the value caption one line
tall (the code gives it texts up to 315 px: "Not allowed to change type after an adjustment is created."): main's width; Hidden and the
No Color box (hidden in the designer) on the first row; Name and Value 170 wide from the padding 8 px apart, the color column 8 px after
them (the color swatch, under it "..." and the clear button 6 px apart), all their captions 2 px over them, bottom-aligned (the Value
caption two lines tall); the Default % row (hidden in the designer) under Value."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W = 423


def layout(h):
    s.size(h, MAIN_W, None)
    h.check_left("checkHidden")
    h.set("checkHidden", 8, 8)
    h.check_left("checkNoColor")
    h.set("checkNoColor", 8 + h.n("checkHidden").nw + 8, 8)
    cap_v = 2 * 16 + 2
    yc = 8 + 20 + 4
    yb = yc + cap_v + 2
    w = 170
    xv = 8 + w + 8
    xc = xv + w + 8
    h.set("textName", 8, yb, w, None)
    h.set("textValue", xv, yb, w, None)
    ln = h.n("labelName")
    h.set("labelName", 8, yb - 2 - ln.nh, w, None)
    h.set("labelValue", xv, yc, w, cap_v)
    lc = h.n("labelColor")
    h.set("labelColor", xc, yb - 2 - lc.nh, MAIN_W - 8 - xc, None)
    h.set("butColor", xc, yb)
    ys = yb + h.n("butColor").nh + 4
    h.set("butSelect", xc, ys)
    h.set("butClearValue", xc + h.n("butSelect").nw + 6, ys)
    tv = h.n("textValue")
    yp = tv.ny + tv.nh + 4
    lp = h.n("labelDefaultPercent")
    h.set("labelDefaultPercent", xv, yp, None, None)
    h.set("textDefaultPercent", xv, yp + lp.nh + 2)
    s.bar(h, left=("butDelete",), right=("butSave",))
