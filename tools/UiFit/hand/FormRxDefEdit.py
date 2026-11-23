"""FormRxDefEdit on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). One touch on the planner's layout: Delete at the
bottom bar's left content edge (8 px from the window's left, as Save is 8 px from its right; the planner kept main's x).
Continuation (the audit's pass 4): the Sig box 4 px short of the Drug box's right edge, and the RxNorm picker button 4 px past the right
edge of the Notes and Patient Instructions boxes: Sig as wide as Drug, the RxNorm box narrower so its button ends on that edge."""


def layout(h):
    h.set("butDelete", 8, None)
    drug = h.n("textDrug")
    h.set("textSig", None, None, drug.w, None)
    notes = h.n("textNotes")
    R = notes.x + notes.w
    b = h.n("butRxNormSelect")
    cui = h.n("textRxCui")
    h.set("butRxNormSelect", R - b.w, None)
    h.set("textRxCui", None, None, R - b.w - 6 - cui.x, None)
