"""FormTerminalConnection on the grid (UI-12 slice 9, layout script on the planner's tree): the captions and boxes stood at 25 and ended 160
px short of the window's edge (a window with no buttons): from the padding to the right content edge, each caption 2 px over its box, 8 px
between the pairs, the Progress box to the bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, HT = h.n(".").nw, h.n(".").nh
    y = 8
    for cap, box in (("label1", "textTx"), ("label2", "textRx"), ("label3", "textProgress")):
        h.set(cap, 8, y, None, 18)
        y += 18 + 2
        hh = HT - 8 - y if box == "textProgress" else h.n(box).nh
        h.set(box, 8, y, W - 16, hh)
        y += hh + 8
