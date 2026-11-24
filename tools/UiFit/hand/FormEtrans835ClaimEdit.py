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
    # Fix round (review of slice 13): the Balancing caption is one space-padded string (labelEquation, kept): each box's left edge under its
    # caption word and each sign under the caption's sign. Where TextRenderer draws them at Segoe UI 9, measured in the after picture (px
    # from the label's left edge): the words at 3 (Claim Fee), 140, 286 and 430, the signs' centres at 124, 270 and 412.5. A box's text
    # starts 3 px inside it, as Claim Fee's box under its word at the label's edge; the signs' glyphs centre 11 px (16 px "-") and 13.5 px
    # (20 px "=") into their right-aligned labels.
    L = h.n("labelEquation").nx
    for box, word in (("textClaimFee2", 3), ("textClaimAdjAmtSum", 140), ("textProcAdjAmtSum", 286), ("textInsPaidCalc", 430)):
        h.set(box, L + word - 3, None)
    for sign, centre, glyph in (("label18", 124, 11), ("label3", 270, 11), ("label10", 412.5, 13.5)):
        h.set(sign, int(L + centre - glyph), None)
    # gridSupplementalInfo as wide as the other three grids (it was 2 px short of their right edge; the code sizes its columns from its width).
    h.set("gridSupplementalInfo", None, None, h.n("gridClaimAdjustments").nw, None)
    bottom = s.stack(h, ("gridClaimAdjustments", "gridProcedureBreakdown", "gridAdjudicationInfo", "gridSupplementalInfo"), y, x=8)
    bar_top = bottom + s.SECTION
    hh = bar_top + h.n("butPrint").nh + 8
    h.set(".", None, None, None, hh)
    s.bar(h, left=("butPrint",))
