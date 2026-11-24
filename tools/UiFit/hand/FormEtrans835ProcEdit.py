"""FormEtrans835ProcEdit on the grid (UI-12 slice 13, layout script on the planner's tree; the harness builds it with an empty ERA procedure,
Scan/SampleData.cs). The planner left the left label column 48 px in from the padding, the right column's boxes 90 and 88 wide and the
three grids 8, 4 and 12 px apart. The left labels from the padding, the right column 16 px after the left boxes with one
label and one box column (Date Service as wide as the others: the code widens it to 160 for a date range (LayoutManager.MoveWidth), which the window's room keeps),
the grids 8 px apart with their heights, the section gap over Print."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    left = (("label9", "textProcAdjudicated"), ("label8", "textProcSubmitted"))
    fx = s.label_column(h, left, 8, 8)
    right_x = fx + h.n("textProcAdjudicated").nw + 16
    right = (("label2", "textDateService"), ("label1", "textInsPaid"), ("label5", "textProcNum"))
    s.label_column(h, right, right_x, 8, fw=98)
    y = 8 + 2 * 27 + h.H + 8
    h.set("groupBalancing", 8, y)
    y += h.n("groupBalancing").nh + 8
    # Fix round (review of slice 13): each Balancing caption centred over its box (they stood 22 to 97 px left of their boxes, drifting
    # further with each column): as wide as its text or the box, whichever is wider, as FormEtrans835Edit's captions over its boxes.
    for cap, box in (("labelEquation1", "textProcFee"), ("labelEquation2", "textPatRespSum"), ("labelEquation3", "textContractualObligSum"),
                     ("labelEquation4", "textPayorReductionSum"), ("labelEquation5", "textOtherAdjustmentSum"),
                     ("labelEquation6", "textInsPaidCalc")):
        b = h.n(box)
        cw = max(h.lw(cap), b.nw)
        h.set(cap, b.nx - (cw - b.nw) // 2, None, cw, None)
    # gridSupplementalInfo as wide as the other two grids (it was 2 px short of their right edge; the code sizes its columns from its width).
    h.set("gridSupplementalInfo", None, None, h.n("gridRemarks").nw, None)
    bottom = s.stack(h, ("gridProcedureAdjustments", "gridRemarks", "gridSupplementalInfo"), y, x=8)
    bar_top = bottom + s.SECTION
    h.set(".", None, None, None, bar_top + h.n("butPrint").nh + 8)
    s.bar(h, left=("butPrint",))
