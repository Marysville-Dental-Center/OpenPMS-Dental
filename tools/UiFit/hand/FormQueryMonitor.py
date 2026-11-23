"""FormQueryMonitor on the grid (UI-12 slice 6, layout script on the planner's tree). The button column started 76 px down and ended 6 px short
of the right edge, 9 px after the split panels: Start, Clear, Log, Filter and the filter group line from the top on the right edge, 4 px
apart, Copy (anchored to the bottom) 8 px over the bottom on that edge, the split panels 8 px before them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    bx = R - 75
    y = 8
    for name in ("butStart", "buttonClear", "butLog", "butFilter"):
        h.set(name, bx, y)
        y += 24 + 4
    h.set("labelFilterGroup", bx, y, 75, 48)
    h.set("butCopy", bx, e.bottom_edge(h) - 24)
    h.set("splitContainer", None, None, bx - 8 - 8, None)
