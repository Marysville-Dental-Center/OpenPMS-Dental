"""FormClearinghouseEdit (Edit Clearinghouse) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner
paired the Description field's note ("Also used in X12 1000B NM103") with the clinic picker as its label, right-aligned under the picker's own
Clinic label; the script puts the note 6 px after the Description field, left-aligned as on main, and the clinic picker 16 px after the note's
text; the ERA behavior list on the right column's x (the Use Claim Export Path box's) to the Location ID box's right edge; the payor note
(a borderless read-only box) as tall as its three lines."""


def layout(h):
    d = h.n("textDescription")
    wn = h.lw("label17")
    xn = d.x + d.w + 6
    h.set("label17", xn, d.y, wn, d.h)
    h.text_align("label17", "MiddleLeft")
    c = h.n("comboClinic")
    h.set("comboClinic", xn + wn + 16, d.y)
    # The right column on one x (the Use Claim Export Path box's): the ERA behavior list to the Location ID box's right edge.
    chk = h.n("checkIsClaimExportAllowed")
    loc = h.n("textLocationID")
    h.set("listBoxEraBehavior", chk.x, None, loc.x + loc.w - chk.x)
    # The payor note (a borderless read-only box) as tall as its three lines (44 px showed two); the rows hidden under it move down with it.
    t = h.n("textBox2")
    dy = 52 - t.h
    h.set("textBox2", None, None, None, 52)
    bar = h.n("butSave").y
    for k in h.tree.children:
        if k.y >= t.y + t.h and k.y + k.h + dy <= bar - 4 and k.dock in ("None", "") and k.name not in ("butSave", "butDelete"):
            h.set(k.name, None, k.y + dy)
    # Fix round 8: one right edge. The DentalXChange group (shown with ClaimConnect, beside Save DXC Transmissions) sets the window's width
    # (1022); the X12 Required Fields group to the window's content edge (main ended it on the clinic picker's and Save's edge), its
    # Hexadecimal Delimiters group to the group's content edge, the clinic picker ending on the same edge.
    R = h.tree.nw - 8
    g1, g3, c = h.n("groupBox1"), h.n("groupBox3"), h.n("comboClinic")
    h.set("groupBox1", None, None, R - g1.nx, None)
    h.set("groupBox3", None, None, (R - g1.nx) - 8 - g3.nx, None)
    h.set("comboClinic", R - c.nw, None)
    # The Sender ID group on the X12 group's 8 px padding (12 px), 8 px before the Hexadecimal Delimiters group.
    g2 = h.n("groupBox2")
    h.set("groupBox2", 8, None, g3.nx - 8 - 8, None)
    # Its fields keep the window's field column (the X12 group's): they move right by as much as the group moved left.
    dx = g2.x - 8
    for name in ("labelTIN", "textSenderTIN", "labelSenderName", "textSenderName", "labelSenderTelephone", "textSenderTelephone"):
        h.set(name, h.n(name).nx + dx, None)
