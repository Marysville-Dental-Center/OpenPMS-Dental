"""FormMarkupTableEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The grid kept main's 12 px margin, the buttons of the
right column were 30, 44, 71, 73, 75 and 88 px wide in an 89 px column: the grid from the padding to 8 px before the column, Man Edit and
Paste Cells as wide as the column, every button in the groups as wide as the group's content (L and R one pair on that width)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    cx = R - 89
    h.set("gridMain", 8, None, cx - 8 - 8, None)
    for name in ("butManEdit", "butPaste"):
        h.set(name, cx, None, 89, None)
    for name in ("butRowUp", "butRowDown", "butRowInsert", "butRowDelete", "butHeaders", "butColumnInsert", "butColumnDelete"):
        h.set(name, 8, None, 73, None)
    h.set("butColumnLeft", 8, None, 30, None)
    h.set("butColumnRight", 8 + 73 - 30, None, 30, None)
