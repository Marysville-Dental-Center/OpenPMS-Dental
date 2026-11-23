"""FormMedications on the grid (UI-12 slice 7, layout script on the planner's tree). Fix round (the lead's review of 2026-11-23): the planner
left Add Generic and the grid at 14 against the 8 px right margin of the grid, Export and OK: Add Generic on the content edge, Add Brand 6
px after it, the grid from the content edge (its right edge kept)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.row_left(h, ("butAddGeneric", "butAddBrand"), 8)
    s.span(h, "gridAllMedications", x=8)
