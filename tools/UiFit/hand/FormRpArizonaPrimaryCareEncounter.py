"""FormRpArizonaPrimaryCareEncounter on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner kept the left column at main's x
(12 and 9 in the two Arizona reports) and the buttons at 75 and 83 px (Copy Log Text's text, 82 px, filled its button). The left column
at the content edge to 8 px before the button column; the four buttons one width (92 px) at the right content edge, Browse on the folder
box's row and the others under it 4 px apart; the log box down to the content's bottom."""


def layout(h):
    root = h.tree
    R = root.w - 8
    BW = 92
    BX = R - BW
    LW = BX - 8 - 8
    for name in ("label1", "label3", "label2"):
        h.set(name, 8, None)
    f = h.n("textEncounterFolder")
    h.set("textEncounterFolder", 8, None, LW, None)
    h.set("textEncounterFile", 8, None)
    h.set("groupBox1", 8, None, LW, None)
    t = h.n("textLog")
    h.set("textLog", 8, None, LW, root.h - 8 - t.ny)
    h.set("butBrowse", BX, f.ny, BW, 24)
    # Slice 5 (the anchor check): Copy Log Text, Run and Finished are anchored to the bottom (Bottom|Right; main drew them low beside the
    # log, at 325, 371 and 422); under Browse at the top they left their place on every resize (onto Browse when the window got shorter). At the bottom of the
    # button column, 4 px apart in main's order, Finished ending on the log box's bottom edge (Top|Bottom: the same edge).
    y = root.h - 8 - 24
    for name in ("butFinished", "butRun", "butCopy"):
        h.set(name, BX, y, BW, 24)
        y -= 24 + 4
