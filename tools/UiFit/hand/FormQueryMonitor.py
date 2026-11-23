"""FormQueryMonitor on the grid (UI-12 slice 6, layout script on the planner's tree). The button column started 76 px down and ended 6 px short
of the right edge, 9 px after the split panels: Start, Clear, Log, Filter and the filter group line from the top on the right edge, 4 px
apart, Copy (anchored to the bottom) 8 px over the bottom on that edge, the split panels 8 px before them. The planner also moved the split container's two panels to 8, 8 (the container places them itself) and grew the window by 22 x 14 for that: the panels back at 0, 0 and under the splitter, the window at main's 1036 x 512.
Fix round (ui/ui12-6-2): the Command caption on the Start, Stop and Elapsed captions' row (y 20, 17 px tall; it sat 5 px lower), the
Elapsed box as wide as the Start and Stop boxes (123)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    W, H = 1036, 512
    h.set(".", None, None, W, H)
    R, B = W - 8, H - 8
    bx = R - 75
    y = 8
    for name in ("butStart", "buttonClear", "butLog", "butFilter"):
        h.set(name, bx, y)
        y += 24 + 4
    h.set("labelFilterGroup", bx, y, 75, 48)
    h.set("butCopy", bx, B - 24)
    sw = bx - 8 - 8
    h.set("splitContainer", 8, 8, sw, 494)
    h.set("splitterPanel1", 0, 0, sw, 227)
    h.set("splitterPanel2", 0, 231, sw, 263)
    h.set("label1", None, 20, None, 17)
    h.set("textElapsed", None, None, h.n("textDateTimeStart").nw, None)
