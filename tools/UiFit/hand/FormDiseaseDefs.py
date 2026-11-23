"""FormDiseaseDefs on the grid (UI-12 slice 7, layout script on the planner's tree). The Search group and the grid stood 18 px from the left
and 26 px before the button column, the four search boxes 20 px tall: the group and the grid from the content edge to 8 px before the button
column's note (wider than the buttons), the boxes at the field height on the 27 px pitch, Description to the group's content edge. Fix round
(the lead's review of 2026-11-23): the buttons on OK's column (x and width; they stood 20 px left of it), the Alphabetize note right-aligned
on that edge (it is wider than the buttons: 'alphabetically' alone needs 80 px)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    for n in ("butAdd", "butMerge", "butAlphabetize", "butUp", "butDown"):
        h.set(n, ok.nx, None, ok.nw, None)
    note = h.n("labelAlphabetize")
    s.move(h, "labelAlphabetize", x=ok.nx + ok.nw - note.nw)
    h.text_align("labelAlphabetize", "MiddleRight")
    r = note.nx - 8
    s.span(h, "groupBox1", x=8, r=r)
    s.span(h, "gridMain", x=8, r=r)
    for n in ("textICD9", "textICD10", "textSnoMed", "textDescript"):
        h.set(n, None, None, None, h.H)
    s.span(h, "textDescript", r=(r - 8) - 8)
