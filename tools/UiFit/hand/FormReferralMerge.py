"""FormReferralMerge on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner left each group's captions 1-4 px
off their fields (main's offsets) and the Change buttons at two x; the second group's title wider than the group (cut). Each group is one
row of captioned fields (grid_hand.Hand.caption_row: the caption 2 px over its field from the field's x, the columns 6 px apart, Change on
the content's right edge), both groups one width (their titles' widths + 16 at least), 8 px apart, Merge at the window's bottom right corner."""


def layout(h):
    W = max(h.n("groupBox1").nw, h.n("groupBox2").nw, h.n("groupBox1").text_w + 16, h.n("groupBox2").text_w + 16)
    y = 8
    y += h.caption_row("groupBox1", [("label2", "textReferralNameInto"), ("label5", "textTitleInto"), ("label1", "checkIsPersonInto"), ("label7", "checkIsDoctorInto")], 8, y, W, button="butChangeReferralInto") + 8
    y += h.caption_row("groupBox2", [("label8", "textReferralNameFrom"), ("label6", "textTitleFrom"), ("label4", "checkIsPersonFrom"), ("label3", "checkIsDoctorFrom")], 8, y, W, button="butChangeReferralFrom", under=()) + 8
    # The window never shrinks (section 1): Merge 8 px from its bottom right corner.
    hr = max(h.tree.h, y + 24 + 8)
    m = h.n("butMerge")
    h.set("butMerge", 8 + W - m.nw, hr - 8 - 24, m.nw, 24)
    h.set(".", None, None, max(h.tree.w, W + 16), hr)
