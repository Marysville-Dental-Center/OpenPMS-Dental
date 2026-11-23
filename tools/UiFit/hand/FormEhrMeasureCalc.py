"""FormEhrMeasureCalc on the grid (UI-12 slice 7, layout script on the planner's tree). The notes and the grid stood 12 px from the sides
against the bottom bar's 8, the grid 23 px over the bar: on the content edges, the grid 12 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for n in ("labelInternalReview", "labelNotes", "labelProgress", "gridMain"):
        s.span(h, n, x=8, r=R)
    s.grid_bottom(h, "gridMain")
