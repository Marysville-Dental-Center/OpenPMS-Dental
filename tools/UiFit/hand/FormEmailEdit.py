"""FormEmailEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 1230 x 696 by
14 x 13 for its padding around the two boxes, whose captions stood under them (main's place) with the Raw HTML box 16 px under them:
main's size; the Plain Text/HTML and Preview captions over their boxes 8 px under the toolbars; the HTML box and the preview one width,
8 px apart, from the padding to the right padding and down to 8 px over Save; the Raw HTML box on the padding on Save's row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W, MAIN_H = 1230, 696


def layout(h):
    s.size(h, MAIN_W, MAIN_H)
    yb = s.bar(h, right=("butSave",))
    tb = h.n("toolBarBottom")
    top = tb.ny + tb.nh + 8
    cap = h.n("labelPlainText").nh
    yb0 = top + cap + 2
    w = (MAIN_W - 16 - 8) // 2
    xr = 8 + w + 8
    h.set("labelPlainText", 8, top)
    h.set("labelPreview", xr, top)
    h.set("textContentEmail", 8, yb0, w, yb - 8 - yb0)
    h.set("webViewEmail", xr, yb0, MAIN_W - 8 - xr, yb - 8 - yb0)
    h.check_left("checkIsRaw")
    ck = h.n("checkIsRaw")
    sv = h.n("butSave")
    h.set("checkIsRaw", 8, yb + (sv.nh - ck.nh) // 2)
