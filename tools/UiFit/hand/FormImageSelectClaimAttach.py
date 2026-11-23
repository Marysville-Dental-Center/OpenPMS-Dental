"""FormImageSelectClaimAttach on the grid (UI-12 slice 7, layout script on the planner's tree). Fix round (the lead's review of 2026-11-23):
the planner left Import, Snipping Tool and Paste Image 14 px before OK's right edge and the grid at 12: the three buttons and OK one column
ending on OK's edge, as wide as the buttons (85 px: 'Snipping Tool' needs it), the clipboard note right-aligned on that edge (it is wider
than the buttons), the grid from the content edge to 8 px before the note."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    E = ok.nx + ok.nw
    w = h.n("butImport").nw
    for n in ("butImport", "butSnipTool", "butPasteImage", "butOK"):
        s.move(h, n, x=E - w)
        h.set(n, None, None, w, None)
    note = h.n("labelPasteImage")
    s.move(h, "labelPasteImage", x=E - note.nw)
    h.text_align("labelPasteImage", "TopRight")
    s.span(h, "gridMain", x=8, r=note.nx - 8)
