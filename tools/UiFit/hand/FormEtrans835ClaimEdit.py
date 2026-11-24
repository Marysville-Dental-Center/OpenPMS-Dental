"""FormEtrans835ClaimEdit on the grid (UI-12 slice 13, layout script on the planner's tree; the harness builds it with an empty ERA claim,
Scan/SampleData.cs). The planner left the left label column 36 px in from the padding, the right column's boxes at three left edges (626,
626, 636) and the four grids 8, 4, 4 and 12 px apart. The left labels from the padding, the right column 16 px after the left boxes with one
label and one box column (Date Service as wide as the others: the code widens it to 160 for a date range, which the window's room keeps),
the grids 8 px apart with their heights, the section gap over Print."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    left = (("label12", "textSubscriberName"), ("label8", "textPatientName"), ("label1", "textClaimIdentifier"),
            ("label2", "textPayorControlNum"), ("label4", "textStatus"))
    fx = s.label_column(h, left, 8, 8)
    right_x = fx + h.n("textSubscriberName").nw + 16
    right = (("label9", "textDateService"), ("label5", "textClaimFee"), ("label6", "textInsPaid"), ("label7", "textPatResp"),
             ("label11", "textDatePayerReceived"))
    s.label_column(h, right, right_x, 8, fw=98)
    y = 8 + 4 * 27 + h.H + 8
    h.set("groupBalancing", 8, y)
    y += h.n("groupBalancing").nh + 8
    bottom = s.stack(h, ("gridClaimAdjustments", "gridProcedureBreakdown", "gridAdjudicationInfo", "gridSupplementalInfo"), y, x=8)
    bar_top = bottom + s.SECTION
    hh = bar_top + h.n("butPrint").nh + 8
    h.set(".", None, None, None, hh)
    s.bar(h, left=("butPrint",))
