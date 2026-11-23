"""FormMedicationMerge on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner left each group's captions 1-4 px
off their fields (main's offsets) and the Change buttons at two x; the second group's title wider than the group (cut). Each group is one
row of captioned fields (grid_hand.Hand.caption_row: the caption 2 px over its field from the field's x, the columns 6 px apart, Change on
the content's right edge), both groups one width (their titles' widths + 16 at least), 8 px apart, Merge at the window's bottom right corner."""


def layout(h):
    W = max(h.n("groupBoxInto").nw, h.n("groupBoxFrom").nw, h.n("groupBoxInto").text_w + 16, h.n("groupBoxFrom").text_w + 16)
    y = 8
    y += h.caption_row("groupBoxInto", [("label1", "textMedNumInto"), ("label7", "textGenNumInto"), ("label2", "textMedNameInto"), ("label5", "textRxInto")], 8, y, W, button="butChangeMedInto") + 8
    y += h.caption_row("groupBoxFrom", [("label8", "textMedNumFrom"), ("label3", "textGenNumFrom"), ("label6", "textMedNameFrom"), ("label4", "textRxFrom")], 8, y, W, button="butChangeMedFrom", under=()) + 8
    # The window never shrinks (section 1): Merge 8 px from its bottom right corner.
    hr = max(h.tree.h, y + 24 + 8)
    m = h.n("butMerge")
    h.set("butMerge", 8 + W - m.nw, hr - 8 - 24, m.nw, 24)
    h.set(".", None, None, max(h.tree.w, W + 16), hr)
