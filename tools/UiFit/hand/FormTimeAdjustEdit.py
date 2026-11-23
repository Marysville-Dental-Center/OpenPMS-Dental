"""FormTimeAdjustEdit on the grid (UI-12 slice 4, layout script on the planner's tree). Main drew the two radio buttons box-right in the
label column; the planner turned them box-left but left them 17 px apart in that column (drawn into each other) and found no fixed point:
the radio buttons as a list on the field column (the list pitch), the deletion note 6 px after Manually entered, then the field rows on the
27 px pitch (the two check boxes a list), the Note label on the box's first line; the bottom bar as the planner placed it."""


def layout(h):
    b = h.box(".")
    F = h.n("textTimeEntry").nx
    b.checks(["radioAuto", "radioManual"], F, after={"radioManual": [("label5", None, h.lw("label5"), h.CH)]})
    b.field("label1", "textTimeEntry", F, 156)
    b.field("label2", "textHours", F, 68)
    b.row(("checkOvertime", F, None), ("label3", None, h.lw("label3"), h.CH), h=h.CH)
    b.row(("checkUnpaidProtectedLeave", F, None))
    b.field("labelPTO", "comboPTO", F, 120)
    b.field("labelUser", "textUser", F, 120)
    y = b.y
    b.field("label4", "textNote", F, 350)
    h.set("label4", None, y)
