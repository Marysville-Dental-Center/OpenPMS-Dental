"""FormFilePicker on the grid (UI-12 slice 7, layout script on the planner's tree). Preview and Import stood centred under the thumbnail at
75 px and Select Local File at the thumbnail's x at 109 px, the path row and the grid 10 px from the left: the three buttons 109 px, centred
under the thumbnail; the path label and the grid on the content edge, the path box 6 px after its label."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    g = h.n("groupThumbnail")
    bx = g.nx + (g.nw - 109) // 2
    for n in ("butPreview", "butImport", "butFileChoose"):
        h.set(n, bx, None, 109, None)
    lw = h.lw("label1")
    h.set("label1", 8, None, lw, None)
    s.span(h, "textPath", x=8 + lw + 6)
    s.span(h, "gridMain", x=8)
