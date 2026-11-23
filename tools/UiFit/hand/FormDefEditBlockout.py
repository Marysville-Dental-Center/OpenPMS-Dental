"""FormDefEditBlockout on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 493 to
501 px and kept the content at x 9, the Usage group 2 px under the Name box's top and the Color column 18 px under the Name caption: main's
width; Hidden, the Name caption and the Name box from the padding; the Usage group 8 px after the box on its top, the box as tall as the
group; the Color caption and swatch 8 px after the group on the Name caption's and the box's rows."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W = 493


def layout(h):
    s.size(h, MAIN_W, None)
    h.check_left("checkHidden")
    h.set("checkHidden", 8, 8)
    ln = h.n("labelName")
    yc = 8 + 20 + 4
    h.set("labelName", 8, yc)
    yb = yc + ln.nh + 2
    g = h.n("groupBoxUsage")
    tn = h.n("textName")
    h.set("textName", 8, yb, None, g.nh)
    xg = 8 + tn.nw + 8
    h.set("groupBoxUsage", xg, yb)
    xc = xg + g.nw + 8
    h.set("labelColor", xc, yc)
    h.set("butColor", xc, yb)
    s.bar(h, right=("butSave",))
