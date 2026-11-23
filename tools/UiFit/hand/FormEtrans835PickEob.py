"""FormEtrans835PickEob on the grid (UI-12 slice 7, layout script on the planner's tree). The planner kept the note 468 px wide (for a 193
px text) and grew the window from 332 to 488 px for it, leaving 171 px of empty window beside the grid: the note as wide as its text, the
window back at main's 332 px, the grid 12 px from both sides (its margins)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = 332
    h.set(".", None, None, W, None)
    h.set("label1", None, None, h.lw("label1"), None)
    s.span(h, "gridEobs", r=W - 12)
