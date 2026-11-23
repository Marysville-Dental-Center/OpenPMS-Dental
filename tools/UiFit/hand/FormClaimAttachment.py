"""FormClaimAttachment on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner put the "(sends attachment)"
note under the validation box, away from the Save button it describes, left three right edges (the box 575, the side buttons 578, Save 597)
and the narrative box 2 px left of the grid: one right edge for the validation box, the side buttons and Save (the right content edge),
the side buttons as wide as the Paste Image caption under them (its text was wider than the button), the grid from the narrative box's x to
6 px before the side buttons; the 2000 counter ending on the narrative box's right edge and the Claim Status caption on its box's x; the
note 6 px before Save on the Save row."""


def layout(h):
    root = h.tree
    R = root.w - 8
    cap = h.n("labelPasteImage")
    bw = cap.nw
    xb = R - bw
    for name in ("butAddImage", "butSnipTool", "butPasteImage"):
        h.set(name, xb, None, bw, None)
    h.set("labelPasteImage", xb, None, None, None)
    nar = h.n("textNarrative")
    g = h.n("gridAttachedImages")
    h.set("gridAttachedImages", nar.nx, None, xb - 6 - nar.nx, None)
    st = h.n("textClaimStatus")
    h.set("textClaimStatus", None, None, R - st.nx, None)
    h.set("label1", st.nx, None, None, None)
    cc = h.n("labelCharCount")
    h.set("labelCharCount", nar.nx + nar.nw - cc.nw, None, None, None)
    sv = h.n("butSave")
    h.set("butSave", R - sv.nw, None, None, None)
    ls = h.n("labelSave")
    h.set("labelSave", R - sv.nw - 6 - ls.nw, sv.ny, None, sv.nh)
    h.text_align("labelSave", "MiddleRight")
    w = h.n("labelClaimAttachWarning")
    h.set("labelClaimAttachWarning", None, nar.ny + nar.nh + 6, None, None)
