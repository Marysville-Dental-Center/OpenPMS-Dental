"""FormElectIDEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The Attributes grid ended 39 px over Save and 1 px short
of it, the Payer Service box 8 px short: the grid down to 8 px over Save and, with the Payer Service row, on Save's right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    s = h.n("butSave")
    g = h.n("gridAttributes")
    h.set("gridAttributes", None, None, R - g.nx, s.ny - 8 - g.ny)
    t = h.n("textPayerService")
    h.set("textPayerService", R - t.nw, None)
    h.set("labelPayerService", R - t.nw - 6 - h.n("labelPayerService").nw, None)
