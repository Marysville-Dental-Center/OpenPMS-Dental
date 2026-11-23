"""FormClaimPayTotal on the grid (UI-12 slice 6, layout script on the planner's tree). The planner took the totals row apart (the lab fee box 5
px low and 1 px wider, drawn 10 px into the deductible box) and moved the two captions 49 px over their buttons: the totals row as main
draws it and the code keeps it (63 px boxes, each 1 px into the one before: Load moves them so, LayoutManager.MoveLocation with Right-1) 4
px under the grid, the Save note under the boxes 8 px lower, the two captions bottom-aligned 2 px over their buttons on the bottom bar (Pay
Labs, shown by the code in Canada, 6 px after Write-off)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    y = h.n("gridMain").ny + h.n("gridMain").nh + 4
    x = 405
    for name in ("textClaimFee", "textLabFees", "textDedApplied", "textInsPayAllowed", "textInsPayAmt", "textWriteOff", "textPatResp"):
        h.set(name, x, y, 63, 23)
        x += 62
    h.set("label1", 405 - 6 - 44, y)
    h.set("label2", 405, y + 23 + 8)
    yb = h.n("butSave").ny
    h.set("butDeductible", 8, yb)
    h.set("label3", 8, yb - 2 - 34, 116, 34)
    h.text_align("label3", "BottomLeft")
    xw = 8 + 116 + 8
    h.set("butWriteOff", xw, yb)
    h.set("label4", xw, yb - 2 - 34, 132, 34)
    h.text_align("label4", "BottomLeft")
    h.set("butPayLabs", xw + 90 + 6, yb)
