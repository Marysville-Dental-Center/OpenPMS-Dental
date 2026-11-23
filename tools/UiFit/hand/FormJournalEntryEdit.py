"""FormJournalEntryEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 320 to
342 px (the Attached to Reconcile caption and box under the 60 px Notes box) and left the Credit caption 2 px right of its box: main's
height; the Notes box as tall as Memo (46), the reconcile caption 4 px under it and its box 2 px under the caption, 8 px over the bottom
bar; the Debit and Credit captions as wide as their boxes, over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 320


def layout(h):
    s.size(h, None, MAIN_H)
    yb = s.bar(h, left=("butDelete",), right=("butSave",))
    tr = h.n("textReconcile")
    ytr = yb - 8 - tr.nh
    h.set("textReconcile", None, ytr)
    h.set("labelReconcile", None, ytr - 2 - 17, None, 17)
    tn = h.n("textNotes")
    h.set("textNotes", None, None, None, ytr - 2 - 17 - 4 - tn.ny)
    for cap, box in (("labelDebit", "textDebit"), ("labelCredit", "textCredit")):
        b = h.n(box)
        h.set(cap, b.nx, None, b.nw, None)
