"""FormSupplyProcCodes on the grid (UI-12 slice 6, layout script on the planner's tree). The note and the codes grid kept main's 12 px margin,
the grids 6 px apart, 4 px short of the right edge and 12 px over the bottom row: from the padding to the right edge, the grids 8 px apart
down to 8 px over the bottom row, Add Supply and Remove from the materials grid's x."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("labelInstructions", 8, None, R - 8, None)
    g = h.n("gridCodes")
    yb = h.n("butClose").ny
    gh = yb - 8 - g.ny
    h.set("gridCodes", 8, None, 334, gh)
    xm = 8 + 334 + 8
    h.set("gridMaterials", xm, g.ny, R - xm, gh)
    h.set("butAddSupply", xm, None)
    h.set("butRemoveSupply", xm + 90 + 6, None)
