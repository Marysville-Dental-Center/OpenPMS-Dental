"""FormDatabaseMaintInvalidChars on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 12 px margins: the
content on the padding, the explanation (the text Load gives it, grid-runtime-texts) as tall as its lines."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    e.frame(h)
    R = e.right_edge(h)
    eh = 3 * 16 + 2  # the run-time text: three lines at this width (GridRulesTests sets it)
    h.set("labelExplain", None, 8, None, eh)
    dy = 8 + eh + 4 - h.n("gridColumns").ny
    if dy:
        for name in ("gridColumns", "labelSummary"):
            h.set(name, None, h.n(name).ny + dy)
        g = h.n("gridPreview")
        h.set("gridPreview", None, g.ny + dy, None, g.nh - dy)
