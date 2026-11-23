"""FormRpLaserLabels (Laser Labels) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The Patient Labels
page holds a second "End Name:" label that main keeps outside the page (x -95: never seen, the page shows "End Name (L, F):" beside the box);
the planner brought it into the page beside the visible label. The script keeps it where main has it.
Continuation (the audit's pass 4): the planner left two field columns 43 px apart on two pages (more than the 40 px within which it shares
a column): on Patient Labels the Start Name box at 159 and the End Name box and the check boxes at 202, on Insurance Company Labels the Start
Name box and the radio buttons at 159 and the Label Count (Single Company) and End Name (Range, hidden in the designer; the code switches the
two by Visible) boxes at 116 with their labels top-left. The script puts each page's fields on one column, its labels right-aligned 6 px
before it, the picker buttons 6 px after their boxes."""


def layout(h):
    H = h.H
    h.set("labEndName", -95, 53, 61, 20)
    # The window's left content edge at 8 (the planner kept main's x).
    h.frame_left()
    # ---- Patient Labels: one field column (the End Name box's and the check boxes') ----------------------------------------------------
    end = h.n("textEndName")
    F = end.x
    start = h.n("textStartName")
    h.set("textStartName", F, None, start.w, None)
    b = h.n("butStartName")
    h.set("butStartName", F + start.w + 6, None)
    lb = h.n("labStartName")
    h.set("labStartName", F - 6 - lb.w, None)
    # ---- Insurance Company Labels: one field column (the Start Name box's and the radio buttons') -------------------------------------
    F2 = h.n("textInsCoStart").x
    row = h.n("numericInsCoSingle").y
    h.set("numericInsCoSingle", F2, row)
    te = h.n("textInsCoEnd")
    h.set("textInsCoEnd", F2, row, te.w, H)
    be = h.n("butInsCoEnd")
    h.set("butInsCoEnd", F2 + te.w + 6, row, be.w, H)
    for name in ("labInsCoSingle", "labInsCoEnd"):
        k = h.n(name)
        w = h.lw(name)
        h.set(name, F2 - 6 - w, row, w, H)
        h.text_align(name, "MiddleRight")
