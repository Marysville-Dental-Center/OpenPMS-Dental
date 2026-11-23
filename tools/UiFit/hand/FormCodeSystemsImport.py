"""FormCodeSystemsImport on the grid (UI-12 slice 6, layout script on the planner's tree). The grids and the note kept main's 12 px margins and
the buttons stood 27 px over the bottom: the content on the padding, the buttons 8 px over the bottom, the status grid 8 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    e.frame(h, bottom=False)
    B = e.bottom_edge(h)
    for name in ("butImportValueSets", "butImport"):
        h.set(name, None, B - 24)
    e.bottom_to(h, "gridStatus", B - 24 - 8)
